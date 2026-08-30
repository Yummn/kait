using System;
using System.Collections.Generic;
using UnityEngine;

public enum KaitDirection { Up, Down, Left, Right }
public enum KaitEnemyType { Grunt = 1, Guard = 2, Archer = 3, Elite = 4 }
public enum KaitEnemyLife { Preparing, Active, Dead }
public enum KaitIntentType { None, Move, Melee, LineShot }
public enum KaitSpawnState { Preview, Ready }

[Serializable] public sealed class KaitBalanceConfig
{
    public int threatSize = 7, initialThreatTiles = 4, newThreatTilesPerTurn = 2;
    public int baseMomentum = 1, momentumPerEmptyCell = 1, momentumLossOnKill = 1;
    public int maxActiveEnemies = 8, maxMaterializePerTurn = 2, maxHardBlockers = 2;
}

[Serializable] public sealed class KaitMergeEvent { public int resultValue; public Vector2Int threatCell; }
[Serializable] public sealed class KaitIntent { public KaitIntentType type; public Vector2Int target, direction; }
[Serializable] public sealed class KaitEnemy
{
    public int id, hp, maxHp;
    public KaitEnemyType type;
    public Vector2Int pos;
    public KaitEnemyLife life;
    public KaitIntent intent = new KaitIntent();
    public bool IsHardBlocker => type == KaitEnemyType.Guard || type == KaitEnemyType.Elite;
}
[Serializable] public sealed class KaitSpawnRequest
{
    public int tier, turnsUntilSpawn;
    public Vector2Int sourceThreatCell, targetCell;
    public KaitSpawnState state;
    public KaitDirection initialDirection;
}
[Serializable] public sealed class KaitEnemyAction
{
    public int enemyId; public KaitIntentType type; public Vector2Int from, to;
    public readonly List<Vector2Int> affectedCells = new List<Vector2Int>(); public bool hitKate;
}
[Serializable] public sealed class KaitThreatMotion
{ public int value; public Vector2Int from, to; public bool merged; }

public sealed class KaitTurnResult
{
    public bool valid, turnComplete, awaitingTurnChoice;
    public readonly List<KaitDirection> availableDirections = new List<KaitDirection>();
    public readonly List<Vector2Int> katePath = new List<Vector2Int>();
    public readonly List<int> pathMomentum = new List<int>();
    public readonly List<int> killedEnemyIds = new List<int>();
    public readonly List<Vector2Int> killedEnemyCells = new List<Vector2Int>();
    public readonly List<KaitMergeEvent> merges = new List<KaitMergeEvent>();
    public readonly List<KaitEnemyAction> enemyActions = new List<KaitEnemyAction>();
    public readonly List<KaitThreatMotion> threatMotions = new List<KaitThreatMotion>();
    public readonly List<Vector2Int> spawnedEnemyCells = new List<Vector2Int>();
    public readonly List<Vector2Int> newThreatCells = new List<Vector2Int>();
    public int[,] threatBefore, threatAfter;
    public int slideDistance, damagedEnemyId = -1, damageDealt, enemyHpAfter = -1, momentumBefore, momentumAfter;
    public Vector2Int blockedEnemyCell = new Vector2Int(-1, -1);
    public string message;
}

public sealed class KaitRun
{
    public const int BattleSize = 9, DefaultThreatSize = 7;
    public readonly KaitBalanceConfig config;
    public readonly int[,] threat;
    public readonly bool[,] walls = new bool[BattleSize, BattleSize];
    public readonly List<KaitEnemy> enemies = new List<KaitEnemy>();
    public readonly List<KaitSpawnRequest> spawns = new List<KaitSpawnRequest>();
    public int ThreatSize => config.threatSize;
    public Vector2Int katePos { get; private set; }
    public int turn { get; private set; }
    public int kills { get; private set; }
    public int highestThreat { get; private set; }
    public int momentum { get; private set; }
    public bool chainActive { get; private set; }
    public KaitDirection currentDirection { get; private set; }
    public int threatLocks { get; private set; }
    public bool ended { get; private set; }
    public string endReason { get; private set; }
    public int mapIndex { get; private set; }

    private System.Random random;
    private int nextEnemyId;

    private static readonly string[][] Maps =
    {
        new[] { "#########", "#.......#", "#.......#", "#.......#", "#.......#", "#.......#", "#.......#", "#.......#", "#########" },
        new[] { "#########", "#.......#", "#.......#", "#...#...#", "#.......#", "#.....#.#", "#.......#", "#.......#", "#########" },
        new[] { "#########", "#.......#", "#..#....#", "#.......#", "#.....#.#", "#.......#", "#...#...#", "#.......#", "#########" }
    };

    public KaitRun(KaitBalanceConfig balance = null)
    {
        config = balance ?? new KaitBalanceConfig();
        config.threatSize = Mathf.Max(2, config.threatSize);
        threat = new int[config.threatSize, config.threatSize];
    }

    public void Reset(int seed)
    {
        random = new System.Random(seed); Array.Clear(threat, 0, threat.Length); Array.Clear(walls, 0, walls.Length);
        enemies.Clear(); spawns.Clear(); nextEnemyId = 1; turn = kills = threatLocks = 0; highestThreat = 2;
        momentum = 0; chainActive = ended = false; endReason = string.Empty; mapIndex = random.Next(Maps.Length);
        string[] map = Maps[mapIndex];
        for (int y = 0; y < BattleSize; y++) for (int x = 0; x < BattleSize; x++) walls[x, BattleSize - 1 - y] = map[y][x] == '#';
        katePos = FindOpenNearCenter();
        for (int i = 0; i < config.initialThreatTiles; i++) SpawnThreatTwo();
        GenerateIntents();
    }

    public KaitTurnResult TryTurn(KaitDirection direction)
    {
        var result = new KaitTurnResult();
        if (ended) { result.message = "本局已结束"; return result; }
        if (chainActive) { result.message = "请选择击杀后的转向"; return result; }
        if (!CanEnterFrom(katePos, direction)) { result.message = "紧贴障碍，未消耗回合"; return result; }
        result.valid = true; currentDirection = direction; momentum = config.baseMomentum; chainActive = true;
        result.threatBefore = CopyThreat();
        result.merges.AddRange(MoveThreat(direction, result.threatMotions));
        foreach (KaitMergeEvent merge in result.merges) QueueSpawn(merge, direction);
        result.threatAfter = CopyThreat(); ResolveKateSegment(result); result.slideDistance = result.katePath.Count; return result;
    }

    public KaitTurnResult ContinueChain(KaitDirection direction)
    {
        var result = new KaitTurnResult();
        if (!chainActive) { result.message = "当前没有可继续的连斩"; return result; }
        if (!AllowedTurnDirections().Contains(direction)) { result.message = "击杀后只能直行、左转或右转"; return result; }
        result.valid = true; currentDirection = direction; ResolveKateSegment(result); result.slideDistance = result.katePath.Count; return result;
    }

    public List<KaitDirection> AllowedTurnDirections()
    {
        var choices = new List<KaitDirection>(); if (!chainActive) return choices;
        foreach (KaitDirection d in new[] { TurnLeft(currentDirection), currentDirection, TurnRight(currentDirection) })
            if (CanEnterFrom(katePos, d)) choices.Add(d);
        return choices;
    }

    private void ResolveKateSegment(KaitTurnResult result)
    {
        result.momentumBefore = momentum; Vector2Int delta = Delta(currentDirection);
        for (int guard = 0; guard < 64; guard++)
        {
            Vector2Int next = katePos + delta;
            if (IsHardBlocked(next)) { FinishTurn(result); return; }
            KaitEnemy enemy = EnemyAt(next);
            if (enemy == null)
            {
                katePos = next; momentum += config.momentumPerEmptyCell;
                result.katePath.Add(katePos); result.pathMomentum.Add(momentum); continue;
            }
            int damage = momentum; enemy.hp = Mathf.Max(0, enemy.hp - damage);
            result.damagedEnemyId = enemy.id; result.damageDealt = damage; result.enemyHpAfter = enemy.hp; result.blockedEnemyCell = enemy.pos;
            if (enemy.hp > 0) { FinishTurn(result); return; }
            enemy.life = KaitEnemyLife.Dead; kills++; result.killedEnemyIds.Add(enemy.id); result.killedEnemyCells.Add(enemy.pos);
            result.blockedEnemyCell = new Vector2Int(-1, -1);
            katePos = next; result.katePath.Add(katePos); momentum = Mathf.Max(1, momentum - config.momentumLossOnKill); result.pathMomentum.Add(momentum);
            List<KaitDirection> choices = AllowedTurnDirections();
            if (choices.Count == 0) { FinishTurn(result); return; }
            result.awaitingTurnChoice = true; result.availableDirections.AddRange(choices); result.momentumAfter = momentum;
            result.message = "击杀成功：选择直行、左转或右转继续"; return;
        }
        FinishTurn(result);
    }

    private void FinishTurn(KaitTurnResult result)
    {
        chainActive = false; result.turnComplete = true; result.momentumAfter = momentum; ResolveEnemyIntents(result);
        if (!ended)
        {
            AgePreparingEnemies(); AdvanceSpawnRequests(result);
            for (int i = 0; i < config.newThreatTilesPerTurn; i++) { Vector2Int p = SpawnThreatTwo(); if (p.x >= 0) result.newThreatCells.Add(p); }
            if (ThreatLocked()) ResetLockedThreat();
        }
        turn++; momentum = 0; GenerateIntents();
        if (string.IsNullOrEmpty(result.message))
            result.message = result.damagedEnemyId >= 0 && result.enemyHpAfter > 0
                ? $"造成 {result.damageDealt} 点伤害，敌人剩余 {result.enemyHpAfter} HP"
                : result.killedEnemyIds.Count > 0 ? "击杀后无可行方向，回合结束" : "回合完成";
    }

    public KaitEnemy EnemyAt(Vector2Int p) => enemies.Find(e => e.life != KaitEnemyLife.Dead && e.pos == p);
    public KaitSpawnRequest SpawnAt(Vector2Int p) => spawns.Find(s => s.targetCell == p);
    public Vector2Int MapThreatToBattle(Vector2Int p) => p + Vector2Int.one;

    private void ResolveEnemyIntents(KaitTurnResult result)
    {
        enemies.Sort((a, b) => a.id.CompareTo(b.id));
        foreach (KaitEnemy enemy in enemies)
        {
            if (enemy.life != KaitEnemyLife.Active) continue;
            KaitIntent intent = enemy.intent; var action = new KaitEnemyAction { enemyId = enemy.id, type = intent.type, from = enemy.pos, to = enemy.pos };
            if (intent.type == KaitIntentType.Move && IsFreeForEnemy(intent.target)) { enemy.pos = intent.target; action.to = enemy.pos; }
            else if (intent.type == KaitIntentType.Melee)
            { action.affectedCells.Add(intent.target); action.hitKate = katePos == intent.target; if (action.hitKate) End("Kate Defeated"); }
            else if (intent.type == KaitIntentType.LineShot)
            { action.affectedCells.AddRange(ShotCells(enemy.pos, intent.direction)); action.hitKate = action.affectedCells.Contains(katePos); if (action.hitKate) End("Kate Defeated"); }
            result.enemyActions.Add(action); if (ended) return;
        }
    }

    private void GenerateIntents()
    {
        foreach (KaitEnemy enemy in enemies)
        {
            enemy.intent = new KaitIntent(); if (enemy.life != KaitEnemyLife.Active) continue; Vector2Int diff = katePos - enemy.pos;
            if (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) == 1) { enemy.intent.type = KaitIntentType.Melee; enemy.intent.target = katePos; }
            else if (enemy.type == KaitEnemyType.Archer && (diff.x == 0 || diff.y == 0) && HasLineOfSight(enemy.pos, katePos))
            { enemy.intent.type = KaitIntentType.LineShot; enemy.intent.direction = new Vector2Int(Math.Sign(diff.x), Math.Sign(diff.y)); enemy.intent.target = katePos; }
            else
            {
                Vector2Int step = Mathf.Abs(diff.x) >= Mathf.Abs(diff.y) ? new Vector2Int(Math.Sign(diff.x), 0) : new Vector2Int(0, Math.Sign(diff.y));
                if (!IsFreeForEnemy(enemy.pos + step)) step = step.x != 0 ? new Vector2Int(0, Math.Sign(diff.y)) : new Vector2Int(Math.Sign(diff.x), 0);
                enemy.intent.type = KaitIntentType.Move; enemy.intent.target = enemy.pos + step; enemy.intent.direction = step;
            }
        }
    }

    private void AgePreparingEnemies() { foreach (KaitEnemy e in enemies) if (e.life == KaitEnemyLife.Preparing) e.life = KaitEnemyLife.Active; }

    private sealed class ThreatToken { public int value; public readonly List<Vector2Int> sources = new List<Vector2Int>(); public bool merged; }
    private List<KaitMergeEvent> MoveThreat(KaitDirection direction, List<KaitThreatMotion> motions)
    {
        var merges = new List<KaitMergeEvent>(); bool horizontal = direction == KaitDirection.Left || direction == KaitDirection.Right;
        bool reverse = direction == KaitDirection.Right || direction == KaitDirection.Up;
        for (int line = 0; line < ThreatSize; line++)
        {
            var values = new List<ThreatToken>();
            for (int i = 0; i < ThreatSize; i++)
            {
                int index = reverse ? ThreatSize - 1 - i : i, x = horizontal ? index : line, y = horizontal ? line : index;
                if (threat[x, y] == 0) continue; var token = new ThreatToken { value = threat[x, y] }; token.sources.Add(new Vector2Int(x, y)); values.Add(token);
            }
            var packed = new List<ThreatToken>();
            for (int i = 0; i < values.Count; i++)
            {
                if (i + 1 < values.Count && values[i].value == values[i + 1].value)
                { var merged = new ThreatToken { value = values[i].value * 2, merged = true }; merged.sources.AddRange(values[i].sources); merged.sources.AddRange(values[++i].sources); packed.Add(merged); }
                else packed.Add(values[i]);
            }
            for (int i = 0; i < ThreatSize; i++)
            {
                int index = reverse ? ThreatSize - 1 - i : i, x = horizontal ? index : line, y = horizontal ? line : index;
                ThreatToken token = i < packed.Count ? packed[i] : null; threat[x, y] = token?.value ?? 0; if (token == null) continue;
                Vector2Int destination = new Vector2Int(x, y);
                foreach (Vector2Int source in token.sources) motions.Add(new KaitThreatMotion { value = token.merged ? token.value / 2 : token.value, from = source, to = destination, merged = token.merged });
                if (token.merged) merges.Add(new KaitMergeEvent { resultValue = token.value, threatCell = destination }); highestThreat = Mathf.Max(highestThreat, token.value);
            }
        }
        return merges;
    }

    private Vector2Int SpawnThreatTwo()
    {
        var empty = new List<Vector2Int>();
        for (int y = 0; y < ThreatSize; y++) for (int x = 0; x < ThreatSize; x++) if (threat[x, y] == 0) empty.Add(new Vector2Int(x, y));
        if (empty.Count == 0) return new Vector2Int(-1, -1); Vector2Int p = empty[random.Next(empty.Count)]; threat[p.x, p.y] = 2; return p;
    }

    private void QueueSpawn(KaitMergeEvent merge, KaitDirection direction)
    {
        int tier = Mathf.Clamp((int)Mathf.Log(merge.resultValue, 2f) - 1, 1, 4); Vector2Int exact = MapThreatToBattle(merge.threatCell);
        Vector2Int target = walls[exact.x, exact.y] ? FindDeterministicObstacleFallback(exact, direction) : exact; if (target.x < 0) return;
        spawns.Add(new KaitSpawnRequest { tier = tier, sourceThreatCell = merge.threatCell, targetCell = target, turnsUntilSpawn = 1, state = KaitSpawnState.Preview, initialDirection = direction });
    }

    private void AdvanceSpawnRequests(KaitTurnResult result)
    {
        int made = 0, active = enemies.FindAll(e => e.life != KaitEnemyLife.Dead).Count, hard = enemies.FindAll(e => e.life != KaitEnemyLife.Dead && e.IsHardBlocker).Count;
        for (int i = 0; i < spawns.Count && made < config.maxMaterializePerTurn && active < config.maxActiveEnemies;)
        {
            KaitSpawnRequest request = spawns[i]; request.turnsUntilSpawn = Mathf.Max(0, request.turnsUntilSpawn - 1); request.state = request.turnsUntilSpawn > 0 ? KaitSpawnState.Preview : KaitSpawnState.Ready;
            if (request.turnsUntilSpawn > 0 || !IsFreeForSpawn(request.targetCell)) { i++; continue; }
            KaitEnemyType type = TierType(request.tier); if ((type == KaitEnemyType.Guard || type == KaitEnemyType.Elite) && hard >= config.maxHardBlockers) type = KaitEnemyType.Grunt;
            int hp = type == KaitEnemyType.Grunt || type == KaitEnemyType.Archer ? 1 : 3;
            enemies.Add(new KaitEnemy { id = nextEnemyId++, type = type, pos = request.targetCell, hp = hp, maxHp = hp, life = KaitEnemyLife.Preparing });
            if (type == KaitEnemyType.Guard || type == KaitEnemyType.Elite) hard++; active++; made++; result.spawnedEnemyCells.Add(request.targetCell); spawns.RemoveAt(i);
        }
    }

    private static KaitEnemyType TierType(int tier) => tier <= 1 ? KaitEnemyType.Grunt : tier == 2 ? KaitEnemyType.Archer : tier == 3 ? KaitEnemyType.Guard : KaitEnemyType.Elite;
    private Vector2Int FindDeterministicObstacleFallback(Vector2Int origin, KaitDirection initial)
    {
        KaitDirection reverse = Opposite(initial), left = TurnLeft(reverse), right = TurnRight(reverse);
        for (int d = 1; d < BattleSize; d++) foreach (KaitDirection direction in new[] { reverse, left, right })
        { Vector2Int p = origin + Delta(direction) * d; if (Inside(p) && !walls[p.x, p.y]) return p; }
        return new Vector2Int(-1, -1);
    }

    private void ResetLockedThreat() { threatLocks++; Array.Clear(threat, 0, threat.Length); for (int i = 0; i < config.initialThreatTiles; i++) SpawnThreatTwo(); }
    private bool ThreatLocked()
    {
        for (int y = 0; y < ThreatSize; y++) for (int x = 0; x < ThreatSize; x++)
        { if (threat[x, y] == 0) return false; if (x + 1 < ThreatSize && threat[x, y] == threat[x + 1, y]) return false; if (y + 1 < ThreatSize && threat[x, y] == threat[x, y + 1]) return false; }
        return true;
    }

    private Vector2Int FindOpenNearCenter()
    { var c = new List<Vector2Int>(); for (int y = 3; y <= 5; y++) for (int x = 3; x <= 5; x++) if (!walls[x, y]) c.Add(new Vector2Int(x, y)); return c[random.Next(c.Count)]; }
    private bool CanEnterFrom(Vector2Int from, KaitDirection d) { Vector2Int p = from + Delta(d); return !IsHardBlocked(p) || EnemyAt(p) != null; }
    private bool IsHardBlocked(Vector2Int p) => !Inside(p) || walls[p.x, p.y];
    private bool IsFreeForEnemy(Vector2Int p) => !IsHardBlocked(p) && p != katePos && EnemyAt(p) == null && SpawnAt(p) == null;
    private bool IsFreeForSpawn(Vector2Int p) => Inside(p) && !walls[p.x, p.y] && p != katePos && EnemyAt(p) == null;
    private static bool Inside(Vector2Int p) => p.x >= 0 && p.x < BattleSize && p.y >= 0 && p.y < BattleSize;
    private bool HasLineOfSight(Vector2Int from, Vector2Int to)
    { Vector2Int step = new Vector2Int(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y)); for (Vector2Int p = from + step; p != to; p += step) if (IsHardBlocked(p) || EnemyAt(p) != null) return false; return true; }
    private List<Vector2Int> ShotCells(Vector2Int from, Vector2Int direction)
    { var cells = new List<Vector2Int>(); for (Vector2Int p = from + direction; Inside(p) && !IsHardBlocked(p); p += direction) { if (EnemyAt(p) != null) break; cells.Add(p); } return cells; }
    private int[,] CopyThreat() { var copy = new int[ThreatSize, ThreatSize]; Array.Copy(threat, copy, threat.Length); return copy; }
    private void End(string reason) { ended = true; endReason = reason; chainActive = false; }

    public static Vector2Int Delta(KaitDirection d)
    { switch (d) { case KaitDirection.Up: return Vector2Int.up; case KaitDirection.Down: return Vector2Int.down; case KaitDirection.Left: return Vector2Int.left; default: return Vector2Int.right; } }
    public static KaitDirection TurnLeft(KaitDirection d)
    { switch (d) { case KaitDirection.Up: return KaitDirection.Left; case KaitDirection.Left: return KaitDirection.Down; case KaitDirection.Down: return KaitDirection.Right; default: return KaitDirection.Up; } }
    public static KaitDirection TurnRight(KaitDirection d) => TurnLeft(TurnLeft(TurnLeft(d)));
    public static KaitDirection Opposite(KaitDirection d) => TurnLeft(TurnLeft(d));
}
