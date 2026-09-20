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
    $expected = if ($Lease.PSObject.Properties['StateSha256']) { $Lease.StateSha256 } else { $null }
    Write-KmgRuntimeAtomicBytes $Lease.StatePath $bytes $expected
    $Lease | Add-Member NoteProperty StateSha256 (Get-KmgCompatibilitySha256 $Lease.StatePath) -Force
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
        if ($Purpose -cmatch '^runtime ([a-z0-9-]+)$') {
            # Only a standalone generic launcher owns this journal. Inherited
            # settings/profile transactions keep their own restoration contract.
            $lease.State.runtime = [ordered]@{ scenario=$Matches[1]; request=$null; game=$null; outcome=$null }
            $lease.State.completionCommand = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Complete-KingmakerRuntimeLease.ps1 -RunId $runId"
        }
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

function Set-KmgGenericRuntimeRequest($Scope, [string]$RequestPath, [string]$DeploymentPath) {
    if (-not $Scope.Acquired) { return }
    $lease = $Scope.Lease; Assert-KmgRuntimeLease $lease
    $request = Read-KmgCompatibilityJson $RequestPath
    if ($lease.State.purpose -cne ('runtime ' + $request.scenario) -or $null -ne $lease.State.runtime.request) {
        throw 'Generic runtime request does not belong to this new lease.'
    }
    $lease.State.runtime.request = [ordered]@{ path=[IO.Path]::GetFullPath($RequestPath)
        sha256=Get-KmgCompatibilitySha256 $RequestPath; runId=$request.runId
        exitAfterCompletion=$request.exitAfterCompletion; deploymentPath=[IO.Path]::GetFullPath($DeploymentPath)
        deploymentSha256=Get-KmgCompatibilitySha256 $DeploymentPath }
    Write-KmgRuntimeLeaseState $lease
}

function Set-KmgGenericRuntimeProcess($Scope, $Process) {
    if (-not $Scope.Acquired) { return }
    $lease=$Scope.Lease; Assert-KmgRuntimeLease $lease
    if ($null -ne $lease.State.runtime.game -or $null -eq $lease.State.runtime.request -or $Process.ProcessName -cne 'Kingmaker') {
        throw 'Generic runtime process binding is missing or already set.'
    }
    $lease.State.runtime.game = [ordered]@{ processId=$Process.Id; startedUtc=$Process.StartTime.ToUniversalTime().ToString('o') }
    Write-KmgRuntimeLeaseState $lease
}

function Set-KmgGenericRuntimeOutcome($Scope, [string]$ResultPath) {
    if (-not $Scope.Acquired) { return }
    $lease=$Scope.Lease; Assert-KmgRuntimeLease $lease
    $result=Read-KmgCompatibilityJson $ResultPath
    if ($result.runId -cne $lease.State.runtime.request.runId -or $result.scenario -cne $lease.State.runtime.scenario) {
        throw 'Generic runtime outcome belongs to a different request.'
    }
    $lease.State.runtime.outcome = [ordered]@{ path=[IO.Path]::GetFullPath($ResultPath)
        sha256=Get-KmgCompatibilitySha256 $ResultPath; status=$result.status }
    Write-KmgRuntimeLeaseState $lease
}

function Assert-KmgGenericRuntimeRecord($State) {
    if ($State.schemaVersion -ne 1 -or $null -ne $State.settings -or
        -not $State.PSObject.Properties['runtime'] -or $null -eq $State.runtime -or
        $State.purpose -cne ('runtime ' + $State.runtime.scenario) -or
        $State.runtime.scenario -cnotmatch '^[a-z0-9-]+$') {
        throw 'Only a generic runtime lease without settings/profile restoration obligations may be completed here.'
    }
    if ($null -ne $State.runtime.request) {
        $request=$State.runtime.request
        foreach ($pin in @(@($request.path,$request.sha256),@($request.deploymentPath,$request.deploymentSha256))) {
            if ($pin[1] -cnotmatch '^[0-9a-f]{64}$' -or (Get-KmgCompatibilitySha256 $pin[0]) -cne $pin[1]) {
                throw 'Generic runtime request/deployment binding changed; preserve foreign files and investigate.'
            }
        }
        $body=Read-KmgCompatibilityJson $request.path
        if ($body.runId -cne $request.runId -or $body.scenario -cne $State.runtime.scenario -or
            $body.exitAfterCompletion -isnot [bool] -or $body.exitAfterCompletion -cne $request.exitAfterCompletion) {
            throw 'Generic runtime request binding is inconsistent.'
        }
    }
    if ($null -ne $State.runtime.outcome -and
        (Get-KmgCompatibilitySha256 $State.runtime.outcome.path) -cne $State.runtime.outcome.sha256) {
        throw 'Generic runtime outcome changed; preserve foreign evidence and investigate.'
    }
}

function Test-KmgRuntimeProcessIdentity($Process, [int]$Id, [string]$StartedUtc) {
    return $null -ne $Process -and $Process.Id -eq $Id -and
        $Process.StartTime.ToUniversalTime().ToString('o') -ceq $StartedUtc
}

function Exit-KmgRuntimeLease($Scope, [switch]$PermitRuntimeHandoff) {
    if ($null -eq $Scope -or -not $Scope.Acquired) { return }
    $lease = $Scope.Lease
    Assert-KmgRuntimeLease $lease
    try {
        $games = @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue)
        if ($PermitRuntimeHandoff) {
            # Ordered dictionaries in live owners and PSCustomObjects in recovery
            # share this validation shape. No handoff may clear recovery debt.
            Assert-KmgGenericRuntimeRecord ($lease.State | ConvertTo-Json -Depth 16 | ConvertFrom-Json)
            $runtime=$lease.State.runtime
            if ($lease.State.recoveryRequired -or $null -eq $runtime.request -or $runtime.request.exitAfterCompletion -or
                $null -eq $runtime.game -or $null -eq $runtime.outcome -or $runtime.outcome.status -cne 'PASS') {
                throw 'A handoff requires a successful, explicitly leave-open generic run without restoration obligations.'
            }
            if ($games.Count -eq 1 -and (Test-KmgRuntimeProcessIdentity $games[0] $runtime.game.processId $runtime.game.startedUtc)) {
                $lease.State.status='CompletionPending'
                $lease.State.reason='Native PASS; the identified game remains open as requested. Close it normally, then complete this exact lease.'
                Write-KmgRuntimeLeaseState $lease
                Write-Host $lease.State.reason
                Write-Host $lease.State.completionCommand
                return
            }
        }
        if ($games.Count -gt 0) {
            $lease.State.recoveryRequired = $true
            $lease.State.reason = 'Kingmaker remains running; do not restore, kill it or steal this lease.'
        }
        if ($lease.State.recoveryRequired) {
            $lease.State.status = 'RecoveryRequired'; Write-KmgRuntimeLeaseState $lease
            $hint = if ($lease.State -is [Collections.IDictionary] -and $lease.State.Contains('completionCommand')) { ' ' + $lease.State.completionCommand } else { '' }
            throw "Runtime ownership retained for recovery: $($lease.StatePath). $($lease.State.reason)$hint"
        }
        $lease.State.status = 'Completed'; Write-KmgRuntimeLeaseState $lease
        $lease.Stream.Dispose()
        Remove-KmgCompatibilityOwnedLock $lease.LockPath $lease.RunId
    } finally { $lease.Stream.Dispose() }
}

function Complete-KmgGenericRuntimeLease {
    [CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
    param([Parameter(Mandatory=$true)][ValidatePattern('^runtime-[A-Za-z0-9-]+$')][string]$RunId,
        [string]$StateRoot='C:/Dev/KingmakerGunslingerLab/compatibility-state')
    $statePath=Join-Path (Join-Path $StateRoot $RunId) 'runtime-lease.json'
    $lockPath=Join-Path $StateRoot 'compatibility.lock'
    $stateSha=Get-KmgCompatibilitySha256 $statePath
    $state=Read-KmgCompatibilityJson $statePath
    if ($state.runId -cne $RunId -or $state.status -cnotin @('Active','RecoveryRequired','CompletionPending','Completed') -or
        (Test-Path -LiteralPath (Join-Path (Split-Path $statePath) 'transaction.json'))) {
        throw 'Generic completion requires its exact recorded run, not a profile transaction.'
    }
    Assert-KmgGenericRuntimeRecord $state
    $assertOwnersExited = {
        $owner=Get-Process -Id $state.processId -ErrorAction SilentlyContinue
        if (Test-KmgRuntimeProcessIdentity $owner $state.processId $state.processStartedUtc) { throw 'The original runtime owner is still alive.' }
        if ($null -ne $state.runtime.game) {
            $game=Get-Process -Id $state.runtime.game.processId -ErrorAction SilentlyContinue
            if (Test-KmgRuntimeProcessIdentity $game $state.runtime.game.processId $state.runtime.game.startedUtc) { throw 'The recorded Kingmaker process is still alive.' }
        }
        if (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count) { throw 'Kingmaker is running; completion never kills or releases a running session.' }
    }
    & $assertOwnersExited
    if ((Get-Content -LiteralPath $lockPath -Raw).Trim() -cne $RunId) { throw 'Generic runtime lock belongs to a foreign run.' }
    if (-not $PSCmdlet.ShouldProcess($statePath,'complete exact generic runtime ownership after process exit; no deployment/settings restoration')) { return }
    # ReadWrite conflicts with active read leases AND another recovery. Keep the
    # handle through the guarded journal write; never delete a lock by age/PID.
    $stream=[IO.File]::Open($lockPath,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite,[IO.FileShare]::Read)
    try {
        $lease=[pscustomobject]@{RunId=$RunId;LockPath=$lockPath;Stream=$stream;ProcessId=$PID
            StatePath=$statePath;StateSha256=$stateSha;Compatibility=$false;State=$state}
        Assert-KmgRuntimeLease $lease
        if ((Get-KmgCompatibilitySha256 $statePath) -cne $stateSha) { throw 'Generic runtime journal changed before completion ownership.' }
        Assert-KmgGenericRuntimeRecord $state
        & $assertOwnersExited
        $state.status='Completed'; $state.recoveryRequired=$false
        $state.reason='Guarded generic completion after original owner and game exit. No settings, profile, deployment or native outcome was changed.'
        $state | Add-Member NoteProperty completedBy ([ordered]@{processId=$PID;startedUtc=(Microsoft.PowerShell.Management\Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o');atUtc=[DateTime]::UtcNow.ToString('o')}) -Force
        Write-KmgRuntimeLeaseState $lease
        & $assertOwnersExited
        Assert-KmgRuntimeLease $lease
        $stream.Dispose()
        Remove-KmgCompatibilityOwnedLock $lockPath $RunId
        Write-Host "Completed generic runtime lease $RunId. Native result remains unchanged."
    } finally { $stream.Dispose() }
}
