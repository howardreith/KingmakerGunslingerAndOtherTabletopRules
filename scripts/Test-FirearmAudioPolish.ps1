[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$temp = Join-Path ([IO.Path]::GetTempPath()) ('kmg-audio-polish-' + [guid]::NewGuid().ToString('N'))
try {
    & (Join-Path $root 'scripts\audio\Prepare-FirearmWwiseSources.ps1') -Destination $temp
    $map = Get-Content -LiteralPath (Join-Path $root 'assets-source\wwise\KingmakerGunslingerFirearms\source-map.json') -Raw | ConvertFrom-Json
    $provenance = Get-Content -LiteralPath (Join-Path $root 'assets-source\third-party\audio\sse-library-guns\audio-manifest.json') -Raw | ConvertFrom-Json
    foreach($event in $map.events) {
        $path = Join-Path $temp $event.source
        $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        $derived = $event.PSObject.Properties['derivedSha256']
        if($derived) {
            if($hash -cne [string]$derived.Value) { throw "Derived WAV hash mismatch: $($event.source)" }
            if((Get-Item -LiteralPath $path).Length -ne 174764) { throw 'Derived Blunderbuss WAV length changed.' }
        } else {
            $record = @($provenance.records | Where-Object { $_.processed -ceq $event.source })
            if($record.Count -ne 1 -or $hash -cne [string]$record[0].processedSha256) {
                throw "Unmodified Wwise source changed: $($event.source)"
            }
        }
    }
    $presentation = Get-Content -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Blueprints\FirearmWeaponPresentation.cs') -Raw
    foreach($token in @('Materialize(visual, "m_WeaponModel", source.Model)',
        'Materialize(visual, "m_InventoryTakeSound", source.InventoryTakeSound)',
        'Set(visual, "m_WhooshSound", string.Empty)',
        'Set(visual, "<Prototype>k__BackingField", null)')) {
        if(-not $presentation.Contains($token)) { throw "Missing firearm presentation token: $token" }
    }
    $scatter = Get-Content -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Scatter\ScatterShotAbilityLogic.cs') -Raw
    if(-not $scatter.Contains('result.Projectiles = new[] { firearmProjectile }') -or
       -not $scatter.Contains('result.Type = nativeCone.Type') -or
       -not $scatter.Contains('result.Length = nativeCone.Length')) {
        throw 'Scatter does not preserve cone geometry with the firearm projectile.'
    }
    Write-Output 'PASS: deterministic Blunderbuss trim and firearm-scoped presentation/audio polish contracts validated.'
}
finally {
    if(Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Recurse -Force }
}
