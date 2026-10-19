$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
if(-not(Test-KmgGriffonDevelopmentPersistence -Scope griffon -Purpose sprint27-native-prototype)){throw 'Exact development scope refused'}
if(Test-KmgGriffonDevelopmentPersistence -Scope griffon -Purpose ''){throw 'Release path became development'}
$count=2
foreach($case in @(@('Griffon','sprint27-native-prototype'),@('griffon','Sprint27-native-prototype'),@('griffon','other'),@('magical-beasts','sprint27-native-prototype'),@('whole-roster','sprint27-native-prototype'),@('','sprint27-native-prototype'),@('griffon','sprint27-native-prototype '))){
 $refused=$false;try{[void](Test-KmgGriffonDevelopmentPersistence -Scope $case[0] -Purpose $case[1])}catch{$refused=$true}
 if(-not $refused){throw 'Invalid development persistence authority accepted'};$count++
}
Write-Host "Griffon development persistence authority PASS=$count; pure scope/refusal fixtures only; no save or game access."
