[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$observer = Get-Content -LiteralPath (
    Join-Path $runtime 'ManualSaveLoadObservation.cs') -Raw
$runner = Get-Content -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs') -Raw
$catalog = Get-Content -LiteralPath (
    Join-Path $runtime 'RuntimeTestScenarioCatalog.cs') -Raw
$result = Get-Content -LiteralPath (Join-Path $runtime 'RuntimeTestResult.cs') -Raw
$orchestrator = Get-Content -LiteralPath (
    Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1') -Raw
$common = Get-Content -LiteralPath (
    Join-Path $root 'scripts\RuntimeAutomation.Common.ps1') -Raw
. (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$fixture = Get-Content -LiteralPath (
    Join-Path $root 'tests\fixtures\save-load-observation-timeout.json') -Raw |
    ConvertFrom-Json
$now = [DateTime]::UtcNow
$marker = [pscustomobject]@{
    schemaVersion = 1; runId = 'current-run'; scenario = 'observe-manual-save-load'
    loadedModVersion = '0.0.30'; processId = 42
    readinessTimestampUtc = $now.ToString('o')
    installedObservationHookIdentifiers = @('Kingmaker.Game.LoadGame(...)')
}

$checks = [ordered]@{
    'guarded-allowlist-only' = $catalog.Contains('ObserveManualSaveLoad') -and
        $runner.Contains('RuntimeTestRequestParser.TryActivate')
    'baseline-always-fails' = $observer.Contains(
        'string.Equals(identity, BaselineSave') -and
        $observer.Contains('_identityRejected = true')
    'unknown-is-not-pass' = $observer.Contains('_identityAmbiguous = true')
    'working-only-pass' = $observer.Contains('_acceptedName == WorkingSave') -and
        $runner.Contains(
        'evidence.AcceptedSaveName == ManualSaveLoadObservation.WorkingSave')
    'no-load-api-invocation' = -not ($observer -match
        '\.Invoke\([^;]*(LoadGame|LoadRoutine)')
    'no-write-api-invocation' = -not ($observer -match
        '\.Invoke\([^;]*(Save|AutoSave|QuickSave|Delete|Rename|Migrate|Overwrite)')
    'patches-observe-only' = $observer.Contains(
        'private static void ObservePrefix(MethodBase __originalMethod, object[] __args)') -and
        $observer.Contains(
        'private static void ObservePostfix(MethodBase __originalMethod, object[] __args)') -and
        -not ($observer -match
            'Observe(Prefix|Postfix)\([^)]*(\bref\b|\bout\b|__result)')
    'patches-removed' = $observer.Contains(
        'Unpatch(method, HarmonyPatchType.All, _context.ModId)')
    'completion-required' = $observer.Contains(
        '_completionCallback && _stableSamples >= 2')
    'load-start-recorded' = $observer.Contains(
        '_loadStartUtc = DateTime.UtcNow.ToString("o")')
    'completion-event-recorded' = $observer.Contains(
        '_loadCompletionUtc = DateTime.UtcNow.ToString("o")') -and
        $observer.Contains('"load-completion-callback"')
    'game-thread-enforced' = $observer.Contains(
        'Thread.CurrentThread.ManagedThreadId != _gameThreadManagedId') -and
        $runner.Contains('Assertion("game-thread-only"')
    'scene-area-fingerprint' = $observer.Contains(
        'ReadMember(game, "CurrentlyLoadedArea")') -and
        $observer.Contains('ReadMember(game, "CurrentScene")')
    'stable-player-party-fingerprint' = $observer.Contains(
        'ReadMember(player, "Party")') -and
        $observer.Contains('ReadMember(player, "MainCharacter")')
    'timeout-status' = $runner.Contains('CreateResult("TIMEOUT"')
    'accepted-result-sealed' = $observer.Contains('if (_sealed) return') -and
        $observer.Contains('if (_acceptedName != null')
    'scenario-scoped-install' = $runner.Contains(
        '_saveLoadObservation = new ManualSaveLoadObservation') -and
        $runner.Contains(
        'if (_request.Scenario == RuntimeTestScenarioCatalog.ModLoadSmoke)')
    'atomic-result' = $result.Contains('stream.Flush(true)') -and
        $result.Contains('File.Move(temporary, path)')
    'structured-timestamps' = $result.Contains(
        'JsonProperty("loadStartUtc"') -and
        $result.Contains('JsonProperty("loadCompletionUtc"')
    'no-raw-object-dumps' = -not $observer.Contains('.ToString()')
    'banner-after-ready-wait' = $orchestrator.IndexOf(
        'Test-KmgRuntimeReadyMarker', [StringComparison]::Ordinal) -lt
        $orchestrator.IndexOf(
        'MANUALLY LOAD KMG_AUTOMATION_WORKING NOW', [StringComparison]::Ordinal)
    'manual-timeout-after-readiness' = $orchestrator.IndexOf(
        '$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds + 15)',
        [StringComparison]::Ordinal) -gt $orchestrator.IndexOf(
        'Test-KmgRuntimeReadyMarker', [StringComparison]::Ordinal)
    'valid-ready-marker-accepted' = Test-KmgRuntimeReadyMarker -Marker $marker `
        -RunId 'current-run' -Scenario 'observe-manual-save-load' `
        -ExpectedVersion '0.0.30' -ProcessId 42 -RequestWrittenUtc $now.AddSeconds(-1)
    'stale-ready-marker-rejected' = -not (Test-KmgRuntimeReadyMarker -Marker $marker `
        -RunId 'current-run' -Scenario 'observe-manual-save-load' `
        -ExpectedVersion '0.0.30' -ProcessId 42 -RequestWrittenUtc $now.AddSeconds(1))
    'mismatched-run-rejected' = -not (Test-KmgRuntimeReadyMarker -Marker $marker `
        -RunId 'different-run' -Scenario 'observe-manual-save-load' `
        -ExpectedVersion '0.0.30' -ProcessId 42 -RequestWrittenUtc $now.AddSeconds(-1))
    'readiness-stage-specific' = $runner.Contains(
        'timeoutStage=" + _workingStartupStage') -and $orchestrator.Contains(
        'stage=observer-readiness')
    'failed-run-fixture-reproduced' = $fixture.classification -eq 'E' -and
        $fixture.observerInstalled -and $fixture.loadStartObserved -and
        $fixture.acceptedSaveName -eq 'KMG_AUTOMATION_WORKING' -and
        -not $fixture.completionCallbackRegistered -and
        -not $fixture.completionCallbackObserved
    'field-backed-save-manager-repaired' = $observer.Contains(
        'ReadMember(game, "SaveManager")') -and $observer.Contains(
        'GetField(')
    'incremental-events-atomic' = $result.Contains(
        '"runtime-events.json"') -and $result.Contains(
        'RuntimeTestResultWriter.WriteAtomic(')
    'runtime-exceptions-are-error' = $runner.Contains(
        '_trace.Record("runtime-exception"') -and $runner.Contains(
        'RuntimeTestStatuses.Error')
    'ready-marker-is-atomic' = $result.Contains(
        '"runtime-ready.json"') -and $result.Contains(
        'WriteReady(RuntimeReadyMarker marker)')
    'run-id-on-every-event' = $result.Contains(
        'value.RunId = _runId')
    'non-observation-no-hooks' = $runner.IndexOf(
        'if (_request.Scenario == RuntimeTestScenarioCatalog.ModLoadSmoke)',
        [StringComparison]::Ordinal) -lt $runner.IndexOf(
        'RunManualSaveLoadObservation();', [StringComparison]::Ordinal)
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Manual save-load observation tests failed: $($failed -join ', ')"
}
Write-Host "Manual save-load observation tests passed: $($checks.Count)"
