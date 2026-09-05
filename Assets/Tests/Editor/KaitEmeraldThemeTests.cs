using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitEmeraldThemeTests
{
    [Test]
    public void CanopyCoordinatesMeetAcrossCellsAndFollowCanvasScale()
    {
        var source = new GameObject("Grass", typeof(RectTransform));
        var a = new GameObject("Cell A", typeof(RectTransform));
        var b = new GameObject("Cell B", typeof(RectTransform));
        try
        {
            var map = source.GetComponent<RectTransform>();
            map.sizeDelta = new Vector2(1080, 1080);
            map.localScale = Vector3.one * 1.3f;
            var ar = a.GetComponent<RectTransform>(); var br = b.GetComponent<RectTransform>();
            ar.SetParent(map, false); br.SetParent(map, false);
            ar.sizeDelta = br.sizeDelta = new Vector2(144, 144);
            ar.anchoredPosition = new Vector2(-72, 0); br.anchoredPosition = new Vector2(72, 0);
            Rect au = KaitGroundDecal.CalculateUvRect(ar, map), bu = KaitGroundDecal.CalculateUvRect(br, map);
            Assert.AreEqual(au.xMax, bu.xMin, .0001f);
            Assert.AreEqual(au.yMin, bu.yMin, .0001f);
            Assert.AreEqual(144f / 1080f, au.width, .0001f);
            Assert.AreEqual(new Rect(0, 0, 1, 1), KaitGroundDecal.CalculateUvRect(map, map));
        }
        finally { Object.DestroyImmediate(source); }
    }

    [Test]
    public void GroundDecalsDoNotInterceptGridInput()
    {
        var root = new GameObject("Ground", typeof(RectTransform));
        try
        {
            var decal = KaitGroundDecal.Create(root.transform, root.GetComponent<RectTransform>(), Texture2D.whiteTexture,
                Color.gray, "Canopy");
            Assert.IsFalse(decal.GetComponent<RawImage>().raycastTarget);
            Assert.IsNotNull(KaitGroundDecal.MaskMaterial);
            Assert.AreEqual("UI/Kait Ground Mask", KaitGroundDecal.MaskMaterial.shader.name);
        }
        finally { Object.DestroyImmediate(root); }
    }

    [Test]
    public void NewThemeKeepsOldArtAndFlatCardAvailable()
    {
        Assert.IsTrue(KaitSunlitTheme.ResourceRoot.EndsWith("EmeraldCourtyard/"));
        foreach (string name in new[] { "Garden", "Floor", "Pillar", "CanopyMask", "Rift", "Panel", "Button", "PassiveCardHD", "PassiveCardFlat" })
            Assert.IsNotNull(Resources.Load<Texture2D>(KaitSunlitTheme.ResourceRoot + name), name);
        Assert.IsNotNull(Resources.Load<Texture2D>("KaitVisuals/SunlitCourtyard/Garden"));
    }

    [Test]
    public void ContactShadowIsBelowActorAndDoesNotIntroduceAMask()
    {
        var root = new GameObject("Actor", typeof(RectTransform));
        var actor = new GameObject("Sprite", typeof(RectTransform), typeof(Image));
        try
        {
            actor.transform.SetParent(root.transform, false);
            var shadow = KaitContactShadow.Create(root.GetComponent<RectTransform>(), actor.GetComponent<Image>(),
                new Vector2(0, -42), new Vector2(42, 10));
            Assert.AreEqual(0, shadow.transform.GetSiblingIndex());
            Assert.IsFalse(shadow.raycastTarget);
            Assert.IsFalse(shadow.maskable);
            Assert.AreEqual(0, root.GetComponentsInChildren<Mask>().Length);
        }
        finally { Object.DestroyImmediate(root); }
    }
}
