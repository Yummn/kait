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
    private const int TargetFrameRate = 60;
    private const float SwipeThresholdScreenFraction = 0.06f;
    private const float MinimumSwipeDistancePixels = 48f;
    private const float EnemyHitFlashDuration = 0.075f;
    private const float KaitAttackImpactLead = 0.09f;
    private const float EnemyAttackReleaseLead = 0.1f;
    private const float PostImpactHold = 0.06f;
    private const float EnemyMoveDuration = 0.2f;
    private const float WorldStyleTopSplit = 0.563f;
    private const float WorldStyleBottomSplit = 0.447f;
    private const float ContentWidth = 1600f;
    private const float ContentHeight = 900f;
    private const string DisableThreatPillarsPreference = "Kait.DisableThreatPillars";
    private const string PlayerInvinciblePreference = "Kait.PlayerInvincible";
    private const string DisableRiftDamagePreference = "Kait.DisableRiftDamage";
    private const string DisableFriendlyFirePreference = "Kait.DisableFriendlyFire";
    private const string DisableCollisionDamagePreference = "Kait.DisableCollisionDamage";

    private static KaitGame instance;
    private readonly KaitRun run = new KaitRun();
    private Font uiFont;
    private Font threatBoardFont;
    private Canvas canvas;
    private RectTransform gameContent;
    private Coroutine screenShakeRoutine;
    private Coroutine kaitSkillAnimationRoutine;
    private Vector2 gameContentBasePosition;
    private Sprite roundedSprite;
    private Image[] battleCells;
    private Image[] battleCellTiles;
    private Image[] battleCellTints;
    private Image[] battleUnitClips;
    private RectTransform[] battleDeathLayers;
    private RectTransform battleUnderEffectLayer;
    private RectTransform battleActorLayer;
    private RectTransform battleEnemyHitLayer;
    private RectTransform battleKaitLayer;
    private RectTransform battleDangerLayer;
    private RectTransform battleEffectLayer;
    private Text[] battleLabels;
    private Image[] battlePortraits;
    private HealthBarView[] battleHealthBars;
    private Text[] battleFacingLabels;
    private Text[] battleStatusLabels;
    private Image[] battleWarningLines;
    private KaitMageEffectGraphic[] battleMageWarnings;
    private readonly HashSet<int> firingWarlocks = new HashSet<int>();
    private readonly List<KaitMageEffectGraphic> mageImpacts = new List<KaitMageEffectGraphic>();
    private Image[] battleRifts;
    private Image[] battleRiftDangerIcons;
    private Image[] threatCells;
    private Text[] threatLabels;
    private Text turnText;
    private HealthBarView runHealthBar;
    private Text statusText;
    private KaitSkillDeck skillDeck;
    private KaitPassiveDeck passiveDeck;
    private GlobalStyleSplit styleSplit;
    private GameObject controlsPanel;
    private KaitSkill targetingSkill;
    private GameObject endOverlay;
    private Text endText;
    private bool busy;
    private Vector2Int? displayKate;
    private bool hideKate;
    private List<KaitEnemy> animatedEnemies;
    private List<KaitSpawnRequest> animatedSpawns;
    private int[,] displayedThreat;
    private bool hideThreatValues;
    private readonly HashSet<Vector2Int> impactCells = new HashSet<Vector2Int>();
    private GameObject tutorialOverlay;
    private GameObject settingsOverlay;
    private Toggle disableThreatPillarsToggle;
    private Toggle playerInvincibleToggle;
    private Toggle disableRiftDamageToggle;
    private Toggle disableFriendlyFireToggle;
    private Toggle disableCollisionDamageToggle;
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
    private Sprite spawnRiftSprite;
    private Sprite riftDangerWarningSprite;
    private Sprite dungeonPanelSprite;
    private Sprite dungeonButtonSprite;
    private Sprite dungeonButtonPressedSprite;
    private readonly Sprite[] healthFillSprites = new Sprite[3];
    private readonly Sprite[] healthSlotSprites = new Sprite[3];
    private Sprite grassBackgroundSprite;
    private RectTransform gardenMapping;
    private KaitLayeredGarden layeredGarden;
    private GameObject[] battleObstacleShadows;
    private Image[] battleObstacles;
    private static readonly Color CanopyTint = new Color(0.18f, 0.29f, 0.25f, 0.25f);
    private SkeletonDataAsset arrowProjectileEffect;
    private SkeletonDataAsset shadowSmokeEffect;
    private Texture2D swordSlashTexture;
    private readonly Dictionary<KaitEnemyType, SkeletonDataAsset> enemySkeletonData = new Dictionary<KaitEnemyType, SkeletonDataAsset>();
    private readonly Dictionary<int, EnemySpineView> enemySpines = new Dictionary<int, EnemySpineView>();
    private readonly Dictionary<int, float> enemyDeathAnimationStartedAt = new Dictionary<int, float>();
    private readonly List<EnemySpineView> detachedEnemyDeaths = new List<EnemySpineView>();
    private readonly List<KaitSpineView> activeFloatingKaits = new List<KaitSpineView>();
    private readonly List<KaitTrailVisual> activeTrailVisuals = new List<KaitTrailVisual>();
    private readonly List<SpineEffectView> activeEffectViews = new List<SpineEffectView>();
    private readonly List<KaitCombatEffectGraphic> activeCombatEffects = new List<KaitCombatEffectGraphic>();
    private readonly List<RawImage> activeSwordSlashEffects = new List<RawImage>();
    private int swipeFingerId = -1;
    private Vector2 swipeStartPosition;
    private bool swipeTriggered;
    private bool swipeStartedOverButton;
    private string logPath;
    private int announcedSkillMilestone;
    private int announcedPassiveMilestone;
    private bool endAudioPlayed;
    private int kaitDefeatingEnemyId = -1;
    private KaitEnemyType kaitDefeatingEnemyType;

    private sealed class ThreatVisual
    {
        public RectTransform rect;
        public Vector3 from;
        public Vector3 to;
    }

    private sealed class EnemyMoveVisual
    {
        public KaitCombatEffectGraphic pressure;
        public KaitEnemy enemy;
        public RectTransform rect;
        public Vector3 from;
        public Vector3 to;
    }

    private sealed class ProjectileVisual
    {
        public RectTransform rect;
        public SpineEffectView spine;
        public Vector3 from;
        public Vector3 to;
    }

    private sealed class KaitTrailVisual
    {
        public RectTransform rect;
        public KaitSpineView spine;
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
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;
        if (instance != null) return;
        var host = new GameObject("Kait Game Runtime");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<KaitGame>();
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        run.config.enableThreatPillars = PlayerPrefs.GetInt(DisableThreatPillarsPreference, 0) == 0;
        run.config.playerInvincible = PlayerPrefs.GetInt(PlayerInvinciblePreference, 0) != 0;
        run.config.enableRiftDamage = PlayerPrefs.GetInt(DisableRiftDamagePreference, 0) == 0;
        run.config.enableFriendlyFire = PlayerPrefs.GetInt(DisableFriendlyFirePreference, 0) == 0;
        run.config.enableCollisionDamage = PlayerPrefs.GetInt(DisableCollisionDamagePreference, 0) == 0;
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
        dungeonFloorSprite = KaitSunlitTheme.Load("Floor");
        dungeonWallSprite = KaitSunlitTheme.Load("Pillar");
        spawnRiftSprite = KaitSunlitTheme.Load("Rift");
        riftDangerWarningSprite = LoadUiSprite("KaitVisuals/RiftDangerWarning");
        dungeonPanelSprite = KaitSunlitTheme.Load("Panel", 0.10f, 160f);
        dungeonButtonSprite = KaitSunlitTheme.Load("Button", 0.15f, 80f);
        // Shared button state supplies the darkening and press/release scale.
        dungeonButtonPressedSprite = dungeonButtonSprite;
        healthFillSprites[0] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthFillLeft");
        healthFillSprites[1] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthFillMiddle");
        healthFillSprites[2] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthFillRight");
        healthSlotSprites[0] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthSlotLeft");
        healthSlotSprites[1] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthSlotMiddle");
        healthSlotSprites[2] = LoadPixelSprite("KaitVisuals/DungeonUI/HealthSlotRight");
        grassBackgroundSprite = KaitSunlitTheme.Load(KaitLayeredGarden.ArtReady ? "GrassBase" : "Garden");
        LoadEnemySkeleton(KaitEnemyType.Grunt, "100161");
        LoadEnemySkeleton(KaitEnemyType.Swordsman, "105731");
        LoadEnemySkeleton(KaitEnemyType.Archer, "106331");
        LoadEnemySkeleton(KaitEnemyType.Guard, "112731");
        LoadEnemySkeleton(KaitEnemyType.Warlock, "111031");
        LoadEnemySkeleton(KaitEnemyType.ShieldKnight, "104731");
        arrowProjectileEffect = LoadEffectSkeleton("huangzhong_atk_zd_1");
        shadowSmokeEffect = LoadEffectSkeleton("Buff_Effect_yinni_1");
        swordSlashTexture = Resources.Load<Texture2D>("KaitVisuals/Effects/KaitSwordSlashSheet");
    }

    private void OnDestroy()
    {
        ClearFloatingKaitAnimations();
        ClearAllTrailVisuals();
        ClearAllEffectViews();
        ClearAllCombatEffects();
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
        if (CommandLineValue("-kaitGrowthPreview") == "1") PrepareGrowthPreview();
        string cardsPreview = CommandLineValue("-kaitCardsPreview");
        if (!string.IsNullOrEmpty(cardsPreview))
        {
            if (cardsPreview == "choose") PrepareGrowthPreview();
            else
            {
                run.passives.Add(KaitPassive.BirdEye);
                run.passives.Add(KaitPassive.BloodBookmark);
                run.passives.Add(KaitPassive.SweepTail);
                RefreshAll();
                if (cardsPreview == "cross") passiveDeck.Owned[0].PreviewAt(new Vector2(0, 40));
            }
        }
        string skillPreview = CommandLineValue("-kaitSkillCardsPreview");
        if (!string.IsNullOrEmpty(skillPreview))
        {
            if (skillPreview == "choose") PrepareGrowthPreview();
            else
            {
                run.skills.Add(KaitSkill.SwiftBoots);
                run.skills.Add(KaitSkill.IceTomb);
                run.skills.Add(KaitSkill.CatAgility);
                run.passives.Add(KaitPassive.BirdEye);
                run.passives.Add(KaitPassive.BloodBookmark);
                if (skillPreview == "cooldown")
                {
                    run.TryUseSkill(KaitSkill.SwiftBoots, -1, out _);
                    run.TryUseSkill(KaitSkill.CatAgility, -1, out _);
                }
                RefreshAll();
                if (skillPreview == "drag") skillDeck.Owned[0].PreviewAt(KaitSkillDeck.CastZone.center);
            }
        }
        string logoPreview = CommandLineValue("-kaitLogoPreview");
        if (!string.IsNullOrEmpty(logoPreview)) PrepareCardLogoPreview(logoPreview);
        string screenshotPath = CommandLineValue("-kaitScreenshot");
        if (Application.platform == RuntimePlatform.WindowsPlayer && string.IsNullOrEmpty(screenshotPath))
            StartCoroutine(EnforceWindowsFullscreen());
        string demoStepsValue = CommandLineValue("-kaitDemoSteps");
        if (CommandLineValue("-kaitTutorial") == "1")
        {
            tutorialOverlay.SetActive(true);
            tutorialOverlay.transform.SetAsLastSibling();
            if (int.TryParse(CommandLineValue("-kaitTutorialPage"), out int tutorialPage))
                tutorialOverlay.GetComponent<KaitTutorialBook>().ShowPage(tutorialPage - 1);
        }
        if (CommandLineValue("-kaitSettings") == "1")
        {
            tutorialOverlay.SetActive(false);
            settingsOverlay.SetActive(true);
            settingsOverlay.transform.SetAsLastSibling();
        }
        int.TryParse(demoStepsValue, out int demoSteps);
        string vfxPreview = CommandLineValue("-kaitVfxPreview");
        if (skillPreview == "preview") StartCoroutine(PreviewSkillCardReveal(screenshotPath));
        else if (skillPreview == "cast") StartCoroutine(PreviewSkillCardCast(screenshotPath));
        else if (!string.IsNullOrEmpty(vfxPreview)) StartCoroutine(PreviewCombatVfx(vfxPreview, screenshotPath));
        else if (!string.IsNullOrEmpty(screenshotPath)) StartCoroutine(CaptureAndQuit(screenshotPath, demoSteps));
    }

    private void PrepareCardLogoPreview(string mode)
    {
        var areaObject = new GameObject("Card Logo QA", typeof(RectTransform));
        areaObject.transform.SetParent(canvas.transform, false);
        var area = areaObject.GetComponent<RectTransform>(); area.sizeDelta = new Vector2(1920,1080);
        var split = styleSplit;
        if (mode == "all")
        {
            var backing = areaObject.AddComponent<Image>(); backing.color = new Color(.16f,.15f,.19f); backing.raycastTarget = false;
            split = areaObject.AddComponent<GlobalStyleSplit>(); split.Configure(area,1,1);
        }
        var skills = new[] { KaitSkill.SwiftBoots, KaitSkill.CatAgility, KaitSkill.DreadSlash, KaitSkill.IceTomb, KaitSkill.LesserPhantom, KaitSkill.ShadowStep };
        int count = mode == "all" ? 18 : 3;
        for (int i=0; i<count; i++)
        {
            Vector2 p = mode == "all" ? new Vector2((i%6-2.5f)*232, 318-(i/6)*310) : new Vector2((i-1)*260,80);
            if (mode != "all")
            {
                split.GetLocalSplits(area, out float bottomCut, out float topCut);
                float logoY = p.y + (mode == "passive" ? 62 : 51);
                float t = (logoY-area.rect.yMin)/area.rect.height;
                p.x += area.rect.xMin + area.rect.width * Mathf.LerpUnclamped(bottomCut,topCut,t);
            }
            if (i<6 && mode != "passive")
            {
                var card = KaitSkillCard.Create(area,split,threatBoardFont,KaitSunlitTheme.Load("SkillCardHD"),KaitSunlitTheme.Load("SkillCardFlat"),null,null,null);
                card.Show(mode == "all" ? skills[i] : KaitSkill.SwiftBoots,true,p,p.x);
                card.SetAvailability(true,0,false); card.PreviewAt(p);
            }
            else
            {
                var card = KaitPassiveCard.Create(area,split,threatBoardFont,KaitSunlitTheme.Load("PassiveCardBlankHD"),KaitSunlitTheme.Load("PassiveCardBlankFlat"),null,null);
                card.Show(mode == "passive" ? KaitPassive.BirdEye : KaitPassiveCatalog.All[i-6],true,p,p.x); card.PreviewAt(p);
            }
        }
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

    private void PrepareGrowthPreview()
    {
        for (int y = 0; y < run.ThreatSize; y++)
            for (int x = 0; x < run.ThreatSize; x++)
            {
                run.threat[x, y] = 0;
                run.threatTwoBirth[x, y] = 0;
            }
        run.threat[0, 0] = 8;
        run.threat[1, 0] = 8;
        run.TryGlobalInput(KaitDirection.Right);
        RefreshAll();
    }

    private void Update()
    {
        if (TutorialBlocksInput()) { ResetSwipeTracking(); return; }
        if (run.ended) return;
        HandleTouchSwipe();
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) HandleDirection(KaitDirection.Up);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) HandleDirection(KaitDirection.Down);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) HandleDirection(KaitDirection.Left);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) HandleDirection(KaitDirection.Right);
        else if (Input.GetKeyDown(KeyCode.R)) NewRun();
    }

    private bool TutorialBlocksInput()
    {
        return (tutorialOverlay != null && tutorialOverlay.activeInHierarchy) || KaitTutorialBook.ClosedFrame == Time.frameCount;
    }

    private void HandleTouchSwipe()
    {
        if (Input.touchCount == 0)
        {
            ResetSwipeTracking();
            return;
        }

        Touch trackedTouch = default;
        bool foundTrackedTouch = false;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (swipeFingerId < 0 && touch.phase == TouchPhase.Began)
            {
                swipeFingerId = touch.fingerId;
                swipeStartPosition = touch.position;
                swipeTriggered = false;
                swipeStartedOverButton = IsTouchOverButton(touch.position);
            }
            if (touch.fingerId != swipeFingerId) continue;
            trackedTouch = touch;
            foundTrackedTouch = true;
            break;
        }

        if (!foundTrackedTouch) return;

        if (!swipeTriggered && !swipeStartedOverButton &&
            (trackedTouch.phase == TouchPhase.Moved || trackedTouch.phase == TouchPhase.Ended))
        {
            Vector2 delta = trackedTouch.position - swipeStartPosition;
            float threshold = Mathf.Max(MinimumSwipeDistancePixels,
                Mathf.Min(Screen.width, Screen.height) * SwipeThresholdScreenFraction);
            if (delta.sqrMagnitude >= threshold * threshold)
            {
                swipeTriggered = true;
                KaitDirection direction = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                    ? (delta.x >= 0f ? KaitDirection.Right : KaitDirection.Left)
                    : (delta.y >= 0f ? KaitDirection.Up : KaitDirection.Down);
                HandleDirection(direction);
            }
        }

        if (trackedTouch.phase == TouchPhase.Ended || trackedTouch.phase == TouchPhase.Canceled)
            ResetSwipeTracking();
    }

    private static bool IsTouchOverButton(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;
        var eventData = new PointerEventData(EventSystem.current) { position = screenPosition };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, hits);
        foreach (RaycastResult hit in hits)
            if (hit.gameObject.GetComponentInParent<Button>() != null ||
                hit.gameObject.GetComponentInParent<KaitPassiveCard>() != null ||
                hit.gameObject.GetComponentInParent<KaitSkillCard>() != null) return true;
        return false;
    }

    private void ResetSwipeTracking()
    {
        swipeFingerId = -1;
        swipeTriggered = false;
        swipeStartedOverButton = false;
    }

    private void LateUpdate()
    {
        UpdateRiftDangerIcons();

        // The actor layer is outside the individual grid cells so later-drawn
        // neighbour tiles can never cover Kait's sword. Keep it locked to the
        // current cell after layout/fullscreen changes as well.
        if (kaitSpine == null || battleKaitLayer == null || battleCells == null) return;
        Vector2Int kate = displayKate ?? run.katePos;
        int index = kate.x + kate.y * KaitRun.BattleSize;
        if (index < 0 || index >= battleCells.Length || battleCells[index] == null) return;
        if (kaitSpine.Root.parent == battleKaitLayer)
            kaitSpine.Root.position = battleCells[index].rectTransform.position;
    }

    private void BuildUI()
    {
        var canvasGo = new GameObject("Kait Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvas.pixelPerfect = true;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image bg = Rect("Background", canvas.transform, Vector2.zero, new Vector2(1920, 1080), Background);
        if (grassBackgroundSprite != null)
        {
            Image garden = Rect("Emerald Garden", bg.transform, Vector2.zero, Vector2.zero, Color.white);
            garden.sprite = grassBackgroundSprite;
            garden.type = Image.Type.Simple;
            garden.raycastTarget = false;
            garden.rectTransform.anchorMin = Vector2.zero;
            garden.rectTransform.anchorMax = new Vector2(WorldStyleTopSplit, 1f);
            garden.rectTransform.offsetMin = garden.rectTransform.offsetMax = Vector2.zero;
            gardenMapping = garden.rectTransform;
            if (KaitLayeredGarden.ArtReady)
            {
                layeredGarden = bg.gameObject.AddComponent<KaitLayeredGarden>();
                layeredGarden.Initialize(gardenMapping);
            }
            AddGardenShadow(garden.transform, "Garden Shadow");
        }
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.sizeDelta = Vector2.zero;

        // The right half returns to the original flat presentation. The cut
        // deliberately crosses the middle column instead of dividing the two
        // boards with a straight vertical wall.
        // Continue the same diagonal used by the centre info/skill panels so
        // the pixel and flat halves read as one uninterrupted cut.
        AddDiagonalCut(bg.transform, "World Style Split", WorldStyleTopSplit, WorldStyleBottomSplit, Background, Peach, 6f);

        Text title = MakeText("Kait", bg.transform, new Vector2(-860, 500), new Vector2(180, 48), 32, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        title.font = threatBoardFont;
        MakeFlatButton(bg.transform, new Vector2(792, 460), new Vector2(110, 42), "玩法教程").onClick.AddListener(() =>
        {
            GameAudio.PlayClick();
            if (settingsOverlay != null) settingsOverlay.SetActive(false);
            tutorialOverlay.SetActive(true);
            tutorialOverlay.transform.SetAsLastSibling();
        });
        MakeFlatButton(bg.transform, new Vector2(902, 460), new Vector2(90, 42), "设置").onClick.AddListener(() =>
        {
            GameAudio.PlayClick();
            if (tutorialOverlay != null) tutorialOverlay.SetActive(false);
            settingsOverlay.SetActive(true);
            settingsOverlay.transform.SetAsLastSibling();
        });

        var contentGo = new GameObject("Game Content", typeof(RectTransform));
        contentGo.transform.SetParent(bg.transform, false);
        RectTransform content = contentGo.GetComponent<RectTransform>();
        content.sizeDelta = new Vector2(ContentWidth, ContentHeight);
        content.localScale = Vector3.one * 1.2f;
        gameContent = content;
        gameContentBasePosition = content.anchoredPosition;
        styleSplit = contentGo.AddComponent<GlobalStyleSplit>();
        styleSplit.Configure(content, WorldStyleBottomSplit, WorldStyleTopSplit);

        BuildBattleBoard(content);
        BuildThreatBoard(content);
        BuildSidebar(content);
        if (layeredGarden != null)
        {
            var decorationObject = new GameObject("Garden Decorations", typeof(RectTransform));
            decorationObject.transform.SetParent(bg.transform, false);
            var decorationRect = decorationObject.GetComponent<RectTransform>();
            Stretch(decorationRect, 0);
            layeredGarden.BuildDecorations(decorationRect, battleActorLayer);
            title.transform.SetAsLastSibling();
        }
        var cardsObject = new GameObject("Passive Cards", typeof(RectTransform), typeof(KaitPassiveDeck));
        cardsObject.transform.SetParent(bg.transform, false);
        RectTransform cardArea = cardsObject.GetComponent<RectTransform>();
        Stretch(cardArea, 0);
        passiveDeck = cardsObject.GetComponent<KaitPassiveDeck>();
        passiveDeck.Initialize(cardArea, styleSplit, threatBoardFont, ChoosePendingPassive);
        var skillsObject = new GameObject("Skill Cards", typeof(RectTransform), typeof(KaitSkillDeck));
        skillsObject.transform.SetParent(bg.transform, false);
        var skillArea = skillsObject.GetComponent<RectTransform>();
        Stretch(skillArea, 0);
        skillDeck = skillsObject.GetComponent<KaitSkillDeck>();
        skillDeck.Initialize(skillArea, styleSplit, threatBoardFont, ChoosePendingSkill, HandleSkillCardCast,
            () => { targetingSkill = KaitSkill.None; RefreshAll(); });
        BuildEndOverlay(bg.transform);
        BuildTutorialOverlay(bg.transform);
        BuildSettingsOverlay(bg.transform);
    }

    private void AddGardenShadow(Transform parent, string name)
    {
        // Keep the existing artwork usable if a decoration asset is missing.
        Texture coverage = layeredGarden != null ? (Texture)layeredGarden.ShadowField
            : Resources.Load<Texture2D>(KaitSunlitTheme.ResourceRoot + "CanopyMask");
        KaitGroundDecal.Create(parent, gardenMapping, coverage, CanopyTint, name, layeredGarden != null);
    }

    private void BuildBattleBoard(Transform parent)
    {
        var boardGo = new GameObject("Battle Board", typeof(RectTransform));
        boardGo.transform.SetParent(parent, false);
        RectTransform boardRect = boardGo.GetComponent<RectTransform>();
        boardRect.anchorMin = boardRect.anchorMax = boardRect.pivot = new Vector2(0.5f, 0.5f);
        boardRect.sizeDelta = new Vector2(600, 600);
        boardRect.anchoredPosition = new Vector2(-450, 0);
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
        battleMageWarnings = new KaitMageEffectGraphic[KaitRun.BattleSize * KaitRun.BattleSize];
        battleRifts = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleRiftDangerIcons = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleCellTiles = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleObstacles = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleObstacleShadows = new GameObject[KaitRun.BattleSize * KaitRun.BattleSize];
        battleCellTints = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleUnitClips = new Image[KaitRun.BattleSize * KaitRun.BattleSize];
        battleDeathLayers = new RectTransform[KaitRun.BattleSize * KaitRun.BattleSize];
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
                // All stones meet on the same level; rotate only the neutral
                // stone texture, never the shared canopy projection.
                tile.rectTransform.localRotation = Quaternion.Euler(0, 0, ((x * 3 + visualY) % 4) * 90f);
                battleCellTiles[index] = tile;
                AddGardenShadow(cell.transform, "Pavement Shadow");
                var contact = KaitSoftShadow.Create(cell.transform, "Stone Contact Shadow");
                contact.rectTransform.sizeDelta = new Vector2(105, 105);
                contact.rectTransform.anchoredPosition = new Vector2(3, -4);
                contact.Shape(3, 5);
                contact.color = new Color(.12f, .18f, .15f, .26f);
                battleObstacleShadows[index] = contact.gameObject;
                contact.gameObject.SetActive(false);
                Image obstacle = Rect("Top View Stone", cell.transform, Vector2.zero, new Vector2(96, 96), Color.white);
                obstacle.sprite = dungeonWallSprite;
                obstacle.type = Image.Type.Simple;
                obstacle.raycastTarget = false;
                AddGardenShadow(obstacle.transform, "Stone Shadow");
                battleObstacles[index] = obstacle;
                obstacle.gameObject.SetActive(false);
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
                var mageWarningObject = new GameObject("Mage Aim", typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitMageEffectGraphic));
                mageWarningObject.transform.SetParent(cell.transform, false);
                var mageWarning = mageWarningObject.GetComponent<KaitMageEffectGraphic>();
                Stretch(mageWarning.rectTransform, 0);
                mageWarning.ConfigureAim(false);
                mageWarningObject.SetActive(false);
                battleMageWarnings[index] = mageWarning;
                Image rift = Rect("Spawn Rift", cell.transform, Vector2.zero, new Vector2(108, 108), new Color(0.59f, 0.39f, 0.20f, 0.9f));
                rift.raycastTarget = false;
                rift.sprite = spawnRiftSprite;
                rift.type = Image.Type.Simple;
                rift.preserveAspect = true;
                rift.material = KaitGroundDecal.MaskMaterial;
                Image riftCore = Rect("Fracture Core", rift.transform, Vector2.zero, new Vector2(104, 104), Hex("#353039"));
                riftCore.sprite = spawnRiftSprite;
                riftCore.material = KaitGroundDecal.MaskMaterial;
                riftCore.raycastTarget = false;
                rift.gameObject.SetActive(false);
                // Mask-only fissure leaves both the paving and its canopy
                // shadow visible; attack warnings always render above it.
                rift.transform.SetSiblingIndex(warningClip.transform.GetSiblingIndex());
                battleRifts[index] = rift;
                var deathLayerObject = new GameObject("Death Visual Layer", typeof(RectTransform));
                deathLayerObject.transform.SetParent(cell.transform, false);
                RectTransform deathLayer = deathLayerObject.GetComponent<RectTransform>();
                Stretch(deathLayer, 0);
                battleDeathLayers[index] = deathLayer;
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
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);

        // Impact marks that should sit behind character art but above the board
        // tiles live in their own layer. Keeping this separate from the actor
        // layer also makes the ordering stable while actors are reparented.
        var underEffectLayerObject = new GameObject("Battle Under-Actor Effect Overlay", typeof(RectTransform));
        underEffectLayerObject.transform.SetParent(boardGo.transform, false);
        battleUnderEffectLayer = underEffectLayerObject.GetComponent<RectTransform>();
        battleUnderEffectLayer.anchorMin = battleUnderEffectLayer.anchorMax = battleUnderEffectLayer.pivot = new Vector2(0.5f, 0.5f);
        battleUnderEffectLayer.sizeDelta = boardRect.sizeDelta;
        battleUnderEffectLayer.anchoredPosition = Vector2.zero;

        var actorLayerObject = new GameObject("Battle Actor Overlay", typeof(RectTransform));
        actorLayerObject.transform.SetParent(boardGo.transform, false);
        battleActorLayer = actorLayerObject.GetComponent<RectTransform>();
        battleActorLayer.anchorMin = battleActorLayer.anchorMax = battleActorLayer.pivot = new Vector2(0.5f, 0.5f);
        battleActorLayer.sizeDelta = boardRect.sizeDelta;
        battleActorLayer.anchoredPosition = Vector2.zero;

        battleEnemyHitLayer = KaitCombatLayers.AddAbove(battleActorLayer, "Battle Enemy Hit Overlay");
        battleKaitLayer = KaitCombatLayers.AddAbove(battleEnemyHitLayer, "Battle Kait Overlay");

        var dangerLayerObject = new GameObject("Rift Danger Overlay", typeof(RectTransform));
        dangerLayerObject.transform.SetParent(boardGo.transform, false);
        battleDangerLayer = dangerLayerObject.GetComponent<RectTransform>();
        battleDangerLayer.anchorMin = battleDangerLayer.anchorMax = battleDangerLayer.pivot = new Vector2(0.5f, 0.5f);
        battleDangerLayer.sizeDelta = boardRect.sizeDelta;
        battleDangerLayer.anchoredPosition = Vector2.zero;
        for (int y = 1; y < KaitRun.BattleSize - 1; y++)
        {
            for (int x = 1; x < KaitRun.BattleSize - 1; x++)
            {
                int index = x + y * KaitRun.BattleSize;
                Image danger = Rect("Rift Damage Warning", battleDangerLayer, Vector2.zero, new Vector2(30f, 54f), Color.white);
                danger.sprite = riftDangerWarningSprite;
                danger.type = Image.Type.Simple;
                danger.preserveAspect = true;
                danger.raycastTarget = false;
                danger.gameObject.SetActive(false);
                battleRiftDangerIcons[index] = danger;
                PositionRiftDangerIcon(index);
            }
        }

        // Combat marks live above characters and danger icons. Actor refreshes
        // may reorder Spine objects, but can no longer cover hit/kill feedback.
        var effectLayerObject = new GameObject("Battle Effect Overlay", typeof(RectTransform));
        effectLayerObject.transform.SetParent(boardGo.transform, false);
        battleEffectLayer = effectLayerObject.GetComponent<RectTransform>();
        battleEffectLayer.anchorMin = battleEffectLayer.anchorMax = battleEffectLayer.pivot = new Vector2(0.5f, 0.5f);
        battleEffectLayer.sizeDelta = boardRect.sizeDelta;
        battleEffectLayer.anchoredPosition = Vector2.zero;
    }

    private void PositionRiftDangerIcon(int index)
    {
        if (battleDangerLayer == null || battleCells == null || battleRiftDangerIcons == null ||
            index < 0 || index >= battleCells.Length || battleCells[index] == null || battleRiftDangerIcons[index] == null) return;
        Vector3 localPosition = battleDangerLayer.InverseTransformPoint(battleCells[index].rectTransform.position);
        localPosition.y += 48f;
        localPosition.z = 0f;
        battleRiftDangerIcons[index].rectTransform.localPosition = localPosition;
    }

    private void UpdateRiftDangerIcons()
    {
        if (battleRiftDangerIcons == null) return;
        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.06f;
        for (int i = 0; i < battleRiftDangerIcons.Length; i++)
        {
            Image danger = battleRiftDangerIcons[i];
            if (danger == null || !danger.gameObject.activeSelf) continue;
            PositionRiftDangerIcon(i);
            danger.rectTransform.localScale = Vector3.one * pulse;
        }
    }

    private void BuildThreatBoard(Transform parent)
    {
        // Match the battle board's outer 600 x 600 footprint. The inner grid
        // keeps the original 2048 spacing ratio so the simple style remains intact.
        Image frame = Rect("Threat Panel", parent, new Vector2(450, 0), new Vector2(600, 600), Panel);
        var gridGo = new GameObject("Threat Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(frame.transform, false);
        RectTransform rt = gridGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(554, 554);
        rt.anchoredPosition = Vector2.zero;
        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(106, 106);
        grid.spacing = new Vector2(6, 6);
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
                threatLabels[index] = MakeText("", threatCells[index].transform, Vector2.zero, Vector2.zero, 36, Cream, TextAnchor.MiddleCenter, FontStyle.Bold, false);
                threatLabels[index].font = threatBoardFont;
                Stretch(threatLabels[index].rectTransform, 2);
            }
        }
    }

    private void BuildSidebar(Transform parent)
    {
        HybridStyleGraphic info = MakeHybridSurface("Run Info", parent, new Vector2(0, 300), new Vector2(250, 70),
            dungeonPanelSprite, Panel, 5f, 10f);
        turnText = MakeText("", info.transform, new Vector2(0, 14), new Vector2(220, 26), 16, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        MakeText("生命", info.transform, new Vector2(-86, -16), new Vector2(44, 20), 13, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        runHealthBar = MakeHealthBar(info.transform, new Vector2(8, -16), UnitHealthBarSize);
        statusText = MakeText("", parent, Vector2.zero, Vector2.zero, 1, Color.clear, TextAnchor.MiddleCenter);
        statusText.gameObject.SetActive(false);

        Vector2 controlsPosition = new Vector2(0, -210);

        Vector2 controlsSize = new Vector2(250, 220);
        HybridStyleGraphic controls = MakeHybridSurface("Controls", parent, controlsPosition, controlsSize,
            dungeonPanelSprite, Panel, 5f, 14f);
        controlsPanel = controls.gameObject;
        Vector2 keySize = new Vector2(58, 46);
        Vector2 upPosition = new Vector2(0, 65);
        Vector2 leftPosition = new Vector2(-62, 14);
        Vector2 downPosition = new Vector2(0, 14);
        Vector2 rightPosition = new Vector2(62, 14);
        Vector2 restartPosition = new Vector2(0, -57);
        Vector2 restartSize = new Vector2(182, 48);
        Button up = MakeHybridButton(controls.transform, upPosition, keySize, "W");
        Button left = MakeHybridButton(controls.transform, leftPosition, keySize, "A");
        Button down = MakeHybridButton(controls.transform, downPosition, keySize, "S");
        Button right = MakeHybridButton(controls.transform, rightPosition, keySize, "D");
        Button restart = MakeHybridButton(controls.transform, restartPosition, restartSize, "重新开始  R");
        up.onClick.AddListener(() => HandleDirection(KaitDirection.Up));
        left.onClick.AddListener(() => HandleDirection(KaitDirection.Left));
        down.onClick.AddListener(() => HandleDirection(KaitDirection.Down));
        right.onClick.AddListener(() => HandleDirection(KaitDirection.Right));
        restart.onClick.AddListener(() => { GameAudio.PlayClick(); NewRun(); });

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
        MakeButton(card.transform, new Vector2(0, -92), new Vector2(220, 60), "再来一局").onClick.AddListener(() =>
        {
            GameAudio.PlayClick();
            NewRun();
        });
        endOverlay.SetActive(false);
    }

    private void BuildTutorialOverlay(Transform parent)
    {
        tutorialOverlay = KaitTutorialBook.Create(parent, threatBoardFont, roundedSprite).gameObject;
    }

    private void BuildSettingsOverlay(Transform parent)
    {
        Image shade = Rect("Settings Overlay", parent, Vector2.zero, new Vector2(1600, 900),
            new Color(Void.r, Void.g, Void.b, 0.78f));
        Stretch(shade.rectTransform, 0);
        settingsOverlay = shade.gameObject;

        Image card = Rect("Settings Card", shade.transform, Vector2.zero, new Vector2(640, 650), Panel);
        SkinFlatPanel(card);
        Text title = MakeText("设置", card.transform, new Vector2(0, 270), new Vector2(520, 46), 28, Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold, false);
        title.font = threatBoardFont;

        disableThreatPillarsToggle = MakeFlatToggle(card.transform, new Vector2(0, 190), new Vector2(500, 62),
            "取消2048墙体");
        disableThreatPillarsToggle.SetIsOnWithoutNotify(!run.config.enableThreatPillars);
        disableThreatPillarsToggle.onValueChanged.AddListener(SetThreatPillarsDisabled);

        playerInvincibleToggle = MakeFlatToggle(card.transform, new Vector2(0, 118), new Vector2(500, 62),
            "人物无敌");
        playerInvincibleToggle.SetIsOnWithoutNotify(run.config.playerInvincible);
        playerInvincibleToggle.onValueChanged.AddListener(SetPlayerInvincible);

        disableRiftDamageToggle = MakeFlatToggle(card.transform, new Vector2(0, 46), new Vector2(500, 62),
            "去除裂隙伤害");
        disableRiftDamageToggle.SetIsOnWithoutNotify(!run.config.enableRiftDamage);
        disableRiftDamageToggle.onValueChanged.AddListener(SetRiftDamageDisabled);

        disableFriendlyFireToggle = MakeFlatToggle(card.transform, new Vector2(0, -26), new Vector2(500, 62),
            "去除敌人友伤");
        disableFriendlyFireToggle.SetIsOnWithoutNotify(!run.config.enableFriendlyFire);
        disableFriendlyFireToggle.onValueChanged.AddListener(SetFriendlyFireDisabled);

        disableCollisionDamageToggle = MakeFlatToggle(card.transform, new Vector2(0, -98), new Vector2(500, 62),
            "去除碰撞伤害");
        disableCollisionDamageToggle.SetIsOnWithoutNotify(!run.config.enableCollisionDamage);
        disableCollisionDamageToggle.onValueChanged.AddListener(SetCollisionDamageDisabled);

        Text note = MakeText("伤害选项即时生效；墙体选项会重新开始本局", card.transform, new Vector2(0, -168), new Vector2(520, 30),
            14, Peach, TextAnchor.MiddleCenter, FontStyle.Normal, false);
        note.font = threatBoardFont;

        MakeFlatButton(card.transform, new Vector2(0, -250), new Vector2(180, 48), "关闭").onClick.AddListener(() =>
        {
            GameAudio.PlayClick();
            settingsOverlay.SetActive(false);
        });
        settingsOverlay.SetActive(false);
    }

    private void SetThreatPillarsDisabled(bool disabled)
    {
        bool enabled = !disabled;
        if (run.config.enableThreatPillars == enabled) return;

        GameAudio.PlayClick();
        run.config.enableThreatPillars = enabled;
        PlayerPrefs.SetInt(DisableThreatPillarsPreference, disabled ? 1 : 0);
        PlayerPrefs.Save();
        NewRun();
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(true);
            settingsOverlay.transform.SetAsLastSibling();
        }
    }

    private void SetPlayerInvincible(bool enabled)
    {
        if (run.config.playerInvincible == enabled) return;
        GameAudio.PlayClick();
        run.config.playerInvincible = enabled;
        SaveBooleanPreference(PlayerInvinciblePreference, enabled);
    }

    private void SetRiftDamageDisabled(bool disabled)
    {
        bool enabled = !disabled;
        if (run.config.enableRiftDamage == enabled) return;
        GameAudio.PlayClick();
        run.config.enableRiftDamage = enabled;
        SaveBooleanPreference(DisableRiftDamagePreference, disabled);
    }

    private void SetFriendlyFireDisabled(bool disabled)
    {
        bool enabled = !disabled;
        if (run.config.enableFriendlyFire == enabled) return;
        GameAudio.PlayClick();
        run.config.enableFriendlyFire = enabled;
        SaveBooleanPreference(DisableFriendlyFirePreference, disabled);
    }

    private void SetCollisionDamageDisabled(bool disabled)
    {
        bool enabled = !disabled;
        if (run.config.enableCollisionDamage == enabled) return;
        GameAudio.PlayClick();
        run.config.enableCollisionDamage = enabled;
        SaveBooleanPreference(DisableCollisionDamagePreference, disabled);
    }

    private static void SaveBooleanPreference(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void NewRun()
    {
        StopAllCoroutines();
        screenShakeRoutine = null;
        kaitSkillAnimationRoutine = null;
        ClearFloatingKaitAnimations();
        ClearAllTrailVisuals();
        ClearAllEffectViews();
        ClearTransientAnimationObjects();
        ClearEnemySpines();
        ResetInterruptedAnimationState();
        busy = false;
        displayKate = null;
        targetingSkill = KaitSkill.None;
        announcedSkillMilestone = 0;
        announcedPassiveMilestone = 0;
        passiveDeck?.ResetDeck();
        skillDeck?.ResetDeck();
        endAudioPlayed = false;
        kaitDefeatingEnemyId = -1;
        int seed = Environment.TickCount;
        run.Reset(seed);
        EnsureKaitSpine();
        kaitSpine?.PlayLoop(KaitSpineView.Idle);
        logPath = Path.Combine(Application.persistentDataPath, $"kait_run_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllText(logPath, "turn,globalDir,kaitWaited,threatChanged,chainSteps,chainKills,lockedPower,chainMoves,chainEndByStrongEnemy,chainEndByWall,kateStart,kateEnd,kateHp,slideDistance,damage,kills,directKills,nonLethalHits,chainActive,momentum,highestMomentum,longestChain,pushes,friendlyFire,activeWallStops,spawnSuppressed,riftBlocks,wallSuppressedSpawns,internalMerges,internalSpawns,clusterClearCount,threatOrientedWaitCount,emptyMapReachable,emptyMapMaxInputs,activeEnemies,pendingSpawns,highestThreat,threatOccupancy,threatLocks,passives,passiveTriggers,endReason\n", Encoding.UTF8);
        endOverlay.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
        statusText.text = "选择全局方向：凯特与威胁盘分别响应。";
        RefreshAll();
        GameAudio.PlayKaitGameStart();
    }

    private void HandleDirection(KaitDirection direction)
    {
        if (TutorialBlocksInput()) return;
        if (run.ended) return;
        InterruptKaitAnimationForMovement();
        if (busy) InterruptActivePresentationForMovement();
        targetingSkill = KaitSkill.None;
        skillDeck?.Sync(run, targetingSkill);
        Vector2Int start = run.katePos;
        List<KaitEnemy> enemySnapshot = SnapshotEnemies();
        List<KaitSpawnRequest> spawnSnapshot = SnapshotSpawns();
        KaitTurnResult result = run.chainActive ? run.ContinueChain(direction) : run.TryGlobalInput(direction);
        if (!result.valid)
        {
            GameAudio.PlayInvalid();
            statusText.text = result.message;
            StartCoroutine(FlashStatus());
            return;
        }
        if (result.turnComplete) AppendLog(start, result);
        StartCoroutine(PlayTurn(result, start, enemySnapshot, spawnSnapshot));
    }

    private void InterruptKaitAnimationForMovement()
    {
        if (kaitSkillAnimationRoutine != null)
        {
            StopCoroutine(kaitSkillAnimationRoutine);
            kaitSkillAnimationRoutine = null;
        }
        kaitSpine?.SetHitFlash(0f);
        kaitSpine?.SetTint(Color.white);
        kaitSpine?.PlayLoop(run.chainActive ? KaitSpineView.ChainDirectionChoice : KaitSpineView.Idle);
    }

    private void InterruptActivePresentationForMovement()
    {
        // The turn has already been resolved by KaitRun before its presentation
        // starts. Cancelling presentation is therefore safe: snap every visual
        // to that authoritative state, then accept the next direction at once.
        StopAllCoroutines();
        screenShakeRoutine = null;
        kaitSkillAnimationRoutine = null;
        ClearFloatingKaitAnimations();
        ClearAllTrailVisuals();
        ClearAllEffectViews();
        ClearTransientAnimationObjects();
        ClearEnemySpines();
        animatedEnemies = null;
        animatedSpawns = null;
        displayedThreat = null;
        hideThreatValues = false;
        hideKate = false;
        displayKate = null;
        impactCells.Clear();
        firingWarlocks.Clear();
        ResetInterruptedAnimationState();
        busy = false;
        RefreshAll();
    }

    private void ResetInterruptedAnimationState()
    {
        if (gameContent != null) gameContent.anchoredPosition = gameContentBasePosition;
        if (battleCells != null)
            foreach (Image cell in battleCells) if (cell != null) cell.rectTransform.localScale = Vector3.one;
        if (battleUnitClips != null)
            foreach (Image unit in battleUnitClips) if (unit != null) unit.rectTransform.localScale = Vector3.one;
        if (threatCells != null)
            foreach (Image cell in threatCells) if (cell != null) cell.rectTransform.localScale = Vector3.one;
        kaitSpine?.SetHitFlash(0f);
        kaitSpine?.SetTint(Color.white);
    }

    private void ClearTransientAnimationObjects()
    {
        if (canvas == null) return;
        Transform[] transforms = canvas.GetComponentsInChildren<Transform>(true);
        foreach (Transform candidate in transforms)
        {
            if (candidate == null || candidate == canvas.transform) continue;
            string objectName = candidate.gameObject.name;
            if (objectName != "Animation Token" && objectName != "Animation Unit" &&
                objectName != "Archer Projectile" && objectName != "Dread Slash Wave" &&
                objectName != "Floating Damage") continue;
            candidate.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(candidate.gameObject);
            else DestroyImmediate(candidate.gameObject);
        }
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
        PlayPassiveTriggerFeedback(result);

        if (result.awaitingTurnChoice && run.chainActive)
        {
            BeginDetachedEnemyDeaths(healthBefore);
            animatedEnemies = null;
            animatedSpawns = null;
            hideKate = false;
            displayKate = null;
            displayedThreat = null;
            statusText.text = result.message;
            busy = false;
            RefreshAll();
            kaitSpine?.PlayLoop(KaitSpineView.ChainDirectionChoice);
            yield break;
        }

        if (result.pushed) yield return AnimatePush(result);
        else if (result.pushBlockedByWall || result.pushBlockedByUnit)
            yield return PulseBattleUnit(result.pushFrom, Color.white, 0.18f, result.damagedEnemyId);

        if (result.dreadSlash)
        {
            GameAudio.PlayDreadSlash();
            yield return AnimateAllEnemyActions(result.enemyActions.FindAll(a => a.type == KaitIntentType.Move), true);
            yield return AnimateAllEnemyActions(result.enemyActions.FindAll(a => a.type != KaitIntentType.Move));
        }
        else
        {
            List<KaitEnemyAction> enemyMoves = result.enemyActions.FindAll(a => a.type == KaitIntentType.Move);
            List<KaitEnemyAction> enemyAttacks = result.enemyActions.FindAll(a => a.type != KaitIntentType.Move);
            yield return AnimateAllEnemyActions(enemyMoves);
            if (enemyMoves.Count > 0 && enemyAttacks.Count > 0) yield return new WaitForSecondsRealtime(0.05f);
            yield return AnimateAllEnemyActions(enemyAttacks);
        }

        yield return AnimateCombatFeedback(result, healthBefore, kateHpBefore);
        yield return AnimateEnemyDeaths(healthBefore);

        var previousEnemyIds = new HashSet<int>();
        foreach (KaitEnemy enemy in enemySnapshot) previousEnemyIds.Add(enemy.id);
        animatedEnemies = null;
        firingWarlocks.Clear();
        animatedSpawns = null;
        hideKate = false;
        displayKate = null;
        RefreshBattle();

        bool newRiftAppeared = run.spawns.Exists(spawn => !spawnSnapshot.Exists(previous =>
            previous.targetCell == spawn.targetCell && previous.sourceThreatCell == spawn.sourceThreatCell && previous.tier == spawn.tier));
        if (newRiftAppeared)
        {
            GameAudio.PlayRiftWarning();
        }

        bool playedLandingSound = false;
        foreach (KaitEnemy enemy in run.enemies)
            if (enemy.life != KaitEnemyLife.Dead && !previousEnemyIds.Contains(enemy.id))
            {
                EnemySpineView view = EnemySpine(enemy);
                if (view == null) continue;
                PlayEnemyLanding(view);
                GameAudio.PlayEnemySpawnVoice(enemy.type, enemy.id);
                if (!playedLandingSound)
                {
                    playedLandingSound = true;
                    GameAudio.PlayLanding();
                }
            }
        if (result.bossSpawned) GameAudio.PlayBossRoar();
        if (playedLandingSound) yield return new WaitForSecondsRealtime(0.16f);

        yield return AnimateEnemyAttackPreparation();

        // Enemy arrival is communicated by the Spine `landing` animation only;
        // do not scale the underlying board cell when the enemy appears.
        var spawnPulses = new List<RectTransform>();
        foreach (KaitSpawnRequest spawn in run.spawns)
            if (spawn.targetCell.x >= 0) spawnPulses.Add(battleCells[spawn.targetCell.x + spawn.targetCell.y * KaitRun.BattleSize].rectTransform);
        if (spawnPulses.Count > 0) yield return ScalePulseMany(spawnPulses, 0.35f, 1.15f, 0.2f);

        displayedThreat = null;
        statusText.text = result.message + (result.merges.Count > 0 ? $" · 威胁合并 ×{result.merges.Count}" : "");
        busy = false;
        RefreshAll();
        if (run.ended)
        {
            ShowEnd();
        }
        else
        {
            if (run.chainActive) kaitSpine?.PlayLoop(KaitSpineView.ChainDirectionChoice);
        }
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
        skillDeck?.Sync(run, targetingSkill);
        RefreshPassiveUI();
    }

    private void ShowPendingSkillChoice()
    {
        passiveDeck?.Sync(run);
        skillDeck?.Sync(run, targetingSkill);
        int active = run.pendingSkillMilestone, passive = run.pendingPassiveMilestone;
        if (!run.ended && ((active != 0 && active != announcedSkillMilestone) ||
            (passive != 0 && passive != announcedPassiveMilestone)))
            GameAudio.PlaySkillReady();
        announcedSkillMilestone = active;
        announcedPassiveMilestone = passive;
        if (controlsPanel != null) controlsPanel.SetActive(!run.ended);
    }

    private void ChoosePendingSkill(int choiceIndex)
    {
        if (run.ended || run.pendingSkillMilestone == 0) return;
        List<KaitSkill> choices = run.SkillChoicesForMilestone(run.pendingSkillMilestone);
        if (choiceIndex < 0 || choiceIndex >= choices.Count || !run.ChooseSkill(choices[choiceIndex])) return;
        GameAudio.PlayClick();
        targetingSkill = KaitSkill.None;
        statusText.text = $"已获得：{KaitRun.SkillName(choices[choiceIndex])}（不消耗回合）";
        RefreshAll();
    }

    private void ChoosePendingPassive(int choiceIndex)
    {
        if (run.ended || run.pendingPassiveMilestone == 0) return;
        List<KaitPassive> choices = run.PassiveChoicesForMilestone(run.pendingPassiveMilestone);
        if (choiceIndex < 0 || choiceIndex >= choices.Count || !run.ChoosePassive(choices[choiceIndex])) return;
        GameAudio.PlayPassiveConfirm();
        statusText.text = $"已获得被动：{KaitPassiveCatalog.Name(choices[choiceIndex])}（永久生效）";
        RefreshAll();
    }

    private void RefreshPassiveUI()
    {
        passiveDeck?.Sync(run);
    }

    private bool HandleSkillCardCast(int slot)
    {
        if (TutorialBlocksInput()) return false;
        if (run.ended || slot < 0 || slot >= run.skills.Count) return false;
        if (!KaitSkillDeck.IsReady(run, run.skills[slot])) return false;
        if (busy) InterruptActivePresentationForMovement();
        KaitSkill skill = run.skills[slot];
        if (skill == KaitSkill.ShadowStep)
        {
            Vector2Int start = run.katePos;
            if (run.TryShadowStep())
            {
                GameAudio.PlaySkillUse(skill);
                StartCoroutine(AnimateShadowStep(start));
                RefreshSkillUI();
                return true;
            }
            GameAudio.PlayInvalid(); statusText.text = "踏影当前不可用"; RefreshAll();
            return false;
        }
        if (skill == KaitSkill.IceTomb || skill == KaitSkill.LesserPhantom)
        {
            targetingSkill = skill;
            statusText.text = $"{KaitRun.SkillName(skill)}：请点选一个敌人";
            RefreshAll();
            return true;
        }
        targetingSkill = KaitSkill.None;
        bool applied = run.TryUseSkill(skill, -1, out string message);
        if (applied)
        {
            GameAudio.PlaySkillUse(skill);
            if (skill == KaitSkill.DreadSlash) GameAudio.PlayKaitLargeAttackSkillVoice();
            else if (skill == KaitSkill.CatAgility) GameAudio.PlayKaitUltimateVoice();
            statusText.text = message;
            if (kaitSkillAnimationRoutine != null) StopCoroutine(kaitSkillAnimationRoutine);
            kaitSkillAnimationRoutine = StartCoroutine(RunKaitSkillAnimation(skill));
        }
        else
        {
            GameAudio.PlayInvalid();
            statusText.text = message;
        }
        RefreshAll();
        return applied;
    }

    private void HandleBattleCellClick(Vector2Int cell)
    {
        if (run.ended || targetingSkill == KaitSkill.None) return;
        if (busy) InterruptActivePresentationForMovement();
        KaitEnemy target = run.EnemyAt(cell);
        if (target == null) { GameAudio.PlayInvalid(); statusText.text = "这里没有可选敌人"; return; }
        KaitSkill skill = targetingSkill;
        if (run.TryUseSkill(skill, target.id, out string message))
        {
            GameAudio.PlaySkillUse(skill);
            GameAudio.PlayKaitSmallAttackSkillVoice();
            targetingSkill = KaitSkill.None;
            kaitSpine?.PlayOnce(KaitSpineView.OtherSkill,
                run.chainActive ? KaitSpineView.ChainDirectionChoice : KaitSpineView.Idle);
            if (skill == KaitSkill.IceTomb)
                PlayCombatEffectAtCell(KaitCombatEffectKind.Ice, cell, new Vector2(124f, 124f),
                    0.32f, 0.65f, 0f, Vector2.zero, "Ice Tomb Effect");
            else
                PlayCombatEffectAtCell(KaitCombatEffectKind.Phantom, cell, new Vector2(132f, 112f),
                    0.3f, 0.6f, 0f, Vector2.zero, "Lesser Phantom Effect");
            StartCoroutine(PulseBattleUnit(cell, skill == KaitSkill.IceTomb ? Cyan : Coral, 0.2f));
        }
        else GameAudio.PlayInvalid();
        statusText.text = message;
        RefreshAll();
    }

    private void PlayPassiveTriggerFeedback(KaitTurnResult result)
    {
        foreach (KaitPassiveTrigger trigger in result.passiveTriggers)
        {
            if (trigger.threatCell.x >= 0 && trigger.threatCell.y >= 0 && trigger.threatCell.x < run.ThreatSize && trigger.threatCell.y < run.ThreatSize)
            {
                int threatIndex = trigger.threatCell.x + trigger.threatCell.y * run.ThreatSize;
                StartCoroutine(ScalePulse(threatCells[threatIndex].rectTransform, 0.94f, 1.08f, 0.14f));
            }
            if (trigger.battleCell.x > 0 && trigger.battleCell.y > 0 && trigger.battleCell.x < KaitRun.BattleSize - 1 && trigger.battleCell.y < KaitRun.BattleSize - 1)
            {
                int battleIndex = trigger.battleCell.x + trigger.battleCell.y * KaitRun.BattleSize;
                StartCoroutine(ScalePulse(battleCellTints[battleIndex].rectTransform, 0.94f, 1.08f, 0.14f));
            }
        }
    }

    private IEnumerator AnimateShadowStep(Vector2Int start)
    {
        PlayGroundSmokeBurst(start, Vector2Int.zero, new Color(0.68f, 0.64f, 0.78f, 0.72f),
            1.05f, "Shadow Step Departure");
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
            token.position = Vector3.LerpUnclamped(from, to, EaseOutCubic(elapsed / duration));
            elapsed += Time.unscaledDeltaTime; yield return null;
        }
        if (movingKait != null) DestroyFloatingKait(movingKait); else Destroy(token.gameObject);
        PlayGroundSmokeBurst(run.katePos, Vector2Int.zero, new Color(0.68f, 0.64f, 0.78f, 0.72f),
            1.05f, "Shadow Step Arrival");
        hideKate = false; busy = false;
        kaitSpine?.PlayLoop(run.chainActive ? KaitSpineView.ChainDirectionChoice : KaitSpineView.Idle);
        statusText.text = "踏影：额外前进 1 格，可继续选择转向";
        RefreshAll();
    }



    private static string PassiveChoiceDescription(KaitPassive passive)
    {
        switch (passive)
        {
            case KaitPassive.BirdEye: return "鸦后之眼\n预览下一枚 2\n的出生位置";
            case KaitPassive.OldNewsArchive: return "旧闻归档\n第 5 枚 2 出现时\n合并最早两枚";
            case KaitPassive.Simplify: return "化零为整\n同回合同级出怪\n压缩为高一级";
            case KaitPassive.BloodBookmark: return "血色书签\n连杀终点接收\n下次受阻出怪";
            case KaitPassive.MomentumResonance: return "念动力共振\n首次推动同时\n牵动右盘数字";
            case KaitPassive.Devil: return "魔手\n主动技能联动\n另一技能 CD-1";
            case KaitPassive.CheshireCat: return "猫戏老鼠\n敌军友伤最低\n保留 1 点生命";
            case KaitPassive.Squeeze: return "挤压\n出怪受阻时先\n推开占位敌人";
            case KaitPassive.Follower: return "尾随者\n后续敌人靠近\n本回合首名敌人";
            case KaitPassive.BladeCovenant: return "刃之魔契\n连杀每满 3 人\n主动技能 CD-1";
            case KaitPassive.Trend: return "定势\n新 2 从移动方向\n反侧生成";
            case KaitPassive.SweepTail: return "鸦羽扫尾\n主动刹车清除\n对应位置的 2";
            default: return string.Empty;
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
                battleMageWarnings[index].gameObject.SetActive(false);
                battleRifts[index].gameObject.SetActive(false);
                battleRiftDangerIcons[index].gameObject.SetActive(false);
                tile.sprite = dungeonFloorSprite != null ? dungeonFloorSprite : roundedSprite;
                tile.type = dungeonFloorSprite != null ? Image.Type.Simple : Image.Type.Sliced;
                tile.color = Color.white;
                battleObstacles[index].gameObject.SetActive(run.walls[x, y]);
                battleObstacleShadows[index].SetActive(run.walls[x, y]);
                image.color = Color.clear;
                if (run.walls[x, y])
                {
                    image.color = Color.clear;
                    label.text = "";
                }
                Color? intentTint = IntentTintAt(p);
                KaitEnemy aimingWarlock = (animatedEnemies ?? run.enemies).Find(e =>
                    e.life == KaitEnemyLife.Active && !firingWarlocks.Contains(e.id) &&
                    e.intent.type == KaitIntentType.CrossBlast && e.intent.affectedCells.Contains(p));
                if (!run.walls[x,y] && aimingWarlock != null)
                {
                    battleMageWarnings[index].ConfigureAim(aimingWarlock.intent.target == p);
                    battleMageWarnings[index].gameObject.SetActive(true);
                }
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
                        // Keep every Spine actor in one board-local overlay. A
                        // view must never fall back to the Canvas root: its
                        // centre is the middle UI column, which made an enemy
                        // appear to run out of the board during a presentation.
                        Transform actorParent = battleActorLayer != null ? battleActorLayer : unitClip.transform;
                        enemySpine.SetParent(actorParent);
                        enemySpine.Root.position = battleCells[index].rectTransform.position;
                        enemySpine.SetTint(unitTint);
                        enemySpine.Face(enemy.type == KaitEnemyType.ShieldKnight ? enemy.facing : enemy.intent.direction);
                        enemySpine.SetVisible(true);
                    }
                    else
                    {
                        battlePortraits[index].sprite = EnemyPortrait(enemy.type);
                        if (battleActorLayer != null)
                        {
                            battlePortraits[index].transform.SetParent(battleActorLayer, true);
                            battlePortraits[index].rectTransform.position = battleCells[index].rectTransform.position;
                        }
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
                        // Keep Kait above the complete grid, not merely above her
                        // current cell. Otherwise a later sibling cell can cover
                        // the part of her sword extending into that neighbour.
                        Transform actorParent = battleKaitLayer != null ? battleKaitLayer : battleCells[index].transform;
                        kaitSpine.SetParent(actorParent);
                        kaitSpine.Root.position = battleCells[index].rectTransform.position;
                        kaitSpine.SetVisible(true);
                    }
                    else
                    {
                        battlePortraits[index].sprite = kaitPortrait;
                        if (battleKaitLayer != null)
                        {
                            battlePortraits[index].transform.SetParent(battleKaitLayer, true);
                            battlePortraits[index].rectTransform.position = battleCells[index].rectTransform.position;
                        }
                        battlePortraits[index].gameObject.SetActive(true);
                    }
                    if (run.chainActive) battleFacingLabels[index].text = ">";
                    battleFacingLabels[index].rectTransform.localRotation = Quaternion.Euler(0f, 0f, HalfArrowAngle(KaitRun.Delta(run.currentDirection)));
                    battleFacingLabels[index].color = Void;
                }
                bool riftCanDamageOccupant = run.config.enableRiftDamage &&
                    (enemy != null || (!hideKate && kate == p && !run.config.playerInvincible));
                bool unitStandingOnRift = spawn != null && riftCanDamageOccupant;
                if (unitStandingOnRift)
                {
                    Image danger = battleRiftDangerIcons[index];
                    danger.gameObject.SetActive(true);
                    danger.color = spawn.state == KaitSpawnState.Ready
                        ? Color.white
                        : new Color(1f, 1f, 1f, 0.88f);
                    PositionRiftDangerIcon(index);
                }
            }
        }
        // Kait remains above living enemies when both overlap during a hit or
        // a chain kill. Detached death views use their own lower cell layer.
        if (kaitSpine != null && battleKaitLayer != null && kaitSpine.Root.parent == battleKaitLayer)
            kaitSpine.Root.SetAsLastSibling();
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
                bool previewTwo = displayedThreat == null && !hideThreatValues && value == 0 && run.HasPassive(KaitPassive.BirdEye) && run.nextThreatTwoPreview == cell;
                if (previewTwo)
                {
                    threatLabels[index].text = "2";
                    threatCells[index].color = Color.Lerp(ThreatColor(0), ThreatColor(2), 0.48f);
                    threatLabels[index].color = new Color(Void.r, Void.g, Void.b, 0.52f);
                    continue;
                }
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
                StartKaitSwordTipTrail(kaitSpine, killedBlockedEnemy);
                GameAudio.PlaySwordSwing();
                yield return new WaitForSecondsRealtime(KaitAttackImpactLead);
                PlayKaitImpactAudio(result, killedBlockedEnemy);
                PlayKaitImpactEffect(result, killedBlockedEnemy, result.blockedEnemyCell, -1, start);
                if (killedBlockedEnemy)
                {
                    foreach (int enemyId in result.playerKilledEnemyIds) StartEnemyDeathAnimation(enemyId);
                }
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
            {
                kaitSpine?.PlayOnce(KaitSpineView.WallStop);
                GameAudio.PlayWallStop();
                PlayWallStopEffect(result.kaitDirection);
            }
            yield break;
        }

        hideKate = true;
        kaitSpine?.Face(result.kaitDirection);
        GameAudio.PlayDrawSword();
        RefreshBattle();
        KaitSpineView movingKait = CreateFloatingKait(battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, result.kaitDirection);
        RectTransform token = movingKait != null ? movingKait.Root : CreateFloatingPortrait(kaitPortrait, Color.clear, battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform, new Vector2(115, 115));
        movingKait?.PlayLoop(KaitSpineView.Run);
        var ghosts = new List<KaitTrailVisual>();
        var points = new List<Vector3> { battleCells[start.x + start.y * KaitRun.BattleSize].rectTransform.position };
        foreach (Vector2Int cell in result.katePath) points.Add(battleCells[cell.x + cell.y * KaitRun.BattleSize].rectTransform.position);

        int segments = points.Count - 1;
        float duration = Mathf.Min(0.36f, 0.16f + segments * 0.025f);
        float elapsed = 0f;
        int lastReached = 0;
        int killSoundsPlayed = 0;
        var preparedKillSteps = new HashSet<int>();
        int chainKillsBeforeTurn = Mathf.Max(0, result.chainKillCount - result.playerKilledEnemyIds.Count);
        // Process one final frame at progress == 1. The previous `< duration`
        // loop exited before `reached` could become `segments`, so an enemy on
        // the final (and most common) destination cell received audio/death
        // feedback but never spawned its kill or chain-kill visual.
        while (true)
        {
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            float progress = EaseOutCubic(normalizedTime) * segments;
            int segment = Mathf.Min(segments - 1, Mathf.FloorToInt(progress));
            token.position = Vector3.Lerp(points[segment], points[segment + 1],
                Mathf.Clamp01(progress - segment));
            int approachingStep = Mathf.Clamp(Mathf.FloorToInt(progress + 0.35f), 1, segments);
            if (!preparedKillSteps.Contains(approachingStep))
            {
                Vector2Int approachingCell = result.katePath[approachingStep - 1];
                if (animatedEnemies.Exists(e => e.pos == approachingCell && result.playerKilledEnemyIds.Contains(e.id)))
                {
                    preparedKillSteps.Add(approachingStep);
                    GameAudio.PlaySwordSwing();
                    movingKait?.PlayOnce(KaitSpineView.ChainAttack, KaitSpineView.Run);
                    StartKaitSwordTipTrail(movingKait, true);
                }
            }
            int reached = Mathf.Min(segments, Mathf.FloorToInt(progress));
            while (lastReached < reached)
            {
                lastReached++;
                Vector2Int cell = result.katePath[lastReached - 1];
                int momentumAtCell = result.pathMomentum.Count >= lastReached ? result.pathMomentum[lastReached - 1] : run.momentum;
                Vector3 segmentStart = points[lastReached - 1];
                Vector3 segmentEnd = points[lastReached];
                ghosts.Add(CreateGhostToken(Vector3.Lerp(segmentStart, segmentEnd, 0.5f), momentumAtCell, result.kaitDirection, 0.78f));
                ghosts.Add(CreateGhostToken(segmentEnd, momentumAtCell, result.kaitDirection, 1f));
                while (ghosts.Count > Mathf.Clamp(momentumAtCell * 2, 2, 10))
                {
                    DestroyTrailVisual(ghosts[0]);
                    ghosts.RemoveAt(0);
                }
                if (animatedEnemies.Exists(e => e.pos == cell && result.playerKilledEnemyIds.Contains(e.id)))
                {
                    KaitEnemy struckEnemy = animatedEnemies.Find(e => e.pos == cell && result.playerKilledEnemyIds.Contains(e.id));
                    killSoundsPlayed++;
                    int chainKills = chainKillsBeforeTurn + killSoundsPlayed;
                    GameAudio.PlayKaitKill(chainKills);
                    TriggerChainShake(chainKills);
                    PlayKaitImpactEffect(result, true, cell, chainKills, cell);
                    StartEnemyDeathAnimation(struckEnemy.id);
                    RefreshBattle();
                    StartCoroutine(PulseBattleUnit(cell, Color.white, 0.16f, struckEnemy.id));
                }
            }
            if (normalizedTime >= 1f) break;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        token.position = points[points.Count - 1];
        if (result.playerKilledEnemyIds.Count > 0)
            yield return new WaitForSecondsRealtime(0.08f);
        bool killedBlockedEnemyAfterSlide = result.blockedEnemyCell.x >= 0 && animatedEnemies.Exists(e =>
            e.pos == result.blockedEnemyCell && result.playerKilledEnemyIds.Contains(e.id));
        if (movingKait != null) DestroyFloatingKait(movingKait); else Destroy(token.gameObject);
        foreach (KaitTrailVisual ghost in ghosts) StartCoroutine(FadeAndDestroyTrail(ghost, 0.24f));
        // Keep defeated enemies rendered until the finishing attack completes.
        // The normal turn path removes them after `die`; the chain-direction path
        // moves the same visible Spine views into its detached death layer.
        // Removing them here caused a visible blank frame/gap before `die` began.
        impactCells.Clear();
        firingWarlocks.Clear();
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
            StartKaitSwordTipTrail(kaitSpine, killedBlockedEnemyAfterSlide);
            GameAudio.PlaySwordSwing();
            yield return new WaitForSecondsRealtime(KaitAttackImpactLead);
            PlayKaitImpactAudio(result, killedBlockedEnemyAfterSlide);
            Vector2Int attackerCell = result.katePath.Count > 0
                ? result.katePath[result.katePath.Count - 1]
                : start;
            PlayKaitImpactEffect(result, killedBlockedEnemyAfterSlide, result.blockedEnemyCell, -1, attackerCell);
            if (killedBlockedEnemyAfterSlide)
            {
                foreach (int enemyId in result.playerKilledEnemyIds) StartEnemyDeathAnimation(enemyId);
            }
            yield return PulseBattleUnit(result.blockedEnemyCell, Color.white, 0.14f, result.damagedEnemyId);
        }
        else if (result.stoppedByWall || result.chainEndedByWall || result.activeBrake || result.pushBlockedByWall)
        {
            kaitSpine?.PlayOnce(KaitSpineView.WallStop);
            GameAudio.PlayWallStop();
            PlayWallStopEffect(result.kaitDirection);
        }
        else if (result.playerKilledEnemyIds.Count > 0)
        {
            kaitSpine?.PlayOnce(KaitSpineView.ChainAttack);
            foreach (int enemyId in result.playerKilledEnemyIds) StartEnemyDeathAnimation(enemyId);
        }
        else
            kaitSpine?.PlayLoop(KaitSpineView.Idle);
    }

    private void PlayKaitImpactAudio(KaitTurnResult result, bool killed)
    {
        if (result.playerAttackBlocked)
        {
            GameAudio.PlayKaitNormalAttackVoice();
            GameAudio.PlayBlock();
            return;
        }
        // Kills have their own impact cue; do not layer the non-lethal hit over it.
        if (killed || (result.damageDealt <= 0 && result.collisionDamage <= 0)) return;
        GameAudio.PlayNormalHit();
        if (!killed)
        {
            GameAudio.PlayKaitNormalAttackVoice();
            KaitEnemy damagedEnemy = run.enemies.Find(e => e.id == result.damagedEnemyId);
            if (damagedEnemy != null)
                GameAudio.PlayEnemyHurt(damagedEnemy.type, damagedEnemy.id, damagedEnemy.hp,
                    result.pushed || result.pushBlockedByWall || result.pushBlockedByUnit);
            else
                GameAudio.PlayEnemyHurt();
        }
    }

    private IEnumerator AnimateAllEnemyActions(List<KaitEnemyAction> actions, bool dreadSlash = false)
    {
        if (actions == null || actions.Count == 0) yield break;
        var moves = new List<EnemyMoveVisual>();
        var projectiles = new List<ProjectileVisual>();
        var visualFriendlyHp = new Dictionary<int, int>();
        foreach (KaitEnemyAction action in actions)
            foreach (int victimId in action.friendlyHitIds)
            {
                if (!visualFriendlyHp.ContainsKey(victimId))
                {
                    KaitEnemy resolved = run.enemies.Find(e => e.id == victimId);
                    visualFriendlyHp[victimId] = resolved == null || resolved.life == KaitEnemyLife.Dead ? 0 : resolved.hp;
                }
                visualFriendlyHp[victimId] += Mathf.Max(0, action.damage);
            }
        bool hasAttacks = false;
        bool hasArrowAttack = false;
        bool hasMagicAttack = false;
        int longestArrowPath = 0;
        foreach (KaitEnemyAction action in actions)
        {
            KaitEnemy enemy = animatedEnemies?.Find(e => e.id == action.enemyId);
            if (enemy == null) continue;
            if (action.type == KaitIntentType.Move && action.from != action.to)
            {
                RectTransform token = CreateFloatingPortrait(EnemyPortrait(enemy.type), Color.clear, battleCells[action.from.x + action.from.y * KaitRun.BattleSize].rectTransform, new Vector2(115, 115), enemy.hp, enemy.maxHp);
                moves.Add(new EnemyMoveVisual
                {
                    pressure = dreadSlash ? PlayDreadPressure(action.from, action.to - action.from) : null,
                    enemy = enemy,
                    rect = token,
                    from = battleCells[action.from.x + action.from.y * KaitRun.BattleSize].rectTransform.position,
                    to = battleCells[action.to.x + action.to.y * KaitRun.BattleSize].rectTransform.position
                });
                animatedEnemies.Remove(enemy);
            }
            if (action.type == KaitIntentType.Melee || action.type == KaitIntentType.LineShot || action.type == KaitIntentType.CrossBlast)
            {
                hasAttacks = true;
                EnemySpineView attacker = EnemySpine(enemy);
                if (attacker != null)
                {
                    attacker.Face(action.to - action.from);
                    attacker.PlayAttack();
                }
                GameAudio.PlayEnemyAttackVoice(enemy.type, enemy.id);
                if (action.type == KaitIntentType.Melee) GameAudio.PlaySwordSwing();
                else if (action.type == KaitIntentType.LineShot)
                {
                    hasArrowAttack = true;
                    longestArrowPath = Mathf.Max(longestArrowPath, action.affectedCells.Count);
                }
                else
                {
                    hasMagicAttack = true;
                    GameAudio.PlayMagicCast();
                    firingWarlocks.Add(action.enemyId);
                }
                if (action.type != KaitIntentType.CrossBlast)
                    foreach (Vector2Int cell in action.affectedCells) if (InsideBattle(cell)) impactCells.Add(cell);
                if (action.type == KaitIntentType.LineShot && action.affectedCells.Count > 0 && InsideBattle(action.from))
                {
                    Vector2Int firstCell = action.affectedCells[0];
                    Vector2Int lastCell = action.affectedCells[action.affectedCells.Count - 1];
                    float arrowAngle = HalfArrowAngle(firstCell - action.from);
                    SpineEffectView arrowSpine = PlayEffectAtCell(arrowProjectileEffect, action.from,
                        new Vector2(92f, 56f), 3.4f, 1f, arrowAngle, null, false, false,
                        "Archer Projectile");
                    RectTransform arrowRect;
                    if (arrowSpine != null)
                    {
                        arrowRect = arrowSpine.Root;
                    }
                    else
                    {
                        Text arrow = MakeText(">", canvas.transform, Vector2.zero, new Vector2(48, 48), 42, Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
                        arrow.gameObject.name = "Archer Projectile";
                        arrow.raycastTarget = false;
                        Outline outline = arrow.GetComponent<Outline>();
                        outline.effectColor = new Color(Wine.r, Wine.g, Wine.b, 0.95f);
                        outline.effectDistance = new Vector2(2f, -2f);
                        arrowRect = arrow.rectTransform;
                        arrowRect.position = battleCells[action.from.x + action.from.y * KaitRun.BattleSize].rectTransform.position;
                        arrowRect.localRotation = Quaternion.Euler(0f, 0f, arrowAngle);
                        arrowRect.SetAsLastSibling();
                    }
                    projectiles.Add(new ProjectileVisual
                    {
                        rect = arrowRect,
                        spine = arrowSpine,
                        from = arrowRect.position,
                        to = battleCells[lastCell.x + lastCell.y * KaitRun.BattleSize].rectTransform.position
                    });
                }
            }
        }

        RefreshBattle();
        if (moves.Count > 0)
        {
            float moveElapsed = 0f;
            while (moveElapsed < EnemyMoveDuration)
            {
                float t = EaseOutCubic(moveElapsed / EnemyMoveDuration);
                foreach (EnemyMoveVisual move in moves)
                {
                    move.rect.position = Vector3.LerpUnclamped(move.from, move.to, t);
                    if (move.pressure != null)
                        move.pressure.transform.position = move.rect.position - (move.to - move.from).normalized * 38f * battleUnderEffectLayer.lossyScale.x;
                }
                moveElapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            foreach (EnemyMoveVisual move in moves)
            {
                move.rect.position = move.to;
                if (move.pressure != null)
                    move.pressure.transform.position = move.to - (move.to - move.from).normalized * 38f * battleUnderEffectLayer.lossyScale.x;
                move.enemy.pos = run.enemies.Find(e => e.id == move.enemy.id)?.pos ?? move.enemy.pos;
                animatedEnemies.Add(move.enemy);
                Destroy(move.rect.gameObject);
            }
            RefreshBattle();
        }

        if (!hasAttacks) yield break;

        yield return new WaitForSecondsRealtime(EnemyAttackReleaseLead);
        if (hasArrowAttack) GameAudio.PlayArrowFlight();

        float projectileDuration = projectiles.Count == 0
            ? 0f
            : Mathf.Clamp(0.1f + longestArrowPath * 0.055f, 0.14f, 0.32f);
        float projectileElapsed = 0f;
        while (projectileElapsed < projectileDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, projectileElapsed / projectileDuration);
            foreach (ProjectileVisual projectile in projectiles)
                projectile.rect.position = Vector3.Lerp(projectile.from, projectile.to, t);
            projectileElapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        foreach (ProjectileVisual projectile in projectiles) projectile.rect.position = projectile.to;

        bool kaitWasHit = false;
        KaitEnemy kaitAttacker = null;
        bool arrowHit = false;
        bool meleeHit = false;
        bool bodyHit = false;
        foreach (KaitEnemyAction action in actions)
        {
            bool hitUnit = action.hitKate || action.friendlyHitIds.Count > 0;
            if (action.type == KaitIntentType.LineShot && hitUnit) arrowHit = true;
            else if (action.type == KaitIntentType.Melee && action.friendlyHitIds.Count > 0) meleeHit = true;
            if (ShouldPlayBodyHurt(action, run.config.playerInvincible)) bodyHit = true;
            if (action.type == KaitIntentType.CrossBlast && InsideBattle(action.to))
                PlayMageImpact(action.to, action.affectedCells);
            else if (action.type == KaitIntentType.Melee && action.friendlyHitIds.Count > 0 && InsideBattle(action.to))
                PlayCombatEffectAtCell(KaitCombatEffectKind.EnemyHit, action.to,
                    new Vector2(116f, 116f), 0.2f, 0.65f,
                    HalfArrowAngle(action.to - action.from), Vector2.zero,
                    "Enemy Melee Impact Effect");
            if (action.type == KaitIntentType.LineShot && hitUnit)
            {
                if (action.hitKate && InsideBattle(run.katePos))
                    PlayArrowImpact(run.katePos, action.from, true);
                foreach (int victimId in action.friendlyHitIds)
                {
                    KaitEnemy victimEnemy = animatedEnemies?.Find(e => e.id == victimId) ??
                        run.enemies.Find(e => e.id == victimId);
                    if (victimEnemy != null && InsideBattle(victimEnemy.pos))
                        PlayArrowImpact(victimEnemy.pos, action.from, false);
                }
            }
            if (action.hitKate)
            {
                kaitWasHit = true;
                kaitAttacker = run.enemies.Find(e => e.id == action.enemyId) ??
                    animatedEnemies?.Find(e => e.id == action.enemyId);
            }

            foreach (int victimId in action.friendlyHitIds)
            {
                EnemySpineView victim = EnemySpine(victimId);
                if (victim == null || !visualFriendlyHp.ContainsKey(victimId)) continue;
                visualFriendlyHp[victimId] = Mathf.Max(0, visualFriendlyHp[victimId] - Mathf.Max(0, action.damage));
                if (visualFriendlyHp[victimId] > 0)
                {
                    victim.PlayDamage();
                    KaitEnemy victimEnemy = run.enemies.Find(e => e.id == victimId) ??
                        animatedEnemies?.Find(e => e.id == victimId);
                    if (victimEnemy != null)
                        GameAudio.PlayEnemyHurt(victimEnemy.type, victimEnemy.id, visualFriendlyHp[victimId], false);
                    else
                        GameAudio.PlayEnemyHurt();
                }
                else
                {
                    StartEnemyDeathAnimation(victimId);
                }
            }
        }
        if (arrowHit) GameAudio.PlayArrowImpact();
        if (hasMagicAttack) GameAudio.PlayMagicImpact();
        if (meleeHit) GameAudio.PlayNormalHit();
        if (bodyHit) GameAudio.PlayBodyHurt();
        if (kaitWasHit)
        {
            GameAudio.PlayKaitDamageVoice(run.kateHp, run.config.kateMaxHp);
            if (run.kateHp <= 0 && kaitAttacker != null)
            {
                kaitDefeatingEnemyId = kaitAttacker.id;
                kaitDefeatingEnemyType = kaitAttacker.type;
            }
        }

        RefreshBattle();
        if (PostImpactHold > 0f) yield return new WaitForSecondsRealtime(PostImpactHold);
        foreach (ProjectileVisual projectile in projectiles)
        {
            if (projectile.spine != null) DestroyEffectView(projectile.spine);
            else if (projectile.rect != null) Destroy(projectile.rect.gameObject);
        }
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
            pushedView.SetParent(battleActorLayer != null ? battleActorLayer : canvas.transform);
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
        if (result.damageDealt <= 0 && result.collisionDamage <= 0 &&
            result.playerKilledEnemyIds.Count == 0 && !result.playerAttackBlocked)
            GameAudio.PlayPush();
        float elapsed = 0f;
        const float duration = 0.18f;
        while (elapsed < duration)
        {
            token.position = Vector3.LerpUnclamped(from, to, EaseOutCubic(elapsed / duration));
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
            RectTransform token = CreateFloatingToken(motion.value.ToString(), ThreatColor(motion.value), fromCell, new Vector2(74, 74), 28);
            visuals.Add(new ThreatVisual { rect = token, from = fromCell.position, to = toCell.position });
        }

        float elapsed = 0f;
        const float duration = 0.18f;
        while (elapsed < duration)
        {
            float t = EaseOutCubic(elapsed / duration);
            foreach (ThreatVisual visual in visuals) visual.rect.position = Vector3.LerpUnclamped(visual.from, visual.to, t);
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
        Transform parent = hp < 0 ? battleKaitLayer : battleActorLayer;
        Image image = Rect("Animation Unit", parent != null ? parent : canvas.transform, Vector2.zero, size, background);
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
        KaitSpineView view = KaitSpineView.Create(makotoSkeletonData,
            battleKaitLayer != null ? battleKaitLayer : canvas.transform, size, name);
        if (view == null) return null;
        view.Root.position = source.position;
        view.Root.SetAsLastSibling();
        view.Face(direction);
        activeFloatingKaits.Add(view);
        return view;
    }

    private void DestroyFloatingKait(KaitSpineView view)
    {
        if (view == null) return;
        activeFloatingKaits.Remove(view);
        view.SetVisible(false);
        view.Destroy();
    }

    private void ClearFloatingKaitAnimations()
    {
        if (activeFloatingKaits.Count == 0) return;
        KaitSpineView[] views = activeFloatingKaits.ToArray();
        activeFloatingKaits.Clear();
        foreach (KaitSpineView view in views)
        {
            if (view == null) continue;
            view.SetVisible(false);
            view.Destroy();
        }
    }

    private void EnsureKaitSpine()
    {
        if (kaitSpine != null || makotoSkeletonData == null || battleCells == null) return;
        int index = run.katePos.x + run.katePos.y * KaitRun.BattleSize;
        Transform parent = battleKaitLayer != null
            ? battleKaitLayer
            : index >= 0 && index < battleCells.Length && battleCells[index] != null
                ? battleCells[index].transform
            : canvas.transform;
        kaitSpine = KaitSpineView.Create(makotoSkeletonData, parent, new Vector2(115, 115));
        if (kaitSpine != null && index >= 0 && index < battleCells.Length && battleCells[index] != null)
            kaitSpine.Root.position = battleCells[index].rectTransform.position;
    }

    private KaitTrailVisual CreateGhostToken(Vector3 position, int momentumValue, KaitDirection direction, float opacityMultiplier)
    {
        float tier = Mathf.Clamp01((momentumValue - 1) / 4f);
        Color trailColor = KaitSpeedTrailColor(tier);
        trailColor.a = Mathf.Lerp(0.3f, 0.62f, tier) * opacityMultiplier;
        KaitSpineView ghost = makotoSkeletonData == null ? null : KaitSpineView.Create(makotoSkeletonData,
            battleKaitLayer != null ? battleKaitLayer : canvas.transform, new Vector2(115, 115), "Kait Speed Trail");
        if (ghost != null)
        {
            ghost.Root.position = position;
            ghost.Root.SetAsFirstSibling();
            ghost.Face(direction);
            ghost.PlayLoop(KaitSpineView.Run);
            ghost.SetTint(trailColor);
            var visual = new KaitTrailVisual { rect = ghost.Root, spine = ghost };
            activeTrailVisuals.Add(visual);
            return visual;
        }
        RectTransform fallbackSource = battleCells[run.katePos.x + run.katePos.y * KaitRun.BattleSize].rectTransform;
        RectTransform fallback = CreateFloatingPortrait(kaitPortrait, Color.clear, fallbackSource, new Vector2(115, 115));
        fallback.position = position;
        Image portrait = fallback.Find("Portrait")?.GetComponent<Image>();
        if (portrait != null) portrait.color = trailColor;
        var fallbackVisual = new KaitTrailVisual { rect = fallback };
        activeTrailVisuals.Add(fallbackVisual);
        return fallbackVisual;
    }

    private static Color KaitSpeedTrailColor(float tier)
    {
        tier = Mathf.Clamp01(tier);
        if (tier < 0.34f) return Color.Lerp(Cyan, Peach, tier / 0.34f);
        if (tier < 0.67f) return Color.Lerp(Peach, Gold, (tier - 0.34f) / 0.33f);
        return Color.Lerp(Gold, Coral, (tier - 0.67f) / 0.33f);
    }

    private void DestroyTrailVisual(KaitTrailVisual trail)
    {
        if (trail == null) return;
        activeTrailVisuals.Remove(trail);
        if (trail.spine != null) trail.spine.Destroy();
        else if (trail.rect != null) Destroy(trail.rect.gameObject);
    }

    private void ClearAllTrailVisuals()
    {
        if (activeTrailVisuals.Count == 0) return;
        KaitTrailVisual[] trails = activeTrailVisuals.ToArray();
        activeTrailVisuals.Clear();
        foreach (KaitTrailVisual trail in trails)
        {
            if (trail?.spine != null) trail.spine.Destroy();
            else if (trail?.rect != null)
            {
                if (Application.isPlaying) Destroy(trail.rect.gameObject);
                else DestroyImmediate(trail.rect.gameObject);
            }
        }
    }

    private IEnumerator FadeAndDestroyTrail(KaitTrailVisual trail, float duration)
    {
        if (trail == null || trail.rect == null) yield break;
        CanvasGroup group = trail.rect.gameObject.AddComponent<CanvasGroup>();
        float elapsed = 0f;
        while (elapsed < duration && trail.rect != null)
        {
            group.alpha = 1f - elapsed / duration;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        DestroyTrailVisual(trail);
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

    private static bool ShouldPlayBodyHurt(KaitEnemyAction action, bool playerInvincible)
    {
        return action != null && !playerInvincible && action.hitKate && action.damage > 0
            && action.type == KaitIntentType.Melee;
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
            if (afterHp <= 0) StartEnemyDeathAnimation(before.id);
            else StartCoroutine(FlashEnemyWhite(before.id));
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
            if (!result.enemyActions.Exists(action => action.hitKate))
            {
                GameAudio.PlayBodyHurt();
                GameAudio.PlayKaitDamageVoice(run.kateHp, run.config.kateMaxHp);
            }
            kaitSpine?.PlayOnce(run.kateHp <= 0 ? KaitSpineView.Die : KaitSpineView.Damage, run.kateHp <= 0 ? null : KaitSpineView.Idle);
            StartCoroutine(FlashKaitWhite());
            KaitEnemyAction incoming = result.enemyActions.Find(action => action.hitKate && action.damage > 0);
            PlayKaitHurtEffect(incoming != null ? incoming.from : run.katePos);
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
        // Death is a detached background presentation. It must never hold the
        // turn clock or delay the next direction input.
        BeginDetachedEnemyDeaths(healthBefore);
        yield return null;
    }

    private void BeginDetachedEnemyDeaths(List<KaitEnemy> healthBefore)
    {
        foreach (KaitEnemy before in healthBefore)
        {
            KaitEnemy resolved = run.enemies.Find(e => e.id == before.id);
            if (resolved == null || resolved.life != KaitEnemyLife.Dead || before.life == KaitEnemyLife.Dead) continue;
            EnemySpineView view = EnemySpine(before.id);
            if (view == null) continue;
            float deathRemaining = StartEnemyDeathAnimation(before.id);

            int index = before.pos.x + before.pos.y * KaitRun.BattleSize;
            if (index < 0 || index >= battleCells.Length || battleCells[index] == null) continue;
            RectTransform deathLayer = battleDeathLayers[index];
            Transform deathParent = battleActorLayer != null ? battleActorLayer : deathLayer;
            if (deathParent == null) continue;
            enemySpines.Remove(before.id);
            view.SetParent(deathParent);
            view.Root.position = battleCells[index].rectTransform.position;
            view.Root.SetAsFirstSibling();
            view.SetTint(Color.white);
            view.Face(before.type == KaitEnemyType.ShieldKnight ? before.facing : before.intent.direction);
            view.SetVisible(true);
            enemyDeathAnimationStartedAt.Remove(before.id);
            detachedEnemyDeaths.Add(view);
            StartCoroutine(DestroyDetachedEnemyDeath(view, deathRemaining));
        }
    }

    private float StartEnemyDeathAnimation(int enemyId)
    {
        EnemySpineView view = EnemySpine(enemyId);
        if (view == null) return 0f;
        if (!enemyDeathAnimationStartedAt.TryGetValue(enemyId, out float startedAt))
        {
            startedAt = Time.realtimeSinceStartup;
            enemyDeathAnimationStartedAt[enemyId] = startedAt;
            KaitEnemy deadEnemy = run.enemies.Find(e => e.id == enemyId) ??
                animatedEnemies?.Find(e => e.id == enemyId);
            if (deadEnemy != null) GameAudio.PlayEnemyDeath(deadEnemy.type, deadEnemy.id);
            else GameAudio.PlayEnemyDeath();
            view.SetHitFlash(1f);
            StartCoroutine(PlayEnemyDeathAfterHitFlash(view));
        }
        float sequenceDuration = EnemyHitFlashDuration + view.DeathDuration;
        return Mathf.Max(0f, sequenceDuration - (Time.realtimeSinceStartup - startedAt));
    }

    private IEnumerator PlayEnemyDeathAfterHitFlash(EnemySpineView view)
    {
        yield return new WaitForSecondsRealtime(EnemyHitFlashDuration);
        view?.SetHitFlash(0f);
        view?.PlayDeath();
    }

    private IEnumerator DestroyDetachedEnemyDeath(EnemySpineView view, float duration)
    {
        if (duration > 0f) yield return new WaitForSecondsRealtime(duration);
        const float fadeDuration = 0.24f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            view?.SetOpacity(1f - elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        detachedEnemyDeaths.Remove(view);
        view?.Destroy();
    }

    private IEnumerator AnimateEnemyAttackPreparation()
    {
        bool magicPrepared = false;
        bool anyPrepared = false;
        foreach (KaitEnemy enemy in run.enemies)
        {
            if (enemy.life != KaitEnemyLife.Active || enemy.intent.type == KaitIntentType.None) continue;
            EnemySpineView view = EnemySpine(enemy);
            if (view == null) continue;
            view.PlayPrepareAttack();
            GameAudio.PlayEnemyPrepareVoice(enemy.type, enemy.id);
            anyPrepared = true;
            if (enemy.intent.type == KaitIntentType.CrossBlast) magicPrepared = true;
        }
        if (magicPrepared) GameAudio.PlayMagicCharge();
        if (anyPrepared) yield return new WaitForSecondsRealtime(0.16f);
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
        text.gameObject.name = "Floating Damage";
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
        kaitSpine?.PlayOnce(animation,
            run.chainActive ? KaitSpineView.ChainDirectionChoice : KaitSpineView.Idle);
        skillDeck?.Pulse(skill);
        if (skill == KaitSkill.SwiftBoots || skill == KaitSkill.CatAgility)
        {
            PlaySpeedSkillFeedback(skill == KaitSkill.CatAgility);
            yield return PulseBattleUnit(run.katePos, Coral, 0.15f, -1, true);
        }
        else if (skill == KaitSkill.DreadSlash)
            PlayCombatEffectAtCell(KaitCombatEffectKind.DreadSlash, run.katePos,
                new Vector2(150f, 150f), 0.34f, 0.8f,
                HalfArrowAngle(KaitRun.Delta(run.currentDirection)), Vector2.zero,
                "Dread Slash Charge Effect", battleUnderEffectLayer);
    }

    private IEnumerator RunKaitSkillAnimation(KaitSkill skill)
    {
        yield return AnimateSkillPulse(skill);
        kaitSkillAnimationRoutine = null;
    }

    private KaitCombatEffectGraphic PlayDreadPressure(Vector2Int cell, Vector2Int delta)
    {
        delta = new Vector2Int(System.Math.Sign(delta.x), System.Math.Sign(delta.y));
        return PlayCombatEffectAtCell(KaitCombatEffectKind.DreadSlash, cell,
            new Vector2(150f, 150f), EnemyMoveDuration + .16f, .65f,
            HalfArrowAngle(delta), -((Vector2)delta).normalized * 38f,
            "Dread Slash A Pressure", battleUnderEffectLayer);
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
        EnemySpineView flashedEnemyView = null;
        if (enemy != null)
        {
            original = enemy.frozenActions > 0
                ? new Color(0.62f, 0.9f, 1f, 1f)
                : enemy.life == KaitEnemyLife.Preparing
                    ? new Color(1f, 1f, 1f, 0.68f)
                    : Color.white;
            EnemySpineView view = EnemySpine(enemy);
            if (view != null)
            {
                view.SetTint(color);
                // Enemy art is normally already white, so tinting it white did
                // not create a visible hit flash. Use the Spine fill material.
                if (color == Color.white)
                {
                    flashedEnemyView = view;
                    flashedEnemyView.SetHitFlash(1f);
                }
            }
            else if (battlePortraits[index] != null) battlePortraits[index].color = color;
        }
        else
        {
            kaitSpine?.SetTint(color);
            if (kaitSpine == null && battlePortraits[index] != null) battlePortraits[index].color = color;
        }

        yield return ScalePulse(battleUnitClips[index].rectTransform, 0.92f, 1.16f, duration);

        flashedEnemyView?.SetHitFlash(0f);

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
        yield return new WaitForSecondsRealtime(EnemyHitFlashDuration);
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
        if (!endAudioPlayed)
        {
            endAudioPlayed = true;
            if (run.won)
            {
                GameAudio.PlayWin();
                GameAudio.PlayKaitVictory();
            }
            else
            {
                GameAudio.PlayLose();
                GameAudio.PlayKaitFailure();
                if (kaitDefeatingEnemyId >= 0)
                    GameAudio.PlayEnemyDefeatedKaitVoice(kaitDefeatingEnemyType, kaitDefeatingEnemyId);
            }
        }
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
        string passiveList = string.Join("|", run.passives.ConvertAll(KaitPassiveCatalog.ShortName));
        string passiveTriggers = string.Join("|", result.passiveTriggers.ConvertAll(trigger => KaitPassiveCatalog.ShortName(trigger.passive)));
        string line = $"{run.turn},{result.globalDirection},{result.kaitWaited},{result.threatChanged},{result.chainStepCount},{result.chainKillCount},{result.chainPower},{result.chainMoves},{result.chainEndedByStrongEnemy},{result.chainEndedByWall},{start.x}:{start.y},{run.katePos.x}:{run.katePos.y},{run.kateHp},{result.slideDistance},{result.damageDealt},{run.kills},{run.directKills},{run.nonLethalHits},{run.chainActive},{run.momentum},{run.highestMomentum},{run.longestChainKills},{run.pushCount},{run.friendlyFireDamage},{run.activeWallStops},{run.spawnSuppressedCount},{run.riftBlocks},{run.wallSuppressedSpawns},{run.internalMergeCount},{run.internalSpawnCount},{run.clusterClearCount},{run.threatOrientedWaitCount},{run.emptyMapReachable},{run.emptyMapMaxInputs},{run.enemies.FindAll(e => e.life != KaitEnemyLife.Dead).Count},{run.spawns.Count},{run.highestThreat},{occupancy:F3},{run.threatLocks},{passiveList},{passiveTriggers},{run.endReason}\n";
        File.AppendAllText(logPath, line, Encoding.UTF8);
    }

    private IEnumerator FlashStatus()
    {
        Color old = statusText.color;
        statusText.color = Gold;
        yield return new WaitForSecondsRealtime(0.16f);
        statusText.color = old;
    }

    private IEnumerator PreviewSkillCardReveal(string path)
    {
        yield return new WaitForSecondsRealtime(.4f);
        var card = skillDeck.Owned[0];
        var e = new PointerEventData(EventSystem.current) { pointerId = -1 };
        card.OnPointerEnter(e);
        yield return new WaitForSecondsRealtime(.4f);
        CaptureCanvasToPng(path);
        Debug.Log($"Preview QA hover: y={card.Rect.anchoredPosition.y}, expanded={card.ShouldPreviewAt(Time.unscaledTime)}");
        card.OnPointerExit(e);
        yield return new WaitForSecondsRealtime(KaitSkillCard.PreviewHoldSeconds + .4f);
        Debug.Log($"Preview QA timeout: y={card.Rect.anchoredPosition.y}, expanded={card.ShouldPreviewAt(Time.unscaledTime)}");
        card.OnPointerClick(e);
        yield return new WaitForSecondsRealtime(.4f);
        skillDeck.DismissOtherPreviews(null);
        yield return new WaitForSecondsRealtime(.4f);
        CaptureCanvasToPng(path + ".closed.png");
        Debug.Log($"Preview QA outside: y={card.Rect.anchoredPosition.y}, expanded={card.ShouldPreviewAt(Time.unscaledTime)}");
        Application.Quit();
    }

    private IEnumerator PreviewSkillCardCast(string path)
    {
        yield return new WaitForSecondsRealtime(.4f);
        var card = skillDeck.Owned[0];
        var pointer = new PointerEventData(EventSystem.current) { pointerId = -1, button = PointerEventData.InputButton.Left,
            position = RectTransformUtility.WorldToScreenPoint(null, card.Rect.position) };
        card.OnPointerDown(pointer); card.OnBeginDrag(pointer);
        pointer.position = RectTransformUtility.WorldToScreenPoint(null, card.Rect.parent.TransformPoint(KaitSkillDeck.CastZone.center));
        card.OnDrag(pointer); card.OnPointerUp(pointer); card.OnEndDrag(pointer); card.OnPointerClick(pointer);
        Debug.Log($"Skill card drag QA: cooldown={run.SkillCooldown(KaitSkill.SwiftBoots)}, speedModifiers={run.activeSpeedModifiers.Count}, turn={run.turn}");
        yield return new WaitForSecondsRealtime(.65f);
        if (!string.IsNullOrEmpty(path)) { CaptureCanvasToPng(path); yield return new WaitForSecondsRealtime(.2f); Application.Quit(); }
    }

    private IEnumerator CaptureAndQuit(string path, int demoSteps)
    {
        if (CommandLineValue("-kaitTutorialPages") == "all")
        {
            var book = tutorialOverlay.GetComponent<KaitTutorialBook>();
            tutorialOverlay.SetActive(true);
            tutorialOverlay.transform.SetAsLastSibling();
            int beforeTurn = run.turn;
            Vector2Int beforePosition = run.katePos;
            HandleDirection(KaitDirection.Right);
            Debug.Log($"Tutorial input QA: blocked={run.turn == beforeTurn && run.katePos == beforePosition}");
            for (int page = 0; page < book.PageCount; page++)
            {
                book.ShowPage(page);
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                Canvas.ForceUpdateCanvases();
                int overflow = 0;
                foreach (Text text in book.GetComponentsInChildren<Text>())
                    if (text.preferredHeight > text.rectTransform.rect.height + 2) { overflow++; Debug.LogWarning($"Tutorial text overflow: {text.text}"); }
                CaptureCanvasToPng(path + $".page{page + 1:00}.png");
                Debug.Log($"Tutorial page QA: page={page + 1}, art={book.IllustrationLoaded}, overflow={overflow}");
            }
            book.Next();
            Debug.Log($"Tutorial close QA: closed={!tutorialOverlay.activeSelf}");
            yield return new WaitForEndOfFrame();
            CaptureCanvasToPng(path + ".closed.png");
            Application.Quit();
            yield break;
        }
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
        if (CommandLineValue("-kaitGardenPreview") == "motion" && layeredGarden != null)
        {
            layeredGarden.RenderShadows();
            SaveGardenShadowPreview(path + ".shadow-before.png");
            yield return new WaitForSecondsRealtime(1.5f);
            layeredGarden.RenderShadows();
            SaveGardenShadowPreview(path + ".shadow-after.png");
            Debug.Log($"Garden QA: live decoration count={layeredGarden.DecorationCount}, artReady={KaitLayeredGarden.ArtReady}");
        }
        CaptureCanvasToPng(path);
        yield return new WaitForSecondsRealtime(0.2f);
        Application.Quit();
    }

    private void SaveGardenShadowPreview(string path)
    {
        var field = layeredGarden.ShadowField;
        var previous = RenderTexture.active;
        var texture = new Texture2D(field.width, field.height, TextureFormat.RGBA32, false);
        try
        {
            RenderTexture.active = field;
            texture.ReadPixels(new Rect(0, 0, field.width, field.height), 0, 0);
            texture.Apply(false);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally { RenderTexture.active = previous; Destroy(texture); }
    }

    private void CaptureCanvasToPng(string path)
    {
        const int width = 1920, height = 1080;
        RenderMode previousMode = canvas.renderMode;
        Camera previousCamera = canvas.worldCamera;
        float previousPlaneDistance = canvas.planeDistance;
        RenderTexture previousActive = RenderTexture.active;
        var cameraObject = new GameObject("Automated Screenshot Camera", typeof(Camera));
        Camera captureCamera = cameraObject.GetComponent<Camera>();
        RenderTexture target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        try
        {
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = Color.black;
            captureCamera.orthographic = true;
            captureCamera.orthographicSize = height * 0.5f;
            captureCamera.aspect = width / (float)height;
            captureCamera.nearClipPlane = 0.01f;
            captureCamera.farClipPlane = 100f;
            captureCamera.transform.position = new Vector3(0f, 0f, -10f);
            captureCamera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = captureCamera;
            canvas.planeDistance = 10f;
            Canvas.ForceUpdateCanvases();
            if (layeredGarden != null) layeredGarden.RenderShadows();
            captureCamera.Render();
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply(false);
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            canvas.renderMode = previousMode;
            canvas.worldCamera = previousCamera;
            canvas.planeDistance = previousPlaneDistance;
            RenderTexture.active = previousActive;
            captureCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(target);
            Destroy(texture);
            Destroy(cameraObject);
        }
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

    private static SkeletonDataAsset LoadEffectSkeleton(string assetName)
    {
        return Resources.Load<SkeletonDataAsset>($"Effects/Kait/{assetName}/{assetName}_SkeletonData");
    }

    private SpineEffectView PlayEffectAtCell(SkeletonDataAsset data, Vector2Int cell, Vector2 size,
        float playbackSpeed = 1f, float scale = 1f, float rotation = 0f, Color? tint = null,
        bool groundLayer = false, bool autoDestroy = true, string effectName = "Battle Effect")
    {
        if (data == null || !InsideBattle(cell) || battleCells == null) return null;
        int index = cell.x + cell.y * KaitRun.BattleSize;
        if (index < 0 || index >= battleCells.Length || battleCells[index] == null) return null;

        Transform parent = groundLayer
            ? battleCells[index].transform
            : (battleActorLayer != null ? battleActorLayer : canvas.transform);
        SpineEffectView view = SpineEffectView.Create(data, parent, size, "texiao", effectName,
            playbackSpeed, 0.96f, true);
        if (view == null) return null;

        if (groundLayer)
        {
            view.Root.anchoredPosition = Vector2.zero;
            view.Root.SetSiblingIndex(Mathf.Min(4, parent.childCount - 1));
        }
        else
        {
            view.Root.localPosition = parent.InverseTransformPoint(battleCells[index].rectTransform.position);
            view.Root.SetAsLastSibling();
        }
        view.SetScale(scale);
        view.SetRotation(rotation);
        if (tint.HasValue) view.SetTint(tint.Value);
        activeEffectViews.Add(view);
        if (autoDestroy) StartCoroutine(DestroyEffectViewAfter(view, Mathf.Max(0.08f, view.Duration + 0.03f)));
        return view;
    }

    private IEnumerator DestroyEffectViewAfter(SpineEffectView view, float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        activeEffectViews.Remove(view);
        view?.Destroy();
    }

    private void DestroyEffectView(SpineEffectView view)
    {
        if (view == null) return;
        activeEffectViews.Remove(view);
        view.Destroy();
    }

    private void ClearAllEffectViews()
    {
        foreach (SpineEffectView view in activeEffectViews) view?.Destroy();
        activeEffectViews.Clear();
    }

    private KaitCombatEffectGraphic PlayCombatEffectAtCell(KaitCombatEffectKind kind, Vector2Int cell,
        Vector2 size, float duration, float intensity = 0.5f, float rotation = 0f,
        Vector2 offset = default, string effectName = "Combat Effect", RectTransform layerOverride = null)
    {
        bool behindActors = kind == KaitCombatEffectKind.Kill ||
            kind == KaitCombatEffectKind.ChainKill || kind == KaitCombatEffectKind.SwordArc ||
            kind == KaitCombatEffectKind.Push;
        RectTransform effectLayer = layerOverride != null ? layerOverride : KaitCombatLayers.IsEnemyImpact(kind) && battleEnemyHitLayer != null
            ? battleEnemyHitLayer
            : behindActors && battleUnderEffectLayer != null
            ? battleUnderEffectLayer
            : battleEffectLayer != null
                ? battleEffectLayer
                : battleActorLayer;
        if (!InsideBattle(cell) || battleCells == null || effectLayer == null) return null;
        int index = cell.x + cell.y * KaitRun.BattleSize;
        if (index < 0 || index >= battleCells.Length || battleCells[index] == null) return null;

        var effectObject = new GameObject(effectName, typeof(RectTransform), typeof(CanvasRenderer),
            typeof(KaitCombatEffectGraphic));
        effectObject.transform.SetParent(effectLayer, false);
        RectTransform rect = effectObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.localPosition = effectLayer.InverseTransformPoint(battleCells[index].rectTransform.position) +
            new Vector3(offset.x, offset.y, 0f);
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        rect.SetAsLastSibling();

        CombatEffectColors(kind, out Color primary, out Color secondary);
        KaitCombatEffectGraphic graphic = effectObject.GetComponent<KaitCombatEffectGraphic>();
        graphic.Configure(kind, primary, secondary, intensity);
        activeCombatEffects.RemoveAll(item => item == null);
        activeCombatEffects.Add(graphic);
        graphic.Play(duration);
        return graphic;
    }

    private RawImage PlaySwordSlashAtCell(Vector2Int cell, Vector2 size, float duration,
        float rotation, Vector2 offset, Color tint, string effectName)
    {
        RectTransform effectLayer = battleEffectLayer != null ? battleEffectLayer : battleActorLayer;
        if (swordSlashTexture == null || !InsideBattle(cell) || battleCells == null || effectLayer == null)
            return null;

        int index = cell.x + cell.y * KaitRun.BattleSize;
        var effectObject = new GameObject(effectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        effectObject.transform.SetParent(effectLayer, false);
        RectTransform rect = effectObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.localPosition = effectLayer.InverseTransformPoint(battleCells[index].rectTransform.position) +
            new Vector3(offset.x, offset.y, 0f);
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        rect.SetAsLastSibling();

        RawImage slash = effectObject.GetComponent<RawImage>();
        slash.texture = swordSlashTexture;
        slash.uvRect = new Rect(0f, 0f, 1f, 1f);
        slash.color = new Color(tint.r, tint.g, tint.b, 0f);
        slash.raycastTarget = false;
        activeSwordSlashEffects.RemoveAll(item => item == null);
        activeSwordSlashEffects.Add(slash);
        effectObject.AddComponent<KaitSwordSlashEffectView>().Configure(slash, duration, tint);
        return slash;
    }

    private void ClearAllCombatEffects()
    {
        foreach (KaitMageEffectGraphic effect in mageImpacts) if (effect != null) Destroy(effect.gameObject);
        mageImpacts.Clear();
        firingWarlocks.Clear();
        foreach (KaitCombatEffectGraphic graphic in activeCombatEffects)
            if (graphic != null) Destroy(graphic.gameObject);
        activeCombatEffects.Clear();
        foreach (RawImage slash in activeSwordSlashEffects)
            if (slash != null) Destroy(slash.gameObject);
        activeSwordSlashEffects.Clear();
    }

    private static void CombatEffectColors(KaitCombatEffectKind kind, out Color primary, out Color secondary)
    {
        switch (kind)
        {
            case KaitCombatEffectKind.SwordArc:
                primary = new Color(1f, 0.97f, 0.9f, 0.98f);
                secondary = Color.white;
                return;
            case KaitCombatEffectKind.Block:
                primary = new Color(Cyan.r, Cyan.g, Cyan.b, 0.98f);
                secondary = Cream;
                return;
            case KaitCombatEffectKind.Kill:
                primary = new Color(Coral.r, Coral.g, Coral.b, 0.74f);
                secondary = new Color(Cream.r, Cream.g, Cream.b, 0.82f);
                return;
            case KaitCombatEffectKind.ChainKill:
                primary = new Color(Gold.r, Gold.g, Gold.b, 0.78f);
                secondary = new Color(Cream.r, Cream.g, Cream.b, 0.84f);
                return;
            case KaitCombatEffectKind.EnemyHit:
                primary = new Color(Wine.r, Wine.g, Wine.b, 0.98f);
                secondary = Cream;
                return;
            case KaitCombatEffectKind.MagicCast:
            case KaitCombatEffectKind.MagicImpact:
                primary = new Color(Wine.r, Wine.g, Wine.b, 0.9f);
                secondary = Cyan;
                return;
            case KaitCombatEffectKind.Ice:
                primary = new Color(Cyan.r, Cyan.g, Cyan.b, 0.92f);
                secondary = Cream;
                return;
            case KaitCombatEffectKind.Phantom:
                primary = new Color(Wine.r, Wine.g, Wine.b, 0.78f);
                secondary = Peach;
                return;
            case KaitCombatEffectKind.Speed:
                primary = Gold;
                secondary = Cyan;
                return;
            case KaitCombatEffectKind.DreadSlash:
                primary = new Color(Coral.r, Coral.g, Coral.b, 0.94f);
                secondary = Cream;
                return;
            default:
                primary = new Color(Gold.r, Gold.g, Gold.b, 0.92f);
                secondary = Cream;
                return;
        }
    }

    private void PlaySpeedSkillFeedback(bool ultimate)
    {
        float intensity = ultimate ? 1f : 0.55f;
        var speedEffect = PlayCombatEffectAtCell(KaitCombatEffectKind.Speed, run.katePos,
            Vector2.one * 145f, ultimate ? .46f : .4f, intensity,
            effectName: ultimate ? "Cat Agility Feather B" : "Swift Boots Feather B", layerOverride: battleUnderEffectLayer);
        if (speedEffect != null)
        {
            var follow = speedEffect.gameObject.AddComponent<KaitSpeedBuffFollow>();
            follow.Actor = () => {
                if (hideKate) for (int i=activeFloatingKaits.Count-1;i>=0;i--)
                    if (activeFloatingKaits[i]?.Root != null && activeFloatingKaits[i].Root.gameObject.activeInHierarchy)
                        return activeFloatingKaits[i].Root;
                return kaitSpine?.Root;
            };
            follow.Tint = () => KaitSpeedTrailColor(Mathf.Clamp01((run.momentum-1)/4f));
            follow.Direction = () => KaitRun.Delta(run.currentDirection);
            follow.Refresh();
        }

        int count = ultimate ? 5 : 3;
        Vector2Int delta = KaitRun.Delta(run.currentDirection);
        Vector3 backwards = new Vector3(-delta.x, -delta.y, 0f);
        RectTransform speedActor = speedEffect?.GetComponent<KaitSpeedBuffFollow>()?.Actor?.Invoke();
        Vector3 origin = speedActor != null ? speedActor.position : battleCells[run.katePos.x + run.katePos.y * KaitRun.BattleSize].rectTransform.position;
        for (int i = 0; i < count; i++)
        {
            KaitTrailVisual trail = CreateGhostToken(origin + backwards * (9f + i * 8f),
                run.momentum, run.currentDirection,
                Mathf.Lerp(0.82f, 0.32f, i / (float)Mathf.Max(1, count - 1)));
            StartCoroutine(FadeAndDestroyTrail(trail, Mathf.Lerp(0.28f, 0.42f, i / (float)count)));
        }
    }

    private void PlayGroundSmokeBurst(Vector2Int cell, Vector2Int direction, Color tint,
        float scale, string effectName)
    {
        Vector2 perpendicular = direction == Vector2Int.zero
            ? Vector2.right
            : new Vector2(-direction.y, direction.x);
        for (int i = 0; i < 2; i++)
        {
            Color puffTint = tint;
            puffTint.a *= i == 0 ? 0.9f : 0.58f;
            SpineEffectView view = PlayEffectAtCell(shadowSmokeEffect, cell,
                new Vector2(136f, 80f), 3.8f, scale * (i == 0 ? 1f : 0.82f),
                0f, puffTint, false, true, effectName + " Smoke");
            if (view == null) continue;
            float side = i == 0 ? -14f : 14f;
            view.Root.localPosition += new Vector3(
                direction.x * 24f + perpendicular.x * side,
                -36f + perpendicular.y * side,
                0f);
            view.Root.SetSiblingIndex(0);
        }
    }

    private void PlayKaitImpactEffect(KaitTurnResult result, bool killed, Vector2Int cell,
        int chainCountOverride = -1, Vector2Int? attackerCellOverride = null)
    {
        Vector2Int attackDirection = KaitRun.Delta(result.kaitDirection);
        float rotation = HalfArrowAngle(attackDirection);
        Vector2Int attackerCell = attackerCellOverride ?? (cell - attackDirection);
        if (!InsideBattle(attackerCell)) attackerCell = cell;
        Vector2 contactOffset = -(Vector2)attackDirection * 16f + Vector2.down * 8f;

        if (result.playerAttackBlocked)
        {
            PlayCombatEffectAtCell(KaitCombatEffectKind.Block, cell,
                new Vector2(108f, 108f), 0.24f, 1f, rotation,
                contactOffset, "Shield Block Effect");
            return;
        }

        if (killed)
        {
            int visibleChainCount = chainCountOverride > 0 ? chainCountOverride : result.chainKillCount;
            if (visibleChainCount <= 1)
            {
                PlayCombatEffectAtCell(KaitCombatEffectKind.Kill, cell,
                    new Vector2(112f, 112f), 0.32f, 0.78f,
                    rotation, contactOffset, "Kill Impact Effect");
            }
            else
            {
                float chainStrength = Mathf.InverseLerp(2f, 10f, Mathf.Clamp(visibleChainCount, 2, 10));
                float chainSize = Mathf.Lerp(116f, 124f, chainStrength);
                PlayCombatEffectAtCell(KaitCombatEffectKind.ChainKill, cell,
                    new Vector2(chainSize, chainSize), Mathf.Lerp(0.36f, 0.4f, chainStrength),
                    Mathf.Lerp(0.5f, 0.86f, chainStrength), rotation,
                    contactOffset, "Chain Kill Burst");
            }
            return;
        }

        bool dealtDamage = result.damageDealt > 0 || result.collisionDamage > 0;
        if (dealtDamage)
        {
            PlayCombatEffectAtCell(KaitCombatEffectKind.NormalHit, cell,
                new Vector2(108f, 108f), 0.26f, 0.62f, rotation,
                contactOffset, "Normal Damage Spark");
            return;
        }

        bool pushedWithoutDamage = result.pushed || result.pushBlockedByWall || result.pushBlockedByUnit;
        if (pushedWithoutDamage)
        {
            // Short pressure streaks mark the contact side, not the whole attacker cell.
            PlayCombatEffectAtCell(KaitCombatEffectKind.Push, cell,
                new Vector2(160f, 160f), 0.32f, 0.5f, rotation,
                -(Vector2)attackDirection * 50f + Vector2.down * 10f, "Zero Damage Push Lines");
        }
    }

    private EnemyLandingDust PlayEnemyLanding(EnemySpineView view)
    {
        view.PlayLanding();
        return EnemyLandingDust.Create(view, battleUnderEffectLayer);
    }

    private KaitMageEffectGraphic PlayMageImpact(Vector2Int target, IEnumerable<Vector2Int> affected)
    {
        if (!InsideBattle(target) || battleUnderEffectLayer == null) return null;
        Image targetCell = battleCells[target.x + target.y*KaitRun.BattleSize];
        if (targetCell == null) return null;
        var go = new GameObject("Mage Cross A", typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitMageEffectGraphic));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(battleUnderEffectLayer,false);
        rect.position = targetCell.rectTransform.position;
        Vector3[] corners = new Vector3[4];
        targetCell.rectTransform.GetWorldCorners(corners);
        Vector2 cellSize = rect.InverseTransformPoint(corners[2])-rect.InverseTransformPoint(corners[0]);
        rect.sizeDelta = cellSize*3.59375f;
        var legal = new List<Rect>();
        foreach (Vector2Int cell in new HashSet<Vector2Int>(affected))
        {
            if (!InsideBattle(cell) || run.walls[cell.x,cell.y]) continue;
            Image image = battleCells[cell.x+cell.y*KaitRun.BattleSize];
            if (image == null) continue;
            image.rectTransform.GetWorldCorners(corners);
            Vector2 min=rect.InverseTransformPoint(corners[0]),max=rect.InverseTransformPoint(corners[2]);
            legal.Add(UnityEngine.Rect.MinMaxRect(min.x,min.y,max.x,max.y));
        }
        KaitMageEffectGraphic effect = go.GetComponent<KaitMageEffectGraphic>();
        effect.ConfigureImpact(legal);
        mageImpacts.RemoveAll(item=>item==null);mageImpacts.Add(effect);
        return effect;
    }

    private KaitCombatEffectGraphic PlayArrowImpact(Vector2Int target, Vector2Int source, bool hitsKait)
    {
        Vector2 direction = (Vector2)(target - source);
        if (direction.sqrMagnitude < .01f) direction = Vector2.right;
        direction.Normalize();
        return PlayCombatEffectAtCell(KaitCombatEffectKind.ArrowImpact, target,
            Vector2.one * 112f, .24f, .5f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg,
            -direction * 20f, "Arrow Impact A", hitsKait ? battleEffectLayer : battleEnemyHitLayer);
    }

    private KaitCombatEffectGraphic PlayKaitHurtEffect(Vector2Int sourceCell)
    {
        Vector2 direction = (Vector2)(run.katePos - sourceCell);
        if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
        direction.Normalize();
        return PlayCombatEffectAtCell(KaitCombatEffectKind.KaitHurt, run.katePos,
            new Vector2(112f, 112f), 0.26f, 0.5f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg,
            -direction * 14f + Vector2.down * 8f, "Kait Hurt B");
    }

    private KaitSwordAtlasView StartKaitSwordTipTrail(KaitSpineView source, bool finisher)
    {
        RectTransform layer = battleEffectLayer != null ? battleEffectLayer : battleActorLayer;
        if (source == null) return null;
        return KaitSwordAtlasView.Create(source, layer, finisher);
    }

    private IEnumerator PreviewCombatVfx(string preview, string screenshotPath = "")
    {
        yield return new WaitForSecondsRealtime(0.25f);
        Vector2Int attacker = run.katePos;
        Vector2Int target = attacker + Vector2Int.right;
        if (!InsideBattle(target)) target = attacker + Vector2Int.left;
        string normalized = preview.Trim().ToLowerInvariant();
        if (normalized.StartsWith("dread"))
        {
            var direction = normalized.Contains("left") ? Vector2Int.left : normalized.Contains("up") ? Vector2Int.up : normalized.Contains("down") ? Vector2Int.down : Vector2Int.right;
            run.enemies.Clear();
            var actions = new List<KaitEnemyAction>();
            animatedEnemies = new List<KaitEnemy>();
            for (int i = 0; i < 3; i++)
            {
                var from = new Vector2Int(2 + (direction.y != 0 ? i : 0), 2 + (direction.x != 0 ? i : 0));
                var enemy = new KaitEnemy { id = 900+i, type = KaitEnemyType.Grunt, pos = from, hp = 2, maxHp = 2 };
                animatedEnemies.Add(enemy);
                run.enemies.Add(new KaitEnemy { id=enemy.id, type=enemy.type, pos=from+direction, hp=2,maxHp=2 });
                actions.Add(new KaitEnemyAction { enemyId=enemy.id,type=KaitIntentType.Move,from=from,to=from+direction });
            }
            RefreshBattle();
            var routine = AnimateAllEnemyActions(actions, true);
            bool capturedDread = false;
            while (routine.MoveNext())
            {
                if (!capturedDread && activeCombatEffects.FindAll(e=>e!=null && e.UsesDreadAtlas).Count == 3)
                {
                    foreach(var effect in activeCombatEffects.FindAll(e=>e!=null && e.UsesDreadAtlas))
                    {
                        Debug.Assert(effect.transform.parent==battleUnderEffectLayer);
                        effect.SetProgress(.28f);
                        effect.Rebuild(CanvasUpdate.PreRender);
                    }
                    Canvas.ForceUpdateCanvases();
                    if (!string.IsNullOrEmpty(screenshotPath)) CaptureCanvasToPng(screenshotPath);
                    capturedDread = true;
                }
                yield return routine.Current;
            }
            yield return new WaitForSecondsRealtime(.6f);
            Debug.Assert(!activeCombatEffects.Exists(e=>e!=null && e.UsesDreadAtlas));
            if (!string.IsNullOrEmpty(screenshotPath)) Application.Quit();
            yield break;
        }
        if (normalized.StartsWith("speed"))
        {
            // Explicit diagnostic setup only; normal play obtains momentum from the rules.
            int momentum = normalized.Contains("high") ? 5 : normalized.Contains("mid") ? 4 : 1;
            typeof(KaitRun).GetProperty(nameof(KaitRun.momentum)).SetValue(run, momentum);
            var direction=normalized.Contains("left") ? KaitDirection.Left : KaitDirection.Right;
            typeof(KaitRun).GetProperty(nameof(KaitRun.currentDirection)).SetValue(run, direction);
            kaitSpine.Face(direction);
            kaitSpine.PlayOnce(KaitSpineView.JoyShort);
            PlaySpeedSkillFeedback(normalized.Contains("high"));
            var effect=activeCombatEffects.FindLast(item=>item!=null && item.UsesSpeedAtlas);
            Debug.Assert(effect!=null,"Speed skill must use Feather B");
            if(effect==null) { Application.Quit(1);yield break; }
            var follow=effect.GetComponent<KaitSpeedBuffFollow>();
            if(normalized.Contains("move"))
            {
                var actor=CreateFloatingKait(battleCells[run.katePos.x+run.katePos.y*KaitRun.BattleSize].rectTransform,direction);
                hideKate=true;kaitSpine.SetVisible(false);
                actor.Root.position+=Vector3.right*35;actor.PlayLoop(KaitSpineView.Run);
                follow.Refresh();
                Debug.Assert(Vector3.Distance(effect.transform.position,actor.Root.TransformPoint(new Vector3(0,-actor.Root.rect.height*.38f,0)))<.01f);
            }
            Debug.Assert(effect.transform.parent==battleUnderEffectLayer);
            Debug.Assert(effect.color==KaitSpeedTrailColor(Mathf.Clamp01((momentum-1)/4f)));
            yield return new WaitForEndOfFrame(); effect.SetProgress(.3f);
            if(!string.IsNullOrEmpty(screenshotPath))
            {
                CaptureCanvasToPng(screenshotPath);
                yield return new WaitForSecondsRealtime(.6f);
                Debug.Assert(effect==null,"Speed effect must expire independently");Application.Quit();
            }
            yield break;
        }
        if (normalized.StartsWith("boundary"))
        {
            KaitDirection direction = normalized.Contains("left") ? KaitDirection.Left :
                normalized.Contains("up") ? KaitDirection.Up : normalized.Contains("down") ? KaitDirection.Down : KaitDirection.Right;
            Vector2Int slideStart = run.katePos;
            run.enemies.Clear(); RefreshBattle();
            animatedEnemies = new List<KaitEnemy>();
            kaitSpine.Face(direction);
            yield return new WaitForSecondsRealtime(.15f);
            var stop = run.TryGlobalInput(direction);
            Debug.Assert(stop.stoppedByWall || stop.chainEndedByWall, "QA move must reach a boundary");
            yield return AnimateKateSlide(stop, slideStart);
            var dust = activeCombatEffects.FindLast(effect => effect != null && effect.UsesBoundaryAtlas);
            Debug.Assert(dust != null, "Wall-stop path must emit approved dust");
            if (dust == null) { Application.Quit(1); yield break; }
            Debug.Assert(dust.transform.parent == battleUnderEffectLayer);
            Vector3 contact = kaitSpine.Root.TransformPoint(new Vector3(0, -kaitSpine.Root.rect.height * .38f, 0));
            Debug.Assert(Vector3.Distance(contact, dust.transform.position) < .01f, "Dust must sit on the foot shadow");
            yield return new WaitForEndOfFrame();
            dust.SetProgress(.28f);
            if (!string.IsNullOrEmpty(screenshotPath))
            {
                CaptureCanvasToPng(screenshotPath);
                // The stop pose may be interrupted; the already emitted dust stays on the floor.
                kaitSpine.PlayLoop(KaitSpineView.Run);
                yield return new WaitForSecondsRealtime(.5f);
                Debug.Assert(dust == null, "Boundary dust must expire independently");
                Application.Quit();
            }
            yield break;
        }
        if (normalized.StartsWith("landing"))
        {
            KaitEnemyType type = normalized.Contains("sword") ? KaitEnemyType.Swordsman :
                normalized.Contains("archer") ? KaitEnemyType.Archer :
                normalized.Contains("guard") ? KaitEnemyType.Guard :
                normalized.Contains("mage") ? KaitEnemyType.Warlock :
                normalized.Contains("boss") ? KaitEnemyType.ShieldKnight : KaitEnemyType.Grunt;
            var enemy=new KaitEnemy {id=int.MaxValue,type=type,pos=target,hp=2,maxHp=2,life=KaitEnemyLife.Preparing};
            run.enemies.Clear();run.enemies.Add(enemy);RefreshBattle();
            EnemySpineView actor=EnemySpine(enemy);
            yield return new WaitForSecondsRealtime(.2f);
            EnemyLandingDust dust=PlayEnemyLanding(actor);
            Debug.Assert(dust!=null,"Landing must arm foot dust");
            if (normalized.Contains("cancel"))
            {
                actor.PlayAttack();
                yield return new WaitForSecondsRealtime(.1f);
                Debug.Assert(dust==null,"Interrupted landing must cancel dust");
            }
            else
            {
                float deadline=Time.realtimeSinceStartup+2;
                while(dust!=null && !dust.Emitted && Time.realtimeSinceStartup<deadline) yield return null;
                Debug.Assert(dust!=null && dust.Emitted,"Landing contact must emit dust");
                if(dust==null || !dust.Emitted) { Application.Quit(1);yield break; }
                Debug.Assert(actor.CurrentAnimation.TrackTime>=EnemyLandingDust.ContactTime);
                Debug.Assert(dust.Graphic.UsesLandingAtlas && dust.transform.parent==battleUnderEffectLayer);
                Debug.Assert(Vector3.Distance(dust.transform.position,actor.GroundPosition)<.01f);
                yield return new WaitForEndOfFrame();
                dust.Graphic.SetProgress(.28f);
            }
            if (!string.IsNullOrEmpty(screenshotPath))
            {
                CaptureCanvasToPng(screenshotPath);
                yield return new WaitForSecondsRealtime(.45f);
                Debug.Assert(dust==null,"Foot dust must clean itself up");
                Application.Quit();
            }
            yield break;
        }
        if (normalized.StartsWith("mage"))
        {
            Vector2Int center = attacker;
            if (normalized.Contains("edge")) center = new Vector2Int(1,1);
            if (normalized.Contains("wall"))
                for (int x=1;x<5;x++) for (int y=1;y<6;y++)
                    if (!run.walls[x,y] && run.walls[x+1,y]) center = new Vector2Int(x,y);
            var spell = new KaitIntent { type=KaitIntentType.CrossBlast, target=center, damage=1 };
            foreach (Vector2Int delta in new[] {Vector2Int.zero,Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})
            {
                Vector2Int cell=center+delta;
                if (InsideBattle(cell) && !run.walls[cell.x,cell.y]) spell.affectedCells.Add(cell);
            }
            Vector2Int casterCell=new Vector2Int(4,5);
            var caster=new KaitEnemy {id=int.MaxValue,type=KaitEnemyType.Warlock,pos=casterCell,hp=2,maxHp=2,
                life=KaitEnemyLife.Active,rangedState=KaitRangedState.Aim,intent=spell};
            run.enemies.Clear();run.enemies.Add(caster);
            RefreshBattle();
            yield return new WaitForEndOfFrame();
            int warningCount=0;
            foreach (var warning in battleMageWarnings) if (warning!=null && warning.gameObject.activeSelf) warningCount++;
            Debug.Assert(warningCount==spell.affectedCells.Count,"Mage aim must match legal cells");
            if (normalized.Contains("aim"))
            {
                if (!string.IsNullOrEmpty(screenshotPath)) { CaptureCanvasToPng(screenshotPath);Application.Quit(); }
                yield break;
            }
            var action=new KaitEnemyAction {enemyId=caster.id,type=KaitIntentType.CrossBlast,from=casterCell,to=center,damage=1};
            action.affectedCells.AddRange(spell.affectedCells);
            animatedEnemies=new List<KaitEnemy>(run.enemies);
            StartCoroutine(AnimateAllEnemyActions(new List<KaitEnemyAction>{action}));
            float timeout=Time.realtimeSinceStartup+3f;
            while (mageImpacts.Count==0 && Time.realtimeSinceStartup<timeout) yield return null;
            Debug.Assert(mageImpacts.Count>0,"Enemy attack must create Mage A");
            if (mageImpacts.Count==0) { Application.Quit(1);yield break; }
            KaitMageEffectGraphic impact=mageImpacts[mageImpacts.Count-1];
            Debug.Assert(impact!=null && impact.AtlasReady && impact.CellCount==spell.affectedCells.Count);
            Debug.Assert(impact.transform.parent==battleUnderEffectLayer);
            yield return new WaitForEndOfFrame();
            if (impact!=null) impact.SetProgress(.28f);
            if (!string.IsNullOrEmpty(screenshotPath))
            {
                CaptureCanvasToPng(screenshotPath);
                yield return new WaitForSecondsRealtime(.4f);
                Debug.Assert(impact==null,"Mage impact must expire independently");
                Application.Quit();
            }
            yield break;
        }
        if (normalized.StartsWith("slash"))
        {
            bool finisher = normalized.Contains("kill");
            kaitSpine.Face(normalized.Contains("left") ? KaitDirection.Left : KaitDirection.Right);
            for (int frame = 0; frame < 9; frame++)
            {
                // Restart each sample: PNG encoding can outlast the entire slash,
                // so serial screenshots of one live attack skip its bright frames.
                kaitSpine.PlayOnce(finisher ? KaitSpineView.ChainAttack : KaitSpineView.Attack);
                StartKaitSwordTipTrail(kaitSpine, finisher);
                float sampleTime = (finisher ? 0.08f : 0.46f) + 0.018f + frame * 0.035f;
                yield return new WaitForSecondsRealtime(sampleTime);
                yield return new WaitForEndOfFrame();
                if (!string.IsNullOrEmpty(screenshotPath))
                    CaptureCanvasToPng(screenshotPath.Replace(".png", "-" + frame + ".png"));
            }
            if (!string.IsNullOrEmpty(screenshotPath)) Application.Quit();
            yield break;
        }
        bool blocked = normalized == "block";
        bool pushed = normalized.StartsWith("push");
        KaitDirection previewDirection = target.x >= attacker.x ? KaitDirection.Right : KaitDirection.Left;
        if (pushed)
        {
            previewDirection = normalized.Contains("left") ? KaitDirection.Left :
                normalized.Contains("up") ? KaitDirection.Up :
                normalized.Contains("down") ? KaitDirection.Down : KaitDirection.Right;
            target = attacker + KaitRun.Delta(previewDirection);
        }
        bool killed = normalized == "kill" || normalized == "chain";
        int chainCount = normalized == "chain" ? 5 : killed ? 1 : 0;
        // Only this explicit QA mode creates a stationary target, so screenshots
        // verify the real actor/effect layering rather than an empty cell.
        var previewEnemy = new KaitEnemy
        {
            id = int.MaxValue, type = blocked ? KaitEnemyType.ShieldKnight : KaitEnemyType.Grunt,
            pos = target, hp = 2, maxHp = 2, life = KaitEnemyLife.Active,
            facing = Vector2Int.left
        };
        run.enemies.RemoveAll(e => e.pos == target);
        run.enemies.Add(previewEnemy);
        RefreshBattle();
        EnemySpineView previewActor = EnemySpine(previewEnemy);
        Debug.Assert(battleActorLayer.GetSiblingIndex() < battleEnemyHitLayer.GetSiblingIndex());
        Debug.Assert(battleEnemyHitLayer.GetSiblingIndex() < battleKaitLayer.GetSiblingIndex());
        Debug.Assert(kaitSpine.Root.parent == battleKaitLayer);
        Debug.Assert(previewActor.Root.parent == battleActorLayer);
        if (normalized.StartsWith("arrow"))
        {
            bool hitsKait = normalized.Contains("kait");
            Vector2Int impactCell = hitsKait ? attacker : target;
            Vector2Int direction = normalized.Contains("left") ? Vector2Int.left :
                normalized.Contains("up") ? Vector2Int.up :
                normalized.Contains("down") ? Vector2Int.down : Vector2Int.right;
            yield return new WaitForSecondsRealtime(.12f);
            yield return new WaitForEndOfFrame();
            KaitCombatEffectGraphic impact = PlayArrowImpact(impactCell, impactCell - direction, hitsKait);
            Debug.Assert(impact != null && impact.UsesArrowAtlas);
            Debug.Assert(impact.transform.parent == (hitsKait ? battleEffectLayer : battleEnemyHitLayer));
            impact.SetProgress(.28f);
            if (!string.IsNullOrEmpty(screenshotPath))
            {
                CaptureCanvasToPng(screenshotPath);
                yield return new WaitForSecondsRealtime(.4f);
                Debug.Assert(impact == null, "Arrow impact must expire independently of actor animations");
                Application.Quit();
            }
            yield break;
        }
        if (normalized == "hurt")
        {
            previewActor.PlayAttack();
            kaitSpine.PlayOnce(KaitSpineView.Damage);
            // Warm the actor meshes before sampling this very short effect.
            // A cold startup frame can otherwise consume its entire lifetime.
            yield return new WaitForSecondsRealtime(0.12f);
            yield return new WaitForEndOfFrame();
            KaitCombatEffectGraphic hurt = PlayKaitHurtEffect(target);
            Debug.Assert(hurt != null && hurt.UsesHurtAtlas);
            Debug.Assert(hurt.transform.parent == battleEffectLayer);
            // Explicit QA still: sample frame 2 synchronously, before PNG
            // encoding and first-use rendering can advance the playback clock.
            hurt.SetProgress(0.28f);
            if (!string.IsNullOrEmpty(screenshotPath))
            {
                CaptureCanvasToPng(screenshotPath);
                Application.Quit();
            }
            yield break;
        }
        var result = new KaitTurnResult
        {
            kaitDirection = previewDirection,
            damageDealt = blocked || pushed ? 0 : 1,
            playerAttackBlocked = blocked,
            pushed = pushed,
            chainKillCount = chainCount
        };
        if (normalized == "hit-overlap")
        {
            kaitSpine.SetVisible(false);
            KaitSpineView overlapActor = CreateFloatingKait(battleCells[target.x + target.y * KaitRun.BattleSize].rectTransform, previewDirection);
            overlapActor.Root.position += Vector3.left * 28f;
            overlapActor.PlayLoop(KaitSpineView.Idle);
            Debug.Assert(overlapActor.Root.parent == battleKaitLayer);
        }
        // Repeat only in the explicit command-line preview so visual QA can
        // inspect several animation frames without altering normal play.
        for (int pass = 0; pass < 12; pass++)
        {
            previewActor?.SetOpacity(1f);
            previewActor?.SetVisible(true);
            previewActor?.PlayIdle();
            kaitSpine?.Face(result.kaitDirection);
            if (normalized == "hit-overlap")
            {
                kaitSpine.SetVisible(false);
            }
            else
            {
                kaitSpine?.PlayOnce(killed ? KaitSpineView.ChainAttack : KaitSpineView.Attack);
                StartKaitSwordTipTrail(kaitSpine, killed);
            }
            yield return new WaitForSecondsRealtime(KaitAttackImpactLead);
            PlayKaitImpactEffect(result, killed, target, chainCount, attacker);
            if (killed) previewActor?.PlayDeath();
            else if (!blocked && !pushed) previewActor?.PlayDamage();
            if (!string.IsNullOrEmpty(screenshotPath))
            {
                float captureDelay = pushed ? 0.10f :
                    normalized == "block" ? 0.09f : normalized == "hit" ? 0.10f : 0.13f;
                yield return new WaitForSecondsRealtime(captureDelay);
                yield return new WaitForEndOfFrame();
                CaptureCanvasToPng(screenshotPath);
                yield return new WaitForSecondsRealtime(0.8f);
                Application.Quit();
                yield break;
            }
            yield return new WaitForSecondsRealtime(1.35f);
        }
    }

    private KaitCombatEffectGraphic PlayWallStopEffect(KaitDirection direction)
    {
        KaitCombatEffectGraphic dust = PlayCombatEffectAtCell(KaitCombatEffectKind.BoundaryDust,
            run.katePos, Vector2.one * 136f, .38f, effectName: "Boundary Dust A", layerOverride: battleUnderEffectLayer);
        if (dust == null) return null;
        if (kaitSpine != null)
            dust.rectTransform.position = kaitSpine.Root.TransformPoint(new Vector3(0, -kaitSpine.Root.rect.height * .38f, 0));
        else dust.rectTransform.localPosition += Vector3.down * 44f;
        dust.SetBoundaryDirection(KaitRun.Delta(direction));
        return dust;
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
        Transform parent = battleActorLayer != null ? battleActorLayer : canvas.transform;
        EnemySpineView created = EnemySpineView.Create(data, EnemyAnimationPrefix(enemy.type), parent, new Vector2(115, 115), $"Enemy {enemy.id} Spine", visualScale);
        if (created != null)
        {
            // Creation is intentionally invisible until a valid board cell has
            // positioned the host. Callers that only request an animation can
            // no longer expose a one-frame (or interrupted) Canvas-centre actor.
            created.SetVisible(false);
            if (InsideBattle(enemy.pos) && battleCells != null)
            {
                int index = enemy.pos.x + enemy.pos.y * KaitRun.BattleSize;
                if (index >= 0 && index < battleCells.Length && battleCells[index] != null)
                    created.Root.position = battleCells[index].rectTransform.position;
            }
            enemySpines[enemy.id] = created;
        }
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
        foreach (EnemySpineView view in enemySpines.Values)
        {
            view.SetVisible(false);
            view.Destroy();
        }
        enemySpines.Clear();
        enemyDeathAnimationStartedAt.Clear();
        foreach (EnemySpineView view in detachedEnemyDeaths)
        {
            view.SetVisible(false);
            view.Destroy();
        }
        detachedEnemyDeaths.Clear();
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

    private static float EaseOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return 1f - Mathf.Pow(1f - value, 3f);
    }

    private Text MakeText(string value, Transform parent, Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal, bool addOutline = true)
    {
        bool courtyardUi = parent.GetComponentInParent<HybridStyleGraphic>() != null;
        bool courtyardButton = parent.GetComponentInParent<HybridStyleButton>() != null;
        Image parentImage = parent.GetComponentInParent<Image>();
        bool ivoryPanel = parentImage != null && parentImage.sprite == dungeonPanelSprite;
        bool paintedButton = parentImage != null && parentImage.sprite == dungeonButtonSprite;
        if (courtyardUi || ivoryPanel || paintedButton) addOutline = false;
        if (ivoryPanel) color = Hex("#57402E");
        var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        Text text = go.GetComponent<Text>();
        text.text = value;
        text.font = courtyardUi || ivoryPanel || paintedButton ? threatBoardFont : uiFont;
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
            int safeMaxSize = Mathf.Max(8, fontSize - (addOutline ? 2 : 0));
            text.resizeTextMinSize = Mathf.Min(safeMaxSize, Mathf.Max(8, Mathf.RoundToInt(fontSize * 0.54f)));
            text.resizeTextMaxSize = safeMaxSize;
        }
        text.alignByGeometry = anchor == TextAnchor.MiddleCenter;
        if (courtyardUi && !courtyardButton) go.AddComponent<SunlitSplitText>().Configure(styleSplit);
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

    private HybridStyleGraphic MakeHybridSurface(string name, Transform parent, Vector2 position, Vector2 size,
        Sprite pixelSprite, Color flatColor, float seamWidth, float cornerRadius)
    {
        var surfaceObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(HybridStyleGraphic));
        surfaceObject.transform.SetParent(parent, false);
        RectTransform rect = surfaceObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        HybridStyleGraphic surface = surfaceObject.GetComponent<HybridStyleGraphic>();
        surface.raycastTarget = false;
        surface.Configure(styleSplit, pixelSprite, Color.white, flatColor, Peach, seamWidth, cornerRadius);
        return surface;
    }

    private Button MakeHybridButton(Transform parent, Vector2 position, Vector2 size, string label)
    {
        HybridStyleGraphic surface = MakeHybridSurface("Hybrid Button", parent, position, size,
            dungeonButtonSprite, PanelLight, 5f, Mathf.Min(10f, size.y * 0.18f));
        surface.raycastTarget = true;
        HybridStyleButton button = surface.gameObject.AddComponent<HybridStyleButton>();
        button.Configure(surface, dungeonButtonSprite, dungeonButtonPressedSprite, PanelLight);
        Text text = MakeText(label, surface.transform, Vector2.zero, size - new Vector2(8, 8), 17, Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold);
        Stretch(text.rectTransform, 10);
        return button;
    }

    private Button MakeFlatButton(Transform parent, Vector2 position, Vector2 size, string label)
    {
        Image image = Rect("Flat Button", parent, position, size, PanelLight);
        if (roundedSprite != null)
        {
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
        }
        Button button = image.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.58f, 0.58f, 0.58f, 0.72f);
        button.colors = colors;
        Text text = MakeText(label, image.transform, Vector2.zero, size - new Vector2(8, 8), 17, Cream,
            TextAnchor.MiddleCenter, FontStyle.Bold, false);
        text.font = threatBoardFont;
        Stretch(text.rectTransform, 5);
        return button;
    }

    private Toggle MakeFlatToggle(Transform parent, Vector2 position, Vector2 size, string label)
    {
        Image background = Rect("Flat Toggle", parent, position, size, PanelLight);
        if (roundedSprite != null)
        {
            background.sprite = roundedSprite;
            background.type = Image.Type.Sliced;
        }

        Toggle toggle = background.gameObject.AddComponent<Toggle>();
        toggle.transition = Selectable.Transition.ColorTint;
        toggle.targetGraphic = background;
        ColorBlock colors = toggle.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.selectedColor = colors.highlightedColor;
        toggle.colors = colors;

        Image box = Rect("Toggle Box", background.transform, new Vector2(-size.x * 0.5f + 28f, 0), new Vector2(36, 36), Cream);
        if (roundedSprite != null)
        {
            box.sprite = roundedSprite;
            box.type = Image.Type.Sliced;
        }
        box.raycastTarget = false;
        Image mark = Rect("Toggle Mark", box.transform, Vector2.zero, new Vector2(24, 24), Coral);
        if (roundedSprite != null)
        {
            mark.sprite = roundedSprite;
            mark.type = Image.Type.Sliced;
        }
        mark.raycastTarget = false;
        toggle.graphic = mark;

        Text text = MakeText(label, background.transform, new Vector2(24, 0), new Vector2(size.x - 88f, 46), 18, Cream,
            TextAnchor.MiddleLeft, FontStyle.Normal, false);
        text.font = threatBoardFont;
        return toggle;
    }

    private DiagonalCutGraphic AddDiagonalCut(Transform parent, string name, float topSplit, float bottomSplit,
        Color rightColor, Color seamColor, float seamWidth)
    {
        var cutObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(DiagonalCutGraphic));
        cutObject.transform.SetParent(parent, false);
        RectTransform rect = cutObject.GetComponent<RectTransform>();
        Stretch(rect, 0f);
        DiagonalCutGraphic cut = cutObject.GetComponent<DiagonalCutGraphic>();
        cut.raycastTarget = false;
        cut.SetStyle(topSplit, bottomSplit, rightColor, seamColor, seamWidth);
        return cut;
    }

    private void SkinPanel(Image image)
    {
        if (image == null || dungeonPanelSprite == null) return;
        image.sprite = dungeonPanelSprite;
        image.type = Image.Type.Sliced;
        image.color = Color.white;
    }

    private void SkinFlatPanel(Image image)
    {
        if (image == null) return;
        image.color = Panel;
        if (roundedSprite == null) return;
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
    }

    private HealthBarView MakeHealthBar(Transform parent, Vector2 position, Vector2 size)
    {
        Image root = Rect("Health Bar", parent, position, size, Hex("#B49B59"));
        root.sprite = null;
        root.type = Image.Type.Simple;
        root.raycastTarget = false;
        var view = new HealthBarView { root = root };
        float segmentWidth = size.x / 3f;
        for (int i = 0; i < 3; i++)
        {
            Vector2 segmentPosition = new Vector2((i - 1) * segmentWidth, 0f);
            Image slot = Rect($"Health Slot {i + 1}", root.transform, segmentPosition, new Vector2(segmentWidth - 1f, size.y - 2f), Hex("#354937"));
            slot.sprite = null;
            slot.type = Image.Type.Simple;
            slot.preserveAspect = false;
            slot.raycastTarget = false;

            Image fill = Rect($"Health Segment {i + 1}", root.transform, segmentPosition, new Vector2(segmentWidth - 4f, size.y - 5f), Color.white);
            fill.sprite = null;
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
