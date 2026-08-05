[CmdletBinding()]
param(
    [string]$ReferenceBundleDir = 'C:\Dev\KingmakerGunslingerLab\private\extracted-references\KingmakerGunslinger-private-build-references'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$managed = Join-Path $ReferenceBundleDir 'Managed'
$resolver = [ResolveEventHandler]{
    param($sender, $eventArgs)
    $name = ([Reflection.AssemblyName]$eventArgs.Name).Name + '.dll'
    foreach ($candidate in @(
        (Join-Path $managed $name),
        (Join-Path (Join-Path $managed 'UnityModManager') $name))) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return [Reflection.Assembly]::LoadFrom($candidate)
        }
    }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try {
    $game = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'Assembly-CSharp.dll'))
    $unity = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'UnityEngine.CoreModule.dll'))
    $triggerType = $game.GetType(
        'Kingmaker.Designers.Mechanics.Facts.NewRoundTrigger', $true)
    $tickEachRound = $game.GetType(
        'Kingmaker.Controllers.Units.ITickEachRound', $true)
    $roundHandler = $game.GetType(
        'Kingmaker.PubSubSystem.IUnitNewCombatRoundHandler', $true)
    $removeType = $game.GetType(
        'Kingmaker.UnitLogic.Mechanics.Actions.ContextActionRemoveSelf', $true)
    $actionListType = $game.GetType('Kingmaker.ElementsSystem.ActionList', $true)
    $gameActionType = $game.GetType('Kingmaker.ElementsSystem.GameAction', $true)
    $unitType = $game.GetType('Kingmaker.EntitySystem.Entities.UnitEntityData', $true)

    $newRoundActions = $triggerType.GetField('NewRoundActions')
    if (-not $newRoundActions -or $newRoundActions.FieldType -ne $actionListType) {
        throw 'Expected public ActionList NewRoundActions.'
    }
    $actions = $actionListType.GetField('Actions')
    if (-not $actions -or -not $actions.FieldType.IsArray -or
        $actions.FieldType.GetElementType() -ne $gameActionType) {
        throw 'Expected public GameAction[] ActionList.Actions.'
    }
    $handler = $triggerType.GetMethod('HandleNewCombatRound', [Type[]]@($unitType))
    if (-not $handler -or $handler.ReturnType -ne [void] -or
        -not $roundHandler.IsAssignableFrom($triggerType)) {
        throw 'NewRoundTrigger does not implement the installed native combat-round handler.'
    }
    if ($tickEachRound.GetMethod('OnNewRound').ReturnType -ne [void]) {
        throw 'Expected ITickEachRound.OnNewRound() : void.'
    }
    if ($tickEachRound.IsAssignableFrom($triggerType)) {
        throw 'Installed NewRoundTrigger unexpectedly implements ITickEachRound directly.'
    }
    $runAction = $removeType.GetMethod('RunAction', [Type[]]@())
    if (-not $runAction -or $runAction.ReturnType -ne [void] -or
        -not $gameActionType.IsAssignableFrom($removeType)) {
        throw 'Expected ContextActionRemoveSelf : GameAction with void RunAction().'
    }

    # Unity's native CreateInstance internal call is unavailable in this
    # headless reflection process. Public parameterless construction proves
    # the installed managed graph without invoking the Unity runtime.
    $trigger = [Runtime.Serialization.FormatterServices]::GetUninitializedObject(
        $triggerType)
    $remove = [Runtime.Serialization.FormatterServices]::GetUninitializedObject(
        $removeType)
    $list = [Activator]::CreateInstance($actionListType)
    $array = [Array]::CreateInstance($gameActionType, 1)
    $array.SetValue($remove, 0)
    $actions.SetValue($list, $array)
    $newRoundActions.SetValue($trigger, $list)
    if ($newRoundActions.GetValue($trigger) -ne $list -or
        $actions.GetValue($list).Length -ne 1 -or
        $actions.GetValue($list).GetValue(0).GetType() -ne $removeType) {
        throw 'Native Dodge round-removal graph did not retain exactly one remove-self action.'
    }
    Write-Host ('Exact-reference Dodge round graph passed: ' +
        'NewRoundTrigger : IUnitNewCombatRoundHandler; ' +
        'public ActionList NewRoundActions; ' +
        'ITickEachRound.OnNewRound() is a separate installed contract; ' +
        'ContextActionRemoveSelf : GameAction with void RunAction(); one action instantiated.')
}
finally {
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver)
}
