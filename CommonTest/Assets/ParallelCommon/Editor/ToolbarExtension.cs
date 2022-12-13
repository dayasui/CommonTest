using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using UnityEditor.SceneManagement;

namespace ParallelCommon.Editor {

    [InitializeOnLoad]
    public static class ToolbarExtension {
        static ToolbarExtension() {
            // 再生ボタンの左側に GUI を追加する
            ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);

            // 再生ボタンの右側に GUI を追加する
            ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
        }

        // 再生ボタンの左側に GUI を追加する
        private static void OnLeftToolbarGUI() {
            // GUI を詰めて表示するために FlexibleSpace を呼び出しておく
            GUILayout.FlexibleSpace();

            // Save Project を実行するボタンを表示する
            if (IconButton("SaveActive")) {
                EditorApplication.ExecuteMenuItem("File/Save Project");
            }

            // Project Settings を開くボタンを表示する
            if (IconButton("EditorSettings Icon")) {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            }

            // 空のゲームオブジェクトを作成するボタンを表示する
            if (IconButton("GameObject Icon")) {
                EditorApplication.ExecuteMenuItem("GameObject/Create Empty Child");
            }

            // 新しいフォルダを作成するボタンを表示する
            if (IconButton("Folder Icon")) {
                EditorApplication.ExecuteMenuItem("Assets/Create/Folder");
            }

            // 新しいフォルダを作成するボタンを表示する
            if (IconButton("UnityEditor.GameView")) {
                if (!EditorApplication.isPlaying) {
                    PlayScene();
                } else {
                    EditorApplication.isPlaying = false;
                }
            }

#if UNITY_2019_1_OR_NEWER

            // Unity 2019 だと上記のボタンが再生ボタンと重なってしまうため
            // 少し間隔を開けるために GUILayout.Space を呼び出す
            GUILayout.Space(20);
#endif
        }

        // 再生ボタンの右側に GUI を追加する
        private static void OnRightToolbarGUI() {
            // Inspector をロック / アンロックするボタンを表示する
            if (TextButton("Lock")) {
                var tracker = ActiveEditorTracker.sharedTracker;
                tracker.isLocked = !tracker.isLocked;
                tracker.ForceRebuild();
            }

            // Inspector の Debug モードの ON / OFF を切り替えるボタンを表示する
            if (TextButton("Debug")) {
                var window = Resources.FindObjectsOfTypeAll<EditorWindow>();
                var inspectorWindow = ArrayUtility.Find(window, c => c.GetType().Name == "InspectorWindow");

                if (inspectorWindow == null) return;

                var inspectorType = inspectorWindow.GetType();
                var tracker = ActiveEditorTracker.sharedTracker;
                var isNormal = tracker.inspectorMode == InspectorMode.Normal;
                var methodName = isNormal ? "SetDebug" : "SetNormal";

                var attr = BindingFlags.NonPublic | BindingFlags.Instance;
                var methodInfo = inspectorType.GetMethod(methodName, attr);
                methodInfo.Invoke(inspectorWindow, null);
                tracker.ForceRebuild();
            }

            GUILayout.FlexibleSpace();
        }

        // アイコン付きのボタンを表示する
        private static bool IconButton(string name) {
            var content = EditorGUIUtility.IconContent(name);
            return GUILayout.Button(content, GUILayout.Width(32), GUILayout.Height(22));
        }

        // テキスト付きのボタンを表示する
        private static bool TextButton(string text) {
            return GUILayout.Button(text, GUILayout.Height(22));
        }

        /// <summary>
        /// Scene実行処理
        /// </summary>
        private static void PlayScene() {
            string scenePath = EditorBuildSettings.scenes[0].path;
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            if (sceneAsset == null) {
                Debug.Log($"{scenePath} シーンアセットが存在しません");
                return;
            }

            EditorSceneManager.playModeStartScene = sceneAsset;

            EditorApplication.isPlaying = true;
        }
    }
}