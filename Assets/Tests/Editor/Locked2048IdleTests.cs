using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class Locked2048IdleTests
{
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;

    static void Lock(KaitRun run)
    {
        for(int x=0;x<run.ThreatSize;x++)for(int y=0;y<run.ThreatSize;y++)
            if(!run.threatPillars[x,y])run.threat[x,y]=(x+y)%2==0?2:4;
        Assert.IsTrue(run.IsYummn?run.IsYummnThreatLocked():(bool)typeof(KaitRun).GetMethod("ThreatLocked",Hidden).Invoke(run,null));
    }

    static KaitRun Yummn(YummnRulesSnapshot rules)
    {
        var run=new KaitRun();run.SelectCharacter(KaitCharacter.Yummn,916,rules);
        run.enemies.Clear();run.spawns.Clear();Array.Clear(run.threat,0,run.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(2,3));
        Lock(run);return run;
    }

    [Test] public void KaitLockDoesNotEndOrChangeBoard()
    {
        var run=new KaitRun(new KaitBalanceConfig{enableThreatPillars=false,newThreatTilesPerTurn=0});
        run.SelectCharacter(KaitCharacter.Kait,916);Lock(run);var before=(int[,])run.threat.Clone();
        typeof(KaitRun).GetMethod("FinishTurn",Hidden).Invoke(run,new object[]{new KaitTurnResult()});
        Assert.IsFalse(run.ended);Assert.AreEqual(string.Empty,run.endReason);CollectionAssert.AreEqual(before,run.threat);
    }

    [Test] public void CurrentYummnLockDoesNotEndOrChangeBoard()
    {
        var run=Yummn(YummnRulesSnapshot.Current());var before=(int[,])run.threat.Clone();
        Assert.IsTrue(run.TryGlobalInput(KaitDirection.Right).valid);
        Assert.IsFalse(run.ended);Assert.AreEqual(string.Empty,run.endReason);CollectionAssert.AreEqual(before,run.threat);
    }

    [Test] public void WaitingOnLockedBoardStillAdvancesCombatOnly()
    {
        var run=Yummn(YummnRulesSnapshot.Current());var before=(int[,])run.threat.Clone();int turn=run.turn;
        Assert.IsTrue(run.TryYummnWait().valid);
        Assert.IsFalse(run.ended);Assert.AreEqual(turn+1,run.turn);CollectionAssert.AreEqual(before,run.threat);
    }

    [Test] public void LegacyYummnLockNoLongerResetsBoard()
    {
        var run=Yummn(YummnRulesSnapshot.OldV08());var before=(int[,])run.threat.Clone();
        Assert.IsTrue(run.TryGlobalInput(KaitDirection.Right).valid);
        Assert.IsFalse(run.ended);Assert.AreEqual(0,run.threatLocks);CollectionAssert.AreEqual(before,run.threat);
    }
}
