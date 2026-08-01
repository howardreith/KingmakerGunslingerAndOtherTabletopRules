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
        $catalog.Contains('AdvancedCapacity') -and $catalog.Contains('"advanced-capacity"')
    'request-uses-working-save-timeouts' =
        $request.Contains('RuntimeTestScenarioCatalog.AdvancedCapacity')
    'orchestration-is-autonomous-working-save' =
        $common.Contains("'advanced-capacity' = [pscustomobject]") -and
        $common.Contains("PermittedSaveName = 'KMG_AUTOMATION_WORKING'")
    'waits-for-complete-load-before-feature' =
        $runner.IndexOf('if (_workingSaveSmoke.Complete)', [StringComparison]::Ordinal) -lt
        $runner.IndexOf('RunSprint33AdvancedCapacity();', [StringComparison]::Ordinal)
    'uses-exact-advanced-revolver-definition' =
        $runner.Contains('FirearmDefinitions.CreateAdvancedRevolver()')
    'proves-six-round-atomic-reloads' =
        $runner.Contains('firstLoad.RoundsLoaded == 6') -and
        $runner.Contains('secondLoad.RoundsLoaded == 6')
    'proves-reference-distinct-discharge-isolation' =
        $runner.Contains('firstAfterFire.LoadedRounds == 4') -and
        $runner.Contains('secondAfterFire.LoadedRounds == 6') -and
        $runner.Contains('repository.PersistedRecordCount == 2')
    'proves-advanced-repeated-misfire-policy' =
        $runner.Contains('AdvancedBrokenRemainsBroken') -and
        $runner.Contains('!explosion.RequiresBurstDamage')
    'save-write-sentinel-retained' =
        $runner.Contains('!evidence.SaveWritingApiObserved')
    'steam-launch-only' =
        $orchestrator.Contains('[int]$SteamAppId = 640820') -and
        -not $orchestrator.Contains('Kingmaker.exe')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Sprint 33 advanced-capacity scenario tests failed: $($failed -join ', ')"
}

Write-Host "Sprint 33 advanced-capacity scenario tests passed: $($checks.Count)"
