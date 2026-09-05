using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitShadowStepEffectTests
{
    [Test]
    public void ShadowStepUsesApprovedEightFrameAtlas()
    {
        var go = new GameObject("Shadow Step A QA", typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitCombatEffectGraphic));
        try
        {
            var effect = go.GetComponent<KaitCombatEffectGraphic>();
            effect.rectTransform.sizeDelta = Vector2.one * 92f;
            effect.Configure(KaitCombatEffectKind.ShadowStep, Color.magenta, Color.white, .7f);
            Assert.IsTrue(effect.UsesShadowStepAtlas);
            Assert.AreEqual(1774, effect.mainTexture.width);
            Assert.AreEqual(887, effect.mainTexture.height);
            Assert.IsFalse(effect.raycastTarget);
            Assert.IsFalse(effect.maskable);
            effect.SetProgress(.35f);
            effect.Rebuild(CanvasUpdate.PreRender);
            Assert.AreEqual(4, effect.canvasRenderer.GetMesh().vertexCount);
        }
        finally { Object.DestroyImmediate(go); }
    }

    [Test]
    public void ReconfigureClearsShadowStepAtlas()
    {
        var go = new GameObject("Shadow Step A Reset QA", typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitCombatEffectGraphic));
        try
        {
            var effect = go.GetComponent<KaitCombatEffectGraphic>();
            effect.Configure(KaitCombatEffectKind.ShadowStep, Color.white, Color.white, .5f);
            Assert.IsTrue(effect.UsesShadowStepAtlas);
            effect.Configure(KaitCombatEffectKind.NormalHit, Color.white, Color.white, .5f);
            Assert.IsFalse(effect.UsesShadowStepAtlas);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
