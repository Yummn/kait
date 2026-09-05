using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One input surface, two skins. Casting is only an owned-card drag release.
public sealed class KaitSkillCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static readonly Vector2 Size = new Vector2(184, 264);
    public const float DockReveal = 24f;
    public const float PreviewHoldSeconds = 3f;
    public RectTransform Rect { get; private set; }
    public KaitSkill Skill { get; private set; }
    public bool IsCandidate { get; private set; }
    public bool IsDragging { get; private set; }
    public bool SuppressedClick { get; private set; }
    public bool Ready { get; private set; }
    public float DockX { get; private set; }
    // A card may be grabbed by its exposed title rather than its centre.
    public bool InCastZone => KaitSkillDeck.IsInCastZone(Rect.anchoredPosition) || KaitSkillDeck.IsInCastZone(dragPoint);
    private RectTransform bounds;
    private HybridStyleGraphic surface;
    private Sprite face;
    private Text title, description, state, footer;
    private KaitCardLogo logo;
    private CanvasGroup group;
    private Action<KaitSkillCard> choose;
    private Action<KaitSkillCard, float> dock;
    private Func<KaitSkillCard, bool> cast;
    private Vector2 target, offset, dragPoint;
    private bool hovered, covered, targeting, previewDismissed;
    private bool pendingDockSound;
    private int pointer = int.MinValue, cooldown;
    private float revealUntil, playedUntil, feedbackUntil;
    private string feedback;

    public static KaitSkillCard Create(RectTransform parent, GlobalStyleSplit split, Font font,
        Sprite hd, Sprite flat, Action<KaitSkillCard> choose, Action<KaitSkillCard, float> dock, Func<KaitSkillCard, bool> cast)
    {
        var go = new GameObject("Skill Card", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(HybridStyleGraphic), typeof(CanvasGroup), typeof(KaitSkillCard));
        go.transform.SetParent(parent, false);
        var card = go.GetComponent<KaitSkillCard>();
        card.Rect = go.GetComponent<RectTransform>(); card.Rect.sizeDelta = Size;
        card.bounds = parent; card.face = hd; card.choose = choose; card.dock = dock; card.cast = cast;
        card.group = go.GetComponent<CanvasGroup>();
        card.surface = go.GetComponent<HybridStyleGraphic>();
        card.surface.Configure(split, hd, Color.white, Color.white, new Color(.68f, .82f, .9f), 3, 8);
        card.surface.SetRightSprite(flat); card.surface.raycastTarget = true;
        card.title = card.Label("Name", font, split, 101, 26, 19, FontStyle.Bold);
        card.logo = KaitCardLogo.Create(go.transform, split, font, new Vector2(0,51), 68);
        card.state = card.Label("Availability", font, split, 2, 22, 15, FontStyle.Bold);
        card.description = card.Label("Effect", font, split, -55, 75, 15);
        card.footer = card.Label("Gesture", font, split, -110, 24, 13);
        KaitLiftShadow.Attach(card.Rect);
        go.SetActive(false);
        return card;
    }

    private Text Label(string name, Font font, GlobalStyleSplit split, float y, float height, int size, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(transform, false);
        var text = go.GetComponent<Text>();
        text.rectTransform.anchoredPosition = new Vector2(0, y);
        text.rectTransform.sizeDelta = new Vector2(156, height);
        text.font = font; text.fontSize = size; text.fontStyle = style;
        text.color = new Color(.94f, .97f, 1f); text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true; text.resizeTextMinSize = 12; text.resizeTextMaxSize = size;
        text.raycastTarget = false;
        go.AddComponent<SunlitSplitText>().Configure(split);
        return text;
    }

    public void Show(KaitSkill skill, bool candidate, Vector2 position, float x)
    {
        Skill = skill; IsCandidate = candidate; DockX = x;
        IsDragging = hovered = covered = targeting = SuppressedClick = previewDismissed = false;
        pendingDockSound = false;
        pointer = int.MinValue; revealUntil = playedUntil = feedbackUntil = 0;
        target = position; Rect.anchoredPosition = position;
        Rect.localScale = Vector3.one; group.alpha = 0; group.blocksRaycasts = true;
        title.text = KaitRun.SkillName(skill);
        logo.Show(skill); description.text = Description(skill);
        gameObject.SetActive(true); RefreshText();
    }
    public void Hide() { IsDragging = pendingDockSound = false; pointer = int.MinValue; gameObject.SetActive(false); }
    public void SetDock(float x) => DockX = x;
    public void SetCovered(bool value) { covered = value; group.blocksRaycasts = !value; if (value) { IsDragging = pendingDockSound = false; pointer = int.MinValue; } }
    public void SetAvailability(bool ready, int turns, bool selecting) { Ready = ready; cooldown = turns; targeting = selecting; RefreshText(); }
    public void Feedback(string text) { feedback = text; feedbackUntil = Time.unscaledTime + 1.8f; revealUntil = feedbackUntil; }
    public void Pulse() { playedUntil = Time.unscaledTime + .22f; }

    private void Update()
    {
        float blend = 1 - Mathf.Exp(-20 * Time.unscaledDeltaTime);
        group.alpha = Mathf.Lerp(group.alpha, covered ? 0 : 1, blend);
        if (!IsDragging)
        {
            bool expanded = ShouldPreviewAt(Time.unscaledTime);
            if (!IsCandidate) target = new Vector2(DockX, DockY(bounds.rect, expanded, covered));
            Vector2 destination = target;
            if (IsCandidate) destination.y += Mathf.Sin(Time.unscaledTime * 2 + transform.GetSiblingIndex()) * 2;
            // A cast pulse never delays rule execution or accepts input on its behalf.
            if (pointer == int.MinValue && Time.unscaledTime >= playedUntil) Rect.anchoredPosition = Vector2.Lerp(Rect.anchoredPosition, destination, blend);
            if (pendingDockSound && !IsCandidate && !covered && !expanded && pointer == int.MinValue &&
                Vector2.Distance(Rect.anchoredPosition, target) < 2f)
            {
                pendingDockSound = false;
                GameAudio.PlayCardSnap();
            }
        }
        float scale = !IsDragging && Time.unscaledTime < playedUntil ? .90f : 1;
        Rect.localScale = Vector3.Lerp(Rect.localScale, Vector3.one * scale, blend);
        Color tint = IsCandidate || Ready || targeting ? Color.white : new Color(.72f, .75f, .8f);
        if (IsDragging && InCastZone && Ready) tint = new Color(.82f, 1f, .91f);
        surface.SetVisualState(face, tint, tint);
        logo.SetTint(tint);
        RefreshText();
    }

    public static float DockY(Rect area, bool expanded, bool covered) => covered ? area.yMin - Size.y * .5f - 8 :
        expanded ? area.yMin + Size.y * .5f + 12 : area.yMin + DockReveal;

    private void RefreshText()
    {
        state.text = IsCandidate ? "主动技能" : targeting ? "请选择敌人" : cooldown > 0 ? $"冷却 {cooldown} 回合" :
            Ready ? "主动 · 可用" : Skill == KaitSkill.ShadowStep ? "击杀后可用" : "暂不可用";
        footer.text = Time.unscaledTime < feedbackUntil ? feedback : IsCandidate ? "点击选择 · 不暂停" :
            IsDragging ? (InCastZone ? Ready ? "松手打出" : "不可用 · 松手归位" : "拖到中间打出") : "拖到中间打出";
    }
    public bool ShouldPreviewAt(float now) => IsCandidate || IsDragging || (!covered &&
        (hovered || pointer != int.MinValue || now < revealUntil));

    private void RevealPreview()
    {
        previewDismissed = false;
        revealUntil = Time.unscaledTime + PreviewHoldSeconds;
    }

    public void DismissPreview()
    {
        // An outside click must not cancel a drag or a skill target selection.
        if (IsCandidate || IsDragging || pointer != int.MinValue) return;
        hovered = false; revealUntil = 0; previewDismissed = true;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!IsCandidate && !covered && e.pointerId < 0) { hovered = true; RevealPreview(); }
    }
    public void OnPointerExit(PointerEventData e)
    {
        hovered = false;
        if (!previewDismissed && !IsDragging && !covered) RevealPreview();
    }
    public void OnPointerDown(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || covered || pointer != int.MinValue) return;
        pointer = e.pointerId; SuppressedClick = false;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, e.position, e.pressEventCamera, out Vector2 local);
        offset = Rect.anchoredPosition - local;
        dragPoint = local;
    }
    public void OnPointerUp(PointerEventData e) { if (e.pointerId == pointer && !IsDragging) pointer = int.MinValue; }
    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || covered || SuppressedClick || IsDragging) return;
        if (pointer != int.MinValue && pointer != e.pointerId) return;
        if (IsCandidate) choose?.Invoke(this); else RevealPreview();
    }
    public void OnBeginDrag(PointerEventData e)
    {
        if (pointer != e.pointerId || covered || IsDragging) return;
        pendingDockSound = false;
        IsDragging = SuppressedClick = true; hovered = false; e.eligibleForClick = false;
        GameAudio.PlayCardPickUp();
        transform.SetAsLastSibling();
    }
    public void OnDrag(PointerEventData e)
    {
        if (!IsDragging || pointer != e.pointerId) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, e.position, e.pressEventCamera, out Vector2 local))
        {
            dragPoint = local;
            var p = local + offset;
            p.x = Mathf.Clamp(p.x, bounds.rect.xMin + Size.x * .5f + 8, bounds.rect.xMax - Size.x * .5f - 8);
            p.y = Mathf.Clamp(p.y, bounds.rect.yMin, bounds.rect.yMax - Size.y * .5f - 8);
            Rect.anchoredPosition = p;
        }
    }
    public void OnEndDrag(PointerEventData e)
    {
        if (!IsDragging || pointer != e.pointerId) return;
        IsDragging = false; pointer = int.MinValue; hovered = false; revealUntil = 0;
        if (IsCandidate) { target = KaitPassiveCard.ClampToScreen(Rect.anchoredPosition, bounds.rect, true); return; }
        if (InCastZone)
        {
            if (Ready && cast != null && cast(this))
            {
                GameAudio.PlayCardPlay();
                Pulse();
                return;
            }
            else Feedback(cooldown > 0 ? $"冷却 {cooldown} 回合" : "当前无法施放");
        }
        else dock?.Invoke(this, Rect.anchoredPosition.x);
        pendingDockSound = true;
    }

    public static string Sigil(KaitSkill skill)
    {
        switch (skill)
        {
            case KaitSkill.SwiftBoots: return "+1";
            case KaitSkill.CatAgility: return "×2";
            case KaitSkill.DreadSlash: return "斩";
            case KaitSkill.IceTomb: return "冰";
            case KaitSkill.LesserPhantom: return "幻";
            default: return "跃";
        }
    }
    public static string Description(KaitSkill skill)
    {
        switch (skill)
        {
            case KaitSkill.SwiftBoots: return "当前回合速度 +1\n冷却 2 回合";
            case KaitSkill.CatAgility: return "当前回合速度 ×2\n冷却 5 回合";
            case KaitSkill.DreadSlash: return "打出后输入方向\n原地推开普通敌人\n冷却 4 回合";
            case KaitSkill.IceTomb: return "打出后点选敌人\n冻结其下一次行动\n冷却 3 回合";
            case KaitSkill.LesserPhantom: return "打出后点选敌人\n诱导其他敌人攻击它\n冷却 4 回合";
            default: return "击杀后额外前进 1 格\n不推进全局时间";
        }
    }
    public void PreviewAt(Vector2 position) { Rect.anchoredPosition = target = dragPoint = position; group.alpha = 1; IsDragging = true; }
}
