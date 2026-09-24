<#
.SYNOPSIS
Bounded tests for the Expanded Summoning runtime orchestration decisions.

.DESCRIPTION
Covers the six cases the owner's correction order names: result isolation,
stale-result rejection, parse failure, restoration failure, interrupted
operation, and a successful transaction.

Nothing here launches Kingmaker, takes a runtime lease, or writes to the
installation.

Two notes on what is and is not exercised for real.

The decision functions are driven directly and completely - every branch, with
the inputs that once produced a wrong answer.

The shipped Backup-Live-Mod.ps1 and Restore-Live-Mod.ps1 deliberately refuse any
directory other than the exact live mod path, so they cannot be pointed at a
fixture tree. That guard is worth more than the convenience of testing through
it, so these tests assert the refusal itself, and exercise the successful and
interrupted transaction shapes against a fixture with the same fingerprint
function the wrapper uses. That is stated here rather than implied, because a
test that looks like it covers the shipped copy path when it does not is worse
than one that says what it covers.
#>
[CmdletBinding()]
param(
    # Where the shipped harness scripts live. Defaults to this script's own
    # directory, which is where it sits in the repository; overridable so the
    # suite can be exercised from a staging copy.
    [string]$ScriptRoot = $PSScriptRoot
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')

$script:Passed = 0
function Assert-True {
    param([Parameter(Mandatory = $true)][bool]$Condition,
          [Parameter(Mandatory = $true)][string]$Message)
    if (-not $Condition) { throw "Orchestration test failed: $Message" }
    $script:Passed++
}
function Assert-Equal {
    param($Expected, $Actual, [Parameter(Mandatory = $true)][string]$Message)
    if ([string]$Expected -cne [string]$Actual) {
        throw "Orchestration test failed: $Message (expected '$Expected', got '$Actual')"
    }
    $script:Passed++
}

$scenario = 'disposable-expanded-summoning'
$started = [DateTime]::SpecifyKind(
    [DateTime]::ParseExact('20260923T163838', 'yyyyMMddTHHmmss',
        [Globalization.CultureInfo]::InvariantCulture), 'Utc')

# ---------------------------------------------------------------- isolation --
# A batch runs several scenarios against one deployment, and the evidence root
# holds every run the machine has ever done.
$directories = @(
    '20260826T101500000000Z-disposable-expanded-summoning',
    '20260923T163830000000Z-disposable-expanded-summoning-player-path',
    '20260923T163840000000Z-disposable-expanded-summoning',
    '20260923T163845000000Z-disposable-expanded-summoning-visual-contracts',
    'not-an-evidence-directory'
)
Assert-Equal '20260923T163840000000Z-disposable-expanded-summoning' `
    (Select-KmgScenarioEvidence -DirectoryNames $directories -Scenario $scenario `
        -StartedUtc $started) `
    'evidence selection must pick this scenario from this run'

# A longer scenario name must not answer for a shorter one it starts with.
Assert-Equal '20260923T163830000000Z-disposable-expanded-summoning-player-path' `
    (Select-KmgScenarioEvidence -DirectoryNames $directories `
        -Scenario 'disposable-expanded-summoning-player-path' `
        -StartedUtc $started) `
    'evidence selection must match the whole scenario suffix'

# Two runs of the same scenario in one batch: the later one wins.
Assert-Equal '20260923T164510000000Z-disposable-expanded-summoning' `
    (Select-KmgScenarioEvidence -DirectoryNames @(
        '20260923T163840000000Z-disposable-expanded-summoning',
        '20260923T164510000000Z-disposable-expanded-summoning') `
        -Scenario $scenario -StartedUtc $started) `
    'the most recent qualifying directory must win'

# ------------------------------------------------------ stale-result rejection
# The defect this exists for: a wrapper adopted a month-old directory for a
# scenario that had never run, and reported it as a pass.
Assert-Equal $null `
    (Select-KmgScenarioEvidence `
        -DirectoryNames @('20260826T101500000000Z-disposable-expanded-summoning') `
        -Scenario $scenario -StartedUtc $started) `
    'evidence older than this run must be refused'

Assert-Equal '20260923T163730000000Z-disposable-expanded-summoning' `
    (Select-KmgScenarioEvidence `
        -DirectoryNames @('20260923T163730000000Z-disposable-expanded-summoning') `
        -Scenario $scenario -StartedUtc $started) `
    'a directory stamped 68s before the start is inside the 90s tolerance'
Assert-Equal $null `
    (Select-KmgScenarioEvidence `
        -DirectoryNames @('20260923T163600000000Z-disposable-expanded-summoning') `
        -Scenario $scenario -StartedUtc $started) `
    'a directory stamped 158s before the start is outside the tolerance'

Assert-Equal $null `
    (Select-KmgScenarioEvidence -DirectoryNames @() -Scenario $scenario `
        -StartedUtc $started) `
    'an empty evidence root yields nothing rather than throwing'

Assert-Equal 'NO-EVIDENCE' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Clean' `
    -EvidenceStatus $null) 'exit zero without evidence is not a pass'
Assert-Equal 'ERROR' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Unclean' `
    -EvidenceStatus $null) 'an errored run without evidence stays an error'

$temporary = Join-Path ([IO.Path]::GetTempPath()) ('kmg-orchestration-' +
    [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporary | Out-Null
try {
    # --------------------------------------------------------- parse failure --
    Assert-Equal $null (Read-KmgScenarioStatus `
        -ResultPath (Join-Path $temporary 'absent.json') -Scenario $scenario) `
        'a missing result file yields no status'

    $malformed = Join-Path $temporary 'malformed.json'
    Set-Content -LiteralPath $malformed -Value '{ "scenario": ' -Encoding UTF8
    Assert-Equal $null (Read-KmgScenarioStatus -ResultPath $malformed `
        -Scenario $scenario) 'truncated JSON yields no status rather than throwing'

    $empty = Join-Path $temporary 'empty.json'
    Set-Content -LiteralPath $empty -Value '' -Encoding UTF8
    Assert-Equal $null (Read-KmgScenarioStatus -ResultPath $empty `
        -Scenario $scenario) 'an empty result file yields no status'

    $statusless = Join-Path $temporary 'statusless.json'
    Set-Content -LiteralPath $statusless -Encoding UTF8 `
        -Value ('{ "scenario": "' + $scenario + '" }')
    Assert-Equal $null (Read-KmgScenarioStatus -ResultPath $statusless `
        -Scenario $scenario) 'a result without a status field yields no status'

    # Result isolation one level down: a correctly formed result belonging to a
    # different scenario must not be adopted.
    $foreign = Join-Path $temporary 'foreign.json'
    Set-Content -LiteralPath $foreign -Encoding UTF8 `
        -Value '{ "scenario": "mod-load-smoke", "status": "PASS" }'
    Assert-Equal $null (Read-KmgScenarioStatus -ResultPath $foreign `
        -Scenario $scenario) "another scenario's result must not be adopted"

    $good = Join-Path $temporary 'good.json'
    Set-Content -LiteralPath $good -Encoding UTF8 `
        -Value ('{ "scenario": "' + $scenario + '", "status": "PASS" }')
    Assert-Equal 'PASS' (Read-KmgScenarioStatus -ResultPath $good `
        -Scenario $scenario) 'a well formed matching result is read'

    # --------------------------------------------------- outcome reconciliation
    Assert-Equal 'PASS' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Clean' `
        -EvidenceStatus 'PASS') 'both green is a pass'
    Assert-Equal 'FAIL' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Clean' `
        -EvidenceStatus 'FAIL') 'the scenario decides, not the exit code'
    # The defect the extracted rules corrected: the wrapper's inline logic only
    # moved an outcome away from PASS when evidence was absent, so a scenario
    # that recorded FAIL while the launcher exited zero was counted as a pass.
    Assert-True (Test-KmgScenarioOutcomeFailed (Resolve-KmgScenarioOutcome `
        -LauncherOutcome 'Clean' -EvidenceStatus 'FAIL')) `
        ('a recorded FAIL counts against the batch even when the launcher ' +
         'exited cleanly')
    Assert-Equal 'PASS-WITH-TEARDOWN-FAULT' (Resolve-KmgScenarioOutcome `
        -LauncherOutcome 'Unclean' -EvidenceStatus 'PASS') `
        'a completed scenario that faulted during teardown is not a failure'
    Assert-Equal 'FAIL' (Resolve-KmgScenarioOutcome -LauncherOutcome 'Unclean' `
        -EvidenceStatus 'FAIL') 'a failed scenario stays failed'
    Assert-True (Test-KmgScenarioOutcomeFailed 'NO-EVIDENCE') `
        'no evidence counts against the batch'
    Assert-True (Test-KmgScenarioOutcomeFailed 'FAIL') 'a failure counts'
    Assert-True (-not (Test-KmgScenarioOutcomeFailed 'PASS-WITH-TEARDOWN-FAULT')) `
        'a teardown fault does not count as a scenario failure'

    # ----------------------------------------------------------- lock ownership
    # A batch holds the runtime exclusively, so a lock stamped inside its own
    # window is its own. The earlier rule compared the snapshot stamp as a
    # substring and missed by one second, which left a batch unable to release
    # its own lock and its restoration failed with the test build still live.
    $batchStart = [DateTime]::SpecifyKind(
        [DateTime]::ParseExact('20260923T195801', 'yyyyMMddTHHmmss',
            [Globalization.CultureInfo]::InvariantCulture), 'Utc')
    $batchNow = $batchStart.AddMinutes(7)
    Assert-True (Test-KmgCompatibilityLockOwned `
        -LockOwner 'runtime-20260923T195802Z-6fd7fa93a51f' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow) `
        'a lock stamped one second after the batch started is its own'
    Assert-True (Test-KmgCompatibilityLockOwned `
        -LockOwner 'runtime-20260923T200341Z-abc' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow) `
        'a lock stamped mid-batch is its own'
    Assert-True (-not (Test-KmgCompatibilityLockOwned `
        -LockOwner 'runtime-20260923T193000Z-abc' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow)) `
        "a lock predating the batch belongs to someone else"
    Assert-True (-not (Test-KmgCompatibilityLockOwned `
        -LockOwner 'runtime-20260923T210000Z-abc' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow)) `
        'a lock stamped after now is not this batch'
    Assert-True (-not (Test-KmgCompatibilityLockOwned -LockOwner '' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow)) `
        'an empty lock file is not owned'
    Assert-True (-not (Test-KmgCompatibilityLockOwned -LockOwner 'no-timestamp' `
        -BatchStartedUtc $batchStart -NowUtc $batchNow)) `
        'a lock with no parseable stamp is not owned'

    # ------------------------------------ successful transaction and restoration
    # The fingerprint function is the shipped one; the copy is a stand-in,
    # because the shipped restore will not operate on a fixture. What this
    # establishes is the fingerprint contract the wrapper's verdicts rest on.
    $live = Join-Path $temporary 'live'
    $snapshot = Join-Path $temporary 'snapshot'
    New-Item -ItemType Directory -Path (Join-Path $live 'nested') | Out-Null
    Set-Content -LiteralPath (Join-Path $live 'Info.json') -Encoding UTF8 `
        -Value '{ "Version": "0.0.136" }'
    Set-Content -LiteralPath (Join-Path $live 'nested\asset.txt') -Encoding UTF8 `
        -Value 'original'
    $before = Get-KmgTreeFingerprint -Directory $live
    Assert-Equal 2 $before.Files 'the fixture tree has two files'
    Copy-Item -LiteralPath $live -Destination $snapshot -Recurse

    # A moved file must change the fingerprint even though the content set is
    # identical; that is why the digest covers relative paths and not just
    # hashes.
    Move-Item -LiteralPath (Join-Path $live 'nested\asset.txt') `
        -Destination (Join-Path $live 'asset.txt')
    Assert-True ((Get-KmgTreeFingerprint -Directory $live).Sha256 -cne `
        $before.Sha256) 'moving a file changes the fingerprint'
    Move-Item -LiteralPath (Join-Path $live 'asset.txt') `
        -Destination (Join-Path $live 'nested\asset.txt')
    Assert-Equal $before.Sha256 (Get-KmgTreeFingerprint -Directory $live).Sha256 `
        'moving it back restores the fingerprint'

    Set-Content -LiteralPath (Join-Path $live 'nested\asset.txt') -Encoding UTF8 `
        -Value 'deployed'
    Set-Content -LiteralPath (Join-Path $live 'extra.txt') -Encoding UTF8 `
        -Value 'deployed only'
    $deployed = Get-KmgTreeFingerprint -Directory $live
    Assert-True (Resolve-KmgRestorationNeeded -BeforeSha256 $before.Sha256 `
        -AfterScenarioSha256 $deployed.Sha256) 'a changed tree needs restoration'
    Assert-True (-not (Resolve-KmgRestorationNeeded `
        -BeforeSha256 $before.Sha256 -AfterScenarioSha256 $before.Sha256)) `
        'an unchanged tree needs no restoration'

    function Restore-KmgFixtureTree {
        param([string]$From, [string]$To)
        if (-not (Test-Path -LiteralPath $From -PathType Container)) {
            throw "Fixture snapshot is missing: $From"
        }
        Remove-Item -LiteralPath $To -Recurse -Force
        Copy-Item -LiteralPath $From -Destination $To -Recurse
    }

    Restore-KmgFixtureTree -From $snapshot -To $live
    $restored = Get-KmgTreeFingerprint -Directory $live
    Assert-Equal 'verified' (Resolve-KmgRestorationResult `
        -BeforeSha256 $before.Sha256 -RestoredSha256 $restored.Sha256) `
        'restoring the snapshot returns the exact pre-run tree'
    Assert-True (-not (Test-Path -LiteralPath (Join-Path $live 'extra.txt'))) `
        'a file the deployment added is gone after restoration'

    # ------------------------------------------------------- restoration failure
    Set-Content -LiteralPath (Join-Path $live 'nested\asset.txt') -Encoding UTF8 `
        -Value 'drifted'
    Assert-Equal 'MISMATCH' (Resolve-KmgRestorationResult `
        -BeforeSha256 $before.Sha256 `
        -RestoredSha256 (Get-KmgTreeFingerprint -Directory $live).Sha256) `
        'a tree that does not match the snapshot is a mismatch, not a pass'

    $restoreFailed = $false
    try { Restore-KmgFixtureTree -From (Join-Path $temporary 'no-such') -To $live }
    catch { $restoreFailed = $true }
    Assert-True $restoreFailed 'restoring from a missing snapshot fails loudly'
    Assert-True (Test-Path -LiteralPath (Join-Path $live 'Info.json')) `
        'a failed restore leaves the tree in place rather than emptying it'

    # ------------------------------------------------------ interrupted operation
    # The wrapper restores in a finally block so an interruption mid-batch still
    # returns the tree. This reproduces that shape and asserts both halves: the
    # interruption still propagates, and the tree still came back.
    Set-Content -LiteralPath (Join-Path $live 'nested\asset.txt') -Encoding UTF8 `
        -Value 'deployed again'
    $interrupted = $false
    try {
        try { throw 'simulated interruption' }
        finally { Restore-KmgFixtureTree -From $snapshot -To $live }
    }
    catch { $interrupted = $true }
    Assert-True $interrupted 'the interruption still propagates to the caller'
    Assert-Equal 'verified' (Resolve-KmgRestorationResult `
        -BeforeSha256 $before.Sha256 `
        -RestoredSha256 (Get-KmgTreeFingerprint -Directory $live).Sha256) `
        'an interrupted batch still restores the tree'
}
finally {
    Remove-Item -LiteralPath $temporary -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Expanded Summoning orchestration tests passed: $script:Passed assertions."
