using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewSprint29Disarm(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            UnitEntityData source = fixture.Hostile;
            int oldBab = source.Stats.BaseAttackBonus.BaseValue;
            ItemEntity[] sourceItems = source.Descriptor.Inventory.Items.ToArray();
            int[] sourceCounts = sourceItems.Select(item => item.Count).ToArray();
            ItemEntity[] partyItems = Game.Instance.Player.Inventory.Items.ToArray();
            int[] partyCounts = partyItems.Select(item => item.Count).ToArray();
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                source.Stats.BaseAttackBonus.BaseValue = 50;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (string key in new[] { "ettin", "cloud-giant",
                    "storm-giant" })
                {
                    ResetExpandedSummoningHostile(fixture);
                    source.Stats.BaseAttackBonus.BaseValue = 50;
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, key);
                    SetExpandedSummoningBrainActive(owner, false);
                    try
                    {
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        foreach (int frame in Sprint22SetPowerAttack(owner,
                            false)) yield return frame;
                        // Hydrate the alternate private items before taking
                        // the exact item snapshot. Switching later must not
                        // be mistaken for disarm resupply.
                        foreach (int index in key == "ettin" ?
                            new[] { 1, 0 } : new[] { 1, 2, 0 })
                        {
                            var switchCommand =
                                new UnitSwitchHandEquipmentSet(index);
                            owner.Commands.Run(switchCommand);
                            DateTime switchDeadline = DateTime.UtcNow
                                .AddSeconds(20);
                            for (int frame = 0; frame < 1000 &&
                                !switchCommand.IsFinished &&
                                DateTime.UtcNow < switchDeadline; frame++)
                                yield return 0;
                            if (!switchCommand.IsFinished ||
                                switchCommand.Result !=
                                    UnitCommand.ResultType.Success ||
                                owner.Body.CurrentHandEquipmentSetIndex !=
                                    index)
                                throw new InvalidOperationException(
                                    "Sprint 29 disarm setup switch failed: " +
                                    key + "/" + index);
                        }
                        ItemEntity[] items = owner.Descriptor.Inventory.Items
                            .ToArray();
                        int[] counts = items.Select(item => item.Count)
                            .ToArray();
                        ItemEntityWeapon main = owner.Body.PrimaryHand
                            .MaybeWeapon;
                        ItemEntityWeapon off = owner.Body.SecondaryHand
                            .MaybeWeapon;
                        bool precondition = main != null &&
                            ReferenceEquals(main.Blueprint,
                                owner.Blueprint.Body.PrimaryHand) &&
                            ReferenceEquals(main.Owner, owner.Descriptor) &&
                            (key != "ettin" || off != null &&
                                !ReferenceEquals(off, main) &&
                                ReferenceEquals(off.Owner,
                                    owner.Descriptor)) &&
                            source.CanAttack(owner) &&
                            Sprint28InventoryExact(owner.Descriptor.Inventory,
                                items, counts) &&
                            Sprint28InventoryExact(source.Descriptor.Inventory,
                                sourceItems, sourceCounts) &&
                            Sprint28PartyInventoryExact(partyItems,
                                partyCounts);
                        Sprint25Check("s29-" + key + "-disarm-setup",
                            precondition, new JObject {
                                ["owner"] = owner.UniqueId,
                                ["sourceCanAttack"] = source.CanAttack(owner),
                                ["main"] = main?.Blueprint?.AssetGuid,
                                ["off"] = off?.Blueprint?.AssetGuid,
                                ["itemCount"] = items.Length },
                            "Native Disarm starts with distinct exact-owned armament and unchanged unrelated inventories.");
                        if (!precondition) throw new InvalidOperationException(
                            "Sprint 29 disarm setup failed: " + key);

                        var attempts = new JArray();
                        RuleCombatManeuver maneuver = null;
                        for (int attempt = 0; attempt < 5; attempt++)
                        {
                            maneuver = Rulebook.Trigger(new RuleCombatManeuver(
                                source, owner, CombatManeuver.Disarm));
                            attempts.Add(new JObject {
                                ["success"] = maneuver.Success,
                                ["partial"] = maneuver.IsPartialSuccess });
                            if (maneuver.Success) break;
                        }
                        bool disabled = owner.Descriptor.Buffs.GetBuff(
                            BlueprintRoot.Instance.SystemMechanics
                                .DisarmMainHandBuff) != null;
                        bool retained = maneuver != null && maneuver.Success &&
                            disabled &&
                            Sprint28InventoryExact(owner.Descriptor.Inventory,
                                items, counts) &&
                            items.All(item => ReferenceEquals(item.Owner,
                                owner.Descriptor) && item.IsNonRemovable &&
                                !item.IsLootable) &&
                            Sprint28InventoryExact(source.Descriptor.Inventory,
                                sourceItems, sourceCounts) &&
                            Sprint28PartyInventoryExact(partyItems,
                                partyCounts);
                        Sprint25Check("s29-" + key + "-disarm-owned",
                            retained, new JObject {
                                ["owner"] = owner.UniqueId,
                                ["attempts"] = attempts,
                                ["nativeMainHandBuff"] = disabled,
                                ["ownedInventoryExact"] =
                                    Sprint28InventoryExact(
                                        owner.Descriptor.Inventory, items,
                                        counts),
                                ["sourceInventoryExact"] =
                                    Sprint28InventoryExact(
                                        source.Descriptor.Inventory,
                                        sourceItems, sourceCounts),
                                ["partyExact"] =
                                    Sprint28PartyInventoryExact(partyItems,
                                        partyCounts) },
                            "Native Disarm disables the main hand while every summon item stays owned and unlootable.");
                        if (!retained) throw new InvalidOperationException(
                            "Sprint 29 native disarm item boundary failed: " +
                            key);

                        if (key == "ettin")
                        {
                            var attack = new UnitAttack(source) {
                                ForceFullAttack = true };
                            attack.Init(owner);
                            var plan = attack.CreateFullAttack();
                            JObject remainingRule = Sprint29WeaponRule(owner,
                                off);
                            bool oneArm = off != null &&
                                plan.Count == 2 &&
                                plan.All(row => ReferenceEquals(row.Hand,
                                    owner.Body.SecondaryHand)) &&
                                (int)remainingRule["attack"] == 12 &&
                                (int)remainingRule["damageBonus"] == 6 &&
                                owner.Get<UnitPartSummonEttinInitialArms>()
                                    ?.FlailSeeded == true &&
                                Sprint28InventoryExact(
                                    owner.Descriptor.Inventory, items, counts);
                            Sprint25Check("s29-ettin-disarmed-one-arm-plan",
                                oneArm, new JObject {
                                    ["owner"] = owner.UniqueId,
                                    ["planned"] = plan.Count,
                                    ["main"] = plan.Count(row =>
                                        ReferenceEquals(row.Hand,
                                            owner.Body.PrimaryHand)),
                                    ["off"] = plan.Count(row =>
                                        ReferenceEquals(row.Hand,
                                            owner.Body.SecondaryHand)),
                                    ["remainingRule"] = remainingRule,
                                    ["flailSeeded"] = owner.Get<
                                        UnitPartSummonEttinInitialArms>()
                                        ?.FlailSeeded,
                                    ["itemsExact"] = Sprint28InventoryExact(
                                        owner.Descriptor.Inventory, items,
                                        counts) },
                                "The disabled primary cannot contribute a second turn or attacks; the remaining flail retains its own two iteratives.");
                            if (!oneArm) throw new InvalidOperationException(
                                "Ettin disarmed full-attack plan is unsafe.");
                            foreach (int frame in Sprint29DisarmedEttinCommand(
                                fixture, owner, source, off, items, counts,
                                partyItems, partyCounts)) yield return frame;
                        }
                        else
                        {
                            var fallback = new UnitSwitchHandEquipmentSet(1);
                            owner.Commands.Run(fallback);
                            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                            for (int frame = 0; frame < 1000 &&
                                !fallback.IsFinished &&
                                DateTime.UtcNow < deadline; frame++)
                                yield return 0;
                            ItemEntityWeapon slam = owner.Body.PrimaryHand
                                .MaybeWeapon;
                            var attack = new UnitAttack(source);
                            attack.Init(owner);
                            var plan = attack.CreateFullAttack();
                            bool safe = fallback.IsFinished &&
                                fallback.Result ==
                                    UnitCommand.ResultType.Success &&
                                owner.Body.CurrentHandEquipmentSetIndex == 1 &&
                                slam != null &&
                                ReferenceEquals(slam.Blueprint,
                                    owner.Blueprint.Body
                                        .PrimaryHandAlternative1) &&
                                plan.Count == 2 &&
                                Sprint28InventoryExact(
                                    owner.Descriptor.Inventory, items, counts) &&
                                Sprint28PartyInventoryExact(partyItems,
                                    partyCounts);
                            Sprint25Check("s29-" + key +
                                "-disarmed-slam-fallback", safe,
                                new JObject {
                                    ["owner"] = owner.UniqueId,
                                    ["switchFinished"] =
                                        fallback.IsFinished,
                                    ["activeSet"] = owner.Body
                                        .CurrentHandEquipmentSetIndex,
                                    ["slam"] = slam?.Blueprint?.AssetGuid,
                                    ["planned"] = plan.Count,
                                    ["itemsExact"] = Sprint28InventoryExact(
                                        owner.Descriptor.Inventory, items,
                                        counts) },
                                "A disarmed Cloud or Storm selects its distinct two-slam alternative without item resupply.");
                            if (!safe) throw new InvalidOperationException(
                                "Sprint 29 disarmed slam fallback failed: " +
                                key);
                        }
                    }
                    finally
                    {
                        if (!owner.Destroyed)
                            owner.Commands.InterruptAll(true);
                        if (!owner.Destroyed && owner.IsInCombat)
                            owner.LeaveCombat();
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                    }
                }
            }
            finally
            {
                source.Stats.BaseAttackBonus.BaseValue = oldBab;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> Sprint29DisarmedEttinCommand(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target,
            ItemEntityWeapon off, ItemEntity[] items, int[] counts,
            ItemEntity[] partyItems, int[] partyCounts)
        {
            Vector3 far;
            string lane, direction;
            Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                owner, target, 8f, out far, out lane, out direction);
            Vector3 forward = (far - origin).normalized;
            PlaceExpandedSummoningUnit(owner, origin);
            PlaceExpandedSummoningUnit(target, origin + forward *
                (owner.Corpulence + target.Corpulence + .8f));
            owner.SightCache.ClearIfNeeded();
            target.SightCache.ClearIfNeeded();
            var probe = new UnitAttack(target);
            probe.Init(owner);
            bool legal = owner.HasLOS(target) &&
                probe.IsUnitEnoughClose && owner.IsEnemy(target) &&
                !target.Descriptor.State.IsDead;
            Sprint25Check("s29-ettin-disarmed-lane", legal,
                new JObject { ["owner"] = owner.UniqueId,
                    ["target"] = target.UniqueId,
                    ["lane"] = lane, ["direction"] = direction,
                    ["los"] = owner.HasLOS(target),
                    ["inRange"] = probe.IsUnitEnoughClose },
                "The remaining offhand has a visible legal native command lane.");
            if (!legal) throw new InvalidOperationException(
                "Disarmed Ettin has no legal target lane.");
            owner.JoinCombat(); target.JoinCombat();
            owner.Memory.Add(target); target.Memory.Add(owner);
            var observer = new Sprint24AttackObserver {
                Owner = owner, Target = target };
            var costs = new JArray();
            var endings = new JArray();
            var command = new UnitAttack(target) { ForceFullAttack = true };
            EventBus.Subscribe(observer);
            try
            {
                using (var economy = new FirearmReviewCosts(owner, costs))
                using (var trace = new Sprint26CommandTrace(owner, endings))
                {
                    owner.Commands.Run(command);
                    DateTime deadline = DateTime.UtcNow.AddSeconds(35);
                    int frames = 0;
                    while (frames++ < 3500 && DateTime.UtcNow < deadline &&
                        (!command.IsFinished ||
                         observer.WeaponAttacks.Count < 2))
                        yield return 0;
                    int hits = observer.WeaponAttacks.Count(value =>
                        value.AttackRoll != null && value.AttackRoll.IsHit);
                    DateTime impactDeadline = DateTime.UtcNow.AddSeconds(10);
                    for (int frame = 0; frame < 600 &&
                        observer.Damage.Count < hits &&
                        DateTime.UtcNow < impactDeadline; frame++)
                        yield return 0;
                    bool exact = command.IsFinished &&
                        (command.Result == UnitCommand.ResultType.Success ||
                         trace.Exhausted(command)) &&
                        command.AllAttacks.Count == 2 &&
                        observer.OwnerWeaponAttacks.Count == 2 &&
                        observer.WeaponAttacks.Count == 2 &&
                        observer.WeaponAttacks.All(value =>
                            ReferenceEquals(value.Weapon, off)) &&
                        observer.Damage.Count == hits &&
                        economy.Count(command) == 1 &&
                        economy.Exact(command, false) &&
                        Sprint28InventoryExact(owner.Descriptor.Inventory,
                            items, counts) &&
                        Sprint28PartyInventoryExact(partyItems, partyCounts) &&
                        !target.Descriptor.State.IsDead;
                    Sprint25Check("s29-ettin-disarmed-one-arm-command",
                        exact, new JObject {
                            ["owner"] = owner.UniqueId,
                            ["target"] = target.UniqueId,
                            ["finished"] = command.IsFinished,
                            ["result"] = command.Result.ToString(),
                            ["planned"] = command.AllAttacks.Count,
                            ["ownerAttacks"] =
                                observer.OwnerWeaponAttacks.Count,
                            ["targetAttacks"] = observer.WeaponAttacks.Count,
                            ["weapons"] = new JArray(
                                observer.WeaponAttacks.Select(value =>
                                    value.Weapon?.Blueprint?.AssetGuid)),
                            ["rolls"] = new JArray(
                                observer.WeaponAttacks.Select(value =>
                                    new JObject {
                                        ["natural"] = value.AttackRoll?
                                            .Roll.Value,
                                        ["attackBonus"] = value.AttackRoll?
                                            .AttackBonus,
                                        ["hit"] = value.AttackRoll?.IsHit })),
                            ["hits"] = hits,
                            ["damagePackets"] = observer.Damage.Count,
                            ["costs"] = costs,
                            ["nativeEnds"] = endings,
                            ["itemsExact"] = Sprint28InventoryExact(
                                owner.Descriptor.Inventory, items, counts),
                            ["partyExact"] = Sprint28PartyInventoryExact(
                                partyItems, partyCounts) },
                        "One native full-attack command uses only the remaining flail twice and spends one action without recreating items.");
                    if (!exact) throw new InvalidOperationException(
                        "Disarmed Ettin command violated the one-arm bound.");
                }
            }
            finally { EventBus.Unsubscribe(observer); }
        }
    }
}
