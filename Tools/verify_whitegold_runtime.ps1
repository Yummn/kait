$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$player = Join-Path $project 'Build\kait.exe'
$shots = Join-Path $project 'VFXScreenshots'
New-Item -ItemType Directory -Path $shots -Force | Out-Null
foreach ($kind in @('hit', 'kill', 'chain', 'block')) {
    $shot = Join-Path $shots ("whitegold-" + $kind + '.png')
    $log = Join-Path $project ("Logs\whitegold-player-" + $kind + '.log')
    $arguments = "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -kaitVfxPreview $kind -kaitScreenshot `"$shot`" -logFile `"$log`""
    $started = Get-Date
    $process = Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(45000)) {
        # Stop only this diagnostic child, never a user's game instance.
        $process.Kill()
        throw "Timed out capturing $kind"
    }
    $file = Get-Item -LiteralPath $shot -ErrorAction Stop
    if ($file.LastWriteTime -lt $started -or $file.Length -lt 100000) { throw "Blank or invalid screenshot: $shot" }
    $errors = Select-String -LiteralPath $log -Pattern 'NullReferenceException|Shader error|IndexOutOfRangeException|MissingReferenceException'
    if ($errors) { throw ($errors | Out-String) }
    Write-Output "$kind : exit=$($process.ExitCode), screenshot=$($file.Length) bytes"
}
