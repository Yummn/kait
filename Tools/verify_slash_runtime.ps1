$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
foreach ($kind in @('slash', 'slash-left', 'slash-kill', 'slash-kill-left')) {
    $shot = Join-Path $project "VFXScreenshots\$kind.png"
    $log = Join-Path $project "Logs\$kind-player.log"
    $started = Get-Date
    $arguments = "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -kaitVfxPreview $kind -kaitScreenshot `"$shot`" -logFile `"$log`""
    $process = Start-Process -FilePath (Join-Path $project 'Build\kait.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(45000)) { $process.Kill(); throw "Diagnostic timed out: $kind" }
    for ($frame = 0; $frame -lt 9; $frame++) {
        $file = Get-Item -LiteralPath ($shot.Replace('.png', "-$frame.png"))
        if ($file.LastWriteTime -lt $started -or $file.Length -lt 100000) { throw "Invalid frame $kind/$frame" }
    }
    $errors = Select-String -LiteralPath $log -Pattern 'NullReferenceException|Shader error|IndexOutOfRangeException|MissingReferenceException'
    if ($errors) { throw ($errors | Out-String) }
    Write-Output "$kind : exit=$($process.ExitCode), 9 frames captured"
}
