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

        private sealed class Evidence
        {
            public string CotwAssembly { get; set; }
            public List<string> UnitUseAbility { get; set; }
            public List<string> AbilityData { get; set; }
            public List<string> AbilityExecutionContext { get; set; }
            public List<string> RuleCastSpell { get; set; }
            public List<string> Spellbook { get; set; }
            public List<string> ModifiableValue { get; set; }
            public List<string> SharedSpells { get; set; }
            public List<string> SharedSpellsHarmony { get; set; }
            public List<string> RelevantCotwHarmony { get; set; }
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
                AbilityExecutionContext = Describe(typeof(AbilityExecutionContext)),
                RuleCastSpell = Describe(typeof(RuleCastSpell)),
                Spellbook = Describe(typeof(Spellbook)),
                ModifiableValue = Describe(typeof(ModifiableValue)),
                SharedSpells = Describe(shared),
                SharedSpellsHarmony = new List<string>(),
                RelevantCotwHarmony = new List<string>()
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
                "ModifiableValue.AddModifier overloads",
                JoinMatches(evidence.ModifiableValue, "AddModifier"),
                Has(evidence.ModifiableValue, "AddModifier"),
                "descriptor-preserving value interception seam");
            Add(assertions, "cast-engine-duration-context",
                "AbilityExecutionContext ability/context/metamagic surfaces",
                JoinMatches(evidence.AbilityExecutionContext, "Ability",
                    "Context", "Params", "Metamagic"),
                Has(evidence.AbilityExecutionContext, "Ability") &&
                    (Has(evidence.AbilityExecutionContext, "Context") ||
                     Has(evidence.AbilityExecutionContext, "Params")),
                "per-execution duration context; exact metamagic path remains evidence");
            Add(assertions, "cast-engine-shared-spells-harmony",
                "at least one installed SharedSpells patch with ordering metadata",
                evidence.SharedSpellsHarmony.Count == 0 ? "none" :
                    string.Join("|", evidence.SharedSpellsHarmony.ToArray()),
                evidence.SharedSpellsHarmony.Count > 0,
                "Harmony12 live registry target, role, owner, priority, before, and after");
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
                    evidence.RelevantCotwHarmony.Add(record);
                order++;
            }
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
