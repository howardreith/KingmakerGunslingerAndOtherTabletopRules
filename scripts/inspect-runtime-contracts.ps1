[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$KingmakerInstallDir,

    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeContractInspection.Common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
if (-not $OutputPath) {
    $OutputPath = Join-Path $repositoryRoot 'runtime-contracts.json'
}

if (-not (Test-Path -LiteralPath $KingmakerInstallDir -PathType Container)) {
    throw "KingmakerInstallDir does not exist: $KingmakerInstallDir"
}

$KingmakerInstallDir = (Resolve-Path -LiteralPath $KingmakerInstallDir).Path
$managedDirectory = Join-Path $KingmakerInstallDir 'Kingmaker_Data\Managed'
$ummDirectory = Join-Path $managedDirectory 'UnityModManager'
$gameAssemblyPath = Join-Path $managedDirectory 'Assembly-CSharp.dll'
$harmonyAssemblyPath = Join-Path $ummDirectory '0Harmony12.dll'
$ummAssemblyPath = Join-Path $ummDirectory 'UnityModManager.dll'
$newtonsoftAssemblyPath = Join-Path $managedDirectory 'Newtonsoft.Json.dll'

foreach ($requiredPath in @($gameAssemblyPath, $harmonyAssemblyPath, $ummAssemblyPath, $newtonsoftAssemblyPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required assembly does not exist: $requiredPath"
    }
}

function Get-KmgLoadableTypes {
    param(
        [Parameter(Mandatory = $true)]
        [Reflection.Assembly]$Assembly
    )

    try {
        return @($Assembly.GetTypes())
    }
    catch [Reflection.ReflectionTypeLoadException] {
        return @($_.Exception.Types | Where-Object { $_ -ne $null })
    }
}

function Convert-KmgMethodDescription {
    param(
        [Parameter(Mandatory = $true)]
        [Reflection.MethodInfo]$Method
    )

    return [ordered]@{
        name = $Method.Name
        declaringType = $Method.DeclaringType.FullName
        isStatic = $Method.IsStatic
        isPublic = $Method.IsPublic
        returnType = $Method.ReturnType.FullName
        parameterCount = $Method.GetParameters().Count
        parameters = @(
            $Method.GetParameters() | ForEach-Object {
                [ordered]@{
                    name = $_.Name
                    type = $_.ParameterType.FullName
                    isOptional = $_.IsOptional
                }
            }
        )
        signature = $Method.ToString()
        metadataToken = $Method.MetadataToken
    }
}

function Convert-KmgMemberDescription {
    param(
        [Parameter(Mandatory = $true)]
        [Reflection.MemberInfo]$Member
    )

    $memberType = $null
    $isStatic = $false
    if ($Member -is [Reflection.FieldInfo]) {
        $memberType = $Member.FieldType.FullName
        $isStatic = $Member.IsStatic
    }
    elseif ($Member -is [Reflection.PropertyInfo]) {
        $memberType = $Member.PropertyType.FullName
        $accessor = $Member.GetGetMethod($true)
        if (-not $accessor) {
            $accessor = $Member.GetSetMethod($true)
        }
        if ($accessor) {
            $isStatic = $accessor.IsStatic
        }
    }

    return [ordered]@{
        name = $Member.Name
        kind = $Member.MemberType.ToString()
        declaringType = $Member.DeclaringType.FullName
        valueType = $memberType
        isStatic = $isStatic
        metadataToken = $Member.MetadataToken
    }
}

function Get-KmgMemberValueType {
    param(
        [Reflection.MemberInfo]$Member
    )

    if (-not $Member) {
        return $null
    }

    if ($Member -is [Reflection.FieldInfo]) {
        return $Member.FieldType
    }

    if ($Member -is [Reflection.PropertyInfo]) {
        return $Member.PropertyType
    }

    return $null
}

function Get-KmgNamedMembers {
    param(
        [Parameter(Mandatory = $true)]
        [Type]$Type,

        [Parameter(Mandatory = $true)]
        [string[]]$Names,

        [Parameter(Mandatory = $true)]
        [Reflection.BindingFlags]$BindingFlags
    )

    $members = @()
    foreach ($name in $Names) {
        $member = Find-KmgFieldOrProperty -Type $Type -Name $name -BindingFlags $BindingFlags
        if ($member) {
            $members += $member
        }
    }

    return @($members)
}

function Find-KmgFieldOrProperty {
    param(
        [Parameter(Mandatory = $true)]
        [Type]$Type,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [Reflection.BindingFlags]$BindingFlags
    )

    $declaredFlags = $BindingFlags -bor [Reflection.BindingFlags]::DeclaredOnly
    $current = $Type
    while ($current) {
        $field = $current.GetField($Name, $declaredFlags)
        if ($field) {
            return $field
        }

        $properties = @(
            $current.GetProperties($declaredFlags) |
                Where-Object { $_.Name -eq $Name } |
                Sort-Object MetadataToken
        )
        if ($properties.Count -gt 1) {
            throw "Required member lookup is ambiguous on type '$($current.FullName)': $Name"
        }
        if ($properties.Count -eq 1) {
            return $properties[0]
        }
        $current = $current.BaseType
    }
    return $null
}

function Get-KmgWritableWeaponTypeMembers {
    param(
        [Parameter(Mandatory = $true)]
        [Type]$ItemType,

        [Parameter(Mandatory = $true)]
        [Type]$WeaponType
    )

    $members = @()
    $current = $ItemType
    while ($current) {
        $declaredFlags = [Reflection.BindingFlags]::Public -bor
            [Reflection.BindingFlags]::NonPublic -bor
            [Reflection.BindingFlags]::Instance -bor
            [Reflection.BindingFlags]::DeclaredOnly

        foreach ($field in $current.GetFields($declaredFlags)) {
            if ($field.FieldType.FullName -eq $WeaponType.FullName -and
                -not $field.IsInitOnly -and
                -not $field.IsLiteral) {
                $members += $field
            }
        }

        foreach ($property in $current.GetProperties($declaredFlags)) {
            if ($property.PropertyType.FullName -eq $WeaponType.FullName -and
                $property.GetIndexParameters().Count -eq 0 -and
                $property.GetGetMethod($true) -and
                $property.GetSetMethod($true)) {
                $members += $property
            }
        }

        $current = $current.BaseType
    }

    return @($members)
}


function Get-KmgWritableTargetAcMembers {
    param(
        [Parameter(Mandatory = $true)]
        [Type]$RuleCalculateAcType
    )

    $properties = @()
    $current = $RuleCalculateAcType
    while ($current) {
        $declaredFlags = [Reflection.BindingFlags]::Public -bor
            [Reflection.BindingFlags]::NonPublic -bor
            [Reflection.BindingFlags]::Instance -bor
            [Reflection.BindingFlags]::DeclaredOnly
        $property = $current.GetProperty('TargetAC', $declaredFlags)
        if ($property -and
            $property.PropertyType.FullName -eq 'System.Int32' -and
            $property.GetIndexParameters().Count -eq 0 -and
            $property.GetGetMethod($true) -and
            $property.GetSetMethod($true)) {
            $properties += $property
        }
        $current = $current.BaseType
    }

    if ($properties.Count -gt 0) {
        return @($properties)
    }

    $fields = @()
    $current = $RuleCalculateAcType
    while ($current) {
        $declaredFlags = [Reflection.BindingFlags]::Public -bor
            [Reflection.BindingFlags]::NonPublic -bor
            [Reflection.BindingFlags]::Instance -bor
            [Reflection.BindingFlags]::DeclaredOnly
        foreach ($name in @('TargetAC', 'm_TargetAC', '<TargetAC>k__BackingField')) {
            $field = $current.GetField($name, $declaredFlags)
            if ($field -and
                $field.FieldType.FullName -eq 'System.Int32' -and
                -not $field.IsInitOnly -and
                -not $field.IsLiteral) {
                $fields += $field
            }
        }
        $current = $current.BaseType
    }

    return @($fields)
}

$script:KmgReflectionSearchDirectories = @($managedDirectory, $ummDirectory)
$resolveHandler = [ResolveEventHandler] {
    param($sender, $eventArgs)

    $requestedName = New-Object Reflection.AssemblyName($eventArgs.Name)
    foreach ($directory in $script:KmgReflectionSearchDirectories) {
        $candidate = Join-Path $directory ($requestedName.Name + '.dll')
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            try {
                return [Reflection.Assembly]::ReflectionOnlyLoadFrom($candidate)
            }
            catch {
                return $null
            }
        }
    }

    return $null
}

[AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve($resolveHandler)

try {
    $bindingFlags = [Reflection.BindingFlags]::Public -bor [Reflection.BindingFlags]::NonPublic -bor [Reflection.BindingFlags]::Instance -bor [Reflection.BindingFlags]::Static

    $gameAssembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($gameAssemblyPath)
    $libraryType = $gameAssembly.GetType('Kingmaker.Blueprints.LibraryScriptableObject', $false, $false)
    $blueprintBaseType = $gameAssembly.GetType('Kingmaker.Blueprints.BlueprintScriptableObject', $false, $false)
    $blueprintComponentType = $gameAssembly.GetType('Kingmaker.Blueprints.BlueprintComponent', $false, $false)
    $blueprintFeatureType = $gameAssembly.GetType('Kingmaker.Blueprints.Classes.BlueprintFeature', $false, $false)
    $blueprintWeaponType = $gameAssembly.GetType('Kingmaker.Blueprints.Items.Weapons.BlueprintWeaponType', $false, $false)
    $blueprintItemWeaponType = $gameAssembly.GetType('Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon', $false, $false)
    $blueprintWeaponEnchantmentType = $gameAssembly.GetType('Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment', $false, $false)
    $equipmentRestrictionType = $gameAssembly.GetType('Kingmaker.Blueprints.Items.Components.EquipmentRestriction', $false, $false)
    $unitDescriptorType = $gameAssembly.GetType('Kingmaker.UnitLogic.UnitDescriptor', $false, $false)
    $unitPartType = $gameAssembly.GetType('Kingmaker.UnitLogic.UnitPart', $false, $false)
    $gameType = $gameAssembly.GetType('Kingmaker.Game', $false, $false)
    $rulebookEventContextType = $gameAssembly.GetType('Kingmaker.RuleSystem.RulebookEventContext', $false, $false)
    $rulebookRollEntryType = $gameAssembly.GetType('Kingmaker.RuleSystem.RulebookEvent+RollEntry', $false, $false)
    $ruleAttackWithWeaponType = $gameAssembly.GetType('Kingmaker.RuleSystem.Rules.RuleAttackWithWeapon', $false, $false)
    $ruleAttackRollType = $gameAssembly.GetType('Kingmaker.RuleSystem.Rules.RuleAttackRoll', $false, $false)
    $ruleCalculateAcType = $gameAssembly.GetType('Kingmaker.RuleSystem.Rules.RuleCalculateAC', $false, $false)
    $unitEntityDataType = $gameAssembly.GetType('Kingmaker.EntitySystem.Entities.UnitEntityData', $false, $false)
    $itemEntityType = $gameAssembly.GetType('Kingmaker.Items.ItemEntity', $false, $false)
    $itemEntityWeaponType = $gameAssembly.GetType('Kingmaker.Items.ItemEntityWeapon', $false, $false)
    $itemEnchantmentType = $gameAssembly.GetType(
        'Kingmaker.Blueprints.Items.Ecnchantments.ItemEnchantment',
        $false,
        $false)
    $itemApplyEnchantmentsMethods = if ($itemEntityType) {
        @(
            $itemEntityType.GetMethods($bindingFlags) |
                Where-Object {
                    $_.Name -eq 'ApplyEnchantments' -and
                    -not $_.IsStatic -and
                    -not $_.IsGenericMethodDefinition -and
                    $_.DeclaringType.FullName -eq $itemEntityType.FullName -and
                    $_.ReturnType.FullName -eq 'System.Void' -and
                    $_.GetParameters().Count -eq 0
                }
        )
    }
    else { @() }
    foreach ($requiredType in @($libraryType, $blueprintBaseType, $blueprintComponentType, $blueprintFeatureType, $blueprintWeaponType, $blueprintItemWeaponType, $equipmentRestrictionType, $unitDescriptorType, $unitPartType, $gameType)) {
        if (-not $requiredType) {
            throw 'A required Kingmaker blueprint type was not found in Assembly-CSharp.dll.'
        }
    }

    $loadDictionaryMethods = @(
        $libraryType.GetMethods($bindingFlags) |
            Where-Object { $_.Name -eq 'LoadDictionary' } |
            ForEach-Object { Convert-KmgMethodDescription -Method $_ }
    )
    $zeroArgumentLoadDictionaryMethods = @(
        $libraryType.GetMethods($bindingFlags) |
            Where-Object { $_.Name -eq 'LoadDictionary' -and -not $_.IsStatic -and $_.GetParameters().Count -eq 0 }
    )
    $getAllBlueprintsMethods = @(
        $libraryType.GetMethods($bindingFlags) |
            Where-Object { $_.Name -eq 'GetAllBlueprints' -and -not $_.IsStatic -and $_.GetParameters().Count -eq 0 }
    )
    $blueprintsByAssetIdMember = Find-KmgFieldOrProperty -Type $libraryType -Name 'BlueprintsByAssetId' -BindingFlags $bindingFlags
    $assetGuidBindingFlags = [Reflection.BindingFlags]::Instance -bor [Reflection.BindingFlags]::NonPublic
    $assetGuidField = $blueprintBaseType.GetField('m_AssetGuid', $assetGuidBindingFlags)
    $componentsArrayMember = Find-KmgFieldOrProperty -Type $blueprintBaseType -Name 'ComponentsArray' -BindingFlags $bindingFlags
    $hideInUiMember = Find-KmgFieldOrProperty -Type $blueprintFeatureType -Name 'HideInUI' -BindingFlags $bindingFlags
    $ranksMember = Find-KmgFieldOrProperty -Type $blueprintFeatureType -Name 'Ranks' -BindingFlags $bindingFlags
    $weaponTypeMembers = @(Get-KmgWritableWeaponTypeMembers -ItemType $blueprintItemWeaponType -WeaponType $blueprintWeaponType)
    $preferredWeaponTypeMemberNames = @('Type', 'm_Type', 'WeaponType', 'm_WeaponType')
    $preferredWeaponTypeMembers = @(
        $weaponTypeMembers | Where-Object { $preferredWeaponTypeMemberNames -contains $_.Name }
    )
    $canBeEquippedMethods = @(
        $equipmentRestrictionType.GetMethods($bindingFlags) |
            Where-Object {
                $_.Name -eq 'CanBeEquippedBy' -and
                -not $_.IsStatic -and
                $_.ReturnType.FullName -eq 'System.Boolean' -and
                $_.GetParameters().Count -eq 1 -and
                $_.GetParameters()[0].ParameterType.FullName -eq $unitDescriptorType.FullName
            }
    )
    $unitDescriptorProgressionMember = Find-KmgFieldOrProperty `
        -Type $unitDescriptorType -Name 'Progression' -BindingFlags $bindingFlags
    $progressionType = Get-KmgMemberValueType -Member $unitDescriptorProgressionMember
    $progressionFeaturesMember = if ($progressionType) {
        Find-KmgFieldOrProperty -Type $progressionType -Name 'Features' `
            -BindingFlags $bindingFlags
    }
    else { $null }
    $featureCollectionType = Get-KmgMemberValueType -Member $progressionFeaturesMember
    $getRankNamedMethods = if ($featureCollectionType) {
        @(Select-KmgNamedMethodCandidates `
            -Methods @($featureCollectionType.GetMethods($bindingFlags)) `
            -Names @('GetRank'))
    }
    else { @() }
    $getRankMethods = @(
        $getRankNamedMethods |
            Where-Object {
                $_.Name -eq 'GetRank' -and
                -not $_.IsStatic -and
                $_.ReturnType.FullName -eq 'System.Int32' -and
                $_.GetParameters().Count -eq 1 -and
                $_.GetParameters()[0].ParameterType.FullName -eq $blueprintFeatureType.FullName
            }
    )
    $gameInstanceMember = Find-KmgFieldOrProperty -Type $gameType -Name 'Instance' -BindingFlags $bindingFlags
    $gamePlayerMember = Find-KmgFieldOrProperty -Type $gameType -Name 'Player' -BindingFlags $bindingFlags
    $playerType = Get-KmgMemberValueType -Member $gamePlayerMember
    $mainCharacterMembers = @()
    $inventoryMembers = @()
    $inventoryTypes = @()
    $inventoryAddMethods = @()
    $inventoryRemoveMethods = @()
    if ($playerType) {
        $mainCharacterMembers = @(Get-KmgNamedMembers -Type $playerType -Names @('MainCharacterEntity', 'MainCharacter', 'MainCharacterUnit') -BindingFlags $bindingFlags)
        $inventoryMembers = @(Get-KmgNamedMembers -Type $playerType -Names @('Inventory', 'SharedInventory', 'SharedStash') -BindingFlags $bindingFlags)
        $inventoryTypes = @(
            $inventoryMembers |
                ForEach-Object { Get-KmgMemberValueType -Member $_ } |
                Where-Object { $_ -ne $null } |
                Sort-Object -Property FullName -Unique
        )
        foreach ($inventoryType in $inventoryTypes) {
            $inventoryAddMethods += @(
                $inventoryType.GetMethods($bindingFlags) |
                    Where-Object {
                        @('Add', 'AddItem', 'AddItemSilent') -contains $_.Name -and
                        -not $_.IsStatic -and
                        -not $_.IsGenericMethodDefinition -and
                        $_.GetParameters().Count -ge 1 -and
                        $_.GetParameters()[0].ParameterType.IsAssignableFrom($blueprintItemWeaponType)
                    }
            )
            $inventoryRemoveMethods += @(
                $inventoryType.GetMethods($bindingFlags) |
                    Where-Object {
                        @('Remove', 'RemoveItem') -contains $_.Name -and
                        -not $_.IsStatic -and
                        -not $_.IsGenericMethodDefinition -and
                        $_.GetParameters().Count -ge 1
                    }
            )
        }
    }
    $addFeatureNamedMethods = if ($featureCollectionType) {
        @(Select-KmgNamedMethodCandidates `
            -Methods @($featureCollectionType.GetMethods($bindingFlags)) `
            -Names @('AddFeature'))
    }
    else { @() }
    $addFeatureMethods = @(
        $addFeatureNamedMethods |
            Where-Object {
                $_.Name -eq 'AddFeature' -and
                -not $_.IsStatic -and
                -not $_.IsGenericMethodDefinition -and
                $_.GetParameters().Count -eq 2 -and
                $_.GetParameters()[0].ParameterType.FullName -eq $blueprintFeatureType.FullName -and
                $_.GetParameters()[1].ParameterType.FullName -eq 'Kingmaker.UnitLogic.Mechanics.MechanicsContext' -and
                $_.ReturnType.FullName -eq 'Kingmaker.UnitLogic.Feature'
            }
    )

    $unitPartInstanceGetMethods = if ($unitEntityDataType) {
        @(
            $unitEntityDataType.GetMethods($bindingFlags) |
                Where-Object {
                    $_.Name -eq 'Get' -and
                    -not $_.IsStatic -and
                    $_.IsGenericMethodDefinition -and
                    $_.GetGenericArguments().Count -eq 1 -and
                    $_.GetParameters().Count -eq 0
                }
        )
    }
    else { @() }

    $unitPartInstanceEnsureMethods = if ($unitEntityDataType) {
        @(
            $unitEntityDataType.GetMethods($bindingFlags) |
                Where-Object {
                    $_.Name -eq 'Ensure' -and
                    -not $_.IsStatic -and
                    $_.IsGenericMethodDefinition -and
                    $_.GetGenericArguments().Count -eq 1 -and
                    $_.GetParameters().Count -eq 0
                }
        )
    }
    else { @() }

    $unitPartExtensionGetMethods = @()
    $unitPartExtensionEnsureMethods = @()
    $unitPartUnrelatedMethodsExcludedBeforeParameterInspection = 0
    if ($unitEntityDataType) {
        foreach ($candidateType in Get-KmgLoadableTypes -Assembly $gameAssembly) {
            if (-not $candidateType.IsAbstract -or -not $candidateType.IsSealed) {
                continue
            }

            $declaredStaticFlags = [Reflection.BindingFlags]::Public -bor
                [Reflection.BindingFlags]::NonPublic -bor
                [Reflection.BindingFlags]::Static -bor
                [Reflection.BindingFlags]::DeclaredOnly
            $declaredStaticMethods = @($candidateType.GetMethods($declaredStaticFlags))
            $namedCandidates = @(Select-KmgNamedMethodCandidates `
                -Methods $declaredStaticMethods -Names @('Get', 'Ensure'))
            $unitPartUnrelatedMethodsExcludedBeforeParameterInspection +=
                $declaredStaticMethods.Count - $namedCandidates.Count
            foreach ($method in $namedCandidates) {
                if (-not $method.IsGenericMethodDefinition -or
                    $method.GetGenericArguments().Count -ne 1) {
                    continue
                }

                $parameters = @(Get-KmgRequiredMethodParameters -Method $method `
                    -ContractName 'UnitEntityData UnitPart Get/Ensure extension')
                if ($parameters.Count -lt 1) {
                    continue
                }

                $receiverType = $parameters[0].ParameterType
                if (-not ($receiverType.IsAssignableFrom($unitEntityDataType) -or
                    $unitEntityDataType.IsAssignableFrom($receiverType))) {
                    continue
                }

                if ($method.Name -eq 'Get') {
                    $unitPartExtensionGetMethods += $method
                }
                elseif ($method.Name -eq 'Ensure') {
                    $unitPartExtensionEnsureMethods += $method
                }
            }
        }
    }

    $itemEntityWeaponBlueprintMembers = if ($itemEntityWeaponType) {
        @(Get-KmgNamedMembers -Type $itemEntityWeaponType -Names @('Blueprint', 'm_Blueprint', 'BlueprintItem', 'ItemBlueprint') -BindingFlags $bindingFlags)
    }
    else {
        @()
    }
    $itemEntityWeaponRuntimeIdMembers = if ($itemEntityWeaponType) {
        @(Get-KmgNamedMembers -Type $itemEntityWeaponType -Names @('UniqueId', 'm_UniqueId', 'Id', 'm_Id', 'EntityId') -BindingFlags $bindingFlags)
    }
    else {
        @()
    }

    $itemEntityWeaponUniqueIdMembers = @()
    if ($itemEntityWeaponType) {
        $currentIdentityType = $itemEntityWeaponType
        while ($currentIdentityType) {
            $declaredIdentityFlags = [Reflection.BindingFlags]::Public -bor
                [Reflection.BindingFlags]::NonPublic -bor
                [Reflection.BindingFlags]::Instance -bor
                [Reflection.BindingFlags]::DeclaredOnly
            $identityMember = Find-KmgFieldOrProperty -Type $currentIdentityType -Name 'UniqueId' -BindingFlags $declaredIdentityFlags
            if ($identityMember) {
                $itemEntityWeaponUniqueIdMembers += $identityMember
            }
            $currentIdentityType = $currentIdentityType.BaseType
        }
    }

    $itemEntityWeaponUniqueIdMember = if ($itemEntityWeaponUniqueIdMembers.Count -eq 1) {
        $itemEntityWeaponUniqueIdMembers[0]
    }
    else { $null }
    $itemEntityWeaponUniqueIdType = Get-KmgMemberValueType -Member $itemEntityWeaponUniqueIdMember
    $itemEntityWeaponUniqueIdReadable = $false
    $itemEntityWeaponUniqueIdStatic = $true
    if ($itemEntityWeaponUniqueIdMember -is [Reflection.FieldInfo]) {
        $itemEntityWeaponUniqueIdReadable = $true
        $itemEntityWeaponUniqueIdStatic = $itemEntityWeaponUniqueIdMember.IsStatic
    }
    elseif ($itemEntityWeaponUniqueIdMember -is [Reflection.PropertyInfo]) {
        $identityGetter = $itemEntityWeaponUniqueIdMember.GetGetMethod($true)
        if ($identityGetter) {
            $itemEntityWeaponUniqueIdReadable = $true
            $itemEntityWeaponUniqueIdStatic = $identityGetter.IsStatic
        }
    }

    $itemEnchantmentCollectionMembers = @()
    $itemAddEnchantmentMethods = @()
    $itemRemoveEnchantmentMethods = @()
    if ($itemEntityWeaponType) {
        $currentItemType = $itemEntityWeaponType
        while ($currentItemType) {
            $declaredFlags = [Reflection.BindingFlags]::Public -bor
                [Reflection.BindingFlags]::NonPublic -bor
                [Reflection.BindingFlags]::Instance -bor
                [Reflection.BindingFlags]::DeclaredOnly
            foreach ($memberName in @('Enchantments', 'm_Enchantments', 'EnchantmentFacts', 'm_EnchantmentFacts')) {
                $member = Find-KmgFieldOrProperty -Type $currentItemType -Name $memberName -BindingFlags $declaredFlags
                if ($member) {
                    $itemEnchantmentCollectionMembers += $member
                }
            }
            $itemAddEnchantmentMethods += @(
                $currentItemType.GetMethods($declaredFlags) |
                    Where-Object { $_.Name -eq 'AddEnchantment' -and -not $_.IsStatic -and -not $_.IsGenericMethodDefinition }
            )
            $itemRemoveEnchantmentMethods += @(
                $currentItemType.GetMethods($declaredFlags) |
                    Where-Object { $_.Name -eq 'RemoveEnchantment' -and -not $_.IsStatic -and -not $_.IsGenericMethodDefinition }
            )
            $currentItemType = $currentItemType.BaseType
        }
    }

    $itemEnchantmentBlueprintMembers = @()
    if ($itemEnchantmentType) {
        $currentEnchantmentType = $itemEnchantmentType
        while ($currentEnchantmentType) {
            $declaredFlags = [Reflection.BindingFlags]::Public -bor
                [Reflection.BindingFlags]::NonPublic -bor
                [Reflection.BindingFlags]::Instance -bor
                [Reflection.BindingFlags]::DeclaredOnly
            foreach ($memberName in @('Blueprint', 'm_Blueprint')) {
                $member = Find-KmgFieldOrProperty -Type $currentEnchantmentType -Name $memberName -BindingFlags $declaredFlags
                if ($member) {
                    $itemEnchantmentBlueprintMembers += $member
                }
            }
            $currentEnchantmentType = $currentEnchantmentType.BaseType
        }
    }

    $compatibleAddEnchantmentMethods = @()
    if ($blueprintWeaponEnchantmentType) {
        $compatibleAddEnchantmentMethods = @(
            $itemAddEnchantmentMethods | Where-Object {
                $parameters = $_.GetParameters()
                if ($parameters.Count -lt 1 -or
                    -not $parameters[0].ParameterType.IsAssignableFrom($blueprintWeaponEnchantmentType)) {
                    return $false
                }
                for ($index = 1; $index -lt $parameters.Count; $index++) {
                    $parameter = $parameters[$index]
                    if (-not $parameter.IsOptional -and
                        $parameter.ParameterType.IsValueType -and
                        -not [Nullable]::GetUnderlyingType($parameter.ParameterType)) {
                        return $false
                    }
                }
                return $true
            }
        )
    }

    $compatibleRemoveEnchantmentMethods = @()
    if ($itemEnchantmentType) {
        $compatibleRemoveEnchantmentMethods = @(
            $itemRemoveEnchantmentMethods | Where-Object {
                $parameters = $_.GetParameters()
                $parameters.Count -ge 1 -and
                    $parameters[0].ParameterType.IsAssignableFrom($itemEnchantmentType)
            }
        )
    }

    $ruleAttackWithWeaponOnTriggerMethods = if ($ruleAttackWithWeaponType) {
        @(
            $ruleAttackWithWeaponType.GetMethods($bindingFlags) |
                Where-Object { $parameters = $_.GetParameters(); $_.Name -eq 'OnTrigger' -and -not $_.IsStatic -and -not $_.IsGenericMethodDefinition -and $_.ReturnType.FullName -eq 'System.Void' -and $_.DeclaringType.FullName -eq $ruleAttackWithWeaponType.FullName -and $parameters.Count -eq 1 -and $rulebookEventContextType -ne $null -and $parameters[0].ParameterType.FullName -eq $rulebookEventContextType.FullName }
        )
    }
    else { @() }
    $ruleAttackRollOnTriggerMethods = if ($ruleAttackRollType) {
        @(
            $ruleAttackRollType.GetMethods($bindingFlags) |
                Where-Object { $parameters = $_.GetParameters(); $_.Name -eq 'OnTrigger' -and -not $_.IsStatic -and -not $_.IsGenericMethodDefinition -and $_.ReturnType.FullName -eq 'System.Void' -and $_.DeclaringType.FullName -eq $ruleAttackRollType.FullName -and $parameters.Count -eq 1 -and $rulebookEventContextType -ne $null -and $parameters[0].ParameterType.FullName -eq $rulebookEventContextType.FullName }
        )
    }
    else { @() }
    $ruleAttackRollMainRollSetterMethods = if ($ruleAttackRollType -and $rulebookRollEntryType) {
        @(
            $ruleAttackRollType.GetMethods($bindingFlags) |
                Where-Object {
                    $parameters = $_.GetParameters()
                    $_.Name -eq 'set_Roll' -and
                    $_.IsPrivate -and
                    $_.IsSpecialName -and
                    -not $_.IsStatic -and
                    -not $_.IsGenericMethodDefinition -and
                    $_.DeclaringType.FullName -eq $ruleAttackRollType.FullName -and
                    $_.ReturnType.FullName -eq 'System.Void' -and
                    $parameters.Count -eq 1 -and
                    $parameters[0].ParameterType.FullName -eq $rulebookRollEntryType.FullName
                }
        )
    }
    else { @() }
    $ruleAttackRollSuccessMethods = if ($ruleAttackRollType) {
        @(
            $ruleAttackRollType.GetMethods($bindingFlags) |
                Where-Object {
                    $parameters = $_.GetParameters()
                    $_.Name -eq 'IsSuccessRoll' -and
                    $_.IsPublic -and
                    -not $_.IsStatic -and
                    -not $_.IsGenericMethodDefinition -and
                    $_.DeclaringType.FullName -eq $ruleAttackRollType.FullName -and
                    $_.ReturnType.FullName -eq 'System.Boolean' -and
                    $parameters.Count -eq 1 -and
                    $parameters[0].ParameterType.FullName -eq 'System.Int32'
                }
        )
    }
    else { @() }
    $rulebookRollEntryFields = if ($rulebookRollEntryType) {
        @($rulebookRollEntryType.GetFields($bindingFlags) | Where-Object { $_.DeclaringType.FullName -eq $rulebookRollEntryType.FullName })
    }
    else { @() }

    $ruleCalculateAcOnTriggerMethods = if ($ruleCalculateAcType) {
        @(
            $ruleCalculateAcType.GetMethods($bindingFlags) |
                Where-Object { $parameters = $_.GetParameters(); $_.Name -eq 'OnTrigger' -and -not $_.IsStatic -and -not $_.IsGenericMethodDefinition -and $_.ReturnType.FullName -eq 'System.Void' -and $_.DeclaringType.FullName -eq $ruleCalculateAcType.FullName -and $parameters.Count -eq 1 -and $rulebookEventContextType -ne $null -and $parameters[0].ParameterType.FullName -eq $rulebookEventContextType.FullName }
        )
    }
    else { @() }
    $weaponAttackMembers = if ($ruleAttackWithWeaponType) {
        @(Get-KmgNamedMembers -Type $ruleAttackWithWeaponType -Names @('Weapon', 'm_Weapon', 'Initiator', 'Target', 'IsFullAttack', 'IsFirstAttack', 'IsAttackOfOpportunity', 'AttackNumber') -BindingFlags $bindingFlags)
    }
    else { @() }
    $attackRollMembers = if ($ruleAttackRollType) {
        @(Get-KmgNamedMembers -Type $ruleAttackRollType -Names @('Weapon', 'm_Weapon', 'RuleAttackWithWeapon', 'D20', 'RollResult', 'NaturalRoll', 'AttackBonus', 'AttackRoll', 'Result', 'IsHit', 'TargetAC') -BindingFlags $bindingFlags)
    }
    else { @() }
    $calculateAcMembers = if ($ruleCalculateAcType) {
        @(Get-KmgNamedMembers -Type $ruleCalculateAcType -Names @('Initiator', 'm_Initiator', 'Target', 'm_Target', 'TargetAC', 'm_TargetAC', 'Result') -BindingFlags $bindingFlags)
    }
    else { @() }
    $distanceToMethods = if ($unitEntityDataType) {
        @(
            $unitEntityDataType.GetMethods($bindingFlags) |
                Where-Object {
                    $_.Name -eq 'DistanceTo' -and
                    -not $_.IsStatic -and
                    $_.GetParameters().Count -eq 1
                }
        )
    }
    else { @() }

    $writableTargetAcMembers = if ($ruleCalculateAcType) {
        @(Get-KmgWritableTargetAcMembers -RuleCalculateAcType $ruleCalculateAcType)
    }
    else { @() }
    $calculateAcInitiatorMembers = @(
        $calculateAcMembers | Where-Object { @('Initiator', 'm_Initiator') -contains $_.Name }
    )
    $calculateAcTargetMembers = @(
        $calculateAcMembers | Where-Object { @('Target', 'm_Target') -contains $_.Name }
    )
    $unitStatsMember = if ($unitEntityDataType) {
        Find-KmgFieldOrProperty -Type $unitEntityDataType -Name 'Stats' -BindingFlags $bindingFlags
    }
    else { $null }
    $unitStatsType = Get-KmgMemberValueType -Member $unitStatsMember
    $armorClassMember = if ($unitStatsType) {
        Find-KmgFieldOrProperty -Type $unitStatsType -Name 'AC' -BindingFlags $bindingFlags
    }
    else { $null }
    $armorClassType = Get-KmgMemberValueType -Member $armorClassMember
    $ordinaryAcMembers = if ($armorClassType) {
        @(Get-KmgNamedMembers -Type $armorClassType -Names @('ModifiedValue', 'Value') -BindingFlags $bindingFlags)
    }
    else { @() }
    $touchAcMembers = if ($armorClassType) {
        @(Get-KmgNamedMembers -Type $armorClassType -Names @('Touch', 'TouchAC', 'TouchValue') -BindingFlags $bindingFlags)
    }
    else { @() }

    $newtonsoftAssembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($newtonsoftAssemblyPath)
    $jsonPropertyAttributeType = $newtonsoftAssembly.GetType(
        'Newtonsoft.Json.JsonPropertyAttribute',
        $false,
        $false)

    $harmonyAssembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($harmonyAssemblyPath)
    $harmonyType = $harmonyAssembly.GetType('Harmony12.HarmonyInstance', $false, $false)
    if (-not $harmonyType) {
        throw 'Type Harmony12.HarmonyInstance was not found in 0Harmony12.dll.'
    }
    $createMethods = @(
        $harmonyType.GetMethods($bindingFlags) |
            Where-Object { $_.Name -eq 'Create' } |
            ForEach-Object { Convert-KmgMethodDescription -Method $_ }
    )
    $patchAllMethods = @(
        $harmonyType.GetMethods($bindingFlags) |
            Where-Object { $_.Name -eq 'PatchAll' } |
            ForEach-Object { Convert-KmgMethodDescription -Method $_ }
    )
    $compatibleCreateMethods = @(
        $harmonyType.GetMethods($bindingFlags) |
            Where-Object {
                $_.Name -eq 'Create' -and
                $_.IsStatic -and
                $_.GetParameters().Count -eq 1 -and
                $_.GetParameters()[0].ParameterType.FullName -eq 'System.String'
            }
    )
    $compatiblePatchAllMethods = @(
        $harmonyType.GetMethods($bindingFlags) |
            Where-Object {
                $_.Name -eq 'PatchAll' -and
                -not $_.IsStatic -and
                $_.GetParameters().Count -eq 1 -and
                $_.GetParameters()[0].ParameterType.FullName -eq 'System.Reflection.Assembly'
            }
    )

    $ummAssembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($ummAssemblyPath)
    $modEntryType = $ummAssembly.GetType('UnityModManagerNet.UnityModManager+ModEntry', $false, $false)
    $modLoggerType = $ummAssembly.GetType('UnityModManagerNet.UnityModManager+ModEntry+ModLogger', $false, $false)
    $onGuiMember = if ($modEntryType) {
        Find-KmgFieldOrProperty -Type $modEntryType -Name 'OnGUI' -BindingFlags $bindingFlags
    }
    else {
        $null
    }

    $assetGuidContractPassed = (
        $assetGuidField -ne $null -and
        $assetGuidField.FieldType.FullName -eq 'System.String' -and
        -not $assetGuidField.IsStatic
    )
    $blueprintComponentBaseTypes = @()
    $currentComponentBaseType = $blueprintComponentType
    while ($currentComponentBaseType) {
        $blueprintComponentBaseTypes += $currentComponentBaseType.FullName
        $currentComponentBaseType = $currentComponentBaseType.BaseType
    }
    $componentDerivesScriptableObject =
        $blueprintComponentBaseTypes -contains 'UnityEngine.ScriptableObject'
    $componentContractPassed = (
        $blueprintComponentType -ne $null -and
        -not $blueprintComponentType.IsSealed -and
        $componentDerivesScriptableObject
    )
    $featureContractPassed = (
        $componentsArrayMember -ne $null -and
        $hideInUiMember -ne $null -and
        $ranksMember -ne $null
    )
    $libraryRegistrationContractPassed = (
        $blueprintsByAssetIdMember -ne $null -and
        $getAllBlueprintsMethods.Count -eq 1
    )
    $weaponCloneContractPassed = (
        $blueprintWeaponType -ne $null -and
        $blueprintItemWeaponType -ne $null -and
        $blueprintWeaponType.IsSubclassOf($blueprintBaseType) -and
        $blueprintItemWeaponType.IsSubclassOf($blueprintBaseType) -and
        $weaponTypeMembers.Count -ge 1 -and
        ($preferredWeaponTypeMembers.Count -ge 1 -or $weaponTypeMembers.Count -eq 1)
    )
    $firearmProficiencyContractPassed = (
        $equipmentRestrictionType -ne $null -and
        -not $equipmentRestrictionType.IsSealed -and
        $equipmentRestrictionType.IsSubclassOf($blueprintComponentType) -and
        $canBeEquippedMethods.Count -ge 1 -and
        $unitDescriptorProgressionMember -ne $null -and
        $progressionFeaturesMember -ne $null -and
        @($getRankMethods).Count -eq 1
    )
    $developmentUiContractPassed = (
        $onGuiMember -ne $null -and
        $gameInstanceMember -ne $null -and
        $gamePlayerMember -ne $null
    )
    $developmentBridgeContractPassed = (
        $playerType -ne $null -and
        @($mainCharacterMembers).Count -ge 1 -and
        $inventoryMembers.Count -ge 1 -and
        $inventoryAddMethods.Count -ge 1 -and
        $inventoryRemoveMethods.Count -ge 1 -and
        @($addFeatureMethods).Count -eq 1
    )
    $runtimeItemStateContractPassed = (
        $itemEntityWeaponType -ne $null -and
        $itemEntityWeaponBlueprintMembers.Count -ge 1
    )
    $firearmItemIdentityContractPassed = (
        $itemEntityWeaponType -ne $null -and
        $itemEntityWeaponUniqueIdMembers.Count -eq 1 -and
        $itemEntityWeaponUniqueIdReadable -and
        -not $itemEntityWeaponUniqueIdStatic -and
        $itemEntityWeaponUniqueIdType -ne $null -and
        @('System.Guid', 'System.String') -contains $itemEntityWeaponUniqueIdType.FullName
    )
    $persistenceTokenContractPassed = (
        $blueprintWeaponEnchantmentType -ne $null -and
        -not $blueprintWeaponEnchantmentType.IsAbstract -and
        $blueprintWeaponEnchantmentType.IsSubclassOf($blueprintBaseType) -and
        $itemEntityType -ne $null -and
        $itemEntityWeaponType -ne $null -and
        $itemEntityType.IsAssignableFrom($itemEntityWeaponType) -and
        @($itemApplyEnchantmentsMethods).Count -eq 1 -and
        $itemEnchantmentType -ne $null -and
        @($itemEnchantmentCollectionMembers).Count -ge 1 -and
        @($itemEnchantmentBlueprintMembers).Count -ge 1 -and
        @($compatibleAddEnchantmentMethods).Count -ge 1 -and
        @($compatibleRemoveEnchantmentMethods).Count -ge 1
    )
    $unitPartGetContractPassed = (
        @($unitPartInstanceGetMethods).Count -ge 1 -or
        @($unitPartExtensionGetMethods).Count -ge 1
    )
    $unitPartEnsureContractPassed = (
        @($unitPartInstanceEnsureMethods).Count -ge 1 -or
        @($unitPartExtensionEnsureMethods).Count -ge 1
    )
    $unitPartVaultContractPassed = (
        $unitPartType -ne $null -and
        -not $unitPartType.IsSealed -and
        $unitEntityDataType -ne $null -and
        $itemEntityWeaponType -ne $null -and
        $mainCharacterMembers.Count -ge 1 -and
        $jsonPropertyAttributeType -ne $null -and
        $unitPartGetContractPassed -and
        $unitPartEnsureContractPassed
    )
    $combatTracePatchContractPassed = (
        $rulebookEventContextType -ne $null -and
        $ruleAttackWithWeaponType -ne $null -and
        $ruleAttackRollType -ne $null -and
        $ruleCalculateAcType -ne $null -and
        @($ruleAttackWithWeaponOnTriggerMethods).Count -eq 1 -and
        @($ruleAttackRollOnTriggerMethods).Count -eq 1 -and
        @($ruleCalculateAcOnTriggerMethods).Count -eq 1
    )
    $combatTraceDataContractPassed = (
        @($weaponAttackMembers | Where-Object { @('Weapon', 'm_Weapon') -contains $_.Name }).Count -ge 1 -and
        @($attackRollMembers | Where-Object { @('Weapon', 'm_Weapon', 'RuleAttackWithWeapon') -contains $_.Name }).Count -ge 1 -and
        @($attackRollMembers | Where-Object { @('D20', 'RollResult', 'NaturalRoll', 'Result', 'AttackRoll') -contains $_.Name }).Count -ge 1 -and
        @($calculateAcMembers | Where-Object { $_.Name -eq 'TargetAC' }).Count -ge 1 -and
        $unitEntityDataType -ne $null -and
        @($distanceToMethods).Count -ge 1
    )
    $combatTraceContractPassed = $combatTracePatchContractPassed -and $combatTraceDataContractPassed
    $compatibleRollHistoryFields = @(
        $rulebookRollEntryFields | Where-Object {
            if ($_.Name -ne 'RollHistory' -or -not $_.FieldType.IsGenericType) {
                return $false
            }

            $genericDefinition = $_.FieldType.GetGenericTypeDefinition()
            $genericArguments = @($_.FieldType.GetGenericArguments())
            return $genericDefinition.FullName -eq 'System.Collections.Generic.List`1' -and
                $genericArguments.Count -eq 1 -and
                $genericArguments[0].FullName -eq 'System.Int32'
        }
    )
    $firearmMisfireContractPassed = (
        $ruleAttackRollType -ne $null -and
        $rulebookRollEntryType -ne $null -and
        $rulebookRollEntryType.IsValueType -and
        @($ruleAttackRollMainRollSetterMethods).Count -eq 1 -and
        @($ruleAttackRollSuccessMethods).Count -eq 1 -and
        @($rulebookRollEntryFields | Where-Object { $_.Name -eq 'Value' -and $_.FieldType.FullName -eq 'System.Int32' }).Count -eq 1 -and
        @($compatibleRollHistoryFields).Count -eq 1
    )
    $firearmArmorClassContractPassed = (
        $rulebookEventContextType -ne $null -and
        $ruleAttackRollType -ne $null -and
        $ruleCalculateAcType -ne $null -and
        @($ruleAttackRollOnTriggerMethods).Count -eq 1 -and
        @($ruleCalculateAcOnTriggerMethods).Count -eq 1 -and
        @($attackRollMembers | Where-Object { @('Weapon', 'm_Weapon', 'RuleAttackWithWeapon') -contains $_.Name }).Count -ge 1 -and
        @($calculateAcInitiatorMembers).Count -ge 1 -and
        @($calculateAcTargetMembers).Count -ge 1 -and
        @($writableTargetAcMembers).Count -eq 1 -and
        $unitStatsType -ne $null -and
        $armorClassType -ne $null -and
        @($ordinaryAcMembers | Where-Object { (Get-KmgMemberValueType -Member $_).FullName -eq 'System.Int32' }).Count -ge 1 -and
        @($touchAcMembers | Where-Object { (Get-KmgMemberValueType -Member $_).FullName -eq 'System.Int32' }).Count -ge 1 -and
        @($distanceToMethods).Count -ge 1
    )
    $contractPassed = (
        @($zeroArgumentLoadDictionaryMethods).Count -eq 1 -and
        @($compatibleCreateMethods).Count -ge 1 -and
        @($compatiblePatchAllMethods).Count -ge 1 -and
        $modEntryType -ne $null -and
        $modLoggerType -ne $null -and
        $assetGuidContractPassed -and
        $componentContractPassed -and
        $featureContractPassed -and
        $libraryRegistrationContractPassed -and
        $weaponCloneContractPassed -and
        $firearmProficiencyContractPassed -and
        $developmentUiContractPassed -and
        $developmentBridgeContractPassed -and
        $runtimeItemStateContractPassed -and
        $persistenceTokenContractPassed -and
        $combatTraceContractPassed -and
        $firearmArmorClassContractPassed -and
        $firearmMisfireContractPassed
    )

    $result = [ordered]@{
        capturedAtUtc = [DateTime]::UtcNow.ToString('o')
        kingmakerInstallDirectory = $KingmakerInstallDir
        sprint = 23
        milestone = '0.0.29-s29-complete-maintenance-loop'
        contractPassed = $contractPassed
        inspection = [ordered]@{
            policy = 'stable-name-before-required-parameter-metadata'
            unrelatedStaticMethodsExcludedBeforeParameterInspection =
                $unitPartUnrelatedMethodsExcludedBeforeParameterInspection
            toleratedLoaderFailures = @()
        }
        assemblies = @(
            [ordered]@{
                relativePath = 'Assembly-CSharp.dll'
                identity = [Reflection.AssemblyName]::GetAssemblyName($gameAssemblyPath).FullName
                sha256 = Get-KmgSha256 -Path $gameAssemblyPath
            },
            [ordered]@{
                relativePath = 'UnityModManager/0Harmony12.dll'
                identity = [Reflection.AssemblyName]::GetAssemblyName($harmonyAssemblyPath).FullName
                sha256 = Get-KmgSha256 -Path $harmonyAssemblyPath
            },
            [ordered]@{
                relativePath = 'UnityModManager/UnityModManager.dll'
                identity = [Reflection.AssemblyName]::GetAssemblyName($ummAssemblyPath).FullName
                sha256 = Get-KmgSha256 -Path $ummAssemblyPath
            }
        )
        kingmaker = [ordered]@{
            libraryType = $libraryType.FullName
            loadDictionaryMethods = $loadDictionaryMethods
            compatibleZeroArgumentLoadDictionaryCount = $zeroArgumentLoadDictionaryMethods.Count
            getAllBlueprintsMethods = @($getAllBlueprintsMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
            blueprintsByAssetIdMember = if ($blueprintsByAssetIdMember) { Convert-KmgMemberDescription -Member $blueprintsByAssetIdMember } else { $null }
            blueprintBaseType = $blueprintBaseType.FullName
            blueprintComponentType = $blueprintComponentType.FullName
            blueprintComponentIsSealed = $blueprintComponentType.IsSealed
            blueprintComponentBaseTypes = $blueprintComponentBaseTypes
            componentDerivesScriptableObject = $componentDerivesScriptableObject
            componentContractPassed = $componentContractPassed
            assetGuidField = if ($assetGuidField) { Convert-KmgMemberDescription -Member $assetGuidField } else { $null }
            componentsArrayMember = if ($componentsArrayMember) { Convert-KmgMemberDescription -Member $componentsArrayMember } else { $null }
            blueprintFeatureType = $blueprintFeatureType.FullName
            hideInUiMember = if ($hideInUiMember) { Convert-KmgMemberDescription -Member $hideInUiMember } else { $null }
            ranksMember = if ($ranksMember) { Convert-KmgMemberDescription -Member $ranksMember } else { $null }
            blueprintWeaponType = $blueprintWeaponType.FullName
            blueprintItemWeaponType = $blueprintItemWeaponType.FullName
            writableWeaponTypeMembers = @(
                $weaponTypeMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ }
            )
            preferredWritableWeaponTypeMemberCount = $preferredWeaponTypeMembers.Count
            assetGuidContractPassed = $assetGuidContractPassed
            featureContractPassed = $featureContractPassed
            libraryRegistrationContractPassed = $libraryRegistrationContractPassed
            weaponCloneContractPassed = $weaponCloneContractPassed
            equipmentRestrictionType = $equipmentRestrictionType.FullName
            equipmentRestrictionIsSealed = $equipmentRestrictionType.IsSealed
            canBeEquippedByMethods = @($canBeEquippedMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
            unitDescriptorType = $unitDescriptorType.FullName
            unitDescriptorProgressionMember = Convert-KmgMemberDescription -Member $unitDescriptorProgressionMember
            progressionFeaturesMember = Convert-KmgMemberDescription -Member $progressionFeaturesMember
            featureCollectionType = if ($featureCollectionType) { $featureCollectionType.FullName } else { $null }
            getRankMethods = @($getRankMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
            getRankNamedCandidates = @($getRankNamedMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
            gameType = $gameType.FullName
            gameInstanceMember = if ($gameInstanceMember) { Convert-KmgMemberDescription -Member $gameInstanceMember } else { $null }
            gamePlayerMember = if ($gamePlayerMember) { Convert-KmgMemberDescription -Member $gamePlayerMember } else { $null }
            playerType = if ($playerType) { $playerType.FullName } else { $null }
            mainCharacterMembers = @($mainCharacterMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
            inventoryMembers = @($inventoryMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
            inventoryTypes = @($inventoryTypes | ForEach-Object { $_.FullName })
            inventoryAddMethods = @($inventoryAddMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
            inventoryRemoveMethods = @($inventoryRemoveMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
            addFeatureMethods = @($addFeatureMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
            addFeatureNamedCandidates = @($addFeatureNamedMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
            disputedGateProvenance = [ordered]@{
                obsoleteUnitDescriptorGetFeature = 'replaced by reachable UnitDescriptor.Progression.Features.GetRank production call'
                obsoleteUnitDescriptorAddFactOrAddFeature = 'replaced by reachable UnitDescriptor.Progression.Features.AddFeature production call'
            }
            firearmProficiencyContractPassed = $firearmProficiencyContractPassed
            developmentBridgeContractPassed = $developmentBridgeContractPassed
            runtimeItemState = [ordered]@{
                itemEntityType = if ($itemEntityType) { $itemEntityType.FullName } else { $null }
                itemEntityWeaponType = if ($itemEntityWeaponType) { $itemEntityWeaponType.FullName } else { $null }
                blueprintMembers = @($itemEntityWeaponBlueprintMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                runtimeIdMembers = @($itemEntityWeaponRuntimeIdMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                identityPolicy = 'Runtime firearm identity is the exact ItemEntityWeapon reference plus exactly one FirearmDefinitionComponent marker. Item UniqueId is not required.'
                authoritativeCarrier = 'item-owned inert BlueprintWeaponEnchantment token'
                absenceMeans = 'Empty/Normal'
                contractPassed = $runtimeItemStateContractPassed
            }
            rejectedHistoricalItemIdentity = [ordered]@{
                memberName = 'UniqueId'
                historicallyAcceptedValueTypes = @('System.Guid', 'System.String')
                matchingMembers = @($itemEntityWeaponUniqueIdMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                matchingMemberCount = $itemEntityWeaponUniqueIdMembers.Count
                memberReadable = $itemEntityWeaponUniqueIdReadable
                memberIsStatic = $itemEntityWeaponUniqueIdStatic
                memberValueType = if ($itemEntityWeaponUniqueIdType) { $itemEntityWeaponUniqueIdType.FullName } else { $null }
                generatedByMod = $false
                fallbackMembersAccepted = $false
                requiredForCurrentBuild = $false
                rejectedByInstalledRuntimeEvidence = -not $firearmItemIdentityContractPassed
            }
            rejectedHistoricalIdentityVault = [ordered]@{
                carrier = 'save-owned UnitPart keyed by ItemEntityWeapon.UniqueId'
                requiredForCurrentBuild = $false
                unitPartType = if ($unitPartType) { $unitPartType.FullName } else { $null }
                historicalContractPassed = $unitPartVaultContractPassed -and $firearmItemIdentityContractPassed
                policy = 'Compiler diagnostics only. Do not reactivate this carrier or emit new writes.'
            }
            firearmStateTokenCarrier = [ordered]@{
                purpose = 'Authoritative per-item firearm state carrier. Inspect and mutate only ItemEntityWeapon enchantments; reconcile around the exact ItemEntity.ApplyEnchantments() method.'
                blueprintWeaponEnchantmentType = if ($blueprintWeaponEnchantmentType) { $blueprintWeaponEnchantmentType.FullName } else { $null }
                blueprintWeaponEnchantmentIsAbstract = if ($blueprintWeaponEnchantmentType) { $blueprintWeaponEnchantmentType.IsAbstract } else { $null }
                itemEntityApplyEnchantmentsMethods = @($itemApplyEnchantmentsMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
                itemEnchantmentType = if ($itemEnchantmentType) { $itemEnchantmentType.FullName } else { $null }
                enchantmentCollectionMembers = @($itemEnchantmentCollectionMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                itemEnchantmentBlueprintMembers = @($itemEnchantmentBlueprintMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                addEnchantmentMethods = @($itemAddEnchantmentMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
                compatibleAddEnchantmentMethodCount = $compatibleAddEnchantmentMethods.Count
                removeEnchantmentMethods = @($itemRemoveEnchantmentMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
                compatibleRemoveEnchantmentMethodCount = $compatibleRemoveEnchantmentMethods.Count
                contractPassed = $persistenceTokenContractPassed
            }
            combatTracing = [ordered]@{
                rulebookEventContextType = if ($rulebookEventContextType) { $rulebookEventContextType.FullName } else { $null }
                requiredOnTriggerSignature = 'System.Void OnTrigger(Kingmaker.RuleSystem.RulebookEventContext)'
                ruleAttackWithWeaponType = if ($ruleAttackWithWeaponType) { $ruleAttackWithWeaponType.FullName } else { $null }
                ruleAttackWithWeaponOnTriggerMethods = @($ruleAttackWithWeaponOnTriggerMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
                ruleAttackWithWeaponMembers = @($weaponAttackMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                ruleAttackRollType = if ($ruleAttackRollType) { $ruleAttackRollType.FullName } else { $null }
                ruleAttackRollOnTriggerMethods = @($ruleAttackRollOnTriggerMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
                ruleAttackRollMembers = @($attackRollMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                ruleCalculateAcType = if ($ruleCalculateAcType) { $ruleCalculateAcType.FullName } else { $null }
                ruleCalculateAcOnTriggerMethods = @($ruleCalculateAcOnTriggerMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
                ruleCalculateAcMembers = @($calculateAcMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                unitEntityDataType = if ($unitEntityDataType) { $unitEntityDataType.FullName } else { $null }
                distanceToMethods = @($distanceToMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
                patchContractPassed = $combatTracePatchContractPassed
                dataContractPassed = $combatTraceDataContractPassed
                contractPassed = $combatTraceContractPassed
            }
            naturalRollMisfire = [ordered]@{
                ruleAttackRollType = if ($ruleAttackRollType) { $ruleAttackRollType.FullName } else { $null }
                rollEntryType = if ($rulebookRollEntryType) { $rulebookRollEntryType.FullName } else { $null }
                rollEntryIsValueType = if ($rulebookRollEntryType) { $rulebookRollEntryType.IsValueType } else { $null }
                rollEntryFields = @($rulebookRollEntryFields | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                mainRollSetterMethods = @($ruleAttackRollMainRollSetterMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
                successEvaluatorMethods = @($ruleAttackRollSuccessMethods | ForEach-Object { Convert-KmgMethodDescription -Method $_ })
                requiredMainRollSetterSignature = 'private System.Void set_Roll(Kingmaker.RuleSystem.RulebookEvent+RollEntry)'
                requiredSuccessEvaluatorSignature = 'public System.Boolean IsSuccessRoll(System.Int32)'
                contractPassed = $firearmMisfireContractPassed
            }
            firearmArmorClass = [ordered]@{
                writableTargetAcMembers = @($writableTargetAcMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                calculateAcInitiatorMembers = @($calculateAcInitiatorMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                calculateAcTargetMembers = @($calculateAcTargetMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                unitStatsMember = if ($unitStatsMember) { Convert-KmgMemberDescription -Member $unitStatsMember } else { $null }
                unitStatsType = if ($unitStatsType) { $unitStatsType.FullName } else { $null }
                armorClassMember = if ($armorClassMember) { Convert-KmgMemberDescription -Member $armorClassMember } else { $null }
                armorClassType = if ($armorClassType) { $armorClassType.FullName } else { $null }
                ordinaryAcMembers = @($ordinaryAcMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                touchAcMembers = @($touchAcMembers | ForEach-Object { Convert-KmgMemberDescription -Member $_ })
                contractPassed = $firearmArmorClassContractPassed
            }
        }
        harmony12 = [ordered]@{
            type = $harmonyType.FullName
            createMethods = $createMethods
            compatibleCreateMethodCount = $compatibleCreateMethods.Count
            patchAllMethods = $patchAllMethods
            compatiblePatchAllMethodCount = $compatiblePatchAllMethods.Count
        }
        unityModManager = [ordered]@{
            modEntryTypeFound = $modEntryType -ne $null
            modLoggerTypeFound = $modLoggerType -ne $null
            onGuiMember = if ($onGuiMember) { Convert-KmgMemberDescription -Member $onGuiMember } else { $null }
            developmentUiContractPassed = $developmentUiContractPassed
        }
    }

    $result | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
    Write-Host "Wrote runtime contract report: $OutputPath"

    if (-not $contractPassed) {
        throw 'The installed assemblies do not satisfy the Sprint 29 retained attack, roll, state, save, damage, item, inventory, and maintenance runtime contract. Review the generated JSON report; do not fall back to ItemEntityWeapon.UniqueId, a guessed rule-event overload, or a global dice patch.'
    }
}
finally {
    [AppDomain]::CurrentDomain.remove_ReflectionOnlyAssemblyResolve($resolveHandler)
    Remove-Variable -Name KmgReflectionSearchDirectories -Scope Script -ErrorAction SilentlyContinue
}
