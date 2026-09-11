using UnityEngine;
using UnityEngine.UI;

// Untextured perimeter: no ornaments, centre fill, or stroke along the style seam.
public sealed class MinimalDangerGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        float depth = Mathf.Min(r.width, r.height) * .055f;
        const int bands = 16;
        for (int n = 0; n < bands; n++)
        {
            float a = (float)n / bands, b = (float)(n + 1) / bands;
            Vector2[] outer = Corners(r, depth * a), inner = Corners(r, depth * b);
            Color ca = color, cb = color;
            ca.a *= (1 - a) * (1 - a); cb.a *= (1 - b) * (1 - b);
            for (int j = 0; j < 4; j++)
            {
                int k = vh.currentVertCount, next = (j + 1) % 4;
                vh.AddVert(outer[j], ca, Vector2.zero); vh.AddVert(outer[next], ca, Vector2.zero);
                vh.AddVert(inner[next], cb, Vector2.zero); vh.AddVert(inner[j], cb, Vector2.zero);
                vh.AddTriangle(k, k + 1, k + 2); vh.AddTriangle(k, k + 2, k + 3);
            }
        }
    }
    private static Vector2[] Corners(Rect r, float d) => new[] {
        new Vector2(r.xMin+d,r.yMin+d), new Vector2(r.xMin+d,r.yMax-d),
        new Vector2(r.xMax-d,r.yMax-d), new Vector2(r.xMax-d,r.yMin+d) };
}
