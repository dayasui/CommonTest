using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ParallelCommon {
    public class MessageBridgeEditor : IMessageBridge {
        public void Init() { }

        public void Clear() { }

        public void GetInitValue(UnityAction<ParallelInitData> callBack) {
            var envData = ParallelDummy.ParallelDummyEnvProvider.Load();
            var initData = new ParallelInitData();
            initData.user_id = envData.SelectAccount.id;
            initData.device_id = envData.SelectAccount.device_id;
            initData.token = envData.SelectAccount.token;
            initData.api_host = envData.server_url;
            initData.chat_group_id = envData.chat_group_id;
            initData.chat_group_room_id = envData.chat_group_roomID;
            callBack?.Invoke(initData);
        }
        
        public void GetChatRoomSession(UnityAction<ParallelChatRoomSessionDataAll> callBack) {
            var envData = ParallelDummy.ParallelDummyEnvProvider.Load();
            var sessionData = new ParallelChatRoomSessionDataAll();
            sessionData.chat_group_room_session = new ParallelChatRoomSessionData();
            sessionData.chat_group_room_session.chat_group_room_session_users =
                new ParallelChatRoomSessionData.SessionUser[envData.user.Count];
            var sessionUsers = sessionData.chat_group_room_session.chat_group_room_session_users;

            for (int i = 0; i < envData.user.Count; i++) {
                sessionUsers[i] = new ParallelChatRoomSessionData.SessionUser();
                sessionUsers[i].is_owner = envData.owner_user_id == envData.user[i].id;
                sessionUsers[i].user = new ParallelUserData(envData.user[i]);
            }
            callBack?.Invoke(sessionData);
        }
        
        public void SendLog(string eventName, string key, string value) {
            if (ApplicationDataManager.Instance == null) {
                return;
            }

            if (ApplicationDataManager.Instance.IsObserver) {
                return;
            }
        
            if(ApplicationDataManager.Instance.IsStandAlone) {
                return;
            }
            Debug.Log("[FireBaseEvent] eventName:" + eventName + " key:" + key + " value:" + value);
            // TODO
        }
        
        public void GetAppID(UnityAction<int> callBack) {
            // TODO
            callBack?.Invoke(0);
        }
        
        public void ResetSession() {
            // TODO
        }

        public void ShowHeader() { }
        public void HideHeader() { }
        public void ShowFooter() { }
        public void HideFooter() { }
        public void ShowNextContentList() { }
        public void HideNextContentList() { }
        public void OpenWebView(string url) { }
        public void CloseWebView() { }
    }
}