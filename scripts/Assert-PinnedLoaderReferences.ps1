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
    $advice = if ($message -like '*differs from its pin*') {
        ' The file is present with different bytes and was left untouched: inspect it, move or delete that one file yourself, then rerun scripts\build.ps1 to fetch the pinned copy (see docs\BUILDING.md).'
    } elseif ($message -like '*missing*' -or $message -like '*not found*') {
        ' Run scripts\build.ps1 (it fetches absent pinned files into artifacts\references\loader), or pass /p:UnityModManagerDir=<pinned root>\UnityModManager.'
    } else { ' See docs\BUILDING.md (pinned loader references).' }
    [Console]::Error.WriteLine('error KMGPIN: ' + $message + $advice)
    exit 1
}
