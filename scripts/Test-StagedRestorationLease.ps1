# Executable checks for the ownership boundary the staged settings restoration
# depends on. The wrapper's process assertion does NOT reserve the installation:
# another permitted entry point can take the lease after the lock check and
# before any game starts, so a process check alone would let a second owner in.
# These run against the real lease API on a disposable coordination root.
[CmdletBinding()]
param([string]$RepoRoot)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $RepoRoot) { $RepoRoot = Split-Path $PSScriptRoot }
. (Join-Path $RepoRoot 'scripts/RuntimeCoordination.Common.ps1')

# A disposable coordination root, so nothing touches the real one.
$stateRoot = Join-Path $env:TEMP ('kmg-lease-probe-' + [Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($stateRoot)
$pass = 0; $fail = 0
function Check($name, $condition) {
    if ($condition) { $script:pass++; "PASS  $name" } else { $script:fail++; "FAIL  $name" }
}

$owner = $null; $inherited = $null
try {
    # 1. A first acquisition succeeds and reports itself as the acquirer.
    $owner = Enter-KmgRuntimeLease -Purpose 'probe owner' -StateRoot $stateRoot
    Check 'a free coordination root grants a lease' ($null -ne $owner -and $owner.Acquired)
    Check 'the granted scope exposes its lease' ($null -ne $owner.Lease)

    # 2. THE POINT OF THE PATCH: while that lease is held, an unrelated
    #    acquisition must be REFUSED. A process check alone would have let it
    #    through, because no game is running here.
    $games = @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue)
    Check 'no Kingmaker process is running during this probe' ($games.Count -eq 0)
    $refused = $false
    try { $null = Enter-KmgRuntimeLease -Purpose 'probe foreign' -StateRoot $stateRoot }
    catch { $refused = $true }
    Check 'an occupied coordination root refuses a second independent lease' $refused

    # 3. Inheritance returns the SAME lease and does not claim acquisition.
    $inherited = Enter-KmgRuntimeLease -ParentLease $owner.Lease -Purpose 'probe child' -StateRoot $stateRoot
    Check 'an inherited scope reuses the parent lease' `
        ($null -ne $inherited -and [object]::ReferenceEquals($inherited.Lease, $owner.Lease))
    Check 'an inherited scope does not report itself as the acquirer' (-not $inherited.Acquired)

    # 4. Exiting an inherited scope must leave the parent owning the lease.
    Exit-KmgRuntimeLease $inherited
    $inherited = $null
    $stillHeld = $false
    try { $null = Enter-KmgRuntimeLease -Purpose 'probe after child exit' -StateRoot $stateRoot }
    catch { $stillHeld = $true }
    Check 'exiting an inherited scope leaves the lease with its parent' $stillHeld

    # 5. A null scope is a safe no-op, which the wrapper relies on when
    #    acquisition itself failed.
    $nullOk = $true
    try { Exit-KmgRuntimeLease $null } catch { $nullOk = $false }
    Check 'exiting a null scope is a safe no-op' $nullOk
}
finally {
    if ($null -ne $inherited) { Exit-KmgRuntimeLease $inherited }
    if ($null -ne $owner) { Exit-KmgRuntimeLease $owner }
}

# 6. After the owner releases, the root is free again.
$reacquired = $null
try {
    $reacquired = Enter-KmgRuntimeLease -Purpose 'probe reacquire' -StateRoot $stateRoot
    Check 'releasing the owning scope frees the coordination root' `
        ($null -ne $reacquired -and $reacquired.Acquired)
}
finally { if ($null -ne $reacquired) { Exit-KmgRuntimeLease $reacquired } }

Remove-Item -LiteralPath $stateRoot -Recurse -Force -ErrorAction SilentlyContinue
"Staged restoration lease tests passed: $pass assertions."
if ($fail -gt 0) { throw "Staged restoration lease tests failed $fail check(s)." }
