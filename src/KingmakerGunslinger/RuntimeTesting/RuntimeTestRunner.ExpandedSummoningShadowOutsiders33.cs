using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic;
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
        private IEnumerable<int> ReviewShadowOutsiders33(ExpandedSummoningCorrectionFixture fixture, bool motion = false)
        {
            string[] keys = { "succubus", ShadowOutsiderVisualPolicy.Stalker, ShadowOutsiderVisualPolicy.Demon };
            if (!motion)
            {
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
            }
            foreach (string key in keys.Where(value => !motion || ShadowOutsiderVisualPolicy.Handles(value)))
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
                        if (motion)
                        {
                            Vector3 end; string lane, direction;
                            Vector3 spawned = owner.Position;
                            Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, fixture.Hostile, 6f, out end, out lane, out direction);
                            PlaceExpandedSummoningUnit(owner, origin);
                            Vector3 supported; JObject ground;
                            bool support = Cleanup2CreatureNativeGroundRoot(owner, origin, out supported, out ground) &&
                                Vector3.Distance(owner.Position, supported) < .02f;
                            Sprint25Check("sprint33-ground-" + key + "-" + instance, support, new JObject {
                                ["spawnPosition"] = SurveyVector(spawned), ["root"] = SurveyVector(owner.Position),
                                ["nativeFlyHeight"] = owner.FlyHeight, ["support"] = ground, ["lane"] = lane },
                                "Native Translocate matches the installed ground-support rule on an existing surveyed disposable lane.");
                            if (!support) throw new InvalidOperationException("Native root/support mismatch.");
                        }
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
                            ["root"] = SurveyVector(owner.Position), ["nativeFlyHeight"] = owner.FlyHeight,
                            ["bodyCenter"] = body == null ? null : SurveyVector(body.bounds.center),
                            ["bodySize"] = body == null ? null : SurveyVector(body.bounds.size),
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
                        if (motion) foreach (int frame in ReviewShadowMotion33(fixture, owners[0], key)) yield return frame;
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

        private IEnumerable<int> ReviewStalkerPurge33(ExpandedSummoningCorrectionFixture fixture)
        {
            var owners = new List<UnitEntityData>();
            try
            {
                foreach (string key in new[] { "trumpet-archon", "trumpet-archon", ShadowOutsiderVisualPolicy.Stalker })
                {
                    var unit = CastExpandedSummoningOwnTier(fixture, key); owners.Add(unit);
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                for (int frame = 0; frame < 16; frame++) yield return 0;
                if (!owners[2].Descriptor.State.HasCondition(UnitCondition.Invisible))
                    throw new InvalidOperationException("Natural invisibility precondition absent.");
                foreach (int frame in ReviewTrumpet31Purge(fixture, owners[0], owners[1], owners[2])) yield return frame;
            }
            finally
            {
                foreach (var owner in owners) { owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue(); if (owner.IsInCombat) owner.LeaveCombat(); }
                DisposeExpandedSummoningUnits(fixture.Created, owners.ToArray());
            }
        }

        private IEnumerable<int> ReviewShadowMotion33(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, string key)
        {
            var target = fixture.Hostile;
            var mode = Cleanup2EnterNativeDefault(fixture, ExpandedSummoningNativeProfilePolicy.Sprint32ShadowMotionPhase);
            var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
            bool naturalInvisible = key != ShadowOutsiderVisualPolicy.Stalker ||
                owner.Descriptor.State.HasCondition(UnitCondition.Invisible);
            var contacts = new JArray();
            observer.ObserveWeaponContact = rule => {
                if (!ReferenceEquals(rule.Target, target)) return;
                var command = owner.Commands.Raw.OfType<UnitAttack>().FirstOrDefault(value => !value.IsFinished && ReferenceEquals(value.Target, target));
                contacts.Add(new JObject { ["weapon"] = rule.Weapon.Blueprint.AssetGuid,
                    ["hit"] = rule.AttackRoll?.IsHit == true, ["animationPresent"] = command?.Animation != null,
                    ["animationStarted"] = command?.Animation?.IsStarted == true,
                    ["frame"] = Time.frameCount, ["nativeRange"] = command?.IsUnitEnoughClose,
                    ["capture"] = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                        "s33-" + key + "-contact-" + contacts.Count + ".png", TheropodVisualPolicy.BodyRendererName) });
            };
            EventBus.Subscribe(observer);
            try
            {
                Vector3 destination; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 6f, out destination, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 40f);
                var move = new UnitMoveTo(destination, .5f); move.Init(owner);
                if (!move.CanStart) throw new InvalidOperationException("Native shadow outsider move refused.");
                owner.Commands.Run(move);
                float velocity = 0; bool captured = false;
                foreach (int frame in WaitPresentation32(mode, () => move.IsFinished, 25, () => {
                    velocity = Math.Max(velocity, owner.View.MovementAgent.Velocity.magnitude);
                    if (!captured && Vector3.Distance(origin, owner.Position) > 1f) {
                        WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                            "s33-" + key + "-move.png", TheropodVisualPolicy.BodyRendererName); captured = true;
                    }
                })) yield return frame;
                bool moved = move.IsFinished && Vector3.Distance(origin, owner.Position) > 1f && velocity > .01f && captured;
                Sprint25Check("sprint33-native-motion-" + key, moved, new JObject {
                    ["travel"] = Vector3.Distance(origin, owner.Position), ["velocity"] = velocity,
                    ["result"] = move.Result.ToString(), ["lane"] = lane },
                    "An ordinary native move travels on the surveyed lane with positive velocity and a retained moving frame.");
                if (!moved) throw new InvalidOperationException("Native movement incomplete.");
                foreach (int frame in WaitPresentation32(mode, () => owner.Commands.Empty && owner.View.MovementAgent.Velocity.sqrMagnitude < .0001f, 12)) yield return frame;
                origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 6f, out destination, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, destination);
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                for (int attempt = 0; attempt < 3 && !contacts.Any(value => (bool)value["hit"]); attempt++)
                {
                    var attack = new UnitAttack(target) { ForceFullAttack = true }; attack.Init(owner);
                    if (!attack.CanStart) throw new InvalidOperationException("Native attack refused.");
                    owner.Commands.Run(attack);
                    foreach (int frame in WaitPresentation32(mode, () => attack.IsFinished, 30)) yield return frame;
                }
                bool contact = contacts.Any(value => (bool)value["hit"] && (bool)value["animationPresent"] && (bool)value["animationStarted"]) &&
                    observer.Damage.Any(value => !value.IsFake && value.Damage > 0 && ReferenceEquals(value.Initiator, owner));
                Sprint25Check("sprint33-native-attack-" + key, contact, new JObject { ["contacts"] = contacts,
                    ["positiveDamage"] = observer.Damage.Count(value => !value.IsFake && value.Damage > 0 && ReferenceEquals(value.Initiator, owner)) },
                    "Native command/approach and ordinary rolls produce a successful weapon contact with a live native animation; no forced roll or borrowed giant reach.");
                if (!contact) throw new InvalidOperationException("Native attack/animation contact missing.");
                if (key == ShadowOutsiderVisualPolicy.Stalker)
                {
                    bool retained = naturalInvisible && owner.Descriptor.State.HasCondition(UnitCondition.Invisible) &&
                        owner.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == "94b2838e8a492c44ebf89e7fe7a75a62" && value.IsTurnedOn);
                    Sprint25Check("sprint33-natural-invisibility-attack-safe", retained,
                        new JObject { ["invisibleBefore"] = naturalInvisible, ["invisibleAfter"] = owner.Descriptor.State.HasCondition(UnitCondition.Invisible),
                            ["activeNaturalBuff"] = retained }, "Ordinary native weapon attacks preserve active natural invisibility.");
                    if (!retained) throw new InvalidOperationException("Natural invisibility lost during attacks.");
                }
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                if (owner.IsInCombat) owner.LeaveCombat();
                if (target.IsInCombat) target.LeaveCombat();
                mode.Dispose();
            }
        }
    }
}
