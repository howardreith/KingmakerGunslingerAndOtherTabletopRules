[CmdletBinding()]
param([string]$RepositoryRoot)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path (Split-Path $PSScriptRoot) }
Add-Type -AssemblyName System.Drawing
$pilotRoot = Join-Path $RepositoryRoot 'assets-source\original-icons\icon-overhaul-v2\pilot'
$sourceRoot = Join-Path $pilotRoot 'sources'
$exportRoot = Join-Path $pilotRoot 'exports'
$reviewRoot = Join-Path $RepositoryRoot 'reports\icon-overhaul'
New-Item -ItemType Directory -Path $sourceRoot,$exportRoot,$reviewRoot -Force | Out-Null
$keys = @('general-ifrit','fire-affinity','fire-resistance','elemental-strike',
    'hydraulic-maneuver','hydraulic-trip','teleport','greater-teleport','word-of-recall','rapid-reload')
function New-IconGraphics([Drawing.Bitmap]$bitmap) {
    $g = [Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
    return $g
}
function Draw-Scaled([Drawing.Graphics]$g, [Drawing.Image]$source, [Drawing.Rectangle]$rect) {
    $attrs = [Drawing.Imaging.ImageAttributes]::new()
    try {
        $attrs.SetWrapMode([Drawing.Drawing2D.WrapMode]::TileFlipXY)
        $g.DrawImage($source, $rect, 0, 0, $source.Width, $source.Height,
            [Drawing.GraphicsUnit]::Pixel, $attrs)
    } finally { $attrs.Dispose() }
}
# Original measured flat emblem; never a substitute for the requested paintings.
# Screenshot 10's neighboring combat emblems inform the ink and thin circular scaffold.
# The square gold frame and selected/disabled state remain UI decoration.
$rapid = [Drawing.Bitmap]::new(512,512,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$rg = New-IconGraphics $rapid
$ink = [Drawing.ColorTranslator]::FromHtml('#A6533F')
$pen = [Drawing.Pen]::new($ink, 3.0)
$detailPen = [Drawing.Pen]::new($ink, 2.5)
$brush = [Drawing.SolidBrush]::new($ink)
try {
    $rg.Clear([Drawing.Color]::Transparent)
    $rg.ScaleTransform(8,8)
    $pen.StartCap = $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $detailPen.StartCap = $detailPen.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $rg.DrawEllipse($pen,6.0,6.0,52.0,52.0)
    # Diagonal firearm stock and open barrel: a ball and ramrod enter the muzzle.
    # Keep the stock silhouette so the loading gesture cannot read as a plunger.
    $stock = [Drawing.PointF[]]@(
        [Drawing.PointF]::new(13,45),[Drawing.PointF]::new(18,50),
        [Drawing.PointF]::new(29,40),[Drawing.PointF]::new(24,35))
    $rg.FillPolygon($brush,$stock)
    $rg.DrawLine($detailPen,24.0,35.0,34.0,25.0)
    $rg.DrawLine($detailPen,29.0,40.0,39.0,30.0)
    $rg.DrawLine($detailPen,24.0,35.0,29.0,40.0)
    $rg.DrawLine($detailPen,32.0,25.0,35.0,28.0)
    $rg.DrawLine($detailPen,36.0,29.0,39.0,32.0)
    $rg.FillEllipse($brush,37.5,19.5,6.0,6.0)
    $rg.DrawLine($detailPen,44.0,18.0,48.0,14.0)
    $rg.DrawLine($detailPen,45.0,12.0,50.0,17.0)
    $rapid.Save((Join-Path $sourceRoot 'rapid-reload.png'),[Drawing.Imaging.ImageFormat]::Png)
} finally { $rg.Dispose(); $rapid.Dispose(); $pen.Dispose(); $detailPen.Dispose(); $brush.Dispose() }
$records = @()
foreach ($key in $keys) {
    $sourcePath = Join-Path $sourceRoot ($key + '.png')
    $source = [Drawing.Bitmap]::new($sourcePath)
    try {
        if ($source.Width -ne $source.Height) { throw "Non-square pilot source: $key" }
        $size = if ($key -eq 'rapid-reload') {64} else {128}
        $exportPath = Join-Path $exportRoot ($key + '.png')
        $target = [Drawing.Bitmap]::new($size,$size,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g = New-IconGraphics $target
        try {
            $g.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
            Draw-Scaled $g $source ([Drawing.Rectangle]::new(0,0,$size,$size))
            $target.Save($exportPath,[Drawing.Imaging.ImageFormat]::Png)
        } finally { $g.Dispose(); $target.Dispose() }
        $records += [ordered]@{
            key=$key
            source=('assets-source/original-icons/icon-overhaul-v2/pilot/sources/'+$key+'.png')
            sourceSha256=(Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant()
            sourceSize=@($source.Width,$source.Height)
            export=('assets-source/original-icons/icon-overhaul-v2/pilot/exports/'+$key+'.png')
            exportSha256=(Get-FileHash -LiteralPath $exportPath -Algorithm SHA256).Hash.ToLowerInvariant()
            exportSize=@($size,$size)
            technicalStatus='candidate-export'
            visualStatus='awaiting-owner-pilot-review'
            approvedHash=$null
        }
    } finally { $source.Dispose() }
}
$manifest = [ordered]@{
    schemaVersion=1
    authority='Candidate source/export record only; not runtime registration or visual approval.'
    exporter='tools/icon-art/Export-IconPilot.ps1'
    engine='Windows System.Drawing GDI+; HighQualityBicubic; TileFlipXY edges; SourceCopy ARGB32; no chroma key, sharpen, generation, or timestamps during painted export.'
    runtimeInstallation='none; isolated pilot exports are not packaged'
    records=$records
}
$utf8 = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText((Join-Path $pilotRoot 'pilot-manifest.json'),(($manifest | ConvertTo-Json -Depth 8)+[char]10),$utf8)
# Art-inspection contact sheet. These are drawing sizes, not native UI claims.
$sheet = [Drawing.Bitmap]::new(1056,1650,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$sg = New-IconGraphics $sheet
$titleFont = [Drawing.Font]::new('Segoe UI',20,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
$font = [Drawing.Font]::new('Segoe UI',15,[Drawing.FontStyle]::Regular,[Drawing.GraphicsUnit]::Pixel)
$small = [Drawing.Font]::new('Segoe UI',11,[Drawing.FontStyle]::Regular,[Drawing.GraphicsUnit]::Pixel)
$light = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#eadcc3'))
$muted = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#aeaca8'))
try {
    $sg.Clear([Drawing.ColorTranslator]::FromHtml('#171d20'))
    $sg.DrawString('ICON OVERHAUL / PILOT CANDIDATES', $titleFont,$light,24,18)
    $sg.DrawString('Art inspection only. No owner approval or native UI qualification.', $font,$muted,24,49)
    for ($i=0;$i -lt $records.Count;$i++) {
        $record=$records[$i]; $x=24+($i%3)*348; $y=92+[Math]::Floor($i/3)*384
        $bmp=[Drawing.Bitmap]::new((Join-Path $RepositoryRoot $record.export))
        try {
            $sg.DrawString($record.key,$font,$light,[single]$x,[single]$y)
            Draw-Scaled $sg $bmp ([Drawing.Rectangle]::new($x,$y+29,224,224))
            $px=$x
            foreach ($s in @(32,48,64)) {
                $bg=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#d9c8a7'))
                try { $sg.FillRectangle($bg,$px-2,$y+266,($s+4),68) } finally {$bg.Dispose()}
                Draw-Scaled $sg $bmp ([Drawing.Rectangle]::new($px,$y+268,$s,$s))
                $sg.DrawString(($s.ToString()+' px'),$small,$muted,[single]$px,[single]($y+340))
                $px += $s+18
            }
            $gray=[Drawing.Bitmap]::new(48,48,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $gg=New-IconGraphics $gray
            try { Draw-Scaled $gg $bmp ([Drawing.Rectangle]::new(0,0,48,48)) } finally {$gg.Dispose()}
            for ($a=0;$a -lt 48;$a++) { for ($b=0;$b -lt 48;$b++) {
                $c=$gray.GetPixel($a,$b); $l=[int](0.2126*$c.R+0.7152*$c.G+0.0722*$c.B)
                $gray.SetPixel($a,$b,[Drawing.Color]::FromArgb($c.A,$l,$l,$l))
            }}
            try { $sg.DrawImageUnscaled($gray,($x+224),($y+268)) } finally {$gray.Dispose()}
            $sg.DrawString('48 gray',$small,$muted,[single]($x+224),[single]($y+340))
        } finally {$bmp.Dispose()}
    }
    $sheet.Save((Join-Path $reviewRoot 'pilot-contact-sheet.png'),[Drawing.Imaging.ImageFormat]::Png)
} finally { $sg.Dispose();$sheet.Dispose();$titleFont.Dispose();$font.Dispose();$small.Dispose();$light.Dispose();$muted.Dispose() }
Write-Output 'Exported 10 isolated pilot candidates and the labeled art-inspection contact sheet.'
