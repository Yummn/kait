using System;
using System.Collections.Generic;
using UnityEngine;

public enum KaitDirection { Up, Down, Left, Right }
public enum KaitEnemyType { Grunt = 1, Guard = 2, Archer = 3, Elite = 4 }
public enum KaitEnemyLife { Preparing, Active, Dead }
public enum KaitIntentType { None, Move, Melee, LineShot }
public enum KaitSkill { Curse, Mirage, FearSlash }

[Serializable]
public sealed class KaitMergeEvent
{
    public int resultValue;
    public Vector2Int threatCell;
}

[Serializable]
public sealed class KaitIntent
{
    public KaitIntentType type;
    public Vector2Int target;
    public Vector2Int direction;
}

[Serializable]
public sealed class KaitEnemy
{
    public int id;
    public KaitEnemyType type;
    public Vector2Int pos;
    public int threshold;
    public KaitEnemyLife life;
    public int curseTurns;
    public KaitIntent intent = new KaitIntent();

    public int EffectiveThreshold => Mathf.Max(1, threshold - (curseTurns > 0 ? 2 : 0));
}

[Serializable]
public sealed class KaitSpawnRequest
{
    public int tier;
    public Vector2Int sourceThreatCell;
    public Vector2Int targetCell;
    public int turnsUntilSpawn;
}

[Serializable]
public sealed class KaitMirage
{
    public Vector2Int pos;
    public int turnsLeft;
}

public sealed class KaitTurnResult
{
    public bool valid;
    public readonly List<Vector2Int> katePath = new List<Vector2Int>();
    public readonly List<int> killedEnemyIds = new List<int>();
    public readonly List<KaitMergeEvent> merges = new List<KaitMergeEvent>();
    public int slideDistance;
    public string message;
}

public sealed class KaitRun
{
    public const int BattleSize = 9;
    public const int ThreatSize = 4;

    public readonly int[,] threat = new int[ThreatSize, ThreatSize];
    public readonly bool[,] walls = new bool[BattleSize, BattleSize];
    public readonly List<KaitEnemy> enemies = new List<KaitEnemy>();
    public readonly List<KaitSpawnRequest> spawns = new List<KaitSpawnRequest>();
    public readonly List<KaitMirage> mirages = new List<KaitMirage>();
    public readonly HashSet<KaitSkill> skills = new HashSet<KaitSkill>();
    public readonly Dictionary<KaitSkill, int> cooldowns = new Dictionary<KaitSkill, int>();

    public Vector2Int katePos { get; private set; }
    public int turn { get; private set; }
    public int kills { get; private set; }
    public int highestThreat { get; private set; }
    public bool ended { get; private set; }
    public string endReason { get; private set; }
    public int pendingSkillChoices { get; private set; }
    public KaitSkill? armedSkill { get; private set; }
    public int mapIndex { get; private set; }

    private System.Random random;
    private int nextEnemyId;
    private readonly HashSet<int> milestones = new HashSet<int>();

    private static readonly string[][] Maps =
    {
        new[] { "#########", "#.......#", "#.......#", "#..#.#..#", "#.......#", "#..#.#..#", "#.......#", "#.......#", "#########" },
        new[] { "#########", "#.......#", "#..#.#..#", "#.......#", "#.......#", "#.......#", "#..#.#..#", "#.......#", "#########" },
        new[] { "#########", "#.......#", "#...#...#", "#...#...#", "#.......#", "#...#...#", "#...#...#", "#.......#", "#########" }
    };

    public void Reset(int seed)
    {
        random = new System.Random(seed);
        Array.Clear(threat, 0, threat.Length);
        Array.Clear(walls, 0, walls.Length);
        enemies.Clear();
        spawns.Clear();
        mirages.Clear();
        skills.Clear();
        cooldowns.Clear();
        milestones.Clear();
        cooldowns[KaitSkill.Curse] = 0;
        cooldowns[KaitSkill.Mirage] = 0;
        cooldowns[KaitSkill.FearSlash] = 0;
        armedSkill = null;
        pendingSkillChoices = 0;
        nextEnemyId = 1;
        turn = 0;
        kills = 0;
        highestThreat = 2;
        ended = false;
        endReason = string.Empty;
        mapIndex = random.Next(Maps.Length);

        string[] map = Maps[mapIndex];
        for (int y = 0; y < BattleSize; y++)
            for (int x = 0; x < BattleSize; x++)
                walls[x, BattleSize - 1 - y] = map[y][x] == '#';

        katePos = FindOpenNearCenter();
        SpawnThreatTwo();
        SpawnThreatTwo();
    }

    public bool SelectOrArmSkill(KaitSkill skill, out string message)
    {
        if (pendingSkillChoices > 0 && !skills.Contains(skill))
        {
            skills.Add(skill);
            pendingSkillChoices--;
            message = "已解锁：" + SkillName(skill);
            return true;
        }

        if (!skills.Contains(skill))
        {
            message = pendingSkillChoices > 0 ? "请选择一个尚未解锁的战技" : "尚未解锁";
            return false;
        }

        if (cooldowns[skill] > 0)
        {
            message = $"冷却中：{cooldowns[skill]} 回合";
            return false;
        }

        if (skill == KaitSkill.Curse && enemies.Find(e => e.life != KaitEnemyLife.Dead) == null)
        {
            message = "当前没有可诅咒的敌人";
            return false;
        }

        armedSkill = armedSkill == skill ? null : skill;
        message = armedSkill.HasValue ? "已预选：" + SkillName(skill) : "已取消战技";
        return true;
    }

    public KaitTurnResult TryTurn(KaitDirection direction)
    {
        var result = new KaitTurnResult();
        if (ended)
        {
            result.message = "本局已结束";
            return result;
        }

        Vector2Int delta = Delta(direction);
        KaitEnemy adjacentEnemy = EnemyAt(katePos + delta);
        if (IsHardBlocked(katePos + delta) && adjacentEnemy == null)
        {
            result.message = "紧贴障碍，未消耗回合";
            return result;
        }

        result.valid = true;
        Vector2Int start = katePos;
        bool useMirage = armedSkill == KaitSkill.Mirage;
        bool useFear = armedSkill == KaitSkill.FearSlash;
        bool fearUsed = false;

        if (armedSkill == KaitSkill.Curse)
        {
            KaitEnemy target = FindCurseTarget();
            if (target != null)
            {
                target.curseTurns = 3;
                cooldowns[KaitSkill.Curse] = 4;
            }
            armedSkill = null;
        }

        int charge = 0;
        for (int guard = 0; guard < 64; guard++)
        {
            Vector2Int next = katePos + delta;
            if (IsHardBlocked(next)) break;

            KaitEnemy enemy = EnemyAt(next);
            if (enemy != null)
            {
                charge++;
                if (charge >= enemy.EffectiveThreshold)
                {
                    enemy.life = KaitEnemyLife.Dead;
                    result.killedEnemyIds.Add(enemy.id);
                    kills++;
                    katePos = next;
                    result.katePath.Add(katePos);
                    charge = 0;
                    continue;
                }

                if (useFear && !fearUsed)
                {
                    Vector2Int pushed = enemy.pos + delta;
                    fearUsed = true;
                    if (IsFreeForEnemy(pushed)) enemy.pos = pushed;
                }
                break;
            }

            katePos = next;
            result.katePath.Add(katePos);
            charge++;
        }

        result.slideDistance = result.katePath.Count;
        if (useMirage && katePos != start)
        {
            mirages.Add(new KaitMirage { pos = start, turnsLeft = 2 });
            cooldowns[KaitSkill.Mirage] = 5;
            armedSkill = null;
        }
        if (useFear && fearUsed)
        {
            cooldowns[KaitSkill.FearSlash] = 4;
            armedSkill = null;
        }

        ResolveEnemyIntents();
        if (!ended)
        {
            AgeStatuses();
            AdvanceSpawnRequests();
            result.merges.AddRange(MoveThreat(direction));
            foreach (KaitMergeEvent merge in result.merges) QueueSpawn(merge);
            SpawnThreatTwo();
            CheckMilestones();
            if (ThreatLocked()) End("Threat Overload");
        }

        turn++;
        GenerateIntents();
        result.message = result.killedEnemyIds.Count > 0 ? $"穿透击杀 ×{result.killedEnemyIds.Count}" : "回合完成";
        return result;
    }

    public string SkillName(KaitSkill skill)
    {
        switch (skill)
        {
            case KaitSkill.Curse: return "咒剑诅咒";
            case KaitSkill.Mirage: return "次级幻影";
            default: return "惊惧斩";
        }
    }

    public KaitEnemy EnemyAt(Vector2Int p) => enemies.Find(e => e.life != KaitEnemyLife.Dead && e.pos == p);
    public KaitSpawnRequest SpawnAt(Vector2Int p) => spawns.Find(s => s.targetCell == p);
    public KaitMirage MirageAt(Vector2Int p) => mirages.Find(m => m.pos == p && m.turnsLeft > 0);

    private void ResolveEnemyIntents()
    {
        enemies.Sort((a, b) => a.id.CompareTo(b.id));
        foreach (KaitEnemy enemy in enemies)
        {
            if (enemy.life != KaitEnemyLife.Active) continue;
            KaitIntent intent = enemy.intent;
            if (intent.type == KaitIntentType.Move && IsFreeForEnemy(intent.target)) enemy.pos = intent.target;
            else if (intent.type == KaitIntentType.Melee && katePos == intent.target) { End("Kate Defeated"); return; }
            else if (intent.type == KaitIntentType.LineShot && IsOnShotLine(enemy.pos, intent.direction, katePos)) { End("Kate Defeated"); return; }
        }
    }

    private void GenerateIntents()
    {
        foreach (KaitEnemy enemy in enemies)
        {
            enemy.intent = new KaitIntent();
            if (enemy.life != KaitEnemyLife.Active) continue;
            Vector2Int diff = katePos - enemy.pos;
            if (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) == 1)
            {
                enemy.intent.type = KaitIntentType.Melee;
                enemy.intent.target = katePos;
            }
            else if (enemy.type == KaitEnemyType.Archer && (diff.x == 0 || diff.y == 0) && HasLineOfSight(enemy.pos, katePos))
            {
                enemy.intent.type = KaitIntentType.LineShot;
                enemy.intent.direction = new Vector2Int(Math.Sign(diff.x), Math.Sign(diff.y));
                enemy.intent.target = katePos;
            }
            else
            {
                Vector2Int step = Mathf.Abs(diff.x) >= Mathf.Abs(diff.y)
                    ? new Vector2Int(Math.Sign(diff.x), 0)
                    : new Vector2Int(0, Math.Sign(diff.y));
                if (!IsFreeForEnemy(enemy.pos + step))
                    step = step.x != 0 ? new Vector2Int(0, Math.Sign(diff.y)) : new Vector2Int(Math.Sign(diff.x), 0);
                enemy.intent.type = KaitIntentType.Move;
                enemy.intent.target = enemy.pos + step;
            }
        }
    }

    private void AgeStatuses()
    {
        foreach (KaitEnemy enemy in enemies)
        {
            if (enemy.life == KaitEnemyLife.Preparing) enemy.life = KaitEnemyLife.Active;
            if (enemy.curseTurns > 0) enemy.curseTurns--;
        }
        for (int i = mirages.Count - 1; i >= 0; i--)
        {
            mirages[i].turnsLeft--;
            if (mirages[i].turnsLeft <= 0) mirages.RemoveAt(i);
        }
        var keys = new List<KaitSkill>(cooldowns.Keys);
        foreach (KaitSkill skill in keys) if (cooldowns[skill] > 0) cooldowns[skill]--;
    }

    private List<KaitMergeEvent> MoveThreat(KaitDirection direction)
    {
        var merges = new List<KaitMergeEvent>();
        bool horizontal = direction == KaitDirection.Left || direction == KaitDirection.Right;
        bool reverse = direction == KaitDirection.Right || direction == KaitDirection.Up;

        for (int line = 0; line < ThreatSize; line++)
        {
            var values = new List<int>();
            for (int i = 0; i < ThreatSize; i++)
            {
                int index = reverse ? ThreatSize - 1 - i : i;
                int x = horizontal ? index : line;
                int y = horizontal ? line : index;
                if (threat[x, y] != 0) values.Add(threat[x, y]);
            }

            var packed = new List<int>();
            for (int i = 0; i < values.Count; i++)
            {
                if (i + 1 < values.Count && values[i] == values[i + 1])
                {
                    packed.Add(values[i] * 2);
                    i++;
                }
                else packed.Add(values[i]);
            }
            while (packed.Count < ThreatSize) packed.Add(0);

            for (int i = 0; i < ThreatSize; i++)
            {
                int index = reverse ? ThreatSize - 1 - i : i;
                int x = horizontal ? index : line;
                int y = horizontal ? line : index;
                threat[x, y] = packed[i];
                if (packed[i] > 0 && i < packed.Count && IsMergeResultAt(values, packed, i))
                    merges.Add(new KaitMergeEvent { resultValue = packed[i], threatCell = new Vector2Int(x, y) });
                highestThreat = Mathf.Max(highestThreat, packed[i]);
            }
        }
        return merges;
    }

    private static bool IsMergeResultAt(List<int> source, List<int> packed, int packedIndex)
    {
        int outIndex = 0;
        for (int i = 0; i < source.Count; i++)
        {
            bool merged = i + 1 < source.Count && source[i] == source[i + 1];
            if (outIndex == packedIndex) return merged;
            if (merged) i++;
            outIndex++;
        }
        return false;
    }

    private void SpawnThreatTwo()
    {
        var empty = new List<Vector2Int>();
        for (int y = 0; y < ThreatSize; y++)
            for (int x = 0; x < ThreatSize; x++)
                if (threat[x, y] == 0) empty.Add(new Vector2Int(x, y));
        if (empty.Count == 0) return;
        Vector2Int p = empty[random.Next(empty.Count)];
        threat[p.x, p.y] = 2;
    }

    private void QueueSpawn(KaitMergeEvent merge)
    {
        int tier = Mathf.Clamp((int)Mathf.Log(merge.resultValue, 2f) - 1, 1, 4);
        Vector2Int target = FindSpawnCell(merge.threatCell);
        spawns.Add(new KaitSpawnRequest { tier = tier, sourceThreatCell = merge.threatCell, targetCell = target, turnsUntilSpawn = 1 });
    }

    private void AdvanceSpawnRequests()
    {
        for (int i = spawns.Count - 1; i >= 0; i--)
        {
            KaitSpawnRequest request = spawns[i];
            request.turnsUntilSpawn--;
            if (request.turnsUntilSpawn > 0) continue;
            Vector2Int target = IsFreeForSpawn(request.targetCell) ? request.targetCell : FindSpawnCell(request.sourceThreatCell);
            if (target.x < 0) { request.turnsUntilSpawn = 1; continue; }
            KaitEnemyType type = request.tier == 1 ? KaitEnemyType.Grunt : request.tier == 2 ? KaitEnemyType.Guard : request.tier == 3 ? KaitEnemyType.Archer : KaitEnemyType.Elite;
            int threshold = type == KaitEnemyType.Grunt ? 1 : type == KaitEnemyType.Guard ? 3 : type == KaitEnemyType.Archer ? 2 : 4;
            enemies.Add(new KaitEnemy { id = nextEnemyId++, type = type, pos = target, threshold = threshold, life = KaitEnemyLife.Preparing });
            spawns.RemoveAt(i);
        }
    }

    private Vector2Int FindSpawnCell(Vector2Int threatCell)
    {
        Vector2Int anchor = new Vector2Int(1 + threatCell.x * 2, 1 + threatCell.y * 2);
        for (int radius = 0; radius <= 2; radius++)
        {
            var candidates = new List<Vector2Int>();
            for (int y = -radius; y <= radius; y++)
                for (int x = -radius; x <= radius; x++)
                    if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) == radius && IsFreeForSpawn(anchor + new Vector2Int(x, y)))
                        candidates.Add(anchor + new Vector2Int(x, y));
            if (candidates.Count > 0) return candidates[random.Next(candidates.Count)];
        }
        return new Vector2Int(-1, -1);
    }

    private void CheckMilestones()
    {
        foreach (int value in new[] { 4, 8, 16 })
        {
            if (highestThreat < value || milestones.Contains(value)) continue;
            milestones.Add(value);
            if (value == 4 || value == 8) pendingSkillChoices++;
            else
            {
                // The compact demo treats the 16 milestone as access to the remaining skill.
                pendingSkillChoices++;
            }
        }
    }

    private bool ThreatLocked()
    {
        for (int y = 0; y < ThreatSize; y++)
            for (int x = 0; x < ThreatSize; x++)
            {
                if (threat[x, y] == 0) return false;
                if (x + 1 < ThreatSize && threat[x, y] == threat[x + 1, y]) return false;
                if (y + 1 < ThreatSize && threat[x, y] == threat[x, y + 1]) return false;
            }
        return true;
    }

    private KaitEnemy FindCurseTarget()
    {
        KaitEnemy best = null;
        int bestScore = int.MinValue;
        foreach (KaitEnemy enemy in enemies)
        {
            if (enemy.life == KaitEnemyLife.Dead) continue;
            int score = enemy.threshold * 20 - Mathf.Abs(enemy.pos.x - katePos.x) - Mathf.Abs(enemy.pos.y - katePos.y);
            if (score > bestScore) { bestScore = score; best = enemy; }
        }
        return best;
    }

    private Vector2Int FindOpenNearCenter()
    {
        var choices = new List<Vector2Int>();
        for (int y = 3; y <= 5; y++) for (int x = 3; x <= 5; x++) if (!walls[x, y]) choices.Add(new Vector2Int(x, y));
        return choices[random.Next(choices.Count)];
    }

    private bool IsHardBlocked(Vector2Int p) => !Inside(p) || walls[p.x, p.y] || MirageAt(p) != null;
    private bool IsFreeForEnemy(Vector2Int p) => !IsHardBlocked(p) && p != katePos && EnemyAt(p) == null && SpawnAt(p) == null;
    private bool IsFreeForSpawn(Vector2Int p) => Inside(p) && !walls[p.x, p.y] && p != katePos && EnemyAt(p) == null && SpawnAt(p) == null && MirageAt(p) == null;
    private static bool Inside(Vector2Int p) => p.x >= 0 && p.x < BattleSize && p.y >= 0 && p.y < BattleSize;

    private bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        Vector2Int step = new Vector2Int(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y));
        for (Vector2Int p = from + step; p != to; p += step) if (IsHardBlocked(p) || EnemyAt(p) != null) return false;
        return true;
    }

    private bool IsOnShotLine(Vector2Int from, Vector2Int direction, Vector2Int target)
    {
        for (Vector2Int p = from + direction; Inside(p) && !IsHardBlocked(p); p += direction)
        {
            if (EnemyAt(p) != null) return false;
            if (p == target) return true;
        }
        return false;
    }

    private void End(string reason) { ended = true; endReason = reason; }

    public static Vector2Int Delta(KaitDirection direction)
    {
        switch (direction)
        {
            case KaitDirection.Up: return Vector2Int.up;
            case KaitDirection.Down: return Vector2Int.down;
            case KaitDirection.Left: return Vector2Int.left;
            default: return Vector2Int.right;
        }
    }
}
