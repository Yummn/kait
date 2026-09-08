using NUnit.Framework;
using UnityEngine;
public sealed class KaitRewardBarDragTests
{
    [Test] public void ClampKeepsWholeBarInsideViewport()
    {
        var r=new Rect(-960,-540,1920,1080);var size=new Vector2(800,76);
        Assert.AreEqual(new Vector2(552,494),KaitRewardBarDrag.Clamp(new Vector2(9999,9999),r,size));
        Assert.AreEqual(new Vector2(-552,-494),KaitRewardBarDrag.Clamp(new Vector2(-9999,-9999),r,size));
        Assert.AreEqual(new Vector2(40,-70),KaitRewardBarDrag.Clamp(new Vector2(40,-70),r,size));
    }
    [Test] public void UndersizedViewportCentersBarInsteadOfInvalidClamp()
    {Assert.AreEqual(Vector2.zero,KaitRewardBarDrag.Clamp(new Vector2(999,999),new Rect(-100,-20,200,40),new Vector2(800,76)));}
}
