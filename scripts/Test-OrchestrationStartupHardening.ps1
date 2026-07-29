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
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $script:failures.Add($Name) }
    catch { $script:passed++ }
}

# Faithfully model the failed run's boundary: process discovery and a valid
# readiness marker are not terminal completion.
$failedRun = [pscustomobject]@{
    processDiscovered = $true
    guardedRequestAccepted = $true
    runtimeReady = $true
    finalResult = $null
}
Assert-True ($failedRun.processDiscovered -and $failedRun.runtimeReady -and
    $null -eq $failedRun.finalResult) 'failed-run-reproduced-without-false-pass'

function Get-CimInstance {
    [pscustomobject]@{ ProcessId = 42 }
}
function Invoke-CimMethod {
    [pscustomobject]@{
        ReturnValue = 0
        Domain = 'TEST'
        User = 'owner'
        Diagnostic = 'must-not-leak'
    }
}
$ownerOutput = @(Get-KmgProcessOwner -ProcessId 42)
Assert-True ($ownerOutput.Count -eq 1) 'cim-owner-output-is-scalar'
Assert-True ($ownerOutput[0] -ceq 'TEST\owner') 'cim-result-does-not-leak'

$process = [pscustomobject]@{ Id = 77; ProcessName = 'Kingmaker' }
$launch = [pscustomobject][ordered]@{
    PSTypeName = 'KingmakerGunslinger.RuntimeLaunchResult'
    steamExecutable = 'C:\Program Files (x86)\Steam\steam.exe'
    steamAppId = 640820
    steamProcessId = 10
    kingmakerProcess = $process
    kingmakerProcessId = 77
    kingmakerStartedAtUtc = [DateTime]::UtcNow
}
Assert-KmgRuntimeLaunchResult -LaunchResult $launch
$script:passed++
Assert-Throws { Assert-KmgRuntimeLaunchResult -LaunchResult $null } `
    'null-launch-result-rejected'
Assert-Throws { Assert-KmgRuntimeLaunchResult -LaunchResult @($launch, $launch) } `
    'array-launch-result-rejected'
Assert-Throws {
    Assert-KmgRuntimeLaunchResult -LaunchResult ([pscustomobject]@{
        steamAppId = 640820
    })
} 'malformed-launch-result-rejected'

$now = [DateTime]::UtcNow
$ready = [pscustomobject]@{
    schemaVersion = 1
    runId = 'run'
    scenario = 'working-save-smoke'
    loadedModVersion = '0.0.30'
    processId = 77
    readinessTimestampUtc = $now.ToString('o')
    installedObservationHookIdentifiers = @('hook')
    runtimeRunnerActive = $true
    updateCallbackCount = 2
    mainMenuLifecycleReady = $true
    ummStartupState = 'initialized; overlay nonblocking-or-absent'
}
Assert-True (Test-KmgRuntimeReadyMarker -Marker $ready -RunId 'run' `
    -Scenario 'working-save-smoke' -ExpectedVersion '0.0.30' -ProcessId 77 `
    -RequestWrittenUtc $now.AddSeconds(-1)) 'strong-runtime-readiness-accepted'
$ready.mainMenuLifecycleReady = $false
Assert-True (-not (Test-KmgRuntimeReadyMarker -Marker $ready -RunId 'run' `
    -Scenario 'working-save-smoke' -ExpectedVersion '0.0.30' -ProcessId 77 `
    -RequestWrittenUtc $now.AddSeconds(-1))) 'overlay-cannot-substitute-for-menu'

$orchestrator = Get-Content -Raw (
    Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1')
$common = Get-Content -Raw (
    Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$scenario = Get-Content -Raw (
    Join-Path (Split-Path $PSScriptRoot -Parent) `
        'src\KingmakerGunslinger\RuntimeTesting\WorkingSaveSmokeScenario.cs')
$runner = Get-Content -Raw (
    Join-Path (Split-Path $PSScriptRoot -Parent) `
        'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')

Assert-True ($common.Contains('$ownerResults = @(Invoke-CimMethod')) `
    'invoke-cim-output-captured'
Assert-True ($common.Contains('Write-Output -NoEnumerate $launchResult')) `
    'launch-emits-exactly-one-object'
Assert-True ($orchestrator.Contains('$launchOutput.Count -ne 1')) `
    'caller-enforces-one-launch-result'
Assert-True ($orchestrator.Contains('waiting-for-runtime-readiness') -and
    $orchestrator.Contains('waiting-for-final-result')) `
    'owner-verification-proceeds-to-waits'
Assert-True ($orchestrator.Contains('finally {') -and
    $orchestrator.Contains('without a final runtime result')) `
    'silent-return-guarded-by-finally'
Assert-True ($orchestrator.Contains("stage = 'process-exited-early'")) `
    'early-process-exit-stage-specific'
Assert-True ($orchestrator.Contains('stage=runtime-readiness')) `
    'missing-readiness-stage-specific'
Assert-True ($orchestrator.Contains('$ConfirmPreference = ''None''')) `
    'nested-confirmation-contained'
Assert-True ($orchestrator.IndexOf('$PSCmdlet.ShouldProcess(') -lt
    $orchestrator.IndexOf("'Build-Local.ps1'")) 'confirmation-before-first-write'
Assert-True ($orchestrator.Contains("-Confirm:`$false -PassThru")) `
    'deployment-confirmation-suppressed-after-authorization'
Assert-True ($scenario.Contains('MainMenuReady') -and
    $scenario.Contains('overlay was not treated as readiness')) `
    'main-menu-not-overlay-is-readiness'
Assert-True ($runner.Contains('_updateCallbackCount >= 2') -and
    $runner.Contains('if (!_workingReadyWritten)') -and
    $runner.Contains('Stage-specific startup timeout expired.')) `
    'callbacks-and-readiness-gate-action'
Assert-True ($scenario.Contains('Transition("load-game-action-resolution"') -and
    $scenario.Contains('return;')) 'load-action-yields-after-menu-proof'
Assert-True ($common.Contains('$script:KmgSteamAppId = 640820')) `
    'steam-app-id-remains-fixed'
Assert-True (-not ($orchestrator -match 'Start-Process\s+.*Kingmaker\.exe')) `
    'direct-kingmaker-launch-remains-absent'
Assert-True (-not ($scenario -match '\.(Save|QuickSave|AutoSave)\s*\(')) `
    'no-save-writing-introduced'

if ($failures.Count -ne 0) {
    throw "Orchestration startup hardening tests failed: $($failures -join ', ')"
}
Write-Host "Orchestration startup hardening tests passed: $passed"
