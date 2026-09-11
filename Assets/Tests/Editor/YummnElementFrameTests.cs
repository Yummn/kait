using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class YummnElementFrameTests
{
    [TestCase(0,"WaterC")] [TestCase(1,"AirA")] [TestCase(3,"WinterA")] [TestCase(4,"FireA")]
    [TestCase(2,"PillarC")] [TestCase(5,"ShatterA")] [TestCase(8,"PalmBurstA")] [TestCase(12,"PalmMarkB")]
    [TestCase(6,"TeleportB")] [TestCase(7,"DarknessB")] [TestCase(13,"AimDeniedC")] [TestCase(14,"ShadowA")]
    [TestCase(9,"KiA")] [TestCase(10,"ExhaustC")] [TestCase(11,"RecoverB")]
    public void ApprovedElementUsesEightAlphaFrames(int index,string name)
    {
        var go=new GameObject("element test",typeof(RectTransform),typeof(YummnV08Effect));
        try
        {
            var fx=go.GetComponent<YummnV08Effect>();fx.rectTransform.sizeDelta=Vector2.one*116;fx.Initialize(index,.32f);
            Assert.AreEqual(name,YummnV08Effect.SelectedClip(index));Assert.NotNull(fx.sprite);
            Assert.AreSame(Resources.Load<Texture2D>("KaitVisuals/Yummn/Frames/"+name),fx.sprite.texture);
            Assert.AreNotEqual(YummnV08Art.Material,fx.material);Assert.IsFalse(fx.raycastTarget);Assert.IsFalse(fx.maskable);
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var frames=(Sprite[])typeof(YummnV08Effect).GetField("frames",flags).GetValue(fx);
            Assert.AreEqual(8,frames.Length);
            for(int i=1;i<8;i++)Assert.AreNotEqual(frames[i-1].rect,frames[i].rect);
            typeof(YummnV08Effect).GetField("age",flags).SetValue(fx,YummnV08Effect.ClipDuration(index)*.5f);
            typeof(YummnV08Effect).GetMethod("Update",flags).Invoke(fx,null);
            Assert.GreaterOrEqual(fx.FrameIndex,4);Assert.AreEqual(Vector2.one*116,fx.rectTransform.sizeDelta);
        }
        finally {Object.DestroyImmediate(go);}
    }
    [TestCase(2,false,7)] [TestCase(12,true,2)] [TestCase(7,true,2)] [TestCase(14,true,2)]
    public void TerrainHoldsOrLoopsWithoutDisappearing(int index,bool looping,int expected)
    {
        var go=new GameObject("persistent",typeof(RectTransform),typeof(YummnV08Effect));
        try
        {
            var fx=go.GetComponent<YummnV08Effect>();fx.InitializePersistent(index,looping);
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            // Update adds the editor's current delta, which varies with import/load.
            // Sample the same absolute animation time on fast and slow machines.
            typeof(YummnV08Effect).GetField("age",flags).SetValue(fx,1.02f-Time.unscaledDeltaTime);
            typeof(YummnV08Effect).GetMethod("Update",flags).Invoke(fx,null);
            Assert.AreEqual(expected,fx.FrameIndex);Assert.NotNull(fx.sprite);
        }
        finally {Object.DestroyImmediate(go);}
    }
}
