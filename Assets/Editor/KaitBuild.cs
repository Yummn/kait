using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Text;

public static class KaitBuild
{
    [MenuItem("Kait/Build Windows Demo")]
    public static void BuildWindowsDemo()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string output = Path.Combine(projectRoot, "Build", "kait.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));

        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.defaultIsNativeResolution = true;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.resizableWindow = false;

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
        string output = Path.Combine(projectRoot, "Build", "kait-v0.4.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(output));

        PlayerSettings.productName = "Kait";
        PlayerSettings.bundleVersion = "0.4";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.kaitprototype.demo");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.Activity;

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

/// <summary>
/// App UI 1.3.6 rewrites the Android launcher to AppUIActivity during its
/// preprocess callback, but that Java class is not retained in Unity 6's
/// generated Gradle project. Run after package preprocessors and restore the
/// standard Unity activity that is always present in unity-classes.jar.
/// </summary>
public sealed class KaitAndroidLaunchActivityGuard : IPreprocessBuildWithReport
{
    private const string ManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";

    private const string StandardManifest = @"<?xml version=""1.0"" encoding=""utf-8""?>
<manifest xmlns:android=""http://schemas.android.com/apk/res/android""
          xmlns:tools=""http://schemas.android.com/tools""
          package=""com.unity3d.player"">
  <application android:usesCleartextTraffic=""false"" android:allowBackup=""false"">
    <activity android:name=""com.unity3d.player.UnityPlayerActivity""
              android:theme=""@style/UnityThemeSelector""
              android:exported=""true""
              android:configChanges=""fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen"">
      <intent-filter>
        <action android:name=""android.intent.action.MAIN"" />
        <category android:name=""android.intent.category.LAUNCHER"" />
      </intent-filter>
      <meta-data android:name=""unityplayer.UnityActivity"" android:value=""true"" />
    </activity>
    <activity android:name=""com.unity3d.player.appui.AppUIActivity"" tools:node=""remove"" />
    <activity android:name=""com.unity3d.player.appui.AppUIGameActivity"" tools:node=""remove"" />
  </application>
  <uses-permission android:name=""android.permission.VIBRATE"" />
</manifest>
";

    public int callbackOrder => 10000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android) return;
        WriteStandardManifest();
    }

    internal static void WriteStandardManifest()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string absolutePath = Path.Combine(projectRoot, ManifestPath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
        File.WriteAllText(absolutePath, StandardManifest, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(ManifestPath, ImportAssetOptions.ForceUpdate);
        Debug.Log("Kait Android launcher fixed to UnityPlayerActivity.");
    }
}
