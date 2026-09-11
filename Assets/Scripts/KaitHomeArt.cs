using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Fixed polygon, animated UVs: portraits breathe without moving the cut or hit area.
public sealed class KaitHomeArt : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler
{
    public bool Selected;
    bool right,hovered;
    Texture2D art;
    float zoom=1.035f,brightness=.65f;
    public override Texture mainTexture=>art!=null?art:Texture2D.whiteTexture;
    public static float Boundary(bool right,float y)=>Mathf.Lerp(right?.60f:.28f,right?.72f:.40f,y);
    public void Configure(bool isRight,Texture2D texture){right=isRight;art=texture;SetAllDirty();}
    public void OnPointerEnter(PointerEventData e){hovered=true;}
    public void OnPointerExit(PointerEventData e){hovered=false;}
    void Update()
    {
        float ease=1-Mathf.Exp(-7*Time.unscaledDeltaTime);
        zoom=Mathf.Lerp(zoom,Selected?(hovered?1.12f:1.09f):(hovered?1.075f:1.035f),ease);
        brightness=Mathf.Lerp(brightness,Selected||hovered?1:.65f,ease);SetVerticesDirty();
    }
    public override bool Raycast(Vector2 sp,Camera cam)
    {
        if(!base.Raycast(sp,cam)||!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,sp,cam,out var p))return false;
        var r=rectTransform.rect;float x=(p.x-r.xMin)/r.width,y=(p.y-r.yMin)/r.height;
        return right?x>=Boundary(true,y):x<=Boundary(false,y);
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;
        float[] ys={0,.10f,.25f,.45f,.75f,1};float[] shades={.10f,.14f,.45f,1,1,.72f};
        for(int i=0;i<ys.Length-1;i++)
        {
            int n=vh.currentVertCount;
            Add(vh,r,ys[i],false,shades[i]);Add(vh,r,ys[i],true,shades[i]);
            Add(vh,r,ys[i+1],true,shades[i+1]);Add(vh,r,ys[i+1],false,shades[i+1]);
            vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
    }
    void Add(VertexHelper vh,Rect r,float y,bool edge,float shade)
    {
        float x=right?(edge?1:Boundary(true,y)):(edge?Boundary(false,y):0);
        float frameLeft=right?1090:-50;
        float px=(x*1920-frameLeft-440)/zoom+440;
        float py=(y*1080-540-Mathf.Sin(Time.unscaledTime*.9f+(right?Mathf.PI:0))*5)/zoom+540;
        // 880x1080 cover of the original 16:9 CG, as in approved A preview.
        float origin=(880-1920)*(right?.52f:.48f);
        var v=UIVertex.simpleVert;v.position=new Vector3(r.xMin+x*r.width,r.yMin+y*r.height);
        v.uv0=new Vector2((px-origin)/1920,py/1080);
        float c=shade*brightness;if(QualitySettings.activeColorSpace==ColorSpace.Linear)c=Mathf.GammaToLinearSpace(c);
        v.color=new Color(c,c,c,1);vh.AddVert(v);
    }
}

public sealed class KaitHomeSeams : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;
        for(int side=0;side<2;side++)
        {
            int n=vh.currentVertCount;
            for(int i=0;i<4;i++)
            {
                float y=i<2?0:1;
                float x=KaitHomeArt.Boundary(side==1,y)*r.width+r.xMin+(i==0||i==3?-1.5f:1.5f);
                var v=UIVertex.simpleVert;v.position=new Vector3(x,r.yMin+y*r.height);v.color=new Color32(250,199,183,180);vh.AddVert(v);
            }
            vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
    }
}
