using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class KaitCoreTests
{
    [Test]
    public void Settings_CanStartThreatBoardWithoutPillars()
    {
        var run = new KaitRun(new KaitBalanceConfig { enableThreatPillars = false });
        run.Reset(7);

        Assert.AreEqual(0, CountThreatPillars(run));
    }

    [Test]
    public void V036Baseline_UsesIndependentBattleAndThreatPillars()
    {
        KaitRun run = OpenRun(100);
        Assert.AreEqual(7, KaitRun.BattleSize);
        Assert.AreEqual(5, run.ThreatSize);
        Assert.AreEqual(3, CountThreat(run, 2));
        Assert.AreEqual(3, run.kateHp);
        Assert.IsTrue(run.walls[1, 2]);
        Assert.IsTrue(run.walls[5, 4]);
        Assert.IsTrue(run.IsThreatPillar(new Vector2Int(1, 4)));
        Assert.IsTrue(run.IsThreatPillar(new Vector2Int(3, 0)));
        Assert.IsFalse(run.walls[2, 5]);
        Assert.IsFalse(run.walls[4, 1]);
        Assert.AreEqual(2, CountThreatPillars(run));
        Assert.AreEqual(0, run.threat[1, 4]);
        Assert.AreEqual(0, run.threat[3, 0]);
        Assert.IsFalse(run.walls[2, 1]);
        Assert.IsFalse(run.walls[5, 3]);
        Assert.AreEqual(2, KaitRun.MaxHpFor(KaitEnemyType.Grunt));
        Assert.AreEqual(3, KaitRun.MaxHpFor(KaitEnemyType.Swordsman));
        Assert.AreEqual(2, KaitRun.MaxHpFor(KaitEnemyType.Archer));
        Assert.AreEqual(4, KaitRun.MaxHpFor(KaitEnemyType.Guard));
        Assert.AreEqual(2, KaitRun.MaxHpFor(KaitEnemyType.Warlock));
    }

    [Test]
    public void UnitPointTiers_SpawnTheSpecifiedEnemyTypesAndHp()
    {
        KaitRun run = OpenRun(102, new Vector2Int(3, 5));
        KaitEnemyType[] expected =
        {
            KaitEnemyType.Grunt, KaitEnemyType.Swordsman, KaitEnemyType.Archer,
            KaitEnemyType.Guard, KaitEnemyType.Warlock
        };
        for (int tier = 1; tier <= expected.Length; tier++)
            run.spawns.Add(new KaitSpawnRequest
            {
                tier = tier,
                sourceThreatCell = new Vector2Int(tier - 1, 0),
                targetCell = new Vector2Int(tier, 1),
                turnsUntilSpawn = 0,
                state = KaitSpawnState.Ready
            });

        var result = new KaitTurnResult();
        typeof(KaitRun).GetMethod("ResolveSpawnRequests", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, new object[] { result });

        for (int tier = 1; tier <= expected.Length; tier++)
        {
            KaitEnemy enemy = run.EnemyAt(new Vector2Int(tier, 1));
            Assert.IsNotNull(enemy);
            Assert.AreEqual(expected[tier - 1], enemy.type);
            Assert.AreEqual(KaitRun.MaxHpFor(expected[tier - 1]), enemy.hp);
        }
    }

    [Test]
    public void Warlock_AimsThenHitsTheFiveCellCrossOnTheFollowingPhase()
    {
        KaitRun run = OpenRun(103, new Vector2Int(3, 3));
        KaitEnemy warlock = Enemy(1, new Vector2Int(1, 1), 2, KaitEnemyType.Warlock, KaitEnemyLife.Active);
        KaitEnemy upperVictim = Enemy(2, new Vector2Int(3, 4), 2);
        KaitEnemy rightVictim = Enemy(3, new Vector2Int(4, 3), 2);
        run.enemies.Add(warlock); run.enemies.Add(upperVictim); run.enemies.Add(rightVictim);

        KaitTurnResult aim = ResolveEnemyPhase(run);
        Assert.AreEqual(KaitRangedState.Aim, warlock.rangedState);
        Assert.AreEqual(KaitIntentType.CrossBlast, warlock.intent.type);
        Assert.AreEqual(new Vector2Int(3, 3), warlock.intent.target);
        Assert.AreEqual(5, warlock.intent.affectedCells.Count);
        Assert.AreEqual(0, aim.enemyActions.Count);
        LockEnemyIntents(run);

        KaitTurnResult result = ResolveEnemyPhase(run);
        Assert.AreEqual(2, run.kateHp);
        Assert.AreEqual(1, result.playerDamage);
        Assert.AreEqual(1, upperVictim.hp);
        Assert.AreEqual(1, rightVictim.hp);
        Assert.AreEqual(2, result.friendlyFireDamage);
        Assert.AreEqual(KaitRangedState.Ready, warlock.rangedState);

        LockEnemyIntents(run);
        KaitTurnResult nextAim = ResolveEnemyPhase(run);
        Assert.AreEqual(0, nextAim.enemyActions.Count);
        Assert.AreEqual(2, run.kateHp);
        Assert.AreEqual(KaitRangedState.Aim, warlock.rangedState);
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
        Assert.AreEqual(0, result.playerDamage);
    }

    [Test]
    public void T05B_NormalSlideToBoundary_ReportsWallStopForAnimation()
    {
        KaitRun run = OpenRun(51, new Vector2Int(3, 3));

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);

        Assert.IsTrue(result.turnComplete);
        Assert.IsTrue(result.stoppedByWall);
        Assert.IsFalse(result.chainEndedByWall);
        Assert.AreEqual(KaitDirection.Right, result.kaitDirection);
        Assert.Greater(result.katePath.Count, 0);
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
        Assert.AreEqual(0, result.playerDamage);
    }

    [Test]
    public void T07_PushedEnemy_IsHitByPreviouslyCommittedFriendlyFire()
    {
        KaitRun run = OpenRun(7, new Vector2Int(2, 3));
        KaitEnemy victim = Enemy(1, new Vector2Int(3, 3), 3);
        KaitEnemy archer = Enemy(2, new Vector2Int(4, 1), 1, KaitEnemyType.Archer, KaitEnemyLife.Active);
        archer.rangedState = KaitRangedState.Aim;
        archer.intent = LineIntent(archer.pos, Vector2Int.up, new Vector2Int(4, 2), new Vector2Int(4, 3));
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
        archer.rangedState = KaitRangedState.Aim;
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
        Assert.AreEqual(0, result.playerDamage);
        Assert.IsNull(run.SpawnAt(occupant.pos));
        Assert.AreEqual(1, run.enemies.FindAll(e => e.life != KaitEnemyLife.Dead && e.pos == occupant.pos).Count);
    }

    [Test]
    public void T10B_RiftOnKate_RecordsPlayerDamageSeparately()
    {
        KaitRun run = OpenRun(101, new Vector2Int(3, 3));
        run.spawns.Add(new KaitSpawnRequest { tier = 1, sourceThreatCell = Vector2Int.one, targetCell = run.katePos, turnsUntilSpawn = 0, state = KaitSpawnState.Ready });
        var result = new KaitTurnResult();

        typeof(KaitRun).GetMethod("ResolveSpawnRequests", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, new object[] { result });

        Assert.AreEqual(2, run.kateHp);
        Assert.AreEqual(1, result.playerDamage);
        Assert.AreEqual(1, result.riftBlockDamage);
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
    public void T12_Merging64_OffersThirdSkillChoiceInsteadOfWinning()
    {
        KaitRun run = OpenRun(12, new Vector2Int(3, 3)); ClearThreat(run);
        run.threat[0, 0] = 32; run.threat[1, 0] = 32;

        KaitTurnResult result = run.TryTurn(KaitDirection.Right);

        Assert.IsTrue(result.turnComplete);
        Assert.IsFalse(run.ended);
        Assert.AreEqual(64, run.pendingSkillMilestone);
        CollectionAssert.AreEquivalent(new[] { KaitSkill.CatAgility, KaitSkill.ShadowStep }, run.SkillChoicesForMilestone(64));
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
        Assert.AreEqual(KaitDirection.Right, reverse.globalDirection);
        Assert.AreEqual(KaitDirection.Left, reverse.kaitDirection);
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
    public void V036_ThreatPillar_SplitsTopRowAndPreventsCrossPillarMerge()
    {
        KaitRun run = OpenRun(18, new Vector2Int(3, 3)); ClearThreat(run);
        run.threat[0, 4] = 2; run.threat[2, 4] = 2;

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);

        Assert.AreEqual(2, run.threat[0, 4]);
        Assert.AreEqual(0, run.threat[1, 4]);
        Assert.AreEqual(2, run.threat[4, 4]);
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
        Assert.AreEqual(0, run.threat[1, 4]);
        Assert.AreEqual(0, run.threat[3, 0]);
    }

    [Test]
    public void V036_T01_WallCollisionKillOfPrimaryContinuesChain()
    {
        KaitRun run = OpenRun(2601, new Vector2Int(3, 1));
        KaitEnemy primary = Enemy(1, new Vector2Int(5, 1), 2); run.enemies.Add(primary);

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);

        Assert.AreEqual(1, result.chainPower);
        Assert.AreEqual(1, result.damageDealt);
        Assert.AreEqual(1, result.collisionDamage);
        Assert.AreEqual(KaitEnemyLife.Dead, primary.life);
        Assert.AreEqual(new Vector2Int(5, 1), run.katePos);
        Assert.IsTrue(result.awaitingTurnChoice);
        Assert.AreEqual(1, result.chainKillCount);
    }

    [Test]
    public void V036_T02_NonlethalPushStillEndsChain()
    {
        KaitRun run = OpenRun(2602, new Vector2Int(2, 3));
        KaitEnemy primary = Enemy(1, new Vector2Int(4, 3), 2); run.enemies.Add(primary);

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);

        Assert.AreEqual(1, primary.hp);
        Assert.AreEqual(new Vector2Int(5, 3), primary.pos);
        Assert.IsTrue(result.pushed);
        Assert.IsTrue(result.turnComplete);
        Assert.IsFalse(result.awaitingTurnChoice);
    }

    [Test]
    public void V036_T03_PrimaryCollisionDeathContinuesButSecondaryDoesNotAddChainNode()
    {
        KaitRun run = OpenRun(2603, new Vector2Int(1, 3));
        KaitEnemy primary = Enemy(1, new Vector2Int(3, 3), 2);
        KaitEnemy secondary = Enemy(2, new Vector2Int(4, 3), 2);
        run.enemies.Add(primary); run.enemies.Add(secondary);

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);

        Assert.AreEqual(KaitEnemyLife.Dead, primary.life);
        Assert.AreEqual(1, secondary.hp);
        Assert.IsTrue(result.awaitingTurnChoice);
        Assert.AreEqual(1, result.chainKillCount);
        CollectionAssert.AreEqual(new[] { primary.id }, result.playerKilledEnemyIds);
    }

    [Test]
    public void V036_T04_SecondaryDeathAloneDoesNotContinueChain()
    {
        KaitRun run = OpenRun(2604, new Vector2Int(1, 3));
        KaitEnemy primary = Enemy(1, new Vector2Int(3, 3), 3);
        KaitEnemy secondary = Enemy(2, new Vector2Int(4, 3), 1);
        run.enemies.Add(primary); run.enemies.Add(secondary);

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);

        Assert.AreEqual(1, primary.hp);
        Assert.AreEqual(KaitEnemyLife.Dead, secondary.life);
        Assert.IsTrue(result.turnComplete);
        Assert.IsFalse(result.awaitingTurnChoice);
        Assert.AreEqual(0, result.chainKillCount);
        CollectionAssert.DoesNotContain(result.playerKilledEnemyIds, secondary.id);
    }

    [Test]
    public void V036_T05_ArcherReadyPhaseOnlyLocksAim()
    {
        KaitRun run = OpenRun(2605, new Vector2Int(5, 3));
        KaitEnemy archer = Enemy(1, new Vector2Int(1, 3), 2, KaitEnemyType.Archer, KaitEnemyLife.Active); run.enemies.Add(archer);

        KaitTurnResult result = ResolveEnemyPhase(run);

        Assert.AreEqual(KaitRangedState.Aim, archer.rangedState);
        Assert.AreEqual(KaitIntentType.LineShot, archer.intent.type);
        Assert.AreEqual(Vector2Int.right, archer.intent.direction);
        Assert.AreEqual(0, result.enemyActions.Count);
        Assert.AreEqual(3, run.kateHp);
    }

    [Test]
    public void V036_T06_ArcherFiresOnFollowingEnemyPhase()
    {
        KaitRun run = OpenRun(2606, new Vector2Int(4, 3));
        KaitEnemy archer = Enemy(1, new Vector2Int(1, 3), 2, KaitEnemyType.Archer, KaitEnemyLife.Active); run.enemies.Add(archer);
        ResolveEnemyPhase(run);
        LockEnemyIntents(run);

        KaitTurnResult result = ResolveEnemyPhase(run);

        Assert.AreEqual(2, run.kateHp);
        Assert.IsTrue(result.enemyActions.Single().hitKate);
        Assert.AreEqual(1, result.playerDamage);
        Assert.AreEqual(KaitRangedState.Ready, archer.rangedState);
        Assert.AreEqual(KaitIntentType.None, archer.intent.type);
    }

    [Test]
    public void Archer_OffAxisTargetStillAimsAndFiresAlongNearestCardinalDirection()
    {
        KaitRun run = OpenRun(26061, new Vector2Int(4, 4));
        KaitEnemy archer = Enemy(1, new Vector2Int(1, 3), 2, KaitEnemyType.Archer, KaitEnemyLife.Active); run.enemies.Add(archer);

        KaitTurnResult aim = ResolveEnemyPhase(run);
        Assert.AreEqual(0, aim.enemyActions.Count);
        Assert.AreEqual(Vector2Int.right, archer.intent.direction);
        LockEnemyIntents(run);

        KaitTurnResult shot = ResolveEnemyPhase(run);
        Assert.AreEqual(1, shot.enemyActions.Count);
        Assert.AreEqual(KaitIntentType.LineShot, shot.enemyActions.Single().type);
        Assert.Greater(shot.enemyActions.Single().affectedCells.Count, 0);
        Assert.IsFalse(shot.enemyActions.Single().hitKate);
        Assert.AreEqual(3, run.kateHp);
    }

    [Test]
    public void V036_T07_ArcherShotStopsAtFirstOfThreeEnemies()
    {
        KaitRun run = OpenRun(2607, new Vector2Int(5, 5));
        KaitEnemy archer = Enemy(1, new Vector2Int(1, 3), 2, KaitEnemyType.Archer, KaitEnemyLife.Active);
        KaitEnemy first = Enemy(2, new Vector2Int(2, 3), 2), second = Enemy(3, new Vector2Int(3, 3), 2), third = Enemy(4, new Vector2Int(4, 3), 2);
        run.enemies.Add(archer); run.enemies.Add(first); run.enemies.Add(second); run.enemies.Add(third);
        ResolveEnemyPhase(run);

        KaitTurnResult result = ResolveEnemyPhase(run);

        Assert.AreEqual(1, first.hp);
        Assert.AreEqual(2, second.hp);
        Assert.AreEqual(2, third.hp);
        CollectionAssert.AreEqual(new[] { first.id }, result.enemyActions.Single().friendlyHitIds);
    }

    [Test]
    public void V036_T08_PushedAimingArcherFiresFromNewCellInLockedDirection()
    {
        KaitRun run = OpenRun(2608, new Vector2Int(1, 3));
        KaitEnemy archer = Enemy(1, new Vector2Int(3, 3), 2, KaitEnemyType.Archer, KaitEnemyLife.Active);
        archer.rangedState = KaitRangedState.Aim; archer.intent = LineIntent(archer.pos, Vector2Int.up, new Vector2Int(3, 4), new Vector2Int(3, 5));
        KaitEnemy victim = Enemy(2, new Vector2Int(4, 5), 2);
        run.enemies.Add(archer); run.enemies.Add(victim);

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);
        KaitEnemyAction shot = result.enemyActions.Single(a => a.enemyId == archer.id);

        Assert.AreEqual(new Vector2Int(4, 3), archer.pos);
        Assert.AreEqual(new Vector2Int(4, 3), shot.from);
        Assert.AreEqual(1, victim.hp);
        CollectionAssert.Contains(shot.affectedCells, victim.pos);
    }

    [Test]
    public void V036_T09_WallStopsArcherShot()
    {
        KaitRun run = OpenRun(2609, new Vector2Int(5, 5));
        KaitEnemy archer = Enemy(1, new Vector2Int(1, 1), 2, KaitEnemyType.Archer, KaitEnemyLife.Active);
        archer.rangedState = KaitRangedState.Aim; archer.intent = LineIntent(archer.pos, Vector2Int.up);
        KaitEnemy behindWall = Enemy(2, new Vector2Int(1, 3), 2);
        run.enemies.Add(archer); run.enemies.Add(behindWall);

        KaitTurnResult result = ResolveEnemyPhase(run);

        Assert.AreEqual(2, behindWall.hp);
        Assert.AreEqual(0, result.enemyActions.Single().affectedCells.Count);
    }

    [Test]
    public void V036_T10_NonAdjacentMeleeEnemyStillHasNoIntent()
    {
        KaitRun run = OpenRun(2610, new Vector2Int(4, 3));
        KaitEnemy swordsman = Enemy(1, new Vector2Int(2, 3), 4, KaitEnemyType.Swordsman, KaitEnemyLife.Active); run.enemies.Add(swordsman);

        LockEnemyIntents(run);

        Assert.AreEqual(KaitIntentType.None, swordsman.intent.type);
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

    [Test]
    public void V037_T01_Milestone16_OffersExactlyOneOfTwoSkillsWithoutAdvancingTurn()
    {
        KaitRun run = OpenRun(3701); QueueMilestone(run, 16); int before = run.turn;
        CollectionAssert.AreEquivalent(new[] { KaitSkill.SwiftBoots, KaitSkill.DreadSlash }, run.SkillChoicesForMilestone(run.pendingSkillMilestone));
        Assert.IsTrue(run.ChooseSkill(KaitSkill.SwiftBoots)); Assert.AreEqual(before, run.turn); Assert.AreEqual(1, run.skills.Count);
    }

    [Test]
    public void V037_T02_Milestones32And64_ProduceSecondAndThirdSkillSlots()
    {
        KaitRun run = OpenRun(3702); Unlock(run, 16, KaitSkill.SwiftBoots); Unlock(run, 32, KaitSkill.IceTomb); Unlock(run, 64, KaitSkill.ShadowStep);
        CollectionAssert.AreEqual(new[] { KaitSkill.SwiftBoots, KaitSkill.IceTomb, KaitSkill.ShadowStep }, run.skills);
    }

    [Test]
    public void V037_T03_SwiftBoots_AddsOneBeforeFirstContact()
    {
        KaitRun run = OpenRun(3703, new Vector2Int(1, 1)); Unlock(run, 16, KaitSkill.SwiftBoots); run.enemies.Add(Enemy(1, new Vector2Int(4, 1), 3));
        Assert.IsTrue(run.TryUseSkill(KaitSkill.SwiftBoots, -1, out _)); KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(3, result.chainPower); Assert.AreEqual(3, result.damageDealt);
    }

    [Test]
    public void V037_T04_DreadSlash_DoesNotMoveKateAndMovesNormalEnemiesTogether()
    {
        KaitRun run = OpenRun(3704, new Vector2Int(3, 3)); Unlock(run, 16, KaitSkill.DreadSlash);
        KaitEnemy a = Enemy(1, new Vector2Int(1, 1), 3), b = Enemy(2, new Vector2Int(2, 4), 3); run.enemies.Add(a); run.enemies.Add(b);
        Assert.IsTrue(run.TryUseSkill(KaitSkill.DreadSlash, -1, out _)); KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(new Vector2Int(3, 3), run.katePos); Assert.IsTrue(result.dreadSlash); Assert.AreEqual(2, result.enemyActions.Count(x => x.type == KaitIntentType.Move));
    }

    [Test]
    public void V037_T05_IceTomb_SkipsExactlyOneEnemyPhase()
    {
        KaitRun run = OpenRun(3705, new Vector2Int(3, 3)); Unlock(run, 32, KaitSkill.IceTomb);
        KaitEnemy enemy = Enemy(1, new Vector2Int(4, 3), 4, KaitEnemyType.Swordsman, KaitEnemyLife.Active); run.enemies.Add(enemy); LockEnemyIntents(run);
        Assert.IsTrue(run.TryUseSkill(KaitSkill.IceTomb, enemy.id, out _)); ResolveEnemyPhase(run); Assert.AreEqual(3, run.kateHp); Assert.AreEqual(0, enemy.frozenActions);
        LockEnemyIntents(run); ResolveEnemyPhase(run); Assert.AreEqual(2, run.kateHp);
    }

    [Test]
    public void V037_T06_LesserPhantom_RejectsTargetsNoEnemyCanLegallyAttack()
    {
        KaitRun run = OpenRun(3706); Unlock(run, 32, KaitSkill.LesserPhantom);
        KaitEnemy target = Enemy(1, new Vector2Int(1, 1), 2, KaitEnemyType.Grunt, KaitEnemyLife.Active); run.enemies.Add(target);
        Assert.IsFalse(run.TryUseSkill(KaitSkill.LesserPhantom, target.id, out string message)); StringAssert.Contains("合法攻击", message);
    }

    [Test]
    public void V037_T07_LesserPhantom_RedirectsLegalMeleeForOnePhase()
    {
        KaitRun run = OpenRun(3707, new Vector2Int(5, 5)); Unlock(run, 32, KaitSkill.LesserPhantom);
        KaitEnemy target = Enemy(1, new Vector2Int(3, 3), 2, KaitEnemyType.Grunt, KaitEnemyLife.Active);
        KaitEnemy attacker = Enemy(2, new Vector2Int(2, 3), 4, KaitEnemyType.Swordsman, KaitEnemyLife.Active); run.enemies.Add(target); run.enemies.Add(attacker);
        Assert.IsTrue(run.TryUseSkill(KaitSkill.LesserPhantom, target.id, out _)); ResolveEnemyPhase(run);
        Assert.AreEqual(1, target.hp); Assert.AreEqual(-1, run.forcedTargetEnemyId);
    }

    [Test]
    public void V037_T08_CatAgility_DoublesMomentumBeforeFirstContact()
    {
        KaitRun run = OpenRun(3708, new Vector2Int(1, 1)); Unlock(run, 64, KaitSkill.CatAgility); run.enemies.Add(Enemy(1, new Vector2Int(4, 1), 4));
        run.TryUseSkill(KaitSkill.CatAgility, -1, out _); KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(4, result.chainPower);
    }

    [Test]
    public void V037_T09_SpeedSkillOrder_IsDeterministic()
    {
        KaitRun bootsThenCat = OpenRun(3709, new Vector2Int(1, 1)); Unlock(bootsThenCat, 16, KaitSkill.SwiftBoots); Unlock(bootsThenCat, 64, KaitSkill.CatAgility); bootsThenCat.enemies.Add(Enemy(1, new Vector2Int(4, 1), 9));
        bootsThenCat.TryUseSkill(KaitSkill.SwiftBoots, -1, out _); bootsThenCat.TryUseSkill(KaitSkill.CatAgility, -1, out _);
        KaitRun catThenBoots = OpenRun(3710, new Vector2Int(1, 1)); Unlock(catThenBoots, 16, KaitSkill.SwiftBoots); Unlock(catThenBoots, 64, KaitSkill.CatAgility); catThenBoots.enemies.Add(Enemy(1, new Vector2Int(4, 1), 9));
        catThenBoots.TryUseSkill(KaitSkill.CatAgility, -1, out _); catThenBoots.TryUseSkill(KaitSkill.SwiftBoots, -1, out _);
        Assert.AreEqual(6, bootsThenCat.TryGlobalInput(KaitDirection.Right).chainPower); Assert.AreEqual(5, catThenBoots.TryGlobalInput(KaitDirection.Right).chainPower);
    }

    [Test]
    public void V037_T10_CooldownTicksOnGlobalInputButNotChainInput()
    {
        KaitRun run = OpenRun(3711, new Vector2Int(1, 1)); Unlock(run, 16, KaitSkill.SwiftBoots); run.enemies.Add(Enemy(1, new Vector2Int(5, 1), 4));
        run.TryUseSkill(KaitSkill.SwiftBoots, -1, out _); run.TryGlobalInput(KaitDirection.Right); Assert.AreEqual(2, run.SkillCooldown(KaitSkill.SwiftBoots));
        run.ContinueChain(KaitDirection.Right); Assert.AreEqual(2, run.SkillCooldown(KaitSkill.SwiftBoots));
        run.TryGlobalInput(KaitDirection.Left); Assert.AreEqual(1, run.SkillCooldown(KaitSkill.SwiftBoots));
    }

    [Test]
    public void V037_T11_ShadowStep_MovesOneForwardWithoutAdvancingGlobalTurn()
    {
        KaitRun run = OpenRun(3712, new Vector2Int(1, 3)); Unlock(run, 64, KaitSkill.ShadowStep); run.enemies.Add(Enemy(1, new Vector2Int(3, 3), 1));
        run.TryGlobalInput(KaitDirection.Right); int before = run.turn; Assert.IsTrue(run.shadowStepAvailable); Assert.IsTrue(run.TryShadowStep());
        Assert.AreEqual(new Vector2Int(4, 3), run.katePos); Assert.AreEqual(before, run.turn); Assert.IsTrue(run.chainActive);
    }

    [Test]
    public void ActiveSpeedSkills_CanBeUsedDuringChainAndUpdatePowerImmediately()
    {
        KaitRun run = OpenRun(3714, new Vector2Int(1, 3));
        Unlock(run, 16, KaitSkill.SwiftBoots); Unlock(run, 64, KaitSkill.CatAgility);
        run.enemies.Add(Enemy(1, new Vector2Int(5, 3), 3));
        run.TryGlobalInput(KaitDirection.Right);

        Assert.IsTrue(run.chainActive);
        Assert.IsTrue(run.TryUseSkill(KaitSkill.SwiftBoots, -1, out _));
        Assert.AreEqual(4, run.momentum); Assert.AreEqual(4, run.chainPower);
        Assert.IsTrue(run.TryUseSkill(KaitSkill.CatAgility, -1, out _));
        Assert.AreEqual(8, run.momentum); Assert.AreEqual(8, run.chainPower);

        run.enemies.Add(Enemy(2, new Vector2Int(2, 3), 8));
        KaitTurnResult result = run.ContinueChain(KaitDirection.Left);
        Assert.AreEqual(8, result.damageDealt); Assert.IsTrue(result.awaitingTurnChoice);
    }

    [Test]
    public void DreadSlash_CanBeArmedAndTriggeredDuringChain()
    {
        KaitRun run = OpenRun(3715, new Vector2Int(1, 3)); Unlock(run, 16, KaitSkill.DreadSlash);
        run.enemies.Add(Enemy(1, new Vector2Int(5, 3), 3));
        run.TryGlobalInput(KaitDirection.Right);

        Assert.IsTrue(run.TryUseSkill(KaitSkill.DreadSlash, -1, out _));
        KaitTurnResult result = run.ContinueChain(KaitDirection.Left);

        Assert.IsTrue(result.dreadSlash); Assert.IsTrue(result.turnComplete); Assert.IsFalse(run.chainActive);
    }

    [Test]
    public void V037_T12_First128SpawnsBossAndDoesNotAutoWin()
    {
        KaitRun run = OpenRun(3713, new Vector2Int(3, 3)); ClearThreat(run); run.threat[0, 0] = 64; run.threat[1, 0] = 64;
        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);
        Assert.IsFalse(run.ended); Assert.IsTrue(run.bossSpawned); Assert.IsTrue(result.bossSpawned); Assert.AreEqual(8, run.enemies.Single(e => e.type == KaitEnemyType.ShieldKnight).hp);
    }

    [Test]
    public void V037_T13_BossSpawnReplacesOccupantWithoutKillCredit()
    {
        KaitRun run = OpenRun(3714, new Vector2Int(5, 5)); ClearThreat(run); run.threat[0, 0] = 64; run.threat[1, 0] = 64;
        KaitEnemy occupant = Enemy(77, new Vector2Int(3, 1), 2); run.enemies.Add(occupant); run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(KaitEnemyLife.Dead, occupant.life); Assert.AreEqual(0, run.kills); Assert.AreEqual(KaitEnemyType.ShieldKnight, run.EnemyAt(new Vector2Int(3, 1)).type);
    }

    [Test]
    public void V037_T14_ShieldKnight_IsStaticAndUsesSwordsmanMelee()
    {
        KaitRun run = OpenRun(3715, new Vector2Int(4, 3)); KaitEnemy boss = Enemy(1, new Vector2Int(3, 3), 8, KaitEnemyType.ShieldKnight, KaitEnemyLife.Active); run.enemies.Add(boss); LockEnemyIntents(run);
        ResolveEnemyPhase(run); Assert.AreEqual(new Vector2Int(3, 3), boss.pos); Assert.AreEqual(2, run.kateHp);
    }

    [Test]
    public void V037_T15_ShieldKnightFrontTakesZeroDirectAndCollisionDamage()
    {
        KaitRun run = OpenRun(3716, new Vector2Int(5, 3)); KaitEnemy boss = Enemy(1, new Vector2Int(3, 3), 8, KaitEnemyType.ShieldKnight); boss.facing = Vector2Int.right; run.enemies.Add(boss);
        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Left); Assert.AreEqual(0, result.damageDealt); Assert.AreEqual(8, boss.hp);
    }

    [Test]
    public void V037_T16_ShieldKnightSideTakesNormalDamage()
    {
        KaitRun run = OpenRun(3717, new Vector2Int(5, 3)); KaitEnemy boss = Enemy(1, new Vector2Int(3, 3), 8, KaitEnemyType.ShieldKnight); boss.facing = Vector2Int.up; run.enemies.Add(boss);
        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Left); Assert.AreEqual(1, result.damageDealt); Assert.AreEqual(7, boss.hp);
    }

    [Test]
    public void V037_T17_PhantomMakesBossFaceForcedTargetAtEnemyPhaseStart()
    {
        KaitRun run = OpenRun(3718, new Vector2Int(5, 5)); Unlock(run, 32, KaitSkill.LesserPhantom);
        KaitEnemy boss = Enemy(1, new Vector2Int(3, 3), 8, KaitEnemyType.ShieldKnight, KaitEnemyLife.Active);
        KaitEnemy target = Enemy(2, new Vector2Int(2, 3), 2, KaitEnemyType.Grunt, KaitEnemyLife.Active); run.enemies.Add(boss); run.enemies.Add(target);
        Assert.IsTrue(run.TryUseSkill(KaitSkill.LesserPhantom, target.id, out _)); ResolveEnemyPhase(run); Assert.AreEqual(Vector2Int.left, boss.facing);
    }

    [Test]
    public void V037_T18_KillingShieldKnightEndsRunInVictoryImmediately()
    {
        KaitRun run = OpenRun(3719, new Vector2Int(1, 3)); KaitEnemy boss = Enemy(1, new Vector2Int(5, 3), 3, KaitEnemyType.ShieldKnight); boss.facing = Vector2Int.up; run.enemies.Add(boss);
        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);
        Assert.IsTrue(run.ended); Assert.IsTrue(run.won); Assert.AreEqual("Victory: Shield Knight", run.endReason); Assert.IsTrue(result.turnComplete);
    }

    [Test]
    public void V040_T01_PendingSkillChoice_DoesNotBlockGlobalInput()
    {
        KaitRun run = OpenRun(4001, new Vector2Int(3, 3)); ClearThreat(run); run.threat[0, 0] = 2; QueueMilestone(run, 16);
        int before = run.turn; KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);
        Assert.IsTrue(result.valid); Assert.Greater(run.turn, before); Assert.AreEqual(16, run.pendingSkillMilestone);
    }

    [Test]
    public void V040_T02_PendingLaterMilestone_DoesNotBlockUnlockedSkill()
    {
        KaitRun run = OpenRun(4002); Unlock(run, 16, KaitSkill.SwiftBoots); QueueMilestone(run, 32);
        Assert.IsTrue(run.TryUseSkill(KaitSkill.SwiftBoots, -1, out string message), message);
        Assert.AreEqual(32, run.pendingSkillMilestone);
    }

    [Test]
    public void V040_T03_ChoosingPendingSkill_StillDoesNotAdvanceTurn()
    {
        KaitRun run = OpenRun(4003); QueueMilestone(run, 16); int before = run.turn;
        Assert.IsTrue(run.ChooseSkill(KaitSkill.DreadSlash)); Assert.AreEqual(before, run.turn); Assert.AreEqual(0, run.pendingSkillMilestone);
    }

    [Test]
    public void Settings_PlayerInvincible_PreventsEnemyAndRiftDamage()
    {
        KaitRun run = OpenRun(4101, new Vector2Int(3, 3));
        run.config.playerInvincible = true;
        KaitEnemy attacker = Enemy(1, new Vector2Int(3, 2), 3, KaitEnemyType.Swordsman, KaitEnemyLife.Active);
        attacker.intent = new KaitIntent { type = KaitIntentType.Melee, origin = attacker.pos, target = run.katePos, damage = 1 };
        attacker.intent.affectedCells.Add(run.katePos);
        run.enemies.Add(attacker);

        KaitTurnResult attack = ResolveEnemyPhase(run);
        Assert.AreEqual(3, run.kateHp);
        Assert.AreEqual(0, attack.playerDamage);
        Assert.IsTrue(attack.enemyActions.Single().hitKate);

        run.spawns.Add(new KaitSpawnRequest { targetCell = run.katePos, turnsUntilSpawn = 0, state = KaitSpawnState.Ready });
        KaitTurnResult rift = ResolveSpawnPhase(run);
        Assert.AreEqual(3, run.kateHp);
        Assert.AreEqual(0, rift.riftBlockDamage);
        Assert.AreEqual(1, rift.spawnSuppressed);
    }

    [Test]
    public void Settings_DisableRiftDamage_StillSuppressesBlockedSpawn()
    {
        KaitRun run = OpenRun(4102);
        run.config.enableRiftDamage = false;
        Vector2Int cell = new Vector2Int(2, 2);
        KaitEnemy occupant = Enemy(1, cell, 2); run.enemies.Add(occupant);
        run.spawns.Add(new KaitSpawnRequest { targetCell = cell, turnsUntilSpawn = 0, state = KaitSpawnState.Ready });

        KaitTurnResult result = ResolveSpawnPhase(run);

        Assert.AreEqual(2, occupant.hp);
        Assert.AreEqual(0, result.riftBlockDamage);
        Assert.AreEqual(1, result.spawnSuppressed);
        Assert.AreEqual(0, run.spawns.Count);
    }

    [Test]
    public void Settings_DisableFriendlyFire_PreventsEnemyDamage()
    {
        KaitRun run = OpenRun(4103, new Vector2Int(5, 5));
        run.config.enableFriendlyFire = false;
        KaitEnemy attacker = Enemy(1, new Vector2Int(1, 2), 3, KaitEnemyType.Archer, KaitEnemyLife.Active);
        KaitEnemy victim = Enemy(2, new Vector2Int(2, 2), 2, KaitEnemyType.Grunt, KaitEnemyLife.Active);
        attacker.rangedState = KaitRangedState.Aim;
        attacker.intent = LineIntent(attacker.pos, Vector2Int.right, victim.pos, new Vector2Int(3, 2));
        run.enemies.Add(attacker); run.enemies.Add(victim);

        KaitTurnResult result = ResolveEnemyPhase(run);

        Assert.AreEqual(2, victim.hp);
        Assert.AreEqual(0, result.friendlyFireDamage);
        Assert.AreEqual(0, result.enemyActions.Single().friendlyHitIds.Count);
    }

    [Test]
    public void Settings_DisableCollisionDamage_KeepsPushBlockWithoutExtraDamage()
    {
        KaitRun run = OpenRun(4104, new Vector2Int(1, 3));
        run.config.enableCollisionDamage = false;
        KaitEnemy enemy = Enemy(1, new Vector2Int(5, 3), 9); run.enemies.Add(enemy);

        KaitTurnResult result = run.TryGlobalInput(KaitDirection.Right);

        Assert.IsTrue(result.pushBlockedByWall);
        Assert.AreEqual(0, result.collisionDamage);
        Assert.AreEqual(6, enemy.hp);
    }

    private static void QueueMilestone(KaitRun run, int value)
        => typeof(KaitRun).GetMethod("HandleMilestoneMerge", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, new object[] { new KaitMergeEvent { resultValue = value } });
    private static void Unlock(KaitRun run, int milestone, KaitSkill skill) { QueueMilestone(run, milestone); Assert.IsTrue(run.ChooseSkill(skill)); }

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
        var intent = new KaitIntent { type = KaitIntentType.LineShot, origin = origin, direction = direction, damage = 1, target = cells.Length > 0 ? cells[cells.Length - 1] : origin };
        intent.affectedCells.AddRange(cells); return intent;
    }
    private static KaitTurnResult ResolveEnemyPhase(KaitRun run)
    {
        var result = new KaitTurnResult();
        typeof(KaitRun).GetMethod("ResolveEnemyIntents", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, new object[] { result });
        return result;
    }
    private static KaitTurnResult ResolveSpawnPhase(KaitRun run)
    {
        var result = new KaitTurnResult();
        typeof(KaitRun).GetMethod("ResolveSpawnRequests", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, new object[] { result });
        return result;
    }
    private static void LockEnemyIntents(KaitRun run)
        => typeof(KaitRun).GetMethod("LockEnemyIntents", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, null);
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
