Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'compatibility/CompatibilityProfile.Common.ps1')

# The existing compatibility lock is shared by profile staging, ordinary
# deployment/restore, runtime launches and the Circle settings transaction.
# A live handle prevents another entry point from removing an active lease.
function Write-KmgRuntimeAtomicBytes([string]$Path, [byte[]]$Bytes, [string]$ExpectedSha256) {
    $directory = Split-Path -Parent $Path
    $temporary = Join-Path $directory ('.' + [IO.Path]::GetFileName($Path) + '.' + [Guid]::NewGuid().ToString('N') + '.tmp')
    try {
        $stream = [IO.File]::Open($temporary, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
        try { $stream.Write($Bytes, 0, $Bytes.Length); $stream.Flush($true) } finally { $stream.Dispose() }
        if (Test-Path -LiteralPath $Path) {
            if ($ExpectedSha256 -and (Get-KmgCompatibilitySha256 $Path) -cne $ExpectedSha256) { throw 'Atomic destination no longer has the expected owned bytes.' }
            [IO.File]::Replace($temporary, $Path, [Management.Automation.Language.NullString]::Value)
        } else {
            if ($ExpectedSha256) { throw 'Owned atomic destination disappeared.' }
            [IO.File]::Move($temporary, $Path)
        }
    } finally { if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Force } }
}

function Write-KmgRuntimeLeaseState($Lease) {
    if ($Lease.Compatibility) { return }
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes(($Lease.State | ConvertTo-Json -Depth 16))
    Write-KmgRuntimeAtomicBytes $Lease.StatePath $bytes
}

function Assert-KmgRuntimeLease($Lease) {
    if ($null -eq $Lease -or $Lease.ProcessId -ne $PID -or $null -eq $Lease.Stream -or -not $Lease.Stream.CanRead -or
        -not (Test-Path -LiteralPath $Lease.LockPath -PathType Leaf) -or
        (Get-Content -LiteralPath $Lease.LockPath -Raw).Trim() -cne $Lease.RunId) {
        throw 'Runtime/deployment lease is missing, foreign, closed or no longer owned.'
    }
}

function Enter-KmgRuntimeLease {
    param($ParentLease, [string]$Purpose,
        [string]$StateRoot = 'C:/Dev/KingmakerGunslingerLab/compatibility-state')
    if ($null -ne $ParentLease) {
        Assert-KmgRuntimeLease $ParentLease
        $expectedLock = Join-Path ([IO.Path]::GetFullPath($StateRoot)) 'compatibility.lock'
        if (-not [IO.Path]::GetFullPath($ParentLease.LockPath).Equals($expectedLock, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Inherited lease belongs to a different coordination root.'
        }
        return [pscustomobject]@{ Lease = $ParentLease; Acquired = $false }
    }
    $runId = 'runtime-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' + [Guid]::NewGuid().ToString('N')
    $lock = Acquire-KmgCompatibilityLock $StateRoot $runId
    $stream = $null
    try {
        $stream = [IO.File]::Open($lock, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
        $directory = Join-Path $StateRoot $runId
        [void][IO.Directory]::CreateDirectory($directory)
        $lease = [pscustomobject]@{ RunId = $runId; LockPath = $lock; Stream = $stream; ProcessId = $PID
            StatePath = Join-Path $directory 'runtime-lease.json'; Compatibility = $false
            State = [ordered]@{ schemaVersion = 1; runId = $runId; purpose = $Purpose; processId = $PID
                processStartedUtc = (Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o')
                status = 'Active'; recoveryRequired = $false; reason = $null; settings = $null } }
        Write-KmgRuntimeLeaseState $lease
        return [pscustomobject]@{ Lease = $lease; Acquired = $true }
    } catch {
        if ($stream) { $stream.Dispose() }
        # A persistent lock is intentionally retained if initialization was
        # interrupted. Never guess that another caller may steal this run.
        throw "Runtime lease initialization failed; inspect $lock. $($_.Exception.Message)"
    }
}

function Open-KmgCompatibilityRuntimeLease([string]$RunId, [string]$StateRoot) {
    $statePath = Join-Path (Join-Path $StateRoot $RunId) 'transaction.json'
    $state = Read-KmgCompatibilityJson $statePath
    $lock = Join-Path $StateRoot 'compatibility.lock'
    if ($state.runId -cne $RunId -or $state.status -cne 'Active' -or (Get-Content -LiteralPath $lock -Raw).Trim() -cne $RunId) {
        throw 'Only the exact active compatibility transaction may lend runtime ownership.'
    }
    $stream = [IO.File]::Open($lock, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
    return [pscustomobject]@{ RunId = $RunId; LockPath = $lock; Stream = $stream; ProcessId = $PID
        StatePath = $statePath; Compatibility = $true; State = $null }
}

function Exit-KmgRuntimeLease($Scope) {
    if ($null -eq $Scope -or -not $Scope.Acquired) { return }
    $lease = $Scope.Lease
    Assert-KmgRuntimeLease $lease
    try {
        if (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0) {
            $lease.State.recoveryRequired = $true
            $lease.State.reason = 'Kingmaker remains running; do not restore, kill it or steal this lease.'
        }
        if ($lease.State.recoveryRequired) {
            $lease.State.status = 'RecoveryRequired'; Write-KmgRuntimeLeaseState $lease
            throw "Runtime ownership retained for recovery: $($lease.StatePath). $($lease.State.reason)"
        }
        $lease.State.status = 'Completed'; Write-KmgRuntimeLeaseState $lease
        $lease.Stream.Dispose()
        Remove-KmgCompatibilityOwnedLock $lease.LockPath $lease.RunId
    } finally { $lease.Stream.Dispose() }
}
