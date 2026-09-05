using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class KaitSkillDeck : MonoBehaviour
{
    public readonly KaitSkillCard[] Owned = new KaitSkillCard[3];
    public readonly KaitSkillCard[] Candidates = new KaitSkillCard[2];
    public static readonly Rect CastZone = new Rect(-140, -15, 280, 190);
    private RectTransform bounds, banner, releaseZone;
    private Text heading, releaseText;
    private Button cancel;
    private Action<int> choose;
    private Func<int, bool> cast;
    private int milestone;
    private KaitRun run;
    private KaitSkill targeting;
    private Vector2? selectedOrigin;
    private Vector2 lastSize;
    private readonly List<RaycastResult> previewHits = new List<RaycastResult>();

    public void Initialize(RectTransform area, GlobalStyleSplit split, Font font, Action<int> onChoose,
        Func<int, bool> onCast, Action onCancel)
    {
        bounds = area; choose = onChoose; cast = onCast;
        releaseZone = Panel(area, split, "Skill Release Zone", CastZone.center, CastZone.size);
        releaseText = Label(releaseZone, split, font, "Release Hint", new Vector2(0, 22), new Vector2(252, 94), 19);
        var button = new GameObject("Cancel Skill Target", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        button.transform.SetParent(releaseZone, false);
        cancel = button.GetComponent<Button>(); cancel.targetGraphic = button.GetComponent<Image>();
        button.GetComponent<Image>().color = new Color(.35f, .32f, .39f);
        button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -56);
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 34);
        var cancelLabel = Label(button.GetComponent<RectTransform>(), null, font, "Cancel Label", Vector2.zero, new Vector2(142, 32), 15);
        cancelLabel.text = "取消选目标";
        cancel.onClick.AddListener(() => onCancel?.Invoke());
        Sprite hd = KaitSunlitTheme.Load("SkillCardHD"), flat = KaitSunlitTheme.Load("SkillCardFlat");
        for (int i = 0; i < 3; i++) Owned[i] = KaitSkillCard.Create(area, split, font, hd, flat, null, Dock, Cast);
        for (int i = 0; i < 2; i++) Candidates[i] = KaitSkillCard.Create(area, split, font, hd, flat, Select, null, null);
        banner = Panel(area, split, "Skill Choice Banner", new Vector2(0, -96), new Vector2(296, 34));
        heading = Label(banner, split, font, "Heading", Vector2.zero, new Vector2(282, 32), 17);
        banner.gameObject.SetActive(false); releaseZone.gameObject.SetActive(false);
    }

    private static RectTransform Panel(RectTransform parent, GlobalStyleSplit split, string name, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(HybridStyleGraphic));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = size;
        var graphic = go.GetComponent<HybridStyleGraphic>();
        graphic.Configure(split, null, new Color(.9f, .96f, .94f, .94f), new Color(.26f, .24f, .3f, .94f), new Color(.7f, .84f, .9f), 2, 12);
        graphic.raycastTarget = false;
        return rect;
    }
    private static Text Label(RectTransform parent, GlobalStyleSplit split, Font font, string name, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>(); text.rectTransform.anchoredPosition = position; text.rectTransform.sizeDelta = size;
        text.font = font; text.fontSize = fontSize; text.alignment = TextAnchor.MiddleCenter; text.color = new Color(.94f,.97f,1);
        text.resizeTextForBestFit = true; text.resizeTextMinSize = 13; text.resizeTextMaxSize = fontSize; text.raycastTarget = false;
        if (split != null) go.AddComponent<SunlitSplitText>().Configure(split);
        return text;
    }

    public static bool IsInCastZone(Vector2 position) => CastZone.Contains(position);
    public static bool IsReady(KaitRun run, KaitSkill skill) => !run.ended && run.skills.Contains(skill) &&
        (skill == KaitSkill.ShadowStep ? run.chainActive && run.shadowStepAvailable : run.SkillCooldown(skill) == 0);

    public void ResetDeck()
    {
        milestone = 0; run = null; targeting = KaitSkill.None; selectedOrigin = null;
        foreach (var card in Owned) card.Hide(); foreach (var card in Candidates) card.Hide();
        banner.gameObject.SetActive(false); releaseZone.gameObject.SetActive(false);
    }

    public void Sync(KaitRun current, KaitSkill selected)
    {
        run = current; targeting = selected;
        bool rearrange = false;
        for (int i = 0; i < Owned.Length; i++)
        {
            if (i >= run.skills.Count) { Owned[i].Hide(); continue; }
            var card = Owned[i];
            if (!card.gameObject.activeSelf || card.Skill != run.skills[i])
            {
                float x = (i - 1) * 216;
                card.Show(run.skills[i], false, selectedOrigin ?? new Vector2(x, KaitSkillCard.DockY(bounds.rect, false, false)), x);
                selectedOrigin = null; rearrange = true;
            }
            card.SetAvailability(IsReady(run, card.Skill), run.SkillCooldown(card.Skill), targeting == card.Skill);
            card.SetCovered(run.ended);
        }
        if (rearrange) Dock(null, 0);
        int pending = run.ended ? 0 : run.pendingSkillMilestone;
        if (pending != milestone)
        {
            milestone = pending;
            List<KaitSkill> choices = pending == 0 ? new List<KaitSkill>() : run.SkillChoicesForMilestone(pending);
            for (int i = 0; i < Candidates.Length; i++)
            {
                if (i >= choices.Count) { Candidates[i].Hide(); continue; }
                Candidates[i].Show(choices[i], true, new Vector2(i == 0 ? -300 : 300, bounds.rect.yMin + 284), 0);
            }
            heading.text = $"合成 {pending} · 选择一张技能";
            banner.gameObject.SetActive(pending != 0);
        }
    }

    private void Select(KaitSkillCard card)
    {
        selectedOrigin = card.Rect.anchoredPosition;
        choose?.Invoke(Array.IndexOf(Candidates, card));
    }
    private bool Cast(KaitSkillCard card)
    {
        // Recheck real game state at release, not just its last UI refresh.
        if (run == null || !IsReady(run, card.Skill)) return false;
        return cast != null && cast(Array.IndexOf(Owned, card));
    }
    public void Pulse(KaitSkill skill) { foreach (var card in Owned) if (card.Skill == skill && card.gameObject.activeSelf) card.Pulse(); }
    private void LateUpdate()
    {
        if (bounds == null || run == null) return;
        CheckOutsidePreviewPress();
        if (bounds.rect.size != lastSize) { lastSize = bounds.rect.size; Dock(null, 0); }
        KaitSkillCard dragging = Array.Find(Owned, c => c.IsDragging && c.gameObject.activeSelf);
        bool visible = !run.ended && (dragging != null || targeting != KaitSkill.None || run.dreadSlashArmed);
        releaseZone.gameObject.SetActive(visible);
        if (!visible) return;
        cancel.gameObject.SetActive(targeting != KaitSkill.None && dragging == null);
        releaseText.text = dragging != null ? !dragging.Ready ? "技能尚不可用\n松手返回卡槽" : dragging.InCastZone ? "松手打出技能" : "拖到这里打出技能" :
            targeting != KaitSkill.None ? KaitRun.SkillName(targeting) + "\n请点选一个敌人" : "惊惧斩已准备\n输入方向发动";
    }

    private void CheckOutsidePreviewPress()
    {
        if (EventSystem.current == null) return;
        if (Input.touchCount > 0)
        {
            for (int i=0; i<Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began) HandlePreviewPress(touch.position);
            }
        }
        else if (Input.GetMouseButtonDown(0)) HandlePreviewPress(Input.mousePosition);
    }

    private void HandlePreviewPress(Vector2 screenPoint)
    {
        var data = new PointerEventData(EventSystem.current) { position = screenPoint };
        previewHits.Clear();
        EventSystem.current.RaycastAll(data, previewHits);
        DismissOtherPreviews(previewHits.Count > 0 ? previewHits[0].gameObject.transform : null);
    }

    public void DismissOtherPreviews(Transform hit)
    {
        foreach (var card in Owned)
            if (card != null && card.gameObject.activeSelf && (hit == null || !hit.IsChildOf(card.transform)))
                card.DismissPreview();
    }

    private void Dock(KaitSkillCard moved, float x)
    {
        if (moved != null) moved.SetDock(x);
        var cards = new List<KaitSkillCard>();
        foreach (var card in Owned) if (card.gameObject.activeSelf) cards.Add(card);
        cards.Sort((a,b) => a.DockX.CompareTo(b.DockX));
        var wanted = new float[cards.Count]; for (int i = 0; i < wanted.Length; i++) wanted[i] = cards[i].DockX;
        float[] positions = ResolveDockPositions(wanted, bounds.rect);
        for (int i = 0; i < positions.Length; i++) cards[i].SetDock(positions[i]);
    }
    public static float[] ResolveDockPositions(float[] wanted, Rect area)
    {
        float[] result = (float[])wanted.Clone();
        if (result.Length == 0) return result;
        float min = area.xMin + KaitSkillCard.Size.x / 2 + 12, max = area.xMax - KaitSkillCard.Size.x / 2 - 12;
        float spacing = Mathf.Min(KaitSkillCard.Size.x + 14, (max - min) / Mathf.Max(1, result.Length - 1));
        for (int i = 0; i < result.Length; i++) result[i] = Mathf.Max(Mathf.Clamp(result[i], min, max), i == 0 ? min : result[i-1] + spacing);
        result[result.Length - 1] = Mathf.Min(result[result.Length - 1], max);
        for (int i = result.Length - 2; i >= 0; i--) result[i] = Mathf.Min(result[i], result[i+1] - spacing);
        return result;
    }
}
