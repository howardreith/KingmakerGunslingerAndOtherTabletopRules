[CmdletBinding()]
param(
    [string]$EvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
$passed = 0
function Assert-True([bool]$Condition, [string]$Name) {
    if ($Condition) { $script:passed++; return }
    $script:failures.Add($Name)
}
function Assert-Contains([string]$Text, [string]$Value, [string]$Name) {
    Assert-True ($Text.Contains($Value)) $Name
}

$root = Split-Path -Parent $PSScriptRoot
$scenarioSource = Get-Content -LiteralPath (
    Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\WorkingSaveSmokeScenario.cs') -Raw
$runnerSource = Get-Content -LiteralPath (
    Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs') -Raw
$requestSource = Get-Content -LiteralPath (
    Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRequest.cs') -Raw
$orchestrator = Get-Content -LiteralPath (
    Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Raw
$observerSources = @(
    'LoadGameButtonActionObservation.cs',
    'SaveCatalogSelectionObservation.cs',
    'ManualSaveLoadObservation.cs'
) | ForEach-Object {
    Get-Content -LiteralPath (
        Join-Path $root "src\KingmakerGunslinger\RuntimeTesting\$_") -Raw
}

$synthetic = Join-Path $script:KmgRuntimeEvidenceRoot 'working-save-smoke-source-test'
$request = New-KmgRuntimeRequest -Scenario 'working-save-smoke' `
    -ExpectedVersion '0.0.63' -TimeoutSeconds 120 -StartupTimeoutSeconds 180 `
    -CatalogTimeoutSeconds 180 -SelectionTimeoutSeconds 30 `
    -CompletionTimeoutSeconds 180 -MainMenuTimeoutSeconds 180 `
    -ActionResolutionTimeoutSeconds 180 -ActionInvocationTimeoutSeconds 30 `
    -DescriptorResolutionTimeoutSeconds 30 -LoadEntryTimeoutSeconds 30 `
    -FingerprintTimeoutSeconds 180 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{
        saveName = 'KMG_AUTOMATION_WORKING'
    }

Assert-True ($request.parameters.saveName -ceq 'KMG_AUTOMATION_WORKING') 'guarded-request-save-name'
Assert-Contains $requestSource 'RuntimeTestScenarioCatalog.WorkingSaveSmoke' 'scenario-only-action-routing'
Assert-Contains $requestSource 'baseline-save-forbidden' 'baseline-request-rejected'
Assert-Contains $requestSource 'save-name-required' 'missing-save-name-rejected'
Assert-Contains $scenarioSource '_buttonCandidates == 1' 'exactly-one-button'
Assert-Contains $runnerSource 'ButtonCandidateCount > 1' 'multiple-button-ambiguous'
Assert-Contains $scenarioSource 'ExactComponents' 'component-invariants'
Assert-Contains $scenarioSource 'RequireGameThread' 'game-thread-required'
Assert-Contains $scenarioSource '_buttonInvocations++' 'one-normal-action'
Assert-Contains $scenarioSource '_autonomousReceiverBoundAction' 'autonomous-receiver-bound-mode'
Assert-Contains $scenarioSource '_slotAction.Invoke(_receiverBoundSlot, null)' 'exact-proven-slot-action'
Assert-Contains $scenarioSource 'receiver-bound-action-invocation' 'receiver-action-stage'
Assert-Contains $scenarioSource 'ReferenceEquals(receiver, _receiverBoundSlot)' 'exact-slot-receiver'
Assert-Contains $scenarioSource 'ReferenceEquals(receiver, _receiverBoundWindow)' 'exact-window-receiver'
Assert-Contains $scenarioSource 'ReferenceEquals(argument, _workingDescriptor)' 'exact-window-argument'
Assert-True (-not $scenarioSource.Contains('_windowHandler.Invoke(')) 'no-window-handler-shortcut'
Assert-True (-not $scenarioSource.Contains('_handler.Invoke')) 'no-dual-action-route'
Assert-True (-not ($scenarioSource -match
    '(?i)Input\.(Get|mousePosition)|SendKeys|mouse_event|SetCursorPos|computer.?use')) 'no-input-path'
Assert-Contains $scenarioSource '_buttonInvocations != 1 || _handlerInvocations != 1' 'catalog-after-action'
Assert-True (-not ($scenarioSource -match '(?i)thumbnail|portrait|screenshot|sprite|texture')) 'no-visual-dependency'
Assert-Contains $runnerSource 'WorkingCount > 1' 'multiple-working-ambiguous'
Assert-Contains $runnerSource 'ReceiverBoundScopeResolutionFailed' 'receiver-scope-fails-closed'
Assert-Contains $scenarioSource '_workingCount == 1' 'unique-working-required'
Assert-Contains $scenarioSource '_baselineCount == 1' 'baseline-distinguishable'
Assert-Contains $scenarioSource 'ReferenceEquals(' 'exact-descriptor-reference'
Assert-Contains $runnerSource 'DescriptorReferenceCorrelated' 'correlation-required-to-pass'
Assert-Contains $runnerSource 'CompletionCallbackObserved' 'completion-required-to-pass'
Assert-Contains $scenarioSource 'ExpectedGameId' 'game-id-not-sole-identity'
Assert-Contains $scenarioSource 'Transition("load-entry-invocation"' 'load-after-resolution'
Assert-Contains $runnerSource 'load-completion-and-fingerprint' 'completion-and-fingerprint-required'
Assert-Contains $runnerSource 'timeoutStage=' 'exact-timeout-stage'
Assert-True (-not ($scenarioSource -match '\.(Save|QuickSave|AutoSave)\s*\(')) 'no-deliberate-save-call'
Assert-Contains $scenarioSource 'unexpected-save-write' 'unexpected-write-fails'
Assert-True (@($observerSources | Where-Object {
    $_ -match 'ProbeInvokedAction = false|probeInitiatedSaveWriting'
}).Count -ge 2) 'observers-remain-non-initiating'
Assert-Contains $orchestrator '[int]$SteamAppId = 640820' 'steam-app-id-mandatory'
Assert-True (-not ($orchestrator -match 'Start-Process\s+.*Kingmaker\.exe')) 'no-direct-executable-launch'
Assert-Contains $orchestrator "'Deploy-Local.ps1'" 'one-deployment-backup-path'
Assert-Contains $orchestrator 'No deployment or process launch occurred.' 'whatif-no-mutation'
Assert-Contains $runnerSource '_trace.Record("final-result-created"' 'atomic-result-evidence'
Assert-Contains $runnerSource 'WriteLifecycleStage("final-result-flushed")' 'durable-result-flush-marker'
Assert-Contains $runnerSource 'Application.Quit();' 'exit-after-flush'
Assert-True (-not ($scenarioSource -match '(?i)OnButtonContinue|LoadNewest|NewestSave|File\.(Read|Open).*\.zks')) `
    'no-continue-newest-or-raw-save-fallback'
Assert-True (-not ($orchestrator -match '(?i)working-save-smoke[^\r\n]*(click|manual interaction)')) `
    'no-working-smoke-human-banner'

$fixtures = @(
    '20260729T0143436024338Z-observe-manual-save-load',
    '20260729T0208479601809Z-observe-save-catalog-and-selection',
    '20260729T0339107643887Z-observe-load-game-button-action'
)
$fixtureResults = @($fixtures | ForEach-Object {
    Get-Content -LiteralPath (
        Join-Path $EvidenceRoot "$_\runtime-result.json") -Raw | ConvertFrom-Json
})
Assert-True (@($fixtureResults | Where-Object status -ne 'PASS').Count -eq 0) 'three-pass-fixtures'
$catalog = $fixtureResults[1].saveCatalogObservation
Assert-True ($catalog.descriptorCount -eq 47 -and $catalog.catalogComplete -and
    $catalog.workingMatchCount -eq 1 -and $catalog.baselineMatchCount -eq 1) 'deterministic-catalog-fixture'

if ($failures.Count -ne 0) {
    throw "Working-save-smoke tests failed: $($failures -join ', ')"
}
Write-Host "Working-save-smoke source tests passed: $passed"
