[CmdletBinding()]
param(
    [string]$SourcePath = (Join-Path $PSScriptRoot '..\assets-source\original-icons\cord-of-stubborn-resolve\cord-of-stubborn-resolve-oblique-source.png'),
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\assets\game\icons\cord-of-stubborn-resolve.png'),
    [string]$ManifestPath = (Join-Path $PSScriptRoot '..\assets-source\original-icons\cord-of-stubborn-resolve\cord-of-stubborn-resolve-assets.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$runtimeSize = 128
$runtimeMargin = 6

function Get-AlphaBounds(
    [Drawing.Bitmap]$Bitmap, [byte]$Threshold = 3) {
    $rectangle = [Drawing.Rectangle]::new(
        0, 0, $Bitmap.Width, $Bitmap.Height)
    $data = $Bitmap.LockBits($rectangle,
        [Drawing.Imaging.ImageLockMode]::ReadOnly,
        [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $length = [Math]::Abs($data.Stride) * $data.Height
        $bytes = [byte[]]::new($length)
        [Runtime.InteropServices.Marshal]::Copy(
            $data.Scan0, $bytes, 0, $length)
        $minimumX = $Bitmap.Width
        $minimumY = $Bitmap.Height
        $maximumX = -1
        $maximumY = -1
        for ($y = 0; $y -lt $Bitmap.Height; $y++) {
            $row = if ($data.Stride -ge 0) {
                $y * $data.Stride
            } else {
                ($Bitmap.Height - 1 - $y) * (-$data.Stride)
            }
            for ($x = 0; $x -lt $Bitmap.Width; $x++) {
                if ($bytes[$row + $x * 4 + 3] -le $Threshold) {
                    continue
                }
                if ($x -lt $minimumX) { $minimumX = $x }
                if ($x -gt $maximumX) { $maximumX = $x }
                if ($y -lt $minimumY) { $minimumY = $y }
                if ($y -gt $maximumY) { $maximumY = $y }
            }
        }
        if ($maximumX -lt $minimumX -or $maximumY -lt $minimumY) {
            throw 'Cord source has no visible alpha pixels.'
        }
        return [Drawing.Rectangle]::FromLTRB(
            $minimumX, $minimumY, $maximumX + 1, $maximumY + 1)
    }
    finally {
        $Bitmap.UnlockBits($data)
    }
}

function Get-RelativeProjectPath([string]$Path) {
    $fullPath = [IO.Path]::GetFullPath($Path)
    if (-not $fullPath.StartsWith(
        $root + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw ('Cord asset is outside the repository: {0}' -f $fullPath)
    }
    return $fullPath.Substring($root.Length + 1).Replace('\', '/')
}

$sourceFullPath = [IO.Path]::GetFullPath($SourcePath)
$destination = [IO.Path]::GetFullPath($OutputPath)
$manifestFullPath = [IO.Path]::GetFullPath($ManifestPath)
$source = [Drawing.Bitmap]::new($sourceFullPath)
try {
    $sourceBounds = Get-AlphaBounds $source
    if ($sourceBounds.Left -eq 0 -or $sourceBounds.Top -eq 0 -or
        $sourceBounds.Right -eq $source.Width -or
        $sourceBounds.Bottom -eq $source.Height) {
        throw 'Cord source touches a canvas edge.'
    }
    $sourceAspect = $sourceBounds.Width / [double]$sourceBounds.Height
    if ($sourceAspect -lt 1.70 -or $sourceAspect -gt 2.25) {
        throw ('Cord source is not belt-like: aspect={0:N3}' -f
            $sourceAspect)
    }

    $available = $runtimeSize - 2 * $runtimeMargin
    $scale = [Math]::Min(
        $available / [double]$sourceBounds.Width,
        $available / [double]$sourceBounds.Height)
    $width = [Math]::Max(1,
        [int][Math]::Round($sourceBounds.Width * $scale))
    $height = [Math]::Max(1,
        [int][Math]::Round($sourceBounds.Height * $scale))
    $x = [int][Math]::Round(($runtimeSize - $width) / 2.0)
    $y = [int][Math]::Round(($runtimeSize - $height) / 2.0)
    $output = [Drawing.Bitmap]::new(
        $runtimeSize, $runtimeSize,
        [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [Drawing.Graphics]::FromImage($output)
        try {
            $graphics.Clear([Drawing.Color]::Transparent)
            $graphics.CompositingMode =
                [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.CompositingQuality =
                [Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode =
                [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode =
                [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.SmoothingMode =
                [Drawing.Drawing2D.SmoothingMode]::AntiAlias
            $graphics.DrawImage($source,
                [Drawing.Rectangle]::new($x, $y, $width, $height),
                $sourceBounds.X, $sourceBounds.Y,
                $sourceBounds.Width, $sourceBounds.Height,
                [Drawing.GraphicsUnit]::Pixel)
        }
        finally {
            $graphics.Dispose()
        }

        $runtimeBounds = Get-AlphaBounds $output
        $runtimeAspect =
            $runtimeBounds.Width / [double]$runtimeBounds.Height
        $cornerAlpha = @(
            $output.GetPixel(0, 0).A,
            $output.GetPixel($runtimeSize - 1, 0).A,
            $output.GetPixel(0, $runtimeSize - 1).A,
            $output.GetPixel($runtimeSize - 1, $runtimeSize - 1).A)
        if ($runtimeBounds.Width -le $runtimeBounds.Height -or
            $runtimeAspect -lt 1.70 -or
            @($cornerAlpha | Where-Object { $_ -ne 0 }).Count -ne 0) {
            throw 'Cord runtime export lost its belt silhouette or alpha.'
        }

        New-Item -ItemType Directory -Force -Path (
            Split-Path -Parent $destination) | Out-Null
        $output.Save($destination, [Drawing.Imaging.ImageFormat]::Png)

        $manifest = [ordered]@{
            schemaVersion = 2
            generator = 'tools/New-CordOfStubbornResolveIcon.ps1'
            sourceKind = 'imagegen-original-oblique-braided-cord'
            sourcePath = Get-RelativeProjectPath $sourceFullPath
            sourceDimensions = @($source.Width, $source.Height)
            sourceAlphaBounds = @(
                $sourceBounds.X, $sourceBounds.Y,
                $sourceBounds.Width, $sourceBounds.Height)
            sourceAspect = [Math]::Round($sourceAspect, 4)
            sourceSha256 = (Get-FileHash -LiteralPath $sourceFullPath -Algorithm SHA256).Hash.ToLowerInvariant()
            runtimePath = Get-RelativeProjectPath $destination
            runtimeDimensions = @($runtimeSize, $runtimeSize)
            runtimeAlphaBounds = @(
                $runtimeBounds.X, $runtimeBounds.Y,
                $runtimeBounds.Width, $runtimeBounds.Height)
            runtimeAspect = [Math]::Round($runtimeAspect, 4)
            runtimeMargin = $runtimeMargin
            cornerAlpha = $cornerAlpha
            runtimeSha256 = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash.ToLowerInvariant()
        }
        New-Item -ItemType Directory -Force -Path (
            Split-Path -Parent $manifestFullPath) | Out-Null
        [IO.File]::WriteAllText($manifestFullPath,
            ($manifest | ConvertTo-Json -Depth 6) + [Environment]::NewLine,
            [Text.UTF8Encoding]::new($false))
    }
    finally {
        $output.Dispose()
    }
}
finally {
    $source.Dispose()
}

Write-Host ('Exported Cord of Stubborn Resolve icon: {0}' -f $destination)
