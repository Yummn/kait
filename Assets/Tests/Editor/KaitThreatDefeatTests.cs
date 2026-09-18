using NUnit.Framework;
using System.Reflection;

public class KaitThreatDefeatTests
{
    private KaitRun Board()
    {
        var r=new KaitRun(new KaitBalanceConfig { newThreatTilesPerTurn=0, enableThreatPillars=false });r.Reset(42);
        r.enemies.Clear();r.spawns.Clear();
        for(int x=0;x<r.ThreatSize;x++)for(int y=0;y<r.ThreatSize;y++)r.threat[x,y]=(x+y)%2==0?2:4;
        return r;
    }
    private void Finish(KaitRun r) => typeof(KaitRun).GetMethod("FinishTurn",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(r,new object[]{new KaitTurnResult()});
    [Test] public void LockedBoardStaysIdleAndRunContinues()
    {
        var r=Board();var before=(int[,])r.threat.Clone();Finish(r);
        Assert.IsFalse(r.ended);Assert.IsFalse(r.won);Assert.AreEqual(string.Empty,r.endReason);
        CollectionAssert.AreEqual(before,r.threat);Assert.AreEqual(0,r.threatLocks);
    }
    [Test] public void MergeOrEmptyCellStillAllowsPlay()
    {
        var r=Board();r.threat[0,0]=4;Finish(r);Assert.IsFalse(r.ended);
        r=Board();r.threat[0,0]=0;Finish(r);Assert.IsFalse(r.ended);
    }
}
