using UnityEngine;
using UnityEngine.UI;

// Soft perimeter only: the board centre stays clear, and this graphic never receives input.
public sealed class KaitAtmosphereGraphic : MaskableGraphic
{
    private float grey, danger, time;
    private Image clockArt, dangerArt;
    private MinimalDangerGraphic minimalDanger;
    private GlobalStyleSplit split;
    private SunlitSplitText dangerClip, minimalClip;
    public void ConfigureSplit(GlobalStyleSplit value)
    {
        if (split == value) return;
        split = value;
        if (dangerClip != null) dangerClip.Configure(split);
        if (minimalClip != null) minimalClip.Configure(split);
    }
    public void SetState(float g, float d, float t)
    {
        grey = Mathf.Clamp01(g); danger = Mathf.Clamp01(d); time = t;
        raycastTarget = false; SetVerticesDirty();
        if (clockArt == null)
        {
            dangerArt = KaitWarningFrames.Image("Danger C", transform);
            dangerArt.type = Image.Type.Sliced;
            dangerArt.fillCenter = false;
            dangerArt.rectTransform.anchorMin = Vector2.zero; dangerArt.rectTransform.anchorMax = Vector2.one;
            dangerArt.rectTransform.offsetMin = dangerArt.rectTransform.offsetMax = Vector2.zero;
            dangerClip = dangerArt.gameObject.AddComponent<SunlitSplitText>();
            dangerClip.PreserveColors(); dangerClip.SetSides(true, false); dangerClip.Configure(split);
            var minimalObject = new GameObject("Minimal Danger", typeof(RectTransform), typeof(MinimalDangerGraphic));
            minimalObject.transform.SetParent(transform, false);
            minimalDanger = minimalObject.GetComponent<MinimalDangerGraphic>();
            minimalDanger.raycastTarget = false; minimalDanger.maskable = false;
            minimalDanger.rectTransform.anchorMin = Vector2.zero;
            minimalDanger.rectTransform.anchorMax = Vector2.one;
            minimalDanger.rectTransform.offsetMin = minimalDanger.rectTransform.offsetMax = Vector2.zero;
            minimalClip = minimalObject.AddComponent<SunlitSplitText>();
            minimalClip.PreserveColors(); minimalClip.SetSides(false, true); minimalClip.Configure(split);
            clockArt = KaitWarningFrames.Image("Clock A", transform);
            clockArt.rectTransform.anchorMin = clockArt.rectTransform.anchorMax = Vector2.zero;
            clockArt.rectTransform.anchoredPosition = new Vector2(64, 68);
            clockArt.rectTransform.sizeDelta = new Vector2(96, 96);
            clockArt.preserveAspect = true;
        }
        clockArt.sprite = KaitWarningFrames.Frame("ClockA", t);
        dangerArt.sprite = KaitWarningFrames.Frame("DangerC", t);
        FitDangerToViewport();
        clockArt.color = new Color(1,1,1,clockArt.sprite == null ? 0 : grey);
        dangerArt.color = new Color(1,1,1,dangerArt.sprite == null ? 0 : danger*.92f);
        minimalDanger.color = new Color(.84f, .22f, .24f, split == null ? 0 : danger * .48f);
    }
    // The approved sheet has transparent gutters around its four corners. Stretching
    // its RectTransform alone aligns the gutter, not the visible warning, to the screen.
    public static float DangerCornerSize(Vector2 viewport) => Mathf.Min(viewport.x, viewport.y) * .165f;
    public static float DangerBleed(Vector2 viewport) => DangerCornerSize(viewport) * (.06f / .4f);
    private void FitDangerToViewport()
    {
        if (dangerArt == null || dangerArt.sprite == null) return;
        Vector2 viewport = rectTransform.rect.size;
        float corner = DangerCornerSize(viewport);
        if (corner <= 0) return;
        // Nine-slice keeps the corner artwork undistorted at every screen aspect.
        float referencePPU = canvas != null ? canvas.referencePixelsPerUnit : 100f;
        dangerArt.pixelsPerUnitMultiplier = dangerArt.sprite.border.x * referencePPU /
            (dangerArt.sprite.pixelsPerUnit * corner);
        float bleed = DangerBleed(viewport);
        dangerArt.rectTransform.offsetMin = Vector2.one * -bleed;
        dangerArt.rectTransform.offsetMax = Vector2.one * bleed;
    }
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        FitDangerToViewport();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); if (grey < .001f && danger < .001f) return;
        Rect r = rectTransform.rect;
        float depth = Mathf.Min(r.width, r.height) * .14f;
        Color tint = new Color(.36f,.38f,.41f);
        float opacity = grey * .48f;
        const int bands = 28;
        for (int n = 0; n < bands; n++)
        {
            float a = (float)n / bands, b = (float)(n+1) / bands;
            Vector2[] outer = Corners(r, depth*a), inner = Corners(r, depth*b);
            Color ca = tint, cb = tint; ca.a = opacity * Mathf.Pow(1-a,2); cb.a = opacity * Mathf.Pow(1-b,2);
            for (int j=0;j<4;j++)
            {
                int k=vh.currentVertCount, next=(j+1)%4;
                vh.AddVert(outer[j],ca,Vector2.zero);vh.AddVert(outer[next],ca,Vector2.zero);
                vh.AddVert(inner[next],cb,Vector2.zero);vh.AddVert(inner[j],cb,Vector2.zero);
                vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
            }
        }
        if (clockArt != null || grey < .001f) return;
        // Lower-left clock avoids the top card dock and the centre movement controls.
        Vector2 centre = new Vector2(r.xMin+58, r.yMin+64);
        Color ink = new Color(.88f,.91f,.94f,grey*.8f);
        for (int i=0;i<96;i++)
        {
            float a=i*Mathf.PI*2/96, b=(i+1)*Mathf.PI*2/96;
            Line(vh,centre+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*24,centre+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*24,1.6f,ink);
        }
        for(int i=0;i<12;i++)
        {
            Vector2 dir = new Vector2(Mathf.Sin(i*Mathf.PI/6),Mathf.Cos(i*Mathf.PI/6));
            Line(vh,centre+dir*19,centre+dir*22,1,ink);
        }
        Line(vh,centre,centre+new Vector2(-8,9),2,ink);
        float hand = time*.3f;
        Line(vh,centre,centre+new Vector2(Mathf.Sin(hand),Mathf.Cos(hand))*16,1.4f,ink);
    }
    private static Vector2[] Corners(Rect r,float d) => new[]{new Vector2(r.xMin+d,r.yMin+d),new Vector2(r.xMin+d,r.yMax-d),new Vector2(r.xMax-d,r.yMax-d),new Vector2(r.xMax-d,r.yMin+d)};
    private static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color c)
    {
        Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;
        int k=vh.currentVertCount;vh.AddVert(a-n,c,Vector2.zero);vh.AddVert(a+n,c,Vector2.zero);vh.AddVert(b+n,c,Vector2.zero);vh.AddVert(b-n,c,Vector2.zero);
        vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
    }
}
