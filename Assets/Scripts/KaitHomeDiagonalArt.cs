using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Geometry and hit testing share the same cuts. Only artwork UVs animate.
public sealed class KaitHomeDiagonalArt : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler
{
    public bool Selected;
    int index;
    Texture2D art;
    bool hovered;
    float zoom = 1f, brightness = .8f;
    static readonly float[] Cuts = { 0f, .19f, .41f, .65f, 1f };
    static readonly float[] Heights = { 0f, .10f, .24f, .43f, .72f, 1f };
    static readonly float[] Shades = { .13f, .17f, .27f, .82f, 1f, .88f };
    public override Texture mainTexture => art != null ? art : Texture2D.whiteTexture;
    public static float Edge(int edge, float y) => Cuts[edge] + (edge > 0 && edge < 4 ? .10f * y : 0f);
    public static float Center(int section, float y) => ((Edge(section,(y+540)/1080) + Edge(section+1,(y+540)/1080))*.5f-.5f)*1920;
    public static bool Contains(int section, Vector2 normalized) => normalized.y >= 0 && normalized.y <= 1 && normalized.x >= Edge(section, normalized.y) && normalized.x < Edge(section+1, normalized.y);
    public void Configure(int section, Texture2D texture) { index=section; art=texture; SetAllDirty(); }
    public void OnPointerEnter(PointerEventData e) { hovered=true; }
    public void OnPointerExit(PointerEventData e) { hovered=false; }
    protected override void OnDisable() { hovered=false; base.OnDisable(); }
    void Update()
    {
        float t=1-Mathf.Exp(-7*Time.unscaledDeltaTime);
        zoom=Mathf.Lerp(zoom,Selected?(hovered?1.075f:1.045f):(hovered?1.035f:1f),t);
        brightness=Mathf.Lerp(brightness,Selected||hovered?1f:.78f,t); SetVerticesDirty();
    }
    public override bool Raycast(Vector2 sp, Camera cam)
    {
        if (!base.Raycast(sp,cam) || !RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,sp,cam,out var p)) return false;
        var r=rectTransform.rect;
        return Contains(index,new Vector2((p.x-r.xMin)/r.width,(p.y-r.yMin)/r.height));
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); var r=rectTransform.rect;
        for(int row=0;row<Heights.Length-1;row++)
        {
            int n=vh.currentVertCount;
            Add(vh,r,Heights[row],false,Shades[row]); Add(vh,r,Heights[row],true,Shades[row]);
            Add(vh,r,Heights[row+1],true,Shades[row+1]); Add(vh,r,Heights[row+1],false,Shades[row+1]);
            vh.AddTriangle(n,n+1,n+2); vh.AddTriangle(n,n+2,n+3);
        }
    }
    void Add(VertexHelper vh,Rect r,float y,bool right,float shade)
    {
        float x=Edge(index+(right?1:0),y);
        var v=UIVertex.simpleVert;
        v.position=new Vector3(r.xMin+x*r.width,r.yMin+y*r.height);
        float center=Center(index,200)+960;
        float drift=Mathf.Sin(Time.unscaledTime*.75f+index*2)*4;
        float h=index==2?1240:1120;
        float w=art!=null?h*art.width/art.height:1000;
        float sourceCenter=index==0?.46f:index==1?.51f:.54f;
        v.uv0=new Vector2(sourceCenter+(x*1920-center)/(w*zoom),(y*1080-540-drift+(index==2?60:0))/(h*zoom)+.5f);
        Color c=art==null?Color.Lerp(new Color(.14f,.15f,.24f),new Color(.36f,.46f,.55f),y):Color.white;
        c*=shade*(art==null?1:brightness); c.a=1;
        if(QualitySettings.activeColorSpace==ColorSpace.Linear) c=c.linear;
        v.color=c; vh.AddVert(v);
    }
}

public sealed class KaitHomeDiagonalSeams : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); var r=rectTransform.rect;
        for(int edge=1;edge<4;edge++)
        {
            int n=vh.currentVertCount;
            for(int i=0;i<4;i++)
            {
                float y=i<2?0:1;
                var v=UIVertex.simpleVert;
                v.position=new Vector3(r.xMin+KaitHomeDiagonalArt.Edge(edge,y)*r.width+(i==0||i==3?-1.5f:1.5f),r.yMin+y*r.height);
                v.color=new Color32(250,199,183,200); vh.AddVert(v);
            }
            vh.AddTriangle(n,n+1,n+2); vh.AddTriangle(n,n+2,n+3);
        }
    }
}
