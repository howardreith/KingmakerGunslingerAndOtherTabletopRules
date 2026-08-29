[CmdletBinding()]
param(
    [string]$RuntimeEvidenceDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$referenceRoot = Join-Path $repositoryRoot 'docs\reference\icon-polish-round-2\references'
$reportRoot = Join-Path $repositoryRoot 'docs\reports\icon-polish-round-2'
$previewPath = Join-Path $reportRoot 'exact-size-preview.png'
$previewManifestPath = Join-Path $reportRoot 'exact-size-preview-manifest.json'

$references = @(
    [pscustomobject]@{ Kind = 'original'; Label = '1. Cord equipped'; Relative = 'originals\01_cord_current_equipped.png' },
    [pscustomobject]@{ Kind = 'original'; Label = '2. Native ornate belt'; Relative = 'originals\02_native_belt_reference_ornate.png' },
    [pscustomobject]@{ Kind = 'original'; Label = '3. Native buckle belt'; Relative = 'originals\03_native_belt_reference_buckle.png' },
    [pscustomobject]@{ Kind = 'original'; Label = '4. Blunderbuss neighborhood'; Relative = 'originals\04_firearm_category_blunderbuss_reference.png' },
    [pscustomobject]@{ Kind = 'original'; Label = '5. Musket/Pistol neighborhood'; Relative = 'originals\05_firearm_category_musket_pistol_reference.png' },
    [pscustomobject]@{ Kind = 'crop'; Label = '1. Circular Cord crop'; Relative = 'crops\01_cord_current_top_down_circle.png' },
    [pscustomobject]@{ Kind = 'crop'; Label = '2. Native oblique belt crop'; Relative = 'crops\02_native_belt_reference_shallow_oblique.png' },
    [pscustomobject]@{ Kind = 'crop'; Label = '3. Native depth/buckle crop'; Relative = 'crops\03_native_belt_reference_front_depth.png' },
    [pscustomobject]@{ Kind = 'crop'; Label = '4. B glyph comparison crop'; Relative = 'crops\04_blunderbuss_vs_battle_axe_and_bite.png' },
    [pscustomobject]@{ Kind = 'crop'; Label = '5. M/P glyph comparison crop'; Relative = 'crops\05_musket_pistol_vs_native_category_icons.png' }
)

$selectorIcons = @(
    [pscustomobject]@{ Label = 'B'; Path = (Join-Path $repositoryRoot 'assets\game\icons\firearm-monogram-blunderbuss.png') },
    [pscustomobject]@{ Label = 'M'; Path = (Join-Path $repositoryRoot 'assets\game\icons\firearm-monogram-musket.png') },
    [pscustomobject]@{ Label = 'P'; Path = (Join-Path $repositoryRoot 'assets\game\icons\firearm-monogram-pistol.png') }
)
$cordIconPath = Join-Path $repositoryRoot 'assets\game\icons\cord-of-stubborn-resolve.png'
$cordSourcePath = Join-Path $repositoryRoot 'assets-source\original-icons\cord-of-stubborn-resolve\cord-of-stubborn-resolve-oblique-source.png'

function Write-Utf8WithoutBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )

    $encoding = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, $Text, $encoding)
}

function Get-ImageRecord {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Role
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required image is missing: $Path"
    }
    $image = [System.Drawing.Image]::FromFile($Path)
    try {
        return [ordered]@{
            file = $Path.Substring($repositoryRoot.Length + 1).Replace('\', '/')
            role = $Role
            width = $image.Width
            height = $image.Height
            sha256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
        }
    }
    finally {
        $image.Dispose()
    }
}

function Draw-FittedImage {
    param(
        [Parameter(Mandatory = $true)][System.Drawing.Graphics]$Graphics,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][System.Drawing.Rectangle]$Box,
        [Parameter(Mandatory = $true)][System.Drawing.Pen]$BorderPen
    )

    $image = [System.Drawing.Image]::FromFile($Path)
    try {
        $scale = [Math]::Min($Box.Width / [double]$image.Width,
            $Box.Height / [double]$image.Height)
        $width = [int][Math]::Round($image.Width * $scale)
        $height = [int][Math]::Round($image.Height * $scale)
        $x = $Box.X + [int](($Box.Width - $width) / 2)
        $y = $Box.Y + [int](($Box.Height - $height) / 2)
        $Graphics.DrawImage($image, $x, $y, $width, $height)
        $Graphics.DrawRectangle($BorderPen, $x, $y, $width, $height)
    }
    finally {
        $image.Dispose()
    }
}

function Draw-ExactIcon {
    param(
        [Parameter(Mandatory = $true)][System.Drawing.Graphics]$Graphics,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][int]$X,
        [Parameter(Mandatory = $true)][int]$Y,
        [Parameter(Mandatory = $true)][int]$Size,
        [Parameter(Mandatory = $true)][System.Drawing.Brush]$CellBrush,
        [Parameter(Mandatory = $true)][System.Drawing.Pen]$BorderPen
    )

    $Graphics.FillRectangle($CellBrush, $X, $Y, $Size, $Size)
    $image = [System.Drawing.Image]::FromFile($Path)
    try {
        $Graphics.DrawImage($image, $X, $Y, $Size, $Size)
    }
    finally {
        $image.Dispose()
    }
    $Graphics.DrawRectangle($BorderPen, $X, $Y, $Size, $Size)
}

foreach ($reference in $references) {
    $path = Join-Path $referenceRoot $reference.Relative
    $record = Get-ImageRecord -Path $path -Role (
        'supplied-' + $reference.Kind)
    if ($reference.Kind -eq 'original' -and
        ($record.width -ne 1920 -or $record.height -ne 1200)) {
        throw "Full-resolution reference is not 1920x1200: $path"
    }
}
foreach ($selector in $selectorIcons) {
    $record = Get-ImageRecord -Path $selector.Path -Role 'polished-selector'
    if ($record.width -ne 64 -or $record.height -ne 64) {
        throw "Selector is not 64x64: $($selector.Path)"
    }
}
$cordRecord = Get-ImageRecord -Path $cordIconPath -Role 'polished-cord'
if ($cordRecord.width -ne 128 -or $cordRecord.height -ne 128) {
    throw "Cord icon is not 128x128: $cordIconPath"
}
[void](Get-ImageRecord -Path $cordSourcePath -Role 'polished-cord-source')

New-Item -ItemType Directory -Force -Path $reportRoot | Out-Null
$canvas = New-Object System.Drawing.Bitmap 2800, 1800,
    ([System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$graphics = [System.Drawing.Graphics]::FromImage($canvas)
$background = New-Object System.Drawing.SolidBrush (
    [System.Drawing.Color]::FromArgb(23, 20, 17))
$panelBrush = New-Object System.Drawing.SolidBrush (
    [System.Drawing.Color]::FromArgb(48, 42, 36))
$cellBrush = New-Object System.Drawing.SolidBrush (
    [System.Drawing.Color]::FromArgb(31, 28, 25))
$parchmentBrush = New-Object System.Drawing.SolidBrush (
    [System.Drawing.Color]::FromArgb(203, 188, 156))
$headingBrush = New-Object System.Drawing.SolidBrush (
    [System.Drawing.Color]::FromArgb(221, 193, 120))
$mutedBrush = New-Object System.Drawing.SolidBrush (
    [System.Drawing.Color]::FromArgb(159, 145, 120))
$borderPen = New-Object System.Drawing.Pen (
    [System.Drawing.Color]::FromArgb(146, 120, 72)), 2
$headingFont = New-Object System.Drawing.Font 'Segoe UI', 30,
    ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
$labelFont = New-Object System.Drawing.Font 'Segoe UI', 19,
    ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$smallFont = New-Object System.Drawing.Font 'Segoe UI', 16,
    ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
try {
    $graphics.FillRectangle($background, 0, 0, $canvas.Width, $canvas.Height)
    $graphics.InterpolationMode =
        [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode =
        [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality =
        [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    $graphics.DrawString('ICON POLISH ROUND 2 - ALL INPUTS + EXACT-SIZE OUTPUTS',
        $headingFont, $headingBrush, 35, 20)
    $graphics.DrawString(
        'Every supplied 1920x1200 screenshot and every focused crop is included below.',
        $labelFont, $parchmentBrush, 40, 68)

    $originals = @($references | Where-Object { $_.Kind -eq 'original' })
    for ($index = 0; $index -lt $originals.Count; $index++) {
        $x = 25 + (550 * $index)
        $graphics.DrawString($originals[$index].Label, $smallFont,
            $parchmentBrush, $x, 112)
        Draw-FittedImage -Graphics $graphics -Path (
            Join-Path $referenceRoot $originals[$index].Relative) -Box (
            New-Object System.Drawing.Rectangle $x, 145, 520, 325) -BorderPen $borderPen
    }

    $crops = @($references | Where-Object { $_.Kind -eq 'crop' })
    for ($index = 0; $index -lt $crops.Count; $index++) {
        $x = 25 + (550 * $index)
        $graphics.DrawString($crops[$index].Label, $smallFont,
            $parchmentBrush, $x, 500)
        Draw-FittedImage -Graphics $graphics -Path (
            Join-Path $referenceRoot $crops[$index].Relative) -Box (
            New-Object System.Drawing.Rectangle $x, 535, 520, 330) -BorderPen $borderPen
    }

    $graphics.FillRectangle($panelBrush, 25, 920, 2750, 835)
    $graphics.DrawString('POLISHED FIREARM CATEGORY TILES',
        $headingFont, $headingBrush, 75, 955)
    $graphics.DrawString(
        'Left: exact 64 x 64 runtime asset size. Right: exact 32 x 32 half-size check. Enlargements are secondary inspection only.',
        $labelFont, $parchmentBrush, 80, 1005)
    for ($index = 0; $index -lt $selectorIcons.Count; $index++) {
        $x = 100 + (310 * $index)
        Draw-ExactIcon -Graphics $graphics -Path $selectorIcons[$index].Path -X $x -Y 1080 -Size 64 -CellBrush $cellBrush -BorderPen $borderPen
        $graphics.DrawString($selectorIcons[$index].Label + '  64 px',
            $labelFont, $headingBrush, $x, 1155)
        Draw-ExactIcon -Graphics $graphics -Path $selectorIcons[$index].Path -X ($x + 120) -Y 1096 -Size 32 -CellBrush $cellBrush -BorderPen $borderPen
        $graphics.DrawString('32 px', $smallFont, $mutedBrush,
            ($x + 110), 1155)
        $priorInterpolation = $graphics.InterpolationMode
        $graphics.InterpolationMode =
            [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        Draw-ExactIcon -Graphics $graphics -Path $selectorIcons[$index].Path -X $x -Y 1260 -Size 256 -CellBrush $cellBrush -BorderPen $borderPen
        $graphics.InterpolationMode = $priorInterpolation
    }

    $graphics.DrawString('POLISHED CORD OF STUBBORN RESOLVE',
        $headingFont, $headingBrush, 1110, 955)
    $graphics.DrawString(
        'Exact 128 x 128 source cell and 64 x 64 equipment-scale check; oblique braid source shown at right.',
        $labelFont, $parchmentBrush, 1115, 1005)
    Draw-ExactIcon -Graphics $graphics -Path $cordIconPath -X 1140 -Y 1090 -Size 128 -CellBrush $cellBrush -BorderPen $borderPen
    $graphics.DrawString('128 px', $labelFont, $headingBrush, 1170, 1230)
    Draw-ExactIcon -Graphics $graphics -Path $cordIconPath -X 1350 -Y 1122 -Size 64 -CellBrush $cellBrush -BorderPen $borderPen
    $graphics.DrawString('64 px', $smallFont, $mutedBrush, 1358, 1230)
    Draw-FittedImage -Graphics $graphics -Path $cordSourcePath -Box (
        New-Object System.Drawing.Rectangle 1510, 1070, 1185, 605) -BorderPen $borderPen
    $graphics.DrawString(
        'Judgment gate: readable horizontal belt silhouette, front/rear depth, knot and short tails at native viewing size.',
        $labelFont, $parchmentBrush, 1115, 1690)

    $temporaryPreview = "$previewPath.tmp"
    $canvas.Save($temporaryPreview, [System.Drawing.Imaging.ImageFormat]::Png)
    Move-Item -LiteralPath $temporaryPreview -Destination $previewPath -Force
}
finally {
    $smallFont.Dispose()
    $labelFont.Dispose()
    $headingFont.Dispose()
    $borderPen.Dispose()
    $mutedBrush.Dispose()
    $headingBrush.Dispose()
    $parchmentBrush.Dispose()
    $cellBrush.Dispose()
    $panelBrush.Dispose()
    $background.Dispose()
    $graphics.Dispose()
    $canvas.Dispose()
}

$referenceRecords = @()
foreach ($reference in $references) {
    $referenceRecords += Get-ImageRecord -Path (
        Join-Path $referenceRoot $reference.Relative) -Role (
        'supplied-' + $reference.Kind)
}
$outputRecords = @()
foreach ($selector in $selectorIcons) {
    $outputRecords += Get-ImageRecord -Path $selector.Path -Role (
        'polished-selector-' + $selector.Label)
}
$outputRecords += Get-ImageRecord -Path $cordIconPath -Role 'polished-cord'
$outputRecords += Get-ImageRecord -Path $cordSourcePath -Role 'polished-cord-source'
$previewManifest = [ordered]@{
    schemaVersion = 1
    title = 'Kingmaker Gunslinger icon polish Round 2 exact-size preview'
    inputCount = $referenceRecords.Count
    fullResolutionInputCount = @($referenceRecords | Where-Object {
        $_.role -eq 'supplied-original' }).Count
    focusedCropInputCount = @($referenceRecords | Where-Object {
        $_.role -eq 'supplied-crop' }).Count
    selectorJudgmentSizes = @(64, 32)
    cordJudgmentSizes = @(128, 64)
    inputs = $referenceRecords
    outputs = $outputRecords
    preview = Get-ImageRecord -Path $previewPath -Role 'exact-size-preview'
}
Write-Utf8WithoutBom -Path $previewManifestPath -Text (
    ($previewManifest | ConvertTo-Json -Depth 8) + [Environment]::NewLine)

if (-not [string]::IsNullOrWhiteSpace($RuntimeEvidenceDirectory)) {
    $runtimeRoot = (Resolve-Path $RuntimeEvidenceDirectory).Path
    $runtimeResultPath = Join-Path $runtimeRoot 'runtime-result.json'
    $runtimeIndexPath = Join-Path $runtimeRoot 'icon-overhaul-visual-index.json'
    $orchestrationPath = Join-Path $runtimeRoot 'orchestration.json'
    foreach ($requiredPath in @($runtimeResultPath, $runtimeIndexPath,
        $orchestrationPath)) {
        if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
            throw "Required runtime evidence input is missing: $requiredPath"
        }
    }

    $runtimeResult = Get-Content -LiteralPath $runtimeResultPath -Raw |
        ConvertFrom-Json
    $runtimeIndex = Get-Content -LiteralPath $runtimeIndexPath -Raw |
        ConvertFrom-Json
    $orchestration = Get-Content -LiteralPath $orchestrationPath -Raw |
        ConvertFrom-Json
    if ($runtimeResult.scenario -ne 'icon-overhaul-visual-evidence' -or
        $runtimeResult.status -ne 'PASS') {
        throw "Expected passing icon-overhaul-visual-evidence runtime evidence."
    }
    if ($runtimeIndex.renderWidth -ne 1920 -or
        $runtimeIndex.renderHeight -ne 1200) {
        throw "Runtime visual index does not describe 1920x1200 frames."
    }

    $runtimeScreenshots = @(
        [pscustomobject]@{ FileName = 'after-06-round2-weapon-focus-b-comparison.png'; Label = 'B beside Battle Axe and Bite' },
        [pscustomobject]@{ FileName = 'after-07-round2-weapon-focus-mp-comparison.png'; Label = 'M/P native neighborhood' },
        [pscustomobject]@{ FileName = 'after-08-round2-rapid-reload-shared-icons.png'; Label = 'Shared Rapid Reload B/M/P' },
        [pscustomobject]@{ FileName = 'after-09-round2-cord-equipped.png'; Label = 'Cord equipped' },
        [pscustomobject]@{ FileName = 'after-10-round2-cord-inventory.png'; Label = 'Cord inventory comparison' },
        [pscustomobject]@{ FileName = 'after-11-round2-native-belt-equipped.png'; Label = 'Native belt equipped' }
    )
    $runtimeAfterRoot = Join-Path $reportRoot 'runtime-after'
    New-Item -ItemType Directory -Force -Path $runtimeAfterRoot | Out-Null
    $runtimeRecords = @()
    foreach ($screenshot in $runtimeScreenshots) {
        $sourcePath = Join-Path $runtimeRoot $screenshot.FileName
        $sourceRecord = Get-ImageRecord -Path $sourcePath -Role 'runtime-source'
        if ($sourceRecord.width -ne 1920 -or $sourceRecord.height -ne 1200) {
            throw "Runtime frame is not 1920x1200: $sourcePath"
        }
        $indexed = @($runtimeIndex.screenshots | Where-Object {
            $_.fileName -eq $screenshot.FileName })
        if ($indexed.Count -ne 1 -or
            ([string]$indexed[0].sha256).ToLowerInvariant() -ne
                $sourceRecord.sha256) {
            throw "Runtime index mismatch for $($screenshot.FileName)."
        }
        $destinationPath = Join-Path $runtimeAfterRoot $screenshot.FileName
        Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force
        $runtimeRecords += [ordered]@{
            file = ('icon-polish-round-2/runtime-after/' +
                $screenshot.FileName)
            label = $screenshot.Label
            width = 1920
            height = 1200
            sha256 = $sourceRecord.sha256
        }
    }

    $deploymentManifestPath = [string]$orchestration.deploymentManifestPath
    $deployment = Get-Content -LiteralPath $deploymentManifestPath -Raw |
        ConvertFrom-Json
    $runtimeManifest = [ordered]@{
        schemaVersion = 1
        evidenceRole = 'Supporting perceptual evidence from live in-game Unity sprites; structured runtime assertions are authoritative.'
        sourceRunId = [string]$runtimeResult.runId
        sourceScenario = [string]$runtimeResult.scenario
        sourceStatus = [string]$runtimeResult.status
        loadedModVersion = [string]$runtimeResult.loadedModVersion
        gitCommitAtBuild = [string]$runtimeResult.gitCommit
        packageSha256 = [string]$deployment.packageSha256
        dllSha256 = [string]$deployment.dllSha256
        dllMvid = [string]$deployment.dllMvid
        screenshots = $runtimeRecords
    }
    Write-Utf8WithoutBom -Path (
        Join-Path $runtimeAfterRoot 'manifest.json') -Text (
        ($runtimeManifest | ConvertTo-Json -Depth 8) +
            [Environment]::NewLine)

    $contactSheetPath = Join-Path $repositoryRoot (
        'docs\reports\icon-polish-round-2-before-after-contact-sheet.png')
    $contact = New-Object System.Drawing.Bitmap 3600, 1900,
        ([System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $contactGraphics = [System.Drawing.Graphics]::FromImage($contact)
    $contactBackground = New-Object System.Drawing.SolidBrush (
        [System.Drawing.Color]::FromArgb(23, 20, 17))
    $contactHeadingBrush = New-Object System.Drawing.SolidBrush (
        [System.Drawing.Color]::FromArgb(221, 193, 120))
    $contactParchmentBrush = New-Object System.Drawing.SolidBrush (
        [System.Drawing.Color]::FromArgb(203, 188, 156))
    $contactBorderPen = New-Object System.Drawing.Pen (
        [System.Drawing.Color]::FromArgb(146, 120, 72)), 2
    $contactHeadingFont = New-Object System.Drawing.Font 'Segoe UI', 30,
        ([System.Drawing.FontStyle]::Bold),
        ([System.Drawing.GraphicsUnit]::Pixel)
    $contactLabelFont = New-Object System.Drawing.Font 'Segoe UI', 16,
        ([System.Drawing.FontStyle]::Regular),
        ([System.Drawing.GraphicsUnit]::Pixel)
    try {
        $contactGraphics.FillRectangle($contactBackground, 0, 0,
            $contact.Width, $contact.Height)
        $contactGraphics.InterpolationMode =
            [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $contactGraphics.DrawString('BEFORE - ALL SUPPLIED REFERENCES',
            $contactHeadingFont, $contactHeadingBrush, 30, 20)
        Draw-FittedImage -Graphics $contactGraphics -Path (
            Join-Path $referenceRoot 'CONTACT_SHEET.png') -Box (
            New-Object System.Drawing.Rectangle 25, 75, 1650, 1740) -BorderPen $contactBorderPen
        $contactGraphics.DrawString(
            'AFTER - GUARDED LIVE-SPRITE RENDERS (1920 x 1200 EACH)',
            $contactHeadingFont, $contactHeadingBrush, 1725, 20)
        for ($index = 0; $index -lt $runtimeScreenshots.Count; $index++) {
            $column = $index % 2
            $row = [Math]::Floor($index / 2)
            $x = 1725 + (925 * $column)
            $y = 80 + (580 * $row)
            $contactGraphics.DrawString($runtimeScreenshots[$index].Label,
                $contactLabelFont, $contactParchmentBrush, $x, $y)
            Draw-FittedImage -Graphics $contactGraphics -Path (
                Join-Path $runtimeAfterRoot $runtimeScreenshots[$index].FileName) -Box (
                New-Object System.Drawing.Rectangle $x, ($y + 30), 880, 530) -BorderPen $contactBorderPen
        }
        $temporaryContact = "$contactSheetPath.tmp"
        $contact.Save($temporaryContact,
            [System.Drawing.Imaging.ImageFormat]::Png)
        Move-Item -LiteralPath $temporaryContact -Destination (
            $contactSheetPath) -Force
    }
    finally {
        $contactLabelFont.Dispose()
        $contactHeadingFont.Dispose()
        $contactBorderPen.Dispose()
        $contactParchmentBrush.Dispose()
        $contactHeadingBrush.Dispose()
        $contactBackground.Dispose()
        $contactGraphics.Dispose()
        $contact.Dispose()
    }
    Write-Host "Curated $($runtimeRecords.Count) Round 2 runtime frames."
}

Write-Host "Exact-size preview: $previewPath"
Write-Host "Preview manifest: $previewManifestPath"
