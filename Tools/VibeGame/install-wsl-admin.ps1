# Invoked with Windows UAC consent. Never reboots the machine automatically.
$ErrorActionPreference = 'Stop'
$logRoot = Join-Path $PSScriptRoot 'evidence'
New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
$resultFile = Join-Path $logRoot 'wsl-install-result.json'
try {
    $principal = [Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Administrator permission is required to install WSL.'
    }
    $process = Start-Process -FilePath "$env:SystemRoot\System32\wsl.exe" `
        -ArgumentList @('--install', '--no-distribution', '--web-download') `
        -WindowStyle Hidden -PassThru -Wait `
        -RedirectStandardOutput (Join-Path $logRoot 'wsl-install.stdout') `
        -RedirectStandardError (Join-Path $logRoot 'wsl-install.stderr')
    @{ time = (Get-Date).ToString('o'); exitCode = $process.ExitCode; restarted = $false } |
        ConvertTo-Json | Set-Content -LiteralPath $resultFile -Encoding UTF8
} catch {
    @{ time = (Get-Date).ToString('o'); error = $_.Exception.Message; restarted = $false } |
        ConvertTo-Json | Set-Content -LiteralPath $resultFile -Encoding UTF8
    exit 1
}
