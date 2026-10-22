using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Astral31Prefix = "KMG.Summoning.OutsiderCelestial.AstralDeva.";
        internal const string Astral31Unit = "KMG.Summoning.Unit.AstralDeva";

        private static void ConfigureAstralDeva31(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            AstralDevaRulesPolicy.Validate();
            var unit = Require<BlueprintUnit>(bySymbol, Astral31Unit);
            var type = Require<BlueprintUnitType>(bySymbol, Astral31Prefix + "UnitType");
            type.name = InternalName(Astral31Prefix + "UnitType");
            type.Name = LocalizationService.Create(Astral31Prefix + "UnitType.Name", "Astral Deva");
            type.Image = null; type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            var traits = Require<BlueprintFeature>(bySymbol, Astral31Prefix + "Traits");
            traits.name = InternalName(Astral31Prefix + "Traits");
            traits.HideInUI = true; traits.IsClassFeature = false;

            var hammer = Sprint29Weapon(library, bySymbol, Astral31Prefix + "Warhammer1d8",
                Astral31Prefix + "WarhammerType", "3f35d5c01e11d564daa59938dec3db4b",
                Size.Medium, 1, DiceType.D8, 9);
            SetField(hammer.Type, "m_IsTwoHanded", true);
            SetField(hammer, "m_Enchantments", new[] {
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(library,
                    MagicFirearmBlueprints.Enhancement2Guid, "Astral native +2 enhancement"),
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(library,
                    "0f20d79b7049c0f4ca54ca3d1ea44baa", "native disruption enchantment") });
            var slam = Sprint29Weapon(library, bySymbol, Astral31Prefix + "Slam1d8",
                Astral31Prefix + "SlamType", NativeSlam, Size.Medium, 1, DiceType.D8, 9);
            unit.Body = new BlueprintUnit.UnitBody {
                PrimaryHand = hammer, PrimaryHandAlternative1 = slam,
                EmptyHandWeapon = null,
                AdditionalLimbs = Array.Empty<BlueprintItemWeapon>(),
                AdditionalSecondaryLimbs = Array.Empty<BlueprintItemWeapon>(),
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>() };

            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                library, OutsiderClassGuid, "Astral outsider racial HD");
            levels.Levels = AstralDevaRulesPolicy.HitDice; levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown; levels.Skills = Array.Empty<StatType>();
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var racial = ScriptableObject.CreateInstance<SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit; racial.BaseRacialHitPoints = AstralDevaRulesPolicy.BaseRacialHitPoints;
            racial.BaseFortitude = AstralDevaRulesPolicy.BaseFortitude;
            racial.BaseReflex = AstralDevaRulesPolicy.BaseReflex;
            racial.BaseWill = AstralDevaRulesPolicy.BaseWill;
            racial.PerceptionRanks = 15; racial.StealthRanks = 15;
            // The native outsider class supplies the trained +3 for both
            // ranked class skills; adding another carrier produces +29/+25.
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial };
            unit.Skills = new BlueprintUnit.UnitSkills(); unit.Race = null; unit.Type = type;

            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(10); dr.BypassedByAlignment = true; dr.Alignment = DamageAlignment.Evil;
            var poison = ScriptableObject.CreateInstance<SavingThrowBonusAgainstDescriptor>();
            poison.SpellDescriptor = SpellDescriptor.Poison;
            poison.ModifierDescriptor = ModifierDescriptor.Racial; poison.Value = 4; poison.Bonus = Simple(0);
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>(); sr.Value = Simple(AstralDevaRulesPolicy.SpellResistance); sr.AddCR = false;
            var armor = ScriptableObject.CreateInstance<AddStatBonus>();
            armor.Stat = StatType.AC; armor.Value = AstralDevaRulesPolicy.NaturalArmor; armor.Descriptor = ModifierDescriptor.NaturalArmor;
            var alertness = ScriptableObject.CreateInstance<AddStatBonus>();
            alertness.Stat = StatType.SkillPerception; alertness.Value = 2;
            alertness.Descriptor = ModifierDescriptor.UntypedStackable;
            var racialPerception = ScriptableObject.CreateInstance<AddStatBonus>();
            racialPerception.Stat = StatType.SkillPerception; racialPerception.Value = 2;
            racialPerception.Descriptor = ModifierDescriptor.Racial;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit; equipment.GreatWeapon = hammer; equipment.Slam = slam;
            var proficiency = ScriptableObject.CreateInstance<AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { hammer.Category };
            proficiency.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            var focus = ScriptableObject.CreateInstance<WeaponFocus>();
            focus.WeaponType = hammer.Type; focus.AttackBonus = 1;

            ConfigureAstralProtectiveAura(bySymbol, unit);
            var heal = Require<BlueprintAbility>(bySymbol, Astral31Prefix + "Heal");
            var healResource = Require<BlueprintAbilityResource>(bySymbol, Astral31Prefix + "HealResource");
            ConfigureNamedResource(healResource, Astral31Prefix + "HealResource",
                Astral31Prefix + "HealResource", "Astral Deva Heal",
                "One Heal use for this Astral Deva summoning.", 1);
            Sprint29LimitedNativeSpell(library, heal, Astral31Prefix + "Heal",
                "ff8f1534f66559c478448723e16b6624", AstralDevaRulesPolicy.SpellLikeCasterLevel, 6, healResource, "Heal");
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit; parameters.Abilities = new[] { heal };
            parameters.SpellLevels = new[] { 6 };
            parameters.CasterLevel = AstralDevaRulesPolicy.SpellLikeCasterLevel;

            traits.ComponentsArray = new BlueprintComponent[] { dr, poison, sr, armor,
                alertness, racialPerception,
                equipment, proficiency, focus, parameters,
                AddResource(healResource),
                Energy(DamageEnergyType.Electricity, 10), Energy(DamageEnergyType.Fire, 10) }
                .Concat(ArchonPetrificationImmunities()).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Astral31Prefix + "Traits.Name", "Astral Deva Traits"),
                LocalizationService.Create(Astral31Prefix + "Traits.Description",
                    "Angel: DR10/evil; acid, cold and petrification immunity; electricity and fire resistance10; +4 racial saves vs poison; SR25 and Uncanny Dodge. A distinct20-foot protective aura grants +4 deflection AC and +4 resistance saves against evil sources, the shared Magic Circle against Evil, and lesser-globe protection from level0-3 spells and SLAs cast from outside. Heal is once per summoning at CL13. Change shape and caster travel are unavailable."), null);

            var nativeBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "Astral ordinary attack fallback");
            var brain = Require<BlueprintBrain>(bySymbol, Astral31Prefix + "Brain");
            CopyFields(nativeBrain, brain); brain.name = InternalName(Astral31Prefix + "Brain");
            brain.ComponentsArray = (nativeBrain.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            var healAi = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Astral31Prefix + "HealAi"), Astral31Prefix + "HealAi", heal, 0);
            var healEligibility = Require<AstralHealingEligibility>(bySymbol,
                Astral31Prefix + "HealEligibility");
            healEligibility.name = InternalName(Astral31Prefix + "HealEligibility");
            healEligibility.OwningBlueprint = unit; healEligibility.Heal = heal;
            healAi.TargetConsiderations = new Consideration[] { healEligibility };
            healAi.BaseScore = 6;
            brain.Actions = (nativeBrain.Actions ?? Array.Empty<BlueprintAiAction>())
                .Concat(new BlueprintAiAction[] { healAi }).ToArray(); unit.Brain = brain;

            ConfigureUnitCore(unit, "AstralDeva", "Astral Deva", Alignment.NeutralGood,
                Size.Medium, AstralDevaRulesPolicy.Strength, AstralDevaRulesPolicy.Dexterity,
                AstralDevaRulesPolicy.Constitution, AstralDevaRulesPolicy.Intelligence,
                AstralDevaRulesPolicy.Wisdom, AstralDevaRulesPolicy.Charisma, 50);
            ConfigureOutsiderOriginalExpiryVisual(library, unit);
            unit.AddFacts = new BlueprintUnitFact[] {
                Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, GoodSubtypeGuid, "good subtype"),
                Feature(library, ExtraplanarSubtypeGuid, "native extraplanar spell consumer"),
                Feature(library, AirborneGuid, "ordinary-map angel aerial convention"),
                Feature(library, AcidImmunityGuid, "angel acid immunity"),
                Feature(library, ColdImmunityGuid, "angel cold immunity"),
                Feature(library, "3c08d842e802c3e4eb19d15496145709", "native Uncanny Dodge"),
                Feature(library, NativeCleaveFeature, "ordinary native Cleave"),
                Feature(library, "79042cb55f030614ea29956177977c52", "Great Fortitude"),
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, IronWillGuid, "Iron Will"),
                Feature(library, PowerAttackGuid, "Power Attack"),
                Feature(library, "d09b20029e9abfe4480b356c92095623", "Toughness"),
                extraplanar, traits, heal };
        }

        private static void ConfigureAstralProtectiveAura(
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintUnit unit)
        {
            var carrier = Require<BlueprintBuff>(bySymbol, Astral31Prefix + "ProtectiveCarrier");
            var area = Require<BlueprintAbilityAreaEffect>(bySymbol, Astral31Prefix + "ProtectiveArea");
            var recipient = Require<BlueprintBuff>(bySymbol, Astral31Prefix + "ProtectiveRecipient");
            carrier.name = InternalName(Astral31Prefix + "ProtectiveCarrier");
            area.name = InternalName(Astral31Prefix + "ProtectiveArea");
            recipient.name = InternalName(Astral31Prefix + "ProtectiveRecipient");
            carrier.Stacking = StackingType.Stack; recipient.Stacking = StackingType.Stack;
            var ac = ScriptableObject.CreateInstance<ArmorClassBonusAgainstAlignment>();
            ac.alignment = AlignmentComponent.Evil; ac.Value = 4;
            ac.Descriptor = ModifierDescriptor.Deflection;
            var saves = ScriptableObject.CreateInstance<SavingThrowBonusAgainstAlignment>();
            saves.Alignment = AlignmentComponent.Evil; saves.Value = 4;
            saves.Descriptor = ModifierDescriptor.Resistance;
            var globe = ScriptableObject.CreateInstance<AstralProtectiveGlobe>();
            globe.OwningBlueprint = unit; globe.MaximumSpellLevel = AstralDevaRulesPolicy.LesserGlobeMaximumSpellLevel;
            globe.RadiusFeet = AstralDevaRulesPolicy.AuraRadiusFeet;
            recipient.ComponentsArray = new BlueprintComponent[] { ac, saves, globe };
            var delivery = ScriptableObject.CreateInstance<AbilityAreaEffectBuff>();
            delivery.Buff = recipient;
            delivery.Condition = new ConditionsChecker { Conditions = Array.Empty<Condition>() };
            area.Shape = AreaEffectShape.Cylinder;
            area.Size = new Feet(AstralDevaRulesPolicy.AuraRadiusFeet);
            area.AffectEnemies = true; area.AggroEnemies = false; area.AffectDead = false;
            area.IgnoreSleepingUnits = false; area.SpellResistance = false; area.Fx = new PrefabLink();
            area.ComponentsArray = new BlueprintComponent[] { delivery };
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>(); emanation.AreaEffect = area;
            carrier.ComponentsArray = new BlueprintComponent[] { emanation };
            if (carrier.StayOnDeath)
                throw new InvalidOperationException("Astral protective aura must end on emitter death.");
            var access = BlueprintUnitFactAccess.Resolve();
            var title = LocalizationService.Create(Astral31Prefix + "ProtectiveAura.Name", "Astral Deva Protective Aura");
            access.Configure(carrier, title,
                LocalizationService.Create(Astral31Prefix + "ProtectiveCarrier.Description",
                    "A20-foot angel aura protects every covered creature against evil sources and outside low-level magic."), null);
            access.Configure(recipient, title,
                LocalizationService.Create(Astral31Prefix + "ProtectiveRecipient.Description",
                    "+4 deflection AC and +4 resistance saves against evil sources. Level0-3 spells and SLAs cast from outside this deva's20-foot aura cannot affect this creature."), null);
        }
    }
}
