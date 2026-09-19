[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$generator = Join-Path $PSScriptRoot 'icon-art/Export-IconPilot.ps1'
if (-not (Test-Path -LiteralPath $generator -PathType Leaf)) {
    throw "Icon-overhaul generator is missing: $generator"
}

# Candidate-only deterministic exports; never restore rejected runtime art.
& $generator
