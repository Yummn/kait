using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Editor-only bridge. VibeGame never becomes a dependency of the shipped game.
public static class KaitVibeGameTools
{
    public const string ImportRoot = "Assets/Art/VibeGame";
    public static string ToolRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "../Tools/VibeGame"));
    public static string StagingRoot => Path.Combine(ToolRoot, "workspace/output");

    [MenuItem("Kait/VibeGame/使用说明")]
    public static void OpenGuide() => EditorUtility.OpenWithDefaultApp(Path.Combine(ToolRoot, "README.md"));

    [MenuItem("Kait/VibeGame/功能测试报告")]
    public static void OpenReport() => EditorUtility.OpenWithDefaultApp(Path.Combine(ToolRoot, "TEST_REPORT.md"));

    [MenuItem("Kait/VibeGame/打开素材暂存目录")]
    public static void OpenStaging()
    {
        Directory.CreateDirectory(StagingRoot);
        EditorUtility.RevealInFinder(StagingRoot);
    }

    [MenuItem("Kait/VibeGame/导入已确认的图片")]
    public static void ChooseImage()
    {
        Directory.CreateDirectory(StagingRoot);
        string source = EditorUtility.OpenFilePanel("选择已确认的 VibeGame 图片", StagingRoot, "png,jpg,jpeg");
        if (string.IsNullOrEmpty(source)) return;
        try
        {
            string assetPath = ImportImage(source);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        catch (Exception error)
        {
            EditorUtility.DisplayDialog("未导入", error.Message, "确定");
        }
    }

    public static string ImportImage(string source)
    {
        string full = Path.GetFullPath(source);
        string allowed = Path.GetFullPath(StagingRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(allowed, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("请先把图片放到 Tools/VibeGame/workspace/output，再确认导入。");
        string extension = Path.GetExtension(full).ToLowerInvariant();
        if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
            throw new InvalidOperationException("当前只导入 PNG/JPG。图集需在 Sprite Editor 中切片，不会自动覆盖已有动画。");
        var probe = new Texture2D(2, 2);
        try
        {
            if (!ImageConversion.LoadImage(probe, File.ReadAllBytes(full)))
                throw new InvalidDataException("图片解码失败。");
        }
        finally { UnityEngine.Object.DestroyImmediate(probe); }
        string target = ImportRoot + "/" + Path.GetFileName(full);
        Directory.CreateDirectory(ImportRoot);
        if (File.Exists(target)) throw new IOException("同名素材已存在，已取消导入以保护旧素材。请先给新素材改名。");
        File.Copy(full, target, false);
        AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
        return target;
    }
}

public sealed class KaitVibeGameImageImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(KaitVibeGameTools.ImportRoot + "/", StringComparison.Ordinal)) return;
        // Apply defaults only on first import; preserve later edits in the Inspector.
        if (!assetImporter.importSettingsMissing) return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Point;
        importer.maxTextureSize = 8192;
    }
}
