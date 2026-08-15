using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurCastExecutionScenario
    {
        private const string FileName = "brown-fur-cast-execution.json";
        private const string SpellGuid =
            "3481906baed9487e8403e91a2e9d010a";
        private const string BuffGuid =
            "00d8fbe9cf61dc24298be8d95500c84b";

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("spellGuid", Order = 1)] public string Spell { get; set; }
            [JsonProperty("reservoirGuid", Order = 2)] public string Reservoir { get; set; }
            [JsonProperty("initialReservoir", Order = 3)] public int Initial { get; set; }
            [JsonProperty("patchOrder", Order = 4)] public string PatchOrder { get; set; }
            [JsonProperty("patchAfterCotw", Order = 5)] public bool PatchAfterCotw { get; set; }
            [JsonProperty("beginSucceeded", Order = 6)] public bool BeginSucceeded { get; set; }
            [JsonProperty("activeAfterBegin", Order = 7)] public int ActiveAfterBegin { get; set; }
            [JsonProperty("reservedAfterBegin", Order = 8)] public int ReservedAfterBegin { get; set; }
            [JsonProperty("shareScopesAfterBegin", Order = 9)] public int ShareAfterBegin { get; set; }
            [JsonProperty("supremacyScopesAfterBegin", Order = 10)] public int SupremacyAfterBegin { get; set; }
            [JsonProperty("ruleAttached", Order = 11)] public bool RuleAttached { get; set; }
            [JsonProperty("contextExtended", Order = 12)] public bool ContextExtended { get; set; }
            [JsonProperty("commitTracked", Order = 13)] public bool CommitTracked { get; set; }
            [JsonProperty("commitProceed", Order = 14)] public bool CommitProceed { get; set; }
            [JsonProperty("stateAfterCommit", Order = 15)] public string StateAfterCommit { get; set; }
            [JsonProperty("reservoirAfterCommit", Order = 16)] public int AfterCommit { get; set; }
            [JsonProperty("modifierScopesAfterCommit", Order = 17)] public int ModifierAfterCommit { get; set; }
            [JsonProperty("stateAfterRollback", Order = 18)] public string StateAfterRollback { get; set; }
            [JsonProperty("reservoirAfterRollback", Order = 19)] public int AfterRollback { get; set; }
            [JsonProperty("raceBeginSucceeded", Order = 20)] public bool RaceBegin { get; set; }
            [JsonProperty("raceReservoirBeforeCommit", Order = 21)] public int RaceBeforeCommit { get; set; }
            [JsonProperty("raceCommitTracked", Order = 22)] public bool RaceTracked { get; set; }
            [JsonProperty("raceCommitProceed", Order = 23)] public bool RaceProceed { get; set; }
            [JsonProperty("raceState", Order = 24)] public string RaceState { get; set; }
            [JsonProperty("raceReservoirAfterCommit", Order = 25)] public int RaceAfterCommit { get; set; }
            [JsonProperty("suppressedBeforeSpend", Order = 26)] public int SuppressedBefore { get; set; }
            [JsonProperty("suppressedAfterSpend", Order = 27)] public int SuppressedAfter { get; set; }
            [JsonProperty("insufficientBeginRejected", Order = 28)] public bool InsufficientRejected { get; set; }
            [JsonProperty("finalReservoir", Order = 29)] public int Final { get; set; }
            [JsonProperty("finalActive", Order = 30)] public int FinalActive { get; set; }
            [JsonProperty("finalReservations", Order = 31)] public int FinalReservations { get; set; }
            [JsonProperty("finalShareScopes", Order = 32)] public int FinalShare { get; set; }
            [JsonProperty("finalSupremacyScopes", Order = 33)] public int FinalSupremacy { get; set; }
            [JsonProperty("finalModifierScopes", Order = 34)] public int FinalModifier { get; set; }
            [JsonProperty("lastFailure", Order = 35)] public string LastFailure { get; set; }
            [JsonProperty("resourceRemoved", Order = 36)] public bool ResourceRemoved { get; set; }
            [JsonProperty("unitRemoved", Order = 37)] public bool UnitRemoved { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence { Spell = SpellGuid };
            UnitEntityData caster = null;
            bool registered = false;
            CotwArcanistContract contract = null;
            string stage = "contract";
            try
            {
                BrownFurCastExecutionRuntime.Clear();
                CotwArcanistResolution resolution =
                    BrownFurOptionalExtensionCoordinator.Current;
                if (resolution == null || !resolution.Decision.IsCompatible ||
                    resolution.Contract == null ||
                    resolution.Contract.Reservoir == null)
                    throw new InvalidOperationException(
                        "Compatible CotW cast contract is unavailable.");
                contract = resolution.Contract;
                evidence.Reservoir = contract.Reservoir.AssetGuid;
                BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(SpellGuid);
                if (spell == null || spell.School != SpellSchool.Transmutation ||
                    spell.Range != AbilityRange.Personal)
                    throw new InvalidOperationException(
                        "Exact Personal Transmutation fixture is unavailable.");
                ObservePatchOrder(context, evidence);

                stage = "unit";
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                registered = Game.Instance.State.Units.All.Add(caster);
                if (!registered) throw new InvalidOperationException(
                    "Disposable cast-execution caster was not registered.");
                caster.Descriptor.Resources.Add(contract.Reservoir, true);
                evidence.Initial = caster.Descriptor.Resources.GetResourceAmount(
                    contract.Reservoir);
                if (evidence.Initial < 2) throw new InvalidOperationException(
                    "Real CotW reservoir initialized below combined cost.");

                stage = "successful-commit";
                var ability = new AbilityData(spell, caster.Descriptor);
                var target = new TargetWrapper(caster);
                var command = new UnitUseAbility(ability, target);
                BrownFurCastTransaction transaction = Transaction(
                    "cast-execution-success");
                evidence.BeginSucceeded = BrownFurCastExecutionRuntime.Begin(
                    contract, command, ability, target, transaction, Plan());
                evidence.ActiveAfterBegin =
                    BrownFurCastExecutionRuntime.ActiveTransactionCount;
                evidence.ReservedAfterBegin =
                    BrownFurCastExecutionRuntime.ReservationCount;
                evidence.ShareAfterBegin =
                    BrownFurShareTargetingRuntime.ActiveScopeCount;
                evidence.SupremacyAfterBegin =
                    BrownFurSupremacyRuntime.ActiveScopeCount;
                var rule = new RuleCastSpell(ability, target);
                bool proceed;
                evidence.CommitTracked =
                    BrownFurCastExecutionRuntime.TryCommit(rule, out proceed);
                evidence.CommitProceed = proceed;
                evidence.RuleAttached = evidence.CommitTracked;
                evidence.ContextExtended = rule.Context.Params.HasMetamagic(
                    Metamagic.Extend);
                evidence.StateAfterCommit = transaction.State.ToString();
                evidence.AfterCommit = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                evidence.ModifierAfterCommit =
                    BrownFurModifierAdjustmentRuntime.ActiveScopeCount;
                BrownFurCastExecutionRuntime.RuleFailed(rule);
                evidence.StateAfterRollback = transaction.State.ToString();
                evidence.AfterRollback = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);

                stage = "commit-race";
                var raceAbility = new AbilityData(spell, caster.Descriptor);
                var raceCommand = new UnitUseAbility(raceAbility, target);
                BrownFurCastTransaction race = Transaction(
                    "cast-execution-race");
                evidence.RaceBegin = BrownFurCastExecutionRuntime.Begin(
                    contract, raceCommand, raceAbility, target, race, Plan());
                caster.Descriptor.Resources.Spend(contract.Reservoir,
                    evidence.Initial - 1);
                evidence.RaceBeforeCommit = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                var raceRule = new RuleCastSpell(raceAbility, target);
                evidence.RaceTracked = BrownFurCastExecutionRuntime.TryCommit(
                    raceRule, out proceed);
                evidence.RaceProceed = proceed;
                evidence.RaceState = race.State.ToString();
                evidence.RaceAfterCommit = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                evidence.SuppressedBefore =
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
                MethodInfo spend = typeof(AbilityData).GetMethod("Spend",
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (spend == null) throw new InvalidOperationException(
                    "AbilityData.Spend() was not resolved.");
                spend.Invoke(raceAbility, new object[0]);
                evidence.SuppressedAfter =
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
                BrownFurCastExecutionRuntime.EndCommand(raceCommand);
                caster.Descriptor.Resources.Restore(contract.Reservoir,
                    evidence.Initial - evidence.RaceAfterCommit);

                stage = "insufficient-reservation";
                caster.Descriptor.Resources.Spend(contract.Reservoir,
                    evidence.Initial - 1);
                var insufficientAbility = new AbilityData(spell,
                    caster.Descriptor);
                var insufficientCommand = new UnitUseAbility(
                    insufficientAbility, target);
                BrownFurCastTransaction insufficient = Transaction(
                    "cast-execution-insufficient");
                evidence.InsufficientRejected =
                    !BrownFurCastExecutionRuntime.Begin(contract,
                        insufficientCommand, insufficientAbility, target,
                        insufficient, Plan());
                insufficient.Cancel();
                caster.Descriptor.Resources.Restore(contract.Reservoir,
                    evidence.Initial - caster.Descriptor.Resources
                        .GetResourceAmount(contract.Reservoir));
                evidence.Final = caster.Descriptor.Resources.GetResourceAmount(
                    contract.Reservoir);
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" +
                    exception.GetType().FullName + ":" + exception.Message);
            }
            finally
            {
                BrownFurCastExecutionRuntime.Clear();
                evidence.FinalActive =
                    BrownFurCastExecutionRuntime.ActiveTransactionCount;
                evidence.FinalReservations =
                    BrownFurCastExecutionRuntime.ReservationCount;
                evidence.FinalShare =
                    BrownFurShareTargetingRuntime.ActiveScopeCount;
                evidence.FinalSupremacy =
                    BrownFurSupremacyRuntime.ActiveScopeCount;
                evidence.FinalModifier =
                    BrownFurModifierAdjustmentRuntime.ActiveScopeCount;
                evidence.LastFailure =
                    BrownFurCastExecutionRuntime.LastFailure ?? string.Empty;
                if (caster != null && contract != null &&
                    caster.Descriptor.Resources.ContainsResource(
                        contract.Reservoir))
                    caster.Descriptor.Resources.Remove(contract.Reservoir);
                evidence.ResourceRemoved = caster == null || contract == null ||
                    !caster.Descriptor.Resources.ContainsResource(
                        contract.Reservoir);
                if (registered) Game.Instance.State.Units.All.Remove(caster);
                if (caster != null) caster.Dispose();
                evidence.UnitRemoved = caster == null ||
                    !Game.Instance.State.Units.All.Contains(caster);
            }

            Add(assertions, "cast-execution-patch-order",
                "Brown-Fur RuleCastSpell prefix ordered after CotW",
                evidence.PatchOrder ?? string.Empty, evidence.PatchAfterCotw,
                "live Harmony registry for exact production patch");
            Add(assertions, "cast-execution-reservation-scopes",
                "one transaction, one reservation, Share and Supremacy retained",
                "begin=" + evidence.BeginSucceeded + ";active=" +
                    evidence.ActiveAfterBegin + ";reserved=" +
                    evidence.ReservedAfterBegin + ";share=" +
                    evidence.ShareAfterBegin + ";supremacy=" +
                    evidence.SupremacyAfterBegin,
                evidence.BeginSucceeded && evidence.ActiveAfterBegin == 1 &&
                    evidence.ReservedAfterBegin == 1 &&
                    evidence.ShareAfterBegin == 1 &&
                    evidence.SupremacyAfterBegin == 1,
                "real CotW reservoir reserved before rule construction");
            Add(assertions, "cast-execution-commit-debit",
                "combined request debits exactly two once at commit",
                "attached=" + evidence.RuleAttached + ";tracked=" +
                    evidence.CommitTracked + ";proceed=" +
                    evidence.CommitProceed + ";state=" +
                    evidence.StateAfterCommit + ";amount=" +
                    evidence.AfterCommit + ";modifier=" +
                    evidence.ModifierAfterCommit,
                evidence.RuleAttached && evidence.CommitTracked &&
                    evidence.CommitProceed && evidence.StateAfterCommit ==
                    BrownFurCastTransactionState.Committed.ToString() &&
                    evidence.AfterCommit == evidence.Initial - 2 &&
                    evidence.ModifierAfterCommit == 1,
                "production commit coordinator and native CotW resource");
            Add(assertions, "cast-execution-supremacy-context",
                "new exact cast context receives native Extend",
                evidence.ContextExtended.ToString(), evidence.ContextExtended,
                "retained scope precedes RuleCastSpell context construction");
            Add(assertions, "cast-execution-rollback-exact",
                "rule failure restores exact debit and fails transaction",
                "state=" + evidence.StateAfterRollback + ";amount=" +
                    evidence.AfterRollback,
                evidence.StateAfterRollback ==
                    BrownFurCastTransactionState.Failed.ToString() &&
                    evidence.AfterRollback == evidence.Initial,
                "production exception rollback path");
            Add(assertions, "cast-execution-race-rejection",
                "post-reservation shortage rejects without partial debit",
                "begin=" + evidence.RaceBegin + ";before=" +
                    evidence.RaceBeforeCommit + ";tracked=" +
                    evidence.RaceTracked + ";proceed=" + evidence.RaceProceed +
                    ";state=" + evidence.RaceState + ";after=" +
                    evidence.RaceAfterCommit,
                evidence.RaceBegin && evidence.RaceBeforeCommit == 1 &&
                    evidence.RaceTracked && !evidence.RaceProceed &&
                    evidence.RaceState ==
                        BrownFurCastTransactionState.Rejected.ToString() &&
                    evidence.RaceAfterCommit == 1,
                "availability is rechecked by exact native debit at commitment");
            Add(assertions, "cast-execution-spend-suppression",
                "rejected tracked cast suppresses one native Spend only",
                evidence.SuppressedBefore + "/" + evidence.SuppressedAfter,
                evidence.SuppressedBefore == 1 &&
                    evidence.SuppressedAfter == 0,
                "live Harmony AbilityData.Spend prefix");
            Add(assertions, "cast-execution-insufficient-reservation",
                "combined cost is rejected before any cast surface is retained",
                evidence.InsufficientRejected.ToString(),
                evidence.InsufficientRejected,
                "queued reservation includes already reserved points");
            Add(assertions, "cast-execution-cleanup",
                "all scopes and reservations zero; resource/unit removed",
                "reservoir=" + evidence.Final + ";active=" +
                    evidence.FinalActive + ";reserved=" +
                    evidence.FinalReservations + ";share=" +
                    evidence.FinalShare + ";supremacy=" +
                    evidence.FinalSupremacy + ";modifier=" +
                    evidence.FinalModifier + ";failure=" +
                    evidence.LastFailure + ";resource=" +
                    evidence.ResourceRemoved + ";unit=" + evidence.UnitRemoved,
                evidence.Final == evidence.Initial &&
                    evidence.FinalActive == 0 &&
                    evidence.FinalReservations == 0 &&
                    evidence.FinalShare == 0 &&
                    evidence.FinalSupremacy == 0 &&
                    evidence.FinalModifier == 0 &&
                    string.IsNullOrEmpty(evidence.LastFailure) &&
                    evidence.ResourceRemoved && evidence.UnitRemoved,
                "bounded disposable production-boundary cleanup");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("castExecutionSha256=" + Hash(path));
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

        private static BrownFurCastTransaction Transaction(string identity)
        {
            var intent = new BrownFurCastIntent(identity, "disposable-caster",
                SpellGuid, SpellGuid, "cotw-arcanist", "self", true,
                BrownFurAbilityScore.Strength, true, true, 2,
                "share-exact-target", "polymorph-modifier",
                "native-extend");
            var transaction = new BrownFurCastTransaction(intent);
            transaction.Validate(new BrownFurCastDecision(true, string.Empty,
                2, true, true, true, 2, BrownFurShareDelivery.Touch));
            return transaction;
        }

        private static BrownFurBonusAdapterPlan Plan()
        {
            return new BrownFurBonusAdapterPlan(
                BrownFurBonusAdapterPlanStatus.Supported, string.Empty,
                new[] { BrownFurAbilityScore.Strength },
                new[] { BuffGuid }, new[] { "Polymorph" });
        }

        private static void ObservePatchOrder(ModContext context,
            Evidence evidence)
        {
            MethodInfo target = typeof(RuleCastSpell).GetMethod("OnTrigger",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic, null,
                new[] { typeof(RulebookEventContext) }, null);
            Patches patches = target == null ? null :
                context.Harmony.GetPatchInfo(target);
            Patch patch = patches == null ? null : patches.Prefixes
                .FirstOrDefault(value => value.patch != null &&
                    value.patch.DeclaringType ==
                        typeof(BrownFurRuleCommitPatch));
            evidence.PatchOrder = patch == null ? "missing" :
                "owner=" + patch.owner + ";priority=" + patch.priority +
                ";after=" + string.Join(",", patch.after ?? new string[0]);
            evidence.PatchAfterCotw = patch != null &&
                (patch.after ?? new string[0]).Contains("CallOfTheWild");
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
