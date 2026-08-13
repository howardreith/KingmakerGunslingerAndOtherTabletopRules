[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $root 'assets-source\original-icons\expanded-summoning\icon-manifest.json'
$previewRoot = Join-Path $root 'assets-source\original-icons\expanded-summoning\previews'
$outputPath = Join-Path $previewRoot 'expanded-summoning-icons-contact-sheet.png'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.count -ne 77 -or @($manifest.icons).Count -ne 77) {
    throw 'Expected the frozen 77-icon provenance manifest.'
}

Add-Type -AssemblyName System.Drawing
if (-not (Test-Path -LiteralPath $previewRoot)) {
    New-Item -ItemType Directory -Path $previewRoot | Out-Null
}
$columns = 11
$cellWidth = 128
$cellHeight = 152
$rows = [Math]::Ceiling($manifest.count / [double]$columns)
$sheet = New-Object System.Drawing.Bitmap ($columns * $cellWidth), ($rows * $cellHeight),
    ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    $graphics = [System.Drawing.Graphics]::FromImage($sheet)
    try {
        $graphics.Clear([System.Drawing.Color]::FromArgb(255, 24, 20, 18))
        $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
        $font = New-Object System.Drawing.Font 'Segoe UI', 8,
            ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
        $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 236, 222, 190))
        try {
            for ($index = 0; $index -lt $manifest.icons.Count; $index++) {
                $icon = $manifest.icons[$index]
                $x = ($index % $columns) * $cellWidth
                $y = [Math]::Floor($index / $columns) * $cellHeight
                $path = Join-Path $root ([string]$icon.productionFile).Replace('/', '\')
                $image = [System.Drawing.Image]::FromFile($path)
                try { $graphics.DrawImageUnscaled($image, $x, $y) }
                finally { $image.Dispose() }
                $label = [string]$icon.displayName
                if ($label.Length -gt 21) { $label = $label.Substring(0, 20) + [char]0x2026 }
                $graphics.DrawString($label, $font, $brush, $x + 3, $y + 132)
            }
        }
        finally { $brush.Dispose(); $font.Dispose() }
    }
    finally { $graphics.Dispose() }
    $sheet.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
}
finally { $sheet.Dispose() }

Write-Host "Expanded Summoning contact sheet: $outputPath"
