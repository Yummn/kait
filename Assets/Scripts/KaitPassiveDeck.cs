using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitPassiveDeck : MonoBehaviour
{
    private readonly KaitPassiveCard[] owned = new KaitPassiveCard[3];
    private readonly KaitPassiveCard[] candidates = new KaitPassiveCard[3];
    private readonly int[] shownTriggers = new int[3];
    private RectTransform bounds;
    private Text choiceTitle;
    private RectTransform choiceBanner;
    private Action<int> choose;
    private int milestone;
    private Vector2? selectedOrigin;
    private Vector2 lastSize;
    public bool HasChoices => milestone != 0;
    public KaitPassiveCard[] Owned => owned;
    public KaitPassiveCard[] Candidates => candidates;

    public void Initialize(RectTransform area, GlobalStyleSplit split, Font font, Action<int> onChoose)
    {
        bounds = area;
        choose = onChoose;
        Sprite hd = KaitSunlitTheme.Load("PassiveCardBlankHD") ?? KaitSunlitTheme.Load("PassiveCardHD");
        Sprite flat = KaitSunlitTheme.Load("PassiveCardBlankFlat") ?? KaitSunlitTheme.Load("PassiveCardFlat");
        for (int i = 0; i < 3; i++)
        {
            owned[i] = KaitPassiveCard.Create(area, split, font, hd, flat, null, Dock);
            candidates[i] = KaitPassiveCard.Create(area, split, font, hd, flat, Select, null);
        }
        var banner = new GameObject("Passive Choice Banner", typeof(RectTransform), typeof(CanvasRenderer), typeof(HybridStyleGraphic));
        banner.transform.SetParent(area, false);
        choiceBanner = banner.GetComponent<RectTransform>();
        choiceBanner.sizeDelta = new Vector2(328, 36);
        var background = banner.GetComponent<HybridStyleGraphic>();
        background.Configure(split, null, new Color(0.98f, 0.93f, 0.83f), new Color(0.31f, 0.27f, 0.32f),
            new Color(0.98f, 0.78f, 0.72f), 3, 8);
        background.raycastTarget = false;
        var label = new GameObject("Passive Choice Heading", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        label.transform.SetParent(choiceBanner, false);
        choiceTitle = label.GetComponent<Text>();
        choiceTitle.font = font;
        choiceTitle.fontSize = 19;
        choiceTitle.fontStyle = FontStyle.Bold;
        choiceTitle.alignment = TextAnchor.MiddleCenter;
        choiceTitle.color = new Color(1f, 0.94f, 0.88f);
        choiceTitle.raycastTarget = false;
        choiceTitle.rectTransform.sizeDelta = new Vector2(316, 36);
        label.AddComponent<SunlitSplitText>().Configure(split);
        choiceBanner.gameObject.SetActive(false);
    }

    public void ResetDeck()
    {
        milestone = 0;
        selectedOrigin = null;
        Array.Clear(shownTriggers, 0, shownTriggers.Length);
        foreach (var card in owned) card.Hide();
        foreach (var card in candidates) card.Hide();
        choiceBanner.gameObject.SetActive(false);
    }

    public void Sync(KaitRun run)
    {
        bool rearrange = false;
        for (int i = 0; i < owned.Length; i++)
        {
            if (i >= run.passives.Count) { owned[i].Hide(); continue; }
            var card = owned[i];
            if (!card.gameObject.activeSelf || card.Passive != run.passives[i])
            {
                float x = (i - 1) * 216f;
                card.Show(run.passives[i], false, selectedOrigin ?? new Vector2(x, bounds.rect.yMax), x);
                selectedOrigin = null;
                rearrange = true;
            }
            int triggers = run.PassiveTriggerCount(run.passives[i]);
            if (triggers > shownTriggers[i]) card.Pulse(triggers);
            shownTriggers[i] = triggers;
        }
        if (rearrange) Dock(null, 0);

        int pending = run.ended ? 0 : run.pendingPassiveMilestone;
        if (pending != milestone)
        {
            milestone = pending;
            List<KaitPassive> choices = pending == 0 ? new List<KaitPassive>() : run.PassiveChoicesForMilestone(pending);
            for (int i = 0; i < candidates.Length; i++)
            {
                if (i >= choices.Count) { candidates[i].Hide(); continue; }
                candidates[i].Show(choices[i], true, CandidatePosition(i), 0);
            }
            choiceTitle.text = $"合成 {pending} · 选择一张被动";
            choiceBanner.gameObject.SetActive(pending != 0);
        }
        foreach (var card in owned) card.SetCovered(pending != 0 || run.ended);
        choiceBanner.anchoredPosition = new Vector2(0, bounds.rect.yMax - 26f);
    }

    private Vector2 CandidatePosition(int i) => new Vector2((i - 1) * 218f, bounds.rect.yMax - KaitPassiveCard.Size.y * 0.5f - 56f);

    private void Select(KaitPassiveCard card)
    {
        int index = Array.IndexOf(candidates, card);
        if (index < 0) return;
        selectedOrigin = card.Rect.anchoredPosition;
        choose?.Invoke(index);
    }

    private void LateUpdate()
    {
        if (bounds == null || bounds.rect.size == lastSize) return;
        lastSize = bounds.rect.size;
        Dock(null, 0);
    }

    private void Dock(KaitPassiveCard moved, float x)
    {
        if (moved != null) moved.SetDock(x);
        var visible = new List<KaitPassiveCard>();
        foreach (var card in owned) if (card.gameObject.activeSelf) visible.Add(card);
        visible.Sort((a, b) => a.DockX.CompareTo(b.DockX));
        float[] positions = new float[visible.Count];
        for (int i = 0; i < positions.Length; i++) positions[i] = visible[i].DockX;
        positions = ResolveDockPositions(positions, bounds.rect);
        for (int i = 0; i < positions.Length; i++) visible[i].SetDock(positions[i]);
    }

    // Sorted inputs; reserve the top-left title and top-right menu buttons.
    public static float[] ResolveDockPositions(float[] wanted, Rect area)
    {
        var result = (float[])wanted.Clone();
        if (result.Length == 0) return result;
        float min = area.xMin + Mathf.Min(240f, area.width * 0.18f);
        float max = area.xMax - Mathf.Min(320f, area.width * 0.23f);
        float spacing = Mathf.Min(KaitPassiveCard.Size.x + 14f, (max - min) / Mathf.Max(1, result.Length - 1));
        for (int i = 0; i < result.Length; i++)
            result[i] = Mathf.Max(Mathf.Clamp(result[i], min, max), i == 0 ? min : result[i - 1] + spacing);
        result[result.Length - 1] = Mathf.Min(result[result.Length - 1], max);
        for (int i = result.Length - 2; i >= 0; i--) result[i] = Mathf.Min(result[i], result[i + 1] - spacing);
        return result;
    }
}
