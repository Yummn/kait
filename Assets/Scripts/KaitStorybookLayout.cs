using UnityEngine;

// Fit the actual gameplay footprint, not an empty 1920x1080 design canvas.
[DefaultExecutionOrder(-100)]
public sealed class KaitStorybookLayout : MonoBehaviour
{
    public RectTransform Hud;
    public Vector2 HudPosition;
    public RectTransform UiRoot,Help,Settings;
    public RectTransform[] CardAreas;
    public Rect? SafeNormalizedOverride;
    public Vector2 Origin { get; private set; }
    public Rect SafeRect { get; private set; }

    public static float FitScale(Rect safe)=>Mathf.Max(.1f,Mathf.Min((safe.width-56)/1586f,(safe.height-112)/744f));
    public static Vector2 ContentOrigin(Rect safe,float scale)=>new Vector2(safe.center.x,safe.yMax-16-444*scale);
    public static Vector2 ClampCard(Vector2 position,Vector2 size,Rect area)=>new Vector2(
        Mathf.Clamp(position.x,area.xMin+size.x*.5f+8,area.xMax-size.x*.5f-8),
        Mathf.Clamp(position.y,area.yMin+size.y*.5f+12,area.yMax-size.y*.5f-12));
    public static Rect MapSafeRect(Rect root,Rect normalized)=>new Rect(root.xMin+root.width*normalized.x,root.yMin+root.height*normalized.y,root.width*normalized.width,root.height*normalized.height);
    void LateUpdate()=>ApplyLayout();
    public void ApplyLayout()
    {
        var parent=UiRoot!=null?UiRoot:transform.parent as RectTransform;
        if(parent==null)return;
        Rect normalized=SafeNormalizedOverride??(Screen.width>0&&Screen.height>0?new Rect(Screen.safeArea.x/Screen.width,Screen.safeArea.y/Screen.height,Screen.safeArea.width/Screen.width,Screen.safeArea.height/Screen.height):new Rect(0,0,1,1));
        SafeRect=MapSafeRect(parent.rect,normalized);
        float scale=FitScale(SafeRect);Vector2 next=ContentOrigin(SafeRect,scale);
        // Preserve any current combat shake; only apply the change in layout origin.
        ((RectTransform)transform).anchoredPosition+=next-Origin;Origin=next;
        transform.localScale=Vector3.one*scale;
        if(Hud!=null)
        {
            Hud.position=transform.TransformPoint(HudPosition);
            Hud.localScale=transform.localScale;
        }
        Place(Help,new Vector2(SafeRect.xMax-156,SafeRect.yMax-48));
        Place(Settings,new Vector2(SafeRect.xMax-56,SafeRect.yMax-48));
        if(CardAreas!=null)foreach(var area in CardAreas)
        {
            if(area==null)continue;
            bool resized=area.rect.size!=SafeRect.size;
            area.anchorMin=area.anchorMax=Vector2.one*.5f;
            area.anchoredPosition=SafeRect.center;area.sizeDelta=SafeRect.size;
            if(resized)
            {
                foreach(var card in area.GetComponentsInChildren<KaitSkillCard>())
                    if(!card.IsCandidate&&card.GetComponent<CanvasGroup>().blocksRaycasts)
                        card.Rect.anchoredPosition=ClampCard(card.Rect.anchoredPosition,card.Rect.sizeDelta,area.rect);
                foreach(var card in area.GetComponentsInChildren<KaitPassiveCard>())
                    if(!card.IsCandidate&&card.GetComponent<CanvasGroup>().blocksRaycasts)
                        card.Rect.anchoredPosition=ClampCard(card.Rect.anchoredPosition,card.Rect.sizeDelta,area.rect);
            }
        }
    }
    static void Place(RectTransform rect,Vector2 position){if(rect!=null){rect.anchorMin=rect.anchorMax=Vector2.one*.5f;rect.anchoredPosition=position;}}
}
