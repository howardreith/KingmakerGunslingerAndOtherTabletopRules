[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root 'assets-source/original-icons/firearm-feats'
$runtime = Join-Path $root 'assets/game/icons'
$spec = Get-Content (Join-Path $source 'icon-spec.json') -Raw | ConvertFrom-Json
if ($spec.schemaVersion -ne 2 -or $spec.canvas -ne 64 -or
    @($spec.monograms).Count -ne 5 -or $null -eq $spec.rapidReload) {
    throw 'Firearm feat icon specification is incomplete.'
}
$fontFamily = [Drawing.FontFamily]::new('Segoe Script')
if ($fontFamily.Name -cne 'Segoe Script') {
    throw 'Required Windows Segoe Script system font is unavailable.'
}

function New-Graphics([Drawing.Bitmap]$bitmap) {
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode =
        [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.TextRenderingHint =
        [Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $graphics.Clear([Drawing.Color]::Transparent)
    return $graphics
}

function New-FieldPath {
    $path = [Drawing.Drawing2D.GraphicsPath]::new()
    $path.AddArc(3, 3, 14, 14, 180, 90)
    $path.AddArc(47, 3, 14, 14, 270, 90)
    $path.AddArc(47, 47, 14, 14, 0, 90)
    $path.AddArc(3, 47, 14, 14, 90, 90)
    $path.CloseFigure()
    return $path
}

function Draw-NativeParameterField(
    [Drawing.Graphics]$graphics,
    [int]$seed) {
    $path = New-FieldPath
    $fill = [Drawing.Drawing2D.LinearGradientBrush]::new(
        [Drawing.PointF]::new(4, 3),
        [Drawing.PointF]::new(60, 61),
        [Drawing.ColorTranslator]::FromHtml($spec.palette.parchmentLight),
        [Drawing.ColorTranslator]::FromHtml($spec.palette.parchmentShade))
    $border = [Drawing.Pen]::new(
        [Drawing.ColorTranslator]::FromHtml($spec.palette.border), 2.1)
    try {
        $graphics.FillPath($fill, $path)
        $graphics.DrawPath($border, $path)
    }
    finally {
        $path.Dispose()
        $fill.Dispose()
        $border.Dispose()
    }

    $random = [Random]::new($seed)
    for ($index = 0; $index -lt 58; $index++) {
        $tone = if (($index % 3) -eq 0) { 87 } else { 126 }
        $wear = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(
            8 + $random.Next(13), $tone, 65, 42))
        try {
            $x = 7 + $random.NextDouble() * 50
            $y = 7 + $random.NextDouble() * 50
            $radius = 0.35 + $random.NextDouble() * 0.8
            $graphics.FillEllipse($wear, $x, $y, $radius, $radius)
        }
        finally { $wear.Dispose() }
    }

    $corner = [Drawing.Pen]::new(
        [Drawing.ColorTranslator]::FromHtml($spec.palette.cornerBlue), 2.2)
    try {
        $graphics.DrawLine($corner, 7, 17, 7, 8)
        $graphics.DrawLine($corner, 7, 8, 16, 8)
        $graphics.DrawLine($corner, 57, 47, 57, 56)
        $graphics.DrawLine($corner, 57, 56, 48, 56)
    }
    finally { $corner.Dispose() }
}

function Draw-CalligraphicMonogram(
    [Drawing.Graphics]$graphics,
    [string]$text) {
    $size = if ($text.Length -eq 1) { 35 } else { 27 }
    $font = [Drawing.Font]::new(
        $fontFamily,
        $size,
        ([Drawing.FontStyle]::Bold -bor [Drawing.FontStyle]::Italic),
        [Drawing.GraphicsUnit]::Pixel)
    $format = [Drawing.StringFormat]::new()
    $format.Alignment = [Drawing.StringAlignment]::Center
    $format.LineAlignment = [Drawing.StringAlignment]::Center
    $shadow = [Drawing.SolidBrush]::new(
        [Drawing.Color]::FromArgb(72, 68, 38, 24))
    $ink = [Drawing.SolidBrush]::new(
        [Drawing.ColorTranslator]::FromHtml($spec.palette.oxblood))
    try {
        $graphics.DrawString($text, $font, $shadow,
            [Drawing.RectangleF]::new(5.8, 4.1, 54, 54), $format)
        $graphics.DrawString($text, $font, $ink,
            [Drawing.RectangleF]::new(5, 3, 54, 54), $format)
        $flourish = [Drawing.Pen]::new(
            [Drawing.ColorTranslator]::FromHtml($spec.palette.oxblood), 1.7)
        try {
            $graphics.DrawBezier($flourish, 17, 50, 26, 55, 39, 44, 49, 49)
        }
        finally { $flourish.Dispose() }
    }
    finally {
        $font.Dispose()
        $format.Dispose()
        $shadow.Dispose()
        $ink.Dispose()
    }
}

function Draw-RapidReloadGlyph([Drawing.Graphics]$graphics) {
    $ink = [Drawing.ColorTranslator]::FromHtml($spec.palette.oxblood)
    $pen = [Drawing.Pen]::new($ink, 4)
    $pen.StartCap = [Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
    try {
        $graphics.DrawArc($pen, 14, 13, 36, 36, 210, 270)
        $brush = [Drawing.SolidBrush]::new($ink)
        try {
            $graphics.FillPolygon($brush, @(
                [Drawing.PointF]::new(12, 16),
                [Drawing.PointF]::new(23, 16),
                [Drawing.PointF]::new(17, 25)))
        }
        finally { $brush.Dispose() }
        $graphics.DrawLine($pen, 25, 37, 43, 24)
        $graphics.DrawLine($pen, 39, 22, 46, 29)
    }
    finally { $pen.Dispose() }
}

function Save-Icon($entry, [bool]$rapid) {
    $bitmap = [Drawing.Bitmap]::new(
        64, 64, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = New-Graphics $bitmap
    try {
        Draw-NativeParameterField $graphics ([int]$entry.seed)
        if ($rapid) {
            Draw-RapidReloadGlyph $graphics
        }
        else {
            Draw-CalligraphicMonogram $graphics ([string]$entry.monogram)
        }
        $bitmap.Save(
            (Join-Path $runtime ($entry.key + '.png')),
            [Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

foreach ($entry in $spec.monograms) {
    Save-Icon $entry $false
}
Save-Icon $spec.rapidReload $true

$entries = @($spec.monograms) + @($spec.rapidReload)
$sheet = [Drawing.Bitmap]::new(
    480, 126, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [Drawing.Graphics]::FromImage($sheet)
try {
    $graphics.Clear([Drawing.Color]::FromArgb(255, 224, 211, 179))
    $labelFont = [Drawing.Font]::new(
        'Georgia', 8, [Drawing.FontStyle]::Bold, [Drawing.GraphicsUnit]::Pixel)
    $labelBrush = [Drawing.SolidBrush]::new(
        [Drawing.ColorTranslator]::FromHtml($spec.palette.border))
    try {
        for ($index = 0; $index -lt $entries.Count; $index++) {
            $entry = $entries[$index]
            $image = [Drawing.Image]::FromFile(
                (Join-Path $runtime ($entry.key + '.png')))
            try {
                $x = 8 + $index * 78
                $graphics.DrawImage($image, $x, 4, 64, 64)
                $graphics.DrawImage($image, $x + 16, 86, 32, 32)
                $graphics.DrawString(
                    [string]$entry.label, $labelFont, $labelBrush, $x, 72)
            }
            finally { $image.Dispose() }
        }
    }
    finally {
        $labelFont.Dispose()
        $labelBrush.Dispose()
    }
    $sheet.Save(
        (Join-Path $source 'firearm-feat-icon-map.png'),
        [Drawing.Imaging.ImageFormat]::Png)
}
finally {
    $graphics.Dispose()
    $sheet.Dispose()
    $fontFamily.Dispose()
}

$hashLines = @($entries | ForEach-Object {
    $path = Join-Path $runtime ($_.key + '.png')
    $hash = Get-FileHash $path -Algorithm SHA256
    '{0}  ../../../assets/game/icons/{1}' -f
        $hash.Hash.ToLowerInvariant(), [IO.Path]::GetFileName($hash.Path)
})
$mapPath = Join-Path $source 'firearm-feat-icon-map.png'
$mapHash = Get-FileHash $mapPath -Algorithm SHA256
$hashLines += '{0}  {1}' -f
    $mapHash.Hash.ToLowerInvariant(), [IO.Path]::GetFileName($mapHash.Path)
[IO.File]::WriteAllLines(
    (Join-Path $source 'SHA256SUMS.txt'),
    $hashLines,
    [Text.UTF8Encoding]::new($false))
Write-Output ('Generated {0} Nodachi-selector-style icons.' -f $entries.Count)
