using NUnit.Framework;
using UnityEngine;

public sealed class KaitMobile0912Tests
{
    [TestCase(1920,1080)] [TestCase(2146,966)] [TestCase(1821,1138)]
    public void ActualFootprintFitsWithBottomCardLane(float width,float height)
    {
        var safe=new Rect(-width/2,-height/2,width,height);
        float scale=KaitStorybookLayout.FitScale(safe);
        var origin=KaitStorybookLayout.ContentOrigin(safe,scale);
        Assert.GreaterOrEqual(origin.x-793*scale,safe.xMin+27);
        Assert.LessOrEqual(origin.x+793*scale,safe.xMax-27);
        Assert.LessOrEqual(origin.y+444*scale,safe.yMax-15);
        Assert.GreaterOrEqual(origin.y-300*scale,safe.yMin+95);
        Assert.Greater(scale,1);
    }
    [Test] public void SafeAreaAndCompactCardsStayEntirelyVisible()
    {
        var safe=KaitStorybookLayout.MapSafeRect(new Rect(-1200,-540,2400,1080),new Rect(.04f,.02f,.94f,.95f));
        Assert.AreEqual(-1104,safe.xMin,.01f);Assert.AreEqual(1152,safe.xMax,.01f);
        var local=new Rect(-safe.width/2,-safe.height/2,safe.width,safe.height);
        Assert.AreEqual(local.yMin+12,KaitSkillCard.DockY(local,false,false)-KaitSkillCard.DockReveal/2);
        Assert.AreEqual(local.yMax-12,KaitPassiveCard.DockY(local,false,false)+KaitPassiveCard.DockReveal/2);
        for(int i=0;i<6;i++)
        {
            var p=KaitPassiveDeck.HeaderDock(i,6,local);
            Assert.Greater(p.x-100,0);Assert.LessOrEqual(p.x+100,local.xMax-208);
            Assert.AreEqual(i/3,p.y);
        }
    }
}
