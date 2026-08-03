[CmdletBinding()]
param(
    [string]$SourcePath = (Join-Path $PSScriptRoot '..\assets-source\original-icons\fourth-playtest\rapid-reload-chroma-source.png'),
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\assets\game\icons\rapid-reload.png')
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$source = [Drawing.Bitmap]::FromFile([IO.Path]::GetFullPath($SourcePath))
try {
    $keyed = [Drawing.Bitmap]::new($source.Width, $source.Height,
        [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $left = $source.Width; $top = $source.Height; $right = -1; $bottom = -1
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                $other = [Math]::Max([int]$pixel.R, [int]$pixel.B)
                $dominance = [int]$pixel.G - $other
                $alpha = if ($pixel.G -gt 80 -and $dominance -gt 10) {
                    [Math]::Max(0, 255 - (($dominance - 10) * 8))
                } else { 255 }
                if ($alpha -gt 0) {
                    $green = [Math]::Min([int]$pixel.G, $other)
                    $keyed.SetPixel($x, $y, [Drawing.Color]::FromArgb(
                        $alpha, $pixel.R, $green, $pixel.B))
                    if ($alpha -ge 24) {
                        $left = [Math]::Min($left, $x); $top = [Math]::Min($top, $y)
                        $right = [Math]::Max($right, $x); $bottom = [Math]::Max($bottom, $y)
                    }
                }
            }
        }
        if ($right -lt $left -or $bottom -lt $top) {
            throw 'The chroma source contained no non-key icon pixels.'
        }

        $width = $right - $left + 1; $height = $bottom - $top + 1
        $side = [Math]::Max($width, $height)
        $cropX = [Math]::Max(0, $left - [Math]::Floor(($side - $width) / 2))
        $cropY = [Math]::Max(0, $top - [Math]::Floor(($side - $height) / 2))
        $side = [Math]::Min($side, [Math]::Min(
            $keyed.Width - $cropX, $keyed.Height - $cropY))
        $output = [Drawing.Bitmap]::new(64, 64,
            [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $graphics = [Drawing.Graphics]::FromImage($output)
            try {
                $graphics.Clear([Drawing.Color]::Transparent)
                $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
                $graphics.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
                $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $graphics.DrawImage($keyed, [Drawing.Rectangle]::new(0, 0, 64, 64),
                    [single]$cropX, [single]$cropY, [single]$side, [single]$side,
                    [Drawing.GraphicsUnit]::Pixel)
            }
            finally { $graphics.Dispose() }
            $destination = [IO.Path]::GetFullPath($OutputPath)
            New-Item -ItemType Directory -Force -Path (Split-Path $destination) | Out-Null
            $output.Save($destination, [Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $output.Dispose() }
    }
    finally { $keyed.Dispose() }
}
finally { $source.Dispose() }

Write-Host "Exported Rapid Reload icon: $([IO.Path]::GetFullPath($OutputPath))"
