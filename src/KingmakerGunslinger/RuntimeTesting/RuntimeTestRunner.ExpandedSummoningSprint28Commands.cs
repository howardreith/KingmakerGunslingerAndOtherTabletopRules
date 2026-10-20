using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // A fresh disposable giant owns each command. Native hand switches,
        // attack commands, rule events and action costs are all observed; the
        // fixture supplies geometry and a durable hostile, never an attack.
        private IEnumerable<int> ReviewSprint28Commands(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (var spec in new[] {
                    new { Key="hill-giant", WeaponAttacks=2, RockAttacks=1 },
                    new { Key="stone-giant", WeaponAttacks=2, RockAttacks=2 },
                    new { Key="fire-giant", WeaponAttacks=3, RockAttacks=1 }
                })
                foreach (var attackCase in new[] {
                    new { Name="weapon-single", Set=0, Full=false },
                    new { Name="weapon-full", Set=0, Full=true },
                    new { Name="slam-full", Set=1, Full=true },
                    new { Name="rock-single", Set=2, Full=false },
                    new { Name="rock-full", Set=2, Full=true }
                }.Where(value => (value.Name != "rock-single" ||
                    spec.Key == "stone-giant") &&
                    (!turnBased || value.Full ||
                    spec.Key == "stone-giant" && value.Name == "rock-single")))
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, spec.Key);
                    SetExpandedSummoningBrainActive(owner, false);
                    try
                    {
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        foreach (int frame in Sprint22SetPowerAttack(owner, false))
                            yield return frame;
                        if (attackCase.Set != 0)
                        {
                            var switchCommand = new UnitSwitchHandEquipmentSet(
                                attackCase.Set);
                            owner.Commands.Run(switchCommand);
                            int switchFrames = 0;
                            DateTime switchDeadline = DateTime.UtcNow.AddSeconds(20);
                            while (!switchCommand.IsFinished &&
                                switchFrames++ < 1000 &&
                                DateTime.UtcNow < switchDeadline) yield return 0;
                            Sprint25Check("s28-"+spec.Key+"-"+
                                attackCase.Name+"-switch",
                                switchCommand.IsFinished &&
                                owner.Body.CurrentHandEquipmentSetIndex ==
                                    attackCase.Set,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["finished"] = switchCommand.IsFinished,
                                    ["result"] = switchCommand.Result.ToString(),
                                    ["frames"] = switchFrames,
                                    ["active"] = owner.Body.CurrentHandEquipmentSetIndex },
                                "Native hand-set command selects the exact owned weapon before combat.");
                            if (!switchCommand.IsFinished ||
                                owner.Body.CurrentHandEquipmentSetIndex !=
                                    attackCase.Set)
                                throw new InvalidOperationException(
                                    "Giant command hand switch did not finish.");
                        }
                        UnitEntityData target = fixture.Hostile;
                        ResetExpandedSummoningHostile(fixture);
                        Vector3 far; string lane, direction;
                        Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                            owner, target, 8f, out far, out lane,
                            out direction);
                        Vector3 forward = (far-origin).normalized;
                        PlaceExpandedSummoningUnit(owner, origin);
                        float separation = owner.Corpulence +
                            target.Corpulence + .8f;
                        PlaceExpandedSummoningUnit(target,
                            attackCase.Set == 2 ? far :
                                origin + forward * separation);
                        owner.SightCache.ClearIfNeeded();
                        target.SightCache.ClearIfNeeded();
                        var probe = new UnitAttack(target);
                        probe.Init(owner);
                        bool legal = owner.HasLOS(target) &&
                            probe.IsUnitEnoughClose && owner.IsEnemy(target) &&
                            !target.Descriptor.State.IsDead;
                        Sprint25Check("s28-"+spec.Key+"-"+
                            attackCase.Name+"-lane", legal,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["lane"] = lane,
                                ["direction"] = direction,
                                ["distance"] = owner.DistanceTo(target),
                                ["los"] = owner.HasLOS(target),
                                ["inRange"] = probe.IsUnitEnoughClose,
                                ["enemy"] = owner.IsEnemy(target) },
                            "A visible native lane and the exact hostile are real command prerequisites.");
                        if (!legal) throw new InvalidOperationException(
                            "Giant command has no legal visible lane.");
                        owner.JoinCombat(); target.JoinCombat();
                        owner.Memory.Add(target); target.Memory.Add(owner);
                        JObject admission = null;
                        if (turnBased)
                        {
                            admission = new JObject();
                            foreach (int frame in Sprint22EnrollTurnBasedOwners(
                                new[] { owner }, target, admission)) yield return frame;
                            for (int frame = 0; frame < 500 &&
                                !Sprint18OwnerIsActing(owner); frame++) yield return 0;
                            bool acting = (bool)admission["admitted"] &&
                                Sprint18OwnerIsActing(owner);
                            Sprint25Check("s28-" + spec.Key + "-" +
                                attackCase.Name + "-turn-admission", acting,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["target"] = target.UniqueId,
                                    ["admission"] = admission,
                                    ["turn"] = Sprint22TurnState(owner, target) },
                                "The exact summoned giant receives a native turn before its own attack command.");
                            if (!acting) throw new InvalidOperationException(
                                "Giant native turn admission failed.");
                        }
                        int targetDamageBefore = target.Descriptor.Damage;
                        ItemEntityWeapon weapon = owner.Body.PrimaryHand.MaybeWeapon;
                        int expected = !attackCase.Full ? 1 :
                            attackCase.Set == 0 ? spec.WeaponAttacks :
                            attackCase.Set == 1 ? 2 : spec.RockAttacks;
                        var observer = new Sprint24AttackObserver {
                            Owner = owner, Target = target };
                        var costs = new JArray();
                        var endings = new JArray();
                        var command = new UnitAttack(target) {
                            ForceFullAttack = attackCase.Full,
                            IsSingleAttack = !attackCase.Full };
                        EventBus.Subscribe(observer);
                        try
                        {
                            using (var economy = new FirearmReviewCosts(owner,
                                costs))
                            using (var trace = new Sprint26CommandTrace(owner,
                                endings))
                            {
                                owner.Commands.Run(command);
                                int frames = 0;
                                DateTime deadline = DateTime.UtcNow.AddSeconds(35);
                                while (frames++ < 3500 &&
                                    DateTime.UtcNow < deadline &&
                                    (!command.IsFinished ||
                                     observer.WeaponAttacks.Count < expected))
                                    yield return 0;
                                int hits = observer.WeaponAttacks.Count(value =>
                                    value.AttackRoll != null &&
                                    value.AttackRoll.IsHit);
                                int impactFrames = 0;
                                DateTime impactDeadline = DateTime.UtcNow.AddSeconds(10);
                                while (attackCase.Set == 2 &&
                                    observer.Damage.Count < hits &&
                                    impactFrames++ < 600 &&
                                    DateTime.UtcNow < impactDeadline)
                                    yield return 0;
                                for (int frame = 0; frame < 12; frame++)
                                    yield return 0;
                                bool exact = command.IsFinished &&
                                    (command.Result == UnitCommand.ResultType.Success ||
                                     trace.Exhausted(command)) &&
                                    command.AllAttacks.Count == expected &&
                                    observer.OwnerWeaponAttacks.Count == expected &&
                                    observer.WeaponAttacks.Count == expected &&
                                    observer.Rolls.Count == expected &&
                                    observer.WeaponAttacks.All(value =>
                                        ReferenceEquals(value.Weapon.Blueprint,
                                            weapon.Blueprint)) &&
                                    observer.Damage.Count == hits &&
                                    economy.Count(command) == 1 &&
                                    economy.Exact(command, turnBased) &&
                                    !command.Cutscene &&
                                    !command.IsIgnoreCooldown &&
                                    !target.Descriptor.State.IsDead;
                                Sprint25Check("s28-"+spec.Key+"-"+
                                    attackCase.Name+"-resolved", exact,
                                    new JObject { ["owner"] = owner.UniqueId,
                                        ["target"] = target.UniqueId,
                                        ["turnBased"] = turnBased,
                                        ["admission"] = admission,
                                        ["weapon"] = weapon.Blueprint.AssetGuid,
                                        ["finished"] = command.IsFinished,
                                        ["result"] = command.Result.ToString(),
                                        ["frames"] = frames,
                                        ["planned"] = command.AllAttacks.Count,
                                        ["expected"] = expected,
                                        ["ownerAttacks"] = observer.OwnerWeaponAttacks.Count,
                                        ["targetAttacks"] = observer.WeaponAttacks.Count,
                                        ["rolls"] = new JArray(observer.WeaponAttacks.Select(
                                            value => new JObject {
                                                ["weapon"] = value.Weapon?.Blueprint?.AssetGuid,
                                                ["hit"] = value.AttackRoll?.IsHit,
                                                ["natural"] = value.AttackRoll?.Roll.Value,
                                                ["attackBonus"] = value.AttackRoll?.AttackBonus,
                                                ["autoMiss"] = value.AttackRoll?.AutoMiss,
                                                ["nativeDamageRule"] = value.MeleeDamage != null })),
                                        ["damage"] = new JArray(observer.Damage.Select(
                                            Sprint16DamageEvent)),
                                        ["hits"] = hits,
                                        ["damagePackets"] = observer.Damage.Count,
                                        ["impactWaitFrames"] = impactFrames,
                                        ["targetDamageBefore"] = targetDamageBefore,
                                        ["targetDamageAfter"] = target.Descriptor.Damage,
                                        ["costs"] = costs,
                                        ["nativeEnds"] = endings,
                                        ["forcedRolls"] = 0,
                                        ["fixtureAttackCommands"] = 0 },
                                    "One real ordinary or full attack spends native action cost and resolves only its selected weapon packets against the visible hostile.");
                            }
                        }
                        finally { EventBus.Unsubscribe(observer); }
                    }
                    finally
                    {
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                        Game.Instance.TurnBasedCombatController.Activate();
                        owner.Commands.InterruptAll(true);
                        if (owner.IsInCombat) owner.LeaveCombat();
                        ResetExpandedSummoningHostile(fixture);
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
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
    }
}
