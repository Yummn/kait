param([string]$Python = 'python')
$ErrorActionPreference = 'Stop'
$revision = 'cab478bf2dafe93bd586aa1043a1e2182f4da197'
$upstream = Join-Path $PSScriptRoot 'upstream'
$environment = Join-Path $PSScriptRoot '.venv'

function Run-Checked([string]$Program, [string[]]$Arguments) {
    & $Program @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Command failed: $Program (exit $LASTEXITCODE)" }
}

Run-Checked $Python @('-c', 'import sys; assert sys.version_info >= (3,12), "Python 3.12 or newer is required"')
if (-not (Test-Path -LiteralPath $upstream)) {
    Run-Checked 'git' @('clone', 'https://github.com/tettethu/VibeGame.git', $upstream)
    Run-Checked 'git' @('-C', $upstream, 'checkout', '--detach', $revision)
}
$actual = & git -C $upstream rev-parse HEAD
if ($LASTEXITCODE -ne 0 -or $actual -ne $revision) { throw 'Upstream revision differs. No files were reset or overwritten.' }
if (-not (Test-Path -LiteralPath $environment)) { Run-Checked $Python @('-m', 'venv', $environment) }
$toolPython = Join-Path $environment 'Scripts/python.exe'
Run-Checked $toolPython @('-m', 'pip', 'install', '-e', $upstream, '-c', (Join-Path $upstream 'requirements.txt'))
Run-Checked $toolPython @('-m', 'pip', 'install', 'imageio-ffmpeg==0.6.0')
$ffmpegSource = & $toolPython -c 'import imageio_ffmpeg; print(imageio_ffmpeg.get_ffmpeg_exe())'
if ($LASTEXITCODE -ne 0) { throw 'Could not locate bundled FFmpeg.' }
$ffmpegTarget = Join-Path $environment 'Scripts/ffmpeg.exe'
if (-not (Test-Path -LiteralPath $ffmpegTarget)) { Copy-Item -LiteralPath $ffmpegSource -Destination $ffmpegTarget }
Run-Checked $toolPython @('-m', 'playwright', 'install', 'chromium')
$workspace = Join-Path $PSScriptRoot 'workspace'
New-Item -ItemType Directory -Force -Path (Join-Path $workspace 'output') | Out-Null
$demo = Join-Path $workspace 'web-demo'
if (-not (Test-Path -LiteralPath $demo)) {
    Copy-Item -LiteralPath (Join-Path $upstream 'src/skeletons/swipe-slice-arcade') -Destination $demo -Recurse
    Run-Checked $toolPython @('-X', 'utf8', (Join-Path $PSScriptRoot 'compat_main.py'), 'init', $demo, '--lang', 'zh-CN', '--choice', 'skip', '--no-commit')
}
Write-Host 'Installed as a Unity sidecar. No global agent hooks or API credentials were configured.'
