using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class KaitCardAudioTests
{
    [TestCase("CardPickUp_A", .86f)]
    [TestCase("CardSnap_A", .165f)]
    [TestCase("CardPlay_A", .38f)]
    [TestCase("PassiveConfirm_B", .20f)]
    public void SelectedCardSoundPreservesAuditionImport(string name, float seconds)
    {
        string path = "Audio/UI/SelectedCards/" + name;
        var clip = Resources.Load<AudioClip>(path);
        Assert.NotNull(clip);
        Assert.AreEqual(48000, clip.frequency);
        Assert.AreEqual(2, clip.channels);
        Assert.That(clip.length, Is.EqualTo(seconds).Within(.001f));
        var importer = (AudioImporter)AssetImporter.GetAtPath("Assets/Resources/" + path + ".wav");
        Assert.AreEqual(AudioCompressionFormat.PCM, importer.defaultSampleSettings.compressionFormat);
        Assert.AreEqual(AudioSampleRateSetting.PreserveSampleRate, importer.defaultSampleSettings.sampleRateSetting);
        Assert.IsFalse(importer.forceToMono);
        Assert.IsFalse(new SerializedObject(importer).FindProperty("m_Normalize").boolValue);
    }

    private static bool Pending(object card) => (bool)card.GetType()
        .GetField("pendingDockSound", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(card);

    private static void Tick(object card) => card.GetType()
        .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(card, null);

    [TestCase(false, false, true)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    public void SkillCardOnlyQueuesSnapForUnplayedOwnedDrag(bool candidate, bool accepted, bool pending)
    {
        var root = new GameObject("Area", typeof(RectTransform));
        var events = new GameObject("Events", typeof(EventSystem));
        try
        {
            var area = root.GetComponent<RectTransform>(); area.sizeDelta = new Vector2(1920,1080);
            var card = KaitSkillCard.Create(area, null, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                null, null, null, (c,x)=>c.SetDock(x), c=>accepted);
            card.Show(KaitSkill.SwiftBoots, candidate, Vector2.zero, 0);
            Assert.IsFalse(Pending(card), "Initial layout must be silent");
            card.SetAvailability(true, 0, false);
            var e = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId=-1, position=Vector2.zero };
            card.OnPointerDown(e); card.OnBeginDrag(e); card.OnEndDrag(e);
            Assert.AreEqual(pending, Pending(card));
            card.OnEndDrag(e);
            Assert.AreEqual(pending, Pending(card), "Repeated end events cannot re-arm feedback");
            card.SetCovered(true);
            Assert.IsFalse(Pending(card), "Closing/end-game must cancel pending sound");
        }
        finally { Object.DestroyImmediate(root); Object.DestroyImmediate(events); }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ManualDockConsumesSnapOnceAtArrival(bool passive)
    {
        var root = new GameObject("Area", typeof(RectTransform));
        var events = new GameObject("Events", typeof(EventSystem));
        try
        {
            var area = root.GetComponent<RectTransform>(); area.sizeDelta = new Vector2(1920,1080);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var e = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId=-1, position=new Vector2(500,0) };
            if (passive)
            {
                var card = KaitPassiveCard.Create(area,null,font,null,null,null,(c,x)=>c.SetDock(x));
                card.Show(KaitPassive.BirdEye,false,Vector2.zero,0);
                Tick(card); Assert.IsFalse(Pending(card));
                card.OnPointerDown(e); card.OnBeginDrag(e); card.OnEndDrag(e);
                Assert.IsTrue(Pending(card)); Tick(card); Assert.IsTrue(Pending(card));
                card.Rect.anchoredPosition=new Vector2(card.DockX,area.rect.yMax);
                Tick(card); Assert.IsFalse(Pending(card)); Tick(card); Assert.IsFalse(Pending(card));
            }
            else
            {
                var card = KaitSkillCard.Create(area,null,font,null,null,null,(c,x)=>c.SetDock(x),c=>false);
                card.Show(KaitSkill.SwiftBoots,false,new Vector2(500,0),500);
                Tick(card); Assert.IsFalse(Pending(card));
                card.OnPointerDown(e); card.OnBeginDrag(e); card.OnEndDrag(e);
                Assert.IsTrue(Pending(card)); Tick(card); Assert.IsTrue(Pending(card));
                card.Rect.anchoredPosition=new Vector2(card.DockX,KaitSkillCard.DockY(area.rect,false,false));
                Tick(card); Assert.IsFalse(Pending(card)); Tick(card); Assert.IsFalse(Pending(card));
            }
        }
        finally { Object.DestroyImmediate(root); Object.DestroyImmediate(events); }
    }
}
