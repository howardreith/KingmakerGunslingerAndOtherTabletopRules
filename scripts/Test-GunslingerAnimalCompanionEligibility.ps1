<#
.SYNOPSIS
Executes Kingmaker's installed class-eligibility code for an animal companion,
a player character and humanoid companions, before and after the Gunslinger
Animal exclusion.

.DESCRIPTION
Kingmaker 2.1.7b offers a class in level-up/respec (CharBSelectorLayer and the
console CharGenClassPhaseVM) and applies it (LevelUpController.SelectClass,
including level plans) only when SelectClass.Check ->
BlueprintCharacterClass.MeetsPrerequisites passes. That predicate reads only
the class's Prerequisite components. This script invokes the installed native
SelectClass.Check and MeetsPrerequisites; nothing in Kingmaker is replaced.

Unity's native allocator is unavailable headlessly, so blueprints, components
and progression are created uninitialized and given a zeroed stand-in for
Unity's native-object cache pointer, which is all UnityEngine.Object's
liveness test reads. Fighter, Rogue and Animal fixtures copy their native
prerequisite components (Fighter/Rogue: one PrerequisiteNoClassLevel(Animal);
Animal: PrerequisiteClassLevel(Animal, 1)). The Gunslinger fixture is the one
the production bootstrap validator, RequireNativePlayerClassEligibility in the
supplied mod assembly, accepts; the unfixed shape (no components) must be
rejected by that validator.

This is native-contract evidence only. It does not launch Kingmaker, load a
save, render the class selector or prove the registered in-game blueprint.
#>
[CmdletBinding()]
param(
    [string]$ManagedDir,
    [string]$ModAssemblyPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
if (-not $ManagedDir) {
    [xml]$props = Get-Content -LiteralPath (Join-Path $root 'GamePath.props') -Raw
    $ManagedDir = [string]$props.Project.PropertyGroup.KingmakerManagedDir
}
if (-not $ModAssemblyPath) {
    $ModAssemblyPath = Join-Path $root 'artifacts\bin\Release\KingmakerGunslinger\KingmakerGunslinger.dll'
}
foreach ($required in @((Join-Path $ManagedDir 'Assembly-CSharp.dll'),
        (Join-Path $ManagedDir 'UnityEngine.CoreModule.dll'), $ModAssemblyPath)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Required assembly is missing: $required"
    }
}

$resolver = [ResolveEventHandler]{
    param($sender, $eventArgs)
    $name = ([Reflection.AssemblyName]$eventArgs.Name).Name + '.dll'
    foreach ($candidate in @(
        (Join-Path $ManagedDir $name),
        (Join-Path (Join-Path $ManagedDir 'UnityModManager') $name))) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return [Reflection.Assembly]::LoadFrom($candidate)
        }
    }
    return $null
}

$nativeBlocks = New-Object 'System.Collections.Generic.List[IntPtr]'
$failures = New-Object 'System.Collections.Generic.List[string]'
$passed = 0

function Assert-True([bool]$Condition, [string]$Message) {
    if ($Condition) { $script:passed++ } else { $script:failures.Add($Message) }
}

[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try {
    $game = [Reflection.Assembly]::LoadFrom((Join-Path $ManagedDir 'Assembly-CSharp.dll'))
    $unity = [Reflection.Assembly]::LoadFrom((Join-Path $ManagedDir 'UnityEngine.CoreModule.dll'))
    $mod = [Reflection.Assembly]::LoadFrom($ModAssemblyPath)
    $instance = [Reflection.BindingFlags]'Instance,Public'
    $hidden = [Reflection.BindingFlags]'Instance,NonPublic'

    $classType = $game.GetType('Kingmaker.Blueprints.Classes.BlueprintCharacterClass', $true)
    $componentType = $game.GetType('Kingmaker.Blueprints.BlueprintComponent', $true)
    $prerequisiteType = $game.GetType('Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite', $true)
    $noClassType = $game.GetType('Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteNoClassLevel', $true)
    $classLevelType = $game.GetType('Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteClassLevel', $true)
    $descriptorType = $game.GetType('Kingmaker.UnitLogic.UnitDescriptor', $true)
    $progressionType = $game.GetType('Kingmaker.UnitLogic.UnitProgressionData', $true)
    $classDataType = $game.GetType('Kingmaker.UnitLogic.ClassData', $true)
    $featuresType = $game.GetType('Kingmaker.UnitLogic.FeatureCollection', $true)
    $stateType = $game.GetType('Kingmaker.UnitLogic.Class.LevelUp.LevelUpState', $true)
    $selectClassType = $game.GetType('Kingmaker.UnitLogic.Class.LevelUp.Actions.SelectClass', $true)
    $unityObjectType = $unity.GetType('UnityEngine.Object', $true)
    $factoryType = $mod.GetType('KingmakerGunslinger.Blueprints.GunslingerClassBlueprints', $true)

    # Installed native contracts the production fix depends on.
    $meets = $classType.GetMethod('MeetsPrerequisites', [Type[]]@($descriptorType, $stateType))
    $hideIfRestricted = $classType.GetField('HideIfRestricted', $instance)
    $components = $classType.GetProperty('ComponentsArray', $instance)
    $excludedClass = $noClassType.GetField('CharacterClass', $instance)
    $requiredClass = $classLevelType.GetField('CharacterClass', $instance)
    $requiredLevel = $classLevelType.GetField('Level', $instance)
    $group = $prerequisiteType.GetField('Group', $instance)
    $selectConstructor = $selectClassType.GetConstructor([Type[]]@($classType))
    $check = $selectClassType.GetMethod('Check', [Type[]]@($stateType, $descriptorType))
    $cachedPtr = $unityObjectType.GetField('m_CachedPtr', $hidden)
    if (-not $meets -or $meets.ReturnType -ne [bool] -or
        -not $hideIfRestricted -or $hideIfRestricted.FieldType -ne [bool] -or
        -not $components -or $components.PropertyType -ne $componentType.MakeArrayType() -or
        $noClassType.BaseType -ne $prerequisiteType -or
        -not $excludedClass -or $excludedClass.FieldType -ne $classType -or
        -not $requiredClass -or -not $requiredLevel -or -not $group -or
        -not $selectConstructor -or -not $check -or $check.ReturnType -ne [bool] -or
        -not $cachedPtr) {
        throw 'Installed Kingmaker class-eligibility contract did not resolve exactly.'
    }
    $validator = $factoryType.GetMethod('RequireNativePlayerClassEligibility',
        [Reflection.BindingFlags]'Static,NonPublic', $null, [Type[]]@($classType, $classType), $null)
    $animalGuid = $factoryType.GetField('AnimalClassGuid', [Reflection.BindingFlags]'Static,NonPublic')
    if (-not $validator -or -not $animalGuid) {
        throw 'Production Gunslinger class-eligibility contract is missing from the mod assembly.'
    }
    Assert-True ($animalGuid.GetRawConstantValue() -eq '4cd1757a0eea7694ba5c933729a53920') `
        'Production must exclude the native AnimalClass GUID.'

    function New-NativeObject([Type]$Type) {
        $value = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($Type)
        $block = [Runtime.InteropServices.Marshal]::AllocHGlobal(64)
        [Runtime.InteropServices.Marshal]::Copy((New-Object byte[] 64), 0, $block, 64)
        $script:nativeBlocks.Add($block)
        $cachedPtr.SetValue($value, $block)
        return $value
    }
    function New-NativeClass([object[]]$Prerequisites) {
        $class = New-NativeObject $classType
        Set-Prerequisites $class $Prerequisites
        return $class
    }
    function Set-Prerequisites($Class, [object[]]$Prerequisites) {
        $array = [Array]::CreateInstance($componentType, $Prerequisites.Length)
        for ($index = 0; $index -lt $Prerequisites.Length; $index++) {
            $array.SetValue($Prerequisites[$index], $index)
        }
        $components.SetValue($Class, $array, $null)
    }
    function New-NoClassLevel($Excluded) {
        $value = New-NativeObject $noClassType
        $excludedClass.SetValue($value, $Excluded)
        return $value
    }
    function New-ClassLevel($Required, [int]$Level) {
        $value = New-NativeObject $classLevelType
        $requiredClass.SetValue($value, $Required)
        $requiredLevel.SetValue($value, $Level)
        return $value
    }
    function New-Unit([object[]]$Levels) {
        $unit = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($descriptorType)
        $progression = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($progressionType)
        $classes = [Activator]::CreateInstance(
            [System.Collections.Generic.List[object]].GetGenericTypeDefinition().MakeGenericType(
                $classDataType))
        for ($index = 0; $index -lt $Levels.Length; $index += 2) {
            $data = [Activator]::CreateInstance($classDataType, [object[]]@($Levels[$index]))
            $classDataType.GetField('Level').SetValue($data, [int]$Levels[$index + 1])
            [void]$classes.Add($data)
        }
        $progressionType.GetField('Classes').SetValue($progression, $classes)
        $progressionType.GetField('Features').SetValue($progression,
            [Activator]::CreateInstance($featuresType, [object[]]@($unit)))
        $descriptorType.GetField('Progression').SetValue($unit, $progression)
        return $unit
    }

    $animal = New-NativeObject $classType
    Set-Prerequisites $animal @(New-ClassLevel $animal 1)
    $hideIfRestricted.SetValue($animal, $true)
    $fighter = New-NativeClass @(New-NoClassLevel $animal)
    $rogue = New-NativeClass @(New-NoClassLevel $animal)
    $unfixed = New-NativeClass @()
    $gunslinger = New-NativeClass @(New-NoClassLevel $animal)

    function Test-Validator($Class) {
        try { [void]$validator.Invoke($null, [object[]]@($Class, $animal)); return $true }
        catch {
            # PowerShell wraps the reflected TargetInvocationException.
            for ($cause = $_.Exception; $cause; $cause = $cause.InnerException) {
                if ($cause -is [InvalidOperationException]) { return $false }
            }
            throw
        }
    }
    Assert-True (Test-Validator $gunslinger) 'Production validator must accept Fighter''s Animal exclusion.'
    Assert-True (-not (Test-Validator $unfixed)) 'Production validator must reject the unfixed component-free class.'
    Assert-True (-not (Test-Validator (New-NativeClass @(New-NoClassLevel $fighter)))) `
        'Production validator must reject an exclusion of any class but Animal.'
    Assert-True (-not (Test-Validator (New-NativeClass @((New-NoClassLevel $animal), (New-NoClassLevel $animal))))) `
        'Production validator must reject extra class components.'
    $anyGroup = New-NoClassLevel $animal
    $group.SetValue($anyGroup, [Enum]::Parse($group.FieldType, 'Any'))
    Assert-True (-not (Test-Validator (New-NativeClass @($anyGroup)))) `
        'Production validator must reject an Any-group exclusion.'
    Assert-True (-not [bool]$hideIfRestricted.GetValue($gunslinger)) `
        'Gunslinger keeps Fighter''s HideIfRestricted=false.'

    $state = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($stateType)
    function Get-Selectable([object[]]$Catalog, [string[]]$Names, $Unit) {
        $result = @()
        for ($index = 0; $index -lt $Catalog.Length; $index++) {
            $action = $selectConstructor.Invoke([object[]]@($Catalog[$index]))
            $applies = [bool]$check.Invoke($action, [object[]]@($state, $Unit))
            $offered = [bool]$meets.Invoke($Catalog[$index], [object[]]@($Unit, $state))
            Assert-True ($applies -eq $offered) ("SelectClass.Check and MeetsPrerequisites disagree for " + $Names[$index])
            if ($applies) { $result += $Names[$index] }
        }
        return ,$result
    }
    $names = @('Animal', 'Fighter', 'Rogue', 'Gunslinger')
    $before = @($animal, $fighter, $rogue, $unfixed)
    $after = @($animal, $fighter, $rogue, $gunslinger)
    $scenarios = @(
        @{ Name = 'animal companion (Animal 3), before fix'; Catalog = $before; Unit = (New-Unit @($animal, 3)); Expected = @('Animal', 'Gunslinger') },
        @{ Name = 'animal companion (Animal 3)'; Catalog = $after; Unit = (New-Unit @($animal, 3)); Expected = @('Animal') },
        @{ Name = 'animal companion already holding Gunslinger 1'; Catalog = $after; Unit = (New-Unit @($animal, 3, $gunslinger, 1)); Expected = @('Animal') },
        @{ Name = 'player character (Fighter 2)'; Catalog = $after; Unit = (New-Unit @($fighter, 2)); Expected = @('Fighter', 'Rogue', 'Gunslinger') },
        @{ Name = 'humanoid companion (Rogue 4)'; Catalog = $after; Unit = (New-Unit @($rogue, 4)); Expected = @('Fighter', 'Rogue', 'Gunslinger') },
        @{ Name = 'existing Gunslinger (Gunslinger 5)'; Catalog = $after; Unit = (New-Unit @($gunslinger, 5)); Expected = @('Fighter', 'Rogue', 'Gunslinger') }
    )
    foreach ($scenario in $scenarios) {
        $observed = Get-Selectable $scenario.Catalog $names $scenario.Unit
        Assert-True (($observed -join ',') -eq ($scenario.Expected -join ',')) `
            ($scenario.Name + ' selectable classes were: ' + ($observed -join ', '))
        Write-Host ('  {0}: {1}' -f $scenario.Name, ($observed -join ', '))
    }
}
finally {
    foreach ($block in $nativeBlocks) { [Runtime.InteropServices.Marshal]::FreeHGlobal($block) }
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver)
}

if ($failures.Count -ne 0) {
    foreach ($failure in $failures) { Write-Host "FAIL $failure" }
    throw ("Gunslinger animal-companion eligibility: {0} passed, {1} failed." -f $passed, $failures.Count)
}
$gameHash = (Get-FileHash -LiteralPath (Join-Path $ManagedDir 'Assembly-CSharp.dll') -Algorithm SHA256).Hash
$modHash = (Get-FileHash -LiteralPath $ModAssemblyPath -Algorithm SHA256).Hash
Write-Host (("Gunslinger animal-companion eligibility: {0} assertions PASS against native " +
    "SelectClass.Check/MeetsPrerequisites (Assembly-CSharp SHA-256 {1}; mod SHA-256 {2}). " +
    "Native-contract evidence only; no game launch, save, selector rendering or registered " +
    "in-game blueprint.") -f $passed, $gameHash, $modHash)
