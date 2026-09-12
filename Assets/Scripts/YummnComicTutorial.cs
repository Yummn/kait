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
        if(r.AttackAdvancesEnemyPhase||r.MovementAdvancesEnemyPhase)phase="击杀、气竭行动和等待会推进敌方；\n额外条件见“详细说明”。";
        string supply=r.Supply==YummnTileSupplyMode.KillOnly?"每次击杀补一枚2。":r.Supply==YummnTileSupplyMode.EveryAction?"每次方向操作补一枚2。":r.Supply==YummnTileSupplyMode.EffectiveMove?"人物移动至少一格补一枚2。":"移动或替代技能补2，原地拳不补。";
        return new[]{
            new KaitTutorialPages.Page("YummnComic/01","选方向，移动并出拳","左边战斗，右边合成。",
                "③ 靠近敌人，出拳\n基础拳力1，不靠助跑涨伤害。\n"+attack,
                "① 选方向\n按方向键或滑动屏幕，\n人物和2048棋盘同向行动。",
                "② 滑到前方\n"+movement+"\n遇敌、障碍或气不足就停下。", ""),
            new KaitTutorialPages.Page("YummnComic/02","留气防身，气竭先避险",r.KiGuard?"高速有气时可挡一次攻击，随后清空气并进入气竭。":$"气上限{r.MaxKi} · 每击杀回{r.KillKi}气",
                "③ 让敌人打残影\n有效移动起点留下残影；\n每次攻击命中残影，回1气。",
                "① 什么时候轮到敌人？\n"+phase,
                "② 气竭，先躲预警\n走1格或出拳，敌人行动后回1气。\n"+(r.ExhaustionNeedsFullKi?"回满后，下一次操作恢复高速。":"回到1气后，下次操作恢复高速。"), ""),
            new KaitTutorialPages.Page("YummnComic/03","合成选牌，击败Boss","击败Boss获胜；生命归零或2048棋盘锁死则失败。",
                "③ 合成大数字，挑战Boss\n正面盾牌挡伤害，绕到侧背打。\n击败盾骑士，赢下这一局！",
                "① 合成也会引来敌人\n裂隙先预警，之后才出怪。\n"+supply,
                $"② 合成{r.RewardMergeValue}，三选一\n拖入卡槽装备；主动牌拖到中央，\n松手后点方向、自身或目标。", "")};
    }

    public static string Appendix(YummnRulesSnapshot r)
    {
        r=r??YummnRulesSnapshot.Current();
        var s=new StringBuilder();
        s.AppendLine("先记住这三件事");
        s.AppendLine("选方向：左盘移动、贴身出拳，右盘同向合成。留意气量和敌人预警。合成大数字引出Boss，击败它获胜。\n");
        s.AppendLine("怎么操作");
        s.AppendLine("移动 / 出拳：WASD、方向键、屏幕移动键，或在棋盘上滑动。");
        s.AppendLine("连续输入：设置中开启后，按住方向键，或滑动后不松手；当前动作结束会继续同方向操作。松手停止，换方向请重新按键或滑动。击杀后没有额外定时停顿，也会等出拳动作结束。");
        s.AppendLine("等待：点“等待”，或在非按钮、非卡牌区域原地长按约0.55秒。两盘不滑动，补一枚2并推进一次敌方阶段；每次长按只等待一次。选技能目标期间不能等待。\n");
        s.AppendLine("什么时候敌人行动");
        s.AppendLine("敌方阶段＝轮到敌人行动。触发条件：击杀、气竭状态下行动、等待"+(r.AttackAdvancesEnemyPhase?"、主动攻击":"")+(r.MovementAdvancesEnemyPhase?"、人物有效移动（至少一格）":"")+"。同一操作满足多项，也只推进一次。反击不会再追加一个阶段。\n");
        s.AppendLine("耗气与气竭");
        s.AppendLine($"本局气上限{r.MaxKi}，基础拳力1。"+(r.MovementCostMode==YummnMovementCostMode.PerCell?"高速每移动一格耗1气，气不足就停在能到达的最远格。":"高速移动固定耗1气；没有移动不收移动气。"));
        s.AppendLine(r.AttackCostTenths==0?"普通出拳不耗气。":$"高速普通拳每拳耗{r.AttackCostTenths/10f:0.#}气；气不足仍能出拳，耗尽后进入气竭。气竭普通拳不耗气。");
        s.AppendLine("击杀跟进、推动跟进不收移动气；技能另付卡面气费。气耗尽进入气竭：每次只能走一格或出拳，不收基础移动气。"+(r.ExhaustionNeedsFullKi?"气回满后，下一次操作恢复高速。":"气恢复到至少1点后，下一次操作恢复高速。"));
        s.AppendLine($"每击杀一名敌人回{r.KillKi}气。若本次操作开始时已气竭，敌方阶段结束再回1气；等待也适用。高速击杀没有这额外1气。\n");
        s.AppendLine("气格挡");
        s.AppendLine(r.KiGuard?"本局已开启，无需装备卡牌。高速且仍有气、这次行动尚未耗尽气时，自动抵消一次本应扣血的攻击，清空气并立即进入气竭。\n气竭期间不能再次触发，同一敌方阶段的后续攻击仍会扣血。无敌或其他防御能挡住时，不消耗气格挡。":"本局未开启气格挡。可在Yummn设置中开启，下一局生效。");
        s.AppendLine();
        s.AppendLine("残影怎么回气");
        s.AppendLine("人物有效移动至少一格就在起点留下残影，气竭移动、击杀进格、追身步、传送也算；连续滑行只在该段起点生成。残影不挡路，基础承受一次攻击后消失，暗影斗篷可使残影在本敌方阶段无限承伤。每次攻击命中回1气，一次攻击命中多个残影也只回1气。阶段结束全部清除。残影不是护盾：本体也在攻击范围内时，仍需躲避或格挡。\n");
        s.AppendLine("合成、补2与裂隙");
        s.AppendLine(r.Supply==YummnTileSupplyMode.KillOnly?"本局每击杀一名敌人补一枚2。":r.Supply==YummnTileSupplyMode.EveryAction?"本局每次方向操作补一枚2，原地出拳、撞墙也算；无效技能不执行、不补2。":r.Supply==YummnTileSupplyMode.EffectiveMove?"本局人物实际移动至少一格才补一枚2，跟进和传送也算；只动敌人或2048棋盘不算。":"本局移动或替代拳击的技能最多补一枚2，原地普通拳不补；无踪步可免去高速无攻击移动的补给。");
        s.AppendLine("补2时没有空格就不补，不积欠。每次等待固定补一枚2，静谧心境不重复追加；等待中的反击击杀另按本局击杀补给规则结算。");
        s.AppendLine("合成结果对应敌人："+(r.SpawnFromEight?"8杂兵、16剑士、32弓手、64重甲；4不产生裂隙。":"4杂兵、8剑士、16弓手、32重甲、64术士。")+"达到本局Boss门槛的大数字优先召唤Boss，不产生普通敌人裂隙。");
        s.AppendLine("新裂隙不会在产生它的同一次操作中出怪；后续结算时，空格才出怪，被占用就继续等待。裂隙不挡移动、不造成伤害。\n");
        s.AppendLine("看预警，躲攻击");
        s.AppendLine("每个敌方阶段，敌人只做移动、瞄准、攻击中的一件事。新生敌人可以移动或瞄准，不会直接攻击。红色表示锁定区域，黄色表示正在出手。");
        s.AppendLine("杂兵、剑士、重甲：近战攻击；杂兵和重甲可推动。\n弓手：先锁方向，下一阶段射击；箭穿过友军，墙、冰柱和暗幕挡箭。\n术士：先锁地面，下一阶段攻击十字区域。\n盾骑士：转盾锁方向，下一阶段攻击前方整排；正面免伤，侧背可伤。");
        s.AppendLine("推动近战敌人，预警跟着移动；推动弓手，按原方向重新计算射线；术士预警留在原地。本角色没有敌人友伤和碰撞伤害。\n");
        s.AppendLine("选牌与施放");
        s.AppendLine($"合成{r.RewardMergeValue}获得三选一。被动在上、主动在下，共用6个槽，比例任意；拖入空槽装备，拖到任意旧牌替换，拖出归位。同类变体可同时装备，具体触发以卡面为准。");
        s.AppendLine("点击主动牌只看详情。拖到中央并松手，才开始选择释放目标：方向技能点人物四周的箭头；疗伤冥想点Yummn自身；冰柱、暗幕、诱饵点空目标格。施放不滑动副盘；选目标时方向输入不会移动人物。目标无效或气不足会提示释放失败并退出，不扣气、不推进回合；重新拖出卡牌可重试。\n");
        s.AppendLine("冻结与震慑");
        s.AppendLine("冻结敌人视作冰柱，持续跳过行动，不会自行解冻。下一次攻击解除冻结并免疫该次伤害；之后的下一拳可正常命中。震慑持续整个敌方回合，结束后恢复；持续期间震慑拳不重复耗气。冰柱和暗幕均为2气，永久保留到重新放置。");
        s.AppendLine("伺机而动在敌人进入四邻时反击；借机攻击在敌人离开四邻前出拳，推动也可触发，人物自己走远不触发。两者各对每个敌人每次操作触发一次，均可联动疾风连击。");
        s.AppendLine("同类变体");
        s.AppendLine("疾风连击：每次拳击总计花2气，逐拳打两次，替代普通拳气费；不足2气只打普通一拳。反击也能二连击，每拳触发火蛇之牙等效果。目标死亡或推远且无法跟进时停止后续拳。伺机而动：敌人进入四邻范围时反击，移动、推动、出生都算；每个敌人每次操作最多一次，持续相邻不会重复反击。");
        s.AppendLine("无甲防御：等待时抵挡本敌方阶段第一次伤害，治疗等待也有效。不坏气拳优先于散打技巧·推掌。斗战冥想、静谧心境会取代移动补2；斗战冥想每杀补两个2。气海扩张和灵体护身的击杀回气惩罚可以叠加，最低0。");
        s.AppendLine("胜负与设置");
        s.AppendLine("合成大数字，召唤盾骑士Boss；击败Boss获胜。生命归零，或2048棋盘四个方向都无法移动、合并，则失败。暂时无法合成但仍能移动，不算锁死。");
        s.AppendLine("以上显示本局实际规则；规则设置在下一局生效，旧存档保留旧规则及教程。漫画只示意过程，实际两盘均为5×5。");
        return s.ToString();
    }
}
