using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class KaitRewardDeckTests
{
    private GameObject root;
    private KaitRewardDeck deck;
    private KaitRun run;
    private bool allow;
    private static object Field(object target,string name)=>target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
    private void Select(int index)=>typeof(KaitRewardDeck).GetMethod("Select",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(deck,new object[]{index});
    private object Call(string method,params object[] args)=>typeof(KaitRewardDeck).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(deck,args);
    private Image[] Slots=>(Image[])Field(deck,"slots");
    private void Drop(int index,int slot)
    {Assert.IsTrue((bool)Call("BeginDrag",index));var point=Slots[slot].rectTransform.anchoredPosition;Call("MoveDrag",index,point);Call("EndDrag",index,point);}
    [SetUp] public void Setup()
    {
        allow=true;root=new GameObject("Reward Test",typeof(RectTransform));
        var area=root.GetComponent<RectTransform>();area.sizeDelta=new Vector2(1920,1080);
        deck=root.AddComponent<KaitRewardDeck>();
        deck.Initialize(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),()=>allow,()=>{});
        run=new KaitRun();run.Reset(61);run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=16,resultValue=32});
        run.CurrentReward.choices.Clear();
        run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitSkill.HexCurse));
        run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitPassive.BloodBookmark));
        run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitSkill.DimensionDoor));
        deck.Sync(run);
    }
    [TearDown] public void Cleanup()=>Object.DestroyImmediate(root);
    [Test] public void MixedCardsUseOnlyMatchingTypeAndDoNotAddFullscreenBlocker()
    {
        var active=(KaitSkillCard[])Field(deck,"active");var passive=(KaitPassiveCard[])Field(deck,"passive");
        Assert.IsTrue(active[0].gameObject.activeSelf);Assert.IsFalse(passive[0].gameObject.activeSelf);
        Assert.IsFalse(active[1].gameObject.activeSelf);Assert.IsTrue(passive[1].gameObject.activeSelf);
        Assert.IsNull(root.GetComponent<Graphic>());
    }
    [Test] public void TappingOnlyPreviewsThenDroppingStagesCardAndHidesCandidates()
    {
        Select(0);Assert.IsNotNull(run.CurrentReward);Assert.IsEmpty(run.skills);
        Drop(0,0);Assert.IsNull(run.CurrentReward);Assert.IsTrue(run.IsAbilityPending(KaitAbilityCatalog.Get(KaitSkill.HexCurse)));
        foreach(var card in (KaitSkillCard[])Field(deck,"active"))Assert.IsFalse(card.gameObject.activeSelf);
        foreach(var card in (KaitPassiveCard[])Field(deck,"passive"))Assert.IsFalse(card.gameObject.activeSelf);
    }
    [Test] public void BusyGateKeepsPackUnchangedAndChoiceIsNotConsumed()
    {
        allow=false;Select(0);Assert.IsNotNull(run.CurrentReward);Assert.IsEmpty(run.skills);
        Assert.IsFalse((bool)Call("BeginDrag",0));allow=true;Drop(0,0);Assert.IsNull(run.CurrentReward);
    }
    [Test] public void FullSlotsShowReplacementAndCancelKeepsPack()
    {
        run.skills.AddRange(new[]{KaitSkill.SwiftBoots,KaitSkill.CatAgility,KaitSkill.Command});
        Select(0);Assert.AreEqual(3,run.skills.Count);
        for(int i=0;i<Slots.Length;i++)Assert.AreEqual(i<3,Slots[i].gameObject.activeSelf,"Kait retains only three visible replacement targets");
        ((Button)Field(deck,"cancel")).onClick.Invoke();Assert.IsNotNull(run.CurrentReward);
        CollectionAssert.DoesNotContain(run.skills,KaitSkill.HexCurse);
    }
    [Test] public void FullSlotDragReplacesOnlyTheChosenCard()
    {
        run.skills.AddRange(new[]{KaitSkill.SwiftBoots,KaitSkill.CatAgility,KaitSkill.Command});
        Drop(0,1);CollectionAssert.AreEqual(new[]{KaitSkill.SwiftBoots,KaitSkill.HexCurse,KaitSkill.Command},run.skills);
        Assert.IsTrue(run.IsSkillActive(KaitSkill.CatAgility));Assert.IsFalse(run.IsSkillActive(KaitSkill.HexCurse));
    }
    [Test] public void WrongSideReleaseNeverConsumesReward()
    {
        Assert.IsTrue((bool)Call("BeginDrag",0));Call("EndDrag",0,new Vector2(0,348));
        Assert.IsNotNull(run.CurrentReward);Assert.IsEmpty(run.skills);Assert.IsFalse(deck.IsDragging);
    }
    [Test] public void BusyStateAtReleaseRejectsPreviouslyValidSlot()
    {
        Assert.IsTrue((bool)Call("BeginDrag",0));var pos=Slots[0].rectTransform.anchoredPosition;
        allow=false;Call("EndDrag",0,pos);Assert.IsNotNull(run.CurrentReward);Assert.IsEmpty(run.skills);
    }
    [Test] public void PassiveTargetsAreAboveAndActiveTargetsBelow()
    {
        Select(1);Assert.Greater(Slots[0].rectTransform.anchoredPosition.y,250);
        Select(0);Assert.Less(Slots[0].rectTransform.anchoredPosition.y,-250);
        foreach(var target in Slots)Assert.IsFalse(target.raycastTarget);
    }
    [Test] public void SimulacrumNeedsTwoSeparateDropsAndPreservesOriginal()
    {
        run.passives.Add(KaitPassive.BirdEye);run.CurrentReward.choices[1]=KaitAbilityCatalog.Get(KaitPassive.Simulacrum);
        Drop(1,0);Assert.IsNotNull(run.CurrentReward);CollectionAssert.AreEqual(new[]{KaitPassive.BirdEye},run.passives);
        Drop(1,1);Assert.IsNull(run.CurrentReward);CollectionAssert.AreEqual(new[]{KaitPassive.BirdEye,KaitPassive.Simulacrum},run.passives);
        Assert.AreEqual(KaitPassive.BirdEye,run.copiedPassive);
    }
    [Test] public void FullSimulacrumCanCopyOneAndReplaceAnother()
    {
        run.passives.AddRange(new[]{KaitPassive.BirdEye,KaitPassive.CheshireCat,KaitPassive.Trend});
        run.CurrentReward.choices[1]=KaitAbilityCatalog.Get(KaitPassive.Simulacrum);
        Drop(1,0);Drop(1,2);Assert.AreEqual(KaitPassive.Simulacrum,run.passives[2]);Assert.AreEqual(KaitPassive.BirdEye,run.copiedPassive);
    }
    [Test] public void CannotStartAnotherFingerOrConsumeAnotherPack()
    {
        Assert.IsTrue((bool)Call("BeginDrag",0));Assert.IsFalse((bool)Call("BeginDrag",1));
        var pos=Slots[0].rectTransform.anchoredPosition;
        run.SkipReward();run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=16,resultValue=32});
        Call("EndDrag",0,pos);Assert.IsEmpty(run.skills);Assert.IsNotNull(run.CurrentReward);
    }
    [Test] public void ButtonsAndDropZonesHaveLargeTouchAreas()
    {
        foreach(string name in new[]{"skip","cancel","reroll","fold"})
        {var rect=((Button)Field(deck,name)).GetComponent<RectTransform>();Assert.GreaterOrEqual(rect.rect.width,140);Assert.GreaterOrEqual(rect.rect.height,60);}
        Select(0);Assert.GreaterOrEqual(Slots[0].rectTransform.rect.width,230);Assert.GreaterOrEqual(Slots[0].rectTransform.rect.height,160);
        var bar=(RectTransform)Field(deck,"bar");Assert.AreEqual(1,bar.GetComponent<HybridStyleGraphic>().color.a);
        Assert.IsNotNull(bar.Find("Drag Handle").GetComponent<KaitRewardBarDrag>());
        foreach(var button in bar.GetComponentsInChildren<Button>())Assert.IsInstanceOf<HybridStyleButton>(button);
        Assert.GreaterOrEqual(Slots[0].transform.Find("Drop Action").GetComponent<RectTransform>().rect.height,58);
    }
    [Test] public void ReplacementBeforeAllSlotsAreFilledIsExplicitAndValid()
    {run.skills.Add(KaitSkill.SwiftBoots);Drop(0,0);CollectionAssert.AreEqual(new[]{KaitSkill.HexCurse},run.skills);}
    [Test] public void RealPointerHandlersDoNotTurnDragIntoClickAndReturnOnMiss()
    {
        var events=new GameObject("Drag Events",typeof(EventSystem));
        try
        {
            var card=((KaitSkillCard[])Field(deck,"active"))[0];var home=card.Rect.anchoredPosition;
            var pointer=new PointerEventData(events.GetComponent<EventSystem>()){pointerId=7,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,card.Rect.position)};
            card.OnPointerDown(pointer);card.OnBeginDrag(pointer);
            pointer.position=RectTransformUtility.WorldToScreenPoint(null,root.transform.TransformPoint(new Vector3(600,0,0)));
            card.OnDrag(pointer);card.OnPointerUp(pointer);card.OnEndDrag(pointer);card.OnPointerClick(pointer);
            Assert.IsNotNull(run.CurrentReward);Assert.IsEmpty(run.skills);Assert.IsFalse(card.IsDragging);
            typeof(KaitSkillCard).GetMethod("Advance",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(card,new object[]{1f});
            Assert.Less(Vector2.Distance(home,card.Rect.anchoredPosition),3);
        }
        finally{Object.DestroyImmediate(events);}
    }
    [Test] public void ActualOwnedCardCooldownRectStaysAboveBottomEdge()
    {
        var area=root.GetComponent<RectTransform>();
        var card=KaitSkillCard.Create(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null,null,null,null,null);
        card.Show(KaitSkill.HexCurse,false,new Vector2(0,KaitSkillCard.DockY(area.rect,false,false)),0);
        card.SetAvailability(false,3,false);
        var text=card.transform.Find("Availability").GetComponent<Text>();
        float bottom=card.Rect.anchoredPosition.y+text.rectTransform.anchoredPosition.y-text.rectTransform.rect.height*.5f;
        Assert.GreaterOrEqual(bottom,area.rect.yMin+12);
        Assert.AreEqual("冷却 3 回合",text.text);
    }
    [Test] public void DockedPassiveNameRemainsBelowTopEdge()
    {
        var area=root.GetComponent<RectTransform>();
        var card=KaitPassiveCard.Create(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null,null,null,null);
        card.Show(KaitPassive.BirdEye,false,new Vector2(0,KaitPassiveCard.DockY(area.rect,false,false)),0);
        var text=card.transform.Find("Name").GetComponent<Text>();
        float top=card.Rect.anchoredPosition.y+text.rectTransform.anchoredPosition.y+text.rectTransform.rect.height*.5f;
        Assert.LessOrEqual(top,area.rect.yMax-12);
        Assert.AreEqual("警戒武器",text.text);
    }
}
