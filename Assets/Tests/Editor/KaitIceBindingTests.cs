using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitIceBindingTests
{
    [Test]
    public void IceHoldsUntilRulesReleaseAndFollowsContact()
    {
        var go = new GameObject("Ice QA", typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitCombatEffectGraphic), typeof(KaitIceBinding));
        try
        {
            var graphic = go.GetComponent<KaitCombatEffectGraphic>();
            graphic.rectTransform.sizeDelta = Vector2.one * 120;
            graphic.Configure(KaitCombatEffectKind.Ice, Color.cyan, Color.white, .65f);
            Assert.IsTrue(graphic.UsesIceAtlas);
            Assert.AreEqual(1774, graphic.mainTexture.width);
            Assert.IsFalse(graphic.maskable); Assert.IsFalse(graphic.raycastTarget);
            bool frozen = true;
            Vector3 ground = new Vector3(35, 20, 0);
            var binding = go.GetComponent<KaitIceBinding>();
            binding.IsFrozen = () => frozen;
            binding.GroundPosition = () => ground;
            binding.Initialize(graphic);
            binding.Advance(10);
            Assert.IsFalse(binding.Releasing);
            Assert.AreEqual(3, graphic.PushFrame);
            Assert.AreEqual(ground, go.transform.position);
            ground += Vector3.right * 20;
            binding.Advance(0);
            Assert.AreEqual(ground, go.transform.position);
            graphic.Rebuild(CanvasUpdate.PreRender);
            Assert.AreEqual(4, graphic.canvasRenderer.GetMesh().vertexCount);
            Assert.AreEqual(Vector3.one, graphic.transform.localScale);
            frozen = false;
            binding.Advance(.08f);
            Assert.IsTrue(binding.Releasing);
            Assert.That(graphic.PushFrame, Is.InRange(5, 6));
            ground += Vector3.right * 100;
            binding.Advance(.01f);
            Assert.AreNotEqual(ground, go.transform.position, "Breakup remains at last contact");
        }
        finally { Object.DestroyImmediate(go); }
    }

    [Test]
    public void IceCanReconfigureWithoutLeavingAtlasEnabled()
    {
        var go = new GameObject("Ice QA", typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitCombatEffectGraphic));
        try
        {
            var effect = go.GetComponent<KaitCombatEffectGraphic>();
            effect.Configure(KaitCombatEffectKind.Ice, Color.white, Color.white, .5f);
            Assert.IsTrue(effect.UsesIceAtlas);
            effect.Configure(KaitCombatEffectKind.NormalHit, Color.white, Color.white, .5f);
            Assert.IsFalse(effect.UsesIceAtlas);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
