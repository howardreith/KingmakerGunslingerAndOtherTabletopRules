[CmdletBinding()]
param(
    [string]$ProjectPath = 'C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$models = Join-Path $root 'assets-source\third-party\models'
$audio = Join-Path $root 'assets-source\third-party\audio\sse-library-guns\processed'
$approvedModels = Join-Path $ProjectPath 'Assets\ApprovedModels'
$approvedAudio = Join-Path $ProjectPath 'Assets\ApprovedAudio'
New-Item -ItemType Directory -Force -Path $approvedModels,$approvedAudio | Out-Null

$staging = @(
    @{ Name='Pistol'; Source=(Join-Path $models 'cyril43-flintlock-pistol\source\pistol.zip'); Zip=$true },
    @{ Name='Musket'; Source=(Join-Path $models 'mesh-masters-rifle-musket'); Zip=$false },
    @{ Name='Blunderbuss'; Source=(Join-Path $models 'ccotwist-blunderbuss'); Zip=$false },
    @{ Name='Revolver'; Source=(Join-Path $models '1851-navy-colt-revolver'); Zip=$false }
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
Get-ChildItem -LiteralPath $approvedAudio -File -ErrorAction SilentlyContinue | Remove-Item -Force
Copy-Item -LiteralPath (Get-ChildItem -LiteralPath $audio -Filter '*.wav').FullName -Destination $approvedAudio

$forbidden = Get-ChildItem -LiteralPath (Join-Path $ProjectPath 'Assets') -Recurse -File |
    Where-Object { $_.Name -match 'fusil|Martini|Henry|Winchester' }
if ($forbidden) { throw "Quarantined advanced-rifle material entered Unity staging: $($forbidden.FullName -join ', ')" }
Write-Host 'Prepared four approved model families and five approved audio clips.'
