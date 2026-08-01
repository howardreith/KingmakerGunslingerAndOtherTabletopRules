[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$root = Split-Path -Parent $PSScriptRoot
$scenarioPath = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\WorkingSaveSmokeScenario.cs'
$runnerPath = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs'
$resultPath = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestResult.cs'
$catalogPath = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs'
$orchestratorPath = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$scenario = Get-Content -LiteralPath $scenarioPath -Raw
$runner = Get-Content -LiteralPath $runnerPath -Raw
$result = Get-Content -LiteralPath $resultPath -Raw
$catalog = Get-Content -LiteralPath $catalogPath -Raw
$orchestrator = Get-Content -LiteralPath $orchestratorPath -Raw
$deployment = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Deploy-Local.ps1') -Raw
$common = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1') -Raw

function Assert-True([bool]$Condition, [string]$Name) {
    if (-not $Condition) { throw "Assertion failed: $Name" }
    Write-Host "PASS $Name"
}

$exactHooks = @(
    'Kingmaker.UI.SaveLoadWindow.SaveSlot.OnButtonSaveLoad():System.Void',
    'Kingmaker.UI.SaveLoadWindow.SaveLoadWindow.HandleHardcodeMainMenuSaveLoad(Kingmaker.EntitySystem.Persistence.SaveInfo):System.Void',
    'Kingmaker.MainMenu.LoadGame(Kingmaker.EntitySystem.Persistence.SaveInfo):System.Void'
)

Assert-True ($catalog.Contains('ObserveWorkingSaveReceiverBoundAction') -and
    $catalog.Contains('observe-working-save-receiver-bound-action')) 'scenario-allowlisted'
Assert-True ($scenario.Contains('ExactPatchableMethod(slot,') -and
    $scenario.Contains('"OnButtonSaveLoad", Type.EmptyTypes, typeof(void)') -and
    $scenario.Contains('"HandleHardcodeMainMenuSaveLoad",') -and
    $scenario.Contains('RequirePatchableContract(_loadEntry')) 'only-three-exact-action-contracts'
Assert-True ($scenario.Contains(
    'if (_observeReceiverBoundAction || _autonomousReceiverBoundAction)') -and
    $scenario.Contains('InstallExactSaveWriteSentinels(assembly, prefix)') -and
    $scenario.Contains('BindingFlags.DeclaredOnly')) 'no-keyword-or-broad-patch-sweep'
Assert-True ($scenario.Contains('method.GetMethodBody() == null') -and
    $scenario.Contains('method.IsAbstract') -and
    $scenario.Contains('method.ContainsGenericParameters') -and
    $scenario.Contains('method.DeclaringType != declaringType')) 'bodyless-inherited-abstract-generic-rejected'
Assert-True ($scenario.Contains('_entryCandidates = slots.Count') -and
    $scenario.Contains('_entryCandidates == 1')) 'exactly-one-working-slot-required'
Assert-True ($scenario.Contains('windows.Count != 1') -and
    $scenario.Contains('_receiverBoundWindow = windows[0]')) 'exactly-one-owning-window-required'
Assert-True ($runner.IndexOf('_trace.WriteReady(new RuntimeReadyMarker',
    [StringComparison]::Ordinal) -lt $orchestrator.IndexOf(
    'CLICK THE NORMAL LOAD ACTION FOR KMG_AUTOMATION_WORKING ONCE',
    [StringComparison]::Ordinal)) 'readiness-precedes-human-banner'
Assert-True ($scenario.Contains(
    'ProbeInvokedEntryAction = _autonomousReceiverBoundAction') -and
    $scenario.Contains('if (_stage == "receiver-bound-action-invocation")') -and
    $scenario.Contains('_slotAction.Invoke(_receiverBoundSlot, null)') -and
    -not $scenario.Contains('_windowHandler.Invoke(')) `
    'observer-remains-non-initiating-and-autonomous-uses-slot-only'
Assert-True ($scenario.Contains('ReferenceEquals(receiver, _receiverBoundSlot)')) 'slot-receiver-reference-mandatory'
Assert-True ($scenario.Contains('ReferenceEquals(receiver, _receiverBoundWindow)')) 'window-receiver-reference-mandatory'
Assert-True ($scenario.Contains('ReferenceEquals(argument, _workingDescriptor)') -and
    $scenario.Contains('_descriptorCorrelated = ReferenceEquals(argument, _workingDescriptor)')) 'working-saveinfo-reference-mandatory-twice'
Assert-True ($runner.Contains('forbidden-save-selection') -and
    $scenario.Contains('_baselineLoadObserved') -and $scenario.Contains('_otherLoadObserved')) 'baseline-and-other-descriptors-fail'
Assert-True ($scenario.Contains('_slotActionInvocations == 1') -and
    $scenario.Contains('_windowHandlerInvocations == 1') -and
    $scenario.Contains('_loadEntryInvocations == 1')) 'exactly-one-each-required'
Assert-True ($scenario.Contains('_slotActionSequence < _windowHandlerSequence') -and
    $scenario.Contains('_windowHandlerSequence < _loadEntrySequence') -and
    $scenario.Contains('_loadEntrySequence < _completionSequence') -and
    $scenario.Contains('_completionSequence < _fingerprintSequence')) 'strict-event-order-required'
Assert-True ($scenario.Contains('RequireGameThread();') -and
    $scenario.Contains('_wrongThread = true')) 'game-thread-required'
Assert-True ($scenario.Contains('_completionCallback && _stableSamples >= 2')) 'completion-and-fingerprint-mandatory'
Assert-True ($scenario.Contains('SaveWritingApiObserved = _writeObserved') -and
    $runner.Contains('unexpected-save-write')) 'no-save-writing-required'
Assert-True ($scenario.Contains('CaptureHookException') -and
    $runner.Contains('original game behavior was preserved') -and
    $scenario.Contains('// A diagnostic failure must never escape into the game handler.')) 'hook-errors-contained'
Assert-True ($scenario.Contains('RemoveHooks();') -and
    $scenario.Contains('HooksRemoved = _removed')) 'hooks-removed-all-outcomes'
Assert-True ($result.Contains('exactSlotIdentity') -and
    $result.Contains('exactWindowIdentity') -and
    $common.Contains('working-receiver-bound-action-ready')) 'receiver-bound-readiness-schema'
Assert-True ($common.Contains('$script:KmgSteamAppId = 640820') -and
    $common.Contains("@('-applaunch', `$AppId.ToString") -and
    $orchestrator.Contains('Assert-KmgSteamAppId -AppId $SteamAppId')) 'steam-app-id-640820-only'
Assert-True (([regex]::Matches($orchestrator,
    'Deploy-Local\.ps1''\) `\r?\n    -PackagePath \$package -Confirm')).Count -eq 1 -and
    ([regex]::Matches($deployment, 'Backup-Live-Mod\.ps1')).Count -eq 1) 'exactly-one-deployment-backup'
Assert-True ($orchestrator.Contains('Source-only/WhatIf validation passed. No deployment or process launch occurred.') -and
    $orchestrator.IndexOf('if (-not $PSCmdlet.ShouldProcess(', [StringComparison]::Ordinal) -lt
    $orchestrator.IndexOf("Build-Local.ps1'", [StringComparison]::Ordinal)) 'whatif-stops-before-mutation'
Assert-True ($runner.Contains('ReceiverBoundScopeResolutionFailed') -and
    $runner.Contains('RuntimeTestStatuses.Ambiguous')) 'zero-or-multiple-scope-is-ambiguous'
Assert-True ($runner.Contains('workingSaveReceiverBoundActionObservation') -or
    $result.Contains('workingSaveReceiverBoundActionObservation')) 'structured-result-present'
Assert-True ($common.Contains('Write-KmgUtf8NoBom') -and
    $common.Contains('[IO.File]::Replace') -and
    $common.Contains('[IO.File]::Move')) 'readiness-and-result-write-support-atomic'

$marker = [pscustomobject]@{
    schemaVersion = 1; runId = 'receiver-run'
    scenario = 'observe-working-save-receiver-bound-action'
    loadedModVersion = '0.0.30'; processId = 77
    readinessTimestampUtc = [DateTime]::UtcNow.ToString('o')
    installedObservationHookIdentifiers = $exactHooks
    runtimeRunnerActive = $true; updateCallbackCount = 2
    mainMenuLifecycleReady = $true
    ummStartupState = 'initialized; overlay nonblocking-or-absent'
    readinessStage = 'working-receiver-bound-action-ready'
    saveName = 'KMG_AUTOMATION_WORKING'
    exactSlotIdentity = 'Kingmaker.UI.SaveLoadWindow.SaveSlot#1'
    exactWindowIdentity = 'Kingmaker.UI.SaveLoadWindow.SaveLoadWindow#2'
}
$written = [DateTime]::UtcNow.AddSeconds(-1)
Assert-True (Test-KmgRuntimeReadyMarker -Marker $marker -RunId 'receiver-run' `
    -Scenario 'observe-working-save-receiver-bound-action' `
    -ExpectedVersion '0.0.30' -ProcessId 77 -RequestWrittenUtc $written) 'valid-receiver-bound-ready-marker'
$marker.exactSlotIdentity = ''
Assert-True (-not (Test-KmgRuntimeReadyMarker -Marker $marker -RunId 'receiver-run' `
    -Scenario 'observe-working-save-receiver-bound-action' `
    -ExpectedVersion '0.0.30' -ProcessId 77 -RequestWrittenUtc $written)) 'missing-slot-identity-rejected'

Write-Host 'Working-save receiver-bound action focused tests passed.'
