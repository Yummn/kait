using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class YummnExitStunTests
{
    KaitRun New()
    {var r=new KaitRun(new KaitBalanceConfig{initialThreatTiles=0,playerInvincible=true});r.SelectCharacter(KaitCharacter.Yummn,1723,YummnRulesSnapshot.Current());r.enemies.Clear();r.spawns.Clear();return r;}
    KaitEnemy Enemy(KaitRun r,int hp=20)
    {var p=r.katePos+Vector2Int.right;var e=new KaitEnemy{id=123,pos=p,hp=hp,maxHp=hp,type=KaitEnemyType.Grunt,life=KaitEnemyLife.Active,intent=new KaitIntent{origin=p}};r.enemies.Add(e);return e;}
    object Call(KaitRun r,string method,params object[] args)=>typeof(KaitRun).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(r,args);
    KaitTurnResult Result()=>new KaitTurnResult{valid=true,yummnAction=new YummnActionContext{direction=KaitDirection.Right}};
    [Test] public void IndependentUncommonPassiveHasArt()
    {var d=YummnCatalog.Get("R40");Assert.AreEqual("借机攻击",d.nameZh);Assert.AreEqual(KaitRarity.Uncommon,d.rarity);Assert.AreEqual(KaitAbilityKind.Passive,d.kind);Assert.AreNotEqual(YummnCatalog.Get("S06").passive,d.passive);Assert.IsNotNull(KaitCardSkin.Icon(d));}
    [Test] public void PushOutTriggersOnePunchBeforeMove()
    {var r=New();r.passives.Add(KaitPassive.OpportunityAttack);var e=Enemy(r);var a=Result();Call(r,"BeginYummnRangeTracking");Call(r,"ForceYummnEnemy",e,Vector2Int.right,1,a);Assert.AreEqual(19,e.hp);Assert.AreEqual(1,a.yummnPunches);Assert.AreEqual(r.katePos+Vector2Int.right*2,e.pos);Assert.Less(a.yummnEvents.FindIndex(x=>x.kind==YummnEventKind.Hit),a.yummnEvents.FindIndex(x=>x.kind==YummnEventKind.Move&&x.targetId==e.id));}
    [Test] public void DepartureKillCancelsEnemyMove()
    {var r=New();r.passives.Add(KaitPassive.OpportunityAttack);var e=Enemy(r,1);var a=Result();Call(r,"BeginYummnRangeTracking");Call(r,"ForceYummnEnemy",e,Vector2Int.right,1,a);Assert.AreEqual(KaitEnemyLife.Dead,e.life);Assert.IsFalse(a.yummnEvents.Any(x=>x.kind==YummnEventKind.Move&&x.targetId==e.id));}
    [Test] public void PlayerLeavingDoesNotTrigger()
    {var r=New();r.passives.Add(KaitPassive.OpportunityAttack);var e=Enemy(r);r.TryGlobalInput(KaitDirection.Left);Assert.AreEqual(20,e.hp);}
    [Test] public void DepartureUsesDoublePunchAndCannotRecurse()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.OpportunityAttack,KaitPassive.TwinPunch});var e=Enemy(r);var a=Result();Call(r,"BeginYummnRangeTracking");Call(r,"ForceYummnEnemy",e,Vector2Int.right,1,a);Assert.AreEqual(18,e.hp);Assert.AreEqual(4,r.Ki);Assert.AreEqual(2,a.yummnPunches);}
    [Test] public void DoublePunchOnlyPaysStunOnce()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.StunStrike,KaitPassive.TwinPunch});var e=Enemy(r);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(3,r.Ki);Assert.AreEqual(1,a.yummnEvents.Count(x=>x.status=="Stunned"));Assert.IsTrue(e.yummnStunned);}
    [Test] public void StunPersistsAcrossPlayerActionsAndExpiresAtEnemyRoundEnd()
    {var r=New();r.passives.Add(KaitPassive.StunStrike);var e=Enemy(r);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(5,r.Ki);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(5,r.Ki);Assert.IsTrue(e.yummnStunned);var a=r.TryYummnWait();Assert.IsFalse(e.yummnStunned);Assert.AreEqual(KaitIntentType.None,e.intent.type);Assert.AreEqual(1,a.yummnEvents.Count(x=>x.status=="ControlConsumed"));r.TryYummnWait();Assert.AreNotEqual(KaitIntentType.None,e.intent.type);}
    [Test] public void PushAndEntryCannotChargeExistingStunAgain()
    {var r=New();r.passives.AddRange(new[]{KaitPassive.StunStrike,KaitPassive.OpenHand,KaitPassive.OpportunityAttack});var e=Enemy(r);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(5,r.Ki);Assert.AreEqual(1,a.yummnEvents.Count(x=>x.status=="Stunned"));Assert.AreEqual(2,a.yummnPunches);Assert.AreEqual(18,e.hp);}
}
