using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewSprint29Items(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            ItemEntity[] partyBefore = Game.Instance.Player.Inventory.Items.ToArray();
            int[] partyCounts = partyBefore.Select(value => value.Count).ToArray();
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (string key in new[] { "ettin", "cloud-giant",
                    "storm-giant" })
                {
                    ResetExpandedSummoningHostile(fixture);
                    SummonMultiplicity quantity = key == "ettin" ?
                        SummonMultiplicity.OneD4PlusOne :
                        key == "cloud-giant" ?
                        SummonMultiplicity.OneD3 :
                        SummonMultiplicity.One;
                    var route = ExpandedSummoningOwnTierVariant(key, quantity);
                    UnitEntityData[] group = CastExpandedSummoningVariant(
                        fixture.Blueprints, fixture.Caster, route, null,
                        fixture.Evidence);
                    fixture.Created.AddRange(group);
                    var owned = new List<ItemEntity>();
                    try
                    {
                        foreach (UnitEntityData unit in group)
                            SetExpandedSummoningBrainActive(unit, false);
                        for (int frame = 0; frame < 12; frame++) yield return 0;
                        var rows = new JArray();
                        bool exact = group.Length >= (key == "ettin" ? 2 : 1) &&
                            group.Length <= (key == "ettin" ? 5 :
                                key == "cloud-giant" ? 3 : 1);
                        foreach (UnitEntityData unit in group)
                        {
                            // Kingmaker creates alternate hands lazily. Use
                            // real switch commands before taking ownership
                            // or transfer evidence, especially for Ettin's
                            // second javelin and Storm's bow.
                            var switchRows = new JArray(new JObject {
                                ["set"] = unit.Body.CurrentHandEquipmentSetIndex,
                                ["items"] = Sprint29ItemSlots(unit) });
                            foreach (int index in new[] { 1, 2, 0 })
                            {
                                if (index == 2 && key == "ettin") continue;
                                var switchCommand =
                                    new UnitSwitchHandEquipmentSet(index);
                                unit.Commands.Run(switchCommand);
                                DateTime deadline = DateTime.UtcNow
                                    .AddSeconds(20);
                                for (int frame = 0; frame < 1000 &&
                                    !switchCommand.IsFinished &&
                                    DateTime.UtcNow < deadline; frame++)
                                    yield return 0;
                                exact &= switchCommand.IsFinished &&
                                    switchCommand.Result ==
                                        Kingmaker.UnitLogic.Commands.Base
                                            .UnitCommand.ResultType.Success &&
                                    unit.Body.CurrentHandEquipmentSetIndex ==
                                        index;
                                switchRows.Add(new JObject {
                                    ["set"] = index,
                                    ["items"] = Sprint29ItemSlots(unit) });
                            }
                            var body = unit.Blueprint.Body;
                            string[] expected = new[] {
                                body.PrimaryHand?.AssetGuid,
                                body.SecondaryHand?.AssetGuid,
                                body.PrimaryHandAlternative1?.AssetGuid,
                                body.SecondaryHandAlternative1?.AssetGuid,
                                body.PrimaryHandAlternative2?.AssetGuid,
                                body.SecondaryHandAlternative2?.AssetGuid,
                                body.Armor?.AssetGuid }
                                .Where(value => value != null).ToArray();
                            ItemEntity[] items = unit.Descriptor.Inventory.Items
                                .ToArray();
                            bool unitExact = expected.Distinct(
                                    StringComparer.Ordinal).Count() ==
                                    expected.Length &&
                                items.Length == expected.Length &&
                                items.Select(value => value.Blueprint.AssetGuid)
                                    .OrderBy(value => value,
                                        StringComparer.Ordinal).SequenceEqual(
                                    expected.OrderBy(value => value,
                                        StringComparer.Ordinal)) &&
                                items.All(value => value.Count == 1 &&
                                    ReferenceEquals(value.Owner,
                                        unit.Descriptor) &&
                                    value.IsNonRemovable && !value.IsLootable);
                            exact &= unitExact;
                            owned.AddRange(items);
                            rows.Add(new JObject {
                                ["unit"] = unit.UniqueId,
                                ["expected"] = new JArray(expected),
                                ["items"] = new JArray(items.Select(value =>
                                    new JObject {
                                        ["blueprint"] = value.Blueprint.AssetGuid,
                                        ["instance"] =
                                            RuntimeHelpers.GetHashCode(value),
                                        ["owner"] = value.Owner?.Unit?.UniqueId,
                                        ["count"] = value.Count,
                                        ["nonRemovable"] = value.IsNonRemovable,
                                        ["lootable"] = value.IsLootable })),
                                ["switches"] = switchRows,
                                ["exact"] = unitExact });
                        }
                        exact &= owned.Count == owned.Distinct().Count() &&
                            Sprint28PartyInventoryExact(partyBefore,
                                partyCounts);
                        Sprint25Check("s29-" + key + "-quantity-owned-items",
                            exact, new JObject {
                                ["quantity"] = group.Length,
                                ["route"] = quantity.ToString(),
                                ["actors"] = rows,
                                ["uniqueInstances"] = owned.Distinct().Count(),
                                ["partyExact"] = Sprint28PartyInventoryExact(
                                    partyBefore, partyCounts) },
                            "The actual hidden quantity route owns distinct nonlootable weapon, armor and projectile instances across resolved hand sets.");
                        if (!exact) throw new InvalidOperationException(
                            "Sprint 29 quantity item precondition failed.");

                        var destination = fixture.Hostile.Descriptor.Inventory;
                        ItemEntity[] destinationBefore = destination.Items
                            .ToArray();
                        int[] destinationCounts = destinationBefore.Select(
                            value => value.Count).ToArray();
                        var transferRows = new JArray();
                        bool refused = true;
                        foreach (UnitEntityData unit in group)
                        foreach (ItemEntity item in unit.Descriptor.Inventory.Items
                            .ToArray())
                        {
                            var source = unit.Descriptor.Inventory;
                            ItemEntity moved = null;
                            string error = null;
                            bool slotBlocks = item.HoldingSlot != null &&
                                !item.HoldingSlot.CanRemoveItem();
                            try { moved = source.Transfer(item, item.Count,
                                destination); }
                            catch (Exception exception)
                            { error = exception.GetType().Name; }
                            bool row = error == null && moved == null &&
                                slotBlocks &&
                                ReferenceEquals(item.Owner, unit.Descriptor) &&
                                Sprint28InventoryExact(destination,
                                    destinationBefore, destinationCounts) &&
                                Sprint28PartyInventoryExact(partyBefore,
                                    partyCounts);
                            refused &= row;
                            transferRows.Add(new JObject {
                                ["unit"] = unit.UniqueId,
                                ["item"] = item.Blueprint.AssetGuid,
                                ["slotBlocks"] = slotBlocks,
                                ["returnedItem"] = moved == null ? null :
                                    RuntimeHelpers.GetHashCode(moved).ToString(),
                                ["error"] = error,
                                ["refused"] = row });
                            if (moved != null &&
                                ReferenceEquals(moved.Collection, destination))
                            {
                                destination.Remove(moved);
                                moved.Dispose();
                            }
                        }
                        Sprint25Check("s29-" + key + "-transfer-refused",
                            refused, new JObject {
                                ["cases"] = transferRows,
                                ["destinationExact"] = Sprint28InventoryExact(
                                    destination, destinationBefore,
                                    destinationCounts),
                                ["partyExact"] = Sprint28PartyInventoryExact(
                                    partyBefore, partyCounts) },
                            "Native transfer refuses every exact-owned equipment and projectile instance without touching unrelated inventories.");
                        if (!refused) throw new InvalidOperationException(
                            "Sprint 29 summon equipment crossed a transfer boundary.");

                        UnitEntityData dead = group[0];
                        GameHelper.KillUnit(dead, fixture.Hostile);
                        for (int frame = 0; frame < 80; frame++) yield return 0;
                        bool death = dead.Descriptor.State.IsDead &&
                            !dead.IsDeadAndHasLoot &&
                            owned.All(value => !value.IsLootable) &&
                            Sprint28PartyInventoryExact(partyBefore,
                                partyCounts);
                        Sprint25Check("s29-" + key + "-death-no-loot",
                            death, new JObject {
                                ["unit"] = dead.UniqueId,
                                ["dead"] = dead.Descriptor.State.IsDead,
                                ["corpseHasLoot"] = dead.IsDeadAndHasLoot,
                                ["ownedLootable"] = owned.Count(value =>
                                    value.IsLootable),
                                ["partyExact"] = Sprint28PartyInventoryExact(
                                    partyBefore, partyCounts) },
                            "Native death does not make exact-owned new giant items collectible or change legitimate party inventory.");

                        UnitEntityData[] expiring = group.Skip(1).ToArray();
                        if (expiring.Length == 0)
                        {
                            UnitEntityData extra =
                                CastExpandedSummoningOwnTier(fixture, key);
                            SetExpandedSummoningBrainActive(extra, false);
                            expiring = new[] { extra };
                        }
                        var expiryRows = new JArray();
                        foreach (UnitEntityData unit in expiring)
                        {
                            Buff marker = unit.Buffs.Enumerable.Single(value =>
                                ReferenceEquals(value.Blueprint,
                                    BlueprintRoot.Instance.SystemMechanics
                                        .SummonedUnitBuff));
                            expiryRows.Add(ExpireSprint16OwnedBuff(unit,
                                marker));
                        }
                        DateTime expiryDeadline = DateTime.UtcNow.AddSeconds(18);
                        for (int frame = 0; frame < 2500 &&
                            DateTime.UtcNow < expiryDeadline &&
                            expiring.Any(value => !value.Destroyed ||
                                value.View != null ||
                                value.HoldingState != null); frame++)
                            yield return 0;
                        bool expired = expiring.All(value =>
                            value.Destroyed && value.View == null &&
                            value.HoldingState == null) &&
                            owned.All(value => !value.IsLootable) &&
                            Sprint28PartyInventoryExact(partyBefore,
                                partyCounts);
                        Sprint25Check("s29-" + key + "-expiry-no-loot",
                            expired, new JObject {
                                ["expired"] = new JArray(expiring.Select(
                                    value => new JObject {
                                        ["unit"] = value.UniqueId,
                                        ["destroyed"] = value.Destroyed,
                                        ["viewAbsent"] = value.View == null })),
                                ["nativeExpiries"] = expiryRows,
                                ["ownedLootable"] = owned.Count(value =>
                                    value.IsLootable),
                                ["partyExact"] = Sprint28PartyInventoryExact(
                                    partyBefore, partyCounts) },
                            "Native summon expiry removes exact actors and their nonlootable equipment without changing party items.");
                    }
                    finally
                    {
                        foreach (UnitEntityData unit in group)
                        {
                            if (!unit.Destroyed)
                                unit.Commands.InterruptAll(true);
                            if (!unit.Destroyed && unit.IsInCombat)
                                unit.LeaveCombat();
                        }
                        DisposeExpandedSummoningUnits(fixture.Created, group);
                    }
                }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private static JArray Sprint29ItemSlots(UnitEntityData unit)
        {
            return new JArray(unit.Descriptor.Inventory.Items.Select(item =>
            {
                var slots = new JArray();
                for (int index = 0;
                    index < unit.Body.HandsEquipmentSets.Count; index++)
                {
                    var set = unit.Body.HandsEquipmentSets[index];
                    if (ReferenceEquals(set.PrimaryHand?.MaybeWeapon, item))
                        slots.Add(index + ":main");
                    if (ReferenceEquals(set.SecondaryHand?.MaybeWeapon, item))
                        slots.Add(index + ":off");
                }
                if (ReferenceEquals(unit.Body.Armor?.MaybeArmor, item))
                    slots.Add("armor");
                return new JObject {
                    ["blueprint"] = item.Blueprint?.AssetGuid,
                    ["instance"] = RuntimeHelpers.GetHashCode(item),
                    ["holdingSlot"] = item.HoldingSlot?.GetType().Name,
                    ["slots"] = slots
                };
            }));
        }
    }
}
