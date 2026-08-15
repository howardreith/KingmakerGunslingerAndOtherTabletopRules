using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurCastEngineContractObserver
    {
        private const string FileName = "brown-fur-cast-engine-contract.json";
        private const BindingFlags All = BindingFlags.Public |
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("cotwAssembly", Order = 1)]
            public string CotwAssembly { get; set; }
            [JsonProperty("unitUseAbility", Order = 2)]
            public List<string> UnitUseAbility { get; set; }
            [JsonProperty("abilityData", Order = 3)]
            public List<string> AbilityData { get; set; }
            [JsonProperty("abilityParams", Order = 4)]
            public List<string> AbilityParams { get; set; }
            [JsonProperty("abilityExecutionContext", Order = 5)]
            public List<string> AbilityExecutionContext { get; set; }
            [JsonProperty("ruleCastSpell", Order = 6)]
            public List<string> RuleCastSpell { get; set; }
            [JsonProperty("spellbook", Order = 7)]
            public List<string> Spellbook { get; set; }
            [JsonProperty("modifiableValue", Order = 8)]
            public List<string> ModifiableValue { get; set; }
            [JsonProperty("modifier", Order = 9)]
            public List<string> Modifier { get; set; }
            [JsonProperty("modifierSourceFact", Order = 10)]
            public List<string> ModifierSourceFact { get; set; }
            [JsonProperty("modifierSourceBuff", Order = 11)]
            public List<string> ModifierSourceBuff { get; set; }
            [JsonProperty("abilityBonusCarriers", Order = 12)]
            public List<string> AbilityBonusCarriers { get; set; }
            [JsonProperty("sharedSpells", Order = 13)]
            public List<string> SharedSpells { get; set; }
            [JsonProperty("sharedSpellsBodies", Order = 14)]
            public List<string> SharedSpellsBodies { get; set; }
            [JsonProperty("directSharedSpellsHarmony", Order = 15)]
            public List<string> SharedSpellsHarmony { get; set; }
            [JsonProperty("relevantCotwHarmony", Order = 16)]
            public List<string> RelevantCotwHarmony { get; set; }
            [JsonProperty("relevantCotwTargetingBodies", Order = 17)]
            public List<string> RelevantCotwTargetingBodies { get; set; }
            [JsonProperty("nativeDeliveryBodies", Order = 18)]
            public List<string> NativeDeliveryBodies { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            CotwArcanistResolution resolution =
                BrownFurOptionalExtensionCoordinator.Current;
            CotwArcanistContract contract = resolution == null ? null :
                resolution.Contract;
            Type shared = contract == null || contract.Assembly == null ? null :
                contract.Assembly.GetType("CallOfTheWild.SharedSpells", false,
                    false);
            var evidence = new Evidence {
                CotwAssembly = contract == null || contract.Assembly == null ?
                    string.Empty : contract.Assembly.FullName,
                UnitUseAbility = Describe(typeof(UnitUseAbility)),
                AbilityData = Describe(typeof(AbilityData)),
                AbilityParams = Describe(typeof(AbilityParams)),
                AbilityExecutionContext = Describe(typeof(AbilityExecutionContext)),
                RuleCastSpell = Describe(typeof(RuleCastSpell)),
                Spellbook = Describe(typeof(Spellbook)),
                ModifiableValue = Describe(typeof(ModifiableValue)),
                Modifier = Describe(typeof(ModifiableValue.Modifier)),
                ModifierSourceFact = Describe(EngineType(
                    "Kingmaker.Blueprints.Facts.Fact")),
                ModifierSourceBuff = Describe(EngineType(
                    "Kingmaker.UnitLogic.Buffs.Buff")),
                AbilityBonusCarriers = DescribeTypes(new[] {
                    "Kingmaker.UnitLogic.FactLogic.AddStatBonus",
                    "Kingmaker.UnitLogic.FactLogic.AddContextStatBonus",
                    "Kingmaker.UnitLogic.Buffs.Components.AddGenericStatBonus",
                    "Kingmaker.Designers.Mechanics.Buffs.AddStatBonusAbilityValue",
                    "Kingmaker.UnitLogic.Buffs.Polymorph",
                    "Kingmaker.Designers.Mechanics.Buffs.ChangeUnitSize"
                }),
                SharedSpells = Describe(shared),
                SharedSpellsBodies = DescribeSharedSpellsBodies(contract),
                SharedSpellsHarmony = new List<string>(),
                RelevantCotwHarmony = new List<string>(),
                RelevantCotwTargetingBodies = new List<string>(),
                NativeDeliveryBodies = DescribeNativeDeliveryBodies()
            };
            ObserveHarmony(context, evidence);

            Add(assertions, "cast-engine-cotw-contract", "compatible",
                resolution == null ? "missing" :
                    resolution.Decision.Availability.ToString(),
                resolution != null && resolution.Decision.IsCompatible &&
                    contract != null && shared != null,
                "isolated structural CotW contract and exact SharedSpells type");
            Add(assertions, "cast-engine-command-lifecycle",
                "UnitUseAbility constructor, OnAction, and OnEnded",
                JoinMatches(evidence.UnitUseAbility, ".ctor", "OnAction",
                    "OnEnded"),
                Has(evidence.UnitUseAbility, ".ctor") &&
                    Has(evidence.UnitUseAbility, "OnAction") &&
                    Has(evidence.UnitUseAbility, "OnEnded"),
                "read-only reflection of the native command execution boundary");
            Add(assertions, "cast-engine-canonicalization",
                "AbilityData Blueprint, Spellbook, ConvertedFrom, and SpellLevel",
                JoinMatches(evidence.AbilityData, "Blueprint", "Spellbook",
                    "ConvertedFrom", "SpellLevel"),
                Has(evidence.AbilityData, "Blueprint") &&
                    Has(evidence.AbilityData, "Spellbook") &&
                    Has(evidence.AbilityData, "ConvertedFrom") &&
                    Has(evidence.AbilityData, "SpellLevel"),
                "per-cast source and conversion-chain surfaces");
            Add(assertions, "cast-engine-rule-commit", "RuleCastSpell.OnTrigger",
                JoinMatches(evidence.RuleCastSpell, "OnTrigger", "Spell",
                    "Success"),
                Has(evidence.RuleCastSpell, "OnTrigger") &&
                    Has(evidence.RuleCastSpell, "Spell"),
                "native cast rule commit and result surfaces");
            Add(assertions, "cast-engine-slot-accounting",
                "Spellbook CanSpend and Spend surfaces",
                JoinMatches(evidence.Spellbook, "CanSpend", "Spend"),
                Has(evidence.Spellbook, "CanSpend") &&
                    Has(evidence.Spellbook, "Spend"),
                "native slot validation and expenditure methods");
            Add(assertions, "cast-engine-modifier-registration",
                "AddModifier plus mutable descriptor-preserving modifier provenance",
                JoinMatches(evidence.ModifiableValue, "AddModifier", ".Type",
                    ".Owner") + "|" + JoinMatches(evidence.Modifier,
                    "ModValue", "ModDescriptor", "Source", "SourceComponent",
                    "AppliedTo"),
                Has(evidence.ModifiableValue, "AddModifier") &&
                    Has(evidence.ModifiableValue, ".Type") &&
                    Has(evidence.ModifiableValue, ".Owner") &&
                    Has(evidence.Modifier, "ModValue") &&
                    Has(evidence.Modifier, "ModDescriptor") &&
                    Has(evidence.Modifier, "Source") &&
                    Has(evidence.Modifier, "SourceComponent") &&
                    Has(evidence.Modifier, "AppliedTo"),
                "execution-scoped interception can alter only ModValue while " +
                "retaining the original descriptor and exact source fact");
            Add(assertions, "cast-engine-modifier-source-provenance",
                "source Fact Blueprint/MaybeContext and Buff Blueprint/Context",
                JoinMatches(evidence.ModifierSourceFact, "Blueprint",
                    "MaybeContext") + "|" + JoinMatches(
                    evidence.ModifierSourceBuff, "Blueprint", "Context",
                    "IsFromSpell"),
                Has(evidence.ModifierSourceFact, "Blueprint") &&
                    Has(evidence.ModifierSourceFact, "MaybeContext") &&
                    Has(evidence.ModifierSourceBuff, "Blueprint") &&
                    Has(evidence.ModifierSourceBuff, "Context") &&
                    Has(evidence.ModifierSourceBuff, "IsFromSpell"),
                "modifier ownership can be cross-checked against the applied " +
                "spell buff and its execution context");
            Add(assertions, "cast-engine-ability-bonus-carriers",
                "six installed carrier families with stat/value/activation surfaces",
                "types=" + CountTypes(evidence.AbilityBonusCarriers) + ";" +
                    JoinMatches(evidence.AbilityBonusCarriers, ".Stat", ".Value",
                        "Bonus", "OnTurnOn", "OnFactActivate"),
                CountTypes(evidence.AbilityBonusCarriers) == 6 &&
                    Has(evidence.AbilityBonusCarriers, ".Stat") &&
                    Has(evidence.AbilityBonusCarriers, "OnTurnOn") &&
                    Has(evidence.AbilityBonusCarriers, "OnFactActivate"),
                "authoritative inventory carrier families are structurally " +
                "available for generic or named adapter decisions");
            Add(assertions, "cast-engine-duration-context",
                "AbilityExecutionContext plus AbilityParams metamagic surfaces",
                JoinMatches(evidence.AbilityExecutionContext, "Ability",
                    "Context", "Params") + "|" +
                    JoinMatches(evidence.AbilityParams, "Metamagic", "CasterLevel",
                        "SpellLevel"),
                Has(evidence.AbilityExecutionContext, "Ability") &&
                    (Has(evidence.AbilityExecutionContext, "Context") ||
                     Has(evidence.AbilityExecutionContext, "Params")) &&
                    Has(evidence.AbilityParams, "Metamagic"),
                "per-execution duration and native metamagic state");
            Add(assertions, "cast-engine-shared-spells-harmony",
                "SharedSpells helpers plus relevant CotW patch ordering metadata",
                "directSharedSpellsPatches=" +
                    evidence.SharedSpellsHarmony.Count + ";relevantCotwPatches=" +
                    evidence.RelevantCotwHarmony.Count,
                Has(evidence.SharedSpells, "canShareSpell") &&
                    Has(evidence.SharedSpells, "isValidShareSpellTarget") &&
                    evidence.RelevantCotwHarmony.Count > 0,
                "live registry proves whether SharedSpells owns patches and records " +
                "target, role, owner, priority, before, and after for adjacent CotW patches");
            Add(assertions, "cast-engine-shared-spells-bodies",
                "both exact helper bodies decoded",
                "instructions=" + evidence.SharedSpellsBodies.Count,
                contract != null && contract.SharedSpells != null &&
                    evidence.SharedSpellsBodies.Count > 2 &&
                    Has(evidence.SharedSpellsBodies, "canShareSpell") &&
                    Has(evidence.SharedSpellsBodies,
                        "isValidShareSpellTarget"),
                "installed CotW IL with metadata tokens resolved in-process");
            Add(assertions, "cast-engine-cotw-targeting-bodies",
                "exact CotW CanTarget and TargetAnchor patch bodies decoded",
                "instructions=" + evidence.RelevantCotwTargetingBodies.Count,
                evidence.RelevantCotwTargetingBodies.Count > 2 &&
                    Has(evidence.RelevantCotwTargetingBodies,
                        "AbilityData__CanTarget__Patch.Prefix") &&
                    Has(evidence.RelevantCotwTargetingBodies,
                        "AbilityData__TargetAnchor__Getter__Patch.Prefix"),
                "installed CotW targeting IL and patch ordering resolved in-process");
            Add(assertions, "cast-engine-native-delivery-bodies",
                "native approach distance and command decision bodies decoded",
                "instructions=" + evidence.NativeDeliveryBodies.Count,
                evidence.NativeDeliveryBodies.Count > 3 &&
                    Has(evidence.NativeDeliveryBodies,
                        "AbilityData.GetApproachDistance") &&
                    Has(evidence.NativeDeliveryBodies,
                        "UnitUseAbility.get_ShouldUnitApproach") &&
                    Has(evidence.NativeDeliveryBodies,
                        "UnitUseAbility.get_ApproachRadius"),
                "exact Kingmaker delivery IL resolved without mutating a spell or save");
            Add(assertions, "save-free-observer", "no save or input API invoked",
                "read-only engine and live Harmony registry inspection", true,
                "observer does not select, load, mutate, or save a character");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("engineContractSha256=" + Hash(path) +
                ";sharedPatches=" + evidence.SharedSpellsHarmony.Count +
                ";relevantCotwPatches=" + evidence.RelevantCotwHarmony.Count);
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void ObserveHarmony(ModContext context, Evidence evidence)
        {
            foreach (MethodBase target in context.Harmony.GetPatchedMethods())
            {
                Patches patches = context.Harmony.GetPatchInfo(target);
                AddPatches(target, "prefix", patches.Prefixes, evidence);
                AddPatches(target, "postfix", patches.Postfixes, evidence);
                AddPatches(target, "transpiler", patches.Transpilers, evidence);
            }
            evidence.SharedSpellsHarmony.Sort(StringComparer.Ordinal);
            evidence.RelevantCotwHarmony.Sort(StringComparer.Ordinal);
        }

        private static void AddPatches(MethodBase target, string role,
            IEnumerable<Patch> patches, Evidence evidence)
        {
            int order = 0;
            foreach (Patch patch in patches)
            {
                string patchType = patch.patch == null ||
                    patch.patch.DeclaringType == null ? string.Empty :
                    patch.patch.DeclaringType.FullName;
                string record = "target=" + Signature(target) + ";role=" + role +
                    ";order=" + order + ";owner=" + patch.owner +
                    ";priority=" + patch.priority + ";before=" +
                    string.Join(",", patch.before ?? new string[0]) + ";after=" +
                    string.Join(",", patch.after ?? new string[0]) + ";patch=" +
                    (patch.patch == null ? "<missing>" : Signature(patch.patch));
                if (patchType.IndexOf("CallOfTheWild.SharedSpells",
                        StringComparison.Ordinal) >= 0)
                    evidence.SharedSpellsHarmony.Add(record);
                if (string.Equals(patch.owner, "CallOfTheWild",
                        StringComparison.Ordinal) && IsRelevant(target, patchType))
                {
                    evidence.RelevantCotwHarmony.Add(record);
                    if (patch.patch != null && IsTargetingPatch(target))
                    {
                        evidence.RelevantCotwTargetingBodies.Add("method " +
                            Signature(patch.patch));
                        evidence.RelevantCotwTargetingBodies.AddRange(
                            BrownFurIlDisassembler.Describe(patch.patch));
                    }
                }
                order++;
            }
        }

        private static bool IsTargetingPatch(MethodBase target)
        {
            return target != null && target.DeclaringType == typeof(AbilityData) &&
                (string.Equals(target.Name, "CanTarget",
                    StringComparison.Ordinal) || string.Equals(target.Name,
                    "get_TargetAnchor", StringComparison.Ordinal));
        }

        private static bool IsRelevant(MethodBase target, string patchType)
        {
            string type = target == null || target.DeclaringType == null ?
                string.Empty : target.DeclaringType.FullName;
            return type.IndexOf("AbilityData", StringComparison.Ordinal) >= 0 ||
                type.IndexOf("UnitUseAbility", StringComparison.Ordinal) >= 0 ||
                type.IndexOf("RuleCastSpell", StringComparison.Ordinal) >= 0 ||
                type.IndexOf("AbilityExecutionContext", StringComparison.Ordinal) >= 0 ||
                type.IndexOf("ModifiableValue", StringComparison.Ordinal) >= 0 ||
                patchType.IndexOf("SharedSpells", StringComparison.Ordinal) >= 0 ||
                patchType.IndexOf("Metamag", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static List<string> Describe(Type type)
        {
            if (type == null) return new List<string>();
            var result = new List<string>();
            result.AddRange(type.GetConstructors(All).Select(Signature));
            result.AddRange(type.GetFields(All).Select(value => "field " +
                TypeName(value.FieldType) + " " + type.FullName + "." +
                value.Name));
            result.AddRange(type.GetProperties(All).Select(value => "property " +
                TypeName(value.PropertyType) + " " + type.FullName + "." +
                value.Name));
            result.AddRange(type.GetMethods(All).Where(value =>
                value.DeclaringType == type).Select(Signature));
            return result.Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToList();
        }

        private static List<string> DescribeTypes(IEnumerable<string> names)
        {
            var result = new List<string>();
            foreach (string name in names)
            {
                Type type = EngineType(name);
                result.Add("type " + name + " resolved=" + (type != null));
                result.AddRange(Describe(type).Select(value => name + " :: " +
                    value));
            }
            return result;
        }

        private static Type EngineType(string fullName)
        { return typeof(ModifiableValue).Assembly.GetType(fullName, false, false); }

        private static int CountTypes(IEnumerable<string> values)
        { return values.Count(value => value.StartsWith("type ",
            StringComparison.Ordinal) && value.EndsWith("resolved=True",
            StringComparison.OrdinalIgnoreCase)); }

        private static string Signature(MethodBase method)
        {
            if (method == null) return "<missing>";
            MethodInfo info = method as MethodInfo;
            string result = info == null ? string.Empty :
                TypeName(info.ReturnType) + " ";
            return result + TypeName(method.DeclaringType) + "." + method.Name +
                "(" + string.Join(",", method.GetParameters().Select(value =>
                    TypeName(value.ParameterType) + " " + value.Name).ToArray()) +
                ")";
        }

        private static string TypeName(Type type)
        { return type == null ? "<null>" : type.FullName ?? type.Name; }

        private static bool Has(IEnumerable<string> values, string token)
        { return values.Any(value => value.IndexOf(token,
            StringComparison.Ordinal) >= 0); }

        private static string JoinMatches(IEnumerable<string> values,
            params string[] tokens)
        {
            string[] matches = values.Where(value => tokens.Any(token =>
                value.IndexOf(token, StringComparison.Ordinal) >= 0)).ToArray();
            return matches.Length == 0 ? "none" : string.Join("|", matches);
        }

        private static void Add(List<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = name,
                Expected = expected, Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = evidence });
        }

        private static List<string> DescribeSharedSpellsBodies(
            CotwArcanistContract contract)
        {
            var result = new List<string>();
            if (contract == null || contract.SharedSpells == null) return result;
            foreach (MethodInfo method in new[] {
                contract.SharedSpells.CanShareSpell,
                contract.SharedSpells.IsValidShareSpellTarget })
            {
                result.Add("method " + Signature(method));
                result.AddRange(BrownFurIlDisassembler.Describe(method));
            }
            return result;
        }

        private static List<string> DescribeNativeDeliveryBodies()
        {
            var methods = new List<MethodInfo>();
            methods.Add(typeof(AbilityData).GetMethod("GetApproachDistance", All,
                null, new[] { typeof(Kingmaker.EntitySystem.Entities.UnitEntityData) },
                null));
            foreach (string name in new[] { "ShouldUnitApproach",
                "ApproachRadius" })
            {
                PropertyInfo property = typeof(UnitUseAbility).GetProperty(name,
                    All);
                methods.Add(property == null ? null : property.GetGetMethod(true));
            }
            var result = new List<string>();
            foreach (MethodInfo method in methods.Where(value => value != null))
            {
                result.Add("method " + Signature(method));
                result.AddRange(BrownFurIlDisassembler.Describe(method));
            }
            return result;
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    item.Key == key);
            return value == null ? string.Empty : value.Value;
        }
    }
}
