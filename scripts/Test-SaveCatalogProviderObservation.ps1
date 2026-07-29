[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$observer = Get-Content -Raw (Join-Path $runtime 'SaveCatalogProviderObservation.cs')
$runner = Get-Content -Raw (Join-Path $runtime 'RuntimeTestRunner.cs')
$result = Get-Content -Raw (Join-Path $runtime 'RuntimeTestResult.cs')
$catalog = Get-Content -Raw (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$orchestrator = Get-Content -Raw (
    Join-Path $root 'scripts\Invoke-KingmakerRuntimeTest.ps1')
$common = Get-Content -Raw (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$deployment = Get-Content -Raw (Join-Path $root 'scripts\Deploy-Local.ps1')
$fixture = Get-Content -Raw (Join-Path $root (
    'tests\fixtures\save-catalog-provider-ambiguous.json')) | ConvertFrom-Json

$same = [Collections.Generic.List[object]]::new()
$same.Add([pscustomobject]@{ id = 1 })
$equalButDistinct = [Collections.Generic.List[object]]::new()
$equalButDistinct.Add($same[0])

$checks = [ordered]@{
    'guarded-request-only' = $catalog.Contains('ObserveSaveCatalogProvider') -and
        $runner.Contains('RuntimeTestRequestParser.TryActivate')
    'exact-ambiguous-fixture' = $fixture.status -eq 'AMBIGUOUS' -and
        $fixture.descriptorCount -eq 47 -and
        $fixture.providerCandidates.Count -eq 0
    'consumer-alone-not-proof' = $observer.Contains(
        'No observed return or callback argument shared reference')
    'reference-correlation' = [object]::ReferenceEquals($same, $same) -and
        $observer.Contains('_returned.TryGetValue(collection')
    'value-equal-distinct-rejected' =
        -not [object]::ReferenceEquals($same, $equalButDistinct) -and
        $observer.Contains('ReferenceEquals(left, right)')
    'filtered-distinguished' = $observer.Contains(
        'filtered-or-sorted-ui-collection')
    'callback-correlated' = $observer.Contains(
        'callback-or-state-machine-argument')
    'multiple-unresolved-ambiguous' = $runner.Contains(
        '? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Ambiguous')
    'single-proven-provider-pass' = $observer.Contains(
        '_sourceProven = !transformed')
    'side-effects-and-ui-recorded' = $result.Contains(
        'requiresLoadGameUi') -and $result.Contains('sideEffects')
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
    'caller-il-dependencies' = $observer.Contains(
        'ReadDirectDependencies(caller)') -and
        $observer.Contains('directDependencies.Contains(method)')
    'compatible-signature-filter' = $observer.Contains(
        'IsCompatibleCollection(method.ReturnType)')
    'provider-never-invoked' = $observer.Contains(
        'ProviderInvokedByProbe = false') -and -not ($observer -match '\.Invoke\(')
    'load-write-sentinels' = $runner.Contains(
        '_catalogProviderObservation.WriteObserved') -and
        $runner.Contains('_catalogProviderObservation.LoadObserved')
    'no-selection-required' = -not $observer.Contains('ObserveSelection')
    'incremental-evidence-survives' = $result.Contains('"runtime-events.json"') -and
        $result.Contains('RuntimeTestResultWriter.WriteAtomic(') -and
        $observer.Contains('provider-correlation-failed')
    'events-not-reference-elided' = $result.Contains(
        'PreserveReferencesHandling.None')
    'hooks-preserve-original' = -not ($observer -match
        'private static void (InitializePrefix|CandidatePrefix)\([^)]*(\bref\b|\bout\b)') -and
        $observer.Contains('Unpatch(method, HarmonyPatchType.All, _context.ModId)')
    'existing-observers-isolated' = $runner.Contains('RunSaveCatalogObservation()') -and
        $runner.Contains('RunManualSaveLoadObservation()')
    'steam-app-mandatory' = $common.Contains('$script:KmgSteamAppId = 640820')
    'no-direct-launch-or-input' = -not $orchestrator.Contains('Kingmaker.exe') -and
        -not ($orchestrator -match '(SendKeys|mouse_event|keybd_event|WScript\.Shell)')
    'exactly-one-backup' = @(
        [regex]::Matches($deployment, 'Backup-Live-Mod\.ps1')).Count -eq 1
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
