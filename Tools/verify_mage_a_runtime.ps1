$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
foreach ($kind in @('mage-aim', 'mage', 'mage-wall', 'mage-edge', 'arrow', 'hit', 'gameplay')) {
    $shot = Join-Path $project "VFXScreenshots\mage-a-$kind.png"
    $log = Join-Path $project "Logs\mage-a-$kind-player.log"
    $started = Get-Date
    $mode = if ($kind -eq 'gameplay') { '-kaitDemoSteps 12' } else { "-kaitVfxPreview $kind" }
    $arguments = "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 $mode -kaitScreenshot `"$shot`" -logFile `"$log`""
    $process = Start-Process -FilePath (Join-Path $project 'Build\kait.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(45000)) { $process.Kill(); throw "Diagnostic timed out: $kind" }
    if ($process.ExitCode -ne 0) { throw "Player failed: $kind" }
    $file = Get-Item -LiteralPath $shot
    if ($file.LastWriteTime -lt $started -or $file.Length -lt 100000) { throw "Invalid screenshot: $kind" }
    $errors = Select-String -LiteralPath $log -Pattern 'Assertion failed|AssertionException|NullReferenceException|Shader error|IndexOutOfRangeException|MissingReferenceException'
    if ($errors) { throw ($errors | Out-String) }
    Write-Output "$kind : exit=$($process.ExitCode), screenshot=$($file.Length) bytes"
}
