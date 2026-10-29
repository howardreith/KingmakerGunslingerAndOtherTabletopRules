using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewInfernalBodies34(ExpandedSummoningCorrectionFixture fixture)
        {
            var trace = new InfernalEvents34();
            var receipt = new JObject { ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint32InfernalBodiesPhase,
                ["events"] = trace.Rows };
            var bodies = new JArray(); receipt["bodies"] = bodies;
            try {
                foreach (string key in new[] { InfernalVisualPolicy34.Barbazu, InfernalVisualPolicy34.Kyton }) {
                    var owners = new List<UnitEntityData>(); var resources = new List<UnityEngine.Object>();
                    Mesh cache; Texture2D albedo; string[] bones; string status;
                    if (!PteranodonAssetRuntime.TryGetInfernalVisual34(key, out cache, out bones, out albedo, out status))
                        throw new InvalidOperationException(key + " original asset absent: " + status);
                    var donor = fixture.Blueprints.OfType<BlueprintUnit>().Single(value => value.AssetGuid == TheropodVisualPolicy.DonorGuid).Prefab.Load(false);
                    var renderers = donor.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    var donorMeshes = renderers.Select(value => value.sharedMesh).ToArray();
                    var donorMaterials = renderers.Select(value => value.sharedMaterial).ToArray();
                    try {
                        for (int instance = 0; instance < 2; instance++) {
                            var owner = CastExpandedSummoningOwnTier(fixture, key); owners.Add(owner);
                            Sprint28SetBrainActivePreservingActions(owner, false);
                            owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                            bool waiting = _sprint25WaitingForUnityAppearance;
                            try {
                                DateTime deadline = DateTime.UtcNow.AddSeconds(15);
                                while (DateTime.UtcNow < deadline && (owner.View == null || !EntityFadedIn(owner) ||
                                    DissolveAmount(owner) > MotionReviewIntactDissolve || !owner.Descriptor.State.CanAct)) {
                                    _sprint25WaitingForUnityAppearance = owner.View != null && EntityFadedIn(owner) && owner.Descriptor.State.CanAct;
                                    Game.Instance.IsPaused = false; yield return 0;
                                }
                            } finally { _sprint25WaitingForUnityAppearance = waiting; }
                            if (owner.View == null) throw new InvalidOperationException(key + " view absent.");
                            var body = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                                .Single(value => value.name == TheropodVisualPolicy.BodyRendererName);
                            resources.AddRange(ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View));
                            var vertices = Sprint17OriginalWorldVertices(body);
                            var selection = Sprint22SelectionSurface(owner);
                            string capture = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                                "s34-" + key + "-body-" + instance + ".png", body.name);
                            bool pass = body.sharedMesh != cache && body.sharedMesh.name == "KMG_" + key + "_Original" &&
                                vertices.Length == cache.vertexCount && vertices.All(Astral31Finite) && body.enabled && EntityFadedIn(owner) &&
                                DissolveAmount(owner) <= MotionReviewIntactDissolve && (bool)selection["selectionSurface"] &&
                                capture.Contains("inFrame=true") && capture.Contains("screenLit=true") && capture.Contains("rendererEnabled=true");
                            var cell = new JObject { ["owner"] = owner.UniqueId, ["key"] = key, ["mesh"] = body.sharedMesh.name,
                                ["vertices"] = vertices.Length, ["enabled"] = body.enabled, ["dissolve"] = DissolveAmount(owner),
                                ["selection"] = selection, ["capture"] = capture, ["bounds"] = body.bounds.ToString(),
                                ["glaive"] = owner.View.GetComponent<BarbazuGlaiveView34>()?.Status,
                                ["chainComponent"] = owner.View.GetComponent<KytonChainVisual34>() != null };
                            bodies.Add(cell); Sprint25Check("sprint34-body-" + key + "-" + instance, pass, cell,
                                "Live original mesh, fader, finite skin, native selection and party-camera visibility.");
                            if (!pass) throw new InvalidOperationException(key + " body presentation incomplete.");
                        }
                        var pair = owners.Select(owner => owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                            .Single(value => value.name == TheropodVisualPolicy.BodyRendererName)).ToArray();
                        bool independent = pair[0].sharedMesh != pair[1].sharedMesh && pair[0].sharedMaterial != pair[1].sharedMaterial;
                        Sprint25Check("sprint34-private-pair-" + key, independent, new JObject { ["independent"] = independent },
                            "Repeated owners hold separate mutable mesh/material instances.");
                        if (!independent) throw new InvalidOperationException("Shared mutable original body.");
                        foreach (int frame in ReviewInfernalMotion34(fixture, owners[0], key, trace, receipt)) yield return frame;
                    } finally {
                        foreach (var owner in owners.Where(value => !value.Destroyed)) {
                            owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                            if (owner.IsInCombat) owner.LeaveCombat();
                        }
                        DisposeExpandedSummoningUnits(fixture.Created, owners.ToArray());
                    }
                    for (int frame = 0; frame < 16; frame++) yield return 0;
                    bool clean = owners.All(value => value.Destroyed && !value.IsInState) && resources.All(value => value == null) &&
                        cache != null && albedo != null && renderers.Select(value => value.sharedMesh).SequenceEqual(donorMeshes) &&
                        renderers.Select(value => value.sharedMaterial).SequenceEqual(donorMaterials);
                    Sprint25Check("sprint34-body-cleanup-" + key, clean, new JObject { ["privateRemaining"] = resources.Count(value => value != null) },
                        "Every owned body/weapon component is released; original cache and native donor resources remain unchanged.");
                    if (!clean) throw new InvalidOperationException("Infernal private resource teardown mismatch.");
                }
            } finally {
                trace.Dispose();
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "sprint34-infernal-bodies.json"), receipt.ToString(Formatting.Indented));
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private static JArray InfernalEquipment34(UnitEntityData owner)
        {
            return new JArray(owner.Body.HandsEquipmentSets.Select((set, index) => new JObject {
                ["index"] = index, ["active"] = owner.Body.CurrentHandEquipmentSetIndex == index,
                ["item"] = set.PrimaryHand.MaybeItem?.Blueprint.AssetGuid,
                ["effectiveWeapon"] = set.PrimaryHand.MaybeWeapon?.Blueprint.AssetGuid }));
        }

        private static JObject InfernalRenderedGeometry34(UnitEntityData owner, UnitEntityData target)
        {
            var body = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Single(value => value.name == TheropodVisualPolicy.BodyRendererName);
            var chain = owner.View.GetComponent<KytonChainVisual34>();
            var glaive = owner.View.GetComponent<BarbazuGlaiveView34>();
            var result = new JObject { ["frame"] = Time.frameCount, ["owner"] = SurveyVector(owner.Position),
                ["target"] = SurveyVector(target.Position), ["targetRadius"] = target.Corpulence,
                ["rendererBounds"] = body.bounds.ToString(), ["rendererScale"] = SurveyVector(body.transform.lossyScale) };
            if (chain != null) {
                var snapshot = new Mesh { name = "KMG_Runtime_S34_ChainBake" };
                try {
                    body.BakeMesh(snapshot);
                    var points = snapshot.vertices.Select(body.transform.TransformPoint).ToArray();
                    var indices = PteranodonAssetRuntime.KytonChains34()[chain.ContactChain].Vertices;
                    var nearest = indices.OrderBy(index => Vector3.SqrMagnitude(points[index] - chain.ContactPoint)).First();
                    result["chain"] = chain.ContactChain; result["chainResting"] = chain.Resting;
                    result["manualContact"] = SurveyVector(chain.ContactPoint);
                    result["bakedNearest"] = SurveyVector(points[nearest]);
                    result["bakeToManualGap"] = Vector3.Distance(points[nearest], chain.ContactPoint);
                    result["bakedVertexCount"] = points.Length;
                    result["bakedLocalBounds"] = snapshot.bounds.ToString();
                } finally { UnityEngine.Object.DestroyImmediate(snapshot); }
            }
            if (glaive?.Model != null) result["glaiveMeshes"] = new JArray(glaive.Model.GetComponentsInChildren<MeshFilter>(true)
                .Where(filter => filter.sharedMesh != null).Select(filter => new JObject {
                    ["name"] = filter.sharedMesh.name, ["readable"] = filter.sharedMesh.isReadable,
                    ["localBounds"] = filter.sharedMesh.bounds.ToString(),
                    ["origin"] = SurveyVector(filter.transform.position), ["scale"] = SurveyVector(filter.transform.lossyScale),
                    ["right"] = SurveyVector(filter.transform.right), ["up"] = SurveyVector(filter.transform.up),
                    ["forward"] = SurveyVector(filter.transform.forward),
                    ["boundsCenter"] = SurveyVector(filter.transform.TransformPoint(filter.sharedMesh.bounds.center)) }));
            return result;
        }

        private IEnumerable<int> ReviewInfernalMotion34(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            string key, InfernalEvents34 trace, JObject receipt)
        {
            var target = fixture.Hostile;
            var mode = Cleanup2EnterNativeDefault(fixture, ExpandedSummoningNativeProfilePolicy.Sprint32InfernalBodiesPhase);
            var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
            var contacts = new JArray(); receipt[key + "-contacts"] = contacts;
            var errors = new JArray(); receipt[key + "-capture-errors"] = errors;
            int contactFrame = -100, sampledContact = -1;
            var renderedFrames = new JArray(); receipt[key + "-rendered-contact-frames"] = renderedFrames;
            observer.ObserveWeaponContact = rule => {
                if (!ReferenceEquals(rule.Target, target)) return;
                try {
                    var command = owner.Commands.Raw.OfType<UnitAttack>().FirstOrDefault(value => !value.IsFinished && ReferenceEquals(value.Target, target));
                    var chains = owner.View.GetComponent<KytonChainVisual34>();
                    var glaive = owner.View.GetComponent<BarbazuGlaiveView34>();
                    var clip = command?.Animation?.ActiveAnimation?.GetPlayableClip();
                    contacts.Add(new JObject { ["weapon"] = rule.Weapon.Blueprint.AssetGuid, ["hit"] = rule.AttackRoll?.IsHit,
                        ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["nativeClip"] = clip?.name,
                        ["animationStarted"] = command?.Animation?.IsStarted == true,
                        ["animationTime"] = command?.Animation?.GetTime(),
                        ["powerAttack"] = key == InfernalVisualPolicy34.Barbazu ? Sprint22PowerAttackState(owner,
                            owner.Descriptor.ActivatableAbilities.Enumerable.Single(value => value.Blueprint.AssetGuid == "a7b339e4f6ff93a4697df5d7a87ff619")) : null,
                        ["animationStyle"] = rule.Weapon.GetAnimationStyle(false).ToString(), ["rangeFeet"] = rule.Weapon.AttackRange.Value,
                        ["chain"] = chains?.ContactChain, ["chainContacts"] = chains?.Contacts, ["chainContactGap"] = chains?.ContactGap,
                        ["renderedGeometry"] = InfernalRenderedGeometry34(owner, target),
                        ["glaiveVisible"] = glaive?.Model?.activeInHierarchy,
                        ["glaiveRenderers"] = glaive?.Model == null ? null : new JArray(glaive.Model.GetComponentsInChildren<Renderer>(true)
                            .Select(renderer => new JObject { ["name"] = renderer.name, ["bounds"] = renderer.bounds.ToString(), ["enabled"] = renderer.enabled })),
                        ["capture"] = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                            "s34-" + key + "-contact-" + contacts.Count + ".png", TheropodVisualPolicy.BodyRendererName) });
                    contactFrame = Time.frameCount; sampledContact = contacts.Count - 1;
                } catch (Exception error) { errors.Add(error.ToString()); }
            };
            EventBus.Subscribe(observer);
            try {
                Vector3 destination; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 6f, out destination, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 40f);
                // Translocate grounds the view and the native agent settles on a
                // later frame. Reuse the presentation fixture's quiet boundary;
                // never count that placement correction as observed locomotion.
                int quiet = 0; Vector3 previous = owner.Position;
                foreach (int frame in WaitPresentation32(mode, () => quiet >= 3, 8, () => {
                    bool stable = owner.Commands.Empty && owner.View.MovementAgent.Velocity.sqrMagnitude < .0001f &&
                        Vector3.Distance(previous, owner.Position) < .01f && owner.Descriptor.State.CanAct;
                    quiet = stable ? quiet + 1 : 0; previous = owner.Position;
                })) yield return frame;
                origin = owner.Position;
                Vector3 supported; JObject ground;
                bool grounded = Cleanup2CreatureNativeGroundRoot(owner, origin, out supported, out ground) &&
                    Vector3.Distance(supported, origin) < .02f;
                receipt[key + "-move-ground"] = ground;
                if (!grounded) throw new InvalidOperationException("Infernal movement starts outside native ground support.");
                var move = new UnitMoveTo(destination, .5f); move.Init(owner);
                if (!move.CanStart) throw new InvalidOperationException("Native infernal move refused.");
                owner.Commands.Run(move); float velocity = 0; bool movingCapture = false;
                var movement = new JObject { ["start"] = SurveyVector(origin), ["goal"] = SurveyVector(destination),
                    ["lane"] = lane, ["queued"] = owner.Commands.Contains(move), ["before"] = Sprint27AiCommand(move) };
                receipt[key + "-move"] = movement;
                if (!owner.Commands.Contains(move)) throw new InvalidOperationException("Native move was not queued.");
                foreach (int frame in WaitPresentation32(mode, () => move.IsFinished, 25, () => {
                    velocity = Math.Max(velocity, owner.View.MovementAgent.Velocity.magnitude);
                    if (!movingCapture && Vector3.Distance(origin, owner.Position) > 1f) {
                        WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory, "s34-" + key + "-move.png", TheropodVisualPolicy.BodyRendererName);
                        movingCapture = true;
                    }
                })) yield return frame;
                movement["after"] = Sprint27AiCommand(move); movement["end"] = SurveyVector(owner.Position);
                movement["travel"] = Vector3.Distance(origin, owner.Position); movement["velocity"] = velocity;
                movement["goalGap"] = Vector3.Distance(destination, owner.Position); movement["movingCapture"] = movingCapture;
                bool moved = move.IsFinished && Vector3.Distance(origin, owner.Position) > 1f && velocity > .01f && movingCapture;
                Sprint25Check("sprint34-move-" + key, moved, movement,
                    "Native movement completes on the surveyed lane with actual motion and a retained frame.");
                if (!moved) throw new InvalidOperationException("Native infernal movement incomplete.");
                foreach (int frame in WaitPresentation32(mode, () => owner.Commands.Empty && owner.View.MovementAgent.Velocity.sqrMagnitude < .0001f, 12)) yield return frame;
                var profile = InfernalMartialProfile34.For(owner);
                // The owned disposable faction uses the native non-player
                // PowerAttackWatcher, which turns PA on at the first roll.
                // Settle ON before these visual commands; OFF/live scaling
                // is a separate mechanics control, not inferred here.
                foreach (bool alternate in new[] { false, true }) {
                    if (alternate && key == InfernalVisualPolicy34.Barbazu) {
                        receipt[key + "-sets-before"] = InfernalEquipment34(owner);
                        int index = Enumerable.Range(0, owner.Body.HandsEquipmentSets.Count).Single(value =>
                            ReferenceEquals(owner.Body.HandsEquipmentSets[value].PrimaryHand.MaybeItem?.Blueprint, profile.Claw));
                        var change = new UnitSwitchHandEquipmentSet(index); owner.Commands.Run(change);
                        foreach (int frame in WaitPresentation32(mode, () => change.IsFinished, 20)) yield return frame;
                        receipt[key + "-sets-after"] = InfernalEquipment34(owner);
                        if (owner.Body.CurrentHandEquipmentSetIndex != index ||
                            !ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon?.Blueprint, profile.Claw)) throw new InvalidOperationException("Natural set switch failed.");
                    }
                    if (alternate && key == InfernalVisualPolicy34.Kyton) {
                        var blueprint = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == "KMG_Summoning_Infernal_KytonChainDevil_DancingChains");
                        var ability = new AbilityData(owner.Descriptor.Abilities.GetAbility(blueprint));
                        var activate = new UnitUseAbility(ability, new TargetWrapper(owner)); owner.Commands.Run(activate);
                        foreach (int frame in WaitPresentation32(mode, () => activate.IsFinished, 20)) yield return frame;
                        if (!profile.ControlsOwnChains(owner)) throw new InvalidOperationException("Owned chain control failed.");
                    }
                    InfernalCombatLane34(fixture, owner, target, alternate ? (key == InfernalVisualPolicy34.Kyton ? 20 : 3) : 8);
                    if (key == InfernalVisualPolicy34.Barbazu)
                        foreach (int frame in Sprint22SetPowerAttack(owner, true)) yield return frame;
                    int start = trace.Rows.Count, contactStart = contacts.Count;
                    var attack = new UnitAttack(target) { ForceFullAttack = true, IsSingleAttack = false }; attack.Init(owner);
                    if (!attack.CanStart) throw new InvalidOperationException("Native infernal attack refused.");
                    var costs = new JArray();
                    using (var cost = new FirearmReviewCosts(owner, costs)) {
                        owner.Commands.Run(attack);
                        foreach (int frame in WaitPresentation32(mode, () => attack.IsFinished, 35, () => {
                            int lag = Time.frameCount - contactFrame;
                            if (sampledContact < 0 || lag < 1 || lag > 3) return;
                            var rendered = InfernalRenderedGeometry34(owner, target);
                            rendered["contactIndex"] = sampledContact; rendered["framesAfterContact"] = lag;
                            rendered["capture"] = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                                "s34-" + key + "-contact-" + sampledContact + "-after-" + lag + ".png", TheropodVisualPolicy.BodyRendererName);
                            renderedFrames.Add(rendered);
                        })) yield return frame;
                        var rows = trace.Rows.Skip(start).Where(row => (string)row["event"] == "attack" && (string)row["owner"] == owner.UniqueId).ToArray();
                        var observed = contacts.Skip(contactStart).ToArray();
                        int expected = key == InfernalVisualPolicy34.Kyton ? 4 : 2;
                        bool correct = attack.IsFinished && attack.Result == UnitCommand.ResultType.Success && rows.Length == expected &&
                            observed.Length == expected && errors.Count == 0 && observed.All(row => (bool)row["animationStarted"] && !string.IsNullOrEmpty((string)row["nativeClip"])) &&
                            (key != InfernalVisualPolicy34.Kyton || observed.All(row => (float)row["chainContactGap"] < .15f)) &&
                            (key != InfernalVisualPolicy34.Barbazu || observed.All(row => (bool)row["glaiveVisible"] == !alternate));
                        var cell = new JObject { ["owner"] = owner.UniqueId, ["alternate"] = alternate, ["command"] = Sprint27AiCommand(attack),
                            ["nativePlan"] = attack.AllAttacks.Count, ["attacks"] = new JArray(rows), ["contacts"] = new JArray(observed), ["costs"] = costs };
                        Sprint25Check("sprint34-native-contact-" + key + "-" + alternate, correct, cell,
                            "One native full attack uses the correct alternative routine with actual animation/contact and no extra attacks.");
                        if (!correct) throw new InvalidOperationException("Infernal routine or live contact mismatch.");
                        if (alternate && key == InfernalVisualPolicy34.Barbazu) {
                            bool bothHit = rows.All(row => (bool)row["hit"]);
                            var beard = trace.Rows.Skip(start).Where(row => (string)row["event"] == "damage" &&
                                (string)row["owner"] == owner.UniqueId && (string)row["target"] == target.UniqueId &&
                                !(bool)row["hasAttackRoll"] && ((JArray)row["packets"]).Any(packet => (string)packet["dice"] == "1d8")).ToArray();
                            bool conditional = beard.Length == (bothHit ? 1 : 0);
                            Sprint25Check("sprint34-beard-observed-condition", conditional, new JObject {
                                ["bothNativeClawsHit"] = bothHit, ["beardEvents"] = new JArray(beard),
                                ["command"] = Sprint27AiCommand(attack) },
                                "Beard's separate native damage packet occurs exactly when both claws of this command hit the same target; natural misses stay recorded.");
                            if (!conditional) throw new InvalidOperationException("Observed Beard condition differs.");
                        }
                    }
                    owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    foreach (int frame in WaitPresentation32(mode, () => owner.Commands.Empty &&
                        (owner.View.GetComponent<KytonChainVisual34>()?.Resting ?? true), 10)) yield return frame;
                }
            } finally {
                EventBus.Unsubscribe(observer);
                owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                if (owner.IsInCombat) owner.LeaveCombat(); if (target.IsInCombat) target.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat(); mode.Dispose();
            }
        }
    }
}
