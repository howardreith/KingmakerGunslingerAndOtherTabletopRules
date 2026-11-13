[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$reviewedSprint35Merge = '1ddc27cf709d2ff4cd44ea01013dec14ece82d57'
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
    # The 2026-11-13 history rewrite (docs/history/HISTORY-REWRITE-2026-11-13.md)
    # renamed the prior Sprint 35 candidate that the frozen suite diffs against.
    # Resolve its pre-rewrite id to the rewritten commit (identical tree) in this
    # throwaway clone only.
    & git -C $isolatedRoot update-ref refs/replace/f785c3035662c7baeae5e0c5f82e85973e03e489 `
        1c4067ec683b450d12fdc21596120662b6e8f481
    if ($LASTEXITCODE -ne 0) { throw 'Could not map the rewritten prior Sprint 35 candidate.' }

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
