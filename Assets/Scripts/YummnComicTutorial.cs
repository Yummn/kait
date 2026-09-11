using System.Text;

// Three readable core pages; exact run settings and exceptions stay in the appendix.
public static class YummnComicTutorial
{
    public static KaitTutorialPages.Page[] ForRules(YummnRulesSnapshot r)
    {
        r=r??YummnRulesSnapshot.Current();
        string movement=r.MovementCostMode==YummnMovementCostMode.PerCell?"高速每走1格耗1气。":"高速移动一次耗1气。";
        string attack=r.AttackCostTenths==0?"贴身拳不耗气。":$"高速每拳耗{r.AttackCostTenths/10f:0.#}气；气竭拳免费。";
        string phase="击杀、气竭行动或等待，\n会推进一次敌方阶段。";
        if(r.AttackAdvancesEnemyPhase||r.MovementAdvancesEnemyPhase)phase="击杀、气竭行动和等待会推进敌方；\n本局额外条件见附录。";
        string supply=r.Supply==YummnTileSupplyMode.KillOnly?"每次击杀补一枚2。":r.Supply==YummnTileSupplyMode.EveryAction?"每次方向操作补一枚2。":r.Supply==YummnTileSupplyMode.EffectiveMove?"人物移动至少一格补一枚2。":"移动或替代技能补2，原地拳不补。";
        return new[]{
            new KaitTutorialPages.Page("YummnComic/01","选方向，移动并出拳","左边战斗，右边合成。",
                "③ 靠近敌人，出拳\n基础拳力1，不靠助跑涨伤害。\n"+attack,
                "① 选方向\n按方向键或滑动屏幕，\n人物和2048棋盘同向行动。",
                "② 滑到前方\n"+movement+"\n遇敌、障碍或气不足就停下。", ""),
            new KaitTutorialPages.Page("YummnComic/02","用气抢先手，用残影回气",$"气上限{r.MaxKi} · 每击杀回{r.KillKi}气",
                "③ 让敌人打残影\n高速移动起点留下残影；\n每次攻击命中残影，回1气。",
                "① 什么时候轮到敌人？\n"+phase,
                "② 气竭，先躲预警\n走1格或出拳，敌人行动后回1气。\n"+(r.ExhaustionNeedsFullKi?"回满后，下一次操作恢复高速。":"回到1气后，下次操作恢复高速。"), ""),
            new KaitTutorialPages.Page("YummnComic/03","合成选牌，击败Boss","击败Boss获胜；生命归零或2048棋盘锁死则失败。",
                "③ 合成大数字，挑战Boss\n正面盾牌挡伤害，绕到侧背打。\n击败盾骑士，赢下这一局！",
                "① 合成也会引来敌人\n裂隙先预警，之后才出怪。\n"+supply,
                "② 合成32，三选一\n拖入卡槽装备；主动牌先准备，\n再选方向施放。", "")};
    }

    public static string Appendix(YummnRulesSnapshot r)
    {
        r=r??YummnRulesSnapshot.Current();
        var s=new StringBuilder();
        s.AppendLine("操作与敌方阶段");
        s.AppendLine("WASD、方向键、屏幕移动键或空白处滑动：选择方向。点击等待按钮或轻点Yummn：人物和2048棋盘不动，只推进敌方阶段。");
        s.AppendLine("敌方阶段＝轮到敌人行动。触发条件：击杀、气竭状态下行动、等待"+(r.AttackAdvancesEnemyPhase?"、主动攻击":"")+(r.MovementAdvancesEnemyPhase?"、人物有效移动（至少一格）":"")+"。同一操作满足多项，也只推进一次。反击不会再追加一个阶段。\n");
        s.AppendLine("耗气与气竭");
        s.AppendLine($"本局气上限{r.MaxKi}，基础拳力1。"+(r.MovementCostMode==YummnMovementCostMode.PerCell?"高速每移动一格耗1气，气不足就停在能到达的最远格。":"高速移动固定耗1气；没有移动不收移动气。"));
        s.AppendLine(r.AttackCostTenths==0?"普通出拳不耗气。":$"高速普通拳每拳耗{r.AttackCostTenths/10f:0.#}气；气不足仍能出拳，耗尽后进入气竭。气竭普通拳不耗气。");
        s.AppendLine("击杀跟进、推动跟进不收移动气；技能另付卡面气费。气耗尽进入气竭：每次只能走一格或出拳，不收基础移动气。"+(r.ExhaustionNeedsFullKi?"气回满后，下一次操作恢复高速。":"气恢复到至少1点后，下一次操作恢复高速。"));
        s.AppendLine($"每击杀一名敌人回{r.KillKi}气。若本次操作开始时已气竭，敌方阶段结束再回1气；等待也适用。高速击杀没有这额外1气。\n");
        s.AppendLine("残影怎么回气");
        s.AppendLine("高速主动移动后，起点留下残影。气竭移动、跟进和普通传送不留残影。残影不挡路，可在同一敌方阶段承受多次攻击：每次攻击命中回1气，一次攻击命中多个残影也只回1气。阶段结束全部清除，消失本身不回气。本体与残影同时被打中，本体仍会受伤。\n");
        s.AppendLine("合成、补2与裂隙");
        s.AppendLine(r.Supply==YummnTileSupplyMode.KillOnly?"本局每击杀一名敌人补一枚2。":r.Supply==YummnTileSupplyMode.EveryAction?"本局每次方向操作补一枚2，原地出拳、撞墙也算；无效技能不执行、不补2。":r.Supply==YummnTileSupplyMode.EffectiveMove?"本局人物实际移动至少一格才补一枚2，跟进和传送也算；只动敌人或2048棋盘不算。":"本局移动或替代拳击的技能最多补一枚2，原地普通拳不补；无踪步可免去高速无攻击移动的补给。");
        s.AppendLine("补2时没有空格就不补，不积欠。等待本身不补2；等待中的反击击杀，仍按击杀补给模式结算。");
        s.AppendLine("合成结果对应敌人："+(r.SpawnFromEight?"8杂兵、16剑士、32弓手、64重甲、128术士；4不产生裂隙。":"4杂兵、8剑士、16弓手、32重甲、64和128术士。"));
        s.AppendLine("新裂隙不会在产生它的同一次操作中出怪；后续结算时，空格才出怪，被占用就继续等待。裂隙不挡移动、不造成伤害。\n");
        s.AppendLine("看预警，躲攻击");
        s.AppendLine("每个敌方阶段，敌人只做移动、瞄准、攻击中的一件事。新生敌人可以移动或瞄准，不会直接攻击。红色表示锁定区域，黄色表示正在出手。");
        s.AppendLine("杂兵、剑士、重甲：近战攻击；杂兵和重甲可推动。\n弓手：先锁方向，下一阶段射击；箭穿过友军，墙、冰柱和暗幕挡箭。\n术士：先锁地面，下一阶段攻击十字区域。\n盾骑士：转盾锁方向，下一阶段攻击前方整排；正面免伤，侧背可伤。");
        s.AppendLine("推动近战敌人，预警跟着移动；推动弓手，按原方向重新计算射线；术士预警留在原地。本角色没有敌人友伤和碰撞伤害。\n");
        s.AppendLine("选牌与施放");
        s.AppendLine("合成32获得三选一。被动在上、主动在下，各3槽；奖励卡拖入同类空槽装备，拖到旧牌替换，拖出归位。散打、四象、暗影可以混搭。");
        s.AppendLine("点击已装备的主动牌，或拖到中央准备，再输入方向施放。目标无效或技能气不足：整次不执行，保留准备。技能气费、目标和叠加限制看卡面。\n");
        s.AppendLine("胜负与设置");
        s.AppendLine("合成大数字，召唤盾骑士Boss；击败Boss获胜。生命归零，或2048棋盘四个方向都无法移动、合并，则失败。暂时无法合成但仍能移动，不算锁死。");
        s.AppendLine("以上显示本局实际规则；规则设置在下一局生效，旧存档保留旧规则及教程。漫画只示意过程，实际两盘均为5×5。");
        return s.ToString();
    }
}
