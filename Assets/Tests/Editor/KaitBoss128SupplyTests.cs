using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class KaitBoss128SupplyTests
{
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static KaitRun Run(bool supply=true,KaitCharacter character=KaitCharacter.Kait)
    {
        var r=new KaitRun(new KaitBalanceConfig{playerInvincible=true,kaitEffectiveMoveSupply=supply,enableThreatPillars=false});
        r.SelectCharacter(character,12812,YummnRulesSnapshot.Current());
        r.enemies.Clear();r.spawns.Clear();Array.Clear(r.threat,0,r.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(r,new Vector2Int(1,3));
        return r;
    }
    [TestCase(KaitCharacter.Kait)] [TestCase(KaitCharacter.Yummn)]
    public void Merge128SpawnsBossWithoutNormalRift(KaitCharacter character)
    {
        var r=Run(character:character);r.threat[0,0]=64;r.threat[1,0]=64;
        var result=r.TryGlobalInput(KaitDirection.Right);
        Assert.IsTrue(r.bossSpawned);Assert.IsFalse(r.ended);
        Assert.AreEqual(1,r.enemies.Count(e=>e.type==KaitEnemyType.ShieldKnight));
        Assert.AreEqual(0,r.spawns.Count);
    }
    [Test] public void OldBalanceStillUses256()
    {
        var r=Run();r.config.winValue=256;
        var method=typeof(KaitRun).GetMethod("HandleMilestoneMerge",Hidden);
        method.Invoke(r,new object[]{new KaitMergeEvent{resultValue=128,threatCell=Vector2Int.zero}});
        Assert.IsFalse((bool)typeof(KaitRun).GetField("bossPending",Hidden).GetValue(r));
        method.Invoke(r,new object[]{new KaitMergeEvent{resultValue=256,threatCell=Vector2Int.zero}});
        Assert.IsTrue((bool)typeof(KaitRun).GetField("bossPending",Hidden).GetValue(r));
    }
    [TestCase(true,0)] [TestCase(false,1)]
    public void ThreatOnlyMovementRespectsOption(bool enabled,int expected)
    {
        var r=Run(enabled);r.threat[3,0]=2;
        var result=r.TryGlobalInput(KaitDirection.Left);
        Assert.IsTrue(result.valid);Assert.IsTrue(result.kaitWaited);
        Assert.AreEqual(expected,result.newThreatCells.Count);
    }
    [Test] public void NormalSlideSuppliesOne()
    {var r=Run();Assert.AreEqual(1,r.TryGlobalInput(KaitDirection.Right).newThreatCells.Count);}
    [Test] public void ChainNeverSuppliesUntilItEnds()
    {
        var r=Run();r.enemies.Add(new KaitEnemy{id=1,pos=new Vector2Int(3,3),hp=1,maxHp=1,type=KaitEnemyType.Grunt,life=KaitEnemyLife.Active});
        var hit=r.TryGlobalInput(KaitDirection.Right);
        Assert.IsTrue(hit.awaitingTurnChoice);Assert.AreEqual(0,hit.newThreatCells.Count);
        Assert.AreEqual(1,r.ContinueChain(KaitDirection.Left).newThreatCells.Count);
    }
    [Test] public void SettingsAndNewBossThresholdSurviveReplay()
    {
        var r=new KaitRun(new KaitBalanceConfig{kaitEffectiveMoveSupply=true});r.Reset(128);
        var copy=new KaitRun();Assert.IsTrue(copy.RestoreReplay(r.SaveReplay()));
        Assert.IsTrue(copy.config.kaitEffectiveMoveSupply);Assert.AreEqual(128,copy.config.winValue);
    }
}
