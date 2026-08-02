[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$trace = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Diagnostics\CombatTracePatches.cs')
$misfire = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Misfires\FirearmMisfirePatches.cs')
$armor = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Rules\FirearmArmorClassRuntime.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$tests = Get-Content -Raw -LiteralPath (Join-Path $root 'tests\KingmakerGunslinger.DomainTests\Program.cs')
$checks = [ordered]@{
    'native-weapon-event-observed' = $trace.Contains('RuleAttackWithWeaponTracePatch') -and
        $trace.Contains('CombatTraceRuntime.Before(CombatTraceStage.WeaponAttack') -and
        $trace.Contains('CombatTraceRuntime.After(CombatTraceStage.WeaponAttack')
    'native-attack-bracket-retained' = $trace.Contains('RuleAttackRollFirearmPatch') -and
        $trace.Contains('private static void Prefix(object __instance)') -and
        $trace.Contains('private static void Postfix(object __instance)')
    'misfire-postfix-only-failure' = $misfire.Contains('private static void Postfix(') -and
        $misfire.Contains('ref bool __result') -and
        -not $misfire.Contains('return true;')
    'contextual-ac-postfix' = $trace.Contains('RuleCalculateAcFirearmPatch') -and
        $trace.Contains('FirearmArmorClassRuntime.AfterCalculateArmorClass(__instance)') -and
        $armor.Contains('currentTargetArmorClass') -and
        $armor.Contains('decision.SelectedTargetArmorClass')
    'contextual-ac-regressions' = $tests.Contains('ArmorClassDeadeyePreservesContext') -and
        $tests.Contains('ArmorClassPreservesCoverAdjustment') -and
        $tests.Contains('ArmorClassPreservesFlatFootedAdjustment')
    'native-damage-and-critical-runtime' = $runner.Contains('native RuleAttackWithWeapon and firearm pipeline') -and
        $runner.Contains('ordinary hit consumes one chamber and deals native damage') -and
        $runner.Contains('RunDisposableGunslingerTargetingTorso')
    'comprehensive-composition' = $runner.Contains('RunDisposableGunslingerComprehensiveAcceptance') -and
        $runner.Contains('RunDisposableGunslingerDeadeye') -and
        $runner.Contains('RunDisposableGunslingerStunningShot(false)')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 82 native pipeline tests failed: $($failed -join ', ')" }
Write-Host "Sprint 82 native pipeline tests passed: $($checks.Count)"
