using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

public sealed class HybridStyleGraphic : MaskableGraphic
{
    private struct HybridVertex
    {
        public Vector2 position;
        public Vector2 uv;
        public Vector2 normalizedPosition;

        public static HybridVertex Lerp(HybridVertex from, HybridVertex to, float t)
        {
            return new HybridVertex
            {
                position = Vector2.LerpUnclamped(from.position, to.position, t),
                uv = Vector2.LerpUnclamped(from.uv, to.uv, t),
                normalizedPosition = Vector2.LerpUnclamped(from.normalizedPosition, to.normalizedPosition, t)
            };
        }
    }

    private static Material sharedHybridMaterial;

    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;
    private Material dualTextureMaterial;
    [SerializeField] private Color leftTint = Color.white;
    [SerializeField] private Color rightColor = Color.gray;
    [SerializeField] private Color seamColor = Color.white;
    [SerializeField] private float seamWidth = 4f;
    [SerializeField] private float cornerRadius = 8f;

    private GlobalStyleSplit splitContext;
    private float fallbackBottomSplit = 0.5f;
    private float fallbackTopSplit = 0.5f;
    private float lastBottomSplit = float.NaN;
    private float lastTopSplit = float.NaN;

    public override Texture mainTexture => leftSprite != null && leftSprite.texture != null
        ? leftSprite.texture
        : s_WhiteTexture;

    public void Configure(GlobalStyleSplit context, Sprite sprite, Color left, Color right, Color seam,
        float width, float radius)
    {
        splitContext = context;
        leftSprite = sprite;
        leftTint = left;
        rightColor = right;
        seamColor = seam;
        seamWidth = Mathf.Max(0f, width);
        cornerRadius = Mathf.Max(0f, radius);
        EnsureMaterial();
        SetAllDirty();
    }

    public void SetFallbackSplit(float bottom, float top)
    {
        fallbackBottomSplit = bottom;
        fallbackTopSplit = top;
        SetVerticesDirty();
    }

    public void SetRightSprite(Sprite sprite)
    {
        rightSprite = sprite;
        EnsureMaterial();
        SetAllDirty();
    }

    protected override void OnDestroy()
    {
        if (dualTextureMaterial != null)
        {
            if (Application.isPlaying) Destroy(dualTextureMaterial);
            else DestroyImmediate(dualTextureMaterial);
        }
        base.OnDestroy();
    }

    public void SetVisualState(Sprite sprite, Color left, Color right)
    {
        bool textureChanged = leftSprite == null || sprite == null || leftSprite.texture != sprite.texture;
        leftSprite = sprite;
        leftTint = left;
        rightColor = right;
        SetVerticesDirty();
        if (textureChanged) SetMaterialDirty();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EnsureMaterial();
    }

    private void LateUpdate()
    {
        GetSplits(out float bottom, out float top);
        if (Mathf.Approximately(bottom, lastBottomSplit) && Mathf.Approximately(top, lastTopSplit)) return;
        lastBottomSplit = bottom;
        lastTopSplit = top;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = rectTransform.rect;
        if (rect.width <= 0f || rect.height <= 0f) return;

        GetSplits(out float bottomSplit, out float topSplit);
        float bottomX = Mathf.LerpUnclamped(rect.xMin, rect.xMax, bottomSplit);
        float topX = Mathf.LerpUnclamped(rect.xMin, rect.xMax, topSplit);

        AddSlicedSide(vertexHelper, rect, bottomX, topX, leftSprite, true);
        if (rightSprite != null) AddSlicedSide(vertexHelper, rect, bottomX, topX, rightSprite, false);
        else AddSolidSide(vertexHelper, rect, bottomX, topX, false, rightColor);

        if (seamWidth > 0f && seamColor.a > 0f)
        {
            float half = seamWidth * 0.5f;
            var seam = new List<HybridVertex>
            {
                MakeVertex(new Vector2(bottomX - half, rect.yMin), rect, Vector2.zero),
                MakeVertex(new Vector2(bottomX + half, rect.yMin), rect, Vector2.zero),
                MakeVertex(new Vector2(topX + half, rect.yMax), rect, Vector2.zero),
                MakeVertex(new Vector2(topX - half, rect.yMax), rect, Vector2.zero)
            };
            AddPolygon(vertexHelper, seam, seamColor, 1f, rect);
        }
    }

    private void AddSlicedSide(VertexHelper helper, Rect rect, float bottomX, float topX, Sprite sprite, bool left)
    {
        Vector4 outer = sprite != null ? DataUtility.GetOuterUV(sprite) : new Vector4(0f, 0f, 1f, 1f);
        Vector4 inner = sprite != null ? DataUtility.GetInnerUV(sprite) : outer;
        Vector4 border = AdjustBorders(sprite != null ? sprite.border : Vector4.zero, rect, sprite);

        float[] xs = { rect.xMin, rect.xMin + border.x, rect.xMax - border.z, rect.xMax };
        float[] ys = { rect.yMin, rect.yMin + border.y, rect.yMax - border.w, rect.yMax };
        float[] us = { outer.x, inner.x, inner.z, outer.z };
        float[] vs = { outer.y, inner.y, inner.w, outer.w };

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (xs[x + 1] <= xs[x] || ys[y + 1] <= ys[y]) continue;
                var quad = new List<HybridVertex>
                {
                    MakeVertex(new Vector2(xs[x], ys[y]), rect, new Vector2(us[x], vs[y])),
                    MakeVertex(new Vector2(xs[x + 1], ys[y]), rect, new Vector2(us[x + 1], vs[y])),
                    MakeVertex(new Vector2(xs[x + 1], ys[y + 1]), rect, new Vector2(us[x + 1], vs[y + 1])),
                    MakeVertex(new Vector2(xs[x], ys[y + 1]), rect, new Vector2(us[x], vs[y + 1]))
                };
                AddPolygon(helper, ClipToStyleSide(quad, rect, bottomX, topX, left),
                    left ? leftTint : rightColor, left ? 0f : 2f, rect);
            }
        }
    }

    private void AddSolidSide(VertexHelper helper, Rect rect, float bottomX, float topX, bool left, Color tint)
    {
        var quad = new List<HybridVertex>
        {
            MakeVertex(new Vector2(rect.xMin, rect.yMin), rect, Vector2.zero),
            MakeVertex(new Vector2(rect.xMax, rect.yMin), rect, Vector2.zero),
            MakeVertex(new Vector2(rect.xMax, rect.yMax), rect, Vector2.zero),
            MakeVertex(new Vector2(rect.xMin, rect.yMax), rect, Vector2.zero)
        };
        AddPolygon(helper, ClipToStyleSide(quad, rect, bottomX, topX, left), tint, 1f, rect);
    }

    private static List<HybridVertex> ClipToStyleSide(List<HybridVertex> input, Rect rect,
        float bottomX, float topX, bool keepLeft)
    {
        var output = new List<HybridVertex>();
        if (input.Count == 0) return output;

        HybridVertex previous = input[input.Count - 1];
        float previousDistance = SideDistance(previous.position, rect, bottomX, topX);
        bool previousInside = keepLeft ? previousDistance <= 0f : previousDistance >= 0f;
        foreach (HybridVertex current in input)
        {
            float currentDistance = SideDistance(current.position, rect, bottomX, topX);
            bool currentInside = keepLeft ? currentDistance <= 0f : currentDistance >= 0f;
            if (currentInside != previousInside)
            {
                float denominator = previousDistance - currentDistance;
                float t = Mathf.Abs(denominator) < 0.0001f ? 0f : previousDistance / denominator;
                output.Add(HybridVertex.Lerp(previous, current, t));
            }
            if (currentInside) output.Add(current);
            previous = current;
            previousDistance = currentDistance;
            previousInside = currentInside;
        }
        return output;
    }

    private static float SideDistance(Vector2 position, Rect rect, float bottomX, float topX)
    {
        float y = Mathf.InverseLerp(rect.yMin, rect.yMax, position.y);
        return position.x - Mathf.Lerp(bottomX, topX, y);
    }

    private void AddPolygon(VertexHelper helper, List<HybridVertex> polygon, Color tint, float textureMode, Rect rect)
    {
        if (polygon == null || polygon.Count < 3) return;
        int start = helper.currentVertCount;
        Vector4 auxiliary = new Vector4(rect.width, rect.height, cornerRadius, 0f);
        foreach (HybridVertex source in polygon)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = source.position;
            vertex.color = tint;
            vertex.uv0 = source.uv;
            vertex.uv1 = new Vector4(source.normalizedPosition.x, source.normalizedPosition.y, textureMode, 0f);
            vertex.uv2 = auxiliary;
            helper.AddVert(vertex);
        }
        for (int i = 1; i < polygon.Count - 1; i++) helper.AddTriangle(start, start + i, start + i + 1);
    }

    private static HybridVertex MakeVertex(Vector2 position, Rect rect, Vector2 uv)
    {
        return new HybridVertex
        {
            position = position,
            uv = uv,
            normalizedPosition = new Vector2(
                (position.x - rect.xMin) / rect.width,
                (position.y - rect.yMin) / rect.height)
        };
    }

    private Vector4 AdjustBorders(Vector4 border, Rect rect, Sprite sprite)
    {
        if (sprite == null || border == Vector4.zero) return Vector4.zero;
        float referencePixelsPerUnit = canvas != null ? canvas.referencePixelsPerUnit : 100f;
        float pixelsPerUnit = sprite.pixelsPerUnit / Mathf.Max(1f, referencePixelsPerUnit);
        border /= Mathf.Max(0.0001f, pixelsPerUnit);

        float horizontal = border.x + border.z;
        if (horizontal > rect.width && horizontal > 0f)
        {
            float scale = rect.width / horizontal;
            border.x *= scale;
            border.z *= scale;
        }
        float vertical = border.y + border.w;
        if (vertical > rect.height && vertical > 0f)
        {
            float scale = rect.height / vertical;
            border.y *= scale;
            border.w *= scale;
        }
        return border;
    }

    private void GetSplits(out float bottom, out float top)
    {
        if (splitContext != null) splitContext.GetLocalSplits(rectTransform, out bottom, out top);
        else
        {
            bottom = fallbackBottomSplit;
            top = fallbackTopSplit;
        }
    }

    private void EnsureMaterial()
    {
        if (sharedHybridMaterial == null)
        {
            Shader shader = Resources.Load<Shader>("Shaders/UIHybridStyle");
            if (shader == null) shader = Shader.Find("UI/Hybrid Style");
            if (shader != null)
            {
                sharedHybridMaterial = new Material(shader) { name = "Hybrid Style UI Material" };
                sharedHybridMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
        }
        if (sharedHybridMaterial == null) return;
        if (rightSprite != null)
        {
            if (dualTextureMaterial == null)
                dualTextureMaterial = new Material(sharedHybridMaterial) { hideFlags = HideFlags.HideAndDontSave };
            dualTextureMaterial.SetTexture("_RightTex", rightSprite.texture);
            material = dualTextureMaterial;
        }
        else material = sharedHybridMaterial;
    }
}
