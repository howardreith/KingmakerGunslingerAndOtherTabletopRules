using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewInfernalCombat34(ExpandedSummoningCorrectionFixture fixture)
        {
            var owners = new List<UnitEntityData>(); var trace = new InfernalEvents34(); var cells = new JArray();
            var receipt = new JObject { ["phase"] = "sprint34-infernal-combat", ["cells"] = cells, ["events"] = trace.Rows };
            Action<string, bool, JObject, string> check = (id, pass, row, description) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint34-" + id, pass, row, description);
                if (!pass) throw new InvalidOperationException("Sprint34 native combat: " + id);
            };
            try {
                foreach (string key in new[] { InfernalVisualPolicy34.Barbazu, InfernalVisualPolicy34.Kyton }) {
                    var owner = CastExpandedSummoningOwnTier(fixture, key); owners.Add(owner);
                    Sprint28SetBrainActivePreservingActions(owner, false); owner.Commands.InterruptAll(true);
                    foreach (int frame in Outsider30NativeAppearance(owner, key)) yield return frame;
                    var profile = InfernalMartialProfile34.For(owner); bool barbazu = profile.Glaive != null;
                    foreach (int frame in ReviewInfernalWeaponStats34(owner, check)) yield return frame;
                    foreach (int frame in InfernalSwitch34(owner, barbazu ? profile.Glaive : profile.Chain)) yield return frame;
                    if (barbazu) foreach (int frame in Sprint22SetPowerAttack(owner, true)) yield return frame;
                    foreach (int frame in InfernalSingleAndOpportunity34(fixture, owner, trace, check, key, 8)) yield return frame;
                    foreach (int frame in InfernalCharge34(fixture, owner, check, key)) yield return frame;
                    if (barbazu) {
                        foreach (int frame in InfernalSwitch34(owner, profile.Claw)) yield return frame;
                        foreach (int frame in InfernalSingleAndOpportunity34(fixture, owner, trace, check, "barbazu-claws", 3)) yield return frame;
                        foreach (int frame in InfernalBeardDisease34(fixture, owner, trace, check)) yield return frame;
                    } else {
                        var ability = InfernalSavedBlueprint34<BlueprintAbility>(true, "DancingChains");
                        var control = new UnitUseAbility(new AbilityData(owner.Descriptor.Abilities.GetAbility(ability)), new TargetWrapper(owner));
                        owner.Commands.Run(control);
                        foreach (int frame in WaitInfernalCommand34(control, () => profile.ControlsOwnChains(owner), 20)) yield return frame;
                        foreach (int frame in InfernalSingleAndOpportunity34(fixture, owner, trace, check, "kyton-controlled", 20)) yield return frame;
                    }
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }); owners.Remove(owner);
                    ResetExpandedSummoningHostile(fixture);
                }
            } finally {
                trace.Dispose(); DisposeExpandedSummoningUnits(fixture.Created, owners); ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "sprint34-infernal-combat.json"), receipt.ToString(Formatting.Indented));
            }
        }
        private IEnumerable<int> InfernalSwitch34(UnitEntityData owner, BlueprintItemWeapon weapon)
        {
            if (ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon?.Blueprint, weapon)) yield break;
            int index = Enumerable.Range(0, owner.Body.HandsEquipmentSets.Count).Single(value =>
                ReferenceEquals(owner.Body.HandsEquipmentSets[value].PrimaryHand.MaybeItem?.Blueprint, weapon));
            var command = new UnitSwitchHandEquipmentSet(index); owner.Commands.Run(command);
            foreach (int frame in WaitInfernalCommand34(command, () => ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon?.Blueprint, weapon), 20)) yield return frame;
            if (command.Result != UnitCommand.ResultType.Success) throw new InvalidOperationException("Native weapon switch failed.");
        }
        private IEnumerable<int> ReviewInfernalWeaponStats34(UnitEntityData owner, Action<string, bool, JObject, string> check)
        {
            var profile = InfernalMartialProfile34.For(owner); bool barbazu = profile.Glaive != null;
            int strength = owner.Stats.Strength.BaseValue;
            try {
                foreach (var weapon in barbazu ? new[] { profile.Glaive, profile.Claw } : new[] { profile.Chain }) {
                    foreach (int frame in InfernalSwitch34(owner, weapon)) yield return frame;
                    if (barbazu) foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                    var items = new[] { owner.Body.PrimaryHand.MaybeWeapon }.Concat(owner.Body.AdditionalLimbs.Select(slot => slot.MaybeWeapon))
                        .Where(item => item != null && ReferenceEquals(item.Blueprint, weapon)).ToArray();
                    foreach (int score in new[] { strength, strength + 4, 7 }) {
                        owner.Stats.Strength.BaseValue = score; int bonus = owner.Stats.Strength.Bonus;
                        bool glaive = ReferenceEquals(weapon, profile.Glaive);
                        var rows = new JArray(items.Select(item => {
                            var row = Sprint29WeaponRule(owner, item);
                            var attack = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner, item, 0));
                            row["sources"] = new JArray(attack.BonusSources.Select(value => new JObject {
                                ["bonus"] = value.Bonus, ["fact"] = value.Source?.Blueprint?.AssetGuid })); return row;
                        }));
                        int expectedAttack = owner.Stats.BaseAttackBonus.ModifiedValue + bonus + (glaive || !barbazu ? 1 : 0);
                        int expectedDamage = glaive && bonus > 0 ? bonus + bonus / 2 : bonus;
                        check("weapon-live-" + weapon.name + "-" + score,
                            rows.All(row => (int)row["attack"] == expectedAttack && (int)row["damageBonus"] == expectedDamage) &&
                            items.All(item => item.AttackRange.Value == (glaive || !barbazu ? 10 : 5)),
                            new JObject { ["owner"] = owner.UniqueId, ["strength"] = score, ["liveModifier"] = bonus,
                                ["expectedAttack"] = expectedAttack, ["expectedDamage"] = expectedDamage, ["weapons"] = rows,
                                ["nativeCalculationOnly"] = true },
                            "Native weapon rules apply live signed Strength, correct main/additional primary damage, and Weapon Focus only to its declared weapon type.");
                    }
                    owner.Stats.Strength.BaseValue = strength;
                    if (barbazu) {
                        var off = new JArray(items.Select(item => Sprint29WeaponRule(owner, item)));
                        foreach (int frame in Sprint22SetPowerAttack(owner, true)) yield return frame;
                        var on = new JArray(items.Select(item => Sprint29WeaponRule(owner, item)));
                        int damage = ReferenceEquals(weapon, profile.Glaive) ? 6 : 4;
                        check("weapon-power-" + weapon.name, Enumerable.Range(0, items.Length).All(index =>
                            (int)on[index]["attack"] == (int)off[index]["attack"] - 2 &&
                            (int)on[index]["damageBonus"] == (int)off[index]["damageBonus"] + damage),
                            new JObject { ["owner"] = owner.UniqueId, ["off"] = off, ["on"] = on },
                            "Real native Power Attack toggle gives its level-six penalty and weapon-specific two-hand or primary-natural damage.");
                    }
                }
            } finally { owner.Stats.Strength.BaseValue = strength; }
        }
        private IEnumerable<int> InfernalSingleAndOpportunity34(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            InfernalEvents34 trace, Action<string, bool, JObject, string> check, string label, int feet)
        {
            var target = fixture.Hostile; owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
            InfernalCombatLane34(fixture, owner, target, feet);
            int first = trace.Rows.Count; var command = new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
            var costs = new JArray();
            using (var economy = new FirearmReviewCosts(owner, costs)) {
                owner.Commands.Run(command);
                foreach (int frame in WaitInfernalCommand34(command, () => true, 25)) yield return frame;
                var attacks = trace.Rows.Skip(first).Where(row => (string)row["event"] == "attack" && (string)row["owner"] == owner.UniqueId).ToArray();
                check("single-" + label, command.Result == UnitCommand.ResultType.Success && attacks.Length == 1 && economy.Count(command) == 1 &&
                    (economy.InCombat(command) ? economy.Exact(command, false) : economy.Unchanged(command)),
                    new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["command"] = Sprint27AiCommand(command),
                        ["attacks"] = new JArray(attacks), ["costs"] = costs }, "One real single-attack command uses the selected weapon and pays its normal action cost.");
            }
            owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
            owner.CombatState.OnNewRound(); owner.CombatState.PreventAttacksOfOpporunityNextFrame = false;
            target.CombatState.PreventAttacksOfOpporunityNextFrame = false;
            for (int frame = 0; frame < 8; frame++) yield return 0;
            var observer = new Sprint24AttackObserver { Owner = owner, Target = target }; EventBus.Subscribe(observer);
            try {
                bool eligible = owner.CombatState.AttackOfOpportunity(target, true);
                bool queued = eligible && owner.CombatState.AttackOfOpportunity(target, false);
                var commands = owner.Commands.Raw.OfType<UnitAttackOfOpportunity>().ToArray();
                DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                while (DateTime.UtcNow < deadline && commands.Any(value => !value.IsFinished)) { Game.Instance.IsPaused = false; yield return 0; }
                check("opportunity-" + label, eligible && queued && commands.Length == 1 && commands[0].IsFinished && observer.WeaponAttacks.Count == 1 &&
                    observer.WeaponAttacks[0].IsAttackOfOpportunity && ReferenceEquals(observer.WeaponAttacks[0].Weapon, owner.Body.PrimaryHand.MaybeWeapon),
                    new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["eligible"] = eligible, ["queued"] = queued,
                        ["feetBeyondBodies"] = feet, ["weaponReach"] = owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Value,
                        ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["attacks"] = observer.WeaponAttacks.Count,
                        ["nativeReactionApi"] = true, ["fixtureAttackRules"] = 0 },
                    "The native reaction API dispatches exactly one opportunity with the actual close or extended weapon range.");
            } finally { EventBus.Unsubscribe(observer); owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue(); }
        }
        private IEnumerable<int> InfernalCharge34(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            Action<string, bool, JObject, string> check, string label)
        {
            var target = fixture.Hostile;
            var blueprint = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.AssetGuid == "c78506dd0e14f7c45a599990e4e65038");
            var grant = owner.Descriptor.Abilities.GetAbility(blueprint);
            if (grant == null) throw new InvalidOperationException("Summoned outsider lacks native Charge; no fixture grant supplied.");
            var data = new AbilityData(grant); Vector3 start; string route;
            var selected = Sprint23FindChargeRoute(fixture, owner, target, data, out start, out route);
            if (selected == null) throw new InvalidOperationException("Native infernal charge lane unavailable.");
            owner.JoinCombat(); target.JoinCombat(); owner.Memory.Add(target); target.Memory.Add(owner);
            var observer = new Sprint24AttackObserver { Owner = owner, Target = target }; EventBus.Subscribe(observer);
            try {
                var costs = new JArray();
                using (var economy = new FirearmReviewCosts(owner, costs)) using (var queue = new Sprint26QueueTrace(owner)) {
                    var command = new UnitUseAbility(data, selected); owner.Commands.Run(command);
                    DateTime deadline = DateTime.UtcNow.AddSeconds(40);
                    while (DateTime.UtcNow < deadline) {
                        if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Tick();
                        Game.Instance.IsPaused = false; yield return 0;
                        if (command.IsFinished && queue.Commands.Count != 0 && queue.Commands.All(value => value.IsFinished)) break;
                    }
                    check("charge-" + label, command.IsFinished && queue.Commands.Count == 1 && queue.Commands[0].IsCharge &&
                        queue.Commands[0].IsFinished && observer.WeaponAttacks.Count == 1 && observer.WeaponAttacks[0].IsCharge &&
                        ReferenceEquals(observer.WeaponAttacks[0].Weapon, owner.Body.PrimaryHand.MaybeWeapon) && economy.Count(command) == 1 && economy.Exact(command, false),
                        new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["ability"] = blueprint.AssetGuid,
                            ["command"] = Sprint27AiCommand(command), ["queued"] = new JArray(queue.Commands.Select(Sprint27AiCommand)),
                            ["attacks"] = observer.WeaponAttacks.Count, ["costs"] = costs, ["route"] = route,
                            ["travel"] = Vector3.Distance(start, owner.Position), ["nativeExecutionProcessPumped"] = true,
                            ["forcedAnimationEvents"] = 0, ["fixtureAttackRules"] = 0 },
                        "Existing native Charge ability moves and queues one normal attack with a real full-round cost; no pounce or extra natural routine.");
                }
            } finally { EventBus.Unsubscribe(observer); owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue(); }
        }
        private IEnumerable<int> InfernalBeardDisease34(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            InfernalEvents34 trace, Action<string, bool, JObject, string> check)
        {
            var target = fixture.Hostile; var disease = InfernalSavedBlueprint34<BlueprintBuff>(false, "DevilChills");
            foreach (var buff in target.Buffs.Enumerable.Where(value => value.Blueprint == disease && ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray()) buff.Remove();
            Kingmaker.UnitLogic.Buffs.Buff infected = null;
            for (int attempt = 0; attempt < 8 && infected == null; attempt++) {
                InfernalCombatLane34(fixture, owner, target, 3); int first = trace.Rows.Count;
                var command = new UnitAttack(target) { ForceFullAttack = true }; owner.Commands.Run(command);
                foreach (int frame in WaitInfernalCommand34(command, () => true, 25)) yield return frame;
                var attacks = trace.Rows.Skip(first).Where(row => (string)row["event"] == "attack" && (string)row["owner"] == owner.UniqueId).ToArray();
                var beard = trace.Rows.Skip(first).Where(row => (string)row["event"] == "damage" && (string)row["owner"] == owner.UniqueId &&
                    !(bool)row["hasAttackRoll"] && ((JArray)row["packets"]).Any(packet => (string)packet["dice"] == "1d8")).ToArray();
                bool both = attacks.Length == 2 && attacks.All(row => (bool)row["hit"]);
                infected = target.Buffs.Enumerable.SingleOrDefault(value => value.Blueprint == disease && ReferenceEquals(value.Context?.MaybeCaster, owner));
                check("beard-injury-attempt-" + attempt, command.Result == UnitCommand.ResultType.Success && attacks.Length == 2 && beard.Length == (both ? 1 : 0),
                    new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["command"] = Sprint27AiCommand(command),
                        ["attacks"] = new JArray(attacks), ["beard"] = new JArray(beard), ["events"] = new JArray(trace.Rows.Skip(first)),
                        ["infected"] = infected != null }, "Only both claws of this real full command qualify Beard; actual damage and natural injury saves remain visible.");
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
            }
            if (infected == null) throw new InvalidOperationException("Devil Chills injury precondition unobserved in eight native full attacks.");
            var clock = infected.SelectComponents<DevilChills34>().Single(); var exposure = clock.NextExposure;
            double days = (exposure - Game.Instance.TimeController.GameTime).TotalDays; int firstStat = trace.Rows.Count;
            double until = Game.Instance.TimeController.GameTime.TotalSeconds + 6.1; DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < deadline && Game.Instance.TimeController.GameTime.TotalSeconds < until) { Game.Instance.IsPaused = false; yield return 0; }
            check("devil-chills-day-onset", days > .99 && days <= 4 && clock.NextExposure == exposure && clock.ConsecutiveSaves == 0 &&
                !trace.Rows.Skip(firstStat).Any(row => (string)row["event"] == "stat-damage" && (string)row["target"] == target.UniqueId),
                new JObject { ["effect"] = InfernalSavedEffect34(infected), ["onsetDaysRemaining"] = days, ["elapsedGameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds - until + 6.1 },
                "Actual injury disease waits its printed day-scale onset and never substitutes round-scale Strength damage.");
            var healer = CastExpandedSummoningOwnTier(fixture, "astral-deva");
            try {
                Sprint28SetBrainActivePreservingActions(healer, false); healer.Commands.InterruptAll(true);
                foreach (int frame in WaitInfernalSupport34(healer)) yield return frame;
                var spell = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix + "RemoveDisease"];
                PlaceExpandedSummoningUnit(healer, target.Position + Vector3.right * (healer.Corpulence + target.Corpulence + .3f));
                var attempts = new JArray();
                for (int attempt = 0; attempt < 8 && !infected.IsDisposed; attempt++) {
                    UnitUseAbility command = null; int first = trace.Rows.Count;
                    foreach (int frame in Sprint26Cast(healer, spell, new TargetWrapper(target), value => command = value)) yield return frame;
                    attempts.Add(new JObject { ["command"] = Sprint27AiCommand(command), ["events"] = new JArray(trace.Rows.Skip(first)), ["disposed"] = infected.IsDisposed });
                }
                check("devil-chills-native-cure", infected.IsDisposed && !target.Buffs.Enumerable.Contains(infected),
                    new JObject { ["healer"] = healer.UniqueId, ["target"] = target.UniqueId, ["ability"] = spell.AssetGuid, ["attempts"] = attempts },
                    "Real Remove Disease commands use the native disease cure consumer; the observer does not remove the accepted infection.");
            } finally { DisposeExpandedSummoningUnits(fixture.Created, new[] { healer }); }
        }
    }
}
