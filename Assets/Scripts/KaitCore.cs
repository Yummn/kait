using System;
using System.Collections.Generic;
using UnityEngine;

public enum KaitDirection { Up, Down, Left, Right }
public enum KaitEnemyType { Grunt = 1, Swordsman = 2, Archer = 3, Guard = 4, Elite = 5 }
public enum KaitEnemyLife { Preparing, Active, Dead }
public enum KaitArcherState { Ready, Aim }
public enum KaitIntentType { None, Move, Melee, LineShot }
public enum KaitSpawnState { Preview, Ready }

[Serializable] public sealed class KaitBalanceConfig
{
    public int threatSize = 5, initialThreatTiles = 3, newThreatTilesPerTurn = 1, winValue = 64;
    public int baseMomentum = 0, momentumPerEmptyCell = 1, momentumLossOnKill = 0;
    public int kateMaxHp = 3, wallCollisionDamage = 1, unitCollisionDamage = 1, riftBlockDamage = 1;
    public int archerRange = 3;
    public bool enablePush = true, enableFriendlyFire = true, enableInternalObstacle = true;
}

[Serializable] public sealed class KaitMergeEvent { public int resultValue; public Vector2Int threatCell; public bool spawnSuppressed; }
[Serializable] public sealed class KaitIntent
{
    public KaitIntentType type;
    public Vector2Int origin, target, direction;
    public int damage;
    public readonly List<Vector2Int> affectedCells = new List<Vector2Int>();
}
[Serializable] public sealed class KaitEnemy
{
    public int id, hp, maxHp;
    public KaitEnemyType type;
    public Vector2Int pos;
    public KaitEnemyLife life;
    public KaitArcherState archerState;
    public KaitIntent intent = new KaitIntent();
}
[Serializable] public sealed class KaitSpawnRequest
{
    public int tier, turnsUntilSpawn, createdTurn = -1;
    public Vector2Int sourceThreatCell, targetCell;
    public KaitSpawnState state;
}
[Serializable] public sealed class KaitEnemyAction
{
    public int enemyId, damage;
    public KaitIntentType type;
    public Vector2Int from, to;
    public readonly List<Vector2Int> affectedCells = new List<Vector2Int>();
    public readonly List<int> friendlyHitIds = new List<int>();
    public bool hitKate;
}
[Serializable] public sealed class KaitThreatMotion { public int value; public Vector2Int from, to; public bool merged; }

public sealed class KaitTurnResult
{
    public bool valid, turnComplete, awaitingTurnChoice;
    public readonly List<KaitDirection> availableDirections = new List<KaitDirection>();
    public readonly List<Vector2Int> katePath = new List<Vector2Int>();
    public readonly List<int> pathMomentum = new List<int>();
    public readonly List<int> killedEnemyIds = new List<int>();
    public readonly List<int> playerKilledEnemyIds = new List<int>();
    public readonly List<Vector2Int> killedEnemyCells = new List<Vector2Int>();
    public readonly List<KaitMergeEvent> merges = new List<KaitMergeEvent>();
    public readonly List<KaitEnemyAction> enemyActions = new List<KaitEnemyAction>();
    public readonly List<KaitThreatMotion> threatMotions = new List<KaitThreatMotion>();
    public readonly List<Vector2Int> spawnedEnemyCells = new List<Vector2Int>();
    public readonly List<Vector2Int> newThreatCells = new List<Vector2Int>();
    public int[,] threatBefore, threatAfter;
    public int slideDistance, damagedEnemyId = -1, damageDealt, enemyHpAfter = -1, momentumBefore, momentumAfter;
    public int collisionDamage, friendlyFireDamage, riftBlockDamage;
    public int spawnSuppressed;
    public bool pushed, pushBlockedByWall, pushBlockedByUnit, activeBrake;
    public bool threatChanged, kaitWaited;
    public KaitDirection globalDirection;
    public int chainStepCount, chainKillCount;
    public int chainPower, chainMoves;
    public bool powerLocked, chainEndedByStrongEnemy, chainEndedByWall;
    public Vector2Int pushFrom = new Vector2Int(-1, -1), pushTo = new Vector2Int(-1, -1);
    public Vector2Int blockedEnemyCell = new Vector2Int(-1, -1);
    public string message;
}

public sealed class KaitRun
{
    public const int BattleSize = 7, DefaultThreatSize = 5;
    public readonly KaitBalanceConfig config;
    public readonly int[,] threat;
    public readonly bool[,] threatPillars;
    public readonly int[,] mergeHeatmap;
    public readonly int[,] spawnHeatmap;
    public readonly bool[,] walls = new bool[BattleSize, BattleSize];
    public readonly List<KaitEnemy> enemies = new List<KaitEnemy>();
    public readonly List<KaitSpawnRequest> spawns = new List<KaitSpawnRequest>();

    public int ThreatSize => config.threatSize;
    public Vector2Int katePos { get; private set; }
    public int kateHp { get; private set; }
    public int turn { get; private set; }
    public int kills { get; private set; }
    public int highestThreat { get; private set; }
    public int momentum { get; private set; }
    public int highestMomentum { get; private set; }
    public int currentChainKills { get; private set; }
    public int longestChainKills { get; private set; }
    public int chainPower { get; private set; }
    public int currentChainMoves { get; private set; }
    public bool powerLocked { get; private set; }
    public int pushCount { get; private set; }
    public int friendlyFireDamage { get; private set; }
    public int riftBlocks { get; private set; }
    public int directKills { get; private set; }
    public int nonLethalHits { get; private set; }
    public int activeWallStops { get; private set; }
    public int wallSuppressedSpawns { get; private set; }
    public int spawnSuppressedCount { get; private set; }
    public int chainEndByStrongEnemy { get; private set; }
    public int chainEndByWall { get; private set; }
    public int clusterClearCount { get; private set; }
    public int threatOrientedWaitCount { get; private set; }
    public bool emptyMapReachable { get; private set; }
    public int emptyMapMaxInputs { get; private set; }
    public int internalMergeCount { get; private set; }
    public int internalSpawnCount { get; private set; }
    public readonly int[] lockedPowerCounts = new int[BattleSize];
    public KaitDirection currentGlobalDirection { get; private set; }
    public bool threatChangedThisTurn { get; private set; }
    public bool kaitWaitedThisTurn { get; private set; }
    public int chainStepCount { get; private set; }
    public bool chainActive { get; private set; }
    public KaitDirection currentDirection { get; private set; }
    public int threatLocks { get; private set; }
    public bool ended { get; private set; }
    public bool won { get; private set; }
    public string endReason { get; private set; }
    public int mapIndex { get; private set; }

    private System.Random random;
    private int nextEnemyId;

    public KaitRun(KaitBalanceConfig balance = null)
    {
        config = balance ?? new KaitBalanceConfig();
        config.threatSize = Mathf.Max(2, config.threatSize);
        threat = new int[config.threatSize, config.threatSize];
        threatPillars = new bool[config.threatSize, config.threatSize];
        mergeHeatmap = new int[config.threatSize, config.threatSize];
        spawnHeatmap = new int[config.threatSize, config.threatSize];
    }

    public void Reset(int seed)
    {
        random = new System.Random(seed); Array.Clear(threat, 0, threat.Length); Array.Clear(walls, 0, walls.Length);
        Array.Clear(threatPillars, 0, threatPillars.Length); Array.Clear(mergeHeatmap, 0, mergeHeatmap.Length); Array.Clear(spawnHeatmap, 0, spawnHeatmap.Length);
        enemies.Clear(); spawns.Clear(); nextEnemyId = 1; turn = kills = threatLocks = pushCount = friendlyFireDamage = riftBlocks = 0;
        directKills = nonLethalHits = activeWallStops = wallSuppressedSpawns = spawnSuppressedCount = 0;
        chainEndByStrongEnemy = chainEndByWall = clusterClearCount = threatOrientedWaitCount = internalMergeCount = internalSpawnCount = 0;
        threatChangedThisTurn = kaitWaitedThisTurn = false; chainStepCount = currentChainMoves = 0;
        highestThreat = 2; momentum = highestMomentum = currentChainKills = longestChainKills = chainPower = 0;
        powerLocked = chainActive = ended = won = false; endReason = string.Empty; Array.Clear(lockedPowerCounts, 0, lockedPowerCounts.Length);
        for (int y = 0; y < BattleSize; y++) for (int x = 0; x < BattleSize; x++) walls[x, y] = x == 0 || y == 0 || x == BattleSize - 1 || y == BattleSize - 1;
        mapIndex = 1;
        walls[2, 1] = true;
        walls[5, 4] = true;
        AddSharedThreatPillar(new Vector2Int(2, 1));
        AddSharedThreatPillar(new Vector2Int(5, 4));
        EvaluateEmptyMapReachability();
        katePos = FindOpenNearCenter(); kateHp = config.kateMaxHp;
        for (int i = 0; i < config.initialThreatTiles; i++) SpawnThreatTwo();
        LockEnemyIntents();
    }

    public KaitTurnResult TryTurn(KaitDirection direction) => TryGlobalInput(direction);

    public KaitTurnResult TryGlobalInput(KaitDirection direction)
    {
        var result = new KaitTurnResult();
        if (ended) { result.message = "本局已结束"; return result; }
        if (chainActive) { result.message = "请选择击杀后的转向"; return result; }
        bool kaitCanRespond = CanEnterFrom(katePos, direction);
        int[,] before = CopyThreat(); result.merges.AddRange(MoveThreat(direction, result.threatMotions)); int[,] after = CopyThreat();
        bool threatChanged = !ThreatEquals(before, after);
        if (!kaitCanRespond && !threatChanged)
        {
            result.merges.Clear(); result.threatMotions.Clear(); result.message = "两盘均无法响应，未消耗回合"; return result;
        }

        result.valid = true; result.globalDirection = direction; result.threatChanged = threatChanged; result.kaitWaited = !kaitCanRespond;
        result.threatBefore = before; result.threatAfter = after;
        if (!threatChanged) result.threatMotions.Clear();
        currentGlobalDirection = direction; threatChangedThisTurn = threatChanged; kaitWaitedThisTurn = !kaitCanRespond;
        currentDirection = direction; momentum = 0; chainPower = 0; powerLocked = false;
        currentChainKills = 0; currentChainMoves = 0; chainStepCount = 0; chainActive = kaitCanRespond;
        if (!kaitCanRespond && threatChanged) threatOrientedWaitCount++;
        foreach (KaitMergeEvent merge in result.merges) if (merge.resultValue < config.winValue) QueueSpawn(merge);
        result.spawnSuppressed += result.merges.FindAll(m => m.spawnSuppressed).Count;
        if (!kaitCanRespond)
        {
            result.message = "凯特原地等待：威胁盘已整理，时间正常推进"; FinishTurn(result); ApplyTurnContext(result); return result;
        }
        ResolveKateSegment(result); result.slideDistance = result.katePath.Count; ApplyTurnContext(result); return result;
    }

    public KaitTurnResult ContinueChain(KaitDirection direction)
    {
        var result = new KaitTurnResult();
        if (!chainActive) { result.message = "当前没有可继续的连斩"; return result; }
        result.valid = true; currentDirection = direction; chainStepCount++;
        if (IsHardBlocked(katePos + Delta(direction)))
        {
            result.activeBrake = true; result.chainEndedByWall = true; activeWallStops++; chainEndByWall++;
            result.message = "主动撞墙刹车：原地结束连锁"; FinishTurn(result); ApplyTurnContext(result); return result;
        }
        ResolveKateSegment(result); result.slideDistance = result.katePath.Count; ApplyTurnContext(result); return result;
    }

    public List<KaitDirection> AllowedTurnDirections()
    {
        if (!chainActive) return new List<KaitDirection>();
        return new List<KaitDirection> { KaitDirection.Up, KaitDirection.Down, KaitDirection.Left, KaitDirection.Right };
    }

    private void ResolveKateSegment(KaitTurnResult result)
    {
        result.momentumBefore = momentum; Vector2Int delta = Delta(currentDirection);
        for (int guard = 0; guard < 64; guard++)
        {
            Vector2Int next = katePos + delta;
            if (IsHardBlocked(next))
            {
                if (powerLocked) { result.chainEndedByWall = true; chainEndByWall++; }
                FinishTurn(result); return;
            }
            KaitEnemy enemy = EnemyAt(next);
            if (enemy == null)
            {
                katePos = next;
                if (!powerLocked)
                {
                    momentum += config.momentumPerEmptyCell;
                    highestMomentum = Mathf.Max(highestMomentum, momentum);
                }
                else currentChainMoves++;
                result.katePath.Add(katePos); result.pathMomentum.Add(momentum); continue;
            }

            if (!powerLocked)
            {
                chainPower = momentum; powerLocked = true;
                lockedPowerCounts[Mathf.Clamp(chainPower, 0, lockedPowerCounts.Length - 1)]++;
            }
            int damage = chainPower; DamageEnemy(enemy, damage, true, result);
            result.damagedEnemyId = enemy.id; result.damageDealt = damage; result.enemyHpAfter = enemy.hp; result.blockedEnemyCell = enemy.pos;
            if (enemy.life == KaitEnemyLife.Dead)
            {
                directKills++; ContinueAfterPrimaryKill(next, result); return;
            }

            ResolvePush(enemy, delta, result);
            if (enemy.life == KaitEnemyLife.Dead)
            {
                ContinueAfterPrimaryKill(enemy.pos, result); return;
            }
            nonLethalHits++; result.chainEndedByStrongEnemy = true; chainEndByStrongEnemy++;
            FinishTurn(result); return;
        }
        FinishTurn(result);
    }

    private void ContinueAfterPrimaryKill(Vector2Int enemyCell, KaitTurnResult result)
    {
        katePos = enemyCell;
        if (result.katePath.Count == 0 || result.katePath[result.katePath.Count - 1] != katePos)
        {
            result.katePath.Add(katePos); result.pathMomentum.Add(momentum);
        }
        result.blockedEnemyCell = new Vector2Int(-1, -1);
        currentChainKills++; longestChainKills = Mathf.Max(longestChainKills, currentChainKills);
        List<KaitDirection> choices = AllowedTurnDirections();
        if (choices.Count == 0) { FinishTurn(result); return; }
        result.awaitingTurnChoice = true; result.availableDirections.AddRange(choices); result.momentumAfter = momentum;
        result.message = result.collisionDamage > 0
            ? "碰撞处决成功：上下左右自由转向，撞墙可刹车"
            : "击杀成功：上下左右自由转向，撞墙可刹车";
    }

    private void ResolvePush(KaitEnemy enemy, Vector2Int delta, KaitTurnResult result)
    {
        Vector2Int origin = enemy.pos, target = origin + delta, kateBeforeImpact = katePos; result.pushFrom = origin; result.pushTo = target;
        if (!config.enablePush) return;
        pushCount++;
        KaitEnemy blocker = EnemyAt(target);
        if (IsHardBlocked(target))
        {
            result.pushBlockedByWall = true; result.collisionDamage += config.wallCollisionDamage;
            DamageEnemy(enemy, config.wallCollisionDamage, true, result);
            if (enemy.life == KaitEnemyLife.Dead && !result.playerKilledEnemyIds.Contains(enemy.id)) result.playerKilledEnemyIds.Add(enemy.id);
            if (enemy.life == KaitEnemyLife.Dead) katePos = origin;
        }
        else if (blocker != null)
        {
            result.pushBlockedByUnit = true; result.collisionDamage += config.unitCollisionDamage * 2;
            DamageEnemy(enemy, config.unitCollisionDamage, true, result); DamageEnemy(blocker, config.unitCollisionDamage, false, result);
            if (enemy.life == KaitEnemyLife.Dead && !result.playerKilledEnemyIds.Contains(enemy.id)) result.playerKilledEnemyIds.Add(enemy.id);
            if (enemy.life == KaitEnemyLife.Dead) katePos = origin;
        }
        else
        {
            enemy.pos = target; result.pushed = true; katePos = origin;
        }
        if (katePos != kateBeforeImpact) { result.katePath.Add(katePos); result.pathMomentum.Add(momentum); }
        result.enemyHpAfter = enemy.hp;
    }

    private void FinishTurn(KaitTurnResult result)
    {
        ApplyTurnContext(result);
        chainActive = false; result.turnComplete = true; result.momentumAfter = momentum;
        if (currentChainKills >= 3) clusterClearCount++;
        ResolveEnemyIntents(result);
        if (!ended)
        {
            AgePreparingEnemies(); ResolveSpawnRequests(result);
            for (int i = 0; i < config.newThreatTilesPerTurn; i++) { Vector2Int p = SpawnThreatTwo(); if (p.x >= 0) result.newThreatCells.Add(p); }
            if (ThreatLocked()) ResetLockedThreat();
            if (highestThreat >= config.winValue) End("Victory 64", true);
            else if (kateHp <= 0) End("Kate Defeated", false);
        }
        turn++; momentum = 0; chainPower = 0; powerLocked = false; currentChainMoves = 0; LockEnemyIntents();
        ApplyTurnContext(result);
        if (string.IsNullOrEmpty(result.message))
        {
            if (result.pushed) result.message = "未击杀：推动敌人 1 格，连锁结束";
            else if (result.activeBrake) result.message = "主动撞墙刹车：原地结束连锁";
            else if (result.pushBlockedByWall) result.message = "撞墙：敌人额外受到 1 点伤害";
            else if (result.pushBlockedByUnit) result.message = "撞敌：双方受到 1 点碰撞伤害";
            else result.message = "回合完成";
        }
    }

    public KaitEnemy EnemyAt(Vector2Int p) => enemies.Find(e => e.life != KaitEnemyLife.Dead && e.pos == p);
    public KaitSpawnRequest SpawnAt(Vector2Int p) => spawns.Find(s => s.targetCell == p);
    public Vector2Int MapThreatToBattle(Vector2Int p) => p + Vector2Int.one;
    public bool IsThreatPillar(Vector2Int p) => p.x >= 0 && p.x < ThreatSize && p.y >= 0 && p.y < ThreatSize && threatPillars[p.x, p.y];

    private void ResolveEnemyIntents(KaitTurnResult result)
    {
        enemies.Sort((a, b) => a.id.CompareTo(b.id));
        var readyArchers = new List<KaitEnemy>(enemies.FindAll(e => e.life == KaitEnemyLife.Active && e.type == KaitEnemyType.Archer && e.archerState == KaitArcherState.Ready));
        var committed = new List<KaitEnemy>(enemies.FindAll(e => e.life == KaitEnemyLife.Active && e.intent.type != KaitIntentType.None));
        foreach (KaitEnemy attacker in committed)
        {
            if (attacker.life == KaitEnemyLife.Dead) continue;
            KaitIntent intent = attacker.type == KaitEnemyType.Archer ? BuildArcherFireIntent(attacker) : attacker.intent;
            Vector2Int actionOrigin = attacker.type == KaitEnemyType.Archer ? attacker.pos : intent.origin;
            var action = new KaitEnemyAction { enemyId = attacker.id, type = intent.type, from = actionOrigin, to = intent.target, damage = intent.damage };
            action.affectedCells.AddRange(intent.affectedCells);
            foreach (Vector2Int cell in intent.affectedCells)
            {
                bool hitUnit = false;
                if (katePos == cell)
                {
                    kateHp = Mathf.Max(0, kateHp - intent.damage); action.hitKate = true; hitUnit = true;
                }
                if (config.enableFriendlyFire)
                {
                    KaitEnemy victim = enemies.Find(e => e.life != KaitEnemyLife.Dead && e.id != attacker.id && e.pos == cell);
                    if (victim != null)
                    {
                        int before = victim.hp; DamageEnemy(victim, intent.damage, false, result); int dealt = before - victim.hp;
                        friendlyFireDamage += dealt; result.friendlyFireDamage += dealt; action.friendlyHitIds.Add(victim.id); hitUnit = true;
                    }
                }
                if (attacker.type == KaitEnemyType.Archer && hitUnit) break;
            }
            result.enemyActions.Add(action);
            if (attacker.type == KaitEnemyType.Archer)
            {
                attacker.archerState = KaitArcherState.Ready;
                attacker.intent = new KaitIntent { origin = attacker.pos };
            }
        }
        foreach (KaitEnemy archer in readyArchers) if (archer.life == KaitEnemyLife.Active) BeginArcherAim(archer);
        if (kateHp <= 0) End("Kate Defeated", false);
    }

    private void LockEnemyIntents()
    {
        foreach (KaitEnemy enemy in enemies)
        {
            enemy.intent = new KaitIntent { origin = enemy.pos };
            if (enemy.life != KaitEnemyLife.Active) continue;
            Vector2Int diff = katePos - enemy.pos;
            if (enemy.type == KaitEnemyType.Archer) continue;
            if (enemy.type == KaitEnemyType.Elite)
            {
                Vector2Int direction = Mathf.Abs(diff.x) >= Mathf.Abs(diff.y) ? new Vector2Int(Math.Sign(diff.x), 0) : new Vector2Int(0, Math.Sign(diff.y));
                if (direction == Vector2Int.zero) direction = Vector2Int.up;
                enemy.intent.type = KaitIntentType.LineShot; enemy.intent.direction = direction; enemy.intent.damage = 1;
                int range = enemy.type == KaitEnemyType.Elite ? config.archerRange + 1 : config.archerRange;
                for (int i = 1; i <= range; i++) { Vector2Int p = enemy.pos + direction * i; if (!Inside(p) || walls[p.x, p.y]) break; enemy.intent.affectedCells.Add(p); }
                if (enemy.intent.affectedCells.Count > 0) enemy.intent.target = enemy.intent.affectedCells[enemy.intent.affectedCells.Count - 1];
            }
            else if (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) == 1)
            {
                enemy.intent.type = KaitIntentType.Melee; enemy.intent.target = katePos; enemy.intent.damage = enemy.type == KaitEnemyType.Guard ? 2 : 1; enemy.intent.affectedCells.Add(katePos);
            }
        }
    }

    private void BeginArcherAim(KaitEnemy archer)
    {
        Vector2Int diff = katePos - archer.pos;
        Vector2Int direction = Mathf.Abs(diff.x) >= Mathf.Abs(diff.y) ? new Vector2Int(Math.Sign(diff.x), 0) : new Vector2Int(0, Math.Sign(diff.y));
        if (direction == Vector2Int.zero) direction = Vector2Int.up;
        archer.archerState = KaitArcherState.Aim;
        archer.intent = BuildLineIntent(archer.pos, direction, config.archerRange, false);
    }

    private KaitIntent BuildArcherFireIntent(KaitEnemy archer)
    {
        return BuildLineIntent(archer.pos, archer.intent.direction, config.archerRange, true);
    }

    private KaitIntent BuildLineIntent(Vector2Int origin, Vector2Int direction, int range, bool stopAtFirstUnit)
    {
        var intent = new KaitIntent { type = KaitIntentType.LineShot, origin = origin, direction = direction, damage = 1 };
        for (int i = 1; i <= range; i++)
        {
            Vector2Int p = origin + direction * i;
            if (!Inside(p) || walls[p.x, p.y]) break;
            intent.affectedCells.Add(p); intent.target = p;
            if (stopAtFirstUnit && (katePos == p || EnemyAt(p) != null)) break;
        }
        return intent;
    }

    private void AgePreparingEnemies() { foreach (KaitEnemy e in enemies) if (e.life == KaitEnemyLife.Preparing) e.life = KaitEnemyLife.Active; }
    private void DamageEnemy(KaitEnemy enemy, int amount, bool creditKate, KaitTurnResult result)
    {
        if (enemy == null || enemy.life == KaitEnemyLife.Dead || amount <= 0) return;
        enemy.hp = Mathf.Max(0, enemy.hp - amount);
        if (enemy.hp > 0) return;
        enemy.life = KaitEnemyLife.Dead;
        enemy.intent = new KaitIntent { origin = enemy.pos };
        if (creditKate) { kills++; if (!result.playerKilledEnemyIds.Contains(enemy.id)) result.playerKilledEnemyIds.Add(enemy.id); }
        if (!result.killedEnemyIds.Contains(enemy.id)) { result.killedEnemyIds.Add(enemy.id); result.killedEnemyCells.Add(enemy.pos); }
    }

    private sealed class ThreatToken { public int value; public readonly List<Vector2Int> sources = new List<Vector2Int>(); public bool merged; }
    private List<KaitMergeEvent> MoveThreat(KaitDirection direction, List<KaitThreatMotion> motions)
    {
        var merges = new List<KaitMergeEvent>(); bool horizontal = direction == KaitDirection.Left || direction == KaitDirection.Right;
        bool reverse = direction == KaitDirection.Right || direction == KaitDirection.Up;
        for (int line = 0; line < ThreatSize; line++)
        {
            var segment = new List<Vector2Int>();
            for (int i = 0; i < ThreatSize; i++)
            {
                int index = reverse ? ThreatSize - 1 - i : i, x = horizontal ? index : line, y = horizontal ? line : index;
                Vector2Int cell = new Vector2Int(x, y);
                if (IsThreatPillar(cell))
                {
                    ProcessThreatSegment(segment, motions, merges); segment.Clear();
                }
                else segment.Add(cell);
            }
            ProcessThreatSegment(segment, motions, merges);
        }
        return merges;
    }

    private void ProcessThreatSegment(List<Vector2Int> segment, List<KaitThreatMotion> motions, List<KaitMergeEvent> merges)
    {
        if (segment.Count == 0) return;
        var values = new List<ThreatToken>();
        foreach (Vector2Int cell in segment)
        {
            if (threat[cell.x, cell.y] == 0) continue;
            var token = new ThreatToken { value = threat[cell.x, cell.y] }; token.sources.Add(cell); values.Add(token);
        }
        var packed = new List<ThreatToken>();
        for (int i = 0; i < values.Count; i++)
        {
            if (i + 1 < values.Count && values[i].value == values[i + 1].value)
            {
                var merged = new ThreatToken { value = values[i].value * 2, merged = true };
                merged.sources.AddRange(values[i].sources); merged.sources.AddRange(values[++i].sources); packed.Add(merged);
            }
            else packed.Add(values[i]);
        }
        foreach (Vector2Int cell in segment) threat[cell.x, cell.y] = 0;
        for (int i = 0; i < packed.Count; i++)
        {
            ThreatToken token = packed[i]; Vector2Int destination = segment[i]; threat[destination.x, destination.y] = token.value;
            foreach (Vector2Int source in token.sources) motions.Add(new KaitThreatMotion { value = token.merged ? token.value / 2 : token.value, from = source, to = destination, merged = token.merged });
            if (!token.merged) continue;
            merges.Add(new KaitMergeEvent { resultValue = token.value, threatCell = destination });
            mergeHeatmap[destination.x, destination.y]++; if (IsInternalThreatCell(destination)) internalMergeCount++;
            highestThreat = Mathf.Max(highestThreat, token.value);
        }
    }

    private Vector2Int SpawnThreatTwo()
    {
        var empty = new List<Vector2Int>();
        for (int y = 0; y < ThreatSize; y++) for (int x = 0; x < ThreatSize; x++) if (!threatPillars[x, y] && threat[x, y] == 0) empty.Add(new Vector2Int(x, y));
        if (empty.Count == 0) return new Vector2Int(-1, -1); Vector2Int p = empty[random.Next(empty.Count)]; threat[p.x, p.y] = 2; return p;
    }
    private void QueueSpawn(KaitMergeEvent merge)
    {
        Vector2Int target = MapThreatToBattle(merge.threatCell);
        if (walls[target.x, target.y]) { merge.spawnSuppressed = true; wallSuppressedSpawns++; spawnSuppressedCount++; return; }
        int tier = Mathf.Clamp((int)Mathf.Log(merge.resultValue, 2f) - 1, 1, 4);
        spawns.Add(new KaitSpawnRequest { tier = tier, sourceThreatCell = merge.threatCell, targetCell = target, turnsUntilSpawn = 1, createdTurn = turn, state = KaitSpawnState.Preview });
    }
    private void ResolveSpawnRequests(KaitTurnResult result)
    {
        for (int i = 0; i < spawns.Count;)
        {
            KaitSpawnRequest request = spawns[i];
            if (request.createdTurn == turn) { i++; continue; }
            request.turnsUntilSpawn = Mathf.Max(0, request.turnsUntilSpawn - 1); request.state = request.turnsUntilSpawn > 0 ? KaitSpawnState.Preview : KaitSpawnState.Ready;
            if (request.turnsUntilSpawn > 0) { i++; continue; }
            KaitEnemy occupant = EnemyAt(request.targetCell);
            if (katePos == request.targetCell)
            {
                kateHp = Mathf.Max(0, kateHp - config.riftBlockDamage); result.riftBlockDamage += config.riftBlockDamage; result.spawnSuppressed++; riftBlocks++; spawnSuppressedCount++; spawns.RemoveAt(i); continue;
            }
            if (occupant != null)
            {
                DamageEnemy(occupant, config.riftBlockDamage, false, result); result.riftBlockDamage += config.riftBlockDamage; result.spawnSuppressed++; riftBlocks++; spawnSuppressedCount++; spawns.RemoveAt(i); continue;
            }
            KaitEnemyType type = EnemyTypeForSpawn(request);
            int hp = MaxHpFor(type);
            enemies.Add(new KaitEnemy { id = nextEnemyId++, type = type, pos = request.targetCell, hp = hp, maxHp = hp, life = KaitEnemyLife.Preparing });
            spawnHeatmap[request.sourceThreatCell.x, request.sourceThreatCell.y]++; if (IsInternalThreatCell(request.sourceThreatCell)) internalSpawnCount++;
            result.spawnedEnemyCells.Add(request.targetCell); spawns.RemoveAt(i);
        }
    }

    private void ResetLockedThreat() { threatLocks++; Array.Clear(threat, 0, threat.Length); for (int i = 0; i < config.initialThreatTiles; i++) SpawnThreatTwo(); }
    private bool ThreatLocked()
    {
        for (int y = 0; y < ThreatSize; y++) for (int x = 0; x < ThreatSize; x++)
        {
            if (threatPillars[x, y]) continue;
            if (threat[x, y] == 0) return false;
            if (x + 1 < ThreatSize && !threatPillars[x + 1, y] && threat[x, y] == threat[x + 1, y]) return false;
            if (y + 1 < ThreatSize && !threatPillars[x, y + 1] && threat[x, y] == threat[x, y + 1]) return false;
        }
        return true;
    }

    private void AddSharedThreatPillar(Vector2Int battleCell)
    {
        Vector2Int threatCell = battleCell - Vector2Int.one;
        if (threatCell.x >= 0 && threatCell.x < ThreatSize && threatCell.y >= 0 && threatCell.y < ThreatSize) threatPillars[threatCell.x, threatCell.y] = true;
    }

    private bool IsInternalThreatCell(Vector2Int p) => p.x > 0 && p.y > 0 && p.x < ThreatSize - 1 && p.y < ThreatSize - 1;
    private void ApplyTurnContext(KaitTurnResult result)
    {
        result.globalDirection = currentGlobalDirection;
        result.threatChanged = threatChangedThisTurn;
        result.kaitWaited = kaitWaitedThisTurn;
        result.chainStepCount = chainStepCount;
        result.chainKillCount = currentChainKills;
        if (!result.turnComplete)
        {
            result.chainPower = chainPower;
            result.powerLocked = powerLocked;
            result.chainMoves = currentChainMoves;
        }
    }

    private void EvaluateEmptyMapReachability()
    {
        int center = BattleSize / 2, far = BattleSize - 2;
        int[] checks =
        {
            InputsToReachRegion(new Vector2Int(1, center), p => p.x >= far - 1, 3),
            InputsToReachRegion(new Vector2Int(far, center), p => p.x <= 2, 3),
            InputsToReachRegion(new Vector2Int(center, 1), p => p.y >= far - 1, 3),
            InputsToReachRegion(new Vector2Int(center, far), p => p.y <= 2, 3)
        };
        emptyMapReachable = true; emptyMapMaxInputs = 0;
        foreach (int inputs in checks)
        {
            if (inputs < 0) { emptyMapReachable = false; continue; }
            emptyMapMaxInputs = Mathf.Max(emptyMapMaxInputs, inputs);
        }
    }

    private int InputsToReachRegion(Vector2Int start, Func<Vector2Int, bool> target, int maxInputs)
    {
        var distances = new Dictionary<Vector2Int, int> { [start] = 0 };
        var queue = new Queue<Vector2Int>(); queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector2Int origin = queue.Dequeue(); int nextDepth = distances[origin] + 1;
            if (nextDepth > maxInputs) continue;
            foreach (KaitDirection direction in (KaitDirection[])Enum.GetValues(typeof(KaitDirection)))
            {
                Vector2Int delta = Delta(direction), current = origin;
                while (!IsHardBlocked(current + delta))
                {
                    current += delta;
                    if (target(current)) return nextDepth;
                }
                if (current != origin && !distances.ContainsKey(current)) { distances[current] = nextDepth; queue.Enqueue(current); }
            }
        }
        return -1;
    }
    private bool ThreatEquals(int[,] a, int[,] b)
    {
        for (int y = 0; y < ThreatSize; y++) for (int x = 0; x < ThreatSize; x++) if (a[x, y] != b[x, y]) return false;
        return true;
    }
    private static KaitEnemyType EnemyTypeForSpawn(KaitSpawnRequest request)
    {
        if (request.tier <= 1) return KaitEnemyType.Grunt;
        if (request.tier == 2) return (request.targetCell.x + request.targetCell.y) % 2 == 0 ? KaitEnemyType.Swordsman : KaitEnemyType.Archer;
        if (request.tier == 3) return KaitEnemyType.Guard;
        return KaitEnemyType.Elite;
    }
    public static int MaxHpFor(KaitEnemyType type)
    {
        switch (type)
        {
            case KaitEnemyType.Grunt: return 2;
            case KaitEnemyType.Swordsman: return 4;
            case KaitEnemyType.Archer: return 2;
            case KaitEnemyType.Guard: return 6;
            default: return 8;
        }
    }
    private Vector2Int FindOpenNearCenter()
    { Vector2Int center = new Vector2Int(BattleSize / 2, BattleSize / 2); if (!walls[center.x, center.y]) return center; return center + Vector2Int.left; }
    private bool CanEnterFrom(Vector2Int from, KaitDirection d) { Vector2Int p = from + Delta(d); return !IsHardBlocked(p) || EnemyAt(p) != null; }
    private bool IsHardBlocked(Vector2Int p) => !Inside(p) || walls[p.x, p.y];
    private static bool Inside(Vector2Int p) => p.x >= 0 && p.x < BattleSize && p.y >= 0 && p.y < BattleSize;
    private int[,] CopyThreat() { var copy = new int[ThreatSize, ThreatSize]; Array.Copy(threat, copy, threat.Length); return copy; }
    private void End(string reason, bool victory) { ended = true; won = victory; endReason = reason; chainActive = false; }

    public static Vector2Int Delta(KaitDirection d)
    { switch (d) { case KaitDirection.Up: return Vector2Int.up; case KaitDirection.Down: return Vector2Int.down; case KaitDirection.Left: return Vector2Int.left; default: return Vector2Int.right; } }
    public static KaitDirection TurnLeft(KaitDirection d)
    { switch (d) { case KaitDirection.Up: return KaitDirection.Left; case KaitDirection.Left: return KaitDirection.Down; case KaitDirection.Down: return KaitDirection.Right; default: return KaitDirection.Up; } }
    public static KaitDirection TurnRight(KaitDirection d) => TurnLeft(TurnLeft(TurnLeft(d)));
    public static KaitDirection Opposite(KaitDirection d) => TurnLeft(TurnLeft(d));
}
