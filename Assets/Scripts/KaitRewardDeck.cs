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
    public Action SelectionStarted;

    public void Initialize(RectTransform parent,GlobalStyleSplit split,Font font,Func<bool> allowed,Action refresh)
    {
        area=parent;styleSplit=split;canInteract=allowed;changed=refresh;uiFont=font;
        bar=new GameObject("Reward Actions",typeof(RectTransform),typeof(HybridStyleGraphic)).GetComponent<RectTransform>();bar.SetParent(parent,false);
        bar.sizeDelta=new Vector2(800,76);
        bar.anchoredPosition=new Vector2(-60,area.rect.yMax-46);
        var barPlate=bar.GetComponent<HybridStyleGraphic>();
        barPlate.Configure(split,KaitSunlitTheme.Load("Panel",.10f,160),Color.white,new Color(.20f,.17f,.23f,1),new Color(.98f,.78f,.72f),2,10);barPlate.raycastTarget=false;
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
        for(int i=0;i<6;i++)
        {
            int index=i;
            var target=new GameObject("Reward Drop Slot "+(i+1),typeof(RectTransform),typeof(Image)).GetComponent<Image>();
            target.transform.SetParent(parent,false);target.rectTransform.sizeDelta=new Vector2(236,164);
            target.sprite=KaitCardSkin.RoundRect();target.type=Image.Type.Sliced;target.raycastTarget=false;
            slots[i]=target;
            var edge=new GameObject("Drop Outline",typeof(RectTransform),typeof(KaitCardOutline)).GetComponent<KaitCardOutline>();edge.transform.SetParent(target.transform,false);
            edge.rectTransform.anchorMin=Vector2.zero;edge.rectTransform.anchorMax=Vector2.one;edge.rectTransform.sizeDelta=Vector2.zero;edge.raycastTarget=false;slotOutlines[i]=edge;
            slotNames[i]=Label(target.rectTransform,font,"Slot Name",new Vector2(0,10),new Vector2(214,44),22);
            slotHints[i]=Label(target.rectTransform,font,"Drop Action",new Vector2(0,-48),new Vector2(216,58),20);
            dropGlyphs[i]=KaitUiGlyph.Create(target.transform,KaitUiGlyph.Symbol.Plus,new Vector2(0,-48),30);
            slotIcons[i]=KaitCardLogo.Create(target.transform,split,font,new Vector2(0,56),44);
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
        run=current;var pack=run.CurrentReward;
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
            IsDragging&&hoverSlot>=0?(NeedsCopy?"复制":hoverSlot<(run.IsYummn?run.EquippedCardCount:Definition.kind==KaitAbilityKind.Active?run.skills.Count:run.passives.Count)?"替换":"装备")+" · "+slotNames[hoverSlot].text:
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
        int count=run.IsYummn?run.EquippedCardCount:isPassive?run.passives.Count:run.skills.Count,visible=copying?count:Mathf.Min(run.IsYummn?6:3,count+1);
        for(int i=0;i<slots.Length;i++)
        {
            bool show=i<visible;slots[i].gameObject.SetActive(show);validSlots[i]=false;if(!show)continue;
            slots[i].transform.SetAsLastSibling();
            slots[i].rectTransform.sizeDelta=new Vector2(run.IsYummn?176:236,164);
            slots[i].rectTransform.anchoredPosition=new Vector2((i-(visible-1)*.5f)*(run.IsYummn?188:258),isPassive?area.rect.yMax-192:area.rect.yMin+198);
            slotNames[i].rectTransform.sizeDelta=new Vector2(run.IsYummn?164:214,44);
            bool filled=i<count;validSlots[i]=Allowed()&&run.YummnPrerequisite(def)&&(!copying||filled&&KaitAbilityCatalog.Get(run.passives[i])?.copyable==true);
            bool hovered=validSlots[i]&&hoverSlot==i;
            slots[i].color=hovered?new Color(.18f,.48f,.39f,.98f):validSlots[i]?new Color(.28f,.26f,.34f,.98f):new Color(.19f,.18f,.22f,.95f);
            slotOutlines[i].color=hovered?Color.white:validSlots[i]?new Color(.60f,1,.84f):new Color(.45f,.44f,.49f);
            slotNames[i].text=filled?(run.IsYummn?run.EquippedCard(i).nameZh:isPassive?KaitPassiveCatalog.Name(run.passives[i]):KaitRun.SkillName(run.skills[i])):"";
            slotHints[i].text="";
            dropGlyphs[i].SetSymbol(!validSlots[i]?KaitUiGlyph.Symbol.Close:hovered?KaitUiGlyph.Symbol.Check:filled?KaitUiGlyph.Symbol.Swap:KaitUiGlyph.Symbol.Plus);
            slotHints[i].color=hovered?Color.white:new Color(.60f,1,.84f);
            slotIcons[i].gameObject.SetActive(filled);
            if(filled){var equipped=run.IsYummn?run.EquippedCard(i):null;if(equipped!=null){if(equipped.kind==KaitAbilityKind.Active)slotIcons[i].Show(equipped.skill);else slotIcons[i].Show(equipped.passive);}else if(isPassive)slotIcons[i].Show(run.passives[i]);else slotIcons[i].Show(run.skills[i]);}
        }
    }
    private int HitSlot(Vector2 point)
    {
        for(int i=0;i<slots.Length;i++)if(validSlots[i]&&slots[i].gameObject.activeSelf&&
            new Rect(slots[i].rectTransform.anchoredPosition-slots[i].rectTransform.sizeDelta*.5f,slots[i].rectTransform.sizeDelta).Contains(point))return i;
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
        if(NeedsCopy){copy=run.passives[slot];GameAudio.PlayCardSnap();Sync(run);return;}
        int count=run.IsYummn?run.EquippedCardCount:Definition.kind==KaitAbilityKind.Active?run.skills.Count:run.passives.Count;
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
    private void Update(){if(run!=null)Sync(run);}
    private void OnDisable(){ResetSelection();shown=null;}
    private void OnApplicationFocus(bool focused)
    {if(!focused&&IsDragging){ResetSelection();shown=null;if(run!=null)Sync(run);}}
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
        var sprite=KaitSunlitTheme.Load("Button",.15f,80);var surface=b.GetComponent<HybridStyleGraphic>();var flat=new Color(.32f,.28f,.37f,.98f);
        surface.Configure(styleSplit,sprite,Color.white,flat,new Color(.98f,.78f,.72f),2,10);surface.raycastTarget=true;b.Configure(surface,sprite,sprite,flat);
        // Both button skins are dark; keep their shared foreground cream for contrast.
        b.navigation=new Navigation{mode=Navigation.Mode.None};var labelText=Label(r,font,"Label",new Vector2(17,0),new Vector2(62,48),20);labelText.text=label;
        KaitUiGlyph.Create(r,label=="跳过"?KaitUiGlyph.Symbol.Skip:label=="重抽"?KaitUiGlyph.Symbol.Reroll:label=="取消"?KaitUiGlyph.Symbol.Close:label=="确认"?KaitUiGlyph.Symbol.Check:KaitUiGlyph.Symbol.Up,new Vector2(-32,0),22);
        b.onClick.AddListener(()=>{GameAudio.PlayClick();action();});return b;
    }
}
