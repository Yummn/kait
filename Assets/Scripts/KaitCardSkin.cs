using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitCardSkin : MonoBehaviour
{
    public const string Root="KaitVisuals/Build061/";
    private static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
    private KaitCardOutline outline;
    private KaitUiGlyph clockIcon;
    private Text cooldown;
    private static Sprite rounded;
    public static Sprite RoundRect()
    {
        if(rounded!=null)return rounded;
        var t=new Texture2D(32,32,TextureFormat.RGBA32,false){name="Card Rounded UI",filterMode=FilterMode.Bilinear};
        for(int y=0;y<32;y++)for(int x=0;x<32;x++)
        {
            float dx=Mathf.Max(Mathf.Abs(x-15.5f)-7.5f,0),dy=Mathf.Max(Mathf.Abs(y-15.5f)-7.5f,0);
            t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(8.5f-Mathf.Sqrt(dx*dx+dy*dy))));
        }
        t.Apply();rounded=Sprite.Create(t,new Rect(0,0,32,32),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(10,10,10,10));
        return rounded;
    }
    public static Sprite Face(KaitAbilityDef def)
    {
        if(def==null) return null;
        // Raw generation sheets are not runtime assets until alpha has been verified.
        if(Resources.Load<TextAsset>(Root+"ArtReady")==null)return null;
        string key="frame"+(int)def.kind+(int)def.rarity;
        if(cache.TryGetValue(key,out var s))return s;
        var t=Resources.Load<Texture2D>(Root+"CardFrames");if(t==null)return null;
        float w=t.width/3f,h=t.height/2f;
        s=Sprite.Create(t,new Rect((int)def.rarity*w,(def.kind==KaitAbilityKind.Active?1:0)*h,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
        cache[key]=s;return s;
    }
    public static Sprite Icon(KaitAbilityDef def)
    {
        if(def==null)return null;
        string key=def.id;if(cache.TryGetValue(key,out var s))return s;
        int index=KaitAbilityCatalog.All.IndexOf(def),sheet=index/16,cell=index%16;
        var t=Resources.Load<Texture2D>(Root+"Icons"+sheet);if(t==null)return null;
        int rows=sheet==2?2:4;float w=t.width/4f,h=t.height/(float)rows;
        s=Sprite.Create(t,new Rect((cell%4)*w,(rows-1-cell/4)*h,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
        cache[key]=s;return s;
    }
    public static void Apply(GameObject card,KaitAbilityDef def,Font font,GlobalStyleSplit split=null)
    {
        if(def==null)return;
        var skin=card.GetComponent<KaitCardSkin>()??card.AddComponent<KaitCardSkin>();
        if(skin.outline==null)
        {
            var border=new GameObject("Rarity Outline",typeof(RectTransform),typeof(CanvasRenderer),typeof(KaitCardOutline));border.transform.SetParent(card.transform,false);
            skin.outline=border.GetComponent<KaitCardOutline>();skin.outline.raycastTarget=false;
            var rect=skin.outline.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.sizeDelta=Vector2.zero;
            skin.clockIcon=KaitUiGlyph.Create(card.transform,KaitUiGlyph.Symbol.Clock,new Vector2(-12,-94),15);
            var cd=new GameObject("Cooldown Number",typeof(RectTransform),typeof(Text));cd.transform.SetParent(card.transform,false);
            skin.cooldown=cd.GetComponent<Text>();skin.cooldown.font=font;skin.cooldown.fontSize=14;skin.cooldown.alignment=TextAnchor.MiddleCenter;skin.cooldown.raycastTarget=false;
            skin.cooldown.rectTransform.anchoredPosition=new Vector2(10,-94);skin.cooldown.rectTransform.sizeDelta=new Vector2(26,20);
            cd.AddComponent<SunlitSplitText>().Configure(split);
            skin.clockIcon.color=new Color(.66f,.52f,.36f);
        }
        skin.outline.ConfigureRightSide(split);
        skin.outline.color=KaitAbilityCatalog.RarityColor(def.rarity);
        skin.clockIcon.gameObject.SetActive(def.kind==KaitAbilityKind.Active);
        skin.cooldown.gameObject.SetActive(def.kind==KaitAbilityKind.Active);
        skin.cooldown.text=def.cooldown.ToString();
    }
}
