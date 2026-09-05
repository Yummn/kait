# Run upstream tools only in the isolated workspace, never initialize Unity as Phaser.
$ErrorActionPreference = 'Stop'
$vibePython = Join-Path $PSScriptRoot '.venv/Scripts/python.exe'
$vibeWorkspace = Join-Path $PSScriptRoot 'workspace'
if ($args.Count -gt 0 -and $args[0] -eq 'web') {
    $vibeWorkspace = Join-Path $vibeWorkspace 'web-demo'
    $args = @($args | Select-Object -Skip 1)
}
if (-not (Test-Path -LiteralPath $vibePython)) { throw 'Install the isolated Python environment first. See README.md.' }
New-Item -ItemType Directory -Force -Path $vibeWorkspace | Out-Null
$oldEncoding = $env:PYTHONIOENCODING
$oldPath = $env:PATH
$env:PYTHONIOENCODING = 'utf-8'
$env:PATH = (Join-Path $PSScriptRoot '.venv/Scripts') + [IO.Path]::PathSeparator + $oldPath
Push-Location $vibeWorkspace
try {
    & $vibePython -X utf8 (Join-Path $PSScriptRoot 'compat_main.py') @args
    $vibeExit = $LASTEXITCODE
} finally {
    Pop-Location
    $env:PYTHONIOENCODING = $oldEncoding
    $env:PATH = $oldPath
}
exit $vibeExit
