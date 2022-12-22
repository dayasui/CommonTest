#if UNITY_EDITOR
using System.Diagnostics;

public class ExecuteProccess {
    /// <summary>
    /// proccess起動
    /// </summary>
    public static void StartProcess(string path, string arg) {
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \" {path} {arg} \"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        //    process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        UnityEngine.Debug.Log(process.StartInfo.Arguments);

        //結果待ち
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Close();

        UnityEngine.Debug.Log($"result:{output}");
    }
}
#endif