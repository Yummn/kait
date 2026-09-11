using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class YummnRiftDelayTests
{
    static KaitRun Run(bool exhausted)
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,911,YummnRulesSnapshot.Current(movePhase:true));
        r.enemies.Clear();r.spawns.Clear();Array.Clear(r.threat,0,r.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(r,new Vector2Int(3,3));
        if(exhausted){r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=0;}
        r.threat[1,1]=4;r.threat[2,1]=4;
        return r;
    }
    [TestCase(true)] [TestCase(false)]
    public void MergeKeepsWarningUntilLaterInput(bool exhausted)
    {
        var r=Run(exhausted);var result=r.TryGlobalInput(KaitDirection.Left);
        Assert.IsTrue(result.valid);Assert.IsTrue(result.yummnAction.enemyPhase);
        Assert.AreEqual(1,r.spawns.Count);Assert.IsEmpty(result.spawnedEnemyCells);
        var cell=r.spawns.Single().targetCell;
        var next=r.TryYummnWait();Assert.IsEmpty(r.spawns);
        CollectionAssert.Contains(next.spawnedEnemyCells,cell);
        Assert.IsFalse(next.yummnEvents.Any(e=>e.kind==YummnEventKind.EnemyAttack));
    }
    [Test] public void ExistingRiftCanSpawnWhileNewOneWaits()
    {
        var r=Run(true);r.spawns.Add(new KaitSpawnRequest{tier=1,createdTurn=-1,targetCell=new Vector2Int(5,4),sourceThreatCell=new Vector2Int(4,3)});
        var result=r.TryGlobalInput(KaitDirection.Left);
        Assert.AreEqual(1,result.spawnedEnemyCells.Count);CollectionAssert.Contains(result.spawnedEnemyCells,new Vector2Int(5,4));
        Assert.AreEqual(1,r.spawns.Count);
    }
    [Test] public void RepeatedCheckInSameInputCannotConsumeNewWarning()
    {
        var r=Run(true);var request=new KaitSpawnRequest{tier=1,createdTurn=r.turn,targetCell=new Vector2Int(4,4),sourceThreatCell=new Vector2Int(3,3)};r.spawns.Add(request);
        var resolve=typeof(KaitRun).GetMethod("ResolveYummnRifts",BindingFlags.Instance|BindingFlags.NonPublic);
        for(int i=0;i<3;i++)resolve.Invoke(r,new object[]{new KaitTurnResult()});
        CollectionAssert.Contains(r.spawns,request);Assert.IsEmpty(r.enemies);
    }
}
