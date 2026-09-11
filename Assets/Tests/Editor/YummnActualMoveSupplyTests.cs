using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class YummnActualMoveSupplyTests
{
    static KaitRun R(bool exhausted=false,bool current=true)
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,912,new YummnRulesSnapshot(YummnTileSupplyMode.SkipStationaryPunch,actualMoveSupply:current));
        r.enemies.Clear();r.spawns.Clear();Array.Clear(r.threat,0,r.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(r,new Vector2Int(1,3));
        if(exhausted){r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=3;}
        return r;
    }
    static void E(KaitRun r,int x,int hp)=>r.enemies.Add(new KaitEnemy{id=100+r.enemies.Count,pos=new Vector2Int(x,3),hp=hp,maxHp=hp,type=KaitEnemyType.Grunt,life=KaitEnemyLife.Active});
    static void Prepare(KaitRun r,KaitSkill skill){r.skills.Add(skill);Assert.IsTrue(r.TryUseSkill(skill,-1,out var message),message);}
    static void Supply(KaitTurnResult a,int count){Assert.IsTrue(a.valid);Assert.AreEqual(count,a.yummnAction.requestedTwos);Assert.AreEqual(count,a.newThreatCells.Count);}
    [TestCase(false)] [TestCase(true)] public void PlainMovementSuppliesOnce(bool exhausted)
    {var r=R(exhausted);Supply(r.TryGlobalInput(KaitDirection.Right),1);Assert.AreNotEqual(new Vector2Int(1,3),r.katePos);}
    [TestCase(false)] [TestCase(true)] public void StationaryPunchDoesNotSupply(bool exhausted)
    {var r=R(exhausted);E(r,2,9);Supply(r.TryGlobalInput(KaitDirection.Right),0);Assert.AreEqual(new Vector2Int(1,3),r.katePos);}
    [TestCase(false)] [TestCase(true)] public void KillFollowIsActualMovement(bool exhausted)
    {var r=R(exhausted);E(r,2,1);Supply(r.TryGlobalInput(KaitDirection.Right),1);Assert.AreEqual(new Vector2Int(2,3),r.katePos);}
    [TestCase(false)] [TestCase(true)] public void PalmNeedsPlayerFollowNotJustEnemyPush(bool follow)
    {var r=R();E(r,2,9);Prepare(r,KaitSkill.Palm);if(follow)r.passives.Add(KaitPassive.FollowThrough);Supply(r.TryGlobalInput(KaitDirection.Right),follow?1:0);}
    [TestCase(KaitSkill.ShapeIce)] [TestCase(KaitSkill.Darkness)] [TestCase(KaitSkill.WaterWhip)] [TestCase(KaitSkill.FrostBreath)]
    public void StationarySkillsDoNotSupply(KaitSkill skill)
    {var r=R();if(skill==KaitSkill.WaterWhip||skill==KaitSkill.FrostBreath)E(r,3,9);Prepare(r,skill);Supply(r.TryGlobalInput(KaitDirection.Right),0);Assert.AreEqual(new Vector2Int(1,3),r.katePos);}
    [Test] public void StationarySpellMultiKillStillDoesNotSupply()
    {var r=R();E(r,2,1);E(r,3,1);Prepare(r,KaitSkill.FrostBreath);Supply(r.TryGlobalInput(KaitDirection.Right),0);Assert.AreEqual(2,r.kills);}
    [Test] public void ThreatOnlyMovementDoesNotSupply()
    {var r=R();Assert.IsFalse(r.TryGlobalInput(KaitDirection.Left).valid);r.threat[2,2]=2;var a=r.TryGlobalInput(KaitDirection.Left);Supply(a,0);Assert.IsTrue(a.threatChanged);Assert.AreEqual(new Vector2Int(1,3),r.katePos);}
    [Test] public void SlideThenPunchSuppliesOnlyOnce()
    {var r=R();E(r,4,9);Supply(r.TryGlobalInput(KaitDirection.Right),1);Assert.AreEqual(new Vector2Int(3,3),r.katePos);}
    [Test] public void TeleportSuppliesOne()
    {var r=R();r.Yummn.icePillar=new Vector2Int(4,4);Prepare(r,KaitSkill.YummnShadowStep);Supply(r.TryGlobalInput(KaitDirection.Right),1);Assert.AreEqual(new Vector2Int(4,3),r.katePos);}
    [TestCase(false,0)] [TestCase(true,1)] public void TraceKeepsItsExistingPhaseException(bool exhausted,int expected)
    {var r=R(exhausted);r.passives.Add(KaitPassive.PassWithoutTrace);Supply(r.TryGlobalInput(KaitDirection.Right),expected);}
    [Test] public void OldSnapshotKeepsOldSpellAndFollowBehavior()
    {var r=R(current:false);E(r,2,1);Supply(r.TryGlobalInput(KaitDirection.Right),0);r=R(current:false);Prepare(r,KaitSkill.ShapeIce);Supply(r.TryGlobalInput(KaitDirection.Right),1);}
    [TestCase(false)] [TestCase(true)] public void ReplayPreservesRule(bool current)
    {var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,912,new YummnRulesSnapshot(YummnTileSupplyMode.SkipStationaryPunch,actualMoveSupply:current));for(int i=0;i<8&&!r.ended;i++)r.TryGlobalInput((KaitDirection)(i%4));var copy=new KaitRun();Assert.IsTrue(copy.RestoreReplay(r.SaveReplay()));Assert.AreEqual(current,copy.Yummn.rules.ActualMoveSupply);Assert.AreEqual(r.SaveReplay(),copy.SaveReplay());}
    [Test] public void MissingSnapshotFieldDoesNotMigrateOldReplay()
    {var snapshot=new YummnRulesSnapshot(YummnTileSupplyMode.SkipStationaryPunch);string json=JsonUtility.ToJson(snapshot).Replace(",\"actualMoveSupply\":true","");var old=JsonUtility.FromJson<YummnRulesSnapshot>(json);Assert.IsFalse(old.ActualMoveSupply);Assert.AreNotEqual(old.ScoreKey,snapshot.ScoreKey);Assert.AreEqual(new YummnRulesSnapshot().ScoreKey,new YummnRulesSnapshot(actualMoveSupply:false).ScoreKey);}
    [Test] public void TutorialExplainsActualMovementAndFits()
    {
        var root=new GameObject("Movement supply tutorial",typeof(RectTransform),typeof(Canvas));
        try
        {
            var rules=new YummnRulesSnapshot(YummnTileSupplyMode.SkipStationaryPunch);
            var book=KaitTutorialBook.Create(root.transform,Resources.Load<Font>("NotoSansCJKsc-Regular"),null);
            book.YummnRules=rules;book.YummnMode=true;book.gameObject.SetActive(true);book.ShowPage(3);Canvas.ForceUpdateCanvases();
            foreach(var t in book.GetComponentsInChildren<Text>()){Assert.LessOrEqual(t.preferredHeight,t.rectTransform.rect.height+2,t.text);Assert.LessOrEqual(t.preferredWidth,t.rectTransform.rect.width+2,t.text);}
            StringAssert.Contains("跟进、传送算移动",YummnTutorial.ForRules(rules)[3].Tip);
            StringAssert.DoesNotContain("跟进不算",YummnTutorial.ForRules(rules)[3].Tip);
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
}
