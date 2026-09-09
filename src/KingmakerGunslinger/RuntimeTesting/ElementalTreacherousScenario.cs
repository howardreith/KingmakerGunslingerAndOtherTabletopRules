using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class ElementalTreacherousScenario
    {
        internal static RuntimeTestResult Run(ModContext context, RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var files = new List<string>();
            string failure = string.Empty;
            try { GroundTargets(request, assertions, diagnostics, files); Exercise(request, assertions, diagnostics, files); Actions(request, assertions, diagnostics, files); }
            catch (Exception exception) { failure = exception.ToString(); diagnostics.Add(failure); }
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId, Scenario = request.Scenario,
                Status = failure.Length == 0 && assertions.Count > 0 &&
                    assertions.All(value => value.Status == RuntimeTestStatuses.Pass)
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" + assembly.ManifestModule.ModuleVersionId +
                    ";pid=" + Process.GetCurrentProcess().Id,
                GitCommit = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                    .OfType<AssemblyMetadataAttribute>().Single(value => value.Key == "GitCommit").Value,
                GameVersion = Application.version, StartUtc = started.ToString("o"), EndUtc = string.Empty,
                DurationMilliseconds = (long)(DateTime.UtcNow - started).TotalMilliseconds,
                Assertions = assertions, Diagnostics = diagnostics, Warnings = new List<string>(),
                ExceptionSummary = failure, EvidenceFiles = files, EvidenceDirectory = request.EvidenceDirectory,
                AutomaticExitRequested = request.ExitAfterCompletion
            };
        }
        private static void Exercise(RuntimeTestRequest request, ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics, ICollection<string> files)
        {
            var rows = new JArray();
            var world = Game.Instance.State.Units.All.ToArray();
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            var clock = Game.Instance.Player.GameTime;
            var race = BlueprintBootstrap.ElementalRaces.Oread;
            var trait = race.AlternateTraits.Require(ElementalAlternateTraitId.TreacherousEarth);
            var ability = trait.Mechanics().OfType<BlueprintAbility>().Single();
            var terrain = trait.Mechanics().OfType<BlueprintBuff>().Single();
            var areaBlueprint = trait.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
            var created = new List<AreaEffectEntityData>();
            ElementalNereidScenario.NativeNereidMovement effectGround = null;
            var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
            var provider = ScriptableObject.CreateInstance<BlueprintFeature>();
            provider.name = "KMG_Runtime_Treacherous_Effect_Provider"; provider.Ranks = 1;
            provider.ComponentsArray = trait.Provider.ComponentsArray.Where(value =>
                !(value is ElementalAlternateTraitProviderController)).Select(value => UnityEngine.Object.Instantiate(value)).ToArray();
            try
            {
                var caster = fixture.Caster;
                var target = fixture.SpawnFixtureUnit(null, caster.Blueprint.Faction, new Vector3(1, 0, 0), "TerrainTarget");
                var other = fixture.SpawnFixtureUnit(null, caster.Blueprint.Faction, new Vector3(1, 0, .5f), "OtherTerrainSource");
                caster.Descriptor.AddFact(trait.Marker); caster.Descriptor.AddFact(provider);
                other.Descriptor.AddFact(trait.Marker);
                var fighter = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "48ac8db94d5de7645906c7d0ad3bcfbd", "terrain total-level fixture");
                ElementalSpellAffinityScenario.Advance(caster.Descriptor, fighter, 2);
                ElementalSpellAffinityScenario.Advance(other.Descriptor, fighter, 2);
                var data = new AbilityData(ability, caster.Descriptor);
                effectGround = new ElementalNereidScenario.NativeNereidMovement(rows, assertions, physicalGround: true);
                Check(assertions, rows, "owned-ground-effect-pending-publication",
                    !ElementalTreacherousPolicy.EligibilityQualified && !trait.Definition.IsPublished &&
                    ElementalTreacherousFactory.IsExactEffect(trait) &&
                    data.CanTarget(new TargetWrapper(new Vector3(1, 0, 0))),
                    "owned effect graph and native ground checker; full trait integration remains unpublished");
                float speed = target.CurrentSpeedMps;
                int hp = target.Damage;
                var area = Spawn(caster, ability, areaBlueprint, new Vector3(1, 0, 0)); created.Add(area); Tick(area);
                Check(assertions, rows, "native-terrain-effect",
                    target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain) &&
                    Math.Abs(target.CurrentSpeedMps - speed * .5f) < .0001f && target.Damage == hp &&
                    !target.Descriptor.State.HasCondition(UnitCondition.Prone) &&
                    !target.Descriptor.State.HasCondition(UnitCondition.Entangled),
                    "native movement speed=" + speed + "->" + target.CurrentSpeedMps + ";no damage/prone/entangle");
                Check(assertions, rows, "fixed-location-and-owner",
                    area.Position == new Vector3(1, 0, 0) && ReferenceEquals(area.Context.MaybeCaster, caster) &&
                    target.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, terrain) &&
                        value.SourceAreaEffectId == area.UniqueId) == 1,
                    "native fixed area position, caster context and owned terrain fact match");
                target.Position = new Vector3(1 + 10.Feet().Meters + target.View.Corpulence + .05f, 0, 0); Tick(area);
                Check(assertions, rows, "radius-outside", !target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain) &&
                    Math.Abs(target.CurrentSpeedMps - speed) < .0001f, "outside ten feet plus native body boundary restores speed");
                target.Position = new Vector3(1 + 10.Feet().Meters - .05f, 0, 0); Tick(area);
                Check(assertions, rows, "radius-inside", target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain),
                    "inside the native ten-foot shape applies difficult terrain");
                target.Position = new Vector3(1, 0, 0); Tick(area);
                var second = Spawn(other, ability, areaBlueprint, new Vector3(1, 0, 0)); created.Add(second); Tick(second);
                Check(assertions, rows, "native-overlap",
                    target.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, terrain)) == 2 &&
                    Math.Abs(target.CurrentSpeedMps - speed * .5f) < .0001f,
                    "two independently owned terrain facts; native speed penalty does not multiply");
                area.ForceEnd(); Tick(area);
                Check(assertions, rows, "one-source-expiration",
                    target.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, terrain)) == 1 &&
                    Math.Abs(target.CurrentSpeedMps - speed * .5f) < .0001f,
                    "ending one source preserves the other source and its native terrain effect");
                second.ForceEnd(); Tick(second);
                var feather = BlueprintBootstrap.ElementalRaces.Sylph.SlaAbility
                    .GetComponent<AbilityEffectRunAction>().Actions.Actions.OfType<ContextActionApplyBuff>().Single().Buff;
                var featherContext = new Kingmaker.UnitLogic.Mechanics.MechanicsContext(target,
                    target.Descriptor, feather, null, new TargetWrapper(target));
                var featherBuff = target.Descriptor.AddBuff(feather, featherContext, TimeSpan.FromMinutes(5));
                area = Spawn(caster, ability, areaBlueprint, new Vector3(1, 0, 0)); created.Add(area); Tick(area);
                Check(assertions, rows, "native-feather-step",
                    featherBuff != null && !target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain) &&
                    Math.Abs(target.CurrentSpeedMps - speed) < .0001f,
                    "exact released Sylph Feather Step buff preserves normal native movement speed");
                featherBuff.Remove(); area.ForceEnd(); Tick(area);
                area = Spawn(caster, ability, areaBlueprint, new Vector3(1, 0, 0)); created.Add(area); Tick(area);
                var started = Game.Instance.Player.GameTime;
                Game.Instance.Player.GameTime = started + TimeSpan.FromSeconds(119.9); Tick(area);
                Check(assertions, rows, "duration-before-deadline",
                    !area.IsEnded && target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain),
                    "two total levels retain terrain immediately before two minutes");
                Game.Instance.Player.GameTime = started + TimeSpan.FromSeconds(120.1); Tick(area);
                Check(assertions, rows, "duration-after-deadline",
                    area.IsEnded && !target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain) &&
                    Math.Abs(target.CurrentSpeedMps - speed) < .0001f,
                    "native fixed-area timer ends without restarting or leaving a terrain fact");
                area = Spawn(caster, ability, areaBlueprint, new Vector3(1, 0, 0)); created.Add(area); Tick(area);
                caster.Descriptor.RemoveFact(trait.Marker); Tick(area);
                Check(assertions, rows, "trait-removal-ownership",
                    area.IsEnded && !target.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, terrain)),
                    "exact trait removal ends only the caster's owned patch");
                effectGround.Dispose(); effectGround = null;
                NativeTerrainMovement(caster, target, other, ability, terrain, areaBlueprint, created, rows, assertions);
            }
            finally
            {
                foreach (var area in created) { area.ForceEnd(); Tick(area); }
                Game.Instance.EntityCreator.Tick();
                if (effectGround != null) effectGround.Dispose();
                fixture.Dispose(); UnityEngine.Object.Destroy(provider);
                Game.Instance.Player.GameTime = clock;
                Check(assertions, rows, "exact-cleanup",
                    world.SequenceEqual(Game.Instance.State.Units.All) &&
                    areasBefore.SequenceEqual(Game.Instance.State.AreaEffects.All) &&
                    fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                    fixture.NativeObservationReleased && fixture.AreaContextRestored && fixture.PlayerContextRestored,
                    "native errors=" + fixture.NativeErrors + ";exceptions=" + fixture.NativeExceptions +
                    ";world/areas/player/clock restored");
                string path = Path.Combine(request.EvidenceDirectory, "elemental-treacherous-effect.json");
                File.WriteAllText(path, new JObject { { "saveStateTouched", false },
                    { "qualificationBoundary", "native fixed-area effect and native movement over an owned navigation plane; native immunity/overlap costs; actual authored-area placement, creator/respec and persistence excluded; native command and ground fixtures reported separately" },
                    { "observations", rows }, { "diagnostics", new JArray(diagnostics) } }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }
        private static void PlaceTerrainActor(UnitEntityData unit, Vector3 position)
        {
            unit.Position = position; unit.View.transform.position = position;
        }
        private static void NativeTerrainMovement(UnitEntityData caster, UnitEntityData target, UnitEntityData other,
            BlueprintAbility ability, BlueprintBuff terrain, BlueprintAbilityAreaEffect blueprint,
            List<AreaEffectEntityData> created, JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            var actors = new[] { caster, target, other };
            var positions = actors.Select(value => value.Position).ToArray();
            var agent = target.View.AgentASP;
            var active = new List<AreaEffectEntityData>();
            Buff immunity = null;
            try
            {
                // The earlier removal check deliberately removed this owned marker.
                caster.Descriptor.AddFact(BlueprintBootstrap.ElementalRaces.Oread.AlternateTraits
                    .Require(ElementalAlternateTraitId.TreacherousEarth).Marker);
                PlaceTerrainActor(caster, new Vector3(-10, 0, 0));
                PlaceTerrainActor(other, new Vector3(10, 0, 0));
                PlaceTerrainActor(target, new Vector3(0, 0, -2.3f));
                using (var movement = new ElementalNereidScenario.NativeNereidMovement(rows, assertions, physicalGround: true))
                {
                    Func<string, int> traverse = label => {
                        agent.Stop(); target.Commands.InterruptAll(true); target.Commands.RemoveFinishedAndUpdateQueue();
                        Vector3 start = new Vector3(0, 0, -2.3f), end = new Vector3(0, 0, 2.3f);
                        PlaceTerrainActor(target, start);
                        foreach (var area in active) Tick(area);
                        // The main-menu fixture has no HUD pointer manager. This
                        // changes only the command's native presentation flag.
                        var command = new Kingmaker.UnitLogic.Commands.UnitMoveTo(end) { ShowTargetMarker = false };
                        target.Commands.Run(command);
                        agent.ForcePath(new Pathfinding.ForcedPath(new List<Vector3> { start, end }), .05f);
                        int ticks = 0;
                        while (Vector3.Dot(target.Position - start, Vector3.forward) < 4f && ticks < 240)
                        {
                            movement.Tick();
                            foreach (var area in active) Tick(area);
                            ticks++;
                        }
                        float progress = Vector3.Dot(target.Position - start, Vector3.forward);
                        bool arrived = ticks < 240 && progress >= 4f && progress < 4.25f &&
                            Math.Abs(target.Position.x - start.x) < .25f;
                        Check(assertions, rows, "native-traverse-" + label, arrived,
                            "nativeMovementTicks=" + ticks + ";deltaSeconds=.05;distance=" + Vector3.Distance(start, target.Position) +
                            ";progressAlongSegment=" + progress + ";position=" + target.Position + ";end=" + end +
                            ";nativeMoving=" + agent.IsReallyMoving + ";speedMps=" + target.CurrentSpeedMps +
                            ";difficultTerrain=" + target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain));
                        agent.Stop(); target.Commands.InterruptAll(true); target.Commands.RemoveFinishedAndUpdateQueue();
                        if (!arrived) throw new InvalidOperationException("Native terrain traversal did not complete: " + label);
                        return ticks;
                    };
                    int ordinary = traverse("ordinary");
                    var first = Spawn(caster, ability, blueprint, Vector3.zero); created.Add(first); active.Add(first); Tick(first);
                    int slowed = traverse("one-patch");
                    var second = Spawn(other, ability, blueprint, Vector3.zero); created.Add(second); active.Add(second); Tick(second);
                    int overlap = traverse("overlap");
                    first.ForceEnd(); Tick(first); active.Remove(first);
                    int oneEnded = traverse("one-source-ended");
                    var feather = BlueprintBootstrap.ElementalRaces.Sylph.SlaAbility.GetComponent<AbilityEffectRunAction>()
                        .Actions.Actions.OfType<ContextActionApplyBuff>().Single().Buff;
                    var featherContext = new Kingmaker.UnitLogic.Mechanics.MechanicsContext(target, target.Descriptor, feather, null, new TargetWrapper(target));
                    immunity = target.Descriptor.AddBuff(feather, featherContext, TimeSpan.FromMinutes(5));
                    int featherTicks = traverse("feather-step");
                    immunity.Remove(); immunity = null;
                    var wings = BlueprintBootstrap.ElementalFeats.RequireSymbol<BlueprintBuff>(ElementalRaceIdentityCatalog.WingsOfAirBuff);
                    var wingContext = new Kingmaker.UnitLogic.Mechanics.MechanicsContext(target, target.Descriptor, wings, null, new TargetWrapper(target));
                    immunity = target.Descriptor.AddBuff(wings, wingContext, TimeSpan.FromMinutes(5));
                    int flightTicks = traverse("released-flight");
                    Check(assertions, rows, "native-terrain-movement-cost-and-immunities",
                        slowed > ordinary * 1.6 && slowed < ordinary * 2.4 && Math.Abs(overlap - slowed) <= 2 &&
                        Math.Abs(oneEnded - slowed) <= 2 && Math.Abs(featherTicks - ordinary) <= 2 &&
                        Math.Abs(flightTicks - ordinary) <= 2 && !target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain),
                        "same measured 4m segment along a native 4.6m path;ordinary=" + ordinary + ";terrain=" + slowed + ";overlap=" + overlap +
                        ";oneEnded=" + oneEnded + ";feather=" + featherTicks + ";flight=" + flightTicks +
                        ";tick counts include native acceleration; no path search or material classification is asserted");
                }
            }
            finally
            {
                agent.Stop(); target.Commands.InterruptAll(true); target.Commands.RemoveFinishedAndUpdateQueue();
                if (immunity != null) immunity.Remove();
                foreach (var area in active) { area.ForceEnd(); Tick(area); }
                for (int index = 0; index < actors.Length; index++) PlaceTerrainActor(actors[index], positions[index]);
                Check(assertions, rows, "native-terrain-movement-cleanup", actors.Select((value, index) => value.Position.Equals(positions[index])).All(value => value) &&
                    !target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain) &&
                    !target.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, terrain)),
                    "exact owned actor placement restored and all owned terrain/immunity facts ended");
            }
        }

        private static AreaEffectEntityData Spawn(UnitEntityData caster, BlueprintAbility ability,
            BlueprintAbilityAreaEffect area, Vector3 point)
        {
            var data = new AbilityData(ability, caster.Descriptor);
            var target = new TargetWrapper(point);
            var context = new AbilityExecutionContext(data, data.CalculateParams(), target, null);
            return ElementalTreacherousActivate.Spawn(context, area, target);
        }
        private static void Tick(AreaEffectEntityData area)
        {
            if (area == null) throw new InvalidOperationException("Native fixed area absent.");
            typeof(AreaEffectEntityData).GetMethod("UpdateUnits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(area, null);
            area.Tick(); area.Tick(); Game.Instance.EntityCreator.Tick();
        }
        private static void Check(ICollection<RuntimeTestAssertion> assertions, JArray rows, string name, bool pass, string observed)
        {
            rows.Add(new JObject { { "name", name }, { "pass", pass }, { "observed", observed } });
            assertions.Add(new RuntimeTestAssertion { Name = "elemental-treacherous-" + name,
                Expected = "owned native difficult-terrain graph behavior", Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "native areas, conditions and movement speed on request-local actors; no save touched" });
        }
    }
}
