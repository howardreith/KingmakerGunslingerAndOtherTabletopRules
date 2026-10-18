[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$OutputPath)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(Test-Path -LiteralPath $OutputPath){throw 'Original output already exists'}
Add-Type -AssemblyName System.Drawing
function Fill-LeapShape([Drawing.Graphics]$G,[Drawing.Brush]$B,[single[]]$XY){
 $points=for($i=0;$i -lt $XY.Length;$i+=2){[Drawing.PointF]::new($XY[$i],$XY[$i+1])}
 $G.FillPolygon($B,[Drawing.PointF[]]$points)
}
$bitmap=[Drawing.Bitmap]::new(512,512,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g=[Drawing.Graphics]::FromImage($bitmap)
$ink=[Drawing.ColorTranslator]::FromHtml('#A6533F')
$brush=[Drawing.SolidBrush]::new($ink);$pen=[Drawing.Pen]::new($ink,3.0)
$thin=[Drawing.Pen]::new($ink,2.2);$clear=[Drawing.SolidBrush]::new([Drawing.Color]::Transparent)
try{
 $g.Clear([Drawing.Color]::Transparent)
 $g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias
 $g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::HighQuality
 $g.CompositingQuality=[Drawing.Drawing2D.CompositingQuality]::HighQuality
 $g.ScaleTransform(8,8)
 $pen.StartCap=$pen.EndCap=[Drawing.Drawing2D.LineCap]::Round
 $thin.StartCap=$thin.EndCap=[Drawing.Drawing2D.LineCap]::Round
 # Native-10 / approved Rapid Reload scaffold, original landshark gesture.
 $g.DrawEllipse($pen,6.0,6.0,52.0,52.0)
 # Low armored body and wedge jaw, with its dorsal fin above the shoulders.
 Fill-LeapShape $g $brush @(14,26,19,25,23,22,31,21,36,22,44,25,50,29,46,32,38,30,31,32,24,31,19,29)
 Fill-LeapShape $g $brush @(28,23,32,14,37,24)
 # Exactly four outstretched clawed limbs; readable space below the leap.
 $g.DrawLine($thin,24.0,30.0,20.0,36.0);$g.DrawLine($thin,20.0,36.0,16.0,37.0)
 $g.DrawLine($thin,29.0,29.0,26.0,35.0);$g.DrawLine($thin,26.0,35.0,23.0,36.0)
 $g.DrawLine($thin,36.0,30.0,41.0,36.0);$g.DrawLine($thin,41.0,36.0,45.0,35.0)
 $g.DrawLine($thin,40.0,28.0,45.0,32.0);$g.DrawLine($thin,45.0,32.0,49.0,32.0)
 # Ground and rising motion stroke use no baked UI state or text.
 $g.DrawLine($thin,16.0,46.0,48.0,46.0)
 $g.DrawArc($thin,13.0,15.0,36.0,31.0,108.0,77.0)
 Fill-LeapShape $g $brush @(10,25,15,21,17,28)
 $g.CompositingMode=[Drawing.Drawing2D.CompositingMode]::SourceCopy
 $g.FillEllipse($clear,42.0,26.0,2.8,2.8)
 $bitmap.Save([IO.Path]::GetFullPath($OutputPath),[Drawing.Imaging.ImageFormat]::Png)
}finally{$g.Dispose();$bitmap.Dispose();$brush.Dispose();$pen.Dispose();$thin.Dispose();$clear.Dispose()}
