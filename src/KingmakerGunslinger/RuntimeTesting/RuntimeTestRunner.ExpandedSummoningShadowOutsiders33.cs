using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Fixed Sprint33 cells in the existing save-free Sprint32 fixture/clock.
        // Route casting, native appearance, camera and teardown retain their existing helpers.
        private IEnumerable<int> ReviewShadowOutsiders33(ExpandedSummoningCorrectionFixture fixture)
        {
            string[] keys = { "succubus", ShadowOutsiderVisualPolicy.Stalker, ShadowOutsiderVisualPolicy.Demon };
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(value => keys.Contains(value.Creature.Key)).OrderBy(value => value.Creature.Key)
                .ThenBy(value => value.ParentTier).ToArray();
            bool census = routes.Length == 12 && keys.All(key => routes.Where(value => value.Creature.Key == key)
                .Select(value => value.ParentTier).SequenceEqual(new[] { 6, 7, 8, 9 })) &&
                routes.All(value => SummonVisibilityCatalog.IsPublished(value) && !value.Creature.MonsterTemplated &&
                    value.Multiplicity == (value.ParentTier == 6 ? SummonMultiplicity.One : value.ParentTier == 7 ?
                        SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne)) &&
                !ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Any(value => keys.Contains(value.Creature.Key));
            Sprint25Check("sprint33-twelve-routes", census, new JObject { ["count"] = routes.Length },
                "The three existing untemplated VI-IX routes retain all twelve placements and quantities.");
            if (!census) throw new InvalidOperationException("Sprint33 route census differs.");
            Sprint24PlayerRoutes(fixture, routes, Sprint25Check);
            foreach (string key in keys)
            {
                var owners = new List<UnitEntityData>();
                var owned = new List<UnityEngine.Object>();
                Mesh cache = null; Texture2D albedo = null; string[] bones; string status;
                bool custom = ShadowOutsiderVisualPolicy.Handles(key);
                if (custom && !PteranodonAssetRuntime.TryGetShadowOutsiderVisual(key, out cache, out bones, out albedo, out status))
                    throw new InvalidOperationException(key + " original payload missing: " + status);
                var native = custom ? ShadowOutsiderVisualPolicy.SourceView(key) : null;
                var nativeRenderers = native == null ? new SkinnedMeshRenderer[0] : native.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var nativeMeshes = nativeRenderers.Select(value => value.sharedMesh).ToArray();
                var nativeMaterials = nativeRenderers.Select(value => value.sharedMaterial).ToArray();
                try
                {
                    for (int instance = 0; instance < (custom ? 2 : 1); instance++)
                    {
                        var owner = CastExpandedSummoningOwnTier(fixture, key); owners.Add(owner);
                        Sprint28SetBrainActivePreservingActions(owner, false);
                        owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                        bool oldWaiting = _sprint25WaitingForUnityAppearance;
                        try
                        {
                            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
                            while (DateTime.UtcNow < deadline && (owner.View == null || !EntityFadedIn(owner) ||
                                DissolveAmount(owner) > MotionReviewIntactDissolve || !owner.Descriptor.State.CanAct))
                            {
                                _sprint25WaitingForUnityAppearance = owner.View != null && EntityFadedIn(owner) && owner.Descriptor.State.CanAct;
                                Game.Instance.IsPaused = false; yield return 0;
                            }
                        }
                        finally { _sprint25WaitingForUnityAppearance = oldWaiting; }
                        if (owner.View == null) throw new InvalidOperationException(key + " view missing.");
                        var renderers = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        var body = custom ? renderers.SingleOrDefault(value => value.name == TheropodVisualPolicy.BodyRendererName) : renderers.FirstOrDefault();
                        if (custom) owned.AddRange(ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View));
                        Vector3[] vertices = body == null ? new Vector3[0] : Sprint17OriginalWorldVertices(body);
                        bool finite = vertices.Length > 0 && vertices.All(Astral31Finite);
                        var selection = Sprint22SelectionSurface(owner);
                        string frame = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                            "s33-" + key + "-" + instance + ".png", body == null ? null : body.name);
                        bool mesh = body != null && body.sharedMesh != null && (!custom ||
                            (body.sharedMesh.name == "KMG_" + key + "_Original" && body.sharedMesh != cache &&
                             body.sharedMesh.vertexCount == cache.vertexCount && owned.Count > 0));
                        bool exact = mesh && finite && body.enabled && EntityFadedIn(owner) &&
                            DissolveAmount(owner) <= MotionReviewIntactDissolve && (bool)selection["selectionSurface"] &&
                            frame.Contains("inFrame=true") && frame.Contains("screenLit=true") && frame.Contains("rendererEnabled=true");
                        Sprint25Check("sprint33-body-" + key + "-" + instance, exact, new JObject {
                            ["unit"] = owner.Blueprint.AssetGuid, ["owner"] = owner.UniqueId,
                            ["prefab"] = owner.Blueprint.Prefab.AssetId, ["mesh"] = body?.sharedMesh?.name,
                            ["vertices"] = vertices.Length, ["finite"] = finite, ["selection"] = selection,
                            ["partyCamera"] = frame, ["ownedResources"] = owned.Count,
                            ["renderers"] = new JArray(renderers.Select(value => new JObject { ["name"] = value.name,
                                ["mesh"] = value.sharedMesh?.name, ["enabled"] = value.enabled, ["shader"] = value.sharedMaterial?.shader?.name })) },
                            "Actual original geometry (never fallback), finite native skin, native selection and party-camera visibility; Succubus preserved.");
                        if (!exact) throw new InvalidOperationException(key + " body presentation failed.");
                    }
                    if (custom)
                    {
                        var bodies = owners.Select(value => value.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                            .Single(renderer => renderer.name == TheropodVisualPolicy.BodyRendererName)).ToArray();
                        bool distinct = bodies[0].sharedMesh != bodies[1].sharedMesh && bodies[0].sharedMaterial != bodies[1].sharedMaterial;
                        Sprint25Check("sprint33-private-pair-" + key, distinct, new JObject { ["distinct"] = distinct },
                            "Repeated summoned owners have independent mutable mesh/material instances.");
                        if (!distinct) throw new InvalidOperationException("Shared mutable body resources.");
                    }
                }
                finally
                {
                    foreach (var owner in owners) { owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue(); if (owner.IsInCombat) owner.LeaveCombat(); }
                    DisposeExpandedSummoningUnits(fixture.Created, owners.ToArray());
                }
                for (int frame = 0; frame < 16; frame++) yield return 0;
                bool nativeExact = nativeRenderers.Select(value => value.sharedMesh).SequenceEqual(nativeMeshes) &&
                    nativeRenderers.Select(value => value.sharedMaterial).SequenceEqual(nativeMaterials);
                bool clean = owners.All(value => value.Destroyed && !value.IsInState) && owned.All(value => value == null) &&
                    (!custom || (cache != null && albedo != null && nativeExact));
                Sprint25Check("sprint33-body-cleanup-" + key, clean, new JObject {
                    ["privateRemaining"] = owned.Count(value => value != null), ["sourceNativeUnchanged"] = nativeExact },
                    "Disposed owners release private resources while process-cache and native source resources remain intact.");
                if (!clean) throw new InvalidOperationException(key + " cleanup failed.");
            }
        }
    }
}
