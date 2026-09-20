using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>A single modal book, independent of the game's split-style controls.</summary>
public sealed class KaitTutorialBook : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    static readonly Color Cream = KaitStorybookTheme.Ink;
    static readonly Color Peach = KaitStorybookTheme.Ink;
    static readonly Color Plum = KaitStorybookTheme.Paper;
    Font font;
    Sprite rounded;
    Text heading, title, lead, body, leftCaption, rightCaption, tip, counter, nextLabel;
    RawImage comic;
    Image bossNumberLabel;
    YummnTutorialDiagram yummnDiagram;
    Button previous, next;
    Image divider, note;
    Text thirdCaption, appendixLabel, appendixText, navigationHint;
    Button appendixButton;
    ScrollRect appendix;
    bool appendixOpen;
    public bool ReynardMode;
    public bool AppendixOpen => appendixOpen;
    bool ComicMode => !YummnMode || YummnRules != null && YummnRules.Is082;
    readonly Image[] tabs = new Image[KaitTutorialPages.All.Length];
    Vector2 dragStart;
    public int PageIndex { get; private set; }
    private bool yummnMode;
    public YummnRulesSnapshot YummnRules { get; set; }=new YummnRulesSnapshot();
    public bool YummnMode
    {
        get => yummnMode;
        set
        {
            if (yummnMode == value) return;
            yummnMode = value;
            ShowPage(0);
        }
    }
    public int PageCount => ComicMode ? 3 : YummnMode ? YummnTutorial.ForRules(YummnRules).Length : KaitTutorialPages.All.Length;
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
        heading=Label(card.transform,"Kait · 玩法图解",new Vector2(-470,407),new Vector2(430,36),22,Peach);
        counter=Label(card.transform,"",new Vector2(556,406),new Vector2(116,36),22,Peach,TextAnchor.MiddleCenter);
        AddButton(card.transform,"关闭 ×",new Vector2(642,406),new Vector2(110,46),Close);
        appendixButton=AddButton(card.transform,"详细说明",new Vector2(325,406),new Vector2(180,46),()=>ShowAppendix(!appendixOpen));
        appendixLabel=appendixButton.GetComponentInChildren<Text>();
        title=Label(card.transform,"",new Vector2(-245,340),new Vector2(870,64),38,Cream);
        var art=new GameObject("Two Panel Comic",typeof(RectTransform),typeof(RawImage));
        art.transform.SetParent(card.transform,false);
        var artRect=(RectTransform)art.transform; artRect.sizeDelta=new Vector2(840,560); artRect.anchoredPosition=new Vector2(-265,17);
        comic=art.GetComponent<RawImage>(); comic.raycastTarget=false;
        // Editable annotation covers the provisional number baked into the comic.
        bossNumberLabel=Box("Boss Goal Label",comic.transform,new Vector2(294,139),new Vector2(70,46),new Color32(252,222,201,255));
        bossNumberLabel.sprite=null;bossNumberLabel.raycastTarget=false;
        Label(bossNumberLabel.transform,"大数字",Vector2.zero,new Vector2(70,46),21,new Color32(67,40,34,255),TextAnchor.MiddleCenter);
        leftCaption=Label(card.transform,"",new Vector2(-475,-297),new Vector2(410,54),23,Cream,TextAnchor.MiddleCenter);
        rightCaption=Label(card.transform,"",new Vector2(-55,-297),new Vector2(410,54),23,Cream,TextAnchor.MiddleCenter);
        divider=Box("Reading Divider",card.transform,new Vector2(185,10),new Vector2(2,570),new Color(1,1,1,.12f));divider.raycastTarget=false;
        lead=Label(card.transform,"",new Vector2(453,268),new Vector2(452,92),29,Peach);
        body=Label(card.transform,"",new Vector2(453,15),new Vector2(452,392),25,Cream,TextAnchor.UpperLeft);
        body.lineSpacing=1f;
        note=Box("Quick Tip",card.transform,new Vector2(453,-255),new Vector2(470,137),KaitStorybookTheme.Mint);
        tip=Label(note.transform,"",Vector2.zero,new Vector2(428,115),23,Peach);
        thirdCaption=Label(card.transform,"",new Vector2(448,-274),new Vector2(416,120),24,Cream,TextAnchor.UpperLeft);
        BuildAppendix(card.transform);
        previous=AddButton(card.transform,"上一页",new Vector2(-594,-400),new Vector2(176,54),Previous);
        next=AddButton(card.transform,"下一页",new Vector2(594,-400),new Vector2(176,54),Next);
        nextLabel=next.GetComponentInChildren<Text>();
        for(int i=0;i<tabs.Length;i++)
        {
            int index=i;
            var tab=AddButton(card.transform,(i+1).ToString(),new Vector2((i-(PageCount-1)*.5f)*63,-400),new Vector2(56,54),()=>ShowPage(index));
            tabs[i]=tab.GetComponent<Image>();
        }
        navigationHint=Label(card.transform,"左右滑动 / ← → 翻页 · Esc 关闭",new Vector2(0,-353),new Vector2(750,32),18,KaitStorybookTheme.Muted,TextAnchor.MiddleCenter);
    }

    public void ShowPage(int index)
    {
        PageIndex=Mathf.Clamp(index,0,PageCount-1);
        if(comic==null)return;
        if(ReynardMode){ShowReynardTutorial();return;}
        heading.text=YummnMode ? "Yummn · 玩法图解" : "Kait · 玩法图解";
        appendixOpen=false;appendix.gameObject.SetActive(false);
        SetComicLayout(ComicMode);
        var p=!YummnMode ? KaitComicTutorial.Pages[PageIndex] : ComicMode ? YummnComicTutorial.ForRules(YummnRules)[PageIndex] : YummnTutorial.ForRules(YummnRules)[PageIndex];
        title.text=p.Title; SetReadableCopy(lead,p.Lead); SetReadableCopy(body,p.Body);
        leftCaption.text=p.LeftCaption; rightCaption.text=p.RightCaption; SetReadableCopy(tip,p.Tip);
        if(ComicMode){SetReadableCopy(leftCaption,p.LeftCaption);SetReadableCopy(rightCaption,p.RightCaption);SetReadableCopy(thirdCaption,p.Body);}
        counter.text=$"{PageIndex+1} / {PageCount}";
        comic.texture=Resources.Load<Texture2D>(p.ResourcePath);comic.enabled=!YummnMode||ComicMode;
        bossNumberLabel.gameObject.SetActive(YummnMode&&ComicMode&&PageIndex==2);
        if(ComicMode && comic.texture!=null)comic.rectTransform.sizeDelta=new Vector2(1340,1340f*comic.texture.height/comic.texture.width);
        if(YummnMode&&!ComicMode&&yummnDiagram==null)
        {
            var go=new GameObject("Yummn Rule Diagram",typeof(RectTransform),typeof(YummnTutorialDiagram));go.transform.SetParent(comic.transform.parent,false);
            var rt=go.GetComponent<RectTransform>();rt.sizeDelta=comic.rectTransform.sizeDelta;rt.anchoredPosition=comic.rectTransform.anchoredPosition;yummnDiagram=go.GetComponent<YummnTutorialDiagram>();
        }
        if(yummnDiagram!=null){yummnDiagram.gameObject.SetActive(YummnMode&&!ComicMode);if(YummnMode&&!ComicMode)yummnDiagram.Show(PageIndex,font,YummnRules!=null&&YummnRules.Legacy,YummnRules?.MaxKi??6,YummnRules);}
        previous.interactable=PageIndex>0;
        nextLabel.text=PageIndex==PageCount-1 ? "开始游戏" : "下一页";
        for(int i=0;i<tabs.Length;i++){tabs[i].gameObject.SetActive(i<PageCount);tabs[i].rectTransform.anchoredPosition=new Vector2((i-(PageCount-1)*.5f)*63,-400);tabs[i].color=i==PageIndex ? KaitStorybookTheme.Mint : Color.white;}
    }
    private void ShowReynardTutorial()
    {
        SetComicLayout(false);heading.text="Reynard · 方向编咒";title.text="方向施咒，等待召狐";
        comic.gameObject.SetActive(false);leftCaption.text=rightCaption.text=thirdCaption.text="";
        body.gameObject.SetActive(true);body.rectTransform.anchoredPosition=new Vector2(-235,0);body.rectTransform.sizeDelta=new Vector2(820,510);body.fontSize=28;
        SetReadableCopy(body,"每次方向操作尝试移动一格，同时滑动右盘。\n\n无论是否成功移动，Reynard都会朝输入方向发射一发子弹。子弹命中前方第一个敌人并造成1点伤害；受阻时人物留在原地。\n\n等待：把镜狐残影召到脚下对应的右盘数字。没有数字则保留原位置。\n\n镜狐数字不随滑动移动。同值数字可以撞入合并。");
        SetReadableCopy(lead,"2＝I环 · 4＝II环 · 8＝III环\n高环可施放低环法术。");
        SetReadableCopy(tip,"点卡牌，再点目标施放。法术耗竭节点；合并使节点恢复。六槽自由混装，同时只维持一个持续法术。方向、等待、施法均补一个2并推进敌人行动。");
        counter.text="核心规则";previous.interactable=false;nextLabel.text="关闭";PageIndex=PageCount-1;
        appendix.gameObject.SetActive(false);if(yummnDiagram!=null)yummnDiagram.gameObject.SetActive(false);bossNumberLabel.gameObject.SetActive(false);
    }
    public void Next() { if(appendixOpen){ShowAppendix(false);return;} if(PageIndex==PageCount-1) { Close(); Completed?.Invoke(); } else ShowPage(PageIndex+1); }

    void SetComicLayout(bool enabled)
    {
        appendixButton.gameObject.SetActive(enabled);appendixLabel.text="详细说明";
        navigationHint.text="左右滑动 / ← → 翻页 · Esc 关闭";
        divider.gameObject.SetActive(!enabled);note.gameObject.SetActive(!enabled);body.gameObject.SetActive(!enabled);
        thirdCaption.gameObject.SetActive(enabled);
        comic.gameObject.SetActive(true);lead.gameObject.SetActive(true);leftCaption.gameObject.SetActive(true);rightCaption.gameObject.SetActive(true);
        title.rectTransform.anchoredPosition=enabled?new Vector2(0,335):new Vector2(-245,340);
        title.rectTransform.sizeDelta=enabled?new Vector2(1340,64):new Vector2(870,64);
        title.alignment=enabled?TextAnchor.MiddleCenter:TextAnchor.MiddleLeft;
        lead.rectTransform.anchoredPosition=enabled?new Vector2(0,276):new Vector2(453,268);
        lead.rectTransform.sizeDelta=enabled?new Vector2(1340,50):new Vector2(452,92);
        lead.fontSize=enabled?26:29;lead.alignment=enabled?TextAnchor.MiddleCenter:TextAnchor.MiddleLeft;
        comic.rectTransform.anchoredPosition=enabled?new Vector2(0,12):new Vector2(-265,17);
        comic.rectTransform.sizeDelta=enabled?new Vector2(1340,447):new Vector2(840,560);
        leftCaption.rectTransform.anchoredPosition=enabled?new Vector2(-448,-274):new Vector2(-475,-297);
        rightCaption.rectTransform.anchoredPosition=enabled?new Vector2(0,-274):new Vector2(-55,-297);
        foreach(var caption in new[]{leftCaption,rightCaption})
        {
            caption.rectTransform.sizeDelta=enabled?new Vector2(416,120):new Vector2(410,54);
            caption.fontSize=enabled?24:23;caption.alignment=enabled?TextAnchor.UpperLeft:TextAnchor.MiddleCenter;
        }
        next.interactable=true;
    }

    void BuildAppendix(Transform parent)
    {
        var view=Box("Yummn Text Appendix",parent,new Vector2(0,-15),new Vector2(1340,660),new Color32(54,47,56,255));
        appendix=view.gameObject.AddComponent<ScrollRect>();appendix.horizontal=false;appendix.movementType=ScrollRect.MovementType.Clamped;
        var viewport=Box("Viewport",view.transform,Vector2.zero,new Vector2(1280,620),Color.clear);
        viewport.gameObject.AddComponent<RectMask2D>();appendix.viewport=viewport.rectTransform;
        // The appendix sits on a dark reading surface, unlike the paper-backed comic UI.
        appendixText=Label(viewport.transform,"",Vector2.zero,new Vector2(1250,620),30,KaitStorybookTheme.Paper,TextAnchor.UpperLeft);
        appendixText.lineSpacing=1.08f;
        var content=appendixText.rectTransform;content.anchorMin=content.anchorMax=new Vector2(.5f,1);content.pivot=new Vector2(.5f,1);
        appendix.content=content;appendix.scrollSensitivity=45;
        var track=Box("Scroll Track",view.transform,new Vector2(653,0),new Vector2(12,620),new Color(1,1,1,.08f));
        var handle=Box("Scroll Handle",track.transform,Vector2.zero,new Vector2(12,80),KaitStorybookTheme.Peach);
        handle.rectTransform.anchorMin=Vector2.zero;handle.rectTransform.anchorMax=Vector2.one;
        handle.rectTransform.offsetMin=handle.rectTransform.offsetMax=Vector2.zero;
        var bar=track.gameObject.AddComponent<Scrollbar>();bar.direction=Scrollbar.Direction.BottomToTop;bar.handleRect=handle.rectTransform;bar.targetGraphic=handle;
        appendix.verticalScrollbar=bar;view.gameObject.SetActive(false);
    }

    public void ShowAppendix(bool show)
    {
        if(!ComicMode)return;
        if(!show){ShowPage(PageIndex);return;}
        appendixOpen=true;appendixLabel.text="返回漫画";appendix.gameObject.SetActive(true);
        title.text=YummnMode?"详细说明 · 按本局设置显示":"详细说明 · Kait";
        foreach(var g in new GameObject[]{comic.gameObject,lead.gameObject,leftCaption.gameObject,rightCaption.gameObject,thirdCaption.gameObject})g.SetActive(false);
        SetReadableCopy(appendixText,YummnMode?YummnComicTutorial.Appendix(YummnRules):KaitComicTutorial.Details);
        var settings=appendixText.GetGenerationSettings(new Vector2(appendixText.rectTransform.rect.width,10000));
        settings.verticalOverflow=VerticalWrapMode.Overflow;
        float height=new TextGenerator().GetPreferredHeight(appendixText.text,settings)/appendixText.pixelsPerUnit+30;
        appendixText.rectTransform.sizeDelta=new Vector2(1250,Mathf.Max(620,height));
        Canvas.ForceUpdateCanvases();appendix.verticalNormalizedPosition=1;
        previous.interactable=false;next.interactable=true;nextLabel.text="返回漫画";
        navigationHint.text="上下滑动 / 滚轮阅读 · Esc 返回漫画";
    }

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
    void OnEnable() { if (comic != null) ShowPage(PageIndex); }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){if(appendixOpen)ShowAppendix(false);else Close();}
        else if(appendixOpen)return;
        else if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.PageDown))ShowPage(PageIndex+1);
        else if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.PageUp))Previous();
    }
    public void OnBeginDrag(PointerEventData e) { dragStart=e.position; }
    public void OnDrag(PointerEventData e) { }
    public void OnEndDrag(PointerEventData e)
    {
        if(appendixOpen)return;
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
        if(name=="Tutorial Book")KaitStorybookTheme.PaperPanel(img);
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
        var img=Box(label,parent,position,size,Color.white);img.sprite=KaitStorybookTheme.Button;
        var button=img.gameObject.AddComponent<Button>(); button.targetGraphic=img;
        button.navigation=new Navigation{mode=Navigation.Mode.None};
        button.onClick.AddListener(()=>{ GameAudio.PlayClick(); action(); });
        Label(img.transform,label,Vector2.zero,size-new Vector2(10,4),22,Cream,TextAnchor.MiddleCenter);
        return button;
    }
}
