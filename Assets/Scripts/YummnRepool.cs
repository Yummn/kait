using UnityEngine;

public sealed partial class KaitRun
{
    public int EquippedCardCount=>skills.Count+passives.Count;
    public KaitAbilityDef EquippedCard(int index)=>index<0?null:index<skills.Count?KaitAbilityCatalog.Get(skills[index]):index<EquippedCardCount?(IsYummn?YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Passive&&d.passive==passives[index-skills.Count]):null)??KaitAbilityCatalog.Get(passives[index-skills.Count]):null;
    private bool ResolveYummnSharedReward(KaitAbilityDef def,int slot)
    {
        if(slot < -1||slot>=EquippedCardCount||EquippedCardCount>=6&&slot<0)return false;
        if(slot>=0)
        {
            var old=EquippedCard(slot);if(!inactiveAbilities.Remove(old.id))retiredAbilities.Add(old.id);
            if(slot<skills.Count)skills.RemoveAt(slot);else passives.RemoveAt(slot-skills.Count);
            Yummn.prepared.Remove(old.id);
        }
        if(def.kind==KaitAbilityKind.Active){skills.Add(def.skill);skillCooldowns[def.skill]=0;}else passives.Add(def.passive);
        inactiveAbilities.Add(def.id);
        var counts=slot>=0?Yummn.metrics.cardReplacements:Yummn.metrics.cardSelections;
        counts[def.id]=counts.TryGetValue(def.id,out var n)?n+1:1;
        rewardQueue.Dequeue();return true;
    }
    private Vector2Int? repoolCellTarget;
    private bool CastYummnCell(KaitSkill skill,Vector2Int cell,out string message)
    {
        message=YummnCatalog.TargetHint(skill);
        lastSkillResult=null;
        Yummn.prepared.Clear();
        if(!CanUseYummnSkill(skill)){message="技能未装备或当前不可用";return false;}
        if(!IsLegalSkillCell(skill,cell))return false;
        Yummn.prepared.Clear();Yummn.prepared.Add(KaitAbilityCatalog.Get(skill).id);
        repoolCellTarget=cell;
        var d=cell-katePos;
        var direction=d.x>0?KaitDirection.Right:d.x<0?KaitDirection.Left:d.y>0?KaitDirection.Up:KaitDirection.Down;
        try {lastSkillResult=TryYummnDirection(direction);}
        finally {repoolCellTarget=null;}
        if(!lastSkillResult.valid){Yummn.prepared.Clear();message=lastSkillResult.message;return false;}
        message="已使用："+SkillName(skill);RecordReplay("cellskill",(int)skill,cell.x,cell.y);return true;
    }
    private void SyncYummnCapacity()
    {
        Yummn.profile.maxKi=Yummn.rules.MaxKi+(HasPassive(KaitPassive.DeepReservoir)?3:0);
        if(Ki>=Yummn.profile.maxKi){Yummn.ki=Yummn.profile.maxKi;Yummn.kiTenths=0;}
    }
    private int YummnKillGain=>Mathf.Max(0,Yummn.rules.KillKi-(HasPassive(KaitPassive.DeepReservoir)?1:0)-(HasPassive(KaitPassive.KiAegis)?1:0));
    private bool SpendRepoolKi(int amount,KaitTurnResult r)
    {
        if(ExactKi<amount)return false;
        Yummn.ki-=amount;Yummn.metrics.kiSpent+=amount;
        if(ExactKi==0&&KiPhase==YummnPhase.Burst)r.yummnAction.reachedZeroKi=true;
        RepoolStatus(r,"KiSpent",katePos,amount);return true;
    }
    private void RepoolStatus(KaitTurnResult r,string status,Vector2Int cell,int amount=0)
    {r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,actionId=Yummn.actionId,status=status,to=cell,amount=amount});}
    private void PrepareYummnWaitDefenses(YummnActionContext a)
    {
        if(!a.isWait)return;
        Yummn.defense|=HasPassive(KaitPassive.WaitingGuard);
    }
    private Vector2Int EchoDestination(KaitDirection dir)
    {
        for(var p=katePos+Delta(dir);Inside(p);p+=Delta(dir))
            if(YummnEmpty(p)&&Yummn.afterimages.Exists(m=>m.alive&&m.cell==p))return p;
        return YummnRun.NoCell;
    }
    private Vector2Int PhantomDestination(KaitDirection dir)
    {
        var last=katePos;
        for(var p=katePos+Delta(dir);!IsHardBlocked(p);p+=Delta(dir))
        {var e=EnemyAt(p);if(e!=null&&e.yummnFrozen)break;if(e==null)last=p;}
        return last;
    }
    private void FreezeYummnEnemy(KaitEnemy e,KaitTurnResult r)
    {
        if(e.life==KaitEnemyLife.Dead)return;
        e.yummnFrozen=true;e.intent=new KaitIntent{origin=e.pos};e.rangedState=KaitRangedState.Ready;
        SyncYummnControl(e);YummnStatusEvent(r,e,"Frozen");
    }
}
