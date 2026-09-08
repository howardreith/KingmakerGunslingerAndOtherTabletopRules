using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.State;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void PollTeleportationContext()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationContext || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Context fixture requires its guarded named working save, automatic exit and intact write sentinels.");
            if (_teleportationMapLoad == null)
            {
                _teleportationMapLoad = Stopwatch.StartNew();
                Game.Instance.LoadArea(Game.Instance.BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None);
                return;
            }
            if (_teleportationMapLoad.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Context fixture world-map load timed out.");
            if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive ||
                GlobalMapRules.Instance == null || Game.Instance.CurrentMode != GameModeType.GlobalMap) return;
            Complete(RunTeleportationContext());
        }

        private RuntimeTestResult RunTeleportationContext()
        {
            Player player = Game.Instance.Player;
            GlobalMapRules rules = GlobalMapRules.Instance;
            GlobalMapState map = GlobalMapRules.State;
            UnitPartTeleportFamiliarity ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
            if (ledger == null || map.TravelData != null || map.CurrentEncounterData != null)
                throw new InvalidOperationException("Context fixture needs the enabled ledger and stationary encounter-free map.");
            var points = rules.AllLocations.Where(TeleportFamiliarityFixturePoint)
                .OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal).Take(2).ToArray();
            GlobalMapLocation oleg = rules.AllLocations.Single(value => value.Blueprint.AssetGuid == WordOfRecallDestinationPolicy.OlegId);
            GlobalMapLocation capital = rules.AllLocations.Single(value => value.Blueprint.AssetGuid == WordOfRecallDestinationPolicy.CapitalId);
            if (points.Length != 2) throw new InvalidOperationException("Two native crossroads anchors required.");
            var capitalBlueprint = KingdomRoot.Instance.BlueprintRegionCapital;
            KingdomState originalKingdom = player.Kingdom;
            // The named working save is pre-kingdom. Construct native save-owned
            // state in isolation through its deserialization constructor, whose
            // body initializes collections only. The normal public constructor
            // emits BP notifications and is deliberately not used here.
            KingdomState probeKingdom = originalKingdom;
            if (probeKingdom == null)
            {
                ConstructorInfo constructor = typeof(KingdomState).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single(value => value.GetParameters().Length == 1 && value.GetParameters()[0].ParameterType.Name == "JsonConstructorMark");
                probeKingdom = (KingdomState)constructor.Invoke(new[] { Activator.CreateInstance(constructor.GetParameters()[0].ParameterType) });
                typeof(KingdomState).GetField("Regions").SetValue(probeKingdom, new List<RegionState> { new RegionState(capitalBlueprint) });
            }
            RegionState capitalRegion = probeKingdom.Regions.Single(value => ReferenceEquals(value.Blueprint, capitalBlueprint));
            var settlement = capitalRegion.Settlement;
            if (!capitalBlueprint.SettlementIsPrebuilt || settlement == null || !ReferenceEquals(settlement.Region, capitalRegion) ||
                settlement.Location == null || settlement.Location.AssetGuid != WordOfRecallDestinationPolicy.CapitalId)
                throw new InvalidOperationException("Exact native prebuilt capital settlement contract differs.");
            MethodInfo setClaimed = typeof(RegionState).GetProperty("IsClaimed").GetSetMethod(true);
            MethodInfo setRevealed = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
            FieldInfo payload = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
            var originalPosition = map.PartyPosition;
            var originalLast = map.LastLocation;
            var originalTime = player.GameTime;
            var originalHistory = map.HistoryTravels.ToArray();
            string[] originalParty = player.Party.Select(value => value.UniqueId).ToArray();
            object originalLedger = payload.GetValue(ledger);
            bool originalClaimed = capitalRegion.IsClaimed;
            var pointRecords = map.Locations.ToArray();
            var edgeRecords = map.Edges.ToArray();
            var snapshots = pointRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value))
                .Concat(edgeRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value))).ToArray();
            var fixtures = new List<TeleportResourceFixtureOwner>();
            var assertions = new List<RuntimeTestAssertion>();
            var captures = new List<object>();
            string path = Path.Combine(_request.EvidenceDirectory, "teleportation-context.json");
            Exception failure = null;
            bool cleaned = false;
            try
            {
                assertions.Add(Assertion("teleportation-context-prekingdom-recall-state", "native absent-kingdom state resolves before establishment",
                    "kingdomAbsent=" + (originalKingdom == null), originalKingdom != null ||
                    TeleportationWorldMapAdapter.ReadRecall(player).DestinationId == WordOfRecallDestinationPolicy.OlegId, path));
                player.Kingdom = probeKingdom;
                BlueprintLocation[] visited = new[] { points[0].Blueprint, points[1].Blueprint, oleg.Blueprint, capital.Blueprint };
                foreach (BlueprintLocation point in visited)
                {
                    LocationData data;
                    if (!map.Locations.TryGetValue(point, out data)) throw new InvalidOperationException("Fixture point has no preexisting native record.");
                    setRevealed.Invoke(data, new object[] { true }); data.EdgesOpened = true; data.IsClosed = false;
                }
                var counts = new TeleportFamiliarityState(); counts.MigrateLegacy(visited.Select(value => value.AssetGuid));
                payload.SetValue(ledger, counts.Serialize());
                setClaimed.Invoke(capitalRegion, new object[] { false });
                rules.SetCurrentPosition(new MapPosition(points[0].Blueprint)); rules.UpdatePawnPosition();
                TeleportationWorldMapContext context = TeleportationWorldMapAdapter.Capture(false);
                captures.Add(new { step = "native-context", context.Diagnostic, capitalRegion = capitalBlueprint.AssetGuid,
                    prebuilt = capitalBlueprint.SettlementIsPrebuilt, capitalClaimed = capitalRegion.IsClaimed,
                    nativeCustomUIScene = Game.Instance.CurrentlyLoadedArea.CustomUIScene.SceneName,
                    nativeStaticScene = Game.Instance.CurrentlyLoadedArea.GetStaticScene().SceneName, rulesScene = rules.gameObject.scene.name });
                assertions.Add(Assertion("teleportation-context-native-state", "usable native stationary world-map party and campaign context",
                    context.Diagnostic, context.Usable, path));
                if (!context.Usable) throw new InvalidOperationException(context.Diagnostic);
                assertions.Add(Assertion("teleportation-context-no-spell-source", "no magical actions before real fixture spellbooks",
                    "actions=" + TeleportationWorldMapAdapter.Compose(context, points[1].Blueprint).Count,
                    TeleportationWorldMapAdapter.Compose(context, points[1].Blueprint).Count == 0, path));
                BlueprintCharacterClass wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "ba34257984f4c41408ce1dc2004e342e", "native Wizard context fixture");
                BlueprintCharacterClass sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "b3a505fb61437dc4097f43c3f8f9a4cf", "native Sorcerer context fixture");
                BlueprintCharacterClass cleric = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "67819271767a9dd4fbfd4ae700befea0", "native Cleric context fixture");
                var owners = player.Party.Where(TeleportationSpellbookAdapter.CasterAvailable).Where(value =>
                    new[] { wizard.Spellbook, sorcerer.Spellbook, cleric.Spellbook }.All(book => value.Descriptor.GetSpellbook(book) == null)).Take(2).ToArray();
                if (owners.Length != 2) throw new InvalidOperationException("Two owners without the temporary native books required.");
                foreach (var owner in owners) fixtures.Add(new TeleportResourceFixtureOwner(owner));
                Spellbook first = fixtures[0].AddBook(wizard.Spellbook);
                Spellbook spontaneous = fixtures[0].AddBook(sorcerer.Spellbook);
                Spellbook recall = fixtures[0].AddBook(cleric.Spellbook);
                Spellbook second = fixtures[1].AddBook(wizard.Spellbook);
                foreach (Spellbook book in new[] { first, spontaneous, second })
                {
                    book.AddKnown(5, BlueprintBootstrap.Teleportation.Teleport, true);
                    book.AddKnown(7, BlueprintBootstrap.Teleportation.GreaterTeleport, true);
                    if (!book.Blueprint.Spontaneous && (!book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.Teleport, book), null) ||
                        !book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.GreaterTeleport, book), null)))
                        throw new InvalidOperationException("Real contextual preparation failed.");
                    book.Rest();
                }
                if (!recall.IsKnown(BlueprintBootstrap.Teleportation.WordOfRecall)) recall.AddKnown(6, BlueprintBootstrap.Teleportation.WordOfRecall, true);
                if (!recall.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.WordOfRecall, recall), null))
                    throw new InvalidOperationException("Real contextual Recall preparation failed.");
                recall.Rest();
                var books = fixtures.SelectMany(value => value.Books).ToArray();
                string resources = string.Join("|", books.Select(TeleportResourceFingerprint));
                string familiar = ledger.Read().Serialize();
                var readSnapshots = map.Locations.Select(value => new TeleportNativeFieldSnapshot(value.Value))
                    .Concat(map.Edges.Select(value => new TeleportNativeFieldSnapshot(value.Value))).ToArray();
                Func<BlueprintLocation, WorldMapPointSpellAction[]> compose = point => TeleportationWorldMapAdapter.Compose(
                    TeleportationWorldMapAdapter.Capture(false), point).ToArray();
                var actions = compose(points[1].Blueprint);
                captures.Add(new { step = "real-contextual-sources", actions = actions.Select(value => new { key = value.Key,
                    spell = value.Source.Spell.ToString(), caster = value.Source.CasterName, casterId = value.Source.CasterId,
                    book = value.Source.BookName, bookId = value.Source.BookId, value.ShowBook,
                    uses = value.Source.Uses, kind = value.Source.Kind.ToString(), level = value.Source.SpellLevel }).ToArray() });
                assertions.Add(Assertion("teleportation-context-real-sources", "six actual Greater/Teleport rows; three distinct sources per spell",
                    "actions=" + actions.Length, actions.Length == 6 && actions.Select(value => value.Key).Distinct().Count() == 6 &&
                    actions.Take(3).All(value => value.Source.Spell == TeleportSpellKind.GreaterTeleport) &&
                    actions.Skip(3).All(value => value.Source.Spell == TeleportSpellKind.Teleport) &&
                    actions.Where(value => value.Source.CasterId == owners[0].UniqueId).All(value => value.ShowBook), path));
                assertions.Add(Assertion("teleportation-context-current-point", "no relocation spell at occupied point",
                    "actions=" + compose(points[0].Blueprint).Length, compose(points[0].Blueprint).Length == 0, path));
                assertions.Add(Assertion("teleportation-context-precapital-recall", "Recall only at Oleg before capital-region claim",
                    "recall=" + context.Recall.Diagnostic,
                    compose(oleg.Blueprint).Count(value => value.Source.Spell == TeleportSpellKind.WordOfRecall) == 1 &&
                    compose(capital.Blueprint).All(value => value.Source.Spell != TeleportSpellKind.WordOfRecall), path));
                setClaimed.Invoke(capitalRegion, new object[] { true });
                assertions.Add(Assertion("teleportation-context-capital-recall", "Recall only at exact capital after capital-region claim",
                    TeleportationWorldMapAdapter.ReadRecall(player).Diagnostic,
                    compose(capital.Blueprint).Count(value => value.Source.Spell == TeleportSpellKind.WordOfRecall) == 1 &&
                    compose(oleg.Blueprint).All(value => value.Source.Spell != TeleportSpellKind.WordOfRecall), path));
                capitalRegion.Settlement = null;
                assertions.Add(Assertion("teleportation-context-invalid-capital-no-fallback", "missing established capital suppresses Recall everywhere",
                    TeleportationWorldMapAdapter.ReadRecall(player).Diagnostic,
                    compose(capital.Blueprint).Concat(compose(oleg.Blueprint)).All(value => value.Source.Spell != TeleportSpellKind.WordOfRecall), path));
                capitalRegion.Settlement = settlement;
                var targetData = map.Locations[points[1].Blueprint];
                targetData.IsClosed = true;
                assertions.Add(Assertion("teleportation-context-closed-destination", "native campaign closure omits every magical action",
                    "actions=" + compose(points[1].Blueprint).Length, compose(points[1].Blueprint).Length == 0, path));
                targetData.IsClosed = false;
                setRevealed.Invoke(targetData, new object[] { false });
                assertions.Add(Assertion("teleportation-context-hidden-destination", "unrevealed destination omits every magical action",
                    "actions=" + compose(points[1].Blueprint).Length, compose(points[1].Blueprint).Length == 0, path));
                setRevealed.Invoke(targetData, new object[] { true });
                targetData.EdgesOpened = false; bool explored = targetData.IsExplored; targetData.IsExplored = false;
                var unvisited = new TeleportFamiliarityState(); unvisited.MigrateLegacy(visited.Where(value => value != points[1].Blueprint).Select(value => value.AssetGuid));
                payload.SetValue(ledger, unvisited.Serialize());
                assertions.Add(Assertion("teleportation-context-unvisited-destination", "unvisited destination omits every magical action",
                    "actions=" + compose(points[1].Blueprint).Length, compose(points[1].Blueprint).Length == 0, path));
                targetData.EdgesOpened = true; targetData.IsExplored = explored; payload.SetValue(ledger, familiar);
                var pending = TeleportationWorldMapAdapter.Capture(true);
                assertions.Add(Assertion("teleportation-context-pending-relocation", "another pending request omits actions",
                    pending.Diagnostic, !pending.Usable && TeleportationWorldMapAdapter.Compose(pending, points[1].Blueprint).Count == 0, path));
                // Read every existing native point without creating a record, traversing a route or changing a count.
                var inventory = rules.AllLocations.Select(point => {
                    var snapshot = TeleportationWorldMapAdapter.ReadDestination(context, point.Blueprint);
                    var decision = TeleportDestinationPolicy.Evaluate(snapshot, context.OriginId, TeleportationWorldMapAdapter.Forbidden);
                    return new { id = snapshot.Id, kind = snapshot.Kind.ToString(), facts = snapshot.Facts.ToString(),
                        snapshot.NativeVisited, snapshot.OrdinaryArrivals, reason = decision.Reason.ToString() };
                }).ToArray();
                captures.Add(new { step = "current-destination-inventory", points = inventory });
                assertions.Add(Assertion("teleportation-context-reads-preserve-state", "all point reads and repeated source composition preserve native state/resources",
                    "points=" + inventory.Length, readSnapshots.All(value => value.Matches()) && map.Locations.Count == pointRecords.Length &&
                    map.Edges.Count == edgeRecords.Length && resources == string.Join("|", books.Select(TeleportResourceFingerprint)) &&
                    familiar == ledger.Read().Serialize() && map.TravelData == null && originalTime == player.GameTime &&
                    actions.Select(value => value.Key).SequenceEqual(compose(points[1].Blueprint).Select(value => value.Key)), path));
            }
            catch (Exception exception) { failure = exception; }
            finally
            {
                capitalRegion.Settlement = settlement; setClaimed.Invoke(capitalRegion, new object[] { originalClaimed });
                player.Kingdom = originalKingdom;
                if (!ReferenceEquals(probeKingdom, originalKingdom)) probeKingdom.Dispose();
                foreach (var fixture in fixtures.AsEnumerable().Reverse()) fixture.Restore();
                payload.SetValue(ledger, originalLedger);
                foreach (var snapshot in snapshots) snapshot.Restore();
                rules.SetCurrentPosition(originalPosition); rules.UpdatePawnPosition(); map.LastLocation = originalLast;
                cleaned = fixtures.All(value => value.IsRestored()) && snapshots.All(value => value.Matches()) &&
                    originalParty.SequenceEqual(player.Party.Select(value => value.UniqueId)) && originalTime == player.GameTime &&
                    map.TravelData == null && map.HistoryTravels.SequenceEqual(originalHistory) && map.CurrentEncounterData == null &&
                    map.Locations.Count == pointRecords.Length && map.Edges.Count == edgeRecords.Length &&
                    Equals(payload.GetValue(ledger), originalLedger) && capitalRegion.IsClaimed == originalClaimed &&
                    ReferenceEquals(capitalRegion.Settlement, settlement) && ReferenceEquals(player.Kingdom, originalKingdom) && !_workingSaveSmoke.WriteObserved;
            }
            assertions.Add(Assertion("teleportation-context-fixture-cleanup", "exact map/ledger/capital/resources restored; no save writes",
                "cleaned=" + cleaned, cleaned, path));
            WriteTeleportationForensicJson(path, new { schemaVersion = 1, runId = _request.RunId,
                claims = "Production current-map/destination/Recall/source composition only. Native UI rows, confirmations and completed magical casts are not yet qualified by this probe.",
                captures, assertions, cleaned, saveWriteObserved = _workingSaveSmoke.WriteObserved, error = failure == null ? null : failure.ToString() });
            return CreateResult(failure != null ? RuntimeTestStatuses.Error : assertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, failure == null ? null : failure.ToString());
        }
    }
}
