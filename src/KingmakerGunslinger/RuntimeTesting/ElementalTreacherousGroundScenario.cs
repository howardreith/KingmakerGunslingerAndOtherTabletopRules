using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Visual.FogOfWar;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathfinding;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class ElementalTreacherousScenario
    {
        private static void GroundTargets(RuntimeTestRequest request, ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics, ICollection<string> files)
        {
            var rows = new JArray();
            var world = Game.Instance.State.Units.All.ToArray();
            var originalAreas = Game.Instance.State.AreaEffects.All.ToArray();
            var race = BlueprintBootstrap.ElementalRaces.Oread;
            var trait = race.AlternateTraits.Require(ElementalAlternateTraitId.TreacherousEarth);
            var ability = trait.Mechanics().OfType<BlueprintAbility>().Single();
            var terrain = trait.Mechanics().OfType<BlueprintBuff>().Single();
            var areaBlueprint = trait.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            // One graph contains a lower floor, an upper floor and bridge,
            // a nearby disconnected landing, and a sloped walkable surface.
            // These are native request-owned navigation triangles, not material
            // labels or replacements for authored-area qualification.
            foreach (var strip in new[] { new[] { -2f, -.3f }, new[] { -.3f, .3f }, new[] { .3f, 2f } })
            {
                GroundQuad(vertices, triangles, -2, 2, strip[0], strip[1], 2, 2);
                GroundQuad(vertices, triangles, 4.5f, 6.5f, strip[0], strip[1], 2, 2);
            }
            GroundQuad(vertices, triangles, 2, 4.5f, -.3f, .3f, 2, 2);
            GroundQuad(vertices, triangles, 2.5f, 4.5f, .5f, 2, 2, 2);
            GroundQuad(vertices, triangles, -12, -8, -2, 2, 2, 4);
            var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
            var provider = CoreProvider(trait);
            ElementalNereidScenario.NativeNereidMovement navigation = null;
            var areas = new List<AreaEffectEntityData>();
            try
            {
                var caster = fixture.Caster;
                var subject = fixture.SpawnFixtureUnit(null, caster.Blueprint.Faction,
                    new Vector3(1, 0, 0), "TreacherousGroundSubject");
                caster.Descriptor.AddFact(trait.Marker); caster.Descriptor.AddFact(provider);
                navigation = new ElementalNereidScenario.NativeNereidMovement(rows, assertions,
                    vertices.ToArray(), triangles.ToArray(), physicalGround: true);
                PlaceTerrainActor(caster, Vector3.zero);
                var data = new AbilityData(ability, caster.Descriptor);
                Func<Vector3, bool> can = point => data.CanTarget(new TargetWrapper(point));
                Check(assertions, rows, "ground-flat-and-point-only",
                    can(new Vector3(1, 0, 0)) && !data.CanTarget(new TargetWrapper(subject)),
                    "unmodified production checker accepts real ground and rejects a creature target");
                ElementalGroundPoint contact;
                Check(assertions, rows, "ground-contact-height-and-invalid-controls",
                    ElementalTreacherousGroundRuntime.TryGround(new Vector3(1, .09f, 0), out contact) &&
                    Math.Abs(contact.Position.y) < .001f &&
                    !can(new Vector3(1, .11f, 0)) && !can(new Vector3(1, -1, 0)) &&
                    !can(new Vector3(float.NaN, 0, 0)) && !can(new Vector3(float.PositiveInfinity, 0, 0)) &&
                    !ElementalTreacherousGroundRuntime.TryGround(new Vector3(17, 0, 0), out contact),
                    "contact is a verified 3D triangle point; floating, below-floor, nonfinite and off-mesh points fail closed");
                float reach = data.GetApproachDistance();
                Check(assertions, rows, "ground-native-touch-boundary",
                    can(new Vector3(0, 0, reach - .02f)) && !can(new Vector3(0, 0, reach + .02f)) &&
                    !can(new Vector3(0, 0, 8)),
                    "native touch approach distance=" + reach + "; no long-range placement");
                ElementalGroundPoint lower, upper;
                bool foundLower = ElementalTreacherousGroundRuntime.TryGround(Vector3.zero, out lower);
                bool foundUpper = ElementalTreacherousGroundRuntime.TryGround(new Vector3(0, 2, 0), out upper);
                GraphHitInfo hit;
                var trace = new List<GraphNode>();
                bool nativeXZClear = foundLower && foundUpper &&
                    !((IRaycastableGraph)lower.Graph).Linecast(lower.Position, upper.Position, lower.Node, out hit, trace);
                Check(assertions, rows, "ground-stacked-floor-native-negative-control",
                    foundLower && foundUpper && ReferenceEquals(lower.Graph, upper.Graph) &&
                    !ReferenceEquals(lower.Node, upper.Node) && nativeXZClear &&
                    !ElementalTreacherousGroundRuntime.ConnectedSurface(lower, upper) && !can(new Vector3(0, 2, 0)),
                    "native XZ linecast alone reports clear; production terminal-height validation rejects the other floor");
                PlaceTerrainActor(caster, new Vector3(0, 2, 0));
                Check(assertions, rows, "ground-upper-floor-positive",
                    can(new Vector3(1, 2, 0)) && !can(Vector3.zero),
                    "upper walkable floor is selectable from that floor; the floor below remains ineligible");
                PlaceTerrainActor(caster, new Vector3(1.8f, 2, 1));
                Check(assertions, rows, "ground-gap-and-disconnected-landing",
                    !can(new Vector3(2.25f, 2, 1)) && !can(new Vector3(3, 2, 1)),
                    "no ground in gap; nearby landing has no unblocked direct local ground path");
                PlaceTerrainActor(caster, new Vector3(1.8f, 2, 0));
                Check(assertions, rows, "ground-bridge-portals",
                    can(new Vector3(3, 2, 0)) && !can(new Vector3(3, 0, 0)),
                    "connected bridge triangles preserve height; the ground beneath the bridge is a negative control");
                PlaceTerrainActor(caster, new Vector3(-11, 2.5f, 0));
                Check(assertions, rows, "ground-slope-positive-and-height-negative",
                    can(new Vector3(-10, 3, 0)) && !can(new Vector3(-10, 2.5f, 0)),
                    "native 3D sloped surface is accepted at its actual height, without a nearest-floor guess");
                // Pick the interior of one triangle, not the shared diagonal
                // where the other still-walkable triangle legitimately qualifies.
                var nonwalkablePoint = new Vector3(-10, 3, .6f);
                ElementalGroundPoint slope;
                if (!ElementalTreacherousGroundRuntime.TryGround(nonwalkablePoint, out slope))
                    throw new InvalidOperationException("Native slope fixture lost its triangle.");
                bool walkable = slope.Node.Walkable;
                try
                {
                    slope.Node.Walkable = false;
                    Check(assertions, rows, "ground-native-nonwalkable-control",
                        !can(nonwalkablePoint), "interior of a nonwalkable native triangle cannot be legalized by closeness or visibility");
                }
                finally { slope.Node.Walkable = walkable; }
                PlaceTerrainActor(caster, Vector3.zero);
                GroundObstruction(caster, data, rows, assertions);
                var lowerArea = Spawn(caster, ability, areaBlueprint, Vector3.zero); areas.Add(lowerArea);
                PlaceTerrainActor(subject, new Vector3(0, 2, 0)); Tick(lowerArea);
                Check(assertions, rows, "ground-area-other-floor-excluded",
                    lowerArea.UnitsInside.Contains(subject) && !subject.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain),
                    "native cylinder includes the other floor; owned terrain logic rejects it");
                PlaceTerrainActor(subject, new Vector3(1, 0, 0)); Tick(lowerArea);
                Check(assertions, rows, "ground-area-return-to-source-floor",
                    subject.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, terrain) &&
                        value.SourceAreaEffectId == lowerArea.UniqueId) == 1,
                    "returning to the patch's actual floor restores exactly one owned terrain fact");
                var upperArea = Spawn(caster, ability, areaBlueprint, new Vector3(0, 2, 0)); areas.Add(upperArea);
                PlaceTerrainActor(subject, new Vector3(0, 2, 0)); Tick(lowerArea); Tick(upperArea);
                Check(assertions, rows, "ground-area-floor-transition-ownership",
                    subject.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, terrain)) == 1 &&
                    subject.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, terrain) &&
                        value.SourceAreaEffectId == upperArea.UniqueId),
                    "moving between overlapping cylinders removes only the previous floor's terrain source");
                lowerArea.ForceEnd(); Tick(lowerArea);
                Check(assertions, rows, "ground-area-other-source-expiry",
                    subject.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, terrain) &&
                        value.SourceAreaEffectId == upperArea.UniqueId),
                    "expiring the lower area preserves the legitimate upper area");
                upperArea.ForceEnd(); Tick(upperArea);
                Check(assertions, rows, "ground-area-final-cleanup",
                    !subject.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, terrain)),
                    "both source areas expired; no terrain fact remains");
            }
            finally
            {
                foreach (var area in areas) { area.ForceEnd(); Tick(area); }
                Game.Instance.EntityCreator.Tick();
                if (navigation != null) navigation.Dispose();
                fixture.Dispose(); UnityEngine.Object.Destroy(provider);
                Check(assertions, rows, "ground-fixture-exact-restoration",
                    world.SequenceEqual(Game.Instance.State.Units.All) &&
                    originalAreas.SequenceEqual(Game.Instance.State.AreaEffects.All) &&
                    fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                    fixture.NativeObservationReleased && fixture.AreaContextRestored && fixture.PlayerContextRestored,
                    "native fixture errors=" + fixture.NativeErrors + "; exceptions=" + fixture.NativeExceptions +
                    "; exact units/areas/navigation/player restoration");
                string path = System.IO.Path.Combine(request.EvidenceDirectory, "elemental-treacherous-ground.json");
                File.WriteAllText(path, new JObject { { "saveStateTouched", false },
                    { "qualificationBoundary", "production checker on request-owned native triangles: contact, slope, bridge, gap, stacked floors, obstruction, range and owned area membership; authored-area and complete native Player/save qualification remain separate" },
                    { "observations", rows } }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void GroundQuad(List<Vector3> vertices, List<int> triangles,
            float minX, float maxX, float minZ, float maxZ, float leftHeight, float rightHeight)
        {
            int start = vertices.Count;
            vertices.AddRange(new[] { new Vector3(minX, leftHeight, minZ), new Vector3(minX, leftHeight, maxZ),
                new Vector3(maxX, rightHeight, maxZ), new Vector3(maxX, rightHeight, minZ) });
            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }

        private static void GroundObstruction(UnitEntityData caster, AbilityData data, JArray rows,
            ICollection<RuntimeTestAssertion> assertions)
        {
            var geometry = LineOfSightGeometry.Instance;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var cells = typeof(LineOfSightGeometry).GetField("m_Cells", flags);
            var bounds = typeof(LineOfSightGeometry).GetField("m_Bounds", flags);
            var updated = typeof(LineOfSightGeometry).GetProperty("LastUpdateTime", flags);
            var segments = typeof(LineOfSightGeometry).GetField("s_CellsForSegment", BindingFlags.Static | BindingFlags.NonPublic);
            if (geometry == null || cells == null || bounds == null || updated == null || segments == null ||
                FogOfWarBlocker.All.Count != 0)
                throw new InvalidOperationException("Ground obstruction requires the exact empty request-owned geometry.");
            object oldCells = cells.GetValue(geometry), oldBounds = bounds.GetValue(geometry), oldUpdated = updated.GetValue(geometry, null);
            var work = (IList)segments.GetValue(null);
            var oldWork = work == null ? Array.Empty<object>() : work.Cast<object>().ToArray();
            var obstacle = new GameObject("KMG_Runtime_Treacherous_Ground_Obstruction"); obstacle.SetActive(false);
            var blocker = obstacle.AddComponent<FogOfWarBlocker>(); blocker.HeightMinMax = new Vector2(-5, 20);
            var target = new TargetWrapper(new Vector3(1, 0, 0));
            try
            {
                geometry.Init(new Bounds(Vector3.zero, new Vector3(40, 10, 40)));
                geometry.AddPointsList(new[] { new Vector2(.5f, -5), new Vector2(.5f, 5) }, false, blocker);
                Check(assertions, rows, "ground-native-line-of-effect-blocked",
                    geometry.HasObstacle(caster.Position, target.Point, 0) && !data.CanTarget(target),
                    "native wall geometry blocks a nearby otherwise valid ground target");
                geometry.Init(new Bounds(Vector3.zero, new Vector3(40, 10, 40)));
                Check(assertions, rows, "ground-native-line-of-effect-restored",
                    !geometry.HasObstacle(caster.Position, target.Point, 0) && data.CanTarget(target),
                    "removing the request-owned wall restores eligibility without changing navigation or the blueprint");
            }
            finally
            {
                cells.SetValue(geometry, oldCells); bounds.SetValue(geometry, oldBounds); updated.SetValue(geometry, oldUpdated, null);
                if (work != null) { work.Clear(); foreach (var value in oldWork) work.Add(value); }
                UnityEngine.Object.DestroyImmediate(obstacle);
                Check(assertions, rows, "ground-obstruction-exact-restoration",
                    ReferenceEquals(cells.GetValue(geometry), oldCells) && bounds.GetValue(geometry).Equals(oldBounds) &&
                    updated.GetValue(geometry, null).Equals(oldUpdated) && FogOfWarBlocker.All.Count == 0 &&
                    (work == null || work.Cast<object>().SequenceEqual(oldWork)),
                    "exact native geometry references, update time and scratch entries restored");
            }
        }
    }
}
