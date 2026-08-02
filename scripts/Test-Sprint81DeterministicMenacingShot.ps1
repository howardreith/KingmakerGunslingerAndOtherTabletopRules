[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$start = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerMenacingShot()')
$end = $runner.IndexOf('private static void AdvanceDisposableGunslinger', $start)
if ($start -lt 0 -or $end -le $start) { throw 'Menacing Shot runtime method was not found.' }
$method = $runner.Substring($start, $end - $start)
$checks = [ordered]@{
    'failure-native-one' = $method.Contains('UnityEngine.Random.InitState(FindNativeD20Seed(1));')
    'success-native-twenty' = $method.Contains('UnityEngine.Random.InitState(FindNativeD20Seed(20));')
    'production-unchanged' = $method.Contains('effect.Apply(failedContext') -and
        $method.Contains('effect.Apply(passedContext')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 81 deterministic Menacing Shot tests failed: $($failed -join ', ')" }
Write-Host "Sprint 81 deterministic Menacing Shot tests passed: $($checks.Count)"
