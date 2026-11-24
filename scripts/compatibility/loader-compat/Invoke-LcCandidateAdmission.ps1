[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [string]$DevelopmentPurpose,
    [string]$HistoricalReleaseVersion,
    [Parameter(Mandatory = $true)][string]$AdmittedCopyDirectory
)
# Candidate admission for the loader harness, in its own process with every value passed as a bound parameter: the same
# admission Deploy-Local applies (or the recorded historical release), then a private copy with the admitted digest.
# Writes the admitted identity as one JSON line.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
. (Join-Path $repository 'scripts\RuntimeHarness.Common.ps1')
$admission = if ($HistoricalReleaseVersion) {
    Read-KmgHistoricalReleasePackage -PackagePath $PackagePath -RepositoryRoot $repository -Version $HistoricalReleaseVersion
} else {
    Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $repository -DevelopmentPurpose $DevelopmentPurpose -RequireCurrentMaterialInputs
}
$copy = New-KmgAdmittedPackageCopy -Admission $admission -Destination $AdmittedCopyDirectory
[ordered]@{ packagePath = [string]$admission.packagePath; packageSha256 = [string]$admission.packageSha256
    dllSha256 = [string]$admission.dllSha256; dllMvid = [string]$admission.dllMvid; privateCopy = $copy
    historicalRelease = [bool]$HistoricalReleaseVersion } | ConvertTo-Json -Compress
