using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>A single modal book, independent of the game's split-style controls.</summary>
public sealed class KaitTutorialBook : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    static readonly Color Cream = new Color32(255, 244, 226, 255);
    static readonly Color Peach = new Color32(250, 199, 183, 255);
    static readonly Color Plum = new Color32(67, 56, 66, 255);
    Font font;
    Sprite rounded;
    Text title, lead, body, leftCaption, rightCaption, tip, counter, nextLabel;
    RawImage comic;
    Button previous, next;
    readonly Image[] tabs = new Image[KaitTutorialPages.All.Length];
    Vector2 dragStart;
    public int PageIndex { get; private set; }
    public int PageCount => KaitTutorialPages.All.Length;
    public bool IllustrationLoaded => comic != null && comic.texture != null;
    public System.Action Completed;
    // Also guards against Escape closing the book before KaitGame.Update runs that frame.
    public static int ClosedFrame { get; private set; } = -1;

    public static KaitTutorialBook Create(Transform parent, Font textFont, Sprite roundedSprite)
    {
        var root = new GameObject("Tutorial Overlay", typeof(RectTransform), typeof(Image), typeof(KaitTutorialBook));
        root.transform.SetParent(parent, false);
        var rt = (RectTransform)root.transform;
        rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero;
        root.GetComponent<Image>().color = new Color(0.08f,0.06f,0.09f,.88f);
        var book=root.GetComponent<KaitTutorialBook>();
        book.font=textFont; book.rounded=roundedSprite; book.Build(); book.ShowPage(0);
        root.SetActive(false);
        return book;
    }

    void Build()
    {
        var card=Box("Tutorial Book",transform,Vector2.zero,new Vector2(1460,920),Plum);
        Label(card.transform,"Kait · 玩法图解",new Vector2(-470,407),new Vector2(430,36),22,Peach);
        counter=Label(card.transform,"",new Vector2(556,406),new Vector2(116,36),22,Peach,TextAnchor.MiddleCenter);
        AddButton(card.transform,"关闭 ×",new Vector2(642,406),new Vector2(110,46),Close);
        title=Label(card.transform,"",new Vector2(-245,340),new Vector2(870,64),38,Cream);
        var art=new GameObject("Two Panel Comic",typeof(RectTransform),typeof(RawImage));
        art.transform.SetParent(card.transform,false);
        var artRect=(RectTransform)art.transform; artRect.sizeDelta=new Vector2(840,560); artRect.anchoredPosition=new Vector2(-265,17);
        comic=art.GetComponent<RawImage>(); comic.raycastTarget=false;
        leftCaption=Label(card.transform,"",new Vector2(-475,-297),new Vector2(410,54),23,Cream,TextAnchor.MiddleCenter);
        rightCaption=Label(card.transform,"",new Vector2(-55,-297),new Vector2(410,54),23,Cream,TextAnchor.MiddleCenter);
        Box("Reading Divider",card.transform,new Vector2(185,10),new Vector2(2,570),new Color(1,1,1,.12f)).raycastTarget=false;
        lead=Label(card.transform,"",new Vector2(453,268),new Vector2(452,80),29,Peach);
        body=Label(card.transform,"",new Vector2(453,15),new Vector2(452,392),25,Cream,TextAnchor.UpperLeft);
        body.lineSpacing=1f;
        var note=Box("Quick Tip",card.transform,new Vector2(453,-255),new Vector2(470,137),new Color32(84,71,81,255));
        tip=Label(note.transform,"",Vector2.zero,new Vector2(428,115),23,Peach);
        previous=AddButton(card.transform,"上一页",new Vector2(-594,-400),new Vector2(176,54),Previous);
        next=AddButton(card.transform,"下一页",new Vector2(594,-400),new Vector2(176,54),Next);
        nextLabel=next.GetComponentInChildren<Text>();
        for(int i=0;i<PageCount;i++)
        {
            int index=i;
            var tab=AddButton(card.transform,(i+1).ToString(),new Vector2((i-(PageCount-1)*.5f)*57,-400),new Vector2(44,44),()=>ShowPage(index));
            tabs[i]=tab.GetComponent<Image>();
        }
        Label(card.transform,"左右滑动 / ← → 翻页 · Esc 关闭",new Vector2(0,-353),new Vector2(750,32),18,new Color32(194,180,190,255),TextAnchor.MiddleCenter);
    }

    public void ShowPage(int index)
    {
        PageIndex=Mathf.Clamp(index,0,PageCount-1);
        if(comic==null)return;
        var p=KaitTutorialPages.All[PageIndex];
        title.text=p.Title; lead.text=p.Lead; SetReadableCopy(body,p.Body);
        leftCaption.text=p.LeftCaption; rightCaption.text=p.RightCaption; SetReadableCopy(tip,p.Tip);
        counter.text=$"{PageIndex+1} / {PageCount}";
        comic.texture=Resources.Load<Texture2D>(p.ResourcePath);
        previous.interactable=PageIndex>0;
        nextLabel.text=PageIndex==PageCount-1 ? "开始游戏" : "下一页";
        for(int i=0;i<tabs.Length;i++)tabs[i].color=i==PageIndex ? new Color32(151,104,99,255) : new Color32(93,79,87,255);
    }
    public void Next() { if(PageIndex==PageCount-1) { Close(); Completed?.Invoke(); } else ShowPage(PageIndex+1); }

    // Legacy UGUI treats long Chinese runs around Latin spaces as words. Pre-wrap by
    // measured glyph width, keeping punctuation off line starts and Latin tokens intact.
    static void SetReadableCopy(Text target,string value)
    {
        var generator=new TextGenerator();
        var settings=target.GetGenerationSettings(new Vector2(10000,10000));
        settings.horizontalOverflow=HorizontalWrapMode.Overflow;
        settings.verticalOverflow=VerticalWrapMode.Overflow;
        float width=target.rectTransform.rect.width-2;
        var result=new System.Text.StringBuilder();
        string line="";
        foreach(System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(value,@"[A-Za-z0-9]+|[^\r]"))
        {
            string token=match.Value;
            if(token=="\n") { result.Append(line.TrimEnd()).Append('\n'); line=""; continue; }
            if(line.Length==0 && token==" ")continue;
            if(line.Length>0 && generator.GetPreferredWidth(line+token,settings)/target.pixelsPerUnit>width)
            {
                string carried="";
                if("，。；：！？、）】》".Contains(token) && line.Length>1)
                { carried=line.Substring(line.Length-1); line=line.Substring(0,line.Length-1); }
                result.Append(line.TrimEnd()).Append('\n'); line=carried;
            }
            line+=token;
        }
        result.Append(line.TrimEnd());
        target.horizontalOverflow=HorizontalWrapMode.Overflow;
        target.text=result.ToString();
    }

    public void Previous() => ShowPage(PageIndex-1);
    public void Close() { ClosedFrame=Time.frameCount; gameObject.SetActive(false); }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))Close();
        else if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.PageDown))ShowPage(PageIndex+1);
        else if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.PageUp))Previous();
    }
    public void OnBeginDrag(PointerEventData e) { dragStart=e.position; }
    public void OnDrag(PointerEventData e) { }
    public void OnEndDrag(PointerEventData e)
    {
        var delta=e.position-dragStart;
        float scale=GetComponentInParent<Canvas>().scaleFactor;
        if(Mathf.Abs(delta.x)<70*scale || Mathf.Abs(delta.x)<Mathf.Abs(delta.y)*1.3f)return;
        ShowPage(PageIndex+(delta.x<0?1:-1));
    }
    Image Box(string name,Transform parent,Vector2 position,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false);
        var rt=(RectTransform)go.transform; rt.sizeDelta=size; rt.anchoredPosition=position;
        var img=go.GetComponent<Image>(); img.color=color; img.sprite=rounded; img.type=Image.Type.Sliced;
        return img;
    }
    Text Label(Transform parent,string value,Vector2 position,Vector2 size,int fontSize,Color color,TextAnchor alignment=TextAnchor.MiddleLeft)
    {
        var go=new GameObject("Tutorial Text",typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
        var rt=(RectTransform)go.transform; rt.sizeDelta=size; rt.anchoredPosition=position;
        var text=go.GetComponent<Text>(); text.font=font; text.fontSize=fontSize; text.color=color; text.text=value;
        text.alignment=alignment; text.raycastTarget=false; text.supportRichText=false;
        text.horizontalOverflow=HorizontalWrapMode.Wrap; text.verticalOverflow=VerticalWrapMode.Truncate;
        return text;
    }
    Button AddButton(Transform parent,string label,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action)
    {
        var img=Box(label,parent,position,size,new Color32(93,79,87,255));
        var button=img.gameObject.AddComponent<Button>(); button.targetGraphic=img;
        button.navigation=new Navigation{mode=Navigation.Mode.None};
        button.onClick.AddListener(()=>{ GameAudio.PlayClick(); action(); });
        Label(img.transform,label,Vector2.zero,size-new Vector2(10,4),22,Cream,TextAnchor.MiddleCenter);
        return button;
    }
}
