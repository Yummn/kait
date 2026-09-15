using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class KaitStorybook0911Tests
{
    [Test] public void SnowBoardUsesSixStableSquareVariantsWithoutChangingGameplayRandom()
    {
        var variants=new HashSet<Rect>();var before=Random.state;
        for(int y=1;y<=5;y++)for(int x=1;x<=5;x++)
        {
            var tile=KaitStorybookArt.Floor(true,x,y);
            Assert.AreEqual("IceFloorAtlas",tile.texture.name);
            Assert.AreEqual(tile.rect.width,tile.rect.height,.01f);
            Assert.AreSame(tile,KaitStorybookArt.Floor(true,x,y));variants.Add(tile.rect);
        }
        Assert.AreEqual(6,variants.Count);Assert.AreEqual(before,Random.state);
    }
    [Test] public void QiUsesApprovedOriginalWispForBothHudAndCardCosts()
    {
        Assert.AreEqual("KiWispB",YummnKiWisp.Skin(0).texture.name);
        Assert.AreNotEqual(YummnKiWisp.Skin(0).rect,YummnKiWisp.Skin(2).rect);
        Assert.AreSame(YummnKiWisp.Skin(0),KaitStorybookArt.Load("Qi"));
    }
}
