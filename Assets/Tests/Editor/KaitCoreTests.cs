using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class KaitCoreTests
{
    [Test]
    public void Reset_CreatesTwoThreatTilesAndValidKatePosition()
    {
        var run = new KaitRun();
        run.Reset(2048);

        int twos = 0;
        for (int y = 0; y < KaitRun.ThreatSize; y++)
            for (int x = 0; x < KaitRun.ThreatSize; x++)
                if (run.threat[x, y] == 2) twos++;

        Assert.AreEqual(2, twos);
        Assert.IsFalse(run.walls[run.katePos.x, run.katePos.y]);
    }

    [Test]
    public void SharedDirection_MergesThreatAndQueuesExactlyOneSpawn()
    {
        var run = new KaitRun();
        run.Reset(88);
        for (int y = 0; y < KaitRun.ThreatSize; y++)
            for (int x = 0; x < KaitRun.ThreatSize; x++)
                run.threat[x, y] = 0;

        KaitDirection direction = FindValidDirection(run);
        if (direction == KaitDirection.Left || direction == KaitDirection.Right)
        {
            run.threat[0, 0] = 2;
            run.threat[1, 0] = 2;
        }
        else
        {
            run.threat[0, 0] = 2;
            run.threat[0, 1] = 2;
        }

        KaitTurnResult result = run.TryTurn(direction);

        Assert.IsTrue(result.valid);
        Assert.AreEqual(1, result.merges.Count);
        Assert.AreEqual(4, result.merges[0].resultValue);
        Assert.AreEqual(1, run.spawns.Count);
        Assert.AreEqual(1, run.spawns[0].tier);
    }

    [Test]
    public void FourEqualTiles_ProducesTwoMerges_NotAChainMerge()
    {
        var run = new KaitRun();
        run.Reset(117);
        for (int y = 0; y < KaitRun.ThreatSize; y++)
            for (int x = 0; x < KaitRun.ThreatSize; x++)
                run.threat[x, y] = 0;

        KaitDirection direction = FindValidDirection(run);
        if (direction == KaitDirection.Left || direction == KaitDirection.Right)
            for (int x = 0; x < 4; x++) run.threat[x, 0] = 2;
        else
            for (int y = 0; y < 4; y++) run.threat[0, y] = 2;

        KaitTurnResult result = run.TryTurn(direction);

        Assert.AreEqual(2, result.merges.Count);
        Assert.IsTrue(result.merges.All(m => m.resultValue == 4));
        Assert.AreEqual(2, run.spawns.Count);
    }

    [Test]
    public void StrongEnemy_BlocksShortChargeInsteadOfBeingCrossed()
    {
        var run = new KaitRun();
        run.Reset(919);
        KaitDirection direction = FindValidDirection(run);
        Vector2Int enemyCell = run.katePos + KaitRun.Delta(direction);
        run.enemies.Add(new KaitEnemy
        {
            id = 99,
            type = KaitEnemyType.Guard,
            pos = enemyCell,
            threshold = 4,
            life = KaitEnemyLife.Preparing
        });
        Vector2Int start = run.katePos;

        KaitTurnResult result = run.TryTurn(direction);

        Assert.IsTrue(result.valid);
        Assert.AreEqual(start, run.katePos);
        Assert.AreEqual(enemyCell, result.blockedEnemyCell);
        Assert.AreNotEqual(KaitEnemyLife.Dead, run.enemies[0].life);
    }

    [Test]
    public void CrossingSpawnWarning_DoesNotRemoveTheRift()
    {
        var run = new KaitRun();
        run.Reset(1204);
        KaitDirection direction = FindValidDirection(run);
        Vector2Int riftCell = run.katePos + KaitRun.Delta(direction);
        run.spawns.Add(new KaitSpawnRequest
        {
            tier = 1,
            sourceThreatCell = Vector2Int.zero,
            targetCell = riftCell,
            turnsUntilSpawn = 2
        });

        KaitTurnResult result = run.TryTurn(direction);

        Assert.IsTrue(result.valid);
        Assert.Contains(riftCell, result.katePath);
        Assert.IsNotNull(run.SpawnAt(riftCell));
        Assert.AreEqual(1, run.SpawnAt(riftCell).turnsUntilSpawn);
    }

    private static KaitDirection FindValidDirection(KaitRun run)
    {
        foreach (KaitDirection direction in new[] { KaitDirection.Up, KaitDirection.Down, KaitDirection.Left, KaitDirection.Right })
        {
            Vector2Int p = run.katePos + KaitRun.Delta(direction);
            if (!run.walls[p.x, p.y]) return direction;
        }
        Assert.Fail("Kate has no valid initial direction.");
        return KaitDirection.Up;
    }
}
