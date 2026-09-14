using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class YummnPalm097Tests
{
    void Call(KaitRun run,string name,params object[] args)=>typeof(KaitRun).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(run,args);
    [TestCase(false,false)] [TestCase(false,true)] [TestCase(true,false)] [TestCase(true,true)]
    public void PunchAndKickShareDirectionalMarks(bool firstKick,bool secondKick)
    {
        var run=Run(out var e);
        Hit(run,e,firstKick,Vector2Int.right);
        Assert.IsTrue(run.Yummn.TryPalm(e.id,out var d));Assert.AreEqual(Vector2Int.right,d);
        Hit(run,e,secondKick,Vector2Int.right);
        Assert.AreEqual(18,e.hp);Assert.IsTrue(run.Yummn.TryPalm(e.id,out d));
        var result=Hit(run,e,secondKick,Vector2Int.up);
        Assert.AreEqual(15,e.hp);Assert.IsFalse(run.Yummn.TryPalm(e.id,out d));
        Assert.AreEqual(1,result.yummnEvents.Count(x=>x.status=="PalmDetonate"));
        Assert.AreEqual(2,result.yummnEvents.Where(x=>x.kind==YummnEventKind.Hit&&x.damageCause==YummnDamageCause.Quivering).Sum(x=>x.amount));
    }
    [Test] public void IceGuardKickDoesNotPlaceOrDetonateMark()
    {
        var run=Run(out var e);e.yummnFrozen=true;Hit(run,e,true,Vector2Int.right);
        Assert.AreEqual(20,e.hp);Assert.IsFalse(run.Yummn.TryPalm(e.id,out _));
        run.Yummn.MarkPalm(e.id,Vector2Int.up);e.yummnFrozen=true;
        Hit(run,e,true,Vector2Int.right);Assert.AreEqual(20,e.hp);
        Assert.IsTrue(run.Yummn.TryPalm(e.id,out var d));Assert.AreEqual(Vector2Int.up,d);
    }
    KaitRun Run(out KaitEnemy e)
    {
        var run=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0});run.SelectCharacter(KaitCharacter.Yummn,9701,YummnRulesSnapshot.Current());
        run.enemies.Clear();run.spawns.Clear();run.passives.Add(KaitPassive.QuiveringPalm);
        run.Yummn.phase=YummnPhase.Exhausted;run.Yummn.ki=5;
        e=new KaitEnemy{id=1,type=KaitEnemyType.Grunt,pos=new Vector2Int(3,2),hp=20,maxHp=20,life=KaitEnemyLife.Active};run.enemies.Add(e);return run;
    }
    KaitTurnResult Hit(KaitRun run,KaitEnemy e,bool kick,Vector2Int d)
    {
        typeof(KaitRun).GetProperty("katePos").SetValue(run,e.pos-d);Call(run,"BeginYummnRoot");
        var result=new KaitTurnResult{valid=true,yummnAction=new YummnActionContext{phaseAtStart=run.Yummn.phase,direction=d==Vector2Int.right?KaitDirection.Right:KaitDirection.Up}};
        if(kick)Call(run,"ResolveYummnKick",e,d,result,true);else Call(run,"ResolveYummnPunch",e,result,false);
        return result;
    }
}
