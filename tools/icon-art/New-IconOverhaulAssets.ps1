[CmdletBinding()]
param(
    [ValidateSet('All', 'Feat', 'Items')]
    [string]$Mode = 'All'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$runtime = Join-Path $root 'assets/game/icons'
$featSource = Join-Path $root 'assets-source/original-icons/firearm-feats'
$itemSource = Join-Path $root 'assets-source/original-icons/firearm-items'
$preview = Join-Path $root 'docs/reports/icon-overhaul-asset-preview.png'
$manifestPath = Join-Path $root 'assets-source/original-icons/icon-overhaul-assets.json'
$sourceCanvas = 512

function Convert-HexColor([string]$value) {
    return [Drawing.ColorTranslator]::FromHtml($value)
}

function New-Graphics([Drawing.Bitmap]$bitmap) {
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode =
        [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode =
        [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality =
        [Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.Clear([Drawing.Color]::Transparent)
    return $graphics
}

function New-RoundedRectanglePath(
    [single]$x, [single]$y, [single]$width, [single]$height,
    [single]$radius) {
    $diameter = $radius * 2
    $path = [Drawing.Drawing2D.GraphicsPath]::new()
    $path.AddArc($x, $y, $diameter, $diameter, 180, 90)
    $path.AddArc($x + $width - $diameter, $y,
        $diameter, $diameter, 270, 90)
    $path.AddArc($x + $width - $diameter,
        $y + $height - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($x, $y + $height - $diameter,
        $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    return $path
}

function Get-AlphaBounds([Drawing.Bitmap]$bitmap, [byte]$threshold = 3) {
    $rectangle = [Drawing.Rectangle]::new(0, 0, $bitmap.Width, $bitmap.Height)
    $data = $bitmap.LockBits($rectangle,
        [Drawing.Imaging.ImageLockMode]::ReadOnly,
        [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $length = [Math]::Abs($data.Stride) * $data.Height
        $bytes = [byte[]]::new($length)
        [Runtime.InteropServices.Marshal]::Copy(
            $data.Scan0, $bytes, 0, $length)
        $minimumX = $bitmap.Width
        $minimumY = $bitmap.Height
        $maximumX = -1
        $maximumY = -1
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            $row = if ($data.Stride -ge 0) { $y * $data.Stride }
                else { ($bitmap.Height - 1 - $y) * (-$data.Stride) }
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                if ($bytes[$row + $x * 4 + 3] -le $threshold) { continue }
                if ($x -lt $minimumX) { $minimumX = $x }
                if ($x -gt $maximumX) { $maximumX = $x }
                if ($y -lt $minimumY) { $minimumY = $y }
                if ($y -gt $maximumY) { $maximumY = $y }
            }
        }
        if ($maximumX -lt $minimumX -or $maximumY -lt $minimumY) {
            throw 'Icon source has no visible alpha pixels.'
        }
        return [Drawing.Rectangle]::FromLTRB(
            $minimumX, $minimumY, $maximumX + 1, $maximumY + 1)
    }
    finally { $bitmap.UnlockBits($data) }
}

function Save-Png([Drawing.Bitmap]$bitmap, [string]$path) {
    $directory = Split-Path -Parent $path
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $bitmap.Save($path, [Drawing.Imaging.ImageFormat]::Png)
}

function Export-FittedIcon(
    [string]$sourcePath, [string]$destinationPath,
    [int]$size, [int]$margin) {
    $source = [Drawing.Bitmap]::new($sourcePath)
    try {
        $bounds = Get-AlphaBounds $source
        if ($bounds.Left -eq 0 -or $bounds.Top -eq 0 -or
            $bounds.Right -eq $source.Width -or
            $bounds.Bottom -eq $source.Height) {
            throw "Icon source touches a canvas edge: $sourcePath"
        }
        $available = $size - 2 * $margin
        $scale = [Math]::Min(
            $available / [double]$bounds.Width,
            $available / [double]$bounds.Height)
        $width = [Math]::Max(1, [int][Math]::Round($bounds.Width * $scale))
        $height = [Math]::Max(1, [int][Math]::Round($bounds.Height * $scale))
        $x = [int][Math]::Round(($size - $width) / 2.0)
        $y = [int][Math]::Round(($size - $height) / 2.0)
        $target = [Drawing.Bitmap]::new($size, $size,
            [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = New-Graphics $target
        try {
            $graphics.CompositingMode =
                [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $destination = [Drawing.Rectangle]::new($x, $y, $width, $height)
            $graphics.DrawImage($source, $destination, $bounds.X, $bounds.Y,
                $bounds.Width, $bounds.Height, [Drawing.GraphicsUnit]::Pixel)
            Save-Png $target $destinationPath
        }
        finally {
            $graphics.Dispose()
            $target.Dispose()
        }
        return [pscustomobject]@{
            SourcePath = $sourcePath
            DestinationPath = $destinationPath
            SourceWidth = $source.Width
            SourceHeight = $source.Height
            SourceBounds = '{0},{1},{2},{3}' -f $bounds.X, $bounds.Y,
                $bounds.Width, $bounds.Height
            TargetSize = $size
            TargetMargin = $margin
        }
    }
    finally { $source.Dispose() }
}

function Draw-SelectorField([Drawing.Graphics]$graphics, [int]$seed) {
    $fill = [Drawing.Drawing2D.LinearGradientBrush]::new(
        [Drawing.PointF]::new(5, 3), [Drawing.PointF]::new(59, 61),
        (Convert-HexColor '#2B1716'), (Convert-HexColor '#754A32'))
    $outer = [Drawing.Pen]::new((Convert-HexColor '#1A0F0D'), 2.2)
    $inner = [Drawing.Pen]::new((Convert-HexColor '#B28B55'), 1.15)
    try {
        $graphics.FillRectangle($fill, 0, 0, 64, 64)
        $graphics.DrawRectangle($outer, 1.1, 1.1, 61.8, 61.8)
        $graphics.DrawRectangle($inner, 4.0, 4.0, 56.0, 56.0)
    }
    finally { $fill.Dispose(); $outer.Dispose(); $inner.Dispose() }

    $random = [Random]::new($seed)
    for ($index = 0; $index -lt 90; $index++) {
        $alpha = 5 + $random.Next(12)
        $tone = if (($index % 3) -eq 0) {
            [Drawing.Color]::FromArgb($alpha, 225, 181, 119)
        } else { [Drawing.Color]::FromArgb($alpha, 20, 9, 8) }
        $brush = [Drawing.SolidBrush]::new($tone)
        try {
            $x = 5 + $random.NextDouble() * 54
            $y = 5 + $random.NextDouble() * 54
            $radius = 0.25 + $random.NextDouble() * 0.8
            $graphics.FillEllipse($brush, $x, $y, $radius, $radius)
        }
        finally { $brush.Dispose() }
    }

    $ornament = [Drawing.Pen]::new((Convert-HexColor '#D1AE72'), 1.15)
    try {
        $graphics.DrawLine($ornament, 7, 14, 7, 7)
        $graphics.DrawLine($ornament, 7, 7, 14, 7)
        $graphics.DrawLine($ornament, 50, 57, 57, 57)
        $graphics.DrawLine($ornament, 57, 57, 57, 50)
        $graphics.DrawBezier($ornament, 7, 50, 11, 57, 16, 57, 19, 56)
        $graphics.DrawBezier($ornament, 45, 8, 50, 7, 55, 12, 57, 16)
    }
    finally { $ornament.Dispose() }
}

function New-MonogramPath([string]$letter) {
    $path = [Drawing.Drawing2D.GraphicsPath]::new()
    switch ($letter) {
        'P' {
            $path.StartFigure()
            $path.AddBezier(20, 49, 19, 38, 20, 24, 22, 13)
            $path.StartFigure()
            $path.AddBezier(21, 15, 39, 9, 49, 15, 47, 25)
            $path.AddBezier(47, 25, 45, 34, 30, 34, 22, 31)
            $path.StartFigure(); $path.AddLine(15, 49, 27, 49)
            $path.StartFigure(); $path.AddLine(17, 14, 27, 14)
        }
        'M' {
            $path.StartFigure()
            $path.AddBezier(14, 49, 15, 36, 16, 23, 18, 14)
            $path.AddBezier(18, 14, 24, 22, 28, 31, 32, 39)
            $path.AddBezier(32, 39, 36, 29, 41, 20, 47, 14)
            $path.AddBezier(47, 14, 48, 25, 49, 38, 50, 49)
            $path.StartFigure(); $path.AddLine(10, 49, 22, 49)
            $path.StartFigure(); $path.AddLine(44, 49, 55, 49)
        }
        'B' {
            $path.StartFigure()
            $path.AddBezier(19, 49, 19, 37, 20, 24, 21, 13)
            $path.StartFigure()
            $path.AddBezier(21, 14, 38, 10, 47, 15, 46, 24)
            $path.AddBezier(46, 24, 44, 31, 31, 31, 22, 29)
            $path.StartFigure()
            $path.AddBezier(22, 29, 41, 26, 50, 33, 47, 42)
            $path.AddBezier(47, 42, 44, 51, 30, 50, 20, 47)
            $path.StartFigure(); $path.AddLine(14, 49, 27, 49)
        }
        default { throw "Unsupported original monogram path: $letter" }
    }
    return $path
}

function Draw-OriginalMonogram(
    [Drawing.Graphics]$graphics, [string]$letter) {
    $path = New-MonogramPath $letter
    $shadowPath = [Drawing.Drawing2D.GraphicsPath]$path.Clone()
    $matrix = [Drawing.Drawing2D.Matrix]::new()
    $matrix.Translate(1.0, 1.2)
    $shadowPath.Transform($matrix)
    $shadow = [Drawing.Pen]::new((Convert-HexColor '#170C0A'), 6.4)
    $gold = [Drawing.Pen]::new((Convert-HexColor '#D2AE73'), 4.35)
    $highlight = [Drawing.Pen]::new((Convert-HexColor '#F0D69B'), 1.15)
    foreach ($pen in @($shadow, $gold, $highlight)) {
        $pen.StartCap = [Drawing.Drawing2D.LineCap]::Round
        $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
        $pen.LineJoin = [Drawing.Drawing2D.LineJoin]::Round
    }
    try {
        $graphics.DrawPath($shadow, $shadowPath)
        $graphics.DrawPath($gold, $path)
        $graphics.DrawPath($highlight, $path)
        $flourishShadow = [Drawing.Pen]::new(
            [Drawing.Color]::FromArgb(135, 20, 10, 8), 3.8)
        $flourish = [Drawing.Pen]::new((Convert-HexColor '#C49B62'), 2.2)
        try {
            $graphics.DrawBezier($flourishShadow,
                12, 53, 25, 59, 39, 45, 53, 50)
            $graphics.DrawBezier($flourish,
                11, 52, 24, 58, 39, 44, 53, 49)
        }
        finally { $flourishShadow.Dispose(); $flourish.Dispose() }
    }
    finally {
        $path.Dispose(); $shadowPath.Dispose(); $matrix.Dispose()
        $shadow.Dispose(); $gold.Dispose(); $highlight.Dispose()
    }
}

function Draw-RapidReloadGlyph([Drawing.Graphics]$graphics) {
    $shadow = [Drawing.Pen]::new(
        [Drawing.Color]::FromArgb(120, 64, 31, 26), 7.2)
    $ink = [Drawing.Pen]::new((Convert-HexColor '#A6533F'), 5.6)
    $highlight = [Drawing.Pen]::new((Convert-HexColor '#C77B63'), 1.2)
    foreach ($pen in @($shadow, $ink, $highlight)) {
        $pen.StartCap = [Drawing.Drawing2D.LineCap]::Round
        $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
        $pen.LineJoin = [Drawing.Drawing2D.LineJoin]::Round
    }
    try {
        $graphics.DrawArc($shadow, 10.5, 9.5, 44, 44, 205, 274)
        $graphics.DrawArc($ink, 9.5, 8.5, 44, 44, 205, 274)
        $graphics.DrawArc($highlight, 10.2, 9.2, 42.5, 42.5, 215, 235)
        $shadowBrush = [Drawing.SolidBrush]::new(
            [Drawing.Color]::FromArgb(120, 64, 31, 26))
        $inkBrush = [Drawing.SolidBrush]::new(
            (Convert-HexColor '#A6533F'))
        try {
            $graphics.FillPolygon($shadowBrush, @(
                [Drawing.PointF]::new(11.5, 14.5),
                [Drawing.PointF]::new(25.5, 16.5),
                [Drawing.PointF]::new(17.5, 29.0)))
            $graphics.FillPolygon($inkBrush, @(
                [Drawing.PointF]::new(10.5, 13.5),
                [Drawing.PointF]::new(24.5, 15.5),
                [Drawing.PointF]::new(16.5, 28.0)))
        }
        finally { $shadowBrush.Dispose(); $inkBrush.Dispose() }

        $graphics.DrawLine($shadow, 23.5, 47.0, 43.5, 25.0)
        $graphics.DrawLine($ink, 22.5, 46.0, 42.5, 24.0)
        $graphics.DrawLine($highlight, 23.0, 45.0, 41.5, 24.7)
        $graphics.DrawLine($shadow, 39.5, 19.0, 43.5, 25.0)
        $graphics.DrawLine($shadow, 43.5, 25.0, 50.0, 23.5)
        $graphics.DrawLine($ink, 38.5, 18.0, 42.5, 24.0)
        $graphics.DrawLine($ink, 42.5, 24.0, 49.0, 22.5)
        $graphics.DrawEllipse($shadow, 18.5, 43.0, 8.0, 8.0)
        $graphics.DrawEllipse($ink, 17.5, 42.0, 8.0, 8.0)
    }
    finally { $shadow.Dispose(); $ink.Dispose(); $highlight.Dispose() }
}

function Export-FullCanvasIcon(
    [string]$sourcePath, [string]$destinationPath, [int]$size) {
    $source = [Drawing.Bitmap]::new($sourcePath)
    try {
        if ($source.Width -ne $sourceCanvas -or
            $source.Height -ne $sourceCanvas) {
            throw "Feat source is not ${sourceCanvas}px: $sourcePath"
        }
        $target = [Drawing.Bitmap]::new($size, $size,
            [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = New-Graphics $target
        try {
            $graphics.CompositingMode =
                [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.DrawImage($source, 0, 0, $size, $size)
            Save-Png $target $destinationPath
        }
        finally { $graphics.Dispose(); $target.Dispose() }
        return [pscustomobject]@{
            SourcePath = $sourcePath
            DestinationPath = $destinationPath
            SourceWidth = $source.Width
            SourceHeight = $source.Height
            SourceBounds = 'full-canvas'
            TargetSize = $size
            TargetMargin = 0
        }
    }
    finally { $source.Dispose() }
}

function New-FeatSource(
    [string]$key, [string]$letter, [int]$seed, [bool]$rapid) {
    $directory = Join-Path $featSource 'sources'
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    $path = Join-Path $directory ($key + '-source.png')
    $bitmap = [Drawing.Bitmap]::new($sourceCanvas, $sourceCanvas,
        [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = New-Graphics $bitmap
    try {
        $graphics.ScaleTransform(8.0, 8.0)
        if ($rapid) {
            Draw-RapidReloadGlyph $graphics
        }
        else {
            Draw-SelectorField $graphics $seed
            Draw-OriginalMonogram $graphics $letter
        }
        $graphics.ResetTransform()
        Save-Png $bitmap $path
    }
    finally { $graphics.Dispose(); $bitmap.Dispose() }
    return $path
}

$outputs = [Collections.Generic.List[object]]::new()
$featEntries = @(
    [pscustomobject]@{ Key = 'firearm-monogram-pistol';
        Letter = 'P'; Seed = 9201; Rapid = $false },
    [pscustomobject]@{ Key = 'firearm-monogram-musket';
        Letter = 'M'; Seed = 9202; Rapid = $false },
    [pscustomobject]@{ Key = 'firearm-monogram-blunderbuss';
        Letter = 'B'; Seed = 9203; Rapid = $false },
    [pscustomobject]@{ Key = 'rapid-reload';
        Letter = ''; Seed = 9204; Rapid = $true }
)

if ($Mode -eq 'All' -or $Mode -eq 'Feat') {
    foreach ($entry in $featEntries) {
        $sourcePath = New-FeatSource $entry.Key $entry.Letter $entry.Seed $entry.Rapid
        $result = Export-FullCanvasIcon $sourcePath (Join-Path $runtime ($entry.Key + '.png')) 64
        $outputs.Add($result) | Out-Null
    }
}

$itemEntries = @(
    [pscustomobject]@{ Key = 'early-pistol'; Source =
        'assets-source/original-icons/firearm-items/early-pistol-source.png';
        Kind = 'imagegen-original-firearm' },
    [pscustomobject]@{ Key = 'musket'; Source =
        'assets-source/original-icons/firearm-items/musket-source.png';
        Kind = 'imagegen-original-firearm' },
    [pscustomobject]@{ Key = 'blunderbuss'; Source =
        'assets-source/original-icons/firearm-items/blunderbuss-source.png';
        Kind = 'imagegen-original-firearm' },
    [pscustomobject]@{ Key = 'wakizashi'; Source =
        'assets-source/original-models/eastern-weapons/wakizashi-icon-source.png';
        Kind = 'project-fbx-render' },
    [pscustomobject]@{ Key = 'katana'; Source =
        'assets-source/original-models/eastern-weapons/katana-icon-source.png';
        Kind = 'project-fbx-render' },
    [pscustomobject]@{ Key = 'nodachi'; Source =
        'assets-source/original-models/eastern-weapons/nodachi-icon-source.png';
        Kind = 'project-fbx-render' },
    [pscustomobject]@{ Key = 'night-without-moon'; Source =
        'assets-source/original-models/eastern-weapons/night-without-moon-icon-source.png';
        Kind = 'project-fbx-render' },
    [pscustomobject]@{ Key = 'heavens-measure'; Source =
        'assets-source/original-models/eastern-weapons/heavens-measure-icon-source.png';
        Kind = 'project-fbx-render' },
    [pscustomobject]@{ Key = 'world-tree-severer'; Source =
        'assets-source/original-models/eastern-weapons/world-tree-severer-icon-source.png';
        Kind = 'project-fbx-render' },
    [pscustomobject]@{ Key = 'elven-branched-spear'; Source =
        'assets-source/original-models/elven-branched-spear/elven-branched-spear-icon.png';
        Kind = 'project-fbx-render' }
)

if ($Mode -eq 'All' -or $Mode -eq 'Items') {
    foreach ($entry in $itemEntries) {
        $result = Export-FittedIcon (Join-Path $root $entry.Source) (Join-Path $runtime ($entry.Key + '.png')) 128 5
        $outputs.Add($result) | Out-Null
    }
}

function Get-ImageRecord([string]$key, [string]$sourcePath,
    [string]$sourceKind, [int]$expectedSize) {
    $finalPath = Join-Path $runtime ($key + '.png')
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf) -or
        -not (Test-Path -LiteralPath $finalPath -PathType Leaf)) {
        throw "Icon manifest input is missing: $key"
    }
    $sourceImage = [Drawing.Bitmap]::new($sourcePath)
    $finalImage = [Drawing.Bitmap]::new($finalPath)
    try {
        $sourceBounds = Get-AlphaBounds $sourceImage
        $finalBounds = Get-AlphaBounds $finalImage
        if ($finalImage.Width -ne $expectedSize -or
            $finalImage.Height -ne $expectedSize) {
            throw "Unexpected final icon dimensions: $key"
        }
        return [ordered]@{
            key = $key
            sourceKind = $sourceKind
            sourcePath = $sourcePath.Substring($root.Length + 1).Replace('\', '/')
            sourceDimensions = @($sourceImage.Width, $sourceImage.Height)
            sourceAlphaBounds = @($sourceBounds.X, $sourceBounds.Y,
                $sourceBounds.Width, $sourceBounds.Height)
            sourceSha256 = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant()
            finalPath = $finalPath.Substring($root.Length + 1).Replace('\', '/')
            finalDimensions = @($finalImage.Width, $finalImage.Height)
            finalAlphaBounds = @($finalBounds.X, $finalBounds.Y,
                $finalBounds.Width, $finalBounds.Height)
            cornerAlpha = @(
                $finalImage.GetPixel(0, 0).A,
                $finalImage.GetPixel($finalImage.Width - 1, 0).A,
                $finalImage.GetPixel(0, $finalImage.Height - 1).A,
                $finalImage.GetPixel(
                    $finalImage.Width - 1, $finalImage.Height - 1).A)
            finalSha256 = (Get-FileHash -LiteralPath $finalPath -Algorithm SHA256).Hash.ToLowerInvariant()
        }
    }
    finally { $sourceImage.Dispose(); $finalImage.Dispose() }
}

function New-FirearmFeatMap {
    $sheet = [Drawing.Bitmap]::new(376, 128,
        [Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $graphics = [Drawing.Graphics]::FromImage($sheet)
    $font = [Drawing.Font]::new('Arial', 9, [Drawing.FontStyle]::Bold,
        [Drawing.GraphicsUnit]::Pixel)
    $brush = [Drawing.SolidBrush]::new((Convert-HexColor '#E8D6A9'))
    try {
        $graphics.Clear((Convert-HexColor '#202127'))
        $graphics.InterpolationMode =
            [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        for ($index = 0; $index -lt $featEntries.Count; $index++) {
            $entry = $featEntries[$index]
            $image = [Drawing.Image]::FromFile(
                (Join-Path $runtime ($entry.Key + '.png')))
            try {
                $x = 10 + $index * 92
                $graphics.DrawImage($image, $x, 6, 64, 64)
                $graphics.DrawImage($image, $x + 50, 82, 32, 32)
                $label = if ($entry.Rapid) { 'Rapid Reload' }
                    else { $entry.Key.Replace('firearm-monogram-', '') }
                $graphics.DrawString($label, $font, $brush, $x, 75)
            }
            finally { $image.Dispose() }
        }
        Save-Png $sheet (Join-Path $featSource 'firearm-feat-icon-map.png')
    }
    finally {
        $graphics.Dispose(); $font.Dispose(); $brush.Dispose(); $sheet.Dispose()
    }
}

function New-AssetPreview([System.Collections.IEnumerable]$records) {
    $values = @($records)
    $columns = 5
    $cellWidth = 190
    $cellHeight = 180
    $rows = [int][Math]::Ceiling($values.Count / [double]$columns)
    $sheet = [Drawing.Bitmap]::new($columns * $cellWidth,
        $rows * $cellHeight, [Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $graphics = [Drawing.Graphics]::FromImage($sheet)
    $font = [Drawing.Font]::new('Arial', 11, [Drawing.FontStyle]::Bold,
        [Drawing.GraphicsUnit]::Pixel)
    $labelBrush = [Drawing.SolidBrush]::new((Convert-HexColor '#E8D6A9'))
    $cellBrush = [Drawing.SolidBrush]::new((Convert-HexColor '#282B31'))
    $cellPen = [Drawing.Pen]::new((Convert-HexColor '#5C4A3C'), 1)
    try {
        $graphics.Clear((Convert-HexColor '#1C1E23'))
        $graphics.InterpolationMode =
            [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        for ($index = 0; $index -lt $values.Count; $index++) {
            $record = $values[$index]
            $column = $index % $columns
            $row = [int][Math]::Floor($index / $columns)
            $left = $column * $cellWidth
            $top = $row * $cellHeight
            $graphics.FillRectangle($cellBrush, $left + 4, $top + 4,
                $cellWidth - 8, $cellHeight - 8)
            $graphics.DrawRectangle($cellPen, $left + 4, $top + 4,
                $cellWidth - 8, $cellHeight - 8)
            $image = [Drawing.Image]::FromFile((Join-Path $root $record.finalPath))
            try {
                if ($image.Width -eq 64) {
                    $graphics.DrawImage($image, $left + 18, $top + 12, 128, 128)
                    $graphics.DrawImage($image, $left + 150, $top + 104, 32, 32)
                }
                else {
                    $graphics.DrawImage($image, $left + 30, $top + 10, 128, 128)
                }
                $graphics.DrawString([string]$record.key, $font, $labelBrush,
                    $left + 10, $top + 148)
            }
            finally { $image.Dispose() }
        }
        Save-Png $sheet $preview
    }
    finally {
        $graphics.Dispose(); $font.Dispose(); $labelBrush.Dispose()
        $cellBrush.Dispose()
        $cellPen.Dispose(); $sheet.Dispose()
    }
}

New-FirearmFeatMap

$records = [Collections.Generic.List[object]]::new()
foreach ($entry in $featEntries) {
    $records.Add((Get-ImageRecord $entry.Key (Join-Path $featSource ('sources/' + $entry.Key + '-source.png')) 'original-vector-path' 64)) | Out-Null
}
foreach ($entry in $itemEntries) {
    $records.Add((Get-ImageRecord $entry.Key (Join-Path $root $entry.Source) $entry.Kind 128)) | Out-Null
}

New-AssetPreview $records

$manifest = [ordered]@{
    schemaVersion = 1
    generator = 'tools/icon-art/New-IconOverhaulAssets.ps1'
    blenderSourceGenerator =
        'tools/icon-art/render_weapon_icon_sources.py'
    assetSet = 'complete-icon-overhaul'
    featSourceDimensions = @($sourceCanvas, $sourceCanvas)
    featRuntimeDimensions = @(64, 64)
    itemRuntimeDimensions = @(128, 128)
    selectorStyle = 'full-square burgundy-brown gradient; original gold path monogram'
    rapidReloadStyle =
        'transparent canvas; muted #A6533F enlarged circular arrow and tool; no blue corners'
    itemFit = 'alpha-bounds fit with 5px runtime safety margin'
    records = @($records)
    previewPath = $preview.Substring($root.Length + 1).Replace('\', '/')
    previewSha256 = (Get-FileHash -LiteralPath $preview -Algorithm SHA256).Hash.ToLowerInvariant()
}
[IO.File]::WriteAllText($manifestPath,
    ($manifest | ConvertTo-Json -Depth 8) + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))

$featHashLines = @($featEntries | ForEach-Object {
    $path = Join-Path $runtime ($_.Key + '.png')
    $hash = Get-FileHash -LiteralPath $path -Algorithm SHA256
    '{0}  ../../../assets/game/icons/{1}' -f
        $hash.Hash.ToLowerInvariant(), [IO.Path]::GetFileName($hash.Path)
})
$mapPath = Join-Path $featSource 'firearm-feat-icon-map.png'
$mapHash = Get-FileHash -LiteralPath $mapPath -Algorithm SHA256
$featHashLines += '{0}  {1}' -f
    $mapHash.Hash.ToLowerInvariant(), [IO.Path]::GetFileName($mapHash.Path)
[IO.File]::WriteAllLines((Join-Path $featSource 'SHA256SUMS.txt'),
    $featHashLines, [Text.UTF8Encoding]::new($false))

Write-Output ('Generated icon-overhaul assets: mode={0}; records={1}; preview={2}' -f
    $Mode, $records.Count, $preview)
