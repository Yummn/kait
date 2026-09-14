using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class KaitCards096Tests
{
    object Call(KaitRun run,string name,params object[] args)=>typeof(KaitRun).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(run,args);
    [TestCase(false,false,2)] [TestCase(false,true,2)] [TestCase(true,false,2)] [TestCase(true,true,2)]
    [TestCase(false,true,0)] [TestCase(true,true,0)]
    public void ReactionsPayOneKiAndKickWithoutPunchLinks(bool exit,bool exhausted,int ki)
    {
        var run=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0});run.SelectCharacter(KaitCharacter.Yummn,9601,YummnRulesSnapshot.Current());
        run.enemies.Clear();run.spawns.Clear();typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(2,2));
        run.passives.AddRange(new[]{exit?KaitPassive.OpportunityAttack:KaitPassive.Opportunist,KaitPassive.TwinPunch,KaitPassive.OpenHand});
        run.Yummn.phase=exhausted?YummnPhase.Exhausted:YummnPhase.Burst;run.Yummn.ki=ki;run.Yummn.kiTenths=0;
        Call(run,"BeginYummnRoot");Call(run,"BeginYummnRangeTracking");
        var e=new KaitEnemy{id=1,type=KaitEnemyType.Grunt,pos=new Vector2Int(exit?4:3,2),hp=10,maxHp=10,life=KaitEnemyLife.Active};run.enemies.Add(e);
        var r=new KaitTurnResult{valid=true,yummnAction=new YummnActionContext{phaseAtStart=run.Yummn.phase}};
        if(exit)Call(run,"ResolveYummnRangeExit",e,new Vector2Int(3,2),r,null);else Call(run,"ResolveYummnRangeEntries",r);
        Assert.AreEqual(ki>0?9:10,e.hp);Assert.AreEqual(ki>0?ki-1:0,run.Ki);
        Assert.AreEqual(ki>0?1:0,r.yummnEvents.Count(x=>x.kind==YummnEventKind.Hit&&x.damageCause==YummnDamageCause.Kick));
        Assert.IsFalse(r.yummnEvents.Any(x=>x.kind==YummnEventKind.Hit&&x.damageCause==YummnDamageCause.Punch));Assert.AreEqual(new Vector2Int(exit?4:3,2),e.pos);
    }
    [Test] public void LibraryListsBothCompletePoolsExactlyOnceByRarity()
    {
        foreach(var c in new[]{KaitCharacter.Kait,KaitCharacter.Yummn})
        {
            var all=KaitCardLibrary.Cards(c,-1);Assert.AreEqual(c==KaitCharacter.Kait?39:56,all.Count);Assert.AreEqual(all.Count,all.Select(d=>d.id).Distinct().Count());
            Assert.AreEqual(all.Count,Enumerable.Range(0,3).Sum(r=>KaitCardLibrary.Cards(c,r).Count));
            for(int i=1;i<all.Count;i++)Assert.LessOrEqual((int)all[i-1].rarity,(int)all[i].rarity);
            Assert.IsTrue(all.All(d=>!string.IsNullOrWhiteSpace(d.cardText)));
        }
    }
}
