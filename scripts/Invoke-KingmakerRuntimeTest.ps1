[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('mod-load-smoke', 'observe-manual-save-load')]
    [string]$Scenario,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [ValidateRange(5, 1800)]
    [int]$TimeoutSeconds = 120,

    [bool]$ExitAfterCompletion = $true,
    [hashtable]$Parameters = @{},
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

if ($Scenario -eq 'observe-manual-save-load' -and -not $ManualInteractionRequired) {
    throw 'observe-manual-save-load requires -ManualInteractionRequired.'
}
if ($Scenario -ne 'observe-manual-save-load' -and $ManualInteractionRequired) {
    throw '-ManualInteractionRequired is valid only for observe-manual-save-load.'
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

& {
    # The orchestrator's -WhatIf controls deployment and launch. Build-Local is
    # an ordinary source/artifact qualification and must not inherit that
    # preference or its internal staging operations become inconsistent.
    $WhatIfPreference = $false
    & (Join-Path $PSScriptRoot 'Build-Local.ps1')
}
$package = Join-Path $root "artifacts\local-runtime\0.0.30\KingmakerGunslinger-$ExpectedVersion-local-runtime.zip"
if (-not (Test-Path -LiteralPath $package -PathType Leaf)) {
    throw "Build-Local did not produce the expected package: $package"
}

& (Join-Path $PSScriptRoot 'Deploy-Local.ps1') -PackagePath $package -WhatIf
if (-not $PSCmdlet.ShouldProcess(
    "Steam App ID $SteamAppId",
    "deploy the validated package and launch scenario '$Scenario'")) {
    Write-Host 'Source-only/WhatIf validation passed. No deployment or process launch occurred.'
    return
}

$currentOwner = [Security.Principal.WindowsIdentity]::GetCurrent().Name
$steamPreflight = Wait-KmgSteamProcess -SteamPath $SteamPath `
    -TimeoutSeconds $SteamStartupTimeoutSeconds
Assert-KmgProcessOwner -ProcessId $steamPreflight.Id -ExpectedOwner $currentOwner -Label 'Steam'

& (Join-Path $PSScriptRoot 'Deploy-Local.ps1') -PackagePath $package -Confirm:$false

$evidence = Join-Path $script:KmgRuntimeEvidenceRoot (
    [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '-' + $Scenario)
New-Item -ItemType Directory -Path $evidence | Out-Null
$request = New-KmgRuntimeRequest -Scenario $Scenario -ExpectedVersion $ExpectedVersion `
    -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion $ExitAfterCompletion `
    -EvidenceDirectory $evidence -Parameters $Parameters
$requestPath = Join-Path $evidence 'runtime-request.json'
Write-KmgUtf8NoBom -Path $requestPath `
    -Content (($request | ConvertTo-Json -Depth 8) + [Environment]::NewLine)

$preLaunchProcesses = @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue)
$orchestration = [ordered]@{
    schemaVersion = 2
    runId = $request.runId
    status = 'ACTIVE'
    startedAtUtc = [DateTime]::UtcNow.ToString('o')
    requestPath = $requestPath
    guardedRequestAccepted = $false
    preLaunchKingmakerProcesses = @($preLaunchProcesses | Sort-Object StartTime, Id |
        ForEach-Object {
            [ordered]@{
                processId = $_.Id
                startedAtUtc = $_.StartTime.ToUniversalTime().ToString('o')
            }
        })
}
[void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence -Record $orchestration)

try {
    $launch = Start-KmgSteamKingmaker -SteamPath $SteamPath -AppId $SteamAppId `
        -RequestPath $requestPath -PreLaunchProcesses @($preLaunchProcesses) `
        -SteamStartupTimeoutSeconds $SteamStartupTimeoutSeconds `
        -GameStartupTimeoutSeconds $GameStartupTimeoutSeconds
    $process = $launch.kingmakerProcess
    $orchestration.steamExecutable = $launch.steamExecutable
    $orchestration.steamAppId = $launch.steamAppId
    $orchestration.sanitizedLaunchArguments = $launch.sanitizedLaunchArguments
    $orchestration.steamProcessId = $launch.steamProcessId
    $orchestration.kingmakerProcessId = $launch.kingmakerProcessId
    $orchestration.kingmakerStartedAtUtc = $launch.kingmakerStartedAtUtc.ToString('o')
    [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence -Record $orchestration)

    if ($ManualInteractionRequired) {
        Write-Host ''
        Write-Host '============================================================' -ForegroundColor Yellow
        Write-Host 'MANUALLY LOAD KMG_AUTOMATION_WORKING NOW' -ForegroundColor Yellow
        Write-Host 'DO NOT LOAD OR OVERWRITE KMG_AUTOMATION_BASELINE' -ForegroundColor Red
        Write-Host 'No keyboard or mouse input will be sent by this script.' -ForegroundColor Yellow
        Write-Host '============================================================' -ForegroundColor Yellow
        Write-Host ''
    }

    $resultPath = Join-Path $evidence 'runtime-result.json'
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds + 15)
    while (-not (Test-Path -LiteralPath $resultPath -PathType Leaf)) {
        $process.Refresh()
        if ($process.HasExited) {
            throw "Kingmaker exited before committing a result. PID=$($process.Id); exitCode=$($process.ExitCode)"
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

    $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
    $orchestration.guardedRequestAccepted = ($result.runId -eq $request.runId)
    $orchestration.status = $result.status
    $orchestration.completedAtUtc = [DateTime]::UtcNow.ToString('o')
    [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence -Record $orchestration)
    & (Join-Path $PSScriptRoot 'Collect-Runtime-Evidence.ps1') `
        -EvidenceDirectory $evidence -PackagePath $package
    Write-Host "Runtime result: $resultPath"
    Write-Host "Status: $($result.status)"
    if ($result.status -ne 'PASS') { exit 1 }
}
catch {
    $orchestration.status = 'ERROR'
    $orchestration.completedAtUtc = [DateTime]::UtcNow.ToString('o')
    $orchestration.exception = [ordered]@{
        type = $_.Exception.GetType().FullName
        message = $_.Exception.Message
    }
    [void](Write-KmgOrchestrationEvidence -EvidenceDirectory $evidence -Record $orchestration)
    throw
}
