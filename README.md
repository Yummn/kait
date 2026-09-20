# Kait

> 项目分类：个人项目 / Unity 游戏 Demo / 持续开发中

Kait 是一个把 **2048 合成** 和 **滑行战斗** 结合起来的玩法 Demo。

一次方向输入会同时作用于两部分：角色在战场中直线滑行，数字盘按 2048 规则移动与合并；数字合成会进一步转化为裂隙、敌人刷新和战斗压力。当前版本还加入了主动 / 被动混合牌池，用构筑影响移动、出怪、推动、友伤和技能冷却等规则。

当前源码已迭代到 **0.9.23**。Kait 卡图改为简洁大色块图标，以武器、道具和法术符号为主，少量技能保留黑紫发Q版人物；可下载版本、平台构建和验证状态以 [Releases](https://github.com/Yummn/kait/releases) 中各版本说明为准。

## 想直接玩

### Windows

1. 打开 [Releases](https://github.com/Yummn/kait/releases)。
2. 下载带 `windows` 的完整压缩包并解压。
3. 运行 `kait.exe`。

不要只单独移动 exe；`kait_Data`、`UnityPlayer.dll` 等文件需要和 exe 保持原目录关系。

### Android

如果对应 Release 提供 APK，直接下载并安装即可。

- 横屏运行
- 当前项目面向 ARM64 构建
- Android 最低版本、是否完成真机验收等信息请看对应 Release 说明

## 操作

- `WASD` / 方向键 / 界面方向按钮：移动
- `R`：重新开始
- 鼠标 / 触摸：选择技能、目标和成长

## 玩法简述

- Kait 会沿输入方向滑行，直到碰到边界或无法穿透的单位。
- 数字盘与战场共用同一个方向输入。
- 数字合成后会生成裂隙预警，随后进一步转化为敌人压力。
- 撞击敌人会造成伤害；击杀后可以继续选择方向形成连杀。
- 敌人会按意图执行移动、近战、远程瞄准或范围攻击。
- Kait 每次 `16 + 16 = 32` 时获得一次主动 / 被动混合三选一。
- 主动和被动共享 6 个槽位，可任意搭配并跨类型替换。
- 生命归零失败，击败盾骑士 Boss 获胜。

## 当前内容

- Yummn 56 张技能牌；Kait 保留 47 张身份，普通对局使用 24 张默认池，另有 11 张实验牌与 12 张 Legacy 牌
- 主动 / 被动混合构筑
- 双角色玩法
- Spine 动画
- 拖放选牌与目标指示
- Windows / Android 构建
- 16:9、20:9 横屏与安全区适配
- 基于随机种子和操作序列的可复现对局
- EditMode 规则与资源检查

## 从源码运行

- Unity：`6000.0.30f1`
- 主场景：`Assets/Scenes/Scene.unity`

克隆项目后用对应 Unity 版本打开，运行主场景即可。主要界面由运行时代码创建。

## 代码位置

- `Assets/Scripts/KaitCore.cs`：棋盘、回合、敌人、伤害和技能规则
- `Assets/Scripts/KaitPassives.cs`：被动牌数据与规则
- `Assets/Scripts/KaitGame.cs`：输入、UI、动画和运行时表现
- `Assets/Scripts/KaitMainMenu.cs`：主界面、布局与安全区适配
- `Assets/Tests/Editor`：规则与资源检查

## 版本与开发记录

更细的规则变化、构建状态、兼容性和已知问题不在 README 里堆叠，统一放在：

- [Releases](https://github.com/Yummn/kait/releases)
- [VERSION_HISTORY.md](VERSION_HISTORY.md)
- `Docs/` 下的各版本说明

项目仍在持续迭代，因此源码版本可能领先于最近一个可下载 Release。
