using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class YummnRoot090Tests
{
    KaitRun New(params KaitPassive[] passives)
    {
        var run=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0,playerInvincible=true});
        run.SelectCharacter(KaitCharacter.Yummn,9001,YummnRulesSnapshot.Current());
        run.enemies.Clear();run.spawns.Clear();run.passives.AddRange(passives);Place(run,new Vector2Int(2,2));
        Call(run,"BeginYummnRoot");Call(run,"BeginYummnRangeTracking");return run;
    }
    object Call(KaitRun run,string name,params object[] args)=>typeof(KaitRun).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(run,args);
    void Place(KaitRun run,Vector2Int cell)=>typeof(KaitRun).GetProperty("katePos").SetValue(run,cell);
    KaitEnemy Enemy(KaitRun run,Vector2Int cell,int hp=20,KaitEnemyType type=KaitEnemyType.Grunt)
    {var e=new KaitEnemy{id=100+run.enemies.Count,pos=cell,hp=hp,maxHp=hp,type=type,life=KaitEnemyLife.Active,intent=new KaitIntent{origin=cell}};run.enemies.Add(e);return e;}
    KaitTurnResult Result(YummnPhase phase=YummnPhase.Burst)=>new KaitTurnResult{valid=true,yummnAction=new YummnActionContext{direction=KaitDirection.Right,phaseAtStart=phase}};
    [Test] public void MainPunchPrecedesThreatDamage()
    {
        var run=New(KaitPassive.MagicMissile);var e=Enemy(run,new Vector2Int(3,2));run.threat[0,2]=run.threat[1,2]=2;
        var r=run.TryGlobalInput(KaitDirection.Right);
        Assert.Less(r.yummnEvents.FindIndex(x=>x.damageCause==YummnDamageCause.Punch&&x.kind==YummnEventKind.Hit),r.yummnEvents.FindIndex(x=>x.damageCause==YummnDamageCause.MergeMissile));
    }
    [Test] public void ExhaustedKickNeverFlurriesPushesOrStuns()
    {
        var run=New(KaitPassive.TwinPunch,KaitPassive.StunStrike,KaitPassive.OpenHand,KaitPassive.FireSnake);
        run.Yummn.phase=YummnPhase.Exhausted;run.Yummn.ki=4;var e=Enemy(run,new Vector2Int(3,2));var rear=Enemy(run,new Vector2Int(4,2));
        var r=run.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(19,e.hp);Assert.AreEqual(20,rear.hp);Assert.IsFalse(e.yummnStunned);Assert.AreEqual(new Vector2Int(3,2),e.pos);Assert.AreEqual(0,r.yummnAction.attackCostTenths);
        Assert.IsTrue(r.yummnEvents.Any(x=>x.damageCause==YummnDamageCause.Kick));Assert.AreEqual(1,run.EnemyResolveCount);
    }
    [Test] public void ForcedExitCommitsThenKicksWithoutMartialEffects()
    {
        var run=New(KaitPassive.OpportunityAttack,KaitPassive.TwinPunch,KaitPassive.OpenHand);var e=Enemy(run,new Vector2Int(3,2));var r=Result();
        Call(run,"ForceYummnEnemy",e,Vector2Int.right,5,r);Assert.AreEqual(new Vector2Int(5,2),e.pos);Assert.AreEqual(19,e.hp);
        int move=r.yummnEvents.FindIndex(x=>x.kind==YummnEventKind.Move),hit=r.yummnEvents.FindIndex(x=>x.kind==YummnEventKind.Hit);Assert.Less(move,hit);Assert.AreEqual(YummnDamageCause.Kick,r.yummnEvents[hit].damageCause);
    }
    [Test] public void PlayerLeavingRangeAlsoKicks()
    {var run=New(KaitPassive.OpportunityAttack);var e=Enemy(run,new Vector2Int(3,2));Call(run,"MoveYummnPlayer",new Vector2Int(1,2),YummnMoveCause.Teleport,Result());Assert.AreEqual(19,e.hp);}
    [Test] public void ExhaustedEntryDoesNotReact()
    {var run=New(KaitPassive.Opportunist,KaitPassive.TwinPunch);var e=Enemy(run,new Vector2Int(4,2));Call(run,"MoveYummnPlayer",new Vector2Int(3,2),YummnMoveCause.Teleport,Result(YummnPhase.Exhausted));Assert.AreEqual(20,e.hp);}
    [Test] public void LongPushFollowsFinalLandingNotOldCell()
    {var run=New(KaitPassive.EndlessPush,KaitPassive.FollowThrough);var e=Enemy(run,new Vector2Int(3,2));run.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(new Vector2Int(5,2),e.pos);Assert.AreEqual(new Vector2Int(4,2),run.katePos);}
    [Test] public void FollowEntryCanStartItsOwnFlurry()
    {var run=New(KaitPassive.TwinPunch,KaitPassive.OpenHand,KaitPassive.FollowThrough,KaitPassive.Opportunist);var e=Enemy(run,new Vector2Int(3,2));var r=run.TryGlobalInput(KaitDirection.Right);Assert.GreaterOrEqual(r.yummnPunches,3);Assert.LessOrEqual(run.Ki,2);}
    [Test] public void KillGainIsImmediateButRootPhaseStaysExhausted()
    {var run=New();run.Yummn.phase=YummnPhase.Exhausted;run.Yummn.ki=0;var e=Enemy(run,new Vector2Int(3,2),1);var r=Result(YummnPhase.Exhausted);Call(run,"YummnHit",e,1,Vector2Int.right,YummnDamageCause.Kick,r);Assert.AreEqual(3,run.Ki);Assert.AreEqual(YummnPhase.Exhausted,run.KiPhase);}
    [Test] public void SameRootRiftSpawnsInOnlyWindow()
    {var run=New();run.spawns.Add(new KaitSpawnRequest{targetCell=new Vector2Int(4,4),sourceThreatCell=new Vector2Int(3,3),tier=1,createdTurn=run.turn});var r=run.TryYummnWait();Assert.AreEqual(1,r.spawnedEnemyCells.Count);Assert.AreEqual(1,run.Yummn.metrics.spawnChecks);Assert.AreEqual(1,run.EnemyResolveCount);}
    [Test] public void NewbornMeleeOnlyPrepares()
    {var run=New();run.spawns.Add(new KaitSpawnRequest{targetCell=new Vector2Int(3,2),sourceThreatCell=new Vector2Int(2,1),tier=1,createdTurn=run.turn});var r=run.TryYummnWait();Assert.IsEmpty(r.enemyActions);Assert.AreEqual(KaitIntentType.Melee,run.enemies[0].intent.type);}
    [Test] public void FollowDoesNotScheduleMovementPhase()
    {var run=New();run.SelectCharacter(KaitCharacter.Yummn,1,YummnRulesSnapshot.Current(movePhase:true));var a=new YummnActionContext{playerMoved=true,phaseAtStart=YummnPhase.Burst};Assert.IsFalse(run.ShouldRunYummnEnemyPhase(a));a.voluntaryMoved=true;Assert.IsTrue(run.ShouldRunYummnEnemyPhase(a));}
    [Test] public void DamagePacketCommitsBothIceGuardsBeforeShatter()
    {var run=New(KaitPassive.ShatteringPalm);var a=Enemy(run,new Vector2Int(3,2));var b=Enemy(run,new Vector2Int(4,2));a.yummnFrozen=b.yummnFrozen=true;var r=Result();Call(run,"YummnDamagePacket",new System.Collections.Generic.List<KaitEnemy>{a,b},1,Vector2Int.right,YummnDamageCause.WinterBreath,r,false);Assert.AreEqual(19,a.hp);Assert.AreEqual(19,b.hp);Assert.AreEqual("IceGuard",r.yummnEvents.Where(x=>x.kind==YummnEventKind.Hit).Take(2).Last().status);}
    [Test] public void MisdirectionArcherAimsInsteadOfImmediateMelee()
    {var run=New(KaitPassive.Misdirection);var e=Enemy(run,new Vector2Int(4,3),20,KaitEnemyType.Archer);run.Yummn.afterimages.Add(new YummnAfterimageMarker{id=1,cell=new Vector2Int(3,3)});var r=Result();Call(run,"ResolveYummnCommandActor",e,r);Assert.AreEqual(KaitIntentType.LineShot,e.intent.type);Assert.IsEmpty(r.enemyActions);Assert.IsTrue(run.Yummn.afterimages[0].alive);}
    [Test] public void ReflectedAttackerDeathDoesNotCancelPlayerHit()
    {var run=New(KaitPassive.EchoReprisal);run.config.playerInvincible=false;var e=Enemy(run,new Vector2Int(3,2),1);var cell=run.katePos;run.Yummn.afterimages.Add(new YummnAfterimageMarker{id=1,cell=cell});e.intent=new KaitIntent{type=KaitIntentType.Melee,origin=e.pos,target=cell,damage=1};e.intent.affectedCells.Add(cell);int hp=run.kateHp;var r=Result();Call(run,"ResolveYummnCommandActor",e,r);Assert.AreEqual(hp-1,run.kateHp);Assert.AreEqual(KaitEnemyLife.Dead,e.life);}
    [Test] public void DecoyAndEchoShareOneKiRewardPerAttack()
    {var run=New();run.Yummn.ki=0;run.Yummn.decoy=new Vector2Int(3,2);run.Yummn.afterimages.Add(new YummnAfterimageMarker{id=1,cell=run.Yummn.decoy});var e=Enemy(run,new Vector2Int(4,2));e.intent=new KaitIntent{type=KaitIntentType.Melee,damage=1,target=run.Yummn.decoy};e.intent.affectedCells.Add(run.Yummn.decoy);Call(run,"ResolveYummnCommandActor",e,Result());Assert.AreEqual(1,run.Ki);}
    [Test] public void PendulumReturnUsesItsOwnDirectionAndRestoresBase()
    {
        var run=New(KaitPassive.GravityPendulum);var r=Result();
        var field=typeof(KaitRun).GetProperty("actualThreatDirection",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        field.SetValue(run,KaitDirection.Right);run.threat[2,2]=run.threat[3,2]=2;
        r.merges.Add(new KaitMergeEvent{resultValue=4,threatCell=new Vector2Int(4,3)});
        Call(run,"ResolveYummnPendulum",r);
        var merge=r.merges.Single(m=>m.mergeSource=="PendulumReturn");
        Assert.AreEqual(KaitDirection.Left,merge.actualThreatDirection);
        Assert.AreEqual(KaitDirection.Right,field.GetValue(run));
    }
    [Test] public void ReactionOpensRouteAndSlideContinuesInOriginalDirection()
    {
        var run=New(KaitPassive.Opportunist,KaitPassive.OpenHand,KaitPassive.OpportunityAttack,KaitPassive.FollowThrough);
        var enemy=Enemy(run,new Vector2Int(4,2),2);
        var r=run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(KaitEnemyLife.Dead,enemy.life);
        Assert.AreEqual(new Vector2Int(5,2),run.katePos);
        Assert.AreEqual(2,r.yummnAction.voluntaryCells);
        Assert.AreEqual(2,r.yummnAction.movementKiCost);
        Assert.AreEqual(KaitDirection.Right,r.yummnAction.direction);
    }
}
