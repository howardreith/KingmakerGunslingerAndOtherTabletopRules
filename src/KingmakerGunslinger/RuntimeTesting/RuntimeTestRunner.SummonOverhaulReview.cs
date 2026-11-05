using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Three closed, save-free phases; no arbitrary fixture or module toggle.
        private IEnumerable<int> ReviewSummonOverhaul(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            bool off = phase == ExpandedSummoningNativeProfilePolicy.Sprint39OverhaulOffPhase;
            bool indoor = phase == ExpandedSummoningNativeProfilePolicy.Sprint39OverhaulIndoorPhase;
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || _context.FeatureModules.Active.ExpandedSummoning == off)
                throw new InvalidOperationException("Exact fresh overhaul fixture and actual startup module state required.");
            var rows = new JArray();
            var owned = new List<UnitEntityData>();
            string[] keys = off ? new[] { "nalfeshnee", "glabrezu", "vrock", "hezrou", "bone-devil" } :
                indoor ? new[] { "nalfeshnee" } : new[] { "nalfeshnee", "glabrezu", "vrock", "babau", "hezrou", "barbed-devil", "bone-devil", "lillend-azata" };
            try
            {
                foreach (string key in keys)
                {
                    UnitEntityData owner;
                    if (off)
                    {
                        var spec = SummonViewScaleCatalog.All.Single(value => value.CreatureKey == key);
                        var blueprint = ExpandedSummoningBlueprints.Registered.BySymbol.Values.OfType<BlueprintUnit>()
                            .Single(value => value.name == spec.BlueprintName);
                        owner = Game.Instance.EntityCreator.SpawnUnit(blueprint, fixture.Caster.Position, Quaternion.identity, fixture.Scene);
                        if (owner == null || fixture.Created.Contains(owner) || fixture.UnitsBefore.Contains(owner) ||
                            fixture.ExactStart.Contains(owner) || fixture.PartyBefore.Contains(owner) || !ReferenceEquals(owner.Blueprint, blueprint))
                            throw new InvalidOperationException("Fresh OFF compatibility creation refused.");
                        fixture.Created.Add(owner); owned.Add(owner);
                        SetExpandedSummoningBrainActive(owner, false); Game.Instance.EntityCreator.Tick();
                    }
                    else
                    {
                        owner = CastExpandedSummoningOwnTier(fixture, key); owned.Add(owner);
                        Sprint28SetBrainActivePreservingActions(owner, false);
                    }
                    owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    foreach (int frame in Cleanup2WaitAppearance(owner, phase + "-" + key)) yield return frame;
                    if (off == owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                        .Any(value => value.enabled && value.sharedMesh != null && value.sharedMesh.name.StartsWith("KMG_", StringComparison.Ordinal)))
                        throw new InvalidOperationException("Actual module state and original-body presentation disagree.");
                    foreach (int frame in SummonOverhaulSettlePartyCamera(owner)) yield return frame;
                    CaptureSummonOverhaulAppearance(owner);
                    var row = new JObject { ["key"] = key, ["unit"] = owner.UniqueId, ["moduleEnabled"] = !off,
                        ["selection"] = Sprint22SelectionSurface(owner), ["body"] = Presentation32Pose(owner, key == "lillend-azata"),
                        ["selectionRays"] = SummonOverhaulSelectionRays(owner),
                        ["shapes"] = Cleanup2ShapeHierarchy(owner), ["reference"] = Cleanup2ShapeHierarchy(fixture.Caster),
                        ["referenceBlueprint"] = fixture.Caster.Blueprint.name, ["referenceBody"] = Presentation32Pose(fixture.Caster, false),
                        ["matchedImage"] = WriteExpandedSummoningPartyCameraCapture(owner,
                            _request.EvidenceDirectory, "overhaul-" + key + (off ? "-off" : indoor ? "-indoor" : "-daylight") + "-reference.png"),
                        ["cameraScope"] = "Native party camera after scroll and twelve Unity presentation frames; no overhead pose, depth-of-field, brightness or material override.",
                        ["referenceDistanceMeters"] = owner.DistanceTo(fixture.Caster),
                        ["originalScale"] = SurveyVector(owner.View.OriginalScale), ["rootScale"] = SurveyVector(owner.View.transform.localScale) };
                    rows.Add(row);
                    if (!(bool)row["selection"]["selectionSurface"] || !(bool)row["selectionRays"]["bothHit"] || !(bool)row["body"]["finite"])
                        throw new InvalidOperationException("Native body or selection surface unavailable.");
                    if (!indoor)
                    {
                        if (!off) using (var mode = Cleanup2EnterNativeDefault(fixture, phase))
                        {
                            Vector3 destination; string lane, direction;
                            var start = Sprint24AiCombatLane(fixture.Caster, owner, fixture.Hostile ?? fixture.Caster,
                                6f, out destination, out lane, out direction);
                            PlaceExpandedSummoningUnit(owner, start);
                            int quiet = 0; Vector3 previous = owner.Position;
                            foreach (int frame in WaitPresentation32(mode, () => quiet >= 3, 8, () => {
                                bool stable = owner.Commands.Empty && owner.View.MovementAgent.Velocity.sqrMagnitude < .0001f &&
                                    Vector3.Distance(previous, owner.Position) < .01f && owner.Descriptor.State.CanAct;
                                quiet = stable ? quiet + 1 : 0; previous = owner.Position;
                            })) yield return frame;
                            Vector3 supported; JObject ground;
                            if (!Cleanup2CreatureNativeGroundRoot(owner, owner.Position, out supported, out ground) ||
                                Vector3.Distance(supported, owner.Position) >= .02f)
                                throw new InvalidOperationException("Overhaul movement lacks settled native ground support.");
                            row["movementGround"] = ground;
                            start = owner.Position;
                            var command = new UnitMoveTo(destination, .5f); command.Init(owner);
                            if (!command.CanStart) throw new InvalidOperationException("Native movement command refused.");
                            owner.Commands.Run(command);
                            bool captured = false;
                            foreach (int frame in WaitPresentation32(mode, () => command.IsFinished && owner.Commands.Empty, 16, () => {
                                if (!captured && Vector3.Distance(start, owner.Position) > .5f) {
                                    row["movingPose"] = Presentation32Pose(owner, key == "lillend-azata");
                                    row["movingImage"] = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                                        "overhaul-" + key + "-moving.png", key == "lillend-azata" ?
                                            owner.View.GetComponent<LillendHumanVisualAttachment>()?.Body?.name :
                                            TheropodVisualPolicy.BodyRendererName); captured = true;
                                }
                            })) yield return frame;
                            bool moved = command.IsFinished && command.IsActed && Vector3.Distance(start, owner.Position) > 2f;
                            row["movement"] = new JObject { ["command"] = Sprint27AiCommand(command), ["lane"] = lane,
                                ["direction"] = direction, ["start"] = SurveyVector(start), ["destination"] = SurveyVector(destination),
                                ["end"] = SurveyVector(owner.Position), ["moved"] = moved, ["captured"] = captured,
                                ["shapesAfterMovement"] = Cleanup2ShapeHierarchy(owner) };
                            if (!moved || !captured) throw new InvalidOperationException("Actual native movement/clearance incomplete.");
                            if (key == "lillend-azata")
                                foreach (int frame in SummonOverhaulLillendCast(owner, mode, row)) yield return frame;
                        }
                        if (key == "nalfeshnee")
                        {
                            string identity = owner.UniqueId;
                            var oldView = owner.View;
                            var source = owner.Blueprint.Prefab.Load(false);
                            Vector3 sourceScale = source.transform.localScale;
                            oldView.Data = null;
                            UnityEngine.Object.DestroyImmediate(oldView.gameObject);
                            owner.AttachToViewOnLoad(null);
                            foreach (int frame in Cleanup2WaitAppearance(owner, phase + "-reconstruction")) yield return frame;
                            float expected = off ? 1f : 2.10f;
                            bool reconstructed = owner.UniqueId == identity && owner.View != null && !ReferenceEquals(owner.View, oldView) &&
                                ReferenceEquals(owner.View.Data, owner) && Vector3.Distance(owner.View.OriginalScale, sourceScale * expected) < .002f &&
                                Vector3.Distance(owner.View.transform.localScale, sourceScale * expected * owner.View.GetSizeScale()) < .002f;
                            row["reconstruction"] = new JObject { ["pass"] = reconstructed, ["sameIdentity"] = owner.UniqueId == identity,
                                ["expectedMultiplier"] = expected, ["sourceScale"] = SurveyVector(sourceScale),
                                ["originalScale"] = SurveyVector(owner.View.OriginalScale), ["rootScale"] = SurveyVector(owner.View.transform.localScale),
                                ["scope"] = "Owned disposable view replacement through native AttachToViewOnLoad(null); no save serialization, disk load, manual appearance patch or setting mutation." };
                            Sprint25Check("overhaul-reconstruction-" + (off ? "off" : "on"), reconstructed, (JObject)row["reconstruction"],
                                "Normal native reconstruction retains the correct startup-module scale behavior.");
                            if (!reconstructed) throw new InvalidOperationException("Native reconstruction scale mismatch.");
                        }
                    }
                    if (off)
                    {
                        var native = owner.Blueprint.Prefab.Load(false);
                        bool exact = Vector3.Distance(owner.View.OriginalScale, native.transform.localScale) < .002f &&
                            Vector3.Distance(owner.View.transform.localScale, native.transform.localScale * owner.View.GetSizeScale()) < .002f;
                        Sprint25Check("overhaul-off-scale-" + key, exact, row, "Actual OFF construction retains the donor OriginalScale and native size-derived root scale.");
                        if (!exact) throw new InvalidOperationException("OFF donor view enlarged.");
                    }
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                }
                if (!off && !indoor)
                    foreach (int frame in ReviewInfernalBodies34(fixture)) yield return frame;
                Sprint25Check("overhaul-" + phase, true, new JObject { ["bodies"] = rows,
                    ["scope"] = "Native body/material, actual collider and selection surfaces, clear-lane command movement and presentation captures. Images require owner aesthetic review; no full compatibility qualification." },
                    "The selected fresh-process native observations completed.");
            }
            finally
            {
                foreach (var owner in owned.Where(value => !value.Destroyed))
                {
                    owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    if (owner.IsInCombat) owner.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"),
                    new JObject { ["phase"] = phase, ["moduleEnabled"] = !off, ["bodies"] = rows }.ToString(Formatting.Indented));
            }
        }

        private IEnumerable<int> SummonOverhaulSettlePartyCamera(UnitEntityData owner)
        {
            var rig = TeleportationCastingCamera();
            if (rig == null) throw new InvalidOperationException("Native party camera missing.");
            bool waiting = _sprint25WaitingForUnityAppearance;
            try
            {
                _sprint25WaitingForUnityAppearance = true;
                rig.ScrollToImmediately(owner.Position);
                for (int frame = 0; frame < 12; frame++) yield return 0;
            }
            finally { _sprint25WaitingForUnityAppearance = waiting; }
        }

        private static JObject SummonOverhaulSelectionRays(UnitEntityData owner)
        {
            var rig = TeleportationCastingCamera();
            var camera = rig == null ? Camera.main : rig.Camera;
            if (camera == null) throw new InvalidOperationException("Native party camera missing for selection rays.");
            var rows = new JArray(); bool both = true;
            foreach (string member in new[] { "m_SoftCollider", "m_CoreCollider" })
            {
                var collider = ReadExpandedSummoningOptionalMember(owner.View, member) as Collider;
                RaycastHit hit = default(RaycastHit);
                bool present = collider != null && collider.enabled && collider.gameObject.activeInHierarchy;
                Vector3 point = present ? collider.bounds.center : owner.Position;
                var ray = new Ray(camera.transform.position, point - camera.transform.position);
                bool hitExact = present && collider.Raycast(ray, out hit, Vector3.Distance(camera.transform.position, point) + 10f) &&
                    ReferenceEquals(hit.collider, collider);
                both &= hitExact;
                rows.Add(new JObject { ["member"] = member, ["hit"] = hitExact, ["distance"] = hitExact ? hit.distance : (float?)null,
                    ["origin"] = SurveyVector(ray.origin), ["target"] = SurveyVector(point),
                    ["bounds"] = present ? SurveyVector(collider.bounds.size) : null });
            }
            return new JObject { ["bothHit"] = both, ["rays"] = rows,
                ["scope"] = "Read-only native soft/core Collider.Raycast from the real party camera toward each center. Confirms selectable geometric surfaces; no synthetic UI click or selection-state mutation." };
        }

        private IEnumerable<int> SummonOverhaulLillendCast(UnitEntityData owner, Cleanup2NativeDefaultScope mode, JObject row)
        {
            var creation = owner.Blueprint.GetComponents<LillendBardBook32>().Single();
            var book = owner.Descriptor.GetSpellbook(creation.Book);
            var cure = book.GetKnownSpells(1).Single(value => value.Blueprint.AssetGuid == "5590652e1c2225c4ca30c4a699ab3649");
            int oldDamage = owner.Damage, slots = book.GetSpontaneousSlots(1); bool captured = false;
            try
            {
                owner.Damage = 5;
                if (!cure.IsAvailable || !cure.CanTarget(new TargetWrapper(owner))) throw new InvalidOperationException("Native Lillend self-cure unavailable.");
                var cast = new UnitUseAbility(cure, new TargetWrapper(owner)); owner.Commands.Run(cast);
                foreach (int frame in WaitPresentation32(mode, () => cast.IsFinished && cast.ExecutionProcess?.IsEnded == true &&
                    owner.Damage < 5 && owner.Commands.Empty, 24, () => {
                        if (captured || cast.Animation?.IsStarted != true || cast.Animation.ActiveAnimation == null) return;
                        row["castingPose"] = Presentation32Pose(owner, true);
                        row["castingImage"] = WriteExpandedSummoningOverheadStrikeCapture(owner, owner,
                            _request.EvidenceDirectory, "overhaul-lillend-casting.png", 13f); captured = true;
                    })) yield return frame;
                bool pass = captured && cast.IsActed && owner.Damage < 5 && book.GetSpontaneousSlots(1) == slots - 1 &&
                    (bool)row["castingPose"]["finite"];
                row["casting"] = new JObject { ["pass"] = pass, ["command"] = Sprint27AiCommand(cast),
                    ["damage"] = owner.Damage, ["slotsBefore"] = slots, ["slotsAfter"] = book.GetSpontaneousSlots(1) };
                if (!pass) throw new InvalidOperationException("Native Lillend casting presentation incomplete.");
            }
            finally { owner.Damage = oldDamage; }
        }
    }
}
