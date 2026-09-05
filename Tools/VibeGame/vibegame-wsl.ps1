$ErrorActionPreference = 'Stop'
$linuxScript = & wsl.exe -d Ubuntu --exec wslpath -u (Join-Path $PSScriptRoot 'vibegame-wsl.sh')
if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve the integration path inside Ubuntu.' }
& wsl.exe -d Ubuntu --exec sh $linuxScript.Trim() @args
exit $LASTEXITCODE
