[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ReferenceProvenance.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
function Assert-True([bool]$Condition, [string]$Name) {
    if (-not $Condition) { $failures.Add($Name) }
}
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $failures.Add($Name) } catch { }
}

$root = Split-Path $PSScriptRoot -Parent
$inspector = Get-Content -LiteralPath (
    Join-Path $PSScriptRoot 'inspect-runtime-contracts.ps1') -Raw
$restriction = Get-Content -LiteralPath (
    Join-Path $root 'src\KingmakerGunslinger\Firearms\FirearmProficiencyRestriction.cs') -Raw
$bridge = Get-Content -LiteralPath (
    Join-Path $root 'src\KingmakerGunslinger\Development\KingmakerDevelopmentBridge.cs') -Raw
$buildLocal = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Build-Local.ps1') -Raw
$qualifier = Get-Content -LiteralPath (
    Join-Path $PSScriptRoot 'qualify-runtime-candidate.ps1') -Raw

Assert-True ($restriction.Contains(
    'unit.Progression.Features.GetRank(m_RequiredProficiency) > 0')) `
    'reachable-restriction-contract-is-get-rank'
Assert-True ($bridge.Contains(
    'descriptor.Progression.Features.AddFeature(') -and
    $bridge.Contains('unitDescriptor.Progression.Features.GetRank(feature) > 0')) `
    'reachable-development-contract-is-feature-collection'
Assert-True (-not $inspector.Contains('$getFeatureMethods')) `
    'obsolete-unit-descriptor-get-feature-gate-removed'
Assert-True (-not $inspector.Contains('$featureGrantMethods')) `
    'obsolete-unit-descriptor-grant-gate-removed'
Assert-True ($inspector.Contains(
    "-Names @('GetRank')") -and
    $inspector.Contains("-Names @('AddFeature')")) `
    'actual-named-members-resolved-narrowly'
Assert-True ($inspector.Contains(
    "'Kingmaker.UnitLogic.Mechanics.MechanicsContext'") -and
    $inspector.Contains("'Kingmaker.UnitLogic.Feature'")) `
    'alternate-api-shape-validated-exactly'
Assert-True ($inspector.Contains(
    "'Kingmaker.Blueprints.Items.Ecnchantments.ItemEnchantment'")) `
    'typed-token-store-enchantment-contract-uses-production-namespace'
Assert-True ($buildLocal.Contains('Assert-KmgReferenceBundleMatchesInstall')) `
    'exact-build-references-compared-before-build'
Assert-True ($qualifier.Contains('Completed 611 tests; failures=0.') -and
    -not $qualifier.Contains('Completed 599 tests; failures=0.')) `
    'deterministic-qualifier-uses-current-sprint30-test-total'
Assert-True ($qualifier.Contains(
    "& (Join-Path `$PSScriptRoot 'Build-Local.ps1') @buildArguments") -and
    -not $qualifier.Contains(
    "& (Join-Path `$PSScriptRoot 'build.ps1') @buildArguments")) `
    'qualifier-rebuilds-through-exact-reference-path'

$fixture = Join-Path $root 'artifacts\reference-provenance-test'
if (Test-Path -LiteralPath $fixture) {
    Remove-Item -LiteralPath $fixture -Recurse -Force
}
try {
    $bundle = Join-Path $fixture 'bundle'
    $install = Join-Path $fixture 'install'
    foreach ($relative in $script:KmgPrivateReferencePaths) {
        $bundleFile = Join-Path (Join-Path $bundle 'Managed') $relative
        $installFile = Join-Path (Join-Path $install 'Kingmaker_Data\Managed') $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $bundleFile) -Force | Out-Null
        New-Item -ItemType Directory -Path (Split-Path -Parent $installFile) -Force | Out-Null
        [IO.File]::WriteAllText($bundleFile, "fixture:$relative")
        [IO.File]::WriteAllText($installFile, "fixture:$relative")
    }
    $matched = @(Assert-KmgReferenceBundleMatchesInstall `
        -ReferenceBundleDir $bundle -KingmakerInstallDir $install)
    Assert-True ($matched.Count -eq 13) 'all-explicit-references-match'
    [IO.File]::AppendAllText(
        (Join-Path $bundle 'Managed\Assembly-CSharp.dll'),
        'mismatch')
    Assert-Throws {
        Assert-KmgReferenceBundleMatchesInstall `
            -ReferenceBundleDir $bundle -KingmakerInstallDir $install
    } 'mismatched-private-reference-fails'
    Remove-Item -LiteralPath (
        Join-Path $install 'Kingmaker_Data\Managed\Assembly-CSharp.dll') -Force
    Assert-Throws {
        Assert-KmgReferenceBundleMatchesInstall `
            -ReferenceBundleDir $bundle -KingmakerInstallDir $install
    } 'missing-installed-required-reference-fails'
}
finally {
    if (Test-Path -LiteralPath $fixture) {
        Remove-Item -LiteralPath $fixture -Recurse -Force
    }
}

Assert-True ($inspector.Contains('disputedGateProvenance')) `
    'gate-provenance-recorded-in-contract-output'
Assert-True ($inspector.Contains('$contractPassed = (')) `
    'strict-aggregate-contract-gate-retained'

if ($failures.Count -ne 0) {
    throw "Contract-provenance tests failed: $($failures -join ', ')"
}
Write-Host 'Contract-provenance tests passed: 15'
