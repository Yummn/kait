using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class KaitTutorialTests
{
    [Test]
    public void CharacterSwitchRefreshesHiddenBookBeforeItsFirstOpen()
    {
        var root=new GameObject("Root",typeof(RectTransform),typeof(Canvas));
        try
        {
            var book=KaitTutorialBook.Create(root.transform,Resources.Load<Font>("NotoSansCJKsc-Regular"),null);
            book.ShowPage(8);
            book.YummnMode=true;
            book.gameObject.SetActive(true);
            Assert.AreEqual(0,book.PageIndex);
            Assert.AreEqual(4,book.PageCount);
            var text=string.Join("\n",System.Array.ConvertAll(book.GetComponentsInChildren<Text>(),t=>t.text));
            StringAssert.Contains("Yummn · 玩法图解",text);
            StringAssert.Contains("耗气进攻，回满再爆发",text);
            StringAssert.Contains("1 / 4",text);
            StringAssert.DoesNotContain("一条指令，两盘行动",text);
            book.gameObject.SetActive(false);
            book.YummnMode=false;
            book.gameObject.SetActive(true);
            Assert.AreEqual(3,book.PageCount);
            Assert.IsFalse(book.GetComponentInChildren<YummnTutorialDiagram>() != null);
            Assert.IsTrue(book.IllustrationLoaded);
        }
        finally { Object.DestroyImmediate(root); }
    }

    [TestCase("NotoSansCJKsc-Regular")]
    [TestCase("Fonts/FusionPixel12pxProportionalZhHans")]
    public void YummnPagesFitWithoutShrinkingText(string fontPath)
    {
        var root=new GameObject("Root",typeof(RectTransform),typeof(Canvas));
        try
        {
            var book=KaitTutorialBook.Create(root.transform,Resources.Load<Font>(fontPath),null);
            book.YummnMode=true;book.gameObject.SetActive(true);
            for(int page=0;page<book.PageCount;page++)
            {
                book.ShowPage(page);Canvas.ForceUpdateCanvases();
                foreach(var text in book.GetComponentsInChildren<Text>())
                {
                    Assert.LessOrEqual(text.preferredHeight,text.rectTransform.rect.height+2,$"Yummn page {page+1}: {text.text}");
                    Assert.LessOrEqual(text.preferredWidth,text.rectTransform.rect.width+2,$"Yummn page {page+1}: {text.text}");
                }
            }
        }
        finally { Object.DestroyImmediate(root); }
    }

    [Test]
    public void EveryPageFitsItsTextBoxesAtFullSize()
    {
        var root=new GameObject("Root",typeof(RectTransform),typeof(Canvas));
        try
        {
            var font=Resources.Load<Font>("NotoSansCJKsc-Regular");
            Assert.IsNotNull(font);
            var book=KaitTutorialBook.Create(root.transform,font,null);
            book.gameObject.SetActive(true);
            for(int i=0;i<book.PageCount;i++)
            {
                book.ShowPage(i); Canvas.ForceUpdateCanvases();
                foreach(var text in book.GetComponentsInChildren<Text>())
                {
                    Assert.LessOrEqual(text.preferredHeight,text.rectTransform.rect.height+2,$"Page {i+1}: {text.text}");
                    Assert.LessOrEqual(text.preferredWidth,text.rectTransform.rect.width+2,$"Page {i+1} width: {text.text}");
                }
            }
        }
        finally { Object.DestroyImmediate(root); }
    }

    [Test]
    public void TenChaptersHaveUniqueIllustrationsAndShortNativeText()
    {
        Assert.AreEqual(10,KaitTutorialPages.All.Length);
        var ids=new HashSet<string>();
        foreach(var p in KaitTutorialPages.All)
        {
            Assert.IsTrue(ids.Add(p.Id));
            var texture=Resources.Load<Texture2D>(p.ResourcePath);
            Assert.IsNotNull(texture,p.ResourcePath);
            Assert.AreEqual(1536,texture.width); Assert.AreEqual(1024,texture.height);
            Assert.Less(p.Body.Length,200); Assert.IsNotEmpty(p.Tip);
        }
    }

    [Test]
    public void NavigationClampsAndFinalActionClosesWithoutLosingPage()
    {
        var root=new GameObject("Root",typeof(RectTransform),typeof(Canvas));
        try
        {
            var book=KaitTutorialBook.Create(root.transform,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null);
            Assert.IsFalse(book.gameObject.activeSelf);
            book.gameObject.SetActive(true); book.Previous(); Assert.AreEqual(0,book.PageIndex);
            book.Next(); Assert.AreEqual(1,book.PageIndex);
            book.ShowPage(999); Assert.AreEqual(book.PageCount-1,book.PageIndex);
            Assert.IsTrue(book.IllustrationLoaded);
            book.Next(); Assert.IsFalse(book.gameObject.activeSelf);
            book.gameObject.SetActive(true); Assert.AreEqual(book.PageCount-1,book.PageIndex);
            book.ShowPage(-10); Assert.AreEqual(0,book.PageIndex);
            foreach(var button in book.GetComponentsInChildren<Button>())
                Assert.AreEqual(Navigation.Mode.None,button.navigation.mode);
            Assert.IsTrue(book.GetComponent<Image>().raycastTarget);
        }
        finally { Object.DestroyImmediate(root); }
    }

    [Test]
    public void HorizontalSwipeTurnsOnePageAndVerticalOrShortSwipesDoNot()
    {
        var root=new GameObject("Root",typeof(RectTransform),typeof(Canvas));
        var events=new GameObject("Events",typeof(EventSystem));
        try
        {
            var book=KaitTutorialBook.Create(root.transform,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null);
            book.gameObject.SetActive(true);
            var pointer=new PointerEventData(events.GetComponent<EventSystem>()){position=new Vector2(400,400)};
            book.OnBeginDrag(pointer); pointer.position=new Vector2(200,400); book.OnEndDrag(pointer);
            Assert.AreEqual(1,book.PageIndex);
            book.OnBeginDrag(pointer); pointer.position=new Vector2(190,400); book.OnEndDrag(pointer);
            Assert.AreEqual(1,book.PageIndex);
            book.OnBeginDrag(pointer); pointer.position=new Vector2(200,600); book.OnEndDrag(pointer);
            Assert.AreEqual(1,book.PageIndex);
            book.OnBeginDrag(pointer); pointer.position=new Vector2(400,600); book.OnEndDrag(pointer);
            Assert.AreEqual(0,book.PageIndex);
        }
        finally { Object.DestroyImmediate(root); Object.DestroyImmediate(events); }
    }
}
