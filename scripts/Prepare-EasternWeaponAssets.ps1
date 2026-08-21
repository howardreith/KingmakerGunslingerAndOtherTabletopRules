[CmdletBinding()]
param(
    [string]$ProjectPath = 'C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$source = Join-Path $root 'assets-source\original-models\eastern-weapons'
$destination = Join-Path $ProjectPath 'Assets\EasternWeapons'
$editor = Join-Path $ProjectPath 'Assets\Editor'
$fbxFiles = @('wakizashi.fbx', 'wakizashi-petal.fbx',
    'wakizashi-moon.fbx', 'wakizashi-capstone.fbx', 'katana.fbx',
    'katana-reed.fbx', 'katana-regal.fbx', 'katana-capstone.fbx',
    'nodachi.fbx', 'nodachi-cleaver.fbx', 'nodachi-titan.fbx',
    'nodachi-capstone.fbx')
foreach ($required in @($fbxFiles + @(
        'eastern-weapons-build-report.json'))) {
    if (-not (Test-Path -LiteralPath (Join-Path $source $required) -PathType Leaf)) {
        throw "Required original Eastern Weapon asset is missing: $required"
    }
}
New-Item -ItemType Directory -Force -Path $destination,$editor | Out-Null
foreach ($name in $fbxFiles) {
    Copy-Item -LiteralPath (Join-Path $source $name) `
        -Destination (Join-Path $destination $name) -Force
}
Copy-Item -LiteralPath (Join-Path $source 'eastern-weapons-build-report.json') `
    -Destination (Join-Path $destination 'source-build-report.json') -Force
Copy-Item -LiteralPath (Join-Path $root 'tools\unity\BuildEasternWeaponsBundle.cs') `
    -Destination (Join-Path $editor 'BuildEasternWeaponsBundle.cs') -Force
Copy-Item -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Assets\WeaponPresentationSemanticFrame.cs') `
    -Destination (Join-Path $editor 'WeaponPresentationSemanticFrame.cs') -Force
Write-Host 'Prepared twelve original Eastern Weapon FBXs and the dedicated Unity builder.'
