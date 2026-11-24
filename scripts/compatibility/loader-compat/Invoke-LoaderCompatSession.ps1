<#
.SYNOPSIS
Runs one loader-compatibility session: one loader profile and one mod set in the
disposable game copy, then a sequence of fresh game processes.

.DESCRIPTION
Each launch is a new Kingmaker process started directly from the copy. The
test-only KmgLoaderCompatHarness redirects saves and the area cache to the
session profile, records the in-process loader/Harmony/mod identity, and either
runs its own steps or only isolates while the candidate's own guarded runtime
scenario (-kmgRuntimeTestRequest) runs. The copy's Mods folder and the session
profile persist between launches, so settings and saves cross real restarts.

Safety (adapted from the Kingmaker Cutscene Skip lab harness). The runner
supports one new, exclusively owned session followed by its normal verified
completion:
- before anything changes it refuses a running Kingmaker, any configured
  runtime or game-copy lock, a pending session journal, or Steam Cloud sync not
  proven disabled, and takes over or cleans up none of them;
- it refuses any copy outside the lab's game-copies root and stops only a
  process it started from the copy;
- it holds the lab's shared runtime lease and its own game-copy lock for the
  whole session and re-checks the host before every launch; a foreign game, a
  foreign lab lock or a lost claim observed at any point is remembered, and
  that session is retained without any automatic restoration, even if the peer
  has exited by completion;
- it hashes the owner's saves and loader/mod files at intake, backs up the
  game's registry settings bound to a digest, and at completion imports a held,
  verified copy of that backup, reads the key back and compares it exactly,
  and verifies the owner's files before releasing ownership.

Any failure of that completion exits 1 and retains the journal, both locks, the
backup and a RecoveryRequired lease; later sessions refuse that state. There
is no recovery mode: an interrupted or retained session is resolved by a
deliberate manual procedure ("Manual recovery" in
docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md).
#>
[CmdletBinding()]
param(
    [string]$SessionFile,
    [ValidateSet('umm-0.31.1', 'umm-0.32.4', 'owner-0.33.0')][string]$LoaderProfile,
    [string]$CandidateZip,
    # The development purpose the candidate was built for (development candidates), and the explicit marker for a
    # historical release control (package contract only).
    [string]$CandidateDevelopmentPurpose,
    [switch]$HistoricalReleaseControl,
    [string]$HarnessDir,
    [string]$GameCopy = 'C:\Dev\KingmakerGunslingerLab\game-copies\loader-compat',
    [string]$FixtureDir = 'C:\Dev\KingmakerSkipCinematicLab\fixtures',
    [int]$LaunchTimeoutSeconds = 900,
    # Read-only: prints the machine-recorded results of an existing session - the session-summary.json identity,
    # verdict, restoration and per-launch log fields, or with -ReportLaunch one launch's harness-result.json steps
    # and pricing/initialization/profile records - each with its file's SHA-256, so a finished session can be
    # surfaced as recorded command output. Starts nothing and changes nothing.
    [string]$Report,
    # With -Report: print that launch's records (label prefix, e.g. 02) instead of the session summary.
    [string]$ReportLaunch,
    # Runs the offline session-guard checks (Test-LoaderCompatSafety.ps1) and exits.
    [switch]$SafetyTests,
    # With -HistoricalReleaseControl: the recorded release version (scripts/historical-release-records.json).
    [string]$HistoricalReleaseVersion
)

. (Join-Path $PSScriptRoot 'LoaderCompat.Common.ps1')

if ($Report) {
    if ($Report -notmatch '^[0-9]{8}T[0-9]{6}Z-[A-Za-z0-9.\-]+$') { throw '-Report takes a session id such as 20261121T171410Z-umm-0.31.1-spear-bootstrap-simulated.' }
    $reportDir = Join-Path $script:LcEvidenceRoot $Report
    $summaryPath = Join-Path $reportDir 'session-summary.json'
    if (-not (Test-Path -LiteralPath $summaryPath)) { throw "No session summary at $summaryPath" }
    function Get-LcReportValue($Object, [string]$Name) {
        if ($null -eq $Object) { return $null }
        $property = $Object.PSObject.Properties[$Name]
        if ($null -eq $property) { return $null }
        # The unary comma keeps an array (empty and single-element included) as one value through the function
        # pipeline, so the report preserves each field's JSON type and cardinality.
        return , $property.Value
    }
    # Shape self-check, before any evidence is printed: empty and single-element arrays must survive the projection.
    $shapeProbe = '{"problems":[],"activeMods":["only"],"steps":[{"step":"a"}]}' | ConvertFrom-Json
    $shapeText = ([ordered]@{ problems = (Get-LcReportValue $shapeProbe 'problems'); activeMods = (Get-LcReportValue $shapeProbe 'activeMods')
        steps = (Get-LcReportValue $shapeProbe 'steps') } | ConvertTo-Json -Depth 4 -Compress)
    if ($shapeText -ne '{"problems":[],"activeMods":["only"],"steps":[{"step":"a"}]}') { throw "report projection changes array shape: $shapeText" }
    Write-Host "REPORT SHAPE CHECK $shapeText"
    $summaryData = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
    if (-not $ReportLaunch) {
        Write-Host "READ-ONLY REPORT $summaryPath sha256=$(Get-LcSha256 $summaryPath)"
        $head = [ordered]@{}
        foreach ($name in @('sessionId', 'session', 'loaderProfile', 'candidateZip', 'candidateZipSha256', 'git', 'candidateVersion', 'status',
                'ownerSaveFiles', 'registryRestored', 'registryRestore', 'registryVerification', 'transactionSucceeded', 'scenarioPassed', 'ownerSavesUnchanged',
                'ownerInstallUnchanged', 'ownerIntegrity', 'ownership', 'transactionId', 'runtimeLease', 'hostConflict', 'retained', 'failure', 'integrityFailures', 'stagedMods')) {
            $head[$name] = Get-LcReportValue $summaryData $name
        }
        # The session's shared runtime lease as it stands now: its journal status and whether the shared lock is free.
        $leaseId = Get-LcReportValue $summaryData 'runtimeLease'
        if ($leaseId) {
            $leasePath = Join-Path (Join-Path $script:LcStateRoot $leaseId) 'runtime-lease.json'
            $leaseState = if (Test-Path -LiteralPath $leasePath) { Get-Content -LiteralPath $leasePath -Raw | ConvertFrom-Json } else { $null }
            $sharedLock = Join-Path $script:LcStateRoot 'compatibility.lock'
            $head['runtimeLeaseNow'] = [ordered]@{ status = (Get-LcReportValue $leaseState 'status'); recoveryRequired = (Get-LcReportValue $leaseState 'recoveryRequired')
                purpose = (Get-LcReportValue $leaseState 'purpose'); reason = (Get-LcReportValue $leaseState 'reason')
                integrity = (Get-LcReportValue $leaseState 'integrity'); registryRestore = (Get-LcReportValue $leaseState 'registryRestore')
                sharedLockHeldBy = $(if (Test-Path -LiteralPath $sharedLock) { (Get-Content -LiteralPath $sharedLock -Raw).Trim() } else { '<free>' }) }
        }
        Write-Host ($head | ConvertTo-Json -Depth 6 -Compress)
        foreach ($launch in @($summaryData.launches)) {
            $identity = Get-LcReportValue $launch 'identity'
            Write-Host ([ordered]@{ label = $launch.label; verdict = $launch.verdict; harness = (Get-LcReportValue $launch 'harness')
                problems = (Get-LcReportValue $launch 'problems'); seconds = (Get-LcReportValue $launch 'seconds')
                conditions = (Get-LcReportValue $launch 'conditions'); productSha256 = (Get-LcReportValue $identity 'productSha256')
                umm = (Get-LcReportValue $identity 'umm'); activeMods = (Get-LcReportValue $identity 'activeMods')
                log = (Get-LcReportValue $launch 'log') } | ConvertTo-Json -Depth 6 -Compress)
        }
        return
    }
    foreach ($launchDir in @(Get-ChildItem -LiteralPath $reportDir -Directory | Where-Object { $_.Name -match '^[0-9]{2}-' -and $_.Name.StartsWith($ReportLaunch) } | Sort-Object Name)) {
        $resultPath = Join-Path $launchDir.FullName 'harness\harness-result.json'
        if (-not (Test-Path -LiteralPath $resultPath)) { Write-Host "== $($launchDir.Name): no harness-result.json"; continue }
        $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
        $picked = [ordered]@{ runId = (Get-LcReportValue $result 'runId'); status = (Get-LcReportValue $result 'status')
            failure = (Get-LcReportValue $result 'failure'); isolationRestored = (Get-LcReportValue $result 'isolationRestored')
            steps = (Get-LcReportValue $result 'steps') }
        foreach ($property in $result.PSObject.Properties) {
            $value = $property.Value
            if ($property.Name -match '^kmgItemPricing_') {
                $picked[$property.Name] = [ordered]@{ repeat = (Get-LcReportValue $value 'repeat')
                    sellPricePatchOwners = (Get-LcReportValue $value 'sellPricePatchOwners')
                    ordinary = @((Get-LcReportValue $value 'ordinary') | Where-Object { $_ } | ForEach-Object { [ordered]@{ name = $_.name
                        queries = $_.queries; sellPrices = $_.sellPrices; tooltipSellPrices = $_.tooltipSellPrices; priceThrows = $_.priceThrows
                        tooltipThrows = $_.tooltipThrows; priceCauseMatches = $_.priceCauseMatches; tooltipCauseMatches = $_.tooltipCauseMatches } })
                    batteredInspectionUnavailableReported = (Get-LcReportValue $value 'batteredInspectionUnavailableReported')
                    bootstrap = (Get-LcReportValue $value 'bootstrap') }
            }
            elseif ($property.Name -match '^(kmgInitialization|kmgSpearPersistFixture|kmgSpearPersistedItems|kmgModules)_') { $picked[$property.Name] = $value }
            elseif ($property.Name -match '^kmgItemDamage_') {
                $damage = [ordered]@{}
                foreach ($entry in $value.PSObject.Properties) { if ($entry.Name -ne 'conditions') { $damage[$entry.Name] = $entry.Value } }
                $picked[$property.Name] = $damage
            }
            elseif ($property.Name -match '^kmgWeaponTypeProfile_') {
                $owned = Get-LcReportValue $value 'ownedSpear'
                $picked[$property.Name] = [ordered]@{ ownedSpearPresent = (Get-LcReportValue $value 'ownedSpearPresent')
                    ownedGetters = (Get-LcReportValue $owned 'getters'); conditions = (Get-LcReportValue $value 'conditions') }
            }
        }
        Write-Host "== $($launchDir.Name) harness-result.json sha256=$(Get-LcSha256 $resultPath)"
        Write-Host ($picked | ConvertTo-Json -Depth 8 -Compress)
    }
    return
}

if ($SafetyTests) {
    # Offline fixture checks of the session guards: refusal of pre-existing state, shared ownership, per-launch
    # host checks, verified restoration and retention on failure. No game, loader, registry or owner file is touched.
    try { & (Join-Path $PSScriptRoot 'Test-LoaderCompatSafety.ps1') }
    catch { Write-Host "Safety checks aborted: $($_.Exception.Message)`n$($_.ScriptStackTrace)"; exit 1 }
    return
}

foreach ($required in @('SessionFile', 'LoaderProfile', 'CandidateZip', 'HarnessDir')) {
    if (-not $PSBoundParameters.ContainsKey($required)) { throw "-$required is required." }
}
$session = Get-Content -LiteralPath $SessionFile -Raw | ConvertFrom-Json
$copyRoot = Assert-LcGameCopy $GameCopy
$paths = Get-LcCopyPaths $copyRoot
$sessionId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' + $LoaderProfile + '-' + $session.name
$evidence = Join-Path $script:LcEvidenceRoot $sessionId
$profileDir = Join-Path $evidence 'profile'

# Everything checkable is checked before anything is acquired.
$admitted = Assert-LcSessionInputs $CandidateZip $HarnessDir $CandidateDevelopmentPurpose -HistoricalReleaseControl:$HistoricalReleaseControl -HistoricalReleaseVersion $HistoricalReleaseVersion
$summary = [ordered]@{ sessionId = $sessionId; session = $session.name; loaderProfile = $LoaderProfile
    candidateZip = $CandidateZip; candidateZipSha256 = [string]$admitted.packageSha256; candidateDllSha256 = [string]$admitted.dllSha256; gameCopy = $copyRoot
    admission = [ordered]@{ mode = [string]$admitted.admissionMode; version = [string]$admitted.version; producerCommit = [string]$admitted.producerCommit
        releaseTag = $admitted.releaseTag; packageSha256 = [string]$admitted.packageSha256; dllSha256 = [string]$admitted.dllSha256; dllMvid = [string]$admitted.dllMvid }
    git = (& git -C (Split-Path (Split-Path (Split-Path $PSScriptRoot))) rev-parse HEAD); launches = @(); status = 'started' }
# A busy host or a refused acquisition stops the session before staging; the admitted private copy is removed then.
# One journaled transaction for the whole session: the shared runtime lease (compatibility.lock) and the private
# loader-compat lock. From here on every fallible step is inside the try, whose finally completes or retains it.
$transaction = Enter-LcSessionForAdmission $admitted $sessionId
$scope = $transaction.Scope
$journal = $transaction.Journal
$summary.runtimeLease = $scope.Lease.RunId
$summary.transactionId = $journal.transactionId
$hostError = $false
$verdict = $null
try {
    New-Item -ItemType Directory -Force -Path $evidence, (Join-Path $profileDir 'Saved Games') | Out-Null
    $intake = Save-LcIntake $journal
    $summary.ownerSaveFiles = $intake.savesCount
    $registryBackup = Join-Path $evidence 'game-settings.reg'
    Backup-LcRegistry $registryBackup
    # Bound to its digest: completion imports only a held copy of these verified bytes and compares the readback with them.
    Set-LcRegistryBackup $journal $registryBackup

    Initialize-LcGameCopy $paths
    $summary.loaderInstall = Install-LcLoaderProfile $paths $LoaderProfile
    $summary.stagedMods = Set-LcGameCopyMods $paths @($session.ownerMods) $admitted $HarnessDir
    # The product's guarded request must name the staged candidate's own version.
    $candidateVersion = [string](Get-Content -LiteralPath (Join-Path $paths.ModsDir 'KingmakerGunslinger\Info.json') -Raw | ConvertFrom-Json).Version
    $summary.candidateVersion = $candidateVersion
    foreach ($name in @($session.fixtureSaves | Where-Object { $_ })) {
        Copy-Item -LiteralPath (Join-Path $FixtureDir $name) -Destination (Join-Path $profileDir "Saved Games\$name")
    }
    $expectedUmm = $summary.loaderInstall.files['UnityModManager.dll']
    $expectedHarmony2 = $summary.loaderInstall.files['0Harmony.dll']
    $expectedHarmony12 = $summary.loaderInstall.files['0Harmony12.dll']
    $expectedProduct = [string]$admitted.dllSha256

    $index = 0
    foreach ($launch in @($session.launches)) {
        $index++
        $label = '{0:D2}-{1}' -f $index, $launch.label
        $launchDir = Join-Path $evidence $label
        New-Item -ItemType Directory -Force -Path $launchDir | Out-Null
        $record = [ordered]@{ label = $label; verdict = 'not-run'; problems = @() }
        $product = $launch.PSObject.Properties['product']
        $harness = $launch.PSObject.Properties['harness']

        # Harness request: its own steps, or isolation and identity only.
        $save = $null; $steps = @(); $harnessExits = $true
        # Controlled pre-initialization conditions (ReproductionConditions.cs); they apply to product launches too.
        $conditions = [ordered]@{}
        if ($launch.PSObject.Properties['conditions'] -and $launch.conditions) {
            foreach ($p in $launch.conditions.PSObject.Properties) { $conditions[$p.Name] = [string]$p.Value }
        }
        if ($harness) {
            $steps = @($harness.Value.steps)
            if ($harness.Value.PSObject.Properties['save'] -and $harness.Value.save) {
                $save = [string]$harness.Value.save
                if ($save.StartsWith('$written:')) {
                    $written = Get-ChildItem -LiteralPath (Join-Path $profileDir 'Saved Games') -Filter ('*_' + $save.Substring(9) + '.zks') |
                        Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
                    if (-not $written) { $record.verdict = 'blocked'; $record.problems += "no written save $save"; $summary.launches += $record; continue }
                    $save = $written.Name
                }
            }
        }
        if ($product) { $harnessExits = $false }
        $harnessRequest = [ordered]@{ runId = "$sessionId-$label"; evidenceDir = (Join-Path $launchDir 'harness'); profileDir = $profileDir
            save = $save; timeoutSeconds = $LaunchTimeoutSeconds - 60; exitAfterCompletion = $harnessExits; steps = $steps; conditions = $conditions }
        $harnessRequestPath = Join-Path $launchDir 'harness-request.json'
        # '$candidateVersion' in a step stands for the staged candidate's own Info.json version.
        [IO.File]::WriteAllText($harnessRequestPath, ($harnessRequest | ConvertTo-Json -Depth 12).Replace('$candidateVersion', $candidateVersion),
            (New-Object Text.UTF8Encoding($false)))
        $arguments = @("-kmgLoaderCompatRequest=$harnessRequestPath")
        # Extra game arguments, for example -locale=zhCN, which Kingmaker's own startup reads.
        if ($launch.PSObject.Properties['gameArguments']) { $arguments += @($launch.gameArguments | ForEach-Object { [string]$_ }) }
        $record.conditions = $conditions
        $record.gameArguments = @($arguments | Select-Object -Skip 1)
        $productEvidence = $null
        if ($product) {
            $productEvidence = Join-Path $launchDir 'product'
            New-Item -ItemType Directory -Force -Path $productEvidence | Out-Null
            $runId = ('lc-' + $sessionId + '-' + $label) -replace '[^A-Za-z0-9._-]', '-'
            if ($runId.Length -gt 100) { $runId = $runId.Substring($runId.Length - 100) }
            $request = [ordered]@{ schemaVersion = 1; enabled = $true; runId = $runId; scenario = $product.Value.scenario
                expectedModVersion = $candidateVersion; evidenceDirectory = $productEvidence
                timeoutSeconds = [int]$(if ($product.Value.PSObject.Properties['timeoutSeconds']) { $product.Value.timeoutSeconds } else { 600 })
                startupTimeoutSeconds = 300; exitAfterCompletion = $true
                parameters = $(if ($product.Value.PSObject.Properties['parameters']) { $product.Value.parameters } else { [ordered]@{} }) }
            # A scenario that loads a save through the product's own working-save route must carry the
            # request's catalog, selection, completion and stage timeouts; the product refuses it otherwise.
            if ($product.Value.PSObject.Properties['timeouts'] -and $product.Value.timeouts) {
                foreach ($p in $product.Value.timeouts.PSObject.Properties) { $request[$p.Name] = [int]$p.Value }
            }
            $productRequestPath = Join-Path $launchDir 'product-request.json'
            Write-LcJson $productRequestPath $request
            $arguments += @('-kmgRuntimeTestRequest', $productRequestPath)
        }

        # Every fresh launch re-checks the host and the session's claims: a peer game or a sibling lab lock that
        # appeared after the session preflight withholds this launch and every later one.
        try { Assert-LcLaunchAvailable $scope.Lease $journal }
        catch {
            $record.verdict = 'withheld'; $record.problems += $_.Exception.Message
            Write-LcJson (Join-Path $launchDir 'launch-summary.json') $record
            $summary.launches += $record
            Write-Host ("{0} WITHHELD {1}" -f $label, $_.Exception.Message)
            break
        }
        $started = Get-Date
        $process = Start-LcKingmakerCopy $paths $arguments
        $record.pid = $process.Id
        if (-not $process.WaitForExit($LaunchTimeoutSeconds * 1000)) {
            $record.problems += 'launch timeout'
            $record.stop = Stop-LcOwnedProcess $process $paths
        }
        $record.seconds = [int]((Get-Date) - $started).TotalSeconds
        foreach ($log in @(@((Join-Path $paths.UmmDir 'Log.txt'), 'umm-log.txt'), @((Join-Path $script:LcOwnerData 'output_log.txt'), 'player-log.txt'))) {
            if (Test-Path -LiteralPath $log[0]) { Copy-Item -LiteralPath $log[0] -Destination (Join-Path $launchDir $log[1]) -Force }
        }

        # In-process identity: the files the process actually loaded.
        $identityPath = Join-Path $launchDir 'harness\loader-identity.json'
        if (Test-Path -LiteralPath $identityPath) {
            $identity = Get-Content -LiteralPath $identityPath -Raw | ConvertFrom-Json
            $productEntry = @($identity.modEntries | Where-Object id -eq 'KingmakerGunslinger')
            $record.identity = [ordered]@{ umm = $identity.unityModManager.version; ummSha256 = $identity.unityModManager.sha256
                ummReported = $identity.unityModManagerReportedVersion
                harmony2 = @($identity.harmony2 | ForEach-Object { $_.version + ' ' + $_.sha256 }); harmony12Sha256 = $identity.harmony12.sha256
                productSha256 = $(if ($productEntry.Count -eq 1) { $productEntry[0].assembly.sha256 })
                activeMods = @($identity.modEntries | Where-Object { $_.active } | ForEach-Object { $_.id + ' ' + $_.version }) }
            if ($identity.unityModManager.sha256 -ne $expectedUmm) { $record.problems += "in-process UnityModManager $($identity.unityModManager.sha256) is not the installed profile file" }
            if (@($identity.harmony2).Count -ne 1 -or $identity.harmony2[0].sha256 -ne $expectedHarmony2) { $record.problems += 'in-process 0Harmony is not exactly the profile file' }
            if ($identity.harmony12.sha256 -ne $expectedHarmony12) { $record.problems += 'in-process 0Harmony12 is not the profile file' }
            if ($record.identity.productSha256 -ne $expectedProduct) { $record.problems += 'in-process product DLL is not the candidate' }
        } else { $record.problems += 'no in-process loader identity was recorded' }

        $harnessResultPath = Join-Path $launchDir 'harness\harness-result.json'
        if (Test-Path -LiteralPath $harnessResultPath) {
            $harnessResult = Get-Content -LiteralPath $harnessResultPath -Raw | ConvertFrom-Json
            $record.harness = $harnessResult.status
            if ($launch.PSObject.Properties['expectHarnessFailure'] -and $launch.expectHarnessFailure) {
                # This launch must end with the harness failing, its failure naming every listed text, and none of
                # the listed steps (matched by "[label]") having run.
                $record.expectedHarnessFailure = @($launch.expectHarnessFailure)
                if ($harnessResult.status -ne 'failed') { $record.problems += "harness $($harnessResult.status); an expected failure did not occur" }
                foreach ($needle in @($launch.expectHarnessFailure)) {
                    if (([string]$harnessResult.failure).IndexOf([string]$needle, [StringComparison]::Ordinal) -lt 0) {
                        $record.problems += "harness failure lacks '$needle': $($harnessResult.failure)"
                    }
                }
                if ($launch.PSObject.Properties['expectStepsNotRun']) {
                    foreach ($stepLabel in @($launch.expectStepsNotRun)) {
                        if (@($harnessResult.steps | Where-Object { ([string]$_.step).Contains("[$stepLabel]") }).Count -ne 0) {
                            $record.problems += "step [$stepLabel] ran after the expected stop"
                        }
                    }
                }
            } elseif ($harnessResult.status -ne 'passed') { $record.problems += "harness: $($harnessResult.failure)" }
        } elseif ($product) {
            # The product's scenario owns the exit; the harness only had to
            # isolate the profile and record identity, both checked here.
            $stage = Join-Path $launchDir 'harness\harness-stage-loaded.json'
            if (-not (Test-Path -LiteralPath $stage) -or -not ((Get-Content -LiteralPath $stage -Raw | ConvertFrom-Json).isolation)) {
                $record.problems += 'profile isolation was not confirmed'
            }
        } else { $record.problems += 'no harness result' }
        if ($product) {
            $productResultPath = Join-Path $productEvidence 'runtime-result.json'
            if (Test-Path -LiteralPath $productResultPath) {
                $productResult = Get-Content -LiteralPath $productResultPath -Raw | ConvertFrom-Json
                $record.product = [ordered]@{ scenario = $productResult.scenario; status = $productResult.status
                    failed = @($productResult.assertions | Where-Object { $_.status -ne 'PASS' } | ForEach-Object { $_.name + ': ' + $_.observed }) }
                if ($productResult.status -ne 'PASS') { $record.problems += "product $($product.Value.scenario): $($productResult.status)" }
            } else { $record.problems += "no product result for $($product.Value.scenario)" }
        }
        # Product log lines this launch must and must not contain, read from the launch's own loader log copy.
        if ($launch.PSObject.Properties['expectLog']) {
            $logCopy = Join-Path $launchDir 'umm-log.txt'
            if (-not (Test-Path -LiteralPath $logCopy)) { $record.problems += 'no loader log was captured for expectLog' }
            else {
                $logText = [IO.File]::ReadAllText($logCopy)
                $record.log = [ordered]@{ present = @(); absent = @(); once = @() }
                foreach ($group in @('present', 'absent', 'once')) {
                    if (-not $launch.expectLog.PSObject.Properties[$group]) { continue }
                    foreach ($needle in @($launch.expectLog.$group | Where-Object { $_ })) {
                        $count = ([regex]::Matches($logText, [regex]::Escape([string]$needle))).Count
                        $record.log[$group] += "$needle x$count"
                        if ($group -eq 'present' -and $count -eq 0) { $record.problems += "log lacks '$needle'" }
                        if ($group -eq 'absent' -and $count -ne 0) { $record.problems += "log contains '$needle' ($count)" }
                        if ($group -eq 'once' -and $count -ne 1) { $record.problems += "log contains '$needle' $count times, expected exactly once" }
                    }
                }
            }
        }
        $record.verdict = $(if ($record.problems.Count -eq 0) { 'PASS' } else { 'FAIL' })
        Write-LcJson (Join-Path $launchDir 'launch-summary.json') $record
        $summary.launches += $record
        Write-Host ("{0} {1} {2}s {3}" -f $label, $record.verdict, $record.seconds, ($record.problems -join '; '))
    }
}
catch {
    $hostError = $true
    $summary.failure = $_.ToString()
    Write-Host "Session host error: $($_.Exception.Message)`n$($_.ScriptStackTrace)"
}
finally {
    $problems = @()
    # The admitted private copy is the session's: removed if staging did not consume it.
    try { Remove-LcAdmittedCopy $admitted } catch { $problems += "admitted private copy cleanup failed: $($_.Exception.Message)" }
    # At a safe boundary only: restore the verified game settings, read them back, verify the owner's files, then
    # complete the lease and release this session's claims. Any failure retains the session for manual recovery.
    $ownership = $null
    try { $ownership = Complete-LcSessionOwnership $journal $scope { param($Path) Restore-LcRegistry $Path } }
    catch { $problems += "ownership completion threw: $($_.Exception.Message)" }
    $summary.ownership = $(if ($null -ne $ownership) { $ownership.Outcome } else { 'unknown' })
    $summary.registryRestore = [string]$journal.registryRestore
    $summary.registryRestored = ([string]$journal.registryRestore) -cin @('applied', 'not-needed')
    $summary.registryVerification = $journal.registryVerification
    $summary.hostConflict = $journal.hostConflict
    if ($null -eq $ownership -or $ownership.Outcome -cne 'completed') {
        if ($null -ne $ownership) { $problems += $ownership.Reason }
        $summary.retained = [ordered]@{ journal = $script:LcJournal; transactionId = $journal.transactionId; leaseRunId = $journal.leaseRunId
            leaseStatePath = $journal.leaseStatePath; sharedLock = (Get-LcSharedLockPath); privateLock = $script:LcLock
            registryBackup = $journal.registryBackup; registryBackupSha256 = $journal.registryBackupSha256; phase = $journal.phase }
    }
    $check = $journal.integrity
    if ($null -eq $check) { $summary.ownerIntegrity = 'not-verified' }
    else {
        $summary.ownerIntegrity = $check.status
        if ($check.Contains('ownerSavesUnchanged')) {
            $summary.ownerSavesUnchanged = $check.ownerSavesUnchanged
            $summary.ownerInstallUnchanged = $check.ownerInstallUnchanged
            if (-not $check.ownerSavesUnchanged) { $summary.ownerSaveChanges = $check.ownerSaveChanges }
            if (-not $check.ownerInstallUnchanged) { $summary.ownerInstallChanges = $check.ownerInstallChanges }
        }
    }
    $verdict = Get-LcSessionVerdict @($summary.launches) $hostError $ownership
    $summary.scenarioPassed = $verdict.ScenarioPassed
    $summary.transactionSucceeded = $verdict.TransactionSucceeded
    $summary.status = $verdict.Status
    if ($problems.Count) { $summary.integrityFailures = $problems }
    if (Test-Path -LiteralPath $evidence) { Write-LcJson (Join-Path $evidence 'session-summary.json') $summary }
    Write-Host "Session $sessionId : $($summary.status) (scenario $(if ($verdict.ScenarioPassed) { 'passed' } else { 'not passed' }); transaction $($summary.ownership), game settings $($summary.registryRestore), owner files $($summary.ownerIntegrity)) $($problems -join '; ')"
    Write-Host "Evidence: $evidence"
}
# Only a session whose launches all passed and whose transaction completed with verified restoration exits 0.
if ($null -eq $verdict) { exit 1 }
exit $verdict.ExitCode
