using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
 private sealed class ReynardPayment
 {
  public readonly List<Vector2Int> sigils=new List<Vector2Int>();
  public int tails;
  public bool[,] snapshot;
 }
 public int ReynardSigilCount
 {
  get{int n=0;if(Reynard.sigils!=null)for(int y=0;y<ThreatSize;y++)for(int x=0;x<ThreatSize;x++)if(Reynard.sigils[x,y])n++;return n;}
 }
 public int ReynardTailCap=>HasPassive(KaitPassive.ReynardNineTails)?9:3;
 public int ReynardMaterialCount=>ReynardSigilCount+Reynard.tails;

 private KaitTurnResult BeginReynardAction(){ActivateBuildForInput();Reynard.actionId++;turnTriggers.Clear();return new KaitTurnResult{valid=true,threatBefore=CopyThreat(),globalDirection=Reynard.lastDirection,kaitDirection=Reynard.lastDirection};}
 private KaitTurnResult TryReynardDirection(KaitDirection d)
 {
  if(ended)return new KaitTurnResult{message="本局已结束"};var r=BeginReynardAction();currentDirection=currentGlobalDirection=actualThreatDirection=d;r.globalDirection=r.kaitDirection=d;
  var next=katePos+Delta(d);bool moved=!IsHardBlocked(next)&&EnemyAt(next)==null;if(moved){katePos=next;r.katePath.Add(next);r.pathMomentum.Add(0);}else r.kaitWaited=true;
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
   case KaitPassive.ReynardReprise:return Tag("ReynardAttackSpell");
   case KaitPassive.ReynardDiffusion:return Tag("ReynardAttackSpell")||passives.Contains(KaitPassive.ReynardMissileArrayPassive)||passives.Contains(KaitPassive.ReynardFoxfirePassive);
   case KaitPassive.ReynardDistantSpell:return skills.Exists(s=>ReynardCatalog.Has(ReynardCatalog.Get(s),"ReynardTargeted"));
   case KaitPassive.ReynardMirrorWard:case KaitPassive.ReynardRuneDetonation:case KaitPassive.ReynardCircleResonance:case KaitPassive.ReynardFoxfireRite:return skills.Exists(s=>ReynardCatalog.Cost(s)>0);
   default:return true;
  }
 }
 private bool TryPlanReynardPayment(KaitSkill s,Vector2Int target,out ReynardPayment payment,out string error)
 {
  payment=new ReynardPayment();error=null;int cost=ReynardCatalog.Cost(s);
  payment.snapshot=new bool[ThreatSize,ThreatSize];for(int y=0;y<ThreatSize;y++)for(int x=0;x<ThreatSize;x++)payment.snapshot[x,y]=Reynard.sigils[x,y];
  if(s==KaitSkill.ReynardTailNova){if(Reynard.tails<2){error="需要2条狐尾";return false;}payment.tails=2;return true;}
  var candidates=new List<Vector2Int>();for(int y=0;y<ThreatSize;y++)for(int x=0;x<ThreatSize;x++)if(Reynard.sigils[x,y])candidates.Add(new Vector2Int(x,y));
  candidates.Sort((a,b)=>Reynard.sigilCreated[a.x,a.y]!=Reynard.sigilCreated[b.x,b.y]?Reynard.sigilCreated[a.x,a.y].CompareTo(Reynard.sigilCreated[b.x,b.y]):(a.y*ThreatSize+a.x).CompareTo(b.y*ThreatSize+b.x));
  if(s==KaitSkill.ReynardRuneStep){var required=target-Vector2Int.one;if(required.x<0||required.y<0||required.x>=ThreatSize||required.y>=ThreatSize||!Reynard.sigils[required.x,required.y]){error="目标没有法印";return false;}payment.sigils.Add(required);candidates.Remove(required);}
  foreach(var p in candidates)if(payment.sigils.Count<cost)payment.sigils.Add(p);
  payment.tails=Mathf.Max(0,cost-payment.sigils.Count);
  if(payment.tails>Reynard.tails){error=$"材料不足：需要{cost}";return false;}return true;
 }
 public string ReynardCastFailure(KaitSkill s)
 {
  var d=ReynardCatalog.Get(s);if(ended||d==null||!skills.Contains(s))return "技能未装备";
  if(s==KaitSkill.ReynardTailNova)return Reynard.tails>=2?null:"需要2条狐尾";
  return ReynardMaterialCount>=d.spellLevel?null:$"材料不足：需要{d.spellLevel}";
 }
 private Vector2Int ReynardFieldCell(KaitSkill s)=>s==KaitSkill.ReynardFog?Reynard.fogCell:s==KaitSkill.ReynardWeb?Reynard.webCell:s==KaitSkill.ReynardStoneWall?Reynard.stoneWallCell:new Vector2Int(-1,-1);
 private void SetReynardFieldCell(KaitSkill s,Vector2Int p){if(s==KaitSkill.ReynardFog)Reynard.fogCell=p;else if(s==KaitSkill.ReynardWeb)Reynard.webCell=p;else if(s==KaitSkill.ReynardStoneWall)Reynard.stoneWallCell=p;}
 public bool IsLegalReynardCell(KaitSkill s,Vector2Int p)
 {
  var d=ReynardCatalog.Get(s);if(d==null)return false;
  if(s==KaitSkill.ReynardLightningBolt||s==KaitSkill.ReynardScorchingRay)return Manhattan(katePos,p)==1;
  if(s==KaitSkill.ReynardThunderwave||s==KaitSkill.ReynardCounterspell||s==KaitSkill.ReynardTailNova)return p==katePos;
  if(!Inside(p))return false;
  if(s==KaitSkill.ReynardRuneStep){var q=p-Vector2Int.one;return q.x>=0&&q.y>=0&&q.x<ThreatSize&&q.y<ThreatSize&&Reynard.sigils[q.x,q.y]&&!IsHardBlocked(p)&&EnemyAt(p)==null&&p!=katePos;}
  int range=3+(HasPassive(KaitPassive.ReynardDistantSpell)?2:0);if(Manhattan(katePos,p)>range)return false;
  if(s==KaitSkill.ReynardMistyStep)return p!=katePos&&!IsHardBlocked(p)&&EnemyAt(p)==null;
  if(s==KaitSkill.ReynardChainLightning)return EnemyAt(p)!=null;
  if(s==KaitSkill.ReynardStoneWall)return !IsHardBlocked(p)&&EnemyAt(p)==null&&p!=katePos&&SpawnAt(p)==null;
  return !IsHardBlocked(p)||ReynardFieldCell(s)==p;
 }
 private bool CastReynard(KaitSkill s,Vector2Int p,out string message)
 {
  message=ReynardCastFailure(s);if(message!=null)return false;if(!IsLegalReynardCell(s,p)){message="目标不合法";return false;}
  if(ReynardCatalog.IsField(s)&&ReynardFieldCell(s)==p){SetReynardFieldCell(s,new Vector2Int(-1,-1));message="已撤销："+ReynardCatalog.Get(s).nameZh;RecordReplay("cellskill",(int)s,p.x,p.y);return true;}
  if(!TryPlanReynardPayment(s,p,out var payment,out message))return false;
  var r=BeginReynardAction();r.reynardCast=s;lastSkillResult=r;var d=ReynardCatalog.Get(s);Reynard.casts++;
  foreach(var q in payment.sigils){Reynard.sigils[q.x,q.y]=false;Reynard.sigilCreated[q.x,q.y]=0;r.reynardEvents.Add(new ReynardVisualEvent{kind="SigilSpent",from=q,to=MapThreatToBattle(q),amount=1});}
  Reynard.tails-=payment.tails;for(int i=0;i<payment.tails;i++)r.reynardEvents.Add(new ReynardVisualEvent{kind="TailSpent",from=katePos,to=katePos,amount=1});
  if(ReynardCatalog.IsField(s)){SetReynardFieldCell(s,p);if(s==KaitSkill.ReynardStoneWall)r.reynardEvents.Add(new ReynardVisualEvent{kind="StoneWall",from=p,to=p,amount=1});}else ResolveReynardInstant(s,p,r);
  ResolveReynardPaymentEffects(payment,r);
  if(HasPassive(KaitPassive.ReynardMirrorWard))Reynard.mirrorWard=true;
  if(ReynardCatalog.IsInstant(s)&&HasPassive(KaitPassive.ReynardQuickenedTome))ResolveReynardShot(Reynard.hasLastDirection?Reynard.lastDirection:KaitDirection.Up,r);
  FinishReynardAction(r);message="已施放："+d.nameZh;RecordReplay("cellskill",(int)s,p.x,p.y);return true;
 }
 private void ResolveReynardPaymentEffects(ReynardPayment payment,KaitTurnResult r)
 {
  foreach(var q in payment.sigils)
  {
   if(HasPassive(KaitPassive.ReynardRuneDetonation)){var cell=MapThreatToBattle(q);var target=EnemyAt(cell);if(target!=null){r.reynardEvents.Add(new ReynardVisualEvent{kind="RuneDetonation",from=cell,to=cell,amount=1});ReynardHit(target,1,ReynardDamageKind.SigilEffect,r);}}
   if(HasPassive(KaitPassive.ReynardCircleResonance))foreach(var linked in ReynardConnectedSigils(payment.snapshot,q)){var cell=MapThreatToBattle(linked);var target=EnemyAt(cell);if(target!=null){r.reynardEvents.Add(new ReynardVisualEvent{kind="CircleResonance",from=MapThreatToBattle(q),to=cell,amount=1});ReynardHit(target,1,ReynardDamageKind.SigilEffect,r);}}
  }
  for(int i=0;i<payment.tails;i++)
  {
   if(HasPassive(KaitPassive.ReynardFoxfireRite))foreach(var e in new List<KaitEnemy>(enemies))if(Manhattan(e.pos,katePos)==1&&ReynardHit(e,1,ReynardDamageKind.TailCounter,r)>0)r.reynardEvents.Add(new ReynardVisualEvent{kind="FoxflameRite",from=katePos,to=e.pos,amount=1});
   if(HasPassive(KaitPassive.ReynardTailChant)){int before=Reynard.directionSpells;ResolveReynardShot(Reynard.hasLastDirection?Reynard.lastDirection:KaitDirection.Up,r);if(Reynard.directionSpells>before)r.reynardEvents.Add(new ReynardVisualEvent{kind="TailwindChant",from=katePos,to=katePos,amount=1});}
  }
 }
 private List<Vector2Int> ReynardConnectedSigils(bool[,] snapshot,Vector2Int start)
 {
  var found=new List<Vector2Int>();if(snapshot==null||!snapshot[start.x,start.y])return found;var queue=new Queue<Vector2Int>();var seen=new HashSet<Vector2Int>();queue.Enqueue(start);seen.Add(start);
  while(queue.Count>0){var p=queue.Dequeue();found.Add(p);foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}){var n=p+d;if(n.x>=0&&n.y>=0&&n.x<ThreatSize&&n.y<ThreatSize&&snapshot[n.x,n.y]&&seen.Add(n))queue.Enqueue(n);}}
  return found;
 }
}
