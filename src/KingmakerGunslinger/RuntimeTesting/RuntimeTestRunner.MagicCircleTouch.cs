using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static void CircleTouchAndMetamagic(UnitEntityData caster, UnitEntityData bearer,
            UnitEntityData hostile, Spellbook book, List<UnitEntityData> actors,
            List<BlueprintUnit> prototypes, List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            // A fresh native hostile actor has no prior domination/faction
            // history from the shared-delivery regressions earlier in this run.
            hostile = CircleSpawn("HostileTouch", bearer.Position, bearer, actors, prototypes, hostile.Blueprint.Faction);
            var circles = BlueprintBootstrap.MagicCircles;
            bool casterCheater = caster.Blueprint.IsCheater, targetCheater = hostile.Blueprint.IsCheater;
            var circle = circles.Single(value => value.Alignment == "Evil");
            int strength = caster.Stats.Strength.BaseValue, dexterity = hostile.Stats.Dexterity.BaseValue,
                wisdom = hostile.Stats.Wisdom.BaseValue;
            var hostilePosition = hostile.Position; var bearerPosition = bearer.Position;
            var capture = new CircleTouchCapture(caster, hostile);
            object levelController = null;
            try {
                hostile.Position = bearer.Position;
                CircleSynchronize(actors); EventBus.Subscribe(capture);
                caster.Blueprint.IsCheater = hostile.Blueprint.IsCheater = false;
                diagnostics.Add("hostile-touch-preflight:targetEnemy=" + hostile.IsEnemy(caster) + ";casterEnemy=" + caster.IsEnemy(hostile) +
                    ";targetAlly=" + hostile.IsAlly(caster) + ";casterAlly=" + caster.IsAlly(hostile));
                if (!hostile.IsEnemy(caster) || !caster.IsEnemy(hostile)) throw new InvalidOperationException("Fresh hostile-touch actors must recognize each other as enemies.");
                // Native stats create bounded, realistic miss/save opportunities.
                // No dice, attack result, save result or protection rule is forced.
                caster.Stats.Strength.BaseValue = 3; hostile.Stats.Dexterity.BaseValue = 50;
                hostile.Stats.Wisdom.BaseValue = 50;
                bool missed = false;
                int spent = 0;
                for (int attempt = 0; attempt < 16 && !missed; attempt++) {
                    book.Rest(); int slots = book.GetSpontaneousSlots(3); capture.Clear();
                    CircleCast(caster, hostile, new AbilityData(circle.Spell, book), diagnostics);
                    if (book.GetSpontaneousSlots(3) != slots - 1 || capture.Attacks != 1)
                        throw new InvalidOperationException("Hostile touch did not produce one native attack and one slot debit: " + capture.Describe());
                    diagnostics.Add("hostile-touch-attempt:" + capture.Describe());
                    missed = capture.Misses == 1;
                    spent = book.GetSpontaneousSlots(3);
                    foreach (var buff in CircleBuffs(hostile, circle.Carrier)) buff.Remove();
                }
                var held = caster.Get<UnitPartTouch>();
                assertions.Add(Assertion("circle-hostile-touch-miss", "one slot spent; charge held; no save or carrier", capture.Describe(),
                    missed && held != null && ReferenceEquals(held.Ability.Data.Blueprint, circle.Delivery) &&
                    capture.Saves == 0 && CircleBuffs(hostile, circle.Carrier).Length == 0,
                    "actual hostile touch attack; bounded native random rolls, no forced outcomes"));
                if (!missed || held == null) throw new InvalidOperationException("No retained native missed-touch charge observed.");
                var original = held.Ability.Data.StickyTouch;
                caster.Stats.Strength.BaseValue = 30; hostile.Stats.Dexterity.BaseValue = 3;
                bool saved = false;
                for (int attempt = 0; attempt < 8 && !saved; attempt++) {
                    capture.Clear(); CircleCast(caster, hostile, held.Ability.Data, diagnostics);
                    diagnostics.Add("circle-held-retry:" + capture.Describe());
                    if (capture.Hits == 0) continue;
                    saved = capture.PassedSaves == 1;
                    break;
                }
                assertions.Add(Assertion("circle-held-touch-save-and-cost", "hit and passed Will consume charge without another slot or carrier", capture.Describe(),
                    saved && book.GetSpontaneousSlots(3) == spent && caster.Get<UnitPartTouch>() == null &&
                    CircleBuffs(hostile, circle.Carrier).Length == 0 && original != null && ReferenceEquals(original.Blueprint, circle.Spell),
                    "retry uses the exact native held AbilityData and original spell context; a successful save negates bearer application"));
                if (!saved) throw new InvalidOperationException("No successful native hostile-bearer save observed.");

                hostile.Stats.Wisdom.BaseValue = 3;
                bool applied = false;
                for (int attempt = 0; attempt < 8 && !applied; attempt++) {
                    capture.Clear();
                    held = caster.Get<UnitPartTouch>();
                    if (held == null) { book.Rest(); spent = book.GetSpontaneousSlots(3) - 1; }
                    CircleCast(caster, hostile, held == null ? new AbilityData(circle.Spell, book) : held.Ability.Data, diagnostics);
                    applied = capture.Hits == 1 && capture.FailedSaves == 1 && CircleBuffs(hostile, circle.Carrier).Length == 1;
                }
                var carrier = CircleBuffs(hostile, circle.Carrier).Single(); var area = CircleArea(carrier);
                CircleRefresh(area, actors); CircleRefresh(area, actors);
                assertions.Add(Assertion("circle-hostile-bearer-application", "failed Will creates one moving emanation for all covered creatures", capture.Describe(),
                    applied && book.GetSpontaneousSlots(3) == spent && caster.Get<UnitPartTouch>() == null && hostile.IsEnemy(caster) &&
                    ReferenceEquals(area.Context.MaybeOwner, hostile) && ReferenceEquals(carrier.Context.MaybeCaster, caster) &&
                    CircleBuffs(hostile, circle.Recipient).Length == 1 && CircleBuffs(bearer, circle.Recipient).Length == 1,
                    "real hostile touch attack and native Will result; hostile bearer remains the area owner"));
                int saves = capture.AllSaves; var deadline = carrier.EndTime;
                bearer.Position += new UnityEngine.Vector3(8, 0, 0); CircleRefresh(area, actors);
                bearer.Position = hostile.Position; CircleRefresh(area, actors);
                assertions.Add(Assertion("circle-hostile-area-no-entry-saves", "no additional saves; original deadline", capture.Describe(),
                    capture.AllSaves == saves && carrier.EndTime == deadline && CircleBuffs(bearer, circle.Recipient).Length == 1,
                    "membership, including entry by an enemy of the bearer, does not repeat the spell's initial Will save"));
                carrier.Remove(); CircleRefresh(area, actors);
                EventBus.Unsubscribe(capture);

                var metacaster = CircleSpawn("MetamagicCaster", bearer.Position, bearer, actors, prototypes);
                metacaster.Stats.Charisma.BaseValue = 30;
                var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "b3a505fb61437dc4097f43c3f8f9a4cf", "native Sorcerer class");
                AdvanceDisposableSpellcaster(metacaster.Descriptor, sorcerer, 14, ref levelController);
                var metaBook = metacaster.Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, sorcerer.Spellbook));
                while (metaBook.CasterLevel < 14) metaBook.AddCasterLevel();
                metaBook.UpdateAllSlotsSize(false); CircleSynchronize(actors);
                foreach (var definition in circles) {
                    metaBook.AddKnown(3, definition.Spell, true);
                    foreach (var flag in new[] { Metamagic.Extend, Metamagic.Quicken, Metamagic.Heighten, Metamagic.Reach }) {
                        var metadata = new MetamagicData { SpellLevelCost = flag == Metamagic.Heighten ? 2 : flag.DefaultCost(), HeightenLevel = flag == Metamagic.Heighten ? 2 : 0 };
                        metadata.Add(flag);
                        var data = new AbilityData(definition.Spell, metaBook) { MetamagicData = metadata };
                        metaBook.Rest(); int level = data.SpellLevel, slots = metaBook.GetSpontaneousSlots(level);
                        var originalPosition = metacaster.Position;
                        if (flag == Metamagic.Reach) metacaster.Position = bearer.Position + new UnityEngine.Vector3(8, 0, 0);
                        CircleSynchronize(actors);
                        CircleCast(metacaster, bearer, data, diagnostics);
                        var buff = CircleBuffs(bearer, definition.Carrier).Single(); var sourceArea = CircleArea(buff);
                        CircleRefresh(sourceArea, actors); CircleRefresh(sourceArea, actors);
                        double duration = 600 * metaBook.CasterLevel * (flag == Metamagic.Extend ? 2 : 1);
                        assertions.Add(Assertion("circle-metamagic-" + definition.Alignment + "-" + flag, "native adjusted slot, action, context and original duration", "level=" + level + ";action=" + data.ActionType + ";fullRound=" + data.RequireFullRoundAction + ";seconds=" + buff.TimeLeft.TotalSeconds,
                            level == 3 + metadata.SpellLevelCost && metaBook.GetSpontaneousSlots(level) == slots - 1 &&
                            buff.Context.HasMetamagic(flag) && sourceArea.Context.HasMetamagic(flag) &&
                            ReferenceEquals(sourceArea.Context.MaybeCaster, metacaster) && ReferenceEquals(sourceArea.Context.MaybeOwner, bearer) &&
                            Math.Abs(buff.TimeLeft.TotalSeconds - duration) < 2 &&
                            data.ActionType == (flag == Metamagic.Quicken ? UnitCommand.CommandType.Swift : UnitCommand.CommandType.Standard) &&
                            data.RequireFullRoundAction == (flag != Metamagic.Quicken) &&
                            buff.Context.Params.SpellLevel == (flag == Metamagic.Heighten ? 5 : 3) &&
                            (flag != Metamagic.Reach || UnityEngine.Vector3.Distance(metacaster.Position, bearer.Position) > 7),
                            "native spontaneous metamagic casting; Reach executes the engine's converted command, ranged-touch rule and isolated native projectile ticks; no real-time animation claim"));
                        buff.Remove(); CircleRefresh(sourceArea, actors); metacaster.Position = originalPosition;
                    }
                }
            }
            finally {
                EventBus.Unsubscribe(capture);
                caster.Blueprint.IsCheater = casterCheater; hostile.Blueprint.IsCheater = targetCheater;
                caster.Stats.Strength.BaseValue = strength; hostile.Stats.Dexterity.BaseValue = dexterity; hostile.Stats.Wisdom.BaseValue = wisdom;
                hostile.Position = hostilePosition; bearer.Position = bearerPosition;
                foreach (var actor in actors)
                    foreach (var definition in circles)
                        foreach (var buff in CircleBuffs(actor, definition.Carrier)) buff.Remove();
                if (levelController != null) levelController.GetType().GetMethod("Cancel").Invoke(levelController, null);
            }
        }

        private sealed class CircleTouchCapture : IGlobalRulebookHandler<RuleAttackRoll>, IGlobalRulebookHandler<RuleSavingThrow>
        {
            private readonly UnitEntityData _caster, _target;
            internal int Attacks, Hits, Misses, PassedSaves, FailedSaves, AllSaves;
            internal string AttackDetails;
            internal int Saves { get { return PassedSaves + FailedSaves; } }
            internal CircleTouchCapture(UnitEntityData caster, UnitEntityData target) { _caster = caster; _target = target; }
            internal void Clear() { Attacks = Hits = Misses = PassedSaves = FailedSaves = AllSaves = 0; AttackDetails = null; }
            internal string Describe() { return "attacks=" + Attacks + ";hits=" + Hits + ";misses=" + Misses + ";passed=" + PassedSaves + ";failed=" + FailedSaves + ";" + AttackDetails; }
            public void OnEventAboutToTrigger(RuleAttackRoll evt) { }
            public void OnEventDidTrigger(RuleAttackRoll evt) {
                if (!ReferenceEquals(evt.Initiator, _caster) || !ReferenceEquals(evt.Target, _target)) return;
                Attacks++; if (evt.IsHit) Hits++; else Misses++;
                AttackDetails = "type=" + evt.AttackType + ";autoHit=" + evt.AutoHit + ";roll=" + evt.Roll + ";bonus=" + evt.AttackBonus + ";ac=" + evt.TargetAC + ";flat=" + evt.IsTargetFlatFooted;
            }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt) {
                AllSaves++;
                if (!ReferenceEquals(evt.Initiator, _target)) return;
                if (evt.IsPassed) PassedSaves++; else FailedSaves++;
            }
        }
    }
}
