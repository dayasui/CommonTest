using UnityEngine;
using UnityEngine.Events;

namespace ParallelCommon {
    public class MessageBridgeWebGL : IMessageBridge {
        public class JSBrigeParameter {
            public string callbackGameObjectName;
            public string callbackFunctionName;
        }

        private void GetInitValue() {
            var callbackParameter = new JSBrigeParameter {
                callbackGameObjectName = "JSMessageReceiver",
                callbackFunctionName = "",
            };
            var parameterJson = JsonUtility.ToJson(callbackParameter);
            JBNativeExecuter executer = new JBNativeExecuter();
            executer.Execute("getInitValue", parameterJson);
            
        }
        private void GetSession() {
            var callbackParameter = new JSBrigeParameter {
                callbackGameObjectName = "JSMessageReceiver",
                callbackFunctionName = "",
            };
            var parameterJson = JsonUtility.ToJson(callbackParameter);
            JBNativeExecuter executer = new JBNativeExecuter();
            executer.Execute("getSession", parameterJson);
        }

        public void Init() { }

        public void Clear() { }

        public void GetInitValue(UnityAction<ParallelInitData> callBack) {
            this.GetInitValue();
            JSMessageReceiver.Instance.GetInitValueReceiver = (json) => {
                JSMessageReceiver.Instance.GetInitValueReceiver = null;
                Debug.Log($"GetInitValue:{json}");
                callBack?.Invoke(JsonUtility.FromJson<ParallelInitData>(json));
            };
        }
        
        public void GetChatRoomSession(UnityAction<ParallelChatRoomSessionDataAll> callBack) {
            this.GetSession();
            JSMessageReceiver.Instance.GetSessionReceiver = (json) => {
                JSMessageReceiver.Instance.GetSessionReceiver = null;
                Debug.Log($"WebGL GetSession:{json}");
                callBack?.Invoke(JsonUtility.FromJson<ParallelChatRoomSessionDataAll>(json));
            };
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
            // TODO
        }
        
        public void GetAppID(UnityAction<Constants.ApplicationID> callBack) {
            // TODO
            callBack?.Invoke((Constants.ApplicationID)0);
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