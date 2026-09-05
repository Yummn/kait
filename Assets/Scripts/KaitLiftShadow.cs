using UnityEngine;

// Kept as a sibling, never drawn over the card face or its text.
public sealed class KaitLiftShadow : MonoBehaviour
{
    private RectTransform target;
    private KaitSoftShadow shadow;
    private KaitPassiveCard passive;
    private KaitSkillCard skill;
    private CanvasGroup visibility;
    private float lift;
    public static void Attach(RectTransform card)
    {
        var follow = card.gameObject.AddComponent<KaitLiftShadow>();
        follow.target = card;
        follow.passive = card.GetComponent<KaitPassiveCard>();
        follow.skill = card.GetComponent<KaitSkillCard>();
        follow.visibility = card.GetComponent<CanvasGroup>();
        follow.shadow = KaitSoftShadow.Create(card.parent,"Card Lift Shadow");
        follow.Sync();
    }
    private void LateUpdate() => Sync();
    private void OnEnable() { if(shadow != null) shadow.gameObject.SetActive(true); }
    private void OnDisable() { if(shadow != null) shadow.gameObject.SetActive(false); }
    private void OnDestroy()
    {
        if(shadow == null) return;
        if(Application.isPlaying) Destroy(shadow.gameObject); else DestroyImmediate(shadow.gameObject);
    }
    private void Sync()
    {
        if (target == null || shadow == null) return;
        bool dragged = passive != null ? passive.IsDragging : skill != null && skill.IsDragging;
        bool expanded = passive != null ? passive.Expanded || passive.IsCandidate : skill != null &&
            (skill.IsCandidate || target.anchoredPosition.y > ((RectTransform)target.parent).rect.yMin + 80);
        lift = Mathf.Lerp(lift, dragged ? 1 : expanded ? .4f : 0, 1-Mathf.Exp(-20*Time.unscaledDeltaTime));
        float softness = Mathf.Lerp(3,11,lift);
        var rect = shadow.rectTransform;
        rect.anchorMin = target.anchorMin; rect.anchorMax = target.anchorMax; rect.pivot = target.pivot;
        rect.sizeDelta = target.sizeDelta + Vector2.one * softness * 2;
        rect.localScale = target.localScale; rect.localRotation = target.localRotation;
        rect.anchoredPosition = target.anchoredPosition + Vector2.Lerp(new Vector2(3,-4),new Vector2(11,-16),lift);
        shadow.Shape(9,softness);
        shadow.color = new Color(.1f,.15f,.13f, Mathf.Lerp(.15f,.23f,lift)*(visibility != null ? visibility.alpha : 1));
        int targetIndex=target.GetSiblingIndex(), ownIndex=rect.GetSiblingIndex();
        int wanted=targetIndex-(ownIndex<targetIndex?1:0);
        if(ownIndex!=wanted) rect.SetSiblingIndex(wanted);
    }
}
