using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Saved ownership of the Spider Summoner persistence fixture:
    /// the caster that holds the pinned shortcuts, and the active spider.</summary>
    public sealed class UnitPartSpiderPersistenceReceipt : UnitPart
    {
        [JsonProperty] public string Scope, Role, UnitId, CasterId, PrepareRunId, State;
    }

    /// <summary>
    /// RU-16 with the reload parts of RU-02 and RU-14, on the Working save, in
    /// the sequence <see cref="SpiderSummonerPersistencePolicy"/> fixes. A
    /// Sorcerer 11 fixture caster holds Spider Summoner and two native action
    /// bar shortcuts, each a <c>MechanicActionBarSlotSpontaneusConvertedSpell</c>
    /// holding the AbilityData the variant popup builds from a known Summon
    /// Monster spell: the original Races Unleashed Giant Black Widow row (IV)
    /// and the generated Giant Crab Spider 1d4+1 row (V). The Widow cast from
    /// its shortcut is the active spider the save carries.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int SpiderPinOriginalIndex = 0, SpiderPinGeneratedIndex = 1;
        private const string SpiderMarkerBlueprintName = "KMG_Summoning_Spider_DcBonus";
        private bool SpiderPersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == SpiderSummonerPersistencePolicy.Scope; } }
        private bool SpiderCheckpointPreserved
        { get { return (string)_request.Parameters?[SpiderSummonerPersistencePolicy.CheckpointParameter] == SpiderSummonerPersistencePolicy.Preserve; } }
        private IEnumerator<int> _spiderPersistenceSteps;
        private bool? _spiderPersistencePause;
        private bool _spiderPersistenceDone;
        private readonly List<RuntimeTestAssertion> _spiderPersistenceChecks = new List<RuntimeTestAssertion>();
        private readonly JArray _spiderPersistenceRows = new JArray();
        private static bool _spiderSummonRuleCapture;
        private static readonly List<RuleSummonUnit> SpiderSummonRules = new List<RuleSummonUnit>();

        private static void SpiderSummonRulePostfix(RuleSummonUnit __instance)
        {
            if (_spiderSummonRuleCapture && __instance?.SummonedUnit != null && SpiderSummonRules.Count < 16) SpiderSummonRules.Add(__instance);
        }

        /// <summary>What the engine's own summon rule carried: the creature, the
        /// rounds it was given, and the caster level and metamagic of the spell
        /// context that produced them.</summary>
        private static JObject SpiderSummonRuleRecord(RuleSummonUnit rule)
        {
            var parameters = rule.Context?.Params;
            return new JObject { ["unit"] = rule.SummonedUnit?.Blueprint?.AssetGuid, ["durationSeconds"] = rule.Duration.Seconds.TotalSeconds,
                ["casterLevel"] = parameters == null ? -1 : parameters.CasterLevel, ["metamagic"] = parameters == null ? null : parameters.Metamagic.ToString(),
                ["extended"] = rule.Context != null && rule.Context.HasMetamagic(Metamagic.Extend),
                ["ability"] = rule.Context?.SourceAbility?.AssetGuid, ["level"] = rule.Level };
        }

        /// <summary>Null when every summon rule gave the creature the caster
        /// level's rounds, doubled when the context is extended, otherwise why not.</summary>
        private static string SpiderDurationProblem(JArray rules, int expectedCasterLevel, bool expectedExtend)
        { return SpiderSummonDurationVerdict.Problem(rules, expectedCasterLevel, expectedExtend); }

        /// <summary>The route record when the canonical summoning set is
        /// registered; otherwise the identity and lifetime alone.</summary>
        private static JObject SpiderPersistenceUnitRecord(UnitEntityData unit, UnitEntityData caster)
        {
            if (ExpandedSummoningBlueprints.Registered != null) return SpiderUnitRecord(unit, caster);
            Buff lifetime = unit.Descriptor.Buffs.Enumerable.FirstOrDefault(value => ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
            return new JObject { ["unit"] = unit.Blueprint.AssetGuid, ["markers"] = SpiderPersistenceMarkers(unit), ["templates"] = -1,
                ["nativeTemplates"] = unit.Descriptor.Buffs.Enumerable.Count(value => ExpandedSummoningAbilityBuilder.NativeTemplateIds.Contains(value.Blueprint.AssetGuid)),
                ["secondsLeft"] = lifetime == null ? -1 : lifetime.TimeLeft.TotalSeconds };
        }

        private static int SpiderPersistenceMarkers(UnitEntityData unit)
        {
            return unit.Descriptor.Buffs.Enumerable.Count(buff => buff?.Blueprint != null && buff.Blueprint.name == SpiderMarkerBlueprintName);
        }

        private static JObject SpiderPersistencePoison(UnitEntityData unit, bool recalculate)
        {
            var result = new JObject();
            foreach (string guid in new[] { SpiderSummonerCatalog.Find("giant-black-widow").PoisonFeatureGuid, SpiderSummonerCatalog.HuntingSpiderPoisonFeature })
            {
                var features = unit.Descriptor.Progression.Features.Enumerable.Where(value => value?.Blueprint?.AssetGuid == guid).ToArray();
                var context = features.Length == 1 ? features[0].MaybeContext : null;
                int stored = context?.Params == null ? -1 : context.Params.DC;
                int recalculated = -1;
                if (recalculate && context != null) { context.Recalculate(); recalculated = context.Params == null ? -1 : context.Params.DC; }
                result[guid] = new JObject { ["facts"] = features.Length, ["dc"] = stored, ["recalculatedDc"] = recalculated };
            }
            return result;
        }

        private void SpiderPersistenceCheck(string name, bool passed, JObject row, string expected)
        {
            row["check"] = name; row["passed"] = passed; _spiderPersistenceRows.Add(row);
            _spiderPersistenceChecks.Add(Assertion("spider-persistence-" + name, expected, row.ToString(Formatting.None), passed,
                "Working-save Spider Summoner persistence fixture"));
        }

        private void StartSpiderPersistence()
        {
            if (_spiderPersistenceDone) return;
            try
            {
                if (_spiderPersistenceSteps == null) _spiderPersistenceSteps = RunSpiderPersistence().GetEnumerator();
                if (!_spiderPersistenceSteps.MoveNext() && !_spiderPersistenceDone && !_expandedSummoningPersistenceSaveStarted)
                    CompleteSpiderPersistence(RuntimeTestStatuses.Fail, "The fixture ended without a verdict.");
            }
            catch (Exception error)
            {
                SpiderPersistenceCheck("exception", false, new JObject { ["exception"] = DescribeExpandedSummoningCorrectionException(error) },
                    "The fixture completes without an exception.");
                CompleteSpiderPersistence(RuntimeTestStatuses.Fail, "No save was armed after the fixture failed.");
            }
        }

        private static UnitEntityData[] SpiderPersistenceOwned(string role)
        {
            return Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed &&
                unit.Get<UnitPartSpiderPersistenceReceipt>()?.Scope == SpiderSummonerPersistencePolicy.ReceiptScope &&
                (role == null || unit.Get<UnitPartSpiderPersistenceReceipt>().Role == role)).ToArray();
        }

        private static UnitEntityData[] SpiderPersistenceMarked()
        {
            return Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed && SpiderPersistenceMarkers(unit) > 0).ToArray();
        }

        private IEnumerable<int> RunSpiderPersistence()
        {
            string scenario = _request.Scenario;
            bool prepare = scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare;
            bool verify = scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup;
            bool preserve = SpiderCheckpointPreserved;
            bool moduleOn = _context.FeatureModules.Active.ExpandedSummoning;
            for (int frame = 0; frame < 30; frame++) yield return 0;
            string stage = SpiderSummonerPersistencePolicy.StageProblem(scenario, preserve, moduleOn);
            SpiderPersistenceCheck("stage-module-state", stage == null, new JObject { ["scenario"] = scenario, ["preserve"] = preserve, ["moduleEnabled"] = moduleOn,
                ["integration"] = SpiderSummonerIntegration.Outcome, ["problem"] = stage },
                "Prepare, the enabled cleanup and the absence check load with Expanded Summoning ON; the preserving reload loads with it OFF.");
            if (stage != null) { CompleteSpiderPersistence(RuntimeTestStatuses.Fail, "Stage and module state differ; nothing was changed or saved."); yield break; }
            UnitEntityData[] party = Game.Instance.Player.Party.Where(value => value != null && value.Descriptor != null).ToArray();
            if (party.Length != WorkingSaveSmokeScenario.ExpectedPartyCount) throw new InvalidOperationException("Working party fingerprint changed.");
            UnitEntityData anchor = party.First(value => value.HoldingState != null);
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            BlueprintScriptableObject found;
            BlueprintFeature feature = library.BlueprintsByAssetId.TryGetValue(SummonForeignRowClassifier.SpiderSummonerFeatureGuid, out found) ? found as BlueprintFeature : null;
            if (feature == null) throw new InvalidOperationException("Races Unleashed's Spider Summoner feature is not installed.");
            _spiderPersistencePause = Game.Instance.IsPaused; Game.Instance.IsPaused = true;
            if (prepare) foreach (int frame in PrepareSpiderPersistence(anchor, feature)) yield return frame;
            else if (verify) foreach (int frame in VerifySpiderPersistence(anchor, feature, preserve, moduleOn)) yield return frame;
            else
            {
                UnitEntityData[] owned = SpiderPersistenceOwned(null), marked = SpiderPersistenceMarked();
                SpiderPersistenceCheck("fresh-absence", owned.Length == 0 && marked.Length == 0, new JObject { ["receiptUnits"] = owned.Length, ["markedUnits"] = marked.Length },
                    "A fresh load of the cleaned Working save holds no fixture caster, no fixture spider and no unit carrying the Spider Summoner marker.");
            }
            Game.Instance.IsPaused = true;
            bool unrelated = Game.Instance.Player.Party.SequenceEqual(party);
            SpiderPersistenceCheck("party-preserved", unrelated, new JObject { ["party"] = party.Length }, "The Working party is unchanged.");
            if (_spiderPersistenceChecks.Any(value => value.Status != RuntimeTestStatuses.Pass))
            { CompleteSpiderPersistence(RuntimeTestStatuses.Fail, "No save was armed after a mandatory failure."); yield break; }
            if (SpiderSummonerPersistencePolicy.Writes(scenario, preserve)) BeginExpandedSummoningPersistenceSave();
            else CompleteSpiderPersistence(RuntimeTestStatuses.Pass, preserve ? "Checkpoint preserved: the module-OFF reload wrote nothing." : "");
        }

        private static BlueprintAbility SpiderPersistenceParent(int tier)
        {
            return BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, ExpandedSummoningPublisher.CanonicalParentGuids[tier - 1], "Summon Monster parent");
        }

        private IEnumerable<int> PrepareSpiderPersistence(UnitEntityData anchor, BlueprintFeature feature)
        {
            UnitEntityData[] intake = SpiderPersistenceOwned(null), marked = SpiderPersistenceMarked();
            SpiderPersistenceCheck("intake-absent", intake.Length == 0 && marked.Length == 0, new JObject { ["receiptUnits"] = intake.Length, ["markedUnits"] = marked.Length },
                "The Working save holds no earlier fixture and no marked spider before preparation.");
            if (intake.Length != 0 || marked.Length != 0) throw new InvalidOperationException("Earlier spider fixture state is present; no broad retirement is permitted.");
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            Vector3 point = anchor.Position + Vector3.right * 2.5f;
            UnitEntityData caster = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter, point, Quaternion.identity, anchor.HoldingState);
            Game.Instance.EntityCreator.Tick();
            if (caster == null || !caster.IsInState) throw new InvalidOperationException("The fixture caster did not enter the area.");
            // DefaultPlayerCharacter may cache the player group; detach it, as the Sprint 32 auxiliaries do.
            caster.GroupId = caster.UniqueId;
            var receipt = caster.Ensure<UnitPartSpiderPersistenceReceipt>();
            receipt.Scope = SpiderSummonerPersistencePolicy.ReceiptScope; receipt.Role = SpiderSummonerPersistencePolicy.CasterRole;
            receipt.UnitId = caster.UniqueId; receipt.CasterId = caster.UniqueId; receipt.PrepareRunId = _request.RunId;
            SetExpandedSummoningBrainActive(caster, false); PlaceExpandedSummoningUnit(caster, point);
            if (!Game.Instance.State.AwakeUnits.Contains(caster)) Game.Instance.State.AwakeUnits.Add(caster);
            caster.Stats.Charisma.BaseValue = 30; caster.Stats.HitPoints.BaseValue = 100000;
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, SpiderSorcererClass, "Sorcerer");
            JObject levels = SpiderNaturalLevels(caster, sorcerer, 11, null, null, null, Alignment.TrueNeutral);
            caster.Descriptor.Alignment.Set(Alignment.TrueNeutral);
            UnitEntityData[] pets = Game.Instance.State.Units.All.Where(value => value != null && ReferenceEquals(value.Descriptor.Master.Value, caster)).ToArray();
            if (pets.Length != 0) DisposeExpandedSummoningUnits(new List<UnitEntityData>(pets), pets);
            Spellbook book = caster.Descriptor.GetSpellbook(sorcerer);
            if (book == null) throw new InvalidOperationException("The fixture caster has no Sorcerer book.");
            foreach (int tier in new[] { 4, 5 })
                if (!book.GetKnownSpells(tier).Any(value => ReferenceEquals(value.Blueprint, SpiderPersistenceParent(tier)))) book.AddKnown(tier, SpiderPersistenceParent(tier), true);
            book.UpdateAllSlotsSize(false); book.Rest();
            caster.Descriptor.AddFact(feature);
            AbilityData known4 = book.GetKnownSpells(4).First(value => ReferenceEquals(value.Blueprint, SpiderPersistenceParent(4)));
            AbilityData known5 = book.GetKnownSpells(5).First(value => ReferenceEquals(value.Blueprint, SpiderPersistenceParent(5)));
            BlueprintAbility widow = (BlueprintAbility)library.BlueprintsByAssetId[SpiderSummonerCatalog.RowIdentity(SummonFamily.Monster, SpiderSummonerCatalog.Find("giant-black-widow"))];
            SpiderPlacement crabPlacement = SpiderSummonerCatalog.Placements(SummonFamily.Monster).Single(value => value.Species.Key == "giant-crab-spider" && value.Tier == 5);
            BlueprintAbility crab = library.BlueprintsByAssetId.Values.OfType<BlueprintAbility>().Single(value => SpiderSummonerIntegration.PlacementOf(value)?.Symbol == crabPlacement.Symbol);
            // As the variant popup expands a known spell's children.
            var pinOriginal = new MechanicActionBarSlotSpontaneusConvertedSpell { Spell = new AbilityData(known4, widow), Unit = caster };
            var pinGenerated = new MechanicActionBarSlotSpontaneusConvertedSpell { Spell = new AbilityData(known5, crab), Unit = caster };
            caster.UISettings.GetSlot(0, caster);
            caster.UISettings.SetSlot(pinOriginal, SpiderPinOriginalIndex);
            caster.UISettings.SetSlot(pinGenerated, SpiderPinGeneratedIndex);
            bool pinsReady = pinOriginal.Spell.IsVisible() && pinOriginal.Spell.IsAvailableForCast && pinGenerated.Spell.IsVisible() && pinGenerated.Spell.IsAvailableForCast &&
                ReferenceEquals(pinOriginal.Spell.ConvertedFrom, known4) && ReferenceEquals(pinGenerated.Spell.ConvertedFrom, known5);
            SpiderPersistenceCheck("prepared-caster-and-pins", pinsReady && SpiderHasFeature(caster, feature) && book.CasterLevel == 11, new JObject { ["levels"] = levels,
                ["casterLevel"] = book.CasterLevel, ["pets"] = pets.Length, ["original"] = widow.AssetGuid, ["generated"] = crab.AssetGuid, ["generatedSymbol"] = crabPlacement.Symbol,
                ["slotType"] = typeof(MechanicActionBarSlotSpontaneusConvertedSpell).Name },
                "A Sorcerer 11 with Spider Summoner pins the original Black Widow (IV) and the generated Crab Spider 1d4+1 (V), each the popup's child of its known spell, and both are castable.");
            if (!pinsReady) throw new InvalidOperationException("Prepared pins are not castable.");

            JObject cast = null; UnitEntityData[] spawned = null;
            foreach (int frame in SpiderPersistenceCast(caster, pinOriginal.Spell, book, 4, DiceType.Zero, true, (record, units) => { cast = record; spawned = units; })) yield return frame;
            string unitsProblem = SpiderRouteObservationVerdict.UnitsProblem(cast["units"] as JArray, 1, SpiderSummonerCatalog.Find("giant-black-widow").UnitGuid, true);
            string durationProblem = SpiderDurationProblem(cast["summonRules"] as JArray, 11, false);
            bool castOk = (string)cast["result"] == "Success" && (bool)cast["exactlyOnePoolUse"] && unitsProblem == null && durationProblem == null && spawned.Length == 1;
            cast["unitsProblem"] = SpiderEscapeObservationVerdict.Optional(unitsProblem); cast["durationProblem"] = SpiderEscapeObservationVerdict.Optional(durationProblem);
            SpiderPersistenceCheck("prepared-active-widow", castOk, cast,
                "The original shortcut casts one Black Widow with one benefit and one Summon Monster template, 11 rounds at caster level 11, spending exactly one level 4 use.");
            if (!castOk) throw new InvalidOperationException("The active spider was not established.");
            UnitEntityData summon = spawned[0];
            var summonReceipt = summon.Ensure<UnitPartSpiderPersistenceReceipt>();
            summonReceipt.Scope = SpiderSummonerPersistencePolicy.ReceiptScope; summonReceipt.Role = SpiderSummonerPersistencePolicy.SummonRole;
            summonReceipt.UnitId = summon.UniqueId; summonReceipt.CasterId = caster.UniqueId; summonReceipt.PrepareRunId = _request.RunId;
            SetExpandedSummoningBrainActive(summon, false);
            JObject poison = SpiderPersistencePoison(summon, false);
            var state = new JObject { ["original"] = widow.AssetGuid, ["generated"] = crab.AssetGuid, ["known4"] = known4.Blueprint.AssetGuid, ["known5"] = known5.Blueprint.AssetGuid,
                ["book"] = book.Blueprint.AssetGuid, ["pool4"] = book.GetSpontaneousSlots(4), ["pool5"] = book.GetSpontaneousSlots(5), ["summon"] = summon.UniqueId,
                ["markers"] = SpiderPersistenceMarkers(summon), ["poison"] = poison };
            receipt.State = state.ToString(Formatting.None); summonReceipt.State = receipt.State;
            bool poisonExact = poison.Properties().All(value => (int)value.Value["facts"] == 1 && (int)value.Value["dc"] == 16);
            SpiderPersistenceCheck("prepared-saved-state", poisonExact && (int)state["markers"] == 1, state,
                "The saved Widow carries one marker and both its poisons at DC 16 (14 + 2).");
        }

        /// <summary>Casts exactly this AbilityData through the native command
        /// path. The spawned units are disposed unless kept.</summary>
        private IEnumerable<int> SpiderPersistenceCast(UnitEntityData caster, AbilityData data, Spellbook book, int level, DiceType forcedDie, bool keep,
            Action<JObject, UnitEntityData[]> report)
        {
            var debit = new Cleanup2SourceDebit(caster);
            int poolBefore = book.GetSpontaneousSlots(level);
            bool visible = data.IsVisible(), available = data.IsAvailableForCast;
            var die = forcedDie == DiceType.Zero ? null : new SpiderDieOverride { Initiator = caster, Die = forcedDie, Value = 2 };
            var hooks = HarmonyInstance.Create("kmg.spider.persistence." + _request.RunId + "." + Time.frameCount);
            UnitUseAbility command = null; string result;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleSummonUnit).GetMethod("OnTrigger", BindingFlags.Public | BindingFlags.Instance), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("SpiderSummonRulePostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"), new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                SpiderSummonRules.Clear(); _spiderSummonRuleCapture = true;
                _expandedSummoningPlayerPathSpawnActionCount = 0; _expandedSummoningPlayerPathCaptureActive = true;
                if (die != null) EventBus.Subscribe(die);
                command = new UnitUseAbility(data, new TargetWrapper(caster.Position + new Vector3(2, 0, 2)));
                caster.Commands.Run(command);
                while (!command.IsFinished && watch.Elapsed.TotalSeconds < 25) { Game.Instance.IsPaused = false; yield return 0; }
                result = command.IsFinished ? command.Result.ToString() : "unfinished";
            }
            finally
            {
                if (die != null) EventBus.Unsubscribe(die);
                _spiderSummonRuleCapture = false; _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id);
                if (command != null && !command.IsFinished) command.Interrupt(true);
                Game.Instance.IsPaused = true;
            }
            for (int frame = 0; frame < 12; frame++) yield return 0;
            UnitEntityData[] units = SpiderSummonRules.Select(value => value.SummonedUnit).Where(value => value != null).Distinct().ToArray();
            var record = new JObject { ["ability"] = data.Blueprint.AssetGuid, ["name"] = data.Blueprint.name, ["visible"] = visible, ["available"] = available,
                ["result"] = result, ["spawnActions"] = _expandedSummoningPlayerPathSpawnActionCount, ["count"] = units.Length,
                ["units"] = new JArray(units.Select(value => SpiderPersistenceUnitRecord(value, caster))), ["summonRules"] = new JArray(SpiderSummonRules.Select(SpiderSummonRuleRecord)),
                ["poolBefore"] = poolBefore, ["poolAfter"] = book.GetSpontaneousSlots(level),
                ["exactlyOnePoolUse"] = debit.ExactlyOneSpontaneous(book, level), ["unspent"] = debit.Unspent,
                ["forcedDie"] = die == null ? null : new JArray(die.Rolls), ["wallSeconds"] = watch.Elapsed.TotalSeconds };
            foreach (JObject unit in ((JArray)record["units"]).OfType<JObject>()) unit["persistenceMarkers"] = 0;
            for (int index = 0; index < units.Length; index++) ((JObject)((JArray)record["units"])[index])["persistenceMarkers"] = SpiderPersistenceMarkers(units[index]);
            SpiderSummonRules.Clear();
            report(record, units);
            if (!keep && units.Length != 0) DisposeExpandedSummoningUnits(new List<UnitEntityData>(units), units);
        }

        private IEnumerable<int> VerifySpiderPersistence(UnitEntityData anchor, BlueprintFeature feature, bool preserve, bool moduleOn)
        {
            UnitEntityData[] casters = SpiderPersistenceOwned(SpiderSummonerPersistencePolicy.CasterRole);
            UnitEntityData[] summons = SpiderPersistenceOwned(SpiderSummonerPersistencePolicy.SummonRole);
            UnitEntityData caster = casters.Length == 1 ? casters[0] : null;
            UnitEntityData summon = summons.Length == 1 ? summons[0] : null;
            JObject saved = caster?.Get<UnitPartSpiderPersistenceReceipt>()?.State == null ? null : JObject.Parse(caster.Get<UnitPartSpiderPersistenceReceipt>().State);
            bool owned = caster != null && summon != null && saved != null && (string)saved["summon"] == summon.UniqueId &&
                summon.Get<UnitPartSpiderPersistenceReceipt>().CasterId == caster.UniqueId && caster.Get<UnitPartSpiderPersistenceReceipt>().UnitId == caster.UniqueId;
            SpiderPersistenceCheck("reloaded-ownership", owned, new JObject { ["casters"] = casters.Length, ["summons"] = summons.Length, ["saved"] = saved,
                ["prepareRun"] = caster?.Get<UnitPartSpiderPersistenceReceipt>()?.PrepareRunId },
                "The reloaded save holds exactly the prepared caster and its active spider, bound by their saved receipts.");
            if (!owned) throw new InvalidOperationException("The prepared fixture is not exactly present.");
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, SpiderSorcererClass, "Sorcerer");
            Spellbook book = caster.Descriptor.GetSpellbook(sorcerer);
            MechanicActionBarSlot[] slots = caster.UISettings.Slots ?? new MechanicActionBarSlot[0];
            var pinOriginal = slots.Length > SpiderPinOriginalIndex ? slots[SpiderPinOriginalIndex] as MechanicActionBarSlotSpontaneusConvertedSpell : null;
            var pinGenerated = slots.Length > SpiderPinGeneratedIndex ? slots[SpiderPinGeneratedIndex] as MechanicActionBarSlotSpontaneusConvertedSpell : null;
            Func<MechanicActionBarSlotSpontaneusConvertedSpell, string, int, JObject> describe = (pin, guid, tier) => new JObject {
                ["present"] = pin != null, ["blueprint"] = pin?.Spell?.Blueprint?.AssetGuid, ["name"] = pin?.Spell?.Blueprint?.name, ["expected"] = guid,
                ["convertedFrom"] = pin?.Spell?.ConvertedFrom?.Blueprint?.AssetGuid, ["book"] = pin?.Spell?.Spellbook?.Blueprint?.AssetGuid,
                ["owner"] = ReferenceEquals(pin?.Unit, caster) && ReferenceEquals(pin?.Spell?.Caster, caster.Descriptor),
                ["visible"] = pin?.Spell?.Blueprint != null && pin.Spell.IsVisible(), ["available"] = pin?.Spell?.Blueprint != null && pin.Spell.IsAvailableForCast,
                ["inPublishedParent"] = pin?.Spell?.Blueprint != null && (SpiderPersistenceParent(tier).Variants ?? new BlueprintAbility[0]).Contains(pin.Spell.Blueprint) };
            JObject original = describe(pinOriginal, (string)saved["original"], 4), generated = describe(pinGenerated, (string)saved["generated"], 5);
            bool identities = pinOriginal?.Spell?.Blueprint != null && pinGenerated?.Spell?.Blueprint != null &&
                (string)original["blueprint"] == (string)saved["original"] && (string)generated["blueprint"] == (string)saved["generated"] &&
                (string)original["convertedFrom"] == (string)saved["known4"] && (string)generated["convertedFrom"] == (string)saved["known5"] &&
                (bool)original["owner"] && (bool)generated["owner"] && book != null && (string)original["book"] == book.Blueprint.AssetGuid && (string)generated["book"] == book.Blueprint.AssetGuid;
            bool menus = moduleOn ? (bool)original["inPublishedParent"] && (bool)generated["inPublishedParent"] && (bool)generated["visible"] && (bool)generated["available"]
                : (bool)original["inPublishedParent"] && !(bool)generated["inPublishedParent"] && !(bool)generated["available"];
            SpiderPersistenceCheck((moduleOn ? "on" : "off") + "-reloaded-shortcuts", identities && menus, new JObject { ["original"] = original, ["generated"] = generated, ["moduleEnabled"] = moduleOn },
                moduleOn ? "Both saved shortcuts resolve to their exact rows, children of their saved known spells in the Sorcerer book, both published and castable."
                    : "Both saved shortcuts resolve to registered rows, so no GUID dangles; the original row stays published while the generated row is in no published menu and cannot be cast.");

            // The active spider and its benefit after the reload: one marker, the saved DC, no second +2.
            JObject poison = SpiderPersistencePoison(summon, true);
            var lifetime = summon.Descriptor.Buffs.Enumerable.FirstOrDefault(value => ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
            int markers = SpiderPersistenceMarkers(summon);
            bool activeExact = summon.Blueprint.AssetGuid == SpiderSummonerCatalog.Find("giant-black-widow").UnitGuid && markers == 1 &&
                lifetime != null && !lifetime.IsPermanent && lifetime.TimeLeft > TimeSpan.Zero && lifetime.TimeLeft.TotalSeconds <= 66.5 &&
                ReferenceEquals(lifetime.MaybeContext?.MaybeCaster, caster) &&
                poison.Properties().All(value => (int)value.Value["facts"] == 1 && (int)value.Value["dc"] == (int)saved["poison"][value.Name]["dc"] &&
                    (int)value.Value["recalculatedDc"] == (int)saved["poison"][value.Name]["dc"]);
            SpiderPersistenceCheck((moduleOn ? "on" : "off") + "-reloaded-active-spider", activeExact, new JObject { ["unit"] = summon.Blueprint.AssetGuid, ["markers"] = markers,
                ["secondsLeft"] = lifetime == null ? -1 : lifetime.TimeLeft.TotalSeconds, ["poison"] = poison, ["savedPoison"] = saved["poison"], ["moduleEnabled"] = moduleOn },
                "The saved Black Widow is back with its finite summon lifetime from the caster, exactly one marker, and both poisons at the saved DC 16, also after recalculation, never 18."
                + (moduleOn ? "" : " With the module OFF it keeps what it was cast with; no new grant is made."));

            if (!moduleOn)
            {
                // The generated shortcut: refused, nothing spent, nothing summoned.
                JObject refusal = null;
                foreach (int frame in SpiderPersistenceCast(caster, pinGenerated.Spell, book, 5, DiceType.Zero, false, (record, units) => refusal = record)) yield return frame;
                bool refused = (string)refusal["result"] != "Success" && (bool)refusal["unspent"] && (int)refusal["count"] == 0 && (int)refusal["spawnActions"] == 0;
                SpiderPersistenceCheck("off-generated-shortcut-refused", refused, refusal,
                    "With the module OFF the saved generated shortcut casts nothing and spends nothing: its dormant row leaks no KMG choice.");
                // The original shortcut keeps Races Unleashed's own behavior: its creature, no benefit.
                JObject native = null;
                foreach (int frame in SpiderPersistenceCast(caster, pinOriginal.Spell, book, 4, DiceType.Zero, false, (record, units) => native = record)) yield return frame;
                var nativeUnits = native["units"] as JArray;
                bool originalBehavior = (string)native["result"] == "Success" && (bool)native["exactlyOnePoolUse"] && nativeUnits.Count == 1 &&
                    (string)nativeUnits[0]["unit"] == SpiderSummonerCatalog.Find("giant-black-widow").UnitGuid && (int)nativeUnits[0]["persistenceMarkers"] == 0 &&
                    (double)nativeUnits[0]["secondsLeft"] > 0 && (double)nativeUnits[0]["secondsLeft"] < 100000;
                SpiderPersistenceCheck("off-original-shortcut-native", originalBehavior, native,
                    "With the module OFF the saved original shortcut casts Races Unleashed's Black Widow as Races Unleashed defines it, with no benefit, spending exactly one level 4 use.");
                yield break;
            }

            // Module ON: cast from both saved shortcuts.
            JObject crab = null;
            foreach (int frame in SpiderPersistenceCast(caster, pinGenerated.Spell, book, 5, DiceType.D4, false, (record, units) => crab = record)) yield return frame;
            string crabUnits = SpiderRouteObservationVerdict.UnitsProblem(crab["units"] as JArray, 3, SpiderSummonerCatalog.Find("giant-crab-spider").UnitGuid, true);
            string crabDuration = SpiderDurationProblem(crab["summonRules"] as JArray, 11, false);
            crab["unitsProblem"] = SpiderEscapeObservationVerdict.Optional(crabUnits); crab["durationProblem"] = SpiderEscapeObservationVerdict.Optional(crabDuration);
            SpiderPersistenceCheck("on-generated-shortcut-cast", (string)crab["result"] == "Success" && (bool)crab["exactlyOnePoolUse"] && crabUnits == null && crabDuration == null, crab,
                "The reloaded generated shortcut casts 1d4+1 Crab Spiders (die forced to 2: three), each with one benefit and one template, 11 rounds at caster level 11, spending exactly one level 5 use.");
            JObject widow = null;
            foreach (int frame in SpiderPersistenceCast(caster, pinOriginal.Spell, book, 4, DiceType.Zero, false, (record, units) => widow = record)) yield return frame;
            string widowUnits = SpiderRouteObservationVerdict.UnitsProblem(widow["units"] as JArray, 1, SpiderSummonerCatalog.Find("giant-black-widow").UnitGuid, true);
            widow["unitsProblem"] = SpiderEscapeObservationVerdict.Optional(widowUnits);
            SpiderPersistenceCheck("on-original-shortcut-cast", (string)widow["result"] == "Success" && (bool)widow["exactlyOnePoolUse"] && widowUnits == null, widow,
                "The reloaded original shortcut casts one Black Widow with one benefit and one template, spending exactly one level 4 use.");
            int stillOne = SpiderPersistenceMarkers(summon);
            SpiderPersistenceCheck("on-no-duplicated-benefit", stillOne == 1, new JObject { ["savedSpiderMarkers"] = stillOne },
                "Casting again after the reload adds no second marker to the saved spider.");

            // The saved choices after the feat is lost: refused, nothing spent.
            Kingmaker.UnitLogic.Feature held = caster.Descriptor.Progression.Features.Enumerable.FirstOrDefault(value => ReferenceEquals(value?.Blueprint, feature));
            if (held != null) caster.Descriptor.RemoveFact(held);
            foreach (var stale in new[] { new { Key = "generated", Pin = pinGenerated, Level = 5 }, new { Key = "original", Pin = pinOriginal, Level = 4 } })
            {
                var scoped = stale; JObject attempt = null;
                foreach (int frame in SpiderPersistenceCast(caster, scoped.Pin.Spell, book, scoped.Level, DiceType.Zero, false, (record, units) => attempt = record)) yield return frame;
                bool refused = held != null && !SpiderHasFeature(caster, feature) && (string)attempt["result"] != "Success" && (bool)attempt["unspent"] && (int)attempt["count"] == 0;
                SpiderPersistenceCheck("on-saved-" + scoped.Key + "-shortcut-after-feat-loss", refused, attempt,
                    "After the feat is removed, the reloaded " + scoped.Key + " shortcut is refused with nothing spent and nothing summoned.");
            }

            // Retire the fixture, then the enabled cleanup save.
            var retire = new List<UnitEntityData> { summon, caster };
            retire.AddRange(Game.Instance.State.Units.All.Where(value => value != null && !value.Destroyed && ReferenceEquals(value.Descriptor.Master.Value, caster)));
            DisposeExpandedSummoningUnits(new List<UnitEntityData>(retire), retire.Distinct().ToArray());
            Game.Instance.EntityDestroyer.Tick();
            for (int frame = 0; frame < 6; frame++) yield return 0;
            UnitEntityData[] left = SpiderPersistenceOwned(null), marked = SpiderPersistenceMarked();
            SpiderPersistenceCheck("cleanup", left.Length == 0 && marked.Length == 0, new JObject { ["receiptUnits"] = left.Length, ["markedUnits"] = marked.Length },
                "The fixture caster, its pins and the saved spider are retired before the cleanup save, leaving no marked unit.");
        }

        private void CompleteSpiderPersistence(string status, string warning)
        {
            if (_spiderPersistenceDone) return;
            _spiderPersistenceDone = true;
            _spiderSummonRuleCapture = false;
            if (_spiderPersistencePause.HasValue) { Game.Instance.IsPaused = _spiderPersistencePause.Value; _spiderPersistencePause = null; }
            var evidence = _workingSaveSmoke.Stop();
            bool writes = SpiderSummonerPersistencePolicy.Writes(_request.Scenario, SpiderCheckpointPreserved);
            _spiderPersistenceChecks.Add(Assertion("exact-working-load", "exact working descriptor, distinct baseline",
                "working=" + evidence.WorkingMatchCount + ";baseline=" + evidence.BaselineMatchCount,
                evidence.WorkingMatchCount == 1 && evidence.BaselineMatchCount == 1 && evidence.DescriptorReferenceCorrelated, "native guarded load sentinel"));
            // A failure before the save arms writes nothing; a PASS must match the stage's write contract.
            bool saveExpected = writes && status == RuntimeTestStatuses.Pass;
            _spiderPersistenceChecks.Add(Assertion("exact-working-save-write", saveExpected ? "one native working save" : "no write",
                "count=" + evidence.ExpectedWorkingSaveRoutineCount + ";stashed=" + evidence.ExpectedWorkingStashedAreaCount + ";preserve=" + SpiderCheckpointPreserved,
                !evidence.SaveWritingApiObserved && evidence.ExpectedWorkingSaveRoutineCount == (saveExpected ? 1 : 0) &&
                    (!saveExpected || evidence.ExpectedWorkingStashedAreaCount >= 1), "exact descriptor-bound native save sentinel"));
            _spiderPersistenceChecks.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version, "loaded UMM version"));
            if (_spiderPersistenceChecks.Any(value => value.Status != RuntimeTestStatuses.Pass)) status = RuntimeTestStatuses.Fail;
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory, "spider-summoner-persistence.json"), _spiderPersistenceRows.ToString(Formatting.Indented));
            var result = CreateResult(status, _spiderPersistenceChecks, null); result.WorkingSaveSmoke = evidence;
            if (!string.IsNullOrWhiteSpace(warning)) result.Warnings.Add(warning);
            Complete(result);
        }
    }
}
