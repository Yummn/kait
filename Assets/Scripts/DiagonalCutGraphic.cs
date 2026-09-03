using UnityEngine;
using UnityEngine.UI;

public sealed class DiagonalCutGraphic : MaskableGraphic
{
    [Range(0f, 1f)] public float topSplit = 0.65f;
    [Range(0f, 1f)] public float bottomSplit = 0.35f;
    public float seamWidth = 4f;
    public Color rightColor = Color.black;
    public Color seamColor = Color.white;

    public void SetStyle(float top, float bottom, Color right, Color seam, float width)
    {
        topSplit = Mathf.Clamp01(top);
        bottomSplit = Mathf.Clamp01(bottom);
        rightColor = right;
        seamColor = seam;
        seamWidth = Mathf.Max(0f, width);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = rectTransform.rect;
        float topX = Mathf.Lerp(rect.xMin, rect.xMax, topSplit);
        float bottomX = Mathf.Lerp(rect.xMin, rect.xMax, bottomSplit);

        AddQuad(vertexHelper,
            new Vector2(bottomX, rect.yMin),
            new Vector2(rect.xMax, rect.yMin),
            new Vector2(rect.xMax, rect.yMax),
            new Vector2(topX, rect.yMax),
            rightColor);

        if (seamWidth <= 0f || seamColor.a <= 0f) return;
        float halfWidth = seamWidth * 0.5f;
        AddQuad(vertexHelper,
            new Vector2(bottomX - halfWidth, rect.yMin),
            new Vector2(bottomX + halfWidth, rect.yMin),
            new Vector2(topX + halfWidth, rect.yMax),
            new Vector2(topX - halfWidth, rect.yMax),
            seamColor);
    }

    private static void AddQuad(VertexHelper vertexHelper, Vector2 bottomLeft, Vector2 bottomRight,
        Vector2 topRight, Vector2 topLeft, Color color)
    {
        int start = vertexHelper.currentVertCount;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = bottomLeft;
        vertexHelper.AddVert(vertex);
        vertex.position = bottomRight;
        vertexHelper.AddVert(vertex);
        vertex.position = topRight;
        vertexHelper.AddVert(vertex);
        vertex.position = topLeft;
        vertexHelper.AddVert(vertex);

        vertexHelper.AddTriangle(start, start + 1, start + 2);
        vertexHelper.AddTriangle(start, start + 2, start + 3);
    }
}
