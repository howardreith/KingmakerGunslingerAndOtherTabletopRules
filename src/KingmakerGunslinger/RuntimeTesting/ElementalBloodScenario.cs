using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Native damage, buff scheduler, healing, rest and level-up rules.
    /// No effect internals are invoked; only the disposable player's clock advances.</summary>
    internal static class ElementalBloodScenario
    {
        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            var rows = new JArray();
            var diagnostics = new List<string>();
            var temporary = new List<UnityEngine.Object>();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            UnityEngine.Random.State random = UnityEngine.Random.state;
            try
            {
                foreach (ElementalAlternateTraitId id in new[] {
                    ElementalAlternateTraitId.FireInTheBlood,
                    ElementalAlternateTraitId.StoneInTheBlood,
                    ElementalAlternateTraitId.StormInTheBlood })
                {
                    ElementalRaceBlueprints race = BlueprintBootstrap.ElementalRaces
                        .OrderedBlueprints().Single(value => value.AlternateTraits
                            .Traits().Any(trait => trait.Definition.Id == id));
                    ElementalUndineFeatScenario.PortalHarness fixture =
                        ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    TimeSpan clock = Game.Instance.TimeController.GameTime;
                    try { RunTrait(fixture.Caster, race, id, rows, assertions, temporary); }
                    finally
                    {
                        Game.Instance.Player.GameTime = clock;
                        fixture.Dispose();
                        Check(assertions, rows, id + "-native-lifetime",
                            fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                                fixture.NativeInitializationObserved && fixture.NativeTeardownObserved &&
                                fixture.NativeObservationReleased && fixture.AreaContextRestored &&
                                fixture.PlayerContextRestored,
                            "nativeErrors=" + fixture.NativeErrors + ";nativeExceptions=" +
                                fixture.NativeExceptions + ";area=" + fixture.AreaContextRestored +
                                ";player=" + fixture.PlayerContextRestored);
                    }
                }
            }
            finally
            {
                UnityEngine.Random.state = random;
                bool clean = Game.Instance.State.Units.All.Count == before.Length &&
                    before.All(value => Game.Instance.State.Units.All.Contains(value));
                if (clean)
                    foreach (UnityEngine.Object value in temporary.AsEnumerable().Reverse())
                        UnityEngine.Object.DestroyImmediate(value);
                Check(assertions, rows, "fixture-cleanup", clean,
                    "before=" + before.Length + ";after=" + Game.Instance.State.Units.All.Count);
                string path = Path.Combine(request.EvidenceDirectory, "elemental-blood-traits.json");
                File.WriteAllText(path, new JObject {
                    { "schemaVersion", 1 }, { "saveStateTouched", false },
                    { "cleanupExact", clean }, { "diagnostics", new JArray(diagnostics) },
                    { "observations", rows }
                }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void RunTrait(UnitEntityData owner, ElementalRaceBlueprints race,
            ElementalAlternateTraitId id, JArray rows,
            ICollection<RuntimeTestAssertion> assertions, ICollection<UnityEngine.Object> temporary)
        {
            BlueprintCharacterClass wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                BlueprintBootstrap.Library, "ba34257984f4c41408ce1dc2004e342e", "native blood fixture class");
            BlueprintCharacterClass druid = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                BlueprintBootstrap.Library, "610d836f3a3a9ed42a4349b62f002e96", "native multiclass fixture");
            ElementalSpellAffinityScenario.Advance(owner.Descriptor, wizard, 1);
            owner.Stats.HitPoints.BaseValue = 500;
            owner.Damage = 20;
            ElementalAlternateTraitBlueprints trait = race.AlternateTraits.Require(id);
            BlueprintBuff blueprint = trait.Mechanics().OfType<BlueprintBuff>().Single();
            ElementalBloodDamageTrigger trigger = trait.Provider.ComponentsArray
                .OfType<ElementalBloodDamageTrigger>().Single();
            owner.Descriptor.AddFact(trait.Marker);
            Check(assertions, rows, id + "-replacement-and-budget",
                owner.Descriptor.HasFact(trait.Provider) && !owner.Descriptor.HasFact(race.Affinity) &&
                    owner.Descriptor.HasFact(race.Resistance) && owner.Descriptor.HasFact(race.SlaFeature) &&
                    Capacity(owner).Remaining(id) == 2 &&
                    ReferenceEquals(trigger.HealingBuff, blueprint),
                "level=" + owner.Descriptor.Progression.CharacterLevel + ";remaining=" + Capacity(owner).Remaining(id));

            RuleDealDamage resisted = Damage(owner, trigger.Energy, 3, false);
            Buff active = RequireBuff(owner, blueprint);
            TimeSpan firstTick = NextTick(active);
            TimeSpan firstEnd = active.EndTime;
            Check(assertions, rows, id + "-resisted-trigger",
                resisted.Damage == 0 && resisted.ResultDamage.Any(value =>
                    value.ValueWithoutReduction == 3) && owner.Damage == 20 &&
                    Math.Abs((firstEnd - Game.Instance.TimeController.GameTime).TotalSeconds - 6) < 0.001 &&
                    active.IsNotDispelable,
                "HPDamage=" + resisted.Damage + ";preReduction=" + resisted.DamageWithoutReduction +
                    ";duration=" + (firstEnd - Game.Instance.TimeController.GameTime).TotalSeconds);
            Game.Instance.Player.GameTime += TimeSpan.FromSeconds(1);
            Rulebook.Trigger(resisted);
            Check(assertions, rows, id + "-same-event-once", active.EndTime == firstEnd,
                "replaying the same damage rule does not refresh a second time");
            Damage(owner, trigger.Energy, 3, false);
            Check(assertions, rows, id + "-repeat-no-stack-no-postponement",
                owner.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, blueprint)) == 1 &&
                    ReferenceEquals(RequireBuff(owner, blueprint), active) &&
                    NextTick(active) == firstTick && active.EndTime > firstEnd,
                "sameBuff=true;nextTickUnchanged=" + (NextTick(active) == firstTick));
            owner.Damage = 1;
            TickAt(owner, firstTick);
            Check(assertions, rows, id + "-actual-healing-not-nominal",
                owner.Damage == 0 && Capacity(owner).Spent(id) == 1 && Capacity(owner).Remaining(id) == 1,
                "wounds=" + owner.Damage + ";spent=" + Capacity(owner).Spent(id));
            owner.Buffs.Tick();
            Check(assertions, rows, id + "-same-tick-once", Capacity(owner).Spent(id) == 1,
                "second native collection tick at unchanged time spends nothing");
            TickAt(owner, active.EndTime);
            Check(assertions, rows, id + "-native-expiry", FindBuff(owner, blueprint) == null,
                "expired one-round buff removed by native BuffCollection.Tick");

            owner.Damage = 20;
            Damage(owner, trigger.Energy, 3, false);
            TickAt(owner, NextTick(RequireBuff(owner, blueprint)));
            Check(assertions, rows, id + "-last-capacity-point",
                owner.Damage == 19 && Capacity(owner).Spent(id) == 2 && Capacity(owner).Remaining(id) == 0,
                "wounds=" + owner.Damage + ";spent=" + Capacity(owner).Spent(id));
            Damage(owner, trigger.Energy, 3, false);
            Check(assertions, rows, id + "-exhausted-no-buff", FindBuff(owner, blueprint) == null,
                "zero remaining capacity blocks a new healing effect");

            owner.Damage = 0;
            ElementalSpellAffinityScenario.Advance(owner.Descriptor, druid, 1);
            owner.Stats.HitPoints.BaseValue = 500;
            Check(assertions, rows, id + "-multiclass-no-refill",
                owner.Descriptor.Progression.CharacterLevel == 2 && Capacity(owner).Spent(id) == 2 &&
                    Capacity(owner).Remaining(id) == 2,
                "native Wizard 1/Druid 1;spent=" + Capacity(owner).Spent(id) +
                    ";remaining=" + Capacity(owner).Remaining(id));
            owner.Descriptor.RemoveFact(trait.Marker);
            owner.Descriptor.AddFact(trait.Marker);
            ElementalHeritageRuntime.Reconcile(owner.Descriptor, null, null);
            Check(assertions, rows, id + "-remove-readd-no-refill",
                Capacity(owner).Spent(id) == 2 && Capacity(owner).Remaining(id) == 2,
                "spent capacity survives removal, re-add and reconciliation");

            owner.Damage = 20;
            var immunity = ScriptableObject.CreateInstance<AddEnergyImmunity>();
            immunity.Type = trigger.Energy;
            BlueprintFeature immunityFact = FixtureFeature(immunity, temporary);
            owner.Descriptor.AddFact(immunityFact);
            try
            {
                RuleDealDamage immune = Damage(owner, trigger.Energy, 10, false);
                Check(assertions, rows, id + "-immune-trigger",
                    immune.Damage == 0 && immune.ResultDamage.Any(value => value.Source.Immune &&
                        value.ValueWithoutReduction > 0) && FindBuff(owner, blueprint) != null,
                    "native immune=" + immune.ResultDamage.Any(value => value.Source.Immune) +
                        ";HPDamage=" + immune.Damage);
                TickAt(owner, NextTick(RequireBuff(owner, blueprint)));
                Check(assertions, rows, id + "-immune-native-heal",
                    owner.Damage == 18 && Capacity(owner).Spent(id) == 4,
                    "native tick heals two after immune hit;spent=" + Capacity(owner).Spent(id));
            }
            finally { owner.Descriptor.RemoveFact(immunityFact); }

            owner.Descriptor.RemoveFact(trait.Marker);
            Damage(owner, trigger.Energy, 3, false);
            Check(assertions, rows, id + "-inactive-inert", FindBuff(owner, blueprint) == null,
                "an absent provider cannot trigger blood healing");
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner.Descriptor);
            owner.Descriptor.AddFact(trait.Marker);
            Check(assertions, rows, id + "-ordinary-rest-while-inactive",
                Capacity(owner).Spent(id) == 0 && Capacity(owner).Remaining(id) == 4,
                "native ApplyRest resets the persistent ledger even while trait is absent");
            Damage(owner, DamageEnergyType.Cold, 3, false);
            Check(assertions, rows, id + "-nonmatching-exclusion",
                FindBuff(owner, blueprint) == null && Capacity(owner).Spent(id) == 0,
                "a real Cold damage rule cannot arm Fire/Stone/Storm healing");
            RuleDealDamage empty = Rulebook.Trigger(new RuleDealDamage(owner, owner,
                new DamageBundle(new BaseDamage[0])));
            Check(assertions, rows, id + "-empty-damage-exclusion",
                empty.Damage == 0 && (empty.ResultDamage == null || empty.ResultDamage.Count == 0) &&
                    FindBuff(owner, blueprint) == null,
                "an empty native damage event has no qualifying packet");
            RuleDealDamage fake = Damage(owner, trigger.Energy, 3, true);
            Check(assertions, rows, id + "-fake-exclusion",
                fake.IsFake && FindBuff(owner, blueprint) == null && Capacity(owner).Spent(id) == 0,
                "IsFake=" + fake.IsFake + ";armed=" + (FindBuff(owner, blueprint) != null));
            // Kingmaker clamps a nominal zero energy roll to one before
            // resistance. That is a real qualifying hit, not a zero packet.
            RuleDealDamage minimum = Damage(owner, trigger.Energy, 0, false);
            Check(assertions, rows, id + "-native-minimum-damage",
                minimum.ResultDamage.Any(value => value.RolledValue == 0 &&
                    value.ValueWithoutReduction == 1) && FindBuff(owner, blueprint) != null,
                "native rolled=" + minimum.ResultDamage[0].RolledValue +
                    ";beforeResistance=" + minimum.ResultDamage[0].ValueWithoutReduction);
            TickAt(owner, NextTick(RequireBuff(owner, blueprint)));

            foreach (int percent in new[] { 0, 50, 200 })
            {
                Kingmaker.Controllers.Rest.RestController.ApplyRest(owner.Descriptor);
                if (percent == 200)
                {
                    owner.Damage = 1;
                    Damage(owner, trigger.Energy, 3, false);
                    TickAt(owner, NextTick(RequireBuff(owner, blueprint)));
                }
                int alreadySpent = Capacity(owner).Spent(id);
                owner.Damage = 20;
                Damage(owner, trigger.Energy, 3, false);
                var nativeModifier = ScriptableObject.CreateInstance<OutcomingDamageAndHealingModifier>();
                nativeModifier.ModifierPercents = new ContextValue { ValueType = ContextValueType.Simple,
                    Value = percent };
                BlueprintFeature modifierFact = FixtureFeature(nativeModifier, temporary);
                owner.Descriptor.AddFact(modifierFact);
                try
                {
                    TickAt(owner, NextTick(RequireBuff(owner, blueprint)));
                    int nativeAmount = 2 * percent / 100;
                    int expected = Math.Min(4 - alreadySpent, nativeAmount);
                    Check(assertions, rows, id + "-native-healing-modifier-" + percent,
                        owner.Damage == 20 - expected &&
                            Capacity(owner).Spent(id) == alreadySpent + expected,
                        "wounds=" + owner.Damage + ";spent=" + Capacity(owner).Spent(id) +
                            ";priorSpent=" + alreadySpent + ";nativeAmount=" + nativeAmount);
                    owner.Damage = 20;
                    RuleHealDamage unrelated = Rulebook.Trigger(new RuleHealDamage(owner, owner,
                        new DiceFormula(0, DiceType.D6), 2));
                    Check(assertions, rows, id + "-unrelated-healing-" + percent,
                        unrelated.Value == nativeAmount &&
                            Capacity(owner).Spent(id) == alreadySpent + expected,
                        "ordinary native healing=" + unrelated.Value + ";bloodSpentUnchanged=true");
                }
                finally { owner.Descriptor.RemoveFact(modifierFact); }
            }
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner.Descriptor);
            owner.Damage = 20;
            Damage(owner, trigger.Energy, 3, false);
            Buff beforeReconstruction = RequireBuff(owner, blueprint);
            var providerFact = owner.Descriptor.Progression.Features.RawFacts.OfType<Feature>()
                .Single(value => ReferenceEquals(value.Blueprint, trait.Provider));
            providerFact.Deactivate();
            Check(assertions, rows, id + "-native-provider-deactivation-preserves-saved-buff",
                ReferenceEquals(FindBuff(owner, blueprint), beforeReconstruction) && Capacity(owner).Spent(id) == 0,
                "Native Fact.Deactivate is not permanent marker loss; exact active buff and ledger retained.");
            providerFact.Activate();
            owner.Descriptor.RemoveFact(trait.Marker);
            Check(assertions, rows, id + "-active-provider-removal-cleans-buff",
                FindBuff(owner, blueprint) == null && Capacity(owner).Spent(id) == 0,
                "exact owned active buff removed; unspent ledger retained");
        }

        private static BlueprintFeature FixtureFeature(BlueprintComponent component,
            ICollection<UnityEngine.Object> temporary)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_Runtime_BloodNativeBoundary_" + temporary.Count;
            feature.Ranks = 1;
            feature.ComponentsArray = new[] { component };
            temporary.Add(component);
            temporary.Add(feature);
            return feature;
        }

        private static UnitPartElementalBloodCapacity Capacity(UnitEntityData owner)
        {
            UnitPartElementalBloodCapacity result = owner.Descriptor.Get<UnitPartElementalBloodCapacity>();
            if (result == null) throw new InvalidOperationException("Blood capacity ledger is missing.");
            return result;
        }

        private static RuleDealDamage Damage(UnitEntityData owner, DamageEnergyType energy,
            int amount, bool fake)
        {
            return Rulebook.Trigger(new RuleDealDamage(owner, owner,
                new DamageBundle(new EnergyDamage(new DiceFormula(0, DiceType.D6), energy)
                    { PreRolledValue = amount })) { IsFake = fake });
        }

        private static Buff FindBuff(UnitEntityData owner, BlueprintBuff blueprint)
        {
            return owner.Buffs.Enumerable.SingleOrDefault(value => ReferenceEquals(value.Blueprint, blueprint));
        }

        private static Buff RequireBuff(UnitEntityData owner, BlueprintBuff blueprint)
        {
            return FindBuff(owner, blueprint) ?? throw new InvalidOperationException(
                "Expected native blood buff is absent: " + blueprint.AssetGuid);
        }

        private static TimeSpan NextTick(Buff buff)
        {
            PropertyInfo property = typeof(Buff).GetProperty("NextTickTime",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || property.PropertyType != typeof(TimeSpan))
                throw new InvalidOperationException("Native Buff.NextTickTime observation is unavailable.");
            return (TimeSpan)property.GetValue(buff, null);
        }

        private static void TickAt(UnitEntityData owner, TimeSpan time)
        {
            if (time < Game.Instance.TimeController.GameTime)
                throw new InvalidOperationException("A fixture tick cannot move backwards.");
            Game.Instance.Player.GameTime = time;
            owner.Buffs.Tick();
        }

        private static void Check(ICollection<RuntimeTestAssertion> assertions,
            JArray rows, string name, bool pass, string observed)
        {
            assertions.Add(new RuntimeTestAssertion { Name = "elemental-blood-" + name,
                Expected = "exact native reactive-healing contract", Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "native damage, BuffCollection.Tick, RuleHealDamage, rest and level-up" });
            rows.Add(new JObject { { "name", name }, { "pass", pass }, { "observed", observed } });
        }
    }
}
