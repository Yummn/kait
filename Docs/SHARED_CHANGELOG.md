# Kait 共享修改清单

最后更新：2026-09-20（北京时间）。维护者：Reynard GitHub发布会话。

这是多个 Codex 任务的本地交接入口。打开同一个工程的任务共读此文件；不同目录的项目副本或远程任务不会自动同步。当前用户请求优先；文中的待办和其他会话记录不是执行授权。

## 1. 工程与交付快照

| 项目 | 已核实状态 / 证据 |
| --- | --- |
| 工程 | C:/Users/yummn/Downloads/kait |
| Unity | 6000.0.30f1；C:/Unity/6000.0.30f1/Editor/Unity.exe |
| 场景 | Assets/Scenes/Scene.unity |
| 本地规则版本 | 应用0.9.33、Android配置版本码933；Reynard规则Reynard.0.4-direction-shot，牌池reynard-core24-direction-shot-20260920，存档Kait.Run.Reynard.0.4；Kait保留47个身份，普通对局默认池24张、实验池11张、Legacy池12张，牌池kait-round2-default24-20260919，规则Kait.0.9.21-round2-default24；Yummn牌池0.9.17-card-balance，共56牌（白17/蓝30/金9），规则快照yummn-0.9.17-card-balance，新存档Kait.Run.Yummn.0.9.17；保留0.9.7旧存档入口。2048锁盘保持静止，不判负、不重置 |
| Git 基线 | 0.9.24—0.9.33 Reynard第三角色、资源、规则、测试和说明已由提交4cc90df推送origin/master；Yummn/kait保持PUBLIC。0.9.23标签与改版前0.9.7标签继续保留。未覆盖/上传未核实的旧v0.8.1对照实测报告 |
| 最近 Windows 构建 | Build/kait.exe及配套kait_Data v0.9.33；Logs/reynard-blocked-shot-0933-windows-final.log成功退出0，受阻仍朝输入方向射击已同步；旧APK/ZIP未更新 |
| 最近 EditMode 测试 | Logs/reynard-blocked-shot-0933-final.xml：Reynard 37/37通过；非全量测试 |
| 本轮运行检查 | enemy-aim-runtime.log：WARNINGS_QA_COMPLETE；六兵种、出手变黄、三敌重叠、盾骑士四方向、死亡清理及气氛特效。已看六兵种/术士黄色/重叠/上下方向截图。34项玩家设置存档按类型和值还原，非Android验收 |
| 最近视觉运行检查 | Logs/reynard-runtime.log：HOME0926_SELECTION_QA_COMPLETE、REYNARD_QA_COMPLETE；47项玩家偏好已恢复。Build/Logs/reynard-blocked-shot.png与reynard-forward-bullet.png已查看：相邻敌人阻挡时人物保持原位、右向弹道与1伤结算正确，之后可继续操作；非Android实机验收 |
| 最新 iOS 导出 | Build/kait-v0.9.18-ios-xcode.zip，514095718字节；Logs/ios0918-export.log成功。Xcode工程版本0.9.18(918)、com.kaitprototype.demo、iOS 13+、ARM64/Metal、iPhone+iPad、横屏、AppIcon与IL2CPP元数据核验通过；独立解压后3130个文件逐项SHA256一致。SHA256 8F4FCEAA708DA76635F071C83AAA4E4067B7122389655D5622FC7E921387855B。Windows无Xcode与Apple签名材料，尚未编译/签名IPA或真机验收 |
| 最新 Android | Build/kait-v0.9.32.apk，2026-09-20 17:17:38，354260781字节；Logs/kait0932-android.log成功退出0。版本0.9.32(932)、最低API26、目标API35、ARM64、UnityPlayerActivity；v2签名有效。SHA256 44612CF56AD7EE4BC24818412956DE144C24E822FA48097CA66DE2B70922E393。ADB无设备，未做真机验收 |
| Android | Build/kait-v0.8.2.apk，2026-09-12 23:52:44，234614258字节；Logs/android-exit-stun-20260912.log成功退出0；同步主动技能拖放分类、R40借机攻击及整回合震慑。版本802、Android 8.0+、ARM64、UnityPlayerActivity、v2签名有效且签名与旧包一致。SHA256 84B07BFDDB9A8CADB451C0A1034BC34B3BFC7B2D315D14064EE4E7D0DB8ADBA6；ADB无设备未真机验收，未上传GitHub |
| 历史文档 | README / VERSION_HISTORY 中部分内容描述 v0.6.1 归档，不是当前未提交工作树的完整说明 |

Windows 交付必须带整个 Build 内的关联文件，不能只拷贝 kait.exe。构建后核对日志和 kait_Data 中资源/程序集时间，不只看启动 exe 的时间。

## 2. 进行中与占用登记

RELEASE-20260920-REYNARD-0933-GITHUB：完成，本会话。0.9.24—0.9.33的Reynard第三角色、场景/首页、选牌修复、伊甸语音、移动射击、裂隙白块修复及受阻仍射击规则以提交4cc90df推送公开origin/master；168个LFS对象约69 MB上传完成。明确排除来源未核实的旧Docs/v0.8.1-四组对照实测.md本地修改，Build与Logs继续按忽略规则仅保留本地。

RULE-20260920-REYNARD-BLOCKED-SHOT：完成，v0.9.33，本会话。Reynard每次方向操作都朝输入方向发射1伤子弹；被敌人、柱子或边界阻挡时人物不位移，但弹道、附加法术、右盘及敌方阶段照常。规则/牌池/存档独立升版；专项37/37，Windows构建退出0，运行构造相邻敌人阻挡并核验原位、1伤和右向弹道，47项偏好恢复。Android仍0.9.32，不上传。说明见Docs/v0.9.33-Reynard受阻仍射击.md。

BUILD-20260920-0932-ANDROID：完成，本会话。Build/kait-v0.9.32.apk于2026-09-20 17:17:38生成，354260781字节；Logs/kait0932-android.log成功退出0。核验0.9.32(932)、API26/35、ARM64、UnityPlayerActivity与v2签名有效；SHA256 44612CF56AD7EE4BC24818412956DE144C24E822FA48097CA66DE2B70922E393。ADB无设备，未安装/真机启动；不上传GitHub。

FIX-20260920-REYNARD-RIFT-WHITE-RECT：完成，v0.9.32，本会话。Yummn原RGB裂纹按同一明度公式预烘焙为RGBA，Yummn/Reynard共用同款双层裂纹且不再依赖运行时抠色材质；无Spine和无备用头像时不激活空Image。Reynard专项35/35，Windows构建退出0，运行扫描全部战斗格并实际查看构造裂隙与连续行动截图，无白块。Android仍0.9.23，不上传。说明见Docs/v0.9.32-Reynard裂隙与白块修复.md。

RULE-20260920-REYNARD-FORWARD-SHOT：完成，v0.9.31，本会话。Reynard成功移动后从新位置向前发射1伤子弹，阻挡不射击；R13—R17适配为三向、溅射、推动、穿透和等待射击。新增狐火飞行弹道，镜狐改为右盘数字下层的Noel半透明人物残影。规则/牌池/存档独立升版；Reynard专项35/35，Windows构建退出0，运行自动核对残影父格并截取弹道中间帧，46项偏好恢复。Android仍0.9.23，不上传。说明见Docs/v0.9.31-Reynard移动射击与镜狐残影.md。

UI-20260920-REYNARD-SPLIT-WHITE-FIX：完成，v0.9.30，本会话。Reynard移动/移动击杀不再触发整屏震动；紫藤场景裁至斜线左侧，右侧恢复深紫极简背景并增加统一暖色剪切线。白块根因是RGB裂隙色键材质临时失效，Reynard改走RGBA透明裂隙和普通UI材质，缺图不激活空Image。专项34/34，运行构造裂隙/移动中间帧/战斗页截图均已查看，REYNARD_QA_COMPLETE且45项偏好恢复；Windows构建退出0。Android仍0.9.23，不上传。说明Docs/v0.9.30-Reynard战斗页剪切与白块修复.md。

AUDIO-20260920-REYNARD-EDEN-VOICES：完成，v0.9.29，本会话。Reynard主控11202003接入Noel语音；专属杂兵/剑士/弓手/重甲/术士/盾骑士依次接入Chocolat/Stick/Quinn/Kiki/Megumin/Nouet语音。125个WAV进入独立Resources目录，人声与魔法音效分轨，沿用每兵种通道及后发语音压低旧语音。Reynard专项34/34，Windows构建退出0，运行REYNARD_QA_COMPLETE且45项偏好恢复。源码/Windows同步，Android仍0.9.23，不上传。说明Docs/v0.9.29-Reynard伊甸语音.md。

INPUT-20260920-REYNARD-EXHAUSTED-MOVE：完成，v0.9.28，本会话。Reynard普通移动直接复用Yummn气竭移动协程：同一EaseOutCubic、距离时长、walk循环、无残影和单步输入衔接；人物与威胁盘同步开始。步进限制为最多1/30秒，掉帧不再跳过整格。Reynard专项33/33，运行日志记录中间进度0.4918689且无移动样式错误，REYNARD_QA_COMPLETE；Windows已构建，45项玩家偏好恢复。Android仍0.9.23，不上传。说明Docs/v0.9.28-Reynard气竭式移动.md。

FIX-20260920-REWARD-FREEZE-WHITE-RECT：完成，v0.9.27，本会话。奖励仅在演出/连斩结算完成后出现，三角色选牌统一独占输入，失焦/暂停/关闭会复原卡栏；修复隐藏卡空定义、旧存档空卡定义替换和同类槽位顺序。Reynard动态图片改为素材存在后才创建，卡图缺失保持透明，消除默认白色矩形。选牌19/19、Reynard33/33专项通过；Windows构建成功，运行REYNARD_QA_COMPLETE/HOME0926_SELECTION_QA_COMPLETE并恢复45项偏好。源码/Windows同步，Android仍0.9.23，不上传。说明Docs/v0.9.27-选牌与白色矩形修复.md。

UI-20260920-HOME-DIAGONAL：完成，v0.9.26。去除三张矩形卡框，恢复三角色+简约菜单的平行斜切；各行按切面居中，独立多边形点击，悬停/选择放大提亮。Reynard使用画师kan作品页2500×1782 Noel宣传CG，弃用模糊的小立绘。3/3首页专项通过；Windows最终构建退出0，HOME0926_SELECTION_QA_COMPLETE/REYNARD_QA_COMPLETE，三个选中态实际截图已逐张检查，45项玩家偏好恢复。源码/Windows同步，APK仍0.9.23；不上传，保留其他修改。说明Docs/v0.9.26-三角色斜切首页.md。


FIX-20260920-REYNARD-SCENE：完成，v0.9.25。移除Reynard独有的敌人追近行为，沿用旧版攻击阶段；修复LateUpdate覆盖移动插值，0.24秒先快后慢滑行；主控按排除武器的身体范围居中、适配大小。紫藤/月光/琥珀灯火完整背景+真实透明前景，动态开口贴合双棋盘，保留狐像/栏杆，移除旧绿色边缘。53/53专项通过；Logs/reynard0925-final.log Windows构建成功，运行REYNARD_QA_COMPLETE并捕获进度0.194的中途滑行画面；44项玩家偏好还原。源码/Windows同步，不Android/上传。

RULE-20260920-REYNARD：完成，本会话，v0.9.24。第三角色11202003、六敌人BACABA仅Reynard替换、24牌/方向咒/镜狐法术位/独立回放与存档；生成24透明卡图、森林背景、8帧狐火和5个既有素材混音。首页四栏布局，Windows构建成功并完成实际截图复查，52专项通过。说明见Docs/v0.9.24-Reynard第三角色.md。未上传、未构建Android；现有APK仍0.9.23。保留旧v0.8.1报告修改，无Unity占用。

RELEASE-20260920-0923-ANDROID-GITHUB：完成，本会话。基于当前0.9.23简洁Kait卡图与二轮牌池工作树，KaitCandidate47Tests 21/21通过；Android构建成功并核验版本923、API26/35、ARM64、UnityPlayerActivity、v2签名及SHA256，ADB无设备故未真机验收。0.9.19—0.9.23源码、正式资源、测试与说明已由本次发布提交推送公开origin/master并建立v0.9.23标签，APK随同名GitHub Release发布。明确排除未核实的旧v0.8.1对照实测报告本地修改。

ART-20260920-KAIT-SIMPLE：完成，本会话。以用户Yummn截图为参考，内置图像工具逐张重做47张Kait卡图；44张物件/符号，3张简化Q版人物。47张RGBA透明检查通过，冰墓人物黑紫发已单独修正，导入设为512像素无压缩。Windows0.9.23构建成功并核实版本，Kait八页与Yummn对照页实际截图已检查，图像完整且无卡框裁剪，42项玩家偏好恢复；不改玩法。说明与提示词见Docs/v0.9.23-Kait简洁卡图.md。未Android/上传。

BUILD-20260920-WINDOWS-0922：完成，本会话。当前0.9.22源码、二轮牌池和统一Kait人物卡图已构建到Build/kait.exe及配套kait_Data。Logs/kait0922-windows.log成功退出0，构建数据内版本0.9.22，资源/程序集时间为2026-09-20 00:04；隐藏窗口启动烟测存活12秒且初始化正常。未重打Windows ZIP，不构建Android、不上传GitHub。

ART-20260919-KAIT-CHARACTER-CONSISTENCY：完成，本会话。逐张检查Kait全部47张卡图，重绘其中33张含Kait Q版人物的卡图；人物固定为黑紫发、琥珀眼、猫耳、黑棕/紫色咒剑士服装、金色扣带与巨型暗色咒剑。纯道具、法术和敌方剪影14张保留原构图。正式资源路径不变，卡牌大全/选牌/折叠卡直接加载新版；47张RGBA透明校验通过，Unity专项21/21通过。应用0.9.22(922)，规则/牌池/存档不变；不构建Android/Windows、不上传GitHub。详见Docs/v0.9.22-Kait人物卡图统一.md。

RULE-20260919-KAIT-ROUND2-ART39：完成，本会话。按《试玩反馈后二轮牌池重构》实施默认24/实验11/Legacy12、首刀/余势/咒刃/震慑/自动魔能爆/统一主动被动冷却及动态抽取前置；47个身份、旧ID和解析入口保留。为上一轮新增8张之外的其余39张Kait牌生成独立透明卡图并接入，画风对齐Yummn ApprovedA，应用0.9.21(921)。专项21/21通过；本轮不构建Android/Windows、不上传GitHub。详见Docs/v0.9.21-Kait二轮牌池与全卡图.md。

RULE-20260918-KAIT-ART-REWARD16：完成，本会话。新增8张透明独立卡图并接入Kait新牌，画风对齐Yummn ApprovedA；Kait每次合成出16三选一，Yummn阈值不变。应用0.9.20(920)，Kait规则/牌池快照升版避免旧回放静默套用。Unity专项18/18通过；Android构建、版本/入口/API/ARM64/v2签名与SHA256已核验，ADB无设备未真机验收。Windows仍0.9.18，未上传GitHub。详见Docs/v0.9.20-Kait新卡图与16选牌.md。

RULE-20260918-KAIT-CANDIDATE47：完成，本会话。按《Kait 候选47张全接入》实现47张候选池（13主动/34被动）、37张建议正式池、共享六槽跨类型替换及重剑/咒剑/敌军内斗/战争法师/真实合并规则；8张新牌优先复用Yummn获批卡图。应用0.9.19，独立规则与牌池版本阻止旧回放静默套用新规则。专项17/17通过；全量1131项962通过、169项旧契约失败，如实保留。Android构建、版本/入口/API/ARM64/v2签名与哈希已核验，ADB无设备未真机验收；首次切Android的旧DAG漏新脚本后Unity自动刷新并在同次构建成功。Yummn 56张牌池不改，不上传GitHub。详见Docs/v0.9.19-Kait候选47张与六槽构筑.md。

BUILD-20260918-IOS-0918：完成Xcode工程导出，本会话。安装Unity 6000.0.30f1官方Windows iOS Build Support，新增可复用的iOS Xcode导出入口；配置0.9.18/918、iOS 13+、iPhone与iPad、横屏、ARM64/Metal、IL2CPP、完整应用图标和com.kaitprototype.demo。Logs/ios0918-export.log成功；工程压缩后独立解压，3130个文件逐项SHA256一致，包体与哈希见上方快照。README-iOS.txt说明在Mac/Xcode选择Apple Team后运行或归档。Windows无Xcode和Apple签名材料，未编译/签名IPA、未真机验收；不上传GitHub。

BUILD-20260918-WINDOWS-ZIP-0918：完成，本会话。将0.9.18 Windows运行所需exe、Data、MonoBleedingEdge、UnityPlayer、崩溃处理器和D3D12共195个文件打包；排除APK、Burst调试符号与旧归档。独立解压后逐文件大小/SHA256一致，并从解压目录启动存活10秒。包体和哈希见上方快照；不上传GitHub。

UI-20260918-TUTORIAL-0918：完成，本会话。根因是详细说明面板为RGB(54,47,56)，正文沿用RGB(49,45,68)，亮度几乎相同。正文改为浅纸色、字号28升至30并增加少量行距，滚动条改为暖色；漫画页不变。应用升0.9.18，规则、牌池与0.9.17存档键保持不变。KaitComicTutorialTests 1/1通过；Windows构建成功。运行检查生成Kait/Yummn详细说明顶部/底部截图并逐张查看，正文清晰且无正文溢出；该旧QA另有气点和设置数量旧断言，不属于本次功能且未表述为整套通过。不构建Android、不上传GitHub。

RULE-20260918-YUMMN-CARDS-0917：完成，本会话。按用户清单调整16张Yummn卡的范围、目标、气费、稀有度及补给/气上限/格挡效果；冬之吐息改前方4格伤害冻结，暗影步可选任意合法空暗影格，静谧心境等待补两个2，气海扩张上限+3，灵体护身任何状态3气格挡。更新牌池/规则/应用至0.9.17并保留0.9.7旧存档入口，卡面教程同步。专项6/6、相关回归10/10通过；Windows构建和烟测正常。Android 0.9.17构建成功，版本/入口/ARM64/v2签名核验通过，大小与哈希见上方快照；不上传GitHub，不改Kait牌池。详见Docs/v0.9.17-Yummn卡牌平衡.md。

RULE-20260918-LOCKED2048-IDLE：完成，本会话。取消Kait、Yummn现行/旧规则及原2048控制器的锁盘判负/重置；锁死后数字盘保持不变，不移动、不合成、不补入数字，角色与敌方时序照常，等待也可继续。教程/结算同步，应用0.9.16。Locked2048IdleTests 4/4通过；首轮同时抽跑169项旧大类为120通过/49旧断言失败，不作为全量通过，锁盘专项均已改正并由独立测试复核。Windows构建成功并启动烟测，无异常；Android仍0.9.15，不上传。保留他人旧对照报告改动。详见Docs/v0.9.16-锁盘静止.md。

ROLLBACK-20260915-SCENE0916：完成，本会话。用户要求恢复上一版本，0916中央镂空、素材与缩放已撤销；应用与构建配置恢复0.9.15，源码与HEAD一致（仅保留交接/历史记录及他人的报告改动）。Windows重建成功退出0，STORYBOOK0915_QA_COMPLETE，41项偏好恢复，Yummn1920实际图已确认恢复。新增文件归档Logs/reverted0916不参与构建，无Unity/游戏占用；不Android、不上传。

UI-20260915-SCENE0916：已按用户后续要求撤销。中央镂空、场景合成和布局缩放不再使用，新增文件归档Logs/reverted0916；实现与版本恢复0.9.15。原检查与说明仅为历史试验记录，见Docs/v0.9.16-完整场景镂空衔接.md。

BUILD-20260915-0915-ANDROID：完成，本会话。Build/kait-v0.9.15.apk于2026-09-15 10:03:54生成，316349485字节；Logs/storybook0915-android.log最终成功退出0（初轮旧DAG漏新类，Unity自动重建成功）。版本915、最低API26、目标API35、ARM64、UnityPlayerActivity核验通过；v2签名有效且与0.9.14证书一致，IL2CPP元数据包含KaitForestDetail/KaitCornerBough。SHA256 E8AD620BAE140242E8D7CA6B656BFBA979DA6BBFBD80ECC33A1AF9480E1A2FE5。当前源码/Windows/Android均0.9.15；旧APK保留，不改玩法、不上传GitHub，无Unity占用。ADB无设备，未真机启动验收。本记录取代上方快照中Android仍914的时点说明。

RELEASE-20260915-STORYBOOK：完成，本会话。用户授权后，0.9.8—0.9.15绘本界面、正式美术、测试与说明共139个文件以提交f358223推送至origin/master；27个LFS图像对象（约28 MB）上传完成。未纳入未核实的Docs/v0.8.1-四组对照实测.md本地改动，APK仍按.gitignore仅保留本地。仓库继续公开，无Unity占用。

UI-20260915-STORYBOOK0915：完成，本会话。左侧独立1302×1208森林细节、右下树枝透明前景/按轮廓少量搭边、HUD左移10、Kait草地衔接与双角色各六种少缝地砖接入。内置图像生成+获准本地背景抠图，原图保留；整图非4K，未虚称分辨率。首轮树枝直切口已通过贴屏幕边缘修正；隐藏进程首张黑帧检查拦截后改先真实切分辨率，最终四张两角色/两比例图已看，STORYBOOK0915_QA_COMPLETE，41项偏好恢复。Windows和Android最终成功退出0，新类首轮旧DAG漏列后Unity自动重建成功；应用0.9.15、规则存档不变，无Unity/游戏占用。详见Docs/v0.9.15-森林清晰度与铺地衔接.md；保留全部既有未提交工作。

UI-20260914-STORYBOOK0914：完成，本会话。默认7气固定原比例34高，8/9允许外溢不挤压；HUD细边/拳头边距/头像与行动条重排。方键改固定外框与独立下沉内面，矮设置行保留文字高度；薄托盘/内凹格/简约裂纹石，生成雪地衔接层并沿用用户允许的本地抠图保留原RGB。两轮实际看图修正半截色带、刷痕及矮键边距，STORYBOOK0914_QA_COMPLETE，7气/生命/拳头内边界、比例和按钮状态通过，41项偏好恢复。Windows/Android最终成功，签名/版本/入口/架构/元数据/哈希见快照；首轮旧DAG漏新类后Unity自动重建成功。应用0.9.14/914，规则/存档不变，无Unity/游戏占用，不上传GitHub，ADB无设备未真机验收。详见Docs/v0.9.14-界面精修与地面衔接.md。

UI-20260914-STORYBOOK0913：完成，源码/Windows/Android同步，无Unity/游戏占用。72插画/164槽与完整绘本卡盘、0.24秒上浮淡入、替换时暂隐原折叠卡；全屏极简红边、方形双框按钮、紧凑HUD、独立副盘石块/配色、地面边缘与树影已改。方向点击覆盖层专用标识修复按下即被空白取消误判。storybook0913-runtime.log为STORYBOOK0913_QA_COMPLETE：两种技能各四向、六槽边界与实际替换、九气框内通过；20:9/16:9图已看，41项偏好还原。Windows/Android最终构建成功，APK版本/入口/架构/v2签名/IL2CPP元数据核验见快照。Android首轮旧DAG漏新文件后自动重建成功，未留编译错误。应用0.9.13/913，规则/存档不变；ADB无设备未真机验收，不上传，保留其他改动。详见Docs/v0.9.13-卡盘与绘本界面修正.md。

BUILD-20260914-MOBILE0912-ANDROID：完成，本会话。0.9.12/912 APK构建成功退出0，版本、入口、ARM64、v2签名及新布局IL2CPP元数据核验通过，与0.9.11证书一致；大小/哈希见快照。切平台的旧DAG首轮漏列MobileLayoutQA，Unity自动重建后编译成功，未改源码规避。源码/Windows/Android同步；保留旧APK和全部既有未提交改动，不改玩法、不上传GitHub。日志Logs/mobile0912-android.log；ADB无设备，未真机验收，无Unity/游戏占用。

UI-20260914-MOBILE0912：完成，本会话。安全区菜单、实际内容边界放大等大双盘、完整短卡、顶部两行被动/底部主动、选牌工具条和分辨率切换边界已调整；复用批准美术，不改玩法，应用0.9.12。6项专项通过，Windows最终构建成功，真实宽屏及安全区/六卡/展开/奖励看图完成，41项玩家偏好恢复；原截图固定16:9导致漏检的问题已在新QA纠正。源码/Windows同步，Android仍0.9.11（本轮未要求），未真机验收、未上传。无Unity/游戏占用，保留既有未提交工作与其他会话文档。详见Docs/v0.9.12-手机横屏布局.md。

UI-20260914-STORYBOOK0911：完成，本会话。少雪六变体地砖、76×66操作按钮、118框外头像、原KiWispB与32高生命/气已接入；Windows最终构建成功，5项专项与最终实际运行看图通过，41项玩家数据恢复。应用0.9.11，规则/存档0.9.7不变。Android构建成功退出0，版本/ARM64/入口/v2签名与旧包证书核验通过，详情见快照；首次切Android时缓存漏新脚本，自动重建DAG后编译成功，不是未解决编译错误。新地砖Storybook0911原图保留且无本地修改。无Unity/游戏占用；未真机验收、不上传GitHub，保留全部原有未提交改动。详见Docs/v0.9.11-地砖与操作区调整.md。

UI-20260914-STORYBOOK0910：完成，本会话。按C重绘Q版头像/拳头/心/气滴/齿轮、两角色背景/地砖/障碍、右盘边饰和大全绘本页。新素材Storybook0910，原图保留，沿用明确允许的本地背景抠图；6种HUD小图mipmap抗锯齿，其余无mipmap。移除旧地面/树冠叠层；两盘同尺寸更贴合场景，完整绘本浮层不再硬裁卡图/按钮；九气/六心框内排列、重开按钮移除、折叠名居中无小图、内灰框/穿字横线去除，大全紧凑六卡分页。应用0.9.10/910，规则/存档0.9.7不变。Windows最终成功，14/14专项，运行截图检查通过，41项玩家数据恢复；测试临时清空的Standalone宏恢复。无Unity/游戏占用，不Android/GitHub；保留其他会话文档。详见Docs/v0.9.10-统一绘本美术.md，原图及完整提示词在内。

UI-20260914-STORYBOOK099：完成，本会话。对照C图补齐短卡比例、层叠框/厚角、统一可拖拽跨线奖励工具条、原CG头像HUD、雪松灯笼及大全书堆旗帜。图集按已选C风格生成；本轮明确许可本地抠棋盘背景，原图/RGB保留。右盘空槽改灰藕色，数字/障碍颜色不动；UI改用已有Noto字体。应用0.9.9/909，规则/存档仍0.9.7。Windows成功，11/11专项通过，复用Storybook098入口并新增实际选牌检查；storybook099-runtime.log完成无QA错误，41项玩家数据恢复。实看奖励/大全/两角色战斗/设置，修正耗气行高度与边框间距、旗帜裁切；临时纹理支持EditMode清理。详见Docs/v0.9.9-绘本细节补齐.md。未Android、未GitHub上传；原改动和其他会话文档保留，无Unity/游戏占用。

UI-20260914-STORYBOOK098：完成，绘本棋匣界面会话。先将0.9.7源码/正式资源与完整Windows包备份GitHub，PUBLIC不变，远端commit/tag/ZIP摘要均核验。再实施C绘本棋匣：奶油底/烟紫厚线/圆角，战斗HUD置主盘上方，中间无大底板；紧凑卡牌保留图标/名称/耗气冷却，原生银蓝金双风格框，大全角色目录+稀有度六张分页；首页按钮、设置和教程同步。9项专项通过，最终Windows构建成功，STORYBOOK098_QA_COMPLETE及九张运行截图已看；修正树冠遮HUD、按钮字色与边框、旧被动框回退。旧卡回退初次遗漏id导致QA中断，补齐后重跑通过；无须改规则。40项玩家数据恢复。应用0.9.8/908但沿用0.9.7规则牌池存档；本轮不Android、不再次上传0.9.8，保留其他任务的Android097说明和旧对照报告。无Unity/游戏占用，详见v0.9.8-绘本棋匣界面.md。

BUILD-20260914-097-ANDROID：完成，本会话。当前0.9.7 Android构建成功退出0，版本907/ARM64/UnityPlayerActivity及v2签名核验通过，与0.9.3签名证书一致；大小与SHA256见快照。保留旧APK，不改玩法、不上传GitHub。Logs/android097-build.log，无Unity占用；ADB无设备，未真机启动/触摸验收。

RULE-20260914-PALM097：完成源码/Windows，本会话。震颤掌共用ResolveYummnPalmHit，拳击和踢击可互相留印/异向引爆，额外2伤、同向保留、免伤不触发，沿用特效音效；卡面/大全/详细说明同步。应用/规则/存档0.9.7，旧存档保留。palm097-tests.xml 12/12专项通过（含四种拳踢组合、冰冻免伤和上轮反应回归），未做本轮视觉验收；palm097-windows.log构建成功退出0。不Android/上传，无Unity占用，收尾发现游戏kait PID22256运行中，未操作该进程。详见v0.9.7-拳踢共享震颤掌.md。

CARDS-20260914-096：完成源码/Windows，本会话。两种反应各耗1气踢击、气竭可用且不足1气不触发；95张双角色卡面文案统一简化，字号和图案放大、按透明边界统一图案尺寸、边框内留白。首页新增角色/稀有度卡牌大全，6张一页。应用/规则/存档更新0.9.6并保留旧存档。cards096-verified-tests.xml针对性9/9通过（不是全量）；cards096-windows-final.log构建成功退出0；repool-runtime.log的CARDS096_QA_COMPLETE验证首页入口、双角色全部稀有度翻页及文本生成，代表截图与游戏内主动/被动卡已看，40项偏好按原类型原值还原。没有Android/上传，无Unity占用。说明见Docs/v0.9.6-卡牌文本与大全.md。

FIX-20260914-RANGE095：完成源码/Windows，本会话。上轮误解为图层遮挡，已撤回0.9.4预警上层及跟随代码，恢复地面图层。Yummn BuildYummnArrow新增瞄准/开火区分：准备和推动重算不被人物/诱饵截短，开火及受伤危险预测仍按当前目标截断，保留墙/冰柱/黑暗阻挡；Kait原已区分，未改。range095-runtime.log双角色均aim=3/fire=1、WARNINGS_QA_COMPLETE，两张贴脸截图已看；旧六兵种/重叠/死亡检查通过，40项偏好还原。range095-windows-final.log构建成功退出0；应用0.9.5/905，伤害规则/存档不变，无Unity占用，未Android/上传。

FIX-20260914-WARNINGS094：完成源码/Windows，本会话。攻击预警原在地砖子层被独立人物层遮住；改为人物上方的独立Battle Attack Warning Overlay，逐帧同步原格位置/大小/缩放，保留圆角细线、透明度、不拦截输入，不改攻击范围。warnings094-runtime.log WARNINGS_QA_COMPLETE，Kait/Yummn站在射线格及重甲占格两张截图已看，线条连续穿过人物；六兵种、重叠、出手及死亡清理既有运行检查通过。40项偏好原类型原值恢复。warnings094-windows.log构建成功退出0；应用0.9.4/904，规则存档不变。无Unity/游戏占用，未Android/上传。

BUILD-20260914-093-ANDROID：完成，本会话。当前0.9.3已打包，日志android093-build.log成功退出0；签名/版本903/ARM64/UnityPlayerActivity核验通过，与旧包证书一致，包体及哈希见快照。旧APK保留，不改玩法、不上传GitHub，无Unity占用。ADB无设备，未真机启动/触摸验收。

ANIM-20260914-SMILE093：完成源码/Windows，本会话。Yummn胜利smile循环，保留终局锁与首次播放后结算，不改失败/Kait动画；应用0.9.3/903，规则存档不变。Logs/smile093-windows.log构建成功退出0；repool-runtime.log VICTORY_SMILE_QA_COMPLETE，实测原轨道连续2.197轮，结算/待机刷新不覆盖不重启，重新开局清锁；40项偏好恢复。未Android/上传，无Unity/游戏占用。

FIX-20260914-SETTINGS092：完成源码/Windows，本会话。五个数值按钮32高扣除上下5边距只剩22，字体行高与Truncate导致整行无网格；改为完整32高、保留水平边距并取消单行纵向截断。实际Windows运行五行及点击刷新均有文字网格，旧22高布局对照均0顶点；Logs/yummn-defaults-runtime.log为YUMMN_DEFAULTS_QA_COMPLETE，yummn-defaults.settings.png已看，无文字超框。40项偏好按原值恢复。首次QA编译使用了不适用的GetMesh重载，已改为当前API后构建成功；Logs/settings092-windows.log退出0。应用0.9.2/版本码902，规则与存档保留0.9.1；无Unity占用，不Android/上传。

RULE-20260914-MERGE091：完成源码/Windows，本会话。法术回响每次真实合成原位额外触发一次完整合成事件，数值不再翻倍、回响不能复制自身；回气/攻击/奖励/裂隙/异次元袋/丰饶之壶/共鸣均走合成流程。共鸣按结果值合成另外两个同值数字，排除本次结果格，不限新旧或相邻，一次触发一对，后续真实合成可继续联动。应用/规则/牌池/新存档递增0.9.1，AGENTS记录以后自动递增；卡面/教程/版本说明同步。merge091-tests.xml 10/10通过；Windows最终构建成功；repool-runtime.log MERGE091_QA_COMPLETE、merge091-result.png实际盘面已看，2+2保留4且另两4变8，正常补2。首次运行断言漏考虑补2占用空格，已修正验证脚本并重跑通过，未改游戏规则。40项偏好原值还原，无Unity/游戏占用；未Android、未上传。详见v0.9.1-合成回响与共鸣水晶.md。

FIX-20260914-VICTORY-SMILE：完成源码/Windows/Android，本会话。胜利smile映射原本存在，但ShowEnd同帧显示92%不透明遮罩；现显式播放Yummn的000000_smile（Kait保留mana_jump），等待Spine实际完成并保留末帧后显示结算。重复ShowEnd不重启，ResetRunPresentation清理终局等待状态。victory-smile-tests.xml 5/5通过；repool-runtime.log为VICTORY_SMILE_QA_COMPLETE，实际Windows的播放中/结算两张截图已看，轨道前进、无待机排队、延迟结算及状态清理通过；40项玩家偏好原类型原值恢复。Windows/Android构建成功，包与签名信息见快照。无Unity/游戏占用，ADB无设备未真机验收，不上传GitHub。

RULE-VFX-20260914-DARKNESS：完成源码/Windows，本会话。黑暗术改为主棋盘25格任选，含人物/敌人/障碍；保留2气、永久单处、阻箭及禁攻击。内置图像生成重绘透明八帧大色块厚边黑雾，显示在人物上层、目标选择标记下方，不拦截输入；卡面与教程同步。原图备份Backups/Darkness-before-20260914。darkness-tests.xml 24/24通过；darkness-build.log构建成功（首次增量编译短暂报目标标记类型缺失，构建系统刷新后自动重编成功，无需改该脚本）。repool-runtime.log为DARKNESS_QA_COMPLETE，darkness-enemy/self两张真实Windows截图已看，覆盖敌人与自身且旧雾清除；40项玩家偏好按原类型原值恢复。无Unity/游戏占用；未Android、未上传。素材路径及完整提示词见黑暗术任意格与卡通黑雾-20260914.md。

UI-20260914-TARGET-VFX-SETTINGS：完成，本会话。按最后要求恢复上一版青金弧光SkillTargetSelector，本轮大色块重绘稿未接入正式资源。修复Yummn设置页文字可见性，统一字体、不透明度与深色描边；规则开关默认全不勾选，勾选表示偏离默认规则，并校正存档读取、UI同步与运行映射。Logs/settings-default-vfx-tests.xml 3/3通过；恢复原特效后Logs/settings-original-target-tests.xml 2/2通过，Unity编译成功。未构建Windows/Android，未上传GitHub。

INPUT-20260914-SKILL-CLICK-TARGET：完成源码与资源，本会话。主动技能卡现在单击即展开并进入准备状态，再点击主棋盘合法目标释放；点击空白、准备时再次点击任一卡牌或等待5秒会取消并立即折叠。保留拖动兼容，但不再要求拖到释放区。移除原来整格蓝色覆盖，新增SkillTargetSelector透明4×2八帧弧光指示，战场标记统一绘制在人物上层且中心透明；方向技能使用完整格隐形点击面，修复有人格子提示/点击被遮挡。法师之手在副盘同样显示目标标记。图像由内置图像生成工具生成并保存至正式Resources，透明采样0–253。Logs/skill-click-target-tests.xml 3/3通过，Logs/skill-click-cancel-test.xml 1/1通过；Unity编译成功。未做运行截图/真机验收，未构建Windows/Android，未上传GitHub。

BUILD-20260914-PRIORITY-FX-ANDROID：完成，本会话。正式接入18组审批特效与7组审批音效，覆盖碎岩撞击、粉碎音波、守卫刻文、法师之手/魔法飞弹、镜影术、命令术，以及雷鸣波、法力珍珠、镜影共鸣、影刃回响、误导术、持久幻影、横扫追击、重力摆锤、共鸣水晶、法术回响和丰饶之壶等事件；碎岩撞击使用本轮重新生成的厚重石材版。新增YummnPriorityEffect统一播放4×2八帧图，事件层按战场/威胁盘定位。Logs/priority-feedback-tests.xml资源导入专项2/2通过；Logs/priorityfx-android-build.log成功退出0，APK、版本、架构、签名及SHA256见最新Android快照。ADB无设备，未真机验收；未清理其他改动，未上传GitHub。

PREVIEW-20260913-PRIORITY-FEEDBACK-GROUP3：完成第三组待选视觉特效，本会话。按反馈重做碎岩撞击与横扫追击：碎岩改为横向挤压撞墙、墙侧开裂及石片反弹，横扫改用现有近战米白/青灰实体拳风，不再使用暗影魔法轨迹；另生成剩余重力摆锤、共鸣水晶、法术回响、丰饶之壶四套。共6张4×2八帧图及6个320×320 GIF保存在VFXPreviews/PriorityFeedback-20260913-Group3。碎岩与壶的烘入棋盘格按此前授权仅做本地背景清理，其余保留原始透明通道；六张均验证RGBA透明范围0–255。只供选稿，未覆盖正式素材、未接入游戏、未构建、未上传GitHub。

RULE-20260913-MIRROR-KI：完成源码、说明与Windows，本会话。镜影共鸣及范围攻击的残影回气去重粒度由“每次攻击一次”改为“攻击编号×残影编号”；每个实际受击残影恢复1气，同一攻击重复结算同一残影不重复回气，诱饵继续使用独立标识。卡面说明、教程、当前设计文档及旧T08测试同步。Logs/mirror-ki-tests.xml与mirror-ki-cross-tests.xml两项专项各1/1通过，分别验证共鸣两影逐个回气/重复攻击去重/新攻击再次回气，以及术士一次覆盖两影恢复2气。宽范围旧YummnV082Tests运行47/66，其中本次导致的旧T08预期已更新并专项通过，其余18项为当前默认规则相对旧0.8.2断言的既有差异，未据此宣称全量通过。Logs/mirror-ki-build.log构建成功退出0，Windows资源与程序集时间见工程快照；Android未更新、未上传。

PREVIEW-20260913-PRIORITY-FEEDBACK-GROUP2：完成第二组待选视觉特效，本会话。生成横扫追击、雷鸣波、法力珍珠、镜影共鸣、影刃回响、误导术、持久幻影七套4×2八帧图；按机制分别突出折返追击、四向推出、合成回气、同步承伤、四邻反刺、目标偏转和持续延长。生成器把棋盘格烘入RGB后，按用户此前授权仅在本地清理背景，七张均验证RGBA透明范围0–255；另生成七个320×320八帧GIF，保存在VFXPreviews/PriorityFeedback-20260913-Group2。只供选稿，未覆盖正式素材、未接入游戏、未构建、未上传GitHub。

PREVIEW-20260913-PRIORITY-SFX-V2：完成第二版待选音效，本会话。碎岩撞击重做为硬裂、坠击与石屑散落；粉碎音波重做为蓄压、爆发与空气回卷；守卫刻文和命令术拆成独立声音，分别强调四向符文封印与短促命令锁定。按用户选择交换现有候选映射：法师之手使用上一版MagicMissile，魔法飞弹使用上一版MageHand。共6条48kHz双声道WAV保存在AudioPreviews/PriorityFeedback-20260913-v2，仅供试听，未覆盖正式素材、未接入游戏、未构建、未上传GitHub。

PREVIEW-20260913-PRIORITY-FEEDBACK：完成待选稿，本会话。内置图像生成8项优先素材：R40借机攻击踢击卡图，以及碎岩护腕、粉碎音波、守卫刻文、魔法飞弹、法师之手、镜影术、命令术·出手七套透明4×2八帧图；原图与小GIF保存在VFXPreviews/PriorityFeedback-20260913。另用本地原创脚本生成6条48kHz双声道试听WAV至AudioPreviews/PriorityFeedback-20260913。只生成预览，未覆盖正式素材、未接入、未构建Windows/Android、未上传GitHub，无Unity占用。

RULE-20260913-YUMMN-DEFAULTS：完成源码/Windows，本会话。把截图组合固化为Yummn新局默认：7气、按格耗1、击杀回3、有效移动至少一格补一个2、气竭回满、高速攻击耗1、移动不额外推进、4开始出怪、气格挡、16得牌；长按连续输入固定开启但不再显示开关。设置移除长按、攻击推进、8出怪、技能阈值选项；补给选择保留，默认有效移动补2，四档文案统一为“每次/有效移动…补一个2”。旧回放继续使用自身序列化规则。最终专项Logs/yummn-defaults-final-tests.xml为12/12通过；前轮相关范围Logs/yummn-defaults-regression.xml共111项、89通过、22失败，包含既有牌池与旧规则/旧教程断言，未宣称全量通过。Logs/yummn-defaults-build.log构建成功；运行日志YUMMN_DEFAULTS_QA_COMPLETE，最终设置截图已看，删除项无残留、布局无溢出，40项玩家偏好/存档按原类型和值恢复。未Android、未上传，无Unity/游戏占用；保留其他未提交改动。

RULE-20260913-EXHAUST-KILL-FOLLOW：完成源码/Windows，本会话。气竭状态主动踢击击杀后自动进入被击杀敌人格，并记为KillFollow及实际移动；不改变借机攻击等反应踢击，防止反应击杀把人物拉走。详细教程同步说明。Logs/exhaust-kill-follow-final-tests.xml为2/2通过；前一轮宽范围旧回归136项116通过20失败，失败均为当前牌池/旧规则断言，另行保留记录，未宣称全量通过。Logs/exhaust-kill-follow-build.log包含Kait build created并退出0，Windows资源22:11:14、程序集22:11:18。未Android、未上传，无Unity/游戏占用；保留其他未提交改动。

UI-20260913-CARD-CENTER：完成，本会话。展开主动卡图68→92、被动68→100，在标题与说明之间的留白中心放置；主动标题避让圆点装饰。标题21→23、正文18→20、耗气/冷却14→16，长文本保留自动适配；简约小标识框维持76×58。底部费用与反馈互斥，避免重叠，修正折叠文字边距。最终布局专项Logs/card-center-layout-tests.xml为5/5通过；前轮34项中32通过，两项旧断言失败为按旧Flurry资源路径查图、所有技能简约符号强制唯一，与本次布局无关，未宣称全量通过。Logs/card-center-final-build.log包含Kait build created并退出0，Windows资源17:57:43、程序集17:57:47；root090-runtime.log为ROOT090_QA_COMPLETE，最终root090-cards.png已看，主动/被动/跨线、标题与底部耗气显示正常，40项玩家偏好及存档按原类型原值恢复。未更新Android、未上传GitHub，无Unity/游戏占用；保留原有历史报告未提交改动。

RELEASE-20260913-ROOT090：完成，本会话。源码/正式资源/说明172个文件提交b381b01已推送，v0.9.0标签核对一致；56个LFS卡图对象上传完成。https://github.com/Yummn/kait/releases/tag/v0.9.0 为开发预览版，包含APK、安装/存档说明、检查范围与已知问题；APK远端state=uploaded、253232326字节，SHA256 A21EAF07007A3D16FF6B472796BE98FB76135FF83D33C3A37E3DCD94094A7482与本地相同。README及版本记录增加新版入口。保持PUBLIC，无Unity占用；缓存、玩家存档和未审批预览不上传。Docs/v0.8.1-四组对照实测.md的历史数据变化来源未核定，保留本地未提交，不覆盖远端旧报告。

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
