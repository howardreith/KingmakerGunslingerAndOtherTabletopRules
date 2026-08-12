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
        private const string Tail1d12Symbol =
            "KMG.Summoning.Natural.Tail1d12";
        private const string NativeBite1d6Guid =
            "a000716f88c969c499a535dadcf09286";
        private const string NativeBite1d8Guid =
            "c988aa874d11ff84d873508ddc9b928f";
        private const string NativeBite2d6Guid =
            "d2f99947db522e24293a7ec4eded453f";
        private const string NativeClaw1d3Guid =
            "800092a2b9a743b48ae8aeeb5d243dcc";
        private const string NativeClaw1d4Guid =
            "118fdd03e569a66459ab01a20af6811a";
        private const string NativeClaw1d6Guid =
            "c76f72a862d168d44838206524366e1c";
        private const string NativeGore1d8Guid =
            "73ed4e955295e62469fe471f1d49d9ef";
        private const string NativeGore2d6Guid =
            "d1f80b5c5c73cc84db7854774850b08c";
        private const string NativeTail1d8Guid =
            "ae822725634c6f0418b8c48bd29df255";
        private const string AnimalClassGuid =
            "4cd1757a0eea7694ba5c933729a53920";
        private const string VerminClassGuid =
            "d1a15612d1a96334d94edf5f1d3b8d29";
        private const string DumbBrainGuid =
            "5abc8884c6f15204c8604cb01a2efbab";
        private static readonly IDictionary<int, string> NaturalArmorGuids =
            new Dictionary<int, string> {
                { 1, "10c7c5e3c5806bc4ca676e22d6fbf17e" },
                { 2, "45a52ce762f637f4c80cc741c91f58b7" },
                { 3, "f6e106931f95fec4eb995f0d0629fb84" },
                { 4, "16fc201a83edcde4cbd64c291ebe0d07" },
                { 6, "987ba44303e88054c9504cb3083ba0c9" }
            };
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
                { "Toughness", "d09b20029e9abfe4480b356c92095623" },
                { "ReducedReach", "c33f2d68d93ceee488aa4004347dffca" },
                { "Ferocity", "955e356c813de1743a98ab3485d5bc69" },
                { "Pounce", "1a8149c09e0bdfc48a305ee6ac3729a8" },
                { "SkillFocusStealth", "3a8d34905eae4a74892aae37df3352b9" },
                { "GreatFortitude", "79042cb55f030614ea29956177977c52" },
                { "MonitorLizardPoison", "d88236a83413baa45ae9c8e5ddce5a6c" },
                { "ImprovedInitiative", "797f25d709f559546b29e7bcb181cc74" },
                { "Stealthy", "c7e1d5ef809325943af97f093e149c4f" },
                { "WeaponFocusBite", "b97edcf55321a814ea6b7807d246726c" },
                { "Dodge", "97e216dbb46ae3c4faef90cf6bbe6fd5" }
            };
        private static readonly ISet<string> BaseUnitFactKeys =
            new HashSet<string>(new[] { "ReducedReach", "Ferocity" },
                StringComparer.Ordinal);

        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (bySymbol == null) throw new ArgumentNullException("bySymbol");
            if (extraplanar == null) throw new ArgumentNullException("extraplanar");
            ExpandedSummoningNaturalProfiles.Validate();
            BlueprintItemWeapon nativeBite = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBite1d6Guid,
                    "native bite animation weapon");
            ConfigureWeapon(nativeBite, Require<BlueprintItemWeapon>(bySymbol,
                Bite1d4Symbol), Bite1d4Symbol, DiceType.D4);
            ConfigureWeapon(nativeBite, Require<BlueprintItemWeapon>(bySymbol,
                Bite1d3Symbol), Bite1d3Symbol, DiceType.D3);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeTail1d8Guid,
                    "native animated tail weapon"),
                Require<BlueprintItemWeapon>(bySymbol, Tail1d12Symbol),
                Tail1d12Symbol, DiceType.D12);
            foreach (NaturalSummonProfile profile in
                ExpandedSummoningNaturalProfiles.All)
                ConfigureUnit(library, Require<BlueprintUnit>(bySymbol,
                    ExpandedSummoningIdentityCatalog.UnitSymbol(
                        ExpandedSummoningCatalog.All.Single(creature =>
                            creature.Key == profile.Key))), profile, bySymbol,
                    extraplanar);
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
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
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
            BlueprintItemWeapon[] secondary = profile.AdditionalSecondaryWeapons
                .Select(key => Weapon(library, bySymbol, key)).ToArray();
            unit.Body = new BlueprintUnit.UnitBody {
                PrimaryHand = primary,
                AdditionalLimbs = additional,
                AdditionalSecondaryLimbs = secondary,
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
            if (profile.NaturalArmor != 0)
            {
                string armorGuid;
                if (!NaturalArmorGuids.TryGetValue(profile.NaturalArmor,
                        out armorGuid))
                    throw new InvalidOperationException(
                        "Unsupported natural armor value " +
                        profile.NaturalArmor + ".");
                facts.Add(BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(
                    library, armorGuid, "natural armor +" +
                        profile.NaturalArmor));
            }
            foreach (string fact in profile.Facts)
            {
                BlueprintUnitFact value = BaseUnitFactKeys.Contains(fact)
                    ? BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(
                        library, FactGuids[fact], profile.DisplayName + " " + fact)
                    : BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                        library, FactGuids[fact], profile.DisplayName + " " + fact);
                facts.Add(value);
            }
            facts.Add(extraplanar);
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
            if (key == "Bite1d8") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBite1d8Guid, "1d8 bite");
            if (key == "Bite2d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBite2d6Guid, "2d6 bite");
            if (key == "Claw1d3") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw1d3Guid, "1d3 claw");
            if (key == "Claw1d4") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw1d4Guid, "1d4 claw");
            if (key == "Claw1d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw1d6Guid, "1d6 claw");
            if (key == "Gore1d8") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeGore1d8Guid, "1d8 gore");
            if (key == "Gore2d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeGore2d6Guid, "2d6 gore");
            if (key == "Tail1d12") return Require<BlueprintItemWeapon>(bySymbol,
                Tail1d12Symbol);
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
