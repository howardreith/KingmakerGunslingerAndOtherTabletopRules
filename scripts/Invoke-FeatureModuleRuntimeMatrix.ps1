[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$ExpectedVersion = '0.0.107',
    [ValidateRange(5, 1800)][int]$TimeoutSeconds = 300,
    [string]$Combination = 'all',
    [bool]$ExitAfterCompletion = $true,
    [switch]$ConfirmEach,
    [switch]$AllowDirtyGit,
    [switch]$Boundary,
    [switch]$Boundary14,
    [switch]$ReuseInstalledArtifact,
    [string]$DeploymentManifestPath,
    [string]$PackagePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'FeatureModuleCatalog.ps1')
$invoke = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$settings = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger\FeatureModules.json'
$expectedParent = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger'
$resolvedParent = [IO.Path]::GetFullPath((Split-Path -Parent $settings)).TrimEnd('\')
if ($resolvedParent -cne $expectedParent) { throw 'Feature-module settings target changed unexpectedly.' }
if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
    throw 'Pathfinder: Kingmaker must not be running before a settings transaction.'
}

$originalExists = Test-Path -LiteralPath $settings -PathType Leaf
$originalBytes = if ($originalExists) { [IO.File]::ReadAllBytes($settings) } else { $null }
$sha = [Security.Cryptography.SHA256]::Create()
try {
    $originalHash = if ($originalExists) {
        ([BitConverter]::ToString($sha.ComputeHash($originalBytes))).Replace('-', '').ToLowerInvariant()
    } else { '<absent>' }
} finally { $sha.Dispose() }

$moduleCatalog = @(Get-KmgFeatureModuleCatalog)
$boundaryRequested = $Combination -ceq 'all'
if ($Boundary14) {
    Write-Warning '-Boundary14 is obsolete; it now selects the complete generic boundary matrix (20 states for nine modules).'
}
if (($Boundary -or $Boundary14) -and $Combination -ne 'all') {
    throw 'A boundary matrix cannot be combined with a single -Combination.'
}
if ($boundaryRequested) {
    $combinations = @(Get-KmgFeatureModuleConfigurations -Boundary)
} else {
    # Exhaustive enumeration remains a fast catalog/domain-test capability.
    # Runtime callers may select one focused configuration, but this launcher
    # deliberately has no generic 2^N game-launch mode.
    $combinations = @(Get-KmgFeatureModuleConfigurations)
    $selected = @($combinations | Where-Object { $_.Name -ceq $Combination })
    if ($selected.Count -ne 1) {
        throw "Unknown feature-module combination '$Combination'."
    }
    $combinations = $selected
}
if ($ReuseInstalledArtifact -and
    ([string]::IsNullOrWhiteSpace($DeploymentManifestPath) -or
     [string]::IsNullOrWhiteSpace($PackagePath))) {
    throw '-ReuseInstalledArtifact requires deployment and package paths.'
}

$failure = $null
try {
    foreach ($entry in $combinations) {
        $configuration = [ordered]@{
            schemaVersion = 8
        }
        $runtimeParameters = @{}
        foreach ($module in $moduleCatalog) {
            $enabled = [bool]$entry.Values[$module.RuntimeParameter]
            $configuration[$module.JsonKey] = $enabled
            $runtimeParameters[$module.RuntimeParameter] = $enabled
        }
        $json = $configuration | ConvertTo-Json -Depth 4
        $temporary = $settings + '.kmg-module-matrix.tmp'
        [IO.File]::WriteAllText($temporary, $json, (New-Object Text.UTF8Encoding($false)))
        Move-Item -LiteralPath $temporary -Destination $settings -Force
        $invokeArguments = @{
            Scenario = 'observe-feature-module-settings'
            ExpectedVersion = $ExpectedVersion
            TimeoutSeconds = $TimeoutSeconds
            Parameters = $runtimeParameters
            ExitAfterCompletion = $ExitAfterCompletion
            Confirm = [bool]$ConfirmEach
            AllowDirtyGit = [bool]$AllowDirtyGit
        }
        if ($ReuseInstalledArtifact) {
            $invokeArguments.ReuseInstalledArtifact = $true
            $invokeArguments.DeploymentManifestPath = $DeploymentManifestPath
            $invokeArguments.PackagePath = $PackagePath
        }
        & $invoke @invokeArguments
        if ($LASTEXITCODE -ne 0) {
            throw "Feature-module runtime combination $($entry.Name) failed."
        }
        if ($ExitAfterCompletion) {
            $exitDeadline = [DateTime]::UtcNow.AddSeconds(30)
            while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and
                [DateTime]::UtcNow -lt $exitDeadline) {
                Start-Sleep -Milliseconds 250
            }
            if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) {
                throw "Kingmaker did not exit within 30 seconds after combination $($entry.Name)."
            }
        }
    }
} catch {
    $failure = $_
} finally {
    if ($originalExists) {
        $temporary = $settings + '.kmg-module-restore.tmp'
        [IO.File]::WriteAllBytes($temporary, $originalBytes)
        Move-Item -LiteralPath $temporary -Destination $settings -Force
    } elseif (Test-Path -LiteralPath $settings) {
        Remove-Item -LiteralPath $settings -Force
    }
    $restoredExists = Test-Path -LiteralPath $settings -PathType Leaf
    if ($restoredExists -ne $originalExists) { throw 'Settings existence restoration failed.' }
    if ($originalExists) {
        $restored = [IO.File]::ReadAllBytes($settings)
        if ($restored.Length -ne $originalBytes.Length -or
            [Convert]::ToBase64String($restored) -cne [Convert]::ToBase64String($originalBytes)) {
            throw 'Settings byte-for-byte restoration failed.'
        }
    }
    Write-Host "Feature-module settings restored exactly; original SHA-256: $originalHash"
}
if ($failure -ne $null) { throw $failure }
Write-Host "Feature-module runtime matrix PASS: $($combinations.Name -join ', ')"
