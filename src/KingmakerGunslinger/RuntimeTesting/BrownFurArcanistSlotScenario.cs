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
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
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
    internal static class BrownFurArcanistSlotScenario
    {
        private const string FileName = "brown-fur-arcanist-slot.json";
        private const string SpellGuid =
            "3481906baed9487e8403e91a2e9d010a";
        private const string BuffGuid =
            "00d8fbe9cf61dc24298be8d95500c84b";
        private const int SpellLevel = 3;

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("classGuid", Order = 1)] public string ClassGuid { get; set; }
            [JsonProperty("classLevel", Order = 2)] public int ClassLevel { get; set; }
            [JsonProperty("castingBlueprintGuid", Order = 3)] public string CastingGuid { get; set; }
            [JsonProperty("preparationBlueprintGuid", Order = 4)] public string PreparationGuid { get; set; }
            [JsonProperty("ownedSpellbookCount", Order = 5)] public int OwnedBookCount { get; set; }
            [JsonProperty("castingCasterLevel", Order = 6)] public int CasterLevel { get; set; }
            [JsonProperty("castingSpontaneous", Order = 7)] public bool Spontaneous { get; set; }
            [JsonProperty("spellGuid", Order = 8)] public string Spell { get; set; }
            [JsonProperty("spellLevel", Order = 9)] public int Level { get; set; }
            [JsonProperty("sourceSpellbookGuid", Order = 10)] public string SourceBook { get; set; }
            [JsonProperty("canSpendBefore", Order = 11)] public bool CanSpendBefore { get; set; }
            [JsonProperty("slotsBefore", Order = 12)] public int SlotsBefore { get; set; }
            [JsonProperty("reservoirBefore", Order = 13)] public int ReservoirBefore { get; set; }
            [JsonProperty("boundaryBegan", Order = 14)] public bool BoundaryBegan { get; set; }
            [JsonProperty("commitTracked", Order = 15)] public bool CommitTracked { get; set; }
            [JsonProperty("commitProceed", Order = 16)] public bool CommitProceed { get; set; }
            [JsonProperty("nativeSlotSpend", Order = 17)] public bool NativeSlotSpend { get; set; }
            [JsonProperty("slotsAfter", Order = 18)] public int SlotsAfter { get; set; }
            [JsonProperty("reservoirAfter", Order = 19)] public int ReservoirAfter { get; set; }
            [JsonProperty("rollbackReservoir", Order = 20)] public int RollbackReservoir { get; set; }
            [JsonProperty("raceBoundaryBegan", Order = 21)] public bool RaceBegan { get; set; }
            [JsonProperty("raceCommitTracked", Order = 22)] public bool RaceTracked { get; set; }
            [JsonProperty("raceCommitProceed", Order = 23)] public bool RaceProceed { get; set; }
            [JsonProperty("raceSlotsBefore", Order = 24)] public int RaceSlotsBefore { get; set; }
            [JsonProperty("raceSlotsAfter", Order = 25)] public int RaceSlotsAfter { get; set; }
            [JsonProperty("raceReservoirBefore", Order = 26)] public int RaceReservoirBefore { get; set; }
            [JsonProperty("raceReservoirAfter", Order = 27)] public int RaceReservoirAfter { get; set; }
            [JsonProperty("suppressedBefore", Order = 28)] public int SuppressedBefore { get; set; }
            [JsonProperty("suppressedAfter", Order = 29)] public int SuppressedAfter { get; set; }
            [JsonProperty("finalActive", Order = 30)] public int FinalActive { get; set; }
            [JsonProperty("finalReservations", Order = 31)] public int FinalReservations { get; set; }
            [JsonProperty("finalScopes", Order = 32)] public string FinalScopes { get; set; }
            [JsonProperty("resourceRemoved", Order = 33)] public bool ResourceRemoved { get; set; }
            [JsonProperty("unitRemoved", Order = 34)] public bool UnitRemoved { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence { Spell = SpellGuid,
                Level = SpellLevel };
            UnitEntityData caster = null;
            CotwArcanistContract contract = null;
            Spellbook casting = null;
            object controller = null;
            bool registered = false;
            string stage = "contract";
            try
            {
                BrownFurCastExecutionRuntime.Clear();
                CotwArcanistResolution resolution =
                    BrownFurOptionalExtensionCoordinator.Current;
                if (resolution == null || !resolution.Decision.IsCompatible ||
                    resolution.Contract == null)
                    throw new InvalidOperationException(
                        "Compatible CotW Arcanist contract is unavailable.");
                contract = resolution.Contract;
                evidence.ClassGuid = contract.ArcanistClass.AssetGuid;
                BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(SpellGuid);
                if (spell == null || spell.School != SpellSchool.Transmutation ||
                    contract.CastingSpellbook.SpellList.GetLevel(spell) !=
                        SpellLevel)
                    throw new InvalidOperationException(
                        "Exact level-three Arcanist Transmutation is unavailable.");

                stage = "level-up";
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                caster.Descriptor.Stats.Intelligence.BaseValue = 30;
                registered = Game.Instance.State.Units.All.Add(caster);
                if (!registered) throw new InvalidOperationException(
                    "Disposable Arcanist caster was not registered.");
                Advance(caster.Descriptor, contract.ArcanistClass, 5,
                    ref controller);
                evidence.ClassLevel = caster.Descriptor.Progression
                    .GetClassLevel(contract.ArcanistClass);
                Spellbook[] books = caster.Descriptor.Spellbooks.Where(value =>
                    value != null && value.Blueprint != null).ToArray();
                evidence.OwnedBookCount = books.Length;
                casting = books.SingleOrDefault(value => ReferenceEquals(
                    value.Blueprint, contract.CastingSpellbook));
                Spellbook preparation = books.SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint,
                        contract.MemorizationSpellbook));
                if (casting == null || preparation == null)
                    throw new InvalidOperationException(
                        "Native Arcanist level-up did not create both resolved spellbooks.");
                evidence.CastingGuid = casting.Blueprint.AssetGuid;
                evidence.PreparationGuid = preparation.Blueprint.AssetGuid;
                while (casting.CasterLevel < evidence.ClassLevel)
                    casting.AddCasterLevel();
                while (preparation.CasterLevel < evidence.ClassLevel)
                    preparation.AddCasterLevel();
                casting.UpdateAllSlotsSize(false);
                preparation.UpdateAllSlotsSize(false);
                casting.Rest();
                preparation.Rest();
                casting.AddKnown(SpellLevel, spell, true);
                preparation.AddKnown(SpellLevel, spell, true);
                evidence.CasterLevel = casting.CasterLevel;
                evidence.Spontaneous = casting.Blueprint.Spontaneous;

                stage = "committed-slot";
                var data = new AbilityData(spell, casting);
                evidence.SourceBook = data.Spellbook == null ? string.Empty :
                    data.Spellbook.Blueprint.AssetGuid;
                evidence.CanSpendBefore = casting.CanSpend(data, false);
                evidence.SlotsBefore = AvailableSlots(casting, SpellLevel);
                caster.Descriptor.Resources.Add(contract.Reservoir, true);
                evidence.ReservoirBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                var target = new TargetWrapper(caster);
                var command = new UnitUseAbility(data, target);
                BrownFurCastTransaction transaction = Transaction(
                    "arcanist-slot-success");
                evidence.BoundaryBegan = BrownFurCastExecutionRuntime.Begin(
                    contract, command, data, target, transaction, Plan());
                var rule = new RuleCastSpell(data, target);
                bool proceed;
                evidence.CommitTracked = BrownFurCastExecutionRuntime.TryCommit(
                    rule, out proceed);
                evidence.CommitProceed = proceed;
                evidence.NativeSlotSpend = casting.Spend(data, false);
                evidence.SlotsAfter = AvailableSlots(casting, SpellLevel);
                evidence.ReservoirAfter = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                BrownFurCastExecutionRuntime.RuleFailed(rule);
                evidence.RollbackReservoir = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);

                stage = "rejected-slot";
                casting.Rest();
                var raceData = new AbilityData(spell, casting);
                evidence.RaceSlotsBefore = AvailableSlots(casting, SpellLevel);
                var raceCommand = new UnitUseAbility(raceData, target);
                BrownFurCastTransaction race = Transaction(
                    "arcanist-slot-race");
                evidence.RaceBegan = BrownFurCastExecutionRuntime.Begin(
                    contract, raceCommand, raceData, target, race, Plan());
                caster.Descriptor.Resources.Spend(contract.Reservoir,
                    evidence.RollbackReservoir - 1);
                evidence.RaceReservoirBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                var raceRule = new RuleCastSpell(raceData, target);
                evidence.RaceTracked = BrownFurCastExecutionRuntime.TryCommit(
                    raceRule, out proceed);
                evidence.RaceProceed = proceed;
                evidence.SuppressedBefore =
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
                InvokeAbilitySpend(raceData);
                evidence.SuppressedAfter =
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
                evidence.RaceSlotsAfter = AvailableSlots(casting, SpellLevel);
                evidence.RaceReservoirAfter = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                BrownFurCastExecutionRuntime.EndCommand(raceCommand);
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" +
                    exception.GetType().FullName + ":" + exception.Message);
            }
            finally
            {
                if (controller != null) TryCancel(controller);
                BrownFurCastExecutionRuntime.Clear();
                evidence.FinalActive =
                    BrownFurCastExecutionRuntime.ActiveTransactionCount;
                evidence.FinalReservations =
                    BrownFurCastExecutionRuntime.ReservationCount;
                evidence.FinalScopes = "share=" +
                    BrownFurShareTargetingRuntime.ActiveScopeCount +
                    ";supremacy=" +
                    BrownFurSupremacyRuntime.ActiveScopeCount +
                    ";modifier=" +
                    BrownFurModifierAdjustmentRuntime.ActiveScopeCount +
                    ";suppressed=" +
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
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

            Add(assertions, "arcanist-slot-spellbooks",
                "native level-five CotW Arcanist owns both resolved books",
                "level=" + evidence.ClassLevel + ";books=" +
                    evidence.OwnedBookCount + ";casting=" +
                    evidence.CastingGuid + ";preparation=" +
                    evidence.PreparationGuid + ";casterLevel=" +
                    evidence.CasterLevel,
                evidence.ClassLevel == 5 && evidence.CasterLevel >= 5 &&
                    evidence.CastingGuid == ContractGuid(contract, true) &&
                    evidence.PreparationGuid == ContractGuid(contract, false),
                "native CotW class-level and spellbook creation");
            Add(assertions, "arcanist-slot-source",
                "AbilityData source is exact CotW casting spellbook",
                evidence.SourceBook + ";level=" + evidence.Level +
                    ";canSpend=" + evidence.CanSpendBefore +
                    ";spontaneous=" + evidence.Spontaneous,
                evidence.SourceBook == evidence.CastingGuid &&
                    evidence.Level == SpellLevel && evidence.CanSpendBefore,
                "real spellbook-backed AbilityData, not item or SLA");
            Add(assertions, "arcanist-slot-combined-commit",
                "combined intent spends two reservoir and one level-three slot",
                "begin=" + evidence.BoundaryBegan + ";tracked=" +
                    evidence.CommitTracked + ";proceed=" +
                    evidence.CommitProceed + ";reservoir=" +
                    evidence.ReservoirBefore + "->" +
                    evidence.ReservoirAfter + ";slots=" +
                    evidence.SlotsBefore + "->" + evidence.SlotsAfter +
                    ";spent=" + evidence.NativeSlotSpend,
                evidence.BoundaryBegan && evidence.CommitTracked &&
                    evidence.CommitProceed && evidence.NativeSlotSpend &&
                    evidence.ReservoirAfter == evidence.ReservoirBefore - 2 &&
                    evidence.SlotsAfter == evidence.SlotsBefore - 1,
                "production boundary plus native Spellbook.Spend");
            Add(assertions, "arcanist-slot-exception-rollback",
                "rule failure restores reservoir but not an already spent slot",
                evidence.RollbackReservoir.ToString(),
                evidence.RollbackReservoir == evidence.ReservoirBefore,
                "reservoir rollback is exact and independent of native slot");
            Add(assertions, "arcanist-slot-rejected-no-spend",
                "post-reservation shortage rejects and suppresses real slot spend",
                "begin=" + evidence.RaceBegan + ";tracked=" +
                    evidence.RaceTracked + ";proceed=" + evidence.RaceProceed +
                    ";reservoir=" + evidence.RaceReservoirBefore + "->" +
                    evidence.RaceReservoirAfter + ";slots=" +
                    evidence.RaceSlotsBefore + "->" +
                    evidence.RaceSlotsAfter + ";suppression=" +
                    evidence.SuppressedBefore + "->" +
                    evidence.SuppressedAfter,
                evidence.RaceBegan && evidence.RaceTracked &&
                    !evidence.RaceProceed && evidence.RaceReservoirBefore == 1 &&
                    evidence.RaceReservoirAfter == 1 &&
                    evidence.RaceSlotsAfter == evidence.RaceSlotsBefore &&
                    evidence.SuppressedBefore == 1 &&
                    evidence.SuppressedAfter == 0,
                "live AbilityData.Spend Harmony suppression on a real spellbook");
            Add(assertions, "arcanist-slot-cleanup",
                "all Brown-Fur state, resource, and disposable unit removed",
                "active=" + evidence.FinalActive + ";reservations=" +
                    evidence.FinalReservations + ";scopes=" +
                    evidence.FinalScopes + ";resource=" +
                    evidence.ResourceRemoved + ";unit=" + evidence.UnitRemoved,
                evidence.FinalActive == 0 && evidence.FinalReservations == 0 &&
                    evidence.FinalScopes ==
                        "share=0;supremacy=0;modifier=0;suppressed=0" &&
                    evidence.ResourceRemoved && evidence.UnitRemoved,
                "bounded save-free fixture cleanup");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("arcanistSlotSha256=" + Hash(path));
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

        private static void InvokeAbilitySpend(AbilityData ability)
        {
            MethodInfo spend = typeof(AbilityData).GetMethod("Spend",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (spend == null) throw new MissingMethodException(
                typeof(AbilityData).FullName, "Spend");
            spend.Invoke(ability, new object[0]);
        }

        private static BrownFurCastTransaction Transaction(string identity)
        {
            var intent = new BrownFurCastIntent(identity, "cotw-arcanist",
                SpellGuid, SpellGuid, "cotw-casting-spellbook", "self", true,
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

        private static string ContractGuid(CotwArcanistContract contract,
            bool casting)
        {
            BlueprintSpellbook book = contract == null ? null :
                (casting ? contract.CastingSpellbook :
                    contract.MemorizationSpellbook);
            return book == null ? string.Empty : book.AssetGuid;
        }

        private static void TryCancel(object controller)
        {
            try
            {
                controller.GetType().GetMethod("Cancel", BindingFlags.Public |
                    BindingFlags.Instance).Invoke(controller, null);
            }
            catch { }
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
