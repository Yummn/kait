# 晴日庭院 A：左侧美术替换

参考已确认的 A 方案，将像素庭院替换成暖白石砖、日光草坪、白金面板和蓝金按钮。右侧 2048 的颜色、排布和数字样式不变，角色 Spine 动画、数值与规则不变。

## 素材位置

`Assets/Resources/KaitVisuals/SunlitCourtyard/`

- Garden：独立草坪背景，仅铺在斜线左边。
- Floor：单格石砖，运行时逐格铺设；不是把棋盘烘焙在一张图里。
- Pillar：石墩障碍，仍使用现有柱子坐标和碰撞规则。
- Rift：与地砖一致的裂隙版本，绘制在攻击预警下方。
- Panel / Button：九宫格 UI，避免长面板和按钮把角饰拉伸。

图片使用内置 imagegen 生成，未使用外部 CLI。生成原图保留，Unity 导入使用双线性过滤、无损压缩设置，背景上限 2048，其余上限 1024。运行时素材不依赖用户目录外的临时文件。

旧像素素材仍保留。改动前已将 Git HEAD（dc0a655）打包到 `C:/Users/yummn/Downloads/kait-before-sunlit-A-0904.zip`；此包是已提交版本，不包含原有未提交的 VibeGame 工具文件，那些文件保持原样。

## 交互与字色

按钮沿用 HybridStyleButton，一个主体统一处理点击、按下缩放和变暗，两侧不分别响应。左侧保留素材原色，右侧继续使用原来的状态颜色。中央非按钮文字用一份 Text 网格在斜线处切分字色，左侧棕色、右侧原色。2048 数字不受影响。

三段血条改用金色细框与矩形色段，保留绿色 / 土色 / 银色叠层以及先红后消失的扣血表现。人物缩放、动画时序、残影和特效未改动。

## 检查记录

2026-09-04：244 项 EditMode 测试全部通过。新增检查覆盖 6 张素材的导入设置、九宫格边框尺寸、斜切文字的颜色分区，以及按钮两侧统一的按下与单次点击。

测试结果：`Logs/sunlit-A-final-tests.xml`。Windows 构建输出仍为 `Build/kait.exe`，本次不构建 Android。原有未提交的 VibeGame 相关改动未纳入或覆盖。

最终构建成功；已查看独立 Windows 程序的实机截图：`VFXScreenshots/sunlit-A-final.png`（行动 5 次后）、`VFXScreenshots/sunlit-A-growth.png`（成长选择）。对应运行日志未发现异常。背景、人物、石墩、裂隙分层正常，成长选择保留原来的简约面板。备份中的 261 个 LFS 文件已补入实际素材内容，不只是指针。

## 完整生成提示词

```json
[
  {
    "name": "Garden",
    "prompt": "Use case: stylized-concept. Production 2D Unity game asset, not screenshot/mockup. Image 1 is ONLY the approved art direction reference, especially its LEFT sunlit courtyard side. Match its refined high-resolution Japanese anime fantasy painterly rendering, warm ivory limestone, sage grass, restrained gold. No pixel art, no text, no characters, no UI labels, no collage, no watermark. A single square 1536x1536 top-down background of sunlit sage-green grass garden. Fill entire image with grass, absolutely no board, tiles, plinths, architecture or interface. Dense softly painted leaves and clusters of tiny white/yellow daisies and pale blue flowers confined to outermost 5 percent around all four edges, soft leaf shadows from top left. The inner 85 percent is largely quiet, luminous yellow-sage grass with subtle texture and occasional sparse tiny flowers. No horizon, no perspective. This will be placed behind an interactive square board; foreground decoration must remain at edges. Opaque image, no transparency."
  },
  {
    "name": "Floor",
    "prompt": "Use case: stylized-concept. Production 2D Unity game asset, not screenshot/mockup. Image 1 is ONLY the approved art direction reference, especially its LEFT sunlit courtyard side. Match its refined high-resolution Japanese anime fantasy painterly rendering, warm ivory limestone, sage grass, restrained gold. No pixel art, no text, no characters, no UI labels, no collage, no watermark. A single square top-down warm ivory limestone floor slab game tile. Fill canvas edge-to-edge with this ONE tile, square exactly screen-aligned. Very narrow 2 percent subtly rounded/beveled warm-gold outer stone rim, quiet creamy interior with tiny delicate pale hairline cracks chiefly at corners. No grass, no ground outside the tile, no shadow outside, no symbols. Tile center virtually plain for character readability. No thick dark outline. Opaque ivory canvas including corners, no transparency. 1024x1024."
  },
  {
    "name": "Panel",
    "prompt": "Use case: stylized-concept. Production 2D Unity game asset, not screenshot/mockup. Image 1 is ONLY the approved art direction reference, especially its LEFT sunlit courtyard side. Match its refined high-resolution Japanese anime fantasy painterly rendering, warm ivory limestone, sage grass, restrained gold. No pixel art, no text, no characters, no UI labels, no collage, no watermark. One blank square ivory-and-gold fantasy UI panel for 9-slicing. 1024x1024. Full canvas occupied by panel, outer gold frame reaches image edges (no external margin). Modest rounded corners, slim elegant double gold rim located within outer 4 percent, tiny delicate gold corner flourishes entirely within corner 10 percent zones. Flat warm ivory interior completely blank and uniform, absolutely no text, no icons, no inset panels. NO gradients across full panel; edges must be straight repeatable for 9-slicing. No shadow outside, no wood, no pixel art. Match reference white-gold central UI. Opaque pale ivory even at corner tips."
  },
  {
    "name": "Button",
    "prompt": "Use case: stylized-concept. Production 2D Unity game asset, not screenshot/mockup. Image 1 is ONLY the approved art direction reference, especially its LEFT sunlit courtyard side. Match its refined high-resolution Japanese anime fantasy painterly rendering, warm ivory limestone, sage grass, restrained gold. No pixel art, no text, no characters, no UI labels, no collage, no watermark. One blank square royal-muted-blue and gold fantasy UI button for 9-slicing. 1024x1024. Button fills whole canvas edge-to-edge with no surrounding margins. Slim gold frame in outer 5 percent and delicate gold corners confined to corner 9 percent zones. Smooth muted slate royal blue center (#506891) uniform, no emblem, no text, no icon, no shine stripe. Clear narrow upper bevel and slightly darker lower bevel to suggest clickable normal state. No external drop shadow, no outer background. Restrained polished Japanese anime RPG UI matched to reference small blue buttons. Opaque full square."
  },
  {
    "name": "Pillar",
    "prompt": "Use case: precise-object-edit. Production Unity game ground tile. Input 1 is the approved ONE ivory square limestone floor tile; preserve its exact square footprint, ivory stone edge and quiet background. Output one square tile, not screenshot. No text, no characters, no checkerboard, no transparent-background simulation. Fully OPAQUE tile reaching all four canvas edges. Smooth 2D Japanese anime fantasy art, not pixel, not 3D.\nPlace a low square ivory carved stone obstacle block ON this floor tile. Block fills 90% width and 94% height, centered. Low, squat, solid and clearly impassable. Warm beveled white stone cap, recessed front leaf emblem as Princess Connect guild courtyard architecture, a short darkened block base with very subtle moss. Slightly raised front/top 2D view, no tall column. Keep a tiny visible margin of original ivory floor around the block; absolutely no checkerboard or white background outside floor. Strong silhouette and shaded front to distinguish obstacle from flat tile."
  },
  {
    "name": "Rift",
    "prompt": "Use case: precise-object-edit. Production Unity game ground tile. Input 1 is the approved ONE ivory square limestone floor tile; preserve its exact square footprint, ivory stone edge and quiet background. Output one square tile, not screenshot. No text, no characters, no checkerboard, no transparent-background simulation. Fully OPAQUE tile reaching all four canvas edges. Smooth 2D Japanese anime fantasy art, not pixel, not 3D.\nAdd a dark broken fissure opening in center of this limestone tile. Jagged branching fine cracks, narrow near-black central gap with restrained warm ember-orange rim, like a magical spawning crack in stone. Fissure spreads across center 65% but leaves the same original ivory square tile edge untouched. No red fog, no cloud, no flames, no particles outside tile, no numbers, no portal circle, no raised rock pile. Clearly readable at 144px tile size, restrained small embers localized strictly inside opening. Preserve original ground color for seamless overlay on matching slab."
  }
]
```
