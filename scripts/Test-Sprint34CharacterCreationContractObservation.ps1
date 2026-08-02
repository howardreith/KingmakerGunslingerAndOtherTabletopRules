[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$catalog = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs')
$common = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$checks = [ordered]@{
    'scenario-allowlisted' = $catalog.Contains('ObserveCharacterCreationContracts') -and
        $catalog.Contains('"observe-character-creation-contracts"')
    'save-free-autonomous' = $common.Contains("'observe-character-creation-contracts' = [pscustomobject]") -and
        $common.Contains('RequiresSaveName = $false')
    'exact-types' = $runner.Contains('Kingmaker.UI.LevelUp.ChargenUnit') -and
        $runner.Contains('Kingmaker.Blueprints.Root.CharGenRoot') -and
        $runner.Contains('Kingmaker.Blueprints.BlueprintUnit') -and
        $runner.Contains('Kingmaker.UnitLogic.Class.LevelUp.LevelUpController') -and
        $runner.Contains('Kingmaker.UnitLogic.Class.LevelUp.Actions.SelectClass') -and
        $runner.Contains('Kingmaker.UnitLogic.Class.LevelUp.Actions.LevelUpHelper') -and
        $runner.Contains('Kingmaker.UnitLogic.UnitProgressionData') -and
        $runner.Contains('Kingmaker.UnitLogic.ClassData')
    'metadata-only' = $runner.Contains('GetConstructors(flags)') -and
        $runner.Contains('GetMethods(flags)') -and $runner.Contains('GetMembers(flags)') -and
        $runner.Contains('Enum.GetNames(type)') -and
        $runner.Contains('startWithoutStatic.GetParameters()[4].ParameterType')
    'exact-call-graph' = $runner.Contains('DescribeCalledMethods(') -and
        $runner.Contains('GetMethod("Commit"') -and
        $runner.Contains('GetMethod("SetupNewCharacher"') -and
        $runner.Contains('GetMethod("AddStartingInventory"') -and
        $runner.Contains('RequireExactApplyMethod(selectActionType)') -and
        $runner.Contains('RequireExactApplyMethod(mechanicsActionType)') -and
        $runner.Contains('GetMethod("<SetupNewCharacher>b__71_0"') -and
        $runner.Contains('value.Name == "AddStartingItems"') -and
        $runner.Contains('MethodBody IL call/callvirt tokens resolved without method invocation')
    'reads-rooted-unit-identities' = $runner.Contains('root.DefaultPlayerCharacter') -and
        $runner.Contains('root.CharGen.Pregens') -and $runner.Contains('DescribeBlueprintUnit')
    'exact-respec-call-graphs' = $runner.Contains('UnitDescriptor.PrepareRespec=') -and
        $runner.Contains('UnitEntityData.PrepareRespec=') -and
        $runner.Contains('Player.RespecCompanion=') -and
        $runner.Contains('UnitDescriptor.Body.set=') -and
        $runner.Contains('UnitDescriptor.Dispose=') -and
        $runner.Contains('UnitEntityData.Dispose=') -and
        $runner.Contains('LevelUpController.StartWithoutStatic=') -and
        $runner.Contains('LevelUpController.ctor=') -and
        $runner.Contains('LevelUpController.RequestPreview=') -and
        $runner.Contains('no respec or cleanup method invoked')
    'no-construction' = $runner.Contains('scenario invokes no constructor, method, save, input, or registry mutation')
}
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object Key)
if ($failed.Count) { throw "Sprint 34 character-creation contract observer tests failed: $($failed -join ', ')" }
Write-Host "Sprint 34 character-creation contract observer tests passed: $($checks.Count)"
