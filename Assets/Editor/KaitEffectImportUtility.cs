using System.Collections.Generic;
using System.IO;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEngine;

public static class KaitEffectImportUtility
{
    private const string EffectRoot = "Assets/Resources/Effects/Kait";

    public static void ReimportAll()
    {
        if (!AssetDatabase.IsValidFolder(EffectRoot))
        {
            throw new DirectoryNotFoundException(EffectRoot);
        }

        string[] folders = Directory.GetDirectories(EffectRoot);
        foreach (string absoluteFolder in folders)
        {
            string folder = absoluteFolder.Replace('\\', '/');
            int assetsIndex = folder.IndexOf("Assets/", System.StringComparison.Ordinal);
            if (assetsIndex >= 0)
            {
                folder = folder.Substring(assetsIndex);
            }

            string name = Path.GetFileName(folder);
            string png = folder + "/" + name + ".png";
            string atlas = folder + "/" + name + ".atlas.txt";
            string json = folder + "/" + name + ".json";

            AssetDatabase.ImportAsset(png, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(atlas, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(json, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetUtility.ImportSpineContent(new[] { png, atlas, json }, new List<string>(), true);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Debug.Log("KAIT_EFFECT_IMPORT_COMPLETE");
    }

    public static void ValidateAll()
    {
        int validated = 0;
        foreach (string absoluteFolder in Directory.GetDirectories(EffectRoot))
        {
            string name = Path.GetFileName(absoluteFolder);
            string assetPath = EffectRoot + "/" + name + "/" + name + "_SkeletonData.asset";
            var asset = AssetDatabase.LoadAssetAtPath<Spine.Unity.SkeletonDataAsset>(assetPath);
            if (asset == null)
            {
                throw new InvalidDataException("Missing effect SkeletonDataAsset: " + assetPath);
            }

            Spine.SkeletonData skeleton = asset.GetSkeletonData(false);
            Spine.Animation animation = skeleton?.FindAnimation("texiao");
            if (animation == null)
            {
                throw new InvalidDataException("Missing texiao animation: " + assetPath);
            }

            foreach (Spine.Unity.AtlasAssetBase atlas in asset.atlasAssets)
            {
                if (atlas == null) throw new InvalidDataException("Missing atlas: " + assetPath);
                foreach (Material material in atlas.Materials)
                    if (material == null || material.mainTexture == null)
                        throw new InvalidDataException("Missing atlas texture: " + assetPath);
            }

            validated++;
            Debug.Log($"KAIT_EFFECT_VALID {name} duration={animation.Duration:0.###}");
        }

        if (validated != 14)
        {
            throw new InvalidDataException("Expected 14 effects, validated " + validated);
        }
        Debug.Log("KAIT_EFFECT_VALIDATION_COMPLETE count=" + validated);
    }
}
