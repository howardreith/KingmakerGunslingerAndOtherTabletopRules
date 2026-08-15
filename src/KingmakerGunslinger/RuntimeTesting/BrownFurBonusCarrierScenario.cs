using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
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
            [JsonProperty("sourceContextIsBuffContext", Order = 16)] public bool SourceContextIsBuffContext { get; set; }
            [JsonProperty("buffParentIsCastContext", Order = 17)] public bool BuffParentIsCastContext { get; set; }
            [JsonProperty("contextCasterExact", Order = 18)] public bool ContextCasterExact { get; set; }
            [JsonProperty("contextAbilityExact", Order = 19)] public bool ContextAbilityExact { get; set; }
            [JsonProperty("contextTargetExact", Order = 20)] public bool ContextTargetExact { get; set; }
            [JsonProperty("contextCasterLevel", Order = 21)] public int ContextCasterLevel { get; set; }
            [JsonProperty("removed", Order = 22)] public bool Removed { get; set; }
            [JsonProperty("valueRestored", Order = 23)] public bool ValueRestored { get; set; }
            [JsonProperty("pass", Order = 24)] public bool Pass { get; set; }
            [JsonProperty("failure", Order = 25)] public string Failure { get; set; }
            [JsonProperty("adjustedAfter", Order = 26)] public int AdjustedAfter { get; set; }
            [JsonProperty("adjustedModifierValue", Order = 27)] public int AdjustedModifierValue { get; set; }
            [JsonProperty("adjustedDescriptor", Order = 28)] public string AdjustedDescriptor { get; set; }
            [JsonProperty("adjustedCount", Order = 29)] public int AdjustedCount { get; set; }
            [JsonProperty("adjustedRemoved", Order = 30)] public bool AdjustedRemoved { get; set; }
            [JsonProperty("scopeReleased", Order = 31)] public bool ScopeReleased { get; set; }
            [JsonProperty("postReleaseModifierValue", Order = 32)] public int PostReleaseModifierValue { get; set; }
            [JsonProperty("postReleaseRemoved", Order = 33)] public bool PostReleaseRemoved { get; set; }
            [JsonProperty("noScopeLeak", Order = 34)] public bool NoScopeLeak { get; set; }
            [JsonProperty("mismatchModifierValue", Order = 35)] public int MismatchModifierValue { get; set; }
            [JsonProperty("mismatchAdjustedCount", Order = 36)] public int MismatchAdjustedCount { get; set; }
            [JsonProperty("mismatchRejected", Order = 37)] public bool MismatchRejected { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("cases", Order = 1)]
            public List<Observation> Cases { get; set; }
            [JsonProperty("unitsRemoved", Order = 2)]
            public bool UnitsRemoved { get; set; }
            [JsonProperty("advanced", Order = 3)]
            public AdvancedObservation Advanced { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class AdvancedObservation
        {
            [JsonProperty("weakerCompetition", Order = 1)] public int WeakerCompetition { get; set; }
            [JsonProperty("equalCompetition", Order = 2)] public int EqualCompetition { get; set; }
            [JsonProperty("strongerCompetition", Order = 3)] public int StrongerCompetition { get; set; }
            [JsonProperty("competitionModifierValues", Order = 4)] public List<int> CompetitionModifierValues { get; set; }
            [JsonProperty("ordinaryToEnhancedValue", Order = 5)] public int OrdinaryToEnhancedValue { get; set; }
            [JsonProperty("ordinaryToEnhancedCount", Order = 6)] public int OrdinaryToEnhancedCount { get; set; }
            [JsonProperty("enhancedRetainedAfterRelease", Order = 7)] public int EnhancedRetainedAfterRelease { get; set; }
            [JsonProperty("enhancedToOrdinaryValue", Order = 8)] public int EnhancedToOrdinaryValue { get; set; }
            [JsonProperty("enhancedToOrdinaryCount", Order = 9)] public int EnhancedToOrdinaryCount { get; set; }
            [JsonProperty("capstoneModifierValue", Order = 10)] public int CapstoneModifierValue { get; set; }
            [JsonProperty("capstoneDescriptor", Order = 11)] public string CapstoneDescriptor { get; set; }
            [JsonProperty("cleanup", Order = 12)] public bool Cleanup { get; set; }
            [JsonProperty("pass", Order = 13)] public bool Pass { get; set; }
            [JsonProperty("dispelRuleSuccess", Order = 14)] public bool DispelRuleSuccess { get; set; }
            [JsonProperty("dispelBuffRemoved", Order = 15)] public bool DispelBuffRemoved { get; set; }
            [JsonProperty("dispelValueRestored", Order = 16)] public bool DispelValueRestored { get; set; }
            [JsonProperty("expirationTimeElapsed", Order = 17)] public bool ExpirationTimeElapsed { get; set; }
            [JsonProperty("expirationBuffRemoved", Order = 18)] public bool ExpirationBuffRemoved { get; set; }
            [JsonProperty("expirationValueRestored", Order = 19)] public bool ExpirationValueRestored { get; set; }
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
                stage = "advanced-stacking-recast-capstone";
                evidence.Advanced = ObserveAdvanced(caster, target);
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
                    "ordinary=" + item.Value + ";adjusted=" +
                        (item.Value + 2) + ";descriptor=" + item.Descriptor +
                        ";source provenance exact;removed/restored;no leak",
                    observed == null ? "missing" : Describe(observed),
                    observed != null && observed.Pass,
                    "real installed spell buff on disposable engine units");
            }
            Add(assertions, "bonus-carrier-external-isolation",
                "disposable units removed", evidence.UnitsRemoved.ToString(),
                evidence.UnitsRemoved, "live unit registry cleanup");
            Add(assertions, "bonus-carrier-stacking-recast-capstone",
                "descriptor competition 6/6/10; recast 6 then 4; capstone 8 Enhancement",
                Describe(evidence.Advanced), evidence.Advanced != null &&
                    evidence.Advanced.Pass,
                "real Bull's Strength buff plus disposable descriptor competitors");

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
                result.SourceContextIsBuffContext = modifier != null &&
                    ReferenceEquals(modifier.Source.MaybeContext,
                        applied.Context);
                result.BuffParentIsCastContext = applied.Context != null &&
                    ReferenceEquals(applied.Context.ParentContext, castContext);
                result.ContextCasterExact = applied.Context != null &&
                    ReferenceEquals(applied.Context.MaybeCaster, caster);
                result.ContextAbilityExact = applied.Context != null &&
                    ReferenceEquals(applied.Context.SourceAbility, spell);
                result.ContextTargetExact = applied.Context != null &&
                    applied.Context.MainTarget.Unit == target;
                result.ContextCasterLevel = applied.Context == null ||
                    applied.Context.Params == null ? -1 :
                    applied.Context.Params.CasterLevel;
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
                result.SourceContextIsBuffContext &&
                result.BuffParentIsCastContext &&
                result.ContextCasterExact && result.ContextAbilityExact &&
                result.ContextTargetExact && result.ContextCasterLevel == 20 &&
                result.Removed && result.ValueRestored;
            string transaction = "carrier-" + item.Family;
            bool began = BrownFurModifierAdjustmentRuntime.Begin(transaction,
                castContext, caster, spell, Score(item.Stat), 2,
                new[] { item.BuffGuid }, item.Family.Split('+'));
            Buff adjusted = null;
            try
            {
                if (began) adjusted = target.Descriptor.Buffs.AddBuff(blueprint,
                    castContext, TimeSpan.FromMinutes(20d));
                if (adjusted != null)
                {
                    result.AdjustedAfter = stat.ModifiedValue;
                    ModifiableValue.Modifier modifier = stat.Modifiers.SingleOrDefault(
                        value => ReferenceEquals(value.Source, adjusted));
                    if (modifier != null)
                    {
                        result.AdjustedModifierValue = modifier.ModValue;
                        result.AdjustedDescriptor =
                            modifier.ModDescriptor.ToString();
                    }
                    result.AdjustedCount =
                        BrownFurModifierAdjustmentRuntime.AdjustedModifierCount(
                            transaction);
                }
            }
            finally
            {
                if (adjusted != null)
                {
                    adjusted.Remove();
                    result.AdjustedRemoved = !stat.Modifiers.Any(value =>
                        ReferenceEquals(value.Source, adjusted));
                }
                result.ScopeReleased = began &&
                    BrownFurModifierAdjustmentRuntime.Release(transaction);
            }
            Buff postRelease = target.Descriptor.Buffs.AddBuff(blueprint,
                castContext, TimeSpan.FromMinutes(20d));
            if (postRelease != null)
            {
                ModifiableValue.Modifier modifier = stat.Modifiers.SingleOrDefault(
                    value => ReferenceEquals(value.Source, postRelease));
                result.PostReleaseModifierValue = modifier == null ? 0 :
                    modifier.ModValue;
                postRelease.Remove();
                result.PostReleaseRemoved = !stat.Modifiers.Any(value =>
                    ReferenceEquals(value.Source, postRelease));
            }
            string mismatchTransaction = transaction + "-mismatch";
            bool mismatchBegan = BrownFurModifierAdjustmentRuntime.Begin(
                mismatchTransaction, castContext, caster, spell,
                BrownFurAbilityScore.Charisma, 2,
                new[] { item.BuffGuid }, item.Family.Split('+'));
            Buff mismatch = null;
            bool mismatchReleased = false;
            try
            {
                if (mismatchBegan) mismatch = target.Descriptor.Buffs.AddBuff(
                    blueprint, castContext, TimeSpan.FromMinutes(20d));
                if (mismatch != null)
                {
                    ModifiableValue.Modifier modifier = stat.Modifiers.
                        SingleOrDefault(value => ReferenceEquals(value.Source,
                            mismatch));
                    result.MismatchModifierValue = modifier == null ? 0 :
                        modifier.ModValue;
                    result.MismatchAdjustedCount =
                        BrownFurModifierAdjustmentRuntime.AdjustedModifierCount(
                            mismatchTransaction);
                }
            }
            finally
            {
                if (mismatch != null) mismatch.Remove();
                mismatchReleased = mismatchBegan &&
                    BrownFurModifierAdjustmentRuntime.Release(
                        mismatchTransaction);
            }
            result.MismatchRejected = mismatchBegan && mismatch != null &&
                result.MismatchModifierValue == item.Value &&
                result.MismatchAdjustedCount == 0 && mismatchReleased &&
                stat.ModifiedValue == result.Before;
            result.NoScopeLeak = BrownFurModifierAdjustmentRuntime.ActiveScopeCount ==
                0 && result.PostReleaseModifierValue == item.Value &&
                result.PostReleaseRemoved && stat.ModifiedValue == result.Before;
            result.Pass = result.Pass && began && adjusted != null &&
                result.AdjustedAfter == result.Before + item.Value + 2 &&
                result.AdjustedModifierValue == item.Value + 2 &&
                result.AdjustedDescriptor == item.Descriptor.ToString() &&
                result.AdjustedCount == 1 && result.AdjustedRemoved &&
                result.ScopeReleased && result.MismatchRejected &&
                result.NoScopeLeak;
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
                value.IsFromSpell + ";sourceBuffContext=" +
                value.SourceContextIsBuffContext + ";parent=" +
                value.BuffParentIsCastContext + ";caster=" +
                value.ContextCasterExact + ";ability=" +
                value.ContextAbilityExact + ";target=" +
                value.ContextTargetExact + ";casterLevel=" +
                value.ContextCasterLevel + ";removed=" + value.Removed +
                ";restored=" + value.ValueRestored + ";adjustedAfter=" +
                value.AdjustedAfter + ";adjustedValue=" +
                value.AdjustedModifierValue + ";adjustedDescriptor=" +
                value.AdjustedDescriptor + ";adjustedCount=" +
                value.AdjustedCount + ";adjustedRemoved=" +
                value.AdjustedRemoved + ";scopeReleased=" +
                value.ScopeReleased + ";postReleaseValue=" +
                value.PostReleaseModifierValue + ";noLeak=" +
                value.NoScopeLeak + ";mismatchValue=" +
                value.MismatchModifierValue + ";mismatchCount=" +
                value.MismatchAdjustedCount + ";mismatchRejected=" +
                value.MismatchRejected + ";failure=" + value.Failure;
        }

        private static BrownFurAbilityScore Score(StatType stat)
        {
            switch (stat)
            {
                case StatType.Strength: return BrownFurAbilityScore.Strength;
                case StatType.Dexterity: return BrownFurAbilityScore.Dexterity;
                case StatType.Constitution:
                    return BrownFurAbilityScore.Constitution;
                case StatType.Intelligence:
                    return BrownFurAbilityScore.Intelligence;
                case StatType.Wisdom: return BrownFurAbilityScore.Wisdom;
                case StatType.Charisma: return BrownFurAbilityScore.Charisma;
                default: return BrownFurAbilityScore.None;
            }
        }

        private static AdvancedObservation ObserveAdvanced(UnitEntityData caster,
            UnitEntityData target)
        {
            var result = new AdvancedObservation {
                CompetitionModifierValues = new List<int>(),
                CapstoneDescriptor = string.Empty };
            BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                BlueprintAbility>("4c3d08935262b6544ae97599b3a9556d");
            BlueprintBuff blueprint = ResourcesLibrary.TryGetBlueprint<
                BlueprintBuff>("b175001b42b1a02479881b72fe132116");
            if (spell == null || blueprint == null) return result;
            ModifiableValue stat = target.Descriptor.Stats.Strength;
            int baseline = stat.ModifiedValue;
            foreach (int competitor in new[] { 2, 6, 10 })
            {
                BlueprintFeature feature = CompetitionFeature(competitor);
                target.Descriptor.AddFact(feature);
                var context = CarrierContext(caster, target, spell);
                string transaction = "competition-" + competitor;
                Buff buff = null;
                try
                {
                    if (!BrownFurModifierAdjustmentRuntime.Begin(transaction,
                        context, caster, spell, BrownFurAbilityScore.Strength, 2,
                        new[] { "b175001b42b1a02479881b72fe132116" },
                        new[] { "AddStatBonus" })) return result;
                    buff = target.Descriptor.Buffs.AddBuff(blueprint, context,
                        TimeSpan.FromMinutes(20d));
                    ModifiableValue.Modifier modifier = buff == null ? null :
                        stat.Modifiers.SingleOrDefault(value => ReferenceEquals(
                            value.Source, buff));
                    result.CompetitionModifierValues.Add(modifier == null ? 0 :
                        modifier.ModValue);
                    int currentValue = stat.ModifiedValue;
                    if (competitor == 2)
                        result.WeakerCompetition = currentValue;
                    else if (competitor == 6)
                        result.EqualCompetition = currentValue;
                    else result.StrongerCompetition = currentValue;
                }
                finally
                {
                    if (buff != null) buff.Remove();
                    BrownFurModifierAdjustmentRuntime.Release(transaction);
                    target.Descriptor.RemoveFact(feature);
                    UnityEngine.Object.Destroy(feature);
                }
            }

            var recastContext = CarrierContext(caster, target, spell);
            Buff ordinary = target.Descriptor.Buffs.AddBuff(blueprint,
                recastContext, TimeSpan.FromMinutes(20d));
            const string ordinaryToEnhanced = "recast-ordinary-enhanced";
            Buff enhanced = null;
            try
            {
                if (!BrownFurModifierAdjustmentRuntime.Begin(ordinaryToEnhanced,
                    recastContext, caster, spell, BrownFurAbilityScore.Strength,
                    2, new[] { "b175001b42b1a02479881b72fe132116" },
                    new[] { "AddStatBonus" })) return result;
                enhanced = target.Descriptor.Buffs.AddBuff(blueprint,
                    recastContext, TimeSpan.FromMinutes(20d));
                result.OrdinaryToEnhancedValue = stat.ModifiedValue;
                result.OrdinaryToEnhancedCount = ExactBuffCount(target,
                    blueprint);
            }
            finally
            {
                BrownFurModifierAdjustmentRuntime.Release(ordinaryToEnhanced);
            }
            result.EnhancedRetainedAfterRelease = stat.ModifiedValue;
            RemoveExactBuffs(target, blueprint);

            var reverseContext = CarrierContext(caster, target, spell);
            const string enhancedToOrdinary = "recast-enhanced-ordinary";
            Buff firstEnhanced = null;
            try
            {
                if (!BrownFurModifierAdjustmentRuntime.Begin(enhancedToOrdinary,
                    reverseContext, caster, spell, BrownFurAbilityScore.Strength,
                    2, new[] { "b175001b42b1a02479881b72fe132116" },
                    new[] { "AddStatBonus" })) return result;
                firstEnhanced = target.Descriptor.Buffs.AddBuff(blueprint,
                    reverseContext, TimeSpan.FromMinutes(20d));
            }
            finally
            {
                BrownFurModifierAdjustmentRuntime.Release(enhancedToOrdinary);
            }
            Buff finalOrdinary = target.Descriptor.Buffs.AddBuff(blueprint,
                reverseContext, TimeSpan.FromMinutes(20d));
            result.EnhancedToOrdinaryValue = stat.ModifiedValue;
            result.EnhancedToOrdinaryCount = ExactBuffCount(target, blueprint);
            RemoveExactBuffs(target, blueprint);

            var capstoneContext = CarrierContext(caster, target, spell);
            const string capstone = "capstone-four";
            Buff capstoneBuff = null;
            try
            {
                if (!BrownFurModifierAdjustmentRuntime.Begin(capstone,
                    capstoneContext, caster, spell,
                    BrownFurAbilityScore.Strength, 4,
                    new[] { "b175001b42b1a02479881b72fe132116" },
                    new[] { "AddStatBonus" })) return result;
                capstoneBuff = target.Descriptor.Buffs.AddBuff(blueprint,
                    capstoneContext, TimeSpan.FromMinutes(20d));
                ModifiableValue.Modifier modifier = capstoneBuff == null ? null :
                    stat.Modifiers.SingleOrDefault(value => ReferenceEquals(
                        value.Source, capstoneBuff));
                result.CapstoneModifierValue = modifier == null ? 0 :
                    modifier.ModValue;
                result.CapstoneDescriptor = modifier == null ? string.Empty :
                    modifier.ModDescriptor.ToString();
            }
            finally
            {
                if (capstoneBuff != null) capstoneBuff.Remove();
                BrownFurModifierAdjustmentRuntime.Release(capstone);
            }

            const string dispel = "native-dispel";
            Buff dispelBuff = null;
            try
            {
                MechanicsContext dispelContext = CarrierContext(caster,
                    target, spell);
                if (!BrownFurModifierAdjustmentRuntime.Begin(dispel,
                    dispelContext, caster, spell,
                    BrownFurAbilityScore.Strength, 2,
                    new[] { "b175001b42b1a02479881b72fe132116" },
                    new[] { "AddStatBonus" })) return result;
                dispelBuff = target.Descriptor.Buffs.AddBuff(blueprint,
                    dispelContext, TimeSpan.FromMinutes(20d));
            }
            finally
            {
                BrownFurModifierAdjustmentRuntime.Release(dispel);
            }
            if (dispelBuff != null)
            {
                var dispelRule = new RuleDispelMagic(caster, target,
                    dispelBuff, RuleDispelMagic.CheckType.CasterLevel,
                    StatType.SkillKnowledgeArcana);
                SetPrivateInt(dispelRule, "<CheckRoll>k__BackingField", 20);
                SetPrivateInt(dispelRule, "<CasterLevel>k__BackingField", 20);
                SetPrivateInt(dispelRule, "<DC>k__BackingField", 1);
                Rulebook.Trigger(dispelRule);
                result.DispelRuleSuccess = dispelRule.Success;
                result.DispelBuffRemoved = ExactBuffCount(target,
                    blueprint) == 0;
                result.DispelValueRestored = stat.ModifiedValue == baseline;
                if (!result.DispelBuffRemoved) dispelBuff.Remove();
            }

            const string expiration = "native-expiration";
            Buff expirationBuff = null;
            try
            {
                MechanicsContext expirationContext = CarrierContext(caster,
                    target, spell);
                if (!BrownFurModifierAdjustmentRuntime.Begin(expiration,
                    expirationContext, caster, spell,
                    BrownFurAbilityScore.Strength, 2,
                    new[] { "b175001b42b1a02479881b72fe132116" },
                    new[] { "AddStatBonus" })) return result;
                expirationBuff = target.Descriptor.Buffs.AddBuff(blueprint,
                    expirationContext, TimeSpan.FromSeconds(1d));
            }
            finally
            {
                BrownFurModifierAdjustmentRuntime.Release(expiration);
            }
            if (expirationBuff != null)
            {
                SetPrivateNullableTimeSpan(expirationBuff, "m_EndTime",
                    Game.Instance.TimeController.GameTime -
                    TimeSpan.FromSeconds(1d));
                result.ExpirationTimeElapsed =
                    expirationBuff.TimeLeft <= TimeSpan.Zero;
                target.Descriptor.Buffs.UpdateNextEvent();
                target.Descriptor.Buffs.Tick();
                result.ExpirationBuffRemoved = ExactBuffCount(target,
                    blueprint) == 0;
                result.ExpirationValueRestored =
                    stat.ModifiedValue == baseline;
                if (!result.ExpirationBuffRemoved) expirationBuff.Remove();
            }
            result.Cleanup = stat.ModifiedValue == baseline &&
                ExactBuffCount(target, blueprint) == 0 &&
                BrownFurModifierAdjustmentRuntime.ActiveScopeCount == 0;
            result.Pass = result.WeakerCompetition == baseline + 6 &&
                result.EqualCompetition == baseline + 6 &&
                result.StrongerCompetition == baseline + 10 &&
                result.CompetitionModifierValues.SequenceEqual(
                    new[] { 6, 6, 6 }) &&
                result.OrdinaryToEnhancedValue == baseline + 6 &&
                result.OrdinaryToEnhancedCount == 1 &&
                result.EnhancedRetainedAfterRelease == baseline + 6 &&
                result.EnhancedToOrdinaryValue == baseline + 4 &&
                result.EnhancedToOrdinaryCount == 1 &&
                result.CapstoneModifierValue == 8 &&
                result.CapstoneDescriptor == "Enhancement" &&
                result.DispelRuleSuccess && result.DispelBuffRemoved &&
                result.DispelValueRestored && result.ExpirationTimeElapsed &&
                result.ExpirationBuffRemoved &&
                result.ExpirationValueRestored && result.Cleanup;
            return result;
        }

        private static MechanicsContext CarrierContext(UnitEntityData caster,
            UnitEntityData target, BlueprintAbility spell)
        {
            var context = new MechanicsContext(caster, caster.Descriptor, spell,
                null, new TargetWrapper(target));
            context.Params.CasterLevel = 20;
            return context;
        }

        private static BlueprintFeature CompetitionFeature(int value)
        {
            var component = ScriptableObject.CreateInstance<AddStatBonus>();
            component.Stat = StatType.Strength;
            component.Value = value;
            component.Descriptor = ModifierDescriptor.Enhancement;
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_Runtime_BrownFur_Competition_" + value;
            feature.Ranks = 1;
            feature.ComponentsArray = new BlueprintComponent[] { component };
            return feature;
        }

        private static int ExactBuffCount(UnitEntityData target,
            BlueprintBuff blueprint)
        {
            return target.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value =>
                ReferenceEquals(value.Blueprint, blueprint));
        }

        private static void SetPrivateInt(object owner, string fieldName,
            int value)
        {
            FieldInfo field = owner == null ? null : owner.GetType().GetField(
                fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(int))
                throw new MissingFieldException(owner == null ? string.Empty :
                    owner.GetType().FullName, fieldName);
            field.SetValue(owner, value);
        }

        private static void SetPrivateNullableTimeSpan(object owner,
            string fieldName, TimeSpan value)
        {
            FieldInfo field = owner == null ? null : owner.GetType().GetField(
                fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(TimeSpan?))
                throw new MissingFieldException(owner == null ? string.Empty :
                    owner.GetType().FullName, fieldName);
            field.SetValue(owner, (TimeSpan?)value);
        }

        private static void RemoveExactBuffs(UnitEntityData target,
            BlueprintBuff blueprint)
        {
            foreach (Buff buff in target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => ReferenceEquals(value.Blueprint, blueprint))
                .ToArray()) buff.Remove();
        }

        private static string Describe(AdvancedObservation value)
        {
            if (value == null) return "missing";
            return "competition=" + value.WeakerCompetition + "/" +
                value.EqualCompetition + "/" + value.StrongerCompetition +
                ";modifiers=" + string.Join("/", value.CompetitionModifierValues
                    .Select(item => item.ToString()).ToArray()) +
                ";ordinaryEnhanced=" + value.OrdinaryToEnhancedValue + "/" +
                value.OrdinaryToEnhancedCount + ";retained=" +
                value.EnhancedRetainedAfterRelease + ";enhancedOrdinary=" +
                value.EnhancedToOrdinaryValue + "/" +
                value.EnhancedToOrdinaryCount + ";capstone=" +
                value.CapstoneModifierValue + "/" + value.CapstoneDescriptor +
                ";dispel=" + value.DispelRuleSuccess + "/" +
                value.DispelBuffRemoved + "/" +
                value.DispelValueRestored + ";expiration=" +
                value.ExpirationTimeElapsed + "/" +
                value.ExpirationBuffRemoved + "/" +
                value.ExpirationValueRestored +
                ";cleanup=" + value.Cleanup;
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
