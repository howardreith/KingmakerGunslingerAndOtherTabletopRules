[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$scenario = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'WorkingSaveSmokeScenario.cs')
$runner = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'RuntimeTestRunner.cs')
$request = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'RuntimeTestRequest.cs')
$catalog = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$result = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'RuntimeTestResult.cs')
$orchestrator = Get-Content -Raw -LiteralPath (
    Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1')
$common = Get-Content -Raw -LiteralPath (
    Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$entryStart = $scenario.IndexOf(
    'private void ResolveWorkingEntryAction()', [StringComparison]::Ordinal)
$entryEnd = $scenario.IndexOf(
    'private static LoadGameButtonCandidateEvidence ButtonEvidence',
    $entryStart, [StringComparison]::Ordinal)
$entryResolver = $scenario.Substring($entryStart, $entryEnd - $entryStart)

$checks = [ordered]@{
    'guarded-request-only' =
        $catalog.Contains('ObserveWorkingSaveEntryAction') -and
        $runner.Contains('RuntimeTestRequestParser.TryActivate')
    'working-name-required' =
        $request.Contains('workingEntryObservation') -and
        $request.Contains('"save-name-required"')
    'only-working-accepted' =
        $request.Contains('ManualSaveLoadObservation.WorkingSave')
    'baseline-rejected' =
        $request.Contains('"baseline-save-forbidden"') -and
        $scenario.Contains('ForbiddenName = "KMG_AUTOMATION_BASELINE"')
    'complete-catalog-required' =
        $scenario.Contains('_catalogComplete') -and
        $scenario.Contains('IsSaveInfoList(_catalogObject.GetType())')
    'one-working-required' =
        $runner.Contains('_workingSaveSmoke.WorkingCount > 1') -and
        $runner.Contains('_workingSaveSmoke.WorkingCount == 0')
    'entry-reference-correlation' =
        $entryResolver.Contains('ObjectContainsReference(component, _workingDescriptor)')
    'one-entry-required' =
        $entryResolver.Contains('_entryCandidates = entries.Count') -and
        $entryResolver.Contains('if (entries.Count != 1) return')
    'one-action-required' =
        $entryResolver.Contains('_entryActionCandidates = matches.Count') -and
        $entryResolver.Contains('if (matches.Count != 1) return')
    'other-save-actions-excluded' =
        $entryResolver.Contains('_workingDescriptor') -and
        -not $entryResolver.Contains('ForbiddenName')
    'visible-text-insufficient' =
        -not $entryResolver.Contains('SafeLabelIdentities')
    'banner-after-readiness' =
        $runner.Contains('_workingSaveSmoke.WorkingEntryReady') -and
        $runner.Contains('? "working-entry-ready"') -and
        $orchestrator.Contains('CLICK LOAD ON KMG_AUTOMATION_WORKING ONCE NOW')
    'pre-click-readiness-not-load-invocation' =
        $scenario.Contains('_stage == "working-entry-readiness"') -and
        $scenario.Contains('? "receiver-bound-action-invocation"') -and
        $scenario.Contains(': "working-entry-click"') -and
        $scenario.IndexOf(': "working-entry-click"') -lt
            $scenario.IndexOf('if (_stage == "working-entry-click" && _loadEntryInvocations == 1)')
    'readiness-allows-post-click-action-proof' =
        $scenario.Contains('_observeSelectionLoadAction || _entryActionCandidates <= 1') -and
        $scenario.Contains('_loadEntry != null') -and
        $runner.Contains('RuntimeTestStatuses.Ambiguous') -and
        $runner.Contains('"entry-action-correlation"')
    'readiness-marker-save-identity' =
        $result.Contains('[JsonProperty("saveName", Order = 14)]') -and
        $runner.Contains('WorkingSaveSmokeScenario.ExpectedName') -and
        $common.Contains("`$Marker.saveName -cne 'KMG_AUTOMATION_WORKING'") -and
        $common.Contains("`$failures.Add('saveName')")
    'click-timeout-post-readiness' =
        $scenario.Contains('stage == "working-entry-click"') -and
        $runner.Contains('return _request.SelectionTimeoutSeconds')
    'click-before-load-distinguished' =
        $scenario.Contains('Transition("load-entry-invocation"') -and
        $runner.Contains('return _request.LoadEntryTimeoutSeconds')
    'load-completion-timeout-distinguished' =
        $runner.Contains('if (stage == "load-completion")') -and
        $runner.Contains('return _request.CompletionTimeoutSeconds')
    'observer-never-invokes-entry-action' =
        -not ($entryResolver -match '\.Invoke\s*\(') -and
        $result.Contains('probeInvokedEntryAction')
    'observer-does-not-mutate-event' =
        -not ($entryResolver -match
            '(AddListener|RemoveListener|RemoveAllListeners|SetPersistentListenerState)')
    'human-invocation-exactly-once' =
        $scenario.Contains('_humanActionInvocations == 1')
    'listener-target-method-captured' =
        $scenario.Contains('_listenerTarget = listeners[0].Target') -and
        $scenario.Contains('_listenerMethod = listeners[0].Method')
    'descriptor-through-load-entry' =
        $scenario.Contains('ReferenceEquals(argument, _workingDescriptor)') -and
        $scenario.Contains('_observedLoadReceiver = receiver')
    'wrong-descriptor-fails' =
        $runner.Contains('RuntimeTestStatuses.Fail') -and
        $runner.Contains('"forbidden-save-selection"') -and
        $scenario.Contains('_baselineLoadObserved = IsBaseline(argument)') -and
        $scenario.Contains('_otherLoadObserved = !_baselineLoadObserved')
    'completion-and-fingerprint-required' =
        $scenario.Contains('_completionCallback && _stableSamples >= 2')
    'no-deliberate-save-write' =
        -not ($entryResolver -match
            '\.(Save|AutoSave|QuickSave|Delete|Rename|Migrate|Overwrite)\s*\(')
    'existing-scenarios-retained' =
        $catalog.Contains('ObserveManualSaveLoad') -and
        $catalog.Contains('ObserveLoadGameButtonAction') -and
        $catalog.Contains('WorkingSaveSmoke')
    'steam-640820-mandatory' =
        $common.Contains('$script:KmgSteamAppId = 640820') -and
        -not $orchestrator.Contains('Kingmaker.exe')
    'single-deployment-call' =
        ([regex]::Matches($orchestrator,
            'Deploy-Local\.ps1''\) `\r?\n    -PackagePath \$package -Confirm')).Count -eq 1
    'whatif-before-build-deploy-launch' =
        $orchestrator.IndexOf('$PSCmdlet.ShouldProcess(') -lt
        $orchestrator.IndexOf("Build-Local.ps1'")
    'atomic-evidence' =
        $result.Contains('stream.Flush(true)') -and
        $result.Contains('File.Replace(temporary, path, null)')
    'request-scoped-hook-removal' =
        $scenario.Contains('Unpatch(method, HarmonyPatchType.All,') -and
        $scenario.Contains('if (ReferenceEquals(_active, this)) _active = null')
    'manual-interaction-required' =
        $common.Contains("'observe-working-save-entry-action' = [pscustomobject]@{") -and
        $common.Contains('RequiresManualInteraction = $true') -and
        $orchestrator.Contains('-EnforceManualInteraction')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count) {
    throw "Working-save entry-action observation tests failed: $($failed -join ', ')"
}
Write-Host "Working-save entry-action observation tests passed: $($checks.Count)"
