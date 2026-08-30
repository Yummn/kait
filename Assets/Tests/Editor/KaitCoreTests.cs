using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class KaitCoreTests
{
    [Test]
    public void V03Baseline_UsesSevenTotalFiveThreatAndThreeTwos()
    {
        KaitRun run = OpenRun(100);
        Assert.AreEqual(7, KaitRun.BattleSize);
        Assert.AreEqual(5, run.ThreatSize);
        Assert.AreEqual(3, CountThreat(run, 2));
        Assert.AreEqual(3, run.kateHp);
    }

    [Test]
    public void T01_BasicMomentum_KillsAndEntersEnemyCell()
    {
        KaitRun run = OpenRun(1, new Vector2Int(1, 3));
        KaitEnemy enemy = Enemy(1, new Vector2Int(4, 3), 1);
        run.enemies.Add(enemy);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(KaitEnemyLife.Dead, enemy.life);
        Assert.AreEqual(new Vector2Int(4, 3), run.katePos);
        Assert.AreEqual(3, run.momentum);
        Assert.IsTrue(result.awaitingTurnChoice);
    }

    [Test]
    public void T02_KillTurn_AllowsForwardLeftRightButNotReverse()
    {
        KaitRun run = OpenRun(2, new Vector2Int(2, 3));
        run.enemies.Add(Enemy(1, new Vector2Int(3, 3), 1));

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        CollectionAssert.AreEquivalent(new[] { KaitDirection.Up, KaitDirection.Right, KaitDirection.Down }, result.availableDirections);
        CollectionAssert.DoesNotContain(result.availableDirections, KaitDirection.Left);
    }

    [Test]
    public void T03_ChainTurns_DoNotMoveThreatAgain()
    {
        KaitRun run = OpenRun(3, new Vector2Int(2, 3)); ClearThreat(run);
        run.threat[0, 0] = 2; run.threat[1, 0] = 2;
        run.enemies.Add(Enemy(1, new Vector2Int(3, 3), 1));
        run.enemies.Add(Enemy(2, new Vector2Int(3, 4), 1));

        KaitTurnResult initial = run.TryTurn(KaitDirection.Right);
        KaitTurnResult chained = run.ContinueChain(KaitDirection.Up);

        Assert.AreEqual(1, initial.merges.Count);
        Assert.IsNull(chained.threatBefore);
        Assert.AreEqual(0, chained.threatMotions.Count);
    }

    [Test]
    public void T04_NonLethalHit_PushesOneCellAndKateEntersOrigin()
    {
        KaitRun run = OpenRun(4, new Vector2Int(1, 3));
        KaitEnemy enemy = Enemy(1, new Vector2Int(2, 3), 2); run.enemies.Add(enemy);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.pushed);
        Assert.AreEqual(new Vector2Int(3, 3), enemy.pos);
        Assert.AreEqual(new Vector2Int(2, 3), run.katePos);
        Assert.IsTrue(result.turnComplete);
    }

    [Test]
    public void T05_WallCollision_DealsExtraDamageWithoutMovingEnemy()
    {
        KaitRun run = OpenRun(5, new Vector2Int(4, 3));
        KaitEnemy enemy = Enemy(1, new Vector2Int(5, 3), 3); run.enemies.Add(enemy);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.pushBlockedByWall);
        Assert.AreEqual(1, enemy.hp);
        Assert.AreEqual(new Vector2Int(5, 3), enemy.pos);
        Assert.AreEqual(new Vector2Int(5, 3), run.katePos);
    }

    [Test]
    public void T06_UnitCollision_DamagesBothAndDoesNotChainPush()
    {
        KaitRun run = OpenRun(6, new Vector2Int(2, 3));
        KaitEnemy a = Enemy(1, new Vector2Int(3, 3), 3), b = Enemy(2, new Vector2Int(4, 3), 2);
        run.enemies.Add(a); run.enemies.Add(b);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.pushBlockedByUnit);
        Assert.AreEqual(1, a.hp);
        Assert.AreEqual(1, b.hp);
        Assert.AreEqual(new Vector2Int(3, 3), a.pos);
        Assert.AreEqual(new Vector2Int(4, 3), b.pos);
    }

    [Test]
    public void T07_PushedEnemy_IsHitByPreviouslyCommittedFriendlyFire()
    {
        KaitRun run = OpenRun(7, new Vector2Int(2, 3));
        KaitEnemy victim = Enemy(1, new Vector2Int(3, 3), 3);
        KaitEnemy archer = Enemy(2, new Vector2Int(1, 1), 1, KaitEnemyType.Archer, KaitEnemyLife.Active);
        archer.intent = LineIntent(archer.pos, Vector2Int.right, new Vector2Int(4, 3));
        run.enemies.Add(victim); run.enemies.Add(archer);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(new Vector2Int(4, 3), victim.pos);
        Assert.AreEqual(1, victim.hp);
        Assert.AreEqual(1, result.friendlyFireDamage);
    }

    [Test]
    public void T08_CommittedIntent_DoesNotReaimAfterKateMoves()
    {
        KaitRun run = OpenRun(8, new Vector2Int(3, 3));
        KaitEnemy archer = Enemy(1, new Vector2Int(1, 3), 1, KaitEnemyType.Archer, KaitEnemyLife.Active);
        archer.intent = LineIntent(archer.pos, Vector2Int.right, new Vector2Int(2, 3), new Vector2Int(3, 3), new Vector2Int(4, 3));
        run.enemies.Add(archer);

        KaitTurnResult result = run.TryTurn(KaitDirection.Up);

        Assert.AreEqual(3, run.kateHp);
        CollectionAssert.Contains(result.enemyActions.Single().affectedCells, new Vector2Int(3, 3));
        CollectionAssert.DoesNotContain(result.enemyActions.Single().affectedCells, run.katePos);
    }

    [Test]
    public void T09_ThreatMerge_CreatesRiftAtExactMappedCell()
    {
        KaitRun run = OpenRun(9, new Vector2Int(3, 3)); ClearThreat(run);
        run.threat[0, 2] = 8; run.threat[1, 2] = 16; run.threat[2, 2] = 2; run.threat[3, 2] = 2;
        run.enemies.Add(Enemy(1, new Vector2Int(2, 3), 1));

        KaitTurnResult result = run.TryTurn(KaitDirection.Left);
        KaitMergeEvent merge = result.merges.Single(m => m.resultValue == 4);
        KaitSpawnRequest rift = run.spawns.Single(s => s.sourceThreatCell == merge.threatCell);

        Assert.AreEqual(new Vector2Int(2, 2), merge.threatCell);
        Assert.AreEqual(new Vector2Int(3, 3), rift.targetCell);
    }

    [Test]
    public void T10_BlockedRift_DamagesOccupantAndConsumesSpawn()
    {
        KaitRun run = OpenRun(10, new Vector2Int(3, 3));
        KaitEnemy occupant = Enemy(1, new Vector2Int(2, 2), 2); run.enemies.Add(occupant);
        run.spawns.Add(new KaitSpawnRequest { tier = 1, sourceThreatCell = Vector2Int.one, targetCell = occupant.pos, turnsUntilSpawn = 0, state = KaitSpawnState.Ready });

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(1, occupant.hp);
        Assert.AreEqual(1, result.riftBlockDamage);
        Assert.IsNull(run.SpawnAt(occupant.pos));
        Assert.AreEqual(1, run.enemies.FindAll(e => e.life != KaitEnemyLife.Dead && e.pos == occupant.pos).Count);
    }

    [Test]
    public void T11_NewEnemy_IsPreparingForOneFullPlayerAction()
    {
        KaitRun run = OpenRun(11, new Vector2Int(3, 3));
        Vector2Int target = new Vector2Int(1, 1);
        run.spawns.Add(new KaitSpawnRequest { tier = 1, sourceThreatCell = Vector2Int.zero, targetCell = target, turnsUntilSpawn = 0, state = KaitSpawnState.Ready });

        run.TryTurn(KaitDirection.Right);
        KaitEnemy spawned = run.EnemyAt(target);
        Assert.AreEqual(KaitEnemyLife.Preparing, spawned.life);
        Assert.AreEqual(KaitIntentType.None, spawned.intent.type);

        run.TryTurn(KaitDirection.Left);
        Assert.AreEqual(KaitEnemyLife.Active, spawned.life);
    }

    [Test]
    public void T12_Merging64_WinsAfterCurrentTurnFinishes()
    {
        KaitRun run = OpenRun(12, new Vector2Int(3, 3)); ClearThreat(run);
        run.threat[0, 0] = 32; run.threat[1, 0] = 32;

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.turnComplete);
        Assert.IsTrue(run.ended);
        Assert.IsTrue(run.won);
        Assert.AreEqual("Victory 64", run.endReason);
    }

    [Test]
    public void RiftCreatedThisTurn_WarnsBeforeSpawningOnNextValidAction()
    {
        KaitRun run = OpenRun(13, new Vector2Int(3, 3)); ClearThreat(run);
        run.threat[0, 0] = 2; run.threat[1, 0] = 2;

        KaitTurnResult first = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(first.turnComplete);
        Assert.AreEqual(1, run.spawns.Count);
        Assert.AreEqual(0, first.spawnedEnemyCells.Count);
        KaitSpawnRequest warning = run.spawns.Single();
        Assert.AreEqual(KaitSpawnState.Preview, warning.state);
        Assert.AreEqual(1, warning.turnsUntilSpawn);
        Assert.IsNull(run.EnemyAt(warning.targetCell));

        Vector2Int target = warning.targetCell;
        KaitTurnResult second = run.TryTurn(KaitDirection.Left);

        Assert.IsTrue(second.turnComplete);
        Assert.AreEqual(0, run.spawns.Count);
        Assert.AreEqual(KaitEnemyLife.Preparing, run.EnemyAt(target).life);
    }

    private static KaitRun OpenRun(int seed, Vector2Int? kate = null)
    {
        var run = new KaitRun(); run.Reset(seed);
        if (kate.HasValue) typeof(KaitRun).GetField("<katePos>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(run, kate.Value);
        return run;
    }
    private static KaitEnemy Enemy(int id, Vector2Int pos, int hp, KaitEnemyType type = KaitEnemyType.Grunt, KaitEnemyLife life = KaitEnemyLife.Preparing)
        => new KaitEnemy { id = id, pos = pos, hp = hp, maxHp = hp, type = type, life = life };
    private static KaitIntent LineIntent(Vector2Int origin, Vector2Int direction, params Vector2Int[] cells)
    {
        var intent = new KaitIntent { type = KaitIntentType.LineShot, origin = origin, direction = direction, damage = 1, target = cells[cells.Length - 1] };
        intent.affectedCells.AddRange(cells); return intent;
    }
    private static void ClearThreat(KaitRun run) { for (int y = 0; y < run.ThreatSize; y++) for (int x = 0; x < run.ThreatSize; x++) run.threat[x, y] = 0; }
    private static int CountThreat(KaitRun run, int value) { int count = 0; foreach (int cell in run.threat) if (cell == value) count++; return count; }
}
