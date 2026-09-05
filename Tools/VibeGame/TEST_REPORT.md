# VibeGame 接入测试记录

测试日期：2026-09-04。最新复测完成时间：21:25（北京时间）。当前环境：Windows / WSL Ubuntu 26.04，Unity 6000.0.30f1，两侧独立 Python 3.12.14。

结论：素材处理工具和独立网页测试环境可用；Unity 图片导入已接通。AI 生图及自动开发团队尚未启用，不能视为全功能接入。

## 最新复测：2026-09-04 21:25

本次范围是 VibeGame 接入功能，不是 Kait 所有游戏规则。以下结果来自本次重新运行，不沿用上午的通过记录。

| 测试组 | 本次结果 | 范围 |
| --- | --- | --- |
| Windows 素材工具 | 16/16 通过 | 图片分析、去底色、切帧、图集、翻转缩放、边缘/像素处理、动画导出拆帧、配置及导入边界检测 |
| Windows 网页工具 | 11/11 通过 | 校验、启动、输入、推进帧、截图、属性修改、录制、刷新、关闭、工作台 |
| WSL 素材工具 | 16/16 通过 | 同组本地工具在 Linux 下复测 |
| WSL 网页工具 | 12/12 通过 | 包含实际执行录制生成的 Bash 回放 |
| Unity 导入接入 | 2/2 通过 | 实际 PNG 导入及 Sprite 设置、像素保持、防覆盖、拒绝暂存区外路径 |
| WSL 基础环境 | 通过 | tmux 创建/检测/关闭独立测试会话；63 个 Python 包依赖兼容检查 |

共 57 个通过的自动测试用例，包含跨平台重复验证和配置检测，不代表 57 种独立功能。没有调用付费 API。Unity 本次仅筛选 `KaitVibeGameTests`，不能把下文历史 234/234 当成本次全游戏回归结果。

补充核验：两侧缩放产物均为 128×128，视频均拆出 3 张可读取的 64×64 PNG。已目视检查本次 Windows/Linux Runtime 和 Linux Objects 页截图：网页示例正常显示，Objects 能列出模板；未测试每一个属性编辑控件。测试结束后未发现残留 Runtime、Dashboard、测试浏览器或测试导入图片。

本次 Unity 原始结果：`../../Logs/VibeGame-Current-EditMode.xml`（2 通过、0 失败、0 跳过）。Windows/WSL 原始 JSON 和截图路径见下文，均已更新为本次结果。

项目已有其他未提交的游戏与美术改动，本次没有更改或回退它们。Unity 测试启动时自动移除了 Standalone 的 `SENTIS_ANALYTICS_ENABLED`，已精确恢复为测试前状态；Packages、ProjectSettings 最终无差异。没有构建 EXE/APK，没有提交或推送 GitHub。

当前服务检测仍显示 IMAGE_API_KEY、VLM_API_KEY、VIDEO_API_KEY、QWEN_SERVER_URL 均未配置。因此 AI 生图/改绘、视觉理解、AI 视频、语义分层均未实测；模型团队也没有启动。Windows/WSL 上游源码保持固定提交 `cab478bf2dafe93bd586aa1043a1e2182f4da197` 且无本地修改。

## 首次接入及安装记录

以下保留首次接入与 WSL 安装过程；其中 Unity 全量测试数量属于历史记录。

## 本地工具：16 项通过

| 项目 | 结果与验证方式 |
| --- | --- |
| CLI | 帮助和子命令可以启动 |
| 图片分析 | 成功读出尺寸、透明度与主色 |
| 素材目录扫描 | 成功列出测试图片 |
| 本地去背景 | 背景 Alpha 变为 0，前景 RGBA 保持不变 |
| 自动连通区域切帧 | 从测试图准确提取 3 个精灵；不是 VLM 智能识别 |
| 图集拼接 | 得到正确尺寸的图集及 manifest |
| 翻转 | 与预期翻转图逐像素一致 |
| 缩放 | 64×64 输出为 128×128 |
| 边缘清理 | 成功输出 PNG |
| 像素网格处理 | 成功输出 PNG |
| 像素矩阵编解码 | 往返后 RGBA 像素一致 |
| 动画导出 | MP4 可解码，帧数为 3 |
| 视频拆帧 | 成功从测试视频导出帧 |
| 生图服务配置检测 | 正确提示未配置密钥；没有实际生图 |
| Unity 辅助环境状态 | 正确识别项目及缺失服务 |
| 素材导入边界检查 | 拒绝暂存区外路径；预演不会写入 Assets |

原始结果：`evidence/art-results.json`。用合成测试图验证工具，不把测试图当作正式美术质量证明。测试不会替换 Kait 素材。

## 网页环境：11 项通过

使用上游自带切水果模板，不是把 Kait 迁移到了网页。

- 工程结构、资源引用校验通过。
- 无界面 Runtime 正常启动，能在第 0 帧暂停并读取 21 个初始节点。
- 输入开始操作后推进 60 帧：状态进入 playing，生成 2 个目标。
- 成功截图，已人工查看，画面不是黑屏。
- 运行时修改属性后能够读回。
- 开始录制操作、录制期间推进帧、结束录制均成功。
- 刷新 Runtime 成功。
- Runtime 关闭成功。
- 无智能体 Dashboard 页面可显示；Assets/Objects 数据树与页签经过检查。

原始结果：`evidence/web-results.json`；运行截图：`evidence/web-runtime.png`；工作台截图：`evidence/dashboard.png`。

录制得到上游 Bash 脚本；本次验证了录制，没有声称 Windows 上的 Bash 重放已验证。Objects 的全部编辑控件、动画边界编辑、每一种模板也未逐项覆盖。

## Unity：234/234 通过

原有 232 项 EditMode 测试，加上 2 项接入测试全部通过。新增测试实际导入临时 PNG，确认像素不变、能加载为 Sprite、无压缩、不生成 Mipmap、采用 Point 过滤，并拒绝同名覆盖。测试生成的临时资源已自动清除，只保留空的导入目录。

测试结果：`../../Logs/VibeGame-EditMode.xml`。`Assets/Scripts`、Packages、ProjectSettings 没有保留改动。未重新编译 EXE/APK：此次只新增开发工具和编辑器菜单，运行程序不需要这些工具。

## 实测发现并处理的问题

1. 动画导出初次失败：环境缺少 FFmpeg。已在隔离环境补充 imageio-ffmpeg 0.6.0，复测通过。
2. 上游 Windows 后台 `-b` Runtime 不支持：测试改用隐藏进程运行前台命令，结束后关闭，不修改上游源码。
3. 中文 Windows 进程检测报错：上游 tasklist 的 GBK 输出被当作 UTF-8 解码。新增 `compat_main.py` 使用 Windows API 查询进程；已验证当前进程、无效 PID 和失效 PID，再跑网页测试通过。

## 尚未配置或尚未验证

| 功能 | 当前状态 |
| --- | --- |
| 文字生图、参考图改绘、AI 生成动画帧 | 缺少用户选择的图像服务与 API 配置，没有实际调用 |
| 智能抠图/切帧、视觉审核与 AI 试玩判断 | 缺少 VLM 配置；本地抠图不等于这些功能已通过 |
| AI 视频生成 | 缺少视频服务；上游标为实验功能，需要另行确认成本 |
| Qwen 语义分层 | 缺少对应 GPU 服务 |
| 自动策划、编程、审查、团队聊天、经验提炼 | WSL、tmux、Linux 侧 VibeGame 依赖已安装并验证；模型 CLI 与服务尚未配置完成，未启动模型会话 |
| VibeGame 自动控制 Unity 场景/Spine/EXE | 上游未提供 Unity 后端，本次也未实现运行时控制桥 |
| 导出后自动挂接现有动画、Unity 图集自动切片 | 未实现；当前为手动确认图片导入 |

## WSL 补充安装与实测（2026-09-04）

Ubuntu 首次初始化期间，普通用户和 root 命令一度超时。用户完成启动提示后恢复响应；无需重启或关闭用户的安装终端。

安装结果：WSL 2.7.12、Ubuntu 26.04、默认用户 `yummn`；tmux 3.6 创建会话测试通过。工具使用独立 Python 3.12.14，而不是系统 Python 3.14.4；uv 0.12.9 安装在独立目录，没有修改 shell 启动文件。63 个 Python 包通过依赖兼容检查，FFmpeg 和 Chromium 系统依赖安装完成。

| 补充验证 | 结果 |
| --- | --- |
| Linux 素材工具 | 16/16 通过，包含去背景像素检查、切帧、图集、MP4 解码、Unity 导入边界预演 |
| Linux 网页实验 | 12/12 通过，包含浏览器启动、输入、推进帧、属性修改、截图、录制、Bash 回放、关闭与 Dashboard |
| Bash 回放 | 实际执行本次生成的脚本，确认从第 0 帧推进到第 2 帧，再关闭控制会话；以 pipefail 检测命令失败 |
| Windows 回归 | 素材 16/16、网页 11/11，再次全部通过 |
| 视觉检查 | 已查看 Linux Runtime 与 Objects 页面截图，示例正常显示；不是 Kait 游戏画面 |
| 收尾清理 | 未留下测试浏览器、Runtime、Dashboard 或 tmux 测试会话；Linux 监听列表只剩系统 DNS |
| Unity 保持情况 | 再次检查 Assets/Scripts、Packages、ProjectSettings 无差异；本轮未修改 C#，未重跑 Unity 测试或构建 EXE/APK |

原始结果：`evidence/wsl/art-results.json`、`evidence/wsl/web-results.json`；截图与回放输出在同一目录。Windows 原始结果仍单独保留。

兼容处理与限制：

- 固定版本 Playwright 1.58 没有 Ubuntu 26.04 的浏览器下载映射。入口仅针对该系统设置 `PLAYWRIGHT_HOST_PLATFORM_OVERRIDE=ubuntu24.04-x64`，已完成上述实测；仍属于兼容运行，不代表该固定版本正式支持 26.04。未升级或改写上游代码。
- 上游初始化用带权限元数据的复制方式，直接从 Linux 源码复制到 `/mnt/c` 时报 Operation not permitted。网页示例改放 Linux 文件系统后初始化成功；失败的暂存副本保留在 `evidence/wsl/initial-copy-failed`。
- 回放测试第一次失败是测试在 deactivate 之后还索取快照，此时控制会话已结束；改为严格验证回放的三条实际响应后通过，没有修改上游回放行为。
- Linux 上关闭前台 Runtime 偶尔会打印 failed to stop；本次等待启动进程退出后，另外核对进程与端口，确认无残留。未将此提示当成游戏异常。
- 生图、VLM、视频等密钥仍未配置。此次无付费模型调用，没有测试真实 AI 开发团队，也没有将网页 Runtime API 接入 Unity 游戏。

没有执行付费测试，没有设置全局模型或信任 hooks。WSL/Linux 已安装；后续若要启用 AI 功能，需要配置所需服务及模型 CLI 的登录。不能据当前测试结果认定完整自动开发工作流可用。
