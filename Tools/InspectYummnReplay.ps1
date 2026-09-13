param([string]$ReplayPath)
$ErrorActionPreference='Stop'
# Read-only archive inspection. Never replay a 38-card RNG stream in the 56-card engine.
if ($ReplayPath) {
    $raw=Get-Content -LiteralPath $ReplayPath -Raw
} else {
    $key=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\KaitPrototype\Kait',$false)
    if (!$key) { throw 'No local player preferences found.' }
    try {
        $name=$key.GetValueNames() | Where-Object { $_ -like 'Kait.Run.Yummn.0.8.2*' } | Select-Object -First 1
        if (!$name) { throw 'No archived v0.8.2 Yummn save found.' }
        $value=$key.GetValue($name)
        $raw=if($value -is [byte[]]){[Text.Encoding]::UTF8.GetString($value).TrimEnd([char]0)}else{[string]$value}
    } finally { $key.Close() }
}
$replay=$raw | ConvertFrom-Json
if (!$replay.cardPoolVersion -or $null -eq $replay.steps) { throw 'Not a recognized replay file.' }
[pscustomobject]@{
    PoolVersion=$replay.cardPoolVersion
    Rules=$replay.rulesProfileId
    Seed=$replay.seed
    Actions=$replay.steps.Count
    Steps=$replay.steps
    RulesSnapshot=$replay.yummnRules
    ReadOnly=$true
}
