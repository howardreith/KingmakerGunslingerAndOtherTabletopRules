<#
.SYNOPSIS
The decisions Invoke-ExpandedSummoningRuntimeScenario.ps1 makes, as functions.

.DESCRIPTION
These were inline in the wrapper, where the only way to exercise them was to
launch the game. Each one has already been wrong at least once in a way that
reported a green result:

- evidence selection adopted an August directory for a scenario that had never
  run in that batch, and reported it as a pass;
- outcome reconciliation treated a teardown fault as a scenario failure, and a
  scenario failure as a teardown fault, depending on which signal was read
  first;
- restoration was attempted while Kingmaker was still shutting down, and the
  refusal was recorded as a restoration failure.

They are pure functions of their inputs so the test script can drive every
branch without a runtime lease, a deployment, or a launch.
#>

Set-StrictMode -Version Latest

<#
.SYNOPSIS
Fingerprints a directory tree: one order-stable digest over every relative path
and its content hash, so a moved, added, or edited file all change the result.
#>
function Get-KmgTreeFingerprint {
    param([Parameter(Mandatory = $true)][string]$Directory)
    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) {
        return [pscustomobject]@{ Files = 0; Sha256 = '<absent>' }
    }
    $root = (Resolve-Path -LiteralPath $Directory).Path.TrimEnd('\') + '\'
    $entries = Get-ChildItem -LiteralPath $Directory -Recurse -File |
        Sort-Object { $_.FullName.Substring($root.Length) }
    $builder = New-Object Text.StringBuilder
    foreach ($entry in $entries) {
        [void]$builder.Append($entry.FullName.Substring($root.Length).ToLowerInvariant())
        [void]$builder.Append('|')
        [void]$builder.Append((Get-FileHash -LiteralPath $entry.FullName -Algorithm SHA256).Hash)
        [void]$builder.AppendLine()
    }
    $bytes = [Text.Encoding]::UTF8.GetBytes($builder.ToString())
    $stream = New-Object IO.MemoryStream (, $bytes)
    try { $hash = (Get-FileHash -InputStream $stream -Algorithm SHA256).Hash }
    finally { $stream.Dispose() }
    return [pscustomobject]@{ Files = $entries.Count; Sha256 = $hash }
}

<#
.SYNOPSIS
Picks the evidence directory a scenario produced in THIS run, or nothing.

.DESCRIPTION
Evidence directories are named "<UTC stamp>-<scenario>". Taking the newest
directory whose name merely matches the scenario once adopted a result from a
previous month for a scenario that had never run, and reported it as a pass, so
the stamp must be at or after the moment this scenario started. The tolerance
covers the harness stamping its directory a moment before the wrapper records
its own start time; it is deliberately small enough that a previous run of the
same scenario in the same batch cannot be mistaken for this one.
#>
function Select-KmgScenarioEvidence {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()]
        [string[]]$DirectoryNames,
        [Parameter(Mandatory = $true)][string]$Scenario,
        [Parameter(Mandatory = $true)][DateTime]$StartedUtc,
        [ValidateRange(0, 600)][int]$ToleranceSeconds = 90
    )
    $floor = $StartedUtc.ToUniversalTime().AddSeconds(-$ToleranceSeconds)
    $candidates = @()
    foreach ($name in $DirectoryNames) {
        # The scenario must be the whole suffix. A substring match would let
        # "disposable-expanded-summoning-player-path" answer for
        # "disposable-expanded-summoning".
        if ($name -cnotlike ('*-' + $Scenario)) { continue }
        if ($name -notmatch '^(?<t>[0-9]{8}T[0-9]{6})') { continue }
        $stamp = [DateTime]::ParseExact($Matches['t'], 'yyyyMMddTHHmmss',
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::AssumeUniversal -bor
            [Globalization.DateTimeStyles]::AdjustToUniversal)
        if ($stamp -lt $floor) { continue }
        $candidates += $name
    }
    if ($candidates.Count -eq 0) { return $null }
    return (@($candidates | Sort-Object -Descending))[0]
}

<#
.SYNOPSIS
Reads a scenario's own recorded status, or $null when it cannot be trusted.

.DESCRIPTION
Never throws. A result file that is missing, unreadable, malformed, or for a
different scenario yields $null, which the outcome rules treat as "no usable
evidence" rather than as a pass.
#>
function Read-KmgScenarioStatus {
    param(
        [Parameter(Mandatory = $true)][string]$ResultPath,
        [Parameter(Mandatory = $true)][string]$Scenario
    )
    if (-not (Test-Path -LiteralPath $ResultPath -PathType Leaf)) { return $null }
    try {
        $result = Get-Content -LiteralPath $ResultPath -Raw | ConvertFrom-Json
    }
    catch { return $null }
    if ($null -eq $result) { return $null }
    if (-not ($result.PSObject.Properties.Name -contains 'status')) { return $null }
    if (-not ($result.PSObject.Properties.Name -contains 'scenario')) { return $null }
    if ([string]$result.scenario -cne $Scenario) { return $null }
    $status = [string]$result.status
    if ([string]::IsNullOrWhiteSpace($status)) { return $null }
    return $status
}

<#
.SYNOPSIS
Reconciles how the launcher exited with the scenario's own recorded status.

.DESCRIPTION
The two are separate facts and neither alone is the answer. The harness can
write a complete result and still throw during teardown, and it can exit zero
without the scenario having recorded anything.

  launcher   evidence status   outcome
  Clean      PASS              PASS
  Clean      FAIL              FAIL                      the scenario decides
  Clean      (none/unreadable) NO-EVIDENCE               never a pass
  Unclean    PASS              PASS-WITH-TEARDOWN-FAULT  the scenario finished
  Unclean    FAIL              FAIL
  Unclean    (none/unreadable) ERROR

The Clean/FAIL row is a correction, not an extraction. The wrapper's inline
logic only ever moved an outcome away from PASS when evidence was absent, so a
scenario that recorded FAIL while the launcher happened to exit zero was counted
as a pass and did not increment the batch's failure count. That relied on the
launcher's exit code agreeing with the scenario's own verdict, which is exactly
the conflation the correction order calls out: an individual scenario's result
and the run's cleanliness are separate facts and both have to be preserved.

"Unclean" covers both a non-zero exit and a thrown exception; they mean the same
thing here, which is that the launcher did not finish tidily.
#>
function Resolve-KmgScenarioOutcome {
    param(
        [Parameter(Mandatory = $true)][ValidateSet('Clean', 'Unclean')]
        [string]$LauncherOutcome,
        [AllowNull()][AllowEmptyString()][string]$EvidenceStatus
    )
    $hasStatus = -not [string]::IsNullOrWhiteSpace($EvidenceStatus)
    if (-not $hasStatus) {
        if ($LauncherOutcome -ceq 'Clean') { return 'NO-EVIDENCE' }
        return 'ERROR'
    }
    if ($EvidenceStatus -ceq 'PASS') {
        if ($LauncherOutcome -ceq 'Unclean') { return 'PASS-WITH-TEARDOWN-FAULT' }
        return 'PASS'
    }
    return 'FAIL'
}

<#
.SYNOPSIS
Whether an outcome counts against the batch.

.DESCRIPTION
A scenario that recorded PASS and then faulted during teardown is not a scenario
failure; the fault is reported separately.
#>
function Test-KmgScenarioOutcomeFailed {
    param([Parameter(Mandatory = $true)][string]$Outcome)
    return -not ($Outcome -ceq 'PASS' -or $Outcome -ceq 'PASS-WITH-TEARDOWN-FAULT')
}

<#
.SYNOPSIS
Whether restoration is needed, and whether it succeeded.
#>
function Resolve-KmgRestorationNeeded {
    param(
        [Parameter(Mandatory = $true)][string]$BeforeSha256,
        [Parameter(Mandatory = $true)][string]$AfterScenarioSha256
    )
    return -not ($BeforeSha256 -ceq $AfterScenarioSha256)
}

function Resolve-KmgRestorationResult {
    param(
        [Parameter(Mandatory = $true)][string]$BeforeSha256,
        [Parameter(Mandatory = $true)][string]$RestoredSha256
    )
    if ($BeforeSha256 -ceq $RestoredSha256) { return 'verified' }
    return 'MISMATCH'
}

<#
.SYNOPSIS
Whether a compatibility lock was created by this batch.

.DESCRIPTION
A batch may release only a lock it owns. The first version of this compared the
snapshot directory's timestamp against the lock owner's as a substring, on the
assumption that the harness stamps its lock with the same run stamp. It does
not: the snapshot is taken when the batch starts and the lock when the harness
takes the runtime, and one second between them is enough to make the substring
miss. That happened, the batch declined to release its own lock, and restoration
failed with the live tree still carrying the test build.

The reliable rule is the window. A batch holds the runtime exclusively for its
whole duration, so any compatibility lock whose run id is stamped between the
batch starting and now was created by this batch. A lock from outside that
window belongs to someone else and is left alone, because stealing another
session's lock is worse than leaving a stale one.
#>
function Test-KmgCompatibilityLockOwned {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$LockOwner,
        [Parameter(Mandatory = $true)][DateTime]$BatchStartedUtc,
        [DateTime]$NowUtc = [DateTime]::UtcNow
    )
    if ([string]::IsNullOrWhiteSpace($LockOwner)) { return $false }
    if ($LockOwner -notmatch '(?<stamp>[0-9]{8}T[0-9]{6})') { return $false }
    $stamp = [DateTime]::ParseExact($Matches['stamp'], 'yyyyMMddTHHmmss',
        [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::AssumeUniversal -bor
        [Globalization.DateTimeStyles]::AdjustToUniversal)
    # A couple of seconds of slack at the start: the harness can stamp its lock
    # marginally before the wrapper records its own start.
    return $stamp -ge $BatchStartedUtc.ToUniversalTime().AddSeconds(-5) -and
        $stamp -le $NowUtc.ToUniversalTime().AddSeconds(5)
}
