$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
foreach ($kind in @('push', 'push-left', 'push-up', 'push-down', 'hit', 'kill', 'block')) {
    $shot = Join-Path $project "VFXScreenshots\push-a-$kind.png"
    $log = Join-Path $project "Logs\push-a-$kind-player.log"
    $started = Get-Date
    $arguments = "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -kaitVfxPreview $kind -kaitScreenshot `"$shot`" -logFile `"$log`""
    $process = Start-Process -FilePath (Join-Path $project 'Build\kait.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(45000)) { $process.Kill(); throw "Diagnostic timed out: $kind" }
    $file = Get-Item -LiteralPath $shot
    if ($file.LastWriteTime -lt $started -or $file.Length -lt 100000) { throw "Invalid screenshot: $kind" }
    $errors = Select-String -LiteralPath $log -Pattern 'NullReferenceException|Shader error|IndexOutOfRangeException|MissingReferenceException'
    if ($errors) { throw ($errors | Out-String) }
    Write-Output "$kind : exit=$($process.ExitCode), screenshot=$($file.Length) bytes"
}
