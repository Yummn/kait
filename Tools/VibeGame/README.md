# Kait 的 VibeGame 辅助工具

Unity 仍是唯一游戏引擎。本目录把 VibeGame 作为独立素材工具和网页实验环境使用，不会把 Kait 转成 Phaser，也不会将 Python、浏览器或 AI 服务打进游戏。

## 已接入的部分

- 本地素材分析、去底色、切帧、图集拼接、像素处理、图片变换、动画视频导出和拆帧。
- Unity 菜单 `Kait → VibeGame`：查看说明、测试报告、打开暂存区、导入确认过的图片。
- 导入只接受暂存区的 PNG/JPG，保留原图像素，不覆盖同名素材。默认 Sprite、Point、无压缩、不生成 Mipmap；导入后可以在 Inspector 自行改为 Bilinear 等设置。
- 网页实验目录：运行上游提供的切水果模板，检查它的运行控制、截图和工作台。模板用色块占位是原始模板内容，不是给 Kait 新做的美术。

## 边界

这是开发期辅助接入，不是官方 Unity 后端。工作台的 Objects、Play 和 Runtime API 操作的是 `workspace/web-demo`，不是 Unity 场景、Spine 或 Kait.exe。

AI 生图、参考图重绘、智能抠图/切帧、视觉审核、视频生成和语义分层尚未配置外部服务，未做付费实测。WSL/Ubuntu 和 tmux 已可用，Linux 侧素材与网页测试通过；多智能体开发仍需要配置对应的模型 CLI 和登录，尚未启用。没有修改用户的全局 Codex/Claude 配置或安装全局 hooks。

Unity 图集自动切片、Spine 骨骼生成、将图集绑定到已有角色动画都不在本次接入中。图集可作为 PNG 导入，再用 Unity Sprite Editor 切片。现有角色、音效和游戏规则没有被替换。

## 日常使用

在本目录打开 PowerShell。工具会自动使用隔离的 Python 环境；普通命令工作目录是 `workspace`。

```powershell
# 查看命令
.\vibegame.ps1 art --help

# 查看图片颜色和透明度（把图片放入 workspace/input）
.\vibegame.ps1 art analyze input/sample.png

# 示例：去掉洋红底色，保存为新图，不动源文件
.\vibegame.ps1 art rmbg input/sample.png -c 255,0,255 --seed match -o output/sample-clean.png

# 拼接已准备好的动作帧
.\vibegame.ps1 art concat input/frames -o output/action-atlas.png --layout row --spacing 2

# 导出动画预览
.\vibegame.ps1 art f2v input/frames --fps 12 -o output/action.mp4

# 查看配置状态，只显示密钥是否存在，不显示密钥
.\.venv\Scripts\python.exe -X utf8 bridge.py status

# 重跑本地工具测试 / 网页测试
.\.venv\Scripts\python.exe -X utf8 bridge.py test
.\.venv\Scripts\python.exe -X utf8 bridge.py web-test
```

最终确认过的图片放入 `workspace/output`，回到 Unity 使用 `Kait → VibeGame → 导入已确认的图片`。目标目录为 `Assets/Art/VibeGame`。导入后仍需主动将素材分配给需要的 Image/Sprite，不会替换现有表现。

命令行也可以导入，之后 Unity 会刷新资源：

```powershell
.\.venv\Scripts\python.exe -X utf8 bridge.py import-asset workspace/output/sample-clean.png --dry-run
.\.venv\Scripts\python.exe -X utf8 bridge.py import-asset workspace/output/sample-clean.png
```

## 网页实验与工作台

```powershell
# 无智能体的工作台：本机访问 http://127.0.0.1:18766
.\.venv\Scripts\python.exe -X utf8 bridge.py dashboard

# 另一个终端启动网页 Runtime（Windows 不使用上游的 -b 参数）
.\vibegame.ps1 web run . --headless --activate --port 18765

# 再开终端操作这个网页示例
.\vibegame.ps1 web play --port 18765 input -a start
.\vibegame.ps1 web play --port 18765 continue -f 60
.\vibegame.ps1 web play --port 18765 snapshot
.\vibegame.ps1 web close . --port 18765
```

工作台未启动智能体，Chat 不会自动回复。不要在 Kait 根目录执行 `vibegame init` 或启动上游自动开发流程，否则会混入 Phaser 的工程配置。

## 安装与迁移

上游版本固定为 `cab478bf2dafe93bd586aa1043a1e2182f4da197`，许可证见 `upstream/LICENSE`。上游源码没有修改。

换电脑后，需要 Python 3.12+、Git 和网络。在本目录运行：

```powershell
.\install.ps1 -Python '你的 Python 3.12 或更新版本的 python.exe 路径'
```

安装器使用本目录的 `.venv`，按上游 requirements.txt 锁定依赖，再安装 imageio-ffmpeg 0.6.0 和 Playwright Chromium。浏览器安装在 Playwright 的用户级缓存，其余依赖留在本目录，不会改动系统 PATH。

`compat_main.py` 只为中文 Windows 修复进程存活检测：上游通过 tasklist 文本判断进程，UTF-8 与 GBK 不一致时会崩溃；适配入口改用 Windows 进程查询 API。生图配置只在用户明确配置本目录 `.env` 后提供给工具，文件已被 Git 忽略。

## WSL / Ubuntu 使用入口

Linux 侧已安装在 `/home/yummn/.local/share/kait-vibegame`，包含 uv 0.12.9、独立 Python 3.12.14 环境和相同固定版本的上游源码。系统自带 tmux 3.6，另已安装 FFmpeg 与 Chromium 所需系统库。Linux Python 环境不与 Windows 的 `.venv` 混用。

在本目录的 PowerShell 中运行：

```powershell
.\vibegame-wsl.ps1 bridge status
.\vibegame-wsl.ps1 art --help
.\vibegame-wsl.ps1 bridge test
.\vibegame-wsl.ps1 bridge web-test
.\vibegame-wsl.ps1 bridge dashboard
```

工作台启动后地址是 `http://127.0.0.1:18766`，只在本机监听，不启动模型会话。`web` 命令使用 Linux 自己的网页示例 `/home/yummn/.local/share/kait-vibegame/workspace/web-demo`；普通素材命令仍使用 Windows 项目中的 `workspace`，产物放入 `output` 后可以从 Unity 菜单手动导入。传入命令的文件路径需使用 Linux 路径或相对路径。

Linux 网页示例放在 Linux 文件系统，是因为上游初始化会复制文件权限，直接跨文件系统复制到 `/mnt/c` 曾报错。第一次失败的示例副本保留在 `evidence/wsl/initial-copy-failed`，不用于运行。

当前 Ubuntu 为 26.04，而上游固定的 Playwright 1.58 没有该系统的下载映射。`wsl-environment.sh` 仅对该系统的 x86_64 环境设置 Ubuntu 24.04 构建兼容选项，不改系统版本，也不改上游包；已通过浏览器启动、交互、截图和录制回放测试，但不能据此声称所有浏览器功能均受官方支持。

`install-wsl.sh` 可重复安装 Linux 侧依赖（需要已安装此目录下的 uv 与系统库），不重置不同版本的源码，不安装全局 hooks，不启动付费服务。uv 安装方法见 [官方文档](https://docs.astral.sh/uv/getting-started/installation/)；本机使用独立安装目录且关闭了 shell PATH 修改。

## 版本管理与回退

接入前 Unity 版本：`dc0a655`（v0.5）。新增内容是 `Tools/VibeGame`、编辑器脚本 `KaitVibeGameTools.cs` 和测试 `KaitVibeGameTests.cs`，没有游戏运行时代码改动。

上游 checkout、虚拟环境、工作区、测试输出和 `.env` 均已忽略，不会误上传缓存或密钥。这里只保留接入脚本和说明供版本管理；本次未提交或推送 GitHub。禁用时先关闭工具进程，再移走新增编辑器脚本和本目录；已主动导入的素材可按需保留。
