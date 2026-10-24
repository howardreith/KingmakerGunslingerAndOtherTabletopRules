using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewDretch30Melee(ExpandedSummoningCorrectionFixture fixture, bool ai)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (int frame in ReviewDretch30MeleeCommands(fixture, ai)) yield return frame;
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> ReviewDretch30MeleeCommands(ExpandedSummoningCorrectionFixture fixture, bool ai)
        {
            ResetExpandedSummoningHostile(fixture);
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            var inventoryBefore = Game.Instance.Player.Inventory.Items.ToArray();
            var inventoryCounts = inventoryBefore.Select(value => value.Count).ToArray();
            UnityEngine.Object[] bodyResources = null;
            Kingmaker.Items.ItemEntity[] items = null;
            var owner = CastExpandedSummoningOwnTier(fixture, "dretch");
            const string prefix = "KMG.Summoning.OutsiderCelestial.Dretch.";
            var claw = (Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "Claw1d4"];
            var bite = (Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "Bite1d4"];
            var fear = (BlueprintAbilityResource)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "CauseFearResource"];
            var cloud = (BlueprintAbilityResource)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "StinkingCloudResource"];
            int originalStrength = owner.Stats.Strength.BaseValue;
            int[] resourcesBefore = { owner.Descriptor.Resources.GetResourceAmount(fear), owner.Descriptor.Resources.GetResourceAmount(cloud) };
            if (resourcesBefore.Any(value => value != 1)) throw new InvalidOperationException("Fresh Dretch resources differ.");
            // Explicit exhaustion fixture only: finite native spell casting is
            // covered by dretch-cloud/AI. These debits do not count as casts.
            if (ai) { owner.Descriptor.Resources.Spend(fear, 1); owner.Descriptor.Resources.Spend(cloud, 1); }
            var hitWeapons = new HashSet<string>();
            bool full = !ai;
            var target = fixture.Hostile;
            Sprint28SetBrainActivePreservingActions(owner, false);
            string id = ai ? "ai-fallback" : "full";
            int targetAc = target.Descriptor.Stats.AC.BaseValue;
            try
            {
                // Match the final successful native command fixture: allow
                // fresh spawn/view state to settle before checking readiness.
                for (int frame = 0; frame < 8; frame++) yield return 0;
                owner.Commands.InterruptAll(true); target.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue(); target.Commands.RemoveFinishedAndUpdateQueue();
                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 5f, out far, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, ai ? far : origin + (far - origin).normalized *
                    (owner.Corpulence + target.Corpulence + .8f));
                // Only the owned target's AC is lowered; rolls and hit decisions remain native.
                target.Descriptor.Stats.AC.BaseValue = -100;
                foreach (var unit in new[] { owner, target })
                {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                if (ai) { Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions(); }
                var probe = new UnitAttack(target); probe.Init(owner);
                bodyResources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View);
                items = new Kingmaker.Items.ItemEntity[] { owner.Body.PrimaryHand.MaybeWeapon }
                    .Concat(owner.Body.AdditionalLimbs.Select(slot => (Kingmaker.Items.ItemEntity)slot.MaybeWeapon))
                    .Where(value => value != null).Distinct().ToArray();
                float beforeRangePlacement = owner.DistanceTo(target);
                bool inRangeBefore = probe.IsUnitEnoughClose;
                if (!ai)
                {
                    // Derive contact from this actual native command instead
                    // of borrowing a giant weapon's fixed separation margin.
                    float contact = Math.Max(owner.Corpulence + target.Corpulence + .01f,
                        probe.ApproachRadius - .05f);
                    PlaceExpandedSummoningUnit(target, origin + (far - origin).normalized * contact);
                    owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                }
                bool powerAttackAbsent = !owner.Descriptor.HasFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                    BlueprintBootstrap.Library, "9972f33f977fc724c838e59641b2fca5", "native Power Attack control")) &&
                    !owner.Descriptor.ActivatableAbilities.RawFacts.Any(value => value.Blueprint.AssetGuid ==
                        "a7b339e4f6ff93a4697df5d7a87ff619");
                bool ready = owner.IsInGame && owner.View != null && owner.View.IsInGame &&
                    target.IsInGame && target.View != null && target.View.IsInGame &&
                    owner.Descriptor.State.CanAct && owner.IsEnemy(target) && target.IsEnemy(owner) &&
                    owner.HasLOS(target) && owner.Memory.ContainsVisible(target) && powerAttackAbsent &&
                    !target.Descriptor.State.IsDead && owner.Stats.Strength.ModifiedValue == 12 &&
                    owner.Body.PrimaryHand.MaybeWeapon.Blueprint == claw &&
                    owner.Body.AdditionalLimbs.Count() == 2 &&
                    owner.Descriptor.Resources.GetResourceAmount(fear) == (ai ? 0 : 1) &&
                    owner.Descriptor.Resources.GetResourceAmount(cloud) == (ai ? 0 : 1) && (ai ? owner.IsAIEnabled && owner.Brain.Actions.Count > 0 &&
                        owner.Brain.AvailableActions.Count > 0 : probe.IsUnitEnoughClose);
                Sprint25Check("dretch-melee-" + id + "-ready", ready, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["lane"] = lane,
                    ["direction"] = direction, ["los"] = owner.HasLOS(target),
                    ["canAct"] = owner.Descriptor.State.CanAct,
                    ["conscious"] = owner.Descriptor.State.IsConscious,
                    ["ownerEnemy"] = owner.IsEnemy(target), ["targetEnemy"] = target.IsEnemy(owner),
                    ["ownerInCombat"] = owner.IsInCombat, ["targetAlive"] = !target.Descriptor.State.IsDead,
                    ["aiEnabled"] = owner.IsAIEnabled, ["ownerInGame"] = owner.IsInGame,
                    ["viewInGame"] = owner.View != null && owner.View.IsInGame,
                    ["targetInGame"] = target.IsInGame,
                    ["targetViewInGame"] = target.View != null && target.View.IsInGame,
                    ["nativeInRange"] = probe.IsUnitEnoughClose, ["approachRadius"] = probe.ApproachRadius,
                    ["weaponReachFeet"] = owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Value,
                    ["ownerRadius"] = owner.Corpulence, ["targetRadius"] = target.Corpulence,
                    ["distanceBeforeContact"] = beforeRangePlacement, ["inRangeBeforeContact"] = inRangeBefore,
                    ["spawnSettlingFrames"] = 8,
                    ["visible"] = owner.Memory.ContainsVisible(target), ["distance"] = owner.DistanceTo(target),
                    ["fixtureTargetAcBase"] = -100, ["nativeTargetAc"] = target.Descriptor.Stats.AC.ModifiedValue,
                    ["resourcesAtCreation"] = new JArray(resourcesBefore), ["fixtureResourceDebits"] = ai ? 2 : 0,
                    ["fearBeforeCombat"] = owner.Descriptor.Resources.GetResourceAmount(fear),
                    ["cloudBeforeCombat"] = owner.Descriptor.Resources.GetResourceAmount(cloud),
                    ["powerAttackAbsent"] = powerAttackAbsent, ["actions"] = owner.Brain.Actions.Count,
                    ["available"] = owner.Brain.AvailableActions.Count },
                    "An awake visible disposable hostile and the native owner/action preconditions precede combat.");
                if (!ready) throw new InvalidOperationException("Dretch melee fixture not ready: " + id);
                // Same owner and target continue successive real commands;
                // this is live Strength scaling, not a reset/shared-case claim.
                var samples = new[] { new { Strength = 12, Bonus = 1, Attack = 4 },
                    new { Strength = 6, Bonus = -2, Attack = 1 },
                    new { Strength = 18, Bonus = 4, Attack = 7 } };
                foreach (var sample in samples.Take(ai ? 1 : samples.Length))
                {
                    owner.Stats.Strength.BaseValue = sample.Strength;
                    id = ai ? "ai-fallback" : "full-str" + sample.Strength;
                    owner.Commands.RemoveFinishedAndUpdateQueue();
                    var observer = new Sprint24AttackObserver { Owner = owner, Target = target };
                    var costs = new JArray(); var endings = new JArray();
                    var commands = new HashSet<UnitCommand>(); var ambient = new HashSet<string>();
                    UnitAttack issued = ai ? null : new UnitAttack(target) { ForceFullAttack = full, IsSingleAttack = !full };
                    int ticks = 0, frames = 0, impactFrames = 0;
                    EventBus.Subscribe(observer);
                    try
                    {
                        using (var economy = new FirearmReviewCosts(owner, costs))
                        using (var trace = new Sprint26CommandTrace(owner, endings))
                        {
                            if (issued != null) { commands.Add(issued); owner.Commands.Run(issued); }
                            DateTime deadline = DateTime.UtcNow.AddSeconds(45);
                            while (frames++ < 4000 && DateTime.UtcNow < deadline)
                            {
                                if (ai && (frames == 1 || frames % 60 == 0 &&
                                    !owner.Commands.Raw.Any(value => value != null && !value.IsFinished)))
                                {
                                    foreach (string candidate in Sprint24TickExactAi(owner))
                                        if (candidate != owner.UniqueId) ambient.Add(candidate);
                                    ticks++;
                                }
                                foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                                bool terminal = ai ? commands.OfType<UnitAttack>().Any(value => value.AiAction != null &&
                                    value.IsFinished && (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value))) :
                                    issued.IsFinished && observer.WeaponAttacks.Count >= (full ? 3 : 1);
                                if (terminal && (!ai || observer.Damage.Any(value => value.Damage > 0))) break;
                                Game.Instance.IsPaused = false; yield return 0;
                            }
                            int hits = observer.WeaponAttacks.Count(value => value.AttackRoll != null && value.AttackRoll.IsHit);
                            DateTime impactDeadline = DateTime.UtcNow.AddSeconds(10);
                            while (observer.Damage.Count < hits && impactFrames++ < 600 && DateTime.UtcNow < impactDeadline) yield return 0;
                            bool packets = observer.WeaponAttacks.Count > 0 && observer.WeaponAttacks.All(value =>
                                (ReferenceEquals(value.Weapon?.Blueprint, claw) || ReferenceEquals(value.Weapon?.Blueprint, bite)) &&
                                value.AttackRoll != null && !value.AttackRoll.AutoHit &&
                                value.AttackRoll.AttackBonus == sample.Attack && value.AttackRoll.AttackBonusPenalty == 0 &&
                                value.AttackRoll.WeaponStats?.DamageDescription.FirstOrDefault()?.Bonus == sample.Bonus &&
                                value.AttackRoll.WeaponStats.DamageDescription.First().Dice.Rolls == 1 &&
                                value.AttackRoll.WeaponStats.DamageDescription.First().Dice.Dice == DiceType.D4) &&
                                observer.Damage.Count == hits && observer.Rolls.Count == observer.WeaponAttacks.Count;
                            foreach (var hit in observer.WeaponAttacks.Where(value => value.AttackRoll?.IsHit == true))
                                hitWeapons.Add(hit.Weapon.Blueprint.AssetGuid);
                            bool roles = ai || observer.WeaponAttacks.Count(value => ReferenceEquals(value.Weapon.Blueprint, claw)) == 2 &&
                                observer.WeaponAttacks.Count(value => ReferenceEquals(value.Weapon.Blueprint, bite)) == 1;
                            bool resourcesExact = owner.Descriptor.Resources.GetResourceAmount(fear) == (ai ? 0 : 1) &&
                                owner.Descriptor.Resources.GetResourceAmount(cloud) == (ai ? 0 : 1);
                            bool nativeAiCost = !ai || commands.OfType<UnitAttack>().Any(value =>
                                value.AiAction != null && value.IsFinished &&
                                (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value)) &&
                                economy.Count(value) == 1 && economy.Exact(value, false) && !value.Cutscene && !value.IsIgnoreCooldown);
                            bool exact = packets && roles && resourcesExact && nativeAiCost && hits > 0 &&
                                owner.Stats.Strength.ModifiedValue == sample.Strength && !target.Descriptor.State.IsDead && (ai ? ticks > 0 &&
                                commands.OfType<UnitAttack>().Any(value => value.AiAction != null && value.IsFinished &&
                                    (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value))) &&
                                observer.Damage.Any(value => value.Damage > 0) : issued.IsFinished &&
                                (issued.Result == UnitCommand.ResultType.Success || trace.Exhausted(issued)) &&
                                issued.AllAttacks.Count == (full ? 3 : 1) && observer.WeaponAttacks.Count == (full ? 3 : 1) &&
                                observer.OwnerWeaponAttacks.Count == (full ? 3 : 1) &&
                                economy.Count(issued) == 1 && economy.Exact(issued, false) && !issued.Cutscene && !issued.IsIgnoreCooldown);
                            Sprint25Check("dretch-melee-" + id + "-resolved", exact, new JObject {
                                ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["frames"] = frames,
                                ["strength"] = owner.Stats.Strength.ModifiedValue, ["expectedAttack"] = sample.Attack,
                                ["expectedSignedBonus"] = sample.Bonus, ["primaryRoles"] = roles, ["nativeAiCost"] = nativeAiCost,
                                ["fearAfter"] = owner.Descriptor.Resources.GetResourceAmount(fear),
                                ["cloudAfter"] = owner.Descriptor.Resources.GetResourceAmount(cloud),
                                ["issuedFinished"] = issued?.IsFinished, ["issuedResult"] = issued?.Result.ToString(),
                                ["planned"] = issued?.AllAttacks.Count, ["weaponAttacks"] = observer.WeaponAttacks.Count,
                                ["rolls"] = new JArray(observer.WeaponAttacks.Select(value => new JObject {
                                    ["weapon"] = value.Weapon?.Blueprint?.AssetGuid, ["hit"] = value.AttackRoll?.IsHit,
                                    ["natural"] = value.AttackRoll?.Roll.Value, ["attackBonus"] = value.AttackRoll?.AttackBonus,
                                    ["attackPenalty"] = value.AttackRoll?.AttackBonusPenalty,
                                    ["signedDamageBonus"] = value.AttackRoll?.WeaponStats?.DamageDescription.FirstOrDefault()?.Bonus,
                                    ["nativeBaseDice"] = value.AttackRoll?.WeaponStats?.DamageDescription.FirstOrDefault()?.Dice.ToString() })),
                                ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)), ["hits"] = hits,
                                ["physicalAlignments"] = new JArray(observer.Damage.SelectMany(value => value.DamageBundle
                                    .OfType<PhysicalDamage>()).Select(value => value.AlignmentsMask.ToString())),
                                ["impactWaitFrames"] = impactFrames, ["costs"] = costs, ["nativeEnds"] = endings,
                                ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["aiTicks"] = ticks,
                                ["ambientCandidatesNeverTicked"] = new JArray(ambient),
                                ["fixtureAttackCommands"] = ai ? 0 : 1, ["forcedRolls"] = 0,
                                ["powerAttackAbsent"] = powerAttackAbsent }, ai ?
                                "With explicitly spent SLA resources, the native brain chooses and resolves its own natural-attack fallback; no fixture attack command or supplied roll." :
                                "One native full-attack command resolves two primary claws and one primary bite with live signed Strength, actual damage and one action cost; no supplied roll.");
                            if (!exact) throw new InvalidOperationException("Dretch melee consumer failed: " + id);
                        }
                    }
                    finally { EventBus.Unsubscribe(observer); }
                }
                bool liveBoth = ai || hitWeapons.Contains(claw.AssetGuid) && hitWeapons.Contains(bite.AssetGuid);
                Sprint25Check("dretch-melee-" + (ai ? "ai-fallback" : "full") + "-actual-weapons", liveBoth,
                    new JObject { ["owner"] = owner.UniqueId, ["hitWeaponGuids"] = new JArray(hitWeapons) },
                    "The resolved command sequence includes actual hits from both natural weapon kinds; AI separately proves its chosen fallback.");
                if (!liveBoth) throw new InvalidOperationException("Dretch actual damage coverage lacks one natural weapon kind.");
            }
            finally
            {
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); target.Commands.InterruptAll(true);
                if (owner.IsInCombat) owner.LeaveCombat(); if (target.IsInCombat) target.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
                if (target.Memory.Contains(owner)) target.Memory.Remove(owner);
                if (owner.Memory.Contains(target)) owner.Memory.Remove(target);
                owner.Stats.Strength.BaseValue = originalStrength;
                target.Descriptor.Stats.AC.BaseValue = targetAc;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                ResetExpandedSummoningHostile(fixture);
            }
            for (int frame = 0; frame < 8; frame++) yield return 0;
            var itemRows = new JArray((items ?? Array.Empty<Kingmaker.Items.ItemEntity>()).Select(value =>
                OutsiderEscapeItemState(value, owner, new[] { target })));
            bool itemsSafe = items != null && items.Length == 3 && itemRows.All(value => (bool)value["safe"]);
            bool bodyReleased = bodyResources != null && bodyResources.Length >= 3 && bodyResources.All(value => value == null);
            bool inventoryExact = inventoryBefore.SequenceEqual(Game.Instance.Player.Inventory.Items) &&
                inventoryBefore.Select(value => value.Count).SequenceEqual(inventoryCounts);
            bool areasExact = areasBefore.Length == Game.Instance.State.AreaEffects.All.Count() &&
                areasBefore.All(value => Game.Instance.State.AreaEffects.All.Contains(value));
            bool clean = owner.Destroyed && !owner.IsInState && !fixture.Created.Contains(owner) &&
                !target.Memory.Contains(owner) && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished) &&
                !target.Commands.Raw.Any(value => value != null && !value.IsFinished) &&
                target.Stats.AC.BaseValue == targetAc && target.Damage == fixture.HostileDamage &&
                itemsSafe && bodyReleased && inventoryExact && areasExact;
            Sprint25Check("dretch-melee-" + (ai ? "ai-fallback" : "full") + "-cleanup", clean,
                new JObject { ["ownerAbsent"] = !owner.IsInState, ["items"] = itemRows,
                    ["bodyResourcesReleased"] = bodyReleased, ["partyInventoryExact"] = inventoryExact,
                    ["areasExact"] = areasExact, ["targetCombatState"] = target.IsInCombat,
                    ["nextCaseAdmitted"] = false, ["freshProcessBoundary"] = true },
                "Owned actors, natural items and private body resources are gone; party inventory and areas remain exact. Native combat cases retain the established fresh-process boundary.");
            if (!clean) throw new InvalidOperationException("Dretch melee owned cleanup failed.");
        }
    }
}
