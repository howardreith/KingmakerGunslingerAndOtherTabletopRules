using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Globalmap;
using Kingmaker.Items;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private JObject _teleportPersistenceAcquisition;

        // R4: the real acquisition lifecycle inside the guarded fresh-process
        // persistence phases. Phase A performs an actual native gold purchase
        // from the selected supplier's shared stock, a native copy-from-scroll
        // into a fresh book, a Conjuration favorite preparation with native
        // rest, and one native scroll activation whose strategic relocation is
        // followed by a real first-arrow movement action. The exact resulting
        // state is captured into the phase plan; every later fresh process
        // verifies it byte-for-byte before any fixture can reconstruct it.
        private IEnumerable<int> EstablishTeleportPersistenceAcquisition(GlobalMapLocation[] chain)
        {
            var game = Game.Instance; var player = game.Player; var map = GlobalMapRules.State;
            var rules = GlobalMapRules.Instance;
            var scrolls = BlueprintBootstrap.TeleportationScrolls;
            if (scrolls == null || BlueprintBootstrap.TeleportationScrollVendors == null)
                throw new InvalidOperationException("The persistence acquisition chain requires the scroll machinery.");
            var supplier = TeleportationScrollVendorPublication.DecideSupplier(BlueprintBootstrap.Library);
            if (supplier.Arcane == null || supplier.Priest == null)
                throw new InvalidOperationException("Both mission suppliers must resolve for the acquisition chain.");
            var reader = player.Party.FirstOrDefault(TeleportationSpellbookAdapter.CasterAvailable);
            if (reader == null) throw new InvalidOperationException("No available acquisition reader.");
            // Real gold purchase: the exact native vendor boundary the shop UI
            // drives, against the supplier the shared decision selected.
            var vendorPart = reader.Descriptor.Ensure<UnitPartVendor>();
            vendorPart.SetSharedInventory(supplier.Arcane);
            var stock = player.SharedVendorTables.GetTable(supplier.Arcane);
            long goldBefore = player.Money;
            if (goldBefore < 5000) player.GainMoney(5000 - goldBefore);
            long goldBase = player.Money;
            var trade = new VendorLogic();
            trade.BeginTrading(reader);
            ItemEntity forSale = null;
            foreach (var entity in trade.StoreItems)
                if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, scrolls.Teleport)) { forSale = entity; break; }
            if (forSale == null) throw new InvalidOperationException("The selected supplier offers no Teleport scroll to buy.");
            long price = trade.GetItemBuyPrice(forSale);
            trade.AddForBuy(forSale, 2);
            trade.Deal();
            trade.EndTraiding();
            int carried = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport);
            int remainingStock = CountPersistenceItems(stock, scrolls.Teleport);
            PersistenceAssert("acquisition-purchase", "a real native gold purchase buys two scrolls at the exact native price from the selected supplier",
                goldBase - player.Money == price * 2 && carried == 2 && remainingStock >= 0,
                new { supplier = supplier.Arcane.name, fallback = supplier.ArcaneFallback, price, goldDelta = goldBase - player.Money, carried, remainingStock });
            // Native copy-from-scroll into a fresh book on the reader.
            var fixture = new TeleportResourceFixtureOwner(reader);
            var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "ba34257984f4c41408ce1dc2004e342e", "native persistence acquisition Wizard class");
            var book = fixture.AddBook(wizard.Spellbook);
            book.UpdateAllSlotsSize(false);
            var purchased = default(ItemEntity);
            foreach (var unit in player.Party)
            {
                if (unit == null || unit.Inventory == null) continue;
                foreach (var entity in unit.Inventory)
                    if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, scrolls.Teleport)) { purchased = entity; break; }
                if (purchased != null) break;
            }
            if (purchased == null) throw new InvalidOperationException("The purchased scrolls did not reach the party inventory.");
            var copy = scrolls.Teleport.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>().Single();
            if (!copy.CanCopy(purchased, reader)) throw new InvalidOperationException("The purchased scroll is not natively copyable here.");
            var doCopy = typeof(Kingmaker.Blueprints.Items.Components.CopyScroll).GetMethod("DoCopy",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            doCopy.Invoke(copy, new object[] { purchased, reader });
            copy.RemoveItem(purchased, reader);
            bool learned = book.GetKnownSpells(5).Any(value => value.Blueprint == scrolls.Teleport.Ability);
            PersistenceAssert("acquisition-copy", "the native copy action learns the canonical spell and consumes one purchased scroll",
                learned && TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) == 1,
                new { learned, carriedAfterCopy = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) });
            // Conjuration favorite preparation on the copied spell + native rest.
            var conjurationList = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList>(BlueprintBootstrap.Library,
                "69a6eba12bc77ea4191f573d63c9df12", "Conjuration special list");
            book.AddSpecialList(conjurationList);
            book.UpdateAllSlotsSize(false);
            var favorite = RawSlots(book, 5).SingleOrDefault(value => value.Type == SpellSlotType.Favorite);
            if (favorite == null || !book.Memorize(new AbilityData(scrolls.Teleport.Ability, book), favorite))
                throw new InvalidOperationException("Native favorite preparation failed in the acquisition chain.");
            book.Rest();
            // One native scroll activation through the contextual request, then a
            // real first-arrow movement action at the arrival.
            var origin = chain[0]; var target = chain[2];
            rules.SetCurrentPosition(new Kingmaker.Globalmap.State.MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
            var panel = TeleportationFixturePanel();
            TeleportDestinationRows rows = null;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                SelectTeleportationCastingPoint(panel, target);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                for (int frame = 0; frame < 40; frame++)
                {
                    rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                    if (rows != null && rows.Actions.Count > 0) break;
                    yield return 0;
                }
                if (rows != null && rows.Actions.Any(value => value.Source.Kind == TeleportCastSourceKind.Scroll &&
                    value.Source.CasterId == reader.UniqueId)) break;
                for (int frame = 0; frame < 30; frame++) yield return 0;
            }
            var scrollRow = rows == null ? null : rows.Actions.SingleOrDefault(value => value.Source.Kind == TeleportCastSourceKind.Scroll &&
                value.Source.CasterId == reader.UniqueId && value.Source.Spell == TeleportSpellKind.Teleport);
            if (scrollRow == null) throw new InvalidOperationException("The acquisition scroll row was not composed.");
            rows.QualificationRolls = new TeleportationFixtureRolls(new[] { 1 });
            rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == scrollRow.Key)].onClick.Invoke();
            var request = TeleportContextConfirmationPresenter.Current;
            if (request == null || !DialogMessageBox.Instance.IsShown)
                throw new InvalidOperationException("The acquisition confirmation did not open.");
            for (int frame = 0; frame < 8; frame++) yield return 0;
            TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
            for (int frame = 0; frame < 12; frame++) yield return 0;
            bool arrived = request.Transaction.State == TeleportTransactionState.Completed &&
                request.Transaction.Result != null && request.Transaction.Result.Status == TeleportExecutionStatus.Arrived &&
                request.Transaction.Result.DestinationId == target.Blueprint.AssetGuid;
            var arrivalPoint = rules.GetLocationObject(map.PartyLocation);
            var labels = ArrowCompassLabels();
            var firstArrow = labels.FirstOrDefault();
            bool arrowMoved = false;
            if (firstArrow != null)
            {
                firstArrow.OnClick();
                for (int frame = 0; frame < 10; frame++) yield return 0;
                arrowMoved = map.TravelData != null && map.TravelData.Walking;
                if (map.TravelData != null)
                {
                    if (map.TravelData.Walking) rules.OnBreak();
                    map.TravelData = null;
                }
            }
            rules.SetCurrentPosition(new Kingmaker.Globalmap.State.MapPosition(target.Blueprint)); rules.UpdatePawnPosition();
            PersistenceAssert("acquisition-activation",
                "one native scroll activation relocates, consumes exactly one scroll, spends no slot, and the first legal arrow really moves the party",
                arrived && arrowMoved && TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) == 0,
                new { arrived, arrowMoved, carriedAfterCast = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport),
                    activation = request.Execution.Resource == null ? null : TeleportationDiagnosticJson.Serialize(request.Execution.Resource.Evidence()) });
            if (goldBefore < 5000) player.SpendMoney(player.Money - goldBefore);
            vendorPart.Dispose();
            // The deterministic acquisition snapshot later fresh processes verify
            // BEFORE any fixture can reconstruct it.
            var grantsField = typeof(UnitPartTeleportFamiliarity).GetField("_scrollVendorGrants", BindingFlags.Instance | BindingFlags.NonPublic);
            var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
            var grants = (List<string>)grantsField.GetValue(ledger);
            _teleportPersistenceAcquisition = new JObject {
                ["supplierTable"] = supplier.Arcane.AssetGuid,
                ["supplierFallback"] = supplier.ArcaneFallback,
                ["priestTable"] = supplier.Priest == null ? null : supplier.Priest.AssetGuid,
                ["grantMarkers"] = new JArray(grants ?? new List<string>()),
                ["remainingTeleportStock"] = CountPersistenceItems(stock, scrolls.Teleport),
                ["remainingGreaterStock"] = CountPersistenceItems(stock, scrolls.GreaterTeleport),
                ["gold"] = player.Money.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["carriedTeleportScrolls"] = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport),
                ["learnedTeleport"] = book.GetKnownSpells(5).Any(value => value.Blueprint == scrolls.Teleport.Ability),
                ["acquisitionBookId"] = book.Blueprint.AssetGuid,
                ["readyFavoriteUses"] = RawSlots(book, 5).Count(value => value.Spell != null &&
                    value.Spell.Blueprint == scrolls.Teleport.Ability && value.Available),
                ["spentFavoriteUses"] = RawSlots(book, 5).Count(value => value.Spell != null &&
                    value.Spell.Blueprint == scrolls.Teleport.Ability && !value.Available),
                ["arrivalPoint"] = map.PartyLocation == null ? null : map.PartyLocation.AssetGuid,
                ["miles"] = map.MilesTravelled.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
            };
        }

        private static int CountPersistenceItems(ItemsCollection collection, BlueprintItem item)
        {
            if (collection == null) return 0;
            int total = 0;
            foreach (var entity in collection)
                if (entity != null && ReferenceEquals(entity.Blueprint, item)) total += entity.Count;
            return total;
        }
    }
}
