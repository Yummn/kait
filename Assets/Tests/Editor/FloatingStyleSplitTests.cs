using NUnit.Framework;
using UnityEngine;

public class FloatingStyleSplitTests
{
    [TestCase(300f,250f,70f)]
    [TestCase(220f,250f,90f)]
    [TestCase(-300f,250f,220f)]
    public void FloatingCutHasConstantScreenOffset(float y,float width,float height)
    {
        var root=new GameObject("Screen",typeof(RectTransform));
        try
        {
            var screen=(RectTransform)root.transform;screen.sizeDelta=new Vector2(1920,1080);
            var content=new GameObject("Scaled content",typeof(RectTransform));content.transform.SetParent(screen,false);
            content.transform.localScale=Vector3.one*1.2f;
            var panel=new GameObject("Panel",typeof(RectTransform));panel.transform.SetParent(content.transform,false);
            var rect=(RectTransform)panel.transform;rect.sizeDelta=new Vector2(width,height);rect.anchoredPosition=new Vector2(0,y);
            var world=root.AddComponent<GlobalStyleSplit>();world.Configure(screen,.447f,.563f);
            var floating=root.AddComponent<GlobalStyleSplit>();floating.Configure(screen,.447f,.563f,28f);
            world.GetLocalSplits(rect,out float wb,out float wt);
            floating.GetLocalSplits(rect,out float fb,out float ft);
            Assert.That((fb-wb)*width*1.2f,Is.EqualTo(28f).Within(.001f));
            Assert.That((ft-wt)*width*1.2f,Is.EqualTo(28f).Within(.001f));
            var child=new GameObject("Nested icon",typeof(RectTransform));child.transform.SetParent(rect,false);
            var cr=(RectTransform)child.transform;cr.sizeDelta=new Vector2(20,36);
            floating.GetLocalSplits(cr,out float cb,out float ct);
            float px=rect.rect.xMin+width*Mathf.LerpUnclamped(fb,ft,.5f);
            float cx=cr.rect.xMin+20*Mathf.LerpUnclamped(cb,ct,.5f);
            Assert.That(px,Is.EqualTo(cx).Within(.001f));
        }
        finally{Object.DestroyImmediate(root);}
    }
}
