using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

public static class KaitAppIconSettings
{
    public const string PortraitPath = "Assets/Art/AppIcon/KaitAppIcon.png";
    private const string BackgroundPath = "Assets/Art/AppIcon/AdaptiveMint.asset";

    [MenuItem("Kait/Apply Kait App Icon")]
    public static void Apply()
    {
        AssetDatabase.ImportAsset(PortraitPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(PortraitPath) as TextureImporter;
        if (importer == null) throw new BuildFailedException("Missing Kait app portrait");
        importer.textureType = TextureImporterType.Default;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048;
        importer.mipmapEnabled = false;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
        var portrait = AssetDatabase.LoadAssetAtPath<Texture2D>(PortraitPath);
        if (portrait == null || portrait.width != portrait.height)
            throw new BuildFailedException("Kait icon must be a square texture");
        var background = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
        if (background == null)
        {
            // A code-native solid background, visible only during launcher parallax.
            background = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var mint = new Color32(117, 223, 177, 255);
            background.SetPixels32(new[] { mint, mint, mint, mint }); background.Apply();
            background.name = "Kait Adaptive Mint";
            AssetDatabase.CreateAsset(background, BackgroundPath);
        }
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { portrait }, IconKind.Any);
        foreach (var kind in new[] { AndroidPlatformIconKind.Legacy, AndroidPlatformIconKind.Round, AndroidPlatformIconKind.Adaptive })
        {
            var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            foreach (var icon in icons)
            {
                if (kind == AndroidPlatformIconKind.Adaptive) icon.SetTextures(background, portrait);
                else icon.SetTexture(portrait, 0);
            }
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
            Debug.Log($"Kait icon configured: {kind}, slots={icons.Length}");
        }
        AssetDatabase.SaveAssets();
    }
}

public sealed class KaitAdaptiveIconInset : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 11000;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string root = Path.Combine(Directory.GetParent(path).FullName, "launcher", "src", "main", "res");
        if (!Directory.Exists(root)) throw new BuildFailedException("Android launcher resources not found: " + root);
        int count = 0;
        foreach (string file in Directory.GetFiles(root, "*.xml", SearchOption.AllDirectories))
        {
            var document = XDocument.Load(file);
            if (!InsetForeground(document)) continue;
            document.Save(file); count++;
        }
        if (count == 0) throw new BuildFailedException("No adaptive launcher icon found; refusing a build with an unchecked icon");
        Debug.Log($"Kait adaptive icon safe-area inset applied: {count} resources");
    }

    public static bool InsetForeground(XDocument document)
    {
        if (document.Root?.Name.LocalName != "adaptive-icon") return false;
        XNamespace android = "http://schemas.android.com/apk/res/android";
        var foreground = document.Root.Element("foreground");
        var drawable = foreground?.Attribute(android + "drawable");
        if (drawable == null) return false;
        string resource = drawable.Value; drawable.Remove();
        // Android displays the central 72/108 region. Fit the full portrait into it.
        foreground.Add(new XElement("inset", new XAttribute(android + "drawable", resource),
            new XAttribute(android + "inset", "16.6667%")));
        return true;
    }
}
