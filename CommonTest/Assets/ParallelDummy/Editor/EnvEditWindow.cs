#if UNITY_EDITOR
using System.IO;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace ParallelDummy {
    public class EnvEditWindow : EditorWindow {
        private static string Path => "Assets/ParallelDummy/Resources/parallel-dummy-env.json";

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
                EditorGUILayout.LabelField(Path);
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
        }

        public static void Save(ParallelDummyEnvData envData) {
            var serializeText = JsonUtility.ToJson(envData);
            File.WriteAllText(Path, serializeText);
            AssetDatabase.ImportAsset(Path);
        }
        
        public static ParallelDummyEnvData Load() {
            if (!File.Exists(Path)) {
                Save(new ParallelDummyEnvData());
            }

            string text = "";
            text = File.ReadAllText(Path);
            if (!string.IsNullOrEmpty(text)) {
                return JsonUtility.FromJson<ParallelDummyEnvData>(text);
            }

            return null;
        }
    }
}
#endif
