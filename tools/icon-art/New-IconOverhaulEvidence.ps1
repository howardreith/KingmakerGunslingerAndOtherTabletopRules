[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RuntimeEvidenceDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$runtimeRoot = (Resolve-Path $RuntimeEvidenceDirectory).Path
$reportRoot = Join-Path $repositoryRoot 'docs\reports'
$afterRoot = Join-Path $reportRoot 'icon-overhaul\runtime-after'
$referenceContactSheet = Join-Path $repositoryRoot 'docs\reference\icon-overhaul\references\CONTACT_SHEET.png'
$contactSheetPath = Join-Path $reportRoot 'icon-overhaul-before-after-contact-sheet.png'
$runtimeResultPath = Join-Path $runtimeRoot 'runtime-result.json'
$runtimeIndexPath = Join-Path $runtimeRoot 'icon-overhaul-visual-index.json'
$orchestrationPath = Join-Path $runtimeRoot 'orchestration.json'

foreach ($requiredPath in @($referenceContactSheet, $runtimeResultPath, $runtimeIndexPath, $orchestrationPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required evidence input is missing: $requiredPath"
    }
}

$runtimeResult = Get-Content -LiteralPath $runtimeResultPath -Raw | ConvertFrom-Json
$runtimeIndex = Get-Content -LiteralPath $runtimeIndexPath -Raw | ConvertFrom-Json
$orchestration = Get-Content -LiteralPath $orchestrationPath -Raw | ConvertFrom-Json
if ($runtimeResult.scenario -ne 'icon-overhaul-visual-evidence' -or $runtimeResult.status -ne 'PASS') {
    throw "Expected a passing icon-overhaul-visual-evidence result; observed scenario=$($runtimeResult.scenario), status=$($runtimeResult.status)."
}
if ($runtimeIndex.renderWidth -ne 1920 -or $runtimeIndex.renderHeight -ne 1200) {
    throw "Expected the runtime visual index to describe exact 1920x1200 renders."
}

$deploymentManifestPath = [string]$orchestration.deploymentManifestPath
if (-not (Test-Path -LiteralPath $deploymentManifestPath -PathType Leaf)) {
    throw "The deployment manifest recorded by orchestration is missing: $deploymentManifestPath"
}
$deployment = Get-Content -LiteralPath $deploymentManifestPath -Raw | ConvertFrom-Json

$screenshots = @(
    [pscustomobject]@{ FileName = 'after-01-rapid-reload-feat-list.png'; Label = '1. Rapid Reload beside native feats' },
    [pscustomobject]@{ FileName = 'after-02-rapid-reload-supported-choices.png'; Label = '2. Rapid Reload exact supported choices' },
    [pscustomobject]@{ FileName = 'after-03-weapon-focus-firearm-choices.png'; Label = '3. Weapon Focus exact firearm parameters' },
    [pscustomobject]@{ FileName = 'after-04-supported-firearm-items.png'; Label = '4. Supported firearm item icons' },
    [pscustomobject]@{ FileName = 'after-05-eastern-and-spear-items.png'; Label = '5. Eastern and spear item icons' }
)

function Get-DecodedPixelSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)

    $bitmap = [System.Drawing.Bitmap]::FromFile($Path)
    try {
        $rectangle = New-Object System.Drawing.Rectangle 0, 0, $bitmap.Width, $bitmap.Height
        $data = $bitmap.LockBits(
            $rectangle,
            [System.Drawing.Imaging.ImageLockMode]::ReadOnly,
            [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $length = [Math]::Abs($data.Stride) * $data.Height
            $bytes = New-Object 'byte[]' $length
            [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $length)
            $sha256 = [System.Security.Cryptography.SHA256]::Create()
            try {
                return ([BitConverter]::ToString($sha256.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
            }
            finally {
                $sha256.Dispose()
            }
        }
        finally {
            $bitmap.UnlockBits($data)
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Write-Utf8WithoutBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )

    $encoding = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, $Text, $encoding)
}

New-Item -ItemType Directory -Force -Path $afterRoot | Out-Null
$curatedRecords = @()
foreach ($screenshot in $screenshots) {
    $sourcePath = Join-Path $runtimeRoot $screenshot.FileName
    $destinationPath = Join-Path $afterRoot $screenshot.FileName
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        throw "Runtime screenshot is missing: $sourcePath"
    }

    $sourceImage = [System.Drawing.Image]::FromFile($sourcePath)
    try {
        if ($sourceImage.Width -ne 1920 -or $sourceImage.Height -ne 1200) {
            throw "$($screenshot.FileName) is $($sourceImage.Width)x$($sourceImage.Height), not 1920x1200."
        }

        $temporaryPath = "$destinationPath.tmp"
        $sourceImage.Save($temporaryPath, [System.Drawing.Imaging.ImageFormat]::Png)
        Move-Item -LiteralPath $temporaryPath -Destination $destinationPath -Force
    }
    finally {
        $sourceImage.Dispose()
    }

    $indexedRecord = @($runtimeIndex.screenshots | Where-Object { $_.fileName -eq $screenshot.FileName })
    if ($indexedRecord.Count -ne 1) {
        throw "Expected exactly one runtime-index entry for $($screenshot.FileName)."
    }

    $sourceSha256 = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($sourceSha256 -ne ([string]$indexedRecord[0].sha256).ToLowerInvariant()) {
        throw "Runtime-index SHA-256 mismatch for $($screenshot.FileName)."
    }

    $sourcePixelSha256 = Get-DecodedPixelSha256 -Path $sourcePath
    $curatedPixelSha256 = Get-DecodedPixelSha256 -Path $destinationPath
    if ($sourcePixelSha256 -ne $curatedPixelSha256) {
        throw "Lossless curation changed decoded pixels for $($screenshot.FileName)."
    }

    $curatedRecords += [ordered]@{
        file = "icon-overhaul/runtime-after/$($screenshot.FileName)"
        label = $screenshot.Label
        width = 1920
        height = 1200
        sourceSha256 = $sourceSha256
        curatedSha256 = (Get-FileHash -LiteralPath $destinationPath -Algorithm SHA256).Hash.ToLowerInvariant()
        decodedPixelSha256 = $curatedPixelSha256
    }
}

$manifest = [ordered]@{
    schemaVersion = 1
    title = 'Kingmaker Gunslinger icon-overhaul curated runtime visuals'
    sourceRunId = [string]$runtimeResult.runId
    sourceScenario = [string]$runtimeResult.scenario
    sourceStatus = [string]$runtimeResult.status
    evidenceRole = 'Supporting perceptual evidence from in-game Unity rendering of live loaded blueprint sprites; mechanical claims use structured runtime assertions.'
    loadedModVersion = [string]$runtimeResult.loadedModVersion
    gitCommitAtBuild = [string]$runtimeResult.gitCommit
    packageSha256 = [string]$deployment.packageSha256
    dllSha256 = [string]$deployment.dllSha256
    dllMvid = [string]$deployment.dllMvid
    easternItemCount = [int]$runtimeIndex.easternItemCount
    spearItemCount = [int]$runtimeIndex.spearItemCount
    screenshots = $curatedRecords
}
$manifestPath = Join-Path $afterRoot 'manifest.json'
Write-Utf8WithoutBom -Path $manifestPath -Text (($manifest | ConvertTo-Json -Depth 8) + [Environment]::NewLine)

$canvas = New-Object System.Drawing.Bitmap 3300, 1750, ([System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$graphics = [System.Drawing.Graphics]::FromImage($canvas)
$background = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(23, 20, 17))
$headingBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(221, 199, 148))
$labelBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(231, 220, 197))
$noteBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(171, 158, 134))
$borderPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(151, 124, 68)), 2
$headingFont = New-Object System.Drawing.Font 'Segoe UI', 22, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
$labelFont = New-Object System.Drawing.Font 'Segoe UI', 17, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$noteFont = New-Object System.Drawing.Font 'Segoe UI', 14, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
try {
    $graphics.FillRectangle($background, 0, 0, $canvas.Width, $canvas.Height)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    $graphics.DrawString('BEFORE - SUPPLIED FULL-RESOLUTION REFERENCES', $headingFont, $headingBrush, 20, 14)
    $beforeImage = [System.Drawing.Image]::FromFile($referenceContactSheet)
    try {
        $graphics.DrawImage($beforeImage, 20, 60, 1580, 1669)
        $graphics.DrawRectangle($borderPen, 20, 60, 1580, 1669)
    }
    finally {
        $beforeImage.Dispose()
    }

    $graphics.DrawString('AFTER - GUARDED IN-GAME LIVE-SPRITE RENDERS', $headingFont, $headingBrush, 1650, 14)
    for ($index = 0; $index -lt $screenshots.Count; $index++) {
        $column = $index % 2
        $row = [Math]::Floor($index / 2)
        $x = 1650 + (810 * $column)
        $y = 65 + (535 * $row)
        $graphics.DrawString($screenshots[$index].Label, $labelFont, $labelBrush, $x, $y)

        $afterImagePath = Join-Path $afterRoot $screenshots[$index].FileName
        $afterImage = [System.Drawing.Image]::FromFile($afterImagePath)
        try {
            $graphics.DrawImage($afterImage, $x, ($y + 30), 770, 481)
            $graphics.DrawRectangle($borderPen, $x, ($y + 30), 770, 481)
        }
        finally {
            $afterImage.Dispose()
        }
    }

    $noteX = 2460
    $noteY = 1140
    $qualificationText = @(
        'PASS - exact B/M/P publication',
        'PASS - Rifle/Revolver absent from ordinary selectors',
        'PASS - 30 Eastern + 12 spear item icons loaded',
        '',
        'Visual frames are supporting perceptual evidence.',
        'Structured guarded scenarios provide mechanical proof.'
    ) -join [Environment]::NewLine
    $graphics.DrawString('QUALIFICATION', $headingFont, $headingBrush, $noteX, $noteY)
    $graphics.DrawString(
        $qualificationText,
        $noteFont,
        $noteBrush,
        (New-Object System.Drawing.RectangleF $noteX, ($noteY + 45), 760, 330))

    $temporaryContactSheet = "$contactSheetPath.tmp"
    $canvas.Save($temporaryContactSheet, [System.Drawing.Imaging.ImageFormat]::Png)
    Move-Item -LiteralPath $temporaryContactSheet -Destination $contactSheetPath -Force
}
finally {
    $noteFont.Dispose()
    $labelFont.Dispose()
    $headingFont.Dispose()
    $borderPen.Dispose()
    $noteBrush.Dispose()
    $labelBrush.Dispose()
    $headingBrush.Dispose()
    $background.Dispose()
    $graphics.Dispose()
    $canvas.Dispose()
}

Write-Host "Curated $($curatedRecords.Count) lossless 1920x1200 runtime screenshots."
Write-Host "Evidence manifest: $manifestPath"
Write-Host "Before/after contact sheet: $contactSheetPath"
