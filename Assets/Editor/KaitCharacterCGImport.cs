using UnityEditor;
using UnityEngine;

public sealed class KaitCharacterCGImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/Resources/KaitVisuals/CharacterSelection/"))return;
        var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.npotScale=TextureImporterNPOTScale.None;
        t.mipmapEnabled=false;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;
        t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;
    }
}
