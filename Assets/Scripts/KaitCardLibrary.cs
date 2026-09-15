using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitCardLibrary : MonoBehaviour
{
    public KaitCharacter Character {get;private set;}
    public int Rarity {get;private set;}=-1;
    public int Page {get;private set;}
    public int Total {get;private set;}
    const int PageSize=6;
    RectTransform layout,body;
    Font font;
    Text heading,pageLabel;
    Button previous,next;
    readonly List<Button> filters=new List<Button>();
    static readonly Color Ink=KaitStorybookTheme.Ink,Cream=KaitStorybookTheme.Paper;
    public static KaitCardLibrary Open(Transform parent,Font font,KaitCharacter character)
    {
        var go=new GameObject("Card Library",typeof(RectTransform),typeof(Image),typeof(KaitCardLibrary));
        go.transform.SetParent(parent,false);var rt=(RectTransform)go.transform;
        rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.sizeDelta=Vector2.zero;
        go.GetComponent<Image>().color=Cream;
        var view=go.GetComponent<KaitCardLibrary>();view.font=font;
        view.layout=view.Rect("Library Layout",rt,Vector2.zero,new Vector2(1920,1080));
        var backdrop=KaitStorybookArt.Icon(view.layout,"LibraryBackdrop",Vector2.zero,new Vector2(1920,1080));backdrop.preserveAspect=false;
        view.heading=view.Label(view.layout,"卡牌大全",new Vector2(-660,436),new Vector2(320,60),38,Ink);
        view.Button(view.layout,"×",new Vector2(870,455),new Vector2(58,56),()=>Destroy(go));
        foreach(var c in new[]{KaitCharacter.Kait,KaitCharacter.Yummn})
        {
            var selected=c;var button=view.Button(view.layout,c.ToString(),new Vector2(-680,c==KaitCharacter.Kait?294:188),new Vector2(256,88),()=>view.Select(selected,view.Rarity));
            var label=button.GetComponentInChildren<Text>();label.rectTransform.anchoredPosition=new Vector2(33,0);label.rectTransform.sizeDelta=new Vector2(138,50);
            KaitStorybookDetails.PortraitBadge(button.transform,c,new Vector2(-74,0),66);view.filters.Add(button);
        }
        for(int i=-1;i<3;i++)
        {int rarity=i;view.filters.Add(view.Button(view.layout,i<0?"全部":KaitAbilityCatalog.RarityName((KaitRarity)i),new Vector2(-260+(i+1)*242,436),new Vector2(204,54),()=>view.Select(view.Character,rarity)));}
        view.body=view.Rect("Cards",view.layout,new Vector2(155,-15),new Vector2(1270,776));
        view.previous=view.Button(view.layout,"上一页",new Vector2(-140,-467),new Vector2(190,56),()=>view.ChangePage(-1));
        view.pageLabel=view.Label(view.layout,"",new Vector2(155,-467),new Vector2(360,50),23,Ink);
        view.next=view.Button(view.layout,"下一页",new Vector2(450,-467),new Vector2(190,56),()=>view.ChangePage(1));
        view.Select(character,-1);view.Fit();return view;
    }
    public void Select(KaitCharacter character,int rarity){Character=character;Rarity=rarity;Page=0;Refresh();}
    public void ChangePage(int delta){Page=Mathf.Clamp(Page+delta,0,Mathf.Max(0,(Total-1)/PageSize));Refresh();}
    public static List<KaitAbilityDef> Cards(KaitCharacter c,int rarity)
    {
        var list=new List<KaitAbilityDef>(c==KaitCharacter.Yummn?YummnCatalog.Cards:KaitAbilityCatalog.All);
        if(rarity>=0)list.RemoveAll(d=>(int)d.rarity!=rarity);
        list.Sort((a,b)=>{int rank=a.rarity.CompareTo(b.rarity);if(rank!=0)return rank;rank=a.kind.CompareTo(b.kind);return rank!=0?rank:System.StringComparer.Ordinal.Compare(a.id,b.id);});
        return list;
    }
    void Refresh()
    {
        foreach(Transform child in body){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        var list=Cards(Character,Rarity);Total=list.Count;
        heading.text="卡牌大全";
        pageLabel.text=$"{Page+1} / {Mathf.Max(1,(Total+PageSize-1)/PageSize)}　·　{Total} 张";
        previous.interactable=Page>0;next.interactable=(Page+1)*PageSize<Total;
        for(int i=0;i<filters.Count;i++)
        {
            bool selected=i<2?i==(int)Character:i-3==Rarity;
            var image=filters[i].GetComponent<Image>();image.color=Color.white;
            image.sprite=selected?KaitStorybookTheme.Surface("library-selected",KaitStorybookTheme.Mint,Ink,5):KaitStorybookTheme.Button;
        }
        for(int i=Page*PageSize;i<Mathf.Min(list.Count,(Page+1)*PageSize);i++)DrawCard(list[i],i%PageSize);
    }
    void DrawCard(KaitAbilityDef def,int index)
    {
        var card=Rect(def.id,body,new Vector2((index%3-1)*380,index<3?194:-194),new Vector2(354,362));
        var face=card.gameObject.AddComponent<Image>();face.sprite=KaitStorybookTheme.Card(def,false,true);face.color=Color.white;
        if(face.sprite==null){face.sprite=KaitCardSkin.RoundRect();face.color=new Color32(251,240,220,255);}
        face.raycastTarget=false;
        bool active=def.kind==KaitAbilityKind.Active;
        Label(card,def.nameZh,new Vector2(0,132),new Vector2(294,36),28,Ink);
        var art=Rect("Illustration",card,new Vector2(0,34),Vector2.one*154).gameObject.AddComponent<Image>();
        art.sprite=KaitCardArt.Fit(KaitCardSkin.Icon(def));art.preserveAspect=true;art.raycastTarget=false;
        if(art.sprite==null)art.color=Color.clear;
        var description=Label(card,def.cardText.TrimEnd('。'),new Vector2(0,-86),new Vector2(282,82),24,Ink);
        description.resizeTextMinSize=20;
        string cost=def.experimental?"实验牌 · 未启用":active?(YummnCatalog.IsMonk(def)?"耗气 "+def.kiExtraCost:"冷却 "+def.cooldown+" 回合"):"";
        if(active&&YummnCatalog.IsMonk(def)&&!def.experimental)
        {KaitStorybookArt.Icon(card,"Qi",new Vector2(-16,-144),new Vector2(16,22));Label(card,def.kiExtraCost.ToString(),new Vector2(12,-144),new Vector2(40,26),21,Ink);}
        else Label(card,cost,new Vector2(0,-144),new Vector2(264,24),18,KaitStorybookTheme.Muted);
    }
    void Fit(){var size=((RectTransform)transform).rect.size;layout.localScale=Vector3.one*Mathf.Min(size.x/1920,size.y/1080);}
    void Update(){Fit();if(Input.GetKeyDown(KeyCode.Escape)){Destroy(gameObject);return;}if(Input.mouseScrollDelta.y>0)ChangePage(-1);else if(Input.mouseScrollDelta.y<0)ChangePage(1);}
    RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size){var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);rt.sizeDelta=size;rt.anchoredPosition=pos;return rt;}
    Text Label(Transform parent,string text,Vector2 pos,Vector2 size,int fontSize,Color color)
    {
        var t=Rect("Text",parent,pos,size).gameObject.AddComponent<Text>();t.font=font;t.text=text;t.fontSize=fontSize;t.color=color;t.alignment=TextAnchor.MiddleCenter;
        t.resizeTextForBestFit=true;t.resizeTextMinSize=16;t.resizeTextMaxSize=fontSize;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.raycastTarget=false;return t;
    }
    Button Button(Transform parent,string name,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action)
    {
        var rt=Rect(name,parent,pos,size);var image=rt.gameObject.AddComponent<Image>();image.sprite=KaitStorybookTheme.Button;image.type=Image.Type.Sliced;image.color=Color.white;
        var b=rt.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);Label(rt,name,Vector2.zero,size-new Vector2(24,12),24,Ink);return b;
    }
}
