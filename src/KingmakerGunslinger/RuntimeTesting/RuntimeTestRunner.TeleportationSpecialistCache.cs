using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.UI;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _teleportationSpecialistCacheSteps;
        private readonly List<RuntimeTestAssertion> _teleportationSpecialistCacheAssertions = new List<RuntimeTestAssertion>();
        private readonly List<object> _teleportationSpecialistCacheCaptures = new List<object>();
        private bool IsTeleportationSpecialistCacheFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationSpecialistCache; } }

        // The owner's defect, reproduced and repaired through native seams only:
        // a real Conjuration-specialist book whose Teleport knowledge predates
        // the Conjuration-list publication keeps a stale special-spell cache, so
        // the native favorite slot refuses the spell. The production load seam
        // (the Harmony postfix on the public Spellbook.PostLoad) restores the
        // membership native would have cached, for both spell levels
        // independently, and the native spellbook UI/eligibility boundary
        // accepts, prepares, rests and counts the favorite uses again.
        private IEnumerable<int> RunTeleportationSpecialistCache()
        {
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            if (!_context.FeatureModules.Active.TeleportationSpells || !TeleportSpecialistSpellCachePatches.Installed ||
                BlueprintBootstrap.Teleportation == null || game.IsControllerGamepad ||
                game.CurrentMode != GameModeType.Default || ui.SpellBookController == null || ui.ServiceWindow == null ||
                GroupController.Instance == null || ui.SelectionManagerPC == null)
                throw new InvalidOperationException("Native specialist-cache qualification prerequisites differ.");
            var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "ba34257984f4c41408ce1dc2004e342e", "native specialist Wizard class");
            var conjurationFeature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                "cee0f7edbd874a042952ee150f878b84", "native Conjuration specialization feature");
            var evocationFeature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                "c46512b796216b64899f26301241e4e6", "native Evocation specialization feature");
            var coneOfCold = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                "e7c530f8137630f4d9d7ee1aa7b1edc0", "native Evocation level-5 control spell");
            var teleport = BlueprintBootstrap.Teleportation.Teleport;
            var greaterTeleport = BlueprintBootstrap.Teleportation.GreaterTeleport;
            var conjurationList = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(BlueprintBootstrap.Library,
                TeleportationSpellListPublication.ConjurationListId, "Conjuration special list");
            var evocationList = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(BlueprintBootstrap.Library,
                "79e731172a2dc1f4d92ba229c6216502", "Evocation special list");
            var owners = player.Party.Where(value => TeleportationSpellbookAdapter.CasterAvailable(value) && value.View != null && value.IsDirectlyControllable &&
                value.Descriptor.GetSpellbook(wizard.Spellbook) == null).Take(3).ToArray();
            if (owners.Length != 3) throw new InvalidOperationException("Three available fixture owners are required.");
            var conjurer = owners[0]; var evoker = owners[1]; var blank = owners[2];
            var originalSelection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var originalParty = player.Party.ToArray();
            bool originalPaused = game.IsPaused;
            var uiSnapshots = originalParty.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var fixtureConjurer = new TeleportResourceFixtureOwner(conjurer);
            var fixtureEvoker = new TeleportResourceFixtureOwner(evoker);
            var fixtureBlank = new TeleportResourceFixtureOwner(blank);
            bool conjurerFeatureAttached = false, evokerFeatureAttached = false, blankFeatureAttached = false;
            // In-memory Conjuration-list rollback fixture: the exact publication
            // mutation reversed, so the stale-cache order can be reproduced.
            var cache = typeof(SpellLevelList).GetField("m_SpellsFiltered", BindingFlags.Instance | BindingFlags.NonPublic);
            var conjurationLevels = new[] { 5, 7 }.Select(level => conjurationList.SpellsByLevel.Single(value => value.SpellLevel == level)).ToArray();
            var originalSpells = conjurationLevels.Select(value => value.Spells).ToArray();
            var originalCaches = conjurationLevels.Select(value => cache.GetValue(value)).ToArray();
            try
            {
                game.IsPaused = true;
                var bookConjurer = fixtureConjurer.AddBook(wizard.Spellbook);
                var bookEvoker = fixtureEvoker.AddBook(wizard.Spellbook);
                var bookBlank = fixtureBlank.AddBook(wizard.Spellbook);
                // STALE ORDER (the owner's save): the school feature attached and
                // the spells were learned while the published Conjuration list did
                // NOT yet contain them, exactly like knowledge that predates the
                // publication. Native AddKnown's special branch no-ops.
                for (int index = 0; index < conjurationLevels.Length; index++)
                {
                    conjurationLevels[index].Spells = conjurationLevels[index].Spells
                        .Where(value => value != teleport && value != greaterTeleport).ToList();
                    cache.SetValue(conjurationLevels[index], null);
                }
                conjurer.Descriptor.AddFact(conjurationFeature); conjurerFeatureAttached = true;
                evoker.Descriptor.AddFact(evocationFeature); evokerFeatureAttached = true;
                blank.Descriptor.AddFact(conjurationFeature); blankFeatureAttached = true;
                bookConjurer.AddSpecialList(conjurationList);
                bookEvoker.AddSpecialList(evocationList);
                // A genuine Conjuration specialist that never knows either
                // strategic spell: the direct knowledge boundary.
                bookBlank.AddSpecialList(conjurationList);
                bookConjurer.AddKnown(5, teleport, true); bookConjurer.AddKnown(5, coneOfCold, true);
                bookConjurer.AddKnown(7, greaterTeleport, true);
                bookEvoker.AddKnown(5, teleport, true);
                bookBlank.AddKnown(5, coneOfCold, true); // never knows either strategic spell
                foreach (var book in new[] { bookConjurer, bookEvoker, bookBlank }) { book.UpdateAllSlotsSize(false); book.Rest(); }
                // Publication restored to the production state BEFORE the load seam.
                for (int index = 0; index < conjurationLevels.Length; index++)
                {
                    conjurationLevels[index].Spells = originalSpells[index];
                    cache.SetValue(conjurationLevels[index], originalCaches[index]);
                }
                var conjurerFavorite5 = RawSlots(bookConjurer, 5).SingleOrDefault(value => value.Type == SpellSlotType.Favorite);
                var conjurerFavorite7 = RawSlots(bookConjurer, 7).SingleOrDefault(value => value.Type == SpellSlotType.Favorite);
                var evokerFavorite5 = RawSlots(bookEvoker, 5).SingleOrDefault(value => value.Type == SpellSlotType.Favorite);
                if (conjurerFavorite5 == null || conjurerFavorite7 == null || evokerFavorite5 == null)
                    throw new InvalidOperationException("Native school specializations did not create their favorite slots.");
                bool staleTeleport = !bookConjurer.PosibleMemorize(new AbilityData(teleport, bookConjurer), conjurerFavorite5);
                bool staleGreater = !bookConjurer.PosibleMemorize(new AbilityData(greaterTeleport, bookConjurer), conjurerFavorite7);
                CaptureTeleportationSpecialistCache("stale-state", new {
                    special5 = bookConjurer.GetSpecialSpells(5).Select(value => value.Blueprint.name).ToArray(),
                    special7 = bookConjurer.GetSpecialSpells(7).Select(value => value.Blueprint.name).ToArray(),
                    listContainsTeleport5 = conjurationList.GetSpells(5).Contains(teleport),
                    listContainsGreater7 = conjurationList.GetSpells(7).Contains(greaterTeleport),
                    knowsTeleport = bookConjurer.IsKnown(teleport), knowsGreater = bookConjurer.IsKnown(greaterTeleport),
                    reconciliationInstalled = TeleportSpecialistSpellCachePatches.Installed });
                SpecialistCacheAssert("stale-favorite-rejects-both-levels",
                    "the owner's defect: a stale special-spell cache refuses Teleport at 5 and Greater Teleport at 7 in their favorite slots",
                    "teleport5=" + staleTeleport + ";greater7=" + staleGreater, staleTeleport && staleGreater);
                // PRODUCTION REPAIR SEAM: the public native load method, patched
                // by the production Harmony postfix. No membership is attached
                // by this fixture.
                var postLoad = typeof(Spellbook).GetMethod("PostLoad", Type.EmptyTypes);
                postLoad.Invoke(bookConjurer, null);
                postLoad.Invoke(bookEvoker, null);
                postLoad.Invoke(bookBlank, null);
                bool repairedTeleport = bookConjurer.GetSpecialSpells(5).Any(value => value.Blueprint == teleport) &&
                    bookConjurer.PosibleMemorize(new AbilityData(teleport, bookConjurer), conjurerFavorite5) &&
                    bookConjurer.IsSpellSpecial(new AbilityData(teleport, bookConjurer));
                bool repairedGreater = bookConjurer.GetSpecialSpells(7).Any(value => value.Blueprint == greaterTeleport) &&
                    bookConjurer.PosibleMemorize(new AbilityData(greaterTeleport, bookConjurer), conjurerFavorite7) &&
                    bookConjurer.IsSpellSpecial(new AbilityData(greaterTeleport, bookConjurer));
                CaptureTeleportationSpecialistCache("after-postload", new {
                    special5 = bookConjurer.GetSpecialSpells(5).Select(value => value.Blueprint.name).ToArray(),
                    special7 = bookConjurer.GetSpecialSpells(7).Select(value => value.Blueprint.name).ToArray(),
                    evokerSpecial5 = bookEvoker.GetSpecialSpells(5).Select(value => value.Blueprint.name).ToArray(),
                    blankSpecial5 = bookBlank.GetSpecialSpells(5).Select(value => value.Blueprint.name).ToArray(),
                    blankKnowsTeleport = bookBlank.IsKnown(teleport), blankKnowsGreater = bookBlank.IsKnown(greaterTeleport) });
                SpecialistCacheAssert("postload-restores-both-levels",
                    "the production load seam restores special membership and native favorite acceptance for Teleport@5 and GreaterTeleport@7 independently",
                    "teleport5=" + repairedTeleport + ";greater7=" + repairedGreater, repairedTeleport && repairedGreater);
                SpecialistCacheAssert("postload-negative-controls",
                    "the Evocation book gains nothing, the Conjuration favorite still refuses non-Conjuration spells, and a genuine Conjuration specialist that never knew the spells auto-learns nothing",
                    "evoker=" + bookEvoker.GetSpecialSpells(5).Any(value => value.Blueprint == teleport) +
                        ";coneRejected=" + !bookConjurer.PosibleMemorize(new AbilityData(coneOfCold, bookConjurer), conjurerFavorite5) +
                        ";blankKnowsTeleport=" + bookBlank.IsKnown(teleport) + ";blankKnowsGreater=" + bookBlank.IsKnown(greaterTeleport),
                    !bookEvoker.GetSpecialSpells(5).Any(value => value.Blueprint == teleport) &&
                    !bookConjurer.PosibleMemorize(new AbilityData(coneOfCold, bookConjurer), conjurerFavorite5) &&
                    !bookEvoker.PosibleMemorize(new AbilityData(teleport, bookEvoker), evokerFavorite5) &&
                    !bookBlank.IsKnown(teleport) && !bookBlank.IsKnown(greaterTeleport) &&
                    !bookBlank.GetSpecialSpells(5).Any(value => value.Blueprint == teleport));
                // Idempotence: a second load seam adds no duplicates.
                int specialCount = bookConjurer.GetSpecialSpells(5).Count(value => value.Blueprint == teleport) +
                    bookConjurer.GetSpecialSpells(7).Count(value => value.Blueprint == greaterTeleport);
                postLoad.Invoke(bookConjurer, null);
                SpecialistCacheAssert("postload-idempotent", "a repeated load seam never duplicates special membership",
                    "before=" + specialCount + ";after=" + (bookConjurer.GetSpecialSpells(5).Count(value => value.Blueprint == teleport) +
                    bookConjurer.GetSpecialSpells(7).Count(value => value.Blueprint == greaterTeleport)),
                    specialCount == 2 && bookConjurer.GetSpecialSpells(5).Count(value => value.Blueprint == teleport) == 1 &&
                    bookConjurer.GetSpecialSpells(7).Count(value => value.Blueprint == greaterTeleport) == 1);
                // Native spellbook UI boundary for BOTH levels: prepare through
                // the controller the drag-drop path uses, rest, and count mixed
                // favorite/ordinary preparations exactly.
                ui.SelectionManagerPC.SelectUnit(conjurer.View, true, true, false);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                ui.SelectionManagerPC.SelectUnit(conjurer.View, true, true, false);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                var controller = ui.SpellBookController;
                if (!controller.IsShow) throw new InvalidOperationException("Native spellbook did not open for the specialist.");
                GroupController.Instance.SelectUnit(conjurer);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                for (int frame = 0; frame < 30 && !ReferenceEquals(controller.CurrentSpellbook, bookConjurer); frame++) yield return 0;
                if (!ReferenceEquals(controller.CurrentSpellbook, bookConjurer))
                    throw new InvalidOperationException("Native spellbook did not select the specialist book.");
                foreach (var level in new[] { 5, 7 })
                {
                    var spell = level == 5 ? teleport : greaterTeleport;
                    controller.SpellBookView.LevelBookSwitcher.m_Tabs[level].Toggle.isOn = true;
                    if (controller.CurrentBookLevel != level)
                        throw new InvalidOperationException("Native level tab did not select level " + level + ".");
                    Kingmaker.UI.ServiceWindow.SpellItem row = null;
                    for (int page = 0; page < 30; page++)
                    {
                        row = controller.SpellBookView.GetComponentsInChildren<Kingmaker.UI.ServiceWindow.SpellItem>(true).SingleOrDefault(value =>
                            value.gameObject.activeInHierarchy && value.SpellData != null && value.SpellData.Blueprint == spell);
                        if (row != null) break;
                        controller.SpellBookView.GoNextPage();
                        if (controller.CurrentBookLevel != level) break;
                    }
                    if (row == null) throw new InvalidOperationException("Native spellbook pagination has no level-" + level + " specialist row.");
                    var displayedFavorite = controller.GetComponentsInChildren<Kingmaker.UI.ServiceWindow.SpellSlotItem>(true)
                        .SingleOrDefault(value => value.gameObject.activeInHierarchy && value.MechanicSlot ==
                            (level == 5 ? conjurerFavorite5 : conjurerFavorite7));
                    if (displayedFavorite == null)
                        throw new InvalidOperationException("Native memorize panel does not display the level-" + level + " favorite slot.");
                    controller.MemorizeWithSound(row.SpellData, level == 5 ? conjurerFavorite5 : conjurerFavorite7);
                    row.Memorize();
                    for (int frame = 0; frame < 4; frame++) yield return 0;
                    var mixed = RawSlots(bookConjurer, level).Where(value => value.Spell != null && value.Spell.Blueprint == spell).ToArray();
                    SpecialistCacheAssert("favorite-mixed-count-level-" + level,
                        "favorite plus one ordinary preparation count exactly two unready preparations at level " + level,
                        "count=" + mixed.Length + ";types=" + string.Join(",", mixed.Select(value => value.Type.ToString()).ToArray()),
                        mixed.Length == 2 && mixed.Count(value => value.Type == SpellSlotType.Favorite) == 1 && mixed.All(value => !value.Available));
                }
                bookConjurer.Rest();
                SpecialistCacheAssert("rest-readies-both-favorite-levels",
                    "native rest readies the favorite and ordinary preparations at both levels without duplicating slots",
                    "ready5=" + RawSlots(bookConjurer, 5).Count(value => value.Spell != null && value.Spell.Blueprint == teleport && value.Available) +
                        ";ready7=" + RawSlots(bookConjurer, 7).Count(value => value.Spell != null && value.Spell.Blueprint == greaterTeleport && value.Available),
                    RawSlots(bookConjurer, 5).Count(value => value.Spell != null && value.Spell.Blueprint == teleport) == 2 &&
                    RawSlots(bookConjurer, 7).Count(value => value.Spell != null && value.Spell.Blueprint == greaterTeleport) == 2 &&
                    RawSlots(bookConjurer, 5).Count(value => value.Spell != null && value.Spell.Blueprint == teleport && value.Available) == 2 &&
                    RawSlots(bookConjurer, 7).Count(value => value.Spell != null && value.Spell.Blueprint == greaterTeleport && value.Available) == 2);
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                // World-map phase: spend the REPAIRED favorite-only preparations
                // at both levels through the real cast boundaries — Greater
                // Teleport settles directly, ordinary Teleport through its
                // confirmation — proving the reconciled slots are not merely
                // displayable but genuinely spendable.
                foreach (var ordinary5 in RawSlots(bookConjurer, 5).Where(value =>
                    value.Spell != null && value.Spell.Blueprint == teleport && value.Type == SpellSlotType.Common).ToArray())
                    bookConjurer.ForgetMemorized(ordinary5);
                foreach (var ordinary7 in RawSlots(bookConjurer, 7).Where(value =>
                    value.Spell != null && value.Spell.Blueprint == greaterTeleport && value.Type == SpellSlotType.Common).ToArray())
                    bookConjurer.ForgetMemorized(ordinary7);
                bookConjurer.Rest();
                var onlyFavorites = RawSlots(bookConjurer, 5).Count(value => value.Spell != null && value.Spell.Blueprint == teleport && value.Available) +
                    RawSlots(bookConjurer, 7).Count(value => value.Spell != null && value.Spell.Blueprint == greaterTeleport && value.Available);
                if (onlyFavorites != 2)
                    throw new InvalidOperationException("The world-map phase lacks exactly one ready favorite preparation per level.");
                if (GlobalMapRules.Instance != null) throw new InvalidOperationException("A global map is already loaded before the world-map phase.");
                game.LoadArea(game.BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None);
                for (int frame = 0; frame < 600; frame++)
                {
                    yield return 0;
                    if (!LoadingProcess.Instance.IsLoadingInProcess && !LoadingProcess.Instance.IsLoadingScreenActive &&
                        GlobalMapRules.Instance != null && game.CurrentMode == GameModeType.GlobalMap) break;
                }
                if (GlobalMapRules.Instance == null || game.CurrentMode != GameModeType.GlobalMap)
                    throw new InvalidOperationException("The world-map phase did not finish loading.");
                var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
                var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
                var payloadState = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
                var originalPayload = payloadState.GetValue(ledger);
                var originalPosition = map.PartyPosition; var originalLast = map.LastLocation;
                var originalTime = player.GameTime; float originalMiles = map.MilesTravelled;
                var originalHistory = map.HistoryTravels.ToArray(); var originalPerception = map.PerceptionRolledLocations.ToArray();
                var pointRecords = map.Locations.ToArray(); var edgeRecords = map.Edges.ToArray();
                var snapshots = pointRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value))
                    .Concat(edgeRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value)))
                    .Concat(new[] { new TeleportNativeFieldSnapshot(ledger) }).ToArray();
                try
                {
                    var chain = FindTeleportInteractionChain(rules);
                    var origin = chain[0]; var mapTarget = chain[2];
                    var setRevealed = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
                    foreach (var point in chain) { setRevealed.Invoke(point.Data, new object[] { true }); point.Data.EdgesOpened = true; }
                    var familiarity = new TeleportFamiliarityState(); familiarity.MigrateLegacy(chain.Select(value => value.Blueprint.AssetGuid));
                    payloadState.SetValue(ledger, familiarity.Serialize());
                    rules.StopWhenRevealingNewEdges = false;
                    rules.SetCurrentPosition(new Kingmaker.Globalmap.State.MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                    var rig = Resources.FindObjectsOfTypeAll<Kingmaker.View.CameraRig>().Single(value => value != null &&
                        value.gameObject.activeInHierarchy && value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded);
                    rig.ScrollTo(mapTarget.transform.position);
                    for (int frame = 0; frame < 60; frame++) yield return 0;
                    var panel = TeleportationFixturePanel();
                    TeleportDestinationRows worldRows = null;
                    for (int attempt = 0; attempt < 3 && (worldRows == null || worldRows.Actions.Count == 0); attempt++)
                    {
                        SelectTeleportationCastingPoint(panel, mapTarget);
                        foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                        for (int frame = 0; frame < 40; frame++)
                        {
                            worldRows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                            if (worldRows != null && worldRows.Actions.Count > 0) break;
                            yield return 0;
                        }
                        if (worldRows != null && worldRows.Actions.Count > 0) break;
                        SelectTeleportationCastingPoint(panel, mapTarget);
                        foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                    }
                    if (worldRows == null) throw new InvalidOperationException("The world-map phase composed no destination rows.");
                    // Level 7: direct Greater Teleport from the repaired favorite-only preparation.
                    var greaterAction = worldRows.Actions.SingleOrDefault(value => value.Source.Spell == TeleportSpellKind.GreaterTeleport &&
                        value.Source.BookId == bookConjurer.Blueprint.AssetGuid && value.Source.CasterId == conjurer.UniqueId);
                    SpecialistCacheAssert("world-map-favorite-greater-row",
                        "the repaired favorite-only Greater Teleport preparation composes as a real world-map source",
                        "uses=" + (greaterAction == null ? "absent" : greaterAction.Source.Uses.ToString()),
                        greaterAction != null && greaterAction.Source.Uses == 1);
                    if (greaterAction == null) throw new InvalidOperationException("No repaired favorite Greater Teleport world-map source.");
                    worldRows.QualificationRolls = new TeleportationFixtureRolls(new int[0]);
                    TeleportContextConfirmationPresenter.ResetDirectCastDiagnostics();
                    worldRows.Buttons[worldRows.Actions.ToList().FindIndex(value => value.Key == greaterAction.Key)].onClick.Invoke();
                    var greaterOutcome = TeleportContextConfirmationPresenter.LastDirectCast;
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    int remainingGreater = RawSlots(bookConjurer, 7).Count(value => value.Spell != null && value.Spell.Blueprint == greaterTeleport && value.Available);
                    bool greaterArrived = greaterOutcome != null && greaterOutcome.Transaction.State == TeleportTransactionState.Completed &&
                        greaterOutcome.Transaction.Result != null && greaterOutcome.Transaction.Result.Status == TeleportExecutionStatus.Arrived &&
                        greaterOutcome.Transaction.Result.DestinationId == mapTarget.Blueprint.AssetGuid;
                    CaptureTeleportationSpecialistCache("world-map-greater-spend", new {
                        transaction = greaterOutcome == null ? null : greaterOutcome.Transaction.State.ToString(),
                        evidence = greaterOutcome == null ? null : greaterOutcome.Execution.LastEvidence, remainingGreater });
                    SpecialistCacheAssert("world-map-favorite-greater-spend",
                        "the repaired favorite-only Greater Teleport preparation is spent directly: exactly one use, exact arrival, none remaining",
                        "arrived=" + greaterArrived + ";expenditure=" + (greaterOutcome != null && greaterOutcome.Execution.Resource != null ?
                            greaterOutcome.Execution.Resource.ObserveExpenditure().ToString() : "none") + ";remaining=" + remainingGreater,
                        greaterArrived && greaterOutcome.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne &&
                        remainingGreater == 0 && !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown);
                    // Level 5: ordinary Teleport through its confirmation from the repaired favorite-only preparation.
                    rules.SetCurrentPosition(new Kingmaker.Globalmap.State.MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                    player.GameTime = originalTime;
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        SelectTeleportationCastingPoint(panel, mapTarget);
                        foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                        worldRows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                        if (worldRows != null && worldRows.Actions.Any(value => value.Source.Spell == TeleportSpellKind.Teleport &&
                            value.Source.BookId == bookConjurer.Blueprint.AssetGuid)) break;
                        for (int frame = 0; frame < 30; frame++) yield return 0;
                    }
                    var teleportAction = worldRows.Actions.SingleOrDefault(value => value.Source.Spell == TeleportSpellKind.Teleport &&
                        value.Source.BookId == bookConjurer.Blueprint.AssetGuid && value.Source.CasterId == conjurer.UniqueId);
                    SpecialistCacheAssert("world-map-favorite-teleport-row",
                        "the repaired favorite-only Teleport preparation composes as a real world-map source",
                        "uses=" + (teleportAction == null ? "absent" : teleportAction.Source.Uses.ToString()),
                        teleportAction != null && teleportAction.Source.Uses == 1);
                    if (teleportAction == null) throw new InvalidOperationException("No repaired favorite Teleport world-map source.");
                    worldRows.QualificationRolls = new TeleportationFixtureRolls(new[] { 1 });
                    worldRows.Buttons[worldRows.Actions.ToList().FindIndex(value => value.Key == teleportAction.Key)].onClick.Invoke();
                    var teleportRequest = TeleportContextConfirmationPresenter.Current;
                    if (teleportRequest == null || !DialogMessageBox.Instance.IsShown)
                        throw new InvalidOperationException("The repaired favorite Teleport confirmation did not open.");
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    int remainingTeleport = RawSlots(bookConjurer, 5).Count(value => value.Spell != null && value.Spell.Blueprint == teleport && value.Available);
                    bool teleportArrived = teleportRequest.Transaction.State == TeleportTransactionState.Completed &&
                        teleportRequest.Transaction.Result != null && teleportRequest.Transaction.Result.Status == TeleportExecutionStatus.Arrived &&
                        teleportRequest.Transaction.Result.DestinationId == mapTarget.Blueprint.AssetGuid;
                    CaptureTeleportationSpecialistCache("world-map-teleport-spend", new {
                        transaction = teleportRequest.Transaction.State.ToString(), remainingTeleport });
                    SpecialistCacheAssert("world-map-favorite-teleport-spend",
                        "the repaired favorite-only Teleport preparation spends exactly one use through its confirmation and arrives exactly",
                        "arrived=" + teleportArrived + ";expenditure=" + teleportRequest.Execution.Resource.ObserveExpenditure() + ";remaining=" + remainingTeleport,
                        teleportArrived && teleportRequest.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne &&
                        remainingTeleport == 0 && !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown);
                }
                finally
                {
                    try { if (TeleportContextConfirmationPresenter.Pending && DialogMessageBox.Instance.IsShown) TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke(); }
                    catch { /* cleanup must continue */ }
                    map.TravelData = null;
                    rules.StopWhenRevealingNewEdges = true;
                    map.PartyPosition = originalPosition; map.LastLocation = originalLast;
                    map.HistoryTravels.Clear(); foreach (var entry in originalHistory) map.HistoryTravels.Add(entry);
                    map.PerceptionRolledLocations.Clear(); foreach (var entry in originalPerception) map.PerceptionRolledLocations.Add(entry);
                    map.MilesTravelled = originalMiles; player.GameTime = originalTime;
                    foreach (var snapshot in snapshots) snapshot.Restore();
                    payloadState.SetValue(ledger, originalPayload);
                }
            }
            finally
            {
                if (ui.ServiceWindow != null && ui.ServiceWindow.WindowTabs != null && ui.ServiceWindow.WindowTabs.IsShow) ui.ServiceWindow.HandleOpenSpellbook();
                for (int index = 0; index < conjurationLevels.Length; index++)
                {
                    conjurationLevels[index].Spells = originalSpells[index];
                    cache.SetValue(conjurationLevels[index], originalCaches[index]);
                }
                if (evokerFeatureAttached) evoker.Descriptor.RemoveFact(evocationFeature);
                if (blankFeatureAttached) blank.Descriptor.RemoveFact(conjurationFeature);
                if (conjurerFeatureAttached) conjurer.Descriptor.RemoveFact(conjurationFeature);
                fixtureBlank.Restore(); fixtureEvoker.Restore(); fixtureConjurer.Restore();
                var selectionManager = ui.SelectionManagerPC;
                if (selectionManager != null && originalSelection.Select(value => value.View).All(value => value != null))
                    selectionManager.MultiSelect(originalSelection.Select(value => value.View).ToArray(), false);
                foreach (var snapshot in uiSnapshots) snapshot.Restore();
                game.IsPaused = originalPaused;
                bool selectionRestored = selectionManager == null || selectionManager.SelectedUnits.SequenceEqual(originalSelection);
                bool restored = fixtureConjurer.IsRestored() && fixtureEvoker.IsRestored() && fixtureBlank.IsRestored() &&
                    uiSnapshots.All(value => value.IsRestored()) &&
                    !conjurer.Descriptor.HasFact(conjurationFeature) && !evoker.Descriptor.HasFact(evocationFeature) &&
                    !blank.Descriptor.HasFact(conjurationFeature) &&
                    selectionRestored && player.Party.SequenceEqual(originalParty) &&
                    conjurationLevels[0].Spells == originalSpells[0] && conjurationLevels[1].Spells == originalSpells[1] &&
                    !TeleportContextConfirmationPresenter.Pending && !_workingSaveSmoke.WriteObserved;
                CaptureTeleportationSpecialistCache("cleanup", new { restored });
                SpecialistCacheAssert("cleanup", "exact original books, features, lists, selection and party; zero writes",
                    "restored=" + restored, restored);
            }
        }

        private void SpecialistCacheAssert(string id, string expected, string actual, bool pass)
        { _teleportationSpecialistCacheAssertions.Add(Assertion("teleportation-specialist-cache-" + id, expected, actual, pass,
            Path.Combine(_request.EvidenceDirectory, "teleportation-specialist-cache.json"))); }
        private void CaptureTeleportationSpecialistCache(string step, object state)
        { _teleportationSpecialistCacheCaptures.Add(new { step, frame = Time.frameCount, state }); }

        private void PollTeleportationSpecialistCache()
        {
            if (!IsTeleportationSpecialistCacheFixture || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Specialist-cache qualification requires its guarded working save, automatic exit and intact write sentinels.");
            if (_teleportationSpecialistCacheSteps == null) _teleportationSpecialistCacheSteps = RunTeleportationSpecialistCache().GetEnumerator();
            Exception failure = null;
            try { if (_teleportationSpecialistCacheSteps.MoveNext()) return; }
            catch (Exception exception) { failure = exception; }
            try { _teleportationSpecialistCacheSteps.Dispose(); }
            catch (Exception exception) { failure = failure == null ? exception : new AggregateException(failure, exception); }
            _teleportationSpecialistCacheSteps = null;
            WriteTeleportationForensicJson(Path.Combine(_request.EvidenceDirectory, "teleportation-specialist-cache.json"), new {
                schemaVersion = 1, runId = _request.RunId,
                claims = "A real Conjuration book whose strategic-spell knowledge predates the Conjuration-list publication keeps a stale special-spell cache (native favorite slots refuse both spells); the production Spellbook.PostLoad load seam restores membership for Teleport@5 and GreaterTeleport@7 independently, native UI prepares/rests mixed favorite+ordinary uses, and negative controls hold. Request-local books/features; in-memory list rollback restored; no save writes.",
                captures = _teleportationSpecialistCacheCaptures, assertions = _teleportationSpecialistCacheAssertions,
                saveWriteObserved = _workingSaveSmoke.WriteObserved, error = failure == null ? null : failure.ToString() });
            Complete(CreateResult(failure != null ? RuntimeTestStatuses.Error : _teleportationSpecialistCacheAssertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _teleportationSpecialistCacheAssertions, failure == null ? null : failure.ToString()));
        }
    }
}
