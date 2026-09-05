using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitMageEffectTests
{
    [Test]
    public void ClippedSpellPreservesFrameSamplingAndLeavesBlockedArmEmpty()
    {
        var go=new GameObject("Mage clipping test",typeof(RectTransform),typeof(CanvasRenderer),typeof(KaitMageEffectGraphic));
        try
        {
            var effect=go.GetComponent<KaitMageEffectGraphic>();
            effect.rectTransform.sizeDelta=Vector2.one*360;
            // Right arm is blocked. Diagonals must never receive fragments.
            Rect[] legal={new Rect(-50,-50,100,100),new Rect(-150,-50,100,100),
                new Rect(-50,50,100,100),new Rect(-50,-150,100,100)};
            effect.ConfigureImpact(legal);Assert.IsTrue(effect.AtlasReady);
            Assert.IsFalse(effect.maskable);Assert.IsFalse(effect.raycastTarget);
            var texture=(Texture2D)effect.mainTexture;
            for (int frame=0;frame<8;frame++)
            {
                effect.SetProgress((frame+.1f)/8);effect.Rebuild(CanvasUpdate.PreRender);
                var mesh=effect.canvasRenderer.GetMesh();Assert.AreEqual(16,mesh.vertexCount);
                Rect uv=KaitSwordAtlasView.FrameUv(frame,texture.width,texture.height);
                for (int i=0;i<mesh.vertexCount;i++)
                {
                    Vector3 p=mesh.vertices[i];Rect cell=legal[i/4];
                    Assert.That(p.x,Is.InRange(cell.xMin,cell.xMax));
                    Assert.That(p.y,Is.InRange(cell.yMin,cell.yMax));
                    Assert.That(mesh.uv[i].x,Is.EqualTo(uv.xMin+(p.x+180)/360*uv.width).Within(.00001f));
                    Assert.That(mesh.uv[i].y,Is.EqualTo(uv.yMin+(p.y+180)/360*uv.height).Within(.00001f));
                }
            }
            effect.SetProgress(1);effect.Rebuild(CanvasUpdate.PreRender);
            Assert.AreEqual(0,effect.canvasRenderer.GetMesh().vertexCount);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
