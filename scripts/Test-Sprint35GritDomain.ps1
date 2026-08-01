[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$service = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Grit\GritPoolService.cs')
$state = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\Grit\GritPoolState.cs')
$tests = Get-Content -Raw -LiteralPath (Join-Path $root 'tests\KingmakerGunslinger.DomainTests\Sprint35Tests.cs')
$checks = [ordered]@{
    'wisdom-minimum' = $service.Contains('Math.Max(1, wisdomModifier)')
    'daily-reset' = $service.Contains('ResetDaily') -and $service.Contains('maximum, maximum')
    'bounded-state' = $state.Contains('current < 0 || current > maximum')
    'maximum-reconcile' = $service.Contains('ReconcileMaximum') -and
        $service.Contains('Math.Min(state.Current, maximum)')
    'atomic-spend' = $service.Contains('GritTransactionStatus.Insufficient') -and
        $service.Contains('state.Current - cost')
    'capped-restore' = $service.Contains('GritTransactionStatus.AtMaximum') -and
        $service.Contains('Math.Min((long)state.Maximum')
    'operation-dedupe' = $service.Contains('GritTransactionStatus.Duplicate') -and
        $service.Contains('gate.MarkApplied(operationId)')
    'focused-tests' = $tests.Contains('GritDuplicateSpendRejected') -and
        $tests.Contains('GritDuplicateRestoreRejected') -and
        $tests.Contains('GritUnitGatesAreIsolated')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) {
    throw "Sprint 35 grit domain tests failed: $($failed -join ', ')"
}
Write-Host "Sprint 35 grit domain tests passed: $($checks.Count)"
