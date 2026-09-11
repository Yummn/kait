public static class YummnTutorial
{
    public static KaitTutorialPages.Page[] Pages=>ForRules(YummnRulesSnapshot.Current());
    public static KaitTutorialPages.Page[] ForRules(YummnRulesSnapshot rules)
    {
        if(rules!=null&&rules.Legacy)return LegacyPages;
        rules=rules??YummnRulesSnapshot.Current();
        if(rules.Is082)return Pages082(rules);
        string supply=rules.Supply==YummnTileSupplyMode.KillOnly?"每击杀一名敌人，敌方阶段结束后补一枚2；没有击杀就不补。":"原地出拳不补2；其他有效行动补一枚2，击杀不额外补。滑行后出拳仍补2。";
        string mapping=rules.SpawnFromEight?"8杂兵、16剑士、32弓手、64重甲、128术士；4不出怪。":"4杂兵、8剑士、16弓手、32重甲、64和128术士。";
        if(rules.ExhaustedMoveSupply)supply=rules.Supply==YummnTileSupplyMode.KillOnly?"击杀补2；气竭移动另补2。":"原地拳不补2，其余动作补一枚2。";
        string supplyTip=rules.ExhaustedMoveSupply?"原地出拳、击杀跟进不算移动。气竭移动不重复叠加已有移动补给，也不受无踪步抑制。开关下局生效。":"设置中的两项对照规则仅对下一局Yummn生效。没有可合数字但仍能移动属于断供，不会直接判负。";
        if(rules.Supply==YummnTileSupplyMode.EveryAction)
        {supply="每次有效方向操作补一枚2。";supplyTip="包括原地出拳，不叠加击杀或气竭补给，无踪步不抑制。无效操作不补。设置下局生效。";}
        if(rules.ActualMoveSupply)
        {supply="人物有效移动至少一格才补一枚2。";supplyTip="跟进、传送算移动；原地动作、只动敌人或副盘不补。无踪步仅抑制高速无攻击移动的补给。";}
        string bossTip=rules.BossLine?"盾骑士远距转盾锁向，下阶段斩击前方整排；墙、冰柱和暗幕阻挡，正面免伤。":"盾骑士准备近战时转盾，正面免伤。";
        return new[]{
            new KaitTutorialPages.Page("Y01","耗气进攻，回满再爆发",$"拳力1 · 气上限{rules.MaxKi} · 击杀+{rules.KillKi}气","高速：耗1气，滑到敌人前出拳。\n\n气竭：走1格或打相邻敌人，不耗基础气；敌方阶段后恢复1气。\n\n本次有击杀，或开始时处于气竭：敌人行动一次。两者同时发生也只一次。","高速击杀：敌人也行动","气竭：敌人行动一次",$"气竭必须回满{rules.MaxKi}气，下一次输入才恢复高速；高速击杀不额外恢复1气。"),
            new KaitTutorialPages.Page("Y02","预警先锁定，再行动","高速击杀也会推进敌方阶段。","每阶段只做一件事：靠近、瞄准或攻击。离开红格躲避锁定攻击。\n\n弓手、术士先瞄准，下一阶段攻击。箭穿过友军；墙、冰柱和暗幕挡箭。\n\n"+bossTip,"先瞄准","下一阶段：打锁定区域","新出生敌人本次不行动；反击不追加敌方阶段，控制按阶段消耗。"),
            LegacyPages[2],
            new KaitTutorialPages.Page("Y04","构筑与战场循环",supply,"合成32三选一，主被动各3槽。\n\n"+mapping+"裂隙在敌方阶段后检查，占格则等待。\n\n256预留Boss，击败Boss获胜；生命归零或2048四向都无法移动、合并则失败，不再重置。","三选一 · 拖动替换","占据裂隙：保留等待",supplyTip)};
    }
    private static KaitTutorialPages.Page[] Pages082(YummnRulesSnapshot rules)
    {
        string move=rules.MovementCostMode==YummnMovementCostMode.PerCell?"高速每走1格耗1气；气不足时走到可支付的最远格。":"高速非零移动固定耗1气。";
        string phase=rules.AttackAdvancesEnemyPhase?"主动攻击、击杀或气竭行动，敌人各行动一次。":"高速未击杀时敌人不行动；击杀或气竭行动，敌人各行动一次。";
        if(rules.MovementAdvancesEnemyPhase)phase="有效移动额外推进敌方；原地击杀、气竭行动仍推进。条件重叠只行动一次。";
        string supply=rules.Supply==YummnTileSupplyMode.KillOnly?"每个击杀补一枚2，满盘不欠账。":"原地拳不补2；移动或非拳击替代技能最多补一枚2，击杀不额外补。";
        if(rules.Supply==YummnTileSupplyMode.EveryAction)supply="每次方向操作补一枚2，包括原地拳和撞墙；无效技能不执行、不补。";
        if(rules.Supply==YummnTileSupplyMode.EffectiveMove)supply="人物实际移动至少一格补一枚2；包括跟进、传送，原地拳不补。";
        if(rules.AttackCostsTenth)move+=" 高速每击0.1气；气竭免费，气不足仍可出拳。";
        if(rules.AttackCostsOne)move+=" 高速每击1气；气竭免费，气不足仍可出拳。";
        return new[]{
            new KaitTutorialPages.Page("Y01",rules.AttackCostTenths>0?"走位与攻击耗气":"走位耗气，贴身免费",$"拳力1 · 气上限{rules.MaxKi} · 击杀+{rules.KillKi}气",move+"\n\n贴身出拳、击杀跟进、推动跟进不收移动气。技能另付牌面气费。\n\n0气进入气竭：每次走1格或出拳，敌方阶段后自然+1气。","移动耗气","拳击固定1伤",rules.ExhaustionNeedsFullKi?$"气竭需回满{rules.MaxKi}气，下一次输入恢复高速。":"气竭恢复到1气以上，下一次输入恢复高速。"),
            new KaitTutorialPages.Page("Y02","留下残影，引导攻击",phase,"高速实际移动后，起点留下一个不阻挡的残影。\n\n敌人攻击覆盖残影：消耗命中的残影并+1气。同一次攻击即使命中多个残影，也只+1气。\n\n敌方阶段结束，剩余残影消退，不回气。","起点留下残影","命中残影：+1气","本体与残影可同时被命中：本体照常受伤。气竭移动、跟进与普通传送不留逻辑残影。"),
            new KaitTutorialPages.Page("Y03","准备技能，再选方向","散打 · 四象 · 暗影，自由混搭","主动牌点击或拖入中间准备，方向输入后执行。\n\n总耗气 = 实际移动气 + 技能额外气。目标无效或技能气不足，整次不执行、保留准备。\n\n每阶段敌人只做一件事：移动、瞄准或攻击。新生敌人可以移动或瞄准，不会直接攻击。","多拳只算一次行动","攻击预警先行","推动近战：预警随人平移；推动弓手：保留方向重算射线；术士锁定地面不跟随。"),
            new KaitTutorialPages.Page("Y04","合成构筑，击败Boss",supply,"合成32三选一，主被动各3槽。\n\n"+(rules.SpawnFromEight?"8杂兵、16剑士、32弓手、64重甲、128术士；4不出怪。":"4杂兵、8剑士、16弓手、32重甲、64和128术士。")+"\n\n256召唤Boss；击杀Boss获胜。生命归零或2048四向都不能移动、合并则失败。","32选牌 · 256召唤Boss","设置只影响下一局","裂隙占格则等待；阶段前出生的新敌人加入本阶段。没有敌人友伤、碰撞伤害或裂隙伤害。")};
    }
    public static readonly KaitTutorialPages.Page[] LegacyPages={
        new KaitTutorialPages.Page("Y01","耗气进攻，回满再爆发","拳力1 · 气上限3 · 击杀+2气","高速：方向输入耗1气，滑到敌人前出拳；敌人不行动。\n\n气竭：走1格或打相邻敌人，不耗基础气；敌人行动一次，随后恢复1气。\n\n气竭后必须回满3气，下一次输入才恢复高速。","击杀不返还行动","回满后，下一次输入高速","每次有效输入都移动一次2048；两盘都无变化不扣气。"),
        new KaitTutorialPages.Page("Y02","预警先锁定，再行动","高速期间敌人不会偷偷推进阶段。","敌人每阶段只做一件事：靠近、瞄准或攻击。红格是已锁定区域，离开即可躲避。\n\n弓手、术士先瞄准，下一阶段攻击。箭穿过友军；墙、冰柱和暗幕挡箭。\n\n盾骑士仅在准备近战时转盾，正面免伤。","先瞄准","下一阶段：打锁定区域","没有敌人友伤、碰撞伤害或裂隙伤害。"),
        new KaitTutorialPages.Page("Y03","准备技能，再选方向","散打 · 四象 · 暗影，自由混搭","点击主动牌或拖到中间进入准备，再输入方向执行。总气耗 = 基础气 + 技能额外气。\n\n疾风连击打同一目标三拳，死亡即停。推掌、震慑可叠加；水鞭、吐息、地形或传送只能选一种，并替换拳击增益。\n\n气不足或目标无效：整次不执行，保留准备。","卡面：主动 / 被动","颜色：白银 / 蓝 / 金","冰柱邻格是阴影；紫色角标指示。暗幕本身不产生阴影。"),
        new KaitTutorialPages.Page("Y04","构筑与战场循环","主动、被动各3槽；新装备下次有效输入生效。","合成32获得三选一，拖入卡槽装备。替换前置牌时会提示受影响的被动。\n\n击杀后或气竭敌人阶段结束，检查裂隙出怪。占格的裂隙保留，不伤人；64与128都是术士。\n\n首次256预留Boss格，击败Boss获胜，生命归零失败。2048锁死会重置数字盘。","三选一 · 拖动替换","占据裂隙：保留等待","本规则仅适用Yummn；Kait仍为2048锁死判负，存档分开。")};
}
