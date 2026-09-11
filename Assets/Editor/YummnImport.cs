using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Spine.Unity;
using Spine.Unity.Editor;
public static class YummnImport
{
    public static void BuildWindows(){Run();KaitBuild.BuildWindowsDemo();}
    public static void Run()
    {
        string root="Assets/Resources/Characters/Yummn/108231";
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        AssetUtility.ImportSpineContent(new[]{root+".png",root+".atlas.txt",root+".json"},new List<string>(),true);
        foreach(var path in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/KaitVisuals/Yummn"}))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(path));
            importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();var data=AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(root+"_SkeletonData.asset");
        if(data==null||data.GetSkeletonData(false)==null)throw new System.Exception("Yummn skeleton import failed");
        Debug.Log("YUMMN_IMPORT_OK animations="+data.GetSkeletonData(false).Animations.Count);
    }
}
