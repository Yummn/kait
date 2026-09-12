using NUnit.Framework;
using UnityEngine;
public class YummnKiGuardTests
{
    KaitRun Setup(bool guard=true)
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,123,YummnRulesSnapshot.Current(kiGuard:guard));
        r.enemies.Clear();r.spawns.Clear();r.config.playerInvincible=false;return r;
    }
    void AddAttack(KaitRun r,int id)
    {
        var p=r.katePos+Vector2Int.right;
        var i=new KaitIntent{type=KaitIntentType.Melee,origin=p,target=r.katePos,damage=1,direction=Vector2Int.left};i.affectedCells.Add(r.katePos);
        r.enemies.Add(new KaitEnemy{id=id,pos=p,type=KaitEnemyType.Grunt,hp=2,maxHp=2,life=KaitEnemyLife.Active,intent=i});
    }
    [Test] public void GuardBlocksFirstButNotSecondAttackInSamePhase()
    {
        var r=Setup();AddAttack(r,1);AddAttack(r,2);int hp=r.kateHp;
        var result=r.TryYummnWait();Assert.AreEqual(hp-1,r.kateHp);Assert.AreEqual(0,r.Ki);
        Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);
        Assert.AreEqual(1,result.yummnEvents.FindAll(e=>e.status=="KiGuard").Count);
    }
    [TestCase(false,false)] [TestCase(true,true)]
    public void DisabledOrExhaustedDoesNotGuard(bool enabled,bool exhausted)
    {
        var r=Setup(enabled);if(exhausted){r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=2;}
        AddAttack(r,1);int hp=r.kateHp;var result=r.TryYummnWait();
        Assert.AreEqual(hp-1,r.kateHp);Assert.False(result.yummnAction.kiGuardTriggered);
    }
    [Test] public void ExistingDefenseDoesNotSpendKiGuard()
    {
        var r=Setup();r.Yummn.defense=true;AddAttack(r,1);int hp=r.kateHp,ki=r.Ki;
        var result=r.TryYummnWait();Assert.AreEqual(hp,r.kateHp);Assert.AreEqual(ki,r.Ki);Assert.False(result.yummnAction.kiGuardTriggered);
    }
    [Test] public void RuleSerializesAndLegacyRemainsOff()
    {
        var rules=YummnRulesSnapshot.Current(kiGuard:true);
        Assert.True(JsonUtility.FromJson<YummnRulesSnapshot>(JsonUtility.ToJson(rules)).KiGuard);
        Assert.False(new YummnRulesSnapshot().KiGuard);
    }
}
