using System;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private readonly JArray _traitPersistenceRecords = new JArray();

            private static readonly ElementalAlternateTraitId[] BloodPersistenceTraits =
            {
                ElementalAlternateTraitId.FireInTheBlood,
                ElementalAlternateTraitId.StoneInTheBlood,
                ElementalAlternateTraitId.StormInTheBlood
            };

            private void PrepareTraitPersistenceTransientState()
            {
                if (!Game.Instance.IsPaused)
                    throw new InvalidOperationException("Trait persistence requires the existing guarded pre-save pause.");
                foreach (ElementalPersistenceFixture fixture in _fixtures)
                {
                    UnitEntityData unit = _createdUnits.Single(value => IsFixtureUnit(value, fixture));
                    ArmBloodPersistence(fixture, unit);
                    PrepareCrystallinePersistence(fixture, unit);
                    RecordTraitPersistence(fixture, unit, 1, 1, true, "prepare-immediately-before-save");
                    if (!EfreetiPersistenceBuffExact(fixture, unit,
                        PersistenceSlaTrait(fixture, fixture.Heritage) == null ? 0 : 1))
                        throw new InvalidOperationException(fixture.Label + " lost its native Efreeti effect before save.");
                }
                int traitFixtures = _fixtures.Count(value => ExpectedPersistenceTraits(value, value.Heritage).Length != 0);
                int combinedFixtures = _fixtures.Count(value => ExpectedPersistenceTraits(value, value.Heritage).Length == 2);
                int bloodFixtures = _fixtures.Count(value => PersistenceBloodTrigger(value) != null);
                Add(_assertions, "elemental-traits-eight-trait-save-inventory",
                    "18 native-selected trait fixtures, six legal two-trait Ifrits, eight partially spent blood buffs, eight distinct traits",
                    "traitFixtures=" + traitFixtures + ";combinedFixtures=" + combinedFixtures + ";bloodFixtures=" + bloodFixtures,
                    traitFixtures == 18 && combinedFixtures == 6 && bloodFixtures == 8 && _fixtures
                        .SelectMany(value => ExpectedPersistenceTraits(value, value.Heritage))
                        .Select(value => value.Definition.Id).Distinct().Count() == 8,
                    "pure eight-trait matrix, native selections/commands, real blood damage/ticks; native deflection-resource expenditure and consent setup");
            }

            private ElementalBloodDamageTrigger PersistenceBloodTrigger(ElementalPersistenceFixture fixture)
            {
                return ExpectedPersistenceTraits(fixture, fixture.Heritage)
                    .SelectMany(trait => trait.Provider.ComponentsArray)
                    .OfType<ElementalBloodDamageTrigger>().SingleOrDefault();
            }

            private void ArmBloodPersistence(ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                ElementalBloodDamageTrigger trigger = PersistenceBloodTrigger(fixture);
                if (trigger == null) return;
                if (!IsFixtureUnit(unit, fixture) || !Game.Instance.IsPaused)
                    throw new InvalidOperationException("Only the exact paused disposable fixture can receive blood persistence setup.");
                UnitPartElementalBloodCapacity capacity = unit.Descriptor.Get<UnitPartElementalBloodCapacity>();
                ElementalAlternateTraitId trait = (ElementalAlternateTraitId)trigger.Trait;
                if (capacity == null || capacity.Spent(trait) != 0 ||
                    unit.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, trigger.HealingBuff)))
                    throw new InvalidOperationException(fixture.Label + " must begin blood setup with unspent capacity and no active blood buff.");
                TimeSpan gameTime = Game.Instance.TimeController.GameTime;
                unit.Damage = 1;
                RuleDealDamage damage = Rulebook.Trigger(new RuleDealDamage(unit, unit,
                    new DamageBundle(new EnergyDamage(new DiceFormula(0, DiceType.D6), trigger.Energy)
                        { PreRolledValue = 3 })));
                Buff buff = unit.Buffs.Enumerable.SingleOrDefault(value => ReferenceEquals(value.Blueprint, trigger.HealingBuff));
                if (damage.Damage != 0 || buff == null || !damage.ResultDamage.Any(value => value.ValueWithoutReduction == 3))
                    throw new InvalidOperationException(fixture.Label + " did not trigger its native blood buff from resisted matching damage.");

                // Isolate only this fixture's scheduler boundary. Never advance
                // the campaign clock or invoke the production effect component.
                PropertyInfo nextTick = typeof(Buff).GetProperty("NextTickTime",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (nextTick == null || nextTick.PropertyType != typeof(TimeSpan) || !nextTick.CanWrite)
                    throw new InvalidOperationException("Native Buff.NextTickTime setter is unavailable.");
                nextTick.SetValue(buff, gameTime, null);
                unit.Buffs.UpdateNextEvent();
                unit.Buffs.Tick();
                bool exact = unit.Damage == 0 && capacity.Spent(trait) == 1 &&
                    capacity.Remaining(trait) == unit.Descriptor.Progression.CharacterLevel * 2 - 1 &&
                    buff.RoundNumber == 1 && buff.EndTime > gameTime &&
                    (TimeSpan)nextTick.GetValue(buff, null) > gameTime &&
                    Game.Instance.TimeController.GameTime == gameTime;
                Add(_assertions, "elemental-blood-persistence-native-healing-" + fixture.Label,
                    "one actual HP healed; one HP spent; one native tick; global clock unchanged",
                    "wounds=" + unit.Damage + ";spent=" + capacity.Spent(trait) + ";round=" + buff.RoundNumber,
                    exact, "exact disposable fixture scheduling boundary, native RuleDealDamage and BuffCollection.Tick");
                if (!exact) throw new InvalidOperationException(fixture.Label + " did not produce exact partially spent native healing state.");
            }

            private void RemoveBloodPersistenceBuff(ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                ElementalBloodDamageTrigger trigger = PersistenceBloodTrigger(fixture);
                if (trigger == null) return;
                if (!IsFixtureUnit(unit, fixture))
                    throw new InvalidOperationException("Blood cleanup target is not the exact disposable fixture.");
                foreach (Buff buff in unit.Buffs.Enumerable.Where(value =>
                    ReferenceEquals(value.Blueprint, trigger.HealingBuff)).ToArray())
                    unit.Buffs.RemoveFact(buff);
                if (unit.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, trigger.HealingBuff)))
                    throw new InvalidOperationException("Exact owned blood buff cleanup failed.");
            }

            private void RestoredBloodPersistenceState(ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                RecordTraitPersistence(fixture, unit, 2, 0, false, "module-off-after-rest");
                PrepareCrystallinePersistence(fixture, unit);
                ArmBloodPersistence(fixture, unit);
                RemoveBloodPersistenceBuff(fixture, unit);
                RecordTraitPersistence(fixture, unit, 2, 1, false, "module-off-respent-before-save");
            }

            private void RecordTraitPersistence(ElementalPersistenceFixture fixture, UnitEntityData unit,
                int level, int spent, bool activeBloodBuff, string phase, bool traitsExpected = true)
            {
                ElementalHeritageBlueprints heritage = traitsExpected ? fixture.Heritage : fixture.RestoredHeritage;
                ElementalAlternateTraitBlueprints[] traits = ExpectedPersistenceTraits(fixture, heritage, traitsExpected);
                ElementalBloodDamageTrigger trigger = traits.SelectMany(trait => trait.Provider.ComponentsArray)
                    .OfType<ElementalBloodDamageTrigger>().SingleOrDefault();
                UnitPartElementalBloodCapacity capacity = unit.Descriptor.Get<UnitPartElementalBloodCapacity>();
                var buffs = fixture.Blueprints.AlternateTraits.Traits().SelectMany(value => value.Provider.ComponentsArray)
                    .OfType<ElementalBloodDamageTrigger>().Select(value => value.HealingBuff).ToArray();
                Buff[] active = unit.Buffs.Enumerable.Where(value => buffs.Any(blueprint =>
                    ReferenceEquals(value.Blueprint, blueprint))).ToArray();
                bool blood = trigger != null;
                bool exact = IsFixtureUnit(unit, fixture) && unit.Descriptor.Progression.CharacterLevel == level &&
                    AlternateTraitsExact(unit.Descriptor, fixture, traits) &&
                    active.Length == (blood && activeBloodBuff ? 1 : 0) &&
                    active.All(value => ReferenceEquals(value.Blueprint, trigger.HealingBuff) &&
                        value.Active && !value.IsSuppressed && value.RoundNumber == 1 &&
                        value.EndTime > Game.Instance.TimeController.GameTime) &&
                    (blood ? capacity != null && BloodPersistenceTraits.All(id => capacity.Spent(id) ==
                        (id == (ElementalAlternateTraitId)trigger.Trait ? spent : 0)) &&
                        capacity.Remaining((ElementalAlternateTraitId)trigger.Trait) == level * 2 - spent
                        : capacity == null);
                var resource = PersistenceSlaResource(fixture, heritage);
                Buff[] sizeBuffs = EfreetiPersistenceBuffs(fixture, unit);
                int resourceBefore = unit.Descriptor.Resources.GetResourceAmount(resource);
                bool reconciled = ElementalHeritageRuntime.Reconcile(unit.Descriptor, null, null);
                UnitPartElementalBloodCapacity after = unit.Descriptor.Get<UnitPartElementalBloodCapacity>();
                exact &= reconciled && ReferenceEquals(capacity, after) &&
                    unit.Descriptor.Resources.GetResourceAmount(resource) == resourceBefore &&
                    AlternateTraitsExact(unit.Descriptor, fixture, traits) &&
                    sizeBuffs.SequenceEqual(EfreetiPersistenceBuffs(fixture, unit)) &&
                    active.All(value => unit.Buffs.Enumerable.Any(current => ReferenceEquals(value, current))) &&
                    (!blood || after.Spent((ElementalAlternateTraitId)trigger.Trait) == spent);
                var record = new JObject
                {
                    { "crystalline", RecordCrystallinePersistence(fixture, unit, traits, phase) },
                    { "fixture", fixture.Label }, { "phase", phase },
                    { "traits", new JArray(traits.Select(trait => trait.Definition.Id.ToString())) },
                    { "level", level }, { "bloodLedgerPresent", capacity != null },
                    { "gameTimeTicks", Game.Instance.TimeController.GameTime.Ticks },
                    { "paused", Game.Instance.IsPaused },
                    { "spent", blood && capacity != null ? capacity.Spent((ElementalAlternateTraitId)trigger.Trait) : 0 },
                    { "remaining", blood && capacity != null ? capacity.Remaining((ElementalAlternateTraitId)trigger.Trait) : 0 },
                    { "activeBuffs", new JArray(active.Select(value => new JObject
                        { { "guid", value.Blueprint.AssetGuid }, { "round", value.RoundNumber },
                          { "endTimeTicks", value.EndTime.Ticks } })) },
                    { "reconcileAccepted", reconciled }, { "racialResourceGuid", resource.AssetGuid },
                    { "racialResourceAmount", resourceBefore }, { "exact", exact }
                };
                _traitPersistenceRecords.Add(record);
                Add(_assertions, "elemental-trait-persistence-" + phase + "-" + fixture.Label,
                    "exact selected/retained facts, active buff and actual-HP expenditure; idempotent reconciliation",
                    record.ToString(Newtonsoft.Json.Formatting.None), exact,
                    "native saved UnitPart, provider facts and BuffCollection; no reconstructed expenditure or manual ledger assignment");
                if (!exact) throw new InvalidOperationException("Trait persistence state diverged: " + record.ToString(Newtonsoft.Json.Formatting.None));
            }
        }
    }
}
