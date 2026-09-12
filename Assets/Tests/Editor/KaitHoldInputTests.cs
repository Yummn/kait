using NUnit.Framework;
using UnityEngine;

public class KaitHoldInputTests
{
    [Test] public void RepeatWaitsForDelayAndAnimationThenDoesNotCatchUp()
    {
        var h=new KaitHoldInput();h.Begin(KaitDirection.Right,1,0);
        Assert.False(h.Poll(.3f,true,true,out _));Assert.False(h.Poll(1,true,false,out _));
        Assert.True(h.Poll(2,true,true,out var d));Assert.AreEqual(KaitDirection.Right,d);
        Assert.False(h.Poll(2,true,true,out _));Assert.True(h.Poll(2.2f,true,true,out _));
    }
    [Test] public void OffReleaseAndSourceOwnershipPreventRepeat()
    {
        var h=new KaitHoldInput();h.Begin(KaitDirection.Left,1,0);
        Assert.False(h.Poll(1,false,true,out _));h.End(2);Assert.True(h.Poll(1,true,true,out _));
        h.End(1);Assert.False(h.Poll(2,true,true,out _));
        h.Begin(KaitDirection.Up,2,3);h.Clear();Assert.False(h.Poll(5,true,true,out _));
    }
    [Test] public void ChangedDirectionRestartsHoldDelay()
    {
        var h=new KaitHoldInput();h.Begin(KaitDirection.Left,1,0);h.Begin(KaitDirection.Right,1,1);
        Assert.False(h.Poll(1.1f,true,true,out _));Assert.True(h.Poll(1.5f,true,true,out var d));Assert.AreEqual(KaitDirection.Right,d);
    }
    [Test] public void StationaryWaitOnlyFiresOnceAndAllowsSmallJitter()
    {
        var h=new KaitStationaryHold();h.Begin(Vector2.zero,0);
        Assert.False(h.Poll(Vector2.one,.4f,20,true));Assert.True(h.Poll(Vector2.one,.56f,20,true));
        Assert.False(h.Poll(Vector2.zero,5,20,true));
    }
    [Test] public void MovingOutAndReturningDoesNotBecomeWait()
    {
        var h=new KaitStationaryHold();h.Begin(Vector2.zero,0);
        Assert.False(h.Poll(new Vector2(30,0),.3f,20,true));Assert.False(h.Poll(Vector2.zero,1,20,true));
    }
    [Test] public void WaitCanStartDuringBusyButCancelNeverQueues()
    {
        var h=new KaitStationaryHold();h.Begin(Vector2.zero,0);
        Assert.False(h.Poll(Vector2.zero,1,20,false));Assert.True(h.Poll(Vector2.zero,1.1f,20,true));
        h.Begin(Vector2.zero,2);h.Cancel();Assert.False(h.Poll(Vector2.zero,5,20,true));
    }
}
