using UnityEditor;
using UnityEngine;

public sealed class KaitShatterImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (assetPath != "Assets/Resources/KaitVisuals/Effects/WhiteGoldShatter.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/WhiteGoldSlashNormal.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/WhiteGoldSlashFinisher.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/WhiteGoldPush.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/KaitHurtB.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/ArrowImpactA.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/MageImpactA.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/LandingDustA.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/BoundaryDustA.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/SpeedBuffB.png" &&
            assetPath != "Assets/Resources/KaitVisuals/Effects/DreadSlashA.png") return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
    }
}
