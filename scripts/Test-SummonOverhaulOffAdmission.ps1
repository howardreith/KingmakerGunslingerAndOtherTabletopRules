[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$good = @{Scenario=@('disposable-expanded-summoning-sprint32-native-review'); SaveName='';
    ScenarioParameters=@{reviewPhase='summon-overhaul-off'};ScenarioParameterMap=@{};
    DevelopmentPurpose='summon-outsider-review-corrections'}
if (!(Test-KmgTemporaryExpandedSummoningOffScope @good)) { throw 'Exact OFF correction refused.' }
if (!(Test-KmgTemporaryExpandedSummoningOffScope -Scenario @('working-save-expanded-summoning-verify-cleanup') -SaveName KMG_AUTOMATION_WORKING)) { throw 'Released cleanup route regressed.' }
$spider = @{Scenario=@('disposable-expanded-summoning-sprint32-native-review'); SaveName='';
    ScenarioParameters=@{reviewPhase='spider-module-off'};ScenarioParameterMap=@{};
    DevelopmentPurpose='races-unleashed-summoning'}
if (!(Test-KmgTemporaryExpandedSummoningOffScope @spider)) { throw 'Exact Spider Summoner OFF control refused.' }
foreach ($cross in @(
    @{ScenarioParameters=@{reviewPhase='summon-overhaul-off'}},
    @{ScenarioParameters=@{reviewPhase='spider-web'}},
    @{ScenarioParameters=@{reviewPhase='spider-mechanics'}},
    @{ScenarioParameters=@{reviewPhase='summon-cleric-alignment'}},
    @{DevelopmentPurpose='summon-outsider-review-corrections'},
    @{DevelopmentPurpose='Races-Unleashed-Summoning'},
    @{SaveName='KMG_AUTOMATION_WORKING'})) {
    $probe=$spider.Clone(); foreach ($name in $cross.Keys) {$probe[$name]=$cross[$name]}
    if (Test-KmgTemporaryExpandedSummoningOffScope @probe) { throw ('Cross-paired OFF request admitted: '+($cross|ConvertTo-Json -Compress)) }
}
$crossGood=$good.Clone(); $crossGood.ScenarioParameters=@{reviewPhase='spider-module-off'}
if (Test-KmgTemporaryExpandedSummoningOffScope @crossGood) { throw 'Outsider purpose admitted the spider OFF phase.' }
$cases = @(
    @{Scenario=@('disposable-expanded-summoning-sprint32-native-review','working-save-expanded-summoning-verify-cleanup')},
    @{Scenario=@('disposable-expanded-summoning-cleanup2-native-review')},
    @{Scenario=@('Disposable-expanded-summoning-sprint32-native-review')},
    @{SaveName='KMG_AUTOMATION_WORKING'}, @{SaveName='KMG_AUTOMATION_BASELINE'},
    @{ScenarioParameters=@{reviewPhase='summon-overhaul-daylight'}},
    @{ScenarioParameters=@{reviewPhase='summon-wall-placement-preflight'}},
    @{ScenarioParameters=@{reviewPhase='summon-overhaul-off '}},
    @{ScenarioParameters=@{reviewPhase='summon-overhaul-off'; extra='x'}},
    @{ScenarioParameters=@{}}, @{ScenarioParameterMap=@{'x'=@{reviewPhase='summon-overhaul-off'}}},
    @{ScenarioParameters=@{ReviewPhase='summon-overhaul-off'}},
    @{ScenarioParameters=@{reviewPhase=@('summon-overhaul-off')}},
    @{DevelopmentPurpose='summon-outsider-visual-overhaul'}, @{DevelopmentPurpose=''}
)
foreach ($change in $cases) {
    $probe=$good.Clone(); foreach ($name in $change.Keys) {$probe[$name]=$change[$name]}
    if (Test-KmgTemporaryExpandedSummoningOffScope @probe) { throw ('Foreign OFF request admitted: '+($change|ConvertTo-Json -Compress)) }
}
foreach ($phase in @('summon-overhaul-indoor','summon-overhaul-daylight','summon-overhaul-motion','summon-overhaul-wall','summon-overhaul-off','summon-wall-placement-preflight','spider-web','spider-mechanics','spider-module-off')) {
    if (!(Test-KmgSprint32ReviewPhaseScope -Scenario 'disposable-expanded-summoning-sprint32-native-review' -Parameters @{reviewPhase=$phase})) { throw "Exact phase rejected: $phase" }
}
Write-Host 'PASS: exact OFF pairs and released cleanup admitted; fifteen foreign and eight cross-paired OFF requests rejected; nine exact phases recognized. No runtime launch.'
