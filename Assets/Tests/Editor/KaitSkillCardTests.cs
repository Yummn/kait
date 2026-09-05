using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class KaitSkillCardTests
{
    [Test]
    public void PreviewSupportsHoverGraceClickTimeoutAndOutsidePress()
    {
        var root = new GameObject("Deck",typeof(RectTransform),typeof(KaitSkillDeck));
        var events = new GameObject("Events",typeof(EventSystem));
        try
        {
            var area = root.GetComponent<RectTransform>(); area.sizeDelta = new Vector2(1920,1080);
            var deck = root.GetComponent<KaitSkillDeck>();
            deck.Initialize(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),i=>{},i=>true,()=>{});
            var run = new KaitRun(); run.Reset(42); run.skills.Add(KaitSkill.SwiftBoots); run.skills.Add(KaitSkill.IceTomb);
            deck.Sync(run,KaitSkill.None);
            var card = deck.Owned[0]; var other = deck.Owned[1];
            var e = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId=-1 };
            card.OnPointerEnter(e);
            Assert.IsTrue(card.ShouldPreviewAt(Time.unscaledTime+100),"Hover stays readable");
            card.OnPointerExit(e);
            Assert.IsTrue(card.ShouldPreviewAt(Time.unscaledTime+2));
            Assert.IsFalse(card.ShouldPreviewAt(Time.unscaledTime+4));
            card.OnPointerClick(e); other.OnPointerClick(e);
            deck.DismissOtherPreviews(card.transform.Find("Name"));
            Assert.IsTrue(card.ShouldPreviewAt(Time.unscaledTime));
            Assert.IsFalse(other.ShouldPreviewAt(Time.unscaledTime));
            card.SetAvailability(true,0,true);
            deck.DismissOtherPreviews(null);
            card.OnPointerExit(e);
            Assert.IsFalse(card.ShouldPreviewAt(Time.unscaledTime),"Outside click must not be undone by exit or targeting");
            card.OnPointerClick(e);
            Assert.IsTrue(card.ShouldPreviewAt(Time.unscaledTime+2));
            Assert.IsFalse(card.ShouldPreviewAt(Time.unscaledTime+4));
            card.OnPointerDown(e); card.OnBeginDrag(e); deck.DismissOtherPreviews(null);
            Assert.IsTrue(card.IsDragging);
        }
        finally { Object.DestroyImmediate(root); Object.DestroyImmediate(events); }
    }

    [Test]
    public void BottomDockShowsCompleteCooldownAndExpandsInsideScreen()
    {
        var area = new Rect(-960,-540,1920,1080);
        Assert.AreEqual(-516, KaitSkillCard.DockY(area, false, false));
        // Availability is centred at y=2 and 22 pixels high on the card.
        Assert.GreaterOrEqual(KaitSkillCard.DockY(area, false, false) + 2 - 11, area.yMin + 12);
        Assert.Greater(KaitSkillCard.DockY(area, true, false) - KaitSkillCard.Size.y / 2, area.yMin);
        Assert.Less(KaitSkillCard.DockY(area, false, true) + KaitSkillCard.Size.y / 2, area.yMin);
        var x = KaitSkillDeck.ResolveDockPositions(new[] { 800f, 810f, 820f }, area);
        Assert.LessOrEqual(x[2] + KaitSkillCard.Size.x / 2, area.xMax);
        Assert.GreaterOrEqual(x[1] - x[0], KaitSkillCard.Size.x + 14);
        Assert.IsFalse(KaitSkillDeck.IsInCastZone(new Vector2(0,-400)));
        Assert.IsTrue(KaitSkillDeck.IsInCastZone(KaitSkillDeck.CastZone.center));
    }

    [TestCase(false, true, true, 1, 0)]
    [TestCase(false, true, false, 0, 1)]
    [TestCase(false, false, true, 0, 0)]
    [TestCase(true, true, true, 0, 0)]
    public void OnlyReadyOwnedCardReleasedInCenterCasts(bool candidate, bool ready, bool central, int castsExpected, int docksExpected)
    {
        var root = new GameObject("Root", typeof(RectTransform));
        var events = new GameObject("Events", typeof(EventSystem));
        Sprite hd = KaitSunlitTheme.Load("SkillCardHD"), flat = KaitSunlitTheme.Load("SkillCardFlat");
        try
        {
            var area = root.GetComponent<RectTransform>(); area.sizeDelta = new Vector2(1920,1080);
            int casts = 0, docks = 0, choices = 0;
            var card = KaitSkillCard.Create(area, null, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), hd, flat,
                c => choices++, (c,x) => docks++, c => { casts++; return true; });
            card.Show(KaitSkill.SwiftBoots, candidate, Vector2.zero, 0);
            card.SetAvailability(ready, ready ? 0 : 2, false);
            var e = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId = -1, position = Vector2.zero };
            // Clicking never casts; it only reveals or chooses.
            card.OnPointerDown(e); card.OnPointerUp(e); card.OnPointerClick(e);
            Assert.AreEqual(0, casts); choices = 0;
            card.OnPointerDown(e); card.OnBeginDrag(e);
            e.position = central ? KaitSkillDeck.CastZone.center : new Vector2(500,-300);
            card.OnDrag(e); card.OnPointerUp(e); card.OnEndDrag(e); card.OnPointerClick(e);
            card.OnEndDrag(e);
            Assert.AreEqual(castsExpected, casts); Assert.AreEqual(docksExpected, docks);
            Assert.AreEqual(0, choices); Assert.IsFalse(card.IsDragging); Assert.IsTrue(card.SuppressedClick);
        }
        finally { Object.DestroyImmediate(root); Object.DestroyImmediate(events); Object.DestroyImmediate(hd); Object.DestroyImmediate(flat); }
    }

    [Test]
    public void CastRechecksCooldownAndDoesNotConsumeTurn()
    {
        var run = new KaitRun(); run.Reset(42); run.skills.Add(KaitSkill.SwiftBoots);
        int turn = run.turn;
        Assert.IsTrue(KaitSkillDeck.IsReady(run, KaitSkill.SwiftBoots));
        Assert.IsTrue(run.TryUseSkill(KaitSkill.SwiftBoots, -1, out _));
        Assert.IsFalse(KaitSkillDeck.IsReady(run, KaitSkill.SwiftBoots));
        Assert.AreEqual(1, run.activeSpeedModifiers.Count); Assert.AreEqual(turn, run.turn);
        Assert.IsFalse(KaitSkillDeck.IsReady(run, KaitSkill.ShadowStep));
    }

    [Test]
    public void GrabbingExposedTitleCanReleaseAtScreenCentre()
    {
        var root = new GameObject("Root", typeof(RectTransform));
        var events = new GameObject("Events", typeof(EventSystem));
        try
        {
            var area = root.GetComponent<RectTransform>(); area.sizeDelta = new Vector2(1920,1080);
            int casts = 0;
            var card = KaitSkillCard.Create(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null,null,null,null,c=>{casts++; return true;});
            card.Show(KaitSkill.SwiftBoots,false,new Vector2(0,-540),0);
            card.SetAvailability(true,0,false);
            var pointer = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId = 0, position = new Vector2(0,-440) };
            card.OnPointerDown(pointer); card.OnBeginDrag(pointer);
            pointer.position = Vector2.zero;
            card.OnDrag(pointer);
            Assert.Less(card.Rect.anchoredPosition.y, KaitSkillDeck.CastZone.yMin);
            card.OnPointerUp(pointer); card.OnEndDrag(pointer);
            Assert.AreEqual(1, casts);
        }
        finally { Object.DestroyImmediate(root); Object.DestroyImmediate(events); }
    }

    [Test]
    public void SkillDeckSyncKeepsOwnedAndResetHidesEverything()
    {
        var root = new GameObject("Deck", typeof(RectTransform), typeof(KaitSkillDeck));
        try
        {
            var area = root.GetComponent<RectTransform>(); area.sizeDelta = new Vector2(1920,1080);
            var deck = root.GetComponent<KaitSkillDeck>();
            deck.Initialize(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),i=>{},i=>true,()=>{});
            var run = new KaitRun(); run.Reset(42); run.skills.Add(KaitSkill.IceTomb);
            deck.Sync(run,KaitSkill.None);
            Assert.IsTrue(deck.Owned[0].gameObject.activeSelf); Assert.IsTrue(deck.Owned[0].Ready);
            Assert.IsFalse(deck.Owned[1].gameObject.activeSelf);
            deck.ResetDeck();
            foreach (var c in deck.Owned) Assert.IsFalse(c.gameObject.activeSelf);
            foreach (var c in deck.Candidates) Assert.IsFalse(c.gameObject.activeSelf);
        }
        finally { Object.DestroyImmediate(root); }
    }

    [Test]
    public void SkillFacesDifferFromPassivesAndAllSkillsHaveDifferentSigils()
    {
        Assert.IsNotNull(Resources.Load<Texture2D>(KaitSunlitTheme.ResourceRoot + "SkillCardHD"));
        Assert.IsNotNull(Resources.Load<Texture2D>(KaitSunlitTheme.ResourceRoot + "SkillCardFlat"));
        var set = new System.Collections.Generic.HashSet<string>();
        foreach (KaitSkill skill in System.Enum.GetValues(typeof(KaitSkill)))
            if (skill != KaitSkill.None) Assert.IsTrue(set.Add(KaitSkillCard.Sigil(skill)));
    }
}
