using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.MagicCircle;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static void CircleAcquisition(UnitEntityData caster, UnitEntityData bearer,
            UnitEntityData recipient, Spellbook book, List<UnitEntityData> actors,
            List<BlueprintUnit> prototypes, List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var game = Game.Instance; var player = game.Player; var library = BlueprintBootstrap.Library;
            var circles = BlueprintBootstrap.MagicCircles;
            var inventory = player.Inventory;
            var itemsBefore = inventory.ToArray();
            var countsBefore = itemsBefore.Select(item => item.Count).ToArray();
            var chargesBefore = itemsBefore.Select(item => item.Charges).ToArray();
            var tablesBefore = player.SharedVendorTables;
            var tablesField = typeof(Player).GetField("SharedVendorTables", BindingFlags.Instance | BindingFlags.Public);
            long moneyBefore = player.Money;
            var moneySetter = typeof(Player).GetProperty("Money").GetSetMethod(true);
            var owner = player.MainCharacter.Value.Descriptor;
            var ledgerBefore = owner.Get<UnitPartMagicCircleScrollGrants>();
            var ledger = owner.Ensure<UnitPartMagicCircleScrollGrants>();
            var ledgerField = typeof(UnitPartMagicCircleScrollGrants).GetField("_tables", BindingFlags.Instance | BindingFlags.NonPublic);
            var ledgerRows = (List<string>)ledgerField.GetValue(ledger);
            var ledgerSnapshot = ledgerRows.ToArray();
            var teleportLedger = owner.Get<UnitPartTeleportFamiliarity>();
            var teleportSnapshot = teleportLedger == null ? null : new TeleportNativeFieldSnapshot(teleportLedger);
            var teleportGrantsField = typeof(UnitPartTeleportFamiliarity).GetField("_scrollVendorGrants", BindingFlags.Instance | BindingFlags.NonPublic);
            UnitPartVendor vendor = null;
            object levelController = null;
            var trade = new VendorLogic();
            try {
                if (!game.IsPaused || circles.Any(circle => inventory.Any(item => ReferenceEquals(item.Blueprint, circle.Scroll))) ||
                    bearer.Descriptor.Get<UnitPartVendor>() != null || moneySetter == null)
                    throw new InvalidOperationException("Acquisition requires paused request-owned vendor and no pre-existing circle scrolls.");
                // Synchronous request-local native stock. Preserve the complete
                // saved vendor object by reference; never mutate campaign stock.
                // The installed native field is readonly, so the fixture uses
                // reflection for this bounded swap and restores it in finally.
                tablesField.SetValue(player, new SharedVendorTables());
                ledgerRows.Clear();
                // The existing Teleportation hook observes the same native
                // merchant event. Isolate its mutable grant list too, then
                // restore its exact original payload and reference at cleanup.
                if (teleportLedger != null) {
                    var grants = (List<string>)teleportGrantsField.GetValue(teleportLedger);
                    teleportGrantsField.SetValue(teleportLedger, grants == null ? null : new List<string>(grants));
                }
                moneySetter.Invoke(player, new object[] { Math.Max(moneyBefore, 20000L) });
                CirclePublicationContracts(assertions);

                var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                    "ba34257984f4c41408ce1dc2004e342e", "native scribing Wizard");
                var scribe = CircleSpawn("Scribe", caster.Position, bearer, actors, prototypes);
                scribe.Stats.Intelligence.BaseValue = 30;
                AdvanceDisposableSpellcaster(scribe.Descriptor, wizard, 5, ref levelController);
                var prepared = scribe.Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, wizard.Spellbook));
                while (prepared.CasterLevel < 5) prepared.AddCasterLevel();
                prepared.UpdateAllSlotsSize(false);
                if (prepared.IsKnown(MagicCircleBlueprints.Family) || circles.Any(circle => prepared.IsKnown(circle.Spell)))
                    throw new InvalidOperationException("Fresh native scribing book already knows a circle.");
                var school = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, "30f20e6f850519b48aa59e8c0ff66ae9", "native Abjuration specialization");
                scribe.Descriptor.AddFact(school); prepared.UpdateAllSlotsSize(false);
                prepared.PostLoad(); prepared.PostLoad();
                assertions.Add(Assertion("circle-specialist-unknown-negative", "attached Abjuration list grants no unknown spell or favorite-slot entry",
                    "known=" + circles.Count(c => prepared.IsKnown(c.Spell)), circles.All(c => !prepared.IsKnown(c.Spell) &&
                        !prepared.GetSpecialSpells(3).Any(value => ReferenceEquals(value.Blueprint, c.Spell))),
                    "real native specialist with no circles learned; repeated production PostLoad must not teach spells"));
                var supplier = TeleportationScrollVendorPublication.DecideSupplier(library);
                vendor = bearer.Descriptor.Ensure<UnitPartVendor>();
                vendor.SetSharedInventory(supplier.Arcane);
                trade.BeginTrading(bearer);
                var stock = trade.StoreItems;
                assertions.Add(Assertion("circle-market-finite-stock", "five of each canonical scroll", string.Join(",", circles.Select(c => CircleItemCount(stock, c.Scroll))),
                    circles.All(c => CircleItemCount(stock, c.Scroll) == MagicCircleScrollVendors.Stock) && ledger.Has(supplier.Arcane.AssetGuid),
                    "native shared-table generation and actual BeginTrading migration callback"));
                foreach (var circle in circles) {
                    var forSale = stock.Single(item => ReferenceEquals(item.Blueprint, circle.Scroll));
                    long gold = player.Money, price = trade.GetItemBuyPrice(forSale);
                    trade.AddForBuy(forSale, 2); trade.Deal();
                    assertions.Add(Assertion("circle-market-purchase-" + circle.Alignment, "two scrolls at native price; remaining finite stock three",
                        "unitPrice=" + price + ";gold=" + gold + "->" + player.Money,
                        price > 0 && gold - player.Money == 2 * price && CircleItemCount(stock, circle.Scroll) == 3 && CircleItemCount(inventory, circle.Scroll) == 2,
                        "actual VendorLogic.AddForBuy/Deal; no direct grant substituted for purchase"));
                }
                trade.EndTraiding();
                trade.BeginTrading(bearer); trade.EndTraiding();
                assertions.Add(Assertion("circle-market-partial-no-refill", "repeat visits preserve three remaining", string.Join(",", circles.Select(c => CircleItemCount(stock, c.Scroll))),
                    circles.All(c => CircleItemCount(stock, c.Scroll) == 3), "actual migration hook on a recorded shared-table grant"));

                var doCopy = typeof(CopyScroll).GetMethod("DoCopy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var circle in circles) {
                    var purchased = inventory.Single(item => ReferenceEquals(item.Blueprint, circle.Scroll));
                    var copy = circle.Scroll.GetComponent<CopyScroll>();
                    int known = prepared.GetKnownSpells(3).Count();
                    bool eligible = copy.CanCopy(purchased, scribe), spontaneousRejected = !copy.CanCopy(purchased, caster);
                    if (!eligible || doCopy == null) throw new InvalidOperationException("Native purchased-scroll scribing is unavailable: " + circle.Alignment);
                    doCopy.Invoke(copy, new object[] { purchased, scribe });
                    copy.RemoveItem(purchased, scribe);
                    assertions.Add(Assertion("circle-scribe-" + circle.Alignment, "one level-three family learned; one scroll consumed; no spontaneous copying",
                        "known=" + known + "->" + prepared.GetKnownSpells(3).Count(),
                        eligible && spontaneousRejected && prepared.GetKnownSpells(3).Count(value => ReferenceEquals(value.Blueprint, MagicCircleBlueprints.Family)) == 1 &&
                        prepared.GetKnownSpells(3).Count() == known + 1 && circles.All(c => !prepared.IsKnown(c.Spell)) && CircleItemCount(inventory, circle.Scroll) == 1 &&
                        !copy.CanCopy(inventory.Single(item => ReferenceEquals(item.Blueprint, circle.Scroll)), scribe),
                        "native CanCopy/DoCopy/RemoveItem follow the child Parent to one family; exact scroll still casts its alignment"));

                    assertions.Add(Assertion("circle-scribe-family-duplicate-" + circle.Alignment, "all other alignment scrolls reject duplicate family learning", "family=" + MagicCircleBlueprints.Family.AssetGuid,
                        inventory.Where(item => circles.Any(c => ReferenceEquals(c.Scroll, item.Blueprint)))
                            .All(item => !item.Blueprint.GetComponent<CopyScroll>().CanCopy(item, scribe)),
                        "native copying sees one known family, not four separate spell identities"));
                    var scroll = inventory.Single(item => ReferenceEquals(item.Blueprint, circle.Scroll));
                    string resources = TeleportResourceFingerprint(book);
                    bool used = scroll.TryUseFromInventory(caster, new TargetWrapper(bearer));
                    game.EntityCreator.Tick();
                    var carrier = CircleBuffs(bearer, circle.Carrier).Single();
                    var area = CircleArea(carrier);
                    CircleRefresh(area, actors); CircleRefresh(area, actors);
                    assertions.Add(Assertion("circle-scroll-native-cast-" + circle.Alignment, "one scroll, no spellbook debit, CL5 and original 3000-second duration",
                        "used=" + used + ";cl=" + carrier.Context.Params.CasterLevel + ";seconds=" + carrier.TimeLeft.TotalSeconds,
                        used && CircleItemCount(inventory, circle.Scroll) == 0 && TeleportResourceFingerprint(book) == resources &&
                        carrier.Context.Params.CasterLevel == 5 && Math.Abs(carrier.TimeLeft.TotalSeconds - 3000) < 2 &&
                        ReferenceEquals(carrier.Context.MaybeCaster, caster) && ReferenceEquals(area.Context.MaybeOwner, bearer) &&
                        CircleBuffs(recipient, circle.Recipient).Length == 1,
                        "public ItemEntity.TryUseFromInventory uses native held-touch delivery, RuleCastSpell and source-item Spend exactly once"));
                    var deadline = carrier.EndTime;
                    recipient.Position = bearer.Position + new Vector3(8, 0, 0); CircleRefresh(area, actors);
                    recipient.Position = bearer.Position; CircleRefresh(area, actors);
                    assertions.Add(Assertion("circle-scroll-refresh-free-" + circle.Alignment, "entry/re-entry costs nothing and retains original deadline", "deadline=" + carrier.EndTime.Ticks,
                        carrier.EndTime == deadline && TeleportResourceFingerprint(book) == resources && CircleItemCount(inventory, circle.Scroll) == 0 &&
                        CircleBuffs(recipient, circle.Recipient).Length == 1,
                        "native area membership after the one-shot source item was consumed"));
                    carrier.Remove(); CircleRefresh(area, actors);

                    var slot = RawSlots(prepared, 3).First(value => value.Type == SpellSlotType.Common && value.Spell == null);
                    bool memorized = prepared.Memorize(new AbilityData(MagicCircleBlueprints.Family, prepared), slot);
                    prepared.Rest();
                    int ready = RawSlots(prepared, 3).Count(value => value.Available && value.Spell != null);
                    if (!memorized || !slot.Available) throw new InvalidOperationException("Copied circle did not become a prepared spell.");
                    CircleCast(scribe, bearer, CirclePreparedVariant(slot, circle.Spell), diagnostics);
                    carrier = CircleBuffs(bearer, circle.Carrier).Single(); area = CircleArea(carrier); CircleRefresh(area, actors);
                    assertions.Add(Assertion("circle-prepared-cast-" + circle.Alignment, "copied spell casts from one ordinary preparation", "ready=" + ready + "->" + RawSlots(prepared, 3).Count(value => value.Available && value.Spell != null),
                        !slot.Available && RawSlots(prepared, 3).Count(value => value.Available && value.Spell != null) == ready - 1 &&
                        ReferenceEquals(carrier.Context.MaybeCaster, scribe) && carrier.Context.Params.CasterLevel == prepared.CasterLevel,
                        "native Memorize/Rest/UnitUseAbility; purchased, copied, prepared and cast without granting the spell directly"));
                    carrier.Remove(); CircleRefresh(area, actors); prepared.ForgetMemorized(slot);
                    // Reset only this disposable book between independent scribe cases.
                    // The final case leaves one learned family for specialist checks.
                    if (!ReferenceEquals(circle, circles.Last())) prepared.RemoveSpell(MagicCircleBlueprints.Family);
                }

                var special = (List<AbilityData>[])typeof(Spellbook).GetField("m_SpecialSpells", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(prepared);
                var stale = special[3].Single(value => ReferenceEquals(value.Blueprint, MagicCircleBlueprints.Family));
                var knownBeforeRepair = prepared.GetAllKnownSpells().ToArray();
                if (!special[3].Remove(stale)) throw new InvalidOperationException("Could not stage exact owned stale specialist cache.");
                prepared.PostLoad(); prepared.PostLoad();
                assertions.Add(Assertion("circle-specialist-stale-cache", "known spell repaired once without changing any learned spell or owner",
                    "special=" + special[3].Count, prepared.GetSpecialSpells(3).Count(value => ReferenceEquals(value.Blueprint, MagicCircleBlueprints.Family)) == 1 &&
                        prepared.GetAllKnownSpells().SequenceEqual(knownBeforeRepair) && ReferenceEquals(prepared.Owner, scribe.Descriptor),
                    "removed only one exact cache entry on the request-owned native book; real PostLoad callback; fresh disk load remains separately tested"));
                var favorite = RawSlots(prepared, 3).Single(value => value.Type == SpellSlotType.Favorite);
                var fireball = wizard.Spellbook.SpellList.GetSpells(3).Single(value => value.name == "Fireball" &&
                    value.GetComponent<SpellComponent>()?.School == SpellSchool.Evocation);
                diagnostics.Add("native-school-negative:Fireball=" + fireball.AssetGuid);
                prepared.AddKnown(3, fireball, true);
                prepared.PostLoad(); prepared.PostLoad();
                assertions.Add(Assertion("circle-specialist-native-eligibility", "one known family in Abjuration favorite; Evocation refused", "special=" + prepared.IsSpellSpecial(new AbilityData(MagicCircleBlueprints.Family, prepared)),
                    prepared.GetSpecialSpells(3).Count(value => ReferenceEquals(value.Blueprint, MagicCircleBlueprints.Family)) == 1 &&
                        prepared.PosibleMemorize(new AbilityData(MagicCircleBlueprints.Family, prepared), favorite) &&
                    !prepared.PosibleMemorize(new AbilityData(fireball, prepared), favorite),
                    "actual native specialization feature, spellbook PostLoad callback and slot eligibility; fresh-load stale-cache case remains separate"));

                trade.BeginTrading(bearer);
                foreach (var circle in circles) trade.AddForBuy(trade.StoreItems.Single(item => ReferenceEquals(item.Blueprint, circle.Scroll)), 3);
                trade.Deal(); trade.EndTraiding();
                trade.BeginTrading(bearer); trade.EndTraiding();
                assertions.Add(Assertion("circle-market-bought-out-no-refill", "all four stocks remain empty after native buyout and revisit", string.Join(",", circles.Select(c => CircleItemCount(stock, c.Scroll))),
                    circles.All(c => CircleItemCount(stock, c.Scroll) == 0 && CircleItemCount(inventory, c.Scroll) == 3),
                    "finite stock and saved-marker production logic; disk save/load is a separate guarded persistence scenario"));
            }
            finally {
                if (trade.IsTrading) trade.EndTraiding();
                if (vendor != null) { vendor.Dispose(); bearer.Descriptor.Remove<UnitPartVendor>(); }
                if (levelController != null) levelController.GetType().GetMethod("Cancel").Invoke(levelController, null);
                foreach (var item in inventory.Except(itemsBefore).ToArray()) inventory.Remove(item);
                tablesField.SetValue(player, tablesBefore);
                if (moneySetter != null) moneySetter.Invoke(player, new object[] { moneyBefore });
                ledgerRows.Clear(); ledgerRows.AddRange(ledgerSnapshot);
                if (ledgerBefore == null) owner.Remove<UnitPartMagicCircleScrollGrants>();
                if (teleportSnapshot != null) teleportSnapshot.Restore();
                else owner.Remove<UnitPartTeleportFamiliarity>();
            }
            assertions.Add(Assertion("circle-acquisition-exact-cleanup", "original inventory, money, stock object and grant marker", "money=" + player.Money,
                inventory.SequenceEqual(itemsBefore) && itemsBefore.Select(item => item.Count).SequenceEqual(countsBefore) &&
                itemsBefore.Select(item => item.Charges).SequenceEqual(chargesBefore) && player.Money == moneyBefore &&
                ReferenceEquals(player.SharedVendorTables, tablesBefore) && ReferenceEquals(owner.Get<UnitPartMagicCircleScrollGrants>(), ledgerBefore) &&
                ledgerRows.SequenceEqual(ledgerSnapshot) && ReferenceEquals(owner.Get<UnitPartTeleportFamiliarity>(), teleportLedger) &&
                (teleportSnapshot == null || teleportSnapshot.Matches()), "synchronous request-only fixture; no save write or campaign merchant mutation"));
        }

        private static int CircleItemCount(ItemsCollection items, BlueprintItem blueprint)
        { return items.Where(item => ReferenceEquals(item.Blueprint, blueprint)).Sum(item => item.Count); }

        private static void CirclePublicationContracts(List<RuntimeTestAssertion> assertions)
        {
            var library = BlueprintBootstrap.Library; var circles = BlueprintBootstrap.MagicCircles;
            assertions.Add(Assertion("circle-family-exact-variants", "one full family and two restricted native variant parents", "parents=" + MagicCircleBlueprints.Families.Length,
                MagicCircleBlueprints.Family.Variants.Length == 4 && circles.All(circle => MagicCircleBlueprints.Family.Variants.Contains(circle.Spell) && circle.Spell.Parent == MagicCircleBlueprints.Family) &&
                MagicCircleBlueprints.PaladinFamily.Variants.Length == 2 && circles.All(circle => MagicCircleBlueprints.PaladinFamily.Variants.Contains(circle.Spell) == (circle.Alignment == "Evil" || circle.Alignment == "Chaos")) &&
                MagicCircleBlueprints.AntipaladinFamily.Variants.Length == 2 && circles.All(circle => MagicCircleBlueprints.AntipaladinFamily.Variants.Contains(circle.Spell) == (circle.Alignment == "Good" || circle.Alignment == "Law")),
                "independent alignment expectations on the final registered blueprints, including negative restricted children"));
            var repeat = MagicCircleSpellListPublication.Publish(library, circles);
            repeat.ReconcileNative(library, circles); repeat.Rollback();
            foreach (string id in new[] { "8443ce803d2d31347897a3d85cc32f53", "ba0401fdeb4062f40a7aa95b6f07fe89", "57c894665b7895c499b3dce058c284b3", "9f5be2f7ea64fe04eb40878347b147bc" }) {
                var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(library, id, "native published list");
                assertions.Add(Assertion("circle-publication-" + list.name, "exact legitimate level-three variants after repeated publication and rollback", list.name,
                    CirclePublishedFamily(list, id == "9f5be2f7ea64fe04eb40878347b147bc" ? MagicCircleBlueprints.PaladinFamily : MagicCircleBlueprints.Family),
                    "actual final native class list; duplicate-safe secondary publisher owns no existing entries"));
            }
            CircleOptionalPublicationContracts(assertions);
            var repeatedStock = MagicCircleScrollVendors.Publish(library, circles); repeatedStock.Rollback();
            var listFixture = ScriptableObject.CreateInstance<BlueprintSpellList>();
            var spellFixture = UnityEngine.Object.Instantiate(circles[0].Spell);
            var replacement = UnityEngine.Object.Instantiate(circles[0].Spell);
            var foreign = ScriptableObject.CreateInstance<SpellListComponent>();
            var transaction = new MagicCircleSpellListPublication();
            try {
                listFixture.SpellsByLevel = new[] { new SpellLevelList(3) };
                var level = listFixture.SpellsByLevel[0];
                var cacheBefore = level.SpellsFiltered;
                transaction.Add(listFixture, spellFixture); transaction.Add(listFixture, spellFixture);
                bool cacheRefreshed = !ReferenceEquals(cacheBefore, level.SpellsFiltered) && level.SpellsFiltered.Contains(spellFixture);
                bool once = level.Spells.Count(value => ReferenceEquals(value, spellFixture)) == 1;
                // A later publisher replaces its same-GUID entry and appends its
                // own component. Our rollback must preserve both exact objects.
                level.Spells = new List<BlueprintAbility> { replacement };
                spellFixture.ComponentsArray = spellFixture.ComponentsArray.Concat(new BlueprintComponent[] { foreign }).ToArray();
                transaction.Rollback();
                assertions.Add(Assertion("circle-publication-native-ownership", "cache invalidation/idempotence; foreign replacement and component survive rollback", "cache=" + cacheRefreshed + ";once=" + once,
                    cacheRefreshed && once && level.Spells.Count == 1 && ReferenceEquals(level.Spells[0], replacement) && spellFixture.ComponentsArray.Contains(foreign) &&
                    !spellFixture.ComponentsArray.OfType<SpellListComponent>().Any(value => ReferenceEquals(value.SpellList, listFixture)),
                    "production publication on unregistered native blueprint fixtures; no foreign library object is replaced"));
            }
            finally {
                transaction.Rollback();
                UnityEngine.Object.Destroy(listFixture); UnityEngine.Object.Destroy(spellFixture);
                UnityEngine.Object.Destroy(replacement); UnityEngine.Object.Destroy(foreign);
            }
        }
    }
}
