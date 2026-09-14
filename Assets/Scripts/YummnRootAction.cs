using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    private int yummnEventSequence,yummnParentEvent;
    private sealed class YummnEventScope:IDisposable
    {
        private readonly KaitRun run;private readonly int parent;
        public YummnEventScope(KaitRun run,KaitTurnResult result,YummnAttackFamily family,YummnAttackOrigin origin,int target)
        {
            this.run=run;parent=run.yummnParentEvent;int id=++run.yummnEventSequence;
            result.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.AttackBegin,eventId=id,parentEventId=parent,actionId=result.yummnAction.actionId,targetId=target,attackFamily=family,attackOrigin=origin});
            run.yummnParentEvent=id;
        }
        public void Dispose(){run.yummnParentEvent=parent;}
    }
    // Direct victims are committed before break-ice/reflection chains run.
    private int yummnPacketDepth;
    private readonly Queue<Action> yummnPacketReactions=new Queue<Action>();
    private readonly HashSet<string> yummnBrokenIceThisRoot=new HashSet<string>();
    private readonly Dictionary<int,int> yummnFreezeInstances=new Dictionary<int,int>();
    private readonly HashSet<long> yummnAfterimageKiHits=new HashSet<long>();
    private void GainYummnAfterimageKi(int attackId,KaitTurnResult r,int markerId=-1)
    {
        long hitKey=((long)attackId<<32)^(uint)markerId;
        if(yummnAfterimageKiHits.Add(hitKey))Yummn.metrics.afterimageKi+=GainYummnKi(1,r,"Afterimage");
    }
    private void FinishYummnPacket()
    {
        yummnPacketDepth--;
        if(yummnPacketDepth==0)while(yummnPacketReactions.Count>0)yummnPacketReactions.Dequeue()();
    }
    private void YummnAfterPacket(Action reaction)
    {
        if(yummnPacketDepth>0)yummnPacketReactions.Enqueue(reaction);else reaction();
    }
    private void YummnDamagePacket(List<KaitEnemy> victims,int damage,Vector2Int direction,YummnDamageCause cause,KaitTurnResult r,bool freeze=false)
    {
        yummnPacketDepth++;
        try
        {
            foreach(var victim in victims)
                if(YummnHit(victim,damage,direction,cause,r)>0&&freeze&&victim.life!=KaitEnemyLife.Dead)
                    FreezeYummnEnemy(victim,r);
        }
        finally
        {
            FinishYummnPacket();
        }
    }
    private void StampYummnRoot(KaitTurnResult result)
    {
        int id=yummnEventSequence;
        foreach(var ev in result.yummnEvents)
        {
            if(ev.eventId==0)ev.eventId=++id;ev.actionId=result.yummnAction.actionId;
            if(ev.kind==YummnEventKind.Hit||ev.kind==YummnEventKind.Kill)
                ev.attackFamily=ev.damageCause==YummnDamageCause.Kick?YummnAttackFamily.Kick:
                    ev.damageCause==YummnDamageCause.Punch||ev.damageCause==YummnDamageCause.Counter?YummnAttackFamily.Martial:
                    ev.damageCause==YummnDamageCause.Collision?YummnAttackFamily.Environment:YummnAttackFamily.Spell;
        }
    }
}
