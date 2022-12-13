using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ParallelCommon {
    
    [System.Serializable]
    public class EnvData {
        public bool isDebug;
        public string AppIdPUN;
    }
    public static class EnvProvider {

        private static readonly string PARALLEL_UNITY_ENV = "parallel-unity-env";
        private static readonly string ENV_STAGING = "env-staging";
        private static readonly string ENV_PRODUCTION = "env-production";
        private static readonly string ENV_DEVELOPMENT = "env-development";
        private static readonly string ENV_DIR = "";

        
        public static string GetParallelUnityEnvFilePath() {
            return Path.Combine("Assets/ParallelCommon/Resources/Env", $"{PARALLEL_UNITY_ENV}.json");
        }


#if UNITY_EDITOR
        public static void SaveParallelUnityEnv(ParallelUnityEnvData envData) {
            SaveParallelUnityEnv(JsonUtility.ToJson(envData));
        }

        public static void SaveParallelUnityEnv(string serializeText) {
            string filepath = GetParallelUnityEnvFilePath();
            File.WriteAllText(filepath, serializeText);
            AssetDatabase.ImportAsset(filepath);
        }

        public static ParallelUnityEnvData LoadParallelUnityEnv() {
            string filepath = GetParallelUnityEnvFilePath();
            if (!File.Exists(filepath)) {
                SaveParallelUnityEnv(new ParallelUnityEnvData());
            }

            string text = "";
            text = File.ReadAllText(filepath);
            if (!string.IsNullOrEmpty(text)) {
                return JsonUtility.FromJson<ParallelUnityEnvData>(text);
            }

            return null;
        }
#else
    public static void SaveParallelUnityEnv(ParallelUnityEnvData envData, ParallelUnityEnvType type) {
    }
    public static ParallelUnityEnvData LoadParallelUnityEnv(ParallelUnityEnvType type = ParallelUnityEnvType.None) {
        if (type == ParallelUnityEnvType.None) {
            type = ParallelUnityEnvType.Device;
        }
        string filepath = $"Env/{PARALLEL_UNITY_ENV}";
        string text = "";
        text = Resources.Load<TextAsset>(filepath).text;

        if (!string.IsNullOrEmpty(text)) {
            return JsonUtility.FromJson<ParallelUnityEnvData>(text);
        }
        return null;
    }

#endif

        //    public static EnvData LoadEnv() {
        //#if UNITY_EDITOR
        //        if (!File.Exists(FilePath)) {
        //            SaveEnv(new EnvData());
        //        }
        //#endif
        //        var ta = Resources.Load<TextAsset>("Env/env");
        //        if(ta != null) {
        //            return JsonUtility.FromJson<EnvData>(ta.text);
        //        } else {
        //            return null;
        //        }
        //    }

        public static EnvData LoadEnv(string build_env) {
            string filepath = string.Empty;
            // イレギュラー時には production を読み込む。最悪大事故は防げる
            if (string.IsNullOrEmpty(build_env)) {
                filepath = Path.Combine("Env", ENV_PRODUCTION);
            } else {
                if (build_env.Equals("production")) {
                    filepath = Path.Combine("Env", ENV_PRODUCTION);
                } else if (build_env.Equals("staging")) {
                    filepath = Path.Combine("Env", ENV_STAGING);
                } else if (build_env.Equals("development")) {
                    filepath = Path.Combine("Env", ENV_DEVELOPMENT);
                } else {
                    filepath = Path.Combine("Env", ENV_PRODUCTION);
                }
            }

            if (string.IsNullOrEmpty(filepath)) {
                Debug.LogError("Missing Env File!!");
                return null;
            }

            Debug.Log("env path:" + filepath);
            var text = Resources.Load<TextAsset>(filepath).text;
            if (!string.IsNullOrEmpty(text)) {
                return JsonUtility.FromJson<EnvData>(text);
            } else {
                Debug.LogError("Env Error!!");
                return null;
            }
        }
    }
}
