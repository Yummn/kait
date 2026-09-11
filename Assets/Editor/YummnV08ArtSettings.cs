using UnityEditor;
using UnityEngine;
public sealed class YummnV08ArtSettings:AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/Resources/KaitVisuals/Yummn/V08/"))return;
        var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.textureCompression=TextureImporterCompression.Uncompressed;
        t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.wrapMode=TextureWrapMode.Clamp;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=2048;t.sRGBTexture=true;
    }
}
