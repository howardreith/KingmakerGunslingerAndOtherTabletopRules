using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The native maneuver, buff, item ownership and alternative hand-set
        // are observed on request-owned actors; no party item is borrowed.
        private IEnumerable<int> ReviewSprint28Disarm(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            UnitEntityData source = fixture.Hostile;
            int originalBab = source.Stats.BaseAttackBonus.BaseValue;
            ItemEntity[] party = Game.Instance.Player.Inventory.Items.ToArray();
            int[] partyCounts = party.Select(value => value.Count).ToArray();
            ItemEntity[] sourceItems = source.Descriptor.Inventory.Items.ToArray();
            int[] sourceCounts = sourceItems.Select(value => value.Count).ToArray();
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                source.Stats.BaseAttackBonus.BaseValue = 50;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (string key in new[] { "hill-giant", "stone-giant", "fire-giant" })
                {
                    UnitEntityData giant = CastExpandedSummoningOwnTier(fixture, key);
                    SetExpandedSummoningBrainActive(giant, false);
                    try
                    {
                        ItemEntity[] items = giant.Descriptor.Inventory.Items.ToArray();
                        int[] counts = items.Select(value => value.Count).ToArray();
                        ItemEntityWeapon great = giant.Body.PrimaryHand.MaybeWeapon;
                        bool ready = great != null &&
                            ReferenceEquals(great.Blueprint, giant.Blueprint.Body.PrimaryHand) &&
                            ReferenceEquals(great.Owner, giant.Descriptor) &&
                            source.CanAttack(giant) &&
                            Sprint28InventoryExact(giant.Descriptor.Inventory, items, counts);
                        Sprint25Check("s28-" + key + "-disarm-precondition", ready,
                            new JObject { ["giant"] = giant.UniqueId,
                                ["source"] = source.UniqueId,
                                ["weapon"] = great?.Blueprint.AssetGuid,
                                ["sourceCanAttack"] = source.CanAttack(giant),
                                ["itemCount"] = items.Length },
                            "A manufactured exact-owned weapon and disposable legal opponent exist before native Disarm.");
                        if (!ready) throw new InvalidOperationException(
                            "Native giant disarm prerequisites are absent.");
                        var attempts = new JArray();
                        RuleCombatManeuver maneuver = null;
                        for (int attempt = 0; attempt < 5; attempt++)
                        {
                            maneuver = Rulebook.Trigger(new RuleCombatManeuver(
                                source, giant, CombatManeuver.Disarm));
                            attempts.Add(new JObject { ["success"] = maneuver.Success,
                                ["partial"] = maneuver.IsPartialSuccess,
                                ["nativeBuff"] = giant.Buffs.Enumerable.Any(value =>
                                    ReferenceEquals(value.Blueprint, BlueprintRoot.Instance
                                        .SystemMechanics.DisarmMainHandBuff)) });
                            if (maneuver.Success) break;
                        }
                        bool disabled = giant.Buffs.Enumerable.Any(value =>
                            ReferenceEquals(value.Blueprint, BlueprintRoot.Instance
                                .SystemMechanics.DisarmMainHandBuff));
                        bool retained = maneuver != null && maneuver.Success && disabled &&
                            Sprint28InventoryExact(giant.Descriptor.Inventory, items, counts) &&
                            items.All(value => ReferenceEquals(value.Owner, giant.Descriptor) &&
                                value.IsNonRemovable && !value.IsLootable) &&
                            ReferenceEquals(great.Collection, giant.Descriptor.Inventory) &&
                            Sprint28InventoryExact(source.Descriptor.Inventory,
                                sourceItems, sourceCounts) &&
                            Sprint28PartyInventoryExact(party, partyCounts);
                        Sprint25Check("s28-" + key + "-native-disarm-items", retained,
                            new JObject { ["giant"] = giant.UniqueId,
                                ["source"] = source.UniqueId, ["attempts"] = attempts,
                                ["buff"] = disabled,
                                ["greatInstance"] = RuntimeHelpers.GetHashCode(great),
                                ["greatOwner"] = great.Owner?.Unit?.UniqueId,
                                ["greatCollection"] = great.Collection?.GetType().Name,
                                ["ownedInventoryExact"] = Sprint28InventoryExact(
                                    giant.Descriptor.Inventory, items, counts),
                                ["sourceInventoryExact"] = Sprint28InventoryExact(
                                    source.Descriptor.Inventory, sourceItems, sourceCounts),
                                ["partyInventoryExact"] =
                                    Sprint28PartyInventoryExact(party, partyCounts) },
                            "Native Disarm disables the main hand but leaves every summon item owned, nonlootable and out of unrelated inventories.");
                        if (!retained) throw new InvalidOperationException(
                            "Native Disarm did not preserve exact summon equipment.");
                        var hand = new UnitSwitchHandEquipmentSet(1);
                        giant.Commands.Run(hand);
                        DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                        for (int frame = 0; frame < 1000 &&
                            !hand.IsFinished && DateTime.UtcNow < deadline; frame++)
                            yield return 0;
                        ItemEntityWeapon slam = giant.Body.PrimaryHand.MaybeWeapon;
                        bool fallback = hand.IsFinished &&
                            giant.Body.CurrentHandEquipmentSetIndex == 1 &&
                            slam != null && ReferenceEquals(slam.Blueprint,
                                giant.Blueprint.Body.PrimaryHandAlternative1) &&
                            ReferenceEquals(slam.Owner, giant.Descriptor) &&
                            Sprint28InventoryExact(giant.Descriptor.Inventory, items, counts) &&
                            Sprint28PartyInventoryExact(party, partyCounts);
                        Sprint25Check("s28-" + key + "-disarmed-slam", fallback,
                            new JObject { ["giant"] = giant.UniqueId,
                                ["switchFinished"] = hand.IsFinished,
                                ["switchResult"] = hand.Result.ToString(),
                                ["activeSet"] = giant.Body.CurrentHandEquipmentSetIndex,
                                ["slam"] = slam?.Blueprint.AssetGuid,
                                ["slamOwner"] = slam?.Owner?.Unit?.UniqueId,
                                ["ownedInventoryExact"] = Sprint28InventoryExact(
                                    giant.Descriptor.Inventory, items, counts),
                                ["partyInventoryExact"] =
                                    Sprint28PartyInventoryExact(party, partyCounts) },
                            "A disarmed giant can select its own alternative slams without looting or changing party items.");
                    }
                    finally
                    {
                        giant.Commands.InterruptAll(true);
                        if (giant.IsInCombat) giant.LeaveCombat();
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { giant });
                    }
                }
            }
            finally
            {
                source.Stats.BaseAttackBonus.BaseValue = originalBab;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = mode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = pause;
            }
        }
    }
}
