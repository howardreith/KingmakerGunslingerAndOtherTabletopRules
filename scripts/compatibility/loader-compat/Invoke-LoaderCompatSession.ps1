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

Safety (adapted from the Kingmaker Cutscene Skip lab harness): refuses while
Kingmaker runs, another lab holds a runtime lock, a journal is pending or Steam
Cloud sync is not proven disabled; refuses any copy outside the lab's
game-copies root; hashes the owner's saves and loader/mod files before and
after; backs up and exactly restores the game's registry settings (journalled,
-RecoverOnly replays it); stops only a process it started from the copy.
#>
[CmdletBinding()]
param(
    [string]$SessionFile,
    [ValidateSet('umm-0.31.1', 'umm-0.32.4', 'owner-0.33.0')][string]$LoaderProfile,
    [string]$CandidateZip,
    [string]$HarnessDir,
    [string]$GameCopy = 'C:\Dev\KingmakerGunslingerLab\game-copies\loader-compat',
    [string]$FixtureDir = 'C:\Dev\KingmakerSkipCinematicLab\fixtures',
    [int]$LaunchTimeoutSeconds = 900,
    [switch]$RecoverOnly
)

. (Join-Path $PSScriptRoot 'LoaderCompat.Common.ps1')

if ($RecoverOnly) {
    if (-not (Test-Path -LiteralPath $script:LcJournal)) { Write-Host 'No interrupted session journal.'; return }
    if (Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) { throw 'Kingmaker is running; recover after it exits.' }
    $journal = Get-Content -LiteralPath $script:LcJournal -Raw | ConvertFrom-Json
    if ($journal.registryBackup) { Restore-LcRegistry $journal.registryBackup }
    Remove-Item -LiteralPath $script:LcJournal -Force
    Exit-LcLock
    Write-Host "Recovered interrupted session $($journal.sessionId)."
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

Assert-LcPreflight
Enter-LcLock $sessionId
New-Item -ItemType Directory -Force -Path $evidence, (Join-Path $profileDir 'Saved Games') | Out-Null
$summary = [ordered]@{ sessionId = $sessionId; session = $session.name; loaderProfile = $LoaderProfile
    candidateZip = $CandidateZip; candidateZipSha256 = Get-LcSha256 $CandidateZip; gameCopy = $copyRoot
    git = (& git -C (Split-Path (Split-Path (Split-Path $PSScriptRoot))) rev-parse HEAD); launches = @(); status = 'started' }
$journal = [ordered]@{ sessionId = $sessionId; registryBackup = $null }
$savesBefore = $null; $installBefore = $null
try {
    $savesBefore = Get-LcOwnerSavesManifest
    $installBefore = Get-LcOwnerInstallManifest
    $summary.ownerSaveFiles = $savesBefore.Count
    Write-LcJson $script:LcJournal $journal
    $registryBackup = Join-Path $evidence 'game-settings.reg'
    Backup-LcRegistry $registryBackup
    $journal.registryBackup = $registryBackup
    Write-LcJson $script:LcJournal $journal

    Initialize-LcGameCopy $paths
    $summary.loaderInstall = Install-LcLoaderProfile $paths $LoaderProfile
    $summary.stagedMods = Set-LcGameCopyMods $paths @($session.ownerMods) $CandidateZip $HarnessDir
    # The product's guarded request must name the staged candidate's own version.
    $candidateVersion = [string](Get-Content -LiteralPath (Join-Path $paths.ModsDir 'KingmakerGunslinger\Info.json') -Raw | ConvertFrom-Json).Version
    $summary.candidateVersion = $candidateVersion
    foreach ($name in @($session.fixtureSaves | Where-Object { $_ })) {
        Copy-Item -LiteralPath (Join-Path $FixtureDir $name) -Destination (Join-Path $profileDir "Saved Games\$name")
    }
    $expectedUmm = $summary.loaderInstall.files['UnityModManager.dll']
    $expectedHarmony2 = $summary.loaderInstall.files['0Harmony.dll']
    $expectedHarmony12 = $summary.loaderInstall.files['0Harmony12.dll']
    $expectedProduct = $summary.stagedMods['KingmakerGunslinger'].dllSha256

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
    $summary.status = $(if (@($summary.launches | Where-Object { $_.verdict -ne 'PASS' }).Count -eq 0) { 'PASS' } else { 'FAIL' })
}
catch {
    $summary.status = 'host-error'
    $summary.failure = $_.ToString()
    throw
}
finally {
    $integrity = @()
    $deadline = (Get-Date).AddSeconds(60)
    while ((Get-Process -Name Kingmaker -ErrorAction SilentlyContinue) -and (Get-Date) -lt $deadline) { Start-Sleep -Seconds 1 }
    try {
        if ($journal.registryBackup) { Restore-LcRegistry $journal.registryBackup }
        $summary.registryRestored = $true
        if (Test-Path -LiteralPath $script:LcJournal) { Remove-Item -LiteralPath $script:LcJournal -Force }
    } catch { $summary.registryRestored = $false; $integrity += "registry restoration failed: $($_.Exception.Message)" }
    if ($null -ne $savesBefore) {
        $diff = Compare-LcManifest $savesBefore (Get-LcOwnerSavesManifest)
        $summary.ownerSavesUnchanged = ($diff.Count -eq 0)
        if ($diff.Count) { $summary.ownerSaveChanges = $diff; $integrity += 'owner saves changed' }
    }
    if ($null -ne $installBefore) {
        $diff = Compare-LcManifest $installBefore (Get-LcOwnerInstallManifest)
        $summary.ownerInstallUnchanged = ($diff.Count -eq 0)
        if ($diff.Count) { $summary.ownerInstallChanges = $diff; $integrity += 'owner installation changed' }
    }
    if ($integrity.Count) { $summary.status = 'integrity-failed'; $summary.integrityFailures = $integrity }
    Write-LcJson (Join-Path $evidence 'session-summary.json') $summary
    if (-not (Test-Path -LiteralPath $script:LcJournal)) { Exit-LcLock }
    Write-Host "Session $sessionId : $($summary.status) $($integrity -join '; ')"
    Write-Host "Evidence: $evidence"
}
