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
                var targetPoint = new TargetWrapper(bearer.Position);
                if (Game.Instance.State.AreaEffects.All.Any(value => !areas.Contains(value) &&
                    value.View?.Shape != null && value.View.Shape.Contains(targetPoint.Point, 0)))
                    throw new InvalidOperationException("Point dispel would intersect a foreign area; fixture refused.");
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
                    debits && successes.Length == 1 && ReferenceEquals(successes[0].AreaEffect, remainingArea) &&
                    remainingArea.IsEnded && !bearer.Buffs.Enumerable.Contains(remaining) &&
                    CircleBuffs(recipient, circle.Recipient).Length == 0,
                    "actual native point Dispel Magic; exact parent/area link; carrier cannot reconstruct a dispelled area"));
                diagnostics.AddRange(capture.Rules.Select(value => "area-dispel:success=" + value.Success +
                    ";roll=" + value.CheckRoll + ";level=" + value.CasterLevel + ";dc=" + value.DC));
            }
            finally { EventBus.Unsubscribe(capture); }
        }

        private sealed class CircleDispelCapture : IGlobalRulebookHandler<RuleDispelMagic>
        {
            internal readonly List<RuleDispelMagic> Rules = new List<RuleDispelMagic>();
            public void OnEventAboutToTrigger(RuleDispelMagic evt) { }
            public void OnEventDidTrigger(RuleDispelMagic evt) { Rules.Add(evt); }
        }
    }
}
