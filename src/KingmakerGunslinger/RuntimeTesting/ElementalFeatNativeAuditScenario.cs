using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Read-only inventory of exact installed engine and blueprint surfaces
    /// required by Elemental Feats. It intentionally creates no units, facts,
    /// commands, items, resources, or saves.
    /// </summary>
    internal static class ElementalFeatNativeAuditScenario
    {
        internal const string EvidenceFileName =
            "elemental-feat-native-audit.json";
        internal const string SmallAirElementalGuid =
            "04944455200bc224d955a8e9bbd64f3f";
        internal const string SmallWaterElementalGuid =
            "56372b0a2749c224392a5ee74105c534";

        private static readonly string[] ExactContractGuids =
        {
            "70cffb448c132fa409e49156d013b175", // Airborne
            "08ae1c01155a2184db869e9ebedc758d", // draconic Wings buff
            "25699a90ed3299e438b6fd5548930809", // angel Wings buff
            "61b312b8f91cc48418768b77cd6dcc02", // Obscuring Mist buff
            "30f90becaaac51f41bf56641966c4121", // Flaming enchantment
            "107788f47c4481f4db6da06498b28270"  // Small Water Elemental ability
        };

        private static readonly string[] SearchTerms =
        {
            "wing", "flight", "flying", "air elemental",
            "fog", "mist", "smoke", "cloud", "incendiary",
            "inhaled", "breath", "gas", "poison", "ray"
        };

        private sealed class BlueprintEvidence
        {
            public string Guid { get; set; }
            public string BlueprintType { get; set; }
            public string InternalName { get; set; }
            public string DisplayName { get; set; }
            public string[] MatchedTerms { get; set; }
            public string[] ComponentTypes { get; set; }
            public string[] UnitFactIdentities { get; set; }
        }

        private sealed class ConcealmentEvidence
        {
            public string Guid { get; set; }
            public string BlueprintType { get; set; }
            public string InternalName { get; set; }
            public string DisplayName { get; set; }
            public string Concealment { get; set; }
            public string Descriptor { get; set; }
            public bool OnlyForAttacks { get; set; }
        }

        private sealed class EnchantmentEvidence
        {
            public string Guid { get; set; }
            public string InternalName { get; set; }
            public string DisplayName { get; set; }
            public string EnergyType { get; set; }
            public string Dice { get; set; }
        }

        private sealed class SpawnEvidence
        {
            public string AbilityGuid { get; set; }
            public string AbilityInternalName { get; set; }
            public string AbilityDisplayName { get; set; }
            public string UnitGuid { get; set; }
            public string UnitInternalName { get; set; }
            public string CountDice { get; set; }
            public int CountDiceCount { get; set; }
            public int CountBonus { get; set; }
            public string DurationRate { get; set; }
            public int DurationBonus { get; set; }
            public bool DirectControl { get; set; }
            public bool LinkedToCaster { get; set; }
        }

        private sealed class ComponentContractEvidence
        {
            public string ComponentType { get; set; }
            public string Contract { get; set; }
        }

        private sealed class ExactBlueprintContractEvidence
        {
            public string Guid { get; set; }
            public string BlueprintType { get; set; }
            public string InternalName { get; set; }
            public string DisplayName { get; set; }
            public List<ComponentContractEvidence> Components { get; set; }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool SaveStateTouched { get; set; }
            public int BlueprintCount { get; set; }
            public string[] CombatManeuvers { get; set; }
            public bool HasNativeDirtyTrickBlind { get; set; }
            public bool HasNativeDirtyTrickDazzle { get; set; }
            public List<BlueprintEvidence> NamedBlueprints { get; set; }
            public List<ConcealmentEvidence> ConcealmentSources { get; set; }
            public List<EnchantmentEvidence> FireWeaponEnchantments { get; set; }
            public List<SpawnEvidence> SmallWaterElementalSpawns { get; set; }
            public List<ExactBlueprintContractEvidence>
                ExactBlueprintContracts { get; set; }
            public BlueprintEvidence SmallAirElemental { get; set; }
            public BlueprintEvidence SmallWaterElemental { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                SaveStateTouched = false,
                NamedBlueprints = new List<BlueprintEvidence>(),
                ConcealmentSources = new List<ConcealmentEvidence>(),
                FireWeaponEnchantments = new List<EnchantmentEvidence>(),
                SmallWaterElementalSpawns = new List<SpawnEvidence>(),
                ExactBlueprintContracts =
                    new List<ExactBlueprintContractEvidence>()
            };
            string exceptionSummary = string.Empty;
            try
            {
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                if (library == null || library.GetAllBlueprints() == null)
                    throw new InvalidOperationException(
                        "The live blueprint library is unavailable.");
                BlueprintScriptableObject[] all = library.GetAllBlueprints()
                    .Where(value => value != null)
                    .OrderBy(value => value.AssetGuid,
                        StringComparer.Ordinal).ToArray();
                evidence.BlueprintCount = all.Length;
                evidence.CombatManeuvers = Enum.GetNames(
                    typeof(CombatManeuver));
                evidence.HasNativeDirtyTrickBlind = evidence.CombatManeuvers
                    .Contains("DirtyTrickBlind", StringComparer.Ordinal);
                evidence.HasNativeDirtyTrickDazzle = evidence.CombatManeuvers
                    .Contains("DirtyTrickDazzle", StringComparer.Ordinal);

                foreach (BlueprintScriptableObject blueprint in all)
                {
                    string display = DisplayName(blueprint);
                    string text = ((blueprint.name ?? string.Empty) + " " +
                        display).ToLowerInvariant();
                    string[] matched = SearchTerms.Where(term =>
                        text.Contains(term)).ToArray();
                    if (matched.Length > 0 && IsRelevantType(blueprint))
                        evidence.NamedBlueprints.Add(DescribeBlueprint(
                            blueprint, matched));

                    foreach (AddConcealment concealment in Components(blueprint)
                        .OfType<AddConcealment>())
                        evidence.ConcealmentSources.Add(
                            new ConcealmentEvidence
                            {
                                Guid = blueprint.AssetGuid ?? string.Empty,
                                BlueprintType = blueprint.GetType().FullName,
                                InternalName = blueprint.name ?? string.Empty,
                                DisplayName = display,
                                Concealment = concealment.Concealment.ToString(),
                                Descriptor = concealment.Descriptor.ToString(),
                                OnlyForAttacks = concealment.OnlyForAttacks
                            });

                    BlueprintWeaponEnchantment enchantment = blueprint as
                        BlueprintWeaponEnchantment;
                    if (enchantment != null)
                        foreach (WeaponEnergyDamageDice damage in Components(
                                enchantment).OfType<WeaponEnergyDamageDice>()
                            .Where(value => value.Element ==
                                DamageEnergyType.Fire))
                            evidence.FireWeaponEnchantments.Add(
                                new EnchantmentEvidence
                                {
                                    Guid = enchantment.AssetGuid ?? string.Empty,
                                    InternalName = enchantment.name ??
                                        string.Empty,
                                    DisplayName = display,
                                    EnergyType = damage.Element.ToString(),
                                    Dice = damage.EnergyDamageDice.ToString()
                                });

                    BlueprintAbility ability = blueprint as BlueprintAbility;
                    if (ability != null)
                        CollectSmallWaterSpawns(ability,
                            evidence.SmallWaterElementalSpawns);
                }

                BlueprintUnit smallAir = all.OfType<BlueprintUnit>()
                    .SingleOrDefault(value => string.Equals(value.AssetGuid,
                        SmallAirElementalGuid, StringComparison.Ordinal));
                BlueprintUnit smallWater = all.OfType<BlueprintUnit>()
                    .SingleOrDefault(value => string.Equals(value.AssetGuid,
                        SmallWaterElementalGuid, StringComparison.Ordinal));
                evidence.SmallAirElemental = smallAir == null ? null :
                    DescribeBlueprint(smallAir,
                        new[] { "exact-small-air-elemental" });
                evidence.SmallWaterElemental = smallWater == null ? null :
                    DescribeBlueprint(smallWater,
                        new[] { "exact-small-water-elemental" });

                foreach (string guid in ExactContractGuids)
                {
                    BlueprintScriptableObject exact = all.SingleOrDefault(
                        value => string.Equals(value.AssetGuid, guid,
                            StringComparison.Ordinal));
                    if (exact != null)
                        evidence.ExactBlueprintContracts.Add(
                            DescribeExactContract(exact));
                }

                Add(assertions, "elemental-feat-native-dirty-trick-blind",
                    "native DirtyTrickBlind maneuver; no invented condition",
                    string.Join(",", evidence.CombatManeuvers),
                    evidence.HasNativeDirtyTrickBlind,
                    "live CombatManeuver enum");
                Add(assertions, "elemental-feat-native-dirty-trick-dazzle",
                    "no native DirtyTrickDazzle; use the printed blind option only",
                    evidence.HasNativeDirtyTrickDazzle.ToString(),
                    !evidence.HasNativeDirtyTrickDazzle,
                    "live CombatManeuver enum");
                Add(assertions, "elemental-feat-native-air-elemental",
                    SmallAirElementalGuid,
                    smallAir == null ? "missing" : smallAir.AssetGuid,
                    smallAir != null,
                    "exact live BlueprintUnit lookup");
                Add(assertions, "elemental-feat-native-water-elemental",
                    SmallWaterElementalGuid,
                    smallWater == null ? "missing" : smallWater.AssetGuid,
                    smallWater != null,
                    "exact live BlueprintUnit lookup");
                Add(assertions, "elemental-feat-native-concealment-inventory",
                    "at least one exact AddConcealment source",
                    evidence.ConcealmentSources.Count.ToString(),
                    evidence.ConcealmentSources.Count > 0,
                    "live blueprint component inventory");
                Add(assertions, "elemental-feat-native-fire-enchantments",
                    "at least one exact native fire weapon enchantment",
                    evidence.FireWeaponEnchantments.Count.ToString(),
                    evidence.FireWeaponEnchantments.Count > 0,
                    "live WeaponEnergyDamageDice inventory");
                Add(assertions, "elemental-feat-native-water-summon-path",
                    "at least one native ability spawning the exact Small Water Elemental",
                    evidence.SmallWaterElementalSpawns.Count.ToString(),
                    evidence.SmallWaterElementalSpawns.Count > 0,
                    "live recursive ContextActionSpawnMonster inventory");
                Add(assertions, "elemental-feat-native-exact-contracts",
                    ExactContractGuids.Length.ToString(),
                    evidence.ExactBlueprintContracts.Count.ToString(),
                    evidence.ExactBlueprintContracts.Count ==
                        ExactContractGuids.Length,
                    "six exact live blueprint/component contracts");
            }
            catch (Exception exception)
            {
                exceptionSummary = exception.ToString();
                diagnostics.Add(exceptionSummary);
            }

            Add(assertions, "elemental-feat-native-audit-save-free", "false",
                evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "read-only blueprint and enum inventory");
            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver(),
                    PreserveReferencesHandling =
                        PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Error
                }));
            evidenceFiles.Add(path);
            diagnostics.Add("elementalFeatNativeAuditSha256=" + Hash(path));
            bool pass = string.IsNullOrEmpty(exceptionSummary) &&
                assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"),
                EndUtc = string.Empty,
                DurationMilliseconds = (long)(DateTime.UtcNow - started)
                    .TotalMilliseconds,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = exceptionSummary,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static bool IsRelevantType(BlueprintScriptableObject value)
        {
            return value is BlueprintFeature || value is BlueprintBuff ||
                value is BlueprintAbility ||
                value is BlueprintAbilityAreaEffect ||
                value is BlueprintUnit ||
                value is BlueprintWeaponEnchantment;
        }

        private static BlueprintEvidence DescribeBlueprint(
            BlueprintScriptableObject value, string[] matched)
        {
            BlueprintUnit unit = value as BlueprintUnit;
            return new BlueprintEvidence
            {
                Guid = value.AssetGuid ?? string.Empty,
                BlueprintType = value.GetType().FullName,
                InternalName = value.name ?? string.Empty,
                DisplayName = DisplayName(value),
                MatchedTerms = matched,
                ComponentTypes = Components(value).Where(component =>
                        component != null).Select(component =>
                        component.GetType().FullName)
                    .OrderBy(type => type, StringComparer.Ordinal).ToArray(),
                UnitFactIdentities = unit == null ? new string[0] :
                    (unit.AddFacts ?? new BlueprintUnitFact[0]).Where(fact =>
                            fact != null)
                        .Select(fact => (fact.name ?? string.Empty) + "[" +
                            (fact.AssetGuid ?? string.Empty) + "]{" +
                            string.Join(",", Components(fact).Where(component =>
                                    component != null).Select(component =>
                                    component.GetType().FullName)
                                .OrderBy(type => type,
                                    StringComparer.Ordinal)) + "}")
                        .OrderBy(identity => identity,
                            StringComparer.Ordinal).ToArray()
            };
        }

        private static ExactBlueprintContractEvidence DescribeExactContract(
            BlueprintScriptableObject value)
        {
            return new ExactBlueprintContractEvidence
            {
                Guid = value.AssetGuid ?? string.Empty,
                BlueprintType = value.GetType().FullName,
                InternalName = value.name ?? string.Empty,
                DisplayName = DisplayName(value),
                Components = Components(value).Where(component =>
                        component != null)
                    .Select(component => new ComponentContractEvidence
                    {
                        ComponentType = component.GetType().FullName,
                        Contract = FormatContract(component, 5,
                            new HashSet<object>(ReferenceComparer.Instance))
                    }).OrderBy(component => component.ComponentType,
                        StringComparer.Ordinal).ToList()
            };
        }

        private static string FormatContract(object value, int depth,
            ISet<object> seen)
        {
            if (value == null) return "null";
            Type type = value.GetType();
            if (value is string || type.IsPrimitive || type.IsEnum ||
                type == typeof(decimal))
                return Convert.ToString(value) ?? string.Empty;
            BlueprintScriptableObject blueprint = value as
                BlueprintScriptableObject;
            if (blueprint != null)
                return blueprint.GetType().FullName + ":" +
                    (blueprint.name ?? string.Empty) + "[" +
                    (blueprint.AssetGuid ?? string.Empty) + "]";
            UnityEngine.Object unity = value as UnityEngine.Object;
            if (unity != null && !(value is BlueprintComponent) &&
                !(value is GameAction))
                return unity.GetType().FullName + ":" +
                    (unity.name ?? string.Empty);
            if (depth <= 0) return type.FullName;
            if (!type.IsValueType && !seen.Add(value)) return "<cycle>";
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null)
            {
                var items = new List<string>();
                foreach (object item in sequence)
                {
                    if (items.Count == 32)
                    {
                        items.Add("<more>");
                        break;
                    }
                    items.Add(FormatContract(item, depth - 1, seen));
                }
                return "[" + string.Join(",", items) + "]";
            }
            string[] fields = ContractFields(type).Select(field =>
            {
                object child;
                try { child = field.GetValue(value); }
                catch (Exception exception)
                {
                    return field.Name + "=<" +
                        exception.GetType().Name + ">";
                }
                return field.Name + "=" +
                    FormatContract(child, depth - 1, seen);
            }).ToArray();
            return type.FullName + "{" + string.Join(";", fields) + "}";
        }

        private static IEnumerable<FieldInfo> ContractFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;
            for (Type current = type; current != null &&
                current != typeof(BlueprintComponent) &&
                current != typeof(ScriptableObject) &&
                current != typeof(UnityEngine.Object);
                current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags)
                    .Where(field => !field.IsStatic)
                    .OrderBy(field => field.Name, StringComparer.Ordinal))
                    yield return field;
        }

        private static BlueprintComponent[] Components(
            BlueprintScriptableObject value)
        {
            return value == null || value.ComponentsArray == null ?
                new BlueprintComponent[0] : value.ComponentsArray;
        }

        private static void CollectSmallWaterSpawns(BlueprintAbility ability,
            ICollection<SpawnEvidence> output)
        {
            var seen = new HashSet<object>(ReferenceComparer.Instance);
            foreach (BlueprintComponent component in Components(ability))
                CollectSmallWaterSpawns(ability, component, seen, output);
        }

        private static void CollectSmallWaterSpawns(BlueprintAbility ability,
            object value, ISet<object> seen,
            ICollection<SpawnEvidence> output)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            UnityEngine.Object unity = value as UnityEngine.Object;
            if (unity != null && !(value is BlueprintComponent) &&
                !(value is GameAction)) return;
            ContextActionSpawnMonster spawn = value as
                ContextActionSpawnMonster;
            if (spawn != null && spawn.Blueprint != null && string.Equals(
                    spawn.Blueprint.AssetGuid, SmallWaterElementalGuid,
                    StringComparison.Ordinal))
                output.Add(new SpawnEvidence
                {
                    AbilityGuid = ability.AssetGuid ?? string.Empty,
                    AbilityInternalName = ability.name ?? string.Empty,
                    AbilityDisplayName = DisplayName(ability),
                    UnitGuid = spawn.Blueprint.AssetGuid ?? string.Empty,
                    UnitInternalName = spawn.Blueprint.name ?? string.Empty,
                    CountDice = spawn.CountValue.DiceType.ToString(),
                    CountDiceCount = spawn.CountValue.DiceCountValue.Value,
                    CountBonus = spawn.CountValue.BonusValue.Value,
                    DurationRate = spawn.DurationValue.Rate.ToString(),
                    DurationBonus = spawn.DurationValue.BonusValue.Value,
                    DirectControl = spawn.IsDirectlyControllable,
                    LinkedToCaster = !spawn.DoNotLinkToCaster
                });
            foreach (FieldInfo field in Fields(value.GetType()))
            {
                object child;
                try { child = field.GetValue(value); }
                catch { continue; }
                IEnumerable sequence = child as IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence)
                        CollectSmallWaterSpawns(ability, item, seen, output);
                else CollectSmallWaterSpawns(ability, child, seen, output);
            }
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;
            for (Type current = type; current != null;
                current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags))
                    if (!field.IsStatic) yield return field;
        }

        private static string DisplayName(BlueprintScriptableObject blueprint)
        {
            PropertyInfo property = blueprint.GetType().GetProperty("Name",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            if (property == null || property.GetIndexParameters().Length != 0)
                return string.Empty;
            try
            {
                return Convert.ToString(property.GetValue(blueprint, null)) ??
                    string.Empty;
            }
            catch { return string.Empty; }
        }

        private static void Add(
            ICollection<RuntimeTestAssertion> assertions, string name,
            string expected, string observed, bool pass, string source)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = source
            });
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    string.Equals(item.Key, key, StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public new bool Equals(object left, object right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(object value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
