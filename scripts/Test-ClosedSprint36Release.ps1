[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Sprint 36's release test derives the live summoning surface and pins it at
# that sprint's totals, so registering any later creature fails it. That is the
# test doing its job, not a regression: it guards a closed release. Sprint 36
# closed Sprint 35 the same way, and this does the same for Sprint 36 — assert
# the publisher policy and its refusal suite are byte-identical to the reviewed
# merge, then run that suite in an isolated clone checked out at that merge,
# where the surface it pins is still the current one.

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$reviewedSprint36Merge = 'b1cf3a3dd97a4df7d4838961bdea97a254a576d8'
$protectedSprint36Files = @(
    'scripts/Sprint36QualifiedRelease.Common.ps1',
    'scripts/Test-Sprint36Release.ps1'
)

$currentCommit = (& git -C $repositoryRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Current source is unavailable.' }
& git -C $repositoryRoot merge-base --is-ancestor $reviewedSprint36Merge $currentCommit
if ($LASTEXITCODE -ne 0) { throw 'Current source does not descend from the reviewed Sprint 36 merge.' }

$changedProtectedFiles = @(& git -C $repositoryRoot diff --name-only --no-renames `
    $reviewedSprint36Merge $currentCommit -- @protectedSprint36Files)
if ($LASTEXITCODE -ne 0 -or $changedProtectedFiles.Count -ne 0) {
    throw 'Closed Sprint 36 publisher policy or its refusal suite changed.'
}

$isolatedRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('kmg-closed-sprint36-' + [guid]::NewGuid().ToString('N'))
try {
    & git clone --shared --no-checkout --quiet $repositoryRoot $isolatedRoot
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the isolated Sprint 36 validation clone.' }
    & git -C $isolatedRoot checkout --detach --quiet $reviewedSprint36Merge
    if ($LASTEXITCODE -ne 0) { throw 'Could not select the reviewed Sprint 36 merge.' }

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $isolatedRoot 'scripts/Test-Sprint36Release.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Closed Sprint 36 publisher tests failed.' }
}
finally {
    if (Test-Path -LiteralPath $isolatedRoot) {
        Remove-Item -LiteralPath $isolatedRoot -Recurse -Force
    }
}

Write-Host 'Closed Sprint 36 publisher policy is unchanged and its reviewed refusal suite passed in isolation.'
