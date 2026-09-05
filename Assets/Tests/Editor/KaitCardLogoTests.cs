using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitCardLogoTests
{
    [Test]
    public void EveryCardHasAnIndependentIllustration()
    {
        foreach(KaitSkill skill in Enum.GetValues(typeof(KaitSkill)))
            if(skill != KaitSkill.None) Assert.IsNotNull(KaitCardLogo.Load(skill.ToString()),skill.ToString());
        foreach(var passive in KaitPassiveCatalog.All)
        {
            Assert.IsNotNull(KaitCardLogo.Load(passive.ToString()),passive.ToString());
            Assert.IsNotEmpty(KaitCardLogo.PassiveSymbol(passive));
        }
        Assert.IsNotNull(KaitSunlitTheme.Load("PassiveCardBlankHD"));
        Assert.IsNotNull(KaitSunlitTheme.Load("PassiveCardFlatCompact"));
        foreach (string path in System.IO.Directory.GetFiles("Assets/Resources/"+KaitCardLogo.ResourceRoot+"Transparent", "*.png"))
        {
            var importer = UnityEditor.AssetImporter.GetAtPath(path.Replace('\\','/')) as UnityEditor.TextureImporter;
            Assert.IsNotNull(importer,path);
            Assert.IsTrue(importer.DoesSourceTextureHaveAlpha(),path);
            Assert.IsTrue(importer.alphaIsTransparency,path);
        }
    }

    [Test]
    public void LogoIsVisualOnlyAndUsesCorrectSkillPair()
    {
        var root = new GameObject("Root",typeof(RectTransform));
        try
        {
            var split = root.AddComponent<GlobalStyleSplit>(); split.Configure(root.GetComponent<RectTransform>(),.5f,.5f);
            var logo = KaitCardLogo.Create(root.transform,split,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),Vector2.zero,68);
            logo.Show(KaitSkill.SwiftBoots);
            Assert.AreEqual("SwiftBoots",logo.AssetName); Assert.AreEqual("+1",logo.FlatSymbol);
            Assert.AreEqual(new Vector2(76,58),logo.FlatFrameSize);
            foreach(var graphic in logo.GetComponentsInChildren<Graphic>()) Assert.IsFalse(graphic.raycastTarget);
            Assert.AreEqual(0,logo.GetComponentsInChildren<Button>().Length);
            Assert.AreEqual(0,logo.GetComponentsInChildren<Mask>().Length);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [Test]
    public void PassiveBadgeIsACompactWideRectangle()
    {
        var root = new GameObject("Root",typeof(RectTransform));
        try
        {
            var logo = KaitCardLogo.Create(root.transform,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),Vector2.zero,92);
            logo.Show(KaitPassive.Devil);
            Assert.AreEqual(new Vector2(96,64),logo.FlatFrameSize);
            Assert.AreEqual("CD−1",logo.FlatSymbol);
            var text = logo.GetComponentInChildren<Text>();
            Assert.LessOrEqual(text.rectTransform.sizeDelta.x,logo.FlatFrameSize.x-14);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [TestCase(-200,0)]
    [TestCase(200,3)]
    [TestCase(0,3)]
    public void SimpleSymbolIsClippedToTheRightSide(float x,int expectedMinimum)
    {
        var root = new GameObject("Root",typeof(RectTransform));
        try
        {
            var area=root.GetComponent<RectTransform>(); area.sizeDelta=new Vector2(1000,1000);
            var split=root.AddComponent<GlobalStyleSplit>(); split.Configure(area,.5f,.5f);
            var label=new GameObject("Text",typeof(RectTransform),typeof(Text),typeof(SunlitSplitText));
            label.transform.SetParent(root.transform,false);
            var rect=label.GetComponent<RectTransform>(); rect.sizeDelta=new Vector2(100,100); rect.anchoredPosition=new Vector2(x,0);
            var cut=label.GetComponent<SunlitSplitText>(); cut.Configure(split); cut.SetSides(false,true);
            using(var vh=new VertexHelper())
            {
                vh.AddVert(new Vector3(-40,-40),Color.white,Vector2.zero);
                vh.AddVert(new Vector3(40,-40),Color.white,Vector2.zero);
                vh.AddVert(new Vector3(0,40),Color.white,Vector2.zero); vh.AddTriangle(0,1,2);
                cut.ModifyMesh(vh);
                if(expectedMinimum==0) Assert.AreEqual(0,vh.currentVertCount);
                else Assert.GreaterOrEqual(vh.currentVertCount,expectedMinimum);
                for(int i=0;i<vh.currentVertCount;i++) { var v=new UIVertex(); vh.PopulateUIVertex(ref v,i); Assert.GreaterOrEqual(v.position.x+x,-.001f); }
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
}
