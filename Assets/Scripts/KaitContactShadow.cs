using UnityEngine;
using UnityEngine.UI;

// A small analytical contact mark, not a sprite mask around the character.
public sealed class KaitContactShadow : MaskableGraphic
{
    private Graphic actor;
    private Spine.Unity.SkeletonGraphic skeleton;
    private Vector2 baseSize;
    private float lastAlpha = -1;

    public static KaitContactShadow Create(RectTransform parent, Graphic actor, Vector2 position, Vector2 size)
    {
        var go = new GameObject("Foot Contact Shadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitContactShadow));
        go.transform.SetParent(parent, false);
        go.transform.SetAsFirstSibling();
        var shadow = go.GetComponent<KaitContactShadow>();
        shadow.actor = actor;
        shadow.skeleton = actor as Spine.Unity.SkeletonGraphic;
        shadow.baseSize = size * 1.1f;
        shadow.rectTransform.anchoredPosition = position;
        shadow.rectTransform.sizeDelta = shadow.baseSize;
        shadow.raycastTarget = false;
        shadow.maskable = false;
        shadow.color = new Color(0.16f, 0.20f, 0.18f, 0.29f);
        return shadow;
    }

    private void LateUpdate()
    {
        float alpha = actor != null ? actor.color.a : 1;
        var entry = skeleton != null ? skeleton.AnimationState?.GetCurrent(0) : null;
        float lift = 0;
        if (entry?.Animation != null && entry.Animation.Name.IndexOf("jump", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            float duration = Mathf.Max(.01f, entry.Animation.Duration);
            float phase = entry.Loop ? Mathf.Repeat(entry.TrackTime, duration) / duration : Mathf.Clamp01(entry.TrackTime / duration);
            lift = Mathf.Sin(phase * Mathf.PI);
        }
        rectTransform.sizeDelta = baseSize * (1 - lift * .25f);
        alpha *= 1 - lift * .4f;
        if (Mathf.Approximately(lastAlpha, alpha)) return;
        lastAlpha = alpha;
        color = new Color(0.16f, 0.20f, 0.18f, alpha * 0.29f);
    }

    protected override void OnPopulateMesh(VertexHelper helper)
    {
        helper.Clear();
        const int steps = 32;
        Rect r = rectTransform.rect;
        helper.AddVert(r.center, color, Vector2.zero);
        Color edge = color; edge.a = 0;
        for (int i = 0; i < steps; i++)
        {
            float angle = i * Mathf.PI * 2f / steps;
            Vector2 radius = new Vector2(Mathf.Cos(angle) * r.width * .5f, Mathf.Sin(angle) * r.height * .5f);
            helper.AddVert(r.center + radius * .65f, color, Vector2.zero);
            helper.AddVert(r.center + radius, edge, Vector2.zero);
        }
        for (int i = 0; i < steps; i++)
        {
            int a = 1 + i * 2, b = 1 + ((i + 1) % steps) * 2;
            helper.AddTriangle(0, a, b);
            helper.AddTriangle(a, a + 1, b + 1);
            helper.AddTriangle(a, b + 1, b);
        }
    }
}
