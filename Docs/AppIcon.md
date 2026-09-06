# Kait 安卓应用图标

## 这一版做了什么

图标使用主界面中 Kait 的紫发、猫耳和紫色眼睛形象，重新绘制为头肩特写。参考日式角色 RPG 的头像式应用图标，保留粗轮廓和大色块，背景沿用游戏的翠绿色。不加标题，以免缩小后文字难以辨认。

- 图标原图：`Assets/Art/AppIcon/KaitAppIcon.png`
- 自适应底色：`Assets/Art/AppIcon/AdaptiveMint.asset`
- 配置脚本：`Assets/Editor/KaitAppIconSettings.cs`
- 安装包：`Build/kait-v0.5.3.1.apk`
- 版本号：0.5.3.1，Android versionCode：504。

传统图标、圆形图标和 Android 8 及以上的自适应图标都已配置。自适应图标前景内缩 16.6667%，给系统裁切留出空间。原图没有预先画圆角，由手机桌面决定最终外形。上一版 APK 保留，本次没有改游戏规则，也没有重新发布 GitHub Release。

## 检查记录

- Unity Android 构建成功。
- 已查看 Unity 导出的 192 × 192 图标，头像清晰可辨。
- 已检查 APK 内自适应图标 XML，包含头像前景、翠绿底色和安全区内缩。
- APK v2 签名校验通过，与 v0.5.3 使用同一证书、同一包名，可作为更新包安装。
- 当前 ADB 没有连接设备，未做手机桌面显示及真机启动验证。
- 图标新增测试 2/2 通过。全量测试首次 406/407，通过复跑为 405/407；未通过项是原有 `KaitCardAudioTests.ManualDockConsumesSnapOnceAtArrival` 的主动/被动卡片用例（77、87 行）。这些用例手动调用依赖真实 `Time.unscaledDeltaTime` 的 Update，并断言单次调用后仍未吸附；帧间隔可能影响结果，尚未完成独立根因验证。本次没有改卡片代码，不能把全量回归标为通过。日志在 `Logs/app-icon-tests.xml` 和 `Logs/app-icon-retest.xml`。
- SHA256：`AA6223D7B1F04683C4741521CC7B83654CAF733E8D6816C894DC4C2977F1A898`

## 素材生成记录

使用内置图像生成工具，参考输入为 `Assets/Resources/KaitVisuals/MainMenu/CourtyardAB.png`。并非从《公主连结》图标中提取角色。

生成提示词：

```text
Use case: identity-preserve. Asset type: final square mobile game launcher icon, 1024x1024. Reference image is supporting CHARACTER IDENTITY input only. Use exactly the purple-eyed chibi catgirl Kait visible on the left: preserve her purple layered hair, white inner cat ears, large violet eyes, friendly confident small smile, distinctive bangs and face, white-black-gold collar. Reframe into a crisp close-up HEAD AND SHOULDERS portrait facing viewer, no sword, no hands, no full body. The head is the icon, like a Japanese anime RPG portrait app icon. Chunky painted anime chibi style, bold clean dark outlines, large color shapes, clear highlights, readable at 48px. All essential face and ears fit within central 66 percent of canvas; surrounding hair and shoulders may extend farther. Eyes near horizontal center, face visually centered. Simple full-bleed light emerald/mint backdrop with one broad pale warm highlight behind head, very few shapes, no detailed scenery. SQUARE opaque image filled edge to edge. No baked rounded corners, no frame, no text, no letters, no logo, no UI, no diagonal divider, no tiny clutter. One finished icon only, not a mockup sheet.
```
