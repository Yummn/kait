using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitDreadAtlasTests
{
    [Test]
    public void ApprovedDreadFramesRemainSmallAndDoNotBlockInput()
    {
        var go = new GameObject("Dread QA", typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitCombatEffectGraphic));
        try
        {
            var effect = go.GetComponent<KaitCombatEffectGraphic>();
            effect.rectTransform.sizeDelta = Vector2.one * 150;
            effect.Configure(KaitCombatEffectKind.DreadSlash, Color.red, Color.white, .65f);
            effect.Play(.4f);
            Assert.IsTrue(effect.UsesDreadAtlas);
            Assert.AreEqual(1774, effect.mainTexture.width);
            Assert.IsFalse(effect.maskable);
            Assert.IsFalse(effect.raycastTarget);
            Assert.AreEqual(Vector3.one, effect.transform.localScale);
            Assert.AreEqual(Color.white, effect.color);
            for (int frame = 0; frame < 8; frame++)
            {
                effect.SetProgress((frame + .1f) / 8);
                effect.Rebuild(CanvasUpdate.PreRender);
                Assert.AreEqual(frame, effect.PushFrame);
                Assert.AreEqual(4, effect.canvasRenderer.GetMesh().vertexCount);
            }
            effect.SetProgress(1);
            effect.Rebuild(CanvasUpdate.PreRender);
            Assert.AreEqual(0, effect.canvasRenderer.GetMesh().vertexCount);
            effect.Configure(KaitCombatEffectKind.Speed, Color.white, Color.white, .5f);
            Assert.IsFalse(effect.UsesDreadAtlas);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
