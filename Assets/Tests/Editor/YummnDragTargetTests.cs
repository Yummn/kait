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
    [Test] public void ClickOwnedCardPreparesAndKeepsItExpanded()
    {
        var root=new GameObject("Test",typeof(RectTransform));
        var events=new GameObject("Events",typeof(EventSystem));
        try {
            int casts=0;
            var card=KaitSkillCard.Create((RectTransform)root.transform,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null,null,null,null,c=>{casts++;c.SetAvailability(true,0,true);return true;});
            card.Show(KaitSkill.FrostBreath,false,Vector2.zero,0);card.SetAvailability(true,0,false);
            card.OnPointerClick(new PointerEventData(events.GetComponent<EventSystem>()){button=PointerEventData.InputButton.Left});
            Assert.AreEqual(1,casts);
            Assert.IsTrue(card.ShouldPreviewAt(Time.unscaledTime+100));
        } finally {Object.DestroyImmediate(root);Object.DestroyImmediate(events);}
    }
    [Test] public void TargetSelectionTimeoutIsFiveSeconds()
    {
        Assert.IsFalse(KaitSkillDeck.HasTargetTimedOut(10f,14.99f));
        Assert.IsTrue(KaitSkillDeck.HasTargetTimedOut(10f,15f));
    }
    [Test] public void TargetSelectorSheetIsAvailable()
    {
        var sheet=Resources.Load<Texture2D>("KaitVisuals/Yummn/SkillTargetSelector");
        Assert.NotNull(sheet);Assert.GreaterOrEqual(sheet.width/4,64);Assert.GreaterOrEqual(sheet.height/2,64);
    }
    [Test] public void SecondCardClickCancelsAndFoldsPreparedCard()
    {
        var root=new GameObject("Deck",typeof(RectTransform),typeof(KaitSkillDeck));
        var events=new GameObject("Events",typeof(EventSystem));
        try
        {
            var area=(RectTransform)root.transform;area.sizeDelta=new Vector2(1920,1080);
            var run=New();run.skills.Add(KaitSkill.FrostBreath);
            var deck=root.GetComponent<KaitSkillDeck>();KaitSkill selected=KaitSkill.None;
            deck.Initialize(area,null,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null,
                i=>{selected=run.skills[i];deck.Sync(run,selected);return true;},
                ()=>{selected=KaitSkill.None;deck.Sync(run,selected);});
            deck.Sync(run,selected);
            var click=new PointerEventData(events.GetComponent<EventSystem>()){button=PointerEventData.InputButton.Left};
            deck.Owned[0].OnPointerClick(click);
            Assert.AreEqual(KaitSkill.FrostBreath,selected);Assert.IsTrue(deck.Owned[0].ShouldPreviewAt(Time.unscaledTime+100));
            deck.Owned[0].OnPointerClick(click);
            Assert.AreEqual(KaitSkill.None,selected);Assert.IsFalse(deck.Owned[0].ShouldPreviewAt(Time.unscaledTime+100));
        }
        finally {Object.DestroyImmediate(root);Object.DestroyImmediate(events);}
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
        Assert.AreEqual(skill==KaitSkill.Darkness,r.IsLegalSkillCell(skill,p));
        Assert.IsTrue(r.TryUseSkillAt(skill,target,out var why),why);Assert.AreEqual(p,r.katePos);
    }
    [Test] public void EveryActiveHasExactlyOneTargetMode()
    {
        foreach(var card in YummnCatalog.Cards)if(card.kind==KaitAbilityKind.Active)
            Assert.AreEqual(1,(YummnCatalog.TargetsSelf(card.skill)?1:0)+(YummnCatalog.TargetsGround(card.skill)?1:0)+(YummnCatalog.TargetsDirection(card.skill)?1:0),card.id);
        Assert.IsTrue(YummnCatalog.TargetsDirection(KaitSkill.FrostBreath));
    }
    [Test] public void DarknessAcceptsAllVisibleCellsIncludingOccupiedAndPillars()
    {
        var r=New();r.skills.Add(KaitSkill.Darkness);
        r.enemies.Add(new KaitEnemy{id=999,pos=new Vector2Int(4,3),hp=2,maxHp=2,type=KaitEnemyType.Grunt,life=KaitEnemyLife.Active});
        for(int y=1;y<=5;y++)for(int x=1;x<=5;x++)
            Assert.IsTrue(r.IsLegalSkillCell(KaitSkill.Darkness,new Vector2Int(x,y)),x+","+y);
        foreach(var p in new[]{new Vector2Int(0,3),new Vector2Int(6,3),new Vector2Int(3,0),new Vector2Int(3,6)})
            Assert.IsFalse(r.IsLegalSkillCell(KaitSkill.Darkness,p));
        var origin=r.katePos;int ki=r.Ki;
        Assert.IsTrue(r.TryUseSkillAt(KaitSkill.Darkness,origin,out var why),why);
        Assert.AreEqual(origin,r.Yummn.darkness);Assert.AreEqual(origin,r.katePos);
        Assert.AreEqual(ki-2,r.Ki);
    }
    [Test] public void DarknessOnAimingEnemyCancelsAttackAndReplacesPreviousFog()
    {
        var r=New();r.skills.Add(KaitSkill.Darkness);var target=new Vector2Int(4,3);
        var enemy=new KaitEnemy{id=999,pos=target,hp=2,maxHp=2,type=KaitEnemyType.Archer,life=KaitEnemyLife.Active,
            intent=new KaitIntent{type=KaitIntentType.LineShot,origin=target,target=r.katePos,direction=Vector2Int.left}};
        enemy.intent.affectedCells.Add(r.katePos);r.enemies.Add(enemy);
        r.Yummn.darkness=new Vector2Int(2,5);
        Assert.IsTrue(r.TryUseSkillAt(KaitSkill.Darkness,target,out var why),why);
        Assert.AreEqual(target,r.Yummn.darkness);
        Assert.IsFalse(r.lastSkillResult.enemyActions.Exists(a=>a.enemyId==enemy.id&&a.type==KaitIntentType.LineShot));
        r.TryYummnWait();Assert.AreEqual(target,r.Yummn.darkness);
    }
}
