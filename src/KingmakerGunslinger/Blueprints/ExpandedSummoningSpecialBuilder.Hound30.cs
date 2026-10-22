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
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private const string Hound30Prefix = "KMG.Summoning.OutsiderCelestial.HoundArchon.";
        private const string Hound30Unit = "KMG.Summoning.Unit.HoundArchon";
        private static void ConfigureHoundArchon30(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintFeature extraplanar)
        {
            var unit = Require<BlueprintUnit>(bySymbol, Hound30Unit);
            var type = Require<BlueprintUnitType>(bySymbol, Hound30Prefix + "UnitType");
            type.name = InternalName(Hound30Prefix + "UnitType");
            type.Name = LocalizationService.Create(Hound30Prefix + "UnitType.Name", "Hound Archon");
            type.Image = null; type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            var traits = Require<BlueprintFeature>(bySymbol, Hound30Prefix + "Traits");
            traits.name = InternalName(Hound30Prefix + "Traits"); traits.HideInUI = true; traits.IsClassFeature = false;
            var sword = Sprint29Weapon(library, bySymbol, Hound30Prefix + "Greatsword2d6",
                Hound30Prefix + "GreatswordType", NativeGreatsword, Size.Medium, 2, DiceType.D6, 9);
            SetField(sword, "m_Enchantments", new[] { BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(
                library, EasternWeaponBlueprints.NativeMasterworkGuid, "Hound masterwork greatsword accuracy") });
            var bite = Sprint29Weapon(library, bySymbol, Hound30Prefix + "Bite1d8",
                Hound30Prefix + "BiteType", NativeBite1d6Guid, Size.Medium, 1, DiceType.D8, 9);
            var slam = Sprint29Weapon(library, bySymbol, Hound30Prefix + "Slam1d4",
                Hound30Prefix + "SlamType", NativeSlam, Size.Medium, 1, DiceType.D4, 9);
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, OutsiderClassGuid, "Hound outsider racial HD");
            levels.Levels = 6; levels.RaceStat = StatType.Constitution; levels.LevelsStat = StatType.Unknown;
            levels.Skills = Array.Empty<StatType>(); levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>(); levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var racial = ScriptableObject.CreateInstance<SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit; racial.BaseRacialHitPoints = 33;
            racial.BaseFortitude = 5; racial.BaseReflex = 5; racial.BaseWill = 2;
            racial.PerceptionRanks = 6; racial.PersuasionRanks = 6; racial.StealthRanks = 6; racial.MobilityRanks = 6;
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial };
            unit.Skills = new BlueprintUnit.UnitSkills(); unit.Type = type; unit.Race = null;
            // Installed IsSecondary automatically makes a natural bite secondary
            // beside a manufactured weapon. It returns primary when the active
            // hand is natural. Do not force this limb secondary permanently.
            unit.Body = new BlueprintUnit.UnitBody {
                // A natural EmptyHandWeapon also equips the secondary hand.
                // The alternative already owns its one slam in the main hand;
                // leave the native empty hand empty, as in NaturalBody.
                PrimaryHand = sword, PrimaryHandAlternative1 = slam, EmptyHandWeapon = null,
                AdditionalLimbs = new[] { bite }, AdditionalSecondaryLimbs = Array.Empty<BlueprintItemWeapon>(),
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>() };
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(10); dr.BypassedByAlignment = true; dr.Alignment = DamageAlignment.Evil;
            var poison = ScriptableObject.CreateInstance<SavingThrowBonusAgainstDescriptor>();
            poison.SpellDescriptor = SpellDescriptor.Poison; poison.ModifierDescriptor = ModifierDescriptor.Racial;
            poison.Value = 4; poison.Bonus = Simple(0);
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>(); sr.Value = Simple(15); sr.AddCR = false;
            var armor = ScriptableObject.CreateInstance<AddStatBonus>(); armor.Stat = StatType.AC;
            armor.Value = 9; armor.Descriptor = ModifierDescriptor.NaturalArmor;
            var stealth = ScriptableObject.CreateInstance<AddStatBonus>(); stealth.Stat = StatType.SkillStealth;
            stealth.Value = 4; stealth.Descriptor = ModifierDescriptor.Racial;
            var intimidation = ScriptableObject.CreateInstance<SummonGiantIntimidateClassSkill>(); intimidation.OwningBlueprint = unit;
            var strength = ScriptableObject.CreateInstance<DeinonychusPrimaryTalonStrength>(); strength.OwningBlueprint = unit; strength.Talon = slam;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit; equipment.GreatWeapon = sword; equipment.Slam = slam; equipment.Rock = bite;
            var proficiency = ScriptableObject.CreateInstance<AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { sword.Category };
            proficiency.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            traits.ComponentsArray = new BlueprintComponent[] { dr, poison, sr, armor, stealth, intimidation,
                strength, equipment, proficiency }.Concat(ArchonPetrificationImmunities()).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Hound30Prefix + "Traits.Name", "Hound Archon Traits"),
                LocalizationService.Create(Hound30Prefix + "Traits.Description",
                    "DR10/evil, electricity and petrification immunity, +4 racial saves vs poison, SR15. Armed greatsword and secondary bite, or primary slam and bite; live general PF1 Strength scaling. Its10-foot Magic Circle protects covered creatures against evil; Aid is available at will at CL6. Change shape and teleportation are unavailable to this summon."), null);
            var nativeBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library, DumbBrainGuid, "Hound ordinary attack fallback");
            var brain = Require<BlueprintBrain>(bySymbol, Hound30Prefix + "Brain");
            CopyFields(nativeBrain, brain); brain.name = InternalName(Hound30Prefix + "Brain");
            brain.ComponentsArray = (nativeBrain.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            brain.Actions = (nativeBrain.Actions ?? Array.Empty<BlueprintAiAction>()).ToArray(); unit.Brain = brain;
            ConfigureUnitCore(unit, "HoundArchon", "Hound Archon", Alignment.LawfulGood, Size.Medium,
                15, 10, 13, 10, 13, 12, 40);
            ConfigureOutsiderOriginalExpiryVisual(library, unit);
            unit.AddFacts = new BlueprintUnitFact[] { Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, GoodSubtypeGuid, "good subtype"), Feature(library, LawfulSubtypeGuid, "lawful subtype"),
                Feature(library, ElectricityImmunityGuid, "archon electricity immunity"),
                Feature(library, ImprovedInitiativeGuid, "Hound Improved Initiative"),
                Feature(library, IronWillGuid, "Hound Iron Will"), Feature(library, PowerAttackGuid, "Hound Power Attack"),
                Feature(library, ExtraplanarSubtypeGuid, "native extraplanar spell consumer"), extraplanar, traits };
        }
        private static BlueprintComponent[] ArchonPetrificationImmunities()
        {
            var condition = ScriptableObject.CreateInstance<AddConditionImmunity>(); condition.Condition = UnitCondition.Petrified;
            var buff = ScriptableObject.CreateInstance<BuffDescriptorImmunity>(); buff.CheckFact = false; buff.Descriptor = SpellDescriptor.Petrified;
            var ability = ScriptableObject.CreateInstance<SpellImmunityToSpellDescriptor>(); ability.Descriptor = SpellDescriptor.Petrified;
            return new BlueprintComponent[] { condition, buff, ability };
        }
        internal static void BindArchonMagicCircle(ExpandedSummoningBlueprintSet summoning, MagicCircleBlueprintSet[] circles)
        {
            if (summoning == null || circles == null) throw new InvalidOperationException("Archon shared Magic Circle dependency is absent.");
            var circle = circles.Single(value => value.Alignment == "Evil");
            var hound = Require<BlueprintUnit>(summoning.BySymbol, Hound30Unit);
            var traits = Require<BlueprintFeature>(summoning.BySymbol, Hound30Prefix + "Traits");
            if (traits.GetComponent<ArchonConstantMagicCircle>() != null)
                throw new InvalidOperationException("Hound shared circle was bound twice.");
            // Exact shared carrier/area/recipient, with their existing lifetime,
            // alignment, typed stacking and configured control behavior. Future
            // fixes to that implementation are consumed directly by Hound.
            var grant = ScriptableObject.CreateInstance<ArchonConstantMagicCircle>();
            grant.OwningBlueprint = hound; grant.Carrier = circle.Carrier; grant.Spell = circle.Spell; grant.CasterLevel = 6;
            traits.ComponentsArray = traits.ComponentsArray.Concat(new BlueprintComponent[] { grant }).ToArray();
            var trumpet = Require<BlueprintUnit>(summoning.BySymbol, Trumpet31Unit);
            var trumpetTraits = Require<BlueprintFeature>(summoning.BySymbol, Trumpet31Prefix + "Traits");
            var trumpetGrant = ScriptableObject.CreateInstance<ArchonConstantMagicCircle>();
            trumpetGrant.OwningBlueprint = trumpet; trumpetGrant.Carrier = circle.Carrier;
            trumpetGrant.Spell = circle.Spell; trumpetGrant.CasterLevel = 14;
            trumpetTraits.ComponentsArray = trumpetTraits.ComponentsArray.Concat(new BlueprintComponent[] { trumpetGrant }).ToArray();
        }
    }
}
