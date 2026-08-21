[CmdletBinding()]
param(
    [string]$ProjectPath = 'C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$models = Join-Path $root 'assets-source\third-party\models'
$fitExperiments = Join-Path $root 'assets-source\original-models\firearm-fit-experiments'
$longGunDerivatives = Join-Path $models 'firearm-long-gun-derivatives'
$pistolVariants = Join-Path $root 'assets-source\original-models\firearm-pistol-variants'
$audio = Join-Path $root 'assets-source\third-party\audio\sse-library-guns\processed'
$approvedModels = Join-Path $ProjectPath 'Assets\ApprovedModels'
$approvedAudio = Join-Path $ProjectPath 'Assets\ApprovedAudio'
New-Item -ItemType Directory -Force -Path $approvedModels,$approvedAudio | Out-Null
$editor = Join-Path $ProjectPath 'Assets\Editor'
New-Item -ItemType Directory -Force -Path $editor | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'tools\unity\BuildFirearmBundles.cs') `
    -Destination (Join-Path $editor 'BuildFirearmBundles.cs') -Force
Copy-Item -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Assets\WeaponPresentationSemanticFrame.cs') `
    -Destination (Join-Path $editor 'WeaponPresentationSemanticFrame.cs') -Force
$staging = @(
    @{ Name='Pistol'; Source=(Join-Path $models 'cyril43-flintlock-pistol\source\pistol.zip'); Zip=$true },
    @{ Name='Musket'; Source=(Join-Path $models 'mesh-masters-rifle-musket'); Zip=$false },
    @{ Name='Blunderbuss'; Source=(Join-Path $models 'ccotwist-blunderbuss'); Zip=$false },
    @{ Name='Revolver'; Source=(Join-Path $models '1851-navy-colt-revolver'); Zip=$false },
    @{ Name='Rifle'; Source=(Join-Path $models 'killian-delias-winchester-lever-action-rifle'); Zip=$false }
)
foreach ($item in $staging) {
    $destination = Join-Path $approvedModels $item.Name
    if (Test-Path -LiteralPath $destination) { Remove-Item -LiteralPath $destination -Recurse -Force }
    New-Item -ItemType Directory -Path $destination | Out-Null
    if ($item.Zip) {
        Expand-Archive -LiteralPath $item.Source -DestinationPath $destination
    } else {
        Copy-Item -LiteralPath (Join-Path $item.Source 'source') -Destination $destination -Recurse
        Copy-Item -LiteralPath (Join-Path $item.Source 'textures') -Destination $destination -Recurse
    }
}
$musketDestination = Join-Path $approvedModels 'Musket'
$fitCandidates = @(
    'musket-pass-through.fbx',
    'musket-minimal-control.fbx',
    'musket-clearance-stock.fbx'
)
foreach ($candidate in $fitCandidates) {
    $source = Join-Path $fitExperiments $candidate
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
        throw "Missing generated Musket fit candidate: $source"
    }
    Copy-Item -LiteralPath $source -Destination $musketDestination -Force
}
$derivedLongGuns = @(
    @{ Family='Musket'; File='musket-normalized.fbx' },
    @{ Family='Blunderbuss'; File='blunderbuss-normalized.fbx' }
)
foreach ($candidate in $derivedLongGuns) {
    $source = Join-Path $longGunDerivatives $candidate.File
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
        throw "Missing generated long-gun derivative: $source"
    }
    Copy-Item -LiteralPath $source -Destination `
        (Join-Path $approvedModels $candidate.Family) -Force
}
$pistolDestination = Join-Path $approvedModels 'Pistol'
$generatedPistols = @('pistol-duelist.fbx','pistol-last-word.fbx')
foreach ($candidate in $generatedPistols) {
    $source = Join-Path $pistolVariants $candidate
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
        throw "Missing generated Pistol variant: $source"
    }
    Copy-Item -LiteralPath $source -Destination $pistolDestination -Force
}
Get-ChildItem -LiteralPath $approvedAudio -File -ErrorAction SilentlyContinue | Remove-Item -Force
Copy-Item -LiteralPath (Get-ChildItem -LiteralPath $audio -Filter '*.wav').FullName -Destination $approvedAudio

Write-Host 'Prepared five approved model families, two normalized long-gun derivatives, three Musket fit candidates, two Pistol item variants, and five approved audio clips.'
