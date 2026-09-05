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
    public RectTransform Layout { get; private set; }
    static readonly Color Peach = new Color32(250, 199, 183, 255);
    static readonly Color Plum = new Color32(47, 41, 50, 255);

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
        var art = menu.Layout.gameObject.AddComponent<RawImage>();
        art.texture = Resources.Load<Texture2D>(BackgroundPath);
        art.raycastTarget = false;
        menu.StartButton = menu.MakeButton(font, rounded, "开始游戏", new Vector2(516, -40), new Vector2(660, 168), true, start);
        menu.TutorialButton = menu.MakeButton(font, rounded, "玩法教程", new Vector2(516, -213), new Vector2(614, 110), false, tutorial);
        menu.SettingsButton = menu.MakeButton(font, rounded, "设置", new Vector2(516, -353), new Vector2(614, 110), false, settings);
        menu.Fit();
        return menu;
    }

    public static float FitScale(Vector2 available) => Mathf.Min(available.x / 1920f, available.y / 1080f);

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
        border.color = primary ? new Color32(191, 143, 127, 255) : Peach;
        var inset = MakeRect("Button Face", frame, Vector2.zero, size - new Vector2(6, 6));
        var face = inset.gameObject.AddComponent<Image>();
        face.sprite = rounded; face.type = Image.Type.Sliced;
        face.color = primary ? Peach : Plum;
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
        text.font = font; text.fontStyle = FontStyle.Bold; text.fontSize = primary ? 58 : 40;
        text.alignment = TextAnchor.MiddleCenter; text.text = label;
        text.color = primary ? Plum : new Color32(255, 242, 221, 255);
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
