using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

public sealed class YummnSelectedFramesTests
{
    private GameObject host;
    [TearDown] public void Cleanup(){if(host!=null)Object.DestroyImmediate(host);}
    [TestCase(0,"PunchB",.32f)] [TestCase(1,"FlurryB",.48f)]
    [TestCase(3,"StunB",.64f)] [TestCase(4,"BlockA",.4f)]
    public void SelectedArtUsesEightDifferentFramesWithoutScaleAnimation(int kind,string clip,float life)
    {
        Assert.AreEqual(clip,YummnEffectGraphic.SelectedClip(kind));Assert.AreEqual(life,YummnEffectGraphic.Duration(kind));
        host=new GameObject("Selected frames",typeof(RectTransform),typeof(YummnEffectGraphic));
        var fx=host.GetComponent<YummnEffectGraphic>();fx.Initialize(kind);
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var sprites=(Sprite[])typeof(YummnEffectGraphic).GetField("frames",flags).GetValue(fx);
        Assert.AreEqual(8,sprites.Length);
        var texture=Resources.Load<Texture2D>("KaitVisuals/Yummn/Frames/"+clip);Assert.NotNull(texture);
        Assert.AreEqual(1774,texture.width);Assert.AreEqual(887,texture.height);
        for(int i=0;i<8;i++)
        {
            var rect=sprites[i].rect;Assert.Greater(rect.width,440);Assert.LessOrEqual(rect.xMax,texture.width);Assert.LessOrEqual(rect.yMax,texture.height);
            if(i>0)Assert.AreNotEqual(sprites[i-1].rect,rect);
        }
        typeof(YummnEffectGraphic).GetField("age",flags).SetValue(fx,life*.5f);
        typeof(YummnEffectGraphic).GetMethod("Update",flags).Invoke(fx,null);
        Assert.GreaterOrEqual(fx.FrameIndex,4);Assert.AreSame(sprites[fx.FrameIndex],fx.sprite);
        Assert.AreEqual(Vector3.one,fx.rectTransform.localScale);Assert.IsFalse(fx.raycastTarget);Assert.IsFalse(fx.maskable);
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/KaitVisuals/Yummn/Frames/"+clip+".png");
        Assert.IsFalse(importer.mipmapEnabled);Assert.IsTrue(importer.alphaIsTransparency);
        Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
    }
    [Test] public void UnselectedLegacyArtStillUsesItsExistingPath(){Assert.IsNull(YummnEffectGraphic.SelectedClip(2));Assert.IsNull(YummnEffectGraphic.SelectedClip(5));}
}
