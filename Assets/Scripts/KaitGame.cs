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
    private Font threatBoardFont;
    private Canvas canvas;
    private RectTransform gameContent;
    private Coroutine screenShakeRoutine;
    private Vector2 gameContentBasePosition;
    private Sprite roundedSprite;
    private Image[] battleCells;
    private Image[] battleCellTiles;
    private Image[] battleCellTints;
    private Image[] battleUnitClips;
    private Text[] battleLabels;
    private Image[] battlePortraits;
    private HealthBarView[] battleHealthBars;
    private Text[] battleFacingLabels;
    private Text[] battleStatusLabels;
    private Image[] battleWarningLines;
    private Image[] battleRifts;
    private Image[] threatCells;
    private Text[] threatLabels;
    private Text turnText;
    private HealthBarView runHealthBar;
    private Text statusText;
    private Text helpText;
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
    private KaitDirection? bufferedDirection;
    private Vector2Int? displayKate;
    private Vector2Int? trailCell;
    private bool hideKate;
    private List<KaitEnemy> animatedEnemies;
    private List<KaitSpawnRequest> animatedSpawns;
    private int[,] displayedThreat;
    private bool hideThreatValues;
    private readonly HashSet<Vector2Int> impactCells = new HashSet<Vector2Int>();
    private GameObject tutorialOverlay;
    private Sprite kaitPortrait;
    private SkeletonDataAsset makotoSkeletonData;
    private KaitSpineView kaitSpine;
    private Sprite gruntPortrait;
    private Sprite swordsmanPortrait;
    private Sprite archerPortrait;
    private Sprite guardPortrait;
    private Sprite warlockPortrait;
    private Sprite bossPortrait;
    private Sprite attackWarningSprite;
    private Sprite dungeonFloorSprite;
    private Sprite dungeonWallSprite;
    private readonly Sprite[] stoneFenceTiles = new Sprite[7];
    private Sprite spawnRiftSprite;
    private Sprite dungeonPanelSprite;
    private Sprite dungeonButtonSprite;
    private Sprite dungeonButtonPressedSprite;
    private readonly Sprite[] healthFillSprites = new Sprite[3];
    private readonly Sprite[] healthSlotSprites = new Sprite[3];
    private Sprite grassBackgroundSprite;
    private readonly Dictionary<KaitEnemyType, SkeletonDataAsset> enemySkeletonData = new Dictionary<KaitEnemyType, SkeletonDataAsset>();
    private readonly Dictionary<int, EnemySpineView> enemySpines = new Dictionary<int, EnemySpineView>();
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

    private sealed class ProjectileVisual
    {
        public RectTransform rect;
        public Vector3 from;
        public Vector3 to;
    }

    private sealed class HealthBarView
    {
        public Image root;
        public readonly Image[] fills = new Image[3];
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
    private static readonly Color GrassBase = Hex("#83AF6F");
    private static readonly Vector2 UnitHealthBarSize = new Vector2(72f, 16f);
    private static readonly Vector2 UnitHealthBarPosition = new Vector2(0f, -50f);

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
        threatBoardFont = Resources.Load<Font>("NotoSansCJKsc-Regular");
        if (threatBoardFont == null) threatBoardFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Arial" }, 24);
        if (threatBoardFont == null) threatBoardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiFont = Resources.Load<Font>("Fonts/FusionPixel12pxProportionalZhHans");
        if (uiFont == null) uiFont = threatBoardFont;
        ConfigureUiFontAtlas(uiFont);
        Font.textureRebuilt += OnFontTextureRebuilt;
        kaitPortrait = LoadPixelSprite("KenneyTinyDungeon/Kait");
        makotoSkeletonData = Resources.Load<SkeletonDataAsset>("Characters/Makoto/Makoto_SkeletonData");
        gruntPortrait = LoadPortraitSprite("EnemyPortraits/100161", new Rect(0.3452f, 0.1883f, 0.1913f, 0.3315f));
        swordsmanPortrait = LoadPortraitSprite("EnemyPortraits/105731", new Rect(0.3635f, 0.1883f, 0.2190f, 0.3400f));
        archerPortrait = LoadPortraitSprite("EnemyPortraits/106331", new Rect(0.3452f, 0.1883f, 0.2373f, 0.3293f));
        guardPortrait = LoadPortraitSprite("EnemyPortraits/112731", new Rect(0.3386f, 0.1883f, 0.2623f, 0.4137f));
        warlockPortrait = LoadPortraitSprite("EnemyPortraits/111031", new Rect(0.3635f, 0.1883f, 0.2269f, 0.3426f));
        bossPortrait = LoadPortraitSprite("EnemyPortraits/104731", new Rect(0.3632f, 0.1885f, 0.2285f, 0.3793f));
        attackWarningSprite = LoadUiSprite("KaitVisuals/AttackWarningStripes");
        dungeonFloorSprite = LoadPixelSprite("KaitVisuals/DungeonFloor");
        dungeonWallSprite = LoadPixelSprite("KaitVisuals/DungeonWall");
        for (int i = 0; i < stoneFenceTiles.Length; i++)
            stoneFenceTiles[i] = LoadPixelSprite($"KaitVisuals/TownWall/GK_OB_C_{68 + i:D3}");
        spawnRiftSprite = LoadPixelSprite("KaitVisuals/SpawnRift");
        dungeonPanelSprite = LoadSlicedAtlasSprite("KaitVisuals/DungeonUI/TileMap1", new Rect(16f, 160f, 48f, 48f), 16f);
        dungeonButtonSprite = LoadSlicedAtlasSprite("KaitVisuals/DungeonUI/ButtonsMap", new Rect(0f, 54f, 16f, 16f), 4f);
        dungeonButtonPressedSprite = LoadSlicedAtlasSprite("KaitVisuals/DungeonUI/ButtonsMap", new Rect(0f, 36f, 16f, 16f), 4f);
        healthFillSprites[0] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthFillLeft");
        healthFillSprites[1] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthFillMiddle");
        healthFillSprites[2] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthFillRight");
        healthSlotSprites[0] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthSlotLeft");
        healthSlotSprites[1] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthSlotMiddle");
        healthSlotSprites[2] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthSlotRight");
        grassBackgroundSprite = LoadTiledPixelSprite("KaitVisuals/GrassBackground");
        LoadEnemySkeleton(KaitEnemyType.Grunt, "100161");
        LoadEnemySkeleton(KaitEnemyType.Swordsman, "105731");
        LoadEnemySkeleton(KaitEnemyType.Archer, "106331");
        LoadEnemySkeleton(KaitEnemyType.Guard, "112731");
        LoadEnemySkeleton(KaitEnemyType.Warlock, "111031");
        LoadEnemySkeleton(KaitEnemyType.ShieldKnight, "104731");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Font.textureRebuilt -= OnFontTextureRebuilt;
        if (instance == this) instance = null;
    }

    private void OnFontTextureRebuilt(Font rebuiltFont)
    {
        if (rebuiltFont == uiFont) ConfigureUiFontAtlas(rebuiltFont);
    }

    private static void ConfigureUiFontAtlas(Font font)
    {
        if (font == null || font.material == null || font.material.mainTexture == null) return;
        font.material.mainTexture.filterMode = FilterMode.Bilinear;
        font.material.mainTexture.anisoLevel = 0;
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
        if (Application.platform == RuntimePlatform.WindowsPlayer && string.IsNullOrEmpty(screenshotPath))
            StartCoroutine(EnforceWindowsFullscreen());
        string demoStepsValue = CommandLineValue("-kaitDemoSteps");
        if (CommandLineValue("-kaitTutorial") == "1") tutorialOverlay.SetActive(true);
        int.TryParse(demoStepsValue, out int demoSteps);
        if (!string.IsNullOrEmpty(screenshotPath)) StartCoroutine(CaptureAndQuit(screenshotPath, demoSteps));
    }

    private IEnumerator EnforceWindowsFullscreen()
    {
        for (int frame = 0; frame < 6; frame++)
        {
            yield return new WaitForEndOfFrame();
            Resolution native = Screen.currentResolution;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.SetResolution(native.width, native.height, FullScreenMode.FullScreenWindow);
            Screen.fullScreen = true;
        }
        Debug.Log($"Kait fullscreen applied: {Screen.width}x{Screen.height}, mode={Screen.fullScreenMode}");
    }

    private void Update()
    {
        if (run.ended) return;
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
        canvas.pixelPerfect = true;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image bg = Rect("Background", canvas.transform, Vector2.zero, new Vector2(1920, 1080), Background);
        if (grassBackgroundSprite != null)
        {
            bg.sprite = grassBackgroundSprite;
            bg.type = Image.Type.Tiled;
            bg.color = Color.white;
        }
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.sizeDelta = Vector2.zero;

        MakeText("Kait", bg.transform, new Vector2(-860, 500), new Vector2(180, 48), 32, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        MakeButton(bg.transform, new Vector2(908, 500), new Vector2(48, 48), "?").onClick.AddListener(() => tutorialOverlay.SetActive(true));

        var contentGo = new GameObject("Game Content", typeof(RectTransform));
        contentGo.transform.SetParent(bg.transform, false);
        RectTransform content = contentGo.GetComponent<RectTransform>();
        content.sizeDelta = new Vector2(1600, 900);
        content.localScale = Vector3.one * 1.2f;
        gameContent = content;
        gameContentBasePosition = content.anchoredPosition;

        BuildBattleBoard(content);
        BuildThreatBoard(content);
        BuildSidebar(content);
        BuildEndOverlay(bg.transform);
        BuildSkillChoiceOverlay(content);
        BuildTutorialOverlay(bg.transform);
    }

    private void BuildBattleBoard(Transform parent)
    {
        var boardGo = new GameObject("Battle Board", typeof(RectTransform));
        boardGo.transform.SetParent(parent, false);
        RectTransform boardRect = boardGo.GetComponent<RectTransform>();
        boardRect.anchorMin = boardRect.anchorMax = boardRect.pivot = new Vector2(0.5f, 0.5f);
        boardRect.sizeDelta = new Vector2(600, 600);
        boardRect.anchoredPosition = new Vector2(-440, 0);
        BuildStoneFence(boardGo.transform);
        var gridGo = new GameObject("Battle Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(boardGo.transform, false);
        RectTransform gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(600, 600);
        gridRect.anchoredPosition = Vector2.zero;
        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(120, 120);
        grid.spacing = Vector2.zero;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        battleCells = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleLabels = new Text[KaitRun.BattleSize * KaitRun.BattleSize];
        battlePortraits = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleHealthBars = new HealthBarView[KaitRun.BattleSize * KaitRun.BattleSize];
        battleFacingLabels = new Text[KaitRun.BattleSize * KaitRun.BattleSize];
        battleStatusLabels = new Text[KaitRun.BattleSize * KaitRun.BattleSize];
        battleWarningLines = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleRifts = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleCellTiles = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleCellTints = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleUnitClips = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        for (int visualY = KaitRun.BattleSize - 2; visualY >= 1; visualY--)
        {
            for (int x = 1; x < KaitRun.BattleSize - 1; x++)
            {
                int index = x + visualY * KaitRun.BattleSize;
                Image cell = Rect($"Cell {x},{visualY}", gridGo.transform, Vector2.zero, Vector2.zero, Color.clear);
                cell.sprite = null;
                cell.type = Image.Type.Simple;
                Vector2Int targetCell = new Vector2Int(x, visualY);
                cell.gameObject.AddComponent<Button>().onClick.AddListener(() => HandleBattleCellClick(targetCell));
                battleCells[index] = cell;
                Image tile = Rect("Dungeon Tile", cell.transform, Vector2.zero, Vector2.zero, Color.white);
                tile.sprite = dungeonFloorSprite;
                tile.type = Image.Type.Simple;
                tile.preserveAspect = false;
                tile.raycastTarget = false;
                Stretch(tile.rectTransform, 0);
                battleCellTiles[index] = tile;
                Image cellTint = Rect("Cell Tint", cell.transform, Vector2.zero, Vector2.zero, BattleTint(ThreatColor(0)));
                cellTint.sprite = null;
                cellTint.type = Image.Type.Simple;
                cellTint.raycastTarget = false;
                Stretch(cellTint.rectTransform, 0);
                battleCellTints[index] = cellTint;
                Image warningClip = Rect("Attack Warning Clip", cell.transform, Vector2.zero, new Vector2(109, 109), Color.white);
                warningClip.raycastTarget = false;
                warningClip.sprite = roundedSprite;
                warningClip.type = Image.Type.Sliced;
                Mask warningMask = warningClip.gameObject.AddComponent<Mask>();
                warningMask.showMaskGraphic = false;
                Image warning = Rect("Attack Warning", warningClip.transform, Vector2.zero, attackWarningSprite != null ? new Vector2(109, 109) : new Vector2(98, 14), Color.clear);
                warning.raycastTarget = false;
                warning.sprite = attackWarningSprite;
                warning.type = Image.Type.Simple;
                warning.preserveAspect = false;
                if (attackWarningSprite == null)
                {
                    for (int dash = -1; dash <= 1; dash++)
                    {
                        Image segment = Rect("Warning Dash", warning.transform, new Vector2(dash * 31, 0), new Vector2(22, 9), new Color(Coral.r, 0.18f, 0.22f, 0.72f));
                        segment.raycastTarget = false;
                    }
                }
                warning.gameObject.SetActive(false);
                battleWarningLines[index] = warning;
                Image rift = Rect("Spawn Rift", cell.transform, Vector2.zero, new Vector2(106, 106), Color.white);
                rift.raycastTarget = false;
                rift.sprite = spawnRiftSprite;
                rift.type = Image.Type.Simple;
                rift.preserveAspect = true;
                rift.gameObject.SetActive(false);
                battleRifts[index] = rift;
                Image unitClip = Rect("Unit Visual", cell.transform, Vector2.zero, new Vector2(114, 114), Color.clear);
                unitClip.raycastTarget = false;
                unitClip.sprite = null;
                unitClip.type = Image.Type.Simple;
                // Build order keeps rifts as ground decals below unit art and health bars.
                battleUnitClips[index] = unitClip;
                Image portrait = Rect("Unit Portrait", unitClip.transform, new Vector2(0, -2), new Vector2(112, 112), Color.white);
                portrait.sprite = null;
                portrait.type = Image.Type.Simple;
                portrait.preserveAspect = true;
                portrait.raycastTarget = false;
                portrait.gameObject.SetActive(false);
                battlePortraits[index] = portrait;
                unitClip.gameObject.SetActive(false);
                battleLabels[index] = MakeText("", cell.transform, Vector2.zero, Vector2.zero, 34, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(battleLabels[index].rectTransform, 3);
                battleFacingLabels[index] = MakeText("", cell.transform, new Vector2(0, -30), new Vector2(72, 24), 25, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                battleHealthBars[index] = MakeHealthBar(cell.transform, UnitHealthBarPosition, UnitHealthBarSize);
                battleHealthBars[index].root.gameObject.SetActive(false);
                battleStatusLabels[index] = MakeText("", cell.transform, new Vector2(-43, 42), new Vector2(28, 26), 19, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            }
        }
    }

    private void BuildStoneFence(Transform parent)
    {
        if (stoneFenceTiles[0] == null) return;
        const int tilesPerSide = 15;
        const float tileSize = 40f;
        const float edgeCenter = 320f;
        const float firstAxis = -280f;

        // The source artwork is a front-facing wall cap. Side walls therefore
        // stay upright and overlap from top to bottom, exposing only each
        // narrow bright cap while the next tile covers its dark wall face.
        const float sideStep = 6.25f;
        const int sideStackCount = 97;
        for (int i = 0; i < sideStackCount; i++)
        {
            float y = 300f - i * sideStep;
            MakeStoneFenceSideCover($"Town Wall Left Cover {i}", parent, new Vector2(-edgeCenter, y));
            MakeStoneFenceTile($"Town Wall Left {i}", parent, new Vector2(-edgeCenter, y), i + 5);
            MakeStoneFenceSideCover($"Town Wall Right Cover {i}", parent, new Vector2(edgeCenter, y));
            MakeStoneFenceTile($"Town Wall Right {i}", parent, new Vector2(edgeCenter, y), i + 1);
        }

        // Draw horizontal rows after the overlapping side stacks so their end
        // faces and the four corners are cleanly covered.
        for (int i = 0; i < tilesPerSide; i++)
        {
            float axis = firstAxis + i * tileSize;
            MakeStoneFenceTile($"Town Wall Top {i}", parent, new Vector2(axis, edgeCenter), i);
            MakeStoneFenceTile($"Town Wall Bottom {i}", parent, new Vector2(axis, -edgeCenter), i + 3);
        }

        // Dedicated corner blocks close the four 40 px outer gaps without
        // stretching a side texture across the turn.
        MakeStoneFenceTile("Town Wall Corner TL", parent, new Vector2(-edgeCenter, edgeCenter), 0);
        MakeStoneFenceTile("Town Wall Corner TR", parent, new Vector2(edgeCenter, edgeCenter), 1);
        MakeStoneFenceTile("Town Wall Corner BL", parent, new Vector2(-edgeCenter, -edgeCenter), 2);
        MakeStoneFenceTile("Town Wall Corner BR", parent, new Vector2(edgeCenter, -edgeCenter), 3);
    }

    private void MakeStoneFenceSideCover(string name, Transform parent, Vector2 position)
    {
        Image cover = Rect(name, parent, position, new Vector2(40f, 40f), GrassBase);
        cover.sprite = null;
        cover.type = Image.Type.Simple;
        cover.raycastTarget = false;
    }

    private void MakeStoneFenceTile(string name, Transform parent, Vector2 position, int tileIndex)
    {
        Sprite sprite = stoneFenceTiles[Mathf.Abs(tileIndex) % stoneFenceTiles.Length];
        if (sprite == null) return;
        Image tile = Rect(name, parent, position, new Vector2(40f, 40f), Color.white);
        tile.sprite = sprite;
        tile.type = Image.Type.Simple;
        tile.preserveAspect = true;
        tile.raycastTarget = false;
    }

    private void BuildThreatBoard(Transform parent)
    {
        Image frame = Rect("Threat Panel", parent, new Vector2(460, 125), new Vector2(390, 390), Panel);
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
                // Keep the original 2048-board number treatment unchanged.
                threatLabels[index] = MakeText("", threatCells[index].transform, Vector2.zero, Vector2.zero, 24, Cream, TextAnchor.MiddleCenter, FontStyle.Bold, false);
                threatLabels[index].font = threatBoardFont;
                Stretch(threatLabels[index].rectTransform, 2);
            }
        }
    }

    private void BuildSidebar(Transform parent)
    {
        Image info = Rect("Run Info", parent, new Vector2(55, 245), new Vector2(250, 70), Panel);
        SkinPanel(info);
        turnText = MakeText("", info.transform, new Vector2(0, 14), new Vector2(220, 26), 16, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        MakeText("生命", info.transform, new Vector2(-86, -16), new Vector2(44, 20), 13, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        runHealthBar = MakeHealthBar(info.transform, new Vector2(8, -16), UnitHealthBarSize);
        statusText = MakeText("", parent, Vector2.zero, Vector2.zero, 1, Color.clear, TextAnchor.MiddleCenter);
        statusText.gameObject.SetActive(false);

        Image rules = Rect("Skills", parent, new Vector2(55, -20), new Vector2(250, 420), Panel);
        SkinPanel(rules);
        MakeText("技能栏", rules.transform, new Vector2(0, 176), new Vector2(204, 30), 18, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        skillStatusText = MakeText("尚未解锁技能", rules.transform, new Vector2(0, 145), new Vector2(204, 26), 13, Peach, TextAnchor.MiddleCenter);
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int slot = i;
            skillButtons[i] = MakeButton(rules.transform, new Vector2(0, 70 - i * 100), new Vector2(220, 82), "未解锁");
            skillButtonLabels[i] = skillButtons[i].GetComponentInChildren<Text>();
            skillButtons[i].onClick.AddListener(() => HandleSkillButton(slot));
        }

        Image controls = Rect("Controls", parent, new Vector2(460, -205), new Vector2(390, 230), Panel);
        SkinPanel(controls);
        controlsPanel = controls.gameObject;
        MakeButton(controls.transform, new Vector2(-20, 55), new Vector2(58, 50), "W").onClick.AddListener(() => HandleDirection(KaitDirection.Up));
        MakeButton(controls.transform, new Vector2(-82, -3), new Vector2(58, 50), "A").onClick.AddListener(() => HandleDirection(KaitDirection.Left));
        MakeButton(controls.transform, new Vector2(-20, -3), new Vector2(58, 50), "S").onClick.AddListener(() => HandleDirection(KaitDirection.Down));
        MakeButton(controls.transform, new Vector2(42, -3), new Vector2(58, 50), "D").onClick.AddListener(() => HandleDirection(KaitDirection.Right));
        MakeButton(controls.transform, new Vector2(130, 25), new Vector2(96, 88), "重开\nR").onClick.AddListener(NewRun);

        helpText = MakeText("", parent, Vector2.zero, Vector2.zero, 1, Color.clear, TextAnchor.MiddleCenter);
        helpText.gameObject.SetActive(false);
    }

    private void BuildSkillChoiceOverlay(Transform parent)
    {
        Image card = Rect("Skill Choice Side Panel", parent, new Vector2(460, -205), new Vector2(390, 230), Panel);
        SkinPanel(card);
        skillChoiceOverlay = card.gameObject;
        skillChoiceTitle = MakeText("选择成长", card.transform, new Vector2(0, 84), new Vector2(350, 26), 15, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
        for (int i = 0; i < 2; i++)
        {
            int choice = i;
            skillChoiceButtons[i] = MakeButton(card.transform, new Vector2(-91 + i * 182, -18), new Vector2(172, 138), "");
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
        SkinPanel(card);
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
        SkinPanel(card);
        MakeText("玩法教程", card.transform, new Vector2(0, 295), new Vector2(820, 52), 30, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        Text body = MakeText(
            "双盘联动\n每次输入方向，战场与右侧数字盘同步响应。数字盘按 2048 规则移动与合并；合成会在战场对应位置留下出生裂隙。\n\n" +
            "战场与边界\n画面显示 5×5 活动区域。凯特会沿方向滑行；抵达画面边界等同撞墙停止，逻辑仍保留原 7×7 外圈边界。\n\n" +
            "单位信息\n人物直接绘制在地砖上，右上角血条表示剩余生命，下方半箭头表示朝向。武器可自然伸出格子，但人物身体始终以格子中心定位。\n\n" +
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
        ClearEnemySpines();
        busy = false;
        bufferedDirection = null;
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
        if (run.ended) return;
        if (busy)
        {
            // Keep the most recent direction pressed during an animation so the
            // next action starts immediately when the visual sequence releases.
            bufferedDirection = direction;
            return;
        }
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
        var healthBefore = new List<KaitEnemy>();
        foreach (KaitEnemy enemy in enemySnapshot)
            healthBefore.Add(new KaitEnemy { id = enemy.id, hp = enemy.hp, maxHp = enemy.maxHp, pos = enemy.pos, life = enemy.life });
        int kateHpBefore = run.kateHp + result.playerDamage;
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
            yield return PulseBattleUnit(result.pushFrom, Color.white, 0.18f, result.damagedEnemyId);

        if (result.dreadSlash)
        {
            yield return AnimateDreadSlashWave(result.globalDirection);
            yield return AnimateAllEnemyActions(result.enemyActions.FindAll(a => a.type == KaitIntentType.Move));
            yield return AnimateAllEnemyActions(result.enemyActions.FindAll(a => a.type != KaitIntentType.Move));
        }
        else yield return AnimateAllEnemyActions(result.enemyActions);

        yield return AnimateCombatFeedback(result, healthBefore, kateHpBefore);
        yield return AnimateEnemyDeaths(healthBefore);

        var previousEnemyIds = new HashSet<int>();
        foreach (KaitEnemy enemy in enemySnapshot) previousEnemyIds.Add(enemy.id);
        animatedEnemies = null;
        animatedSpawns = null;
        hideKate = false;
        displayKate = null;
        trailCell = null;
        RefreshBattle();

        float landingDuration = 0f;
        foreach (KaitEnemy enemy in run.enemies)
            if (enemy.life != KaitEnemyLife.Dead && !previousEnemyIds.Contains(enemy.id))
            {
                EnemySpineView view = EnemySpine(enemy);
                if (view == null) continue;
                view.PlayLanding();
                landingDuration = Mathf.Max(landingDuration, view.LandingDuration);
            }
        if (landingDuration > 0f) yield return new WaitForSecondsRealtime(Mathf.Min(landingDuration, 0.55f));

        yield return AnimateEnemyAttackPreparation();

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
        if (run.ended)
        {
            bufferedDirection = null;
            ShowEnd();
        }
        else ConsumeBufferedDirection();
    }

    private void ConsumeBufferedDirection()
    {
        if (busy || run.ended || !bufferedDirection.HasValue) return;
        KaitDirection next = bufferedDirection.Value;
        bufferedDirection = null;
        HandleDirection(next);
    }

    private void RefreshAll()
    {
        RefreshBattle();
        RefreshThreat();
        RefreshSkillUI();
        turnText.text = $"回合 {run.turn}　速度 {run.momentum}";
        SetHealthBar(runHealthBar, run.kateHp);
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
            StartCoroutine(PulseBattleUnit(cell, skill == KaitSkill.IceTomb ? Cyan : Coral, 0.2f));
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
        ConsumeBufferedDirection();
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
        foreach (EnemySpineView view in enemySpines.Values) view.SetVisible(false);
        Vector2Int kate = displayKate ?? run.katePos;
        List<KaitDirection> allowed = run.chainActive ? run.AllowedTurnDirections() : new List<KaitDirection>();
        for (int y = 1; y < KaitRun.BattleSize - 1; y++)
        {
            for (int x = 1; x < KaitRun.BattleSize - 1; x++)
            {
                int index = x + y * KaitRun.BattleSize;
                Vector2Int p = new Vector2Int(x, y);
                Image tile = battleCellTiles[index];
                Image image = battleCellTints[index];
                Text label = battleLabels[index];
                label.text = "";
                label.rectTransform.localRotation = Quaternion.identity;
                battleHealthBars[index].root.gameObject.SetActive(false);
                battleFacingLabels[index].text = "";
                battleFacingLabels[index].rectTransform.localRotation = Quaternion.identity;
                battleStatusLabels[index].text = "";
                battlePortraits[index].gameObject.SetActive(false);
                battlePortraits[index].color = Color.white;
                battleUnitClips[index].gameObject.SetActive(false);
                battleWarningLines[index].gameObject.SetActive(false);
                battleRifts[index].gameObject.SetActive(false);
                tile.sprite = dungeonFloorSprite != null ? dungeonFloorSprite : roundedSprite;
                tile.type = dungeonFloorSprite != null ? Image.Type.Simple : Image.Type.Sliced;
                tile.color = Color.white;
                image.color = new Color(Background.r, Background.g, Background.b, 0.12f);
                if (run.walls[x, y])
                {
                    tile.sprite = dungeonWallSprite != null ? dungeonWallSprite : roundedSprite;
                    tile.type = dungeonWallSprite != null ? Image.Type.Simple : Image.Type.Sliced;
                    tile.color = dungeonWallSprite != null ? Color.white : Void;
                    image.color = Color.clear;
                    label.text = "";
                }
                Color? intentTint = IntentTintAt(p);
                bool impact = impactCells.Contains(p);
                if (!run.walls[x, y] && (intentTint.HasValue || impact))
                {
                    Image warning = battleWarningLines[index];
                    warning.gameObject.SetActive(true);
                    Color warningTint = impact ? Gold : Hex("#B64832");
                    float warningAlpha = impact ? 0.9f : 0.52f;
                    warning.color = warning.sprite != null
                        ? new Color(warningTint.r, warningTint.g, warningTint.b, warningAlpha)
                        : Color.clear;
                    foreach (Image dash in warning.GetComponentsInChildren<Image>(true))
                        if (dash != warning) dash.color = new Color(warningTint.r, warningTint.g, warningTint.b, impact ? 0.95f : 0.72f);
                    warning.rectTransform.localRotation = Quaternion.identity;
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
                if (targetingSkill != KaitSkill.None && run.EnemyAt(p) != null) image.color = Color.Lerp(image.color, Cyan, 0.35f);

                KaitSpawnRequest spawn = SpawnAtVisual(p);
                KaitEnemy enemy = EnemyAtVisual(p);
                if (spawn != null)
                {
                    Image rift = battleRifts[index];
                    rift.gameObject.SetActive(true);
                }
                if (enemy != null)
                {
                    Image unitClip = battleUnitClips[index];
                    unitClip.color = Color.clear;
                    unitClip.gameObject.SetActive(true);
                    image.color = Color.clear;
                    Color unitTint = enemy.frozenActions > 0 ? new Color(0.62f, 0.9f, 1f, 1f) : enemy.life == KaitEnemyLife.Preparing ? new Color(1f, 1f, 1f, 0.68f) : Color.white;
                    EnemySpineView enemySpine = EnemySpine(enemy);
                    if (enemySpine != null)
                    {
                        enemySpine.SetParent(unitClip.transform, 1);
                        enemySpine.SetTint(unitTint);
                        enemySpine.Face(enemy.type == KaitEnemyType.ShieldKnight ? enemy.facing : enemy.intent.direction);
                        enemySpine.SetVisible(true);
                    }
                    else
                    {
                        battlePortraits[index].sprite = EnemyPortrait(enemy.type);
                        battlePortraits[index].gameObject.SetActive(true);
                        battlePortraits[index].color = unitTint;
                    }
                    SetHealthBar(battleHealthBars[index], enemy.hp);
                    battleHealthBars[index].root.gameObject.SetActive(true);
                    Vector2Int facing = enemy.type == KaitEnemyType.ShieldKnight ? enemy.facing : enemy.intent.direction;
                    if (facing != Vector2Int.zero) battleFacingLabels[index].text = ">";
                    battleFacingLabels[index].rectTransform.localRotation = Quaternion.Euler(0f, 0f, HalfArrowAngle(facing));
                    battleFacingLabels[index].color = enemy.life == KaitEnemyLife.Preparing ? Peach : Cream;
                    if (enemy.frozenActions > 0) { battleStatusLabels[index].text = "❄"; battleStatusLabels[index].color = Cyan; }
                    if (run.forcedTargetEnemyId == enemy.id) { battleStatusLabels[index].text = "!"; battleStatusLabels[index].color = Coral; }
                }
                if (!hideKate && kate == p)
                {
                    Image unitClip = battleUnitClips[index];
                    unitClip.color = Color.clear;
                    unitClip.gameObject.SetActive(true);
                    image.color = Color.clear;
                    if (kaitSpine != null)
                    {
                        // Keep Kait outside the legacy per-unit visual container.
                        // Her sword is allowed to extend over adjacent cells.
                        kaitSpine.SetParent(battleCells[index].transform, 4);
                        kaitSpine.SetVisible(true);
                    }
                    else
                    {
                        battlePortraits[index].sprite = kaitPortrait;
                        battlePortraits[index].gameObject.SetActive(true);
                    }
                    if (run.chainActive) battleFacingLabels[index].text = ">";
                    battleFacingLabels[index].rectTransform.localRotation = Quaternion.Euler(0f, 0f, HalfArrowAngle(KaitRun.Delta(run.currentDirection)));
                    battleFacingLabels[index].color = Void;
                }
            }
        }
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
                    threatLabels[index].text = "";
                    threatCells[index].color = Void;
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
                rangedState = enemy.rangedState,
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
                bool killedBlockedEnemy = animatedEnemies != null && animatedEnemies.Exists(e =>
                    e.pos == result.blockedEnemyCell && result.playerKilledEnemyIds.Contains(e.id));
                kaitSpine?.Face(result.kaitDirection);
                kaitSpine?.PlayOnce(killedBlockedEnemy ? KaitSpineView.ChainAttack : KaitSpineView.Attack);
                for (int i = 0; i < result.playerKilledEnemyIds.Count; i++)
                {
                    int chainKills = Mathf.Max(1, result.chainKillCount - result.playerKilledEnemyIds.Count + i + 1);
                    GameAudio.PlayKaitKill(chainKills);
                    TriggerChainShake(chainKills);
                    if (i + 1 < result.playerKilledEnemyIds.Count) yield return new WaitForSecondsRealtime(0.06f);
                }
                yield return PulseBattleUnit(result.blockedEnemyCell, Color.white, 0.16f, result.damagedEnemyId);
            }
            else if (result.stoppedByWall || result.chainEndedByWall || result.activeBrake || result.pushBlockedByWall)
                kaitSpine?.PlayOnce(KaitSpineView.StandBy);
            yield break;
        }

        hideKate = true;
        kaitSpine?.Face(result.kaitDirection);
        RefreshBattle();
        KaitSpineView movingKait = CreateFloatingKait(battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, result.kaitDirection);
        RectTransform token = movingKait != null ? movingKait.Root : CreateFloatingPortrait(kaitPortrait, Color.clear, battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, new Vector2(115, 115));
        movingKait?.PlayLoop(KaitSpineView.Run);
        var ghosts = new List<RectTransform>();
        var points = new List<Vector3> { battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform.position };
        foreach (Vector2Int cell in result.katePath) points.Add(battleCells[cell.x + cell.y * KaitRun.BattleSize].rectTransform.position);

        int segments = points.Count - 1;
        float duration = Mathf.Min(0.36f, 0.16f + segments * 0.025f);
        float elapsed = 0f;
        int lastReached = 0;
        int killSoundsPlayed = 0;
        int chainKillsBeforeTurn = Mathf.Max(0, result.chainKillCount - result.playerKilledEnemyIds.Count);
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
                RectTransform ghost = CreateGhostToken(battleCells[cell.x + cell.y * KaitRun.BattleSize].rectTransform, momentumAtCell, result.kaitDirection);
                ghosts.Add(ghost);
                while (ghosts.Count > Mathf.Clamp(momentumAtCell, 1, 5))
                {
                    Destroy(ghosts[0].gameObject);
                    ghosts.RemoveAt(0);
                }
                if (animatedEnemies.Exists(e => e.pos == cell && result.playerKilledEnemyIds.Contains(e.id)))
                {
                    KaitEnemy struckEnemy = animatedEnemies.Find(e => e.pos == cell && result.playerKilledEnemyIds.Contains(e.id));
                    killSoundsPlayed++;
                    int chainKills = chainKillsBeforeTurn + killSoundsPlayed;
                    GameAudio.PlayKaitKill(chainKills);
                    TriggerChainShake(chainKills);
                    movingKait?.PlayOnce(KaitSpineView.ChainAttack, KaitSpineView.Run);
                    RefreshBattle();
                    StartCoroutine(PulseBattleUnit(cell, Color.white, 0.16f, struckEnemy.id));
                }
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        token.position = points[points.Count - 1];
        if (result.playerKilledEnemyIds.Count > 0)
            yield return new WaitForSecondsRealtime(0.08f);
        bool killedBlockedEnemyAfterSlide = result.blockedEnemyCell.x >= 0 && animatedEnemies.Exists(e =>
            e.pos == result.blockedEnemyCell && result.playerKilledEnemyIds.Contains(e.id));
        if (movingKait != null) movingKait.Destroy(); else Destroy(token.gameObject);
        foreach (RectTransform ghost in ghosts) StartCoroutine(FadeAndDestroy(ghost, 0.22f));
        animatedEnemies.RemoveAll(e => result.playerKilledEnemyIds.Contains(e.id));
        impactCells.Clear();
        hideKate = false;
        displayKate = null;
        RefreshBattle();
        while (killSoundsPlayed < result.playerKilledEnemyIds.Count)
        {
            killSoundsPlayed++;
            int chainKills = chainKillsBeforeTurn + killSoundsPlayed;
            GameAudio.PlayKaitKill(chainKills);
            TriggerChainShake(chainKills);
            if (killSoundsPlayed < result.playerKilledEnemyIds.Count) yield return new WaitForSecondsRealtime(0.06f);
        }
        if (result.blockedEnemyCell.x >= 0)
        {
            kaitSpine?.Face(result.kaitDirection);
            kaitSpine?.PlayOnce(killedBlockedEnemyAfterSlide ? KaitSpineView.ChainAttack : KaitSpineView.Attack);
            yield return PulseBattleUnit(result.blockedEnemyCell, Color.white, 0.14f, result.damagedEnemyId);
        }
        else if (result.stoppedByWall || result.chainEndedByWall || result.activeBrake || result.pushBlockedByWall)
            kaitSpine?.PlayOnce(KaitSpineView.StandBy);
        else if (result.playerKilledEnemyIds.Count > 0)
            kaitSpine?.PlayOnce(KaitSpineView.ChainAttack);
        else
            kaitSpine?.PlayLoop(KaitSpineView.Idle);
    }

    private IEnumerator AnimateAllEnemyActions(List<KaitEnemyAction> actions)
    {
        var moves = new List<EnemyMoveVisual>();
        var projectiles = new List<ProjectileVisual>();
        float actionAnimationDuration = 0f;
        foreach (KaitEnemyAction action in actions)
        {
            KaitEnemy enemy = animatedEnemies?.Find(e => e.id == action.enemyId);
            if (enemy == null) continue;
            if (action.type == KaitIntentType.Move && action.from != action.to)
            {
                RectTransform token = CreateFloatingPortrait(EnemyPortrait(enemy.type), EnemyTileColor(enemy.type), battleCells[action.from.x + action.from.y * KaitRun.BattleSize].rectTransform, new Vector2(115, 115), enemy.hp, enemy.maxHp);
                moves.Add(new EnemyMoveVisual
                {
                    enemy = enemy,
                    rect = token,
                    from = battleCells[action.from.x + action.from.y * KaitRun.BattleSize].rectTransform.position,
                    to = battleCells[action.to.x + action.to.y * KaitRun.BattleSize].rectTransform.position
                });
                animatedEnemies.Remove(enemy);
            }
            if (action.type == KaitIntentType.Melee || action.type == KaitIntentType.LineShot || action.type == KaitIntentType.CrossBlast)
            {
                EnemySpineView attacker = EnemySpine(enemy);
                if (attacker != null)
                {
                    attacker.Face(action.to - action.from);
                    attacker.PlayAttack();
                    actionAnimationDuration = Mathf.Max(actionAnimationDuration, attacker.AttackDuration);
                }
                foreach (Vector2Int cell in action.affectedCells) if (InsideBattle(cell)) impactCells.Add(cell);
                if (action.type == KaitIntentType.LineShot && action.affectedCells.Count > 0 && InsideBattle(action.from))
                {
                    Vector2Int firstCell = action.affectedCells[0];
                    Vector2Int lastCell = action.affectedCells[action.affectedCells.Count - 1];
                    Text arrow = MakeText(">", canvas.transform, Vector2.zero, new Vector2(48, 48), 42, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
                    arrow.gameObject.name = "Archer Projectile";
                    arrow.raycastTarget = false;
                    Outline outline = arrow.GetComponent<Outline>();
                    outline.effectColor = new Color(Wine.r, Wine.g, Wine.b, 0.95f);
                    outline.effectDistance = new Vector2(2f, -2f);
                    RectTransform arrowRect = arrow.rectTransform;
                    arrowRect.position = battleCells[action.from.x + action.from.y * KaitRun.BattleSize].rectTransform.position;
                    arrowRect.localRotation = Quaternion.Euler(0f, 0f, HalfArrowAngle(firstCell - action.from));
                    arrowRect.SetAsLastSibling();
                    projectiles.Add(new ProjectileVisual
                    {
                        rect = arrowRect,
                        from = arrowRect.position,
                        to = battleCells[lastCell.x + lastCell.y * KaitRun.BattleSize].rectTransform.position
                    });
                    actionAnimationDuration = Mathf.Max(actionAnimationDuration, 0.42f);
                }
                foreach (int victimId in action.friendlyHitIds)
                {
                    EnemySpineView victim = EnemySpine(victimId);
                    if (victim == null) continue;
                    KaitEnemy resolvedVictim = run.enemies.Find(e => e.id == victimId);
                    if (resolvedVictim != null && resolvedVictim.life != KaitEnemyLife.Dead)
                    {
                        victim.PlayDamage();
                        actionAnimationDuration = Mathf.Max(actionAnimationDuration, victim.DamageDuration);
                    }
                }
            }
        }

        RefreshBattle();
        if (moves.Count == 0 && impactCells.Count == 0 && projectiles.Count == 0) yield break;
        float elapsed = 0f;
        float duration = Mathf.Max(0.2f, Mathf.Min(actionAnimationDuration, 0.5f));
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            foreach (EnemyMoveVisual move in moves) move.rect.position = Vector3.Lerp(move.from, move.to, t);
            foreach (ProjectileVisual projectile in projectiles) projectile.rect.position = Vector3.Lerp(projectile.from, projectile.to, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        foreach (EnemyMoveVisual move in moves)
        {
            move.enemy.pos = run.enemies.Find(e => e.id == move.enemy.id)?.pos ?? move.enemy.pos;
            animatedEnemies.Add(move.enemy);
            Destroy(move.rect.gameObject);
        }
        foreach (ProjectileVisual projectile in projectiles) Destroy(projectile.rect.gameObject);
        impactCells.Clear();
        RefreshBattle();
    }

    private IEnumerator AnimatePush(KaitTurnResult result)
    {
        KaitEnemy enemy = animatedEnemies?.Find(e => e.pos == result.pushFrom && e.life != KaitEnemyLife.Dead);
        if (enemy == null || !InsideBattle(result.pushTo)) yield break;
        KaitEnemy resolved = run.enemies.Find(e => e.id == enemy.id);
        if (resolved != null) enemy.hp = resolved.hp;
        EnemySpineView pushedView = EnemySpine(enemy);
        RectTransform token = pushedView != null
            ? pushedView.Root
            : CreateFloatingPortrait(EnemyPortrait(enemy.type), Color.clear, battleCells[result.pushFrom.x + result.pushFrom.y * KaitRun.BattleSize].rectTransform, new Vector2(115, 115), enemy.hp, enemy.maxHp);
        animatedEnemies.Remove(enemy);
        RefreshBattle();
        Vector3 from = battleCells[result.pushFrom.x + result.pushFrom.y * KaitRun.BattleSize].rectTransform.position;
        Vector3 to = battleCells[result.pushTo.x + result.pushTo.y * KaitRun.BattleSize].rectTransform.position;
        if (pushedView != null)
        {
            pushedView.SetParent(canvas.transform);
            pushedView.Root.position = from;
            pushedView.Root.SetAsLastSibling();
            pushedView.SetVisible(true);
            pushedView.SetTint(Gold);
            pushedView.PlayDamage();
        }
        else
        {
            Transform portrait = token.Find("Portrait");
            if (portrait != null) portrait.GetComponent<Image>().color = Gold;
        }
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
        if (pushedView != null)
        {
            pushedView.SetTint(Color.white);
            pushedView.SetVisible(false);
        }
        else Destroy(token.gameObject);
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
            if (enemy.intent.type == KaitIntentType.CrossBlast && enemy.intent.affectedCells.Contains(p)) return Hex("#B05A70");
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
        Text text = MakeText(label, image.transform, Vector2.zero, size, fontSize, color.grayscale < 0.55f ? Cream : Void, TextAnchor.MiddleCenter, FontStyle.Bold, false);
        Stretch(text.rectTransform, 2);
        return rect;
    }

    private RectTransform CreateFloatingPortrait(Sprite portraitSprite, Color background, RectTransform source, Vector2 size, int hp = -1, int maxHp = -1)
    {
        Image image = Rect("Animation Unit", canvas.transform, Vector2.zero, size, background);
        image.rectTransform.position = source.position;
        image.rectTransform.SetAsLastSibling();
        Image portrait = Rect("Portrait", image.transform, new Vector2(0, -2), size * 0.96f, Color.white);
        portrait.sprite = portraitSprite;
        portrait.type = Image.Type.Simple;
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        if (hp >= 0 && maxHp > 0)
        {
            HealthBarView hpBar = MakeHealthBar(image.transform, new Vector2(0f, -size.y * 0.43f), UnitHealthBarSize);
            SetHealthBar(hpBar, hp);
        }
        return image.rectTransform;
    }

    private KaitSpineView CreateFloatingKait(RectTransform source, KaitDirection direction)
        => CreateFloatingKait(source, direction, new Vector2(115, 115), "Kait Spine Animation");

    private KaitSpineView CreateFloatingKait(RectTransform source, KaitDirection direction, Vector2 size, string name)
    {
        if (makotoSkeletonData == null) return null;
        KaitSpineView view = KaitSpineView.Create(makotoSkeletonData, canvas.transform, size, name);
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

    private RectTransform CreateGhostToken(RectTransform source, int momentumValue, KaitDirection direction)
    {
        float tier = Mathf.Clamp01((momentumValue - 1) / 4f);
        Color borderColor = Color.Lerp(new Color(Peach.r, Peach.g, Peach.b, 0.42f), new Color(Coral.r, Coral.g, Coral.b, 0.72f), tier);
        Image border = Rect("Kait Speed Trail", canvas.transform, Vector2.zero, new Vector2(88, 88), borderColor);
        border.raycastTarget = false;
        border.rectTransform.position = source.position;
        border.rectTransform.SetAsLastSibling();
        Image inside = Rect("Trail Inner", border.transform, Vector2.zero, new Vector2(78, 78), new Color(Panel.r, Panel.g, Panel.b, 0.48f));
        inside.raycastTarget = false;
        KaitSpineView ghost = makotoSkeletonData == null ? null : KaitSpineView.Create(makotoSkeletonData, inside.transform, new Vector2(78, 78), "Kait Trail Character");
        if (ghost != null)
        {
            ghost.Face(direction);
            ghost.PlayLoop(KaitSpineView.Run);
            ghost.SetOpacity(Mathf.Lerp(0.42f, 0.68f, tier));
            return border.rectTransform;
        }
        return border.rectTransform;
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

    private IEnumerator AnimateCombatFeedback(KaitTurnResult result, List<KaitEnemy> healthBefore, int kateHpBefore)
    {
        bool hasImpact = false;
        int longestHealthLoss = 0;
        foreach (KaitEnemy before in healthBefore)
        {
            KaitEnemy resolved = run.enemies.Find(e => e.id == before.id);
            int afterHp = resolved == null || resolved.life == KaitEnemyLife.Dead ? 0 : resolved.hp;
            if (before.hp <= afterHp) continue;
            KaitEnemy visual = animatedEnemies?.Find(e => e.id == before.id);
            Vector2Int cell = visual != null ? visual.pos : before.pos;
            if (!InsideBattle(cell)) continue;
            HealthBarView bar = battleHealthBars[cell.x + cell.y * KaitRun.BattleSize];
            StartCoroutine(AnimateHealthLoss(bar, before.hp, afterHp, afterHp <= 0));
            if (afterHp > 0) StartCoroutine(FlashEnemyWhite(before.id));
            longestHealthLoss = Mathf.Max(longestHealthLoss, before.hp - afterHp);
        }
        if (result.damageDealt > 0 && InsideBattle(result.blockedEnemyCell))
        {
            EnemySpineView damagedEnemy = EnemySpine(result.damagedEnemyId);
            KaitEnemy resolvedEnemy = run.enemies.Find(e => e.id == result.damagedEnemyId);
            if (resolvedEnemy != null && resolvedEnemy.life != KaitEnemyLife.Dead) damagedEnemy?.PlayDamage();
            StartCoroutine(FloatDamage(result.blockedEnemyCell, result.damageDealt, Coral));
            hasImpact = true;
        }
        if (result.playerDamage > 0)
        {
            kaitSpine?.PlayOnce(run.kateHp <= 0 ? KaitSpineView.Die : KaitSpineView.Damage, run.kateHp <= 0 ? null : KaitSpineView.Idle);
            StartCoroutine(FlashKaitWhite());
            StartCoroutine(FloatDamage(run.katePos, result.playerDamage, Gold));
            StartCoroutine(AnimateHealthLoss(runHealthBar, kateHpBefore, run.kateHp, false));
            longestHealthLoss = Mathf.Max(longestHealthLoss, kateHpBefore - run.kateHp);
            hasImpact = true;
        }
        if (longestHealthLoss > 0) yield return new WaitForSecondsRealtime(longestHealthLoss * 0.15f);
        else if (hasImpact) yield return new WaitForSecondsRealtime(0.055f);
    }

    private IEnumerator AnimateEnemyDeaths(List<KaitEnemy> healthBefore)
    {
        var deadIds = new HashSet<int>();
        float longestDuration = 0f;
        foreach (KaitEnemy before in healthBefore)
        {
            KaitEnemy resolved = run.enemies.Find(e => e.id == before.id);
            if (resolved == null || resolved.life != KaitEnemyLife.Dead || before.life == KaitEnemyLife.Dead) continue;
            deadIds.Add(before.id);
            EnemySpineView view = EnemySpine(before.id);
            if (view == null) continue;
            view.PlayDeath();
            longestDuration = Mathf.Max(longestDuration, view.DeathDuration);
        }
        if (deadIds.Count == 0) yield break;
        if (longestDuration > 0f) yield return new WaitForSecondsRealtime(longestDuration);
        animatedEnemies?.RemoveAll(e => deadIds.Contains(e.id));
        RefreshBattle();
    }

    private IEnumerator AnimateEnemyAttackPreparation()
    {
        float longestDuration = 0f;
        foreach (KaitEnemy enemy in run.enemies)
        {
            if (enemy.life != KaitEnemyLife.Active || enemy.intent.type == KaitIntentType.None) continue;
            EnemySpineView view = EnemySpine(enemy);
            if (view == null) continue;
            view.PlayPrepareAttack();
            longestDuration = Mathf.Max(longestDuration, view.PrepareAttackDuration);
        }
        if (longestDuration > 0f)
            yield return new WaitForSecondsRealtime(Mathf.Min(longestDuration, 0.4f));
    }

    private IEnumerator AnimateHealthLoss(HealthBarView bar, int before, int after, bool hideWhenDone)
    {
        if (bar == null) yield break;
        bar.root.gameObject.SetActive(true);
        SetHealthBar(bar, before);
        for (int hp = before; hp > after; hp--)
        {
            int slot = (hp - 1) % 3;
            Image segment = bar.fills[slot];
            segment.gameObject.SetActive(true);
            segment.color = Hex("#E04444");
            yield return new WaitForSecondsRealtime(0.11f);
            SetHealthBar(bar, hp - 1);
            yield return new WaitForSecondsRealtime(0.04f);
        }
        SetHealthBar(bar, after);
        if (hideWhenDone) bar.root.gameObject.SetActive(false);
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

    private void TriggerChainShake(int chainCount)
    {
        if (chainCount < 2 || gameContent == null) return;
        if (screenShakeRoutine != null)
        {
            StopCoroutine(screenShakeRoutine);
            gameContent.anchoredPosition = gameContentBasePosition;
        }
        screenShakeRoutine = StartCoroutine(ShakeGameContent(chainCount));
    }

    private IEnumerator ShakeGameContent(int chainCount)
    {
        float intensity = Mathf.InverseLerp(2f, 10f, Mathf.Clamp(chainCount, 2, 10));
        float magnitude = Mathf.Lerp(2.2f, 4.2f, intensity);
        float duration = Mathf.Lerp(0.10f, 0.14f, intensity);
        float elapsed = 0f;
        while (elapsed < duration && gameContent != null)
        {
            float t = elapsed / duration;
            float fade = 1f - t;
            float phase = t * Mathf.PI * 8f;
            Vector2 offset = new Vector2(Mathf.Sin(phase), Mathf.Cos(phase * 1.3f) * 0.65f) * magnitude * fade;
            gameContent.anchoredPosition = gameContentBasePosition + offset;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        if (gameContent != null) gameContent.anchoredPosition = gameContentBasePosition;
        screenShakeRoutine = null;
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
            yield return PulseBattleUnit(run.katePos, Coral, 0.15f, -1, true);
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

    private IEnumerator PulseBattleUnit(Vector2Int cell, Color color, float duration, int expectedEnemyId = -1, bool requireKait = false)
    {
        if (!InsideBattle(cell)) yield break;
        int index = cell.x + cell.y * KaitRun.BattleSize;
        KaitEnemy enemy = expectedEnemyId >= 0
            ? animatedEnemies?.Find(e => e.id == expectedEnemyId) ?? run.enemies.Find(e => e.id == expectedEnemyId)
            : requireKait ? null : EnemyAtVisual(cell);
        bool isKate = !hideKate && (displayKate ?? run.katePos) == cell && expectedEnemyId < 0;
        if (expectedEnemyId >= 0 && (enemy == null || enemy.pos != cell)) yield break;
        if (requireKait && !isKate) yield break;
        if (enemy == null && !isKate) yield break;

        Color original = Color.white;
        if (enemy != null)
        {
            original = enemy.frozenActions > 0
                ? new Color(0.62f, 0.9f, 1f, 1f)
                : enemy.life == KaitEnemyLife.Preparing
                    ? new Color(1f, 1f, 1f, 0.68f)
                    : Color.white;
            EnemySpineView view = EnemySpine(enemy);
            if (view != null) view.SetTint(color);
            else if (battlePortraits[index] != null) battlePortraits[index].color = color;
        }
        else
        {
            kaitSpine?.SetTint(color);
            if (kaitSpine == null && battlePortraits[index] != null) battlePortraits[index].color = color;
        }

        yield return ScalePulse(battleUnitClips[index].rectTransform, 0.92f, 1.16f, duration);

        if (enemy != null)
        {
            EnemySpineView view = EnemySpine(enemy);
            if (view != null) view.SetTint(original);
            else if (battlePortraits[index] != null) battlePortraits[index].color = original;
        }
        else
        {
            kaitSpine?.SetTint(Color.white);
            if (kaitSpine == null && battlePortraits[index] != null) battlePortraits[index].color = Color.white;
        }
    }

    private IEnumerator FlashEnemyWhite(int enemyId)
    {
        EnemySpineView view = EnemySpine(enemyId);
        if (view == null) yield break;
        view.SetHitFlash(1f);
        yield return new WaitForSecondsRealtime(0.075f);
        view.SetHitFlash(0f);
    }

    private IEnumerator FlashKaitWhite()
    {
        if (kaitSpine == null) yield break;
        kaitSpine.SetHitFlash(1f);
        yield return new WaitForSecondsRealtime(0.075f);
        kaitSpine.SetHitFlash(0f);
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

    private static Sprite LoadPortraitSprite(string resourcePath, Rect normalizedRect)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null) return null;
        texture.filterMode = FilterMode.Bilinear;
        Rect pixels = new Rect(
            normalizedRect.x * texture.width,
            normalizedRect.y * texture.height,
            normalizedRect.width * texture.width,
            normalizedRect.height * texture.height);
        return Sprite.Create(texture, pixels, new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite LoadUiSprite(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null) return null;
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite LoadSlicedPixelSprite(string resourcePath, float border)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null) return null;
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            16f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
    }

    private static Sprite LoadSlicedAtlasSprite(string resourcePath, Rect pixels, float border)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null) return null;
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(
            texture,
            pixels,
            new Vector2(0.5f, 0.5f),
            16f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
    }

    private static Sprite LoadTiledPixelSprite(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null) return null;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite EnemyPortrait(KaitEnemyType type)
    {
        if (type == KaitEnemyType.Grunt) return gruntPortrait;
        if (type == KaitEnemyType.Swordsman) return swordsmanPortrait;
        if (type == KaitEnemyType.Archer) return archerPortrait;
        if (type == KaitEnemyType.Guard) return guardPortrait;
        if (type == KaitEnemyType.ShieldKnight) return bossPortrait;
        return warlockPortrait;
    }

    private void LoadEnemySkeleton(KaitEnemyType type, string assetId)
    {
        SkeletonDataAsset data = Resources.Load<SkeletonDataAsset>($"Characters/Enemies/{assetId}/{assetId}_SkeletonData");
        if (data != null) enemySkeletonData[type] = data;
    }

    private EnemySpineView EnemySpine(KaitEnemy enemy)
    {
        if (enemy == null) return null;
        if (enemySpines.TryGetValue(enemy.id, out EnemySpineView existing)) return existing;
        if (!enemySkeletonData.TryGetValue(enemy.type, out SkeletonDataAsset data) || data == null) return null;
        float visualScale = enemy.type == KaitEnemyType.Guard ? 1.1f : 1f;
        EnemySpineView created = EnemySpineView.Create(data, EnemyAnimationPrefix(enemy.type), canvas.transform, new Vector2(115, 115), $"Enemy {enemy.id} Spine", visualScale);
        if (created != null) enemySpines[enemy.id] = created;
        return created;
    }

    private EnemySpineView EnemySpine(int enemyId)
    {
        if (enemySpines.TryGetValue(enemyId, out EnemySpineView existing)) return existing;
        KaitEnemy enemy = animatedEnemies?.Find(e => e.id == enemyId) ?? run.enemies.Find(e => e.id == enemyId);
        return EnemySpine(enemy);
    }

    private void ClearEnemySpines()
    {
        foreach (EnemySpineView view in enemySpines.Values) view.Destroy();
        enemySpines.Clear();
    }

    private static string EnemyAnimationPrefix(KaitEnemyType type)
    {
        if (type == KaitEnemyType.Grunt) return "01_";
        if (type == KaitEnemyType.Swordsman) return "04_";
        if (type == KaitEnemyType.Archer) return "08_";
        if (type == KaitEnemyType.Guard) return "06_";
        if (type == KaitEnemyType.Warlock) return "26_";
        return "05_";
    }

    private static Color EnemyTileColor(KaitEnemyType type)
    {
        if (type == KaitEnemyType.Grunt) return ThreatColor(4);
        if (type == KaitEnemyType.Swordsman) return ThreatColor(8);
        if (type == KaitEnemyType.Archer) return ThreatColor(16);
        if (type == KaitEnemyType.Guard) return ThreatColor(32);
        if (type == KaitEnemyType.Warlock) return ThreatColor(64);
        if (type == KaitEnemyType.ShieldKnight) return ThreatColor(128);
        return ThreatColor(4);
    }

    private static float HalfArrowAngle(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return 90f;
        if (direction == Vector2Int.left) return 180f;
        if (direction == Vector2Int.down) return -90f;
        return 0f;
    }

    private Text MakeText(string value, Transform parent, Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal, bool addOutline = true)
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
        if (fontSize >= 8)
        {
            // Bordered UI may shrink long localized strings, but never enlarge
            // short labels beyond the intended visual hierarchy.
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(8, Mathf.RoundToInt(fontSize * 0.58f));
            text.resizeTextMaxSize = fontSize;
        }
        text.alignByGeometry = anchor == TextAnchor.MiddleCenter;
        if (addOutline && fontSize >= 8)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.035f, 0.027f, 0.035f, 0.94f);
            float thickness = fontSize >= 24 ? 1.5f : 1f;
            outline.effectDistance = new Vector2(thickness, -thickness);
            outline.useGraphicAlpha = true;
        }
        return text;
    }

    private Button MakeButton(Transform parent, Vector2 position, Vector2 size, string label)
    {
        Image image = Rect("Button", parent, position, size, PanelLight);
        if (dungeonButtonSprite != null)
        {
            image.sprite = dungeonButtonSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }
        Button button = image.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.SpriteSwap;
        SpriteState sprites = button.spriteState;
        sprites.highlightedSprite = dungeonButtonSprite;
        sprites.selectedSprite = dungeonButtonSprite;
        sprites.pressedSprite = dungeonButtonPressedSprite;
        sprites.disabledSprite = dungeonButtonSprite;
        button.spriteState = sprites;
        Text text = MakeText(label, image.transform, Vector2.zero, size - new Vector2(8, 8), 17, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        Stretch(text.rectTransform, 10);
        return button;
    }

    private void SkinPanel(Image image)
    {
        if (image == null || dungeonPanelSprite == null) return;
        image.sprite = dungeonPanelSprite;
        image.type = Image.Type.Sliced;
        image.color = Color.white;
    }

    private HealthBarView MakeHealthBar(Transform parent, Vector2 position, Vector2 size)
    {
        Image root = Rect("Health Bar", parent, position, size, Color.clear);
        root.sprite = null;
        root.type = Image.Type.Simple;
        root.raycastTarget = false;
        var view = new HealthBarView { root = root };
        float segmentWidth = size.x / 3f;
        for (int i = 0; i < 3; i++)
        {
            Vector2 segmentPosition = new Vector2((i - 1) * segmentWidth, 0f);
            Image slot = Rect($"Health Slot {i + 1}", root.transform, segmentPosition, new Vector2(segmentWidth, size.y), Color.white);
            slot.sprite = healthSlotSprites[i];
            slot.type = Image.Type.Simple;
            slot.preserveAspect = false;
            slot.raycastTarget = false;

            Image fill = Rect($"Health Segment {i + 1}", root.transform, segmentPosition, new Vector2(segmentWidth, size.y), Color.white);
            fill.sprite = healthFillSprites[i];
            fill.type = Image.Type.Simple;
            fill.preserveAspect = false;
            fill.raycastTarget = false;
            view.fills[i] = fill;
        }
        return view;
    }

    private static void SetHealthBar(HealthBarView bar, int current)
    {
        if (bar == null) return;
        for (int i = 0; i < bar.fills.Length; i++)
        {
            int layer = (current - (i + 1)) / 3;
            bool visible = current >= i + 1;
            bar.fills[i].gameObject.SetActive(visible);
            if (!visible) continue;
            bar.fills[i].color = layer >= 2
                ? Hex("#C5CEDA")
                : layer == 1
                    ? Hex("#B97852")
                    : Hex("#65B84F");
        }
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
        if (type == KaitEnemyType.Warlock) return Hex("#6E527F");
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

    private static Color BattleTint(Color color)
    {
        color.a = 0.68f;
        return color;
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color color);
        return color;
    }
}
