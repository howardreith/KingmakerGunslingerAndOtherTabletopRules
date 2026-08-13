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
    'fixed-two-unit-fixture' =
        $runner.Contains('"small-air-elemental"') -and
        $runner.Contains('value.Creature.Key == "wolf"') -and
        $runner.Contains('ExpandedSummoningRuleCapture.Count != 2') -and
        $runner.Contains('Game.Instance.EntityCreator.Tick();') -and
        $runner.Contains('ReferenceEquals(value.HoldingState, caster.HoldingState)')
    'fresh-load-identity-context-duration' =
        $runner.Contains('ReferenceEquals(unit.Blueprint, blueprint)') -and
        $runner.Contains('value.MaybeContext.MaybeCaster == caster') -and
        $runner.Contains('value.TimeLeft <= TimeSpan.FromSeconds(121d)')
    'native-control-contract' =
        $runner.Contains('value.Commands != null') -and
        $runner.Contains('value.View.Data == value') -and
        $runner.Contains('ReferenceEquals(faction, ExpandedSummoningFields(')
    'enabled-disabled-publication-exact' =
        $publication.Contains('RequiredBasePublicationIsExact') -and
        $publication.Contains('count != (expectedEnabled ? 1 : 0)') -and
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
