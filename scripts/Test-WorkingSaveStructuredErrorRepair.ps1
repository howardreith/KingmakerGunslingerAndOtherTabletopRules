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
$result = Get-Content -Raw -LiteralPath (
    Join-Path $runtime 'RuntimeTestResult.cs')
$fixture = Get-Content -Raw -LiteralPath (
    Join-Path $root 'tests\fixtures\working-save-structured-main-menu-receiver-error.json') |
    ConvertFrom-Json

$resolverStart = $scenario.IndexOf(
    'private static object ResolveLoadEntryReceiver(',
    [StringComparison]::Ordinal)
$resolverEnd = $scenario.IndexOf(
    'private static void AddTypedMembers(', $resolverStart,
    [StringComparison]::Ordinal)
$resolver = $scenario.Substring($resolverStart, $resolverEnd - $resolverStart)

$checks = [ordered]@{
    'failed-result-reproduces-last-success' =
        $fixture.status -eq 'ERROR' -and
        $fixture.lastCompletedLifecycleStage -eq 'hooks-install-complete' -and
        $fixture.errorStage -eq 'main-menu-readiness'
    'failed-result-reproduces-operation' =
        $fixture.failingOperation -eq 'ResolveLoadEntryReceiver' -and
        $fixture.exceptionType -eq 'System.MissingMemberException' -and
        $fixture.staticMainMenuReceiverCount -eq 0 -and
        $fixture.directGameMainMenuReceiverCount -eq 0
    'failed-result-remained-structured-error' =
        $fixture.exceptionMessage -eq
            'The exact Kingmaker.MainMenu load-entry receiver could not be resolved.' -and
        $fixture.runtimeReadinessReached -eq $false -and
        $fixture.hooksRemoved -eq $true
    'repair-uses-proven-lifecycle-root' =
        $scenario.Contains('_mainMenuButtons as Component') -and
        $resolver.Contains('Root(lifecycleReceiver.transform)') -and
        $resolver.Contains('GetComponentsInChildren(expectedType, true)')
    'repair-requires-exact-runtime-type' =
        $resolver.Contains('expectedType.IsAssignableFrom(component.GetType())')
    'repair-requires-unique-receiver' =
        $resolver.Contains('matches.Count > 1') -and
        $resolver.Contains('matches.Count == 1 ? matches[0] : null')
    'fixture-proves-repair-can-advance' =
        $fixture.sameRootMainMenuReceiverCount -eq 1 -and
        $fixture.mainMenuTypeBaseType -eq 'UnityEngine.MonoBehaviour'
    'normal-load-handler-preserved' =
        $scenario.Contains('private static void Prefix(') -and
        -not $scenario.Contains('private static bool Prefix(')
    'baseline-denial-retained' =
        $scenario.Contains('ForbiddenName = "KMG_AUTOMATION_BASELINE"') -and
        $scenario.Contains('_workingCount == 1 && _baselineCount == 1')
    'unknown-or-ambiguous-cannot-pass' =
        $runner.Contains('_workingSaveSmoke.WorkingCount > 1') -and
        $runner.Contains('_workingSaveSmoke.WorkingCount == 0')
    'action-at-most-once' = $scenario.Contains('_buttonInvocations != 1')
    'load-entry-at-most-once' = $scenario.Contains('_loadEntryInvocations != 1')
    'resolution-before-load' =
        $scenario.IndexOf('ResolveDescriptors();', [StringComparison]::Ordinal) -lt
        $scenario.IndexOf('_loadEntry.Invoke(_loadEntryReceiver',
            [StringComparison]::Ordinal)
    'completion-and-fingerprint-required' =
        $scenario.Contains('_completionCallback && _stableSamples >= 2')
    'no-deliberate-save-write' =
        -not ($scenario -match '\.(Save|AutoSave|QuickSave)\s*\(')
    'structured-stage-fields-exist' =
        @('ErrorStage', 'LastCompletedStage', 'ExceptionType',
          'ExceptionMessage', 'ExceptionStack', 'ExceptionManagedThreadId') |
        ForEach-Object { $result.Contains($_) } |
        Where-Object { -not $_ } | Measure-Object |
        Select-Object -ExpandProperty Count | ForEach-Object { $_ -eq 0 }
    'startup-error-schema-populated' =
        $runner.Contains('result.ErrorStage = _workingSaveSmoke == null') -and
        $runner.Contains('result.ExceptionManagedThreadId =') -and
        $runner.Contains('SanitizeExceptionStack(exception)') -and
        $runner.Contains('result.WorkingSaveSmoke = _workingSaveSmoke.Stop()')
    'future-error-lifecycle-facts' =
        @('HooksInstalled', 'UiActionOccurred', 'DescriptorResolved',
          'LoadingBegan', 'LoadingCompleted', 'SaveWritingApiObserved') |
        ForEach-Object { $result.Contains($_) } |
        Where-Object { -not $_ } | Measure-Object |
        Select-Object -ExpandProperty Count | ForEach-Object { $_ -eq 0 }
    'hooks-removed-after-error' =
        $runner.Contains('result.WorkingSaveSmoke = _workingSaveSmoke.Stop()')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Working-save structured-error repair tests failed: $($failed -join ', ')"
}
Write-Host "Working-save structured-error repair tests passed: $($checks.Count)"
