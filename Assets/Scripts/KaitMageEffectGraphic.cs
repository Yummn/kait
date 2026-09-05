using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Ground-only spell art, clipped in geometry to the resolved legal cells.</summary>
public sealed class KaitMageEffectGraphic : MaskableGraphic
{
    private static Texture2D atlas;
    private static Material atlasMaterial;
    private readonly List<Rect> cells = new List<Rect>();
    private bool aiming, center, playing;
    private float progress, elapsed;
    public const float Duration = .32f;
    public int Frame => Mathf.Clamp(Mathf.FloorToInt(progress * 8f), 0, 7);
    public bool AtlasReady => atlas != null && atlasMaterial != null;
    public int CellCount => cells.Count;
    public override Texture mainTexture => aiming ? base.mainTexture : atlas;

    public void ConfigureAim(bool isCenter)
    {
        aiming = true; center = isCenter; playing = false;
        material = null; raycastTarget = false; maskable = false;
        SetMaterialDirty(); SetVerticesDirty();
    }

    public void ConfigureImpact(IEnumerable<Rect> legalCells)
    {
        aiming = false; cells.Clear(); cells.AddRange(legalCells);
        if (atlas == null) atlas = Resources.Load<Texture2D>("KaitVisuals/Effects/MageImpactA");
        if (atlasMaterial == null)
        {
            Shader shader = Resources.Load<Shader>("Shaders/UIWhiteGoldShatter");
            if (shader != null) atlasMaterial = new Material(shader)
                { name = "Mage A Ground", hideFlags = HideFlags.HideAndDontSave };
        }
        material = atlasMaterial; color = Color.white; raycastTarget = false; maskable = false;
        elapsed = 0; progress = 0; playing = true;
        SetMaterialDirty(); SetVerticesDirty();
    }

    public void SetProgress(float value) { progress = Mathf.Clamp01(value); SetVerticesDirty(); }

    private void Update()
    {
        if (!playing) return;
        elapsed += Time.unscaledDeltaTime;
        SetProgress(elapsed / Duration);
        if (elapsed >= Duration) { playing = false; Destroy(gameObject); }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); Rect full = rectTransform.rect;
        if (full.width <= 0 || full.height <= 0) return;
        if (aiming) { DrawAim(vh, full); return; }
        if (!AtlasReady || progress >= 1) return;
        Rect uv = KaitSwordAtlasView.FrameUv(Frame, atlas.width, atlas.height);
        Color tint = new Color(1, 1, 1, progress > .75f ? (1-progress)/.25f : 1);
        foreach (Rect cell in cells)
        {
            Rect cut = Rect.MinMaxRect(Mathf.Max(full.xMin,cell.xMin), Mathf.Max(full.yMin,cell.yMin),
                Mathf.Min(full.xMax,cell.xMax), Mathf.Min(full.yMax,cell.yMax));
            if (cut.width <= 0 || cut.height <= 0) continue;
            Rect sample = Rect.MinMaxRect(
                uv.xMin + (cut.xMin-full.xMin)/full.width*uv.width,
                uv.yMin + (cut.yMin-full.yMin)/full.height*uv.height,
                uv.xMin + (cut.xMax-full.xMin)/full.width*uv.width,
                uv.yMin + (cut.yMax-full.yMin)/full.height*uv.height);
            Quad(vh, cut, tint, sample);
        }
    }

    private void DrawAim(VertexHelper vh, Rect rect)
    {
        float unit = Mathf.Min(rect.width,rect.height);
        Rect inner = new Rect(rect.xMin+unit*.04f,rect.yMin+unit*.04f,rect.width-unit*.08f,rect.height-unit*.08f);
        Quad(vh, inner, new Color(.443f,.255f,.608f,.12f), new Rect());
        Color purple = new Color(.502f,.329f,.616f,.9f);
        float inset=unit*.095f, length=unit*.14f, width=unit*.025f;
        foreach (int x in new[] {-1,1}) foreach (int y in new[] {-1,1})
        {
            Vector2 corner = rect.center + new Vector2(x*(rect.width/2-inset),y*(rect.height/2-inset));
            Line(vh,corner,corner+new Vector2(-x*length,0),width,purple);
            Line(vh,corner,corner+new Vector2(0,-y*length),width,purple);
        }
        if (!center) return;
        float radius=unit*.17f;
        for (int i=0;i<4;i++)
        {
            float a=i*Mathf.PI/2,b=(i+1)*Mathf.PI/2;
            Line(vh,rect.center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,
                rect.center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,width,new Color(.918f,.855f,.961f,1));
        }
    }

    private static void Quad(VertexHelper vh, Rect rect, Color tint, Rect uv)
    {
        int n=vh.currentVertCount;
        vh.AddVert(new Vector3(rect.xMin,rect.yMin),tint,new Vector2(uv.xMin,uv.yMin));
        vh.AddVert(new Vector3(rect.xMin,rect.yMax),tint,new Vector2(uv.xMin,uv.yMax));
        vh.AddVert(new Vector3(rect.xMax,rect.yMax),tint,new Vector2(uv.xMax,uv.yMax));
        vh.AddVert(new Vector3(rect.xMax,rect.yMin),tint,new Vector2(uv.xMax,uv.yMin));
        vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n+2,n+3,n);
    }

    private static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color tint)
    {
        Vector2 d=b-a,normal=new Vector2(-d.y,d.x).normalized*width/2;
        int n=vh.currentVertCount;
        vh.AddVert(a-normal,tint,Vector2.zero);vh.AddVert(a+normal,tint,Vector2.zero);
        vh.AddVert(b+normal,tint,Vector2.zero);vh.AddVert(b-normal,tint,Vector2.zero);
        vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n+2,n+3,n);
    }
}
