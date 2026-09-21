[CmdletBinding()]
param([string]$RepositoryRoot)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path (Split-Path $PSScriptRoot) }
$RepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
Add-Type -AssemblyName System.Drawing
$sources = Join-Path $RepositoryRoot 'assets-source/original-icons/icon-overhaul-v2/production/sources'
$donorPath = Join-Path $sources 'scroll-of-word-of-recall.png'
if ((Get-FileHash -LiteralPath $donorPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne '512c572bb55d6c9b182f4ede08041dd15c8c0c4aaa8617e26766853d832bec90') {
    throw 'The reviewed Recall scroll shell source changed; inspect the new source before recomposing.'
}
# The three existing strategic scroll composites have pixel-identical shells.
# Their entire symbol difference is the 62x62 window at (32,31). Reuse those
# existing shell pixels directly, without reconstructing or repainting them.
$shell = [Drawing.Bitmap]::new($donorPath)
try {
    foreach ($sibling in @('scroll-of-teleport','scroll-of-greater-teleport')) {
        $other = [Drawing.Bitmap]::new((Join-Path $sources ($sibling + '.png')))
        try {
            if ($shell.Width -ne 128 -or $shell.Height -ne 128 -or $other.Size -ne $shell.Size) { throw 'Unexpected strategic scroll source geometry.' }
            for ($y=0; $y -lt 128; $y++) { for ($x=0; $x -lt 128; $x++) {
                if ($x -ge 32 -and $x -lt 94 -and $y -ge 31 -and $y -lt 93) { continue }
                if ($shell.GetPixel($x,$y) -ne $other.GetPixel($x,$y)) { throw "Strategic scroll shells differ outside the emblem window: $sibling ($x,$y)." }
            }}
        } finally { $other.Dispose() }
    }
    foreach ($alignment in @('evil','good','chaos','law')) {
        $painting = [Drawing.Bitmap]::new((Join-Path $sources ('magic-circle-against-' + $alignment + '.png')))
        $composite = [Drawing.Bitmap]::new(512,512,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [Drawing.Graphics]::FromImage($composite)
        $background = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(255,8,11,28))
        $attributes = [Drawing.Imaging.ImageAttributes]::new()
        try {
            # Exact 4x replication retains the native shell pixels in the source;
            # the existing production exporter owns the final 128px filtering.
            $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
            $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
            $attributes.SetWrapMode([Drawing.Drawing2D.WrapMode]::TileFlipXY)
            $graphics.DrawImage($shell,[Drawing.Rectangle]::new(0,0,512,512),0,0,128,128,[Drawing.GraphicsUnit]::Pixel,$attributes)
            $window = [Drawing.Rectangle]::new(128,124,248,248)
            # Remove the complete old emblem, including behind transparent new
            # pixels. No donor glyph can leak through the new painting.
            $graphics.FillRectangle($background,$window)
            $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceOver
            $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.DrawImage($painting,$window,0,0,$painting.Width,$painting.Height,[Drawing.GraphicsUnit]::Pixel,$attributes)
            $composite.Save((Join-Path $sources ('scroll-of-magic-circle-against-' + $alignment + '.png')),[Drawing.Imaging.ImageFormat]::Png)
        } finally { $attributes.Dispose(); $background.Dispose(); $graphics.Dispose(); $composite.Dispose(); $painting.Dispose() }
    }
} finally { $shell.Dispose() }
Write-Output 'Composed four Magic Circle scroll sources on the unchanged strategic scroll shell.'
