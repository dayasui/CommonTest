using System.Collections;
using System.Collections.Generic;
using BestHTTP.JSON;
using ParallelCommon;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ParallelDummy {

    public class ParallelDummyScene : MonoBehaviour {
        [SerializeField] private Text _titleText = null;
        [SerializeField] private Text _userNameText = null;
        [SerializeField] private GameObject _roomContentRoot = null;
        [SerializeField] private GameObject _roomItemPrefab = null;
        [SerializeField] private GameObject _buttonRootObject = null;
        private bool _isBusy = false;
        private List<int> _userIDs = new List<int>();
        private List<RoomItem> _roomList = new List<RoomItem>();
        private ParallelCommon.IParallelNetwork _parallelDummyNetwork = new ParallelDummyNetwork();
        private ParallelDummyAPIs _parallelDummyAPIs = new ParallelDummyAPIs();
        private ParallelDummyEnvData _dummyEnvData = null;

        // Start is called before the first frame update
        private void Start() {
            this._dummyEnvData = ParallelDummyEnvProvider.Load();
            this._titleText.text = this._dummyEnvData.app_name;
            this._isBusy = false;
            this._parallelDummyNetwork.ChatGroupID = -1;
            this._userIDs.Clear();
            this._buttonRootObject.SetActive(false);
            this._userNameText.text = this._dummyEnvData.SelectAccount.name;
            this._roomList.Clear();
            this._parallelDummyNetwork.Init(this._dummyEnvData.server_url, this._dummyEnvData.SelectAccount.token, this._dummyEnvData.SelectAccount.device_id);
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
            this._parallelDummyAPIs.Reset(this._parallelDummyNetwork, this._dummyEnvData.SelectAccount.id, (data) => { wait = false; });
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
            
            ParallelCommon.Util.DestroyChildObject(this._roomContentRoot.transform);
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
                Debug.LogError("入室に失敗しました");
                this._isBusy = false;
                //DialogManager.Instance.OpenDialogDummyWarning(error, () => { this._isBusy = false; });
            } else {
                this._buttonRootObject.SetActive(true);
                this._isBusy = false;

                if (roomData.chat_group_room.chat_group_room_sessions != null &&
                    roomData.chat_group_room.chat_group_room_sessions.Length > 0) {
                    // 途中入室処理
                    var roomSession = roomData.chat_group_room.chat_group_room_sessions[0];
                    this._parallelDummyNetwork.ChatGroupRoomSessionID = roomSession.id;
                    this._dummyEnvData.owner_user_id = roomData.chat_group_room.chat_group_room_sessions[0].OwnerUserID;

                    wait = true;
                    error = null;
                    
                    this._parallelDummyAPIs.JoinSession(this._parallelDummyNetwork, this._dummyEnvData.is_observer, this._dummyEnvData.SelectAccount.id, (j, e) => {
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
            
            if (this._userIDs.Count < this._dummyEnvData.app_required) {
                Debug.LogError("ルーム参加者が足りません");
                this._isBusy = false;
                //string message = string.Format("ルーム参加者が足りません", this._dummyEnvData.SelectAccount.id, this._dummyEnvData.SelectAccount.name);
                //DialogManager.Instance.OpenDialogDummyWarning(message, () => { this._isBusy = false; });
                yield break;
            }

            //common.UserDataManager.Instance.SetUsersData(userList.users);

            if (isOwner) {
                // ルームセッション作成
                this._parallelDummyAPIs.CreateRoomSession(this._parallelDummyNetwork, this._dummyEnvData.app_id,
                    this._userIDs.ToArray(), (sessionData) => {
                        if (sessionData.chat_group_room_session.id == 0) {
                            string message = string.Format("ルームセッション生成に失敗しました");
                            //DialogManager.Instance.OpenDialogDummyWarning(message, () => { this._isBusy = false; });
                        } else {
                            this._parallelDummyNetwork.ChatGroupRoomSessionID = sessionData.chat_group_room_session.id;
                            this._dummyEnvData.owner_user_id = this._dummyEnvData.SelectAccount.id;
                            this._dummyEnvData.user.Clear();
                            foreach (var sessionUser in sessionData.chat_group_room_session.chat_group_room_session_users) {
                                this._dummyEnvData.user.Add(sessionUser.user);
                            }
                            this.changeScene();
                        }
                    });
            } else {
                this._parallelDummyAPIs.GetChatRoomSessionID(this._parallelDummyNetwork, (json) => {
                    // オーナー以外はここで、chat_group_room_session_idを取得する
                    JSONObject jsonObj = new JSONObject(json);
                    var chatGroupRoom = jsonObj.GetField("chat_group_room");
                    var roomSessions = chatGroupRoom.GetField("chat_group_room_sessions").list;
                    ParallelChatRoomSessionData roomSessionData =
                        JsonUtility.FromJson<ParallelChatRoomSessionData>(roomSessions[0].ToString());
                    this._parallelDummyNetwork.ChatGroupRoomSessionID = (int)roomSessions[0].GetField("id").i;
                    var sessionUsers = roomSessionData.chat_group_room_session_users;
                    this._dummyEnvData.user.Clear();
                    foreach (var sessionUser in sessionUsers) {
                        this._dummyEnvData.user.Add(sessionUser.user);
                        if (sessionUser.is_owner) {
                            this._dummyEnvData.owner_user_id = sessionUser.user.id;
                        }
                    }
                    /*
                    var roomSessions = jsonObj.GetField("chat_group_room")
                        .GetField("chat_group_room_sessions").list;
                    this._parallelDummyNetwork.ChatGroupRoomSessionID =
                        (int) roomSessions[0].GetField("id").i;
                    
                    // 一人退室用にchat_group_room_session_user_idを取得する
                    var roomSessionUsers = roomSessions[0].GetField("chat_group_room_session_users").list;
                    foreach (var roomSessionUser in roomSessionUsers) {
                        var userID = (int) roomSessionUser.GetField("user").GetField("id").i;
                        if (userID == this._dummyEnvData.SelectAccount.id) {
                            this._parallelDummyNetwork.ChatGroupRoomSessionUserID =
                                (int) roomSessionUser.GetField("id").i;
                        }

                        ParallelCommon.ParallelChatRoomSessionData.SessionUser userData =
                            JsonUtility.FromJson<ParallelCommon.ParallelChatRoomSessionData.SessionUser>(
                                roomSessionUser.ToString());
                        // オーナーのユーザIDを保持しておく
                        if (userData.is_owner) {
                            this._dummyEnvData.owner_user_id = userData.user.id;
                        }
                    }
                    */

                    this.changeScene();
                });
            }

            this._isBusy = false;
        }

        private void changeScene() {
            this._dummyEnvData.chat_group_id = _parallelDummyNetwork.ChatGroupID;
            this._dummyEnvData.chat_group_roomID = _parallelDummyNetwork.ChatGroupRoomID;
            this._dummyEnvData.chat_group_roomSessionID = _parallelDummyNetwork.ChatGroupRoomSessionID;
            this._dummyEnvData.chat_group_roomSessionUserID = _parallelDummyNetwork.ChatGroupRoomSessionUserID;
            var json = JsonUtility.ToJson(this._dummyEnvData);
            ParallelDummyEnvProvider.Save(json);
            SceneManager.LoadScene("ParallelEntryScene");
        }
    }
}
