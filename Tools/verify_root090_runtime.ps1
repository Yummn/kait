$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\KaitPrototype\Kait', $true)
if ($null -eq $key) { throw 'Missing player preferences key' }
$snapshot = @{}
foreach ($name in $key.GetValueNames()) { $snapshot[$name] = @{ Kind=$key.GetValueKind($name); Value=$key.GetValue($name) } }
$snapshot | Export-Clixml -LiteralPath (Join-Path $project 'Logs/root090-playerprefs-backup.xml')
try {
    $proc = Start-Process -FilePath (Join-Path $project 'Build/kait.exe') -ArgumentList '-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -root090QA 1 -logFile C:/Users/yummn/Downloads/kait/Logs/root090-runtime.log' -WindowStyle Hidden -PassThru
    if (!$proc.WaitForExit(45000)) { Stop-Process -Id $proc.Id; $proc.WaitForExit(); throw 'RootAction QA timed out' }
    if ($proc.ExitCode -ne 0) { throw "Player exit $($proc.ExitCode)" }
} finally {
    if ($null -eq $proc -or $proc.HasExited) {
        foreach ($name in $key.GetValueNames()) { if (!$snapshot.ContainsKey($name)) { $key.DeleteValue($name) } }
        foreach ($name in $snapshot.Keys) { $key.SetValue($name, $snapshot[$name].Value, $snapshot[$name].Kind) }
        Write-Output "Restored $($snapshot.Count) preferences."
    }
    $key.Close()
}
$log = Get-Content -LiteralPath (Join-Path $project 'Logs/root090-runtime.log')
if (!($log -match 'ROOT090_QA_COMPLETE') -or ($log -match 'ROOT090_QA:|NullReferenceException|MissingReferenceException|Exception:')) { throw 'RootAction runtime QA failed' }
$log | Select-String 'ROOT090_QA'
