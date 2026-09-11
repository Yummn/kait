# Yummn 雪庭：A 的衔接，B 的配色

## 本次范围

采用已确认的组合预览：冷蓝灰地砖和场景配色，外缘用薄而不规则的积雪连接背景；墙脚采用短接触阴影。不增加外围围墙、台阶或悬空底座。

只调整 Yummn 场景。Kait 翠庭、人物动作、右侧 2048 盘、角色坐标与碰撞规则不变。主副盘障碍仍为（行，列）（1，1）、（5，5）。

## 分层方式

- `Ground.png`：连续的蓝灰石庭背景，按原图比例铺满左侧。
- `Floor.png`：单格石板，逐格铺设，不将预览截图直接当作棋盘。
- `Pillar.png`：独立障碍石块，底部接触阴影与素材分开。图像工具两次未输出真实透明通道，最终让工具生成纯黑底版本，由游戏材质隐藏黑底；没有本地修改图片像素或使用带棋盘格的素材。
- `SnowMask.png`：手绘积雪覆盖贴图，白色表示积雪、黑色表示不覆盖；着色器读取覆盖量，不把黑底画到画面上。

积雪边缘仅用于外圈 16 个格子、20 条边。它位于地砖上方、障碍和预警下方，不参与鼠标点击和碰撞。没有重新给人物加遮罩。

素材目录：`Assets/Resources/KaitVisuals/Yummn/SnowCourtyard`。接入逻辑为 `YummnSnowCourtyard.cs` 和 `KaitGame.Yummn.cs`，积雪覆盖着色器为 `UISnowCoverage.shader`。

切换回 Kait 时恢复原地砖、石块尺寸、阴影位置，并隐藏雪边。原背景保留为 `Assets/Resources/KaitVisuals/Yummn/Courtyard.png`，本轮不覆盖它。

## 备份

修改前备份：`Backups/before-snow-courtyard-20260909-124722`，包含相关脚本、原雪庭背景和说明文档。没有上传 GitHub。

## 生成记录

素材使用内置图像生成工具，参考选中的 A+B 组合预览；没有调用收费 API 脚本，也没有从预览中截取人物或 UI。完整提示词记录在同目录 `Yummn雪庭-素材提示词.md`。

## 实机调整与检查

第一轮实机中，地砖缩小后细碎纹理偏多，连续的雪边也有白色框线感。因此重新生成了更平整的大色块地砖，让雪边宽窄交错并留出间断。仅新场景贴图启用 mipmap 和三线性过滤，人物、字体、卡牌的导入设置没有变动。

最终 EditMode 回归测试通过 549 项，失败 0 项，结果位于 `Logs/snow-courtyard-final-tests.xml`。其中新增检查覆盖素材导入设置、外围 16 格的 20 条雪边、Kait 场景恢复，以及装饰不改变碰撞坐标。

Windows 程序已更新至 `Build/kait.exe`。最终运行检查正常退出（退出码 0），日志记录 `YUMMN_QA_COMPLETE`，没有报告异常或检查错误。已检查初始棋盘、两次行动后的预警层和切回 Kait 的实机截图：石块黑底未显示，人物及预警可正常显示，翠庭未残留雪层。测试结束后已恢复测试前的本机存档。

- 实机画面：`VFXScreenshots/snow-courtyard-final.png`
- 行动与预警：`VFXScreenshots/snow-courtyard-final.png.after-two-actions.png`
- Kait 恢复检查：`VFXScreenshots/snow-courtyard-final.png.kait-regression.png`
- 打包日志：`Logs/snow-courtyard-final-build.log`
- 运行日志：`Logs/snow-courtyard-final-runtime.log`

本轮未打包 Android，也未上传 GitHub。
