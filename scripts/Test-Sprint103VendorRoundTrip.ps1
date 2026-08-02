[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$runner=Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$checks=[ordered]@{
 'exact-stage'=$runner.Contains('vendor.AddForSell(batteredItem, 1)') -and $runner.Contains('EnumerateRuntimeInventory(vendor.ItemsForSell)')
 'exact-return'=$runner.Contains('vendor.RemoveFromSell(staged, 1)')
 'same-reference'=$runner.Contains('ReferenceEquals(staged, batteredItem)') -and $runner.Contains('ReferenceEquals(returned, batteredItem)')
 'origin'=$runner.Contains('ReferenceEquals(vendorOwner, mainDescriptor.Unit)')
 'exact-vendor'=$runner.Contains('FindVendorUnit(') -and $runner.Contains('CapitalVendorBlueprints.TableGuid')
 'capital-jhod'=$runner.Contains('c8d4913edee594749b706de35924617e')
 'begin-trading'=$runner.Contains('vendor.BeginTrading(vendorUnit)')
 'finally'=$runner.Contains('new[] { "ReturnItems" }')
 'end-trading'=$runner.Contains('vendor.EndTraiding()')
 'assertion'=$runner.Contains('"native-vendor-staging-roundtrip"')
 'sale-deal'=$runner.Contains('vendor.AddForSell(batteredItem, 1)') -and $runner.Contains('vendor.Deal()')
 'repurchase'=$runner.Contains('vendor.AddForBuy(stored, 1)') -and $runner.Contains('"native-vendor-deal-roundtrip"')
 'money-rollback'=$runner.Contains('long moneyDelta = moneyBefore - player.Money')
}
$failed=@($checks.GetEnumerator()|Where-Object{-not $_.Value}|ForEach-Object Key)
if($failed.Count){throw "Sprint 103 vendor roundtrip failed: $($failed -join ', ')"}
"Sprint 103 vendor roundtrip tests passed: $($checks.Count)"
