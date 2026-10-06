[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
. (Join-Path $PSScriptRoot 'ElementalCharacterTraitPersistence.Common.ps1')
$root=Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$fixture=Join-Path $root ('artifacts/tests/trait-save-'+[Guid]::NewGuid().ToString('N'))
$saveDir=Join-Path $fixture 'saves';$evidence=Join-Path $fixture 'evidence'
[void][IO.Directory]::CreateDirectory($saveDir);[void][IO.Directory]::CreateDirectory($evidence)
$tx='20261006T2100001234567Z_'+[Guid]::NewGuid().ToString('N');$name='KMG_TRAITS_0142_'+$tx
$seed=Join-Path $saveDir 'Manual_299_KMG_AUTOMATION_WORKING.zks'
$foreign=Join-Path $saveDir 'Manual_1_foreign.zks'
[IO.File]::WriteAllText($seed,'fake-seed');[IO.File]::WriteAllText($foreign,'fake-foreign')
$script:checks=0
function Check([bool]$Value,[string]$Name){if(-not $Value){throw ('Invariant: '+$Name)};$script:checks++}
function Reject([scriptblock]$Action,[string]$Name){$rejected=$false;try{& $Action|Out-Null}catch{$rejected=$true};Check $rejected $Name}
$catalog=$null;$stream=$null
try{
    $catalog=Open-KmgProtectedSaveCatalog -EvidenceDirectory $evidence -SaveDirectory $saveDir
    Check ($catalog.Files.Count -eq 2) 'snapshot-every-preexisting-file'
    Check ((Assert-KmgProtectedSaveCatalog $catalog).passed) 'exact-original-inventory'
    Reject {[IO.File]::WriteAllText($seed,'changed')} 'seed-locked-load-only'
    Reject {[IO.File]::WriteAllText($foreign,'changed')} 'foreign-locked'
    Reject {Remove-Item -LiteralPath $foreign} 'preexisting-delete-blocked'
    $lock=Join-Path $fixture 'save.lock';$stream=[IO.File]::Open($lock,[IO.FileMode]::CreateNew,[IO.FileAccess]::ReadWrite,[IO.FileShare]::Read)
    $bytes=[Text.Encoding]::UTF8.GetBytes($tx);$stream.Write($bytes,0,$bytes.Length);$stream.Flush($true)
    $lease=[pscustomobject]@{Transaction=$tx;Descriptor=$name;Path=(Join-Path $fixture 'save-lease.json');Stream=$stream;LockPath=$lock}
    $state=[ordered]@{status='Active';transactionId=$tx;descriptor=$name;phase='prepare';ownerPid=$PID;
        ownerStartedUtc=(Get-Process -Id $PID).StartTime.ToUniversalTime().ToString('o');expiresUtc=[DateTime]::UtcNow.AddHours(1).ToString('o');ownedPath=$null;ownedSha256=$null}
    Write-ElementalTraitSaveJson $lease.Path $state
    Check ((Assert-ElementalTraitSaveLease $lease 'prepare').transactionId -ceq $tx) 'active-exact-lease'
    foreach($bad in @('status','transactionId','descriptor','phase','ownerPid','ownerStartedUtc','expiresUtc')){
        $old=$state[$bad];$state[$bad]=if($bad -ceq 'ownerPid'){1}elseif($bad -ceq 'expiresUtc'){[DateTime]::UtcNow.AddMinutes(-1).ToString('o')}else{'wrong'}
        Write-ElementalTraitSaveJson $lease.Path $state
        Reject {Assert-ElementalTraitSaveLease $lease 'prepare'} ('reject-lease-'+$bad)
        $state[$bad]=$old;Write-ElementalTraitSaveJson $lease.Path $state
    }
    $owned=Join-Path $saveDir ('Manual_301_'+$name+'.zks')
    [IO.File]::WriteAllText($owned,'owned-first')
    Reject {Assert-KmgProtectedSaveCatalog $catalog} 'new-save-needs-admission'
    Check ((Assert-KmgProtectedSaveCatalog $catalog @($owned)).passed) 'only-owned-creation'
    foreach($extra in @('Auto_1.zks','Quick_1.zks','Manual_99_unrelated.zks')){
        $path=Join-Path $saveDir $extra;[IO.File]::WriteAllText($path,'unexpected')
        Reject {Assert-KmgProtectedSaveCatalog $catalog @($owned)} ('reject-'+$extra)
        Remove-Item -LiteralPath $path
    }
    $run=Join-Path $fixture 'native-run';[void][IO.Directory]::CreateDirectory($run)
    Write-ElementalTraitSaveJson (Join-Path $run 'runtime-request.json') ([ordered]@{runId='test-native-run';scenario='elemental-character-traits-owned-save';enabled=$true;parameters=@{phase='prepare'}})
    $entry=[ordered]@{transactionId=$tx;phase='prepare';name=$name;path=$owned;existedBeforePreparation=$false;lifecycle='native-prepared-before-write';runId='test-native-run'}
    Write-ElementalTraitSaveJson (Join-Path $run 'elemental-trait-owned-save.json') $entry
    Check ((Register-ElementalTraitOwnedSave $catalog $lease $run 'prepare') -ceq $owned) 'native-ownership-before-write'
    foreach($bad in @('transactionId','phase','name','lifecycle','existedBeforePreparation','path','runId')){
        $old=$entry[$bad];$entry[$bad]=if($bad -ceq 'path'){$foreign}elseif($bad -ceq 'existedBeforePreparation'){$true}else{'wrong'}
        Write-ElementalTraitSaveJson (Join-Path $run 'elemental-trait-owned-save.json') $entry
        Reject {Register-ElementalTraitOwnedSave $catalog $lease $run 'prepare'} ('reject-receipt-'+$bad)
        $entry[$bad]=$old
    }
    $state=Get-Content -LiteralPath $lease.Path -Raw|ConvertFrom-Json
    $state.ownedPath=$foreign;Write-ElementalTraitSaveJson $lease.Path $state
    Reject {Remove-ElementalTraitOwnedSave $catalog $lease 'prepare'} 'cleanup-cannot-delete-preexisting'
    $state.ownedPath=$owned;Write-ElementalTraitSaveJson $lease.Path $state
    [IO.File]::WriteAllText($owned,'owned-second')
    Check ((Assert-KmgProtectedSaveCatalog $catalog @($owned)).passed) 'only-owned-overwrite'
    Reject {Remove-ElementalTraitOwnedSave $catalog $lease 'prepare'} 'cleanup-rejects-unreceipted-hash'
    $state.ownedSha256=(Get-FileHash -LiteralPath $owned -Algorithm SHA256).Hash.ToLowerInvariant();Write-ElementalTraitSaveJson $lease.Path $state
    Remove-ElementalTraitOwnedSave $catalog $lease 'prepare'
    Check (-not(Test-Path -LiteralPath $owned)) 'delete-only-exact-owned'
    Check ((Assert-KmgProtectedSaveCatalog $catalog).passed) 'exact-final-inventory'
    Check ((Get-FileHash -LiteralPath $seed -Algorithm SHA256).Hash.ToLowerInvariant() -ceq ($catalog.Files|Where-Object path -CEQ $seed).sha256) 'seed-final-hash'
}finally{
    if($null -ne $catalog){Close-KmgProtectedSaveCatalog $catalog}
    if($null -ne $stream){$stream.Dispose()}
}
Write-Host ('PASS elemental trait save isolation helpers: '+$script:checks+' checks; fake filesystem only; no game/no real save.')
