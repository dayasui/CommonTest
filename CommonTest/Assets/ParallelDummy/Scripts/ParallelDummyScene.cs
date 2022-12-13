using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;
using ParallelCommon;
using UnityEngine.SceneManagement;

namespace ParallelDummy {

    public class ParallelDummyScene : MonoBehaviour {
        [SerializeField] private Text _titleText = null;
        [SerializeField] private Text _userNameText = null;
        [SerializeField] private GameObject _roomContentRoot = null;
        [SerializeField] private GameObject _roomItemPrefab = null;
        [SerializeField] private GameObject _buttonRootObject = null;
        [SerializeField] private GameObject _appContentRoot = null;
        [SerializeField] private GameObject _appItemPrefab = null;
        [SerializeField] private Scrollbar _appScrollBar = null;

        [SerializeField] private Constants.ApplicationID _applicationId;
        [SerializeField] private string _appName;
        [SerializeField] private int _appRequired;
        
        private int _id;
        private string _name;
        private string _token;
        private string _device_id;
        private string _server_url;
        private int _ownerUserId = 0;
        private int _userId = 0;
        private bool _isOwner = false;
        

        private bool _isBusy = false;
        private List<int> _userIDs = new List<int>();
        private List<RoomItem> _roomList = new List<RoomItem>();
        private IParallelNetwork _parallelDummyNetwork = new ParallelDummyNetwork();
        private ParallelDummyAPIs _parallelDummyAPIs = new ParallelDummyAPIs();
        

        // Start is called before the first frame update
       private void Start() {

            var dummyEnvData = ParallelDummyEnvProvider.Load();
            var accountData = dummyEnvData.accounts[dummyEnvData.account_index];
            this._id = accountData.id;
            this._name = accountData.name;
            this._token = accountData.token;
            this._device_id = accountData.device_id;
            this._server_url = dummyEnvData.server_url;
            
            this._isBusy = false;
            this._parallelDummyNetwork.ChatGroupID = -1;
            this._userIDs.Clear();
            this._buttonRootObject.SetActive(false);
            this._userNameText.text = this._name;
            this._roomList.Clear();
            this._parallelDummyNetwork.Init(this._server_url, this._token, this._device_id);
            this.createRooms();
        }

        private void createRooms(UnityAction onComplete = null) {
            this._isBusy = true;
            StartCoroutine(this.createRoomRoutine(() => {
                this._isBusy = false; 
                onComplete?.Invoke();
            }));
        }

        private IEnumerator createRoomRoutine(UnityAction onComplete) {
            bool wait = true;
            this._parallelDummyAPIs.Reset(this._parallelDummyNetwork, this._id, (data) => { wait = false; });
            while (wait) {
                yield return null;
            }

            wait = true;

            NetChatGroupDataArray dataArray = null;
            this._parallelDummyAPIs.GetChatGroupData(this._parallelDummyNetwork, (data) => {
                dataArray = data;
                wait = false;
            });

            while (wait) {
                yield return null;
            }
            
            Util.DestroyChildObject(this._roomContentRoot.transform);
            foreach (var group in dataArray.chat_groups) {
                GameObject go = Instantiate(this._roomItemPrefab, this._roomContentRoot.transform);
                RoomItem room = go.GetComponent<RoomItem>();
                this._roomList.Add(room);
                room.Init(group, this.joinRoom);
            }
            onComplete?.Invoke();
        }

        private void joinRoom(RoomItem roomItem) {
            Debug.Log("joinRoom a");
            if (this._isBusy) {
                return;
            }
            this._isBusy = true;
            Debug.Log("joinRoom b");
            StartCoroutine(this.joinRoomRoutine(roomItem));
        }

        public void OnClickLeaveRoom() {
            this.leaveRoom();
        }

        private void leaveRoom() {
            if (this._isBusy) {
                return;
            }

            this._isBusy = true;
            this._buttonRootObject.SetActive(false);
            this._parallelDummyAPIs.LeaveRoom(this._parallelDummyNetwork,() => {
                this._isBusy = false;
                this.createRooms();
            });
        }

        private IEnumerator joinRoomRoutine(RoomItem roomItem) {
            Debug.Log("JoinRoomRoutine");
            RoomItem[] children = this._roomContentRoot.GetComponentsInChildren<RoomItem>();
            foreach (var child in children) {
                if (child == roomItem) {
                    child.Disacitvate();
                } else {
                    Destroy(child.gameObject);
                }
            }
            this._parallelDummyNetwork.ChatGroupID = roomItem.Data.id;
            bool wait = true;

            NetChatGroupRoomDataArray dataArray = null;
            this._parallelDummyAPIs.GetChatGroupRoomID(this._parallelDummyNetwork, (d) => {
                dataArray = d;
                wait = false;
            });

            while (wait) {
                yield return null;
            }

            if (dataArray.chat_group_rooms.Length == 0) {
                Debug.Log("参加可能なルームがありません");
                yield break;
            }

            this._parallelDummyNetwork.ChatGroupRoomID = dataArray.chat_group_rooms[0].id;
            wait = true;
            NetJoinRoomData roomData = null;
            string error = null;
            this._parallelDummyAPIs.JoinRoom(this._parallelDummyNetwork, (r, e) => {
                roomData = r;
                error = e;
                wait = false;
            });

            while (wait) {
                yield return null;
            }

            if (error != null) {
                //DialogManager.Instance.OpenDialogDummyWarning(error, () => { this._isBusy = false; });
            } else {
                this._buttonRootObject.SetActive(true);
                this._isBusy = false;

                if (roomData.chat_group_room.chat_group_room_sessions != null &&
                    roomData.chat_group_room.chat_group_room_sessions.Length > 0) {
                    // 途中入室処理
                    var roomSession = roomData.chat_group_room.chat_group_room_sessions[0];
                    this._parallelDummyNetwork.ChatGroupRoomSessionID = roomSession.id;
                    this._ownerUserId = roomData.chat_group_room.chat_group_room_sessions[0].OwnerUserID;

                    wait = true;
                    error = null;

                    bool canObserve = false;
                    this._parallelDummyAPIs.JoinSession(this._parallelDummyNetwork, this._userId, (j, e) => {
                        ApplicationDataManager.Instance.IsObserver = canObserve;
                        wait = false;
                        error = e;
                    });
                    while (wait) {
                        yield return null;
                    }

                    if (error != null) {
                        //DialogManager.Instance.OpenDialogDummyWarning(error, () => { this._isBusy = false; });
                    } else {
                        StartCoroutine(this.startGameRoutine(false));
                    }
                }
            }
        }

        public void OnClickOwner() {
            Debug.Log("OnClickOwner");
            if (this._isBusy || this._parallelDummyNetwork.ChatGroupID == -1) {
                return;
            }

            this._isBusy = true;

            StartCoroutine(this.startGameRoutine(true));
        }

        public void OnClickOther() {
            Debug.Log("OnClickOther");
            if (this._isBusy || this._parallelDummyNetwork.ChatGroupID == -1) {
                return;
            }

            this._isBusy = true;
            StartCoroutine(this.startGameRoutine(false));
        }

        private IEnumerator startGameRoutine(bool isOwner) {
            bool wait = true;
            //// Websocket接続に必要なendpointを取得する
            //NetworkManager.Instance.UpdateWebSocketEndPoint(() => {
            //    wait = false;
            //});
            //while(wait) { yield return null; }

            //wait = true;
            //// WebSocket接続
            //common.Receiver.Instance.ConnectWebSocket(() => {
            //    wait = false;
            //});
            //while (wait) { yield return null; }

            wait = true;
            NetUserListDataArray userList = null;
            this._parallelDummyAPIs.GetUserList(this._parallelDummyNetwork, (list) => {
                userList = list;
                wait = false;
            });
            while (wait) {
                yield return null;
            }

            this._userIDs.Clear();
            foreach (var user in userList.users) {
                this._userIDs.Add(user.id);
            }
            
            if (this._userIDs.Count < this._appRequired) {
                string message = string.Format("ルーム参加者が足りません", this._id, this._name);
                //DialogManager.Instance.OpenDialogDummyWarning(message, () => { this._isBusy = false; });
                yield break;
            }

            //common.UserDataManager.Instance.SetUsersData(userList.users);

            if (isOwner) {
                // CreateRoomSessionを呼んだユーザーがオーナーになる
                this._isOwner = true;
                // ルームセッション作成
                this._parallelDummyAPIs.CreateRoomSession(this._parallelDummyNetwork, this._applicationId,
                    this._userIDs.ToArray(), (sessionData) => {
                        if (sessionData.chat_group_room_session.id == 0) {
                            string message = string.Format("ルームセッション生成に失敗しました");
                            //DialogManager.Instance.OpenDialogDummyWarning(message, () => { this._isBusy = false; });
                        } else {
                            this._parallelDummyNetwork.ChatGroupRoomSessionID = sessionData.chat_group_room_session.id;
                            this.changeScene();
                        }
                    });
            } else {
                this._isOwner = false;
                this.changeScene();
            }

            this._isBusy = false;
        }

        private void changeScene() {
            SceneManager.LoadScene("ParallelEntryScene", LoadSceneMode.Additive);
        }
    }
}
