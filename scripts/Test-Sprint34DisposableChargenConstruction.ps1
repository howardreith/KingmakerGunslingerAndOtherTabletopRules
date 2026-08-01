[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('DisposableDescriptorConstruction') -and $catalog.Contains('"disposable-descriptor-construction"')
    'save-free' = $common.Contains("'disposable-descriptor-construction' = [pscustomobject]") -and $common.Contains('RequiresSaveName = $false')
    'exact-source' = $runner.Contains('4391e8b9afbb0cf43aeba700c089f56d') -and
        $runner.Contains('typeof(Kingmaker.UnitLogic.UnitDescriptor)') -and
        $runner.Contains('new object[] { source }')
    'detached' = $runner.Contains('descriptor.Unit == null') -and $runner.Contains('!ContainsReference(party, descriptor)') -and $runner.Contains('!ContainsReference(allUnits, descriptor)')
    'finally-disposed' = $runner.Contains('finally') -and $runner.Contains('if (descriptor != null) descriptor.Dispose()')
    'snapshots-verified' = $runner.Contains('SameReferences(partyBefore') -and $runner.Contains('SameReferences(unitsBefore')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 34 disposable chargen construction tests failed: $($failed -join ', ')" }
Write-Host "Sprint 34 disposable chargen construction tests passed: $($checks.Count)"
