# 敌人落地薄尘 A

更新日期：2026-09-05

采用已确认的 A 方案：脚边左右两团浅色薄尘，不添加出生格子缩放，不遮挡人物。

## 接入方式

- 六种敌人共用已确认的 LandingDustA 图集，保留原有 Spine landing 动画。
- 跟随 landing 动画进度，在约 0.267 秒的触地点开始播放；薄尘持续 0.34 秒。
- 使用人物脚下阴影的落点作为位置基准，宽度为 120 UI 单位，放在人物下层。
- 落地前若动画被攻击、移动等行为打断，或人物已移动、被移除，取消尚未播放的薄尘。不等待特效，不增加输入锁定。

## 验证结果

- Unity EditMode：346 项通过，0 失败，结果见 Logs/landing-a-tests.xml。
- Windows 实际程序：杂兵、剑士、弓手、重甲兵、术士、Boss 六种落地场景全部退出码 0；已检查落地进度、脚边锚点、图集、图层和结束清理。
- 打断落地测试、12 步玩法回归均通过。八份运行日志未检出断言失败及常见运行异常。
- 已查看六种敌人的截图拼图：VFXScreenshots/landing-a-contact.png。截图使用实际 Spine landing 播放，薄尘固定在峰值帧便于核对位置；这不是完整动作视频验证。
- Build/kait.exe 已包含本次接入。使用同一项目本轮完成的 Windows 构建，日志为 Logs/selected-ui-build.log；构建后的 Assembly-CSharp.dll 时间为 09:30:33，并已直接运行该程序完成上述检查。本轮没有生成 Android 版。

## 回退资料

本次修改前的相关脚本备份：

`C:/Users/yummn/Downloads/kait-backups/landing-a-before-20260905-092637`

这是相关脚本备份，不是整个项目备份。复测脚本：Tools/verify_landing_a_runtime.ps1。
