using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Visual.Decals;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Spells.MagicCircle;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> RunCircleTerrainScene()
        {
            // A separate read-only Working request: the mansion has no suitable
            // walkable elevation. Use the native, authored trading-post scene;
            // do not manufacture a test ramp or edit any campaign/save data.
            var game = Game.Instance; var origin = game.CurrentlyLoadedArea;
            var party = game.Player.Party.ToArray(); var positions = party.Select(unit => unit.Position).ToArray();
            var inventory = game.Player.Inventory.Items.ToArray(); var counts = inventory.Select(item => item.Count).ToArray();
            var money = game.Player.Money;
            var camera = TeleportationCastingCamera(); var cameraPosition = camera.transform.position; var cameraTarget = camera.GetPosition();
            var entry = BlueprintLibraryLookup.RequireExact<BlueprintLocation>(BlueprintBootstrap.Library,
                WordOfRecallDestinationPolicy.OlegId, "native trading-post location").AreaEntrance;
            if (origin == null || entry?.Area == null || entry.Area == origin || game.CurrentMode != GameModeType.Default)
                throw new InvalidOperationException("Exact local-area origin and authored trading-post entry required.");
            _circleUiScreens = new NativeIconScreenEvidence(_request);
            Application.logMessageReceived += ObserveCircleUiException;
            try {
                CircleUiCapture("terrain-native-destination", new { origin = origin.AssetGuid, entry = entry.AssetGuid,
                    destination = entry.Area.AssetGuid, entry.Area.name, operation = "native LoadArea, AutoSaveMode.None; mandatory exit; no save writes" });
                foreach (int frame in CircleTerrainLoad(entry.Area, entry)) yield return frame;
                var modes = Enum.GetValues(typeof(GameModeType)).Cast<GameModeType>().Where(game.IsModeActive).ToArray();
                CircleUiCapture("terrain-native-mode", new { current = game.CurrentMode.ToString(), game.IsPaused,
                    active = modes.Select(mode => mode.ToString()).ToArray(), dialog = Kingmaker.UI.DialogMessageBox.Instance.IsShown });
                if (!modes.Contains(GameModeType.Default) || !game.IsPaused ||
                    modes.Any(mode => mode != GameModeType.Default && mode != GameModeType.Pause) ||
                    Kingmaker.UI.DialogMessageBox.Instance.IsShown || _workingSaveSmoke.WriteObserved)
                    throw new InvalidOperationException("Terrain scene must contain only native Default/Pause, no dialog and no writes.");
                var actors = new List<UnitEntityData>(); var prototypes = new List<BlueprintUnit>();
                var existingAreas = game.State.AreaEffects.All.ToArray(); var existingUnits = game.State.Units.All.ToArray();
                var localItems = game.Player.Inventory.Items.ToArray(); var localCounts = localItems.Select(item => item.Count).ToArray();
                var starterDeltas = new Dictionary<ItemEntity, int>();
                TeleportResourceFixtureOwner fixture = null;
                try {
                    var anchor = game.Player.Party.First(unit => unit.IsInGame && unit.View != null);
                    var caster = CircleSpawn("TerrainCaster", anchor.Position, anchor, actors, prototypes);
                    var bearer = CircleSpawn("TerrainBearer", anchor.Position + new Vector3(.5f, 0, 0), anchor, actors, prototypes);
                    game.Player.PartyCharacters.Add(caster); game.Player.PartyCharacters.Add(bearer);
                    game.Player.InvalidateCharacterLists(); game.Player.UpdateCharacterLists();
                    // Capture only synchronous positive deltas from these exact
                    // owned spawns/party registrations, as in the UI fixture.
                    foreach (var item in game.Player.Inventory.Items) {
                        int index = Array.IndexOf(localItems, item);
                        int added = item.Count - (index < 0 ? 0 : localCounts[index]);
                        if (added > 0) starterDeltas.Add(item, added);
                    }
                    caster.Stats.Charisma.BaseValue = 30;
                    fixture = new TeleportResourceFixtureOwner(caster);
                    var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                        "b3a505fb61437dc4097f43c3f8f9a4cf", "native terrain Sorcerer book");
                    var book = fixture.AddBook(sorcerer.Spellbook, 8);
                    book.AddKnown(3, Blueprints.MagicCircleBlueprints.Family, true); book.Rest();
                    CircleSynchronize(actors);
                    foreach (int frame in CircleTerrainObservation(caster, bearer, book, actors)) yield return frame;
                }
                finally {
                    foreach (var actor in actors) foreach (var circle in BlueprintBootstrap.MagicCircles)
                        foreach (var carrier in CircleBuffs(actor, circle.Carrier)) carrier.Remove();
                    foreach (var area in game.State.AreaEffects.All.Except(existingAreas).ToArray()) {
                        if (!BlueprintBootstrap.MagicCircles.Any(circle => circle.Area == area.Blueprint))
                            throw new InvalidOperationException("Unexpected foreign area in terrain fixture.");
                        area.ForceEnd(); area.Tick();
                    }
                    fixture?.Restore();
                    foreach (var actor in actors) { game.Player.PartyCharacters.RemoveAll(value => value.UniqueId == actor.UniqueId); actor.Destroy(); }
                    game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
                    foreach (var prototype in prototypes) UnityEngine.Object.Destroy(prototype);
                    game.Player.InvalidateCharacterLists(); game.Player.UpdateCharacterLists();
                    foreach (var delta in starterDeltas) {
                        if (!ReferenceEquals(delta.Key.Collection, game.Player.Inventory) || delta.Key.Count < delta.Value)
                            throw new InvalidOperationException("Terrain owned starter-item boundary changed.");
                        game.Player.Inventory.Remove(delta.Key, delta.Value);
                    }
                    CircleUiAssert("terrain-local-fixture-cleanup", "only request-owned actors and areas removed", "area=" + game.CurrentlyLoadedArea.AssetGuid,
                        game.State.Units.All.SequenceEqual(existingUnits) && game.State.AreaEffects.All.SequenceEqual(existingAreas) &&
                        game.Player.Party.SequenceEqual(party) && game.Player.Inventory.Items.SequenceEqual(localItems) &&
                        localItems.Select((item, index) => item.Count == localCounts[index]).All(value => value) && !_workingSaveSmoke.WriteObserved);
                }
                foreach (int frame in CircleTerrainLoad(origin, null)) yield return frame;
                for (int index = 0; index < party.Length; index++) party[index].Translocate(positions[index], null);
                camera.ScrollToImmediately(cameraTarget); camera.transform.position = cameraPosition;
                // Native ScrollToImmediately reprojects and rounded X by 13um
                // after the scene round trip. Restore the captured request-owned
                // target exactly, rather than loosening the restoration oracle.
                typeof(Kingmaker.View.CameraRig).GetField("m_TargetPosition", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(camera, cameraTarget);
                CircleUiCapture("terrain-origin-restoration-detail", new {
                    area = game.CurrentlyLoadedArea == origin, party = game.Player.Party.SequenceEqual(party),
                    positions = party.Select((unit, index) => new { expected = positions[index], actual = unit.Position }).ToArray(),
                    inventory = game.Player.Inventory.Items.SequenceEqual(inventory),
                    counts = inventory.Select((item, index) => new { expected = counts[index], actual = item.Count }).ToArray(),
                    expectedMoney = money, actualMoney = game.Player.Money,
                    expectedCamera = cameraPosition, actualCamera = camera.transform.position,
                    expectedTarget = cameraTarget, actualTarget = camera.GetPosition(), _workingSaveSmoke.WriteObserved });
                CircleUiAssert("terrain-origin-restored", "native return to exact starting area with original party and no save writes",
                    "area=" + game.CurrentlyLoadedArea.AssetGuid, game.CurrentlyLoadedArea == origin &&
                    game.Player.Party.SequenceEqual(party) && party.Select((unit, index) => unit.Position == positions[index]).All(value => value) &&
                    game.Player.Inventory.Items.SequenceEqual(inventory) && inventory.Select((item, index) => item.Count == counts[index]).All(value => value) &&
                    game.Player.Money == money && camera.GetPosition() == cameraTarget && camera.transform.position == cameraPosition &&
                    !_workingSaveSmoke.WriteObserved);
            }
            finally {
                _circleUiScreens.Dispose(); Application.logMessageReceived -= ObserveCircleUiException;
                CircleUiAssert("terrain-exceptions", "no native or mod exceptions in scene observation", "count=" + _circleUiExceptions.Count, _circleUiExceptions.Count == 0);
            }
        }

        private IEnumerable<int> CircleTerrainLoad(BlueprintArea area, BlueprintAreaEnterPoint entry)
        {
            var scene = new CircleSceneObservation(); scene.Start();
            try {
                typeof(Game).GetMethod("LoadArea", BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new[] { typeof(BlueprintArea), typeof(BlueprintAreaEnterPoint), typeof(AutoSaveMode), typeof(bool), typeof(SaveInfo) }, null)
                    .Invoke(Game.Instance, new object[] { area, entry, AutoSaveMode.None, false, null });
                while (!scene.Ready) { Game.Instance.IsPaused = true; yield return 0; }
                CircleUiCapture("terrain-scene-loaded", new { area = Game.Instance.CurrentlyLoadedArea.AssetGuid, scene.Events });
                if (Game.Instance.CurrentlyLoadedArea != area || _workingSaveSmoke.WriteObserved)
                    throw new InvalidOperationException("Native terrain load destination/write boundary mismatch.");
            }
            finally { scene.Stop(); }
        }

        // Guarded UI fixture only. Bounded observation of the installed scene;
        // no fabricated floor, altered collision, save writes or production scan.
        private struct CircleTerrainSample
        {
            internal Vector3 point, normal;
            internal GameObject collider;
        }

        private static bool CircleTerrainHit(Vector3 point, out CircleTerrainSample hit)
        {
            // Native GeometryUtils.ProjectToGround mask; keep the vertical ray
            // local to the bearer instead of projecting from above the building.
            // Test-only reflection keeps the optional PhysicsModule out of the
            // qualified build-reference/package contract. This invokes Unity's
            // real query; no substitute terrain or expected height is supplied.
            var physics = Type.GetType("UnityEngine.Physics, UnityEngine.PhysicsModule", true);
            var method = physics.GetMethods(BindingFlags.Public | BindingFlags.Static).Single(value => value.Name == "Raycast" &&
                value.GetParameters().Length == 6 && value.GetParameters()[0].ParameterType == typeof(Vector3) &&
                value.GetParameters()[2].ParameterType.IsByRef);
            var args = new object[] { point + Vector3.up * 7, Vector3.down, null, 14f, 2097409,
                Enum.Parse(method.GetParameters()[5].ParameterType, "Ignore") };
            bool found = (bool)method.Invoke(null, args); hit = new CircleTerrainSample();
            if (!found) return false;
            var native = args[2]; var type = native.GetType();
            hit.point = (Vector3)type.GetProperty("point").GetValue(native, null);
            hit.normal = (Vector3)type.GetProperty("normal").GetValue(native, null);
            hit.collider = ((Component)type.GetProperty("collider").GetValue(native, null)).gameObject;
            return hit.normal.y > .2f;
        }

        private static bool CircleProjectedBoundary(AreaEffectEntityData area, out string evidence, bool levelGround = false)
        {
            var ring = area.View.GetComponentInChildren<MagicCircleRadiusVisual>(true);
            float low = float.MaxValue, high = float.MinValue, margin = float.MaxValue, radiusError = 0;
            int found = 0;
            for (int index = 0; index < 96; index++) {
                float angle = index * Mathf.PI * 2 / 96;
                var point = area.Position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * area.Blueprint.Size.Meters;
                CircleTerrainSample ground;
                if (!CircleTerrainHit(point, out ground)) continue;
                found++; var projected = ring.transform.InverseTransformPoint(ground.point);
                low = Math.Min(low, ground.point.y); high = Math.Max(high, ground.point.y);
                margin = Math.Min(margin, .5f - Math.Abs(projected.y));
                radiusError = Math.Max(radiusError, Math.Abs(new Vector2(projected.x, projected.z).magnitude - .5f));
            }
            var renderer = TeleportationCastingCamera().Camera.GetComponent<ScreenSpaceDecalRenderer>();
            var visible = renderer == null ? null : (List<ScreenSpaceDecal>)typeof(ScreenSpaceDecalRenderer)
                .GetField("m_VisibleDecals", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(renderer);
            bool submitted = visible != null && visible.Contains(ring) && ScreenSpaceDecalRenderer.IsGUIDecalsVisible;
            evidence = CircleUiState(new { center = area.Position, found, surfaceSamples = 96,
                groundHeightRange = high - low, minimumVolumeMargin = margin, radiusUvError = radiusError,
                submittedToNativeCamera = submitted, renderer = ring.GetInstanceID(), material = ring.SharedMaterial.GetInstanceID() })
                .ToString(Newtonsoft.Json.Formatting.None);
            // The rendered depth surface determines decal height. Independently
            // measured collision surfaces prove volume reach and X/Z mapping;
            // framebuffer inspection separately proves visible tread coverage.
            return CircleBoundaryMatches(area) && found == 96 && margin > .05f && radiusError < .001f && submitted &&
                (!levelGround || high - low < .025f);
        }

        private IEnumerable<int> CircleTerrainObservation(UnitEntityData caster, UnitEntityData bearer,
            Spellbook book, IList<UnitEntityData> actors)
        {
            var originalCaster = caster.Position; var originalBearer = bearer.Position;
            var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
            // Fixed authored route measured on the unchanged renderer in
            // native run 20260921T0500003919567Z. No scene-wide search is needed
            // in the regression: validate these real surfaces and fail closed.
            var lower = new Vector3(-21f, -4.70963669f, 2.95200133f);
            var slope = new Vector3(-21f, -4.62224674f, 6.00000143f);
            var upper = new Vector3(-23.534317f, -1.98248911f, 7.69337559f);
            foreach (var site in new[] { lower, slope, upper }) {
                CircleTerrainSample hit; var node = AstarPath.active.GetNearest(site);
                if (!CircleTerrainHit(site, out hit) || (hit.point - site).sqrMagnitude > .0001f ||
                    node.node == null || !node.node.Walkable ||
                    new Vector2(node.clampedPosition.x - site.x, node.clampedPosition.z - site.z).magnitude > .35f ||
                    Math.Abs(node.clampedPosition.y - site.y) > .5f)
                    throw new InvalidOperationException("The measured native staircase surface/navigation contract changed.");
            }
            CircleUiCapture("terrain-fixed-route", new { area = Game.Instance.CurrentlyLoadedArea.AssetGuid,
                lower, slope, upper, rise = upper.y - lower.y, sourceRun = "20260921T0500003919567Z" });
            try {
                caster.Translocate(originalBearer, null); bearer.Translocate(originalBearer, null); CircleSynchronize(actors);
                book.Rest();
                var spell = book.Blueprint.Spontaneous ? CircleGroupedVariant(new AbilityData(Blueprints.MagicCircleBlueprints.Family, book), circle.Spell) :
                    CirclePreparedVariant(book.GetMemorizedSpells(3).First(value => value.Available && value.Spell.Blueprint == Blueprints.MagicCircleBlueprints.Family), circle.Spell);
                CircleCast(caster, bearer, spell, _circleUiDiagnostics);
                var carrier = CircleBuffs(bearer, circle.Carrier).Single(); var area = CircleArea(carrier);
                var ring = area.View.GetComponentInChildren<MagicCircleRadiusVisual>(true);
                int renderer = ring.Boundary.GetInstanceID(); int material = ring.Boundary.SharedMaterial.GetInstanceID();
                var deadline = carrier.EndTime;
                var sites = new[] { originalBearer, lower, slope, upper, slope, lower };
                for (int site = 0; site < sites.Length; site++) {
                    bearer.Translocate(sites[site], null); caster.Translocate(sites[site], null); CircleRefresh(area, actors);
                    var camera = TeleportationCastingCamera(); camera.ScrollToImmediately(sites[site]);
                    for (int frame = 0; frame < 24; frame++) yield return 0;
                    var decal = ring.Boundary;
                    var surfaces = Enumerable.Range(0, 96).Select(index => {
                        float angle = index * Mathf.PI * 2 / 96;
                        var point = area.Position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * area.Blueprint.Size.Meters;
                        CircleTerrainSample hit; bool found = CircleTerrainHit(point, out hit);
                        return new { index, horizontalSample = point, found, ground = hit.point, normal = hit.normal,
                            collider = hit.collider == null ? null : hit.collider.name,
                            layer = hit.collider == null ? null : LayerMask.LayerToName(hit.collider.layer),
                            projectionLocal = decal.transform.InverseTransformPoint(hit.point),
                            viewport = camera.Camera.WorldToViewportPoint(hit.point) };
                    }).ToArray();
                    var state = new { site, center = area.Position, bearer = bearer.Position, radius = area.Blueprint.Size.Meters,
                        area = area.UniqueId, renderer, shader = decal.SharedMaterial.shader.name,
                        texture = decal.SharedMaterial.mainTexture.name, projectionScale = decal.transform.lossyScale,
                        zTest = decal.SharedMaterial.GetFloat("_ZTest"), cull = decal.SharedMaterial.GetFloat("_CullMode"),
                        camera = new { position = camera.Camera.transform.position, rotation = camera.Camera.transform.eulerAngles,
                            camera.Camera.fieldOfView, camera.Camera.nearClipPlane, camera.Camera.farClipPlane }, surfaces };
                    CircleUiCapture("terrain-site-" + site, state);
                    string clearanceEvidence;
                    bool grounded = CircleProjectedBoundary(area, out clearanceEvidence);
                    CircleUiAssert("terrain-boundary-" + site, "native depth-projected ten-foot boundary covers the measured surfaces with original renderer/material/deadline",
                        clearanceEvidence, grounded && decal.GetInstanceID() == renderer &&
                        decal.SharedMaterial.GetInstanceID() == material && carrier.EndTime == deadline && !area.IsEnded);
                    foreach (int frame in _circleUiScreens.Capture("terrain-site-" + site, CircleUiState(state),
                        () => carrier.Active && !area.IsEnded && !_workingSaveSmoke.WriteObserved)) yield return frame;
                    if (site == 3) {
                        // The default camera puts the palisade between the
                        // camera and much of the downhill circumference. Two
                        // native camera angles distinguish that occlusion from
                        // missing projection onto exposed treads/ground.
                        var rotation = camera.transform.rotation;
                        try {
                            foreach (float turn in new[] { 90f, 180f }) {
                                camera.SetRotation(rotation.eulerAngles.y + turn);
                                camera.ScrollToImmediately(sites[site]);
                                for (int frame = 0; frame < 24; frame++) yield return 0;
                                string angleEvidence;
                                bool projected = CircleProjectedBoundary(area, out angleEvidence);
                                CircleUiAssert("terrain-upper-angle-" + turn, "native boundary remains submitted under an alternate camera angle",
                                    angleEvidence, projected && carrier.EndTime == deadline);
                                var alternate = new { turn, center = area.Position,
                                    camera = new { position = camera.Camera.transform.position,
                                        rotation = camera.Camera.transform.eulerAngles, camera.Camera.fieldOfView },
                                    samples = surfaces.Select(surface => new { surface.index, surface.ground,
                                        viewport = camera.Camera.WorldToViewportPoint(surface.ground) }).ToArray() };
                                CircleUiCapture("terrain-upper-angle-" + turn, alternate);
                                foreach (int frame in _circleUiScreens.Capture("terrain-upper-angle-" + turn,
                                    CircleUiState(alternate), () => projected && !_workingSaveSmoke.WriteObserved)) yield return frame;
                            }
                        }
                        finally { camera.transform.rotation = rotation; camera.ScrollToImmediately(sites[site]); }
                    }
                }
                // Exercise the native moving area and decal bounds, in both
                // directions across the surveyed 2.727m elevation change.
                // Each frame position uses the real stair collision surface;
                // no fabricated floor, renderer call or aura rewrite is used.
                for (int direction = 0; direction < 2; direction++) {
                    var from = direction == 0 ? lower : upper; var to = direction == 0 ? upper : lower;
                    for (int step = 0; step <= 24; step++) {
                        CircleTerrainSample floor;
                        if (!CircleTerrainHit(Vector3.Lerp(from, to, step / 24f), out floor))
                            throw new InvalidOperationException("Measured stair path lost its native ground.");
                        bearer.Translocate(floor.point, null); caster.Translocate(floor.point, null); CircleRefresh(area, actors);
                        for (int frame = 0; frame < 2; frame++) yield return 0;
                        string evidence; bool grounded = CircleProjectedBoundary(area, out evidence);
                        CircleUiAssert("terrain-motion-" + direction + "-" + step, "moving native projection covers the stair surface without resource recreation or duration restart",
                            evidence, grounded && ring.Boundary.GetInstanceID() == renderer &&
                            ring.Boundary.SharedMaterial.GetInstanceID() == material && carrier.EndTime == deadline);
                        if (step == 12) {
                            var camera = TeleportationCastingCamera(); camera.ScrollToImmediately(floor.point);
                            for (int frame = 0; frame < 12; frame++) yield return 0;
                            foreach (int frame in _circleUiScreens.Capture("terrain-moving-stairs-" + direction,
                                CircleUiState(new { center = area.Position, evidence, direction, step }),
                                () => carrier.Active && !area.IsEnded && !_workingSaveSmoke.WriteObserved)) yield return frame;
                        }
                    }
                }
                carrier.Remove(); CircleRefresh(area, actors);
                CircleUiAssert("terrain-owned-cleanup", "observed boundary disappears with its carrier", "area=" + area.UniqueId, CircleBoundaryGone(area));
            }
            finally {
                foreach (var carrier in CircleBuffs(bearer, circle.Carrier)) carrier.Remove();
                caster.Translocate(originalCaster, null); bearer.Translocate(originalBearer, null); CircleSynchronize(actors);
            }
        }

        private void CircleNativeProjectionAudit()
        {
            var source = Game.Instance.UI.AbilityTargetSelection?.GetComponent<Kingmaker.UI.AbilityTarget.AbilityAoERange>();
            var ranges = source == null ? new Kingmaker.UI.AbilityTarget.AbilityAoERange[0] : new[] { source };
            CircleUiCapture("native-projection-materials", ranges.Select(range => new { range.name,
                root = range.Range.name, scale = range.Range.transform.localScale,
                components = range.Range.GetComponentsInChildren<Component>(true).Select(value => value.GetType().FullName).Distinct().ToArray(),
                decals = range.Range.GetComponentsInChildren<ScreenSpaceDecal>(true).Select(decal => new {
                    decal.name, type = decal.GetType().FullName, scale = decal.transform.localScale, shader = decal.SharedMaterial?.shader?.name,
                    texture = decal.SharedMaterial?.mainTexture?.name,
                    properties = new[] { "_MainTex", "_Color", "_EmissionColor", "_ZTest", "_CullMode", "_NormalCutoff", "_NormalThreshold", "_AlphaScale" }
                        .Select(property => new { property, present = decal.SharedMaterial != null && decal.SharedMaterial.HasProperty(property) }).ToArray()
                }).ToArray() }).ToArray());
        }
    }

}
