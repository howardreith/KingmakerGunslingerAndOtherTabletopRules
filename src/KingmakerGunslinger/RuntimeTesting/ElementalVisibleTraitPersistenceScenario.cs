using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private void PrepareBreezePersistence(ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                var trait = ExpectedPersistenceTraits(fixture, fixture.Heritage)
                    .SingleOrDefault(value => value.Definition.Id == ElementalAlternateTraitId.BreezeKissed);
                if (trait == null) return;
                if (!IsFixtureUnit(unit, fixture) || !Game.Instance.IsPaused)
                    throw new InvalidOperationException("Breeze persistence requires the exact paused disposable actor.");
                var resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
                var calmed = trait.Mechanics().OfType<BlueprintBuff>().Single();
                if (unit.Descriptor.Resources.GetResourceAmount(resource) != 1)
                    throw new InvalidOperationException("Breeze persistence setup requires one unspent native gust.");
                if (fixture.Gender == Gender.Male)
                {
                    if (unit.Descriptor.HasFact(calmed)) throw new InvalidOperationException("Spent-gust fixture unexpectedly owns voluntary calm.");
                    unit.Descriptor.Resources.Spend(resource, 1);
                    return;
                }
                // A rest may retain voluntary calm. Preserve that native choice;
                // only use the actual Calm ability when it is absent.
                if (unit.Descriptor.HasFact(calmed)) return;
                var blueprint = trait.Mechanics().OfType<BlueprintAbility>().Single(value => value.name.EndsWith("_CalmWinds", StringComparison.Ordinal));
                AbilityData data = RequireAbility(unit, blueprint);
                var target = new TargetWrapper(unit);
                if (!data.IsAvailable || !data.CanTarget(target)) throw new InvalidOperationException("Native Calm ability is unavailable.");
                UnitUseAbility command = ElementalUndineFeatScenario.CreateCommand(data, target, unit);
                bool detached = false;
                try
                {
                    ElementalUndineFeatScenario.InvokeCommandAction(command);
                    if (command.ExecutionProcess != null) ElementalUndineFeatScenario.CompleteProcess(command.ExecutionProcess, out detached);
                    ElementalUndineFeatScenario.InvokeCommandEnded(command, false);
                    if (command.ExecutionProcess == null || detached || !unit.Descriptor.HasFact(calmed) ||
                        unit.Descriptor.Resources.GetResourceAmount(resource) != 1)
                        throw new InvalidOperationException("Native Calm failed to establish the exact saved voluntary state.");
                }
                finally
                {
                    if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Detach();
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                }
            }

            private JObject RecordBreezePersistence(ElementalPersistenceFixture fixture, UnitEntityData unit,
                ICollection<ElementalAlternateTraitBlueprints> expectedTraits, string phase)
            {
                if (fixture.Blueprints.AlternateTraits.Race != ElementalHeritageRace.Sylph) return null;
                var trait = fixture.Blueprints.AlternateTraits.Require(ElementalAlternateTraitId.BreezeKissed);
                bool expected = expectedTraits.Contains(trait), afterRest = phase == "module-off-after-rest";
                var resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
                var calmed = trait.Mechanics().OfType<BlueprintBuff>().Single();
                var buffs = unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, calmed)).ToArray();
                int count = unit.Descriptor.Resources.PersistantResources.Count(value => value != null && ReferenceEquals(value.Blueprint, resource));
                int amount = unit.Descriptor.Resources.GetResourceAmount(resource);
                int expectedAmount = expected && (afterRest || fixture.Gender == Gender.Female) ? 1 : 0;
                int expectedCalm = expected && fixture.Gender == Gender.Female ? 1 : 0;
                bool exact = count == (expected ? 1 : 0) && amount == expectedAmount &&
                    (afterRest ? buffs.Length <= (expected ? 1 : 0) : buffs.Length == expectedCalm) &&
                    buffs.All(value => value.Active && !value.IsSuppressed) &&
                    trait.Mechanics().OfType<BlueprintAbility>().All(ability =>
                        unit.Descriptor.Abilities.Enumerable.Count(value => ReferenceEquals(value.Blueprint, ability)) ==
                        (expected && ability.Parent == null ? 1 : 0));
                var record = new JObject { ["fixture"] = fixture.Label, ["phase"] = phase, ["expected"] = expected,
                    ["resourceGuid"] = resource.AssetGuid, ["resourceCount"] = count, ["amount"] = amount,
                    ["expectedAmount"] = expectedAmount, ["calmGuid"] = calmed.AssetGuid, ["calmCount"] = buffs.Length,
                    ["nativeRestCalmObserved"] = afterRest, ["exact"] = exact };
                Add(_assertions, "elemental-breeze-persistence-" + phase + "-" + fixture.Label,
                    "exact saved gust expenditure and voluntary calm, or exact absence", record.ToString(Newtonsoft.Json.Formatting.None),
                    exact, "live loaded provider, ability, resource and buff identities; no fixture reconciliation");
                if (!exact) throw new InvalidOperationException("Breeze persistence diverged: " + record);
                return record;
            }

            private JObject RecordVisibleStatPersistence(ElementalPersistenceFixture fixture, UnitEntityData unit,
                ICollection<ElementalAlternateTraitBlueprints> expectedTraits, string phase)
            {
                var rows = new JArray();
                bool exact = true;
                foreach (var trait in fixture.Blueprints.AlternateTraits.Traits().Where(value => new[] {
                    ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.GraniteSkin,
                    ElementalAlternateTraitId.LikeTheWind, ElementalAlternateTraitId.WhisperingWind }.Contains(value.Definition.Id)))
                {
                    var id = trait.Definition.Id;
                    StatType stat = id == ElementalAlternateTraitId.WildfireHeart ? StatType.Initiative :
                        id == ElementalAlternateTraitId.GraniteSkin ? StatType.AC :
                        id == ElementalAlternateTraitId.LikeTheWind ? StatType.Speed : StatType.SkillStealth;
                    int amount = id == ElementalAlternateTraitId.GraniteSkin ? 1 : id == ElementalAlternateTraitId.LikeTheWind ? 5 : 4;
                    var descriptor = id == ElementalAlternateTraitId.GraniteSkin ? ModifierDescriptor.NaturalArmor : ModifierDescriptor.Racial;
                    var modifiers = unit.Stats.GetStat(stat).Modifiers.Where(value => value.Source != null && ReferenceEquals(value.Source.Blueprint, trait.Provider)).ToArray();
                    int expectedCount = expectedTraits.Contains(trait) ? 1 : 0;
                    bool rowExact = modifiers.Length == expectedCount && modifiers.All(value => value.ModValue == amount && value.ModDescriptor == descriptor);
                    exact &= rowExact;
                    rows.Add(new JObject { ["trait"] = id.ToString(), ["providerGuid"] = trait.Provider.AssetGuid, ["stat"] = stat.ToString(),
                        ["expectedCount"] = expectedCount, ["count"] = modifiers.Length, ["expectedAmount"] = amount,
                        ["amounts"] = new JArray(modifiers.Select(value => value.ModValue)), ["exact"] = rowExact });
                }
                RecordVisibleConditionalPersistence(fixture, unit, expectedTraits, rows, phase);
                var record = new JObject { ["fixture"] = fixture.Label, ["phase"] = phase, ["stats"] = rows, ["exact"] = exact };
                if (!exact) throw new InvalidOperationException("A saved visible passive modifier was lost or duplicated: " + record);
                return record;
            }

            private void RecordVisibleConditionalPersistence(ElementalPersistenceFixture fixture, UnitEntityData unit,
                ICollection<ElementalAlternateTraitBlueprints> expectedTraits, JArray rows, string phase)
            {
                if (!IsFixtureUnit(unit, fixture) || !Game.Instance.IsPaused)
                    throw new InvalidOperationException("Conditional persistence checks require the exact paused disposable actor; identity=" +
                        IsFixtureUnit(unit, fixture) + ";paused=" + Game.Instance.IsPaused + ";phase=" + phase + ".");
                var temporary = new List<UnityEngine.Object>();
                var random = UnityEngine.Random.state;
                try
                {
                    foreach (var trait in fixture.Blueprints.AlternateTraits.Traits().Where(value =>
                        value.Definition.Id == ElementalAlternateTraitId.ForgeHardened ||
                        value.Definition.Id == ElementalAlternateTraitId.Secretive))
                    {
                        bool expected = expectedTraits.Contains(trait);
                        var forge = trait.Definition.Id == ElementalAlternateTraitId.ForgeHardened;
                        var abilities = new[] {
                            PersistenceContextAbility(SpellDescriptor.Fatigue, SpellSchool.Necromancy, null, temporary),
                            PersistenceContextAbility(SpellDescriptor.Exhausted, SpellSchool.None, null, temporary),
                            PersistenceContextAbility(SpellDescriptor.None, SpellSchool.Enchantment, null, temporary),
                            PersistenceContextAbility(SpellDescriptor.None, SpellSchool.Divination, null, temporary),
                            PersistenceContextAbility(SpellDescriptor.Poison, SpellSchool.Necromancy, null, temporary) };
                        foreach (SavingThrowType save in new[] { SavingThrowType.Fortitude, SavingThrowType.Reflex, SavingThrowType.Will })
                        {
                            var stat = save == SavingThrowType.Fortitude ? unit.Stats.SaveFortitude :
                                save == SavingThrowType.Reflex ? unit.Stats.SaveReflex : unit.Stats.SaveWill;
                            var modifiers = stat.Modifiers.ToArray();
                            int ordinary = PersistenceConditionalSave(unit, unit, null, save);
                            var observed = abilities.Select(ability => PersistenceConditionalSave(unit, unit, ability, save) - ordinary).ToArray();
                            var wanted = expected ? forge ? new[] { 2, 2, 0, 0, 0 } : new[] { 0, 0, 2, 2, 0 } : new int[5];
                            bool exact = observed.SequenceEqual(wanted) && modifiers.SequenceEqual(stat.Modifiers) &&
                                PersistenceConditionalSave(unit, unit, null, save) == ordinary;
                            rows.Add(new JObject { ["trait"] = trait.Definition.Id.ToString(), ["save"] = save.ToString(),
                                ["observedBonuses"] = new JArray(observed), ["expectedBonuses"] = new JArray(wanted), ["exact"] = exact });
                            if (!exact) throw new InvalidOperationException("A loaded conditional save provider diverged or leaked a modifier.");
                        }
                    }
                    if (fixture.Blueprints.AlternateTraits.Race == ElementalHeritageRace.Ifrit &&
                        (phase == "module-off-after-rest" || phase == "module-restored-source-before-respec"))
                        RecordBrazenPersistenceAttack(fixture, unit, expectedTraits, rows);
                    if (fixture.Blueprints.AlternateTraits.Race == ElementalHeritageRace.Sylph)
                    {
                        var trait = fixture.Blueprints.AlternateTraits.Require(ElementalAlternateTraitId.ThunderousResilience);
                        bool expected = expectedTraits.Contains(trait);
                        int wounds = unit.Damage;
                        var buffs = unit.Buffs.Enumerable.ToArray();
                        if (unit.Stats.HitPoints.ModifiedValue - wounds <= 6)
                            throw new InvalidOperationException("The disposable sonic resistance control lacks six safe hit points.");
                        try
                        {
                            var packet = new EnergyDamage(new DiceFormula(0, DiceType.D6), DamageEnergyType.Sonic) { PreRolledValue = 6 };
                            var rule = Rulebook.Trigger(new RuleDealDamage(unit, unit, new DamageBundle(packet)));
                            int actual = rule.ResultDamage.Sum(value => value.FinalValue), wanted = expected ? 1 : 6;
                            bool exact = actual == wanted && unit.Damage == wounds + wanted && buffs.SequenceEqual(unit.Buffs.Enumerable);
                            rows.Add(new JObject { ["trait"] = trait.Definition.Id.ToString(), ["actualSonicDamage"] = actual,
                                ["expectedSonicDamage"] = wanted, ["exact"] = exact });
                            if (!exact) throw new InvalidOperationException("A loaded sonic resistance provider diverged.");
                        }
                        finally { unit.Damage = wounds; }
                    }
                }
                finally
                {
                    foreach (var value in temporary.AsEnumerable().Reverse()) UnityEngine.Object.DestroyImmediate(value);
                    UnityEngine.Random.state = random;
                }
            }


            private void RecordBrazenPersistenceAttack(ElementalPersistenceFixture fixture, UnitEntityData unit,
                ICollection<ElementalAlternateTraitBlueprints> expectedTraits, JArray rows)
            {
                var trait = fixture.Blueprints.AlternateTraits.Require(ElementalAlternateTraitId.BrazenFlame);
                bool expected = expectedTraits.Contains(trait);
                var targetFixture = _fixtures.Single(value => value.Blueprints.AlternateTraits.Race == ElementalHeritageRace.Undine &&
                    value.Gender == Gender.Male && value.Heritage.Definition.Id == ElementalHeritageId.GeneralUndine);
                var target = Snapshot(_allUnits).OfType<UnitEntityData>().SingleOrDefault(value => IsFixtureUnit(value, targetFixture));
                if (target == null || target.IsInCombat || unit.IsInCombat || target.HPLeft <= 8)
                    throw new InvalidOperationException("The exact disposable Brazen control target is absent or unsafe.");
                var weapon = unit.Body.EmptyHandWeapon;
                if (weapon == null || !weapon.Blueprint.IsUnarmed || !weapon.Blueprint.IsMelee)
                    throw new InvalidOperationException("The native Brazen control lacks its exact empty-hand weapon.");
                var ownBuffs = unit.Buffs.Enumerable.ToArray();
                var targetBuffs = target.Buffs.Enumerable.ToArray();
                int wounds = target.Damage;
                try
                {
                    var attack = Rulebook.Trigger(new RuleAttackWithWeapon(unit, target, weapon, 0) { AutoHit = true });
                    var damage = attack.MeleeDamage;
                    var fire = damage == null ? new EnergyDamage[0] : damage.DamageBundle.OfType<EnergyDamage>()
                        .Where(value => value.EnergyType == DamageEnergyType.Fire).ToArray();
                    bool exact = attack.AttackRoll != null && attack.AttackRoll.IsHit && damage != null &&
                        fire.Length == (expected ? 1 : 0) && fire.All(value => value.PreRolledValue == 1) &&
                        ownBuffs.SequenceEqual(unit.Buffs.Enumerable) && targetBuffs.SequenceEqual(target.Buffs.Enumerable);
                    if (damage != null) Rulebook.Trigger(new RulePrepareDamage(damage));
                    exact &= damage != null && damage.DamageBundle.OfType<EnergyDamage>().Count(value => value.EnergyType == DamageEnergyType.Fire) == fire.Length;
                    rows.Add(new JObject { ["trait"] = trait.Definition.Id.ToString(), ["nativeUnarmedAttack"] = true,
                        ["firePackets"] = fire.Length, ["expectedFirePackets"] = expected ? 1 : 0, ["exact"] = exact });
                    if (!exact) throw new InvalidOperationException("A loaded Brazen provider diverged, consumed another saved buff, or duplicated damage.");
                }
                finally { target.Damage = wounds; }
            }

        private static int PersistenceConditionalSave(UnitEntityData unit, UnitEntityData source,
            BlueprintAbility ability, SavingThrowType save)
        {
            var rule = new RuleSavingThrow(unit, save, 100);
            if (ability == null) Rulebook.Trigger(rule);
            else
            {
                var context = new MechanicsContext(source, source.Descriptor,
                    ability, null, new TargetWrapper(unit));
                rule.Reason = context;
                context.TriggerRule(rule);
            }
            return rule.StatValue;
        }

        private static BlueprintAbility PersistenceContextAbility(SpellDescriptor descriptor,
            SpellSchool school, BlueprintAbility parent,
            ICollection<UnityEngine.Object> temporary)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintAbility>();
            var descriptors = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            var spell = ScriptableObject.CreateInstance<SpellComponent>();
            descriptors.Descriptor = descriptor;
            spell.School = school;
            ability.name = "KMG_Runtime_TraitSave_" + descriptor + "_" + school;
            ability.Type = AbilityType.Spell;
            ability.Parent = parent;
            ability.ComponentsArray = new BlueprintComponent[] { descriptors, spell };
            temporary.Add(descriptors);
            temporary.Add(spell);
            temporary.Add(ability);
            return ability;
        }

        }
    }
}