using UnityEngine.Events;

namespace ParallelCommon {

    public class MessageBridge : IMessageBridge {
        public void Init() { }
        public void Clear() { }

        public void GetInitValue(UnityAction<ParallelInitData> callBack) {
            callBack?.Invoke(new ParallelInitData());
        }

        public void GetChatRoomSession(UnityAction<ParallelChatRoomSessionDataAll> callBack) {
            callBack?.Invoke(new ParallelChatRoomSessionDataAll());
        }
        public void SendLog(string eventName, string key, string value) { }

        public void GetAppID(UnityAction<Constants.ApplicationID> callBack) {
            callBack?.Invoke((Constants.ApplicationID)0);
        }
        public void ResetSession() { }
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
