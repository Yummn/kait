using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    private void ResolveYummnEnemyPhase(KaitTurnResult r)
    {
        Yummn.metrics.enemyPhases++;Yummn.missileSpent=false;
        var actors=enemies.FindAll(e=>e.life!=KaitEnemyLife.Dead);actors.Sort((a,b)=>a.id.CompareTo(b.id));
        Yummn.metrics.enemyActors+=actors.Count;
        foreach(var e in actors)
        {
            if(ended)break;if(e.life==KaitEnemyLife.Dead)continue;
            if(e.life==KaitEnemyLife.Preparing){e.life=KaitEnemyLife.Active;continue;}
            if(e.yummnFrozen||e.yummnStunned||e.frozenActions>0)
            {
                e.yummnFrozen=e.yummnStunned=false;e.frozenActions=0;e.intent=new KaitIntent{origin=e.pos};e.rangedState=KaitRangedState.Ready;YummnStatusEvent(r,e,"ControlConsumed");continue;
            }
            if(e.intent.type!=KaitIntentType.None)
            {
                var intent=IsYummnLineBoss(e)?BuildYummnBossLine(e.pos,e.intent.direction):e.type==KaitEnemyType.Archer?BuildYummnArrow(e.pos,e.intent.direction):e.intent;
                var action=new KaitEnemyAction{enemyId=e.id,type=intent.type,from=e.pos,to=intent.target,damage=intent.damage};action.affectedCells.AddRange(intent.affectedCells);
                int attackId=++Yummn.nextAttackEventId;
                r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.EnemyAttack,actionId=Yummn.actionId,sourceId=e.id,from=e.pos,to=intent.target,attackEventId=attackId,affectedCells=new List<Vector2Int>(intent.affectedCells)});
                if(Yummn.rules.Is082)HitYummn082Afterimages(e,intent,attackId,r);
                if(intent.affectedCells.Contains(katePos))
                {
                    action.hitKate=true;action.guarded=PreventYummnDamage(e,intent,r);
                    int damage=action.guarded?0:DamageKate(intent.damage,r);
                    if(damage>0){string key="Received."+intent.type;var counts=Yummn.metrics.damageSources;counts[key]=counts.TryGetValue(key,out var old)?old+damage:damage;}
                    r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Hit,actionId=Yummn.actionId,sourceId=e.id,targetId=-1,from=e.pos,to=katePos,amount=damage,hpAfter=kateHp,blocked=action.guarded,damageCause=intent.type==KaitIntentType.LineShot?YummnDamageCause.Arrow:intent.type==KaitIntentType.Melee?YummnDamageCause.Melee:YummnDamageCause.Area});
                    if(kateHp<=0)End(Yummn081?"PlayerDead":"Kate Defeated",false);
                }
                r.enemyActions.Add(action);e.intent=new KaitIntent{origin=e.pos};e.rangedState=KaitRangedState.Ready;continue;
            }
            bool ranged=IsTwoPhaseRanged(e)||IsYummnLineBoss(e),adjacent=(e.pos-katePos).sqrMagnitude==1;
            if(ranged||adjacent)
            {
                // A cloak consumes this legal prepare action, including a Boss turn.
                if(Yummn.cloak){Yummn.cloak=false;YummnStatusEvent(r,e,"AimDenied");YummnTrigger("S03",r);continue;}
                if(e.type==KaitEnemyType.ShieldKnight)e.facing=DirectionToward(e.pos,katePos);
                e.intent=IsYummnLineBoss(e)?BuildYummnBossLine(e.pos,e.facing):e.type==KaitEnemyType.Archer?BuildYummnArrow(e.pos,DirectionToward(e.pos,katePos)):e.type==KaitEnemyType.Warlock?BuildCrossIntent(e.pos,katePos):BuildIntentToward(e,katePos);
                if(ranged)e.rangedState=KaitRangedState.Aim;
                r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Aim,actionId=Yummn.actionId,sourceId=e.id,from=e.pos,to=katePos});continue;
            }
            if(e.type!=KaitEnemyType.Swordsman)continue;
            var to=YummnSwordsmanStep(e);if(to==e.pos)continue;
            var from=e.pos;e.pos=to;
            r.enemyActions.Add(new KaitEnemyAction{enemyId=e.id,type=KaitIntentType.Move,from=from,to=to});
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Move,actionId=Yummn.actionId,targetId=e.id,from=from,to=to,moveCause=YummnMoveCause.AI});
            if((from-katePos).sqrMagnitude!=1&&(to-katePos).sqrMagnitude==1&&HasPassive(KaitPassive.Opportunist))
            {YummnHit(e,1,to-katePos,YummnDamageCause.Counter,r);YummnTrigger("S06",r);}
        }
        Yummn.defense=Yummn.tranquility=false;
        if(Yummn.darkness.x>=0){YummnTerrainEvent(r,"DarknessExpired",Yummn.darkness);Yummn.darkness=YummnRun.NoCell;}
    }
    private bool IsYummnLineBoss(KaitEnemy e)=>IsYummn&&Yummn.rules.BossLine&&e.type==KaitEnemyType.ShieldKnight;
    private KaitIntent BuildYummnBossLine(Vector2Int origin,Vector2Int direction)
    {
        // A directional sword sweep, not an arrow. Keep melee defenses and
        // sword animation/audio; warn every forward cell, including beyond Kait.
        var intent=new KaitIntent{type=KaitIntentType.Melee,origin=origin,target=origin,direction=direction,damage=1};
        if(direction==Vector2Int.zero)return intent;
        for(int i=1;i<BattleSize;i++)
        {
            var p=origin+direction*i;if(IsHardBlocked(p)||p==Yummn.darkness)break;
            intent.affectedCells.Add(p);intent.target=p;
        }
        return intent;
    }
    private KaitIntent BuildYummnArrow(Vector2Int origin,Vector2Int direction)
    {
        var intent=new KaitIntent{type=KaitIntentType.LineShot,origin=origin,target=origin,direction=direction,damage=1};
        for(int i=1;i<=config.archerRange;i++)
        {
            var p=origin+direction*i;if(IsHardBlocked(p)||p==Yummn.darkness)break;
            intent.affectedCells.Add(p);intent.target=p;if(p==katePos)break;
        }
        return intent;
    }
    private Vector2Int YummnSwordsmanStep(KaitEnemy e)
    {
        var queue=new Queue<Vector2Int>();var first=new Dictionary<Vector2Int,Vector2Int>();queue.Enqueue(e.pos);first[e.pos]=e.pos;
        while(queue.Count>0)
        {
            var p=queue.Dequeue();
            foreach(var d in YummnDirections)
            {
                var next=p+d;if(IsHardBlocked(next)||first.ContainsKey(next)||EnemyAt(next)!=null)continue;
                var step=p==e.pos?next:first[p];if(next==katePos)return step;
                first[next]=step;queue.Enqueue(next);
            }
        }
        return e.pos;
    }
    private bool PreventYummnDamage(KaitEnemy e,KaitIntent intent,KaitTurnResult r)
    {
        if(config.playerInvincible||intent.damage<=0)return true;
        if(intent.type==KaitIntentType.LineShot&&!Yummn.missileSpent&&HasPassive(KaitPassive.DeflectMissiles))
        {Yummn.missileSpent=true;r.yummnDeflect=true;YummnTrigger("M04",r);return true;}
        if(intent.type==KaitIntentType.Melee&&Yummn.tranquility)
        {Yummn.tranquility=false;r.yummnGuard=true;YummnTrigger("O04",r);return true;}
        if(Yummn.defense){Yummn.defense=false;r.yummnGuard=true;return true;}return false;
    }
    private void QueueYummnRift(KaitMergeEvent merge,KaitTurnResult r)
    {
        if(Yummn081&&Yummn.rules.SpawnFromEight&&merge.resultValue<8)return;
        var target=MapThreatToBattle(merge.threatCell);
        if(!Inside(target)||walls[target.x,target.y]){merge.spawnSuppressed=true;wallSuppressedSpawns++;spawnSuppressedCount++;return;}
        int tier=Mathf.Clamp((int)Mathf.Log(merge.resultValue,2f)-(Yummn081&&Yummn.rules.SpawnFromEight?2:1),1,5);
        var existing=spawns.Find(s=>s.targetCell==target);
        if(existing!=null){existing.tier=Mathf.Max(existing.tier,tier);return;}
        spawns.Add(new KaitSpawnRequest{tier=tier,sourceThreatCell=merge.threatCell,targetCell=target,createdTurn=turn,state=KaitSpawnState.Preview});
    }
    private void ResolveYummnRifts(KaitTurnResult r)
    {
        Yummn.metrics.spawnChecks++;
        foreach(var request in new List<KaitSpawnRequest>(spawns))
        {
            if(ended)break;
            if(request.targetCell==PendingBossCell||!YummnEmpty(request.targetCell)){Yummn.metrics.riftBlockedChecks++;continue;}
            var type=EnemyTypeForSpawn(request);int hp=MaxHpFor(type);
            var e=new KaitEnemy{id=nextEnemyId++,type=type,pos=request.targetCell,hp=hp,maxHp=hp,life=KaitEnemyLife.Active,intent=new KaitIntent{origin=request.targetCell}};
            enemies.Add(e);spawns.Remove(request);r.spawnedEnemyCells.Add(e.pos);
            spawnHeatmap[request.sourceThreatCell.x,request.sourceThreatCell.y]++;
            if(IsInternalThreatCell(request.sourceThreatCell))internalSpawnCount++;
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Spawn,actionId=Yummn.actionId,targetId=e.id,to=e.pos});
        }
    }
}
