using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static JArray NegativeLevels33(UnitEntityData unit)
        {
            var result = new JArray(); var part = unit.Get<UnitPartNegativeLevels>();
            if (part == null) return result;
            var entries = (IEnumerable)typeof(UnitPartNegativeLevels).GetField("m_LevelsData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(part);
            foreach (object entry in entries)
            {
                var type = entry.GetType();
                var context = (MechanicsContext)type.GetField("ParentContext").GetValue(entry);
                var end = (TimeSpan?)type.GetField("EndTime").GetValue(entry);
                result.Add(new JObject { ["type"] = type.GetField("Type").GetValue(entry).ToString(),
                    ["count"] = (int)type.GetField("Count").GetValue(entry), ["end"] = end?.TotalSeconds,
                    ["caster"] = context?.MaybeCaster?.UniqueId });
            }
            return result;
        }

        // Fixed cells reuse normal native commands and the existing disposable clock/turn admission.
        // Only recipient AC in the miss cell and a real native Death Ward are fixture preconditions.
        private IEnumerable<int> ReviewSuccubusDrainProof33(ExpandedSummoningCorrectionFixture fixture)
        {
            var target = fixture.Hostile; var owners = new List<UnitEntityData>();
            int originalAc = target.Stats.AC.BaseValue;
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            var wardBlueprint = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintBuff>()
                .Single(b => b.name == "DeathWardBuff");
            Buff ward = null;
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    var unit = CastExpandedSummoningOwnTier(fixture, "succubus"); owners.Add(unit);
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                for (int frame = 0; frame < 20; frame++) yield return 0;
                var actors = owners.Concat(new[] { target, fixture.Caster }).ToArray();
                if (actors.Any(u => (u.Get<UnitPartNegativeLevels>()?.Count ?? 0) != 0)) throw new InvalidOperationException("Nonzero drain intake.");
                foreach (string cell in new[] { "first", "sibling", "later", "miss", "immune", "source-expiry", "turn" })
                {
                    UnitEntityData owner = cell == "sibling" || cell == "turn" ? owners[1] : owners[0];
                    bool turn = cell == "turn", miss = cell == "miss", immune = cell == "immune";
                    var starts = new Lillend32CommandStarts { Owner = owner, ObserveRtwpAttackCompletion = !turn };
                    var attacks = new Sprint16RuleObserver { Owner = owner, Target = target };
                    var drain = new Sprint33Drain { Owner = owner, Target = target, Actors = actors };
                    SuccubusDrainTrace33 trace = null;
                    try
                    {
                        target.Stats.AC.BaseValue = miss ? 1000 : originalAc;
                        if (immune)
                        {
                            ward = target.Buffs.AddBuff(wardBlueprint, new MechanicsContext(fixture.Caster, target.Descriptor,
                                wardBlueprint, null, new TargetWrapper(target)), TimeSpan.FromMinutes(2));
                            if (ward == null || !ward.Active) throw new InvalidOperationException("Native Death Ward absent.");
                        }
                        Vector3 end; string lane, direction;
                        float gap = owner.Corpulence + target.Corpulence + owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Meters * .5f;
                        var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, gap, out end, out lane, out direction);
                        PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, end);
                        owner.Wake(0f); target.Wake(0f); owner.Memory.Add(target); target.Memory.Add(owner);
                        owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                        var admission = new JObject();
                        if (turn)
                        {
                            foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                            DateTime wait = DateTime.UtcNow.AddSeconds(30);
                            while (DateTime.UtcNow < wait && !Sprint18OwnerIsActing(owner)) yield return 0;
                            if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner)) throw new InvalidOperationException("Drain native turn not admitted.");
                        }
                        var trait = owner.Buffs.Enumerable.Single(b => b.Blueprint.AssetGuid == "cd51ad31f8764d2797b59eb43da7a9f8");
                        var trigger = trait.SelectComponents<AddInitiatorAttackWithWeaponTrigger>().Single();
                        trace = new SuccubusDrainTrace33(owner, trait, trigger, trigger.Action.Actions.OfType<ContextActionDealDamage>().Single(), actors);
                        EventBus.Subscribe(starts); EventBus.Subscribe(attacks); EventBus.Subscribe(drain);
                        var costs = new JArray();
                        using (var economy = new FirearmReviewCosts(owner, costs))
                        {
                            var attack = new UnitAttack(target) { ForceFullAttack = true }; attack.Init(owner);
                            if (!attack.CanStart) throw new InvalidOperationException("Drain ordinary attack not available.");
                            owner.Commands.Run(attack);
                            DateTime deadline = DateTime.UtcNow.AddSeconds(35);
                            var samples = new JArray(); int previous = -1;
                            while (DateTime.UtcNow < deadline && !attack.IsFinished)
                            {
                                int count = target.Get<UnitPartNegativeLevels>()?.Count ?? 0;
                                if (count != previous) { samples.Add(new JObject { ["time"] = Game.Instance.TimeController.GameTime.TotalSeconds,
                                    ["count"] = count, ["entries"] = NegativeLevels33(target) }); previous = count; }
                                Game.Instance.IsPaused = false; yield return 0;
                            }
                            var resolved = drain.Rows.Where(r => (string)r["stage"] == "after").ToArray();
                            bool hit = attacks.Attacks.Any(r => r.IsHit);
                            bool completed = starts.CompletedAttack(attack, target);
                            bool exactCost = economy.Count(attack) == 1 && economy.Exact(attack, turn);
                            bool effect = miss ? !hit && resolved.Length == 0 : immune ? hit && resolved.Length == 1 && (bool)resolved[0]["immune"] :
                                hit && resolved.Length == 1 && !(bool)resolved[0]["immune"] && (bool)resolved[0]["sourceMatchesOwner"] &&
                                (string)resolved[0]["recipient"] == target.UniqueId && (int)resolved[0]["count"] == 1 &&
                                (string)resolved[0]["type"] == "Temporary" && (double)resolved[0]["durationSeconds"] == 6 &&
                                (int)resolved[0]["nativeLevels"] == 1;
                            var acquired = resolved.FirstOrDefault(r => !(bool)r["immune"]);
                            var stored = acquired?["storedLevels"] as JArray;
                            bool lifetime = miss || immune || stored != null && stored.Count == 1 &&
                                (string)stored[0]["type"] == "Temporary" && (string)stored[0]["caster"] == owner.UniqueId &&
                                Math.Abs((double)stored[0]["end"] - (double)acquired["nativeGameTime"] - 6) < .01;
                            if (cell == "source-expiry" && effect)
                            {
                                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                                if (owner.IsInCombat) owner.LeaveCombat();
                                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                            }
                            // Turn proof covers attack dispatch; expiry uses the already qualified RTWP clock.
                            if (turn) { SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; Game.Instance.TurnBasedCombatController.Activate(); }
                            double expectedEnd = stored != null && stored.Count == 1 ? (double)stored[0]["end"] : Game.Instance.TimeController.GameTime.TotalSeconds;
                            DateTime expiryWait = DateTime.UtcNow.AddSeconds(18);
                            while (DateTime.UtcNow < expiryWait && (Game.Instance.TimeController.GameTime.TotalSeconds < expectedEnd + .2 || (target.Get<UnitPartNegativeLevels>()?.Count ?? 0) != 0))
                            { Game.Instance.IsPaused = false; yield return 0; }
                            samples.Add(new JObject { ["time"] = Game.Instance.TimeController.GameTime.TotalSeconds,
                                ["count"] = target.Get<UnitPartNegativeLevels>()?.Count ?? 0, ["entries"] = NegativeLevels33(target) });
                            bool expired = (target.Get<UnitPartNegativeLevels>()?.Count ?? 0) == 0 && Game.Instance.TimeController.GameTime.TotalSeconds >= expectedEnd;
                            bool isolated = actors.Where(a => !ReferenceEquals(a, target)).All(a => (a.Get<UnitPartNegativeLevels>()?.Count ?? 0) == 0) &&
                                resolved.All(r => (string)r["recipient"] == target.UniqueId && (bool)r["sourceMatchesOwner"]);
                            trace.Dispose();
                            bool pass = completed && exactCost && effect && lifetime && expired && isolated && (cell != "source-expiry" || owner.Destroyed) && trace.Evidence["error"] == null && (bool)trace.Evidence["hooksRemoved"];
                            Sprint25Check("sprint33-drain-" + cell, pass, new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                                ["nativeTurn"] = turn, ["admission"] = admission, ["completed"] = completed, ["costExact"] = exactCost,
                                ["effect"] = effect, ["storedLifetime"] = lifetime, ["expired"] = expired, ["isolated"] = isolated,
                                ["sourceDisposed"] = owner.Destroyed, ["causalTrace"] = trace.Evidence, ["drain"] = drain.Rows,
                                ["samples"] = samples, ["nativeAttackLifecycle"] = starts.AttackRows, ["costs"] = costs,
                                ["fixtureAC"] = target.Stats.AC.BaseValue, ["ward"] = ward?.Blueprint.AssetGuid, ["forcedRolls"] = 0 },
                                "Native first-hit trigger acquires one six-second temporary level, rejects subsequent limbs/miss/immunity, and expires without wrong-owner leakage. RNG prerequisite failures remain failures, not forced successes.");
                            if (!pass) throw new InvalidOperationException("Drain proof failed: " + cell);
                        }
                    }
                    finally
                    {
                        trace?.Dispose(); EventBus.Unsubscribe(starts); EventBus.Unsubscribe(attacks); EventBus.Unsubscribe(drain);
                        if (ward != null && !ward.IsDisposed) ward.Remove(); ward = null;
                        target.Stats.AC.BaseValue = originalAc;
                        if (!owner.Destroyed) { owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue(); if (owner.IsInCombat) owner.LeaveCombat(); }
                        if (target.IsInCombat) target.LeaveCombat(); Game.Instance.Player.UpdateIsInCombat();
                    }
                }
            }
            finally
            {
                if (ward != null && !ward.IsDisposed) ward.Remove();
                target.Stats.AC.BaseValue = originalAc;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; Game.Instance.TurnBasedCombatController.Activate();
                foreach (var owner in owners.Where(u => !u.Destroyed)) { owner.Commands.InterruptAll(true); if (owner.IsInCombat) owner.LeaveCombat(); }
                DisposeExpandedSummoningUnits(fixture.Created, owners.Where(u => !u.Destroyed).ToArray());
                if (target.IsInCombat) target.LeaveCombat(); Game.Instance.Player.UpdateIsInCombat();
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; Game.Instance.TurnBasedCombatController.Activate();
            }
        }
    }
}
