[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('mod-load-smoke')]
    [string]$Scenario,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [ValidateRange(5, 1800)]
    [int]$TimeoutSeconds = 120,

    [bool]$ExitAfterCompletion = $true,
    [hashtable]$Parameters = @{},
    [switch]$AllowDirtyGit,
    [switch]$AllowForceTerminate,
    [string]$KingmakerPath = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Kingmaker.exe'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$git = Get-KmgGitState -RepositoryRoot $root
if (-not $AllowDirtyGit -and $git.Status.Count -ne 0) {
    throw 'Runtime execution requires a clean Git state. Use -AllowDirtyGit only for an explicitly permitted source state.'
}
Assert-KmgNotRunning
if (-not (Test-Path -LiteralPath $KingmakerPath -PathType Leaf)) {
    throw "Kingmaker executable is missing: $KingmakerPath"
}

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
    $KingmakerPath,
    "deploy the validated package and launch scenario '$Scenario'")) {
    Write-Host 'Source-only/WhatIf validation passed. No deployment or process launch occurred.'
    return
}

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

$process = Start-Process -FilePath $KingmakerPath -ArgumentList @(
    '-kmgRuntimeTestRequest', "`"$requestPath`""
) -PassThru
[ordered]@{
    schemaVersion = 1
    processId = $process.Id
    startedAtUtc = [DateTime]::UtcNow.ToString('o')
    requestPath = $requestPath
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidence 'orchestration.json') -Encoding UTF8

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
& (Join-Path $PSScriptRoot 'Collect-Runtime-Evidence.ps1') `
    -EvidenceDirectory $evidence -PackagePath $package
Write-Host "Runtime result: $resultPath"
Write-Host "Status: $($result.status)"
if ($result.status -ne 'PASS') { exit 1 }
