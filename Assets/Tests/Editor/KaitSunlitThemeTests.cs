using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class KaitSunlitThemeTests
{
    [TestCase(-20f)]
    [TestCase(20f)]
    public void BothSidesShareOnePressAndClick(float pointerX)
    {
        var events = new GameObject("Events", typeof(EventSystem));
        var host = new GameObject("Button", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(HybridStyleGraphic), typeof(HybridStyleButton));
        Sprite sprite = KaitSunlitTheme.Load("Button", 0.15f, 80f);
        try
        {
            var rect = host.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 50);
            var surface = host.GetComponent<HybridStyleGraphic>();
            surface.SetFallbackSplit(0.5f, 0.5f);
            var button = host.GetComponent<HybridStyleButton>();
            button.Configure(surface, sprite, sprite, Color.gray);
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            int clicks = 0;
            button.onClick.AddListener(() => clicks++);
            var pointer = new PointerEventData(events.GetComponent<EventSystem>())
            {
                button = PointerEventData.InputButton.Left,
                position = new Vector2(pointerX, 0)
            };
            button.OnPointerDown(pointer);
            Assert.AreEqual(0.94f, rect.localScale.x, 0.001f);
            var serialized = new SerializedObject(surface);
            Assert.AreEqual(0.76f, serialized.FindProperty("leftTint").colorValue.r, 0.001f);
            Assert.AreEqual(0.38f, serialized.FindProperty("rightColor").colorValue.r, 0.001f);
            button.OnPointerUp(pointer);
            button.OnPointerClick(pointer);
            Assert.AreEqual(1, clicks);
            Assert.AreEqual(1f, rect.localScale.x, 0.001f);
            Assert.AreEqual(1, host.GetComponentsInChildren<Button>().Length);
        }
        finally
        {
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(events);
            Object.DestroyImmediate(sprite);
        }
    }

    [TestCase("Garden")]
    [TestCase("Floor")]
    [TestCase("Pillar")]
    [TestCase("Rift")]
    [TestCase("Panel")]
    [TestCase("Button")]
    public void ArtIsPresentSmoothAndUncompressed(string name)
    {
        string path = "Assets/Resources/" + KaitSunlitTheme.ResourceRoot + name + ".png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        Assert.IsNotNull(importer, name);
        Assert.AreEqual(FilterMode.Bilinear, importer.filterMode);
        Assert.IsFalse(importer.mipmapEnabled);
        Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
        Sprite sprite = KaitSunlitTheme.Load(name);
        Assert.IsNotNull(sprite);
        Assert.GreaterOrEqual(sprite.texture.width, 1024);
        Object.DestroyImmediate(sprite);
    }

    [Test]
    public void NineSliceBorderKeepsItsSmallUiFootprint()
    {
        Sprite panel = KaitSunlitTheme.Load("Panel", 0.1f, 160f);
        Sprite button = KaitSunlitTheme.Load("Button", 0.15f, 80f);
        Assert.AreEqual(16f, panel.border.x / (panel.pixelsPerUnit / 100f), 0.01f);
        Assert.AreEqual(12f, button.border.x / (button.pixelsPerUnit / 100f), 0.01f);
        Object.DestroyImmediate(panel);
        Object.DestroyImmediate(button);
    }

    [Test]
    public void SplitTextKeepsOneGraphicAndCutsItsGlyphColors()
    {
        var root = new GameObject("Root", typeof(RectTransform), typeof(GlobalStyleSplit));
        var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(SunlitSplitText));
        try
        {
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 100);
            var split = root.GetComponent<GlobalStyleSplit>();
            split.Configure(rect, 0.5f, 0.5f);
            textObject.transform.SetParent(root.transform, false);
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 100);
            var effect = textObject.GetComponent<SunlitSplitText>();
            effect.Configure(split);
            using (var helper = new VertexHelper())
            {
                helper.AddVert(new Vector3(-50, -10), Color.white, Vector2.zero);
                helper.AddVert(new Vector3(50, -10), Color.white, Vector2.right);
                helper.AddVert(new Vector3(0, 10), Color.white, Vector2.up);
                helper.AddTriangle(0, 1, 2);
                effect.ModifyMesh(helper);
                Assert.GreaterOrEqual(helper.currentVertCount, 6);
                for (int i = 0; i < helper.currentVertCount; i++)
                {
                    var v = UIVertex.simpleVert;
                    helper.PopulateUIVertex(ref v, i);
                    if (v.position.x < -1) Assert.Less(v.color.r, 120);
                    if (v.position.x > 1) Assert.AreEqual(255, v.color.r);
                }
            }
            Assert.AreEqual(1, root.GetComponentsInChildren<Text>().Length);
        }
        finally { Object.DestroyImmediate(root); }
    }
}
