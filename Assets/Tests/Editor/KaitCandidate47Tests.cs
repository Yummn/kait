using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class KaitCandidate47Tests
{
    private const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    private static KaitRun Run(int seed=919)
    {
        var run=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0,newThreatTilesPerTurn=0,playerInvincible=true});
        run.Reset(seed);
        for(int y=1;y<6;y++)for(int x=1;x<6;x++)run.walls[x,y]=false;
        Array.Clear(run.threatPillars,0,run.threatPillars.Length);
        return run;
    }
    private static object Call(KaitRun run,string name,params object[] args)=>typeof(KaitRun).GetMethod(name,Hidden).Invoke(run,args);
    private static void Set(KaitRun run,string name,object value)=>typeof(KaitRun).GetProperty(name).SetValue(run,value);
    private static T Field<T>(KaitRun run,string name)=>(T)typeof(KaitRun).GetField(name,Hidden).GetValue(run);
    private static KaitEnemy Enemy(KaitRun run,int id,int x,int y,int hp)=>new KaitEnemy{id=id,pos=new Vector2Int(x,y),hp=hp,maxHp=hp,type=KaitEnemyType.Grunt,life=KaitEnemyLife.Active};
    private static void Offer(KaitRun run,KaitAbilityDef def)
    {
        run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=8,resultValue=16,threatCell=Vector2Int.zero});
        run.CurrentReward.choices.Clear();run.CurrentReward.choices.Add(def);
    }

    [Test] public void RoundTwoPoolsHaveDocumentedShape()
    {
        Assert.AreEqual(47,KaitAbilityCatalog.CandidatePool().Count);
        Assert.AreEqual(24,KaitAbilityCatalog.DefaultPool().Count);
        Assert.AreEqual(7,KaitAbilityCatalog.DefaultPool().Count(d=>d.kind==KaitAbilityKind.Active));
        Assert.AreEqual(17,KaitAbilityCatalog.DefaultPool().Count(d=>d.kind==KaitAbilityKind.Passive));
        Assert.AreEqual(11,KaitAbilityCatalog.ExperimentalPool().Count);
        Assert.AreEqual(12,KaitAbilityCatalog.LegacyPool().Count);
        Assert.AreEqual(24,KaitAbilityCatalog.RecommendedFinalPool().Count);
        Assert.AreEqual(47,KaitAbilityCatalog.All.Select(d=>d.id).Distinct().Count());
    }

    [Test] public void NewCardsUseDedicatedArtwork()
    {
        foreach(var def in KaitAbilityCatalog.All.Skip(39))
        {
            Assert.IsNotNull(Resources.Load<Texture2D>("KaitVisuals/KaitCandidate47/"+def.id.Substring(def.id.LastIndexOf('.')+1)),def.id);
            Assert.IsNotNull(KaitCardSkin.Icon(def),def.id);
        }
    }

    [Test] public void OriginalThirtyNineCardsUseRoundTwoArtwork()
    {
        foreach(var def in KaitAbilityCatalog.All.Take(39))
        {
            Assert.IsNotNull(Resources.Load<Texture2D>("KaitVisuals/KaitRound2/"+def.id.Substring(def.id.LastIndexOf('.')+1)),def.id);
            Assert.IsNotNull(KaitCardSkin.Icon(def),def.id);
        }
    }

    [Test] public void KaitOffersOneRewardPerMergeToSixteen()
    {
        var run=Run();
        run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=4,resultValue=8});
        Assert.AreEqual(0,run.rewardQueue.Count);
        run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=8,resultValue=16});
        Assert.AreEqual(1,run.rewardQueue.Count);
        run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=16,resultValue=32});
        Assert.AreEqual(1,run.rewardQueue.Count);
        run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=8,resultValue=16});
        Assert.AreEqual(2,run.rewardQueue.Count);
    }

    [Test] public void KaitUsesSixSharedSlotsAndCanReplaceAcrossKinds()
    {
        var run=Run();
        var passives=new[]{KaitPassive.SwiftBoots,KaitPassive.ResidualSlash,KaitPassive.PiercingArrow,
            KaitPassive.CheshireCat,KaitPassive.Enfeeblement,KaitPassive.StaggeringSmite};
        foreach(var passive in passives){Offer(run,KaitAbilityCatalog.Get(passive));Assert.IsTrue(run.SelectReward(0));}
        Assert.AreEqual(6,run.EquippedCardCount);
        Offer(run,KaitAbilityCatalog.Get(KaitPassive.EldritchBlast));Assert.IsFalse(run.SelectReward(0));
        Assert.IsTrue(run.SelectReward(0,2));
        Assert.AreEqual(6,run.EquippedCardCount);Assert.Contains(KaitPassive.EldritchBlast,run.passives);
        Assert.IsFalse(run.passives.Contains(KaitPassive.PiercingArrow));
    }

    [Test] public void TelekinesisCanCreateARealMergeEveryPush()
    {
        var run=Run();run.passives.Add(KaitPassive.MomentumResonance);run.threat[0,0]=run.threat[1,0]=2;
        var result=new KaitTurnResult();Call(run,"ResolveMomentumResonance",new Vector2Int(1,1),Vector2Int.right,result);
        Assert.AreEqual(0,run.threat[0,0]);Assert.AreEqual(4,run.threat[1,0]);Assert.AreEqual(1,result.merges.Count);
        Assert.AreEqual("Telekinesis",result.merges[0].mergeSource);
    }

    [Test] public void MasterHexPropagatesBeforeMaddeningBurstConsumesNeighbourCurse()
    {
        var run=Run();run.passives.Add(KaitPassive.MasterHex);run.passives.Add(KaitPassive.MaddeningHex);
        var dead=Enemy(run,1,2,2,1);var neighbour=Enemy(run,2,2,3,3);dead.cursed=true;run.enemies.Add(dead);run.enemies.Add(neighbour);
        Call(run,"DamageEnemy",dead,1,true,new KaitTurnResult(),false);
        Assert.AreEqual(KaitEnemyLife.Dead,dead.life);Assert.AreEqual(1,neighbour.hp);Assert.IsFalse(neighbour.cursed);
        Assert.GreaterOrEqual(run.PassiveTriggerCount(KaitPassive.MasterHex),1);
        Assert.GreaterOrEqual(run.PassiveTriggerCount(KaitPassive.MaddeningHex),1);
    }

    [Test] public void ResidualSlashSpendsOnlyUnusedBaseDamage()
    {
        var run=Run();Set(run,"katePos",new Vector2Int(1,3));run.passives.Add(KaitPassive.SwiftBoots);run.passives.Add(KaitPassive.ResidualSlash);
        var first=Enemy(run,1,4,3,2);var second=Enemy(run,2,5,3,3);run.enemies.Add(first);run.enemies.Add(second);
        run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(KaitEnemyLife.Dead,first.life);Assert.AreEqual(2,second.hp);
    }

    [Test] public void SwiftBootsOnlyAddsDamageToFirstSlashOfChain()
    {
        var run=Run();Set(run,"katePos",new Vector2Int(1,3));run.passives.Add(KaitPassive.SwiftBoots);
        var first=Enemy(run,1,3,3,2);var second=Enemy(run,2,4,3,5);run.enemies.Add(first);run.enemies.Add(second);
        run.TryGlobalInput(KaitDirection.Right);var follow=run.ContinueChain(KaitDirection.Right);
        Assert.AreEqual(KaitEnemyLife.Dead,first.life);Assert.AreEqual(4,second.hp);
        Assert.IsTrue(follow.chainEndedByStrongEnemy);
    }

    [Test] public void HexBladeMarksNeighboursBeforeResidualConsumesCurse()
    {
        var run=Run();Set(run,"katePos",new Vector2Int(1,3));run.passives.Add(KaitPassive.HexBlade);run.passives.Add(KaitPassive.ResidualSlash);
        var first=Enemy(run,1,4,3,1);var second=Enemy(run,2,5,3,3);run.enemies.Add(first);run.enemies.Add(second);
        run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(KaitEnemyLife.Dead,first.life);Assert.AreEqual(1,second.hp);Assert.IsFalse(second.cursed);
    }

    [Test] public void StaggeringSmiteControlsSurvivorAfterPositiveSlashDamage()
    {
        var run=Run();Set(run,"katePos",new Vector2Int(1,3));run.passives.Add(KaitPassive.StaggeringSmite);
        var enemy=Enemy(run,1,3,3,4);run.enemies.Add(enemy);run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(3,enemy.hp);Assert.GreaterOrEqual(run.PassiveTriggerCount(KaitPassive.StaggeringSmite),1);
    }

    [Test] public void CatAgilityDoublesLockAndRemovesReverseTurn()
    {
        var run=Run();Set(run,"katePos",new Vector2Int(1,3));run.passives.Add(KaitPassive.CatAgility);
        var enemy=Enemy(run,1,4,3,3);run.enemies.Add(enemy);var result=run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(4,result.chainPower);Assert.IsTrue(run.chainActive);
        CollectionAssert.DoesNotContain(run.AllowedTurnDirections(),KaitDirection.Left);
    }

    [Test] public void EldritchBlastAutomaticallyFiresAfterPrimarySlash()
    {
        var run=Run();Set(run,"katePos",new Vector2Int(1,3));run.passives.Add(KaitPassive.EldritchBlast);
        var enemy=Enemy(run,1,3,3,5);run.enemies.Add(enemy);run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(2,run.PassiveCooldown(KaitPassive.EldritchBlast));
        Assert.AreEqual(3,enemy.hp);Assert.GreaterOrEqual(run.PassiveTriggerCount(KaitPassive.EldritchBlast),1);
    }

    [Test] public void DynamicPrerequisitesRemoveDeadOffers()
    {
        var run=Run();var initial=run.EligibleAbilities();
        Assert.IsFalse(initial.Exists(d=>d.passive==KaitPassive.MasterHex));
        Assert.IsFalse(initial.Exists(d=>d.passive==KaitPassive.TwinSigil));
        Assert.IsFalse(initial.Exists(d=>d.passive==KaitPassive.Devil));
        run.passives.Add(KaitPassive.HexBlade);run.passives.Add(KaitPassive.EldritchBlast);
        var unlocked=run.EligibleAbilities();
        Assert.IsTrue(unlocked.Exists(d=>d.passive==KaitPassive.MasterHex));
        Assert.IsTrue(unlocked.Exists(d=>d.passive==KaitPassive.TwinSigil));
        Assert.IsTrue(unlocked.Exists(d=>d.passive==KaitPassive.Devil));
    }

    [Test] public void HexArmorConsumesCurseWithoutTriggeringMaddeningBurst()
    {
        var run=Run();run.passives.Add(KaitPassive.HexArmor);run.passives.Add(KaitPassive.MaddeningHex);
        var attacker=Enemy(run,1,2,2,3);var neighbour=Enemy(run,2,2,3,3);attacker.cursed=true;
        run.enemies.Add(attacker);run.enemies.Add(neighbour);
        var intent=new KaitIntent{type=KaitIntentType.Melee,origin=attacker.pos,target=new Vector2Int(3,2),damage=1};
        Assert.IsTrue((bool)Call(run,"CancelEnemyAttack",attacker,intent,new KaitTurnResult()));
        Assert.IsFalse(attacker.cursed);Assert.AreEqual(3,neighbour.hp);
        Assert.AreEqual(0,run.PassiveTriggerCount(KaitPassive.MaddeningHex));
    }

    [Test] public void BladeCovenantReducesActiveAndPassiveCooldowns()
    {
        var run=Run();run.passives.Add(KaitPassive.BladeCovenant);
        run.skills.Add(KaitSkill.HexCurse);run.skills.Add(KaitSkill.IceTomb);
        var cooldowns=Field<System.Collections.Generic.Dictionary<KaitSkill,int>>(run,"skillCooldowns");
        var passiveCooldowns=Field<System.Collections.Generic.Dictionary<KaitPassive,int>>(run,"passiveCooldowns");
        cooldowns[KaitSkill.HexCurse]=2;cooldowns[KaitSkill.IceTomb]=3;
        run.passives.Add(KaitPassive.EldritchBlast);passiveCooldowns[KaitPassive.EldritchBlast]=2;
        Call(run,"ResolveBladeCovenant",new KaitTurnResult());
        Assert.AreEqual(1,run.SkillCooldown(KaitSkill.HexCurse));Assert.AreEqual(2,run.SkillCooldown(KaitSkill.IceTomb));
        Assert.AreEqual(1,run.PassiveCooldown(KaitPassive.EldritchBlast));
    }

    [Test] public void PactKeeperMergeReducesFirstLongestCooldown()
    {
        var run=Run();run.passives.Add(KaitPassive.Devil);
        run.skills.Add(KaitSkill.HexCurse);run.skills.Add(KaitSkill.IceTomb);
        var cooldowns=Field<System.Collections.Generic.Dictionary<KaitSkill,int>>(run,"skillCooldowns");
        cooldowns[KaitSkill.HexCurse]=3;cooldowns[KaitSkill.IceTomb]=3;
        Call(run,"HandleMilestoneMergeWithResult",new KaitMergeEvent{sourceValue=2,resultValue=4,systemMerge=true},new KaitTurnResult());
        Assert.AreEqual(2,run.SkillCooldown(KaitSkill.HexCurse));Assert.AreEqual(3,run.SkillCooldown(KaitSkill.IceTomb));
    }

    [Test] public void TwinSigilRepeatsAutomaticEldritchBlastWithoutSecondCooldown()
    {
        var run=Run();Set(run,"katePos",new Vector2Int(1,3));run.passives.Add(KaitPassive.EldritchBlast);run.passives.Add(KaitPassive.TwinSigil);
        var enemy=Enemy(run,1,3,3,6);run.enemies.Add(enemy);run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(2,enemy.hp);Assert.AreEqual(2,run.PassiveCooldown(KaitPassive.EldritchBlast));
        Assert.AreEqual(1,run.PassiveTriggerCount(KaitPassive.TwinSigil));
    }

    [Test] public void PiercingArrowPassesEnemiesButStopsAtKait()
    {
        var run=Run();Set(run,"katePos",new Vector2Int(5,3));var blocker=Enemy(run,1,2,3,3);run.enemies.Add(blocker);
        var normal=(KaitIntent)Call(run,"BuildLineIntent",new Vector2Int(1,3),Vector2Int.right,5,true);
        Assert.AreEqual(1,normal.affectedCells.Count);
        run.passives.Add(KaitPassive.PiercingArrow);
        var piercing=(KaitIntent)Call(run,"BuildLineIntent",new Vector2Int(1,3),Vector2Int.right,5,true);
        Assert.AreEqual(new Vector2Int(5,3),piercing.affectedCells.Last());
    }

    [Test] public void ShieldFrontBlocksSpellDamageAndBlastPush()
    {
        var run=Run();Set(run,"katePos",new Vector2Int(1,3));run.passives.Add(KaitPassive.EldritchBlast);
        var boss=Enemy(run,1,3,3,8);boss.type=KaitEnemyType.ShieldKnight;boss.facing=Vector2Int.left;run.enemies.Add(boss);
        run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(8,boss.hp);
    }

    [Test] public void ReplayUsesNewRulesAndPoolVersions()
    {
        var run=Run();string replay=run.SaveReplay();
        StringAssert.Contains(KaitAbilityCatalog.RulesVersion,replay);StringAssert.Contains(KaitAbilityCatalog.Version,replay);
    }
}
