<#
.SYNOPSIS
Stages the Pteranodon membrane source into the exact Unity 2018.4.10f1 project.

.DESCRIPTION
The generated FBX is a machine-local build input, not a committed asset. A
skinned replacement mesh can only exist in the donor's bind frame, so the FBX
necessarily encodes the donor's skeleton geometry; it stays beside the rig
capture it was built from, in the same way GamePath.props and the private
runtime-reference bundle are machine-local. See
assets-source/original-models/pteranodon/SOURCE.md for how to regenerate it.
#>
[CmdletBinding()]
param(
    [string]$ProjectPath = 'C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1',
    [string]$SourceRoot = 'C:\Dev\KingmakerGunslingerLab\unity-asset-build\pteranodon-source'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$committed = Join-Path $root 'assets-source\original-models\pteranodon'
$destination = Join-Path $ProjectPath 'Assets\Pteranodon'
$editor = Join-Path $ProjectPath 'Assets\Editor'

$fbx = Join-Path $SourceRoot 'pteranodon.fbx'
if (-not (Test-Path -LiteralPath $fbx -PathType Leaf)) {
    throw "The generated membrane FBX is missing: $fbx. Regenerate it with " +
        "assets-source\original-models\pteranodon\generate_pteranodon.py as " +
        "described in that directory's SOURCE.md."
}

$report = Join-Path $committed 'pteranodon-build-report.json'
if (-not (Test-Path -LiteralPath $report -PathType Leaf)) {
    throw "The committed membrane build report is missing: $report"
}

# The report records the SHA-256 of the rig the FBX was generated from. A
# staged FBX built from a different rig would bind plausibly and deform wrongly,
# which is exactly the failure the bind-pose work was done to remove, so refuse
# to stage one whose provenance cannot be shown.
$reportJson = Get-Content -Raw -LiteralPath $report | ConvertFrom-Json
if ([string]::IsNullOrWhiteSpace([string]$reportJson.rigSha256) -or
    ([string]$reportJson.rigSha256).Length -ne 64) {
    throw 'The membrane build report does not record a rig SHA-256.'
}
if ([string]$reportJson.rigSpace -notlike '*renderer-local*') {
    throw 'The membrane build report does not record the donor renderer space.'
}

New-Item -ItemType Directory -Force -Path $destination, $editor | Out-Null
Copy-Item -LiteralPath $fbx -Destination (Join-Path $destination `
    'pteranodon.fbx') -Force
Copy-Item -LiteralPath $report -Destination (Join-Path $destination `
    'source-build-report.json') -Force
Copy-Item -LiteralPath (Join-Path $root 'tools\unity\BuildPteranodonBundle.cs') `
    -Destination (Join-Path $editor 'BuildPteranodonBundle.cs') -Force

$fbxHash = (Get-FileHash -LiteralPath $fbx -Algorithm SHA256).Hash
Write-Host "Prepared the Pteranodon membrane source and its dedicated Unity builder."
Write-Host "  FBX SHA-256: $fbxHash"
Write-Host "  rig SHA-256: $($reportJson.rigSha256)"
Write-Host "  vertices=$($reportJson.vertices) polygons=$($reportJson.polygons) bones=$($reportJson.boneGroups)"
