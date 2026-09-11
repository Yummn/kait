param([string]$OutputPrefix = 'warnings-acb')
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$keyPath = 'Software\KaitPrototype\Kait'
$key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($keyPath, $true)
if ($null -eq $key) { throw 'Player preference key missing; inspect before QA.' }
$snapshot = @{}
foreach ($name in $key.GetValueNames()) { $snapshot[$name] = @{ Kind=$key.GetValueKind($name); Value=$key.GetValue($name) } }
$snapshot | Export-Clixml -LiteralPath (Join-Path $project "Logs/$OutputPrefix-playerprefs-backup.xml")
try {
    $proc = Start-Process -FilePath (Join-Path $project 'Build/kait.exe') -ArgumentList "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -kaitWarningsQA 1 -kaitScreenshot C:/Users/yummn/Downloads/kait/Logs/$OutputPrefix -logFile C:/Users/yummn/Downloads/kait/Logs/$OutputPrefix-runtime.log" -WindowStyle Hidden -PassThru
    if (!$proc.WaitForExit(45000)) { throw "QA still running: PID $($proc.Id); do not start another player." }
    if ($proc.ExitCode -ne 0) { throw "Player exit $($proc.ExitCode)" }
} finally {
    if ($null -eq $proc -or $proc.HasExited) {
        foreach ($name in $key.GetValueNames()) { if (!$snapshot.ContainsKey($name)) { $key.DeleteValue($name) } }
        foreach ($name in $snapshot.Keys) { $key.SetValue($name, $snapshot[$name].Value, $snapshot[$name].Kind) }
        foreach ($name in $snapshot.Keys) {
            if ($key.GetValueKind($name) -ne $snapshot[$name].Kind -or (Compare-Object @($key.GetValue($name)) @($snapshot[$name].Value))) { throw 'Preference restore mismatch' }
        }
        Write-Output "Restored $($snapshot.Count) player preference values, with exact types and values."
    }
    $key.Close()
}
$log = Get-Content -LiteralPath (Join-Path $project "Logs/$OutputPrefix-runtime.log")
if (!($log -match 'WARNINGS_QA_COMPLETE') -or ($log -match 'WARNINGS_QA:|NullReferenceException|MissingReferenceException|Shader error')) { throw 'Runtime QA failed; inspect log.' }
Write-Output 'WARNINGS_QA_COMPLETE'
