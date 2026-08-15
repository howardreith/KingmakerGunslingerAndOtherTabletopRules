using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurSupremacyScenario
    {
        private const string FileName = "brown-fur-transmutation-supremacy.json";
        private const string TransmutationSpellGuid =
            "3481906baed9487e8403e91a2e9d010a";
        private const string ResonatingWordGuid =
            "df7d13c967bce6a40bec3ba7c9f0e64c";
        private const string ObsidianFlowGuid =
            "e48638596c955a74c8a32dbc90b518c1";

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("spellGuid", Order = 1)] public string SpellGuid { get; set; }
            [JsonProperty("spellSchool", Order = 2)] public string SpellSchool { get; set; }
            [JsonProperty("rangeBefore", Order = 3)] public string RangeBefore { get; set; }
            [JsonProperty("metamagicSupportBefore", Order = 4)] public string MetamagicSupportBefore { get; set; }
            [JsonProperty("spellLevelBefore", Order = 5)] public int SpellLevelBefore { get; set; }
            [JsonProperty("baselineMetamagic", Order = 6)] public string BaselineMetamagic { get; set; }
            [JsonProperty("baselineExtended", Order = 7)] public bool BaselineExtended { get; set; }
            [JsonProperty("scopeBegan", Order = 8)] public bool ScopeBegan { get; set; }
            [JsonProperty("scopedMetamagic", Order = 9)] public string ScopedMetamagic { get; set; }
            [JsonProperty("scopedExtended", Order = 10)] public bool ScopedExtended { get; set; }
            [JsonProperty("modifiedContexts", Order = 11)] public int ModifiedContexts { get; set; }
            [JsonProperty("duplicateRejected", Order = 12)] public bool DuplicateRejected { get; set; }
            [JsonProperty("scopeReleased", Order = 13)] public bool ScopeReleased { get; set; }
            [JsonProperty("restoredExtended", Order = 14)] public bool RestoredExtended { get; set; }
            [JsonProperty("preparedScopeBegan", Order = 15)] public bool PreparedScopeBegan { get; set; }
            [JsonProperty("preparedMatched", Order = 16)] public bool PreparedMatched { get; set; }
            [JsonProperty("preparedStillExtended", Order = 17)] public bool PreparedStillExtended { get; set; }
            [JsonProperty("preparedModifiedContexts", Order = 18)] public int PreparedModifiedContexts { get; set; }
            [JsonProperty("preparedScopeReleased", Order = 19)] public bool PreparedScopeReleased { get; set; }
            [JsonProperty("rangeAfter", Order = 20)] public string RangeAfter { get; set; }
            [JsonProperty("metamagicSupportAfter", Order = 21)] public string MetamagicSupportAfter { get; set; }
            [JsonProperty("spellLevelAfter", Order = 22)] public int SpellLevelAfter { get; set; }
            [JsonProperty("activeScopesAfter", Order = 23)] public int ActiveScopesAfter { get; set; }
            [JsonProperty("unitRemoved", Order = 24)] public bool UnitRemoved { get; set; }
            [JsonProperty("baselineDurationRounds", Order = 25)] public int BaselineDurationRounds { get; set; }
            [JsonProperty("scopedDurationRounds", Order = 26)] public int ScopedDurationRounds { get; set; }
            [JsonProperty("restoredDurationRounds", Order = 27)] public int RestoredDurationRounds { get; set; }
            [JsonProperty("preparedDurationRounds", Order = 28)] public int PreparedDurationRounds { get; set; }
            [JsonProperty("actionTypeBefore", Order = 29)] public string ActionTypeBefore { get; set; }
            [JsonProperty("actionTypeAfter", Order = 30)] public string ActionTypeAfter { get; set; }
            [JsonProperty("resonatingSupportsExtend", Order = 31)] public bool ResonatingSupportsExtend { get; set; }
            [JsonProperty("resonatingBaselineRounds", Order = 32)] public int ResonatingBaselineRounds { get; set; }
            [JsonProperty("resonatingScopedRounds", Order = 33)] public int ResonatingScopedRounds { get; set; }
            [JsonProperty("obsidianSupportsExtend", Order = 34)] public bool ObsidianSupportsExtend { get; set; }
            [JsonProperty("obsidianBaselineRounds", Order = 35)] public int ObsidianBaselineRounds { get; set; }
            [JsonProperty("obsidianScopedRounds", Order = 36)] public int ObsidianScopedRounds { get; set; }
            [JsonProperty("specialScopesReleased", Order = 37)] public bool SpecialScopesReleased { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence { SpellGuid = TransmutationSpellGuid };
            UnitEntityData caster = null;
            bool registered = false;
            string stage = "contract";
            try
            {
                CotwArcanistResolution resolution =
                    BrownFurOptionalExtensionCoordinator.Current;
                if (resolution == null || !resolution.Decision.IsCompatible)
                    throw new InvalidOperationException(
                        "Compatible Call of the Wild contract is unavailable.");
                BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(TransmutationSpellGuid);
                if (spell == null || spell.School != SpellSchool.Transmutation)
                    throw new InvalidOperationException(
                        "The exact Transmutation context fixture is unavailable.");
                stage = "unit";
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                registered = Game.Instance.State.Units.All.Add(caster);
                if (!registered) throw new InvalidOperationException(
                    "The disposable Supremacy caster was not registered.");
                var data = new AbilityData(spell, caster.Descriptor);
                var target = new TargetWrapper(caster);
                evidence.SpellSchool = spell.School.ToString();
                evidence.RangeBefore = spell.Range.ToString();
                evidence.MetamagicSupportBefore =
                    spell.AvailableMetamagic.ToString();
                evidence.SpellLevelBefore = data.SpellLevel;
                evidence.ActionTypeBefore = data.ActionType.ToString();
                var timedDuration = new ContextDurationValue {
                    Rate = DurationRate.Rounds,
                    DiceType = Kingmaker.RuleSystem.DiceType.Zero,
                    DiceCountValue = 0,
                    BonusValue = 5
                };

                stage = "baseline";
                AbilityExecutionContext baseline =
                    data.CreateExecutionContext(target);
                evidence.BaselineMetamagic = baseline.Params.Metamagic.ToString();
                evidence.BaselineExtended = baseline.Params.HasMetamagic(
                    Metamagic.Extend);
                evidence.BaselineDurationRounds =
                    timedDuration.Calculate(baseline).Value;

                stage = "scoped";
                evidence.ScopeBegan = BrownFurSupremacyRuntime.Begin(
                    "supremacy-runtime-ordinary", data);
                AbilityExecutionContext scoped = data.CreateExecutionContext(target);
                evidence.ScopedMetamagic = scoped.Params.Metamagic.ToString();
                evidence.ScopedExtended = scoped.Params.HasMetamagic(
                    Metamagic.Extend);
                evidence.ScopedDurationRounds =
                    timedDuration.Calculate(scoped).Value;
                evidence.ModifiedContexts =
                    BrownFurSupremacyRuntime.ModifiedContextCount(
                        "supremacy-runtime-ordinary");
                evidence.DuplicateRejected =
                    !BrownFurSupremacyRuntime.TryApply(data, scoped);
                evidence.ScopeReleased = BrownFurSupremacyRuntime.Release(
                    "supremacy-runtime-ordinary");
                AbilityExecutionContext restored =
                    data.CreateExecutionContext(target);
                evidence.RestoredExtended = restored.Params.HasMetamagic(
                    Metamagic.Extend);
                evidence.RestoredDurationRounds =
                    timedDuration.Calculate(restored).Value;

                stage = "prepared";
                AbilityExecutionContext prepared =
                    data.CreateExecutionContext(target);
                prepared.Params.Metamagic |= Metamagic.Extend;
                evidence.PreparedScopeBegan = BrownFurSupremacyRuntime.Begin(
                    "supremacy-runtime-prepared", data);
                evidence.PreparedMatched = BrownFurSupremacyRuntime.TryApply(
                    data, prepared);
                evidence.PreparedStillExtended = prepared.Params.HasMetamagic(
                    Metamagic.Extend);
                evidence.PreparedDurationRounds =
                    timedDuration.Calculate(prepared).Value;
                evidence.PreparedModifiedContexts =
                    BrownFurSupremacyRuntime.ModifiedContextCount(
                        "supremacy-runtime-prepared");
                evidence.PreparedScopeReleased = BrownFurSupremacyRuntime.Release(
                    "supremacy-runtime-prepared");
                evidence.RangeAfter = spell.Range.ToString();
                evidence.MetamagicSupportAfter =
                    spell.AvailableMetamagic.ToString();
                evidence.SpellLevelAfter = data.SpellLevel;
                evidence.ActionTypeAfter = data.ActionType.ToString();

                stage = "nonstandard-timed-spells";
                BlueprintAbility resonating = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(ResonatingWordGuid);
                BlueprintAbility obsidian = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(ObsidianFlowGuid);
                if (resonating == null || obsidian == null ||
                    resonating.School != SpellSchool.Transmutation ||
                    obsidian.School != SpellSchool.Transmutation)
                    throw new InvalidOperationException(
                        "The exact installed nonstandard timed Transmutations were unavailable.");
                evidence.ResonatingSupportsExtend =
                    (resonating.AvailableMetamagic & Metamagic.Extend) != 0;
                evidence.ObsidianSupportsExtend =
                    (obsidian.AvailableMetamagic & Metamagic.Extend) != 0;
                ContextDurationValue resonatingDuration = RootDurations(
                    resonating).Single(value => value.Rate ==
                        DurationRate.Rounds);
                ContextDurationValue obsidianDuration = RootDurations(
                    obsidian).Single(value => value.Rate ==
                        DurationRate.Hours);
                var resonatingData = new AbilityData(resonating,
                    caster.Descriptor);
                var obsidianData = new AbilityData(obsidian,
                    caster.Descriptor);
                AbilityExecutionContext resonatingBaseline = resonatingData
                    .CreateExecutionContext(target);
                AbilityExecutionContext obsidianBaseline = obsidianData
                    .CreateExecutionContext(target);
                evidence.ResonatingBaselineRounds = resonatingDuration
                    .Calculate(resonatingBaseline).Value;
                evidence.ObsidianBaselineRounds = obsidianDuration
                    .Calculate(obsidianBaseline).Value;
                bool resonatingScope = BrownFurSupremacyRuntime.Begin(
                    "supremacy-resonating-word", resonatingData);
                bool obsidianScope = BrownFurSupremacyRuntime.Begin(
                    "supremacy-obsidian-flow", obsidianData);
                AbilityExecutionContext resonatingScoped = resonatingData
                    .CreateExecutionContext(target);
                AbilityExecutionContext obsidianScoped = obsidianData
                    .CreateExecutionContext(target);
                evidence.ResonatingScopedRounds = resonatingDuration
                    .Calculate(resonatingScoped).Value;
                evidence.ObsidianScopedRounds = obsidianDuration
                    .Calculate(obsidianScoped).Value;
                evidence.SpecialScopesReleased = resonatingScope &&
                    obsidianScope && BrownFurSupremacyRuntime.Release(
                        "supremacy-resonating-word") &&
                    BrownFurSupremacyRuntime.Release(
                        "supremacy-obsidian-flow");
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" +
                    exception);
            }
            finally
            {
                BrownFurSupremacyRuntime.Clear();
                evidence.ActiveScopesAfter =
                    BrownFurSupremacyRuntime.ActiveScopeCount;
                if (registered) Game.Instance.State.Units.All.Remove(caster);
                if (caster != null) caster.Dispose();
                evidence.UnitRemoved = caster == null ||
                    !Game.Instance.State.Units.All.Contains(caster);
            }

            Add(assertions, "supremacy-context-baseline",
                "ordinary context lacks free Extend",
                evidence.BaselineMetamagic,
                evidence.SpellSchool == SpellSchool.Transmutation.ToString() &&
                    !evidence.BaselineExtended,
                "real installed Transmutation and native CalculateParams path");
            Add(assertions, "supremacy-context-adds-extend-once",
                "exact scoped context gains Extend exactly once",
                evidence.ScopedMetamagic + ";modified=" +
                    evidence.ModifiedContexts + ";duplicate=" +
                    evidence.DuplicateRejected,
                evidence.ScopeBegan && evidence.ScopedExtended &&
                    evidence.ModifiedContexts == 1 &&
                    evidence.DuplicateRejected && evidence.ScopeReleased,
                "after-CotW CreateExecutionContext postfix on cloned params");
            Add(assertions, "supremacy-context-already-extended",
                "prepared Extend remains single and is not modified",
                "matched=" + evidence.PreparedMatched + ";extended=" +
                    evidence.PreparedStillExtended + ";modified=" +
                    evidence.PreparedModifiedContexts,
                evidence.PreparedScopeBegan && evidence.PreparedMatched &&
                    evidence.PreparedStillExtended &&
                    evidence.PreparedModifiedContexts == 0 &&
                    evidence.PreparedScopeReleased,
                "native prepared/metamixing Extend is retained without doubling");
            Add(assertions, "supremacy-context-release",
                "released scope leaves subsequent context ordinary",
                "restoredExtended=" + evidence.RestoredExtended,
                !evidence.RestoredExtended,
                "transaction-local duration state does not leak");
            Add(assertions, "supremacy-context-duration",
                "native extendable five-round duration becomes ten once",
                "baseline=" + evidence.BaselineDurationRounds +
                    ";scoped=" + evidence.ScopedDurationRounds +
                    ";prepared=" + evidence.PreparedDurationRounds +
                    ";restored=" + evidence.RestoredDurationRounds,
                evidence.BaselineDurationRounds == 5 &&
                    evidence.ScopedDurationRounds == 10 &&
                    evidence.PreparedDurationRounds == 10 &&
                    evidence.RestoredDurationRounds == 5,
                "real ContextDurationValue.Calculate with installed CotW postfix");
            Add(assertions, "supremacy-context-casting-time",
                "ability action type is unchanged by scoped Extend",
                evidence.ActionTypeBefore + "/" + evidence.ActionTypeAfter,
                !string.IsNullOrEmpty(evidence.ActionTypeBefore) &&
                    evidence.ActionTypeBefore == evidence.ActionTypeAfter,
                "context-local metamagic enters after native action-cost selection");
            Add(assertions, "supremacy-resonating-word-duration",
                "fixed three-round Transmutation doubles despite unavailable ordinary Extend",
                "supports=" + evidence.ResonatingSupportsExtend +
                    ";rounds=" + evidence.ResonatingBaselineRounds + "->" +
                    evidence.ResonatingScopedRounds,
                !evidence.ResonatingSupportsExtend &&
                    evidence.ResonatingBaselineRounds == 3 &&
                    evidence.ResonatingScopedRounds == 6,
                "installed Resonating Word root ContextDurationValue");
            Add(assertions, "supremacy-obsidian-flow-duration",
                "fixed one-hour Transmutation doubles despite unavailable ordinary Extend",
                "supports=" + evidence.ObsidianSupportsExtend +
                    ";rounds=" + evidence.ObsidianBaselineRounds + "->" +
                    evidence.ObsidianScopedRounds,
                !evidence.ObsidianSupportsExtend &&
                    evidence.ObsidianBaselineRounds > 0 &&
                    evidence.ObsidianScopedRounds ==
                        evidence.ObsidianBaselineRounds * 2 &&
                    evidence.SpecialScopesReleased,
                "installed Obsidian Flow root area-duration ContextDurationValue");
            Add(assertions, "supremacy-context-isolation-cleanup",
                "blueprint and slot identity unchanged; scopes zero; unit removed",
                "range=" + evidence.RangeBefore + "/" + evidence.RangeAfter +
                    ";support=" + evidence.MetamagicSupportBefore + "/" +
                    evidence.MetamagicSupportAfter + ";level=" +
                    evidence.SpellLevelBefore + "/" +
                    evidence.SpellLevelAfter + ";scopes=" +
                    evidence.ActiveScopesAfter + ";unit=" +
                    evidence.UnitRemoved,
                evidence.RangeBefore == evidence.RangeAfter &&
                    evidence.MetamagicSupportBefore ==
                        evidence.MetamagicSupportAfter &&
                    evidence.SpellLevelBefore == evidence.SpellLevelAfter &&
                    evidence.ActiveScopesAfter == 0 && evidence.UnitRemoved,
                "no shared BlueprintAbility or AbilityData mutation");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("supremacyContextSha256=" + Hash(path));
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

        private static ContextDurationValue[] RootDurations(
            BlueprintAbility ability)
        {
            var values = new List<ContextDurationValue>();
            var visited = new HashSet<object>(ReferenceComparer.Instance);
            foreach (BlueprintComponent component in ability.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
                WalkDurations(component, values, visited, 0);
            return values.Distinct().ToArray();
        }

        private static void WalkDurations(object value,
            ICollection<ContextDurationValue> values, ISet<object> visited,
            int depth)
        {
            if (value == null || depth > 16 || value is string) return;
            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(decimal) ||
                type == typeof(Type)) return;
            ContextDurationValue duration = value as ContextDurationValue;
            if (duration != null)
            {
                values.Add(duration);
                return;
            }
            if (value is BlueprintAbility || value is
                Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff ||
                value is BlueprintAbilityAreaEffect) return;
            if (!visited.Add(value)) return;
            IEnumerable enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                    WalkDurations(item, values, visited, depth + 1);
                return;
            }
            for (Type cursor = type; cursor != null && cursor != typeof(object);
                cursor = cursor.BaseType)
                foreach (FieldInfo field in cursor.GetFields(
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    object member;
                    try { member = field.GetValue(value); }
                    catch { continue; }
                    WalkDurations(member, values, visited, depth + 1);
                }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public new bool Equals(object left, object right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(object value)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers
                    .GetHashCode(value);
            }
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
