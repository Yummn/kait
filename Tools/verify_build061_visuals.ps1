$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$qaDirectory = Join-Path $project 'VFXScreenshots/Build061'
New-Item -ItemType Directory -Force -Path $qaDirectory | Out-Null
foreach ($mode in @('reward','owned','cards0','cards1','cards2','cards3','cards4','cards5','cards6','effects','mechanics')) {
    $shot = Join-Path $qaDirectory ($mode + '.png')
    $log = Join-Path $project ('Logs/v061-player-' + $mode + '.log')
    $started = Get-Date
    $arguments = "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -kaitBuild061Preview $mode -kaitScreenshot `"$shot`" -logFile `"$log`""
    $player = Start-Process -FilePath (Join-Path $project 'Build/kait.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (-not $player.WaitForExit(45000)) { $player.Kill(); throw "$mode preview timed out" }
    if ($player.ExitCode -ne 0) { throw "$mode exited $($player.ExitCode)" }
    $errors = Select-String -LiteralPath $log -Pattern 'Build061 QA:|NullReferenceException|MissingReferenceException|IndexOutOfRangeException|Shader error|Exception:'
    if ($errors) { throw ($errors | Out-String) }
    if ($mode -eq 'mechanics' -and -not (Select-String -LiteralPath $log -Pattern 'Build061 runtime QA passed:')) { throw 'Runtime mechanic checks did not finish' }
    $file = Get-Item -LiteralPath $shot
    if ($file.Length -lt 15000 -or $file.LastWriteTime -lt $started) { throw "Missing or stale screenshot: $mode" }
    Write-Output "$mode : fresh runtime screenshot, no logged exceptions"
}
