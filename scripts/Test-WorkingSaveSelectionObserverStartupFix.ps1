[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$scenario = Get-Content -Raw -LiteralPath (Join-Path $runtime 'WorkingSaveSmokeScenario.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs')
$result = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestResult.cs')
$fixture = Get-Content -Raw -LiteralPath (Join-Path $root 'tests\fixtures\working-save-selection-observer-startup-failure.json') | ConvertFrom-Json

$discoveryStart = $scenario.IndexOf('private void DiscoverCandidateMethods')
$discoveryEnd = $scenario.IndexOf('private static bool ImplementsUiActionHandler', $discoveryStart)
$discovery = $scenario.Substring($discoveryStart, $discoveryEnd - $discoveryStart)
$recordStart = $scenario.IndexOf('private void RecordCandidateMethod')
$recordEnd = $scenario.IndexOf('private static void AddUniqueReference', $recordStart)
$record = $scenario.Substring($recordStart, $recordEnd - $recordStart)

$checks = [ordered]@{
    'real-failure-fixture' = $fixture.status -ceq 'ERROR' -and
        $fixture.method -like '*Selectable::get_currentSelectionState*'
    'original-operation-chain' = $fixture.operation -like '*Harmony patch installation*' -and
        $fixture.exceptionChain[-1] -ceq 'WorkingSaveSmokeScenario.ResolveWorkingSelectionLoadActions'
    'precise-startup-substage' = $scenario.Contains('working-selection-candidate-method-enumeration') -and
        $runner.Contains('ObserverArmingSubstage')
    'discovery-does-not-patch' = -not $discovery.Contains('Patch(') -and
        -not $record.Contains('Patch(')
    'keyword-names-not-authority' = -not $scenario.Contains('name.IndexOf("Select"') -and
        -not $scenario.Contains('name.IndexOf("Load"')
    'unity-framework-rejected' = $scenario.Contains('return "unity-framework-method"')
    'managed-body-required' = $scenario.Contains('method.GetMethodBody() == null')
    'unsafe-method-forms-rejected' = $scenario.Contains('MethodAttributes.PinvokeImpl') -and
        $scenario.Contains('MethodImplAttributes.InternalCall') -and
        $scenario.Contains('method.IsSpecialName')
    'optional-rejection-nonfatal' = $scenario.Contains('optional-action-candidate-rejected') -and
        $result.Contains('candidateRejections')
    'mandatory-install-cleanup' = $scenario.Contains('catch') -and
        $scenario.Contains('RemoveHooks();')
    'authoritative-hook-remains' = $scenario.Contains('Patch(_loadEntry, prefix, null)')
    'full-stack-recorded' = $scenario.Contains('new StackTrace(1, false)') -and
        $result.Contains('loadCallerChain')
    'one-receiver-required' = $scenario.Contains('receivers.Count == 1') -and
        $runner.Contains('evidence.CompatibleCallerReceiverCount == 1')
    'multiple-or-missing-ambiguous' = $scenario.Contains('_compatibleCallerReceiverCount = receivers.Count') -and
        $runner.Contains('RuntimeTestStatuses.Ambiguous')
    'readiness-before-action' = $scenario.IndexOf('working-selection-load-observer-ready') -lt
        $scenario.IndexOf('CaptureLoadCallerChain()')
    'load-completion-not-visual' = $scenario.Contains('_completionCallback && _stableSamples >= 2')
    'observer-noninitiating' = $scenario.Contains(
        'ProbeInvokedEntryAction = _autonomousReceiverBoundAction') -and
        $runner.Contains('receiverBoundObservation')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Working-save selection startup-fix tests failed: $($failed -join ', ')" }
Write-Host "Working-save selection startup-fix tests passed: $($checks.Count)"
