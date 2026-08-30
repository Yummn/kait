using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class KaitCoreTests
{
    [Test]
    public void Reset_UsesSevenBySevenAndFourTwos()
    {
        KaitRun run = OpenRun(2048);
        Assert.AreEqual(7, run.ThreatSize);
        Assert.AreEqual(4, CountThreat(run, 2));
        Assert.IsFalse(run.walls[run.katePos.x, run.katePos.y]);
    }

    [Test]
    public void SevenWideRow_FourTwosMergeOnceIntoTwoFours()
    {
        KaitRun run = OpenRun(1); ClearThreat(run);
        for (int x = 0; x < 4; x++) run.threat[x, 0] = 2;

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        CollectionAssert.AreEqual(new[] { 0, 0, 0, 0, 0, 4, 4 }, Row(result.threatAfter, 0));
        Assert.AreEqual(2, result.merges.Count);
    }

    [Test]
    public void InitialDirectionMovesThreat_ChainTurnDoesNot()
    {
        KaitRun run = OpenRun(2); ClearThreat(run);
        run.threat[0, 0] = 2; run.threat[1, 0] = 2;
        Vector2Int start = run.katePos;
        run.enemies.Add(Enemy(1, start + Vector2Int.right, 1));
        run.enemies.Add(Enemy(2, start + Vector2Int.right + Vector2Int.up, 1));

        KaitTurnResult initial = run.TryTurn(KaitDirection.Right);
        KaitTurnResult chained = run.ContinueChain(KaitDirection.Up);

        Assert.AreEqual(1, initial.merges.Count);
        Assert.IsNull(chained.threatBefore);
        Assert.AreEqual(0, chained.threatMotions.Count);
        Assert.AreEqual(0, chained.merges.Count);
    }

    [Test]
    public void NonLethalCollision_PersistsHpAndStopsBeforeEnemy()
    {
        KaitRun run = OpenRun(3); Vector2Int start = run.katePos;
        run.enemies.Add(Enemy(1, start + Vector2Int.right * 2, 3));

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(1, run.enemies[0].hp);
        Assert.AreEqual(start + Vector2Int.right, run.katePos);
        Assert.AreEqual(2, result.damageDealt);
        Assert.IsTrue(result.turnComplete);
    }

    [Test]
    public void LethalCollision_EntersCellLosesOneMomentumAndWaitsForTurn()
    {
        KaitRun run = OpenRun(4); Vector2Int start = run.katePos;
        Vector2Int target = start + Vector2Int.right * 3;
        run.enemies.Add(Enemy(1, target, 2));

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(target, run.katePos);
        Assert.AreEqual(KaitEnemyLife.Dead, run.enemies[0].life);
        Assert.AreEqual(2, run.momentum);
        Assert.IsTrue(result.awaitingTurnChoice);
    }

    [Test]
    public void KillTurn_OffersForwardLeftRightButNeverReverse()
    {
        KaitRun run = OpenRun(5); Vector2Int start = run.katePos;
        run.enemies.Add(Enemy(1, start + Vector2Int.right, 1));

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        CollectionAssert.AreEquivalent(new[] { KaitDirection.Up, KaitDirection.Right, KaitDirection.Down }, result.availableDirections);
        CollectionAssert.DoesNotContain(result.availableDirections, KaitDirection.Left);
    }

    [Test]
    public void MergeAtThreatThreeFive_PreviewsBattleFourSix()
    {
        KaitRun run = OpenRun(6); ClearThreat(run);
        run.threat[0, 5] = 8; run.threat[1, 5] = 16; run.threat[2, 5] = 32;
        run.threat[3, 5] = 2; run.threat[4, 5] = 2;
        run.enemies.Add(Enemy(1, run.katePos + Vector2Int.left, 1));

        KaitTurnResult result = run.TryTurn(KaitDirection.Left);
        KaitMergeEvent merge = result.merges.Single(m => m.resultValue == 4);
        KaitSpawnRequest spawn = run.spawns.Single(s => s.sourceThreatCell == merge.threatCell);

        Assert.AreEqual(new Vector2Int(3, 5), merge.threatCell);
        Assert.AreEqual(new Vector2Int(4, 6), spawn.targetCell);
    }

    [Test]
    public void OccupiedSpawn_RemainsPendingAtExactCell()
    {
        KaitRun run = OpenRun(7); Vector2Int occupied = new Vector2Int(2, 2);
        run.enemies.Add(Enemy(1, occupied, 3));
        run.spawns.Add(new KaitSpawnRequest { tier = 1, sourceThreatCell = Vector2Int.one, targetCell = occupied, turnsUntilSpawn = 0, state = KaitSpawnState.Ready });

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.turnComplete);
        Assert.IsNotNull(run.SpawnAt(occupied));
        Assert.AreEqual(occupied, run.SpawnAt(occupied).targetCell);
    }

    [Test]
    public void ActiveEnemyCap_KeepsNewSpawnPending()
    {
        KaitRun run = OpenRun(8);
        int id = 1;
        for (int y = 1; y <= 2; y++) for (int x = 1; x <= 4; x++) run.enemies.Add(Enemy(id++, new Vector2Int(x, y), 1));
        Vector2Int target = new Vector2Int(7, 7);
        run.spawns.Add(new KaitSpawnRequest { tier = 1, sourceThreatCell = new Vector2Int(6, 6), targetCell = target, turnsUntilSpawn = 0, state = KaitSpawnState.Ready });

        run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(8, run.enemies.Count(e => e.life != KaitEnemyLife.Dead));
        Assert.IsNotNull(run.SpawnAt(target));
    }

    [Test]
    public void ThreatLock_ResetsToFourTwosWithoutEndingRun()
    {
        KaitRun run = OpenRun(9);
        for (int y = 0; y < run.ThreatSize; y++) for (int x = 0; x < run.ThreatSize; x++) run.threat[x, y] = ((x + y) % 2 == 0) ? 2 : 4;

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.turnComplete);
        Assert.AreEqual(1, run.threatLocks);
        Assert.AreEqual(4, CountThreat(run, 2));
        Assert.IsFalse(run.ended);
    }

    private static KaitRun OpenRun(int seed)
    {
        var run = new KaitRun(); run.Reset(seed);
        for (int y = 1; y < KaitRun.BattleSize - 1; y++) for (int x = 1; x < KaitRun.BattleSize - 1; x++) run.walls[x, y] = false;
        typeof(KaitRun).GetField("<katePos>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(run, new Vector2Int(4, 4));
        return run;
    }

    private static KaitEnemy Enemy(int id, Vector2Int pos, int hp) => new KaitEnemy
    { id = id, type = hp >= 3 ? KaitEnemyType.Guard : KaitEnemyType.Grunt, pos = pos, hp = hp, maxHp = hp, life = KaitEnemyLife.Preparing };
    private static void ClearThreat(KaitRun run) { for (int y = 0; y < run.ThreatSize; y++) for (int x = 0; x < run.ThreatSize; x++) run.threat[x, y] = 0; }
    private static int CountThreat(KaitRun run, int value) { int count = 0; foreach (int cell in run.threat) if (cell == value) count++; return count; }
    private static int[] Row(int[,] board, int y) { int[] row = new int[board.GetLength(0)]; for (int x = 0; x < row.Length; x++) row[x] = board[x, y]; return row; }
}
