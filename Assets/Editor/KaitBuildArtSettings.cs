using UnityEditor;
using UnityEngine;

public sealed class KaitBuildArtSettings : AssetPostprocessor
{
    private const string Root="Assets/Resources/KaitVisuals/Build061/";
    private void OnPreprocessTexture()
    {
        if(assetPath.StartsWith(Root))Configure((TextureImporter)assetImporter);
    }
    private static void Configure(TextureImporter importer)
    {
        importer.textureType=TextureImporterType.Default;
        importer.spriteImportMode=SpriteImportMode.None;
        importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
        importer.npotScale=TextureImporterNPOTScale.None;
        importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
        importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.ClearPlatformTextureSettings("Standalone");
        importer.ClearPlatformTextureSettings("Android");
    }
    public static void Apply()
    {
        foreach(string name in new[]{"CardFrames","Icons0","Icons1","Icons2","Effects"})
        {
            var importer=AssetImporter.GetAtPath(Root+name+".png") as TextureImporter;
            if(importer==null)continue;
            Configure(importer);importer.SaveAndReimport();
        }
    }
}
