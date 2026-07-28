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
    'timeout-status' = $runner.Contains('CreateResult("TIMEOUT"')
    'accepted-result-sealed' = $observer.Contains('if (_sealed) return') -and
        $observer.Contains('if (_acceptedName != null')
    'scenario-scoped-install' = $runner.Contains(
        '_saveLoadObservation = new ManualSaveLoadObservation') -and
        $runner.Contains(
        'if (_request.Scenario == RuntimeTestScenarioCatalog.ModLoadSmoke)')
    'atomic-result' = $result.Contains('stream.Flush(true)') -and
        $result.Contains('File.Move(temporary, path)')
    'no-raw-object-dumps' = -not $observer.Contains('.ToString()')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Manual save-load observation tests failed: $($failed -join ', ')"
}
Write-Host "Manual save-load observation tests passed: $($checks.Count)"
