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
        private bool IsSprints30And31Review { get { return _request.Scenario ==
            RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprints30And31NativeReview; } }

        private IEnumerable<int> ReviewSprints30And31(ExpandedSummoningCorrectionFixture fixture)
        {
            string phase = RuntimeTestRequestParser.Sprints30And31ReviewPhase(_request);
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
            if (phase == "hound-armed" || phase == "hound-natural")
            {
                foreach (int frame in ReviewHound30Combat(fixture, phase == "hound-natural")) yield return frame;
                yield break;
            }
            if (phase == RuntimeTestScenarioCatalog.Sprints30And31LanternRayPhase)
            {
                foreach (int frame in ReviewLantern31Rays(fixture)) yield return frame;
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
    }
}
