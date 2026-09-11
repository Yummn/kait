using UnityEditor;
using UnityEngine;

public sealed class YummnSnowArtSettings : AssetPostprocessor
{
    public override uint GetVersion() => 2;
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Resources/" + YummnSnowCourtyard.ResourceRoot)) return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        // These large painted ground textures are minified heavily at 1080p.
        // Keep text/character imports untouched; only scene art gets mip filtering.
        importer.mipmapEnabled = true;
        importer.alphaIsTransparency = !assetPath.EndsWith("SnowMask.png");
        importer.sRGBTexture = !assetPath.EndsWith("SnowMask.png");
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.filterMode = FilterMode.Trilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
