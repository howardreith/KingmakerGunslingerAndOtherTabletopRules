[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$observer = Get-Content -Raw (Join-Path $runtime 'SaveCatalogSelectionObservation.cs')
$runner = Get-Content -Raw (Join-Path $runtime 'RuntimeTestRunner.cs')
$result = Get-Content -Raw (Join-Path $runtime 'RuntimeTestResult.cs')
$catalog = Get-Content -Raw (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$orchestrator = Get-Content -Raw (
    Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1')
$common = Get-Content -Raw (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
. (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')

$now = [DateTime]::UtcNow
$marker = [pscustomobject]@{
    schemaVersion = 1; runId = 'run'; scenario = 'observe-save-catalog-and-selection'
    stage = 'catalog-captured'; loadedModVersion = '0.0.30'
    timestampUtc = $now.ToString('o'); processId = 42
}
$checks = [ordered]@{
    'guarded-allowlist' = $catalog.Contains('ObserveSaveCatalogAndSelection') -and
        $runner.Contains('RuntimeTestRequestParser.TryActivate')
    'stage-a-after-hooks' = $runner.IndexOf('_catalogObservation.Ready') -lt
        $runner.IndexOf('"runtime-catalog-ready.json"')
    'first-banner-after-stage-a' = $orchestrator.IndexOf(
        'Test-KmgRuntimeStageMarker -Marker $stageA') -lt
        $orchestrator.IndexOf('OPEN THE LOAD GAME SCREEN NOW')
    'stage-b-requires-catalog' = $runner.Contains(
        'if (_catalogObservation.CatalogCaptured && !_catalogMarkerWritten)')
    'second-banner-after-stage-b' = $orchestrator.IndexOf(
        'Test-KmgRuntimeStageMarker -Marker $stageB') -lt
        $orchestrator.IndexOf('SAVE CATALOG CAPTURED')
    'valid-stage-marker' = Test-KmgRuntimeStageMarker -Marker $marker -RunId run `
        -Scenario observe-save-catalog-and-selection -Stage catalog-captured `
        -ExpectedVersion 0.0.30 -ProcessId 42 -RequestWrittenUtc $now.AddSeconds(-1)
    'stale-marker-rejected' = -not (Test-KmgRuntimeStageMarker -Marker $marker `
        -RunId run -Scenario observe-save-catalog-and-selection `
        -Stage catalog-captured -ExpectedVersion 0.0.30 -ProcessId 42 `
        -RequestWrittenUtc $now.AddSeconds(1))
    'mismatch-rejected' = -not (Test-KmgRuntimeStageMarker -Marker $marker `
        -RunId other -Scenario observe-save-catalog-and-selection `
        -Stage catalog-captured -ExpectedVersion 0.0.30 -ProcessId 42 `
        -RequestWrittenUtc $now.AddSeconds(-1))
    'deterministic-catalog' = $observer.Contains(
        'BuildIdentity(descriptor)') -and $observer.Contains('SHA256.Create()')
    'unrelated-minimized' = $observer.Contains(
        'classification == "unrelated" ? "" : name') -and $observer.Contains(
        '? new Dictionary<string, string>()')
    'unique-working-pass' = $runner.Contains(
        'evidence.WorkingMatchCount == 1')
    'zero-working-fails' = $runner.Contains(
        '_catalogObservation.WorkingCount == 0')
    'multiple-working-ambiguous' = $runner.Contains(
        '_catalogObservation.WorkingCount > 1')
    'baseline-denied' = $runner.Contains(
        '_catalogObservation.SelectedClassification == "baseline"')
    'selection-correlates' = $observer.Contains(
        'ReferenceEquals(x, descriptor)') -and $runner.Contains(
        'string.IsNullOrWhiteSpace(_catalogObservation.CorrelationMethod)')
    'other-save-fails' = $runner.Contains(
        '_catalogObservation.SelectedClassification == "other"')
    'completion-required' = $runner.Contains(
        '_catalogObservation.CompletionObserved')
    'fingerprint-required' = $runner.Contains(
        '_catalogObservation.StableFingerprintAvailable')
    'hooks-observe-only' = -not ($observer -match
        'private static void (Prefix|Postfix)\([^)]*(\bref\b|\bout\b|__result)')
    'no-load-invocation' = -not ($observer -match
        '\.Invoke\([^;]*(LoadGame|LoadRoutine)')
    'no-write-invocation' = -not ($observer -match
        '\.Invoke\([^;]*(Save|Delete|Rename|Migrate|Overwrite)')
    'hooks-removed' = $observer.Contains(
        'Unpatch(method, HarmonyPatchType.All, _context.ModId)')
    'stage-timeouts' = $runner.Contains('"catalog-capture"') -and
        $runner.Contains('"save-selection"') -and
        $runner.Contains('"load-completion"')
    'incremental-events' = $result.Contains('"runtime-events.json"') -and
        $result.Contains('WriteAtomic(')
    'manual-observer-isolated' = $runner.Contains('RunManualSaveLoadObservation()')
    'mod-smoke-isolated' = $runner.Contains('RuntimeTestScenarioCatalog.ModLoadSmoke')
    'steam-mandatory' = $common.Contains('$script:KmgSteamAppId = 640820')
    'direct-launch-absent' = -not $orchestrator.Contains('Kingmaker.exe')
    'atomic-stage-markers' = $result.Contains('WriteStage(') -and
        $result.Contains('RuntimeTestResultWriter.WriteAtomic(')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count) {
    throw "Save catalog observation tests failed: $($failed -join ', ')"
}
Write-Host "Save catalog observation tests passed: $($checks.Count)"
