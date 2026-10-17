[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'BrownFurStartupProbe.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$fixture=Join-Path (Get-KmgCompatibilityRepositoryRoot) ('artifacts/brown-fur-startup/protocol-'+[Guid]::NewGuid().ToString('N'))
$mods=Join-Path $fixture 'Mods'
$names=@('BagOfTricks','BetterVendors','CallOfTheWild','CheatMenu','CraftMagicItems','EddicKingmakerRespec',
    'KingmakerBuffPlanner','KingmakerBugfixes','KingmakerDiceRoller','KingmakerGunslinger','KingmakerLastAzlantiPreserver',
    'ProperFlanking2','RacesUnleashed','SkipIntro','TweakOrTreat','ZFavoredClass')
foreach ($name in $names) {
    $path=Join-Path $mods $name;[void][IO.Directory]::CreateDirectory($path)
    $version=if($name -ceq 'CallOfTheWild'){'1.14.4c-2.1'}else{'fixture'}
    [IO.File]::WriteAllText((Join-Path $path 'Info.json'),(@{Id=$name;Version=$version}|ConvertTo-Json))
}
$checks=0
function Check([bool]$value) { if(-not $value){throw 'Brown-Fur probe safety assertion failed.'};$script:checks++ }
function Reject([scriptblock]$action) { $rejected=$false;try{& $action|Out-Null}catch{$rejected=$true};Check $rejected }
$timings=Get-KmgBrownFurProbeTimings 300
$null=Assert-KmgRuntimeScenarioPreflight -Scenario disposable-brown-fur-native-cast -ExpectedVersion '0.0.157' `
    @timings -StartupTimeoutSeconds 300 -Parameters @{saveName='KMG_AUTOMATION_WORKING'}
Check ($timings.MainMenuTimeoutSeconds -gt 0 -and $timings.FingerprintTimeoutSeconds -gt 0)
Reject {Get-KmgBrownFurProbeTimings 0}
Reject {Assert-KmgRuntimeScenarioPreflight -Scenario disposable-brown-fur-native-cast -ExpectedVersion '0.0.157' `
    @timings -Parameters @{saveName='KMG_AUTOMATION_BASELINE'}}
$before=@(Get-KmgCompatibilityDirectoryManifest $mods)
$resolution=New-KmgBrownFurProbeResolution $mods (Join-Path $fixture 'not-yet-existing-backup') 'candidate.zip'
Check ($resolution.runtimeMods.Count -eq 15)
Check (@($resolution.runtimeMods | Where-Object id -ceq 'KingmakerBuffPlanner').Count -eq 1)
Check (Test-KmgCompatibilityManifestEqual $before @(Get-KmgCompatibilityDirectoryManifest $mods))
$unknown=Join-Path $mods 'Unknown';[void][IO.Directory]::CreateDirectory($unknown)
Reject {New-KmgBrownFurProbeResolution $mods $fixture 'candidate.zip'}
Remove-Item -LiteralPath $unknown
$cotwInfo=Join-Path $mods 'CallOfTheWild/Info.json'
[IO.File]::WriteAllText($cotwInfo,'{"Id":"CallOfTheWild","Version":"unsupported"}')
Reject {New-KmgBrownFurProbeResolution $mods $fixture 'candidate.zip'}
[IO.File]::WriteAllText($cotwInfo,'{"Id":"WrongMod","Version":"1.14.4c-2.1"}')
Reject {New-KmgBrownFurProbeResolution $mods $fixture 'candidate.zip'}
Reject {& (Join-Path $PSScriptRoot 'Invoke-BrownFurStartupProbe.ps1') -PackagePath missing -DevelopmentPurpose brown-fur-startup-order -SaveName KMG_AUTOMATION_BASELINE -WhatIf}
Reject {& (Join-Path $PSScriptRoot 'Invoke-BrownFurStartupProbe.ps1') -PackagePath missing -DevelopmentPurpose wrong-purpose -SaveName KMG_AUTOMATION_WORKING -WhatIf}
Write-Host "Brown-Fur probe protocol PASS: $checks assertions; fixture files only."
