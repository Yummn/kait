using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Spine.Unity;

public sealed class KaitGame : MonoBehaviour
{
    private static KaitGame instance;
    private readonly KaitRun run = new KaitRun();
    private Font uiFont;
    private Canvas canvas;
    private Sprite roundedSprite;
    private Image[] battleCells;
    private Text[] battleLabels;
    private Image[] battlePortraits;
    private Text[] battleHpLabels;
    private Text[] battleFacingLabels;
    private Text[] battleStatusLabels;
    private Image[] battleWarningLines;
    private Image[] battleRifts;
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
    private GameObject controlsPanel;
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
    private Image edgePulse;
    private GameObject tutorialOverlay;
    private Sprite kaitPortrait;
    private SkeletonDataAsset makotoSkeletonData;
    private KaitSpineView kaitSpine;
    private Sprite gruntPortrait;
    private Sprite swordsmanPortrait;
    private Sprite archerPortrait;
    private Sprite guardPortrait;
    private Sprite elitePortrait;
    private Sprite bossPortrait;
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
        kaitPortrait = LoadPixelSprite("KenneyTinyDungeon/Kait");
        makotoSkeletonData = Resources.Load<SkeletonDataAsset>("Characters/Makoto/Makoto_SkeletonData");
        gruntPortrait = LoadPixelSprite("KenneyTinyDungeon/Grunt");
        swordsmanPortrait = LoadPixelSprite("KenneyTinyDungeon/Swordsman");
        archerPortrait = LoadPixelSprite("KenneyTinyDungeon/Archer");
        guardPortrait = LoadPixelSprite("KenneyTinyDungeon/Guard");
        elitePortrait = LoadPixelSprite("KenneyTinyDungeon/Elite");
        bossPortrait = LoadPixelSprite("KenneyTinyDungeon/ShieldKnight");
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
        if (roundedSprite == null)
        {
            foreach (Image source in FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (source.sprite != null && source.sprite.name == "Tile_0") { roundedSprite = source.sprite; break; }
        }
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
        if (CommandLineValue("-kaitTutorial") == "1") tutorialOverlay.SetActive(true);
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

        MakeText("Kait", bg.transform, new Vector2(-700, 410), new Vector2(150, 42), 28, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        MakeButton(bg.transform, new Vector2(748, 410), new Vector2(44, 44), "?").onClick.AddListener(() => tutorialOverlay.SetActive(true));

        BuildBattleBoard(bg.transform);
        BuildThreatBoard(bg.transform);
        BuildSidebar(bg.transform);
        BuildEndOverlay(bg.transform);
        BuildSkillChoiceOverlay(bg.transform);
        BuildTutorialOverlay(bg.transform);
        edgePulse = Rect("Chain Edge Pulse", bg.transform, Vector2.zero, new Vector2(1570, 870), new Color(Coral.r, Coral.g, Coral.b, 0f));
        Stretch(edgePulse.rectTransform, 15);
        edgePulse.raycastTarget = false;
        edgePulse.transform.SetAsLastSibling();
    }

    private void BuildBattleBoard(Transform parent)
    {
        Image frame = Rect("Battle Panel", parent, new Vector2(-365, -28), new Vector2(640, 640), Panel);
        dangerText = MakeText("", frame.transform, Vector2.zero, Vector2.zero, 1, Color.clear, TextAnchor.MiddleCenter);
        dangerText.gameObject.SetActive(false);
        var gridGo = new GameObject("Battle Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(frame.transform, false);
        RectTransform gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(600, 600);
        gridRect.anchoredPosition = Vector2.zero;
        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(115, 115);
        grid.spacing = new Vector2(6, 6);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        battleCells = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleLabels = new Text[KaitRun.BattleSize * KaitRun.BattleSize];
        battlePortraits = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleHpLabels = new Text[KaitRun.BattleSize * KaitRun.BattleSize];
        battleFacingLabels = new Text[KaitRun.BattleSize * KaitRun.BattleSize];
        battleStatusLabels = new Text[KaitRun.BattleSize * KaitRun.BattleSize];
        battleWarningLines = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleRifts = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleDangerBadges = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        for (int visualY = KaitRun.BattleSize - 2; visualY >= 1; visualY--)
        {
            for (int x = 1; x < KaitRun.BattleSize - 1; x++)
            {
                int index = x + visualY * KaitRun.BattleSize;
                Image cell = Rect($"Cell {x},{visualY}", gridGo.transform, Vector2.zero, Vector2.zero, PanelLight);
                Mask cellMask = cell.gameObject.AddComponent<Mask>();
                cellMask.showMaskGraphic = true;
                Vector2Int targetCell = new Vector2Int(x, visualY);
                cell.gameObject.AddComponent<Button>().onClick.AddListener(() => HandleBattleCellClick(targetCell));
                battleCells[index] = cell;
                Image warning = Rect("Attack Warning", cell.transform, Vector2.zero, new Vector2(98, 14), new Color(Wine.r, Wine.g, Wine.b, 0.38f));
                warning.raycastTarget = false;
                warning.color = Color.clear;
                for (int dash = -1; dash <= 1; dash++)
                {
                    Image segment = Rect("Warning Dash", warning.transform, new Vector2(dash * 31, 0), new Vector2(22, 9), new Color(Coral.r, 0.18f, 0.22f, 0.72f));
                    segment.raycastTarget = false;
                }
                warning.gameObject.SetActive(false);
                battleWarningLines[index] = warning;
                Image rift = Rect("Spawn Rift", cell.transform, Vector2.zero, new Vector2(82, 82), Color.clear);
                rift.raycastTarget = false;
                for (int crack = 0; crack < 3; crack++)
                {
                    Image line = Rect("Rift Crack", rift.transform, new Vector2((crack - 1) * 12, -7 + crack * 5), new Vector2(54 - crack * 9, 7), new Color(Gold.r, Coral.g, Coral.b, 0.68f));
                    line.raycastTarget = false;
                    line.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -48f + crack * 43f);
                }
                Text spawnArrow = MakeText("⌃", rift.transform, new Vector2(0, 28), new Vector2(40, 32), 28, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
                spawnArrow.raycastTarget = false;
                rift.gameObject.SetActive(false);
                battleRifts[index] = rift;
                Image portrait = Rect("Unit Portrait", cell.transform, new Vector2(0, -3), new Vector2(78, 78), Color.white);
                portrait.sprite = null;
                portrait.type = Image.Type.Simple;
                portrait.preserveAspect = true;
                portrait.raycastTarget = false;
                portrait.gameObject.SetActive(false);
                battlePortraits[index] = portrait;
                battleLabels[index] = MakeText("", cell.transform, Vector2.zero, Vector2.zero, 34, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(battleLabels[index].rectTransform, 3);
                battleFacingLabels[index] = MakeText("", cell.transform, new Vector2(0, -43), new Vector2(72, 28), 25, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                battleHpLabels[index] = MakeText("", cell.transform, new Vector2(43, 42), new Vector2(28, 26), 18, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                battleStatusLabels[index] = MakeText("", cell.transform, new Vector2(-43, 42), new Vector2(28, 26), 19, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
                Image dangerBadge = Rect("Rift Danger Badge", cell.transform, new Vector2(42, 42), new Vector2(28, 28), Gold);
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
        Image frame = Rect("Threat Panel", parent, new Vector2(457, 204), new Vector2(390, 390), Panel);
        var gridGo = new GameObject("Threat Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(frame.transform, false);
        RectTransform rt = gridGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(360, 360);
        rt.anchoredPosition = Vector2.zero;
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
        Image info = Rect("Run Info", parent, new Vector2(457, -28), new Vector2(390, 62), Panel);
        turnText = MakeText("", info.transform, Vector2.zero, new Vector2(350, 34), 18, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        statusText = MakeText("", parent, Vector2.zero, Vector2.zero, 1, Color.clear, TextAnchor.MiddleCenter);
        statusText.gameObject.SetActive(false);

        Image rules = Rect("Skills", parent, new Vector2(457, -155), new Vector2(390, 174), Panel);
        MakeText("技能栏", rules.transform, new Vector2(0, 62), new Vector2(380, 28), 17, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        skillStatusText = MakeText("尚未解锁技能", rules.transform, new Vector2(0, 34), new Vector2(380, 24), 13, Peach, TextAnchor.MiddleLeft);
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int slot = i;
            skillButtons[i] = MakeButton(rules.transform, new Vector2(-126 + i * 126, -25), new Vector2(116, 74), "未解锁");
            skillButtonLabels[i] = skillButtons[i].GetComponentInChildren<Text>();
            skillButtons[i].onClick.AddListener(() => HandleSkillButton(slot));
        }

        Image controls = Rect("Controls", parent, new Vector2(457, -350), new Vector2(390, 146), Panel);
        controlsPanel = controls.gameObject;
        MakeButton(controls.transform, new Vector2(-20, 35), new Vector2(54, 46), "W").onClick.AddListener(() => HandleDirection(KaitDirection.Up));
        MakeButton(controls.transform, new Vector2(-78, -18), new Vector2(54, 46), "A").onClick.AddListener(() => HandleDirection(KaitDirection.Left));
        MakeButton(controls.transform, new Vector2(-20, -18), new Vector2(54, 46), "S").onClick.AddListener(() => HandleDirection(KaitDirection.Down));
        MakeButton(controls.transform, new Vector2(38, -18), new Vector2(54, 46), "D").onClick.AddListener(() => HandleDirection(KaitDirection.Right));
        MakeButton(controls.transform, new Vector2(127, 7), new Vector2(92, 76), "重开\nR").onClick.AddListener(NewRun);

        helpText = MakeText("", parent, Vector2.zero, Vector2.zero, 1, Color.clear, TextAnchor.MiddleCenter);
        helpText.gameObject.SetActive(false);
    }

    private void BuildSkillChoiceOverlay(Transform parent)
    {
        Image card = Rect("Skill Choice Side Panel", parent, new Vector2(457, -350), new Vector2(390, 146), Panel);
        skillChoiceOverlay = card.gameObject;
        skillChoiceTitle = MakeText("选择成长", card.transform, new Vector2(0, 53), new Vector2(350, 24), 15, Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
        for (int i = 0; i < 2; i++)
        {
            int choice = i;
            skillChoiceButtons[i] = MakeButton(card.transform, new Vector2(-91 + i * 182, -14), new Vector2(172, 92), "");
            skillChoiceLabels[i] = skillChoiceButtons[i].GetComponentInChildren<Text>();
            skillChoiceLabels[i].fontSize = 12;
            skillChoiceButtons[i].onClick.AddListener(() => ChoosePendingSkill(choice));
        }
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

    private void BuildTutorialOverlay(Transform parent)
    {
        Image shade = Rect("Tutorial Overlay", parent, Vector2.zero, new Vector2(1600, 900), new Color(0.08f, 0.06f, 0.08f, 0.9f));
        Stretch(shade.rectTransform, 0);
        tutorialOverlay = shade.gameObject;
        Image card = Rect("Tutorial Card", shade.transform, Vector2.zero, new Vector2(940, 680), Panel);
        MakeText("玩法教程", card.transform, new Vector2(-390, 295), new Vector2(220, 52), 30, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        Text body = MakeText(
            "双盘联动\n每次输入方向，战场与右侧数字盘同步响应。数字盘按 2048 规则移动与合并；合成会在战场对应位置留下出生裂隙。\n\n" +
            "战场与边界\n画面显示 5×5 活动区域。凯特会沿方向滑行；抵达画面边界等同撞墙停止，逻辑仍保留原 7×7 外圈边界。\n\n" +
            "单位信息\n头像表示单位类型，右上角数字是生命。单位背景颜色对应其来源数字：例如由 2+2 合成产生的单位使用数字 4 的底色。下方半箭头表示朝向。\n\n" +
            "预警\n红色虚线表示敌人下一次攻击路径；裂纹与向上标记表示敌人即将从该格生成。预警位于头像下方，不遮挡生命。\n\n" +
            "连锁与技能\n击杀后用半箭头选择下一方向。合成 16 / 32 / 64 时，右侧出现成长二选一；不选择也可以继续行动，选择本身不消耗回合。\n\n" +
            "操作\nWASD 或方向键：移动　　R：重新开始　　鼠标：技能、目标与成长选择",
            card.transform, new Vector2(0, -25), new Vector2(820, 580), 15, Peach, TextAnchor.UpperLeft);
        body.lineSpacing = 1.04f;
        MakeButton(card.transform, new Vector2(402, 298), new Vector2(54, 44), "×").onClick.AddListener(() => tutorialOverlay.SetActive(false));
        tutorialOverlay.SetActive(false);
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
        EnsureKaitSpine();
        kaitSpine?.PlayLoop(KaitSpineView.Idle);
        logPath = Path.Combine(Application.persistentDataPath, $"kait_run_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllText(logPath, "turn,globalDir,kaitWaited,threatChanged,chainSteps,chainKills,lockedPower,chainMoves,chainEndByStrongEnemy,chainEndByWall,kateStart,kateEnd,kateHp,slideDistance,damage,kills,directKills,nonLethalHits,chainActive,momentum,highestMomentum,longestChain,pushes,friendlyFire,activeWallStops,spawnSuppressed,riftBlocks,wallSuppressedSpawns,internalMerges,internalSpawns,clusterClearCount,threatOrientedWaitCount,emptyMapReachable,emptyMapMaxInputs,activeEnemies,pendingSpawns,highestThreat,threatOccupancy,threatLocks,endReason\n", Encoding.UTF8);
        endOverlay.SetActive(false);
        skillChoiceOverlay.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
        statusText.text = "选择全局方向：凯特与威胁盘分别响应。";
        RefreshAll();
    }

    private void HandleDirection(KaitDirection direction)
    {
        if (busy || run.ended) return;
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
            yield return AnimateDreadSlashWave(result.globalDirection);
            yield return AnimateAllEnemyActions(result.enemyActions.FindAll(a => a.type == KaitIntentType.Move));
            yield return AnimateAllEnemyActions(result.enemyActions.FindAll(a => a.type != KaitIntentType.Move));
        }
        else yield return AnimateAllEnemyActions(result.enemyActions);

        yield return AnimateCombatFeedback(result);

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

        if (result.playerKilledEnemyIds.Count > 0) StartCoroutine(AnimateChainEdge(Mathf.Max(1, result.chainKillCount)));

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
        turnText.text = $"回合 {run.turn}　生命 {run.kateHp}/{run.config.kateMaxHp}　速度 {run.momentum}";
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
            skillButtons[i].interactable = unlocked && !busy && !run.ended &&
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
        if (milestone == 0 || run.ended)
        {
            skillChoiceOverlay.SetActive(false);
            if (controlsPanel != null) controlsPanel.SetActive(!run.ended);
            return;
        }
        List<KaitSkill> choices = run.SkillChoicesForMilestone(milestone);
        if (choices.Count != 2) return;
        skillChoiceTitle.text = $"合成 {milestone} · 选择一个成长";
        for (int i = 0; i < 2; i++) skillChoiceLabels[i].text = SkillChoiceDescription(choices[i]);
        skillChoiceOverlay.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
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
        if (run.TryUseSkill(skill, -1, out string message))
        {
            statusText.text = message;
            StartCoroutine(AnimateSkillPulse(skill));
        }
        else statusText.text = message;
        RefreshAll();
    }

    private void HandleBattleCellClick(Vector2Int cell)
    {
        if (busy || targetingSkill == KaitSkill.None) return;
        KaitEnemy target = run.EnemyAt(cell);
        if (target == null) { statusText.text = "这里没有可选敌人"; return; }
        KaitSkill skill = targetingSkill;
        if (run.TryUseSkill(skill, target.id, out string message))
        {
            targetingSkill = KaitSkill.None;
            kaitSpine?.PlayOnce(KaitSpineView.OtherSkill);
            StartCoroutine(PulseBattleCell(cell, skill == KaitSkill.IceTomb ? Cyan : Coral, 0.2f));
        }
        statusText.text = message;
        RefreshAll();
    }

    private IEnumerator AnimateShadowStep(Vector2Int start)
    {
        busy = true; hideKate = true; RefreshBattle();
        KaitSpineView movingKait = CreateFloatingKait(battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, run.currentDirection);
        RectTransform token = movingKait != null ? movingKait.Root : CreateFloatingPortrait(kaitPortrait, Color.clear, battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, new Vector2(115, 115));
        movingKait?.PlayOnce(KaitSpineView.ShadowStep, null);
        Vector3 from = battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform.position;
        Vector3 to = battleCells[run.katePos.x + run.katePos.y * KaitRun.BattleSize].rectTransform.position;
        float elapsed = 0f;
        float duration = Mathf.Max(0.14f, movingKait?.Duration(KaitSpineView.ShadowStep) ?? 0.14f);
        while (elapsed < duration)
        {
            token.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            elapsed += Time.unscaledDeltaTime; yield return null;
        }
        if (movingKait != null) movingKait.Destroy(); else Destroy(token.gameObject);
        hideKate = false; busy = false;
        kaitSpine?.PlayLoop(KaitSpineView.Idle);
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
        EnsureKaitSpine();
        kaitSpine?.SetVisible(false);
        Vector2Int kate = displayKate ?? run.katePos;
        bool kateOnRift = false;
        List<KaitDirection> allowed = run.chainActive ? run.AllowedTurnDirections() : new List<KaitDirection>();
        for (int y = 1; y < KaitRun.BattleSize - 1; y++)
        {
            for (int x = 1; x < KaitRun.BattleSize - 1; x++)
            {
                int index = x + y * KaitRun.BattleSize;
                Vector2Int p = new Vector2Int(x, y);
                Image image = battleCells[index];
                Text label = battleLabels[index];
                label.text = "";
                label.rectTransform.localRotation = Quaternion.identity;
                battleHpLabels[index].text = "";
                battleFacingLabels[index].text = "";
                battleFacingLabels[index].rectTransform.localRotation = Quaternion.identity;
                battleStatusLabels[index].text = "";
                battlePortraits[index].gameObject.SetActive(false);
                battlePortraits[index].color = Color.white;
                battleWarningLines[index].gameObject.SetActive(false);
                battleRifts[index].gameObject.SetActive(false);
                image.color = ThreatColor(0);
                if (run.walls[x, y])
                {
                    label.text = "◆";
                    label.color = Peach;
                    label.fontSize = 30;
                }
                Color? intentTint = IntentTintAt(p);
                if (!run.walls[x, y] && intentTint.HasValue)
                {
                    Image warning = battleWarningLines[index];
                    warning.gameObject.SetActive(true);
                    warning.color = Color.clear;
                    foreach (Image dash in warning.GetComponentsInChildren<Image>(true))
                        if (dash != warning) dash.color = new Color(intentTint.Value.r, intentTint.Value.g, intentTint.Value.b, 0.72f);
                    warning.rectTransform.localRotation = Quaternion.Euler(0f, 0f, IntentAngleAt(p));
                }
                if (run.chainActive)
                    foreach (KaitDirection choice in allowed)
                        if (run.katePos + KaitRun.Delta(choice) == p)
                        {
                            label.text = ">";
                            label.rectTransform.localRotation = Quaternion.Euler(0f, 0f, HalfArrowAngle(KaitRun.Delta(choice)));
                            label.fontSize = run.shadowStepAvailable ? 30 : 38;
                            label.color = run.shadowStepAvailable ? Gold : Cyan;
                        }
                if (impactCells.Contains(p)) image.color = Gold;
                if (targetingSkill != KaitSkill.None && run.EnemyAt(p) != null) image.color = Color.Lerp(image.color, Cyan, 0.35f);

                KaitSpawnRequest spawn = SpawnAtVisual(p);
                KaitEnemy enemy = EnemyAtVisual(p);
                bool showRiftDanger = !hideKate && kate == p && spawn != null;
                battleDangerBadges[index].gameObject.SetActive(showRiftDanger);
                if (showRiftDanger) kateOnRift = true;
                if (spawn != null)
                {
                    Image rift = battleRifts[index];
                    rift.gameObject.SetActive(true);
                    battleStatusLabels[index].text = spawn.turnsUntilSpawn > 0 ? spawn.turnsUntilSpawn.ToString() : "!";
                    battleStatusLabels[index].color = Gold;
                }
                if (enemy != null)
                {
                    image.color = EnemyTileColor(enemy.type);
                    if (enemy.life == KaitEnemyLife.Preparing) image.color = Color.Lerp(image.color, Panel, 0.28f);
                    battlePortraits[index].sprite = EnemyPortrait(enemy.type);
                    battlePortraits[index].gameObject.SetActive(true);
                    battlePortraits[index].color = enemy.frozenActions > 0 ? new Color(0.62f, 0.9f, 1f, 1f) : enemy.life == KaitEnemyLife.Preparing ? new Color(1f, 1f, 1f, 0.68f) : Color.white;
                    battleHpLabels[index].text = enemy.hp.ToString();
                    battleHpLabels[index].color = image.color.grayscale > 0.62f ? Void : Cream;
                    Vector2Int facing = enemy.type == KaitEnemyType.ShieldKnight ? enemy.facing : enemy.intent.direction;
                    if (facing != Vector2Int.zero) battleFacingLabels[index].text = ">";
                    battleFacingLabels[index].rectTransform.localRotation = Quaternion.Euler(0f, 0f, HalfArrowAngle(facing));
                    battleFacingLabels[index].color = enemy.life == KaitEnemyLife.Preparing ? Peach : Cream;
                    if (enemy.frozenActions > 0) { battleStatusLabels[index].text = "❄"; battleStatusLabels[index].color = Cyan; }
                    if (run.forcedTargetEnemyId == enemy.id) { battleStatusLabels[index].text = "!"; battleStatusLabels[index].color = Coral; }
                }
                if (!hideKate && kate == p)
                {
                    image.color = showRiftDanger ? Color.Lerp(ThreatColor(2), Gold, 0.32f) : ThreatColor(2);
                    if (kaitSpine != null)
                    {
                        kaitSpine.SetParent(image.transform, 3);
                        kaitSpine.SetVisible(true);
                    }
                    else
                    {
                        battlePortraits[index].sprite = kaitPortrait;
                        battlePortraits[index].gameObject.SetActive(true);
                    }
                    battleHpLabels[index].text = run.kateHp.ToString();
                    battleHpLabels[index].color = Void;
                    if (run.chainActive) battleFacingLabels[index].text = ">";
                    battleFacingLabels[index].rectTransform.localRotation = Quaternion.Euler(0f, 0f, HalfArrowAngle(KaitRun.Delta(run.currentDirection)));
                    battleFacingLabels[index].color = Void;
                    if (showRiftDanger) { battleStatusLabels[index].text = "!"; battleStatusLabels[index].color = Gold; }
                }
            }
        }
        dangerText.text = kateOnRift ? "危险格 !" : "";
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
            {
                kaitSpine?.Face(result.globalDirection);
                kaitSpine?.PlayOnce(KaitSpineView.Attack);
                yield return PulseBattleCell(result.blockedEnemyCell, Coral, 0.16f);
            }
            else if (result.chainEndedByWall || result.activeBrake || result.pushBlockedByWall)
                kaitSpine?.PlayOnce(KaitSpineView.StandBy);
            yield break;
        }

        hideKate = true;
        kaitSpine?.Face(result.globalDirection);
        RefreshBattle();
        KaitSpineView movingKait = CreateFloatingKait(battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, result.globalDirection);
        RectTransform token = movingKait != null ? movingKait.Root : CreateFloatingPortrait(kaitPortrait, Color.clear, battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, new Vector2(115, 115));
        movingKait?.PlayLoop(KaitSpineView.Run);
        var ghosts = new List<RectTransform>();
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
                int momentumAtCell = result.pathMomentum.Count >= lastReached ? result.pathMomentum[lastReached - 1] : run.momentum;
                RectTransform ghost = CreateGhostToken(battleCells[cell.x + cell.y * KaitRun.BattleSize].rectTransform, momentumAtCell);
                ghosts.Add(ghost);
                while (ghosts.Count > Mathf.Clamp(momentumAtCell, 1, 5))
                {
                    Destroy(ghosts[0].gameObject);
                    ghosts.RemoveAt(0);
                }
                if (animatedEnemies.Exists(e => e.pos == cell && result.playerKilledEnemyIds.Contains(e.id)))
                {
                    animatedEnemies.RemoveAll(e => result.playerKilledEnemyIds.Contains(e.id));
                    impactCells.Add(cell);
                    if (!killAudioPlayed)
                    {
                        GameAudio.PlayKaitKill(Mathf.Max(1, run.currentChainKills));
                        killAudioPlayed = true;
                    }
                    movingKait?.PlayOnce(result.chainKillCount > 1 ? KaitSpineView.ChainAttack : KaitSpineView.Attack, KaitSpineView.Run);
                    RefreshBattle();
                }
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        token.position = points[points.Count - 1];
        if (movingKait != null) movingKait.Destroy(); else Destroy(token.gameObject);
        foreach (RectTransform ghost in ghosts) StartCoroutine(FadeAndDestroy(ghost, 0.22f));
        animatedEnemies.RemoveAll(e => result.playerKilledEnemyIds.Contains(e.id));
        impactCells.Clear();
        hideKate = false;
        displayKate = null;
        RefreshBattle();
        if (result.blockedEnemyCell.x >= 0)
        {
            kaitSpine?.Face(result.globalDirection);
            kaitSpine?.PlayOnce(result.chainKillCount > 1 ? KaitSpineView.ChainAttack : KaitSpineView.Attack);
            yield return PulseBattleCell(result.blockedEnemyCell, Coral, 0.14f);
        }
        else if (result.chainEndedByWall || result.activeBrake || result.pushBlockedByWall)
            kaitSpine?.PlayOnce(KaitSpineView.StandBy);
        else if (result.playerKilledEnemyIds.Count > 0)
            kaitSpine?.PlayOnce(result.chainKillCount > 1 ? KaitSpineView.ChainAttack : KaitSpineView.Attack);
        else
            kaitSpine?.PlayLoop(KaitSpineView.Idle);
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
                RectTransform token = CreateFloatingPortrait(EnemyPortrait(enemy.type), EnemyTileColor(enemy.type), battleCells[action.from.x + action.from.y * KaitRun.BattleSize].rectTransform, new Vector2(115, 115), enemy.hp);
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
        RectTransform token = CreateFloatingPortrait(EnemyPortrait(enemy.type), EnemyTileColor(enemy.type), battleCells[result.pushFrom.x + result.pushFrom.y * KaitRun.BattleSize].rectTransform, new Vector2(115, 115), enemy.hp);
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

    private float IntentAngleAt(Vector2Int p)
    {
        List<KaitEnemy> source = animatedEnemies ?? run.enemies;
        foreach (KaitEnemy enemy in source)
        {
            if (enemy.life != KaitEnemyLife.Active || !enemy.intent.affectedCells.Contains(p)) continue;
            Vector2Int direction = enemy.intent.direction;
            if (direction == Vector2Int.up || direction == Vector2Int.down) return 90f;
            if (direction == Vector2Int.left || direction == Vector2Int.right) return 0f;
        }
        return 45f;
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

    private RectTransform CreateFloatingPortrait(Sprite portraitSprite, Color background, RectTransform source, Vector2 size, int hp = -1)
    {
        Image image = Rect("Animation Unit", canvas.transform, Vector2.zero, size, background);
        image.rectTransform.position = source.position;
        image.rectTransform.SetAsLastSibling();
        Image portrait = Rect("Portrait", image.transform, new Vector2(0, -3), size * 0.7f, Color.white);
        portrait.sprite = portraitSprite;
        portrait.type = Image.Type.Simple;
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        if (hp >= 0)
        {
            Text hpText = MakeText(hp.ToString(), image.transform, new Vector2(size.x * 0.36f, size.y * 0.36f), new Vector2(28, 25), 17, background.grayscale > 0.62f ? Void : Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            hpText.raycastTarget = false;
        }
        return image.rectTransform;
    }

    private KaitSpineView CreateFloatingKait(RectTransform source, KaitDirection direction)
    {
        if (makotoSkeletonData == null) return null;
        KaitSpineView view = KaitSpineView.Create(makotoSkeletonData, canvas.transform, new Vector2(115, 115), "Kait Spine Animation");
        if (view == null) return null;
        view.Root.position = source.position;
        view.Root.SetAsLastSibling();
        view.Face(direction);
        return view;
    }

    private void EnsureKaitSpine()
    {
        if (kaitSpine != null || makotoSkeletonData == null || battleCells == null) return;
        int index = run.katePos.x + run.katePos.y * KaitRun.BattleSize;
        Transform parent = index >= 0 && index < battleCells.Length && battleCells[index] != null
            ? battleCells[index].transform
            : canvas.transform;
        kaitSpine = KaitSpineView.Create(makotoSkeletonData, parent, new Vector2(115, 115));
    }

    private RectTransform CreateGhostToken(RectTransform source, int momentumValue)
    {
        float tier = Mathf.Clamp01((momentumValue - 1) / 4f);
        Color ghostColor = Color.Lerp(new Color(1f, 1f, 1f, 0.22f), new Color(Coral.r, 0.18f, 0.24f, 0.48f), tier);
        Image image = Rect("Kait Speed Trail", canvas.transform, Vector2.zero, new Vector2(72, 72), ghostColor);
        image.raycastTarget = false;
        image.rectTransform.position = source.position;
        Text trail = MakeText(">", image.transform, Vector2.zero, new Vector2(58, 58), 30, new Color(Cream.r, Cream.g, Cream.b, 0.7f), TextAnchor.MiddleCenter, FontStyle.Bold);
        trail.raycastTarget = false;
        return image.rectTransform;
    }

    private IEnumerator FadeAndDestroy(RectTransform rect, float duration)
    {
        if (rect == null) yield break;
        CanvasGroup group = rect.gameObject.AddComponent<CanvasGroup>();
        float elapsed = 0f;
        while (elapsed < duration && rect != null)
        {
            group.alpha = 1f - elapsed / duration;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        if (rect != null) Destroy(rect.gameObject);
    }

    private IEnumerator AnimateCombatFeedback(KaitTurnResult result)
    {
        bool hasImpact = false;
        bool kateHit = false;
        if (result.damageDealt > 0 && InsideBattle(result.blockedEnemyCell))
        {
            StartCoroutine(FloatDamage(result.blockedEnemyCell, result.damageDealt, Coral));
            hasImpact = true;
        }
        foreach (KaitEnemyAction action in result.enemyActions)
            if (action.hitKate)
            {
                kateHit = true;
                kaitSpine?.PlayOnce(run.kateHp <= 0 ? KaitSpineView.Die : KaitSpineView.Damage, run.kateHp <= 0 ? null : KaitSpineView.Idle);
                StartCoroutine(FloatDamage(run.katePos, Mathf.Max(1, action.damage), Gold));
                hasImpact = true;
            }
        if (!kateHit && result.collisionDamage + result.riftBlockDamage > 0)
        {
            kaitSpine?.PlayOnce(run.kateHp <= 0 ? KaitSpineView.Die : KaitSpineView.Damage, run.kateHp <= 0 ? null : KaitSpineView.Idle);
            StartCoroutine(FloatDamage(run.katePos, result.collisionDamage + result.riftBlockDamage, Gold));
            hasImpact = true;
        }
        foreach (Vector2Int cell in result.killedEnemyCells)
            if (InsideBattle(cell))
            {
                StartCoroutine(KillFlashAt(cell));
                hasImpact = true;
            }
        if (hasImpact) yield return new WaitForSecondsRealtime(0.055f);
    }

    private IEnumerator FloatDamage(Vector2Int cell, int amount, Color color)
    {
        RectTransform anchor = battleCells[cell.x + cell.y * KaitRun.BattleSize].rectTransform;
        Text text = MakeText($"-{amount}", canvas.transform, Vector2.zero, new Vector2(86, 42), 24, color, TextAnchor.MiddleCenter, FontStyle.Bold);
        text.raycastTarget = false;
        text.rectTransform.position = anchor.position + new Vector3(25f, 20f, 0f);
        Vector3 from = text.rectTransform.position;
        Color original = text.color;
        float elapsed = 0f;
        const float duration = 0.44f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            text.rectTransform.position = from + Vector3.up * (30f * t);
            text.color = new Color(original.r, original.g, original.b, 1f - t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Destroy(text.gameObject);
    }

    private IEnumerator KillFlashAt(Vector2Int cell)
    {
        RectTransform anchor = battleCells[cell.x + cell.y * KaitRun.BattleSize].rectTransform;
        Image flash = Rect("Kill Highlight", canvas.transform, Vector2.zero, new Vector2(88, 88), new Color(Cream.r, Cream.g, Cream.b, 0.85f));
        flash.raycastTarget = false;
        flash.rectTransform.position = anchor.position;
        yield return ScalePulse(flash.rectTransform, 0.72f, 1.16f, 0.08f);
        yield return FadeAndDestroy(flash.rectTransform, 0.2f);
    }

    private IEnumerator AnimateChainEdge(int chainCount)
    {
        if (edgePulse == null || chainCount < 2) yield break;
        float peak = Mathf.Min(0.18f, 0.055f + chainCount * 0.018f);
        float elapsed = 0f;
        const float duration = 0.18f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float alpha = Mathf.Sin(t * Mathf.PI) * peak;
            edgePulse.color = new Color(Coral.r, 0.12f, 0.18f, alpha);
            edgePulse.rectTransform.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.003f * Mathf.Min(chainCount, 6));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        edgePulse.color = new Color(Coral.r, Coral.g, Coral.b, 0f);
        edgePulse.rectTransform.localScale = Vector3.one;
    }

    private IEnumerator AnimateSkillPulse(KaitSkill skill)
    {
        string animation = KaitSpineView.OtherSkill;
        if (skill == KaitSkill.SwiftBoots) animation = KaitSpineView.JoyShort;
        else if (skill == KaitSkill.CatAgility) animation = KaitSpineView.JoyLong;
        else if (skill == KaitSkill.DreadSlash) animation = KaitSpineView.LargeAttack;
        kaitSpine?.PlayOnce(animation);
        int slot = run.skills.IndexOf(skill);
        if (slot >= 0 && slot < skillButtons.Length)
            yield return ScalePulse(skillButtons[slot].GetComponent<RectTransform>(), 0.92f, 1.08f, 0.14f);
        if (skill == KaitSkill.SwiftBoots || skill == KaitSkill.CatAgility)
            yield return PulseBattleCell(run.katePos, Coral, 0.15f);
    }

    private IEnumerator AnimateDreadSlashWave(KaitDirection direction)
    {
        Vector2Int delta = KaitRun.Delta(direction);
        Vector2Int p = run.katePos + delta;
        var waves = new List<RectTransform>();
        while (InsideBattle(p) && !run.walls[p.x, p.y])
        {
            Image wave = Rect("Dread Slash Wave", canvas.transform, Vector2.zero, new Vector2(60, 34), new Color(Coral.r, Coral.g, Coral.b, 0.72f));
            wave.raycastTarget = false;
            wave.rectTransform.position = battleCells[p.x + p.y * KaitRun.BattleSize].rectTransform.position;
            wave.rectTransform.localRotation = Quaternion.Euler(0f, 0f, direction == KaitDirection.Up || direction == KaitDirection.Down ? 90f : 0f);
            waves.Add(wave.rectTransform);
            yield return new WaitForSecondsRealtime(0.025f);
            p += delta;
        }
        foreach (RectTransform wave in waves) StartCoroutine(FadeAndDestroy(wave, 0.16f));
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
        if (run.won) kaitSpine?.PlayLoop(KaitSpineView.Victory);
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
        if (roundedSprite != null)
        {
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
        }
        return image;
    }

    private static Sprite LoadPixelSprite(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null) return null;
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16f);
    }

    private Sprite EnemyPortrait(KaitEnemyType type)
    {
        if (type == KaitEnemyType.Grunt) return gruntPortrait;
        if (type == KaitEnemyType.Swordsman) return swordsmanPortrait;
        if (type == KaitEnemyType.Archer) return archerPortrait;
        if (type == KaitEnemyType.Guard) return guardPortrait;
        if (type == KaitEnemyType.ShieldKnight) return bossPortrait;
        return elitePortrait;
    }

    private static Color EnemyTileColor(KaitEnemyType type)
    {
        if (type == KaitEnemyType.Grunt) return ThreatColor(4);
        if (type == KaitEnemyType.Swordsman || type == KaitEnemyType.Archer) return ThreatColor(8);
        if (type == KaitEnemyType.Guard) return ThreatColor(16);
        if (type == KaitEnemyType.ShieldKnight) return ThreatColor(128);
        return ThreatColor(32);
    }

    private static float HalfArrowAngle(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return 90f;
        if (direction == Vector2Int.left) return 180f;
        if (direction == Vector2Int.down) return -90f;
        return 0f;
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
