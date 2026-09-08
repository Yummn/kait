using System;
using System.Collections.Generic;
using UnityEngine;

// Build mutations are staged; an in-flight global turn always keeps its original rules.
public sealed partial class KaitRun
{
    public readonly Queue<KaitRewardPack> rewardQueue = new Queue<KaitRewardPack>();
    public int packsWithoutRare { get; private set; }
    public KaitRewardPack CurrentReward => ended || rewardQueue.Count==0 ? null : rewardQueue.Peek();
    private readonly HashSet<string> inactiveAbilities = new HashSet<string>();
    private readonly HashSet<string> retiredAbilities = new HashSet<string>();
    private readonly HashSet<string> turnTriggers = new HashSet<string>(), chainTriggers = new HashSet<string>();
    private int nextRewardId;
    public KaitPassive copiedPassive { get; private set; }
    private KaitPassive previousCopiedPassive;
    public KaitDirection actualThreatDirection { get; private set; }
    public bool mirrorNextRift { get; private set; }
    public bool tombArmed { get; private set; }
    public bool smiteArmed { get; private set; }
    public bool specterReady { get; private set; }
    private bool transferCurse;
    private int lockedBaseMomentum;
    private readonly List<Vector2Int> nextTwoPriority = new List<Vector2Int>();

    private void ResetBuildState()
    {
        rewardQueue.Clear(); inactiveAbilities.Clear(); retiredAbilities.Clear(); turnTriggers.Clear(); chainTriggers.Clear();
        packsWithoutRare=nextRewardId=lockedBaseMomentum=0; copiedPassive=previousCopiedPassive=KaitPassive.None;
        mirrorNextRift=tombArmed=smiteArmed=specterReady=transferCurse=false; nextTwoPriority.Clear();
    }
    public bool IsAbilityPending(KaitAbilityDef def) => def!=null && inactiveAbilities.Contains(def.id);
    public bool IsSkillActive(KaitSkill skill) => (skills.Contains(skill) && !inactiveAbilities.Contains("active."+skill)) || retiredAbilities.Contains("active."+skill);
    private bool IsPassiveActive(KaitPassive passive) =>
        (passives.Contains(passive) && !inactiveAbilities.Contains("passive."+passive)) || retiredAbilities.Contains("passive."+passive);
    public int PassiveCopies(KaitPassive passive)
    {
        int count=IsPassiveActive(passive)?1:0;
        if(passive!=KaitPassive.Simulacrum && IsPassiveActive(KaitPassive.Simulacrum))
        {
            KaitPassive effect=inactiveAbilities.Contains("passive.Simulacrum")?previousCopiedPassive:copiedPassive;
            if(effect==passive) count++;
        }
        return count;
    }
    private void ActivateBuildForInput()
    {
        inactiveAbilities.Clear(); retiredAbilities.Clear(); previousCopiedPassive=copiedPassive;
    }
    public List<KaitAbilityDef> EligibleAbilities(List<KaitAbilityDef> pack=null)
    {
        bool curse=skills.Contains(KaitSkill.HexCurse)||(pack!=null && pack.Exists(d=>d.skill==KaitSkill.HexCurse));
        return KaitAbilityCatalog.All.FindAll(d=> !d.experimental &&
            !(d.kind==KaitAbilityKind.Active?skills.Contains(d.skill):passives.Contains(d.passive)) &&
            (pack==null || !pack.Contains(d)) &&
            (!(d.skill==KaitSkill.RelentlessHex || d.passive==KaitPassive.HexArmor || d.passive==KaitPassive.MasterHex ||
                d.passive==KaitPassive.MaddeningHex || d.passive==KaitPassive.Lifedrinker) || curse) &&
            (d.passive!=KaitPassive.Devil || skills.Count>=2) &&
            (d.passive!=KaitPassive.BladeCovenant || skills.Count>0) &&
            (d.passive!=KaitPassive.Simulacrum || passives.Exists(p=>KaitAbilityCatalog.Get(p)?.copyable==true)) &&
            (d.passive!=KaitPassive.Passwall || config.enableThreatPillars));
    }
    private KaitAbilityDef WeightedAbility(List<KaitAbilityDef> pool)
    {
        int total=0;
        for(int r=0;r<3;r++) if(pool.Exists(d=>(int)d.rarity==r)) total+=r==0?55:r==1?35:10;
        if(total==0) return null;
        int roll=random.Next(total), rarity=0;
        for(;rarity<3;rarity++)
        {
            if(!pool.Exists(d=>(int)d.rarity==rarity)) continue;
            roll-=rarity==0?55:rarity==1?35:10;
            if(roll<0) break;
        }
        var selected=pool.FindAll(d=>(int)d.rarity==rarity);
        return selected[random.Next(selected.Count)];
    }
    private void FillReward(KaitRewardPack pack)
    {
        pack.choices.Clear();
        if(packsWithoutRare>=3)
        {
            var rare=EligibleAbilities().FindAll(d=>d.rarity==KaitRarity.Rare);
            if(rare.Count>0) pack.choices.Add(rare[random.Next(rare.Count)]);
        }
        for(int i=pack.choices.Count;i<3;i++)
        {
            var pool=EligibleAbilities(pack.choices);
            if(pack.choices.Exists(d=>d.rarity==KaitRarity.Rare)) pool.RemoveAll(d=>d.rarity==KaitRarity.Rare);
            bool active=pack.choices.Exists(d=>d.kind==KaitAbilityKind.Active), passive=pack.choices.Exists(d=>d.kind==KaitAbilityKind.Passive);
            KaitAbilityKind? required=!active?KaitAbilityKind.Active:!passive?KaitAbilityKind.Passive:(KaitAbilityKind?)null;
            if(required.HasValue && pool.Exists(d=>d.kind==required.Value)) pool=pool.FindAll(d=>d.kind==required.Value);
            var picked=WeightedAbility(pool); if(picked==null) break; pack.choices.Add(picked);
        }
        packsWithoutRare=pack.choices.Exists(d=>d.rarity==KaitRarity.Rare)?0:packsWithoutRare+1;
    }
    public void EnqueueMergeReward(KaitMergeEvent merge)
    {
        if(merge.resultValue!=32 || (merge.sourceValue!=0 && merge.sourceValue!=16)) return;
        var pack=new KaitRewardPack { id=++nextRewardId,sourceTurn=turn,sourceMergeCell=merge.threatCell };
        FillReward(pack); rewardQueue.Enqueue(pack);
    }
    public bool CanSelectReward => !ended && !chainActive && CurrentReward!=null;
    public bool SelectReward(int index,int replaceSlot=-1,KaitPassive copy=KaitPassive.None)
    {
        if(!CanSelectReward || index<0 || index>=CurrentReward.choices.Count) return false;
        var def=CurrentReward.choices[index];
        if(def.kind==KaitAbilityKind.Active ? skills.Contains(def.skill) : passives.Contains(def.passive)) return false;
        if(def.passive==KaitPassive.Simulacrum && (!passives.Contains(copy) || KaitAbilityCatalog.Get(copy)?.copyable!=true)) return false;
        int count=def.kind==KaitAbilityKind.Active?skills.Count:passives.Count;
        if(replaceSlot < -1 || replaceSlot>=count || count>=3 && replaceSlot<0) return false;
        if(replaceSlot>=0)
        {
            string old=def.kind==KaitAbilityKind.Active?"active."+skills[replaceSlot]:"passive."+passives[replaceSlot];
            if(!inactiveAbilities.Remove(old)) retiredAbilities.Add(old);
            if(def.kind==KaitAbilityKind.Active) skills[replaceSlot]=def.skill; else passives[replaceSlot]=def.passive;
        }
        else if(def.kind==KaitAbilityKind.Active) skills.Add(def.skill); else passives.Add(def.passive);
        inactiveAbilities.Add(def.id);
        if(def.kind==KaitAbilityKind.Active) skillCooldowns[def.skill]=0;
        if(def.passive==KaitPassive.Simulacrum) copiedPassive=copy;
        rewardQueue.Dequeue(); return true;
    }
    public bool SkipReward() { if(!CanSelectReward) return false; rewardQueue.Dequeue(); return true; }
    public bool RerollReward()
    {
        if(!CanSelectReward || !HasPassive(KaitPassive.LuckBlade) || CurrentReward.rerolled) return false;
        var pack=CurrentReward; FillReward(pack); pack.rerolled=true; return true;
    }
    public static int CalculateSpeed(int baseSpeed,IEnumerable<KaitSpeedModifier> modifiers)
    {
        int add=0,multiplier=1;
        foreach(var modifier in modifiers) { if(modifier==KaitSpeedModifier.AddOne) add++; else multiplier*=2; }
        return (baseSpeed+add)*multiplier;
    }
    public List<Vector2Int> ArchiveTargets()
    {
        var list=new List<Vector2Int>();
        if(!HasPassive(KaitPassive.OldNewsArchive)) return list;
        for(int y=0;y<ThreatSize;y++) for(int x=0;x<ThreatSize;x++) if(threat[x,y]==2) list.Add(new Vector2Int(x,y));
        if(list.Count<5) { list.Clear(); return list; }
        list.Sort((a,b)=>threatTwoBirth[a.x,a.y]!=threatTwoBirth[b.x,b.y]?threatTwoBirth[a.x,a.y].CompareTo(threatTwoBirth[b.x,b.y]):(a.y*ThreatSize+a.x).CompareTo(b.y*ThreatSize+b.x));
        return list.GetRange(0,2);
    }
    private void ResolveBag(IEnumerable<KaitMergeEvent> merges,KaitTurnResult result)
    {
        if(!HasPassive(KaitPassive.BagHolding)) return;
        foreach(var merge in merges) for(int copy=0;copy<PassiveCopies(KaitPassive.BagHolding);copy++)
        {
            Vector2Int found=new Vector2Int(-1,-1),delta=Delta(merge.actualThreatDirection); int distance=int.MaxValue;
            for(int y=0;y<ThreatSize;y++) for(int x=0;x<ThreatSize;x++)
            {
                if(threat[x,y]!=(merge.sourceValue>0?merge.sourceValue:merge.resultValue/2)) continue;
                int dot=x*delta.x+y*delta.y;
                if(dot<distance) { distance=dot;found=new Vector2Int(x,y); }
            }
            if(found.x<0) continue;
            threat[found.x,found.y]=threatTwoBirth[found.x,found.y]=0;
            TriggerPassive(KaitPassive.BagHolding,result,found,MapThreatToBattle(found),"异次元袋收纳了后方数字");
        }
    }
}
