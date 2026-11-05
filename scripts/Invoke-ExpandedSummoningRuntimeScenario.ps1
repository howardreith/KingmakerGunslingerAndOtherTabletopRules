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
    [string]$PackagePath,
    # Names the exact development-candidate purpose when PackagePath is not a
    # release candidate. Either way, an explicit PackagePath is deployed once
    # and then every scenario uses the exact returned deployment manifest
    # through the guarded reuse path; the purpose only decides whether the
    # candidate is a development one.
    [string]$DevelopmentPurpose,
    # Save name for scenarios that require one. AGENTS permits only
    # KMG_AUTOMATION_WORKING for automated runs.
    [string]$SaveName,
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',
    [string]$RestorationRecordRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence\expanded-summoning-restoration',
    [hashtable]$ScenarioParameters = @{},
    # Different closed guarded requests may share one candidate/snapshot.
    # The ordinary request validator still validates every returned parameter.
    [hashtable]$ScenarioParameterMap = @{},
    [switch]$AllowDirtyGit,
    [switch]$ExpandedSummoningDisabled,
    # One no-save original-configuration recovery; preserves only the native
    # Favored Class dump proven complete by the actual producer observer.
    [switch]$Sprint40DiagnosticRecovery,
    [ValidateRange(5, 1800)]
    [int]$TimeoutSeconds = 120,
    [ValidateRange(5, 600)]
    [int]$ObserverStartupTimeoutSeconds = 180,
    [ValidateRange(5, 1800)]
    [int]$CompletionTimeoutSeconds = 180
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

# The orchestration decisions - which evidence belongs to this run, how the
# launcher's exit reconciles with the scenario's own recorded status, whether
# restoration is needed and whether it worked, and whether a compatibility lock
# belongs to this batch - live in one dot-sourced file so the bounded test
# suite exercises the shipped rules rather than a second copy of them.
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')

if ($PackagePath -and $SaveName -ceq 'KMG_AUTOMATION_WORKING' -and $ScenarioParameters.ContainsKey('persistenceScope') -and $ScenarioParameters.persistenceScope -cin @('ankylosaurus','armored-dinosaurs','theropods','giant-bodies','magical-beasts','griffon')) {
    foreach($name in $Scenario) {
        & (Join-Path $PSScriptRoot 'Invoke-Sprint22InstalledStack.ps1') -Scenario $name -SaveName $SaveName `
            -PackagePath $PackagePath -ExpectedVersion $ExpectedVersion -ExpandedSummoningDisabled:$ExpandedSummoningDisabled `
            -PersistenceScope $ScenarioParameters.persistenceScope -TimeoutSeconds $TimeoutSeconds -Confirm:$false
    }
    return
}

if ($ExpandedSummoningDisabled -and
    ($Scenario.Count -ne 1 -or
        $Scenario[0] -cne 'working-save-expanded-summoning-verify-cleanup' -or
        $SaveName -cne 'KMG_AUTOMATION_WORKING')) {
    throw 'The temporary module-off setting is allowed only for the exact working-save Expanded Summoning verify/cleanup stage.'
}

foreach ($name in $Scenario) {
    $null = Resolve-KmgBatchScenarioParameters -Scenarios $Scenario -CurrentScenario $name `
        -DefaultParameters $ScenarioParameters -ParameterMap $ScenarioParameterMap
}
Assert-KmgNotRunning

$sprint40PeerProtection=$DevelopmentPurpose -ceq 'sprint40-mechanics' -or
    ($ScenarioParameters.ContainsKey('reviewPhase') -and $ScenarioParameters.reviewPhase -cmatch '^sprint40-') -or
    ($ScenarioParameters.ContainsKey('persistenceScope') -and $ScenarioParameters.persistenceScope -ceq 'ice-devil')
$startupPeerScope=$null; $startupPeerSnapshot=$null; $startupPeerCatalog=$null; $startupGameSettings=$null
$startupRestorationFailed=$false
if($sprint40PeerProtection -or $Sprint40DiagnosticRecovery){
    . (Join-Path $PSScriptRoot 'Sprint40StartupFiles.Common.ps1')
    . (Join-Path $PSScriptRoot 'TeleportationPersistence.Common.ps1')
    if($env:COMPUTERNAME -ine 'HOWARD-LAPTOP' -or
        -not [IO.Path]::GetFullPath($LiveModDirectory).Equals('C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',[StringComparison]::OrdinalIgnoreCase)){
        throw 'Sprint40 peer protection requires the assigned host and exact captured game installation.'
    }
}
if($Sprint40DiagnosticRecovery){
    . (Join-Path $PSScriptRoot 'FeatureModuleCatalog.ps1')
    $originalModules=Get-Content -LiteralPath (Join-Path $LiveModDirectory 'FeatureModules.json') -Raw | ConvertFrom-Json
    Assert-KmgSprint40DiagnosticRecoveryRequest -Scenarios $Scenario -Purpose $DevelopmentPurpose -SaveName $SaveName `
        -ModuleDisabled ([bool]$ExpandedSummoningDisabled) -Parameters $ScenarioParameters `
        -OriginalParameters (Get-KmgOriginalModuleRuntimeParameters $originalModules)
    if(-not $PackagePath -or $ScenarioParameterMap.Count -ne 0){throw 'Recovery requires one explicit source-bound package and no alternate parameter map.'}
}
if($sprint40PeerProtection -and -not $PSCmdlet.ShouldProcess('exact HOWARD-LAPTOP installation and nine startup-written peer files',
    'guarded source-bound startup with byte/metadata backup and restoration')){return}

$before = Get-KmgTreeFingerprint -Directory $LiveModDirectory
Write-Host "Pre-run live tree: files=$($before.Files) sha256=$($before.Sha256)"

# Snapshot what we are about to disturb. AllowEmptySource keeps a clean machine
# (no mod installed) restorable to that same clean state.
$snapshot = & (Join-Path $PSScriptRoot 'Backup-Live-Mod.ps1') `
    -LiveModDirectory $LiveModDirectory -Confirm:$false `
    -AllowEmptySource:($before.Files -eq 0)
Write-Host "Mission snapshot: $($snapshot.Destination)"

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
if (-not [string]::IsNullOrEmpty($DevelopmentPurpose)) {
    [void](Assert-KmgDevelopmentPurpose $DevelopmentPurpose)
    if ([string]::IsNullOrWhiteSpace($PackagePath)) {
        throw 'A development purpose requires one explicit development PackagePath.'
    }
}

try {
    if($sprint40PeerProtection){
        $startupPeerScope=Enter-KmgRuntimeLease -Purpose 'Sprint40 startup file protection'
        $startupEvidence=Join-Path 'C:/Dev/KingmakerGunslingerLab/runtime-evidence' ('sprint40-startup-files-'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ'))
        $startupPeerSnapshot=Backup-KmgSprint40StartupFiles -ModsRoot (Split-Path $LiveModDirectory -Parent) -EvidenceDirectory $startupEvidence
        $record.startupPeerSnapshot=$startupPeerSnapshot
        if(-not $SaveName){$startupPeerCatalog=Open-KmgProtectedSaveCatalog -EvidenceDirectory $startupEvidence}
        $key=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\Owlcat Games\Pathfinder Kingmaker',$false)
        try{$startupGameSettings=@(Get-KmgFlatGameSettingsValues $key)}finally{$key.Dispose()}
    }
    if ($ExpandedSummoningDisabled) {
        $settingsPath = Join-Path $LiveModDirectory 'FeatureModules.json'
        if (-not (Test-Path -LiteralPath $settingsPath -PathType Leaf)) {
            throw 'The installed FeatureModules.json is missing; no module-off test was staged.'
        }
        $settingsBefore = [IO.File]::ReadAllBytes($settingsPath)
        $settingsBeforeSha = (Get-FileHash -LiteralPath $settingsPath -Algorithm SHA256).Hash
        $settingsOff = ConvertTo-KmgDisabledExpandedSummoningSettingsBytes `
            -OriginalBytes $settingsBefore
        $settingsTemporary = $settingsPath + '.kmg-phase2-test.tmp'
        [IO.File]::WriteAllBytes($settingsTemporary, $settingsOff)
        Move-Item -LiteralPath $settingsTemporary -Destination $settingsPath -Force
        $record.featureModuleTransition = [ordered]@{
            name = 'expanded-summoning'
            beforeSha256 = $settingsBeforeSha
            stagedSha256 = (Get-FileHash -LiteralPath $settingsPath -Algorithm SHA256).Hash
            restoredBy = 'verified live-tree snapshot'
        }
        Write-Host 'Staged Expanded Summoning OFF for one guarded working-save verify/cleanup; the outer snapshot will restore the exact original setting.'
    }
    if ($PackagePath) {
        # One deployment for the whole batch, whether this is a development
        # candidate or an already-frozen release one. Every scenario below then
        # runs against these exact installed bytes through the guarded reuse
        # authority, which is what lets many fresh executions share one
        # package without any of them rebuilding it.
        $reusePackage = (Resolve-Path -LiteralPath $PackagePath).Path
        $deployArguments = @{ PackagePath = $reusePackage; Confirm = $false; PassThru = $true }
        if($startupPeerScope){$deployArguments.RuntimeLease=$startupPeerScope.Lease}
        if (-not [string]::IsNullOrEmpty($DevelopmentPurpose)) {
            $deployArguments.DevelopmentPurpose = $DevelopmentPurpose
        }
        $deploymentOutput = @(& (Join-Path $PSScriptRoot 'Deploy-Local.ps1') @deployArguments)
        if ($deploymentOutput.Count -ne 1 -or
            $deploymentOutput[0] -isnot [string] -or
            -not (Test-Path -LiteralPath $deploymentOutput[0] -PathType Leaf)) {
            throw 'Explicit-candidate deployment did not return exactly one deployment manifest.'
        }
        $reuseManifest = (Resolve-Path -LiteralPath $deploymentOutput[0]).Path
        $record.explicitPackage = $reusePackage
        $record.explicitDeploymentManifest = $reuseManifest
        if (-not [string]::IsNullOrEmpty($DevelopmentPurpose)) {
            $record.developmentPurpose = $DevelopmentPurpose
            $record.developmentDeploymentManifest = $reuseManifest
            $record.developmentPackage = $reusePackage
        }
    }
    # Each scenario is attempted even if an earlier one fails, so one bad
    # scenario cannot hide the rest of a baseline suite. Restoration still
    # happens exactly once, in the finally below.
    $first = $true
    foreach ($name in $Scenario) {
        # The harness returns while Kingmaker is still shutting down. The
        # restoration below already waits for the process to go; a following
        # scenario must wait the same way, or its launcher refuses on the
        # previous scenario's exiting process and the whole batch after it
        # records ERROR without ever running.
        if (-not $first) {
            $nextDeadline = [DateTime]::UtcNow.AddSeconds(180)
            while (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and
                [DateTime]::UtcNow -lt $nextDeadline) {
                Start-Sleep -Milliseconds 500
            }
            # A scenario whose harness timed out waiting for the result, while
            # the game went on to finish and exit by itself, leaves its lease
            # lock behind and every later scenario refuses on it. Once the
            # game is gone that lock is stale, and it is released here under
            # the same ownership rule the restoration below applies - never a
            # lock this batch did not create, never while the game runs.
            $betweenLock = 'C:\Dev\KingmakerGunslingerLab\compatibility-state\compatibility.lock'
            if (-not $startupPeerScope -and @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -eq 0 -and
                (Test-Path -LiteralPath $betweenLock -PathType Leaf)) {
                $betweenOwner = (Get-Content -LiteralPath $betweenLock -Raw).Trim()
                if (Test-KmgCompatibilityLockOwned -LockOwner $betweenOwner `
                    -BatchStartedUtc ([DateTime]$record.startedAtUtc)) {
                    . (Join-Path $PSScriptRoot 'compatibility\CompatibilityProfile.Common.ps1')
                    Remove-KmgCompatibilityOwnedLock -LockPath $betweenLock -RunId $betweenOwner
                    if (-not $record.Contains('releasedStaleCompatibilityLocksBetweenScenarios')) {
                        $record.releasedStaleCompatibilityLocksBetweenScenarios = @()
                    }
                    $record.releasedStaleCompatibilityLocksBetweenScenarios += $betweenOwner
                    Write-Host "Released this batch's stale compatibility lock before $name`: $betweenOwner"
                }
            }
        }
        Write-Host "=== scenario: $name ==="
        $run = [ordered]@{ scenario = $name; startedAtUtc = [DateTime]::UtcNow.ToString('o') }
        try {
            $nativeParameters = Resolve-KmgBatchScenarioParameters -Scenarios $Scenario -CurrentScenario $name `
                -DefaultParameters $ScenarioParameters -ParameterMap $ScenarioParameterMap
            if($Sprint40DiagnosticRecovery){
                $nativeParameters=$nativeParameters.Clone()
                $nativeParameters.sprint40DiagnosticRecovery=$true
            }
            $arguments = @{
                Scenario = $name
                ExpectedVersion = $ExpectedVersion
                Parameters = $nativeParameters
                TimeoutSeconds = $TimeoutSeconds
                ObserverStartupTimeoutSeconds = $ObserverStartupTimeoutSeconds
                CompletionTimeoutSeconds = $CompletionTimeoutSeconds
                ExitAfterCompletion = $true
                Confirm = $false
            }
            if ($AllowDirtyGit) { $arguments.AllowDirtyGit = $true }
            if ($SaveName) { $arguments.SaveName = $SaveName }
            if ($PackagePath) { $arguments.PassThru = $true }
            if ($DevelopmentPurpose) { $arguments.DevelopmentPurpose = $DevelopmentPurpose }
            if ($startupPeerScope) { $arguments.RuntimeLease = $startupPeerScope.Lease }
            # An explicit package was deployed once above, so every scenario
            # reuses it. Without one, only the first scenario builds and
            # deploys and the rest run against what it installed. Reuse is
            # refused unless the exact deployment manifest and package are
            # named, so both are resolved from what actually happened rather
            # than assumed.
            if ($reusePackage -and $reuseManifest) {
                $arguments.ReuseInstalledArtifact = $true
                $arguments.DeploymentManifestPath = $reuseManifest
                $arguments.PackagePath = $reusePackage
            }
            $launchOutput = @(& (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') @arguments)
            $run.exitCode = $LASTEXITCODE
            # How the launcher exited and what the scenario recorded are two
            # separate facts; this is only the first of them.
            $launcher = if ($LASTEXITCODE -eq 0) { 'Clean' } else { 'Unclean' }
            if ($PackagePath) {
                $receipts = @($launchOutput | Where-Object { $null -ne $_.PSObject.Properties['kind'] -and $_.kind -ceq 'KmgRuntimeLaunchReceipt' })
                if ($receipts.Count -ne 1) { throw 'Explicit-candidate launch did not return exactly one current-run receipt.' }
                $receipt = $receipts[0]
                $reusePackage = $receipt.packagePath
                $reuseManifest = $receipt.deploymentManifestPath
                $run.runId = $receipt.runId
                $run.exactEvidenceDirectory = $receipt.evidenceDirectory
                $run.deploymentManifest = $reuseManifest
                $run.package = $reusePackage
            }
            elseif ($first) {
                # Capture what the deploying run produced so later scenarios can
                # name it exactly; the harness refuses a vague reuse.
                $candidate = (Get-KmgLocalCandidateLayout -RepositoryRoot $repositoryRoot `
            -Commit ((& git -C $repositoryRoot rev-parse HEAD).Trim()) `
            -Id 'KingmakerGunslinger' -Version $ExpectedVersion).PackagePath
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
            $launcher = 'Unclean'
            $run.error = $_.Exception.Message
            Write-Warning "Scenario $name errored: $($_.Exception.Message)"
        }
        # The harness can write a complete result and still throw during
        # teardown. Read what the scenario actually recorded so a teardown fault
        # is not reported as a scenario failure, and vice versa.
        #
        # The evidence MUST come from this run. Taking the newest directory that
        # merely matches the scenario name once adopted an August result for a
        # scenario that never ran, and reported it as a pass. Evidence
        # directories are named with a UTC stamp, so require that stamp to be at
        # or after the moment this scenario started.
        $startedStamp = ([DateTime]$run.startedAtUtc).ToUniversalTime()
        if ($PackagePath) {
            # The array subexpression must wrap the whole conditional. An
            # assignment from a statement whose branch yields an empty array
            # collapses to $null, and a scenario that errors before recording
            # its exact evidence directory takes exactly that branch - so the
            # wrapper used to bind $null to DirectoryNames and throw a
            # parameter-binding error after restoration, hiding the real
            # scenario failure behind it.
            $evidenceNames = @(if ($run.Contains('exactEvidenceDirectory')) {
                Split-Path -Leaf $run.exactEvidenceDirectory })
        } else {
        $evidenceNames = @(Get-ChildItem 'C:\Dev\KingmakerGunslingerLab\runtime-evidence' -Directory -ErrorAction SilentlyContinue |
            ForEach-Object Name)
        }
        $evidenceName = Select-KmgScenarioEvidence -DirectoryNames $evidenceNames `
            -Scenario $name -StartedUtc $startedStamp
        $evidence = if ($evidenceName) {
            Get-Item -LiteralPath (Join-Path 'C:\Dev\KingmakerGunslingerLab\runtime-evidence' $evidenceName)
        } else { $null }
        $scenarioStatus = $null
        if ($evidence) {
            $run.evidenceDirectory = $evidence.Name
            $scenarioStatus = Read-KmgScenarioStatus -Scenario $name `
                -ResultPath (Join-Path $evidence.FullName 'runtime-result.json')
        }
        # A result file that is missing, unreadable, malformed, or written for a
        # different scenario yields no status, which is treated as no usable
        # evidence rather than as a pass.
        $run.scenarioStatus = if ($scenarioStatus) { $scenarioStatus }
            else { 'NO-EVIDENCE-FROM-THIS-RUN' }
        $run.launcherOutcome = $launcher
        $run.outcome = Resolve-KmgScenarioOutcome -LauncherOutcome $launcher `
            -EvidenceStatus $scenarioStatus
        if (Test-KmgScenarioOutcomeFailed $run.outcome) { $failures++ }
        $run.completedAtUtc = [DateTime]::UtcNow.ToString('o')
        $record.runs += $run
        if (Test-KmgBatchCandidateUnavailable -FirstScenario $first -HasEvidence ([bool]$evidence) `
            -HasDeployment ([bool]$reuseManifest) -LauncherOutcome $launcher) {
            # A rejected source/build candidate cannot run any later scenario.
            # Do not repeat that same full gate once per scenario. Runtime
            # failures with current-run evidence still allow independent cells.
            $record.abortedBeforeCandidate = $true
            Write-Warning 'Batch stopped before a candidate existed; remaining scenarios were not launched.'
            break
        }
        $first = $false
    }
}
finally {
    # Runs on success, on failure, and on interruption.
    $record.failures = $failures
    $deployed = Get-KmgTreeFingerprint -Directory $LiveModDirectory
    $record.liveAfterScenario = [ordered]@{ files = $deployed.Files; sha256 = $deployed.Sha256 }

    if (-not (Resolve-KmgRestorationNeeded -BeforeSha256 $before.Sha256 `
        -AfterScenarioSha256 $deployed.Sha256)) {
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
            if (-not $startupPeerScope -and (Test-Path -LiteralPath $compatibilityLock -PathType Leaf)) {
                $lockOwner = (Get-Content -LiteralPath $compatibilityLock -Raw).Trim()
                if (Test-KmgCompatibilityLockOwned -LockOwner $lockOwner `
                    -BatchStartedUtc ([DateTime]$record.startedAtUtc)) {
                    . (Join-Path $PSScriptRoot 'compatibility\CompatibilityProfile.Common.ps1')
                    Remove-KmgCompatibilityOwnedLock -LockPath $compatibilityLock -RunId $lockOwner
                    $record.releasedStaleCompatibilityLock = $lockOwner
                } else {
                    $record.foreignCompatibilityLock = $lockOwner
                }
            }

            Assert-KmgNotRunning
            $restoreArguments=@{BackupDirectory=$snapshot.Destination;LiveModDirectory=$LiveModDirectory;Confirm=$false}
            if($startupPeerScope){$restoreArguments.RuntimeLease=$startupPeerScope.Lease}
            & (Join-Path $PSScriptRoot 'Restore-Live-Mod.ps1') @restoreArguments | Out-Null
            $after = Get-KmgTreeFingerprint -Directory $LiveModDirectory
            $record.liveAfterRestore = [ordered]@{ files = $after.Files; sha256 = $after.Sha256 }
            $record.restoration = Resolve-KmgRestorationResult `
                -BeforeSha256 $before.Sha256 -RestoredSha256 $after.Sha256
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

    if($startupPeerScope){
        try {
            Assert-KmgNotRunning
            $nativeDiagnostic=$null
            if($Sprint40DiagnosticRecovery){
                $record.postStartupDiagnosticCapture=Save-KmgSprint40GeneratedDiagnosticEvidence -Snapshot $startupPeerSnapshot
                if($record.runs.Count -eq 1 -and $record.runs[0].Contains('exactEvidenceDirectory')){
                    $candidateDiagnostic=Join-Path $record.runs[0].exactEvidenceDirectory 'sprint40-generated-diagnostic.json'
                    if(Test-Path -LiteralPath $candidateDiagnostic -PathType Leaf){
                        $targeted=Read-KmgCompatibilityJson $candidateDiagnostic
                        if($targeted.exactOriginalRegeneration -eq $true){$nativeDiagnostic=$candidateDiagnostic}
                    }
                }
            }
            $boundary=Invoke-KmgIndependentBoundaryRestoration -RestoreSettings {
                if($startupGameSettings){
                    $key=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\Owlcat Games\Pathfinder Kingmaker',$true)
                    try{Restore-KmgFlatGameSettingsValues -Key $key -Snapshot $startupGameSettings}finally{$key.Dispose()}
                }
            } -RestoreInstallation {
                if($startupPeerSnapshot){Restore-KmgSprint40StartupFiles -Snapshot $startupPeerSnapshot -RegeneratedDiagnosticReceipt $nativeDiagnostic}
            }
            $record.startupPeerRestoration=$boundary.transaction
            if($startupPeerCatalog){$record.startupSavePreservation=Assert-KmgProtectedSaveCatalog $startupPeerCatalog}
            if($boundary.settingsError){throw $boundary.settingsError}
            if($boundary.installationError){throw $boundary.installationError}
        } catch {
            $startupRestorationFailed=$true
            $record.startupPeerRestoration=[ordered]@{disposition='FAILED';error=$_.Exception.Message}
            Write-Warning ('Sprint40 startup restoration failed: '+$_.Exception.Message)
        } finally {
            if($startupPeerCatalog){Close-KmgProtectedSaveCatalog $startupPeerCatalog}
            Exit-KmgRuntimeLease $startupPeerScope
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
if($startupRestorationFailed){throw 'Sprint40 startup restoration failed; inspect the exact restoration receipt before any further dispatch.'}
