[CmdletBinding()]
param([string]$RepositoryRoot)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

. (Join-Path $PSScriptRoot 'common.ps1')
$root = if ($RepositoryRoot) {
    (Resolve-Path -LiteralPath $RepositoryRoot).Path
} else {
    Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
}

$selectorKeys = @('firearm-monogram-pistol', 'firearm-monogram-musket',
    'firearm-monogram-blunderbuss')
$itemKeys = @('early-pistol', 'musket', 'blunderbuss', 'wakizashi', 'katana',
    'nodachi', 'night-without-moon', 'heavens-measure',
    'world-tree-severer', 'elven-branched-spear')
$expectedKeys = @($selectorKeys) + @('rapid-reload') + @($itemKeys)
$retiredKeys = @('firearm-monogram-rifle', 'firearm-monogram-revolver')

function Read-BigEndianUInt32([byte[]]$Bytes, [int]$Offset) {
    return (([uint32]$Bytes[$Offset] -shl 24) -bor
        ([uint32]$Bytes[$Offset + 1] -shl 16) -bor
        ([uint32]$Bytes[$Offset + 2] -shl 8) -bor
        [uint32]$Bytes[$Offset + 3])
}

function Get-PngObservation([string]$Path) {
    $bytes = [IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -lt 33 -or $bytes[0] -ne 0x89 -or
        $bytes[1] -ne 0x50 -or $bytes[2] -ne 0x4e -or
        $bytes[3] -ne 0x47 -or
        [Text.Encoding]::ASCII.GetString($bytes, 12, 4) -cne 'IHDR') {
        throw "Asset is not a structurally valid PNG: $Path"
    }
    $width = [int](Read-BigEndianUInt32 $bytes 16)
    $height = [int](Read-BigEndianUInt32 $bytes 20)
    if ([int]$bytes[25] -ne 6) {
        throw "Asset is not an RGBA PNG (color type 6): $Path"
    }
    $bitmap = [Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $width -or $bitmap.Height -ne $height) {
            throw "PNG header/decoder dimension mismatch: $Path"
        }
        $minimumX = $width; $minimumY = $height
        $maximumX = -1; $maximumY = -1
        for ($y = 0; $y -lt $height; $y++) {
            for ($x = 0; $x -lt $width; $x++) {
                if ($bitmap.GetPixel($x, $y).A -le 3) { continue }
                if ($x -lt $minimumX) { $minimumX = $x }
                if ($x -gt $maximumX) { $maximumX = $x }
                if ($y -lt $minimumY) { $minimumY = $y }
                if ($y -gt $maximumY) { $maximumY = $y }
            }
        }
        if ($maximumX -lt $minimumX -or $maximumY -lt $minimumY) {
            throw "PNG has no visible alpha content: $Path"
        }
        $right = $width - 1; $bottom = $height - 1
        return [pscustomobject]@{
            Width = $width; Height = $height
            Bounds = @($minimumX, $minimumY,
                ($maximumX - $minimumX + 1),
                ($maximumY - $minimumY + 1))
            Corners = @($bitmap.GetPixel(0, 0).A,
                $bitmap.GetPixel($right, 0).A,
                $bitmap.GetPixel(0, $bottom).A,
                $bitmap.GetPixel($right, $bottom).A)
        }
    }
    finally { $bitmap.Dispose() }
}

$manifestPath = Join-Path $root `
    'assets-source\original-icons\icon-overhaul-assets.json'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Icon-overhaul manifest is missing: $manifestPath"
}
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$records = @($manifest.records)
$recordKeys = @($records | ForEach-Object { [string]$_.key })
if ($manifest.schemaVersion -ne 1 -or
    $manifest.assetSet -cne 'complete-icon-overhaul' -or
    $records.Count -ne $expectedKeys.Count -or
    ($recordKeys | Select-Object -Unique).Count -ne $expectedKeys.Count -or
    @($expectedKeys | Where-Object { $_ -notin $recordKeys }).Count -ne 0 -or
    @($recordKeys | Where-Object { $_ -notin $expectedKeys }).Count -ne 0) {
    throw 'Icon-overhaul manifest does not describe the exact 14-asset set.'
}

foreach ($retired in $retiredKeys) {
    $path = Join-Path $root "assets\game\icons\$retired.png"
    if (Test-Path -LiteralPath $path) {
        throw "Retired player-facing selector icon returned: $path"
    }
}

foreach ($record in $records) {
    $key = [string]$record.key
    $sourcePath = Join-Path $root ([string]$record.sourcePath)
    $finalPath = Join-Path $root ([string]$record.finalPath)
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf) -or
        -not (Test-Path -LiteralPath $finalPath -PathType Leaf)) {
        throw "Icon source/final pair is missing: $key"
    }
    if (([string]$record.finalPath).Replace('\', '/') -cne
        "assets/game/icons/$key.png") {
        throw "Icon uses an unexpected runtime path: $key"
    }
    $sourceHash = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash
    $finalHash = (Get-FileHash -LiteralPath $finalPath -Algorithm SHA256).Hash
    if ($sourceHash -cne ([string]$record.sourceSha256).ToUpperInvariant() -or
        $finalHash -cne ([string]$record.finalSha256).ToUpperInvariant()) {
        throw "Icon hash no longer matches the checked-in manifest: $key"
    }
    $sourceObservation = Get-PngObservation $sourcePath
    $observation = Get-PngObservation $finalPath
    $expectedSize = if ($key -in $itemKeys) { 128 } else { 64 }
    if ($observation.Width -ne $expectedSize -or
        $observation.Height -ne $expectedSize -or
        $sourceObservation.Width -ne [int]$record.sourceDimensions[0] -or
        $sourceObservation.Height -ne [int]$record.sourceDimensions[1] -or
        ($observation.Bounds -join ',') -cne
            (@($record.finalAlphaBounds) -join ',') -or
        ($observation.Corners -join ',') -cne
            (@($record.cornerAlpha) -join ',')) {
        throw "Icon dimensions or alpha metadata changed: $key"
    }

    if ($key -in $selectorKeys) {
        if (($observation.Bounds -join ',') -cne '0,0,64,64' -or
            @($observation.Corners | Where-Object { $_ -lt 200 }).Count -ne 0) {
            throw "Selector is not an opaque full-square icon: $key"
        }
    } elseif ($key -eq 'rapid-reload') {
        if (@($observation.Corners | Where-Object { $_ -ne 0 }).Count -ne 0) {
            throw 'Rapid Reload lost its transparent canvas corners.'
        }
    } else {
        $bounds = $observation.Bounds
        $margins = @($bounds[0], $bounds[1],
            (128 - ($bounds[0] + $bounds[2])),
            (128 - ($bounds[1] + $bounds[3])))
        if (@($observation.Corners | Where-Object { $_ -ne 0 }).Count -ne 0 -or
            @($margins | Where-Object { $_ -lt 4 }).Count -ne 0 -or
            [Math]::Max($bounds[2], $bounds[3]) -lt 116 -or
            [Math]::Max($bounds[2], $bounds[3]) -gt 120) {
            throw "Item icon is opaque, clipped, or fails the diagonal-fill contract: $key"
        }
    }
}

$specPath = Join-Path $root 'assets-source\original-icons\firearm-feats\icon-spec.json'
$spec = Get-Content -LiteralPath $specPath -Raw | ConvertFrom-Json
$specKeys = @($spec.monograms | ForEach-Object { [string]$_.key })
if ($spec.schemaVersion -ne 4 -or $specKeys.Count -ne 3 -or
    ($specKeys -join ',') -cne ($selectorKeys -join ',') -or
    [string]$spec.palette.rapidReloadRed -cne '#A6533F') {
    throw 'Firearm feat icon specification is not exact-three schema 4.'
}

$iconSourcePath = Join-Path $root 'src\KingmakerGunslinger\Blueprints\ProjectAssetIcons.cs'
$iconSource = Get-Content -LiteralPath $iconSourcePath -Raw
foreach ($retired in $retiredKeys) {
    if ($iconSource.Contains('"' + $retired + '"')) {
        throw "Runtime icon publication still references retired selector: $retired"
    }
}
foreach ($token in @(
    'items.SetIcon(firearms.Pistol.Item, Require("early-pistol"))',
    'items.SetIcon(firearms.Musket.Item, Require("musket"))',
    'items.SetIcon(firearms.Blunderbuss.Item, Require("blunderbuss"))',
    'foreach (ElvenBranchedSpearBlueprintEntry entry in',
    'foreach (NamedSpearBlueprintEntry entry in',
    'foreach (EasternWeaponFamilyBlueprintSet family in weapons.Families)',
    'foreach (EasternWeaponNamedBlueprintEntry entry in')) {
    if (-not $iconSource.Contains($token)) {
        throw "Centralized blueprint icon mapping is incomplete: $token"
    }
}

Write-Host (('Icon-overhaul asset validation passed: {0} records; {1} selectors; ' +
    '{2} item icons; retired selectors absent.') -f $records.Count,
    $selectorKeys.Count, $itemKeys.Count)
