using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Read-only inventory of the installed Kingmaker spell blueprints that
    /// may safely donate Release A racial spell-like ability behavior.
    /// </summary>
    internal static class ElementalHeritageDonorAuditScenario
    {
        internal const string EvidenceFileName =
            "elemental-heritage-donor-audit.json";

        private sealed class Target
        {
            internal Target(string name, bool requiredNative)
            {
                Name = name;
                Key = Normalize(name);
                RequiredNative = requiredNative;
            }

            internal string Name { get; private set; }
            internal string Key { get; private set; }
            internal bool RequiredNative { get; private set; }
        }

        private static readonly Target[] Targets =
        {
            new Target("Firebelly", true),
            new Target("Flare Burst", true),
            new Target("Color Spray", true),
            new Target("Unerring Weapon", false),
            new Target("Expeditious Retreat", true),
            new Target("Shocking Grasp", true),
            new Target("Blur", true),
            new Target("Chill Touch", false)
        };

        private sealed class Candidate
        {
            public string Guid { get; set; }
            public string InternalName { get; set; }
            public string DisplayName { get; set; }
            public string Type { get; set; }
            public string ActionType { get; set; }
            public string Range { get; set; }
            public string Descriptor { get; set; }
            public bool SpellResistance { get; set; }
            public bool HasIcon { get; set; }
            public bool CanTargetSelf { get; set; }
            public bool CanTargetFriends { get; set; }
            public bool CanTargetEnemies { get; set; }
            public bool CanTargetPoint { get; set; }
            public string ParentGuid { get; set; }
            public string[] VariantGuids { get; set; }
            public string[] ComponentTypes { get; set; }
            public string[] SpellListLevels { get; set; }
            public bool ExactNameMatch { get; set; }
        }

        private sealed class TargetEvidence
        {
            public string Name { get; set; }
            public bool RequiredNative { get; set; }
            public List<Candidate> Candidates { get; set; }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool SaveStateTouched { get; set; }
            public int AbilityCount { get; set; }
            public int SpellListCount { get; set; }
            public List<TargetEvidence> Targets { get; set; }
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
                Targets = new List<TargetEvidence>()
            };
            string exceptionSummary = string.Empty;
            try
            {
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                if (library == null || library.GetAllBlueprints() == null)
                    throw new InvalidOperationException(
                        "The live blueprint library is unavailable.");
                BlueprintAbility[] abilities = library.GetAllBlueprints()
                    .OfType<BlueprintAbility>().Where(value => value != null)
                    .OrderBy(value => value.AssetGuid,
                        StringComparer.Ordinal).ToArray();
                BlueprintSpellList[] spellLists = library.GetAllBlueprints()
                    .OfType<BlueprintSpellList>().Where(value => value != null)
                    .OrderBy(value => value.AssetGuid,
                        StringComparer.Ordinal).ToArray();
                evidence.AbilityCount = abilities.Length;
                evidence.SpellListCount = spellLists.Length;

                foreach (Target target in Targets)
                {
                    List<Candidate> candidates = abilities.Where(value =>
                            Matches(value, target.Key))
                        .Select(value => Describe(value, target.Key,
                            spellLists)).ToList();
                    evidence.Targets.Add(new TargetEvidence
                    {
                        Name = target.Name,
                        RequiredNative = target.RequiredNative,
                        Candidates = candidates
                    });
                    int exactSpellCount = candidates.Count(value =>
                        value.ExactNameMatch && string.Equals(value.Type,
                            AbilityType.Spell.ToString(),
                            StringComparison.Ordinal));
                    Add(assertions, "elemental-heritage-donor-" +
                            target.Key,
                        target.RequiredNative ?
                            "at least one exact native Spell candidate" :
                            "inventory only; absence permits a project-owned implementation",
                        "candidates=" + candidates.Count +
                            ";exactSpells=" + exactSpellCount,
                        !target.RequiredNative || exactSpellCount > 0,
                        "live GetAllBlueprints BlueprintAbility inventory");
                }
            }
            catch (Exception exception)
            {
                exceptionSummary = exception.ToString();
                diagnostics.Add(exceptionSummary);
            }

            Add(assertions, "elemental-heritage-donor-save-free", "false",
                evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "read-only blueprint inventory; no unit or save access");
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
            diagnostics.Add("elementalHeritageDonorAuditSha256=" + Hash(path));
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

        private static Candidate Describe(BlueprintAbility ability,
            string targetKey, IEnumerable<BlueprintSpellList> spellLists)
        {
            string displayName = DisplayName(ability);
            string internalName = ability.name ?? string.Empty;
            return new Candidate
            {
                Guid = ability.AssetGuid ?? string.Empty,
                InternalName = internalName,
                DisplayName = displayName,
                Type = ability.Type.ToString(),
                ActionType = ability.ActionType.ToString(),
                Range = ability.Range.ToString(),
                Descriptor = ability.SpellDescriptor.ToString(),
                SpellResistance = ability.SpellResistance,
                HasIcon = ability.Icon != null,
                CanTargetSelf = ability.CanTargetSelf,
                CanTargetFriends = ability.CanTargetFriends,
                CanTargetEnemies = ability.CanTargetEnemies,
                CanTargetPoint = ability.CanTargetPoint,
                ParentGuid = ability.Parent == null ? string.Empty :
                    ability.Parent.AssetGuid ?? string.Empty,
                VariantGuids = (ability.Variants ??
                    new BlueprintAbility[0]).Where(value => value != null)
                    .Select(value => value.AssetGuid ?? string.Empty)
                    .ToArray(),
                ComponentTypes = (ability.ComponentsArray ??
                    new BlueprintComponent[0]).Where(value => value != null)
                    .Select(value => value.GetType().FullName)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SpellListLevels = spellLists.Where(value =>
                        value.Contains(ability)).Select(value =>
                            (value.name ?? string.Empty) + "[" +
                            (value.AssetGuid ?? string.Empty) + "]=" +
                            value.GetLevel(ability))
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                ExactNameMatch = IsExactName(internalName, targetKey) ||
                    Normalize(displayName) == targetKey
            };
        }

        private static bool Matches(BlueprintAbility ability,
            string targetKey)
        {
            return Normalize(ability.name).Contains(targetKey) ||
                Normalize(DisplayName(ability)).Contains(targetKey);
        }

        private static bool IsExactName(string name, string targetKey)
        {
            string normalized = Normalize(name);
            return normalized == targetKey ||
                normalized == targetKey + "ability" ||
                normalized == targetKey + "spell";
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return new string(value.Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant).ToArray());
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
                    string.Equals(item.Key, key,
                        StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }
    }
}
