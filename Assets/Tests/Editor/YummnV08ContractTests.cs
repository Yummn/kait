using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class YummnV08ContractTests
{
    private const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    [Test] public void PalmMarksCoexistAndDetonateIndependently()
    {
        var r=R();r.passives.Add(KaitPassive.QuiveringPalm);
        var a=E(r,2,3,20);var b=E(r,4,3,20);
        void Punch(KaitEnemy enemy,KaitDirection direction) {
            var result=new KaitTurnResult{yummnAction=new YummnActionContext{direction=direction}};
            typeof(KaitRun).GetMethod("ResolveYummnPunch",Hidden).Invoke(r,new object[]{enemy,result});
        }
        Punch(a,KaitDirection.Right);Punch(b,KaitDirection.Up);
        Assert.AreEqual(2,r.Yummn.palmMarks.Count);
        Assert.IsTrue(r.Yummn.TryPalm(a.id,out var first));Assert.AreEqual(Vector2Int.right,first);
        Punch(a,KaitDirection.Down);Assert.AreEqual(16,a.hp);
        Assert.IsFalse(r.Yummn.TryPalm(a.id,out _));Assert.IsTrue(r.Yummn.TryPalm(b.id,out var second));Assert.AreEqual(Vector2Int.up,second);
        for(int i=0;i<100;i++)r.Yummn.MarkPalm(1000+i,Vector2Int.left);
        Assert.AreEqual(101,r.Yummn.palmMarks.Count);
        r.Yummn.Reset();Assert.AreEqual(0,r.Yummn.palmMarks.Count);
    }
    [TestCase(1,0)] [TestCase(-1,0)] [TestCase(0,1)] [TestCase(0,-1)]
    public void PalmBottomPointsAlongHit(int x,int y)
    {
        var direction=new Vector2Int(x,y);
        var bottom=Quaternion.Euler(0,0,KaitGame.PalmMarkAngle(direction))*Vector3.down;
        Assert.That(Vector3.Distance(bottom,new Vector3(x,y,0)),Is.LessThan(.001f));
    }
    private KaitRun R(int x=1,int y=3)
    {var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,801,YummnRulesSnapshot.OldV08());r.enemies.Clear();r.spawns.Clear();Array.Clear(r.threat,0,r.threat.Length);Pos(r,x,y);return r;}
    private void Pos(KaitRun r,int x,int y)=>typeof(KaitRun).GetProperty("katePos").SetValue(r,new Vector2Int(x,y));
    private void Hp(KaitRun r,int hp)=>typeof(KaitRun).GetProperty("kateHp").SetValue(r,hp);
    private KaitEnemy E(KaitRun r,int x,int y,int hp=5,KaitEnemyType type=KaitEnemyType.Grunt)
    {var e=new KaitEnemy{id=100+r.enemies.Count,pos=new Vector2Int(x,y),hp=hp,maxHp=hp,type=type,life=KaitEnemyLife.Active};r.enemies.Add(e);return e;}
    private void S(KaitRun r,params KaitSkill[] ss){foreach(var s in ss){if(!r.skills.Contains(s))r.skills.Add(s);Assert.IsTrue(r.TryUseSkill(s,-1,out _));}}
    private void Exhaust(KaitRun r,int ki=0){r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=ki;}
    private void Lock(KaitEnemy e,Vector2Int target,KaitIntentType type=KaitIntentType.Melee)
    {e.intent=new KaitIntent{type=type,origin=e.pos,target=target,direction=new Vector2Int(Math.Sign(target.x-e.pos.x),Math.Sign(target.y-e.pos.y)),damage=1};e.intent.affectedCells.Add(target);e.rangedState=KaitRangedState.Aim;}
    private KaitTurnResult Phase(KaitRun r)
    {var result=new KaitTurnResult{yummnAction=new YummnActionContext{actionId=r.Yummn.actionId,phaseAtStart=YummnPhase.Exhausted,startCell=r.katePos}};typeof(KaitRun).GetMethod("ResolveYummnEnemyPhase",Hidden).Invoke(r,new object[]{result});return result;}
    private void Rift(KaitRun r,int x,int y,int tier=1)=>r.spawns.Add(new KaitSpawnRequest{targetCell=new Vector2Int(x,y),sourceThreatCell=new Vector2Int(x-1,y-1),tier=tier,createdTurn=r.turn});
    [Test] public void C01_KaitIsSeparateAndKeepsExistingLockDefeat()
    {var r=new KaitRun(new KaitBalanceConfig{enableThreatPillars=false,newThreatTilesPerTurn=0});r.SelectCharacter(KaitCharacter.Kait,42);for(int x=0;x<5;x++)for(int y=0;y<5;y++)r.threat[x,y]=(x+y)%2==0?2:4;typeof(KaitRun).GetMethod("FinishTurn",Hidden).Invoke(r,new object[]{new KaitTurnResult()});Assert.IsTrue(r.ended);Assert.AreEqual("Kait.0.6.1",r.RulesProfileId);Assert.IsFalse(r.EligibleAbilities().Any(YummnCatalog.IsMonk));}
    [Test] public void C02_BurstSpendsOnceMovesThreatOnceNoEnemyPhase()
    {var r=R();r.threat[2,2]=2;var result=r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(result.valid);Assert.AreEqual(2,r.Ki);Assert.AreEqual(0,r.EnemyResolveCount);Assert.AreEqual(1,r.NormalTileSpawnCount);Assert.AreEqual(1,r.turn);Assert.IsFalse(r.YummnWindow);}
    [Test] public void C03_ExhaustionRemainsWalkingUntilFull()
    {var r=R(2,3);Exhaust(r);for(int i=1;i<=3;i++){var a=r.TryGlobalInput(i%2==1?KaitDirection.Right:KaitDirection.Left);Assert.AreEqual(1,a.katePath.Count);Assert.AreEqual(i,r.Ki);Assert.AreEqual(i<3?YummnPhase.Exhausted:YummnPhase.Burst,r.KiPhase);}Assert.AreEqual(3,r.EnemyResolveCount);}
    [Test] public void C04_LastKiKillNeverEntersExhaustion()
    {var r=R();r.Yummn.ki=1;r.passives.Add(KaitPassive.PerfectSelf);E(r,2,3,1);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,r.Ki);Assert.AreEqual(YummnPhase.Burst,r.KiPhase);Assert.AreEqual(0,r.Yummn.metrics.perfectKi);}
    [Test] public void C05_ExhaustedKillFullStillRunsEnemyPhase()
    {var r=R();Exhaust(r,1);E(r,2,3,1);var e=E(r,2,4);Lock(e,new Vector2Int(2,3));r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(1,r.EnemyResolveCount);Assert.AreEqual(YummnPhase.Burst,r.KiPhase);}
    [Test] public void C06_InvalidInputHasNoSideEffectsOrPendingActivation()
    {var r=R();var pack=new KaitRewardPack();pack.choices.Add(YummnCatalog.Get("M05"));r.rewardQueue.Enqueue(pack);r.SelectReward(0);var saved=r.SaveReplay();Assert.IsFalse(r.TryGlobalInput(KaitDirection.Left).valid);Assert.AreEqual(saved,r.SaveReplay());Assert.IsTrue(r.IsAbilityPending(YummnCatalog.Get("M05")));Assert.AreEqual(3,r.Ki);}
    [Test] public void C07_InsufficientKiRejectsEntirePreparedAction()
    {var r=R();E(r,2,3);S(r,KaitSkill.Flurry,KaitSkill.Palm,KaitSkill.StunningFist);var saved=r.SaveReplay();Assert.IsFalse(r.TryGlobalInput(KaitDirection.Right).valid);Assert.AreEqual(saved,r.SaveReplay());Assert.AreEqual(3,r.Yummn.prepared.Count);Assert.AreEqual(0,r.turn);}
    [Test] public void C08_KillLandsBeforeOccupiedRiftCheck()
    {var r=R();E(r,2,3,1);Rift(r,2,3);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(new Vector2Int(2,3),r.katePos);Assert.AreEqual(1,r.spawns.Count);Assert.IsNull(r.EnemyAt(r.katePos));}
    [Test] public void C09_KillAndExhaustionCheckBirthOnlyOnce()
    {var r=R();Exhaust(r,1);E(r,2,3,1);Rift(r,4,4);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,r.Yummn.metrics.spawnChecks);Assert.AreEqual(1,r.enemies.Count(e=>e.life!=KaitEnemyLife.Dead));}
    [Test] public void C10_NewEnemyHasNoFreeAimOrMove()
    {var r=R();Exhaust(r);Rift(r,3,3,2);r.TryGlobalInput(KaitDirection.Right);var e=r.EnemyAt(new Vector2Int(3,3));Assert.NotNull(e);Assert.AreEqual(KaitIntentType.None,e.intent.type);Assert.AreEqual(KaitRangedState.Ready,e.rangedState);}
    [Test] public void C11_SwordsmanMovesThenAimsThenAttacks()
    {var r=R(3,3);var e=E(r,1,3,5,KaitEnemyType.Swordsman);Phase(r);Assert.AreEqual(new Vector2Int(2,3),e.pos);Assert.AreEqual(KaitIntentType.None,e.intent.type);Phase(r);Assert.AreEqual(KaitIntentType.Melee,e.intent.type);Assert.AreEqual(3,r.kateHp);Phase(r);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(KaitIntentType.None,e.intent.type);}
    [Test] public void C12_BurstDoesNotChangeIntentOrBossFacing()
    {var r=R();var b=E(r,4,4,8,KaitEnemyType.ShieldKnight);b.facing=Vector2Int.left;Lock(b,new Vector2Int(4,3));var intent=b.intent;r.TryGlobalInput(KaitDirection.Right);Assert.AreSame(intent,b.intent);Assert.AreEqual(Vector2Int.left,b.facing);}
    [Test] public void C13_ArrowPassesFriendsAndStopsAtDarkness()
    {var r=R(4,3);var a=E(r,1,3,2,KaitEnemyType.Archer);var friend=E(r,2,3);Lock(a,r.katePos,KaitIntentType.LineShot);Phase(r);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(5,friend.hp);Lock(a,r.katePos,KaitIntentType.LineShot);r.Yummn.darkness=new Vector2Int(3,3);Phase(r);Assert.AreEqual(2,r.kateHp);}
    [Test] public void C14_FreezeAndStunShareSingleSkippedAction()
    {var r=R();var e=E(r,2,3);Lock(e,r.katePos);e.yummnFrozen=e.yummnStunned=true;e.frozenActions=1;Phase(r);Assert.AreEqual(3,r.kateHp);Assert.AreEqual(KaitIntentType.None,e.intent.type);Assert.IsFalse(e.yummnFrozen||e.yummnStunned);Phase(r);Assert.AreEqual(KaitIntentType.Melee,e.intent.type);}
    [Test] public void C15_CounterRestoresKiWithoutStoppingOtherEnemies()
    {var r=R(3,3);Exhaust(r,1);r.passives.Add(KaitPassive.Opportunist);E(r,1,3,1,KaitEnemyType.Swordsman);var e=E(r,3,4);Lock(e,r.katePos);Phase(r);Assert.AreEqual(3,r.Ki);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);Assert.AreEqual(0,r.Yummn.metrics.spawnChecks);}
    [Test] public void C16_BossWaitsForPlayerAndDoesNotTurnAtBirth()
    {var r=R();typeof(KaitRun).GetField("bossPending",Hidden).SetValue(r,true);typeof(KaitRun).GetField("bossPendingCell",Hidden).SetValue(r,r.katePos);var spawn=typeof(KaitRun).GetMethod("SpawnShieldKnight",Hidden);spawn.Invoke(r,new object[]{new KaitTurnResult()});Assert.IsFalse(r.bossSpawned);var p=r.katePos;Pos(r,2,2);spawn.Invoke(r,new object[]{new KaitTurnResult()});var boss=r.EnemyAt(p);Assert.AreEqual(Vector2Int.down,boss.facing);Assert.AreEqual(KaitIntentType.None,boss.intent.type);}
    [Test] public void C17_ShadowUsesOnlyMainInteriorPillars()
    {var r=R();Assert.AreEqual(7,r.walls.GetLength(0));Assert.IsTrue(r.IsYummnShadow(new Vector2Int(2,5)));Assert.IsFalse(r.IsYummnShadow(new Vector2Int(1,3)));r.threatPillars[2,2]=true;Assert.IsFalse(r.IsYummnShadow(new Vector2Int(3,2)));}
    [Test] public void C18_YummnLockedThreatResetsWithoutChangingPhase()
    {var r=R();for(int x=0;x<5;x++)for(int y=0;y<5;y++)if(!r.threatPillars[x,y])r.threat[x,y]=(x+y)%2==0?2:4;r.TryGlobalInput(KaitDirection.Right);Assert.IsFalse(r.ended);Assert.AreEqual(1,r.threatLocks);Assert.AreEqual(3,r.threat.Cast<int>().Count(v=>v==2));Assert.AreEqual(2,r.Ki);}
    [Test] public void K01_CombinedFlurryPalmCostsThreeAndEmitsThreeHits()
    {var r=R();var e=E(r,2,3,4);S(r,KaitSkill.Flurry,KaitSkill.Palm);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(3,a.yummnAction.totalKiCost);Assert.AreEqual(1,e.hp);Assert.AreEqual(new Vector2Int(3,3),e.pos);Assert.AreEqual(3,a.yummnEvents.Count(ev=>ev.kind==YummnEventKind.Hit));}
    [Test] public void K02_OpenHandAndPalmDoNotDoublePush()
    {var r=R();var e=E(r,2,3,5);S(r,KaitSkill.Flurry,KaitSkill.Palm);r.passives.Add(KaitPassive.OpenHand);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(new Vector2Int(3,3),e.pos);Assert.AreEqual(1,a.yummnEvents.Count(ev=>ev.moveCause==YummnMoveCause.Forced));}
    [Test] public void K03_FollowThroughGoesOnlyToOriginalCell()
    {var r=R();E(r,2,3,5);S(r,KaitSkill.UnbrokenAir);r.passives.Add(KaitPassive.FollowThrough);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(new Vector2Int(2,3),r.katePos);Assert.NotNull(r.EnemyAt(new Vector2Int(5,3)));}
    [Test] public void K04_QuiveringNeedsDifferentActionAndDirection()
    {var r=R();var e=E(r,2,3,5);r.passives.Add(KaitPassive.QuiveringPalm);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(4,e.hp);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(3,e.hp);Pos(r,2,4);r.TryGlobalInput(KaitDirection.Down);Assert.AreEqual(0,e.hp);Assert.AreEqual(new Vector2Int(2,4),r.katePos,"secondary kill must not teleport");Assert.AreEqual(-1,r.Yummn.palmEnemyId);}
    [Test] public void K05_WholenessHealsOnlyOnExhaustionExit()
    {var r=R(2,3);Hp(r,1);r.passives.Add(KaitPassive.Wholeness);Exhaust(r,2);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,r.kateHp);E(r,4,3,1);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(1,r.Yummn.metrics.heals);}
    [Test] public void K06_FireSnakeOnlyOnceAfterThreePunches()
    {var r=R();E(r,2,3,3);var rear=E(r,3,3,2);S(r,KaitSkill.Flurry);r.passives.Add(KaitPassive.FireSnake);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,rear.hp);Assert.AreEqual(1,a.yummnEvents.Count(ev=>ev.damageCause==YummnDamageCause.FireSnake&&ev.kind==YummnEventKind.Hit));Assert.AreEqual(new Vector2Int(2,3),r.katePos);}
    [Test] public void K07_ShatterDoesNotChainAndPreservesStunSource()
    {var r=R();var a=E(r,2,3);var b=E(r,3,3);var c=E(r,4,3);a.yummnFrozen=a.yummnStunned=b.yummnFrozen=true;a.frozenActions=b.frozenActions=1;r.skills.Add(KaitSkill.FrostBreath);r.passives.Add(KaitPassive.ShatteringPalm);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(4,b.hp);Assert.AreEqual(5,c.hp);Assert.IsTrue(b.yummnFrozen);Assert.IsFalse(a.yummnFrozen);Assert.IsTrue(a.yummnStunned);Assert.AreEqual(1,a.frozenActions);}
    [Test] public void K08_IceValidatesBeforeReplacingOldPillar()
    {var r=R(3,3);r.Yummn.icePillar=new Vector2Int(2,3);S(r,KaitSkill.ShapeIce);var saved=r.SaveReplay();Assert.IsFalse(r.TryGlobalInput(KaitDirection.Left).valid);Assert.AreEqual(saved,r.SaveReplay());Assert.AreEqual(new Vector2Int(2,3),r.Yummn.icePillar);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(new Vector2Int(4,3),r.Yummn.icePillar);Assert.IsFalse(r.IsHardBlocked(new Vector2Int(2,3)));}
    [Test] public void K09_DarknessLivesThroughBurstNotBeyondEnemyPhase()
    {var r=R(2,3);S(r,KaitSkill.Darkness);r.TryGlobalInput(KaitDirection.Up);var cell=r.Yummn.darkness;Assert.AreEqual(new Vector2Int(2,4),cell);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(cell,r.Yummn.darkness);Phase(r);Assert.AreEqual(YummnRun.NoCell,r.Yummn.darkness);}
    [Test] public void K10_ShadowStepFindsNearestSameLineEmptyShadow()
    {var r=R(2,2);r.Yummn.icePillar=new Vector2Int(3,3);E(r,2,3);S(r,KaitSkill.YummnShadowStep);r.TryGlobalInput(KaitDirection.Up);Assert.AreEqual(new Vector2Int(2,5),r.katePos);Assert.AreEqual(2,r.enemies[0].pos.x);}
    [Test] public void K11_CloakDeniesOneAimAndDoesNotTurnBoss()
    {var r=R(2,5);r.Yummn.cloak=true;var b=E(r,2,4,8,KaitEnemyType.ShieldKnight);b.facing=Vector2Int.left;var a=E(r,4,5,2,KaitEnemyType.Archer);Phase(r);Assert.AreEqual(KaitIntentType.None,b.intent.type);Assert.AreEqual(Vector2Int.left,b.facing);Assert.AreEqual(KaitRangedState.Aim,a.rangedState);}
    [Test] public void K12_TraceSuppressesOnlyNewTwoNotMerge()
    {var r=R();r.passives.Add(KaitPassive.PassWithoutTrace);r.threat[1,2]=16;r.threat[2,2]=16;var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,a.merges.Count);Assert.AreEqual(1,r.rewardQueue.Count);Assert.AreEqual(0,a.newThreatCells.Count);Assert.IsTrue(a.yummnAction.suppressedTwo);}
    [Test] public void K13_ShadowAssaultUsesStartingCellNotTraversedShadow()
    {var r=R(2,5);var b=E(r,3,5,8,KaitEnemyType.ShieldKnight);b.facing=Vector2Int.left;r.passives.Add(KaitPassive.ShadowAssault);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(7,b.hp);r=R(2,2);r.Yummn.icePillar=new Vector2Int(3,3);b=E(r,2,5,8,KaitEnemyType.ShieldKnight);b.facing=Vector2Int.down;r.passives.Add(KaitPassive.ShadowAssault);r.TryGlobalInput(KaitDirection.Up);Assert.AreEqual(8,b.hp);}
    [Test] public void K14_ForcedMovementDoesNotTriggerOpportunist()
    {var r=R();var e=E(r,4,3,5,KaitEnemyType.Swordsman);r.passives.Add(KaitPassive.Opportunist);S(r,KaitSkill.WaterWhip);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(4,e.hp);Assert.AreEqual(new Vector2Int(2,3),e.pos);Assert.AreEqual(new Vector2Int(1,3),r.katePos);}
    [Test] public void K15_DefensesDoNotDoubleConsume()
    {var r=R(4,3);r.passives.Add(KaitPassive.DeflectMissiles);r.Yummn.defense=true;var a=E(r,1,3,2,KaitEnemyType.Archer);Lock(a,r.katePos,KaitIntentType.LineShot);var b=E(r,4,4);Lock(b,r.katePos);var c=E(r,5,3);Lock(c,r.katePos);var result=Phase(r);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(2,result.enemyActions.Count(e=>e.guarded));}
    [Test] public void K16_CatalogAndRealPrerequisitesAreStable()
    {var r=R();Assert.AreEqual(23,YummnCatalog.Cards.Count);Assert.AreEqual(10,YummnCatalog.Cards.Count(d=>d.kind==KaitAbilityKind.Active));Assert.AreEqual(23,YummnCatalog.Cards.Select(d=>d.id).Distinct().Count());Assert.IsFalse(r.EligibleAbilities(new System.Collections.Generic.List<KaitAbilityDef>{YummnCatalog.Get("M01")}).Contains(YummnCatalog.Get("O02")));r.skills.Add(KaitSkill.Flurry);Assert.IsTrue(r.EligibleAbilities().Contains(YummnCatalog.Get("O02")));}
    [Test] public void K17_ArchiveRunsWithoutNewTwoAndOnlyOnce()
    {var r=R();r.passives.AddRange(new[]{KaitPassive.PassWithoutTrace,KaitPassive.OldNewsArchive});for(int y=0;y<5;y++)r.threat[2,y]=2;var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(0,a.newThreatCells.Count);Assert.AreEqual(1,a.merges.Count(m=>m.systemMerge));Assert.AreEqual(1,r.PassiveTriggerCount(KaitPassive.OldNewsArchive));}
    [Test] public void K18_StaleInputCannotDeductOrReplayAction()
    {var r=R();Assert.IsTrue(r.TryYummnCommand(KaitDirection.Right,0,0).valid);var save=r.SaveReplay();Assert.IsFalse(r.TryYummnCommand(KaitDirection.Left,0,0).valid);Assert.AreEqual(save,r.SaveReplay());Assert.AreEqual(1,r.Yummn.actionId);}
}
