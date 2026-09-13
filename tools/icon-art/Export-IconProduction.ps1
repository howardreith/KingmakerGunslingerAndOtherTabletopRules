[CmdletBinding()]
param([string]$RepositoryRoot)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path (Split-Path $PSScriptRoot) }
$RepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
$relativeRoot = 'assets-source/original-icons/icon-overhaul-v2/production/'
$productionRoot = Join-Path $RepositoryRoot $relativeRoot
$manifestPath = Join-Path $productionRoot 'production-manifest.json'
$previous = @{}
if (Test-Path -LiteralPath $manifestPath) {
    $old = Get-Content -Raw -Encoding UTF8 -LiteralPath $manifestPath | ConvertFrom-Json
    foreach ($record in $old.records) { $previous[$record.key] = $record }
}
$briefs = @(Get-ChildItem -LiteralPath (Join-Path $productionRoot 'briefs') -Filter '*.json' | Sort-Object Name | ForEach-Object {
    $brief = Get-Content -Raw -Encoding UTF8 -LiteralPath $_.FullName | ConvertFrom-Json
    if ($brief.key -notmatch '^[a-z][a-z0-9-]+$' -or $_.BaseName -cne $brief.key) { throw "Invalid production key: $($_.Name)" }
    if ($brief.source -cne ($relativeRoot+'sources/'+$brief.key+'.png')) { throw "Unexpected production source: $($brief.key)" }
    $actual = (Get-FileHash -LiteralPath (Join-Path $RepositoryRoot $brief.source) -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -ne $brief.sourceSha256) { throw "Original source changed: $($brief.key)" }
    $brief
})
if (-not $briefs.Count) { throw 'No individually preserved production sources.' }
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Path (Join-Path $productionRoot 'exports') -Force | Out-Null
$records = @()
foreach ($brief in $briefs) {
    $key = $brief.key
    $relativeExport = $relativeRoot+'exports/'+$key+'.png'
    $exportPath = Join-Path $RepositoryRoot $relativeExport
    $prior = $previous[$key]
    if ($prior -and $prior.visualStatus -eq 'approved') {
        if ($prior.sourceSha256 -ne $brief.sourceSha256 -or
            $prior.export -cne $relativeExport -or
            $prior.approvedHash -ne $prior.exportSha256 -or
            (Get-FileHash -LiteralPath $exportPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $prior.exportSha256) {
            throw "Approved production image changed: $key. Preserve it and create a reviewed revision."
        }
        $records += $prior
        continue
    }
    $source = [Drawing.Bitmap]::new((Join-Path $RepositoryRoot $brief.source))
    try {
        if ($source.Width -ne $source.Height -or $source.Width -lt 512) { throw "Unexpected production source size: $key" }
        $target = [Drawing.Bitmap]::new(128,128,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g = [Drawing.Graphics]::FromImage($target)
        $attrs = [Drawing.Imaging.ImageAttributes]::new()
        try {
            $g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $g.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $g.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
            $g.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $attrs.SetWrapMode([Drawing.Drawing2D.WrapMode]::TileFlipXY)
            $g.DrawImage($source,[Drawing.Rectangle]::new(0,0,128,128),0,0,$source.Width,$source.Height,[Drawing.GraphicsUnit]::Pixel,$attrs)
            $target.Save($exportPath,[Drawing.Imaging.ImageFormat]::Png)
        } finally { $attrs.Dispose(); $g.Dispose(); $target.Dispose() }
        $records += [ordered]@{
            key=$key; source=$brief.source; sourceSha256=$brief.sourceSha256
            sourceSize=@($source.Width,$source.Height)
            export=$relativeExport
            exportSha256=(Get-FileHash -LiteralPath $exportPath -Algorithm SHA256).Hash.ToLowerInvariant()
            exportSize=@(128,128)
            brief=($relativeRoot+'briefs/'+$key+'.json')
            technicalStatus='candidate-export'; visualStatus='awaiting-owner-production-review'; approvedHash=$null
        }
    } finally { $source.Dispose() }
}
$manifest = [ordered]@{
    schemaVersion=1
    authority='Individual production sources/exports only; canonical catalog owns consumer bindings and owner review.'
    exporter='tools/icon-art/Export-IconProduction.ps1'
    engine='Windows System.Drawing GDI+; HighQualityBicubic; TileFlipXY; SourceCopy ARGB32. No generation, chroma removal, sharpening or timestamps.'
    runtimeInstallation='none; isolated production candidates pending integration'
    records=$records
}
[IO.File]::WriteAllText($manifestPath,(($manifest | ConvertTo-Json -Depth 8)+[char]10),[Text.UTF8Encoding]::new($false))
Write-Output "Exported/verified $($records.Count) individual production candidates."
