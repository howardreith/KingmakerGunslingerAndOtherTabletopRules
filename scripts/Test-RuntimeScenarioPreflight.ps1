[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$commonPath = Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1'
$orchestratorPath = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$catalogPath = Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs'
. $commonPath

$failures = [Collections.Generic.List[string]]::new()
$checks = 0
function Assert-True([bool]$Condition, [string]$Name) {
    $script:checks++
    if (-not $Condition) { $script:failures.Add($Name) }
}
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    $script:checks++
    try { & $Action; $script:failures.Add($Name) } catch { }
}

$expected = @(
    'mod-load-smoke',
    'observe-manual-save-load',
    'observe-save-catalog-and-selection',
    'observe-save-catalog-provider',
    'observe-load-game-button-action',
    'working-save-smoke',
    'observe-working-save-entry-action'
)
$catalog = Get-Content -LiteralPath $catalogPath -Raw
$csharpNames = @([regex]::Matches($catalog, '"([a-z][a-z-]+)"') |
    ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
$powershellNames = @($script:KmgRuntimeScenarios | Sort-Object)
Assert-True (($csharpNames -join "`n") -ceq ($powershellNames -join "`n")) `
    'csharp-powershell-catalog-sync'
Assert-True (($expected | Sort-Object) -join "`n" -ceq
    ($powershellNames -join "`n")) 'documented-scenarios-retained'

$entry = Get-KmgRuntimeScenarioMetadata 'observe-working-save-entry-action'
Assert-True $entry.RequiresManualInteraction 'entry-requires-manual-interaction'
Assert-True $entry.RequiresSaveName 'entry-requires-save-name'
Assert-True ($entry.PermittedSaveName -ceq 'KMG_AUTOMATION_WORKING') `
    'entry-only-permits-working-save'

$valid = @{
    Scenario = 'observe-working-save-entry-action'
    ExpectedVersion = '0.0.30'
    TimeoutSeconds = 120
    StartupTimeoutSeconds = 180
    CatalogTimeoutSeconds = 180
    SelectionTimeoutSeconds = 300
    CompletionTimeoutSeconds = 180
    MainMenuTimeoutSeconds = 180
    ActionResolutionTimeoutSeconds = 180
    ActionInvocationTimeoutSeconds = 30
    DescriptorResolutionTimeoutSeconds = 30
    LoadEntryTimeoutSeconds = 30
    FingerprintTimeoutSeconds = 180
    Parameters = @{ saveName = 'KMG_AUTOMATION_WORKING' }
    EnforceManualInteraction = $true
    ManualInteractionRequired = $true
}
Assert-True ($null -ne (Assert-KmgRuntimeScenarioPreflight @valid)) `
    'valid-entry-reaches-request-validation'

$missingSave = $valid.Clone()
$missingSave.Parameters = @{}
Assert-Throws { Assert-KmgRuntimeScenarioPreflight @missingSave } `
    'missing-save-fails-pure-preflight'
$baseline = $valid.Clone()
$baseline.Parameters = @{ saveName = 'KMG_AUTOMATION_BASELINE' }
Assert-Throws { Assert-KmgRuntimeScenarioPreflight @baseline } `
    'baseline-rejected-pure-preflight'
$missingManual = $valid.Clone()
$missingManual.ManualInteractionRequired = $false
Assert-Throws { Assert-KmgRuntimeScenarioPreflight @missingManual } `
    'missing-manual-fails-pure-preflight'
Assert-Throws {
    Assert-KmgRuntimeScenarioPreflight -Scenario 'unsupported-regression-fixture' `
        -ExpectedVersion '0.0.30' -TimeoutSeconds 120
} 'unsupported-fails-pure-preflight'
Assert-Throws {
    Assert-KmgRuntimeScenarioPreflight -Scenario 'mod-load-smoke' `
        -ExpectedVersion '30' -TimeoutSeconds 120
} 'malformed-version-fails-pure-preflight'

$orchestrator = Get-Content -LiteralPath $orchestratorPath -Raw
$preflightIndex = $orchestrator.IndexOf('Assert-KmgRuntimeScenarioPreflight')
foreach ($boundary in @("'Get-KmgRepositoryRoot", "'Build-Local.ps1'",
    "'Deploy-Local.ps1'", 'New-Item -ItemType Directory',
    'Initialize-KmgRuntimeTestEvidence', 'Start-KmgSteamKingmaker')) {
    $index = $orchestrator.IndexOf($boundary.TrimStart("'"))
    Assert-True ($preflightIndex -ge 0 -and $index -gt $preflightIndex) `
        "preflight-before-$boundary"
}
Assert-True (-not $orchestrator.Contains('Wait-KmgSteamProcess -SteamPath $SteamPath')) `
    'no-predeployment-steam-start'
Assert-True (-not $orchestrator.Contains('Kingmaker.exe')) `
    'direct-kingmaker-launch-rejected'

$common = Get-Content -LiteralPath $commonPath -Raw
Assert-True ($common.Contains('$script:KmgRuntimeScenarioMetadata = [ordered]@{')) `
    'one-authoritative-powershell-metadata-table'
Assert-True ($common.Contains("'observe-working-save-entry-action' = [pscustomobject]@{")) `
    'entry-present-in-authoritative-metadata'
Assert-True ($orchestrator.Contains('$scenarioMetadata = Get-KmgRuntimeScenarioMetadata')) `
    'orchestrator-consumes-authoritative-metadata'

$artifactRoot = Join-Path $root 'artifacts'
$backupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod'
$evidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
function Get-TreeFingerprint([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return '<missing>' }
    return (@(Get-ChildItem -LiteralPath $Path -Recurse -Force |
        Sort-Object FullName | ForEach-Object {
            $length = if ($_.PSIsContainer) { 0 } else { $_.Length }
            '{0}|{1}|{2}' -f $_.FullName, $length, $_.LastWriteTimeUtc.Ticks
        }) -join "`n")
}
function Get-DirectoryIdentity([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return '<missing>' }
    return (@(Get-ChildItem -LiteralPath $Path -Directory -Recurse -Force |
        ForEach-Object FullName | Sort-Object) -join "`n")
}
$artifactBefore = Get-TreeFingerprint $artifactRoot
$backupBefore = Get-DirectoryIdentity $backupRoot
$evidenceBefore = Get-DirectoryIdentity $evidenceRoot
$script:cimCalls = 0
$script:startProcessCalls = 0
function global:Get-CimInstance { $script:cimCalls++; throw 'Unexpected CIM call.' }
function global:Start-Process { $script:startProcessCalls++; throw 'Unexpected process launch.' }
try {
    Assert-Throws {
        & $orchestratorPath -Scenario 'unsupported-regression-fixture' `
            -ExpectedVersion '0.0.30' -WhatIf -Confirm:$false
    } 'original-defect-fixture-rejected'
}
finally {
    Remove-Item Function:\global:Get-CimInstance
    Remove-Item Function:\global:Start-Process
}
Assert-True ((Get-TreeFingerprint $artifactRoot) -ceq $artifactBefore) `
    'unsupported-does-not-build-or-stage-package'
Assert-True ((Get-DirectoryIdentity $backupRoot) -ceq $backupBefore) `
    'unsupported-creates-no-backup'
Assert-True ((Get-DirectoryIdentity $evidenceRoot) -ceq $evidenceBefore) `
    'unsupported-creates-no-deployment-or-evidence'
Assert-True ($script:cimCalls -eq 0) 'unsupported-performs-no-cim'
Assert-True ($script:startProcessCalls -eq 0) `
    'unsupported-launches-neither-steam-nor-kingmaker'

if ($failures.Count -ne 0) {
    throw "Runtime scenario preflight tests failed: $($failures -join ', ')"
}
Write-Host "Runtime scenario preflight tests passed: $checks"
