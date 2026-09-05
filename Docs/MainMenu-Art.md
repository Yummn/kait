# 主界面素材

采用确认后的组合：A 版人物，B 版庭院与斜切构图。人物保留紫瞳、正面驻剑姿势；右侧沿用烟墨色和桃粉色。

游戏背景位于 `Assets/Resources/KaitVisuals/MainMenu/CourtyardAB.png`，由内置图像生成工具基于确认图编辑生成。人物和标题暂时保留在插画中，没有另接一套主界面 Spine 动画。三个按钮及中文标签是独立的 Unity UI，不是图片上的点击热区，可以在 `KaitMainMenu.cs` 中调整尺寸、位置和文案。

背景不生成 Mipmap，Windows 使用未压缩纹理，安卓使用 ASTC 4×4。菜单以 1920×1080 为设计尺寸，按安全区等比缩放，超宽屏和不同长宽比的剩余区域使用烟墨色补边，不拉伸人物。

## 本次背景编辑提示词

Use case: precise-object-edit. Production background for Unity main menu. Change ONLY the three buttons on the right beneath the Kait title: remove the entire peach start button with its border/text, remove the tutorial button border/text, remove the settings button border/text. Fill their former areas seamlessly with the existing smoky-plum background. Leave the large 'Kait' title exactly as is. Preserve everything else as faithfully as possible: purple-eyed catgirl, her upright sword and pose, left courtyard, green trees, architecture, diagonal peach seam and its position, decorative small squares on right, lighting, aspect ratio, original composition. No new text, buttons, objects. Output one clean background image ready for separate live Unity UI buttons, landscape 16:9.
