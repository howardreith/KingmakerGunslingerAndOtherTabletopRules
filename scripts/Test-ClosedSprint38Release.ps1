[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Sprint 38's release test derives the live summoning surface and pins it at
# that sprint's totals - 132 creatures, 110 Monster entries and 598 Monster
# placements - so registering any later creature fails it. That is the test
# doing its job, not a regression: it guards a closed release. Sprint 38's own
# note in validate-repository.ps1 said this would happen when Sprint 39 opened,
# and Sprints 35, 36 and 37 were each closed the same way.
#
# Two of the numbers it pins do NOT move, which is worth saying because it is
# easy to misread this as the whole surface shifting. Generated roots stay at
# 1,186 and visible choices at 1,215, because Sprint 39 registers one more
# placement and withholds it, so the published total is unchanged while the
# registered total is not. What moves is the creature and placement census.
#
# So: assert the publisher policy and its refusal suite are byte-identical to
# the reviewed merge, then run that suite in an isolated clone checked out at
# that merge, where the surface it pins is still current.

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$reviewedSprint38Merge = 'de83fc135324da3b34a94acc0a5fcfc3c8fcfae5'
$protectedSprint38Files = @(
    'scripts/Sprint38QualifiedRelease.Common.ps1',
    'scripts/Test-Sprint38Release.ps1'
)

$currentCommit = (& git -C $repositoryRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Current source is unavailable.' }
& git -C $repositoryRoot merge-base --is-ancestor $reviewedSprint38Merge $currentCommit
if ($LASTEXITCODE -ne 0) { throw 'Current source does not descend from the reviewed Sprint 38 merge.' }

$changedProtectedFiles = @(& git -C $repositoryRoot diff --name-only --no-renames `
    $reviewedSprint38Merge $currentCommit -- @protectedSprint38Files)
if ($LASTEXITCODE -ne 0 -or $changedProtectedFiles.Count -ne 0) {
    throw 'Closed Sprint 38 publisher policy or its refusal suite changed.'
}

$isolatedRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('kmg-closed-sprint38-' + [guid]::NewGuid().ToString('N'))
try {
    & git clone --shared --no-checkout --quiet $repositoryRoot $isolatedRoot
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the isolated Sprint 38 validation clone.' }
    & git -C $isolatedRoot checkout --detach --quiet $reviewedSprint38Merge
    if ($LASTEXITCODE -ne 0) { throw 'Could not select the reviewed Sprint 38 merge.' }

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $isolatedRoot 'scripts/Test-Sprint38Release.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Closed Sprint 38 publisher tests failed.' }
}
finally {
    if (Test-Path -LiteralPath $isolatedRoot) {
        Remove-Item -LiteralPath $isolatedRoot -Recurse -Force
    }
}

Write-Host 'Closed Sprint 38 publisher policy is unchanged and its reviewed refusal suite passed in isolation.'
