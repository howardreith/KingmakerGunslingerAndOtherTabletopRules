[CmdletBinding()]
param(
    [string]$SourceDirectory = (Join-Path $PSScriptRoot '..\assets-source\original-icons\supply-icons'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\assets\game\icons')
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$items = [ordered]@{
    'gunsmith-kit-chroma-source.png' = 'gunsmith-kit.png'
    'overhaul-kit-chroma-source.png' = 'overhaul-kit.png'
}

foreach ($entry in $items.GetEnumerator()) {
    $sourcePath = [IO.Path]::GetFullPath((Join-Path $SourceDirectory $entry.Key))
    $outputPath = [IO.Path]::GetFullPath((Join-Path $OutputDirectory $entry.Value))
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        throw "Supply icon source is missing: $sourcePath"
    }
    $source = [Drawing.Bitmap]::FromFile($sourcePath)
    try {
        $keyed = [Drawing.Bitmap]::new($source.Width, $source.Height,
            [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            for ($y = 0; $y -lt $source.Height; $y++) {
                for ($x = 0; $x -lt $source.Width; $x++) {
                    $pixel = $source.GetPixel($x, $y)
                    $other = [Math]::Max([int]$pixel.R, [int]$pixel.B)
                    $dominance = [int]$pixel.G - $other
                    $alpha = if ($pixel.G -gt 80 -and $dominance -gt 10) {
                        [Math]::Max(0, 255 - (($dominance - 10) * 8))
                    } else { 255 }
                    if ($alpha -eq 0) {
                        $keyed.SetPixel($x, $y, [Drawing.Color]::Transparent)
                    } else {
                        $green = [Math]::Min([int]$pixel.G, $other)
                        $keyed.SetPixel($x, $y, [Drawing.Color]::FromArgb(
                            $alpha, $pixel.R, $green, $pixel.B))
                    }
                }
            }

            $output = [Drawing.Bitmap]::new(128, 128,
                [Drawing.Imaging.PixelFormat]::Format32bppArgb)
            try {
                $graphics = [Drawing.Graphics]::FromImage($output)
                try {
                    $graphics.Clear([Drawing.Color]::Transparent)
                    $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
                    $graphics.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
                    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                    $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                    $graphics.DrawImage($keyed, [Drawing.Rectangle]::new(0, 0, 128, 128),
                        0, 0, $keyed.Width, $keyed.Height, [Drawing.GraphicsUnit]::Pixel)
                }
                finally { $graphics.Dispose() }
                $transparent = 0
                $greenFringe = 0
                for ($checkY = 0; $checkY -lt 128; $checkY++) {
                    for ($checkX = 0; $checkX -lt 128; $checkX++) {
                        $check = $output.GetPixel($checkX, $checkY)
                        if ($check.A -eq 0) { $transparent++ }
                        elseif ($check.A -ge 24 -and $check.G -gt 96 -and
                            $check.G -gt ([Math]::Max([int]$check.R,
                                [int]$check.B) + 32)) { $greenFringe++ }
                    }
                }
                if ($transparent -eq 0 -or $greenFringe -ne 0) {
                    throw "Supply icon transparency/despill validation failed: $($entry.Value); transparent=$transparent; greenFringe=$greenFringe"
                }
                New-Item -ItemType Directory -Force -Path (Split-Path $outputPath) | Out-Null
                $output.Save($outputPath, [Drawing.Imaging.ImageFormat]::Png)
            }
            finally { $output.Dispose() }
        }
        finally { $keyed.Dispose() }
    }
    finally { $source.Dispose() }
    Write-Host "Exported supply icon: $outputPath"
}
