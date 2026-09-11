using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class YummnEffectGraphicTests
{
    private GameObject host;
    private YummnEffectGraphic Create()
    {
        host=new GameObject("Effect first-frame test",typeof(RectTransform),typeof(YummnEffectGraphic));
        return host.GetComponent<YummnEffectGraphic>();
    }
    [TearDown] public void Cleanup(){if(host!=null)Object.DestroyImmediate(host);}

    [Test] public void UninitializedEffectHasNoVisibleRectangle()
    {
        var fx=Create();
        Assert.AreEqual(0f,fx.color.a);
        Assert.AreEqual(0,VertexCount(fx));
    }

    [TestCase(0,"Punch")]
    [TestCase(1,"Flurry")]
    [TestCase(2,"Palm")]
    [TestCase(3,"Stun")]
    [TestCase(4,"Ward")]
    [TestCase(5,"Frost")]
    public void TextureIsReadyBeforeFirstUpdate(int kind,string name)
    {
        var fx=Create();fx.kind=kind;
        Assert.NotNull(fx.sprite,"The creation frame must not draw the default white UI image.");
        string selected=YummnEffectGraphic.SelectedClip(kind);
        Assert.AreSame(Resources.Load<Texture2D>(selected!=null?"KaitVisuals/Yummn/Frames/"+selected:"KaitVisuals/Yummn/Effects/"+name),fx.sprite.texture);
        Assert.AreEqual(1f,fx.color.a);
        Assert.That(fx.rectTransform.localScale.x,Is.EqualTo(selected!=null?1f:.66f).Within(.001f));
        Assert.Greater(VertexCount(fx),0);
    }

    [Test] public void MissingSpriteCannotBecomeWhiteAfterUpdate()
    {
        var fx=Create();fx.kind=0;fx.sprite=null;
        typeof(YummnEffectGraphic).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(fx,null);
        Assert.AreEqual(0f,fx.color.a);
        Assert.AreEqual(0,VertexCount(fx));
    }

    private static int VertexCount(YummnEffectGraphic fx)
    {
        using(var vertices=new VertexHelper())
        {
            typeof(YummnEffectGraphic).GetMethod("OnPopulateMesh",BindingFlags.Instance|BindingFlags.NonPublic,null,new[]{typeof(VertexHelper)},null)
                .Invoke(fx,new object[]{vertices});
            return vertices.currentVertCount;
        }
    }
}
