using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ParallelCommon {
    public class JSMessageReceiver : SingletonMonoBehaviour<JSMessageReceiver> {
        public UnityAction<string> GetInitValueReceiver { set; get; } = null;
        public UnityAction<string> GetSessionReceiver { set; get; } = null;

        public void GetInitValueCallback(string data) {
            this.GetInitValueReceiver?.Invoke(data);
        }

        public void GetSessionCallback(string data) {
            this.GetSessionReceiver?.Invoke(data);
        }


        public void Log(string str) {
            Debug.Log("DebugJS:" + str);
        }
    }

}