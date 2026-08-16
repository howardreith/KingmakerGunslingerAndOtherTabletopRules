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
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurShareTargetingScenario
    {
        private const string FileName = "brown-fur-share-targeting.json";
        private const float ThirtyFeetMeters = 9.144f;

        private sealed class SpellSpec
        {
            internal string Name;
            internal string Guid;
            internal int Level;
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class SpellEvidence
        {
            [JsonProperty("name", Order = 1)] public string Name { get; set; }
            [JsonProperty("guid", Order = 2)] public string Guid { get; set; }
            [JsonProperty("level", Order = 3)] public int Level { get; set; }
            [JsonProperty("sourceSpellbookGuid", Order = 4)] public string SourceBook { get; set; }
            [JsonProperty("originalRange", Order = 5)] public string OriginalRange { get; set; }
            [JsonProperty("baselineAnchor", Order = 6)] public string BaselineAnchor { get; set; }
            [JsonProperty("pendingEligible", Order = 7)] public bool PendingEligible { get; set; }
            [JsonProperty("pendingFailure", Order = 8)] public string PendingFailure { get; set; }
            [JsonProperty("pendingDelivery", Order = 9)] public string PendingDelivery { get; set; }
            [JsonProperty("armedAnchor", Order = 10)] public string ArmedAnchor { get; set; }
            [JsonProperty("selfTargetable", Order = 11)] public bool SelfTargetable { get; set; }
            [JsonProperty("allyTargetable", Order = 12)] public bool AllyTargetable { get; set; }
            [JsonProperty("allyRelationship", Order = 13)] public string AllyRelationship { get; set; }
            [JsonProperty("transactionsBefore", Order = 14)] public int TransactionsBefore { get; set; }
            [JsonProperty("transactionsAfterSelection", Order = 15)] public int TransactionsAfterSelection { get; set; }
            [JsonProperty("reservoirBefore", Order = 16)] public int ReservoirBefore { get; set; }
            [JsonProperty("reservoirAfterCancellation", Order = 17)] public int ReservoirAfterCancellation { get; set; }
            [JsonProperty("slotsBefore", Order = 18)] public int SlotsBefore { get; set; }
            [JsonProperty("slotsAfterCancellation", Order = 19)] public int SlotsAfterCancellation { get; set; }
            [JsonProperty("shareOnAfterCancellation", Order = 20)] public bool ShareOnAfterCancellation { get; set; }
            [JsonProperty("restoredAnchor", Order = 21)] public string RestoredAnchor { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("spells", Order = 1)] public List<SpellEvidence> Spells { get; set; }
            [JsonProperty("shareActivatableOn", Order = 2)] public bool ShareActivatableOn { get; set; }
            [JsonProperty("shareMarkerOn", Order = 3)] public bool ShareMarkerOn { get; set; }
            [JsonProperty("capstoneDelivery", Order = 4)] public string CapstoneDelivery { get; set; }
            [JsonProperty("capstoneApproachMeters", Order = 5)] public float CapstoneApproachMeters { get; set; }
            [JsonProperty("capstoneExactThirtyFeet", Order = 6)] public bool CapstoneExactThirtyFeet { get; set; }
            [JsonProperty("rangeFieldsUnchanged", Order = 7)] public bool RangeFieldsUnchanged { get; set; }
            [JsonProperty("activeTransactionsAfter", Order = 8)] public int ActiveTransactionsAfter { get; set; }
            [JsonProperty("activeScopesAfter", Order = 9)] public int ActiveScopesAfter { get; set; }
            [JsonProperty("transientsRemoved", Order = 10)] public bool TransientsRemoved { get; set; }
            [JsonProperty("unitRemoved", Order = 11)] public bool UnitRemoved { get; set; }
            [JsonProperty("actionBarTargetModeExact", Order = 12)] public bool ActionBarTargetModeExact { get; set; }
            [JsonProperty("actionBarOnClickIl", Order = 13)] public string ActionBarOnClickIl { get; set; }
        }

        private static readonly SpellSpec[] RequiredSpells = {
            new SpellSpec { Name = "Beast Shape II", Guid =
                "5d4028eb28a106d4691ed1b92bbb1915", Level = 4 },
            new SpellSpec { Name = "Undead Anatomy I", Guid =
                "8d535e198bb44ba2b6cf6ea603753fe4", Level = 3 },
            new SpellSpec { Name = "Resinous Skin", Guid =
                "41ceee31b77741e99d3b0990bbe40a2a", Level = 3 }
        };

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            string start = DateTime.UtcNow.ToString("o");
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence { Spells = new List<SpellEvidence>() };
            UnitEntityData caster = null;
            UnitEntityData ally = null;
            bool casterRegistered = false;
            bool allyRegistered = false;
            BrownFurBlueprintSet blueprints = null;
            CotwArcanistContract contract = null;
            Spellbook casting = null;
            object levelController = null;
            string stage = "contract";
            var originalRanges = new Dictionary<string, AbilityRange>();
            try
            {
                BrownFurCastExecutionRuntime.Clear();
                CotwArcanistResolution resolution =
                    BrownFurOptionalExtensionCoordinator.Current;
                blueprints = BrownFurOptionalExtensionCoordinator.Blueprints;
                if (resolution == null || !resolution.Decision.IsCompatible ||
                    resolution.Contract == null || blueprints == null)
                    throw new InvalidOperationException(
                        "Compatible registered Brown-Fur contract is unavailable.");
                contract = resolution.Contract;
                MethodInfo onClick = typeof(Kingmaker.UI.UnitSettings
                    .MechanicActionBarSlotAbility).GetMethod("OnClick",
                        BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic);
                evidence.ActionBarOnClickIl = string.Join("|",
                    BrownFurIlDisassembler.Describe(onClick).ToArray());
                int anchorCall = evidence.ActionBarOnClickIl.IndexOf(
                    "AbilityData.get_TargetAnchor()", StringComparison.Ordinal);
                int selectCall = evidence.ActionBarOnClickIl.IndexOf(
                    "ClickWithSelectedAbilityHandler.SetAbility(",
                    StringComparison.Ordinal);
                int commandCall = evidence.ActionBarOnClickIl.IndexOf(
                    "UnitUseAbility.CreateCastCommand(",
                    StringComparison.Ordinal);
                evidence.ActionBarTargetModeExact = anchorCall >= 0 &&
                    selectCall > anchorCall && commandCall > selectCall;

                stage = "units";
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                ally = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                casterRegistered = Game.Instance.State.Units.All.Add(caster);
                allyRegistered = Game.Instance.State.Units.All.Add(ally);
                if (!casterRegistered || !allyRegistered)
                    throw new InvalidOperationException(
                        "Disposable Share targeting units were not registered.");
                caster.Descriptor.Stats.Intelligence.BaseValue = 30;
                Advance(caster.Descriptor, contract.ArcanistClass, 10,
                    ref levelController);
                casting = caster.Descriptor.Spellbooks.SingleOrDefault(value =>
                    value != null && ReferenceEquals(value.Blueprint,
                        contract.CastingSpellbook));
                Spellbook preparation = caster.Descriptor.Spellbooks
                    .SingleOrDefault(value => value != null && ReferenceEquals(
                        value.Blueprint, contract.MemorizationSpellbook));
                if (casting == null || preparation == null)
                    throw new InvalidOperationException(
                        "Disposable Arcanist lacks the resolved CotW spellbooks.");
                while (casting.CasterLevel < 10) casting.AddCasterLevel();
                while (preparation.CasterLevel < 10)
                    preparation.AddCasterLevel();
                casting.UpdateAllSlotsSize(false);
                preparation.UpdateAllSlotsSize(false);
                casting.Rest();
                preparation.Rest();
                foreach (SpellSpec spec in RequiredSpells)
                {
                    BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                        BlueprintAbility>(spec.Guid);
                    if (spell == null || spell.Range != AbilityRange.Personal ||
                        spell.School != SpellSchool.Transmutation)
                        throw new InvalidOperationException(
                            "Required Personal Transmutation is unavailable: " +
                            spec.Name + ".");
                    originalRanges[spec.Guid] = spell.Range;
                    if (!casting.IsKnown(spell))
                        casting.AddKnown(spec.Level, spell, true);
                    if (!preparation.IsKnown(spell))
                        preparation.AddKnown(spec.Level, spell, true);
                }
                caster.Descriptor.Resources.Add(contract.Reservoir, true);
                if (caster.Descriptor.AddFact(
                        blueprints.ShareTransmutation) == null)
                    throw new InvalidOperationException(
                        "Share Transmutation feature could not be granted.");
                ActivatableAbility share = BrownFurPlayerIntentRuntime.Find(
                    caster.Descriptor, blueprints.ShareTransmutationAbility);
                if (share == null) throw new InvalidOperationException(
                    "Share Transmutation activatable was not granted.");

                stage = "pre-command-targeting";
                foreach (SpellSpec spec in RequiredSpells)
                {
                    BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                        BlueprintAbility>(spec.Guid);
                    var baseline = new AbilityData(spell, casting);
                    var row = new SpellEvidence {
                        Name = spec.Name, Guid = spec.Guid, Level = spec.Level,
                        SourceBook = baseline.Spellbook == null ? string.Empty :
                            baseline.Spellbook.Blueprint.AssetGuid,
                        OriginalRange = spell.Range.ToString(),
                        BaselineAnchor = baseline.TargetAnchor.ToString(),
                        TransactionsBefore =
                            BrownFurCastExecutionRuntime.ActiveTransactionCount,
                        ReservoirBefore = caster.Descriptor.Resources
                            .GetResourceAmount(contract.Reservoir),
                        SlotsBefore = AvailableSlots(casting, spec.Level)
                    };
                    share.IsOn = true;
                    var armed = new AbilityData(spell, casting);
                    BrownFurShareDelivery delivery;
                    string failure;
                    row.PendingEligible = BrownFurShareTargetingRuntime
                        .TryResolvePendingShareTargeting(armed, out delivery,
                            out failure);
                    row.PendingFailure = failure;
                    row.PendingDelivery = delivery.ToString();
                    row.ArmedAnchor = armed.TargetAnchor.ToString();
                    row.SelfTargetable = armed.CanTarget(
                        new TargetWrapper(caster));
                    row.AllyRelationship = BrownFurShareRelationshipRuntime
                        .Classify(caster.Descriptor, ally).ToString();
                    row.AllyTargetable = armed.CanTarget(
                        new TargetWrapper(ally));

                    // This is the cancellation boundary: no target is supplied,
                    // so no UnitUseAbility command or RuleCastSpell exists.
                    row.TransactionsAfterSelection =
                        BrownFurCastExecutionRuntime.ActiveTransactionCount;
                    row.ReservoirAfterCancellation = caster.Descriptor.Resources
                        .GetResourceAmount(contract.Reservoir);
                    row.SlotsAfterCancellation = AvailableSlots(casting,
                        spec.Level);
                    row.ShareOnAfterCancellation = share.IsOn;
                    share.IsOn = false;
                    row.RestoredAnchor = new AbilityData(spell, casting)
                        .TargetAnchor.ToString();
                    evidence.Spells.Add(row);
                }
                share.IsOn = true;
                evidence.ShareActivatableOn = share.IsOn;
                evidence.ShareMarkerOn = caster.Descriptor.HasFact(
                    blueprints.ShareTransmutationBuff);

                stage = "capstone";
                if (caster.Descriptor.AddFact(
                        blueprints.TransmutationSupremacy) == null)
                    throw new InvalidOperationException(
                        "Transmutation Supremacy feature could not be granted.");
                var capstone = new AbilityData(ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(RequiredSpells[0].Guid), casting);
                BrownFurShareDelivery capstoneDelivery;
                string capstoneFailure;
                if (!BrownFurShareTargetingRuntime
                    .TryResolvePendingShareTargeting(capstone,
                        out capstoneDelivery, out capstoneFailure))
                    throw new InvalidOperationException(
                        "Capstone pending targeting failed: " + capstoneFailure);
                evidence.CapstoneDelivery = capstoneDelivery.ToString();
                evidence.CapstoneApproachMeters =
                    capstone.GetApproachDistance(caster);
                evidence.CapstoneExactThirtyFeet = Math.Abs(
                    evidence.CapstoneApproachMeters - ThirtyFeetMeters) <=
                    0.001f;
                evidence.RangeFieldsUnchanged = RequiredSpells.All(spec =>
                    ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(
                        spec.Guid).Range == originalRanges[spec.Guid]);
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" + exception);
            }
            finally
            {
                if (levelController != null) TryCancel(levelController);
                if (caster != null && blueprints != null)
                {
                    BrownFurPlayerIntentRuntime.Clear(caster.Descriptor,
                        blueprints);
                    RemoveFeature(caster.Descriptor,
                        blueprints.TransmutationSupremacy);
                    RemoveFeature(caster.Descriptor,
                        blueprints.ShareTransmutation);
                    evidence.TransientsRemoved =
                        !caster.Descriptor.HasFact(
                            blueprints.ShareTransmutationBuff) &&
                        !caster.Descriptor.ActivatableAbilities.Enumerable.Any(
                            value => value != null && value.IsOn &&
                            ReferenceEquals(value.Blueprint,
                                blueprints.ShareTransmutationAbility));
                }
                BrownFurCastExecutionRuntime.Clear();
                evidence.ActiveTransactionsAfter =
                    BrownFurCastExecutionRuntime.ActiveTransactionCount;
                evidence.ActiveScopesAfter =
                    BrownFurShareTargetingRuntime.ActiveScopeCount;
                if (allyRegistered) Game.Instance.State.Units.All.Remove(ally);
                if (casterRegistered)
                    Game.Instance.State.Units.All.Remove(caster);
                if (ally != null) ally.Dispose();
                if (caster != null) caster.Dispose();
                evidence.UnitRemoved = caster == null ||
                    !Game.Instance.State.Units.All.Contains(caster);
            }

            bool requiredRows = evidence.Spells.Count == RequiredSpells.Length;
            Add(assertions, "share-pre-command-required-spells",
                "Beast Shape II, Undead Anatomy, and Resinous Skin report Unit before command construction",
                string.Join("|", evidence.Spells.Select(value => value.Name +
                    ":" + value.BaselineAnchor + "->" + value.ArmedAnchor +
                    ":" + value.PendingDelivery).ToArray()),
                requiredRows && evidence.Spells.All(value =>
                    value.BaselineAnchor != "Unit" && value.PendingEligible &&
                    string.IsNullOrEmpty(value.PendingFailure) &&
                    value.PendingDelivery == "Touch" &&
                    value.ArmedAnchor == "Unit"),
                "AbilityData.TargetAnchor queried with Share armed and no target/scope");
            Add(assertions, "share-action-bar-target-mode",
                "non-Owner TargetAnchor routes through selected-ability targeting before command creation",
                evidence.ActionBarTargetModeExact.ToString(),
                evidence.ActionBarTargetModeExact,
                "loaded Kingmaker MechanicActionBarSlotAbility.OnClick IL: TargetAnchor -> SetAbility; self command is the alternate branch");
            Add(assertions, "share-pre-command-willing-targets",
                "caster and proven controlled ally are legal creature targets",
                string.Join("|", evidence.Spells.Select(value => value.Name +
                    ":self=" + value.SelfTargetable + ":ally=" +
                    value.AllyTargetable + ":" + value.AllyRelationship)
                    .ToArray()),
                requiredRows && evidence.Spells.All(value =>
                    value.SelfTargetable && value.AllyTargetable &&
                    value.AllyRelationship != "Unknown" &&
                    value.AllyRelationship != "Ambiguous"),
                "owner-scoped native willing-target policy");
            Add(assertions, "share-target-cancellation-no-spend",
                "no command/rule, slot, or reservoir change and Share remains armed",
                string.Join("|", evidence.Spells.Select(value => value.Name +
                    ":tx=" + value.TransactionsBefore + "->" +
                    value.TransactionsAfterSelection + ":reservoir=" +
                    value.ReservoirBefore + "->" +
                    value.ReservoirAfterCancellation + ":slots=" +
                    value.SlotsBefore + "->" + value.SlotsAfterCancellation +
                    ":armed=" + value.ShareOnAfterCancellation).ToArray()),
                requiredRows && evidence.Spells.All(value =>
                    value.TransactionsBefore == 0 &&
                    value.TransactionsAfterSelection == 0 &&
                    value.ReservoirBefore == value.ReservoirAfterCancellation &&
                    value.SlotsBefore == value.SlotsAfterCancellation &&
                    value.ShareOnAfterCancellation),
                "target acquisition canceled before UnitUseAbility construction");
            Add(assertions, "share-off-native-self-cast",
                "turning Share off restores every Personal spell Owner anchor",
                string.Join("|", evidence.Spells.Select(value => value.Name +
                    ":" + value.RestoredAnchor).ToArray()),
                requiredRows && evidence.Spells.All(value =>
                    value.RestoredAnchor == value.BaselineAnchor),
                "no shared BlueprintAbility mutation");
            Add(assertions, "share-capstone-exact-thirty-feet",
                "Supremacy selects exact 9.144-meter delivery, not scaling Close range",
                evidence.CapstoneDelivery + ";approach=" +
                    evidence.CapstoneApproachMeters,
                evidence.CapstoneDelivery == "ThirtyFeet" &&
                    evidence.CapstoneExactThirtyFeet,
                "post-CotW owner-scoped approach-distance override");
            Add(assertions, "share-targeting-isolation-cleanup",
                "ranges unchanged, scopes zero, toggles cleared, unit removed",
                "ranges=" + evidence.RangeFieldsUnchanged + ";transactions=" +
                    evidence.ActiveTransactionsAfter + ";scopes=" +
                    evidence.ActiveScopesAfter + ";transients=" +
                    evidence.TransientsRemoved + ";unit=" +
                    evidence.UnitRemoved,
                evidence.RangeFieldsUnchanged &&
                    evidence.ActiveTransactionsAfter == 0 &&
                    evidence.ActiveScopesAfter == 0 &&
                    evidence.TransientsRemoved && evidence.UnitRemoved,
                "bounded cancellation and finally cleanup");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("shareTargetingSha256=" + Hash(path));
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
                StartUtc = start, EndUtc = DateTime.UtcNow.ToString("o"),
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void Advance(UnitDescriptor owner,
            BlueprintCharacterClass characterClass, int levels,
            ref object activeController)
        {
            Type type = typeof(
                Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
            MethodInfo start = type.GetMethods(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                    value.Name == "StartWithoutAssigningStaticInstance" &&
                    value.GetParameters().Length == 5);
            MethodInfo select = type.GetMethod("SelectClass",
                BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
            MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                BindingFlags.Public | BindingFlags.Instance);
            MethodInfo apply = type.GetMethod("ApplyLevelup",
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance);
            MethodInfo cancel = type.GetMethod("Cancel", BindingFlags.Public |
                BindingFlags.Instance);
            object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                "CharGen", false);
            for (int index = 0; index < levels; index++)
            {
                activeController = start.Invoke(null,
                    new object[] { owner, false, null, null, charGen });
                if (!(bool)select.Invoke(activeController,
                    new object[] { characterClass, false }))
                    throw new InvalidOperationException(
                        "Disposable Arcanist class selection failed at level " +
                        (index + 1) + ".");
                mechanics.Invoke(activeController, null);
                apply.Invoke(activeController, new object[] { owner });
                cancel.Invoke(activeController, null);
                activeController = null;
            }
        }

        private static int AvailableSlots(Spellbook book, int level)
        {
            if (book.Blueprint.Spontaneous)
                return book.GetSpontaneousSlots(level);
            return book.GetMemorizedSpellSlots(level).Count(value =>
                value != null && value.Available);
        }

        private static void TryCancel(object controller)
        {
            try
            {
                controller.GetType().GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance).Invoke(
                        controller, null);
            }
            catch { }
        }

        private static void RemoveFeature(UnitDescriptor owner,
            Kingmaker.Blueprints.Facts.BlueprintUnitFact feature)
        {
            if (owner != null && feature != null && owner.HasFact(feature))
                owner.RemoveFact(feature);
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
