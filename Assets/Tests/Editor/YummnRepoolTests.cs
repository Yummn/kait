using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class YummnRepoolTests
{
    private KaitRun New(int kill=3,YummnTileSupplyMode supply=YummnTileSupplyMode.KillOnly)
    {var r=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0});r.SelectCharacter(KaitCharacter.Yummn,412,YummnRulesSnapshot.Current(killKi:kill,supply:supply));return r;}
    private KaitEnemy Enemy(KaitRun r,Vector2Int p,int hp=4,KaitEnemyType type=KaitEnemyType.Grunt)
    {var e=new KaitEnemy{id=100+r.enemies.Count,pos=p,hp=hp,maxHp=hp,type=type,life=KaitEnemyLife.Active,intent=new KaitIntent{origin=p}};r.enemies.Add(e);return e;}
    private void Offer(KaitRun r,KaitAbilityDef d)
    {var pack=new KaitRewardPack();pack.choices.Add(d);r.rewardQueue.Enqueue(pack);}
    [Test] public void PoolHas38UniqueNamedNonExperimentalVariants()
    {
        var c=YummnCatalog.Pool();Assert.AreEqual(38,c.Count);
        Assert.AreEqual(38,c.Select(x=>x.id).Distinct().Count());Assert.AreEqual(38,c.Select(x=>x.nameZh).Distinct().Count());
        Assert.IsNull(YummnCatalog.Get("R18"));Assert.IsFalse(c.Any(x=>x.passive==KaitPassive.FirstEchoWard));
        Assert.IsFalse(c.Any(x=>x.experimental));Assert.IsFalse(c.Any(x=>x.skill==KaitSkill.Flurry));
        foreach(var d in c){Assert.IsNotNull(KaitCardSkin.Icon(d),d.id);Assert.IsNotNull(KaitCardSkin.Face(d),d.id);}
    }
    [Test] public void SixActiveCardsAndCrossTypeReplacement()
    {
        var r=New();foreach(var d in YummnCatalog.Cards.Where(x=>x.kind==KaitAbilityKind.Active).Take(6)){Offer(r,d);Assert.IsTrue(r.SelectReward(0));}
        Assert.AreEqual(6,r.skills.Count);
        Offer(r,YummnCatalog.Get("R01"));Assert.IsFalse(r.SelectReward(0));Assert.IsTrue(r.SelectReward(0,2));
        Assert.AreEqual(5,r.skills.Count);Assert.AreEqual(1,r.passives.Count);
    }
    [Test] public void SixPassiveCardsAndCrossTypeReplacement()
    {
        var r=New();foreach(var d in YummnCatalog.Cards.Where(x=>x.kind==KaitAbilityKind.Passive).Take(6)){Offer(r,d);Assert.IsTrue(r.SelectReward(0));}
        Offer(r,YummnCatalog.Get("R02"));Assert.IsTrue(r.SelectReward(0,3));
        Assert.AreEqual(1,r.skills.Count);Assert.AreEqual(5,r.passives.Count);
    }
    [Test] public void KaitStillHasThreePerType()
    {var r=new KaitRun();r.SelectCharacter(KaitCharacter.Kait,412);foreach(var d in KaitAbilityCatalog.All.Where(x=>x.kind==KaitAbilityKind.Active).Take(3)){Offer(r,d);Assert.IsTrue(r.SelectReward(0));}Offer(r,KaitAbilityCatalog.Get(KaitSkill.CatAgility));Assert.IsFalse(r.SelectReward(0));}
    [Test] public void TwinPunchHitsTwice()
    {var r=New();r.passives.Add(KaitPassive.TwinPunch);var e=Enemy(r,r.katePos+Vector2Int.right);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,e.hp);Assert.AreEqual(2,a.yummnPunches);}
    [Test] public void PushingOutOfRangeStopsSecondPunch()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.TwinPunch,KaitPassive.OpenHand});var e=Enemy(r,r.katePos+Vector2Int.right);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(3,e.hp);Assert.AreEqual(1,a.yummnPunches);}
    [Test] public void PushFollowAllowsSecondPunch()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.TwinPunch,KaitPassive.OpenHand,KaitPassive.FollowThrough});var e=Enemy(r,r.katePos+Vector2Int.right);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,e.hp);Assert.AreEqual(2,a.yummnPunches);}
    [Test] public void FrozenPersistsAndNextHitDealsNoDamage()
    {var r=New();var e=Enemy(r,r.katePos+Vector2Int.right);e.yummnFrozen=true;e.frozenActions=1;r.TryYummnWait();r.TryYummnWait();Assert.IsTrue(e.yummnFrozen);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(4,e.hp);Assert.IsFalse(e.yummnFrozen);Assert.IsTrue(a.yummnEvents.Any(x=>x.status=="IceGuard"));}
    [Test] public void TwinPunchThawsThenDamages()
    {var r=New();r.passives.Add(KaitPassive.TwinPunch);var e=Enemy(r,r.katePos+Vector2Int.right);e.yummnFrozen=true;r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(3,e.hp);}
    [Test] public void ShatterHurtsNeighboursNotFrozenTarget()
    {var r=New();r.passives.Add(KaitPassive.ShatteringPalm);var e=Enemy(r,r.katePos+Vector2Int.right);var n=Enemy(r,e.pos+Vector2Int.up);e.yummnFrozen=true;r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(4,e.hp);Assert.AreEqual(3,n.hp);}
    [Test] public void StunCostsOneAndExpiresOnce()
    {var r=New();r.passives.Add(KaitPassive.StunStrike);var e=Enemy(r,r.katePos+Vector2Int.right);int ki=r.Ki;r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(e.yummnStunned);Assert.AreEqual(ki-1,r.Ki);r.TryYummnWait();Assert.IsFalse(e.yummnStunned);}
    [Test] public void FreezeOnPushNeedsSuccessfulMove()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.OpenHand,KaitPassive.FreezePush});var e=Enemy(r,r.katePos+Vector2Int.right);r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(e.yummnFrozen);}
    [Test] public void DarkAndPillarAreTwoKiAndAdjacentEmptyCells()
    {var r=New();r.skills.AddRange(new[]{KaitSkill.Darkness,KaitSkill.ShapeIce});Assert.IsTrue(r.TryUseSkillAt(KaitSkill.Darkness,new Vector2Int(2,3),out _));Assert.AreEqual(4,r.Ki);r.TryYummnWait();Assert.AreEqual(new Vector2Int(2,3),r.Yummn.darkness);Assert.IsTrue(r.TryUseSkillAt(KaitSkill.ShapeIce,new Vector2Int(3,4),out _));Assert.AreEqual(2,r.Ki);}
    [Test] public void DistantPullStaysStationary()
    {var r=New();r.passives.Add(KaitPassive.DistantPull);var from=r.katePos;var e=Enemy(r,from+Vector2Int.right*2);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(from,r.katePos);Assert.AreEqual(from+Vector2Int.right,e.pos);Assert.AreEqual(3,e.hp);}
    [Test] public void PreciseStepMovesOneForOne()
    {var r=New();r.skills.Add(KaitSkill.PreciseStep);var p=r.katePos;r.TryUseSkill(KaitSkill.PreciseStep,-1,out _);Assert.IsTrue(r.TryGlobalInput(KaitDirection.Right).valid);Assert.AreEqual(p+Vector2Int.right,r.katePos);Assert.AreEqual(5,r.Ki);}
    [Test] public void FrugalMoveSuppliesTwo()
    {var r=New();r.passives.Add(KaitPassive.FrugalStride);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,a.yummnAction.movementKiCost);Assert.AreEqual(2,a.yummnAction.insertedTwos);}
    [Test] public void WaitSupplyReplacesMovingSupply()
    {var r=New(supply:YummnTileSupplyMode.EveryAction);r.passives.Add(KaitPassive.WaitSupply);Assert.AreEqual(0,r.TryGlobalInput(KaitDirection.Right).yummnAction.insertedTwos);Assert.AreEqual(2,r.TryYummnWait().yummnAction.insertedTwos);}
    [Test] public void KillSupplyTwoNotThree()
    {var r=New(supply:YummnTileSupplyMode.EveryAction);r.passives.Add(KaitPassive.KillSupply);Enemy(r,r.katePos+Vector2Int.right,1);Assert.AreEqual(2,r.TryGlobalInput(KaitDirection.Right).yummnAction.insertedTwos);}
    [Test] public void HealingWaitCostsThreeAndActsOnce()
    {var r=New();typeof(KaitRun).GetProperty("kateHp").SetValue(r,1);r.skills.Add(KaitSkill.MendWait);r.TryUseSkill(KaitSkill.MendWait,-1,out _);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(3,r.Ki);Assert.AreEqual(1,r.EnemyResolveCount);Assert.IsTrue(a.yummnAction.isWait);}
    [Test] public void ReservoirExtendsCapacityAndDoesNotFillNewPips()
    {var r=New();r.passives.Add(KaitPassive.DeepReservoir);r.TryYummnWait();Assert.AreEqual(10,r.Yummn.profile.maxKi);Assert.AreEqual(7,r.Ki);}
    [TestCase(false,false,1)] [TestCase(true,false,1)] [TestCase(false,true,2)]
    public void EchoDurability(bool first,bool all,int expectedHits)
    {
        var r=New();if(first)r.passives.Add(KaitPassive.FirstEchoWard);if(all)r.passives.Add(KaitPassive.AllEchoWard);
        var p=new Vector2Int(2,2);r.Yummn.afterimages.Add(new YummnAfterimageMarker{id=1,cell=p});
        for(int i=0;i<2;i++){var e=Enemy(r,new Vector2Int(1+i,3));e.intent.type=KaitIntentType.CrossBlast;e.intent.affectedCells.Add(p);}
        var a=r.TryYummnWait();Assert.AreEqual(expectedHits,a.yummnEvents.Count(x=>x.kind==YummnEventKind.AfterimageHit));
    }
    [Test] public void EchoReflectOnlyOnceForMultipleMarkers()
    {var r=New();r.passives.Add(KaitPassive.EchoReprisal);var p=new Vector2Int(2,2);r.Yummn.afterimages.Add(new YummnAfterimageMarker{id=1,cell=p});r.Yummn.afterimages.Add(new YummnAfterimageMarker{id=2,cell=p});var e=Enemy(r,new Vector2Int(2,3));e.intent.type=KaitIntentType.CrossBlast;e.intent.affectedCells.Add(p);r.TryYummnWait();Assert.AreEqual(3,e.hp);}
    [Test] public void DecoyIsUniqueAndAttractsNextLock()
    {var r=New();r.skills.Add(KaitSkill.UniqueDecoy);Assert.IsTrue(r.TryUseSkillAt(KaitSkill.UniqueDecoy,new Vector2Int(2,3),out _));Assert.IsTrue(r.TryUseSkillAt(KaitSkill.UniqueDecoy,new Vector2Int(3,4),out _));var e=Enemy(r,new Vector2Int(4,4));r.TryYummnWait();Assert.Contains(new Vector2Int(3,4),e.intent.affectedCells);r.TryYummnWait();Assert.AreEqual(YummnRun.NoCell,r.Yummn.decoy);}
    [Test] public void StunnedAndFrozenHaveDifferentVisuals()
    {var e=new KaitEnemy{frozenActions=1,yummnStunned=true};Assert.IsFalse(YummnRepoolArt.IsFrozenVisual(e,true));Assert.IsTrue(YummnRepoolArt.IsFrozenVisual(e,false));e.yummnFrozen=true;Assert.IsTrue(YummnRepoolArt.IsFrozenVisual(e,true));}
    [Test] public void NoMoveSupplyDoesNotSuppressStationaryDirectionSupply()
    {var r=New(supply:YummnTileSupplyMode.EveryAction);r.passives.Add(KaitPassive.WaitSupply);Enemy(r,r.katePos+Vector2Int.right);Assert.AreEqual(1,r.TryGlobalInput(KaitDirection.Right).yummnAction.insertedTwos);}
    [Test] public void CurrentReplayRoundTripWithWait()
    {var r=New();r.TryGlobalInput(KaitDirection.Right);r.TryYummnWait();var copy=new KaitRun();Assert.IsTrue(copy.RestoreReplay(r.SaveReplay()));Assert.AreEqual(r.turn,copy.turn);Assert.AreEqual(r.katePos,copy.katePos);Assert.AreEqual(r.Ki,copy.Ki);}
    [TestCase(16,"RepoolStun_A")][TestCase(17,"RepoolIceGuard_B")][TestCase(18,"RepoolHeal_A")][TestCase(19,"RepoolDecoy_B")][TestCase(20,"RepoolReflect_A")][TestCase(21,"RepoolDeflect_A")]
    public void ApprovedClipsResolve(int index,string clip)
    {Assert.AreEqual(clip,YummnV08Effect.SelectedClip(index));Assert.IsNotNull(Resources.Load<Texture2D>("KaitVisuals/Yummn/Frames/"+clip));}
}
