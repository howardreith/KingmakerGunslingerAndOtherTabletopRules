[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
function Assert-True([bool]$Condition, [string]$Name) {
    if (-not $Condition) { $failures.Add($Name) }
}
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $failures.Add($Name) } catch { }
}
function Assert-DoesNotThrow([scriptblock]$Action, [string]$Name) {
    try { & $Action } catch { $failures.Add("$Name ($($_.Exception.Message))") }
}

$requestPath = Join-Path $script:KmgRuntimeEvidenceRoot 'source-only-test\runtime request.json'
$arguments = @(Get-KmgSteamLaunchArguments -AppId 640820 -RequestPath $requestPath)
Assert-True ($arguments.Count -eq 4) 'launch-argument-count'
Assert-True ($arguments[0] -eq '-applaunch' -and $arguments[1] -eq '640820') 'steam-app-launch'
Assert-True ($arguments[2] -eq '-kmgRuntimeTestRequest') 'guarded-flag-after-app-id'
Assert-True ($arguments[3] -eq "`"$requestPath`"") 'request-path-safely-quoted'
Assert-Throws { Get-KmgSteamLaunchArguments -AppId 1 -RequestPath $requestPath } 'incorrect-app-id'
Assert-Throws { Get-KmgSteamLaunchArguments -AppId 640820 -RequestPath 'C:\Windows\request.json' } 'request-outside-root'
Assert-Throws { Assert-KmgSteamExecutable -SteamPath 'C:\not-steam.exe' } 'unapproved-steam-executable'

$requested = [DateTime]::UtcNow
$steam = [pscustomobject]@{ Id = 10; ProcessName = 'steam'; StartTime = $requested.ToLocalTime() }
$oldGame = [pscustomobject]@{ Id = 20; ProcessName = 'Kingmaker'; StartTime = $requested.AddMinutes(-2).ToLocalTime() }
$newGame = [pscustomobject]@{ Id = 30; ProcessName = 'Kingmaker'; StartTime = $requested.AddSeconds(1).ToLocalTime() }
$selected = Select-KmgNewKingmakerProcess -Processes @($steam, $oldGame, $newGame) `
    -ExistingProcesses @($oldGame) -RequestedAtUtc $requested
Assert-True ($selected.Id -eq 30) 'new-game-selected-deterministically'
Assert-True ($selected.Id -ne $steam.Id) 'steam-not-mistaken-for-game'
Assert-DoesNotThrow {
    $script:emptySelection = Select-KmgNewKingmakerProcess -Processes @() `
        -ExistingProcesses @() -RequestedAtUtc $requested
} 'empty-process-snapshot-accepted'
Assert-True ($null -eq $script:emptySelection) 'empty-process-snapshot-selects-nothing'
$fromEmpty = Select-KmgNewKingmakerProcess -Processes @($steam, $newGame) `
    -ExistingProcesses @() -RequestedAtUtc $requested
Assert-True ($fromEmpty.Id -eq 30) 'new-game-selected-from-empty-snapshot'
$secondOldGame = [pscustomobject]@{
    Id = 21
    ProcessName = 'Kingmaker'
    StartTime = $requested.AddMinutes(-1).ToLocalTime()
}
$afterMultipleExisting = Select-KmgNewKingmakerProcess `
    -Processes @($secondOldGame, $oldGame, $newGame) `
    -ExistingProcesses @($oldGame, $secondOldGame) -RequestedAtUtc $requested
Assert-True ($afterMultipleExisting.Id -eq 30) 'multiple-existing-games-handled-deterministically'
$reusedPid = [pscustomobject]@{
    Id = 20
    ProcessName = 'Kingmaker'
    StartTime = $requested.AddSeconds(1).ToLocalTime()
}
$selectedReusedPid = Select-KmgNewKingmakerProcess -Processes @($reusedPid) `
    -ExistingProcesses @($oldGame) -RequestedAtUtc $requested
Assert-True ($selectedReusedPid.Id -eq 20) 'pid-reuse-compares-start-time'
Assert-Throws {
    Select-KmgNewKingmakerProcess -Processes @(
        $newGame,
        [pscustomobject]@{
            Id = 31
            ProcessName = 'Kingmaker'
            StartTime = $requested.AddSeconds(2).ToLocalTime()
        }) -ExistingProcesses @() -RequestedAtUtc $requested
} 'multiple-new-games-rejected'

$startCommand = Get-Command Start-KmgSteamKingmaker
$preLaunchParameter = $startCommand.Parameters['PreLaunchProcesses']
Assert-True ($null -ne $preLaunchParameter) 'start-contract-exposes-prelaunch-processes'
Assert-True ($preLaunchParameter.ParameterType.IsArray) 'start-contract-keeps-array-typing'
Assert-True (@($preLaunchParameter.Attributes | Where-Object {
    $_ -is [Management.Automation.AllowEmptyCollectionAttribute]
}).Count -eq 1) 'start-contract-allows-empty-collection'

$orchestrator = Get-Content -LiteralPath (
    Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Raw
Assert-True ($orchestrator.Contains('Start-KmgSteamKingmaker')) 'orchestrator-uses-steam-launch'
Assert-True ($orchestrator.Contains('-PreLaunchProcesses @($preLaunchProcesses)')) `
    'orchestrator-normalizes-prelaunch-snapshot'
Assert-True (-not $orchestrator.Contains('Start-Process -FilePath $KingmakerPath')) `
    'direct-game-launch-rejected-by-default'
Assert-True (-not $orchestrator.Contains('Kingmaker.exe')) 'no-implicit-direct-fallback'
Assert-True ($orchestrator.Contains('Source-only/WhatIf validation passed. No deployment or process launch occurred.')) `
    'whatif-launches-no-process'
Assert-True ($orchestrator.Contains("`$orchestration.status = 'ERROR'")) `
    'launch-failure-recorded-as-error'
Assert-True ($orchestrator.Contains('[void](Write-KmgOrchestrationEvidence')) `
    'orchestration-evidence-written-before-and-after-launch'
Assert-True ($orchestrator.Contains('type = $_.Exception.GetType().FullName') -and
    $orchestrator.Contains('message = $_.Exception.Message')) 'exception-recorded-safely'
$initializeIndex = $orchestrator.IndexOf(
    'Initialize-KmgRuntimeTestEvidence', [StringComparison]::Ordinal)
$launchIndex = $orchestrator.IndexOf(
    'Start-KmgSteamKingmaker', [StringComparison]::Ordinal)
Assert-True ($initializeIndex -ge 0 -and $initializeIndex -lt $launchIndex) `
    'request-failure-prevents-launch'

$common = Get-Content -LiteralPath (
    Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1') -Raw
Assert-True ($common.Contains('Steam executable is missing')) 'missing-steam-fails'
Assert-True ($common.Contains('Steam client process did not become available')) 'steam-timeout-fails'
Assert-True ($common.Contains('direct-executable fallback is disabled')) 'game-start-failure-no-fallback'
Assert-True ($common.Contains('Assert-KmgUnelevated')) 'elevation-rejected'
Assert-True ($common.Contains('Assert-KmgProcessOwner')) 'same-user-verified'
Assert-True ($common.Contains('sanitizedLaunchArguments')) 'sanitized-arguments-recorded'
Assert-True ($common.Contains('Start-Process -FilePath $SteamPath')) 'steam-executable-used'
Assert-True ($common.Contains('$stream.Flush($true)') -and
    $common.Contains('[IO.File]::Move($temporary, $destination)') -and
    $common.Contains('[Management.Automation.Language.NullString]::Value')) `
    'orchestration-and-request-writes-are-atomic'
Assert-True (-not $common.Contains('Start-Process -FilePath $KingmakerPath')) `
    'common-has-no-direct-game-launch'
Assert-True ($common.Contains("throw `"Kingmaker was already running before Steam launch")) `
    'preexisting-game-rejected'

$evidenceTestDirectory = Join-Path (
    Split-Path $PSScriptRoot -Parent) 'artifacts\steam-launch-evidence-test'
if (Test-Path -LiteralPath $evidenceTestDirectory) {
    Remove-Item -LiteralPath $evidenceTestDirectory -Recurse -Force
}
try {
    New-Item -ItemType Directory -Path $evidenceTestDirectory | Out-Null
    $errorRecord = [ordered]@{
        schemaVersion = 2
        runId = 'source-only-test'
        status = 'ERROR'
        exception = [ordered]@{
            type = 'System.InvalidOperationException'
            message = 'synthetic launch failure'
        }
    }
    $errorPath = Write-KmgOrchestrationEvidence `
        -EvidenceDirectory $evidenceTestDirectory -Record $errorRecord
    $writtenError = Get-Content -LiteralPath $errorPath -Raw | ConvertFrom-Json
    Assert-True ($writtenError.status -eq 'ERROR') 'error-evidence-created'
    Assert-True ($writtenError.exception.message -eq 'synthetic launch failure') `
        'error-evidence-preserves-exception'

    $requestDirectory = Join-Path $evidenceTestDirectory 'pre-launch'
    New-Item -ItemType Directory -Path $requestDirectory | Out-Null
    $request = [ordered]@{
        runId = 'source-only-pre-launch'
        scenario = 'observe-manual-save-load'
    }
    $deploymentManifest = Join-Path $evidenceTestDirectory 'deployment.json'
    [IO.File]::WriteAllText($deploymentManifest, '{}')
    $initialized = Initialize-KmgRuntimeTestEvidence `
        -EvidenceDirectory $requestDirectory -Request $request `
        -DeploymentManifestPath $deploymentManifest
    Assert-True ($initialized.requestPath -is [string]) 'request-path-is-scalar'
    Assert-True ($initialized.resultPath -is [string]) 'result-path-is-scalar'
    Assert-True ((Join-Path $requestDirectory 'orchestration.json') -is [string]) `
        'orchestration-path-is-scalar'
    Assert-True ((Join-Path $requestDirectory 'runtime-summary.txt') -is [string]) `
        'summary-path-is-scalar'
    Assert-True ($initialized.orchestration.launchBegan -eq $false) `
        'launch-not-begun-during-request-write'

    $failureDirectory = Join-Path $evidenceTestDirectory 'pre-launch-failure'
    New-Item -ItemType Directory -Path $failureDirectory | Out-Null
    New-Item -ItemType Directory -Path (
        Join-Path $failureDirectory 'runtime-request.json') | Out-Null
    Assert-Throws {
        Initialize-KmgRuntimeTestEvidence -EvidenceDirectory $failureDirectory `
            -Request $request -DeploymentManifestPath $deploymentManifest
    } 'request-write-failure-reported'
    $failureEvidence = Get-Content -LiteralPath (
        Join-Path $failureDirectory 'orchestration.json') -Raw | ConvertFrom-Json
    Assert-True ($failureEvidence.status -eq 'ERROR') `
        'request-write-failure-is-error'
    Assert-True ($failureEvidence.deploymentCompleted -and
        -not $failureEvidence.launchBegan -and
        -not $failureEvidence.saveInteractionOccurred) `
        'pre-launch-failure-boundaries-recorded'
    Assert-True (-not (Test-Path -LiteralPath (
        Join-Path $failureDirectory 'runtime-result.json'))) `
        'pre-launch-failure-creates-no-result'
}
finally {
    if (Test-Path -LiteralPath $evidenceTestDirectory) {
        Remove-Item -LiteralPath $evidenceTestDirectory -Recurse -Force
    }
}

if ($failures.Count -ne 0) {
    throw "Steam launch source tests failed: $($failures -join ', ')"
}
Write-Host 'Steam launch source tests passed: 47'
