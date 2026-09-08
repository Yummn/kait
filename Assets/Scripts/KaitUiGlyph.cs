using UnityEngine;
using UnityEngine.UI;

// Resolution-independent UI symbols: no font fallback or emoji dependencies on Android.
public sealed class KaitUiGlyph : MaskableGraphic
{
    public enum Symbol { Card, Sword, Shield, Up, Down, Plus, Swap, Check, Clock, Skip, Reroll, Close }
    public Symbol symbol;
    public static KaitUiGlyph Create(Transform parent, Symbol value, Vector2 position, float size)
    {
        var g=new GameObject(value.ToString(),typeof(RectTransform),typeof(KaitUiGlyph)).GetComponent<KaitUiGlyph>();
        g.transform.SetParent(parent,false);g.rectTransform.anchoredPosition=position;g.rectTransform.sizeDelta=Vector2.one*size;
        g.symbol=value;g.color=new Color(1,.94f,.83f);g.raycastTarget=false;return g;
    }
    public void SetSymbol(Symbol value){if(symbol==value)return;symbol=value;SetVerticesDirty();}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        switch(symbol)
        {
            case Symbol.Card: Path(vh,-.55f,-.75f,.55f,-.75f,.55f,.75f,-.55f,.75f,-.55f,-.75f);Line(vh,-.3f,.35f,.3f,.35f);break;
            case Symbol.Sword: Path(vh,-.45f,-.5f,.3f,.65f,.6f,.8f,.65f,.45f,-.25f,-.65f);Line(vh,-.65f,-.3f,-.05f,-.8f);Line(vh,-.45f,-.55f,-.7f,-.85f);break;
            case Symbol.Shield: Path(vh,-.65f,.65f,0,.85f,.65f,.65f,.55f,-.25f,0,-.8f,-.55f,-.25f,-.65f,.65f);break;
            case Symbol.Up: Line(vh,0,-.7f,0,.7f);Path(vh,-.5f,.2f,0,.7f,.5f,.2f);break;
            case Symbol.Down: Line(vh,0,.7f,0,-.7f);Path(vh,-.5f,-.2f,0,-.7f,.5f,-.2f);break;
            case Symbol.Plus: Line(vh,-.6f,0,.6f,0);Line(vh,0,-.6f,0,.6f);break;
            case Symbol.Swap: Path(vh,-.7f,.35f,.7f,.35f,.3f,.7f);Path(vh,.7f,-.35f,-.7f,-.35f,-.3f,-.7f);break;
            case Symbol.Check: Path(vh,-.65f,0,-.15f,-.5f,.7f,.6f);break;
            case Symbol.Close: Line(vh,-.55f,-.55f,.55f,.55f);Line(vh,-.55f,.55f,.55f,-.55f);break;
            case Symbol.Skip: Path(vh,-.6f,-.65f,.25f,0,-.6f,.65f);Line(vh,.65f,-.65f,.65f,.65f);break;
            case Symbol.Clock:
            case Symbol.Reroll:
                float end=symbol==Symbol.Clock?360:300;
                for(float a=0;a<end;a+=20){float b=Mathf.Min(a+20,end);Line(vh,Mathf.Cos(a*Mathf.Deg2Rad)*.7f,Mathf.Sin(a*Mathf.Deg2Rad)*.7f,Mathf.Cos(b*Mathf.Deg2Rad)*.7f,Mathf.Sin(b*Mathf.Deg2Rad)*.7f);}
                if(symbol==Symbol.Clock)Path(vh,0,.45f,0,0,.35f,-.15f);else Path(vh,.15f,-.4f,.35f,-.61f,.68f,-.55f);
                break;
        }
    }
    private void Path(VertexHelper vh,params float[] p){for(int i=0;i+3<p.Length;i+=2)Line(vh,p[i],p[i+1],p[i+2],p[i+3]);}
    private void Line(VertexHelper vh,float x,float y,float a,float b)
    {
        var r=rectTransform.rect;float s=Mathf.Min(r.width,r.height)*.5f;
        Vector2 p=r.center+new Vector2(x,y)*s,q=r.center+new Vector2(a,b)*s,n=new Vector2(-(q-p).y,(q-p).x).normalized*s*.085f;
        int k=vh.currentVertCount;vh.AddVert(p-n,color,Vector2.zero);vh.AddVert(p+n,color,Vector2.zero);vh.AddVert(q+n,color,Vector2.zero);vh.AddVert(q-n,color,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
    }
}
