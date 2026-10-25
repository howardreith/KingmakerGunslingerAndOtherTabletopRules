using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewSprint32PublicRoutes(ExpandedSummoningCorrectionFixture fixture, bool satyr)
        {
            string key = satyr ? "satyr" : "lillend-azata";
            var family = satyr ? SummonFamily.NaturesAlly : SummonFamily.Monster;
            int tier = satyr ? 4 : 6;
            var routes = ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => value.Creature.Key == key).OrderBy(value => value.ParentTier).ToArray();
            bool census = routes.Select(value => value.ParentTier).SequenceEqual(Enumerable.Range(tier, 10 - tier)) &&
                routes.All(value => SummonVisibilityCatalog.IsPublished(value) && !value.Creature.MonsterTemplated &&
                    value.Multiplicity == (value.ParentTier == tier ? SummonMultiplicity.One :
                        value.ParentTier == tier + 1 ? SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne)) &&
                !ExpandedSummoningCatalog.GenerateVariants(satyr ? SummonFamily.Monster : SummonFamily.NaturesAlly)
                    .Any(value => value.Creature.Key == key);
            Sprint25Check("sprint32-public-census-" + key, census, new JObject {
                ["routes"] = new JArray(routes.Select(value => new JObject {
                    ["key"] = value.StableKey, ["family"] = value.Family.ToString(),
                    ["tier"] = value.ParentTier, ["quantity"] = value.Multiplicity.ToString() })) },
                "Exactly the legal published family, tiers and propagated quantities; no animal template children.");
            if (!census) throw new InvalidOperationException("Sprint32 public route census failed: " + key);
            // One fresh process/caster per family: never level the same fixture
            // as both Wizard20 and Druid20 or seed SNA into a Wizard book.
            Sprint24PlayerRoutes(fixture, routes, Sprint25Check, false, satyr);
            int published;
            bool inventory = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(BlueprintBootstrap.Library, true, out published);
            Sprint25Check("sprint32-public-inventory-" + key, inventory,
                new JObject { ["published"] = published, ["affectedRoutes"] = routes.Length },
                "The live public inventory matches this candidate, including the preserved existing routes.");
            if (!inventory) throw new InvalidOperationException("Sprint32 public inventory differs.");

            var areas = Game.Instance.State.AreaEffects.All.ToArray();
            var owner = CastExpandedSummoningOwnTier(fixture, key);
            var owned = new List<UnityEngine.Object>();
            var borrowed = new List<UnityEngine.Object>();
            bool oldWaiting = _sprint25WaitingForUnityAppearance;
            try
            {
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                var deadline = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < deadline && (owner.View == null || !EntityFadedIn(owner) ||
                    DissolveAmount(owner) > MotionReviewIntactDissolve || !owner.Descriptor.State.CanAct))
                {
                    _sprint25WaitingForUnityAppearance = EntityFadedIn(owner);
                    Game.Instance.IsPaused = false; yield return 0;
                }
                if (owner.View == null) throw new InvalidOperationException("Sprint32 body view absent.");
                var attachment = owner.View.GetComponent<LillendHumanVisualAttachment>();
                if (!satyr && attachment != null)
                {
                    owned.AddRange(attachment.CaptureOwnedResources()); owned.Add(attachment);
                    borrowed.AddRange(attachment.BorrowedResources());
                }
                bool body = satyr || (attachment != null && attachment.Live && attachment.NativeActionsUnchanged &&
                    attachment.AuxiliaryGeometrySuppressed && (bool)Lillend32BodySample(owner, attachment)["finite"]);
                var renderers = owner.View.GetComponentsInChildren<Renderer>(true);
                bool visible = renderers.Any(value => value.enabled && value.gameObject.activeInHierarchy &&
                    Astral31Finite(value.bounds.center) && Astral31Finite(value.bounds.size) && value.bounds.size.sqrMagnitude > .01f);
                var selection = Sprint22SelectionSurface(owner);
                string frame = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                    "s32-public-" + key + ".png", satyr ? null : attachment?.Body?.name);
                var ability = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningIdentityCatalog.AbilitySymbol(routes[0])];
                bool icon = ability.Icon != null && ability.Icon.texture != null &&
                    ability.Icon.texture.width == 128 && ability.Icon.texture.height == 128;
                bool exact = body && visible && icon && EntityFadedIn(owner) && DissolveAmount(owner) <= MotionReviewIntactDissolve &&
                    (bool)selection["selectionSurface"] && frame.Contains("inFrame=true") && frame.Contains("screenLit=true") &&
                    frame.Contains("rendererEnabled=true");
                Sprint25Check("sprint32-public-body-icon-" + key, exact, new JObject {
                    ["owner"] = owner.UniqueId, ["route"] = ability.AssetGuid, ["body"] = body,
                    ["selection"] = selection, ["partyCamera"] = frame, ["visible"] = visible,
                    ["iconWidth"] = ability.Icon?.texture?.width, ["iconHeight"] = ability.Icon?.texture?.height,
                    ["ownedResources"] = owned.Count, ["borrowedResources"] = borrowed.Count },
                    "Actual published blueprint body/icon and native selection at gameplay camera. Motion/contact/lifecycle and personal aesthetic approval are separate.");
                if (!exact) throw new InvalidOperationException("Sprint32 public body/icon failed: " + key);
            }
            finally
            {
                _sprint25WaitingForUnityAppearance = oldWaiting;
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                if (owner.IsInCombat) owner.LeaveCombat();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
            }
            for (int frame = 0; frame < 16; frame++) yield return 0;
            bool gone = owner.Destroyed && !owner.IsInState && owned.All(value => value == null) &&
                borrowed.All(value => value != null) && !Game.Instance.State.AreaEffects.All.Except(areas).Any() &&
                (satyr || (owned.Count > 0 && borrowed.Count > 0));
            Sprint25Check("sprint32-public-body-reset-" + key, gone, new JObject {
                ["ownerGone"] = owner.Destroyed && !owner.IsInState, ["ownedRemaining"] = owned.Count(value => value != null),
                ["borrowedMissing"] = borrowed.Count(value => value == null) },
                "Owned body resources are gone and borrowed native resources remain after fixture disposal; natural lifecycle remains separate.");
            if (!gone) throw new InvalidOperationException("Sprint32 public body reset failed: " + key);
        }
    }
}
