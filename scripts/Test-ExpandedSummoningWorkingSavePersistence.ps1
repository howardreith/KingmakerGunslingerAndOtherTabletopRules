Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$catalog = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$request = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRequest.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs')
$smoke = Get-Content -Raw -LiteralPath (Join-Path $runtime 'WorkingSaveSmokeScenario.cs')
$publication = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Blueprints\ExpandedSummoningPublication.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $root `
    'scripts\RuntimeAutomation.Common.ps1')

$scenarios = @(
    'working-save-expanded-summoning-prepare',
    'working-save-expanded-summoning-verify-cleanup',
    'working-save-expanded-summoning-verify-absent'
)
$checks = [ordered]@{
    'three-phases-allowlisted' = $scenarios.Count -eq 3 -and
        @($scenarios | Where-Object { -not $catalog.Contains($_) }).Count -eq 0
    'working-save-request-policy-reused' =
        $request.Contains('WorkingSaveExpandedSummoningPrepare') -and
        $request.Contains('WorkingSaveExpandedSummoningVerifyCleanup') -and
        $request.Contains('WorkingSaveExpandedSummoningVerifyAbsent')
    'working-save-only-metadata' = @($scenarios | Where-Object {
            -not $common.Contains("'$_' = [pscustomobject]@{")
        }).Count -eq 0
    'exact-descriptor-arm' =
        $smoke.Contains('internal void ArmExactWorkingSaveWrite()') -and
        $smoke.Contains('ReferenceEquals(descriptor, _workingDescriptor)')
    'exact-save-routine-only' =
        $smoke.Contains('method.Name == "SaveRoutine"') -and
        $smoke.Contains('_expectedWorkingSaveRoutineCount == 1')
    'sprint-nine-flying-fixture' =
        $runner.Contains('new[] { "Monster", "eagle", "1" }') -and
        $runner.Contains('new[] { "NaturesAlly", "dire-bat", "3" }') -and
        $runner.Contains('expanded-summoning-persistent-eagle-bat-visuals') -and
        $runner.Contains('ReferenceEquals(value.View.Data, value)') -and
        $runner.Contains('IsEagleAttached(') -and
        $runner.Contains('IsDireBatAttached(')
    'fresh-load-identity-context-duration' =
        $runner.Contains('ReferenceEquals(unit.Blueprint, blueprint)') -and
        $runner.Contains('.SummonedUnitBuff)') -and
        $runner.Contains('value.MaybeContext.MaybeCaster == caster') -and
        $runner.Contains('value.TimeLeft <= TimeSpan.FromSeconds(127d)')
    'native-control-contract' =
        $runner.Contains('value.Commands != null') -and
        $runner.Contains('value.View.Data == value') -and
        $runner.Contains('ReferenceEquals(faction, ExpandedSummoningFields(')
    'enabled-disabled-publication-exact' =
        $publication.Contains('RequiredBasePublicationIsExact') -and
        $publication.Contains('if (count != expected) exact = false;') -and
        $runner.Contains('_context.FeatureModules.Active.ExpandedSummoning')
    'cleanup-and-final-absence' =
        $runner.Contains('.SystemMechanics.SummonedUnitBuff') -and
        $runner.Contains('foreach (Buff buff in summoned) buff.Remove();') -and
        $runner.Contains('unit.Destroy();') -and
        $runner.Contains('Game.Instance.EntityDestroyer.Tick();') -and
        $runner.Contains('_expandedSummoningPersistenceCleanupSettleUpdates++ < 5') -and
        $runner.Contains('postExpirationLive=') -and
        $runner.Contains('_expandedSummoningPersistenceCleanupValid = liveUnits == 0;') -and
        $runner.Contains('_expandedSummoningPersistenceCleanupValid = units.Length == 0;')
    'exact-native-save-invoked' =
        $runner.Contains('value.Name == "SaveGame"') -and
        $runner.Contains('_workingSaveSmoke.WorkingDescriptor')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Expanded Summoning working-save persistence tests failed: $($failed -join ', ')"
}
Write-Host "Expanded Summoning working-save persistence tests passed: $($checks.Count)"

# Exercise the request contract, not source spelling: the closed fixture scope
# must survive serialization, while every unrelated parameter remains denied.
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$requestBase = @{
    ExpectedVersion = '0.0.141'; TimeoutSeconds = 300
    CatalogTimeoutSeconds = 180; SelectionTimeoutSeconds = 300
    CompletionTimeoutSeconds = 180; MainMenuTimeoutSeconds = 180
    ActionResolutionTimeoutSeconds = 180; ActionInvocationTimeoutSeconds = 30
    DescriptorResolutionTimeoutSeconds = 30; LoadEntryTimeoutSeconds = 30
    FingerprintTimeoutSeconds = 180; ExitAfterCompletion = $true
    EvidenceDirectory = (Join-Path $script:KmgRuntimeEvidenceRoot 'crocodilian-persistence-request-test')
}
foreach ($scenario in $scenarios) {
    $targeted = New-KmgRuntimeRequest @requestBase -Scenario $scenario -Parameters @{
        saveName = 'KMG_AUTOMATION_WORKING'; persistenceScope = 'crocodilians' }
    if ($targeted.parameters.Count -ne 2 -or
        $targeted.parameters.saveName -cne 'KMG_AUTOMATION_WORKING' -or
        $targeted.parameters.persistenceScope -cne 'crocodilians') {
        throw "Targeted persistence scope did not round-trip for $scenario."
    }
    $historical = New-KmgRuntimeRequest @requestBase -Scenario $scenario -Parameters @{
        saveName = 'KMG_AUTOMATION_WORKING' }
    if ($historical.parameters.Count -ne 1) { throw 'The historical fixture was changed.' }
    foreach ($bad in @(
        @{ saveName = 'KMG_AUTOMATION_BASELINE'; persistenceScope = 'crocodilians' },
        @{ saveName = 'KMG_AUTOMATION_WORKING'; persistenceScope = 'Crocodilians' },
        @{ saveName = 'KMG_AUTOMATION_WORKING'; persistenceScope = 'wolf' },
        @{ saveName = 'KMG_AUTOMATION_WORKING'; persistenceScope = @('crocodilians') },
        @{ saveName = 'KMG_AUTOMATION_WORKING'; persistenceScope = 'crocodilians'; extra = 'untrusted' }
    )) {
        $rejected = $false
        try { $null = New-KmgRuntimeRequest @requestBase -Scenario $scenario -Parameters $bad }
        catch { $rejected = $true }
        if (-not $rejected) { throw "Targeted persistence accepted an invalid request for $scenario." }
    }
    $manualExit = $requestBase.Clone()
    $manualExit.ExitAfterCompletion = $false
    $rejected = $false
    try { $null = New-KmgRuntimeRequest @manualExit -Scenario $scenario -Parameters @{
        saveName = 'KMG_AUTOMATION_WORKING'; persistenceScope = 'crocodilians' } }
    catch { $rejected = $true }
    if (-not $rejected) { throw 'Targeted persistence accepted a non-exiting request.' }
}
$rejected = $false
try { $null = New-KmgRuntimeRequest @requestBase -Scenario 'working-save-smoke' -Parameters @{
    saveName = 'KMG_AUTOMATION_WORKING'; persistenceScope = 'crocodilians' } }
catch { $rejected = $true }
if (-not $rejected) { throw 'The targeted fixture scope leaked into another scenario.' }
Write-Host 'PASS crocodilian persistence request: three exact scopes, historical defaults and 19 fail-closed cases.'
