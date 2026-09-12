using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Dragging, revealing and docking run independently of the turn animations.
public sealed class KaitPassiveCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static readonly Vector2 Size = new Vector2(184, 264);
    public const float DockReveal = 64f;
    public RectTransform Rect { get; private set; }
    public KaitPassive Passive { get; private set; }
    public bool IsCandidate { get; private set; }
    public bool IsDragging { get; private set; }
    public bool Expanded { get; private set; }
    public float DockX { get; private set; }
    public bool SuppressedClick { get; private set; }
    private RectTransform bounds;
    private GlobalStyleSplit styleSplit;
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
    private Func<bool> rewardBegin;
    private Action<Vector2> rewardMove, rewardEnd;
    private Vector2 rewardHome;
    public void ConfigureRewardDrag(Func<bool> begin, Action<Vector2> move, Action<Vector2> end)
    { rewardBegin=begin; rewardMove=move; rewardEnd=end; }

    public static KaitPassiveCard Create(RectTransform parent, GlobalStyleSplit split, Font font,
        Sprite hd, Sprite flat, Action<KaitPassiveCard> choose, Action<KaitPassiveCard, float> drop)
    {
        hd=KaitSunlitTheme.Load("SkillCardHD")??hd;flat=KaitSunlitTheme.Load("SkillCardFlat")??flat;
        var go = new GameObject("Passive Card", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(HybridStyleGraphic), typeof(CanvasGroup), typeof(KaitPassiveCard));
        go.transform.SetParent(parent, false);
        var card = go.GetComponent<KaitPassiveCard>();
        card.Rect = go.GetComponent<RectTransform>();
        card.Rect.sizeDelta = Size;
        card.bounds = parent;
        card.styleSplit=split;
        card.face = hd;
        card.chosen = choose;
        card.dropped = drop;
        card.visibility = go.GetComponent<CanvasGroup>();
        card.surface = go.GetComponent<HybridStyleGraphic>();
        card.surface.Configure(split, hd, Color.white, Color.white, new Color(0.98f, 0.78f, 0.72f), 3f, 8f);
        card.surface.SetRightSprite(flat);
        card.surface.raycastTarget = true;
        card.logo = KaitCardLogo.Create(go.transform, split, font, new Vector2(0,51), 68);
        card.title = card.Label("Name", font, split, new Vector2(0, 88), new Vector2(156, 28), 21, FontStyle.Bold);
        card.description = card.Label("Description", font, split, new Vector2(0, -57), new Vector2(148, 82), 18);
        card.footer = card.Label("Action", font, split, new Vector2(0, -112), new Vector2(156, 23), 15);
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
        var def=KaitAbilityCatalog.Get(passive);face=KaitCardSkin.Face(def)??face;
        KaitCardSkin.Apply(gameObject,def,title.font,styleSplit);
        logo.Show(passive);
        description.text = KaitPassiveCatalog.Description(passive).TrimEnd('。');
        target = rewardHome = initialPosition;
        Rect.anchoredPosition = initialPosition + (candidate ? new Vector2(0, 24) : Vector2.zero);
        Rect.localScale = Vector3.one*(rewardBegin!=null?1.18f:1);
        visibility.alpha = 0;
        visibility.blocksRaycasts = true;
        surface.SetVisualState(face, Color.white, Color.white);
        gameObject.SetActive(true);
        RefreshDetails();
    }

    public void ApplyDefinition(KaitAbilityDef def)
    {
        if(def==null)return;
        title.text=def.nameZh;description.text=def.cardText.TrimEnd('。');
        face=KaitCardSkin.Face(def)??face;KaitCardSkin.Apply(gameObject,def,title.font,styleSplit);
        logo.ShowDefinition(def);surface.SetVisualState(face,Color.white,Color.white);
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
    }

    private void Update()=>Advance(Time.unscaledDeltaTime);
    private void Advance(float deltaTime)
    {
        if (bounds == null) return;
        float blend = 1f - Mathf.Exp(-20f * deltaTime);
        visibility.alpha = Mathf.Lerp(visibility.alpha, covered ? 0f : 1f, blend);
        if (!IsDragging)
        {
            Expanded = IsCandidate || (!covered && (hovered || Time.unscaledTime < revealUntil));
            if (!IsCandidate)
            {
                float y = DockY(bounds.rect,Expanded,covered);
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
        // The top half is tucked off-screen while docked; keep the card's name visible.
        title.rectTransform.anchoredPosition=new Vector2(0,readable?101:-94);
        footer.rectTransform.anchoredPosition=new Vector2(0,readable?-112:-117);
        footer.rectTransform.sizeDelta=new Vector2(156,readable?23:18);
        logo.gameObject.SetActive(readable);
        GetComponent<KaitCardSkin>()?.SetDetailsVisible(readable);
        description.gameObject.SetActive(readable);
        footer.text = Time.unscaledTime < triggerUntil ? $"触发 ×{triggerCount}" : "";
        if(readable&&!string.IsNullOrEmpty(missingRequirement))footer.text=missingRequirement;
    }

    private bool pendingAbility;
    public static float DockY(Rect area,bool expanded,bool covered)=>covered?area.yMax+Size.y*.5f+8:
        expanded?area.yMax-Size.y*.5f:area.yMax+Size.y*.5f-DockReveal;
    public void SetPending(bool value) { pendingAbility=value; RefreshDetails(); }
    private string missingRequirement;
    public void SetRequirement(string missing)
    {
        missingRequirement=missing;var tint=string.IsNullOrEmpty(missing)?Color.white:new Color(.53f,.55f,.59f);
        surface.SetVisualState(face,tint,tint);logo.SetTint(tint);RefreshDetails();
    }
    public void SetCopiedPassive(KaitPassive copy)
    {
        if(Passive==KaitPassive.Simulacrum && !IsCandidate && copy!=KaitPassive.None)
            description.text="已复制："+KaitPassiveCatalog.Name(copy)+"\n原牌替换后，复制仍保留";
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
        if (IsCandidate && rewardBegin!=null && !rewardBegin()) return;
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
        {
            Rect.anchoredPosition = ClampToScreen(local + dragOffset, bounds.rect, IsCandidate);
            if(IsCandidate)rewardMove?.Invoke(local);
        }
    }
    public void OnEndDrag(PointerEventData e)
    {
        if (!IsDragging || e.pointerId != pointerId) return;
        IsDragging = false;
        pointerId = int.MinValue;
        hovered = false;
        revealUntil = 0;
        surface.SetVisualState(face, Color.white, Color.white);
        if (IsCandidate)
        {
            target=rewardBegin!=null?rewardHome:ClampToScreen(Rect.anchoredPosition,bounds.rect,true);
            if(rewardEnd!=null && RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds,e.position,e.pressEventCamera,out Vector2 release))rewardEnd(release);
        }
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
