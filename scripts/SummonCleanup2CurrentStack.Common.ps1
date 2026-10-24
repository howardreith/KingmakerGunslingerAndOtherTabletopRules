# Mission-owned orchestration. No new native loader, save writer or profile.
Set-StrictMode -Version Latest

function New-KmgSummonCleanup2Plan {
    param([string]$RepositoryRoot,[string]$PackagePath,[string]$ExpectedVersion,
        [string]$DevelopmentPurpose,[string]$SaveName,[object[]]$CreatureKeys,
        [int]$TimeoutSeconds=900)
    if($DevelopmentPurpose -cnotin @('summon-cleanup-iteration2','summon-cleanup-iteration2-baseline')) {
        throw 'Only the two explicit Cleanup 2 development purposes are admitted.'
    }
    if($SaveName -cne 'KMG_AUTOMATION_WORKING') { throw 'Exact KMG_AUTOMATION_WORKING is required.' }
    $allowed=@('giant-crab','griffon','giant-stag-beetle','girallon','ettin','elephant',
        'stegosaurus','hill-giant','stone-giant','fire-giant','cloud-giant',
        'storm-giant','brachiosaurus','gorgon','deinonychus')
    if(@($CreatureKeys | Where-Object {$_ -is [string] -and $_ -ceq 'frost-giant'}).Count -gt 0) {
        throw "frost-giant is a retained native option, not a generated creature-review key. Use disposable-expanded-summoning-cleanup2-native-review with reviewPhase=giant-lineup for the existing Nature's Ally VII and Summon Monster VIII Frost routes."
    }
    if($null -eq $CreatureKeys -or $CreatureKeys.Count -lt 1 -or $CreatureKeys.Count -gt $allowed.Count) {
        throw 'An explicit nonempty bounded creature selection is required.'
    }
    $seen=New-Object 'Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
    foreach($key in $CreatureKeys) {
        if($key -isnot [string] -or $key -cnotin $allowed -or -not $seen.Add($key)) {
            throw 'Creature keys must be exact unique mission-owned strings.'
        }
    }
    if($TimeoutSeconds -lt 30 -or $TimeoutSeconds -gt 900) { throw 'Cleanup review timeout must be 30-900 seconds.' }
    Assert-KmgNotRunning
    $candidate=Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $RepositoryRoot -DevelopmentPurpose $DevelopmentPurpose
    if($candidate.candidateKind -cne 'development' -or $candidate.releaseQualification -ne $false -or
        $candidate.purpose -cne $DevelopmentPurpose -or [string]$candidate.packageSha256 -cnotmatch '^[0-9a-f]{64}$' -or
        [string]$candidate.dllSha256 -cnotmatch '^[0-9a-f]{64}$' -or [string]::IsNullOrWhiteSpace([string]$candidate.dllMvid)) {
        throw 'Cleanup review requires an explicit validated development artifact identity.'
    }
    Assert-KmgDevelopmentSourceBinding -Receipt $candidate -Git (Get-KmgGitState -RepositoryRoot $RepositoryRoot) `
        -SourceStateSha256 (Get-KmgSourceStateFingerprint -RepositoryRoot $RepositoryRoot) -ExpectedVersion $ExpectedVersion
    return [pscustomobject]@{repositoryRoot=$RepositoryRoot;candidate=$candidate;expectedVersion=$ExpectedVersion;
        developmentPurpose=$DevelopmentPurpose;saveName=$SaveName;creatureKeys=@($CreatureKeys);timeoutSeconds=$TimeoutSeconds;
        scenario='working-save-expanded-summoning-creature-review'}
}

function Get-KmgSummonCleanup2Inventory {
    param([string]$ModsPath,[string]$BackupPath)
    if(-not(Test-Path -LiteralPath $ModsPath -PathType Container)) { throw 'Current Mods intake is missing.' }
    $items=@(Get-Item -LiteralPath $ModsPath -Force)+@(Get-ChildItem -LiteralPath $ModsPath -Force -Recurse)
    if(@($items | Where-Object {($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0}).Count) {
        throw 'Current Mods intake contains a reparse point.'
    }
    if(Test-Path -LiteralPath (Join-Path $ModsPath '.kmg-compat-sentinel.json')) { throw 'Current Mods already has a transaction sentinel.' }
    $ids=New-Object 'Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
    $records=@(foreach($directory in @(Get-ChildItem -LiteralPath $ModsPath -Directory -Force | Sort-Object Name)) {
        $infoPath=Join-Path $directory.FullName 'Info.json'
        $info=Read-KmgCompatibilityJson $infoPath
        if($info.Id -isnot [string] -or [string]::IsNullOrWhiteSpace($info.Id) -or
            $info.Version -isnot [string] -or [string]::IsNullOrWhiteSpace($info.Version) -or -not $ids.Add($info.Id)) {
            throw 'Every current mod needs a unique identifiable ID and nonempty version.'
        }
        [pscustomobject]@{destinationName=$directory.Name;sourceDirectory=(Join-Path $BackupPath $directory.Name);
            sourceManifest=@(Get-KmgCompatibilityDirectoryManifest $directory.FullName);id=$info.Id;version=$info.Version}
    })
    $kmg=@($records | Where-Object {$_.id -ceq 'KingmakerGunslinger' -and $_.destinationName -ceq 'KingmakerGunslinger'})
    if($kmg.Count -ne 1) { throw 'Current intake must identify exactly one KingmakerGunslinger directory and ID.' }
    return $records
}

function Assert-KmgSummonCleanup2SaveSelection {
    param($Catalog)
    # Filesystem intake is only an early ambiguity guard. The qualified native
    # loader still proves the complete exact captured SaveInfo identity.
    $working=@($Catalog.Files | Where-Object {[IO.Path]::GetFileName($_.path) -cmatch '^Manual_[0-9]+_KMG_AUTOMATION_WORKING\.zks$'})
    $baseline=@($Catalog.Files | Where-Object {[IO.Path]::GetFileName($_.path) -cmatch '^Manual_[0-9]+_KMG_AUTOMATION_BASELINE\.zks$'})
    if($working.Count -ne 1 -or $baseline.Count -ne 1) { throw 'Working/baseline save intake is missing or ambiguous; no save is selected.' }
    return [pscustomobject]@{working=$working[0];baseline=$baseline[0]}
}

# External boundaries are deliberately small functions. Offline tests replace
# these boundaries while exercising this same orchestration and actual helpers.
function Open-KmgSummonCleanup2Settings {
    param([string]$KeyPath,[bool]$Writable)
    $key=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($KeyPath,$Writable)
    if($null -eq $key) { throw 'Current native game-settings key is missing.' }
    return $key
}
function Get-KmgSummonCleanup2NativeProcessIds {
    # Unlike Get-Process -ErrorAction SilentlyContinue, this API returns an
    # empty array for ordinary absence and throws on enumeration failure.
    # Unknown process state must never authorize guard disposal.
    $processes=[Diagnostics.Process]::GetProcessesByName('Kingmaker')
    try {return @($processes | ForEach-Object Id)}
    finally {foreach($process in $processes){$process.Dispose()}}
}
function Assert-KmgSummonCleanup2NativeExit {
    $ids=@(Get-KmgSummonCleanup2NativeProcessIds)
    if($ids.Count -gt 0){throw ('Kingmaker remains running: '+($ids -join ', '))}
}
function Wait-KmgSummonCleanup2Exit {
    $deadline=[DateTime]::UtcNow.AddSeconds(180)
    while(@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -gt 0 -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 500 }
    Assert-KmgSummonCleanup2NativeExit
}

function Wait-KmgSummonCleanup2ProtectedExit {
    param([Parameter(Mandatory=$true)]$Record,[Parameter(Mandatory=$true)][string]$RecordPath,
        [Parameter(Mandatory=$true)]$ExitFailure)
    # This process owns the live OS handles. A status file or a script variable
    # cannot outlive powershell.exe -File. Do not unwind to an unhandled error
    # until actual process absence has been confirmed, even when evidence IO is
    # the original failure. No settings/install restoration occurs in this loop.
    $guard=[ordered]@{ownerProcessId=$PID;status='WAITING_FOR_NATIVE_EXIT';
        startedUtc=[DateTime]::UtcNow.ToString('o');confirmedExitUtc=$null;
        lastEvidenceError=$null;lastProcessError=$null;reason=$ExitFailure.Exception.Message}
    $Record.protectedExitGuard=$guard
    $Record.status='FAILED'
    $Record.restoration=[ordered]@{verified=$false;status='WAITING_FOR_NATIVE_EXIT';error=$ExitFailure.Exception.Message}
    $nextEvidence=[DateTime]::MinValue
    while($true) {
        $absent=$false
        try {Assert-KmgSummonCleanup2NativeExit;$absent=$true} catch { $absent=$false;$guard.lastProcessError=$_.Exception.Message }
        if($absent) {
            $guard.status='EXIT_CONFIRMED'
            $guard.confirmedExitUtc=[DateTime]::UtcNow.ToString('o')
            return
        }
        if([DateTime]::UtcNow -ge $nextEvidence) {
            try {Write-KmgCompatibilityJsonAtomic $RecordPath $Record}
            catch {$guard.lastEvidenceError=$_.Exception.Message}
            $nextEvidence=[DateTime]::UtcNow.AddSeconds(5)
        }
        [Threading.Thread]::Sleep(250)
    }
}
function Invoke-KmgSummonCleanup2Deploy {
    param($Plan,$Lease,[string]$ScriptsDirectory)
    & (Join-Path $ScriptsDirectory 'Deploy-Local.ps1') -PackagePath $Plan.candidate.packagePath `
        -DevelopmentPurpose $Plan.developmentPurpose -RuntimeLease $Lease -PassThru -Confirm:$false
}
function Invoke-KmgSummonCleanup2Launch {
    param($Plan,$Lease,[string]$Deployment,[string]$ScriptsDirectory)
    & (Join-Path $ScriptsDirectory 'Invoke-KingmakerRuntimeTest.ps1') -Scenario $Plan.scenario `
        -ExpectedVersion $Plan.expectedVersion -SaveName $Plan.saveName -Parameters @{creatures=($Plan.creatureKeys -join ',')} `
        -TimeoutSeconds $Plan.timeoutSeconds -ObserverStartupTimeoutSeconds 600 -CompletionTimeoutSeconds $Plan.timeoutSeconds `
        -FingerprintTimeoutSeconds 600 -ExitAfterCompletion:$true -RuntimeLease $Lease -ReuseInstalledArtifact `
        -DevelopmentPurpose $Plan.developmentPurpose -PackagePath $Plan.candidate.packagePath `
        -DeploymentManifestPath $Deployment -PassThru -Confirm:$false
}

function Invoke-KmgSummonCleanup2Transaction {
    param([Parameter(Mandatory=$true)]$Plan,[Parameter(Mandatory=$true)]$Environment)
    # Environment is constructed by the public wrapper; no public path/profile
    # or fixture override is exposed. FixtureMode is used only by offline tests.
    Assert-KmgNotRunning
    $runId='cleanup2-stack-'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,12)
    $mods=Join-Path $Environment.install 'Mods'
    $backup=Join-Path $Environment.install ("Mods.kmg-compat-$runId.original")
    $evidence=Join-Path $Environment.evidenceRoot $runId
    try {
        $inventory=@(Get-KmgSummonCleanup2Inventory -ModsPath $mods -BackupPath $backup)
        $originalManifest=@(Get-KmgCompatibilityDirectoryManifest $mods)
        $rootFiles=@(Get-ChildItem -LiteralPath $mods -File -Force | ForEach-Object Name)
    } catch {
        Write-KmgCompatibilityJsonAtomic (Join-Path $evidence 'installed-stack.json') ([ordered]@{
            schemaVersion=1;transactionRunId=$runId;scenario=$Plan.scenario;status='REFUSED_BEFORE_ENTRY';
            candidate=$Plan.candidate;creatureKeys=@($Plan.creatureKeys);error=$_.Exception.Message;
            restoration=[ordered]@{status='NOT_ENTERED';installationMutation=$false;settingsMutation=$false}})
        throw
    }
    $record=[ordered]@{schemaVersion=1;transactionRunId=$runId;scenario=$Plan.scenario;status='PREFLIGHT';
        candidateKind='development';releaseQualification=$false;developmentPurpose=$Plan.developmentPurpose;
        candidate=$Plan.candidate;creatureKeys=@($Plan.creatureKeys);actualInstalledMods=$inventory;
        originalManifest=$originalManifest;settingsSnapshot=$null;saveIntake=$null;launchReceipt=$null;
        error=$null;restoration=$null;startedUtc=[DateTime]::UtcNow.ToString('o')}
    $catalog=$null;$lease=$null;$entered=$false;$failure=$null;$exitFailure=$null;$receipt=$null;$settingsCaptured=$false;$nativeAttempted=$false
    try {
        $settings=Open-KmgSummonCleanup2Settings $Environment.settingsKey $false
        try { $settingsSnapshot=@(Get-KmgFlatGameSettingsValues $settings) } finally {$settings.Dispose()}
        $settingsCaptured=$true
        $record.settingsSnapshot=$settingsSnapshot
        $catalog=Open-KmgProtectedSaveCatalog -SaveDirectory $Environment.saveDirectory -EvidenceDirectory $evidence
        $record.saveIntake=Assert-KmgSummonCleanup2SaveSelection $catalog
        [void](Assert-KmgProtectedSaveCatalog $catalog)
        Write-KmgCompatibilityJsonAtomic (Join-Path $evidence 'installed-stack.json') $record
        # Refuse an intake that changed between inventory and lease acquisition.
        if(-not(Test-KmgCompatibilityManifestEqual $originalManifest @(Get-KmgCompatibilityDirectoryManifest $mods))) { throw 'Current Mods changed after intake.' }
        $resolution=[pscustomobject]@{profileId='summon-cleanup2-exact-current-intake';runtimeCapable=$true;
            gunslinger=[pscustomobject]@{packagePath=$Plan.candidate.packagePath};
            runtimeMods=@($inventory | Where-Object {$_.id -cne 'KingmakerGunslinger'})}
        $transaction=Enter-KmgCompatibilityTransaction -Resolution $resolution -KingmakerInstallDir $Environment.install `
            -StateRoot $Environment.stateRoot -RunId $runId -FixtureMode:$Environment.fixtureMode
        $entered=$true
        $lease=Open-KmgCompatibilityRuntimeLease $runId $Environment.stateRoot
        Assert-KmgRuntimeLease $lease
        if(-not(Test-KmgCompatibilityManifestEqual $originalManifest @($transaction.originalManifest))) { throw 'Transaction intake differs from inventoried current Mods.' }
        foreach($file in $rootFiles) { Copy-Item -LiteralPath (Join-Path $backup $file) -Destination (Join-Path $mods $file) }
        $originalSettings=Join-Path $backup 'KingmakerGunslinger/FeatureModules.json'
        if(-not(Test-Path -LiteralPath $originalSettings -PathType Leaf)) { throw 'Actual current module settings are missing.' }
        Copy-Item -LiteralPath $originalSettings -Destination (Join-Path $mods 'KingmakerGunslinger/FeatureModules.json') -Force
        $record.status='ACTIVE'
        Write-KmgCompatibilityJsonAtomic (Join-Path $evidence 'installed-stack.json') $record
        Assert-KmgRuntimeLease $lease
        $deployment=Invoke-KmgSummonCleanup2Deploy $Plan $lease $Environment.scriptsDirectory
        Assert-KmgRuntimeLease $lease
        $settings=Open-KmgSummonCleanup2Settings $Environment.settingsKey $false
        try {$beforeLaunch=@(Get-KmgFlatGameSettingsValues $settings)} finally {$settings.Dispose()}
        if(($beforeLaunch | ConvertTo-Json -Depth 12 -Compress) -cne ($settingsSnapshot | ConvertTo-Json -Depth 12 -Compress)) {
            throw 'Native game settings changed after intake; foreign mutation refused before launch.'
        }
        $nativeAttempted=$true
        $output=@(Invoke-KmgSummonCleanup2Launch $Plan $lease $deployment $Environment.scriptsDirectory)
        $receipt=Get-KmgExactRuntimeLaunchReceipt -Output $output
        $record.launchReceipt=$receipt
        $request=Read-KmgCompatibilityJson (Join-Path $receipt.evidenceDirectory 'runtime-request.json')
        $result=Read-KmgCompatibilityJson (Join-Path $receipt.evidenceDirectory 'runtime-result.json')
        if($request.scenario -cne $Plan.scenario -or $result.scenario -cne $Plan.scenario -or $result.status -cne 'PASS' -or
            $request.expectedModVersion -cne $Plan.expectedVersion -or $request.exitAfterCompletion -ne $true -or
            $request.parameters.saveName -cne $Plan.saveName -or $request.parameters.creatures -cne ($Plan.creatureKeys -join ',')) {
            throw 'Native receipt does not prove the exact requested current candidate/creature review.'
        }
        Assert-KmgRuntimeLease $lease
        $record.status='OBSERVED'
    } catch { $failure=$_;$record.error=$_.Exception.Message;$record.status='FAILED' }
    finally {
        if($entered) {
            try {Wait-KmgSummonCleanup2Exit}
            catch {
                $exitFailure=$_
                if(-not $failure){$failure=$exitFailure}
                Wait-KmgSummonCleanup2ProtectedExit -Record $record -RecordPath (Join-Path $evidence 'installed-stack.json') -ExitFailure $exitFailure
                # Only the confirmed native exit above permits restoration and
                # eventual disposal. Preserve the original failure verdict.
                $exitFailure=$null
            }
        }
        if($null -eq $exitFailure) {
            # Settings and save preservation share one independent boundary;
            # their own checks are also independent so one failure cannot skip
            # the other, or skip installation restoration.
            if($lease) {$lease.Stream.Dispose()}
            $boundary=Invoke-KmgIndependentBoundaryRestoration -RestoreSettings {
                $settingsError=$null
                try {
                    if($entered -and $nativeAttempted -and $settingsCaptured) {
                        $key=Open-KmgSummonCleanup2Settings $Environment.settingsKey $true
                        try {Restore-KmgFlatGameSettingsValues $key $settingsSnapshot} finally {$key.Dispose()}
                    } elseif($settingsCaptured) {
                        # No native child was attempted: do not overwrite a
                        # different owner's settings while refusing preflight.
                        $key=Open-KmgSummonCleanup2Settings $Environment.settingsKey $false
                        try {$currentSettings=@(Get-KmgFlatGameSettingsValues $key)} finally {$key.Dispose()}
                        if(($currentSettings | ConvertTo-Json -Depth 12 -Compress) -cne ($settingsSnapshot | ConvertTo-Json -Depth 12 -Compress)) {
                            throw 'Settings preservation differs before an owned native launch; foreign values retained for review.'
                        }
                    }
                } catch {$settingsError=$_}
                try {if($catalog) {[void](Assert-KmgProtectedSaveCatalog $catalog)}} catch {if(-not $settingsError){$settingsError=$_}}
                if($settingsError) {throw $settingsError}
            } -RestoreInstallation {
                if($entered) {
                    if(-not(Test-KmgCompatibilityManifestEqual $originalManifest @(Get-KmgCompatibilityDirectoryManifest $backup))) { throw 'Protected original Mods intake was changed; restoration refused before moving it.' }
                    Restore-KmgCompatibilityTransaction -RunId $runId -StateRoot $Environment.stateRoot -FixtureMode:$Environment.fixtureMode
                }
            }
            if($catalog) {Close-KmgProtectedSaveCatalog $catalog}
            $verified=(-not $entered -or ($null -ne $boundary.transaction -and $boundary.transaction.restorationVerified))
            $record.restoration=[ordered]@{verified=$verified;settingsAndSavesExact=($null -eq $boundary.settingsError);
                settingsError=$(if($boundary.settingsError){$boundary.settingsError.Exception.Message}else{$null});
                installationError=$(if($boundary.installationError){$boundary.installationError.Exception.Message}else{$null})}
            if($boundary.installationError) {$failure=$boundary.installationError}
            elseif($boundary.settingsError) {$failure=$boundary.settingsError}
            elseif(-not $verified) {$failure=[Management.Automation.ErrorRecord]::new([Exception]::new('Exact installation restoration was not verified.'),'cleanup2-restore','InvalidResult',$null)}
        }
        $record.completedUtc=[DateTime]::UtcNow.ToString('o')
        if($failure){$record.status='FAILED'}else{$record.status='RESTORED'}
        Write-KmgCompatibilityJsonAtomic (Join-Path $evidence 'installed-stack.json') $record
    }
    if($failure){throw $failure}
    return $receipt
}
