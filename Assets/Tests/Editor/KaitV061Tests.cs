using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class KaitV061Tests
{
    private static KaitRun Run()
    {
        var r=new KaitRun(new KaitBalanceConfig { initialThreatTiles=0,newThreatTilesPerTurn=0,playerInvincible=true });r.Reset(612);
        for(int y=1;y<6;y++)for(int x=1;x<6;x++)r.walls[x,y]=false;
        Array.Clear(r.threatPillars,0,r.threatPillars.Length);return r;
    }
    private static object Call(KaitRun r,string name,params object[] args)=>typeof(KaitRun).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(r,args);
    private static void Set(KaitRun r,string name,object value)=>typeof(KaitRun).GetProperty(name).SetValue(r,value);
    private static KaitMergeEvent Merge(int value=32)=>new KaitMergeEvent {sourceValue=value/2,resultValue=value,threatCell=new Vector2Int(0,0),actualThreatDirection=KaitDirection.Right};
    private static KaitEnemy Enemy(KaitRun r,int x=3,int y=3,int hp=10,KaitEnemyType type=KaitEnemyType.Grunt)
    { var e=new KaitEnemy{id=r.enemies.Count+1,pos=new Vector2Int(x,y),hp=hp,maxHp=hp,type=type,life=KaitEnemyLife.Active};r.enemies.Add(e);return e; }
    [Test] public void CatalogContains38EnabledCardsAndOneDisabledExperiment()
    {Assert.AreEqual(14,KaitAbilityCatalog.All.Count(d=>d.kind==KaitAbilityKind.Active));Assert.AreEqual(24,KaitAbilityCatalog.All.Count(d=>d.kind==KaitAbilityKind.Passive&&!d.experimental));Assert.AreEqual(39,KaitAbilityCatalog.All.Select(d=>d.id).Distinct().Count());}
    [Test] public void OnlyActual32MergesRewardAndEveryOccurrenceQueues()
    {var r=Run();foreach(int v in new[]{16,32,32,64,128})Call(r,"HandleMilestoneMerge",Merge(v));Assert.AreEqual(2,r.rewardQueue.Count);Assert.AreEqual(0,r.pendingSkillMilestone);Assert.AreEqual(0,r.pendingPassiveMilestone);}
    [Test] public void PacksStayFixedWhileOtherPacksAreCreated()
    {var r=Run();r.EnqueueMergeReward(Merge());var first=r.CurrentReward;var ids=first.choices.Select(d=>d.id).ToArray();for(int i=0;i<8;i++)r.EnqueueMergeReward(Merge());Assert.AreSame(first,r.CurrentReward);CollectionAssert.AreEqual(ids,first.choices.Select(d=>d.id));}
    [Test] public void WeightedPacksHaveBothKindsNoDuplicatesAndAtMostOneRare()
    {var r=Run();for(int i=0;i<500;i++){r.EnqueueMergeReward(Merge());var p=r.CurrentReward;Assert.AreEqual(3,p.choices.Count);Assert.AreEqual(3,p.choices.Distinct().Count());Assert.IsTrue(p.choices.Any(d=>d.kind==KaitAbilityKind.Active));Assert.IsTrue(p.choices.Any(d=>d.kind==KaitAbilityKind.Passive));Assert.LessOrEqual(p.choices.Count(d=>d.rarity==KaitRarity.Rare),1);Assert.IsFalse(p.choices.Any(d=>d.experimental));r.SkipReward();}}
    [Test] public void FourthPackGuaranteesRareAfterThreeMisses()
    {var r=Run();Set(r,"packsWithoutRare",3);r.EnqueueMergeReward(Merge());Assert.AreEqual(1,r.CurrentReward.choices.Count(d=>d.rarity==KaitRarity.Rare));Assert.AreEqual(0,r.packsWithoutRare);}
    [Test] public void PrerequisitesFilterBeforeDraftButAllowCurseInSamePack()
    {var r=Run();Assert.IsFalse(r.EligibleAbilities().Any(d=>d.passive==KaitPassive.MasterHex));Assert.IsTrue(r.EligibleAbilities(new List<KaitAbilityDef>{KaitAbilityCatalog.Get(KaitSkill.HexCurse)}).Any(d=>d.passive==KaitPassive.MasterHex));Assert.IsFalse(r.EligibleAbilities().Any(d=>d.passive==KaitPassive.Simulacrum));}
    [Test] public void NewCardDoesNotAffectCurrentRulesAndActivatesNextInput()
    {var r=Run();r.EnqueueMergeReward(Merge());r.CurrentReward.choices.Clear();r.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitPassive.ReverseGravity));Assert.IsTrue(r.SelectReward(0));Assert.IsFalse(r.HasPassive(KaitPassive.ReverseGravity));r.threat[2,2]=2;r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(KaitDirection.Left,r.actualThreatDirection);Assert.IsTrue(r.HasPassive(KaitPassive.ReverseGravity));}
    [Test] public void ReplacingPassiveRetainsOldRuleUntilNextInput()
    {var r=Run();r.passives.AddRange(new[]{KaitPassive.BirdEye,KaitPassive.CheshireCat,KaitPassive.Trend});r.EnqueueMergeReward(Merge());r.CurrentReward.choices.Clear();r.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitPassive.ReverseGravity));Assert.IsFalse(r.SelectReward(0));Assert.IsTrue(r.SelectReward(0,2));Assert.IsTrue(r.HasPassive(KaitPassive.Trend));Assert.IsFalse(r.HasPassive(KaitPassive.ReverseGravity));Call(r,"ActivateBuildForInput");Assert.IsFalse(r.HasPassive(KaitPassive.Trend));Assert.IsTrue(r.HasPassive(KaitPassive.ReverseGravity));}
    [Test] public void SimulacrumKeepsCopiedEffectAfterOriginalIsRemoved()
    {var r=Run();r.passives.Add(KaitPassive.BagHolding);r.EnqueueMergeReward(Merge());r.CurrentReward.choices.Clear();r.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitPassive.Simulacrum));Assert.IsTrue(r.SelectReward(0,-1,KaitPassive.BagHolding));Call(r,"ActivateBuildForInput");r.passives.Remove(KaitPassive.BagHolding);Assert.IsTrue(r.HasPassive(KaitPassive.BagHolding));}
    [Test] public void SpeedOrderIsIndependentOfClickOrder()
    {Assert.AreEqual(6,KaitRun.CalculateSpeed(2,new[]{KaitSpeedModifier.Double,KaitSpeedModifier.AddOne}));Assert.AreEqual(6,KaitRun.CalculateSpeed(2,new[]{KaitSpeedModifier.AddOne,KaitSpeedModifier.Double}));}
    [TestCase(false,2)] [TestCase(true,4)] public void PasswallCompressesOnlyNonPillarLogicalSequence(bool enabled,int expected)
    {var r=Run();r.threatPillars[1,2]=true;r.threat[0,2]=r.threat[2,2]=2;if(enabled)r.passives.Add(KaitPassive.Passwall);Call(r,"MoveThreat",KaitDirection.Right,new List<KaitThreatMotion>());Assert.AreEqual(expected,r.threat[4,2]);Assert.AreEqual(0,r.threat[1,2]);Assert.IsTrue(r.threatPillars[1,2]);}
    [Test] public void BagRunsAfterAllMergesAndDoesNotRecompress()
    {var r=Run();r.passives.Add(KaitPassive.BagHolding);r.threat[0,2]=r.threat[1,2]=r.threat[2,2]=2;Set(r,"actualThreatDirection",KaitDirection.Right);var merges=(List<KaitMergeEvent>)Call(r,"MoveThreat",KaitDirection.Right,new List<KaitThreatMotion>());Call(r,"ResolveBag",merges,new KaitTurnResult());Assert.AreEqual(4,r.threat[4,2]);Assert.AreEqual(0,r.threat[3,2]);Assert.AreEqual(1,merges.Count);}
    [Test] public void BagUsesActualReversedDirection()
    {var r=Run();r.passives.Add(KaitPassive.BagHolding);r.threat[0,0]=r.threat[4,1]=2;var m=Merge(4);m.actualThreatDirection=KaitDirection.Left;Call(r,"ResolveBag",new[]{m},new KaitTurnResult());Assert.AreEqual(2,r.threat[0,0]);Assert.AreEqual(0,r.threat[4,1]);}
    [Test] public void ArchiveIsOldestTwoOnceAndBagMayRespond()
    {var r=Run();r.passives.AddRange(new[]{KaitPassive.OldNewsArchive,KaitPassive.BagHolding});for(int x=0;x<5;x++){r.threat[x,0]=2;r.threatTwoBirth[x,0]=5-x;}var result=new KaitTurnResult();Call(r,"ResolveOldNewsArchive",result);Call(r,"ResolveOldNewsArchive",result);Assert.AreEqual(4,r.threat[4,0]);Assert.AreEqual(0,r.threat[3,0]);Assert.AreEqual(1,result.merges.Count);Assert.IsTrue(result.merges[0].systemMerge);Assert.AreEqual(1,r.spawns.Count);Assert.AreEqual(2,r.threat.Cast<int>().Count(v=>v==2));}
    [Test] public void MirrorIsConsumedByExactlyOneRift()
    {var r=Run();r.skills.Add(KaitSkill.DimensionDoor);Assert.IsTrue(r.TryUseSkill(KaitSkill.DimensionDoor,-1,out _));Call(r,"QueueSpawn",Merge(4),new KaitTurnResult());Call(r,"QueueSpawn",Merge(4),new KaitTurnResult());Assert.AreEqual(new Vector2Int(5,5),r.spawns[0].targetCell);Assert.AreEqual(new Vector2Int(1,1),r.spawns[1].targetCell);}
    [Test] public void ConsolidationUsesFirstPairFirstCellOnlyOnce()
    {var r=Run();r.passives.Add(KaitPassive.Simplify);for(int i=0;i<4;i++)r.spawns.Add(new KaitSpawnRequest{tier=1,createdTurn=0,targetCell=new Vector2Int(i+1,1)});Call(r,"ResolveSimplify",new KaitTurnResult());Assert.AreEqual(3,r.spawns.Count);Assert.AreEqual(2,r.spawns[0].tier);Assert.AreEqual(new Vector2Int(1,1),r.spawns[0].targetCell);}
    [Test] public void ArcaneLockDelaysOccupiedRiftOnlyOnce()
    {var r=Run();r.passives.Add(KaitPassive.ArcaneLock);r.spawns.Add(new KaitSpawnRequest{tier=1,createdTurn=-1,targetCell=r.katePos,turnsUntilSpawn=0});Call(r,"ResolveSpawnRequests",new KaitTurnResult());Assert.AreEqual(1,r.spawns.Count);Assert.IsTrue(r.spawns[0].lockDelayed);Call(r,"ResolveSpawnRequests",new KaitTurnResult());Assert.AreEqual(0,r.spawns.Count);}
    [Test] public void OccupiedGlyphDoesNotConsumeOrChangeRift()
    {var r=Run();r.passives.Add(KaitPassive.BloodBookmark);Set(r,"hasBookmark",true);Set(r,"bookmarkCell",r.katePos);object[] args={new Vector2Int(1,1),new KaitTurnResult()};Assert.IsFalse((bool)Call(r,"TryRedirectSpawnToBookmark",args));Assert.IsTrue(r.hasBookmark);}
    [Test] public void GlyphRedirectGetsFullPreviewBeforeSpawning()
    {var r=Run();r.passives.Add(KaitPassive.BloodBookmark);Set(r,"hasBookmark",true);Set(r,"bookmarkCell",new Vector2Int(1,1));r.spawns.Add(new KaitSpawnRequest{tier=1,createdTurn=-1,targetCell=r.katePos,turnsUntilSpawn=0});Call(r,"ResolveSpawnRequests",new KaitTurnResult());Assert.IsEmpty(r.enemies);Assert.AreEqual(new Vector2Int(1,1),r.spawns[0].targetCell);Assert.AreEqual(0,r.spawns[0].createdTurn);}
    [Test] public void TelekinesisNeverMergesSameValue()
    {var r=Run();r.passives.Add(KaitPassive.MomentumResonance);r.threat[0,0]=r.threat[1,0]=2;var result=new KaitTurnResult();Call(r,"ResolveMomentumResonance",new Vector2Int(1,1),Vector2Int.right,result);Assert.AreEqual(2,r.threat[0,0]);Assert.AreEqual(2,r.threat[1,0]);Assert.IsEmpty(result.merges);}
    [Test] public void CurseAddsToFriendlyDamageAndSurvivesZeroDamage()
    {var r=Run();var e=Enemy(r);e.cursed=true;Call(r,"DamageEnemy",e,0,true,new KaitTurnResult(),false);Assert.IsTrue(e.cursed);Call(r,"DamageEnemy",e,1,false,new KaitTurnResult(),true);Assert.AreEqual(8,e.hp);Assert.IsFalse(e.cursed);}
    [Test] public void CurseAndNonlethalFriendlyFireLeaveOneHp()
    {var r=Run();r.passives.Add(KaitPassive.CheshireCat);var e=Enemy(r,hp:2);e.cursed=true;Call(r,"DamageEnemy",e,1,false,new KaitTurnResult(),true);Assert.AreEqual(1,e.hp);Assert.IsFalse(e.cursed);}
    [Test] public void HoldMonsterSkipsSingleAction()
    {var r=Run();r.skills.Add(KaitSkill.IceTomb);var e=Enemy(r,4,3);Assert.IsTrue(r.TryUseSkill(KaitSkill.IceTomb,e.id,out _));Call(r,"LockEnemyIntents");Call(r,"ResolveEnemyIntents",new KaitTurnResult());Assert.AreEqual(0,e.frozenActions);}
    [Test] public void TombProtectsButStillMovesThreat()
    {var r=Run();r.config.playerInvincible=false;r.skills.Add(KaitSkill.LevistusTomb);r.threat[0,0]=2;var start=r.katePos;Assert.IsTrue(r.TryUseSkill(KaitSkill.LevistusTomb,-1,out _));var result=r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(result.valid);Assert.AreEqual(start,r.katePos);Assert.AreEqual(2,r.threat[4,0]);Assert.AreEqual(1,r.turn);}
    [Test] public void MistyStepDoesNotAdvanceEitherBoardOrTurn()
    {var r=Run();r.skills.Add(KaitSkill.MistyStep);r.threat[0,0]=2;Assert.IsTrue(r.TryUseSkillAt(KaitSkill.MistyStep,r.katePos+Vector2Int.up,out _));Assert.AreEqual(0,r.turn);Assert.AreEqual(2,r.threat[0,0]);}
    [Test] public void RelentlessHexRequiresCursedAdjacentDestination()
    {var r=Run();r.skills.Add(KaitSkill.RelentlessHex);var e=Enemy(r,1,1);Assert.IsFalse(r.TryUseSkillAt(KaitSkill.RelentlessHex,new Vector2Int(1,2),out _));e.cursed=true;Assert.IsTrue(r.TryUseSkillAt(KaitSkill.RelentlessHex,new Vector2Int(1,2),out _));}
    [Test] public void DispelRemovesOnlySelectedRiftAndConsumesNoTurn()
    {var r=Run();r.skills.Add(KaitSkill.DispelMagic);r.spawns.Add(new KaitSpawnRequest{targetCell=new Vector2Int(1,1)});r.spawns.Add(new KaitSpawnRequest{targetCell=new Vector2Int(2,1)});Assert.IsTrue(r.TryUseSkillAt(KaitSkill.DispelMagic,new Vector2Int(1,1),out _));Assert.AreEqual(1,r.spawns.Count);Assert.AreEqual(0,r.turn);Assert.AreEqual(4,r.SkillCooldown(KaitSkill.DispelMagic));}
    [Test] public void CommandRotatesArcherLockClockwiseWithoutAdvancingTime()
    {var r=Run();r.skills.Add(KaitSkill.Command);var e=Enemy(r,2,2,type:KaitEnemyType.Archer);e.intent=(KaitIntent)Call(r,"BuildLineIntent",e.pos,Vector2Int.up,5,false);e.rangedState=KaitRangedState.Aim;Assert.IsTrue(r.TryUseSkill(KaitSkill.Command,e.id,out _));Assert.AreEqual(Vector2Int.right,e.intent.direction);Assert.IsTrue(e.intent.affectedCells.All(p=>p.y==2&&p.x>2));Assert.AreEqual(0,r.turn);}
    [Test] public void GraspPullsOnlyClearAlignedEnemyAndEmitsMotion()
    {var r=Run();r.skills.Add(KaitSkill.GraspHadar);var e=Enemy(r,3,5);Assert.IsTrue(r.TryUseSkill(KaitSkill.GraspHadar,e.id,out _));Assert.AreEqual(new Vector2Int(3,4),e.pos);Assert.AreEqual(1,r.lastSkillResult.enemyActions.Count);Assert.AreEqual(0,r.turn);var blocked=Run();blocked.skills.Add(KaitSkill.GraspHadar);var far=Enemy(blocked,3,5);Enemy(blocked,3,4);Assert.IsFalse(blocked.TryUseSkill(KaitSkill.GraspHadar,far.id,out _));}
    [Test] public void HexArmorCancelsOnlyFirstAttackOfCursedTarget()
    {var r=Run();r.passives.Add(KaitPassive.HexArmor);var e=Enemy(r,4,3);e.cursed=true;var intent=new KaitIntent{type=KaitIntentType.Melee};Assert.IsTrue((bool)Call(r,"CancelEnemyAttack",e,intent,new KaitTurnResult()));Assert.IsFalse((bool)Call(r,"CancelEnemyAttack",e,intent,new KaitTurnResult()));Assert.IsTrue(e.cursed);}
    [Test] public void SpecterDoesNotCancelMovementAndExpiresOnAttack()
    {var r=Run();Set(r,"specterReady",true);var e=Enemy(r);Assert.IsFalse((bool)Call(r,"CancelEnemyAttack",e,new KaitIntent{type=KaitIntentType.Move},new KaitTurnResult()));Assert.IsTrue(r.specterReady);Assert.IsTrue((bool)Call(r,"CancelEnemyAttack",e,new KaitIntent{type=KaitIntentType.Melee},new KaitTurnResult()));Assert.IsFalse(r.specterReady);}
    [Test] public void CloakOnlyCancelsFirstRemoteAttackThatReachesKait()
    {var r=Run();r.passives.Add(KaitPassive.DisplacementCloak);var e=Enemy(r,1,3,type:KaitEnemyType.Archer);var intent=new KaitIntent{type=KaitIntentType.LineShot};Assert.IsFalse((bool)Call(r,"CancelEnemyAttack",e,intent,new KaitTurnResult()));intent.affectedCells.Add(r.katePos);Assert.IsTrue((bool)Call(r,"CancelEnemyAttack",e,intent,new KaitTurnResult()));Assert.IsFalse((bool)Call(r,"CancelEnemyAttack",e,intent,new KaitTurnResult()));}
    [Test] public void FriendlyDamageAppliesOneSharedSkipWithoutStacking()
    {var r=Run();r.passives.Add(KaitPassive.Enfeeblement);var e=Enemy(r);Call(r,"DamageEnemy",e,1,false,new KaitTurnResult(),true);Call(r,"DamageEnemy",e,1,false,new KaitTurnResult(),true);Assert.AreEqual(1,e.frozenActions);}
    [Test] public void StaggerNeedsInnerPillarNotOuterBoundary()
    {var r=Run();r.passives.Add(KaitPassive.StaggeringSmite);var e=Enemy(r);Call(r,"ApplyPillarStagger",e,new Vector2Int(0,3),new KaitTurnResult());Assert.AreEqual(0,e.frozenActions);r.walls[2,2]=true;Call(r,"ApplyPillarStagger",e,new Vector2Int(2,2),new KaitTurnResult());Assert.AreEqual(1,e.frozenActions);}
    [Test] public void MaddeningHexOnlyProcsOncePerTurn()
    {var r=Run();r.passives.Add(KaitPassive.MaddeningHex);var e=Enemy(r,2,2);var neighbour=Enemy(r,2,3);e.cursed=true;Call(r,"DamageEnemy",e,1,true,new KaitTurnResult(),false);e.cursed=true;Call(r,"DamageEnemy",e,1,true,new KaitTurnResult(),false);Assert.AreEqual(9,neighbour.hp);Assert.AreEqual(1,r.PassiveTriggerCount(KaitPassive.MaddeningHex));}
    [Test] public void LifedrinkerAndSpecterProcOnlyOncePerChain()
    {var r=Run();Set(r,"kateHp",1);r.passives.AddRange(new[]{KaitPassive.Lifedrinker,KaitPassive.AccursedSpecter});var a=Enemy(r,1,1,hp:1);var b=Enemy(r,1,2,hp:1);a.cursed=b.cursed=true;Call(r,"DamageEnemy",a,1,true,new KaitTurnResult(),false);Call(r,"DamageEnemy",b,1,true,new KaitTurnResult(),false);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(1,r.PassiveTriggerCount(KaitPassive.AccursedSpecter));Assert.IsTrue(r.specterReady);}
    [Test] public void RodOnlyReducesAnotherSkillOnFirstCastPerTurn()
    {var r=Run();r.passives.Add(KaitPassive.Devil);r.skills.AddRange(new[]{KaitSkill.SwiftBoots,KaitSkill.CatAgility});var cds=(Dictionary<KaitSkill,int>)typeof(KaitRun).GetField("skillCooldowns",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(r);cds[KaitSkill.CatAgility]=4;Assert.IsTrue(r.TryUseSkill(KaitSkill.SwiftBoots,-1,out _));Call(r,"ResolveDevil",KaitSkill.SwiftBoots,new KaitTurnResult());Assert.AreEqual(3,r.SkillCooldown(KaitSkill.CatAgility));}
    [Test] public void BladeOnlyProcsAtThreeNotSixKills()
    {var r=Run();r.passives.Add(KaitPassive.BladeCovenant);r.skills.Add(KaitSkill.CatAgility);var cds=(Dictionary<KaitSkill,int>)typeof(KaitRun).GetField("skillCooldowns",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(r);cds[KaitSkill.CatAgility]=5;Set(r,"currentChainKills",3);Call(r,"ResolveBladeCovenant",new KaitTurnResult());Set(r,"currentChainKills",6);Call(r,"ResolveBladeCovenant",new KaitTurnResult());Assert.AreEqual(4,r.SkillCooldown(KaitSkill.CatAgility));}
    [Test] public void LuckRerollsHeadOnlyOnceAndKeepsFollowingPack()
    {var r=Run();r.passives.Add(KaitPassive.LuckBlade);r.EnqueueMergeReward(Merge());r.EnqueueMergeReward(Merge());var tail=r.rewardQueue.Last();var ids=tail.choices.Select(d=>d.id).ToArray();Assert.IsTrue(r.RerollReward());Assert.IsFalse(r.RerollReward());CollectionAssert.AreEqual(ids,tail.choices.Select(d=>d.id));}
    [Test] public void MasterHexTransfersToNextHitWithinSameChain()
    {var r=Run();Set(r,"katePos",new Vector2Int(1,3));r.passives.Add(KaitPassive.MasterHex);var a=Enemy(r,3,3,hp:1);var b=Enemy(r,5,3,hp:2);a.cursed=true;r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(r.chainActive);var next=r.ContinueChain(KaitDirection.Right);Assert.Contains(b.id,next.killedEnemyIds);Assert.AreEqual(2,r.PassiveTriggerCount(KaitPassive.MasterHex));}
    [Test] public void EldritchSmitePushesSurvivorToEnd()
    {var r=Run();Set(r,"katePos",new Vector2Int(1,3));r.skills.Add(KaitSkill.EldritchSmite);var e=Enemy(r,3,3,hp:20);Assert.IsTrue(r.TryUseSkill(KaitSkill.EldritchSmite,-1,out _));var result=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(new Vector2Int(5,3),e.pos);Assert.IsTrue(result.pushed);Assert.IsFalse(r.smiteArmed);}
    [Test] public void RepellingBlastPropagatesOneNeighbourOnly()
    {var r=Run();Set(r,"katePos",new Vector2Int(1,3));r.passives.Add(KaitPassive.RepellingBlast);var a=Enemy(r,3,3,hp:20);var b=Enemy(r,4,3,hp:20);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(new Vector2Int(4,3),a.pos);Assert.AreEqual(new Vector2Int(5,3),b.pos);Assert.AreEqual(1,r.PassiveTriggerCount(KaitPassive.RepellingBlast));}
    [Test] public void ReverseGravityMovesThreatOppositeWithoutChangingKaitDirection()
    {var r=Run();r.passives.Add(KaitPassive.ReverseGravity);r.threat[2,0]=2;var result=r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(result.valid);Assert.AreEqual(KaitDirection.Left,r.actualThreatDirection);Assert.AreEqual(new Vector2Int(5,3),r.katePos);Assert.AreEqual(2,r.threat[0,0]);}
    [Test] public void InvalidInputKeepsSelectedAbilityPending()
    {var r=Run();Set(r,"katePos",new Vector2Int(5,3));r.EnqueueMergeReward(Merge());r.CurrentReward.choices.Clear();r.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitSkill.HexCurse));Assert.IsTrue(r.SelectReward(0));Assert.IsFalse(r.TryGlobalInput(KaitDirection.Right).valid);Assert.IsTrue(r.IsAbilityPending(KaitAbilityCatalog.Get(KaitSkill.HexCurse)));Assert.IsFalse(r.IsSkillActive(KaitSkill.HexCurse));}
    [Test] public void NonBossShieldKnightDoesNotWinRun()
    {var r=Run();var e=Enemy(r,1,1,hp:1,type:KaitEnemyType.ShieldKnight);Call(r,"DamageEnemy",e,1,true,new KaitTurnResult(),false);Assert.IsFalse(r.ended);}
    [Test] public void BossOnlyTriggeredBy256()
    {var r=Run();Call(r,"HandleMilestoneMerge",Merge(128));Assert.IsFalse((bool)typeof(KaitRun).GetField("bossPending",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(r));Call(r,"HandleMilestoneMerge",Merge(256));Call(r,"SpawnShieldKnight",new KaitTurnResult());Assert.IsTrue(r.bossSpawned);var boss=r.enemies.Single();Call(r,"DamageEnemy",boss,100,true,new KaitTurnResult(),false);Assert.IsTrue(r.won);}
}
