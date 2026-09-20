using NUnit.Framework;
using UnityEngine;

public class KaitHomeDiagonalTests
{
    [Test] public void EveryPortraitPixelHasOneOwner()
    {
        for(int y=0;y<=100;y++) for(int x=0;x<1000;x++)
        {
            var p=new Vector2(x/1000f,y/100f); int owners=0;
            for(int i=0;i<3;i++) if(KaitHomeDiagonalArt.Contains(i,p)) owners++;
            Assert.AreEqual(p.x<KaitHomeDiagonalArt.Edge(3,p.y)?1:0,owners);
        }
    }
    [Test] public void MenuRowsAreCenteredInTheirOwnSlice()
    {
        foreach(float y in new[]{346f,263f,176f,125f,35f,-57f,-157f,-250f,-343f})
        {
            float left=KaitHomeDiagonalArt.Edge(3,(y+540)/1080)*1920-960;
            float center=KaitMainMenu.MenuRowCenter(y);
            Assert.AreEqual((left+960)*.5f,center,.01f);
            Assert.Greater(center-165,left+15);
            Assert.Less(center+165,945);
        }
    }
    [Test] public void CgDoesNotReplaceBattlePortrait()
    {
        Assert.NotNull(Resources.Load<Texture2D>("KaitVisuals/CharacterSelection/ReynardCG"));
        Assert.NotNull(Resources.Load<Texture2D>("KaitVisuals/CharacterSelection/Reynard"));
    }
}
