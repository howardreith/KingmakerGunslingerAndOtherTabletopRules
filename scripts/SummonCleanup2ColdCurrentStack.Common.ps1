Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'SummonCleanup2ColdSave.Common.ps1')
. (Join-Path $PSScriptRoot 'SummonCleanup2ColdMetadataRecovery.Common.ps1')

function Get-KmgCleanup2ColdEnvironment {
    return [pscustomobject]@{install='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker';
        stateRoot='C:/Dev/KingmakerGunslingerLab/compatibility-state';evidenceRoot='C:/Dev/KingmakerGunslingerLab/runtime-evidence';
        saveDirectory=(Join-Path (Split-Path -Parent ([Environment]::GetFolderPath('LocalApplicationData'))) 'LocalLow/Owlcat Games/Pathfinder Kingmaker/Saved Games');
        settingsKey='Software\Owlcat Games\Pathfinder Kingmaker';scriptsDirectory=$PSScriptRoot;fixtureMode=$false}
}

function New-KmgCleanup2ColdCurrentStackPlan {
    param([string]$RepositoryRoot,[string]$PackagePath,[string]$ExpectedVersion,[string]$DevelopmentPurpose,
        [string]$SaveName,[string]$PersistenceScope,[int]$TimeoutSeconds=600)
    Assert-KmgCleanup2EttinColdPlanScope $PersistenceScope $DevelopmentPurpose $SaveName $ExpectedVersion
    if($TimeoutSeconds -lt 30 -or $TimeoutSeconds -gt 900){throw 'Cold phase timeout must be 30-900 seconds.'}
    Assert-KmgSummonCleanup2NativeExit
    $candidate=Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $RepositoryRoot -DevelopmentPurpose $DevelopmentPurpose
    if($candidate.candidateKind -cne 'development' -or $candidate.releaseQualification -ne $false -or
        $candidate.purpose -cne 'summon-cleanup-iteration2' -or $candidate.version -cne '0.0.163' -or
        $candidate.packageSha256 -cnotmatch '^[0-9a-f]{64}$' -or $candidate.dllSha256 -cnotmatch '^[0-9a-f]{64}$' -or
        [string]::IsNullOrWhiteSpace($candidate.dllMvid)){throw 'Cold trio requires the exact validated Cleanup2 development candidate.'}
    Assert-KmgDevelopmentSourceBinding -Receipt $candidate -Git (Get-KmgGitState -RepositoryRoot $RepositoryRoot) `
        -SourceStateSha256 (Get-KmgSourceStateFingerprint -RepositoryRoot $RepositoryRoot) -ExpectedVersion $ExpectedVersion
    return [pscustomobject]@{repositoryRoot=$RepositoryRoot;candidate=$candidate;expectedVersion=$ExpectedVersion;
        developmentPurpose=$DevelopmentPurpose;saveName=$SaveName;persistenceScope=$PersistenceScope;timeoutSeconds=$TimeoutSeconds;
        phases=@('working-save-expanded-summoning-prepare','working-save-expanded-summoning-verify-cleanup','working-save-expanded-summoning-verify-absent')}
}

function Write-KmgCleanup2ColdJournal([string]$Path,$Record) {
    $bytes=[Text.UTF8Encoding]::new($false).GetBytes(($Record|ConvertTo-Json -Depth 20))
    Write-KmgRuntimeAtomicBytes -Path $Path -Bytes $bytes -ExpectedSha256 $(if(Test-Path -LiteralPath $Path){Get-KmgCompatibilitySha256 $Path}else{$null})
}

function Write-KmgCleanup2ColdOwnershipNew([string]$Path,$Value) {
    $bytes=[Text.UTF8Encoding]::new($false).GetBytes(($Value|ConvertTo-Json -Depth 20))
    $stream=[IO.File]::Open($Path,[IO.FileMode]::CreateNew,[IO.FileAccess]::ReadWrite,[IO.FileShare]::Read)
    try {
        $stream.Write($bytes,0,$bytes.Length);$stream.Flush($true);$stream.Position=0
        $sha=[Security.Cryptography.SHA256]::Create()
        try {$hash=[BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','').ToLowerInvariant()}finally{$sha.Dispose();$stream.Position=0}
        return [pscustomobject]@{lease=$stream;sha256=$hash}
    } catch {$stream.Dispose();throw}
}

function Assert-KmgCleanup2ColdFinalRestoration($Ownership,$Context,$Record,$Environment) {
    Assert-KmgSummonCleanup2NativeExit
    $errors=New-Object 'Collections.Generic.List[string]'
    if($Record.workingRestored) {
        try {Assert-KmgCleanup2ColdRestoredWorking $Context}
        catch {$Record.workingRestored=$false;$errors.Add('final-working: '+$_.Exception.Message)}
    }
    try {Assert-KmgCleanup2ColdCatalog $Context}
    catch {$errors.Add('final-protected-catalog: '+$_.Exception.Message)}
    if($Ownership -and $Environment -and $Ownership.kind -ceq 'Cleanup2ColdOriginalWorking') {
        if($Record.settingsRestored) {
            try {
                $settings=Open-KmgSummonCleanup2Settings $Environment.settingsKey $false
                try {$current=@(Get-KmgFlatGameSettingsValues $settings)}finally{$settings.Dispose()}
                if(($current|ConvertTo-Json -Depth 12 -Compress) -cne (@($Ownership.settingsSnapshot)|ConvertTo-Json -Depth 12 -Compress)) {
                    throw 'Original settings changed after restoration; foreign values are preserved.'
                }
            } catch {$Record.settingsRestored=$false;$errors.Add('final-settings: '+$_.Exception.Message)}
        }
        if($Record.installationRestored) {
            try {
                if(-not(Test-KmgCompatibilityManifestEqual @($Ownership.originalManifest) @(Get-KmgCompatibilityDirectoryManifest (Join-Path $Environment.install 'Mods')))) {
                    throw 'Original installation changed after restoration; foreign files are preserved.'
                }
            } catch {$Record.installationRestored=$false;$errors.Add('final-installation: '+$_.Exception.Message)}
        }
    }
    if($errors.Count) {
        $Record.finalBoundaryDebt=$true;$Record.recoveryRequired=$true;$Record.status='RECOVERY_REQUIRED'
        $Record.restorationErrors=@($Record.restorationErrors)+@($errors.ToArray())
        throw ($errors -join '; ')
    }
    $metadataOnly=$Record -is [Collections.IDictionary] -and $Record.Contains('metadataOnlyRecovery') -and $Record.metadataOnlyRecovery
    if($Record.workingRestored -and ($metadataOnly -or ($Record.settingsRestored -and $Record.installationRestored))) {
        $Record.finalBoundaryDebt=$false
    }
}

function Wait-KmgCleanup2ColdHandleRelease($Record,[string]$JournalPath,$Lease,$Context=$null,$Ownership=$null,$Environment=$null) {
    if($Record -isnot [Collections.IDictionary]) {
        foreach($name in @('protectedExitGuard','restoration')) {
            if(-not $Record.PSObject.Properties[$name]){$Record|Add-Member NoteProperty $name $null}
        }
    }
    $failure=$null
    while($true) {
        try {
            Assert-KmgSummonCleanup2NativeExit
            break
        }
        catch {
            $exitFailure=$_
            if(-not $failure){$failure=$exitFailure}
            if(-not $Record.error){$Record.error=$exitFailure.Exception.Message}
            # Generic restore needs the caller's read stream closed. If it
            # refused after that close, recover our same persistent lock before
            # waiting; never overwrite a foreign or already discharged lock.
            if($Lease -and -not $Lease.Stream.CanRead -and (Test-Path -LiteralPath $Lease.LockPath)) {
                try {
                    if((Get-Content -LiteralPath $Lease.LockPath -Raw).Trim() -cne $Lease.RunId){throw 'Cold release found a foreign lock owner.'}
                    $Lease.Stream=[IO.File]::Open($Lease.LockPath,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
                } catch {$Record.restorationErrors=@($Record.restorationErrors)+@($_.Exception.Message)}
            }
            Wait-KmgSummonCleanup2ProtectedExit -Record $Record -RecordPath $JournalPath -ExitFailure $exitFailure
        }
    }
    if($Context) {
        try {Assert-KmgCleanup2ColdFinalRestoration $Ownership $Context $Record $Environment}
        catch {if(-not $failure){$failure=$_};if(-not $Record.error){$Record.error=$_.Exception.Message}}
    }
    if($failure) {
        if(-not $Record.error){$Record.error=$failure.Exception.Message}
        $metadataOnly=($Record -is [Collections.IDictionary] -and $Record.Contains('metadataOnlyRecovery') -and $Record.metadataOnlyRecovery)
        $finalDebt=$Record -is [Collections.IDictionary] -and $Record.Contains('finalBoundaryDebt') -and $Record.finalBoundaryDebt
        $Record.recoveryRequired=$finalDebt -or (-not ($Record.workingRestored -and ($metadataOnly -or ($Record.settingsRestored -and $Record.installationRestored))))
        $Record.status=if($Record.recoveryRequired){'RECOVERY_REQUIRED'}else{'FAILED_RESTORED'}
    }
    $finalGuard=$Lease -and $Lease.PSObject.Properties['FinalRestoredGuard'] -and $Lease.FinalRestoredGuard
    if($finalGuard -and -not $Record.recoveryRequired) {
        try {
            Assert-KmgSummonCleanup2NativeExit;Assert-KmgRuntimeLease $Lease
            $Lease.Stream.Dispose();Remove-KmgCompatibilityOwnedLock $Lease.LockPath $Lease.RunId
            $Lease.FinalRestoredGuard=$false;$Record.finalBoundaryDebt=$false
        } catch {
            if(-not $failure){$failure=$_};if(-not $Record.error){$Record.error=$_.Exception.Message}
            $Record.finalBoundaryDebt=$true;$Record.recoveryRequired=$true;$Record.status='RECOVERY_REQUIRED'
            $Record.restorationErrors=@($Record.restorationErrors)+@($_.Exception.Message)
        }
    }
    if($failure -or $finalGuard) {
        try {Write-KmgCleanup2ColdJournal $JournalPath $Record}
        catch {$Record.restorationErrors=@($Record.restorationErrors)+@($_.Exception.Message);if(-not $failure){$failure=$_}}
    }
    return $failure
}

function Invoke-KmgCleanup2ColdLaunch($Plan,[string]$Scenario,$Lease,[string]$Deployment,[string]$ScriptsDirectory) {
    & (Join-Path $ScriptsDirectory 'Invoke-KingmakerRuntimeTest.ps1') -Scenario $Scenario -ExpectedVersion $Plan.expectedVersion `
        -SaveName $Plan.saveName -Parameters @{persistenceScope=$Plan.persistenceScope} -TimeoutSeconds $Plan.timeoutSeconds `
        -ObserverStartupTimeoutSeconds $Plan.timeoutSeconds -CompletionTimeoutSeconds $Plan.timeoutSeconds `
        -FingerprintTimeoutSeconds $Plan.timeoutSeconds -ExitAfterCompletion:$true -RuntimeLease $Lease -ReuseInstalledArtifact `
        -DevelopmentPurpose $Plan.developmentPurpose -PackagePath $Plan.candidate.packagePath -DeploymentManifestPath $Deployment -PassThru -Confirm:$false
}

function Assert-KmgCleanup2ColdReceipt($Plan,[string]$Scenario,$Receipt,[object[]]$PriorPhases) {
    $q=Read-KmgCompatibilityJson (Join-Path $Receipt.evidenceDirectory 'runtime-request.json')
    $r=Read-KmgCompatibilityJson (Join-Path $Receipt.evidenceDirectory 'runtime-result.json')
    $id=Read-KmgCompatibilityJson (Join-Path $Receipt.evidenceDirectory 'runtime-loaded-build-identity.json')
    $writes=if($Scenario -ceq 'working-save-expanded-summoning-verify-absent'){0}else{1}
    if($q.runId -cne $Receipt.runId -or $r.runId -cne $Receipt.runId -or $q.scenario -cne $Scenario -or $r.scenario -cne $Scenario -or
        $r.status -cne 'PASS' -or $q.expectedModVersion -cne $Plan.expectedVersion -or $r.loadedModVersion -cne $Plan.expectedVersion -or
        $q.exitAfterCompletion -ne $true -or @($q.parameters.PSObject.Properties).Count -ne 2 -or
        $q.parameters.saveName -cne 'KMG_AUTOMATION_WORKING' -or $q.parameters.persistenceScope -cne 'giant-humanoids-ii' -or
        $r.gitCommit -cne $Plan.candidate.commit -or $id.gitCommit -cne $Plan.candidate.commit -or
        $id.loadedModuleSha256 -cne $Plan.candidate.dllSha256 -or $id.moduleVersionId -cne $Plan.candidate.dllMvid -or $id.processId -le 0 -or
        @($PriorPhases|Where-Object {$_.runId -ceq $Receipt.runId -or $_.processId -eq $id.processId}).Count -gt 0 -or
        $r.workingSaveSmoke.workingMatchCount -ne 1 -or $r.workingSaveSmoke.baselineMatchCount -ne 1 -or
        $r.workingSaveSmoke.descriptorReferenceCorrelated -ne $true -or $r.workingSaveSmoke.saveWritingApiObserved -ne $false -or
        $r.workingSaveSmoke.expectedWorkingSaveRoutineCount -ne $writes) {throw 'Cold native receipt does not prove the exact fresh source-bound phase and Working write contract.'}
    if(@($r.assertions|Where-Object {$_.name -ceq 'exact-working-save-write' -and $_.status -ceq 'PASS'}).Count -ne 1) {
        throw 'Cold native exact-working-save-write assertion is missing.'
    }
    if($Scenario -ceq 'working-save-expanded-summoning-verify-cleanup') {
        $head=@($r.assertions|Where-Object {$_.name -ceq 'whole-roster-persistence-ettin-authored-head-cold-verify' -and $_.status -ceq 'PASS'})
        if($head.Count -ne 1){throw 'Exact cold Ettin head/material assertion is missing.'}
        $proof=$head[0].observed|ConvertFrom-Json
        if($proof.moduleEnabled -ne $true -or $proof.phase -cne 'cold-verify' -or $proof.passed -ne $true -or
            $proof.observerCalledAttachOrRebuild -ne $false -or $proof.attachmentInitializationInvoked -ne $false -or
            $proof.exactTwoHeadFullBodyAttachment -ne $true -or $proof.ownedMaterialAndBorrowedNativeTextureSlots -ne $true -or
            $proof.exactDrivenBaselineAndSameNativeFade -ne $true -or $proof.rootCount -ne 1 -or $proof.nativePoseSourceHeadCount -ne 1 -or
            $proof.privateHeadBranchCount -ne 2 -or $proof.authoredRendererCount -ne 1 -or $proof.assignedCount -ne 1 -or
            $proof.ownedTextureCount -ne 0 -or $proof.nativeSourceBodyDisabled -ne $true -or $proof.nativeRendererScopePreserved -ne $true) {
            throw 'Cold Ettin proof does not establish untouched native hydration and exact owned full-body/material state.'}
    }
    return [ordered]@{scenario=$Scenario;runId=$Receipt.runId;processId=$id.processId;evidenceDirectory=$Receipt.evidenceDirectory;
        requestSha256=Get-KmgCompatibilitySha256 (Join-Path $Receipt.evidenceDirectory 'runtime-request.json');
        resultSha256=Get-KmgCompatibilitySha256 (Join-Path $Receipt.evidenceDirectory 'runtime-result.json');
        loadedIdentitySha256=Get-KmgCompatibilitySha256 (Join-Path $Receipt.evidenceDirectory 'runtime-loaded-build-identity.json')}
}

function Restore-KmgCleanup2ColdBoundary($Ownership,$Context,$Record,[string]$JournalPath,$Environment,$Lease) {
    Assert-KmgSummonCleanup2NativeExit
    Assert-KmgRuntimeLease $Lease
    $statePath=Join-Path (Join-Path $Environment.stateRoot $Ownership.runId) 'transaction.json'
    $authority=Read-KmgCompatibilityJson $statePath
    if($authority.runId -cne $Ownership.runId -or $authority.profileId -cne 'cleanup2-cold-exact-current-intake' -or
        $authority.status -ceq 'Restored' -or -not(Test-KmgCompatibilityManifestEqual @($Ownership.originalManifest) @($authority.originalManifest))){throw 'Cold restoration has foreign or discharged transaction authority.'}
    $errors=New-Object 'Collections.Generic.List[string]'
    try {
        if($Record.nativeAttempted){Restore-KmgCleanup2ColdWorking $Context}else{Restore-KmgCleanup2ColdUnwrittenWorking $Context}
        $Record.workingRestored=$true
    } catch {$errors.Add('working: '+$_.Exception.Message)}
    try {Assert-KmgCleanup2ColdCatalog $Context} catch {$errors.Add('protected-catalog: '+$_.Exception.Message)}
    try {
        $settings=Open-KmgSummonCleanup2Settings $Environment.settingsKey ([bool]$Record.nativeAttempted)
        try {
            if($Record.nativeAttempted){Restore-KmgFlatGameSettingsValues $settings @($Ownership.settingsSnapshot)}
            elseif((@(Get-KmgFlatGameSettingsValues $settings)|ConvertTo-Json -Depth 12 -Compress) -cne
                (@($Ownership.settingsSnapshot)|ConvertTo-Json -Depth 12 -Compress)){throw 'Foreign settings changed before an owned native launch.'}
        } finally {$settings.Dispose()}
        $Record.settingsRestored=$true
    } catch {$errors.Add('settings: '+$_.Exception.Message)}
    if($errors.Count -eq 0) {
        try {
            Assert-KmgSummonCleanup2NativeExit
            if($Lease){Assert-KmgRuntimeLease $Lease;$Lease.Stream.Dispose()}
            $state=Read-KmgCompatibilityJson (Join-Path (Join-Path $Environment.stateRoot $Ownership.runId) 'transaction.json')
            # All extra obligations are discharged before the generic restore
            # may release its lock. On debt, leave staging and lock untouched.
            if($state.status -cne 'Restored') {
                if(-not(Test-KmgCompatibilityManifestEqual @($Ownership.originalManifest) @($state.originalManifest))) {throw 'Cold original installation identity changed.'}
                $state=Restore-KmgCompatibilityTransaction -RunId $Ownership.runId -StateRoot $Environment.stateRoot -FixtureMode:$Environment.fixtureMode
            }
            $restoredManifest=@(Get-KmgCompatibilityDirectoryManifest (Join-Path $Environment.install 'Mods'))
            if(-not $state.restorationVerified -or -not(Test-KmgCompatibilityManifestEqual @($Ownership.originalManifest) $restoredManifest)) {throw 'Cold original installation restoration is unverified.'}
            $Record.installationRestored=$true
            # The generic restore discharges its own lock. Reacquire the same
            # exact owner before final protected-exit/revalidation; never steal
            # an intervening foreign transaction. Working remains read-leased.
            $lock=Acquire-KmgCompatibilityLock $Environment.stateRoot $Ownership.runId
            $Lease.Stream=[IO.File]::Open($lock,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
            $Lease|Add-Member NoteProperty FinalRestoredGuard $true -Force
            $Record.finalBoundaryDebt=$true
        } catch {$errors.Add('installation: '+$_.Exception.Message)}
    }
    $Record.restorationErrors=@($errors.ToArray());$Record.recoveryRequired=($errors.Count -gt 0)
    if($errors.Count -gt 0 -and -not $Record.error){$Record.error='Cold restoration failed: '+($errors -join '; ')}
    $Record.status=if($errors.Count){'RECOVERY_REQUIRED'}elseif($Record.error){'FAILED_RESTORED'}else{'RESTORED'}
    Write-KmgCleanup2ColdJournal $JournalPath $Record
    if($errors.Count){throw ('Cold restoration debt retained; lock/backups must not be cleared: '+($errors -join '; '))}
}

function Invoke-KmgCleanup2ColdCurrentStackTransaction($Plan,$Environment) {
    Assert-KmgSummonCleanup2NativeExit
    $runId='cleanup2-cold-'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,12)
    $runRoot=Join-Path $Environment.stateRoot $runId;$evidence=Join-Path $Environment.evidenceRoot $runId
    $intakeRoot=Join-Path (Join-Path $Environment.stateRoot 'cleanup2-cold-intake') $runId
    $mods=Join-Path $Environment.install 'Mods';$backup=Join-Path $Environment.install ("Mods.kmg-compat-$runId.original")
    $inventory=@(Get-KmgSummonCleanup2Inventory $mods $backup)
    # The inventory owns identity/ambiguity checks and captures every current
    # mod's manifest. Its size is an observation, never a historical admission
    # pin; no installed mod is removed or substituted to satisfy a count.
    $original=@(Get-KmgCompatibilityDirectoryManifest $mods);$rootFiles=@(Get-ChildItem -LiteralPath $mods -File -Force|ForEach-Object Name)
    $featurePath=Join-Path $mods 'KingmakerGunslinger/FeatureModules.json'
    $modules=Read-KmgCompatibilityJson $featurePath
    if($modules.PSObject.Properties['expanded-summoning'] -and
        ($modules.'expanded-summoning' -isnot [bool] -or $modules.'expanded-summoning' -ne $true)){throw 'Cold trio requires current Expanded Summoning ON; no toggle is authorized.'}
    $context=$null;$lease=$null;$ownershipLease=$null;$entered=$false;$failure=$null;$ownership=$null
    $journal=Join-Path $intakeRoot 'cleanup2-cold-journal.json'
    $record=[ordered]@{schemaVersion=1;runId=$runId;status='PREFLIGHT';ownerProcessId=$PID;ownerStartedUtcTicks=(Get-Process -Id $PID).StartTime.ToUniversalTime().Ticks;
        candidate=$Plan.candidate;persistenceScope='giant-humanoids-ii';saveName='KMG_AUTOMATION_WORKING';nativeAttempted=$false;
        phases=@();error=$null;workingRestored=$false;settingsRestored=$false;installationRestored=$false;recoveryRequired=$false;restorationErrors=@();
        protectedExitGuard=$null;restoration=$null;backupReady=$false;metadataOnlyRecovery=$false;finalBoundaryDebt=$false}
    try {
        $settings=Open-KmgSummonCleanup2Settings $Environment.settingsKey $false
        try {$settingsSnapshot=@(Get-KmgFlatGameSettingsValues $settings)} finally {$settings.Dispose()}
        $context=Open-KmgCleanup2ColdCatalog $Environment.saveDirectory $evidence $record $journal
        if(-not(Test-KmgCompatibilityManifestEqual $original @(Get-KmgCompatibilityDirectoryManifest $mods))){throw 'Current Mods changed during cold intake.'}
        $context.backupPath=Join-Path $runRoot 'original-working.zks'
        $ownership=[ordered]@{schemaVersion=1;kind='Cleanup2ColdOriginalWorking';runId=$runId;ownerProcessId=$record.ownerProcessId;ownerStartedUtcTicks=$record.ownerStartedUtcTicks;
            install=[IO.Path]::GetFullPath($Environment.install);saveDirectory=$context.catalog.Directory;stateRoot=[IO.Path]::GetFullPath($Environment.stateRoot);
            working=$context.working;backupPath=$context.backupPath;allFiles=@($context.allFiles);originalManifest=$original;
            settingsSnapshot=$settingsSnapshot;candidate=$Plan.candidate;persistenceScope='giant-humanoids-ii';phases=@($Plan.phases);
            intakeModCount=$inventory.Count;intakeMods=@($inventory)}
        [void][IO.Directory]::CreateDirectory($intakeRoot)
        Assert-KmgCleanup2ColdTree $intakeRoot
        $ownershipPath=Join-Path $intakeRoot 'cleanup2-cold-ownership.json'
        $immutable=Write-KmgCleanup2ColdOwnershipNew $ownershipPath $ownership
        $ownershipSha=$immutable.sha256;$ownershipLease=$immutable.lease
        $record.ownershipSha256=$ownershipSha;$record.status='ACTIVE';$record.recoveryRequired=$true
        Write-KmgCleanup2ColdJournal $journal $record
        $resolution=[pscustomobject]@{profileId='cleanup2-cold-exact-current-intake';runtimeCapable=$true;
            gunslinger=[pscustomobject]@{packagePath=$Plan.candidate.packagePath};runtimeMods=@($inventory|Where-Object id -cne 'KingmakerGunslinger')}
        $transaction=Enter-KmgCompatibilityTransaction -Resolution $resolution -KingmakerInstallDir $Environment.install `
            -StateRoot $Environment.stateRoot -RunId $runId -FixtureMode:$Environment.fixtureMode
        $entered=$true;$lease=Open-KmgCompatibilityRuntimeLease $runId $Environment.stateRoot
        $transaction.recoveryInstructions="Use Restore-SummonCleanup2ColdCurrentStack.ps1 -RunId $runId -ExpectedOwnershipSha256 $ownershipSha. Do not use generic restore while cold save/settings debt remains."
        Write-KmgCompatibilityJsonAtomic (Join-Path $runRoot 'transaction.json') $transaction
        Write-KmgCleanup2ColdJournal $journal $record
        Backup-KmgCleanup2ColdWorking $context $runRoot
        $record.backupReady=$true;Write-KmgCleanup2ColdJournal $journal $record
        foreach($file in $rootFiles){Copy-Item -LiteralPath (Join-Path $backup $file) -Destination (Join-Path $mods $file)}
        Copy-Item -LiteralPath (Join-Path $backup 'KingmakerGunslinger/FeatureModules.json') -Destination (Join-Path $mods 'KingmakerGunslinger/FeatureModules.json') -Force
        foreach($scenario in $Plan.phases) {
            Assert-KmgSummonCleanup2NativeExit;Assert-KmgRuntimeLease $lease;Assert-KmgCleanup2ColdCatalog $context
            $check=New-KmgCleanup2ColdCurrentStackPlan -RepositoryRoot $Plan.repositoryRoot -PackagePath $Plan.candidate.packagePath `
                -ExpectedVersion $Plan.expectedVersion -DevelopmentPurpose $Plan.developmentPurpose -SaveName $Plan.saveName -PersistenceScope $Plan.persistenceScope -TimeoutSeconds $Plan.timeoutSeconds
            foreach($field in @('commit','sourceStateSha256','packageSha256','dllSha256','dllMvid')){if($check.candidate.$field -cne $Plan.candidate.$field){throw 'Cold candidate changed between phases.'}}
            $deployment=Invoke-KmgSummonCleanup2Deploy $Plan $lease $Environment.scriptsDirectory
            if(-not $record.nativeAttempted) {
                $settings=Open-KmgSummonCleanup2Settings $Environment.settingsKey $false
                try {$beforeNative=@(Get-KmgFlatGameSettingsValues $settings)} finally {$settings.Dispose()}
                if(($beforeNative|ConvertTo-Json -Depth 12 -Compress) -cne ($settingsSnapshot|ConvertTo-Json -Depth 12 -Compress)) {throw 'Cold settings changed before the first owned native launch.'}
            }
            Assert-KmgSummonCleanup2NativeExit
            if($scenario -cne 'working-save-expanded-summoning-verify-absent' -and $context.workingLease){$context.workingLease.Dispose();$context.workingLease=$null}
            $record.nativeAttempted=$true;$record.activePhase=$scenario
            Write-KmgCleanup2ColdJournal $journal $record
            $output=@(Invoke-KmgCleanup2ColdLaunch $Plan $scenario $lease $deployment $Environment.scriptsDirectory)
            $receipt=Get-KmgExactRuntimeLaunchReceipt -Output $output
            Wait-KmgSummonCleanup2Exit
            Assert-KmgSummonCleanup2NativeExit;Assert-KmgRuntimeLease $lease
            if(-not $context.workingLease){$context.workingLease=[IO.File]::Open($context.working.path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)}
            $phase=Assert-KmgCleanup2ColdReceipt $Plan $scenario $receipt @($record.phases)
            $phase.workingSha256=Get-KmgCompatibilitySha256 $context.working.path
            $record.phases=@($record.phases)+@($phase)
            Assert-KmgCleanup2ColdCatalog $context
            Write-KmgCleanup2ColdJournal $journal $record
        }
    } catch {$failure=$_;$record.error=$_.Exception.Message;$record.status='FAILED'}
    finally {
        if($entered) {
            try {Wait-KmgSummonCleanup2Exit}
            catch {
                if(-not $failure){$failure=$_;$record.error=$_.Exception.Message}
                Wait-KmgSummonCleanup2ProtectedExit -Record $record -RecordPath $journal -ExitFailure $_
            }
            try {
                Restore-KmgCleanup2ColdBoundary $ownership $context $record $journal $Environment $lease
            } catch {
                if(-not $failure){$failure=$_};if(-not $record.error){$record.error=$_.Exception.Message}
                $record.restorationErrors=@($record.restorationErrors)+@($_.Exception.Message)
                $record.recoveryRequired=$true;$record.status='RECOVERY_REQUIRED'
                try {Write-KmgCleanup2ColdJournal $journal $record} catch {}
            }
        } elseif($context) {
            # Entry may refuse after an unexpected process appears. Keep all
            # acquired protection until exit before settling read metadata.
            $preentryWait=Wait-KmgCleanup2ColdHandleRelease $record $journal $null
            if($preentryWait -and -not $failure){$failure=$preentryWait}
            try {
                Restore-KmgCleanup2ColdUnwrittenWorking $context;$record.workingRestored=$true
                $record.installationRestored=Test-KmgCompatibilityManifestEqual $original @(Get-KmgCompatibilityDirectoryManifest $mods)
                $settings=Open-KmgSummonCleanup2Settings $Environment.settingsKey $false
                try {$now=@(Get-KmgFlatGameSettingsValues $settings)}finally{$settings.Dispose()}
                $record.settingsRestored=(($now|ConvertTo-Json -Depth 12 -Compress) -ceq ($settingsSnapshot|ConvertTo-Json -Depth 12 -Compress))
                $record.recoveryRequired=(-not ($record.installationRestored -and $record.settingsRestored))
                $record.status=if($record.recoveryRequired){'RECOVERY_REQUIRED'}else{'FAILED_RESTORED'}
                if($ownership){Write-KmgCleanup2ColdJournal $journal $record}
            } catch {
                if(-not $failure){$failure=$_};if(-not $record.error){$record.error=$_.Exception.Message}
                $record.status='RECOVERY_REQUIRED';$record.recoveryRequired=$true
                $record.restorationErrors=@($record.restorationErrors)+@($_.Exception.Message)
                if($ownership){try{Write-KmgCleanup2ColdJournal $journal $record}catch{}}
            }
        }
        $lateFailure=Wait-KmgCleanup2ColdHandleRelease $record $journal $lease $context $ownership $Environment
        if($lateFailure -and -not $failure){$failure=$lateFailure}
        if($lease){$lease.Stream.Dispose()}
        if($ownershipLease){$ownershipLease.Dispose()}
        Close-KmgCleanup2ColdCatalog $context
    }
    if($failure){throw $failure}
    return [pscustomobject]@{kind='Cleanup2ColdTrioReceipt';runId=$runId;ownershipSha256=$record.ownershipSha256;
        journalPath=$journal;status=$record.status;phaseCount=@($record.phases).Count;nativeHydrationAccepted=$false}
}

function Test-KmgCleanup2ColdOwnerAlive($Ownership) {
    try {$owner=[Diagnostics.Process]::GetProcessById([int]$Ownership.ownerProcessId)}
    catch [ArgumentException] {return $false}
    try {return ($owner.StartTime.ToUniversalTime().Ticks -eq $Ownership.ownerStartedUtcTicks)}
    finally {$owner.Dispose()}
}

function Restore-KmgCleanup2ColdCurrentStackTransaction([string]$RunId,[string]$ExpectedOwnershipSha256,$Environment) {
    Assert-KmgSummonCleanup2NativeExit
    if($RunId -cnotmatch '^cleanup2-cold-\d{8}T\d{6}Z-[0-9a-f]{12}$' -or $ExpectedOwnershipSha256 -cnotmatch '^[0-9a-f]{64}$') {throw 'Cold recovery requires exact run ID and trusted original ownership hash.'}
    $runRoot=Join-Path ([IO.Path]::GetFullPath($Environment.stateRoot)) $RunId
    $intakeRoot=Join-Path (Join-Path $Environment.stateRoot 'cleanup2-cold-intake') $RunId
    Assert-KmgCleanup2ColdTree $intakeRoot
    $ownershipPath=Join-Path $intakeRoot 'cleanup2-cold-ownership.json'
    if((Get-KmgCompatibilitySha256 $ownershipPath) -cne $ExpectedOwnershipSha256) {throw 'Cold original ownership identity changed; recovery refused.'}
    $ownership=Read-KmgCompatibilityJson $ownershipPath
    if($ownership.kind -ceq 'Cleanup2ColdPreentryMetadataOnly') {
        return Restore-KmgCleanup2ColdPreentryMetadata $ownership $ExpectedOwnershipSha256 $Environment
    }
    if($ownership.kind -cne 'Cleanup2ColdOriginalWorking' -or $ownership.schemaVersion -ne 1 -or $ownership.runId -cne $RunId -or
        $ownership.install -cne [IO.Path]::GetFullPath($Environment.install) -or
        $ownership.stateRoot -cne [IO.Path]::GetFullPath($Environment.stateRoot) -or
        $ownership.saveDirectory -cne (Resolve-Path -LiteralPath $Environment.saveDirectory).Path -or
        $ownership.working.path -cne (Join-Path $ownership.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks') -or
        $ownership.backupPath -cne (Join-Path $runRoot 'original-working.zks') -or $ownership.persistenceScope -cne 'giant-humanoids-ii' -or
        $ownership.candidate.purpose -cne 'summon-cleanup-iteration2' -or $ownership.candidate.version -cne '0.0.163') {throw 'Cold recovery ownership scope or fixed path mismatch.'}
    if(Test-KmgCleanup2ColdOwnerAlive $ownership){throw 'The original cold transaction owner is still alive; recovery cannot steal its handles.'}
    $journal=Join-Path $intakeRoot 'cleanup2-cold-journal.json';$recordJson=Read-KmgCompatibilityJson $journal
    $record=[ordered]@{};foreach($property in $recordJson.PSObject.Properties){$record[$property.Name]=$property.Value}
    if($record.runId -cne $RunId -or $record.ownershipSha256 -cne $ExpectedOwnershipSha256){throw 'Cold recovery journal is not bound to original ownership.'}
    $statePath=Join-Path $runRoot 'transaction.json'
    # Entry may have rolled back its own installation before returning. Only
    # the read's metadata can remain; no save-byte replacement is admitted.
    if($record.nativeAttempted -eq $false -and @($record.phases).Count -eq 0 -and
        $record.recoveryRequired -eq $true -and $record.workingRestored -eq $false -and
        (-not(Test-Path -LiteralPath $statePath) -or (Read-KmgCompatibilityJson $statePath).status -ceq 'Restored')) {
        return Restore-KmgCleanup2ColdPreentryMetadata $ownership $ExpectedOwnershipSha256 $Environment
    }
    Assert-KmgCleanup2ColdTree $runRoot
    $state=Read-KmgCompatibilityJson (Join-Path $runRoot 'transaction.json')
    if($state.runId -cne $RunId -or $state.profileId -cne 'cleanup2-cold-exact-current-intake' -or
        -not(Test-KmgCompatibilityManifestEqual @($ownership.originalManifest) @($state.originalManifest))){throw 'Cold compatibility transaction identity differs.'}
    $finalOnly=$state.status -ceq 'Restored' -and $record.recoveryRequired -and
        $record.Contains('finalBoundaryDebt') -and $record.finalBoundaryDebt
    if($state.status -ceq 'Restored' -and -not $finalOnly){throw 'Cold installation transaction is already restored; repeated recovery cannot overwrite later Working/settings state.'}
    $context=$null;$lease=$null;$ownershipLease=$null
    try {
        if($state.status -cne 'Restored' -or $finalOnly) {
            $lock=Join-Path $Environment.stateRoot 'compatibility.lock'
            if((Get-Content -LiteralPath $lock -Raw).Trim() -cne $RunId){throw 'Cold recovery does not own compatibility.lock.'}
            $exclusive=[IO.File]::Open($lock,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None)
            $exclusive.Dispose()
            $stream=[IO.File]::Open($lock,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
            $lease=[pscustomobject]@{RunId=$RunId;LockPath=$lock;ProcessId=$PID;Stream=$stream;Compatibility=$true}
            if($finalOnly){$lease|Add-Member NoteProperty FinalRestoredGuard $true}
            Assert-KmgRuntimeLease $lease
        } elseif(-not $state.restorationVerified){throw 'Cold recovered installation state is not verified.'}
        Assert-KmgSummonCleanup2NativeExit
        $ownershipLease=[IO.File]::Open($ownershipPath,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
        if((Get-KmgCompatibilitySha256 $ownershipPath) -cne $ExpectedOwnershipSha256){throw 'Cold ownership changed before its immutable lease was acquired.'}
        $catalog=Open-KmgCleanup2ColdReadCatalog $ownership.saveDirectory (Join-Path $runRoot ('recovery-'+[Guid]::NewGuid().ToString('N'))) $record $journal
        $context=[pscustomobject]@{catalog=$catalog;allFiles=@($ownership.allFiles);working=$ownership.working;workingLease=$null;
            backupPath=$ownership.backupPath;backupSha256=$ownership.working.sha256;backupLease=$null}
        for($i=0;$i -lt $catalog.Files.Count;$i++) {
            if($catalog.Files[$i].path -ceq $ownership.working.path){$context.workingLease=$catalog.Leases[$i];$catalog.Leases.RemoveAt($i);break}
        }
        $catalog.Files=@($ownership.allFiles|Where-Object {$_.path -cne $ownership.working.path})
        if($record.nativeAttempted){$context.backupLease=[IO.File]::Open($ownership.backupPath,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)}
        if($finalOnly) {
            # No byte/settings/install restoration over later human activity.
            # Only an exact original state may discharge this final-check debt.
            Protect-KmgCleanup2ColdRestoredWorking $context
            $record.workingRestored=$true;$record.settingsRestored=$true;$record.installationRestored=$true
            Assert-KmgCleanup2ColdFinalRestoration $ownership $context $record $Environment
            $record.recoveryRequired=$false;$record.status='FAILED_RESTORED'
        } else {Restore-KmgCleanup2ColdBoundary $ownership $context $record $journal $Environment $lease}
        return [pscustomobject]@{kind='Cleanup2ColdRecoveryReceipt';runId=$RunId;status=$record.status;ownershipSha256=$ExpectedOwnershipSha256}
    } finally {
        $lateFailure=Wait-KmgCleanup2ColdHandleRelease $record $journal $lease $context $ownership $Environment
        if($lease){$lease.Stream.Dispose()};if($ownershipLease){$ownershipLease.Dispose()};Close-KmgCleanup2ColdCatalog $context
        if($lateFailure){throw $lateFailure}
    }
}
