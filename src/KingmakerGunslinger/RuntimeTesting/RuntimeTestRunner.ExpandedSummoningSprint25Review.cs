using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using TurnBased.Controllers;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-owned first-area observations. Native clocks, rule capture,
    // command stepping and teardown remain the qualified Sprint 22-24 paths.
    internal sealed partial class RuntimeTestRunner
    {
        private ExpandedSummoningCorrectionFixture _sprint25Fixture;
        private Sprint22NativeClock _sprint25Clock;
        private IEnumerator<int> _sprint25Steps;
        private bool _sprint25WaitingForUnityAppearance;
        private readonly List<RuntimeTestAssertion> _sprint25Assertions =
            new List<RuntimeTestAssertion>();
        private readonly JArray _sprint25Rows = new JArray();

        private void Sprint25Check(string predicate, bool passed,
            JObject row, string expected)
        {
            row["predicate"] = predicate;
            row["passed"] = passed;
            row["runId"] = _request.RunId;
            _sprint25Rows.Add(row);
            _sprint25Assertions.Add(Assertion("sprint25-" + predicate,
                expected, row.ToString(Formatting.None), passed,
                "Exact request-owned giant-body units in the native first-area fixture."));
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                _request.EvidenceDirectory, "sprint25-review-progress.json"),
                _sprint25Rows.ToString(Formatting.Indented));
        }

        private bool PollSprint25NativeReview()
        {
            try
            {
                if (_sprint25Steps == null)
                {
                    _sprint25Fixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_Sprint25_Caster");
                    _sprint25Clock = new Sprint22NativeClock(_sprint25Fixture);
                    _sprint25Steps = ReviewSprint25(_sprint25Fixture).GetEnumerator();
                }
                if (!_sprint25WaitingForUnityAppearance) _sprint25Clock.Pulse();
                if (_sprint25Steps.MoveNext()) return false;
            }
            catch (Exception error)
            {
                Sprint25Check("execution", false, new JObject {
                    ["exception"] = error.ToString()
                }, "The selected phase completes without setup or observer failure.");
            }
            CleanupSprint25NativeReview();
            string report = Path.Combine(_request.EvidenceDirectory,
                "sprint25-review.json");
            RuntimeTestResultWriter.WriteAtomic(report,
                _sprint25Rows.ToString(Formatting.Indented));
            _summoningProfileResult = CreateResult(
                _sprint25Assertions.Count > 1 && _sprint25Assertions.All(
                    value => value.Status == RuntimeTestStatuses.Pass)
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                _sprint25Assertions, null);
            _summoningProfileResult.EvidenceFiles.Add(report);
            return true;
        }

        private void CleanupSprint25NativeReview()
        {
            // Disposing the iterator executes its command/settings finally
            // blocks before the shared exact-owned fixture is dismantled.
            try { _sprint25Steps?.Dispose(); }
            finally
            {
                _sprint25Steps = null;
                _sprint25WaitingForUnityAppearance = false;
                if (_sprint25Fixture != null)
                {
                    bool cleaned = false, clockRestored = false;
                    try
                    {
                        EndExpandedSummoningCorrectionFixture(_sprint25Fixture,
                            out cleaned);
                    }
                    finally
                    {
                        if (_sprint25Clock != null)
                        {
                            _sprint25Clock.Dispose();
                            clockRestored = _sprint25Clock.Restored;
                            _sprint25Clock = null;
                        }
                        _sprint25Fixture = null;
                    }
                    Sprint25Check("fixture-restored", cleaned && clockRestored,
                        new JObject { ["referencesRestored"] = cleaned,
                            ["clockRestored"] = clockRestored },
                        "Original world, party, area references and request-local clock are restored.");
                }
                else { _sprint25Clock?.Dispose(); _sprint25Clock = null; }
            }
        }

        private IEnumerable<int> ReviewSprint25(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            string phase = RuntimeTestRequestParser.Sprint25ReviewPhase(_request);
            if (phase == null) throw new InvalidOperationException("Invalid Sprint 25 phase.");
            try
            {
                Sprint25Check("phase", true, new JObject { ["phase"] = phase },
                    "One bounded, validated native review phase is selected.");
                Game.Instance.IsPaused = true;
                CreateExpandedSummoningCorrectionHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                if (phase == "routes") { Sprint25Routes(fixture); yield break; }
                if (phase == "overrun")
                {
                    foreach (int frame in Sprint25Overrun(fixture)) yield return frame;
                    yield break;
                }
                if (phase == "great-cleave")
                {
                    foreach (bool cleaveTurnBased in new[] { false, true })
                        foreach (int frame in Sprint25Cleave(fixture, cleaveTurnBased)) yield return frame;
                    yield break;
                }
                if (phase == "visual-lifecycle")
                {
                    foreach (int frame in Sprint24VisualLifecycle(fixture,
                        new[] { "brachiosaurus", "elephant" }, true,
                        (name, passed, row, expected) => {
                            string id = (string)row["unit"];
                            UnitEntityData actor = fixture.Created.FirstOrDefault(value => value.UniqueId == id && value.View != null);
                            if (actor != null) row["partyCamera"] = WriteExpandedSummoningPartyCameraCapture(
                                actor, _request.EvidenceDirectory, "sprint25-" + name + ".png");
                            Sprint25Check(name, passed, row, expected);
                        }, () => Sprint25VisualSetup(fixture))) yield return frame;
                    yield break;
                }
                if (phase == "rtwp-ai")
                {
                    foreach (string key in new[] { "brachiosaurus", "elephant" })
                        foreach (int frame in Sprint24AiFallback(fixture, key,
                            key == "brachiosaurus" ? 10.5f : 6f,
                            Sprint25Check, true)) yield return frame;
                    yield break;
                }
                bool turnBased = phase == "turn-based-commands";
                foreach (string key in new[] { "brachiosaurus", "elephant" })
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(fixture, key);
                    SetExpandedSummoningBrainActive(owner, false);
                    try
                    {
                        foreach (int frame in Sprint25NativeAppearance(owner, key)) yield return frame;
                        if (!turnBased)
                        {
                            foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                            Sprint25Profile(fixture, owner, key);
                            foreach (int frame in Sprint25PowerAttack(owner, key, false)) yield return frame;
                            Sprint25Body(owner, key, "idle");
                            foreach (int frame in Sprint25SignedStrength(fixture, owner, key)) yield return frame;
                        }
                        else
                        {
                            var admission = new JObject();
                            foreach (int frame in Sprint22EnrollTurnBasedOwners(
                                new[] { owner }, fixture.Hostile, admission)) yield return frame;
                            if (!(bool)admission["admitted"])
                                throw new InvalidOperationException("Giant owner not admitted to native turn order: " + admission);
                        }
                        foreach (int frame in Sprint25OrdinaryAttack(fixture,
                            owner, key, turnBased)) yield return frame;
                    }
                    finally
                    {
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                        Game.Instance.TurnBasedCombatController.Activate();
                        owner.Commands.InterruptAll(true);
                        if (owner.IsInCombat) owner.LeaveCombat();
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    }
                    foreach (int frame in Sprint23NativeTrampleCommand(fixture,
                        turnBased, key, key == "brachiosaurus" ? 32 : 25, 2,
                        key == "brachiosaurus" ? DiceType.D6 : DiceType.D8,
                        key == "brachiosaurus" ? 19 : 15, Sprint25Check,
                        (unit, frame) => Sprint25Body(unit, key, "trample-" + turnBased + "-" + frame))) yield return frame;
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                    Game.Instance.TurnBasedCombatController.Activate();
                }
                if (!turnBased) Sprint25TriceratopsControl(fixture);
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = mode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = pause;
            }
        }

        private IEnumerable<int> Sprint25OrdinaryAttack(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            string key, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            UnityEngine.Random.State random = UnityEngine.Random.state;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            var observer = new Sprint24AttackObserver {
                Owner = owner, Target = target, BeforeRoll = value => {
                    owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                    UnityEngine.Random.InitState(FindNativeD20Seed(18));
                }
            };
            var costRows = new JArray();
            FirearmReviewCosts costs = null;
            EventBus.Subscribe(observer);
            try
            {
                costs = new FirearmReviewCosts(owner, costRows);
                float reach = (key == "brachiosaurus" ? 20 : 10) * .3048f;
                float distanceOffset = owner.Corpulence + target.Corpulence;
                // Mutual sight is 8.5 m for a peaceful hostile, 11.7 m in
                // combat. The 20-foot boundary plus actor radii exceeds the
                // former. Establish combat, not a vision override.
                owner.Commands.InterruptAll(true);
                target.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                target.Commands.RemoveFinishedAndUpdateQueue();
                owner.JoinCombat(); target.JoinCombat();
                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target,
                    reach + distanceOffset + .2f, out far, out lane, out direction);
                Vector3 forward = (far - origin).normalized;
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, far);
                var range = new UnitAttack(target);
                range.Init(owner);
                PlaceExpandedSummoningUnit(target, origin + forward *
                    (reach + distanceOffset - .05f));
                float inside = owner.DistanceTo(target);
                bool insideAccepted = range.IsUnitEnoughClose;
                PlaceExpandedSummoningUnit(target, origin + forward *
                    (reach + distanceOffset + .05f));
                float outside = owner.DistanceTo(target);
                bool outsideAccepted = range.IsUnitEnoughClose;
                Sprint25Check("reach-boundary-" + key + "-" + turnBased,
                    Math.Abs(range.ApproachRadius - distanceOffset - reach) < .02f &&
                    Math.Abs(owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Meters - reach) < .02f &&
                    inside < range.ApproachRadius && outside > range.ApproachRadius && insideAccepted && !outsideAccepted,
                    new JObject { ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId, ["nativeApproachRadius"] = range.ApproachRadius,
                        ["insideNativeDistance"] = inside, ["outsideNativeDistance"] = outside,
                        ["ownerCorpulence"] = owner.Corpulence, ["targetCorpulence"] = target.Corpulence,
                        ["nativeBodyRadiusAddition"] = distanceOffset,
                        ["legalLaneSurvey"] = lane,
                        ["nativeInsideAccepted"] = insideAccepted, ["nativeOutsideAccepted"] = outsideAccepted,
                        ["weaponRange"] = owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Meters },
                    "The actual UnitAttack approach radius and engine distance convention discriminate the printed reach boundary.");
                PlaceExpandedSummoningUnit(target, origin + forward * 2);
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.JoinCombat(); target.JoinCombat();
                if (!owner.IsInCombat || !target.IsInCombat)
                    throw new InvalidOperationException("Ordinary attack combat prerequisites are unmet.");
                bool combatThroughout = owner.IsInCombat;
                float standardBefore = owner.CombatState.Cooldown.StandardAction;
                float moveBefore = owner.CombatState.Cooldown.MoveAction;
                float maxStandard = standardBefore, maxMove = moveBefore;
                UnitAttack command = null;
                foreach (int frame in Sprint24RunAttackCommand(fixture, owner,
                    target, true, value => command = value,
                    (unit, step) => Sprint25Body(unit, key, "attack-" + turnBased + "-" + step)))
                {
                    maxStandard = Math.Max(maxStandard, owner.CombatState.Cooldown.StandardAction);
                    maxMove = Math.Max(maxMove, owner.CombatState.Cooldown.MoveAction);
                    combatThroughout &= owner.IsInCombat;
                    yield return frame;
                }
                maxStandard = Math.Max(maxStandard, owner.CombatState.Cooldown.StandardAction);
                maxMove = Math.Max(maxMove, owner.CombatState.Cooldown.MoveAction);
                combatThroughout &= owner.IsInCombat;
                int count = key == "brachiosaurus" ? 1 : 2;
                bool exact = command != null && command.IsFinished &&
                    combatThroughout && costs.Count(command) == 1 && costs.Exact(command, turnBased) &&
                    command.AllAttacks.Count == count && observer.WeaponAttacks.Count == count &&
                    observer.Rolls.Count == count && observer.Rolls.All(value => value.IsHit) &&
                    observer.WeaponAttacks.All(value => !value.IsCharge && !value.IsAttackOfOpportunity);
                Sprint25Check("ordinary-command-" + key + "-" + turnBased,
                    exact, new JObject { ["unit"] = owner.UniqueId, ["target"] = target.UniqueId,
                        ["mode"] = turnBased ? "turn-based" : "rtwp",
                        ["commandFinished"] = command?.IsFinished,
                        ["commandResult"] = command?.Result.ToString(),
                        ["plannedAttacks"] = command?.AllAttacks.Count,
                        ["rolls"] = observer.Rolls.Count,
                        ["weapons"] = new JArray(observer.WeaponAttacks.Select(value => value.Weapon.Blueprint.AssetGuid)),
                        ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)),
                        ["standardCooldown"] = new JArray(standardBefore, maxStandard, owner.CombatState.Cooldown.StandardAction),
                        ["moveCooldown"] = new JArray(moveBefore, maxMove, owner.CombatState.Cooldown.MoveAction),
                        ["nativeEconomy"] = !command.Cutscene && !command.IsIgnoreCooldown,
                        ["combatThroughout"] = combatThroughout,
                        ["nativeCostApplications"] = costs.Count(command),
                        ["nativeCostExact"] = costs.Exact(command, turnBased),
                        ["nativeCostBoundary"] = costRows },
                    "The real ordinary command executes exactly the frozen routine and crosses the native cost boundary exactly once with the engine's elapsed-time/mode charge, in combat, without a cutscene or cooldown bypass.");
                Sprint25Body(owner, key, "ordinary-attack-" + turnBased);
            }
            finally
            {
                try { costs?.Dispose(); }
                finally
                {
                    EventBus.Unsubscribe(observer);
                    owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                    owner.Commands.InterruptAll(true);
                    owner.Commands.RemoveFinishedAndUpdateQueue();
                    UnityEngine.Random.state = random;
                    ResetExpandedSummoningHostile(fixture);
                }
            }
        }

        private void Sprint25Routes(ExpandedSummoningCorrectionFixture fixture)
        {
            SummonVariantSpec[] routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(value => GiantBodyRulesPolicy.IsGiantBody(value.Creature.Key)).ToArray();
            SummonVariantSpec[] brach = routes.Where(value => value.Creature.Key == "brachiosaurus").ToArray();
            bool exact = routes.Length == 11 && brach.Length == 3 &&
                brach.All(value => value.Family == SummonFamily.NaturesAlly) &&
                routes.Count(value => value.Creature.Key == "elephant") == 8 &&
                routes.Where(value => value.Creature.Key == "elephant").All(SummonVisibilityCatalog.IsPublished);
            Sprint25Check("route-census", exact, new JObject { ["routes"] = new JArray(routes.Select(value => value.StableKey)),
                ["published"] = new JArray(routes.Where(SummonVisibilityCatalog.IsPublished).Select(value => value.StableKey)) },
                "Exactly three SNA-only Brachiosaurus and eight unchanged public Elephant roots exist.");
            Sprint25IconConsumers(fixture, routes);
            foreach (SummonVariantSpec route in routes.Where(value => !SummonVisibilityCatalog.IsPublished(value)))
            {
                UnitEntityData[] units = CastExpandedSummoningVariant(fixture.Blueprints,
                    fixture.Caster, route, null, fixture.Evidence);
                fixture.Created.AddRange(units);
                int low = route.Multiplicity == SummonMultiplicity.OneD4PlusOne ? 2 : 1;
                int high = route.Multiplicity == SummonMultiplicity.One ? 1 :
                    route.Multiplicity == SummonMultiplicity.OneD3 ? 3 : 5;
                Sprint25Check("hidden-route-" + route.StableKey,
                    units.Length >= low && units.Length <= high &&
                    units.All(value => value.Blueprint.name == GiantBodyVisualPolicy.BrachiosaurusBlueprintName && Sprint24TemplateCount(value) == 0),
                    new JObject { ["route"] = route.StableKey,
                        ["units"] = new JArray(units.Select(Sprint22UnitReadback)),
                        ["execution"] = _expandedSummoningLastAbilityExecution },
                    "The registered hidden SNA route produces only its exact untemplated unit and legal quantity; public slot/selection proof remains separate.");
                DisposeExpandedSummoningUnits(fixture.Created, units);
            }
            Sprint24PlayerRoutes(fixture, routes.Where(SummonVisibilityCatalog.IsPublished).ToArray(), Sprint25Check, true);
        }

        private void Sprint25Profile(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key)
        {
            bool brach = key == "brachiosaurus";
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            var attacks = new JArray();
            bool exact = limbs.Length == (brach ? 1 : 2);
            for (int index = 0; index < limbs.Length; index++)
            {
                RuleCalculateWeaponStats stats = Rulebook.Trigger(
                    new RuleCalculateWeaponStats(owner, limbs[index], null));
                BaseDamage damage = stats.DamageDescription[0].CreateDamage();
                int attack = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(
                    owner, limbs[index], 0)).Result;
                bool row = !limbs[index].IsSecondary && attack == (brach ? 23 : 16) &&
                    damage.Dice.Rolls == (brach ? 4 : 2) &&
                    damage.Dice.Dice == (brach || index == 1 ? DiceType.D6 : DiceType.D8) &&
                    damage.Bonus == (brach ? 19 : 10);
                exact &= row;
                attacks.Add(new JObject { ["weapon"] = limbs[index].Blueprint.AssetGuid,
                    ["attack"] = attack, ["secondary"] = limbs[index].IsSecondary,
                    ["damage"] = Sprint16DamageLine(damage), ["exact"] = row });
            }
            Sprint14CmdBreakdown ordinary = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.Grapple);
            Sprint14CmdBreakdown trip = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.Trip);
            string[] elephantFeats = { "54ee847996c25cd4ba8773d7b8555174", "79042cb55f030614ea29956177977c52",
                "b3614622866fe7046b787a548bbd7f59", "175d1577bb6c9a04baf88eec99c66334",
                "9972f33f977fc724c838e59641b2fca5", "f74c6bdf5c5f5374fb9302ecdc1f7d64" };
            string[] granted = owner.Descriptor.Progression.Features.Enumerable.Select(value => value.Blueprint.AssetGuid).ToArray();
            int bullRush = Rulebook.Trigger(new RuleCalculateCMB(owner, fixture.Hostile, CombatManeuver.BullRush)).Result;
            int ordinaryCmb = Rulebook.Trigger(new RuleCalculateCMB(owner, fixture.Hostile, CombatManeuver.Trip)).Result;
            exact &= owner.Stats.Strength.Bonus == (brach ? 13 : 10) &&
                owner.Stats.HitPoints.ModifiedValue == (brach ? 171 : 93) &&
                owner.Descriptor.Progression.CharacterLevel == (brach ? 18 : 11) &&
                limbs.All(value => value.AttackRange.Value == (brach ? 20 : 10)) &&
                owner.Descriptor.State.Size == (brach ? Size.Gargantuan : Size.Huge) &&
                trip.Result - ordinary.Result == 4 &&
                bullRush - ordinaryCmb == 2 &&
                owner.Stats.SaveFortitude.ModifiedValue == (brach ? 18 : 13) &&
                owner.Stats.SaveWill.ModifiedValue == (brach ? 9 : 6) &&
                owner.Stats.SkillPerception.ModifiedValue == (brach ? 28 : 21) &&
                (brach || elephantFeats.All(granted.Contains));
            Sprint25Check("profile-" + key, exact, new JObject {
                ["unit"] = owner.UniqueId, ["blueprint"] = owner.Blueprint.AssetGuid,
                ["strength"] = owner.Stats.Strength.ModifiedValue,
                ["hd"] = owner.Descriptor.Progression.CharacterLevel,
                ["hp"] = owner.Stats.HitPoints.ModifiedValue,
                ["reachFeet"] = owner.Stats.Reach.ModifiedValue,
                ["effectiveWeaponReachFeet"] = new JArray(limbs.Select(value => value.AttackRange.Value)),
                ["reachConvention"] = "Native Reach stat is an intermediate; ItemEntityWeapon reach is measured separately from UnitAttack's added actor radii.",
                ["size"] = owner.Descriptor.State.Size.ToString(), ["attacks"] = attacks,
                ["cmd"] = ordinary.Describe("CMD"), ["tripCmd"] = trip.Describe("trip"),
                ["cmb"] = ordinaryCmb, ["bullRushCmb"] = bullRush,
                ["fortitude"] = owner.Stats.SaveFortitude.ModifiedValue,
                ["will"] = owner.Stats.SaveWill.ModifiedValue,
                ["perception"] = owner.Stats.SkillPerception.ModifiedValue,
                ["perceptionBreakdown"] = DescribeSprint16Skill(owner.Stats.SkillPerception),
                ["difficultyAcContribution"] = owner.Stats.AC.Modifiers
                    .Where(value => value.ModDescriptor == ModifierDescriptor.Difficulty).Sum(value => value.ModValue),
                ["requiredElephantFeatGuids"] = new JArray(elephantFeats),
                ["facts"] = new JArray(owner.Descriptor.Progression.Features.Enumerable
                    .Select(value => value.Blueprint.name)) },
                "The live frozen adult routine, attack roles, Strength, HD, reach and quadruped defense match the selected profile.");
        }

        private IEnumerable<int> Sprint25SignedStrength(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key)
        {
            int before = owner.Stats.Strength.BaseValue;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int reflexBefore = fixture.Hostile.Stats.SaveReflex.BaseValue;
            UnityEngine.Random.State random = UnityEngine.Random.state;
            ExpandedSummoningCorrectionFixture rules = _rulesFixture;
            string trampleName = "KMG_Summoning_Special_" +
                (key == "brachiosaurus" ? "Brachiosaurus" : "Elephant") + "_Trample";
            var rows = new JArray();
            bool exact = true;
            var observer = new Sprint24AttackObserver { Owner = owner, Target = fixture.Hostile,
                BeforeRoll = value => UnityEngine.Random.InitState(FindNativeD20Seed(18)) };
            var toggle = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value =>
                value.Blueprint.AssetGuid == "a7b339e4f6ff93a4697df5d7a87ff619");
            EventBus.Subscribe(observer);
            try
            {
                _rulesFixture = fixture;
                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                // Independent frozen expected table; the production formula is
                // not called to calculate its own expected result.
                foreach (int[] entry in new[] { new[] { 36, 13, 19, 32, 28 },
                    new[] { 32, 11, 16, 30, 26 }, new[] { 10, 0, 0, 19, 15 }, new[] { 4, -3, -3, 16, 12 } })
                {
                    owner.Stats.Strength.BaseValue = entry[0];
                    int modifier = owner.Stats.Strength.Bonus;
                    var limbs = new JArray();
                    foreach (ItemEntityWeapon limb in Sprint19Limbs(owner))
                    {
                        foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                        JObject toggleBefore = Sprint22PowerAttackState(owner, toggle);
                        bool powerAttackAbsentBefore = owner.Descriptor.Buffs.GetBuff(toggle.Blueprint.Buff) == null;
                        var attack = new RuleAttackWithWeapon(owner, fixture.Hostile, limb, 0);
                        RuleCalculateWeaponStats stats = Rulebook.Trigger(
                            new RuleCalculateWeaponStats(owner, limb, attack));
                        BaseDamage damage = stats.DamageDescription[0].CreateDamage();
                        int expected = key == "brachiosaurus" ? entry[2] : entry[1];
                        observer.Clear();
                        Rulebook.Trigger(attack);
                        RuleDealDamage dealtWeapon = observer.Damage.FirstOrDefault();
                        exact &= modifier == entry[1] && damage.Bonus == expected &&
                            powerAttackAbsentBefore &&
                            dealtWeapon != null && dealtWeapon.DamageBundle.First.Bonus == expected;
                        limbs.Add(new JObject { ["weapon"] = limb.Blueprint.AssetGuid,
                            ["expectedBonus"] = expected, ["damage"] = Sprint16DamageLine(damage),
                            ["powerAttackBefore"] = toggleBefore,
                            ["powerAttackAfterActualAttack"] = Sprint22PowerAttackState(owner, toggle),
                            ["actualDamageRule"] = dealtWeapon == null ? null : Sprint16DamageEvent(dealtWeapon) });
                    }
                    PrepareExpandedSummoningTrampleResponsePair(fixture, owner,
                        fixture.Hostile, 0, 1f);
                    fixture.Hostile.Stats.SaveReflex.BaseValue = -100;
                    Game.Instance.Player.GameTime += TimeSpan.FromSeconds(6);
                    ExpandedSummoningTrampleContactResult trample = RunExpandedSummoningTrampleContact(
                        owner, fixture.Hostile, "sprint25-signed-" + entry[1], 10, trampleName);
                    BaseDamage dealt = trample.Damage.Count == 1 ? trample.Damage[0].DamageBundle.First : null;
                    exact &= dealt != null && dealt.Bonus == entry[2] && trample.Saves.Count == 1 &&
                        trample.Saves[0].DifficultyClass == entry[key == "brachiosaurus" ? 3 : 4];
                    rows.Add(new JObject { ["score"] = owner.Stats.Strength.ModifiedValue,
                        ["modifier"] = modifier, ["weapons"] = limbs,
                        ["trample"] = Sprint23TrampleResult(trample), ["expectedTrampleBonus"] = entry[2] });
                }
                owner.Stats.Strength.BaseValue = before;
                BlueprintCharacterClass animal = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                    BlueprintBootstrap.Library, "4cd1757a0eea7694ba5c933729a53920", "native animal racial class");
                UnitEntityData growth = CastExpandedSummoningQuietUnit(fixture, key);
                try
                {
                    growth.Descriptor.Progression.AddClassLevel(animal);
                    growth.Descriptor.Progression.AddClassLevel(animal);
                    PrepareExpandedSummoningTrampleResponsePair(fixture, growth, fixture.Hostile, 0, 1f);
                    Game.Instance.Player.GameTime += TimeSpan.FromSeconds(6);
                    var liveHd = RunExpandedSummoningTrampleContact(growth, fixture.Hostile, "sprint25-live-hd", 10, trampleName);
                    bool liveExact = liveHd.Saves.Count == 1 && liveHd.Saves[0].DifficultyClass == (key == "brachiosaurus" ? 33 : 26);
                    exact &= liveExact;
                    rows.Add(new JObject { ["case"] = "two-live-racial-HD-added", ["unit"] = growth.UniqueId,
                        ["hd"] = growth.Descriptor.Progression.CharacterLevel,
                        ["trample"] = Sprint23TrampleResult(liveHd), ["exact"] = liveExact });
                }
                finally { DisposeExpandedSummoningUnits(fixture.Created, new[] { growth }); }
            }
            finally
            {
                owner.Stats.Strength.BaseValue = before;
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                fixture.Hostile.Stats.SaveReflex.BaseValue = reflexBefore;
                EventBus.Unsubscribe(observer);
                _rulesFixture = rules;
                UnityEngine.Random.state = random;
                ResetExpandedSummoningHostile(fixture);
            }
            Sprint25Check("signed-strength-" + key, exact, new JObject {
                ["unit"] = owner.UniqueId, ["target"] = fixture.Hostile.UniqueId,
                ["cases"] = rows },
                "Actual native weapon-stat damage and production trample damage match independent odd-positive, zero and penalty Strength rows.");
        }

        private IEnumerable<int> Sprint25Movement(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach (string key in new[] { "brachiosaurus", "elephant" })
            {
                UnitEntityData unit = CastExpandedSummoningQuietUnit(fixture, key);
                try
                {
                    foreach (int frame in Sprint25NativeAppearance(unit, key)) yield return frame;
                    Vector3 destination; string survey, direction;
                    Vector3 origin = Sprint24AiCombatLane(fixture.Caster, unit,
                        fixture.Hostile, 6f, out destination, out survey, out direction);
                    PlaceExpandedSummoningUnit(unit, origin);
                    var move = new UnitMoveTo(destination, .5f);
                    move.Init(unit);
                    if (!move.CanStart) throw new InvalidOperationException("Native giant movement command refused.");
                    unit.Commands.Run(move);
                    float velocity = 0;
                    int contacts = 0, frames = 0;
                    DateTime deadline = DateTime.UtcNow.AddSeconds(25);
                    while (!move.IsFinished && frames++ < 1500 && DateTime.UtcNow < deadline)
                    {
                        velocity = Math.Max(velocity, unit.View.MovementAgent.Velocity.magnitude);
                        contacts = Math.Max(contacts, unit.View.MovementAgent.UnitContacts?.Count ?? 0);
                        if (frames == 6 || frames == 24) Sprint25Body(unit, key, "movement-" + frames);
                        yield return 0;
                    }
                    float travel = Vector3.Distance(origin, unit.Position);
                    float gap = Vector3.Distance(destination, unit.Position);
                    Sprint25Check("movement-" + key, move.IsFinished && travel > 1f && gap <= 1.5f && velocity > .01f,
                        new JObject { ["unit"] = unit.UniqueId, ["survey"] = survey,
                            ["origin"] = SurveyVector(origin), ["destination"] = SurveyVector(destination),
                            ["travel"] = travel, ["gap"] = gap, ["maximumVelocity"] = velocity,
                            ["nativeContacts"] = contacts, ["corpulence"] = unit.Corpulence,
                            ["commandFinished"] = move.IsFinished, ["frames"] = frames },
                        "The original body and native selection navigate a connected real area route with native movement, contacts and command completion.");
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                    foreach (int frame in Sprint25ConfinedMove(fixture, unit, key)) yield return frame;
                }
                finally
                {
                    unit.Commands.InterruptAll(true);
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { unit });
                }
            }
        }

        private IEnumerable<int> Sprint25ConfinedMove(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData unit, string key)
        {
            Vector3 from = Vector3.zero, to = Vector3.zero;
            float width = 0;
            bool found = false;
            foreach (float distance in new[] { 0f, 4f, 8f, 12f })
            {
                foreach (Vector3 offset in CompassOffsets)
                {
                    var centre = AstarPath.active.GetNearest(fixture.Caster.Position + offset * distance);
                    if (centre.node == null || !centre.node.Walkable) continue;
                    foreach (Vector3 direction in CompassOffsets)
                    {
                        var leftEnd = AstarPath.active.GetNearest(centre.clampedPosition - direction * 2f);
                        var rightEnd = AstarPath.active.GetNearest(centre.clampedPosition + direction * 2f);
                        if (leftEnd.node == null || rightEnd.node == null || !leftEnd.node.Walkable || !rightEnd.node.Walkable ||
                            leftEnd.node.Area != rightEnd.node.Area || Vector3.Distance(leftEnd.clampedPosition, rightEnd.clampedPosition) < 3f) continue;
                        Vector3 side = Vector3.Cross(direction, Vector3.up);
                        Vector3 left = Kingmaker.View.ObstacleAnalyzer.TraceAlongNavmesh(centre.clampedPosition, centre.clampedPosition + side * 4f);
                        Vector3 right = Kingmaker.View.ObstacleAnalyzer.TraceAlongNavmesh(centre.clampedPosition, centre.clampedPosition - side * 4f);
                        float clearance = Vector3.Distance(left, centre.clampedPosition) + Vector3.Distance(right, centre.clampedPosition);
                        Vector3 line = Kingmaker.View.ObstacleAnalyzer.TraceAlongNavmesh(leftEnd.clampedPosition, rightEnd.clampedPosition);
                        if (clearance < 1f || clearance >= 7.8f || Vector3.Distance(line, rightEnd.clampedPosition) > .25f) continue;
                        from = leftEnd.clampedPosition; to = rightEnd.clampedPosition; width = clearance; found = true; break;
                    }
                    if (found) break;
                }
                if (found) break;
            }
            if (!found)
            {
                Sprint25Check("confined-fixture-" + key, false, new JObject { ["setup"] = "no-measured-confined-native-segment" },
                    "The native first-area graph supplies a measured confined segment; absence is unmet setup, not product proof.");
                yield break;
            }
            PlaceExpandedSummoningUnit(unit, from);
            var command = new UnitMoveTo(to, .5f); command.Init(unit);
            bool canStart = command.CanStart;
            if (canStart) unit.Commands.Run(command);
            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            while (canStart && !command.IsFinished && !unit.Destroyed && unit.View != null && DateTime.UtcNow < deadline) yield return 0;
            bool finite = !float.IsNaN(unit.Position.x + unit.Position.y + unit.Position.z) &&
                !float.IsInfinity(unit.Position.x + unit.Position.y + unit.Position.z);
            Sprint25Check("confined-movement-" + key, finite && !unit.Destroyed && unit.View != null && (!canStart || command.IsFinished),
                new JObject { ["unit"] = unit.UniqueId, ["nativeClearance"] = width, ["corpulence"] = unit.Corpulence,
                    ["from"] = SurveyVector(from), ["to"] = SurveyVector(to), ["canStart"] = canStart,
                    ["commandFinished"] = command.IsFinished, ["commandResult"] = command.Result.ToString(),
                    ["finalPosition"] = SurveyVector(unit.Position) },
                "In a measured confined native lane, the already-proven mobile unit finishes its command or refuses safely; no invalid placement or command stall is accepted.");
            unit.Commands.InterruptAll(true);
        }

        private IEnumerable<int> Sprint25VisualSetup(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach (int frame in Sprint25Movement(fixture)) yield return frame;
            foreach (string key in new[] { "brachiosaurus", "elephant" })
            {
                Mesh beforeMesh, restoredMesh; string[] beforeBones, restoredBones;
                Texture2D beforePaint, restoredPaint; string beforeStatus, restoredStatus;
                bool loaded = PteranodonAssetRuntime.TryGetGiantBodyVisual(key, out beforeMesh,
                    out beforeBones, out beforePaint, out beforeStatus);
                UnitEntityData failed = null;
                try
                {
                    using (PteranodonAssetRuntime.WithdrawGiantBodyForTest(key))
                        failed = CastExpandedSummoningQuietUnit(fixture, key);
                    foreach (int frame in Sprint25NativeAppearance(failed, key)) yield return frame;
                    var skins = failed.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    string outcome = ExpandedSummoningPteranodonViewPatch.DescribeView(failed.View);
                    Sprint25Check("payload-fallback-" + key,
                        loaded && outcome.StartsWith("donor-visual:withdrawn-for-test:", StringComparison.Ordinal) &&
                        skins.Any(value => value.enabled && value.sharedMesh != null && value.sharedMesh.name != "KMG_" + key + "_Original") &&
                        ExpandedSummoningPteranodonViewPatch.CaptureGiantBodyOwnedResources(failed.View).Length == 0,
                        new JObject { ["unit"] = failed.UniqueId, ["outcome"] = outcome },
                        "An unavailable project payload leaves a usable native donor with no private attachment allocation.");
                }
                finally
                {
                    if (failed != null) DisposeExpandedSummoningUnits(fixture.Created, new[] { failed });
                }
                bool restored = PteranodonAssetRuntime.TryGetGiantBodyVisual(key, out restoredMesh,
                    out restoredBones, out restoredPaint, out restoredStatus);
                Sprint25Check("payload-cache-restored-" + key,
                    loaded && restored && ReferenceEquals(beforeMesh, restoredMesh) && ReferenceEquals(beforePaint, restoredPaint) &&
                    beforeBones != null && restoredBones != null && beforeBones.SequenceEqual(restoredBones) && beforeStatus == restoredStatus,
                    new JObject { ["creature"] = key, ["status"] = restoredStatus },
                    "The fault scope restores the exact intended process-cache references and status.");
                var route = ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.OneD4PlusOne);
                UnitEntityData[] group = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster, route, null, fixture.Evidence);
                fixture.Created.AddRange(group);
                var resources = new List<UnityEngine.Object>();
                try
                {
                    foreach (UnitEntityData unit in group) SetExpandedSummoningBrainActive(unit, false);
                    foreach (UnitEntityData unit in group)
                        foreach (int frame in Sprint25NativeAppearance(unit, key)) yield return frame;
                    bool quantity = group.Length >= 2 && group.Length <= 5 && group.All(value =>
                        EntityFadedIn(value) && DissolveAmount(value) >= 0f && DissolveAmount(value) <= .02f);
                    for (int index = 0; index < group.Length; index++)
                    {
                        Sprint25Body(group[index], key, "quantity-" + index);
                        resources.AddRange(ExpandedSummoningPteranodonViewPatch.CaptureGiantBodyOwnedResources(group[index].View));
                    }
                    Sprint25Check("quantity-visible-" + key, quantity,
                        new JObject { ["route"] = route.StableKey, ["units"] = new JArray(group.Select(value => value.UniqueId)),
                            ["quantity"] = group.Length, ["allFadedIn"] = group.All(EntityFadedIn) },
                        "A representative real quantity cast produces two to five visible, attached, selectable original bodies.");
                }
                finally { DisposeExpandedSummoningUnits(fixture.Created, group); }
                for (int frame = 0; frame < 60 && resources.Any(value => value != null); frame++)
                { Game.Instance.EntityDestroyer.Tick(); yield return 0; }
                Sprint25Check("quantity-resource-cleanup-" + key, resources.All(value => value == null),
                    new JObject { ["privateCaptured"] = resources.Count, ["privateRemaining"] = resources.Count(value => value != null) },
                    "Native quantity despawn releases every captured private object while the intended shared cache remains owned by the process.");
            }
        }

        private IEnumerable<int> Sprint25PowerAttack(UnitEntityData owner, string key, bool turnBased)
        {
            ItemEntityWeapon[] weapons = Sprint19Limbs(owner);
            foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
            int[] attackOff = weapons.Select(value => Rulebook.Trigger(
                new RuleCalculateAttackBonusWithoutTarget(owner, value, 0)).Result).ToArray();
            int[] damageOff = weapons.Select(value => Rulebook.Trigger(
                new RuleCalculateWeaponStats(owner, value, null)).DamageDescription[0].CreateDamage().Bonus).ToArray();
            foreach (int frame in Sprint22SetPowerAttack(owner, true)) yield return frame;
            int[] attackOn = weapons.Select(value => Rulebook.Trigger(
                new RuleCalculateAttackBonusWithoutTarget(owner, value, 0)).Result).ToArray();
            int[] damageOn = weapons.Select(value => Rulebook.Trigger(
                new RuleCalculateWeaponStats(owner, value, null)).DamageDescription[0].CreateDamage().Bonus).ToArray();
            int penalty = key == "brachiosaurus" ? 4 : 3;
            int bonus = key == "brachiosaurus" ? 12 : 6;
            Sprint25Check("power-attack-" + key + "-" + turnBased,
                Enumerable.Range(0, weapons.Length).All(index => attackOn[index] == attackOff[index] - penalty &&
                    damageOn[index] == damageOff[index] + bonus),
                new JObject { ["unit"] = owner.UniqueId, ["attackOff"] = new JArray(attackOff),
                    ["attackOn"] = new JArray(attackOn), ["damageOff"] = new JArray(damageOff),
                    ["damageOn"] = new JArray(damageOn) },
                "The granted native Power Attack toggle changes every primary attack by the frozen adult penalty and appropriate lone/multiple-primary damage benefit.");
            foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
        }

        private void Sprint25TriceratopsControl(ExpandedSummoningCorrectionFixture fixture)
        {
            UnitEntityData unit = CastExpandedSummoningQuietUnit(fixture, "triceratops");
            ExpandedSummoningCorrectionFixture previous = _rulesFixture;
            int reflex = fixture.Hostile.Stats.SaveReflex.BaseValue;
            try
            {
                _rulesFixture = fixture;
                var rows = new JArray();
                bool exact = true;
                foreach (int[] entry in new[] { new[] { 32, 16, 28 }, new[] { 4, -3, 14 } })
                {
                    unit.Stats.Strength.BaseValue = entry[0];
                    PrepareExpandedSummoningTrampleResponsePair(fixture, unit, fixture.Hostile, 0, 1f);
                    fixture.Hostile.Stats.SaveReflex.BaseValue = -100;
                    Game.Instance.Player.GameTime += TimeSpan.FromSeconds(6);
                    var result = RunExpandedSummoningTrampleContact(unit, fixture.Hostile, "sprint25-triceratops-control", 10);
                    BaseDamage damage = result.Damage.Count == 1 ? result.Damage[0].DamageBundle.First : null;
                    exact &= damage != null && damage.Dice.Rolls == 1 && damage.Dice.Dice == DiceType.D8 &&
                        damage.Bonus == entry[1] && result.Saves.Count == 1 && result.Saves[0].DifficultyClass == entry[2];
                    rows.Add(new JObject { ["strength"] = unit.Stats.Strength.ModifiedValue,
                        ["modifier"] = unit.Stats.Strength.Bonus, ["result"] = Sprint23TrampleResult(result) });
                }
                string body = ExpandedSummoningPteranodonViewPatch.DescribeView(unit.View);
                exact &= body.StartsWith("visual:attached;", StringComparison.Ordinal) &&
                    ExpandedSummoningPteranodonViewPatch.CaptureArmoredDinosaurOwnedResources(unit.View).Length >= 2;
                Sprint25Check("triceratops-shared-trample-and-body-control", exact,
                    new JObject { ["unit"] = unit.UniqueId, ["cases"] = rows, ["view"] = body },
                    "The affected older d8 trample retains live odd-positive and penalty Strength/DC behavior and its original body still attaches.");
            }
            finally
            {
                _rulesFixture = previous;
                fixture.Hostile.Stats.SaveReflex.BaseValue = reflex;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { unit });
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private void Sprint25IconConsumers(ExpandedSummoningCorrectionFixture fixture, SummonVariantSpec[] routes)
        {
            var rows = new JArray();
            bool exact = ExpandedSummoningProjectIcons.LoadedCount == SummonIconCatalog.All.Count &&
                ExpandedSummoningProjectIcons.FallbackCount == 0;
            foreach (SummonVariantSpec route in routes)
            {
                Sprite icon = ExpandedSummoningProjectIcons.Require(route.Creature.Key);
                BlueprintAbility root = ExpandedSummoningRoot(fixture.Blueprints, route);
                exact &= ReferenceEquals(root.Icon, icon) && icon.rect.width == 128 && icon.rect.height == 128;
                rows.Add(new JObject { ["route"] = route.StableKey, ["ability"] = root.AssetGuid,
                    ["spriteId"] = root.Icon == null ? 0 : root.Icon.GetInstanceID(),
                    ["expectedSpriteId"] = icon.GetInstanceID() });
                if (route.Family == SummonFamily.Monster && route.Creature.MonsterTemplated)
                    foreach (string suffix in new[] { ".Celestial", ".Fiendish" })
                    {
                        BlueprintAbility child = ResolveExpandedSummoningExecution(fixture.Blueprints, route, suffix);
                        exact &= ReferenceEquals(child.Icon, icon);
                        rows.Add(new JObject { ["route"] = route.StableKey + suffix, ["ability"] = child.AssetGuid,
                            ["spriteId"] = child.Icon == null ? 0 : child.Icon.GetInstanceID() });
                    }
            }
            string preview = "sprint25-native-sprites-normal-ui-32px.png";
            RenderTexture prior = RenderTexture.active;
            var target = new RenderTexture(64, 32, 0);
            Texture2D pixels = null;
            bool matrix = false;
            try
            {
                RenderTexture.active = target;
                GL.Clear(true, true, new Color(.10f, .08f, .05f, 1f));
                GL.PushMatrix(); matrix = true;
                GL.LoadPixelMatrix(0, 64, 32, 0);
                Graphics.DrawTexture(new Rect(0, 0, 32, 32), ExpandedSummoningProjectIcons.Require("brachiosaurus").texture);
                Graphics.DrawTexture(new Rect(32, 0, 32, 32), ExpandedSummoningProjectIcons.Require("elephant").texture);
                pixels = new Texture2D(64, 32, TextureFormat.RGBA32, false);
                pixels.ReadPixels(new Rect(0, 0, 64, 32), 0, 0); pixels.Apply(false, false);
                File.WriteAllBytes(Path.Combine(_request.EvidenceDirectory, preview), EncodeExpandedSummoningPng(pixels));
            }
            finally
            {
                if (matrix) GL.PopMatrix();
                RenderTexture.active = prior;
                if (pixels != null) UnityEngine.Object.DestroyImmediate(pixels);
                target.Release(); UnityEngine.Object.DestroyImmediate(target);
            }
            Sprint25Check("icon-consumers", exact, new JObject { ["consumers"] = rows,
                ["loadedIconCount"] = ExpandedSummoningProjectIcons.LoadedCount,
                ["normalSizeNativeSpritePreview"] = preview,
                ["previewMeaning"] = "Actual loaded sprites rendered at 32 UI pixels; supporting readability evidence, not a claim of native menu navigation." },
                "All eleven real roots and existing template children use their exact required loaded 128-pixel sprite, with no fallback or changed source/export pixels.");
        }

        private void Sprint25Body(UnitEntityData owner, string key, string state)
        {
            Mesh cache; string[] bones; Texture2D paint; string status;
            bool loaded = PteranodonAssetRuntime.TryGetGiantBodyVisual(key,
                out cache, out bones, out paint, out status);
            var skins = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var original = skins.Where(value => value.sharedMesh != null &&
                value.sharedMesh.name == "KMG_" + key + "_Original").ToArray();
            var owned = ExpandedSummoningPteranodonViewPatch.CaptureGiantBodyOwnedResources(owner.View);
            JObject selection = Sprint22SelectionSurface(owner);
            JObject pose = original.Length == 1 ? Sprint24OriginalBodySample(owner, original[0], key) : null;
            float dissolve = DissolveAmount(owner);
            bool exact = loaded && original.Length == 1 && original[0].enabled &&
                EntityFadedIn(owner) && dissolve >= 0f && dissolve <= .02f &&
                original[0].name == GiantBodyVisualPolicy.BodyRendererName &&
                original[0].sharedMesh != cache && original[0].bones.Length == bones.Length &&
                original[0].sharedMaterial != null &&
                ReferenceEquals(original[0].sharedMaterial.mainTexture, paint) &&
                owned.Any(value => ReferenceEquals(value, original[0].sharedMesh)) &&
                (bool)selection["selectionSurface"] && pose != null &&
                (bool)pose["finite"] && (bool)pose["poseFinite"];
            Sprint25Check("body-" + key + "-" + state, exact, new JObject {
                ["unit"] = owner.UniqueId, ["blueprint"] = owner.Blueprint.AssetGuid,
                ["state"] = state, ["assetStatus"] = status, ["selection"] = selection,
                ["dissolve"] = dissolve, ["fadedIn"] = EntityFadedIn(owner),
                ["pose"] = pose,
                ["partyCamera"] = WriteExpandedSummoningPartyCameraCapture(owner,
                    _request.EvidenceDirectory, "sprint25-" + key + "-" + state + "-party.png"),
                ["supportingFrame"] = original.Length == 1 ?
                    CaptureSprint17OriginalBodyFrame(owner, original[0], "sprint25-" + key + "-" + state) : null,
                ["privateResources"] = new JArray(owned.Select(value => value.GetInstanceID())),
                ["bodyCount"] = original.Length },
                "The original body actually attaches to the exact donor renderer with private mesh ownership, borrowed paint and native selection.");
        }

        private IEnumerable<int> Sprint25NativeAppearance(UnitEntityData unit, string key,
            bool requireVisible = true)
        {
            Vector3 spawnPosition = unit.Position;
            if (AstarPath.active == null) throw new InvalidOperationException("No native appearance placement graph.");
            var ground = AstarPath.active.GetNearest(spawnPosition);
            if (ground.node == null || !ground.node.Walkable)
                throw new InvalidOperationException("No walkable native appearance placement: " + key);
            // The save-free new-game anchor is above the settled floor until
            // ordinary terrain/movement controllers run. Use the real graph
            // for this request-owned idle fixture, not a model transform fix.
            PlaceExpandedSummoningUnit(unit, ground.clampedPosition);
            int frames = 0;
            bool ready = false;
            double before = Game.Instance.Player.GameTime.TotalSeconds;
            DateTime deadline = DateTime.UtcNow.AddSeconds(10);
            bool waitingBefore = _sprint25WaitingForUnityAppearance;
            try
            {
                for (; frames < 1800 && DateTime.UtcNow < deadline; frames++)
                {
                    var body = unit.View == null ? null : unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                        .SingleOrDefault(value => value.name == GiantBodyVisualPolicy.BodyRendererName);
                    float dissolve = DissolveAmount(unit);
                    bool appearanceBuffAbsent = unit.Descriptor.Buffs.GetBuff(
                        Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null;
                    ready = body != null && body.enabled && appearanceBuffAbsent &&
                        unit.Descriptor.State.CanAct && unit.Descriptor.State.CanMove &&
                        (!requireVisible || EntityFadedIn(unit) && dissolve >= 0f && dissolve <= .02f);
                    if (ready) break;
                    // Native dissolve reads Unity Time.time, not GameTime.
                    // Once gameplay prerequisites settle, wait on Unity frames
                    // without consuming the summon's duration via .25 s pulses.
                    _sprint25WaitingForUnityAppearance = appearanceBuffAbsent && body != null && EntityFadedIn(unit);
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
            }
            finally { _sprint25WaitingForUnityAppearance = waitingBefore; }
            Sprint25Check((requireVisible ? "native-appearance-ready-" : "native-action-ready-") + key, ready,
                new JObject { ["unit"] = unit.UniqueId, ["frames"] = frames,
                    ["spawnPosition"] = SurveyVector(spawnPosition),
                    ["nativeGroundPosition"] = SurveyVector(ground.clampedPosition),
                    ["gameSecondsDuringWait"] = Game.Instance.Player.GameTime.TotalSeconds - before,
                    ["requireVisible"] = requireVisible,
                    ["fadedIn"] = EntityFadedIn(unit), ["dissolve"] = DissolveAmount(unit) },
                requireVisible
                    ? "Native appearance prerequisites and finite intact material dissolve settle before visual observations; no renderer, fader or material state is forced."
                    : "The native appearance action lock clears and the exact owned actor can act/move; an isolated mechanical lane need not be in party sight. No visual result is claimed here.");
            if (!ready) throw new InvalidOperationException("Native giant appearance did not settle: " + key);
        }
    }
}
