[CmdletBinding()]
param(
    [switch]$VerifyOnly
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$sourceRoot = Join-Path $root 'assets-source\original-icons\expanded-summoning'
$promptPath = Join-Path $sourceRoot 'prompts\icon-prompts.json'
$outputRoot = Join-Path $root 'assets\game\icons\expanded-summoning'
$provenancePath = Join-Path $sourceRoot 'icon-manifest.json'
$runtimePath = Join-Path $outputRoot 'icon-manifest.json'
$blueprintPath = Join-Path $root 'blueprints\blueprints.json'

Add-Type -AssemblyName System.Drawing

function Get-Sha256([string]$Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Convert-KeyToToken([string]$Key) {
    $result = ''
    foreach ($part in $Key.Split('-')) {
        if ($part.Length -gt 0) {
            $result += $part.Substring(0, 1).ToUpperInvariant() + $part.Substring(1)
        }
    }
    return $result
}

function Get-Consumers([string]$Key, [object[]]$Entries) {
    $token = Convert-KeyToToken $Key
    $matches = @($Entries | Where-Object {
        $_.status -eq 'active' -and ($_.symbol -like "KMG.Summoning.Unit.$token" -or
            $_.symbol -like "KMG.Summoning.Ability.*.$token.*" -or
            $_.symbol -like "KMG.Summoning.NativeOption.*.$token.*")
    } | ForEach-Object { $_.symbol })
    return @($matches | Sort-Object -Unique)
}

function Assert-MeaningfulImage([string]$Path, [int]$ExpectedWidth, [int]$ExpectedHeight) {
    $image = [System.Drawing.Bitmap]::FromFile($Path)
    try {
        if ($image.Width -ne $ExpectedWidth -or $image.Height -ne $ExpectedHeight) {
            throw "Unexpected dimensions for ${Path}: $($image.Width)x$($image.Height)."
        }
        $colors = New-Object 'System.Collections.Generic.HashSet[int]'
        $visible = 0
        $white = 0
        $lumaSum = 0.0
        $lumaSquareSum = 0.0
        for ($y = 0; $y -lt $image.Height; $y += 2) {
            for ($x = 0; $x -lt $image.Width; $x += 2) {
                $pixel = $image.GetPixel($x, $y)
                [void]$colors.Add($pixel.ToArgb())
                if ($pixel.A -gt 8) { $visible++ }
                if ($pixel.A -gt 248 -and $pixel.R -gt 248 -and $pixel.G -gt 248 -and $pixel.B -gt 248) { $white++ }
                $luma = (0.2126 * $pixel.R) + (0.7152 * $pixel.G) + (0.0722 * $pixel.B)
                $lumaSum += $luma
                $lumaSquareSum += $luma * $luma
            }
        }
        $samples = [Math]::Ceiling($image.Width / 2.0) * [Math]::Ceiling($image.Height / 2.0)
        $variance = ($lumaSquareSum / $samples) - [Math]::Pow($lumaSum / $samples, 2)
        if ($visible -lt ($samples * 0.25) -or $white -gt ($samples * 0.95) -or
            $colors.Count -lt 128 -or $variance -lt 80.0) {
            throw "Blank, uniform, transparent, or all-white icon rejected: $Path."
        }
    }
    finally { $image.Dispose() }
}

$prompts = Get-Content -LiteralPath $promptPath -Raw | ConvertFrom-Json
$icons = @($prompts.icons)
if ($prompts.schemaVersion -ne 1 -or $icons.Count -ne 77 -or
    @($icons.key | Sort-Object -Unique).Count -ne 77) {
    throw 'Expanded Summoning prompt catalog must contain exactly 77 unique keys.'
}
$blueprints = (Get-Content -LiteralPath $blueprintPath -Raw | ConvertFrom-Json).entries
$catalogKeys = @('redcap','axiomite','soul-eater','bogeyman','movanic-deva','frost-giant','thanadaemon')
$preservedKeys = @('mite','manticore','nereid','hamadryad')

if (-not $VerifyOnly -and -not (Test-Path -LiteralPath $outputRoot)) {
    New-Item -ItemType Directory -Path $outputRoot | Out-Null
}

$provenanceRows = @()
$runtimeRows = @()
foreach ($icon in $icons) {
    $key = [string]$icon.key
    $sourceRelative = "assets-source/original-icons/expanded-summoning/sources/$key.png"
    $outputRelative = "assets/game/icons/expanded-summoning/$key.png"
    $source = Join-Path $root ($sourceRelative.Replace('/', '\'))
    $output = Join-Path $root ($outputRelative.Replace('/', '\'))
    if (-not (Test-Path -LiteralPath $source)) { throw "Missing source icon: $key." }
    Assert-MeaningfulImage $source 1254 1254
    if (-not $VerifyOnly) {
        $inputImage = [System.Drawing.Image]::FromFile($source)
        try {
            $bitmap = New-Object System.Drawing.Bitmap 128, 128, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            try {
                $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
                try {
                    $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
                    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
                    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                    $graphics.DrawImage($inputImage, 0, 0, 128, 128)
                }
                finally { $graphics.Dispose() }
                $bitmap.Save($output, [System.Drawing.Imaging.ImageFormat]::Png)
            }
            finally { $bitmap.Dispose() }
        }
        finally { $inputImage.Dispose() }
    }
    if (-not (Test-Path -LiteralPath $output)) { throw "Missing production icon: $key." }
    Assert-MeaningfulImage $output 128 128
    $scope = if ($catalogKeys -contains $key) { 'split-native' } elseif ($preservedKeys -contains $key) { 'preserved-native' } else { 'kmg-catalog' }
    $consumers = @(Get-Consumers $key $blueprints)
    $sourceHash = Get-Sha256 $source
    $outputHash = Get-Sha256 $output
    $provenanceRows += [ordered]@{
        key = $key; displayName = [string]$icon.displayName; sourceFile = $sourceRelative
        productionFile = $outputRelative; sourceSha256 = $sourceHash
        outputSha256 = $outputHash; width = 128; height = 128; format = 'RGBA PNG'
        scope = $scope; blueprintSymbols = $consumers
    }
    $runtimeRows += [ordered]@{
        key = $key; file = "$key.png"; sha256 = $outputHash
        width = 128; height = 128; format = 'RGBA PNG'; scope = $scope
    }
}

$duplicateHashes = @($runtimeRows | Group-Object { $_['sha256'] } | Where-Object Count -gt 1)
if ($duplicateHashes.Count -ne 0) { throw 'Unrelated icon concepts have duplicate production hashes.' }
$actualOutputs = @(Get-ChildItem -LiteralPath $outputRoot -File -Filter '*.png' | ForEach-Object BaseName | Sort-Object)
$expectedOutputs = @($icons.key | Sort-Object)
if (@(Compare-Object $expectedOutputs $actualOutputs).Count -ne 0) {
    throw 'Production icon directory contains a missing or unmanifested PNG.'
}

$provenance = [ordered]@{
    schemaVersion = 1; provenance = 'Project-owned original AI-assisted artwork; no source images or third-party pixels.'
    generator = 'tools/New-ExpandedSummoningIcons.ps1'; count = 77; icons = $provenanceRows
}
$runtime = [ordered]@{
    schemaVersion = 1; count = 77; icons = $runtimeRows
}
if (-not $VerifyOnly) {
    $provenance | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $provenancePath -Encoding UTF8
    $runtime | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $runtimePath -Encoding UTF8
} else {
    if (-not (Test-Path $provenancePath) -or -not (Test-Path $runtimePath)) { throw 'Icon manifests are missing.' }
    $checkedRuntime = Get-Content -LiteralPath $runtimePath -Raw | ConvertFrom-Json
    foreach ($row in $checkedRuntime.icons) {
        $match = @($runtimeRows | Where-Object key -eq $row.key)
        if ($match.Count -ne 1 -or $match[0].sha256 -ne $row.sha256) { throw "Runtime manifest is stale for $($row.key)." }
    }
}

Write-Host "Expanded Summoning icons PASS: 77 distinct sources and 77 distinct 128x128 RGBA outputs."
