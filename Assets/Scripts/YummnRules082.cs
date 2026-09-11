using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    private void ChargeYummnAttackTenth(KaitTurnResult r)
    {
        var a=r.yummnAction;
        if(Yummn.rules.AttackCostTenths==0||a.phaseAtStart==YummnPhase.Exhausted||a.reachedZeroKi)return;
        int remaining=Ki*10+Yummn.kiTenths;
        int paid=Mathf.Min(remaining,Yummn.rules.AttackCostTenths);
        remaining-=paid;a.attackCostTenths+=paid;Yummn.attackSpentTenths+=paid;
        Yummn.ki=remaining/10;Yummn.kiTenths=remaining%10;
        if(remaining==0)a.reachedZeroKi=true;
    }
    // This is also the HUD preview: no RNG or state changes are allowed here.
    public int PreviewYummnMovementCost(KaitDirection direction)
    {
        if(!IsYummn||!Yummn.rules.Is082)return 0;
        return ValidateYummnAction(direction,out var action,out _)?action.movementKiCost:-1;
    }
    private void PlanYummn082Movement(YummnActionContext a)
    {
        if(a.actionOverride!=null)return;
        int available=Ki-a.skillKiCost;
        int limit=a.phaseAtStart==YummnPhase.Exhausted?1:
            Yummn.rules.MovementCostMode==YummnMovementCostMode.PerCell?available:available>0?BattleSize:0;
        for(int i=1;i<=limit;i++)
        {
            var p=a.startCell+Delta(a.direction)*i;
            if(IsHardBlocked(p)||EnemyAt(p)!=null)break;
            a.voluntaryCells++;
        }
        a.movementKiCost=a.phaseAtStart==YummnPhase.Exhausted?0:
            Yummn.rules.MovementCostMode==YummnMovementCostMode.PerCell?a.voluntaryCells:a.voluntaryCells>0?1:0;
        a.totalKiCost=a.skillKiCost+a.movementKiCost;
    }
    private void CommitYummn082Cost(KaitTurnResult r)
    {
        var a=r.yummnAction;
        Yummn.ki-=a.totalKiCost;
        a.reachedZeroKi=a.phaseAtStart==YummnPhase.Burst&&ExactKi==0;
        Yummn.metrics.kiSpent+=a.totalKiCost;
        Yummn.metrics.movementKiSpent+=a.movementKiCost;
        Yummn.metrics.skillKiSpent+=a.skillKiCost;
        if(a.phaseAtStart!=YummnPhase.Burst||a.voluntaryCells==0)return;
        var marker=new YummnAfterimageMarker{id=++Yummn.nextAfterimageId,sourceActionId=a.actionId,cell=a.startCell,direction=a.direction};
        Yummn.afterimages.Add(marker);Yummn.metrics.afterimagesCreated++;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.AfterimageCreated,actionId=a.actionId,markerId=marker.id,to=marker.cell,direction=Delta(a.direction)});
    }
    public bool ShouldRunYummnEnemyPhase(YummnActionContext a)
    {
        a.enemyPhaseReason=YummnEnemyPhaseReason.None;
        if(a.phaseAtStart==YummnPhase.Exhausted)a.enemyPhaseReason|=YummnEnemyPhaseReason.Exhaustion;
        if(a.killIds.Count>0)a.enemyPhaseReason|=YummnEnemyPhaseReason.Kill;
        if(Yummn.rules.AttackAdvancesEnemyPhase&&a.didAttack)a.enemyPhaseReason|=YummnEnemyPhaseReason.Attack;
        if(Yummn.rules.MovementAdvancesEnemyPhase&&a.playerMoved)a.enemyPhaseReason|=YummnEnemyPhaseReason.Movement;
        return a.enemyPhaseReason!=YummnEnemyPhaseReason.None;
    }
    private void RewardYummn082Kills(KaitTurnResult r)
    {
        var a=r.yummnAction;
        while(a.rewardedKills<a.killIds.Count)
        {a.rewardedKills++;Yummn.metrics.killKi+=GainYummnKi(Yummn.rules.KillKi,r,"Kill");}
    }
    private void SupplyYummn082Twos(KaitTurnResult r,bool countersOnly=false)
    {
        var a=r.yummnAction;
        a.stationaryPunch=a.isPunchAction&&a.preHitTravelCells==0;
        int requested=Yummn.rules.Supply==YummnTileSupplyMode.EveryAction?(countersOnly?0:1):
            Yummn.rules.Supply==YummnTileSupplyMode.EffectiveMove?(countersOnly?0:a.playerMoved?1:0):
            Yummn.rules.Supply==YummnTileSupplyMode.KillOnly?a.killIds.Count-a.suppliedKills:
            countersOnly||a.stationaryPunch?0:a.voluntaryCells>0||a.actionOverride!=null?1:0;
        a.suppliedKills=a.killIds.Count;
        if(Yummn.rules.Supply==YummnTileSupplyMode.SkipStationaryPunch&&requested>0&&!a.didAttack&&katePos!=a.startCell&&HasPassive(KaitPassive.PassWithoutTrace))
        {a.suppressedTwo=true;Yummn.metrics.suppressedTwos++;YummnTrigger("S04",r);requested=0;}
        a.requestedTwos+=requested;Yummn.metrics.requestedTwos+=requested;
        for(int i=0;i<requested;i++)
        {
            nextTwoPriority.RemoveAll(p=>p.x<0||p.y<0||p.x>=ThreatSize||p.y>=ThreatSize||threatPillars[p.x,p.y]||threat[p.x,p.y]!=0);
            var cell=SpawnThreatTwoForTurn(r);
            if(cell.x>=0){a.insertedTwos++;Yummn.metrics.insertedTwos++;Yummn.metrics.naturalTwos++;r.newThreatCells.Add(cell);}
            else {a.droppedTwos++;Yummn.metrics.droppedTwos++;}
        }
    }
    private void ResolveYummn082Tail(KaitTurnResult r)
    {
        var a=r.yummnAction;
        RewardYummn082Kills(r);
        if(ended)return; // A boss kill wins before any subsequent board or enemy check.
        if(a.didAttack||!IsYummnShadow(katePos))Yummn.cloak=false;
        else if(HasPassive(KaitPassive.ShadowCloak)){Yummn.cloak=true;YummnTrigger("S03",r);}
        Yummn.tranquility=a.phaseAtStart==YummnPhase.Exhausted&&!a.didAttack&&katePos!=a.startCell&&HasPassive(KaitPassive.Tranquility);
        SupplyYummn082Twos(r);
        r.yummnThreatAfterSupply=CopyThreat();
        ResolveOldNewsArchive(r);
        r.yummnThreatAfterArchive=CopyThreat();
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="SupplyReady",actionId=a.actionId,amount=r.newThreatCells.Count});
        a.enemyPhase=ShouldRunYummnEnemyPhase(a);
        if(a.enemyPhase)
        {
            a.spawnChecked=true;ResolveYummnRifts(r);
            if(bossPending){SpawnShieldKnight(r);if(r.bossSpawned)Yummn.metrics.bossCreatedAction=a.actionId;}
            ResolveYummnEnemyPhase(r);
            // Counter kills join the same transaction; never recurse into AI.
            RewardYummn082Kills(r);
            int beforeCounterSupply=r.newThreatCells.Count;
            if(!ended)SupplyYummn082Twos(r,true);
            if(r.newThreatCells.Count>beforeCounterSupply)r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="CounterSupplyReady",actionId=a.actionId,amount=beforeCounterSupply});
            ClearYummn082Afterimages(r);
        }
        else if(bossPending){SpawnShieldKnight(r);if(r.bossSpawned)Yummn.metrics.bossCreatedAction=a.actionId;}
        if(ended)return;
        if(a.phaseAtStart==YummnPhase.Exhausted)Yummn.metrics.recoveryKi+=GainYummnKi(1,r,"Recovery");
        if(IsYummnThreatLocked()){threatLocks++;Yummn.metrics.threatLocks++;End("ThreatBoardLocked",false);r.message="2048无可用移动，本局失败";return;}
        FinishYummnPhase(r);
    }
    private void HitYummn082Afterimages(KaitEnemy enemy,KaitIntent intent,int attackId,KaitTurnResult r)
    {
        bool hit=false;
        foreach(var marker in Yummn.afterimages)
        {
            if(!marker.alive||!intent.affectedCells.Contains(marker.cell))continue;
            marker.alive=false;hit=true;Yummn.metrics.afterimagesHit++;
            string key=enemy.type.ToString();var counts=Yummn.metrics.afterimageHitsByEnemy;
            counts[key]=counts.TryGetValue(key,out var n)?n+1:1;
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.AfterimageHit,actionId=Yummn.actionId,sourceId=enemy.id,markerId=marker.id,attackEventId=attackId,to=marker.cell});
        }
        if(hit)Yummn.metrics.afterimageKi+=GainYummnKi(1,r,"Afterimage");
        Yummn.afterimages.RemoveAll(m=>!m.alive);
    }
    private void ClearYummn082Afterimages(KaitTurnResult r)
    {
        foreach(var marker in Yummn.afterimages)
        {
            marker.alive=false;Yummn.metrics.afterimagesCleared++;
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.AfterimageCleared,actionId=Yummn.actionId,markerId=marker.id,to=marker.cell});
        }
        Yummn.afterimages.Clear();
    }
    private void TranslateYummn082Intent(KaitEnemy e,Vector2Int oldOrigin)
    {
        if(e.intent==null||e.intent.type==KaitIntentType.None)return;
        if(e.type==KaitEnemyType.Archer){e.intent=BuildYummnArrow(e.pos,e.intent.direction);return;}
        if(e.type==KaitEnemyType.Warlock)return; // The cross targets the ground, not its caster.
        var offset=e.pos-oldOrigin;e.intent.origin+=offset;e.intent.target+=offset;
        for(int i=0;i<e.intent.affectedCells.Count;i++)e.intent.affectedCells[i]+=offset;
        e.intent.affectedCells.RemoveAll(IsHardBlocked);
    }
}
