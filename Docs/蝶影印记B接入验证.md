# 次级幻影：蝶影印记 B

接入选定的 B 八帧原图，使用低饱和紫色、象牙白中心和短重影。图像画布 60×60，放在目标格内右上侧，不占脸部和血条的位置；层级高于敌人、低于 Kait。实机复查发现正上方会与上一格敌人血条重叠，因此采用格内偏移 (46,44)，保留原图比例。

原先的短暂程序绘制特效与目标感叹号被替换。约 0.2 秒显现，目标仍被诱导时保持；敌人阶段表现结束、目标死亡或标记转移后，约 0.24 秒消散。规则提前结算时会暂存显示目标，避免印记在玩家移动刚开始就消失。打断表现时恢复规则状态，不增加输入等待。

技能仍然是让其他能合法攻击的敌人改换目标，不召唤分身，不额外造成伤害，不修改既有音效。重新开始清除上一局印记。

实现：`KaitPhantomMark.cs`、`KaitCombatEffectGraphic.cs`、`KaitGame.cs`。
检查：`KaitPhantomMarkTests`、`Tools/verify_phantom_b_runtime.ps1`。

验证结果：EditMode 384 项全部通过。最终 Windows 构建后，印记保持、移动跟随、解除、死亡、重开，以及冰封、惊惧斩、命中、常规游戏共 9 个运行场景全部通过，无检测到的运行异常。已查看最终保持和消散截图：印记位于目标右上侧，未盖住脸部或上方单位血条。构建日志为 `Logs/phantom-b-build-final.log`，截图为 `VFXScreenshots/phantom-b-phantom.png` 和 `phantom-b-phantom-release.png`。

改动前备份：`C:/Users/yummn/Downloads/kait-backups/phantom-b-before-20260905-131922`。本次只更新 Windows，不构建 Android，也不自动覆盖 GitHub 的 v0.5.1 归档。
