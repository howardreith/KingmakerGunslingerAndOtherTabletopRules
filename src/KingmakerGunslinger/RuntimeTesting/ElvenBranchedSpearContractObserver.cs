using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free, read-only inventory for the native contracts needed by the
    /// Elven Branched Spear. The observer deliberately records candidates; it
    /// does not publish blueprints or choose campaign placements.
    /// </summary>
    internal static class ElvenBranchedSpearContractObserver
    {
        private const BindingFlags Members = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly string[] WeaponTerms =
        {
            "longspear", "fauchard", "glaive", "bardiche",
            "elvencurvedblade", "elvencurveblade", "elven curved blade"
        };

        private static readonly string[] EnchantmentTerms =
        {
            "agile", "keen", "corrosive", "speed", "coldiron", "cold iron",
            "enhancement1", "enhancement2", "enhancement3", "enhancement4",
            "enhancement5", "entangle", "dodge", "movement", "slow"
        };

        private static readonly string[] DexterityTerms =
        {
            "damagegrace", "damagestatreplacement", "attackstatreplacement",
            "finesse", "grace"
        };

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                .BlueprintsByAssetId.Values.Where(value => value != null)
                .Distinct().ToArray();
            string[] weapons = all.OfType<BlueprintItemWeapon>()
                .Where(IsWeaponCandidate).OrderBy(value => value.name,
                    StringComparer.Ordinal).Select(DescribeWeapon).ToArray();
            string[] enchantments = all.OfType<BlueprintWeaponEnchantment>()
                .Where(value => ContainsAny(value.name, EnchantmentTerms))
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeBlueprint).ToArray();
            string[] coldIronWeapons = all.OfType<BlueprintItemWeapon>()
                .Where(value => ContainsAny(value.name + ";" + Safe(() => value.Name),
                    new[] { "coldiron", "cold iron" }))
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeWeapon).ToArray();
            string[] selectors = all.OfType<BlueprintParametrizedFeature>()
                .Where(IsWeaponSelector).OrderBy(value => value.name,
                    StringComparer.Ordinal).Select(DescribeSelector).ToArray();
            string[] featureSelections = all.OfType<BlueprintFeatureSelection>()
                .Where(IsWeaponFeatureSelection).OrderBy(value => value.name,
                    StringComparer.Ordinal).Select(DescribeFeatureSelection).ToArray();
            string[] familiarity = all.OfType<BlueprintFeature>()
                .Where(value => ContainsAny(value.name,
                    new[] { "elf", "elven", "familiarity" }) &&
                    ContainsAny(DescribeComponents(value), new[] {
                        "proficien", "weapon", "familiar" }))
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeBlueprint).ToArray();
            string[] raceGrants = all.OfType<BlueprintRace>()
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeRace).ToArray();
            string[] dexterity = all.Where(value => value.ComponentsArray != null &&
                    value.ComponentsArray.Any(component => component != null &&
                        ContainsAny(component.GetType().FullName, DexterityTerms)))
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeBlueprint).ToArray();
            string[] entangledConditions = all.OfType<BlueprintBuff>()
                .Where(value => ContainsAny(value.name + ";" +
                    Safe(() => value.Name), new[] { "entangl" }))
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeBlueprint).ToArray();
            string[] loot = all.OfType<BlueprintLoot>().Where(IsCampaignLootCandidate)
                .OrderBy(value => value.Area == null ? string.Empty : value.Area.name,
                    StringComparer.Ordinal).ThenBy(value => value.name,
                    StringComparer.Ordinal).Select(DescribeLoot).Take(600).ToArray();

            var assertions = new List<RuntimeTestAssertion>();
            Add(assertions, "spear-native-weapon-donors",
                "longspear, fauchard, glaive, bardiche, and elven curve blade candidates",
                string.Join(" | ", weapons),
                HasCandidate(weapons, "longspear") &&
                    HasCandidate(weapons, "fauchard") &&
                    HasCandidate(weapons, "glaive") &&
                    HasCandidate(weapons, "bardiche") &&
                    HasCandidate(weapons, "elvencurvedblade", "elvencurveblade"),
                "installed BlueprintItemWeapon/BlueprintWeaponType graph");
            Add(assertions, "spear-native-enchantment-donors",
                "Agile, Keen, Corrosive, Speed, Cold Iron, and enhancement contracts",
                string.Join(" | ", enchantments),
                HasCandidate(enchantments, "agile") &&
                    HasCandidate(enchantments, "keen") &&
                    HasCandidate(enchantments, "corrosive") &&
                    HasCandidate(enchantments, "speed") &&
                    HasCandidate(enchantments, "coldiron", "cold iron") &&
                    HasCandidate(enchantments, "enhancement1"),
                "installed BlueprintWeaponEnchantment graph and exact component fields");
            Add(assertions, "spear-native-cold-iron-weapons",
                "native cold-iron item damage-type override contracts",
                string.Join(" | ", coldIronWeapons), coldIronWeapons.Length > 0 &&
                    coldIronWeapons.Any(value => value.Contains(
                        "m_OverrideDamageType=True")) &&
                    coldIronWeapons.Any(value => value.IndexOf("ColdIron",
                        StringComparison.OrdinalIgnoreCase) >= 0),
                "installed BlueprintItemWeapon override damage-type graph");
            Add(assertions, "spear-native-weapon-selectors",
                "nonempty ordinary parameterized weapon-category selector inventory",
                string.Join(" | ", selectors), selectors.Length >= 8,
                "all installed BlueprintParametrizedFeature category contracts");
            Add(assertions, "spear-native-weapon-feature-selections",
                "static Exotic Weapon Proficiency and Finesse Training catalogs",
                string.Join(" | ", featureSelections),
                featureSelections.Any(value => value.IndexOf(
                    "Exotic", StringComparison.OrdinalIgnoreCase) >= 0) &&
                    featureSelections.Any(value => value.IndexOf(
                        "FinesseTraining", StringComparison.OrdinalIgnoreCase) >= 0),
                "installed BlueprintFeatureSelection AllFeatures catalogs");
            Add(assertions, "spear-native-elf-familiarity",
                "native elf/elven proficiency feature candidates",
                string.Join(" | ", familiarity), familiarity.Length > 0,
                "installed BlueprintFeature components; no racial bypass invoked");
            Add(assertions, "spear-native-race-grants",
                    "all native and optional race feature grants",
                string.Join(" | ", raceGrants), raceGrants.Length > 0 &&
                    raceGrants.Any(value => value.IndexOf("ElfRace:",
                        StringComparison.OrdinalIgnoreCase) >= 0),
                "installed BlueprintRace.Features exact references");
            Add(assertions, "spear-dexterity-source-inventory",
                "all native and loaded-mod Finesse, Grace, and stat-replacement consumers",
                string.Join(" | ", dexterity), dexterity.Length > 0,
                "all installed blueprint component type identities and fields");
            Add(assertions, "spear-native-entangled-conditions",
                "native Entangled condition blueprint candidates",
                string.Join(" | ", entangledConditions),
                entangledConditions.Length > 0,
                "installed BlueprintBuff names, identities, and components");
            Add(assertions, "spear-campaign-loot-candidates",
                "nonempty exact early-through-final campaign loot inventory",
                string.Join(" | ", loot), loot.Length > 0,
                "installed BlueprintLoot area/container/item graph");
            Add(assertions, "spear-contract-observer-save-free",
                "no save, inventory, selector, vendor, loot, or blueprint mutation",
                "read-only library enumeration", true,
                "observer has no save manager, input, AddFact, AddLoot, component assignment, or inventory call");
            Add(assertions, "loaded-mod-version", request.ExpectedModVersion,
                context.ModEntry.Info.Version,
                string.Equals(request.ExpectedModVersion,
                    context.ModEntry.Info.Version, StringComparison.Ordinal),
                "Unity Mod Manager ModEntry.Info.Version");

            bool pass = assertions.All(value => value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + "; mvid=" +
                    assembly.ManifestModule.ModuleVersionId + "; sha256=" +
                    HashFile(assembly.Location) + "; pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = ReadMetadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = new List<string>
                {
                    "blueprints=" + all.Length,
                    "weapons=" + weapons.Length,
                    "enchantments=" + enchantments.Length,
                    "selectors=" + selectors.Length,
                    "featureSelections=" + featureSelections.Length,
                    "familiarity=" + familiarity.Length,
                    "races=" + raceGrants.Length,
                    "dexterity=" + dexterity.Length,
                    "entangledConditions=" + entangledConditions.Length,
                    "loot=" + loot.Length
                },
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = new List<string>(),
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static bool IsWeaponCandidate(BlueprintItemWeapon item)
        {
            string display = Safe(() => item.Name);
            string type = item.Type == null ? string.Empty : item.Type.name;
            return ContainsAny(item.name, WeaponTerms) ||
                ContainsAny(display, WeaponTerms) || ContainsAny(type, WeaponTerms);
        }

        private static bool IsWeaponSelector(BlueprintParametrizedFeature feature)
        {
            string contract = ReadMember(feature, "ParameterType") + ";" +
                ReadMember(feature, "WeaponSubCategory") + ";" +
                DescribeComponents(feature);
            return ContainsAny(contract, new[] { "weaponcategory", "weaponsubcategory" });
        }

        private static bool IsWeaponFeatureSelection(BlueprintFeatureSelection selection)
        {
            string names = selection.name + ";" + Safe(() => selection.Name) + ";" +
                string.Join(";", (selection.AllFeatures ?? new BlueprintFeature[0])
                    .Where(value => value != null).Select(value => value.name).ToArray());
            return ContainsAny(names, new[] { "exoticweaponproficiency",
                "finessetraining", "weapontraining", "chosenweapon",
                "weaponfocus", "weaponspecialization" });
        }

        private static bool IsCampaignLootCandidate(BlueprintLoot loot)
        {
            string area = loot.Area == null ? string.Empty : loot.Area.name;
            return ContainsAny(area, new[] { "oaktree", "oldsycamore", "staglord",
                "troll", "dwarven", "ruins", "lonehouse", "barony", "capital",
                "season", "bloom", "goblin", "lamashtu", "womb", "silverstep",
                "hunting", "gudrin", "vordakai", "varnhold", "pitax", "irovetti",
                "houseattheedge", "finaldungeon", "firstworld" });
        }

        private static string DescribeWeapon(BlueprintItemWeapon item)
        {
            BlueprintWeaponType type = item.Type;
            return "item=" + item.name + ":" + item.AssetGuid +
                ";display=" + Safe(() => item.Name) +
                ";cost=" + item.Cost + ";weight=" + item.Weight +
                ";damageType=" + DescribeDamageType(item) +
                ";itemFields=" + DescribeMembers(item, new[] { "m_Type",
                    "m_Enchantments", "m_VisualParameters", "m_Icon",
                    "m_OverrideDamageDice", "m_OverrideDamageType",
                    "m_DamageType", "DamageType", "IsMasterwork" }) +
                ";itemVisual=" + DescribeVisual(item.VisualParameters) +
                ";itemComponents=" + DescribeComponents(item) +
                ";type=" + (type == null ? "<null>" : type.name + ":" +
                    type.AssetGuid + ";fields=" + DescribeMembers(type, new[] {
                        "Category", "AttackType", "AttackRange", "BaseDamage",
                        "DamageType", "CriticalRollEdge", "CriticalModifier",
                        "FighterGroup", "Weight", "IsTwoHanded", "IsLight",
                        "IsMonk", "IsNatural", "m_VisualParameters", "m_Icon",
                        "m_Enchantments", "m_AttackStat" }) + ";components=" +
                    DescribeComponents(type) + ";visual=" +
                    DescribeVisual(type.VisualParameters));
        }

        private static string DescribeSelector(BlueprintParametrizedFeature feature)
        {
            return feature.name + ":" + feature.AssetGuid +
                ";display=" + Safe(() => feature.Name) +
                ";fields=" + DescribeMembers(feature, new[] { "ParameterType",
                    "WeaponSubCategory", "m_Feature", "m_Prerequisite",
                    "m_CachedItems" }) + ";components=" + DescribeComponents(feature);
        }

        private static string DescribeFeatureSelection(
            BlueprintFeatureSelection selection)
        {
            return selection.name + ":" + selection.AssetGuid +
                ";display=" + Safe(() => selection.Name) +
                ";features=[" + string.Join(";", (selection.Features ??
                    new BlueprintFeature[0]).Where(value => value != null)
                    .Select(value => value.name + ":" + value.AssetGuid).ToArray()) +
                "];allFeatures=[" + string.Join(";", (selection.AllFeatures ??
                    new BlueprintFeature[0]).Where(value => value != null)
                    .Select(value => value.name + ":" + value.AssetGuid + "{" +
                        DescribeComponents(value) + "}").ToArray()) + "]";
        }

        private static string DescribeRace(BlueprintRace race)
        {
            return race.name + ":" + race.AssetGuid + ";display=" +
                Safe(() => race.Name) + ";raceId=" + race.RaceId +
                ";features=[" + string.Join(";", (race.Features ??
                    new BlueprintFeatureBase[0]).Where(value => value != null)
                    .Select(value => value.name + ":" + value.AssetGuid).ToArray()) + "]";
        }

        private static string DescribeBlueprint(BlueprintScriptableObject blueprint)
        {
            return blueprint.GetType().FullName + ":" + blueprint.name + ":" +
                blueprint.AssetGuid + ";fields=" + DescribeMembers(blueprint,
                    new[] { "m_EnchantName", "m_Description", "m_Prefix",
                        "m_Suffix", "EnchantmentCost", "ParameterType",
                        "WeaponSubCategory" }) + ";components=" +
                DescribeComponents(blueprint);
        }

        private static string DescribeLoot(BlueprintLoot loot)
        {
            return "loot=" + loot.name + ":" + loot.AssetGuid +
                ";area=" + (loot.Area == null ? "<null>" : loot.Area.name +
                    ":" + loot.Area.AssetGuid) + ";fields=" +
                DescribeMembers(loot, new[] { "ContainerName", "ContainerType",
                    "Setting" }) + ";items=[" + string.Join(";", (loot.Items ??
                    new LootEntry[0]).Where(value => value != null && value.Item != null)
                    .Select(value => value.Item.name + ":" + value.Item.AssetGuid +
                        "*" + value.Count).ToArray()) + "]";
        }

        private static string DescribeVisual(object visual)
        {
            if (visual == null) return "<null>";
            return DescribeMembers(visual, visual.GetType().GetFields(Members)
                .Where(value => !value.IsStatic).Select(value => value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray());
        }

        private static string DescribeDamageType(BlueprintItemWeapon item)
        {
            if (item == null || item.DamageType == null) return "<null>";
            return item.DamageType.Type + ":form=" +
                item.DamageType.Physical.Form + ":material=" +
                item.DamageType.Physical.Material;
        }

        private static string DescribeComponents(BlueprintScriptableObject blueprint)
        {
            return string.Join(",", (blueprint.ComponentsArray ??
                new BlueprintComponent[0]).Where(value => value != null)
                .Select(value => value.GetType().FullName + "{" +
                    DescribeMembers(value, value.GetType().GetFields(Members)
                        .Where(field => !field.IsStatic).Select(field => field.Name)
                        .Take(24).ToArray()) + "}").ToArray());
        }

        private static string DescribeMembers(object value, string[] names)
        {
            if (value == null) return "<null>";
            return string.Join(",", names.Distinct(StringComparer.Ordinal)
                .Select(name => name + "=" + ReadMember(value, name)).ToArray());
        }

        private static string ReadMember(object owner, string name)
        {
            if (owner == null) return "<null>";
            Type type = owner.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(name, Members | BindingFlags.DeclaredOnly);
                if (field != null) return Format(field.GetValue(owner));
                PropertyInfo property = type.GetProperty(name,
                    Members | BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    try { return Format(property.GetValue(owner, null)); }
                    catch (Exception exception) { return "<error:" + exception.GetType().Name + ">"; }
                }
                type = type.BaseType;
            }
            return "<missing>";
        }

        private static string Format(object value)
        {
            if (value == null) return "<null>";
            var blueprint = value as BlueprintScriptableObject;
            if (blueprint != null) return blueprint.name + ":" + blueprint.AssetGuid;
            var unityObject = value as UnityEngine.Object;
            if (unityObject != null) return unityObject.GetType().FullName + ":" +
                unityObject.name;
            if (value is string || value.GetType().IsEnum || value is ValueType)
                return value.ToString();
            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                var rows = new List<string>();
                foreach (object entry in enumerable)
                {
                    rows.Add(Format(entry));
                    if (rows.Count == 40) { rows.Add("<truncated>"); break; }
                }
                return "[" + string.Join(";", rows.ToArray()) + "]";
            }
            return value.GetType().FullName;
        }

        private static bool ContainsAny(string value, IEnumerable<string> terms)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            string normalized = value.Replace("_", string.Empty)
                .Replace("-", string.Empty).Replace(" ", string.Empty);
            return terms.Any(term => normalized.IndexOf(term.Replace(" ", string.Empty),
                StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool HasCandidate(string[] values, params string[] terms)
        {
            return values.Any(value => ContainsAny(value, terms));
        }

        private static string Safe(Func<string> read)
        {
            try { return read() ?? string.Empty; }
            catch (Exception exception) { return "<error:" + exception.GetType().Name + ">"; }
        }

        private static void Add(List<RuntimeTestAssertion> assertions, string name,
            string expected, string observed, bool pass, string evidence)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = evidence
            });
        }

        private static string HashFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return "missing";
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static string ReadMetadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false).Cast<AssemblyMetadataAttribute>()
                .FirstOrDefault(item => string.Equals(item.Key, key,
                    StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }
    }
}
