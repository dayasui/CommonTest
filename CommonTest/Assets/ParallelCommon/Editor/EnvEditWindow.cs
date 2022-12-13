#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ParallelCommon {

    public class EnvEditWindow : EditorWindow {
        //[SerializeField]
        private static ParallelUnityEnvDataSO _envDataSO = null;
        [MenuItem("Tools/EnvEditor")]
        private static void ShowDevice() {
            Init();
        }


        private static void Init() {
            if (_envDataSO == null) {
                _envDataSO = ScriptableObject.CreateInstance<ParallelUnityEnvDataSO>();
            }

            LoadEnv();
            EditorWindow.GetWindow(typeof(EnvEditWindow)).Show();
        }

        private static void LoadEnv() {
            _envDataSO.envData = EnvProvider.LoadParallelUnityEnv();
        }

        //スクロール位置
        private Vector2 _scrollPosition = Vector2.zero;

        void OnGUI() {
            if (_envDataSO == null) {
                return;
            }


            using (new GUILayout.VerticalScope()) {
                this._scrollPosition = EditorGUILayout.BeginScrollView(this._scrollPosition);
                EditorGUILayout.LabelField(EnvProvider.GetParallelUnityEnvFilePath());
                EditorGUILayout.Space(4);
                ScriptableObject target = _envDataSO;
                SerializedObject so = new SerializedObject(target);
                SerializedProperty envDataProperty = so.FindProperty("envData");
                EditorGUILayout.PropertyField(envDataProperty, true);
                so.ApplyModifiedProperties();
                EditorGUILayout.EndScrollView();
            }

            if (GUILayout.Button("Save")) {
                EnvProvider.SaveParallelUnityEnv(_envDataSO.envData);
            }
        }
    }
}
#endif