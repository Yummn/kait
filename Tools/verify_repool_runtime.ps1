param([switch]$CardNames,[switch]$PunchUnified,[switch]$DragTarget,[switch]$Darkness,[switch]$VictorySmile,[switch]$Merge091,[switch]$Cards096,[switch]$Storybook098,[switch]$MobileLayout,[switch]$Storybook0913,[switch]$Storybook0914,[switch]$Storybook0915,[switch]$KaitArt0923)
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\KaitPrototype\Kait', $true)
if ($null -eq $key) { throw 'Missing player preferences key' }
$snapshot = @{}
foreach ($name in $key.GetValueNames()) { $snapshot[$name] = @{ Kind=$key.GetValueKind($name); Value=$key.GetValue($name) } }
$snapshot | Export-Clixml -LiteralPath (Join-Path $project 'Logs/repool-playerprefs-backup.xml')
try {
    $qaArgs='-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -repoolQA 1 -kaitScreenshot C:/Users/yummn/Downloads/kait/Logs/repool-start.png -logFile C:/Users/yummn/Downloads/kait/Logs/repool-runtime.log'
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
    if (!$proc.WaitForExit(45000)) { Stop-Process -Id $proc.Id; $proc.WaitForExit(); throw "QA timed out: PID $($proc.Id)" }
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
$log = Get-Content -LiteralPath (Join-Path $project 'Logs/repool-runtime.log')
if($KaitArt0923){
    if(!($log -match 'ART0923_GALLERY_COMPLETE') -or ($log -match 'NullReferenceException|MissingReferenceException|Shader error')){throw 'Card art runtime capture failed'}
    Write-Output 'ART0923_GALLERY_COMPLETE';exit
}
if($Storybook0915){
    if(!($log -match 'STORYBOOK0915_QA_COMPLETE') -or ($log -match 'STORYBOOK0915_QA:|STORYBOOK0914_QA:|MOBILE0912_QA:|NullReferenceException|MissingReferenceException|Shader error')){throw '0915 runtime QA failed'}
    Write-Output 'STORYBOOK0915_QA_COMPLETE';exit
}
$completion = if($Storybook0914){'STORYBOOK0914_QA_COMPLETE'}elseif($Storybook0913){'STORYBOOK0913_QA_COMPLETE'}elseif($MobileLayout){'MOBILE0912_QA_COMPLETE'}elseif($Storybook098){'STORYBOOK098_QA_COMPLETE'}elseif($Cards096){'CARDS096_QA_COMPLETE'}elseif($Merge091){'MERGE091_QA_COMPLETE'}elseif($VictorySmile){'VICTORY_SMILE_QA_COMPLETE'}elseif($Darkness){'DARKNESS_QA_COMPLETE'}else{'REPOOL_QA_COMPLETE'}
if (!($log -match $completion) -or ($log -match 'STORYBOOK0914_QA:|STORYBOOK0913_QA:|MOBILE0912_QA:|STORYBOOK098_QA:|CARDS096_QA:|REPOOL_QA:|DARKNESS_QA:|VICTORY_QA:|MERGE091_QA:|NullReferenceException|MissingReferenceException|Shader error')) { throw 'Runtime QA failed; inspect log.' }
Write-Output $completion
