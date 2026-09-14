$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$logPath = Join-Path $project 'Logs/yummn-defaults-runtime.log'
$imagePath = Join-Path $project 'Logs/yummn-defaults.settings.png'
$key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\KaitPrototype\Kait', $true)
if ($null -eq $key) { throw 'Missing player preferences key' }

$snapshot = @{}
foreach ($name in $key.GetValueNames()) {
    $snapshot[$name] = @{ Kind=$key.GetValueKind($name); Value=$key.GetValue($name) }
}

try {
    foreach ($name in @($key.GetValueNames())) {
        if ($name.StartsWith('Kait.Yummn082.') -or $name.StartsWith('Kait.Input.HoldRepeat')) {
            $key.DeleteValue($name)
        }
    }
    $key.Close()
    Remove-Item -LiteralPath $logPath, $imagePath -Force -ErrorAction SilentlyContinue
    $arguments = '-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -yummnDefaultsQA 1 -kaitScreenshot C:/Users/yummn/Downloads/kait/Logs/yummn-defaults -logFile C:/Users/yummn/Downloads/kait/Logs/yummn-defaults-runtime.log'
    $proc = Start-Process -FilePath (Join-Path $project 'Build/kait.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (!$proc.WaitForExit(45000)) {
        Stop-Process -Id $proc.Id
        $proc.WaitForExit()
        throw 'Yummn defaults QA timed out'
    }
    if ($proc.ExitCode -ne 0) { throw "Player exit $($proc.ExitCode)" }
} finally {
    if ($null -ne $key) { try { $key.Close() } catch {} }
    $restore = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\KaitPrototype\Kait', $true)
    foreach ($name in @($restore.GetValueNames())) { $restore.DeleteValue($name) }
    foreach ($name in $snapshot.Keys) { $restore.SetValue($name, $snapshot[$name].Value, $snapshot[$name].Kind) }
    $restore.Close()
    Write-Output "Restored $($snapshot.Count) preferences."
}

$log = Get-Content -LiteralPath $logPath
if (!($log -match 'YUMMN_DEFAULTS_QA_COMPLETE') -or ($log -match 'YUMMN_DEFAULTS_QA:|NullReferenceException|MissingReferenceException|Exception:')) {
    throw 'Yummn defaults runtime QA failed'
}
if (!(Test-Path -LiteralPath $imagePath)) { throw 'Yummn defaults screenshot missing' }
$log | Select-String 'YUMMN_DEFAULTS_QA'
