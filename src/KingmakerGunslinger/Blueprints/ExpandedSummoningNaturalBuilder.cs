using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningNaturalBuilder
    {
        private const string Bite1d4Symbol =
            "KMG.Summoning.Natural.Bite1d4";
        private const string Bite1d3Symbol =
            "KMG.Summoning.Natural.Bite1d3";
        private const string NativeBite1d6Guid =
            "a000716f88c969c499a535dadcf09286";
        private const string NativeClaw1d4Guid =
            "118fdd03e569a66459ab01a20af6811a";
        private const string AnimalClassGuid =
            "4cd1757a0eea7694ba5c933729a53920";
        private const string VerminClassGuid =
            "d1a15612d1a96334d94edf5f1d3b8d29";
        private const string DumbBrainGuid =
            "5abc8884c6f15204c8604cb01a2efbab";
        private const string NaturalArmor1Guid =
            "10c7c5e3c5806bc4ca676e22d6fbf17e";
        private const string NaturalArmor2Guid =
            "45a52ce762f637f4c80cc741c91f58b7";
        private const string ExtraplanarGuid =
            "136fa0343d5b4d348bdaa05d83408db3";

        private static readonly IDictionary<string, string> FactGuids =
            new Dictionary<string, string>(StringComparer.Ordinal) {
                { "TripDefenseFourLegs", "13c87ac5985cc85498ef9d1ac8b78923" },
                { "TripDefenseEightLegs", "a60900c666b2b37478a2bf4bb005973d" },
                { "TripImmune", "c1b26f97b974aec469613f968439e7bb" },
                { "TrippingBite", "f957b4444b6fb404e84ae2a5765797bb" },
                { "SkillFocusPerception", "f74c6bdf5c5f5374fb9302ecdc1f7d64" },
                { "WeaponFinesse", "90e54424d682d104ab36436bd527af09" },
                { "Airborne", "70cffb448c132fa409e49156d013b175" },
                { "PoisonFrog", "1a3f2f384bbef804d8f52db1f9aa62d3" },
                { "CentipedePoison", "6fed981bf0ef27a499969f369f35b5e8" },
                { "GiantSpiderPoison", "094714bb08f4e1943a8e9d2384ebe573" },
                { "Toughness", "d09b20029e9abfe4480b356c92095623" }
            };

        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (bySymbol == null) throw new ArgumentNullException("bySymbol");
            ExpandedSummoningNaturalProfiles.Validate();
            BlueprintItemWeapon nativeBite = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBite1d6Guid,
                    "native bite animation weapon");
            ConfigureWeapon(nativeBite, Require<BlueprintItemWeapon>(bySymbol,
                Bite1d4Symbol), Bite1d4Symbol, DiceType.D4);
            ConfigureWeapon(nativeBite, Require<BlueprintItemWeapon>(bySymbol,
                Bite1d3Symbol), Bite1d3Symbol, DiceType.D3);
            foreach (NaturalSummonProfile profile in
                ExpandedSummoningNaturalProfiles.All)
                ConfigureUnit(library, Require<BlueprintUnit>(bySymbol,
                    ExpandedSummoningIdentityCatalog.UnitSymbol(
                        ExpandedSummoningCatalog.All.Single(creature =>
                            creature.Key == profile.Key))), profile, bySymbol);
        }

        private static void ConfigureWeapon(BlueprintItemWeapon native,
            BlueprintItemWeapon target, string symbol, DiceType dice)
        {
            CopyFields(native, target);
            target.name = InternalName(symbol);
            target.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(target, "m_OverrideDamageDice", true);
            SetField(target, "m_DamageDice", new DiceFormula(1, dice));
            SetField(target, "m_Enchantments", Array.Empty<Kingmaker.Blueprints
                .Items.Ecnchantments.BlueprintWeaponEnchantment>());
        }

        private static void ConfigureUnit(LibraryScriptableObject library,
            BlueprintUnit unit, NaturalSummonProfile profile,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var levels = UnityEngine.ScriptableObject.CreateInstance<
                AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<
                BlueprintCharacterClass>(library,
                    profile.HitDieClass == "Animal" ? AnimalClassGuid :
                        VerminClassGuid,
                    profile.HitDieClass + " racial hit dice");
            levels.Levels = profile.HitDice;
            levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown;
            levels.Skills = new[] { StatType.SkillPerception,
                StatType.SkillMobility, StatType.SkillStealth };
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            unit.ComponentsArray = new BlueprintComponent[] { levels };

            BlueprintItemWeapon primary = Weapon(library, bySymbol,
                profile.PrimaryWeapon);
            BlueprintItemWeapon[] additional = profile.AdditionalWeapons.Select(
                key => Weapon(library, bySymbol, key)).ToArray();
            unit.Body = new BlueprintUnit.UnitBody {
                PrimaryHand = primary,
                AdditionalLimbs = additional,
                AdditionalSecondaryLimbs = Array.Empty<BlueprintItemWeapon>(),
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment
                    .BlueprintItemEquipmentUsable>()
            };
            unit.Brain = BlueprintLibraryLookup.RequireExact<
                Kingmaker.Controllers.Brain.Blueprints.BlueprintBrain>(library,
                    DumbBrainGuid, "bounded natural-attack brain");
            SharedStringAsset name = UnityEngine.ScriptableObject.CreateInstance<
                SharedStringAsset>();
            name.String = LocalizationService.Create(
                "KMG.ExpandedSummoning." + Token(profile.Key) + ".Unit.Name",
                profile.DisplayName);
            unit.LocalizedName = name;
            unit.Alignment = Alignment.TrueNeutral;
            unit.Size = ParseSize(profile.Size);
            unit.Strength = profile.Strength;
            unit.Dexterity = profile.Dexterity;
            unit.Constitution = profile.Constitution;
            unit.Intelligence = profile.Intelligence;
            unit.Wisdom = profile.Wisdom;
            unit.Charisma = profile.Charisma;
            unit.Speed = new Feet(profile.SpeedFeet);
            unit.BaseAttackBonus = 0;
            unit.MaxHP = 0;
            unit.StartingInventory = Array.Empty<BlueprintItem>();
            var facts = new List<BlueprintUnitFact>();
            if (profile.NaturalArmor == 1)
                facts.Add(BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(
                    library, NaturalArmor1Guid, "natural armor +1"));
            else if (profile.NaturalArmor == 2)
                facts.Add(BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(
                    library, NaturalArmor2Guid, "natural armor +2"));
            else if (profile.NaturalArmor != 0)
                throw new InvalidOperationException("Unsupported low-tier natural armor.");
            foreach (string fact in profile.Facts)
                facts.Add(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                    library, FactGuids[fact], profile.DisplayName + " " + fact));
            facts.Add(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, ExtraplanarGuid, "summoned extraplanar subtype"));
            unit.AddFacts = facts.ToArray();
        }

        private static BlueprintItemWeapon Weapon(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, string key)
        {
            if (key == "Bite1d4") return Require<BlueprintItemWeapon>(bySymbol,
                Bite1d4Symbol);
            if (key == "Bite1d3") return Require<BlueprintItemWeapon>(bySymbol,
                Bite1d3Symbol);
            if (key == "Bite1d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBite1d6Guid, "1d6 bite");
            if (key == "Claw1d4") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw1d4Guid, "1d4 claw");
            throw new InvalidOperationException("Unknown natural weapon key " +
                key + ".");
        }

        private static Size ParseSize(string value)
        {
            Size parsed;
            if (!Enum.TryParse(value, out parsed))
                throw new InvalidOperationException("Unknown size " + value + ".");
            return parsed;
        }

        private static void CopyFields(object source, object target)
        {
            foreach (FieldInfo field in Fields(source.GetType()))
            {
                if (field.Name == "m_AssetGuid" || field.IsInitOnly ||
                    field.DeclaringType == typeof(UnityEngine.Object)) continue;
                object value = field.GetValue(source);
                Array array = value as Array;
                field.SetValue(target, array == null ? value : array.Clone());
            }
        }

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = Fields(target.GetType()).SingleOrDefault(candidate =>
                candidate.Name == name);
            if (field == null) throw new MissingFieldException(
                target.GetType().FullName, name);
            field.SetValue(target, value);
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null; current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags)) yield return field;
        }

        private static T Require<T>(IDictionary<string, BlueprintScriptableObject>
            values, string symbol) where T : BlueprintScriptableObject
        { return (T)values[symbol]; }
        private static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }
        private static string Token(string key)
        { return string.Concat(key.Split('-').Select(part =>
            char.ToUpperInvariant(part[0]) + part.Substring(1))); }
    }
}
