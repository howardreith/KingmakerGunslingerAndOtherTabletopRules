[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$ModAssemblyPath,
    [string]$KingmakerInstallDir='C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker'
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
# Read-only target preparation against the actual installed Harmony12 shim.
# Never call Patch/PatchAll or a native UI/game method. The shim constructor
# resolves/validates metadata and stages Harmony2 processors; it applies none.
$managed=Join-Path $KingmakerInstallDir 'Kingmaker_Data/Managed'
$modPath=(Resolve-Path -LiteralPath $ModAssemblyPath).Path
$search=@($managed,(Join-Path $managed 'UnityModManager'),(Split-Path -Parent $modPath),(Join-Path $KingmakerInstallDir 'Mods/CallOfTheWild'))
$resolver=[ResolveEventHandler]{
    param($sender,$eventArgs)
    $file=([Reflection.AssemblyName]$eventArgs.Name).Name+'.dll'
    foreach($dir in $search){$path=Join-Path $dir $file;if(Test-Path -LiteralPath $path){return [Reflection.Assembly]::LoadFrom($path)}}
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try {
    $shimPath=Join-Path $managed 'UnityModManager/0Harmony12.dll'
    if((Get-FileHash -LiteralPath $shimPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne 'aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292'){
        throw 'Installed Harmony12 differs from the inspected metadata-only preparation contract.'
    }
    [void][Reflection.Assembly]::LoadFrom((Join-Path $managed 'UnityModManager/0Harmony.dll'))
    $shim=[Reflection.Assembly]::LoadFrom($shimPath)
    $game=[Reflection.Assembly]::LoadFrom((Join-Path $managed 'Assembly-CSharp.dll'))
    if($game.ManifestModule.ModuleVersionId.ToString() -ne '07fa1e4d-8618-41b3-9b8d-faa17d3b26f7'){throw 'Installed game MVID differs from the supported 2.1.7b contract.'}
    $mod=[Reflection.Assembly]::LoadFrom($modPath)
    $flags=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
    $hm=$shim.GetType('Harmony12.HarmonyMethod',$true)
    $hi=$shim.GetType('Harmony12.HarmonyInstance',$true)
    $pp=$shim.GetType('Harmony12.PatchProcessor',$true)
    $ext=$shim.GetType('Harmony12.HarmonyMethodExtensions',$true)
    $getAttributes=$ext.GetMethod('GetHarmonyMethods',[Type[]]@([Type]))
    $merge=@($hm.GetMethods($flags)|Where-Object{$_.Name -eq 'Merge' -and $_.GetParameters().Count -eq 1})[0]
    $ctor=$pp.GetConstructor($flags,$null,[Type[]]@($hi,[Type],$hm),$null)
    $harmony=$hi.GetMethod('Create',[Type[]]@([string])).Invoke($null,@('kmg.cleanup2.console.targets.'+[guid]::NewGuid().ToString('N')))
    $catalog=@(
        @('ElementalConsoleLearningIntakePatch','CharGenSpellsPhaseVM','GetSpellsCollection',298),
        @('ElementalConsoleLearningProjectionPatch','SelectorEntitiesGroupView`2','GenerateVirtualViews',339),
        @('ElementalConsoleLearningHeaderTextPatch','CharGenSpellSelectorItemView','RefreshView',165),
        @('ElementalConsoleLearningConfirmPatch','CharGenSpellsPhaseDetailedView','PressConfirmOnPhase',261),
        @('ElementalConsolePreparationIntakePatch','SpellBookKnownSpellsVM','.ctor',206),
        @('ElementalConsolePreparationProjectionPatch','SpellbookKnownSpellsScrollView','GenerateVirtualViews',228),
        @('ElementalConsolePreparationConfirmPatch','SpellBookView','WantConfirm',232),
        @('ElementalConsolePreparationHintPatch','SpellBookView','SetupHintsLabels',156),
        @('ElementalConsolePreparationSelectedPatch','SpellBookVM','SelectKnownSpell',13),
        @('ElementalConsolePreparationBudgetGuardPatch','SpellBookVM','TryMemorize',79),
        @('ElementalConsolePreparationContextGuardPatch','SpellBookVM','TryContext',357),
        @('ElementalConsolePreparationMetaGuardPatch','SpellBookVM','SwitchMetaMagicBuilderMode',40),
        @('ElementalConsoleLearningHeaderTooltipPatch','CharGenSpellSelectorItemView','TooltipObject',12),
        @('ElementalConsolePreparationHeaderTooltipPatch','AbilityDataVM','TooltipObject',7),
        @('ElementalConsoleSpellCollectionPatch','ActionBarSelectorVm','CollectSpells',508),
        @('ElementalConsoleSourceResultPatch','ActionBarVM','Result',67),
        @('ElementalConsoleSourceViewPatch','ActionBarSelectorView','Bind',-1),
        @('DruidSummonConversionVariantPatch','MechanicActionBarSlotMemorizedSpell','GetConvertedAbilityData',287),
        @('DruidSummonConversionVariantMembershipPatch','AbilityData','HasVariant',18),
        @('DruidSummonConversionPopupRowsPatch','ActionBarGroupSlot','SetToggleAdditionalSpells',468),
        @('DruidSummonConversionSourceSlotPatch','ActionBarGroupSlot','OnToggleGroupClick',-1),
        @('DruidSummonConversionMenuPatch','ActionBarSpellsGroup','Toggle',-1),
        @('GiantSummonNativeVisibilityPatch','EntityFader','SetRendererVisibility',-1),
        # Desktop/presentation patches: a Prepare pin that misses the installed
        # method silently disables its surface (the PC preparation fold did).
        @('ElementalLearningRowsPatch','CharBSelectorLayer','SetupSpellSelectorView',377),
        @('ElementalLearningRefreshRowsPatch','CharBSelectorLayer','FillSpellLightUpdate',438),
        @('ElementalPreparationRowsPatch','SpellBookView','RefreshSpellsView',731),
        @('ElementalSourceChooserDesktopPatch','ActionBarGroupElement','FillSlots',-1),
        @('EttinSecondHeadVisual','UnitEntityView','OnDataAttached',-1),
        @('EttinSecondHeadTeardown','UnitEntityView','OnDestroy',-1),
        @('ExpandedSummoningPteranodonViewPatch','UnitEntityView','OnDataAttached',-1),
        @('ExpandedSummoningWaspVisualTeardownPatch','UnitEntityView','OnDestroy',-1),
        @('GiantSummonPresentationPatch','UnitEntityView','OnDataAttached',-1),
        @('GiantSummonPresentationDestroyPatch','UnitEntityView','OnDestroy',-1),
        @('GiantSummonPresentationBuffPatch','Buff','SpawnParticleEffect',-1),
        @('GiantSummonDonorFrostStartFxPatch','SpawnFxOnStart','SpawnFx',99),
        @('GiantSummonDonorFrostDeathFxPatch','SpawnFxOnStart','HandleUnitDeath',79),
        @('SummonAttackGestureActPatch','UnitAttack','OnAction',-1),
        @('SummonAttackGestureTickPatch','UnitCommand','Tick',-1)
    )
    $failures=New-Object 'Collections.Generic.List[string]'
    $passed=0
    foreach($entry in $catalog){
        try {
            $type=$mod.GetType('KingmakerGunslinger.Summoning.'+$entry[0],$true)
            $attributes=,$getAttributes.Invoke($null,@($type))
            $merged=$merge.Invoke($null,$attributes)
            # This invokes the shim's real Prepare and TargetMethod resolution,
            # including the same failure that stopped the frozen21 bootstrap.
            $processor=$ctor.Invoke(@($harmony,$type,$merged))
            $originals=@($pp.GetField('originals',$flags).GetValue($processor))
            if($originals.Count -ne 1){throw "Expected one prepared target, found $($originals.Count)."}
            $target=$originals[0]
            if($target.DeclaringType.Name -cne $entry[1] -or $target.Name -cne $entry[2] -or
                $target.Module.ModuleVersionId -ne $game.ManifestModule.ModuleVersionId){throw "Wrong native target: $target"}
            $length=$target.GetMethodBody().GetILAsByteArray().Length
            if($entry[3] -ge 0 -and $length -ne $entry[3]){throw "Unexpected native IL length $length."}
            if($entry[0] -eq 'ElementalConsolePreparationIntakePatch'){
                $args=@($target.GetParameters()|ForEach-Object{$_.ParameterType.FullName})
                if($target.MetadataToken -ne 0x060050df -or $args.Count -ne 3 -or
                    $args[0] -cne 'Kingmaker.UnitLogic.Spellbook' -or
                    $args[1] -cne 'Kingmaker.UI._ConsoleUI.SpellBook.SpellBookLevelVM' -or
                    $args[2] -cne 'UniRx.IReactiveProperty`1[[Kingmaker.UI._ConsoleUI.Common.AbilityDataVM, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]'){
                    throw "Constructor token/parameter mismatch: $($args -join ', ')"
                }
            }
            $passed++
            Write-Output ('PASS '+$entry[0]+' => '+$target.DeclaringType.FullName+'::'+$target+' token=0x'+$target.MetadataToken.ToString('X8')+' bytes='+$length)
        } catch {$message=$_.Exception.GetBaseException().Message;$failures.Add($entry[0]+': '+$message);Write-Output ('FAIL '+$entry[0]+': '+$message)}
    }
    # Reproduce the previous untyped constructor lookup using the actual shim
    # resolver without executing a constructor or synthesizing a native object.
    $old=[Activator]::CreateInstance($hm)
    $hm.GetField('declaringType').SetValue($old,$game.GetType('Kingmaker.UI._ConsoleUI.SpellBook.SpellBookKnownSpellsVM',$true))
    $methodType=$shim.GetType('Harmony12.MethodType',$true)
    $hm.GetField('methodType').SetValue($old,[Enum]::Parse($methodType,'Constructor'))
    $unprepared=[Runtime.Serialization.FormatterServices]::GetUninitializedObject($pp)
    $pp.GetField('containerAttributes',$flags).SetValue($unprepared,$old)
    if($null -ne $pp.GetMethod('GetOriginalMethod',$flags).Invoke($unprepared,@())){throw 'Negative fixture unexpectedly resolved the old untyped constructor.'}
    $passed++;Write-Output 'PASS old untyped constructor remains unresolved (actual Harmony12 negative fixture)'
    # The Druid popup projection drops a prepared parent's own quantity rows
    # only because installed Fill appends that spell's variants after them.
    try {
        $fillExact=$mod.GetType('KingmakerGunslinger.Summoning.DruidSummonConversionPopupRowsPatch',$true).GetMethod('ExactVariantFill',$flags).Invoke($null,@())
        if(-not $fillExact){throw 'Installed ActionBarSpellsGroup.Fill differs from token 0x0600451C / 113 IL bytes.'}
        $passed++;Write-Output 'PASS DruidSummonConversionPopupRowsPatch.ExactVariantFill => ActionBarSpellsGroup::Fill token=0x0600451C bytes=113'
    } catch {$message=$_.Exception.GetBaseException().Message;$failures.Add('ExactVariantFill: '+$message);Write-Output ('FAIL ExactVariantFill: '+$message)}
    Write-Output ('Targets: '+$passed+'/'+($catalog.Count+2)+' passed; modSha256='+(Get-FileHash -LiteralPath $modPath -Algorithm SHA256).Hash.ToLowerInvariant())
    if($failures.Count -gt 0){throw ($failures -join [Environment]::NewLine)}
} finally {[AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver)}
