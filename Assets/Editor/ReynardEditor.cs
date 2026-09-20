using System.IO;
using UnityEditor;
using UnityEngine;
using Spine.Unity;
public static class ReynardEditor
{
 public static void Prepare()
 {
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  foreach(var path in Directory.GetFiles("Assets/Resources/Characters/Reynard","*.png",SearchOption.AllDirectories)){
   string p=path.Replace('\\','/');AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(p);importer.textureType=TextureImporterType.Default;importer.textureShape=TextureImporterShape.Texture2D;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
   string id=new DirectoryInfo(Path.GetDirectoryName(p)).Name;var mat=AssetDatabase.LoadAssetAtPath<Material>(Path.GetDirectoryName(p).Replace('\\','/')+"/"+id+"_Material.mat");if(mat!=null){mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(p);EditorUtility.SetDirty(mat);}
  }
  foreach(var path in Directory.GetFiles("Assets/Resources/Reynard","*.png",SearchOption.AllDirectories)){
   string p=path.Replace('\\','/');AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);var t=(TextureImporter)AssetImporter.GetAtPath(p);t.textureType=TextureImporterType.Default;t.textureShape=TextureImporterShape.Texture2D;t.mipmapEnabled=false;t.alphaIsTransparency=true;t.maxTextureSize=p.Contains("Cards/")?512:2048;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();
  }
  AssetDatabase.SaveAssets();
 }
 public static void Build(){Prepare();KaitBuild.BuildWindowsDemo();}
}

