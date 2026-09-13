using System;
using System.Collections.Generic;
using UnityEngine;

public enum YummnPhase { Burst, Exhausted }
public enum YummnMoveCause { Player, AI, Forced, Teleport, KillFollow, PushFollow }
public enum YummnDamageCause { Punch, WaterWhip, WinterBreath, FireSnake, Shatter, Quivering, Counter, Arrow, Melee, Area, EchoReflect, MergeMissile, MergeGlyph, Sonic, Collision, ShadowBlade, Kick }
public enum YummnAttackFamily { Martial, Kick, Spell, Environment }
public enum YummnAttackOrigin { Voluntary, Opportunist, Opportunity, Sweep, FlurryChild, Skill }
public enum YummnEventKind { Hit, Kill, Move, Status, Resource, Terrain, Aim, Spawn, EnemyAttack, AfterimageCreated, AfterimageHit, AfterimageCleared, AttackBegin }

[Serializable] public sealed class YummnAfterimageMarker
{
    public int id,sourceActionId;
    public int remainingPhases=1;
    public Vector2Int cell;
    public KaitDirection direction;
    public string source="Movement";
    public bool alive=true;
}

[Serializable] public sealed class YummnRulesProfile
{
    public const string Version="Yummn.ThreeTraditions.0.8.1";
    public const string LegacyVersion="Yummn.ThreeTraditions.0.8";
    public int maxKi=5, startKi=5, fistDamage=1, killKi=2, recoveryKi=1;
}
[Serializable] public sealed class YummnActionContext
{
    public int actionId, totalKiCost, kiBefore, kiAfter, mainEnemyId=-1;
    public YummnPhase phaseAtStart, phaseAtEnd;
    public KaitDirection direction, threatDirection;
    public Vector2Int startCell, finalCell, targetCell;
    public Vector2Int iceAtStart, darknessAtStart, decoyAtStart=YummnRun.NoCell;
    public bool didAttack, startedInShadow, enemyPhase, spawnChecked, suppressedTwo;
    public bool isPunchAction,stationaryPunch,isWait;
    public int preHitTravelCells,requestedTwos,insertedTwos,droppedTwos;
    public int voluntaryCells,movementKiCost,skillKiCost,rewardedKills,suppliedKills;
    public bool reachedZeroKi;
    public bool kiGuardTriggered;
    public int attackCostTenths;
    public bool playerMoved;
    public bool voluntaryMoved, voluntaryAttack, enemyPhaseExecuted, spawnWindowOpened;
    public List<string> equipmentSnapshot=new List<string>();
    public YummnEnemyPhaseReason enemyPhaseReason;
    public List<string> plannedSkills=new List<string>();
    public List<int> killIds=new List<int>();
    public string actionOverride;
}
[Serializable] public sealed class YummnCombatEvent
{
    public YummnEventKind kind;
    public int actionId, sourceId=-1, targetId=-1, amount, hpAfter;
    public Vector2Int from, to, direction;
    public YummnDamageCause damageCause;
    public YummnMoveCause moveCause;
    public string status, cardId, displayName;
    public bool blocked;
    public int attackEventId,markerId;
    public int eventId,parentEventId;
    public YummnAttackFamily attackFamily;
    public YummnAttackOrigin attackOrigin;
    public List<Vector2Int> affectedCells=new List<Vector2Int>();
}
[Serializable] public sealed class YummnMetrics
{
    public int actions, enemyPhases, spawnChecks, naturalTwos, suppressedTwos, kiSpent, killKi, recoveryKi, perfectKi;
    public int exhaustionCycles, heals, riftBlockedChecks, duplicatesPrevented, preparationCancels, threatLocks;
    public int requestedTwos,insertedTwos,droppedTwos;
    public int movementKiSpent,skillKiSpent,afterimageKi,overflowKi,afterimagesCreated,afterimagesHit,afterimagesCleared,enemyActors;
    public readonly Dictionary<string,int> afterimageHitsByEnemy=new Dictionary<string,int>();
    public int first32Action=-1,first128Action=-1;
    public int burstLength, longestBurst, exhaustedLength, longestExhausted, bossCreatedAction=-1, bossKilledAction=-1;
    public readonly Dictionary<string,int> cardTriggers=new Dictionary<string,int>(), cardSelections=new Dictionary<string,int>(), cardReplacements=new Dictionary<string,int>();
    public readonly Dictionary<int,int> enemyHits=new Dictionary<int,int>();
    public readonly Dictionary<string,int> damageSources=new Dictionary<string,int>();
}

// Dedicated state and rules profile; the compatibility host supplies the existing
// board, rewards and UI data. No Kait FinishTurn is called by this character.
public sealed class YummnRun
{
    public YummnRulesSnapshot rules {get;internal set;}=new YummnRulesSnapshot();
    public static readonly Vector2Int NoCell=new Vector2Int(-1,-1);
    public readonly YummnRulesProfile profile=new YummnRulesProfile();
    public YummnPhase phase=YummnPhase.Burst;
    public int ki=5, actionId, exhaustionCycleId;
    // Fractional Ki uses integer tenths so repeated 0.1 costs cannot drift.
    public int kiTenths;
    public int attackSpentTenths;
    public bool defense, tranquility, cloak, missileSpent;
    public Vector2Int icePillar=NoCell, darkness=NoCell;
    public int palmEnemyId=-1;
    public Vector2Int palmDirection;
    public readonly Dictionary<int,Vector2Int> palmMarks=new Dictionary<int,Vector2Int>();
    public bool TryPalm(int enemyId,out Vector2Int direction)
    {
        if(palmMarks.TryGetValue(enemyId,out direction))return true;
        direction=palmDirection;return palmEnemyId==enemyId;
    }
    public void MarkPalm(int enemyId,Vector2Int direction)
    {palmMarks[enemyId]=direction;palmEnemyId=enemyId;palmDirection=direction;}
    public void RemovePalm(int enemyId)
    {palmMarks.Remove(enemyId);if(palmEnemyId==enemyId)palmEnemyId=-1;}
    public readonly HashSet<string> prepared=new HashSet<string>();
    public readonly HashSet<int> rewardedDeaths=new HashSet<int>();
    public readonly List<YummnActionContext> history=new List<YummnActionContext>();
    public readonly List<YummnAfterimageMarker> afterimages=new List<YummnAfterimageMarker>();
    public int nextAfterimageId,nextAttackEventId;
    public int protectedEchoId=-1;
    public Vector2Int decoy=NoCell;
    public YummnMetrics metrics=new YummnMetrics();
    public void Reset()
    {
        profile.maxKi=profile.startKi=rules.MaxKi;profile.killKi=rules.KillKi;
        ki=profile.startKi;kiTenths=attackSpentTenths=0;phase=YummnPhase.Burst;actionId=exhaustionCycleId=0;
        defense=tranquility=cloak=missileSpent=false;icePillar=darkness=NoCell;
        palmEnemyId=-1;palmDirection=Vector2Int.zero;palmMarks.Clear();prepared.Clear();rewardedDeaths.Clear();history.Clear();metrics=new YummnMetrics();
        afterimages.Clear();nextAfterimageId=nextAttackEventId=0;protectedEchoId=-1;decoy=NoCell;
    }
}
