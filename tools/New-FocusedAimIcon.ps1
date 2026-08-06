[CmdletBinding()]
param([string]$Source,[string]$Output)
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop';Add-Type -AssemblyName System.Drawing
if(-not $Source){$Source=Join-Path $PSScriptRoot '..\assets-source\original-icons\mysterious-stranger\focused-aim-chroma-source.png'}
if(-not $Output){$Output=Join-Path $PSScriptRoot '..\assets\game\icons\focused-aim.png'}
$sourcePath=[IO.Path]::GetFullPath($Source);$outputPath=[IO.Path]::GetFullPath($Output)
if(-not(Test-Path -LiteralPath $sourcePath -PathType Leaf)){throw "Missing source: $sourcePath"}
$input=[Drawing.Bitmap]::FromFile($sourcePath)
try{$keyed=[Drawing.Bitmap]::new($input.Width,$input.Height,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
try{for($y=0;$y-lt$input.Height;$y++){for($x=0;$x-lt$input.Width;$x++){$p=$input.GetPixel($x,$y);$other=[Math]::Max([int]$p.R,[int]$p.B);$dominance=[int]$p.G-$other;$alpha=if($p.G-gt 80-and$dominance-gt 10){[Math]::Max(0,255-(($dominance-10)*8))}else{255};if($alpha-eq 0){$keyed.SetPixel($x,$y,[Drawing.Color]::Transparent)}else{$green=[Math]::Min([int]$p.G,$other);$keyed.SetPixel($x,$y,[Drawing.Color]::FromArgb($alpha,$p.R,$green,$p.B))}}}
$out=[Drawing.Bitmap]::new(128,128,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
try{$g=[Drawing.Graphics]::FromImage($out);try{$g.Clear([Drawing.Color]::Transparent);$g.CompositingMode=[Drawing.Drawing2D.CompositingMode]::SourceCopy;$g.CompositingQuality=[Drawing.Drawing2D.CompositingQuality]::HighQuality;$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic;$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::HighQuality;$g.DrawImage($keyed,[Drawing.Rectangle]::new(0,0,128,128),0,0,$keyed.Width,$keyed.Height,[Drawing.GraphicsUnit]::Pixel)}finally{$g.Dispose()};if($out.GetPixel(0,0).A-ne 0){throw 'Focused Aim export corner is not transparent.'};New-Item -ItemType Directory -Force -Path(Split-Path $outputPath)|Out-Null;$out.Save($outputPath,[Drawing.Imaging.ImageFormat]::Png)}finally{$out.Dispose()}}finally{$keyed.Dispose()}}finally{$input.Dispose()}
Write-Host "Exported Focused Aim icon: $outputPath"
