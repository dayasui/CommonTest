using UnityEngine;

namespace ParallelCommon {
    [System.Serializable]
    public class JBNetworkParameter {
        public int data;
        public string callbackGameObjectName;
        public string callbackFunctionName;
    }

    public class JBNativeExecuter {

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void execute(string methodName, string parameter);
#endif

        public void Execute(string methodName, string parameter = "{}") {
#if UNITY_WEBGL && !UNITY_EDITOR
            execute(methodName, parameter);
#else
            Debug.Log($"call native method: {methodName}, parameter: {parameter}");
#endif
        }
    }

}