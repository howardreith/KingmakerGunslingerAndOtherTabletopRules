[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='High')]
param([string]$ExpectedVersion='0.0.142',[Parameter(Mandatory=$true)][string]$PackagePath)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
. (Join-Path $PSScriptRoot 'ElementalCharacterTraitPersistence.Common.ps1')
Assert-KmgUnelevated
Assert-KmgNotRunning
$root=Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$manifest=Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $root
if($ExpectedVersion -cne '0.0.142' -or $manifest.commit -cne (& git -C $root rev-parse HEAD).Trim() -or @(& git -C $root status --porcelain).Count -ne 0){throw 'Exact clean committed artifact required.'}
if(-not $PSCmdlet.ShouldProcess('new transaction-owned manual save','qualify canonical traits without writing preexisting saves')){return}
$tx=[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ')+'_'+[Guid]::NewGuid().ToString('N')
$descriptor='KMG_TRAITS_0142_'+$tx
$evidenceRoot='C:/Dev/KingmakerGunslingerLab/runtime-evidence'
$transaction=Join-Path $evidenceRoot ('elemental-trait-save-'+$tx)
[void][IO.Directory]::CreateDirectory($transaction)
$saveLock=Join-Path $evidenceRoot 'elemental-trait-save.lock'
$saveStream=$null;$catalog=$null;$runtime=$null;$backup=$null;$deployment=$null;$failure=$null;$currentPhase='prepare'
$lease=$null;$runs=[Collections.Generic.List[object]]::new()
$live='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker/Mods/KingmakerGunslinger'
function Get-ElementalLiveTree([string]$Path){
    $base=[IO.Path]::GetFullPath($Path).TrimEnd('\')
    return [ordered]@{
        files=@(Get-ChildItem -LiteralPath $base -Recurse -File -Force|Sort-Object FullName|ForEach-Object {
            if($_.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse live tree rejected.'}
            [ordered]@{path=$_.FullName.Substring($base.Length).TrimStart('\');sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()}
        })
        directories=@(Get-ChildItem -LiteralPath $base -Recurse -Directory -Force|ForEach-Object {$_.FullName.Substring($base.Length).TrimStart('\')}|Sort-Object)
    }
}
$receipt=[ordered]@{schemaVersion=1;transactionId=$tx;descriptor=$descriptor;source=$manifest.commit;dllSha256=$manifest.dllSha256;
    mvid=$manifest.dllMvid;zipSha256=$manifest.packageSha256;sourceFingerprint=$manifest.sourceStateSha256;
    status='RUNNING';runs=@();processAttempts=@();saveWrites=0;savesRestored=$false;liveRestored=$false;saveLeaseCompleted=$false;runtimeLeaseCompleted=$false}
try{
    $catalog=Open-KmgProtectedSaveCatalog -EvidenceDirectory $transaction
    $receipt.preexistingSaveCount=@($catalog.Files|Where-Object path -CLike '*.zks').Count
    if(@($catalog.Files|Where-Object {[IO.Path]::GetFileName($_.path) -match [regex]::Escape($descriptor)}).Count -ne 0){throw 'Descriptor collision.'}
    [void](Assert-KmgProtectedSaveCatalog -Catalog $catalog)
    $saveStream=[IO.File]::Open($saveLock,[IO.FileMode]::CreateNew,[IO.FileAccess]::ReadWrite,[IO.FileShare]::Read)
    $bytes=[Text.Encoding]::UTF8.GetBytes($tx);$saveStream.Write($bytes,0,$bytes.Length);$saveStream.Flush($true)
    $lease=[pscustomobject]@{Transaction=$tx;Descriptor=$descriptor;Path=(Join-Path $transaction 'save-lease.json');Stream=$saveStream;LockPath=$saveLock}
    $leaseState=[ordered]@{schemaVersion=1;transactionId=$tx;descriptor=$descriptor;phase=$currentPhase;status='Active';
        ownerPid=$PID;ownerStartedUtc=(Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o');
        expiresUtc=[DateTime]::UtcNow.AddHours(2).ToString('o');ownedPath=$null;ownedSha256=$null;stagingPath=$null;stagingSha256=$null}
    Write-ElementalTraitSaveJson $lease.Path $leaseState
    $runtime=Enter-KmgRuntimeLease -Purpose ('elemental-character-trait-save '+$tx)
    $receipt.runtimeLease=$runtime.Lease.RunId;$receipt.saveLease=$lease.Path
    $original=Get-ElementalLiveTree $live
    $receipt.liveBefore=$original
    $backup=& (Join-Path $PSScriptRoot 'Backup-Live-Mod.ps1') -Confirm:$false
    $receipt.liveBackup=$backup.Destination
    if(($original.files|ConvertTo-Json -Depth 8 -Compress) -cne ((Get-ElementalLiveTree $backup.Destination).files|ConvertTo-Json -Depth 8 -Compress)){throw 'Exact backup differs.'}
    $deployment=& (Join-Path $PSScriptRoot 'Deploy-Local.ps1') -PackagePath $PackagePath -RuntimeLease $runtime.Lease -PassThru -Confirm:$false
    $receipt.deployment=$deployment
    $seedFile='Manual_299_KMG_AUTOMATION_WORKING.zks'
    $seed=@($catalog.Files|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $seedFile})
    if($seed.Count -ne 1){throw 'Exact load-only working seed missing.'}
    $inputSave=[ordered]@{name='KMG_AUTOMATION_WORKING';file=$seedFile;path=$seed[0].path;sha256=$seed[0].sha256;
        gameName='Hedwirg';gameId='dce769e0-229c-4bfd-b8ea-e2d572bf8472';areaName='JamandisMansion';partyCount=3}
    $expected=$null;$previous=$null
    foreach($phase in @('prepare','verify-remove','verify-absent')){
        $currentPhase=$phase
        $state=Get-Content -LiteralPath $lease.Path -Raw|ConvertFrom-Json
        $state.phase=$phase;$state.expiresUtc=[DateTime]::UtcNow.AddHours(2).ToString('o')
        Write-ElementalTraitSaveJson $lease.Path $state
        [void](Assert-ElementalTraitSaveLease $lease $phase)
        [void](Assert-KmgProtectedSaveCatalog -Catalog $catalog -OwnedPaths @($state.ownedPath,$state.stagingPath|Where-Object {$null -ne $_}))
        $planPath=Join-Path $transaction ($phase+'-plan.json')
        Write-ElementalTraitSaveJson $planPath ([ordered]@{schemaVersion=1;transactionId=$tx;phase=$phase;version=$ExpectedVersion;
            dllSha256=$manifest.dllSha256;input=$inputSave;expected=$expected;previousResultPath=$previous;leasePath=$lease.Path})
        $beforeRuns=@(Get-ChildItem -LiteralPath $evidenceRoot -Directory|ForEach-Object FullName)
        $runFailure=$null;$runDirectory=$null
        try{
            $global:LASTEXITCODE=0
            & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') -Scenario 'elemental-character-traits-owned-save' -ExpectedVersion $ExpectedVersion -SaveName $inputSave.name -Parameters @{phase=$phase;planPath=$planPath} -ExitAfterCompletion:$true -TimeoutSeconds 1200 -RuntimeLease $runtime.Lease -ReuseInstalledArtifact -DeploymentManifestPath $deployment -PackagePath $PackagePath -Confirm:$false
            if($LASTEXITCODE -ne 0){throw 'Native persistence phase failed.'}
        }catch{$runFailure=$_}
        finally{
            Wait-PersistenceExit
            $new=@(Get-ChildItem -LiteralPath $evidenceRoot -Directory|Where-Object {$beforeRuns -cnotcontains $_.FullName -and $_.Name.EndsWith('-elemental-character-traits-owned-save',[StringComparison]::Ordinal)})
            if($new.Count -eq 1){
                $runDirectory=$new[0].FullName
                [void](Register-ElementalTraitOwnedSave $catalog $lease $runDirectory $phase)
                $nativePath=Join-Path $runDirectory 'elemental-trait-save.json'
                $stageWrites=0
                if(Test-Path -LiteralPath $nativePath){$stageNative=Get-Content -LiteralPath $nativePath -Raw|ConvertFrom-Json;$stageWrites=[int]$stageNative.saveWrites}
                $receipt.saveWrites+=$stageWrites
                $receipt.processAttempts+=@([ordered]@{phase=$phase;directory=$runDirectory;saveWrites=$stageWrites})
            }
            $state=Get-Content -LiteralPath $lease.Path -Raw|ConvertFrom-Json
            [void](Assert-KmgProtectedSaveCatalog -Catalog $catalog -OwnedPaths @($state.ownedPath,$state.stagingPath|Where-Object {$null -ne $_}))
        }
        if($null -ne $runFailure){throw $runFailure}
        if($null -eq $runDirectory){throw 'Stage evidence is ambiguous.'}
        $previous=Join-Path $runDirectory 'runtime-result.json'
        $result=Get-Content -LiteralPath $previous -Raw|ConvertFrom-Json
        $native=Get-Content -LiteralPath (Join-Path $runDirectory 'elemental-trait-save.json') -Raw|ConvertFrom-Json
        $loaded=Get-Content -LiteralPath (Join-Path $runDirectory 'runtime-loaded-build-identity.json') -Raw|ConvertFrom-Json
        if($result.status -cne 'PASS' -or $result.gitCommit -cne $manifest.commit -or $native.transactionId -cne $tx -or
            $native.phase -cne $phase -or $native.runId -cne $result.runId -or $native.dllSha256 -cne $manifest.dllSha256 -or
            $native.unexpectedSaveWritingApiObserved -ne $false -or $loaded.loadedModuleSha256 -cne $manifest.dllSha256 -or
            $loaded.moduleVersionId -cne $manifest.dllMvid -or @($runs|Where-Object processId -EQ $native.processId).Count -ne 0 -or
            @($result.assertions|Where-Object status -CNE 'PASS').Count -ne 0){throw 'Exact fresh-process structured PASS required.'}
        $runs.Add([ordered]@{phase=$phase;runId=$result.runId;processId=$native.processId;assertions=@($result.assertions).Count;result=$previous;saveWrites=$native.saveWrites})
        $receipt.runs=@($runs.ToArray())
        Write-ElementalTraitSaveJson (Join-Path $transaction 'transaction-result.json') $receipt
        $expected=$native.witness;$inputSave=$native.savedInfo
        if($inputSave.name -cne $descriptor -or $inputSave.sha256 -cne (Get-FileHash -LiteralPath $inputSave.path -Algorithm SHA256).Hash.ToLowerInvariant()){throw 'Native save receipt no longer matches.'}
    }
}catch{$failure=$_}
finally{
    try{
        Wait-PersistenceExit
        if($null -ne $lease){
            Remove-ElementalTraitOwnedSave $catalog $lease $currentPhase
            $final=Assert-KmgProtectedSaveCatalog -Catalog $catalog;$receipt.savesRestored=$final.passed
            $receipt.finalSaveInventoryExact=$final
            $state=Assert-ElementalTraitSaveLease $lease $currentPhase
            $state.status=if($null -eq $failure){'Completed'}else{'FailedRestored'}
            Write-ElementalTraitSaveJson $lease.Path $state
            $saveStream.Dispose();$saveStream=$null;$lease.Stream=$null
            if((Get-Content -LiteralPath $saveLock -Raw).Trim() -cne $tx){throw 'Foreign save lock at closure.'}
            Remove-Item -LiteralPath $saveLock;$receipt.saveLeaseCompleted=$true
        }
    }catch{if($null -eq $failure){$failure=$_};$receipt.saveCleanupFailure=$_.Exception.ToString()}
    finally{
        if($null -ne $catalog){Close-KmgProtectedSaveCatalog $catalog}
        if($null -ne $saveStream){$saveStream.Dispose()}
        if($null -ne $runtime){
            try{
                Assert-KmgNotRunning
                if($null -ne $backup){
                    & (Join-Path $PSScriptRoot 'Restore-Live-Mod.ps1') -BackupDirectory $backup.Destination -RuntimeLease $runtime.Lease -Confirm:$false
                    foreach($relative in $original.directories){
                        $target=Assert-KmgPathWithin -Path (Join-Path $live $relative) -Root $live
                        if(-not(Test-Path -LiteralPath $target -PathType Container)){[void][IO.Directory]::CreateDirectory($target)}
                    }
                    $restored=Get-ElementalLiveTree $live
                    if(($original|ConvertTo-Json -Depth 8 -Compress) -cne ($restored|ConvertTo-Json -Depth 8 -Compress)){throw 'Exact live restoration failed.'}
                    $receipt.liveAfter=$restored;$receipt.liveRestored=$true
                }
                Exit-KmgRuntimeLease $runtime;$receipt.runtimeLeaseCompleted=$true
            }catch{if($null -eq $failure){$failure=$_};$receipt.liveCleanupFailure=$_.Exception.ToString()}
        }
    }
    $receipt.status=if($null -eq $failure -and $runs.Count -eq 3 -and $receipt.savesRestored -and $receipt.liveRestored){'PASS'}else{'FAIL'}
    $receipt.error=if($null -eq $failure){$null}else{$failure.ToString()}
    Write-ElementalTraitSaveJson (Join-Path $transaction 'transaction-result.json') $receipt
}
if($null -ne $failure){throw $failure}
Write-Output ('PASS leased trait persistence: '+$transaction)
