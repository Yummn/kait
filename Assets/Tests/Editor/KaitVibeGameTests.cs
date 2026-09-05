using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class KaitVibeGameTests
{
    [Test]
    public void RejectsFilesOutsideStaging()
    {
        Assert.Throws<InvalidOperationException>(() => KaitVibeGameTools.ImportImage(Path.Combine(Application.dataPath, "outside.png")));
    }

    [Test]
    public void ImportKeepsPixelsCreatesSpriteAndRefusesOverwrite()
    {
        string name = "bridge-test-" + Guid.NewGuid().ToString("N") + ".png";
        string staging = Path.Combine(KaitVibeGameTools.StagingRoot, name);
        string assetPath = KaitVibeGameTools.ImportRoot + "/" + name;
        Directory.CreateDirectory(KaitVibeGameTools.StagingRoot);
        var fixture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        fixture.SetPixel(4, 4, Color.white);
        fixture.Apply();
        byte[] bytes = fixture.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(fixture);
        try
        {
            File.WriteAllBytes(staging, bytes);
            Assert.AreEqual(assetPath, KaitVibeGameTools.ImportImage(staging));
            Assert.That(File.ReadAllBytes(assetPath), Is.EqualTo(bytes));
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Sprite>(assetPath));
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
            Assert.AreEqual(FilterMode.Point, importer.filterMode);
            Assert.Throws<IOException>(() => KaitVibeGameTools.ImportImage(staging));
        }
        finally
        {
            AssetDatabase.DeleteAsset(assetPath);
            if (File.Exists(staging)) File.Delete(staging);
        }
    }
}
