using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

public class YummnDragTargetTests
{
    KaitRun New()
    {
        var r=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0,playerInvincible=true});
        r.SelectCharacter(KaitCharacter.Yummn,1609,YummnRulesSnapshot.Current());
        r.enemies.Clear();r.spawns.Clear();return r;
    }
    [Test] public void ClickOwnedCardOnlyPreviews()
    {
        var root=new GameObject("Test",typeof(RectTransform));
        var events=new GameObject("Events",typeof(EventSystem));
        try {
            int casts=0;
            var card=KaitSkillCard.Create((RectTransform)root.transform,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null,null,null,null,c=>{casts++;return true;});
            card.Show(KaitSkill.FrostBreath,false,Vector2.zero,0);card.SetAvailability(true,0,false);
            card.OnPointerClick(new PointerEventData(events.GetComponent<EventSystem>()){button=PointerEventData.InputButton.Left});
            Assert.AreEqual(0,casts);
        } finally {Object.DestroyImmediate(root);Object.DestroyImmediate(events);}
    }
    [Test] public void HealOnlyAcceptsSelfAndAdvancesOnce()
    {
        var r=New();r.skills.Add(KaitSkill.MendWait);
        Assert.IsFalse(r.IsLegalSkillCell(KaitSkill.MendWait,r.katePos+Vector2Int.up));
        var p=r.katePos;Assert.IsTrue(r.TryUseSkillAt(KaitSkill.MendWait,p,out var why),why);
        Assert.AreEqual(p,r.katePos);Assert.AreEqual(3,r.Ki);Assert.AreEqual(1,r.EnemyResolveCount);
    }
    [TestCase(KaitSkill.ShapeIce)] [TestCase(KaitSkill.Darkness)] [TestCase(KaitSkill.UniqueDecoy)]
    public void GroundSkillsAcceptDistantEmptyCell(KaitSkill skill)
    {
        var r=New();r.skills.Add(skill);var p=r.katePos;var target=new Vector2Int(2,5);
        Assert.IsFalse(r.IsLegalSkillCell(skill,p));
        Assert.IsTrue(r.TryUseSkillAt(skill,target,out var why),why);Assert.AreEqual(p,r.katePos);
    }
    [Test] public void EveryActiveHasExactlyOneTargetMode()
    {
        foreach(var card in YummnCatalog.Cards)if(card.kind==KaitAbilityKind.Active)
            Assert.AreEqual(1,(YummnCatalog.TargetsSelf(card.skill)?1:0)+(YummnCatalog.TargetsGround(card.skill)?1:0)+(YummnCatalog.TargetsDirection(card.skill)?1:0),card.id);
        Assert.IsTrue(YummnCatalog.TargetsDirection(KaitSkill.FrostBreath));
    }
}
