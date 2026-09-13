# Kait 共享修改清单

最后更新：2026-09-13（北京时间）。维护者：Yummn全局时序v0.9.0与审批A卡图接入会话。

这是多个 Codex 任务的本地交接入口。打开同一个工程的任务共读此文件；不同目录的项目副本或远程任务不会自动同步。当前用户请求优先；文中的待办和其他会话记录不是执行授权。

## 1. 工程与交付快照

| 项目 | 已核实状态 / 证据 |
| --- | --- |
| 工程 | C:/Users/yummn/Downloads/kait |
| Unity | 6000.0.30f1；C:/Unity/6000.0.30f1/Editor/Unity.exe |
| 场景 | Assets/Scenes/Scene.unity |
| 本地规则版本 | 应用0.9.0，Yummn牌池0.9.0-root-action，共56牌（15主动/41被动，11普通/33非凡/12稀有）；规则快照yummn-0.9.0-root-action，独立存档Kait.Run.Yummn.0.9.0。旧存档保留，Kait规则不变 |
| Git 基线 | 本轮本地HEAD为510611a；0.9.0源码/卡图/说明尚未上传。本轮未核对远端公开性。历史上传记录仍见各任务；AudioPreviews/VFXPreviews选稿仅本地 |
| 最近 Windows 构建 | Build/kait.exe；Logs/root090-art-final-build.log包含Kait build created并成功退出；0.9.0时序和56张审批A卡图。resources.assets为2026-09-13 17:16:30，程序集17:11:11；Android现已同步0.9.0 |
| 最近 EditMode 测试 | Logs/root090-handoff-tests.xml：135/135专项与Kait核心/重开回归通过。此前全量1064项979通过85失败，包含旧规则/UI断言，未全部核定原因，不能表述为全量通过 |
| 本轮运行检查 | enemy-aim-runtime.log：WARNINGS_QA_COMPLETE；六兵种、出手变黄、三敌重叠、盾骑士四方向、死亡清理及气氛特效。已看六兵种/术士黄色/重叠/上下方向截图。34项玩家设置存档按类型和值还原，非Android验收 |
| 最近视觉运行检查 | root090-runtime.log：ROOT090_QA_COMPLETE；实际Windows主先副后、推动/追身/反击链、56张新卡图加载通过，root090-cards.png已看；40项偏好/存档原类型原值还原。R28内部假透明棋盘已本地清理并看图；非Android验收 |
| 最新 Android | Build/kait-v0.9.0.apk，2026-09-13 17:25:35，253232326字节；root090-android-build.log成功退出0。版本900、最低API26、目标API35、ARM64、UnityPlayerActivity；v2签名有效且与0.8.3相同。SHA256 A21EAF07007A3D16FF6B472796BE98FB76135FF83D33C3A37E3DCD94094A7482。旧0.8.3包保留；ADB无设备，未真机验收，未上传GitHub |
| Android | Build/kait-v0.8.2.apk，2026-09-12 23:52:44，234614258字节；Logs/android-exit-stun-20260912.log成功退出0；同步主动技能拖放分类、R40借机攻击及整回合震慑。版本802、Android 8.0+、ARM64、UnityPlayerActivity、v2签名有效且签名与旧包一致。SHA256 84B07BFDDB9A8CADB451C0A1034BC34B3BFC7B2D315D14064EE4E7D0DB8ADBA6；ADB无设备未真机验收，未上传GitHub |
| 历史文档 | README / VERSION_HISTORY 中部分内容描述 v0.6.1 归档，不是当前未提交工作树的完整说明 |

Windows 交付必须带整个 Build 内的关联文件，不能只拷贝 kait.exe。构建后核对日志和 kait_Data 中资源/程序集时间，不只看启动 exe 的时间。

## 2. 进行中与占用登记

RELEASE-20260913-ROOT090：进行中，本会话。用户授权上传GitHub并说明；已核对origin/master与本地HEAD均510611a、Yummn/kait为PUBLIC，保持公开性。上传当前源码、正式资源、相关说明与APK；缓存、玩家存档、未审批预览不上传，无Unity占用。

BUILD-20260913-ROOT090-ANDROID：完成，无Unity占用。当前0.9.0时序与56张审批A卡图已打包；Logs/root090-android-build.log包含Kait Android build created并退出0。版本/架构/启动入口/v2签名核验通过，签名与旧0.8.3一致；包与SHA256见最新Android快照。旧包保留，不改规则、不上传GitHub；ADB无设备，未真机启动/触摸验收。上轮135项专项通过、全量仍85失败及偶发运行QA超时记录仍保留，不因打包成功视为已解决。

RULE-20260913-ROOT090-ART：完成本轮源码/卡图/Windows接入，无Unity/游戏占用。本地0.8.3基础上实施0.9.0 RootAction；人物先行、合成攻击/补给批次、唯一出生窗口/敌方阶段、末尾回气与阶段固定；拳掌与踢击分离、进入反击/提交后离开踢击、最终落点追身、伤害包及碎冰反伤、命令/误导时序。收尾修正摆锤反向共鸣及实际移动扣气/连锁后继续原方向。接入56张获批A图（透明、无固定卡框），简约图保持；R28假透明棋盘已清理看图；R40踢击仍用获批出拳图，待重画审批。备份Backups/Root090-before-20260913；保留全部原脏工作。root090-handoff-tests.xml 135/135专项和Kait回归通过；此前全量1064项979通过85失败，含历史规则/UI断言，未全部核定原因，未删除/屏蔽。root090-art-final-build.log最终成功；root090-runtime.log最新ROOT090_QA_COMPLETE，56图加载、主先副后、推动追身反击链通过，root090-cards.png已看，40项偏好恢复。最终素材构建后首次QA在PlayTurn等待超过45秒，脚本结束自己的进程并恢复偏好；未改源码重试约8秒通过，原因未复现，保留为偶发现象记录。不Android/不上传。详见Yummn-v0.9.0-RootAction与56卡图.md。

RULE-20260913-POOL083：完成源码/Windows/Android，无Unity占用。56牌含15主动/41被动，普通10/非凡34/稀有12；合成/残影/反击链、特定目标交互、共享版本0.8.3。35/35专项通过（pool083-final-tests.xml）；pool083-verified-build.log Windows成功退出0，pool083-runtime.log POOL083_QA_COMPLETE，两张最终截图已看，40项偏好恢复。初轮QA脚本缺日志路径已修正。旧存档保留并用InspectYummnReplay.ps1实际读取验证，新池单独键需新开局；备份Backups/Pool083-before-20260913。右盘点击目标保留普通滑动。复用获批图标/特效，保留前轮重开修复及Kait规则。Android最终日志pool083-final-android.log成功，包与签名见最新Android快照。ADB无设备，未真机验收，不上传GitHub。详见Yummn目标技能池-v0.8.3-20260913.md。

FIX-20260913-RESTART：完成源码/Windows；根因是NewRun停止协程后未清animatedEnemies/animatedSpawns/displayedThreat，RefreshBattle重新按旧快照创建敌人；同时漏清常驻战斗特效与Yummn独立Update特效。新增ResetRunPresentation统一丢弃旧快照、显示标记、输入缓冲、持续特效、地形/掌印池及气氛状态，销毁前同步隐藏；回气浮字纳入瞬态清理。restart-tests.xml 1/1专项通过（循环3次）；restart-runtime.log RESTART_QA_COMPLETE，实际Windows交替Kait/Yummn强制失败并NewRun共6轮，验证无旧敌人重建、无旧快照或特效；restart-final.png已看。40项玩家偏好/存档原类型原值恢复。restart-build.log构建成功退出0。Android未更新、未上传GitHub，无Unity/游戏占用。

DOC-20260912-CURRENT-DESIGN-GITHUB：已完成；已生成 `Kait_当前版本游戏设计文档_v0.8.2.docx`，覆盖双盘规则、Kait/Yummn角色系统、六类敌人、设置、存档、构建与完整技能牌池（Kait 39张，Yummn 38张，共77张，其中Kait实验牌1张默认关闭）。DOCX共12页，已逐页渲染检查，无表格截断、溢出或缺字；技能关键项文本复核无遗漏。当前源码、资源、测试、专题说明、文档及文档生成脚本一并提交到现有 `origin/master`；本地试听/特效预览缓存继续忽略。无Unity占用。

BUILD-20260912-EXIT-STUN-ANDROID：完成；当前源码已生成Build/kait-v0.8.2.apk，日志android-exit-stun-20260912.log Result Success退出0。包体234614258字节，SHA256 84B07BFDDB9A8CADB451C0A1034BC34B3BFC7B2D315D14064EE4E7D0DB8ADBA6；版本802、最低API26、目标API35、ARM64、UnityPlayerActivity，v2签名有效且证书与旧包一致。旧包保留为Build/kait-v0.8.2-before-exit-stun-20260912.apk。ADB无设备，未真机启动/触摸验收；不上传GitHub，无Unity占用。

RULE-20260912-EXIT-STUN：完成源码/Windows/Android；R40借机攻击独立罕见被动，敌人离开四邻前统一拳击，含推动、不含人物自己离开；每敌人每操作限一次防递归，保留伺机而动。复用获批反击图标，38牌。震慑覆盖一个敌方阶段，阶段末解除，重复命中不刷新/扣气并立即清预警；表现快照同步状态，解除震慑不误清冰冻。exit-stun-tests.xml 67/67专项通过；Windows及Android构建成功。未本轮运行视觉/真机验收，无Unity占用，不上传。新牌池0.8.2-exit-stun需新开Yummn局；详见借机攻击与整回合震慑-20260912.md。此前INPUT-DRAG-TARGET借机攻击待确认已由此次用户明确离开触发解决。

INPUT-20260912-DRAG-TARGET：交互部分完成源码/Windows；点击卡牌仅预览，拖放后方向技能四向选、疗伤点自身、冰柱/暗幕/诱饵点任意合法空格，教程同步。drag-target-tests.xml 59/59专项通过；drag-target-build.log成功退出0；repool-runtime.log REPOOL_QA_COMPLETE，真实卡牌拖放及自身/目标格/方向按钮回调通过，三张drag-target截图已看，40项偏好按类型和值还原。借机攻击与现有伺机而动重复，改名或独立双卡待用户确认，尚未改牌池。无Unity占用，不Android/上传。详见主动技能拖放与目标分类-20260912.md。

RULE-20260912-PUNCH-UNIFY：完成源码/Windows/Android；原O04退出牌池、R06改名无甲防御，共37牌；等待基础补2；新入四邻反击走统一拳击，每敌人每操作限一次；二连拳总2气、不足默认普通拳；有效位移起点残影含气竭/击杀跟进/追身/传送。Yummn所有主动四向点选，阻止移动/长按/等待误输入，失败提示并退出，不扣气回合；卡牌与教程字号放大。59/59专项，Windows运行看图通过，修复首轮引用隐藏边界定位异常，40项偏好还原。APK签名/入口/ARM64通过，旧包before-punch-unified-20260912.apk保留；ADB无设备，未真机验收。新池0.8.2-punch-unified需新开Yummn局；不上传GitHub，无Unity占用。详见Yummn拳击与点选施放-20260912.md。

BUILD-20260912-CARDS-ANDROID：完成；当前38牌正式名称/图标及此前规则已打包，日志Logs/android-formal-cards-20260912.log成功退出0；签名与旧包一致、UnityPlayerActivity/ARM64/版本802检查通过。旧包Build/kait-v0.8.2-before-formal-cards-20260912.apk保留。ADB无设备，未真机启动/触摸验收。不修改游戏规则，不上传GitHub，无Unity占用。

CARD-20260912-NAMES：完成源码/Windows/运行看图；正式38牌名称，移除R18留影不散及其耐受效果，R19万影长存改为唯一暗影斗篷；稳定ID绑定纠正旧图错位，内置图像生成13张透明卡图，原件保留1254、导入512。40/40专项通过，formal-cards-build.log成功，formal-cards-runtime.log完成，38图标总览与长标题截图已看；40项偏好还原。牌池版本0.8.2-repool-names需新开Yummn局，旧存档未删除；其余效果/稀有度及Kait不变。不Android/上传，无Unity占用。详见Yummn正式名称与图标复核-20260912.md。

ANIM-20260912-ENDPOSE：完成源码/Windows；KaitSpineView终局动画一次播放不排队待机，终局锁阻止迟到的待机/攻击和重复胜利覆盖，重新开局重置。胜利由循环改为一次；敌人原死亡后淡出不变。endpose-tests.xml 32/33，新4项末帧/无后续轨道/刷新不覆盖/重开恢复通过；旧Flurry技能测试失败如上。endpose-build.log成功退出；未实机视觉验收，不Android/上传，无Unity占用。

本轮登记 RULE-20260912-REWARD16：已完成源码/Windows；默认8+8得牌，Yummn独立设置开启后16+16得牌，下局生效；Kait和旧存档32不变。教程/规则快照/成绩键同步。reward16-final-tests.xml 6/6，reward16-build.log成功退出；未运行实机视觉验收，不Android、不上传，无Unity占用。详见Yummn选牌阈值-20260912.md。

| 任务编号 | 负责人 / 会话 | 范围 | 状态 / 占用 |
| --- | --- | --- | --- |
| RULE-20260912-REPOOL | 本会话 | Yummn获批39变体/六混合槽/冻结残影规则、ABABAA特效与卡图 | 完成源码/Windows；39张独立命名及稀有度，保留Kait池与3+3；残影基础一击、被动回合无限，冻结永久下一击免伤解冻，冰柱/暗幕2气。六获批图集与六新透明卡图接入，其余复用获批图标。58/58专项，repool-build3.log成功，repool-runtime.log完成并看图；37项偏好恢复。脚本/测试备份Backups/Repool-before-20260912；新池0.8.2-repool需Yummn新局，旧存档不删除。不Android、不上传，无Unity占用。详见Yummn新版牌池与选稿接入-20260912.md |
| PREVIEW-20260912-REPOOL | 本会话 | 六组特效ABC与配套卡图选稿 | 完成待选：VFXPreviews/Repool-20260912，18张八帧图集/六张ABC GIF/六张透明卡图。内置image_gen，透明通道与GIF循环检查、七张预览峰值图已看；非游戏实录。规则确认写入README但尚未实施：基础残影一击、技能后本敌方回合无限、冰柱/暗幕2气，冰柱复用冰冻。六槽/新牌池与完整变体卡图待后续；未改正式素材/源码/Windows/Android/上传，无Unity占用 |
| BUILD-20260912-CLOCK-ANDROID | 本会话 | 当前版本Android打包 | 完成；android-clock-edge-20260912.log Success退出0，签名/入口/ARM64验证通过。旧包Build/kait-v0.8.2-before-clock-edge-20260912.apk保留；新包见上方。adb无设备未真机验收，不上传，无Unity占用 |
| VFX-20260912-CLOCK-EDGE | 本会话 | Kait时停左屏边缘时钟渐增弧线 | 完成源码/Windows；复用获批ClockA，48对象池、1.2秒后错序渐增约10秒满，上下左边+左侧两角双层弧线，大小倾角与动画相位错开，左侧裁切；Yummn及灰边不变。clock-edge-tests.xml 14/14，clock-edge-build.log Success正常退出。未运行视觉验收，不Android/上传，无Unity占用 |
| RULE-20260912-BOSS128 | 本会话 | 双角色128 Boss、Kait有效移动补2开关 | 完成源码/Windows：新局winValue128、触发与普通裂隙共用门槛，旧回放保留配置256；Kait独立默认关闭、即时记录开关，整回合移动后结束补一次、时停不补。教程映射与说明同步。boss128-tests.xml 8/8；final-tests 38/39，本次规则/设置/教程通过，唯一失败是旧FivePipsFitHudAndOldSaveOnlyShowsThree居中前坐标断言，未改。boss128-build.log Success退出0。未运行视觉验收、不Android/上传，无Unity占用 |
| UI-20260912-KI789 | 本会话 | 气上限追加7/8/9，设置/快照/气槽与验证 | 完成源码/Windows：3～9循环，默认6不变、下局生效；快照校验接受9，气点按间距等比缩小避免重叠。ki789-tests.xml 7/7存档往返与HUD数量/边界/间距通过；ki789-build.log Success正常退出。未运行视觉验收，不打Android不上传，无Unity占用 |
| ANIM-20260912-RESTORE-KILL | 本会话 | Yummn击杀还原standBy两倍速 | 完成源码/Windows；击杀还原01_standBy并沿用2倍速，普通拳skipQuest与气格挡不变。restore-kill-tests.xml 29/29通过；restore-kill-build.log Success退出0。未运行视觉验收，不打Android、不上传 |
| UI-20260912-TUTORIAL-ANDROID | 本会话 | 教程简化与当前气格挡规则、Android | 完成源码/Android：三页漫画不加页，第二页气格挡提示；详细说明拆操作/连按/等待，说明气格挡条件与后续攻击，残影非护盾，旧本局关闭提示。Kait连按提示同步、Boss不写固定值。tutorial-0912-tests.xml 6/6文本与布局通过；android-tutorial-20260912.log Success退出0，签名/入口/ARM64通过。旧包before-tutorial-20260912.apk；Windows本轮教程未重打，adb无设备未真机验收，未上传，无Unity占用 |
| RULE-20260912-KI-GUARD | 本会话 | 取消击杀额外停顿、拳击映射、气格挡设置/规则/表现 | 完成源码/Windows：普通拳/反击skipQuest，击杀attack（沿用2倍）；删除0.35秒。气格挡默认下一新局开，高速有气抵消一次并清气气竭，已有防御优先，旧存档保留。skill0截前50帧2倍，正常接待机。41/41专项；ki-guard-final-build.log Success退出0；hold-input-runtime.log HOLD_QA_COMPLETE，气格挡/连按与设置最终截图已看，37项偏好还原。未Android/上传，无Unity占用；见气格挡与拳击映射-20260912.md |
| INPUT-20260911-KILL-PAUSE | 本会话 | 击杀后自动连按缓冲停顿 | 完成源码/Windows：方向操作有玩家击杀时标记，AutoInputReady后开始0.35秒，继续保持恢复一次自动输入；新按键/换向/松手清旧停顿，普通未击杀连按不变。KaitHoldInputTests 8/8（kill-pause-tests.xml），kill-pause-build.log Success退出0。未本轮运行手感/真机验收，未Android/上传，无Unity占用 |
| ANIM-20260911-PUNCH2X | 本会话 | Yummn出拳两倍速、专项测试和Windows | 完成：KaitSpineView对Yummn的01_attack、01_attack_skipQuest、01_standBy单次轨道设2倍；待机/移动/技能/Kait不变。YummnAnimationTests 28/28（punch2x-tests.xml），半时长后接正常待机通过；punch2x-build.log Success退出0；hold-input-runtime.log HOLD_QA_COMPLETE，连续三拳/松手/等待通过，37项偏好还原。源码/Windows同步，Android未同步、未上传，无Unity占用 |
| BUILD-20260911-HOLD-ANDROID | 本会话 | 长按输入与等待手势Android包 | 完成：android-hold-20260911.log Success退出0，签名/入口/ARM64检查通过，APK见上方快照。旧包保留Build/kait-v0.8.2-before-hold-20260911.apk；adb无设备未真机启动/触摸验收。未上传GitHub，无Unity占用 |
| INPUT-20260911-HOLD | 本会话 | 长按方向连发设置、滑动保持、长按等待、测试及Windows | 完成源码/Windows；默认关、0.4秒保持后连发、等待回合及非循环动作结束、松手停止；手机滑后保持，原地0.55秒一次等待。修正棋盘Button被UI过滤排除，技能选目标除外。专项9/9，final构建Success退出0、HOLD_QA_COMPLETE，连续三拳/松手/等待通过，37项偏好还原；未Android打包/真机/上传，无Unity占用；见长按连续输入与等待手势-20260911.md |
| RELEASE-20260911-HOME-A | 本会话 | 当前源码上传GitHub、烟墨首页Android包 | 完成：源码及标签v0.8.2-home-a-20260911已推送，APK和说明已发布同名Release，远端大小221711324与SHA256均匹配本地。Android构建/签名/入口/ARM64通过；PUBLIC不变，旧包before-home-a-20260911.apk。adb无设备未真机验收；无Unity占用，不改规则 |
| UI-20260911-HOME-A | 本会话 | A烟墨双境主界面、双角色选择、斜向逐行菜单、教程路由、Windows | 完成源码/Windows/运行看图；每行取平行斜线中点，选人不启动、单一继续按角色存档、教程/设置随所选人物；CG轻浮动及悬停放大。首页专项7/7；扩展28/29唯一失败为旧气槽位置断言，未改。home-a-final-build.log Success退出0；HOME_QA_COMPLETE，标题行高修正后最终截图已看，37项偏好还原。无游戏/Unity占用，未Android/GitHub；见首页A烟墨双境接入-20260911.md |
| UI-20260911-KAIT-COMIC | 本会话 | Kait三页新漫画、详细说明、教程路由/测试与Windows | 完成源码/Windows/运行看图：内置image_gen三张2172×724，KaitComic/01-03；三页核心+滚动详细说明，Boss不写固定数值，旧十页素材保留。修复三页/四页切换导航容量，Kait和Yummn内容隔离。kait-comic-tests.xml相关11/11；kait-comic-build.log Success退出0；运行QA与Kait三页/说明首尾截图已看，37项偏好还原。未改战斗规则，未Android/GitHub，无Unity/游戏占用；提示词见Kait三页漫画教程-20260911.md |
| UI-20260911-DETAIL-LABEL | 本会话 | 教程按钮和标题改名 | 完成源码/Windows：三处显示字符串统一详细说明，内容和交互不变；静态检查旧词零处、新词三处，detail-label-build.log Success退出0。未重复运行视觉验收，无Unity/游戏占用，未Android/GitHub |
| UI-20260911-BOSS-COPY | 本会话 | Yummn当前教程Boss文案与漫画数字标注 | 完成源码/Windows：字幕和附录改为合成大数字，漫画256用可编辑UI标注覆盖；保留实际触发值。初版运行QA通过、37项偏好还原，截图后微调标注居中及颜色；boss-copy-final-build.log Success退出0，最终位置未重复看图。当前Build/kait.exe已同步，无游戏/Unity占用，未Android/上传 |
| UI-20260911-TUTORIAL-COPY | 本会话 | 当前Yummn三页教程与附录文字、针对性测试 | 完成源码/Windows/运行看图：三页短字幕、附录删除整套旧页重复，按快照说明气费/补2/敌方触发，统一残影与裂隙时机。tutorial-copy-tests.xml 3/3（其后仅“至少1气”短字幕修订，经最终构建）；tutorial-copy-build.log Success退出0；yummn-comic-runtime.log QA通过，三页及附录首尾截图已看，37项偏好还原。未改战斗/Kait/旧存档教程，未Android/GitHub，无Unity/游戏占用 |
| PREVIEW-20260911-HOME-TRIPTYCH | 本会话 | 双角色+中央极简主菜单ABC交互预览 | 完成待选：Tools/HomeTriptychPreview/index.html；A烟墨/B奶油/C冷光，双斜线、轻浮动、悬停/选中放大，选人不开始、单一继续在开始上方、教程随人切换。浏览器ABC看图及Yummn选择/教程验证；沿用获批CG，不改正式游戏/构建/上传。预览127.0.0.1:8880/HomeTriptychPreview/ |
| UI-20260911-KI-CENTER | 本会话 | 悬浮裁线右移、气槽去字居中 | 完成源码/Windows/看图；统一+28，原+12最右气点擦线已修正；去状态与耗气文本、气槽(0,0)居中。相关6/6与最终运行QA通过，37项偏好还原。无Unity/游戏占用，不改规则/Android/GitHub；详见气槽居中与悬浮裁线右移-20260911.md |
| UI-20260911-CHARACTER-CG | 本会话 | 获批原版CG斜切选人界面接入 | 完成源码/Windows/运行看图；真琴与万圣节宫子原CG、独立原生斜切、等比适配，保留开始/三版Yummn存档/返回。相关3/3测试、最终构建Success、CHARACTER_CG_QA_COMPLETE，37项偏好还原。未Android/上传，无Unity/游戏占用；详见原版CG斜切选人界面接入-20260911.md |
| UI-20260911-YUMMN-COMIC | 本会话 | Yummn核心漫画教程及文字附录 | 完成源码/Windows/看图；内置image_gen三张原创Q版三格漫画，字幕随规则快照，附录滚动、返回同页，保留Kait与旧规则教程。相关9/9测试，最终构建Success；运行QA通过，37项设置存档还原。未找到用户所指确切卡拉彼丘漫画，已请求截图，不宣称复刻。未Android/上传，无进程占用；详见Yummn三页漫画教程-20260911.md |
| UI-20260911-FLOAT-SPLIT | 本会话 | 极简气点圆形、悬浮UI裁线统一左移 | 完成源码/Windows/运行看图；卡通B不变、极简同色圆点和暗色空槽。原裁线错误使用缩放内容区，现统一背景坐标，悬浮UI偏移-12，屏幕气氛偏移0。相关9/9测试、构建Success、QA通过；37项设置/存档还原。未打Android/上传，无Unity/游戏占用，详见悬浮裁线与极简气点-20260911.md |
| FIX-20260911-RIFT-DELAY | 本会话 | Yummn同操作新裂隙不能直接出怪 | 完成源码/Windows；v0.8.2 ResolveYummnRifts跳过createdTurn>=turn，之后操作正常检查；旧裂隙无额外延迟，占格继续等待。教程同步，旧v0.8/v0.8.1回放/Kait不变。902/902测试；rift-delay-build.log Success退出0；本轮未运行视觉验收，不打Android/上传，无Unity占用，见裂隙同操作出怪修复-20260911.md |
| VFX-20260911-KI-B | 本会话 | B气流水滴接入、双风格与状态、Windows | 完成源码/Windows：内置生成透明四格B素材、卡通/极简共用值及斜切；满气青蓝、空槽轮廓、气竭灰蓝、小数按比例。898/898测试；ki-wisp-build.log Success退出0；ki-wisp-runtime.log YUMMN082_QA_COMPLETE，已看six-ki和partial-recovery截图，37项设置/存档按原类型值恢复。不改规则、不打Android、不上传，无Unity/游戏占用；详见气点B接入-20260911.md |
| VFX-20260911-KI-PREVIEW | 本会话 | 左侧危险红边增强、气点ABC预览 | 完成源码/Windows（danger-opacity-build.log Success退出0）；左侧倍率0.68到0.92，右侧0.48不变。内置图像生成ABC已看图，仅样式待选，A修订图顶部左侧仍少一珠，不能直接作为计量HUD；实际六气规则未改。提示词/路径见气点样式ABC预览-20260911.md。未运行游戏视觉验收、不打Android/上传，无Unity占用 |
| RULE-20260911-PALMS | 本会话 | 掌印方向与多目标标记 | 完成源码/Windows：每敌人独立记录一个命中方向、全场无数量上限；同向保留，异向引爆/死亡只清对应目标。保留旧单标记字段供旧QA读取；图像池按需要增长复用，掌印下缘朝命中方向，卡牌描述同步。895/895测试，palms-build.log Success退出0；未运行视觉验收，无Unity占用，不打Android、不上传 |
| RULE-20260911-GHOST-IDLE | 本会话 | 幻影整回合多次受击、有气multiidle、Android | 完成源码/Android：逻辑残影受击不销毁，每个攻击事件最多回1气，敌方阶段末统一清理；画面只播命中特效不提前淡出，教程同步。有气（ExactKi>0，包括气竭恢复中）01_multi_idle_standBy，0气01_idle，排队待机开始时重验气量，不抢攻击。890/890测试；APK10:46构建及签名/入口检查通过；旧包before-ghost-idle-20260911.apk。Windows未重打、未上传、未真机视觉验收，无Unity占用 |
| RELEASE-20260911 | 本会话 | 历史备份上传、当前源码留档、Android | 完成：源码b3df338及标签v0.8.2-20260911已推送；Release历史包4316文件/2071519202字节，7z测试通过，远端SHA256与本地68C5B5B7B85BFB46D8B0F6C047AC6A29F12CDD8339AF3731E75A82DB397DDAA4一致。APK构建/签名/架构/入口检查通过并上传，远端哈希一致。旧APK为Build/kait-v0.8.2-before-20260911.apk；仓库PUBLIC不变，无Unity占用，未真机验收 |
| VFX-20260911-DANGER-SPLIT | 本会话 | 受伤屏幕预警沿全局斜线切换左右风格 | 已完成源码/Windows；左侧DangerC九宫格原图、右侧无纹理红色渐隐边缘，共用触发/淡出。SunlitSplitText增加保留原色用于裁剪，默认文字行为不变。888/888测试，danger-split-build.log Success退出0；未实机视觉验收、不做Android/GitHub，无Unity占用 |
| VFX-20260911-POSE-LOOP | 本会话 | 去掉武器图标，锁定posing循环 | 已完成源码/Windows：不创建来源武器或外框，保留危险格范围。posing循环无排队idle，RefreshBattle同步锁定状态且不抢攻击/受击/死亡；885/885测试，pose-loop-build.log Success退出0。未运行视觉验收、不打Android/上传，无Unity占用 |
| VFX-20260911-FULL-CELL | 本会话 | 武器标识填满所在格 | 完成源码与Windows：按用户追加要求使用完整圆角矩形线框替代四角线；标识格中心，武器自身包围盒等比适配格子86%，仍在人物下层，无不透明底板。full-cell-tests.xml 879/879通过；full-cell-build.log Success退出0。未运行视觉验收，无Unity占用，未做Android/GitHub |
| VFX-20260911-GROUND-C | 本会话 | 接入脚下武器C四角定位 | 已接入源码与Windows：四角短线、无底板/圆环，脚下格内人物层下，保留posing。修正等待测试HashSet断言。879/879测试；ground-c-build.log最终Success，退出0。未运行视觉验收、未打Android/上传，无Unity占用 |
| RULE-20260911-WAIT | 本会话 | Yummn等待按钮、点击人物手势、回合结算与测试 | 进行中；不改Kait、不打Android、不上传 |
| VFX-20260911-POSING | 本会话 | 敌人锁定posing、脚下武器标识、ABC预览 | 源码完成：六敌人统一完整动画000000_rarityup_posing，脚下格内(0,-34)标识置于人物层下，取消悬浮底板，补弓/杖/盾。posing-tests.xml 876/876通过，六资源动画均存在。预览VFXPreviews/GroundWeapons-20260911三组GIF/PNG，原生图标+示意人物非游戏截图，B/C装饰待选。未运行视觉验收、未重打Windows/Android、不上传，无Unity占用 |
| FIX-20260911-MOBILE-EDGE | 本会话 | 手机红色危险边框全屏适配、测试 | 源码完成：KaitAtmosphereGraphic按短边调整九宫格角部，向外补偿透明留白，尺寸变化重新布局。876/876 EditMode通过（Logs/mobile-edge-tests.xml）；无Unity占用。Windows/Android未重打包，未上传，未真机视觉验收。见手机危险红边适配-20260911.md |
| AUDIO-20260911-KI-B | 本会话 | B组反向接入耗气/回气 | 已完成：原B_gain用于KiSpendB，原B_spend用于KiGainB，哈希与原件一致，PCM不变速。实际扣气行动开头一次提示，正资源事件回气，免费动作无耗气声。871/871测试，Windows08:57构建成功；未游戏内听感验收，未动存档，无Unity占用，不做Android/GitHub。见气音效B反向接入-20260911.md |
| AUDIO-20260910-KI-REDO | 本会话 | 消耗气与回气音效重新选稿 | 9月11日完成待试听选择：AudioPreviews/QiNatural-20260911，ElevenLabs重新生成纯气流/衣袖/内劲ABC六条1秒WAV及三条配对试听（先消耗后回气）。六文件哈希不同、48k双声道解码正常、无满幅采样；原件保留，听感待用户确认。Tools/package_qi_natural_previews.py与manifest记录来源；未接入，不构建、不上传、不占用Unity |
| BUILD-20260910-082-LATEST | 本会话 | 最新安卓版 | 已完成；23:24:38 APK含六兵种预警、0/1攻击耗气和CCC残影，197551132字节。构建成功、签名与旧包一致、入口/ARM64核验通过；SHA256 6C48E90BDAB393D70F0ED91DD8860FE1DCE403D886BA2DD03F20AE34B3F03B69。旧包保留为Build/kait-v0.8.2-before-enemy-aim-20260910.apk；adb无设备未真机启动验证，无Unity占用，不上传GitHub |
| VFX-20260910-ENEMY-AIM-SELECTED | 本会话 | 六兵种预警接入 | 已完成：弓手/术士/盾骑士最初版，杂兵/剑士/重甲新版AAA。原生图形、危险框与方向去重、近战来源侧徽；替换旧斜纹/紫色术士/BossB地面预警，保留素材及人物规则。866/866测试，Windows23:17同步，运行QA及看图通过，34项设置存档还原。无Unity/游戏占用，未做Android/GitHub；详见六兵种攻击预警接入-20260910.md |
| PREVIEW-20260910-ENEMY-AIM-ABC | 本会话 | 六兵种ABC与重叠预览 | 完成待选择；VFXPreviews/EnemyTelegraphs-ABC-20260910，六张ABC并排GIF（18方案）及overlap.gif，均验证3200ms循环并看静态帧。来源格使用武器/胸甲/盾徽作示意，非替换角色；目标危险底框去重、边缘方向指示。原生预览图形，不接入、不构建、不上传，无Unity占用 |
| PREVIEW-20260910-ENEMY-AIM | 本会话 | 六种敌人攻击预警GIF | 完成待审批；VFXPreviews/EnemyTelegraphs-20260910，六个560×536循环GIF，每轮3360ms，manifest记录实际合并帧数，静态帧已看。程序原生贴格图形、站位符号示意，不是游戏截图或已接入效果。不改游戏/规则、不构建、不上传，无Unity占用 |
| RULE-20260910-ATTACK01 | 本会话 | 更正高速攻击设置为0/1气 | 已完成；关闭0、开启1，气竭免费，不足仍出拳，下局生效。保留旧0.1回放兼容，旧菜单偏好迁移新1气选项。857/857测试、Windows22:50同步、设置运行截图已检查。无Unity/游戏占用，未做Android/GitHub |
| VFX-20260910-GHOST-CCC | 本会话 | 常驻时砂C、命中断帧C、音效C、测试/Windows | 已完成源码/素材与Windows同步；图集/音效哈希与获批C一致。attack01-tests.xml 857/857通过；verify_ghost_ccc_runtime.ps1通过，常驻/命中截图已检查，30项玩家设置存档还原。音效未做听感验收，无Unity/游戏占用，未做Android/GitHub。详见逻辑残影CCC接入-20260910.md |
| RULE-20260910-OPTIONS | 本会话 | Yummn攻击0.1气、方向/移动补2、额外移动敌方阶段、设置与测试 | 已完成：847/847测试，Windows22:35同步；按整数十分位精确扣费，气竭免攻击费/不足仍出拳，移动条件OR合并。设置和教程更新；新选项默认关闭、下局生效，Kait不变。无Unity占用，未运行画面验收/Android/GitHub；详见Yummn附加规则设置-20260910.md |
| PREVIEW-20260910-GHOST | 本会话 | 逻辑残影常驻/命中逐帧选稿和命中音效试听 | 选稿完成待审批：VFXPreviews/LogicalAfterimage-20260910，六张透明八帧图集、两张ABC GIF、三条ElevenLabs WAV。GIF/透明通道/WAV解码已检查，A/B音频原件有触顶采样；未游戏内验收，不改源码/包，无Unity占用。上一条设置请求尚有两项规则待确认，未实施 |
| VFX-20260910-07 | 本会话 | 时钟A、危险红边C、盾骑士整排预警B，资源/显示/测试/Windows | 已完成：837/837测试，Windows21:59同步；WARNINGS_QA_COMPLETE及最终看图通过，24项存档/设置原样恢复。无Unity/游戏占用；未做Android/GitHub。详见预警ACB接入-20260910.md |
| BUILD-20260910-082-ANDROID | 本会话 | 安卓版本号、APK构建和验证 | 已完成：v0.8.2/802，Logs/v082-android-build.log Success；签名与旧0.8.1相同，入口/架构检查通过。旧APK保留，adb无设备未真机验证；无Unity占用，不上传GitHub |
| RULE-20260910-082 | 本会话 | Yummn规则快照、事务、敌人AI、残影、HUD/设置/教程、测试/Windows及审批清单 | 已完成：本地修改前备份17604文件；831/831测试；Windows18:19:54同步，新旧运行QA及看图完成；玩家存档恢复。无Unity/游戏占用。正式残影碎裂/穿空音效待审批，第六组未接入；未做Android/GitHub。详见 v0.8.2-实施与素材审批.md |
| VOICE-20260910-03 | 规则与语音接入会话 | GameAudio.cs、KaitGame.Yummn.cs、KaitGame.cs、语音资源/测试/接入文档 | 已完成并同步 Windows：Yummn Bridget、重甲 Coonya；38条新增WAV统一电平，原音效/其他角色不变。ki-tests-final.xml 786/786通过（31项语音及最后脚本修改）；ki-build-final.log Success，17:33包，ki-runtime-final.log QA通过。未做听感验收、Android/GitHub；详见 Yummn与重甲语音接入-20260910.md |
| VFX-20260910-06 | 特效会话 | YummnV08Art.cs、YummnPresentation、YummnQA、帧测试、第五组资源；第六组预览 | 已完成：ACB 接入，786项测试通过，Windows最终构建和运行QA完成。第六组时钟／危险红边／盾骑士整排预警各ABC八帧及三张GIF待选、未接入。无Unity或游戏占用，不做Android/GitHub |
| VOICE-20260910-02 | 规则与语音筛选会话 | 角色设定检索、Docs/Yummn语音角色匹配-20260910.md、Tools/audition_yummn_voice_roles.py | 检索完成、待试听选定：108231 为宫子万圣节；新增 Aqua 与 Bridget 两套单人试听，后者已用于重甲敌人。仅验证文件配套及 WAV 解码，未做听感/台词验收；不改游戏、不占用 Unity、不构建或上传 |
| DOC-20260910-01 | 音效接入会话 | AGENTS.md、本文、README 入口 | 已完成；未启动 Unity、未改游戏逻辑 |
| VFX-20260910-05 | 特效会话 | YummnV08Art.cs、KaitGame.YummnPresentation.cs、KaitGame.Yummn.cs、KaitGame.YummnQA.cs、相关测试和 Frames | 已完成：第四组 BBCA 接入，752/752 测试，Windows 构建及最终运行截图检查通过，存档设置已还原；无进程占用。第五组 9 张图集、3 张 GIF 待选，未接入；未做 Android/GitHub |
| VFX-20260910-04 | 特效会话 | 第四组选稿目录、本文 | 已完成：teleport/darkness/deny/shadow-ABC.gif，A/B/C 并排循环；核验 8 帧（瞬发另加空白间隔）、延时及无限循环。只改预览，不改游戏或原素材，不构建 |
| VOICE-20260910-01 | 规则与语音筛选会话 | 本清单、Docs/Yummn语音候选-20260910.md、Tools/audition_yummn_voice.py、AudioPreviews/YummnVoice-20260910 | 初筛完成、待用户选声线：六套72条，六段连续试听；只读原包，未接入、不占用 Unity、不改 Windows/Android |
| VFX-20260910-03 | 特效会话 01a06b89-4f27-7931-8d4d-e654b153d193 | YummnV08Art.cs、KaitGame.YummnPresentation.cs、KaitGame.Yummn.cs、KaitGame.YummnQA.cs、Frames 资源及相关测试 | 已完成 CABA 接入、746/746 测试、Windows 构建及实际截图检查；暗影组 12 张选稿和四张图片对比完成待选。无 Unity/游戏占用；未做视频、Android 或 GitHub |

本表不自动追踪其他会话。没有登记不等于没有任务；陈旧 PID 也不等于当前仍占用。已经在运行的会话需主动读取入口后才能遵循新约定。

## 3. 已完成修改

### RULE-20260910-082 · 时停残影与气资源

- Yummn新局默认6气、按格耗气、贴身拳免费、击杀+3；七项下局快照设置。统一一次敌方阶段，触底气竭锁存；新生敌人阶段前加入快照但不立即攻击。
- 逻辑残影与短暂拖尾分离：主动高速移动起点一个标记；攻击覆盖消耗，每次攻击最多+1；阶段末清理。推动近战预警随人平移、弓手重算、术士锁地不动。
- 本轮文档覆盖旧版“气竭移动另补2”和“有效移动含跟进”的新局口径；旧v0.8.1存档仍保留旧逻辑。不要把这些旧会话规则重新套回v0.8.2。
- 本地备份：C:/Users/yummn/Downloads/kait-backups/kait-before-v082-20260910。源码/Windows已同步，Android/GitHub未做。完整实现、验证及审批边界见 [v0.8.2记录](v0.8.2-实施与素材审批.md)。

### VOICE-20260910-01 · Yummn 语音初筛（非接入）

- 沿用现有 AGENTS.md / SHARED_CHANGELOG.md 协作入口，补记规则会话已完成与未获批事项，没有另建共享主清单。
- 从已解压本地语音包筛选 Coonya、Xiao、Tsubaki、Momiji、Mashiro、Shy，共72条；优先试听前三套，依据为攻击时长和配套完整度，并非已核实声线适配。
- 六段试听保存在 AudioPreviews/YummnVoice-20260910；顺序 N2、N3、H1、Go1。原素材不变，正式语音、音效均未替换。声线与台词等待用户确认。
- 本地 tiny 日文识别有明显错字、重复幻觉，screening.json 不作为已核实翻译。完整索引：[语音候选](Yummn语音候选-20260910.md)。
- 未启动 Unity、未构建、未修改存档、未上传。不能将这条记录视为同意实施前述闪避回气方案。

### AUDIO-20260910-01 · Yummn 16 条音效

- 已接入：Move_C、Ready_A、Water_A、Air_A、Ice_A、Frost_C、Fire_A、Shatter_D、Shadow_D、Exhaust_B、Recover_A，以及第二轮 Punch_A、Kill_A、Block_C、PalmSeal_C、Ki_A。
- PalmSeal_C 播放增益提高 25%；其余原始文件未改音量、裁剪或变速。旧资源保留。
- 代码：Assets/Scripts/YummnAudio.cs。资源：Assets/Resources/Audio/Yummn/SelectedModel。
- 导入文件与选定 WAV 的 SHA-256 一致；PCM / 48kHz / 双声道。部分选定原件存在触顶采样，不宣称降低播放音量可以修复原件。
- 详见 [接入记录](Yummn音效接入-20260910.md)。

### VFX-20260910-01 · Yummn 四组逐帧特效

- 已接入：普通拳击 B、疾风连击 B、震慑 B、防御/格挡 A；每张八帧。
- 资源：Assets/Resources/KaitVisuals/Yummn/Frames。保持敌人上方、主角下方层级，不阻塞方向输入。
- 实际截图、生命周期、移动/气竭等检查由特效会话完成；有一条退出阶段 ComputeBuffer 释放提醒，未记录 QA 失败。
- 详见 [特效记录](Yummn逐帧特效接入-20260910.md)。该记录比本清单更具体，不将其预览项目误记为已接入。

### AUDIO-20260910-02 · Boss_A / Cast_B

- Boss_A 替代旧登场提示，走独立 worldSource，增益 0.72、固定音高；不再因存在 Ursula 语音而跳过。保留原有登场事件触发位置。
- Cast_B 替代没有专属音效的通用施放声，增益 0.76；其他已选专属技能音效不变。
- 代码：Assets/Scripts/GameAudio.cs。资源：Audio/World/SelectedModel/Boss_A.wav 与 Audio/UI/SelectedModel/Cast_B.wav（均在 Assets/Resources 下）。
- 740/740 测试通过，Windows 13:25 构建成功；未生成 Android，未重新进行实机听感验收。
- 详见 [接入记录](Boss与通用施法音效接入-20260910.md)。

### VFX-20260910-02 · 元素 CAAA

- 已接入：水鞭 C、气拳 A、冬之吐息 A、火蛇 A，八帧透明图集；按攻击方向旋转，不锁输入。
- 上轮 740/740 测试及 Windows 构建、运行检查通过，本轮 746 测试和运行检查再次覆盖。
- 详见 [元素接入记录](Yummn元素逐帧接入-20260910.md)。

### VFX-20260910-03 · 冰晶掌印 CABA

- 已接入：冰柱 C、碎冰/冻结 A、震颤掌标记 B、掌印引爆 A。
- 冰柱生长后保持末帧；掌印循环并跟随敌人；状态解除时隐藏。碎冰/引爆自动清理，敌人上方、主角下方。
- 746/746 测试、Windows 构建和实际截图检查完成；测试存档与设置已还原，前后哈希一致。未改音效和战斗规则。
- 详见 [冰晶掌印接入记录](Yummn冰晶掌印逐帧接入-20260910.md)。
- 用户展示偏好已澄清：直接在聊天展示 A/B/C 并排循环的小 GIF，不要视频；静态分镜仅作补充。第四组已补充四张 *-ABC.gif。

### VFX-20260910-05 · 暗影 BBCA

- 暗影传送 B、暗幕 B、瞄准失效 C、阴影格 A 已接入八帧素材。暗幕与阴影格循环、状态解除后隐藏；传送和失效提示播放后清理，不阻塞输入。
- 实机检查后将传送范围调整为 140、头顶提示上移至 64，避免被人物遮住。备份、测试及截图见 [暗影接入记录](Yummn暗影逐帧接入-20260910.md)。
- 第五组仅生成回气、进入气竭、恢复高速 A/B/C；内置图像生成，保留原透明图集，转换为并排循环 GIF 供审批。

### VFX-20260910-06 · 气状态 ACB

- 回气 A、进入气竭 C、恢复高速 B 已接入；八帧 480ms 自动清理，不锁输入，不恢复气竭残影。原图复制哈希一致。
- 实机发现回气被头部挡住，已上移后再次构建、截图确认。Windows同步，测试存档与设置还原；详见 [气状态接入记录](Yummn气状态逐帧接入-20260910.md)。
- 同步构建保留了语音会话的 Bridget／Coonya 修改，786项测试包含31项语音；未做语音听感验收。最终包证据已回传语音会话。
- 第六组由内置 image_gen 生成，保留透明原图；三张并排循环GIF已核验八帧、150ms帧间隔、无限循环。盾骑士初稿过厚像火焰，已重做细线版。仅预览，不改游戏。

## 4. 待审批、暂停与建议

### 规则会话补充交接（2026-09-10）

- 已完成：Yummn 盾骑士远距锁向、转盾并于下一敌方阶段攻击前方整排；Kait 不变。688 项测试及运行 QA 通过，详见 Yummn盾骑士整排攻击.md。
- 已完成：原“原地出拳不补新2”选项改为“有效移动至少一格才补新2”。包括击杀跟进、推掌追身及传送；原地施法、只移动敌人或副盘不补。其他补给模式及 Kait 不变，无踪步原例外保留。711 项测试通过，Windows 已在随后音效／特效构建之前同步；旧存档保留旧判定，新局使用新规则。详见 有效移动补2口径修订.md。
- 仅讨论、未实施：击杀回1＋基础贯穿拳＋多杀奖励方案，用户已否定，不应实施。
- 仅讨论、未实施：击杀回1＋高速精准闪避回1方案，尚未获得用户确认。本次语音筛选请求不授权该规则改动。
- 此补记不覆盖上方其他会话更新后的构建和验证状态。旧素材待确认清单为时间快照，须结合本清单及后续选稿记录阅读。

| 内容 | 状态 | 下一步边界 |
| --- | --- | --- |
| v0.8.2逻辑残影与命中反馈 | 已获批C/C/C，源码与Windows已同步，运行截图检查完成 | 常驻保留真实Spine轮廓，时砂为代码原生小菱形；命中/音效直接采用获批C。听感未验收 |
| 背景音乐 BGM_A / BGM_B | 用户明确“先跳过”；保留原 BackgroundMusic.wav | 不继续生成、不替换。网页短样下载要求付费订阅，未购买、未下载 |
| 连杀等待时钟、即将受伤红边、盾骑士整排预警 | 用户已选A/C/B，源码和Windows已接入，运行截图已看；Android未同步 | 时钟保留原触发/灰边，危险红边四角九宫格适配，盾骑士逐格俯视铺设；不改战斗规则 |
| 卡牌插画、卡框细节、人物选择、教程画面 | 可复核项，不是已安排任务 | 参考 [素材待确认清单](当前素材待确认清单-20260910.md)，其中旧音效状态已被本清单更新 |
| 整局混音 | 仅提出建议，用户尚未要求实施 | 可检查重复播放、音量叠加、语音遮蔽；不要自动调参或重新生成 |
| Android / GitHub | Android已按用户后续要求生成v0.8.2；GitHub未执行 | Android未做真机启动验收；后续不因Windows更新自动打包或上传 |

## 5. 协作流程

1. 开工：读本文及相关专题文档，检查最新 diff 和实际进程；登记负责会话、目标文件、任务范围。
2. 进行中：如需修改他人登记的文件，先协调。Unity 测试、导入、构建串行进行，记录 PID、日志、目标平台；不擅自关闭用户编辑器或游戏。
3. 验证：分别记录代码/资源检查、EditMode、运行检查、构建。文件存在不等于测试通过；预览合成图不等于游戏截图。
4. 收尾：再次读取本文，局部更新自己的条目与交付快照；写清未完成项、包是否更新、是否上传。保留他人并发新增内容。
5. 不确定或暂停：写“待核实 / 阻塞”和原因，不猜测完成状态。不在交接文件保存密钥、登录令牌或玩家存档内容。

## 6. 新任务登记模板

- 编号：类别-日期-序号
- 负责人：会话标题或可访问的会话 ID
- 用户请求 / 边界：
- 状态：进行中 / 已完成 / 待审批 / 暂停 / 阻塞
- 文件 / 资源：
- 占用：Unity PID、构建目标、日志路径；无占用则注明
- 已完成：
- 验证：命令或入口、结果、证据路径；未验证项目单列
- 交付：源码 / Windows / Android / GitHub 分别写状态和时间
- 下一步 / 未解决：
- 最后更新：北京时间
