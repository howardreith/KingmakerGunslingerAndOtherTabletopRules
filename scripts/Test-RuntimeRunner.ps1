[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$request = Get-Content -LiteralPath (Join-Path $runtime 'RuntimeTestRequest.cs') -Raw
$result = Get-Content -LiteralPath (Join-Path $runtime 'RuntimeTestResult.cs') -Raw
$runner = Get-Content -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs') -Raw
$catalog = Get-Content -LiteralPath (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs') -Raw
$main = Get-Content -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Main.cs') -Raw

$checks = [ordered]@{
    'flag-required' = $request.Contains('reason = "flag-absent"')
    'missing-file-rejected' = $request.Contains('"request-file-missing"')
    'invalid-json-rejected' = $request.Contains('"invalid-json"')
    'unknown-scenario-rejected' = $request.Contains('"scenario-not-allowed"')
    'version-mismatch-rejected' = $request.Contains('"mod-version-mismatch"')
    'outside-root-rejected' = $request.Contains('"evidence-path-outside-root"')
    'run-id-validated' = $request.Contains('IsValidRunId(request.RunId)')
    'duplicate-run-id-rejected' = $request.Contains('"run-id-duplicate"')
    'reparse-point-rejected' = $request.Contains('"evidence-path-reparse-point"')
    'atomic-result-write' = $result.Contains('stream.Flush(true)') -and
        $result.Contains('File.Move(temporary, path)')
    'exception-to-error' = $runner.Contains('RuntimeTestStatuses.Error') -and
        $runner.Contains('CompleteStartupError(')
    'timeout-to-timeout' = $runner.Contains('CreateResult("TIMEOUT"')
    'exit-is-request-guarded' = $runner.Contains('if (_request.ExitAfterCompletion)') -and
        $runner.Contains('Application.Quit()')
    'production-allowlist-includes-observer' = $catalog.Contains(
        'ObserveManualSaveLoad')
    'real-runtime-identity' = $runner.Contains('Assembly.GetExecutingAssembly()') -and
        $runner.Contains('Process.GetCurrentProcess().Id')
    'main-thread-update-callback' = $runner.Contains('OnUpdate += runner.OnUpdate')
    'bootstrap-attachment-after-loaded' = $main.IndexOf(
        '_state = LoaderState.Loaded;', [StringComparison]::Ordinal) -lt
        $main.IndexOf('RuntimeTestRunner.TryAttach(context);', [StringComparison]::Ordinal)
    'non-save-scenario-isolated' = $runner.Contains(
        'if (_request.Scenario == RuntimeTestScenarioCatalog.ModLoadSmoke)')
    'lightning-reload-guarded-dispatch' = $runner.Contains(
        'RunDisposableGunslingerLightningReload()') -and $catalog.Contains(
        'DisposableGunslingerLightningReload')
    'evasive-native-observer-is-exact' = $runner.Contains(
        'RunObserveEvasiveNativeFeatures()') -and $runner.Contains(
        '576933720c440aa4d8d42b0c54b77e80') -and $catalog.Contains(
        'ObserveEvasiveNativeFeatures')
    'evasive-guarded-dispatch' = $runner.Contains(
        'RunDisposableGunslingerEvasive()') -and $catalog.Contains(
        'DisposableGunslingerEvasive')
    'menacing-native-observer-is-exact' = $runner.Contains(
        'RunObserveMenacingShotNativeFear()') -and $runner.Contains(
        'value.name == "Fear"') -and $catalog.Contains(
        'ObserveMenacingShotNativeFear')
    'write-failure-suppresses-exit' = $runner.Contains(
        'automatic exit was suppressed')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Runtime runner safety tests failed: $($failed -join ', ')"
}
Write-Host "Runtime runner source safety tests passed: $($checks.Count)"
