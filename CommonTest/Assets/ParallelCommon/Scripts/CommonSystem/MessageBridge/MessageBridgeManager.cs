using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using ParallelCommon;
using UnityEngine.Events;

namespace ParallelCommon {

    public class MessageBridgeManager : SingletonMonoBehaviour<MessageBridgeManager> {
        private IMessageBridge _messageBridge;
        protected override void Awake() {
            
#if UNITY_IOS
            this._messageBridge = new MessageBridgeIOS();
#elif UNITY_ANDROID
            this._messageBridge = new MessageBridgeAndroid();
#elif UNITY_WEBGL
            this._messageBridge = new MessageBridgeWebGL();
#else
            this._messageBridge = new MessageBridge();
#endif
            this._messageBridge?.Init();
            base.Awake();
        }

        private void OnDestroy() {
            this._messageBridge?.Clear();
        }

        public void GetInitValue(UnityAction<ParallelInitData> callBack) => this._messageBridge.GetInitValue(callBack);
        public void GetChatRoomSession(UnityAction<ParallelChatRoomSessionDataAll> callBack) =>
            this._messageBridge.GetChatRoomSession(callBack);

        public void SendLog(string eventName, string key, string value) =>
            this._messageBridge.SendLog(eventName, key, value);

        public void GetAppID(UnityAction<Constants.ApplicationID> callBack) => this._messageBridge.GetAppID(callBack);
        public void ResetSession() => this._messageBridge.ResetSession();
        public void ShowHeader() => this._messageBridge.ShowHeader();
        public void HideHeader() => this._messageBridge.HideHeader();
        public void ShowFooter() => this._messageBridge.ShowFooter();
        public void HideFooter() => this._messageBridge.HideFooter();
        public void ShowNextContentList() => this._messageBridge.ShowNextContentList();
        public void HideNextContentList() => this._messageBridge.HideNextContentList();
        public void OpenWebView(string url) => this._messageBridge.OpenWebView(url);
        public void CloseWebView() => this._messageBridge.CloseWebView();
    }
}
