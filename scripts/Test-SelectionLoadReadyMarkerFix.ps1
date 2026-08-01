[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
$passed = 0
function Assert-True([bool]$Condition, [string]$Name) {
    if ($Condition) { $script:passed++; return }
    $script:failures.Add($Name)
}
function Copy-Marker([object]$Marker) {
    return $Marker | ConvertTo-Json -Depth 10 | ConvertFrom-Json
}
function Test-Marker([object]$Marker, [string]$Scenario,
    [string]$RunId = 'run-1', [string]$Version = '0.0.30',
    [int]$ProcessId = 19916, [DateTime]$WrittenUtc = $script:writtenUtc) {
    return Test-KmgRuntimeReadyMarker -Marker $Marker -RunId $RunId `
        -Scenario $Scenario -ExpectedVersion $Version -ProcessId $ProcessId `
        -RequestWrittenUtc $WrittenUtc
}

$writtenUtc = [DateTime]::Parse('2026-08-01T02:22:22.2710663Z').ToUniversalTime()
$workingHooks = @(
    'Kingmaker.MainMenu.LoadGame(Kingmaker.EntitySystem.Persistence.SaveInfo):System.Void',
    'Kingmaker.UI.SaveLoadWindow.ListOfSaves.Initialize(System.Collections.Generic.List,Boolean):System.Void'
)
function New-WorkingMarker([string]$Scenario) {
    [pscustomobject]@{
        schemaVersion = 1; runId = 'run-1'; scenario = $Scenario
        loadedModVersion = '0.0.30'; processId = 19916
        readinessTimestampUtc = '2026-08-01T02:23:21.0384391Z'
        installedObservationHookIdentifiers = $workingHooks
        runtimeRunnerActive = $true; updateCallbackCount = 7
        mainMenuLifecycleReady = $true
        ummStartupState = 'initialized; overlay nonblocking-or-absent'
        readinessStage = 'working-entry-ready'
        saveName = 'KMG_AUTOMATION_WORKING'
    }
}

$selectionScenario = 'observe-working-save-selection-load-action'
$entryScenario = 'observe-working-save-entry-action'
foreach ($scenario in @($selectionScenario, $entryScenario)) {
    $valid = New-WorkingMarker $scenario
    Assert-True (Test-Marker $valid $scenario) "$scenario-valid-accepted"

    $changed = Copy-Marker $valid; $changed.readinessStage = 'observer-ready'
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-observer-ready-rejected"
    $changed = Copy-Marker $valid; $changed.readinessStage = 'load-game-action-resolved'
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-working-stage-required"
    $changed = Copy-Marker $valid; $changed.saveName = 'KMG_AUTOMATION_BASELINE'
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-wrong-save-rejected"
    $changed = Copy-Marker $valid; $changed.saveName = $null
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-missing-save-rejected"
    $changed = Copy-Marker $valid; $changed.runId = 'another-run'
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-wrong-run-rejected"
    $changed = Copy-Marker $valid; $changed.scenario = $entryScenario
    if ($scenario -ceq $entryScenario) { $changed.scenario = $selectionScenario }
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-wrong-scenario-rejected"
    $changed = Copy-Marker $valid; $changed.loadedModVersion = '0.0.29'
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-wrong-version-rejected"
    $changed = Copy-Marker $valid; $changed.processId = 42
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-wrong-process-rejected"
    $changed = Copy-Marker $valid; $changed.readinessTimestampUtc = '2026-08-01T02:22:21Z'
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-stale-rejected"
    $changed = Copy-Marker $valid; $changed.runtimeRunnerActive = $false
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-runner-required"
    $changed = Copy-Marker $valid; $changed.installedObservationHookIdentifiers = @()
    Assert-True (-not (Test-Marker $changed $scenario)) "$scenario-hooks-required"
}

$ordinaryScenario = 'observe-manual-save-load'
$ordinary = [pscustomobject]@{
    schemaVersion = 1; runId = 'run-1'; scenario = $ordinaryScenario
    loadedModVersion = '0.0.30'; processId = 19916
    readinessTimestampUtc = '2026-08-01T02:23:21.0384391Z'
    installedObservationHookIdentifiers = @('Kingmaker.Game.LoadGame(...):System.Void')
}
Assert-True (Test-Marker $ordinary $ordinaryScenario) 'ordinary-observer-contract-accepted'
$ordinaryWrong = Copy-Marker $ordinary
$ordinaryWrong.installedObservationHookIdentifiers = @()
Assert-True (-not (Test-Marker $ordinaryWrong $ordinaryScenario)) `
    'ordinary-observer-still-requires-hooks'

$evidence = Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory `
    -Filter '*-observe-working-save-selection-load-action' |
    Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1 `
    -ExpandProperty FullName
if ([string]::IsNullOrWhiteSpace($evidence)) {
    throw 'The real selection-load readiness evidence fixture is missing.'
}
$request = Get-Content (Join-Path $evidence 'runtime-request.json') -Raw | ConvertFrom-Json
$realMarker = Get-Content (Join-Path $evidence 'runtime-ready.json') -Raw | ConvertFrom-Json
$realWrittenUtc = (Get-Item (Join-Path $evidence 'runtime-request.json')).LastWriteTimeUtc
Assert-True (Test-KmgRuntimeReadyMarker -Marker $realMarker -RunId $request.runId `
    -Scenario $request.scenario -ExpectedVersion $request.expectedModVersion `
    -ProcessId $realMarker.processId -RequestWrittenUtc $realWrittenUtc) `
    'real-failed-marker-accepted-after-classification-fix'

$orchestrator = Get-Content (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Raw
Assert-True ($orchestrator.Contains('Ready marker failed predicates:')) `
    'predicate-diagnostics-exposed'

$diagnosticMarker = New-WorkingMarker $selectionScenario
$diagnosticMarker.runId = 'wrong-run'
$diagnosticMarker.processId = 42
$diagnosticMarker.readinessStage = 'observer-ready'
$diagnosticFailures = $null
$diagnosticAccepted = Test-KmgRuntimeReadyMarker -Marker $diagnosticMarker `
    -RunId 'run-1' -Scenario $selectionScenario -ExpectedVersion '0.0.30' `
    -ProcessId 19916 -RequestWrittenUtc $writtenUtc `
    -FailedPredicates ([ref]$diagnosticFailures)
Assert-True (-not $diagnosticAccepted -and
    @($diagnosticFailures).Count -eq 3 -and
    $diagnosticFailures[0] -ceq 'runId' -and
    $diagnosticFailures[1] -ceq 'processId' -and
    $diagnosticFailures[2] -ceq 'readinessStage') `
    'individual-failed-predicates-reported-in-evaluation-order'

function Get-TreeIdentity([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return '<missing>' }
    return (@(Get-ChildItem -LiteralPath $Path -Recurse -Force |
        Sort-Object FullName | ForEach-Object {
            $length = if ($_.PSIsContainer) { 0 } else { $_.Length }
            '{0}|{1}|{2}' -f $_.FullName, $length, $_.LastWriteTimeUtc.Ticks
        }) -join "`n")
}
$root = Split-Path -Parent $PSScriptRoot
$artifactRoot = Join-Path $root 'artifacts'
$backupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod'
$artifactBefore = Get-TreeIdentity $artifactRoot
$backupBefore = Get-TreeIdentity $backupRoot
$evidenceBefore = Get-TreeIdentity $script:KmgRuntimeEvidenceRoot
$script:cimCalls = 0
$script:startProcessCalls = 0
function global:Get-CimInstance { $script:cimCalls++; return @() }
function global:Start-Process { $script:startProcessCalls++; throw 'Unexpected launch.' }
try {
    $whatIfOutput = @(& (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') `
        -Scenario $selectionScenario -ExpectedVersion '0.0.30' `
        -SaveName KMG_AUTOMATION_WORKING -ManualInteractionRequired `
        -AllowDirtyGit -WhatIf -Confirm:$false 6>&1)
}
finally {
    Remove-Item Function:\global:Get-CimInstance
    Remove-Item Function:\global:Start-Process
}
Assert-True ((Get-TreeIdentity $artifactRoot) -ceq $artifactBefore) `
    'whatif-performs-no-build'
Assert-True ((Get-TreeIdentity $backupRoot) -ceq $backupBefore) `
    'whatif-performs-no-backup'
Assert-True ((Get-TreeIdentity $script:KmgRuntimeEvidenceRoot) -ceq $evidenceBefore) `
    'whatif-performs-no-deployment-or-evidence-write'
Assert-True ($script:startProcessCalls -eq 0) 'whatif-launches-no-process'
Assert-True (($whatIfOutput -join "`n").Contains(
    'No deployment or process launch occurred.')) 'whatif-reports-source-only-boundary'

if ($failures.Count) {
    throw "Selection-load ready-marker tests failed: $($failures -join ', ')"
}
Write-Host "Selection-load ready-marker tests passed: $passed"
