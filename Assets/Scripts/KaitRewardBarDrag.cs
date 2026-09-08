using UnityEngine;
using UnityEngine.EventSystems;

// Only the count/grip area owns dragging; action buttons never move the toolbar.
public sealed class KaitRewardBarDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform bar,area;
    private Vector2 offset;
    private int pointer=int.MinValue;
    public void Configure(RectTransform target,RectTransform bounds){bar=target;area=bounds;}
    public void OnBeginDrag(PointerEventData e)
    {
        if(pointer!=int.MinValue||e.button!=PointerEventData.InputButton.Left||bar==null)return;
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(area,e.position,e.pressEventCamera,out var p))return;
        pointer=e.pointerId;offset=bar.anchoredPosition-p;e.eligibleForClick=false;bar.SetAsLastSibling();
    }
    public void OnDrag(PointerEventData e)
    {
        if(pointer!=e.pointerId)return;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(area,e.position,e.pressEventCamera,out var p))bar.anchoredPosition=Clamp(p+offset,area.rect,bar.rect.size);
    }
    public void OnEndDrag(PointerEventData e){if(pointer==e.pointerId){pointer=int.MinValue;e.eligibleForClick=false;}}
    public static Vector2 Clamp(Vector2 p,Rect bounds,Vector2 size)
    {
        Vector2 room=Vector2.Max(Vector2.zero,(bounds.size-size)*.5f-Vector2.one*8);
        return new Vector2(Mathf.Clamp(p.x,bounds.center.x-room.x,bounds.center.x+room.x),Mathf.Clamp(p.y,bounds.center.y-room.y,bounds.center.y+room.y));
    }
    private void LateUpdate(){if(bar!=null&&area!=null)bar.anchoredPosition=Clamp(bar.anchoredPosition,area.rect,bar.rect.size);}
    private void OnDisable(){pointer=int.MinValue;}
    private void OnApplicationFocus(bool focused){if(!focused)pointer=int.MinValue;}
}
