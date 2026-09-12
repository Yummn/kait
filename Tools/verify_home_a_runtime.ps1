param([ValidateSet('home','hold')][string]$Mode='home')
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
if (Get-Process kait,Unity -ErrorAction SilentlyContinue) { throw 'Game or Unity is already running' }
$key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\KaitPrototype\Kait', $true)
if ($null -eq $key) { throw 'Missing player preferences key' }
$snapshot = @{}
foreach ($name in $key.GetValueNames()) { $snapshot[$name] = @{ Kind=$key.GetValueKind($name); Value=$key.GetValue($name) } }
$snapshot | Export-Clixml -LiteralPath (Join-Path $project 'Logs/home-a-playerprefs-backup.xml')
try {
    $proc = Start-Process -FilePath (Join-Path $project 'Build/kait.exe') -ArgumentList "-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -kaitMenuPreview $Mode -kaitScreenshot C:/Users/yummn/Downloads/kait/Logs/$Mode-input -logFile C:/Users/yummn/Downloads/kait/Logs/$Mode-input-runtime.log" -WindowStyle Hidden -PassThru
    if (!$proc.WaitForExit(45000)) { throw "QA still running: PID $($proc.Id)" }
    if ($proc.ExitCode -ne 0) { throw "Player exit $($proc.ExitCode)" }
} finally {
    if ($null -eq $proc -or $proc.HasExited) {
        foreach ($name in $key.GetValueNames()) { if (!$snapshot.ContainsKey($name)) { $key.DeleteValue($name) } }
        foreach ($name in $snapshot.Keys) { $key.SetValue($name, $snapshot[$name].Value, $snapshot[$name].Kind) }
        foreach ($name in $snapshot.Keys) {
            if ($key.GetValueKind($name) -ne $snapshot[$name].Kind -or (Compare-Object @($key.GetValue($name)) @($snapshot[$name].Value))) { throw 'Preference restore mismatch' }
        }
        Write-Output "Restored $($snapshot.Count) player preference values."
    }
    $key.Close()
}
$log = Get-Content -LiteralPath (Join-Path $project "Logs/$Mode-input-runtime.log")
$complete=$Mode.ToUpper()+'_QA_COMPLETE'
if (!($log -match $complete) -or ($log -match 'HOME_QA:|HOLD_QA:|Main menu QA:|NullReferenceException|MissingReferenceException|Shader error')) { throw 'Runtime QA failed; inspect log.' }
Write-Output $complete
