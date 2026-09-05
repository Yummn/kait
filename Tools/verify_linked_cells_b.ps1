$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
foreach ($kind in @('linkb','linkb-redirect','linkb-blocked','gameplay')) {
    $shot = Join-Path $project "VFXScreenshots\$kind-final.png"
    $log = Join-Path $project "Logs\$kind-player.log"
    $started=Get-Date
    $mode=if($kind -eq 'gameplay'){'-kaitDemoSteps 12'}else{"-kaitVfxPreview $kind"}
    $args="-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 $mode -kaitScreenshot `"$shot`" -logFile `"$log`""
    $p=Start-Process -FilePath (Join-Path $project 'Build\kait.exe') -ArgumentList $args -WindowStyle Hidden -PassThru
    if(-not $p.WaitForExit(45000)){$p.Kill();throw "Timeout: $kind"}
    $file=Get-Item $shot
    if($p.ExitCode -ne 0 -or $file.Length -lt 100000 -or $file.LastWriteTime -lt $started){throw "Failed: $kind"}
    $errors=Select-String -Path $log -Pattern 'Cell signal QA:|NullReferenceException|MissingReferenceException|IndexOutOfRangeException|Shader error'
    if($errors){throw ($errors|Out-String)}
    Write-Output "$kind passed"
}
