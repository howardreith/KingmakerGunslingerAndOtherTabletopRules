[CmdletBinding()]
param([string]$OutputPath = (Join-Path $PSScriptRoot '..\assets\game\icons\rapid-reload.png'))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$bitmap = [Drawing.Bitmap]::new(64, 64, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([Drawing.Color]::Transparent)
$dark = [Drawing.Color]::FromArgb(255,54,31,28)
$salmon = [Drawing.Color]::FromArgb(255,151,70,67)
$salmon2 = [Drawing.Color]::FromArgb(255,103,45,45)
$gold = [Drawing.Color]::FromArgb(255,218,177,91)
$cream = [Drawing.Color]::FromArgb(255,239,215,164)
$graphics.FillEllipse((New-Object Drawing.SolidBrush $salmon2),3,3,58,58)
$graphics.FillEllipse((New-Object Drawing.SolidBrush $salmon),7,7,50,50)
$graphics.DrawEllipse((New-Object Drawing.Pen $dark,3),4,4,56,56)
$graphics.DrawArc((New-Object Drawing.Pen $cream,5),12,10,40,40,205,250)
$arrow = @([Drawing.PointF]::new(49,9),[Drawing.PointF]::new(55,23),[Drawing.PointF]::new(40,20))
$graphics.FillPolygon((New-Object Drawing.SolidBrush $cream),$arrow)
$pen = New-Object Drawing.Pen $dark,7
$pen.StartCap = $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
$graphics.DrawLine($pen,19,47,44,17)
$pen2 = New-Object Drawing.Pen $gold,4
$pen2.StartCap = $pen2.EndCap = [Drawing.Drawing2D.LineCap]::Round
$graphics.DrawLine($pen2,19,47,44,17)
$graphics.FillEllipse((New-Object Drawing.SolidBrush $cream),14,43,9,9)
$graphics.FillEllipse((New-Object Drawing.SolidBrush $cream),40,13,9,9)
$directory = Split-Path ([IO.Path]::GetFullPath($OutputPath))
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$stream = [IO.MemoryStream]::new()
$bitmap.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
[IO.File]::WriteAllBytes([IO.Path]::GetFullPath($OutputPath), $stream.ToArray())
$stream.Dispose()
$graphics.Dispose(); $bitmap.Dispose()
