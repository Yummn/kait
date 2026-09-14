using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class YummnPool083Tests
{
    KaitRun New(params KaitPassive[] passives)
    {
        var r=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0,playerInvincible=true});
        r.SelectCharacter(KaitCharacter.Yummn,8141,YummnRulesSnapshot.Current());r.enemies.Clear();r.spawns.Clear();r.passives.AddRange(passives);
        Call(r,"BeginYummnRoot");Call(r,"BeginYummnRangeTracking");return r;
    }
    object Call(KaitRun r,string method,params object[] args)=>typeof(KaitRun).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(r,args);
    KaitTurnResult Result()=>new KaitTurnResult{valid=true,yummnAction=new YummnActionContext{direction=KaitDirection.Right}};
    void Place(KaitRun r,Vector2Int p)=>typeof(KaitRun).GetProperty("katePos").SetValue(r,p);
    KaitEnemy Enemy(KaitRun r,Vector2Int p,int hp=20)
    {var e=new KaitEnemy{id=100+r.enemies.Count,pos=p,hp=hp,maxHp=hp,type=KaitEnemyType.Grunt,life=KaitEnemyLife.Active,intent=new KaitIntent{origin=p}};r.enemies.Add(e);return e;}
    void Merge(KaitRun r,KaitTurnResult a,Vector2Int p,int value)
    {r.threat[p.x,p.y]=value;a.merges.Add(new KaitMergeEvent{sourceValue=value/2,resultValue=value,threatCell=p});Call(r,"ResolveYummnMergeChains",a);}
    [Test] public void PoolHas56UniqueCardsAndAllIcons()
    {Assert.AreEqual(56,YummnCatalog.Cards.Count);Assert.AreEqual(15,YummnCatalog.Cards.Count(x=>x.kind==KaitAbilityKind.Active));Assert.AreEqual(56,YummnCatalog.Cards.Select(x=>x.id).Distinct().Count());foreach(var d in YummnCatalog.Cards)Assert.IsNotNull(KaitCardSkin.Icon(d),d.id);Assert.AreEqual(39,KaitAbilityCatalog.All.Count);}
    [Test] public void ActiveCostsAndRarityChanges()
    {Assert.AreEqual(2,YummnCatalog.Get("E04").kiExtraCost);Assert.AreEqual(KaitRarity.Common,YummnCatalog.Get("E04").rarity);Assert.AreEqual(KaitRarity.Uncommon,YummnCatalog.Get("R34").rarity);Assert.AreEqual(1,YummnCatalog.Get("N19").kiExtraCost);}
    [Test] public void DoublePunchPushFollowCompletesBothPunches()
    {var r=New(KaitPassive.TwinPunch,KaitPassive.OpenHand,KaitPassive.FollowThrough);Place(r,new Vector2Int(1,2));var e=Enemy(r,new Vector2Int(2,2));var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(18,e.hp);Assert.AreEqual(2,a.yummnPunches);Assert.AreEqual(new Vector2Int(3,2),r.katePos);}
    [Test] public void SweepSpreadsToAnotherEnemyButStops()
    {var r=New(KaitPassive.SweepingPursuit);Place(r,new Vector2Int(2,2));var e=Enemy(r,new Vector2Int(3,2),1);var b=Enemy(r,new Vector2Int(3,3));var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(KaitEnemyLife.Dead,e.life);Assert.AreEqual(19,b.hp);Assert.AreEqual(2,a.yummnPunches);}
    [TestCase(YummnDamageCause.MergeGlyph)] [TestCase(YummnDamageCause.MergeMissile)] [TestCase(YummnDamageCause.Sonic)] [TestCase(YummnDamageCause.EchoReflect)]
    public void AnyDamageBreaksIceAndChainsOnce(YummnDamageCause cause)
    {var r=New(KaitPassive.ShatteringPalm);var e=Enemy(r,new Vector2Int(2,2));var b=Enemy(r,new Vector2Int(3,2));e.yummnFrozen=b.yummnFrozen=true;Call(r,"YummnHit",e,1,Vector2Int.zero,cause,Result());Assert.IsFalse(e.yummnFrozen||b.yummnFrozen);Assert.AreEqual(19,e.hp);Assert.AreEqual(20,b.hp);}
    [Test] public void ResonanceHitsEachEchoOnceAndLaterAttackCanHitAgain()
    {var r=New(KaitPassive.MirrorResonance,KaitPassive.AllEchoWard,KaitPassive.EchoReprisal);r.Yummn.ki=0;var e=Enemy(r,new Vector2Int(4,4));r.Yummn.afterimages.Add(new YummnAfterimageMarker{id=1,cell=new Vector2Int(2,2)});r.Yummn.afterimages.Add(new YummnAfterimageMarker{id=2,cell=new Vector2Int(3,2)});var intent=new KaitIntent();intent.affectedCells.Add(new Vector2Int(2,2));var a=Result();Call(r,"HitYummn082Afterimages",e,intent,1,a);Assert.AreEqual(2,r.Ki);Call(r,"HitYummn082Afterimages",e,intent,1,a);Assert.AreEqual(2,r.Ki);Assert.AreEqual(18,e.hp);Call(r,"HitYummn082Afterimages",e,intent,2,a);Assert.AreEqual(4,r.Ki);Assert.AreEqual(16,e.hp);}
    [Test] public void LastingEchoLivesTwoPhasesButCanBeConsumed()
    {var r=New(KaitPassive.LastingImage);var a=Result();Call(r,"CreateYummnSpellEcho",new Vector2Int(2,2),a);Call(r,"ClearYummn082Afterimages",a);Assert.AreEqual(1,r.Yummn.afterimages.Count);Call(r,"ClearYummn082Afterimages",a);Assert.IsEmpty(r.Yummn.afterimages);Call(r,"CreateYummnSpellEcho",new Vector2Int(2,2),a);var e=Enemy(r,new Vector2Int(4,4));var i=new KaitIntent();i.affectedCells.Add(new Vector2Int(2,2));Call(r,"HitYummn082Afterimages",e,i,1,a);Assert.IsFalse(r.Yummn.afterimages[0].alive);}
    [Test] public void MisdirectionPreservesExistingLock()
    {var r=New(KaitPassive.Misdirection);var e=Enemy(r,new Vector2Int(3,3));var oldTarget=new Vector2Int(3,4);e.intent=new KaitIntent{type=KaitIntentType.Melee,target=oldTarget};e.intent.affectedCells.Add(oldTarget);r.Yummn.afterimages.Add(new YummnAfterimageMarker{id=1,cell=new Vector2Int(2,3)});var a=Result();Call(r,"ResolveYummnCommandActor",e,a);Assert.AreEqual(oldTarget,a.enemyActions[0].to);Assert.IsTrue(r.Yummn.afterimages[0].alive);}
    [Test] public void CommandOnlyActsChosenEnemyAndNoPhase()
    {var r=New();r.skills.Add(KaitSkill.CommandAct);Place(r,new Vector2Int(3,3));var e=Enemy(r,new Vector2Int(3,4));var b=Enemy(r,new Vector2Int(2,3));Assert.IsTrue(r.TryUseSkillAt(KaitSkill.CommandAct,e.pos,out var msg),msg);Assert.AreNotEqual(KaitIntentType.None,e.intent.type);Assert.AreEqual(KaitIntentType.None,b.intent.type);Assert.AreEqual(0,r.EnemyResolveCount);Assert.AreEqual(4,r.Ki);}
    [Test] public void ArchiveHandlesAnyValueAndCreatesRealRiftAndReward()
    {var r=New(KaitPassive.OldNewsArchive);for(int x=0;x<5;x++){r.threat[x,2]=8;r.threatTwoBirth[x,2]=x+1;}var a=Result();Call(r,"ResolveYummnMergeChains",a);Assert.AreEqual(16,r.threat[0,2]);Assert.AreEqual(1,a.merges.Count);Assert.IsNotNull(r.CurrentReward);Assert.IsNotEmpty(r.spawns);}
    [Test] public void PendulumOnlyReturnsOnce()
    {var r=New(KaitPassive.GravityPendulum);r.threat[0,2]=2;r.threat[1,2]=2;var a=r.TryGlobalInput(KaitDirection.Up);Assert.IsTrue(a.valid);var b=Result();Merge(r,b,new Vector2Int(2,2),4);Call(r,"ResolveYummnPendulum",b);int count=b.threatMotions.Count;Call(r,"ResolveYummnPendulum",b);Assert.AreEqual(count,b.threatMotions.Count);}
    [Test] public void CrystalCombinesAnotherPairMatchingTheProducedValue()
    {
        var r=New(KaitPassive.ResonanceCrystal);var a=Result();
        r.threat[2,2]=r.threat[4,2]=4;
        Merge(r,a,new Vector2Int(0,2),4);
        Assert.AreEqual(4,r.threat[0,2]);Assert.AreEqual(8,r.threat[2,2]+r.threat[4,2]);
        Assert.AreEqual(2,a.merges.Count);Assert.AreEqual("Resonance",a.merges[1].mergeSource);
        Call(r,"ResolveYummnMergeChains",a);Assert.AreEqual(2,a.merges.Count);
    }
    [Test] public void CrystalNeedsTwoOtherNumbersAndCanChainToExistingEights()
    {
        var r=New(KaitPassive.ResonanceCrystal);var a=Result();r.threat[4,2]=4;
        Merge(r,a,new Vector2Int(0,2),4);Assert.AreEqual(1,a.merges.Count);Assert.AreEqual(4,r.threat[4,2]);
        r.threat[1,3]=r.threat[3,3]=8;
        Merge(r,a,new Vector2Int(2,2),4);
        Assert.AreEqual(4,r.threat[2,2]);Assert.AreEqual(4,a.merges.Count);
        Assert.AreEqual(16,r.threat[1,3]+r.threat[3,3]);
    }
    [Test] public void GlyphIsCrossDamageNotPunch()
    {var r=New(KaitPassive.WardingGlyph,KaitPassive.TwinPunch);var cell=r.MapThreatToBattle(new Vector2Int(2,2));var e=Enemy(r,cell);var b=Enemy(r,cell+Vector2Int.up);var a=Result();Merge(r,a,new Vector2Int(2,2),4);Assert.AreEqual(19,e.hp);Assert.AreEqual(19,b.hp);Assert.AreEqual(0,a.yummnPunches);}
    [Test] public void MissileTargetDoesNotChangeAfterGlyphKill()
    {var r=New(KaitPassive.MagicMissile,KaitPassive.WardingGlyph);var cell=r.MapThreatToBattle(new Vector2Int(2,2));var e=Enemy(r,cell,1);var b=Enemy(r,new Vector2Int(1,2));var a=Result();Merge(r,a,new Vector2Int(2,2),4);Assert.AreEqual(KaitEnemyLife.Dead,e.life);Assert.AreEqual(20,b.hp);Assert.IsFalse(a.yummnEvents.Any(x=>x.damageCause==YummnDamageCause.MergeMissile));}
    [Test] public void SpellEchoRepeatsEachMergeIncludingFirstWithoutChangingNumber()
    {
        var r=New(KaitPassive.MagicMissile,KaitPassive.SpellEcho,KaitPassive.ManaPearl,KaitPassive.BountyJar);
        r.Yummn.ki=1;var e=Enemy(r,new Vector2Int(3,3));var a=Result();
        Merge(r,a,new Vector2Int(0,2),8);
        Assert.AreEqual(18,e.hp);Assert.AreEqual(3,r.Ki);Assert.AreEqual(8,r.threat[0,2]);
        Assert.AreEqual(2,a.merges.Count);Assert.AreEqual("SpellEcho",a.merges[1].mergeSource);
        Call(r,"FinishYummnRoot",a);Assert.AreEqual(2,r.threat.Cast<int>().Count(v=>v==4));
        Assert.AreEqual(2,a.merges.Count);Assert.AreEqual(18,e.hp);
    }
    [Test] public void SpellEchoTriggersCrystalTwiceButDoesNotReechoItsOwnEvents()
    {
        var r=New(KaitPassive.SpellEcho,KaitPassive.ResonanceCrystal);var a=Result();
        for(int x=1;x<=4;x++)r.threat[x,2]=4;
        Merge(r,a,new Vector2Int(0,2),4);
        Assert.AreEqual(4,r.threat[0,2]);Assert.AreEqual(2,r.threat.Cast<int>().Count(v=>v==8));
        Assert.AreEqual(6,a.merges.Count);Assert.AreEqual(3,a.merges.Count(m=>m.mergeSource=="SpellEcho"));
        Assert.AreEqual(2,a.threatMotions.Count);
    }
    [Test] public void JarReturnsAfterChainsWithoutRetriggerAndNoDebt()
    {var r=New(KaitPassive.BountyJar,KaitPassive.OldNewsArchive);var a=Result();for(int x=0;x<4;x++)r.threat[x,2]=2;Merge(r,a,new Vector2Int(4,2),4);Assert.AreEqual(1,a.merges.Count);Call(r,"FinishYummnRoot",a);Assert.AreEqual(1,a.merges.Count);Assert.AreEqual(5,r.threat.Cast<int>().Count(x=>x==2));r.passives.Remove(KaitPassive.OldNewsArchive);Call(r,"BeginYummnRoot");Call(r,"FinishYummnRoot",Result());Assert.AreEqual(5,r.threat.Cast<int>().Count(x=>x==2));}
    [Test] public void MageHandOnlyDeletesAndCostsOne()
    {var r=New(KaitPassive.ManaPearl,KaitPassive.MagicMissile);r.skills.Add(KaitSkill.MageHand);r.threat[2,2]=16;var pos=r.katePos;Assert.IsTrue(r.TryUseSkillAt(KaitSkill.MageHand,new Vector2Int(2,2),out var msg),msg);Assert.AreEqual(0,r.threat[2,2]);Assert.AreEqual(5,r.Ki);Assert.AreEqual(pos,r.katePos);Assert.AreEqual(0,r.NormalTileSpawnCount);Assert.IsEmpty(r.lastSkillResult.merges);Assert.AreEqual(0,r.EnemyResolveCount);}
    [Test] public void BagDeletionDoesNotCountAsMerge()
    {var r=New(KaitPassive.BagHolding,KaitPassive.BountyJar);r.threat[0,2]=2;var a=Result();Merge(r,a,new Vector2Int(2,2),4);Assert.AreEqual(0,r.threat[0,2]);Assert.AreEqual(1,a.merges.Count);Call(r,"FinishYummnRoot",a);Assert.AreEqual(1,r.threat.Cast<int>().Count(v=>v==2));}
    [Test] public void OldReplayIsRejectedAndNewReplayRoundTrips()
    {var r=New();var json=r.SaveReplay();Assert.IsTrue(New().RestoreReplay(json));Assert.IsFalse(New().RestoreReplay(json.Replace(YummnCatalog.Version,"0.8.2-exit-stun")));}
    [Test] public void RarityDistributionMatchesGuide()
    {Assert.AreEqual(11,YummnCatalog.Cards.Count(d=>d.rarity==KaitRarity.Common));Assert.AreEqual(33,YummnCatalog.Cards.Count(d=>d.rarity==KaitRarity.Uncommon));Assert.AreEqual(12,YummnCatalog.Cards.Count(d=>d.rarity==KaitRarity.Rare));}
    [Test] public void MageHandDoesNotTriggerExistingArchive()
    {var r=New(KaitPassive.OldNewsArchive);for(int x=0;x<5;x++)r.threat[x,2]=8;r.threat[2,3]=2;r.skills.Add(KaitSkill.MageHand);Assert.IsTrue(r.TryUseSkillAt(KaitSkill.MageHand,new Vector2Int(2,3),out _));Assert.IsEmpty(r.lastSkillResult.merges);Assert.IsNull(r.CurrentReward);}
    [Test] public void BracersHitTerrainOnlyOnce()
    {var r=New(KaitPassive.StoneBracers);var e=Enemy(r,new Vector2Int(4,2));var a=Result();Call(r,"ForceYummnEnemy",e,Vector2Int.right,7,a);Assert.AreEqual(19,e.hp);Assert.AreEqual(new Vector2Int(5,2),e.pos);}
    [Test] public void MisdirectionUnlockedPreparesAgainstAdjacentEcho()
    {var r=New(KaitPassive.Misdirection);var e=Enemy(r,new Vector2Int(3,3));r.Yummn.afterimages.Add(new YummnAfterimageMarker{id=1,cell=new Vector2Int(2,3)});var a=Result();Call(r,"ResolveYummnCommandActor",e,a);Assert.IsTrue(r.Yummn.afterimages[0].alive);Assert.IsEmpty(a.enemyActions);Assert.AreEqual(new Vector2Int(2,3),e.intent.target);}
}
