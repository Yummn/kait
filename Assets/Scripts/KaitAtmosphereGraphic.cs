using UnityEngine;
using UnityEngine.UI;

// Soft perimeter only: the board centre stays clear, and this graphic never receives input.
public sealed class KaitAtmosphereGraphic : MaskableGraphic
{
    private float grey, danger, time;
    private Image clockArt, dangerArt;
    public const int EdgeClockCount=48;
    private readonly Image[] edgeClocks=new Image[EdgeClockCount];
    private MinimalDangerGraphic minimalDanger;
    private GlobalStyleSplit split;
    private SunlitSplitText dangerClip, minimalClip;
    public void ConfigureSplit(GlobalStyleSplit value)
    {
        if (split == value) return;
        split = value;
        if (dangerClip != null) dangerClip.Configure(split);
        if (minimalClip != null) minimalClip.Configure(split);
        foreach(var clock in edgeClocks)if(clock!=null)clock.GetComponent<SunlitSplitText>().Configure(split);
    }
    public void SetState(float g, float d, float t, float idleSeconds=-1)
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
            edgeClocks[0]=clockArt;
            for(int i=0;i<EdgeClockCount;i++)
            {
                var clock=i==0?clockArt:KaitWarningFrames.Image("Edge Clock "+i,transform);
                edgeClocks[i]=clock;
                clock.rectTransform.anchorMin=clock.rectTransform.anchorMax=Vector2.zero;
                clock.preserveAspect=true;
                var clip=clock.gameObject.AddComponent<SunlitSplitText>();
                clip.PreserveColors();clip.SetSides(true,false);clip.Configure(split);
            }
        }
        clockArt.sprite = KaitWarningFrames.Frame("ClockA", t);
        dangerArt.sprite = KaitWarningFrames.Frame("DangerC", t);
        FitDangerToViewport();
        for(int i=0;i<EdgeClockCount;i++)
        {
            var clock=edgeClocks[i];
            ClockLayout(i,rectTransform.rect.size,out Vector2 position,out float size,out float angle);
            clock.rectTransform.anchoredPosition=position;
            clock.rectTransform.sizeDelta=Vector2.one*size;
            clock.rectTransform.localRotation=Quaternion.Euler(0,0,angle+Mathf.Sin(t*.32f+i)*3);
            clock.sprite=KaitWarningFrames.Frame("ClockA",t+i*.173f);
            // Permuted reveal order spreads each new wave around the perimeter.
            float delay=1.2f+((i*17)%EdgeClockCount)*.16f;
            float reveal=Mathf.SmoothStep(0,1,Mathf.Clamp01(((idleSeconds<0?12:idleSeconds)-delay)/.7f));
            clock.color=new Color(1,1,1,clock.sprite==null?0:grey*reveal*(.52f+.36f*((i*7)%11)/10f));
        }
        dangerArt.color = new Color(1,1,1,dangerArt.sprite == null ? 0 : danger*.92f);
        minimalDanger.color = new Color(.84f, .22f, .24f, split == null ? 0 : danger * .48f);
    }
    public static void ClockLayout(int i,Vector2 viewport,out Vector2 position,out float size,out float angle)
    {
        float unit=Mathf.Min(viewport.x,viewport.y);
        size=unit*Mathf.Lerp(.055f,.105f,((i*7)%13)/12f);
        angle=-65+(i*47)%130;
        float inset=unit*.035f;
        if(i<24)
        {
            int edge=i%3;float p=(i/3+.5f)/8f;
            position=edge==0?new Vector2(inset,viewport.y*Mathf.Lerp(.16f,.84f,p)):
                new Vector2(Mathf.Lerp(unit*.16f,viewport.x*.39f,p),edge==1?viewport.y-inset:inset);
        }
        else
        {
            // Two dense quarter-circle fans join the top/bottom rows to the left edge.
            int j=(i-24)%12;bool top=i>=36;float a=(j+.5f)/12f*Mathf.PI*.5f;
            float radius=unit*(j%2==0?.105f:.165f);
            position=new Vector2(inset+radius*(1-Mathf.Cos(a)),inset+radius*(1-Mathf.Sin(a)));
            if(top)position.y=viewport.y-position.y;
        }
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
