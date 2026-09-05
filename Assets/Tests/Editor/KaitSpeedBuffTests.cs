using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class KaitSpeedBuffTests
{
    [Test]
    public void ApprovedAtlasIsUnscaledAndUncompressed()
    {
        var tex=Resources.Load<Texture2D>("KaitVisuals/Effects/SpeedBuffB");
        Assert.NotNull(tex);Assert.AreEqual(1774,tex.width);Assert.AreEqual(887,tex.height);
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/KaitVisuals/Effects/SpeedBuffB.png");
        Assert.IsFalse(importer.mipmapEnabled);Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
    }
    [Test]
    public void SpeedFramesUseTintAndNeverScaleOrBlockInput()
    {
        var go=new GameObject("Speed test",typeof(RectTransform),typeof(CanvasRenderer),typeof(KaitCombatEffectGraphic));
        try
        {
            var effect=go.GetComponent<KaitCombatEffectGraphic>();effect.rectTransform.sizeDelta=Vector2.one*145;
            effect.Configure(KaitCombatEffectKind.Speed,Color.cyan,Color.white,.5f);effect.Play(.4f);
            Assert.IsTrue(effect.UsesSpeedAtlas);Assert.IsFalse(effect.maskable);Assert.IsFalse(effect.raycastTarget);
            Assert.AreEqual(Vector3.one,effect.transform.localScale);
            for(int frame=0;frame<8;frame++)
            {
                effect.SetProgress((frame+.2f)/8);effect.Rebuild(CanvasUpdate.PreRender);
                var mesh=effect.canvasRenderer.GetMesh();Assert.AreEqual(4,mesh.vertexCount);
                Assert.AreEqual(0,mesh.colors[0].r);Assert.AreEqual(1,mesh.colors[0].g);
            }
            effect.SetProgress(1);effect.Rebuild(CanvasUpdate.PreRender);Assert.AreEqual(0,effect.canvasRenderer.GetMesh().vertexCount);
            effect.Configure(KaitCombatEffectKind.NormalHit,Color.white,Color.white,.5f);Assert.IsFalse(effect.UsesSpeedAtlas);
        }
        finally { Object.DestroyImmediate(go); }
    }
    [Test]
    public void FollowTransfersActorsAndRefreshesTintWithoutRestarting()
    {
        var a=new GameObject("Standing",typeof(RectTransform));var b=new GameObject("Moving",typeof(RectTransform));
        var go=new GameObject("Speed",typeof(RectTransform),typeof(CanvasRenderer),typeof(KaitCombatEffectGraphic),typeof(KaitSpeedBuffFollow));
        try
        {
            var actor=(RectTransform)a.transform;actor.sizeDelta=Vector2.one*115;
            var effect=go.GetComponent<KaitCombatEffectGraphic>();effect.Configure(KaitCombatEffectKind.Speed,Color.cyan,Color.white,.5f);effect.SetProgress(.4f);
            var follow=go.GetComponent<KaitSpeedBuffFollow>();Color tint=Color.cyan;
            follow.Actor=()=>actor;follow.Tint=()=>tint;follow.Direction=()=>Vector2Int.left;follow.Refresh();
            actor=(RectTransform)b.transform;actor.sizeDelta=Vector2.one*115;actor.position=new Vector3(90,50);tint=Color.red;follow.Refresh();
            Assert.Less(Vector3.Distance(go.transform.position,actor.TransformPoint(new Vector3(0,-43.7f,0))),.001f);
            Assert.AreEqual(Color.red,effect.color);Assert.AreEqual(3,effect.PushFrame);
        }
        finally { Object.DestroyImmediate(go);Object.DestroyImmediate(a);Object.DestroyImmediate(b); }
    }
}
