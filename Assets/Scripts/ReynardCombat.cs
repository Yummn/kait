using System.Collections.Generic;
using UnityEngine;
public sealed partial class KaitRun
{
 private void ReynardHit(KaitEnemy e,int amount,ReynardDamageKind kind,KaitTurnResult r,bool reprise=false)
 {
  if(e==null||e.life==KaitEnemyLife.Dead)return;var p=e.pos;
  var dealt=DamageEnemyWithContext(e,new KaitDamageContext{kind=KaitDamageKind.Spell,baseAmount=amount,creditKate=true,direction=DirectionToward(katePos,p)},r).actualDamage;
  r.reynardEvents.Add(new ReynardVisualEvent{kind=kind.ToString(),from=katePos,to=p,amount=dealt});
  if(dealt>0&&kind==ReynardDamageKind.SpellDirect&&HasPassive(KaitPassive.ReynardDiffusion))foreach(var other in new List<KaitEnemy>(enemies))if(other.id!=e.id&&Manhattan(other.pos,p)==1)ReynardHit(other,1,ReynardDamageKind.SpellSplash,r);
  if(reprise&&dealt>0&&e.life!=KaitEnemyLife.Dead&&HasPassive(KaitPassive.ReynardReprise))ReynardHit(e,dealt,ReynardDamageKind.SpellDirect,r);
 }
 private bool ReynardFogAt(Vector2Int p)=>IsReynard&&Reynard.focusedSkill==KaitSkill.ReynardFog&&Manhattan(p,Reynard.focusedCell)<=1;
 private void ReynardRay(Vector2Int delta,int damage,bool piercing,ReynardDamageKind kind,KaitTurnResult r,bool reprise=false)
 {
  if(ReynardFogAt(katePos))return;
  for(var p=katePos+delta;Inside(p);p+=delta){if(IsHardBlocked(p)||ReynardFogAt(p))break;r.reynardEvents.Add(new ReynardVisualEvent{kind="Ray",from=p-delta,to=p});var e=EnemyAt(p);if(e==null)continue;ReynardHit(e,damage,kind,r,reprise);if(!piercing)break;}
 }
 private void ReynardPush(KaitEnemy e,Vector2Int delta,KaitTurnResult r)
 {
  if(e==null||e.life==KaitEnemyLife.Dead)return;var to=e.pos+delta;if(IsHardBlocked(to)||to==katePos||EnemyAt(to)!=null)return;
  var from=e.pos;e.pos=to;e.intent=new KaitIntent{origin=to};r.enemyActions.Add(new KaitEnemyAction{enemyId=e.id,type=KaitIntentType.Move,from=from,to=to});
 }
 private void ResolveReynardShot(KaitDirection d,KaitTurnResult r)
 {
  Reynard.directionSpells++;
  Vector2Int forward=Delta(d);FireReynardBullet(forward,r);
  if(HasPassive(KaitPassive.ReynardTrident))
  {FireReynardBullet(new Vector2Int(-forward.y,forward.x),r);FireReynardBullet(new Vector2Int(forward.y,-forward.x),r);}
  ReynardAutoAttack(r);
 }
 private void FireReynardBullet(Vector2Int delta,KaitTurnResult r)
 {
  if(delta==Vector2Int.zero||ReynardFogAt(katePos))return;
  bool piercing=HasPassive(KaitPassive.ReynardInterwoven),push=HasPassive(KaitPassive.ReynardBackfirePush),splash=HasPassive(KaitPassive.ReynardRingFlame);
  var targets=new List<KaitEnemy>();Vector2Int end=katePos+delta;
  for(var p=katePos+delta;Inside(p);p+=delta){end=p;if(IsHardBlocked(p)||ReynardFogAt(p))break;var e=EnemyAt(p);if(e==null)continue;targets.Add(e);if(!piercing)break;}
  r.reynardEvents.Add(new ReynardVisualEvent{kind="Bullet",from=katePos,to=end,amount=1});
  foreach(var e in targets)
  {
   Vector2Int impact=e.pos;ReynardHit(e,1,ReynardDamageKind.DirectionSpell,r);
   if(splash)foreach(var other in new List<KaitEnemy>(enemies))if(other.id!=e.id&&other.life!=KaitEnemyLife.Dead&&Manhattan(other.pos,impact)==1)ReynardHit(other,1,ReynardDamageKind.DirectionSpell,r);
   if(push&&e.life!=KaitEnemyLife.Dead)ReynardPush(e,delta,r);
  }
 }
 private void ReynardAutoAttack(KaitTurnResult r)
 {
  if(Reynard.focusedSkill==KaitSkill.ReynardMissileArray){KaitEnemy target=null;int best=int.MaxValue;foreach(var e in enemies)if(e.life!=KaitEnemyLife.Dead){int dist=Manhattan(e.pos,katePos);if(dist<best||dist==best&&e.id<(target?.id??int.MaxValue)){best=dist;target=e;}}ReynardHit(target,1,ReynardDamageKind.SpellDirect,r);}
  if(Reynard.focusedSkill==KaitSkill.ReynardFoxfire)foreach(var e in new List<KaitEnemy>(enemies))if(Manhattan(e.pos,katePos)==1)ReynardHit(e,1,ReynardDamageKind.SpellDirect,r);
 }
 private void ResolveReynardInstant(KaitSkill s,Vector2Int p,KaitTurnResult r)
 {
  if(s==KaitSkill.ReynardMistyStep){r.katePath.Add(p);r.pathMomentum.Add(0);katePos=p;return;}
  if(s==KaitSkill.ReynardCounterspell){foreach(var e in enemies){e.intent=new KaitIntent{origin=e.pos};e.rangedState=KaitRangedState.Ready;}return;}
  if(s==KaitSkill.ReynardLightningBolt){ReynardRay(p-katePos,3,true,ReynardDamageKind.SpellDirect,r,true);return;}
  if(s==KaitSkill.ReynardChainLightning){var e=EnemyAt(p);var hit=new HashSet<int>();for(int i=0;i<5&&e!=null;i++){var from=e.pos;hit.Add(e.id);ReynardHit(e,3,ReynardDamageKind.SpellDirect,r,true);KaitEnemy next=null;int best=3;foreach(var candidate in enemies)if(candidate.life!=KaitEnemyLife.Dead&&!hit.Contains(candidate.id)){int dist=Manhattan(from,candidate.pos);if(dist<best||dist==best&&next!=null&&candidate.id<next.id){best=dist;next=candidate;}}e=next;}return;}
  foreach(var e in new List<KaitEnemy>(enemies)){
   bool hit=s==KaitSkill.ReynardThunderwave?Manhattan(e.pos,katePos)==1:s==KaitSkill.ReynardShatter?Manhattan(e.pos,p)<=1:s==KaitSkill.ReynardFireball&&Mathf.Abs(e.pos.x-p.x)<=1&&Mathf.Abs(e.pos.y-p.y)<=1;
   if(!hit)continue;ReynardHit(e,2,ReynardDamageKind.SpellDirect,r,true);if(s==KaitSkill.ReynardThunderwave)ReynardPush(e,e.pos-katePos,r);
  }
 }
 private void MoveReynardEnemies(KaitTurnResult r)
 {
  foreach(var e in enemies){
   if(e.life!=KaitEnemyLife.Active||e.frozenActions>0||e.type==KaitEnemyType.Archer||e.type==KaitEnemyType.Warlock||Manhattan(e.pos,katePos)<=1)continue;
   if(Reynard.focusedSkill==KaitSkill.ReynardWeb&&Manhattan(e.pos,Reynard.focusedCell)<=1)continue;
   var delta=DirectionToward(e.pos,katePos);var to=e.pos+delta;if(IsHardBlocked(to)||to==katePos||EnemyAt(to)!=null)continue;
   var from=e.pos;e.pos=to;r.enemyActions.Add(new KaitEnemyAction{enemyId=e.id,type=KaitIntentType.Move,from=from,to=to});
  }
 }
}
