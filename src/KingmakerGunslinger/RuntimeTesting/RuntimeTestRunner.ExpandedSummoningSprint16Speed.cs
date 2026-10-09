using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static JObject Sprint16Speed(UnitEntityData unit)
        {
            ModifiableValue speed = unit.Descriptor.Stats.Speed;
            return new JObject {
                ["baseFeet"] = speed.BaseValue, ["modifiedFeet"] = speed.ModifiedValue,
                ["combatMps"] = unit.CombatSpeedMps, ["currentMps"] = unit.CurrentSpeedMps,
                ["slowed"] = unit.Descriptor.State.HasCondition(UnitCondition.Slowed),
                ["modifiers"] = new JArray(speed.Modifiers.Select(value => new JObject {
                    ["value"] = value.ModValue, ["descriptor"] = value.ModDescriptor.ToString(),
                    ["source"] = value.Source == null ? null : value.Source.ToString() }))
            };
        }

        private static BlueprintAbility Sprint16Sprint(BlueprintScriptableObject[] blueprints, string key)
        {
            return blueprints.OfType<BlueprintAbility>().Single(value => value.name ==
                "KMG_Summoning_Special_" + (key == "crocodile" ? "Crocodile" : "DireCrocodile") + "_Sprint");
        }

        private static BlueprintBuff Sprint16SprintBuff(BlueprintScriptableObject[] blueprints,
            string key, bool cooldown)
        {
            return blueprints.OfType<BlueprintBuff>().Single(value => value.name ==
                "KMG_Summoning_Special_" + (key == "crocodile" ? "Crocodile" : "DireCrocodile") +
                (cooldown ? "_SprintCooldown" : "_SprintState"));
        }

        private static void ClearSprint16Sprint(UnitEntityData owner,
            BlueprintBuff state, BlueprintBuff cooldown)
        {
            Buff active = owner.Descriptor.Buffs.GetBuff(state);
            if (active != null) active.Remove();
            Buff spent = owner.Descriptor.Buffs.GetBuff(cooldown);
            if (spent != null) spent.Remove();
            InterruptExpandedSummoningFixtureCommands(owner);
        }

        private void ExerciseSprint16Speed(ExpandedSummoningCorrectionFixture fixture,
            List<RuntimeTestAssertion> assertions)
        {
            var rows = new JArray();
            TimeSpan globalTime = Game.Instance.TimeController.GameTime;
            try
            {
                foreach (string key in new[] { "crocodile", "dire-crocodile" })
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(fixture, key);
                    SetExpandedSummoningBrainActive(owner, false);
                    try
                    {
                        ExerciseSprint16SpeedInteractions(fixture, owner, key, assertions, rows);
                        ExerciseSprint16SprintTimeline(fixture, owner, key, assertions, rows);
                    }
                    finally { DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }); }
                }
            }
            catch (Exception exception)
            {
                Sprint16Check(assertions, rows, "speed-exception", false,
                    new JObject { ["exception"] = DescribeExpandedSummoningCorrectionException(exception) },
                    "all live speed and cooldown cases complete without exception");
            }
            finally
            {
                Sprint16Check(assertions, rows, "speed-global-clock-unchanged",
                    Game.Instance.TimeController.GameTime == globalTime,
                    new JObject { ["before"] = globalTime.ToString(),
                        ["after"] = Game.Instance.TimeController.GameTime.ToString() },
                    "only disposable buff deadlines are advanced; no campaign clock mutation");
                File.WriteAllText(Path.Combine(_request.EvidenceDirectory,
                    "sprint16-speed.json"), rows.ToString(Formatting.Indented));
            }
        }

        private static void ExerciseSprint16SpeedInteractions(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, string key,
            List<RuntimeTestAssertion> assertions, JArray rows)
        {
            BlueprintAbility sprint = Sprint16Sprint(fixture.Blueprints, key);
            BlueprintBuff state = Sprint16SprintBuff(fixture.Blueprints, key, false);
            BlueprintBuff cooldown = Sprint16SprintBuff(fixture.Blueprints, key, true);
            int baseSpeed = owner.Descriptor.Stats.Speed.BaseValue;
            BlueprintBuff haste = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "HasteBuff");
            BlueprintBuff slow = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "SlowBuff");
            foreach (string condition in new[] { "plain", "haste-first", "sprint-first-haste",
                "slow-first", "speed-penalty", "native-minimum", "command-speed-cap" })
            {
                Buff extra = null;
                ModifiableValue.Modifier penalty = null;
                UnitMoveTo move = null;
                try
                {
                    ClearSprint16Sprint(owner, state, cooldown);
                    if (condition == "haste-first" || condition == "slow-first")
                    {
                        BlueprintBuff blueprint = condition == "haste-first" ? haste : slow;
                        extra = owner.Descriptor.Buffs.AddBuff(blueprint,
                            new MechanicsContext(owner, owner.Descriptor, blueprint, null,
                                new TargetWrapper(owner)), TimeSpan.FromMinutes(1));
                        if (extra == null) throw new InvalidOperationException("Native speed control did not apply.");
                    }
                    if (condition == "speed-penalty" || condition == "native-minimum")
                        penalty = owner.Descriptor.Stats.Speed.AddModifier(
                            condition == "speed-penalty" ? -5 : -100,
                            null, "KMG_Sprint16_Disposable_SpeedPenalty", ModifierDescriptor.UntypedStackable);
                    JObject before = Sprint16Speed(owner);
                    int beforeFeet = owner.Descriptor.Stats.Speed.ModifiedValue;
                    ExecuteExpandedSummoningRuntimeAbility(owner, sprint, 0, new TargetWrapper(owner), false);
                    string command = _expandedSummoningLastAbilityExecution;
                    if (condition == "sprint-first-haste")
                        extra = owner.Descriptor.Buffs.AddBuff(haste,
                            new MechanicsContext(owner, owner.Descriptor, haste, null,
                                new TargetWrapper(owner)), TimeSpan.FromMinutes(1));
                    if (condition == "command-speed-cap")
                    {
                        // Native out-of-combat command speed limit, not a new
                        // movement subsystem or a mutation of the base stat.
                        move = new UnitMoveTo(owner.Position + Vector3.forward * 3f) { SpeedLimit = 1f };
                        owner.Commands.Run(move);
                    }
                    JObject active = Sprint16Speed(owner);
                    int activeFeet = owner.Descriptor.Stats.Speed.ModifiedValue;
                    bool available = new AbilityData(owner.Descriptor.Abilities.GetAbility(sprint)).IsAvailable;
                    Buff activeFact = owner.Descriptor.Buffs.GetBuff(state);
                    bool ownState = activeFact != null && ReferenceEquals(activeFact.MaybeContext.MaybeCaster, owner);
                    if (activeFact != null) activeFact.Remove();
                    JObject restored = Sprint16Speed(owner);
                    int afterFeet = owner.Descriptor.Stats.Speed.ModifiedValue;
                    bool exact = ownState && !available && owner.Descriptor.HasFact(cooldown) &&
                        owner.Descriptor.Stats.Speed.BaseValue == baseSpeed &&
                        activeFeet == (condition == "native-minimum" ? 5 :
                            condition == "sprint-first-haste" ? 60 : beforeFeet + 20) &&
                        afterFeet == (condition == "sprint-first-haste" ? 40 : beforeFeet);
                    if (condition == "plain") exact = exact && beforeFeet == 20 && activeFeet == 40 && afterFeet == 20;
                    if (condition == "haste-first") exact = exact && beforeFeet == 40 && activeFeet == 60;
                    if (condition == "slow-first") exact = exact &&
                        (bool)active["slowed"] &&
                        (double)active["combatMps"] > (double)before["combatMps"];
                    if (condition == "command-speed-cap") exact = exact && !owner.IsInCombat &&
                        Math.Abs((double)active["currentMps"] - 1d) < 0.0001 &&
                        Math.Abs((double)restored["currentMps"] - 1d) < 0.0001;
                    Sprint16Check(assertions, rows, key + "-sprint-" + condition, exact,
                        new JObject { ["before"] = before, ["active"] = active,
                            ["afterStateRemoval"] = restored, ["nativeCommand"] = command,
                            ["abilityAvailableDuringCooldown"] = available,
                            ["sourceExact"] = ownState },
                        "bounded +20 ft stacks with Haste; native penalties/floors/command caps remain; no base mutation");
                }
                finally
                {
                    if (move != null && !move.IsFinished) move.Interrupt();
                    if (extra != null) extra.Remove();
                    if (penalty != null) owner.Descriptor.Stats.Speed.RemoveModifier(penalty);
                    ClearSprint16Sprint(owner, state, cooldown);
                }
            }
            Sprint16Check(assertions, rows, key + "-sprint-interaction-cleanup",
                owner.Descriptor.Stats.Speed.BaseValue == baseSpeed &&
                    owner.Descriptor.Stats.Speed.ModifiedValue == 20 &&
                    !owner.Descriptor.HasFact(state) && !owner.Descriptor.HasFact(cooldown),
                Sprint16Speed(owner), "all temporary speed facts, modifiers and commands removed");
        }

        private static void ExerciseSprint16SprintTimeline(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, string key,
            List<RuntimeTestAssertion> assertions, JArray rows)
        {
            BlueprintAbility sprint = Sprint16Sprint(fixture.Blueprints, key);
            BlueprintBuff state = Sprint16SprintBuff(fixture.Blueprints, key, false);
            BlueprintBuff cooldown = Sprint16SprintBuff(fixture.Blueprints, key, true);
            ExecuteExpandedSummoningRuntimeAbility(owner, sprint, 0, new TargetWrapper(owner), false);
            Buff active = owner.Descriptor.Buffs.GetBuff(state);
            Buff spent = owner.Descriptor.Buffs.GetBuff(cooldown);
            TimeSpan now = Game.Instance.TimeController.GameTime;
            if (active == null || spent == null)
                throw new InvalidOperationException("The real Sprint cast did not install both states.");
            TimeSpan stateEnd = active.EndTime;
            TimeSpan cooldownEnd = spent.EndTime;
            PropertyInfo endTime = typeof(Buff).GetProperty("EndTime",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (endTime == null || !endTime.CanWrite)
                throw new MissingMemberException("Native Buff.EndTime setter is unavailable.");
            var timeline = new JArray();
            bool exact = Math.Abs((stateEnd - now).TotalSeconds - 6d) < 0.01 &&
                Math.Abs((cooldownEnd - now).TotalSeconds - 60d) < 0.01 &&
                !new AbilityData(owner.Descriptor.Abilities.GetAbility(sprint)).IsAvailable;
            // Only these two disposable deadlines move. BuffCollection.Tick
            // decides expiry normally; no campaign clock or global scheduler.
            foreach (double elapsed in new[] { 0d, 5.999d, 6.001d, 12.001d, 18.001d,
                24.001d, 30.001d, 36.001d, 42.001d, 48.001d, 54.001d, 59.999d, 60.001d })
            {
                if (owner.Descriptor.HasFact(state))
                    endTime.SetValue(active, stateEnd - TimeSpan.FromSeconds(elapsed), null);
                if (owner.Descriptor.HasFact(cooldown))
                    endTime.SetValue(spent, cooldownEnd - TimeSpan.FromSeconds(elapsed), null);
                owner.Descriptor.Buffs.UpdateNextEvent();
                owner.Descriptor.Buffs.Tick();
                bool running = owner.Descriptor.HasFact(state);
                bool waiting = owner.Descriptor.HasFact(cooldown);
                bool ready = new AbilityData(owner.Descriptor.Abilities.GetAbility(sprint)).IsAvailable;
                int speed = owner.Descriptor.Stats.Speed.ModifiedValue;
                exact = exact && running == (elapsed < 6d) && waiting == (elapsed < 60d) &&
                    ready == (elapsed >= 60d) && speed == (elapsed < 6d ? 40 : 20);
                timeline.Add(new JObject { ["elapsedSeconds"] = elapsed,
                    ["active"] = running, ["cooldown"] = waiting, ["available"] = ready,
                    ["speedFeet"] = speed });
            }
            ExecuteExpandedSummoningRuntimeAbility(owner, sprint, 0, new TargetWrapper(owner), false);
            bool recast = owner.Descriptor.HasFact(state) && owner.Descriptor.HasFact(cooldown) &&
                owner.Descriptor.Stats.Speed.ModifiedValue == 40;
            Sprint16Check(assertions, rows, key + "-sprint-timeline", exact && recast,
                new JObject { ["stateSeconds"] = (stateEnd - now).TotalSeconds,
                    ["cooldownSeconds"] = (cooldownEnd - now).TotalSeconds,
                    ["timeline"] = timeline, ["tenthRoundRecast"] = recast },
                "one round active; blocked through nine more rounds; available at 60 s; real recast succeeds");
            ClearSprint16Sprint(owner, state, cooldown);
        }
    }
}
