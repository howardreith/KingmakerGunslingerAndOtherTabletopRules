using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class ElementalBreathScenario
    {
        private sealed class Observation : IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleDealDamage>, IGlobalRulebookHandler<RuleAttackRoll>
        {
            internal UnitEntityData Caster, Target;
            internal readonly List<RuleSavingThrow> Saves = new List<RuleSavingThrow>();
            internal readonly List<RuleDealDamage> Damage = new List<RuleDealDamage>();
            internal int Attacks, Projectiles;
            internal bool Completed, Released;
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventAboutToTrigger(RuleAttackRoll evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            { if (ReferenceEquals(evt.Initiator, Target)) Saves.Add(evt); }
            public void OnEventDidTrigger(RuleDealDamage evt)
            { if (ReferenceEquals(evt.Initiator, Caster) && ReferenceEquals(evt.Target, Target)) Damage.Add(evt); }
            public void OnEventDidTrigger(RuleAttackRoll evt)
            { if (ReferenceEquals(evt.Initiator, Caster)) Attacks++; }
            public override string ToString()
            { return "complete=" + Completed + ";projectiles=" + Projectiles + ";saves=" + Saves.Count +
                ";passed=" + string.Join(",", Saves.Select(value => value.IsPassed)) + ";damage=" +
                Damage.Sum(value => value.Damage) + ";attacks=" + Attacks + ";observerReleased=" + Released; }
        }

        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            var rows = new JArray();
            var diagnostics = new List<string>();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            var oldProjectiles = Game.Instance.ProjectileController.Projectiles.ToArray();
            UnityEngine.Random.State random = UnityEngine.Random.state;
            if (oldProjectiles.Length != 0) throw new InvalidOperationException("Breath transport fixture must begin pristine.");
            BlueprintAbility donor = Exact<BlueprintAbility>(ElementalBreathFactory.AcidDonorGuid);
            BlueprintComponent[] originalComponents = donor.ComponentsArray;
            string originalContract = RuntimeTestRunner.DescribeNestedObject(donor.ComponentsArray, 10);
            try
            {
                ElementalRaceBlueprints race = BlueprintBootstrap.ElementalRaces.Undine;
                foreach (var heritage in race.Heritages.Choices())
                foreach (var id in new[] { ElementalAlternateTraitId.AcidBreath, ElementalAlternateTraitId.OozeBreath })
                {
                    var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    TimeSpan clock = Game.Instance.TimeController.GameTime;
                    try
                    {
                        UnitEntityData target = fixture.SpawnFixtureUnit(race.Race,
                            fixture.Caster.Blueprint.Faction, new Vector3(0, 0, 0.8f), "BreathTarget");
                        Run(fixture.Caster, target, race, heritage, id, rows, assertions);
                    }
                    finally
                    {
                        Game.Instance.Player.GameTime = clock;
                        fixture.Dispose();
                        Check(assertions, rows, heritage.Definition.Id + "-" + id + "-native-lifetime",
                            fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                            fixture.NativeObservationReleased && fixture.NativeTeardownObserved &&
                            fixture.AreaContextRestored && fixture.PlayerContextRestored,
                            "native errors=" + fixture.NativeErrors + ";exceptions=" + fixture.NativeExceptions);
                    }
                }
                Check(assertions, rows, "donor-unchanged",
                    ReferenceEquals(donor.ComponentsArray, originalComponents) &&
                    RuntimeTestRunner.DescribeNestedObject(donor.ComponentsArray, 10) == originalContract,
                    "exact native acid donor component array and recursive values unchanged");
            }
            finally
            {
                UnityEngine.Random.state = random;
                bool clean = before.Length == Game.Instance.State.Units.All.Count &&
                    before.All(value => Game.Instance.State.Units.All.Contains(value)) &&
                    Game.Instance.ProjectileController.Projectiles.SequenceEqual(oldProjectiles);
                Check(assertions, rows, "fixture-cleanup", clean, "exact original unit and projectile membership");
                string path = Path.Combine(request.EvidenceDirectory, "elemental-undine-breaths.json");
                File.WriteAllText(path, new JObject { { "schemaVersion", 1 }, { "saveStateTouched", false },
                    { "cleanupExact", clean }, { "isolatedBoundary", "request-local projectile transport arrival only; native cone targeting, command, save, damage and condition actions retained" },
                    { "diagnostics", new JArray(diagnostics) }, { "observations", rows } }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void Run(UnitEntityData caster, UnitEntityData target, ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage, ElementalAlternateTraitId id, JArray rows,
            ICollection<RuntimeTestAssertion> assertions)
        {
            UnitDescriptor owner = caster.Descriptor;
            bool ooze = id == ElementalAlternateTraitId.OozeBreath;
            string prefix = heritage.Definition.Id + "-" + id + "-";
            var trait = race.AlternateTraits.Require(id);
            BlueprintAbility ability = trait.Mechanics().OfType<BlueprintAbility>().Single();
            BlueprintAbilityResource resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
            BlueprintBuff sickened = Exact<BlueprintBuff>(ElementalBreathFactory.SickenedGuid);
            BlueprintCharacterClass fighter = Exact<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            BlueprintCharacterClass wizard = Exact<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            owner.AddFact(heritage.Marker);
            owner.AddFact(trait.Marker);
            ElementalSpellAffinityScenario.Advance(owner, fighter, 1);
            target.Stats.HitPoints.BaseValue = 10000;
            target.Stats.SaveReflex.BaseValue = -100;
            var cone = ability.ComponentsArray.OfType<AbilityDeliverProjectile>().Single();
            Check(assertions, rows, prefix + "owned-graph-and-replacement",
                owner.HasFact(trait.Provider) && owner.HasFact(race.Resistance) && owner.HasFact(heritage.Affinity) &&
                race.Heritages.Choices().All(value => !owner.HasFact(value.SlaFeature) &&
                    owner.Abilities.GetAbility(value.SlaAbility) == null) &&
                ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits) &&
                resource.GetMaxAmount(owner) == 1 && ability.Type == AbilityType.Supernatural &&
                ability.ActionType == UnitCommand.CommandType.Standard && !ability.IsFullRoundAction &&
                cone.Length.Equals(5.Feet()) && cone.LineWidth.Equals(5.Feet()) &&
                cone.Type == AbilityProjectileType.Cone && !cone.NeedAttackRoll && !ability.SpellResistance &&
                ability.Icon != null && ability.MaterialComponent.Item == null && !Data(owner, ability).IsAffectedByArcaneSpellFailure,
                "native five-foot supernatural standard cone; independent daily resource; all replaced SLAs absent");

            foreach (int level in new[] { 1, 2, 5, 10, 11, 20 })
            {
                if (level > owner.Progression.CharacterLevel)
                {
                    ElementalSpellAffinityScenario.Advance(owner, wizard, level - owner.Progression.CharacterLevel);
                    Check(assertions, rows, prefix + "level-up-keeps-spent-" + level,
                        owner.Resources.GetResourceAmount(resource) == 0,
                        "native multiclass level-up updates scaling without restoring the spent use");
                }
                CheckParameters(owner, ability, assertions, rows, prefix + "parameters-" + level);
                foreach (int adjustment in new[] { 4, -4 })
                {
                    var modifier = owner.Stats.Constitution.AddModifier(adjustment, owner.GetFact(trait.Provider),
                        "breath-temporary-Constitution", ModifierDescriptor.UntypedStackable);
                    try { CheckParameters(owner, ability, assertions, rows, prefix + "parameters-" + level + "-Con" + adjustment); }
                    finally { owner.Stats.Constitution.RemoveModifier(modifier); }
                }
                Reset(owner, target, sickened);
                Observation cast = Cast(caster, target, ability, new Vector3(0, 0, 1.4f));
                int dice = ElementalBreathPolicy.DamageDice(level);
                Check(assertions, rows, prefix + "native-level-" + level,
                    cast.Completed && cast.Released && cast.Projectiles == 1 && cast.Attacks == 0 &&
                    cast.Saves.Count == 1 && !cast.Saves[0].IsPassed &&
                    cast.Saves[0].DifficultyClass == ElementalBreathPolicy.DifficultyClass(level, owner.Stats.Constitution.Bonus) &&
                    cast.Damage.Count == (dice == 0 ? 0 : 1) &&
                    cast.Damage.All(value => ReferenceEquals(value.SourceAbility, ability) &&
                        value.ResultDamage.Count == 1 && value.ResultDamage[0].Source is EnergyDamage &&
                        ((EnergyDamage)value.ResultDamage[0].Source).EnergyType == DamageEnergyType.Acid &&
                        value.ResultDamage[0].Source.Dice.Rolls == dice &&
                        value.ResultDamage[0].Source.Dice.Dice == (ooze ? DiceType.D4 : DiceType.D8)) &&
                    target.Descriptor.HasFact(sickened) == ooze && owner.Resources.GetResourceAmount(resource) == 0,
                    cast + ";level=" + level + ";expectedDice=" + dice + ";sickened=" + target.Descriptor.HasFact(sickened));
                if (ooze)
                {
                    Buff buff = target.Buffs.GetBuff(sickened);
                    Check(assertions, rows, prefix + "three-rounds-" + level, buff != null &&
                        Math.Abs(buff.TimeLeft.TotalSeconds - 18) < 0.01, "native Sickened TimeLeft=" + (buff == null ? -1 : buff.TimeLeft.TotalSeconds));
                }
                Check(assertions, rows, prefix + "spent-zero-" + level, !Data(owner, ability).IsAvailable &&
                    ElementalHeritageRuntime.Reconcile(owner, null, null) &&
                    owner.Resources.GetResourceAmount(resource) == 0, "accepted cast uses exactly one; reconciliation cannot refill");
            }

            Reset(owner, target, sickened);
            var point = new TargetWrapper(new Vector3(0, 0, 1.4f));
            BlueprintFeature acidAffinity = BlueprintBootstrap.ElementalRaces.Oread.Affinity;
            owner.AddFact(acidAffinity);
            try { CheckParameters(owner, ability, assertions, rows, prefix + "matching-acid-affinity-excluded"); }
            finally { owner.RemoveFact(acidAffinity); }
            var canceled = ElementalUndineFeatScenario.CreateCommand(Data(owner, ability), point, caster);
            caster.Commands.Run(canceled);
            bool queued = caster.Commands.Contains(canceled);
            caster.Commands.InterruptAll(true);
            caster.Commands.RemoveFinishedAndUpdateQueue();
            Check(assertions, rows, prefix + "cancel", queued && !canceled.IsStarted &&
                owner.Resources.GetResourceAmount(resource) == 1 && target.Damage == 0 &&
                !target.Descriptor.HasFact(sickened), "native queued command canceled before action; no effect or expenditure");

            Observation failed = Cast(caster, target, ability, point.Point);
            int fullDamage = failed.Damage.Sum(value => value.Damage);
            Reset(owner, target, sickened);
            target.Stats.SaveReflex.BaseValue = 100;
            Observation saved = Cast(caster, target, ability, point.Point);
            Check(assertions, rows, prefix + "reflex-half", failed.Saves.Count == 1 && !failed.Saves[0].IsPassed &&
                saved.Saves.Count == 1 && saved.Saves[0].IsPassed &&
                saved.Saves[0].D20.Value == failed.Saves[0].D20.Value && fullDamage > 0 &&
                saved.Damage.Sum(value => value.Damage) == fullDamage / 2 && !target.Descriptor.HasFact(sickened),
                "identically seeded actual native commands;full=" + fullDamage + ";saved=" + saved + ";failed=" + failed);

            foreach (string defense in new[] { "acid-resistance", "acid-immunity", "poison-immunity" })
            {
                Reset(owner, target, sickened);
                target.Stats.SaveReflex.BaseValue = -100;
                BlueprintComponent component;
                if (defense == "acid-resistance")
                {
                    var resistance = ScriptableObject.CreateInstance<AddDamageResistanceEnergy>();
                    resistance.Type = DamageEnergyType.Acid;
                    resistance.Value = new ContextValue { ValueType = ContextValueType.Simple, Value = 100 };
                    component = resistance;
                }
                else if (defense == "acid-immunity")
                {
                    var immune = ScriptableObject.CreateInstance<AddEnergyImmunity>();
                    immune.Type = DamageEnergyType.Acid;
                    component = immune;
                }
                else
                {
                    var poison = ScriptableObject.CreateInstance<BuffDescriptorImmunity>();
                    poison.Descriptor = SpellDescriptor.Poison;
                    component = poison;
                }
                var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
                feature.name = "KMG_Runtime_Breath_" + defense;
                feature.Ranks = 1;
                feature.ComponentsArray = new[] { component };
                target.Descriptor.AddFact(feature);
                try
                {
                    Observation cast = Cast(caster, target, ability, point.Point);
                    Check(assertions, rows, prefix + defense, cast.Completed && cast.Saves.Count == 1 && !cast.Saves[0].IsPassed &&
                        cast.Damage.Count == 1 && (defense == "poison-immunity" ? cast.Damage[0].Damage > 0 : cast.Damage[0].Damage == 0) &&
                        target.Descriptor.HasFact(sickened) == ooze, cast + ";sickened=" + target.Descriptor.HasFact(sickened));
                }
                finally { target.Descriptor.RemoveFact(feature); }
            }

            foreach (Vector3 position in new[] { new Vector3(0, 0, 4), new Vector3(0, 0, -3), new Vector3(3, 0, 0) })
            {
                Reset(owner, target, sickened);
                target.Position = position;
                Observation cast = Cast(caster, target, ability, point.Point);
                Check(assertions, rows, prefix + "outside-cone-" + position, cast.Completed &&
                    cast.Saves.Count == 0 && cast.Damage.Count == 0 && !target.Descriptor.HasFact(sickened),
                    cast + ";native target position=" + position);
            }
            target.Position = new Vector3(0, 0, 0.8f);
            Reset(owner, target, sickened);
            target.Stats.SaveReflex.BaseValue = -100;
            Cast(caster, target, ability, point.Point);
            if (ooze)
            {
                Buff buff = target.Buffs.GetBuff(sickened);
                if (buff == null) throw new InvalidOperationException("The native expiry witness needs an active Sickened buff.");
                TimeSpan clock = Game.Instance.TimeController.GameTime;
                try
                {
                    Game.Instance.Player.GameTime = buff.EndTime - TimeSpan.FromSeconds(0.1);
                    target.Buffs.Tick();
                    Check(assertions, rows, prefix + "duration-before-expiry",
                        ReferenceEquals(target.Buffs.GetBuff(sickened), buff), "same native buff just before three-round expiry");
                    Game.Instance.Player.GameTime = buff.EndTime + TimeSpan.FromSeconds(0.1);
                    target.Buffs.Tick();
                    Check(assertions, rows, prefix + "duration-native-expiry",
                        !target.Descriptor.HasFact(sickened), "native ticking removes Sickened after exactly three rounds");
                }
                finally { Game.Instance.Player.GameTime = clock; }
            }
            Fact provider = owner.GetFact(trait.Provider);
            provider.Deactivate();
            provider.Activate();
            Check(assertions, rows, prefix + "reactivation-spent", owner.Resources.GetResourceAmount(resource) == 0 &&
                ElementalHeritageRuntime.Reconcile(owner, null, null) && owner.Resources.GetResourceAmount(resource) == 0,
                "native provider deactivation/activation preserves expenditure");
            owner.RemoveFact(trait.Marker);
            Check(assertions, rows, prefix + "remove-cleans-graph", owner.Abilities.GetAbility(ability) == null &&
                !owner.Resources.PersistantResources.Any(value => ReferenceEquals(value.Blueprint, resource)) &&
                owner.HasFact(heritage.SlaFeature) && ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits),
                "trait removal restores the exact heritage SLA and removes only its own daily graph");
            owner.AddFact(trait.Marker);
            Check(assertions, rows, prefix + "readd-spent", owner.Resources.GetResourceAmount(resource) == 0,
                "same-day remove/re-add retains spent use");
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            Check(assertions, rows, prefix + "ordinary-rest-one", owner.Resources.GetResourceAmount(resource) == 1,
                "native ordinary rest restores exactly one use");
        }

        private static void CheckParameters(UnitDescriptor owner, BlueprintAbility ability,
            ICollection<RuntimeTestAssertion> assertions, JArray rows, string name)
        {
            AbilityData data = Data(owner, ability);
            AbilityParams value = data.CalculateParams();
            var context = data.CreateExecutionContext(new TargetWrapper(new Vector3(0, 0, 1)));
            int level = owner.Progression.CharacterLevel;
            Check(assertions, rows, name, data.Spellbook == null &&
                value.CasterLevel == level && context.Params.CasterLevel == level &&
                value.DC == ElementalBreathPolicy.DifficultyClass(level, owner.Stats.Constitution.Bonus) &&
                context.Params.DC == value.DC && ability.ComponentsArray.OfType<ContextRankConfig>().Single().GetValue(context) ==
                    ElementalBreathPolicy.DamageDice(level),
                "UI/native context DC=" + value.DC + "/" + context.Params.DC + ";CL=" + value.CasterLevel +
                ";totalLevel=" + level + ";currentCon=" + owner.Stats.Constitution.Bonus);
        }

        private static Observation Cast(UnitEntityData caster, UnitEntityData target, BlueprintAbility ability, Vector3 point)
        {
            var observed = new Observation { Caster = caster, Target = target };
            var prior = new HashSet<Projectile>(Game.Instance.ProjectileController.Projectiles);
            var created = new List<Projectile>();
            var command = ElementalUndineFeatScenario.CreateCommand(Data(caster.Descriptor, ability), new TargetWrapper(point), caster);
            EventBus.Subscribe(observed);
            UnityEngine.Random.InitState(7419);
            try
            {
                ElementalUndineFeatScenario.InvokeCommandAction(command);
                for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 100; tick++)
                {
                    command.ExecutionProcess.Tick();
                    foreach (Projectile projectile in Game.Instance.ProjectileController.Projectiles.Where(value =>
                        !prior.Contains(value) && !created.Contains(value) && ReferenceEquals(value.Launcher, caster)).ToArray())
                    {
                        created.Add(projectile);
                        // Same qualified request-local transport seam as ray
                        // tests. The native cone iterator chooses every target.
                        typeof(Projectile).GetProperty("IsHit").GetSetMethod(true).Invoke(projectile, new object[] { true });
                        projectile.OnHit();
                    }
                }
                observed.Completed = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded;
                observed.Projectiles = created.Count;
                if (!observed.Completed && command.ExecutionProcess != null) command.ExecutionProcess.Detach();
                ElementalUndineFeatScenario.InvokeCommandEnded(command, !observed.Completed);
                return observed;
            }
            finally
            {
                EventBus.Unsubscribe(observed);
                observed.Released = true;
                foreach (Projectile projectile in created) projectile.Cleared = true;
                Game.Instance.ProjectileController.Tick();
            }
        }

        private static void Reset(UnitDescriptor owner, UnitEntityData target, BlueprintBuff sickened)
        {
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            Buff buff = target.Buffs.GetBuff(sickened);
            if (buff != null) target.Buffs.RemoveFact(buff);
            target.Damage = 0;
        }
        private static AbilityData Data(UnitDescriptor owner, BlueprintAbility ability)
        {
            Ability fact = owner.Abilities.GetAbility(ability);
            if (fact == null) throw new InvalidOperationException("Exact breath ability absent.");
            return new AbilityData(fact);
        }
        private static T Exact<T>(string guid) where T : BlueprintScriptableObject
        { return BlueprintLibraryLookup.RequireExact<T>(BlueprintBootstrap.Library, guid, "native breath regression"); }
        private static void Check(ICollection<RuntimeTestAssertion> assertions, JArray rows, string name, bool pass, string observed)
        {
            rows.Add(new JObject { { "name", name }, { "pass", pass }, { "observed", observed } });
            assertions.Add(new RuntimeTestAssertion { Name = "elemental-breath-" + name,
                Expected = "exact printed breath and native command/resource/effect behavior", Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "native commands, cone delivery, rule events and exact owned fixtures; no saves" });
        }
    }
}
