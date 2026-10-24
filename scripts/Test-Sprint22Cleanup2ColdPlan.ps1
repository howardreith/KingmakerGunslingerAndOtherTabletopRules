[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
$checks=0
function Assert-Cold([bool]$Condition,[string]$Name) {
    if(-not $Condition){throw "Cleanup 2 cold-plan predicate failed: $Name"}
    $script:checks++
}
function Assert-ColdRefusal([scriptblock]$Action,[string]$Pattern) {
    $caught=$null;try{& $Action | Out-Null}catch{$caught=$_}
    Assert-Cold ($null -ne $caught -and $caught.Exception.Message -like "*$Pattern*") $Pattern
}
function Copy-ColdFixture($Value) {return ($Value | ConvertTo-Json -Depth 8 | ConvertFrom-Json)}
$scope=@{Scope='giant-humanoids-ii';Purpose='summon-cleanup-iteration2';SaveName='KMG_AUTOMATION_WORKING';ExpectedVersion='0.0.163'}
Assert-KmgCleanup2EttinColdPlanScope @scope
Assert-Cold $true 'exact requested scope/purpose/save/version admitted for planning only'
foreach($case in @(
    @('Scope','Giant-humanoids-ii'),@('Scope','giant-humanoids'),@('Scope','whole-roster'),@('Scope','griffon'),
    @('Purpose','Summon-cleanup-iteration2'),@('Purpose','summon-cleanup-iteration2 '),@('Purpose','sprint29-hidden-bootstrap'),
    @('SaveName','KMG_AUTOMATION_BASELINE'),@('SaveName','kmg_automation_working'),
    @('ExpectedVersion','0.0.162'),@('ExpectedVersion','0.0.164'),@('ExpectedVersion','')
)) {
    $trial=@{};foreach($key in $scope.Keys){$trial[$key]=$scope[$key]};$trial[$case[0]]=$case[1]
    Assert-ColdRefusal {Assert-KmgCleanup2EttinColdPlanScope @trial} 'only exact'
}
$receipt=[pscustomobject]@{candidateKind='development';releaseQualification=$false;purpose=$scope.Purpose;
    version='0.0.163';commit=('a'*40);sourceStateSha256=('b'*64);packageSha256=('c'*64);dllSha256=('d'*64)}
$clean=[pscustomobject]@{Commit=$receipt.commit;Status=@()}
$original=[pscustomobject]@{path='C:\Fixture\Saved Games\Manual_299_KMG_AUTOMATION_WORKING.zks';length=[long]1024;
    sha256=('e'*64);creationUtcTicks=[long]638000000000000000;writeUtcTicks=[long]638000000000000001;
    accessUtcTicks=[long]638000000000000002;attributes=[int][IO.FileAttributes]::Archive}
$arguments=@{Receipt=$receipt;Git=$clean;SourceStateSha256=$receipt.sourceStateSha256;OriginalWorking=$original}
$plan=New-KmgCleanup2EttinColdPlan @scope @arguments
Assert-Cold ($plan.sourceBindingVerified -and -not $plan.fullPackageValidationPerformed) 'real source-binding predicate used; no package validation fabricated'
Assert-Cold (-not $plan.liveUsePermitted -and -not $plan.runtimeLaunchPermitted -and $plan.blockers.Count -eq 3) 'retained-owner/restoration/observer prerequisites remain explicit'
Assert-Cold (($plan.keys -join ',') -ceq 'ettin,cloud-giant,storm-giant') 'only exact current trio keys'
Assert-Cold (($plan.phases -join ',') -ceq 'working-save-expanded-summoning-prepare,working-save-expanded-summoning-verify-cleanup,working-save-expanded-summoning-verify-absent' -and
    $plan.freshProcessEveryPhase -and $plan.expandedSummoningEnabledEveryPhase) 'unchanged exact trio order, fresh processes and module-ON cold path'
$original.sha256=('f'*64)
Assert-Cold ($plan.originalWorking.sha256 -ceq ('e'*64)) 'plan freezes scalar intake records independently of caller object mutation'
$original.sha256=('e'*64)
foreach($case in @('purpose','version','commit','source','dirty','kind','release','release-type','package','dll')) {
    $trialReceipt=Copy-ColdFixture $receipt;$trialGit=Copy-ColdFixture $clean
    switch($case) {
        purpose {$trialReceipt.purpose='other-purpose'}
        version {$trialReceipt.version='0.0.162'}
        commit {$trialGit.Commit=('f'*40)}
        source {$trialReceipt.sourceStateSha256=('f'*64)}
        dirty {$trialGit.Status=@(' M assigned-source.cs')}
        kind {$trialReceipt.candidateKind='release-candidate'}
        release {$trialReceipt.releaseQualification=$true}
        release-type {$trialReceipt.releaseQualification='false'}
        package {$trialReceipt.packageSha256='not-a-hash'}
        dll {$trialReceipt.dllSha256='not-a-hash'}
    }
    Assert-ColdRefusal {New-KmgCleanup2EttinColdPlan @scope -Receipt $trialReceipt -Git $trialGit -SourceStateSha256 $receipt.sourceStateSha256 -OriginalWorking $original} ''
}
$comparison=Compare-KmgCleanup2WorkingSaveRestoration -Original $original -Restored (Copy-ColdFixture $original)
Assert-Cold ($comparison.verified -and -not $comparison.liveUsePermitted -and -not $comparison.bytesReadOrWritten -and -not $comparison.nativeHydrationProven) 'exact scalar equality cannot authorize writes or claim native hydration'
foreach($field in @('path','length','sha256','creationUtcTicks','writeUtcTicks','accessUtcTicks','attributes')) {
    $changed=Copy-ColdFixture $original
    switch($field) {
        path {$changed.path='C:\DifferentFixture\Manual_299_KMG_AUTOMATION_WORKING.zks'}
        sha256 {$changed.sha256=('f'*64)}
        attributes {$changed.attributes=[int][IO.FileAttributes]::Normal}
        default {$changed.$field++}
    }
    $comparison=Compare-KmgCleanup2WorkingSaveRestoration -Original $original -Restored $changed
    Assert-Cold (-not $comparison.verified -and -not $comparison.fields.$field) ('restoration mismatch refuses: '+$field)
}
foreach($case in @('baseline-path','missing-access','fractional-length','bad-hash','reparse','directory')) {
    $changed=Copy-ColdFixture $original
    switch($case) {
        baseline-path {$changed.path='C:\Fixture\Saved Games\Manual_298_KMG_AUTOMATION_BASELINE.zks'}
        missing-access {$changed.PSObject.Properties.Remove('accessUtcTicks')}
        fractional-length {$changed.length=1024.5}
        bad-hash {$changed.sha256='not-a-hash'}
        reparse {$changed.attributes=[int][IO.FileAttributes]::ReparsePoint}
        directory {$changed.attributes=[int][IO.FileAttributes]::Directory}
    }
    Assert-ColdRefusal {Compare-KmgCleanup2WorkingSaveRestoration -Original $original -Restored $changed} 'Cleanup 2'
}
# Exercise the real production entry point. The explicit new-purpose refusal
# precedes process inspection, package access, deployment or any save operation.
Assert-ColdRefusal {
    & (Join-Path $PSScriptRoot 'Invoke-Sprint22InstalledStack.ps1') -Scenario working-save-expanded-summoning-prepare `
        -SaveName KMG_AUTOMATION_WORKING -PackagePath 'must-not-be-read.zip' -ExpectedVersion 0.0.163 `
        -PersistenceScope giant-humanoids-ii -DevelopmentPurpose summon-cleanup-iteration2 -Confirm:$false
} 'retained owner spanning the entire trio'
Assert-Cold (Test-KmgSprint29DevelopmentPersistence -Scope giant-humanoids-ii -Purpose sprint29-hidden-bootstrap) 'prior exact Sprint29 authority unchanged'
Assert-ColdRefusal {Test-KmgSprint29DevelopmentPersistence -Scope giant-humanoids-ii -Purpose summon-cleanup-iteration2} 'only exact'
Write-Host "Cleanup 2 Ettin cold-plan safety PASS=$checks; synthetic scalar records and pre-runtime refusals only; no save, game, package, installation or settings access."
