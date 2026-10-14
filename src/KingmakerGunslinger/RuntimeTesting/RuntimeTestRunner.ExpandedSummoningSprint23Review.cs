using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using TurnBased.Controllers;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 23's hidden-candidate native review. It reuses the qualified
    /// save-free first-area fixture and request-local clock from Sprint 22,
    /// but owns a separate allowlisted scenario, evidence document and exact
    /// cleanup path. No human save is selected or written.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private ExpandedSummoningCorrectionFixture _sprint23Fixture;
        private IEnumerator<int> _sprint23Steps;
        private Sprint22NativeClock _sprint23Clock;
        private readonly List<RuntimeTestAssertion> _sprint23Assertions =
            new List<RuntimeTestAssertion>();
        private readonly JArray _sprint23Rows = new JArray();

        private void Sprint23Check(string predicate, bool passed,
            JObject row, string expected)
        {
            row["predicate"] = predicate;
            row["passed"] = passed;
            row["runId"] = _request.RunId;
            _sprint23Rows.Add(row);
            _sprint23Assertions.Add(Assertion("sprint23-" + predicate,
                expected, row.ToString(Formatting.None), passed,
                "Request-owned Triceratops, Stegosaurus and controls in the save-free native first-area fixture."));
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                _request.EvidenceDirectory, "sprint23-review-progress.json"),
                BoundaryObservationPolicy.Serialize(_sprint23Rows));
        }

        private bool PollSprint23NativeReview()
        {
            try
            {
                if (_sprint23Steps == null)
                {
                    _sprint23Fixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_Sprint23_Caster");
                    _sprint23Clock = new Sprint22NativeClock(
                        _sprint23Fixture);
                    _sprint23Steps = ReviewSprint23(_sprint23Fixture)
                        .GetEnumerator();
                }
                _sprint23Clock.Pulse();
                if (_sprint23Steps.MoveNext()) return false;
            }
            catch (Exception error)
            {
                Sprint23Check("execution", false, new JObject {
                    ["unit"] = "armored-dinosaurs",
                    ["event"] = "native-review",
                    ["owner"] = _sprint23Fixture?.Caster?.UniqueId,
                    ["exception"] = error.ToString()
                }, "Native review completes without exception.");
            }
            CleanupSprint23NativeReview();
            string report = Path.Combine(_request.EvidenceDirectory,
                "sprint23-review.json");
            RuntimeTestResultWriter.WriteAtomic(report,
                BoundaryObservationPolicy.Serialize(_sprint23Rows));
            _summoningProfileResult = CreateResult(_sprint23Assertions.All(
                    value => value.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                _sprint23Assertions, null);
            _summoningProfileResult.EvidenceFiles.Add(report);
            return true;
        }

        private void CleanupSprint23NativeReview()
        {
            if (_sprint23Steps != null)
            {
                _sprint23Steps.Dispose();
                _sprint23Steps = null;
            }
            if (_sprint23Fixture == null)
            {
                if (_sprint23Clock != null)
                {
                    _sprint23Clock.Dispose();
                    _sprint23Clock = null;
                }
                return;
            }
            bool cleaned = false, clockRestored = false;
            int pulses = _sprint23Clock == null ? 0 :
                _sprint23Clock.Pulses;
            int movementTicks = _sprint23Clock == null ? 0 :
                _sprint23Clock.MovementTicks;
            double advanced = _sprint23Clock == null ? 0d :
                _sprint23Clock.AdvancedSeconds;
            try
            {
                EndExpandedSummoningCorrectionFixture(_sprint23Fixture,
                    out cleaned);
            }
            finally
            {
                if (_sprint23Clock != null)
                {
                    _sprint23Clock.Dispose();
                    clockRestored = _sprint23Clock.Restored;
                    _sprint23Clock = null;
                }
            }
            Sprint23Check("fixture-restored", cleaned && clockRestored,
                new JObject {
                    ["unit"] = "armored-dinosaurs",
                    ["event"] = "exact-owned-teardown-and-clock-restore",
                    ["referencesRestored"] = cleaned,
                    ["clockRestored"] = clockRestored,
                    ["nativePulses"] = pulses,
                    ["nativeMovementTicks"] = movementTicks,
                    ["nativeAdvancedSeconds"] = advanced
                }, "Exact original unit/party/area references and request-local GameTime are restored.");
            _sprint23Fixture = null;
        }

        private IEnumerable<int> ReviewSprint23(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool originalMode = Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                .EnableTurnBasedMode.CurrentValue;
            bool originalPause = Game.Instance.IsPaused;
            string reviewPhase = RuntimeTestRequestParser
                .Sprint23ReviewPhase(_request);
            if (reviewPhase == null)
                throw new InvalidOperationException(
                    "Sprint 23 native review phase was not validated.");
            bool foundation = reviewPhase == "rtwp-foundation";
            bool chargeRounding = reviewPhase == "charge-rounding-rtwp" ||
                reviewPhase == "charge-rounding-turn-based";
            bool turnBased = reviewPhase == "turn-based-control" ||
                reviewPhase == "charge-rounding-turn-based";
            try
            {
                Sprint23Check("review-phase-" + reviewPhase, true,
                    new JObject {
                        ["unit"] = "armored-dinosaurs",
                        ["event"] = "bounded-native-review-selection",
                        ["reviewPhase"] = reviewPhase,
                        ["combatMode"] = turnBased ? "turn-based" : "rtwp"
                    }, "The exact allowlisted bounded review phase is recorded.");
                Game.Instance.IsPaused = true;
                if (foundation) Sprint23RegisteredRoutes(fixture);
                CreateExpandedSummoningCorrectionHostile(fixture);

                Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                    .EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;

                if (chargeRounding)
                {
                    foreach (int frame in Sprint23ChargeRoundingReview(
                        fixture, turnBased)) yield return frame;
                    yield break;
                }

                if (foundation)
                    foreach (int frame in Sprint23VisualLifecycle(fixture))
                        yield return frame;

                foreach (string key in new[] {
                    ArmoredDinosaurRulesPolicy.TriceratopsKey,
                    ArmoredDinosaurRulesPolicy.StegosaurusKey })
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, key);
                    SetExpandedSummoningBrainActive(owner, false);
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    if (turnBased)
                    {
                        var admission = new JObject();
                        foreach (int frame in Sprint22EnrollTurnBasedOwners(
                            new[] { owner }, fixture.Hostile, admission))
                            yield return frame;
                        Sprint23Check("turn-order-" + key,
                            owner.IsInCombat && fixture.Hostile.IsInCombat &&
                            CombatController.IsInTurnBasedCombat() &&
                            (bool)admission["admitted"], admission,
                            "The exact request-owned dinosaur and hostile enter native turn order.");
                    }
                    foreach (int frame in Sprint22SetPowerAttack(owner,
                        false)) yield return frame;
                    Sprint23ReadProfile(fixture, owner, key, turnBased);
                    Sprint23ReadBody(owner, key, turnBased);
                    foreach (int frame in Sprint23PowerAttack(owner, key,
                        turnBased)) yield return frame;
                    foreach (int frame in Sprint23NativeOrdinaryAttack(
                        fixture, owner, key, turnBased)) yield return frame;
                    bool ownerAvailable = owner != null &&
                        !owner.Destroyed && owner.IsInState &&
                        owner.Descriptor != null &&
                        owner.Descriptor.State != null &&
                        !owner.Descriptor.State.IsDead &&
                        owner.Descriptor.ActivatableAbilities.Enumerable.Any(
                            value => value.Blueprint.AssetGuid ==
                                "a7b339e4f6ff93a4697df5d7a87ff619");
                    if (!ownerAvailable)
                    {
                        Sprint23Check("ordinary-source-lifecycle-" + key +
                            "-" + (turnBased ? "turn-based" : "rtwp"),
                            false, new JObject {
                                ["unit"] = owner?.UniqueId,
                                ["event"] =
                                    "source-unavailable-after-native-command",
                                ["destroyed"] = owner == null ||
                                    owner.Destroyed,
                                ["inState"] = owner != null &&
                                    owner.IsInState,
                                ["dead"] = owner == null ||
                                    owner.Descriptor == null ||
                                    owner.Descriptor.State == null ||
                                    owner.Descriptor.State.IsDead,
                                ["abilityCount"] = owner == null ||
                                    owner.Descriptor == null ? -1 :
                                    owner.Descriptor.Abilities.Enumerable.Count()
                            },
                            "The request-owned summon remains live after its bounded native ordinary command.");
                        foreach (int frame in
                            Sprint23LeaveTurnBasedBeforeDisposal(turnBased))
                            yield return frame;
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                        continue;
                    }
                    if (key == ArmoredDinosaurRulesPolicy.TriceratopsKey)
                    {
                        // A real attack can restart Kingmaker's attack-mode
                        // toggle even after the isolated off/on arithmetic
                        // probe.  Reapply the already-qualified forced-stop
                        // sequence so charge is measured without Power Attack;
                        // its own off/on proof remains the preceding assertion.
                        foreach (int frame in Sprint22SetPowerAttack(owner,
                            false)) yield return frame;
                        Sprint23PowerfulChargeRules(fixture, owner,
                            turnBased);
                        if (foundation)
                            Sprint23TrampleMatrix(fixture, owner);
                    }
                    else Sprint23TailTripMatrix(fixture, owner,
                        turnBased);
                    foreach (int frame in
                        Sprint23LeaveTurnBasedBeforeDisposal(turnBased))
                        yield return frame;
                    DisposeExpandedSummoningUnits(fixture.Created,
                        new[] { owner });
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                }

                foreach (int frame in Sprint23NativeChargeCommand(fixture,
                    turnBased)) yield return frame;
                foreach (int frame in Sprint23NativeTrampleCommand(fixture,
                    turnBased)) yield return frame;

                if (foundation)
                {
                    string control;
                    bool rhino = ExerciseExpandedSummoningRegisteredRhinoCharge(
                        fixture, out control);
                    Sprint23Check("released-rhino-charge-control", rhino,
                        new JObject { ["unit"] = "rhinoceros-controls",
                            ["event"] = "released-powerful-charge",
                            ["detail"] = control },
                        "Released Rhinoceros and Woolly Rhinoceros charge arithmetic remains unchanged.");
                    Sprint23AnkylosaurusControl(fixture);
                }
            }
            finally
            {
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                    .EnableTurnBasedMode.CurrentValue = originalMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = originalPause;
            }
        }

        /// <summary>
        /// Bounded follow-up for the released Powerful Charge rounding defect.
        /// It reuses the qualified Sprint 23 world, movement and command seams
        /// but deliberately omits art, trample, trip, routes and persistence.
        /// </summary>
        private IEnumerable<int> Sprint23ChargeRoundingReview(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            UnitEntityData owner = CastExpandedSummoningOwnTier(fixture,
                ArmoredDinosaurRulesPolicy.TriceratopsKey);
            SetExpandedSummoningBrainActive(owner, false);
            for (int frame = 0; frame < 12; frame++) yield return 0;
            try
            {
                if (turnBased)
                {
                    var admission = new JObject();
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(
                        new[] { owner }, fixture.Hostile, admission))
                        yield return frame;
                    Sprint23Check("charge-rounding-turn-order",
                        owner.IsInCombat && fixture.Hostile.IsInCombat &&
                        CombatController.IsInTurnBasedCombat() &&
                        (bool)admission["admitted"], admission,
                        "The request-owned Triceratops and hostile enter native turn order.");
                }
                foreach (int frame in Sprint22SetPowerAttack(owner, false))
                    yield return frame;
                foreach (int frame in Sprint23PowerAttack(owner,
                    ArmoredDinosaurRulesPolicy.TriceratopsKey, turnBased))
                    yield return frame;
                foreach (int frame in Sprint23NativeOrdinaryAttack(fixture,
                    owner, ArmoredDinosaurRulesPolicy.TriceratopsKey,
                    turnBased)) yield return frame;
                foreach (int frame in Sprint22SetPowerAttack(owner, false))
                    yield return frame;
                Sprint23PowerfulChargeRules(fixture, owner, turnBased);
                foreach (int frame in Sprint23PowerAttackChargeControl(
                    fixture, owner, turnBased)) yield return frame;
                foreach (int frame in Sprint22SetPowerAttack(owner, false))
                    yield return frame;
                Sprint23CriticalChargeControl(fixture, owner, turnBased);
                foreach (int frame in Sprint23LeaveTurnBasedBeforeDisposal(
                    turnBased)) yield return frame;
            }
            finally
            {
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner });
            }

            foreach (int strength in new[] { 24, 26, 28, 30 })
                foreach (int frame in Sprint23NativeChargeCommand(fixture,
                    turnBased, strength)) yield return frame;
            if (!turnBased)
            {
                string control;
                bool rhino = ExerciseExpandedSummoningRegisteredRhinoCharge(
                    fixture, out control);
                Sprint23Check("charge-rounding-released-rhino-control", rhino,
                    new JObject { ["unit"] = "rhinoceros-controls",
                        ["event"] = "released-fixed-powerful-charge",
                        ["detail"] = control },
                    "Released Rhinoceros and Woolly Rhinoceros fixed charge increments remain unchanged.");
            }
        }

        /// <summary>
        /// A completed native command can leave its summon as the active
        /// turn owner.  Retire turn-based mode through the same supported
        /// setting/controller toggle used by Sprint 22 before destroying
        /// that request-owned actor.  The following RTWP pulse proves the
        /// controller is no longer advancing a stale TurnController.Unit.
        /// The next trial independently rebuilds turn order through
        /// Sprint22EnrollTurnBasedOwners.
        /// </summary>
        private IEnumerable<int> Sprint23LeaveTurnBasedBeforeDisposal(
            bool turnBased)
        {
            if (!turnBased) yield break;
            Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                .EnableTurnBasedMode.CurrentValue = false;
            Game.Instance.TurnBasedCombatController.Activate();
            if (CombatController.IsInTurnBasedCombat())
                throw new InvalidOperationException(
                    "Sprint 23 could not retire the active native turn " +
                    "before owned summon disposal.");
            yield return 0;
            if (CombatController.IsInTurnBasedCombat())
                throw new InvalidOperationException(
                    "Sprint 23 turn-based retirement did not remain stable " +
                    "through its native settle pulse.");
        }

        private void Sprint23RegisteredRoutes(
            ExpandedSummoningCorrectionFixture fixture)
        {
            SummonVariantSpec[] routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly))
                .Where(value => ArmoredDinosaurVisualPolicy.Handles(
                    value.Creature.Key)).ToArray();
            Sprint23Check("route-census", routes.Length == 12 &&
                routes.Count(value => value.Creature.Key ==
                    ArmoredDinosaurRulesPolicy.TriceratopsKey) == 8 &&
                routes.Count(value => value.Creature.Key ==
                    ArmoredDinosaurRulesPolicy.StegosaurusKey) == 4 &&
                !routes.Any(value => value.Creature.Key ==
                    ArmoredDinosaurRulesPolicy.StegosaurusKey &&
                    value.Family == SummonFamily.Monster) &&
                routes.All(SummonVisibilityCatalog.IsPublished),
                new JObject { ["unit"] = "armored-dinosaurs",
                    ["event"] = "public-route-census",
                    ["publishedRoutes"] = routes.Count(
                        SummonVisibilityCatalog.IsPublished),
                    ["routes"] = new JArray(routes.Select(value =>
                        value.StableKey)) },
                "Exactly eight Triceratops and four Stegosaurus roots are published, with no Stegosaurus Monster route.");
            foreach (SummonVariantSpec route in routes)
            {
                UnitEntityData[] units = CastExpandedSummoningVariant(
                    fixture.Blueprints, fixture.Caster, route, null,
                    fixture.Evidence);
                fixture.Created.AddRange(units);
                int low = route.Multiplicity == SummonMultiplicity.One ? 1 :
                    route.Multiplicity == SummonMultiplicity.OneD3 ? 1 : 2;
                int high = route.Multiplicity == SummonMultiplicity.One ? 1 :
                    route.Multiplicity == SummonMultiplicity.OneD3 ? 3 : 5;
                BlueprintUnit expected = fixture.Blueprints
                    .OfType<BlueprintUnit>().Single(value => value.name ==
                        "KMG_Summoning_Unit_" +
                        (route.Creature.Key ==
                            ArmoredDinosaurRulesPolicy.TriceratopsKey
                            ? "Triceratops" : "Stegosaurus"));
                bool exact = units.Length >= low && units.Length <= high &&
                    units.All(value => ReferenceEquals(value.Blueprint,
                        expected));
                Sprint23Check("route-" + route.StableKey, exact,
                    new JObject { ["unit"] = route.Creature.Key,
                        ["event"] = "published-ability-execution",
                        ["route"] = route.StableKey,
                        ["execution"] = _expandedSummoningLastAbilityExecution,
                        ["quantity"] = units.Length,
                        ["unitReadback"] = new JArray(units.Select(
                            Sprint22UnitReadback)) },
                    "The published root executes only its exact unit identity in the legal quantity band.");
                DisposeExpandedSummoningUnits(fixture.Created, units);
            }
        }

        private void Sprint23ReadProfile(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key, bool turnBased)
        {
            bool tri = key == ArmoredDinosaurRulesPolicy.TriceratopsKey;
            ModifiableValue[] abilities = {
                owner.Stats.Strength, owner.Stats.Dexterity,
                owner.Stats.Constitution, owner.Stats.Intelligence,
                owner.Stats.Wisdom, owner.Stats.Charisma };
            int[] scores = abilities.Select(value => value.ModifiedValue)
                .ToArray();
            int[] expectedScores = tri ? new[] { 26, 9, 19, 2, 12, 7 } :
                new[] { 27, 14, 17, 2, 13, 10 };
            int difficulty = owner.Stats.AC.Modifiers.Where(value =>
                value.ModDescriptor == ModifierDescriptor.Difficulty)
                .Sum(value => value.ModValue);
            Sprint14CmdBreakdown cmd = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.Grapple);
            Sprint14CmdBreakdown trip = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.Trip);
            // CMD loses only a positive Dexterity contribution while denied
            // Dexterity.  A negative Dexterity modifier remains part of the
            // native result; adding it again made the Triceratops readback
            // report a false mismatch even though native CMD was exactly 29.
            int deniedDex = cmd.DexterityDenied || cmd.FlatFooted ?
                Math.Max(0, owner.Stats.Dexterity.Bonus) : 0;
            int cmb = Rulebook.Trigger(new RuleCalculateCMB(owner,
                fixture.Hostile, CombatManeuver.Trip)).Result;
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            RuleCalculateWeaponStats weaponStats = limbs.Length == 1 ?
                Rulebook.Trigger(new RuleCalculateWeaponStats(owner,
                    limbs[0], null)) : null;
            BaseDamage damage = weaponStats == null ? null :
                weaponStats.DamageDescription[0].CreateDamage();
            int attack = limbs.Length == 1 ? Rulebook.Trigger(
                new RuleCalculateAttackBonusWithoutTarget(owner, limbs[0],
                    0)).Result : int.MinValue;
            string[] facts = owner.Descriptor.Progression.Features.Enumerable
                .Select(value => value.Blueprint.name).ToArray();
            string[] actions = owner.Blueprint.Brain == null ||
                owner.Blueprint.Brain.Actions == null ? new string[0] :
                owner.Blueprint.Brain.Actions.Select(value =>
                    value.GetType().FullName + ":" + value.name).ToArray();
            bool ordinaryFallback = actions.Any(value => value.StartsWith(
                "Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAttack:",
                StringComparison.Ordinal));
            string[] grantedAbilities = owner.Descriptor.Abilities.Enumerable
                .Select(value => value.Blueprint.name).ToArray();
            bool exact = scores.SequenceEqual(expectedScores) &&
                owner.Descriptor.State.Size == Size.Huge &&
                owner.Descriptor.Progression.CharacterLevel ==
                    (tri ? 14 : 12) &&
                owner.Stats.HitPoints.ModifiedValue == (tri ? 119 : 90) &&
                owner.Stats.AC.ModifiedValue - difficulty == (tri ? 21 : 22) &&
                owner.Stats.AC.Touch - difficulty == (tri ? 7 : 10) &&
                owner.Stats.AC.FlatFooted - difficulty == (tri ? 21 : 20) &&
                owner.Stats.SaveFortitude.ModifiedValue == (tri ? 15 : 13) &&
                owner.Stats.SaveReflex.ModifiedValue == (tri ? 8 : 10) &&
                owner.Stats.SaveWill.ModifiedValue == 5 &&
                owner.Stats.SkillPerception.ModifiedValue == (tri ? 24 : 16) &&
                owner.Stats.BaseAttackBonus.ModifiedValue == (tri ? 10 : 9) &&
                cmb == (tri ? 20 : 19) &&
                cmd.Result + deniedDex - difficulty == (tri ? 29 : 31) &&
                trip.Result - cmd.Result == 4 &&
                owner.Stats.Reach.ModifiedValue == 15 &&
                owner.Stats.Speed.ModifiedValue == 30 && limbs.Length == 1 &&
                attack == (tri ? 17 : 16) && damage != null &&
                damage.Dice.Rolls == (tri ? 2 : 4) &&
                (int)damage.Dice.Dice == (tri ? 10 : 6) &&
                damage.Bonus == 12 && !weaponStats.SecondaryWeapon &&
                Math.Abs(limbs[0].AttackRange.Meters - 4.572f) < .002f &&
                ordinaryFallback &&
                (tri ? weaponStats.DoubleCriticalEdge :
                    facts.Contains("CleaveFeature"));
            exact &= facts.Count(value => value == (tri ?
                "KMG_Summoning_Natural_Triceratops_Chassis" :
                "KMG_Summoning_Natural_Stegosaurus_Chassis")) == 1;
            exact &= !facts.Any(value => value != null &&
                (value.Contains("AnkylosaurusStun") ||
                 value.Contains("Ankylosaurus_Chassis"))) || tri;
            Sprint23Check("profile-" + key + "-" +
                (turnBased ? "turn-based" : "rtwp"), exact,
                new JObject { ["unit"] = owner.UniqueId,
                    ["event"] = "native-profile-readback",
                    ["owner"] = owner.Blueprint.AssetGuid,
                    ["scores"] = new JArray(scores),
                    ["hd"] = owner.Descriptor.Progression.CharacterLevel,
                    ["hp"] = owner.Stats.HitPoints.ModifiedValue,
                    ["acRaw"] = owner.Stats.AC.ModifiedValue,
                    ["touchRaw"] = owner.Stats.AC.Touch,
                    ["flatRaw"] = owner.Stats.AC.FlatFooted,
                    ["difficultyAcContribution"] = difficulty,
                    ["fortitude"] = owner.Stats.SaveFortitude.ModifiedValue,
                    ["reflex"] = owner.Stats.SaveReflex.ModifiedValue,
                    ["will"] = owner.Stats.SaveWill.ModifiedValue,
                    ["perception"] = DescribeSprint16Skill(
                        owner.Stats.SkillPerception),
                    ["bab"] = owner.Stats.BaseAttackBonus.ModifiedValue,
                    ["cmb"] = cmb, ["cmd"] = cmd.Describe("CMD"),
                    ["tripCmd"] = trip.Describe("trip"),
                    ["deniedPositiveDexRestoration"] = deniedDex,
                    ["reachFeet"] = owner.Stats.Reach.ModifiedValue,
                    ["speedFeet"] = owner.Stats.Speed.ModifiedValue,
                    ["weaponRangeMeters"] = limbs.Length == 1 ?
                        limbs[0].AttackRange.Meters : -1f,
                    ["attack"] = attack,
                    ["damage"] = damage == null ? null :
                        Sprint16DamageLine(damage),
                    ["improvedCritical"] = weaponStats == null ? false :
                        weaponStats.DoubleCriticalEdge,
                    ["facts"] = new JArray(facts),
                    ["brain"] = owner.Blueprint.Brain?.name,
                    ["brainActions"] = new JArray(actions),
                    ["grantedAbilities"] = new JArray(grantedAbilities),
                    ["ordinaryFallback"] = ordinaryFallback },
                "The live adult profile matches every printed score, HD/hp, defense, save, Perception, attack, reach and quadruped trip-defense value, with the bounded native natural-attack AI fallback intact.");
        }

        private void Sprint23ReadBody(UnitEntityData owner, string key,
            bool turnBased)
        {
            Mesh cacheMesh; string[] bones; Texture2D albedo; string status;
            bool loaded = PteranodonAssetRuntime.TryGetArmoredDinosaurVisual(
                key, out cacheMesh, out bones, out albedo, out status);
            string expectedName = "KMG_" + key + "_Original";
            SkinnedMeshRenderer[] skins = owner.View == null ?
                new SkinnedMeshRenderer[0] : owner.View
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true);
            SkinnedMeshRenderer[] original = skins.Where(value =>
                value.sharedMesh != null && value.sharedMesh.name ==
                    expectedName).ToArray();
            JObject selection = Sprint22SelectionSurface(owner);
            bool exact = loaded && original.Length == 1 &&
                original[0].enabled && original[0].bones.Length ==
                    (key == ArmoredDinosaurRulesPolicy.TriceratopsKey
                        ? 26 : 24) && original[0].sharedMesh != cacheMesh &&
                original[0].sharedMaterial != null &&
                ReferenceEquals(original[0].sharedMaterial.mainTexture,
                    albedo) && (bool)selection["selectionSurface"] &&
                owner.Descriptor.State.Size == Size.Huge;
            string frame = original.Length == 1 ?
                CaptureSprint17OriginalBodyFrame(owner, original[0],
                    "sprint23-" + key + "-" +
                    (turnBased ? "turn-based" : "rtwp")) :
                "no-original-renderer";
            Sprint23Check("body-" + key + "-" +
                (turnBased ? "turn-based" : "rtwp"), exact,
                new JObject { ["unit"] = owner.UniqueId,
                    ["event"] = "owned-original-body-bind",
                    ["owner"] = owner.Blueprint.AssetGuid,
                    ["status"] = status, ["supportingFrame"] = frame,
                    ["selection"] = selection,
                    ["skins"] = new JArray(skins.Select(value =>
                        new JObject { ["name"] = value.name,
                            ["enabled"] = value.enabled,
                            ["mesh"] = value.sharedMesh?.name,
                            ["meshId"] = value.sharedMesh == null ? 0 :
                                value.sharedMesh.GetInstanceID(),
                            ["materialId"] = value.sharedMaterial == null ? 0 :
                                value.sharedMaterial.GetInstanceID(),
                            ["bones"] = value.bones == null ? 0 :
                                value.bones.Length })) },
                "One private original body binds to the exact donor drivers, retains Huge selection/collision, and borrows only the immutable painting cache.");
        }

        private IEnumerable<int> Sprint23VisualLifecycle(
            ExpandedSummoningCorrectionFixture fixture)
        {
            foreach (string key in ArmoredDinosaurVisualPolicy.Keys)
            {
                BlueprintUnit donorBlueprint = fixture.Blueprints
                    .OfType<BlueprintUnit>().Single(value =>
                        value.AssetGuid == ArmoredDinosaurVisualPolicy
                            .DonorGuidFor(key));
                UnitEntityData control = Game.Instance.EntityCreator.SpawnUnit(
                    donorBlueprint, fixture.Caster.Position,
                    Quaternion.identity, fixture.Scene);
                fixture.Created.Add(control);
                SetExpandedSummoningBrainActive(control, false);
                Mesh[] donorMeshes = control.View
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Select(value => value.sharedMesh).ToArray();
                try
                {
                    for (int cycle = 0; cycle < 3; cycle++)
                    {
                        int faultReached = 0;
                        UnitEntityData unit;
                        try
                        {
                            if (cycle == 0)
                                ExpandedSummoningPteranodonViewPatch
                                    .PostSuppressionFaultForTest = () => {
                                        faultReached++;
                                        throw new InvalidOperationException(
                                            "Sprint23 request-owned post-swap fault");
                                    };
                            unit = CastExpandedSummoningOwnTier(fixture, key);
                        }
                        finally
                        {
                            ExpandedSummoningPteranodonViewPatch
                                .PostSuppressionFaultForTest = null;
                        }
                        SetExpandedSummoningBrainActive(unit, false);
                        for (int frame = 0; frame < 30; frame++)
                            yield return 0;
                        var view = unit.View;
                        SkinnedMeshRenderer[] skins = view
                            .GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        string expectedName = "KMG_" + key + "_Original";
                        bool original = skins.Any(value => value.enabled &&
                            value.sharedMesh != null &&
                            value.sharedMesh.name == expectedName);
                        UnityEngine.Object[] owned =
                            ExpandedSummoningPteranodonViewPatch
                                .CaptureArmoredDinosaurOwnedResources(view);
                        string outcome = ExpandedSummoningPteranodonViewPatch
                            .DescribeView(view);
                        bool donorRestored = skins.Any(value => value.enabled &&
                            donorMeshes.Contains(value.sharedMesh));
                        bool bound = cycle == 0
                            ? faultReached == 1 && !original && donorRestored &&
                                owned.Length == 0 && outcome.StartsWith(
                                    "donor-visual:attach-failed:",
                                    StringComparison.Ordinal)
                            : faultReached == 0 && original &&
                                outcome.StartsWith("visual:attached;",
                                    StringComparison.Ordinal) &&
                                owned.OfType<Mesh>().Count() == 1 &&
                                owned.OfType<Material>().Any() &&
                                EntityFadedIn(unit);
                        Sprint23Check("view-" + key + "-cycle-" + cycle,
                            bound, new JObject {
                                ["unit"] = unit.UniqueId,
                                ["event"] =
                                    "fault-rollback-recovery-and-native-fader",
                                ["owner"] = unit.Blueprint.AssetGuid,
                                ["faultReached"] = faultReached,
                                ["original"] = original,
                                ["donorRestored"] = donorRestored,
                                ["fadedIn"] = EntityFadedIn(unit),
                                ["status"] = outcome,
                                ["owned"] = new JArray(owned.Select(value =>
                                    new JObject {
                                        ["id"] = value.GetInstanceID(),
                                        ["name"] = value.name,
                                        ["type"] = value.GetType().FullName }))
                            },
                            "A request-local post-swap fault restores the exact native donor; two subsequent bodies bind private owned resources through the native fader.");

                        Mesh cachedMesh; Texture2D cachedPaint;
                        string[] cachedBones; string cacheStatus;
                        bool cacheLoaded = PteranodonAssetRuntime
                            .TryGetArmoredDinosaurVisual(key, out cachedMesh,
                                out cachedBones, out cachedPaint,
                                out cacheStatus);
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { unit });
                        int waited = 0;
                        for (; waited < 60; waited++)
                        {
                            Game.Instance.EntityDestroyer.Tick();
                            yield return 0;
                            if (unit.Destroyed && unit.View == null &&
                                unit.HoldingState == null &&
                                owned.All(value => value == null)) break;
                        }
                        bool released = unit.Destroyed && unit.View == null &&
                            unit.HoldingState == null &&
                            owned.All(value => value == null);
                        bool borrowedSurvive = cacheLoaded &&
                            cachedMesh != null && cachedPaint != null &&
                            donorMeshes.All(value => value != null);
                        ExpandedSummoningPteranodonViewPatch
                            .ReleasePhase2View(view);
                        Sprint23Check("view-" + key + "-teardown-" + cycle,
                            released && borrowedSurvive, new JObject {
                                ["unit"] = unit.UniqueId,
                                ["event"] =
                                    "native-OnDestroy-private-resource-release",
                                ["owner"] = "exact-per-view-resources",
                                ["waitedFrames"] = waited,
                                ["released"] = released,
                                ["borrowedSurvive"] = borrowedSurvive,
                                ["cacheStatus"] = cacheStatus,
                                ["cacheMeshId"] = cachedMesh == null ? 0 :
                                    cachedMesh.GetInstanceID(),
                                ["cachePaintId"] = cachedPaint == null ? 0 :
                                    cachedPaint.GetInstanceID()
                            },
                            "Native destruction releases only the captured private mesh/material family and preserves the donor and immutable mesh/painting caches; repeat release is harmless.");
                    }
                    bool isolated = control.View != null &&
                        ExpandedSummoningPteranodonViewPatch.DescribeView(
                            control.View) == "not-attempted" &&
                        donorMeshes.SequenceEqual(control.View
                            .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                            .Select(value => value.sharedMesh)) &&
                        donorMeshes.All(value => value != null);
                    Sprint23Check("native-donor-isolation-" + key, isolated,
                        new JObject { ["unit"] = control.UniqueId,
                            ["event"] = "donor-negative-control",
                            ["owner"] = donorBlueprint.AssetGuid,
                            ["unchanged"] = isolated },
                        "The native donor retains its exact renderer/mesh references across fault and recovered original-body instances.");
                }
                finally
                {
                    ExpandedSummoningPteranodonViewPatch
                        .PostSuppressionFaultForTest = null;
                    DisposeExpandedSummoningUnits(fixture.Created,
                        new[] { control });
                }
            }
        }

        private IEnumerable<int> Sprint23PowerAttack(UnitEntityData owner,
            string key, bool turnBased)
        {
            var toggle = owner.Descriptor.ActivatableAbilities.Enumerable
                .Single(value => value.Blueprint.AssetGuid ==
                    "a7b339e4f6ff93a4697df5d7a87ff619");
            ItemEntityWeapon weapon = Sprint19Limbs(owner).Single();
            foreach (int frame in Sprint22SetPowerAttack(owner, false))
                yield return frame;
            int offAttack = Rulebook.Trigger(
                new RuleCalculateAttackBonusWithoutTarget(owner, weapon,
                    0)).Result;
            int offDamage = Rulebook.Trigger(new RuleCalculateWeaponStats(
                owner, weapon, null)).DamageDescription[0].CreateDamage()
                .Bonus;
            foreach (int frame in Sprint22SetPowerAttack(owner, true))
                yield return frame;
            int onAttack = Rulebook.Trigger(
                new RuleCalculateAttackBonusWithoutTarget(owner, weapon,
                    0)).Result;
            int onDamage = Rulebook.Trigger(new RuleCalculateWeaponStats(
                owner, weapon, null)).DamageDescription[0].CreateDamage()
                .Bonus;
            bool exact = toggle.IsOn && toggle.IsRunning &&
                offAttack == (key ==
                    ArmoredDinosaurRulesPolicy.TriceratopsKey ? 17 : 16) &&
                onAttack == offAttack - 3 && offDamage == 12 &&
                onDamage == 21;
            Sprint23Check("power-attack-" + key + "-" +
                (turnBased ? "turn-based" : "rtwp"), exact,
                new JObject { ["unit"] = owner.UniqueId,
                    ["event"] = "native-power-attack-toggle",
                    ["offAttack"] = offAttack, ["onAttack"] = onAttack,
                    ["offDamage"] = offDamage,
                    ["onDamage"] = onDamage,
                    ["state"] = Sprint22PowerAttackState(owner, toggle) },
                "At BAB +9/+10 native Power Attack applies -3 and the exact lone-primary +9 damage without Sprint 22's +2 compensation.");
            foreach (int frame in Sprint22SetPowerAttack(owner, false))
                yield return frame;
        }

        private IEnumerable<int> Sprint23NativeOrdinaryAttack(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            target.Descriptor.State.RemoveConditionAll(UnitCondition.Prone);
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int cmbBefore = owner.Stats.AdditionalCMB.BaseValue;
            int damageBefore = target.Descriptor.Damage;
            ItemEntityWeapon weapon = Sprint19Limbs(owner).Single();
            // Summon placement can begin above the first-area terrain while
            // the view settles.  The released Sprint 22 command fixture pins
            // both request-owned actors to the measured ground before issuing
            // its native command; reuse that qualified setup so a fall cannot
            // interrupt the damage cue after a successful attack roll.
            // The art survey requires a broad empty presentation stage and can
            // legitimately reject the crowded post-route fixture.  The Sprint
            // 22 combat cells instead use this bounded caster-relative ground
            // pair, which remains available after route and visual review.
            Vector3 floor = fixture.Caster.Position + Vector3.right * 4f;
            Vector3 targetPoint = floor + Vector3.forward * 3f;
            PlaceExpandedSummoningUnit(owner, floor);
            PlaceExpandedSummoningUnit(target, targetPoint);
            Vector3 ownerStart = owner.Position;
            Vector3 targetStart = target.Position;
            float distanceBefore = owner.DistanceTo(target);
            var observer = new Sprint16RuleObserver {
                Owner = owner, Target = target };
            UnitAttack command = null;
            EventBus.Subscribe(observer);
            try
            {
                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                owner.Stats.AdditionalCMB.BaseValue = 100;
                foreach (int frame in Sprint18RunFullAttack(fixture,
                    owner, value => command = value)) yield return frame;
                RuleAttackRoll[] exactRolls = observer.Attacks.Where(value =>
                    value.Weapon != null && ReferenceEquals(
                        value.Weapon.Blueprint, weapon.Blueprint)).ToArray();
                RuleCombatManeuver[] trips = observer.Checks.Where(value =>
                    value.Type == CombatManeuver.Trip &&
                    ReferenceEquals(value.Initiator, owner) &&
                    ReferenceEquals(value.Target, target)).ToArray();
                bool tri = key == ArmoredDinosaurRulesPolicy.TriceratopsKey;
                bool exact = command != null && command.IsFinished &&
                    exactRolls.Length == 1 && exactRolls[0].IsHit &&
                    target.Descriptor.Damage > damageBefore &&
                    (tri ? trips.Length == 0 : trips.Length == 1) &&
                    !owner.Descriptor.HasFact(
                        BlueprintRoot.Instance.SystemMechanics.ChargeBuff);
                Sprint23Check("ordinary-command-" + key + "-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject { ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "real-native-full-attack-command",
                        ["commandFinished"] = command?.IsFinished,
                        ["commandResult"] = command?.Result.ToString(),
                        ["commandShouldInterrupt"] = command == null ?
                            (JToken)JValue.CreateNull() :
                            command.ShouldBeInterrupted,
                        ["plannedAttacks"] = command == null ? -1 :
                            command.AllAttacks.Count,
                        ["plannedAttackPresent"] = command != null &&
                            command.PlannedAttack != null,
                        ["executedAttackPresent"] = command != null &&
                            command.LastExecutedAttack != null,
                        ["attackRules"] = observer.Attacks.Count,
                        ["exactWeaponRolls"] = exactRolls.Length,
                        ["exactRollTargets"] = new JArray(exactRolls.Select(
                            value => value.Target?.UniqueId)),
                        ["weaponAttackRules"] = observer.WeaponAttacks,
                        ["damageRules"] = observer.Damage.Count,
                        ["damageEvents"] = new JArray(observer.Damage.Select(
                            Sprint16DamageEvent)),
                        ["tripRules"] = trips.Length,
                        ["damage"] = target.Descriptor.Damage - damageBefore,
                        ["groundProbe"] =
                            "released-sprint22-caster-relative-combat-pair",
                        ["ownerStart"] = SurveyVector(ownerStart),
                        ["ownerEnd"] = SurveyVector(owner.Position),
                        ["targetStart"] = SurveyVector(targetStart),
                        ["targetEnd"] = SurveyVector(target.Position),
                        ["distanceBefore"] = distanceBefore,
                        ["distanceAfter"] = owner.DistanceTo(target),
                        ["weapon"] = weapon.Blueprint?.AssetGuid,
                        ["weaponType"] = weapon.Blueprint?.Type?.AssetGuid,
                        ["weaponRangeMeters"] = weapon.AttackRange.Meters,
                        ["weaponStyle"] = weapon.GetAnimationStyle()
                            .ToString(),
                        ["ownerDestroyed"] = owner.Destroyed,
                        ["ownerInState"] = owner.IsInState,
                        ["ownerDead"] = owner.Descriptor.State.IsDead,
                        ["standardCooldown"] = owner.CombatState.Cooldown
                            .StandardAction,
                        ["moveCooldown"] = owner.CombatState.Cooldown
                            .MoveAction },
                    "A real native command spends its action and resolves exactly one ordinary gore or spiked-tail strike; only the Stegosaurus starts one native Trip.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                owner.Stats.AdditionalCMB.BaseValue = cmbBefore;
                target.Descriptor.State.RemoveConditionAll(
                    UnitCondition.Prone);
                target.Descriptor.Damage = damageBefore;
            }
        }

        private void Sprint23PowerfulChargeRules(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            ItemEntityWeapon gore = Sprint19Limbs(owner).Single();
            BlueprintBuff marker = BlueprintRoot.Instance.SystemMechanics
                .ChargeBuff;
            int strengthBefore = owner.Stats.Strength.BaseValue;
            try
            {
                JObject rows = new JObject();
                bool exact = true;
                foreach (int strength in new[] { 24, 26, 28, 30, 10, 8 })
                {
                    owner.Stats.Strength.BaseValue = strength;
                    int modifier = owner.Stats.Strength.Bonus;
                    int nativeStrength = modifier > 0
                        ? modifier + modifier / 2 : modifier;
                    int desiredStrength = modifier > 0
                        ? checked(modifier * 2) : modifier;
                    var ordinaryAttack = new RuleAttackWithWeapon(owner,
                        fixture.Hostile, gore, 0) {
                        IsCharge = false, IsFirstAttack = true };
                    RuleCalculateWeaponStats ordinary = Rulebook.Trigger(
                        new RuleCalculateWeaponStats(owner, gore,
                            ordinaryAttack));
                    var abortedCharge = new RuleAttackWithWeapon(owner,
                        fixture.Hostile, gore, 0) {
                        IsCharge = true, IsFirstAttack = true };
                    RuleCalculateWeaponStats markerless = Rulebook.Trigger(
                        new RuleCalculateWeaponStats(owner, gore,
                            abortedCharge));
                    if (owner.Descriptor.AddFact(marker) == null &&
                        !owner.Descriptor.HasFact(marker))
                        throw new InvalidOperationException(
                            "Native charge marker was unavailable.");
                    RuleCalculateWeaponStats charged = Rulebook.Trigger(
                        new RuleCalculateWeaponStats(owner, gore,
                            abortedCharge));
                    var later = new RuleAttackWithWeapon(owner,
                        fixture.Hostile, gore, 0) {
                        IsCharge = true, IsFirstAttack = false };
                    var opportunity = new RuleAttackWithWeapon(owner,
                        fixture.Hostile, gore, 0) {
                        IsCharge = true, IsFirstAttack = true,
                        IsAttackOfOpportunity = true };
                    RuleCalculateWeaponStats laterStats = Rulebook.Trigger(
                        new RuleCalculateWeaponStats(owner, gore, later));
                    RuleCalculateWeaponStats opportunityStats = Rulebook
                        .Trigger(new RuleCalculateWeaponStats(owner, gore,
                            opportunity));
                    owner.Descriptor.RemoveFact(marker);
                    int ordinaryBonus = ordinary.DamageDescription[0].Bonus;
                    int chargedBonus = charged.DamageDescription[0].Bonus;
                    int additionalCharge = chargedBonus - ordinaryBonus;
                    int expectedCorrection = desiredStrength - nativeStrength;
                    bool rowExact = owner.Stats.Strength.ModifiedValue ==
                            strength && ordinary.DamageDescription.Count == 1 &&
                        charged.DamageDescription.Count == 1 &&
                        ordinary.DamageDescription[0].Dice.Rolls == 2 &&
                        (int)ordinary.DamageDescription[0].Dice.Dice == 10 &&
                        charged.DamageDescription[0].Dice.Rolls == 4 &&
                        (int)charged.DamageDescription[0].Dice.Dice == 10 &&
                        ordinaryBonus == nativeStrength &&
                        chargedBonus == desiredStrength &&
                        additionalCharge == expectedCorrection &&
                        markerless.DamageDescription[0].Bonus == ordinaryBonus &&
                        markerless.DamageDescription[0].Dice.Rolls == 2 &&
                        laterStats.DamageDescription[0].Bonus == ordinaryBonus &&
                        laterStats.DamageDescription[0].Dice.Rolls == 2 &&
                        opportunityStats.DamageDescription[0].Bonus ==
                            ordinaryBonus &&
                        opportunityStats.DamageDescription[0].Dice.Rolls == 2 &&
                        ordinary.DamageBonusStat == StatType.Strength &&
                        ordinary.DamageBonusStatMultiplier == 1.5f &&
                        ordinary.BonusDamage - nativeStrength == 0 &&
                        charged.BonusDamage - desiredStrength == 0;
                    exact &= rowExact;
                    rows[strength.ToString()] = new JObject {
                        ["strength"] = owner.Stats.Strength.ModifiedValue,
                        ["modifier"] = modifier,
                        ["nativeStrengthContribution"] = nativeStrength,
                        ["desiredStrengthContribution"] = desiredStrength,
                        ["additionalChargeContribution"] = additionalCharge,
                        ["expectedChargeCorrection"] = expectedCorrection,
                        ["ordinary"] = Sprint23WeaponStatsRow(ordinary,
                            nativeStrength),
                        ["abortedChargeWithoutMarker"] =
                            Sprint23WeaponStatsRow(markerless, nativeStrength),
                        ["charged"] = Sprint23WeaponStatsRow(charged,
                            desiredStrength),
                        ["laterChargeStrike"] = Sprint23WeaponStatsRow(
                            laterStats, nativeStrength),
                        ["opportunityChargeStrike"] = Sprint23WeaponStatsRow(
                            opportunityStats, nativeStrength),
                        ["passed"] = rowExact };
                }
                Sprint23Check("powerful-charge-live-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject { ["unit"] = owner.UniqueId,
                        ["target"] = fixture.Hostile.UniqueId,
                        ["event"] = "charge-marker-and-live-strength",
                        ["owner"] = owner.Blueprint.AssetGuid,
                        ["weapon"] = gore.Blueprint.AssetGuid,
                        ["weaponInstance"] = gore.GetHashCode().ToString("X8"),
                        ["command"] = JValue.CreateNull(),
                        ["calculationOnly"] = true,
                        ["rows"] = rows },
                    "At Strength 24/26/28/30 the marker-owned first charge " +
                    "gore raises the native truncated 1.5x contribution to " +
                    "the documented positive 2x target; zero and penalties " +
                    "remain once-applied, while ordinary, aborted, later and " +
                    "opportunity attacks remain ordinary.");
            }
            finally
            {
                owner.Stats.Strength.BaseValue = strengthBefore;
                if (marker != null && owner.Descriptor.HasFact(marker))
                    owner.Descriptor.RemoveFact(marker);
            }
        }

        private static JObject Sprint23WeaponStatsRow(
            RuleCalculateWeaponStats stats, int expectedStrengthContribution)
        {
            DamageDescription damage = stats.DamageDescription[0];
            return new JObject {
                ["diceRolls"] = damage.Dice.Rolls,
                ["dieSides"] = (int)damage.Dice.Dice,
                ["damageBonus"] = damage.Bonus,
                ["bonusDamage"] = stats.BonusDamage,
                ["damageBonusStat"] = stats.DamageBonusStat.HasValue ?
                    stats.DamageBonusStat.Value.ToString() : null,
                ["damageBonusStatMultiplier"] =
                    stats.DamageBonusStatMultiplier,
                ["strengthContribution"] = expectedStrengthContribution,
                ["otherAdditiveDamage"] = stats.BonusDamage -
                    expectedStrengthContribution,
                ["enhancement"] = stats.Enhancement,
                ["criticalEdge"] = stats.CriticalEdge,
                ["doubleCriticalEdge"] = stats.DoubleCriticalEdge,
                ["weaponSize"] = stats.WeaponSize.ToString(),
                ["damageType"] = damage.TypeDescription.Type.ToString(),
                ["physicalForm"] = damage.TypeDescription.Type ==
                    DamageType.Physical ?
                    damage.TypeDescription.Physical.Form.ToString() : null,
                ["physicalEnhancementTotal"] = damage.TypeDescription.Type ==
                    DamageType.Physical ? damage.TypeDescription.Physical
                        .EnhancementTotal : 0
            };
        }

        private IEnumerable<int> Sprint23PowerAttackChargeControl(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            ItemEntityWeapon gore = Sprint19Limbs(owner).Single();
            BlueprintBuff marker = BlueprintRoot.Instance.SystemMechanics
                .ChargeBuff;
            int strengthBefore = owner.Stats.Strength.BaseValue;
            RuleCalculateWeaponStats ordinaryOff = null, chargedOff = null,
                ordinaryOn = null, chargedOn = null;
            try
            {
                owner.Stats.Strength.BaseValue = 28;
                foreach (int frame in Sprint22SetPowerAttack(owner, false))
                    yield return frame;
                var ordinaryAttack = new RuleAttackWithWeapon(owner,
                    fixture.Hostile, gore, 0) { IsFirstAttack = true };
                var chargeAttack = new RuleAttackWithWeapon(owner,
                    fixture.Hostile, gore, 0) {
                    IsCharge = true, IsFirstAttack = true };
                ordinaryOff = Rulebook.Trigger(new RuleCalculateWeaponStats(
                    owner, gore, ordinaryAttack));
                owner.Descriptor.AddFact(marker);
                chargedOff = Rulebook.Trigger(new RuleCalculateWeaponStats(
                    owner, gore, chargeAttack));
                owner.Descriptor.RemoveFact(marker);

                foreach (int frame in Sprint22SetPowerAttack(owner, true))
                    yield return frame;
                ordinaryOn = Rulebook.Trigger(new RuleCalculateWeaponStats(
                    owner, gore, ordinaryAttack));
                owner.Descriptor.AddFact(marker);
                chargedOn = Rulebook.Trigger(new RuleCalculateWeaponStats(
                    owner, gore, chargeAttack));
                owner.Descriptor.RemoveFact(marker);

                int offOrdinary = ordinaryOff.DamageDescription[0].Bonus;
                int offCharged = chargedOff.DamageDescription[0].Bonus;
                int onOrdinary = ordinaryOn.DamageDescription[0].Bonus;
                int onCharged = chargedOn.DamageDescription[0].Bonus;
                bool exact = owner.Stats.Strength.ModifiedValue == 28 &&
                    owner.Stats.Strength.Bonus == 9 &&
                    offOrdinary == 13 && offCharged == 18 &&
                    onOrdinary == 22 && onCharged == 27 &&
                    offCharged - offOrdinary == 5 &&
                    onCharged - onOrdinary == 5 &&
                    onOrdinary - offOrdinary == 9 &&
                    onCharged - offCharged == 9;
                Sprint23Check("powerful-charge-power-attack-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject { ["unit"] = owner.UniqueId,
                        ["target"] = fixture.Hostile.UniqueId,
                        ["event"] = "odd-strength-power-attack-comparison",
                        ["strength"] = owner.Stats.Strength.ModifiedValue,
                        ["modifier"] = owner.Stats.Strength.Bonus,
                        ["weapon"] = gore.Blueprint.AssetGuid,
                        ["powerAttackOff"] = new JObject {
                            ["ordinary"] = Sprint23WeaponStatsRow(
                                ordinaryOff, 13),
                            ["charged"] = Sprint23WeaponStatsRow(
                                chargedOff, 18) },
                        ["powerAttackOn"] = new JObject {
                            ["ordinary"] = Sprint23WeaponStatsRow(
                                ordinaryOn, 13),
                            ["charged"] = Sprint23WeaponStatsRow(
                                chargedOn, 18) },
                        ["ordinaryPowerAttackContribution"] =
                            onOrdinary - offOrdinary,
                        ["chargedPowerAttackContribution"] =
                            onCharged - offCharged,
                        ["chargeContributionPowerAttackOff"] =
                            offCharged - offOrdinary,
                        ["chargeContributionPowerAttackOn"] =
                            onCharged - onOrdinary },
                    "At odd modifier +9, native Power Attack contributes +9 " +
                    "exactly once to both ordinary and charged gore while " +
                    "the charge-only Strength correction remains +5.");
            }
            finally
            {
                owner.Stats.Strength.BaseValue = strengthBefore;
                if (marker != null && owner.Descriptor.HasFact(marker))
                    owner.Descriptor.RemoveFact(marker);
            }
            foreach (int frame in Sprint22SetPowerAttack(owner, false))
                yield return frame;
        }

        private void Sprint23CriticalChargeControl(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            ItemEntityWeapon gore = Sprint19Limbs(owner).Single();
            BlueprintBuff marker = BlueprintRoot.Instance.SystemMechanics
                .ChargeBuff;
            int strengthBefore = owner.Stats.Strength.BaseValue;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int hitPointsBefore = target.Stats.HitPoints.BaseValue;
            int damageBefore = target.Descriptor.Damage;
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            var observer = new Sprint16RuleObserver {
                Owner = owner,
                Target = target,
                BeforeAttackRollForFixture = value => {
                    value.AutoCriticalThreat = true;
                    value.AutoCriticalConfirmation = true;
                }
            };
            EventBus.Subscribe(observer);
            try
            {
                owner.Stats.Strength.BaseValue = 28;
                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                target.Stats.HitPoints.BaseValue = 100000;
                target.Descriptor.Damage = 0;
                owner.Descriptor.AddFact(marker);
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                RuleAttackWithWeapon attack = Rulebook.Trigger(
                    new RuleAttackWithWeapon(owner, target, gore, 0) {
                        IsCharge = true, IsFirstAttack = true });
                PhysicalDamage physical = attack.MeleeDamage?.DamageBundle
                    .OfType<PhysicalDamage>().SingleOrDefault();
                bool exact = attack.AttackRoll != null &&
                    attack.AttackRoll.IsHit &&
                    attack.AttackRoll.IsCriticalConfirmed &&
                    attack.WeaponStats != null &&
                    attack.WeaponStats.DamageDescription.Count == 1 &&
                    attack.WeaponStats.DamageDescription[0].Dice.Rolls == 4 &&
                    (int)attack.WeaponStats.DamageDescription[0].Dice.Dice == 10 &&
                    attack.WeaponStats.DamageDescription[0].Bonus == 18 &&
                    physical != null && physical.CriticalModifier == 2 &&
                    observer.WeaponAttacks == 1 && observer.Damage.Count == 1;
                Sprint23Check("powerful-charge-critical-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject { ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "native-confirmed-critical-charge-rule",
                        ["weapon"] = gore.Blueprint.AssetGuid,
                        ["strength"] = owner.Stats.Strength.ModifiedValue,
                        ["modifier"] = owner.Stats.Strength.Bonus,
                        ["naturalRoll"] = attack.AttackRoll?.Roll.Value,
                        ["attackBonus"] = attack.AttackRoll?.AttackBonus,
                        ["targetArmorClass"] = attack.AttackRoll?.TargetAC,
                        ["hit"] = attack.AttackRoll?.IsHit,
                        ["criticalThreat"] = attack.AttackRoll?.IsCriticalRoll,
                        ["criticalConfirmed"] = attack.AttackRoll?
                            .IsCriticalConfirmed,
                        ["criticalMultiplier"] = physical == null ? -1 :
                            (int)physical.CriticalModifier,
                        ["weaponStats"] = attack.WeaponStats == null ? null :
                            Sprint23WeaponStatsRow(attack.WeaponStats, 18),
                        ["damage"] = attack.MeleeDamage == null ? null :
                            Sprint16DamageEvent(attack.MeleeDamage),
                        ["weaponAttackRules"] = observer.WeaponAttacks,
                        ["damageRules"] = observer.Damage.Count },
                    "A confirmed native critical multiplies the one corrected " +
                    "4d10+18 weapon packet exactly once; the charge component " +
                    "does not replay damage or critical handling.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                owner.Stats.Strength.BaseValue = strengthBefore;
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                target.Stats.HitPoints.BaseValue = hitPointsBefore;
                target.Descriptor.Damage = damageBefore;
                UnityEngine.Random.state = randomBefore;
                if (marker != null && owner.Descriptor.HasFact(marker))
                    owner.Descriptor.RemoveFact(marker);
            }
        }

        private TargetWrapper Sprint23FindChargeRoute(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target,
            AbilityData charge, out Vector3 start, out string detail)
        {
            if (AstarPath.active == null)
                throw new InvalidOperationException(
                    "Sprint 23 native charge requires live area navigation.");
            var attempts = new List<string>();
            var centres = new List<Vector3> {
                target.Position, fixture.Caster.Position };
            centres.AddRange(CompassOffsets.Select(direction =>
                fixture.Caster.Position + direction * 4f));
            for (int centreIndex = 0; centreIndex < centres.Count;
                centreIndex++)
            {
                Pathfinding.NNInfo targetNode = Kingmaker.View
                    .ObstacleAnalyzer.GetNearestNode(centres[centreIndex]);
                if (targetNode.node == null || !targetNode.node.Walkable ||
                    Vector3.Distance(targetNode.clampedPosition,
                        centres[centreIndex]) > 1.5f) continue;
                PlaceExpandedSummoningUnit(target,
                    targetNode.clampedPosition);
                // Fifteen-foot reach plus the Huge collision radii can put a
                // seven-metre centre gap inside attack approach distance.  A
                // charge must still travel at least ten feet, so search the
                // measured open fixture at genuinely discriminating gaps.
                foreach (float distance in new[] { 12f, 11f, 10f, 9f, 8f })
                    for (int index = 0; index < CompassOffsets.Length;
                        index++)
                    {
                        Vector3 requested = target.Position +
                            CompassOffsets[index] * distance;
                        Pathfinding.NNInfo nearest = Kingmaker.View
                            .ObstacleAnalyzer.GetNearestNode(requested);
                        if (nearest.node == null || !nearest.node.Walkable ||
                            Vector3.Distance(nearest.clampedPosition,
                                requested) > 1.5f) continue;
                        Vector3 candidate = nearest.clampedPosition;
                        Vector3 traced = Kingmaker.View.ObstacleAnalyzer
                            .TraceAlongNavmesh(candidate, target.Position);
                        bool line = Kingmaker.Visual.FogOfWar
                            .LineOfSightGeometry.Instance != null &&
                            !Kingmaker.Visual.FogOfWar.LineOfSightGeometry
                                .Instance.HasObstacle(candidate,
                                    target.Position, 0);
                        PlaceExpandedSummoningUnit(owner, candidate);
                        var wrapper = new TargetWrapper(target);
                        bool allowed = charge.CanTarget(wrapper) &&
                            charge.IsAvailable;
                        if (attempts.Count < 30 || allowed)
                            attempts.Add(centreIndex + "/" + distance +
                                "/" + index + ":allowed=" + allowed +
                                ",line=" + line + ",gap=" +
                                Vector3.Distance(candidate, target.Position)
                                    .ToString("0.00") + ",trace=" +
                                Vector3.Distance(traced, target.Position)
                                    .ToString("0.00"));
                        if (!allowed || !line ||
                            Vector3.Distance(traced, target.Position) > 1f ||
                            Vector3.Distance(candidate, target.Position) <=
                                3.3f) continue;
                        start = owner.Position;
                        detail = string.Join("|", attempts.ToArray());
                        return wrapper;
                    }
            }
            start = owner.Position;
            detail = string.Join("|", attempts.ToArray());
            return null;
        }

        private IEnumerable<int> Sprint23NativeChargeCommand(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased,
            int requestedStrength = 26)
        {
            UnitEntityData owner = CastExpandedSummoningQuietUnit(fixture,
                ArmoredDinosaurRulesPolicy.TriceratopsKey);
            UnitEntityData target = fixture.Hostile;
            Size sizeBefore = target.Descriptor.State.Size;
            int damageBefore = target.Descriptor.Damage;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int strengthBefore = owner.Stats.Strength.BaseValue;
            int armorClassBefore = target.Stats.AC.BaseValue;
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            ExpandedSummoningChargeAttackObserver observer = null;
            UnitUseAbility command = null;
            UnitAttack attackCommand = null;
            UnitEntityData[] awakeBefore = null;
            int targetArmorClassBefore = 0;
            int targetArmorClassDuring = 0;
            int desiredTargetArmorClass = 0;
            int nativeRollSeed = 0;
            JObject powerAttackBeforeCommand = null;
            try
            {
                target.Descriptor.State.Size = Size.Medium;
                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                owner.Stats.Strength.BaseValue = requestedStrength;
                foreach (int frame in Sprint22SetPowerAttack(owner, false))
                    yield return frame;
                BlueprintAbility chargeBlueprint = fixture.Blueprints
                    .OfType<BlueprintAbility>().Single(value =>
                        value.AssetGuid ==
                            "c78506dd0e14f7c45a599990e4e65038");
                Ability granted = owner.Descriptor.Abilities.GetAbility(
                    chargeBlueprint);
                if (granted == null)
                    throw new InvalidOperationException(
                        "Triceratops lacks native ChargeAbility.");
                var charge = new AbilityData(granted);
                Vector3 start; string route;
                TargetWrapper selected = Sprint23FindChargeRoute(fixture,
                    owner, target, charge, out start, out route);
                if (selected == null)
                    throw new InvalidOperationException(
                        "No live Triceratops charge route: " + route);
                if (turnBased)
                {
                    var admission = new JObject();
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(
                        new[] { owner }, target, admission)) yield return frame;
                    if (!(bool)admission["admitted"])
                        throw new InvalidOperationException(
                            "Triceratops charge owner did not enter turn order: " +
                            admission.ToString(Formatting.None));
                    int waited = 0;
                    while (!Sprint18OwnerIsActing(owner) && waited++ < 320)
                        yield return 0;
                    if (!Sprint18OwnerIsActing(owner))
                        throw new InvalidOperationException(
                            "Triceratops charge owner never received its turn.");
                }
                ItemEntityWeapon gore = Sprint19Limbs(owner).Single();
                var ordinaryAttack = new RuleAttackWithWeapon(owner, target,
                    gore, 0) { IsFirstAttack = true };
                RuleCalculateWeaponStats ordinaryStats = Rulebook.Trigger(
                    new RuleCalculateWeaponStats(owner, gore,
                        ordinaryAttack));
                int ordinaryAttackBonus = Rulebook.Trigger(
                    new RuleCalculateAttackBonusWithoutTarget(owner, gore,
                        0)).Result;
                targetArmorClassBefore = Rulebook.Trigger(
                    new RuleCalculateAC(owner, target,
                        AttackType.Melee)).TargetAC;
                // PowerAttackWatcher compares native expected damage with and
                // without its BAB+10 penalty. Begin with the native toggle off;
                // at this exact AC its no-Power-Attack branch remains the
                // winner for every +7..+10 Strength row, while a deterministic
                // natural 18 still makes the resulting charge hit. This
                // exercises the watcher instead of disabling it and keeps its
                // +9 damage out of the charge-rounding measurement.
                desiredTargetArmorClass = ordinaryAttackBonus + 15;
                target.Stats.AC.BaseValue += desiredTargetArmorClass -
                    targetArmorClassBefore;
                targetArmorClassDuring = Rulebook.Trigger(
                    new RuleCalculateAC(owner, target,
                        AttackType.Melee)).TargetAC;
                var preparedPowerAttack = owner.Descriptor
                    .ActivatableAbilities.Enumerable.Single(value =>
                        value.Blueprint.AssetGuid ==
                            "a7b339e4f6ff93a4697df5d7a87ff619");
                powerAttackBeforeCommand = Sprint22PowerAttackState(owner,
                    preparedPowerAttack);
                nativeRollSeed = FindNativeD20Seed(18);
                observer = new ExpandedSummoningChargeAttackObserver {
                    Initiator = owner, Target = target,
                    Gore = gore.Blueprint,
                    Marker = BlueprintRoot.Instance.SystemMechanics
                        .ChargeBuff,
                    ObservePowerAttackChoice = true,
                    NativeRollSeed = nativeRollSeed };
                EventBus.Subscribe(observer);
                awakeBefore = Game.Instance.State.AwakeUnits.ToArray();
                foreach (UnitEntityData unit in new[] { owner, target })
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                float standardBefore = owner.CombatState.Cooldown
                    .StandardAction;
                float moveBefore = owner.CombatState.Cooldown.MoveAction;
                float maxStandard = standardBefore;
                float maxMove = moveBefore;
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                command = new UnitUseAbility(charge, selected);
                bool nativeEconomy = !command.Cutscene &&
                    !command.IsIgnoreCooldown;
                if (!command.CanStart)
                    throw new InvalidOperationException(
                        "Native Triceratops charge command cannot start.");
                owner.Commands.Run(command);
                bool queued = owner.Commands.Raw.Contains(command);
                bool executionSeen = command.ExecutionProcess != null;
                var controller = new UnitActionController();
                int movementTicks = 0;
                int frames = 0;
                while (frames++ < 600 &&
                    !observer.FirstChargeHitWithMarker)
                {
                    if (!command.IsFinished &&
                        (!command.IsStarted || command.ExecutionProcess == null))
                    {
                        if (command.Animation != null && frames > 4)
                            command.Animation.IsActed = true;
                        ElementalBreathScenario.TickCommand(controller,
                            command);
                    }
                    executionSeen |= command.ExecutionProcess != null;
                    float delta = Game.Instance.TimeController.DeltaTime;
                    if (command.ExecutionProcess != null && delta > 0f &&
                        owner.Descriptor.State.CanMove && !CombatController
                            .IsInTurnBasedCombat())
                    {
                        owner.View.MovementAgent.TickMovement(delta);
                        owner.Position = owner.View.transform.position;
                        movementTicks++;
                    }
                    if (command.ExecutionProcess != null)
                        TickExpandedSummoningDetachedAbility(command, frames);
                    if (attackCommand == null)
                        attackCommand = owner.Commands.Raw
                            .OfType<UnitAttack>().FirstOrDefault(value =>
                                ReferenceEquals(value.Target, target));
                    if (attackCommand != null &&
                        !attackCommand.IsFinished)
                    {
                        if (attackCommand.Animation != null && frames > 12)
                            attackCommand.Animation.IsActed = true;
                        ElementalBreathScenario.TickCommand(controller,
                            attackCommand);
                    }
                    maxStandard = Mathf.Max(maxStandard,
                        owner.CombatState.Cooldown.StandardAction);
                    maxMove = Mathf.Max(maxMove,
                        owner.CombatState.Cooldown.MoveAction);
                    yield return 0;
                }
                for (int settle = 0; settle < 24 &&
                    command.IsRunning && !command.IsFinished; settle++)
                {
                    if (command.Animation != null)
                        command.Animation.IsActed = true;
                    yield return 0;
                }
                float moved = Vector3.Distance(start, owner.Position);
                int damage = target.Descriptor.Damage - damageBefore;
                // RTWP outside combat owns actions through an ordinary queued
                // command but does not accrue turn cooldown. Turn-based must
                // prove the numeric action debit separately.
                bool spent = turnBased ? maxStandard > standardBefore ||
                    maxMove > moveBefore : nativeEconomy &&
                    command.IsStarted;
                Game.Instance.State.AwakeUnits.Clear();
                Game.Instance.State.AwakeUnits.AddRange(awakeBefore);
                bool awakeRestored = Game.Instance.State.AwakeUnits
                    .SequenceEqual(awakeBefore);
                awakeBefore = null;
                RuleAttackWithWeapon resolved = observer.AttackRules.Count == 1
                    ? observer.AttackRules[0] : null;
                RuleCalculateWeaponStats chargedStats = resolved == null ?
                    null : resolved.WeaponStats;
                RuleAttackRoll roll = resolved == null ? null :
                    resolved.AttackRoll;
                RuleDealDamage damageRule = resolved == null ? null :
                    resolved.MeleeDamage;
                var powerAttack = owner.Descriptor.ActivatableAbilities
                    .Enumerable.Single(value => value.Blueprint.AssetGuid ==
                        "a7b339e4f6ff93a4697df5d7a87ff619");
                int modifier = owner.Stats.Strength.Bonus;
                int nativeStrength = modifier > 0 ?
                    modifier + modifier / 2 : modifier;
                int desiredStrength = modifier > 0 ?
                    checked(modifier * 2) : modifier;
                int chargeCorrection = desiredStrength - nativeStrength;
                bool arithmetic = owner.Stats.Strength.ModifiedValue ==
                        requestedStrength &&
                    modifier == (requestedStrength - 10) / 2 &&
                    observer.PowerAttackDecisionCount == 1 &&
                    !observer.PowerAttackActiveBeforeRoll &&
                    observer.PowerAttackOffAfterAttack &&
                    targetArmorClassDuring == desiredTargetArmorClass &&
                    ordinaryStats.DamageDescription[0].Dice.Rolls == 2 &&
                    (int)ordinaryStats.DamageDescription[0].Dice.Dice == 10 &&
                    ordinaryStats.DamageDescription[0].Bonus ==
                        nativeStrength &&
                    chargedStats != null &&
                    chargedStats.DamageDescription.Count == 1 &&
                    chargedStats.DamageDescription[0].Dice.Rolls == 4 &&
                    (int)chargedStats.DamageDescription[0].Dice.Dice == 10 &&
                    chargedStats.DamageDescription[0].Bonus ==
                        desiredStrength &&
                    chargedStats.DamageDescription[0].Bonus -
                        ordinaryStats.DamageDescription[0].Bonus ==
                            chargeCorrection &&
                    chargedStats.BonusDamage - desiredStrength == 0 &&
                    roll != null &&
                    roll.Roll.Value == 18 &&
                    roll.AttackBonus == ordinaryAttackBonus + 2 &&
                    roll.IsHit && !roll.AutoMiss &&
                    !roll.IsCriticalConfirmed && damageRule != null &&
                    damageRule.DamageBundle.Weapon != null &&
                    ReferenceEquals(damageRule.DamageBundle.Weapon.Blueprint,
                        gore.Blueprint);
                bool exact = queued && nativeEconomy && command.IsStarted &&
                    executionSeen &&
                    observer.FirstChargeHitWithMarker &&
                    moved >= new Feet(10).Meters - .05f &&
                    damage > 0 && spent && observer.Attacks.Count == 1 &&
                    attackCommand != null && attackCommand.IsStarted &&
                    awakeRestored && arithmetic;
                Sprint23Check("native-charge-command-" +
                    (turnBased ? "turn-based" : "rtwp") + "-strength-" +
                    requestedStrength, exact,
                    new JObject { ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "queued-native-charge",
                        ["route"] = route, ["queued"] = queued,
                        ["started"] = command.IsStarted,
                        ["running"] = command.IsRunning,
                        ["finished"] = command.IsFinished,
                        ["result"] = command.Result.ToString(),
                        ["commandId"] = command.GetHashCode().ToString("X8"),
                        ["executionSeen"] = executionSeen,
                        ["frames"] = frames, ["movedMeters"] = moved,
                        ["damage"] = damage,
                        ["nativeEconomy"] = nativeEconomy,
                        ["movementTicks"] = movementTicks,
                        ["attackCommand"] = attackCommand == null ? null :
                            attackCommand.Result.ToString(),
                        ["attackCommandId"] = attackCommand == null ? null :
                            attackCommand.GetHashCode().ToString("X8"),
                        ["awakeRestored"] = awakeRestored,
                        ["agent"] = DescribeExpandedSummoningTrampleAgent(
                            owner),
                        ["attacks"] = new JArray(observer.Attacks),
                        ["strength"] = owner.Stats.Strength.ModifiedValue,
                        ["modifier"] = owner.Stats.Strength.Bonus,
                        ["ordinaryAttackBonus"] = ordinaryAttackBonus,
                        ["targetArmorClassBefore"] =
                            targetArmorClassBefore,
                        ["targetArmorClassDuring"] =
                            targetArmorClassDuring,
                        ["desiredTargetArmorClass"] =
                            desiredTargetArmorClass,
                        ["nativeRollSeed"] = nativeRollSeed,
                        ["powerAttackBeforeCommand"] =
                            powerAttackBeforeCommand,
                        ["powerAttack"] = Sprint22PowerAttackState(owner,
                            powerAttack),
                        ["powerAttackBeforeChargeAttack"] =
                            observer.PowerAttackBeforeAttack,
                        ["powerAttackAfterNativeChoice"] =
                            observer.PowerAttackAfterAttack,
                        ["powerAttackDecisionCount"] =
                            observer.PowerAttackDecisionCount,
                        ["powerAttackActiveBeforeRoll"] =
                            observer.PowerAttackActiveBeforeRoll,
                        ["powerAttackOffAfterAttack"] =
                            observer.PowerAttackOffAfterAttack,
                        ["weapon"] = gore.Blueprint.AssetGuid,
                        ["weaponInstance"] = gore.GetHashCode().ToString("X8"),
                        ["ordinaryWeaponStats"] = Sprint23WeaponStatsRow(
                            ordinaryStats, nativeStrength),
                        ["chargedWeaponStats"] = chargedStats == null ? null :
                            Sprint23WeaponStatsRow(chargedStats,
                                desiredStrength),
                        ["additionalChargeContribution"] = chargedStats ==
                            null ? -999 : chargedStats.DamageDescription[0]
                                .Bonus - ordinaryStats.DamageDescription[0]
                                .Bonus,
                        ["naturalRoll"] = roll?.Roll.Value,
                        ["attackBonus"] = roll?.AttackBonus,
                        ["targetArmorClass"] = roll?.TargetAC,
                        ["hit"] = roll?.IsHit,
                        ["criticalThreat"] = roll?.IsCriticalRoll,
                        ["criticalConfirmed"] = roll?
                            .IsCriticalConfirmed,
                        ["damageRule"] = damageRule == null ? null :
                            Sprint16DamageEvent(damageRule),
                        ["standardCooldown"] = new JArray(standardBefore,
                            maxStandard,
                            owner.CombatState.Cooldown.StandardAction),
                        ["moveCooldown"] = new JArray(moveBefore, maxMove,
                            owner.CombatState.Cooldown.MoveAction),
                        ["mode"] = turnBased ? "turn-based" : "rtwp" },
                    "At Strength " + requestedStrength + " (modifier " +
                    modifier + "), a queued native ChargeAbility follows a " +
                    "real path and its sole noncritical first gore lands with " +
                    "the exact live Strength correction and native action " +
                    "ownership.");
            }
            finally
            {
                if (observer != null) EventBus.Unsubscribe(observer);
                EndExpandedSummoningDetachedAbility(command);
                owner.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                if (awakeBefore != null)
                {
                    Game.Instance.State.AwakeUnits.Clear();
                    Game.Instance.State.AwakeUnits.AddRange(awakeBefore);
                }
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                owner.Stats.Strength.BaseValue = strengthBefore;
                target.Stats.AC.BaseValue = armorClassBefore;
                target.Descriptor.State.Size = sizeBefore;
                target.Descriptor.Damage = damageBefore;
                UnityEngine.Random.state = randomBefore;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner });
            }
        }

        private TargetWrapper Sprint23FindTrampleRoute(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target,
            AbilityData trample, out Vector3 start, out string detail)
        {
            if (AstarPath.active == null)
                throw new InvalidOperationException(
                    "Sprint 23 native trample requires live area navigation.");
            var attempts = new List<string>();
            var centres = new List<Vector3> {
                target.Position, fixture.Caster.Position };
            centres.AddRange(CompassOffsets.Select(direction =>
                fixture.Caster.Position + direction * 4f));
            for (int centreIndex = 0; centreIndex < centres.Count;
                centreIndex++)
                foreach (float[] distance in new[] {
                    new[] { 3f, 3f }, new[] { 4f, 3f },
                    new[] { 5f, 4f } })
                    for (int index = 0; index < CompassOffsets.Length;
                        index++)
                    {
                        Vector3 direction = CompassOffsets[index];
                        Pathfinding.NNInfo from = Kingmaker.View
                            .ObstacleAnalyzer.GetNearestNode(
                                centres[centreIndex] - direction *
                                    distance[0]);
                        Pathfinding.NNInfo to = Kingmaker.View
                            .ObstacleAnalyzer.GetNearestNode(
                                centres[centreIndex] + direction *
                                    distance[1]);
                        if (from.node == null || to.node == null ||
                            !from.node.Walkable || !to.node.Walkable)
                            continue;
                        PlaceExpandedSummoningUnit(owner,
                            from.clampedPosition);
                        PlaceExpandedSummoningUnit(target,
                            (from.clampedPosition + to.clampedPosition) * .5f);
                        var wrapper = new TargetWrapper(to.clampedPosition);
                        Vector3 traced = Kingmaker.View.ObstacleAnalyzer
                            .TraceAlongNavmesh(owner.Position,
                                to.clampedPosition);
                        bool line = Kingmaker.Visual.FogOfWar
                            .LineOfSightGeometry.Instance != null &&
                            !Kingmaker.Visual.FogOfWar.LineOfSightGeometry
                                .Instance.HasObstacle(owner.EyePosition,
                                    to.clampedPosition, 0);
                        bool allowed = trample.CanTarget(wrapper) &&
                            trample.IsAvailable;
                        if (attempts.Count < 30 || allowed)
                            attempts.Add(centreIndex + "/" + distance[0] +
                                "/" + distance[1] + "/" + index +
                                ":allowed=" + allowed + ",line=" + line +
                                ",trace=" + Vector3.Distance(traced,
                                    to.clampedPosition).ToString("0.00"));
                        if (!allowed || !line || Vector3.Distance(traced,
                                to.clampedPosition) > .25f) continue;
                        start = owner.Position;
                        detail = string.Join("|", attempts.ToArray());
                        return wrapper;
                    }
            start = owner.Position;
            detail = string.Join("|", attempts.ToArray());
            return null;
        }

        private IEnumerable<int> Sprint23NativeTrampleCommand(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            UnitEntityData owner = CastExpandedSummoningQuietUnit(fixture,
                ArmoredDinosaurRulesPolicy.TriceratopsKey);
            UnitEntityData target = fixture.Hostile;
            Size sizeBefore = target.Descriptor.State.Size;
            int reflexBefore = target.Stats.SaveReflex.BaseValue;
            int damageBefore = target.Descriptor.Damage;
            ExpandedSummoningTrampleObserver observer = null;
            UnitUseAbility command = null;
            UnitEntityData[] awakeBefore = null;
            try
            {
                target.Descriptor.State.Size = Size.Medium;
                target.Stats.SaveReflex.BaseValue = -100;
                target.CombatState.OnNewRound();
                target.CombatState.AttackOfOpportunityCount = 0;
                BlueprintAbility blueprint = fixture.Blueprints
                    .OfType<BlueprintAbility>().Single(value => value.name ==
                        "KMG_Summoning_Special_Triceratops_Trample");
                Ability granted = owner.Descriptor.Abilities.GetAbility(
                    blueprint);
                if (granted == null)
                    throw new InvalidOperationException(
                        "Triceratops lacks its exact Trample ability.");
                var trample = new AbilityData(granted);
                Vector3 start; string route;
                TargetWrapper selected = Sprint23FindTrampleRoute(fixture,
                    owner, target, trample, out start, out route);
                if (selected == null)
                    throw new InvalidOperationException(
                        "No live Triceratops trample route: " + route);
                if (turnBased)
                {
                    var admission = new JObject();
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(
                        new[] { owner }, target, admission)) yield return frame;
                    if (!(bool)admission["admitted"])
                        throw new InvalidOperationException(
                            "Triceratops trample owner did not enter turn order: " +
                            admission.ToString(Formatting.None));
                    int waited = 0;
                    while (!Sprint18OwnerIsActing(owner) && waited++ < 320)
                        yield return 0;
                    if (!Sprint18OwnerIsActing(owner))
                        throw new InvalidOperationException(
                            "Triceratops trample owner never received its turn.");
                }
                observer = new ExpandedSummoningTrampleObserver {
                    Caster = owner, Target = target };
                EventBus.Subscribe(observer);
                awakeBefore = Game.Instance.State.AwakeUnits.ToArray();
                foreach (UnitEntityData unit in new[] { owner, target })
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                float standardBefore = owner.CombatState.Cooldown
                    .StandardAction;
                float moveBefore = owner.CombatState.Cooldown.MoveAction;
                float maxStandard = standardBefore;
                float maxMove = moveBefore;
                command = new UnitUseAbility(trample, selected);
                bool nativeEconomy = !command.Cutscene &&
                    !command.IsIgnoreCooldown && trample.RequireFullRoundAction &&
                    blueprint.IsFullRoundAction;
                if (!command.CanStart)
                    throw new InvalidOperationException(
                        "Native Triceratops trample command cannot start.");
                owner.Commands.Run(command);
                bool queued = owner.Commands.Raw.Contains(command);
                bool executionSeen = command.ExecutionProcess != null;
                var controller = new UnitActionController();
                int movementTicks = 0;
                int frames = 0;
                while (frames++ < 600 &&
                    (observer.Saves.Count == 0 ||
                     observer.Damage.Count == 0))
                {
                    if (!command.IsFinished &&
                        (!command.IsStarted || command.ExecutionProcess == null))
                    {
                        if (command.Animation != null && frames > 4)
                            command.Animation.IsActed = true;
                        ElementalBreathScenario.TickCommand(controller,
                            command);
                    }
                    executionSeen |= command.ExecutionProcess != null;
                    float delta = Game.Instance.TimeController.DeltaTime;
                    if (command.ExecutionProcess != null && delta > 0f &&
                        owner.Descriptor.State.CanMove && !CombatController
                            .IsInTurnBasedCombat())
                    {
                        owner.View.MovementAgent.TickMovement(delta);
                        owner.Position = owner.View.transform.position;
                        movementTicks++;
                    }
                    if (command.ExecutionProcess != null)
                        TickExpandedSummoningDetachedAbility(command, frames);
                    maxStandard = Mathf.Max(maxStandard,
                        owner.CombatState.Cooldown.StandardAction);
                    maxMove = Mathf.Max(maxMove,
                        owner.CombatState.Cooldown.MoveAction);
                    yield return 0;
                }
                for (int settle = 0; settle < 24 && command.IsRunning &&
                    !command.IsFinished; settle++)
                {
                    if (command.Animation != null)
                        command.Animation.IsActed = true;
                    yield return 0;
                }
                float moved = Vector3.Distance(start, owner.Position);
                BaseDamage dealt = observer.Damage.Count == 1 ?
                    observer.Damage[0].DamageBundle.First : null;
                // RTWP outside combat owns this full-round action through the
                // normal command contract; only turn-based accrues both
                // numeric cooldowns in this save-free world.
                bool spent = turnBased ? maxStandard > standardBefore &&
                    maxMove > moveBefore : nativeEconomy &&
                    command.IsStarted;
                Game.Instance.State.AwakeUnits.Clear();
                Game.Instance.State.AwakeUnits.AddRange(awakeBefore);
                bool awakeRestored = Game.Instance.State.AwakeUnits
                    .SequenceEqual(awakeBefore);
                awakeBefore = null;
                bool exact = queued && nativeEconomy && command.IsStarted &&
                    executionSeen &&
                    moved > 3f && observer.Opportunities.Count == 0 &&
                    observer.Saves.Count == 1 &&
                    observer.Saves[0].DifficultyClass == 25 &&
                    observer.Damage.Count == 1 && dealt != null &&
                    dealt.Dice.Rolls == 1 && dealt.Dice.Dice == DiceType.D8 &&
                    dealt.Bonus == 12 && spent && awakeRestored;
                Sprint23Check("native-trample-command-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject { ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "queued-native-trample-path-contact",
                        ["route"] = route, ["queued"] = queued,
                        ["started"] = command.IsStarted,
                        ["running"] = command.IsRunning,
                        ["finished"] = command.IsFinished,
                        ["result"] = command.Result.ToString(),
                        ["executionSeen"] = executionSeen,
                        ["frames"] = frames, ["movedMeters"] = moved,
                        ["nativeEconomy"] = nativeEconomy,
                        ["movementTicks"] = movementTicks,
                        ["awakeRestored"] = awakeRestored,
                        ["agent"] = DescribeExpandedSummoningTrampleAgent(
                            owner),
                        ["targetDamage"] = new JArray(damageBefore,
                            target.Descriptor.Damage),
                        ["saves"] = new JArray(observer.Saves.Select(value =>
                            new JObject { ["dc"] = value.DifficultyClass,
                                ["passed"] = value.IsPassed })),
                        ["damage"] = dealt == null ? null :
                            Sprint16DamageLine(dealt),
                        ["standardCooldown"] = new JArray(standardBefore,
                            maxStandard,
                            owner.CombatState.Cooldown.StandardAction),
                        ["moveCooldown"] = new JArray(moveBefore, maxMove,
                            owner.CombatState.Cooldown.MoveAction),
                        ["mode"] = turnBased ? "turn-based" : "rtwp" },
                    "A queued full-round native Trample follows a real path, contacts only the smaller enemy once, resolves DC 25 Reflex and 1d8+12, and owns native action cost.");
            }
            finally
            {
                if (observer != null) EventBus.Unsubscribe(observer);
                EndExpandedSummoningDetachedAbility(command);
                owner.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                if (awakeBefore != null)
                {
                    Game.Instance.State.AwakeUnits.Clear();
                    Game.Instance.State.AwakeUnits.AddRange(awakeBefore);
                }
                target.Descriptor.State.Size = sizeBefore;
                target.Stats.SaveReflex.BaseValue = reflexBefore;
                target.Descriptor.Damage = damageBefore;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner });
            }
        }

        private static JObject Sprint23TrampleResult(
            ExpandedSummoningTrampleContactResult value)
        {
            return new JObject { ["detail"] = value.Describe(),
                ["opportunities"] = value.Opportunities.Count,
                ["saves"] = new JArray(value.Saves.Select(save =>
                    new JObject { ["dc"] = save.DifficultyClass,
                        ["passed"] = save.IsPassed })),
                ["damage"] = new JArray(value.Damage.Select(dealt =>
                    new JObject { ["amount"] = dealt.Damage,
                        ["half"] = dealt.HalfBecauseSavingThrow,
                        ["dice"] = dealt.DamageBundle.First.Dice.Rolls +
                            "d" + (int)dealt.DamageBundle.First.Dice.Dice,
                        ["bonus"] = dealt.DamageBundle.First.Bonus })) };
        }

        private void Sprint23TrampleMatrix(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData triceratops)
        {
            ExpandedSummoningCorrectionFixture rulesBefore = _rulesFixture;
            int reflexBefore = fixture.Hostile.Stats.SaveReflex.BaseValue;
            Size sizeBefore = fixture.Hostile.Descriptor.State.Size;
            TimeSpan timeBefore = Game.Instance.Player.GameTime;
            var owned = new List<UnitEntityData>();
            try
            {
                _rulesFixture = fixture;
                PrepareExpandedSummoningTrampleResponsePair(fixture,
                    triceratops, fixture.Hostile, 0, 1f);
                fixture.Hostile.Stats.SaveReflex.BaseValue = -100;
                ExpandedSummoningTrampleContactResult first =
                    RunExpandedSummoningTrampleContact(triceratops,
                        fixture.Hostile, "baseline-d8", 10);
                ExpandedSummoningTrampleContactResult duplicate =
                    RunExpandedSummoningTrampleContact(triceratops,
                        fixture.Hostile, "same-round-duplicate", 10);
                BaseDamage firstDamage = first.Damage.Count == 1 ?
                    first.Damage[0].DamageBundle.First : null;
                bool baseline = first.Saves.Count == 1 &&
                    first.Saves[0].DifficultyClass == 25 &&
                    first.Damage.Count == 1 && firstDamage != null &&
                    firstDamage.Dice.Rolls == 1 &&
                    firstDamage.Dice.Dice == DiceType.D8 &&
                    firstDamage.Bonus == 12 &&
                    duplicate.Saves.Count == 0 &&
                    duplicate.Damage.Count == 0;

                Game.Instance.Player.GameTime += TimeSpan.FromSeconds(6d);
                ExpandedSummoningTrampleContactResult next =
                    RunExpandedSummoningTrampleContact(triceratops,
                        fixture.Hostile, "next-round-contact", 10);
                bool nextRound = next.Saves.Count == 1 &&
                    next.Damage.Count == 1;

                UnitEntityData second = CastExpandedSummoningQuietUnit(
                    fixture, ArmoredDinosaurRulesPolicy.TriceratopsKey);
                owned.Add(second);
                PrepareExpandedSummoningTrampleResponsePair(fixture, second,
                    fixture.Hostile, 0, 1f);
                ExpandedSummoningTrampleContactResult independent =
                    RunExpandedSummoningTrampleContact(second,
                        fixture.Hostile, "independent-trampler", 10);
                bool independentSource = independent.Saves.Count == 1 &&
                    independent.Damage.Count == 1;

                UnitEntityData live = CastExpandedSummoningQuietUnit(
                    fixture, ArmoredDinosaurRulesPolicy.TriceratopsKey);
                owned.Add(live);
                BlueprintCharacterClass animal = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library,
                        "4cd1757a0eea7694ba5c933729a53920",
                        "native animal racial hit-die class");
                live.Stats.Strength.BaseValue += 4;
                live.Descriptor.Progression.AddClassLevel(animal);
                live.Descriptor.Progression.AddClassLevel(animal);
                PrepareExpandedSummoningTrampleResponsePair(fixture, live,
                    fixture.Hostile, 0, 1f);
                ExpandedSummoningTrampleContactResult liveMath =
                    RunExpandedSummoningTrampleContact(live,
                        fixture.Hostile, "live-strength-hd", 10);
                BaseDamage liveDamage = liveMath.Damage.Count == 1 ?
                    liveMath.Damage[0].DamageBundle.First : null;
                bool liveExact = live.Descriptor.Progression.GetClassLevel(
                        animal) == 16 && live.Stats.Strength.Bonus == 10 &&
                    liveMath.Saves.Count == 1 &&
                    liveMath.Saves[0].DifficultyClass == 28 &&
                    liveDamage != null && liveDamage.Bonus == 15;

                UnitEntityData oversizedTarget =
                    CastExpandedSummoningQuietUnit(fixture, "wolf",
                        fixture.Hostile);
                owned.Add(oversizedTarget);
                PrepareExpandedSummoningTrampleResponsePair(fixture,
                    triceratops, oversizedTarget, 0, 1f);
                oversizedTarget.Descriptor.State.Size = Size.Colossal;
                ExpandedSummoningTrampleContactResult oversized =
                    RunExpandedSummoningTrampleContact(triceratops,
                        oversizedTarget, "oversized", 10);
                bool sizeLegal = oversized.Saves.Count == 0 &&
                    oversized.Damage.Count == 0;

                UnitEntityData aooTarget = CastExpandedSummoningQuietUnit(
                    fixture, "wolf", fixture.Hostile);
                owned.Add(aooTarget);
                if (!aooTarget.Descriptor.HasFact(
                        BlueprintBootstrap.BodyguardFeats.CombatReflexes))
                    aooTarget.Descriptor.AddFact(
                        BlueprintBootstrap.BodyguardFeats.CombatReflexes);
                aooTarget.Stats.BaseAttackBonus.BaseValue = -100;
                UnitEntityData aooTrampler = CastExpandedSummoningQuietUnit(
                    fixture, ArmoredDinosaurRulesPolicy.TriceratopsKey);
                owned.Add(aooTrampler);
                PrepareExpandedSummoningTrampleResponsePair(fixture,
                    aooTrampler, aooTarget, 1, 1f);
                ExpandedSummoningTrampleContactResult aoo =
                    RunExpandedSummoningTrampleContact(aooTrampler,
                        aooTarget, "opportunity", 10);
                bool aooExact = aoo.Opportunities.Count == 1 &&
                    aoo.Opportunities[0].AttackBonusPenalty == 4 &&
                    aoo.OpportunityBefore == 1 &&
                    aoo.OpportunityAfter == 0 && aoo.Saves.Count == 0 &&
                    aoo.Damage.Count == 1;

                UnitEntityData lethalTarget =
                    CastExpandedSummoningQuietUnit(fixture, "wolf",
                        fixture.Hostile);
                UnitEntityData laterTarget =
                    CastExpandedSummoningQuietUnit(fixture, "wolf",
                        fixture.Hostile);
                UnitEntityData lethalTrampler =
                    CastExpandedSummoningQuietUnit(fixture,
                        ArmoredDinosaurRulesPolicy.TriceratopsKey);
                owned.Add(lethalTarget);
                owned.Add(laterTarget);
                owned.Add(lethalTrampler);
                if (!lethalTarget.Descriptor.HasFact(
                        BlueprintBootstrap.BodyguardFeats.CombatReflexes))
                    lethalTarget.Descriptor.AddFact(
                        BlueprintBootstrap.BodyguardFeats.CombatReflexes);
                lethalTarget.Stats.BaseAttackBonus.BaseValue = 100;
                PrepareExpandedSummoningTrampleResponsePair(fixture,
                    lethalTrampler, lethalTarget, 1, 1f);
                // Preparation resets the disposable trampler by design.  Put
                // it at one hit point only after that reset so the real native
                // opportunity attack can discriminate interruption.
                lethalTrampler.Descriptor.Damage += Math.Max(0,
                    lethalTrampler.HPLeft - 1);
                ExpandedSummoningTrampleContactResult lethal =
                    RunExpandedSummoningTrampleContact(lethalTrampler,
                        lethalTarget, "lethal-opportunity", 10);
                PrepareExpandedSummoningTrampleResponsePair(fixture,
                    lethalTrampler, laterTarget, 1, 1f, false, false);
                int laterDamageBefore = laterTarget.Descriptor.Damage;
                ExpandedSummoningTrampleContactResult afterLethal =
                    RunExpandedSummoningTrampleContact(lethalTrampler,
                        laterTarget, "after-lethal-opportunity", 10);
                bool stopped = lethalTrampler.Destroyed ||
                    lethalTrampler.Descriptor.State.IsDead ||
                    !lethalTrampler.Descriptor.State.IsConscious ||
                    !lethalTrampler.Descriptor.State.CanAct ||
                    !lethalTrampler.Descriptor.State.CanMove ||
                    lethal.TramplerHpAfter <= 0;
                bool lethalExact = lethal.Opportunities.Count == 1 &&
                    lethal.Opportunities[0].AttackRoll != null &&
                    lethal.Opportunities[0].AttackRoll.IsHit && stopped &&
                    lethal.Saves.Count == 0 && lethal.Damage.Count == 0 &&
                    lethal.TargetDamageAfter == lethal.TargetDamageBefore &&
                    afterLethal.Opportunities.Count == 0 &&
                    afterLethal.Saves.Count == 0 &&
                    afterLethal.Damage.Count == 0 &&
                    laterTarget.Descriptor.Damage == laterDamageBefore;

                UnitEntityData aurochs = CastExpandedSummoningQuietUnit(
                    fixture, "aurochs");
                owned.Add(aurochs);
                UnitEntityData d6Target = CastExpandedSummoningQuietUnit(
                    fixture, "wolf", fixture.Hostile);
                owned.Add(d6Target);
                d6Target.Stats.SaveReflex.BaseValue = -100;
                PrepareExpandedSummoningTrampleResponsePair(fixture,
                    aurochs, d6Target, 0, 1f);
                ExpandedSummoningTrampleContactResult d6 =
                    RunExpandedSummoningTrampleContact(aurochs, d6Target,
                        "released-d6-control", 10);
                BaseDamage d6Damage = d6.Damage.Count == 1 ?
                    d6.Damage[0].DamageBundle.First : null;
                bool d6Exact = d6.Saves.Count == 1 &&
                    d6.Saves[0].DifficultyClass == 17 &&
                    d6Damage != null && d6Damage.Dice.Dice == DiceType.D6 &&
                    d6Damage.Bonus == 9;

                bool exact = baseline && nextRound && independentSource &&
                    liveExact && sizeLegal && aooExact && lethalExact &&
                    d6Exact;
                Sprint23Check("trample-contact-matrix", exact,
                    new JObject { ["unit"] = triceratops.UniqueId,
                        ["event"] = "exact-contact-response-and-round-ledger",
                        ["baseline"] = Sprint23TrampleResult(first),
                        ["duplicate"] = Sprint23TrampleResult(duplicate),
                        ["nextRound"] = Sprint23TrampleResult(next),
                        ["independent"] = Sprint23TrampleResult(independent),
                        ["liveMath"] = Sprint23TrampleResult(liveMath),
                        ["oversized"] = Sprint23TrampleResult(oversized),
                        ["opportunity"] = Sprint23TrampleResult(aoo),
                        ["lethalOpportunity"] =
                            Sprint23TrampleResult(lethal),
                        ["afterLethal"] =
                            Sprint23TrampleResult(afterLethal),
                        ["lethalStopped"] = stopped,
                        ["releasedD6Control"] = Sprint23TrampleResult(d6) },
                    "Triceratops uses d8/live Strength/live HD, one response and one contact per source-target-round; size, lethal interruption, independent-source, next-round and released d6 controls discriminate correctly.");
            }
            finally
            {
                _rulesFixture = rulesBefore;
                fixture.Hostile.Stats.SaveReflex.BaseValue = reflexBefore;
                fixture.Hostile.Descriptor.State.Size = sizeBefore;
                Game.Instance.Player.GameTime = timeBefore;
                DisposeExpandedSummoningUnits(fixture.Created, owned);
            }
        }

        private static RuleAttackWithWeapon Sprint23TailStrike(
            UnitEntityData owner, UnitEntityData target,
            ItemEntityWeapon tail, int natural)
        {
            UnityEngine.Random.InitState(FindNativeD20Seed(natural));
            return Rulebook.Trigger(new RuleAttackWithWeapon(owner, target,
                tail, 0));
        }

        private static JObject Sprint23TailTripCase(
            RuleAttackWithWeapon strike,
            IEnumerable<RuleCombatManeuver> checks,
            UnitEntityData owner, UnitEntityData target)
        {
            return new JObject {
                ["attackHit"] = strike != null && strike.AttackRoll != null &&
                    strike.AttackRoll.IsHit,
                ["targetProne"] = target.Descriptor.State.HasCondition(
                    UnitCondition.Prone),
                ["ownerProne"] = owner.Descriptor.State.HasCondition(
                    UnitCondition.Prone),
                ["maneuvers"] = new JArray(checks.Select(value =>
                    new JObject {
                        ["initiator"] = value.Initiator?.UniqueId,
                        ["target"] = value.Target?.UniqueId,
                        ["type"] = value.Type.ToString(),
                        ["roll"] = value.InitiatorRoll.Value,
                        ["cmb"] = value.InitiatorCMB,
                        ["cmd"] = value.TargetCMD,
                        ["autoFailure"] = value.AutoFailure,
                        ["engineSuccess"] = value.Success,
                        ["success"] = SummonManeuverChecks.Succeeded(value) }))
            };
        }

        private void Sprint23TailTripMatrix(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            ItemEntityWeapon tail = Sprint19Limbs(owner).Single();
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int cmbBefore = owner.Stats.AdditionalCMB.BaseValue;
            int damageBefore = target.Descriptor.Damage;
            Size sizeBefore = target.Descriptor.State.Size;
            var observer = new Sprint16RuleObserver {
                Owner = owner, Target = target };
            EventBus.Subscribe(observer);
            try
            {
                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                target.Descriptor.State.RemoveConditionAll(UnitCondition.Prone);
                owner.Descriptor.State.RemoveConditionAll(UnitCondition.Prone);
                observer.Clear();
                observer.BeforeCombatManeuverForFixture = value =>
                    value.AdditionalBonus += 1000;
                RuleAttackWithWeapon success = Sprint23TailStrike(owner,
                    target, tail, 10);
                bool successful = success.AttackRoll.IsHit &&
                    observer.Checks.Count == 1 &&
                    observer.Checks[0].Type == CombatManeuver.Trip &&
                    SummonManeuverChecks.Succeeded(observer.Checks[0]) &&
                    target.Descriptor.State
                        .HasCondition(UnitCondition.Prone);
                JObject successDetail = Sprint23TailTripCase(success,
                    observer.Checks.ToArray(), owner, target);

                target.Descriptor.State.RemoveConditionAll(UnitCondition.Prone);
                owner.Descriptor.State.RemoveConditionAll(UnitCondition.Prone);
                observer.Clear();
                observer.BeforeCombatManeuverForFixture = value =>
                    value.AutoFailure = true;
                RuleAttackWithWeapon failure = Sprint23TailStrike(owner,
                    target, tail, 10);
                bool failed = failure.AttackRoll.IsHit &&
                    observer.Checks.Count == 1 &&
                    !SummonManeuverChecks.Succeeded(observer.Checks[0]) &&
                    !target.Descriptor.State.HasCondition(UnitCondition.Prone) &&
                    !owner.Descriptor.State.HasCondition(UnitCondition.Prone);
                JObject failureDetail = Sprint23TailTripCase(failure,
                    observer.Checks.ToArray(), owner, target);

                observer.BeforeCombatManeuverForFixture = null;
                owner.Stats.AdditionalAttackBonus.BaseValue = -100;
                observer.Clear();
                RuleAttackWithWeapon miss = Sprint23TailStrike(owner,
                    target, tail, 10);
                bool missed = !miss.AttackRoll.IsHit &&
                    observer.Checks.Count == 0;

                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                observer.Clear();
                target.Descriptor.State.AddCondition(
                    UnitCondition.ImmuneToCombatManeuvers, null);
                Sprint23TailStrike(owner, target, tail, 10);
                bool immune = observer.Checks.Count == 0;
                target.Descriptor.State.RemoveConditionAll(
                    UnitCondition.ImmuneToCombatManeuvers);

                observer.Clear();
                target.Descriptor.State.Size = Size.Colossal;
                Sprint23TailStrike(owner, target, tail, 10);
                bool oversized = observer.Checks.Count == 0;
                target.Descriptor.State.Size = sizeBefore;

                observer.Clear();
                target.Descriptor.State.AddCondition(UnitCondition.Prone,
                    null);
                Sprint23TailStrike(owner, target, tail, 10);
                bool alreadyProne = observer.Checks.Count == 0;
                target.Descriptor.State.RemoveConditionAll(UnitCondition.Prone);

                observer.Clear();
                observer.BeforeCombatManeuverForFixture = value =>
                    value.AutoFailure = true;
                RuleAttackWithWeapon once = Sprint23TailStrike(owner,
                    target, tail, 10);
                StegosaurusTailTrip rider = owner.Descriptor.Progression
                    .Features.Enumerable.SelectMany(value =>
                        value.SelectComponents<StegosaurusTailTrip>())
                    .Single();
                rider.OnEventDidTrigger(once);
                rider.OnEventDidTrigger(once);
                bool duplicate = observer.Checks.Count == 1;
                Sprint23TailStrike(owner, target, tail, 10);
                bool separate = observer.Checks.Count == 2;

                observer.BeforeCombatManeuverForFixture = null;
                BlueprintItemWeapon biteBlueprint = fixture.Blueprints
                    .OfType<BlueprintItemWeapon>().First(value =>
                        value.Type != null && value.Type.Category ==
                            WeaponCategory.Bite);
                observer.Clear();
                Sprint23TailStrike(owner, target,
                    new ItemEntityWeapon(biteBlueprint), 10);
                bool otherWeapon = observer.Checks.Count == 0;

                string[] facts = owner.Descriptor.Progression.Features
                    .Enumerable.Select(value => value.Blueprint.name)
                    .ToArray();
                bool noAnkylosaurusRider = !facts.Any(value =>
                    value != null && value.Contains("Ankylosaurus")) &&
                    !owner.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(
                        value => value.Blueprint.AssetGuid ==
                            "74654177d90e4d1d8fd512c0dabe3966" ||
                        value.Blueprint.AssetGuid ==
                            "1f95fa2f17b042df8065a28823e09fc3");
                bool exact = successful && failed && missed && immune &&
                    oversized && alreadyProne && duplicate && separate &&
                    otherWeapon && noAnkylosaurusRider;
                Sprint23Check("tail-trip-matrix-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject { ["unit"] = owner.UniqueId,
                        ["event"] = "exact-tail-native-trip",
                        ["success"] = successful, ["failure"] = failed,
                        ["successDetail"] = successDetail,
                        ["failureDetail"] = failureDetail,
                        ["miss"] = missed, ["immune"] = immune,
                        ["oversized"] = oversized,
                        ["alreadyProne"] = alreadyProne,
                        ["duplicateSuppressed"] = duplicate,
                        ["separateStrikes"] = separate,
                        ["otherWeapon"] = otherWeapon,
                        ["noAnkylosaurusRider"] = noAnkylosaurusRider,
                        ["facts"] = new JArray(facts) },
                    "Each genuine exact tail hit starts at most one native Trip; miss, failure, immunity, illegal size, prone state, duplicate callback and foreign weapon are discriminated without backlash or Ankylosaurus control effects.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                owner.Stats.AdditionalCMB.BaseValue = cmbBefore;
                target.Descriptor.Damage = damageBefore;
                owner.Descriptor.State.RemoveConditionAll(UnitCondition.Prone);
                target.Descriptor.State.RemoveConditionAll(
                    UnitCondition.ImmuneToCombatManeuvers);
                target.Descriptor.State.RemoveConditionAll(UnitCondition.Prone);
                target.Descriptor.State.Size = sizeBefore;
            }
        }

        private void Sprint23AnkylosaurusControl(
            ExpandedSummoningCorrectionFixture fixture)
        {
            UnitEntityData owner = CastExpandedSummoningOwnTier(fixture,
                "ankylosaurus");
            SetExpandedSummoningBrainActive(owner, false);
            string[] facts = owner.Descriptor.Progression.Features.Enumerable
                .Select(value => value.Blueprint.name).ToArray();
            bool exact = Sprint19Limbs(owner).Length == 1 &&
                facts.Contains("KMG_Summoning_Natural_Ankylosaurus_Chassis") &&
                facts.Contains("KMG_Summoning_Special_Ankylosaurus_Stun") &&
                !facts.Any(value => value != null &&
                    value.Contains("Stegosaurus"));
            Sprint23Check("ankylosaurus-negative-control", exact,
                new JObject { ["unit"] = owner.UniqueId,
                    ["event"] = "released-tail-control-identity",
                    ["owner"] = owner.Blueprint.AssetGuid,
                    ["facts"] = new JArray(facts) },
                "Released Ankylosaurus retains its exact chassis/stun graph and gains no Stegosaurus rider.");
            DisposeExpandedSummoningUnits(fixture.Created,
                new[] { owner });
        }
    }
}
