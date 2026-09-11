using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class YummnExhaustedSupplyTests
{
    static KaitRun Run(bool alt=false,int ki=0)
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,910,new YummnRulesSnapshot(alt?YummnTileSupplyMode.SkipStationaryPunch:YummnTileSupplyMode.KillOnly));
        r.enemies.Clear();r.spawns.Clear();Array.Clear(r.threat,0,r.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(r,new Vector2Int(2,3));
        r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=ki;return r;
    }
    [TestCase(false)] [TestCase(true)] public void WalkAlwaysSuppliesOnceEvenWithTrace(bool alt)
    {var r=Run(alt);r.passives.Add(KaitPassive.PassWithoutTrace);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,a.yummnAction.insertedTwos);Assert.IsFalse(a.yummnAction.suppressedTwo);Assert.AreEqual(1,r.EnemyResolveCount);Assert.AreEqual(1,r.Ki);}
    [Test] public void LastRecoveryStepStillSuppliesButFollowingBurstDoesNot()
    {var r=Run(false,4);Assert.AreEqual(1,r.TryGlobalInput(KaitDirection.Right).newThreatCells.Count);Assert.AreEqual(YummnPhase.Burst,r.KiPhase);Assert.AreEqual(0,r.TryGlobalInput(KaitDirection.Left).newThreatCells.Count);}
    [TestCase(false,1)] [TestCase(true,1)] public void KillFollowSuppliesOnceUnderEitherCurrentMode(bool alt,int expected)
    {var r=Run(alt);r.enemies.Add(new KaitEnemy{id=99,pos=new Vector2Int(3,3),hp=1,maxHp=1,life=KaitEnemyLife.Active});var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(expected,a.newThreatCells.Count);Assert.AreEqual(new Vector2Int(3,3),r.katePos);}
    [Test] public void InvalidInputDoesNotSupply()
    {var r=Run();typeof(KaitRun).GetProperty("katePos").SetValue(r,new Vector2Int(1,3));Assert.IsFalse(r.TryGlobalInput(KaitDirection.Left).valid);Assert.AreEqual(0,r.NormalTileSpawnCount);}
    [Test] public void WalkCounterKillAddsSeparateKillReward()
    {var r=Run();r.passives.Add(KaitPassive.Opportunist);r.enemies.Add(new KaitEnemy{id=99,pos=new Vector2Int(5,3),hp=1,maxHp=1,type=KaitEnemyType.Swordsman,life=KaitEnemyLife.Active});var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,r.kills);Assert.AreEqual(2,a.newThreatCells.Count);Assert.AreEqual(1,r.EnemyResolveCount);}
    [Test] public void FullBoardRecordsDroppedWalkSupply()
    {var r=Run();for(int x=0;x<5;x++)for(int y=0;y<5;y++)if(!r.threatPillars[x,y])r.threat[x,y]=(x+y)%2==0?2:4;var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,a.yummnAction.requestedTwos);Assert.AreEqual(1,a.yummnAction.droppedTwos);Assert.AreEqual("ThreatBoardLocked",r.endReason);}
    [Test] public void NewRuleSnapshotReplaysAndOldSnapshotStaysOld()
    {foreach(bool enabled in new[]{false,true}){var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,910,new YummnRulesSnapshot(moveSupply:enabled));for(int i=0;i<8;i++)r.TryGlobalInput((KaitDirection)(i%4));var copy=new KaitRun();Assert.IsTrue(copy.RestoreReplay(r.SaveReplay()));Assert.AreEqual(enabled,copy.Yummn.rules.ExhaustedMoveSupply);CollectionAssert.AreEqual(r.threat,copy.threat);Assert.AreEqual(r.ScoreRulesKey,copy.ScoreRulesKey);}}
    [Test] public void NoPairsIsNoLongerPermanentStarvation()
    {var r=Run();r.threat[1,1]=8;r.threat[2,1]=4;r.threat[3,1]=2;Assert.IsFalse(r.YummnSupplyStarved);Assert.AreEqual(1,r.TryGlobalInput(KaitDirection.Right).newThreatCells.Count);}
}
