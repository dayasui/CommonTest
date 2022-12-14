using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ParallelDummy {
    public class ParallelDummyEnvDataSO : ScriptableObject {
        public ParallelDummyEnvData data;
    }

    [System.Serializable]
    public class ParallelDummyEnvData {
        public bool is_observer;
        public int account_index;
        public string server_url;
        public ParallelAccountData[] accounts;
        
        public int owner_user_id;
        public int chat_group_id;
        public int chat_group_roomID;
        public int chat_group_roomSessionID;
        public int chat_group_roomSessionUserID;
        public ParallelAccountData SelectAccount => this.accounts[this.account_index];
    }

    [System.Serializable]
    public class ParallelAccountData {
        public int id;
        public string name;
        public string token;
        public string device_id;
    }
    
    public static class ParallelDummyEnvProvider {
        public static string Path() {
            return "Assets/ParallelDummy/Resources/parallel-dummy-env.json";
        }


#if UNITY_EDITOR
        public static void Save(ParallelDummyEnvData data) {
            Save(JsonUtility.ToJson(data));
        }

        public static void Save(string serializeText) {
            string filepath = Path();
            File.WriteAllText(filepath, serializeText);
            AssetDatabase.ImportAsset(filepath);
        }

        public static ParallelDummyEnvData Load() {
            string filepath = Path();
            if (!File.Exists(filepath)) {
                Save(new ParallelDummyEnvData());
            }

            string text = "";
            text = File.ReadAllText(filepath);
            if (!string.IsNullOrEmpty(text)) {
                return JsonUtility.FromJson<ParallelDummyEnvData>(text);
            }

            return null;
        }
#endif
    }
}
