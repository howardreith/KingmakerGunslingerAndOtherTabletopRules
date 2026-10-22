using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using Pathfinding;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewLantern31Body(
            ExpandedSummoningCorrectionFixture fixture)
        {
            object[] unitsBefore = Game.Instance.State.Units.All.Cast<object>().ToArray();
            object[] areasBefore = Game.Instance.State.AreaEffects.All.Cast<object>().ToArray();
            var captured = new List<UnityEngine.Object>();
            UnitEntityData owner = null, sibling = null;
            int beforeComponents, beforeRoots, beforeOrbs, beforeMaterials, beforeLights;
            string before = LanternArchonOrbView.CountOwnedObjects(out beforeComponents,
                out beforeRoots, out beforeOrbs, out beforeMaterials, out beforeLights);
            bool initiallyClean = beforeComponents == 0 && beforeRoots == 0 &&
                beforeOrbs == 0 && beforeMaterials == 0 && beforeLights == 0;
            Sprint25Check("lantern-body-preconditions", initiallyClean,
                new JObject { ["ownedObjects"] = before,
                    ["worldUnits"] = unitsBefore.Length,
                    ["areas"] = areasBefore.Length },
                "The fixed body case starts without a prior project-owned Lantern orb, light or material.");
            if (!initiallyClean) throw new InvalidOperationException(
                "Lantern body preconditions contain owned residue.");
            try
            {
                owner = CastExpandedSummoningOwnTier(fixture, "lantern-archon");
                sibling = CastExpandedSummoningOwnTier(fixture, "lantern-archon");
                foreach (UnitEntityData unit in new[] { owner, sibling })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    if (AstarPath.active == null)
                        throw new InvalidOperationException(
                            "Lantern body review has no native placement graph.");
                    var ground = AstarPath.active.GetNearest(unit.Position);
                    if (ground.node == null || !ground.node.Walkable)
                        throw new InvalidOperationException(
                            "Lantern body review has no walkable native placement.");
                    PlaceExpandedSummoningUnit(unit, ground.clampedPosition);
                }

                LanternArchonOrbView ownerOrb = null, siblingOrb = null;
                bool appearanceBuffAbsent = false, faded = false;
                int frames = 0;
                DateTime deadline = DateTime.UtcNow.AddSeconds(15);
                while (frames++ < 1800 && DateTime.UtcNow < deadline)
                {
                    ownerOrb = owner.View == null ? null :
                        owner.View.GetComponent<LanternArchonOrbView>();
                    siblingOrb = sibling.View == null ? null :
                        sibling.View.GetComponent<LanternArchonOrbView>();
                    appearanceBuffAbsent = owner.Descriptor.Buffs.GetBuff(
                        BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null &&
                        sibling.Descriptor.Buffs.GetBuff(
                            BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null;
                    faded = EntityFadedIn(owner) && EntityFadedIn(sibling);
                    if (ownerOrb != null && siblingOrb != null && ownerOrb.Ready &&
                        siblingOrb.Ready && appearanceBuffAbsent && faded &&
                        owner.Descriptor.State.CanAct && sibling.Descriptor.State.CanAct)
                        break;
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                bool ready = ownerOrb != null && siblingOrb != null &&
                    ownerOrb.Ready && siblingOrb.Ready && appearanceBuffAbsent && faded;
                Sprint25Check("lantern-body-native-ready", ready,
                    new JObject { ["frames"] = frames,
                        ["owner"] = ownerOrb == null ? "<none>" : ownerOrb.Describe(),
                        ["sibling"] = siblingOrb == null ? "<none>" : siblingOrb.Describe(),
                        ["appearanceBuffAbsent"] = appearanceBuffAbsent,
                        ["fadedIn"] = faded },
                    "Both exact Lantern views reach their ordinary faded-in native state with independently owned visible orbs.");
                if (!ready) throw new InvalidOperationException(
                    "Lantern owned body did not reach a visible native state.");

                UnityEngine.Object[] ownerResources = ownerOrb.CaptureOwnedResources();
                UnityEngine.Object[] siblingResources = siblingOrb.CaptureOwnedResources();
                captured.AddRange(ownerResources);
                captured.AddRange(siblingResources);
                bool separate = ownerResources.Length == 6 && siblingResources.Length == 6 &&
                    !ownerResources.Any(left => siblingResources.Any(right =>
                        ReferenceEquals(left, right)));
                Renderer[] ownerNative = owner.View.GetComponentsInChildren<Renderer>(true)
                    .Where(value => value != null &&
                        !ReferenceEquals(value, ownerOrb.OrbRenderer)).ToArray();
                Renderer[] siblingNative = sibling.View.GetComponentsInChildren<Renderer>(true)
                    .Where(value => value != null &&
                        !ReferenceEquals(value, siblingOrb.OrbRenderer)).ToArray();
                bool donorIsolated = ownerNative.Length == ownerOrb.NativeRendererCount &&
                    siblingNative.Length == siblingOrb.NativeRendererCount &&
                    ownerNative.Concat(siblingNative).All(value =>
                        !value.sharedMaterials.Any(donorMaterial =>
                            ReferenceEquals(donorMaterial, ownerOrb.OrbMaterial) ||
                            ReferenceEquals(donorMaterial, siblingOrb.OrbMaterial)));
                Material material = ownerOrb.OrbMaterial;
                Color emission = material != null && material.HasProperty("_EmissionColor")
                    ? material.GetColor("_EmissionColor") : Color.black;
                MeshRenderer renderer = ownerOrb.OrbRenderer;
                float diameter = renderer == null ? 0f : Mathf.Max(renderer.bounds.size.x,
                    Mathf.Max(renderer.bounds.size.y, renderer.bounds.size.z));
                Light light = ownerOrb.OrbLight;
                var selection = Sprint22SelectionSurface(owner);
                string party = WriteExpandedSummoningPartyCameraCapture(owner,
                    _request.EvidenceDirectory, "s31-lantern-owned-orb-party.png");
                int components, roots, orbs, materials, lights;
                string liveCounts = LanternArchonOrbView.CountOwnedObjects(out components,
                    out roots, out orbs, out materials, out lights);
                bool exact = separate && donorIsolated &&
                    owner.Blueprint.AssetGuid == LanternArchonOrbView.UnitGuid &&
                    sibling.Blueprint.AssetGuid == LanternArchonOrbView.UnitGuid &&
                    ownerOrb.Root.transform.parent == owner.View.transform &&
                    siblingOrb.Root.transform.parent == sibling.View.transform &&
                    ownerOrb.Orb.name == LanternArchonOrbView.OrbName &&
                    siblingOrb.Orb.name == LanternArchonOrbView.OrbName &&
                    renderer != null && renderer.enabled &&
                    ReferenceEquals(renderer.sharedMaterial, material) &&
                    material != null && material.name == LanternArchonOrbView.MaterialName &&
                    emission.maxColorComponent > 1f && diameter >= .5f && diameter <= 1.3f &&
                    light != null && light.enabled && light.type == LightType.Point &&
                    light.shadows == LightShadows.None && light.range == 2.4f &&
                    Math.Abs(light.intensity - 1.55f) < .001f &&
                    components == 2 && roots == 2 && orbs == 2 && materials == 2 && lights == 2 &&
                    (bool)selection["selectionSurface"] &&
                    party.Contains("png=s31-lantern-owned-orb-party.png") &&
                    party.Contains("inFrame=true") && party.Contains("screenLit=true") &&
                    party.Contains("faderVisible=true") &&
                    !owner.Descriptor.Inventory.Items.Any() &&
                    !sibling.Descriptor.Inventory.Items.Any();
                Sprint25Check("lantern-body-owned-presentation", exact,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["sibling"] = sibling.UniqueId,
                        ["ownerView"] = ownerOrb.Describe(),
                        ["siblingView"] = siblingOrb.Describe(),
                        ["ownerResourceCount"] = ownerResources.Length,
                        ["siblingResourceCount"] = siblingResources.Length,
                        ["privateResourcesSeparate"] = separate,
                        ["donorRenderersIsolated"] = donorIsolated,
                        ["ownerNativeRenderers"] = ownerNative.Length,
                        ["siblingNativeRenderers"] = siblingNative.Length,
                        ["emission"] = new JArray(emission.r, emission.g, emission.b),
                        ["worldDiameter"] = diameter,
                        ["lightRange"] = light == null ? 0f : light.range,
                        ["lightIntensity"] = light == null ? 0f : light.intensity,
                        ["liveOwnedObjects"] = liveCounts,
                        ["selection"] = selection, ["partyCamera"] = party,
                        ["inventoryCounts"] = new JArray(
                            owner.Descriptor.Inventory.Items.Count(),
                            sibling.Descriptor.Inventory.Items.Count()),
                        ["scope"] = "owned visual body/light only; no illumination mechanics or aesthetic approval" },
                    "Two exact Lanterns have distinct project-owned emissive orb/material/light resources at usable camera scale while every native donor renderer and inventory remains isolated.");
                if (!exact) throw new InvalidOperationException(
                    "Lantern owned body presentation failed.");
            }
            finally
            {
                foreach (UnitEntityData unit in new[] { owner, sibling }
                    .Where(value => value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    if (unit.IsInCombat) unit.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling }.Where(value =>
                        value != null && !value.Destroyed).ToArray());
                Game.Instance.Player.UpdateIsInCombat();
            }
            for (int frame = 0; frame < 12; frame++) yield return 0;
            int afterComponents, afterRoots, afterOrbs, afterMaterials, afterLights;
            string after = LanternArchonOrbView.CountOwnedObjects(out afterComponents,
                out afterRoots, out afterOrbs, out afterMaterials, out afterLights);
            bool clean = captured.Count == 12 && captured.All(value => value == null) &&
                afterComponents == 0 && afterRoots == 0 && afterOrbs == 0 &&
                afterMaterials == 0 && afterLights == 0 &&
                !Game.Instance.State.Units.All.Cast<object>().Except(unitsBefore).Any() &&
                !Game.Instance.State.AreaEffects.All.Cast<object>().Except(areasBefore).Any();
            Sprint25Check("lantern-body-cleanup", clean,
                new JObject { ["capturedResources"] = captured.Count,
                    ["capturedResourcesGone"] = captured.All(value => value == null),
                    ["ownedObjects"] = after,
                    ["unitsExact"] = !Game.Instance.State.Units.All.Cast<object>()
                        .Except(unitsBefore).Any(),
                    ["areasExact"] = !Game.Instance.State.AreaEffects.All.Cast<object>()
                        .Except(areasBefore).Any() },
                "Both disposable Lanterns and every project-owned orb, renderer, material and light are absent while the native fixture is exact.");
            if (!clean) throw new InvalidOperationException(
                "Lantern owned body cleanup failed.");
        }
    }
}
