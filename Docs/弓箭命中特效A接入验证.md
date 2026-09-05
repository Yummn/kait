# 弓箭命中特效 A

2026-09-05

采用已选的 A「尖锐穿刺」素材，替换弓箭命中的旧 Spine 特效。箭飞行到目标后播放八帧白金色穿刺闪光，约 0.24 秒。逐帧补偿接触点，四个方向都落在目标迎箭的一侧。

命中敌人时位于敌人上方、Kait 下方；命中 Kait 时位于 Kait 上方。原来的飞行箭、命中音效、伤害规则及 Kait 扣血闪光保留。没有新增等待或输入锁定。

验证结果：

- EditMode 337 项全部通过：`Logs/arrow-a-tests.xml`。
- Windows 打包成功：`Logs/arrow-a-build.log`；输出 `Build/kait.exe`。
- Player 检查四个命中方向、Kait 目标、普通近战命中和 12 步移动，七次均正常退出。检查了贴图加载、父层和特效自动销毁断言，无相关异常。
- 目视检查 `VFXScreenshots/arrow-a-contact.png`：方向对应、落点正确，近战特效仍使用原图。
- 特效截图为调用正式播放方法后固定取峰值帧的诊断画面，用于确认尺寸、位置和层级；不是完整弓手回合录像，也不代表逐帧性能测试。

本次三个脚本的改动前备份位于 `C:/Users/yummn/Downloads/kait-backups/arrow-a-before-20260905-082258`。原始图片保留于生成目录，项目使用 `Assets/Resources/KaitVisuals/Effects/ArrowImpactA.png`。仅更新 Windows 版。
