# Only a failed pre-entry read's metadata debt. No native process was admitted,
# no installation/settings were staged and no save bytes may be replaced.
Set-StrictMode -Version Latest
function Restore-KmgCleanup2ColdPreentryMetadata($Ownership,[string]$ExpectedOwnershipSha256,$Environment) {
    Assert-KmgSummonCleanup2NativeExit
    $runId=$Ownership.runId;$runRoot=Join-Path $Environment.stateRoot $runId
    $intakeRoot=Join-Path (Join-Path $Environment.stateRoot 'cleanup2-cold-intake') $runId
    $originalEntry=($Ownership.kind -ceq 'Cleanup2ColdOriginalWorking')
    if($Ownership.kind -cnotin @('Cleanup2ColdPreentryMetadataOnly','Cleanup2ColdOriginalWorking') -or $Ownership.schemaVersion -ne 1 -or
        (-not $originalEntry -and $Ownership.nativeAdmitted -ne $false) -or
        $Ownership.persistenceScope -cne 'giant-humanoids-ii' -or $Ownership.candidate.purpose -cne 'summon-cleanup-iteration2' -or
        $Ownership.candidate.version -cne '0.0.163' -or $Ownership.saveDirectory -cne (Resolve-Path -LiteralPath $Environment.saveDirectory).Path -or
        $Ownership.working.path -cne (Join-Path $Ownership.saveDirectory 'Manual_299_KMG_AUTOMATION_WORKING.zks') -or
        (-not $originalEntry -and (Test-Path -LiteralPath (Join-Path $runRoot 'transaction.json')))) {throw 'Pre-entry metadata recovery cannot replace a native/installation transaction or foreign save path.'}
    if(Test-KmgCleanup2ColdOwnerAlive $Ownership){throw 'Original pre-entry owner is still alive.'}
    $journal=Join-Path $intakeRoot 'cleanup2-cold-journal.json';$parsed=Read-KmgCompatibilityJson $journal
    $record=[ordered]@{};foreach($p in $parsed.PSObject.Properties){$record[$p.Name]=$p.Value}
    if($record.runId -cne $runId -or $record.ownershipSha256 -cne $ExpectedOwnershipSha256 -or $record.nativeAttempted -ne $false -or @($record.phases).Count -ne 0) {
        throw 'Pre-entry metadata receipt is not the exact no-native attempt.'
    }
    if($record.recoveryRequired -ne $true -or $record.workingRestored -eq $true -or $record.status -ceq 'PREENTRY_METADATA_RESTORED'){throw 'Pre-entry metadata debt is already discharged; later state must not be overwritten.'}
    if($originalEntry) {
        if($Ownership.install -cne [IO.Path]::GetFullPath($Environment.install) -or
            $Ownership.stateRoot -cne [IO.Path]::GetFullPath($Environment.stateRoot) -or
            $Ownership.backupPath -cne (Join-Path $runRoot 'original-working.zks')){throw 'No-entry metadata recovery has foreign installation/state paths.'}
        $statePath=Join-Path $runRoot 'transaction.json'
        if(Test-Path -LiteralPath $statePath) {
            $state=Read-KmgCompatibilityJson $statePath
            if($state.runId -cne $runId -or $state.profileId -cne 'cleanup2-cold-exact-current-intake' -or
                $state.status -cne 'Restored' -or -not $state.restorationVerified -or
                -not(Test-KmgCompatibilityManifestEqual @($Ownership.originalManifest) @($state.originalManifest))){throw 'No-entry installation rollback is not exact and discharged.'}
        }
        if(-not(Test-KmgCompatibilityManifestEqual @($Ownership.originalManifest) @(Get-KmgCompatibilityDirectoryManifest (Join-Path $Environment.install 'Mods')))){throw 'No-entry original installation changed; metadata-only recovery cannot write it.'}
        $settings=Open-KmgSummonCleanup2Settings $Environment.settingsKey $false
        try {$currentSettings=@(Get-KmgFlatGameSettingsValues $settings)}finally{$settings.Dispose()}
        if(($currentSettings|ConvertTo-Json -Depth 12 -Compress) -cne (@($Ownership.settingsSnapshot)|ConvertTo-Json -Depth 12 -Compress)){throw 'No-entry original settings changed; metadata-only recovery cannot write them.'}
    }
    $record.metadataOnlyRecovery=$true
    $lease=$null;$context=$null;$ownerLease=$null;$failure=$null
    try {
        $lock=Join-Path $Environment.stateRoot 'compatibility.lock'
        if(Test-Path -LiteralPath $lock) {
            if((Get-Content -LiteralPath $lock -Raw).Trim() -cne $runId){throw 'Pre-entry recovery found another compatibility owner.'}
            $probe=[IO.File]::Open($lock,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None);$probe.Dispose()
        } else {
            if(@(Get-KmgCompatibilityActiveTransactions $Environment.stateRoot).Count){throw 'Pre-entry recovery found unresolved installation debt.'}
            [void](Acquire-KmgCompatibilityLock $Environment.stateRoot $runId)
        }
        $lease=[pscustomobject]@{RunId=$runId;ProcessId=$PID;LockPath=$lock;Stream=[IO.File]::Open($lock,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)}
        Assert-KmgRuntimeLease $lease;Assert-KmgSummonCleanup2NativeExit
        $ownerPath=Join-Path $intakeRoot 'cleanup2-cold-ownership.json'
        $ownerLease=[IO.File]::Open($ownerPath,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
        if((Get-KmgCompatibilitySha256 $ownerPath) -cne $ExpectedOwnershipSha256){throw 'Pre-entry ownership changed before its immutable lease was acquired.'}
        $catalog=Open-KmgCleanup2ColdReadCatalog $Ownership.saveDirectory (Join-Path $intakeRoot ('metadata-recovery-'+[Guid]::NewGuid().ToString('N'))) $record $journal
        $context=[pscustomobject]@{catalog=$catalog;working=$Ownership.working;workingLease=$null;backupLease=$null;backupPath=$null}
        $selection=Assert-KmgSummonCleanup2SaveSelection $catalog
        if($selection.working.path -cne $Ownership.working.path){throw 'Pre-entry Working selection changed.'}
        for($i=0;$i -lt $catalog.Files.Count;$i++){if($catalog.Files[$i].path -ceq $Ownership.working.path){$context.workingLease=$catalog.Leases[$i];$catalog.Leases.RemoveAt($i);break}}
        $catalog.Files=if($originalEntry){@($Ownership.allFiles|Where-Object {$_.path -cne $Ownership.working.path})}else{@($catalog.Files|Where-Object {$_.path -cne $Ownership.working.path})}
        # Requires identical original data/creation/write/attributes/security.
        # Only the read-modified access timestamp can be settled here.
        Restore-KmgCleanup2ColdUnwrittenWorking $context
        $record.workingRestored=$true;$record.recoveryRequired=$false;$record.status='PREENTRY_METADATA_RESTORED'
        Write-KmgCleanup2ColdJournal $journal $record
        Assert-KmgSummonCleanup2NativeExit
        $lease.Stream.Dispose();Remove-KmgCompatibilityOwnedLock $lock $runId
    } catch {
        $failure=$_;$record.recoveryRequired=$true;$record.status='RECOVERY_REQUIRED'
        $record.restorationErrors=@($record.restorationErrors)+@($_.Exception.Message)
        try{Write-KmgCleanup2ColdJournal $journal $record}catch{}
    } finally {
        $late=Wait-KmgCleanup2ColdHandleRelease $record $journal $lease
        if($lease){$lease.Stream.Dispose()};if($ownerLease){$ownerLease.Dispose()};Close-KmgCleanup2ColdCatalog $context
        if($late -and -not $failure){$failure=$late}
    }
    if($failure){throw $failure}
    return [pscustomobject]@{kind='Cleanup2ColdPreentryMetadataReceipt';runId=$runId;status='PREENTRY_METADATA_RESTORED';nativeAdmitted=$false;saveBytesWritten=$false}
}
