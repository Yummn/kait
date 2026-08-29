using System;
using System.Collections;
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
    private Text[] skillTexts = new Text[3];
    private Image[] skillImages = new Image[3];
    private GameObject endOverlay;
    private Text endText;
    private bool busy;
    private Vector2Int? displayKate;
    private Vector2Int? trailCell;
    private string logPath;

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
        if (!string.IsNullOrEmpty(screenshotPath)) StartCoroutine(CaptureAndQuit(screenshotPath));
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

        MakeText("Kait · Shared Direction Prototype", bg.transform, new Vector2(-320, 406), new Vector2(900, 54), 30, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        MakeText("同一个方向，同时改变滑行战场与威胁盘", bg.transform, new Vector2(480, 407), new Vector2(520, 42), 17, Peach, TextAnchor.MiddleRight);

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
        MakeText("4 × 4  威胁盘", frame.transform, new Vector2(0, 202), new Vector2(374, 42), 22, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        MakeText("共享方向 · 每个有效回合新增 2", frame.transform, new Vector2(0, 172), new Vector2(374, 28), 14, Peach, TextAnchor.MiddleLeft);
        var gridGo = new GameObject("Threat Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(frame.transform, false);
        RectTransform rt = gridGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(358, 358);
        rt.anchoredPosition = new Vector2(0, -25);
        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(82, 82);
        grid.spacing = new Vector2(8, 8);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        threatCells = new Image[16];
        threatLabels = new Text[16];
        for (int visualY = 3; visualY >= 0; visualY--)
        {
            for (int x = 0; x < 4; x++)
            {
                int index = x + visualY * 4;
                threatCells[index] = Rect($"Threat {x},{visualY}", gridGo.transform, Vector2.zero, Vector2.zero, Void);
                threatLabels[index] = MakeText("", threatCells[index].transform, Vector2.zero, Vector2.zero, 29, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(threatLabels[index].rectTransform, 2);
            }
        }
    }

    private void BuildSidebar(Transform parent)
    {
        Image info = Rect("Run Info", parent, new Vector2(477, -119), new Vector2(420, 82), Panel);
        turnText = MakeText("", info.transform, new Vector2(0, 16), new Vector2(380, 34), 18, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        statusText = MakeText("", info.transform, new Vector2(0, -18), new Vector2(380, 30), 15, Peach, TextAnchor.MiddleLeft);

        Image skills = Rect("Skills", parent, new Vector2(477, -249), new Vector2(420, 156), Panel);
        MakeText("战技 · 在方向输入前预选", skills.transform, new Vector2(0, 57), new Vector2(380, 30), 17, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        KaitSkill[] values = { KaitSkill.Curse, KaitSkill.Mirage, KaitSkill.FearSlash };
        for (int i = 0; i < values.Length; i++)
        {
            int captured = i;
            Button button = MakeButton(skills.transform, new Vector2(-128 + i * 128, -17), new Vector2(116, 82), "");
            skillImages[i] = button.GetComponent<Image>();
            skillTexts[i] = button.GetComponentInChildren<Text>();
            skillTexts[i].fontSize = 14;
            button.onClick.AddListener(() => OnSkill(values[captured]));
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
        File.WriteAllText(logPath, "turn,direction,kateStart,kateEnd,slideDistance,kills,enemyCount,highestThreat,endReason\n", Encoding.UTF8);
        endOverlay.SetActive(false);
        statusText.text = "选择方向开始。不能主动刹车。";
        RefreshAll();
    }

    private void HandleDirection(KaitDirection direction)
    {
        if (busy || run.ended) return;
        Vector2Int start = run.katePos;
        KaitTurnResult result = run.TryTurn(direction);
        if (!result.valid)
        {
            statusText.text = result.message;
            StartCoroutine(FlashStatus());
            return;
        }
        AppendLog(direction, start, result);
        StartCoroutine(PlayTurn(result, start));
    }

    private IEnumerator PlayTurn(KaitTurnResult result, Vector2Int start)
    {
        busy = true;
        displayKate = start;
        foreach (Vector2Int step in result.katePath)
        {
            trailCell = displayKate;
            displayKate = step;
            RefreshBattle();
            yield return new WaitForSecondsRealtime(0.045f);
        }
        displayKate = null;
        trailCell = null;
        statusText.text = result.message + (result.merges.Count > 0 ? $" · 威胁合并 ×{result.merges.Count}" : "");
        RefreshAll();
        busy = false;
        if (run.ended) ShowEnd();
    }

    private void OnSkill(KaitSkill skill)
    {
        if (busy || run.ended) return;
        run.SelectOrArmSkill(skill, out string message);
        statusText.text = message;
        RefreshSkills();
    }

    private void RefreshAll()
    {
        RefreshBattle();
        RefreshThreat();
        RefreshSkills();
        turnText.text = $"回合 {run.turn}   击杀 {run.kills}   最高威胁 {run.highestThreat}";
        helpText.text = run.pendingSkillChoices > 0 ? $"成长触发：请从右侧选择 {run.pendingSkillChoices} 个新战技" : "弱敌可穿透，强敌会成为停止点；红色裂隙将在下一有效回合出生敌人";
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
                if (trailCell.HasValue && trailCell.Value == p) image.color = new Color(Peach.r, Peach.g, Peach.b, 0.45f);

                KaitSpawnRequest spawn = run.SpawnAt(p);
                KaitMirage mirage = run.MirageAt(p);
                KaitEnemy enemy = run.EnemyAt(p);
                if (spawn != null)
                {
                    image.color = Wine;
                    label.text = $"裂\nT{spawn.tier}";
                    label.color = Cream;
                }
                if (mirage != null)
                {
                    image.color = Hex("#6C77A8");
                    label.text = $"幻\n{mirage.turnsLeft}";
                    label.color = Cream;
                }
                if (enemy != null)
                {
                    image.color = EnemyColor(enemy.type, enemy.life);
                    string type = enemy.type == KaitEnemyType.Grunt ? "兵" : enemy.type == KaitEnemyType.Guard ? "盾" : enemy.type == KaitEnemyType.Archer ? "弓" : "精";
                    string intent = IntentGlyph(enemy.intent.type, enemy.intent.direction);
                    label.text = enemy.life == KaitEnemyLife.Preparing ? $"{type}\n准备" : $"{type} {enemy.EffectiveThreshold}\n{intent}";
                    label.color = enemy.life == KaitEnemyLife.Preparing ? Peach : Cream;
                }
                if (kate == p)
                {
                    image.color = Coral;
                    label.text = "凯";
                    label.fontSize = 28;
                    label.color = Cream;
                }
                else label.fontSize = 17;
            }
        }
    }

    private void RefreshThreat()
    {
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
            {
                int index = x + y * 4;
                int value = run.threat[x, y];
                threatLabels[index].text = value == 0 ? "" : value.ToString();
                threatCells[index].color = ThreatColor(value);
                threatLabels[index].color = value >= 16 ? Cream : Void;
            }
    }

    private void RefreshSkills()
    {
        KaitSkill[] values = { KaitSkill.Curse, KaitSkill.Mirage, KaitSkill.FearSlash };
        for (int i = 0; i < values.Length; i++)
        {
            KaitSkill skill = values[i];
            bool owned = run.skills.Contains(skill);
            bool armed = run.armedSkill == skill;
            int cd = run.cooldowns[skill];
            skillImages[i].color = armed ? Gold : owned ? PanelLight : new Color(0.18f, 0.16f, 0.19f, 1f);
            string footer = owned ? (cd > 0 ? $"CD {cd}" : armed ? "已预选" : "可用") : run.pendingSkillChoices > 0 ? "点击解锁" : "未解锁";
            skillTexts[i].text = run.SkillName(skill).Replace("次级", "次级\n").Replace("咒剑", "咒剑\n") + "\n" + footer;
            skillTexts[i].color = owned || run.pendingSkillChoices > 0 ? Cream : new Color(0.62f, 0.56f, 0.62f, 1f);
        }
    }

    private void ShowEnd()
    {
        endOverlay.SetActive(true);
        string reason = run.endReason == "Threat Overload" ? "威胁盘锁死" : "凯特被敌人击中";
        endText.text = $"{reason}\n\n生存回合：{run.turn}    击杀：{run.kills}\n最高威胁：{run.highestThreat}";
    }

    private void AppendLog(KaitDirection direction, Vector2Int start, KaitTurnResult result)
    {
        string line = $"{run.turn},{direction},{start.x}:{start.y},{run.katePos.x}:{run.katePos.y},{result.slideDistance},{run.kills},{run.enemies.FindAll(e => e.life != KaitEnemyLife.Dead).Count},{run.highestThreat},{run.endReason}\n";
        File.AppendAllText(logPath, line, Encoding.UTF8);
    }

    private IEnumerator FlashStatus()
    {
        Color old = statusText.color;
        statusText.color = Gold;
        yield return new WaitForSecondsRealtime(0.16f);
        statusText.color = old;
    }

    private IEnumerator CaptureAndQuit(string path)
    {
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
