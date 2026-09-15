using UnityEngine;
using UnityEngine.UI;

// A dedicated higher-density left forest texture, clipped geometrically to
// the global world edge. No stencil mask and no new clipping on characters.
public sealed class KaitForestDetail : MaskableGraphic
{
    public Sprite Sprite;
    public bool Snow;
    public override Texture mainTexture=>Sprite!=null?Sprite.texture:Texture2D.whiteTexture;
    public void SetSeason(bool snow)
    {
        Snow=snow;Sprite=KaitStorybookArt.Detail(snow?"SnowForest":"GrassForest");
        raycastTarget=false;SetAllDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();if(Sprite==null)return;
        Rect r=rectTransform.rect;
        // Re-register each illustration's ink edge to the same world diagonal;
        // don't let the generated purple triangle overwrite the right backdrop.
        float sourceBottom=Snow?.674f:.646f,sourceTop=Snow?.984f:.958f;
        vh.AddVert(new Vector3(r.xMin,r.yMin),color,new Vector2(0,0));
        vh.AddVert(new Vector3(r.xMin+r.width*(.422f/.606f),r.yMin),color,new Vector2(sourceBottom,0));
        vh.AddVert(new Vector3(r.xMax,r.yMax),color,new Vector2(sourceTop,1));
        vh.AddVert(new Vector3(r.xMin,r.yMax),color,new Vector2(0,1));
        vh.AddTriangle(0,1,2);vh.AddTriangle(2,3,0);
    }
}
