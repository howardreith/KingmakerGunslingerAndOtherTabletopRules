[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SourceCopyPath,
    [string]$ExpectedVersion = '0.0.106',
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

$expectedHash = 'B4D6D093EABAB2080E8AE4D8A501B56449E0FC8D7850C0527495BA853032655D'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$artifactRoot = (Resolve-Path (Join-Path $repoRoot 'artifacts\save-forensics')).Path
$source = (Resolve-Path -LiteralPath $SourceCopyPath).Path
if (-not $source.StartsWith($artifactRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'SourceCopyPath must be under artifacts/save-forensics.'
}
if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -cne $expectedHash) {
    throw 'The source copy does not match the exact affected-save intake hash.'
}

$saveRoot = Join-Path $env:USERPROFILE `
    'AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Saved Games'
$original = Join-Path $saveRoot 'Quick_6.zks'
$staged = Join-Path $saveRoot 'KMG_P0_FOCUSED_AIM_AFFECTED_COPY.zks'
$saveRootFull = [IO.Path]::GetFullPath($saveRoot).TrimEnd('\')
$stagedFull = [IO.Path]::GetFullPath($staged)
if (-not $stagedFull.StartsWith($saveRootFull + '\',
        [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The staged-save path escaped the exact Kingmaker save directory.'
}
if (-not (Test-Path -LiteralPath $original -PathType Leaf) -or
    (Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash -cne $expectedHash) {
    throw 'The original affected save is missing or no longer matches the intake hash.'
}
if (Test-Path -LiteralPath $staged) {
    throw 'The transaction-owned staged save already exists.'
}

$outcome = 'not-started'
try {
    Copy-Item -LiteralPath $source -Destination $staged -ErrorAction Stop
    if ((Get-FileHash -LiteralPath $staged -Algorithm SHA256).Hash -cne $expectedHash) {
        throw 'The staged copy is not byte-identical before launch.'
    }
    $invoke = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
    $arguments = @{
        Scenario = 'p0-affected-focused-aim-save-load'
        ExpectedVersion = $ExpectedVersion
        SaveName = 'KMG_P0_FOCUSED_AIM_AFFECTED_COPY'
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
    if (Test-Path -LiteralPath $staged) {
        throw 'The transaction-owned staged copy could not be removed.'
    }
    if ($originalHash -cne $expectedHash) {
        throw 'The original affected save changed during the guarded transaction.'
    }
    Write-Output "P0 save transaction cleanup: $outcome; original SHA-256 unchanged."
}
