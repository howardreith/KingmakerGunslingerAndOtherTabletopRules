[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.100',
    [ValidateSet('KMG_AUTOMATION_WORKING')]
    [string]$SaveName = 'KMG_AUTOMATION_WORKING',
    [ValidateRange(120, 900)][int]$TimeoutSeconds = 300,
    [ValidateSet('prepare', 'cleanup')]
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
    throw 'Pathfinder: Kingmaker must not be running before CMI persistence qualification.'
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
    'run the authorized two-launch CMI custom-firearm persistence sequence')) {
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
        throw "Kingmaker did not exit within 45 seconds after CMI persistence $phase."
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

if ($StartPhase -ceq 'prepare') {
    & $invoke -Scenario 'working-save-craft-magic-items-prepare' `
        -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
        -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
        -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach `
        -ReuseInstalledArtifact `
        -DeploymentManifestPath $DeploymentManifestPath `
        -PackagePath $PackagePath
    if ($LASTEXITCODE -ne 0) {
        throw 'CMI persistence prepare failed.'
    }
    Wait-ForGuardedKingmakerExit 'prepare'
}

& $invoke -Scenario 'working-save-craft-magic-items-verify-cleanup' `
    -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
    -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
    -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach `
    -ReuseInstalledArtifact `
    -DeploymentManifestPath $DeploymentManifestPath `
    -PackagePath $PackagePath
if ($LASTEXITCODE -ne 0) {
    throw 'CMI persistence verify/cleanup failed.'
}
Wait-ForGuardedKingmakerExit 'verify-cleanup'

Write-Host "CMI two-launch working-save persistence PASS; package=$PackagePath; deployment=$DeploymentManifestPath"
