using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Mechanics;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbAcidBombGuid = "fd101fbc4aacf5d48b76a65e3aa5db6d";
        private const string FcbForceBombGuid = "557898e059f5ff644848b0a4df087391";
        private const string FcbHolyBombGuid = "b94ee802dc1574b4fb71215a4a6f11dc";
        private const string FcbExplosiveBombGuid = "2b76e3bd89b4fa0419853a69fec0785f";
        private const string FcbArcaneFireBombGuid = "cd65a240f32aa7441a0f10b1a86ed520";
        private const string FcbBreathWeaponBombGuid = "17041de68a89a5f4b92889c3ed475c00";
        private const string FcbAlchemistBombsFeatureGuid = "c59b2f256f5a70a4d896568658315b7d";

        /// <summary>Forces the thrower's attack result and chosen saves; records every damage rule.</summary>
        private sealed class FcbBombObserver : IGlobalRulebookHandler<RuleAttackRoll>,
            IGlobalRulebookHandler<RuleSavingThrow>, IGlobalRulebookHandler<RuleDealDamage>
        {
            internal UnitEntityData Thrower;
            internal string AttackMode = "hit";
            internal readonly HashSet<UnitEntityData> FailSave = new HashSet<UnitEntityData>();
            internal readonly HashSet<UnitEntityData> PassSave = new HashSet<UnitEntityData>();
            internal readonly List<RuleDealDamage> Damage = new List<RuleDealDamage>();

            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (evt == null || !ReferenceEquals(evt.Initiator, Thrower)) return;
                if (AttackMode == "miss")
                {
                    evt.AutoMiss = true;
                    return;
                }
                evt.AutoHit = true;
                if (AttackMode == "critical")
                {
                    evt.AutoCriticalThreat = true;
                    evt.AutoCriticalConfirmation = true;
                }
            }

            public void OnEventDidTrigger(RuleAttackRoll evt) { }

            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (evt != null && PassSave.Contains(evt.Initiator)) evt.AutoPass = true;
            }

            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                // A failed save that no natural roll can turn: the result is
                // decided before the saved branch reads it.
                if (evt != null && FailSave.Contains(evt.Initiator)) evt.BaseRollResult = -100;
            }

            public void OnEventAboutToTrigger(RuleDealDamage evt) { }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (evt != null) Damage.Add(evt);
            }
        }

        // M14: real bomb abilities thrown by a live thrower at live targets:
        // the production bomb's own projectile delivery, attack roll, 8-ft
        // splash with Reflex saves and damage actions run natively; only the
        // projectile transport is completed and the attack and save results
        // are forced. The same seeded throw with and without the counter must
        // differ by exactly the earned steps on each damage roll's flat bonus,
        // once per damaged creature (direct hit, splash with a failed or a
        // passed save, the miss splash, a critical, converted variants), and
        // by nothing for a bomb outside the proven lineage or a bomb buff's
        // follow-up damage.
        private RuntimeTestResult RunFavoredClassBombs()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                leaves != null && FavoredClassRuntime.MechanicsEnabled &&
                !Game.Instance.State.Units.All.Any() && !Game.Instance.Player.Party.Any() &&
                Game.Instance.CurrentlyLoadedArea == null;
            assertions.Add(Assertion("fcb-integration-published",
                "the exact qualified host is ready, mechanics are enabled and the save-free world is empty",
                status.ToString(), ready, "FavoredClassIntegrationStatusRegistry; empty main-menu world"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            var evidence = new JObject();
            var failures = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string key in new[] { "direct", "splash", "miss", "critical", "variants", "outside", "followUp",
                "cleanup" })
                failures[key] = new List<string>();
            var diagnostics = new List<string>();
            var scene = new ElementalUndineFeatScenario.PortalHarness(diagnostics);
            BlueprintFaction hostile = null;
            var observer = new FcbBombObserver();
            bool subscribed = false;
            UnitEntityData thrower = null;
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            PropertyInfo handsProperty = typeof(Game).GetProperty("HandsEquipmentController",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            UnitHandEquipmentController handsBefore = Game.Instance.HandsEquipmentController;
            if (handsBefore == null)
                handsProperty.SetValue(Game.Instance, new UnitHandEquipmentController(), null);
            try
            {
                var library = BlueprintBootstrap.Library;
                Func<string, BlueprintAbility> ability = guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, guid, guid);
                BlueprintRace human = BlueprintRoot.Instance.Progression.CharacterRaces.Single(race =>
                    race.AssetGuid == FcbHumanRace);
                thrower = scene.Initialize(human);
                thrower.Descriptor.Stats.Intelligence.BaseValue = 16;
                thrower.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    FcbAlchemistBombsFeatureGuid, "AlchemistBombsFeature"));
                hostile = UnityEngine.Object.Instantiate(thrower.Blueprint.Faction);
                hostile.name = "KMG_Runtime_FavoredClassBombs_Hostile";
                hostile.Peaceful = hostile.AlwaysEnemy = hostile.Neutral = hostile.IsDirectlyControllable = false;
                hostile.Dummy = null;
                hostile.AttackFactions = new[] { thrower.Blueprint.Faction };
                // The main target 4 m away (beyond the 8-ft splash from the
                // thrower); two splash targets 1.2 m from it.
                Vector3 origin = thrower.Position;
                UnitEntityData main = scene.SpawnFixtureUnit(human, hostile, origin + new Vector3(0, 0, 4f),
                    "FavoredClassBombMain");
                UnitEntityData splashFail = scene.SpawnFixtureUnit(human, hostile,
                    origin + new Vector3(1.2f, 0, 4f), "FavoredClassBombSplashFail");
                UnitEntityData splashPass = scene.SpawnFixtureUnit(human, hostile,
                    origin + new Vector3(-1.2f, 0, 4f), "FavoredClassBombSplashPass");
                var labels = new Dictionary<UnitEntityData, string>
                {
                    { thrower, "thrower" }, { main, "main" }, { splashFail, "splashFailedSave" },
                    { splashPass, "splashPassedSave" }
                };
                foreach (UnitEntityData unit in new[] { main, splashFail, splashPass })
                {
                    unit.Descriptor.Stats.HitPoints.BaseValue = 1000;
                    thrower.Memory.Add(unit);
                    unit.Memory.Add(thrower);
                }
                observer.Thrower = thrower;
                observer.FailSave.Add(splashFail);
                // The aimed target saves only against a miss's splash.
                observer.FailSave.Add(main);
                observer.PassSave.Add(splashPass);
                EventBus.Subscribe(observer);
                subscribed = true;
                FavoredClassLeafPair pair = leaves.Pair(FavoredClassCatalog.EffectBombDamage, null);
                Action invest = () => GrantFavoredClassRanks(thrower, pair.Full, 3);
                Action divest = () => RemoveFavoredClassRanks(thrower, pair.Full);
                invest();
                int steps = FavoredClassEarnedSteps.For(thrower.Descriptor, FavoredClassCatalog.EffectBombDamage, null);
                divest();
                evidence["steps"] = steps;
                if (steps != 3)
                    failures["direct"].Add("three full ranks earned " + steps + " steps, expected 3");

                // One seeded throw per configuration, control then invested.
                Func<BlueprintAbility, string, UnitEntityData, JArray> pairThrow = (bomb, mode, target) =>
                {
                    var both = new JArray();
                    foreach (bool invested in new[] { false, true })
                    {
                        if (invested) invest(); else divest();
                        both.Add(FcbThrow(thrower, bomb, target, observer, mode, 7419, labels));
                    }
                    divest();
                    return both;
                };
                BlueprintAbility standard = ability(FcbBombStandardGuid);
                JArray hit = pairThrow(standard, "hit", main);
                JArray critical = pairThrow(standard, "critical", main);
                JArray miss = pairThrow(standard, "miss", main);
                evidence["standardHit"] = hit;
                evidence["standardCritical"] = critical;
                evidence["standardMiss"] = miss;
                FcbCompareBombThrow(hit, "main", steps, 1, false, failures["direct"], "direct hit");
                FcbCompareBombThrow(hit, "splashFailedSave", steps, 1, false, failures["splash"], "splash, failed save");
                FcbCompareBombThrow(hit, "splashPassedSave", steps, 1, true, failures["splash"], "splash, passed save");
                FcbCompareBombThrow(critical, "main", steps, 2, false, failures["critical"], "critical direct hit");
                FcbCompareBombThrow(critical, "splashFailedSave", steps, 2, false, failures["critical"],
                    "splash of a critical (the game multiplies it too)");
                FcbCompareBombThrow(miss, "main", steps, 1, false, failures["miss"], "miss splash on the aimed target");
                FcbCompareBombThrow(miss, "splashFailedSave", steps, 1, false, failures["miss"], "miss splash, failed save");

                // Converted variants in the lineage (Acid, Force, Holy against an
                // evil target, Explosive, the Arcane Bomber's fire bomb).
                var variants = new JObject();
                main.Descriptor.Alignment.Set(Kingmaker.Enums.Alignment.NeutralEvil);
                foreach (var variant in new[]
                {
                    Tuple.Create("acid", FcbAcidBombGuid), Tuple.Create("force", FcbForceBombGuid),
                    Tuple.Create("holy", FcbHolyBombGuid), Tuple.Create("explosive", FcbExplosiveBombGuid),
                    Tuple.Create("arcaneFire", FcbArcaneFireBombGuid),
                })
                {
                    JArray throws = pairThrow(ability(variant.Item2), "hit", main);
                    variants[variant.Item1] = throws;
                    FcbCompareBombThrow(throws, "main", steps, 1, null, failures["variants"], variant.Item1 + " direct hit");
                }
                evidence["variants"] = variants;

                // A bomb-like ability outside the proven lineage gains nothing.
                JArray outside = pairThrow(ability(FcbBreathWeaponBombGuid), "hit", main);
                evidence["outsideLineage"] = outside;
                FcbCompareBombThrow(outside, null, 0, 1, null, failures["outside"], "Breath Weapon Bomb (outside the lineage)");

                // The acid bomb's lingering damage runs in its buff's context.
                var acidBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, FcbAcidBombBuffGuid,
                    "AcidBombBuff");
                var followUps = new JArray();
                foreach (bool invested in new[] { false, true })
                {
                    if (invested) invest(); else divest();
                    FcbThrow(thrower, ability(FcbAcidBombGuid), main, observer, "hit", 7419, labels);
                    observer.Damage.Clear();
                    bool had = main.Descriptor.HasFact(acidBuff);
                    UnityEngine.Random.InitState(1234);
                    if (had) main.Descriptor.RemoveFact(acidBuff);
                    followUps.Add(new JObject { ["invested"] = invested, ["hadBuff"] = had,
                        ["damage"] = FcbDescribeDamage(observer.Damage, labels) });
                }
                divest();
                evidence["acidFollowUp"] = followUps;
                if (!(bool)followUps[0]["hadBuff"] || !(bool)followUps[1]["hadBuff"])
                    failures["followUp"].Add("the acid bomb applied no lingering buff to follow");
                else
                {
                    var controlEvents = (JArray)followUps[0]["damage"];
                    var investedEvents = (JArray)followUps[1]["damage"];
                    if (controlEvents.Count == 0 || controlEvents.Count != investedEvents.Count)
                        failures["followUp"].Add("the lingering acid damage events do not pair");
                    for (int index = 0; index < Math.Min(controlEvents.Count, investedEvents.Count); index++)
                        if ((int)investedEvents[index]["firstBonus"] != (int)controlEvents[index]["firstBonus"] ||
                            (int)investedEvents[index]["damage"] != (int)controlEvents[index]["damage"])
                            failures["followUp"].Add("the lingering acid damage received the bonus");
                }
            }
            catch (Exception exception)
            {
                failures["cleanup"].Add("exception=" + exception);
            }
            finally
            {
                try
                {
                    if (subscribed) EventBus.Unsubscribe(observer);
                    scene.Dispose();
                    new SleepingUnitsController().Tick();
                    if (hostile != null) UnityEngine.Object.DestroyImmediate(hostile);
                    bool restored = !Game.Instance.State.Units.All.Any() &&
                        !Game.Instance.State.AwakeUnits.Any() && !Game.Instance.AwakeUnitGroups.Any() &&
                        scene.PlayerContextRestored && scene.AreaContextRestored;
                    evidence["restored"] = restored;
                    if (!restored) failures["cleanup"].Add("the save-free fixture was not restored exactly");
                }
                finally
                {
                    UnityEngine.Random.state = randomBefore;
                    if (handsBefore == null) handsProperty.SetValue(Game.Instance, null, null);
                }
            }
            evidence["diagnostics"] = new JArray(diagnostics);
            string evidencePath = WriteFavoredClassEvidence("favored-class-bombs.json", evidence);
            Action<string, string, string> add = (key, id, expectation) => assertions.Add(Assertion(id, expectation,
                Describe(null, failures[key]), failures[key].Count == 0,
                "native bomb ability delivery and effects (AbilityExecutionProcess); RuleDealDamage observer"));
            add("direct", "fcb-bombs-direct-hit",
                "a real Bomb's direct hit: the invested throw's flat damage bonus and its damage exceed the same seeded control throw's by exactly the three earned steps, on one damage roll");
            add("splash", "fcb-bombs-splash",
                "each splashed creature takes one damage roll whose flat bonus exceeds the control's by exactly the earned steps; a failed save keeps it whole, a passed save halves the sum");
            add("miss", "fcb-bombs-miss",
                "a missed bomb's splash (on the aimed target and around it) carries the same flat bonus once per creature");
            add("critical", "fcb-bombs-critical",
                "a confirmed critical doubles the bonus with the rest of the damage (the game multiplies the throw's splash too), never adding it twice");
            add("variants", "fcb-bombs-variants",
                "the lineage's converted variants (Acid, Force, Holy against an evil target, Explosive, the Arcane Bomber's fire bomb) each carry the earned steps once on their damage roll");
            add("outside", "fcb-bombs-outside-lineage",
                "a bomb-like ability outside the proven Fast Bombs lineage (Breath Weapon Bomb) gains nothing");
            add("followUp", "fcb-bombs-follow-up",
                "the acid bomb's lingering damage runs in its buff's context and gains nothing");
            add("cleanup", "fcb-bombs-cleanup", "the save-free fixture is restored exactly");
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        /// <summary>
        /// Compares the control and invested throws for one target label (null:
        /// every damage event): exactly one damage roll per creature in each,
        /// the flat bonus differing by the steps, and the final damage by the
        /// steps times the multiplier (whole, or halved on a passed save; a
        /// null halved flag accepts either as the throw decides).
        /// </summary>
        private static void FcbCompareBombThrow(JArray throws, string label, int steps, int multiplier, bool? halved,
            IList<string> failures, string name)
        {
            var control = (JArray)throws[0]["damage"];
            var invested = (JArray)throws[1]["damage"];
            Func<JArray, JObject[]> pick = events => events.OfType<JObject>().Where(value =>
                label == null || (string)value["target"] == label).ToArray();
            JObject[] controlEvents = pick(control), investedEvents = pick(invested);
            if (!(bool)throws[0]["completed"] || !(bool)throws[1]["completed"])
            {
                failures.Add(name + ": a throw did not complete");
                return;
            }
            if (label != null && (controlEvents.Length != 1 || investedEvents.Length != 1))
            {
                failures.Add(name + ": " + controlEvents.Length + " control and " + investedEvents.Length +
                    " invested damage rolls, expected exactly one each");
                return;
            }
            if (label == null && controlEvents.Length != investedEvents.Length)
            {
                failures.Add(name + ": the control and invested throws damaged different creatures");
                return;
            }
            if (label == null && controlEvents.Length == 0)
                return;
            for (int index = 0; index < controlEvents.Length; index++)
            {
                JObject before = controlEvents[index], after = investedEvents[index];
                int bonusDelta = (int)after["firstBonus"] - (int)before["firstBonus"];
                int damageDelta = (int)after["damage"] - (int)before["damage"];
                bool half = (bool)after["half"];
                if (bonusDelta != steps)
                    failures.Add(name + " (" + before["target"] + "): flat bonus delta " + bonusDelta + ", expected " + steps);
                if (halved.HasValue && half != halved.Value)
                    failures.Add(name + " (" + before["target"] + "): the save outcome was not " +
                        (halved.Value ? "passed" : "failed"));
                int whole = steps * multiplier;
                bool expected = half ? damageDelta == whole / 2 || damageDelta == (whole + 1) / 2 : damageDelta == whole;
                if (!expected)
                    failures.Add(name + " (" + before["target"] + "): damage delta " + damageDelta + " for " +
                        steps + " steps x" + multiplier + (half ? " halved" : ""));
            }
        }

        /// <summary>One seeded native throw of a bomb ability at a live target.</summary>
        private static JObject FcbThrow(UnitEntityData thrower, BlueprintAbility bomb, UnitEntityData target,
            FcbBombObserver observer, string mode, int seed, IDictionary<UnitEntityData, string> labels)
        {
            observer.AttackMode = mode;
            observer.Damage.Clear();
            foreach (UnitEntityData unit in labels.Keys) unit.Damage = 0;
            Projectile[] previous = Game.Instance.ProjectileController.Projectiles.ToArray();
            var observed = new List<Projectile>();
            var data = new AbilityData(bomb, thrower.Descriptor);
            AbilityExecutionContext context = data.CreateExecutionContext(new TargetWrapper(target));
            UnityEngine.Random.InitState(seed);
            var process = new AbilityExecutionProcess(context);
            bool completed;
            try
            {
                for (int tick = 0; !process.IsEnded && tick < 100; tick++)
                {
                    process.Tick();
                    foreach (Projectile projectile in Game.Instance.ProjectileController.Projectiles.Where(value =>
                        !previous.Contains(value) && !value.Cleared && ReferenceEquals(value.Launcher, thrower) &&
                        !observed.Contains(value)).ToArray())
                    {
                        observed.Add(projectile);
                        // Complete only the asynchronous transport; the native
                        // OnHit and the delivery and effects that follow run as is.
                        typeof(Projectile).GetProperty("IsHit").GetSetMethod(true).Invoke(projectile, new object[] { true });
                        projectile.OnHit();
                    }
                }
                completed = process.IsEnded;
            }
            finally
            {
                if (!process.IsEnded) process.Detach();
                foreach (Projectile projectile in observed) projectile.Cleared = true;
                Game.Instance.ProjectileController.Tick();
            }
            return new JObject
            {
                ["bomb"] = bomb.name,
                ["mode"] = mode,
                ["completed"] = completed,
                ["projectiles"] = observed.Count,
                ["damage"] = FcbDescribeDamage(observer.Damage, labels),
            };
        }

        private static JArray FcbDescribeDamage(IEnumerable<RuleDealDamage> events, IDictionary<UnitEntityData, string> labels)
        {
            var result = new JArray();
            foreach (RuleDealDamage damage in events)
            {
                string label;
                if (damage.Target == null || !labels.TryGetValue(damage.Target, out label))
                    label = damage.Target == null ? "none" : damage.Target.CharacterName;
                BaseDamage first = damage.DamageBundle == null ? null : damage.DamageBundle.First;
                result.Add(new JObject
                {
                    ["target"] = label,
                    ["source"] = damage.Reason == null || damage.Reason.Context == null ||
                        damage.Reason.Context.AssociatedBlueprint == null ? "none" :
                        damage.Reason.Context.AssociatedBlueprint.name,
                    ["firstBonus"] = first == null ? int.MinValue : first.Bonus,
                    ["chunks"] = damage.DamageBundle == null ? 0 : damage.DamageBundle.Count(),
                    ["half"] = damage.HalfBecauseSavingThrow,
                    ["critical"] = damage.AttackRoll != null && damage.AttackRoll.IsCriticalConfirmed,
                    ["damage"] = damage.Damage,
                });
            }
            return result;
        }
    }
}
