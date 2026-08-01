[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$scenario = Get-Content -Raw -LiteralPath (Join-Path $runtime 'WorkingSaveSmokeScenario.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs')
$result = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestResult.cs')
$catalog = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$orchestrator = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1')
$common = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$fixtures = @(
    Get-Content -Raw -LiteralPath (Join-Path $root 'tests\fixtures\working-save-entry-action-ambiguous-run-1.json') | ConvertFrom-Json
    Get-Content -Raw -LiteralPath (Join-Path $root 'tests\fixtures\working-save-entry-action-ambiguous-run-2.json') | ConvertFrom-Json
)
$selectionStart = $scenario.IndexOf('private void ResolveWorkingSelectionLoadActions()')
$selectionEnd = $scenario.IndexOf('private void DiscoverCandidateMethods', $selectionStart)
$selectionResolver = $scenario.Substring($selectionStart, $selectionEnd - $selectionStart)

$checks = [ordered]@{
    'both-real-runs-regressed' = $fixtures.Count -eq 2 -and
        @($fixtures | Where-Object status -ceq 'AMBIGUOUS').Count -eq 2 -and
        @($fixtures | Where-Object finalInvariant -ceq 'entry-action-correlation').Count -eq 2
    'entry-local-absence-not-no-action' = @($fixtures | Where-Object {
        $_.entryActionCandidateCount -eq 0 -and $_.loadEntryInvocationCount -eq 1 }).Count -eq 2
    'shared-action-needs-selected-reference' = $scenario.Contains(
        '_selectedWorkingStateObserved &&') -and $scenario.Contains(
        'ReferenceEquals(value, _workingDescriptor)')
    'selection-and-load-recorded-separately' = $result.Contains('selectedSaveStorage') -and
        $result.Contains('immediateLoadCaller')
    'direct-entry-not-assumed' = $scenario.Contains('ResolveWorkingSelectionLoadActions()') -and
        $scenario.Contains('GetComponentsInChildren<Button>(true)')
    'visible-text-not-identity' = $runner.Contains('visible-text-not-identity') -and
        -not $selectionResolver.Contains('SafeLabelIdentities')
    'baseline-always-fails' = $runner.Contains('_workingSaveSmoke.BaselineLoadObserved') -and
        $runner.Contains('RuntimeTestStatuses.Fail')
    'other-save-always-fails' = $runner.Contains('_workingSaveSmoke.OtherLoadObserved')
    'exactly-one-working' = $runner.Contains('_workingSaveSmoke.WorkingCount > 1') -and
        $runner.Contains('_workingSaveSmoke.WorkingCount == 0')
    'exactly-one-final-action' = $scenario.Contains('_finalLoadActionCount == 1')
    'exact-load-object-reference' = $scenario.Contains('ReferenceEquals(argument, _workingDescriptor)')
    'loading-screen-not-completion' = $scenario.Contains('_completionCallback && _stableSamples >= 2')
    'callback-and-fingerprint-mandatory' = @($fixtures | Where-Object {
        $_.completionCallbackObserved -and $_.stableFingerprint }).Count -eq 2
    'result-flush-before-exit' = $result.Contains('stream.Flush(true)') -and
        $runner.IndexOf('WriteLifecycleStage("final-result-flushed")') -lt
        $runner.IndexOf('Application.Quit();')
    'no-deliberate-save-write' = -not ($scenario -match
        '\.(AutoSave|QuickSave|DeleteSave|RenameSave|MigrateSave|Overwrite)\s*\(')
    'observers-remain-non-initiating' = $scenario.Contains('ProbeInvokedEntryAction = false') -and
        $runner.Contains('observer invokes neither selection nor loading')
    'steam-app-id' = $common.Contains('$script:KmgSteamAppId = 640820')
    'single-deployment-backup' = ([regex]::Matches($orchestrator,
        'Deploy-Local\.ps1''\) `\r?\n    -PackagePath \$package -Confirm')).Count -eq 1
    'whatif-precedes-mutation' = $orchestrator.IndexOf('$PSCmdlet.ShouldProcess(') -lt
        $orchestrator.IndexOf("Build-Local.ps1'")
    'atomic-evidence' = $result.Contains('File.Replace(temporary, path, null)')
    'scenario-allowlisted' = $catalog.Contains('ObserveWorkingSaveSelectionLoadAction') -and
        $common.Contains("'observe-working-save-selection-load-action' = [pscustomobject]@{")
    'exact-slot-owner-scope' = $scenario.Contains('ReferenceEquals(component, _entryOwner)') -and
        $scenario.Contains('typeName == CatalogType')
    'caller-chain-captured' = $scenario.Contains('new StackTrace(1, false)') -and
        $result.Contains('loadCallerChain')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Working-save ambiguity tests failed: $($failed -join ', ')" }
Write-Host "Working-save ambiguity tests passed: $($checks.Count)"
