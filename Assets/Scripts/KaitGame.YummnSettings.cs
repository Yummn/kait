using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public sealed partial class KaitGame
{
    private const string Yummn082Preference="Kait.Yummn082.";
    private Toggle yummnEightToggle,yummnAttackPhaseToggle,yummnFullRecoveryToggle;
    private Toggle yummnAttackCostToggle;
    private readonly List<GameObject> yummnSettingsControls=new List<GameObject>();
    private readonly List<Button> yummnRuleButtons=new List<Button>();
    private Text yummnSettingsNote,characterSettingsTitle,characterSettingsHint;
    private float yummnRunStartedAt;
    private static YummnRulesSnapshot YummnPreset()=>YummnRulesSnapshot.Current(
        Mathf.Clamp(PlayerPrefs.GetInt(Yummn082Preference+"MaxKi",6),3,9),
        PlayerPrefs.GetInt(Yummn082Preference+"FixedMove",0)==1?YummnMovementCostMode.FixedOne:YummnMovementCostMode.PerCell,
        Mathf.Clamp(PlayerPrefs.GetInt(Yummn082Preference+"KillKi",3),1,3),
        PlayerPrefs.GetInt(Yummn082Preference+"AttackPhase",0)==1,PlayerPrefs.GetInt(Yummn082Preference+"FullRecovery",1)==1,
        (YummnTileSupplyMode)Mathf.Clamp(PlayerPrefs.GetInt(Yummn082Preference+"SupplyMode",PlayerPrefs.GetInt(Yummn082Preference+"ActionSupply",0)),0,3),
        PlayerPrefs.GetInt(Yummn082Preference+"FromEight",0)==1,
        false,
        PlayerPrefs.GetInt(Yummn082Preference+"MovePhase",0)==1,
        PlayerPrefs.GetInt(Yummn082Preference+"AttackOne",PlayerPrefs.GetInt(Yummn082Preference+"AttackTenth",0))==1,
        PlayerPrefs.GetInt(Yummn082Preference+"KiGuard",1)==1,
        PlayerPrefs.GetInt(Yummn082Preference+"Reward32",0)==1?32:16);
    private void AddYummnSettings(Transform parent)
    {
        string[] keys={"MaxKi","FixedMove","KillKi","SupplyMode"};
        for(int i=0;i<4;i++)
        {
            int index=i;var b=MakeFlatButton(parent,new Vector2(0,125-i*36),new Vector2(580,36),"");
            b.gameObject.name="Yummn Rule "+keys[i];yummnSettingsControls.Add(b.gameObject);yummnRuleButtons.Add(b);
            var label=b.GetComponentInChildren<Text>();label.font=threatBoardFont;label.fontSize=20;label.resizeTextMinSize=16;label.resizeTextMaxSize=20;
            b.onClick.AddListener(()=>
            {
                var r=YummnPreset();int value=index==0?(r.MaxKi==3?9:r.MaxKi-1):index==1?(r.MovementCostMode==YummnMovementCostMode.PerCell?1:0):index==2?(r.KillKi==1?3:r.KillKi-1):((int)r.Supply+1)%4;
                PlayerPrefs.SetInt(Yummn082Preference+keys[index],value);PlayerPrefs.Save();RefreshYummnSettingsNote();
            });
        }
        var next=YummnPreset();
        yummnAttackPhaseToggle=AddYummnRuleToggle(parent,-20,"攻击也推进敌方回合","AttackPhase",next.AttackAdvancesEnemyPhase);
        yummnFullRecoveryToggle=AddYummnRuleToggle(parent,-56,"气竭需要回满气","FullRecovery",next.ExhaustionNeedsFullKi);
        yummnAttackCostToggle=AddYummnRuleToggle(parent,-92,"高速攻击耗气：0气 / 1气","AttackOne",next.AttackCostsOne);
        AddYummnRuleToggle(parent,-128,"有效移动额外推进敌方回合","MovePhase",next.MovementAdvancesEnemyPhase);
        yummnEightToggle=AddYummnRuleToggle(parent,-164,"从8开始出怪（默认4）","FromEight",next.SpawnFromEight);
        AddYummnRuleToggle(parent,-200,"气格挡：抵消攻击并进入气竭","KiGuard",next.KiGuard);
        AddYummnRuleToggle(parent,-236,"技能获得阈值：32（关闭为16）","Reward32",next.RewardMergeValue==32);
        yummnSettingsNote=MakeText("",parent,new Vector2(0,-273),new Vector2(680,22),14,Peach,TextAnchor.MiddleCenter,FontStyle.Normal,false);yummnSettingsNote.font=threatBoardFont;
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
            kaitEffectiveMoveSupplyToggle.gameObject.SetActive(!yummn);
            kaitEffectiveMoveSupplyToggle.SetIsOnWithoutNotify(MainMenuVisible?PlayerPrefs.GetInt(KaitEffectiveMoveSupplyPreference,0)==1:run.config.kaitEffectiveMoveSupply);
        }
        if(characterSettingsTitle!=null)characterSettingsTitle.text=character+" · 设置";
        if(characterSettingsHint!=null)characterSettingsHint.text=yummn?"人物无敌即时生效；规则选项仅在下一局生效":"伤害选项即时生效；墙体选项会重新开始本局";
        foreach(var t in new[]{disableThreatPillarsToggle,disableRiftDamageToggle,disableFriendlyFireToggle,disableCollisionDamageToggle})if(t!=null)t.gameObject.SetActive(!yummn);
        foreach(var g in yummnSettingsControls)g.SetActive(yummn);
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
    }
    private void RefreshYummnSettingsNote()
    {
        if(yummnSettingsNote==null)return;var next=YummnPreset();
        if(yummnAttackCostToggle!=null)yummnAttackCostToggle.GetComponentInChildren<Text>().text=$"高速攻击耗气：{(next.AttackCostsOne?1:0)}气（气竭免费）";
        string supplyLabel=next.Supply==YummnTileSupplyMode.KillOnly?"每击杀补2":next.Supply==YummnTileSupplyMode.EveryAction?"每次方向操作补2":next.Supply==YummnTileSupplyMode.EffectiveMove?"有效移动至少一格补2":"旧版动作补2（原地拳不补）";
        string[] labels={$"气上限：{next.MaxKi}　›",$"移动耗气：{(next.MovementCostMode==YummnMovementCostMode.PerCell?"每格1气":"移动固定1气")}　›",$"每次击杀恢复：{next.KillKi}气　›",$"补给：{supplyLabel}　›"};
        for(int i=0;i<yummnRuleButtons.Count;i++)yummnRuleButtons[i].GetComponentInChildren<Text>().text=labels[i];
        yummnSettingsNote.text=$"本局：{run.Yummn.rules.MaxKi}气上限 · 击杀+{run.Yummn.rules.KillKi}气"+(run.Yummn.rules.Is082?"":" · 旧版存档")+"　｜　"+
            "下局："+supplyLabel;
        if(MainMenuVisible)yummnSettingsNote.text="新局："+supplyLabel;
    }
}
