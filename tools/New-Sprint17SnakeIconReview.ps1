[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$manifest=Get-Content -LiteralPath (Join-Path $root 'assets-source/original-icons/expanded-summoning/icon-manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
Add-Type -AssemblyName System.Drawing
$sheet=[Drawing.Bitmap]::new(640,300,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics=[Drawing.Graphics]::FromImage($sheet)
$font=[Drawing.Font]::new('Segoe UI',12,[Drawing.FontStyle]::Regular,[Drawing.GraphicsUnit]::Pixel)
$brush=[Drawing.SolidBrush]::new([Drawing.Color]::White)
try {
    $graphics.Clear([Drawing.Color]::FromArgb(24,20,18))
    $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.DrawString('Sprint 17: source review only; native UI and owner review pending',$font,$brush,8,6)
    $index=0
    foreach ($key in @('viper','constrictor-snake')) {
        $rows=@($manifest.icons | Where-Object key -CEQ $key)
        if ($rows.Count -ne 1) { throw "Missing unique icon $key" }
        $row=$rows[0]
        $path=Join-Path $root $row.productionFile
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $row.outputSha256) { throw "Icon hash mismatch: $key" }
        $source=[Drawing.Bitmap]::new($path)
        try {
            $x=8+$index*320
            $graphics.DrawString($key,$font,$brush,$x,30)
            $graphics.DrawImageUnscaled($source,$x,50)
            $cellX=$x
            foreach ($size in @(32,48,64)) {
                $graphics.DrawImage($source,[Drawing.Rectangle]::new($cellX,198,$size,$size))
                $graphics.DrawString([string]$size,$font,$brush,$cellX,274)
                $cellX+=$size+12
            }
            $gray=[Drawing.Bitmap]::new(48,48,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $gg=[Drawing.Graphics]::FromImage($gray)
            try { $gg.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic; $gg.DrawImage($source,0,0,48,48) } finally { $gg.Dispose() }
            try {
                for ($a=0;$a -lt 48;$a++) { for ($b=0;$b -lt 48;$b++) {
                    $c=$gray.GetPixel($a,$b)
                    $v=[int](0.2126*$c.R+0.7152*$c.G+0.0722*$c.B)
                    $gray.SetPixel($a,$b,[Drawing.Color]::FromArgb($c.A,$v,$v,$v))
                }}
                $graphics.DrawImageUnscaled($gray,$x+220,198)
                $graphics.DrawString('gray48',$font,$brush,$x+220,274)
            } finally { $gray.Dispose() }
        } finally { $source.Dispose() }
        $index++
    }
    $output=Join-Path $root 'artifacts/sprint17-snake-icons-review.png'
    $sheet.Save($output,[Drawing.Imaging.ImageFormat]::Png)
    Write-Output $output
} finally { $graphics.Dispose();$sheet.Dispose();$font.Dispose();$brush.Dispose() }
