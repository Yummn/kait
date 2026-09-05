# 版本记录

这里记录的是可以直接回退的标签，不是每一次零碎提交。`backup-*` 是改大规则前留下的保险点，`v*` 是当时可以单独检查的阶段版本。

## 最新归档

| 标签 | 说明 |
| --- | --- |
| `v0.5.1` | 翠绿庭院、双画风浮动卡片、分层阴影和新一轮视听反馈；惊惧斩 A 与上下急停烟尘修正。冰墓新美术尚未接入。详见 [版本说明](Docs/Release-v0.5.1.md)。 |

## 早期规则阶段

| 标签 | 说明 |
| --- | --- |
| `backup-v0.2-before-v0.3-20260830` | v0.2 收尾备份，裂隙阻挡和预警显示已经整理过。 |
| `backup-v0.3-before-v0.3.1-20260830` | 升级高耐久与自由转向前的 v0.3。 |
| `backup-v0.3.1-before-v0.3.3-20260830` | v0.3.1 规则稳定点，准备进入双盘回合实验。 |
| `backup-v0.3.3-before-v0.3.4-20260830` | 双盘回合语义完成，保留人物脚下裂隙提示。 |
| `backup-v0.3.4-before-v0.3.5-20260830` | 锁定冲锋强度与地图修正后的备份。 |
| `backup-v0.3.5-before-v0.3.6-20260831` | 双盘共享柱子实验结束时的版本。 |
| `backup-v0.3.6-before-coordinate-fix-20260831` | 碰撞击杀和弓手节奏完成，尚未修正行列坐标。 |
| `backup-v0.3.6-before-v0.3.7-20260831` | 主副棋盘柱子坐标修正后的 v0.3.6。 |
| `v0.3.7` | 加入三阶段技能成长和盾骑士 Boss。 |
| `backup-v0.3.7-before-v0.4-20260831` | 修好技能按钮后，进入 UI 重做前的备份。 |
| `backup-v0.4-before-visual-rework-20260831` | v0.4 初版，规则和非阻塞技能选择已经接通。 |
| `v0.4` | v0.4 的正式规则节点。 |
| `backup-before-makoto-spine-20260831` | 接入角色 Spine 前的视觉版本。 |
| `v0.4-visual-refresh` | 棋盘、预警、教程和单位信息完成第一轮整理。 |
| `v0.4-makoto-spine` | 主角首次完整接入 Spine 动画。 |
| `backup-before-1920-visual-20260901` | 调整到 1920×1080 前的角色居中版本。 |

## v0.4 视觉与交互迭代

| 标签 | 说明 |
| --- | --- |
| `v0.4.1-character-centering` | 按人物身体而不是武器范围做视觉居中。 |
| `v0.4.2-1080p-movement-visuals` | 改为 1080p 布局，更新移动、停止和拖影。 |
| `v0.4.3-fullscreen-trails` | 修正全屏显示，并补足 Kait 残影。 |
| `v0.4.4-kait-damage-feedback` | Kait 与敌人的受击反馈分开处理。 |
| `v0.4.5-movement-feedback` | 普通撞墙停止和连杀转向都有明确动画。 |
| `v0.4.6-enemy-roster` | 按点数重新整理敌人职业、数值和角色素材。 |
| `v0.4.7-unit-scale` | 杂兵换成 100161，敌我单位尺寸统一。 |
| `v0.4.8-enemy-animation` | 敌人接入出生、待机、攻击和受击动画。 |
| `v0.4.9-attack-warning` | 攻击预警换成条纹覆盖。 |
| `v0.4.10-dungeon-tiles` | 战场第一次换成地牢地砖。 |
| `v0.4.11-warning-clip` | 预警按格子方向对齐，并限制在圆角范围内。 |
| `v0.4.12-enemy-scale` | 统一敌人的视觉中心与大小。 |
| `v0.4.13-five-by-five-tiles` | 单格改为 5×5 小砖拼接。 |
| `v0.4.14-impact-warning` | 攻击瞬间由警示线变色提示。 |
| `v0.4.15-town-tiles` | 战场换成城镇草地自动图块。 |
| `v0.4.16-town-rift` | 裂隙图像融入草地地砖。 |
| `v0.4.17-ranged-cadence` | 弓手和术士改成瞄准、攻击交替执行。 |
| `v0.4.18-body-centered-unmasked` | 取消人物遮罩，按身体中心固定位置。 |
| `v0.4.19-center-floor` | 更换 3×3 地砖的中心块。 |
| `v0.4.20-three-column-layout` | 布局整理为主棋盘、技能栏、副棋盘三列。 |
| `v0.4.21-dungeon-ui` | 面板、按钮和血条换成地牢 UI 素材。 |
| `v0.4.22-grass-background` | 全局背景铺成草坪。 |
| `v0.4.23-rift-layering` | 人物显示在裂隙上方，并取消当时的出生格缩放。 |
| `v0.4.24-segmented-health` | 加入绿色、土色、银色三段式血条。 |
| `v0.4.25-unit-feedback` | 命中变色和缩放从格子移到人物身上。 |
| `v0.4.26-pixel-font` | UI 改用中文像素字体。 |
| `v0.4.27-ui-atlas` | 面板和按钮正式使用地牢图集及按下状态。 |
| `v0.4.28-wall-stop-speed` | 撞墙停止动画加速。 |
| `v0.4.29-kill-feedback` | 击杀使用连杀攻击，并加入逐次升调音效。 |
| `v0.4.30-health-bar-alignment` | 敌人血条重新居中，移除 Kait 头顶血条。 |
| `v0.4.31-board-on-grass` | 去掉战场背景板，格子直接接草地。 |
| `v0.4.32-stone-fence` | 5×5 战场外加入石围栏。 |
| `v0.4.33-crisp-font` | 调整像素字体采样，减少模糊。 |
| `v0.4.34-chain-shake` | 连杀加入轻微屏幕震动。 |
| `v0.4.35-rift-shadow-cleanup` | 清理裂隙左侧红色阴影。 |
| `v0.4.36-enemy-attack-ready-spine` | 敌人准备攻击时加入 Spine 特效。 |
| `v0.4.37-town-stone-fence` | 围栏换成城镇素材中的石墙。 |
| `v0.4.38-kait-unmasked` | 清除 Kait 武器上的残留裁剪。 |
| `v0.4.39-rift-without-label` | 移除裂隙倒计时数字。 |
| `v0.4.40-contained-ui-text` | 标题居中，文字限制在边框内。 |
| `v0.4.41-no-attack-ready-effect` | 暂时取消准备攻击特效。 |
| `v0.4.42-unit-impact-highlights` | 推动和击杀改为人物高亮，不再亮格子。 |
| `v0.4.43-three-tiles-per-cell-wall` | 围墙按每格三块小砖重新搭建。 |
| `v0.4.44-upright-side-wall-caps` | 两侧墙保持亮面朝上，用堆叠补足高度。 |
| `v0.4.45-white-hit-flash` | 所有受击闪光统一为白色，并修正闪错单位。 |
| `v0.4.46-enemy-no-clipping` | 敌人 Spine 关闭 UI 与 Spine 裁剪。 |
| `v0.4.47-enemy-death-ready-animations` | 敌人接入死亡和准备攻击动画。 |
| `v0.4.48-pixel-font-outlines` | 常用 UI 文字加入黑色描边。 |
| `v0.4.49-health-bar-layout` | 血条放到人物下方，Kait 使用同款栏内血条。 |
| `v0.4.50-no-chain-red-flash` | 移除连杀时的全屏红光。 |
| `v0.4.51-buffered-animation-input` | 曾尝试加入动画期间的方向缓冲。 |
| `v0.4.52-larger-guard-visual` | 单独放大视觉偏小的重甲兵。 |
| `v0.4.53-text-safe-area` | 再次收紧描边文字的安全区域。 |
| `v0.4.54-dense-kait-trails` | 残影数量翻倍，并按速度改变颜色。 |
| `v0.4.55-chain-speed-aura` | 连杀等待转向时加入速度残影提示。 |
| `v0.4.56-rift-top-cleanup` | 裁掉裂隙顶部残留的红色像素。 |
| `v0.4.57-no-input-buffer` | 取消方向输入缓冲，回到直接输入。 |
| `v0.4.58-mid-chain-skills` | 连杀途中也可以随时释放主动技能。 |
| `v0.4.59-chain-wall-animations` | 等待转向和撞墙停止换用新的 Spine 动画。 |
| `v0.4.60-no-chain-aura` | 移除会拖慢转向的等待残影。 |
| `v0.4.61-no-brick-fence` | 取消战场砖墙。 |
| `v0.4.62-interruptible-chain-pose` | 击杀反馈结束后立即开放连杀方向输入。 |
| `v0.4.63-decoupled-chain-pose` | 转向待机与敌人死亡动画分开计时。 |
| `v0.4.64-layered-death-fade` | 敌人在原地播放死亡动画，位于 Kait 下方并淡出。 |
| `v0.4.65-seamless-enemy-death` | 补上致死白闪，消除攻击与死亡之间的空帧。 |
| `v0.4.66-complete-death-transition` | 所有收尾路径都保持敌人可见直到死亡开始。 |
| `v0.4.67-parallel-death-animation` | 死亡动画与真正致死的攻击同时播放。 |
| `v0.4.68-kait-actor-overlay` | Kait 提到整块战场的角色层，武器不再被相邻地砖盖住。 |
| `v0.4.69-no-enemy-cell-scale` | 删除敌人正式出现时的格子放大。 |
| `v0.4.70-quick-rules-panel` | 右上角加入常驻规则速览。 |
| `v0.4.71-interruptible-kait` | 场上动画与操作解耦，方向输入可以打断 Kait 当前演出。 |

需要回看某一版时，可以直接执行：

```bash
git switch --detach 标签名
```

回到最新开发分支：

```bash
git switch master
```

## v0.5 被动构筑

| 标签 | 说明 |
| --- | --- |
| `v0.4-pre-v0.5` | 开始改被动系统前的完整 v0.4 备份，包含当时的视听反馈、语音和双盘界面。 |
| `v0.5` | 加入 12 张被动牌、三次被动三选一、跨盘联动、被动槽与触发提示。 |
