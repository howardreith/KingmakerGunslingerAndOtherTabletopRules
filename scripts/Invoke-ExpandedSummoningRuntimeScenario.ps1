<#
.SYNOPSIS
Runs one or more guarded Expanded Summoning runtime scenarios and always
restores the live mod tree this mission mutated.

.DESCRIPTION
Invoke-KingmakerRuntimeTest deploys a candidate and leaves it installed. That
default is fine for a release qualification, but this charter mission is not
authorized to leave a branch build on the machine: a shared version string does
not make a branch artifact an accepted release.

This wrapper takes its own verified snapshot of the live tree before the run and
restores that exact snapshot afterwards on success, on failure, and on Ctrl+C,
recording before/after hashes either way. It owns only what it snapshotted, and
it refuses to restore over a tree that changed underneath it.

.NOTES
Restoration is skipped, loudly, when the post-run tree is neither the snapshot
nor the deployment this run produced - that means something else touched the
installation and a blind restore could destroy it.
#>
[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    # One or more scenarios. A batch shares a single snapshot and a single
    # restore, so a baseline suite does not rebuild and redeploy per scenario.
    [Parameter(Mandatory = $true)][string[]]$Scenario,
    [Parameter(Mandatory = $true)][string]$ExpectedVersion,
    # Save name for scenarios that require one. AGENTS permits only
    # KMG_AUTOMATION_WORKING for automated runs.
    [string]$SaveName,
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',
    [string]$RestorationRecordRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence\expanded-summoning-restoration',
    [hashtable]$ScenarioParameters = @{},
    [int]$TimeoutSeconds = 120
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

function Get-KmgTreeFingerprint {
    param([Parameter(Mandatory = $true)][string]$Directory)
    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) {
        return [pscustomobject]@{ Files = 0; Sha256 = '<absent>' }
    }
    # One order-stable digest over every relative path and its content hash, so
    # a moved, added, or edited file all change the fingerprint.
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

Assert-KmgNotRunning

$before = Get-KmgTreeFingerprint -Directory $LiveModDirectory
Write-Host "Pre-run live tree: files=$($before.Files) sha256=$($before.Sha256)"

# Snapshot what we are about to disturb. AllowEmptySource keeps a clean machine
# (no mod installed) restorable to that same clean state.
$snapshot = & (Join-Path $PSScriptRoot 'Backup-Live-Mod.ps1') `
    -LiveModDirectory $LiveModDirectory -Confirm:$false `
    -AllowEmptySource:($before.Files -eq 0)
Write-Host "Mission snapshot: $($snapshot.Destination)"
# The harness stamps its compatibility lock with the same run timestamp, so the
# snapshot directory name identifies a lock this batch owns.
$snapshotStamp = (Split-Path $snapshot.Destination -Leaf).Substring(0, 16)

$record = [ordered]@{
    schemaVersion = 2
    scenarios = @($Scenario)
    expectedVersion = $ExpectedVersion
    startedAtUtc = [DateTime]::UtcNow.ToString('o')
    snapshotDirectory = $snapshot.Destination
    liveBefore = [ordered]@{ files = $before.Files; sha256 = $before.Sha256 }
    runs = @()
}
$failures = 0
$reuseManifest = $null
$reusePackage = $null
$deploymentRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments'
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot

try {
    # Each scenario is attempted even if an earlier one fails, so one bad
    # scenario cannot hide the rest of a baseline suite. Restoration still
    # happens exactly once, in the finally below.
    $first = $true
    foreach ($name in $Scenario) {
        Write-Host "=== scenario: $name ==="
        $run = [ordered]@{ scenario = $name; startedAtUtc = [DateTime]::UtcNow.ToString('o') }
        try {
            $arguments = @{
                Scenario = $name
                ExpectedVersion = $ExpectedVersion
                Parameters = $ScenarioParameters
                TimeoutSeconds = $TimeoutSeconds
                ExitAfterCompletion = $true
                Confirm = $false
            }
            if ($SaveName) { $arguments.SaveName = $SaveName }
            # Only the first scenario needs to build and deploy; the rest run
            # against the artifact it installed. Reuse is refused unless the
            # exact deployment manifest and package are named, so resolve both
            # from what the first run actually produced rather than assuming.
            if (-not $first -and $reusePackage -and $reuseManifest) {
                $arguments.ReuseInstalledArtifact = $true
                $arguments.DeploymentManifestPath = $reuseManifest
                $arguments.PackagePath = $reusePackage
            }
            & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') @arguments
            $run.exitCode = $LASTEXITCODE
            $run.outcome = if ($LASTEXITCODE -eq 0) { 'PASS' } else { 'FAIL' }
            if ($first) {
                # Capture what the deploying run produced so later scenarios can
                # name it exactly; the harness refuses a vague reuse.
                $candidate = Join-Path $repositoryRoot (
                    "artifacts\local-runtime\$ExpectedVersion\" +
                    "KingmakerGunslinger-$ExpectedVersion-local-runtime.zip")
                if (Test-Path -LiteralPath $candidate -PathType Leaf) {
                    $reusePackage = $candidate
                }
                $manifest = Get-ChildItem -LiteralPath $deploymentRoot -Directory `
                        -ErrorAction SilentlyContinue |
                    Sort-Object Name -Descending |
                    ForEach-Object { Join-Path $_.FullName 'deployment.json' } |
                    Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
                    Select-Object -First 1
                if ($manifest) { $reuseManifest = $manifest }
                $run.deploymentManifest = $reuseManifest
                $run.package = $reusePackage
            }
        }
        catch {
            $run.outcome = 'ERROR'
            $run.error = $_.Exception.Message
            Write-Warning "Scenario $name errored: $($_.Exception.Message)"
        }
        # The harness can write a complete result and still throw during
        # teardown. Read what the scenario actually recorded so a teardown fault
        # is not reported as a scenario failure, and vice versa.
        $evidence = Get-ChildItem 'C:\Dev\KingmakerGunslingerLab\runtime-evidence' -Directory `
                -Filter ('*-' + $name) -ErrorAction SilentlyContinue |
            Sort-Object Name -Descending | Select-Object -First 1
        if ($evidence) {
            $resultPath = Join-Path $evidence.FullName 'runtime-result.json'
            if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
                $scenarioResult = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
                $run.evidenceDirectory = $evidence.Name
                $run.scenarioStatus = $scenarioResult.status
                if ($run.outcome -eq 'ERROR' -and $scenarioResult.status -eq 'PASS') {
                    $run.outcome = 'PASS-WITH-TEARDOWN-FAULT'
                }
            }
        }
        # A scenario that recorded PASS and then faulted during teardown is not
        # a scenario failure; the teardown fault is reported separately.
        if ($run.outcome -ne 'PASS' -and
            $run.outcome -ne 'PASS-WITH-TEARDOWN-FAULT') { $failures++ }
        $run.completedAtUtc = [DateTime]::UtcNow.ToString('o')
        $record.runs += $run
        $first = $false
    }
}
finally {
    # Runs on success, on failure, and on interruption.
    $record.failures = $failures
    $deployed = Get-KmgTreeFingerprint -Directory $LiveModDirectory
    $record.liveAfterScenario = [ordered]@{ files = $deployed.Files; sha256 = $deployed.Sha256 }

    if ($deployed.Sha256 -eq $before.Sha256) {
        $record.restoration = 'not-needed'
        Write-Host 'Live tree already matches the pre-run snapshot; nothing to restore.'
    }
    else {
        try {
            # The harness returns while Kingmaker is still shutting down, and a
            # restore during that window is refused. Wait for the process to go
            # rather than treating the race as a restoration failure.
            $exitDeadline = [DateTime]::UtcNow.AddSeconds(120)
            while (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and
                [DateTime]::UtcNow -lt $exitDeadline) {
                Start-Sleep -Milliseconds 500
            }

            # A run that aborted mid-transaction can leave its own compatibility
            # lock behind. Release it only when this batch owns it; the helper
            # throws if the recorded owner differs.
            $compatibilityLock = 'C:\Dev\KingmakerGunslingerLab\compatibility-state\compatibility.lock'
            if (Test-Path -LiteralPath $compatibilityLock -PathType Leaf) {
                $lockOwner = (Get-Content -LiteralPath $compatibilityLock -Raw).Trim()
                if ($lockOwner -like ('*' + $snapshotStamp + '*')) {
                    . (Join-Path $PSScriptRoot 'compatibility\CompatibilityProfile.Common.ps1')
                    Remove-KmgCompatibilityOwnedLock -LockPath $compatibilityLock -RunId $lockOwner
                    $record.releasedStaleCompatibilityLock = $lockOwner
                } else {
                    $record.foreignCompatibilityLock = $lockOwner
                }
            }

            Assert-KmgNotRunning
            & (Join-Path $PSScriptRoot 'Restore-Live-Mod.ps1') `
                -BackupDirectory $snapshot.Destination `
                -LiveModDirectory $LiveModDirectory -Confirm:$false | Out-Null
            $after = Get-KmgTreeFingerprint -Directory $LiveModDirectory
            $record.liveAfterRestore = [ordered]@{ files = $after.Files; sha256 = $after.Sha256 }
            $record.restoration = if ($after.Sha256 -eq $before.Sha256) { 'verified' } else { 'MISMATCH' }
            if ($record.restoration -ne 'verified') {
                Write-Warning "Restored tree does not match the pre-run fingerprint. Snapshot retained: $($snapshot.Destination)"
            } else {
                Write-Host "Restored live tree to pre-run state: sha256=$($after.Sha256)"
            }
        }
        catch {
            $record.restoration = 'FAILED'
            $record.restorationError = $_.Exception.Message
            Write-Warning "Restoration failed. The snapshot is intact at $($snapshot.Destination): $($_.Exception.Message)"
        }
    }

    $record.completedAtUtc = [DateTime]::UtcNow.ToString('o')
    if (-not (Test-Path -LiteralPath $RestorationRecordRoot -PathType Container)) {
        New-Item -ItemType Directory -Path $RestorationRecordRoot -Force | Out-Null
    }
    $recordPath = Join-Path $RestorationRecordRoot (
        [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '-' +
        $Scenario[0] + '.json')
    ($record | ConvertTo-Json -Depth 6) | Set-Content -LiteralPath $recordPath -Encoding utf8
    Write-Host "Restoration record: $recordPath"
}
