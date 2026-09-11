using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitCharacterSelectionTests
{
    [TestCase(1920,1080)] [TestCase(2400,1080)] [TestCase(1280,720)]
    public void SelectionFitsAndRetainsCallbacks(int width,int height)
    {
        var root=new GameObject("UI",typeof(RectTransform),typeof(Canvas));
        try
        {
            ((RectTransform)root.transform).sizeDelta=new Vector2(width,height);
            var ui=KaitCharacterSelection.Create(root.transform,Resources.Load<Font>("NotoSansCJKsc-Regular"),key=>true);
            Canvas.ForceUpdateCanvases();ui.Fit();
            Assert.AreEqual(2,ui.StartButtons.Count);Assert.AreEqual(4,ui.ContinueButtons.Count);
            KaitCharacter called=KaitCharacter.Kait;ui.StartCharacter=c=>called=c;
            ui.StartButtons[1].onClick.Invoke();Assert.AreEqual(KaitCharacter.Yummn,called);
            ui.StartButtons[0].onClick.Invoke();Assert.AreEqual(KaitCharacter.Kait,called);
            string resumed=null;ui.ContinueCharacter=k=>resumed=k;
            foreach(var button in ui.ContinueButtons){button.onClick.Invoke();Assert.IsNotEmpty(resumed);}
            foreach(var t in ui.GetComponentsInChildren<Text>())
            {Assert.LessOrEqual(t.preferredHeight,t.rectTransform.rect.height+2,t.text);Assert.LessOrEqual(t.preferredWidth,t.rectTransform.rect.width+2,t.text);}
            foreach(var c in new[]{"Kait","Yummn"}){var texture=Resources.Load<Texture2D>("KaitVisuals/CharacterSelection/"+c);Assert.NotNull(texture);Assert.AreEqual(1440,texture.width);}
            Assert.That(KaitSelectionArt.Seam(0),Is.EqualTo(.42f));Assert.That(KaitSelectionArt.Seam(1),Is.EqualTo(.58f));
        }
        finally{Object.DestroyImmediate(root);}
    }
}
