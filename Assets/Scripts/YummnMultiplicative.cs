using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    private readonly HashSet<KaitMergeEvent> processedYummnMerges=new HashSet<KaitMergeEvent>();
    private readonly Queue<KaitMergeEvent> pendingYummnResonance=new Queue<KaitMergeEvent>();
    private readonly List<KaitMergeEvent> pendingYummnBag=new List<KaitMergeEvent>();
    private readonly List<int> deferredYummnNumbers=new List<int>();
    private readonly HashSet<int> yummnSweepTargets=new HashSet<int>();
    private bool yummnPendulumUsed;
    private void CreateYummnSpellEcho(Vector2Int cell,KaitTurnResult r)
    {
        var marker=new YummnAfterimageMarker{id=++Yummn.nextAfterimageId,sourceActionId=Yummn.actionId,cell=cell,direction=r.yummnAction.direction,source="MirrorImage",remainingPhases=HasPassive(KaitPassive.LastingImage)?2:1};
        Yummn.afterimages.Add(marker);Yummn.metrics.afterimagesCreated++;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.AfterimageCreated,actionId=Yummn.actionId,markerId=marker.id,to=cell,direction=Delta(marker.direction)});
    }
    private sealed class YummnMergeAttack { public Vector2Int cell;public int targetId=-1;public bool missile; }
    private void BeginYummnRoot()
    {
        processedYummnMerges.Clear();pendingYummnResonance.Clear();pendingYummnBag.Clear();deferredYummnNumbers.Clear();
        yummnSweepTargets.Clear();yummnPendulumUsed=false;
        yummnPacketDepth=0;yummnPacketReactions.Clear();yummnBrokenIceThisRoot.Clear();
        yummnAfterimageKiHits.Clear();
        yummnFreezeInstances.Clear();
        yummnEventSequence=yummnParentEvent=0;
    }
    private void ExecuteYummnMergeAttack(YummnMergeAttack spell,KaitTurnResult r)
    {
        if(spell.missile)
        {
            var enemy=enemies.Find(e=>e.id==spell.targetId&&e.life!=KaitEnemyLife.Dead);
            if(enemy!=null)
            {
                r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="MergeMissileLaunch",from=spell.cell,to=enemy.pos,targetId=enemy.id,actionId=Yummn.actionId});
                YummnHit(enemy,1,Vector2Int.zero,YummnDamageCause.MergeMissile,r);
            }
        }
        else
        {
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="MergeGlyphCast",to=spell.cell,actionId=Yummn.actionId});
            YummnDamagePacket(enemies.FindAll(enemy=>enemy.life!=KaitEnemyLife.Dead&&(enemy.pos-spell.cell).sqrMagnitude<=1),1,Vector2Int.zero,YummnDamageCause.MergeGlyph,r);
        }
    }
    private void ProcessYummnMerge(KaitMergeEvent merge,KaitTurnResult r)
    {
        if(!processedYummnMerges.Add(merge))return;
        merge.rootActionId=Yummn.actionId;merge.mergeId=processedYummnMerges.Count;
        if(string.IsNullOrEmpty(merge.mergeSource))merge.mergeSource="BasePass";
        HandleMilestoneMerge(merge);
        if(merge.resultValue<config.winValue)QueueYummnRift(merge,r);
        if(!merge.spawnSuppressed&&merge.resultValue<config.winValue)
        {
            var request=spawns.Find(s=>s.targetCell==MapThreatToBattle(merge.threatCell));
            if(request!=null)r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="RiftQueued",to=request.targetCell,from=merge.threatCell,amount=request.tier});
        }
        pendingYummnBag.Add(merge);
        if(HasPassive(KaitPassive.ResonanceCrystal))pendingYummnResonance.Enqueue(merge);
        if(HasPassive(KaitPassive.BountyJar)){deferredYummnNumbers.Add(merge.sourceValue>0?merge.sourceValue:merge.resultValue/2);YummnTrigger("N18",r);}
        if(HasPassive(KaitPassive.ManaPearl)&&merge.resultValue>=8){GainYummnKi(1,r,"MergePearl");YummnTrigger("N08",r);}
        var direct=new List<YummnMergeAttack>();var cell=MapThreatToBattle(merge.threatCell);
        // Each merge occurrence locks its own target before its glyph resolves.
        if(HasPassive(KaitPassive.WardingGlyph))direct.Add(new YummnMergeAttack{cell=cell});
        if(HasPassive(KaitPassive.MagicMissile))
        {
            var targets=enemies.FindAll(e=>e.life!=KaitEnemyLife.Dead);
            targets.Sort((a,b)=>{int c=(Mathf.Abs(a.pos.x-cell.x)+Mathf.Abs(a.pos.y-cell.y)).CompareTo(Mathf.Abs(b.pos.x-cell.x)+Mathf.Abs(b.pos.y-cell.y));return c!=0?c:a.id.CompareTo(b.id);});
            direct.Add(new YummnMergeAttack{cell=cell,missile=true,targetId=targets.Count>0?targets[0].id:-1});
        }
        foreach(var spell in direct){YummnTrigger(spell.missile?"N14":"N09",r);ExecuteYummnMergeAttack(spell,r);}
        if(HasPassive(KaitPassive.SpellEcho)&&merge.mergeSource!="SpellEcho")
        {
            var echo=new KaitMergeEvent{sourceValue=merge.sourceValue,resultValue=merge.resultValue,threatCell=merge.threatCell,
                actualThreatDirection=merge.actualThreatDirection,systemMerge=true,sequence=r.merges.Count,mergeSource="SpellEcho"};
            r.merges.Add(echo);YummnTrigger("N17",r);
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="SpellEchoCast",to=cell,actionId=Yummn.actionId});
            ProcessYummnMerge(echo,r); // Full triggers, but no tile mutation or recursive echo.
        }
    }
    private void MergeYummnPair(Vector2Int a,Vector2Int b,KaitTurnResult r,string card)
    {
        int value=threat[a.x,a.y];threat[a.x,a.y]=value*2;threat[b.x,b.y]=0;highestThreat=Mathf.Max(highestThreat,value*2);
        threatTwoBirth[a.x,a.y]=++nextThreatTwoBirth;threatTwoBirth[b.x,b.y]=0;
        var merge=new KaitMergeEvent{sourceValue=value,resultValue=value*2,threatCell=a,systemMerge=true,sequence=r.merges.Count,actualThreatDirection=actualThreatDirection,mergeSource=card=="N16"?"Resonance":"Archive"};
        r.merges.Add(merge);r.threatMotions.Add(new KaitThreatMotion{value=value,from=b,to=a,merged=true});
        mergeHeatmap[a.x,a.y]++;if(IsInternalThreatCell(a))internalMergeCount++;
        YummnTrigger(card,r);
        if(card=="N16")r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="ResonanceCrystal",from=b,to=a,actionId=Yummn.actionId});
        ProcessYummnMerge(merge,r);
    }
    private void ResolveYummnMergeChains(KaitTurnResult r)=>ResolveYummnMergeBatch(r,true);
    private void FlushYummnMergeBag(KaitTurnResult r)
    {
        ResolveBag(pendingYummnBag,r);pendingYummnBag.Clear();
    }
    private void ResolveYummnMergeBatch(KaitTurnResult r,bool archive)
    {
        var batch=r.merges.FindAll(m=>!processedYummnMerges.Contains(m));
        foreach(var merge in batch)ProcessYummnMerge(merge,r);
        FlushYummnMergeBag(r);
        // Each iteration consumes a tile. This is finite without an arbitrary cap.
        while(!ended)
        {
            Vector2Int first=YummnRun.NoCell,second=first;string card=null;
            while(pendingYummnResonance.Count>0&&card==null)
            {
                var trigger=pendingYummnResonance.Dequeue();
                if(!HasPassive(KaitPassive.ResonanceCrystal))continue;
                var positions=new List<Vector2Int>();var axis=Delta(trigger.actualThreatDirection);
                for(int y=0;y<ThreatSize;y++)for(int x=0;x<ThreatSize;x++)
                {
                    var p=new Vector2Int(x,y);
                    if(p!=trigger.threatCell&&!IsThreatPillar(p)&&threat[x,y]==trigger.resultValue)positions.Add(p);
                }
                positions.Sort((a,b)=>{int c=(b.x*axis.x+b.y*axis.y).CompareTo(a.x*axis.x+a.y*axis.y);return c!=0?c:(a.y*ThreatSize+a.x).CompareTo(b.y*ThreatSize+b.x);});
                if(positions.Count>=2){first=positions[0];second=positions[1];card="N16";}
            }
            if(archive&&card==null&&HasPassive(KaitPassive.OldNewsArchive))
            {
                var groups=new SortedDictionary<int,List<Vector2Int>>();
                for(int y=0;y<ThreatSize;y++)for(int x=0;x<ThreatSize;x++)if(threat[x,y]>0)
                {int v=threat[x,y];if(!groups.ContainsKey(v))groups[v]=new List<Vector2Int>();groups[v].Add(new Vector2Int(x,y));}
                foreach(var pair in groups)if(pair.Value.Count>=5)
                {
                    pair.Value.Sort((a,b)=>{int c=threatTwoBirth[a.x,a.y].CompareTo(threatTwoBirth[b.x,b.y]);return c!=0?c:(a.y*ThreatSize+a.x).CompareTo(b.y*ThreatSize+b.x);});
                    first=pair.Value[0];second=pair.Value[1];card="R34";break;
                }
            }
            if(card==null)break;
            MergeYummnPair(first,second,r,card);
            FlushYummnMergeBag(r);
        }
    }
    private void ResolveYummnPendulum(KaitTurnResult r)
    {
        if(yummnPendulumUsed||!HasPassive(KaitPassive.GravityPendulum)||r.merges.Count==0)return;
        yummnPendulumUsed=true;YummnTrigger("N15",r);r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="GravityPendulum",actionId=Yummn.actionId});
        var baseDirection=actualThreatDirection;
        actualThreatDirection=Opposite(baseDirection);
        try {
        var motions=new List<KaitThreatMotion>();var merges=MoveThreat(actualThreatDirection,motions);
        foreach(var merge in merges){merge.actualThreatDirection=actualThreatDirection;merge.mergeSource="PendulumReturn";}
        r.threatMotions.AddRange(motions);r.merges.AddRange(merges);ResolveYummnMergeBatch(r,false);
        } finally {actualThreatDirection=baseDirection;}
    }
    private void FinishYummnRoot(KaitTurnResult r)
    {
        if(r.yummnAction.actionOverride!="MageHand")ResolveYummnMergeChains(r);
        // Late merges from counter-kill supply also award their kills exactly once.
        while(!ended&&r.yummnAction.suppliedKills<r.yummnAction.killIds.Count)
        {RewardYummn082Kills(r);SupplyYummn082Twos(r,true);ResolveYummnMergeChains(r);}
        RewardYummn082Kills(r);
        // Deposits are deliberately outside the merge processing transaction.
        foreach(int value in deferredYummnNumbers)
        {
            Vector2Int empty=YummnRun.NoCell;
            for(int y=0;y<ThreatSize&&empty.x<0;y++)for(int x=0;x<ThreatSize;x++)
                if(!IsThreatPillar(new Vector2Int(x,y))&&threat[x,y]==0){empty=new Vector2Int(x,y);break;}
            if(empty.x<0)continue;
            threat[empty.x,empty.y]=value;threatTwoBirth[empty.x,empty.y]=++nextThreatTwoBirth;
            r.newThreatCells.Add(empty);r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="BountyJarDeposit",to=empty,amount=value,actionId=Yummn.actionId});
        }
        deferredYummnNumbers.Clear();
    }
    private void ResolveYummnSweep(KaitTurnResult r)
    {
        if(!HasPassive(KaitPassive.SweepingPursuit)||ended)return;
        var targets=enemies.FindAll(e=>e.life!=KaitEnemyLife.Dead&&(e.pos-katePos).sqrMagnitude==1&&!yummnSweepTargets.Contains(e.id));
        targets.Sort((a,b)=>a.id.CompareTo(b.id));if(targets.Count==0)return;
        var target=targets[0];yummnSweepTargets.Add(target.id);YummnTrigger("N01",r);
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="SweepPursuit",to=target.pos,targetId=target.id,actionId=Yummn.actionId});
        ResolveYummnMartialAttack(target,r,true,YummnAttackOrigin.Sweep);
    }
}
