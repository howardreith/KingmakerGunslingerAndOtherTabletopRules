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
    Join-Path $root 'tests\fixtures\working-save-post-readiness-target-exception.json') |
    ConvertFrom-Json

$checks = [ordered]@{
    'failed-condition-fixture' =
        $fixture.errorStage -eq 'load-entry-invocation' -and
        $fixture.exceptionType -eq 'System.Reflection.TargetException' -and
        $fixture.mainMenuLoadGameEntered -eq $false
    'separate-receivers' =
        $scenario.Contains('_mainMenuButtons') -and
        $scenario.Contains('_loadEntryReceiver')
    'typed-load-receiver' =
        $scenario.Contains('expectedType.IsAssignableFrom') -and
        $scenario.Contains('_loadEntry.Invoke(_loadEntryReceiver')
    'exact-saveinfo-through-entry' =
        $scenario.Contains('new[] { _workingDescriptor }') -and
        $scenario.Contains('ReferenceEquals(argument, _workingDescriptor)')
    'one-action-and-one-load' =
        $scenario.Contains('_buttonInvocations != 1') -and
        $scenario.Contains('_loadEntryInvocations != 1')
    'no-duplicate-higher-load-path' =
        -not ($scenario -match 'SaveLoadWindow.*\\.Invoke|LoadRoutine.*\\.Invoke')
    'no-invented-ui-transition' =
        -not ($scenario -match '(?i)CloseWindow|HideWindow|ui-transition-start')
    'ui-hooks-before-load' =
        $scenario.IndexOf('RemoveUiHooks();', [StringComparison]::Ordinal) -lt
        $scenario.IndexOf('_loadEntry.Invoke(_loadEntryReceiver',
            [StringComparison]::Ordinal)
    'ui-references-released' =
        $scenario.Contains('_button = null;') -and
        $scenario.Contains('_mainMenuButtons = null;') -and
        $scenario.Contains('_catalogObject = null;')
    'immutable-descriptor-evidence' =
        $scenario.Contains('DescriptorEvidence(_workingDescriptor') -and
        $scenario.Contains('immutable scalar descriptor evidence captured')
    'after-load-required' =
        $scenario.Contains('_completionCallback && _stableSamples >= 2')
    'fingerprint-required' =
        $scenario.Contains('if (_stage == "post-load-fingerprint") PollFingerprint()')
    'loading-overlay-not-completion' =
        -not ($scenario -match '(?i)loading.?screen.*Complete')
    'post-readiness-error-contained' =
        $runner.Contains('CompletePostReadinessError(exception)')
    'error-schema-complete' =
        @('ErrorStage', 'LastCompletedStage', 'ExceptionType',
          'ExceptionMessage', 'ExceptionStack', 'ExceptionManagedThreadId') |
        ForEach-Object { $result.Contains($_) } |
        Where-Object { -not $_ } | Measure-Object |
        Select-Object -ExpandProperty Count | ForEach-Object { $_ -eq 0 }
    'atomic-required-stages' =
        @('load-game-action-invoke-start', 'load-game-action-invoked',
          'catalog-enter', 'catalog-complete', 'descriptor-resolution-start',
          'working-descriptor-resolved', 'baseline-excluded',
          'load-entry-start', 'load-entry-complete', 'after-load-callback',
          'fingerprint-start', 'fingerprint-complete',
          'ui-hooks-removed', 'post-readiness-error', 'final-result-flushed') |
        ForEach-Object { ($scenario + $runner).Contains($_) } |
        Where-Object { -not $_ } | Measure-Object |
        Select-Object -ExpandProperty Count | ForEach-Object { $_ -eq 0 }
    'no-save-writing-call' =
        -not ($scenario -match '\.(Save|AutoSave|QuickSave)\s*\(')
    'exit-after-flush' =
        $runner.IndexOf('RuntimeTestResultWriter.Write',
            [StringComparison]::Ordinal) -lt
        $runner.IndexOf('Application.Quit();', [StringComparison]::Ordinal)
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Working-save post-readiness repair tests failed: $($failed -join ', ')"
}
Write-Host "Working-save post-readiness repair tests passed: $($checks.Count)"
