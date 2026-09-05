# 术士特效 A 接入记录

2026-09-05

采用 A「紫白裂光」。瞄准使用紫色角标、半透明五格底色和中心菱形；攻击时播放约 0.32 秒的八帧十字裂光。裂光放在人物下方，按这次攻击实际生效的格子裁切，柱子和棋盘外没有特效。裁切调整绘制区域及其贴图坐标，不拉伸断开的十字，也不新增人物遮罩。

旧的施法程序图形和中心爆点已替换。瞄准—攻击的回合规则、伤害与音效保持原有逻辑。正在攻击的术士暂时隐藏其旧预警，直到表现切回本回合结算后的状态，防止旧预警闪回。

验证：

- EditMode 338 项全部通过，包含原有术士两阶段与五格伤害规则，以及新增的裁切后逐帧 UV、阻挡格排除、播放结束清空检查。结果：`Logs/mage-a-tests.xml`。
- Windows 打包成功：`Logs/mage-a-build.log`，输出 `Build/kait.exe`。
- 七次 Player 检查通过：瞄准、完整十字、靠柱子、靠边缘、弓箭、近战和 12 步移动。未出现检查项中的空引用、越界或断言失败。
- 特效检查调用正式敌人攻击协程；画面固定取最亮帧以便观察。已目视查看 `VFXScreenshots/mage-a-contact.png`，确认人物在裂光上方，墙格和边界外被裁掉。截图不是完整回合录像或性能测试。

本次改动前备份了 KaitGame.cs 与 KaitShatterImporter.cs，目录：`C:/Users/yummn/Downloads/kait-backups/mage-a-before-20260905-084317`。这是本次相关文件备份。原始生成图片保留不变，项目素材为 `Assets/Resources/KaitVisuals/Effects/MageImpactA.png`。

仅更新 Windows 版。
