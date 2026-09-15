using UnityEngine;
using UnityEngine.UI;

// Keep the original off-canvas cut ends at the actual screen edges. Fit the
// transparent silhouette, not its rectangular bounds, against the tray corner.
[DefaultExecutionOrder(50)]
public sealed class KaitCornerBough : MonoBehaviour
{
    public RectTransform Viewport;
    public bool Snow;
    static readonly float[] snow={.015f,.058f,.061f,.078f,.117f,.212f,.236f,.338f,.372f,.414f,.421f,.321f,.331f,.353f,.389f,.397f,.404f,.428f,.489f,.513f,.691f,.723f,.742f,.776f,.869f,.905f,.908f,.915f,.934f,.968f,1,1,1};
    static readonly float[] grass={.015f,.063f,.046f,.063f,.102f,.131f,.165f,.326f,.333f,.328f,.343f,.319f,.328f,.350f,.382f,.382f,.438f,.491f,.487f,.504f,.552f,.684f,.710f,.793f,.861f,.844f,.842f,.854f,.878f,.966f,.978f,1,1};
    void LateUpdate()=>Fit();
    public void Fit()
    {
        if(Viewport==null)return;
        var rect=(RectTransform)transform;var board=(RectTransform)transform.parent;
        Vector2 edge=board.InverseTransformPoint(Viewport.TransformPoint(new Vector2(Viewport.rect.xMax,Viewport.rect.yMin)));
        // This point is just inside the paper rim, outside the playable grid.
        float dx=edge.x-(board.rect.xMax-8),dy=(board.rect.yMin+8)-edge.y;
        var contour=Snow?snow:grass;float width=120;
        for(;width<900;width+=1)
        {
            float v=dy/(width*391f/412);if(v<0||v>=1)continue;
            float index=v*32;int lower=Mathf.FloorToInt(index);
            float boundary=Mathf.Lerp(contour[lower],contour[Mathf.Min(32,lower+1)],index-lower);
            if(1-dx/width>=boundary)break;
        }
        width+=2;
        rect.pivot=new Vector2(1,0);rect.anchoredPosition=edge;
        rect.sizeDelta=new Vector2(width,width*391f/412);
    }
}
