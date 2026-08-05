[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$ModAssemblyPath,
    [string]$ReferenceBundleDir = 'C:\Dev\KingmakerGunslingerLab\private\extracted-references\KingmakerGunslinger-private-build-references',
    [string]$KingmakerInstallDir = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$managed = Join-Path $ReferenceBundleDir 'Managed'
$resolver = [ResolveEventHandler]{
    param($sender, $eventArgs)
    $name = ([Reflection.AssemblyName]$eventArgs.Name).Name + '.dll'
    $candidate = Get-ChildItem -LiteralPath $managed -Filter $name -Recurse -File | Select-Object -First 1
    if (-not $candidate) { $candidate = Get-ChildItem -LiteralPath (Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed') -Filter $name -Recurse -File | Select-Object -First 1 }
    if ($candidate) { return [Reflection.Assembly]::LoadFrom($candidate.FullName) }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try {
    [Reflection.Assembly]::LoadFrom((Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed\UnityModManager\0Harmony.dll')) | Out-Null
    $game = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'Assembly-CSharp.dll'))
    $harmonyAssembly = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'UnityModManager\0Harmony12.dll'))
    $mod = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $ModAssemblyPath).Path)
    $fact = $game.GetType('Kingmaker.Blueprints.Facts.Fact', $true)
    $buffCollection = $game.GetType('Kingmaker.UnitLogic.Buffs.BuffCollection', $true)
    $onCreated = $buffCollection.GetMethod('OnFactCreated', [Reflection.BindingFlags]'Instance,NonPublic', $null, [Type[]]@($fact), $null)
    if (-not $onCreated) { throw 'OnFactCreated(Fact) did not resolve exactly.' }
    $originalParameters = @($onCreated.GetParameters())
    if ($originalParameters.Count -ne 1 -or $originalParameters[0].Name -ne 'newFact') {
        throw 'Expected void OnFactCreated(Fact newFact).'
    }
    $diagnosticTypes = @($mod.GetTypes() | Where-Object { $_.Namespace -eq 'KingmakerGunslinger.Diagnostics' -and $_.Name -like 'DodgeForensics*Patch' })
    if ($diagnosticTypes.Count -ne 10) { throw "Expected 10 diagnostic patch types, found $($diagnosticTypes.Count)." }
    foreach ($type in $diagnosticTypes) {
        foreach ($method in $type.GetMethods([Reflection.BindingFlags]'Static,NonPublic')) {
            if ($method.Name -notin @('Prefix','Postfix','Finalizer')) { continue }
            foreach ($parameter in $method.GetParameters()) {
                $name = $parameter.Name
                if ($name -in @('__instance','__result','__state','__exception')) { continue }
                if ($name -match '^___') { continue }
                if ($name -notmatch '^__([0-9]+)$') { throw "$($type.Name).$($method.Name) has unsupported parameter '$name'." }
            }
        }
    }
    $onCreatedPatch = $mod.GetType('KingmakerGunslinger.Diagnostics.DodgeForensicsOnFactCreatedPatch', $true)
    $postfix = $onCreatedPatch.GetMethod('Postfix', [Reflection.BindingFlags]'Static,NonPublic')
    if (@($postfix.GetParameters()).Name -notcontains '__0') { throw 'OnFactCreated patch does not bind Fact as __0.' }
    $harmonyType = $harmonyAssembly.GetType('Harmony12.HarmonyInstance', $true)
    $create = $harmonyType.GetMethod('Create', [Type[]]@([string]))
    $harmony = $create.Invoke($null, @('kmg.dodge.compatibility.' + [guid]::NewGuid().ToString('N')))
    $harmonyMethodType = $harmonyAssembly.GetType('Harmony12.HarmonyMethod', $true)
    $harmonyMethodConstructor = $harmonyMethodType.GetConstructor([Type[]]@([Reflection.MethodInfo]))
    $patchMethod = $harmonyType.GetMethod('Patch')
    $catalog = @(
        @('DodgeForensicsTriggerRuleApplyBuffPatch','Kingmaker.UnitLogic.Buffs.BuffCollection','TriggerRuleApplyBuff',@('Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff','Kingmaker.UnitLogic.Mechanics.MechanicsContext','System.Nullable`1[System.TimeSpan]')),
        @('DodgeForensicsAddBuffInternalPatch','Kingmaker.UnitLogic.Buffs.BuffCollection','AddBuffInternal',@('Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff','Kingmaker.UnitLogic.Mechanics.MechanicsContext','System.Nullable`1[System.TimeSpan]')),
        @('DodgeForensicsOnFactCreatedPatch','Kingmaker.UnitLogic.Buffs.BuffCollection','OnFactCreated',@('Kingmaker.Blueprints.Facts.Fact')),
        @('DodgeForensicsOnFactAddedPatch','Kingmaker.UnitLogic.Buffs.BuffCollection','OnFactAdded',@('Kingmaker.Blueprints.Facts.Fact')),
        @('DodgeForensicsUpdateNextEventPatch','Kingmaker.UnitLogic.Buffs.BuffCollection','UpdateNextEvent',@()),
        @('DodgeForensicsTickPatch','Kingmaker.UnitLogic.Buffs.BuffCollection','Tick',@()),
        @('DodgeForensicsRemoveFactPatch','Kingmaker.Blueprints.FactCollection','RemoveFact',@('Kingmaker.Blueprints.Facts.Fact')),
        @('DodgeForensicsOnFactRemovedPatch','Kingmaker.UnitLogic.Buffs.BuffCollection','OnFactRemoved',@('Kingmaker.Blueprints.Facts.Fact')),
        @('DodgeForensicsBuffOnRemovePatch','Kingmaker.UnitLogic.Buffs.Buff','OnRemove',@()),
        @('DodgeForensicsBuffDisposePatch','Kingmaker.UnitLogic.Buffs.Buff','Dispose',@())
    )
    foreach ($entry in $catalog) {
        $targetType = $game.GetType($entry[1], $true)
        $parameterTypes = [Type[]]@($entry[3] | ForEach-Object { if ($_ -eq 'System.Nullable`1[System.TimeSpan]') { [Nullable[TimeSpan]] } else { $resolvedType = $game.GetType($_, $false); if (-not $resolvedType) { $resolvedType = [Type]::GetType($_, $true) }; $resolvedType } })
        $target = $targetType.GetMethod($entry[2], [Reflection.BindingFlags]'Instance,Static,Public,NonPublic', $null, $parameterTypes, $null)
        if (-not $target) { throw "Target did not resolve exactly: $($entry[1]).$($entry[2])" }
        $patchType = $mod.GetType('KingmakerGunslinger.Diagnostics.' + $entry[0], $true)
        $prefix = $patchType.GetMethod('Prefix', [Reflection.BindingFlags]'Static,NonPublic')
        $postfix = $patchType.GetMethod('Postfix', [Reflection.BindingFlags]'Static,NonPublic')
        $prefixHarmony = if ($prefix) { $harmonyMethodConstructor.Invoke(@($prefix)) } else { $null }
        $postfixHarmony = if ($postfix) { $harmonyMethodConstructor.Invoke(@($postfix)) } else { $null }
        [void]$patchMethod.Invoke($harmony, @($target, $prefixHarmony, $postfixHarmony, $null))
    }
    Write-Host 'Exact-reference Dodge Harmony compatibility passed: 10 targets; OnFactCreated(Fact newFact); Harmony 1.2 wrapper construction succeeded.'
}
finally { [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver) }
