using UnityEngine;
using UnityEngine.UI;

// A shallow ground transition, not a raised drop-shadow panel. Deterministic
// edge chips bridge the painted scenery and the regular gameplay grid.
public sealed class KaitBoardGrounding : MaskableGraphic
{
    public bool Paper;
    public bool Snow=true;
    public static KaitBoardGrounding Create(RectTransform board,bool paper)
    {
        var g=new GameObject(paper?"Tray Contact Rim":"Pavement Ground Apron",typeof(RectTransform),typeof(KaitBoardGrounding)).GetComponent<KaitBoardGrounding>();
        g.transform.SetParent(board,false);g.rectTransform.sizeDelta=board.sizeDelta;g.Paper=paper;g.raycastTarget=false;g.maskable=false;
        return g;
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();float half=rectTransform.rect.width*.5f;
        if(!Paper)
        {
            // An opaque patch of clean ground covers scenery under the paving.
            // Its outer edge feathers irregularly into the backdrop, not a white halo.
            Color ground=Snow?new Color32(211,231,246,255):new Color32(100,136,81,255);
            Quad(vh,new Vector2(-half,-half),new Vector2(half,-half),new Vector2(half,half),new Vector2(-half,half),ground,ground);
            for(int side=0;side<4;side++)for(int i=0;i<80;i++)
            {
                float a=-half+i*half/40f,b=a+half/40f;
                float spread=Snow?3:1.5f;
                var c=ground;c.a=0;
                Quad(vh,Rotate(new Vector2(a,half),side),Rotate(new Vector2(b,half),side),Rotate(new Vector2(b,half+spread),side),Rotate(new Vector2(a,half+spread),side),ground,ground);
                Quad(vh,Rotate(new Vector2(a,half+spread),side),Rotate(new Vector2(b,half+spread),side),Rotate(new Vector2(b,half+spread+4),side),Rotate(new Vector2(a,half+spread+4),side),ground,c);
            }
            return;
        }
        for(int side=0;side<4;side++)for(int i=0;i<60;i++)
        {
            float along=-half+i*half/30f,next=along+half/30f;
            float wobble=Paper?0:3+Mathf.Sin(i*2.1f+side)*2+Mathf.Sin(i*.7f+side)*2;
            Color inner=Paper?new Color(.24f,.20f,.29f,.20f):Snow?new Color(.65f,.74f,.82f,.55f):new Color(.31f,.43f,.27f,.6f);
            Color outer=Paper?new Color(.24f,.20f,.29f,0):Snow?new Color(.84f,.91f,.95f,0):new Color(.40f,.61f,.36f,0);
            float a=Paper?half-1:half-3,b=half+(Paper?3:13+wobble);
            Quad(vh,Rotate(new Vector2(along,a),side),Rotate(new Vector2(next,a),side),Rotate(new Vector2(next,b),side),Rotate(new Vector2(along,b),side),inner,outer);
        }
    }
    static Vector2 Rotate(Vector2 p,int side)=>side==0?p:side==1?new Vector2(p.y,-p.x):side==2?-p:new Vector2(-p.y,p.x);
    static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color inner,Color outer)
    {int n=vh.currentVertCount;vh.AddVert(a,inner,Vector2.zero);vh.AddVert(b,inner,Vector2.zero);vh.AddVert(c,outer,Vector2.zero);vh.AddVert(d,outer,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
}
