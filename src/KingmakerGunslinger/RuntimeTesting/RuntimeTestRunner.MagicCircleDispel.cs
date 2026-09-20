using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static void CircleDispel(UnitEntityData caster, UnitEntityData bearer,
            UnitEntityData recipient, Spellbook book, MagicCircleBlueprintSet circle,
            List<UnitEntityData> actors, List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var root = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                "92681f181b507b34ea87018e8f7a528a", "native Dispel Magic");
            var variants = root.GetComponent<AbilityVariants>()?.Variants ?? new[] { root };
            foreach (var variant in variants)
                diagnostics.Add("native-dispel-graph:" + variant.AssetGuid + ":" + variant.name +
                    ";point=" + variant.CanTargetPoint + ";friends=" + variant.CanTargetFriends +
                    ";enemies=" + variant.CanTargetEnemies + ";components=" +
                    string.Join(",", variant.ComponentsArray.Select(value => value.GetType().Name)));
            var leaves = variants.Where(value => value.GetComponent<AbilityEffectRunAction>()?.Actions.Actions
                .OfType<ContextActionDispelMagic>().Any() == true).ToArray();
            var targeted = leaves.Single(value => !value.CanTargetPoint && value.CanTargetFriends && value.CanTargetEnemies);
            var point = leaves.Single(value => value.CanTargetPoint);
            book.AddKnown(3, root, true); book.Rest();
            CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
            CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
            var carriers = CircleBuffs(bearer, circle.Carrier);
            var areas = carriers.Select(CircleArea).ToArray();
            foreach (var area in areas) CircleRefresh(area, actors);
            if (carriers.Length != 2 || CircleBuffs(recipient, circle.Recipient).Length != 2)
                throw new InvalidOperationException("Dispel prerequisite requires two native source-owned circles.");
            var deadlines = carriers.Select(value => value.EndTime).ToArray();
            var positioned = new[] { caster, bearer, recipient };
            var originalPositions = positioned.Select(value => value.Position).ToArray();
            var capture = new CircleDispelCapture();
            EventBus.Subscribe(capture);
            try {
                int slots = book.GetSpontaneousSlots(3);
                CircleCast(caster, recipient, new AbilityData(new AbilityData(root, book), targeted), diagnostics);
                foreach (var area in areas) CircleRefresh(area, actors);
                assertions.Add(Assertion("circle-recipient-native-dispel", "derivative protection is not an independent spell",
                    "rules=" + capture.Rules.Count + ";contributions=" + CircleBuffs(recipient, circle.Recipient).Length,
                    capture.Rules.Count == 0 && CircleBuffs(recipient, circle.Recipient).Length == 2 &&
                    carriers.Select((value, i) => value.Active && value.EndTime == deadlines[i]).All(value => value) &&
                    book.GetSpontaneousSlots(3) == slots - 1,
                    "actual native targeted Dispel Magic cast; native IsNotDispelable gate; one spent slot"));

                capture.Rules.Clear();
                int attempts = 0;
                bool debits = true;
                while (CircleBuffs(bearer, circle.Carrier).Length == 2 && attempts++ < 12) {
                    book.Rest(); slots = book.GetSpontaneousSlots(3);
                    CircleCast(caster, bearer, new AbilityData(new AbilityData(root, book), targeted), diagnostics);
                    debits &= book.GetSpontaneousSlots(3) == slots - 1;
                    foreach (var area in areas.Where(value => !value.Destroyed)) CircleRefresh(area, actors);
                }
                var successes = capture.Rules.Where(value => value.Success).ToArray();
                assertions.Add(Assertion("circle-native-carrier-dispel-ownership", "one dispelled carrier, one surviving source",
                    "attempts=" + attempts + ";successfulRules=" + successes.Length +
                        ";carriers=" + CircleBuffs(bearer, circle.Carrier).Length,
                    debits && successes.Length == 1 && carriers.Contains(successes[0].Buff) &&
                    CircleBuffs(bearer, circle.Carrier).Length == 1 &&
                    CircleBuffs(recipient, circle.Recipient).Length == 1 &&
                    CircleBuffs(recipient, circle.Recipient).Single().SourceAreaEffectId ==
                        CircleArea(CircleBuffs(bearer, circle.Carrier).Single()).UniqueId,
                    "bounded real native Dispel Magic casts and unmodified RuleDispelMagic dice; exact buff/area ownership"));
                if (CircleBuffs(bearer, circle.Carrier).Length != 1)
                    throw new InvalidOperationException("Native carrier dispel did not leave exactly one source.");

                var remaining = CircleBuffs(bearer, circle.Carrier).Single();
                var remainingArea = CircleArea(remaining);
                CircleIsolatePointDispel(positioned, areas, actors, diagnostics);
                var targetPoint = new TargetWrapper(bearer.Position);
                if (Game.Instance.State.AreaEffects.All.Any(value => !areas.Contains(value) &&
                    value.View?.Shape != null && value.View.Shape.Contains(targetPoint.Point, 0)))
                    throw new InvalidOperationException("Point dispel would intersect a foreign area; fixture refused.");
                assertions.Add(Assertion("circle-point-dispel-fixture-isolated", "native point belongs only to this cast",
                    "point=" + targetPoint.Point + ";area=" + remainingArea.UniqueId,
                    remainingArea.View.Shape.Contains(targetPoint.Point, 0) &&
                    remaining.EndTime == deadlines[Array.IndexOf(carriers, remaining)] &&
                    CircleBuffs(recipient, circle.Recipient).Single().SourceAreaEffectId == remainingArea.UniqueId,
                    "bounded walkable placement of request-owned actors; all foreign area shapes remain excluded"));
                capture.Rules.Clear(); attempts = 0;
                while (!remainingArea.IsEnded && attempts++ < 12) {
                    book.Rest(); slots = book.GetSpontaneousSlots(3);
                    CircleCast(caster, targetPoint, new AbilityData(new AbilityData(root, book), point), diagnostics);
                    debits &= book.GetSpontaneousSlots(3) == slots - 1;
                    CircleRefresh(remainingArea, actors);
                }
                successes = capture.Rules.Where(value => value.Success).ToArray();
                assertions.Add(Assertion("circle-native-area-dispel-carrier-cleanup", "successful area dispel ends its original carrier",
                    "attempts=" + attempts + ";successfulRules=" + successes.Length +
                        ";carrierActive=" + remaining.Active + ";areaEnded=" + remainingArea.IsEnded,
                    debits && successes.Length == 1 && capture.Rules.All(value => ReferenceEquals(value.AreaEffect, remainingArea)) &&
                    remainingArea.IsEnded && !bearer.Buffs.Enumerable.Contains(remaining) &&
                    CircleBuffs(recipient, circle.Recipient).Length == 0,
                    "actual native point Dispel Magic; exact parent/area link; carrier cannot reconstruct a dispelled area"));
                diagnostics.AddRange(capture.Rules.Select(value => "area-dispel:success=" + value.Success +
                    ";roll=" + value.CheckRoll + ";level=" + value.CasterLevel + ";dc=" + value.DC));
            }
            finally {
                EventBus.Unsubscribe(capture);
                for (int index = 0; index < positioned.Length; index++)
                    positioned[index].Translocate(originalPositions[index], null);
                CircleSynchronize(actors);
                foreach (var area in areas.Where(value => !value.Destroyed)) CircleRefresh(area, actors);
            }
        }

        private static void CircleIsolatePointDispel(UnitEntityData[] positioned,
            AreaEffectEntityData[] ownedAreas, List<UnitEntityData> actors, List<string> diagnostics)
        {
            var game = Game.Instance;
            Vector3 origin = positioned[1].Position;
            var foreign = game.State.AreaEffects.All.Where(value => !ownedAreas.Contains(value)).ToArray();
            Func<Vector3, bool> intersects = point => foreign.Any(value =>
                value.View?.Shape != null && value.View.Shape.Contains(point, 0));
            var overlaps = foreign.Where(value => value.View?.Shape != null && value.View.Shape.Contains(origin, 0)).ToArray();
            diagnostics.AddRange(overlaps.Select(value => "point-dispel-original-overlap:id=" + value.UniqueId +
                ";blueprint=" + value.Blueprint.AssetGuid + ":" + value.Blueprint.name +
                ";ended=" + value.IsEnded + ";destroyed=" + value.Destroyed +
                ";owner=" + value.Context?.MaybeOwner?.UniqueId));
            if (overlaps.Length == 0) return;
            // Native ContextActionDispelMagic tests every enumerated shape.
            // Never excuse an overlapping area by name, blueprint or ended flag.
            // This is a bounded test-fixture search, not production membership.
            var foreignUnits = game.State.Units.All.Except(actors).ToArray();
            foreach (float radius in new[] { 6f, 12f, 18f }) {
                for (int direction = 0; direction < 8; direction++) {
                    float angle = direction * Mathf.PI / 4;
                    Vector3 wanted = origin + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    var nearest = AstarPath.active.GetNearest(wanted);
                    if (nearest.node == null || !nearest.node.Walkable ||
                        Vector3.Distance(nearest.clampedPosition, wanted) > .4f || intersects(nearest.clampedPosition) ||
                        foreignUnits.Any(unit => Vector3.Distance(unit.Position, nearest.clampedPosition) < 5 + unit.Corpulence)) continue;
                    foreach (var unit in positioned) unit.Translocate(nearest.clampedPosition, null);
                    CircleSynchronize(actors);
                    foreach (var area in ownedAreas.Where(value => !value.Destroyed)) CircleRefresh(area, actors);
                    diagnostics.Add("point-dispel-isolated-fixture:radius=" + radius + ";direction=" + direction +
                        ";point=" + positioned[1].Position);
                    return;
                }
            }
            throw new InvalidOperationException("No bounded walkable point-dispel fixture excludes all foreign areas and actors.");
        }

        private sealed class CircleDispelCapture : IGlobalRulebookHandler<RuleDispelMagic>
        {
            internal readonly List<RuleDispelMagic> Rules = new List<RuleDispelMagic>();
            public void OnEventAboutToTrigger(RuleDispelMagic evt) { }
            public void OnEventDidTrigger(RuleDispelMagic evt) { Rules.Add(evt); }
        }
    }
}
