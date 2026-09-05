using System;
using UnityEngine;
using UnityEngine.UI;

// Cell-local geometry keeps the approved segmented warning parallel to the board.
public sealed class KaitCellSignal : MaskableGraphic
{
    public Func<bool> Exists;
    public bool Warning;
    public float Started;
    public const float ResonanceDuration = .46f;
    public bool ManualPreview;
    public float PreviewPhase = .5f;
    public RectTransform Target;
    private readonly Vector3[] corners = new Vector3[4];

    public void SyncPose()
    {
        if (Target == null) return;
        Target.GetWorldCorners(corners);
        Vector3 min = transform.parent.InverseTransformPoint(corners[0]);
        Vector3 max = transform.parent.InverseTransformPoint(corners[2]);
        rectTransform.localPosition = (min + max) * .5f;
        rectTransform.sizeDelta = new Vector2(max.x-min.x,max.y-min.y);
    }

    protected override void Awake() { base.Awake(); raycastTarget = false; maskable = false; }
    private void Update()
    {
        SyncPose();
        if (Exists != null && !Exists()) { Destroy(gameObject); return; }
        if (!Warning && !ManualPreview && Time.unscaledTime - Started >= ResonanceDuration)
        { Destroy(gameObject); return; }
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        float t = ManualPreview ? PreviewPhase : Warning
            ? Mathf.PingPong(Time.unscaledTime * 1.6f, 1f)
            : Mathf.Clamp01((Time.unscaledTime - Started) / ResonanceDuration);
        float pulse = Warning ? Mathf.SmoothStep(.38f, .86f, t) : Mathf.Sin(t * Mathf.PI) * .88f;
        Rect r = rectTransform.rect;
        float inset = Warning ? 4f : 2f;
        for (int side = 0; side < 4; side++)
        {
            float half = (side % 2 == 0 ? r.width : r.height) * .5f;
            float depth = (side % 2 == 0 ? r.height : r.width) * .5f - inset;
            if (Warning)
            {
                // A flat notched plate, with no perspective side face or diagonal footprint.
                float length = half * .42f;
                Polygon(vh, side, r.center, new Color(.39f,.15f,.07f,pulse),
                    new Vector2(-length,depth),new Vector2(length,depth),new Vector2(length-3,depth-4),
                    new Vector2(0,depth-8),new Vector2(-length+3,depth-4));
                Polygon(vh, side, r.center, new Color(.91f,.39f,.14f,pulse),
                    new Vector2(-length+2,depth-1),new Vector2(length-2,depth-1),
                    new Vector2(0,depth-6));
                Polygon(vh, side, r.center, new Color(1f,.76f,.4f,pulse*.8f),
                    new Vector2(-5,depth-4),new Vector2(5,depth-4),new Vector2(0,depth-6));
            }
            else
            {
                Polygon(vh, side, r.center, new Color(1f,.79f,.43f,pulse),
                    new Vector2(-half+6,depth),new Vector2(half-6,depth),
                    new Vector2(half-6,depth-2.5f),new Vector2(-half+6,depth-2.5f));
            }
        }
    }

    private static void Polygon(VertexHelper vh, int side, Vector2 center, Color tint, params Vector2[] points)
    {
        int first = vh.currentVertCount;
        foreach (var point in points)
        {
            Vector2 p = side == 0 ? point : side == 1 ? new Vector2(point.y,-point.x)
                : side == 2 ? -point : new Vector2(-point.y,point.x);
            vh.AddVert(center+p,tint,Vector2.zero);
        }
        for(int i=1;i<points.Length-1;i++)vh.AddTriangle(first,first+i,first+i+1);
    }
}
