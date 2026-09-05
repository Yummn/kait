# 被动浮动卡片

## 用法

- 获得被动后，卡片收在屏幕上边缘，只露出下半张和名称。
- 鼠标移上可展开查看；触摸屏点按展开，短暂显示后自动收回。
- 可以拖到画面任意位置查看。松手后自动吸附回顶部，横向位置跟随落点，并与其他卡片保持间距；顶部的标题和菜单预留空位。
- 到达被动选择节点时，三张候选卡完整浮出。点按选取，拖动只改变位置，不会选取。选择不消耗回合，也不暂停角色行动。
- 候选卡选择期间，已拥有的卡片暂时收起，避免与候选卡叠在一起；被动效果仍然生效。
- 被动触发时对应卡片短暂展开显示触发次数。

## 画面与结构

技能栏由 250×450 缩短到 250×300，移除原被动槽。WASD 保留原排列，移动栏中心从 (0,-320) 上移至 (0,-162)。两个棋盘尺寸及位置不变。

每张卡片只有一个交互主体。HybridStyleGraphic 现在可同时使用两张独立贴图：斜线左边采样高清卡面，右边采样简约卡面。两种字色使用同一份文字网格。裁切交点不再限制到 0～1，避免拖动时在卡片边缘发生斜率变化。非卡片控件继续使用原来的单素材＋纯色模式。

素材由内置 imagegen 生成，未使用 CLI：

- `Assets/Resources/KaitVisuals/SunlitCourtyard/PassiveCardHD.png`
- `Assets/Resources/KaitVisuals/SunlitCourtyard/PassiveCardFlat.png`

脚本：`KaitPassiveCard.cs`（输入与动画）、`KaitPassiveDeck.cs`（候选 / 持有卡与吸附）、`HybridStyleGraphic.cs`（双皮肤）、`GlobalStyleSplit.cs`（屏幕斜线坐标）。拖动卡片已纳入触摸 UI 判定，不会触发全局滑动移动。

改动前的脚本、着色器与说明备份在 `C:/Users/yummn/Downloads/kait-before-passive-cards-0904.zip`。原卡面、旧像素素材和 VibeGame 工具文件均保留。

## 验证与程序

2026-09-04：254 项 EditMode 测试全部通过，覆盖拖动不误选、顶部半隐藏、吸附间距、双材质跨线切换以及卡片外分割线裁切。Windows 构建成功，已更新 `Build/kait.exe` 及其数据目录；未构建安卓。

实际运行截图：`VFXScreenshots/passive-cards-dock.png`（顶部收纳）、`VFXScreenshots/passive-cards-choose.png`（浮出选择）、`VFXScreenshots/passive-cards-cross.png`（拖到斜线处）。这些截图使用显式预览参数准备局面，正常启动不会自动获得被动。三个运行日志未出现错误或异常。

测试记录：`Logs/passive-cards-tests.xml`。构建记录：`Logs/passive-cards-build.log`。

## 生成提示词

```json
[
  [
    "PassiveCardHD",
    "Use case: ui-mockup. Production Unity 2D passive ability CARD FACE asset, ONE full-bleed portrait rectangular card, aspect ratio 2:3. Not a screenshot, not a collage. Card fills the entire canvas to its four edges, no outside shadow/margin, opaque. No words, numbers or letters: all game text will be real live UI added later. Coordinate composition: top 40% decorative artwork, bottom 60% quiet completely blank surface for name and description and a footer. Thin border around the entire card, modest round corners. ALL important ornament must stay in top 40% or outermost 3% border; do NOT draw any text, labels, fake text or buttons. Japanese anime fantasy high-definition 2D illustrated style matching reference ivory/gold courtyard UI. Warm ivory parchment-like smooth base, finely beveled pale gold frame, delicate botanical corners. Top third: beautiful small ivory-and-gold embossed winged leaf medallion over a restrained dusty blue inset, soft hand painted light, crisp clean contours. Blank lower portion warm cream, not textured too heavily. Refined and readable when scaled to 184 by 264 pixels. Source image 1 is style reference only, not an edit target."
  ],
  [
    "PassiveCardFlat",
    "Use case: ui-mockup. Production Unity 2D passive ability CARD FACE asset, ONE full-bleed portrait rectangular card, aspect ratio 2:3. Not a screenshot, not a collage. Card fills the entire canvas to its four edges, no outside shadow/margin, opaque. No words, numbers or letters: all game text will be real live UI added later. Coordinate composition: top 40% decorative artwork, bottom 60% quiet completely blank surface for name and description and a footer. Thin border around the entire card, modest round corners. ALL important ornament must stay in top 40% or outermost 3% border; do NOT draw any text, labels, fake text or buttons. Minimal flat graphic style matching a charcoal / mauve / peach 2048 game. Main card background solid muted mauve #504551, slim blush peach #FAC7B7 rim and a second restrained line. Top third has a simple flat pale-peach stylized winged leaf glyph (not shaded, not ornate), sitting in a solid slightly darker rounded area. Bottom 60% completely empty mauve. No gradients, no bevels, no gold, no shadows, no texture, no photorealism. Elegant restrained geometry, crisp anti-aliased edges. Corresponding shape and proportions to an illustrated fantasy card, so these are two skins for the SAME draggable card."
  ]
]
```
