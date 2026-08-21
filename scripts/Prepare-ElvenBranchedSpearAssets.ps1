[CmdletBinding()]
param(
    [string]$ProjectPath = 'C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$source = Join-Path $root 'assets-source\original-models\elven-branched-spear'
$destination = Join-Path $ProjectPath 'Assets\ElvenBranchedSpear'
$editor = Join-Path $ProjectPath 'Assets\Editor'
foreach ($required in @('elven-branched-spear.fbx',
        'elven-branched-spear-thorn.fbx',
        'elven-branched-spear-crown.fbx',
        'elven-branched-spear-build-report.json')) {
    if (-not (Test-Path -LiteralPath (Join-Path $source $required) -PathType Leaf)) {
        throw "Required original spear asset is missing: $required"
    }
}
New-Item -ItemType Directory -Force -Path $destination,$editor | Out-Null
foreach ($fbx in @('elven-branched-spear.fbx',
        'elven-branched-spear-thorn.fbx',
        'elven-branched-spear-crown.fbx')) {
    Copy-Item -LiteralPath (Join-Path $source $fbx) `
        -Destination (Join-Path $destination $fbx) -Force
}
Copy-Item -LiteralPath (Join-Path $source 'elven-branched-spear-build-report.json') `
    -Destination (Join-Path $destination 'source-build-report.json') -Force
Copy-Item -LiteralPath (Join-Path $root `
    'tools\unity\BuildElvenBranchedSpearBundle.cs') -Destination `
    (Join-Path $editor 'BuildElvenBranchedSpearBundle.cs') -Force
Copy-Item -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Assets\WeaponPresentationSemanticFrame.cs') `
    -Destination (Join-Path $editor 'WeaponPresentationSemanticFrame.cs') -Force
Write-Host 'Prepared three project-owned spear FBXs and the dedicated Unity builder.'
