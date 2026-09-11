using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class YummnBossLineTests
{
    const BindingFlags Hidden=BindingFlags.NonPublic|BindingFlags.Instance;
    static void Pos(KaitRun r,int x,int y)=>typeof(KaitRun).GetProperty("katePos").SetValue(r,new Vector2Int(x,y));
    static KaitRun R(bool line=true)
    {var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,913,new YummnRulesSnapshot(bossLine:line));r.enemies.Clear();r.spawns.Clear();Array.Clear(r.threat,0,r.threat.Length);Pos(r,4,3);return r;}
    static KaitEnemy Boss(KaitRun r,int x=1,int y=3)
    {var b=new KaitEnemy{id=9900,type=KaitEnemyType.ShieldKnight,pos=new Vector2Int(x,y),hp=8,maxHp=8,life=KaitEnemyLife.Active,facing=Vector2Int.down};r.enemies.Add(b);return b;}
    static KaitTurnResult Phase(KaitRun r)
    {var a=new KaitTurnResult{yummnAction=new YummnActionContext{phaseAtStart=YummnPhase.Exhausted,startCell=r.katePos}};typeof(KaitRun).GetMethod("ResolveYummnEnemyPhase",Hidden).Invoke(r,new object[]{a});return a;}
    [TestCase(5,3,1,0)] [TestCase(1,3,-1,0)] [TestCase(3,5,0,1)] [TestCase(3,1,0,-1)]
    public void DistantTargetTurnsShieldAndWarnsForwardRow(int x,int y,int dx,int dy)
    {
        var r=R();Pos(r,x,y);var b=Boss(r,3,3);var a=Phase(r);
        Assert.AreEqual(new Vector2Int(dx,dy),b.facing);Assert.AreEqual(b.facing,b.intent.direction);Assert.AreEqual(KaitRangedState.Aim,b.rangedState);
        Assert.AreEqual(KaitIntentType.Melee,b.intent.type);Assert.AreEqual(2,b.intent.affectedCells.Count);Assert.Contains(r.katePos,b.intent.affectedCells);Assert.IsEmpty(a.enemyActions);Assert.AreEqual(3,r.kateHp);
    }
    [Test] public void OffAxisTargetStillLocksAndAttackDoesNotChaseNewPosition()
    {
        var r=R();Pos(r,5,4);var b=Boss(r);Phase(r);Assert.AreEqual(Vector2Int.right,b.facing);Assert.AreEqual(4,b.intent.affectedCells.Count);
        Pos(r,1,1);var a=Phase(r);Assert.AreEqual(Vector2Int.right,b.facing);Assert.AreEqual(3,r.kateHp);Assert.AreEqual(1,a.enemyActions.Count);Assert.AreEqual(KaitIntentType.None,b.intent.type);
        Phase(r);Assert.AreEqual(Vector2Int.down,b.facing);Assert.AreEqual(KaitRangedState.Aim,b.rangedState);
    }
    [Test] public void WarningIncludesCellsBeyondPlayerAndDamageHappensOnlyOnNextPhase()
    {
        var r=R();Pos(r,3,3);var b=Boss(r);Phase(r);CollectionAssert.AreEqual(new[]{new Vector2Int(2,3),new Vector2Int(3,3),new Vector2Int(4,3),new Vector2Int(5,3)},b.intent.affectedCells);
        var a=Phase(r);Assert.AreEqual(2,r.kateHp);Assert.IsTrue(a.enemyActions.Single().hitKate);Assert.AreEqual(1,a.playerDamage);Assert.AreEqual(KaitRangedState.Ready,b.rangedState);
        Phase(r);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(KaitRangedState.Aim,b.rangedState);
    }
    [TestCase("wall")] [TestCase("ice")] [TestCase("dark")]
    public void ObstacleStopsLineAndNewObstacleBlocksPreviouslyLockedAttack(string kind)
    {
        var r=R();var b=Boss(r);Phase(r);
        if(kind=="wall")r.walls[3,3]=true;else if(kind=="ice")r.Yummn.icePillar=new Vector2Int(3,3);else r.Yummn.darkness=new Vector2Int(3,3);
        var a=Phase(r);CollectionAssert.AreEqual(new[]{new Vector2Int(2,3)},a.enemyActions.Single().affectedCells);Assert.AreEqual(3,r.kateHp);
    }
    [Test] public void OtherEnemyDoesNotStopSweepOrTakeFriendlyDamage()
    {
        var r=R();var b=Boss(r);var friend=new KaitEnemy{id=9999,pos=new Vector2Int(2,3),hp=2,maxHp=2,life=KaitEnemyLife.Active,type=KaitEnemyType.Guard};r.enemies.Add(friend);
        Phase(r);Phase(r);Assert.AreEqual(2,r.kateHp);Assert.AreEqual(2,friend.hp);
    }
    [Test] public void DangerWarningUsesLockedDirectionAndCurrentObstaclesWithoutMutating()
    {
        var r=R();var b=Boss(r);Assert.IsFalse(r.IsKateInImminentDanger());Phase(r);Assert.IsTrue(r.IsKateInImminentDanger());
        var locked=b.intent;r.Yummn.icePillar=new Vector2Int(3,3);Assert.IsFalse(r.IsKateInImminentDanger());Assert.AreSame(locked,b.intent);Assert.AreEqual(Vector2Int.right,b.facing);
        r.Yummn.icePillar=YummnRun.NoCell;Pos(r,4,4);Assert.IsFalse(r.IsKateInImminentDanger());
    }
    [Test] public void CloakAndFreezeStillDelayPreparationAndTurning()
    {
        var r=R();var b=Boss(r);r.Yummn.cloak=true;Phase(r);Assert.AreEqual(Vector2Int.down,b.facing);Assert.AreEqual(KaitIntentType.None,b.intent.type);
        b.yummnFrozen=true;Phase(r);Assert.AreEqual(Vector2Int.down,b.facing);Assert.AreEqual(KaitIntentType.None,b.intent.type);Phase(r);Assert.AreEqual(Vector2Int.right,b.facing);
    }
    [Test] public void LockedShieldFrontStillBlocksButSideDoesNot()
    {
        var r=R();var b=Boss(r);Phase(r);Pos(r,2,3);var a=r.TryGlobalInput(KaitDirection.Left);Assert.IsTrue(a.playerAttackBlocked);Assert.AreEqual(8,b.hp);
        Pos(r,1,2);r.TryGlobalInput(KaitDirection.Up);Assert.AreEqual(7,b.hp);
    }
    [Test] public void ExistingSaveAndKaitKeepTheirOwnBossRules()
    {
        var old=R(false);var boss=Boss(old);Phase(old);Assert.AreEqual(KaitIntentType.None,boss.intent.type);Assert.AreEqual(Vector2Int.down,boss.facing);
        var serialized=JsonUtility.ToJson(new YummnRulesSnapshot(bossLine:false)).Replace(",\"bossLine\":false","");Assert.IsFalse(JsonUtility.FromJson<YummnRulesSnapshot>(serialized).BossLine);
        var kait=new KaitRun();kait.SelectCharacter(KaitCharacter.Kait,913);var kb=Boss(kait);Pos(kait,4,3);
        var intent=(KaitIntent)typeof(KaitRun).GetMethod("BuildIntentToward",Hidden).Invoke(kait,new object[]{kb,kait.katePos});Assert.AreEqual(KaitIntentType.None,intent.type);
        Pos(kait,2,3);intent=(KaitIntent)typeof(KaitRun).GetMethod("BuildIntentToward",Hidden).Invoke(kait,new object[]{kb,kait.katePos});Assert.AreEqual(KaitIntentType.Melee,intent.type);Assert.AreEqual(1,intent.affectedCells.Count);
    }
    [TestCase(false)] [TestCase(true)] public void SnapshotReplayKeepsBossMode(bool line)
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,913,new YummnRulesSnapshot(bossLine:line));for(int i=0;i<10&&!r.ended;i++)r.TryGlobalInput((KaitDirection)(i%4));
        var copy=new KaitRun();Assert.IsTrue(copy.RestoreReplay(r.SaveReplay()));Assert.AreEqual(line,copy.Yummn.rules.BossLine);Assert.AreEqual(r.SaveReplay(),copy.SaveReplay());
    }
    [Test] public void AllForwardWarningCellsTintButBlockedOriginDoesNot()
    {
        var root=new GameObject("Boss warning test");root.SetActive(false);
        try
        {
            var game=root.AddComponent<KaitGame>();var r=(KaitRun)typeof(KaitGame).GetField("run",Hidden).GetValue(game);
            r.SelectCharacter(KaitCharacter.Yummn,913,new YummnRulesSnapshot());r.enemies.Clear();var b=Boss(r);Pos(r,3,3);Phase(r);
            var tint=typeof(KaitGame).GetMethod("IntentTintAt",Hidden);
            foreach(var p in b.intent.affectedCells)Assert.IsNotNull(tint.Invoke(game,new object[]{p}));
            r.walls[2,3]=true;b.intent=new KaitIntent{origin=b.pos};Phase(r);Assert.IsEmpty(b.intent.affectedCells);Assert.IsNull(tint.Invoke(game,new object[]{b.pos}));
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
}
