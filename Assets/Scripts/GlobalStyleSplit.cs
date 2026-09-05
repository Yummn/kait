using UnityEngine;

public sealed class GlobalStyleSplit : MonoBehaviour
{
    private RectTransform coordinateSpace;
    private Vector2 bottomNormalized;
    private Vector2 topNormalized;

    public void Configure(RectTransform space, float bottomX, float topX)
    {
        coordinateSpace = space;
        bottomNormalized = new Vector2(Mathf.Clamp01(bottomX), 0f);
        topNormalized = new Vector2(Mathf.Clamp01(topX), 1f);
    }

    public void GetLocalSplits(RectTransform target, out float bottomSplit, out float topSplit)
    {
        bottomSplit = bottomNormalized.x;
        topSplit = topNormalized.x;
        if (coordinateSpace == null || target == null) return;

        Rect sourceRect = coordinateSpace.rect;
        Vector3 bottomWorld = coordinateSpace.TransformPoint(new Vector3(
            Mathf.Lerp(sourceRect.xMin, sourceRect.xMax, bottomNormalized.x),
            Mathf.Lerp(sourceRect.yMin, sourceRect.yMax, bottomNormalized.y), 0f));
        Vector3 topWorld = coordinateSpace.TransformPoint(new Vector3(
            Mathf.Lerp(sourceRect.xMin, sourceRect.xMax, topNormalized.x),
            Mathf.Lerp(sourceRect.yMin, sourceRect.yMax, topNormalized.y), 0f));

        Vector3 localBottom = target.InverseTransformPoint(bottomWorld);
        Vector3 localTop = target.InverseTransformPoint(topWorld);
        Rect targetRect = target.rect;
        bottomSplit = LocalSplitAtY(localBottom, localTop, targetRect.yMin, targetRect);
        topSplit = LocalSplitAtY(localBottom, localTop, targetRect.yMax, targetRect);
    }

    private static float LocalSplitAtY(Vector3 lineBottom, Vector3 lineTop, float y, Rect rect)
    {
        float deltaY = lineTop.y - lineBottom.y;
        float t = Mathf.Abs(deltaY) < 0.0001f ? 0f : (y - lineBottom.y) / deltaY;
        float x = Mathf.LerpUnclamped(lineBottom.x, lineTop.x, t);
        // Keep the intersection outside [0,1]. Clamping bends the diagonal
        // when a moving card crosses the cut with only one corner.
        return rect.width > 0.0001f ? (x - rect.xMin) / rect.width : 0f;
    }
}
