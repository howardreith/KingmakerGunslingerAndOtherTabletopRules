[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-save-free' = $catalog.Contains('DisposableGunslingerMulticlassCommit') -and
        $common.Contains("'disposable-gunslinger-multiclass-commit'")
    'fighter-seed' = $runner.Contains('new object[] { fighter, false }') -and
        $runner.Contains('fighterSeeded == 1')
    'native-commit' = $runner.Contains('commit.Invoke(multiclass, null)') -and
        $runner.Contains('committedGunslinger == 1 && callback')
    'level-one-facts' = $runner.Contains('proficiencies && grit')
    'expanded-isolation' = $runner.Contains('SameReferences(remoteBefore') -and
        $runner.Contains('SameReferences(crossBefore') -and
        $runner.Contains('SameReferences(inventoryBefore')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 73 multiclass commit tests failed: $($failed -join ', ')" }
Write-Host "Sprint 73 multiclass commit tests passed: $($checks.Count)"
