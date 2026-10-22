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
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private const string Trumpet31Prefix = "KMG.Summoning.OutsiderCelestial.TrumpetArchon.";
        private const string Trumpet31Unit = "KMG.Summoning.Unit.TrumpetArchon";
        private const string NativeClericBook31 = "4673d19a0cf2fab4f885cc4d1353da33";

        // First hidden caster slice: the real book and two printed Heal slots.
        // Remaining legal preparation, signature trumpet and presentation are
        // deliberately completed only after this native ownership path works.
        private static void ConfigureTrumpet31(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintFeature extraplanar)
        {
            var unit = Require<BlueprintUnit>(bySymbol, Trumpet31Unit);
            var type = Require<BlueprintUnitType>(bySymbol, Trumpet31Prefix + "UnitType");
            type.name = InternalName(Trumpet31Prefix + "UnitType");
            type.Name = LocalizationService.Create(Trumpet31Prefix + "UnitType.Name", "Trumpet Archon");
            type.Image = null; type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            var traits = Require<BlueprintFeature>(bySymbol, Trumpet31Prefix + "Traits");
            traits.name = InternalName(Trumpet31Prefix + "Traits"); traits.HideInUI = true;
            var sword = Sprint29Weapon(library, bySymbol, Trumpet31Prefix + "Greatsword2d6",
                Trumpet31Prefix + "GreatswordType", NativeGreatsword, Size.Medium, 2, DiceType.D6, 9);
            SetField(sword, "m_Enchantments", new[] { BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(
                library, MagicFirearmBlueprints.Enhancement4Guid, "Trumpet native +4 enhancement") });
            unit.Body = new BlueprintUnit.UnitBody { PrimaryHand = sword,
                AdditionalLimbs = Array.Empty<BlueprintItemWeapon>(), AdditionalSecondaryLimbs = Array.Empty<BlueprintItemWeapon>(),
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>() };
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, OutsiderClassGuid, "Trumpet racial HD");
            levels.Levels = 14; levels.RaceStat = StatType.Constitution; levels.LevelsStat = StatType.Unknown;
            levels.Skills = Array.Empty<StatType>(); levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>(); levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var racial = ScriptableObject.CreateInstance<SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit; racial.BaseRacialHitPoints = 77;
            racial.BaseFortitude = 9; racial.BaseReflex = 4; racial.BaseWill = 9;
            racial.PerceptionRanks = 14; racial.PersuasionRanks = 14; racial.StealthRanks = 14;
            var nativeBook = BlueprintLibraryLookup.RequireExact<BlueprintSpellbook>(library, NativeClericBook31, "native prepared cleric book");
            var heal = nativeBook.SpellList.SpellsByLevel.Single(row => row.SpellLevel == 6).Spells
                .Single(ability => ability.name == "Heal" && ability.Type == AbilityType.Spell);
            var list = Require<BlueprintSpellList>(bySymbol, Trumpet31Prefix + "SpellList");
            list.name = InternalName(Trumpet31Prefix + "SpellList");
            list.SpellsByLevel = Enumerable.Range(0, 10).Select(level => new SpellLevelList {
                SpellLevel = level, Spells = level == 6 ? new List<BlueprintAbility> { heal } : new List<BlueprintAbility>() }).ToArray();
            var book = Require<BlueprintSpellbook>(bySymbol, Trumpet31Prefix + "Spellbook");
            CopyFields(nativeBook, book); book.name = InternalName(Trumpet31Prefix + "Spellbook");
            book.Name = LocalizationService.Create(Trumpet31Prefix + "Spellbook.Name", "Trumpet Archon Prepared Spells");
            book.ComponentsArray = Array.Empty<BlueprintComponent>(); book.SpellList = list;
            book.Spontaneous = false; book.AllSpellsKnown = false; book.CanCopyScrolls = false;
            book.CastingAttribute = StatType.Wisdom; book.CasterLevelModifier = 0;
            var preparation = ScriptableObject.CreateInstance<TrumpetPreparedBook>();
            preparation.OwningBlueprint = unit; preparation.Book = book; preparation.Heal = heal;
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial, preparation };
            unit.Skills = new BlueprintUnit.UnitSkills(); unit.Race = null; unit.Type = type;
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(10); dr.BypassedByAlignment = true; dr.Alignment = DamageAlignment.Evil;
            var poison = ScriptableObject.CreateInstance<SavingThrowBonusAgainstDescriptor>();
            poison.SpellDescriptor = SpellDescriptor.Poison; poison.ModifierDescriptor = ModifierDescriptor.Racial;
            poison.Value = 4; poison.Bonus = Simple(0);
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>(); sr.Value = Simple(25); sr.AddCR = false;
            var armor = ScriptableObject.CreateInstance<AddStatBonus>(); armor.Stat = StatType.AC;
            armor.Value = 14; armor.Descriptor = ModifierDescriptor.NaturalArmor;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit; equipment.GreatWeapon = sword;
            var proficiency = ScriptableObject.CreateInstance<AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { sword.Category }; proficiency.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            traits.ComponentsArray = new BlueprintComponent[] { dr, poison, sr, armor, equipment, proficiency }
                .Concat(ArchonPetrificationImmunities()).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Trumpet31Prefix + "Traits.Name", "Trumpet Archon Traits"),
                LocalizationService.Create(Trumpet31Prefix + "Traits.Description",
                    "DR10/evil, electricity and petrification immunity, +4 racial saves vs poison, SR25. Prepared divine spells use the archon's own spell slots at caster level14."), null);
            var nativeBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library, DumbBrainGuid, "Trumpet ordinary attack fallback");
            var brain = Require<BlueprintBrain>(bySymbol, Trumpet31Prefix + "Brain");
            CopyFields(nativeBrain, brain); brain.name = InternalName(Trumpet31Prefix + "Brain");
            brain.ComponentsArray = (nativeBrain.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            var ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol, Trumpet31Prefix + "HealAi"),
                Trumpet31Prefix + "HealAi", heal, 0);
            var eligibility = Require<TrumpetPreparedHealingEligibility>(bySymbol, Trumpet31Prefix + "HealEligibility");
            eligibility.OwningBlueprint = unit; eligibility.Book = book; eligibility.Heal = heal;
            ai.TargetConsiderations = new Consideration[] { eligibility }; ai.BaseScore = 6;
            brain.Actions = (nativeBrain.Actions ?? Array.Empty<BlueprintAiAction>()).Concat(new BlueprintAiAction[] { ai }).ToArray();
            unit.Brain = brain;
            ConfigureUnitCore(unit, "TrumpetArchon", "Trumpet Archon", Alignment.LawfulGood, Size.Medium,
                20, 17, 25, 16, 20, 17, 40);
            unit.AddFacts = new BlueprintUnitFact[] { Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, GoodSubtypeGuid, "good subtype"), Feature(library, LawfulSubtypeGuid, "lawful subtype"),
                Feature(library, ElectricityImmunityGuid, "archon electricity immunity"),
                Feature(library, NativeCleaveFeature, "ordinary native Cleave"),
                Feature(library, CombatReflexesGuid, "Combat Reflexes"), Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, LightningReflexesGuid, "Lightning Reflexes"), Feature(library, PowerAttackGuid, "Power Attack"), extraplanar, traits };
        }
    }
}
