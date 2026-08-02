[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$runner=Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$checks=[ordered]@{
  'native-extract-add'=$runner.Contains('new[] { "Extract" }') -and $runner.Contains('new[] { "Add" }')
  'same-item-owner'=$runner.Contains('BatteredFirearmOriginRuntime.TryGetOwner') -and $runner.Contains('ReferenceEquals(transferredOwner, mainDescriptor.Unit)')
  'return-path'=$runner.Contains('transferredAway = false')
  'finally-repair'=$runner.Contains('if (transferredAway && transferUnit != null')
  'runtime-assertion'=$runner.Contains('"native-inventory-transfer"')
}
$failed=@($checks.GetEnumerator()|Where-Object{-not $_.Value}|ForEach-Object Key)
if($failed.Count){throw "Sprint 102 transfer failed: $($failed -join ', ')"}
"Sprint 102 transfer tests passed: $($checks.Count)"
