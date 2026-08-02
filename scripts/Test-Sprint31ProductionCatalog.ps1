[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$catalog = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$request = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRequest.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$orchestrator = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1')

$checks = [ordered]@{
    'scenario-exactly-allowlisted' =
        $catalog.Contains('ProductionFirearmCatalog') -and
        $catalog.Contains('"production-firearm-catalog"')
    'request-uses-working-save-timeouts' =
        $request.Contains('RuntimeTestScenarioCatalog.ProductionFirearmCatalog')
    'orchestration-is-autonomous-working-save' =
        $common.Contains("'production-firearm-catalog' = [pscustomobject]") -and
        $common.Contains("PermittedSaveName = 'KMG_AUTOMATION_WORKING'")
    'waits-for-complete-load-before-feature' =
        $runner.IndexOf('if (_workingSaveSmoke.Complete)', [StringComparison]::Ordinal) -lt
        $runner.IndexOf('RunSprint31ProductionFirearmCatalog();', [StringComparison]::Ordinal)
    'validates-concrete-runtime-blueprints' =
        $runner.Contains('ProductionFirearmBlueprints.Validate(') -and
        $runner.Contains('catalog.Pistol.Spec.Equals(ProductionFirearmCatalog.CreatePistol())')
    'proves-marker-and-native-isolation' =
        $runner.Contains('"marker-and-native-source-isolation"') -and
        $runner.Contains('nativeHeavyMarkers == 0')
    'special-range-fails-closed' =
        $runner.Contains('"special-range-fails-closed"') -and
        $runner.Contains('blunderbussUnavailable == 1')
    'production-critical-profiles-exact' =
        $runner.Contains('"production-critical-profiles"') -and
        $runner.Contains('HasCriticalProfile(catalog.Pistol.WeaponType, 20, 4)') -and
        $runner.Contains('HasCriticalProfile(catalog.Musket.WeaponType, 20, 4)') -and
        $runner.Contains('HasCriticalProfile(catalog.Blunderbuss.WeaponType, 20, 2)') -and
        $runner.Contains('HasCriticalProfile(catalog.AdvancedRifle.WeaponType, 20, 4)') -and
        $runner.Contains('HasCriticalProfile(catalog.AdvancedRevolver.WeaponType, 20, 4)')
    'save-write-sentinel-retained' =
        $runner.Contains('!evidence.SaveWritingApiObserved')
    'steam-launch-only' =
        $orchestrator.Contains('[int]$SteamAppId = 640820') -and
        -not $orchestrator.Contains('Kingmaker.exe')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Sprint 31 production-catalog scenario tests failed: $($failed -join ', ')"
}

Write-Host "Sprint 31 production-catalog scenario tests passed: $($checks.Count)"
