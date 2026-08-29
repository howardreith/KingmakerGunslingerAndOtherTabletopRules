[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$generator = Join-Path $PSScriptRoot 'icon-art/New-IconOverhaulAssets.ps1'
if (-not (Test-Path -LiteralPath $generator -PathType Leaf)) {
    throw "Icon-overhaul generator is missing: $generator"
}

& $generator -Mode Feat
