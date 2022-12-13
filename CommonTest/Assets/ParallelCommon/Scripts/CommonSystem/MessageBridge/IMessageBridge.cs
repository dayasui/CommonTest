using UnityEngine.Events;

namespace ParallelCommon {
    public interface IMessageBridge {
        void Init();
        void Clear();
        void GetInitValue(UnityAction<ParallelInitData> callBack);
        void GetChatRoomSession(UnityAction<ParallelChatRoomSessionDataAll> callBack);
        void SendLog(string eventName, string key, string value);
        void GetAppID(UnityAction<Constants.ApplicationID> callBack);
        void ResetSession();
        void ShowHeader();
        void HideHeader();
        void ShowFooter();
        void HideFooter();
        void ShowNextContentList();
        void HideNextContentList();
        void OpenWebView(string url);
        void CloseWebView();
    }
}