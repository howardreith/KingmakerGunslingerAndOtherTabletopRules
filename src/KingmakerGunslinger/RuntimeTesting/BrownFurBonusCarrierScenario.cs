using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurBonusCarrierScenario
    {
        private const string FileName = "brown-fur-bonus-carriers.json";

        private sealed class Case
        {
            internal string Family;
            internal string SpellGuid;
            internal string BuffGuid;
            internal StatType Stat;
            internal int Value;
            internal ModifierDescriptor Descriptor;
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Observation
        {
            [JsonProperty("family", Order = 1)] public string Family { get; set; }
            [JsonProperty("spellGuid", Order = 2)] public string SpellGuid { get; set; }
            [JsonProperty("buffGuid", Order = 3)] public string BuffGuid { get; set; }
            [JsonProperty("stat", Order = 4)] public string Stat { get; set; }
            [JsonProperty("before", Order = 5)] public int Before { get; set; }
            [JsonProperty("after", Order = 6)] public int After { get; set; }
            [JsonProperty("modifierCount", Order = 7)] public int ModifierCount { get; set; }
            [JsonProperty("modifierValue", Order = 8)] public int ModifierValue { get; set; }
            [JsonProperty("modifierDescriptor", Order = 9)] public string ModifierDescriptor { get; set; }
            [JsonProperty("sourceIsAppliedBuff", Order = 10)] public bool SourceIsAppliedBuff { get; set; }
            [JsonProperty("sourceComponent", Order = 11)] public string SourceComponent { get; set; }
            [JsonProperty("appliedToStat", Order = 12)] public string AppliedToStat { get; set; }
            [JsonProperty("sourceContextIsCastContext", Order = 13)] public bool SourceContextIsCastContext { get; set; }
            [JsonProperty("buffContextIsCastContext", Order = 14)] public bool BuffContextIsCastContext { get; set; }
            [JsonProperty("isFromSpell", Order = 15)] public bool IsFromSpell { get; set; }
            [JsonProperty("removed", Order = 16)] public bool Removed { get; set; }
            [JsonProperty("valueRestored", Order = 17)] public bool ValueRestored { get; set; }
            [JsonProperty("pass", Order = 18)] public bool Pass { get; set; }
            [JsonProperty("failure", Order = 19)] public string Failure { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("cases", Order = 1)]
            public List<Observation> Cases { get; set; }
            [JsonProperty("unitsRemoved", Order = 2)]
            public bool UnitsRemoved { get; set; }
        }

        private static readonly Case[] Cases = {
            new Case { Family = "AddStatBonus",
                SpellGuid = "4c3d08935262b6544ae97599b3a9556d",
                BuffGuid = "b175001b42b1a02479881b72fe132116",
                Stat = StatType.Strength, Value = 4,
                Descriptor = ModifierDescriptor.Enhancement },
            new Case { Family = "AddContextStatBonus",
                SpellGuid = "6f1f99b38e471fa42b1b42f7549b4210",
                BuffGuid = "082caf8c1005f114ba6375a867f638cf",
                Stat = StatType.Constitution, Value = 2,
                Descriptor = ModifierDescriptor.Enhancement },
            new Case { Family = "AddStatBonusAbilityValue",
                SpellGuid = "08ccad78cac525040919d51963f9ac39",
                BuffGuid = "b574e1583768798468335d8cdb77e94c",
                Stat = StatType.Dexterity, Value = 6,
                Descriptor = ModifierDescriptor.Enhancement },
            new Case { Family = "Polymorph",
                SpellGuid = "3481906baed9487e8403e91a2e9d010a",
                BuffGuid = "00d8fbe9cf61dc24298be8d95500c84b",
                Stat = StatType.Strength, Value = 2,
                Descriptor = ModifierDescriptor.Polymorph },
            new Case { Family = "AddGenericStatBonus+ChangeUnitSize",
                SpellGuid = "c60969e7f264e6d4b84a1499fdcf9039",
                BuffGuid = "4f139d125bb602f48bfaec3d3e1937cb",
                Stat = StatType.Strength, Value = 2,
                Descriptor = ModifierDescriptor.Size }
        };

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence { Cases = new List<Observation>() };
            CotwArcanistResolution resolution =
                BrownFurOptionalExtensionCoordinator.Current;
            Add(assertions, "bonus-carrier-cotw-contract", "compatible",
                resolution == null ? "missing" :
                    resolution.Decision.Availability.ToString(),
                resolution != null && resolution.Decision.IsCompatible,
                "isolated structural CotW contract");

            UnitEntityData caster = null;
            UnitEntityData target = null;
            bool casterRegistered = false;
            bool targetRegistered = false;
            string stage = "construct";
            try
            {
                if (resolution == null || !resolution.Decision.IsCompatible)
                    throw new InvalidOperationException(
                        "Compatible Call of the Wild contract is unavailable.");
                var source = BlueprintRoot.Instance.DefaultPlayerCharacter;
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                casterRegistered = Game.Instance.State.Units.All.Add(caster);
                targetRegistered = Game.Instance.State.Units.All.Add(target);
                if (!casterRegistered || !targetRegistered)
                    throw new InvalidOperationException(
                        "Disposable carrier units were not registered.");

                foreach (Case item in Cases)
                {
                    stage = item.Family;
                    evidence.Cases.Add(Observe(item, caster, target));
                }
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" +
                    exception.GetType().FullName + ":" + exception.Message);
            }
            finally
            {
                if (targetRegistered) Game.Instance.State.Units.All.Remove(target);
                if (casterRegistered) Game.Instance.State.Units.All.Remove(caster);
                if (target != null) target.Dispose();
                if (caster != null) caster.Dispose();
                evidence.UnitsRemoved = (target == null ||
                    !Game.Instance.State.Units.All.Contains(target)) &&
                    (caster == null ||
                    !Game.Instance.State.Units.All.Contains(caster));
            }

            foreach (Case item in Cases)
            {
                Observation observed = evidence.Cases.FirstOrDefault(value =>
                    value.Family == item.Family);
                Add(assertions, "bonus-carrier-" + item.Family.ToLowerInvariant(),
                    "value=" + item.Value + ";descriptor=" + item.Descriptor +
                        ";source/context exact;removed/restored",
                    observed == null ? "missing" : Describe(observed),
                    observed != null && observed.Pass,
                    "real installed spell buff on disposable engine units");
            }
            Add(assertions, "bonus-carrier-external-isolation",
                "disposable units removed", evidence.UnitsRemoved.ToString(),
                evidence.UnitsRemoved, "live unit registry cleanup");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("cases=" + evidence.Cases.Count + ";sha256=" +
                Hash(path));
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

        private static Observation Observe(Case item, UnitEntityData caster,
            UnitEntityData target)
        {
            var result = new Observation { Family = item.Family,
                SpellGuid = item.SpellGuid, BuffGuid = item.BuffGuid,
                Stat = item.Stat.ToString(), Failure = string.Empty };
            BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                BlueprintAbility>(item.SpellGuid);
            BlueprintBuff blueprint = ResourcesLibrary.TryGetBlueprint<
                BlueprintBuff>(item.BuffGuid);
            if (spell == null || blueprint == null)
            {
                result.Failure = "blueprint-missing";
                return result;
            }
            ModifiableValue stat = target.Descriptor.Stats.GetStat(item.Stat);
            result.Before = stat.ModifiedValue;
            var castContext = new MechanicsContext(caster, caster.Descriptor,
                spell, null, new TargetWrapper(target));
            castContext.Params.CasterLevel = 20;
            Buff applied = target.Descriptor.Buffs.AddBuff(blueprint,
                castContext, TimeSpan.FromMinutes(20d));
            if (applied == null)
            {
                result.Failure = "buff-rejected";
                return result;
            }
            try
            {
                result.After = stat.ModifiedValue;
                ModifiableValue.Modifier[] modifiers = stat.Modifiers.Where(
                    value => ReferenceEquals(value.Source, applied)).ToArray();
                result.ModifierCount = modifiers.Length;
                ModifiableValue.Modifier modifier = modifiers.SingleOrDefault();
                if (modifier != null)
                {
                    result.ModifierValue = modifier.ModValue;
                    result.ModifierDescriptor = modifier.ModDescriptor.ToString();
                    result.SourceIsAppliedBuff = ReferenceEquals(
                        modifier.Source, applied);
                    result.SourceComponent = modifier.SourceComponent ?? string.Empty;
                    result.AppliedToStat = modifier.AppliedTo == null ? string.Empty :
                        modifier.AppliedTo.Type.ToString();
                    result.SourceContextIsCastContext = ReferenceEquals(
                        modifier.Source.MaybeContext, castContext);
                }
                result.BuffContextIsCastContext = ReferenceEquals(applied.Context,
                    castContext);
                result.IsFromSpell = applied.IsFromSpell;
            }
            finally
            {
                applied.Remove();
                result.Removed = !stat.Modifiers.Any(value =>
                    ReferenceEquals(value.Source, applied));
                result.ValueRestored = stat.ModifiedValue == result.Before;
            }
            result.Pass = result.ModifierCount == 1 &&
                result.ModifierValue == item.Value &&
                result.ModifierDescriptor == item.Descriptor.ToString() &&
                result.SourceIsAppliedBuff &&
                !string.IsNullOrWhiteSpace(result.SourceComponent) &&
                result.AppliedToStat == item.Stat.ToString() &&
                result.SourceContextIsCastContext &&
                result.BuffContextIsCastContext && result.IsFromSpell &&
                result.Removed && result.ValueRestored;
            if (!result.Pass) result.Failure = "carrier-contract-mismatch";
            return result;
        }

        private static string Describe(Observation value)
        {
            return "before=" + value.Before + ";after=" + value.After +
                ";count=" + value.ModifierCount + ";value=" +
                value.ModifierValue + ";descriptor=" +
                value.ModifierDescriptor + ";component=" +
                value.SourceComponent + ";source=" +
                value.SourceIsAppliedBuff + ";sourceContext=" +
                value.SourceContextIsCastContext + ";buffContext=" +
                value.BuffContextIsCastContext + ";spell=" +
                value.IsFromSpell + ";removed=" + value.Removed +
                ";restored=" + value.ValueRestored + ";failure=" +
                value.Failure;
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
