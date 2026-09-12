using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    private readonly HashSet<int> yummnInReach=new HashSet<int>();
    private readonly HashSet<int> yummnReacted=new HashSet<int>();
    private bool yummnResolvingReaction;
    private int yummnPunchDepth;
    private readonly HashSet<int> yummnExitReacted=new HashSet<int>();
    private bool yummnInEnemyPhase;
    private void ResolveYummnRangeExit(KaitEnemy e,Vector2Int to,KaitTurnResult r)
    {
        if(!HasPassive(KaitPassive.OpportunityAttack)||ended||e.life==KaitEnemyLife.Dead||
           (e.pos-katePos).sqrMagnitude!=1||(to-katePos).sqrMagnitude==1||!yummnExitReacted.Add(e.id))return;
        bool previous=yummnResolvingReaction;yummnResolvingReaction=true;
        try {YummnTrigger("R40",r);ResolveYummnPunch(e,r,true);}
        finally {yummnResolvingReaction=previous;}
    }
    private void BeginYummnRangeTracking()
    {
        yummnInReach.Clear();yummnReacted.Clear();yummnExitReacted.Clear();yummnPunchDepth=0;yummnResolvingReaction=false;
        foreach(var e in enemies)if(e.life!=KaitEnemyLife.Dead&&(e.pos-katePos).sqrMagnitude==1)yummnInReach.Add(e.id);
    }
    private void ResolveYummnRangeEntries(KaitTurnResult r)
    {
        if(yummnPunchDepth>0||yummnResolvingReaction||!HasPassive(KaitPassive.Opportunist)||ended)return;
        yummnResolvingReaction=true;
        try
        {
            var pending=new Queue<KaitEnemy>();
            // Refresh after every reaction: push/follow/kill can change both positions.
            // One reaction per enemy per input bounds chained push/follow combinations.
            for(int guard=0;guard<enemies.Count+1&&!ended;guard++)
            {
                var inReach=enemies.FindAll(e=>e.life!=KaitEnemyLife.Dead&&(e.pos-katePos).sqrMagnitude==1);
                inReach.Sort((a,b)=>a.id.CompareTo(b.id));
                foreach(var e in inReach)if(!yummnInReach.Contains(e.id)&&!yummnReacted.Contains(e.id))pending.Enqueue(e);
                yummnInReach.Clear();foreach(var e in inReach)yummnInReach.Add(e.id);
                if(pending.Count==0)break;
                var entrant=pending.Dequeue();
                if(entrant.life==KaitEnemyLife.Dead||!yummnInReach.Contains(entrant.id)||yummnReacted.Contains(entrant.id))continue;
                yummnReacted.Add(entrant.id);YummnTrigger("S06",r);
                ResolveYummnPunch(entrant,r,true);
            }
        }
        finally {yummnResolvingReaction=false;}
    }
    private void LeaveYummnMovementEcho(Vector2Int from,Vector2Int to,KaitTurnResult r)
    {
        if(!Yummn.rules.Is082||from==to)return;
        // A contiguous slide is one displacement. A later follow-up gets its own
        // marker; never stack identical markers on the same departure cell/input.
        if(Yummn.afterimages.Exists(m=>m.alive&&m.sourceActionId==Yummn.actionId&&m.cell==from))return;
        var d=to-from;var direction=Mathf.Abs(d.x)>=Mathf.Abs(d.y)?(d.x>0?KaitDirection.Right:KaitDirection.Left):(d.y>0?KaitDirection.Up:KaitDirection.Down);
        var marker=new YummnAfterimageMarker{id=++Yummn.nextAfterimageId,sourceActionId=Yummn.actionId,cell=from,direction=direction};
        Yummn.afterimages.Add(marker);Yummn.metrics.afterimagesCreated++;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.AfterimageCreated,actionId=Yummn.actionId,markerId=marker.id,to=from,direction=Delta(direction)});
    }
}
