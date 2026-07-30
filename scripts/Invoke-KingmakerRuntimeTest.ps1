[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('mod-load-smoke', 'observe-manual-save-load',
        'observe-save-catalog-and-selection',
        'observe-save-catalog-provider',
        'observe-load-game-button-action',
        'working-save-smoke')]
    [string]$Scenario,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [ValidateRange(5, 1800)]
    [int]$TimeoutSeconds = 120,
    [ValidateRange(5, 600)]
    [int]$ObserverStartupTimeoutSeconds = 180,
    [ValidateRange(5, 1800)][int]$CatalogTimeoutSeconds = 180,
    [ValidateRange(5, 1800)][int]$SelectionTimeoutSeconds = 300,
    [ValidateRange(5, 1800)][int]$CompletionTimeoutSeconds = 180,
    [ValidateRange(5, 1800)][int]$MainMenuTimeoutSeconds = 180,
    [ValidateRange(5, 1800)][int]$ActionResolutionTimeoutSeconds = 180,
    [ValidateRange(5, 1800)][int]$ActionInvocationTimeoutSeconds = 30,
    [ValidateRange(5, 1800)][int]$DescriptorResolutionTimeoutSeconds = 30,
    [ValidateRange(5, 1800)][int]$LoadEntryTimeoutSeconds = 30,
    [ValidateRange(5, 1800)][int]$FingerprintTimeoutSeconds = 180,

    [bool]$ExitAfterCompletion = $true,
    [hashtable]$Parameters = @{},
    [ValidateSet('KMG_AUTOMATION_WORKING')]
    [string]$SaveName,
    [switch]$AllowDirtyGit,
    [switch]$AllowForceTerminate,
    [switch]$ManualInteractionRequired,
    [string]$SteamPath = 'C:\Program Files (x86)\Steam\steam.exe',
    [ValidateRange(1, 300)]
    [int]$SteamStartupTimeoutSeconds = 60,
    [ValidateRange(1, 300)]
    [int]$GameStartupTimeoutSeconds = 60,
    [int]$SteamAppId = 640820
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$manualScenarios = @('observe-manual-save-load',
    'observe-save-catalog-and-selection',
    'observe-save-catalog-provider',
    'observe-load-game-button-action')
if ($Scenario -in $manualScenarios -and -not $ManualInteractionRequired) {
    throw "$Scenario requires -ManualInteractionRequired."
}
if ($Scenario -notin $manualScenarios -and $ManualInteractionRequired) {
    throw '-ManualInteractionRequired is valid only for supervised observations.'
}
if ($Scenario -eq 'working-save-smoke') {
    if ([string]::IsNullOrWhiteSpace($SaveName)) {
        throw 'working-save-smoke requires explicit -SaveName KMG_AUTOMATION_WORKING.'
    }
    if ($Parameters.Count -ne 0) {
        throw 'Use the strictly typed -SaveName parameter, not -Parameters.'
    }
    $Parameters = @{ saveName = $SaveName }
}
elseif ($PSBoundParameters.ContainsKey('SaveName')) {
    throw '-SaveName is valid only for working-save-smoke.'
}

$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$git = Get-KmgGitState -RepositoryRoot $root
if (-not $AllowDirtyGit -and $git.Status.Count -ne 0) {
    throw 'Runtime execution requires a clean Git state. Use -AllowDirtyGit only for an explicitly permitted source state.'
}
Assert-KmgNotRunning
Assert-KmgSteamAppId -AppId $SteamAppId
Assert-KmgUnelevated
$SteamPath = Assert-KmgSteamExecutable -SteamPath $SteamPath

if (-not $PSCmdlet.ShouldProcess(
    "Steam App ID $SteamAppId",
    "build, validate, deploy, and launch guarded scenario '$Scenario'")) {
    Write-Host 'Source-only/WhatIf validation passed. No deployment or process launch occurred.'
    return
}

# This is the single confirmation boundary. A caller's -Confirm preference is
# intentionally contained after authorization so trusted nested cmdlets do not
# fan out into separate prompts. Direct invocation of those scripts retains
# their own ShouldProcess behavior.
$ConfirmPreference = 'None'
$WhatIfPreference = $false
& (Join-Path $PSScriptRoot 'Build-Local.ps1')
$package = Join-Path $root "artifacts\local-runtime\0.0.30\KingmakerGunslinger-$ExpectedVersion-local-runtime.zip"
if (-not (Test-Path -LiteralPath $package -PathType Leaf)) {
    throw "Build-Local did not produce the expected package: $package"
}
& (Join-Path $PSScriptRoot 'Deploy-Local.ps1') -PackagePath $package `
    -WhatIf -Confirm:$false

$currentOwner = [Security.Principal.WindowsIdentity]::GetCurrent().Name
$steamPreflight = Wait-KmgSteamProcess -SteamPath $SteamPath `
    -TimeoutSeconds $SteamStartupTimeoutSeconds
Assert-KmgProcessOwner -ProcessId $steamPreflight.Id -ExpectedOwner $currentOwner -Label 'Steam'

$deploymentManifestPath = & (Join-Path $PSScriptRoot 'Deploy-Local.ps1') `
    -PackagePath $package -Confirm:$false -PassThru

$evidence = Join-Path $script:KmgRuntimeEvidenceRoot (
    [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '-' + $Scenario)
New-Item -ItemType Directory -Path $evidence | Out-Null
$request = New-KmgRuntimeRequest -Scenario $Scenario -ExpectedVersion $ExpectedVersion `
    -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion $ExitAfterCompletion `
    -EvidenceDirectory $evidence -Parameters $Parameters `
    -StartupTimeoutSeconds $ObserverStartupTimeoutSeconds `
    -CatalogTimeoutSeconds $(if ($Scenario -in @(
        'observe-save-catalog-and-selection', 'observe-save-catalog-provider',
        'observe-load-game-button-action', 'working-save-smoke')) {
        $CatalogTimeoutSeconds } else { 0 }) `
    -SelectionTimeoutSeconds $(if ($Scenario -in @(
        'observe-save-catalog-and-selection', 'working-save-smoke')) {
        $SelectionTimeoutSeconds } else { 0 }) `
    -CompletionTimeoutSeconds $(if ($Scenario -in @(
        'observe-save-catalog-and-selection', 'working-save-smoke')) {
        $CompletionTimeoutSeconds } else { 0 }) `
    -MainMenuTimeoutSeconds $(if ($Scenario -eq 'working-save-smoke') {
        $MainMenuTimeoutSeconds } else { 0 }) `
    -ActionResolutionTimeoutSeconds $(if ($Scenario -eq 'working-save-smoke') {
        $ActionResolutionTimeoutSeconds } else { 0 }) `
    -ActionInvocationTimeoutSeconds $(if ($Scenario -eq 'working-save-smoke') {
        $ActionInvocationTimeoutSeconds } else { 0 }) `
    -DescriptorResolutionTimeoutSeconds $(if ($Scenario -eq 'working-save-smoke') {
        $DescriptorResolutionTimeoutSeconds } else { 0 }) `
    -LoadEntryTimeoutSeconds $(if ($Scenario -eq 'working-save-smoke') {
        $LoadEntryTimeoutSeconds } else { 0 }) `
    -FingerprintTimeoutSeconds $(if ($Scenario -eq 'working-save-smoke') {
        $FingerprintTimeoutSeconds } else { 0 })
$initialized = Initialize-KmgRuntimeTestEvidence -EvidenceDirectory $evidence `
    -Request $request -DeploymentManifestPath $deploymentManifestPath
$requestPath = $initialized.requestPath
$resultPath = $initialized.resultPath
$orchestration = $initialized.orchestration
$orchestration.stage = 'request-written'
[void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence `
    -Record $orchestration)
Write-Host 'Stage: deployment-complete'
Write-Host 'Stage: request-written'
$terminalOutcomeRecorded = $false

try {
    $preLaunchProcesses = @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue)
    $orchestration.preLaunchKingmakerProcesses = @(
        $preLaunchProcesses | Sort-Object StartTime, Id | ForEach-Object {
            [ordered]@{
                processId = $_.Id
                startedAtUtc = $_.StartTime.ToUniversalTime().ToString('o')
            }
        })
    $orchestration.stage = 'steam-launch-requested'
    [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence `
        -Record $orchestration)
    Write-Host 'Stage: steam-launch-requested'
    $launchOutput = @(Start-KmgSteamKingmaker -SteamPath $SteamPath `
        -AppId $SteamAppId -RequestPath $requestPath `
        -PreLaunchProcesses @($preLaunchProcesses) `
        -SteamStartupTimeoutSeconds $SteamStartupTimeoutSeconds `
        -GameStartupTimeoutSeconds $GameStartupTimeoutSeconds)
    if ($launchOutput.Count -ne 1) {
        throw "Steam launch must emit exactly one result; observed $($launchOutput.Count)."
    }
    $launch = $launchOutput[0]
    Assert-KmgRuntimeLaunchResult -LaunchResult $launch
    $process = $launch.kingmakerProcess
    $orchestration.launchBegan = $true
    $orchestration.steamExecutable = $launch.steamExecutable
    $orchestration.steamAppId = $launch.steamAppId
    $orchestration.sanitizedLaunchArguments = $launch.sanitizedLaunchArguments
    $orchestration.steamProcessId = $launch.steamProcessId
    $orchestration.kingmakerProcessId = $launch.kingmakerProcessId
    $orchestration.kingmakerStartedAtUtc = $launch.kingmakerStartedAtUtc.ToString('o')
    $orchestration.stage = 'owner-context-verified'
    [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence -Record $orchestration)
    Write-Host 'Stage: kingmaker-process-discovered'
    Write-Host 'Stage: owner-context-verified'

    if ($Scenario -eq 'working-save-smoke') {
        $orchestration.stage = 'waiting-for-runtime-readiness'
        [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence `
            -Record $orchestration)
        Write-Host 'Stage: waiting-for-runtime-readiness'
        $requestWrittenUtc = (Get-Item -LiteralPath $requestPath).LastWriteTimeUtc
        $readyPath = Join-Path $evidence 'runtime-ready.json'
        $readyDeadline = [DateTime]::UtcNow.AddSeconds(
            $ObserverStartupTimeoutSeconds + 15)
        $result = $null
        $ready = $null
        while (-not $ready -and $null -eq $result) {
            $result = Get-KmgCurrentRuntimeResult -ResultPath $resultPath `
                -EvidenceDirectory $evidence -RunId $request.runId `
                -Scenario $Scenario -ExpectedVersion $ExpectedVersion `
                -RequestWrittenUtc $requestWrittenUtc
            if ($null -ne $result) { break }
            if (Test-Path -LiteralPath $readyPath -PathType Leaf) {
                $candidate = Get-Content -LiteralPath $readyPath -Raw |
                    ConvertFrom-Json
                if (-not (Test-KmgRuntimeReadyMarker -Marker $candidate `
                    -RunId $request.runId -Scenario $Scenario `
                    -ExpectedVersion $ExpectedVersion -ProcessId $process.Id `
                    -RequestWrittenUtc $requestWrittenUtc)) {
                    throw 'working-save-smoke ready marker is stale or mismatched.'
                }
                $ready = $candidate
                break
            }
            # Incremental stage files are intentionally left intact and are
            # collected even when readiness or the process exit is observed.
            $process.Refresh()
            if ($process.HasExited) {
                $result = Wait-KmgRuntimeResultFlushGrace `
                    -ResultPath $resultPath -EvidenceDirectory $evidence `
                    -RunId $request.runId -Scenario $Scenario `
                    -ExpectedVersion $ExpectedVersion `
                    -RequestWrittenUtc $requestWrittenUtc
                if ($null -eq $result) {
                    throw 'Kingmaker exited before working-save-smoke readiness and no final result appeared during flush grace.'
                }
                break
            }
            if ([DateTime]::UtcNow -ge $readyDeadline) {
                throw 'working-save-smoke readiness timed out. stage=runtime-readiness'
            }
            Start-Sleep -Milliseconds 250
        }
        if ($ready) {
            $orchestration.observerReadyAtUtc = $ready.readinessTimestampUtc
            $orchestration.guardedRequestAccepted = $true
            $orchestration.stage = 'runtime-ready'
            [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence `
                -Record $orchestration)
            Write-Host 'Stage: runtime-ready'
        }
        elseif ($null -ne $result) {
            $orchestration.readinessObservationMissed = $true
            $orchestration.warnings = @(
                'A valid final result advanced beyond orchestration readiness observation.')
        }
    }
    elseif ($Scenario -in @('observe-save-catalog-provider',
        'observe-load-game-button-action')) {
        $requestWrittenUtc = (Get-Item -LiteralPath $requestPath).LastWriteTimeUtc
        $readyPath = Join-Path $evidence 'runtime-ready.json'
        $readyDeadline = [DateTime]::UtcNow.AddSeconds(
            $ObserverStartupTimeoutSeconds + 15)
        $ready = $null
        while (-not $ready) {
            if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
                throw 'Provider observation ended before readiness. stage=observer-readiness'
            }
            $process.Refresh()
            if ($process.HasExited) {
                throw 'Kingmaker exited before provider observer readiness.'
            }
            if ([DateTime]::UtcNow -ge $readyDeadline) {
                throw 'Provider observer readiness timed out. stage=observer-readiness'
            }
            if (Test-Path -LiteralPath $readyPath -PathType Leaf) {
                $candidate = Get-Content -LiteralPath $readyPath -Raw | ConvertFrom-Json
                if (Test-KmgRuntimeReadyMarker -Marker $candidate `
                    -RunId $request.runId -Scenario $Scenario `
                    -ExpectedVersion $ExpectedVersion -ProcessId $process.Id `
                    -RequestWrittenUtc $requestWrittenUtc) {
                    $ready = $candidate
                }
                else {
                    throw 'Provider ready marker is stale or mismatched.'
                }
            }
            if (-not $ready) { Start-Sleep -Milliseconds 250 }
        }
        $orchestration.observerReadyAtUtc = $ready.readinessTimestampUtc
        [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence `
            -Record $orchestration)
        $providerInstruction = 'CLICK ' + 'LOAD GAME ONCE NOW'
        Write-Host $providerInstruction -ForegroundColor Yellow
        Write-Host 'DO NOT SELECT OR LOAD A SAVE' -ForegroundColor Red
    }
    elseif ($Scenario -eq 'observe-save-catalog-and-selection') {
        $requestWrittenUtc = (Get-Item -LiteralPath $requestPath).LastWriteTimeUtc
        $stageAPath = Join-Path $evidence 'runtime-catalog-ready.json'
        $stageADeadline = [DateTime]::UtcNow.AddSeconds(
            $ObserverStartupTimeoutSeconds + 15)
        while (-not (Test-Path -LiteralPath $stageAPath -PathType Leaf)) {
            $process.Refresh()
            if ($process.HasExited) {
                throw 'Kingmaker exited before Stage A. stage=catalog-observer-ready'
            }
            if ([DateTime]::UtcNow -ge $stageADeadline) {
                throw 'Stage A timed out. stage=catalog-observer-ready'
            }
            Start-Sleep -Milliseconds 250
        }
        $stageA = Get-Content -LiteralPath $stageAPath -Raw | ConvertFrom-Json
        if (-not (Test-KmgRuntimeStageMarker -Marker $stageA `
            -RunId $request.runId -Scenario $Scenario `
            -Stage 'catalog-observer-ready' -ExpectedVersion $ExpectedVersion `
            -ProcessId $process.Id -RequestWrittenUtc $requestWrittenUtc)) {
            throw 'Stage A marker is stale or mismatched.'
        }
        Write-Host 'OPEN THE LOAD GAME SCREEN NOW' -ForegroundColor Yellow
        Write-Host 'DO NOT SELECT A SAVE YET' -ForegroundColor Red

        $stageBPath = Join-Path $evidence 'runtime-catalog-captured.json'
        $stageBDeadline = [DateTime]::UtcNow.AddSeconds($CatalogTimeoutSeconds + 15)
        while (-not (Test-Path -LiteralPath $stageBPath -PathType Leaf)) {
            if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
                throw 'Observation ended before Stage B. stage=catalog-capture'
            }
            $process.Refresh()
            if ($process.HasExited) {
                throw 'Kingmaker exited before Stage B. stage=catalog-capture'
            }
            if ([DateTime]::UtcNow -ge $stageBDeadline) {
                throw 'Stage B timed out. stage=catalog-capture'
            }
            Start-Sleep -Milliseconds 250
        }
        $stageB = Get-Content -LiteralPath $stageBPath -Raw | ConvertFrom-Json
        if (-not (Test-KmgRuntimeStageMarker -Marker $stageB `
            -RunId $request.runId -Scenario $Scenario -Stage 'catalog-captured' `
            -ExpectedVersion $ExpectedVersion -ProcessId $process.Id `
            -RequestWrittenUtc $requestWrittenUtc)) {
            throw 'Stage B marker is stale or mismatched.'
        }
        Write-Host 'SAVE CATALOG CAPTURED' -ForegroundColor Green
        Write-Host 'SELECT AND LOAD KMG_AUTOMATION_WORKING NOW' -ForegroundColor Yellow
        Write-Host 'DO NOT SELECT KMG_AUTOMATION_BASELINE' -ForegroundColor Red
    }
    elseif ($ManualInteractionRequired) {
        $readyPath = Join-Path $evidence 'runtime-ready.json'
        $readyDeadline = [DateTime]::UtcNow.AddSeconds($ObserverStartupTimeoutSeconds + 15)
        $ready = $null
        while (-not $ready) {
            if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
                $startupResult = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
                throw "Observer failed before readiness. stage=observer-readiness; status=$($startupResult.status); diagnostics=$($startupResult.diagnostics -join ';')"
            }
            $process.Refresh()
            if ($process.HasExited) {
                throw "Kingmaker exited before observer readiness. stage=observer-readiness; PID=$($process.Id)"
            }
            if ([DateTime]::UtcNow -ge $readyDeadline) {
                throw "Observer readiness timed out. stage=observer-readiness; timeoutSeconds=$ObserverStartupTimeoutSeconds"
            }
            if (Test-Path -LiteralPath $readyPath -PathType Leaf) {
                try {
                    $candidate = Get-Content -LiteralPath $readyPath -Raw | ConvertFrom-Json
                    $requestWrittenUtc = (Get-Item -LiteralPath $requestPath).LastWriteTimeUtc
                    if (Test-KmgRuntimeReadyMarker -Marker $candidate `
                        -RunId $request.runId -Scenario $Scenario `
                        -ExpectedVersion $ExpectedVersion -ProcessId $process.Id `
                        -RequestWrittenUtc $requestWrittenUtc) {
                        $ready = $candidate
                    }
                    else {
                        throw 'Ready marker identity, freshness, version, process, or hooks did not match this run.'
                    }
                }
                catch {
                    throw "Invalid observer ready marker. stage=observer-readiness; $($_.Exception.Message)"
                }
            }
            if (-not $ready) { Start-Sleep -Milliseconds 250 }
        }
        $orchestration.observerReadyAtUtc = $ready.readinessTimestampUtc
        $orchestration.manualInteractionTimeoutBeganAtUtc = [DateTime]::UtcNow.ToString('o')
        [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence -Record $orchestration)
        Write-Host ''
        Write-Host '============================================================' -ForegroundColor Yellow
        Write-Host 'MANUALLY LOAD KMG_AUTOMATION_WORKING NOW' -ForegroundColor Yellow
        Write-Host 'DO NOT LOAD OR OVERWRITE KMG_AUTOMATION_BASELINE' -ForegroundColor Red
        Write-Host 'No keyboard or mouse input will be sent by this script.' -ForegroundColor Yellow
        Write-Host '============================================================' -ForegroundColor Yellow
        Write-Host ''
    }

    if ($Scenario -eq 'working-save-smoke') {
        $deadline = [DateTime]::UtcNow.AddSeconds(
            $MainMenuTimeoutSeconds + $ActionResolutionTimeoutSeconds +
            $ActionInvocationTimeoutSeconds + $CatalogTimeoutSeconds +
            $DescriptorResolutionTimeoutSeconds + $LoadEntryTimeoutSeconds +
            $CompletionTimeoutSeconds + $FingerprintTimeoutSeconds + 30)
    }
    elseif ($Scenario -eq 'observe-save-catalog-and-selection') {
        $deadline = [DateTime]::UtcNow.AddSeconds(
            $SelectionTimeoutSeconds + $CompletionTimeoutSeconds + 15)
    }
    elseif ($Scenario -in @('observe-save-catalog-provider',
        'observe-load-game-button-action')) {
        $deadline = [DateTime]::UtcNow.AddSeconds($CatalogTimeoutSeconds + 15)
    }
    else {
        $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds + 15)
    }
    $orchestration.stage = 'waiting-for-final-result'
    [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence `
        -Record $orchestration)
    Write-Host 'Stage: waiting-for-final-result'
    while ($null -eq $result) {
        $result = Get-KmgCurrentRuntimeResult -ResultPath $resultPath `
            -EvidenceDirectory $evidence -RunId $request.runId `
            -Scenario $Scenario -ExpectedVersion $ExpectedVersion `
            -RequestWrittenUtc $requestWrittenUtc
        if ($null -ne $result) { break }
        $process.Refresh()
        if ($process.HasExited) {
            $result = Wait-KmgRuntimeResultFlushGrace `
                -ResultPath $resultPath -EvidenceDirectory $evidence `
                -RunId $request.runId -Scenario $Scenario `
                -ExpectedVersion $ExpectedVersion `
                -RequestWrittenUtc $requestWrittenUtc
            if ($null -eq $result) {
                $orchestration.stage = 'process-exited-early'
                [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence `
                    -Record $orchestration)
                Write-Host 'Stage: process-exited-early'
                throw "Kingmaker exited before committing a result after final rescan and bounded flush grace. stage=waiting-for-final-result; PID=$($process.Id); exitCode=$($process.ExitCode)"
            }
            break
        }
        if ([DateTime]::UtcNow -ge $deadline) {
            if ($AllowForceTerminate) {
                Stop-Process -Id $process.Id -Force
                throw "Runtime result timed out and explicitly authorized force termination was used. PID=$($process.Id)"
            }
            throw "Runtime result timed out; Kingmaker was left running. PID=$($process.Id)"
        }
        Start-Sleep -Milliseconds 500
    }

    $orchestration.guardedRequestAccepted = ($result.runId -eq $request.runId)
    $orchestration.status = $result.status
    $orchestration.stage = 'final-result-received'
    $orchestration.completedAtUtc = [DateTime]::UtcNow.ToString('o')
    [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence -Record $orchestration)
    $terminalOutcomeRecorded = $true
    & (Join-Path $PSScriptRoot 'Collect-Runtime-Evidence.ps1') `
        -EvidenceDirectory $evidence -PackagePath $package
    Write-Host "Runtime evidence manifest: $(Join-Path $evidence 'evidence-manifest.json')"
    Write-Host "Runtime result: $resultPath"
    Write-Host "Status: $($result.status)"
    Write-Host 'Stage: final-result-received'
    if ($result.status -ne 'PASS') { exit 1 }
}
catch {
    $orchestration.status = 'ERROR'
    $orchestration.stage = 'orchestration-error'
    $orchestration.completedAtUtc = [DateTime]::UtcNow.ToString('o')
    $orchestration.exception = [ordered]@{
        type = $_.Exception.GetType().FullName
        message = $_.Exception.Message
    }
    [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence -Record $orchestration)
    $terminalOutcomeRecorded = $true
    Write-Host 'Stage: orchestration-error'
    throw
}
finally {
    if (-not $terminalOutcomeRecorded) {
        $orchestration.status = 'ERROR'
        $orchestration.stage = 'orchestration-error'
        $orchestration.completedAtUtc = [DateTime]::UtcNow.ToString('o')
        $orchestration.exception = [ordered]@{
            type = 'System.Management.Automation.PipelineStoppedException'
            message = 'The orchestration pipeline ended without a final runtime result or explicit caught error.'
        }
        [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence `
            -Record $orchestration)
        Write-Host 'Stage: orchestration-error'
    }
}
