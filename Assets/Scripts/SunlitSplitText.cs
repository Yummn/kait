using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// One Text and one click target across both styles. Only the glyph mesh color
// changes at the global cut: brown on ivory, original color on dark panels.
[RequireComponent(typeof(Text))]
public sealed class SunlitSplitText : BaseMeshEffect
{
    private GlobalStyleSplit context;
    private readonly Color ink = new Color(0.34f, 0.25f, 0.18f, 1f);
    private float lastBottom = float.NaN;
    private float lastTop = float.NaN;
    private bool showLeft = true, showRight = true;

    public void SetSides(bool left, bool right)
    {
        showLeft = left; showRight = right;
        graphic.SetVerticesDirty();
    }

    public void Configure(GlobalStyleSplit split)
    {
        context = split;
        graphic.SetVerticesDirty();
    }

    private void LateUpdate()
    {
        if (context == null || graphic == null) return;
        context.GetLocalSplits(graphic.rectTransform, out float bottom, out float top);
        if (Mathf.Approximately(bottom, lastBottom) && Mathf.Approximately(top, lastTop)) return;
        lastBottom = bottom;
        lastTop = top;
        graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper helper)
    {
        if (!IsActive() || context == null) return;
        context.GetLocalSplits(graphic.rectTransform, out float bottom, out float top);
        Rect rect = graphic.rectTransform.rect;
        var input = new List<UIVertex>();
        helper.GetUIVertexStream(input);
        helper.Clear();
        for (int i = 0; i + 2 < input.Count; i += 3)
        {
            var triangle = new List<UIVertex> { input[i], input[i + 1], input[i + 2] };
            if (showLeft) AddPolygon(helper, Clip(triangle, rect, bottom, top, true), true);
            if (showRight) AddPolygon(helper, Clip(triangle, rect, bottom, top, false), false);
        }
    }

    private static float Distance(UIVertex v, Rect rect, float bottom, float top)
    {
        float y = Mathf.InverseLerp(rect.yMin, rect.yMax, v.position.y);
        return v.position.x - (rect.xMin + rect.width * Mathf.LerpUnclamped(bottom, top, y));
    }

    private static List<UIVertex> Clip(List<UIVertex> input, Rect rect, float bottom, float top, bool left)
    {
        var output = new List<UIVertex>();
        UIVertex previous = input[input.Count - 1];
        float previousDistance = Distance(previous, rect, bottom, top);
        foreach (UIVertex current in input)
        {
            float distance = Distance(current, rect, bottom, top);
            bool inside = left ? distance <= 0 : distance >= 0;
            bool previousInside = left ? previousDistance <= 0 : previousDistance >= 0;
            if (inside != previousInside)
            {
                float t = previousDistance / (previousDistance - distance);
                UIVertex intersection = current;
                intersection.position = Vector3.LerpUnclamped(previous.position, current.position, t);
                intersection.uv0 = Vector4.LerpUnclamped(previous.uv0, current.uv0, t);
                intersection.uv1 = Vector4.LerpUnclamped(previous.uv1, current.uv1, t);
                intersection.color = Color32.Lerp(previous.color, current.color, t);
                output.Add(intersection);
            }
            if (inside) output.Add(current);
            previous = current;
            previousDistance = distance;
        }
        return output;
    }

    private void AddPolygon(VertexHelper helper, List<UIVertex> polygon, bool left)
    {
        if (polygon.Count < 3) return;
        int start = helper.currentVertCount;
        foreach (UIVertex original in polygon)
        {
            UIVertex vertex = original;
            if (left)
            {
                Color color = ink;
                color.a = original.color.a / 255f;
                vertex.color = color;
            }
            helper.AddVert(vertex);
        }
        for (int i = 1; i < polygon.Count - 1; i++) helper.AddTriangle(start, start + i, start + i + 1);
    }
}
