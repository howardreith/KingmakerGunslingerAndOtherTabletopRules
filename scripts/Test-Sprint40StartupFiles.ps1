$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeCoordination.Common.ps1')
. (Join-Path $PSScriptRoot 'Sprint40StartupFiles.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$fixture=Join-Path (Split-Path $PSScriptRoot -Parent) ('artifacts/fixtures/s40-peer-'+[Guid]::NewGuid().ToString('N'))
$mods=Join-Path $fixture 'Mods'; [void][IO.Directory]::CreateDirectory($mods)
$checks=0
function Check($pass,$message){if(-not $pass){throw $message};$script:checks++}
$original=@{gunslinger=$true;expandedSummoning=$true}
Assert-KmgSprint40DiagnosticRecoveryRequest -Scenarios @('observe-feature-module-settings') -Purpose sprint40-mechanics `
 -SaveName '' -ModuleDisabled $false -Parameters $original.Clone() -OriginalParameters $original
Check $true 'Original-configuration no-save recovery request admitted.'
foreach($mode in @('gameplay','wrong-purpose','save-load','module-off','changed-parameter','empty-parameters')){
 $request=@{Scenarios=@('observe-feature-module-settings');Purpose='sprint40-mechanics';SaveName='';ModuleDisabled=$false;Parameters=$original.Clone();OriginalParameters=$original}
 switch($mode){
  'gameplay'{$request.Scenarios=@('disposable-expanded-summoning-sprint32-native-review')}
  'wrong-purpose'{$request.Purpose='other-purpose'}
  'save-load'{$request.SaveName='KMG_AUTOMATION_WORKING'}
  'module-off'{$request.ModuleDisabled=$true}
  'changed-parameter'{$request.Parameters.expandedSummoning=$false}
  'empty-parameters'{$request.Parameters=@{}}
 }
 $refused=$false;try{Assert-KmgSprint40DiagnosticRecoveryRequest @request}catch{$refused=$true}
 Check $refused ('Recovery request refuses '+$mode)
}
$modules=@{gunslinger=$true;acadamaeGraduate=$true;shieldOther=$true;expandedSummoning=$true;
 elvenBranchedSpears=$true;easternWeapons=$true;brownFurTransmuter=$true;urbanBarbarian=$true;
 bodyguardFeats=$true;protectionFromAlignmentControlImmunity=$true;elementalRaces=$true;
 teleportationSpells=$true;magicCircleSpells=$true}
$activeVersion=(Get-KmgModInfo -RepositoryRoot (Split-Path $PSScriptRoot -Parent)).Version
$requestBase=@{Scenario='observe-feature-module-settings';ExpectedVersion=$activeVersion;TimeoutSeconds=120;
 ExitAfterCompletion=$true;EvidenceDirectory=('C:/Dev/KingmakerGunslingerLab/runtime-evidence/test-s40-request-'+[Guid]::NewGuid().ToString('N'))}
$ordinary=New-KmgRuntimeRequest @requestBase -Parameters $modules
Check ($ordinary.parameters.Count -eq 13 -and -not $ordinary.parameters.Contains('sprint40DiagnosticRecovery')) 'Ordinary settings request remains unchanged.'
$recovery=$modules.Clone();$recovery.sprint40DiagnosticRecovery=$true
$recoveryRequest=New-KmgRuntimeRequest @requestBase -Parameters $recovery
Check ($recoveryRequest.parameters.Count -eq 14 -and $recoveryRequest.parameters.sprint40DiagnosticRecovery -eq $true) 'Exact-true recovery metadata reaches the native request.'
foreach($mode in @('false','string','unknown')){
 $bad=$modules.Clone()
 if($mode -ceq 'false'){$bad.sprint40DiagnosticRecovery=$false}
 elseif($mode -ceq 'string'){$bad.sprint40DiagnosticRecovery='true'}
 else{$bad.foreignRecovery=$true}
 $refused=$false;try{$null=New-KmgRuntimeRequest @requestBase -Parameters $bad}catch{$refused=$true}
 Check $refused ('Runtime request refuses recovery metadata '+$mode)
}
foreach($relative in Get-KmgSprint40StartupFiles){
 $path=Join-Path $mods $relative;[void][IO.Directory]::CreateDirectory((Split-Path $path -Parent))
 [IO.File]::WriteAllText($path,'original '+$relative)
 [IO.File]::SetCreationTimeUtc($path,[DateTime]::new(2026,1,2,3,4,5,[DateTimeKind]::Utc))
 [IO.File]::SetLastWriteTimeUtc($path,[DateTime]::new(2026,2,3,4,5,6,[DateTimeKind]::Utc))
}
$snapshot=Backup-KmgSprint40StartupFiles -ModsRoot $mods -EvidenceDirectory (Join-Path $fixture 'backup')
foreach($relative in Get-KmgSprint40StartupFiles){[IO.File]::WriteAllText((Join-Path $mods $relative),'changed '+$relative)}
$result=Restore-KmgSprint40StartupFiles $snapshot
Check ($result.restored.Count -eq 9 -and $null -eq $result.preservedNativeDiagnostic) 'All nine original byte/metadata records restored.'
$favored=Join-Path $mods 'ZFavoredClass\loaded_blueprints.txt'
[IO.File]::WriteAllText($favored,'native fresh output')
$capture=Save-KmgSprint40GeneratedDiagnosticEvidence -Snapshot $snapshot
Check ((Get-KmgCompatibilitySha256 $capture.copyPath) -ceq (Get-KmgCompatibilitySha256 $favored)) 'Post-startup bytes are captured before restoration disposition.'
$native=Join-Path $fixture 'native.json'
$nativeCopy=Join-Path $fixture 'sprint40-generated-diagnostic.txt';Copy-Item -LiteralPath $favored -Destination $nativeCopy
$nativeHash=Get-KmgCompatibilitySha256 $favored;$nativeLength=(Get-Item $favored).Length
Write-KmgCompatibilityJsonAtomic $native ([ordered]@{scenario='observe-feature-module-settings';path=$favored;
 exactOriginalRegeneration=$true;bootstrapInitialized=$true;freshNativeWrite=$true;structuralRowsValid=$true;
 allRowsResolveLive=$true;actualRows=1;resolvedLiveRows=1;sha256=$nativeHash;length=$nativeLength;
 evidenceCopyPath=$nativeCopy;evidenceCopySha256=$nativeHash})
$null=Assert-KmgSprint40GeneratedDiagnosticReceipt -ReceiptPath $native -ExpectedPath $favored -ExpectedSha256 $nativeHash -ExpectedLength $nativeLength
Check $true 'Native recovery receipt validates actual current bytes and its captured evidence copy.'
foreach($mode in @('wrong-path','failed-producer','stale-bytes','missing-copy')){
 $bad=Read-KmgCompatibilityJson $native
 if($mode -ceq 'wrong-path'){$bad.path=Join-Path $mods 'CallOfTheWild\loaded_blueprints.txt'}
 if($mode -ceq 'failed-producer'){$bad.exactOriginalRegeneration=$false}
 if($mode -ceq 'stale-bytes'){$bad.sha256='0'*64}
 if($mode -ceq 'missing-copy'){$bad.evidenceCopyPath=Join-Path $fixture 'other.txt'}
 $badPath=Join-Path $fixture ($mode+'.json');Write-KmgCompatibilityJsonAtomic $badPath $bad
 $refused=$false;try{Assert-KmgSprint40GeneratedDiagnosticReceipt -ReceiptPath $badPath -ExpectedPath $favored -ExpectedSha256 $nativeHash -ExpectedLength $nativeLength|Out-Null}catch{$refused=$true}
 Check $refused ('Refuses '+$mode)
}
$first=Join-Path (Split-Path $snapshot.receiptPath -Parent) 'BagOfTricks\Favourites\Functions.xml'
[IO.File]::AppendAllText($first,'tamper')
$refused=$false;try{Restore-KmgSprint40StartupFiles $snapshot|Out-Null}catch{$refused=$true}
Check $refused 'Tampered backup refused before restoration.'
Write-Output "Sprint40 startup peer protection PASS=$checks; disposable files only."
