using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    [Serializable] private sealed class MetricCount { public string key;public int count; }
    [Serializable] private sealed class YummnActionLog
    {
        public string rules,character,endReason;public int seed,hp,threatOccupied,rifts;
        public YummnActionContext action;public List<YummnCombatEvent> events;
        public YummnMetrics totals;
        public List<MetricCount> triggers,selections,replacements,enemyHits,damageSources,afterimageHits;
        public List<string> active,passive;
        public YummnRulesSnapshot snapshot;
        public string scoreGroup,diagnostic;
        public int threatSum,highestTile,spawnedHp,pendingHp,enemyMoves,enemyAims,enemyAttacks;
        public float elapsedSeconds;
        public List<KaitMergeEvent> merges;
    }
    private static List<MetricCount> MetricList<T>(Dictionary<T,int> values)
    {var result=new List<MetricCount>();foreach(var p in values)result.Add(new MetricCount{key=p.Key.ToString(),count=p.Value});result.Sort((a,b)=>string.CompareOrdinal(a.key,b.key));return result;}
    public string YummnActionJson(KaitTurnResult result,float elapsedSeconds=-1)
    {
        if(!IsYummn||result?.yummnAction==null)return null;
        int occupied=0,sum=0,highest=0,pendingHp=0,spawnedHp=0;foreach(int v in threat)if(v>0){occupied++;sum+=v;highest=Mathf.Max(highest,v);}
        foreach(var s in spawns)pendingHp+=MaxHpFor(EnemyTypeForSpawn(s));
        foreach(var p in result.spawnedEnemyCells){var e=EnemyAt(p);if(e!=null)spawnedHp+=e.maxHp;}
        int moves=0,aims=0,attacks=0;foreach(var e in result.yummnEvents){if(e.kind==YummnEventKind.Move&&e.moveCause==YummnMoveCause.AI)moves++;if(e.kind==YummnEventKind.Aim)aims++;if(e.kind==YummnEventKind.EnemyAttack)attacks++;}
        return JsonUtility.ToJson(new YummnActionLog
        {
            rules=RulesProfileId,character="Yummn",seed=replaySeed,hp=kateHp,endReason=endReason,
            snapshot=Yummn.rules,scoreGroup=ScoreRulesKey,diagnostic=YummnSupplyStarved?"SupplyStarvation":"",threatSum=sum,highestTile=highest,spawnedHp=spawnedHp,pendingHp=pendingHp,enemyMoves=moves,enemyAims=aims,enemyAttacks=attacks,merges=result.merges,elapsedSeconds=elapsedSeconds,
            threatOccupied=occupied,rifts=spawns.Count,action=result.yummnAction,events=result.yummnEvents,totals=Yummn.metrics,
            triggers=MetricList(Yummn.metrics.cardTriggers),selections=MetricList(Yummn.metrics.cardSelections),replacements=MetricList(Yummn.metrics.cardReplacements),
            enemyHits=MetricList(Yummn.metrics.enemyHits),damageSources=MetricList(Yummn.metrics.damageSources),afterimageHits=MetricList(Yummn.metrics.afterimageHitsByEnemy),
            active=skills.ConvertAll(s=>SkillAbilityId(s)),passive=passives.ConvertAll(p=>PassiveAbilityId(p))
        });
    }
}
