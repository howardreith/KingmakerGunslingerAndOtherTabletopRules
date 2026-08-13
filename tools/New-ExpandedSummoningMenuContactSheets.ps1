[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$IndexPath,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$indexFile = Get-Item -LiteralPath $IndexPath
$index = Get-Content -LiteralPath $indexFile.FullName -Raw | ConvertFrom-Json
if ($index.schemaVersion -ne 1 -or @($index.sheets).Count -ne 9) {
    throw 'Live menu index must contain exactly nine schema-1 sheets.'
}
$outputRoot = $indexFile.Directory.FullName
$iconRoot = Join-Path $RepositoryRoot 'assets\game\icons\expanded-summoning'
$spritePrefix = 'KMG_SummonIcon_'
$rows = [Collections.Generic.List[object]]::new()

foreach ($sheet in $index.sheets) {
    $choices = @($sheet.choices)
    if ($choices.Count -eq 0) { throw "Menu $($sheet.family)$($sheet.tier) is empty." }
    $columns = 8
    $cellWidth = 176
    $cellHeight = 112
    $headerHeight = 62
    $sheetRows = [Math]::Ceiling($choices.Count / $columns)
    $bitmap = New-Object Drawing.Bitmap -ArgumentList @(
        ($columns * $cellWidth),
        ($headerHeight + $sheetRows * $cellHeight),
        [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.Clear([Drawing.Color]::FromArgb(255, 18, 14, 12))
            $headerFont = New-Object Drawing.Font 'Georgia',18,([Drawing.FontStyle]::Bold)
            $labelFont = New-Object Drawing.Font 'Arial',8,([Drawing.FontStyle]::Regular)
            $metaFont = New-Object Drawing.Font 'Consolas',7,([Drawing.FontStyle]::Regular)
            try {
                $graphics.DrawString("$($sheet.family) $($sheet.tier) - $($sheet.parentName)",
                    $headerFont,[Drawing.Brushes]::AntiqueWhite,14,10)
                $graphics.DrawString("exact final-live order | icon display size 64x64 | parent $($sheet.parentGuid)",
                    $metaFont,[Drawing.Brushes]::DarkGray,16,39)
                foreach ($choice in $choices) {
                    if ([string]::IsNullOrWhiteSpace($choice.spriteName) -or
                        -not $choice.spriteName.StartsWith($spritePrefix,[StringComparison]::Ordinal)) {
                        throw "Choice $($choice.guid) lacks a project-owned sprite key."
                    }
                    if ($choice.textureWidth -ne 128 -or $choice.textureHeight -ne 128) {
                        throw "Choice $($choice.guid) texture is not 128x128."
                    }
                    $key = $choice.spriteName.Substring($spritePrefix.Length)
                    $iconPath = Join-Path $iconRoot ($key + '.png')
                    if (-not (Test-Path -LiteralPath $iconPath -PathType Leaf)) {
                        throw "Production icon is missing for live key $key."
                    }
                    $column = $choice.position % $columns
                    $row = [Math]::Floor($choice.position / $columns)
                    $x = $column * $cellWidth
                    $y = $headerHeight + $row * $cellHeight
                    $background = if ($choice.displayName -match '1d4\+1') {
                        [Drawing.Color]::FromArgb(255,45,24,47)
                    } elseif ($choice.displayName -match '1d3') {
                        [Drawing.Color]::FromArgb(255,24,36,51)
                    } else { [Drawing.Color]::FromArgb(255,39,34,25) }
                    $cellBrush = New-Object Drawing.SolidBrush $background
                    try { $graphics.FillRectangle($cellBrush,$x + 2,$y + 2,
                            $cellWidth - 4,$cellHeight - 4) }
                    finally { $cellBrush.Dispose() }
                    $icon = [Drawing.Image]::FromFile($iconPath)
                    try {
                        $graphics.DrawImage($icon,$x + 8,$y + 8,64,64)
                    } finally { $icon.Dispose() }
                    $graphics.DrawString(([string]$choice.displayName),$labelFont,
                        [Drawing.Brushes]::White,
                        (New-Object Drawing.RectangleF -ArgumentList @(
                            ($x + 78),($y + 8),92,58)))
                    $graphics.DrawString(("#{0:D2} {1}" -f $choice.position,
                        $key),$metaFont,[Drawing.Brushes]::Silver,
                        (New-Object Drawing.RectangleF -ArgumentList @(
                            ($x + 8),($y + 79),160,28)))
                    $rows.Add([pscustomobject]@{
                        family = $sheet.family; tier = $sheet.tier
                        position = $choice.position; guid = $choice.guid
                        displayName = $choice.displayName; iconKey = $key
                        spriteName = $choice.spriteName
                    })
                }
            } finally {
                $headerFont.Dispose(); $labelFont.Dispose(); $metaFont.Dispose()
            }
        } finally { $graphics.Dispose() }
        $output = Join-Path $outputRoot ([string]$sheet.fileName)
        $bitmap.Save($output,[Drawing.Imaging.ImageFormat]::Png)
    } finally { $bitmap.Dispose() }
}

$manifestPath = Join-Path $outputRoot 'expanded-summoning-menu-contact-sheets.json'
$manifest = [ordered]@{
    schemaVersion = 1
    sourceIndex = $indexFile.Name
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    sheets = @($index.sheets | ForEach-Object {
        $path = Join-Path $outputRoot ([string]$_.fileName)
        [ordered]@{
            family = $_.family; tier = $_.tier; file = $_.fileName
            choiceCount = @($_.choices).Count
            sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        }
    })
    choices = $rows
}
$json = $manifest | ConvertTo-Json -Depth 8
[IO.File]::WriteAllText($manifestPath,$json + [Environment]::NewLine,
    (New-Object Text.UTF8Encoding($false)))
Write-Host "Expanded Summoning live menu contact sheets created: 9; choices=$($rows.Count)"
Write-Output $manifestPath
