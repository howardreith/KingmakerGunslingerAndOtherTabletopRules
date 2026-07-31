[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$scenario = Get-Content -Raw -LiteralPath (
    Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\WorkingSaveSmokeScenario.cs')
$fixture = Get-Content -Raw -LiteralPath (
    Join-Path $root 'tests\fixtures\working-save-deferred-load-entry-receiver-error.json') |
    ConvertFrom-Json

$pollStart = $scenario.IndexOf('internal void Poll()', [StringComparison]::Ordinal)
$stopStart = $scenario.IndexOf(
    'internal WorkingSaveSmokeEvidence Stop()', $pollStart,
    [StringComparison]::Ordinal)
$poll = $scenario.Substring($pollStart, $stopStart - $pollStart)
$mainMenuStage = $poll.Substring(
    $poll.IndexOf('if (_stage == "main-menu-readiness")',
        [StringComparison]::Ordinal),
    $poll.IndexOf('if (_stage == "load-game-action-resolution")',
        [StringComparison]::Ordinal) -
    $poll.IndexOf('if (_stage == "main-menu-readiness")',
        [StringComparison]::Ordinal))
$loadEntryStage = $poll.Substring(
    $poll.IndexOf('if (_stage == "load-entry-invocation")',
        [StringComparison]::Ordinal),
    $poll.IndexOf('if (_stage == "load-completion"',
        [StringComparison]::Ordinal) -
    $poll.IndexOf('if (_stage == "load-entry-invocation")',
        [StringComparison]::Ordinal))

$checks = [ordered]@{
    'fixture-reproduces-structured-scenario-error' =
        $fixture.runtimeStatus -eq 'ERROR' -and
        $fixture.orchestrationStage -eq 'final-result-received' -and
        $fixture.errorStage -eq 'main-menu-readiness'
    'fixture-reproduces-first-failed-operation' =
        $fixture.failingOperation -eq 'ResolveLoadEntryReceiver' -and
        $fixture.exceptionType -eq 'System.MissingMemberException'
    'fixture-proves-no-action-or-load' =
        $fixture.uiActionOccurred -eq $false -and
        $fixture.loadingBegan -eq $false -and
        $fixture.loadingCompleted -eq $false
    'fixture-proves-clean-hook-removal' =
        $fixture.hooksInstalled -eq $true -and
        $fixture.hooksRemoved -eq $true
    'pre-action-stage-does-not-resolve-load-receiver' =
        -not $mainMenuStage.Contains('ResolveLoadEntryReceiver(')
    'pre-action-stage-still-requires-exact-menu-lifecycle' =
        $mainMenuStage.Contains('ResolveMainMenu();') -and
        $mainMenuStage.Contains('if (_mainMenuButtons != null)')
    'readiness-still-requires-exact-button' =
        $scenario.Contains(
            'return _mainMenuButtons != null && _button != null &&') -and
        $scenario.Contains('_buttonCandidates == 1 && _stage == "action-invocation"')
    'load-receiver-resolved-after-descriptor-gates' =
        $loadEntryStage.Contains('ResolveLoadEntryReceiver(') -and
        $poll.IndexOf('ResolveDescriptors();', [StringComparison]::Ordinal) -lt
        $poll.IndexOf('_loadEntryReceiver = ResolveLoadEntryReceiver(',
            [StringComparison]::Ordinal)
    'load-receiver-required-before-ui-hook-removal' =
        $loadEntryStage.IndexOf('if (_loadEntryReceiver == null)',
            [StringComparison]::Ordinal) -lt
        $loadEntryStage.IndexOf('RemoveUiHooks();', [StringComparison]::Ordinal)
    'same-catalog-reference-correlation-retained' =
        $loadEntryStage.Contains(
            '_descriptorCorrelated = ContainsReference(') -and
        $loadEntryStage.Contains(
            '_loadEntry.Invoke(_loadEntryReceiver, new[] { _workingDescriptor })')
    'baseline-denial-retained' =
        $scenario.Contains('ForbiddenName = "KMG_AUTOMATION_BASELINE"') -and
        $scenario.Contains('_workingCount == 1 && _baselineCount == 1')
    'no-deliberate-save-writing' =
        -not ($scenario -match '\.(Save|AutoSave|QuickSave)\s*\(')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Working-save deferred load-entry receiver tests failed: $($failed -join ', ')"
}
Write-Host "Working-save deferred load-entry receiver tests passed: $($checks.Count)"
