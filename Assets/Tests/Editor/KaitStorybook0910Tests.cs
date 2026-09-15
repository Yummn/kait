using NUnit.Framework;
using UnityEngine;

public sealed class KaitStorybook0910Tests
{
    [Test] public void FloatingPaperNeverCutsItsContentButWorldStillSplits()
    {
        var root=new GameObject("root",typeof(RectTransform),typeof(GlobalStyleSplit));
        var card=new GameObject("card",typeof(RectTransform),typeof(KaitUnifiedPaper));
        try
        {
            var rt=(RectTransform)root.transform;rt.sizeDelta=new Vector2(1920,1080);
            var split=root.GetComponent<GlobalStyleSplit>();split.Configure(rt,.425f,.609f);
            split.GetLocalSplits(rt,out float worldBottom,out float worldTop);
            Assert.AreEqual(.425f,worldBottom,.001f);Assert.AreEqual(.609f,worldTop,.001f);
            card.transform.SetParent(root.transform,false);
            split.GetLocalSplits((RectTransform)card.transform,out float bottom,out float top);
            Assert.Greater(bottom,1);Assert.Greater(top,1);
        }
        finally{Object.DestroyImmediate(root);}
    }
    [Test] public void AllSelectedArtworkExistsAndPortraitsHaveTransparentPadding()
    {
        foreach(string name in new[]{"YummnPortrait","KaitPortrait","Fist","Heart","Qi","Gear","IceA","IceB","IceWall","GrassA","GrassB","GrassWall","SnowBackdrop","GrassBackdrop","LibraryBackdrop"})
            Assert.NotNull(KaitStorybookArt.Load(name),name);
    }
    [Test] public void FoldedActiveAndPassiveCardsHideIllustrationsAndCenterTitles()
    {
        var root=new GameObject("root",typeof(RectTransform));
        try
        {
            var rt=(RectTransform)root.transform;rt.sizeDelta=new Vector2(1920,1080);
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var active=KaitSkillCard.Create(rt,null,font,null,null,null,null,null);
            active.Show(KaitSkill.SwiftBoots,false,Vector2.zero,0);
            var passive=KaitPassiveCard.Create(rt,null,font,null,null,null,null);
            passive.Show(KaitPassive.BirdEye,false,Vector2.zero,0);
            foreach(var card in new[]{active.gameObject,passive.gameObject})
            {
                Assert.IsFalse(card.GetComponentInChildren<KaitCardLogo>(true).gameObject.activeSelf);
                Assert.AreEqual(0,((RectTransform)card.transform.Find("Name")).anchoredPosition.x);
            }
        }
        finally{Object.DestroyImmediate(root);}
    }
}
