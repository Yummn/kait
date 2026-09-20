using UnityEngine;

public sealed partial class KaitRun
{
    public KaitTurnResult TryYummnWait()
    {
        if(IsReynard)return TryReynardWait();
        var r=new KaitTurnResult();
        if(!IsYummn||ended){r.message="当前不能等待";return r;}
        ActivateBuildForInput();SyncYummnCapacity();
        var a=new YummnActionContext { actionId=++Yummn.actionId,phaseAtStart=KiPhase,
            kiBefore=Ki,startCell=katePos,finalCell=katePos,targetCell=YummnRun.NoCell,
            iceAtStart=Yummn.icePillar,darknessAtStart=Yummn.darkness,decoyAtStart=Yummn.decoy,
            enemyPhase=true,spawnChecked=true,isWait=true };
        r.yummnAction=a;r.valid=r.turnComplete=true;r.threatBefore=CopyThreat();
        r.yummnThreatAfterMerge=CopyThreat();r.yummnThreatAfterSupply=CopyThreat();r.yummnThreatAfterArchive=CopyThreat();
        Yummn.metrics.actions++;turnTriggers.Clear();
        BeginYummnRangeTracking();BeginYummnRoot();
        Yummn.prepared.Clear();
        PrepareYummnWaitDefenses(a);
        if(Yummn.rules.Is082)
        {
            ResolveYummn082Tail(r);
            a.kiAfter=Ki;a.phaseAtEnd=KiPhase;a.finalCell=katePos;r.threatAfter=CopyThreat();
            StampYummnRoot(r);Yummn.history.Add(a);turn++;PrepareThreatTwoPreview();
            RecordReplay("wait",0,0,0);return r;
        }
        SupplyYummn082Twos(r);
        r.yummnThreatAfterSupply=CopyThreat();
        ResolveOldNewsArchive(r);r.yummnThreatAfterArchive=CopyThreat();
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="SupplyReady",actionId=a.actionId,amount=r.newThreatCells.Count});
        // Waiting never slides either board.
        ResolveYummnRifts(r);
        if(!ended&&bossPending)SpawnShieldKnight(r);
        ResolveYummnRangeEntries(r);
        if(!ended)ResolveYummnEnemyPhase(r);
        if(Yummn.rules.Is082)
        {
            RewardYummn082Kills(r);
            if(!ended)SupplyYummn082Twos(r,true);
            ClearYummn082Afterimages(r);
        }
        FinishYummnRoot(r);
        if(!ended&&a.phaseAtStart==YummnPhase.Exhausted)
            Yummn.metrics.recoveryKi+=GainYummnKi(1,r,"Recovery");
        if(!ended)FinishYummnPhase(r);
        a.kiAfter=Ki;a.phaseAtEnd=KiPhase;a.finalCell=katePos;
        r.threatAfter=CopyThreat();Yummn.history.Add(a);turn++;PrepareThreatTwoPreview();
        RecordReplay("wait",0,0,0);return r;
    }
}
