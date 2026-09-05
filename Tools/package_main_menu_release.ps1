$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$output = 'C:/Users/yummn/Downloads/kait-releases/Kait-v0.5.3-Windows.zip'
if (Test-Path -LiteralPath $output) { throw 'Release archive already exists; do not overwrite it blindly.' }
$names = @('kait.exe','kait_Data','UnityPlayer.dll','UnityCrashHandler64.exe','MonoBleedingEdge','D3D12')
$paths = @($names | ForEach-Object { Join-Path $project "Build/$_" })
foreach ($path in $paths) { if (-not (Test-Path -LiteralPath $path)) { throw "Missing runtime component: $path" } }
Compress-Archive -LiteralPath $paths -DestinationPath $output -CompressionLevel Optimal
Get-FileHash -Algorithm SHA256 -LiteralPath $output, (Join-Path $project 'Build/kait-v0.5.3.apk')
