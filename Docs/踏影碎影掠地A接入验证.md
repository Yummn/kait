# 踏影：碎影掠地 A

踏影保留 Kait 原有的 `000000_run_jump` 人物动作和移动曲线。起点、落点原有的程序烟尘改为 A 版八帧碎影：低饱和紫色、象牙白小高光、尖锐碎片轮廓。

特效采用 92×92 画布，位于地面特效层和人物下方，不遮挡 Kait、敌人或血条。图集基线按 Unity UI 坐标方向校正，使碎影贴在脚底；起点与落点独立播放约 0.34 秒，不延长踏影过程，也不增加输入锁。

实现涉及 `KaitCombatEffectGraphic.cs`、`KaitGame.cs`、`KaitShatterImporter.cs` 和 `ShadowStepA.png`。规则、位移距离、音效和其他技能没有调整。

验证结果：EditMode 387 项全部通过。Windows 构建完成后，踏影、蝶影印记、冰封、命中和常规游戏 5 个运行场景全部通过。首次实机截图发现图集锚点偏高，已反向校正并重新构建；最终截图中起点与落点碎影均贴地，层级位于人物下方。

最终构建日志：`Logs/shadow-step-a-build-final.log`。最终截图：`VFXScreenshots/shadow-step-a-final.png`。

改动前备份：`C:/Users/yummn/Downloads/kait-backups/shadow-step-a-before-20260905-140020`。本次只更新 Windows，不构建 Android，也不自动上传 GitHub。
