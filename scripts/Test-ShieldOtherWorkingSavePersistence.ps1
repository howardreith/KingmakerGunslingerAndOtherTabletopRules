Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$runtime = Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting'
$catalog = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestScenarioCatalog.cs')
$request = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRequest.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $runtime 'RuntimeTestRunner.cs')
$smoke = Get-Content -Raw -LiteralPath (Join-Path $runtime 'WorkingSaveSmokeScenario.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')

$checks = [ordered]@{
    'prepare-allowlisted' = $catalog.Contains('working-save-shield-other-prepare')
    'verify-cleanup-allowlisted' = $catalog.Contains('working-save-shield-other-verify-cleanup')
    'working-save-request-policy-reused' =
        $request.Contains('WorkingSaveShieldOtherPrepare') -and
        $request.Contains('WorkingSaveShieldOtherVerifyCleanup')
    'working-save-only-metadata' =
        $common.Contains("'working-save-shield-other-prepare' = [pscustomobject]@{") -and
        $common.Contains("'working-save-shield-other-verify-cleanup' = [pscustomobject]@{")
    'exact-descriptor-arm' =
        $smoke.Contains('internal void ArmExactWorkingSaveWrite()') -and
        $smoke.Contains('ReferenceEquals(descriptor, _workingDescriptor)')
    'exact-save-routine-only' =
        $smoke.Contains('method.Name == "SaveRoutine"') -and
        $smoke.Contains('_expectedWorkingSaveRoutineCount == 1')
    'exact-worker-area-write-only' =
        $smoke.Contains('method.Name == "SaveStashedArea"') -and
        $smoke.Contains('IsWorking(stashedDescriptor)') -and
        $smoke.Contains('!IsBaseline(stashedDescriptor)') -and
        $runner.Contains('evidence.ExpectedWorkingStashedAreaCount >= 1')
    'unexpected-write-still-fails' =
        $smoke.Contains('_writeObserved = true;') -and
        $runner.Contains('if (_workingSaveSmoke.WriteObserved)')
    'prepare-fixed-context' =
        $runner.Contains('context.Params.CasterLevel = 5;') -and
        $runner.Contains('TimeSpan.FromHours(5d)')
    'fresh-load-context-checked' =
        $runner.Contains('link.MaybeContext.MaybeCaster == caster') -and
        $runner.Contains('link.MaybeContext.MainTarget.Unit == subject')
    'odd-split-checked' =
        $runner.Contains('subjectBefore - subject.HPLeft == 1') -and
        $runner.Contains('casterBefore - caster.HPLeft == 2')
    'cleanup-before-save' =
        $runner.Contains('if (link != null) link.Remove();') -and
        $runner.Contains('!subject.Descriptor.HasFact(')
    'exact-native-save-invoked' =
        $runner.Contains('value.Name == "SaveGame"') -and
        $runner.Contains('_workingSaveSmoke.WorkingDescriptor')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count -ne 0) {
    throw "Shield Other working-save persistence tests failed: $($failed -join ', ')"
}
Write-Host "Shield Other working-save persistence tests passed: $($checks.Count)"
