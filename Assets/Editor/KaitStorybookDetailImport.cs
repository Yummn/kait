using UnityEditor;

public sealed class KaitStorybookDetailImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/Resources/KaitVisuals/Storybook099/")&&!assetPath.StartsWith("Assets/Resources/KaitVisuals/Storybook0910/")&&!assetPath.StartsWith("Assets/Resources/KaitVisuals/Storybook0911/")&&!assetPath.StartsWith("Assets/Resources/KaitVisuals/Storybook0914/")&&!assetPath.StartsWith("Assets/Resources/KaitVisuals/Storybook0915/"))return;
        Configure((TextureImporter)assetImporter,assetPath);
    }
    static void Configure(TextureImporter importer,string assetPath)
    {
        importer.textureType=TextureImporterType.Default;
        importer.npotScale=TextureImporterNPOTScale.None;
        importer.filterMode=UnityEngine.FilterMode.Bilinear;
        importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;
        importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
        // Large source portraits are minified into an 80px HUD: mip filtering
        // prevents thin hair/ink edges from becoming a pixel staircase.
        if(assetPath.Contains("Storybook0910/")&&(assetPath.Contains("Portrait.png")||assetPath.EndsWith("Fist.png")||assetPath.EndsWith("Heart.png")||assetPath.EndsWith("Qi.png")||assetPath.EndsWith("Gear.png")))
        {importer.mipmapEnabled=true;importer.filterMode=UnityEngine.FilterMode.Trilinear;}
        importer.maxTextureSize=assetPath.Contains("Storybook0915/")?4096:2048;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
    }
    public static void Apply()
    {
        foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/KaitVisuals/Storybook0910","Assets/Resources/KaitVisuals/Storybook0911","Assets/Resources/KaitVisuals/Storybook0915"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Configure(importer,path);importer.SaveAndReimport();
        }
    }
}
