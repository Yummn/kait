using System.Collections.Generic;
using UnityEngine;
public sealed partial class KaitRun
{
 private KaitTurnResult BeginReynardAction(){ActivateBuildForInput();Reynard.actionId++;turnTriggers.Clear();return new KaitTurnResult{valid=true,threatBefore=CopyThreat(),globalDirection=Reynard.lastDirection,kaitDirection=Reynard.lastDirection};}
 private KaitTurnResult TryReynardDirection(KaitDirection d)
 {
  if(ended)return new KaitTurnResult{message="本局已结束"};var r=BeginReynardAction();currentDirection=currentGlobalDirection=actualThreatDirection=d;r.globalDirection=r.kaitDirection=d;
  var next=katePos+Delta(d);bool moved=!IsHardBlocked(next)&&EnemyAt(next)==null;if(moved){katePos=next;r.katePath.Add(next);r.pathMomentum.Add(0);}else r.kaitWaited=true;
  // Direction input is always an attack.  Being stopped by a unit, pillar or
  // board edge only cancels displacement; the shot and its attached effects
  // still resolve from Reynard's current cell.
  ResolveReynardShot(d,r);
  r.merges.AddRange(MoveThreatReynard(d,r.threatMotions));ProcessReynardMerges(r);r.threatChanged=!ThreatEquals(r.threatBefore,CopyThreat());
  FinishReynardAction(r);Reynard.lastDirection=d;Reynard.hasLastDirection=true;return r;
 }
 private KaitTurnResult TryReynardWait()
 {
  if(ended)return new KaitTurnResult{message="本局已结束"};var r=BeginReynardAction();var p=katePos-Vector2Int.one;
  bool found=p.x>=0&&p.y>=0&&p.x<ThreatSize&&p.y<ThreatSize&&!IsThreatPillar(p)&&threat[p.x,p.y]>0;
  if(found){Reynard.mirrorFoxCell=p;Reynard.summons++;}r.message=found?"镜狐已转移":"召狐失败，保留当前";
  if(HasPassive(KaitPassive.ReynardStillBackfire)&&Reynard.hasLastDirection)ResolveReynardShot(Reynard.lastDirection,r);
  FinishReynardAction(r);RecordReplay("wait",0,0,0);return r;
 }
 private void FinishReynardAction(KaitTurnResult r)
 {
  var p=SpawnThreatTwoForTurn(r);if(p.x>=0)r.newThreatCells.Add(p);
  // Resolve old rifts before the phase; freshly spawned enemies remain Preparing.
  bool ward=Reynard.mirrorWard;Reynard.mirrorWard=false;
  ResolveSpawnRequests(r);if(!ended&&bossPending)SpawnShieldKnight(r);
  Reynard.mirrorWard=ward;
  if(!ended){ResolveEnemyIntents(r);Reynard.enemyPhases++;}AgePreparingEnemies();
  Reynard.mirrorWard=false;turn++;r.turnComplete=true;r.threatAfter=CopyThreat();LockEnemyIntents();
  if(ended)Reynard.focusedSkill=KaitSkill.None;
 }
 public bool ReynardPrerequisite(KaitAbilityDef d)
 {
  bool Tag(string tag)=>skills.Exists(s=>ReynardCatalog.Has(ReynardCatalog.Get(s),tag));
  switch(d.passive){
   case KaitPassive.ReynardArcaneRecall:case KaitPassive.ReynardArcaneDevour:case KaitPassive.ReynardMirrorWard:return skills.Count>0;
   case KaitPassive.ReynardReprise:return Tag("ReynardAttackSpell");
   case KaitPassive.ReynardDiffusion:return Tag("ReynardAttackSpell")||Tag("ReynardSustainAttack");
   case KaitPassive.ReynardDistantSpell:return Tag("ReynardTargeted");
   case KaitPassive.ReynardConcord:return Tag("ReynardInstant")&&(Tag("ReynardSustainAttack")||skills.Contains(KaitSkill.ReynardQuickenedCircle));
   default:return true;
  }
 }
 public string ReynardCastFailure(KaitSkill s)
 {
  var d=ReynardCatalog.Get(s);if(ended||d==null||!skills.Contains(s))return "技能未装备";
  if(Reynard.focusedSkill==s)return "维持中";
  if(!ReynardSlotReady)return "法术位已耗竭";
  if(ReynardCatalog.Level(ReynardSlotValue)<d.spellLevel)return "需要更高环阶";return null;
 }
 public bool IsLegalReynardCell(KaitSkill s,Vector2Int p)
 {
  var d=ReynardCatalog.Get(s);if(d==null)return false;
  if(s==KaitSkill.ReynardLightningBolt)return Manhattan(katePos,p)==1;
  if(!ReynardCatalog.Has(d,"ReynardTargeted"))return p==katePos;
  if(!Inside(p)||IsHardBlocked(p)||Manhattan(katePos,p)>3+(HasPassive(KaitPassive.ReynardDistantSpell)?2:0))return false;
  if(s==KaitSkill.ReynardMistyStep)return p!=katePos&&EnemyAt(p)==null;
  if(s==KaitSkill.ReynardChainLightning)return EnemyAt(p)!=null;return true;
 }
 private bool CastReynard(KaitSkill s,Vector2Int p,out string message)
 {
  message=ReynardCastFailure(s);if(message!=null)return false;if(!IsLegalReynardCell(s,p)){message="目标不合法";return false;}
  var r=BeginReynardAction();r.reynardCast=s;lastSkillResult=r;var d=ReynardCatalog.Get(s);Reynard.spellReady[Reynard.mirrorFoxCell.x,Reynard.mirrorFoxCell.y]=false;Reynard.casts++;
  if(HasPassive(KaitPassive.ReynardMirrorWard))Reynard.mirrorWard=true;
  if(ReynardCatalog.Has(d,"ReynardSustain")){Reynard.focusedSkill=s;Reynard.focusedCell=p;}
  else{
   ResolveReynardInstant(s,p,r);
   if(Reynard.focusedSkill==KaitSkill.ReynardQuickenedCircle&&Reynard.hasLastDirection)ResolveReynardShot(Reynard.lastDirection,r);
   if(HasPassive(KaitPassive.ReynardConcord))ReynardAutoAttack(r);
  }
  FinishReynardAction(r);message="已施放："+d.nameZh;RecordReplay("cellskill",(int)s,p.x,p.y);return true;
 }
}
