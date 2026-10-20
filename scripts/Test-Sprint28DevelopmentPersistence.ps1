$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
if(-not(Test-KmgSprint28DevelopmentPersistence -Scope giant-humanoids -Purpose sprint28-native-prototype)){throw 'Exact Sprint 28 development scope refused'}
if(Test-KmgSprint28DevelopmentPersistence -Scope giant-humanoids -Purpose ''){throw 'Release path became development'}
if(-not(Test-KmgGriffonDevelopmentPersistence -Scope griffon -Purpose sprint27-native-prototype)){throw 'Sprint 27 scoped authority changed'}
$count=3
foreach($case in @(@('Giant-humanoids','sprint28-native-prototype'),@('giant-humanoids','Sprint28-native-prototype'),@('giant-humanoids','other'),@('griffon','sprint28-native-prototype'),@('whole-roster','sprint28-native-prototype'),@('','sprint28-native-prototype'),@('giant-humanoids','sprint28-native-prototype '))){
 $refused=$false;try{[void](Test-KmgSprint28DevelopmentPersistence -Scope $case[0] -Purpose $case[1])}catch{$refused=$true}
 if(-not $refused){throw 'Invalid Sprint 28 development persistence authority accepted'};$count++
}
Write-Host "Sprint28 development persistence authority PASS=$count; pure scope/refusal fixtures only; no save or game access."
