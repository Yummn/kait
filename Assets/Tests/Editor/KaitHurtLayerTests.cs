using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class KaitHurtLayerTests
{
    [Test]
    public void HurtBUsesOriginalEightFramesAndDoesNotTintOtherImpacts()
    {
        var tex=Resources.Load<Texture2D>("KaitVisuals/Effects/KaitHurtB");
        Assert.NotNull(tex); Assert.AreEqual(1774,tex.width); Assert.AreEqual(887,tex.height);
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/KaitVisuals/Effects/KaitHurtB.png");
        Assert.IsFalse(importer.mipmapEnabled);
        Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
        var go=new GameObject("Hurt B",typeof(RectTransform),typeof(CanvasRenderer),typeof(KaitCombatEffectGraphic));
        try
        {
            var graphic=go.GetComponent<KaitCombatEffectGraphic>();
            graphic.rectTransform.sizeDelta=Vector2.one*112;
            graphic.Configure(KaitCombatEffectKind.KaitHurt,Color.red,Color.blue,1);
            Assert.IsTrue(graphic.UsesHurtAtlas); Assert.IsFalse(graphic.UsesPushAtlas);
            Assert.AreSame(tex,graphic.mainTexture); Assert.AreEqual(Color.white,graphic.color);
            Assert.IsFalse(graphic.raycastTarget); Assert.IsFalse(graphic.maskable);
            for(int frame=0;frame<8;frame++)
            {
                graphic.SetProgress((frame+.2f)/8f); graphic.Rebuild(CanvasUpdate.PreRender);
                var mesh=graphic.canvasRenderer.GetMesh(); Assert.AreEqual(4,mesh.vertexCount);
                var uv=KaitSwordAtlasView.FrameUv(frame,1774,887);
                foreach(var point in mesh.uv)
                {
                    Assert.That(point.x,Is.InRange(uv.xMin,uv.xMax));
                    Assert.That(point.y,Is.InRange(uv.yMin,uv.yMax));
                }
            }
            graphic.Configure(KaitCombatEffectKind.NormalHit,Color.white,Color.white,.5f);
            Assert.IsFalse(graphic.UsesHurtAtlas); Assert.IsTrue(graphic.UsesShatterAtlas);
        }
        finally { Object.DestroyImmediate(go); }
    }

    [TestCase(KaitCombatEffectKind.NormalHit)]
    [TestCase(KaitCombatEffectKind.Kill)]
    [TestCase(KaitCombatEffectKind.ChainKill)]
    [TestCase(KaitCombatEffectKind.Block)]
    [TestCase(KaitCombatEffectKind.Push)]
    [TestCase(KaitCombatEffectKind.EnemyHit)]
    public void EnemyImpactsUseMiddleLayer(KaitCombatEffectKind kind)
    { Assert.IsTrue(KaitCombatLayers.IsEnemyImpact(kind)); }

    [TestCase(KaitCombatEffectKind.KaitHurt)]
    [TestCase(KaitCombatEffectKind.MagicCast)]
    [TestCase(KaitCombatEffectKind.Speed)]
    public void OtherEffectsDoNotEnterEnemyHitLayer(KaitCombatEffectKind kind)
    { Assert.IsFalse(KaitCombatLayers.IsEnemyImpact(kind)); }

    [Test]
    public void ReorderingActorsCannotCrossEnemyImpactLayer()
    {
        var root=new GameObject("Board",typeof(RectTransform));
        try
        {
            var enemies=new GameObject("Enemies",typeof(RectTransform)).GetComponent<RectTransform>();
            enemies.SetParent(root.transform,false); enemies.sizeDelta=new Vector2(720,720);
            var impacts=KaitCombatLayers.AddAbove(enemies,"Enemy impacts");
            var kait=KaitCombatLayers.AddAbove(impacts,"Kait");
            var enemy=new GameObject("Pushed enemy").transform; enemy.SetParent(enemies,false);
            var death=new GameObject("Detached death").transform; death.SetParent(enemies,false);
            var player=new GameObject("Moving Kait").transform; player.SetParent(kait,false);
            enemy.SetAsLastSibling(); death.SetAsFirstSibling(); player.SetAsLastSibling();
            Assert.Less(enemies.GetSiblingIndex(),impacts.GetSiblingIndex());
            Assert.Less(impacts.GetSiblingIndex(),kait.GetSiblingIndex());
            Assert.AreEqual(enemies.sizeDelta,kait.sizeDelta);
            Assert.AreEqual(enemies.anchoredPosition,impacts.anchoredPosition);
            Assert.IsEmpty(root.GetComponentsInChildren<Mask>(true));
            Assert.IsEmpty(root.GetComponentsInChildren<RectMask2D>(true));
        }
        finally { Object.DestroyImmediate(root); }
    }
}
