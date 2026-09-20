using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void CircleFamily(UnitEntityData caster, UnitEntityData bearer, UnitEntityData recipient, UnitEntityData controller,
            Spellbook book, List<UnitEntityData> actors, List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var library = BlueprintBootstrap.Library;
            var dominate = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, "d7cbd2004ce66a042aeab2e95a3c5c61", "native incoming Dominate Person");
            var dominated = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "c0f4e1c24c9cd334ca988ed1bd9d201f", "native incoming control terminal");
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, "b3a505fb61437dc4097f43c3f8f9a4cf", "incoming controller class");
            object levelController = null;
            int wisdom = recipient.Stats.Wisdom.BaseValue;
            var observer = new CircleControlCastCapture(recipient, dominated);
            try {
                controller.Stats.Charisma.BaseValue = 30;
                AdvanceDisposableSpellcaster(controller.Descriptor, sorcerer, 12, ref levelController);
                var controlBook = controller.Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, sorcerer.Spellbook));
                while (controlBook.CasterLevel < 12) controlBook.AddCasterLevel();
                controlBook.UpdateAllSlotsSize(false); controlBook.AddKnown(5, dominate, true);
                recipient.Stats.Wisdom.BaseValue = 3;
                EventBus.Subscribe(observer);
                bool enabled = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
                var matching = new[] { Alignment.LawfulEvil, Alignment.ChaoticGood, Alignment.LawfulNeutral, Alignment.ChaoticNeutral };
                var opposite = new[] { Alignment.ChaoticGood, Alignment.LawfulEvil, Alignment.ChaoticEvil, Alignment.LawfulGood };
                var descriptors = new[] { SpellDescriptor.Good, SpellDescriptor.Evil, SpellDescriptor.Chaos, SpellDescriptor.Law };
                var circles = BlueprintBootstrap.MagicCircles;
                for (int index = 0; index < circles.Length; index++) {
                    var circle = circles[index];
                    string label = "circle-family-" + circle.Alignment.ToLowerInvariant() + "-";
                    book.Rest();
                    if (!book.GetKnownSpells(3).Any(value => ReferenceEquals(value.Blueprint, circle.Spell))) book.AddKnown(3, circle.Spell, true);
                    controller.Descriptor.Alignment.Set(matching[index]);
                    int initialAc = CircleAttackAC(controller, recipient), initialSave = CircleSave(controller, recipient, dominate);
                    CircleIncomingCast(controller, recipient, controlBook, dominate, dominated, observer, diagnostics);
                    assertions.Add(Assertion(label + "unprotected-positive", "native control spell really applies", observer.Describe(),
                        observer.Applications == 1 && observer.CanApply && CircleBuffs(recipient, dominated).Length == 1,
                        "native Sorcerer casting, failed Will save and RuleApplyBuff; no forced dice or substituted actions"));
                    foreach (var buff in CircleBuffs(recipient, dominated)) buff.Remove();
                    int slots = book.GetSpontaneousSlots(3);
                    CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
                    var carrier = CircleBuffs(bearer, circle.Carrier).Single(); var area = CircleArea(carrier);
                    CircleRefresh(area, actors); CircleRefresh(area, actors);
                    assertions.Add(Assertion(label + "native-cast-and-defenses", "correct descriptor, one slot, caster/bearer context, +2 typed defenses", "source=" + carrier.Context.MaybeCaster?.UniqueId,
                        book.GetSpontaneousSlots(3) == slots - 1 && ReferenceEquals(carrier.Context.MaybeCaster, caster) &&
                        ReferenceEquals(area.Context.MaybeOwner, bearer) && circle.Spell.GetComponent<SpellComponent>().School == SpellSchool.Abjuration &&
                        circle.Spell.GetComponent<SpellDescriptorComponent>().Descriptor == descriptors[index] &&
                        CircleBuffs(recipient, circle.Recipient).Length == 1 && CircleAttackAC(controller, recipient) == initialAc + 2 &&
                        CircleSave(controller, recipient, dominate) == initialSave + 2,
                        "actual independently learned spell, native held-touch command, attached area, attack and saving-throw rules"));
                    CircleIncomingCast(controller, recipient, controlBook, dominate, dominated, observer, diagnostics);
                    assertions.Add(Assertion(label + "matching-control-cast", enabled ? "RuleApplyBuff veto" : "control applies", observer.Describe(),
                        observer.Applications == 1 && observer.CanApply != enabled && CircleBuffs(recipient, dominated).Length == (enabled ? 0 : 1),
                        "actual incoming Dominate Person cast after a failed native saving throw; controller alignment differs from protected actors"));
                    foreach (var buff in CircleBuffs(recipient, dominated)) buff.Remove();
                    foreach (var alignment in new[] { opposite[index], Alignment.TrueNeutral }) {
                        controller.Descriptor.Alignment.Set(alignment);
                        CircleIncomingCast(controller, recipient, controlBook, dominate, dominated, observer, diagnostics);
                        assertions.Add(Assertion(label + "nonmatching-" + alignment, "native control applies", observer.Describe(),
                            observer.Applications == 1 && observer.CanApply && CircleBuffs(recipient, dominated).Length == 1,
                            "same bounded spell path; only incoming controller alignment changes"));
                        foreach (var buff in CircleBuffs(recipient, dominated)) buff.Remove();
                    }
                    controller.Descriptor.Alignment.Set(matching[index]);
                    observer.Clear();
                    var directContext = new MechanicsContext(controller, controller.Descriptor, dominated, null, new TargetWrapper(recipient));
                    if (directContext.SourceAbility != null) throw new InvalidOperationException("Terminal-only context unexpectedly has an ability.");
                    var direct = recipient.Buffs.AddBuff(dominated, directContext, TimeSpan.FromMinutes(1));
                    assertions.Add(Assertion(label + "terminal-only", enabled ? "blocked" : "allowed", observer.Describe(),
                        observer.Applications == 1 && observer.CanApply != enabled && (direct == null) == enabled,
                        "real terminal buff delivery with no source ability; shared catalog terminal classification"));
                    direct?.Remove();
                    observer.Clear();
                    var unresolved = CircleLostControllerTerminal(controller, recipient, dominated, observer);
                    assertions.Add(Assertion(label + "unresolved-source", "fail-open", observer.Describe(),
                        observer.Applications == 1 && observer.SourceMissing && observer.CanApply && unresolved != null,
                        "native pending terminal context after permanent controller removal; no source ability or trusted metadata; native faction logic cannot find the removed controller"));
                    unresolved?.Remove();
                    carrier.Remove(); CircleRefresh(area, actors);
                    assertions.Add(Assertion(label + "exit-cleanup", "only this cast removed", "recipient=" + CircleBuffs(recipient, circle.Recipient).Length,
                        CircleBuffs(recipient, circle.Recipient).Length == 0 && area.IsEnded,
                        "native carrier-owned area and derivative ownership"));
                }
            }
            finally {
                EventBus.Unsubscribe(observer); recipient.Stats.Wisdom.BaseValue = wisdom;
                (levelController as IDisposable)?.Dispose();
            }
        }
        private static Buff CircleLostControllerTerminal(UnitEntityData source, UnitEntityData recipient,
            BlueprintBuff terminal, CircleControlCastCapture observer)
        {
            // Constructor(null, owner, ...) substitutes owner as the caster.
            // Model a genuinely unresolved pending application instead: assemble
            // its native recipient context while the controller exists, then
            // permanently remove that request-local controller before dispatch.
            var game = Game.Instance;
            var lost = game.EntityCreator.SpawnUnit(source.Blueprint, source.Position,
                UnityEngine.Quaternion.identity, source.HoldingState);
            Alignment before = recipient.Descriptor.Alignment.Value;
            try {
                game.EntityCreator.Tick();
                lost.Descriptor.CustomName = "KMG_RUNTIME_MAGIC_CIRCLE_RemovedController";
                lost.Descriptor.Alignment.Set(source.Descriptor.Alignment.Value);
                recipient.Descriptor.Alignment.Set(source.Descriptor.Alignment.Value);
                var pending = new MechanicsContext(lost, recipient.Descriptor, terminal, null, new TargetWrapper(recipient));
                lost.Destroy(); game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
                if (!lost.Destroyed || pending.MaybeCaster != null || pending.SourceAbility != null)
                    throw new InvalidOperationException("Permanent-removal context did not become unresolved.");
                var trigger = typeof(BuffCollection).GetMethod("TriggerRuleApplyBuff", BindingFlags.Instance | BindingFlags.NonPublic);
                if (trigger == null) throw new MissingMethodException("Native pending buff dispatch boundary is absent.");
                observer.Clear();
                return (Buff)trigger.Invoke(recipient.Buffs, new object[] { terminal, pending, (TimeSpan?)TimeSpan.FromMinutes(1) });
            }
            finally {
                recipient.Descriptor.Alignment.Set(before);
                if (!lost.Destroyed) { lost.Destroy(); game.EntityDestroyer.Tick(); }
            }
        }
        private static void CircleIncomingCast(UnitEntityData controller, UnitEntityData recipient, Spellbook book, BlueprintAbility spell,
            BlueprintBuff terminal, CircleControlCastCapture observer, List<string> diagnostics)
        {
            // A natural 20 may legitimately save. Bound retries and require an
            // observed failed save plus the actual terminal application attempt.
            // No rule result, random value or internal protection policy is mocked.
            for (int attempt = 0; attempt < 8; attempt++) {
                if (CircleBuffs(recipient, terminal).Length != 0) throw new InvalidOperationException("Incoming control requires an unoccupied terminal.");
                observer.Clear(); book.Rest(); int slots = book.GetSpontaneousSlots(5);
                CircleCast(controller, recipient, new AbilityData(spell, book), diagnostics);
                if (book.GetSpontaneousSlots(5) != slots - 1) throw new InvalidOperationException("Incoming native control spell did not consume exactly one slot.");
                diagnostics.Add("incoming-control:" + controller.Descriptor.Alignment.Value + ":" + observer.Describe());
                if (observer.FailedSaves == 1 && observer.Applications == 1) return;
                if (observer.PassedSaves != 1 || observer.Applications != 0)
                    throw new InvalidOperationException("Ambiguous incoming control delivery: " + observer.Describe());
            }
            throw new InvalidOperationException("Eight native control saves passed; no qualifying control application was observed.");
        }
        private sealed class CircleControlCastCapture : IGlobalRulebookHandler<RuleApplyBuff>, IGlobalRulebookHandler<RuleSavingThrow>
        {
            private readonly UnitEntityData _target; private readonly BlueprintBuff _terminal;
            internal int Applications, FailedSaves, PassedSaves; internal bool CanApply, SourceMissing;
            internal CircleControlCastCapture(UnitEntityData target, BlueprintBuff terminal) { _target = target; _terminal = terminal; }
            internal void Clear() { Applications = FailedSaves = PassedSaves = 0; CanApply = SourceMissing = false; }
            internal string Describe() { return "failedSaves=" + FailedSaves + ";passedSaves=" + PassedSaves + ";applications=" + Applications + ";canApply=" + CanApply + ";sourceMissing=" + SourceMissing; }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt) { if (ReferenceEquals(evt.Initiator, _target) && ReferenceEquals(evt.Blueprint, _terminal)) { Applications++; CanApply = evt.CanApply; SourceMissing = evt.Context != null && evt.Context.MaybeCaster == null && evt.Context.SourceAbility == null; } }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt) { if (ReferenceEquals(evt.Initiator, _target)) { if (evt.IsPassed) PassedSaves++; else FailedSaves++; } }
        }
    }
}
