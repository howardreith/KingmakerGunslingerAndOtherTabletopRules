[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$FrozenWorktree)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

$real = Get-KmgGitState -RepositoryRoot $FrozenWorktree
if ($real.Commit -cnotmatch '^[0-9a-f]{40}$' -or $real.Branch -cne '' -or $real.Status.Count -ne 0) {
    throw 'The actual detached frozen checkout did not retain its exact clean commit identity.'
}
$checks = 1
$script:fixtureBranch = 'codex/fixture'
$script:fixtureStatus = @()
$script:fixtureExit = 0
function git {
    $global:LASTEXITCODE = $script:fixtureExit
    if ($args -contains '--show-current') { return $script:fixtureBranch }
    if ($args -contains '--porcelain') { return $script:fixtureStatus }
    return ('a' * 40)
}
try {
    $named = Get-KmgGitState -RepositoryRoot $FrozenWorktree
    if ($named.Branch -cne 'codex/fixture') { throw 'Named branch identity changed.' }; $checks++
    $script:fixtureBranch = $null
    $detached = Get-KmgGitState -RepositoryRoot $FrozenWorktree
    if ($detached.Branch -cne '' -or $detached.Commit -cne ('a' * 40)) { throw 'Empty output lost the frozen commit.' }; $checks++
    $script:fixtureStatus = @('?? unexpected-source.cs')
    $dirty = Get-KmgGitState -RepositoryRoot $FrozenWorktree
    if ($dirty.Status.Count -ne 1 -or $dirty.Status[0] -cne '?? unexpected-source.cs') { throw 'Detached status hid dirty source.' }; $checks++
    $script:fixtureExit = 1
    $refused = $false
    try { $null = Get-KmgGitState -RepositoryRoot $FrozenWorktree } catch { $refused = $true }
    if (-not $refused) { throw 'A failed Git command was accepted.' }; $checks++
} finally { Remove-Item Function:\git }
Write-Host "Frozen source identity checks passed: $checks"
