using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    [Serializable] public sealed class ReplayStep { public string op, data; public int a,b,c; }
    [Serializable] public sealed class ReplaySave
    {
        public int format=1,seed;public KaitCharacter characterId;public string rulesProfileId,cardPoolVersion;
        public KaitBalanceConfig balance;public List<ReplayStep> steps;
        public YummnRulesSnapshot yummnRules;
    }
    private int replaySeed;
    private bool replaying;
    private string replayInitialBalance;
    private readonly List<ReplayStep> replaySteps=new List<ReplayStep>();
    public Action StateCommitted;
    private void ResetReplay(int seed){replaySeed=seed;replaySteps.Clear();replayInitialBalance=JsonUtility.ToJson(config);}
    public void RecordSettingsChange()
    {
        if(replaying)return;
        replaySteps.Add(new ReplayStep{op="settings",data=JsonUtility.ToJson(config)});StateCommitted?.Invoke();
    }
    private void RecordReplay(string op,int a,int b,int c)
    {if(replaying)return;replaySteps.Add(new ReplayStep{op=op,a=a,b=b,c=c});StateCommitted?.Invoke();}
    public KaitTurnResult TryGlobalInput(KaitDirection direction)
    {var r=ResolveGlobalInput(direction);if(r.valid)RecordReplay("direction",(int)direction,0,0);return r;}
    public KaitTurnResult ContinueChain(KaitDirection direction)
    {var r=ResolveChainInput(direction);if(r.valid)RecordReplay("chain",(int)direction,0,0);return r;}
    public bool TryUseSkill(KaitSkill skill,int targetEnemyId,out string message)
    {bool ok=ResolveSkill(skill,targetEnemyId,out message);if(ok)RecordReplay("skill",(int)skill,targetEnemyId,0);return ok;}
    public bool SelectReward(int index,int replaceSlot=-1,KaitPassive copy=KaitPassive.None)
    {bool ok=ResolveRewardSelection(index,replaceSlot,copy);if(ok)RecordReplay("reward",index,replaceSlot,(int)copy);return ok;}
    public string SaveReplay()=>JsonUtility.ToJson(new ReplaySave{seed=replaySeed,characterId=Character,rulesProfileId=RulesProfileId,cardPoolVersion=IsYummn?YummnCatalog.Version:"0.6.1",balance=JsonUtility.FromJson<KaitBalanceConfig>(replayInitialBalance),steps=replaySteps,yummnRules=IsYummn?Yummn.rules:null});
    public bool RestoreReplay(string json)
    {
        ReplaySave save;
        try { save=JsonUtility.FromJson<ReplaySave>(json); } catch(ArgumentException) { return false; }
        if(save==null||save.format!=1||save.balance==null||save.steps==null)return false;
        var snapshot=save.yummnRules;
        // Unity may materialize a missing inline serializable class as an empty
        // object rather than null when loading a save written before this field.
        if(save.characterId==KaitCharacter.Yummn&&(snapshot==null||string.IsNullOrEmpty(snapshot.Version))&&save.rulesProfileId==YummnRulesProfile.LegacyVersion)snapshot=YummnRulesSnapshot.OldV08();
        if(save.characterId==KaitCharacter.Yummn&&(snapshot==null||!snapshot.Valid))return false;
        string expected=save.characterId==KaitCharacter.Yummn?snapshot.Version:"Kait.0.6.1";
        if(!Enum.IsDefined(typeof(KaitCharacter),save.characterId)||save.rulesProfileId!=expected||save.cardPoolVersion!=(save.characterId==KaitCharacter.Yummn?YummnCatalog.Version:"0.6.1"))return false;
        replaying=true;
        try
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(save.balance),config);SelectCharacter(save.characterId,save.seed,snapshot);
            foreach(var s in save.steps)
            {
                bool ok=false;
                switch(s.op)
                {
                    case "direction":ok=TryGlobalInput((KaitDirection)s.a).valid;break;
                    case "chain":ok=ContinueChain((KaitDirection)s.a).valid;break;
                    case "skill":ok=TryUseSkill((KaitSkill)s.a,s.b,out _);break;
                    case "reward":ok=SelectReward(s.a,s.b,(KaitPassive)s.c);break;
                    case "skip":ok=SkipReward();break;
                    case "cellskill":ok=TryUseSkillAt((KaitSkill)s.a,new Vector2Int(s.b,s.c),out _);break;
                    case "wait":ok=TryYummnWait().valid;break;
                    case "shadow":ok=TryShadowStep();break;
                    case "reroll":ok=RerollReward();break;
                    case "settings":JsonUtility.FromJsonOverwrite(s.data,config);ok=true;break;
                }
                if(!ok)return false;
            }
            replaySteps.AddRange(save.steps);return true;
        }
        finally{replaying=false;}
    }
    public KaitTurnResult TryYummnCommand(KaitDirection direction,int globalTurnId,int actionIndex)
    {
        if(!IsYummn||globalTurnId!=turn||actionIndex!=ActionIndex)return new KaitTurnResult{message="过期行动"};
        return TryGlobalInput(direction);
    }
}
