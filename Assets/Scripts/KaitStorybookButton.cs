using UnityEngine;
using UnityEngine.UI;

// Fixed hit box / rim; only the ivory face and label depress.
[RequireComponent(typeof(Image))]
public sealed class KaitStorybookButton : Button
{
    public RectTransform Face { get; private set; }
    Image faceImage;
    float targetY=1,visualY=1;
    public void Prepare()
    {
        var seat=GetComponent<Image>();seat.sprite=KaitStorybookTheme.KeyLayer(false);seat.type=Image.Type.Sliced;seat.color=Color.white;
        targetGraphic=seat;transition=Transition.None;
        var go=new GameObject("Ivory Button Face",typeof(RectTransform),typeof(Image));go.transform.SetParent(transform,false);
        Face=(RectTransform)go.transform;Face.anchorMin=Vector2.zero;Face.anchorMax=Vector2.one;Face.offsetMin=new Vector2(5,6);Face.offsetMax=new Vector2(-5,-4);
        faceImage=go.GetComponent<Image>();faceImage.sprite=KaitStorybookTheme.KeyLayer(true);faceImage.type=Image.Type.Sliced;faceImage.raycastTarget=false;
        if(((RectTransform)transform).rect.height<40)
        {
            // Short settings rows need the full text height, not a square-key bezel.
            seat.sprite=KaitStorybookTheme.Hud;
            faceImage.sprite=KaitStorybookTheme.Surface("rule-row-face",KaitStorybookTheme.Paper,Color.clear,0,8);
            Face.offsetMin=new Vector2(3,1);Face.offsetMax=new Vector2(-3,-1);
        }
    }
    protected override void DoStateTransition(SelectionState state,bool instant)
    {
        base.DoStateTransition(state,true);targetY=state==SelectionState.Pressed?-1.5f:1f;
        if(faceImage!=null)faceImage.color=state==SelectionState.Disabled?new Color(.78f,.77f,.79f,.82f):state==SelectionState.Pressed?new Color(.95f,.97f,.97f):state==SelectionState.Highlighted?new Color(1.025f,1.025f,1.025f):Color.white;
        if(instant){visualY=targetY;PlaceFace();}
    }
    void Update(){visualY=Mathf.MoveTowards(visualY,targetY,45*Time.unscaledDeltaTime);PlaceFace();}
    void PlaceFace(){if(Face!=null)Face.anchoredPosition=new Vector2(0,visualY);}
    protected override void OnDisable(){base.OnDisable();targetY=visualY=1;PlaceFace();}
}
