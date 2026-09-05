using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class KaitPushEffectTests
{
    [Test]
    public void ApprovedPushAtlasIsUncompressedAndUnscaled()
    {
        var tex = Resources.Load<Texture2D>("KaitVisuals/Effects/WhiteGoldPush");
        Assert.NotNull(tex); Assert.AreEqual(1774,tex.width); Assert.AreEqual(887,tex.height);
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/KaitVisuals/Effects/WhiteGoldPush.png");
        Assert.IsFalse(importer.mipmapEnabled);
        Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
        Assert.AreEqual(TextureImporterNPOTScale.None,importer.npotScale);
    }

    [Test]
    public void PushHasEightFramesNoMaskNoRaycastAndNoScalePulse()
    {
        var go=new GameObject("Push test",typeof(RectTransform),typeof(CanvasRenderer),typeof(KaitCombatEffectGraphic));
        try
        {
            var graphic=go.GetComponent<KaitCombatEffectGraphic>();
            graphic.rectTransform.sizeDelta=Vector2.one*160;
            graphic.Configure(KaitCombatEffectKind.Push,Color.red,Color.blue,.5f);
            Assert.IsTrue(graphic.UsesPushAtlas); Assert.IsFalse(graphic.UsesShatterAtlas);
            Assert.AreEqual(Color.white,graphic.color);
            Assert.IsFalse(graphic.maskable); Assert.IsFalse(graphic.raycastTarget);
            graphic.Play(.32f); Assert.AreEqual(Vector3.one,graphic.rectTransform.localScale);
            for(int frame=0;frame<8;frame++)
            {
                graphic.SetProgress((frame+.25f)/8f);
                Assert.AreEqual(frame,graphic.PushFrame);
                graphic.Rebuild(CanvasUpdate.PreRender);
                var mesh=graphic.canvasRenderer.GetMesh(); Assert.AreEqual(4,mesh.vertexCount);
                Rect uv=KaitSwordAtlasView.FrameUv(frame,1774,887);
                foreach(var point in mesh.uv)
                {
                    Assert.That(point.x,Is.InRange(uv.xMin,uv.xMax));
                    Assert.That(point.y,Is.InRange(uv.yMin,uv.yMax));
                }
            }
            graphic.SetProgress(1); graphic.Rebuild(CanvasUpdate.PreRender);
            Assert.AreEqual(0,graphic.canvasRenderer.GetMesh().vertexCount);
            graphic.Configure(KaitCombatEffectKind.NormalHit,Color.white,Color.white,.5f);
            Assert.IsFalse(graphic.UsesPushAtlas); Assert.IsTrue(graphic.UsesShatterAtlas);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
