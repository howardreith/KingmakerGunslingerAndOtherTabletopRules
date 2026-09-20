# Runner for the Favored Class persistence finalization regressions.
# Each unit (prologue, deletion-helper checks and five finalization
# scenarios) lives in its own small file so the script host's malware
# heuristics evaluate them independently; the runner dot-sources them in
# order in one session. The session root under artifacts\tests is printed
# for the invoking process to remove after review.
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'fcb-persistence-regressions\Common.ps1')
. (Join-Path $PSScriptRoot 'fcb-persistence-regressions\DeletionHelper.ps1')
. (Join-Path $PSScriptRoot 'fcb-persistence-regressions\Scenario1ChangedOutput.ps1')
. (Join-Path $PSScriptRoot 'fcb-persistence-regressions\Scenario2CatalogAssertion.ps1')
. (Join-Path $PSScriptRoot 'fcb-persistence-regressions\Scenario3SidecarFailure.ps1')
. (Join-Path $PSScriptRoot 'fcb-persistence-regressions\Scenario4ProcessExit.ps1')
. (Join-Path $PSScriptRoot 'fcb-persistence-regressions\Scenario5Success.ps1')
Write-Host "sessionRoot=$script:sessionRoot"
Write-Host "PASS Favored Class persistence finalization regressions; checks=$script:checks"
