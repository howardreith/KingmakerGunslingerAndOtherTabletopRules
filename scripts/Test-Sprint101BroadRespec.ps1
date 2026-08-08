[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw (Join-Path $root 'scripts\RuntimeAutomation.Common.ps1')
$start = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerBroadRespec()')
$end = $runner.IndexOf('private RuntimeTestResult RunDisposableGunslingerGritResource()', $start)
if ($start -lt 0 -or $end -le $start) { throw 'Broad respec method unavailable.' }
$method = $runner.Substring($start, $end - $start)
$checks = [ordered]@{
  'save-free-scenario' = $catalog.Contains('DisposableGunslingerBroadRespec') -and $common.Contains("'disposable-gunslinger-broad-respec'")
  'archetype-reconciliation-matrix' = $catalog.Contains('DisposableArchetypeReconciliation') -and $common.Contains("'disposable-archetype-reconciliation'") -and $runner.Contains('base-to-pistolero') -and $runner.Contains('base-to-musket-master') -and $runner.Contains('pistolero-to-base') -and $runner.Contains('musket-master-to-base') -and $runner.Contains('pistolero-to-musket-master')
  'exact-global-callback' = $method.Contains('player.RespecCompanion(source')
  'exact-handler' = $runner.Contains('ILevelUpInitiateUIHandler') -and $runner.Contains('HandleLevelUpStart(UnitDescriptor unit')
  'handler-lifetime' = $method.Contains('EventBus.Subscribe(handler)') -and $method.Contains('EventBus.Unsubscribe(handler)')
  'native-commit' = $runner.Contains('commit.Invoke(Controller, null)')
  'native-archetype-selection' = $runner.Contains('SelectedArchetype') -and $runner.Contains('new[] { typeof(BlueprintArchetype) }') -and $runner.Contains('addArchetype.Invoke(Controller')
  'facts' = $method.Contains('descriptorsAlias') -and $method.Contains('sourceFighter == 0 && sourceGunslinger == 1') -and $method.Contains('replacementFighter == 0 && replacementGunslinger == 1')
  'guaranteed-cleanup' = $method.Contains('replacementEntity.Descriptor.Body != null') -and $method.Contains('source.Descriptor.Body != null') -and $method.Contains('SameReferences(crossBefore')
  'no-save' = -not $method.Contains('SaveGame')
}
$failed=@($checks.GetEnumerator() | Where-Object {-not $_.Value} | ForEach-Object Key)
if($failed.Count){throw "Sprint 101 broad respec failed: $($failed -join ', ')"}
"Sprint 101 broad respec tests passed: $($checks.Count)"
