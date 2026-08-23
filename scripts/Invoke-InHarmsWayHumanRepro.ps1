[CmdletBinding()]
param(
    [string]$ExpectedVersion = '0.0.94',
    [ValidateRange(120, 900)]
    [int]$TimeoutSeconds = 600,
    [switch]$AllowDirtyGit,
    [switch]$AllowForceTerminate,
    [switch]$ReuseInstalledArtifact,
    [string]$DeploymentManifestPath,
    [string]$PackagePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$expectedHash = '3414D67CB2E5F8C4F18A952D23247DC6DD9D9F5579066EA64CA7FF29E61B8F01'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$source = Join-Path $repoRoot `
    'artifacts\save-forensics\20260823-human-in-harms-way-regression\Quick_3_HelpfulDefenderTest.protected-intake.zks'
if (-not (Test-Path -LiteralPath $source -PathType Leaf) -or
    (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -cne
        $expectedHash) {
    throw 'The protected human-repro intake is missing or has changed.'
}

$saveRoot = Join-Path $env:USERPROFILE `
    'AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Saved Games'
$original = Join-Path $saveRoot 'Quick_3.zks'
$staged = Join-Path $saveRoot 'KMG_IHW_HUMAN_REPRO_COPY.zks'
$saveRootFull = [IO.Path]::GetFullPath($saveRoot).TrimEnd('\')
$stagedFull = [IO.Path]::GetFullPath($staged)
if (-not $stagedFull.StartsWith($saveRootFull + '\',
        [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The staged-save path escaped the exact Kingmaker save directory.'
}
if (-not (Test-Path -LiteralPath $original -PathType Leaf) -or
    (Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash -cne
        $expectedHash) {
    throw 'The original human test save is missing or no longer matches intake.'
}
if (Test-Path -LiteralPath $staged) {
    throw 'The transaction-owned staged human-repro save already exists.'
}

$outcome = 'not-started'
try {
    Copy-Item -LiteralPath $source -Destination $staged -ErrorAction Stop
    if ((Get-FileHash -LiteralPath $staged -Algorithm SHA256).Hash -cne
        $expectedHash) {
        throw 'The staged human-repro copy is not byte-identical.'
    }
    $invoke = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
    $arguments = @{
        Scenario = 'disposable-in-harms-way-human-repro'
        ExpectedVersion = $ExpectedVersion
        SaveName = 'KMG_IHW_HUMAN_REPRO_COPY'
        TimeoutSeconds = $TimeoutSeconds
        CompletionTimeoutSeconds = $TimeoutSeconds
        FingerprintTimeoutSeconds = $TimeoutSeconds
        ExitAfterCompletion = $true
        Confirm = $false
    }
    if ($ReuseInstalledArtifact) { $arguments.ReuseInstalledArtifact = $true }
    if ($AllowDirtyGit) { $arguments.AllowDirtyGit = $true }
    if ($AllowForceTerminate) { $arguments.AllowForceTerminate = $true }
    if (-not [string]::IsNullOrWhiteSpace($DeploymentManifestPath)) {
        $arguments.DeploymentManifestPath = $DeploymentManifestPath
    }
    if (-not [string]::IsNullOrWhiteSpace($PackagePath)) {
        $arguments.PackagePath = $PackagePath
    }
    & $invoke @arguments
    $outcome = 'runtime-command-complete'
}
finally {
    $originalHash = if (Test-Path -LiteralPath $original -PathType Leaf) {
        (Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash
    } else { 'MISSING' }
    if (Test-Path -LiteralPath $staged -PathType Leaf) {
        Remove-Item -LiteralPath $staged -Force
    }
    $sidecarPrefix = [IO.Path]::GetFileName($staged) + '.'
    $sidecars = @(Get-ChildItem -LiteralPath $saveRoot -File | Where-Object {
        $_.Name.StartsWith($sidecarPrefix,
            [StringComparison]::OrdinalIgnoreCase)
    })
    foreach ($sidecar in $sidecars) {
        $sidecarFull = [IO.Path]::GetFullPath($sidecar.FullName)
        if (-not $sidecarFull.StartsWith($saveRootFull + '\',
                [StringComparison]::OrdinalIgnoreCase)) {
            throw 'A transaction-owned save sidecar escaped the exact save directory.'
        }
        Remove-Item -LiteralPath $sidecarFull -Force
    }
    if (Test-Path -LiteralPath $staged) {
        throw 'The transaction-owned staged human-repro copy was not removed.'
    }
    $remainingSidecars = @(Get-ChildItem -LiteralPath $saveRoot -File |
        Where-Object { $_.Name.StartsWith($sidecarPrefix,
            [StringComparison]::OrdinalIgnoreCase) })
    if ($remainingSidecars.Count -ne 0) {
        throw 'A transaction-owned staged human-repro sidecar was not removed.'
    }
    if ($originalHash -cne $expectedHash) {
        throw 'The original human test save changed during the transaction.'
    }
    Write-Output "In Harms Way save transaction cleanup: $outcome; original SHA-256 unchanged."
}
