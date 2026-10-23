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
        internal const string Trumpet31Prefix = "KMG.Summoning.OutsiderCelestial.TrumpetArchon.";
        private const string Trumpet31Unit = "KMG.Summoning.Unit.TrumpetArchon";
        private const string NativeClericBook31 = "4673d19a0cf2fab4f885cc4d1353da33";

        // Hidden caster continuation after the native Heal ownership proof.
        // Available printed preparations and the first private missing spell
        // use the real book. The remaining named consumers stay explicit.
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
            // Native Heal is named HealCast. Its exact identity was retained in
            // the bounded MovanicDeva input capture; require actual cleric L6
            // membership and Spell type rather than accepting a donor SLA.
            var sixth = nativeBook.SpellList.SpellsByLevel.Single(row => row.SpellLevel == 6).Spells;
            var heal = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                "5da172c4c89f9eb4cbb614f3a67357d3", "captured native HealCast");
            if (!sixth.Contains(heal) || heal.Type != AbilityType.Spell)
                throw new InvalidOperationException("Native HealCast is not a cleric level6 spell: " +
                    heal.name + "/" + heal.Type + ";level6=" + string.Join("|", sixth.Select(value => value.name + ":" + value.AssetGuid).ToArray()));
            var purge = ConfigureTrumpet31Purge(library, bySymbol);
            var dispelEvil = ConfigureDispelEvil31(library, bySymbol, unit,
                Trumpet31Prefix, AbilityType.Spell,
                "Spend one prepared 5th-level spell at caster level14 to gain +4 deflection AC against attacks by evil creatures for 14 rounds and hold one melee touch with either discharge available until use. The touch either invokes Dismissal against an evil extraplanar creature (Will negates; spell resistance applies) or automatically removes one dispellable evil spell or evil-caster enchantment from a creature. A delivered touch discharges the ward.");
            var preparations = Trumpet31NativePreparations(library, nativeBook).Concat(new[] {
                new TrumpetPreparedSpell { Level = 3, Ability = purge, Copies = 1 },
                new TrumpetPreparedSpell { Level = 5, Ability = dispelEvil[0], Copies = 1 } }).ToArray();
            var list = Require<BlueprintSpellList>(bySymbol, Trumpet31Prefix + "SpellList");
            list.name = InternalName(Trumpet31Prefix + "SpellList");
            list.SpellsByLevel = Enumerable.Range(0, 10).Select(level => new SpellLevelList {
                SpellLevel = level, Spells = preparations.Where(row => row.Level == level)
                    .Select(row => row.Ability).Distinct().ToList() }).ToArray();
            var book = Require<BlueprintSpellbook>(bySymbol, Trumpet31Prefix + "Spellbook");
            CopyFields(nativeBook, book); book.name = InternalName(Trumpet31Prefix + "Spellbook");
            book.Name = LocalizationService.Create(Trumpet31Prefix + "Spellbook.Name", "Trumpet Archon Prepared Spells");
            book.ComponentsArray = Array.Empty<BlueprintComponent>(); book.SpellList = list;
            book.Spontaneous = false; book.AllSpellsKnown = false; book.CanCopyScrolls = false;
            book.CastingAttribute = StatType.Wisdom; book.CasterLevelModifier = 0;
            var preparation = ScriptableObject.CreateInstance<TrumpetPreparedBook>();
            preparation.OwningBlueprint = unit; preparation.Book = book; preparation.Heal = heal;
            preparation.Preparations = preparations;
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
            var virtue = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                "d3a852385ba4cd740992d1970170301a", "captured printed Virtue orison");
            if (virtue.Type != AbilityType.Spell || virtue.GetComponent<CantripComponent>() == null)
                throw new InvalidOperationException("Trumpet Virtue must remain a native Spell/Cantrip fact.");
            var orison = ScriptableObject.CreateInstance<TrumpetOrisonParameters>();
            orison.OwningBlueprint = unit; orison.Book = book; orison.Orison = virtue;
            var namedSkills = ScriptableObject.CreateInstance<TrumpetNamedPersuasion>(); namedSkills.OwningBlueprint = unit;
            var proficiency = ScriptableObject.CreateInstance<AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { sword.Category }; proficiency.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            var escapeArtist = ScriptableObject.CreateInstance<OutsiderEscapeArtist>();
            escapeArtist.OwningBlueprint = unit; escapeArtist.Ranks = 14; escapeArtist.ClassSkill = false;
            traits.ComponentsArray = new BlueprintComponent[] { dr, poison, sr, armor, equipment, proficiency, orison, namedSkills, escapeArtist }
                .Concat(ArchonPetrificationImmunities()).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Trumpet31Prefix + "Traits.Name", "Trumpet Archon Traits"),
                LocalizationService.Create(Trumpet31Prefix + "Traits.Description",
                    "DR10/evil, electricity and petrification immunity, +4 racial saves vs poison, SR25. Prepared divine spells use the archon's own spell slots at caster level14. Escape Artist +17 uses the native break-free consumer only; ordinary Mobility is unchanged."), null);
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
            var banishment = preparations.Single(row => row.Ability.AssetGuid == "d361391f645db984bbf58907711a146a").Ability;
            var banishAi = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol, Trumpet31Prefix + "BanishmentAi"),
                Trumpet31Prefix + "BanishmentAi", banishment, 0);
            var banishSafety = Require<TrumpetBanishmentEligibility>(bySymbol, Trumpet31Prefix + "BanishmentEligibility");
            banishSafety.OwningBlueprint = unit; banishSafety.Book = book; banishSafety.Ability = banishment;
            banishSafety.Extraplanar = Feature(library, ExtraplanarSubtypeGuid, "native Banishment target consumer"); banishAi.TargetConsiderations = new Consideration[] { banishSafety };
            banishAi.BaseScore = 5;
            brain.Actions = (nativeBrain.Actions ?? Array.Empty<BlueprintAiAction>()).Concat(new BlueprintAiAction[] { ai, banishAi }).ToArray();
            unit.Brain = brain;
            ConfigureUnitCore(unit, "TrumpetArchon", "Trumpet Archon", Alignment.LawfulGood, Size.Medium,
                20, 17, 25, 16, 20, 17, 40);
            unit.AddFacts = new BlueprintUnitFact[] { Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, GoodSubtypeGuid, "good subtype"), Feature(library, LawfulSubtypeGuid, "lawful subtype"),
                Feature(library, ElectricityImmunityGuid, "archon electricity immunity"),
                Feature(library, NativeCleaveFeature, "ordinary native Cleave"),
                Feature(library, CombatReflexesGuid, "Combat Reflexes"), Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, LightningReflexesGuid, "Lightning Reflexes"), Feature(library, PowerAttackGuid, "Power Attack"), Feature(library, ExtraplanarSubtypeGuid, "native extraplanar spell consumer"), extraplanar, traits, virtue }
                .Concat(ConfigureTrumpet31Feats(library, bySymbol, unit)).ToArray();
        }
        // Native leveled portion of the printed preparation. Native
        // UpdateAllSlotsSize initializes only levels1-7; do not send the
        // separate level-zero orison access path through Memorize.
        // Native portion of the printed preparation. Missing named combat
        // consumers are completed separately; their slots remain unprepared
        // until their real dependencies exist. Filtered travel/material spells
        // never become replacement combat slots. This is not the final loadout.
        private static TrumpetPreparedSpell[] Trumpet31NativePreparations(
            LibraryScriptableObject library, BlueprintSpellbook nativeBook)
        {
            var rows = new[] {
                new { Level = 1, Copies = 1, Guid = "90e59f4a4ada87243b7b3535a06d0638" }, // Bless
                new { Level = 1, Copies = 2, Guid = "5590652e1c2225c4ca30c4a699ab3649" }, // Cure Light Wounds
                new { Level = 1, Copies = 1, Guid = "9d5d2d3ffdd73c648af3eb3e585b1113" }, // Divine Favor
                new { Level = 1, Copies = 1, Guid = "183d5bb91dea3a1489a6db6c9cb64445" }, // Shield of Faith
                new { Level = 2, Copies = 1, Guid = "4c3d08935262b6544ae97599b3a9556d" }, // Bull's Strength
                new { Level = 2, Copies = 2, Guid = "6b90c773a6543dc49b2505858ce33db5" }, // Cure Moderate Wounds
                new { Level = 2, Copies = 1, Guid = "f0455c9295b53904f9e02fc571dd2ce1" }, // Owl's Wisdom
                new { Level = 2, Copies = 1, Guid = "e84fc922ccf952943b5240293669b171" }, // Lesser Restoration
                new { Level = 3, Copies = 1, Guid = "2d4263d80f5136b4296d6eb43a221d7d" }, // Magic Vestment, captured native alias
                new { Level = 3, Copies = 1, Guid = "3361c5df793b4c8448756146a88026ad" }, // Cure Serious Wounds
                new { Level = 3, Copies = 1, Guid = "d2f116cfe05fcdd4a94e80143b67046f" }, // Protection from Energy
                new { Level = 4, Copies = 1, Guid = "95f7cdcec94e293489a85afdf5af1fd7" }, // Dismissal
                new { Level = 4, Copies = 1, Guid = "ef16771cb05d1344989519e87f25b3c5" }, // Divine Power
                new { Level = 4, Copies = 1, Guid = "e7240516af4241b42b2cd819929ea9da" }, // Neutralize Poison
                new { Level = 5, Copies = 1, Guid = "5d3d689392e4ff740a761ef346815074" }, // Mass Cure Light Wounds
                new { Level = 6, Copies = 1, Guid = "d361391f645db984bbf58907711a146a" }, // Banishment
                new { Level = 6, Copies = 2, Guid = "5da172c4c89f9eb4cbb614f3a67357d3" }, // Heal
                new { Level = 7, Copies = 2, Guid = "0cea35de4d553cc439ae80b3a8724397" }, // Mass Cure Serious Wounds
            };
            return rows.Select(row => {
                var spell = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, row.Guid,
                    "captured Trumpet native cleric preparation");
                if (spell.Type != AbilityType.Spell || !nativeBook.SpellList.SpellsByLevel
                    .Single(level => level.SpellLevel == row.Level).Spells.Contains(spell))
                    throw new InvalidOperationException("Trumpet preparation is not the captured native cleric spell: " + row.Guid);
                return new TrumpetPreparedSpell { Level = row.Level, Ability = spell, Copies = row.Copies };
            }).ToArray();
        }
    }
}
