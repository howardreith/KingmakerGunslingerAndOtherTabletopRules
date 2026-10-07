<#
Exercises the actual outer launcher's typed-save/parameter routing in WhatIf.
No runtime lease, live-install observation, deployment, evidence directory or
game launch is permitted. Requires this lab's ordinary unelevated Steam setup.
AllowDirtyGit is scoped to these source-only WhatIf calls, never a live run.
#>
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$root = Split-Path -Parent $PSScriptRoot
$version = (Get-KmgModInfo -RepositoryRoot $root).Version
$launcher = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$stage = Join-Path $root 'artifacts\deploy-staging'
$lock = 'C:\Dev\KingmakerGunslingerLab\compatibility-state\compatibility.lock'
if (@(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -ne 0) {
    throw 'Launcher WhatIf checks require no running Kingmaker.'
}
$beforeEvidence = @(Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory | ForEach-Object FullName)
$beforeStage = Test-Path -LiteralPath $stage
$beforeLock = Test-Path -LiteralPath $lock
$valid = 0
$rejected = 0
function Invoke-SourceOnlyLauncher([string]$Scenario, [hashtable]$Parameters,
    [string]$Save = 'KMG_AUTOMATION_WORKING', [bool]$ExitAutomatically = $true) {
    & $launcher -Scenario $Scenario -ExpectedVersion $version -SaveName $Save -Parameters $Parameters `
        -ExitAfterCompletion:$ExitAutomatically -AllowDirtyGit -WhatIf -Confirm:$false
}
foreach ($scenario in @('working-save-expanded-summoning-prepare',
    'working-save-expanded-summoning-verify-cleanup', 'working-save-expanded-summoning-verify-absent')) {
    Invoke-SourceOnlyLauncher $scenario @{}
    $valid++
    foreach ($scope in @('crocodilians', 'snakes')) {
        Invoke-SourceOnlyLauncher $scenario @{ persistenceScope = $scope }
        $valid++
    }
}
foreach ($roster in @('crocodile,dire-crocodile', 'viper,constrictor-snake')) {
    Invoke-SourceOnlyLauncher 'working-save-expanded-summoning-creature-review' @{ creatures = $roster }
    $valid++
    Invoke-SourceOnlyLauncher 'working-save-expanded-summoning-creature-review' @{
        creatures = $roster; quantity = 'OneD4PlusOne' }
    $valid++
}
$prepare = 'working-save-expanded-summoning-prepare'
$crowd = 'working-save-expanded-summoning-creature-review'
$badCases = @(
    @{ Scenario=$prepare; Parameters=@{persistenceScope='snakes'}; Save='KMG_AUTOMATION_BASELINE' },
    @{ Scenario=$prepare; Parameters=@{persistenceScope='Snakes'} },
    @{ Scenario=$prepare; Parameters=@{persistenceScope='salamander'} },
    @{ Scenario=$prepare; Parameters=@{persistenceScope=@('snakes')} },
    @{ Scenario=$prepare; Parameters=@{persistenceScope='snakes';extra='untrusted'} },
    @{ Scenario=$prepare; Parameters=@{persistenceScope='snakes'}; ExitAutomatically=$false },
    @{ Scenario=$crowd; Parameters=@{creatures='viper';quantity='OneD3'} },
    @{ Scenario=$crowd; Parameters=@{creatures='viper,salamander';quantity='OneD4PlusOne'} },
    @{ Scenario=$crowd; Parameters=@{creatures='Viper';quantity='OneD4PlusOne'} },
    @{ Scenario='working-save-smoke'; Parameters=@{persistenceScope='snakes'} }
)
foreach ($bad in $badCases) {
    $didReject = $false
    try { Invoke-SourceOnlyLauncher @bad }
    catch { $didReject = $true }
    if (-not $didReject) { throw 'Actual launcher accepted an invalid scoped request.' }
    $rejected++
}
$afterEvidence = @(Get-ChildItem -LiteralPath $script:KmgRuntimeEvidenceRoot -Directory | ForEach-Object FullName)
if (@(Compare-Object $beforeEvidence $afterEvidence).Count -ne 0 -or
    (Test-Path -LiteralPath $stage) -ne $beforeStage -or
    (Test-Path -LiteralPath $lock) -ne $beforeLock -or
    @(Get-Process -Name Kingmaker -ErrorAction SilentlyContinue).Count -ne 0) {
    throw 'Source-only launcher routing changed runtime evidence, lease, staging or processes.'
}
Write-Host "PASS actual launcher WhatIf: $valid valid routes, $rejected rejected routes; no runtime mutation."
