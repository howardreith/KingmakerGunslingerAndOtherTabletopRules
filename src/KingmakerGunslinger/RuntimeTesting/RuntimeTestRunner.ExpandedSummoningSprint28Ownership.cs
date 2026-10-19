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
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The quantity route allocates real equipment instances. Death and
        // scheduled summon expiry run before observing the item boundary.
        private IEnumerable<int> ReviewSprint28Ownership(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            var inventory = Game.Instance.Player.Inventory;
            ItemEntity[] before = inventory.Items.ToArray();
            int[] counts = before.Select(value => value.Count).ToArray();
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (string key in new[] { "hill-giant", "stone-giant",
                    "fire-giant" })
                {
                    var route = ExpandedSummoningOwnTierVariant(key,
                        SummonMultiplicity.OneD4PlusOne);
                    UnitEntityData[] group = CastExpandedSummoningVariant(
                        fixture.Blueprints, fixture.Caster, route, null,
                        fixture.Evidence);
                    fixture.Created.AddRange(group);
                    try
                    {
                        foreach (UnitEntityData unit in group)
                            SetExpandedSummoningBrainActive(unit, false);
                        for (int frame = 0; frame < 12; frame++) yield return 0;
                        var rows = new JArray();
                        var all = new List<ItemEntity>();
                        bool owned = group.Length >= 2 && group.Length <= 5;
                        foreach (UnitEntityData unit in group)
                        {
                            var primaries = unit.Body.HandsEquipmentSets.Take(3)
                                .Select(value => value?.PrimaryHand?.MaybeWeapon)
                                .ToArray();
                            ItemEntity armor = unit.Body.Armor?.MaybeArmor;
                            var items = primaries.Cast<ItemEntity>()
                                .Concat(armor == null ? Array.Empty<ItemEntity>() :
                                    new[] { armor }).ToArray();
                            owned &= primaries.Length == 3 &&
                                primaries.All(value => value != null) &&
                                primaries.Distinct().Count() == 3 &&
                                ((key == "stone-giant") == (armor == null)) &&
                                items.All(value => value != null &&
                                    ReferenceEquals(value.Owner, unit.Descriptor) &&
                                    value.IsNonRemovable && !value.IsLootable);
                            all.AddRange(items);
                            rows.Add(new JObject {
                                ["unit"] = unit.UniqueId,
                                ["items"] = new JArray(items.Select(value =>
                                    value == null ? null : new JObject {
                                        ["blueprint"] = value.Blueprint.AssetGuid,
                                        ["instance"] = RuntimeHelpers.GetHashCode(value),
                                        ["owner"] = value.Owner?.Unit?.UniqueId,
                                        ["collection"] = value.Collection?.GetType().Name,
                                        ["nonRemovable"] = value.IsNonRemovable,
                                        ["lootable"] = value.IsLootable })),
                                ["unitInventory"] = new JArray(unit.Descriptor
                                    .Inventory.Items.Select(value =>
                                        value.Blueprint.AssetGuid)) });
                        }
                        owned &= all.Count == all.Distinct().Count() &&
                            Sprint28PartyInventoryExact(before, counts);
                        Sprint25Check("s28-" + key + "-quantity-items", owned,
                            new JObject { ["count"] = group.Length,
                                ["actors"] = rows,
                                ["distinctItems"] = all.Distinct().Count(),
                                ["partyInventoryExact"] =
                                    Sprint28PartyInventoryExact(before, counts) },
                            "The highest quantity route creates distinct exact-owner weapon and armor instances with native no-loot and no-removal flags, leaving the party inventory untouched.");
                        UnitEntityData dead = group[0];
                        GameHelper.KillUnit(dead, fixture.Hostile);
                        for (int frame = 0; frame < 80; frame++) yield return 0;
                        bool death = dead.Descriptor.State.IsDead &&
                            all.All(value => !value.IsLootable) &&
                            Sprint28PartyInventoryExact(before, counts) &&
                            !inventory.Items.Any(value => all.Contains(value));
                        Sprint25Check("s28-" + key + "-death-items", death,
                            new JObject { ["unit"] = dead.UniqueId,
                                ["dead"] = dead.Descriptor.State.IsDead,
                                ["corpseInventory"] = new JArray(dead.Descriptor
                                    .Inventory.Items.Select(value =>
                                        new JObject {
                                            ["blueprint"] = value.Blueprint.AssetGuid,
                                            ["instance"] = RuntimeHelpers.GetHashCode(value),
                                            ["lootable"] = value.IsLootable })),
                                ["ownedItemsLootable"] = all.Count(value =>
                                    value.IsLootable),
                                ["partyInventoryExact"] =
                                    Sprint28PartyInventoryExact(before, counts) },
                            "Native death does not turn an exact giant item lootable or transfer it into party inventory.");
                        var expiries = new JArray();
                        foreach (UnitEntityData unit in group.Skip(1))
                        {
                            Buff marker = unit.Buffs.Enumerable.Single(value =>
                                ReferenceEquals(value.Blueprint,
                                    BlueprintRoot.Instance.SystemMechanics
                                        .SummonedUnitBuff));
                            expiries.Add(ExpireSprint16OwnedBuff(unit, marker));
                        }
                        DateTime deadline = DateTime.UtcNow.AddSeconds(18);
                        for (int frame = 0; frame < 2500 &&
                            DateTime.UtcNow < deadline &&
                            group.Skip(1).Any(value => !value.Destroyed ||
                                value.View != null || value.HoldingState != null);
                            frame++) yield return 0;
                        bool expired = group.Skip(1).All(value =>
                            value.Destroyed && value.View == null &&
                            value.HoldingState == null) &&
                            all.All(value => !value.IsLootable) &&
                            Sprint28PartyInventoryExact(before, counts) &&
                            !inventory.Items.Any(value => all.Contains(value));
                        Sprint25Check("s28-" + key + "-expiry-items", expired,
                            new JObject { ["units"] = new JArray(group.Skip(1)
                                    .Select(value => new JObject {
                                        ["unit"] = value.UniqueId,
                                        ["destroyed"] = value.Destroyed,
                                        ["viewAbsent"] = value.View == null,
                                        ["holdingStateAbsent"] =
                                            value.HoldingState == null })),
                                ["nativeExpiries"] = expiries,
                                ["ownedItemsLootable"] = all.Count(value =>
                                    value.IsLootable),
                                ["partyInventoryExact"] =
                                    Sprint28PartyInventoryExact(before, counts) },
                            "Native summon expiration removes exact quantity actors without releasing owned items to party inventory.");
                    }
                    finally
                    {
                        foreach (UnitEntityData unit in group)
                        {
                            if (!unit.Destroyed) unit.Commands.InterruptAll(true);
                            if (!unit.Destroyed && unit.IsInCombat)
                                unit.LeaveCombat();
                        }
                        DisposeExpandedSummoningUnits(fixture.Created, group);
                    }
                }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = mode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = pause;
            }
        }

        private static bool Sprint28PartyInventoryExact(ItemEntity[] before,
            int[] counts)
        {
            ItemEntity[] after = Game.Instance.Player.Inventory.Items.ToArray();
            return after.Length == before.Length &&
                after.Select((value, index) =>
                    ReferenceEquals(value, before[index]) &&
                    value.Count == counts[index]).All(value => value);
        }
    }
}
