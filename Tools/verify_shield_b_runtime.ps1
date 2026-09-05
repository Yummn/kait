$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
foreach ($kind in @('shield-right','shield-up','shield-left','shield-down','shield-block','shield-side')) {
    $shot = Join-Path $project "VFXScreenshots\$kind-b.png"
    $log = Join-Path $project "Logs\$kind-b-player.log"
    $started = Get-Date
    $args = "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -kaitVfxPreview $kind -kaitScreenshot `"$shot`" -logFile `"$log`""
    $p = Start-Process -FilePath (Join-Path $project 'Build\kait.exe') -ArgumentList $args -WindowStyle Hidden -PassThru
    if (-not $p.WaitForExit(45000)) { $p.Kill(); throw "Timeout: $kind" }
    if ($p.ExitCode -ne 0) { throw "Exit failure: $kind" }
    $file = Get-Item $shot
    if ($file.LastWriteTime -lt $started -or $file.Length -lt 100000) { throw "Screenshot failed: $kind" }
    $errors = Select-String -Path $log -Pattern 'Shield QA:|NullReferenceException|MissingReferenceException|IndexOutOfRangeException|Shader error'
    if ($errors) { throw ($errors | Out-String) }
    Write-Output "$kind passed"
}
