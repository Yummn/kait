using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class KaitBoundaryDustTests
{
    [Test]
    public void AtlasPreservesApprovedSizeAndFiltering()
    {
        var tex=Resources.Load<Texture2D>("KaitVisuals/Effects/BoundaryDustA");
        Assert.NotNull(tex); Assert.AreEqual(1774,tex.width); Assert.AreEqual(887,tex.height);
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/KaitVisuals/Effects/BoundaryDustA.png");
        Assert.IsFalse(importer.mipmapEnabled);
        Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
        Assert.AreEqual(TextureImporterNPOTScale.None,importer.npotScale);
    }

    [TestCase(1,0)] [TestCase(-1,0)] [TestCase(0,1)] [TestCase(0,-1)]
    public void DustTrailsBehindMovementAndPinsContact(int x,int y)
    {
        var direction=new Vector2Int(x,y);
        Assert.AreEqual(Vector2.zero,KaitCombatEffectGraphic.BoundaryFloorPoint(Vector2.zero,direction));
        var tail=KaitCombatEffectGraphic.BoundaryFloorPoint(Vector2.left*50,direction);
        Assert.Less(Vector2.Dot(tail,direction),0);
        if (y != 0) Assert.AreEqual(0f, tail.x, "Vertical dust must trail straight behind, not diagonally");
    }

    [Test]
    public void FramesPreserveContactUvAndDoNotBlockInput()
    {
        var go=new GameObject("Boundary test",typeof(RectTransform),typeof(CanvasRenderer),typeof(KaitCombatEffectGraphic));
        try
        {
            var graphic=go.GetComponent<KaitCombatEffectGraphic>();
            graphic.rectTransform.sizeDelta=Vector2.one*136;
            graphic.Configure(KaitCombatEffectKind.BoundaryDust,Color.red,Color.blue,.5f);
            Assert.IsTrue(graphic.UsesBoundaryAtlas); Assert.IsFalse(graphic.maskable); Assert.IsFalse(graphic.raycastTarget);
            graphic.Play(.38f); Assert.AreEqual(Vector3.one,graphic.rectTransform.localScale);
            for(int frame=0;frame<8;frame++)
            {
                graphic.SetProgress((frame+.25f)/8); graphic.Rebuild(CanvasUpdate.PreRender);
                var mesh=graphic.canvasRenderer.GetMesh(); Assert.AreEqual(4,mesh.vertexCount);
                float baseline=frame<4?.742f:.563f;
                Vector3 contact=mesh.vertices[0]+(mesh.vertices[3]-mesh.vertices[0])*.835f+
                    (mesh.vertices[1]-mesh.vertices[0])*(1-baseline);
                Assert.Less(contact.magnitude,.001f);
                Rect uv=KaitSwordAtlasView.FrameUv(frame,1774,887);
                foreach(var point in mesh.uv)
                {
                    Assert.That(point.x,Is.InRange(uv.xMin,uv.xMax));
                    Assert.That(point.y,Is.InRange(uv.yMin,uv.yMax));
                }
            }
            graphic.SetProgress(1);graphic.Rebuild(CanvasUpdate.PreRender);
            Assert.AreEqual(0,graphic.canvasRenderer.GetMesh().vertexCount);
            graphic.Configure(KaitCombatEffectKind.NormalHit,Color.white,Color.white,.5f);
            Assert.IsFalse(graphic.UsesBoundaryAtlas); Assert.IsTrue(graphic.UsesShatterAtlas);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
