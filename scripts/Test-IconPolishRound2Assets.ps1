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

function Read-BigEndianUInt32([byte[]]$Bytes, [int]$Offset) {
    return (([uint32]$Bytes[$Offset] -shl 24) -bor
        ([uint32]$Bytes[$Offset + 1] -shl 16) -bor
        ([uint32]$Bytes[$Offset + 2] -shl 8) -bor
        [uint32]$Bytes[$Offset + 3])
}

function Get-PngObservation([string]$Path) {
    $bytes = [IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -lt 33 -or
        [Text.Encoding]::ASCII.GetString($bytes, 12, 4) -cne 'IHDR') {
        throw ('Asset is not a structurally valid PNG: {0}' -f $Path)
    }
    $width = [int](Read-BigEndianUInt32 $bytes 16)
    $height = [int](Read-BigEndianUInt32 $bytes 20)
    if ([int]$bytes[25] -ne 6) {
        throw ('Asset is not an RGBA PNG: {0}' -f $Path)
    }
    $bitmap = [Drawing.Bitmap]::new($Path)
    try {
        $minimumX = $width
        $minimumY = $height
        $maximumX = -1
        $maximumY = -1
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
            throw ('PNG has no visible pixels: {0}' -f $Path)
        }
        return [pscustomobject]@{
            Width = $width
            Height = $height
            Bounds = @($minimumX, $minimumY,
                ($maximumX - $minimumX + 1),
                ($maximumY - $minimumY + 1))
            Corners = @(
                $bitmap.GetPixel(0, 0).A,
                $bitmap.GetPixel($width - 1, 0).A,
                $bitmap.GetPixel(0, $height - 1).A,
                $bitmap.GetPixel($width - 1, $height - 1).A)
            Bitmap = $bitmap
        }
    }
    catch {
        $bitmap.Dispose()
        throw
    }
}

function Test-GoldPixel([Drawing.Color]$Pixel) {
    return $Pixel.A -ge 200 -and $Pixel.R -ge 145 -and
        $Pixel.G -ge 95 -and $Pixel.B -ge 55 -and
        $Pixel.R -gt $Pixel.G
}

function Get-GoldBorderCount(
    [Drawing.Bitmap]$Bitmap, [int]$Inset) {
    $minimum = $Inset
    $maximumX = $Bitmap.Width - 1 - $Inset
    $maximumY = $Bitmap.Height - 1 - $Inset
    $count = 0
    for ($x = $minimum; $x -le $maximumX; $x++) {
        if (Test-GoldPixel $Bitmap.GetPixel($x, $minimum)) { $count++ }
        if (Test-GoldPixel $Bitmap.GetPixel($x, $maximumY)) { $count++ }
    }
    for ($y = $minimum + 1; $y -lt $maximumY; $y++) {
        if (Test-GoldPixel $Bitmap.GetPixel($minimum, $y)) { $count++ }
        if (Test-GoldPixel $Bitmap.GetPixel($maximumX, $y)) { $count++ }
    }
    return $count
}

function Assert-Sha256([string]$RelativePath, [string]$Expected) {
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw ('Protected file is missing: {0}' -f $RelativePath)
    }
    $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    if ($actual -cne $Expected.ToUpperInvariant()) {
        throw ('Protected file hash changed: {0}; expected={1}; actual={2}' -f
            $RelativePath, $Expected, $actual)
    }
}

$protected = [ordered]@{
    'assets/game/icons/rapid-reload.png' = 'efab95075ad8af61fe10425090015a75432b74113fbc34ebc185969e1e82b321'
    'assets/game/icons/early-pistol.png' = '1cd06b9aeea63b4842951568812791e50e8fd9472884078449dd84c1c9bf0719'
    'assets/game/icons/musket.png' = '638077254f298a626f3fa8a8c098bb1e9f2c4f3678df90a1e28920f4a9ffd086'
    'assets/game/icons/blunderbuss.png' = 'e5923f9b5820eef3ca3d41e5af559b09ef8ea21b0052dc04909fd72f73ac929f'
    'assets/game/icons/rifle.png' = '0fa35d1d917006b6ab36d2e0a449a142cf24d3e9c3cc02634d88ab17e7ac1f66'
    'assets/game/icons/revolver.png' = 'ff4aab9347f7c8515509c3957f2b4db42742711e17b0e67811720b954509a5b2'
    'assets/game/icons/wakizashi.png' = 'cb32f5afdc9522bebf45d863b7a2f153c8ea908292c96cb30601f739a27d9dc1'
    'assets/game/icons/katana.png' = '139ff7292bb4d8270b92083e90b4c46be50b54a9e0ac9382eb9397acd6f09a90'
    'assets/game/icons/nodachi.png' = '1e3f8d208e4d4733a32ee71968b051182f818ffe407dfd76a7b8a731b8bfa8da'
    'assets/game/icons/night-without-moon.png' = 'a6681e97cc07e3d4a3c894e2c1b479f647ef60cf24f40eaa945d6fdc96824f0e'
    'assets/game/icons/heavens-measure.png' = '428c6c8099b27926cbe962fe5ff40e7a24db75826eee060b654345a9ba0f63f4'
    'assets/game/icons/world-tree-severer.png' = '730072a080d7b4c405d554e2f34e498cde973d36627976b964d4b69c81c20e32'
    'assets/game/icons/elven-branched-spear.png' = '5a8d3d10f95af61c6afd324c8791b37bb675d4a74d3dcd4eca7cdb4d0464109a'
    'assets-source/original-icons/firearm-feats/sources/rapid-reload-source.png' = 'a115b060976a73e60eb178f9209ac9f176fdec13dae25076715f530d153d3e98'
    'assets-source/original-icons/firearm-items/early-pistol-source.png' = 'c6a76485178cdb1a7b37291b8169e034c78df4b0d551da70b18d428d30abde6b'
    'assets-source/original-icons/firearm-items/musket-source.png' = '624582c0f7a63a097f85f289edbd9aa4933264d70f4f91148b2222878f4a94e6'
    'assets-source/original-icons/firearm-items/blunderbuss-source.png' = '773cbf0c27329c520eacedc7f6e85645493ee7a85e48436b6cfa0e1b190582e7'
    'assets-source/original-models/eastern-weapons/wakizashi-icon-source.png' = '6a2c02473bc1f87e000d83f327244b5f450c7260dbbc3871025f86a7220f554c'
    'assets-source/original-models/eastern-weapons/katana-icon-source.png' = '1b97a26b4c7a3dfbd25df9d9e5f64c5b3e2ff7c9743e49df6906f58778ceec2f'
    'assets-source/original-models/eastern-weapons/nodachi-icon-source.png' = '3c715265c312def544593bafa5f76bb48f778a68a013b747cd2e3b903abd2547'
    'assets-source/original-models/eastern-weapons/night-without-moon-icon-source.png' = '122c539c6ce002ff029a6b2e05bbb2bb17cb7a3d4d190d5d750184365fdd977a'
    'assets-source/original-models/eastern-weapons/heavens-measure-icon-source.png' = 'c5a285ef03454eb5a64dd2dbdd2894951fd0d7702702b8238bcbd8f7b837a2bf'
    'assets-source/original-models/eastern-weapons/world-tree-severer-icon-source.png' = 'cff3b5db26c709d15d47cd8af6cfe9da62c0c7c9ae7a681ce028a0efe85e3e33'
    'assets-source/original-models/elven-branched-spear/elven-branched-spear-icon.png' = 'ece96570240e97ec009914f42a569415b622282689276b8beeee258e95846960'
    'src/KingmakerGunslinger/Firearms/FirearmKind.cs' = 'e3a94f162f9b62cdbb4b1b5274d1a6d4aa43d4477d1099a1d5f709c45aaee911'
    'src/KingmakerGunslinger/Blueprints/FirearmFeatBlueprints.cs' = 'f08609beb8f8ffca8eefb0f02035c347298773753268009619ef1f24f52919b1'
    'src/KingmakerGunslinger/Feats/NativeFirearmFeatIntegration.cs' = 'bc22787d2838a418dd22b656b87554a5e3be8d25c9f9b420c3d2a07e3410bc75'
    'src/KingmakerGunslinger/Blueprints/GunTrainingBlueprints.cs' = '8603c87a4fc9fecd86ed0aa2da52bdcd9c5969d898639a6688139faeb93c0564'
    'src/KingmakerGunslinger/Firearms/ProductionFirearmCatalog.cs' = '75a8352c85c2e4fe5369ea02c414df9adc8a04f33c075f9176bd2f1138ad18dd'
    'src/KingmakerGunslinger/CraftMagicItemsCompatibility/CraftMagicItemsCompatibilityPolicy.cs' = 'ae8f6c1739268bea982334153c6eb37eacf6625a798e7e6bc0d2e3a94bc06ce3'
}
foreach ($entry in $protected.GetEnumerator()) {
    Assert-Sha256 $entry.Key $entry.Value
}

$manifestPath = Join-Path $root 'assets-source/original-icons/icon-overhaul-assets.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$beforeBounds = @{
    'firearm-monogram-pistol' = @(10, 10, 44, 47)
    'firearm-monogram-musket' = @(7, 12, 53, 45)
    'firearm-monogram-blunderbuss' = @(10, 10, 44, 47)
}
foreach ($key in $beforeBounds.Keys) {
    $record = @($manifest.records | Where-Object { $_.key -ceq $key })
    if ($record.Count -ne 1) {
        throw ('Round 2 selector manifest record mismatch: {0}' -f $key)
    }
    $glyph = @($record[0].glyphAlphaBounds)
    if ($glyph.Count -ne 4 -or $glyph[2] -lt 29 -or $glyph[2] -gt 35 -or
        $glyph[3] -lt 27 -or $glyph[3] -gt 33) {
        throw ('Round 2 glyph bounds are not native-scale: {0}' -f $key)
    }
    $centerX = $glyph[0] + ($glyph[2] - 1) / 2.0
    $centerY = $glyph[1] + ($glyph[3] - 1) / 2.0
    if ([Math]::Abs($centerX - 32) -gt 1.5 -or
        [Math]::Abs($centerY - 32) -gt 1.5) {
        throw ('Round 2 glyph is not optically centered: {0}' -f $key)
    }
    $before = $beforeBounds[$key]
    $beforeArea = $before[2] * $before[3]
    $afterArea = $glyph[2] * $glyph[3]
    if ($afterArea / [double]$beforeArea -gt 0.60) {
        throw ('Round 2 glyph was not substantially reduced: {0}' -f $key)
    }

    $finalPath = Join-Path $root $record[0].finalPath
    $observation = Get-PngObservation $finalPath
    try {
        if ($observation.Width -ne 64 -or $observation.Height -ne 64 -or
            ($observation.Bounds -join ',') -cne '0,0,64,64' -or
            @($observation.Corners | Where-Object { $_ -lt 200 }).Count -ne 0) {
            throw ('Round 2 selector is not an opaque 64px full-bleed tile: {0}' -f
                $key)
        }
        if ((Get-GoldBorderCount $observation.Bitmap 1) -gt 12 -or
            (Get-GoldBorderCount $observation.Bitmap 4) -gt 12) {
            throw ('Round 2 selector contains a baked gold frame: {0}' -f $key)
        }
    }
    finally {
        $observation.Bitmap.Dispose()
    }
}

$spec = Get-Content -LiteralPath (Join-Path $root 'assets-source/original-icons/firearm-feats/icon-spec.json') -Raw | ConvertFrom-Json
if ($spec.schemaVersion -ne 4 -or $spec.bakedFrame -ne $false -or
    [string]$spec.selectorStyle -notmatch 'no baked border' -or
    [double]$spec.monogramStyle.scale.P -ne 0.66 -or
    [double]$spec.monogramStyle.scale.M -ne 0.62 -or
    [double]$spec.monogramStyle.scale.B -ne 0.66) {
    throw 'Round 2 firearm icon specification is incomplete.'
}
$generatorText = Get-Content -LiteralPath (Join-Path $root 'tools/icon-art/New-IconOverhaulAssets.ps1') -Raw
foreach ($rejected in @(
    '$graphics.DrawRectangle($outer',
    '$graphics.DrawRectangle($inner',
    '$ornament = [Drawing.Pen]')) {
    if ($generatorText.Contains($rejected)) {
        throw ('Baked selector-frame construction returned: {0}' -f $rejected)
    }
}
foreach ($retired in @(
    'firearm-monogram-rifle.png',
    'firearm-monogram-revolver.png')) {
    if (Test-Path -LiteralPath (Join-Path $root ('assets/game/icons/' + $retired))) {
        throw ('Retired selector icon returned: {0}' -f $retired)
    }
}

$cordManifestPath = Join-Path $root 'assets-source/original-icons/cord-of-stubborn-resolve/cord-of-stubborn-resolve-assets.json'
$cordManifest = Get-Content -LiteralPath $cordManifestPath -Raw | ConvertFrom-Json
$cordSourcePath = Join-Path $root $cordManifest.sourcePath
$cordRuntimePath = Join-Path $root $cordManifest.runtimePath
$cordObservation = Get-PngObservation $cordRuntimePath
try {
    $cordBounds = $cordObservation.Bounds
    $cordAspect = $cordBounds[2] / [double]$cordBounds[3]
    if ($cordManifest.schemaVersion -ne 2 -or
        [string]$cordManifest.sourcePath -cne
            'assets-source/original-icons/cord-of-stubborn-resolve/cord-of-stubborn-resolve-oblique-source.png' -or
        $cordObservation.Width -ne 128 -or $cordObservation.Height -ne 128 -or
        $cordBounds[2] -lt 112 -or $cordBounds[2] -gt 120 -or
        $cordBounds[3] -lt 58 -or $cordBounds[3] -gt 70 -or
        $cordAspect -lt 1.70 -or $cordBounds[2] -le $cordBounds[3] -or
        @($cordObservation.Corners | Where-Object { $_ -ne 0 }).Count -ne 0) {
        throw 'Cord runtime icon is not a transparent, belt-like 128px asset.'
    }
    Assert-Sha256 $cordManifest.sourcePath $cordManifest.sourceSha256
    Assert-Sha256 $cordManifest.runtimePath $cordManifest.runtimeSha256
}
finally {
    $cordObservation.Bitmap.Dispose()
}
$cordGenerator = Get-Content -LiteralPath (Join-Path $root 'tools/New-CordOfStubbornResolveIcon.ps1') -Raw
if ($cordGenerator.Contains('cord-of-stubborn-resolve-chroma-source.png') -or
    -not $cordGenerator.Contains('cord-of-stubborn-resolve-oblique-source.png')) {
    throw 'Cord generator still references stale circular source art.'
}

Write-Host (('Icon polish Round 2 validation passed: {0} protected files; ' +
    '3 no-frame selectors; belt-like Cord aspect {1:N4}.') -f
    $protected.Count, $cordManifest.runtimeAspect)
