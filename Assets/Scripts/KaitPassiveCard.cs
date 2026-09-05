using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Dragging, revealing and docking run independently of the turn animations.
public sealed class KaitPassiveCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static readonly Vector2 Size = new Vector2(184, 264);
    public RectTransform Rect { get; private set; }
    public KaitPassive Passive { get; private set; }
    public bool IsCandidate { get; private set; }
    public bool IsDragging { get; private set; }
    public bool Expanded { get; private set; }
    public float DockX { get; private set; }
    public bool SuppressedClick { get; private set; }
    private RectTransform bounds;
    private HybridStyleGraphic surface;
    private Sprite face;
    private KaitCardLogo logo;
    private Text title, description, footer;
    private CanvasGroup visibility;
    private Action<KaitPassiveCard> chosen;
    private Action<KaitPassiveCard, float> dropped;
    private Vector2 target, dragOffset;
    private float revealUntil, triggerUntil;
    private bool hovered, covered;
    private bool pendingDockSound;
    private int pointerId = int.MinValue;
    private int triggerCount;

    public static KaitPassiveCard Create(RectTransform parent, GlobalStyleSplit split, Font font,
        Sprite hd, Sprite flat, Action<KaitPassiveCard> choose, Action<KaitPassiveCard, float> drop)
    {
        var go = new GameObject("Passive Card", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(HybridStyleGraphic), typeof(CanvasGroup), typeof(KaitPassiveCard));
        go.transform.SetParent(parent, false);
        var card = go.GetComponent<KaitPassiveCard>();
        card.Rect = go.GetComponent<RectTransform>();
        card.Rect.sizeDelta = Size;
        card.bounds = parent;
        card.face = hd;
        card.chosen = choose;
        card.dropped = drop;
        card.visibility = go.GetComponent<CanvasGroup>();
        card.surface = go.GetComponent<HybridStyleGraphic>();
        card.surface.Configure(split, hd, Color.white, Color.white, new Color(0.98f, 0.78f, 0.72f), 3f, 8f);
        card.surface.SetRightSprite(KaitSunlitTheme.Load("PassiveCardFlatCompact") ?? flat);
        card.surface.raycastTarget = true;
        card.logo = KaitCardLogo.Create(go.transform, split, font, new Vector2(0,62), 92);
        card.title = card.Label("Name", font, split, new Vector2(0, -19), new Vector2(160, 26), 19, FontStyle.Bold);
        card.description = card.Label("Description", font, split, new Vector2(0, -64), new Vector2(154, 62), 15);
        card.footer = card.Label("Action", font, split, new Vector2(0, -112), new Vector2(156, 23), 13);
        KaitLiftShadow.Attach(card.Rect);
        go.SetActive(false);
        return card;
    }

    private Text Label(string name, Font font, GlobalStyleSplit split, Vector2 position, Vector2 size,
        int fontSize, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.94f, 0.88f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = fontSize;
        text.raycastTarget = false;
        go.AddComponent<SunlitSplitText>().Configure(split);
        return text;
    }

    public void Show(KaitPassive passive, bool candidate, Vector2 initialPosition, float dockX)
    {
        Passive = passive;
        IsCandidate = candidate;
        DockX = dockX;
        IsDragging = false;
        covered = hovered = Expanded = SuppressedClick = false;
        pendingDockSound = false;
        revealUntil = triggerUntil = 0;
        triggerCount = 0;
        pointerId = int.MinValue;
        title.text = KaitPassiveCatalog.Name(passive);
        logo.Show(passive);
        description.text = KaitPassiveCatalog.Description(passive);
        target = initialPosition;
        Rect.anchoredPosition = initialPosition + (candidate ? new Vector2(0, 24) : Vector2.zero);
        Rect.localScale = Vector3.one;
        visibility.alpha = 0;
        visibility.blocksRaycasts = true;
        surface.SetVisualState(face, Color.white, Color.white);
        gameObject.SetActive(true);
        RefreshDetails();
    }

    public void Hide()
    {
        IsDragging = false;
        pendingDockSound = false;
        pointerId = int.MinValue;
        gameObject.SetActive(false);
    }

    public void SetDock(float x) { DockX = x; }
    public void SetCovered(bool value) { covered = value; visibility.blocksRaycasts = !value; if (value) pendingDockSound = false; }

    public void Pulse(int count)
    {
        triggerCount = count;
        triggerUntil = Time.unscaledTime + 1.25f;
        revealUntil = Mathf.Max(revealUntil, triggerUntil);
    }

    private void Update()
    {
        if (bounds == null) return;
        float blend = 1f - Mathf.Exp(-20f * Time.unscaledDeltaTime);
        visibility.alpha = Mathf.Lerp(visibility.alpha, covered ? 0f : 1f, blend);
        if (!IsDragging)
        {
            Expanded = IsCandidate || (!covered && (hovered || Time.unscaledTime < revealUntil));
            if (!IsCandidate)
            {
                float y = covered ? bounds.rect.yMax + Size.y * 0.5f + 8f :
                    Expanded ? bounds.rect.yMax - Size.y * 0.5f : bounds.rect.yMax;
                target = new Vector2(DockX, y);
            }
            Vector2 destination = target;
            if (IsCandidate) destination.y += Mathf.Sin(Time.unscaledTime * 2f + transform.GetSiblingIndex()) * 2f;
            Rect.anchoredPosition = Vector2.Lerp(Rect.anchoredPosition, destination, blend);
            if (pendingDockSound && !IsCandidate && !covered && !Expanded && pointerId == int.MinValue &&
                Vector2.Distance(Rect.anchoredPosition, target) < 2f)
            {
                pendingDockSound = false;
                GameAudio.PlayCardSnap();
            }
        }
        RefreshDetails();
    }

    private void RefreshDetails()
    {
        bool readable = IsCandidate || Expanded || IsDragging;
        description.gameObject.SetActive(readable);
        footer.text = Time.unscaledTime < triggerUntil ? $"触发 · {triggerCount}" :
            IsCandidate ? "点击选择" : readable ? "自动生效 · 拖动收起" : "被动 · 点按展开";
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!IsCandidate && e.pointerId < 0) hovered = true;
    }
    public void OnPointerExit(PointerEventData e)
    {
        hovered = false;
        revealUntil = Mathf.Max(revealUntil, Time.unscaledTime + 0.35f);
    }
    public void OnPointerDown(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || covered || pointerId != int.MinValue) return;
        pointerId = e.pointerId;
        SuppressedClick = false;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, e.position, e.pressEventCamera, out Vector2 local);
        dragOffset = Rect.anchoredPosition - local;
        surface.SetVisualState(face, Color.white * 0.94f, Color.white * 0.94f);
    }
    public void OnPointerUp(PointerEventData e)
    {
        if (e.pointerId != pointerId) return;
        surface.SetVisualState(face, Color.white, Color.white);
        // EventSystem sends Up before EndDrag, so leave the id until EndDrag.
        if (!IsDragging) pointerId = int.MinValue;
    }
    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || SuppressedClick || IsDragging || covered) return;
        if (pointerId != int.MinValue && e.pointerId != pointerId) return;
        if (IsCandidate) chosen?.Invoke(this);
        else revealUntil = Time.unscaledTime + 3f;
    }
    public void OnBeginDrag(PointerEventData e)
    {
        if (e.pointerId != pointerId || covered || IsDragging) return;
        pendingDockSound = false;
        IsDragging = true;
        GameAudio.PlayCardPickUp();
        SuppressedClick = true;
        e.eligibleForClick = false;
        hovered = false;
        transform.SetAsLastSibling();
    }
    public void OnDrag(PointerEventData e)
    {
        if (!IsDragging || e.pointerId != pointerId) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, e.position, e.pressEventCamera, out Vector2 local))
            Rect.anchoredPosition = ClampToScreen(local + dragOffset, bounds.rect, IsCandidate);
    }
    public void OnEndDrag(PointerEventData e)
    {
        if (!IsDragging || e.pointerId != pointerId) return;
        IsDragging = false;
        pointerId = int.MinValue;
        hovered = false;
        revealUntil = 0;
        surface.SetVisualState(face, Color.white, Color.white);
        if (IsCandidate) target = ClampToScreen(Rect.anchoredPosition, bounds.rect, true);
        else
        {
            dropped?.Invoke(this, Rect.anchoredPosition.x);
            pendingDockSound = true;
        }
    }

    public static Vector2 ClampToScreen(Vector2 position, Rect area, bool fullyVisible)
    {
        position.x = Mathf.Clamp(position.x, area.xMin + Size.x * 0.5f + 8f, area.xMax - Size.x * 0.5f - 8f);
        position.y = Mathf.Clamp(position.y, area.yMin + Size.y * 0.5f + 8f,
            area.yMax - (fullyVisible ? Size.y * 0.5f + 12f : 0f));
        return position;
    }

    // Explicit QA hook: the same normal drag handlers are exercised by tests.
    public void PreviewAt(Vector2 position)
    {
        Rect.anchoredPosition = target = position;
        visibility.alpha = 1;
        Expanded = true;
        IsDragging = true;
        RefreshDetails();
    }
}
