using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Draw the complete painting above the boards, omitting only their live bounds.
// No stencil/RectMask2D: holes follow layout changes rather than a baked screen size.
public sealed class ReynardSceneFrame:MaskableGraphic
{
 public Texture2D Texture;public RectTransform Background;
 public RectTransform[] Battle,Threat;
 float topSplit=.609f,bottomSplit=.425f;
 public override Texture mainTexture=>Texture!=null?Texture:s_WhiteTexture;
 public void ConfigureSplit(float top,float bottom){topSplit=Mathf.Clamp01(top);bottomSplit=Mathf.Clamp01(bottom);SetVerticesDirty();}
 readonly Vector3[] corners=new Vector3[4];
 Rect BoundsOf(RectTransform[] cells,float padding){bool first=true;Vector2 lo=Vector2.zero,hi=lo;foreach(var cell in cells){if(cell==null)continue;cell.GetWorldCorners(corners);foreach(var c in corners){Vector2 p=rectTransform.InverseTransformPoint(c);if(first){lo=hi=p;first=false;}else{lo=Vector2.Min(lo,p);hi=Vector2.Max(hi,p);}}}return Rect.MinMaxRect(lo.x-padding,lo.y-padding,hi.x+padding,hi.y+padding);}
 void LateUpdate(){if(Background==null)return;Background.GetWorldCorners(corners);Vector3 a=transform.parent.InverseTransformPoint(corners[0]),b=transform.parent.InverseTransformPoint(corners[2]);rectTransform.anchoredPosition=(a+b)*.5f;rectTransform.sizeDelta=b-a;SetVerticesDirty();}
 float Map(float p,float[] from,float[] to){for(int i=0;i<from.Length-1;i++)if(p<=from[i+1])return Mathf.Lerp(to[i],to[i+1],Mathf.InverseLerp(from[i],from[i+1],p));return 1;}
 float Side(Vector2 p,Rect r){float t=Mathf.InverseLerp(r.yMin,r.yMax,p.y);float cut=Mathf.Lerp(r.xMin+r.width*bottomSplit,r.xMin+r.width*topSplit,t);return p.x-cut;}
 List<Vector2> ClipLeft(List<Vector2> input,Rect r){var output=new List<Vector2>();if(input.Count==0)return output;Vector2 previous=input[input.Count-1];float previousSide=Side(previous,r);foreach(var current in input){float currentSide=Side(current,r);bool currentInside=currentSide<=0,previousInside=previousSide<=0;if(currentInside!=previousInside){float t=previousSide/(previousSide-currentSide);output.Add(Vector2.Lerp(previous,current,t));}if(currentInside)output.Add(current);previous=current;previousSide=currentSide;}return output;}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(Battle==null||Threat==null||Texture==null)return;Rect r=rectTransform.rect,a=BoundsOf(Battle,-1),b=BoundsOf(Threat,24);
 var xs=new List<float>{r.xMin,r.xMax,Mathf.Clamp(a.xMin,r.xMin,r.xMax),Mathf.Clamp(a.xMax,r.xMin,r.xMax),Mathf.Clamp(b.xMin,r.xMin,r.xMax),Mathf.Clamp(b.xMax,r.xMin,r.xMax)};
 var ys=new List<float>{r.yMin,r.yMax,Mathf.Clamp(a.yMin,r.yMin,r.yMax),Mathf.Clamp(a.yMax,r.yMin,r.yMax),Mathf.Clamp(b.yMin,r.yMin,r.yMax),Mathf.Clamp(b.yMax,r.yMin,r.yMax)};xs.Sort();ys.Sort();
 for(int x=0;x<xs.Count-1;x++)for(int y=0;y<ys.Count-1;y++){float l=xs[x],rr=xs[x+1],d=ys[y],u=ys[y+1];if(rr-l<.01f||u-d<.01f)continue;var polygon=ClipLeft(new List<Vector2>{new Vector2(l,d),new Vector2(l,u),new Vector2(rr,u),new Vector2(rr,d)},r);if(polygon.Count<3)continue;int n=vh.currentVertCount;foreach(var p in polygon)vh.AddVert(p,color,new Vector2(Map(p.x,new[]{r.xMin,a.xMin,a.xMax,b.xMin,b.xMax,r.xMax},new[]{0f,.067f,.42f,.58f,.943f,1f}),Map(p.y,new[]{r.yMin,a.yMin,a.yMax,r.yMax},new[]{0f,.24f,.823f,1f})));for(int i=1;i<polygon.Count-1;i++)vh.AddTriangle(n,n+i,n+i+1);}
 }
}
