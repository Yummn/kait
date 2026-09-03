using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Samples the visible main-weapon bone and renders only the sword tip's recent
/// path. This keeps the white slash attached to Kait's actual animation instead
/// of placing a generic slash over the enemy cell.
/// </summary>
public sealed class KaitSwordTipTrailGraphic : MaskableGraphic
{
    private struct TrailPoint
    {
        public Vector2 position;
        public float time;
    }

    private readonly List<TrailPoint> points = new List<TrailPoint>();
    private KaitSpineView source;
    private float startedAt;
    private float captureDuration;
    private float lingerDuration;
    private float strength;

    public static KaitSwordTipTrailGraphic Create(KaitSpineView source, RectTransform parent,
        float captureSeconds, float trailStrength)
    {
        if (source == null || parent == null || !source.IsReady) return null;
        var host = new GameObject("Kait Sword Tip Trail", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(KaitSwordTipTrailGraphic));
        host.transform.SetParent(parent, false);
        RectTransform rect = host.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.SetAsLastSibling();

        KaitSwordTipTrailGraphic trail = host.GetComponent<KaitSwordTipTrailGraphic>();
        trail.source = source;
        trail.startedAt = Time.unscaledTime;
        trail.captureDuration = Mathf.Max(0.16f, captureSeconds);
        trail.lingerDuration = Mathf.Lerp(0.48f, 0.72f, Mathf.Clamp01(trailStrength));
        trail.strength = Mathf.Clamp01(trailStrength);
        trail.color = Color.white;
        trail.raycastTarget = false;
        return trail;
    }

    private void LateUpdate()
    {
        float now = Time.unscaledTime;
        if (now - startedAt <= captureDuration && source != null &&
            source.TryGetSwordTipWorldPosition(out Vector3 worldTip))
        {
            Vector2 localTip = rectTransform.InverseTransformPoint(worldTip);
            if (points.Count == 0 || Vector2.Distance(points[points.Count - 1].position, localTip) >= 0.6f)
                points.Add(new TrailPoint { position = localTip, time = now });
        }

        float oldestAllowed = now - lingerDuration;
        while (points.Count > 2 && points[1].time < oldestAllowed) points.RemoveAt(0);
        SetVerticesDirty();

        if (now - startedAt > captureDuration + lingerDuration)
            Destroy(gameObject);
    }

    protected override void OnPopulateMesh(VertexHelper helper)
    {
        helper.Clear();
        if (points.Count < 2) return;
        float now = Time.unscaledTime;
        for (int i = 1; i < points.Count; i++)
        {
            Vector2 from = points[i - 1].position;
            Vector2 to = points[i].position;
            if ((to - from).sqrMagnitude < 0.01f) continue;
            float age = now - points[i].time;
            float life = 1f - Mathf.Clamp01(age / lingerDuration);
            float fromAlong = (i - 1) / (float)(points.Count - 1);
            float toAlong = i / (float)(points.Count - 1);
            float maximumWidth = Mathf.Lerp(11f, 16f, strength);
            float fromWidth = Mathf.Lerp(0.45f, maximumWidth, Mathf.SmoothStep(0f, 1f, fromAlong)) * life;
            float toWidth = Mathf.Lerp(0.45f, maximumWidth, Mathf.SmoothStep(0f, 1f, toAlong)) * life;
            // Make the oldest end a pointed tail. Previously every segment
            // began at full width, leaving a conspicuous square block at the
            // start of the slash.
            if (i == 1) fromWidth *= 0.08f;
            AddSegment(helper, from, to, fromWidth * 2.15f, toWidth * 2.15f,
                new Color(1f, 0.91f, 0.76f, 0.24f * life));
            AddSegment(helper, from, to, fromWidth, toWidth,
                new Color(1f, 0.985f, 0.94f, 0.96f * life));
        }
        TrailPoint newest = points[points.Count - 1];
        float newestLife = 1f - Mathf.Clamp01((now - newest.time) / lingerDuration);
        AddDiamond(helper, newest.position, Mathf.Lerp(5f, 8f, strength) * newestLife,
            new Color(1f, 1f, 0.96f, 0.94f * newestLife));
    }

    private static void AddSegment(VertexHelper helper, Vector2 from, Vector2 to,
        float fromWidth, float toWidth, Color tint)
    {
        Vector2 direction = to - from;
        if (direction.sqrMagnitude < 0.001f || (fromWidth <= 0.01f && toWidth <= 0.01f)) return;
        Vector2 normal = new Vector2(-direction.y, direction.x).normalized;
        Vector2 fromNormal = normal * fromWidth * 0.5f;
        Vector2 toNormal = normal * toWidth * 0.5f;
        int start = helper.currentVertCount;
        AddVertex(helper, from - fromNormal, tint);
        AddVertex(helper, from + fromNormal, tint);
        AddVertex(helper, to + toNormal, tint);
        AddVertex(helper, to - toNormal, tint);
        helper.AddTriangle(start, start + 1, start + 2);
        helper.AddTriangle(start, start + 2, start + 3);
    }

    private static void AddVertex(VertexHelper helper, Vector2 position, Color tint)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = tint;
        helper.AddVert(vertex);
    }

    private static void AddDiamond(VertexHelper helper, Vector2 center, float radius, Color tint)
    {
        if (radius <= 0.01f) return;
        int start = helper.currentVertCount;
        AddVertex(helper, center + Vector2.up * radius, tint);
        AddVertex(helper, center + Vector2.right * radius, tint);
        AddVertex(helper, center + Vector2.down * radius, tint);
        AddVertex(helper, center + Vector2.left * radius, tint);
        helper.AddTriangle(start, start + 1, start + 2);
        helper.AddTriangle(start, start + 2, start + 3);
    }
}
