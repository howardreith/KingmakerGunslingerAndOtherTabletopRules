Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$invoke = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot `
    'Invoke-KingmakerCompatibilityProfile.ps1')
$profiles = Get-Content -Raw -LiteralPath (Join-Path $root `
    'compatibility\profiles.json') | ConvertFrom-Json

$required = @(
    'gunslinger-only',
    'gunslinger-call-of-the-wild',
    'gunslinger-arms-armor',
    'gunslinger-toggle-custom-soundpacks',
    'gunslinger-high-risk-combined'
)
$ids = @($profiles.profiles | ForEach-Object id)
$missing = @($required | Where-Object { $_ -cnotin $ids })
if ($missing.Count -ne 0) {
    throw "Expanded Summoning compatibility profiles are missing: $($missing -join ', ')"
}
if (-not $invoke.Contains("'observe-expanded-summoning-inventory'")) {
    throw 'Expanded Summoning structural runtime scenario is not allowlisted for compatibility profiles.'
}
if (-not $invoke.Contains('restorationVerified') -or
    -not $invoke.Contains('Restore-KingmakerCompatibilityProfile.ps1')) {
    throw 'Compatibility invocation no longer requires exact transaction restoration.'
}

Write-Host "Expanded Summoning compatibility profile tests passed: $($required.Count)"
