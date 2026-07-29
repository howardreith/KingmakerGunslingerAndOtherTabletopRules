[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$observer = Get-Content -Raw (
    Join-Path $runtime 'SaveCatalogProviderObservation.cs')
$runner = Get-Content -Raw (Join-Path $runtime 'RuntimeTestRunner.cs')
$result = Get-Content -Raw (Join-Path $runtime 'RuntimeTestResult.cs')
$catalog = Get-Content -Raw (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$orchestrator = Get-Content -Raw (
    Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1')
$common = Get-Content -Raw (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')

$checks = [ordered]@{
    'guarded-request-only' = $catalog.Contains('ObserveSaveCatalogProvider') -and
        $runner.Contains('RuntimeTestRequestParser.TryActivate')
    'ready-after-hooks' = $runner.IndexOf('_catalogProviderObservation.Ready') -lt
        $runner.IndexOf('"catalog provider hooks active on game thread"')
    'banner-after-ready-validation' = $orchestrator.IndexOf(
        'Test-KmgRuntimeReadyMarker -Marker $candidate') -lt
        $orchestrator.IndexOf('Write-Host $providerInstruction')
    'stale-ready-rejected' = $orchestrator.Contains(
        'Provider ready marker is stale or mismatched.')
    'exact-initialize-only' = $observer.Contains(
        '"Kingmaker.UI.SaveLoadWindow.ListOfSaves"') -and
        $observer.Contains('method.Name == "Initialize"') -and
        $observer.Contains('parameters.Length == 2')
    'minimized-deterministic-chain' = $observer.Contains(
        'OrderBy(x => x.FullName, StringComparer.Ordinal)') -and
        $observer.Contains('if (chain.Count == 12) break')
    'candidate-signature-filter' = $observer.Contains(
        'IsSaveInfoCollection(method.ReturnType)') -and
        $observer.Contains('method.GetParameters().Any')
    'unproven-is-ambiguous' = $runner.Contains(
        '? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Ambiguous')
    'provider-never-invoked' = $observer.Contains(
        'ProviderInvokedByProbe = false') -and -not ($observer -match '\.Invoke\(')
    'load-write-sentinels' = $runner.Contains(
        '_catalogProviderObservation.WriteObserved') -and
        $runner.Contains('_catalogProviderObservation.LoadObserved')
    'no-selection-required' = -not $observer.Contains('ObserveSelection')
    'incremental-evidence' = $result.Contains('"runtime-events.json"') -and
        $result.Contains('RuntimeTestResultWriter.WriteAtomic(')
    'hooks-preserve-original' = -not ($observer -match
        'private static void (InitializePrefix|CandidatePrefix)\([^)]*(\bref\b|\bout\b)') -and
        $observer.Contains('Unpatch(method, HarmonyPatchType.All, _context.ModId)')
    'existing-observers-isolated' = $runner.Contains('RunSaveCatalogObservation()') -and
        $runner.Contains('RunManualSaveLoadObservation()')
    'steam-app-mandatory' = $common.Contains('$script:KmgSteamAppId = 640820')
    'no-direct-launch-or-input' = -not $orchestrator.Contains('Kingmaker.exe') -and
        -not ($orchestrator -match '(SendKeys|mouse_event|keybd_event|WScript\.Shell)')
    'atomic-provider-artifacts' = $runner.Contains(
        '"runtime-catalog-provider-captured.json"') -and
        $result.Contains('providerCapturedPath')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count) {
    throw "Save catalog provider observation tests failed: $($failed -join ', ')"
}
Write-Host "Save catalog provider observation tests passed: $($checks.Count)"
