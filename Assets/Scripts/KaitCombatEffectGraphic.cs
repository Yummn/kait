using UnityEngine;
using UnityEngine.UI;

public enum KaitCombatEffectKind
{
    NormalHit,
    SwordArc,
    Block,
    Kill,
    ChainKill,
    EnemyHit,
    MagicCast,
    MagicImpact,
    Ice,
    Phantom,
    Speed,
    DreadSlash
}

/// <summary>
/// Small, texture-free combat marks designed for the battle board. Keeping the
/// marks inside one local rect prevents a hit flash from turning into a
/// full-screen overlay, while hard-edged geometry matches the pixel half of the UI.
/// </summary>
public sealed class KaitCombatEffectGraphic : MaskableGraphic
{
    [SerializeField] private KaitCombatEffectKind kind;
    [SerializeField] private Color secondaryColor = Color.white;
    [SerializeField] [Range(0f, 1f)] private float progress;
    [SerializeField] [Range(0f, 1f)] private float intensity;
    private bool autoPlaying;
    private float playbackDuration;
    private float playbackElapsed;

    public void Configure(KaitCombatEffectKind effectKind, Color primary, Color secondary, float strength)
    {
        kind = effectKind;
        color = primary;
        secondaryColor = secondary;
        intensity = Mathf.Clamp01(strength);
        raycastTarget = false;
        SetVerticesDirty();
    }

    public void SetProgress(float value)
    {
        progress = Mathf.Clamp01(value);
        SetVerticesDirty();
    }

    public void Play(float duration)
    {
        playbackDuration = Mathf.Max(0.08f, duration);
        playbackElapsed = 0f;
        autoPlaying = true;
        SetProgress(0.02f);
        rectTransform.localScale = Vector3.one * 0.68f;
    }

    private void Update()
    {
        if (!autoPlaying) return;
        playbackElapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(playbackElapsed / playbackDuration);
        SetProgress(t);
        float scale = t < 0.28f
            ? Mathf.Lerp(0.68f, 1.08f, Mathf.SmoothStep(0f, 1f, t / 0.28f))
            : Mathf.Lerp(1.08f, 1f, Mathf.SmoothStep(0f, 1f, (t - 0.28f) / 0.72f));
        rectTransform.localScale = Vector3.one * scale;
        if (t < 1f) return;
        autoPlaying = false;
        Destroy(gameObject);
    }

    protected override void OnPopulateMesh(VertexHelper helper)
    {
        helper.Clear();
        Rect rect = rectTransform.rect;
        float unit = Mathf.Min(rect.width, rect.height);
        if (unit <= 0f) return;

        float appear = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress * 5f));
        // Hold the contact mark for long enough to remain readable during fast
        // chain input. Movement may continue immediately, but feedback persists.
        float fade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.72f, 1f, progress));
        float spread = Mathf.Lerp(0.62f, 1f, Mathf.SmoothStep(0f, 1f, progress));
        Color primary = WithAlpha(color, color.a * appear * fade);
        Color secondary = WithAlpha(secondaryColor, secondaryColor.a * appear * fade);

        switch (kind)
        {
            case KaitCombatEffectKind.SwordArc:
                AddArc(helper, unit * 0.39f * spread, unit * 0.05f, secondary,
                    -64f, 64f, 8, 0.74f);
                AddArc(helper, unit * 0.3f * spread, unit * 0.025f, primary,
                    -54f, 54f, 7, 0.68f);
                AddRay(helper, new Vector2(-unit * 0.16f, 0f), new Vector2(unit * 0.32f, 0f),
                    unit * 0.025f, primary, 0f);
                break;

            case KaitCombatEffectKind.NormalHit:
                AddSlash(helper, unit * 0.42f * spread, unit * 0.055f, primary, 34f);
                AddSlash(helper, unit * 0.23f * spread, unit * 0.035f, secondary, -48f);
                AddDiamond(helper, Vector2.zero, unit * 0.075f, secondary);
                break;

            case KaitCombatEffectKind.Block:
                AddShield(helper, unit, primary, secondary, spread);
                break;

            case KaitCombatEffectKind.Kill:
                AddShatterBurst(helper, unit * 0.075f, unit * 0.43f * spread,
                    unit * 0.105f, 8, primary, secondary, -8f, 0.78f);
                AddFracture(helper, unit * 0.32f * spread, unit * 0.025f,
                    secondary, -18f);
                break;

            case KaitCombatEffectKind.ChainKill:
                AddShatterBurst(helper, unit * 0.065f,
                    unit * Mathf.Lerp(0.4f, 0.49f, intensity) * spread,
                    unit * 0.1f, Mathf.RoundToInt(Mathf.Lerp(9f, 13f, intensity)),
                    primary, secondary, progress * 24f - 14f, 0.92f);
                AddFracture(helper, unit * 0.35f * spread, unit * 0.024f,
                    secondary, 16f + progress * 12f);
                break;

            case KaitCombatEffectKind.EnemyHit:
                AddSlash(helper, unit * 0.38f * spread, unit * 0.07f, primary, 28f);
                AddRays(helper, unit * 0.08f, unit * 0.31f * spread, unit * 0.035f, 4, secondary, 0f);
                break;

            case KaitCombatEffectKind.MagicCast:
                AddBrokenRing(helper, unit * Mathf.Lerp(0.22f, 0.34f, progress), unit * 0.035f,
                    primary, 8, 0.72f);
                AddRays(helper, unit * 0.11f, unit * 0.3f, unit * 0.025f, 4, secondary, progress * 60f);
                break;

            case KaitCombatEffectKind.MagicImpact:
                AddBrokenRing(helper, unit * 0.32f * spread, unit * 0.045f, primary, 8, 0.7f);
                AddSlash(helper, unit * 0.42f * spread, unit * 0.055f, secondary, 0f);
                AddSlash(helper, unit * 0.42f * spread, unit * 0.055f, secondary, 90f);
                break;

            case KaitCombatEffectKind.Ice:
                AddCrystal(helper, new Vector2(-unit * 0.2f, -unit * 0.08f), unit * 0.33f * spread, primary);
                AddCrystal(helper, new Vector2(0f, unit * 0.02f), unit * 0.46f * spread, secondary);
                AddCrystal(helper, new Vector2(unit * 0.2f, -unit * 0.1f), unit * 0.3f * spread, primary);
                break;

            case KaitCombatEffectKind.Phantom:
                AddChevron(helper, new Vector2(-unit * 0.24f * spread, 0f), unit * 0.2f, primary);
                AddChevron(helper, Vector2.zero, unit * 0.25f, secondary);
                AddChevron(helper, new Vector2(unit * 0.24f * spread, 0f), unit * 0.2f, primary);
                break;

            case KaitCombatEffectKind.Speed:
                AddChevron(helper, new Vector2(-unit * 0.26f * spread, -unit * 0.1f), unit * 0.2f, primary);
                AddChevron(helper, new Vector2(-unit * 0.05f * spread, 0f), unit * 0.24f, secondary);
                AddChevron(helper, new Vector2(unit * 0.2f * spread, unit * 0.1f), unit * 0.18f, primary);
                break;

            case KaitCombatEffectKind.DreadSlash:
                AddArc(helper, unit * 0.38f * spread, unit * 0.08f, primary,
                    -72f, 72f, 8, 0.82f);
                AddSlash(helper, unit * 0.72f * spread, unit * 0.065f, secondary, 0f);
                AddSlash(helper, unit * 0.56f * spread, unit * 0.025f, primary, 0f,
                    new Vector2(0f, unit * 0.12f));
                break;
        }
    }

    private static void AddShield(VertexHelper helper, float unit, Color edge, Color spark, float spread)
    {
        float radius = unit * 0.34f * spread;
        const int segments = 7;
        for (int i = 0; i < segments; i++)
        {
            float a0 = Mathf.Lerp(-68f, 68f, i / (float)segments) * Mathf.Deg2Rad;
            float a1 = Mathf.Lerp(-68f, 68f, (i + 0.68f) / segments) * Mathf.Deg2Rad;
            Vector2 p0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
            Vector2 p1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            AddQuad(helper, p0, p1, unit * 0.045f, edge);
        }
        AddDiamond(helper, new Vector2(radius * 0.78f, 0f), unit * 0.085f, spark);
        AddRay(helper, new Vector2(radius * 0.82f, 0f), new Vector2(radius * 1.25f, 0f), unit * 0.035f, spark);
    }

    private static void AddBrokenRing(VertexHelper helper, float radius, float width, Color tint,
        int segments, float fill)
    {
        for (int i = 0; i < segments; i++)
        {
            float start = i * Mathf.PI * 2f / segments;
            float end = start + Mathf.PI * 2f / segments * fill;
            Vector2 a = new Vector2(Mathf.Cos(start), Mathf.Sin(start)) * radius;
            Vector2 b = new Vector2(Mathf.Cos(end), Mathf.Sin(end)) * radius;
            AddQuad(helper, a, b, width, tint);
        }
    }

    private static void AddArc(VertexHelper helper, float radius, float width, Color tint,
        float startDegrees, float endDegrees, int segments, float fill)
    {
        for (int i = 0; i < segments; i++)
        {
            float t0 = i / (float)segments;
            float t1 = (i + fill) / segments;
            float a0 = Mathf.Lerp(startDegrees, endDegrees, t0) * Mathf.Deg2Rad;
            float a1 = Mathf.Lerp(startDegrees, endDegrees, t1) * Mathf.Deg2Rad;
            Vector2 p0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
            Vector2 p1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            AddQuad(helper, p0, p1, width, tint);
        }
    }

    private static void AddRays(VertexHelper helper, float inner, float outer, float width,
        int count, Color tint, float rotation)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (rotation + i * 360f / count) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            AddRay(helper, direction * inner, direction * outer, width, tint);
        }
    }

    private static void AddShatterBurst(VertexHelper helper, float inner, float outer, float width,
        int count, Color primary, Color secondary, float rotation, float lengthVariation)
    {
        // Deliberately uneven, pointed fragments. The previous concentric rings
        // read as a soft circular pulse; these wedges leave open gaps and make
        // the contact look like a plate breaking from the impact point.
        for (int i = 0; i < count; i++)
        {
            float jitter = ((i * 37) % 11 - 5) * 2.1f;
            float angle = (rotation + i * 360f / count + jitter) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 normal = new Vector2(-direction.y, direction.x);
            float lengthSeed = ((i * 29) % 7) / 6f;
            float fragmentOuter = Mathf.Lerp(outer * (1f - lengthVariation * 0.34f), outer, lengthSeed);
            float baseHalf = width * Mathf.Lerp(0.26f, 0.52f, ((i * 17) % 5) / 4f);
            Vector2 baseCenter = direction * (inner + (i % 3) * width * 0.08f);
            Vector2 shoulder = direction * Mathf.Lerp(inner, fragmentOuter, 0.62f) +
                normal * (i % 2 == 0 ? baseHalf * 0.28f : -baseHalf * 0.28f);
            Vector2 tip = direction * fragmentOuter;
            Color tint = i % 3 == 0 ? secondary : primary;
            AddPolygon(helper, tint,
                baseCenter - normal * baseHalf,
                shoulder - normal * baseHalf * 0.34f,
                tip,
                shoulder + normal * baseHalf * 0.18f,
                baseCenter + normal * baseHalf * 0.72f);

            // A few detached chips keep the silhouette broken instead of
            // closing back into another implied circle.
            if (i % 2 != 0) continue;
            Vector2 chipCenter = direction * fragmentOuter * 0.78f + normal * baseHalf * 0.9f;
            float chipSize = width * Mathf.Lerp(0.13f, 0.22f, lengthSeed);
            AddPolygon(helper, tint,
                chipCenter + direction * chipSize,
                chipCenter + normal * chipSize * 0.7f,
                chipCenter - direction * chipSize * 0.8f,
                chipCenter - normal * chipSize * 0.45f);
        }
    }

    private static void AddFracture(VertexHelper helper, float reach, float width, Color tint,
        float rotation)
    {
        // Three short, kinked cracks through the centre; none forms a complete
        // circle, and their unequal branches reinforce the shattered feel.
        for (int branch = 0; branch < 3; branch++)
        {
            float angle = (rotation + branch * 113f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 normal = new Vector2(-direction.y, direction.x);
            Vector2 start = direction * reach * 0.04f;
            Vector2 kink = direction * reach * 0.48f + normal * reach * (branch == 1 ? -0.1f : 0.07f);
            Vector2 end = direction * reach * Mathf.Lerp(0.76f, 1f, branch / 2f) - normal * reach * 0.04f;
            AddRay(helper, start, kink, width, tint, 0.58f);
            AddRay(helper, kink, end, width * 0.68f, tint, 0f);
        }
    }

    private static void AddSlash(VertexHelper helper, float length, float width, Color tint,
        float angle, Vector2 offset = default)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        AddRay(helper, offset - direction * length * 0.5f, offset + direction * length * 0.5f,
            width, tint, 0.42f);
    }

    private static void AddRay(VertexHelper helper, Vector2 from, Vector2 to, float width, Color tint,
        float endScale = 0f)
    {
        Vector2 direction = to - from;
        if (direction.sqrMagnitude < 0.0001f) return;
        Vector2 normal = new Vector2(-direction.y, direction.x).normalized;
        float half = width * 0.5f;
        AddPolygon(helper, tint,
            from - normal * half,
            from + normal * half,
            to + normal * half * endScale,
            to - normal * half * endScale);
    }

    private static void AddQuad(VertexHelper helper, Vector2 from, Vector2 to, float width, Color tint)
    {
        AddRay(helper, from, to, width, tint, 1f);
    }

    private static void AddDiamond(VertexHelper helper, Vector2 center, float radius, Color tint)
    {
        AddPolygon(helper, tint,
            center + Vector2.up * radius,
            center + Vector2.right * radius,
            center + Vector2.down * radius,
            center + Vector2.left * radius);
    }

    private static void AddCrystal(VertexHelper helper, Vector2 center, float height, Color tint)
    {
        float halfWidth = height * 0.18f;
        AddPolygon(helper, tint,
            center + new Vector2(0f, height * 0.5f),
            center + new Vector2(halfWidth, -height * 0.18f),
            center + new Vector2(0f, -height * 0.5f),
            center + new Vector2(-halfWidth, -height * 0.18f));
    }

    private static void AddChevron(VertexHelper helper, Vector2 center, float size, Color tint)
    {
        Vector2 top = center + new Vector2(-size * 0.42f, size * 0.48f);
        Vector2 middle = center + new Vector2(size * 0.42f, 0f);
        Vector2 bottom = center + new Vector2(-size * 0.42f, -size * 0.48f);
        AddQuad(helper, top, middle, size * 0.14f, tint);
        AddQuad(helper, middle, bottom, size * 0.14f, tint);
    }

    private static void AddPolygon(VertexHelper helper, Color tint, params Vector2[] points)
    {
        if (points == null || points.Length < 3) return;
        int start = helper.currentVertCount;
        foreach (Vector2 point in points)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = point;
            vertex.color = tint;
            helper.AddVert(vertex);
        }
        for (int i = 1; i < points.Length - 1; i++) helper.AddTriangle(start, start + i, start + i + 1);
    }

    private static Color WithAlpha(Color source, float alpha)
    {
        source.a = Mathf.Clamp01(alpha);
        return source;
    }
}
