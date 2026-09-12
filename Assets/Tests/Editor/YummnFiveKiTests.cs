using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class YummnFiveKiTests
{
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static KaitRun R(bool one=false)
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,912,new YummnRulesSnapshot(killKiOne:one));
        r.enemies.Clear();r.spawns.Clear();Array.Clear(r.threat,0,r.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(r,new Vector2Int(2,3));return r;
    }
    static KaitEnemy E(KaitRun r,int x,int y,int hp=1,KaitEnemyType type=KaitEnemyType.Grunt)
    {var e=new KaitEnemy{id=900+r.enemies.Count,pos=new Vector2Int(x,y),hp=hp,maxHp=hp,type=type,life=KaitEnemyLife.Active};r.enemies.Add(e);return e;}
    [Test] public void NewRunStartsFullAndFifthNonkillActionEntersExhaustion()
    {
        var r=R();Assert.AreEqual(5,r.Ki);Assert.AreEqual(5,r.Yummn.profile.maxKi);Assert.AreEqual(2,r.Yummn.profile.killKi);
        for(int i=0;i<5;i++){Assert.IsTrue(r.TryGlobalInput(i%2==0?KaitDirection.Right:KaitDirection.Left).valid);Assert.AreEqual(4-i,r.Ki);Assert.AreEqual(i<4?YummnPhase.Burst:YummnPhase.Exhausted,r.KiPhase);}
        Assert.AreEqual(0,r.EnemyResolveCount);
    }
    [Test] public void RecoveryRequiresFivePhasesAndFinalStepStillWalks()
    {
        var r=R();r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=0;
        for(int i=1;i<=5;i++){var a=r.TryGlobalInput(i%2==1?KaitDirection.Right:KaitDirection.Left);Assert.AreEqual(1,a.katePath.Count);Assert.AreEqual(i,r.Ki);Assert.AreEqual(i<5?YummnPhase.Exhausted:YummnPhase.Burst,r.KiPhase);Assert.AreEqual(i,r.EnemyResolveCount);}
        Assert.Greater(r.TryGlobalInput(KaitDirection.Right).katePath.Count,1);Assert.AreEqual(4,r.Ki);Assert.AreEqual(5,r.EnemyResolveCount);
    }
    [TestCase(false,3)] [TestCase(true,2)] public void UserLastHitExampleDoesNotImmediatelyLeaveExhaustion(bool one,int expected)
    {
        var r=R(one);r.Yummn.ki=1;var e=E(r,3,3,2);
        r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,e.hp);Assert.AreEqual(0,r.Ki);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);
        r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(expected,r.Ki);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);Assert.AreEqual(1,r.EnemyResolveCount);
    }
    [TestCase(false,false,2)] [TestCase(true,false,1)] [TestCase(false,true,3)] [TestCase(true,true,2)]
    public void DirectKillUsesSelectedAmountInBothPhases(bool one,bool exhausted,int expected)
    {
        var r=R(one);r.Yummn.ki=exhausted?0:1;r.Yummn.phase=exhausted?YummnPhase.Exhausted:YummnPhase.Burst;E(r,3,3);
        var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(expected,r.Ki);Assert.AreEqual(one?1:2,r.Yummn.metrics.killKi);Assert.AreEqual(1,a.newThreatCells.Count);
    }
    [TestCase(false,4)] [TestCase(true,2)] public void SecondaryKillsRewardEachEnemyNotOncePerAction(bool one,int expected)
    {
        var r=R(one);r.Yummn.ki=1;E(r,3,3);E(r,4,3);r.passives.Add(KaitPassive.FireSnake);
        var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,r.kills);Assert.AreEqual(expected,r.Ki);Assert.AreEqual(2,a.newThreatCells.Count);Assert.AreEqual(1,r.EnemyResolveCount);
    }
    [TestCase(false,3)] [TestCase(true,2)] public void CounterUsesSameKillSettingAndKeepsWalkSupply(bool one,int expected)
    {
        var r=R(one);r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=0;r.passives.Add(KaitPassive.Opportunist);E(r,5,3,1,KaitEnemyType.Swordsman);
        var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,r.kills);Assert.AreEqual(expected,r.Ki);Assert.AreEqual(2,a.newThreatCells.Count);Assert.AreEqual(1,r.EnemyResolveCount);
    }
    [TestCase(false)] [TestCase(true)] public void KillAndRecoveryClampAtFive(bool one)
    {
        var r=R(one);r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=4;E(r,3,3);
        r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(5,r.Ki);Assert.AreEqual(YummnPhase.Burst,r.KiPhase);Assert.AreEqual(0,r.Yummn.metrics.recoveryKi);
    }
    [Test] public void PerfectSelfAndWholenessUseFiveKiThreshold()
    {
        var r=R();r.Yummn.ki=1;r.passives.AddRange(new[]{KaitPassive.PerfectSelf,KaitPassive.Wholeness});
        typeof(KaitRun).GetProperty("kateHp").SetValue(r,1);
        r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,r.Ki);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);
        for(int i=0;i<4;i++){r.TryGlobalInput(i%2==0?KaitDirection.Left:KaitDirection.Right);Assert.AreEqual(i==3?2:1,r.kateHp);}
        Assert.AreEqual(5,r.Ki);Assert.AreEqual(1,r.Yummn.metrics.heals);
    }
    [TestCase(false)] [TestCase(true)] public void FiveKiReplayPreservesSettingsAndState(bool one)
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,912,new YummnRulesSnapshot(killKiOne:one));
        for(int i=0;i<20&&!r.ended;i++)r.TryGlobalInput((KaitDirection)(i%4));
        var copy=new KaitRun();Assert.IsTrue(copy.RestoreReplay(r.SaveReplay()));Assert.AreEqual(5,copy.Yummn.profile.maxKi);Assert.AreEqual(one?1:2,copy.Yummn.profile.killKi);
        Assert.AreEqual(r.Ki,copy.Ki);Assert.AreEqual(r.KiPhase,copy.KiPhase);CollectionAssert.AreEqual(r.threat,copy.threat);Assert.AreEqual(r.SaveReplay(),copy.SaveReplay());
    }
    [Test] public void OldSavesWithoutNewFieldsKeepThreeKiAndOldScoreKey()
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,912,new YummnRulesSnapshot(fiveKi:false));
        for(int i=0;i<9&&!r.ended;i++)r.TryGlobalInput((KaitDirection)(i%4));
        var save=r.SaveReplay().Replace(",\"fiveKi\":false","").Replace(",\"killKiOne\":false","");
        var copy=new KaitRun();Assert.IsTrue(copy.RestoreReplay(save));Assert.AreEqual(3,copy.Yummn.profile.maxKi);Assert.AreEqual(2,copy.Yummn.profile.killKi);
        Assert.AreEqual(r.Ki,copy.Ki);CollectionAssert.AreEqual(r.threat,copy.threat);Assert.AreEqual(r.ScoreRulesKey,copy.ScoreRulesKey);
        copy.SelectCharacter(KaitCharacter.Yummn,912,new YummnRulesSnapshot());Assert.AreEqual(5,copy.Ki);
        copy.SelectCharacter(KaitCharacter.Yummn,912,YummnRulesSnapshot.OldV08());Assert.AreEqual(3,copy.Ki);
        Assert.AreEqual(3,new[]{new YummnRulesSnapshot(fiveKi:false),new YummnRulesSnapshot(),new YummnRulesSnapshot(killKiOne:true)}.Select(s=>s.ScoreKey).Distinct().Count());
    }
    [Test] public void SettingsFilterByCharacterAndKillCycleOnlyAffectsNextRun()
    {
        const string key="Kait.Yummn082.KillKi";bool existed=PlayerPrefs.HasKey(key);int before=PlayerPrefs.GetInt(key);
        var root=new GameObject("Ki settings",typeof(RectTransform),typeof(Canvas));root.SetActive(false);
        try
        {
            PlayerPrefs.SetInt(key,3);var g=root.AddComponent<KaitGame>();var font=Resources.Load<Font>("NotoSansCJKsc-Regular");
            typeof(KaitGame).GetField("uiFont",Hidden).SetValue(g,font);typeof(KaitGame).GetField("threatBoardFont",Hidden).SetValue(g,font);
            var r=(KaitRun)typeof(KaitGame).GetField("run",Hidden).GetValue(g);r.SelectCharacter(KaitCharacter.Yummn,912,new YummnRulesSnapshot());
            typeof(KaitGame).GetMethod("BuildSettingsOverlay",Hidden).Invoke(g,new object[]{root.transform});
            var button=root.GetComponentsInChildren<Button>(true).Single(b=>b.name=="Yummn Rule KillKi");
            var current=r.SaveReplay();button.onClick.Invoke();button.onClick.Invoke();Assert.AreEqual(current,r.SaveReplay());Assert.AreEqual(2,r.Yummn.profile.killKi);
            var preset=typeof(KaitGame).GetMethod("YummnPreset",BindingFlags.Static|BindingFlags.NonPublic);
            Assert.AreEqual(1,((YummnRulesSnapshot)preset.Invoke(null,null)).KillKi);
            foreach(var c in new[]{KaitCharacter.Yummn,KaitCharacter.Kait,KaitCharacter.Yummn})
            {
                r.SelectCharacter(c,912);typeof(KaitGame).GetMethod("RefreshCharacterSettings",Hidden).Invoke(g,null);
                var visible=root.GetComponentsInChildren<Toggle>(true).Where(t=>t.gameObject.activeSelf).ToArray();Assert.AreEqual(c==KaitCharacter.Yummn?8:7,visible.Length);
                Assert.AreEqual(c==KaitCharacter.Yummn,button.gameObject.activeSelf);
                Assert.AreEqual(c==KaitCharacter.Kait,((Toggle)typeof(KaitGame).GetField("disableThreatPillarsToggle",Hidden).GetValue(g)).gameObject.activeSelf);
                var rects=visible.Select(t=>(RectTransform)t.transform).OrderByDescending(t=>t.anchoredPosition.y).ToArray();
                for(int i=1;i<rects.Length;i++)Assert.Greater(rects[i-1].anchoredPosition.y-rects[i-1].rect.height/2,rects[i].anchoredPosition.y+rects[i].rect.height/2);
                foreach(var t in root.GetComponentsInChildren<Text>(true).Where(t=>t.gameObject.activeSelf))Assert.LessOrEqual(t.preferredHeight,t.rectTransform.rect.height+2,t.text);
            }
            button.onClick.Invoke();Assert.AreEqual(3,((YummnRulesSnapshot)preset.Invoke(null,null)).KillKi);
        }
        finally{UnityEngine.Object.DestroyImmediate(root);if(existed)PlayerPrefs.SetInt(key,before);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();}
    }
    [Test] public void FivePipsFitHudAndOldSaveOnlyShowsThree()
    {
        var root=new GameObject("Ki HUD",typeof(RectTransform));root.SetActive(false);
        try
        {
            var g=root.AddComponent<KaitGame>();var turn=new GameObject("Turn",typeof(RectTransform),typeof(Text));turn.transform.SetParent(root.transform,false);
            typeof(KaitGame).GetField("turnText",Hidden).SetValue(g,turn.GetComponent<Text>());
            typeof(KaitGame).GetField("uiFont",Hidden).SetValue(g,Resources.Load<Font>("NotoSansCJKsc-Regular"));
            var r=(KaitRun)typeof(KaitGame).GetField("run",Hidden).GetValue(g);
            foreach(bool five in new[]{true,false,true})
            {
                r.SelectCharacter(KaitCharacter.Yummn,912,new YummnRulesSnapshot(fiveKi:five));
                typeof(KaitGame).GetMethod("RefreshYummnKiDisplay",Hidden).Invoke(g,null);
                var pips=(Image[])typeof(KaitGame).GetField("actionPips",Hidden).GetValue(g);
                var visible=pips.Where(p=>p.gameObject.activeSelf).ToArray();Assert.AreEqual(five?5:3,visible.Length);
                foreach(var p in visible){Assert.Greater(p.rectTransform.anchoredPosition.x-p.rectTransform.rect.width/2,-33);Assert.Less(p.rectTransform.anchoredPosition.x+p.rectTransform.rect.width/2,125);}
            }
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
    [TestCase(false)] [TestCase(true)] public void TutorialReflectsActualRunNotNextPreference(bool one)
    {var pages=YummnTutorial.ForRules(new YummnRulesSnapshot(killKiOne:one));StringAssert.Contains("气上限5",pages[0].Lead);StringAssert.Contains(one?"击杀+1气":"击杀+2气",pages[0].Lead);StringAssert.Contains("回满5气",pages[0].Tip);StringAssert.Contains("气上限3",YummnTutorial.ForRules(new YummnRulesSnapshot(fiveKi:false))[0].Lead);}
}
