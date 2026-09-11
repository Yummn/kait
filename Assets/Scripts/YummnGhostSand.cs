using UnityEngine;
using UnityEngine.UI;

// C's sparse diamond motes, drawn independently of the actual frozen Spine pose.
public sealed class YummnGhostSand : MaskableGraphic
{
    private float age;
    protected override void Awake(){base.Awake();raycastTarget=false;maskable=false;color=new Color(.68f,.9f,1f,.65f);}
    private void Update(){age+=Time.unscaledDeltaTime;SetVerticesDirty();}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        for(int i=0;i<8;i++)
        {
            float phase=age*.6f+i*.83f;
            float x=(i%2==0?-1:1)*(38+i%3*5);
            float y=-40+i*12+Mathf.Sin(phase)*3;
            float radius=1.2f+(Mathf.Sin(phase)+1)*.65f;
            var c=color;c.a*=.35f+.65f*(Mathf.Sin(phase)+1)*.5f;
            int n=vh.currentVertCount;
            vh.AddVert(new Vector3(x,y+radius*1.7f),c,Vector2.zero);
            vh.AddVert(new Vector3(x+radius,y),c,Vector2.zero);
            vh.AddVert(new Vector3(x,y-radius*1.7f),c,Vector2.zero);
            vh.AddVert(new Vector3(x-radius,y),c,Vector2.zero);
            vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
    }
}
