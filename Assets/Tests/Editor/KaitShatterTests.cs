using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public sealed class KaitShatterTests
{
    [TestCase(KaitCombatEffectKind.NormalHit, 0)]
    [TestCase(KaitCombatEffectKind.Kill, 1)]
    [TestCase(KaitCombatEffectKind.ChainKill, 2)]
    [TestCase(KaitCombatEffectKind.Block, 3)]
    public void ApprovedAtlasIsUsedWithCorrectRow(KaitCombatEffectKind kind, int row)
    {
        var go = new GameObject("Shatter", typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitCombatEffectGraphic));
        try
        {
            var g = go.GetComponent<KaitCombatEffectGraphic>();
            g.rectTransform.sizeDelta = new Vector2(112,112);
            g.Configure(kind, Color.red, Color.blue, 0.5f);
            Assert.IsTrue(g.UsesShatterAtlas);
            Assert.AreEqual(row, g.ShatterRow);
            Assert.AreEqual(Color.white, g.color, "Do not tint the approved palette");
            Assert.IsFalse(g.raycastTarget);
            Assert.IsFalse(g.maskable);
            for (int frame=0;frame<6;frame++)
            {
                g.SetProgress((frame+0.2f)/6f);
                Assert.AreEqual(frame,g.ShatterFrame);
                g.Rebuild(CanvasUpdate.PreRender);
                Mesh mesh = g.canvasRenderer.GetMesh();
                Assert.AreEqual(4,mesh.vertexCount);
                foreach(var uv in mesh.uv)
                {
                    Assert.That(uv.x,Is.InRange(frame/6f,(frame+1)/6f));
                    Assert.That(uv.y,Is.InRange(1f-(row+1)/4f,1f-row/4f));
                }
            }
            g.SetProgress(1f);g.Rebuild(CanvasUpdate.PreRender);
            Assert.AreEqual(0,g.canvasRenderer.GetMesh().vertexCount);
            g.Play(.32f);Assert.AreEqual(Vector3.one,g.rectTransform.localScale);
        }
        finally { Object.DestroyImmediate(go); }
    }

    [Test]
    public void AtlasKeepsGridDimensionsAndNoLossyCompression()
    {
        var t=Resources.Load<Texture2D>("KaitVisuals/Effects/WhiteGoldShatter");
        Assert.NotNull(t);Assert.AreEqual(1536,t.width);Assert.AreEqual(1024,t.height);
        var i=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/KaitVisuals/Effects/WhiteGoldShatter.png");
        Assert.IsFalse(i.mipmapEnabled);
        Assert.AreEqual(TextureImporterCompression.Uncompressed,i.textureCompression);
        Assert.AreEqual(TextureImporterNPOTScale.None,i.npotScale);
        Assert.NotNull(Resources.Load<Shader>("Shaders/UIWhiteGoldShatter"));
    }

    [TestCase(KaitCombatEffectKind.SwordArc)]
    [TestCase(KaitCombatEffectKind.MagicImpact)]
    [TestCase(KaitCombatEffectKind.EnemyHit)]
    public void UnrelatedEffectsKeepTheirExistingRenderer(KaitCombatEffectKind kind)
    { Assert.AreEqual(-1,KaitCombatEffectGraphic.AtlasRow(kind)); }
}
