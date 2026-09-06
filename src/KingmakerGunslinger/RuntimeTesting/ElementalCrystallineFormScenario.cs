using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Native commands and attack/effect resolution on named disposable
    /// units. Only asynchronous projectile travel is completed at its impact
    /// boundary: no direct invocation of the trait or damage effect.</summary>
    internal static class ElementalCrystallineFormScenario
    {
        private const string Frost = "9af2ab69df6538f4793b2f9c3cc85603";
        private const string Snowball = "9f10909f0be1f5141bf1c102041f93d9";

        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            var rows = new JArray();
            var diagnostics = new List<string>();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            UnityEngine.Random.State random = UnityEngine.Random.state;
            if (Game.Instance.ProjectileController.Projectiles.Any())
                throw new InvalidOperationException("Ray fixture refuses preexisting projectiles.");
            ElementalRaceBlueprints race = BlueprintBootstrap.ElementalRaces.Oread;
            try
            {
                foreach (ElementalHeritageBlueprints heritage in race.Heritages.Choices())
                {
                    var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    try
                    {
                        UnitEntityData defender = fixture.Caster;
                        // Native ranged-touch attacks against willing allies
                        // auto-hit before AC calculation. Give only this request's
                        // attacker a hostile, unregistered faction clone.
                        BlueprintFaction hostile = UnityEngine.Object.Instantiate(defender.Blueprint.Faction);
                        hostile.name = "KMG_Runtime_Crystalline_HostileFaction";
                        hostile.Peaceful = false;
                        hostile.AlwaysEnemy = false;
                        hostile.Neutral = false;
                        hostile.IsDirectlyControllable = false;
                        hostile.Dummy = null;
                        hostile.AttackFactions = new[] { defender.Blueprint.Faction };
                        UnitEntityData attacker = fixture.SpawnFixtureUnit(race.Race,
                            hostile, new Vector3(3, 0, 0), "CrystallineRayAttacker");
                        Check(assertions, rows, heritage.Definition.Id + "-native-hostility",
                            defender.IsEnemy(attacker) && attacker.IsEnemy(defender),
                            "native groups recognize hostility; no shared faction array mutated");
                        foreach (UnitEntityData unit in new[] { attacker, defender })
                        {
                            unit.CombatState.JoinCombat();
                            unit.CombatState.OnNewRound();
                        }
                        attacker.Memory.Add(defender);
                        defender.Memory.Add(attacker);
                        Run(attacker, defender, race, heritage, rows, assertions);
                    }
                    finally
                    {
                        // Native controller cleanup owns these request-local
                        // projectile views. No Destroy on registered objects.
                        foreach (Projectile projectile in Game.Instance.ProjectileController.Projectiles.ToArray())
                            projectile.Cleared = true;
                        Game.Instance.ProjectileController.Tick();
                        fixture.Dispose();
                        Check(assertions, rows, heritage.Definition.Id + "-native-lifetime",
                            fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                            fixture.NativeObservationReleased && fixture.NativeTeardownObserved &&
                            fixture.AreaContextRestored && fixture.PlayerContextRestored,
                            "errors=" + fixture.NativeErrors + ";exceptions=" + fixture.NativeExceptions);
                    }
                }
            }
            finally
            {
                UnityEngine.Random.state = random;
                bool clean = Game.Instance.State.Units.All.Count == before.Length &&
                    before.All(value => Game.Instance.State.Units.All.Contains(value)) &&
                    !Game.Instance.ProjectileController.Projectiles.Any();
                Check(assertions, rows, "fixture-cleanup", clean, "exact units and empty original projectile catalog");
                string path = Path.Combine(request.EvidenceDirectory, "elemental-crystalline-form.json");
                File.WriteAllText(path, new JObject {
                    { "schemaVersion", 1 }, { "saveStateTouched", false }, { "cleanupExact", clean },
                    { "isolatedBoundary", "request-local asynchronous projectile arrival only; native command, projectile creation, attack roll, OnHit event and damage effects retained" },
                    { "diagnostics", new JArray(diagnostics) }, { "observations", rows }
                }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void Run(UnitEntityData attacker, UnitEntityData defender, ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage, JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            UnitDescriptor owner = defender.Descriptor;
            owner.AddFact(heritage.Marker);
            ElementalAlternateTraitBlueprints trait = race.AlternateTraits.Require(ElementalAlternateTraitId.CrystallineForm);
            BlueprintAbilityResource resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
            BlueprintActivatableAbility modeBlueprint = trait.Mechanics().OfType<BlueprintActivatableAbility>().Single();
            string prefix = heritage.Definition.Id + "-";
            BlueprintAbility frost = Require(Frost);
            owner.AddFact(trait.Marker);
            ActivatableAbility mode = Mode(owner, modeBlueprint);
            Check(assertions, rows, prefix + "owned-graph",
                owner.HasFact(trait.Provider) && owner.HasFact(race.Resistance) && owner.HasFact(heritage.SlaFeature) &&
                !owner.HasFact(heritage.Affinity) && resource.GetMaxAmount(owner) == 1 && !mode.IsOn &&
                ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits),
                "affinity replaced, resistance/SLA retained; independent use and off-by-default mode");

            // Every cataloged native/project ray traverses actual RuleAttackRoll
            // and nested native touch AC, including racial stacking and cleanup.
            foreach (string guid in ElementalCrystallineFormPolicy.RayAbilityGuids.Concat(new[] {
                Snowball, "0c852a2405dd9f14a8bbcfaf245ff823", "9a46dfd390f943647ab4395fc997936d",
                "0a2f7c6aa81bc6548ac7780d8b70bcbc", "5e1db2ef80ff361448549beeb7785791" }))
            {
                BlueprintAbility ability = Require(guid);
                bool nativeDelivery = ability.ComponentsArray.OfType<AbilityDeliverProjectile>().Any();
                if (!nativeDelivery && guid != "e50e2db3d78b7ff4aa5c9699ba26febe")
                    throw new InvalidOperationException("Unexpected changed ray delivery: " + guid);
                owner.RemoveFact(trait.Marker);
                int baseline = ArmorClass(attacker, defender, ability);
                int raw = owner.Stats.AC.ModifiedValue;
                owner.AddFact(trait.Marker);
                int current = ArmorClass(attacker, defender, ability);
                int expected = nativeDelivery && ElementalCrystallineFormPolicy.RayAbilityGuids.Contains(guid) ? 2 : 0;
                Check(assertions, rows, prefix + "ray-ac-" + ability.name,
                    current - baseline == expected && owner.Stats.AC.ModifiedValue == raw,
                    "baseline=" + baseline + ";withTrait=" + current + ";expectedDelta=" + expected + ";rawRestored=" + (owner.Stats.AC.ModifiedValue == raw) +
                    (nativeDelivery ? "" : ";native delivery absent: paired ray weapon negative control only; optional replacement mechanic NOT-RUN;components=" +
                        string.Join(",", ability.ComponentsArray.Select(value => value.GetType().FullName))));
            }
            mode = Mode(owner, modeBlueprint);
            int withoutRacial = ArmorClass(attacker, defender, frost);
            var racial = owner.Stats.AC.AddModifier(4, owner.GetFact(heritage.Marker),
                "KMG_Runtime_Crystalline_RacialControl", ModifierDescriptor.Racial);
            owner.Stats.AC.UpdateValue();
            int strongerRacial = ArmorClass(attacker, defender, frost);
            racial.Remove();
            Check(assertions, rows, prefix + "independent-racial-stacking", strongerRacial - withoutRacial == 4,
                "PRD bonus-type exception and native Racial preserve both independent sources (2 + 4); delta=" +
                (strongerRacial - withoutRacial));

            float[] actionBudget = Cooldowns(defender);
            mode.IsOn = true;
            var consent = owner.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, modeBlueprint.Buff));
            try
            {
                owner.TurnOff(false);
                Check(assertions, rows, prefix + "unit-unload-retains-consent",
                    !owner.IsTurnedOn && ReferenceEquals(Mode(owner, modeBlueprint), mode) && mode.IsOn &&
                    owner.Buffs.Enumerable.Any(value => ReferenceEquals(value, consent)) &&
                    owner.Resources.GetResourceAmount(resource) == 1,
                    "native whole-unit TurnOff is not permanent race or trait removal");
            }
            finally { owner.TurnOn(); }
            Check(assertions, rows, prefix + "unit-reload-retains-consent",
                owner.IsTurnedOn && ElementalHeritageRuntime.Reconcile(owner, null, null) &&
                ReferenceEquals(Mode(owner, modeBlueprint), mode) && mode.IsOn &&
                owner.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, modeBlueprint.Buff)) == consent &&
                consent.Active && owner.Resources.GetResourceAmount(resource) == 1,
                "same native mode and buff, exact resource and idempotent provider reconciliation after TurnOn");
            mode.IsOn = false;
            Check(assertions, rows, prefix + "toggle-cancel",
                owner.Resources.GetResourceAmount(resource) == 1 && !owner.HasFact(modeBlueprint.Buff) &&
                actionBudget.SequenceEqual(Cooldowns(defender)),
                "native opt-in then cancel spends no use or standard/move/swift action and removes consent buff");
            CastEvidence unarmedOff = Cast(attacker, defender, frost, true);
            Check(assertions, rows, prefix + "off-allows-ray",
                unarmedOff.Completed && unarmedOff.Damage > 0 && unarmedOff.Hits == 1 &&
                owner.Resources.GetResourceAmount(resource) == 1, unarmedOff.ToString());

            mode.IsOn = true;
            CastEvidence deflected = Cast(attacker, defender, frost, true);
            Check(assertions, rows, prefix + "native-hit-deflected",
                deflected.Completed && deflected.Projectiles == 1 && deflected.Parried == 1 && deflected.Damage == 0 &&
                owner.Resources.GetResourceAmount(resource) == 0 && !mode.IsOn && !mode.IsAvailable &&
                actionBudget.SequenceEqual(Cooldowns(defender)),
                deflected + ";uses=" + owner.Resources.GetResourceAmount(resource) + ";freeHand=" +
                ElementalCrystallineFormRuntime.HasFreeHand(owner) + ";canAct=" + defender.CombatState.CanActInCombat +
                ";flat=" + Rulebook.Trigger(new RuleCheckTargetFlatFooted(attacker, defender)).IsFlatFooted);

            CastEvidence spent = Cast(attacker, defender, frost, true);
            Check(assertions, rows, prefix + "spent-use-cannot-deflect",
                spent.Completed && spent.Projectiles == 1 && spent.Hits == 1 && spent.Parried == 0 &&
                spent.Damage > 0 && owner.Resources.GetResourceAmount(resource) == 0, spent.ToString());

            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            mode.IsOn = true;
            CastEvidence nonray = Cast(attacker, defender, Require(Snowball), true);
            Check(assertions, rows, prefix + "non-ray-retains-use",
                nonray.Completed && nonray.Damage > 0 && nonray.Parried == 0 &&
                owner.Resources.GetResourceAmount(resource) == 1 && mode.IsOn, nonray.ToString());
            CastEvidence miss = Cast(attacker, defender, frost, false);
            Check(assertions, rows, prefix + "miss-retains-use", miss.Completed && miss.Hits == 0 &&
                miss.Parried == 0 && miss.Damage == 0 && owner.Resources.GetResourceAmount(resource) == 1, miss.ToString());
            owner.Resources.Spend(resource, 1);
            owner.GetFact(trait.Provider).Deactivate();
            owner.GetFact(trait.Provider).Activate();
            Check(assertions, rows, prefix + "reactivation-retains-spent",
                ElementalHeritageRuntime.Reconcile(owner, null, null) && owner.Resources.GetResourceAmount(resource) == 0,
                "native deactivate/activate and provider reconciliation do not refill");
            owner.RemoveFact(trait.Marker);
            Check(assertions, rows, prefix + "remove-cleans-owned",
                owner.HasFact(heritage.Affinity) && !owner.HasFact(modeBlueprint.Buff) &&
                ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits),
                "native affinity restored; exact mode/buff/resource cleaned");
            owner.AddFact(trait.Marker);
            Check(assertions, rows, prefix + "readd-retains-spent", owner.Resources.GetResourceAmount(resource) == 0,
                "true marker removal/re-add retains daily expenditure");
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            Check(assertions, rows, prefix + "rest-one-use", owner.Resources.GetResourceAmount(resource) == 1,
                "ordinary native rest restores exactly one deflection");
            // The preceding real marker removal/re-add replaces AddFacts-owned
            // activatables. Never drive the detached pre-respec instance.
            EquipmentAndAwareness(attacker, defender, resource, Mode(owner, modeBlueprint), frost, prefix, rows, assertions);
        }

        private static void EquipmentAndAwareness(UnitEntityData attacker, UnitEntityData defender,
            BlueprintAbilityResource resource, ActivatableAbility mode, BlueprintAbility frost, string prefix,
            JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            UnitDescriptor owner = defender.Descriptor;
            if (!ReferenceEquals(Mode(owner, mode.Blueprint), mode))
                throw new InvalidOperationException("Ray consent must use the current native owned activatable.");
            if (owner.Body.PrimaryHand.HasItem || owner.Body.SecondaryHand.HasItem)
                throw new InvalidOperationException("Ray equipment fixture requires initially empty hands.");
            BlueprintItemWeapon sword = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                BlueprintBootstrap.Library, "57c8994d1f1becf49ac4f642e5d8ca9d", "native short sword");
            var primary = new ItemEntityWeapon(sword);
            var secondary = new ItemEntityWeapon(sword);
            try
            {
                owner.Body.PrimaryHand.InsertItem(primary);
                owner.Body.SecondaryHand.InsertItem(secondary);
                mode.IsOn = true;
                CastEvidence occupied = Cast(attacker, defender, frost, true);
                Check(assertions, rows, prefix + "both-hands-occupied",
                    ReferenceEquals(owner.Body.PrimaryHand.MaybeItem, primary) &&
                    ReferenceEquals(owner.Body.SecondaryHand.MaybeItem, secondary) &&
                    !ElementalCrystallineFormRuntime.HasFreeHand(owner) && occupied.Completed &&
                    occupied.Hits == 1 && occupied.Parried == 0 && occupied.Damage > 0 &&
                    owner.Resources.GetResourceAmount(resource) == 1 && mode.IsOn, occupied.ToString());
                owner.Body.SecondaryHand.RemoveItem(false);
                CastEvidence free = Cast(attacker, defender, frost, true);
                Check(assertions, rows, prefix + "free-hand-restores-deflection",
                    ElementalCrystallineFormRuntime.HasFreeHand(owner) && free.Completed &&
                    free.Parried == 1 && free.Damage == 0 && owner.Resources.GetResourceAmount(resource) == 0,
                    free + ";native equipment removal requires no rest or trait re-add");
            }
            finally
            {
                if (ReferenceEquals(owner.Body.PrimaryHand.MaybeItem, primary)) owner.Body.PrimaryHand.RemoveItem(false);
                if (ReferenceEquals(owner.Body.SecondaryHand.MaybeItem, secondary)) owner.Body.SecondaryHand.RemoveItem(false);
                primary.Dispose();
                secondary.Dispose();
            }
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            foreach (UnitCondition condition in new[] { UnitCondition.Blindness, UnitCondition.Paralyzed })
            {
                mode.IsOn = true;
                owner.State.AddCondition(condition, null);
                try
                {
                    bool flat = Rulebook.Trigger(new RuleCheckTargetFlatFooted(attacker, defender)).IsFlatFooted;
                    CastEvidence blocked = Cast(attacker, defender, frost, true);
                    Check(assertions, rows, prefix + "native-condition-" + condition,
                        owner.State.HasCondition(condition) && (flat || owner.State.IsHelpless) &&
                        blocked.Completed && blocked.Parried == 0 && blocked.Damage > 0 &&
                        owner.Resources.GetResourceAmount(resource) == 1,
                        blocked + ";flatFooted=" + flat + ";helpless=" + owner.State.IsHelpless);
                }
                finally { owner.State.RemoveCondition(condition); }
            }
            mode.IsOn = true;
            CastEvidence recovered = Cast(attacker, defender, frost, true);
            Check(assertions, rows, prefix + "awareness-restored",
                recovered.Completed && recovered.Parried == 1 && recovered.Damage == 0 &&
                owner.Resources.GetResourceAmount(resource) == 0, recovered.ToString());
        }

        private static int ArmorClass(UnitEntityData attacker, UnitEntityData target, BlueprintAbility ability)
        {
            AbilityData data = Data(attacker.Descriptor, ability);
            var context = new AbilityExecutionContext(data, data.CalculateParams(), new TargetWrapper(target), null);
            AbilityDeliverProjectile delivery = ability.ComponentsArray.OfType<AbilityDeliverProjectile>().SingleOrDefault();
            // A foreign rewrite can leave a cataloged identity without native
            // ray delivery. Deliberately pair that context with the known native
            // ray weapon as a negative control; never reconstruct its old spell.
            var weapon = new ItemEntityWeapon((delivery ?? Require(Frost).ComponentsArray
                .OfType<AbilityDeliverProjectile>().Single()).Weapon);
            var roll = new RuleAttackRoll(attacker, target, weapon, 100) { Reason = new RuleReason(context) };
            UnityEngine.Random.InitState(7419);
            Rulebook.Trigger(roll);
            if (roll.ACRule == null) throw new InvalidOperationException("Ray probe did not resolve native AC: " +
                ability.name + ";enemy=" + target.IsEnemy(attacker) + ";autoHit=" + roll.AutoHit +
                ";autoMiss=" + roll.AutoMiss + ";result=" + roll.Result);
            return roll.ACRule.TargetAC;
        }

        private sealed class CastEvidence
        {
            internal bool Completed;
            internal int Projectiles, Hits, Parried, Damage;
            internal string Sources;
            public override string ToString() { return "completed=" + Completed + ";projectiles=" + Projectiles +
                ";hits=" + Hits + ";parried=" + Parried + ";damage=" + Damage + ";sources=" + Sources; }
        }

        private static CastEvidence Cast(UnitEntityData caster, UnitEntityData target, BlueprintAbility ability, bool hit)
        {
            var observed = new List<Projectile>();
            var previous = new HashSet<Projectile>(Game.Instance.ProjectileController.Projectiles);
            int damage = target.Damage;
            int bonus = caster.Stats.AdditionalAttackBonus.BaseValue;
            caster.Stats.AdditionalAttackBonus.BaseValue = hit ? 100 : -100;
            var command = ElementalUndineFeatScenario.CreateCommand(Data(caster.Descriptor, ability), new TargetWrapper(target), caster);
            command.ForceAlwaysHit = hit;
            UnityEngine.Random.InitState(7419);
            try
            {
                ElementalUndineFeatScenario.InvokeCommandAction(command);
                for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 100; tick++)
                {
                    command.ExecutionProcess.Tick();
                    foreach (Projectile projectile in Game.Instance.ProjectileController.Projectiles.Where(value =>
                        !previous.Contains(value) && !value.Cleared && ReferenceEquals(value.Launcher, caster) &&
                        !observed.Contains(value)).ToArray())
                    {
                        observed.Add(projectile);
                        // Complete only the asynchronous transport boundary.
                        // Native OnHit and subsequent delivery/effects are untouched.
                        typeof(Projectile).GetProperty("IsHit").GetSetMethod(true).Invoke(projectile, new object[] { true });
                        projectile.OnHit();
                    }
                }
                bool complete = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded;
                if (!complete && command.ExecutionProcess != null) command.ExecutionProcess.Detach();
                ElementalUndineFeatScenario.InvokeCommandEnded(command, !complete);
                return new CastEvidence { Completed = complete, Projectiles = observed.Count,
                    Hits = observed.Count(value => value.AttackRoll != null && value.AttackRoll.IsHit),
                    Parried = observed.Count(value => value.AttackRoll != null && value.AttackRoll.Result == AttackResult.Parried),
                    Damage = target.Damage - damage,
                    Sources = string.Join(",", observed.Select(value => value.AttackRoll == null ? "no-roll" :
                        "eligible=" + ElementalCrystallineFormRuntime.IsRay(value.AttackRoll) + "/reason=" + value.AttackRoll.Reason.Name)) };
            }
            finally
            {
                caster.Stats.AdditionalAttackBonus.BaseValue = bonus;
                foreach (Projectile projectile in observed) projectile.Cleared = true;
                Game.Instance.ProjectileController.Tick();
            }
        }

        private static AbilityData Data(UnitDescriptor owner, BlueprintAbility ability)
        {
            if (owner.Abilities.GetAbility(ability) == null) owner.AddFact(ability);
            return new AbilityData(owner.Abilities.GetAbility(ability));
        }
        private static float[] Cooldowns(UnitEntityData unit) { return new[] {
            unit.CombatState.Cooldown.StandardAction, unit.CombatState.Cooldown.MoveAction,
            unit.CombatState.Cooldown.SwiftAction }; }
        private static BlueprintAbility Require(string guid) { return BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
            BlueprintBootstrap.Library, guid, "exact Crystalline Form native/project ray witness"); }
        private static ActivatableAbility Mode(UnitDescriptor owner, BlueprintActivatableAbility blueprint) {
            return owner.ActivatableAbilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, blueprint)); }
        private static void Check(ICollection<RuntimeTestAssertion> assertions, JArray rows, string name, bool pass, string observed)
        {
            rows.Add(new JObject { { "name", name }, { "pass", pass }, { "observed", observed } });
            assertions.Add(new RuntimeTestAssertion { Name = "elemental-crystalline-" + name,
                Expected = "true", Observed = observed, Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "native disposable command/rule/impact scenario; projectile travel isolated; no save access" });
        }
    }
}
