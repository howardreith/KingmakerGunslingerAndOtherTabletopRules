[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$scenario = Get-Content -Raw -LiteralPath (Join-Path $runtime 'WorkingSaveSmokeScenario.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs')
$result = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestResult.cs')
$orchestrator = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1')
$common = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$fixture = Get-Content -Raw -LiteralPath (Join-Path $root 'tests\fixtures\working-save-entry-readiness-deadlock.json') | ConvertFrom-Json

$checks = [ordered]@{
    'old-deadlock-reproduced' = $fixture.catalogComplete -and
        $fixture.workingMatchCount -eq 1 -and $fixture.entryCandidateCount -eq 1 -and
        $fixture.loadEntryInvocationCount -eq 0 -and -not $fixture.readinessWritten -and
        -not $fixture.humanClickPermitted -and $fixture.timeoutStage -ceq 'load-entry-invocation'
    'stage-b-before-click' = $scenario.IndexOf('Transition("working-entry-click"') -lt
        $scenario.IndexOf('_humanActionInvocations++')
    'stage-a-observer-armed' = $runner.Contains(
        '_workingStartupStage = "observer-armed"') -and
        $runner.IndexOf('_workingStartupStage = "observer-armed"') -lt
            $runner.IndexOf('? "working-entry-ready"')
    'stage-b-does-not-require-load-entry' = $scenario.Contains(
        '_observeSelectionLoadAction || _entryActionCandidates <= 1') -and
        $scenario.Contains('_loadEntry != null')
    'banner-follows-marker-validation' = $orchestrator.IndexOf(
        'Test-KmgRuntimeReadyMarker') -lt $orchestrator.IndexOf(
        'CLICK LOAD ON KMG_AUTOMATION_WORKING ONCE NOW')
    'click-clock-after-stage-b' = $orchestrator.IndexOf(
        '$orchestration.manualInteractionTimeoutBeganAtUtc') -gt
        $orchestrator.IndexOf('Test-KmgRuntimeReadyMarker')
    'probe-does-not-initiate-entry-load' = $scenario.Contains(
        'ProbeInvokedEntryAction = _autonomousReceiverBoundAction') -and
        $runner.Contains('receiverBoundObservation')
    'unique-working-required' = $runner.Contains('_workingSaveSmoke.WorkingCount == 0') -and
        $runner.Contains('_workingSaveSmoke.WorkingCount > 1')
    'baseline-distinct-required' = $runner.Contains('_workingSaveSmoke.BaselineCount == 0') -and
        $scenario.Contains('working and baseline object references are distinct')
    'multiple-working-ambiguous' = $runner.Contains('Multiple exact working descriptors were captured.')
    'wrong-selection-fails' = $runner.Contains('"forbidden-save-selection"')
    'load-entry-is-post-click-evidence' = $scenario.Contains(
        '_stage == "working-entry-click" ||') -and
        $scenario.Contains('Transition("load-completion"')
    'uncorrelated-action-ambiguous' = $runner.Contains('"entry-action-correlation"') -and
        $runner.Contains('RuntimeTestStatuses.Ambiguous')
    'pass-needs-completion-fingerprint' = $scenario.Contains(
        'return _completionCallback && _stableSamples >= 2')
    'hooks-preserve-original' = -not $scenario.Contains('__runOriginal') -and
        -not $scenario.Contains('__result')
    'hooks-request-scoped' = $scenario.Contains('RemoveHooks();') -and
        $scenario.Contains('if (ReferenceEquals(_active, this)) _active = null')
    'readiness-validates-save' = $result.Contains('JsonProperty("saveName"') -and
        $common.Contains('Test-KmgSupervisedWorkingSaveEntryReadinessBehavior') -and
        $common.Contains("`$Marker.saveName -cne 'KMG_AUTOMATION_WORKING'") -and
        $common.Contains("`$failures.Add('saveName')")
    'steam-only' = $common.Contains('$script:KmgSteamAppId = 640820') -and
        -not $orchestrator.Contains('Kingmaker.exe')
    'atomic-ready-and-result' = $result.Contains('stream.Flush(true)') -and
        $result.Contains('File.Replace(temporary, path, null)')
    'no-save-write-invocation' = -not ($scenario -match
        '\.(AutoSave|QuickSave|DeleteSave|RenameSave|MigrateSave|Overwrite)\s*\(')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Working-entry readiness tests failed: $($failed -join ', ')" }
Write-Host "Working-entry readiness tests passed: $($checks.Count)"
