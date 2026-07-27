[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [string]$ExpectedCommit,
    [switch]$AllowDirty,
    [string]$GameDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',
    [string]$UnityModManagerPath = 'C:\Users\howar\Documents\UnityModManagerInstaller\UnityModManager.exe',
    [string]$MSBuildPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot

$gitCommand = Get-Command git -ErrorAction Stop
$pythonCommand = Get-Command python -ErrorAction Stop
$msbuild = Resolve-KmgMsBuild -ExplicitPath $MSBuildPath
$net47 = 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7'
foreach ($path in @($GameDirectory, $LiveModDirectory)) {
    if (-not (Test-Path -LiteralPath $path -PathType Container)) { throw "Required directory is missing: $path" }
}
if (-not (Test-Path -LiteralPath $UnityModManagerPath -PathType Leaf)) {
    throw "Unity Mod Manager executable is missing: $UnityModManagerPath"
}
if (-not (Test-Path -LiteralPath (Join-Path $net47 'mscorlib.dll') -PathType Leaf)) {
    throw '.NET Framework 4.7 reference support is missing.'
}
if (-not (Test-KmgDirectoryWritableByAcl -Path $LiveModDirectory)) {
    throw 'Current security token does not have effective write/create ACL rights on the live mod directory.'
}
Assert-KmgNotRunning
$manifest = Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $root
$git = Get-KmgGitState -RepositoryRoot $root
if ($ExpectedCommit -and $git.Commit -ne $ExpectedCommit) {
    throw "Repository commit mismatch. Expected $ExpectedCommit; observed $($git.Commit)."
}
if (-not $AllowDirty -and $git.Status.Count -gt 0) {
    throw 'Repository worktree is not clean.'
}

[ordered]@{
    status = 'PASS'
    git = $gitCommand.Source
    python = $pythonCommand.Source
    msbuild = $msbuild
    netFramework47 = $net47
    gameDirectory = (Resolve-Path -LiteralPath $GameDirectory).Path
    unityModManager = (Resolve-Path -LiteralPath $UnityModManagerPath).Path
    liveModDirectory = (Resolve-Path -LiteralPath $LiveModDirectory).Path
    liveModWritableByAcl = $true
    kingmakerRunning = $false
    packageVersion = $manifest.version
    packageSha256 = $manifest.packageSha256
    commit = $git.Commit
    branch = $git.Branch
    dirtyPaths = $git.Status
} | ConvertTo-Json -Depth 5
