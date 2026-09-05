using UnityEditor;
using UnityEngine;

public sealed class KaitTutorialImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Resources/KaitVisuals/Tutorial/")) return;
        var importer=(TextureImporter)assetImporter;
        importer.textureType=TextureImporterType.Default;
        importer.maxTextureSize=2048;
        importer.npotScale=TextureImporterNPOTScale.None;
        importer.mipmapEnabled=false;
        importer.filterMode=FilterMode.Bilinear;
        importer.wrapMode=TextureWrapMode.Clamp;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
    }
}
