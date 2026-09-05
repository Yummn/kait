using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class KaitSwordAtlasTests
{
    [TestCase("Normal")]
    [TestCase("Finisher")]
    public void ApprovedAtlasIsFullResolution(string variant)
    {
        string path = "KaitVisuals/Effects/WhiteGoldSlash" + variant;
        var texture = Resources.Load<Texture2D>(path);
        Assert.NotNull(texture);
        Assert.AreEqual(1774, texture.width);
        Assert.AreEqual(887, texture.height);
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/" + path + ".png");
        Assert.IsFalse(importer.mipmapEnabled);
        Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
    }

    [Test]
    public void EightFramesStayInsideTheirCells()
    {
        for (int i = 0; i < 8; i++)
        {
            Rect uv = KaitSwordAtlasView.FrameUv(i, 1774, 887);
            Assert.Greater(uv.xMin, i % 4 / 4f);
            Assert.Less(uv.xMax, (i % 4 + 1) / 4f);
            Assert.Greater(uv.yMin, 1f - (i / 4 + 1) / 2f);
            Assert.Less(uv.yMax, 1f - i / 4 / 2f);
        }
    }
}
