using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
 private int ReynardHit(KaitEnemy e,int amount,ReynardDamageKind kind,KaitTurnResult r,bool reprise=false)
 {
  if(e==null||e.life==KaitEnemyLife.Dead)return 0;var p=e.pos;
  var dealt=DamageEnemyWithContext(e,new KaitDamageContext{kind=KaitDamageKind.Spell,baseAmount=amount,creditKate=true,direction=DirectionToward(katePos,p)},r).actualDamage;
  r.reynardEvents.Add(new ReynardVisualEvent{kind=kind.ToString(),from=katePos,to=p,amount=dealt});
  if(dealt>0&&kind==ReynardDamageKind.SpellDirect&&HasPassive(KaitPassive.ReynardDiffusion))foreach(var other in new List<KaitEnemy>(enemies))if(other.id!=e.id&&Manhattan(other.pos,p)==1)ReynardHit(other,1,ReynardDamageKind.SpellSplash,r);
  if(reprise&&dealt>0&&e.life!=KaitEnemyLife.Dead&&HasPassive(KaitPassive.ReynardReprise))ReynardHit(e,dealt,ReynardDamageKind.SpellDirect,r);
  return dealt;
 }
 private bool ReynardFogAt(Vector2Int p)=>IsReynard&&Reynard.fogCell.x>=0&&Manhattan(p,Reynard.fogCell)<=1;
 private bool ReynardWebAt(Vector2Int p)=>IsReynard&&Reynard.webCell.x>=0&&Manhattan(p,Reynard.webCell)<=1;
 private bool ReynardWallAt(Vector2Int p)=>IsReynard&&Reynard.stoneWallCell==p;
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
 Vector2Int forward=Delta(d);int volleys=HasPassive(KaitPassive.ReynardDoubleVolley)?2:1;bool fired=false;
  for(int volley=0;volley<volleys;volley++){
   var dirs=new List<Vector2Int>{forward};
   if(HasPassive(KaitPassive.ReynardTrident)){dirs.Add(new Vector2Int(-forward.y,forward.x));dirs.Add(new Vector2Int(forward.y,-forward.x));}
   if(HasPassive(KaitPassive.ReynardReverseOrb))dirs.Add(-forward);
   var used=new HashSet<Vector2Int>();foreach(var dir in dirs)if(used.Add(dir))fired|=FireReynardBullet(dir,r);
  }
 if(fired){Reynard.directionSpells++;ReynardAutoAttack(r);}
}
private bool FireReynardBullet(Vector2Int delta,KaitTurnResult r)
{
 if(delta==Vector2Int.zero||ReynardFogAt(katePos))return false;
  bool piercing=HasPassive(KaitPassive.ReynardInterwoven),push=HasPassive(KaitPassive.ReynardBackfirePush),splash=HasPassive(KaitPassive.ReynardRingFlame);
 var targets=new List<KaitEnemy>();Vector2Int end=katePos;
 for(var p=katePos+delta;Inside(p);p+=delta){if(IsHardBlocked(p)||ReynardFogAt(p))break;var e=EnemyAt(p);if(e==null)continue;targets.Add(e);end=p;if(!piercing)break;}
 if(targets.Count==0)return false;
  r.reynardEvents.Add(new ReynardVisualEvent{kind="Bullet",from=katePos,to=end,amount=1});
  foreach(var e in targets)
  {
   if(e.life==KaitEnemyLife.Dead)continue;Vector2Int impact=e.pos;bool adjacent=Manhattan(impact,katePos)==1;int before=e.hp;
   bool charged=adjacent&&HasPassive(KaitPassive.ReynardChargedOrb);int dealt=ReynardHit(e,charged?2:1,ReynardDamageKind.Orb,r);if(charged&&dealt>0)r.reynardEvents.Add(new ReynardVisualEvent{kind="ChargedOrb",from=katePos,to=impact,amount=dealt});bool killed=before>0&&e.life==KaitEnemyLife.Dead;
   if(splash)foreach(var other in new List<KaitEnemy>(enemies))if(other.id!=e.id&&other.life!=KaitEnemyLife.Dead&&Manhattan(other.pos,impact)==1)ReynardHit(other,1,ReynardDamageKind.SpellSplash,r);
   if(dealt>0&&HasPassive(KaitPassive.ReynardSplitOrb))FireReynardFragment(impact,new Vector2Int(-delta.y,delta.x),r);if(dealt>0&&HasPassive(KaitPassive.ReynardSplitOrb))FireReynardFragment(impact,new Vector2Int(delta.y,-delta.x),r);
   if(push&&e.life!=KaitEnemyLife.Dead)ReynardPush(e,delta,r);
   if(killed&&HasPassive(KaitPassive.ReynardStarOrb))ResolveReynardStarChain(impact,e.id,r);
  }
 return true;
}
 private void FireReynardFragment(Vector2Int from,Vector2Int delta,KaitTurnResult r)
 {
  for(var p=from+delta;Inside(p);p+=delta){if(IsHardBlocked(p)||ReynardFogAt(p))break;var e=EnemyAt(p);if(e==null)continue;r.reynardEvents.Add(new ReynardVisualEvent{kind="Fragment",from=from,to=p,amount=1});ReynardHit(e,1,ReynardDamageKind.OrbSecondary,r);break;}
 }
 private void ResolveReynardStarChain(Vector2Int from,int firstId,KaitTurnResult r)
 {
  var hit=new HashSet<int>{firstId};for(int step=0;step<20;step++){KaitEnemy next=null;int best=3;foreach(var e in enemies)if(e.life!=KaitEnemyLife.Dead&&!hit.Contains(e.id)){int d=Manhattan(from,e.pos);if(d<best||d==best&&next!=null&&e.id<next.id){best=d;next=e;}}if(next==null)break;hit.Add(next.id);var p=next.pos;int before=next.hp;r.reynardEvents.Add(new ReynardVisualEvent{kind="Star",from=from,to=p,amount=1});ReynardHit(next,1,ReynardDamageKind.OrbSecondary,r);if(before<=0||next.life!=KaitEnemyLife.Dead)break;from=p;}
 }
 private void ReynardAutoAttack(KaitTurnResult r)
 {
  if(HasPassive(KaitPassive.ReynardMissileArrayPassive)){KaitEnemy target=null;int best=int.MaxValue;foreach(var e in enemies)if(e.life!=KaitEnemyLife.Dead){int dist=Manhattan(e.pos,katePos);if(dist<best||dist==best&&e.id<(target?.id??int.MaxValue)){best=dist;target=e;}}ReynardHit(target,1,ReynardDamageKind.SpellDirect,r);}
  if(HasPassive(KaitPassive.ReynardFoxfirePassive))foreach(var e in new List<KaitEnemy>(enemies))if(Manhattan(e.pos,katePos)==1)ReynardHit(e,1,ReynardDamageKind.SpellDirect,r);
 }
 private void ResolveReynardInstant(KaitSkill s,Vector2Int p,KaitTurnResult r)
 {
  if(s==KaitSkill.ReynardMistyStep||s==KaitSkill.ReynardRuneStep){var from=katePos;r.katePath.Add(p);r.pathMomentum.Add(0);katePos=p;if(s==KaitSkill.ReynardRuneStep)r.reynardEvents.Add(new ReynardVisualEvent{kind="RuneStep",from=from,to=p,amount=1});return;}
  if(s==KaitSkill.ReynardCounterspell){foreach(var e in enemies){e.intent=new KaitIntent{origin=e.pos};e.rangedState=KaitRangedState.Ready;}return;}
  if(s==KaitSkill.ReynardLightningBolt){ReynardRay(p-katePos,3,true,ReynardDamageKind.SpellDirect,r,true);return;}
  if(s==KaitSkill.ReynardScorchingRay){var delta=p-katePos;var impact=p;for(var q=p;Inside(q);q+=delta){if(IsHardBlocked(q)||ReynardFogAt(q))break;impact=q;if(EnemyAt(q)!=null)break;}r.reynardEvents.Add(new ReynardVisualEvent{kind="ScorchingRay",from=katePos,to=impact,amount=3});for(int i=0;i<3;i++)ReynardRay(delta,1,false,ReynardDamageKind.SpellDirect,r,true);return;}
  if(s==KaitSkill.ReynardChainLightning){var e=EnemyAt(p);var hit=new HashSet<int>();for(int i=0;i<5&&e!=null;i++){var from=e.pos;hit.Add(e.id);ReynardHit(e,3,ReynardDamageKind.SpellDirect,r,true);KaitEnemy next=null;int best=3;foreach(var candidate in enemies)if(candidate.life!=KaitEnemyLife.Dead&&!hit.Contains(candidate.id)){int dist=Manhattan(from,candidate.pos);if(dist<best||dist==best&&next!=null&&candidate.id<next.id){best=dist;next=candidate;}}e=next;}return;}
  if(s==KaitSkill.ReynardTailNova){r.reynardEvents.Add(new ReynardVisualEvent{kind="TailNova",from=katePos,to=katePos,amount=3});foreach(var e in new List<KaitEnemy>(enemies))if(Mathf.Abs(e.pos.x-katePos.x)<=1&&Mathf.Abs(e.pos.y-katePos.y)<=1)ReynardHit(e,3,ReynardDamageKind.SpellDirect,r,true);return;}
  if(s==KaitSkill.ReynardMassHold){r.reynardEvents.Add(new ReynardVisualEvent{kind="MassHold",from=p,to=p,amount=1});foreach(var e in enemies)if(Manhattan(e.pos,p)<=1)e.frozenActions=Mathf.Max(e.frozenActions,1);return;}
  foreach(var e in new List<KaitEnemy>(enemies)){
   bool hit=s==KaitSkill.ReynardThunderwave?Manhattan(e.pos,katePos)==1:s==KaitSkill.ReynardShatter?Manhattan(e.pos,p)<=1:s==KaitSkill.ReynardFireball&&Mathf.Abs(e.pos.x-p.x)<=1&&Mathf.Abs(e.pos.y-p.y)<=1;
   if(!hit)continue;ReynardHit(e,2,ReynardDamageKind.SpellDirect,r,true);if(s==KaitSkill.ReynardThunderwave)ReynardPush(e,e.pos-katePos,r);
  }
 }
 private void MoveReynardEnemies(KaitTurnResult r)
 {
  foreach(var e in enemies){
   if(e.life!=KaitEnemyLife.Active||e.frozenActions>0||e.type==KaitEnemyType.Archer||e.type==KaitEnemyType.Warlock||Manhattan(e.pos,katePos)<=1)continue;
   if(ReynardWebAt(e.pos))continue;
   var delta=DirectionToward(e.pos,katePos);var to=e.pos+delta;if(IsHardBlocked(to)||to==katePos||EnemyAt(to)!=null)continue;
   var from=e.pos;e.pos=to;r.enemyActions.Add(new KaitEnemyAction{enemyId=e.id,type=KaitIntentType.Move,from=from,to=to});
  }
 }
}
