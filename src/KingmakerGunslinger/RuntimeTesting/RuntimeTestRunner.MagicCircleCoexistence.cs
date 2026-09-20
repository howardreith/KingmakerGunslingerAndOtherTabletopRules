using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void CircleCommunalAndPaladin(UnitEntityData caster, UnitEntityData bearer,
            UnitEntityData recipient, UnitEntityData controller, Spellbook book,
            List<UnitEntityData> actors, List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var library = BlueprintBootstrap.Library;
            var communal = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, "93f391b0c5a99e04e83bbfbe3bb6db64", "native communal Protection from Evil");
            var ward = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "4a6911969911ce9499bf27dde9bfcedc", "native shared Protection from Evil terminal");
            var auraFeature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, "4ddace64ffabcf24a8268e4d52c23e88", "native Paladin protection aura feature");
            var auraCarrier = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "c8876df41a13f9243b3bfdb15b84b129", "native Paladin aura carrier");
            var auraRecipient = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "8deb9d5cef3472646ac5199eb9edfb87", "native Paladin aura recipient");
            var dominate = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, "d7cbd2004ce66a042aeab2e95a3c5c61", "native Dominate Person");
            var dominated = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "c0f4e1c24c9cd334ca988ed1bd9d201f", "native domination terminal");
            var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
            var changed = new[] { recipient, controller };
            var positioned = new[] { caster, bearer, recipient, controller };
            var positions = positioned.Select(unit => unit.Position).ToArray();
            var factions = changed.Select(unit => unit.Faction).ToArray();
            var originalAlignment = controller.Descriptor.Alignment.Value;
            int wisdom = recipient.Stats.Wisdom.BaseValue;
            var foreign = Game.Instance.State.Units.All.Except(actors).ToArray();
            var foreignBuffs = foreign.Select(unit => unit.Buffs.Enumerable.ToArray()).ToArray();
            var nativeWards = new List<Buff>();
            Fact auraFact = null; AreaEffectEntityData paladinArea = null, circleArea = null;
            Buff carrier = null;
            var observer = new CircleControlCastCapture(recipient, dominated);
            try {
                // Native communal Protection is ally-only. Isolate owned actors
                // on real walkable scene geometry before joining the recipients
                // to the caster's native faction. No campaign actor is targeted.
                var selector = communal.GetComponent<AbilityTargetsAround>();
                float exclusion = selector.AoERadius.Meters + 3f;
                UnityEngine.Vector3? staging = null;
                for (int radius = 14; radius <= 42 && !staging.HasValue; radius += 7)
                    for (int direction = 0; direction < 32 && !staging.HasValue; direction++) {
                        float angle = direction * UnityEngine.Mathf.PI / 16;
                        var requested = caster.Position + new UnityEngine.Vector3(UnityEngine.Mathf.Cos(angle) * radius, 0, UnityEngine.Mathf.Sin(angle) * radius);
                        var node = AstarPath.active.GetNearest(requested);
                        if (node.node != null && node.node.Walkable && UnityEngine.Vector3.Distance(node.clampedPosition, requested) < .5f &&
                            foreign.All(unit => UnityEngine.Vector3.Distance(unit.Position, node.clampedPosition) > exclusion + unit.Corpulence)) staging = node.clampedPosition;
                    }
                if (!staging.HasValue) throw new InvalidOperationException("No bounded walkable scene point isolates the native communal radius.");
                foreach (var unit in positioned) unit.Translocate(staging.Value, null);
                recipient.Descriptor.SwitchFactions(caster.Faction, true);
                controller.Descriptor.SwitchFactions(factions[0], true);
                CircleSynchronize(actors);
                controller.Descriptor.Alignment.Set(Alignment.LawfulEvil); recipient.Stats.Wisdom.BaseValue = 3;
                int baselineAc = CircleAttackAC(controller, recipient), baselineSave = CircleSave(controller, recipient, dominate);
                book.Rest(); book.AddKnown(2, communal.Parent ?? communal, true);
                var data = communal.Parent == null ? new AbilityData(communal, book) : new AbilityData(new AbilityData(communal.Parent, book), communal);
                var context = new AbilityExecutionContext(data, data.CalculateParams(), new TargetWrapper(caster), Rulebook.CurrentContext);
                var targets = communal.GetComponent<AbilityTargetsAround>().Select(context, new TargetWrapper(caster)).Select(value => value.Unit).ToArray();
                diagnostics.Add("communal-native-preflight:radius=" + selector.AoERadius.Meters + ";targets=" + string.Join(",", targets.Select(unit => unit.Descriptor.CustomName + ":" + unit.UniqueId)) +
                    ";recipientAlly=" + caster.IsAlly(recipient) + ";recipientEnemy=" + caster.IsEnemy(recipient) + ";casterPlayer=" + caster.IsPlayerFaction + ";recipientPlayer=" + recipient.IsPlayerFaction);
                if (!targets.Contains(recipient) || targets.Any(unit => !actors.Contains(unit)))
                    throw new InvalidOperationException("Native communal target selection is not confined to the owned actors.");
                int slots = book.GetSpontaneousSlots(2);
                CircleCast(caster, caster, data, diagnostics);
                nativeWards.AddRange(actors.SelectMany(unit => CircleBuffs(unit, ward)));
                var communalRecipient = CircleBuffs(recipient, ward).Single();
                var deadline = communalRecipient.EndTime;
                assertions.Add(Assertion("circle-native-communal-delivery", "one level-2 slot, native one-minute communal duration", "slots=" + slots + "->" + book.GetSpontaneousSlots(2) + ";seconds=" + communalRecipient.TimeLeft.TotalSeconds,
                    book.GetSpontaneousSlots(2) == slots - 1 && Math.Abs(communalRecipient.TimeLeft.TotalSeconds - 60) < 2 &&
                    targets.All(unit => CircleBuffs(unit, ward).Length == 1),
                    "real unchanged native communal spell, selector parent, target selection and shared terminal"));
                auraFact = bearer.Descriptor.AddFact(auraFeature);
                paladinArea = CircleArea(CircleBuffs(bearer, auraCarrier).Single());
                CircleRefresh(paladinArea, actors); CircleRefresh(paladinArea, actors);
                var paladinRecipient = CircleBuffs(recipient, auraRecipient).Single();
                book.Rest(); CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
                carrier = CircleBuffs(bearer, circle.Carrier).Single(); circleArea = CircleArea(carrier);
                CircleRefresh(circleArea, actors); CircleRefresh(circleArea, actors);
                assertions.Add(Assertion("circle-communal-paladin-coexistence", "three distinct sources with only +2 typed defenses", "ac=" + CircleAttackAC(controller, recipient) + ";save=" + CircleSave(controller, recipient, dominate),
                    communalRecipient.Active && paladinRecipient.Active && CircleBuffs(recipient, circle.Recipient).Length == 1 &&
                    CircleAttackAC(controller, recipient) == baselineAc + 2 && CircleSave(controller, recipient, dominate) == baselineSave + 2,
                    "actual communal cast, native Paladin AuraFeatureComponent and independently cast circle"));
                EventBus.Subscribe(observer);
                var controlBook = controller.Descriptor.Spellbooks.Single(value => value.Blueprint.name == "SorcererSpellbook");
                bool enhanced = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
                carrier.Remove(); CircleRefresh(circleArea, actors);
                assertions.Add(Assertion("circle-leaves-communal-and-paladin", "exact existing Protection sources remain", "communal=" + communalRecipient.Active + ";paladin=" + paladinRecipient.Active,
                    CircleBuffs(recipient, circle.Recipient).Length == 0 && communalRecipient.Active && communalRecipient.EndTime == deadline &&
                    paladinRecipient.Active && ReferenceEquals(CircleBuffs(recipient, auraRecipient).Single(), paladinRecipient) && !paladinArea.IsEnded,
                    "only the circle carrier was removed; no shared Protection instance or aura was removed"));
                CircleIncomingCast(controller, recipient, controlBook, dominate, dominated, observer, diagnostics);
                assertions.Add(Assertion("circle-remaining-communal-paladin-control", enhanced ? "native incoming control blocked" : "native incoming control applies", observer.Describe(),
                    observer.CanApply != enhanced && CircleBuffs(recipient, dominated).Length == (enhanced ? 0 : 1),
                    "same real Dominate Person spell and observed failed save after circle removal"));
                foreach (var applied in CircleBuffs(recipient, dominated)) applied.Remove();
                foreach (var buff in nativeWards) buff.Remove();
                CircleIncomingCast(controller, recipient, controlBook, dominate, dominated, observer, diagnostics);
                assertions.Add(Assertion("circle-paladin-only-shared-control", enhanced ? "native incoming control blocked" : "native incoming control applies", observer.Describe(),
                    paladinRecipient.Active && observer.CanApply != enhanced && CircleBuffs(recipient, dominated).Length == (enhanced ? 0 : 1),
                    "native Paladin recipient is now the only remaining shared Protection delivery"));
                foreach (var applied in CircleBuffs(recipient, dominated)) applied.Remove();
                bearer.Descriptor.RemoveFact(auraFact); auraFact = null;
                CircleRefresh(paladinArea, actors);
                CircleIncomingCast(controller, recipient, controlBook, dominate, dominated, observer, diagnostics);
                assertions.Add(Assertion("circle-family-removed-control-positive", "same incoming spell now applies", observer.Describe(),
                    observer.CanApply && CircleBuffs(recipient, dominated).Length == 1 && CircleBuffs(recipient, auraRecipient).Length == 0,
                    "positive control after native Paladin feature removal; no lingering circle or communal immunity"));
            }
            finally {
                EventBus.Unsubscribe(observer);
                foreach (var applied in CircleBuffs(recipient, dominated)) applied.Remove();
                carrier?.Remove();
                if (auraFact != null) bearer.Descriptor.RemoveFact(auraFact);
                foreach (var buff in nativeWards) if (buff.Active) buff.Remove();
                foreach (var area in new[] { circleArea, paladinArea }.Where(value => value != null && !value.Destroyed)) CircleRefresh(area, actors);
                for (int index = 0; index < changed.Length; index++) changed[index].Descriptor.SwitchFactions(factions[index], true);
                controller.Descriptor.Alignment.Set(originalAlignment); recipient.Stats.Wisdom.BaseValue = wisdom;
                for (int index = 0; index < positioned.Length; index++) positioned[index].Translocate(positions[index], null);
                CircleSynchronize(actors);
            }
            assertions.Add(Assertion("circle-family-coexistence-isolation", "all foreign buffs remain exact", "foreign=" + foreign.Length,
                foreign.Select((unit, index) => unit.Buffs.Enumerable.SequenceEqual(foreignBuffs[index])).All(value => value),
                "native communal selection was preflighted; native Paladin and circle sources belong only to request actors"));
        }
    }
}
