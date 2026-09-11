using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
public sealed class KaitCompactCardTests
{
    [Test] public void DockLeavesOnlyNarrowTabs()
    {
        var r=new Rect(-960,-540,1920,1080);
        Assert.AreEqual(80,KaitSkillCard.DockY(r,false,false)+132-r.yMin);
        Assert.AreEqual(64,r.yMax-(KaitPassiveCard.DockY(r,false,false)-132));
    }
    [Test] public void ActiveUsesRoundRowAndPassiveUsesAngularRow()
    {
        var active=KaitCardSkin.Face(KaitAbilityCatalog.Get(KaitSkill.SwiftBoots));
        var passive=KaitCardSkin.Face(KaitAbilityCatalog.Get(KaitPassive.BirdEye));
        Assert.AreEqual(0,active.rect.y);Assert.AreEqual(passive.texture.height/2,passive.rect.y);
    }
    [Test] public void CompactCardsHideArtworkButKeepCooldownAndName()
    {
        var go=new GameObject("Cards",typeof(RectTransform));
        try
        {
            var r=go.GetComponent<RectTransform>();r.sizeDelta=new Vector2(1920,1080);var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var active=KaitSkillCard.Create(r,null,font,null,null,null,null,null);
            active.Show(KaitSkill.SwiftBoots,false,new Vector2(0,KaitSkillCard.DockY(r.rect,false,false)),0);active.SetAvailability(false,2,false);
            Assert.IsFalse(active.transform.Find("Card Logo").gameObject.activeSelf);Assert.IsFalse(active.transform.Find("Effect").gameObject.activeSelf);
            Assert.AreEqual("冷却 2 回合",active.transform.Find("Availability").GetComponent<Text>().text);
            active.SetPending(true);
            Assert.AreEqual("冷却 2 回合",active.transform.Find("Availability").GetComponent<Text>().text);
            active.SetAvailability(true,0,false);
            Assert.AreEqual("冷却 0 回合",active.transform.Find("Availability").GetComponent<Text>().text);
            var passive=KaitPassiveCard.Create(r,null,font,null,null,null,null);
            passive.Show(KaitPassive.BirdEye,false,new Vector2(0,KaitPassiveCard.DockY(r.rect,false,false)),0);passive.Pulse(1);
            Assert.IsFalse(passive.Expanded);Assert.IsFalse(passive.transform.Find("Card Logo").gameObject.activeSelf);
            Assert.AreEqual("警戒武器",passive.transform.Find("Name").GetComponent<Text>().text);
        }
        finally{Object.DestroyImmediate(go);}
    }
}
