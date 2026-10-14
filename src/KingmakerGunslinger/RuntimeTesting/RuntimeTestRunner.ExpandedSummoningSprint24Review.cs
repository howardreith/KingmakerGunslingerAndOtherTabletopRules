using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 24's bounded save-free native review. Each request selects one
    /// mechanics or presentation phase and receives a fresh Kingmaker process.
    /// The fixture, native clock, command drivers and exact teardown are the
    /// already-qualified Sprint 22/23 seams; this file owns only theropod
    /// observations and their evidence document.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private ExpandedSummoningCorrectionFixture _sprint24Fixture;
        private IEnumerator<int> _sprint24Steps;
        private Sprint22NativeClock _sprint24Clock;
        private readonly List<RuntimeTestAssertion> _sprint24Assertions =
            new List<RuntimeTestAssertion>();
        private readonly JArray _sprint24Rows = new JArray();

        private void Sprint24Check(string predicate, bool passed,
            JObject row, string expected)
        {
            row["predicate"] = predicate;
            row["passed"] = passed;
            row["runId"] = _request.RunId;
            _sprint24Rows.Add(row);
            _sprint24Assertions.Add(Assertion("sprint24-" + predicate,
                expected, row.ToString(Formatting.None), passed,
                "Request-owned theropods and controls in the save-free native first-area fixture."));
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                _request.EvidenceDirectory, "sprint24-review-progress.json"),
                BoundaryObservationPolicy.Serialize(_sprint24Rows));
        }

        private bool PollSprint24NativeReview()
        {
            try
            {
                if (_sprint24Steps == null)
                {
                    _sprint24Fixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_Sprint24_Caster");
                    _sprint24Clock = new Sprint22NativeClock(
                        _sprint24Fixture);
                    _sprint24Steps = ReviewSprint24(_sprint24Fixture)
                        .GetEnumerator();
                }
                _sprint24Clock.Pulse();
                if (_sprint24Steps.MoveNext()) return false;
            }
            catch (Exception error)
            {
                Sprint24Check("execution", false, new JObject {
                    ["unit"] = "theropods",
                    ["event"] = "native-review",
                    ["owner"] = _sprint24Fixture?.Caster?.UniqueId,
                    ["exception"] = error.ToString()
                }, "The selected native review phase completes without exception.");
            }
            CleanupSprint24NativeReview();
            string report = Path.Combine(_request.EvidenceDirectory,
                "sprint24-review.json");
            RuntimeTestResultWriter.WriteAtomic(report,
                BoundaryObservationPolicy.Serialize(_sprint24Rows));
            _summoningProfileResult = CreateResult(_sprint24Assertions.All(
                    value => value.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                _sprint24Assertions, null);
            _summoningProfileResult.EvidenceFiles.Add(report);
            return true;
        }

        private void CleanupSprint24NativeReview()
        {
            if (_sprint24Steps != null)
            {
                _sprint24Steps.Dispose();
                _sprint24Steps = null;
            }
            if (_sprint24Fixture == null)
            {
                if (_sprint24Clock != null)
                {
                    _sprint24Clock.Dispose();
                    _sprint24Clock = null;
                }
                return;
            }
            bool cleaned = false, clockRestored = false;
            int pulses = _sprint24Clock == null ? 0 :
                _sprint24Clock.Pulses;
            int movementTicks = _sprint24Clock == null ? 0 :
                _sprint24Clock.MovementTicks;
            double advanced = _sprint24Clock == null ? 0d :
                _sprint24Clock.AdvancedSeconds;
            try
            {
                EndExpandedSummoningCorrectionFixture(_sprint24Fixture,
                    out cleaned);
            }
            finally
            {
                if (_sprint24Clock != null)
                {
                    _sprint24Clock.Dispose();
                    clockRestored = _sprint24Clock.Restored;
                    _sprint24Clock = null;
                }
            }
            Sprint24Check("fixture-restored", cleaned && clockRestored,
                new JObject {
                    ["unit"] = "theropods",
                    ["event"] = "exact-owned-teardown-and-clock-restore",
                    ["referencesRestored"] = cleaned,
                    ["clockRestored"] = clockRestored,
                    ["nativePulses"] = pulses,
                    ["nativeMovementTicks"] = movementTicks,
                    ["nativeAdvancedSeconds"] = advanced
                }, "Exact original unit/party/area references and request-local GameTime are restored.");
            _sprint24Fixture = null;
        }

        private IEnumerable<int> ReviewSprint24(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool originalMode = SettingsRoot.Instance.EnableTurnBasedMode
                .CurrentValue;
            bool originalPause = Game.Instance.IsPaused;
            string phase = RuntimeTestRequestParser.Sprint24ReviewPhase(
                _request);
            if (phase == null)
                throw new InvalidOperationException(
                    "Sprint 24 native review phase was not validated.");
            bool turnBased = phase.StartsWith("turn-based-",
                StringComparison.Ordinal);
            try
            {
                Sprint24Check("review-phase-" + phase, true,
                    new JObject {
                        ["unit"] = "theropods",
                        ["event"] = "bounded-native-review-selection",
                        ["reviewPhase"] = phase,
                        ["combatMode"] = turnBased ? "turn-based" : "rtwp"
                    }, "The exact allowlisted bounded review phase is recorded.");
                Game.Instance.IsPaused = true;
                CreateExpandedSummoningCorrectionHostile(fixture);
                // The qualified Sprint 22/23 controller first admits exact
                // request-owned actors in RTWP, then lets the supported mode
                // toggle rebuild native turn order.  Enabling TB here before
                // admission leaves later commands outside SortedUnits and can
                // also make AbilityCustomCharge route probes dereference a
                // missing current-turn path.
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;

                if (phase == "rtwp-foundation")
                {
                    Sprint24RegisteredRoutes(fixture);
                    foreach (string key in TheropodVisualPolicy.Keys)
                    {
                        UnitEntityData owner = CastExpandedSummoningOwnTier(
                            fixture, key);
                        SetExpandedSummoningBrainActive(owner, false);
                        for (int frame = 0; frame < 12; frame++) yield return 0;
                        Sprint24ReadProfile(fixture, owner, key, false);
                        Sprint24ReadBody(owner, key, false);
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                    }
                }
                else if (phase == "rtwp-deinonychus" ||
                    phase == "turn-based-deinonychus")
                {
                    foreach (int frame in Sprint24ReviewDeinonychus(
                        fixture, turnBased)) yield return frame;
                }
                else if (phase == "rtwp-tyrannosaurus" ||
                    phase == "turn-based-tyrannosaurus")
                {
                    foreach (int frame in Sprint24ReviewTyrannosaurus(
                        fixture, turnBased)) yield return frame;
                }
                else if (phase == "visual-lifecycle")
                {
                    foreach (int frame in Sprint24VisualLifecycle(fixture))
                        yield return frame;
                    foreach (int frame in Sprint24MovementAndCrowding(fixture))
                        yield return frame;
                }
                else if (phase == "shared-controls")
                {
                    foreach (int frame in Sprint24SharedControls(fixture))
                        yield return frame;
                }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    originalMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = originalPause;
            }
        }

        private void Sprint24RegisteredRoutes(
            ExpandedSummoningCorrectionFixture fixture)
        {
            SummonVariantSpec[] routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly))
                .Where(value => TheropodRulesPolicy.UsesExactRanks(
                    value.Creature.Key)).ToArray();
            int published = routes.Count(SummonVisibilityCatalog.IsPublished);
            bool coherentStage = published == 0 || published == 18;
            Sprint24Check("route-census", routes.Length == 18 &&
                routes.Count(value => value.Creature.Key ==
                    TheropodRulesPolicy.DeinonychusKey) == 12 &&
                routes.Count(value => value.Creature.Key ==
                    TheropodRulesPolicy.TyrannosaurusKey) == 6 &&
                coherentStage,
                new JObject {
                    ["unit"] = "theropods",
                    ["event"] = published == 18 ?
                        "public-route-census" : "hidden-route-census",
                    ["publishedRoutes"] = published,
                    ["registeredRoutes"] = routes.Length,
                    ["routes"] = new JArray(routes.Select(value =>
                        value.StableKey))
                }, "Exactly eighteen theropod roots are registered and the candidate is wholly hidden or wholly public.");

            if (published == 18)
            {
                Sprint24PlayerRoutes(fixture, routes);
                return;
            }

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
                            TheropodRulesPolicy.DeinonychusKey
                            ? "Deinonychus" : "Tyrannosaurus"));
                bool monster = route.Family == SummonFamily.Monster;
                bool exactTemplate = units.All(value =>
                    Sprint24TemplateCount(value) == (monster ? 1 : 0));
                bool exact = units.Length >= low && units.Length <= high &&
                    units.All(value => ReferenceEquals(value.Blueprint,
                        expected)) && exactTemplate &&
                    !SummonVisibilityCatalog.IsPublished(route);
                Sprint24Check("hidden-route-" + route.StableKey, exact,
                    new JObject {
                        ["unit"] = route.Creature.Key,
                        ["event"] = "registered-hidden-ability-execution",
                        ["route"] = route.StableKey,
                        ["execution"] =
                            _expandedSummoningLastAbilityExecution,
                        ["quantity"] = units.Length,
                        ["templateCounts"] = new JArray(units.Select(
                            Sprint24TemplateCount)),
                        ["unitReadback"] = new JArray(units.Select(
                            Sprint22UnitReadback))
                    }, "The hidden registered root executes only its exact unit, legal quantity and existing family template policy.");
                DisposeExpandedSummoningUnits(fixture.Created, units);
            }
        }

        private static int Sprint24TemplateCount(UnitEntityData unit)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value =>
                value.Blueprint != null &&
                (value.Blueprint.name.StartsWith(
                    "KMG_Summoning_Template_Celestial_",
                    StringComparison.Ordinal) ||
                 value.Blueprint.name.StartsWith(
                    "KMG_Summoning_Template_Fiendish_",
                    StringComparison.Ordinal)));
        }

        private void Sprint24PlayerRoutes(
            ExpandedSummoningCorrectionFixture fixture,
            SummonVariantSpec[] routes)
        {
            object levelController = null;
            MethodInfo castRule = typeof(RuleCastSpell).GetMethod("OnTrigger");
            MethodInfo spawnAction = typeof(ContextActionSpawnMonster)
                .GetMethod("RunAction");
            const BindingFlags statics = BindingFlags.NonPublic |
                BindingFlags.Static;
            HarmonyInstance hooks = HarmonyInstance.Create(
                "kmg.sprint24.routes." + _request.RunId);
            try
            {
                fixture.Caster.Descriptor.Stats.Intelligence.BaseValue = 30;
                BlueprintCharacterClass wizard = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library,
                        "ba34257984f4c41408ce1dc2004e342e",
                        "native Wizard targeted spellbook path");
                AdvanceDisposableSpellcaster(fixture.Caster.Descriptor,
                    wizard, 20, ref levelController);
                Spellbook book = fixture.Caster.Descriptor.GetSpellbook(
                    wizard);
                while (book.CasterLevel < 20) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false);
                book.Rest();
                BlueprintAbility[] parents = ExpandedSummoningInventoryObserver
                    .CanonicalParentGuids.Select(guid =>
                        BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                            BlueprintBootstrap.Library, guid,
                            "canonical summon parent")).ToArray();
                hooks.Patch(castRule, null, new HarmonyMethod(
                    typeof(RuntimeTestRunner).GetMethod(
                        "ExpandedSummoningPlayerPathRuleCastPostfix",
                        statics)), null);
                hooks.Patch(spawnAction, new HarmonyMethod(
                    typeof(RuntimeTestRunner).GetMethod(
                        "ExpandedSummoningPlayerPathSpawnPrefix", statics)),
                    null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                foreach (SummonVariantSpec route in routes)
                {
                    object[] before = SnapshotReferences(fixture.AllUnits);
                    var cases = new List<ExpandedSummoningPlayerPathCase>();
                    AddExpandedSummoningPlayerPathRootCase(cases,
                        fixture.Blueprints, fixture.Caster, book, parents,
                        route, ExpandedSummoningPlayerPathAlignmentFor(route),
                        route.Family == SummonFamily.NaturesAlly
                            ? (SummonAlignmentMode?)SummonAlignmentMode.Caster
                            : SummonAlignmentMode.Celestial,
                        fixture.SceneEntities, fixture.AllUnits,
                        route.StableKey);
                    ExpandedSummoningPlayerPathCase item = cases.Single();
                    var durations = ExpandedSummoningRuleDurationCapture
                        .ToArray();
                    bool duration = durations.Length == item.LiveCount &&
                        durations.All(value => Math.Abs(
                            value.BaseDuration.TotalSeconds - 120d) <= .001 &&
                            value.BonusDuration.TotalSeconds >= 0 &&
                            value.Unit != null && value.Unit.Destroyed);
                    bool exact = item.LiveContract && item.SlotContract &&
                        item.CommandStarted && item.CommandResult == "Success" &&
                        item.RuleCastCount == 1 && item.SpawnActionCount == 1 &&
                        item.QuantityContract && duration &&
                        before.SequenceEqual(SnapshotReferences(
                            fixture.AllUnits));
                    Sprint24Check("player-route-" + route.StableKey, exact,
                        new JObject {
                            ["unit"] = route.Creature.Key,
                            ["event"] =
                                "native-parent-selected-variant-spellbook",
                            ["owner"] = route.StableKey,
                            ["detail"] = item.Describe(),
                            ["baseDurations"] = new JArray(durations.Select(
                                value => value.BaseDuration.TotalSeconds)),
                            ["slotContract"] = item.SlotContract,
                            ["quantityContract"] = item.QuantityContract,
                            ["durationContract"] = duration
                        }, "The selected real parent consumes one native spellbook slot and one cast/spawn action, preserves template and quantity, carries CL20 duration, and cleans exactly.");
                }
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive = false;
                hooks.UnpatchAll(hooks.Id);
                ExpandedSummoningPlayerPathEvents.Clear();
                if (levelController != null)
                    levelController.GetType().GetMethod("Cancel")
                        .Invoke(levelController, null);
            }
        }

        private void Sprint24ReadProfile(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key, bool turnBased)
        {
            bool dein = key == TheropodRulesPolicy.DeinonychusKey;
            ModifiableValue[] abilities = {
                owner.Stats.Strength, owner.Stats.Dexterity,
                owner.Stats.Constitution, owner.Stats.Intelligence,
                owner.Stats.Wisdom, owner.Stats.Charisma };
            int[] scores = abilities.Select(value => value.ModifiedValue)
                .ToArray();
            int[] expectedScores = dein
                ? new[] { 15, 15, 19, 2, 12, 14 }
                : new[] { 32, 13, 19, 2, 15, 10 };
            int difficulty = owner.Stats.AC.Modifiers.Where(value =>
                value.ModDescriptor == ModifierDescriptor.Difficulty)
                .Sum(value => value.ModValue);
            Sprint14CmdBreakdown cmd = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.Grapple);
            Sprint14CmdBreakdown trip = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.Trip);
            int deniedDex = cmd.DexterityDenied || cmd.FlatFooted
                ? Math.Max(0, owner.Stats.Dexterity.Bonus) : 0;
            int cmb = Rulebook.Trigger(new RuleCalculateCMB(owner,
                fixture.Hostile, CombatManeuver.Trip)).Result;
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            var attacks = new JArray();
            int[] expectedAttack = dein
                ? new[] { 5, 5, 5, 0 } : new[] { 20 };
            int[] expectedRolls = dein
                ? new[] { 1, 1, 1, 1 } : new[] { 4 };
            DiceType[] expectedDice = dein
                ? new[] { DiceType.D8, DiceType.D8, DiceType.D6,
                    DiceType.D4 }
                : new[] { DiceType.D6 };
            int[] expectedDamage = dein
                ? new[] { 2, 2, 2, 1 } : new[] { 22 };
            bool attackShape = limbs.Length == expectedAttack.Length;
            for (int index = 0; index < limbs.Length; index++)
            {
                RuleCalculateWeaponStats stats = Rulebook.Trigger(
                    new RuleCalculateWeaponStats(owner, limbs[index], null));
                BaseDamage damage = stats.DamageDescription[0].CreateDamage();
                int attack = Rulebook.Trigger(
                    new RuleCalculateAttackBonusWithoutTarget(owner,
                        limbs[index], 0)).Result;
                bool secondary = limbs[index].IsSecondary;
                bool exact = index < expectedAttack.Length &&
                    attack == expectedAttack[index] &&
                    damage.Dice.Rolls == expectedRolls[index] &&
                    damage.Dice.Dice == expectedDice[index] &&
                    damage.Bonus == expectedDamage[index] &&
                    secondary == (dein && index == 3);
                attackShape &= exact;
                attacks.Add(new JObject {
                    ["index"] = index,
                    ["weapon"] = limbs[index].Blueprint.AssetGuid,
                    ["attack"] = attack,
                    ["damage"] = Sprint16DamageLine(damage),
                    ["secondary"] = secondary,
                    ["calculationSecondary"] = stats.SecondaryWeapon,
                    ["exact"] = exact
                });
            }
            string[] facts = owner.Descriptor.Progression.Features.Enumerable
                .Select(value => value.Blueprint.name).ToArray();
            bool forbidden = facts.Any(value => value != null &&
                (value.IndexOf("PowerfulCharge",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                 (dein && (value.IndexOf("Rake",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("TripDefense",
                    StringComparison.OrdinalIgnoreCase) >= 0)) ||
                 (!dein && (value.IndexOf("Pounce",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("PowerAttack",
                    StringComparison.OrdinalIgnoreCase) >= 0))));
            int targetCmd = dein ? 17 : 39;
            bool exactProfile = scores.SequenceEqual(expectedScores) &&
                owner.Descriptor.State.Size == (dein
                    ? Size.Medium : Size.Gargantuan) &&
                owner.Descriptor.Progression.CharacterLevel ==
                    (dein ? 4 : 18) &&
                owner.Stats.HitPoints.ModifiedValue == (dein ? 34 : 153) &&
                owner.Stats.AC.ModifiedValue - difficulty ==
                    (dein ? 15 : 21) &&
                owner.Stats.AC.Touch - difficulty == (dein ? 12 : 7) &&
                owner.Stats.AC.FlatFooted - difficulty ==
                    (dein ? 13 : 20) &&
                owner.Stats.SaveFortitude.ModifiedValue == (dein ? 8 : 15) &&
                owner.Stats.SaveReflex.ModifiedValue == (dein ? 6 : 12) &&
                owner.Stats.SaveWill.ModifiedValue == (dein ? 2 : 10) &&
                owner.Stats.SkillMobility.ModifiedValue ==
                    (dein ? 10 : owner.Stats.Dexterity.Bonus) &&
                owner.Stats.SkillPerception.ModifiedValue ==
                    (dein ? 14 : 37) &&
                owner.Stats.SkillStealth.ModifiedValue ==
                    (dein ? 15 : owner.Stats.Dexterity.Bonus - 12) &&
                owner.Stats.BaseAttackBonus.ModifiedValue ==
                    (dein ? 3 : 13) &&
                cmb == (dein ? 5 : 28) &&
                cmd.Result + deniedDex - difficulty == targetCmd &&
                (dein ? trip.Result == cmd.Result : true) &&
                owner.Stats.Reach.ModifiedValue == (dein ? 5 : 20) &&
                owner.Stats.Speed.ModifiedValue == (dein ? 60 : 40) &&
                attackShape && !forbidden;
            Sprint24Check("profile-" + key + "-" +
                (turnBased ? "turn-based" : "rtwp"), exactProfile,
                new JObject {
                    ["unit"] = owner.UniqueId,
                    ["event"] = "native-adult-profile-readback",
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
                    ["mobility"] = DescribeSprint16Skill(
                        owner.Stats.SkillMobility),
                    ["perception"] = DescribeSprint16Skill(
                        owner.Stats.SkillPerception),
                    ["stealth"] = DescribeSprint16Skill(
                        owner.Stats.SkillStealth),
                    ["bab"] = owner.Stats.BaseAttackBonus.ModifiedValue,
                    ["cmb"] = cmb,
                    ["cmd"] = cmd.Describe("CMD"),
                    ["tripCmd"] = trip.Describe("trip"),
                    ["reachFeet"] = owner.Stats.Reach.ModifiedValue,
                    ["speedFeet"] = owner.Stats.Speed.ModifiedValue,
                    ["attacks"] = attacks,
                    ["facts"] = new JArray(facts),
                    ["forbiddenCarrier"] = forbidden
                }, "The live adult profile derives every score, HD/hp, defense, save, mapped skill, limb-owned attack role, reach and bipedal CMD term without a forbidden carrier; the standalone calculation's diagnostic SecondaryWeapon flag is recorded but does not outrank the equipped secondary limb and its resulting attack/damage arithmetic.");
        }

        private void Sprint24ReadBody(UnitEntityData owner, string key,
            bool turnBased)
        {
            Mesh cacheMesh;
            string[] bones;
            Texture2D albedo;
            string status;
            bool loaded = PteranodonAssetRuntime.TryGetTheropodVisual(key,
                out cacheMesh, out bones, out albedo, out status);
            string expectedName = "KMG_" + key + "_Original";
            SkinnedMeshRenderer[] skins = owner.View == null
                ? new SkinnedMeshRenderer[0]
                : owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(
                    true);
            SkinnedMeshRenderer[] original = skins.Where(value =>
                value.sharedMesh != null &&
                value.sharedMesh.name == expectedName).ToArray();
            SkinnedMeshRenderer body = skins.SingleOrDefault(value =>
                value.name == TheropodVisualPolicy.BodyRendererName);
            SkinnedMeshRenderer accessory = skins.SingleOrDefault(value =>
                value.name == TheropodVisualPolicy.AccessoryRendererName);
            JObject selection = Sprint22SelectionSurface(owner);
            bool exact = loaded && original.Length == 1 &&
                ReferenceEquals(original[0], body) && body.enabled &&
                body.bones.Length == TheropodVisualPolicy.Bones.Length &&
                body.sharedMesh != cacheMesh && body.sharedMaterial != null &&
                ReferenceEquals(body.sharedMaterial.mainTexture, albedo) &&
                accessory != null && !accessory.enabled &&
                (bool)selection["selectionSurface"] &&
                owner.Descriptor.State.Size == (key ==
                    TheropodRulesPolicy.DeinonychusKey
                        ? Size.Medium : Size.Gargantuan);
            string frame = original.Length == 1
                ? CaptureSprint17OriginalBodyFrame(owner, original[0],
                    "sprint24-" + key + "-" +
                    (turnBased ? "turn-based" : "rtwp"))
                : "no-original-renderer";
            Sprint24Check("body-" + key + "-" +
                (turnBased ? "turn-based" : "rtwp"), exact,
                new JObject {
                    ["unit"] = owner.UniqueId,
                    ["event"] = "owned-original-body-bind",
                    ["owner"] = owner.Blueprint.AssetGuid,
                    ["status"] = status,
                    ["supportingFrame"] = frame,
                    ["selection"] = selection,
                    ["skins"] = new JArray(skins.Select(value =>
                        new JObject {
                            ["name"] = value.name,
                            ["enabled"] = value.enabled,
                            ["mesh"] = value.sharedMesh?.name,
                            ["meshId"] = value.sharedMesh == null ? 0 :
                                value.sharedMesh.GetInstanceID(),
                            ["materialId"] = value.sharedMaterial == null ? 0 :
                                value.sharedMaterial.GetInstanceID(),
                            ["bones"] = value.bones == null ? 0 :
                                value.bones.Length
                        }))
                }, "One private original body replaces only the exact body renderer, the accessory is disabled per instance, selection remains native, and the immutable paint cache is borrowed.");
        }

        private sealed class Sprint24AttackObserver :
            IGlobalRulebookHandler<RuleAttackWithWeapon>,
            IGlobalRulebookHandler<RuleAttackRoll>,
            IGlobalRulebookHandler<RuleDealDamage>
        {
            internal UnitEntityData Owner;
            internal UnitEntityData Target;
            internal Action<RuleAttackRoll> BeforeRoll;
            internal Action<RuleAttackWithWeapon> AfterWeapon;
            internal readonly List<RuleAttackWithWeapon> WeaponAttacks =
                new List<RuleAttackWithWeapon>();
            internal readonly List<RuleAttackRoll> Rolls =
                new List<RuleAttackRoll>();
            internal readonly List<RuleDealDamage> Damage =
                new List<RuleDealDamage>();

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, Target))
                {
                    WeaponAttacks.Add(evt);
                    if (AfterWeapon != null) AfterWeapon(evt);
                }
            }
            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, Target) &&
                    BeforeRoll != null) BeforeRoll(evt);
            }
            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, Target)) Rolls.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, Target)) Damage.Add(evt);
            }
            internal void Clear()
            {
                WeaponAttacks.Clear();
                Rolls.Clear();
                Damage.Clear();
            }
        }

        private IEnumerable<int> Sprint24EnrollTurnBasedOwner(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key)
        {
            var admission = new JObject();
            foreach (int frame in Sprint22EnrollTurnBasedOwners(
                new[] { owner }, fixture.Hostile, admission))
                yield return frame;
            bool admitted = owner.IsInCombat && fixture.Hostile.IsInCombat &&
                CombatController.IsInTurnBasedCombat() &&
                (bool)admission["admitted"];
            Sprint24Check("turn-order-" + key, admitted, admission,
                "The exact request-owned theropod and hostile enter native turn order through the qualified RTWP-enrollment and supported-mode-toggle seam.");
            if (!admitted)
                throw new InvalidOperationException(
                    key + " did not enter native turn order: " +
                    admission.ToString(Formatting.None));
            int waited = 0;
            while (!Sprint18OwnerIsActing(owner) && waited++ < 320)
                yield return 0;
            if (!Sprint18OwnerIsActing(owner))
                throw new InvalidOperationException(
                    key + " never received its native turn after admission.");
        }

        private IEnumerable<int> Sprint24ReviewDeinonychus(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            UnitEntityData owner = CastExpandedSummoningOwnTier(fixture,
                TheropodRulesPolicy.DeinonychusKey);
            SetExpandedSummoningBrainActive(owner, false);
            UnitEntityData target = fixture.Hostile;
            int hitPointsBefore = target.Stats.HitPoints.BaseValue;
            int damageBefore = target.Descriptor.Damage;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            try
            {
                for (int frame = 0; frame < 12; frame++) yield return 0;
                Sprint24ReadProfile(fixture, owner,
                    TheropodRulesPolicy.DeinonychusKey, turnBased);
                Sprint24ReadBody(owner,
                    TheropodRulesPolicy.DeinonychusKey, turnBased);
                if (turnBased)
                    foreach (int frame in Sprint24EnrollTurnBasedOwner(
                        fixture, owner,
                        TheropodRulesPolicy.DeinonychusKey))
                        yield return frame;
                owner.Stats.HitPoints.BaseValue = 100000;
                target.Stats.HitPoints.BaseValue = 100000;
                foreach (int frame in Sprint24DeinonychusOrdinaryCommands(
                    fixture, owner, turnBased)) yield return frame;
                foreach (int frame in Sprint24DeinonychusCharge(
                    fixture, owner, turnBased)) yield return frame;
                Sprint24DeinonychusOpportunityControl(fixture, owner,
                    turnBased);
            }
            finally
            {
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                target.Stats.HitPoints.BaseValue = hitPointsBefore;
                target.Descriptor.Damage = damageBefore;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner });
            }
        }

        private IEnumerable<int> Sprint24DeinonychusOrdinaryCommands(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            var observer = new Sprint24AttackObserver {
                Owner = owner,
                Target = target,
                BeforeRoll = value => {
                    owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                    UnityEngine.Random.InitState(FindNativeD20Seed(18));
                }
            };
            EventBus.Subscribe(observer);
            try
            {
                Vector3 floor = fixture.Caster.Position + Vector3.right * 4f;
                PlaceExpandedSummoningUnit(owner, floor);
                PlaceExpandedSummoningUnit(target,
                    floor + Vector3.forward * 2f);
                UnitAttack full = null;
                foreach (int frame in Sprint24RunAttackCommand(fixture,
                    owner, target, true, value => full = value))
                    yield return frame;
                var perLimb = new JArray(limbs.Select((weapon, index) =>
                    new JObject {
                        ["index"] = index,
                        ["weapon"] = weapon.Blueprint.AssetGuid,
                        ["secondary"] = weapon.IsSecondary,
                        ["rolls"] = observer.Rolls.Count(value =>
                            ReferenceEquals(value.Weapon, weapon))
                    }));
                bool fullExact = full != null && full.IsFinished &&
                    observer.Rolls.Count == 4 &&
                    observer.WeaponAttacks.Count == 4 &&
                    limbs.Length == 4 &&
                    limbs.All(weapon => observer.Rolls.Count(value =>
                        ReferenceEquals(value.Weapon, weapon)) == 1) &&
                    !owner.Descriptor.HasFact(
                        BlueprintRoot.Instance.SystemMechanics.ChargeBuff);
                Sprint24Check("deinonychus-ordinary-full-" +
                    (turnBased ? "turn-based" : "rtwp"), fullExact,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "real-native-full-attack-command",
                        ["commandFinished"] = full?.IsFinished,
                        ["commandResult"] = full?.Result.ToString(),
                        ["plannedAttacks"] = full == null ? -1 :
                            full.AllAttacks.Count,
                        ["rolls"] = observer.Rolls.Count,
                        ["rollDetails"] = new JArray(observer.Rolls.Select(
                            value => new JObject {
                                ["weapon"] = value.Weapon?.Blueprint?.AssetGuid,
                                ["natural"] = value.Roll.Value,
                                ["hit"] = value.IsHit
                            })),
                        ["weaponAttacks"] = observer.WeaponAttacks.Count,
                        ["perLimb"] = perLimb,
                        ["chargeMarker"] = owner.Descriptor.HasFact(
                            BlueprintRoot.Instance.SystemMechanics.ChargeBuff)
                    }, "A real ordinary full-attack command resolves the two talons, bite and one secondary foreclaws roll exactly once each without Pounce.");

                if (turnBased)
                    foreach (int frame in Sprint18AdvanceRound(owner))
                        yield return frame;
                observer.Clear();
                ResetExpandedSummoningHostile(fixture);
                UnitAttack single = null;
                foreach (int frame in Sprint24RunAttackCommand(fixture,
                    owner, target, false, value => single = value))
                    yield return frame;
                bool singleExact = single != null && single.IsFinished &&
                    observer.Rolls.Count == 1 &&
                    observer.WeaponAttacks.Count == 1 &&
                    ReferenceEquals(observer.Rolls[0].Weapon,
                        owner.Body.PrimaryHand.MaybeWeapon) &&
                    !observer.WeaponAttacks[0].IsCharge &&
                    !observer.WeaponAttacks[0].IsAttackOfOpportunity;
                Sprint24Check("deinonychus-ordinary-single-" +
                    (turnBased ? "turn-based" : "rtwp"), singleExact,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "real-native-single-attack-command",
                        ["commandFinished"] = single?.IsFinished,
                        ["commandResult"] = single?.Result.ToString(),
                        ["plannedAttacks"] = single == null ? -1 :
                            single.AllAttacks.Count,
                        ["rolls"] = observer.Rolls.Count,
                        ["weapons"] = new JArray(observer.Rolls.Select(
                            value => value.Weapon?.Blueprint?.AssetGuid))
                    }, "A real ordinary single-attack action resolves only the primary talon and never becomes a free full attack.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                owner.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private IEnumerable<int> Sprint24RunAttackCommand(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target, bool full,
            Action<UnitAttack> record)
        {
            owner.Memory.Add(target);
            target.Memory.Add(owner);
            foreach (UnitEntityData unit in new[] { owner, target })
                if (!Game.Instance.State.AwakeUnits.Contains(unit))
                    Game.Instance.State.AwakeUnits.Add(unit);
            int waited = 0;
            while (CombatController.IsInTurnBasedCombat() &&
                ++waited <= Sprint18AttackFrames)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if (Sprint18OwnerIsActing(owner)) break;
                yield return 0;
            }
            var attack = new UnitAttack(target) {
                ForceFullAttack = full,
                IsSingleAttack = !full
            };
            record(attack);
            owner.Commands.Run(attack);
            int frames = 0;
            while (++frames <= Sprint18AttackFrames)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if (CombatController.IsInTurnBasedCombat())
                    Sprint18OwnerIsActing(owner);
                if (attack.Animation != null && frames > 12)
                    attack.Animation.IsActed = true;
                yield return 0;
                if (attack.IsFinished) break;
            }
        }

        private IEnumerable<int> Sprint24DeinonychusCharge(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            int damageBefore = target.Descriptor.Damage;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            BlueprintAbility chargeBlueprint = fixture.Blueprints
                .OfType<BlueprintAbility>().Single(value =>
                    value.AssetGuid == "c78506dd0e14f7c45a599990e4e65038");
            Ability granted = owner.Descriptor.Abilities.GetAbility(
                chargeBlueprint);
            if (granted == null)
                throw new InvalidOperationException(
                    "Deinonychus lacks the exact native ChargeAbility.");
            var charge = new AbilityData(granted);
            UnitUseAbility command = null;
            UnitAttack attackCommand = null;
            UnitEntityData[] awakeBefore = null;
            var observer = new Sprint24AttackObserver {
                Owner = owner,
                Target = target,
                BeforeRoll = value => {
                    owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                    UnityEngine.Random.InitState(FindNativeD20Seed(18));
                }
            };
            EventBus.Subscribe(observer);
            try
            {
                if (turnBased)
                {
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                        false;
                    Game.Instance.TurnBasedCombatController.Activate();
                    Game.Instance.IsPaused = false;
                    yield return 0;
                    if (CombatController.IsInTurnBasedCombat())
                        throw new InvalidOperationException(
                            "Deinonychus charge route survey could not retire " +
                            "the preceding native turn.");
                }
                Vector3 start;
                string route;
                TargetWrapper selected = Sprint23FindChargeRoute(fixture,
                    owner, target, charge, out start, out route);
                if (selected == null)
                    throw new InvalidOperationException(
                        "No live Deinonychus charge route: " + route);
                if (turnBased)
                {
                    var admission = new JObject();
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(
                        new[] { owner }, target, admission)) yield return frame;
                    if (!(bool)admission["admitted"])
                        throw new InvalidOperationException(
                            "Deinonychus did not enter native turn order: " +
                            admission.ToString(Formatting.None));
                    int waited = 0;
                    while (!Sprint18OwnerIsActing(owner) && waited++ < 320)
                        yield return 0;
                }
                awakeBefore = Game.Instance.State.AwakeUnits.ToArray();
                foreach (UnitEntityData unit in new[] { owner, target })
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                float standardBefore = owner.CombatState.Cooldown
                    .StandardAction;
                float moveBefore = owner.CombatState.Cooldown.MoveAction;
                float maxStandard = standardBefore;
                float maxMove = moveBefore;
                command = new UnitUseAbility(charge, selected);
                bool nativeEconomy = !command.Cutscene &&
                    !command.IsIgnoreCooldown;
                if (!command.CanStart)
                    throw new InvalidOperationException(
                        "Native Deinonychus charge command cannot start.");
                owner.Commands.Run(command);
                bool queued = owner.Commands.Raw.Contains(command);
                bool executionSeen = command.ExecutionProcess != null;
                var controller = new UnitActionController();
                int movementTicks = 0;
                int frames = 0;
                while (frames++ < 700 && observer.Rolls.Count < 4)
                {
                    if (!command.IsFinished && (!command.IsStarted ||
                        command.ExecutionProcess == null))
                    {
                        if (command.Animation != null && frames > 4)
                            command.Animation.IsActed = true;
                        ElementalBreathScenario.TickCommand(controller,
                            command);
                    }
                    executionSeen |= command.ExecutionProcess != null;
                    float delta = Game.Instance.TimeController.DeltaTime;
                    if (command.ExecutionProcess != null && delta > 0f &&
                        owner.Descriptor.State.CanMove &&
                        !CombatController.IsInTurnBasedCombat())
                    {
                        owner.View.MovementAgent.TickMovement(delta);
                        owner.Position = owner.View.transform.position;
                        movementTicks++;
                    }
                    if (command.ExecutionProcess != null)
                        TickExpandedSummoningDetachedAbility(command, frames);
                    if (attackCommand == null)
                        attackCommand = owner.Commands.Raw.OfType<UnitAttack>()
                            .FirstOrDefault(value => ReferenceEquals(
                                value.Target, target));
                    if (attackCommand != null && !attackCommand.IsFinished)
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
                ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
                float moved = Vector3.Distance(start, owner.Position);
                bool spent = turnBased
                    ? maxStandard > standardBefore || maxMove > moveBefore
                    : nativeEconomy && command.IsStarted;
                bool chargeMarkerAfterPounce = owner.Descriptor.HasFact(
                    BlueprintRoot.Instance.SystemMechanics.ChargeBuff);
                bool exact = queued && command.IsStarted && executionSeen &&
                    attackCommand != null && attackCommand.IsStarted &&
                    observer.Rolls.Count == 4 &&
                    observer.WeaponAttacks.Count == 4 &&
                    limbs.Length == 4 && limbs.All(weapon =>
                        observer.Rolls.Count(value => ReferenceEquals(
                            value.Weapon, weapon)) == 1) &&
                    moved >= new Feet(10).Meters - .05f && spent &&
                    target.Descriptor.Damage > damageBefore;
                Sprint24Check("deinonychus-legal-pounce-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "queued-native-charge-pounce",
                        ["route"] = route,
                        ["queued"] = queued,
                        ["started"] = command.IsStarted,
                        ["finished"] = command.IsFinished,
                        ["executionSeen"] = executionSeen,
                        ["frames"] = frames,
                        ["movedMeters"] = moved,
                        ["movementTicks"] = movementTicks,
                        ["nativeEconomy"] = nativeEconomy,
                        ["spent"] = spent,
                        ["attackCommand"] = attackCommand?.Result.ToString(),
                        ["chargeMarkerAfterPounce"] =
                            chargeMarkerAfterPounce,
                        ["rolls"] = new JArray(observer.Rolls.Select(value =>
                            new JObject {
                                ["weapon"] = value.Weapon?.Blueprint?.AssetGuid,
                                ["natural"] = value.Roll.Value,
                                ["hit"] = value.IsHit,
                                ["secondary"] = value.Weapon != null &&
                                    value.Weapon.IsSecondary
                            })),
                        ["weaponAttackFlags"] = new JArray(
                            observer.WeaponAttacks.Select(value =>
                                new JObject {
                                    ["charge"] = value.IsCharge,
                                    ["first"] = value.IsFirstAttack,
                                    ["opportunity"] =
                                        value.IsAttackOfOpportunity
                                })),
                        ["standardCooldown"] = new JArray(standardBefore,
                            maxStandard,
                            owner.CombatState.Cooldown.StandardAction),
                        ["moveCooldown"] = new JArray(moveBefore, maxMove,
                            owner.CombatState.Cooldown.MoveAction)
                    }, "One legal queued native charge travels at least ten feet, spends ordinary charge economy, and Pounce resolves exactly one legal four-limb routine without replay.");

                Game.Instance.State.AwakeUnits.Clear();
                Game.Instance.State.AwakeUnits.AddRange(awakeBefore);
                awakeBefore = null;
                EndExpandedSummoningDetachedAbility(command);
                owner.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                int freshTurnFrames = 0;
                bool freshTurnReached = !turnBased;
                if (turnBased)
                {
                    foreach (int frame in Sprint18AdvanceRound(owner))
                        yield return frame;
                    while (++freshTurnFrames <= Sprint18AttackFrames)
                    {
                        if (Game.Instance.IsPaused)
                            Game.Instance.IsPaused = false;
                        if (Sprint18OwnerIsActing(owner))
                        {
                            freshTurnReached = true;
                            break;
                        }
                        yield return 0;
                    }
                    if (!freshTurnReached)
                        throw new InvalidOperationException(
                            "Deinonychus never reached its fresh native turn " +
                            "after the legal Pounce.");
                }
                bool chargeMarkerBeforeIllegal = owner.Descriptor.HasFact(
                    BlueprintRoot.Instance.SystemMechanics.ChargeBuff);

                observer.Clear();
                ResetExpandedSummoningHostile(fixture);
                PlaceExpandedSummoningUnit(target,
                    owner.Position + Vector3.forward * .5f);
                var illegalTarget = new TargetWrapper(target);
                bool illegalCanTarget = charge.CanTarget(illegalTarget) &&
                    charge.IsAvailable;
                var aborted = new UnitUseAbility(charge, illegalTarget);
                bool abortedCanStart = aborted.CanStart;
                if (abortedCanStart)
                {
                    owner.Commands.Run(aborted);
                    owner.Commands.InterruptAll(true);
                    EndExpandedSummoningDetachedAbility(aborted);
                }
                for (int settle = 0; settle < 8; settle++) yield return 0;
                bool chargeMarkerAfterIllegal = owner.Descriptor.HasFact(
                    BlueprintRoot.Instance.SystemMechanics.ChargeBuff);
                bool noStale = freshTurnReached &&
                    !chargeMarkerBeforeIllegal && !chargeMarkerAfterIllegal &&
                    observer.Rolls.Count == 0 &&
                    observer.WeaponAttacks.Count == 0;
                Sprint24Check("deinonychus-illegal-aborted-charge-" +
                    (turnBased ? "turn-based" : "rtwp"),
                    !illegalCanTarget && noStale,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "too-close-illegal-charge",
                        ["canTarget"] = illegalCanTarget,
                        ["canStart"] = abortedCanStart,
                        ["freshTurnReached"] = freshTurnReached,
                        ["freshTurnFrames"] = freshTurnFrames,
                        ["chargeMarkerAfterPounce"] =
                            chargeMarkerAfterPounce,
                        ["chargeMarkerBeforeIllegal"] =
                            chargeMarkerBeforeIllegal,
                        ["rolls"] = observer.Rolls.Count,
                        ["weaponAttacks"] = observer.WeaponAttacks.Count,
                        ["chargeMarkerAfter"] = chargeMarkerAfterIllegal
                    }, "A too-close charge is rejected by the native target predicate; an immediately aborted generic command emits no follow-up attack and leaves no stale charge state for the next command.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                EndExpandedSummoningDetachedAbility(command);
                owner.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                if (awakeBefore != null)
                {
                    Game.Instance.State.AwakeUnits.Clear();
                    Game.Instance.State.AwakeUnits.AddRange(awakeBefore);
                }
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                target.Descriptor.Damage = damageBefore;
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private void Sprint24DeinonychusOpportunityControl(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            ItemEntityWeapon talon = owner.Body.PrimaryHand.MaybeWeapon;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int damageBefore = target.Descriptor.Damage;
            var observer = new Sprint24AttackObserver {
                Owner = owner,
                Target = target
            };
            EventBus.Subscribe(observer);
            try
            {
                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                UnityEngine.Random.InitState(FindNativeD20Seed(18));
                RuleAttackWithWeapon attack = Rulebook.Trigger(
                    new RuleAttackWithWeapon(owner, target, talon, 0) {
                        IsAttackOfOpportunity = true,
                        IsFirstAttack = true
                    });
                bool exact = attack.AttackRoll != null &&
                    attack.AttackRoll.IsHit && observer.Rolls.Count == 1 &&
                    observer.WeaponAttacks.Count == 1 &&
                    observer.WeaponAttacks[0].IsAttackOfOpportunity &&
                    !observer.WeaponAttacks[0].IsCharge &&
                    !owner.Descriptor.HasFact(
                        BlueprintRoot.Instance.SystemMechanics.ChargeBuff);
                Sprint24Check("deinonychus-opportunity-control-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "native-opportunity-rule",
                        ["weapon"] = talon.Blueprint.AssetGuid,
                        ["natural"] = attack.AttackRoll?.Roll.Value,
                        ["hit"] = attack.AttackRoll?.IsHit,
                        ["rolls"] = observer.Rolls.Count,
                        ["weaponAttacks"] = observer.WeaponAttacks.Count,
                        ["chargeMarker"] = owner.Descriptor.HasFact(
                            BlueprintRoot.Instance.SystemMechanics.ChargeBuff)
                    }, "A native attack of opportunity remains one talon strike and cannot expose Pounce or stale charge state.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                target.Descriptor.Damage = damageBefore;
            }
        }

        private IEnumerable<int> Sprint24ReviewTyrannosaurus(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            UnitEntityData owner = CastExpandedSummoningOwnTier(fixture,
                TheropodRulesPolicy.TyrannosaurusKey);
            SetExpandedSummoningBrainActive(owner, false);
            UnitEntityData target = fixture.Hostile;
            int hitPointsBefore = target.Stats.HitPoints.BaseValue;
            int damageBefore = target.Descriptor.Damage;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int cmbBefore = owner.Stats.AdditionalCMB.BaseValue;
            Size sizeBefore = target.Descriptor.State.Size;
            try
            {
                for (int frame = 0; frame < 12; frame++) yield return 0;
                Sprint24ReadProfile(fixture, owner,
                    TheropodRulesPolicy.TyrannosaurusKey, turnBased);
                Sprint24ReadBody(owner,
                    TheropodRulesPolicy.TyrannosaurusKey, turnBased);
                if (turnBased)
                    foreach (int frame in Sprint24EnrollTurnBasedOwner(
                        fixture, owner,
                        TheropodRulesPolicy.TyrannosaurusKey))
                        yield return frame;
                owner.Stats.HitPoints.BaseValue = 100000;
                target.Stats.HitPoints.BaseValue = 100000;
                foreach (int frame in Sprint24TyrannosaurusOrdinaryCommand(
                    fixture, owner, turnBased)) yield return frame;
                Sprint24PowerfulBiteMatrix(fixture, owner, turnBased);
                Sprint24BleedingCriticalSuite(fixture, owner, turnBased);
                Sprint24GrabAndSwallow(fixture, owner, turnBased);
                foreach (int frame in Sprint24IndependentSwallowers(
                    fixture, owner, turnBased)) yield return frame;
            }
            finally
            {
                Sprint24ReleaseHold(owner, target);
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                owner.Stats.AdditionalCMB.BaseValue = cmbBefore;
                target.Stats.HitPoints.BaseValue = hitPointsBefore;
                target.Descriptor.Damage = damageBefore;
                target.Descriptor.State.Size = sizeBefore;
                ResetExpandedSummoningHostile(fixture);
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner });
            }
        }

        private IEnumerable<int> Sprint24TyrannosaurusOrdinaryCommand(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            int damageBefore = target.Descriptor.Damage;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int cmbBefore = owner.Stats.AdditionalCMB.BaseValue;
            var observer = new Sprint24AttackObserver {
                Owner = owner,
                Target = target,
                BeforeRoll = value => {
                    owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                    UnityEngine.Random.InitState(FindNativeD20Seed(18));
                    target.Descriptor.State.AddCondition(
                        UnitCondition.ImmuneToCombatManeuvers, null);
                }
            };
            EventBus.Subscribe(observer);
            try
            {
                Vector3 floor = fixture.Caster.Position + Vector3.right * 4f;
                PlaceExpandedSummoningUnit(owner, floor);
                PlaceExpandedSummoningUnit(target,
                    floor + Vector3.forward * 4f);
                UnitAttack command = null;
                foreach (int frame in Sprint24RunAttackCommand(fixture,
                    owner, target, true, value => command = value))
                    yield return frame;
                ItemEntityWeapon bite = owner.Body.PrimaryHand.MaybeWeapon;
                bool exact = command != null && command.IsFinished &&
                    command.AllAttacks.Count == 1 &&
                    observer.Rolls.Count == 1 &&
                    observer.WeaponAttacks.Count == 1 &&
                    ReferenceEquals(observer.Rolls[0].Weapon, bite) &&
                    observer.Rolls[0].IsHit &&
                    observer.Damage.Count == 1 &&
                    !observer.WeaponAttacks[0].IsCharge &&
                    !observer.WeaponAttacks[0].IsAttackOfOpportunity;
                Sprint24Check("tyrannosaurus-one-bite-command-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "real-native-one-bite-command",
                        ["commandFinished"] = command?.IsFinished,
                        ["commandResult"] = command?.Result.ToString(),
                        ["plannedAttacks"] = command == null ? -1 :
                            command.AllAttacks.Count,
                        ["rolls"] = observer.Rolls.Count,
                        ["weaponAttacks"] = observer.WeaponAttacks.Count,
                        ["damageEvents"] = new JArray(observer.Damage.Select(
                            Sprint16DamageEvent)),
                        ["weapon"] = bite?.Blueprint?.AssetGuid
                    }, "A real full-attack command is exactly one powerful bite; no claw, tail or charge-only rider appears.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                target.Descriptor.State.RemoveConditionAll(
                    UnitCondition.ImmuneToCombatManeuvers);
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                owner.Stats.AdditionalCMB.BaseValue = cmbBefore;
                target.Descriptor.Damage = damageBefore;
                Sprint24ReleaseHold(owner, target);
            }
        }

        private void Sprint24PowerfulBiteMatrix(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            ItemEntityWeapon bite = owner.Body.PrimaryHand.MaybeWeapon;
            int strengthBefore = owner.Stats.Strength.BaseValue;
            var rows = new JArray();
            bool exact = true;
            try
            {
                foreach (int score in new[] { 28, 30, 32, 34, 10, 8, 1 })
                {
                    owner.Stats.Strength.BaseValue = score;
                    int modifier = owner.Stats.Strength.Bonus;
                    var attack = new RuleAttackWithWeapon(owner,
                        fixture.Hostile, bite, 0) {
                            IsCharge = false,
                            IsFirstAttack = true
                        };
                    RuleCalculateWeaponStats stats = Rulebook.Trigger(
                        new RuleCalculateWeaponStats(owner, bite, attack));
                    BaseDamage damage = stats.DamageDescription[0]
                        .CreateDamage();
                    int native = TheropodRulesPolicy
                        .NativeOrdinaryBiteStrengthContribution(modifier);
                    int correction = TheropodRulesPolicy
                        .PowerfulBiteCorrection(modifier);
                    bool rowExact = stats.DamageDescription.Count == 1 &&
                        damage.Dice.Rolls == 4 &&
                        damage.Dice.Dice == DiceType.D6 &&
                        damage.Bonus == modifier * 2 &&
                        stats.BonusDamage == modifier * 2 &&
                        !stats.SecondaryWeapon;
                    exact &= rowExact;
                    rows.Add(new JObject {
                        ["score"] = score,
                        ["modifier"] = modifier,
                        ["nativeContribution"] = native,
                        ["ownedCorrection"] = correction,
                        ["desiredContribution"] = modifier * 2,
                        ["damage"] = Sprint16DamageLine(damage),
                        ["bonusDamage"] = stats.BonusDamage,
                        ["secondary"] = stats.SecondaryWeapon,
                        ["exact"] = rowExact
                    });
                }
                Sprint24Check("tyrannosaurus-powerful-bite-signed-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = fixture.Hostile.UniqueId,
                        ["event"] = "live-signed-strength-matrix",
                        ["weapon"] = bite.Blueprint.AssetGuid,
                        ["cases"] = rows
                    }, "At live modifiers +9, +10, +11, +12, zero and penalties, the ordinary bite keeps 4d6 and contributes exactly twice the signed Strength modifier without charge state or bundle doubling.");
            }
            finally
            {
                owner.Stats.Strength.BaseValue = strengthBefore;
            }
        }

        private static int Sprint24FindD20PairSeed(int first, int second)
        {
            UnityEngine.Random.State before = UnityEngine.Random.state;
            try
            {
                for (int seed = 0; seed < 2000000; seed++)
                {
                    UnityEngine.Random.InitState(seed);
                    int one = UnityEngine.Random.Range(1, 21);
                    int two = UnityEngine.Random.Range(1, 21);
                    if (one == first && two == second) return seed;
                }
            }
            finally { UnityEngine.Random.state = before; }
            throw new InvalidOperationException("No bounded native d20 pair seed for " +
                first + "/" + second + ".");
        }

        private void Sprint24BleedingCriticalSuite(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            ItemEntityWeapon bite = owner.Body.PrimaryHand.MaybeWeapon;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int armorClassBefore = target.Stats.AC.BaseValue;
            int hitPointsBefore = target.Stats.HitPoints.BaseValue;
            int damageBefore = target.Descriptor.Damage;
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            BlueprintBuff bleed = fixture.Blueprints.OfType<BlueprintBuff>()
                .Single(value => value.AssetGuid ==
                    "16249b8075ab8684ca105a78a047a5ef");
            var observer = new Sprint24AttackObserver {
                Owner = owner,
                Target = target
            };
            EventBus.Subscribe(observer);
            try
            {
                owner.Stats.AdditionalAttackBonus.BaseValue = 0;
                target.Stats.HitPoints.BaseValue = 100000;
                target.Descriptor.Damage = 0;
                Sprint24RemoveBuff(target, bleed);
                Sprint24ReleaseHold(owner, target);
                target.Descriptor.State.AddCondition(
                    UnitCondition.ImmuneToCombatManeuvers, null);

                int ordinaryAttackBonus = Rulebook.Trigger(
                    new RuleCalculateAttackBonusWithoutTarget(owner, bite,
                        0)).Result;
                int targetArmorClassBefore = Rulebook.Trigger(
                    new RuleCalculateAC(owner, target,
                        AttackType.Melee)).TargetAC;
                int desiredTargetArmorClass = ordinaryAttackBonus + 10;
                target.Stats.AC.BaseValue += desiredTargetArmorClass -
                    targetArmorClassBefore;
                int targetArmorClassDuring = Rulebook.Trigger(
                    new RuleCalculateAC(owner, target,
                        AttackType.Melee)).TargetAC;

                int probeSeed = FindNativeD20Seed(15);
                UnityEngine.Random.InitState(probeSeed);
                RuleAttackRoll focusProbe = Rulebook.Trigger(
                    new RuleAttackRoll(owner, target, bite, 0));
                bool carrier = focusProbe.CriticalConfirmationBonus == 4 &&
                    focusProbe.WeaponStats != null &&
                    focusProbe.WeaponStats.DoubleCriticalEdge &&
                    !focusProbe.AutoCriticalThreat &&
                    !focusProbe.AutoCriticalConfirmation;

                int unconfirmedSeed = Sprint24FindD20PairSeed(19, 1);
                UnityEngine.Random.InitState(unconfirmedSeed);
                RuleAttackWithWeapon unconfirmed = Rulebook.Trigger(
                    new RuleAttackWithWeapon(owner, target, bite, 0));
                bool noBleed = unconfirmed.AttackRoll != null &&
                    unconfirmed.AttackRoll.Roll.Value == 19 &&
                    unconfirmed.AttackRoll.IsCriticalRoll &&
                    unconfirmed.AttackRoll.CriticalConfirmationRoll.Value == 1 &&
                    !unconfirmed.AttackRoll.IsCriticalConfirmed &&
                    !target.Descriptor.HasFact(bleed);

                observer.Clear();
                target.Descriptor.Damage = 0;
                int confirmedSeed = Sprint24FindD20PairSeed(19, 20);
                UnityEngine.Random.InitState(confirmedSeed);
                RuleAttackWithWeapon confirmed = Rulebook.Trigger(
                    new RuleAttackWithWeapon(owner, target, bite, 0));
                Buff active = target.Descriptor.Buffs.GetBuff(bleed);
                PhysicalDamage physical = confirmed.MeleeDamage == null
                    ? null : confirmed.MeleeDamage.DamageBundle
                        .OfType<PhysicalDamage>().SingleOrDefault();
                bool applied = confirmed.AttackRoll != null &&
                    confirmed.AttackRoll.Roll.Value == 19 &&
                    confirmed.AttackRoll.CriticalConfirmationRoll.Value == 20 &&
                    confirmed.AttackRoll.IsCriticalConfirmed &&
                    !confirmed.AttackRoll.AutoCriticalConfirmation &&
                    active != null && physical != null &&
                    physical.CriticalModifier == 2;

                RuleDealDamage bleedDamage = null;
                bool cadence = false;
                if (active != null)
                {
                    PropertyInfo nextTick = typeof(Buff).GetProperty(
                        "NextTickTime", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (nextTick == null || !nextTick.CanWrite)
                        throw new MissingMemberException(
                            "Native bleed Buff.NextTickTime setter is unavailable.");
                    observer.Clear();
                    TimeSpan now = Game.Instance.TimeController.GameTime;
                    int round = active.RoundNumber;
                    nextTick.SetValue(active, now, null);
                    target.Descriptor.Buffs.UpdateNextEvent();
                    target.Descriptor.Buffs.Tick();
                    bleedDamage = observer.Damage.SingleOrDefault(value =>
                        value.AttackRoll == null);
                    BaseDamage chunk = bleedDamage == null ? null :
                        bleedDamage.DamageBundle.SingleOrDefault();
                    int once = observer.Damage.Count;
                    target.Descriptor.Buffs.Tick();
                    cadence = chunk != null &&
                        chunk.Dice.Rolls == 2 &&
                        chunk.Dice.Dice == DiceType.D6 &&
                        chunk.Bonus == 0 &&
                        active.RoundNumber == round + 1 &&
                        observer.Damage.Count == once && once == 1;
                }
                target.Descriptor.Damage = Math.Max(20,
                    target.Descriptor.Damage);
                RuleHealDamage healing = Rulebook.Trigger(
                    new RuleHealDamage(owner, target,
                        new DiceFormula(0, DiceType.D6), 5));
                bool removedByHealing = healing.Value > 0 &&
                    !target.Descriptor.HasFact(bleed);
                bool noGrabSideEffect =
                    owner.Get<UnitPartGrappleInitiator>() == null &&
                    target.Get<UnitPartGrappleTarget>() == null &&
                    SummonHoldComponent.HeldTarget(owner) == null &&
                    target.Get<UnitPartSwallowed>() == null;

                bool exact = carrier && noBleed && applied && cadence &&
                    removedByHealing && noGrabSideEffect &&
                    observer.WeaponAttacks.Count == 0;
                Sprint24Check("tyrannosaurus-critical-suite-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "native-critical-feats-and-bleed",
                        ["weapon"] = bite.Blueprint.AssetGuid,
                        ["focusProbeSeed"] = probeSeed,
                        ["criticalFocusBonus"] =
                            focusProbe.CriticalConfirmationBonus,
                        ["doubleCriticalEdge"] =
                            focusProbe.WeaponStats?.DoubleCriticalEdge,
                        ["autoThreat"] = focusProbe.AutoCriticalThreat,
                        ["autoConfirmation"] =
                            focusProbe.AutoCriticalConfirmation,
                        ["ordinaryAttackBonus"] = ordinaryAttackBonus,
                        ["targetArmorClassBefore"] =
                            targetArmorClassBefore,
                        ["targetArmorClassDuring"] =
                            targetArmorClassDuring,
                        ["desiredTargetArmorClass"] =
                            desiredTargetArmorClass,
                        ["unconfirmedSeed"] = unconfirmedSeed,
                        ["unconfirmedNatural"] =
                            unconfirmed.AttackRoll?.Roll.Value,
                        ["unconfirmedConfirmation"] =
                            unconfirmed.AttackRoll?
                                .CriticalConfirmationRoll.Value,
                        ["unconfirmedThreat"] =
                            unconfirmed.AttackRoll?.IsCriticalRoll,
                        ["unconfirmedConfirmed"] =
                            unconfirmed.AttackRoll?.IsCriticalConfirmed,
                        ["unconfirmedAutoConfirmation"] =
                            unconfirmed.AttackRoll?
                                .AutoCriticalConfirmation,
                        ["unconfirmedConfirmationTotal"] =
                            unconfirmed.AttackRoll == null ? -1 :
                            unconfirmed.AttackRoll
                                .CriticalConfirmationRoll.Value +
                            unconfirmed.AttackRoll.AttackBonus +
                            unconfirmed.AttackRoll
                                .CriticalConfirmationBonus,
                        ["unconfirmedTargetCriticalAc"] =
                            unconfirmed.AttackRoll?.TargetCriticalAC,
                        ["confirmedSeed"] = confirmedSeed,
                        ["confirmedNatural"] =
                            confirmed.AttackRoll?.Roll.Value,
                        ["confirmedConfirmation"] =
                            confirmed.AttackRoll?
                                .CriticalConfirmationRoll.Value,
                        ["confirmed"] =
                            confirmed.AttackRoll?.IsCriticalConfirmed,
                        ["criticalMultiplier"] = physical == null ? -1 :
                            (int)physical.CriticalModifier,
                        ["bleedApplied"] = active != null,
                        ["bleedTick"] = bleedDamage == null ? null :
                            Sprint16DamageEvent(bleedDamage),
                        ["oneBleedTick"] = cadence,
                        ["healing"] = healing.Value,
                        ["removedByHealing"] = removedByHealing,
                        ["noGrabSideEffect"] = noGrabSideEffect
                    }, "Improved Critical gives a native 19-20 threat, Critical Focus adds +4 without automatic confirmation, only a confirmed exact bite applies one native 2d6 bleed cadence, and healing removes it.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                Sprint24RemoveBuff(target, bleed);
                Sprint24ReleaseHold(owner, target);
                target.Descriptor.State.RemoveConditionAll(
                    UnitCondition.ImmuneToCombatManeuvers);
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                target.Stats.AC.BaseValue = armorClassBefore;
                target.Stats.HitPoints.BaseValue = hitPointsBefore;
                target.Descriptor.Damage = damageBefore;
                UnityEngine.Random.state = randomBefore;
            }
        }

        private static void Sprint24RemoveBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            Buff buff = unit == null || blueprint == null ? null :
                unit.Descriptor.Buffs.GetBuff(blueprint);
            if (buff != null) unit.Descriptor.Buffs.RemoveFact(buff);
        }

        private void Sprint24GrabAndSwallow(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, bool turnBased)
        {
            UnitEntityData target = fixture.Hostile;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            ItemEntityWeapon bite = owner.Body.PrimaryHand.MaybeWeapon;
            Size sizeBefore = target.Descriptor.State.Size;
            int damageBefore = target.Descriptor.Damage;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            int cmbBefore = owner.Stats.AdditionalCMB.BaseValue;
            int hitPointsBefore = target.Stats.HitPoints.BaseValue;
            var observer = new Sprint16RuleObserver {
                Owner = owner,
                Target = target
            };
            EventBus.Subscribe(observer);
            try
            {
                if (grab == null || bite == null)
                    throw new InvalidOperationException(
                        "Tyrannosaurus exact bite grab carrier is absent.");
                bool cleanStart =
                    owner.Get<UnitPartGrappleInitiator>() == null &&
                    target.Get<UnitPartGrappleTarget>() == null &&
                    SummonHoldComponent.HeldTarget(owner) == null &&
                    target.Get<UnitPartSwallowed>() == null;
                owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                owner.Stats.AdditionalCMB.BaseValue = 100;
                target.Stats.HitPoints.BaseValue = 100000;
                int delta = Rulebook.Trigger(new RuleCalculateCMB(owner,
                        target, CombatManeuver.Grapple)).Result -
                    Rulebook.Trigger(new RuleCalculateCMB(owner, target,
                        CombatManeuver.Trip)).Result;
                bool exactLimb = grab.IsGrabLimb(owner, bite) &&
                    Sprint19Limbs(owner).Length == 1 &&
                    grab.MaxHeldTargets == 1 && grab.RakeLimbCount == 0 &&
                    delta == TheropodRulesPolicy.TyrannosaurusGrappleBonus;

                target.Descriptor.State.Size = Size.Colossal;
                bool oversized = grab.TryGrab(target, bite, true);
                target.Descriptor.State.Size = Size.Gargantuan;
                bool miss = grab.TryGrab(target, bite, false);
                bool wrongLimb = grab.TryGrab(target,
                    target.Body.PrimaryHand.MaybeWeapon, true);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool sameSize = grab.TryGrab(target, bite, true);
                bool sameSizeCannotSwallow =
                    !grab.IsSwallowSizeAllowed(owner, target) &&
                    target.Get<UnitPartSwallowed>() == null;
                Buff sameHeld = SummonHoldComponent.HeldState(owner,
                    target, grab);
                if (sameHeld != null)
                {
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    sameHeld.TickMechanics();
                    Buff hold = owner.Descriptor.Buffs.GetBuff(
                        grab.HoldBuff);
                    if (hold != null)
                    {
                        UnityEngine.Random.InitState(FindNativeD20Seed(20));
                        hold.TickMechanics();
                    }
                }
                bool sameSizeMaintainedNotSwallowed = sameSize &&
                    target.Get<UnitPartSwallowed>() == null &&
                    ReferenceEquals(SummonHoldComponent.HeldTarget(owner),
                        target);
                Sprint24ReleaseHold(owner, target);
                ResetExpandedSummoningHostile(fixture);

                target.Descriptor.State.Size = Size.Huge;
                observer.Clear();
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool grabbed = grab.TryGrab(target, bite, true);
                Buff held = SummonHoldComponent.HeldState(owner, target,
                    grab);
                bool immediateRefused = grabbed && held != null &&
                    !SummonHoldComponent.IsHeldSinceRoundStart(owner,
                        target) && target.Get<UnitPartSwallowed>() == null;
                if (held == null)
                    throw new InvalidOperationException(
                        "Tyrannosaurus Huge target hold did not establish reciprocal state.");
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                held.TickMechanics();
                bool laterEligible = SummonHoldComponent
                    .IsHeldSinceRoundStart(owner, target);
                observer.Clear();
                Buff ownerHold = owner.Descriptor.Buffs.GetBuff(
                    grab.HoldBuff);
                if (ownerHold == null)
                    throw new InvalidOperationException(
                        "Tyrannosaurus owner hold buff is absent.");
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                ownerHold.TickMechanics();
                UnitPartSwallowed swallowedPart = target
                    .Get<UnitPartSwallowed>();
                UnitPartSwallowWhole swallower = owner
                    .Get<UnitPartSwallowWhole>();
                Buff swallowed = target.Descriptor.Buffs.GetBuff(
                    grab.SwallowedBuff);
                RuleDealDamage swallowBite = observer.Damage
                    .SingleOrDefault();
                BaseDamage biteChunk = swallowBite == null ? null :
                    swallowBite.DamageBundle.SingleOrDefault();
                bool swallowedState = laterEligible &&
                    swallowedPart != null &&
                    ReferenceEquals(swallowedPart.Swallower.Value, owner) &&
                    swallowed != null &&
                    ReferenceEquals(swallowed.MaybeContext.MaybeCaster,
                        owner) && swallower != null &&
                    swallower.SwallowedUnits.Count == 1 &&
                    target.Get<UnitPartGrappleTarget>() == null &&
                    biteChunk != null && biteChunk.Dice.Rolls == 4 &&
                    biteChunk.Dice.Dice == DiceType.D6 &&
                    biteChunk.Bonus == 22;

                var ticks = new JArray();
                bool cadence = swallowedState;
                PropertyInfo nextTick = typeof(Buff).GetProperty(
                    "NextTickTime", BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (nextTick == null || !nextTick.CanWrite)
                    throw new MissingMemberException(
                        "Native swallowed Buff.NextTickTime setter is unavailable.");
                TimeSpan gameTime = Game.Instance.TimeController.GameTime;
                for (int round = 1; round <= 3 && swallowed != null;
                    round++)
                {
                    observer.Clear();
                    int priorRound = swallowed.RoundNumber;
                    nextTick.SetValue(swallowed, gameTime, null);
                    target.Descriptor.Buffs.UpdateNextEvent();
                    target.Descriptor.Buffs.Tick();
                    RuleDealDamage damage = observer.Damage
                        .SingleOrDefault();
                    BaseDamage chunk = damage == null ? null :
                        damage.DamageBundle.SingleOrDefault();
                    bool one = chunk is PhysicalDamage &&
                        damage.DamageBundle.Count() == 1 &&
                        chunk.Dice.Rolls == 2 &&
                        chunk.Dice.Dice == DiceType.D8 &&
                        chunk.Bonus == 11 &&
                        ReferenceEquals(damage.Initiator, owner) &&
                        damage.AttackRoll == null &&
                        observer.Attacks.Count == 0 &&
                        observer.Checks.Count == 0 &&
                        swallowed.RoundNumber == priorRound + 1 &&
                        (TimeSpan)nextTick.GetValue(swallowed, null) >
                            gameTime;
                    target.Descriptor.Buffs.Tick();
                    one &= observer.Damage.Count == 1;
                    cadence &= one;
                    ticks.Add(new JObject {
                        ["laterRound"] = round,
                        ["oneOwnedBundle"] = one,
                        ["nativeRound"] = swallowed.RoundNumber,
                        ["damage"] = damage == null ? null :
                            Sprint16DamageEvent(damage)
                    });
                }
                observer.Clear();
                if (swallowedPart != null)
                    swallowedPart.TryToBreakFree();
                bool initialEscapeGate = swallowedPart != null &&
                    !swallowedPart.ShouldBeFree &&
                    observer.Checks.Count == 0 &&
                    swallowedPart.SecondsToBreakFreeAttempt > 0;
                if (swallowedPart != null)
                {
                    swallowedPart.SecondsToBreakFreeAttempt = 0;
                    target.Descriptor.Stats.BaseAttackBonus.BaseValue = 200;
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    swallowedPart.TryToBreakFree();
                }
                bool nativeEscape = swallowedPart != null &&
                    swallowedPart.ShouldBeFree &&
                    observer.Checks.Count == 1;
                observer.Clear();
                if (swallower != null) swallower.Free(target);
                bool removal = target.Get<UnitPartSwallowed>() == null &&
                    !target.Descriptor.HasFact(grab.SwallowedBuff) &&
                    observer.Damage.Count == 0 &&
                    !target.Descriptor.State.HasCondition(
                        UnitCondition.CantAct);

                bool exact = cleanStart && exactLimb && !oversized && !miss &&
                    !wrongLimb && sameSizeCannotSwallow &&
                    sameSizeMaintainedNotSwallowed && immediateRefused &&
                    swallowedState && cadence && initialEscapeGate &&
                    nativeEscape && removal;
                Sprint24Check("tyrannosaurus-grab-swallow-" +
                    (turnBased ? "turn-based" : "rtwp"), exact,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] = "bite-grab-later-swallow-cadence",
                        ["bite"] = bite.Blueprint.AssetGuid,
                        ["cleanStart"] = cleanStart,
                        ["grappleLessTrip"] = delta,
                        ["maxHeld"] = grab.MaxHeldTargets,
                        ["oversizedRefused"] = !oversized,
                        ["missRefused"] = !miss,
                        ["wrongLimbRefused"] = !wrongLimb,
                        ["sameSizeGrabbed"] = sameSize,
                        ["sameSizeSwallowRefused"] =
                            sameSizeCannotSwallow,
                        ["sameSizeMaintainedNotSwallowed"] =
                            sameSizeMaintainedNotSwallowed,
                        ["hugeGrabbed"] = grabbed,
                        ["immediateSwallowRefused"] = immediateRefused,
                        ["laterEligible"] = laterEligible,
                        ["swallowBite"] = swallowBite == null ? null :
                            Sprint16DamageEvent(swallowBite),
                        ["swallowed"] = swallowedState,
                        ["laterRoundTicks"] = ticks,
                        ["nativeInitialSixSecondGate"] =
                            initialEscapeGate,
                        ["nativeEscapeCheck"] = nativeEscape,
                        ["removalWithoutDamage"] = removal,
                        ["interiorDisposition"] =
                            "OwnerAcceptedEngineLimitation: SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED"
                    }, "Only the exact bite grabs with +4; same-size prey may be held but never swallowed; one-category-smaller prey is swallowed only after the later check, then receives one owned 2d8+11 packet per native buff round and can escape cleanly.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                Sprint24ReleaseHold(owner, target);
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                owner.Stats.AdditionalCMB.BaseValue = cmbBefore;
                target.Stats.HitPoints.BaseValue = hitPointsBefore;
                target.Descriptor.Damage = damageBefore;
                target.Descriptor.State.Size = sizeBefore;
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private static void Sprint24ReleaseHold(UnitEntityData owner,
            UnitEntityData target)
        {
            if (owner == null || target == null) return;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            UnitPartSwallowWhole swallower = owner.Get<UnitPartSwallowWhole>();
            if (swallower != null && target.Get<UnitPartSwallowed>() != null)
                swallower.Free(target);
            if (grab != null)
                SummonHoldComponent.ReleaseLink(owner, target, grab, true);
            if (owner.Get<UnitPartGrappleInitiator>() != null)
                owner.Remove<UnitPartGrappleInitiator>();
            if (target.Get<UnitPartGrappleTarget>() != null)
                target.Remove<UnitPartGrappleTarget>();
        }

        private IEnumerable<int> Sprint24IndependentSwallowers(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData firstOwner, bool turnBased)
        {
            UnitEntityData firstTarget = fixture.Hostile;
            UnitEntityData secondOwner = CastExpandedSummoningQuietUnit(
                fixture, TheropodRulesPolicy.TyrannosaurusKey);
            BlueprintUnit secondBlueprint = null;
            UnitEntityData secondTarget = Sprint18SecondTarget(fixture,
                out secondBlueprint);
            // The Sprint 22 native clock advances only request-owned fixture
            // actors.  This disposable hostile normally needs no controller,
            // but the post-release continuation deliberately observes its
            // real later-round BuffCollection delivery.
            fixture.Created.Add(secondTarget);
            UnitEntityData[] owners = { firstOwner, secondOwner };
            UnitEntityData[] targets = { firstTarget, secondTarget };
            Size firstSize = firstTarget.Descriptor.State.Size;
            int[] attackBefore = owners.Select(value =>
                value.Stats.AdditionalAttackBonus.BaseValue).ToArray();
            int[] cmbBefore = owners.Select(value =>
                value.Stats.AdditionalCMB.BaseValue).ToArray();
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            var observers = new[] {
                new Sprint16RuleObserver { Owner = firstOwner,
                    Target = firstTarget },
                new Sprint16RuleObserver { Owner = secondOwner,
                    Target = secondTarget }
            };
            foreach (Sprint16RuleObserver observer in observers)
                EventBus.Subscribe(observer);
            JObject secondPairTurnEnrollment = null;
            int initialOwnerTurnFrames = 0;
            bool initialOwnerTurnReady = !turnBased;
            try
            {
                if (turnBased)
                {
                    secondPairTurnEnrollment = new JObject();
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(
                        new[] { secondOwner }, secondTarget,
                        secondPairTurnEnrollment))
                        yield return frame;
                    if ((bool?)secondPairTurnEnrollment["admitted"] != true)
                        throw new InvalidOperationException(
                            "Independent Tyrannosaurus pair did not enter " +
                            "native turn order.");
                    while (++initialOwnerTurnFrames <=
                        Sprint18AttackFrames)
                    {
                        if (Game.Instance.IsPaused)
                            Game.Instance.IsPaused = false;
                        if (Sprint18OwnerIsActing(firstOwner))
                        {
                            initialOwnerTurnReady = true;
                            break;
                        }
                        yield return 0;
                    }
                    if (!initialOwnerTurnReady)
                        throw new InvalidOperationException(
                            "First Tyrannosaurus did not reach its native " +
                            "turn before the independent swallow trial.");
                }
                foreach (UnitEntityData target in targets)
                {
                    target.Descriptor.State.Size = Size.Huge;
                    target.Stats.HitPoints.BaseValue = 100000;
                }
                for (int index = 0; index < owners.Length; index++)
                {
                    UnitEntityData owner = owners[index];
                    UnitEntityData target = targets[index];
                    SummonGrabComponent grab = SummonGrabComponent.Find(owner);
                    ItemEntityWeapon bite = owner.Body.PrimaryHand.MaybeWeapon;
                    owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                    owner.Stats.AdditionalCMB.BaseValue = 100;
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    if (grab == null || !grab.TryGrab(target, bite, true))
                        throw new InvalidOperationException(
                            "Independent Tyrannosaurus hold did not establish: " +
                            index + ".");
                    Buff held = SummonHoldComponent.HeldState(owner,
                        target, grab);
                    if (held == null)
                        throw new InvalidOperationException(
                            "Independent Tyrannosaurus held state is absent: " +
                            index + ".");
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    held.TickMechanics();
                    Buff ownerHold = owner.Descriptor.Buffs.GetBuff(
                        grab.HoldBuff);
                    if (ownerHold == null)
                        throw new InvalidOperationException(
                            "Independent Tyrannosaurus owner hold is absent: " +
                            index + ".");
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    ownerHold.TickMechanics();
                }

                Buff[] swallowed = owners.Select((owner, index) =>
                    targets[index].Descriptor.Buffs.GetBuff(
                        SummonGrabComponent.Find(owner).SwallowedBuff))
                    .ToArray();
                UnitPartSwallowWhole[] swallowers = owners.Select(value =>
                    value.Get<UnitPartSwallowWhole>()).ToArray();
                bool independentState = swallowed.All(value =>
                        value != null) &&
                    swallowers.All(value => value != null &&
                        value.SwallowedUnits.Count == 1) &&
                    swallowed.Select((value, index) =>
                        ReferenceEquals(value.MaybeContext.MaybeCaster,
                            owners[index]) &&
                        ReferenceEquals(targets[index]
                            .Get<UnitPartSwallowed>()?.Swallower.Value,
                            owners[index])).All(value => value);

                PropertyInfo nextTick = typeof(Buff).GetProperty(
                    "NextTickTime", BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (nextTick == null || !nextTick.CanWrite)
                    throw new MissingMemberException(
                        "Native swallowed Buff.NextTickTime setter is unavailable.");
                foreach (Sprint16RuleObserver observer in observers)
                    observer.Clear();
                TimeSpan gameTime = Game.Instance.TimeController.GameTime;
                // Deliberately make only pair one due now. Pair two remains
                // on its own native schedule so its later packet can prove
                // both independence and real round continuation after pair
                // one is released.
                nextTick.SetValue(swallowed[0], gameTime, null);
                targets[0].Descriptor.Buffs.UpdateNextEvent();
                targets[0].Descriptor.Buffs.Tick();
                targets[0].Descriptor.Buffs.Tick();
                RuleDealDamage firstInitialDamage = observers[0].Damage
                    .SingleOrDefault();
                BaseDamage firstInitialChunk = firstInitialDamage == null ?
                    null : firstInitialDamage.DamageBundle.SingleOrDefault();
                bool firstInitialPayloadExact = firstInitialDamage != null &&
                    ReferenceEquals(firstInitialDamage.Initiator,
                        firstOwner) &&
                    ReferenceEquals(firstInitialDamage.Target,
                        firstTarget) &&
                    firstInitialDamage.AttackRoll == null &&
                    firstInitialDamage.DamageBundle.Count() == 1 &&
                    firstInitialChunk != null &&
                    firstInitialChunk.Dice.Rolls == 2 &&
                    firstInitialChunk.Dice.Dice == DiceType.D8 &&
                    firstInitialChunk.Bonus == 11 &&
                    observers[1].Damage.Count == 0;

                int firstInitialPeriodicEvents = observers[0].Damage.Count;
                int secondInitialPeriodicEvents = observers[1].Damage.Count;
                int secondDamageBeforeRelease = secondInitialPeriodicEvents;
                swallowers[0].Free(firstTarget);
                Buff secondLiveSwallowed = secondTarget.Descriptor.Buffs
                    .GetBuff(SummonGrabComponent.Find(secondOwner)
                        .SwallowedBuff);
                TimeSpan continuationGameTime =
                    Game.Instance.TimeController.GameTime;
                double continuationDelayBefore = secondLiveSwallowed == null
                    ? -1d : ((TimeSpan)nextTick.GetValue(
                        secondLiveSwallowed, null) -
                        continuationGameTime).TotalSeconds;
                bool releaseDidNotTickSecond =
                    observers[1].Damage.Count == secondDamageBeforeRelease;
                bool secondBuffPreserved = secondLiveSwallowed != null &&
                    ReferenceEquals(secondLiveSwallowed.MaybeContext
                        .MaybeCaster, secondOwner) &&
                    ReferenceEquals(secondTarget.Get<UnitPartSwallowed>()?
                        .Swallower.Value, secondOwner);
                bool firstReleaseDoesNotTouchSecond =
                    firstTarget.Get<UnitPartSwallowed>() == null &&
                    secondTarget.Get<UnitPartSwallowed>() != null &&
                    swallowers[1].SwallowedUnits.Count == 1 &&
                    releaseDidNotTickSecond && secondBuffPreserved;
                observers[0].Clear();
                observers[1].Clear();
                int secondRound = secondLiveSwallowed == null ? -1 :
                    secondLiveSwallowed.RoundNumber;
                CombatController continuationController = Game.Instance
                    .TurnBasedCombatController;
                int continuationControllerRoundBefore =
                    continuationController.RoundNumber;
                int continuationNativePulses = 0;
                int continuationEndedTurns = 0;
                string continuationLastTurnState = null;
                var continuationTurnTransitions = new JArray();
                var continuationPulsedPairTurns =
                    new HashSet<TurnController>();
                while (secondBuffPreserved &&
                    observers[1].Damage.Count == 0 &&
                    continuationNativePulses < Sprint18AttackFrames)
                {
                    if (turnBased && CombatController
                        .IsInTurnBasedCombat())
                    {
                        TurnController turn = continuationController
                            .CurrentTurn;
                        string state = "round=" +
                            continuationController.RoundNumber +
                            ";unit=" + (turn?.Unit?.UniqueId ?? "<none>") +
                            ";status=" + (turn == null ? "<none>" :
                                turn.Status.ToString());
                        if (state != continuationLastTurnState)
                        {
                            continuationTurnTransitions.Add(state);
                            continuationLastTurnState = state;
                        }
                        bool pairTurn = turn != null &&
                            (ReferenceEquals(turn.Unit, secondOwner) ||
                                ReferenceEquals(turn.Unit, secondTarget));
                        bool leavePairCurrentForOnePulse = pairTurn &&
                            continuationPulsedPairTurns.Add(turn);
                        if (!leavePairCurrentForOnePulse && turn != null &&
                            (turn.Status ==
                                TurnController.TurnStatus.Preparing ||
                            turn.Status ==
                                TurnController.TurnStatus.Acting))
                        {
                            Sprint18EndTurn(turn);
                            continuationEndedTurns++;
                        }
                    }
                    continuationNativePulses++;
                    yield return 0;
                }
                TimeSpan continuationGameTimeAfter =
                    Game.Instance.TimeController.GameTime;
                Buff secondAfterContinuation = secondTarget.Descriptor.Buffs
                    .GetBuff(SummonGrabComponent.Find(secondOwner)
                        .SwallowedBuff);
                double continuationDelayAfter =
                    secondAfterContinuation == null ? -1d :
                    ((TimeSpan)nextTick.GetValue(secondAfterContinuation,
                        null) - continuationGameTimeAfter).TotalSeconds;
                RuleDealDamage continuationDamage = observers[1].Damage
                    .SingleOrDefault();
                BaseDamage continuationChunk = continuationDamage == null ?
                    null : continuationDamage.DamageBundle.SingleOrDefault();
                bool firstStayedReleased = observers[0].Damage.Count == 0 &&
                    firstTarget.Get<UnitPartSwallowed>() == null;
                bool continuationRoundAdvanced = !turnBased ||
                    continuationController.RoundNumber >
                        continuationControllerRoundBefore;
                bool secondContinues = secondBuffPreserved &&
                    ReferenceEquals(secondAfterContinuation,
                        secondLiveSwallowed) &&
                    continuationDamage != null &&
                    ReferenceEquals(continuationDamage.Initiator,
                        secondOwner) &&
                    ReferenceEquals(continuationDamage.Target,
                        secondTarget) &&
                    continuationDamage.AttackRoll == null &&
                    continuationDamage.DamageBundle.Count() == 1 &&
                    continuationChunk != null &&
                    continuationChunk.Dice.Rolls == 2 &&
                    continuationChunk.Dice.Dice == DiceType.D8 &&
                    continuationChunk.Bonus == 11 &&
                    secondAfterContinuation.RoundNumber == secondRound + 1 &&
                    continuationDelayAfter > 0d && firstStayedReleased &&
                    continuationRoundAdvanced;
                swallowers[1].Free(secondTarget);
                bool cleanup = targets.All(value =>
                    value.Get<UnitPartSwallowed>() == null) &&
                    swallowers.All(value => value.SwallowedUnits.Count == 0);

                Sprint24Check("tyrannosaurus-independent-swallowers-" +
                    (turnBased ? "turn-based" : "rtwp"),
                    independentState && firstInitialPayloadExact &&
                    firstReleaseDoesNotTouchSecond && secondContinues &&
                    cleanup, new JObject {
                        ["unit"] = firstOwner.UniqueId,
                        ["secondUnit"] = secondOwner.UniqueId,
                        ["target"] = firstTarget.UniqueId,
                        ["secondTarget"] = secondTarget.UniqueId,
                        ["event"] =
                            "independent-predators-victims-and-owned-payloads",
                        ["secondPairTurnEnrollment"] =
                            secondPairTurnEnrollment,
                        ["initialOwnerTurnFrames"] =
                            initialOwnerTurnFrames,
                        ["initialOwnerTurnReady"] =
                            initialOwnerTurnReady,
                        ["independentState"] = independentState,
                        ["firstInitialPeriodicEvents"] =
                            firstInitialPeriodicEvents,
                        ["secondInitialPeriodicEvents"] =
                            secondInitialPeriodicEvents,
                        ["releasedFirstPeriodicEvents"] =
                            observers[0].Damage.Count,
                        ["secondContinuedPeriodicEvents"] =
                            observers[1].Damage.Count,
                        ["secondBuffReferenceStable"] =
                            ReferenceEquals(secondAfterContinuation,
                                secondLiveSwallowed),
                        ["secondBuffPreserved"] = secondBuffPreserved,
                        ["releaseDidNotTickSecond"] =
                            releaseDidNotTickSecond,
                        ["firstStayedReleased"] = firstStayedReleased,
                        ["firstInitialPayloadExact"] =
                            firstInitialPayloadExact,
                        ["continuationNativePulses"] =
                            continuationNativePulses,
                        ["continuationEndedTurns"] =
                            continuationEndedTurns,
                        ["continuationPulsedPairTurns"] =
                            continuationPulsedPairTurns.Count,
                        ["continuationControllerRoundBefore"] =
                            continuationControllerRoundBefore,
                        ["continuationControllerRoundAfter"] =
                            continuationController.RoundNumber,
                        ["continuationRoundAdvanced"] =
                            continuationRoundAdvanced,
                        ["continuationTurnTransitions"] =
                            continuationTurnTransitions,
                        ["continuationClockCoverage"] =
                            _sprint22Clock?.DescribeTurnCoverage(),
                        ["continuationAdvancedSeconds"] =
                            (continuationGameTimeAfter -
                                continuationGameTime).TotalSeconds,
                        ["continuationDelayBeforeSeconds"] =
                            continuationDelayBefore,
                        ["continuationDelayAfterSeconds"] =
                            continuationDelayAfter,
                        ["continuationDamage"] =
                            continuationDamage == null ? null :
                            Sprint16DamageEvent(continuationDamage),
                        ["firstReleasePreservedSecond"] =
                            firstReleaseDoesNotTouchSecond,
                        ["secondContinuedAfterFirstRelease"] =
                            secondContinues,
                        ["cleaned"] = cleanup
                    }, "Two simultaneous Tyrannosaurus/victim pairs own distinct swallowed state and 2d8+11 round payloads; releasing one cannot release, tick or relink the other.");
            }
            finally
            {
                foreach (Sprint16RuleObserver observer in observers)
                    EventBus.Unsubscribe(observer);
                for (int index = 0; index < owners.Length; index++)
                {
                    Sprint24ReleaseHold(owners[index], targets[index]);
                    owners[index].Stats.AdditionalAttackBonus.BaseValue =
                        attackBefore[index];
                    owners[index].Stats.AdditionalCMB.BaseValue =
                        cmbBefore[index];
                }
                firstTarget.Descriptor.State.Size = firstSize;
                UnityEngine.Random.state = randomBefore;
                ResetExpandedSummoningHostile(fixture);
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { secondOwner, secondTarget });
                if (secondBlueprint != null)
                    UnityEngine.Object.Destroy(secondBlueprint);
            }
        }

        private IEnumerable<int> Sprint24VisualLifecycle(
            ExpandedSummoningCorrectionFixture fixture)
        {
            BlueprintUnit donorBlueprint = fixture.Blueprints
                .OfType<BlueprintUnit>().Single(value => value.AssetGuid ==
                    TheropodVisualPolicy.DonorGuid);
            UnitEntityData control = Game.Instance.EntityCreator.SpawnUnit(
                donorBlueprint, fixture.Caster.Position,
                Quaternion.identity, fixture.Scene);
            fixture.Created.Add(control);
            SetExpandedSummoningBrainActive(control, false);
            SkinnedMeshRenderer[] controlSkins = control.View
                .GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Mesh[] donorMeshes = controlSkins.Select(value =>
                value.sharedMesh).ToArray();
            bool[] donorEnabled = controlSkins.Select(value =>
                value.enabled).ToArray();
            try
            {
                foreach (string key in TheropodVisualPolicy.Keys)
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
                                            "Sprint24 request-owned post-swap fault");
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
                        SkinnedMeshRenderer[] skins = unit.View
                            .GetComponentsInChildren<SkinnedMeshRenderer>(
                                true);
                        string expectedName = "KMG_" + key + "_Original";
                        SkinnedMeshRenderer body = skins.SingleOrDefault(
                            value => value.name ==
                                TheropodVisualPolicy.BodyRendererName);
                        SkinnedMeshRenderer accessory = skins.SingleOrDefault(
                            value => value.name ==
                                TheropodVisualPolicy.AccessoryRendererName);
                        bool original = body != null && body.enabled &&
                            body.sharedMesh != null &&
                            body.sharedMesh.name == expectedName;
                        UnityEngine.Object[] owned =
                            ExpandedSummoningPteranodonViewPatch
                                .CaptureTheropodOwnedResources(unit.View);
                        string outcome = ExpandedSummoningPteranodonViewPatch
                            .DescribeView(unit.View);
                        bool donorRestored = body != null && body.enabled &&
                            donorMeshes.Contains(body.sharedMesh) &&
                            accessory != null && accessory.enabled;
                        bool bound = cycle == 0
                            ? faultReached == 1 && !original &&
                                donorRestored && owned.Length == 0 &&
                                outcome.StartsWith(
                                    "donor-visual:attach-failed:",
                                    StringComparison.Ordinal)
                            : faultReached == 0 && original &&
                                accessory != null && !accessory.enabled &&
                                outcome.StartsWith("visual:attached;",
                                    StringComparison.Ordinal) &&
                                owned.OfType<Mesh>().Count() == 1 &&
                                owned.OfType<Material>().Any() &&
                                EntityFadedIn(unit);
                        string supportingFrame = original
                            ? CaptureSprint17OriginalBodyFrame(unit, body,
                                "sprint24-" + key + "-cycle-" + cycle)
                            : "donor-fallback";
                        Sprint24Check("view-" + key + "-cycle-" + cycle,
                            bound, new JObject {
                                ["unit"] = unit.UniqueId,
                                ["event"] =
                                    "fault-rollback-recovery-and-native-fader",
                                ["owner"] = unit.Blueprint.AssetGuid,
                                ["faultReached"] = faultReached,
                                ["original"] = original,
                                ["bodyEnabled"] = body != null &&
                                    body.enabled,
                                ["accessoryEnabled"] = accessory != null &&
                                    accessory.enabled,
                                ["donorRestored"] = donorRestored,
                                ["fadedIn"] = EntityFadedIn(unit),
                                ["dissolve"] = DissolveAmount(unit),
                                ["status"] = outcome,
                                ["supportingFrame"] = supportingFrame,
                                ["owned"] = new JArray(owned.Select(value =>
                                    new JObject {
                                        ["id"] = value.GetInstanceID(),
                                        ["name"] = value.name,
                                        ["type"] = value.GetType().FullName
                                    }))
                            }, "A request-local post-swap fault restores both exact native renderers; two subsequent views bind one private body, disable only the per-instance accessory, and fade in once.");

                        Mesh cachedMesh;
                        Texture2D cachedPaint;
                        string[] cachedBones;
                        string cacheStatus;
                        bool cacheLoaded = PteranodonAssetRuntime
                            .TryGetTheropodVisual(key, out cachedMesh,
                                out cachedBones, out cachedPaint,
                                out cacheStatus);
                        var view = unit.View;
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
                        ExpandedSummoningPteranodonViewPatch.ReleasePhase2View(
                            view);
                        Sprint24Check("view-" + key + "-teardown-" + cycle,
                            released && borrowedSurvive,
                            new JObject {
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
                            }, "Native destruction releases only captured private mesh/material objects and preserves donor and immutable cache identities; repeated release is harmless.");
                    }
                    foreach (int frame in Sprint24DeathExpiryLifecycle(
                        fixture, key)) yield return frame;
                }
                SkinnedMeshRenderer[] after = control.View
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true);
                bool isolated = control.View != null &&
                    ExpandedSummoningPteranodonViewPatch.DescribeView(
                        control.View) == "not-attempted" &&
                    donorMeshes.SequenceEqual(after.Select(value =>
                        value.sharedMesh)) &&
                    donorEnabled.SequenceEqual(after.Select(value =>
                        value.enabled)) &&
                    donorMeshes.All(value => value != null);
                Sprint24Check("native-donor-isolation", isolated,
                    new JObject {
                        ["unit"] = control.UniqueId,
                        ["event"] = "donor-negative-control",
                        ["owner"] = donorBlueprint.AssetGuid,
                        ["rendererCount"] = after.Length,
                        ["unchanged"] = isolated
                    }, "The native donor retains the exact renderer mesh and enabled-state references across faults, recovered instances and teardown.");
            }
            finally
            {
                ExpandedSummoningPteranodonViewPatch
                    .PostSuppressionFaultForTest = null;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { control });
            }
        }

        private IEnumerable<int> Sprint24DeathExpiryLifecycle(
            ExpandedSummoningCorrectionFixture fixture, string key)
        {
            ResetExpandedSummoningHostile(fixture);
            UnitEntityData unit = CastExpandedSummoningOwnTier(fixture, key);
            SetExpandedSummoningBrainActive(unit, false);
            var samples = new JArray();
            var resources = new List<UnityEngine.Object>();
            SkinnedMeshRenderer body = null;
            Kingmaker.Visual.Animation.AnimationSet set = null;
            UnitAnimationAction hit = null;
            UnitAnimationAction prone = null;
            bool finite = true, deathPlayed = false, deadObserved = false;
            float maximumDissolve = 0f;
            JObject expiry = null;
            try
            {
                for (int frame = 0; frame < 300; frame++)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                    body = unit.View == null ? null : unit.View
                        .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                        .SingleOrDefault(value => value.name ==
                            TheropodVisualPolicy.BodyRendererName);
                    if (body != null && body.enabled &&
                        body.sharedMesh != null && EntityFadedIn(unit)) break;
                }
                if (body == null || body.sharedMesh == null)
                    throw new InvalidOperationException(
                        "Theropod lifecycle body did not settle: " + key);
                resources.AddRange(ExpandedSummoningPteranodonViewPatch
                    .CaptureTheropodOwnedResources(unit.View));
                set = unit.View.AnimationManager.AnimationSet;
                hit = unit.View.AnimationManager.GetAction(
                    UnitAnimationType.Hit);
                prone = unit.View.AnimationManager.GetAction(
                    UnitAnimationType.Prone);
                bool hasHit = hit != null && hit.Clips != null &&
                    hit.Clips.Any(value => value != null);
                bool hasDeath = prone != null && prone.Clips != null &&
                    prone.Clips.Any(value => value != null);
                Sprint24Check("lifecycle-carriers-" + key,
                    set != null && resources.Count >= 2,
                    new JObject {
                        ["unit"] = unit.UniqueId,
                        ["event"] =
                            "native-animation-and-original-resource-carriers",
                        ["animationSet"] = set == null ? null : set.name,
                        ["nativeHitActionPresent"] = hasHit,
                        ["nativeDeathActionPresent"] = hasDeath,
                        ["hitDisposition"] = hasHit
                            ? "native Hit carrier is retained; no project-authored flinch or synthetic playback"
                            : "native rig has no Hit action; no flinch claimed",
                        ["ownedResources"] = resources.Count,
                        ["actionTypes"] = new JArray(set == null
                            ? Enumerable.Empty<string>()
                            : set.Actions.OfType<UnitAnimationAction>()
                                .Select(value => value.Type.ToString()))
                    }, "The exact native action set remains authoritative; native Hit availability or absence is explicit and no presentation event is invented.");

                Kingmaker.Designers.GameHelper.KillUnit(unit,
                    fixture.Hostile);
                DateTime deadline = DateTime.UtcNow.AddSeconds(8);
                for (int frame = 0; frame < 600 &&
                    DateTime.UtcNow < deadline && unit.View != null; frame++)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                    deadObserved |= unit.Descriptor.State.IsDead;
                    if (unit.View == null) continue;
                    var manager = unit.View.AnimationManager;
                    finite &= ReferenceEquals(manager.AnimationSet, set);
                    var played = new JArray();
                    foreach (UnitAnimationActionHandle handle in manager
                        .ActiveActions.OfType<UnitAnimationActionHandle>())
                    {
                        var active = handle.ActiveAnimation;
                        var clip = active == null ? null :
                            active.GetPlayableClip();
                        bool exact = manager.IsDead &&
                            ReferenceEquals(handle.Action, prone) &&
                            handle.IsStarted && clip != null &&
                            !string.IsNullOrEmpty(clip.name) &&
                            clip.length > 0f &&
                            !double.IsNaN(active.GetTime()) &&
                            !float.IsNaN(active.GetWeight());
                        if (!exact) continue;
                        deathPlayed = true;
                        played.Add(new JObject {
                            ["action"] = handle.Action.name,
                            ["clip"] = clip.name,
                            ["time"] = active.GetTime(),
                            ["duration"] = clip.length,
                            ["weight"] = active.GetWeight()
                        });
                    }
                    if (played.Count > 0 || frame % 30 == 0)
                    {
                        JObject pose = Sprint17OriginalBodySample(unit,
                            body);
                        finite &= (bool)pose["finite"] &&
                            (bool)pose["poseFinite"];
                        if (samples.Count < 12)
                            samples.Add(new JObject {
                                ["phase"] = "death",
                                ["frame"] = Time.frameCount,
                                ["dead"] = unit.Descriptor.State.IsDead,
                                ["managerDead"] = manager.IsDead,
                                ["played"] = played,
                                ["pose"] = pose
                            });
                    }
                    if (deadObserved && (!hasDeath || deathPlayed) &&
                        frame >= 30) break;
                }
                Sprint24Check("native-death-" + key,
                    deadObserved && finite && (!hasDeath || deathPlayed),
                    new JObject {
                        ["unit"] = unit.UniqueId,
                        ["event"] = "native-KillUnit-death-prone",
                        ["deadObserved"] = deadObserved,
                        ["nativeDeathActionPresent"] = hasDeath,
                        ["nativeDeathPlayed"] = deathPlayed,
                        ["finiteOriginalPose"] = finite,
                        ["samples"] = samples.DeepClone()
                    }, "Native death preserves finite original geometry and plays the native death/prone carrier when one exists; no synthetic pose is proof.");

                if (!unit.Destroyed)
                {
                    Buff marker = unit.Descriptor.Buffs.Enumerable.Single(
                        value => ReferenceEquals(value.Blueprint,
                            BlueprintRoot.Instance.SystemMechanics
                                .SummonedUnitBuff));
                    expiry = ExpireSprint16OwnedBuff(unit, marker);
                }
                deadline = DateTime.UtcNow.AddSeconds(15);
                for (int frame = 0; frame < 1200 &&
                    DateTime.UtcNow < deadline; frame++)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                    ObserveSprint17NativeExpiryMaterial(body,
                        ref maximumDissolve);
                    if (unit.Destroyed && unit.View == null &&
                        unit.HoldingState == null &&
                        resources.All(value => value == null)) break;
                }
                bool released = unit.Destroyed && unit.View == null &&
                    unit.HoldingState == null &&
                    resources.All(value => value == null);
                Sprint24Check("native-fade-despawn-" + key,
                    finite && maximumDissolve > .05f && released,
                    new JObject {
                        ["unit"] = unit.UniqueId,
                        ["event"] =
                            "native-summon-expiry-fader-and-teardown",
                        ["maxOriginalDissolve"] = maximumDissolve,
                        ["nativeExpiry"] = expiry,
                        ["destroyed"] = unit.Destroyed,
                        ["viewGone"] = unit.View == null,
                        ["holdingGone"] = unit.HoldingState == null,
                        ["capturedResources"] = resources.Count,
                        ["remainingResources"] = resources.Count(value =>
                            value != null)
                    }, "Native summon expiry visibly dissolves the original body, despawns the logical unit, and reclaims only exact per-instance resources.");
            }
            finally
            {
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { unit });
            }
            yield return 0;
        }

        private IEnumerable<int> Sprint24MovementAndCrowding(
            ExpandedSummoningCorrectionFixture fixture)
        {
            UnitEntityData dein = CastExpandedSummoningOwnTier(fixture,
                TheropodRulesPolicy.DeinonychusKey);
            UnitEntityData tyr = CastExpandedSummoningOwnTier(fixture,
                TheropodRulesPolicy.TyrannosaurusKey);
            SetExpandedSummoningBrainActive(dein, false);
            SetExpandedSummoningBrainActive(tyr, false);
            UnitEntityData[] pair = { dein, tyr };
            UnitEntityData[] awakeBefore = Game.Instance.State.AwakeUnits
                .ToArray();
            bool pauseBefore = Game.Instance.IsPaused;
            try
            {
                for (int frame = 0; frame < 30; frame++) yield return 0;
                JObject deinSelection = Sprint22SelectionSurface(dein);
                JObject tyrSelection = Sprint22SelectionSurface(tyr);
                Pathfinding.NNInfo anchor = AstarPath.active.GetNearest(
                    fixture.Caster.Position);
                if (anchor.node == null || !anchor.node.Walkable)
                    throw new InvalidOperationException(
                        "No walkable native party floor for theropod movement review.");
                Pathfinding.NNInfo origin = default(Pathfinding.NNInfo);
                Pathfinding.NNInfo destination = default(Pathfinding.NNInfo);
                bool routeFound = false;
                int attempts = 0;
                foreach (float distance in new[] { 12f, 10f, 8f, 6f })
                {
                    foreach (Vector3 direction in CompassOffsets)
                    {
                        attempts++;
                        Pathfinding.NNInfo from = AstarPath.active.GetNearest(
                            anchor.clampedPosition - direction *
                                (distance / 2f));
                        Pathfinding.NNInfo to = AstarPath.active.GetNearest(
                            anchor.clampedPosition + direction *
                                (distance / 2f));
                        if (from.node == null || to.node == null ||
                            !from.node.Walkable || !to.node.Walkable ||
                            from.node.Area != anchor.node.Area ||
                            to.node.Area != anchor.node.Area ||
                            Vector3.Distance(from.clampedPosition,
                                to.clampedPosition) < 5f) continue;
                        Vector3 traced = Kingmaker.View.ObstacleAnalyzer
                            .TraceAlongNavmesh(from.clampedPosition,
                                to.clampedPosition);
                        if (Vector3.Distance(traced, to.clampedPosition) > 1f)
                            continue;
                        origin = from;
                        destination = to;
                        routeFound = true;
                        break;
                    }
                    if (routeFound) break;
                }
                if (!routeFound)
                    throw new InvalidOperationException(
                        "No bounded connected theropod movement route.");
                var movementRows = new JArray();
                Game.Instance.IsPaused = false;
                foreach (UnitEntityData unit in pair)
                {
                    PlaceExpandedSummoningUnit(unit, origin.clampedPosition);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                    for (int settle = 0; settle < 3; settle++) yield return 0;
                    Vector3 start = unit.Position;
                    var move = new UnitMoveTo(destination.clampedPosition,
                        .5f);
                    move.Init(unit);
                    bool canStart = move.CanStart;
                    float maximumSpeed = 0f;
                    int maximumContacts = 0;
                    var clock = Stopwatch.StartNew();
                    if (canStart) unit.Commands.Run(move);
                    while (canStart && !move.IsFinished &&
                        !unit.Destroyed && unit.View != null &&
                        unit.View.MovementAgent != null &&
                        clock.Elapsed.TotalSeconds < 20)
                    {
                        maximumSpeed = Math.Max(maximumSpeed,
                            unit.View.MovementAgent.Velocity.magnitude);
                        maximumContacts = Math.Max(maximumContacts,
                            unit.View.MovementAgent.UnitContacts == null ? 0 :
                                unit.View.MovementAgent.UnitContacts.Count);
                        yield return 0;
                    }
                    float travel = Vector3.Distance(start, unit.Position);
                    float gap = Vector3.Distance(destination.clampedPosition,
                        unit.Position);
                    movementRows.Add(new JObject {
                        ["unit"] = unit.UniqueId,
                        ["creature"] = unit.Blueprint.name,
                        ["size"] = unit.Descriptor.State.Size.ToString(),
                        ["speedFeet"] = unit.Stats.Speed.ModifiedValue,
                        ["reachFeet"] = unit.Stats.Reach.ModifiedValue,
                        ["corpulence"] = unit.Corpulence,
                        ["agentCorpulence"] =
                            unit.View?.MovementAgent?.Corpulence,
                        ["origin"] = SurveyVector(start),
                        ["destination"] = SurveyVector(
                            destination.clampedPosition),
                        ["travel"] = travel,
                        ["gap"] = gap,
                        ["maxVelocity"] = maximumSpeed,
                        ["maximumNativeContacts"] = maximumContacts,
                        ["canStart"] = canStart,
                        ["commandFinished"] = move.IsFinished,
                        ["wallSeconds"] = clock.Elapsed.TotalSeconds
                    });
                    unit.Commands.InterruptAll(true);
                }
                bool movement = movementRows.OfType<JObject>().All(value =>
                    (bool)value["canStart"] &&
                    (bool)value["commandFinished"] &&
                    (float)value["travel"] >= 1f &&
                    (float)value["gap"] <= 1.5f &&
                    (float)value["maxVelocity"] > .01f) &&
                    (bool)deinSelection["selectionSurface"] &&
                    (bool)tyrSelection["selectionSurface"] &&
                    dein.Stats.Speed.ModifiedValue == 60 &&
                    tyr.Stats.Speed.ModifiedValue == 40 &&
                    dein.Stats.Reach.ModifiedValue == 5 &&
                    tyr.Stats.Reach.ModifiedValue == 20 &&
                    tyr.Corpulence > dein.Corpulence;
                Sprint24Check("movement-selection-scale", movement,
                    new JObject {
                        ["unit"] = "theropods",
                        ["event"] =
                            "native-shared-route-movement-and-selection",
                        ["routeSearchAttempts"] = attempts,
                        ["deinonychusSelection"] = deinSelection,
                        ["tyrannosaurusSelection"] = tyrSelection,
                        ["members"] = movementRows
                    }, "Medium and Gargantuan theropods retain distinct selection/collision surfaces, measured 60/40-foot speed and 5/20-foot reach while both complete the same native route without skating or invisible blockers.");

                foreach (UnitEntityData unit in pair)
                {
                    unit.Commands.InterruptAll(true);
                    PlaceExpandedSummoningUnit(unit,
                        origin.clampedPosition);
                }
                PlaceExpandedSummoningUnit(tyr,
                    destination.clampedPosition);
                var deinMove = new UnitMoveTo(destination.clampedPosition,
                    .5f);
                var tyrMove = new UnitMoveTo(origin.clampedPosition, .5f);
                deinMove.Init(dein);
                tyrMove.Init(tyr);
                bool pairCanStart = deinMove.CanStart && tyrMove.CanStart;
                if (deinMove.CanStart) dein.Commands.Run(deinMove);
                if (tyrMove.CanStart) tyr.Commands.Run(tyrMove);
                int deinContacts = 0, tyrContacts = 0, frames = 0;
                float initialDistance = Vector3.Distance(dein.Position,
                    tyr.Position);
                float minimumDistance = initialDistance;
                var collisionClock = Stopwatch.StartNew();
                while (pairCanStart &&
                    collisionClock.Elapsed.TotalSeconds < 20)
                {
                    deinContacts = Math.Max(deinContacts,
                        dein.View.MovementAgent.UnitContacts == null ? 0 :
                            dein.View.MovementAgent.UnitContacts.Count);
                    tyrContacts = Math.Max(tyrContacts,
                        tyr.View.MovementAgent.UnitContacts == null ? 0 :
                            tyr.View.MovementAgent.UnitContacts.Count);
                    minimumDistance = Math.Min(minimumDistance,
                        Vector3.Distance(dein.Position, tyr.Position));
                    frames++;
                    if (deinMove.IsFinished && tyrMove.IsFinished) break;
                    yield return 0;
                }
                bool collision = pairCanStart &&
                    (deinContacts > 0 || tyrContacts > 0) &&
                    initialDistance - minimumDistance >= 1f &&
                    pair.All(value =>
                        !value.View.MovementAgent.AvoidanceDisabled &&
                        value.Corpulence > 0 && Math.Abs(value.Corpulence -
                            value.View.MovementAgent.Corpulence) < .001f);
                Sprint24Check("mixed-size-crowding", collision,
                    new JObject {
                        ["unit"] = "theropods",
                        ["event"] =
                            "head-on-native-movement-contacts",
                        ["canStart"] = pairCanStart,
                        ["deinonychusContacts"] = deinContacts,
                        ["tyrannosaurusContacts"] = tyrContacts,
                        ["initialCenterDistance"] = initialDistance,
                        ["minimumCenterDistance"] = minimumDistance,
                        ["finalCenterDistance"] = Vector3.Distance(
                            dein.Position, tyr.Position),
                        ["frames"] = frames,
                        ["wallSeconds"] =
                            collisionClock.Elapsed.TotalSeconds
                    }, "The Medium and Gargantuan bodies approach head-on, produce native contacts, retain avoidance, and never replace logical position or collision with visual offsets.");

                foreach (UnitEntityData unit in pair)
                {
                    unit.Commands.InterruptAll(true);
                    PlaceExpandedSummoningUnit(unit,
                        fixture.Caster.Position + Vector3.right * 4f);
                }
                foreach (int frame in Sprint24ContactPresentation(fixture,
                    dein, "deinonychus")) yield return frame;
                foreach (int frame in Sprint24ContactPresentation(fixture,
                    tyr, "tyrannosaurus")) yield return frame;
            }
            finally
            {
                foreach (UnitEntityData unit in pair)
                {
                    unit.Commands.InterruptAll(true);
                    if (!awakeBefore.Contains(unit))
                        Game.Instance.State.AwakeUnits.Remove(unit);
                }
                Game.Instance.IsPaused = pauseBefore;
                DisposeExpandedSummoningUnits(fixture.Created, pair);
            }
        }

        private IEnumerable<int> Sprint24ContactPresentation(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, string key)
        {
            UnitEntityData target = fixture.Hostile;
            int damageBefore = target.Descriptor.Damage;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            target.Descriptor.State.AddCondition(
                UnitCondition.ImmuneToCombatManeuvers, null);
            SkinnedMeshRenderer body = owner.View
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Single(value => value.name ==
                    TheropodVisualPolicy.BodyRendererName);
            var contacts = new JArray();
            UnitAttack command = null;
            var observer = new Sprint24AttackObserver {
                Owner = owner,
                Target = target,
                BeforeRoll = value =>
                    owner.Stats.AdditionalAttackBonus.BaseValue = 100,
                AfterWeapon = value => {
                    JObject animation = Sprint17IssuedAnimationObservation(
                        command);
                    contacts.Add(new JObject {
                        ["weapon"] = value.Weapon?.Blueprint?.AssetGuid,
                        ["category"] = value.Weapon?.Blueprint?.Category
                            .ToString(),
                        ["charge"] = value.IsCharge,
                        ["opportunity"] = value.IsAttackOfOpportunity,
                        ["ownerPosition"] = SurveyVector(owner.Position),
                        ["targetPosition"] = SurveyVector(target.Position),
                        ["centerDistance"] = owner.DistanceTo(target),
                        ["weaponReachMeters"] = value.Weapon == null ? 0f :
                            value.Weapon.AttackRange.Meters,
                        ["animation"] = animation,
                        ["supportingFrame"] =
                            CaptureSprint17OriginalBodyFrame(owner, body,
                                "sprint24-" + key + "-contact-" +
                                contacts.Count)
                    });
                }
            };
            EventBus.Subscribe(observer);
            try
            {
                PlaceExpandedSummoningUnit(target,
                    owner.Position + Vector3.forward *
                        (key == "tyrannosaurus" ? 5f : 1.5f));
                foreach (int frame in Sprint24RunAttackCommand(fixture,
                    owner, target, true, value => command = value))
                    yield return frame;
                bool exactCount = key == "deinonychus"
                    ? observer.Rolls.Count == 4
                    : observer.Rolls.Count == 1;
                bool presentation = command != null && command.IsStarted &&
                    command.IsFinished && exactCount && contacts.Count ==
                        observer.WeaponAttacks.Count && contacts.Count > 0 &&
                    contacts.OfType<JObject>().All(value =>
                        (float)value["centerDistance"] <=
                            (float)value["weaponReachMeters"] +
                                owner.Corpulence + target.Corpulence + .5f &&
                        (bool?)value["animation"]["handlePresent"] == true &&
                        (string)value["supportingFrame"] != null);
                Sprint24Check("contact-presentation-" + key,
                    presentation, new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] =
                            "issued-native-contact-and-original-body-pose",
                        ["commandStarted"] = command?.IsStarted,
                        ["commandFinished"] = command?.IsFinished,
                        ["rolls"] = observer.Rolls.Count,
                        ["contacts"] = contacts
                    }, key == "deinonychus"
                        ? "The issued native routine exposes four perceptible talon/bite/foreclaw contacts on the attached original body at legal range."
                        : "The issued native bite exposes one perceptible jaw contact on the attached original body at legal twenty-foot reach.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                target.Descriptor.State.RemoveConditionAll(
                    UnitCondition.ImmuneToCombatManeuvers);
                target.Descriptor.Damage = damageBefore;
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                Sprint24ReleaseHold(owner, target);
            }
        }

        private IEnumerable<int> Sprint24SharedControls(
            ExpandedSummoningCorrectionFixture fixture)
        {
            string rakeDetail;
            ResetExpandedSummoningHostile(fixture);
            bool pouncer = ExerciseExpandedSummoningRakeCadence(
                fixture.Blueprints, fixture.Caster, fixture.Hostile,
                fixture.Created, fixture.Evidence, out rakeDetail);
            Sprint24Check("released-pouncer-control", pouncer,
                new JObject {
                    ["unit"] = "leopard",
                    ["event"] = "released-pounce-rake-control",
                    ["detail"] = rakeDetail
                }, "The released Leopard retains its qualified ordinary/charge/held rake cadence while the native Pounce carrier is reused.");

            string grappleDetail;
            ResetExpandedSummoningHostile(fixture);
            bool swallowers = ExerciseExpandedSummoningGrappleLifecycle(
                fixture.Blueprints, fixture.Caster, fixture.Hostile,
                fixture.Created, fixture.Evidence, out grappleDetail);
            Sprint24Check("released-grab-swallow-controls", swallowers,
                new JObject {
                    ["unit"] = "shambling-mound-purple-worm",
                    ["event"] = "released-grapple-swallow-control",
                    ["detail"] = grappleDetail
                }, "The released shared Grab lifecycle and Purple Worm later-turn swallow remain unchanged, including release and native swallowed state.");

            UnitEntityData triceratops = CastExpandedSummoningQuietUnit(
                fixture, ArmoredDinosaurRulesPolicy.TriceratopsKey);
            ItemEntityWeapon gore = Sprint19Limbs(triceratops).Single();
            BlueprintBuff marker = BlueprintRoot.Instance.SystemMechanics
                .ChargeBuff;
            int strengthBefore = triceratops.Stats.Strength.BaseValue;
            var rows = new JArray();
            bool strengthControl = true;
            try
            {
                foreach (int score in new[] { 24, 28 })
                {
                    triceratops.Stats.Strength.BaseValue = score;
                    int modifier = triceratops.Stats.Strength.Bonus;
                    var ordinaryAttack = new RuleAttackWithWeapon(
                        triceratops, fixture.Hostile, gore, 0) {
                            IsFirstAttack = true
                        };
                    var chargeAttack = new RuleAttackWithWeapon(
                        triceratops, fixture.Hostile, gore, 0) {
                            IsCharge = true,
                            IsFirstAttack = true
                        };
                    RuleCalculateWeaponStats ordinary = Rulebook.Trigger(
                        new RuleCalculateWeaponStats(triceratops, gore,
                            ordinaryAttack));
                    triceratops.Descriptor.AddFact(marker);
                    RuleCalculateWeaponStats charged = Rulebook.Trigger(
                        new RuleCalculateWeaponStats(triceratops, gore,
                            chargeAttack));
                    triceratops.Descriptor.RemoveFact(marker);
                    int ordinaryBonus = ordinary.DamageDescription[0].Bonus;
                    int chargedBonus = charged.DamageDescription[0].Bonus;
                    bool exact = ordinary.DamageDescription[0].Dice.Rolls == 2 &&
                        ordinary.DamageDescription[0].Dice.Dice == DiceType.D10 &&
                        ordinaryBonus == modifier + modifier / 2 &&
                        charged.DamageDescription[0].Dice.Rolls == 4 &&
                        charged.DamageDescription[0].Dice.Dice == DiceType.D10 &&
                        chargedBonus == modifier * 2;
                    strengthControl &= exact;
                    rows.Add(new JObject {
                        ["strength"] = score,
                        ["modifier"] = modifier,
                        ["ordinary"] = Sprint23WeaponStatsRow(ordinary,
                            modifier + modifier / 2),
                        ["charged"] = Sprint23WeaponStatsRow(charged,
                            modifier * 2),
                        ["exact"] = exact
                    });
                }
            }
            finally
            {
                if (triceratops.Descriptor.HasFact(marker))
                    triceratops.Descriptor.RemoveFact(marker);
                triceratops.Stats.Strength.BaseValue = strengthBefore;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { triceratops });
            }
            Sprint24Check("triceratops-strength-regression",
                strengthControl, new JObject {
                    ["unit"] = "triceratops",
                    ["event"] = "released-charge-only-strength-control",
                    ["cases"] = rows
                }, "Released Triceratops retains its +7/+9 charge-only dice and Strength arithmetic; Tyrannosaurus Powerful Bite does not change the shared seam.");
            foreach (int frame in Sprint24AiFallback(fixture))
                yield return frame;
        }

        private IEnumerable<int> Sprint24AiFallback(
            ExpandedSummoningCorrectionFixture fixture)
        {
            ResetExpandedSummoningHostile(fixture);
            UnitEntityData owner = CastExpandedSummoningQuietUnit(fixture,
                TheropodRulesPolicy.DeinonychusKey);
            UnitEntityData target = fixture.Hostile;
            int targetHp = target.Stats.HitPoints.BaseValue;
            int targetDamage = target.Descriptor.Damage;
            int attackBefore = owner.Stats.AdditionalAttackBonus.BaseValue;
            bool pauseBefore = Game.Instance.IsPaused;
            var observer = new Sprint24AttackObserver {
                Owner = owner,
                Target = target,
                BeforeRoll = value =>
                    owner.Stats.AdditionalAttackBonus.BaseValue = 100
            };
            var commands = new HashSet<UnitAttack>();
            var join = new UnitCombatJoinController();
            var prepare = new UnitCombatPrepareController();
            EventBus.Subscribe(observer);
            try
            {
                string[] actions = owner.Blueprint.Brain == null ||
                    owner.Blueprint.Brain.Actions == null
                    ? new string[0]
                    : owner.Blueprint.Brain.Actions.Select(value =>
                        value.GetType().FullName + ":" + value.name)
                        .ToArray();
                bool ordinaryFallback = actions.Any(value =>
                    value.StartsWith(
                        "Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAttack:",
                        StringComparison.Ordinal));
                target.Stats.HitPoints.BaseValue = 100000;
                Vector3 origin = fixture.Caster.Position +
                    Vector3.right * 4f;
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target,
                    origin + Vector3.forward * 7f);
                foreach (UnitEntityData unit in new[] { owner, target })
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                owner.Memory.Add(target);
                target.Memory.Add(owner);
                owner.JoinCombat();
                target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                join.Tick();
                prepare.Tick();
                SetExpandedSummoningBrainActive(owner, true);
                owner.Brain.RestoreAvailableActions();
                owner.CombatState.AIData.NextCommandTime = Time.time;
                Game.Instance.IsPaused = false;

                int frames = 0;
                float travel = 0f;
                DateTime deadline = DateTime.UtcNow.AddSeconds(45);
                while (DateTime.UtcNow < deadline && frames++ < 5000 &&
                    observer.WeaponAttacks.Count == 0)
                {
                    foreach (UnitAttack command in owner.Commands.Raw
                        .OfType<UnitAttack>()) commands.Add(command);
                    travel = Math.Max(travel,
                        Vector3.Distance(origin, owner.Position));
                    yield return 0;
                }
                foreach (UnitAttack command in owner.Commands.Raw
                    .OfType<UnitAttack>()) commands.Add(command);
                bool exact = ordinaryFallback && owner.IsBrainActive &&
                    commands.Count > 0 && observer.Rolls.Count > 0 &&
                    observer.WeaponAttacks.Count > 0 && travel >= .25f &&
                    owner.Blueprint.Brain.Actions.Select(value =>
                        value.GetType().FullName + ":" + value.name)
                        .SequenceEqual(actions);
                Sprint24Check("deinonychus-genuine-ai-fallback", exact,
                    new JObject {
                        ["unit"] = owner.UniqueId,
                        ["target"] = target.UniqueId,
                        ["event"] =
                            "unmodified-natural-attack-ai-approach-and-command",
                        ["brain"] = owner.Blueprint.Brain?.name,
                        ["brainActive"] = owner.IsBrainActive,
                        ["brainActions"] = new JArray(actions),
                        ["ordinaryFallback"] = ordinaryFallback,
                        ["nativeCommands"] = commands.Count,
                        ["rolls"] = observer.Rolls.Count,
                        ["weaponAttacks"] = observer.WeaponAttacks.Count,
                        ["travel"] = travel,
                        ["frames"] = frames
                    }, "Without a fixture-issued attack, the unchanged natural-attack AI approaches and executes a real Deinonychus weapon command; manual command evidence is not presented as AI proof.");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                SetExpandedSummoningBrainActive(owner, false);
                owner.Commands.InterruptAll(true);
                target.Commands.InterruptAll(true);
                owner.CombatState.LeaveCombat();
                target.CombatState.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
                owner.Stats.AdditionalAttackBonus.BaseValue = attackBefore;
                target.Stats.HitPoints.BaseValue = targetHp;
                target.Descriptor.Damage = targetDamage;
                Game.Instance.IsPaused = pauseBefore;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner });
                ResetExpandedSummoningHostile(fixture);
            }
        }
    }
}
