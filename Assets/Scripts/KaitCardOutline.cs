using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class KaitCardOutline : MaskableGraphic
{
    private GlobalStyleSplit split;
    private float lastBottom=float.NaN,lastTop=float.NaN;
    private readonly List<UIVertex> source=new List<UIVertex>(216);
    private readonly UIVertex[] clipped=new UIVertex[4];

    // Slot highlights deliberately do not call this: only the flat card border
    // belongs exclusively to the right style. No mask or extra input surface.
    public void ConfigureRightSide(GlobalStyleSplit context)
    {split=context;lastBottom=lastTop=float.NaN;SetVerticesDirty();}
    private void LateUpdate()
    {
        if(split==null)return;
        split.GetLocalSplits(rectTransform,out float bottom,out float top);
        if(Mathf.Approximately(bottom,lastBottom)&&Mathf.Approximately(top,lastTop))return;
        lastBottom=bottom;lastTop=top;SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var rect=rectTransform.rect;
        const int segments=8;const float radius=9,thickness=2;
        for(int corner=0;corner<4;corner++)for(int step=0;step<=segments;step++)
        {
            float angle=(corner*90+step*90f/segments)*Mathf.Deg2Rad;
            var center=new Vector2(corner==0||corner==3?rect.xMax-radius:rect.xMin+radius,corner<2?rect.yMax-radius:rect.yMin+radius);
            var delta=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
            vh.AddVert(center+delta*radius,color,Vector2.zero);
            vh.AddVert(center+delta*(radius-thickness),color,Vector2.zero);
        }
        int count=4*(segments+1);
        for(int i=0;i<count;i++) {int next=(i+1)%count;vh.AddTriangle(i*2,next*2,i*2+1);vh.AddTriangle(next*2,next*2+1,i*2+1);}
        ClipRight(vh,rect);
    }
    private void ClipRight(VertexHelper vh,Rect rect)
    {
        if(split==null)return;
        split.GetLocalSplits(rectTransform,out float bottom,out float top);
        if(Mathf.Min(bottom,top)>=1){vh.Clear();return;}
        if(Mathf.Max(bottom,top)<=0)return;
        source.Clear();vh.GetUIVertexStream(source);vh.Clear();
        for(int i=0;i+2<source.Count;i+=3)
        {
            int count=0;var previous=source[i+2];float previousDistance=Distance(previous,rect,bottom,top);
            for(int j=0;j<3;j++)
            {
                var current=source[i+j];float distance=Distance(current,rect,bottom,top);
                if((distance>=0)!=(previousDistance>=0))
                {
                    var edge=current;edge.position=Vector3.LerpUnclamped(previous.position,current.position,previousDistance/(previousDistance-distance));
                    clipped[count++]=edge;
                }
                if(distance>=0)clipped[count++]=current;
                previous=current;previousDistance=distance;
            }
            if(count<3)continue;
            int start=vh.currentVertCount;
            for(int j=0;j<count;j++)vh.AddVert(clipped[j]);
            for(int j=1;j<count-1;j++)vh.AddTriangle(start,start+j,start+j+1);
        }
    }
    private static float Distance(UIVertex vertex,Rect rect,float bottom,float top)
    {
        float y=Mathf.InverseLerp(rect.yMin,rect.yMax,vertex.position.y);
        return vertex.position.x-(rect.xMin+rect.width*Mathf.LerpUnclamped(bottom,top,y));
    }
}
