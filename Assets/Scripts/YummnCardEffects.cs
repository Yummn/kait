using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    private bool Planned(KaitTurnResult r,string id)=>r.yummnAction.plannedSkills.Contains("yummn."+id);
    private void ResolveYummnPlayerAction(KaitTurnResult r)
    {
        var a=r.yummnAction;var d=Delta(a.direction);
        switch(a.actionOverride)
        {
            case "Ice":
                Yummn.icePillar=a.targetCell;YummnTerrainEvent(r,"Ice",a.targetCell);return;
            case "Darkness":
                Yummn.darkness=a.targetCell;YummnTerrainEvent(r,"Darkness",a.targetCell);return;
            case "Shadow":
                MoveYummnPlayer(a.targetCell,YummnMoveCause.Teleport,r);YummnTerrainEvent(r,"ShadowStep",a.targetCell);return;
            case "Water":
                a.didAttack=true;ChargeYummnAttackTenth(r);var target=FirstYummnRayEnemy(a.direction);if(target==null)return;
                YummnHit(target,1,d,YummnDamageCause.WaterWhip,r);
                if(!ended&&target.life!=KaitEnemyLife.Dead)ForceYummnEnemy(target,-d,2,r);
                return;
            case "Winter":
                a.didAttack=true;ChargeYummnAttackTenth(r);r.yummnFrost=true;
                for(int i=1;i<=2&&!ended;i++)
                {
                    var p=katePos+d*i;if(IsHardBlocked(p))break;var e=EnemyAt(p);if(e==null)continue;
                    if(YummnHit(e,1,d,YummnDamageCause.WinterBreath,r)>0&&e.life!=KaitEnemyLife.Dead)
                    {e.yummnFrozen=true;SyncYummnControl(e);YummnStatusEvent(r,e,"Frozen");}
                }
                return;
        }
        int max=a.phaseAtStart==YummnPhase.Burst?BattleSize:1;
        for(int i=0;i<max&&!ended;i++)
        {
            var next=katePos+d;
            if(IsHardBlocked(next)){r.stoppedByWall=true;break;}
            var enemy=EnemyAt(next);
            if(enemy!=null){ResolveYummnPunch(enemy,r);break;}
            if(Yummn.rules.Is082&&i>=a.voluntaryCells)break;
            MoveYummnPlayer(next,YummnMoveCause.Player,r);
        }
    }
    private void MoveYummnPlayer(Vector2Int to,YummnMoveCause cause,KaitTurnResult r)
    {
        var from=katePos;katePos=to;r.katePath.Add(to);r.pathMomentum.Add(1);
        if(from!=to)r.yummnAction.playerMoved=true;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Move,actionId=Yummn.actionId,from=from,to=to,moveCause=cause});
    }
    private void YummnTerrainEvent(KaitTurnResult r,string status,Vector2Int cell)=>r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Terrain,actionId=Yummn.actionId,status=status,to=cell});
    private void YummnStatusEvent(KaitTurnResult r,KaitEnemy e,string status)=>r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,actionId=Yummn.actionId,status=status,targetId=e.id,to=e.pos,direction=Yummn.palmDirection});
    private void SyncYummnControl(KaitEnemy e)=>e.frozenActions=e.yummnFrozen||e.yummnStunned?1:0;
    private void ResolveYummnPunch(KaitEnemy e,KaitTurnResult r)
    {
        var a=r.yummnAction;var d=Delta(a.direction);var original=e.pos;
        a.isPunchAction=true;a.preHitTravelCells=r.katePath.Count;a.stationaryPunch=a.preHitTravelCells==0;
        a.didAttack=true;a.mainEnemyId=e.id;r.blockedEnemyCell=original;r.damagedEnemyId=e.id;
        bool oldFrozen=e.yummnFrozen,primaryKill=false,actualHit=false,marked=false;
        bool oldPalm=Yummn.palmEnemyId==e.id;var oldDirection=Yummn.palmDirection;
        bool nearby=(e.pos-katePos).sqrMagnitude==1;
        r.yummnFlurry=Planned(r,"M01");int punches=r.yummnFlurry?3:1;
        for(int i=0;i<punches&&e.life!=KaitEnemyLife.Dead&&!ended;i++)
        {
            ChargeYummnAttackTenth(r);
            int damage=YummnHit(e,Yummn.profile.fistDamage,d,YummnDamageCause.Punch,r);r.yummnPunches++;r.damageDealt+=damage;
            if(damage==0)continue;
            actualHit=true;if(e.life==KaitEnemyLife.Dead)primaryKill=true;
            if(!marked&&HasPassive(KaitPassive.QuiveringPalm))
            {
                marked=true;
                if(oldPalm&&oldDirection!=d)
                {
                    Yummn.palmEnemyId=-1;YummnTrigger("O06",r);
                    if(e.life!=KaitEnemyLife.Dead&&!ended)YummnHit(e,2,d,YummnDamageCause.Quivering,r);
                    YummnStatusEvent(r,e,"PalmDetonate");
                }
                else if(e.life!=KaitEnemyLife.Dead){Yummn.palmEnemyId=e.id;Yummn.palmDirection=d;YummnStatusEvent(r,e,"PalmMark");}
            }
        }
        r.playerAttackBlocked=!actualHit;r.enemyHpAfter=e.hp;
        if(!ended&&actualHit&&HasPassive(KaitPassive.FireSnake))
        {
            var rear=original+d;if(!IsHardBlocked(rear))
            {var victim=EnemyAt(rear);if(victim!=null){YummnHit(victim,1,d,YummnDamageCause.FireSnake,r);YummnTrigger("E05",r);}}
        }
        if(!ended&&actualHit&&oldFrozen&&HasPassive(KaitPassive.ShatteringPalm))
        {
            e.yummnFrozen=false;SyncYummnControl(e);YummnStatusEvent(r,e,"Shatter");YummnTrigger("E06",r);
            foreach(var offset in YummnDirections)
            {if(ended)break;var victim=EnemyAt(original+offset);if(victim!=null)YummnHit(victim,1,offset,YummnDamageCause.Shatter,r);}
        }
        if(!ended&&actualHit&&e.life!=KaitEnemyLife.Dead)
        {
            nonLethalHits++;
            if(Planned(r,"M03")){e.yummnStunned=true;SyncYummnControl(e);r.yummnStun=true;YummnStatusEvent(r,e,"Stunned");}
            int distance=Planned(r,"E02")?BattleSize:Planned(r,"O01")||r.yummnFlurry&&HasPassive(KaitPassive.OpenHand)?1:0;
            if(distance>0&&ForceYummnEnemy(e,d,distance,r))
            {
                if(r.yummnFlurry&&HasPassive(KaitPassive.OpenHand))YummnTrigger("O02",r);
                if(nearby&&HasPassive(KaitPassive.FollowThrough)&&YummnEmpty(original)){MoveYummnPlayer(original,Yummn.rules.Is082?YummnMoveCause.PushFollow:YummnMoveCause.Player,r);YummnTrigger("O03",r);}
            }
        }
        // A secondary detonation kill is credited but must not teleport the player.
        if(primaryKill&&EnemyAt(original)==null&&!IsHardBlocked(original)){directKills++;MoveYummnPlayer(original,Yummn.rules.Is082?YummnMoveCause.KillFollow:YummnMoveCause.Player,r);}
    }
    private int YummnHit(KaitEnemy e,int damage,Vector2Int direction,YummnDamageCause cause,KaitTurnResult r)
    {
        if(e==null||e.life==KaitEnemyLife.Dead||ended)return 0;
        bool bypass=(cause==YummnDamageCause.Punch||cause==YummnDamageCause.Quivering)&&r.yummnAction.startedInShadow&&HasPassive(KaitPassive.ShadowAssault);
        bool blocked=e.type==KaitEnemyType.ShieldKnight&&e.facing==-direction&&!bypass;
        int dealt=blocked?0:Mathf.Min(e.hp,damage);e.hp-=dealt;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Hit,actionId=Yummn.actionId,targetId=e.id,from=katePos,to=e.pos,direction=direction,damageCause=cause,amount=dealt,hpAfter=e.hp,blocked=blocked});
        if(dealt>0)
        {
            var hits=Yummn.metrics.enemyHits;hits[e.id]=hits.TryGetValue(e.id,out var h)?h+1:1;
            var sources=Yummn.metrics.damageSources;string key=cause.ToString();sources[key]=sources.TryGetValue(key,out var n)?n+dealt:dealt;
            if(bypass&&cause==YummnDamageCause.Punch)YummnTrigger("S05",r);
        }
        if(e.hp>0)return dealt;
        e.life=KaitEnemyLife.Dead;e.intent=new KaitIntent{origin=e.pos};
        if(Yummn.palmEnemyId==e.id)Yummn.palmEnemyId=-1;
        if(Yummn.rewardedDeaths.Add(e.id))
        {
            kills++;r.killedEnemyIds.Add(e.id);r.playerKilledEnemyIds.Add(e.id);r.killedEnemyCells.Add(e.pos);r.yummnAction.killIds.Add(e.id);
            if(!Yummn.rules.Is082)Yummn.metrics.killKi+=GainYummnKi(Yummn.profile.killKi,r,"Kill");
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Kill,actionId=Yummn.actionId,targetId=e.id,to=e.pos,damageCause=cause});
        }
        else Yummn.metrics.duplicatesPrevented++;
        if(e.id==bossEnemyId){Yummn.metrics.bossKilledAction=Yummn.actionId;End("Victory: Shield Knight",true);}
        return dealt;
    }
    private bool ForceYummnEnemy(KaitEnemy e,Vector2Int direction,int distance,KaitTurnResult r)
    {
        if(e.type==KaitEnemyType.ShieldKnight)return false;
        Vector2Int origin=e.pos,to=origin;
        for(int i=0;i<distance;i++){var next=to+direction;if(IsHardBlocked(next)||next==katePos||EnemyAt(next)!=null)break;to=next;}
        if(to==origin)return false;
        e.pos=to;pushCount++;r.pushed=true;r.pushFrom=origin;r.pushTo=to;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Move,actionId=Yummn.actionId,targetId=e.id,from=origin,to=to,moveCause=YummnMoveCause.Forced});
        // Locked melee/area cells do not follow their victim. Arrows retain direction.
        if(Yummn.rules.Is082)TranslateYummn082Intent(e,origin);
        else if(e.type==KaitEnemyType.Archer&&e.rangedState==KaitRangedState.Aim)e.intent=BuildYummnArrow(e.pos,e.intent.direction);
        ResolveMomentumResonance(origin,direction,r);return true;
    }
}
