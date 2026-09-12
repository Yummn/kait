using System;
using UnityEngine;

public enum YummnTileSupplyMode { KillOnly, SkipStationaryPunch, EveryAction, EffectiveMove }
[Flags] public enum YummnEnemyPhaseReason { None=0, Kill=1, Exhaustion=2, Attack=4, Movement=8 }
public enum YummnMovementCostMode { PerCell, FixedOne }

// Menu presets are copied only when creating a run. No public setters: neither
// a changed preference nor a replayed general setting can hot-swap these rules.
[Serializable] public sealed class YummnRulesSnapshot
{
    [SerializeField] private string version;
    [SerializeField] private YummnTileSupplyMode supply;
    [SerializeField] private bool spawnFromEight;
    [SerializeField] private bool exhaustedMoveSupply;
    // Missing fields in existing saves stay false: those runs retain 3 Ki / +2.
    [SerializeField] private bool fiveKi;
    [SerializeField] private bool killKiOne;
    [SerializeField] private bool bossLine;
    // Absent in old replays: preserve their pre-hit/stationary-punch rule.
    [SerializeField] private bool actualMoveSupply;
    public const string CurrentVersion="yummn-0.8.2";
    [SerializeField] private int maxKi082,killKi082;
    [SerializeField] private YummnMovementCostMode movementCostMode;
    [SerializeField] private bool attackAdvancesEnemyPhase,exhaustionNeedsFullKi;
    [SerializeField] private bool attackCostsTenth,movementAdvancesEnemyPhase;
    public bool AttackCostsTenth=>Is082&&attackCostsTenth;
    [SerializeField] private bool attackCostsOne;
    public bool AttackCostsOne=>Is082&&attackCostsOne;
    [SerializeField] private bool kiGuard;
    [SerializeField] private int rewardMergeValue;
    // Missing in old saves: retain their 32-point reward cadence.
    public int RewardMergeValue=>Is082&&rewardMergeValue==16?16:32;
    public bool KiGuard=>Is082&&kiGuard;
    public int AttackCostTenths=>AttackCostsOne?10:AttackCostsTenth?1:0;
    public bool MovementAdvancesEnemyPhase=>Is082&&movementAdvancesEnemyPhase;
    public bool Is082=>version==CurrentVersion;
    public YummnMovementCostMode MovementCostMode=>movementCostMode;
    public bool AttackAdvancesEnemyPhase=>Is082&&attackAdvancesEnemyPhase;
    public bool ExhaustionNeedsFullKi=>!Is082||exhaustionNeedsFullKi;
    public bool ActualMoveSupply=>!Is082&&!Legacy&&supply==YummnTileSupplyMode.SkipStationaryPunch&&actualMoveSupply;
    public bool BossLine=>!Legacy&&bossLine;
    public int MaxKi=>Is082?maxKi082:!Legacy&&fiveKi?5:3;
    public int KillKi=>Is082?killKi082:!Legacy&&killKiOne?1:2;
    public bool ExhaustedMoveSupply=>!Is082&&exhaustedMoveSupply&&!Legacy;
    public string Version=>version;
    public YummnTileSupplyMode Supply=>supply;
    public bool SpawnFromEight=>spawnFromEight;
    public bool Legacy=>version==YummnRulesProfile.LegacyVersion;
    public bool Valid=>Is082?(rewardMergeValue==0||rewardMergeValue==16||rewardMergeValue==32)&&maxKi082>=3&&maxKi082<=9&&killKi082>=1&&killKi082<=3&&Enum.IsDefined(typeof(YummnMovementCostMode),movementCostMode)&&Enum.IsDefined(typeof(YummnTileSupplyMode),supply):(version==YummnRulesProfile.Version||Legacy)&&supply!=YummnTileSupplyMode.EffectiveMove&&Enum.IsDefined(typeof(YummnTileSupplyMode),supply);
    public string ScoreKey=>Is082?$"{version}.Ki{MaxKi}.{MovementCostMode}.Kill{KillKi}.Attack{attackAdvancesEnemyPhase}.Full{exhaustionNeedsFullKi}.{supply}.Spawn{(spawnFromEight?8:4)}"+(attackCostsOne?".AttackOne":"")+(attackCostsTenth?".AttackTenth":"")+(movementAdvancesEnemyPhase?".MovePhase":"")+(KiGuard?".KiGuard":"")+(RewardMergeValue==16?".Reward16":""):Legacy?version:version+"."+supply+"."+(spawnFromEight?8:4)+(ExhaustedMoveSupply?".ExhaustedMove":"")+(fiveKi?".Ki5":"")+(killKiOne?".KillKi1":"")+(bossLine?".BossLine":"")+(ActualMoveSupply?".ActualMove":"");
    // Explicit compatibility constructor for v0.8.1 replays and comparison tests.
    public YummnRulesSnapshot(YummnTileSupplyMode mode=YummnTileSupplyMode.KillOnly,bool fromEight=false,bool moveSupply=true,bool fiveKi=true,bool killKiOne=false,bool bossLine=true,bool actualMoveSupply=true)
    {version=YummnRulesProfile.Version;supply=mode;spawnFromEight=fromEight;exhaustedMoveSupply=moveSupply;this.fiveKi=fiveKi;this.killKiOne=killKiOne;this.bossLine=bossLine;this.actualMoveSupply=actualMoveSupply;}
    public static YummnRulesSnapshot OldV08()=>new YummnRulesSnapshot{version=YummnRulesProfile.LegacyVersion};
    public static YummnRulesSnapshot Current(int maxKi=6,YummnMovementCostMode movement=YummnMovementCostMode.PerCell,int killKi=3,bool attackPhase=false,bool fullRecovery=true,YummnTileSupplyMode supply=YummnTileSupplyMode.KillOnly,bool fromEight=false,bool attackTenth=false,bool movePhase=false,bool attackOne=false,bool kiGuard=false,int rewardValue=16)
    {
        var rules=new YummnRulesSnapshot{version=CurrentVersion,maxKi082=maxKi,killKi082=killKi,movementCostMode=movement,attackAdvancesEnemyPhase=attackPhase,exhaustionNeedsFullKi=fullRecovery,supply=supply,spawnFromEight=fromEight,attackCostsTenth=attackTenth,attackCostsOne=attackOne,movementAdvancesEnemyPhase=movePhase};
        rules.kiGuard=kiGuard;
        if(rewardValue!=16&&rewardValue!=32)throw new ArgumentOutOfRangeException(nameof(rewardValue));
        rules.rewardMergeValue=rewardValue;
        if(!rules.Valid)throw new ArgumentOutOfRangeException("Invalid v0.8.2 rules");return rules;
    }
}

public sealed partial class KaitRun
{
    public string ScoreRulesKey=>IsYummn?Yummn.rules.ScoreKey:RulesProfileId;
    private bool Yummn081=>IsYummn&&!Yummn.rules.Legacy;
    public bool YummnSupplyStarved
    {
        get
        {
            if(!Yummn081||Yummn.rules.ExhaustedMoveSupply||ended||Yummn.rules.Supply!=YummnTileSupplyMode.KillOnly||bossPending||enemies.Exists(e=>e.life!=KaitEnemyLife.Dead)||spawns.Count>0)return false;
            var values=new System.Collections.Generic.HashSet<int>();
            foreach(int v in threat)if(v>0&&!values.Add(v))return false;
            return true; // Diagnostic, never an additional loss condition.
        }
    }
    public bool IsYummnThreatLocked()
    {
        bool any=false;foreach(int v in threat)if(v>0){any=true;break;}
        if(!any)return false;
        foreach(KaitDirection d in Enum.GetValues(typeof(KaitDirection)))if(YummnThreatCanChange(d))return false;
        return true;
    }
    private void SupplyYummnTwos(KaitTurnResult r)
    {
        var a=r.yummnAction;
        a.stationaryPunch=a.isPunchAction&&a.preHitTravelCells==0;
        // Count actual player displacement, including post-hit follow-through
        // and teleports; enemy movement and threat-board-only inputs do not count.
        bool moved=r.yummnEvents.Exists(e=>e.kind==YummnEventKind.Move&&e.targetId<0&&e.from!=e.to&&
            (e.moveCause==YummnMoveCause.Player||e.moveCause==YummnMoveCause.Teleport));
        int requested=Yummn.rules.Supply==YummnTileSupplyMode.KillOnly?a.killIds.Count:
            Yummn.rules.Supply==YummnTileSupplyMode.EveryAction?1:
            Yummn.rules.ActualMoveSupply?(moved?1:0):a.stationaryPunch?0:1;
        // Follow-through after a stationary punch is not an exhausted walk.
        bool exhaustedMove=Yummn.rules.ExhaustedMoveSupply&&a.phaseAtStart==YummnPhase.Exhausted&&moved&&
            (Yummn.rules.ActualMoveSupply||!a.isPunchAction);
        if(exhaustedMove)requested=Yummn.rules.Supply==YummnTileSupplyMode.KillOnly?requested+1:1;
        a.suppressedTwo=!exhaustedMove&&Yummn.rules.Supply==YummnTileSupplyMode.SkipStationaryPunch&&katePos!=a.startCell&&!a.didAttack&&HasPassive(KaitPassive.PassWithoutTrace);
        if(a.suppressedTwo){requested=0;Yummn.metrics.suppressedTwos++;YummnTrigger("S04",r);}
        a.requestedTwos=requested;
        for(int i=0;i<requested;i++)
        {
            nextTwoPriority.RemoveAll(p=>p.x<0||p.y<0||p.x>=ThreatSize||p.y>=ThreatSize||threatPillars[p.x,p.y]||threat[p.x,p.y]!=0);
            var p=SpawnThreatTwoForTurn(r);
            if(p.x>=0){a.insertedTwos++;r.newThreatCells.Add(p);Yummn.metrics.naturalTwos++;}
            else a.droppedTwos++;
        }
        Yummn.metrics.requestedTwos+=a.requestedTwos;Yummn.metrics.insertedTwos+=a.insertedTwos;Yummn.metrics.droppedTwos+=a.droppedTwos;
    }
    private void ResolveYummn081Tail(KaitTurnResult r)
    {
        var a=r.yummnAction;
        if(a.didAttack||!IsYummnShadow(katePos))Yummn.cloak=false;
        else if(HasPassive(KaitPassive.ShadowCloak)){Yummn.cloak=true;YummnTrigger("S03",r);}
        Yummn.tranquility=a.phaseAtStart==YummnPhase.Exhausted&&!a.didAttack&&katePos!=a.startCell&&HasPassive(KaitPassive.Tranquility);
        if(a.killIds.Count>0)a.enemyPhaseReason|=YummnEnemyPhaseReason.Kill;
        if(a.phaseAtStart==YummnPhase.Exhausted)a.enemyPhaseReason|=YummnEnemyPhaseReason.Exhaustion;
        if(a.enemyPhaseReason!=YummnEnemyPhaseReason.None){a.enemyPhase=true;ResolveYummnEnemyPhase(r);}
        if(ended)return;
        SupplyYummnTwos(r);
        r.yummnThreatAfterSupply=CopyThreat();
        ResolveOldNewsArchive(r);
        if(a.enemyPhase){a.spawnChecked=true;ResolveYummnRifts(r);}
        if(bossPending){SpawnShieldKnight(r);if(r.bossSpawned)Yummn.metrics.bossCreatedAction=a.actionId;}
        if(a.phaseAtStart==YummnPhase.Exhausted){int gained=GainYummnKi(Yummn.profile.recoveryKi,r,"Recovery");Yummn.metrics.recoveryKi+=gained;}
        FinishYummnPhase(r);
        if(!ended&&IsYummnThreatLocked()){threatLocks++;Yummn.metrics.threatLocks++;End("ThreatBoardLocked",false);r.message="2048无可用移动，本局失败";}
    }
}
