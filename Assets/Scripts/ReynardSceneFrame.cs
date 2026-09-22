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
 // The 2048 merge animation scales individual cells.  GetWorldCorners would
 // feed that cosmetic scale into the scene cutout, making the entire painted
 // background appear to wobble.  Build the bounds from the laid-out cell rect
 // while deliberately ignoring each cell's temporary localScale.
 Rect BoundsOf(RectTransform[] cells,float padding){bool first=true;Vector2 lo=Vector2.zero,hi=lo;foreach(var cell in cells){if(cell==null||cell.parent==null)continue;Vector3 center=cell.parent.TransformPoint(cell.localPosition);Quaternion rotation=cell.localRotation;Vector3 dx=cell.parent.TransformVector(rotation*(Vector3.right*(cell.rect.width*.5f)));Vector3 dy=cell.parent.TransformVector(rotation*(Vector3.up*(cell.rect.height*.5f)));corners[0]=center-dx-dy;corners[1]=center-dx+dy;corners[2]=center+dx+dy;corners[3]=center+dx-dy;foreach(var c in corners){Vector2 p=rectTransform.InverseTransformPoint(c);if(first){lo=hi=p;first=false;}else{lo=Vector2.Min(lo,p);hi=Vector2.Max(hi,p);}}}return Rect.MinMaxRect(lo.x-padding,lo.y-padding,hi.x+padding,hi.y+padding);}
 void LateUpdate(){if(Background==null)return;Background.GetWorldCorners(corners);Vector3 a=transform.parent.InverseTransformPoint(corners[0]),b=transform.parent.InverseTransformPoint(corners[2]);rectTransform.anchoredPosition=(a+b)*.5f;rectTransform.sizeDelta=b-a;SetVerticesDirty();}
 float Map(float p,float[] from,float[] to){for(int i=0;i<from.Length-1;i++)if(p<=from[i+1])return Mathf.Lerp(to[i],to[i+1],Mathf.InverseLerp(from[i],from[i+1],p));return 1;}
 float Side(Vector2 p,Rect r){float t=Mathf.InverseLerp(r.yMin,r.yMax,p.y);float cut=Mathf.Lerp(r.xMin+r.width*bottomSplit,r.xMin+r.width*topSplit,t);return p.x-cut;}
 List<Vector2> ClipLeft(List<Vector2> input,Rect r){var output=new List<Vector2>();if(input.Count==0)return output;Vector2 previous=input[input.Count-1];float previousSide=Side(previous,r);foreach(var current in input){float currentSide=Side(current,r);bool currentInside=currentSide<=0,previousInside=previousSide<=0;if(currentInside!=previousInside){float t=previousSide/(previousSide-currentSide);output.Add(Vector2.Lerp(previous,current,t));}if(currentInside)output.Add(current);previous=current;previousSide=currentSide;}return output;}
 // Sample the actual backdrop at the same screen position. Alpha falls inward,
 // so the painted ground and foliage gently overlap the paving, without a frame.
 protected override void OnPopulateMesh(VertexHelper vh)
 {
  vh.Clear();if(Battle==null||Texture==null)return;
  Rect board=BoundsOf(Battle,0),screen=rectTransform.rect;
  float feather=board.width*.012f;
  for(int side=0;side<4;side++)for(int i=0;i<64;i++)
  {
   float t=i/64f,u=(i+1)/64f;
   float wa=feather*(.8f+.2f*Mathf.Sin(i*.67f+side));
   float wb=feather*(.8f+.2f*Mathf.Sin((i+1)*.67f+side));
   Vector2 a,b,inward;
   if(side==0){a=new Vector2(Mathf.Lerp(board.xMin,board.xMax,t),board.yMin);b=new Vector2(Mathf.Lerp(board.xMin,board.xMax,u),board.yMin);inward=Vector2.up;}
   else if(side==1){a=new Vector2(board.xMax,Mathf.Lerp(board.yMin,board.yMax,t));b=new Vector2(board.xMax,Mathf.Lerp(board.yMin,board.yMax,u));inward=Vector2.left;}
   else if(side==2){a=new Vector2(Mathf.Lerp(board.xMax,board.xMin,t),board.yMax);b=new Vector2(Mathf.Lerp(board.xMax,board.xMin,u),board.yMax);inward=Vector2.down;}
   else{a=new Vector2(board.xMin,Mathf.Lerp(board.yMax,board.yMin,t));b=new Vector2(board.xMin,Mathf.Lerp(board.yMax,board.yMin,u));inward=Vector2.right;}
   int n=vh.currentVertCount;Add(vh,a-inward,screen,1);Add(vh,b-inward,screen,1);Add(vh,b+inward*wb,screen,0);Add(vh,a+inward*wa,screen,0);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
  }
 }
 void Add(VertexHelper vh,Vector2 p,Rect r,float alpha){var c=color;c.a*=alpha;vh.AddVert(p,c,new Vector2((p.x-r.xMin)/r.width,(p.y-r.yMin)/r.height));}
}