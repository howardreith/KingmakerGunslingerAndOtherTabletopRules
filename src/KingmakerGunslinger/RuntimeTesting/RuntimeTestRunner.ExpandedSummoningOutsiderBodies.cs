using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Pathfinding;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static JArray Outsider30BodyResources(UnitEntityData actor)
        {
            var resources = actor.Descriptor.Abilities.RawFacts.OfType<Ability>().Where(fact =>
                fact.Blueprint.Type == AbilityType.SpellLike)
                .SelectMany(fact => fact.Blueprint.ComponentsArray.OfType<AbilityResourceLogic>())
                .Select(component => component.RequiredResource).Where(value => value != null)
                .Distinct().OrderBy(value => value.AssetGuid, StringComparer.Ordinal);
            return new JArray(resources.Select(value => new JObject {
                ["guid"] = value.AssetGuid, ["name"] = value.name,
                ["amount"] = actor.Descriptor.Resources.GetResourceAmount(value) }));
        }

        private IEnumerable<int> ReviewOutsider30Bodies(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                ResetExpandedSummoningHostile(fixture);
                var completed = new List<UnitEntityData>();
                var items = new List<ItemEntity>();
                var itemOwners = new Dictionary<ItemEntity, UnitEntityData>();
                var privateResources = new List<UnityEngine.Object>();
                var scene = SnapshotReferences(fixture.SceneEntities);
                var targetBuffs = fixture.Hostile.Buffs.RawFacts.Select(value => value.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal).ToArray();
                var casterBuffs = fixture.Caster.Buffs.RawFacts.Select(value => value.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal).ToArray();
                var areas = Game.Instance.State.AreaEffects.All.ToArray();
                var partyItems = Game.Instance.Player.Inventory.Items.ToArray();
                var partyCounts = partyItems.Select(value => value.Count).ToArray();
                var rig = TeleportationCastingCamera();
                var camera = rig == null ? Camera.main : rig.Camera;
                if (camera == null) throw new InvalidOperationException("Original body pair requires the native party camera.");
                var targetTexture = camera.targetTexture;
                var activeTexture = RenderTexture.active;
                float fov = camera.fieldOfView, orthoSize = camera.orthographicSize;
                bool ortho = camera.orthographic;
                Func<JObject> reset = () => {
                    var row = Lemure30ResetState(fixture, completed, targetBuffs, areas);
                    var live = Game.Instance.State.Units.All.Where(value => value != null && !value.Destroyed && value.IsInState).ToArray();
                    var itemRows = new JArray(items.Select(value => OutsiderEscapeItemState(value, itemOwners[value], live)));
                    bool itemSafe = itemRows.All(value => (bool)value["safe"]);
                    bool casterExact = casterBuffs.SequenceEqual(fixture.Caster.Buffs.RawFacts.Select(value => value.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal));
                    bool casterMemory = completed.All(value => !fixture.Caster.Memory.Contains(value));
                    bool noGlobalCombat = !Game.Instance.Player.IsInCombat;
                    bool casterCommands = !fixture.Caster.Commands.Raw.Any(value => value != null && !value.IsFinished) && !fixture.Caster.IsInCombat;
                    bool sceneExact = SameReferences(scene, SnapshotReferences(fixture.SceneEntities));
                    bool partyExact = Sprint28PartyInventoryExact(partyItems, partyCounts);
                    bool objectsGone = privateResources.All(value => value == null);
                    bool cameraExact = ReferenceEquals(camera.targetTexture, targetTexture) && ReferenceEquals(RenderTexture.active, activeTexture) &&
                        camera.fieldOfView == fov && camera.orthographicSize == orthoSize && camera.orthographic == ortho;
                    bool noBodies = !live.Any(value => value.Blueprint.AssetGuid == LemureVisualPolicy.BlueprintGuid || value.Blueprint.AssetGuid == DretchVisualPolicy.BlueprintGuid);
                    bool noReviewCamera = !Resources.FindObjectsOfTypeAll<Camera>().Any(value => value != null && value.name == "KMG_Sprint17OwnedReviewCamera");
                    row["itemStates"] = itemRows; row["itemsSafe"] = itemSafe;
                    row["casterBuffsExact"] = casterExact; row["casterCommandsClear"] = casterCommands;
                    row["casterMemoryClear"] = casterMemory; row["globalCombatClear"] = noGlobalCombat;
                    row["sceneReferencesExact"] = sceneExact; row["partyInventoryExact"] = partyExact;
                    row["privateResourcesReleased"] = objectsGone; row["privateResourceCount"] = privateResources.Count;
                    row["cameraSettingsAndTargetsExact"] = cameraExact; row["reviewCameraAbsent"] = noReviewCamera;
                    row["bothSpeciesAbsent"] = noBodies;
                    row["clean"] = (bool)row["clean"] && itemSafe && casterExact && casterCommands && casterMemory && noGlobalCombat && sceneExact && partyExact && objectsGone && cameraExact && noBodies && noReviewCamera;
                    return row;
                };
                foreach (string key in RuntimeTestScenarioCatalog.Sprints30And31OutsiderBodySequence)
                {
                    var before = reset(); before["subcase"] = key;
                    Sprint25Check("outsider-bodies-" + key + "-preconditions", (bool)before["clean"], before,
                        "Fixed same-artifact/profile RTWP body subcase starts only from observed exact disposable fixture, item, resource, camera and combat preconditions.");
                    if (!(bool)before["clean"]) throw new InvalidOperationException("Body-pair preconditions failed; later cases unrun: " + key);
                    UnitEntityData owner = null, sibling = null;
                    Mesh cache; string[] bones; Texture2D albedo; string status;
                    bool loaded = key == "lemure" ? PteranodonAssetRuntime.TryGetLemureVisual(out cache, out bones, out albedo, out status) :
                        key == "dretch" ? PteranodonAssetRuntime.TryGetDretchVisual(out cache, out bones, out albedo, out status) :
                        throw new InvalidOperationException("Unsupported compiled body subcase.");
                    if (!loaded) throw new InvalidOperationException("Original body cache not ready: " + key + ":" + status);
                    try
                    {
                        owner = CastExpandedSummoningOwnTier(fixture, key);
                        sibling = CastExpandedSummoningOwnTier(fixture, key);
                        foreach (var actor in new[] { owner, sibling })
                        {
                            completed.Add(actor);
                            Sprint28SetBrainActivePreservingActions(actor, false);
                            foreach (var item in actor.Descriptor.Inventory.Items)
                            { items.Add(item); itemOwners.Add(item, actor); }
                        }
                        var residue = reset(); residue["subcase"] = key;
                        bool detected = !(bool)residue["clean"] && !(bool)residue["bothSpeciesAbsent"] && !(bool)residue["itemsSafe"];
                        Sprint25Check("outsider-bodies-" + key + "-live-residue-refused", detected, residue,
                            "The same next-case observer rejects intentionally still-live exact owned actors/items, rather than trusting a cleanup return value.");
                        if (!detected) throw new InvalidOperationException("Body-pair live-residue detector failed.");
                        foreach (int frame in Outsider30NativeAppearance(owner, key)) yield return frame;
                        var beforeResources = Outsider30BodyResources(owner);
                        var siblingResources = Outsider30BodyResources(sibling);
                        var renderers = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        var body = renderers.Single(value => value.name == TheropodVisualPolicy.BodyRendererName);
                        var otherBody = sibling.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(value => value.name == TheropodVisualPolicy.BodyRendererName);
                        privateResources.AddRange(ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View));
                        privateResources.AddRange(ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(sibling.View));
                        var pose = Sprint17OriginalWorldVertices(body);
                        bool finite = pose.Length > 0 && pose.All(point => !float.IsNaN(point.x) && !float.IsInfinity(point.x) && !float.IsNaN(point.y) && !float.IsInfinity(point.y) && !float.IsNaN(point.z) && !float.IsInfinity(point.z));
                        var selection = Sprint22SelectionSurface(owner);
                        string party = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                            "s30-" + key + "-body-party.png", TheropodVisualPolicy.BodyRendererName);
                        string supporting = CaptureSprint17OriginalBodyFrame(owner, body, "s30-" + key);
                        var afterResources = Outsider30BodyResources(owner);
                        var siblingAfterResources = Outsider30BodyResources(sibling);
                        bool ownerBrainActive = (bool)ReadExpandedSummoningOptionalMember(owner, "IsBrainActive");
                        bool siblingBrainActive = (bool)ReadExpandedSummoningOptionalMember(sibling, "IsBrainActive");
                        var marker = owner.Blueprint.ComponentsArray.OfType<OutsiderExtraplanarIdentity>().Single().NativeMarker;
                        var missile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, FcbMagicMissileGuid, "body pair real no-SR consumer");
                        var context = new AbilityExecutionContext(new AbilityData(missile, fixture.Hostile.Descriptor), new AbilityParams { CasterLevel = 1, SpellLevel = 1 }, new TargetWrapper(owner), null);
                        var sr = context.TriggerRule(new RuleSpellResistanceCheck(context, owner));
                        int partSr = owner.Get<UnitPartSpellResistance>()?.GetValue(context) ?? 0;
                        bool resourcesExact = JToken.DeepEquals(beforeResources, afterResources) && JToken.DeepEquals(siblingResources, siblingAfterResources) &&
                            (key == "lemure" ? beforeResources.Count == 0 : beforeResources.Count == 2 && beforeResources.All(value => (int)value["amount"] == 1) && siblingResources.Count == 2 && siblingResources.All(value => (int)value["amount"] == 1));
                        bool exact = !ownerBrainActive && !siblingBrainActive && body.enabled && EntityFadedIn(owner) && finite && (bool)selection["selectionSurface"] &&
                            body.sharedMesh.name == "KMG_" + key + "_Original" && body.sharedMesh != cache && otherBody.sharedMesh != cache &&
                            body.sharedMesh != otherBody.sharedMesh && body.sharedMaterial != otherBody.sharedMaterial &&
                            body.sharedMesh.vertexCount == cache.vertexCount && body.bones.Select(value => value.name).OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(bones.OrderBy(value => value, StringComparer.Ordinal)) &&
                            ReferenceEquals(body.sharedMaterial.mainTexture, albedo) &&
                            renderers.Single(value => value.name == TheropodVisualPolicy.AccessoryRendererName).enabled == false &&
                            party.Contains("inFrame=true") && party.Contains("screenLit=true") && party.Contains("rendererEnabled=true") &&
                            party.Contains("faderVisible=true") && supporting.Contains("supporting-art-only") && resourcesExact &&
                            partSr == 0 && sr.SpellResistance == 0 && !sr.TargetIsImmune && owner.Descriptor.HasFact(marker) &&
                            items.Where(value => ReferenceEquals(itemOwners[value], owner) || ReferenceEquals(itemOwners[value], sibling)).All(value => !value.IsLootable);
                        Sprint25Check("outsider-bodies-" + key + "-presentation", exact,
                            new JObject { ["subcase"] = key, ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                                ["assetStatus"] = status, ["attachment"] = ExpandedSummoningPteranodonViewPatch.DescribeView(owner.View),
                                ["partyCamera"] = party, ["supportingFrame"] = supporting, ["selection"] = selection,
                                ["posedVertices"] = pose.Length, ["poseFinite"] = finite, ["meshInstance"] = body.sharedMesh.GetInstanceID(),
                                ["siblingMeshInstance"] = otherBody.sharedMesh.GetInstanceID(), ["cacheInstance"] = cache.GetInstanceID(),
                                ["materialInstance"] = body.sharedMaterial.GetInstanceID(), ["siblingMaterialInstance"] = otherBody.sharedMaterial.GetInstanceID(),
                                ["bones"] = new JArray(body.bones.Select(value => value.name)), ["resourcesBefore"] = beforeResources,
                                ["resourcesAfter"] = afterResources, ["siblingResourcesBefore"] = siblingResources, ["siblingResourcesAfter"] = siblingAfterResources,
                                ["nativePartSr"] = partSr, ["nativeRuleSr"] = sr.SpellResistance, ["nativeMarker"] = marker.AssetGuid,
                                ["nativeMarkerPresent"] = owner.Descriptor.HasFact(marker), ["ownerBrainActive"] = ownerBrainActive, ["siblingBrainActive"] = siblingBrainActive,
                                ["scope"] = "actual native bodies/camera/ownership; no attack, root-slot or complete creature qualification" },
                            "Actual original body is intact/visible/finite/selectable at the native party camera, with private sibling meshes/materials, original albedo and no item/resource/SR side effects.");
                        if (!exact) throw new InvalidOperationException("Original body presentation failed; later subcases unrun: " + key);
                    }
                    finally
                    {
                        foreach (var actor in new[] { owner, sibling }.Where(value => value != null))
                        {
                            actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                            if (actor.IsInCombat) actor.LeaveCombat();
                            if (fixture.Caster.Memory.Contains(actor)) fixture.Caster.Memory.Remove(actor);
                            if (actor.Memory.Contains(fixture.Caster)) actor.Memory.Remove(fixture.Caster);
                            if (fixture.Hostile.Memory.Contains(actor)) fixture.Hostile.Memory.Remove(actor);
                            if (actor.Memory.Contains(fixture.Hostile)) actor.Memory.Remove(fixture.Hostile);
                        }
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(value => value != null).ToArray());
                        ResetExpandedSummoningHostile(fixture);
                        Game.Instance.Player.UpdateIsInCombat();
                    }
                    for (int frame = 0; frame < 3; frame++) yield return 0;
                    var after = reset(); after["subcase"] = key;
                    after["cacheAlive"] = cache != null && albedo != null;
                    bool clean = (bool)after["clean"] && cache != null && albedo != null;
                    Sprint25Check("outsider-bodies-" + key + "-cleanup", clean, after,
                        "Exact owned bodies/items/private meshes/materials/camera are gone and next-case preconditions are observed; immutable source cache remains published.");
                    if (!clean) throw new InvalidOperationException("Body-pair contamination; later subcases unrun: " + key);
                }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> Outsider30NativeAppearance(UnitEntityData unit, string key)
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
            SkinnedMeshRenderer body = null;
            bool appearanceBuffAbsent = false, canAct = false, canMove = false;
            bool fadedIn = false;
            float dissolve = 1f;
            double before = Game.Instance.Player.GameTime.TotalSeconds;
            DateTime deadline = DateTime.UtcNow.AddSeconds(10);
            bool waitingBefore = _sprint25WaitingForUnityAppearance;
            try
            {
                for (; frames < 1800 && DateTime.UtcNow < deadline; frames++)
                {
                    body = unit.View == null ? null : unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                        .SingleOrDefault(value => value.name == TheropodVisualPolicy.BodyRendererName);
                    dissolve = DissolveAmount(unit);
                    fadedIn = EntityFadedIn(unit);
                    appearanceBuffAbsent = unit.Descriptor.Buffs.GetBuff(
                        Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null;
                    canAct = unit.Descriptor.State.CanAct;
                    canMove = unit.Descriptor.State.CanMove;
                    ready = body != null && body.enabled && appearanceBuffAbsent &&
                        canAct && canMove && fadedIn && dissolve >= 0f && dissolve <= .02f;
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
            Sprint25Check("native-appearance-ready-" + key, ready,
                new JObject { ["unit"] = unit.UniqueId, ["frames"] = frames,
                    ["spawnPosition"] = SurveyVector(spawnPosition),
                    ["nativeGroundPosition"] = SurveyVector(ground.clampedPosition),
                    ["gameSecondsDuringWait"] = Game.Instance.Player.GameTime.TotalSeconds - before,
                    ["bodyPresent"] = body != null, ["bodyEnabled"] = body != null && body.enabled,
                    ["appearanceBuffAbsent"] = appearanceBuffAbsent,
                    ["canAct"] = canAct, ["canMove"] = canMove,
                    ["fadedIn"] = fadedIn, ["dissolve"] = dissolve },
                "Native appearance prerequisites and finite intact material dissolve settle near the party before observation or isolated placement; no renderer, fader or material state is forced.");
            if (!ready) throw new InvalidOperationException("Native outsider appearance did not settle: " + key);
        }
    }
}
