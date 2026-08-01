[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$catalog = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$request = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'RuntimeTestRequest.cs')
$runner = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'RuntimeTestRunner.cs')
$controls = Get-Content -Raw -LiteralPath (
    Join-Path $root 'src\KingmakerGunslinger\Development\DevelopmentControls.cs')
$orchestrator = Get-Content -Raw -LiteralPath (
    Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1')

$checks = [ordered]@{
    'scenario-exactly-allowlisted' =
        $catalog.Contains('GenericFirearmActions') -and
        $catalog.Contains('"generic-firearm-actions"')
    'request-uses-working-save-timeouts' =
        $request.Contains('RuntimeTestScenarioCatalog.GenericFirearmActions')
    'reuses-qualified-working-save-loader' =
        $runner.Contains('RunWorkingSaveSmoke();') -and
        $runner.Contains('RuntimeTestScenarioCatalog.GenericFirearmActions')
    'waits-for-complete-load-before-feature' =
        $runner.IndexOf('if (_workingSaveSmoke.Complete)',
            [StringComparison]::Ordinal) -lt
        $runner.IndexOf('RunSprint30GenericActions();',
            [StringComparison]::Ordinal)
    'uses-existing-maintenance-fixture' =
        $runner.Contains(
            'DevelopmentControls.RunMaintenanceQualificationImmediately()') -and
        $controls.Contains('RunMaintenanceQualificationImmediately()')
    'requires-complete-loop' =
        $runner.Contains('maintenance.Message.IndexOf("MaintenanceLoopPassed"')
    'proves-native-heavy-crossbow-isolation' =
        $runner.Contains('"native-heavy-crossbow-isolation"') -and
        $runner.Contains('nativeMarkerCount == 0 && markedMarkerCount == 1')
    'save-write-sentinel-retained' =
        $runner.Contains('evidence.SaveWritingApiObserved ? "observed" : "none"') -and
        $runner.Contains('!evidence.SaveWritingApiObserved')
    'baseline-not-accepted' =
        $orchestrator.Contains(
            '[ValidateSet(''KMG_AUTOMATION_WORKING'')]')
    'steam-launch-only' =
        $orchestrator.Contains('[int]$SteamAppId = 640820') -and
        -not $orchestrator.Contains('Kingmaker.exe')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Sprint 30 generic-action scenario tests failed: $($failed -join ', ')"
}

Write-Host "Sprint 30 generic-action scenario tests passed: $($checks.Count)"
