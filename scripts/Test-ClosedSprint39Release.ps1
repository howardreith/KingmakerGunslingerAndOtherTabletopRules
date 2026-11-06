[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Sprint 39 is released. Preserve its exact publisher/refusal contracts and
# execute its frozen roster assertions at the reviewed product merge.
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$reviewedSprint39Merge = '6782721cf08c75c0377f777a265b2f8b01f2cbad'
$protectedSprint39Files = @(
    'scripts/Sprint39QualifiedRelease.Common.ps1',
    'scripts/Test-Sprint39Release.ps1'
)

$currentCommit = (& git -C $repositoryRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Current source is unavailable.' }
& git -C $repositoryRoot merge-base --is-ancestor $reviewedSprint39Merge $currentCommit
if ($LASTEXITCODE -ne 0) { throw 'Current source does not descend from the reviewed Sprint 39 merge.' }

$changedProtectedFiles = @(& git -C $repositoryRoot diff --name-only --no-renames `
    $reviewedSprint39Merge $currentCommit -- @protectedSprint39Files)
if ($LASTEXITCODE -ne 0 -or $changedProtectedFiles.Count -ne 0) {
    throw 'Closed Sprint 39 publisher policy or its refusal suite changed.'
}

$isolatedRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('kmg-closed-sprint39-' + [guid]::NewGuid().ToString('N'))
try {
    & git clone --shared --no-checkout --quiet $repositoryRoot $isolatedRoot
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the isolated Sprint 39 validation clone.' }
    & git -C $isolatedRoot checkout --detach --quiet $reviewedSprint39Merge
    if ($LASTEXITCODE -ne 0) { throw 'Could not select the reviewed Sprint 39 merge.' }

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $isolatedRoot 'scripts/Test-Sprint39Release.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Closed Sprint 39 publisher tests failed.' }
}
finally {
    if (Test-Path -LiteralPath $isolatedRoot) {
        $resolved = [IO.Path]::GetFullPath($isolatedRoot)
        $temporary = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
        if (-not $resolved.StartsWith($temporary, [StringComparison]::OrdinalIgnoreCase) -or
            [IO.Path]::GetFileName($resolved) -notlike 'kmg-closed-sprint39-*') {
            throw 'Refusing cleanup outside the exact disposable validation clone.'
        }
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
}

Write-Host 'Closed Sprint 39 publisher policy is unchanged and its reviewed refusal suite passed in isolation.'
