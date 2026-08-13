[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runner = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $root `
    'scripts\RuntimeAutomation.Common.ps1')

$checks = [ordered]@{
    'save-free-existing-observer' =
        $common.Contains("'observe-vendor-table-contracts' = [pscustomobject]") -and
        $common.Contains('RequiresSaveName = $false')
    'critical-profile-assertion' =
        $runner.Contains('"production-critical-profiles"') -and
        $runner.Contains('"pistol=20/x4;musket=20/x4;blunderbuss=20/x2;"')
    'all-five-native-fields' =
        $runner.Contains('HasCriticalProfile(production.Pistol.WeaponType, 20, 4)') -and
        $runner.Contains('HasCriticalProfile(production.Musket.WeaponType, 20, 4)') -and
        $runner.Contains('HasCriticalProfile(production.Blunderbuss.WeaponType, 20, 2)') -and
        $runner.Contains('HasCriticalProfile(production.AdvancedRifle.WeaponType, 20, 4)') -and
        $runner.Contains('HasCriticalProfile(production.AdvancedRevolver.WeaponType, 20, 4)')
    'current-capital-entry-count' =
        $runner.Contains('capitalEntries.Count == 29')
    'observation-only-contract' =
        $runner.Contains('"vendor-observation-only"') -and
        $runner.Contains('"read-only blueprint enumeration"')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Sprint 69 critical-profile tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 69 critical-profile tests passed: $($checks.Count)"
