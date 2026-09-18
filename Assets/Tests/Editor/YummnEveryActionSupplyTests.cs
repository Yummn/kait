using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class YummnEveryActionSupplyTests
{
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static KaitRun R(bool exhausted=false)
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,911,new YummnRulesSnapshot(YummnTileSupplyMode.EveryAction));
        r.enemies.Clear();r.spawns.Clear();Array.Clear(r.threat,0,r.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(r,new Vector2Int(1,3));
        if(exhausted){r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=0;}return r;
    }
    static void Enemy(KaitRun r,int x,int hp)=>r.enemies.Add(new KaitEnemy{id=100+r.enemies.Count,pos=new Vector2Int(x,3),hp=hp,maxHp=hp,life=KaitEnemyLife.Active});
    [TestCase(false)] [TestCase(true)] public void EveryWalkSuppliesOneNotTwo(bool exhausted)
    {var r=R(exhausted);r.passives.Add(KaitPassive.PassWithoutTrace);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,a.yummnAction.requestedTwos);Assert.AreEqual(1,a.newThreatCells.Count);Assert.IsFalse(a.yummnAction.suppressedTwo);Assert.AreEqual(exhausted?1:0,r.EnemyResolveCount);}
    [TestCase(false)] [TestCase(true)] public void StationaryNonlethalPunchSupplies(bool exhausted)
    {var r=R(exhausted);Enemy(r,2,5);var a=r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(a.yummnAction.stationaryPunch);Assert.AreEqual(1,a.newThreatCells.Count);Assert.AreEqual(exhausted?1:0,r.EnemyResolveCount);}
    [Test] public void MultiKillStillSuppliesOneAndOneEnemyPhase()
    {var r=R();Enemy(r,2,1);Enemy(r,3,1);r.passives.Add(KaitPassive.FireSnake);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,r.kills);Assert.AreEqual(1,a.newThreatCells.Count);Assert.AreEqual(1,r.EnemyResolveCount);}
    [Test] public void ValidThreatOnlyMovementAlsoSuppliesButInvalidDoesNot()
    {var r=R();Assert.IsFalse(r.TryGlobalInput(KaitDirection.Left).valid);Assert.AreEqual(0,r.NormalTileSpawnCount);r.threat[2,2]=2;var a=r.TryGlobalInput(KaitDirection.Left);Assert.IsTrue(a.valid);Assert.AreEqual(0,a.katePath.Count);Assert.AreEqual(1,a.newThreatCells.Count);}
    [Test] public void FullBoardDropsOneAndStaysIdleWithoutLoss()
    {var r=R();for(int x=0;x<5;x++)for(int y=0;y<5;y++)if(!r.threatPillars[x,y])r.threat[x,y]=(x+y)%2==0?2:4;var before=(int[,])r.threat.Clone();var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,a.yummnAction.droppedTwos);Assert.IsFalse(r.ended);CollectionAssert.AreEqual(before,r.threat);}
    [Test] public void NewModeHasIndependentReplayAndScoreKey()
    {var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,911,new YummnRulesSnapshot(YummnTileSupplyMode.EveryAction));for(int i=0;i<8&&!r.ended;i++)r.TryGlobalInput((KaitDirection)(i%4));var copy=new KaitRun();Assert.IsTrue(copy.RestoreReplay(r.SaveReplay()));Assert.AreEqual(YummnTileSupplyMode.EveryAction,copy.Yummn.rules.Supply);CollectionAssert.AreEqual(r.threat,copy.threat);Assert.AreEqual(r.SaveReplay(),copy.SaveReplay());Assert.AreEqual(3,Enum.GetValues(typeof(YummnTileSupplyMode)).Cast<YummnTileSupplyMode>().Where(m=>m!=YummnTileSupplyMode.EffectiveMove).Select(m=>new YummnRulesSnapshot(m).ScoreKey).Distinct().Count());}
    [Test] public void CurrentSettingsKeepOneConsistentlyWordedSupplySelectorAndFitPanel()
    {
        string key="Kait.Yummn082.SupplyMode";bool existed=PlayerPrefs.HasKey(key);int value=PlayerPrefs.GetInt(key);
        var root=new GameObject("Settings test",typeof(RectTransform),typeof(Canvas));root.SetActive(false);
        try
        {
            PlayerPrefs.SetInt(key,(int)YummnTileSupplyMode.EveryAction);
            var g=root.AddComponent<KaitGame>();var font=Resources.Load<Font>("NotoSansCJKsc-Regular");
            typeof(KaitGame).GetField("uiFont",Hidden).SetValue(g,font);typeof(KaitGame).GetField("threatBoardFont",Hidden).SetValue(g,font);
            var r=(KaitRun)typeof(KaitGame).GetField("run",Hidden).GetValue(g);r.SelectCharacter(KaitCharacter.Yummn,911);var current=r.SaveReplay();
            typeof(KaitGame).GetMethod("BuildSettingsOverlay",Hidden).Invoke(g,new object[]{root.transform});
            var preset=typeof(KaitGame).GetMethod("YummnPreset",BindingFlags.Static|BindingFlags.NonPublic);
            Assert.AreEqual(YummnTileSupplyMode.EveryAction,((YummnRulesSnapshot)preset.Invoke(null,null)).Supply);
            Assert.AreEqual(current,r.SaveReplay());Assert.IsFalse(((YummnRulesSnapshot)preset.Invoke(null,null)).ActualMoveSupply);
            Assert.IsTrue(root.GetComponentsInChildren<Button>(true).Any(b=>b.name=="Yummn Rule SupplyMode"));
            Assert.IsTrue(root.GetComponentsInChildren<Text>(true).Any(t=>t.text.Contains("每次方向操作补一个2")));
            var toggles=root.GetComponentsInChildren<Toggle>(true).Where(t=>t.gameObject.activeSelf).Select(t=>(RectTransform)t.transform).OrderByDescending(t=>t.anchoredPosition.y).ToArray();Assert.AreEqual(5,toggles.Length);
            for(int i=1;i<toggles.Length;i++)Assert.GreaterOrEqual(toggles[i-1].anchoredPosition.y-toggles[i-1].rect.height/2,toggles[i].anchoredPosition.y+toggles[i].rect.height/2);
        }
        finally{UnityEngine.Object.DestroyImmediate(root);if(existed)PlayerPrefs.SetInt(key,value);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();}
    }
    [Test] public void NewModeTutorialFitsAndExplainsStationaryPunch()
    {
        var root=new GameObject("Tutorial test",typeof(RectTransform),typeof(Canvas));
        try
        {
            var book=KaitTutorialBook.Create(root.transform,Resources.Load<Font>("NotoSansCJKsc-Regular"),null);
            book.YummnRules=new YummnRulesSnapshot(YummnTileSupplyMode.EveryAction);book.YummnMode=true;book.gameObject.SetActive(true);book.ShowPage(3);Canvas.ForceUpdateCanvases();
            foreach(var t in book.GetComponentsInChildren<Text>()){Assert.LessOrEqual(t.preferredHeight,t.rectTransform.rect.height+2,t.text);Assert.LessOrEqual(t.preferredWidth,t.rectTransform.rect.width+2,t.text);}
            StringAssert.Contains("原地出拳",YummnTutorial.ForRules(book.YummnRules)[3].Tip);
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
}
