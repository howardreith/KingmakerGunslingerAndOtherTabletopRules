[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.104',
    [ValidateSet('KMG_AUTOMATION_WORKING')]
    [string]$SaveName = 'KMG_AUTOMATION_WORKING',
    [ValidateRange(120, 900)][int]$TimeoutSeconds = 300,
    [ValidateSet('prepare', 'cleanup')]
    [string]$StartPhase = 'prepare',
    [switch]$VerifyBrownFurOff,
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
$settings = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger\FeatureModules.json'
$expectedParent = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger'
if ([IO.Path]::GetFullPath((Split-Path -Parent $settings)).TrimEnd('\') -cne
    $expectedParent) { throw 'Feature-module settings target changed unexpectedly.' }
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
    'run the authorized two-launch Brown-Fur persistence sequence')) {
    return
}

$ConfirmPreference = 'None'
$originalExists = Test-Path -LiteralPath $settings -PathType Leaf
$originalBytes = if ($originalExists) {
    [IO.File]::ReadAllBytes($settings)
} else { $null }
$failure = $null

function Set-BrownFurEnabled([bool]$enabled) {
    $configuration = [ordered]@{
        schemaVersion = 8
        gunslinger = $true
        'acadamae-graduate' = $true
        'shield-other' = $true
        'expanded-summoning' = $true
        'elven-branched-spears' = $true
        'eastern-weapons' = $true
        'brown-fur-transmuter' = $enabled
        'urban-barbarian' = $true
        'bodyguard-feats' = $true
    }
    $temporary = $settings + '.kmg-brown-fur-persistence.tmp'
    [IO.File]::WriteAllText($temporary,
        ($configuration | ConvertTo-Json -Depth 4),
        (New-Object Text.UTF8Encoding($false)))
    Move-Item -LiteralPath $temporary -Destination $settings -Force
}

function Restore-OriginalFeatureState {
    if ($originalExists) {
        $temporary = $settings + '.kmg-brown-fur-persistence-restore.tmp'
        [IO.File]::WriteAllBytes($temporary, $originalBytes)
        Move-Item -LiteralPath $temporary -Destination $settings -Force
    }
    elseif (Test-Path -LiteralPath $settings) {
        Remove-Item -LiteralPath $settings -Force
    }
}

function Wait-ForGuardedKingmakerExit([string]$phase) {
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and
        [DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 250
    }
    if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
        throw "Kingmaker did not exit within 45 seconds after Brown-Fur persistence $phase."
    }
}

try {
    Set-BrownFurEnabled $true
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
        & $invoke -Scenario 'working-save-brown-fur-prepare' `
            -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
            -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
            -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach `
            -ReuseInstalledArtifact `
            -DeploymentManifestPath $DeploymentManifestPath `
            -PackagePath $PackagePath
        if ($LASTEXITCODE -ne 0) {
            throw 'Brown-Fur persistence prepare failed.'
        }
        Wait-ForGuardedKingmakerExit 'prepare'
    }

    if ($VerifyBrownFurOff) { Set-BrownFurEnabled $false }
    $verifyScenario = if ($VerifyBrownFurOff) {
        'working-save-brown-fur-off-verify-cleanup'
    } else { 'working-save-brown-fur-verify-cleanup' }
    & $invoke -Scenario $verifyScenario `
        -ExpectedVersion $ExpectedVersion -SaveName $SaveName `
        -TimeoutSeconds $TimeoutSeconds -ExitAfterCompletion:$true `
        -AllowDirtyGit:$AllowDirtyGit -Confirm:$ConfirmEach `
        -ReuseInstalledArtifact `
        -DeploymentManifestPath $DeploymentManifestPath `
        -PackagePath $PackagePath
    if ($LASTEXITCODE -ne 0) {
        throw 'Brown-Fur persistence verify/cleanup failed.'
    }
    Wait-ForGuardedKingmakerExit 'verify-cleanup'
}
catch { $failure = $_ }
finally {
    Restore-OriginalFeatureState
    $restoredExists = Test-Path -LiteralPath $settings -PathType Leaf
    if ($restoredExists -ne $originalExists) {
        throw 'Feature settings existence was not restored exactly.'
    }
    if ($originalExists) {
        $restored = [IO.File]::ReadAllBytes($settings)
        if ([Convert]::ToBase64String($restored) -cne
            [Convert]::ToBase64String($originalBytes)) {
            throw 'Feature settings bytes were not restored exactly.'
        }
    }
}
if ($failure -ne $null) { throw $failure }
Write-Host "Brown-Fur two-launch working-save persistence PASS; package=$PackagePath; deployment=$DeploymentManifestPath"
