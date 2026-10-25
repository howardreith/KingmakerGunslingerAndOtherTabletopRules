using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Fixed order of existing resettable, save-free fixtures. Controller-
        // sensitive negative cases retain their isolated boundary processes.
        private IEnumerable<int> ReviewSprints3031FinalIntegration(ExpandedSummoningCorrectionFixture fixture)
        {
            var inventory = Kingmaker.Game.Instance.Player.Inventory.Items.ToArray();
            var counts = inventory.Select(value => value.Count).ToArray();
            var areas = Kingmaker.Game.Instance.State.AreaEffects.All.ToArray();
            var residue = CastExpandedSummoningOwnTier(fixture, "lemure");
            bool detected = fixture.Created.Any(value => value != null && !value.Destroyed && value.IsInState);
            Sprint25Check("final-integration-live-residue-negative", detected, new JObject {
                ["ownedResidue"] = residue.UniqueId, ["inState"] = residue.IsInState, ["destroyed"] = residue.Destroyed,
                ["resetGuardRejectsLiveOwnedActor"] = detected },
                "The sequence reset predicate detects an intentionally live disposable summon; no cleanup-return value substitutes for this observation.");
            DisposeExpandedSummoningUnits(fixture.Created, new[] { residue });
            for (int frame = 0; frame < 16; frame++) yield return 0;
            if (!detected || !residue.Destroyed || residue.IsInState)
                throw new InvalidOperationException("Integration residue negative/reset failed.");
            var cases = new[] { "shared-circles", "source-lifecycle", "celestial-cleave" };
            foreach (string id in cases)
            {
                IEnumerable<int> run = id == "shared-circles" ? ReviewSprints3031Circles(fixture) :
                    id == "source-lifecycle" ? ReviewSprints3031SourceLifecycle(fixture) : ReviewSprint28Cleave(fixture, true, true);
                foreach (int frame in run) yield return frame;
                var live = fixture.Created.Where(value => value != null && !value.Destroyed && value.IsInState).ToArray();
                bool clean = live.Length == 0 && !Kingmaker.Game.Instance.State.AreaEffects.All.Except(areas).Any() &&
                    Sprint28PartyInventoryExact(inventory, counts) &&
                    !fixture.Hostile.Commands.Raw.Any(value => value != null && !value.IsFinished) &&
                    !TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
                Sprint25Check("final-integration-reset-" + id, clean, new JObject {
                    ["subcase"] = id, ["remainingOwnedUnits"] = new JArray(live.Select(value => value.UniqueId)),
                    ["areasExact"] = !Kingmaker.Game.Instance.State.AreaEffects.All.Except(areas).Any(),
                    ["partyInventoryExact"] = Sprint28PartyInventoryExact(inventory, counts),
                    ["turnBasedActive"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat(),
                    ["targetQueueEmpty"] = !fixture.Hostile.Commands.Raw.Any(value => value != null && !value.IsFinished) },
                    "Each existing native fixture must restore owned units, areas, queues, inventory and RTWP before the next fixed subcase; a failure stops later cases.");
                if (!clean) throw new InvalidOperationException("Final integration isolation failed after " + id);
            }
        }

        private bool IsSprints30And31Review { get { return _request.Scenario ==
            RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprints30And31NativeReview; } }

        private IEnumerable<int> ReviewSprints30And31(ExpandedSummoningCorrectionFixture fixture)
        {
            string phase = RuntimeTestRequestParser.Sprints30And31ReviewPhase(_request);
            if (phase == "final-integration")
            { foreach (int frame in ReviewSprints3031FinalIntegration(fixture)) yield return frame; yield break; }
            if (phase == "celestial-cleave")
            { foreach (int frame in ReviewSprint28Cleave(fixture, true)) yield return frame; yield break; }
            if (new[] { "boundary-lemure", "boundary-dretch", "boundary-hound-archon", "boundary-lantern-archon", "boundary-astral-deva", "boundary-trumpet-archon" }.Contains(phase))
            { foreach (int frame in ReviewSprints3031ActionBoundary(fixture, phase.Substring("boundary-".Length))) yield return frame; yield break; }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31PublicRoutesPhase)
            { foreach (int frame in ReviewSprints30And31PublicRoutes(fixture)) yield return frame; yield break; }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31DefenseConsumersPhase)
            {
                foreach (int frame in ReviewSprints3031Defenses(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31SourceLifecyclePhase)
            {
                foreach (int frame in ReviewSprints3031SourceLifecycle(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31OutsiderBodyPhase)
            {
                foreach (int frame in ReviewOutsider30Bodies(fixture)) yield return frame;
                yield break;
            }
            if (phase == "lemure-core" || phase == "lemure-full" || phase == "lemure-ai")
            {
                foreach (int frame in ReviewLemure30Core(fixture, phase)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31DretchMeleePhase ||
                phase == RuntimeTestScenarioCatalog.Sprints30And31DretchMeleeAiPhase)
            {
                foreach (int frame in ReviewDretch30Melee(fixture,
                    phase == RuntimeTestScenarioCatalog.Sprints30And31DretchMeleeAiPhase)) yield return frame;
                yield break;
            }
            if (phase == "dretch-cloud")
            {
                foreach (int frame in ReviewDretch30Cloud(fixture)) yield return frame;
                yield break;
            }
            if (phase == "dretch-ai" || phase == "dretch-ai-ally")
            {
                foreach (int frame in ReviewDretch30Ai(fixture, phase == "dretch-ai-ally")) yield return frame;
                yield break;
            }
            if (phase == "archon-core" || phase == "archon-hound")
            {
                foreach (int frame in ReviewArchon30Core(fixture, phase == "archon-hound")) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31ArchonBoundariesPhase)
            {
                foreach (int frame in ReviewArchon30Core(fixture, false, true)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31SharedCirclesPhase)
            {
                foreach (int frame in ReviewSprints3031Circles(fixture)) yield return frame;
                yield break;
            }
            if (phase == "hound-armed" || phase == "hound-natural" ||
                phase == RuntimeTestScenarioCatalog.Sprints30And31HoundDisarmPhase ||
                phase == RuntimeTestScenarioCatalog.Sprints30And31HoundSinglePhase ||
                phase == RuntimeTestScenarioCatalog.Sprints30And31HoundAiPhase ||
                phase == RuntimeTestScenarioCatalog.Sprints30And31HoundTurnPhase)
            {
                foreach (int frame in ReviewHound30Combat(fixture, phase == "hound-natural" || phase == RuntimeTestScenarioCatalog.Sprints30And31HoundDisarmPhase,
                    phase == RuntimeTestScenarioCatalog.Sprints30And31HoundSinglePhase || phase == RuntimeTestScenarioCatalog.Sprints30And31HoundTurnPhase,
                    phase == RuntimeTestScenarioCatalog.Sprints30And31HoundAiPhase,
                    phase == RuntimeTestScenarioCatalog.Sprints30And31HoundTurnPhase,
                    phase == RuntimeTestScenarioCatalog.Sprints30And31HoundDisarmPhase)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31LanternRayPhase)
            {
                foreach (int frame in ReviewLantern31Rays(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31LanternBodyPhase)
            {
                foreach (int frame in ReviewLantern31Body(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31AstralCorePhase)
            {
                foreach (int frame in ReviewAstral31Core(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31AstralHammerPhase)
            {
                foreach (int frame in ReviewAstral31Hammer(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31AstralBladePhase)
            {
                foreach (int frame in ReviewAstral31Blade(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31AstralSupportPhase)
            {
                foreach (int frame in ReviewAstral31Support(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31AstralAtWillPhase)
            {
                foreach (int frame in ReviewAstral31AtWill(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31AstralSmitePhase)
            {
                foreach (int frame in ReviewAstral31Smite(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31AstralDispelPhase)
            {
                foreach (int frame in ReviewAstral31Dispel(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31TrumpetDispelPhase)
            {
                foreach (int frame in ReviewTrumpet31Dispel(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31TrumpetBlastAiSafePhase)
            {
                foreach (int frame in ReviewTrumpet31BlastAiSafe(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31TrumpetBlastAiPhase)
            {
                foreach (int frame in ReviewTrumpet31BlastAi(fixture)) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31TrumpetBlastPhase)
            {
                foreach (int frame in ReviewTrumpet31Blast(fixture)) yield return frame;
                yield break;
            }
            if (phase == "trumpet-caster")
            {
                foreach (int frame in ReviewTrumpet31Caster(fixture)) yield return frame;
                yield break;
            }
            if (phase == "trumpet-book-turn")
            {
                foreach (int frame in ReviewTrumpet31BookTurn(fixture)) yield return frame;
                yield break;
            }
            if (phase == "trumpet-book")
            {
                foreach (int frame in ReviewTrumpet31Book(fixture)) yield return frame;
                yield break;
            }
            if (phase != "bootstrap") throw new InvalidOperationException("Unimplemented S30/S31 phase.");
            var inputs = RunExpandedSummoningSprints30And31Inputs();
            Sprint25Check("bootstrap-input-capture", inputs.Status == RuntimeTestStatuses.Pass,
                new JObject { ["captureStatus"] = inputs.Status, ["researchOnly"] = true },
                "The bounded installed-consumer capture completes; this is research, not mechanics proof.");
            int publicReferences;
            bool publicExact = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(
                BlueprintBootstrap.Library, true, out publicReferences);
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == "lemure").ToArray();
            var parents = ExpandedSummoningInventoryObserver.CanonicalParentGuids.Select(guid =>
                BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    guid, "live parent in hidden bootstrap")).ToArray();
            int choices = parents.Sum(parent => parent.ComponentsArray.OfType<AbilityVariants>()
                .Single().Variants.Length);
            Sprint25Check("bootstrap-publication", publicExact && routes.Length == 8 &&
                routes.All(value => !SummonVisibilityCatalog.IsPublished(value)) &&
                publicReferences == SummonVisibilityCatalog.PublishedLogicalPlacementCount &&
                choices == publicReferences + ExpandedSummoningIdentityCatalog.NativeExpandedOptionIdentityCount,
                new JObject { ["publicReferences"] = publicReferences, ["choices"] = choices,
                    ["hiddenLemureRoutes"] = routes.Length, ["publicExact"] = publicExact },
                "Every released generated root remains exactly once; eight Lemure roots stay hidden and retained wrappers remain.");
            var owner = CastExpandedSummoningOwnTier(fixture, "lemure");
            SetExpandedSummoningBrainActive(owner, false);
            for (int frame = 0; frame < 12; frame++) yield return 0;
            var stats = owner.Descriptor.Stats;
            var blueprint = owner.Blueprint;
            var hand = owner.Body.PrimaryHand.MaybeWeapon;
            int additional = owner.Body.AdditionalLimbs.Count();
            bool profile = stats.Strength.BaseValue == 11 && stats.Dexterity.BaseValue == 10 &&
                stats.Constitution.BaseValue == 12 && stats.Intelligence.BaseValue == 0 &&
                stats.Wisdom.BaseValue == 11 && stats.Charisma.BaseValue == 5 &&
                stats.HitPoints.ModifiedValue == 13 && stats.BaseAttackBonus.ModifiedValue == 2 &&
                owner.Descriptor.State.IsConscious && !owner.Descriptor.State.IsDead &&
                owner.Body.HandsAreEnabled &&
                hand != null && hand.Blueprint.AssetGuid == Lemure30Claw &&
                additional == 1 && blueprint.Body.AdditionalSecondaryLimbs.Length == 0;
            Sprint25Check("bootstrap-lemure-instantiation", profile,
                new JObject { ["unit"] = blueprint.AssetGuid, ["owner"] = owner.UniqueId,
                    ["scores"] = new JArray(stats.Strength.BaseValue, stats.Dexterity.BaseValue,
                        stats.Constitution.BaseValue, stats.Intelligence.BaseValue,
                        stats.Wisdom.BaseValue, stats.Charisma.BaseValue),
                    ["hitPoints"] = stats.HitPoints.ModifiedValue,
                    ["bab"] = stats.BaseAttackBonus.ModifiedValue,
                    ["conscious"] = owner.Descriptor.State.IsConscious,
                    ["primaryWeapon"] = hand == null ? null : hand.Blueprint.AssetGuid,
                    ["handsEnabled"] = owner.Body.HandsAreEnabled,
                    ["configuredDisableHands"] = blueprint.Body.DisableHands,
                    ["emptyHandWeapon"] = owner.Body.EmptyHandWeapon == null ? null :
                        owner.Body.EmptyHandWeapon.Blueprint.AssetGuid,
                    ["additionalLimbs"] = additional,
                    ["visualStatus"] = "borrowed biped proxy; original Lemure body and presentation not qualified" },
                "Actual hidden Lemure is conscious at zero Intelligence, has its own adult bases and two claw slots. Combat, defenses, AI and body remain separate required proof.");
            if (!profile) throw new InvalidOperationException("First hidden Lemure instantiation failed; stop dependent expansion.");
            // Native plan only: this is not a hit, resolved attack or AI claim.
            var attack = new UnitAttack(owner);
            attack.Init(owner);
            var plan = attack.CreateFullAttack();
            bool claws = plan.Count == 2 && plan.All(value =>
                value != null && value.Hand != null &&
                value.Hand.MaybeWeapon != null &&
                value.Hand.MaybeWeapon.Blueprint.AssetGuid ==
                    Lemure30Claw &&
                value.AttackBonusPenalty == 0);
            Sprint25Check("bootstrap-native-claw-plan", claws,
                new JObject { ["owner"] = owner.UniqueId, ["planOnly"] = true,
                    ["plan"] = new JArray(plan.Select(value => new JObject {
                        ["weapon"] = value?.Hand?.MaybeWeapon?.Blueprint?.AssetGuid,
                        ["penalty"] = value?.AttackBonusPenalty,
                        ["number"] = value?.AttackNumber })) },
                "Native UnitAttack.CreateFullAttack produces exactly two primary claw entries; no attack is issued and no hit or AI verdict is claimed.");
            if (!claws) throw new InvalidOperationException(
                "Native first-slice claw plan failed; stop dependent expansion.");
        }
        private IEnumerable<int> ReviewSprints30And31PublicRoutes(ExpandedSummoningCorrectionFixture fixture)
        {
            var tiers = new Dictionary<string, int>(StringComparer.Ordinal) {
                { "lemure", 2 }, { "dretch", 3 }, { "hound-archon", 4 },
                { "lantern-archon", 3 }, { "astral-deva", 9 }, { "trumpet-archon", 9 } };
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(value => tiers.ContainsKey(value.Creature.Key))
                .OrderBy(value => value.Creature.Key, StringComparer.Ordinal)
                .ThenBy(value => value.ParentTier).ToArray();
            int expected = tiers.Sum(value => 10 - value.Value);
            bool exact = routes.Length == expected && tiers.All(pair => routes
                    .Where(value => value.Creature.Key == pair.Key).Select(value => value.ParentTier)
                    .SequenceEqual(Enumerable.Range(pair.Value, 10 - pair.Value))) &&
                !ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                    .Any(value => tiers.ContainsKey(value.Creature.Key)) &&
                routes.All(value => !value.Creature.MonsterTemplated && SummonVisibilityCatalog.IsPublished(value) &&
                    value.Multiplicity == (value.ParentTier == tiers[value.Creature.Key] ? SummonMultiplicity.One :
                        value.ParentTier == tiers[value.Creature.Key] + 1 ? SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne));
            Sprint25Check("public-affected-route-census", exact, new JObject {
                ["routes"] = new JArray(routes.Select(value => value.StableKey)),
                ["derivedExpected"] = expected,
                ["newRoutes"] = routes.Count(value => value.Creature.Key != "lantern-archon"),
                ["preservedChangedLanternRoutes"] = routes.Count(value => value.Creature.Key == "lantern-archon"),
                ["published"] = routes.Count(SummonVisibilityCatalog.IsPublished),
                ["registeredRoots"] = SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                ["publishedRoots"] = SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                ["suppressedRoots"] = SummonVisibilityCatalog.SuppressedLogicalPlacementCount },
                "Exactly the six chartered untemplated Monster route families are public at their actual one/1d3/1d4+1 tiers; no Nature route is added. Released identity/order comparisons retain their separate bindings.");
            if (!exact) throw new InvalidOperationException("Combined public route preconditions are incomplete.");
            // The final S29 fixture already supports untemplated Monster and
            // Nature parents. It observes each actual selected parent/variant,
            // native Wizard20 slot, command/cast/spawn, quantity, CL20 duration
            // and exact actor cleanup. No new route/caster framework is needed.
            Sprint24PlayerRoutes(fixture, routes, Sprint25Check, false);
            int published;
            bool inventoryExact = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(
                BlueprintBootstrap.Library, true, out published);
            Sprint25Check("public-inventory-integration", inventoryExact,
                new JObject { ["published"] = published, ["affectedRoutes"] = routes.Length },
                "The combined artifact's native parent/variant inventory matches its actual published catalog.");
            if (!inventoryExact) throw new InvalidOperationException("Public inventory differs.");
            foreach (string key in tiers.Keys.OrderBy(value => value, StringComparer.Ordinal)) {
                var areasBeforeBody = Kingmaker.Game.Instance.State.AreaEffects.All.ToArray();
                var actor = CastExpandedSummoningOwnTier(fixture, key);
                var privateResources = Array.Empty<UnityEngine.Object>();
                bool pause = Kingmaker.Game.Instance.IsPaused;
                bool waiting = _sprint25WaitingForUnityAppearance;
                try {
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                    var deadline = DateTime.UtcNow.AddSeconds(12);
                    while (DateTime.UtcNow < deadline && (actor.View == null || !EntityFadedIn(actor) ||
                        DissolveAmount(actor) > MotionReviewIntactDissolve || !actor.Descriptor.State.CanAct)) {
                        _sprint25WaitingForUnityAppearance = EntityFadedIn(actor);
                        Kingmaker.Game.Instance.IsPaused = false; yield return 0;
                    }
                    if (actor.View == null) throw new InvalidOperationException("Public body view absent: " + key);
                    privateResources = Sprints3031PrivateResources(actor, key);
                    var renderers = actor.View.GetComponentsInChildren<UnityEngine.Renderer>(true);
                    bool visible = renderers.Any(value => value.enabled && value.gameObject.activeInHierarchy &&
                        Astral31Finite(value.bounds.center) && Astral31Finite(value.bounds.size) && value.bounds.size.sqrMagnitude > .01f);
                    if (key == "lantern-archon") {
                        var orb = actor.View.GetComponent<LanternArchonOrbView>();
                        visible = orb != null && orb.OrbRenderer != null && orb.OrbRenderer.enabled &&
                            orb.OrbRenderer.gameObject.activeInHierarchy;
                    }
                    string renderer = key == "lemure" || key == "dretch" || key == "hound-archon" ?
                        TheropodVisualPolicy.BodyRendererName : null;
                    string frame = WriteExpandedSummoningPartyCameraCapture(actor, _request.EvidenceDirectory,
                        "s3031-public-" + key + ".png", renderer);
                    var selection = Sprint22SelectionSurface(actor);
                    var own = routes.Single(value => value.Creature.Key == key && value.ParentTier == tiers[key]);
                    var ability = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                        ExpandedSummoningIdentityCatalog.AbilitySymbol(own)];
                    bool icon = ability.Icon != null && ability.Icon.texture != null &&
                        ability.Icon.texture.width == 128 && ability.Icon.texture.height == 128;
                    bool bodyExact = visible && EntityFadedIn(actor) && DissolveAmount(actor) <= MotionReviewIntactDissolve &&
                        (bool)selection["selectionSurface"] && frame.Contains("inFrame=true") && frame.Contains("screenLit=true") && icon &&
                        (key == "lantern-archon" || frame.Contains("rendererEnabled=true"));
                    Sprint25Check("public-body-icon-" + key, bodyExact, new JObject {
                        ["creature"] = key, ["owner"] = actor.UniqueId, ["route"] = ability.AssetGuid,
                        ["visible"] = visible, ["selection"] = selection, ["partyCamera"] = frame,
                        ["iconWidth"] = ability.Icon?.texture?.width, ["iconHeight"] = ability.Icon?.texture?.height,
                        ["privateResources"] = privateResources.Length },
                        "The final public artifact resolves the real icon and a visible selectable owned body at native party-camera scale; rough anatomy and texture polish remain accepted backlog.");
                    if (!bodyExact) throw new InvalidOperationException("Public body/icon integration failed: " + key);
                } finally {
                    _sprint25WaitingForUnityAppearance = waiting;
                    actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                    if (actor.IsInCombat) actor.LeaveCombat();
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { actor });
                    Kingmaker.Game.Instance.IsPaused = pause;
                }
                for (int frame = 0; frame < 16; frame++) yield return 0;
                bool gone = actor.Destroyed && !actor.IsInState && privateResources.All(value => value == null) &&
                    !Kingmaker.Game.Instance.State.AreaEffects.All.Except(areasBeforeBody).Any();
                Sprint25Check("public-body-reset-" + key, gone,
                    new JObject { ["ownerGone"] = actor.Destroyed && !actor.IsInState,
                        ["privateResourcesGone"] = privateResources.All(value => value == null) },
                    "The exact disposable body and its private resources are absent before the next presentation case.");
                if (!gone) throw new InvalidOperationException("Public body reset failed: " + key);
            }

        }

    }
}
