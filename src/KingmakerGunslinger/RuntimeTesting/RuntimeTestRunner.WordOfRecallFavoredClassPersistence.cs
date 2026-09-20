using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using UnityEngine;
using Kingmaker.UI;
using Kingmaker.UI.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Authorized disposable save-file persistence acceptance for the Word of
    // Recall Favored Class route. The prepare phase plays the genuine route on
    // a fresh request-local Aasimar Oracle, commits the award, registers the
    // unit in the traveling party and writes one new native manual save
    // through the guarded disposable lease. The verify phase runs in a fresh
    // process, reloads that exact save, and proves canonical Recall at Oracle
    // level 6, the granting parametrized feature and its Recall parameter,
    // correct award accounting against the installed rules, and one strategic
    // cast with the expected spontaneous-slot expenditure. The owner's live
    // campaign and the protected baseline save are never loaded or written.
    internal sealed partial class RuntimeTestRunner
    {
        private FcbPersistencePlan _fcbPersistencePlan;
        private IEnumerator<int> _fcbPersistenceSteps;
        private readonly List<RuntimeTestAssertion> _fcbPersistenceAssertions =
            new List<RuntimeTestAssertion>();
        private readonly List<object> _fcbPersistenceEvents = new List<object>();
        private JObject _fcbPersistenceExpected;
        private JObject _fcbPersistenceSaved;
        private string FcbPersistencePath
        { get { return Path.Combine(_request.EvidenceDirectory,
            "word-of-recall-favored-class-persistence.json"); } }

        private void FcbPersistenceAssert(string id, string expectation, bool pass,
            object evidence)
        {
            _fcbPersistenceEvents.Add(new { id, evidence });
            _fcbPersistenceAssertions.Add(Assertion(
                "word-of-recall-fcb-persistence-" + id, expectation,
                TeleportationDiagnosticJson.Serialize(evidence), pass,
                FcbPersistencePath));
            if (!pass)
                throw new InvalidOperationException(
                    "Favored Class persistence assertion failed: " + id);
        }

        private void PollFcbPersistence()
        {
            if (_fcbPersistencePlan == null || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete ||
                _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException(
                    "Favored Class persistence phase lacks its guarded exact input or intact write boundary.");
            if (_fcbPersistenceSteps == null)
                _fcbPersistenceSteps = RunFcbPersistence().GetEnumerator();
            Exception failure = null;
            try { if (_fcbPersistenceSteps.MoveNext()) return; }
            catch (Exception exception) { failure = exception; }
            try { _fcbPersistenceSteps.Dispose(); }
            catch (Exception exception)
            { failure = failure == null ? exception : new AggregateException(failure, exception); }
            _fcbPersistenceSteps = null;
            var combined = new List<RuntimeTestAssertion>(_fcbPersistenceAssertions);
            combined.AddRange(_teleportationSpellbookUiAssertions);
            WriteFcbPersistenceReceipt(failure == null ? null : failure.ToString());
            Complete(CreateResult(failure == null ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Error, combined,
                failure == null ? null : failure.ToString()));
        }

        private void WriteFcbPersistenceReceipt(string error)
        {
            WriteTeleportationForensicJson(FcbPersistencePath, new {
                schemaVersion = 1, runId = _request.RunId,
                transactionId = _fcbPersistencePlan.Transaction,
                phase = _fcbPersistencePlan.Phase,
                processId = Process.GetCurrentProcess().Id,
                dllSha256 = FcbPersistencePlan.Hash(
                    typeof(RuntimeTestRunner).Assembly.Location),
                input = _fcbPersistencePlan.Input,
                expected = _fcbPersistenceExpected,
                savedInfo = _fcbPersistenceSaved,
                events = _fcbPersistenceEvents,
                assertions = _fcbPersistenceAssertions, error });
        }

        private IEnumerable<int> RunFcbPersistence()
        {
            var plan = _fcbPersistencePlan;
            var game = Game.Instance; var player = game.Player;
            var oracle = TeleportationFinalLiveReconciler.ResolveOracleClass(
                BlueprintBootstrap.Library);
            var recall = BlueprintBootstrap.Teleportation.WordOfRecall;
            BlueprintScriptableObject raw;
            var bonusSelection = BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(
                FcbOracleBonusSpellSelectionId, out raw)
                ? raw as BlueprintFeatureSelection : null;
            var level6Feature = BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(
                FcbOracleLevel6FeatureId, out raw)
                ? raw as BlueprintParametrizedFeature : null;
            var partialFeature = BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(
                FcbOraclePartialFeatureId, out raw) ? raw as BlueprintFeature : null;
            if (oracle == null || bonusSelection == null || level6Feature == null ||
                partialFeature == null)
                throw new InvalidOperationException(
                    "The installed Favored Class Oracle route is incomplete.");
            if (!_context.FeatureModules.Active.TeleportationSpells ||
                BlueprintBootstrap.TeleportationPublication == null)
                throw new InvalidOperationException(
                    "Favored Class persistence requires the published teleportation module.");
            var aasimar = ResolveFcbRace(FcbAasimarRaceId, "Aasimar");
            var ui = game.UI;
            var presenter = ui.CharacterBuildController;
            var priorBackend = ui.LevelUpController;
            var priorPresenterUnit = presenter.Unit;
            var originalParty = player.Party.ToArray();
            var originalPartyRefs = player.PartyCharacters.ToArray();
            var originalCross = player.CrossSceneState.AllEntityData.ToArray();
            var originalItems = player.Inventory.Items.ToArray();
            var originalCounts = originalItems.Select(value => value.Count).ToArray();
            long originalMoney = player.Money;
            bool originalPause = game.IsPaused;
            var starterDeltas = new Dictionary<Kingmaker.Items.ItemEntity, int>();
            Action<string> captureStarterItems = stage => {
                foreach (var item in player.Inventory.Items)
                {
                    int index = Array.IndexOf(originalItems, item);
                    int delta = item.Count - (index < 0 ? 0 : originalCounts[index]);
                    if (delta > 0) starterDeltas[item] = delta;
                }
            };
            var experienceProperty = typeof(UnitProgressionData).GetProperty("Experience");
            UnitEntityData unit = null;
            LevelUpController backend = null;
            var trace = new List<object>();
            Application.logMessageReceived += ObserveTeleportSpellbookUiException;
            try
            {
                game.IsPaused = true;
                if (plan.Phase == "prepare")
                {
                    var anchor = originalParty.First(value => value.View != null &&
                        value.Descriptor.Progression.Race != null);
                    unit = SpawnFcbOracleFixture(oracle, aasimar,
                        "KMG FCB Persistence Oracle", anchor, player, out backend, 13, trace);
                    var book = unit.Descriptor.GetSpellbook(oracle.Spellbook);
                    string[] knownSixthBefore = book.GetKnownSpells(6).Select(value =>
                        value.Blueprint.AssetGuid).ToArray();
                    int allowanceThirteenth = book.Blueprint.SpellsKnown.GetCount(13, 6) ?? 0;
                    int allowanceFourteenth = book.Blueprint.SpellsKnown.GetCount(14, 6) ?? 0;
                    experienceProperty.SetValue(unit.Descriptor.Progression,
                        game.BlueprintRoot.Progression.XPTable.GetBonus(14), null);
                    int successes = 0;
                    presenter.HandleLevelUpStart(unit.Descriptor, null, () => successes++);
                    backend = presenter.LevelUpController;
                    if (backend == null || backend.AutoCommit ||
                        ReferenceEquals(backend.Preview, unit.Descriptor))
                        throw new InvalidOperationException(
                            "Persistence prepare level-up did not create an independent preview.");
                    for (int frame = 0; frame < 15; frame++) yield return 0;
                    presenter.SetClass(oracle);
                    foreach (int tick in OpenFcbAwardAndSelectRecall(oracle,
                        bonusSelection, level6Feature, recall, "persistence-prepare")) yield return tick;
                    FillFcbOracleChoices(backend, oracle, trace);
                    foreach (int tick in WaitTeleportLevelUpUi(() => backend.State.IsComplete(),
                        "persistence prepare choices complete")) yield return tick;
                    presenter.Next();
                    foreach (int tick in WaitTeleportLevelUpUi(() =>
                        presenter.CurrentPhase == CharBPhase.Type.Total,
                        "persistence prepare summary")) yield return tick;
                    var finish = (UnityEngine.UI.Button)typeof(CharacterBuildController)
                        .GetField("m_CompleteButton", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(presenter);
                    if (finish == null || !finish.interactable || !backend.State.IsComplete())
                        throw new InvalidOperationException(
                            "The persistence prepare completion button is unavailable.");
                    finish.onClick.Invoke(); backend = null;
                    foreach (int tick in WaitTeleportLevelUpUi(() => !presenter.IsShow,
                        "persistence prepare committed")) yield return tick;
                    ui.LevelUpController = priorBackend;
                    book = unit.Descriptor.GetSpellbook(oracle.Spellbook);
                    var grantFacts = unit.Descriptor.Progression.Features.Enumerable
                        .Where(value => ReferenceEquals(value.Blueprint, level6Feature)).ToArray();
                    var partialFacts = unit.Descriptor.Progression.Features.Enumerable
                        .Where(value => ReferenceEquals(value.Blueprint, partialFeature)).ToArray();
                    string[] knownSixth = book.GetKnownSpells(6).Select(value =>
                        value.Blueprint.AssetGuid).ToArray();
                    FcbPersistenceAssert("prepare-committed",
                        "the genuine favored-class award commits canonical Recall once at Oracle 6 before saving",
                        unit.Descriptor.Progression.GetClassLevel(oracle) == 14 &&
                            book.CasterLevel == 14 && successes == 1 &&
                            book.GetKnownSpells(6).Count(value =>
                                ReferenceEquals(value.Blueprint, recall)) == 1 &&
                            grantFacts.Length == 1 && grantFacts.Count(value =>
                                value.Param != null && value.Param.Value != null &&
                                ReferenceEquals(value.Param.Value.Blueprint, recall)) == 1 &&
                            partialFacts.Length == 1 && partialFacts[0].Rank == 1,
                        new { classLevel = unit.Descriptor.Progression.GetClassLevel(oracle),
                            casterLevel = book.CasterLevel, callbacks = successes,
                            knownSixth, grants = grantFacts.Length,
                            partialRank = partialFacts.Length == 0 ? 0 : partialFacts[0].Rank });
                    // The committed unit must be part of the saved traveling party.
                    player.PartyCharacters.Add(unit);
                    player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                    if (!player.Party.Contains(unit))
                        throw new InvalidOperationException(
                            "The persistence Oracle did not enter the traveling party.");
                    _fcbPersistenceExpected = new JObject {
                        ["unitId"] = unit.UniqueId,
                        ["unitName"] = "KMG FCB Persistence Oracle",
                        ["classId"] = oracle.AssetGuid,
                        ["bookId"] = book.Blueprint.AssetGuid,
                        ["classLevel"] = unit.Descriptor.Progression.GetClassLevel(oracle),
                        ["casterLevel"] = book.CasterLevel,
                        ["knownSixth"] = new JArray(knownSixth),
                        ["recallId"] = recall.AssetGuid,
                        ["level6FeatureId"] = level6Feature.AssetGuid,
                        ["partialId"] = partialFeature.AssetGuid,
                        ["partialRank"] = partialFacts.Length == 0 ? 0 : partialFacts[0].Rank,
                        ["allowanceThirteenth"] = allowanceThirteenth,
                        ["allowanceFourteenth"] = allowanceFourteenth,
                        ["knownSixthBefore"] = new JArray(knownSixthBefore) };
                    foreach (int tick in SaveFcbPersistence(game, plan)) yield return tick;
                    // The disposable save is the artifact; the live process
                    // restores its request-owned membership and disposes the
                    // fixture exactly like the acceptance scenario.
                    player.PartyCharacters.RemoveAll(value => value.UniqueId == unit.UniqueId);
                    if (unit.HoldingState != null &&
                        unit.HoldingState.AllEntityData.Contains(unit))
                        unit.HoldingState.RemoveEntityData(unit);
                    player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                    captureStarterItems("persistence-prepare-starter-items");
                    foreach (var entry in starterDeltas)
                        if (ReferenceEquals(entry.Key.Collection, player.Inventory) &&
                            entry.Key.Count >= entry.Value)
                            player.Inventory.Remove(entry.Key, entry.Value);
                    unit.Dispose(); unit = null;
                }
                else
                {
                    string unitId = (string)plan.Expected["unitId"];
                    var classId = (string)plan.Expected["classId"];
                    var bookId = (string)plan.Expected["bookId"];
                    string recallId = (string)plan.Expected["recallId"];
                    string level6Id = (string)plan.Expected["level6FeatureId"];
                    unit = player.Party.SingleOrDefault(value =>
                        value.UniqueId == unitId) ??
                        game.State.Units.SingleOrDefault(value => value.UniqueId == unitId);
                    if (unit == null)
                        throw new InvalidOperationException(
                            "The saved Favored Class Oracle is absent from the reloaded save.");
                    var loadedClass = BlueprintBootstrap.Library.BlueprintsByAssetId[
                        classId] as BlueprintCharacterClass;
                    var book = unit.Descriptor.GetSpellbook(loadedClass.Spellbook);
                    var grantFacts = unit.Descriptor.Progression.Features.Enumerable
                        .Where(value => value.Blueprint != null &&
                            value.Blueprint.AssetGuid == level6Id).ToArray();
                    var partialFacts = unit.Descriptor.Progression.Features.Enumerable
                        .Where(value => value.Blueprint != null &&
                            value.Blueprint.AssetGuid == (string)plan.Expected["partialId"]).ToArray();
                    string[] knownSixth = book.GetKnownSpells(6).Select(value =>
                        value.Blueprint.AssetGuid).ToArray();
                    string[] expectedKnown = ((JArray)plan.Expected["knownSixth"])
                        .Select(value => (string)value).OrderBy(value => value,
                            StringComparer.Ordinal).ToArray();
                    int allowanceFourteenth = (int)plan.Expected["allowanceFourteenth"];
                    string[] expectedBefore = ((JArray)plan.Expected["knownSixthBefore"])
                        .Select(value => (string)value).ToArray();
                    FcbPersistenceAssert("reload-unit-present",
                        "the fresh-process reload restores the exact saved Favored Class Oracle",
                        unit.CharacterName == (string)plan.Expected["unitName"] &&
                            ReferenceEquals(book.Blueprint,
                                BlueprintBootstrap.Library.BlueprintsByAssetId[bookId]),
                        new { unitId = unit.UniqueId, name = unit.CharacterName,
                            book = book.Blueprint.AssetGuid });
                    FcbPersistenceAssert("reload-recall-known",
                        "canonical Recall persists at Oracle level 6 exactly once through the save-file reload",
                        book.CasterLevel == (int)plan.Expected["casterLevel"] &&
                            unit.Descriptor.Progression.GetClassLevel(loadedClass) ==
                                (int)plan.Expected["classLevel"] &&
                            knownSixth.Count(value => string.Equals(value, recallId,
                                StringComparison.Ordinal)) == 1,
                        new { casterLevel = book.CasterLevel,
                            classLevel = unit.Descriptor.Progression.GetClassLevel(loadedClass),
                            knownSixth });
                    FcbPersistenceAssert("reload-grant-fact",
                        "the granting parametrized feature and its Recall parameter persist exactly once",
                        grantFacts.Length == 1 && grantFacts[0].Param != null &&
                            grantFacts[0].Param.Value != null &&
                            string.Equals(grantFacts[0].Param.Value.Blueprint.AssetGuid,
                                recallId, StringComparison.Ordinal) &&
                            partialFacts.Length == 1 &&
                            partialFacts[0].Rank == (int)plan.Expected["partialRank"],
                        new { grants = grantFacts.Length,
                            param = grantFacts.Length != 1 || grantFacts[0].Param == null ||
                                grantFacts[0].Param.Value == null ||
                                grantFacts[0].Param.Value.Blueprint == null ? null :
                                    grantFacts[0].Param.Value.Blueprint.AssetGuid,
                            partialRank = partialFacts.Length == 0 ? 0 : partialFacts[0].Rank });
                    string[] expectedNew = expectedKnown.Except(expectedBefore,
                        StringComparer.Ordinal).ToArray();
                    FcbPersistenceAssert("reload-award-accounting",
                        "the reloaded known spells equal the prepare snapshot: installed at-level allowance plus exactly the one favored-class grant",
                        knownSixth.OrderBy(value => value, StringComparer.Ordinal)
                            .SequenceEqual(expectedKnown, StringComparer.Ordinal) &&
                            expectedNew.Length == allowanceFourteenth + 1 &&
                            expectedNew.Count(value => string.Equals(value, recallId,
                                StringComparison.Ordinal)) == 1,
                        new { knownSixth, expectedKnown, expectedNew, allowanceFourteenth });
                    // Strategic cast from the reloaded save: exactly one
                    // sixth-level spontaneous slot, no scroll substitution.
                    game.LoadArea(game.BlueprintRoot.GlobalMap.GlobalMapEnterPoint,
                        Kingmaker.EntitySystem.Persistence.AutoSaveMode.None);
                    var watch = Stopwatch.StartNew();
                    while (Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingInProcess ||
                        Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingScreenActive ||
                        Kingmaker.Globalmap.GlobalMapRules.Instance == null ||
                        game.CurrentMode != GameModeType.GlobalMap)
                    {
                        if (watch.Elapsed.TotalSeconds > 60)
                            throw new InvalidOperationException(
                                "Persistence verify world-map load timed out.");
                        yield return 0;
                    }
                    foreach (int tick in CastNewlyLearnedOracleRecall(unit, book))
                        yield return tick;
                    FcbPersistenceAssert("verify-write-guard",
                        "the verify process performs no save writes",
                        !_workingSaveSmoke.WriteObserved,
                        new { writeObserved = _workingSaveSmoke.WriteObserved });
                }
            }
            finally
            {
                if (backend != null)
                {
                    if (presenter.IsShow &&
                        ReferenceEquals(presenter.LevelUpController, backend))
                        presenter.Show(false);
                    backend.Cancel();
                }
                ui.LevelUpController = priorBackend; presenter.Unit = priorPresenterUnit;
                if (unit != null && plan.Phase == "verify")
                {
                    // The verify process leaves the loaded save untouched; the
                    // reloaded fixture belongs to the disposable save, so it is
                    // only detached from this process, never rewritten.
                    player.PartyCharacters.RemoveAll(value =>
                        value.UniqueId == unit.UniqueId);
                }
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                game.IsPaused = originalPause;
                Application.logMessageReceived -= ObserveTeleportSpellbookUiException;
                bool restored = plan.Phase == "prepare" &&
                    originalParty.SequenceEqual(player.Party) &&
                    originalPartyRefs.SequenceEqual(player.PartyCharacters) &&
                    originalCross.SequenceEqual(player.CrossSceneState.AllEntityData) &&
                    originalItems.SequenceEqual(player.Inventory.Items) &&
                    originalCounts.SequenceEqual(originalItems.Select(value => value.Count)) &&
                    player.Money == originalMoney && !_workingSaveSmoke.WriteObserved;
                _fcbPersistenceEvents.Add(new { id = "cleanup", evidence = new {
                    restored, exceptions = _teleportationSpellbookUiExceptions.Count } });
                if (plan.Phase == "prepare")
                    FcbPersistenceAssert("cleanup",
                        "prepare restores party, cross-scene entities, inventory and money with only the leased save written",
                        restored && _teleportationSpellbookUiExceptions.Count == 0,
                        new { restored,
                            exceptions = _teleportationSpellbookUiExceptions.Count });
                else if (_teleportationSpellbookUiExceptions.Count != 0)
                    throw new InvalidOperationException(
                        "The persistence verify process observed native or mod exceptions.");
            }
        }

        private IEnumerable<int> SaveFcbPersistence(Game game, FcbPersistencePlan plan)
        {
            if (!game.SaveManager.IsSaveAllowed() || game.SaveManager.CommitInProgress ||
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance.OnlyOneSave.CurrentValue)
                throw new InvalidOperationException(
                    "The exact new native manual save is not permitted in this state.");
            var requested = game.SaveManager.CreateNewSave(plan.OutputName);
            var lease = new GuardedDisposableSaveLease(requested, plan.OutputName,
                game.SaveManager.SavePath, save =>
                {
                    WriteTeleportationForensicJson(Path.Combine(
                        _request.EvidenceDirectory,
                        "word-of-recall-fcb-persistence-owned-save.json"),
                        new { schemaVersion = 1, runId = _request.RunId,
                            transactionId = plan.Transaction, phase = plan.Phase,
                            name = save.Name, file = save.FileName, path = save.FolderName,
                            gameId = save.GameId, existedBeforePreparation = false,
                            lifecycle = "native-prepared-before-write" });
                });
            _workingSaveSmoke.ArmDisposableSave(lease);
            bool completed = false;
            game.SaveGame(requested, () => { completed = true; });
            var watch = Stopwatch.StartNew();
            while (!completed || lease.Saved == null || game.SaveManager.CommitInProgress ||
                lease.Saved.OperationState !=
                    Kingmaker.EntitySystem.Persistence.SaveInfo.StateType.None ||
                Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingInProcess ||
                !File.Exists(lease.Saved.FolderName))
            {
                if (watch.Elapsed.TotalSeconds > 90)
                    throw new InvalidOperationException(
                        "Native disposable save did not complete its exact disk commit.");
                yield return 0;
            }
            var saved = lease.Saved;
            JObject header; JToken party;
            using (var reader = saved.Saver.Clone())
            {
                header = JObject.Parse(reader.ReadHeader());
                party = JToken.Parse(reader.ReadJson("party"));
            }
            string unitId = (string)_fcbPersistenceExpected["unitId"];
            string recallId = (string)_fcbPersistenceExpected["recallId"];
            string level6Id = (string)_fcbPersistenceExpected["level6FeatureId"];
            var unitToken = FindFcbPersistenceUnit(party, unitId);
            string unitText = unitToken == null ? "" :
                unitToken.ToString(Newtonsoft.Json.Formatting.None);
            FcbPersistenceAssert("disk-payload",
                "the native campaign save serializes the favored-class grant feature, its Recall parameter and the learned spell inside the saved unit",
                unitToken != null && unitText.Contains(level6Id) && unitText.Contains(recallId),
                new { unitFound = unitToken != null,
                    containsLevel6 = unitText.Contains(level6Id),
                    containsRecall = unitText.Contains(recallId) });
            FcbPersistenceAssert("disk-header",
                "the completed new native manual save carries the exact campaign identity",
                (string)header["Name"] == plan.OutputName &&
                    (string)header["GameId"] == game.Player.GameId &&
                    saved.PartyPortraits.Count == game.Player.Party.Count &&
                    saved.LoadedTimes == 0,
                new { header, path = saved.FolderName, name = saved.Name });
            _fcbPersistenceSaved = new JObject {
                ["name"] = saved.Name, ["file"] = saved.FileName,
                ["path"] = saved.FolderName,
                ["sha256"] = FcbPersistencePlan.Hash(saved.FolderName),
                ["gameName"] = saved.GameName, ["gameId"] = saved.GameId,
                ["areaName"] = saved.Area.name,
                ["partyCount"] = saved.PartyPortraits.Count };
            _fcbPersistenceEvents.Add(new { id = "saved", evidence = _fcbPersistenceSaved });
            WriteFcbPersistenceReceipt(null);
        }

        private static JToken FindFcbPersistenceUnit(JToken party, string unitId)
        {
            return ((JContainer)party).DescendantsAndSelf().OfType<JObject>()
                .FirstOrDefault(value => ((string)value["UniqueId"] ??
                    (string)value["m_UniqueId"] ?? "") == unitId);
        }
    }
}
