using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KaitMainMenuTests
{
    [TestCase(1920, 1080, 1f)]
    [TestCase(2400, 1080, 1f)]
    [TestCase(1600, 1200, .8333333f)]
    public void ArtworkFitsWithoutStretchOrCrop(float w, float h, float expected)
    {
        Assert.That(KaitMainMenu.FitScale(new Vector2(w,h)), Is.EqualTo(expected).Within(.0001f));
    }

    [Test] public void BackgroundIsIncludedAndMobileSafe()
    {
        var texture = Resources.Load<Texture2D>(KaitMainMenu.BackgroundPath);
        Assert.NotNull(texture);
        var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
        Assert.False(importer.mipmapEnabled);
        Assert.False(importer.isReadable);
        Assert.AreEqual(TextureImporterNPOTScale.None, importer.npotScale);
        Assert.AreEqual(TextureImporterFormat.ASTC_4x4, importer.GetPlatformTextureSettings("Android").format);
    }

    [Test] public void ButtonsAreSeparateAndDispatchOnlyTheirOwnAction()
    {
        var root = new GameObject("Menu Test", typeof(RectTransform), typeof(Canvas));
        try
        {
            int start=0, tutorial=0, settings=0;
            var menu = KaitMainMenu.Create(root.transform, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null,
                () => start++, () => tutorial++, () => settings++);
            menu.TutorialButton.onClick.Invoke();
            Assert.AreEqual(0,start); Assert.AreEqual(1,tutorial); Assert.AreEqual(0,settings);
            menu.SettingsButton.onClick.Invoke(); menu.StartButton.onClick.Invoke();
            Assert.AreEqual(1,start); Assert.AreEqual(1,settings);
            Assert.AreEqual(3,menu.GetComponentsInChildren<Button>().Length);
            foreach(var label in menu.GetComponentsInChildren<Text>()) Assert.False(label.raycastTarget);
            Assert.False(menu.Layout.GetComponent<RawImage>().raycastTarget);
        }
        finally { Object.DestroyImmediate(root); }
    }

    [Test] public void PressResetsOnReleaseAndExit()
    {
        var go = new GameObject("Press Test", typeof(RectTransform));
        var events = new GameObject("Events", typeof(EventSystem));
        try
        {
            var feedback=go.AddComponent<KaitMenuButtonFeedback>();
            var pointer=new PointerEventData(events.GetComponent<EventSystem>()) { button=PointerEventData.InputButton.Left };
            feedback.OnPointerDown(pointer); Assert.Less(go.transform.localScale.x,1);
            feedback.OnPointerUp(pointer); Assert.AreEqual(Vector3.one,go.transform.localScale);
            feedback.OnPointerDown(pointer); feedback.OnPointerExit(pointer);
            Assert.AreEqual(Vector3.one,go.transform.localScale);
        }
        finally { Object.DestroyImmediate(go); Object.DestroyImmediate(events); }
    }
}
