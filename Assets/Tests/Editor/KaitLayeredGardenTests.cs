using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitLayeredGardenTests
{
    [TestCase("TreeTrunk")]
    [TestCase("TreeCanopy")]
    [TestCase("FlowerClump")]
    public void IndependentDecorationsHaveRealSourceAlpha(string name)
    {
        string path = "Assets/Resources/" + KaitSunlitTheme.ResourceRoot + name + ".png";
        var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
        Assert.IsNotNull(importer);
        Assert.IsTrue(importer.DoesSourceTextureHaveAlpha(), name);
        Assert.IsTrue(importer.alphaIsTransparency);
        Assert.IsFalse(importer.mipmapEnabled);
        Assert.IsTrue(KaitLayeredGarden.ArtReady);
    }

    [Test]
    public void ProjectionUsesOneLightDirectionAndPreservesFootprint()
    {
        var centre = new Vector2(20,30);
        var m = KaitLayeredGarden.ProjectionMatrix(centre, new Vector2(100,80), 0, 40, .5f);
        Vector3 projected = m.MultiplyPoint3x4(Vector3.zero);
        Assert.AreEqual(48.8f, projected.x, .001f);
        Assert.AreEqual(-10, projected.y, .001f);
        Assert.AreEqual(100, m.MultiplyVector(Vector3.right).magnitude, .001f);
        Assert.AreEqual(40, m.MultiplyVector(Vector3.up).magnitude, .001f);
    }

    [Test]
    public void AlphaFieldDoesNotUseLegacyBlackWhiteCoverage()
    {
        Assert.AreEqual(1, KaitGroundDecal.AlphaMaterial.GetFloat("_UseAlpha"));
        Assert.AreEqual(0, KaitGroundDecal.MaskMaterial.GetFloat("_UseAlpha"));
        Assert.IsNotNull(Resources.Load<Shader>("Shaders/UIDecorProjection"));
    }

    [Test]
    public void LiftShadowFollowsCardAndHidesWithItWithoutBlockingInput()
    {
        var root = new GameObject("Root", typeof(RectTransform));
        try
        {
            var area = root.GetComponent<RectTransform>();
            area.sizeDelta = new Vector2(1920,1080);
            var card = KaitSkillCard.Create(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                KaitSunlitTheme.Load("SkillCardHD"),KaitSunlitTheme.Load("SkillCardFlat"),null,null,null);
            var shadow = root.GetComponentInChildren<KaitSoftShadow>(true);
            Assert.IsNotNull(shadow);
            // EditMode does not dispatch a non-ExecuteAlways component's lifecycle.
            InvokeLifecycle(card.GetComponent<KaitLiftShadow>(), "OnDisable");
            Assert.IsFalse(shadow.gameObject.activeSelf);
            card.Show(KaitSkill.SwiftBoots,false,new Vector2(120,-540),120);
            InvokeLifecycle(card.GetComponent<KaitLiftShadow>(), "OnEnable");
            InvokeLifecycle(card.GetComponent<KaitLiftShadow>(), "LateUpdate");
            Assert.IsTrue(shadow.gameObject.activeSelf);
            Assert.AreEqual(card.transform.GetSiblingIndex()-1,shadow.transform.GetSiblingIndex());
            Assert.Greater(shadow.rectTransform.anchoredPosition.x,card.Rect.anchoredPosition.x);
            Assert.Less(shadow.rectTransform.anchoredPosition.y,card.Rect.anchoredPosition.y);
            Assert.IsFalse(shadow.raycastTarget);
            Assert.IsFalse(shadow.maskable);
            card.Hide();
            InvokeLifecycle(card.GetComponent<KaitLiftShadow>(), "OnDisable");
            Assert.IsFalse(shadow.gameObject.activeSelf);
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static void InvokeLifecycle(KaitLiftShadow shadow, string method)
    {
        typeof(KaitLiftShadow).GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(shadow, null);
    }

    [Test]
    public void ContactShadowTracksActorFade()
    {
        var root = new GameObject("Root",typeof(RectTransform));
        try
        {
            var actor = new GameObject("Actor",typeof(RectTransform),typeof(Image)).GetComponent<Image>();
            actor.transform.SetParent(root.transform,false);
            var shadow = KaitContactShadow.Create(root.GetComponent<RectTransform>(),actor,Vector2.zero,new Vector2(40,10));
            actor.color = new Color(1,1,1,.5f);
            shadow.SendMessage("LateUpdate");
            Assert.AreEqual(.145f,shadow.color.a,.001f);
            Assert.AreEqual(new Vector2(44,11),shadow.rectTransform.sizeDelta);
        }
        finally { Object.DestroyImmediate(root); }
    }
}
