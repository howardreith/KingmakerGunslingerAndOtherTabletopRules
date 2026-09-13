[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][ValidatePattern('^[a-z][a-z0-9-]+$')][string]$Name,
    [Parameter(Mandatory=$true)][string]$Keys,
    [string]$RepositoryRoot
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path (Split-Path $PSScriptRoot) }
$assets = @{}
foreach ($family in @('pilot','production')) {
    $path = Join-Path $RepositoryRoot "assets-source/original-icons/icon-overhaul-v2/$family/$family-manifest.json"
    $manifest = Get-Content -Raw -Encoding UTF8 -LiteralPath $path | ConvertFrom-Json
    foreach ($record in $manifest.records) { $assets[$record.key] = $record }
}
$selected = @($Keys.Split(',') | ForEach-Object {
    $key = $_.Trim()
    if (-not $assets.ContainsKey($key)) { throw "No exported art for $key" }
    $assets[$key]
})
Add-Type -AssemblyName System.Drawing
$height = 88 + [int]([Math]::Ceiling($selected.Count/3.0)*320)
$sheet = [Drawing.Bitmap]::new(1056,$height,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [Drawing.Graphics]::FromImage($sheet)
$g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$font = [Drawing.Font]::new('Segoe UI',15,[Drawing.FontStyle]::Regular,[Drawing.GraphicsUnit]::Pixel)
$title = [Drawing.Font]::new('Segoe UI',22,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
$white = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#f0e4cc'))
$light = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#d9c8a7'))
try {
    $g.Clear([Drawing.ColorTranslator]::FromHtml('#171d20'))
    $g.DrawString(('ICON PRODUCTION / '+$Name.ToUpperInvariant()),$title,$white,24,14)
    $g.DrawString('Art inspection: 160 / 32 / 48 / 64 px, plus 48 px grayscale. Native UI and final owner review pending.',$font,$white,24,47)
    for ($i=0; $i -lt $selected.Count; $i++) {
        $record = $selected[$i]; $x = 24+($i%3)*348; $y = 90+[int][Math]::Floor($i/3)*320
        $path = Join-Path $RepositoryRoot $record.export
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $record.exportSha256) { throw "Export changed: $($record.key)" }
        $bmp = [Drawing.Bitmap]::new($path)
        try {
            $g.DrawString($record.key,$font,$white,[single]$x,[single]$y)
            $g.DrawImage($bmp,[Drawing.Rectangle]::new($x,$y+25,160,160))
            $px = $x
            foreach ($s in @(32,48,64)) {
                $g.FillRectangle($light,$px-2,$y+198,$s+4,68)
                $g.DrawImage($bmp,[Drawing.Rectangle]::new($px,$y+200,$s,$s))
                $px += $s+16
            }
            $gray = [Drawing.Bitmap]::new(48,48,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $gg = [Drawing.Graphics]::FromImage($gray)
            try { $gg.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic; $gg.DrawImage($bmp,0,0,48,48) } finally { $gg.Dispose() }
            for ($a=0;$a -lt 48;$a++) { for ($b=0;$b -lt 48;$b++) {
                $c=$gray.GetPixel($a,$b); $v=[int](0.2126*$c.R+0.7152*$c.G+0.0722*$c.B)
                $gray.SetPixel($a,$b,[Drawing.Color]::FromArgb($c.A,$v,$v,$v))
            }}
            try { $g.DrawImageUnscaled($gray,$x+222,$y+200) } finally { $gray.Dispose() }
            $g.DrawString('32        48            64          gray',$font,$white,[single]$x,[single]($y+272))
        } finally { $bmp.Dispose() }
    }
    $destination = Join-Path $RepositoryRoot ('reports/icon-overhaul/production-'+$Name+'.png')
    $sheet.Save($destination,[Drawing.Imaging.ImageFormat]::Png)
    Write-Output $destination
} finally { $g.Dispose();$sheet.Dispose();$font.Dispose();$title.Dispose();$white.Dispose();$light.Dispose() }
