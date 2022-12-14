using UnityEngine;
using UnityEngine.Events;

namespace ParallelCommon {
    public class MessageBridgeIOS : IMessageBridge {
        public void Init() { }

        public void Clear() { }

        public void GetInitValue(UnityAction<ParallelInitData> callBack) {
            string json = NativeAPI.GetInitValue();
            Debug.Log("GetInitValue:" + json);
            callBack?.Invoke(JsonUtility.FromJson<ParallelInitData>(json));
        }
        
        public void GetChatRoomSession(UnityAction<ParallelChatRoomSessionDataAll> callBack) {
            string json = NativeAPI.GetSession();
            Debug.Log("GetChatRoomSession:" + json);
            callBack?.Invoke(JsonUtility.FromJson<ParallelChatRoomSessionDataAll>(json));
        }
        
        public void SendLog(string eventName, string key, string value) {
            Debug.Log("[FireBaseEvent] eventName:" + eventName + " key:" + key + " value:" + value);
            if (ApplicationDataManager.Instance == null) {
                return;
            }

            if (ApplicationDataManager.Instance.IsObserver) {
                return;
            }
        
            if(ApplicationDataManager.Instance.IsStandAlone) {
                return;
            }
            NativeAPI.SendLog(eventName, key, value);
        }
        
        public void GetAppID(UnityAction<int> callBack) {
            int appID = NativeAPI.GetAppId();
            callBack?.Invoke(appID);
        }
        
        public void ResetSession() {
            NativeAPI.ResetSession();
        }
        
        public void ShowHeader() { 
            NativeAPI.ShowHeader();
        }

        public void HideHeader() {
            NativeAPI.HideHeader();
        }

        public void ShowFooter() {
            NativeAPI.ShowFooter();
        }

        public void HideFooter() {
            NativeAPI.HideFooter();
        }

        public void ShowNextContentList() {
            NativeAPI.ShowNextContentList();
        }
    
        public void HideNextContentList() {
            NativeAPI.HideNextContentList();
        }

        public void OpenWebView(string url) {
            NativeAPI.OpenWebView(url);
        }
    
        public void CloseWebView() {
            NativeAPI.CloseWebView();
        }
    }
}
