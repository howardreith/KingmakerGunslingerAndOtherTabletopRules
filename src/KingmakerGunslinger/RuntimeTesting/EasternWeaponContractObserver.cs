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
using Kingmaker.Blueprints.Root;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free, read-only inventory for the installed native contracts needed
    /// by Eastern Weapons. This observer deliberately records candidates before
    /// production category values, donors, enchantments, or campaign targets are
    /// selected.
    /// </summary>
    internal static class EasternWeaponContractObserver
    {
        private const BindingFlags Members = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly string[] WeaponTerms =
        {
            "kukri", "shortsword", "rapier", "scimitar", "longsword",
            "bastardsword", "falchion", "greatsword"
        };

        private static readonly string[] EnchantmentTerms =
        {
            "masterwork", "enhancement1", "enhancement2", "enhancement3",
            "enhancement4", "enhancement5", "flaming", "frost", "agile",
            "keen", "ghosttouch", "shock", "thundering", "holy",
            "brilliantenergy", "speed", "mightycleaving", "impact",
            "coldiron", "cold iron"
        };

        private static readonly string[] RuleTerms =
        {
            "proficien", "nonproficien", "bastardsword", "twohand",
            "onehand", "weaponcategory", "fightergroup", "weapontraining",
            "equipmentset", "criticalconfirm", "initiative", "powerattack",
            "newround", "forcedamage", "polymorph", "originalsize",
            "changesize", "weaponsize", "coupdegrace", "damagegrace",
            "damagestatreplacement", "attackstatreplacement"
        };

        private static readonly string[] CampaignTerms =
        {
            "oaktree", "oldsycamore", "staglord", "tradingpost", "act1",
            "capital", "hassuf", "jhod", "verdant", "silverstep", "season",
            "bloom", "varnhold", "vordakai", "pitax", "rushlight",
            "irovetti", "palace", "houseattheedge", "hateot", "finaldungeon",
            "firstworld", "tenebrous", "xelliren", "honestguy", "btsl"
        };

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");

            BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                .GetAllBlueprints().Where(value => value != null).Distinct().ToArray();
            BlueprintItemWeapon[] weaponBlueprints = all.OfType<BlueprintItemWeapon>()
                .ToArray();
            string[] weapons = weaponBlueprints.Where(IsWeaponCandidate)
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeWeapon).Take(240).ToArray();
            BlueprintWeaponEnchantment[] nativeEnchantments = all
                .OfType<BlueprintWeaponEnchantment>().Where(IsEnchantmentCandidate)
                .OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
            string[] enchantments = nativeEnchantments.Select(value =>
                DescribeEnchantment(value, weaponBlueprints)).Take(320).ToArray();
            string[] selectors = all.OfType<BlueprintParametrizedFeature>()
                .Where(value => ContainsAny(DescribeBlueprint(value), RuleTerms))
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeBlueprint).Take(240).ToArray();
            string[] selections = all.OfType<BlueprintFeatureSelection>()
                .Where(value => ContainsAny(DescribeSelection(value), new[] {
                    "exoticweaponproficiency", "martialweaponproficiency",
                    "finessetraining", "weapontraining", "weaponfocus",
                    "weaponspecialization", "chosenweapon", "weaponmastery" }))
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeSelection).Take(160).ToArray();
            string[] ruleBlueprints = all.Where(value =>
                    ContainsAny(value.name + ";" + DescribeComponents(value), RuleTerms))
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .Select(DescribeBlueprint).Take(500).ToArray();
            string[] ruleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeTypes).Where(value => value != null &&
                    value.FullName != null && value.FullName.StartsWith("Kingmaker",
                        StringComparison.Ordinal) &&
                    ContainsAny(value.FullName, RuleTerms))
                .OrderBy(value => value.FullName, StringComparer.Ordinal)
                .Select(DescribeType).Take(500).ToArray();

            BlueprintSharedVendorTable[] vendorTables = all
                .OfType<BlueprintSharedVendorTable>()
                .OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
            BlueprintScriptableObject[] campaignLoot = all.Where(value =>
                    IsCampaignCandidate(value) &&
                    (value is BlueprintLoot || value.GetType().Name.IndexOf("Loot",
                        StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(value => value.name, StringComparer.Ordinal).Take(700).ToArray();
            var referenceTargets = vendorTables.Cast<BlueprintScriptableObject>()
                .Concat(campaignLoot).Distinct().ToArray();
            Dictionary<string, List<string>> references = BuildReferenceIndex(all,
                referenceTargets);
            string[] vendors = vendorTables.Select(value => DescribeCampaignBlueprint(
                value, references)).Take(320).ToArray();
            string[] loot = campaignLoot.Select(value => DescribeCampaignBlueprint(
                value, references)).ToArray();

            var assertions = new List<RuntimeTestAssertion>();
            Add(assertions, "eastern-native-weapon-donors",
                "Kukri, Shortsword, Rapier, Scimitar, Longsword, Bastard Sword, Falchion, and Greatsword candidates",
                string.Join(" | ", weapons), WeaponTerms.All(term =>
                    HasCandidate(weapons, term)),
                "installed BlueprintItemWeapon and BlueprintWeaponType graph");
            Add(assertions, "eastern-native-enchantment-inventory",
                "native masterwork, +1 through +5, approved properties, material, and damage-size candidates",
                string.Join(" | ", enchantments), enchantments.Length >= 12 &&
                    HasCandidate(enchantments, "masterwork") &&
                    HasCandidate(enchantments, "enhancement1") &&
                    HasCandidate(enchantments, "enhancement5") &&
                    HasCandidate(enchantments, "keen") &&
                    HasCandidate(enchantments, "speed"),
                "installed BlueprintWeaponEnchantment identities, costs, components, and donor items");
            Add(assertions, "eastern-native-selector-inventory",
                "generic weapon-category selectors and static proficiency/training catalogs",
                "selectors=" + string.Join(" | ", selectors) +
                    ";selections=" + string.Join(" | ", selections),
                selectors.Length >= 8 && selections.Length > 0 &&
                    HasCandidate(selections, "exoticweaponproficiency"),
                "installed parameterized feature and AllFeatures contracts");
            Add(assertions, "eastern-native-rule-boundaries",
                "installed proficiency, grip, equipment, critical, Power Attack, size, polymorph, and damage rule candidates",
                "blueprints=" + string.Join(" | ", ruleBlueprints) +
                    ";types=" + string.Join(" | ", ruleTypes),
                ruleBlueprints.Length > 0 && ruleTypes.Length > 0 &&
                    HasCandidate(ruleTypes, "proficien") &&
                    HasCandidate(ruleTypes, "critical") &&
                    HasCandidate(ruleTypes, "polymorph", "size"),
                "loaded CLR type identities plus installed blueprint component fields");
            Add(assertions, "eastern-campaign-contract-inventory",
                "nonempty exact vendor and early-through-final campaign loot candidates with direct owners",
                "vendors=" + string.Join(" | ", vendors) +
                    ";loot=" + string.Join(" | ", loot),
                vendors.Length > 0 && loot.Length > 0 &&
                    HasCandidate(vendors, "7de959347266092448d8a72089ef9778"),
                "installed vendor-table, loot, area, component, and direct-reference graph");
            Add(assertions, "eastern-contract-observer-save-free",
                "no save, inventory, selector, vendor, loot, or blueprint mutation",
                "read-only installed-library and CLR-type enumeration", true,
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
                    "weaponCandidates=" + weapons.Length,
                    "enchantments=" + enchantments.Length,
                    "selectors=" + selectors.Length,
                    "selections=" + selections.Length,
                    "ruleBlueprints=" + ruleBlueprints.Length,
                    "ruleTypes=" + ruleTypes.Length,
                    "vendorTables=" + vendors.Length,
                    "campaignLoot=" + loot.Length
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

        private static bool IsEnchantmentCandidate(BlueprintWeaponEnchantment value)
        {
            return ContainsAny(value.name + ";" + Safe(() => value.Name) + ";" +
                DescribeComponents(value), EnchantmentTerms);
        }

        private static bool IsCampaignCandidate(BlueprintScriptableObject value)
        {
            var loot = value as BlueprintLoot;
            string area = loot == null || loot.Area == null ? string.Empty :
                loot.Area.name;
            return ContainsAny(value.name + ";" + area + ";" +
                DescribeComponents(value), CampaignTerms);
        }

        private static string DescribeWeapon(BlueprintItemWeapon item)
        {
            BlueprintWeaponType type = item.Type;
            return "item=" + item.name + ":" + item.AssetGuid +
                ";display=" + Safe(() => item.Name) +
                ";cost=" + item.Cost + ";weight=" + item.Weight +
                ";damage=" + DescribeDamageType(item) +
                ";itemFields=" + DescribeMembers(item, new[] { "m_Type",
                    "m_Enchantments", "m_VisualParameters", "m_Icon",
                    "m_OverrideDamageDice", "m_OverrideDamageType",
                    "m_DamageType", "DamageType", "IsMasterwork" }) +
                ";itemComponents=" + DescribeComponents(item) +
                ";type=" + (type == null ? "<null>" : type.name + ":" +
                    type.AssetGuid + ";fields=" + DescribeMembers(type, new[] {
                        "Category", "AttackType", "AttackRange", "BaseDamage",
                        "DamageType", "CriticalRollEdge", "CriticalModifier",
                        "FighterGroup", "Weight", "IsTwoHanded", "IsLight",
                        "IsFinessable", "IsReach", "m_VisualParameters",
                        "m_EquipmentEntity", "m_AnimationStyle", "m_SoundType" }) +
                    ";components=" + DescribeComponents(type));
        }

        private static string DescribeEnchantment(BlueprintWeaponEnchantment value,
            BlueprintItemWeapon[] weapons)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField("m_Enchantments",
                Members);
            string[] donors = field == null ? new string[0] : weapons.Where(item =>
                {
                    var entries = field.GetValue(item) as BlueprintWeaponEnchantment[];
                    return entries != null && entries.Any(entry =>
                        ReferenceEquals(entry, value));
                }).OrderBy(item => item.name, StringComparer.Ordinal).Take(20)
                .Select(item => item.name + ":" + item.AssetGuid).ToArray();
            return DescribeBlueprint(value) + ";donors=[" +
                string.Join(";", donors) + "]";
        }

        private static string DescribeSelection(BlueprintFeatureSelection value)
        {
            return value.name + ":" + value.AssetGuid + ";display=" +
                Safe(() => value.Name) + ";features=[" +
                string.Join(";", (value.Features ?? new BlueprintFeature[0])
                    .Where(feature => feature != null)
                    .Select(feature => feature.name + ":" + feature.AssetGuid)
                    .ToArray()) + "];allFeatures=[" +
                string.Join(";", (value.AllFeatures ?? new BlueprintFeature[0])
                    .Where(feature => feature != null)
                    .Select(feature => feature.name + ":" + feature.AssetGuid +
                        "{" + DescribeComponents(feature) + "}").ToArray()) + "]";
        }

        private static string DescribeCampaignBlueprint(
            BlueprintScriptableObject value,
            Dictionary<string, List<string>> references)
        {
            List<string> owners;
            string[] direct = references.TryGetValue(value.AssetGuid, out owners)
                ? owners.Take(30).ToArray() : new string[0];
            var loot = value as BlueprintLoot;
            string details = loot == null ? string.Empty : ";area=" +
                (loot.Area == null ? "<null>" : loot.Area.name + ":" +
                    loot.Area.AssetGuid) + ";container=" + loot.ContainerName +
                ";setting=" + loot.Setting + ";items=[" + string.Join(";",
                    (loot.Items ?? new LootEntry[0]).Where(entry => entry != null)
                    .Select(entry => (entry.Item == null ? "<null>" :
                        entry.Item.name + ":" + entry.Item.AssetGuid) + "*" +
                        entry.Count).ToArray()) + "]";
            return value.GetType().FullName + ":" + value.name + ":" +
                value.AssetGuid + details + ";components=" +
                DescribeComponents(value) + ";directOwners=" + direct.Length +
                "[" + string.Join(";", direct) + "]";
        }

        private static Dictionary<string, List<string>> BuildReferenceIndex(
            BlueprintScriptableObject[] owners, BlueprintScriptableObject[] targets)
        {
            var targetGuids = new HashSet<string>(targets.Select(value =>
                value.AssetGuid), StringComparer.Ordinal);
            var result = targetGuids.ToDictionary(value => value,
                value => new List<string>(), StringComparer.Ordinal);
            foreach (BlueprintScriptableObject owner in owners)
            {
                var found = new HashSet<string>(StringComparer.Ordinal);
                FindReferences(owner, targetGuids, found);
                foreach (BlueprintComponent component in owner.ComponentsArray ??
                    new BlueprintComponent[0])
                    FindReferences(component, targetGuids, found);
                foreach (string guid in found)
                    result[guid].Add(owner.GetType().FullName + ":" + owner.name +
                        ":" + owner.AssetGuid);
            }
            return result;
        }

        private static void FindReferences(object owner,
            HashSet<string> targets, HashSet<string> found)
        {
            if (owner == null) return;
            for (Type type = owner.GetType(); type != null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(Members |
                    BindingFlags.DeclaredOnly))
                {
                    object value;
                    try { value = field.GetValue(owner); }
                    catch { continue; }
                    var direct = value as BlueprintScriptableObject;
                    if (direct != null && targets.Contains(direct.AssetGuid))
                        found.Add(direct.AssetGuid);
                    var array = value as Array;
                    if (array == null) continue;
                    foreach (object entry in array)
                    {
                        var referenced = entry as BlueprintScriptableObject;
                        if (referenced != null && targets.Contains(
                            referenced.AssetGuid)) found.Add(referenced.AssetGuid);
                    }
                }
            }
        }

        private static string DescribeBlueprint(BlueprintScriptableObject value)
        {
            return value.GetType().FullName + ":" + value.name + ":" +
                value.AssetGuid + ";fields=" + DescribeMembers(value, new[] {
                    "m_EnchantName", "m_Description", "m_Prefix", "m_Suffix",
                    "EnchantmentCost", "ParameterType", "WeaponSubCategory",
                    "m_Group", "Group", "m_WeaponCategory" }) +
                ";components=" + DescribeComponents(value);
        }

        private static string DescribeType(Type type)
        {
            return type.FullName + ";base=" +
                (type.BaseType == null ? "<null>" : type.BaseType.FullName) +
                ";interfaces=[" + string.Join(";", type.GetInterfaces()
                    .Select(value => value.FullName).OrderBy(value => value,
                        StringComparer.Ordinal).ToArray()) + "];members=[" +
                string.Join(";", type.GetMembers(BindingFlags.Instance |
                    BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Select(value => value.MemberType + ":" + value.Name)
                    .OrderBy(value => value, StringComparer.Ordinal).Take(80)
                    .ToArray()) + "]";
        }

        private static IEnumerable<Type> SafeTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(value => value != null);
            }
            catch { return new Type[0]; }
        }

        private static string DescribeComponents(BlueprintScriptableObject value)
        {
            return string.Join(",", (value.ComponentsArray ??
                new BlueprintComponent[0]).Where(component => component != null)
                .Select(component => component.GetType().FullName + "{" +
                    DescribeMembers(component, component.GetType().GetFields(Members)
                        .Where(field => !field.IsStatic).Select(field => field.Name)
                        .Take(30).ToArray()) + "}").ToArray());
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
            for (Type type = owner.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Members |
                    BindingFlags.DeclaredOnly);
                if (field != null) return Format(field.GetValue(owner));
                PropertyInfo property = type.GetProperty(name, Members |
                    BindingFlags.DeclaredOnly);
                if (property == null || property.GetIndexParameters().Length != 0)
                    continue;
                try { return Format(property.GetValue(owner, null)); }
                catch (Exception exception)
                {
                    return "<error:" + exception.GetType().Name + ">";
                }
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
            if (enumerable == null) return value.GetType().FullName;
            var rows = new List<string>();
            foreach (object entry in enumerable)
            {
                rows.Add(Format(entry));
                if (rows.Count == 40) { rows.Add("<truncated>"); break; }
            }
            return "[" + string.Join(";", rows.ToArray()) + "]";
        }

        private static string DescribeDamageType(BlueprintItemWeapon item)
        {
            if (item == null || item.DamageType == null) return "<null>";
            return item.DamageType.Type + ":form=" +
                item.DamageType.Physical.Form + ":material=" +
                item.DamageType.Physical.Material;
        }

        private static bool ContainsAny(string value, IEnumerable<string> terms)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            string normalized = value.Replace("_", string.Empty)
                .Replace("-", string.Empty).Replace(" ", string.Empty);
            return terms.Any(term => normalized.IndexOf(term.Replace(" ",
                string.Empty), StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool HasCandidate(string[] values, params string[] terms)
        {
            return values.Any(value => ContainsAny(value, terms));
        }

        private static string Safe(Func<string> read)
        {
            try { return read() ?? string.Empty; }
            catch (Exception exception)
            {
                return "<error:" + exception.GetType().Name + ">";
            }
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
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static string ReadMetadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    string.Equals(item.Key, key, StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }
    }
}
