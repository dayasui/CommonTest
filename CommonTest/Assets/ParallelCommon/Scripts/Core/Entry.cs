using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ParallelDummyLogin;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace ParallelCommon {

    public interface INetworkInitialize {
        
    }

    public class NetworkInitializeStandAlone : INetworkInitialize {
        
    }

    public class NetworkInitialize : INetworkInitialize  {
        
    }
    
    public class NetworkInitializeWebGL : INetworkInitialize  {
        
    }
    public class Entry : MonoBehaviour {

        void Awake() {
            SceneManager.LoadScene("ParallelManagerScene", LoadSceneMode.Additive);
        }
        
        // Start is called before the first frame update
        private void Start() {
            /*
            MessageBridgeManager.Instance.HideNextContentList();
            MessageBridgeManager.Instance.GetAppID((Constants.ApplicationID applicationID) => {
                ApplicationDataManager.Instance.ApplicationID = applicationID;
                //StartUp();
            })
            */
            SceneManager.UnloadSceneAsync("ParallelEntryScene");
            SceneManager.LoadScene("MessageBridgeTestScene", LoadSceneMode.Additive);
            
        }
        
        /*
        public void StartUp() {
            Constants.ApplicationID applicationID = ApplicationDataManager.Instance.ApplicationID;
            if (ApplicationDataManager.Instance.IsStandAlone) {
                networkInitializeStandAlone(() => {
                    MySceneManager.Instance.ChangeScene(data.sceneType);
                });
            } else {
#if !UNITY_WEBGL
                networkInitialize(() => {
                    MySceneManager.Instance.ChangeScene(data.sceneType);
                });
#else
                //networkInitializeWebGL(() => {
                //    MySceneManager.Instance.ChangeScene(data.sceneType);
                //});
                networkInitialize(() => {
                    MySceneManager.Instance.ChangeScene(data.sceneType);
                });
#endif
            }
            
        }
        */

        /*
        /// <summary>
        /// （基本的に）Unity　Editor用の処理
        /// </summary>
        static private void networkInitializeStandAlone(UnityAction onComplete) {
#if !UNITY_WEBGL || UNITY_EDITOR
            NetworkNetworkManager.Instance.GetChatRoomSessionID((json) => {
                // オーナー以外はここで、chat_group_room_session_idを取得する
                JSONObject jsonObj = new JSONObject(json);
                var roomSessions = jsonObj.GetField("chat_group_room")
                    .GetField("chat_group_room_sessions").list;
                NetworkManager.Instance.ChatGroupRoomSessionID =
                    (int)roomSessions[0].GetField("id").i;

                // 一人退室用にchat_group_room_session_user_idを取得する
                var roomSessionUsers =
                    roomSessions[0].GetField("chat_group_room_session_users").list;
                foreach (var roomSessionUser in roomSessionUsers) {
                    var userID = (int)roomSessionUser.GetField("user").GetField("id").i;
                    if (userID == UserDataManager.UserID) {
                        NetworkManager.Instance.ChatGroupRoomSessionUserID =
                            (int)roomSessionUser.GetField("id").i;
                    }
                    ParallelChatRoomSessionData.SessionUser userData = JsonUtility.FromJson<ParallelChatRoomSessionData.SessionUser>(roomSessionUser.ToString());
                    // オーナーのユーザIDを保持しておく
                    if (userData.is_owner) {
                        UserDataManager.OwnerUserID = userData.user.id;
                    }
                    UserDataManager.Instance.AddUsersData(userData);
                }
                updateWebSocketEndPoint(() => {
                    Receiver.Instance.ConnectWebSocket(() => {
                        if (onComplete != null) {
                            onComplete();
                        }
                    });
                });
            });
#else
            // WEBGL?
            //debugSetCookies();
            networkInitializeWebGL(onComplete);
#endif

        }
        */

        /*
        /// <summary>
        /// （基本的に）実機用の処理
        /// </summary>
        /// <param name="onComplete"></param>
        static private void networkInitialize(UnityAction onComplete) {
            UnityMessageManagerWrapper.Instance.GetInitValue((initData) => {
                NetworkManager.Instance.Init(initData.api_host, initData.token, initData.device_id,
                    initData.platform, initData.app_version, initData.accept_language);
                NetworkManager.Instance.ChatGroupID = initData.chat_group_id;
                NetworkManager.Instance.ChatGroupRoomID = initData.chat_group_room_id;
                common.UserDataManager.UserID = initData.user_id;
                ApplicationDataManager.Instance.IsObserver = initData.is_observer;

                UnityMessageManagerWrapper.Instance.GetChatRoomSession((sessionData) => {
                    NetworkManager.Instance.ChatGroupRoomSessionID = sessionData.chat_group_room_session.id;
                    Debug.Log($"ChatGroupRoomSessionID:{NetworkManager.Instance.ChatGroupRoomSessionID }");
                    updateWebSocketEndPoint(() => {
                        Receiver.Instance.ConnectWebSocket(() => {
                            var sessionUser =
                                sessionData.chat_group_room_session.chat_group_room_session_users.FirstOrDefault(
                                    e => e.user.id == common.UserDataManager.UserID);
                            if (sessionUser != null) {
                                common.UserDataManager.IsOwner = sessionUser.is_owner;
                                foreach (var user in sessionData.chat_group_room_session
                                    .chat_group_room_session_users) {
                                    // オーナーのユーザIDを保持しておく
                                    if (user.is_owner) {
                                        UserDataManager.OwnerUserID = user.user.id;
                                    }

                                    UserDataManager.Instance.AddUsersData(user);
                                }
                            } else {
                                Debug.LogError("sessionDataに自分がいない");
                            }

                            if (onComplete != null) {
                                onComplete();
                            }
                        });
                    });
                });
            }, ApplicationDataManager.Instance.IsParallelDummy);
        }
        */

        /*
        /// <summary>
        /// （基本的に）実機用WebGLの処理
        /// </summary>
        /// <param name="onComplete"></param>
        static private void networkInitializeWebGL(UnityAction onComplete) {
            Debug.Log("networkInitializeWebGL");

            UnityMessageManagerWrapper.Instance.GetInitValue((initData) => {
                NetworkManager.Instance.Init(initData.api_host, initData.token, initData.device_id,
                    initData.platform, initData.app_version, initData.accept_language);
                NetworkManager.Instance.ChatGroupID = initData.chat_group_id;
                NetworkManager.Instance.ChatGroupRoomID = initData.chat_group_room_id;
                NetworkManager.Instance.ChatGroupRoomSessionID = initData.chat_group_room_session_id;
                common.UserDataManager.UserID = initData.user_id;
                ApplicationDataManager.Instance.IsObserver = initData.is_observer;

                Debug.Log($"ChatGroupID:{NetworkManager.Instance.ChatGroupID}");
                Debug.Log($"ChatGroupRoomID:{NetworkManager.Instance.ChatGroupRoomID}");
                Debug.Log($"ChatGroupRoomSessionID:{NetworkManager.Instance.ChatGroupRoomSessionID}");

                initializeWebGLCore(onComplete);
            }, ApplicationDataManager.Instance.IsParallelDummy);
        }
        */

        /*
        static private void initializeWebGLCore(UnityAction onComplete) {
            NetworkManager.Instance.GeneralAPIAuth((userData) => {
                UserDataManager.UserID = userData.id;
                NetworkManager.Instance.GeneralAPIGetInfo((sessionData) => {
                    Receiver.Instance.ConnectWebSocket(() => {
                        Debug.Log($"UserID:{UserDataManager.UserID} OwnerUserID:{sessionData.OwnerUserID}");
                        common.UserDataManager.IsOwner = sessionData.OwnerUserID == UserDataManager.UserID;

                        List<ParallelChatRoomSessionData.SessionUser> sessionUsers = new List<ParallelChatRoomSessionData.SessionUser>();

                        // オーナー
                        ParallelChatRoomSessionData.SessionUser owner = new ParallelChatRoomSessionData.SessionUser();
                        owner.is_observer = false;
                        owner.is_deleted = false;
                        owner.is_owner = true;
                        owner.is_player = true;
                        owner.user = new ParallelUserData(sessionData.owner);
                        sessionUsers.Add(owner);
                        UserDataManager.OwnerUserID = owner.user.id;  // オーナーのユーザIDを保持しておく

                        foreach (var guest in sessionData.guests) {
                            ParallelChatRoomSessionData.SessionUser user = new ParallelChatRoomSessionData.SessionUser();
                            user.is_observer = false;
                            user.is_deleted = false;
                            user.is_owner = false;
                            user.is_player = true;
                            user.user = new ParallelUserData(guest);
                            sessionUsers.Add(user);
                        }

                        foreach (var guest in sessionData.web_guests) {
                            // TODO
                        }

                        foreach (var user in sessionUsers) {
                            UserDataManager.Instance.AddUsersData(user);
                        }

                        onComplete?.Invoke();
                    });
                });
            });
        }
        */

        /*
        static private void updateWebSocketEndPoint(UnityAction onComplete) {
#if !UNITY_WEBGL
            NetworkManager.Instance.UpdateWebSocketEndPoint(onComplete);
#else
            // WebSocket接続に必要なパラメータをcookieに埋め込んでもらうために呼ぶ
            NetworkManager.Instance.GeneralAPIAuth((userData) => {
                NetworkManager.Instance.UpdateWebSocketEndPoint(onComplete);
            });
#endif
        }
        */
    }
}