using UnityEngine;

/// <summary>Separate overlay roots keep actor-local reordering from changing hit priority.</summary>
public static class KaitCombatLayers
{
    public static RectTransform AddAbove(RectTransform previous, string name)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(previous.parent, false);
        rect.anchorMin = previous.anchorMin;
        rect.anchorMax = previous.anchorMax;
        rect.pivot = previous.pivot;
        rect.sizeDelta = previous.sizeDelta;
        rect.anchoredPosition = previous.anchoredPosition;
        rect.SetSiblingIndex(previous.GetSiblingIndex() + 1);
        return rect;
    }

    public static bool IsEnemyImpact(KaitCombatEffectKind kind)
    {
        return kind == KaitCombatEffectKind.NormalHit || kind == KaitCombatEffectKind.Kill ||
            kind == KaitCombatEffectKind.ChainKill || kind == KaitCombatEffectKind.Block ||
            kind == KaitCombatEffectKind.Push || kind == KaitCombatEffectKind.EnemyHit;
    }
}
