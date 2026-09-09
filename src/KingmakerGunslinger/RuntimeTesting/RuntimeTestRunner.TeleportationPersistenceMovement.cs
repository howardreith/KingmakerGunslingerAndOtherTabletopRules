using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> EstablishTeleportPersistence(UnitPartTeleportFamiliarity ledger, GlobalMapLocation[] chain, bool first)
        {
            var game = Game.Instance; var map = GlobalMapRules.State; var rules = GlobalMapRules.Instance;
            var panel = TeleportationFixturePanel();
            var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "ba34257984f4c41408ce1dc2004e342e", "native persistence caster");
            var caster = game.Player.Party.FirstOrDefault(value => TeleportationSpellbookAdapter.CasterAvailable(value) &&
                value.Descriptor.GetSpellbook(wizard.Spellbook) == null);
            if (caster == null) throw new InvalidOperationException("No existing active party owner for a temporary native persistence book.");
            var fixture = new TeleportResourceFixtureOwner(caster);
            bool stop = rules.StopWhenRevealingNewEdges;
            try
            {
                var book = fixture.AddBook(wizard.Spellbook);
                foreach (var spell in new[] { BlueprintBootstrap.Teleportation.Teleport, BlueprintBootstrap.Teleportation.GreaterTeleport })
                {
                    book.AddKnown(spell == BlueprintBootstrap.Teleportation.Teleport ? 5 : 7, spell, true);
                    if (!book.Memorize(new AbilityData(spell, book), null)) throw new InvalidOperationException("Native persistence preparation failed.");
                }
                book.Rest();
                string before = ledger.Read().Serialize();
                if (first)
                {
                    var reveal = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
                    foreach (var point in chain) { reveal.Invoke(point.Data, new object[] { true }); point.Data.EdgesOpened = true; }
                    foreach (var edge in chain[1].Edges.Where(value => value.GetOppositeLocation(chain[1]) == chain[0] || value.GetOppositeLocation(chain[1]) == chain[2]))
                        edge.Data.UpdateExplored(1, 1);
                    rules.SetCurrentPosition(new MapPosition(chain[0].Blueprint)); rules.UpdatePawnPosition();
                    var zero = chain.Where(point => ledger.Read().Count(point.Blueprint.AssetGuid) == 0 && point.Blueprint != map.PartyLocation).ToArray();
                    PersistenceAssert("late-native-unlock", "Post-migration native reveal/open alone provides neither Teleport family",
                        zero.Length > 0 && zero.All(point => !TeleportationWorldMapAdapter.Compose(TeleportationWorldMapAdapter.Capture(false), point.Blueprint)
                            .Any(row => row.Source.Spell != TeleportSpellKind.WordOfRecall)) && before == ledger.Read().Serialize(),
                        new { zero = zero.Select(point => point.Blueprint.AssetGuid).ToArray(), before, after = ledger.Read().Serialize() });
                }
                var targetForPanel = first ? chain[2] : chain[0];
                SelectTeleportationCastingPoint(panel, targetForPanel);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                panel.Hide();
                PersistenceAssert("selection-cancel", "Selection, native panel, reveal and cancellation add no arrival", before == ledger.Read().Serialize(),
                    new { before, after = ledger.Read().Serialize() });
                rules.StopWhenRevealingNewEdges = false;
                if (first) TravelTeleportPersistence(chain[2], chain[1], ledger);
                TravelTeleportPersistence(chain[0], chain[1], ledger);
                var context = TeleportationWorldMapAdapter.Capture(false);
                var actions = TeleportationWorldMapAdapter.Compose(context, chain[2].Blueprint);
                PersistenceAssert("persisted-composition", "Observed or restored positive ledger enables both spell families from real prepared sources",
                    actions.Any(row => row.Source.Spell == TeleportSpellKind.Teleport) && actions.Any(row => row.Source.Spell == TeleportSpellKind.GreaterTeleport),
                    new { target = chain[2].Blueprint.AssetGuid, arrivals = ledger.Read().Count(chain[2].Blueprint.AssetGuid), rows = actions.Select(row => row.Key).ToArray() });
                var recall = TeleportationWorldMapAdapter.ReadRecall(game.Player);
                PersistenceAssert("recall-sanctuary", "Recall retains its exact native kingdom sanctuary and cannot resolve an arbitrary point",
                    recall.Known && (recall.Established ? recall.DestinationId != WordOfRecallDestinationPolicy.OlegId : recall.DestinationId == WordOfRecallDestinationPolicy.OlegId),
                    new { recall.Known, recall.Established, recall.DestinationId, recall.Diagnostic });
                string preMagic = ledger.Read().Serialize();
                var cancel = OpenTeleportationFixtureConfirmation(panel, chain[2], TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, new TeleportationFixtureRolls(new[] { 1 }));
                TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke();
                PersistenceAssert("confirmation-cancel", "Native cancellation spends nothing and changes no familiarity",
                    cancel.Transaction.State == TeleportTransactionState.Cancelled && preMagic == ledger.Read().Serialize(), new { state = cancel.Transaction.State.ToString() });
                var cast = OpenTeleportationFixtureConfirmation(panel, chain[2], TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, new TeleportationFixtureRolls(new[] { 1 }));
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                PersistenceAssert("native-magical-cast", "Actual contextual confirmation spends exactly one native use, relocates, changes no counts and records exploration boundary",
                    cast.Transaction.State == TeleportTransactionState.Completed && cast.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne &&
                    map.PartyLocation == chain[2].Blueprint && ledger.Read().Serialize() == preMagic && ledger.ReadExplorationBoundary() != null,
                    new { state = cast.Transaction.State.ToString(), resource = cast.Execution.Resource.Evidence(), outcome = cast.Execution.LastEvidence,
                        before = preMagic, after = ledger.Read().Serialize(), boundary = ledger.ReadExplorationBoundary()?.Serialize() });
                for (int frame = 0; frame < 8; frame++) yield return 0;
                PersistenceAssert("deferred-magical-arrival", "Subsequent native frames preserve familiarity and the real exploration boundary",
                    ledger.Read().Serialize() == preMagic && ledger.ReadExplorationBoundary() != null, new { ledger = preMagic, boundary = ledger.ReadExplorationBoundary()?.Serialize() });
            }
            finally
            {
                CloseTeleportationFixturePanels(); rules.StopWhenRevealingNewEdges = stop; fixture.Restore();
                if (!fixture.IsRestored()) throw new InvalidOperationException("Temporary real spellbook fixture was not fully removed before native saving.");
            }
        }
        private void TravelTeleportPersistence(GlobalMapLocation target, GlobalMapLocation middle, UnitPartTeleportFamiliarity ledger)
        {
            var game = Game.Instance; var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
            var before = ledger.Read(); string origin = map.PartyLocation.AssetGuid;
            var panel = TeleportationFixturePanel(); panel.OnLocationSelect(target.Blueprint, true); panel.Accept();
            var travel = map.TravelData;
            if (travel == null || !travel.Walking || travel.Path.Count != 2 || travel.Path.Any(edge => !rules.GetEdgeObject(edge.Blueprint).Data.Revealed))
                throw new InvalidOperationException("Native persistence travel requires the exact revealed two-edge path.");
            var crossed = travel.Path.Select(edge => edge.Direction > 0 ? rules.GetEdgeObject(edge.Blueprint).Location2 : rules.GetEdgeObject(edge.Blueprint).Location1).ToArray();
            if (crossed[0] != middle || crossed[1] != target) throw new InvalidOperationException("Unexpected native persistence arrival boundaries.");
            float speed = game.BlueprintRoot.GlobalMap.VisualSpeedBase * (float)typeof(MapMovementController)
                .GetMethod("CalcSpeedModifiers", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            if (speed <= 0 || float.IsNaN(speed) || float.IsInfinity(speed)) throw new InvalidOperationException("Unusable native movement speed.");
            float delta = game.TimeController.DeltaTime;
            try
            {
                game.TimeController.SetDeltaTime((travel.Path.Sum(edge => rules.GetEdgeObject(edge.Blueprint).Spline.WorldLength) + 0.01f) / speed);
                new MapMovementController().Tick();
            }
            finally { game.TimeController.SetDeltaTime(delta); }
            var after = ledger.Read();
            PersistenceAssert("ordinary-boundaries-" + _teleportPersistenceEvents.Count, "One actual native step crosses intermediate and final boundaries, each exactly once",
                crossed.All(point => after.Count(point.Blueprint.AssetGuid) == before.Count(point.Blueprint.AssetGuid) + 1) &&
                before.Count(origin) == after.Count(origin) && map.PartyLocation == target.Blueprint && map.TravelData == null,
                new { origin, target = target.Blueprint.AssetGuid, crossed = crossed.Select(point => point.Blueprint.AssetGuid).ToArray(),
                    before = before.Serialize(), after = after.Serialize(), route = DescribeFamiliarityRoute(travel) });
        }
    }
}
