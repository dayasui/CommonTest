#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Debug = UnityEngine.Debug;

public static class BuildManager {
    private const string SceneDir
        = "ParallelEntry";
    private const string ExistsScene = "ParallelDummy/ParallelDummyScene.unity";
    
    private const string StagingProfileName = "StagingProfile";
    private const string ProductionProfileName = "ProductionProfile";

    public static void UnityBuild(bool isProductionBuild) {
        BuildCore(isProductionBuild);
    }
    
    [MenuItem("Tools/Export/Staging")]
    private static void BuildStaging() {
        BuildCore(false);
    }

    [MenuItem("Tools/Export/Production")]
    private static void BuildProduction() {
        BuildCore(true);
    }
    
    private static void BuildCore(bool isProductionBuild) {
        string sceneDirAssetsPath = GetAssetsPath(SceneDir);
        string existsScene = GetAssetsPath(ExistsScene);
        
        List<EditorBuildSettingsScene> defaultScenes = new List<EditorBuildSettingsScene> (EditorBuildSettings.scenes);
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene> (EditorBuildSettings.scenes);
        EditorBuildSettingsScene tmpScene = null;
        if (isProductionBuild) {
            foreach (EditorBuildSettingsScene scene in scenes) {
                if (scene.path.Contains("ParallelDummyScene")) {
                    tmpScene = scene;
                    scene.enabled = false;
                    break;
                }
            }
            EditorBuildSettings.scenes = scenes.ToArray ();
        }
#if UNITY_IOS
        Build.DoBuildIOS(() => {
            Debug.Log("Build Complete");
            envData.isDebug = isDebug;
            envData.isStandAlone = isStandAlone;
            EnvProvider.SaveEnv(envData, EnvProvider.EnvType.Device);
            EditorBuildSettings.scenes = defaultScenes.ToArray ();
        });
#elif UNITY_ANDROID
        Build.DoBuildAndroidLibrary(() => {
            Debug.Log("Build Complete");
            envData.isDebug = isDebug;
            envData.isStandAlone = isStandAlone;
            EnvProvider.SaveEnv(envData, EnvProvider.EnvType.Device);
            EditorBuildSettings.scenes = defaultScenes.ToArray ();
        });
#elif UNITY_WEBGL
        Build.DoBuildWebGL(() => {
            
        });
#endif
    }

    private static string GetFullPath(string path) {
        return Application.dataPath + "/" + path;
    }

    private static string GetAssetsPath(string path) {
        return "Assets/" + path;
    }


    [MenuItem("Tools/Archive/Staging（UnityFrameworkを作るので時間がかかります）")]
    private static void BuildAndArchiveStaging() {
        BuildCore(false);

        string path = Application.dataPath + "/../../tools/unityframeworkbuild.sh";
        ExecuteProccess.StartProcess(path, "");
    }

    [MenuItem("Tools/Archive/Production（UnityFrameworkを作るので時間がかかります）")]
    private static void BuildAndArchiveProduction() {
        
        BuildCore(true);

        string path = Application.dataPath + "/../../tools/unityframeworkbuild.sh";
        ExecuteProccess.StartProcess(path, "");
    }
}
#endif