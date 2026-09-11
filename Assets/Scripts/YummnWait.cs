using UnityEngine;

public sealed partial class KaitRun
{
    public KaitTurnResult TryYummnWait()
    {
        var r=new KaitTurnResult();
        if(!IsYummn||ended){r.message="当前不能等待";return r;}
        var a=new YummnActionContext { actionId=++Yummn.actionId,phaseAtStart=KiPhase,
            kiBefore=Ki,startCell=katePos,finalCell=katePos,targetCell=YummnRun.NoCell,
            iceAtStart=Yummn.icePillar,darknessAtStart=Yummn.darkness,
            enemyPhase=true,spawnChecked=true,isWait=true };
        r.yummnAction=a;r.valid=r.turnComplete=true;r.threatBefore=CopyThreat();
        r.yummnThreatAfterMerge=CopyThreat();r.yummnThreatAfterSupply=CopyThreat();r.yummnThreatAfterArchive=CopyThreat();
        Yummn.metrics.actions++;turnTriggers.Clear();
        // No player action, tile movement/supply, skill commitment or Ki payment.
        ResolveYummnRifts(r);
        if(!ended&&bossPending)SpawnShieldKnight(r);
        if(!ended)ResolveYummnEnemyPhase(r);
        if(Yummn.rules.Is082)
        {
            RewardYummn082Kills(r);
            if(!ended)SupplyYummn082Twos(r,true);
            ClearYummn082Afterimages(r);
        }
        if(!ended&&a.phaseAtStart==YummnPhase.Exhausted)
            Yummn.metrics.recoveryKi+=GainYummnKi(1,r,"Recovery");
        if(!ended)FinishYummnPhase(r);
        if(!ended&&IsYummnThreatLocked()){End("ThreatBoardLocked",false);r.message="2048无可用移动，本局失败";}
        a.kiAfter=Ki;a.phaseAtEnd=KiPhase;a.finalCell=katePos;
        r.threatAfter=CopyThreat();Yummn.history.Add(a);turn++;PrepareThreatTwoPreview();
        return r;
    }
}
