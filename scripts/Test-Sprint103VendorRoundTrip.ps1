[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$runner=Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$checks=[ordered]@{
 'exact-stage'=$runner.Contains('vendor.AddForBuy(batteredItem, 1)')
 'exact-return'=$runner.Contains('vendor.RemoveFromBuy(staged, 1)')
 'same-reference'=$runner.Contains('ReferenceEquals(staged, batteredItem)') -and $runner.Contains('ReferenceEquals(returned, batteredItem)')
 'origin'=$runner.Contains('ReferenceEquals(vendorOwner, mainDescriptor.Unit)')
 'finally'=$runner.Contains('new[] { "ReturnItems" }')
 'assertion'=$runner.Contains('"native-vendor-staging-roundtrip"')
}
$failed=@($checks.GetEnumerator()|Where-Object{-not $_.Value}|ForEach-Object Key)
if($failed.Count){throw "Sprint 103 vendor roundtrip failed: $($failed -join ', ')"}
"Sprint 103 vendor roundtrip tests passed: $($checks.Count)"
