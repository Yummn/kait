using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitDangerViewportTests
{
    [TestCase(1920,1080)]
    [TestCase(2400,1080)]
    [TestCase(2340,1080)]
    [TestCase(2560,1600)]
    [TestCase(1080,2400)]
    public void CornersFollowViewportNotReferenceAspect(int width, int height)
    {
        var root = new GameObject("Viewport", typeof(RectTransform), typeof(Canvas));
        try
        {
            var go = new GameObject("Edges", typeof(RectTransform), typeof(KaitAtmosphereGraphic));
            go.transform.SetParent(root.transform, false);
            var graphic = go.GetComponent<KaitAtmosphereGraphic>();
            graphic.rectTransform.sizeDelta = new Vector2(width, height);
            graphic.SetState(0, 1, 0);
            var art = go.transform.Find("Danger C").GetComponent<Image>();
            Assert.That(art.type, Is.EqualTo(Image.Type.Sliced));
            Assert.That(art.fillCenter, Is.False);
            Assert.That(art.raycastTarget, Is.False);
            Assert.That(art.maskable, Is.False);
            float corner = art.sprite.border.x / art.pixelsPerUnitMultiplier;
            Assert.That(corner, Is.EqualTo(Mathf.Min(width,height)*.165f).Within(.01f));
            float bleed = KaitAtmosphereGraphic.DangerBleed(new Vector2(width,height));
            Assert.That(art.rectTransform.offsetMin.x, Is.EqualTo(-bleed).Within(.01f));
            Assert.That(art.rectTransform.offsetMax.y, Is.EqualTo(bleed).Within(.01f));
            graphic.rectTransform.sizeDelta = new Vector2(height,width)*.5f;
            graphic.SetState(0,1,.3f);
            Assert.That(art.rectTransform.offsetMax.x, Is.EqualTo(bleed*.5f).Within(.01f));
        }
        finally { Object.DestroyImmediate(root); }
    }
}
