[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$reviewedSprint35Merge = 'e58bb50ab6a835cad266ca2abe95efddba911058'
$protectedSprint35Files = @(
    'scripts/Sprint35QualifiedRelease.Common.ps1',
    'scripts/Test-Sprint35Release.ps1'
)

$currentCommit = (& git -C $repositoryRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Current source is unavailable.' }
& git -C $repositoryRoot merge-base --is-ancestor $reviewedSprint35Merge $currentCommit
if ($LASTEXITCODE -ne 0) { throw 'Current source does not descend from the reviewed Sprint 35 merge.' }

$changedProtectedFiles = @(& git -C $repositoryRoot diff --name-only --no-renames `
    $reviewedSprint35Merge $currentCommit -- @protectedSprint35Files)
if ($LASTEXITCODE -ne 0 -or $changedProtectedFiles.Count -ne 0) {
    throw 'Closed Sprint 35 publisher policy or its refusal suite changed.'
}

$isolatedRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('kmg-closed-sprint35-' + [guid]::NewGuid().ToString('N'))
try {
    & git clone --shared --no-checkout --quiet $repositoryRoot $isolatedRoot
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the isolated Sprint 35 validation clone.' }
    & git -C $isolatedRoot checkout --detach --quiet $reviewedSprint35Merge
    if ($LASTEXITCODE -ne 0) { throw 'Could not select the reviewed Sprint 35 merge.' }

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $isolatedRoot 'scripts/Test-Sprint35Release.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Closed Sprint 35 publisher tests failed.' }
}
finally {
    if (Test-Path -LiteralPath $isolatedRoot) {
        Remove-Item -LiteralPath $isolatedRoot -Recurse -Force
    }
}

Write-Host 'Closed Sprint 35 publisher policy is unchanged and its reviewed refusal suite passed in isolation.'
