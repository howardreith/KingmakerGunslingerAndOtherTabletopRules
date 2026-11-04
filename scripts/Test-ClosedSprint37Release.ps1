[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Sprint 37's release test derives the live summoning surface and pins it at
# that sprint's totals - 131 creatures, 1,185 generated roots, 1,214 choices -
# so registering any later creature fails it. That is the test doing its job,
# not a regression: it guards a closed release. Sprint 37 closed Sprint 35 this
# way and Sprint 37 closed Sprint 37 the same way; this does the same for
# Sprint 37 — assert the publisher policy and its refusal suite are
# byte-identical to the reviewed merge, then run that suite in an isolated
# clone checked out at that merge, where the surface it pins is still current.

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$reviewedSprint37Merge = '2af6f60858b23fef58e981da78725c5e6d8cde73'
$protectedSprint37Files = @(
    'scripts/Sprint37QualifiedRelease.Common.ps1',
    'scripts/Test-Sprint37Release.ps1'
)

$currentCommit = (& git -C $repositoryRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Current source is unavailable.' }
& git -C $repositoryRoot merge-base --is-ancestor $reviewedSprint37Merge $currentCommit
if ($LASTEXITCODE -ne 0) { throw 'Current source does not descend from the reviewed Sprint 37 merge.' }

$changedProtectedFiles = @(& git -C $repositoryRoot diff --name-only --no-renames `
    $reviewedSprint37Merge $currentCommit -- @protectedSprint37Files)
if ($LASTEXITCODE -ne 0 -or $changedProtectedFiles.Count -ne 0) {
    throw 'Closed Sprint 37 publisher policy or its refusal suite changed.'
}

$isolatedRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('kmg-closed-sprint37-' + [guid]::NewGuid().ToString('N'))
try {
    & git clone --shared --no-checkout --quiet $repositoryRoot $isolatedRoot
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the isolated Sprint 37 validation clone.' }
    & git -C $isolatedRoot checkout --detach --quiet $reviewedSprint37Merge
    if ($LASTEXITCODE -ne 0) { throw 'Could not select the reviewed Sprint 37 merge.' }

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $isolatedRoot 'scripts/Test-Sprint37Release.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Closed Sprint 37 publisher tests failed.' }
}
finally {
    if (Test-Path -LiteralPath $isolatedRoot) {
        Remove-Item -LiteralPath $isolatedRoot -Recurse -Force
    }
}

Write-Host 'Closed Sprint 37 publisher policy is unchanged and its reviewed refusal suite passed in isolation.'
