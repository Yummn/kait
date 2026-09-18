using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class YummnPunchUnifiedTests
{
    private KaitRun New(YummnTileSupplyMode supply=YummnTileSupplyMode.KillOnly)
    {
        var r=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0,playerInvincible=true});
        r.SelectCharacter(KaitCharacter.Yummn,1609,YummnRulesSnapshot.Current(supply:supply));
        r.enemies.Clear();r.spawns.Clear();return r;
    }
    private KaitEnemy Enemy(KaitRun r,Vector2Int p,int hp=8,KaitEnemyType type=KaitEnemyType.Grunt)
    {var e=new KaitEnemy{id=100+r.enemies.Count,pos=p,hp=hp,maxHp=hp,type=type,life=KaitEnemyLife.Active,intent=new KaitIntent{origin=p}};r.enemies.Add(e);return e;}
    [Test] public void OnlyWaitingGuardRemainsNamedUnarmored()
    {Assert.AreEqual(38,YummnCatalog.Cards.Count);Assert.IsFalse(YummnCatalog.Cards.Any(d=>d.passive==KaitPassive.Tranquility));Assert.AreEqual("无甲防御",YummnCatalog.Cards.Single(d=>d.passive==KaitPassive.WaitingGuard).nameZh);}
    [TestCase(YummnTileSupplyMode.KillOnly)] [TestCase(YummnTileSupplyMode.EveryAction)] [TestCase(YummnTileSupplyMode.EffectiveMove)]
    public void WaitAlwaysSuppliesExactlyOne(YummnTileSupplyMode supply)
    {var r=New(supply);Assert.AreEqual(1,r.TryYummnWait().yummnAction.insertedTwos);r.passives.Add(KaitPassive.WaitSupply);Assert.AreEqual(2,r.TryYummnWait().yummnAction.insertedTwos);}
    [Test] public void DoublePunchCostsTwoAndEachHitsFireSnake()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.TwinPunch,KaitPassive.FireSnake});var e=Enemy(r,r.katePos+Vector2Int.right);var rear=Enemy(r,r.katePos+Vector2Int.right*2);int ki=r.Ki;var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(ki-2,r.Ki);Assert.AreEqual(6,e.hp);Assert.AreEqual(6,rear.hp);Assert.AreEqual(2,a.yummnPunches);}
    [Test] public void OneKiFallsBackToOnePunch()
    {var r=New();r.Yummn.ki=1;r.passives.Add(KaitPassive.TwinPunch);var e=Enemy(r,r.katePos+Vector2Int.right);Assert.AreEqual(1,r.TryGlobalInput(KaitDirection.Right).yummnPunches);Assert.AreEqual(7,e.hp);}
    [Test] public void AiEntryUsesDoublePunchAndFireSnake()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.Opportunist,KaitPassive.TwinPunch,KaitPassive.FireSnake});var e=Enemy(r,r.katePos+Vector2Int.up*2,8,KaitEnemyType.Swordsman);var a=r.TryYummnWait();Assert.AreEqual(6,e.hp);Assert.AreEqual(2,a.yummnEvents.Count(x=>x.damageCause==YummnDamageCause.Counter&&x.kind==YummnEventKind.Hit));Assert.AreEqual(4,r.Ki);}
    [Test] public void AlreadyAdjacentDoesNotRepeatWhenWaiting()
    {var r=New();r.passives.Add(KaitPassive.Opportunist);var e=Enemy(r,r.katePos+Vector2Int.right);r.TryYummnWait();r.TryYummnWait();Assert.AreEqual(8,e.hp);}
    [Test] public void SpawnEntryTriggersPunch()
    {var r=New();r.passives.Add(KaitPassive.Opportunist);var p=r.katePos+Vector2Int.right;r.spawns.Add(new KaitSpawnRequest{tier=1,targetCell=p,sourceThreatCell=new Vector2Int(2,2),createdTurn=-1});var a=r.TryYummnWait();Assert.IsTrue(a.yummnEvents.Any(x=>x.damageCause==YummnDamageCause.Counter));}
    [Test] public void PullEntryTriggersPunch()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.Opportunist,KaitPassive.DistantPull});var e=Enemy(r,r.katePos+Vector2Int.right*2);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(6,e.hp);Assert.AreEqual(1,a.yummnPunches);}
    [Test] public void PlayerStepCreatesNewRangeReaction()
    {var r=New();r.passives.Add(KaitPassive.Opportunist);r.skills.Add(KaitSkill.PreciseStep);var e=Enemy(r,r.katePos+Vector2Int.right+Vector2Int.up);Assert.IsTrue(r.TryUseSkillAt(KaitSkill.PreciseStep,r.katePos+Vector2Int.right,out _));Assert.AreEqual(7,e.hp);}
    [Test] public void KillFollowLeavesEchoAtDeparture()
    {var r=New();var p=r.katePos;Enemy(r,p+Vector2Int.right,1);var a=r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(a.yummnEvents.Any(x=>x.kind==YummnEventKind.AfterimageCreated&&x.to==p));}
    [Test] public void PushFollowLeavesEchoAtDeparture()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.OpenHand,KaitPassive.FollowThrough});var p=r.katePos;Enemy(r,p+Vector2Int.right);var a=r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(a.yummnEvents.Any(x=>x.kind==YummnEventKind.AfterimageCreated&&x.to==p));}
    [Test] public void ExhaustedMoveAlsoLeavesEcho()
    {var r=New();r.Yummn.ki=0;r.Yummn.phase=YummnPhase.Exhausted;var a=r.TryGlobalInput(KaitDirection.Left);Assert.AreEqual(1,a.yummnEvents.Count(x=>x.kind==YummnEventKind.AfterimageCreated));}
    [Test] public void SlideHasOneOriginNotEveryCell()
    {var r=New();var a=r.TryGlobalInput(KaitDirection.Left);Assert.Greater(a.katePath.Count,1);Assert.AreEqual(1,a.yummnEvents.Count(x=>x.kind==YummnEventKind.AfterimageCreated));}
    [Test] public void EveryActiveUsesCellSelection()
    {foreach(var d in YummnCatalog.Cards.Where(x=>x.kind==KaitAbilityKind.Active))Assert.IsTrue(KaitRun.NeedsCellTarget(d.skill),d.id);}
    [Test] public void FrostChoosesClickedDirectionAndDoesNotSlide()
    {var r=New();var p=r.katePos;r.skills.Add(KaitSkill.FrostBreath);var e=Enemy(r,p+Vector2Int.up);r.threat[0,2]=2;Assert.IsTrue(r.TryUseSkillAt(KaitSkill.FrostBreath,p+Vector2Int.up,out var why),why);Assert.AreEqual(p,r.katePos);Assert.AreEqual(2,r.threat[0,2]);Assert.AreEqual(7,e.hp);Assert.IsTrue(e.yummnFrozen);Assert.IsFalse(r.Yummn.prepared.Any());}
    [Test] public void InvalidCastClearsPreparedAndDoesNotConsumeTurn()
    {var r=New();r.skills.Add(KaitSkill.FrostBreath);r.TryUseSkill(KaitSkill.FrostBreath,-1,out _);int ki=r.Ki,turn=r.turn;Assert.IsFalse(r.TryUseSkillAt(KaitSkill.FrostBreath,r.katePos+Vector2Int.up,out var why));Assert.IsNotEmpty(why);Assert.IsEmpty(r.Yummn.prepared);Assert.AreEqual(ki,r.Ki);Assert.AreEqual(turn,r.turn);}
    [Test] public void InsufficientCastClearsPreparedAndDoesNotConsumeTurn()
    {var r=New();r.Yummn.ki=0;r.skills.Add(KaitSkill.ShapeIce);Assert.IsFalse(r.TryUseSkillAt(KaitSkill.ShapeIce,r.katePos+Vector2Int.up,out var why));Assert.IsTrue(why.Contains("气不足"));Assert.AreEqual(0,r.turn);Assert.IsEmpty(r.Yummn.prepared);}
}
