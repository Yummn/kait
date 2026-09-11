using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class YummnGhostCCTest
{
    [Test] public void SelectedCClipHasNativeFrames()
    {
        Assert.AreEqual("GhostHitC",YummnV08Effect.SelectedClip(15));
        var root=new GameObject("Ghost hit test",typeof(RectTransform),typeof(YummnV08Effect));
        try{var fx=root.GetComponent<YummnV08Effect>();fx.Initialize(15,.4f);Assert.IsNotNull(fx.sprite);Assert.IsFalse(fx.raycastTarget);Assert.IsFalse(fx.maskable);Assert.AreEqual(0,fx.FrameIndex);}
        finally{Object.DestroyImmediate(root);}
    }
    [Test] public void SelectedSoundCLoadsAsOneSecond()
    {var clip=YummnAudio.LoadClip("GhostHit");Assert.IsNotNull(clip);Assert.AreEqual(1f,clip.length,.02f);}
    [Test] public void SandDoesNotBlockClicksOrUseMasks()
    {var root=new GameObject("Sand test",typeof(RectTransform),typeof(YummnGhostSand));try{var sand=root.GetComponent<YummnGhostSand>();Assert.IsFalse(sand.raycastTarget);Assert.IsFalse(sand.maskable);Assert.AreEqual(0,root.GetComponents<Mask>().Length);}finally{Object.DestroyImmediate(root);}}
}
