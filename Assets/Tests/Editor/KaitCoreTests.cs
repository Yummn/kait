using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class KaitCoreTests
{
    [Test]
    public void V035Baseline_UsesSharedPillarsAndHighDurabilityTable()
    {
        KaitRun run = OpenRun(100);
        Assert.AreEqual(7, KaitRun.BattleSize);
        Assert.AreEqual(5, run.ThreatSize);
        Assert.AreEqual(3, CountThreat(run, 2));
        Assert.AreEqual(3, run.kateHp);
        Assert.IsTrue(run.walls[1, 2]);
        Assert.IsTrue(run.walls[5, 4]);
        Assert.IsTrue(run.IsThreatPillar(new Vector2Int(0, 1)));
        Assert.IsTrue(run.IsThreatPillar(new Vector2Int(4, 3)));
        Assert.AreEqual(new Vector2Int(1, 2), run.MapThreatToBattle(new Vector2Int(0, 1)));
        Assert.AreEqual(new Vector2Int(5, 4), run.MapThreatToBattle(new Vector2Int(4, 3)));
        Assert.AreEqual(2, CountThreatPillars(run));
        Assert.AreEqual(0, run.threat[0, 1]);
        Assert.AreEqual(0, run.threat[4, 3]);
        Assert.IsFalse(run.walls[1, 3]);
        Assert.IsFalse(run.walls[5, 3]);
        Assert.AreEqual(2, KaitRun.MaxHpFor(KaitEnemyType.Grunt));
        Assert.AreEqual(4, KaitRun.MaxHpFor(KaitEnemyType.Swordsman));
        Assert.AreEqual(2, KaitRun.MaxHpFor(KaitEnemyType.Archer));
        Assert.AreEqual(6, KaitRun.MaxHpFor(KaitEnemyType.Guard));
        Assert.AreEqual(8, KaitRun.MaxHpFor(KaitEnemyType.Elite));
    }

    [Test]
    public void T01_BasicMomentum_KillsAndEntersEnemyCell()
    {
        KaitRun run = OpenRun(1, new Vector2Int(1, 1));
        KaitEnemy enemy = Enemy(1, new Vector2Int(5, 1), 1);
        run.enemies.Add(enemy);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(KaitEnemyLife.Dead, enemy.life);
        Assert.AreEqual(new Vector2Int(5, 1), run.katePos);
        Assert.AreEqual(3, run.momentum);
        Assert.IsTrue(result.awaitingTurnChoice);
    }

    [Test]
    public void T02_KillTurn_AllowsAllFourDirectionsIncludingReverse()
    {
        KaitRun run = OpenRun(2, new Vector2Int(1, 3));
        run.enemies.Add(Enemy(1, new Vector2Int(3, 3), 1));

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        CollectionAssert.AreEquivalent(new[] { KaitDirection.Up, KaitDirection.Down, KaitDirection.Left, KaitDirection.Right }, result.availableDirections);
    }

    [Test]
    public void T03_ChainTurns_DoNotMoveThreatAgain()
    {
        KaitRun run = OpenRun(3, new Vector2Int(1, 3)); ClearThreat(run);
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
        KaitRun run = OpenRun(4, new Vector2Int(2, 2));
        KaitEnemy enemy = Enemy(1, new Vector2Int(3, 2), 2); run.enemies.Add(enemy);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.pushed);
        Assert.AreEqual(0, result.damageDealt);
        Assert.AreEqual(new Vector2Int(4, 2), enemy.pos);
        Assert.AreEqual(new Vector2Int(3, 2), run.katePos);
        Assert.IsTrue(result.turnComplete);
    }

    [Test]
    public void T05_WallCollision_DealsExtraDamageWithoutMovingEnemy()
    {
        KaitRun run = OpenRun(5, new Vector2Int(3, 4));
        KaitEnemy enemy = Enemy(1, new Vector2Int(4, 4), 2); run.enemies.Add(enemy);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.pushBlockedByWall);
        Assert.AreEqual(1, enemy.hp);
        Assert.AreEqual(new Vector2Int(4, 4), enemy.pos);
        Assert.AreEqual(new Vector2Int(3, 4), run.katePos);
    }

    [Test]
    public void T06_UnitCollision_DamagesBothAndDoesNotChainPush()
    {
        KaitRun run = OpenRun(6, new Vector2Int(2, 3));
        KaitEnemy a = Enemy(1, new Vector2Int(3, 3), 3), b = Enemy(2, new Vector2Int(4, 3), 2);
        run.enemies.Add(a); run.enemies.Add(b);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.pushBlockedByUnit);
        Assert.AreEqual(2, a.hp);
        Assert.AreEqual(1, b.hp);
        Assert.AreEqual(new Vector2Int(3, 3), a.pos);
        Assert.AreEqual(new Vector2Int(4, 3), b.pos);
        Assert.AreEqual(new Vector2Int(2, 3), run.katePos);
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
        Assert.AreEqual(2, victim.hp);
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

    [Test]
    public void V034_MomentumThree_KillsHpThreeWithoutConversion()
    {
        KaitRun run = OpenRun(14, new Vector2Int(1, 1));
        KaitEnemy swordsman = Enemy(1, new Vector2Int(5, 1), 3, KaitEnemyType.Swordsman); run.enemies.Add(swordsman);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(3, result.damageDealt);
        Assert.AreEqual(3, result.chainPower);
        Assert.AreEqual(KaitEnemyLife.Dead, swordsman.life);
        Assert.IsTrue(result.awaitingTurnChoice);
    }

    [Test]
    public void V034_MomentumTwo_LeavesHpTwoAndPushes()
    {
        KaitRun run = OpenRun(15, new Vector2Int(1, 1));
        KaitEnemy swordsman = Enemy(1, new Vector2Int(4, 1), 4, KaitEnemyType.Swordsman); run.enemies.Add(swordsman);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(2, result.damageDealt);
        Assert.AreEqual(2, swordsman.hp);
        Assert.AreEqual(new Vector2Int(5, 1), swordsman.pos);
        Assert.AreEqual(new Vector2Int(4, 1), run.katePos);
        Assert.IsTrue(result.turnComplete);
    }

    [Test]
    public void V031_ReverseDirection_IsValidAndDoesNotMoveThreatAgain()
    {
        KaitRun run = OpenRun(16, new Vector2Int(1, 3)); ClearThreat(run);
        run.threat[0, 0] = 2; run.threat[1, 0] = 4;
        run.enemies.Add(Enemy(1, new Vector2Int(3, 3), 1));

        KaitTurnResult first = run.TryTurn(KaitDirection.Right);
        int[,] afterInitialInput = CopyThreat(run);
        KaitTurnResult reverse = run.ContinueChain(KaitDirection.Left);

        Assert.IsTrue(first.awaitingTurnChoice);
        Assert.IsTrue(reverse.valid);
        Assert.IsNull(reverse.threatBefore);
        Assert.AreEqual(0, reverse.threatMotions.Count);
        for (int y = 0; y < run.ThreatSize; y++)
            for (int x = 0; x < run.ThreatSize; x++)
                if (afterInitialInput[x, y] != 0) Assert.AreEqual(afterInitialInput[x, y], run.threat[x, y]);
    }

    [Test]
    public void V031_ChoosingAdjacentWallAfterKill_ActivelyBrakes()
    {
        KaitRun run = OpenRun(17, new Vector2Int(4, 2));
        run.enemies.Add(Enemy(1, new Vector2Int(2, 2), 1));

        KaitTurnResult kill = run.TryTurn(KaitDirection.Left);
        KaitTurnResult brake = run.ContinueChain(KaitDirection.Left);

        Assert.IsTrue(kill.awaitingTurnChoice);
        Assert.IsTrue(brake.activeBrake);
        Assert.IsTrue(brake.turnComplete);
        Assert.AreEqual(new Vector2Int(2, 2), run.katePos);
        Assert.AreEqual(1, run.activeWallStops);
    }

    [Test]
    public void V035_ThreatPillar_SplitsColumnAndPreventsCrossPillarMerge()
    {
        KaitRun run = OpenRun(18, new Vector2Int(3, 3)); ClearThreat(run);
        run.threat[0, 0] = 2; run.threat[0, 2] = 2;

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Up);

        Assert.AreEqual(2, run.threat[0, 0]);
        Assert.AreEqual(0, run.threat[0, 1]);
        Assert.AreEqual(2, run.threat[0, 4]);
        Assert.AreEqual(0, result.merges.Count);
        Assert.AreEqual(0, run.spawns.Count);
        Assert.AreEqual(0, run.wallSuppressedSpawns);
    }

    [Test]
    public void PlayerKill_ClearsCommittedIntentImmediately()
    {
        KaitRun run = OpenRun(19, new Vector2Int(1, 3));
        KaitEnemy archer = Enemy(1, new Vector2Int(3, 3), 1, KaitEnemyType.Archer, KaitEnemyLife.Active);
        archer.intent = LineIntent(archer.pos, Vector2Int.right, new Vector2Int(4, 3), new Vector2Int(5, 3));
        run.enemies.Add(archer);

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.AreEqual(KaitEnemyLife.Dead, archer.life);
        Assert.AreEqual(KaitIntentType.None, archer.intent.type);
        CollectionAssert.Contains(result.playerKilledEnemyIds, archer.id);
    }

    [Test]
    public void V033_KateBlockedButThreatMoves_WaitsAndAdvancesGlobalTurn()
    {
        KaitRun run = OpenRun(21, new Vector2Int(2, 2)); ClearThreat(run);
        run.threat[4, 4] = 2;

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Left);

        Assert.IsTrue(result.valid);
        Assert.IsTrue(result.kaitWaited);
        Assert.IsTrue(result.threatChanged);
        Assert.IsTrue(result.turnComplete);
        Assert.AreEqual(new Vector2Int(2, 2), run.katePos);
        Assert.AreEqual(1, run.turn);
        Assert.AreEqual(2, CountOccupiedThreat(run));
    }

    [Test]
    public void V033_KateMovesWhenThreatCannot_NewTwoStillAppears()
    {
        KaitRun run = OpenRun(22, new Vector2Int(2, 2)); ClearThreat(run);
        run.threat[4, 0] = 2;

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);

        Assert.IsTrue(result.valid);
        Assert.IsFalse(result.kaitWaited);
        Assert.IsFalse(result.threatChanged);
        Assert.IsTrue(result.turnComplete);
        Assert.AreEqual(1, run.turn);
        Assert.AreEqual(2, CountOccupiedThreat(run));
        Assert.AreEqual(2, run.threat[4, 0]);
    }

    [Test]
    public void V033_BothBoardsCannotRespond_InputIsFreeAndInvalid()
    {
        KaitRun run = OpenRun(23, new Vector2Int(2, 2)); ClearThreat(run);
        run.threat[0, 0] = 2;

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Left);

        Assert.IsFalse(result.valid);
        Assert.AreEqual(0, run.turn);
        Assert.AreEqual(1, CountOccupiedThreat(run));
        Assert.AreEqual(0, run.spawns.Count);
    }

    [Test]
    public void V033_ChainInputs_DoNotAdvanceThreatRiftEnemyOrNewTwo()
    {
        KaitRun run = OpenRun(24, new Vector2Int(1, 3)); ClearThreat(run);
        run.threat[0, 0] = 2; run.threat[1, 0] = 4;
        run.enemies.Add(Enemy(1, new Vector2Int(3, 4), 1));
        run.enemies.Add(Enemy(2, new Vector2Int(3, 3), 1));
        run.spawns.Add(new KaitSpawnRequest { tier = 1, targetCell = new Vector2Int(4, 4), turnsUntilSpawn = 1, state = KaitSpawnState.Preview });

        KaitTurnResult initial = run.TryGlobalInput(KaitDirection.Right);
        int[,] afterGlobal = CopyThreat(run);
        KaitTurnResult chain = run.ContinueChain(KaitDirection.Up);

        Assert.IsTrue(initial.awaitingTurnChoice);
        Assert.IsTrue(chain.awaitingTurnChoice);
        Assert.AreEqual(0, run.turn);
        Assert.AreEqual(1, run.spawns.Single().turnsUntilSpawn);
        Assert.AreEqual(0, chain.threatMotions.Count);
        CollectionAssert.AreEqual(afterGlobal, run.threat);
        Assert.AreEqual(1, chain.chainStepCount);
        Assert.AreEqual(2, chain.chainKillCount);
    }

    [Test]
    public void V035_ThreatPillar_AllowsMergeInsideOneSegmentAndMapsSpawnExactly()
    {
        KaitRun run = OpenRun(25, new Vector2Int(3, 3)); ClearThreat(run);
        run.threat[0, 2] = 2; run.threat[0, 3] = 2;

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Up);
        KaitMergeEvent merge = result.merges.Single();
        KaitSpawnRequest warning = run.spawns.Single();

        Assert.AreEqual(4, run.threat[0, 4]);
        Assert.AreEqual(new Vector2Int(0, 4), merge.threatCell);
        Assert.AreEqual(new Vector2Int(1, 5), warning.targetCell);
        Assert.AreEqual(merge.threatCell, warning.sourceThreatCell);
        Assert.AreEqual(1, run.mergeHeatmap[0, 4]);
        Assert.AreEqual(0, run.wallSuppressedSpawns);
    }

    [Test]
    public void V035_NewTwoNeverUsesPillarCell()
    {
        KaitRun run = OpenRun(251, new Vector2Int(3, 3)); ClearThreat(run);
        for (int y = 0; y < run.ThreatSize; y++)
            for (int x = 0; x < run.ThreatSize; x++)
                if (!run.IsThreatPillar(new Vector2Int(x, y))) run.threat[x, y] = 2;
        Vector2Int onlyEmpty = new Vector2Int(2, 2);
        run.threat[onlyEmpty.x, onlyEmpty.y] = 0;

        MethodInfo spawn = typeof(KaitRun).GetMethod("SpawnThreatTwo", BindingFlags.Instance | BindingFlags.NonPublic);
        Vector2Int spawned = (Vector2Int)spawn.Invoke(run, null);

        Assert.AreEqual(onlyEmpty, spawned);
        Assert.AreEqual(2, run.threat[onlyEmpty.x, onlyEmpty.y]);
        Assert.AreEqual(0, run.threat[0, 1]);
        Assert.AreEqual(0, run.threat[4, 3]);
    }

    [Test]
    public void V034_FirstContactLocksChainPowerAtRunUpDistance()
    {
        KaitRun run = OpenRun(26, new Vector2Int(1, 1));
        KaitEnemy enemy = Enemy(1, new Vector2Int(5, 1), 3); run.enemies.Add(enemy);

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);

        Assert.IsTrue(result.awaitingTurnChoice);
        Assert.IsTrue(result.powerLocked);
        Assert.AreEqual(3, result.chainPower);
        Assert.AreEqual(3, result.damageDealt);
        Assert.AreEqual(1, run.lockedPowerCounts[3]);
    }

    [Test]
    public void V034_ChainMovementDoesNotIncreaseLockedDamage()
    {
        KaitRun run = OpenRun(27, new Vector2Int(1, 3));
        run.enemies.Add(Enemy(1, new Vector2Int(5, 3), 3));

        KaitTurnResult first = run.TryGlobalInput(KaitDirection.Right);
        run.enemies.Add(Enemy(2, new Vector2Int(2, 3), 3));
        KaitTurnResult reverse = run.ContinueChain(KaitDirection.Left);

        Assert.AreEqual(3, first.chainPower);
        Assert.AreEqual(3, reverse.chainPower);
        Assert.AreEqual(3, reverse.damageDealt);
        Assert.AreEqual(2, reverse.chainMoves);
        Assert.AreEqual(3, run.momentum);
        Assert.IsTrue(reverse.awaitingTurnChoice);
    }

    [Test]
    public void V034_StrongEnemyStopsChainAtFixedPowerAndIsPushed()
    {
        KaitRun run = OpenRun(28, new Vector2Int(1, 3));
        run.enemies.Add(Enemy(1, new Vector2Int(5, 3), 3));
        run.TryGlobalInput(KaitDirection.Right);
        KaitEnemy strong = Enemy(2, new Vector2Int(2, 3), 4, KaitEnemyType.Swordsman); run.enemies.Add(strong);

        KaitTurnResult result = run.ContinueChain(KaitDirection.Left);

        Assert.AreEqual(3, result.chainPower);
        Assert.AreEqual(3, result.damageDealt);
        Assert.AreEqual(1, strong.hp);
        Assert.AreEqual(new Vector2Int(1, 3), strong.pos);
        Assert.IsTrue(result.pushed);
        Assert.IsTrue(result.turnComplete);
        Assert.IsTrue(result.chainEndedByStrongEnemy);
        Assert.AreEqual(1, run.chainEndByStrongEnemy);
    }

    [Test]
    public void V034_DamagedStrongEnemyCanJoinFixedPowerChain()
    {
        KaitRun run = OpenRun(29, new Vector2Int(1, 3));
        run.enemies.Add(Enemy(1, new Vector2Int(5, 3), 3));
        run.TryGlobalInput(KaitDirection.Right);
        KaitEnemy weakened = Enemy(2, new Vector2Int(2, 3), 3, KaitEnemyType.Guard); run.enemies.Add(weakened);

        KaitTurnResult result = run.ContinueChain(KaitDirection.Left);

        Assert.AreEqual(KaitEnemyLife.Dead, weakened.life);
        Assert.AreEqual(3, result.damageDealt);
        Assert.IsTrue(result.awaitingTurnChoice);
    }

    [Test]
    public void V034_EmptyMapStopGraphIsReachableWithoutEnemyKeys()
    {
        KaitRun run = OpenRun(30);

        Assert.IsTrue(run.emptyMapReachable);
        Assert.Greater(run.emptyMapMaxInputs, 0);
        Assert.LessOrEqual(run.emptyMapMaxInputs, 6);
    }

    [Test]
    public void V034_TenSeedSmokeRuns_PreserveLockedPowerInvariant()
    {
        KaitDirection[] directions = { KaitDirection.Up, KaitDirection.Right, KaitDirection.Down, KaitDirection.Left };
        for (int seed = 100; seed < 110; seed++)
        {
            KaitRun run = OpenRun(seed);
            for (int step = 0; step < 160 && !run.ended; step++)
            {
                KaitTurnResult result = null;
                if (run.chainActive)
                {
                    int locked = run.chainPower;
                    result = run.ContinueChain(directions[(step + seed) % directions.Length]);
                    if (result.powerLocked) Assert.AreEqual(locked, result.chainPower, $"seed {seed}, step {step}");
                }
                else
                {
                    for (int offset = 0; offset < directions.Length; offset++)
                    {
                        result = run.TryGlobalInput(directions[(step + seed + offset) % directions.Length]);
                        if (result.valid) break;
                    }
                }
                Assert.IsNotNull(result);
                if (result.damagedEnemyId >= 0) Assert.AreEqual(result.chainPower, result.damageDealt, $"seed {seed}, step {step}");
            }
            Assert.Greater(run.turn, 0, $"seed {seed}");
            Assert.IsTrue(run.emptyMapReachable, $"seed {seed}");
        }
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
    private static int[,] CopyThreat(KaitRun run) { var copy = new int[run.ThreatSize, run.ThreatSize]; System.Array.Copy(run.threat, copy, run.threat.Length); return copy; }
    private static int CountThreat(KaitRun run, int value) { int count = 0; foreach (int cell in run.threat) if (cell == value) count++; return count; }
    private static int CountOccupiedThreat(KaitRun run) { int count = 0; foreach (int cell in run.threat) if (cell != 0) count++; return count; }
    private static int CountThreatPillars(KaitRun run)
    {
        int count = 0;
        for (int y = 0; y < run.ThreatSize; y++) for (int x = 0; x < run.ThreatSize; x++) if (run.IsThreatPillar(new Vector2Int(x, y))) count++;
        return count;
    }
}
