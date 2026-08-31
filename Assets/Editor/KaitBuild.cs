using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class KaitBuild
{
    [MenuItem("Kait/Build Windows Demo")]
    public static void BuildWindowsDemo()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string output = Path.Combine(projectRoot, "Build", "kait.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Scene.unity" },
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Kait Windows build failed: " + report.summary.result);

        Debug.Log("Kait build created: " + output);
    }

    [MenuItem("Kait/Build Android Demo")]
    public static void BuildAndroidDemo()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string output = Path.Combine(projectRoot, "Build", "kait-v0.3.7.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(output));

        PlayerSettings.productName = "Kait";
        PlayerSettings.bundleVersion = "0.3.7";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.kaitprototype.demo");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Scene.unity" },
            locationPathName = output,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Kait Android build failed: " + report.summary.result);

        Debug.Log("Kait Android build created: " + output);
    }
}
