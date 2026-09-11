using UnityEditor;
using UnityEngine;

public sealed class YummnFrameArtSettings : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/Resources/KaitVisuals/Yummn/Frames/"))return;
        var t=(TextureImporter)assetImporter;
        t.textureType=TextureImporterType.Default;t.spriteImportMode=SpriteImportMode.None;
        t.mipmapEnabled=false;t.alphaIsTransparency=true;t.npotScale=TextureImporterNPOTScale.None;
        t.filterMode=FilterMode.Bilinear;t.wrapMode=TextureWrapMode.Clamp;
        t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;
        t.ClearPlatformTextureSettings("Standalone");t.ClearPlatformTextureSettings("Android");
    }
}
