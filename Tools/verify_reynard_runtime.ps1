param([switch]$CardNames,[switch]$PunchUnified,[switch]$DragTarget,[switch]$Darkness,[switch]$VictorySmile,[switch]$Merge091,[switch]$Cards096,[switch]$Storybook098,[switch]$MobileLayout,[switch]$Storybook0913,[switch]$Storybook0914,[switch]$Storybook0915,[switch]$KaitArt0923)
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\KaitPrototype\Kait', $true)
if ($null -eq $key) { throw 'Missing player preferences key' }
$snapshot = @{}
foreach ($name in $key.GetValueNames()) { $snapshot[$name] = @{ Kind=$key.GetValueKind($name); Value=$key.GetValue($name) } }
$snapshot | Export-Clixml -LiteralPath (Join-Path $project 'Logs/repool-playerprefs-backup.xml')
try {
    $qaArgs='-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -reynardQA 1 -kaitScreenshot C:/Users/yummn/Downloads/kait/Logs/repool-start.png -logFile C:/Users/yummn/Downloads/kait/Logs/reynard-runtime.log'
    if($CardNames){$qaArgs+=' -cardNamesQA 1'}
    if($PunchUnified){$qaArgs+=' -punchUnifiedQA 1'}
    if($DragTarget){$qaArgs+=' -dragTargetQA 1'}
    if($Darkness){$qaArgs+=' -darknessQA 1'}
    if($VictorySmile){$qaArgs+=' -victorySmileQA 1'}
    if($Merge091){$qaArgs+=' -merge091QA 1'}
    if($Cards096){$qaArgs+=' -cards096QA 1'}
    if($KaitArt0923){$qaArgs+=' -kaitArt0923QA 1'}
    if($Storybook098){$qaArgs+=' -storybook098QA 1'}
    if($MobileLayout){$qaArgs+=' -mobileLayoutQA 1'}
    if($Storybook0913){$qaArgs+=' -storybook0913QA 1'}
    if($Storybook0914){$qaArgs+=' -storybook0914QA 1'}
    if($Storybook0915){
        # A hidden Windows player may not present its initial swapchain until
        # the first real resize. Start at a different size from the first test.
        $qaArgs=$qaArgs.Replace('-screen-width 1920 -screen-height 1080','-screen-width 1600 -screen-height 900')
        $qaArgs+=' -storybook0915QA 1'
    }
    $proc = Start-Process -FilePath (Join-Path $project 'Build/kait.exe') -ArgumentList $qaArgs -WindowStyle Hidden -PassThru
    if (!$proc.WaitForExit(90000)) { Stop-Process -Id $proc.Id; $proc.WaitForExit(); throw "QA timed out: PID $($proc.Id)" }
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

