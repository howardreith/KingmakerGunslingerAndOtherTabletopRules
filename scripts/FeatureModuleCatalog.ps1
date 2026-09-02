Set-StrictMode -Version Latest

function Get-KmgFeatureModuleCatalog {
    @(
        [pscustomobject]@{
            InternalProperty = 'Gunslinger'
            JsonKey = 'gunslinger'
            DisplayName = 'Gunslinger'
            DependencyState = 'independent'
            RuntimeParameter = 'gunslinger'
        }
        [pscustomobject]@{
            InternalProperty = 'AcadamaeGraduate'
            JsonKey = 'acadamae-graduate'
            DisplayName = 'Acadamae Graduate and Cord of Stubborn Resolve'
            DependencyState = 'independent'
            RuntimeParameter = 'acadamaeGraduate'
        }
        [pscustomobject]@{
            InternalProperty = 'ShieldOther'
            JsonKey = 'shield-other'
            DisplayName = 'Shield Other'
            DependencyState = 'independent'
            RuntimeParameter = 'shieldOther'
        }
        [pscustomobject]@{
            InternalProperty = 'ExpandedSummoning'
            JsonKey = 'expanded-summoning'
            DisplayName = 'Expanded Summoning'
            DependencyState = 'independent'
            RuntimeParameter = 'expandedSummoning'
        }
        [pscustomobject]@{
            InternalProperty = 'ElvenBranchedSpears'
            JsonKey = 'elven-branched-spears'
            DisplayName = 'Elven Branched Spears'
            DependencyState = 'independent'
            RuntimeParameter = 'elvenBranchedSpears'
        }
        [pscustomobject]@{
            InternalProperty = 'EasternWeapons'
            JsonKey = 'eastern-weapons'
            DisplayName = 'Eastern Weapons'
            DependencyState = 'independent'
            RuntimeParameter = 'easternWeapons'
        }
        [pscustomobject]@{
            InternalProperty = 'BrownFurTransmuter'
            JsonKey = 'brown-fur-transmuter'
            DisplayName = 'Brown-Fur Transmuter  requires Call of the Wild'
            DependencyState = 'requires-call-of-the-wild'
            RuntimeParameter = 'brownFurTransmuter'
        }
        [pscustomobject]@{
            InternalProperty = 'UrbanBarbarian'
            JsonKey = 'urban-barbarian'
            DisplayName = 'Urban Barbarian'
            DependencyState = 'independent'
            RuntimeParameter = 'urbanBarbarian'
        }
        [pscustomobject]@{
            InternalProperty = 'BodyguardFeats'
            JsonKey = 'bodyguard-feats'
            DisplayName = 'Bodyguard and In Harms Way'
            DependencyState = 'independent'
            RuntimeParameter = 'bodyguardFeats'
        }
        [pscustomobject]@{
            InternalProperty = 'ProtectionFromAlignmentControlImmunity'
            JsonKey = 'protection-from-alignment-control-immunity'
            DisplayName = 'Protection from Alignment: control immunity'
            DependencyState = 'independent'
            RuntimeParameter = 'protectionFromAlignmentControlImmunity'
        }
        [pscustomobject]@{
            InternalProperty = 'ElementalRaces'
            JsonKey = 'elemental-races'
            DisplayName = 'Elemental Races: Ifrit, Oread, Sylph, and Undine (preview)'
            DependencyState = 'independent-preview'
            RuntimeParameter = 'elementalRaces'
        }
    )
}

function Get-KmgFeatureModuleConfigurations {
    param([switch]$Boundary)

    $catalog = @(Get-KmgFeatureModuleCatalog)
    $moduleCount = $catalog.Count
    $exhaustiveCount = 1 -shl $moduleCount
    $states = [Collections.Generic.List[object]]::new()
    for ($mask = $exhaustiveCount - 1; $mask -ge 0; $mask--) {
        $values = [ordered]@{}
        $labels = [Collections.Generic.List[string]]::new()
        $enabledCount = 0
        for ($index = 0; $index -lt $moduleCount; $index++) {
            $enabled = ($mask -band (1 -shl ($moduleCount - 1 - $index))) -ne 0
            $values[$catalog[$index].RuntimeParameter] = $enabled
            $labels.Add($(if ($enabled) { 'on' } else { 'off' }))
            if ($enabled) { $enabledCount++ }
        }
        if ($Boundary -and -not ($enabledCount -eq 0 -or $enabledCount -eq 1 -or
            $enabledCount -eq $moduleCount - 1 -or $enabledCount -eq $moduleCount)) {
            continue
        }
        $states.Add([pscustomobject]@{
            Name = $labels -join '-'
            Values = $values
        })
    }
    $expectedCount = if ($Boundary) { 2 + 2 * $moduleCount } else {
        $exhaustiveCount
    }
    if ($states.Count -ne $expectedCount) {
        throw "Feature-module matrix expected $expectedCount states for $moduleCount modules; observed $($states.Count)."
    }
    return $states.ToArray()
}
