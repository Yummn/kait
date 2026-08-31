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
    private Image[] battleDangerBadges;
    private Image[] threatCells;
    private Text[] threatLabels;
    private Text turnText;
    private Text statusText;
    private Text helpText;
    private Text dangerText;
    private Text skillStatusText;
    private readonly Button[] skillButtons = new Button[3];
    private readonly Text[] skillButtonLabels = new Text[3];
    private GameObject skillChoiceOverlay;
    private Text skillChoiceTitle;
    private readonly Button[] skillChoiceButtons = new Button[2];
    private readonly Text[] skillChoiceLabels = new Text[2];
    private KaitSkill targetingSkill;
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
        uiFont = Resources.Load<Font>("NotoSansCJKsc-Regular");
        if (uiFont == null) uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Arial" }, 24);
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

        MakeText("Kait · Dual-Board Strategy Prototype v0.3.7", bg.transform, new Vector2(-320, 406), new Vector2(900, 54), 30, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        MakeText("三阶段技能成长 · 合成128召唤盾骑士", bg.transform, new Vector2(480, 407), new Vector2(520, 42), 17, Peach, TextAnchor.MiddleRight);

        BuildBattleBoard(bg.transform);
        BuildThreatBoard(bg.transform);
        BuildSidebar(bg.transform);
        BuildEndOverlay(bg.transform);
        BuildSkillChoiceOverlay(bg.transform);
    }

    private void BuildBattleBoard(Transform parent)
    {
        Image frame = Rect("Battle Panel", parent, new Vector2(-365, -38), new Vector2(748, 748), Panel);
        MakeText("7 × 7  滑行战场 · 活动区 5 × 5", frame.transform, new Vector2(0, 347), new Vector2(700, 42), 22, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        dangerText = MakeText("", frame.transform, new Vector2(172, 347), new Vector2(350, 42), 17, Gold, TextAnchor.MiddleRight, FontStyle.Bold);
        var gridGo = new GameObject("Battle Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(frame.transform, false);
        RectTransform gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(690, 690);
        gridRect.anchoredPosition = new Vector2(0, -20);
        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(94, 94);
        grid.spacing = new Vector2(5, 5);
        grid.padding = new RectOffset(1, 1, 1, 1);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = KaitRun.BattleSize;

        battleCells = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleLabels = new Text[KaitRun.BattleSize * KaitRun.BattleSize];
        battleDangerBadges = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        for (int visualY = KaitRun.BattleSize - 1; visualY >= 0; visualY--)
        {
            for (int x = 0; x < KaitRun.BattleSize; x++)
            {
                int index = x + visualY * KaitRun.BattleSize;
                Image cell = Rect($"Cell {x},{visualY}", gridGo.transform, Vector2.zero, Vector2.zero, PanelLight);
                Vector2Int targetCell = new Vector2Int(x, visualY);
                cell.gameObject.AddComponent<Button>().onClick.AddListener(() => HandleBattleCellClick(targetCell));
                battleCells[index] = cell;
                battleLabels[index] = MakeText("", cell.transform, Vector2.zero, Vector2.zero, 19, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(battleLabels[index].rectTransform, 3);
                Image dangerBadge = Rect("Rift Danger Badge", cell.transform, new Vector2(31, 31), new Vector2(30, 30), Gold);
                dangerBadge.raycastTarget = false;
                Text dangerBadgeText = MakeText("!", dangerBadge.transform, Vector2.zero, new Vector2(30, 30), 22, Void, TextAnchor.MiddleCenter, FontStyle.Bold);
                dangerBadgeText.raycastTarget = false;
                Stretch(dangerBadgeText.rectTransform, 0);
                dangerBadge.gameObject.SetActive(false);
                battleDangerBadges[index] = dangerBadge;
            }
        }
    }

    private void BuildThreatBoard(Transform parent)
    {
        Image frame = Rect("Threat Panel", parent, new Vector2(477, 164), new Vector2(420, 465), Panel);
        MakeText("5 × 5  精确威胁盘", frame.transform, new Vector2(0, 202), new Vector2(374, 42), 22, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        MakeText("逐格映射 · 每回合新增 1 · 128 召唤 Boss", frame.transform, new Vector2(0, 172), new Vector2(374, 28), 14, Peach, TextAnchor.MiddleLeft);
        var gridGo = new GameObject("Threat Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(frame.transform, false);
        RectTransform rt = gridGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(358, 358);
        rt.anchoredPosition = new Vector2(0, -25);
        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(68, 68);
        grid.spacing = new Vector2(4, 4);
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
                threatLabels[index] = MakeText("", threatCells[index].transform, Vector2.zero, Vector2.zero, 24, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(threatLabels[index].rectTransform, 2);
            }
        }
    }

    private void BuildSidebar(Transform parent)
    {
        Image info = Rect("Run Info", parent, new Vector2(477, -119), new Vector2(420, 82), Panel);
        turnText = MakeText("", info.transform, new Vector2(0, 16), new Vector2(380, 34), 18, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        statusText = MakeText("", info.transform, new Vector2(0, -18), new Vector2(380, 30), 15, Peach, TextAnchor.MiddleLeft);

        Image rules = Rect("Skills", parent, new Vector2(477, -249), new Vector2(420, 156), Panel);
        MakeText("技能 · 16 / 32 / 64 三阶段成长", rules.transform, new Vector2(0, 54), new Vector2(380, 28), 17, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        skillStatusText = MakeText("尚未解锁技能", rules.transform, new Vector2(0, 25), new Vector2(380, 24), 13, Peach, TextAnchor.MiddleLeft);
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int slot = i;
            skillButtons[i] = MakeButton(rules.transform, new Vector2(-126 + i * 126, -28), new Vector2(116, 62), "未解锁");
            skillButtonLabels[i] = skillButtons[i].GetComponentInChildren<Text>();
            skillButtons[i].onClick.AddListener(() => HandleSkillButton(slot));
        }

        Image controls = Rect("Controls", parent, new Vector2(477, -393), new Vector2(420, 114), Panel);
        MakeText("WASD / 方向键", controls.transform, new Vector2(-88, 30), new Vector2(180, 28), 14, Peach, TextAnchor.MiddleLeft);
        MakeButton(controls.transform, new Vector2(-20, 28), new Vector2(48, 42), "W").onClick.AddListener(() => HandleDirection(KaitDirection.Up));
        MakeButton(controls.transform, new Vector2(-72, -20), new Vector2(48, 42), "A").onClick.AddListener(() => HandleDirection(KaitDirection.Left));
        MakeButton(controls.transform, new Vector2(-20, -20), new Vector2(48, 42), "S").onClick.AddListener(() => HandleDirection(KaitDirection.Down));
        MakeButton(controls.transform, new Vector2(32, -20), new Vector2(48, 42), "D").onClick.AddListener(() => HandleDirection(KaitDirection.Right));
        MakeButton(controls.transform, new Vector2(139, 0), new Vector2(112, 72), "重新开始\nR").onClick.AddListener(NewRun);

        helpText = MakeText("", parent, new Vector2(0, -424), new Vector2(760, 35), 15, Peach, TextAnchor.MiddleCenter);
    }

    private void BuildSkillChoiceOverlay(Transform parent)
    {
        Image shade = Rect("Skill Choice Overlay", parent, Vector2.zero, new Vector2(1600, 900), new Color(0.08f, 0.06f, 0.08f, 0.9f));
        Stretch(shade.rectTransform, 0);
        skillChoiceOverlay = shade.gameObject;
        Image card = Rect("Skill Choice Card", shade.transform, Vector2.zero, new Vector2(650, 330), Panel);
        skillChoiceTitle = MakeText("阶段成长", card.transform, new Vector2(0, 108), new Vector2(580, 60), 30, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        for (int i = 0; i < 2; i++)
        {
            int choice = i;
            skillChoiceButtons[i] = MakeButton(card.transform, new Vector2(-155 + i * 310, -20), new Vector2(270, 150), "");
            skillChoiceLabels[i] = skillChoiceButtons[i].GetComponentInChildren<Text>();
            skillChoiceButtons[i].onClick.AddListener(() => ChoosePendingSkill(choice));
        }
        MakeText("选择不消耗回合；每个里程碑只能获得一个技能", card.transform, new Vector2(0, -125), new Vector2(580, 34), 15, Peach, TextAnchor.MiddleCenter);
        skillChoiceOverlay.SetActive(false);
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
        targetingSkill = KaitSkill.None;
        int seed = Environment.TickCount;
        run.Reset(seed);
        logPath = Path.Combine(Application.persistentDataPath, $"kait_run_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllText(logPath, "turn,globalDir,kaitWaited,threatChanged,chainSteps,chainKills,lockedPower,chainMoves,chainEndByStrongEnemy,chainEndByWall,kateStart,kateEnd,kateHp,slideDistance,damage,kills,directKills,nonLethalHits,chainActive,momentum,highestMomentum,longestChain,pushes,friendlyFire,activeWallStops,spawnSuppressed,riftBlocks,wallSuppressedSpawns,internalMerges,internalSpawns,clusterClearCount,threatOrientedWaitCount,emptyMapReachable,emptyMapMaxInputs,activeEnemies,pendingSpawns,highestThreat,threatOccupancy,threatLocks,endReason\n", Encoding.UTF8);
        endOverlay.SetActive(false);
        skillChoiceOverlay.SetActive(false);
        statusText.text = "选择全局方向：凯特与威胁盘分别响应。";
        RefreshAll();
    }

    private void HandleDirection(KaitDirection direction)
    {
        if (busy || run.ended || run.pendingSkillMilestone > 0) return;
        targetingSkill = KaitSkill.None;
        Vector2Int start = run.katePos;
        List<KaitEnemy> enemySnapshot = SnapshotEnemies();
        List<KaitSpawnRequest> spawnSnapshot = SnapshotSpawns();
        KaitTurnResult result = run.chainActive ? run.ContinueChain(direction) : run.TryGlobalInput(direction);
        if (!result.valid)
        {
            statusText.text = result.message;
            StartCoroutine(FlashStatus());
            return;
        }
        if (result.turnComplete) AppendLog(start, result);
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

        if (result.pushed) yield return AnimatePush(result);
        else if (result.pushBlockedByWall || result.pushBlockedByUnit)
            yield return PulseBattleCell(result.pushFrom, Gold, 0.18f);

        if (result.dreadSlash)
        {
            yield return AnimateAllEnemyActions(result.enemyActions.FindAll(a => a.type == KaitIntentType.Move));
            yield return AnimateAllEnemyActions(result.enemyActions.FindAll(a => a.type != KaitIntentType.Move));
        }
        else yield return AnimateAllEnemyActions(result.enemyActions);

        animatedEnemies = null;
        animatedSpawns = null;
        hideKate = false;
        displayKate = null;
        trailCell = null;
        RefreshBattle();

        var spawnPulses = new List<RectTransform>();
        foreach (Vector2Int cell in result.spawnedEnemyCells)
            spawnPulses.Add(battleCells[cell.x + cell.y * KaitRun.BattleSize].rectTransform);
        foreach (KaitSpawnRequest spawn in run.spawns)
            if (spawn.targetCell.x >= 0) spawnPulses.Add(battleCells[spawn.targetCell.x + spawn.targetCell.y * KaitRun.BattleSize].rectTransform);
        if (spawnPulses.Count > 0) yield return ScalePulseMany(spawnPulses, 0.35f, 1.15f, 0.2f);

        displayedThreat = null;
        statusText.text = result.message + (result.merges.Count > 0 ? $" · 威胁合并 ×{result.merges.Count}" : "");
        busy = false;
        RefreshAll();
        if (run.ended) ShowEnd();
    }

    private void RefreshAll()
    {
        RefreshBattle();
        RefreshThreat();
        RefreshSkillUI();
        turnText.text = $"回合 {run.turn}   凯特 HP {run.kateHp}/{run.config.kateMaxHp}   敌人 {run.enemies.FindAll(e => e.life != KaitEnemyLife.Dead).Count}   M{run.momentum}";
        helpText.text = targetingSkill != KaitSkill.None ? "点选战场中的存活敌人作为技能目标" : run.chainActive ? "连杀方向：只影响凯特；不移动威胁盘、不推进裂隙/敌军/新2" : "全局方向：任一盘可响应即推进；凯特贴墙时可用回合整理威胁盘";
        ShowPendingSkillChoice();
        if (run.ended) ShowEnd();
    }

    private void RefreshSkillUI()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            bool unlocked = i < run.skills.Count;
            KaitSkill skill = unlocked ? run.skills[i] : KaitSkill.None;
            int cooldown = unlocked ? run.SkillCooldown(skill) : 0;
            skillButtonLabels[i].text = !unlocked ? "未解锁" : $"{KaitRun.SkillName(skill)}\n{(cooldown > 0 ? $"CD {cooldown}" : skill == KaitSkill.ShadowStep ? "击杀后点按" : "可用")}";
            bool passiveReady = skill == KaitSkill.ShadowStep && run.chainActive && run.shadowStepAvailable;
            skillButtons[i].interactable = unlocked && !busy && run.pendingSkillMilestone == 0 && !run.ended &&
                (skill == KaitSkill.ShadowStep ? passiveReady : !run.chainActive && cooldown == 0);
            skillButtons[i].GetComponent<Image>().color = targetingSkill == skill ? Cyan : PanelLight;
        }
        if (targetingSkill != KaitSkill.None) skillStatusText.text = $"{KaitRun.SkillName(targetingSkill)}：请选择敌人";
        else if (run.skills.Count == 0) skillStatusText.text = "尚未解锁技能";
        else if (run.dreadSlashArmed) skillStatusText.text = "惊惧斩已准备：输入一个方向发动";
        else skillStatusText.text = string.Join(" · ", run.skills.ConvertAll(KaitRun.SkillName));
    }

    private void ShowPendingSkillChoice()
    {
        int milestone = run.pendingSkillMilestone;
        if (milestone == 0 || run.ended) { skillChoiceOverlay.SetActive(false); return; }
        List<KaitSkill> choices = run.SkillChoicesForMilestone(milestone);
        if (choices.Count != 2) return;
        skillChoiceTitle.text = $"合成 {milestone} · 选择一个成长";
        for (int i = 0; i < 2; i++) skillChoiceLabels[i].text = SkillChoiceDescription(choices[i]);
        skillChoiceOverlay.SetActive(true);
        skillChoiceOverlay.transform.SetAsLastSibling();
    }

    private void ChoosePendingSkill(int choiceIndex)
    {
        if (busy || run.pendingSkillMilestone == 0) return;
        List<KaitSkill> choices = run.SkillChoicesForMilestone(run.pendingSkillMilestone);
        if (choiceIndex < 0 || choiceIndex >= choices.Count || !run.ChooseSkill(choices[choiceIndex])) return;
        targetingSkill = KaitSkill.None;
        statusText.text = $"已获得：{KaitRun.SkillName(choices[choiceIndex])}（不消耗回合）";
        RefreshAll();
    }

    private void HandleSkillButton(int slot)
    {
        if (busy || run.ended || slot < 0 || slot >= run.skills.Count) return;
        KaitSkill skill = run.skills[slot];
        if (skill == KaitSkill.ShadowStep)
        {
            Vector2Int start = run.katePos;
            if (run.TryShadowStep()) StartCoroutine(AnimateShadowStep(start));
            else { statusText.text = "踏影当前不可用"; RefreshAll(); }
            return;
        }
        if (skill == KaitSkill.IceTomb || skill == KaitSkill.LesserPhantom)
        {
            targetingSkill = targetingSkill == skill ? KaitSkill.None : skill;
            statusText.text = targetingSkill == KaitSkill.None ? "已取消选择目标" : $"{KaitRun.SkillName(skill)}：请点选一个敌人";
            RefreshAll();
            return;
        }
        if (run.TryUseSkill(skill, -1, out string message)) statusText.text = message;
        else statusText.text = message;
        RefreshAll();
    }

    private void HandleBattleCellClick(Vector2Int cell)
    {
        if (busy || targetingSkill == KaitSkill.None) return;
        KaitEnemy target = run.EnemyAt(cell);
        if (target == null) { statusText.text = "这里没有可选敌人"; return; }
        KaitSkill skill = targetingSkill;
        if (run.TryUseSkill(skill, target.id, out string message)) targetingSkill = KaitSkill.None;
        statusText.text = message;
        RefreshAll();
    }

    private IEnumerator AnimateShadowStep(Vector2Int start)
    {
        busy = true; hideKate = true; RefreshBattle();
        RectTransform token = CreateFloatingToken("凯", Coral, battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, new Vector2(94, 94), 28);
        Vector3 from = battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform.position;
        Vector3 to = battleCells[run.katePos.x + run.katePos.y * KaitRun.BattleSize].rectTransform.position;
        float elapsed = 0f;
        while (elapsed < 0.14f)
        {
            token.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / 0.14f));
            elapsed += Time.unscaledDeltaTime; yield return null;
        }
        Destroy(token.gameObject); hideKate = false; busy = false;
        statusText.text = "踏影：额外前进 1 格，可继续选择转向";
        RefreshAll();
    }

    private static string SkillChoiceDescription(KaitSkill skill)
    {
        switch (skill)
        {
            case KaitSkill.SwiftBoots: return "疾步之靴\n当前回合动量 +1\n冷却 2 回合";
            case KaitSkill.DreadSlash: return "惊惧斩\n凯特不动，普通敌人推至最远\n冷却 4 回合";
            case KaitSkill.IceTomb: return "冰墓\n冻结一个敌人的下一次行动\n冷却 3 回合";
            case KaitSkill.LesserPhantom: return "次级幻影\n敌人下一阶段改攻所选目标\n冷却 4 回合";
            case KaitSkill.CatAgility: return "猫之迅捷\n当前回合动量 ×2\n冷却 5 回合";
            case KaitSkill.ShadowStep: return "踏影（被动）\n击杀后可额外向前 1 格\n不推进任何全局时间";
            default: return "";
        }
    }

    private void RefreshBattle()
    {
        Vector2Int kate = displayKate ?? run.katePos;
        bool kateOnRift = false;
        for (int y = 0; y < KaitRun.BattleSize; y++)
        {
            for (int x = 0; x < KaitRun.BattleSize; x++)
            {
                int index = x + y * KaitRun.BattleSize;
                Vector2Int p = new Vector2Int(x, y);
                Image image = battleCells[index];
                Text label = battleLabels[index];
                label.text = "";
                image.color = run.walls[x, y] ? Void : Hex("#796573");
                if (run.walls[x, y] && x > 0 && y > 0 && x < KaitRun.BattleSize - 1 && y < KaitRun.BattleSize - 1) { label.text = "柱"; label.color = Peach; }
                Color? intentTint = IntentTintAt(p);
                if (!run.walls[x, y] && intentTint.HasValue) image.color = Color.Lerp(image.color, intentTint.Value, 0.55f);
                if (run.chainActive)
                {
                    foreach (KaitDirection choice in run.AllowedTurnDirections())
                        if (run.katePos + KaitRun.Delta(choice) == p) image.color = Cyan;
                }
                if (trailCell.HasValue && trailCell.Value == p) image.color = new Color(Peach.r, Peach.g, Peach.b, 0.45f);
                if (impactCells.Contains(p)) image.color = Gold;
                if (targetingSkill != KaitSkill.None && run.EnemyAt(p) != null) image.color = Color.Lerp(image.color, Cyan, 0.55f);

                KaitSpawnRequest spawn = SpawnAtVisual(p);
                KaitEnemy enemy = EnemyAtVisual(p);
                bool showRiftDanger = !hideKate && kate == p && spawn != null;
                battleDangerBadges[index].gameObject.SetActive(showRiftDanger);
                if (showRiftDanger) kateOnRift = true;
                if (spawn != null)
                {
                    image.color = intentTint.HasValue ? Color.Lerp(Wine, intentTint.Value, 0.55f) : Wine;
                    label.text = $"裂\nT{spawn.tier}";
                    label.color = Cream;
                }
                if (enemy != null)
                {
                    image.color = EnemyColor(enemy.type, enemy.life);
                    string type = EnemyGlyph(enemy.type);
                    string intent = enemy.type == KaitEnemyType.Archer && enemy.archerState == KaitArcherState.Aim
                        ? $"瞄准 {DirectionGlyph(enemy.intent.direction)}"
                        : IntentGlyph(enemy.intent.type, enemy.intent.direction);
                    if (enemy.type == KaitEnemyType.ShieldKnight) intent = $"朝向 {DirectionGlyph(enemy.facing)}";
                    string frozen = enemy.frozenActions > 0 ? " 冻" : "";
                    label.text = enemy.life == KaitEnemyLife.Preparing ? $"{type} {enemy.hp}\n准备{frozen}" : $"{type} HP{enemy.hp}\n{intent}{frozen}";
                    label.color = enemy.life == KaitEnemyLife.Preparing ? Peach : Cream;
                }
                if (!hideKate && kate == p)
                {
                    image.color = showRiftDanger ? Color.Lerp(Coral, Gold, 0.3f) : Coral;
                    label.text = showRiftDanger
                        ? $"凯 HP{run.kateHp}\n裂隙危险"
                        : run.chainActive ? $"凯 HP{run.kateHp}\nM{run.momentum}" : $"凯\nHP{run.kateHp}";
                    label.fontSize = showRiftDanger || run.chainActive ? 18 : 28;
                    label.color = Cream;
                }
                else label.fontSize = 17;
            }
        }
        dangerText.text = kateOnRift ? $"危险：停留在裂隙上，将受到 {run.config.riftBlockDamage} 点伤害" : "";
    }

    private void RefreshThreat()
    {
        for (int y = 0; y < run.ThreatSize; y++)
            for (int x = 0; x < run.ThreatSize; x++)
            {
                int index = x + y * run.ThreatSize;
                Vector2Int cell = new Vector2Int(x, y);
                if (run.IsThreatPillar(cell))
                {
                    threatLabels[index].text = "柱";
                    threatCells[index].color = Void;
                    threatLabels[index].color = Peach;
                    continue;
                }
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
            var intent = new KaitIntent
            {
                type = enemy.intent.type,
                origin = enemy.intent.origin,
                target = enemy.intent.target,
                direction = enemy.intent.direction,
                damage = enemy.intent.damage
            };
            intent.affectedCells.AddRange(enemy.intent.affectedCells);
            snapshot.Add(new KaitEnemy
            {
                id = enemy.id,
                type = enemy.type,
                pos = enemy.pos,
                hp = enemy.hp,
                maxHp = enemy.maxHp,
                life = enemy.life,
                archerState = enemy.archerState,
                frozenActions = enemy.frozenActions,
                facing = enemy.facing,
                intent = intent
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
                createdTurn = spawn.createdTurn,
                state = spawn.state
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
        RectTransform token = CreateFloatingToken("凯", Coral, battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, new Vector2(94, 94), 28);
        var points = new List<Vector3> { battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform.position };
        foreach (Vector2Int cell in result.katePath) points.Add(battleCells[cell.x + cell.y * KaitRun.BattleSize].rectTransform.position);

        int segments = points.Count - 1;
        float duration = Mathf.Min(0.36f, 0.16f + segments * 0.025f);
        float elapsed = 0f;
        int lastReached = 0;
        bool killAudioPlayed = false;
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
                if (animatedEnemies.Exists(e => e.pos == cell && result.playerKilledEnemyIds.Contains(e.id)))
                {
                    animatedEnemies.RemoveAll(e => result.playerKilledEnemyIds.Contains(e.id));
                    impactCells.Add(cell);
                    if (!killAudioPlayed)
                    {
                        GameAudio.PlayKaitKill(Mathf.Max(1, run.currentChainKills));
                        killAudioPlayed = true;
                    }
                    RefreshBattle();
                }
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        token.position = points[points.Count - 1];
        Destroy(token.gameObject);
        animatedEnemies.RemoveAll(e => result.playerKilledEnemyIds.Contains(e.id));
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
                string type = EnemyGlyph(enemy.type);
                RectTransform token = CreateFloatingToken($"{type} {enemy.hp}", EnemyColor(enemy.type, enemy.life), battleCells[action.from.x + action.from.y * KaitRun.BattleSize].rectTransform, new Vector2(94, 94), 17);
                moves.Add(new EnemyMoveVisual
                {
                    enemy = enemy,
                    rect = token,
                    from = battleCells[action.from.x + action.from.y * KaitRun.BattleSize].rectTransform.position,
                    to = battleCells[action.to.x + action.to.y * KaitRun.BattleSize].rectTransform.position
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
        animatedEnemies?.RemoveAll(e => run.enemies.Find(r => r.id == e.id)?.life == KaitEnemyLife.Dead);
        impactCells.Clear();
        RefreshBattle();
    }

    private IEnumerator AnimatePush(KaitTurnResult result)
    {
        KaitEnemy enemy = animatedEnemies?.Find(e => e.pos == result.pushFrom && e.life != KaitEnemyLife.Dead);
        if (enemy == null || !InsideBattle(result.pushTo)) yield break;
        KaitEnemy resolved = run.enemies.Find(e => e.id == enemy.id);
        if (resolved != null) enemy.hp = resolved.hp;
        string type = EnemyGlyph(enemy.type);
        RectTransform token = CreateFloatingToken($"{type} {enemy.hp}", EnemyColor(enemy.type, enemy.life), battleCells[result.pushFrom.x + result.pushFrom.y * KaitRun.BattleSize].rectTransform, new Vector2(94, 94), 17);
        animatedEnemies.Remove(enemy);
        RefreshBattle();
        Vector3 from = battleCells[result.pushFrom.x + result.pushFrom.y * KaitRun.BattleSize].rectTransform.position;
        Vector3 to = battleCells[result.pushTo.x + result.pushTo.y * KaitRun.BattleSize].rectTransform.position;
        float elapsed = 0f;
        const float duration = 0.18f;
        while (elapsed < duration)
        {
            token.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        enemy.pos = result.pushTo;
        animatedEnemies.Add(enemy);
        Destroy(token.gameObject);
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
            if (enemy.intent.type == KaitIntentType.Melee && enemy.intent.target == p)
                return Hex("#A64F5A");
            if (enemy.intent.type == KaitIntentType.LineShot && enemy.intent.affectedCells.Contains(p)) return Hex("#944A58");
        }
        return null;
    }

    private bool BlocksIntentLine(Vector2Int p, List<KaitEnemy> source)
    {
        if (run.walls[p.x, p.y]) return true;
        return source.Exists(e => e.life != KaitEnemyLife.Dead && e.pos == p);
    }

    private static bool InsideBattle(Vector2Int p) => p.x >= 0 && p.x < KaitRun.BattleSize && p.y >= 0 && p.y < KaitRun.BattleSize;

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
        Image image = battleCells[cell.x + cell.y * KaitRun.BattleSize];
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
        endOverlay.transform.SetAsLastSibling();
        string reason = run.won ? "击败盾骑士 · 本局胜利" : "凯特 HP 归零 · 本局失败";
        endText.text = $"{reason}\n\n回合：{run.turn}    击杀：{run.kills}    推动：{run.pushCount}\n最高动量：{run.highestMomentum}    主动刹车：{run.activeWallStops}\n刷怪抑制：{run.spawnSuppressedCount}    友伤：{run.friendlyFireDamage}";
    }

    private void AppendLog(Vector2Int start, KaitTurnResult result)
    {
        int occupied = 0;
        foreach (int value in run.threat) if (value != 0) occupied++;
        float occupancy = occupied / (float)run.threat.Length;
        string line = $"{run.turn},{result.globalDirection},{result.kaitWaited},{result.threatChanged},{result.chainStepCount},{result.chainKillCount},{result.chainPower},{result.chainMoves},{result.chainEndedByStrongEnemy},{result.chainEndedByWall},{start.x}:{start.y},{run.katePos.x}:{run.katePos.y},{run.kateHp},{result.slideDistance},{result.damageDealt},{run.kills},{run.directKills},{run.nonLethalHits},{run.chainActive},{run.momentum},{run.highestMomentum},{run.longestChainKills},{run.pushCount},{run.friendlyFireDamage},{run.activeWallStops},{run.spawnSuppressedCount},{run.riftBlocks},{run.wallSuppressedSpawns},{run.internalMergeCount},{run.internalSpawnCount},{run.clusterClearCount},{run.threatOrientedWaitCount},{run.emptyMapReachable},{run.emptyMapMaxInputs},{run.enemies.FindAll(e => e.life != KaitEnemyLife.Dead).Count},{run.spawns.Count},{run.highestThreat},{occupancy:F3},{run.threatLocks},{run.endReason}\n";
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

    private static string DirectionGlyph(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return "↑";
        if (direction == Vector2Int.down) return "↓";
        if (direction == Vector2Int.left) return "←";
        if (direction == Vector2Int.right) return "→";
        return "·";
    }

    private static string EnemyGlyph(KaitEnemyType type)
    {
        if (type == KaitEnemyType.Grunt) return "兵";
        if (type == KaitEnemyType.Swordsman) return "剑";
        if (type == KaitEnemyType.Archer) return "弓";
        if (type == KaitEnemyType.Guard) return "盾";
        if (type == KaitEnemyType.ShieldKnight) return "骑";
        return "精";
    }

    private static Color EnemyColor(KaitEnemyType type, KaitEnemyLife life)
    {
        if (life == KaitEnemyLife.Preparing) return Hex("#6D5966");
        if (type == KaitEnemyType.Grunt) return Hex("#B96C72");
        if (type == KaitEnemyType.Swordsman) return Hex("#A85B68");
        if (type == KaitEnemyType.Guard) return Hex("#875064");
        if (type == KaitEnemyType.Archer) return Hex("#507C83");
        if (type == KaitEnemyType.ShieldKnight) return Hex("#3D6070");
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
