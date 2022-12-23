using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;

namespace ParallelCommon {
    public class JSBridge {
        public UnityAction<string> callback;

    }


    public class JSBridgeWrapper : MonoBehaviour {
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string getCookies();

    [DllImport("__Internal")]
    private static extern void postMessage();

    [DllImport("__Internal")]
    private static extern void getInitValue();
#endif

        public static Dictionary<string, string> GetCookies() {
            Dictionary<string, string> cookiesDict = new Dictionary<string, string>();

#if UNITY_WEBGL && !UNITY_EDITOR
            string cookiesStr = getCookies();

            Debug.Log($"getCookies raw value:{cookiesStr}");

            string[] cookies = cookiesStr.Split(';');
            foreach (var cookie in cookies) {
                string[] keyValue = cookie.Split('=');
                if (keyValue.Length == 2) {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();
                    cookiesDict.Add(key, value);
                }
            }

#else
            Debug.Log($"call GetCookies in Editor");
#endif
            return cookiesDict;
        }

        public static void PostMessageTest() {
#if UNITY_WEBGL && !UNITY_EDITOR
            postMessage();
#else
            Debug.Log($"PostMessageTest");
#endif
        }

        public static void GetInitValue() {
#if UNITY_WEBGL && !UNITY_EDITOR
            getInitValue();
#else
            Debug.Log($"call GetInitValue in Editor");
#endif
        }
    }

}