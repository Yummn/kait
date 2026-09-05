$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$shot = Join-Path $project 'VFXScreenshots/main-menu.png'
$log = Join-Path $project 'Logs/main-menu-player.log'
$started = Get-Date
$args = "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -kaitMenuPreview all -kaitScreenshot `"$shot`" -logFile `"$log`""
$p = Start-Process -FilePath (Join-Path $project 'Build/kait.exe') -ArgumentList $args -WindowStyle Hidden -PassThru
if (-not $p.WaitForExit(45000)) { $p.Kill(); throw 'Main menu runtime timeout' }
if ($p.ExitCode -ne 0) { throw "Runtime exited $($p.ExitCode)" }
$errors = Select-String -LiteralPath $log -Pattern 'Main menu QA:|NullReferenceException|MissingReferenceException|IndexOutOfRangeException|Shader error|Exception:'
if ($errors) { throw ($errors | Out-String) }
if (-not (Select-String -LiteralPath $log -Pattern 'Main menu QA passed:')) { throw 'Missing completion marker' }
foreach ($suffix in @('', '.pressed.png', '.tutorial.png', '.settings.png', '.gameplay.png')) {
    $file = Get-Item ($shot + $suffix)
    if ($file.Length -lt 100000 -or $file.LastWriteTime -lt $started) { throw "Stale screenshot $suffix" }
}
Write-Output 'Main menu runtime checks passed; five fresh screenshots written.'
