using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UI.UnitSettings;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.UrbanBarbarian;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class UrbanBarbarianFocusedScenario
    {
        private const BindingFlags Members = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private const string NativeRageFeatureGuid =
            "2479395977cfeeb46b482bc3385f4647";
        private const string NativeRageActivatableGuid =
            "df6a2cce8e3a9bd4592fb1968b83f730";
        private const string NativeRageResourceGuid =
            "24353fcf8096ea54684a72bf58dedbc9";

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            UrbanBarbarianBlueprintSet set = BlueprintBootstrap.UrbanBarbarian;
            object allUnits = Game.Instance.State.Units.All;
            object[] unitsBefore = Snapshot(allUnits);
            Kingmaker.EntitySystem.SceneEntitiesState scene = null;
            UnitEntityData urban = null;
            UnitEntityData enemyOne = null;
            UnitEntityData enemyTwo = null;
            UnitEntityData enemyThree = null;
            BlueprintUnit hostileSource = null;
            ItemEntityWeapon weapon = null;
            ItemEntityWeapon enemyWeapon = null;
            ItemEntityWeapon rangedWeapon = null;
            BlueprintAbility spellProbe = null;
            bool urbanRegistered = false;
            bool enemyOneRegistered = false;
            bool enemyTwoRegistered = false;
            bool enemyThreeRegistered = false;
            bool cleaned = false;
            string stage = "fixture";
            try
            {
                scene = new Kingmaker.EntitySystem.SceneEntitiesState(
                    "KMG_Urban_Barbarian_Focused_Fixture");
                BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
                urban = Game.Instance.EntityCreator.SpawnUnit(source,
                    Vector3.zero, Quaternion.identity, scene);
                enemyOne = ElvenBranchedSpearCombatScenario.SpawnHostileTarget(
                    urban, source, new Vector3(9f, 0f, 0f), scene,
                    out hostileSource);
                enemyTwo = Game.Instance.EntityCreator.SpawnUnit(hostileSource,
                    new Vector3(-9f, 0f, 0f), Quaternion.identity, scene);
                enemyThree = Game.Instance.EntityCreator.SpawnUnit(hostileSource,
                    new Vector3(0f, 0f, 9f), Quaternion.identity, scene);
                if (urban == null || enemyOne == null || enemyTwo == null ||
                    enemyThree == null || urban.View == null)
                    throw new InvalidOperationException(
                        "The disposable Urban live-unit fixture is incomplete.");
                urbanRegistered = Game.Instance.State.Units.All.Add(urban);
                enemyOneRegistered = Game.Instance.State.Units.All.Add(enemyOne);
                enemyTwoRegistered = Game.Instance.State.Units.All.Add(enemyTwo);
                enemyThreeRegistered = Game.Instance.State.Units.All.Add(enemyThree);
                if (!urbanRegistered || !enemyOneRegistered ||
                    !enemyTwoRegistered || !enemyThreeRegistered)
                    throw new InvalidOperationException(
                        "The disposable Urban units did not register exactly once in the native world-unit pool.");
                enemyOne.Descriptor.State.Immortality.Retain();
                enemyTwo.Descriptor.State.Immortality.Retain();
                enemyThree.Descriptor.State.Immortality.Retain();
                urban.CombatState.JoinCombat();
                enemyOne.CombatState.JoinCombat();
                enemyTwo.CombatState.JoinCombat();
                enemyThree.CombatState.JoinCombat();

                stage = "level-one-archetype";
                ApplyLevel(urban.Descriptor, set.BarbarianClass, set.Archetype,
                    true);
                Add(assertions, "urban-level-one-progression",
                    "level 1 Urban owner has proficiency, Crowd Control, Controlled Rage, only the ordinary selector, and no Fast Movement",
                    "level=" + urban.Descriptor.Progression.GetClassLevel(
                        set.BarbarianClass) + ";facts=" +
                    string.Join(",", new BlueprintUnitFact[] { set.Proficiency, set.CrowdControl,
                        set.ControlledRage, set.OrdinarySelector,
                        set.LegacySelector, set.GreaterSelector,
                        set.MightySelector }.Select(value =>
                            value.name + ":" + urban.Descriptor.HasFact(value))),
                    urban.Descriptor.Progression.GetClassLevel(
                        set.BarbarianClass) == 1 &&
                    urban.Descriptor.HasFact(set.Proficiency) &&
                    urban.Descriptor.HasFact(set.CrowdControl) &&
                    urban.Descriptor.HasFact(set.ControlledRage) &&
                    urban.Descriptor.HasFact(set.OrdinarySelector) &&
                    !urban.Descriptor.HasFact(set.LegacySelector) &&
                    !urban.Descriptor.HasFact(set.GreaterSelector) &&
                    !urban.Descriptor.HasFact(set.MightySelector) &&
                    !urban.Descriptor.HasFact(BlueprintBootstrap.Library
                        .GetAllBlueprints().OfType<BlueprintFeature>().Single(
                            value => value.AssetGuid ==
                                UrbanBarbarianBlueprints.FastMovementGuid)),
                    "native LevelUpController with exact Urban archetype");

                stage = "ordinary-controlled-rage";
                urban.Descriptor.Stats.Strength.BaseValue = 10;
                urban.Descriptor.Stats.Dexterity.BaseValue = 12;
                urban.Descriptor.Stats.Constitution.BaseValue = 14;
                urban.Descriptor.Stats.HitPoints.BaseValue = 100;
                int strBefore = urban.Descriptor.Stats.Strength.ModifiedValue;
                int dexBefore = urban.Descriptor.Stats.Dexterity.ModifiedValue;
                int conBefore = urban.Descriptor.Stats.Constitution.ModifiedValue;
                int maxBefore = urban.MaxHP;
                Buff[] beforeDefaultRage = urban.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().ToArray();
                var rageContext = new MechanicsContext(urban, urban.Descriptor,
                    set.ControlledRage, null, new TargetWrapper(urban));
                Buff rage = urban.Descriptor.Buffs.AddBuff(
                    set.NativeRageBuff, rageContext, null);
                bool substituted = rage != null && ReferenceEquals(
                    rage.Blueprint, set.RageBuff) &&
                    !urban.Descriptor.HasFact(set.NativeRageBuff);
                int strRaging = urban.Descriptor.Stats.Strength.ModifiedValue;
                int dexRaging = urban.Descriptor.Stats.Dexterity.ModifiedValue;
                int conRaging = urban.Descriptor.Stats.Constitution.ModifiedValue;
                int maxRaging = urban.MaxHP;
                Ability selectorFact = urban.Descriptor.Abilities.GetAbility(
                    set.OrdinarySelector);
                AbilityData selector = selectorFact == null ? null :
                    new AbilityData(set.OrdinarySelector, urban.Descriptor);
                AbilityData[] visibleOrdinary = LivePanelVariants(urban,
                    selector);
                bool locked = visibleOrdinary.Length == 6 &&
                    visibleOrdinary.All(value => !value.IsAvailable);
                Add(assertions, "urban-ordinary-rage",
                    "owner-scoped substitution; default STR +4 only; no temporary HP; six options locked while raging",
                    "substituted=" + substituted + ";stats=" + strBefore + "/" +
                        dexBefore + "/" + conBefore + "->" + strRaging + "/" +
                        dexRaging + "/" + conRaging + ";maxHp=" + maxBefore +
                        "->" + maxRaging + ";tempHp=" +
                        urban.Descriptor.Stats.TemporaryHitPoints.ModifiedValue +
                        ";variants=" + visibleOrdinary.Length + ";locked=" + locked,
                    substituted && strRaging == strBefore + 4 &&
                        dexRaging == dexBefore && conRaging == conBefore &&
                        urban.Descriptor.Stats.TemporaryHitPoints.ModifiedValue == 0 &&
                        visibleOrdinary.Length == 6 && locked,
                    "live BuffCollection substitution, actual stats, and MechanicActionBarSlotAbility.GetConvertedAbilityData");
                urban.Descriptor.RemoveFact(set.RageBuff);
                RemoveIntroducedBuffs(urban.Descriptor, beforeDefaultRage);

                stage = "ordinary-live-selector-presentation";
                AbilityData[] ordinaryPanel = LivePanelVariants(urban,
                    new AbilityData(set.OrdinarySelector, urban.Descriptor));
                MechanicActionBarSlotAbility ordinaryParentSlot = LiveSlot(urban,
                    new AbilityData(set.OrdinarySelector, urban.Descriptor));
                MechanicActionBarSlotAbility selectedChildSlot = LiveSlot(urban,
                    ordinaryPanel.Single(value => ControlledRageRuntime.IsSelected(
                        value)));
                Sprite selectedChildIcon = selectedChildSlot.GetIcon();
                string selectedChildTitle = selectedChildSlot.GetTitle();
                Sprite parentIcon = ordinaryParentSlot.GetIcon();
                string parentTitle = ordinaryParentSlot.GetTitle();
                bool ordinaryIcons = ordinaryPanel.All(value =>
                    value.Blueprint.Icon != null && value.Blueprint.Icon.texture !=
                        null) && ordinaryPanel.Select(value => value.Blueprint.Icon)
                    .Distinct().Count() == 6;
                Add(assertions, "urban-ordinary-live-selector-presentation",
                    "the actual player-facing grid has six distinguishable icons and the parent/selected child show the persisted selection without hover",
                    "variants=" + ordinaryPanel.Length + ";baseIcons=" +
                        string.Join(",", ordinaryPanel.Select(value =>
                            value.Blueprint.Icon.name)) + ";parentTitle=" + parentTitle +
                        ";parentIcon=" + (parentIcon == null ? "<null>" :
                            parentIcon.name) + ";selectedTitle=" +
                        selectedChildTitle + ";selectedIcon=" +
                        (selectedChildIcon == null ? "<null>" :
                            selectedChildIcon.name),
                    ordinaryPanel.Length == 6 && ordinaryIcons &&
                        parentTitle.Contains("STR +4") &&
                        selectedChildTitle.StartsWith("Selected \u2713 ",
                            StringComparison.Ordinal) && parentIcon != null &&
                        selectedChildIcon != null && ReferenceEquals(parentIcon,
                            selectedChildIcon) && selectedChildIcon.name.Contains(
                                "KMG_ControlledRage_Selected_T4_S4_D0_C0") &&
                        selectedChildIcon.texture.width == 128 &&
                        selectedChildIcon.texture.height == 128,
                    "native MechanicActionBarSlotAbility live title/icon/enumeration path");

                stage = "ordinary-split-and-hp";
                SelectDirect(urban.Descriptor, set,
                    ControlledRageTier.Ordinary, 0, 2, 2);
                int damageBefore = 17;
                urban.Descriptor.Damage = damageBefore;
                int splitDexBefore = urban.Descriptor.Stats.Dexterity.ModifiedValue;
                int splitConBefore = urban.Descriptor.Stats.Constitution.ModifiedValue;
                int hpBefore = urban.HPLeft;
                Buff[] beforeSplitRage = urban.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().ToArray();
                rage = urban.Descriptor.Buffs.AddBuff(
                    set.NativeRageBuff, rageContext, null);
                int hpDuring = urban.HPLeft;
                int maxDuring = urban.MaxHP;
                int dexSplit = urban.Descriptor.Stats.Dexterity.ModifiedValue;
                int conSplit = urban.Descriptor.Stats.Constitution.ModifiedValue;
                ControlledRageAllocation splitSelection =
                    ControlledRageRuntime.ResolveSelection(urban.Descriptor,
                        false);
                urban.Descriptor.RemoveFact(set.RageBuff);
                int hpAfter = urban.HPLeft;
                int maxAfter = urban.MaxHP;
                Add(assertions, "urban-constitution-hp-cycle",
                    "DEX +2/CON +2 actual scores; Constitution HP and damage deficit restore exactly on exit",
                    "hp=" + hpBefore + "/" + hpDuring + "/" + hpAfter +
                        ";max=" + maxBefore + "/" + maxDuring + "/" + maxAfter +
                        ";damage=" + urban.Descriptor.Damage + ";dex=" + dexSplit +
                        ";con=" + conSplit + ";selection=" + splitSelection +
                        ";dexModifiers=" + DescribeModifiers(
                            urban.Descriptor.Stats.Dexterity),
                    dexSplit == splitDexBefore + 2 &&
                        conSplit == splitConBefore + 2 &&
                        maxDuring == maxAfter + 1 && hpDuring == hpBefore + 1 &&
                        hpAfter == hpBefore &&
                        maxAfter == maxBefore &&
                        urban.Descriptor.Damage == damageBefore,
                    "genuine Constitution morale modifier and native HP/damage accounting");
                RemoveIntroducedBuffs(urban.Descriptor, beforeSplitRage);

                stage = "ordinary-allocation-and-leakage";
                weapon = ElvenBranchedSpearCombatScenario.Equip(urban,
                    BlueprintBootstrap.ElvenBranchedSpears.Entries[0].Item);
                bool ordinaryDexterity = MeasureAllocation(urban.Descriptor,
                    set, rageContext, ControlledRageTier.Ordinary,
                    0, 4, 0, out string ordinaryDexterityDetail);
                bool ordinaryConstitution = MeasureAllocation(urban.Descriptor,
                    set, rageContext, ControlledRageTier.Ordinary,
                    0, 0, 4, out string ordinaryConstitutionDetail);
                SelectDirect(urban.Descriptor, set,
                    ControlledRageTier.Ordinary, 0, 0, 4);
                int attackBeforeRage = Attack(urban, weapon);
                int damageBonusBeforeRage = ElvenBranchedSpearCombatScenario
                    .WeaponStats(urban, weapon).BonusDamage;
                int willBeforeRage = urban.Descriptor.Stats.SaveWill.ModifiedValue;
                int acBeforeRage = ArmorClass(urban, enemyOne);
                int temporaryHpBeforeRage = urban.Descriptor.Stats
                    .TemporaryHitPoints.ModifiedValue;
                Buff[] beforeLeakageRage = urban.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().ToArray();
                spellProbe = ScriptableObject.CreateInstance<BlueprintAbility>();
                spellProbe.name = "UrbanBarbarianRuntimeSpellRestrictionProbe";
                spellProbe.Type = AbilityType.Spell;
                spellProbe.ComponentsArray = Array.Empty<BlueprintComponent>();
                var spellAvailability = new AbilityData(spellProbe,
                    urban.Descriptor);
                bool spellAvailableBeforeRage = spellAvailability.IsAvailable;
                int spellLockBeforeRage = urban.Descriptor.State
                    .SpellCastingForbidden.Count;
                rage = urban.Descriptor.Buffs.AddBuff(
                    set.NativeRageBuff, rageContext, null);
                bool spellAvailableDuringRage = spellAvailability.IsAvailable;
                int spellLockDuringRage = urban.Descriptor.State
                    .SpellCastingForbidden.Count;
                RuleSkillCheck trickeryDuringRage = Rulebook.Trigger(
                    new RuleSkillCheck(urban, StatType.SkillThievery, 0));
                int attackDuringRage = Attack(urban, weapon);
                int damageBonusDuringRage = ElvenBranchedSpearCombatScenario
                    .WeaponStats(urban, weapon).BonusDamage;
                int willDuringRage = urban.Descriptor.Stats.SaveWill.ModifiedValue;
                int acDuringRage = ArmorClass(urban, enemyOne);
                int temporaryHpDuringRage = urban.Descriptor.Stats
                    .TemporaryHitPoints.ModifiedValue;
                string[] liveRageComponents = (set.RageBuff.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).Select(value =>
                        value == null ? "<null>" : value.GetType().FullName)
                    .ToArray();
                urban.Descriptor.RemoveFact(set.RageBuff);
                RemoveIntroducedBuffs(urban.Descriptor, beforeLeakageRage);
                bool spellAvailableAfterRage = spellAvailability.IsAvailable;
                int spellLockAfterRage = urban.Descriptor.State
                    .SpellCastingForbidden.Count;
                bool spellRestrictionRetained = liveRageComponents.Contains(
                    "Kingmaker.UnitLogic.FactLogic.ForbidSpellCasting");
                bool ordinaryBenefitTypesAbsent = !liveRageComponents.Any(
                    value => value.Contains("TemporaryHitPointsPerLevel") ||
                        value.Contains("AttackTypeAttackBonus") ||
                        value.Contains("WeaponAttackTypeDamageBonus") ||
                        value.Contains("WeaponGroupDamageBonus") ||
                        value.Contains("AddContextStatBonus"));
                Add(assertions, "urban-ordinary-allocation-and-leakage",
                    "full DEX +4 and CON +4 work; CON allocation leaks no attack, damage, temporary HP, Will, or AC; spellcasting restriction remains",
                    ordinaryDexterityDetail + ";" +
                        ordinaryConstitutionDetail + ";attack=" +
                        attackBeforeRage + "->" + attackDuringRage +
                        ";damage=" + damageBonusBeforeRage + "->" +
                        damageBonusDuringRage + ";will=" + willBeforeRage +
                        "->" + willDuringRage + ";ac=" + acBeforeRage +
                        "->" + acDuringRage + ";temporaryHp=" +
                        temporaryHpBeforeRage + "->" +
                        temporaryHpDuringRage + ";components=" +
                        string.Join(",", liveRageComponents) +
                        ";spellAvailable=" + spellAvailableBeforeRage + "/" +
                        spellAvailableDuringRage + "/" +
                        spellAvailableAfterRage + ";spellLock=" +
                        spellLockBeforeRage + "/" + spellLockDuringRage +
                        "/" + spellLockAfterRage + ";trickery=" +
                        trickeryDuringRage.BaseRollResult,
                    ordinaryDexterity && ordinaryConstitution &&
                        attackDuringRage == attackBeforeRage &&
                        damageBonusDuringRage == damageBonusBeforeRage &&
                        willDuringRage == willBeforeRage &&
                        acDuringRage == acBeforeRage &&
                        temporaryHpDuringRage == temporaryHpBeforeRage &&
                        spellRestrictionRetained && ordinaryBenefitTypesAbsent &&
                        spellAvailableBeforeRage &&
                        !spellAvailableDuringRage && spellAvailableAfterRage &&
                        spellLockBeforeRage == 0 && spellLockDuringRage == 1 &&
                        spellLockAfterRage == 0 &&
                        trickeryDuringRage.BaseRollResult >= 1 &&
                        trickeryDuringRage.BaseRollResult <= 20,
                    "live score, attack, weapon-damage, save, AC, temporary-HP, exact AbilityData spell availability, Dexterity-based RuleSkillCheck, and finalized buff-component observations");
                Add(assertions,
                    "urban-controlled-trickery-and-spell-restriction",
                    "Controlled Rage permits a Dexterity-based Trickery rule while exact native spell availability is prohibited only for the Rage duration",
                    "trickeryD20=" + trickeryDuringRage.BaseRollResult +
                        ";spellAvailable=" + spellAvailableBeforeRage + "/" +
                        spellAvailableDuringRage + "/" +
                        spellAvailableAfterRage + ";spellLock=" +
                        spellLockBeforeRage + "/" + spellLockDuringRage +
                        "/" + spellLockAfterRage,
                    trickeryDuringRage.BaseRollResult >= 1 &&
                        trickeryDuringRage.BaseRollResult <= 20 &&
                        spellAvailableBeforeRage &&
                        !spellAvailableDuringRage && spellAvailableAfterRage &&
                        spellLockBeforeRage == 0 && spellLockDuringRage == 1 &&
                        spellLockAfterRage == 0,
                    "RuleSkillCheck(StatType.SkillThievery) plus AbilityData.IsAvailable for an exact AbilityType.Spell probe and UnitState.SpellCastingForbidden");

                stage = "ordinary-repeated-constitution";
                SelectDirect(urban.Descriptor, set,
                    ControlledRageTier.Ordinary, 0, 0, 4);
                int repeatedBaseMax = urban.MaxHP;
                urban.Descriptor.Damage = repeatedBaseMax - 1;
                bool repeatedConstitutionValid = true;
                var repeatedConstitutionDetail = new List<string>();
                for (int cycle = 1; cycle <= 3; cycle++)
                {
                    int cycleDamage = urban.Descriptor.Damage;
                    int cycleHp = urban.HPLeft;
                    Buff[] beforeCycle = urban.Descriptor.Buffs.RawFacts
                        .OfType<Buff>().ToArray();
                    rage = urban.Descriptor.Buffs.AddBuff(
                        set.NativeRageBuff, rageContext, null);
                    int ragingHp = urban.HPLeft;
                    int ragingMax = urban.MaxHP;
                    urban.Descriptor.RemoveFact(set.RageBuff);
                    int endingHp = urban.HPLeft;
                    int endingMax = urban.MaxHP;
                    repeatedConstitutionValid &= ragingMax == repeatedBaseMax + 2 &&
                        ragingHp == cycleHp + 2 && endingHp == cycleHp &&
                        endingMax == repeatedBaseMax &&
                        urban.Descriptor.Damage == cycleDamage;
                    repeatedConstitutionDetail.Add(cycle + ":" + cycleHp +
                        "/" + ragingHp + "/" + endingHp + ";max=" +
                        repeatedBaseMax + "/" + ragingMax + "/" + endingMax +
                        ";damage=" + urban.Descriptor.Damage);
                    RemoveIntroducedBuffs(urban.Descriptor, beforeCycle);
                }
                Add(assertions, "urban-repeated-low-hp-constitution",
                    "three low-HP CON +4 entry/exit cycles preserve exact damage deficit without healing or duplication",
                    string.Join("|", repeatedConstitutionDetail),
                    repeatedConstitutionValid && urban.HPLeft == 1 &&
                        urban.MaxHP == repeatedBaseMax,
                    "three live genuine-Constitution modifier cycles at one HP");
                urban.Descriptor.Damage = damageBefore;

                stage = "ordinary-native-rage-toggle";
                BlueprintActivatableAbility rageBlueprint =
                    BlueprintBootstrap.Library.GetAllBlueprints()
                    .OfType<BlueprintActivatableAbility>().Single(value =>
                        value.AssetGuid == NativeRageActivatableGuid);
                BlueprintAbilityResource rageResource = BlueprintBootstrap.Library
                    .GetAllBlueprints().OfType<BlueprintAbilityResource>().Single(
                        value => value.AssetGuid == NativeRageResourceGuid);
                urban.Descriptor.Resources.Add(rageResource, true);
                int ordinaryResourceBefore = urban.Descriptor.Resources
                    .GetResourceAmount(rageResource);
                ActivatableAbility ordinaryToggle = urban.Descriptor
                    .ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                        value != null && ReferenceEquals(value.Blueprint,
                            rageBlueprint));
                bool ordinaryActivated = false, ordinaryCanceled = false,
                    ordinaryFatigued = false;
                bool ordinaryFatiguedBefore = urban.Descriptor.State.HasCondition(
                    UnitCondition.Fatigued);
                int ordinaryResourceRunning = ordinaryResourceBefore;
                int ordinaryResourceAfterRound = ordinaryResourceBefore;
                if (ordinaryToggle != null)
                {
                    Buff[] beforeToggle = urban.Descriptor.Buffs.RawFacts
                        .OfType<Buff>().ToArray();
                    ordinaryToggle.IsOn = true;
                    ordinaryToggle.TryStart();
                    ordinaryActivated = ordinaryToggle.IsOn &&
                        urban.Descriptor.HasFact(set.RageBuff);
                    ordinaryResourceRunning = urban.Descriptor.Resources
                        .GetResourceAmount(rageResource);
                    ordinaryToggle.OnNewRound();
                    ordinaryResourceAfterRound = urban.Descriptor.Resources
                        .GetResourceAmount(rageResource);
                    ordinaryToggle.IsOn = false;
                    ordinaryToggle.Stop(true);
                    ordinaryCanceled = !ordinaryToggle.IsOn &&
                        !urban.Descriptor.HasFact(set.RageBuff);
                    ordinaryFatigued = urban.Descriptor.State.HasCondition(
                        UnitCondition.Fatigued);
                    foreach (Buff introduced in urban.Descriptor.Buffs.RawFacts
                        .OfType<Buff>().Where(value => !beforeToggle.Contains(
                            value)).ToArray())
                        introduced.Remove();
                }
                Add(assertions, "urban-native-rage-lifecycle",
                    "native Rage toggle activates Urban buff, cancels, and applies ordinary fatigue",
                    "toggle=" + (ordinaryToggle != null) + ";activated=" +
                        ordinaryActivated + ";canceled=" + ordinaryCanceled +
                        ";fatigued=" + ordinaryFatiguedBefore + "->" +
                        ordinaryFatigued + ";resource=" +
                        ordinaryResourceBefore + "->" + ordinaryResourceRunning +
                        "->" + ordinaryResourceAfterRound,
                    ordinaryToggle != null && ordinaryActivated &&
                        ordinaryCanceled && !ordinaryFatiguedBefore &&
                        ordinaryFatigued &&
                        ordinaryResourceBefore > 0 &&
                        ordinaryResourceRunning <= ordinaryResourceBefore &&
                        ordinaryResourceAfterRound == ordinaryResourceBefore - 1,
                    "native Rage activatable and retained AddFactContextActions lifecycle");

                stage = "tier-transitions";
                ApplyLevel(urban.Descriptor, set.BarbarianClass, null, false);
                AbilityData[] levelTwo = LivePanelVariants(urban,
                    new AbilityData(set.OrdinarySelector, urban.Descriptor));
                bool levelTwoSelectorFacts =
                    urban.Descriptor.HasFact(set.OrdinarySelector) &&
                    !urban.Descriptor.HasFact(set.LegacySelector) &&
                    !urban.Descriptor.HasFact(set.GreaterSelector) &&
                    !urban.Descriptor.HasFact(set.MightySelector);
                Add(assertions, "urban-level-two-live-selector-boundary",
                    "a level-2 player's actual variant-grid enumeration can expose exactly six ordinary allocations and no future-tier parent",
                    "level=" + urban.Descriptor.Progression.GetClassLevel(
                        set.BarbarianClass) + ";variants=" + levelTwo.Length +
                        ";ordinary=" + urban.Descriptor.HasFact(
                            set.OrdinarySelector) + ";legacy=" +
                        urban.Descriptor.HasFact(set.LegacySelector) +
                        ";greater=" + urban.Descriptor.HasFact(
                            set.GreaterSelector) + ";mighty=" +
                        urban.Descriptor.HasFact(set.MightySelector),
                    levelTwo.Length == 6 && levelTwoSelectorFacts,
                    "MechanicActionBarSlotAbility.GetConvertedAbilityData and owner fact inventory");
                for (int level = 3; level <= 11; level++)
                    ApplyLevel(urban.Descriptor, set.BarbarianClass, null, false);
                AbilityData[] greater = LivePanelVariants(urban,
                    new AbilityData(set.GreaterSelector, urban.Descriptor));
                bool greaterSelectorFacts =
                    urban.Descriptor.HasFact(set.GreaterSelector) &&
                    !urban.Descriptor.HasFact(set.LegacySelector) &&
                    !urban.Descriptor.HasFact(set.OrdinarySelector) &&
                    !urban.Descriptor.HasFact(set.MightySelector);
                bool greaterDefault = Selected(set, urban.Descriptor,
                    ControlledRageTier.Greater, 6, 0, 0);
                bool greaterFull = MeasureAllocation(urban.Descriptor, set,
                    rageContext, ControlledRageTier.Greater, 6, 0, 0,
                    out string greaterFullDetail);
                bool greaterSplit = MeasureAllocation(urban.Descriptor, set,
                    rageContext, ControlledRageTier.Greater, 4, 2, 0,
                    out string greaterSplitDetail);
                bool greaterThreeWay = MeasureAllocation(urban.Descriptor, set,
                    rageContext, ControlledRageTier.Greater, 2, 2, 2,
                    out string greaterThreeWayDetail);
                for (int level = 12; level <= 20; level++)
                    ApplyLevel(urban.Descriptor, set.BarbarianClass, null, false);
                AbilityData[] mighty = LivePanelVariants(urban,
                    new AbilityData(set.MightySelector, urban.Descriptor));
                bool mightySelectorFacts =
                    urban.Descriptor.HasFact(set.MightySelector) &&
                    !urban.Descriptor.HasFact(set.LegacySelector) &&
                    !urban.Descriptor.HasFact(set.OrdinarySelector) &&
                    !urban.Descriptor.HasFact(set.GreaterSelector);
                bool mightyDefault = Selected(set, urban.Descriptor,
                    ControlledRageTier.Mighty, 8, 0, 0);
                bool mightyFull = MeasureAllocation(urban.Descriptor, set,
                    rageContext, ControlledRageTier.Mighty, 8, 0, 0,
                    out string mightyFullDetail);
                bool mightySixTwo = MeasureAllocation(urban.Descriptor, set,
                    rageContext, ControlledRageTier.Mighty, 6, 2, 0,
                    out string mightySixTwoDetail);
                bool mightyFourFour = MeasureAllocation(urban.Descriptor, set,
                    rageContext, ControlledRageTier.Mighty, 4, 4, 0,
                    out string mightyFourFourDetail);
                bool mightyThreeWay = MeasureAllocation(urban.Descriptor, set,
                    rageContext, ControlledRageTier.Mighty, 4, 2, 2,
                    out string mightyThreeWayDetail);
                Add(assertions, "urban-greater-mighty-tiers",
                    "level 11 exposes ten +6 options and level 20 fifteen +8 options; full and every allocation family execute exactly with independent STR defaults",
                    "level=" + urban.Descriptor.Progression.GetClassLevel(
                        set.BarbarianClass) + ";greater=" + greater.Length +
                        "/default:" + greaterDefault + "/" +
                        greaterFullDetail + "/" + greaterSplitDetail + "/" +
                        greaterThreeWayDetail +
                        ";mighty=" + mighty.Length + "/default:" +
                        mightyDefault + "/" + mightyFullDetail + "/" +
                        mightySixTwoDetail + "/" + mightyFourFourDetail +
                        "/" + mightyThreeWayDetail,
                    greater.Length == 10 && greaterSelectorFacts &&
                        greaterDefault && greaterFull &&
                        greaterSplit && greaterThreeWay && mighty.Length == 15 &&
                        mightySelectorFacts && mightyDefault && mightyFull && mightySixTwo &&
                        mightyFourFour && mightyThreeWay &&
                        set.SelectionFacts.Count(urban.Descriptor.HasFact) == 3 &&
                        urban.Descriptor.Get<
                            UnitPartControlledRageSelection>() != null,
                    "actual Barbarian progression facts, exact live player-panel variants, and live score modifiers");

                stage = "native-rage-toggle";
                ActivatableAbility nativeToggle = urban.Descriptor
                    .ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                        value != null && ReferenceEquals(value.Blueprint,
                            rageBlueprint));
                bool nativeFeature = urban.Descriptor.HasFact(
                    BlueprintBootstrap.Library.GetAllBlueprints()
                        .OfType<BlueprintFeature>().Single(value =>
                            value.AssetGuid == NativeRageFeatureGuid));
                urban.Descriptor.Resources.Add(rageResource, true);
                bool activated = false, canceled = false;
                bool fatigueBeforeTireless = urban.Descriptor.State.HasCondition(
                    UnitCondition.Fatigued);
                bool fatigueAfterTireless = true;
                if (nativeToggle != null)
                {
                    nativeToggle.IsOn = true;
                    nativeToggle.TryStart();
                    activated = nativeToggle.IsOn &&
                        urban.Descriptor.HasFact(set.RageBuff);
                    nativeToggle.IsOn = false;
                    nativeToggle.Stop(true);
                    canceled = !nativeToggle.IsOn &&
                        !urban.Descriptor.HasFact(set.RageBuff);
                    fatigueAfterTireless = urban.Descriptor.State.HasCondition(
                        UnitCondition.Fatigued);
                }
                Add(assertions, "urban-tireless-rage-lifecycle",
                    "level-20 native Rage activates/cancels Urban buff without fatigue",
                    "feature=" + nativeFeature + ";toggle=" +
                        (nativeToggle != null) + ";activated=" + activated +
                        ";canceled=" + canceled + ";fatigue=" +
                        fatigueBeforeTireless + "->" + fatigueAfterTireless,
                    nativeFeature && urban.Descriptor.HasFact(
                        set.NativeTirelessRage) && nativeToggle != null &&
                        activated && canceled &&
                        fatigueAfterTireless == fatigueBeforeTireless,
                    "native Rage activatable and retained AddFactContextActions lifecycle");

                stage = "crowd-control";
                if (weapon == null)
                    weapon = ElvenBranchedSpearCombatScenario.Equip(urban,
                        BlueprintBootstrap.ElvenBranchedSpears.Entries[0].Item);
                int attackZero = Attack(urban, weapon);
                int acZero = ArmorClass(urban, enemyOne);
                float reachOnlyEdgeDistance = 1.8f;
                float reachOnlyCenterDistance = reachOnlyEdgeDistance +
                    urban.Corpulence + enemyOne.Corpulence;
                SetPosition(enemyOne, new Vector3(reachOnlyCenterDistance,
                    0f, 0f));
                SetPosition(enemyTwo, new Vector3(-reachOnlyCenterDistance,
                    0f, 0f));
                float observedReachEdgeDistance = CrowdControlComponent
                    .EdgeDistance(urban, enemyOne);
                int adjacentWithinReach = CrowdControlComponent
                    .CountAdjacentActiveEnemies(urban);
                int attackWithinReach = Attack(urban, weapon);
                SetPosition(enemyTwo, new Vector3(-9f, 0f, 0f));
                Size originalSize = enemyOne.Descriptor.State.Size;
                enemyOne.Descriptor.State.Size = Size.Large;
                float largeCenterDistance = 1.514f + urban.Corpulence +
                    enemyOne.Corpulence;
                SetPosition(enemyOne, new Vector3(largeCenterDistance, 0f, 0f));
                float largeNativeDistance = urban.DistanceTo(enemyOne);
                float largeEdgeDistance = CrowdControlComponent.EdgeDistance(
                    urban, enemyOne);
                int adjacentLarge = CrowdControlComponent
                    .CountAdjacentActiveEnemies(urban);
                enemyOne.Descriptor.State.Size = originalSize;
                SetPosition(enemyOne, new Vector3(1.5f, 0f, 0f));
                int attackOne = Attack(urban, weapon);
                int acOne = ArmorClass(urban, enemyOne);
                SetPosition(enemyTwo, new Vector3(-1.5f, 0f, 0f));
                float distanceOne = urban.DistanceTo(enemyOne);
                float distanceTwo = urban.DistanceTo(enemyTwo);
                int adjacentTwo = CrowdControlComponent
                    .CountAdjacentActiveEnemies(urban);
                int attackTwo = Attack(urban, weapon);
                int acTwo = ArmorClass(urban, enemyOne);
                enemyWeapon = ElvenBranchedSpearCombatScenario.Equip(enemyOne,
                    BlueprintBootstrap.ElvenBranchedSpears.Entries[0].Item);
                RuleAttackWithWeapon playerIssuedTwo = PlayerIssuedAttack(
                    urban, enemyOne);
                RuleAttackWithWeapon incomingTwo = PlayerIssuedAttack(enemyOne,
                    urban);
                string twoCandidates = CrowdControlComponent.DescribeCandidate(
                    urban, enemyOne) + "|" +
                    CrowdControlComponent.DescribeCandidate(urban, enemyTwo);
                string twoAttackObservation =
                    CrowdControlComponent.LastAttackObservation;
                string twoAcObservation =
                    CrowdControlComponent.LastArmorClassObservation;
                SetPosition(enemyTwo, new Vector3(-9f, 0f, 0f));
                RuleAttackWithWeapon playerIssuedOne = PlayerIssuedAttack(
                    urban, enemyOne);
                RuleAttackWithWeapon incomingOne = PlayerIssuedAttack(enemyOne,
                    urban);
                string oneCandidates = CrowdControlComponent.DescribeCandidate(
                    urban, enemyOne) + "|" +
                    CrowdControlComponent.DescribeCandidate(urban, enemyTwo);
                string oneAttackObservation =
                    CrowdControlComponent.LastAttackObservation;
                string oneAcObservation =
                    CrowdControlComponent.LastArmorClassObservation;
                bool actualAttackSource = HasBonusSource(playerIssuedTwo
                    .AttackRoll.AttackBonusRule.BonusSources,
                    set.CrowdControl, 1) && !HasBonusSource(playerIssuedOne
                        .AttackRoll.AttackBonusRule.BonusSources,
                        set.CrowdControl, 1);
                bool actualAcSource = HasBonusSource(incomingTwo.AttackRoll
                    .ACRule.BonusSources, set.CrowdControl, 1) &&
                    !HasBonusSource(incomingOne.AttackRoll.ACRule.BonusSources,
                        set.CrowdControl, 1);
                bool actualCombatLog = playerIssuedTwo.AttackRoll
                    .AddedToCombatLog && playerIssuedTwo.AttackRoll
                    .AttackLogEntry != null && incomingTwo.AttackRoll
                    .AddedToCombatLog && incomingTwo.AttackRoll.AttackLogEntry !=
                    null;
                Add(assertions, "urban-crowd-control-player-attack-pipeline",
                    "a native player-issued UnitAttack supplies Crowd Control +1 to the outgoing attack and +1 dodge AC to the incoming attack only at the two-enemy threshold, with exact combat-log sources",
                    "outgoing=" + playerIssuedOne.AttackRoll.AttackBonus +
                        "->" + playerIssuedTwo.AttackRoll.AttackBonus +
                        ";incomingAc=" + incomingOne.AttackRoll.TargetAC +
                        "->" + incomingTwo.AttackRoll.TargetAC +
                        ";attackRules=" + playerIssuedTwo.AttackRoll
                            .AttackBonusRule.GetType().FullName + "/" +
                        typeof(RuleCalculateAttackBonusWithoutTarget).FullName +
                        ";acRule=" + incomingTwo.AttackRoll.ACRule.GetType()
                            .FullName + ";attackSource=" + actualAttackSource +
                        ";acSource=" + actualAcSource + ";combatLog=" +
                        actualCombatLog + ";twoAttack={" +
                        twoAttackObservation + "};twoAc={" + twoAcObservation +
                        "};oneAttack={" + oneAttackObservation +
                        "};oneAc={" + oneAcObservation + "};twoCandidates={" +
                        twoCandidates + "};oneCandidates={" + oneCandidates +
                        "}",
                    playerIssuedTwo.AttackRoll.AttackBonus ==
                        playerIssuedOne.AttackRoll.AttackBonus + 1 &&
                        incomingTwo.AttackRoll.TargetAC ==
                            incomingOne.AttackRoll.TargetAC + 1 &&
                        actualAttackSource && actualAcSource && actualCombatLog &&
                        twoAttackObservation.Contains("adjacent=2;applies=True;value=1;descriptor=Untyped") &&
                        twoAcObservation.Contains("adjacent=2;applies=True;value=1;descriptor=Dodge") &&
                        oneAttackObservation.Contains("adjacent=1;applies=False;value=0;descriptor=Untyped") &&
                        oneAcObservation.Contains("adjacent=1;applies=False;value=0;descriptor=Dodge"),
                    "UnitAttack.CreateAttackCommand -> UnitAttack.TriggerAttackRule -> RuleAttackWithWeapon -> RuleAttackRoll outer/inner attack and AC rules");
                SetPosition(enemyTwo, new Vector3(-1.5f, 0f, 0f));
                var rangedBlueprint = BlueprintLibraryLookup.RequireExact<
                    Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>(
                        BlueprintBootstrap.Library,
                        "19a5092244dcf99478dcd73c974828b1",
                        "native Standard Heavy Crossbow");
                rangedWeapon = new ItemEntityWeapon(rangedBlueprint);
                int rangedAttackTwo = Attack(urban, rangedWeapon);
                SetPosition(enemyTwo, new Vector3(-9f, 0f, 0f));
                int rangedAttackOne = Attack(urban, rangedWeapon);
                SetPosition(enemyTwo, new Vector3(-1.5f, 0f, 0f));
                object hostileGroup = enemyTwo.Group;
                string hostileGroupId = (string)Read(enemyTwo, "m_GroupId");
                FieldInfo groupField = typeof(UnitEntityData).GetField(
                    "m_Group", Members);
                FieldInfo groupIdField = typeof(UnitEntityData).GetField(
                    "m_GroupId", Members);
                groupIdField.SetValue(enemyTwo, urban.Group.Id);
                groupField.SetValue(enemyTwo, urban.Group);
                bool isEnemyWhileFriendly = urban.IsEnemy(enemyTwo);
                int adjacentAfterFriendly = CrowdControlComponent
                    .CountAdjacentActiveEnemies(urban);
                int attackAfterFriendly = Attack(urban, weapon);
                int acAfterFriendly = ArmorClass(urban, enemyOne);
                groupIdField.SetValue(enemyTwo, hostileGroupId);
                groupField.SetValue(enemyTwo, hostileGroup);
                bool isEnemyAfterRestore = urban.IsEnemy(enemyTwo);
                int adjacentAfterHostile = CrowdControlComponent
                    .CountAdjacentActiveEnemies(urban);
                enemyTwo.Descriptor.State.LifeState =
                    UnitLifeState.Unconscious;
                bool isUnconscious = enemyTwo.Descriptor.State.IsUnconscious;
                int adjacentAfterUnconscious = CrowdControlComponent
                    .CountAdjacentActiveEnemies(urban);
                int attackAfterUnconscious = Attack(urban, weapon);
                int acAfterUnconscious = ArmorClass(urban, enemyOne);
                enemyTwo.Descriptor.State.LifeState = UnitLifeState.Dead;
                int adjacentAfterDead = CrowdControlComponent
                    .CountAdjacentActiveEnemies(urban);
                int attackAfterDead = Attack(urban, weapon);
                int acAfterDead = ArmorClass(urban, enemyOne);
                SetPosition(enemyThree, new Vector3(0f, 0f, 1.5f));
                int attackThree = Attack(urban, weapon);
                int acThree = ArmorClass(urban, enemyOne);
                enemyThree.Destroyed = true;
                bool isDestroyed = enemyThree.Destroyed;
                int adjacentAfterDestroyed = CrowdControlComponent
                    .CountAdjacentActiveEnemies(urban);
                int attackAfterDestroyed = Attack(urban, weapon);
                int acAfterDestroyed = ArmorClass(urban, enemyOne);
                enemyThree.Destroyed = false;
                SetPosition(enemyTwo, new Vector3(-9f, 0f, 0f));
                SetPosition(enemyOne, new Vector3(9f, 0f, 0f));
                int attackMovedOut = Attack(urban, weapon);
                Add(assertions, "urban-crowd-control-rule-events",
                    "zero/one grant none; two/three grant exactly +1 attack and +1 dodge AC; hostility, unconsciousness, death, destruction, and movement update immediately",
                    "attack=" + attackZero + "/" + attackOne + "/" +
                        attackTwo + "/" + attackThree + "/" + attackMovedOut +
                        ";ac=" + acZero + "/" + acOne + "/" + acTwo + "/" +
                        acThree + ";distance=" + distanceOne + "/" +
                        distanceTwo + ";reach=" + reachOnlyCenterDistance +
                        "/" + observedReachEdgeDistance + "/" +
                        adjacentWithinReach + "/" + attackWithinReach +
                        ";large=" + largeCenterDistance +
                        "/" + largeNativeDistance + "/" + largeEdgeDistance +
                        "/" + adjacentLarge +
                        ";ranged=" + rangedAttackOne + "/" + rangedAttackTwo +
                        ";adjacent=" + adjacentTwo +
                        ";friendly=" + adjacentAfterFriendly + "/" +
                        attackAfterFriendly + "/" + acAfterFriendly +
                        "/enemy:" + isEnemyWhileFriendly +
                        ";hostile=" + adjacentAfterHostile + "/enemy:" +
                        isEnemyAfterRestore +
                        ";unconscious=" + adjacentAfterUnconscious + "/" +
                        attackAfterUnconscious + "/" +
                        acAfterUnconscious + "/state:" +
                        UnitLifeState.Unconscious + "/" + isUnconscious +
                        ";dead=" + adjacentAfterDead + "/" +
                        attackAfterDead + "/" + acAfterDead + "/state:" +
                        enemyTwo.Descriptor.State.LifeState +
                        ";destroyed=" +
                        adjacentAfterDestroyed + "/" +
                        attackAfterDestroyed + "/" + acAfterDestroyed +
                        "/state:" + isDestroyed +
                        ";states=" + EnemyState(urban, enemyOne) + "/" +
                        EnemyState(urban, enemyTwo),
                    attackOne == attackZero && attackTwo == attackZero + 1 &&
                        attackThree == attackZero + 1 &&
                        attackMovedOut == attackZero && acOne == acZero &&
                        acTwo == acZero + 1 && acThree == acZero + 1 &&
                        observedReachEdgeDistance > 1.52400031f &&
                        adjacentWithinReach == 0 &&
                        attackWithinReach == attackZero &&
                        largeCenterDistance > 1.52400031f &&
                        largeEdgeDistance <= 1.52400031f &&
                        adjacentLarge == 1 &&
                        rangedAttackTwo == rangedAttackOne + 1 &&
                        adjacentAfterFriendly < 2 &&
                        !isEnemyWhileFriendly && isEnemyAfterRestore &&
                        attackAfterFriendly == attackZero &&
                        acAfterFriendly == acZero &&
                        adjacentAfterHostile == 2 &&
                        adjacentTwo == 2 && adjacentAfterUnconscious == 1 &&
                        isUnconscious && !enemyTwo.Descriptor.State.IsConscious &&
                        attackAfterUnconscious == attackZero &&
                        acAfterUnconscious == acZero &&
                        adjacentAfterDead == 1 &&
                        enemyTwo.Descriptor.State.IsDead &&
                        attackAfterDead == attackZero &&
                        acAfterDead == acZero && isDestroyed &&
                        adjacentAfterDestroyed == 1 &&
                        attackAfterDestroyed == attackZero &&
                        acAfterDestroyed == acZero,
                    "live attack/AC Rulebook events and native edge-to-edge DistanceTo");
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" + exception);
            }
            finally
            {
                if (rangedWeapon != null) rangedWeapon.Dispose();
                if (spellProbe != null)
                    UnityEngine.Object.DestroyImmediate(spellProbe);
                ElvenBranchedSpearCombatScenario.RemoveEquipped(enemyOne,
                    ref enemyWeapon);
                ElvenBranchedSpearCombatScenario.RemoveEquipped(urban, ref weapon);
                if (enemyThreeRegistered)
                    Game.Instance.State.Units.All.Remove(enemyThree);
                if (enemyTwoRegistered)
                    Game.Instance.State.Units.All.Remove(enemyTwo);
                if (enemyOneRegistered)
                    Game.Instance.State.Units.All.Remove(enemyOne);
                if (urbanRegistered)
                    Game.Instance.State.Units.All.Remove(urban);
                foreach (UnitEntityData unit in new[] { enemyThree, enemyTwo,
                    enemyOne })
                {
                    if (unit == null) continue;
                    if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
                    unit.Descriptor.State.Immortality.ReleaseAll();
                    unit.Dispose();
                }
                if (urban != null)
                {
                    if (urban.CombatState.IsInCombat)
                        urban.CombatState.LeaveCombat();
                    urban.Dispose();
                }
                if (scene != null) scene.Dispose();
                if (hostileSource != null)
                    UnityEngine.Object.DestroyImmediate(hostileSource);
                cleaned = Same(unitsBefore, Snapshot(allUnits));
            }
            Add(assertions, "urban-focused-cleanup",
                "global-unit snapshot restored after request-local fixture",
                "cleaned=" + cleaned, cleaned,
                "disposable SceneEntitiesState, units, facts, and weapon");
            Add(assertions, "loaded-mod-version", request.ExpectedModVersion,
                context.ModEntry.Info.Version,
                request.ExpectedModVersion == context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version");
            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            bool pass = diagnostics.Count == 0 && assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            return new RuntimeTestResult
            {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = identity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary =
                    diagnostics.Count == 0 ? string.Empty : diagnostics[0],
                EvidenceFiles = new List<string>(),
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void ApplyLevel(UnitDescriptor unit,
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype,
            bool first)
        {
            Type type = typeof(LevelUpController);
            MethodInfo start = type.GetMethods(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                    value.Name == "StartWithoutAssigningStaticInstance" &&
                    value.GetParameters().Length == 5);
            object mode = Enum.Parse(start.GetParameters()[4].ParameterType,
                first ? "CharGen" : "LevelUp", false);
            object controller = start.Invoke(null,
                new object[] { unit, false, null, null, mode });
            try
            {
                MethodInfo select = type.GetMethod("SelectClass", Members,
                    null, new[] { typeof(BlueprintCharacterClass), typeof(bool) },
                    null);
                if (!(bool)select.Invoke(controller,
                    new object[] { characterClass, false }))
                    throw new InvalidOperationException(
                        "Barbarian class selection was rejected.");
                if (archetype != null)
                {
                    MethodInfo add = type.GetMethod("AddArchetype", Members,
                        null, new[] { typeof(BlueprintArchetype) }, null);
                    if (add == null || !(bool)add.Invoke(controller,
                        new object[] { archetype }))
                        throw new InvalidOperationException(
                            "Urban archetype selection was rejected.");
                }
                type.GetMethod("ApplyClassMechanics", Members).Invoke(
                    controller, null);
                type.GetMethod("ApplyLevelup", Members).Invoke(controller,
                    new object[] { unit });
            }
            finally
            {
                type.GetMethod("Cancel", Members).Invoke(controller, null);
            }
        }

        private static void SelectDirect(UnitDescriptor owner,
            UrbanBarbarianBlueprintSet set, ControlledRageTier tier,
            int strength, int dexterity, int constitution)
        {
            ControlledRageAllocation allocation =
                ControlledRageAllocationPolicy.Generate(tier).Single(value =>
                    value.Strength == strength && value.Dexterity == dexterity &&
                    value.Constitution == constitution);
            if (!ControlledRageRuntime.TrySelect(owner, allocation))
                throw new InvalidOperationException(
                    "Controlled Rage persisted selection was rejected.");
        }

        private static bool Selected(UrbanBarbarianBlueprintSet set,
            UnitDescriptor owner, ControlledRageTier tier, int strength,
            int dexterity, int constitution)
        {
            ControlledRageAllocation allocation =
                ControlledRageAllocationPolicy.Generate(tier).Single(value =>
                    value.Strength == strength && value.Dexterity == dexterity &&
                    value.Constitution == constitution);
            return Equals(ControlledRageRuntime.ResolveSelection(owner, false),
                allocation);
        }

        private static int AbilityDeltaTotal(UnitDescriptor owner,
            int strength, int dexterity, int constitution)
        {
            return owner.Stats.Strength.ModifiedValue - strength +
                owner.Stats.Dexterity.ModifiedValue - dexterity +
                owner.Stats.Constitution.ModifiedValue - constitution;
        }

        private static bool MeasureAllocation(UnitDescriptor owner,
            UrbanBarbarianBlueprintSet set, MechanicsContext rageContext,
            ControlledRageTier tier, int strength, int dexterity,
            int constitution, out string detail)
        {
            SelectDirect(owner, set, tier, strength, dexterity, constitution);
            int strengthBefore = owner.Stats.Strength.ModifiedValue;
            int dexterityBefore = owner.Stats.Dexterity.ModifiedValue;
            int constitutionBefore = owner.Stats.Constitution.ModifiedValue;
            Buff[] before = owner.Buffs.RawFacts.OfType<Buff>().ToArray();
            Buff rage = owner.Buffs.AddBuff(set.NativeRageBuff,
                rageContext, null);
            int strengthDelta = owner.Stats.Strength.ModifiedValue -
                strengthBefore;
            int dexterityDelta = owner.Stats.Dexterity.ModifiedValue -
                dexterityBefore;
            int constitutionDelta = owner.Stats.Constitution.ModifiedValue -
                constitutionBefore;
            bool substituted = rage != null &&
                ReferenceEquals(rage.Blueprint, set.RageBuff) &&
                !owner.HasFact(set.NativeRageBuff);
            owner.RemoveFact(set.RageBuff);
            RemoveIntroducedBuffs(owner, before);
            detail = tier + ":" + strength + "/" + dexterity + "/" +
                constitution + "->" + strengthDelta + "/" + dexterityDelta +
                "/" + constitutionDelta + ";substituted=" + substituted;
            return substituted && strengthDelta == strength &&
                dexterityDelta == dexterity &&
                constitutionDelta == constitution &&
                AbilityDeltaTotal(owner, strengthBefore, dexterityBefore,
                    constitutionBefore) == 0;
        }

        private static int Attack(UnitEntityData unit, ItemEntityWeapon weapon)
        {
            return Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(
                unit, weapon, 0)).Result;
        }

        private static RuleAttackWithWeapon PlayerIssuedAttack(
            UnitEntityData attacker, UnitEntityData target)
        {
            if (attacker == null || target == null)
                throw new ArgumentNullException("player-issued attack");
            attacker.Commands.InterruptAll(true);
            UnitCommand issued = UnitAttack.CreateAttackCommand(attacker,
                target);
            UnitAttack command = issued as UnitAttack;
            if (command == null) throw new InvalidOperationException(
                "Native UnitAttack.CreateAttackCommand did not produce an adjacent UnitAttack command: " +
                    (issued == null ? "<null>" : issued.GetType().FullName));
            attacker.Commands.Run(command);
            FieldInfo attacksField = typeof(UnitAttack).GetField("m_AllAttacks",
                Members);
            FieldInfo indexField = typeof(UnitAttack).GetField("m_AttackIndex",
                Members);
            MethodInfo create = typeof(UnitAttack).GetMethod(
                "CreateSingleAttack", Members, null, Type.EmptyTypes, null);
            MethodInfo trigger = typeof(UnitAttack).GetMethod(
                "TriggerAttackRule", Members, null,
                new[] { typeof(AttackHandInfo) }, null);
            if (attacksField == null || indexField == null || create == null ||
                trigger == null) throw new MissingMethodException(
                    typeof(UnitAttack).FullName,
                    "CreateSingleAttack/TriggerAttackRule contract");
            var attacks = attacksField.GetValue(command) as
                List<AttackHandInfo>;
            if (attacks == null || attacks.Count == 0)
            {
                attacks = create.Invoke(command, null) as List<AttackHandInfo>;
                attacksField.SetValue(command, attacks);
                indexField.SetValue(command, 0);
            }
            if (attacks == null || attacks.Count != 1 || attacks[0] == null)
                throw new InvalidOperationException(
                    "The native player-issued single attack plan was ambiguous.");
            int damage = target.Descriptor.Damage;
            try { trigger.Invoke(command, new object[] { attacks[0] }); }
            finally
            {
                target.Descriptor.Damage = damage;
                attacker.Commands.InterruptAll(true);
            }
            if (command.LastAttackRule == null ||
                command.LastAttackRule.AttackRoll == null ||
                command.LastAttackRule.AttackRoll.AttackBonusRule == null ||
                command.LastAttackRule.AttackRoll.ACRule == null)
                throw new InvalidOperationException(
                    "The player-issued UnitAttack did not expose its native attack and AC rules.");
            return command.LastAttackRule;
        }

        private static bool HasBonusSource(IEnumerable<BonusSource> sources,
            BlueprintFeature source, int value)
        {
            return sources != null && source != null && sources.Count(item =>
                item.Bonus == value && item.Source != null &&
                ReferenceEquals(item.Source.Blueprint, source)) == 1;
        }

        private static int ArmorClass(UnitEntityData defender,
            UnitEntityData attacker)
        {
            return Rulebook.Trigger(new RuleCalculateAC(attacker, defender,
                AttackType.Melee)).TargetAC;
        }

        private static void SetPosition(UnitEntityData unit, Vector3 position)
        {
            PropertyInfo property = typeof(UnitEntityData).GetProperty(
                "Position", Members);
            property.SetValue(unit, position, null);
            if (unit.View != null) unit.View.transform.position = position;
        }

        private static object Read(object value, string name)
        {
            Type type = value.GetType();
            PropertyInfo property = type.GetProperty(name, Members);
            if (property != null) return property.GetValue(value, null);
            FieldInfo field = type.GetField(name, Members);
            if (field != null) return field.GetValue(value);
            throw new MissingMemberException(type.FullName, name);
        }

        private static object[] Snapshot(object source)
        {
            IEnumerable enumerable = source as IEnumerable;
            return enumerable == null ? new object[0] :
                enumerable.Cast<object>().ToArray();
        }

        private static MechanicActionBarSlotAbility LiveSlot(UnitEntityData unit,
            AbilityData ability)
        {
            if (unit == null || ability == null) throw new ArgumentNullException(
                unit == null ? "unit" : "ability");
            var slot = new MechanicActionBarSlotAbility { Ability = ability };
            FieldInfo unitField = typeof(MechanicActionBarSlot).GetField("Unit",
                Members);
            if (unitField == null || unitField.FieldType != typeof(UnitEntityData))
                throw new MissingFieldException(
                    typeof(MechanicActionBarSlot).FullName, "Unit");
            unitField.SetValue(slot, unit);
            return slot;
        }

        private static AbilityData[] LivePanelVariants(UnitEntityData unit,
            AbilityData selector)
        {
            return selector == null ? new AbilityData[0] :
                LiveSlot(unit, selector).GetConvertedAbilityData().ToArray();
        }

        private static string DescribeModifiers(
            Kingmaker.EntitySystem.Stats.ModifiableValue stat)
        {
            return string.Join("|", stat.Modifiers.Select(value =>
                value.ModValue + ":" + value.ModDescriptor + ":" +
                (value.SourceComponent == null ? "<null>" :
                    value.SourceComponent.GetType().FullName)));
        }

        private static string EnemyState(UnitEntityData owner,
            UnitEntityData candidate)
        {
            return "inGame:" + candidate.IsInGame + ",destroyed:" +
                candidate.Destroyed + ",detached:" + candidate.IsDetached +
                ",on:" + candidate.IsTurnedOn + ",conscious:" +
                candidate.Descriptor.State.IsConscious + ",enemy:" +
                owner.IsEnemy(candidate);
        }

        private static void RemoveIntroducedBuffs(UnitDescriptor owner,
            Buff[] before)
        {
            foreach (Buff introduced in owner.Buffs.RawFacts.OfType<Buff>()
                .Where(value => !before.Contains(value)).ToArray())
                introduced.Remove();
        }

        private static bool Same(object[] left, object[] right)
        {
            return left.Length == right.Length && left.Zip(right,
                (a, b) => ReferenceEquals(a, b)).All(value => value);
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string id, string expected, string observed, bool passed,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = id,
                Expected = expected, Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail, Evidence = evidence });
        }
    }
}
