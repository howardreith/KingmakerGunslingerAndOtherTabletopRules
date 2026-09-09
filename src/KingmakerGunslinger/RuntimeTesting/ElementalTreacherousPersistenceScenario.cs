using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private bool TreacherousPersistence => _nereidPersistence &&
                (string)_request.Parameters?["qualificationEffect"] == "TreacherousEarth";
            private readonly HashSet<AreaEffectEntityData> _treacherousLoadedAreas = new HashSet<AreaEffectEntityData>();
            private ElementalAlternateTraitBlueprints TreacherousPersistenceTrait =>
                _blueprintSet.Oread.AlternateTraits.Require(ElementalAlternateTraitId.TreacherousEarth);
            private static readonly FieldInfo TerrainCreation = typeof(AreaEffectEntityData).GetField(
                "m_CreationTime", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly FieldInfo TerrainDuration = typeof(AreaEffectEntityData).GetField(
                "m_Duration", BindingFlags.Instance | BindingFlags.NonPublic);

            private UnitEntityData[] TreacherousSavedActors()
            {
                return new[] { Gender.Male, Gender.Female }.Select(sex => {
                    var fixture = _fixtures.Single(value => value.Blueprints.AlternateTraits.Race == ElementalHeritageRace.Oread &&
                        value.Gender == sex && value.Heritage.Definition.IsGeneral);
                    var unit = Game.Instance.State.Units.All.Single(value => IsFixtureUnit(value, fixture));
                    if (unit.View == null || unit.Descriptor.State.IsDead || unit.IsInCombat)
                        throw new InvalidOperationException("The exact terrain effect actors are not ready.");
                    return unit;
                }).ToArray();
            }
            private void PrepareTreacherousSavedArea(int level)
            {
                if (!TreacherousPersistence) return;
                EnsureNereidPersistencePause();
                var trait = TreacherousPersistenceTrait;
                var blueprint = trait.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
                var ability = trait.Mechanics().OfType<BlueprintAbility>().Single();
                var actors = TreacherousSavedActors();
                if (!trait.Definition.IsPublished || !ElementalTreacherousFactory.IsExactEffect(trait) ||
                    actors[0].Descriptor.Progression.CharacterLevel != level || actors[0].Descriptor.HasFact(trait.Marker) ||
                    Game.Instance.State.AreaEffects.All.Any(value => ReferenceEquals(value.Blueprint, blueprint)))
                    throw new InvalidOperationException("Only the exact independent terrain-effect fixture is permitted.");
                // This is explicitly an effect-only fixture. Traversable native
                // staging is not evidence of legal ground material or selection.
                var point = NereidPersistencePosition(actors[0], new[] { actors[1] });
                PlaceNereidPersistenceUnit(actors[0], point);
                PlaceNereidPersistenceUnit(actors[1], point + new UnityEngine.Vector3(0, 0, .8f));
                var record = new JObject { ["kind"] = "treacherous-native-effect-preparation", ["level"] = level,
                    ["scope"] = "owned native fixed-area persistence; no production material, activation or trait-selection claim" };
                SynchronizeNereidFixturePositions(actors, record);
                actors[0].Descriptor.AddFact(trait.Marker);
                var resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
                actors[0].Descriptor.Resources.Spend(resource, 1);
                record["providerPresent"] = actors[0].Descriptor.HasFact(trait.Provider);
                record["resourceAmount"] = actors[0].Descriptor.Resources.GetResourceAmount(resource);
                var context = new AbilityData(ability, actors[0].Descriptor).CreateExecutionContext(new TargetWrapper(point));
                var area = ElementalTreacherousActivate.Spawn(context, blueprint, new TargetWrapper(point));
                _treacherousLoadedAreas.Add(area);
                Game.Instance.EntityCreator.Tick();
                if (TerrainCreation == null || TerrainDuration == null ||
                    (TimeSpan)TerrainCreation.GetValue(area) != Game.Instance.TimeController.GameTime ||
                    (TimeSpan)TerrainDuration.GetValue(area) != TimeSpan.FromMinutes(level))
                    throw new InvalidOperationException("The native fixed-area lifetime fields differ from their audited contract.");
                // Age only this owned area's creation timestamp, never world time.
                TerrainCreation.SetValue(area, (TimeSpan)TerrainCreation.GetValue(area) - TimeSpan.FromSeconds(20));
                TickNereidNativeArea(area);
                record["areaId"] = area.UniqueId; record["agedBySeconds"] = 20;
                _nereidPersistenceRecords.Add(record);
            }
            private void CleanupTreacherousFixtureAreas()
            {
                if (!TreacherousPersistence) return;
                var blueprint = TreacherousPersistenceTrait.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
                var owned = Game.Instance.State.AreaEffects.All.Where(value => ReferenceEquals(value.Blueprint, blueprint)).ToArray();
                if (owned.Any(value => !_treacherousLoadedAreas.Contains(value)))
                    throw new InvalidOperationException("A foreign terrain area entered completion fixture cleanup.");
                foreach (var area in owned) {
                    area.ForceEnd(); TickNereidNativeArea(area); TickNereidNativeArea(area);
                }
                if (owned.Length != 0) DrainNereidAreaDestruction(owned[0]);
            }

            private void RecordTreacherousSavedArea(int level, string phase)
            {
                if (!TreacherousPersistence) return;
                var trait = TreacherousPersistenceTrait;
                var blueprint = trait.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
                var terrain = trait.Mechanics().OfType<BlueprintBuff>().Single();
                var units = Game.Instance.State.Units.All.ToArray();
                var areas = Game.Instance.State.AreaEffects.All.Where(value => ReferenceEquals(value.Blueprint, blueprint)).ToArray();
                var record = new JObject { ["kind"] = "treacherous-native-saved-area", ["phase"] = phase,
                    ["expectedLevel"] = level, ["areaCount"] = areas.Length };
                bool exact;
                if (level == 0) exact = areas.Length == 0 && units.All(unit =>
                    !unit.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, terrain)));
                else {
                    if (areas.Length != 1 || TerrainCreation == null || TerrainDuration == null)
                        throw new InvalidOperationException("One exact saved fixed terrain area is required.");
                    var actors = TreacherousSavedActors(); var area = areas[0];
                    if (phase == "fresh-load-before-mutation") _treacherousLoadedAreas.Add(area);
                    var created = (TimeSpan)TerrainCreation.GetValue(area);
                    var duration = (TimeSpan)TerrainDuration.GetValue(area);
                    var now = Game.Instance.TimeController.GameTime;
                    var remaining = created + duration - now;
                    var point = area.Position;
                    SynchronizeNereidFixturePositions(actors, record);
                    TickNereidNativeArea(area); TickNereidNativeArea(area);
                    var speed = actors[1].CurrentSpeedMps;
                    var affected = units.SelectMany(unit => unit.Buffs.Enumerable.Where(value =>
                        ReferenceEquals(value.Blueprint, terrain)).Select(buff => new { unit, buff })).ToArray();
                    exact = !area.IsEnded && area.Position.Equals(point) &&
                        ReferenceEquals(area.Context.MaybeCaster, actors[0]) && actors[0].Descriptor.HasFact(trait.Marker) &&
                        actors[0].Descriptor.HasFact(trait.Provider) &&
                        actors[0].Descriptor.Resources.GetResourceAmount(trait.Mechanics().OfType<BlueprintAbilityResource>().Single()) == 0 &&
                        duration == TimeSpan.FromMinutes(level) &&
                        Math.Abs(remaining.TotalSeconds - (level * 60 - 20)) < .05 &&
                        actors.All(unit => unit.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain)) &&
                        affected.Length == 2 && affected.All(value => actors.Contains(value.unit) &&
                            value.buff.SourceAreaEffectId == area.UniqueId && ReferenceEquals(value.buff.Context.MaybeCaster, actors[0])) &&
                        actors.All(unit => affected.Count(value => ReferenceEquals(value.unit, unit)) == 1);
                    record["areaId"] = area.UniqueId; record["ownerId"] = area.Context.MaybeCaster?.UniqueId;
                    record["sourceAbility"] = area.Context.SourceAbility?.AssetGuid;
                    record["position"] = new JArray(point.x, point.y, point.z);
                    record["creationTicks"] = created.Ticks; record["durationTicks"] = duration.Ticks;
                    record["nowTicks"] = now.Ticks; record["remainingSeconds"] = remaining.TotalSeconds;
                    record["affected"] = new JArray(affected.Select(value => value.unit.UniqueId));
                    record["originalArchiveComparisonRequired"] = true;
                    if (exact && phase == "fresh-load-before-mutation") {
                        if (_moduleRestored) {
                            TerrainCreation.SetValue(area, now - duration + TimeSpan.FromSeconds(.1));
                            TickNereidNativeArea(area);
                            bool before = !area.IsEnded && actors[1].Descriptor.State.HasCondition(UnitCondition.DifficultTerrain);
                            TerrainCreation.SetValue(area, now - duration - TimeSpan.FromSeconds(.1));
                            TickNereidNativeArea(area); TickNereidNativeArea(area);
                            exact &= before && area.IsEnded;
                            record["nativeExpiryBeforeAfter"] = before && area.IsEnded;
                        }
                        actors[0].Descriptor.RemoveFact(trait.Marker);
                        TickNereidNativeArea(area); TickNereidNativeArea(area);
                        DrainNereidAreaDestruction(area);
                        exact &= area.Destroyed && !Game.Instance.State.AreaEffects.All.Contains(area) &&
                            actors.All(unit => !unit.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, terrain))) &&
                            Math.Abs(actors[1].CurrentSpeedMps - speed * 2) < .001 && Game.Instance.TimeController.GameTime == now;
                        record["ownedRemovalAndNativeMovementRestored"] = exact;
                    }
                }
                record["exact"] = exact; _nereidPersistenceRecords.Add(record);
                Add(_assertions, "elemental-treacherous-saved-state-" + phase,
                    "owned fixed area preserves native owner/position/remaining lifetime; native expiry/removal leaves no terrain facts",
                    record.ToString(Formatting.None), exact, "independent effect scope; source archive identity comparison is a separate required driver gate");
                if (!exact) throw new InvalidOperationException("The owned fixed-area save state diverged: " + record.ToString(Formatting.None));
            }
        }
    }
}
