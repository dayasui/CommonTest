using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ParallelCommon;

namespace ParallelPhoton {
    public class PhotonManager : SingletonMonoBehaviourPunCallbacks<PhotonManager> {
        public enum Status {
            Disconnect,
            Connecting,
            InLobby,
            MatchingOwner, // オーナーマッチング中
            MatchingOther, // オーナー以外マッチング中
            MatchingCanceling, // マッチングキャンセル中
            MatchingCancelingForRetry, // リトライのためにマッチングキャンセル中（リトライは実装見送り予定なので使わない可能性大）
            MatchingCanceled, // マッチングキャンセル（タイムアウト含む）
            Matched, // マッチング完了
        }
        private Status _status = Status.Disconnect;
        public bool IsMatched {
            get {
                return _status == Status.Matched;
            }
        }

        public bool IsInLobby => _status == Status.InLobby;
        public bool IsConnectedAndReady => PhotonNetwork.IsConnectedAndReady;

        private ParallelCommon.Constants.MatchingMode _matchingMode = ParallelCommon.Constants.MatchingMode.None;

        private readonly int MaxPlayers = 16;
        private readonly TypedLobby _sqlLobby = new TypedLobby("customSqlLobby", LobbyType.SqlLobby);

        public UnityAction<Player> OnPlayerEnteredRoomListener { set; get; } = null;
        public UnityAction<Player> OnPlayerLeftRoomListener { set; get; } = null;
        public UnityAction OnJoinedLobbyListener { set; get; } = null;
        public UnityAction OnJoinedRoomListener { set; get; } = null;
        public UnityAction<int, string> OnCreateRoomFailedListener { set; get; } = null;
        public UnityAction OnLeftRoomListener { set; get; } = null;
        public UnityAction OnMatchingListener { set; get; } = null;
        public UnityAction<DisconnectCause> OnDisconnectedListener { set; get; } = null;
        public UnityAction<Player> OnMasterClientSwitchedListener { set; get; } = null;
        public UnityAction OnMatchingTimeoutListener { set; get; } = null;
        public UnityAction OnMatchingCancelListener { set; get; } = null;

        public float MatchingTimeoutTime { set; get; } = 120; // マッチングでタイムアウトするまでの時間[s]
                                                              // public float MatchingResultFilterTime { set; get; } = 30; // 勝ち負け結果をマッチングフィルターに反映させる時間[s] これを過ぎたらフィルター無しになる

        private bool _joinRoomFailed = false;
        private int _numberOfPlayers = 0; // 全体の人数
        public int NumberOfPlayers => _numberOfPlayers;
        private int _numberOfPlayersInTeam = 0; // 1チームの人数
        private int _numberOfTeams = 0;
        public int NumberOfTeams => _numberOfTeams;
        private string[] _expectedUsers = null;
        private byte _maxPlayers;
        private Constants.MatchingGameResult _recentGameResult = Constants.MatchingGameResult.None;

        private List<RoomInfo> _roomList = null;
        private UnityAction<List<RoomInfo>> _onGetCustomRoolList;
        private bool _isBusy = false;
        private string _currentRoomName = "";
        private Coroutine _matchingMainCo = null;
        private Coroutine _matchingCo = null;
        private bool _canCancelMatching = false;

        public static bool DebugIsNoMatchingFilter { set; get; } = false; // 直前の勝ち負けをマッチング条件に入れない


        public float KeepAliveInBackground {
            set {
                float before = PhotonNetwork.KeepAliveInBackground;
                PhotonNetwork.KeepAliveInBackground = value;
                Debug.Log($"change PhotonNetwork.KeepAliveInBackground:{before} -> {value}");
            }

            get {
                return PhotonNetwork.KeepAliveInBackground;
            }
        }
        public int SerializationRate {
            set {
                int before = PhotonNetwork.SerializationRate;
                PhotonNetwork.SerializationRate = value;
                Debug.Log($"change PhotonNetwork.SerializationRate:{before} -> {value}");
            }

            get {
                return PhotonNetwork.SerializationRate;
            }
        }

        public static bool OfflineMode { set; get; } = false;
        public static string OfflineUserId {private set; get; } = "";

        public Transform gameObjectRoot {
            set; get;
        } = null;

        private void Start() {
        }

        // Update is called once per frame
        void Update() {

        }

        private void OnDestroy() {
            StopAllCoroutines();
        }

        public void Disconnect() {
            Debug.Log("called Disconnect");
            PhotonNetwork.Disconnect();
        }

        /// <summary>
        /// リスナーを消して切断する
        /// </summary>
        public void DisconnectSilent() {
            this.clearListeners();
            this.Disconnect();
        }

        private void clearListeners() {
            this.OnPlayerEnteredRoomListener = null;
            this.OnPlayerLeftRoomListener = null;
            this.OnJoinedLobbyListener = null;
            this.OnJoinedRoomListener  = null;
            this.OnCreateRoomFailedListener = null;
            this.OnLeftRoomListener = null;
            this.OnMatchingListener = null;
            this.OnDisconnectedListener = null;
            this.OnMasterClientSwitchedListener = null;
            this.OnMatchingTimeoutListener = null;
            this.OnMatchingCancelListener = null;
        }

        /// <summary>
        /// Photonにつないでロビーに入る
        /// </summary>
        /// <param name = "onConnect" ></ param >
        public void Connect() {
            Debug.Log($"called connect. current state is {PhotonNetwork.NetworkingClient.State}");

            AuthenticationValues authValues = new AuthenticationValues();
            authValues.UserId = this.generatePhotonUserID(UserDataManager.UserID);
            PhotonNetwork.AuthValues = authValues;

            if (!PhotonNetwork.IsConnectedAndReady && this._status == Status.Disconnect) {
                if (!OfflineMode) {
                    if (PhotonNetwork.ConnectUsingSettings()) {
                        this._status = Status.Connecting;
                    } else {
                        Debug.Log("ConnectUsingSettings error?");
                    }
                } else {
                    OfflineUserId = authValues.UserId;
                    StartCoroutine(this.offlineConnectCo());
                }
            } else {
                Debug.Log("Connect Error??");
            }
        }

        /// <summary>
        /// オフラインモードの接続ダミー関数
        /// </summary>
        /// <returns></returns>
        private IEnumerator offlineConnectCo() {
            this._status = Status.Connecting;
            yield return null;
            // PhotonNetwork.OfflineMode = true;でOnConnectedToMasterが即座によばれるので1フレーム待つ
            PhotonNetwork.OfflineMode = true;
        }

        /// <summary>
        /// Photonで使用するuserIDを生成する
        /// </summary>
        /// <returns></returns>
        private string generatePhotonUserID(int userID) {
            string photonUserID = $"{PhotonUtil.GetGroupRoomSessionID()}_{userID}";
            return photonUserID;
        }

        public override void OnConnected() {
            Debug.Log("OnConnected");
        }
        public override void OnConnectedToMaster() {
            if (!OfflineMode) {
                Debug.Log("OnConnectedToMaster");
                Debug.Log($"PHOTON USER ID:{PhotonNetwork.LocalPlayer.UserId}");
                PhotonNetwork.JoinLobby(this._sqlLobby);
            } else {
                Debug.Log("OnConnectedToMaster at offline mode");
                this.OnJoinedLobby();
            }
        }

        public override void OnDisconnected(DisconnectCause cause) {
            Debug.Log($"OnDisconnected:{cause}");
            this.resetParam();
            this.OnDisconnectedListener?.Invoke(cause);

            // TODO エラー時の再接続
            //   if (aaa) {
            //     PhotonNetwork.ReconnectAndRejoin()

            //}
        }

        private void resetParam() {
            this._currentRoomName = "";
        }

        public override void OnJoinedLobby() {
            Debug.Log("OnJoinedLobby");

            if (this._status == Status.Connecting) {
                this._status = Status.InLobby;
                this.OnJoinedLobbyListener?.Invoke();
            } else {
                Debug.Log("最初の接続時以外でロビーに入ったときはステータスを変えずにリスナーも呼ばない");
                Debug.Log("マッチングキャンセルで部屋から出るとここが呼ばれる模様（ロビーから出ていないのに）");
            }
        }

        public void CloseRoom() {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

        private const string MODE_PROP_KEY = "C0"; // マッチングモード
        private const string AID_PROP_KEY = "C1"; // アプリケーションID
        private const string RECENTRESULT_PROP_KEY = "C2"; // 直近の勝敗

        private Hashtable _customPropertis = new Hashtable();
        private List<string> _customPropertiesForLobby = new List<string>() {
            MODE_PROP_KEY,
            AID_PROP_KEY,
            RECENTRESULT_PROP_KEY
        };

        public void CreateRoom() {
            if (!PhotonNetwork.IsConnectedAndReady) {
                return;
            }

            string roomName = this.generateRoomName();
            RoomOptions roomOptions = new RoomOptions();
            int aid = ApplicationDataManager.Instance != null ? (int)ApplicationDataManager.Instance.ApplicationID : 1;
            
            roomOptions.CustomRoomPropertiesForLobby = this._customPropertiesForLobby.ToArray();
            roomOptions.CustomRoomProperties = new Hashtable() {
                { MODE_PROP_KEY, (int)this._matchingMode },
                { AID_PROP_KEY, aid },
                { RECENTRESULT_PROP_KEY, (int)this._recentGameResult },
            };
            roomOptions.PublishUserId = true;
            roomOptions.MaxPlayers = this._maxPlayers;
            PhotonNetwork.CreateRoom(roomName, roomOptions, this._sqlLobby, this._expectedUsers);

            Debug.Log($"called CreateRoom:{roomName}");
        }

        private string generateRoomName() {
            int uid = UserDataManager.UserID != 0 ? UserDataManager.UserID : UnityEngine.Random.Range(0, 100000000);
            int rid = UnityEngine.Random.Range(0, 100000000);
            string roomName = $"room_{uid}_{rid}";
            return roomName;
        }

        public override void OnCreatedRoom() {
            Debug.Log("OnCreatedRoom");
        }

        public override void OnJoinedRoom() {
            Debug.Log("OnJoinedRoom");
            this._currentRoomName = PhotonNetwork.CurrentRoom.Name;
            this.OnJoinedRoomListener?.Invoke();

            string key = $"userid_{PhotonNetwork.LocalPlayer.ActorNumber}";
            Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            customProperties.Add(key, PhotonNetwork.LocalPlayer.UserId);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

            string[] customPropertiesForLobby = PhotonNetwork.CurrentRoom.PropertiesListedInLobby;
            customPropertiesForLobby = customPropertiesForLobby.Concat(new string[] { key }).ToArray();
            PhotonNetwork.CurrentRoom.SetPropertiesListedInLobby(customPropertiesForLobby);

            string playerName = UserDataManager.Instance?.GetMyName();
            if (string.IsNullOrEmpty(playerName)) {
                int id = PhotonNetwork.LocalPlayer.ActorNumber;
                PhotonNetwork.LocalPlayer.NickName = ($"Test{id}");
            } else {
                PhotonNetwork.LocalPlayer.NickName = playerName;
            }

            this.updateUsersInGameData();
        }


        private void updateUsersInGameData() {
            foreach (var player in PhotonNetwork.PlayerList) {
                PhotonUserDataManager.Instance.AddUserID(player.UserId);
            }
        }

        public override void OnLeftRoom() {
            Debug.Log("OnLeftRoom");
            this._currentRoomName = "";
            this.OnLeftRoomListener?.Invoke();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer) {
            Debug.Log($"OnPlayerEnteredRoom:{newPlayer.NickName}");

            PhotonUserDataManager.Instance.AddUserID(newPlayer.UserId);
            this.OnPlayerEnteredRoomListener?.Invoke(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player leftPlayer) {
            Debug.Log($"OnPlayerLeftRoom:{leftPlayer.NickName}");

            // Only MasterClient
            // TODO 必要であれば、離脱者をチームデータから削除するコードをここに入れる
            if (PhotonNetwork.IsMasterClient) {

            }

            this.OnPlayerLeftRoomListener?.Invoke(leftPlayer);
        }

        public override void OnMasterClientSwitched(Player newMasterClient) {
            Debug.Log($"OnMasterClientSwitched:{newMasterClient.NickName}");
            this.OnMasterClientSwitchedListener?.Invoke(newMasterClient);
        }

        public override void OnCreateRoomFailed(short returnCode, string message) {
            Debug.Log($"OnCreateRoomFailed {returnCode} {message}");

            this.OnCreateRoomFailedListener?.Invoke(returnCode, message);

            if (PhotonNetwork.IsConnectedAndReady) {
                // リトライ処理
                // TODO 条件入れる
                Invoke("CreateRoom", 5.0f);
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message) {
            Debug.Log($"OnJoinRoomFailed {returnCode} {message}");
            this._joinRoomFailed = true;

            if (!PhotonNetwork.InLobby) {
                // Photonの仕様で部屋に入るのを失敗するとロビーから出るらしいので入り直す
                PhotonNetwork.JoinLobby(this._sqlLobby); 
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message) {
            Debug.Log($"OnJoinRoomFailed {returnCode} {message}");
            this._joinRoomFailed = true;

            if (!PhotonNetwork.InLobby) {
                // Photonの仕様で部屋に入るのを失敗するとロビーから出るらしいので入り直す
                PhotonNetwork.JoinLobby(this._sqlLobby);
            }
        }

        /// <summary>
        /// マッチング開始
        /// </summary>
        /// <param name="numberOfTeams">チーム数</param>
        /// <param name="numberOfPlayers">チーム内の人数</param>
        /// <param name="mode">モード</param>
        /// <param name="expectedUsers">一緒に入ることを期待しているユーザー（ルーム外対戦のときにルーム内のユーザーIDを渡す想定）</param>
        public bool StartMatching(int numberOfTeams, int numberOfPlayers, Constants.MatchingMode mode,
            string[] expectedUsers = null, Constants.MatchingGameResult recentGameResult = Constants.MatchingGameResult.None) {
            if (!this.canStartMatching()) {
                Debug.Log("マッチング開始できません");
                return false;
            }

            this._numberOfPlayers = numberOfTeams * numberOfPlayers;
            if (this._numberOfPlayers > 0) {
                this._maxPlayers = (byte)this._numberOfPlayers;
            } else {
                this._numberOfPlayers = MaxPlayers;
            }
            this._numberOfPlayersInTeam = numberOfPlayers;
            this._numberOfTeams = numberOfTeams;
            this._expectedUsers = expectedUsers;
            this._matchingMode = mode;
            this._recentGameResult = !DebugIsNoMatchingFilter ? recentGameResult : Constants.MatchingGameResult.None;

            if (PhotonNetwork.InRoom) {
                // 一旦部屋を出る
                PhotonNetwork.LeaveRoom();
            }

            this._status = Status.MatchingOwner;
            this._matchingMainCo = StartCoroutine(this.startMatchingCoroutines());

            return true;
        }

        private IEnumerator startMatchingCoroutines() {
            float wait = MatchingTimeoutTime;
            if (this._status == Status.MatchingOwner) {
                this._matchingCo = StartCoroutine(this.matchingCo());
            } else {
                this._matchingCo = StartCoroutine(this.matchingExceptOwnerCo());
            }

            Debug.Log($"startMatchingCoroutines wait:{wait}");
            yield return new WaitForSeconds(wait);

            Debug.Log($"startMatchingCoroutines timeout");

            Status preStatus = this._status;

            this._status = Status.MatchingCanceling;

            yield return this.cancelMatchingCo();

            Debug.Log($"startMatchingCoroutines canceled");

            this._matchingMainCo = null;

            this.OnMatchingTimeoutListener?.Invoke();
        }

        //private void retryMatching(bool isOwner) {
        //    if (isOwner) {
        //        this._status = Status.MatchingOwner;
        //        StartCoroutine(this.startMatchingCoroutines());
        //    } else {
        //        this._status = Status.MatchingOther;
        //        StartCoroutine(this.startMatchingCoroutines());
        //    }
        //}

        private bool canStartMatching() {
            if (this._matchingMainCo != null) {
                return false;
            }

            if (this._status == Status.InLobby || this._status == Status.MatchingCanceled || this._status == Status.Matched) {
                return true;
            } else {
                return false;
            }
        }

        private IEnumerator matchingCo() {
            Debug.Log("start matchingCo");

            this._canCancelMatching = false;

            if (!OfflineMode) {
                while (!PhotonNetwork.InLobby || PhotonNetwork.InRoom) {
                    // ロビーに居なかったら入るまで待つ
                    // 部屋に居たら部屋から出るまで待つ
                    yield return null;
                }
            }

            Debug.Log("matchingCo 1");

            string filter = this.getRoomFilter();
            Debug.Log($"room filter:{filter}");
            bool result = true;
            if (this._matchingMode == Constants.MatchingMode.Friend && UserDataManager.IsOwner) {
                // ルーム内対戦の場合、オーナーは必ず部屋を作る
                this._joinRoomFailed = true;
            } else {
                result = PhotonNetwork.JoinRandomRoom(null, this._maxPlayers, MatchmakingMode.FillRoom, null, filter, this._expectedUsers);
            }
            if (result) {
                while (!PhotonNetwork.InRoom) {
                    if (this._joinRoomFailed) {
                        if (OfflineMode || PhotonNetwork.InLobby) {
                            this.CreateRoom();
                            this._joinRoomFailed = false;
                        }
                    }
                    yield return null;
                }

                Debug.Log("matchingCo 2");

                this._canCancelMatching = true;

                if (this._numberOfPlayersInTeam > 0) {
                    while (PhotonUserDataManager.Instance.NumberOfPlayers != this._numberOfPlayers) {
                        yield return null;
                    }
                }

                Debug.Log($"MATCHING OWNER! {PhotonNetwork.IsMasterClient}");

                if (PhotonNetwork.IsMasterClient) {
                    PhotonUserDataManager.Instance.CreateTeam(this._matchingMode == Constants.MatchingMode.Friend, this._numberOfTeams);
                    PhotonCommonRPC.Instance.UpdateTeamData(PhotonUserDataManager.Instance.TeamDataAll);
                }

                while(this._numberOfTeams != PhotonUserDataManager.Instance.NumberOfTeams) {
                    yield return null;
                }
                Debug.Log($"Valid NumberOfTeams {this._numberOfTeams}");

                this.OnMatching();
            } else {
                // 基本的にここには来ない想定
                Debug.Log("ERROR");
            }
        }

        public enum FindFriendReason {
            None,
            JoinRoom,
        }
        private FindFriendReason _findFriendReason = FindFriendReason.None;

        /// <summary>
        /// マッチング開始　部屋を作らないのでパラレル使用時のオーナー以外が呼ぶ想定
        /// </summary>
        /// <param name="numberOfTeams">チーム数</param>
        /// <param name="numberOfPlayers">チーム内の人数</param>
        /// <param name="mode">モード</param>
        public bool StartMatchingExceptOwner(int numberOfTeams, int numberOfPlayers, Constants.MatchingMode mode) {
            if (!this.canStartMatching()) {
                return false;
            }
            this._numberOfPlayers = (byte)(numberOfTeams * numberOfPlayers);
            this._numberOfTeams = numberOfTeams;
            this._numberOfPlayersInTeam = numberOfPlayers;
            this._matchingMode = mode;
            if (PhotonNetwork.InRoom) {
                // 一旦部屋を出る
                PhotonNetwork.LeaveRoom();
            }
 
            this._status = Status.MatchingOther;
            this._matchingMainCo = StartCoroutine(this.startMatchingCoroutines());

            return true;
        }

        private IEnumerator matchingExceptOwnerCo() {
            Debug.Log("start matchingExceptOwnerCo");

            this._canCancelMatching = false;

            while (!PhotonNetwork.InLobby || PhotonNetwork.InRoom) {
                // ロビーに居なかったら入るまで待つ
                // 部屋に居たら部屋から出るまで待つ
                yield return null;
            }

            Debug.Log("matchingExceptOwnerCo 1");

            this._canCancelMatching = true;

            while (this._findFriendReason != FindFriendReason.None) {
                yield return null;
            }

            Debug.Log("matchingExceptOwnerCo 2");

            string ownerID = this.generatePhotonUserID(UserDataManager.OwnerUserID);
            var friendsToFind = new string[1] { ownerID };
            PhotonNetwork.FindFriends(friendsToFind);
            this._findFriendReason = FindFriendReason.JoinRoom;
            Debug.Log($"FIND FRIEND:{ownerID}");

            float time = 0.0f;
            while (!PhotonNetwork.InRoom) {
                yield return null;
                time += Time.deltaTime;
                if (time >= 5.0f) {
                    time = 0.0f;
                    PhotonNetwork.FindFriends(friendsToFind);
                    Debug.Log($"RETRY FIND FRIEND:{ownerID}");
                }
            }

            Debug.Log("matchingExceptOwnerCo 3");

            if (this._numberOfPlayersInTeam > 0) {
                while (PhotonUserDataManager.Instance.NumberOfPlayers != this._numberOfPlayers) {
                    yield return null;
                }
            }

            Debug.Log("MATCHING USER!");

            if (PhotonNetwork.IsMasterClient) {
                PhotonUserDataManager.Instance.CreateTeam(this._matchingMode == Constants.MatchingMode.Friend, this._numberOfTeams);
                PhotonCommonRPC.Instance.UpdateTeamData(PhotonUserDataManager.Instance.TeamDataAll);
            }

            while (this._numberOfTeams != PhotonUserDataManager.Instance.NumberOfTeams) {
                yield return null;
            }
            Debug.Log($"Valid NumberOfTeams {this._numberOfTeams}");

            this.OnMatching();
        }


        public void CancelMatching() {
            Debug.Log("start CancelMatching");

            this._status = Status.MatchingCanceling;
            if (this._matchingMainCo == null) {
                this.OnCancelMatchingInternal();
            } else {
                StopCoroutine(this._matchingMainCo);
                this._matchingMainCo = null;
                StartCoroutine(this.cancelMatchingCo());
            }
        }

        private IEnumerator cancelMatchingCo() {
            if (this._matchingCo != null) {
                while (!this._canCancelMatching) {
                    // マッチング実行中であれば、キャンセル可能になるまで待つ
                    yield return null;
                }

                StopCoroutine(this._matchingCo);
                this._matchingCo = null;
            }

            if (PhotonNetwork.InRoom) {
                // 部屋に居たら部屋から出る
                PhotonNetwork.LeaveRoom();
            }

            while (PhotonNetwork.InRoom) {
                yield return null;
            }

            while (!PhotonNetwork.InLobby) {
                yield return null;
            }

            this.OnCancelMatchingInternal();
        }

        private void OnCancelMatchingInternal() {
            this._isBusy = false;
            this._findFriendReason = FindFriendReason.None;
            PhotonUserDataManager.Instance.Reset();
            this.OnMatchingCancelListener?.Invoke();
            this._status = Status.MatchingCanceled;
        }

        public override void OnFriendListUpdate(List<FriendInfo> friendList) {
            Debug.Log($"OnFriendListUpdate:{friendList.Count} reason:{this._findFriendReason.ToString()}");

            if (this._status == Status.MatchingCanceling) {
                // キャンセル中であれば部屋に入らない
                Debug.Log("Matching is Canceling");
                return;
            }

            foreach (var friend in friendList) {
                Debug.Log($"friend user id:{friend.UserId} IsOnline:{friend.IsOnline} IsInRoom:{friend.IsInRoom}");

                if (this._findFriendReason == FindFriendReason.JoinRoom) {
                    if (friend.IsInRoom) {
                        Debug.Log($"Friend is in the room, so join the room.");

                        PhotonNetwork.JoinRoom(friend.Room);
                        this._findFriendReason = FindFriendReason.None;
                    }
                }
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
            Debug.Log("OnRoomPropertiesUpdate");
            foreach (var prop in propertiesThatChanged) {
                Debug.Log($"{prop.Key}: {prop.Value}");
            }
        }

        private void OnMatching() {            
            StopCoroutine(this._matchingMainCo);
            this._matchingMainCo = null;

            this._status = Status.Matched;

            if (PhotonNetwork.IsMasterClient) {
                this.CloseRoom();
            }

            Debug.Log("OnMatchingListener");
            this.OnMatchingListener?.Invoke();
        }


        private string getRoomFilter() {
            int aid = ApplicationDataManager.Instance != null ? (int)ApplicationDataManager.Instance.ApplicationID : 1;
            string filter = $"{MODE_PROP_KEY} = {(int)this._matchingMode} AND {AID_PROP_KEY} = {aid}";
            if (this._matchingMode == Constants.MatchingMode.Versus && this._recentGameResult != Constants.MatchingGameResult.None) {
                filter += $" AND {RECENTRESULT_PROP_KEY} = {(int)this._recentGameResult}";
            }
            return filter;
        }

        public bool GetCustomRoomList(UnityAction<List<RoomInfo>> onGetCustomRoolList = null) {
            if (this._isBusy) {
                return false;
            }
            this._isBusy = true;
            this._onGetCustomRoolList = onGetCustomRoolList;
            string filter = this.getRoomFilter();
            Debug.Log($"GetCustomRoomList:{filter}");
            return PhotonNetwork.GetCustomRoomList(this._sqlLobby, filter);
        }


        public bool GetCustomRoomListAll(UnityAction<List<RoomInfo>> onGetCustomRoolList = null) {
            if (this._isBusy) {
                return false;
            }
            this._isBusy = true;
            this._onGetCustomRoolList = onGetCustomRoolList;
            string filter = $"{AID_PROP_KEY} > 0";
            Debug.Log($"GetCustomRoomList:{filter}");
            return PhotonNetwork.GetCustomRoomList(this._sqlLobby, filter);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList) {
            Debug.Log($"OnRoomListUpdate:{roomList.Count}");
            this._roomList = roomList;
            this._onGetCustomRoolList?.Invoke(roomList);
            this._isBusy = false;
        }

        public void SetNickName(string nickName) {
            PhotonNetwork.LocalPlayer.NickName = nickName;

            Debug.Log($"nickname:{PhotonNetwork.LocalPlayer.NickName}");
        }

        public void RejoinRoom() {
            PhotonNetwork.RejoinRoom(this._currentRoomName);
        }

        public void Ready(string userID) {
            PhotonCommonRPC.Instance.Ready(userID);
        }

        /// <summary>
        /// PhotonのUSERIDでニックネーム取得
        /// </summary>
        /// <param name="userID"></param>
        /// <returns>ニックネーム</returns>
        public string GetNickNameByUserID(string userID) {
            if (!PhotonNetwork.InRoom) {
                return "";
            }

            Player player = PhotonNetwork.PlayerList.FirstOrDefault(e => e.UserId == userID);
            if (player == null) {
                return "";
            }

            return player.NickName;
        }

        /// <summary>
        /// パラレルのUSERIDでニックネーム取得
        /// </summary>
        /// <param name="userID"></param>
        /// <returns>ニックネーム</returns>
        public string GetNickNameByParallelUserID(int userID) {
            if (!PhotonNetwork.InRoom) {
                return "";
            }

            foreach (Player player in PhotonNetwork.PlayerList) {
                int gid;
                int uid;
                PhotonUtil.GetParallelGroupRoomIDAndUserIDFromPhotonUserID(player.UserId, out gid, out uid);
                if (userID == uid) {
                    return player.NickName;
                }
            }

            return "";
        }

        /// <summary>
        /// PhotonのUSERIDからユーザーアイコンスプライト取得
        /// </summary>
        /// <param name="userID"></param>
        /// <returns>スプライト</returns>
        public Sprite GetPlayerIconSpriteByUserID(string userID) {
            if (!PhotonNetwork.InRoom) {
                return null;
            }

            int gid;
            int uid;
            PhotonUtil.GetParallelGroupRoomIDAndUserIDFromPhotonUserID(userID, out gid, out uid);
            return UserDataManager.Instance?.GetUserIamge(uid);
        }

        public void DestroyAll() {
            if (PhotonNetwork.IsMasterClient) {
                PhotonNetwork.DestroyAll();
            }
        }

        public override void OnDisable() {
            base.OnDisable();

            if (PhotonNetwork.IsConnected) {
                PhotonNetwork.Disconnect();
            }
        }
    }

    static class PhtonPlayerExtensions {
        public static string GetUserId(this Player player) {
            if (!PhotonManager.OfflineMode) {
                return player.UserId;
            } else {
                return PhotonManager.OfflineUserId;
            }
        }
    }

}