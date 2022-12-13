using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

namespace ParallelCommon {
    public sealed class MessageBridgeAndroid : IMessageBridge {
        public void Init() {
            UnityMessageManager.Instance.OnMessage += this.OnMessage;
            UnityMessageManager.Instance.OnRNMessage += this.OnRNMessage;
        }

        public void Clear() {
            UnityMessageManager.Instance.OnMessage -= this.OnMessage;
            UnityMessageManager.Instance.OnRNMessage -= this.OnRNMessage;
        }
        
        private void SendMessageWrapper<T>(string name, JObject data, UnityAction<T> callBack) {
            Debug.Log("SendMessageWrapper name:" + name + " data:" + data);
            UnityMessageManager.Instance.SendMessageToRN(new UnityMessage() {
                name = name,
                data = data,
                callBack = (res) =>
                {
                    JValue jv = res as JValue;
                    callBack?.Invoke((T)jv.Value);
                }
            });
        }
        
        private void OnMessage(string message) {}
        private void OnRNMessage(MessageHandler handler) {}
        public void GetInitValue(UnityAction<ParallelInitData> callBack) {
            this.SendMessageWrapper<string>("getInitValue", null, (json) => {
                callBack?.Invoke(JsonUtility.FromJson<ParallelInitData>(json));
            });
        }
        
        public void GetChatRoomSession(UnityAction<ParallelChatRoomSessionDataAll> callBack) {
            this.SendMessageWrapper<string>("getSession", null, (json) => {
                callBack?.Invoke(JsonUtility.FromJson<ParallelChatRoomSessionDataAll>(json));
            });
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
            JObject jObj = new JObject();
            jObj.Add(key, value);
            this.SendMessageWrapper<string>(eventName, jObj, (json) => {
                // 送るだけなので特にない
            });
        }
        
        public void GetAppID(UnityAction<Constants.ApplicationID> callBack) {
            this.SendMessageWrapper<string>("getAppID", null, (json) => {
                JSONObject jsonObj = new JSONObject(json);
                int appID = (int)jsonObj.GetField("app_id").i;
                callBack?.Invoke((Constants.ApplicationID)appID);
            });
        }
        
        public void ResetSession() {
            this.SendMessageWrapper<string>("resetSession", null, (json) => {
                Debug.Log("resetSession:"); 
            });
        }
        
        public void ShowHeader() {
            this.SendMessageWrapper<string>("showHeader", null, (json) => { });
        }

        public void HideHeader() {
            this.SendMessageWrapper<string>("hideHeader", null, (json) => { });
        }

        public void ShowFooter() {
            this.SendMessageWrapper<string>("showFooter", null, (json) => { });
        }

        public void HideFooter() {
            this.SendMessageWrapper<string>("hideFooter", null, (json) => { });
        }

        public void ShowNextContentList() {
            this.SendMessageWrapper<string>("showNextContentList", null, (json) => { });
        }
    
        public void HideNextContentList() {
            this.SendMessageWrapper<string>("hideNextContentList", null, (json) => { });
        }

        public void OpenWebView(string url) {
            JObject jObj = new JObject();
            jObj.Add("url", url);
            this.SendMessageWrapper<string>("openWebView", jObj, (json) => {
                // 送るだけなので特にない
            });
        }
    
        public void CloseWebView() {
            this.SendMessageWrapper<string>("closeWebView", null, (json) => { });
        }
    }
}
