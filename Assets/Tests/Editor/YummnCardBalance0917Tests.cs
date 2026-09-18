using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class YummnCardBalance0917Tests
{
    static KaitRun Run()
    {
        var run=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0,playerInvincible=false});
        run.SelectCharacter(KaitCharacter.Yummn,917,YummnRulesSnapshot.Current());
        run.enemies.Clear();run.spawns.Clear();Array.Clear(run.threat,0,run.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(1,3));
        return run;
    }

    static KaitEnemy Enemy(KaitRun run,Vector2Int cell,int hp=3,KaitEnemyType type=KaitEnemyType.Grunt)
    {
        var enemy=new KaitEnemy{id=100+run.enemies.Count,pos=cell,hp=hp,maxHp=hp,type=type,life=KaitEnemyLife.Active,intent=new KaitIntent{origin=cell}};
        run.enemies.Add(enemy);return enemy;
    }

    static void Card(string id,KaitRarity rarity,int cost,string text)
    {
        var card=YummnCatalog.Get(id);Assert.NotNull(card,id);Assert.AreEqual(rarity,card.rarity,id);
        if(card.kind==KaitAbilityKind.Active)Assert.AreEqual(cost,card.kiExtraCost,id);
        StringAssert.Contains(text,card.cardText,id);
    }

    [Test] public void RequestedCostsRaritiesAndTextsAreApplied()
    {
        Card("E04",KaitRarity.Common,2,"前方4格");Card("N02",KaitRarity.Common,1,"四邻");
        Card("S01",KaitRarity.Common,1,"任一空暗影格");Card("E03",KaitRarity.Common,2,"永久冰柱");
        Card("N05",KaitRarity.Common,1,"残影");Card("N06",KaitRarity.Common,1,"立即行动");
        Card("R30",KaitRarity.Uncommon,1,"推至尽头");Card("S02",KaitRarity.Common,2,"永久黑雾");
        Card("M04",KaitRarity.Common,0,"消耗1气");Card("R24",KaitRarity.Uncommon,0,"等待补两个2");
        Card("R28",KaitRarity.Uncommon,0,"气上限+3");Card("R31",KaitRarity.Common,0,"穿过柱子");
        Card("O05",KaitRarity.Uncommon,0,"恢复1点生命");Card("R19",KaitRarity.Uncommon,0,"所有攻击");
        Card("R37",KaitRarity.Rare,0,"消耗3气抵挡");Card("R23",KaitRarity.Uncommon,0,"击杀补两个2");
        Assert.AreEqual(17,YummnCatalog.Cards.Count(d=>d.rarity==KaitRarity.Common));
        Assert.AreEqual(30,YummnCatalog.Cards.Count(d=>d.rarity==KaitRarity.Uncommon));
        Assert.AreEqual(9,YummnCatalog.Cards.Count(d=>d.rarity==KaitRarity.Rare));
    }

    [Test] public void FrostBreathHitsAndFreezesFourthCell()
    {
        var run=Run();run.skills.Add(KaitSkill.FrostBreath);
        var near=Enemy(run,new Vector2Int(2,3));var far=Enemy(run,new Vector2Int(5,3));
        Assert.IsTrue(run.TryUseSkillAt(KaitSkill.FrostBreath,new Vector2Int(2,3),out var message),message);
        Assert.AreEqual(2,near.hp);Assert.AreEqual(2,far.hp);Assert.IsTrue(near.yummnFrozen);Assert.IsTrue(far.yummnFrozen);
    }

    [Test] public void ShadowStepTargetsAnySelectedEmptyShadowCell()
    {
        var run=Run();run.skills.Add(KaitSkill.YummnShadowStep);
        Vector2Int target=YummnRun.NoCell;
        for(int y=1;y<=5&&target.x<0;y++)for(int x=1;x<=5;x++)
        {
            var cell=new Vector2Int(x,y);
            if((cell-run.katePos).sqrMagnitude>1&&run.IsYummnShadow(cell)&&run.IsLegalSkillCell(KaitSkill.YummnShadowStep,cell)){target=cell;break;}
        }
        Assert.AreNotEqual(YummnRun.NoCell,target,"map must expose a non-adjacent shadow cell");
        Assert.IsTrue(run.TryUseSkillAt(KaitSkill.YummnShadowStep,target,out var message),message);Assert.AreEqual(target,run.katePos);
    }

    [Test] public void StillnessWaitSuppliesTwoAndReservoirDoesNotReduceKillKi()
    {
        var run=Run();run.passives.Add(KaitPassive.WaitSupply);
        Assert.AreEqual(2,run.TryYummnWait().yummnAction.insertedTwos);
        run=Run();run.passives.AddRange(new[]{KaitPassive.DeepReservoir,KaitPassive.KiAegis});
        run.TryYummnWait();Assert.AreEqual(10,run.Yummn.profile.maxKi);
        var gain=(int)typeof(KaitRun).GetProperty("YummnKillGain",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(run);
        Assert.AreEqual(run.Yummn.rules.KillKi,gain);
    }

    [Test] public void SpiritGuardWorksWhileExhaustedAndSpendsThreeKi()
    {
        var run=Run();run.passives.Add(KaitPassive.KiAegis);run.Yummn.phase=YummnPhase.Exhausted;run.Yummn.ki=3;
        var enemy=Enemy(run,new Vector2Int(2,3),3,KaitEnemyType.Swordsman);
        enemy.intent=new KaitIntent{type=KaitIntentType.Melee,origin=enemy.pos,target=run.katePos,damage=1};enemy.intent.affectedCells.Add(run.katePos);
        int hp=run.kateHp;var result=run.TryYummnWait();
        Assert.AreEqual(hp,run.kateHp);Assert.IsTrue(result.yummnGuard);Assert.GreaterOrEqual(result.yummnAction.kiBefore-run.Ki,2);
    }

    [Test] public void PreviousCardPoolReplayRemainsLoadable()
    {
        var run=Run();run.TryYummnWait();
        string old=run.SaveReplay().Replace(YummnCatalog.Version,YummnCatalog.PreviousVersion)
            .Replace(YummnRulesSnapshot.CurrentVersion,YummnRulesSnapshot.PreviousVersion);
        Assert.IsTrue(new KaitRun().RestoreReplay(old));
    }
}
