using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The approved illustration is a backdrop; labels and hit areas remain live UI.
public sealed class KaitMainMenu : MonoBehaviour
{
    public const string BackgroundPath = "KaitVisuals/MainMenu/CourtyardAB";
    public Button StartButton { get; private set; }
    public Button TutorialButton { get; private set; }
    public Button SettingsButton { get; private set; }
    public Button ContinueButton { get; private set; }
    public Button LibraryButton { get; private set; }
    public Action OpenLibrary;
    public Button[] CharacterButtons { get; private set; }
    public KaitCharacter Selected { get; private set; }
    public Action<string> ContinueCharacter;
    public string ContinueKey { get; private set; }
    Func<string,bool> hasSave = PlayerPrefs.HasKey;
    Text selectedName, selectedTrait;
    readonly KaitHomeDiagonalArt[] portraits = new KaitHomeDiagonalArt[3];
    public RectTransform Layout { get; private set; }
    static readonly Color Peach = new Color32(250, 199, 183, 255);
    static readonly Color Plum = new Color32(41, 35, 47, 255);
    // Both boundaries are parallel. Each row uses their midpoint at its own height.
    public static float RowCenter(float y) => y * (230.4f / 1080f);

    public static KaitMainMenu Create(Transform parent, Font font, Sprite rounded,
        Action start, Action tutorial, Action settings)
    {
        var root = new GameObject("Main Menu", typeof(RectTransform), typeof(Image), typeof(KaitMainMenu));
        root.transform.SetParent(parent, false);
        var rt = (RectTransform)root.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        root.GetComponent<Image>().color = Plum;
        var menu = root.GetComponent<KaitMainMenu>();
        menu.Layout = MakeRect("Menu Artwork and Controls", root.transform, Vector2.zero, new Vector2(1920, 1080));
        menu.CharacterButtons = new Button[3];
        string[] names = { "KAIT", "YUMMN", "REYNARD" };
        string[] roles = { "咒 剑 士", "武 僧", "白 狐 法 师" };
        string[] subtitles = { "助跑蓄势 · 击杀转向", "气与残影 · 三宗派构筑", "移动射击 · 镜狐法术位" };
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            var art = MakeRect(names[i] + " CG", menu.Layout, Vector2.zero, new Vector2(1920,1080)).gameObject.AddComponent<KaitHomeDiagonalArt>();
            art.Configure(i, Resources.Load<Texture2D>("KaitVisuals/CharacterSelection/" + (i == 2 ? "ReynardCG" : names[i] == "KAIT" ? "Kait" : "Yummn")));
            menu.portraits[i] = art;
            var button = art.gameObject.AddComponent<Button>(); button.targetGraphic = art;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => { GameAudio.PlayClick(); menu.Select((KaitCharacter)index); });
            menu.CharacterButtons[i] = button;
        }
        var seams = MakeRect("Diagonal Seams", menu.Layout, Vector2.zero, new Vector2(1920,1080)).gameObject.AddComponent<KaitHomeDiagonalSeams>();
        seams.raycastTarget = false;
        for (int i = 0; i < 3; i++)
        {
            menu.Label(font, roles[i], new Vector2(KaitHomeDiagonalArt.Center(i, -287), -287), new Vector2(350,40), 22, Peach);
            menu.Label(font, names[i], new Vector2(KaitHomeDiagonalArt.Center(i, -344), -344), new Vector2(360,80), 46, new Color32(255,242,232,255));
            menu.Label(font, subtitles[i], new Vector2(KaitHomeDiagonalArt.Center(i, -405), -405), new Vector2(390,42), 22, new Color32(214,201,218,255));
        }
        menu.Label(font,"2048",new Vector2(MenuRowCenter(346),346),new Vector2(390,150),86,Peach);
        menu.Label(font,"三 境 之 间",new Vector2(MenuRowCenter(263),263),new Vector2(390,44),23,Peach);
        menu.selectedName=menu.Label(font,"",new Vector2(MenuRowCenter(176),176),new Vector2(390,45),26,Peach);
        menu.selectedTrait=menu.Label(font,"",new Vector2(MenuRowCenter(125),125),new Vector2(390,40),19,new Color32(194,180,199,255));
        menu.ContinueButton=menu.MakeButton(font,rounded,"继续游戏",new Vector2(MenuRowCenter(35),35),new Vector2(330,70),false,()=>{menu.RefreshSaves();if(menu.ContinueKey!=null)menu.ContinueCharacter?.Invoke(menu.ContinueKey);});
        menu.StartButton=menu.MakeButton(font,rounded,"开始游戏",new Vector2(MenuRowCenter(-57),-57),new Vector2(330,84),true,start);
        menu.TutorialButton=menu.MakeButton(font,rounded,"玩法教程",new Vector2(MenuRowCenter(-157),-157),new Vector2(330,70),false,tutorial);
        menu.SettingsButton=menu.MakeButton(font,rounded,"设置",new Vector2(MenuRowCenter(-250),-250),new Vector2(330,70),false,settings);
        menu.LibraryButton=menu.MakeButton(font,rounded,"卡牌大全",new Vector2(MenuRowCenter(-343),-343),new Vector2(330,70),false,()=>menu.OpenLibrary?.Invoke());
        menu.Select((KaitCharacter)Mathf.Clamp(PlayerPrefs.GetInt("Kait.Character",0),0,2));
        menu.Fit();
        return menu;
    }

    public static float MenuRowCenter(float y) => KaitHomeDiagonalArt.Center(3, y);

    public static float FitScale(Vector2 available) => Mathf.Min(available.x / 1920f, available.y / 1080f);

    public void Select(KaitCharacter character)
    {
        Selected=character;
        for (int i=0;i<portraits.Length;i++) portraits[i].Selected = i==(int)character;
        selectedName.text=character==KaitCharacter.Reynard?"REYNARD · 方向编咒":character==KaitCharacter.Kait?"KAIT · 蓄势连斩":"YUMMN · 气息轮转";
        selectedTrait.text=character==KaitCharacter.Reynard?"镜狐法术位 · 持续法术":character==KaitCharacter.Kait?"助跑蓄势 · 击杀转向":"气与残影 · 三宗派构筑";
        RefreshSaves();
    }
    public void SetSaveLookup(Func<string,bool> lookup){hasSave=lookup;RefreshSaves();}
    public void RefreshSaves()
    {
        ContinueKey=null;
        foreach(string key in KaitCharacterSelection.SaveKeys(Selected))if(hasSave(key)){ContinueKey=key;break;}
        if(ContinueButton!=null)ContinueButton.gameObject.SetActive(ContinueKey!=null);
    }
    Text Label(Font font,string value,Vector2 position,Vector2 size,int fontSize,Color color)
    {
        var t=MakeRect(value,Layout,position,size).gameObject.AddComponent<Text>();
        t.text=value;t.font=font;t.fontSize=fontSize;t.alignment=TextAnchor.MiddleCenter;t.color=color;t.raycastTarget=false;
        t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
    }

    void LateUpdate() => Fit();

    public void Fit()
    {
        if (Layout == null) return;
        // Fit inside safe area instead of cropping character/title on tall phones or tablets.
        var canvas = GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.scaleFactor : 1f;
        Rect safe = Screen.safeArea;
        Vector2 available = ((RectTransform)transform).rect.size;
        if (Screen.width > 0 && Screen.height > 0 && safe.width > 0)
        {
            available = new Vector2(safe.width, safe.height) / scale;
            Layout.anchoredPosition = (safe.center - new Vector2(Screen.width, Screen.height) * .5f) / scale;
        }
        Layout.localScale = Vector3.one * FitScale(available);
    }

    Button MakeButton(Font font, Sprite rounded, string label, Vector2 position, Vector2 size, bool primary, Action action)
    {
        var frame = MakeRect(label, Layout, position, size);
        var border = frame.gameObject.AddComponent<Image>();
        border.sprite = rounded; border.type = Image.Type.Sliced;
border.color = KaitStorybookTheme.Ink;
        var inset = MakeRect("Button Face", frame, Vector2.zero, size - new Vector2(6, 6));
        var face = inset.gameObject.AddComponent<Image>();
        face.sprite = rounded; face.type = Image.Type.Sliced;
face.color = primary ? KaitStorybookTheme.Mint : KaitStorybookTheme.Paper;
        face.raycastTarget = false;
        var button = frame.gameObject.AddComponent<Button>();
        button.targetGraphic = face;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(.76f, .76f, .76f);
        colors.fadeDuration = .07f;
        button.colors = colors;
        var textRect = MakeRect("Label", frame, Vector2.zero, size - new Vector2(44, 20));
        var text = textRect.gameObject.AddComponent<Text>();
        text.font = font; text.fontStyle = FontStyle.Bold; text.fontSize = primary ? 32 : 27;
        text.alignment = TextAnchor.MiddleCenter; text.text = label;
text.color = KaitStorybookTheme.Ink;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        frame.gameObject.AddComponent<KaitMenuButtonFeedback>();
        button.onClick.AddListener(() => { GameAudio.PlayClick(); action?.Invoke(); });
        return button;
    }

    static RectTransform MakeRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform; rt.anchoredPosition = position; rt.sizeDelta = size;
        return rt;
    }
}

public sealed class KaitMenuButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public void OnPointerDown(PointerEventData e) { if(e.button == PointerEventData.InputButton.Left) transform.localScale = Vector3.one * .98f; }
    public void OnPointerUp(PointerEventData e) => ResetScale();
    public void OnPointerExit(PointerEventData e) => ResetScale();
    void OnDisable() => ResetScale();
    void ResetScale() => transform.localScale = Vector3.one;
}

// Separate lifetime so starting a run cannot cancel menu verification midway.
public sealed class KaitMenuRuntimeQA : MonoBehaviour { }
