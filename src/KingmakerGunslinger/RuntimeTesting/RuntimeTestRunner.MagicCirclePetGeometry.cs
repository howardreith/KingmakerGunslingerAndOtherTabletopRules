using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static void CirclePetAndOcclusion(UnitEntityData caster, UnitEntityData bearer,
            UnitEntityData recipient, Spellbook book, List<UnitEntityData> actors,
            List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var game = Game.Instance; var circles = BlueprintBootstrap.MagicCircles;
            var petBlueprint = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                "54cf380dee486ff42b803174d1b9da1b", "native animal companion leopard");
            var prefab = petBlueprint.Prefab.Load(false);
            if (caster.Descriptor.Pet != null || prefab == null || petBlueprint.CustomizationPreset != null)
                throw new InvalidOperationException("Owned pet fixture requires an unassociated caster and exact native leopard prefab.");
            string prefabId = prefab.UniqueId;
            UnitEntityData pet = null;
            var positioned = new[] { caster, bearer, recipient };
            var original = positioned.Select(unit => unit.Position).ToArray();
            try {
                try { pet = game.EntityCreator.SpawnUnit(petBlueprint, bearer.Position, Quaternion.identity, bearer.HoldingState); }
                finally { prefab.UniqueId = prefabId; }
                if (pet == null) throw new InvalidOperationException("Native leopard creation failed.");
                actors.Add(pet); game.EntityCreator.Tick(); pet.Stats.HitPoints.BaseValue = 10000;
                pet.Descriptor.SetMaster(caster); pet.IsInGame = true;
                foreach (var circle in circles) {
                    pet.Translocate(bearer.Position, null); CircleSynchronize(actors);
                    book.Rest(); CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
                    var carrier = CircleBuffs(bearer, circle.Carrier).Single(); var area = CircleArea(carrier);
                    CircleRefresh(area, actors); CircleRefresh(area, actors);
                    var end = carrier.EndTime; int slots = book.GetSpontaneousSlots(3);
                    assertions.Add(Assertion("circle-native-pet-" + circle.Alignment, "reciprocal native animal companion receives the exact area contribution", "pet=" + pet.Blueprint.AssetGuid,
                        ReferenceEquals(caster.Descriptor.Pet, pet) && ReferenceEquals(pet.Descriptor.Master.Value, caster) &&
                        CircleBuffs(pet, circle.Recipient).Single().SourceAreaEffectId == area.UniqueId,
                        "registered native leopard and UnitDescriptor.SetMaster; no fabricated pet flag"));
                    pet.Translocate(bearer.Position + new Vector3(8, 0, 0), null); CircleRefresh(area, actors);
                    bool outside = CircleBuffs(pet, circle.Recipient).Length == 0;
                    pet.Translocate(bearer.Position, null); CircleRefresh(area, actors);
                    assertions.Add(Assertion("circle-pet-reentry-" + circle.Alignment, "pet exit removes one source; entry restores it without cost or renewed carrier", "outside=" + outside,
                        outside && CircleBuffs(pet, circle.Recipient).Length == 1 && carrier.EndTime == end && book.GetSpontaneousSlots(3) == slots,
                        "native translocation and area membership, same original carrier"));
                    carrier.Remove(); CircleRefresh(area, actors);
                }
                pet.Descriptor.SetMaster(null); pet.Destroy(); game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick(); actors.Remove(pet); pet = null;

                // Bounded inspection of actual scene geometry, used only to
                // stage the disposable test. Production uses native membership.
                Vector3? near = null, far = null;
                var foreign = game.State.Units.All.Except(actors).ToArray();
                var geometry = Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance;
                int inspected = 0;
                var blockers = Kingmaker.Visual.FogOfWar.FogOfWarBlocker.All
                    .Where(value => value != null && value.isActiveAndEnabled)
                    .OrderBy(value => Vector3.Distance(value.transform.position, original[1])).ToArray();
                foreach (var blocker in blockers) {
                    var points = blocker.Points;
                    for (int segment = 0; segment < points.Length && inspected < 4096 && !near.HasValue; segment++) {
                        if (segment + 1 == points.Length && !blocker.Closed) break;
                        var a = points[segment]; var b = points[(segment + 1) % points.Length];
                        var tangent = (b - a).normalized;
                        var normal = new Vector2(-tangent.y, tangent.x);
                        foreach (float along in new[] { .25f, .5f, .75f }) {
                            inspected++;
                            var middle = Vector2.Lerp(a, b, along);
                            var left = middle + normal * 1.3f; var right = middle - normal * 1.3f;
                            var wantedA = new Vector3(left.x, original[1].y, left.y);
                            var wantedB = new Vector3(right.x, original[1].y, right.y);
                            var start = AstarPath.active.GetNearest(wantedA); var finish = AstarPath.active.GetNearest(wantedB);
                            if (start.node == null || finish.node == null || !start.node.Walkable || !finish.node.Walkable) continue;
                            var deltaA = start.clampedPosition - wantedA; deltaA.y = 0;
                            var deltaB = finish.clampedPosition - wantedB; deltaB.y = 0;
                            if (deltaA.magnitude > .4f || deltaB.magnitude > .4f ||
                                Vector3.Distance(start.clampedPosition, finish.clampedPosition) > 2.95f ||
                                foreign.Any(unit => Vector3.Distance(unit.Position, start.clampedPosition) < 5 + unit.Corpulence) ||
                                !geometry.HasObstacle(start.clampedPosition, finish.clampedPosition, 0)) continue;
                            near = start.clampedPosition; far = finish.clampedPosition;
                            diagnostics.Add("native-circle-wall-blocker=" + blocker.name + ";segment=" + segment + ";sample=" + along);
                            break;
                        }
                    }
                    if (near.HasValue || inspected >= 4096) break;
                }
                diagnostics.Add("native-circle-wall-search:blockers=" + blockers.Length + ";samples=" + inspected);
                if (!near.HasValue) throw new InvalidOperationException("No bounded native walkable wall pair inside the mechanical radius was found.");
                diagnostics.Add("native-circle-wall:bearer=" + near.Value + ";target=" + far.Value + ";distance=" + Vector3.Distance(near.Value, far.Value));
                foreach (var circle in circles) {
                    caster.Translocate(near.Value, null); bearer.Translocate(near.Value, null); recipient.Translocate(far.Value, null);
                    CircleSynchronize(actors); book.Rest(); CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
                    var carrier = CircleBuffs(bearer, circle.Carrier).Single(); var area = CircleArea(carrier);
                    CircleRefresh(area, actors); CircleRefresh(area, actors);
                    bool geometric = area.View.Shape.Contains(recipient.Position, recipient.View.Corpulence);
                    bool blocked = geometry.HasObstacle(area.View.transform.position, recipient.Position, 0);
                    assertions.Add(Assertion("circle-native-wall-exclusion-" + circle.Alignment, "inside radius but native obstruction prevents coverage", "inside=" + geometric + ";obstacle=" + blocked,
                        geometric && blocked && CircleBuffs(recipient, circle.Recipient).Length == 0 && CircleBuffs(bearer, circle.Recipient).Length == 1,
                        "actual installed scene wall and native area containment/line-of-sight; no distance-only substitution"));
                    var end = carrier.EndTime;
                    recipient.Translocate(near.Value, null); CircleRefresh(area, actors);
                    assertions.Add(Assertion("circle-native-wall-open-positive-" + circle.Alignment, "same cast covers target after moving onto unobstructed side", "count=" + CircleBuffs(recipient, circle.Recipient).Length,
                        !geometry.HasObstacle(area.View.transform.position, recipient.Position, 0) && CircleBuffs(recipient, circle.Recipient).Length == 1 && carrier.EndTime == end,
                        "native positive membership after obstruction removed by owned target movement"));
                    carrier.Remove(); CircleRefresh(area, actors);
                }
            }
            finally {
                foreach (var circle in circles) foreach (var carrier in CircleBuffs(bearer, circle.Carrier)) carrier.Remove();
                if (pet != null) { pet.Descriptor.SetMaster(null); pet.Destroy(); game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick(); actors.Remove(pet); }
                for (int index = 0; index < positioned.Length; index++) positioned[index].Translocate(original[index], null);
                prefab.UniqueId = prefabId;
            }
        }
    }
}
