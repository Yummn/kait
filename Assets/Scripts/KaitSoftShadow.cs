using UnityEngine;
using UnityEngine.UI;

// Rounded silhouette with a feathered rim, generated as a small UI mesh.
public sealed class KaitSoftShadow : MaskableGraphic
{
    private float radius = 8, feather = 5;
    public void Shape(float cornerRadius, float softness)
    {
        if (Mathf.Approximately(radius, cornerRadius) && Mathf.Approximately(feather, softness)) return;
        radius = cornerRadius; feather = softness; SetVerticesDirty();
    }
    public static KaitSoftShadow Create(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(KaitSoftShadow));
        go.transform.SetParent(parent, false);
        var shadow = go.GetComponent<KaitSoftShadow>();
        shadow.raycastTarget = false; shadow.maskable = false;
        shadow.color = new Color(.12f,.18f,.15f,.24f);
        return shadow;
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); Rect r = rectTransform.rect;
        float f = Mathf.Min(feather, Mathf.Min(r.width,r.height) * .2f);
        float innerRadius = Mathf.Min(radius, Mathf.Min(r.width,r.height) * .5f - f);
        Vector2 half = r.size * .5f - Vector2.one * (innerRadius + f);
        vh.AddVert(r.center, color, Vector2.zero);
        Color edge = color; edge.a = 0;
        const int perCorner = 9, count = perCorner * 4;
        for (int c=0; c<4; c++)
        {
            Vector2 centre = r.center + new Vector2(c==0 || c==3 ? half.x : -half.x, c<2 ? half.y : -half.y);
            for (int i=0;i<perCorner;i++)
            {
                float angle = (c*90f + i*90f/(perCorner-1)) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                vh.AddVert(centre + dir * innerRadius,color,Vector2.zero);
                vh.AddVert(centre + dir * (innerRadius+f),edge,Vector2.zero);
            }
        }
        for (int i=0;i<count;i++)
        {
            int a=1+i*2,b=1+((i+1)%count)*2;
            vh.AddTriangle(0,a,b); vh.AddTriangle(a,a+1,b+1); vh.AddTriangle(a,b+1,b);
        }
    }
}
