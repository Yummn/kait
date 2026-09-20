using System;
using System.Collections.Generic;
using UnityEngine;

public enum KaitCharacter { Kait, Yummn, Reynard }
public sealed partial class KaitRun
{
    public KaitCharacter Character { get; private set; }
    public readonly ReynardRun Reynard=new ReynardRun();
    public bool IsReynard=>Character==KaitCharacter.Reynard;
    public bool IsYummn => Character==KaitCharacter.Yummn;
    public int KateMaxHp=>IsYummn?Yummn.rules.MaxHp:config.kateMaxHp;
    public string RulesProfileId => IsReynard?ReynardCatalog.RulesVersion:IsYummn?Yummn.rules.Version:KaitAbilityCatalog.RulesVersion;
    public readonly YummnRun Yummn=new YummnRun();
    public int Ki=>Yummn.ki;
    public float ExactKi=>Ki+Yummn.kiTenths/10f;
    public YummnPhase KiPhase=>Yummn.phase;
    // Compatibility getters for existing callers; there is no two-action window.
    public int ActionsRemaining=>0;
    public int ActionsTotal=>0;
    public int ActionIndex=>Yummn.actionId;
    public bool YummnWindow=>false;
    public int EnemyResolveCount=>Yummn.metrics.enemyPhases;
    public int NormalTileSpawnCount=>Yummn.metrics.naturalTwos;
    public int NaturalCooldownTickCount=>0;
    public bool FlurryArmed=>Yummn.prepared.Contains("yummn.M01");
    public bool PalmArmed=>Yummn.prepared.Contains("yummn.O01");
    public bool StunArmed=>Yummn.prepared.Contains("yummn.M03");
    public bool FrostArmed=>Yummn.prepared.Contains("yummn.E04");
    public bool DefenseArmed=>Yummn.defense||Yummn.prepared.Contains("yummn.M02");
    public bool UnarmoredGuard=>Yummn.tranquility;
    public bool MissileSpent=>Yummn.missileSpent;
    public Vector2Int PendingBossCell=>bossPending?bossPendingCell:YummnRun.NoCell;
    public void SelectCharacter(KaitCharacter character,int seed,YummnRulesSnapshot rules=null)
    {
        Character=character;
        // Kait has no Ki economy. Ignore even a materialized empty replay field.
        Yummn.rules=character==KaitCharacter.Yummn?(rules??YummnRulesSnapshot.Current()):new YummnRulesSnapshot(fiveKi:false);
        Reset(seed);
    }
    private void ResetYummnTurn()=>Yummn.Reset();
    public bool YummnPrerequisite(KaitAbilityDef d)=>!IsYummn||YummnMissingRequirement(d,skills,passives)==null;
    public string YummnMissingRequirement(KaitAbilityDef d,IList<KaitSkill> active=null,IList<KaitPassive> passive=null)
    {
        if(d==null)return "卡牌不存在";
        active=active??skills;passive=passive??passives;
        // Approved variants are independent cards; no legacy prerequisite chains.


        return null;
    }
    public bool CanUseYummnSkill(KaitSkill s)=>!ended&&skills.Contains(s)&&YummnCatalog.IsActive(s);
    public bool IsYummnPrepared(KaitSkill s)=>Yummn.prepared.Contains(KaitAbilityCatalog.Get(s)?.id??"");
    public int PreparedKiCost
    {
        get {int cost=!Yummn.rules.Is082&&KiPhase==YummnPhase.Burst?1:0;foreach(var id in Yummn.prepared)cost+=YummnCatalog.Get(id)?.kiExtraCost??0;return cost;}
    }
    private bool TryUseYummnSkill(KaitSkill s,out string message)
    {
        message="技能未装备";if(!CanUseYummnSkill(s))return false;
        var def=KaitAbilityCatalog.Get(s);
        if(Yummn.prepared.Remove(def.id)){Yummn.metrics.preparationCancels++;message="已取消准备";return true;}
        // Defense may accompany any mode; other modifiers and replacement modes are exclusive.
        foreach(var id in new List<string>(Yummn.prepared))
        {
            var old=YummnCatalog.Get(id);if(old.skill==KaitSkill.PatientDefense||s==KaitSkill.PatientDefense)continue;
            if(def.actionOverride!=null||old.actionOverride!=null){Yummn.prepared.Remove(id);Yummn.metrics.preparationCancels++;}
        }
        Yummn.prepared.Add(def.id);lastSkillResult=null;
        message=Yummn.rules.Is082?"已准备 · 技能气 "+PreparedKiCost+"，移动气按方向预览":"已准备 · 额外气 "+def.kiExtraCost+" / 本次总气 "+PreparedKiCost;return true;
    }
    public bool IsYummnShadow(Vector2Int cell)
    {
        if(!Inside(cell)||IsHardBlocked(cell)||cell.x==0||cell.y==0||cell.x==6||cell.y==6)return false;
        foreach(var d in YummnDirections)
        {
            var p=cell+d;
            if(p==Yummn.icePillar||EnemyAt(p)?.yummnFrozen==true)return true;
            if(p.x>=1&&p.x<=5&&p.y>=1&&p.y<=5&&walls[p.x,p.y])return true;
        }
        return false;
    }
    private static readonly Vector2Int[] YummnDirections={Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
    private bool YummnEmpty(Vector2Int p)=>!IsHardBlocked(p)&&p!=katePos&&EnemyAt(p)==null;
    private Vector2Int ShadowDestination(KaitDirection dir)
    {
        for(var p=katePos+Delta(dir);Inside(p);p+=Delta(dir))if(YummnEmpty(p)&&IsYummnShadow(p))return p;
        return YummnRun.NoCell;
    }
    private KaitEnemy FirstYummnRayEnemy(KaitDirection dir,int range=6)
    {
        for(int i=1;i<=range;i++){var p=katePos+Delta(dir)*i;if(IsHardBlocked(p))break;var e=EnemyAt(p);if(e!=null)return e;}
        return null;
    }
    // Read-only threat simulation: no RNG, heatmap, merge counters or equipment activation.
    private bool YummnThreatCanChange(KaitDirection direction)
    {
        var delta=Delta(direction);bool horizontal=delta.x!=0,reverse=delta.x>0||delta.y>0;
        for(int line=0;line<ThreatSize;line++)
        {
            bool hole=false;int last=0;
            for(int i=0;i<ThreatSize;i++)
            {
                int j=reverse?ThreatSize-1-i:i;var p=horizontal?new Vector2Int(j,line):new Vector2Int(line,j);
                if(IsThreatPillar(p)){if(HasPassive(KaitPassive.Passwall))continue;hole=false;last=0;continue;}
                int v=threat[p.x,p.y];if(v==0){hole=true;continue;}
                if(hole||v==last)return true;last=v;
            }
        }
        return false;
    }
    private bool ValidateYummnAction(KaitDirection dir,out YummnActionContext ctx,out string error)
    {
        ctx=new YummnActionContext{actionId=Yummn.actionId+1,phaseAtStart=KiPhase,kiBefore=Ki,direction=dir,
            startCell=katePos,startedInShadow=IsYummnShadow(katePos),targetCell=YummnRun.NoCell,iceAtStart=Yummn.icePillar,darknessAtStart=Yummn.darkness,decoyAtStart=Yummn.decoy};
        error=null;
        foreach(var def in YummnCatalog.Cards)if(Yummn.prepared.Contains(def.id))
        {
            if(!skills.Contains(def.skill)){error="准备的技能已被替换";return false;}
            ctx.plannedSkills.Add(def.id);ctx.totalKiCost+=def.kiExtraCost;
            if(def.actionOverride!=null)ctx.actionOverride=def.actionOverride;
        }
        ctx.skillKiCost=ctx.totalKiCost;
        if(!Yummn.rules.Is082)ctx.totalKiCost+=KiPhase==YummnPhase.Burst?1:0;
        if(ctx.totalKiCost>Ki){error="气不足：本次需要 "+ctx.totalKiCost+" 气";return false;}
        ctx.threatDirection=HasPassive(KaitPassive.ReverseGravity)?Opposite(dir):dir;
        foreach(var card in YummnCatalog.Cards)
            if(card.kind==KaitAbilityKind.Active?skills.Contains(card.skill):HasPassive(card.passive))ctx.equipmentSnapshot.Add(card.id);
        var next=repoolCellTarget??(katePos+Delta(dir));
        switch(ctx.actionOverride)
        {
            case "Thunder":ctx.targetCell=katePos;break;
            case "Mirror":if(!YummnEmpty(next))error="残影需要空格";else ctx.targetCell=next;break;
            case "Sonic":if(!Inside(next))error="目标不在战场";else ctx.targetCell=next;break;
            case "CommandAct":if(EnemyAt(next)==null)error="请选择敌人";else ctx.targetCell=next;break;
            case "MageHand":if(next.x<0||next.y<0||next.x>=ThreatSize||next.y>=ThreatSize||IsThreatPillar(next)||threat[next.x,next.y]==0)error="请选择副盘数字";else ctx.targetCell=next;break;
            case "AirRay":
            case "Water":if(FirstYummnRayEnemy(dir)==null)error="前方没有可见敌人";break;
            case "Echo":ctx.targetCell=EchoDestination(dir);if(ctx.targetCell.x<0)error="该方向没有可用残影";break;
            case "Precise":if(!YummnEmpty(next))error="前方无法停留";else ctx.targetCell=next;break;
            case "Heal":ctx.isWait=true;break;
            case "Phantom":ctx.targetCell=PhantomDestination(dir);if(ctx.targetCell==katePos)error="没有可停留空格";break;
            case "Decoy":if(!YummnEmpty(next))error="诱饵需要空格";else ctx.targetCell=next;break;
            case "Winter":if(FirstYummnRayEnemy(dir,4)==null)error="前方4格没有目标";break;
            case "Ice":if(!YummnEmpty(next)||PendingBossCell==next)error="不能在此处升起冰柱";else ctx.targetCell=next;break;
            case "Darkness":if(!IsLegalSkillCell(KaitSkill.Darkness,next))error="请选择主棋盘内的格子";else ctx.targetCell=next;break;
            case "Shadow":
                ctx.targetCell=repoolCellTarget.HasValue?next:ShadowDestination(dir);
                if(ctx.targetCell.x<0||!YummnEmpty(ctx.targetCell)||!IsYummnShadow(ctx.targetCell))error="请选择空暗影格";
                break;
            default:if(IsHardBlocked(next)&&!YummnThreatCanChange(ctx.threatDirection)&&!(Yummn.rules.Is082&&Yummn.rules.Supply==YummnTileSupplyMode.EveryAction))error="两盘均无法响应";break;
        }
        if(error==null&&Yummn.rules.Is082)PlanYummn082Movement(ctx);
        return error==null;
    }
    public KaitTurnResult TryYummnDirection(KaitDirection dir)
    {
        var r=new KaitTurnResult();if(ended){r.message="本局已结束";return r;}
        // Candidate equipment is used only for validation, then committed exactly once.
        var inactive=new HashSet<string>(inactiveAbilities);var retired=new HashSet<string>(retiredAbilities);var copy=previousCopiedPassive;
        ActivateBuildForInput();SyncYummnCapacity();YummnActionContext a;string error;
        bool valid=ValidateYummnAction(dir,out a,out error);
        if(!valid){inactiveAbilities.UnionWith(inactive);retiredAbilities.UnionWith(retired);previousCopiedPassive=copy;r.message=error;return r;}
        Yummn.actionId=a.actionId;Yummn.metrics.actions++;r.yummnAction=a;r.valid=r.turnComplete=true;
        r.globalDirection=r.kaitDirection=dir;currentGlobalDirection=currentDirection=dir;actualThreatDirection=a.threatDirection;
        r.threatBefore=CopyThreat();turnTriggers.Clear();
        BeginYummnRoot();BeginYummnRangeTracking();
        if(!Yummn.rules.Is082){Yummn.ki-=a.totalKiCost;Yummn.metrics.kiSpent+=a.totalKiCost;}
        foreach(var id in a.plannedSkills)YummnTrigger(id,r);
        if(a.plannedSkills.Contains("yummn.M02"))Yummn.defense=true;
        if(Yummn.rules.Is082)CommitYummn082Cost(r);
        PrepareYummnWaitDefenses(a);
        ResolveYummnPlayerAction(r);
        ResolveYummnRangeEntries(r);
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="ThreatBegin",actionId=a.actionId});
        if(!ended&&!a.isWait&&!repoolCellTarget.HasValue)r.merges.AddRange(MoveThreat(actualThreatDirection,r.threatMotions));
        r.threatChanged=!ThreatEquals(r.threatBefore,threat);threatChangedThisTurn=r.threatChanged;
        bool baseMerged=r.merges.Count>0;
        if(!ended&&a.actionOverride!="MageHand"){ResolveYummnMergeBatch(r,false);if(baseMerged)ResolveYummnPendulum(r);}
        if(Yummn081){r.yummnThreatAfterMerge=CopyThreat();r.yummnInitialMotionCount=r.threatMotions.Count;}
        a.finalCell=katePos;r.slideDistance=r.katePath.Count;
        Yummn.prepared.Clear();
        if(Yummn.rules.Is082)ResolveYummn082Tail(r);
        else if(!ended&&Yummn081)ResolveYummn081Tail(r);
        else if(!ended)
        {
            a.suppressedTwo=katePos!=a.startCell&&!a.didAttack&&HasPassive(KaitPassive.PassWithoutTrace);
            if(a.suppressedTwo){Yummn.metrics.suppressedTwos++;YummnTrigger("S04",r);}
            else {var p=SpawnThreatTwoForTurn(r);if(p.x>=0){r.newThreatCells.Add(p);Yummn.metrics.naturalTwos++;}}
            ResolveOldNewsArchive(r);
            if(a.didAttack||!IsYummnShadow(katePos))Yummn.cloak=false;
            else if(HasPassive(KaitPassive.ShadowCloak)){Yummn.cloak=true;YummnTrigger("S03",r);}
            Yummn.tranquility=a.phaseAtStart==YummnPhase.Exhausted&&!a.didAttack&&katePos!=a.startCell&&HasPassive(KaitPassive.Tranquility);
            if(a.phaseAtStart==YummnPhase.Exhausted){a.enemyPhase=true;ResolveYummnEnemyPhase(r);}
            if(!ended&&(a.phaseAtStart==YummnPhase.Exhausted||a.killIds.Count>0)){a.spawnChecked=true;ResolveYummnRifts(r);}
            if(!ended&&bossPending){SpawnShieldKnight(r);if(r.bossSpawned)Yummn.metrics.bossCreatedAction=a.actionId;}
            if(!ended&&a.phaseAtStart==YummnPhase.Exhausted){int gained=GainYummnKi(Yummn.profile.recoveryKi,r,"Recovery");Yummn.metrics.recoveryKi+=gained;}
            if(!ended)FinishYummnPhase(r);
        }
        a.kiAfter=Ki;a.phaseAtEnd=KiPhase;a.finalCell=katePos;r.threatAfter=CopyThreat();
        foreach(var m in r.merges){if(m.resultValue==32&&Yummn.metrics.first32Action<0)Yummn.metrics.first32Action=a.actionId;if(m.resultValue==128&&Yummn.metrics.first128Action<0)Yummn.metrics.first128Action=a.actionId;}
        foreach(var trigger in r.passiveTriggers)
        {
            var def=KaitAbilityCatalog.Get(trigger.passive);if(def==null||YummnCatalog.IsMonk(def))continue;
            var counts=Yummn.metrics.cardTriggers;counts[def.id]=counts.TryGetValue(def.id,out var n)?n+1:1;
        }
        if(a.phaseAtStart==YummnPhase.Burst){Yummn.metrics.burstLength++;Yummn.metrics.longestBurst=Mathf.Max(Yummn.metrics.longestBurst,Yummn.metrics.burstLength);Yummn.metrics.exhaustedLength=0;}
        else {Yummn.metrics.exhaustedLength++;Yummn.metrics.longestExhausted=Mathf.Max(Yummn.metrics.longestExhausted,Yummn.metrics.exhaustedLength);Yummn.metrics.burstLength=0;}
        StampYummnRoot(r);Yummn.history.Add(a);turn++;PrepareThreatTwoPreview();return r;
    }
    private void FinishYummnPhase(KaitTurnResult r)
    {
        if(KiPhase==YummnPhase.Burst&&(ExactKi==0||Yummn.rules.Is082&&r.yummnAction.reachedZeroKi))
        {
            Yummn.phase=YummnPhase.Exhausted;Yummn.exhaustionCycleId++;Yummn.metrics.exhaustionCycles++;
            if(HasPassive(KaitPassive.PerfectSelf)){Yummn.metrics.perfectKi+=GainYummnKi(1,r,"PerfectSelf");YummnTrigger("M05",r);}
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="Exhausted",actionId=Yummn.actionId});
        }
        else if(KiPhase==YummnPhase.Exhausted&&!r.yummnAction.kiGuardTriggered&&Ki>=(Yummn.rules.ExhaustionNeedsFullKi?Yummn.profile.maxKi:1))
        {
            Yummn.phase=YummnPhase.Burst;
            if(HasPassive(KaitPassive.Wholeness)){int heal=Mathf.Min(1,KateMaxHp-kateHp);kateHp+=heal;Yummn.metrics.heals+=heal;YummnTrigger("O05",r);RepoolStatus(r,"Heal",katePos,heal);}
            r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="Burst",actionId=Yummn.actionId});
        }
    }
    private int GainYummnKi(int amount,KaitTurnResult r,string cause)
    {
        int gain=Mathf.Min(amount,Yummn.profile.maxKi-Ki);Yummn.ki+=gain;
        if(Ki>=Yummn.profile.maxKi)Yummn.kiTenths=0;
        if(Yummn.rules.Is082)Yummn.metrics.overflowKi+=amount-gain;
        if(gain>0)r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Resource,amount=gain,status=cause,actionId=Yummn.actionId,to=katePos});
        return gain;
    }
    private void YummnTrigger(string id,KaitTurnResult r)
    {
        var def=YummnCatalog.Get(id);id=def?.id??id;
        var counts=Yummn.metrics.cardTriggers;counts[id]=counts.TryGetValue(id,out var n)?n+1:1;
        r.yummnEvents.Add(new YummnCombatEvent{kind=YummnEventKind.Status,status="CardTriggered",cardId=id,displayName=def?.nameZh,actionId=Yummn.actionId});
        if(def!=null&&def.kind==KaitAbilityKind.Passive)TriggerPassive(def.passive,r,YummnRun.NoCell,katePos,def.nameZh);
    }
    private bool PreventYummnHit(KaitEnemy e,KaitIntent i,KaitTurnResult r)=>PreventYummnDamage(e,i,r);
}
