using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class KaitGame : MonoBehaviour
{
    private static KaitGame instance;
    private readonly KaitRun run = new KaitRun();
    private Font uiFont;
    private Canvas canvas;
    private Image[] battleCells;
    private Text[] battleLabels;
    private Image[] threatCells;
    private Text[] threatLabels;
    private Text turnText;
    private Text statusText;
    private Text helpText;
    private GameObject endOverlay;
    private Text endText;
    private bool busy;
    private Vector2Int? displayKate;
    private Vector2Int? trailCell;
    private bool hideKate;
    private List<KaitEnemy> animatedEnemies;
    private List<KaitSpawnRequest> animatedSpawns;
    private int[,] displayedThreat;
    private bool hideThreatValues;
    private readonly HashSet<Vector2Int> impactCells = new HashSet<Vector2Int>();
    private string logPath;

    private sealed class ThreatVisual
    {
        public RectTransform rect;
        public Vector3 from;
        public Vector3 to;
    }

    private sealed class EnemyMoveVisual
    {
        public KaitEnemy enemy;
        public RectTransform rect;
        public Vector3 from;
        public Vector3 to;
    }

    private static readonly Color Background = Hex("#2F2932");
    private static readonly Color Panel = Hex("#493E49");
    private static readonly Color PanelLight = Hex("#62525E");
    private static readonly Color Cream = Hex("#FFF2DD");
    private static readonly Color Peach = Hex("#FAC7B7");
    private static readonly Color Coral = Hex("#E98D83");
    private static readonly Color Wine = Hex("#8C4352");
    private static readonly Color Void = Hex("#211E24");
    private static readonly Color Gold = Hex("#F3C56B");
    private static readonly Color Cyan = Hex("#83D2C9");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        var host = new GameObject("Kait Game Runtime");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<KaitGame>();
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Arial" }, 24);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SetupAfterSceneLoad());
    }

    private IEnumerator SetupAfterSceneLoad()
    {
        yield return null;
        foreach (Canvas oldCanvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (oldCanvas != canvas) oldCanvas.gameObject.SetActive(false);
        foreach (GameManager oldManager in FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)) oldManager.enabled = false;
        foreach (TileBoard oldBoard in FindObjectsByType<TileBoard>(FindObjectsInactive.Include, FindObjectsSortMode.None)) oldBoard.enabled = false;

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(events);
        }

        if (canvas == null) BuildUI();
        NewRun();
        string screenshotPath = CommandLineValue("-kaitScreenshot");
        string demoStepsValue = CommandLineValue("-kaitDemoSteps");
        int.TryParse(demoStepsValue, out int demoSteps);
        if (!string.IsNullOrEmpty(screenshotPath)) StartCoroutine(CaptureAndQuit(screenshotPath, demoSteps));
    }

    private void Update()
    {
        if (busy || run.ended) return;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) HandleDirection(KaitDirection.Up);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) HandleDirection(KaitDirection.Down);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) HandleDirection(KaitDirection.Left);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) HandleDirection(KaitDirection.Right);
        else if (Input.GetKeyDown(KeyCode.R)) NewRun();
    }

    private void BuildUI()
    {
        var canvasGo = new GameObject("Kait Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image bg = Rect("Background", canvas.transform, Vector2.zero, new Vector2(1600, 900), Background);
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.sizeDelta = Vector2.zero;

        MakeText("Kait · Momentum Prototype v0.2", bg.transform, new Vector2(-320, 406), new Vector2(900, 54), 30, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        MakeText("初始方向共享 · 击杀转向只控制凯特", bg.transform, new Vector2(480, 407), new Vector2(520, 42), 17, Peach, TextAnchor.MiddleRight);

        BuildBattleBoard(bg.transform);
        BuildThreatBoard(bg.transform);
        BuildSidebar(bg.transform);
        BuildEndOverlay(bg.transform);
    }

    private void BuildBattleBoard(Transform parent)
    {
        Image frame = Rect("Battle Panel", parent, new Vector2(-365, -38), new Vector2(748, 748), Panel);
        MakeText("9 × 9  滑行战场", frame.transform, new Vector2(0, 347), new Vector2(700, 42), 22, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        var gridGo = new GameObject("Battle Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(frame.transform, false);
        RectTransform gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(690, 690);
        gridRect.anchoredPosition = new Vector2(0, -20);
        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(72, 72);
        grid.spacing = new Vector2(5, 5);
        grid.padding = new RectOffset(1, 1, 1, 1);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 9;

        battleCells = new Image[81];
        battleLabels = new Text[81];
        for (int visualY = 8; visualY >= 0; visualY--)
        {
            for (int x = 0; x < 9; x++)
            {
                int index = x + visualY * 9;
                Image cell = Rect($"Cell {x},{visualY}", gridGo.transform, Vector2.zero, Vector2.zero, PanelLight);
                battleCells[index] = cell;
                battleLabels[index] = MakeText("", cell.transform, Vector2.zero, Vector2.zero, 19, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(battleLabels[index].rectTransform, 3);
            }
        }
    }

    private void BuildThreatBoard(Transform parent)
    {
        Image frame = Rect("Threat Panel", parent, new Vector2(477, 164), new Vector2(420, 465), Panel);
        MakeText("7 × 7  全尺寸威胁盘", frame.transform, new Vector2(0, 202), new Vector2(374, 42), 22, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        MakeText("逐格映射 · 每个有效回合新增 2", frame.transform, new Vector2(0, 172), new Vector2(374, 28), 14, Peach, TextAnchor.MiddleLeft);
        var gridGo = new GameObject("Threat Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(frame.transform, false);
        RectTransform rt = gridGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(358, 358);
        rt.anchoredPosition = new Vector2(0, -25);
        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(46, 46);
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = run.ThreatSize;

        threatCells = new Image[run.ThreatSize * run.ThreatSize];
        threatLabels = new Text[run.ThreatSize * run.ThreatSize];
        for (int visualY = run.ThreatSize - 1; visualY >= 0; visualY--)
        {
            for (int x = 0; x < run.ThreatSize; x++)
            {
                int index = x + visualY * run.ThreatSize;
                threatCells[index] = Rect($"Threat {x},{visualY}", gridGo.transform, Vector2.zero, Vector2.zero, Void);
                threatLabels[index] = MakeText("", threatCells[index].transform, Vector2.zero, Vector2.zero, 18, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(threatLabels[index].rectTransform, 2);
            }
        }
    }

    private void BuildSidebar(Transform parent)
    {
        Image info = Rect("Run Info", parent, new Vector2(477, -119), new Vector2(420, 82), Panel);
        turnText = MakeText("", info.transform, new Vector2(0, 16), new Vector2(380, 34), 18, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        statusText = MakeText("", info.transform, new Vector2(0, -18), new Vector2(380, 30), 15, Peach, TextAnchor.MiddleLeft);

        Image rules = Rect("Momentum Rules", parent, new Vector2(477, -249), new Vector2(420, 156), Panel);
        MakeText("v0.2 动量规则", rules.transform, new Vector2(0, 54), new Vector2(380, 30), 17, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        MakeText("起步 M1 · 每走空格 +1\n碰撞伤害 = 当前动量 · 未击杀会保留 HP\n击杀后 M-1，可直行 / 左转 / 右转", rules.transform, new Vector2(0, -16), new Vector2(380, 92), 15, Peach, TextAnchor.MiddleLeft);

        Image controls = Rect("Controls", parent, new Vector2(477, -393), new Vector2(420, 114), Panel);
        MakeText("WASD / 方向键", controls.transform, new Vector2(-88, 30), new Vector2(180, 28), 14, Peach, TextAnchor.MiddleLeft);
        MakeButton(controls.transform, new Vector2(-20, 28), new Vector2(48, 42), "W").onClick.AddListener(() => HandleDirection(KaitDirection.Up));
        MakeButton(controls.transform, new Vector2(-72, -20), new Vector2(48, 42), "A").onClick.AddListener(() => HandleDirection(KaitDirection.Left));
        MakeButton(controls.transform, new Vector2(-20, -20), new Vector2(48, 42), "S").onClick.AddListener(() => HandleDirection(KaitDirection.Down));
        MakeButton(controls.transform, new Vector2(32, -20), new Vector2(48, 42), "D").onClick.AddListener(() => HandleDirection(KaitDirection.Right));
        MakeButton(controls.transform, new Vector2(139, 0), new Vector2(112, 72), "重新开始\nR").onClick.AddListener(NewRun);

        helpText = MakeText("", parent, new Vector2(0, -424), new Vector2(760, 35), 15, Peach, TextAnchor.MiddleCenter);
    }

    private void BuildEndOverlay(Transform parent)
    {
        Image shade = Rect("End Overlay", parent, Vector2.zero, new Vector2(1600, 900), new Color(0.08f, 0.06f, 0.08f, 0.92f));
        Stretch(shade.rectTransform, 0);
        endOverlay = shade.gameObject;
        Image card = Rect("End Card", shade.transform, Vector2.zero, new Vector2(520, 310), Panel);
        MakeText("本局结束", card.transform, new Vector2(0, 102), new Vector2(460, 58), 32, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        endText = MakeText("", card.transform, new Vector2(0, 20), new Vector2(450, 100), 19, Peach, TextAnchor.MiddleCenter);
        MakeButton(card.transform, new Vector2(0, -92), new Vector2(220, 60), "再来一局").onClick.AddListener(NewRun);
        endOverlay.SetActive(false);
    }

    private void NewRun()
    {
        StopAllCoroutines();
        busy = false;
        displayKate = null;
        trailCell = null;
        int seed = Environment.TickCount;
        run.Reset(seed);
        logPath = Path.Combine(Application.persistentDataPath, $"kait_run_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllText(logPath, "turn,direction,kateStart,kateEnd,slideDistance,damage,kills,chainActive,momentum,activeEnemies,pendingSpawns,highestThreat,threatOccupancy,threatLocks,endReason\n", Encoding.UTF8);
        endOverlay.SetActive(false);
        statusText.text = "选择方向开始。不能主动刹车。";
        RefreshAll();
    }

    private void HandleDirection(KaitDirection direction)
    {
        if (busy || run.ended) return;
        Vector2Int start = run.katePos;
        List<KaitEnemy> enemySnapshot = SnapshotEnemies();
        List<KaitSpawnRequest> spawnSnapshot = SnapshotSpawns();
        KaitTurnResult result = run.chainActive ? run.ContinueChain(direction) : run.TryTurn(direction);
        if (!result.valid)
        {
            statusText.text = result.message;
            StartCoroutine(FlashStatus());
            return;
        }
        if (result.turnComplete) AppendLog(direction, start, result);
        StartCoroutine(PlayTurn(result, start, enemySnapshot, spawnSnapshot));
    }

    private IEnumerator PlayTurn(KaitTurnResult result, Vector2Int start, List<KaitEnemy> enemySnapshot, List<KaitSpawnRequest> spawnSnapshot)
    {
        busy = true;
        animatedEnemies = enemySnapshot;
        animatedSpawns = spawnSnapshot;
        displayedThreat = result.threatBefore;

        bool kateDone = false;
        bool threatDone = result.threatAfter == null;
        StartCoroutine(RunPhase(AnimateKateSlide(result, start), () => kateDone = true));
        if (!threatDone) StartCoroutine(RunPhase(AnimateThreat(result), () => threatDone = true));
        while (!kateDone || !threatDone) yield return null;

        yield return AnimateAllEnemyActions(result.enemyActions);

        animatedEnemies = null;
        animatedSpawns = null;
        hideKate = false;
        displayKate = null;
        trailCell = null;
        RefreshBattle();

        var spawnPulses = new List<RectTransform>();
        foreach (Vector2Int cell in result.spawnedEnemyCells)
            spawnPulses.Add(battleCells[cell.x + cell.y * 9].rectTransform);
        foreach (KaitSpawnRequest spawn in run.spawns)
            if (spawn.targetCell.x >= 0) spawnPulses.Add(battleCells[spawn.targetCell.x + spawn.targetCell.y * 9].rectTransform);
        if (spawnPulses.Count > 0) yield return ScalePulseMany(spawnPulses, 0.35f, 1.15f, 0.2f);

        displayedThreat = null;
        statusText.text = result.message + (result.merges.Count > 0 ? $" · 威胁合并 ×{result.merges.Count}" : "");
        RefreshAll();
        busy = false;
        if (run.ended) ShowEnd();
    }

    private void RefreshAll()
    {
        RefreshBattle();
        RefreshThreat();
        turnText.text = $"回合 {run.turn}   击杀 {run.kills}   敌人 {run.enemies.FindAll(e => e.life != KaitEnemyLife.Dead).Count}/{run.spawns.Count}   动量 M{run.momentum}";
        helpText.text = run.chainActive ? "击杀转向：仅高亮方向可用，禁止 180°；此输入不会推动威胁盘" : "青色格=移动/可选方向  红色格=攻击范围  敌人数字=当前 HP";
        if (run.ended) ShowEnd();
    }

    private void RefreshBattle()
    {
        Vector2Int kate = displayKate ?? run.katePos;
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                int index = x + y * 9;
                Vector2Int p = new Vector2Int(x, y);
                Image image = battleCells[index];
                Text label = battleLabels[index];
                label.text = "";
                image.color = run.walls[x, y] ? Void : Hex("#796573");
                Color? intentTint = IntentTintAt(p);
                if (!run.walls[x, y] && intentTint.HasValue) image.color = intentTint.Value;
                if (run.chainActive)
                {
                    foreach (KaitDirection choice in run.AllowedTurnDirections())
                        if (run.katePos + KaitRun.Delta(choice) == p) image.color = Cyan;
                }
                if (trailCell.HasValue && trailCell.Value == p) image.color = new Color(Peach.r, Peach.g, Peach.b, 0.45f);
                if (impactCells.Contains(p)) image.color = Gold;

                KaitSpawnRequest spawn = SpawnAtVisual(p);
                KaitEnemy enemy = EnemyAtVisual(p);
                if (spawn != null)
                {
                    image.color = Wine;
                    label.text = $"裂\nT{spawn.tier}";
                    label.color = Cream;
                }
                if (enemy != null)
                {
                    image.color = EnemyColor(enemy.type, enemy.life);
                    string type = enemy.type == KaitEnemyType.Grunt ? "兵" : enemy.type == KaitEnemyType.Guard ? "盾" : enemy.type == KaitEnemyType.Archer ? "弓" : "精";
                    string intent = IntentGlyph(enemy.intent.type, enemy.intent.direction);
                    label.text = enemy.life == KaitEnemyLife.Preparing ? $"{type} {enemy.hp}\n准备" : $"{type} HP{enemy.hp}\n{intent}";
                    label.color = enemy.life == KaitEnemyLife.Preparing ? Peach : Cream;
                }
                if (!hideKate && kate == p)
                {
                    image.color = Coral;
                    label.text = run.chainActive ? $"凯\nM{run.momentum}" : "凯";
                    label.fontSize = run.chainActive ? 18 : 28;
                    label.color = Cream;
                }
                else label.fontSize = 17;
            }
        }
    }

    private void RefreshThreat()
    {
        for (int y = 0; y < run.ThreatSize; y++)
            for (int x = 0; x < run.ThreatSize; x++)
            {
                int index = x + y * run.ThreatSize;
                int value = displayedThreat == null ? run.threat[x, y] : displayedThreat[x, y];
                threatLabels[index].text = value == 0 || hideThreatValues ? "" : value.ToString();
                threatCells[index].color = ThreatColor(value);
                threatLabels[index].color = value >= 16 ? Cream : Void;
            }
    }

    private List<KaitEnemy> SnapshotEnemies()
    {
        var snapshot = new List<KaitEnemy>();
        foreach (KaitEnemy enemy in run.enemies)
        {
            if (enemy.life == KaitEnemyLife.Dead) continue;
            snapshot.Add(new KaitEnemy
            {
                id = enemy.id,
                type = enemy.type,
                pos = enemy.pos,
                hp = enemy.hp,
                maxHp = enemy.maxHp,
                life = enemy.life,
                intent = new KaitIntent
                {
                    type = enemy.intent.type,
                    target = enemy.intent.target,
                    direction = enemy.intent.direction
                }
            });
        }
        return snapshot;
    }

    private List<KaitSpawnRequest> SnapshotSpawns()
    {
        var snapshot = new List<KaitSpawnRequest>();
        foreach (KaitSpawnRequest spawn in run.spawns)
            snapshot.Add(new KaitSpawnRequest
            {
                tier = spawn.tier,
                sourceThreatCell = spawn.sourceThreatCell,
                targetCell = spawn.targetCell,
                turnsUntilSpawn = spawn.turnsUntilSpawn,
                state = spawn.state,
                initialDirection = spawn.initialDirection
            });
        return snapshot;
    }

    private KaitSpawnRequest SpawnAtVisual(Vector2Int p)
    {
        if (animatedSpawns == null) return run.SpawnAt(p);
        return animatedSpawns.Find(s => s.targetCell == p);
    }

    private IEnumerator RunPhase(IEnumerator phase, Action onComplete)
    {
        yield return StartCoroutine(phase);
        onComplete();
    }

    private IEnumerator AnimateKateSlide(KaitTurnResult result, Vector2Int start)
    {
        if (result.katePath.Count == 0)
        {
            if (result.blockedEnemyCell.x >= 0)
                yield return PulseBattleCell(result.blockedEnemyCell, Coral, 0.16f);
            yield break;
        }

        hideKate = true;
        RefreshBattle();
        RectTransform token = CreateFloatingToken("凯", Coral, battleCells[start.x + start.y * 9].rectTransform, new Vector2(72, 72), 28);
        var points = new List<Vector3> { battleCells[start.x + start.y * 9].rectTransform.position };
        foreach (Vector2Int cell in result.katePath) points.Add(battleCells[cell.x + cell.y * 9].rectTransform.position);

        int segments = points.Count - 1;
        float duration = Mathf.Min(0.36f, 0.16f + segments * 0.025f);
        float elapsed = 0f;
        int lastReached = 0;
        while (elapsed < duration)
        {
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration) * segments;
            int segment = Mathf.Min(segments - 1, Mathf.FloorToInt(progress));
            token.position = Vector3.Lerp(points[segment], points[segment + 1], progress - segment);
            int reached = Mathf.Min(segments, Mathf.FloorToInt(progress));
            while (lastReached < reached)
            {
                lastReached++;
                Vector2Int cell = result.katePath[lastReached - 1];
                if (result.killedEnemyCells.Contains(cell))
                {
                    animatedEnemies.RemoveAll(e => e.pos == cell);
                    impactCells.Add(cell);
                    RefreshBattle();
                }
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        token.position = points[points.Count - 1];
        Destroy(token.gameObject);
        foreach (Vector2Int killed in result.killedEnemyCells) animatedEnemies.RemoveAll(e => e.pos == killed);
        impactCells.Clear();
        hideKate = false;
        displayKate = null;
        RefreshBattle();
        if (result.blockedEnemyCell.x >= 0)
            yield return PulseBattleCell(result.blockedEnemyCell, Coral, 0.14f);
    }

    private IEnumerator AnimateAllEnemyActions(List<KaitEnemyAction> actions)
    {
        var moves = new List<EnemyMoveVisual>();
        foreach (KaitEnemyAction action in actions)
        {
            KaitEnemy enemy = animatedEnemies?.Find(e => e.id == action.enemyId);
            if (enemy == null) continue;
            if (action.type == KaitIntentType.Move && action.from != action.to)
            {
                string type = enemy.type == KaitEnemyType.Grunt ? "兵" : enemy.type == KaitEnemyType.Guard ? "盾" : enemy.type == KaitEnemyType.Archer ? "弓" : "精";
                RectTransform token = CreateFloatingToken($"{type} {enemy.hp}", EnemyColor(enemy.type, enemy.life), battleCells[action.from.x + action.from.y * 9].rectTransform, new Vector2(72, 72), 17);
                moves.Add(new EnemyMoveVisual
                {
                    enemy = enemy,
                    rect = token,
                    from = battleCells[action.from.x + action.from.y * 9].rectTransform.position,
                    to = battleCells[action.to.x + action.to.y * 9].rectTransform.position
                });
                animatedEnemies.Remove(enemy);
            }
            if (action.type == KaitIntentType.Melee || action.type == KaitIntentType.LineShot)
                foreach (Vector2Int cell in action.affectedCells) if (InsideBattle(cell)) impactCells.Add(cell);
        }

        RefreshBattle();
        if (moves.Count == 0 && impactCells.Count == 0) yield break;
        float elapsed = 0f;
        const float duration = 0.2f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            foreach (EnemyMoveVisual move in moves) move.rect.position = Vector3.Lerp(move.from, move.to, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        foreach (EnemyMoveVisual move in moves)
        {
            move.enemy.pos = run.enemies.Find(e => e.id == move.enemy.id)?.pos ?? move.enemy.pos;
            animatedEnemies.Add(move.enemy);
            Destroy(move.rect.gameObject);
        }
        impactCells.Clear();
        RefreshBattle();
    }

    private KaitEnemy EnemyAtVisual(Vector2Int p)
    {
        if (animatedEnemies == null) return run.EnemyAt(p);
        return animatedEnemies.Find(e => e.life != KaitEnemyLife.Dead && e.pos == p);
    }

    private Color? IntentTintAt(Vector2Int p)
    {
        List<KaitEnemy> source = animatedEnemies ?? run.enemies;
        foreach (KaitEnemy enemy in source)
        {
            if (enemy.life != KaitEnemyLife.Active) continue;
            if (enemy.intent.type == KaitIntentType.Move && enemy.intent.target == p)
                return Hex("#547978");
            if (enemy.intent.type == KaitIntentType.Melee && enemy.intent.target == p)
                return Hex("#A64F5A");
            if (enemy.intent.type == KaitIntentType.LineShot)
            {
                Vector2Int direction = enemy.intent.direction;
                for (Vector2Int cell = enemy.pos + direction; InsideBattle(cell) && !BlocksIntentLine(cell, source); cell += direction)
                    if (cell == p) return Hex("#944A58");
            }
        }
        return null;
    }

    private bool BlocksIntentLine(Vector2Int p, List<KaitEnemy> source)
    {
        if (run.walls[p.x, p.y]) return true;
        return source.Exists(e => e.life != KaitEnemyLife.Dead && e.pos == p);
    }

    private static bool InsideBattle(Vector2Int p) => p.x >= 0 && p.x < 9 && p.y >= 0 && p.y < 9;

    private IEnumerator AnimateThreat(KaitTurnResult result)
    {
        displayedThreat = result.threatBefore;
        hideThreatValues = true;
        RefreshThreat();
        var visuals = new List<ThreatVisual>();
        foreach (KaitThreatMotion motion in result.threatMotions)
        {
            RectTransform fromCell = threatCells[motion.from.x + motion.from.y * run.ThreatSize].rectTransform;
            RectTransform toCell = threatCells[motion.to.x + motion.to.y * run.ThreatSize].rectTransform;
            RectTransform token = CreateFloatingToken(motion.value.ToString(), ThreatColor(motion.value), fromCell, new Vector2(46, 46), 18);
            visuals.Add(new ThreatVisual { rect = token, from = fromCell.position, to = toCell.position });
        }

        float elapsed = 0f;
        const float duration = 0.18f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            foreach (ThreatVisual visual in visuals) visual.rect.position = Vector3.Lerp(visual.from, visual.to, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        foreach (ThreatVisual visual in visuals) Destroy(visual.rect.gameObject);

        displayedThreat = result.threatAfter;
        hideThreatValues = false;
        RefreshThreat();
        if (result.merges.Count > 0)
        {
            int strongest = 0;
            var mergeCells = new List<RectTransform>();
            foreach (KaitMergeEvent merge in result.merges)
            {
                strongest = Mathf.Max(strongest, merge.resultValue);
                mergeCells.Add(threatCells[merge.threatCell.x + merge.threatCell.y * run.ThreatSize].rectTransform);
            }
            yield return ScalePulseMany(mergeCells, 0.72f, 1.2f, 0.14f);
            GameAudio.PlayMerge(strongest);
        }
        foreach (Vector2Int cell in result.newThreatCells)
            yield return ScalePulse(threatCells[cell.x + cell.y * run.ThreatSize].rectTransform, 0.1f, 1.1f, 0.1f);
    }

    private RectTransform CreateFloatingToken(string label, Color color, RectTransform source, Vector2 size, int fontSize)
    {
        Image image = Rect("Animation Token", canvas.transform, Vector2.zero, size, color);
        RectTransform rect = image.rectTransform;
        rect.position = source.position;
        rect.SetAsLastSibling();
        Text text = MakeText(label, image.transform, Vector2.zero, size, fontSize, color.grayscale < 0.55f ? Cream : Void, TextAnchor.MiddleCenter, FontStyle.Bold);
        Stretch(text.rectTransform, 2);
        return rect;
    }

    private IEnumerator PulseBattleCell(Vector2Int cell, Color color, float duration)
    {
        Image image = battleCells[cell.x + cell.y * 9];
        Color original = image.color;
        image.color = color;
        yield return ScalePulse(image.rectTransform, 0.92f, 1.16f, duration);
        image.color = original;
    }

    private IEnumerator ScalePulse(RectTransform rect, float from, float peak, float duration)
    {
        float elapsed = 0f;
        rect.localScale = Vector3.one * from;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = t < 0.55f ? Mathf.Lerp(from, peak, t / 0.55f) : Mathf.Lerp(peak, 1f, (t - 0.55f) / 0.45f);
            rect.localScale = Vector3.one * scale;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    private IEnumerator ScalePulseMany(List<RectTransform> rects, float from, float peak, float duration)
    {
        if (rects.Count == 0) yield break;
        float elapsed = 0f;
        foreach (RectTransform rect in rects) rect.localScale = Vector3.one * from;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = t < 0.55f ? Mathf.Lerp(from, peak, t / 0.55f) : Mathf.Lerp(peak, 1f, (t - 0.55f) / 0.45f);
            foreach (RectTransform rect in rects) rect.localScale = Vector3.one * scale;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        foreach (RectTransform rect in rects) rect.localScale = Vector3.one;
    }

    private void ShowEnd()
    {
        endOverlay.SetActive(true);
        string reason = run.endReason == "Threat Overload" ? "威胁盘锁死" : "凯特被敌人击中";
        endText.text = $"{reason}\n\n生存回合：{run.turn}    击杀：{run.kills}\n最高威胁：{run.highestThreat}";
    }

    private void AppendLog(KaitDirection direction, Vector2Int start, KaitTurnResult result)
    {
        int occupied = 0;
        foreach (int value in run.threat) if (value != 0) occupied++;
        float occupancy = occupied / (float)run.threat.Length;
        string line = $"{run.turn},{direction},{start.x}:{start.y},{run.katePos.x}:{run.katePos.y},{result.slideDistance},{result.damageDealt},{run.kills},{run.chainActive},{run.momentum},{run.enemies.FindAll(e => e.life != KaitEnemyLife.Dead).Count},{run.spawns.Count},{run.highestThreat},{occupancy:F3},{run.threatLocks},{run.endReason}\n";
        File.AppendAllText(logPath, line, Encoding.UTF8);
    }

    private IEnumerator FlashStatus()
    {
        Color old = statusText.color;
        statusText.color = Gold;
        yield return new WaitForSecondsRealtime(0.16f);
        statusText.color = old;
    }

    private IEnumerator CaptureAndQuit(string path, int demoSteps)
    {
        KaitDirection[] sequence = { KaitDirection.Up, KaitDirection.Left, KaitDirection.Down, KaitDirection.Right };
        for (int i = 0; i < demoSteps && !run.ended; i++)
        {
            KaitDirection next = run.chainActive && run.AllowedTurnDirections().Count > 0
                ? run.AllowedTurnDirections()[0]
                : sequence[i % sequence.Length];
            HandleDirection(next);
            while (busy) yield return null;
            yield return new WaitForSecondsRealtime(0.08f);
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(path, 1);
        yield return new WaitForSecondsRealtime(1f);
        Application.Quit();
    }

    private static string CommandLineValue(string key)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == key) return args[i + 1];
        return string.Empty;
    }

    private Image Rect(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Text MakeText(string value, Transform parent, Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        Text text = go.GetComponent<Text>();
        text.text = value;
        text.font = uiFont;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = anchor;
        text.fontStyle = style;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private Button MakeButton(Transform parent, Vector2 position, Vector2 size, string label)
    {
        Image image = Rect("Button", parent, position, size, PanelLight);
        Button button = image.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        button.colors = colors;
        Text text = MakeText(label, image.transform, Vector2.zero, size - new Vector2(8, 8), 17, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        Stretch(text.rectTransform, 4);
        return button;
    }

    private static void Stretch(RectTransform rt, float inset)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }

    private static string IntentGlyph(KaitIntentType type, Vector2Int direction)
    {
        if (type == KaitIntentType.Melee) return "攻击 !";
        if (type == KaitIntentType.LineShot) return "射线 !";
        if (type != KaitIntentType.Move) return "等待";
        if (direction == Vector2Int.up) return "↑";
        if (direction == Vector2Int.down) return "↓";
        if (direction == Vector2Int.left) return "←";
        if (direction == Vector2Int.right) return "→";
        return "移动";
    }

    private static Color EnemyColor(KaitEnemyType type, KaitEnemyLife life)
    {
        if (life == KaitEnemyLife.Preparing) return Hex("#6D5966");
        if (type == KaitEnemyType.Grunt) return Hex("#B96C72");
        if (type == KaitEnemyType.Guard) return Hex("#875064");
        if (type == KaitEnemyType.Archer) return Hex("#507C83");
        return Hex("#693347");
    }

    private static Color ThreatColor(int value)
    {
        if (value == 0) return Hex("#332D35");
        if (value == 2) return Hex("#F9DED3");
        if (value == 4) return Hex("#FAC7B7");
        if (value == 8) return Hex("#EEA08F");
        if (value == 16) return Hex("#C96D72");
        if (value == 32) return Hex("#95485B");
        return Hex("#652F47");
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color color);
        return color;
    }
}
