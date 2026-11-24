[CmdletBinding()]
param(
    # The loader reference root (the folder holding UnityModManager\UnityModManager.dll and UnityModManager\0Harmony12.dll).
    [Parameter(Mandatory = $true)][string]$LoaderReferenceDir,
    # The source Info.json; its ManagerVersion must equal the pinned floor.
    [string]$InfoPath
)

# Called by the product project before references resolve (every MSBuild compile: direct, build.ps1 and the gates).
# The loader files the compiler is given must be the pinned 0.31.1 files (scripts/loader-reference-pins.json), by
# SHA-256; anything else, including the installed Unity Mod Manager, is refused.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ReferenceProvenance.Common.ps1')
try {
    $verified = @(Assert-KmgPinnedLoaderReferences -LoaderReferenceDir $LoaderReferenceDir)
    if ($InfoPath) {
        $info = Get-Content -LiteralPath $InfoPath -Raw | ConvertFrom-Json
        $floor = (Get-KmgLoaderReferencePins).MinimumManagerVersion
        if (-not ($info.ManagerVersion -is [string]) -or $info.ManagerVersion -cne $floor) { throw "Info.json ManagerVersion '$($info.ManagerVersion)' must equal the pinned loader floor $floor." }
    }
    Write-Host ('Pinned loader references verified: ' + (($verified | ForEach-Object { $_.relativePath + ' ' + $_.assemblyVersion }) -join ', '))
}
catch {
    $message = $_.Exception.Message
    # The pinned cache of this repository (artifacts\references\loader) or another loader folder (for example an
    # installed newer loader passed as /p:UnityModManagerDir): files outside the cache are never to be changed.
    $repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
    $pinnedCache = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts\references\loader')).TrimEnd('\')
    $isPinnedCache = [string]::Equals([IO.Path]::GetFullPath($LoaderReferenceDir).TrimEnd('\'), $pinnedCache, [StringComparison]::OrdinalIgnoreCase)
    $useCache = ' Build against the verified cache instead: run scripts\build.ps1 -CompileOnly once (it fetches and verifies the pinned files into artifacts\references\loader), then build again without /p:UnityModManagerDir (or with /p:UnityModManagerDir=<repository>\artifacts\references\loader\UnityModManager).'
    $advice = if ($message -like '*differs from its pin*') {
        if ($isPinnedCache) { ' The cache file has different bytes and was left untouched: inspect it, move or delete that one file yourself, then run scripts\build.ps1 -CompileOnly to fetch the pinned copy (see docs\BUILDING.md).' }
        else { ' This loader folder is not the pinned cache and was left untouched; do not change it.' + $useCache }
    } elseif ($message -like '*missing*' -or $message -like '*not found*') {
        if ($isPinnedCache) { ' Run scripts\build.ps1 -CompileOnly: it fetches and verifies the absent pinned files into artifacts\references\loader.' }
        else { ' This loader folder is not the pinned cache.' + $useCache }
    } else { ' See docs\BUILDING.md (pinned loader references).' }
    [Console]::Error.WriteLine('error KMGPIN: ' + $message + $advice)
    exit 1
}
