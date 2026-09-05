using UnityEditor;
using UnityEngine;

public sealed class KaitSunlitImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Resources/KaitVisuals/SunlitCourtyard/") &&
            !assetPath.StartsWith("Assets/Resources/KaitVisuals/EmeraldCourtyard/")) return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = assetPath.EndsWith("Garden.png") || assetPath.EndsWith("CanopyMask.png")
            || assetPath.EndsWith("GrassBase.png") || assetPath.EndsWith("TreeCanopy.png") ? 2048 : 1024;
        importer.alphaIsTransparency = true;
    }
}
