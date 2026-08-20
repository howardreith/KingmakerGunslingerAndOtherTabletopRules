[CmdletBinding()]
param(
    [string]$SpecPath = (Join-Path $PSScriptRoot '..\assets-source\original-icons\firearm-feats\icon-spec.json'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\assets\game\icons'),
    [string]$SourceDirectory = (Join-Path $PSScriptRoot '..\assets-source\original-icons\firearm-feats')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$spec = Get-Content -LiteralPath $SpecPath -Raw | ConvertFrom-Json
if ($spec.version -ne 1 -or $spec.canvas -ne 64 -or @($spec.icons).Count -ne 6) {
    throw 'Firearm feat icon specification is incomplete.'
}
$fontFamily = [Drawing.FontFamily]::new([string]$spec.fontFamily)
if ($fontFamily.Name -cne [string]$spec.fontFamily) {
    throw "Required system font is unavailable: $($spec.fontFamily)"
}

function New-IconCanvas {
    return [Drawing.Bitmap]::new(64, 64,
        [Drawing.Imaging.PixelFormat]::Format32bppArgb)
}

function Initialize-Graphics([Drawing.Bitmap]$Bitmap) {
    $graphics = [Drawing.Graphics]::FromImage($Bitmap)
    $graphics.Clear([Drawing.Color]::Transparent)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    return $graphics
}

function Draw-AgedField([Drawing.Graphics]$Graphics, [int]$Seed, [bool]$Salmon) {
    $bounds = [Drawing.Rectangle]::new(3, 3, 58, 58)
    $top = if ($Salmon) { [Drawing.Color]::FromArgb(255, 145, 67, 62) } else {
        [Drawing.Color]::FromArgb(255, 126, 96, 58) }
    $bottom = if ($Salmon) { [Drawing.Color]::FromArgb(255, 76, 30, 30) } else {
        [Drawing.Color]::FromArgb(255, 52, 39, 28) }
    $field = [Drawing.Drawing2D.LinearGradientBrush]::new($bounds, $top, $bottom, 90.0)
    $outer = [Drawing.Pen]::new([Drawing.Color]::FromArgb(255, 43, 30, 24), 3.0)
    $middle = [Drawing.Pen]::new([Drawing.Color]::FromArgb(255, 169, 129, 72), 2.0)
    $inner = [Drawing.Pen]::new([Drawing.Color]::FromArgb(210, 230, 195, 129), 1.0)
    try {
        $Graphics.FillEllipse($field, $bounds)
        $Graphics.DrawEllipse($outer, [Drawing.Rectangle]::new(2, 2, 60, 60))
        $Graphics.DrawEllipse($middle, [Drawing.Rectangle]::new(5, 5, 54, 54))
        $Graphics.DrawEllipse($inner, [Drawing.Rectangle]::new(8, 8, 48, 48))
        $state = $Graphics.Save()
        $clip = [Drawing.Drawing2D.GraphicsPath]::new()
        try {
            $clip.AddEllipse([Drawing.Rectangle]::new(7, 7, 50, 50))
            $Graphics.SetClip($clip)
            $random = [Random]::new($Seed)
            $wearPen = [Drawing.Pen]::new([Drawing.Color]::FromArgb(38, 238, 211, 156), 1.0)
            $darkPen = [Drawing.Pen]::new([Drawing.Color]::FromArgb(32, 35, 24, 20), 1.0)
            try {
                for ($i = 0; $i -lt 34; $i++) {
                    $x = $random.Next(8, 56); $y = $random.Next(8, 56)
                    $length = $random.Next(1, 5)
                    $pen = if (($i % 3) -eq 0) { $wearPen } else { $darkPen }
                    $Graphics.DrawLine($pen, $x, $y, $x + $length, $y + ($i % 2))
                }
            }
            finally { $wearPen.Dispose(); $darkPen.Dispose() }
        }
        finally { $clip.Dispose(); $Graphics.Restore($state) }
    }
    finally { $field.Dispose(); $outer.Dispose(); $middle.Dispose(); $inner.Dispose() }
}

function Draw-Monogram([Drawing.Graphics]$Graphics, [string]$Text) {
    $size = if ($Text.Length -eq 1) { 35.0 } else { 25.0 }
    $path = [Drawing.Drawing2D.GraphicsPath]::new()
    $format = [Drawing.StringFormat]::GenericTypographic
    try {
        $path.AddString($Text, $fontFamily, [int][Drawing.FontStyle]::Bold,
            $size, [Drawing.PointF]::new(0, 0), $format)
        $bounds = $path.GetBounds()
        $matrix = [Drawing.Drawing2D.Matrix]::new()
        try {
            $matrix.Translate([single](32 - ($bounds.Left + $bounds.Width / 2)),
                [single](31 - ($bounds.Top + $bounds.Height / 2)))
            $path.Transform($matrix)
        }
        finally { $matrix.Dispose() }
        $shadow = [Drawing.Pen]::new([Drawing.Color]::FromArgb(230, 44, 27, 22), 3.2)
        $edge = [Drawing.Pen]::new([Drawing.Color]::FromArgb(255, 181, 132, 70), 1.1)
        $fill = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(255, 236, 211, 157))
        try {
            $Graphics.DrawPath($shadow, $path)
            $Graphics.FillPath($fill, $path)
            $Graphics.DrawPath($edge, $path)
        }
        finally { $shadow.Dispose(); $edge.Dispose(); $fill.Dispose() }
    }
    finally { $path.Dispose(); $format.Dispose() }
}

function Draw-RapidReload([Drawing.Graphics]$Graphics) {
    $cream = [Drawing.Color]::FromArgb(255, 237, 208, 150)
    $shadow = [Drawing.Color]::FromArgb(235, 48, 25, 24)
    $shadowPen = [Drawing.Pen]::new($shadow, 5.0)
    $linePen = [Drawing.Pen]::new($cream, 2.6)
    try {
        $shadowPen.StartCap = $shadowPen.EndCap = [Drawing.Drawing2D.LineCap]::Round
        $linePen.StartCap = $linePen.EndCap = [Drawing.Drawing2D.LineCap]::Round
        $Graphics.DrawArc($shadowPen, 13, 12, 38, 38, 205, 255)
        $Graphics.DrawArc($linePen, 13, 12, 38, 38, 205, 255)
        $arrowShadow = @([Drawing.PointF]::new(16, 14), [Drawing.PointF]::new(27, 14), [Drawing.PointF]::new(20, 23))
        $arrow = @([Drawing.PointF]::new(17, 15), [Drawing.PointF]::new(25, 15), [Drawing.PointF]::new(20, 21))
        $shadowBrush = [Drawing.SolidBrush]::new($shadow)
        $creamBrush = [Drawing.SolidBrush]::new($cream)
        try { $Graphics.FillPolygon($shadowBrush, $arrowShadow); $Graphics.FillPolygon($creamBrush, $arrow) }
        finally { $shadowBrush.Dispose(); $creamBrush.Dispose() }
        $Graphics.DrawLine($shadowPen, 20, 48, 43, 21)
        $Graphics.DrawLine($linePen, 20, 48, 43, 21)
        $Graphics.DrawLine($shadowPen, 17, 45, 24, 51)
        $Graphics.DrawLine($linePen, 17, 45, 24, 51)
        $Graphics.DrawLine($shadowPen, 40, 19, 46, 24)
        $Graphics.DrawLine($linePen, 40, 19, 46, 24)
    }
    finally { $shadowPen.Dispose(); $linePen.Dispose() }
}

$destination = [IO.Path]::GetFullPath($OutputDirectory)
$sourceDestination = [IO.Path]::GetFullPath($SourceDirectory)
New-Item -ItemType Directory -Force -Path $destination,$sourceDestination | Out-Null
$rendered = @()
foreach ($icon in $spec.icons) {
    $bitmap = New-IconCanvas
    try {
        $graphics = Initialize-Graphics $bitmap
        try {
            $isReload = [string]$icon.key -ceq 'rapid-reload'
            Draw-AgedField $graphics ([int]$icon.seed) $isReload
            if ($isReload) { Draw-RapidReload $graphics }
            else { Draw-Monogram $graphics ([string]$icon.monogram) }
        }
        finally { $graphics.Dispose() }
        $path = Join-Path $destination ($icon.key + '.png')
        $bitmap.Save($path, [Drawing.Imaging.ImageFormat]::Png)
        $rendered += [pscustomobject]@{ Key = [string]$icon.key; Path = $path }
    }
    finally { $bitmap.Dispose() }
}

$map = [Drawing.Bitmap]::new(480, 126, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    $graphics = Initialize-Graphics $map
    try {
        $graphics.Clear([Drawing.Color]::FromArgb(255, 37, 29, 25))
        $labelFont = [Drawing.Font]::new($fontFamily, 9.0, [Drawing.FontStyle]::Regular,
            [Drawing.GraphicsUnit]::Pixel)
        $labelBrush = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(255, 224, 205, 168))
        try {
            for ($i = 0; $i -lt $rendered.Count; $i++) {
                $icon = [Drawing.Bitmap]::FromFile($rendered[$i].Path)
                try {
                    $x = 8 + ($i * 78)
                    $graphics.DrawImage($icon, $x + 7, 7, 64, 64)
                    $graphics.DrawImage($icon, $x + 23, 76, 32, 32)
                    $graphics.DrawString($rendered[$i].Key.Replace('firearm-monogram-', ''),
                        $labelFont, $labelBrush, [single]$x, [single]108)
                }
                finally { $icon.Dispose() }
            }
        }
        finally { $labelFont.Dispose(); $labelBrush.Dispose() }
    }
    finally { $graphics.Dispose() }
    $map.Save((Join-Path $sourceDestination 'firearm-feat-icon-map.png'),
        [Drawing.Imaging.ImageFormat]::Png)
}
finally { $map.Dispose(); $fontFamily.Dispose() }

$sumLines = $rendered | ForEach-Object {
    $hash = (Get-FileHash -LiteralPath $_.Path -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  ../../../assets/game/icons/$($_.Key).png"
}
$mapPath = Join-Path $sourceDestination 'firearm-feat-icon-map.png'
$sumLines += "$((Get-FileHash -LiteralPath $mapPath -Algorithm SHA256).Hash.ToLowerInvariant())  firearm-feat-icon-map.png"
[IO.File]::WriteAllLines((Join-Path $sourceDestination 'SHA256SUMS.txt'), $sumLines,
    [Text.UTF8Encoding]::new($false))
$sumLines | ForEach-Object { Write-Host $_ }
