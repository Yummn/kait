using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class YummnComicTutorialTests
{
    [Test]
    public void AppendixUsesCurrentSettingsWithoutDuplicatingOldPages()
    {
        var normal=YummnComicTutorial.Appendix(YummnRulesSnapshot.Current());
        StringAssert.Contains("同一操作满足多项，也只推进一次",normal);
        StringAssert.Contains("同一次操作中出怪",normal);
        StringAssert.Contains("一次攻击命中多个残影也只回1气",normal);
        StringAssert.DoesNotContain("高速每击0.1气",normal);
        var alternative=YummnComicTutorial.Appendix(YummnRulesSnapshot.Current(3,YummnMovementCostMode.FixedOne,1,true,false,YummnTileSupplyMode.EffectiveMove,true,false,true,true));
        StringAssert.Contains("气上限3",alternative);
        StringAssert.Contains("高速移动固定耗1气",alternative);
        StringAssert.Contains("人物实际移动至少一格才补一枚2",alternative);
    }
    [TestCase(false)] [TestCase(true)]
    public void ThreeComicPagesAndAppendixFit(bool alternate)
    {
        var root=new GameObject("Root",typeof(RectTransform),typeof(Canvas));
        try
        {
            var rules=alternate?YummnRulesSnapshot.Current(3,YummnMovementCostMode.FixedOne,1,true,false,YummnTileSupplyMode.EffectiveMove,true,false,true,true):YummnRulesSnapshot.Current();
            var book=KaitTutorialBook.Create(root.transform,Resources.Load<Font>("NotoSansCJKsc-Regular"),null);
            book.YummnRules=rules;book.YummnMode=true;book.gameObject.SetActive(true);
            Assert.AreEqual(3,book.PageCount);
            foreach(var p in YummnComicTutorial.ForRules(rules))
            {var texture=Resources.Load<Texture2D>(p.ResourcePath);Assert.NotNull(texture);Assert.That((float)texture.width/texture.height,Is.EqualTo(3f).Within(.02f));}
            for(int i=0;i<3;i++)
            {
                book.ShowPage(i);Canvas.ForceUpdateCanvases();Assert.True(book.IllustrationLoaded);CheckText(book);
                Assert.IsNull(book.GetComponentInChildren<YummnTutorialDiagram>());
            }
            book.ShowAppendix(true);Assert.True(book.AppendixOpen);CheckText(book);
            var scroll=book.GetComponentInChildren<ScrollRect>();Assert.NotNull(scroll);
            Assert.Greater(scroll.content.rect.height,scroll.viewport.rect.height);
            var handle=scroll.verticalScrollbar.handleRect;
            Assert.AreEqual(Vector2.zero,handle.sizeDelta);
            StringAssert.Contains("每次攻击命中回1气",YummnComicTutorial.Appendix(rules));
            book.Next();Assert.AreEqual(2,book.PageIndex);Assert.False(book.AppendixOpen);
            book.YummnMode=false;Assert.AreEqual(3,book.PageCount);Assert.True(book.IllustrationLoaded);CheckText(book);
        }
        finally{Object.DestroyImmediate(root);}
    }
    static void CheckText(KaitTutorialBook book)
    {
        Canvas.ForceUpdateCanvases();
        foreach(var t in book.GetComponentsInChildren<Text>())
        {
            Assert.LessOrEqual(t.preferredHeight,t.rectTransform.rect.height+2,t.text);
            Assert.LessOrEqual(t.preferredWidth,t.rectTransform.rect.width+2,t.text);
        }
    }
}
