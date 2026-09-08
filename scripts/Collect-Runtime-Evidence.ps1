[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
    [string]$PackagePath,
    [string]$QualifiedElementalRaces114DeploymentManifestPath,
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',
    [string]$GameDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    [string[]]$LogPath = @(),
    [string[]]$ScreenshotPath = @(),
    [string]$EvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$requiredEvidenceRoot = [IO.Path]::GetFullPath('C:\Dev\KingmakerGunslingerLab\runtime-evidence').TrimEnd('\')
if (-not [IO.Path]::GetFullPath($EvidenceRoot).TrimEnd('\').Equals($requiredEvidenceRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Evidence root must be exactly: $requiredEvidenceRoot"
}
$destination = Assert-KmgPathWithin -Path $EvidenceDirectory -Root $EvidenceRoot -AllowRoot:$false
if (-not (Test-Path -LiteralPath $destination -PathType Container)) {
    throw "Caller-supplied evidence directory must already exist: $destination"
}

$records = @()
foreach ($entry in @(
    @($LogPath | ForEach-Object { [pscustomobject]@{ Path = $_; Kind = 'logs' } }) +
    @($ScreenshotPath | ForEach-Object { [pscustomobject]@{ Path = $_; Kind = 'screenshots' } })
)) {
    $lowerSource = $entry.Path.ToLowerInvariant()
    if ($lowerSource -match '\.zks$|\\saved games\\|credential|browser') {
        throw "Explicit evidence path is prohibited by collection policy: $($entry.Path)"
    }
    if ($entry.Kind -eq 'screenshots' -and [IO.Path]::GetExtension($entry.Path).ToLowerInvariant() -notin @('.png', '.jpg', '.jpeg')) {
        throw "Explicit screenshot has an unsupported extension: $($entry.Path)"
    }
    if (-not (Test-Path -LiteralPath $entry.Path -PathType Leaf)) {
        throw "Explicit evidence file is missing: $($entry.Path)"
    }
    $source = (Resolve-Path -LiteralPath $entry.Path).Path
    $kindDirectory = Join-Path $destination $entry.Kind
    New-Item -ItemType Directory -Path $kindDirectory -Force | Out-Null
    $target = Join-Path $kindDirectory ([IO.Path]::GetFileName($source))
    if (Test-Path -LiteralPath $target) { throw "Evidence collision: $target" }
    Copy-Item -LiteralPath $source -Destination $target
    $records += [ordered]@{
        kind = $entry.Kind
        source = $source
        collected = $target
        sha256 = Get-KmgSha256 -Path $target
    }
}

$git = Get-KmgGitState -RepositoryRoot $root
$info = Get-KmgModInfo -RepositoryRoot $root
$builtHash = $null
$packageVersion = $info.Version
$packageSha256 = $null
$qualifiedLegacyRuntimeOverlay = $null
if ($PackagePath -and $QualifiedElementalRaces114DeploymentManifestPath) {
    throw 'Current-source and qualified-legacy evidence package authorities are mutually exclusive.'
}
if ($PackagePath) {
    $buildManifest = Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $root
    $builtHash = $buildManifest.dllSha256
    $packageVersion = $buildManifest.version
    $packageSha256 = $buildManifest.packageSha256
}
elseif ($QualifiedElementalRaces114DeploymentManifestPath) {
    $legacy = Assert-KmgQualifiedElementalRaces114Deployment `
        -DeploymentManifestPath `
            $QualifiedElementalRaces114DeploymentManifestPath `
        -PackagePath (Join-Path $root `
            'artifacts\release\0.0.114\KingmakerGunslinger-0.0.114-elemental-races.zip') `
        -RepositoryRoot $root -AllowDirtyGit
    $builtHash = $legacy.DllSha256
    $packageVersion = $legacy.Version
    $packageSha256 = $legacy.Deployment.packageSha256
    $qualifiedLegacyRuntimeOverlay = [ordered]@{
        settingsMode = $legacy.SettingsMode
        settingsSha256 = $legacy.SettingsSha256
        runtimeGeneratedFiles = @($legacy.RuntimeGeneratedFiles)
    }
}
$deployedDll = Join-Path $LiveModDirectory 'KingmakerGunslinger.dll'
$gameExe = Join-Path $GameDirectory 'Kingmaker.exe'
$manifest = [ordered]@{
    schemaVersion = 1
    collectedAtUtc = [DateTime]::UtcNow.ToString('o')
    gitCommit = $git.Commit
    gitBranch = $git.Branch
    gitStatus = $git.Status
    packageVersion = $packageVersion
    packageSha256 = $packageSha256
    builtDllSha256 = $builtHash
    deployedDllSha256 = if (Test-Path -LiteralPath $deployedDll -PathType Leaf) { Get-KmgSha256 -Path $deployedDll } else { $null }
    gameVersion = if (Test-Path -LiteralPath $gameExe -PathType Leaf) { (Get-Item -LiteralPath $gameExe).VersionInfo.FileVersion } else { $null }
    qualifiedLegacyRuntimeOverlay = $qualifiedLegacyRuntimeOverlay
    files = $records
    exclusions = @('saves', 'credentials', 'browser data', 'unrelated user files')
}
$manifestPath = Join-Path $destination 'runtime-evidence.json'
$manifest | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
Write-Host "Runtime evidence manifest: $manifestPath"
