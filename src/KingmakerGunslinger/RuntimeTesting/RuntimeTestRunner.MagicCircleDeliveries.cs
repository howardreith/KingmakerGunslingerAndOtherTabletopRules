using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.ProtectionFromAlignment;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void CircleOtherDeliveries(UnitEntityData caster, UnitEntityData bearer,
            UnitEntityData recipient, UnitEntityData controller, Spellbook book,
            List<UnitEntityData> actors, List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var library = BlueprintBootstrap.Library;
            var all = library.GetAllBlueprints().Where(value => value != null).ToArray();
            var matching = new[] { Alignment.LawfulEvil, Alignment.ChaoticGood, Alignment.LawfulNeutral, Alignment.ChaoticNeutral };
            var excluded = new[] { "bd81a3931aa285a4f9844585b5d97e51", "cf6c901fb7acc904e85c63b342e9c949",
                "c7104f7526c4c524f91474614054547e", "41e8a952da7a5c247b3ec1c2dbb73018" }
                .Select(id => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, id,
                    "native excluded Cause Fear / Confusion / Hold Person / Hold Monster")).ToArray();
            foreach (var spell in excluded) diagnostics.Add("circle-excluded-native-identity:" + spell.AssetGuid + ":" + spell.name);
            var optional = MentalControlCatalogDefaults.All.Where(entry =>
                entry.ContentSource == MentalControlContentSource.CallOfTheWild && entry.Kind == MentalControlBlueprintKind.Buff)
                .Select(entry => all.OfType<BlueprintBuff>().SingleOrDefault(value => value.AssetGuid == entry.Guid))
                .Where(value => value != null).ToArray();
            bool enhancement = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
            var dominate = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                "d7cbd2004ce66a042aeab2e95a3c5c61", "native rule source for equipment saves");
            var ringBlueprint = BlueprintLibraryLookup.RequireExact<BlueprintItemEquipmentRing>(library,
                "31315100c28e6a2418396fb152466fcd", "native Ring of Protection +3");
            var cloakBlueprint = all.OfType<BlueprintItemEquipmentShoulders>().Single(value => value.name == "CloakOfResistance3");
            ItemEntity ring = null, cloak = null;
            int wisdom = recipient.Stats.Wisdom.BaseValue;
            var alignment = controller.Descriptor.Alignment.Value;
            try {
                if (recipient.Body.Ring1.MaybeItem != null || recipient.Body.Shoulders.MaybeItem != null)
                    throw new InvalidOperationException("Disposable recipient must have empty equipment slots.");
                recipient.Stats.Wisdom.BaseValue = 3;
                controller.Descriptor.Alignment.Set(Alignment.TrueNeutral);
                int baseAc = CircleAttackAC(controller, recipient), baseSave = CircleSave(controller, recipient, dominate);
                ring = ringBlueprint.CreateEntity(); cloak = cloakBlueprint.CreateEntity();
                recipient.Body.Ring1.InsertItem(ring); recipient.Body.Shoulders.InsertItem(cloak);
                diagnostics.Add("circle-equipment:" + ringBlueprint.AssetGuid + ":" + cloakBlueprint.AssetGuid);
                assertions.Add(Assertion("circle-native-stronger-equipment", "+3 native ring AC and cloak saves", "ac=" + CircleAttackAC(controller, recipient) + ";save=" + CircleSave(controller, recipient, dominate),
                    ReferenceEquals(recipient.Body.Ring1.MaybeItem, ring) && ReferenceEquals(recipient.Body.Shoulders.MaybeItem, cloak) &&
                    CircleAttackAC(controller, recipient) == baseAc + 3 && CircleSave(controller, recipient, dominate) == baseSave + 3,
                    "actual equipped native RingOfProtection3 and CloakOfResistance3 entities"));
                for (int index = 0; index < BlueprintBootstrap.MagicCircles.Length; index++) {
                    var circle = BlueprintBootstrap.MagicCircles[index];
                    controller.Descriptor.Alignment.Set(matching[index]);
                    book.Rest(); CircleCast(caster, bearer, new AbilityData(circle.Spell, book), diagnostics);
                    var carrier = CircleBuffs(bearer, circle.Carrier).Single(); var area = CircleArea(carrier);
                    try {
                        CircleRefresh(area, actors); CircleRefresh(area, actors);
                        assertions.Add(Assertion("circle-stronger-equipment-" + circle.Alignment, "stronger typed bonuses remain +3", "ac=" + CircleAttackAC(controller, recipient) + ";save=" + CircleSave(controller, recipient, dominate),
                            CircleBuffs(recipient, circle.Recipient).Length == 1 && CircleAttackAC(controller, recipient) == baseAc + 3 && CircleSave(controller, recipient, dominate) == baseSave + 3,
                            "actual matching-alignment attack and saving-throw rules; separate native equipment sources"));
                        foreach (var spell in excluded) {
                            // Use the installed spell's actual terminal actions,
                            // avoiding an area Confusion cast into the working
                            // save's party. This tests public native applications,
                            // not saving-throw success or this spell's casting UI.
                            var terminals = ExpandedSummoningObjects<ContextActionApplyBuff>(spell.ComponentsArray)
                                .Select(action => action.Buff).Where(value => value != null).Distinct().ToArray();
                            if (terminals.Length == 0) throw new InvalidOperationException("Excluded spell lacks native terminal actions: " + spell.name);
                            foreach (var terminal in terminals) {
                                var observer = new CircleControlCastCapture(recipient, terminal);
                                EventBus.Subscribe(observer);
                                Buff applied = null;
                                try {
                                    var context = new AbilityExecutionContext(new AbilityData(spell, controller.Descriptor),
                                        new AbilityParams { CasterLevel = 12, SpellLevel = 5, DC = 25 }, new TargetWrapper(recipient), Rulebook.CurrentContext);
                                    applied = recipient.Buffs.AddBuff(terminal, context, TimeSpan.FromSeconds(18));
                                    assertions.Add(Assertion("circle-excluded-" + circle.Alignment + "-" + spell.name + "-" + terminal.AssetGuid,
                                        "native terminal actually applies", observer.Describe(),
                                        observer.Applications == 1 && observer.CanApply && applied != null && applied.Active,
                                        "public AddBuff with actual spell ability context and its own terminal blueprint; no blanket descriptor immunity"));
                                }
                                finally { applied?.Remove(); EventBus.Unsubscribe(observer); }
                            }
                        }
                        foreach (var terminal in optional) {
                            var observer = new CircleControlCastCapture(recipient, terminal);
                            EventBus.Subscribe(observer);
                            Buff applied = null;
                            try {
                                applied = recipient.Buffs.AddBuff(terminal,
                                    new MechanicsContext(controller, controller.Descriptor, terminal, null, new TargetWrapper(recipient)), TimeSpan.FromSeconds(18));
                                assertions.Add(Assertion("circle-optional-terminal-" + circle.Alignment + "-" + terminal.name,
                                    enhancement ? "cataloged terminal blocked" : "cataloged terminal applies", observer.Describe(),
                                    observer.Applications == 1 && observer.CanApply != enhancement && (applied == null) == enhancement,
                                    "existing shared optional catalog, real public terminal delivery without a source ability"));
                                applied?.Remove(); applied = null; observer.Clear();
                                controller.Descriptor.Alignment.Set(Alignment.TrueNeutral);
                                applied = recipient.Buffs.AddBuff(terminal,
                                    new MechanicsContext(controller, controller.Descriptor, terminal, null, new TargetWrapper(recipient)), TimeSpan.FromSeconds(18));
                                assertions.Add(Assertion("circle-optional-positive-" + circle.Alignment + "-" + terminal.name,
                                    "wrong-alignment terminal really applies", observer.Describe(),
                                    observer.Applications == 1 && observer.CanApply && applied != null,
                                    "same actual terminal delivery; only controller alignment changes"));
                            }
                            finally { applied?.Remove(); EventBus.Unsubscribe(observer); controller.Descriptor.Alignment.Set(matching[index]); }
                        }
                        var deadline = carrier.EndTime;
                        recipient.Translocate(bearer.Position + new Vector3(10f, 0, 0), null); CircleRefresh(area, actors);
                        bool outOfRange = CircleBuffs(recipient, circle.Recipient).Length == 0;
                        recipient.Translocate(bearer.Position, null); CircleRefresh(area, actors);
                        assertions.Add(Assertion("circle-native-translocate-" + circle.Alignment, "exit/re-entry rebuilds one contribution without timer reset", "outside=" + outOfRange,
                            outOfRange && CircleBuffs(recipient, circle.Recipient).Length == 1 && carrier.EndTime == deadline,
                            "actual native UnitEntityData.Translocate and area membership; this is same-scene teleportation, not cross-area travel"));
                    }
                    finally { carrier.Remove(); CircleRefresh(area, actors); }
                    assertions.Add(Assertion("circle-weaker-removal-" + circle.Alignment, "native equipment still supplies +3", "ac=" + CircleAttackAC(controller, recipient) + ";save=" + CircleSave(controller, recipient, dominate),
                        CircleBuffs(recipient, circle.Recipient).Length == 0 && CircleAttackAC(controller, recipient) == baseAc + 3 && CircleSave(controller, recipient, dominate) == baseSave + 3,
                        "removing the circle leaves exact equipped ring and cloak sources intact"));
                }
                diagnostics.Add("circle-optional-terminals-present=" + optional.Length);
            }
            finally {
                if (ring != null) { recipient.Body.Ring1.RemoveItem(false); ring.Dispose(); }
                if (cloak != null) { recipient.Body.Shoulders.RemoveItem(false); cloak.Dispose(); }
                recipient.Stats.Wisdom.BaseValue = wisdom; controller.Descriptor.Alignment.Set(alignment);
            }
            CircleSummonedController(caster, bearer, recipient, book, actors, assertions, diagnostics);
        }

        private void CircleSummonedController(UnitEntityData summoner, UnitEntityData bearer,
            UnitEntityData recipient, Spellbook book, List<UnitEntityData> actors,
            List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var library = BlueprintBootstrap.Library; var game = Game.Instance;
            var unit = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library, "0c908145873f4b67a188397ca5f46da1", "Expanded Summoning Succubus");
            var delivery = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, "1c271f1108234671b84a8d3ecfec36c6", "registered Succubus summon variant");
            var spell = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, "1662d63944d94cdeaa62562dc9ac9349", "Succubus domination ability");
            var terminal = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "6e1f6eb3e773451dbda9e0ecd07486d9", "Succubus domination terminal");
            var data = new AbilityData(delivery, summoner.Descriptor);
            var context = new AbilityExecutionContext(data, new AbilityParams { CasterLevel = 12, SpellLevel = 6 }, new TargetWrapper(bearer.Position), Rulebook.CurrentContext);
            UnitEntityData summoned = null; var areas = new List<AreaEffectEntityData>();
            var observer = new CircleControlCastCapture(recipient, terminal);
            var summonerAlignment = summoner.Descriptor.Alignment.Value;
            int wisdom = recipient.Stats.Wisdom.BaseValue;
            try {
                summoner.Descriptor.Alignment.Set(Alignment.LawfulGood); recipient.Stats.Wisdom.BaseValue = 3;
                var rule = new RuleSummonUnit(summoner, unit, bearer.Position, 10.Rounds(), 12) { Context = context, Reason = context };
                Rulebook.Trigger(rule); summoned = rule.SummonedUnit;
                for (int tick = 0; tick < 16; tick++) game.EntityCreator.Tick();
                if (summoned == null) throw new InvalidOperationException("Native summon rule did not create a controller.");
                actors.Add(summoned);
                // End only the request-owned appearance staging buff. No
                // defense, control, source alignment or spell action is changed.
                foreach (var buff in CircleBuffs(summoned, BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff)) buff.Remove();
                summoned.Translocate(bearer.Position, null); CircleSynchronize(actors);
                assertions.Add(Assertion("circle-native-summoned-controller", "Chaotic Evil controller linked to Lawful Good summoner", "source=" + summoned.Descriptor.Alignment.Value + ";summoner=" + summoner.Descriptor.Alignment.Value,
                    summoned.Descriptor.Alignment.Value == Alignment.ChaoticEvil && ReferenceEquals(summoned.Get<UnitPartSummonedMonster>()?.Summoner, summoner) &&
                    CircleBuffs(summoned, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff).Length == 1,
                    "actual native RuleSummonUnit with original summon context and lifecycle; no summon spell slot claim"));
                EventBus.Subscribe(observer);
                var ability = new AbilityData(summoned.Descriptor.Abilities.GetAbility(spell));
                Action incoming = () => {
                    for (int attempt = 0; attempt < 8; attempt++) {
                        observer.Clear(); CircleCast(summoned, recipient, ability, diagnostics);
                        if (observer.FailedSaves == 1 && observer.Applications == 1) return;
                        if (observer.PassedSaves != 1 || observer.Applications != 0) throw new InvalidOperationException("Ambiguous summoned control: " + observer.Describe());
                    }
                    throw new InvalidOperationException("No failed native Succubus save observed after eight attempts.");
                };
                incoming();
                assertions.Add(Assertion("circle-succubus-unprotected-positive", "actual custom control applies", observer.Describe(),
                    observer.CanApply && CircleBuffs(recipient, terminal).Length == 1,
                    "native UnitUseAbility and ContextActionSuccubusDominate, observed failed Will and real RuleApplyBuff"));
                foreach (var applied in CircleBuffs(recipient, terminal)) applied.Remove();
                foreach (var circle in BlueprintBootstrap.MagicCircles) {
                    book.Rest(); CircleCast(summoner, bearer, new AbilityData(circle.Spell, book), diagnostics);
                    var carrier = CircleBuffs(bearer, circle.Carrier).Single(); var area = CircleArea(carrier); areas.Add(area);
                    try {
                        CircleRefresh(area, actors); CircleRefresh(area, actors);
                        assertions.Add(Assertion("circle-covers-summon-" + circle.Alignment, "summoned creature receives its own owned contribution", CircleBuffs(summoned, circle.Recipient).Length.ToString(),
                            CircleBuffs(summoned, circle.Recipient).Length == 1,
                            "native area membership includes a real linked summon"));
                        incoming(); bool blocked = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity && (circle.Alignment == "Evil" || circle.Alignment == "Chaos");
                        assertions.Add(Assertion("circle-succubus-source-" + circle.Alignment, blocked ? "control blocked by controller alignment" : "control applies despite opposite summoner alignment", observer.Describe(),
                            observer.CanApply != blocked && CircleBuffs(recipient, terminal).Length == (blocked ? 0 : 1),
                            "actual Chaotic Evil summoned controller; Lawful Good summoner; existing shared source resolver and catalog"));
                    }
                    finally { foreach (var applied in CircleBuffs(recipient, terminal)) applied.Remove(); carrier.Remove(); CircleRefresh(area, actors); }
                }
                var evil = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
                book.Rest(); CircleCast(summoner, summoned, new AbilityData(evil.Spell, book), diagnostics);
                var ownedCarrier = CircleBuffs(summoned, evil.Carrier).Single(); var ownedArea = CircleArea(ownedCarrier); areas.Add(ownedArea);
                CircleRefresh(ownedArea, actors); CircleRefresh(ownedArea, actors);
                bool before = CircleBuffs(recipient, evil.Recipient).Length == 1;
                CleanupExpandedSummoningUnit(summoned); game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick(); actors.Remove(summoned);
                ownedArea.Tick(); game.EntityDestroyer.Tick();
                assertions.Add(Assertion("circle-permanent-bearer-despawn", "despawn removes this carrier and all its area contributions", "before=" + before + ";ended=" + ownedArea.IsEnded,
                    before && summoned.Destroyed && ownedArea.IsEnded && CircleBuffs(recipient, evil.Recipient).Length == 0,
                    "native summoned-unit removal while bearing an actually cast circle; no recipient cleanup by blueprint"));
            }
            finally {
                EventBus.Unsubscribe(observer);
                foreach (var applied in CircleBuffs(recipient, terminal)) applied.Remove();
                if (summoned != null && !summoned.Destroyed) { CleanupExpandedSummoningUnit(summoned); game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick(); actors.Remove(summoned); }
                foreach (var area in areas.Where(value => !value.Destroyed)) { area.ForceEnd(); area.Tick(); }
                game.EntityDestroyer.Tick();
                summoner.Descriptor.Alignment.Set(summonerAlignment); recipient.Stats.Wisdom.BaseValue = wisdom;
            }
        }
    }
}
