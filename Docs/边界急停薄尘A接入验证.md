# 边界急停薄尘 A

更新日期：2026-09-05

采用确认的 A 方案，替换边界停止时原来的 Spine 浓烟。使用原图 BoundaryDustA.png，没有重新绘制或修改素材像素。

## 表现

- 保留现有停止判定、停止动画、音效与输入时序，只替换烟尘显示。
- 接触点对齐 Kait 脚下阴影；136 UI 单位图集画布，实际图形比一格小，持续 0.38 秒。
- 水平移动时薄尘朝身后展开；向左使用水平镜像，不倒置烟尘。
- 上下方向沿地面纵深展开，并略向脚侧偏移，避免全被人物遮住。
- 放在所有人物下层，不挡点击，不锁定输入。触发后留在原地，自行消散，不跟随下一次移动。
- 踏影等其他行为仍保留自己的效果，本次没有替换。

## 验证

- 全量 EditMode 测试 364 项通过，0 失败，见 Logs/boundary-a-tests.xml。
- 上下方向可见性调整后，6 项专项测试全部通过，见 Logs/boundary-a-final-tests.xml。
- Tools/verify_boundary_a_runtime.ps1 使用 Build/kait.exe 检查右、左、上、下四次实际滑行到边界，以及落地、命中、12 步游玩回归。四方向检查走现有 AnimateKateSlide 停止分支，并验证图层、脚边锚点和自动清理。
- 最终构建的七项运行检查全部退出码 0，日志未检出断言失败及常见运行异常；已查看最终四方向截图。Assembly-CSharp.dll 更新时间为 2026-09-05 09:57:49。
- 截图固定薄尘在峰值帧以核对位置，人物与残影来自实际滑行。截图不是完整动作视频。
- 四方向截图：VFXScreenshots/boundary-a-contact.png。
- Windows 构建日志：Logs/boundary-a-build.log；没有生成 Android 版。

## 回退资料

相关脚本修改前备份：

`C:/Users/yummn/Downloads/kait-backups/boundary-a-before-20260905-094527`

这是相关脚本备份，不是全项目备份。已确认素材的原文件保留在生成目录，项目内副本与原图 SHA256 一致。
