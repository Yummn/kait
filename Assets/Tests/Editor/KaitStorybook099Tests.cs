using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitStorybook099Tests
{
    [Test] public void ShortCardsKeepExposedTabDepthAndEqualProportions()
    {
        var area=new Rect(-960,-540,1920,1080);
        Assert.AreEqual(KaitSkillCard.Size,KaitPassiveCard.Size);
        Assert.AreEqual(80,KaitSkillCard.DockY(area,false,false)+KaitSkillCard.Size.y*.5f-area.yMin);
        Assert.AreEqual(64,area.yMax-KaitPassiveCard.DockY(area,false,false)+KaitPassiveCard.Size.y*.5f);
        var face=KaitStorybookTheme.Card(KaitAbilityCatalog.Get(KaitSkill.SwiftBoots));
        Assert.AreEqual(KaitSkillCard.Size.y/KaitSkillCard.Size.x,face.rect.height/face.rect.width,.001f);
    }
    [Test] public void RewardTextAndCooldownRemainInsideThePaperMargin()
    {
        var root=new GameObject("UI",typeof(RectTransform));
        try
        {
            var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(1920,1080);
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var card=KaitSkillCard.Create(rect,null,font,null,null,null,null,null);
            card.Show(KaitSkill.SwiftBoots,true,Vector2.zero,0);
            foreach(var label in card.GetComponentsInChildren<Text>())
            {
                if(string.IsNullOrWhiteSpace(label.text))continue;
                var r=label.rectTransform;
                Assert.LessOrEqual(Mathf.Abs(r.anchoredPosition.y)+r.sizeDelta.y*.5f,card.Rect.sizeDelta.y*.5f-7,label.name);
            }
            Assert.IsTrue(card.GetComponentInChildren<KaitCardLogo>().gameObject.activeSelf);
        }
        finally{Object.DestroyImmediate(root);}
    }
}
