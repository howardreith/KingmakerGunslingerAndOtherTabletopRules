[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$version = (Get-KmgModInfo -RepositoryRoot $root).Version
$progressPath = Join-Path $root 'artifacts/sprint28-save-preflight-progress.txt'
[IO.File]::WriteAllText($progressPath, "start`n")
$count = 0
function Assert-S28([bool]$Condition, [string]$Name) {
    $script:count++
    if (-not $Condition) { throw "Sprint 28 persistence preflight failed: $Name" }
}
function Refuse-S28([scriptblock]$Action, [string]$Name) {
    $script:count++
    try { & $Action } catch { return }
    throw "Sprint 28 persistence preflight accepted invalid request: $Name"
}

foreach ($phase in @('working-save-expanded-summoning-prepare',
    'working-save-expanded-summoning-verify-cleanup',
    'working-save-expanded-summoning-verify-absent')) {
    $s28RequestArgs = @{ Scenario = $phase; ExpectedVersion = $version;
        TimeoutSeconds = 600; StartupTimeoutSeconds = 180;
        CatalogTimeoutSeconds = 180; SelectionTimeoutSeconds = 300;
        CompletionTimeoutSeconds = 600; MainMenuTimeoutSeconds = 180;
        ActionResolutionTimeoutSeconds = 180;
        ActionInvocationTimeoutSeconds = 30;
        DescriptorResolutionTimeoutSeconds = 30;
        LoadEntryTimeoutSeconds = 30;
        FingerprintTimeoutSeconds = 180; ExitAfterCompletion = $true;
        Parameters = @{ saveName = 'KMG_AUTOMATION_WORKING';
            persistenceScope = 'giant-humanoids' } }
    $metadata = Assert-KmgRuntimeScenarioPreflight @s28RequestArgs
    Assert-S28 ($metadata.RequiresSaveName -and
        $metadata.PermittedSaveName -ceq 'KMG_AUTOMATION_WORKING' -and
        -not $metadata.RequiresManualInteraction) "guarded metadata $phase"
    $request = New-KmgRuntimeRequest @s28RequestArgs -EvidenceDirectory (
        Join-Path $script:KmgRuntimeEvidenceRoot 'sprint28-save-preflight')
    Assert-S28 ($request.parameters.saveName -ceq 'KMG_AUTOMATION_WORKING' -and
        $request.parameters.persistenceScope -ceq 'giant-humanoids' -and
        $request.exitAfterCompletion) "exact request serialization $phase"
    $s28RequestArgs.Parameters.persistenceScope = 'Giant-humanoids'
    Refuse-S28 { Assert-KmgRuntimeScenarioPreflight @s28RequestArgs } "case $phase"
    $s28RequestArgs.Parameters.persistenceScope = 'giant-humanoids'
    $s28RequestArgs.ExitAfterCompletion = $false
    Refuse-S28 { Assert-KmgRuntimeScenarioPreflight @s28RequestArgs } "exit $phase"
    $s28RequestArgs.ExitAfterCompletion = $true
    $s28RequestArgs.Parameters.saveName = 'KMG_AUTOMATION_BASELINE'
    Refuse-S28 { Assert-KmgRuntimeScenarioPreflight @s28RequestArgs } "baseline $phase"
    $s28RequestArgs.Parameters.saveName = 'KMG_AUTOMATION_WORKING'
    $s28RequestArgs.Parameters.extra = $true
    Refuse-S28 { Assert-KmgRuntimeScenarioPreflight @s28RequestArgs } "extra $phase"
    [IO.File]::AppendAllText($progressPath, "pure $phase`n")
}

[IO.File]::AppendAllText($progressPath, "outer start`n")
$outer = & (Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1') `
    -Scenario 'working-save-expanded-summoning-prepare' `
    -ExpectedVersion $version -SaveName 'KMG_AUTOMATION_WORKING' `
    -Parameters @{ persistenceScope = 'giant-humanoids' } `
    -TimeoutSeconds 600 -ExitAfterCompletion:$true -AllowDirtyGit `
    -WhatIf -Confirm:$false 6>&1 | Out-String
[IO.File]::AppendAllText($progressPath, "outer returned`n")
Assert-S28 ($outer.Contains('Source-only/WhatIf validation passed.')) `
    'actual guarded outer dispatch reached WhatIf without a game'
Write-Host "Sprint 28 persistence preflight PASS=$count; no game or save access."
