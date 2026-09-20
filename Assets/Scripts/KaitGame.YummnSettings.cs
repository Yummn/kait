using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public sealed partial class KaitGame
{
    private const string Yummn082Preference="Kait.Yummn082.";
    private Toggle yummnFullRecoveryToggle;
    private Toggle yummnAttackCostToggle;
    private Toggle yummnMovePhaseToggle;
    private Toggle yummnDisableKiGuardToggle;
    private readonly List<GameObject> yummnSettingsControls=new List<GameObject>();
    private readonly List<Button> yummnRuleButtons=new List<Button>();
    private Text yummnSettingsNote,characterSettingsTitle,characterSettingsHint;
    private float yummnRunStartedAt;
    private static YummnRulesSnapshot YummnPreset()=>YummnRulesSnapshot.Current(
        Mathf.Clamp(PlayerPrefs.GetInt(Yummn082Preference+"MaxKi",7),3,9),
        PlayerPrefs.GetInt(Yummn082Preference+"FixedMove",0)==1?YummnMovementCostMode.FixedOne:YummnMovementCostMode.PerCell,
        Mathf.Clamp(PlayerPrefs.GetInt(Yummn082Preference+"KillKi",3),1,3),
        false,PlayerPrefs.GetInt(Yummn082Preference+"ExitExhaustionEarly",0)==0,
        (YummnTileSupplyMode)Mathf.Clamp(PlayerPrefs.GetInt(Yummn082Preference+"SupplyMode",(int)YummnTileSupplyMode.EffectiveMove),0,3),
        false,
        false,
        PlayerPrefs.GetInt(Yummn082Preference+"MovePhase",0)==1,
        PlayerPrefs.GetInt(Yummn082Preference+"FreeFastAttack",0)==0,
        PlayerPrefs.GetInt(Yummn082Preference+"DisableKiGuard",0)==0,
        16,
        PlayerPrefs.GetInt(Yummn082Preference+"MaxHp",6)==3?3:6);
    private void AddYummnSettings(Transform parent)
    {
        string[] keys={"MaxHp","MaxKi","FixedMove","KillKi","SupplyMode"};
        for(int i=0;i<keys.Length;i++)
        {
            int index=i;var b=MakeFlatButton(parent,new Vector2(0,127-i*34),new Vector2(580,32),"");
            b.GetComponent<Image>().sprite=KaitStorybookTheme.Surface("setting-row",new Color32(241,226,212,255),new Color32(219,202,208,255),2,8);
            b.gameObject.name="Yummn Rule "+keys[i];yummnSettingsControls.Add(b.gameObject);yummnRuleButtons.Add(b);
            var label=b.GetComponentInChildren<Text>();label.font=threatBoardFont;label.fontSize=20;label.resizeTextMinSize=16;label.resizeTextMaxSize=20;label.fontStyle=FontStyle.Normal;
            // The shared button inset leaves only 22px in this 32px row.
            // This font's line metrics then truncate even a single fitted line.
            label.rectTransform.offsetMin=new Vector2(10,0);
            label.rectTransform.offsetMax=new Vector2(-10,0);
            label.horizontalOverflow=HorizontalWrapMode.Overflow;
            label.verticalOverflow=VerticalWrapMode.Overflow;
            b.onClick.AddListener(()=>
            {
                var r=YummnPreset();int value=index==0?(r.MaxHp==6?3:6):index==1?(r.MaxKi==3?9:r.MaxKi-1):index==2?(r.MovementCostMode==YummnMovementCostMode.PerCell?1:0):index==3?(r.KillKi==1?3:r.KillKi-1):((int)r.Supply+1)%4;
                PlayerPrefs.SetInt(Yummn082Preference+keys[index],value);PlayerPrefs.Save();RefreshYummnSettingsNote();
            });
        }
        var next=YummnPreset();
        // Every unchecked switch is the approved default. Enabling a switch
        // always means opting into a rule variation.
        yummnFullRecoveryToggle=AddYummnRuleToggle(parent,-38,"气竭未回满也可退出","ExitExhaustionEarly",!next.ExhaustionNeedsFullKi);
        yummnAttackCostToggle=AddYummnRuleToggle(parent,-74,"高速攻击不耗气（气竭仍免费）","FreeFastAttack",!next.AttackCostsOne);
        yummnMovePhaseToggle=AddYummnRuleToggle(parent,-110,"有效移动额外推进敌方回合","MovePhase",next.MovementAdvancesEnemyPhase);
        yummnDisableKiGuardToggle=AddYummnRuleToggle(parent,-146,"关闭气格挡","DisableKiGuard",!next.KiGuard);
        yummnSettingsNote=MakeText("",parent,new Vector2(0,-190),new Vector2(680,26),15,Peach,TextAnchor.MiddleCenter,FontStyle.Normal,false);yummnSettingsNote.font=threatBoardFont;
        RefreshYummnSettingsNote();
    }
    private Toggle AddYummnRuleToggle(Transform parent,float y,string title,string key,bool value)
    {
        var t=MakeFlatToggle(parent,new Vector2(0,y-9),new Vector2(580,36),title);t.SetIsOnWithoutNotify(value);
        yummnSettingsControls.Add(t.gameObject);t.onValueChanged.AddListener(v=>{SaveBooleanPreference(Yummn082Preference+key,v);RefreshYummnSettingsNote();});return t;
    }
    private void RefreshCharacterSettings()
    {
        var character=MainMenuVisible?mainMenu.Selected:run.Character;
        bool yummn=character==KaitCharacter.Yummn;
        if(kaitEffectiveMoveSupplyToggle!=null)
        {
            kaitEffectiveMoveSupplyToggle.gameObject.SetActive(character==KaitCharacter.Kait);
            kaitEffectiveMoveSupplyToggle.SetIsOnWithoutNotify(MainMenuVisible?PlayerPrefs.GetInt(KaitEffectiveMoveSupplyPreference,0)==1:run.config.kaitEffectiveMoveSupply);
        }
        if(characterSettingsTitle!=null)characterSettingsTitle.text=character+" · 设置";
        if(characterSettingsHint!=null)characterSettingsHint.text=yummn?"人物无敌即时生效；规则选项仅在下一局生效":"伤害选项即时生效；墙体选项会重新开始本局";
        foreach(var t in new[]{disableThreatPillarsToggle,disableRiftDamageToggle,disableFriendlyFireToggle,disableCollisionDamageToggle})if(t!=null)t.gameObject.SetActive(!yummn);
        foreach(var g in yummnSettingsControls)g.SetActive(yummn);
        var next=YummnPreset();
        yummnFullRecoveryToggle?.SetIsOnWithoutNotify(!next.ExhaustionNeedsFullKi);
        yummnAttackCostToggle?.SetIsOnWithoutNotify(!next.AttackCostsOne);
        yummnMovePhaseToggle?.SetIsOnWithoutNotify(next.MovementAdvancesEnemyPhase);
        yummnDisableKiGuardToggle?.SetIsOnWithoutNotify(!next.KiGuard);
        if(MainMenuVisible)
        {
            foreach(var t in new[]{disableThreatPillarsToggle,disableRiftDamageToggle,disableFriendlyFireToggle,disableCollisionDamageToggle})if(t!=null)t.interactable=true;
            disableThreatPillarsToggle?.SetIsOnWithoutNotify(PlayerPrefs.GetInt(DisableThreatPillarsPreference,0)==1);
            disableRiftDamageToggle?.SetIsOnWithoutNotify(PlayerPrefs.GetInt(DisableRiftDamagePreference,0)==1);
            disableFriendlyFireToggle?.SetIsOnWithoutNotify(PlayerPrefs.GetInt(DisableFriendlyFirePreference,0)==1);
            disableCollisionDamageToggle?.SetIsOnWithoutNotify(PlayerPrefs.GetInt(DisableCollisionDamagePreference,0)==1);
            if(characterSettingsHint!=null)characterSettingsHint.text="首页修改选项，将应用于新的一局";
        }
        if(yummnSettingsNote!=null)yummnSettingsNote.gameObject.SetActive(yummn);
        RefreshYummnSettingsNote();
        EnsureSettingsTextVisible();
    }
    private void RefreshYummnSettingsNote()
    {
        if(yummnSettingsNote==null)return;var next=YummnPreset();
        string supplyLabel=next.Supply==YummnTileSupplyMode.KillOnly?"每次击杀补一个2":next.Supply==YummnTileSupplyMode.EveryAction?"每次方向操作补一个2":next.Supply==YummnTileSupplyMode.EffectiveMove?"有效移动至少一格补一个2":"每次有效动作补一个2（原地拳不补）";
        string[] labels={$"生命上限：{next.MaxHp}　›",$"气上限：{next.MaxKi}　›",$"移动耗气：{(next.MovementCostMode==YummnMovementCostMode.PerCell?"每格1气":"移动固定1气")}　›",$"每次击杀恢复：{next.KillKi}气　›",$"补给：{supplyLabel}　›"};
        for(int i=0;i<yummnRuleButtons.Count;i++)yummnRuleButtons[i].GetComponentInChildren<Text>().text=labels[i];
        yummnSettingsNote.text=$"本局：{run.KateMaxHp}生命 · {run.Yummn.rules.MaxKi}气上限"+(run.Yummn.rules.Is082?"":" · 旧版存档")+"　｜　"+
            $"下局：{next.MaxHp}生命 · "+supplyLabel;
        if(MainMenuVisible)yummnSettingsNote.text=$"下一局：{next.MaxHp}生命 · "+supplyLabel;
    }

    private void EnsureSettingsTextVisible()
    {
        if(settingsOverlay==null)return;
        Font stable=threatBoardFont!=null?threatBoardFont:uiFont;
        foreach(var label in settingsOverlay.GetComponentsInChildren<Text>(true))
        {
            label.enabled=true;
            if(stable!=null)label.font=stable;
            label.color=KaitStorybookTheme.Ink;
            label.canvasRenderer.SetAlpha(1f);
            label.raycastTarget=false;
            var outline=label.GetComponent<Outline>();
            if(outline!=null)outline.enabled=false;
        }
    }
}
