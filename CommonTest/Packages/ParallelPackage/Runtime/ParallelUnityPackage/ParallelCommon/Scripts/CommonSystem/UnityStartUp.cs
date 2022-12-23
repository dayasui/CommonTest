#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace ParallelCommon {
    [InitializeOnLoad]
    public static class UnityStartup {
        private class StartUpData : ScriptableSingleton<StartUpData> {
            [SerializeField]
            private int _callCount;

            public bool IsStartUp() {
                return _callCount++ == 0;
            }
        }

        static UnityStartup() {
            if (!StartUpData.instance.IsStartUp()) return;

            //シェルスクリプトの実行
            string path = "../tools/update-env.sh";
            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = path;
            process.Start();

            //結果待ち
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            process.Close();

            //ラベルに表示
            UnityEngine.Debug.Log("StartUp " + output);
            // UnityEditorの起動時に行いたい処理を記述する
        }
    }
}
#endif