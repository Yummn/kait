$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$shots = Join-Path $project 'VFXScreenshots/RewardDrag'
New-Item -ItemType Directory -Path $shots -Force | Out-Null
foreach ($mode in @('drag','tutorial')) {
    $path = Join-Path $shots $mode
    $log = Join-Path $project "Logs/reward-drag-$mode-player.log"
    $flag = if ($mode -eq 'drag') {'-kaitRewardDragQA 1'} else {'-kaitTutorialPages all'}
    $started = Get-Date
    $player = Start-Process -FilePath (Join-Path $project 'Build/kait.exe') -ArgumentList "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 $flag -kaitScreenshot `"$path`" -logFile `"$log`"" -WindowStyle Hidden -PassThru
    if (-not $player.WaitForExit(45000)) { $player.Kill(); throw "$mode timed out" }
    if ($player.ExitCode -ne 0) { throw "$mode failed: $($player.ExitCode)" }
    $errors = Select-String -LiteralPath $log -Pattern 'RewardDrag QA:|Exception:|Tutorial text overflow|Shader error'
    if ($errors) { throw ($errors | Out-String) }
    if ($mode -eq 'drag') {
        if (-not (Select-String -LiteralPath $log -Pattern 'RewardDrag runtime QA passed:')) { throw 'Drag QA did not finish' }
        foreach ($suffix in @('offer','replace','equipped','copy','copy-ready','done')) {
            $file = Get-Item -LiteralPath "$path.$suffix.png"
            if ($file.Length -lt 15000 -or $file.LastWriteTime -lt $started) { throw 'Missing fresh drag screenshot' }
        }
    } else {
        $pages = @(Select-String -LiteralPath $log -Pattern 'Tutorial page QA: page=\d+, art=True, overflow=0')
        if ($pages.Count -ne 10) { throw 'Expected all ten tutorial pages to load without overflow' }
        if (-not (Select-String -LiteralPath $log -Pattern 'Tutorial close QA: closed=True')) { throw 'Tutorial did not close' }
    }
    "$mode runtime checks passed"
}
