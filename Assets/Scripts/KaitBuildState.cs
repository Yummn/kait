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
    private int lockedBaseMomentum;
    private readonly List<Vector2Int> nextTwoPriority = new List<Vector2Int>();

    private void ResetBuildState()
    {
        rewardQueue.Clear(); inactiveAbilities.Clear(); retiredAbilities.Clear(); turnTriggers.Clear(); chainTriggers.Clear();
        packsWithoutRare=nextRewardId=lockedBaseMomentum=0; copiedPassive=previousCopiedPassive=KaitPassive.None;
        mirrorNextRift=tombArmed=smiteArmed=specterReady=false; nextTwoPriority.Clear();
    }
    public bool IsAbilityPending(KaitAbilityDef def) => def!=null && inactiveAbilities.Contains(def.id);
    private string SkillAbilityId(KaitSkill skill)=>(IsReynard?ReynardCatalog.Get(skill):KaitAbilityCatalog.Get(skill))?.id??"active."+skill;
    private string PassiveAbilityId(KaitPassive passive)=>(IsReynard?ReynardCatalog.Get(passive):IsYummn?YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Passive&&d.passive==passive):null)?.id??KaitAbilityCatalog.Get(passive)?.id??"passive."+passive;
    public bool IsSkillActive(KaitSkill skill) => (skills.Contains(skill) && !inactiveAbilities.Contains(SkillAbilityId(skill))) || retiredAbilities.Contains(SkillAbilityId(skill));
    private bool RawPassiveActive(KaitPassive passive) =>
        (passives.Contains(passive) && !inactiveAbilities.Contains(PassiveAbilityId(passive))) || retiredAbilities.Contains(PassiveAbilityId(passive));
    private bool IsPassiveActive(KaitPassive passive)
    {
        if(!RawPassiveActive(passive))return false;
        if(!IsYummn)return true;
        if(passive==KaitPassive.OpenHand)return true;
        if(passive==KaitPassive.ShatteringPalm)return true;
        if(passive==KaitPassive.FollowThrough)return true;
        return YummnCatalog.Pool().Exists(d=>d.kind==KaitAbilityKind.Passive&&d.passive==passive);
    }
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
        bool curse=HasCurseSource(), repeatable=HasRepeatableMagic(), cooldown=HasCooldownAbility();
        return (IsReynard ? ReynardCatalog.Pool() : IsYummn ? YummnCatalog.Pool() : KaitAbilityCatalog.DefaultPool()).FindAll(d=> (IsYummn?!d.experimental:true) && YummnPrerequisite(d) && (!IsReynard || ReynardPrerequisite(d)) &&
            true &&
            !(d.kind==KaitAbilityKind.Active?skills.Contains(d.skill):passives.Contains(d.passive)) &&
            (pack==null || !pack.Contains(d)) &&
            (!(d.skill==KaitSkill.RelentlessHex || d.passive==KaitPassive.HexArmor || d.passive==KaitPassive.MasterHex ||
                d.passive==KaitPassive.MaddeningHex || d.passive==KaitPassive.Lifedrinker) || curse) &&
            (d.passive!=KaitPassive.TwinSigil || repeatable) &&
            (!(d.passive==KaitPassive.Devil||d.passive==KaitPassive.BladeCovenant) || cooldown) &&
            (d.passive!=KaitPassive.Simulacrum || passives.Exists(p=>KaitAbilityCatalog.Get(p)?.copyable==true)) &&
            (d.passive!=KaitPassive.Passwall || config.enableThreatPillars));
    }
    public bool HasCurseSource()=>passives.Contains(KaitPassive.HexBlade)||skills.Contains(KaitSkill.HungerOfHadar);
    public bool HasRepeatableMagic()=>passives.Contains(KaitPassive.EldritchBlast)||skills.Contains(KaitSkill.HungerOfHadar);
    public bool HasCooldownAbility()
    {
        foreach(var s in skills)if((KaitAbilityCatalog.Get(s)?.cooldown??0)>0)return true;
        foreach(var p in passives)if((KaitAbilityCatalog.Get(p)?.cooldown??0)>0)return true;
        return false;
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
        int rewardValue=IsYummn?Yummn.rules.RewardMergeValue:16;
        if(merge.resultValue!=rewardValue || (merge.sourceValue!=0 && merge.sourceValue!=rewardValue/2)) return;
        var pack=new KaitRewardPack { id=++nextRewardId,sourceTurn=turn,sourceMergeCell=merge.threatCell,characterId=Character,rulesProfileId=RulesProfileId,cardPoolVersion=IsReynard?ReynardCatalog.Version:IsYummn?YummnCatalog.Version:KaitAbilityCatalog.Version };
        pack.generationSeed=replaySeed;
        FillReward(pack); rewardQueue.Enqueue(pack);
    }
    public bool CanSelectReward => !ended && !chainActive && CurrentReward!=null;
    private bool ResolveRewardSelection(int index,int replaceSlot=-1,KaitPassive copy=KaitPassive.None)
    {
        if(!CanSelectReward || index<0 || index>=CurrentReward.choices.Count) return false;
        var def=CurrentReward.choices[index];
        if(!YummnPrerequisite(def)||IsReynard&&!ReynardPrerequisite(def))return false;
        if(def.kind==KaitAbilityKind.Active ? skills.Contains(def.skill) : passives.Contains(def.passive)) return false;
        if(def.passive==KaitPassive.Simulacrum && (!passives.Contains(copy) || KaitAbilityCatalog.Get(copy)?.copyable!=true)) return false;
        return ResolveSharedReward(def,replaceSlot,copy);
    }
    public string YummnReplacementConsequences(int index,int slot)
    {
        if(!IsYummn)return null;
        if(CurrentReward==null||index<0||index>=CurrentReward.choices.Count||slot<0||slot>=EquippedCardCount)return null;
        var d=CurrentReward.choices[index];var aa=new List<KaitSkill>(skills);var pp=new List<KaitPassive>(passives);
        if(slot<aa.Count)aa.RemoveAt(slot);else pp.RemoveAt(slot-aa.Count);
        if(d.kind==KaitAbilityKind.Active)aa.Add(d.skill);else pp.Add(d.passive);
        var names=new List<string>();
        foreach(var p in pp){var def=KaitAbilityCatalog.Get(p);if(YummnMissingRequirement(def,skills,passives)==null&&YummnMissingRequirement(def,aa,pp)!=null)names.Add(def.nameZh);}
        return names.Count==0?null:string.Join("、",names)+"将失去前置";
    }
    public bool SkipReward() { if(!CanSelectReward) return false; rewardQueue.Dequeue();RecordReplay("skip",0,0,0); return true; }
    public bool RerollReward()
    {
        if(!CanSelectReward || !HasPassive(KaitPassive.LuckBlade) || CurrentReward.rerolled) return false;
        var pack=CurrentReward; FillReward(pack); pack.rerolled=true; RecordReplay("reroll",0,0,0); return true;
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
