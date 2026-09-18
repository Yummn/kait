using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    private bool Planned(KaitTurnResult r,string id)=>r.yummnAction.plannedSkills.Contains("yummn."+id);
    private void ResolveYummnPlayerAction(KaitTurnResult r)
    {
        var a=r.yummnAction;var d=Delta(a.direction);
        if(a.actionOverride=="Sonic"||a.actionOverride=="AirRay"||a.actionOverride=="Water"||a.actionOverride=="Winter")a.voluntaryAttack=true;
        switch(a.actionOverride)
        {
            case "Thunder":
                YummnTerrainEvent(r,"ThunderWave",katePos);
                var pushes=new List<KeyValuePair<KaitEnemy,Vector2Int>>();
                foreach(var offset in YummnDirections){var enemy=EnemyAt(katePos+offset);if(enemy!=null)pushes.Add(new KeyValuePair<KaitEnemy,Vector2Int>(enemy,offset));}
                foreach(var push in pushes)ForceYummnEnemy(push.Key,push.Value,1,r);return;
            case "Sonic":
                a.didAttack=true;YummnTerrainEvent(r,"SonicBurst",a.targetCell);
                if((Yummn.icePillar-a.targetCell).sqrMagnitude<=1){YummnTerrainEvent(r,"IceExpired",Yummn.icePillar);Yummn.icePillar=YummnRun.NoCell;}
                YummnDamagePacket(enemies.FindAll(enemy=>enemy.life!=KaitEnemyLife.Dead&&(enemy.pos-a.targetCell).sqrMagnitude<=1),1,Vector2Int.zero,YummnDamageCause.Sonic,r);
                return;
            case "Mirror":YummnTerrainEvent(r,"MirrorCreate",a.targetCell);CreateYummnSpellEcho(a.targetCell,r);return;
            case "CommandAct":YummnTerrainEvent(r,"CommandAct",a.targetCell);ResolveYummnCommandActor(EnemyAt(a.targetCell),r);return;
            case "MageHand":r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="MageHandCast",to=a.targetCell,actionId=Yummn.actionId});threat[a.targetCell.x,a.targetCell.y]=threatTwoBirth[a.targetCell.x,a.targetCell.y]=0;return;
            case "Precise":MoveYummnPlayer(a.targetCell,YummnMoveCause.Player,r);return;
            case "Phantom":MoveYummnPlayer(a.targetCell,YummnMoveCause.Teleport,r);return;
            case "Echo":
                var echo=Yummn.afterimages.Find(m=>m.alive&&m.cell==a.targetCell);
                if(echo!=null){echo.alive=false;r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.AfterimageCleared,markerId=echo.id,to=echo.cell});}
                MoveYummnPlayer(a.targetCell,YummnMoveCause.Teleport,r);YummnTerrainEvent(r,"ShadowStep",a.targetCell);return;
            case "Heal":
                int heal=Mathf.Min(1,KateMaxHp-kateHp);kateHp+=heal;Yummn.metrics.heals+=heal;RepoolStatus(r,"Heal",katePos,heal);return;
            case "Decoy":Yummn.decoy=a.targetCell;YummnTerrainEvent(r,"Decoy",a.targetCell);return;
            case "AirRay":
                a.didAttack=true;var air=FirstYummnRayEnemy(a.direction);
                if(air!=null&&YummnHit(air,1,d,YummnDamageCause.WaterWhip,r)>0&&air.life!=KaitEnemyLife.Dead)ForceYummnEnemy(air,d,BattleSize,r);
                return;
            case "Ice":
                Yummn.icePillar=a.targetCell;YummnTerrainEvent(r,"Ice",a.targetCell);return;
            case "Darkness":
                Yummn.darkness=a.targetCell;YummnTerrainEvent(r,"Darkness",a.targetCell);return;
            case "Shadow":
                MoveYummnPlayer(a.targetCell,YummnMoveCause.Teleport,r);YummnTerrainEvent(r,"ShadowStep",a.targetCell);return;
            case "Water":
                a.didAttack=true;var target=FirstYummnRayEnemy(a.direction);if(target==null)return;
                YummnHit(target,1,d,YummnDamageCause.WaterWhip,r);
                if(!ended&&target.life!=KaitEnemyLife.Dead)ForceYummnEnemy(target,-d,2,r);
                return;
            case "Winter":
                a.didAttack=true;r.yummnFrost=true;
                var winterTargets=new List<KaitEnemy>();
                for(int i=1;i<=4;i++)
                {
                    var p=katePos+d*i;if(IsHardBlocked(p))break;var e=EnemyAt(p);if(e==null)continue;
                    winterTargets.Add(e);
                }
                YummnDamagePacket(winterTargets,1,d,YummnDamageCause.WinterBreath,r,true);
                return;
        }
        if(HasPassive(KaitPassive.DistantPull)&&EnemyAt(katePos+d)==null)
        {
            var distant=FirstYummnRayEnemy(a.direction);
            if(distant!=null){a.didAttack=a.voluntaryAttack=true;ChargeYummnAttackTenth(r);if(YummnHit(distant,1,d,YummnDamageCause.WaterWhip,r)>0&&distant.life!=KaitEnemyLife.Dead)ForceYummnEnemy(distant,-d,BattleSize,r);return;}
        }
        int max=a.phaseAtStart==YummnPhase.Burst?BattleSize:1;
        for(int i=0;i<max&&!ended;i++)
        {
            var next=katePos+d;
            if(IsHardBlocked(next)){r.stoppedByWall=true;break;}
            var enemy=EnemyAt(next);
            if(enemy!=null){ResolveYummnPunch(enemy,r);break;}
            if(Yummn.rules.Is082&&!CommitYummnSlideStep(r))break;
            MoveYummnPlayer(next,YummnMoveCause.Player,r);
        }
    }
    private void MoveYummnPlayer(Vector2Int to,YummnMoveCause cause,KaitTurnResult r)
    {
        var from=katePos;
        bool continuedSlide=cause==YummnMoveCause.Player&&r.yummnEvents.Count>0&&r.yummnEvents[r.yummnEvents.Count-1].kind==YummnEventKind.Move&&r.yummnEvents[r.yummnEvents.Count-1].targetId<0&&r.yummnEvents[r.yummnEvents.Count-1].moveCause==cause;
        if(!continuedSlide)LeaveYummnMovementEcho(from,to,r);
        katePos=to;r.katePath.Add(to);r.pathMomentum.Add(1);
        if(from!=to)r.yummnAction.playerMoved=true;
        if(from!=to&&cause==YummnMoveCause.Player)r.yummnAction.voluntaryMoved=true;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Move,actionId=Yummn.actionId,from=from,to=to,moveCause=cause});
        foreach(var enemy in new List<KaitEnemy>(enemies))ResolveYummnRangeExit(enemy,enemy.pos,r,from);
        ResolveYummnRangeEntries(r);
    }
    private void YummnTerrainEvent(KaitTurnResult r,string status,Vector2Int cell)=>r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Terrain,actionId=Yummn.actionId,status=status,to=cell});
    private void YummnStatusEvent(KaitTurnResult r,KaitEnemy e,string status)=>r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,actionId=Yummn.actionId,status=status,targetId=e.id,to=e.pos,direction=Yummn.palmDirection});
    private void SyncYummnControl(KaitEnemy e)=>e.frozenActions=e.yummnFrozen||e.yummnStunned?1:0;
    private void ResolveYummnPunch(KaitEnemy e,KaitTurnResult r,bool reaction=false)
        =>ResolveYummnMartialAttack(e,r,reaction,reaction?YummnAttackOrigin.Opportunist:YummnAttackOrigin.Voluntary);
    private void ResolveYummnMartialAttack(KaitEnemy e,KaitTurnResult r,bool reaction,YummnAttackOrigin origin)
    {
        if(e==null||e.life==KaitEnemyLife.Dead||ended)return;
        using(var scope=new YummnEventScope(this,r,YummnAttackFamily.Martial,origin,e.id)) {
        yummnPunchDepth++;
        try {
        var a=r.yummnAction;var d=reaction?e.pos-katePos:Delta(a.direction);
        a.isPunchAction=true;a.preHitTravelCells=r.katePath.Count;a.stationaryPunch=a.preHitTravelCells==0;
        a.didAttack=true;a.mainEnemyId=e.id;r.blockedEnemyCell=e.pos;r.damagedEnemyId=e.id;
        if(!reaction)a.voluntaryAttack=true;
        bool marked=false,actualHit=false;
        bool frozenAtStart=e.yummnFrozen;
        bool doublePunch=HasPassive(KaitPassive.TwinPunch)&&SpendRepoolKi(2,r);
        if(doublePunch){a.attackCostTenths+=20;Yummn.attackSpentTenths+=20;}
        r.yummnFlurry|=doublePunch;int punches=doublePunch?2:1;
        for(int i=0;i<punches&&e.life!=KaitEnemyLife.Dead&&!ended;i++)
        {
            if((e.pos-katePos).sqrMagnitude!=1)break;
            var original=e.pos;bool frozen=e.yummnFrozen;
            if(!doublePunch)ChargeYummnAttackTenth(r);
            int damage=YummnHit(e,Yummn.profile.fistDamage,d,reaction?YummnDamageCause.Counter:YummnDamageCause.Punch,r);
            r.yummnPunches++;r.damageDealt+=damage;actualHit|=damage>0;
            if(!ended&&HasPassive(KaitPassive.FireSnake))
            {var rear=EnemyAt(original+d);if(rear!=null){YummnHit(rear,1,d,YummnDamageCause.FireSnake,r);YummnTrigger("E05",r);}}
            if(damage<=0)continue;
            if(!marked&&HasPassive(KaitPassive.QuiveringPalm))
            {
                marked=true;
                ResolveYummnPalmHit(e,d,r);
            }
            if(e.life==KaitEnemyLife.Dead)
            {directKills++;if(!IsHardBlocked(original)&&EnemyAt(original)==null)MoveYummnPlayer(original,YummnMoveCause.KillFollow,r);ResolveYummnSweep(r);break;}
            nonLethalHits++;
            if(HasPassive(KaitPassive.StunStrike)&&!e.yummnStunned&&SpendRepoolKi(1,r))
            {e.yummnStunned=true;e.yummnStunThroughPhase=Yummn.metrics.enemyPhases+(yummnInEnemyPhase?0:1);e.intent=new KaitIntent{origin=e.pos};e.rangedState=KaitRangedState.Ready;SyncYummnControl(e);r.yummnStun=true;YummnStatusEvent(r,e,"Stunned");}
            int distance=HasPassive(KaitPassive.EndlessPush)?BattleSize:HasPassive(KaitPassive.OpenHand)?1:0;
            if(distance>0)ForceYummnEnemy(e,d,distance,r);
            if(HasPassive(KaitPassive.FreezePunch))FreezeYummnEnemy(e,r);
        }
        r.playerAttackBlocked=!actualHit;r.enemyHpAfter=e.hp;
        } finally {yummnPunchDepth--;}
        ResolveYummnRangeEntries(r);
        }
    }
    private void ResolveYummnPalmHit(KaitEnemy e,Vector2Int direction,KaitTurnResult r)
    {
        if(!HasPassive(KaitPassive.QuiveringPalm))return;
        if(Yummn.TryPalm(e.id,out var previous)&&previous!=direction)
        {
            Yummn.RemovePalm(e.id);YummnTrigger("O06",r);
            if(e.life!=KaitEnemyLife.Dead&&!ended)YummnHit(e,2,direction,YummnDamageCause.Quivering,r);
            YummnStatusEvent(r,e,"PalmDetonate");
        }
        else if(e.life!=KaitEnemyLife.Dead){Yummn.MarkPalm(e.id,direction);YummnStatusEvent(r,e,"PalmMark");}
    }
    private int YummnHit(KaitEnemy e,int damage,Vector2Int direction,YummnDamageCause cause,KaitTurnResult r)
    {
        if(e==null||e.life==KaitEnemyLife.Dead||ended&&yummnPacketDepth==0)return 0;
        if(e.yummnFrozen)
        {
            if(cause==YummnDamageCause.Shatter&&yummnFreezeInstances.ContainsKey(e.id))return 0;
            e.yummnFrozen=false;SyncYummnControl(e);
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Hit,parentEventId=yummnParentEvent,actionId=Yummn.actionId,targetId=e.id,from=katePos,to=e.pos,direction=direction,damageCause=cause,amount=0,hpAfter=e.hp,blocked=true,status="IceGuard"});
            YummnStatusEvent(r,e,"IceGuard");
            string freezeKey=e.id+":"+(yummnFreezeInstances.TryGetValue(e.id,out int instance)?instance:0);
            if(HasPassive(KaitPassive.ShatteringPalm)&&yummnBrokenIceThisRoot.Add(freezeKey))
            {
                YummnTrigger("E06",r);YummnStatusEvent(r,e,"Shatter");
                var victims=enemies.FindAll(v=>v.life!=KaitEnemyLife.Dead&&(v.pos-e.pos).sqrMagnitude==1);
                YummnAfterPacket(()=>YummnDamagePacket(victims,1,Vector2Int.zero,YummnDamageCause.Shatter,r));
            }
            return 0;
        }
        bool bypass=(cause==YummnDamageCause.Punch||cause==YummnDamageCause.Quivering)&&r.yummnAction.startedInShadow&&HasPassive(KaitPassive.ShadowAssault);
        bool blocked=e.type==KaitEnemyType.ShieldKnight&&e.facing==-direction&&!bypass;
        int dealt=blocked?0:Mathf.Min(e.hp,damage);e.hp-=dealt;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Hit,parentEventId=yummnParentEvent,actionId=Yummn.actionId,targetId=e.id,from=katePos,to=e.pos,direction=direction,damageCause=cause,amount=dealt,hpAfter=e.hp,blocked=blocked});
        if(dealt>0)
        {
            var hits=Yummn.metrics.enemyHits;hits[e.id]=hits.TryGetValue(e.id,out var h)?h+1:1;
            var sources=Yummn.metrics.damageSources;string key=cause.ToString();sources[key]=sources.TryGetValue(key,out var n)?n+dealt:dealt;
            if(bypass&&cause==YummnDamageCause.Punch)YummnTrigger("S05",r);
        }
        if(e.hp>0)return dealt;
        e.life=KaitEnemyLife.Dead;e.intent=new KaitIntent{origin=e.pos};
        Yummn.RemovePalm(e.id);
        if(Yummn.rewardedDeaths.Add(e.id))
        {
            kills++;r.killedEnemyIds.Add(e.id);r.playerKilledEnemyIds.Add(e.id);r.killedEnemyCells.Add(e.pos);r.yummnAction.killIds.Add(e.id);
            if(!Yummn.rules.Is082)Yummn.metrics.killKi+=GainYummnKi(Yummn.profile.killKi,r,"Kill");
            else RewardYummn082Kills(r);
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Kill,actionId=Yummn.actionId,targetId=e.id,to=e.pos,damageCause=cause});
        }
        else Yummn.metrics.duplicatesPrevented++;
        if(e.id==bossEnemyId){Yummn.metrics.bossKilledAction=Yummn.actionId;End("Victory: Shield Knight",true);}
        return dealt;
    }
    private bool ForceYummnEnemy(KaitEnemy e,Vector2Int direction,int distance,KaitTurnResult r)
    {
        if(e.type==KaitEnemyType.ShieldKnight||e.yummnFrozen||e.life==KaitEnemyLife.Dead)return false;
        Vector2Int origin=e.pos,to=origin;
        bool terrainCollision=false;
        for(int i=0;i<distance;i++){var next=to+direction;if(IsHardBlocked(next)||EnemyAt(next)?.yummnFrozen==true){terrainCollision=true;break;}if(next==katePos||EnemyAt(next)!=null)break;to=next;}
        if(to==origin){if(terrainCollision&&HasPassive(KaitPassive.StoneBracers))YummnHit(e,1,direction,YummnDamageCause.Collision,r);return false;}
        e.pos=to;pushCount++;r.pushed=true;r.pushFrom=origin;r.pushTo=to;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Move,actionId=Yummn.actionId,targetId=e.id,from=origin,to=to,moveCause=YummnMoveCause.Forced});
        // Locked melee/area cells do not follow their victim. Arrows retain direction.
        if(Yummn.rules.Is082)TranslateYummn082Intent(e,origin);
        else if(e.type==KaitEnemyType.Archer&&e.rangedState==KaitRangedState.Aim)e.intent=BuildYummnArrow(e.pos,e.intent.direction);
        ResolveMomentumResonance(origin,direction,r);
        if(terrainCollision&&HasPassive(KaitPassive.StoneBracers)){YummnTrigger("N03",r);YummnHit(e,1,direction,YummnDamageCause.Collision,r);}
        if(HasPassive(KaitPassive.FreezePush))FreezeYummnEnemy(e,r);
        ResolveYummnRangeExit(e,origin,r);
        if((e.pos-katePos).sqrMagnitude!=1)yummnInReach.Remove(e.id);
        if(!ended&&HasPassive(KaitPassive.FollowThrough))
        {
            var destination=to-direction;var reachable=katePos;
            var difference=destination-katePos;
            if(difference.x*direction.y==difference.y*direction.x&&difference.x*direction.x+difference.y*direction.y>0)
                for(int i=0;i<BattleSize&&reachable!=destination;i++)
                {var next=reachable+direction;if(!YummnEmpty(next))break;reachable=next;}
            if(reachable!=katePos){MoveYummnPlayer(reachable,YummnMoveCause.PushFollow,r);YummnTrigger("O03",r);}
        }
        ResolveYummnRangeEntries(r);return true;
    }
}
