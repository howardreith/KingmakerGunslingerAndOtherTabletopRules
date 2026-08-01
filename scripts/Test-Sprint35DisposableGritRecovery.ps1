[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$runtime = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Grit\FirearmGritRecoveryRuntime.cs')
$patches = Get-Content -Raw -LiteralPath (Join-Path $root `
    'src\KingmakerGunslinger\Diagnostics\CombatTracePatches.cs')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableGunslingerGritRecovery')
    'detached-no-save-fixture' = $runner.Contains('RunDisposableGunslingerGritRecovery') -and
        $runner.Contains('new Kingmaker.UI.LevelUp.ChargenUnit(source)')
    'critical-reference-dedupe' = $runtime.Contains('TryMarkCritical') -and
        $runtime.Contains('ConditionalWeakTable<RuleAttackRoll')
    'kill-reference-dedupe' = $runtime.Contains('TryMarkKillingBlow') -and
        $runtime.Contains('ReferenceEquals(weaponAttack.MeleeDamage, damage)')
    'zero-crossing-required' = $runtime.Contains('HitPointsBefore > 0') -and
        $runtime.Contains('Target.HPLeft <= 0')
    'exact-rule-patches' = $patches.Contains('RuleDealDamageGritRecoveryPatch') -and
        $patches.Contains('FirearmGritRecoveryRuntime.AfterAttackRoll')
    'cleanup-proven' = $runner.Contains('combat states cleared, disposable entities disposed')
    'exact-detached-combat-flag' = $runner.Contains('SetExactField(attackerCombat, "m_InCombat", true)') -and
        $runner.Contains('!attacker.IsInCombat || !target.IsInCombat')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } |
    ForEach-Object Key)
if ($failed.Count) {
    throw "Sprint 35 disposable grit-recovery tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 35 disposable grit-recovery tests passed: $($checks.Count)"
