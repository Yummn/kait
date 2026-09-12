using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class KaitBattleGestureSurface : MonoBehaviour { }

// A held intent, not an input queue. A stalled frame never accumulates commands.
public sealed class KaitHoldInput
{
    public KaitDirection? Direction { get; private set; }
    public int Source { get; private set; }
    float due;
    public void Begin(KaitDirection direction,int source,float now){Direction=direction;Source=source;due=now+.4f;}
    public void End(int source){if(Source==source)Clear();}
    public void Clear(){Direction=null;Source=0;}
    public bool Poll(float now,bool enabled,bool ready,out KaitDirection direction)
    {
        direction=Direction??KaitDirection.Up;
        if(!enabled||!ready||!Direction.HasValue)return false;
        if(now<due)return false;
        due=now+.18f;return true;
    }
}

public sealed class KaitHoldDirectionButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
{
    public System.Action Press,Release;
    bool pressed;
    public void OnPointerDown(PointerEventData e)
    {if(e.button!=PointerEventData.InputButton.Left||!GetComponent<Button>().IsInteractable())return;pressed=true;Press?.Invoke();}
    public void OnPointerUp(PointerEventData e)=>Stop();
    public void OnPointerExit(PointerEventData e)=>Stop();
    void OnDisable()=>Stop();
    void Stop(){if(pressed){pressed=false;Release?.Invoke();}}
}

public sealed class KaitStationaryHold
{
    Vector2 origin;float began,maxTravel;bool consumed;
    public void Begin(Vector2 point,float now){origin=point;began=now;maxTravel=0;consumed=false;}
    public void Cancel(){consumed=true;}
    public bool Poll(Vector2 point,float now,float tolerance,bool ready)
    {
        maxTravel=Mathf.Max(maxTravel,Vector2.Distance(origin,point));
        if(consumed||maxTravel>tolerance||now-began<.55f||!ready)return false;
        consumed=true;return true;
    }
}
