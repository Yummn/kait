using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class KaitTutorialTests
{
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
    public void EightChaptersHaveUniqueIllustrationsAndShortNativeText()
    {
        Assert.AreEqual(8,KaitTutorialPages.All.Length);
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
            book.ShowPage(999); Assert.AreEqual(7,book.PageIndex);
            Assert.IsTrue(book.IllustrationLoaded);
            book.Next(); Assert.IsFalse(book.gameObject.activeSelf);
            book.gameObject.SetActive(true); Assert.AreEqual(7,book.PageIndex);
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
