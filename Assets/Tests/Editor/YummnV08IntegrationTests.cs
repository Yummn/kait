using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class YummnV08IntegrationTests
{
    private const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    [Test] public void MovingPoseSurvivesCellAnchoringAndTracksLayoutChanges()
    {
        var host=new GameObject("Inactive movement fixture");host.SetActive(false);
        try
        {
            var game=host.AddComponent<KaitGame>();
            var run=(KaitRun)typeof(KaitGame).GetField("run",Hidden).GetValue(game);run.SelectCharacter(KaitCharacter.Yummn,89,new YummnRulesSnapshot());
            var cells=new Image[49];
            foreach(int i in new[]{24,26}){var go=new GameObject("Cell",typeof(RectTransform),typeof(Image));go.transform.SetParent(host.transform);cells[i]=go.GetComponent<Image>();}
            cells[24].transform.position=new Vector3(100,200,0);cells[26].transform.position=new Vector3(300,200,0);
            typeof(KaitGame).GetField("battleCells",Hidden).SetValue(game,cells);
            typeof(KaitGame).GetField("yummnMoving",Hidden).SetValue(game,true);
            typeof(KaitGame).GetField("yummnMoveFrom",Hidden).SetValue(game,new Vector2Int(3,3));
            typeof(KaitGame).GetField("yummnMoveTo",Hidden).SetValue(game,new Vector2Int(5,3));
            typeof(KaitGame).GetField("yummnMoveProgress",Hidden).SetValue(game,.5f);
            var method=typeof(KaitGame).GetMethod("KaitVisualPosition",Hidden);
            Assert.AreEqual(new Vector3(200,200,0),method.Invoke(game,new object[]{new Vector2Int(3,3)}));
            cells[26].transform.position=new Vector3(500,200,0);
            Assert.AreEqual(new Vector3(300,200,0),method.Invoke(game,new object[]{new Vector2Int(3,3)}));
            typeof(KaitGame).GetField("yummnMoving",Hidden).SetValue(game,false);
            Assert.AreEqual(new Vector3(100,200,0),method.Invoke(game,new object[]{new Vector2Int(3,3)}));
            run.SelectCharacter(KaitCharacter.Kait,89);typeof(KaitGame).GetField("yummnMoving",Hidden).SetValue(game,true);
            Assert.AreEqual(new Vector3(100,200,0),method.Invoke(game,new object[]{new Vector2Int(3,3)}));
        }
        finally{UnityEngine.Object.DestroyImmediate(host);}
    }
    [Test] public void ArrowBlockedImmediatelyDoesNotTargetDefaultBoardCorner()
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,87,new YummnRulesSnapshot());
        var origin=new Vector2Int(3,3);r.Yummn.icePillar=origin+Vector2Int.right;
        var intent=(KaitIntent)typeof(KaitRun).GetMethod("BuildYummnArrow",Hidden).Invoke(r,new object[]{origin,Vector2Int.right,false});
        Assert.IsEmpty(intent.affectedCells);Assert.AreEqual(origin,intent.target);
    }
    [Test] public void AllMonkAssetsAndCuesArePresent()
    {
        foreach(var d in YummnCatalog.Cards){var icon=KaitCardSkin.Icon(d);Assert.NotNull(icon,d.id);Assert.LessOrEqual(icon.rect.xMax,icon.texture.width);Assert.LessOrEqual(icon.rect.yMax,icon.texture.height);Assert.GreaterOrEqual(icon.rect.yMin,0);Assert.NotNull(KaitCardSkin.Face(d),d.id);Assert.IsNotEmpty(d.sigil);Assert.IsNotEmpty(d.traditionTag);}
        for(int i=0;i<12;i++)Assert.NotNull(YummnV08Art.Effect(i),"effect "+i);
        foreach(var cue in new[]{"Water","Air","Ice","Frost","Fire","Shatter","Shadow","PalmSeal","Ki","Exhaust","Recover"})Assert.NotNull(Resources.Load<AudioClip>("Audio/Yummn/V08/"+cue),cue);
    }
    [Test] public void KiEffectInitializesAlphaSpriteInCreationFrame()
    {
        var go=new GameObject("Fx",typeof(RectTransform),typeof(YummnV08Effect));
        try{var fx=go.GetComponent<YummnV08Effect>();Assert.AreEqual(0,fx.color.a);fx.Initialize(9,.3f);Assert.NotNull(fx.sprite);Assert.AreNotEqual(YummnV08Art.Material,fx.material);Assert.IsFalse(fx.maskable);Assert.IsFalse(fx.raycastTarget);}
        finally{UnityEngine.Object.DestroyImmediate(go);}
    }
    [Test] public void SplitCardKeepsBlackKeyAfterEnableAndRestoresKaitMaterial()
    {
        var root=new GameObject("Logo",typeof(RectTransform));
        try
        {
            var logo=KaitCardLogo.Create(root.transform,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),Vector2.zero,76);
            logo.Show(KaitSkill.Flurry);root.SetActive(false);root.SetActive(true);
            var graphic=logo.transform.Find("Cartoon Logo").GetComponent<HybridStyleGraphic>();
            Assert.AreEqual("UI/Hybrid Style",graphic.material.shader.name);Assert.AreEqual(1,graphic.material.GetFloat("_BlackKeyLeft"));
            logo.Show(KaitSkill.SwiftBoots);Assert.AreEqual(0,graphic.material.GetFloat("_BlackKeyLeft"));
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
    [Test] public void NewActionLogIncludesOrderedHitsPreparedIdsAndCounts()
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,81,new YummnRulesSnapshot());r.enemies.Clear();r.spawns.Clear();
        r.skills.Add(KaitSkill.Flurry);r.TryUseSkill(KaitSkill.Flurry,-1,out _);
        var e=new KaitEnemy{id=71,type=KaitEnemyType.Guard,pos=r.katePos+Vector2Int.right,hp=4,maxHp=4,life=KaitEnemyLife.Active};r.enemies.Add(e);
        var result=r.TryGlobalInput(KaitDirection.Right);var json=r.YummnActionJson(result);
        StringAssert.Contains("yummn.M01",json);StringAssert.Contains("\"enemyHits\":[{\"key\":\"71\",\"count\":3}]",json);
        StringAssert.Contains(YummnRulesProfile.Version,json);StringAssert.Contains("\"seed\":81",json);Assert.AreEqual(3,result.yummnEvents.Count(ev=>ev.kind==YummnEventKind.Hit));
    }
    [Test] public void PreparedSkillReplayRoundTripKeepsTerrainPhaseAndLogDeterministic()
    {
        var a=new KaitRun();a.SelectCharacter(KaitCharacter.Yummn,82,new YummnRulesSnapshot());
        // Acquire via a reproducible merge/reward stream, not directly editing equipment.
        foreach(var d in new[]{KaitDirection.Right,KaitDirection.Up,KaitDirection.Left,KaitDirection.Down,KaitDirection.Right,KaitDirection.Up})a.TryGlobalInput(d);
        var save=a.SaveReplay();var b=new KaitRun();Assert.IsTrue(b.RestoreReplay(save));Assert.AreEqual(a.Ki,b.Ki);Assert.AreEqual(a.KiPhase,b.KiPhase);Assert.AreEqual(save,b.SaveReplay());
        var aa=a.TryGlobalInput(KaitDirection.Left);var bb=b.TryGlobalInput(KaitDirection.Left);
        Assert.AreEqual(a.YummnActionJson(aa),b.YummnActionJson(bb));
    }
    [Test] public void ClickingOwnedMonkOnlyPreparesAndTogglesWithoutKiSpend()
    {
        var root=new GameObject("Deck",typeof(RectTransform),typeof(KaitSkillDeck));var events=new GameObject("Events",typeof(EventSystem));
        try
        {
            var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,83,new YummnRulesSnapshot());r.skills.Add(KaitSkill.Flurry);
            var area=root.GetComponent<RectTransform>();area.sizeDelta=new Vector2(1920,1080);var deck=root.GetComponent<KaitSkillDeck>();
            deck.Initialize(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),i=>{},i=>r.TryUseSkill(r.skills[i],-1,out _),()=>{});deck.Sync(r,KaitSkill.None);
            var e=new PointerEventData(events.GetComponent<EventSystem>()){pointerId=-1};deck.Owned[0].OnPointerClick(e);Assert.IsTrue(r.FlurryArmed);Assert.AreEqual(5,r.Ki);Assert.AreEqual(0,r.turn);Assert.AreEqual(2,r.PreparedKiCost);
            deck.Owned[0].OnPointerClick(e);Assert.IsFalse(r.FlurryArmed);Assert.AreEqual(5,r.Ki);
        }
        finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(events);}
    }
    [Test] public void PreparingReplacementClearsOnlyIncompatibleFistModifiers()
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,84,new YummnRulesSnapshot());r.skills.AddRange(new[]{KaitSkill.Flurry,KaitSkill.PatientDefense,KaitSkill.WaterWhip});
        r.TryUseSkill(KaitSkill.Flurry,-1,out _);r.TryUseSkill(KaitSkill.PatientDefense,-1,out _);r.TryUseSkill(KaitSkill.WaterWhip,-1,out _);
        Assert.IsFalse(r.FlurryArmed);Assert.IsTrue(r.DefenseArmed);Assert.IsTrue(r.IsYummnPrepared(KaitSkill.WaterWhip));Assert.AreEqual(2,r.PreparedKiCost);
    }
    [Test] public void ReplacingPrerequisiteShowsConfirmationAndCancelKeepsBothCards()
    {
        var root=new GameObject("Rewards",typeof(RectTransform),typeof(KaitRewardDeck));
        try
        {
            var area=root.GetComponent<RectTransform>();area.sizeDelta=new Vector2(1920,1080);var deck=root.GetComponent<KaitRewardDeck>();
            var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,85,new YummnRulesSnapshot());r.skills.Add(KaitSkill.Flurry);r.passives.Add(KaitPassive.OpenHand);
            var pack=new KaitRewardPack{id=1};pack.choices.Add(YummnCatalog.Get("E01"));r.rewardQueue.Enqueue(pack);
            deck.Initialize(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),()=>true,()=>{});deck.Sync(r);
            typeof(KaitRewardDeck).GetMethod("BeginDrag",Hidden).Invoke(deck,new object[]{0});
            var slots=(Image[])typeof(KaitRewardDeck).GetField("slots",Hidden).GetValue(deck);
            typeof(KaitRewardDeck).GetMethod("EndDrag",Hidden).Invoke(deck,new object[]{0,slots[0].rectTransform.anchoredPosition});
            var prompt=root.transform.Find("Confirm Prerequisite Replacement");Assert.NotNull(prompt);Assert.IsTrue(r.skills.Contains(KaitSkill.Flurry));Assert.NotNull(r.CurrentReward);
            prompt.Find("取消").GetComponent<Button>().onClick.Invoke();Assert.IsTrue(r.skills.Contains(KaitSkill.Flurry));Assert.IsTrue(r.HasPassive(KaitPassive.OpenHand));Assert.NotNull(r.CurrentReward);
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
    [Test] public void FullThreeHitsCannotTriggerThreeSecondaryWaves()
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,86,new YummnRulesSnapshot());r.skills.AddRange(new[]{KaitSkill.Flurry,KaitSkill.FrostBreath});r.passives.AddRange(new[]{KaitPassive.FireSnake,KaitPassive.ShatteringPalm});
        var target=new KaitEnemy{id=76,pos=r.katePos+Vector2Int.right,hp=8,maxHp=8,life=KaitEnemyLife.Active,yummnFrozen=true,frozenActions=1};
        var rear=new KaitEnemy{id=77,pos=r.katePos+Vector2Int.right*2,hp=8,maxHp=8,life=KaitEnemyLife.Active};r.enemies.Add(target);r.enemies.Add(rear);
        r.TryUseSkill(KaitSkill.Flurry,-1,out _);var result=r.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(5,target.hp);Assert.AreEqual(6,rear.hp);Assert.AreEqual(1,result.yummnEvents.Count(e=>e.kind==YummnEventKind.Hit&&e.damageCause==YummnDamageCause.FireSnake));Assert.AreEqual(1,result.yummnEvents.Count(e=>e.kind==YummnEventKind.Hit&&e.damageCause==YummnDamageCause.Shatter));
    }
}
