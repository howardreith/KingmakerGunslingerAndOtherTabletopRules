[CmdletBinding()]
param([string]$RepositoryRoot)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path (Split-Path $PSScriptRoot) }
$RepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
$sourceRoot = Join-Path $RepositoryRoot 'assets-source/original-icons/icon-overhaul-v2/production/sources'
$keys = @('crocodilian-sprint', 'crocodilian-death-roll', 'dire-crocodile-swallowed')
$catalog = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'assets-source/original-icons/icon-catalog.json') -Raw | ConvertFrom-Json
foreach ($concept in $catalog.concepts | Where-Object { $_.key -cin $keys }) {
    if ($concept.visualReview.status -ceq 'approved') { throw "Approved original is immutable: $($concept.key)" }
}
Add-Type -AssemblyName System.Drawing
function Fill-Shape([Drawing.Graphics]$canvas, [Drawing.Brush]$brush, [single[]]$coordinates) {
    $points = for ($index = 0; $index -lt $coordinates.Length; $index += 2) {
        [Drawing.PointF]::new($coordinates[$index], $coordinates[$index + 1])
    }
    $canvas.FillPolygon($brush, [Drawing.PointF[]]$points)
}
foreach ($key in $keys) {
    $bitmap = [Drawing.Bitmap]::new(512,512,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [Drawing.Graphics]::FromImage($bitmap)
    $ink = [Drawing.ColorTranslator]::FromHtml('#A6533F')
    $brush = [Drawing.SolidBrush]::new($ink)
    $pen = [Drawing.Pen]::new($ink,3.0)
    $thin = [Drawing.Pen]::new($ink,2.5)
    $clear = [Drawing.SolidBrush]::new([Drawing.Color]::Transparent)
    try {
        $g.Clear([Drawing.Color]::Transparent)
        $g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $g.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
        $g.ScaleTransform(8,8)
        $pen.StartCap = $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
        $thin.StartCap = $thin.EndCap = [Drawing.Drawing2D.LineCap]::Round
        # Same measured ink and scaffold as the approved Rapid Reload pilot.
        # Original gestures only; no reference pixels, fonts, UI frame or state.
        $g.DrawEllipse($pen,6.0,6.0,52.0,52.0)
        if ($key -ceq 'crocodilian-sprint') {
            Fill-Shape $g $brush @(11,23, 21,28, 26,27, 35,28, 39,26, 45,27,
                46,29, 53,29, 53,34, 43,35, 38,34, 27,35, 23,33, 17,30)
            Fill-Shape $g $brush @(25,27, 26,23, 29,27, 31,24, 34,28)
            $g.DrawLine($pen,26.0,34.0,21.0,41.0)
            $g.DrawLine($pen,21.0,41.0,16.0,41.0)
            $g.DrawLine($pen,37.0,34.0,42.0,40.0)
            $g.DrawLine($pen,42.0,40.0,48.0,40.0)
            $g.DrawLine($thin,27.0,29.0,24.0,22.0)
            $g.DrawLine($thin,37.0,29.0,41.0,22.0)
            $g.DrawLine($thin,13.0,35.0,19.0,35.0)
            $g.DrawLine($thin,24.0,46.0,34.0,46.0)
            $g.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $g.FillEllipse($clear,43.0,29.0,2.3,2.3)
        } elseif ($key -ceq 'crocodilian-death-roll') {
            # A long jaw clamps one plain bar, with a circular rolling arrow.
            Fill-Shape $g $brush @(17,23, 22,20, 30,23, 34,27, 47,27, 50,31,
                49,34, 28,32, 23,29, 17,30)
            Fill-Shape $g $brush @(20,34, 28,35, 45,36, 43,40, 26,39, 21,38)
            $g.DrawLine($thin,37.0,31.0,35.0,38.0)
            $g.DrawArc($pen,14.0,14.0,36.0,36.0,32.0,237.0)
            Fill-Shape $g $brush @(32,10, 38,15, 31,19)
            $g.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $g.FillEllipse($clear,27.0,26.0,2.8,2.8)
        } else {
            # An enclosed prey silhouette, not an attackable stomach or sword.
            Fill-Shape $g $brush @(14,17, 31,17, 38,20, 49,20, 51,24, 50,28,
                44,27, 42,31, 39,27, 34,27, 31,31, 28,27, 21,27, 15,30)
            Fill-Shape $g $brush @(15,37, 22,42, 29,43, 32,39, 35,43, 41,43,
                44,39, 47,42, 51,37, 52,43, 46,48, 24,49, 16,45)
            $g.FillEllipse($brush,31.0,32.0,4.0,4.0)
            $g.DrawLine($thin,31.0,35.0,28.0,39.0)
            $g.DrawLine($thin,28.0,39.0,25.0,37.0)
            $g.DrawLine($thin,29.0,37.0,35.0,39.0)
            $g.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $g.FillEllipse($clear,28.0,20.0,3.0,3.0)
        }
        $bitmap.Save((Join-Path $sourceRoot ($key + '.png')),[Drawing.Imaging.ImageFormat]::Png)
    } finally {
        $g.Dispose(); $bitmap.Dispose(); $brush.Dispose(); $pen.Dispose(); $thin.Dispose(); $clear.Dispose()
    }
}
Write-Output 'Created three original 512px crocodilian emblem sources; export and approval are separate.'
