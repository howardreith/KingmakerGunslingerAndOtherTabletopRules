[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.108',
    [ValidateSet('KMG_AUTOMATION_WORKING')]
    [string]$SaveName = 'KMG_AUTOMATION_WORKING',
    [ValidateRange(120, 900)][int]$TimeoutSeconds = 300,
    [ValidateSet('prepare', 'verify-cleanup', 'verify-absent')]
    [string]$StartPhase = 'prepare',
    [switch]$AllowDirtyGit,
    [switch]$ConfirmEach,
    [switch]$ReuseInstalledArtifact,
    [string]$DeploymentManifestPath,
    [string]$PackagePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$invoke = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$build = Join-Path $PSScriptRoot 'Build-Local.ps1'
$deploy = Join-Path $PSScriptRoot 'Deploy-Local.ps1'

if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
    throw 'Pathfinder: Kingmaker must not be running before persistence qualification.'
}
if ($ReuseInstalledArtifact -and
    ([string]::IsNullOrWhiteSpace($DeploymentManifestPath) -or
     [string]::IsNullOrWhiteSpace($PackagePath))) {
    throw '-ReuseInstalledArtifact requires deployment and package paths.'
}
if (-not $ReuseInstalledArtifact -and
    (-not [string]::IsNullOrWhiteSpace($DeploymentManifestPath) -or
     -not [string]::IsNullOrWhiteSpace($PackagePath))) {
    throw 'Deployment/package paths are valid only with -ReuseInstalledArtifact.'
}
if (-not $PSCmdlet.ShouldProcess($SaveName,
    'run the authorized three-launch canonical fatigue persistence sequence')) {
    return
}

$ConfirmPreference = 'None'

function Wait-ForGuardedKingmakerExit([string]$phase) {
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and
        [DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 250
    }
    if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
        throw "Kingmaker did not exit within 45 seconds after fatigue persistence $phase."
    }
}

if (-not $ReuseInstalledArtifact) {
    & $build
    $PackagePath = Join-Path $root (
        "artifacts\local-runtime\$ExpectedVersion\KingmakerGunslinger-$ExpectedVersion-local-runtime.zip")
    if (-not (Test-Path -LiteralPath $PackagePath -PathType Leaf)) {
        throw "Build-Local did not produce the expected package: $PackagePath"
    }
    & $deploy -PackagePath $PackagePath -WhatIf -Confirm:$false
    $DeploymentManifestPath = & $deploy -PackagePath $PackagePath `
        -Confirm:$false -PassThru
}

$phases = @(
    [pscustomobject]@{
        Name = 'prepare'; Scenario = 'working-save-fatigue-prepare'
    },
    [pscustomobject]@{
        Name = 'verify-cleanup'
        Scenario = 'working-save-fatigue-verify-cleanup'
    },
    [pscustomobject]@{
        Name = 'verify-absent'
        Scenario = 'working-save-fatigue-verify-absent'
    }
)
$start = switch ($StartPhase) {
    'prepare' { 0 }
    'verify-cleanup' { 1 }
    'verify-absent' { 2 }
    default { throw "Unsupported start phase: $StartPhase" }
}

for ($index = $start; $index -lt $phases.Count; $index++) {
    $phase = $phases[$index]
    & $invoke -Scenario $phase.Scenario `
        -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
        -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
        -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach `
        -ReuseInstalledArtifact `
        -DeploymentManifestPath $DeploymentManifestPath `
        -PackagePath $PackagePath
    if ($LASTEXITCODE -ne 0) {
        throw "Canonical fatigue persistence $($phase.Name) failed."
    }
    Wait-ForGuardedKingmakerExit $phase.Name
}

Write-Host "Canonical fatigue three-launch working-save persistence PASS; package=$PackagePath; deployment=$DeploymentManifestPath"
