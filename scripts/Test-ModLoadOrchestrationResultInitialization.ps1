[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$orchestrator = Get-Content -Raw -LiteralPath (
    Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1')
$automationCommon = Get-Content -Raw -LiteralPath (
    Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$fixture = Get-Content -Raw -LiteralPath (
    Join-Path $root 'tests\fixtures\mod-load-orchestration-uninitialized-result.json') |
    ConvertFrom-Json

$launchBoundary = $orchestrator.IndexOf(
    '$process = $launch.kingmakerProcess', [StringComparison]::Ordinal)
$scenarioBoundary = $orchestrator.IndexOf(
    'if ($Scenario -eq ''working-save-smoke'') {', $launchBoundary,
    [StringComparison]::Ordinal)
$commonInitialization = $orchestrator.Substring(
    $launchBoundary, $scenarioBoundary - $launchBoundary)
$finalWait = $orchestrator.IndexOf(
    'while ($null -eq $result)', $scenarioBoundary,
    [StringComparison]::Ordinal)

$checks = [ordered]@{
    'fixture-reproduces-runtime-pass' =
        $fixture.runtimeStatus -eq 'PASS' -and
        $fixture.runtimeStage -eq 'final-result-flushed' -and
        $fixture.automaticExitRequested -eq $true -and
        $fixture.automaticExitInitiated -eq $true
    'fixture-reproduces-orchestration-error' =
        $fixture.orchestrationStatus -eq 'ERROR' -and
        $fixture.orchestrationStage -eq 'orchestration-error' -and
        $fixture.lastCompletedOrchestrationStage -eq 'waiting-for-final-result'
    'fixture-reproduces-first-failed-operation' =
        $fixture.exceptionType -eq 'System.Management.Automation.RuntimeException' -and
        $fixture.exceptionMessage -eq
            'The variable ''$result'' cannot be retrieved because it has not been set.'
    'fixture-proves-no-save-interaction' =
        $fixture.saveInteractionOccurred -eq $false
    'result-initialized-for-all-scenarios' =
        $commonInitialization.Contains('$result = $null')
    'request-time-initialized-for-all-scenarios' =
        $commonInitialization.Contains(
            '$requestWrittenUtc = (Get-Item -LiteralPath $requestPath).LastWriteTimeUtc')
    'initialization-precedes-final-wait' =
        $orchestrator.IndexOf('$result = $null', $launchBoundary,
            [StringComparison]::Ordinal) -lt $finalWait
    'steam-only-launch-retained' =
        $orchestrator.Contains('[int]$SteamAppId = 640820') -and
        -not ($orchestrator -match 'Start-Process\s+.*Kingmaker\.exe')
    'baseline-and-save-safety-retained' =
        $automationCommon.Contains('saveInteractionOccurred = $false') -and
        $orchestrator.Contains('KMG_AUTOMATION_BASELINE')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Mod-load orchestration result initialization tests failed: $($failed -join ', ')"
}

Write-Host "Mod-load orchestration result initialization tests passed: $($checks.Count)"
