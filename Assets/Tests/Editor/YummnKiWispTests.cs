using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class YummnKiWispTests
{
    [TestCase(0f,false)] [TestCase(.5f,false)] [TestCase(1f,true)]
    public void SharedValueControlsBothSkins(float amount,bool exhausted)
    {
        var root=new GameObject("Ki",typeof(RectTransform),typeof(Image));
        try
        {
            var w=root.AddComponent<YummnKiWisp>();w.Configure(null);w.SetState(amount,exhausted);
            Assert.AreEqual(amount,w.Amount);Assert.AreEqual(exhausted,w.Exhausted);
            var images=root.GetComponentsInChildren<Image>();Assert.AreEqual(4,images.Length);
            foreach(var img in images){Assert.NotNull(img.sprite);Assert.IsFalse(img.raycastTarget);Assert.NotNull(img.GetComponent<SunlitSplitText>());if(img.type==Image.Type.Filled)Assert.AreEqual(amount,img.fillAmount);}
            Assert.AreNotSame(images[2].sprite,images[3].sprite);
            Assert.AreEqual(1f,images[3].sprite.rect.width/images[3].sprite.rect.height);
            Assert.AreEqual(images[3].rectTransform.rect.width,images[3].rectTransform.rect.height);
            if(!exhausted)Assert.AreEqual(YummnKiWisp.Cyan,images[3].color);
        }
        finally{Object.DestroyImmediate(root);}
    }
}
