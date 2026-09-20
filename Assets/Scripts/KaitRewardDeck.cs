using System;
using UnityEngine;
using UnityEngine.UI;

// Cards own the pointer; visual drop targets never consume a board click.
public sealed class KaitRewardDeck : MonoBehaviour
{
    private readonly KaitSkillCard[] active=new KaitSkillCard[3];
    private readonly KaitPassiveCard[] passive=new KaitPassiveCard[3];
    private readonly Image[] slots=new Image[6];
    private readonly KaitCardOutline[] slotOutlines=new KaitCardOutline[6];
    private readonly Text[] slotNames=new Text[6],slotHints=new Text[6];
    private readonly KaitCardLogo[] slotIcons=new KaitCardLogo[6];
    private readonly bool[] validSlots=new bool[6];
    private RectTransform area,bar;
    private RectTransform tray;
    private CanvasGroup trayFade;
    private float trayReveal;
    private GlobalStyleSplit styleSplit;
    private Button skip,cancel,reroll,fold;
    private Text heading,guide;
    private Image guideBackdrop;
    private readonly KaitUiGlyph[] dropGlyphs=new KaitUiGlyph[6];
    private KaitRun run;
    private KaitRewardPack shown;
    private Func<bool> canInteract;
    private Action changed;
    private int selected=-1,dragging=-1,hoverSlot=-1,dragPackId;
    private KaitPassive copy=KaitPassive.None;
    private bool collapsed;
    private string notice;
    private float noticeUntil;
    private GameObject replacementPrompt;
    private Font uiFont;
    public bool IsDragging=>dragging>=0;
    // While the reward tray owns a pointer/focus, gameplay must not also consume
    // direction input. This applies to every character, not only Yummn.
    public bool CapturesGameplayInput=>dragging>=0||selected>=0||replacementPrompt!=null;
    public Action SelectionStarted;
    public RectTransform[] OwnedAreas;

    public void Initialize(RectTransform parent,GlobalStyleSplit split,Font font,Func<bool> allowed,Action refresh)
    {
        area=parent;styleSplit=split;canInteract=allowed;changed=refresh;uiFont=font;
        bar=new GameObject("Reward Actions",typeof(RectTransform),typeof(HybridStyleGraphic),typeof(KaitUnifiedPaper)).GetComponent<RectTransform>();bar.SetParent(parent,false);
        bar.sizeDelta=new Vector2(800,76);
        // Above the offer cards (their top is 221), below the two passive rows.
        bar.anchoredPosition=new Vector2(0,268);
        var barPlate=bar.GetComponent<HybridStyleGraphic>();
        barPlate.Configure(split,KaitStorybookTheme.Panel,Color.white,new Color(.20f,.17f,.23f,1),new Color(.98f,.78f,.72f),2,10);barPlate.raycastTarget=false;
        var handle=new GameObject("Drag Handle",typeof(RectTransform),typeof(Image),typeof(KaitRewardBarDrag));handle.transform.SetParent(bar,false);
        var hit=handle.GetComponent<Image>();hit.color=Color.clear;hit.rectTransform.sizeDelta=new Vector2(172,64);hit.rectTransform.anchoredPosition=new Vector2(-304,0);hit.raycastTarget=true;
        handle.GetComponent<KaitRewardBarDrag>().Configure(bar,area);
        KaitUiGlyph.Create(bar,KaitUiGlyph.Symbol.Card,new Vector2(-356,0),28).gameObject.AddComponent<SunlitSplitText>().Configure(split);
        heading=Label(bar,font,"Reward Count",new Vector2(-298,0),new Vector2(64,48),24);
        heading.gameObject.AddComponent<SunlitSplitText>().Configure(split);
        skip=ButtonAt(bar,font,"跳过",new Vector2(-154,0),()=>{if(!IsDragging&&Allowed()&&run.SkipReward())Complete();});
        reroll=ButtonAt(bar,font,"重抽",new Vector2(2,0),()=>
        {if(!IsDragging&&Allowed()&&run.RerollReward()){ResetSelection();shown=null;changed?.Invoke();Sync(run);}});
        cancel=ButtonAt(bar,font,"取消",new Vector2(158,0),()=>{if(!IsDragging){ResetSelection();Sync(run);}});
        fold=ButtonAt(bar,font,"收起",new Vector2(314,0),()=>{if(!IsDragging){collapsed=!collapsed;ResetSelection();shown=null;Sync(run);}});
        guideBackdrop=new GameObject("Reward Guide Background",typeof(RectTransform),typeof(Image)).GetComponent<Image>();
        guideBackdrop.transform.SetParent(parent,false);guideBackdrop.rectTransform.sizeDelta=new Vector2(320,54);guideBackdrop.rectTransform.anchoredPosition=new Vector2(0,-120);
        guideBackdrop.sprite=KaitCardSkin.RoundRect();guideBackdrop.type=Image.Type.Sliced;
        guideBackdrop.color=new Color(.20f,.17f,.23f,.96f);guideBackdrop.raycastTarget=false;
        guide=Label(guideBackdrop.rectTransform,font,"Reward Gesture Guide",Vector2.zero,new Vector2(306,48),22);
        var trayImage=new GameObject("Equipment Card Tray",typeof(RectTransform),typeof(Image),typeof(CanvasGroup),typeof(KaitUnifiedPaper)).GetComponent<Image>();
        trayImage.transform.SetParent(parent,false);tray=trayImage.rectTransform;
        trayImage.sprite=KaitStorybookTheme.Surface("equipment-tray",KaitStorybookTheme.Paper,KaitStorybookTheme.Ink,4,18);
        trayImage.type=Image.Type.Sliced;trayImage.raycastTarget=false;trayFade=tray.GetComponent<CanvasGroup>();
        tray.gameObject.SetActive(false);
        for(int i=0;i<6;i++)
        {
            int index=i;
            var target=new GameObject("Reward Drop Slot "+(i+1),typeof(RectTransform),typeof(Image)).GetComponent<Image>();
            target.transform.SetParent(tray,false);target.rectTransform.sizeDelta=new Vector2(164,164);
            target.sprite=KaitStorybookTheme.Button;target.type=Image.Type.Sliced;target.raycastTarget=false;
            slots[i]=target;
            var edge=new GameObject("Drop Outline",typeof(RectTransform),typeof(KaitCardOutline)).GetComponent<KaitCardOutline>();edge.transform.SetParent(target.transform,false);
            edge.rectTransform.anchorMin=Vector2.zero;edge.rectTransform.anchorMax=Vector2.one;edge.rectTransform.sizeDelta=Vector2.zero;edge.raycastTarget=false;slotOutlines[i]=edge;
            slotNames[i]=Label(target.rectTransform,font,"Slot Name",new Vector2(0,-27),new Vector2(144,38),20);
            slotNames[i].color=KaitStorybookTheme.Ink;
            slotHints[i]=Label(target.rectTransform,font,"Drop Action",new Vector2(0,-48),new Vector2(216,58),20);
            dropGlyphs[i]=KaitUiGlyph.Create(target.transform,KaitUiGlyph.Symbol.Plus,new Vector2(0,-59),22);
            dropGlyphs[i].color=KaitStorybookTheme.Muted;
            slotIcons[i]=KaitCardLogo.Create(target.transform,split,font,new Vector2(0,28),72);
            slotIcons[i].SetIllustrationSize(72);
            if(i<3){
            active[i]=KaitSkillCard.Create(parent,split,font,KaitSunlitTheme.Load("SkillCardHD"),KaitSunlitTheme.Load("SkillCardFlat"),c=>Select(index),null,null);
            passive[i]=KaitPassiveCard.Create(parent,split,font,KaitSunlitTheme.Load("PassiveCardBlankHD"),KaitSunlitTheme.Load("PassiveCardFlat"),c=>Select(index),null);
            active[i].ConfigureRewardDrag(()=>BeginDrag(index),p=>MoveDrag(index,p),p=>EndDrag(index,p));
            passive[i].ConfigureRewardDrag(()=>BeginDrag(index),p=>MoveDrag(index,p),p=>EndDrag(index,p));
            }
            target.gameObject.SetActive(false);
        }
        bar.gameObject.SetActive(false);guideBackdrop.gameObject.SetActive(false);
    }
    private bool Allowed()=>run!=null&&run.CanSelectReward&&(canInteract?.Invoke()??true);
    private void ResetSelection()
    {selected=dragging=hoverSlot=-1;copy=KaitPassive.None;notice=null;if(replacementPrompt!=null){if(Application.isPlaying)Destroy(replacementPrompt);else DestroyImmediate(replacementPrompt);}replacementPrompt=null;}
    private void Notify(string value){notice=value;noticeUntil=Time.unscaledTime+2.5f;}
    public void Sync(KaitRun current)
    {
        if(current!=run){ResetSelection();shown=null;collapsed=false;}
        run=current;
        // A reward may be queued before the current presentation/chain has
        // finished. Keep it queued, but do not put unresponsive cards over the
        // board until it is actually legal to choose them.
        var pack=Allowed()?run.CurrentReward:null;
        if(pack!=shown)
        {
            ResetSelection();shown=pack;
            for(int i=0;i<3;i++){active[i].Hide();passive[i].Hide();slots[i].gameObject.SetActive(false);}
            if(pack!=null&&!collapsed)for(int i=0;i<pack.choices.Count&&i<3;i++)
            {
                Vector2 pos=new Vector2((i-1)*258,70);var d=pack.choices[i];
                if(d.kind==KaitAbilityKind.Active)active[i].Show(d.skill,true,pos,0);else {passive[i].Show(d.passive,true,pos,0);passive[i].ApplyDefinition(d);}
            }
        }
        bar.gameObject.SetActive(pack!=null);
        bool expanded=pack!=null&&!collapsed;guideBackdrop.gameObject.SetActive(expanded);
        if(pack==null){foreach(var c in active)c.Hide();foreach(var c in passive)c.Hide();foreach(var s in slots)s.gameObject.SetActive(false);return;}
        heading.text=$"×{run.rewardQueue.Count}";
        for(int i=0;i<pack.choices.Count&&i<3;i++)
        {string missing=run.IsYummn?run.YummnMissingRequirement(pack.choices[i]):null;active[i].SetRequirement(missing);passive[i].SetRequirement(missing);}
        skip.interactable=!IsDragging&&Allowed();reroll.interactable=!IsDragging&&Allowed()&&run.HasPassive(KaitPassive.LuckBlade)&&!pack.rerolled;
        cancel.interactable=!IsDragging&&selected>=0;fold.interactable=!IsDragging;
        fold.GetComponentInChildren<Text>().text=collapsed?"展开":"收起";
        fold.GetComponentInChildren<KaitUiGlyph>().SetSymbol(collapsed?KaitUiGlyph.Symbol.Down:KaitUiGlyph.Symbol.Up);
        if(expanded&&selected>=0)ShowSlots();else foreach(var s in slots)s.gameObject.SetActive(false);
        guide.text=!Allowed()?"结算后选牌":
            Time.unscaledTime<noticeUntil&&!string.IsNullOrEmpty(notice)?notice:
            IsDragging&&hoverSlot>=0?(NeedsCopy?"复制":hoverSlot<run.EquippedCardCount?"替换":"装备")+" · "+slotNames[hoverSlot].text:
            NeedsCopy?"① 选择要复制的被动":
            copy!=KaitPassive.None?"② 拖入装备槽":"";
        guideBackdrop.gameObject.SetActive(expanded&&!string.IsNullOrEmpty(guide.text));
    }
    private KaitAbilityDef Definition=>selected>=0&&run?.CurrentReward!=null&&selected<run.CurrentReward.choices.Count?run.CurrentReward.choices[selected]:null;
    private bool NeedsCopy=>Definition?.passive==KaitPassive.Simulacrum&&copy==KaitPassive.None;
    private void Select(int index)
    {
        if(IsDragging||replacementPrompt!=null||!Allowed()||index<0||index>=run.CurrentReward.choices.Count)return;
        if(!run.YummnPrerequisite(run.CurrentReward.choices[index])){Notify(run.YummnMissingRequirement(run.CurrentReward.choices[index]));return;}
        SelectionStarted?.Invoke();
        if(selected!=index)copy=KaitPassive.None;
        selected=index;hoverSlot=-1;notice=null;Sync(run);
    }
    private bool BeginDrag(int index)
    {
        if(IsDragging||replacementPrompt!=null||!Allowed())return false;
        Select(index);if(selected!=index||!run.YummnPrerequisite(Definition))return false;
        dragging=index;dragPackId=run.CurrentReward.id;hoverSlot=-1;ShowSlots();return true;
    }
    private void ShowSlots()
    {
        var def=Definition;if(def==null)return;
        bool copying=NeedsCopy,isPassive=def.kind==KaitAbilityKind.Passive;
        int count=run.EquippedCardCount,visible=copying?count:Mathf.Min(6,count+1);
        for(int i=0;i<slots.Length;i++)
        {
            bool show=i<visible;slots[i].gameObject.SetActive(show);validSlots[i]=false;if(!show)continue;
            slots[i].transform.SetAsLastSibling();
            slots[i].rectTransform.sizeDelta=new Vector2(164,164);
            slots[i].rectTransform.anchoredPosition=new Vector2((i-(visible-1)*.5f)*176,0);
            bool filled=i<count;var equipped=filled?run.EquippedCard(i):null;
            // Old saves can contain an enum whose definition changed type or
            // no longer exists. Treat that slot as empty instead of throwing
            // halfway through opening the reward tray.
            filled=filled&&equipped!=null;
            validSlots[i]=Allowed()&&run.YummnPrerequisite(def)&&(!copying||filled&&equipped.kind==KaitAbilityKind.Passive&&equipped.copyable);
            bool hovered=validSlots[i]&&hoverSlot==i;
            slots[i].sprite=hovered?KaitStorybookTheme.Pressed:KaitStorybookTheme.Button;
            slots[i].color=validSlots[i]?Color.white:new Color(.75f,.75f,.75f,1);
            slotOutlines[i].color=Color.clear;
            slots[i].transform.localScale=Vector3.one*(hovered?1.035f:1f);
            slotNames[i].text=filled?equipped.nameZh:"";
            slotHints[i].text="";
            dropGlyphs[i].SetSymbol(!validSlots[i]?KaitUiGlyph.Symbol.Close:hovered?KaitUiGlyph.Symbol.Check:filled?KaitUiGlyph.Symbol.Swap:KaitUiGlyph.Symbol.Plus);
            slotHints[i].color=hovered?Color.white:new Color(.60f,1,.84f);
            slotIcons[i].gameObject.SetActive(filled);
            if(filled){if(equipped.kind==KaitAbilityKind.Active)slotIcons[i].Show(equipped.skill);else slotIcons[i].Show(equipped.passive);}
        }
        tray.sizeDelta=new Vector2(visible*176+24,204);
    }
    private int HitSlot(Vector2 point)
    {
        for(int i=0;i<slots.Length;i++)if(validSlots[i]&&slots[i].gameObject.activeSelf&&
            slots[i].rectTransform.rect.Contains((Vector2)slots[i].rectTransform.InverseTransformPoint(area.TransformPoint(point))))return i;
        return -1;
    }
    private void MoveDrag(int index,Vector2 point){if(dragging==index){hoverSlot=HitSlot(point);ShowSlots();}}
    private void EndDrag(int index,Vector2 point)
    {
        if(dragging!=index)return;dragging=-1;
        if(!Allowed()||run.CurrentReward.id!=dragPackId||Definition==null)
        {hoverSlot=-1;Notify("暂不可选 · 已归位");Sync(run);return;}
        ShowSlots();int slot=HitSlot(point);hoverSlot=-1;
        if(slot<0){Notify("已归位");Sync(run);return;}
        if(NeedsCopy){copy=run.EquippedCard(slot).passive;GameAudio.PlayCardSnap();Sync(run);return;}
        int count=run.EquippedCardCount;
        string consequence=run.YummnReplacementConsequences(selected,slot<count?slot:-1);
        if(consequence!=null){ConfirmReplacement(slot,consequence);return;}
        if(run.SelectReward(selected,slot<count?slot:-1,copy))Complete();else{Notify("暂不可装备 · 已归位");Sync(run);}
    }
    private void ConfirmReplacement(int slot,string message)
    {
        replacementPrompt=new GameObject("Confirm Prerequisite Replacement",typeof(RectTransform),typeof(Image));replacementPrompt.transform.SetParent(area,false);
        var plate=replacementPrompt.GetComponent<Image>();plate.sprite=KaitCardSkin.RoundRect();plate.type=Image.Type.Sliced;plate.color=new Color(.19f,.17f,.23f,.99f);
        var r=plate.rectTransform;r.anchoredPosition=new Vector2(0,-170);r.sizeDelta=new Vector2(580,172);
        var label=Label(r,uiFont,"Affected Cards",new Vector2(0,36),new Vector2(548,72),23);label.text=message+"\n被动保留，但暂停触发。";
        int choice=selected,packId=run.CurrentReward.id;
        ButtonAt(r,uiFont,"确认",new Vector2(-90,-43),()=>
        {if(Allowed()&&run.CurrentReward.id==packId&&run.SelectReward(choice,slot,copy))Complete();else{ResetSelection();Sync(run);}});
        ButtonAt(r,uiFont,"取消",new Vector2(90,-43),()=>{ResetSelection();Sync(run);});
    }
    private void Complete(){ResetSelection();shown=null;GameAudio.PlayPassiveConfirm();changed?.Invoke();Sync(run);}
    private void Update()
    {
        if(run!=null)Sync(run);
        bool open=Allowed()&&!collapsed&&selected>=0;
        trayReveal=Mathf.MoveTowards(trayReveal,open?1:0,Time.unscaledDeltaTime/0.24f);
        tray.gameObject.SetActive(trayReveal>0);
        float eased=1-Mathf.Pow(1-trayReveal,3);
        trayFade.alpha=eased;
        tray.anchoredPosition=new Vector2(0,area.rect.yMin+136-36*(1-eased));
        tray.localScale=Vector3.one*Mathf.Lerp(.94f,1,eased);
        if(OwnedAreas!=null)foreach(var owned in OwnedAreas)
        {
            var group=owned.GetComponent<CanvasGroup>()??owned.gameObject.AddComponent<CanvasGroup>();
            group.alpha=1-eased;group.blocksRaycasts=group.interactable=!open;
        }
    }
    private void RestoreOwnedAreas()
    {
        trayReveal=0;
        if(tray!=null){tray.gameObject.SetActive(false);if(trayFade!=null)trayFade.alpha=0;}
        if(OwnedAreas==null)return;
        foreach(var owned in OwnedAreas)if(owned!=null)
        {
            var group=owned.GetComponent<CanvasGroup>();
            if(group==null)continue;
            group.alpha=1;group.blocksRaycasts=true;group.interactable=true;
        }
    }
    private void OnDisable(){ResetSelection();shown=null;RestoreOwnedAreas();}
    private void OnApplicationFocus(bool focused)
    {if(!focused&&CapturesGameplayInput){ResetSelection();shown=null;RestoreOwnedAreas();if(run!=null)Sync(run);}}
    private void OnApplicationPause(bool paused)
    {if(paused&&CapturesGameplayInput){ResetSelection();shown=null;RestoreOwnedAreas();}}
    private static Text Label(RectTransform parent,Font font,string name,Vector2 pos,Vector2 size,int fontSize)
    {
        var t=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text)).GetComponent<Text>();
        t.transform.SetParent(parent,false);t.rectTransform.anchoredPosition=pos;t.rectTransform.sizeDelta=size;
        t.font=font;t.fontSize=fontSize;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(1,.95f,.86f);
        t.resizeTextForBestFit=true;t.resizeTextMinSize=18;t.resizeTextMaxSize=fontSize;t.raycastTarget=false;return t;
    }
    private Button ButtonAt(RectTransform parent,Font font,string label,Vector2 pos,Action action)
    {
        var b=new GameObject(label,typeof(RectTransform),typeof(CanvasRenderer),typeof(HybridStyleGraphic),typeof(HybridStyleButton)).GetComponent<HybridStyleButton>();
        b.transform.SetParent(parent,false);var r=(RectTransform)b.transform;r.anchoredPosition=pos;r.sizeDelta=new Vector2(144,60);
        var sprite=KaitStorybookTheme.Button;var surface=b.GetComponent<HybridStyleGraphic>();var flat=new Color(.32f,.28f,.37f,.98f);
        surface.Configure(styleSplit,sprite,Color.white,flat,new Color(.98f,.78f,.72f),2,10);
        surface.SetRightSprite(KaitStorybookTheme.Surface("reward-flat",Color.white,Color.white,0,10));
        surface.raycastTarget=true;b.Configure(surface,sprite,KaitStorybookTheme.Pressed,flat);
        b.navigation=new Navigation{mode=Navigation.Mode.None};var labelText=Label(r,font,"Label",new Vector2(17,0),new Vector2(62,48),20);labelText.text=label;
        labelText.gameObject.AddComponent<SunlitSplitText>().Configure(styleSplit);
        KaitUiGlyph.Create(r,label=="跳过"?KaitUiGlyph.Symbol.Skip:label=="重抽"?KaitUiGlyph.Symbol.Reroll:label=="取消"?KaitUiGlyph.Symbol.Close:label=="确认"?KaitUiGlyph.Symbol.Check:KaitUiGlyph.Symbol.Up,new Vector2(-32,0),22).gameObject.AddComponent<SunlitSplitText>().Configure(styleSplit);
        b.onClick.AddListener(()=>{GameAudio.PlayClick();action();});return b;
    }
}
