#if UNITY_EDITOR
using System.IO;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace ParallelDummy {
    public class EnvEditWindow : EditorWindow {
        private static ParallelDummyEnvDataSO _dataSO = null;
        
        [MenuItem("Tools/DummyEnvEditor")]
        private static void ShowEnvEditor() {
            Init();
        }

        private static void Init() {
            if (_dataSO == null) {
                _dataSO = ScriptableObject.CreateInstance<ParallelDummyEnvDataSO>();
            }

            LoadEnv();
            EditorWindow.GetWindow(typeof(EnvEditWindow)).Show();
        }

        private static void LoadEnv() {
            _dataSO.data = Load();
        }

        //スクロール位置
        private Vector2 _scrollPosition = Vector2.zero;

        void OnGUI() {
            if (_dataSO == null) {
                return;
            }

            using (new GUILayout.VerticalScope()) {
                this._scrollPosition = EditorGUILayout.BeginScrollView(this._scrollPosition);
                EditorGUILayout.LabelField(ParallelDummyEnvProvider.Path());
                EditorGUILayout.Space(4);
                ScriptableObject target = _dataSO;
                SerializedObject so = new SerializedObject(target);
                SerializedProperty envDataProperty = so.FindProperty("data");
                EditorGUILayout.PropertyField(envDataProperty, true);
                so.ApplyModifiedProperties();
                EditorGUILayout.EndScrollView();
            }

            if (GUILayout.Button("Save")) {
                Save(_dataSO.data);
            }
            if (GUILayout.Button("LeaveRoom")) {
                LeaveRoom();
            }
        }

        public static void Save(ParallelDummyEnvData envData) {
            var serializeText = JsonUtility.ToJson(envData);
            File.WriteAllText(ParallelDummyEnvProvider.Path(), serializeText);
            AssetDatabase.ImportAsset(ParallelDummyEnvProvider.Path());
        }
        
        public static void LeaveRoom() {
            ParallelDummyAPIs apis = new ParallelDummyAPIs();
            ParallelCommon.IParallelNetwork parallelDummyNetwork = new ParallelDummyNetwork();
            parallelDummyNetwork.ChatGroupID = _dataSO.data.chat_group_id;
            parallelDummyNetwork.ChatGroupRoomID = _dataSO.data.chat_group_roomID;
            parallelDummyNetwork.ChatGroupRoomSessionID = _dataSO.data.chat_group_roomSessionID;
            parallelDummyNetwork.Init(_dataSO.data.server_url, _dataSO.data.SelectAccount.token, _dataSO.data.SelectAccount.device_id);
            apis.LeaveRoom(parallelDummyNetwork, () => {
                
            });
        }
        
        public static ParallelDummyEnvData Load() {
            if (!File.Exists(ParallelDummyEnvProvider.Path())) {
                Save(new ParallelDummyEnvData());
            }

            string text = "";
            text = File.ReadAllText(ParallelDummyEnvProvider.Path());
            if (!string.IsNullOrEmpty(text)) {
                return JsonUtility.FromJson<ParallelDummyEnvData>(text);
            }

            return null;
        }
    }
}
#endif
