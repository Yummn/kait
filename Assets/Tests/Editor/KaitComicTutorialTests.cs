using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitComicTutorialTests
{
    [Test]
    public void ThreePagesDetailsAndCharacterSwitchStaySeparate()
    {
        var root=new GameObject("Root",typeof(RectTransform),typeof(Canvas));
        try
        {
            var book=KaitTutorialBook.Create(root.transform,Resources.Load<Font>("NotoSansCJKsc-Regular"),null);
            book.gameObject.SetActive(true);
            Assert.AreEqual(3,book.PageCount);
            for(int i=0;i<3;i++)
            {
                var p=KaitComicTutorial.Pages[i];
                var tex=Resources.Load<Texture2D>(p.ResourcePath);
                Assert.NotNull(tex);Assert.That((float)tex.width/tex.height,Is.EqualTo(3f).Within(.02f));
                StringAssert.DoesNotContain("256",p.Body+p.Lead+p.LeftCaption+p.RightCaption);
                book.ShowPage(i);CheckText(book);
            }
            book.ShowAppendix(true);Assert.True(book.AppendixOpen);CheckText(book);
            var visible=string.Join("\n",System.Array.ConvertAll(book.GetComponentsInChildren<Text>(),t=>t.text));
            StringAssert.Contains("详细说明 · Kait",visible);
            StringAssert.DoesNotContain("文字附录",visible);
            StringAssert.DoesNotContain("256",visible);
            var appendix=book.GetComponentInChildren<ScrollRect>();
            Assert.Greater(appendix.content.rect.height,620);
            var appendixCopy=appendix.content.GetComponent<Text>();
            Assert.AreEqual(30,appendixCopy.fontSize);
            Assert.AreEqual(KaitStorybookTheme.Paper,appendixCopy.color);
            Assert.Greater(PerceivedBrightness(appendixCopy.color)-PerceivedBrightness(appendix.GetComponent<Image>().color),.6f);
            book.Next();Assert.AreEqual(2,book.PageIndex);Assert.False(book.AppendixOpen);
            book.YummnRules=YummnRulesSnapshot.Current();book.YummnMode=true;
            book.ShowAppendix(true);StringAssert.Contains("耗气",string.Join("\n",System.Array.ConvertAll(book.GetComponentsInChildren<Text>(),t=>t.text)));
            book.YummnMode=false;Assert.False(book.AppendixOpen);Assert.AreEqual(0,book.PageIndex);
        }
        finally {Object.DestroyImmediate(root);}
    }
    static float PerceivedBrightness(Color c) => c.r*.299f+c.g*.587f+c.b*.114f;
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
