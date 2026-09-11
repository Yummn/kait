using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class YummnV082Tests
{
    [TestCase(false,2)] [TestCase(true,1)]
    public void AttackCostSettingChoosesZeroOrOne(bool enabled,int remaining)
    {var r=R(YummnRulesSnapshot.Current(attackOne:enabled));r.Yummn.ki=2;var e=E(r,2,3,100);Assert.IsTrue(r.TryGlobalInput(KaitDirection.Right).valid);Assert.AreEqual(99,e.hp);Assert.AreEqual(remaining,r.ExactKi);}
    [Test] public void OneKiAttackExhaustsButPunchAndNextFreePunchStillWork()
    {var r=R(YummnRulesSnapshot.Current(attackOne:true));r.Yummn.ki=1;var e=E(r,2,3,100);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(99,e.hp);Assert.AreEqual(0,r.ExactKi);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(98,e.hp);Assert.AreEqual(10,r.Yummn.attackSpentTenths);Assert.AreEqual(1,r.EnemyResolveCount);}
    [Test] public void OneKiSettingAllowsZeroKiPunch()
    {var r=R(YummnRulesSnapshot.Current(attackOne:true));r.Yummn.ki=0;var e=E(r,2,3,100);Assert.IsTrue(r.TryGlobalInput(KaitDirection.Right).valid);Assert.AreEqual(99,e.hp);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);Assert.AreEqual(0,r.ExactKi);}
    [Test] public void OneKiFlurryFinishesAfterRunningOut()
    {var r=R(YummnRulesSnapshot.Current(attackOne:true));r.Yummn.ki=2;E(r,2,3,100);r.skills.Add(KaitSkill.Flurry);var a=Context(r);a.yummnAction.direction=KaitDirection.Right;a.yummnAction.plannedSkills.Add("yummn.M01");Call(r,"ResolveYummnPlayerAction",a);Assert.AreEqual(3,a.yummnPunches);Assert.AreEqual(20,a.yummnAction.attackCostTenths);Assert.IsTrue(a.yummnAction.reachedZeroKi);}
    [Test] public void OneKiSnapshotDistinctFromLegacyTenthAndFree()
    {var one=YummnRulesSnapshot.Current(attackOne:true);var copy=JsonUtility.FromJson<YummnRulesSnapshot>(JsonUtility.ToJson(one));Assert.AreEqual(10,copy.AttackCostTenths);Assert.IsFalse(copy.AttackCostsTenth);Assert.AreNotEqual(one.ScoreKey,YummnRulesSnapshot.Current(attackTenth:true).ScoreKey);Assert.AreNotEqual(one.ScoreKey,YummnRulesSnapshot.Current().ScoreKey);StringAssert.Contains("攻击耗气",YummnTutorial.ForRules(copy)[0].Title);}
    [Test] public void NewPreferenceOverridesLegacyAndDoesNotMutateCurrentRun()
    {
        string key="Kait.Yummn082.AttackOne",old="Kait.Yummn082.AttackTenth";bool had=PlayerPrefs.HasKey(key),hadOld=PlayerPrefs.HasKey(old);int value=PlayerPrefs.GetInt(key),oldValue=PlayerPrefs.GetInt(old);
        try{var current=R().Yummn.rules;var method=typeof(KaitGame).GetMethod("YummnPreset",BindingFlags.Static|BindingFlags.NonPublic);PlayerPrefs.DeleteKey(key);PlayerPrefs.SetInt(old,1);Assert.AreEqual(10,((YummnRulesSnapshot)method.Invoke(null,null)).AttackCostTenths);PlayerPrefs.SetInt(key,0);Assert.AreEqual(0,((YummnRulesSnapshot)method.Invoke(null,null)).AttackCostTenths);PlayerPrefs.SetInt(key,1);Assert.AreEqual(10,((YummnRulesSnapshot)method.Invoke(null,null)).AttackCostTenths);Assert.AreEqual(0,current.AttackCostTenths);}
        finally{if(had)PlayerPrefs.SetInt(key,value);else PlayerPrefs.DeleteKey(key);if(hadOld)PlayerPrefs.SetInt(old,oldValue);else PlayerPrefs.DeleteKey(old);}
    }
    [Test] public void OptionAttackTenTenthsExhaustsWithoutBlockingPunch()
    {var r=R(YummnRulesSnapshot.Current(attackTenth:true));r.Yummn.ki=1;var e=E(r,2,3,100);for(int i=0;i<10;i++)Assert.IsTrue(r.TryGlobalInput(KaitDirection.Right).valid);Assert.AreEqual(90,e.hp);Assert.AreEqual(0,r.ExactKi);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);Assert.AreEqual(10,r.Yummn.attackSpentTenths);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(89,e.hp);Assert.AreEqual(10,r.Yummn.attackSpentTenths);Assert.AreEqual(1,r.EnemyResolveCount);}
    [Test] public void OptionZeroKiStillPunchesAndEntersExhaustion()
    {var r=R(YummnRulesSnapshot.Current(attackTenth:true));r.Yummn.ki=0;var e=E(r,2,3,5);var a=r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(a.valid);Assert.AreEqual(4,e.hp);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);}
    [Test] public void OptionFractionSurvivesWholeMovementCost()
    {var r=R(YummnRulesSnapshot.Current(attackTenth:true));E(r,2,3,5);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(5.9f,r.ExactKi,.001);r.enemies.Clear();r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1.9f,r.ExactKi,.001);}
    [Test] public void OptionMovePhaseOrKillOnlyRunsOnce()
    {var r=R(YummnRulesSnapshot.Current(movePhase:true,attackPhase:true));E(r,2,3,1);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,r.EnemyResolveCount);Assert.IsTrue(a.yummnAction.enemyPhaseReason.HasFlag(YummnEnemyPhaseReason.Movement));Assert.IsTrue(a.yummnAction.enemyPhaseReason.HasFlag(YummnEnemyPhaseReason.Kill));}
    [Test] public void OptionMovePhaseIncludesTravelButNotStationaryNonkill()
    {var r=R(YummnRulesSnapshot.Current(movePhase:true));E(r,2,3,5);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(0,r.EnemyResolveCount);r.enemies.Clear();r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,r.EnemyResolveCount);}
    [Test] public void OptionEveryDirectionSuppliesOnBlockedInput()
    {var r=R(YummnRulesSnapshot.Current(supply:YummnTileSupplyMode.EveryAction),1,2);var a=r.TryGlobalInput(KaitDirection.Left);Assert.IsTrue(a.valid);Assert.AreEqual(1,a.newThreatCells.Count);Assert.IsFalse(a.yummnAction.playerMoved);}
    [Test] public void OptionEffectiveMoveIncludesKillFollowOnlyOnce()
    {var r=R(YummnRulesSnapshot.Current(supply:YummnTileSupplyMode.EffectiveMove,movePhase:true));E(r,2,3,1);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,a.newThreatCells.Count);Assert.AreEqual(1,r.EnemyResolveCount);}
    [Test] public void OptionStationaryPunchNoEffectiveSupply()
    {var r=R(YummnRulesSnapshot.Current(supply:YummnTileSupplyMode.EffectiveMove));E(r,2,3,5);Assert.AreEqual(0,r.TryGlobalInput(KaitDirection.Right).newThreatCells.Count);}
    [Test] public void OptionSnapshotRoundTripAndScoreSeparation()
    {var r=YummnRulesSnapshot.Current(attackTenth:true,movePhase:true,supply:YummnTileSupplyMode.EveryAction);var copy=JsonUtility.FromJson<YummnRulesSnapshot>(JsonUtility.ToJson(r));Assert.IsTrue(copy.Valid&&copy.AttackCostsTenth&&copy.MovementAdvancesEnemyPhase);Assert.AreEqual(r.ScoreKey,copy.ScoreKey);Assert.AreNotEqual(YummnRulesSnapshot.Current().ScoreKey,r.ScoreKey);}
    [Test] public void OptionFlurryChargesPerPunchAndStopsChargingAfterZero()
    {var r=R(YummnRulesSnapshot.Current(attackTenth:true));r.Yummn.ki=0;r.Yummn.kiTenths=2;E(r,2,3,100);r.skills.Add(KaitSkill.Flurry);var a=Context(r);a.yummnAction.direction=KaitDirection.Right;a.yummnAction.plannedSkills.Add("yummn.M01");Call(r,"ResolveYummnPlayerAction",a);Assert.AreEqual(3,a.yummnPunches);Assert.AreEqual(2,a.yummnAction.attackCostTenths);Assert.IsTrue(a.yummnAction.reachedZeroKi);}
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static void Set(KaitRun r,string name,object value){var p=typeof(KaitRun).GetProperty(name);if(p!=null)p.SetValue(r,value);else typeof(KaitRun).GetField(name,Hidden).SetValue(r,value);}
    static object Call(KaitRun r,string name,params object[] args)=>typeof(KaitRun).GetMethod(name,Hidden).Invoke(r,args);
    static KaitRun R(YummnRulesSnapshot rules=null,int x=1,int y=3)
    {var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,8201,rules);r.enemies.Clear();r.spawns.Clear();Array.Clear(r.threat,0,r.threat.Length);Set(r,"katePos",new Vector2Int(x,y));return r;}
    static KaitEnemy E(KaitRun r,int x,int y,int hp=5,KaitEnemyType type=KaitEnemyType.Grunt)
    {var e=new KaitEnemy{id=100+r.enemies.Count,pos=new Vector2Int(x,y),hp=hp,maxHp=hp,type=type,life=KaitEnemyLife.Active};r.enemies.Add(e);return e;}
    static void Skill(KaitRun r,KaitSkill s){r.skills.Add(s);Assert.IsTrue(r.TryUseSkill(s,-1,out _));}
    static void Exhaust(KaitRun r,int ki=0){r.Yummn.phase=YummnPhase.Exhausted;r.Yummn.ki=ki;}
    static KaitTurnResult Context(KaitRun r)=>new KaitTurnResult{yummnAction=new YummnActionContext{phaseAtStart=r.KiPhase,startCell=r.katePos}};
    static void Marker(KaitRun r,int x,int y)=>r.Yummn.afterimages.Add(new YummnAfterimageMarker{id=++r.Yummn.nextAfterimageId,cell=new Vector2Int(x,y)});
    static void Aim(KaitEnemy e,KaitIntentType type,params Vector2Int[] cells)
    {e.intent=new KaitIntent{type=type,origin=e.pos,target=cells.Last(),direction=Vector2Int.right,damage=1};e.intent.affectedCells.AddRange(cells);}
    static void Rift(KaitRun r,int x,int y,int tier)=>r.spawns.Add(new KaitSpawnRequest{targetCell=new Vector2Int(x,y),sourceThreatCell=new Vector2Int(x-1,y-1),tier=tier});
    static void Locked(KaitRun r){for(int x=0;x<5;x++)for(int y=0;y<5;y++)if(!r.threatPillars[x,y])r.threat[x,y]=(x+y)%2==0?2:4;}

    [Test] public void T01_FreeAdjacentPunchThreeHitsOnlyKillTicks()
    {var r=R();r.Yummn.ki=2;E(r,2,3,3);for(int i=0;i<3;i++){var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(0,a.yummnAction.totalKiCost);Assert.AreEqual(i==2?1:0,r.EnemyResolveCount);Assert.AreEqual(i==2?5:2,r.Ki);}Assert.AreEqual(1,r.kills);}
    [Test] public void T02_AttackPhaseIsOneEvenOnKill()
    {var r=R(YummnRulesSnapshot.Current(attackPhase:true));E(r,2,3,3);for(int i=1;i<=3;i++){r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(i,r.EnemyResolveCount);}}
    [TestCase(YummnMovementCostMode.PerCell,3)] [TestCase(YummnMovementCostMode.FixedOne,1)]
    public void T03_ThreeVoluntaryCellsCost(YummnMovementCostMode mode,int cost)
    {var r=R(YummnRulesSnapshot.Current(movement:mode));E(r,5,3);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(3,a.yummnAction.voluntaryCells);Assert.AreEqual(cost,a.yummnAction.movementKiCost);Assert.AreEqual(6-cost,r.Ki);}
    [Test] public void T04_AdjacentAndBlockedThreatOnlyAreFree()
    {var r=R();E(r,2,3);Assert.AreEqual(0,r.TryGlobalInput(KaitDirection.Right).yummnAction.totalKiCost);r=R(null,1,2);r.threat[1,2]=2;Assert.AreEqual(0,r.TryGlobalInput(KaitDirection.Left).yummnAction.totalKiCost);Assert.AreEqual(0,r.Yummn.afterimages.Count);}
    [Test] public void T05_KillFollowIsFreeAndLeavesNoMarker()
    {var r=R();E(r,2,3,1);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(new Vector2Int(2,3),r.katePos);Assert.AreEqual(0,a.yummnAction.movementKiCost);Assert.AreEqual(0,r.Yummn.metrics.afterimagesCreated);Assert.IsTrue(a.yummnEvents.Any(e=>e.moveCause==YummnMoveCause.KillFollow));}
    [Test] public void T06_OneKiBrakesAfterOneCell()
    {var r=R();r.Yummn.ki=1;var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(new Vector2Int(2,3),r.katePos);Assert.AreEqual(0,r.Ki);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);Assert.AreEqual(1,r.Yummn.afterimages.Count);Assert.AreEqual(new Vector2Int(1,3),r.Yummn.afterimages[0].cell);Assert.AreEqual(0,r.EnemyResolveCount);}
    [Test] public void T07_RealMovementMarkerCanBeHit()
    {var r=R();var e=E(r,1,4);Aim(e,KaitIntentType.Melee,r.katePos);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,r.Yummn.afterimages.Count);var a=Context(r);Call(r,"ResolveYummnEnemyPhase",a);Assert.AreEqual(3,r.Ki);Assert.AreEqual(1,r.Yummn.metrics.afterimageKi);Assert.AreEqual(1,r.Yummn.afterimages.Count);Call(r,"ClearYummn082Afterimages",a);Assert.AreEqual(0,r.Yummn.afterimages.Count);}
    [Test] public void SameMarkerSurvivesSeveralAttacksUntilPhaseCleanup()
    {
        var r=R();r.Yummn.ki=0;Marker(r,2,2);
        Aim(E(r,2,1),KaitIntentType.Melee,new Vector2Int(2,2));
        Aim(E(r,3,2),KaitIntentType.Melee,new Vector2Int(2,2));
        var result=Context(r);Call(r,"ResolveYummnEnemyPhase",result);
        Assert.AreEqual(2,r.Ki);Assert.AreEqual(2,r.Yummn.metrics.afterimagesHit);
        Assert.AreEqual(1,r.Yummn.afterimages.Count);Assert.IsTrue(r.Yummn.afterimages[0].alive);
        Call(r,"ClearYummn082Afterimages",result);Assert.AreEqual(0,r.Yummn.afterimages.Count);
        Assert.AreEqual(1,result.yummnEvents.Count(e=>e.kind==YummnEventKind.AfterimageCleared));
    }
    [Test] public void T08_CrossConsumesTwoMarkersButGainsOnlyOne()
    {var r=R();r.Yummn.ki=0;Marker(r,2,2);Marker(r,3,2);var e=E(r,4,4,2,KaitEnemyType.Warlock);Aim(e,KaitIntentType.CrossBlast,new Vector2Int(2,2),new Vector2Int(3,2));Call(r,"ResolveYummnEnemyPhase",Context(r));Assert.AreEqual(1,r.Ki);Assert.AreEqual(2,r.Yummn.metrics.afterimagesHit);}
    [Test] public void T09_DifferentAttackEventsEachRestoreOne()
    {var r=R();r.Yummn.ki=0;Marker(r,2,2);Marker(r,3,2);Aim(E(r,2,1),KaitIntentType.Melee,new Vector2Int(2,2));Aim(E(r,3,1),KaitIntentType.Melee,new Vector2Int(3,2));var a=Context(r);Call(r,"ResolveYummnEnemyPhase",a);Assert.AreEqual(2,r.Ki);Assert.AreEqual(2,a.yummnEvents.Where(e=>e.kind==YummnEventKind.AfterimageHit).Select(e=>e.attackEventId).Distinct().Count());}
    [Test] public void T10_MainAndMarkerCanBothBeHit()
    {var r=R();r.Yummn.ki=0;Marker(r,2,3);var e=E(r,3,3,2,KaitEnemyType.Warlock);Aim(e,KaitIntentType.CrossBlast,r.katePos,new Vector2Int(2,3));Call(r,"ResolveYummnEnemyPhase",Context(r));Assert.AreEqual(2,r.kateHp);Assert.AreEqual(1,r.Ki);}
    [Test] public void T11_UnhitMarkersExpireWithoutKi()
    {var r=R();Marker(r,3,3);Exhaust(r);var a=r.TryGlobalInput(KaitDirection.Up);Assert.AreEqual(0,r.Yummn.afterimages.Count);Assert.AreEqual(0,r.Yummn.metrics.afterimageKi);Assert.AreEqual(1,r.Ki);Assert.IsTrue(a.yummnEvents.Any(e=>e.kind==YummnEventKind.AfterimageCleared));}
    [Test] public void T12_PushedMeleeIntentTranslates()
    {var r=R();var e=E(r,2,3);Aim(e,KaitIntentType.Melee,r.katePos);e.intent.direction=Vector2Int.left;Call(r,"ForceYummnEnemy",e,Vector2Int.up,1,Context(r));Assert.AreEqual(new Vector2Int(2,4),e.intent.origin);Assert.AreEqual(new Vector2Int(1,4),e.intent.target);Assert.AreEqual(Vector2Int.left,e.intent.direction);CollectionAssert.AreEqual(new[]{new Vector2Int(1,4)},e.intent.affectedCells);}
    [Test] public void T13_PushedArcherKeepsDirectionRebuilds()
    {var r=R(null,5,3);var e=E(r,2,3,2,KaitEnemyType.Archer);Aim(e,KaitIntentType.LineShot,r.katePos);e.rangedState=KaitRangedState.Aim;Call(r,"ForceYummnEnemy",e,Vector2Int.up,1,Context(r));Assert.AreEqual(new Vector2Int(2,4),e.intent.origin);Assert.AreEqual(Vector2Int.right,e.intent.direction);CollectionAssert.AreEqual(new[]{new Vector2Int(3,4),new Vector2Int(4,4),new Vector2Int(5,4)},e.intent.affectedCells);}
    [Test] public void T14_PushedWarlockKeepsGroundLock()
    {var r=R();var e=E(r,2,3,2,KaitEnemyType.Warlock);Aim(e,KaitIntentType.CrossBlast,r.katePos,new Vector2Int(1,4));var old=e.intent.affectedCells.ToArray();Call(r,"ForceYummnEnemy",e,Vector2Int.up,1,Context(r));CollectionAssert.AreEqual(old,e.intent.affectedCells);Assert.AreEqual(new Vector2Int(1,4),e.intent.target);}
    [Test] public void T15_KillClampsAndTracksOverflow()
    {var r=R();r.Yummn.ki=5;E(r,2,3,1);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(6,r.Ki);Assert.AreEqual(1,r.Yummn.metrics.killKi);Assert.AreEqual(2,r.Yummn.metrics.overflowKi);}
    [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
    public void T16_MaxKiSnapshotReplayAndHud(int max)
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,8201,YummnRulesSnapshot.Current(max));var save=r.SaveReplay();var c=new KaitRun();Assert.IsTrue(c.RestoreReplay(save));Assert.AreEqual(max,c.Ki);Assert.AreEqual(save,c.SaveReplay());
        var root=new GameObject("082 HUD",typeof(RectTransform));root.SetActive(false);
        try{var g=root.AddComponent<KaitGame>();var turn=new GameObject("Turn",typeof(RectTransform),typeof(Text));turn.transform.SetParent(root.transform,false);typeof(KaitGame).GetField("turnText",Hidden).SetValue(g,turn.GetComponent<Text>());typeof(KaitGame).GetField("uiFont",Hidden).SetValue(g,Resources.Load<Font>("NotoSansCJKsc-Regular"));var live=(KaitRun)typeof(KaitGame).GetField("run",Hidden).GetValue(g);live.SelectCharacter(KaitCharacter.Yummn,8201,YummnRulesSnapshot.Current(max));typeof(KaitGame).GetMethod("RefreshYummnKiDisplay",Hidden).Invoke(g,null);var pips=(Image[])typeof(KaitGame).GetField("actionPips",Hidden).GetValue(g);Assert.AreEqual(max,pips.Length);foreach(var p in pips){Assert.Greater(p.rectTransform.anchoredPosition.x-10.5f,-33);Assert.Less(p.rectTransform.anchoredPosition.x+10.5f,125);}}
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
    [Test] public void T17_ExhaustionStaysUntilFull()
    {var r=R();Exhaust(r);E(r,2,3,1);r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(4,r.Ki);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);r.TryGlobalInput(KaitDirection.Up);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);r.TryGlobalInput(KaitDirection.Down);Assert.AreEqual(6,r.Ki);Assert.AreEqual(YummnPhase.Burst,r.KiPhase);}
    [Test] public void T18_FastRecoveryExitsAfterExhaustedAction()
    {var r=R(YummnRulesSnapshot.Current(fullRecovery:false));Exhaust(r);r.TryGlobalInput(KaitDirection.Up);Assert.AreEqual(1,r.Ki);Assert.AreEqual(YummnPhase.Burst,r.KiPhase);}
    [Test] public void T19_ExhaustedKillOnePhaseKillPlusNatural()
    {var r=R();Exhaust(r);E(r,2,3,1);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(4,r.Ki);Assert.AreEqual(1,r.EnemyResolveCount);Assert.AreEqual(1,a.newThreatCells.Count);}
    [TestCase(2,3,true)] [TestCase(4,3,false)]
    public void T20_T21_NewbornSwordOneBehavior(int x,int y,bool adjacent)
    {var r=R(null,1,3);Exhaust(r,1);Skill(r,KaitSkill.ShapeIce);Rift(r,x,y,2);var a=r.TryGlobalInput(KaitDirection.Up);var e=r.enemies.Single();Assert.AreEqual(KaitEnemyType.Swordsman,e.type);Assert.AreEqual(adjacent?KaitIntentType.Melee:KaitIntentType.None,e.intent.type);Assert.IsFalse(a.yummnEvents.Any(v=>v.kind==YummnEventKind.EnemyAttack));Assert.AreEqual(adjacent?0:1,a.enemyActions.Count(v=>v.type==KaitIntentType.Move));}
    [Test] public void T22_NewbornArcherOnlyAims()
    {var r=R();Exhaust(r);Rift(r,4,3,3);var a=r.TryGlobalInput(KaitDirection.Up);Assert.AreEqual(KaitIntentType.LineShot,r.enemies.Single().intent.type);Assert.IsFalse(a.yummnEvents.Any(e=>e.kind==YummnEventKind.EnemyAttack));}
    [Test] public void T23_MultiKillOnePhaseSupplyPerKill()
    {var r=R();E(r,2,3,2);E(r,3,3,1);Skill(r,KaitSkill.Flurry);r.passives.Add(KaitPassive.FireSnake);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,r.kills);Assert.AreEqual(2,a.newThreatCells.Count);Assert.AreEqual(1,r.EnemyResolveCount);Assert.AreEqual(2,a.yummnAction.rewardedKills);}
    [Test] public void T24_StationaryKillFollowNeverActionSupply()
    {var r=R(YummnRulesSnapshot.Current(supply:YummnTileSupplyMode.SkipStationaryPunch));E(r,2,3,1);E(r,3,3,1);r.passives.Add(KaitPassive.FireSnake);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(2,r.kills);Assert.AreEqual(0,a.newThreatCells.Count);Assert.AreEqual(new Vector2Int(2,3),r.katePos);}
    [TestCase(false)] [TestCase(true)] public void T25_SpawnMapping(bool eight)
    {foreach(int value in new[]{4,8,16,32,64,128}){var r=R(YummnRulesSnapshot.Current(fromEight:eight));var a=Context(r);Call(r,"QueueYummnRift",new KaitMergeEvent{resultValue=value,threatCell=new Vector2Int(2,2)},a);if(eight&&value==4)Assert.AreEqual(0,r.spawns.Count);else Assert.AreEqual(Mathf.Clamp((int)Mathf.Log(value,2)-(eight?2:1),1,5),r.spawns.Single().tier);}}
    [Test] public void T26_FullMergeAvailableStillAlive(){var r=R();Locked(r);r.threat[2,2]=r.threat[3,2];Assert.IsFalse(r.IsYummnThreatLocked());r.TryGlobalInput(KaitDirection.Right);Assert.IsFalse(r.ended);}
    [Test] public void T27_LockedDefeatNoReset(){var r=R();Locked(r);var before=(int[,])r.threat.Clone();r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(r.ended);Assert.AreEqual("ThreatBoardLocked",r.endReason);CollectionAssert.AreEqual(before,r.threat);}
    [Test] public void T28_BossVictoryBeatsLock(){var r=R();Locked(r);var e=E(r,2,3,1,KaitEnemyType.ShieldKnight);e.facing=Vector2Int.right;Set(r,"bossEnemyId",e.id);r.TryGlobalInput(KaitDirection.Right);Assert.IsTrue(r.won);Assert.AreEqual(0,r.EnemyResolveCount);}
    [Test] public void ZeroCrossingCannotBeCancelledBySameActionKill(){var r=R();r.Yummn.ki=1;E(r,3,3,1);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(3,r.Ki);Assert.IsTrue(a.yummnAction.reachedZeroKi);Assert.AreEqual(YummnPhase.Exhausted,r.KiPhase);}
    [Test] public void SkillCostReservedBeforePayableMovement(){var r=R();r.Yummn.ki=2;Skill(r,KaitSkill.Flurry);var a=r.TryGlobalInput(KaitDirection.Right);Assert.AreEqual(1,a.yummnAction.skillKiCost);Assert.AreEqual(1,a.yummnAction.voluntaryCells);Assert.AreEqual(0,r.Ki);}
    [Test] public void InvalidSkillDoesNotMutateEitherBoardOrMarkers(){var r=R();Skill(r,KaitSkill.WaterWhip);var save=r.SaveReplay();var a=r.TryGlobalInput(KaitDirection.Up);Assert.IsFalse(a.valid);Assert.AreEqual(save,r.SaveReplay());Assert.AreEqual(0,r.Yummn.afterimages.Count);}
    [Test] public void PreviewIsReadOnlyAndMatchesCommit(){var r=R();E(r,5,3);var save=r.SaveReplay();for(int i=0;i<10;i++)Assert.AreEqual(3,r.PreviewYummnMovementCost(KaitDirection.Right));Assert.AreEqual(save,r.SaveReplay());Assert.AreEqual(3,r.TryGlobalInput(KaitDirection.Right).yummnAction.movementKiCost);}
    [Test] public void CounterKillIsRewardedSuppliedOnceNoRecursivePhase(){var r=R(null,3,3);Exhaust(r,1);Skill(r,KaitSkill.ShapeIce);r.passives.Add(KaitPassive.Opportunist);E(r,1,3,1,KaitEnemyType.Swordsman);var a=r.TryGlobalInput(KaitDirection.Up);Assert.AreEqual(1,r.kills);Assert.AreEqual(1,r.EnemyResolveCount);Assert.AreEqual(4,r.Ki);Assert.AreEqual(1,a.newThreatCells.Count);}
    [Test] public void NoIntentIsNotCreatedByPush(){var r=R();var e=E(r,2,3);Call(r,"ForceYummnEnemy",e,Vector2Int.up,1,Context(r));Assert.AreEqual(KaitIntentType.None,e.intent.type);}
    [Test] public void FullBoardDropsSupplyNoDebt(){var r=R();Locked(r);var a=Context(r);a.yummnAction.killIds.AddRange(new[]{1,2});Call(r,"SupplyYummn082Twos",a,false);Assert.AreEqual(2,a.yummnAction.droppedTwos);r.threat[1,1]=0;Call(r,"SupplyYummn082Twos",a,true);Assert.AreEqual(0,r.threat[1,1]);}
    [Test] public void AllSevenOptionsSeparateScoreGroups(){var all=new HashSet<string>();foreach(int max in new[]{3,4,5,6})foreach(int kill in new[]{1,2,3})foreach(bool fixedCost in new[]{false,true})foreach(bool attack in new[]{false,true})foreach(bool full in new[]{false,true})foreach(bool supply in new[]{false,true})foreach(bool eight in new[]{false,true})Assert.IsTrue(all.Add(YummnRulesSnapshot.Current(max,fixedCost?YummnMovementCostMode.FixedOne:YummnMovementCostMode.PerCell,kill,attack,full,supply?YummnTileSupplyMode.SkipStationaryPunch:YummnTileSupplyMode.KillOnly,eight).ScoreKey));Assert.AreEqual(384,all.Count);}
    [TestCase(8201)] [TestCase(8202)] [TestCase(8203)] public void DeterministicReplayIncludesMarkers(int seed)
    {var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,seed);r.config.playerInvincible=true;r.RecordSettingsChange();for(int i=0;i<45&&!r.ended;i++)r.TryGlobalInput((KaitDirection)(i%4));var copy=new KaitRun();Assert.IsTrue(copy.RestoreReplay(r.SaveReplay()));Assert.AreEqual(r.SaveReplay(),copy.SaveReplay());Assert.AreEqual(r.Ki,copy.Ki);Assert.AreEqual(r.EnemyResolveCount,copy.EnemyResolveCount);CollectionAssert.AreEqual(r.Yummn.afterimages.Select(m=>m.cell),copy.Yummn.afterimages.Select(m=>m.cell));}
    [Test] public void TutorialCurrentHasFourPagesAndCorrectRules(){var pages=YummnTutorial.ForRules(YummnRulesSnapshot.Current());Assert.AreEqual(4,pages.Length);StringAssert.Contains("上限6",pages[0].Lead);StringAssert.Contains("击杀+3",pages[0].Lead);StringAssert.Contains("残影",pages[1].Title);}
}
