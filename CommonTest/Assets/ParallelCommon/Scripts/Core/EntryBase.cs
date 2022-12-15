using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace ParallelCommon {
    public class EntryBase : MonoBehaviour {
        protected virtual void Awake() {
            SceneManager.LoadScene("ParallelManagerScene", LoadSceneMode.Additive);
        }
        
        // Start is called before the first frame update
        protected virtual void Start() {
            MessageBridgeManager.Instance.HideNextContentList();
            this.Initialize(() => {
                this.OnInitializeComplete();
            });
        }

        protected virtual void OnInitializeComplete() { }

        private void Initialize(UnityAction onComplete) {
            MessageBridgeManager.Instance.GetInitValue((initData) => {
                NetworkManager.Instance.Init(initData.api_host, initData.token, initData.device_id,
                    initData.platform, initData.app_version, initData.accept_language);
                NetworkManager.Instance.ChatGroupID = initData.chat_group_id;
                NetworkManager.Instance.ChatGroupRoomID = initData.chat_group_room_id;
                UserDataManager.UserID = initData.user_id;
                ApplicationDataManager.Instance.IsObserver = initData.is_observer;

                MessageBridgeManager.Instance.GetChatRoomSession((sessionData) => {
                    NetworkManager.Instance.ChatGroupRoomSessionID = sessionData.chat_group_room_session.id;
                    Debug.Log($"ChatGroupRoomSessionID:{NetworkManager.Instance.ChatGroupRoomSessionID }");
                    updateWebSocketEndPoint(() => {
                        
                        //Receiver.Instance.ConnectWebSocket(() => {

                            
                            var sessionUser =
                                sessionData.chat_group_room_session.chat_group_room_session_users.FirstOrDefault(
                                    e => e.user?.id == UserDataManager.UserID);
                            if (sessionUser != null) {
                                UserDataManager.IsOwner = sessionUser.is_owner;
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
                        //});
                    });
                });
            });
        }
        
        
        private void updateWebSocketEndPoint(UnityAction onComplete) {
#if !UNITY_WEBGL
            NetworkManager.Instance.UpdateWebSocketEndPoint(onComplete);
#else
            // WebSocket接続に必要なパラメータをcookieに埋め込んでもらうために呼ぶ
            NetworkManager.Instance.GeneralAPIAuth((userData) => {
                NetworkManager.Instance.UpdateWebSocketEndPoint(onComplete);
            });
#endif
        }
    }
}