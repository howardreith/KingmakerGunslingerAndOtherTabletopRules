using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class ElementalEfreetiMagicScenario
    {
        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            var rows = new JArray();
            var diagnostics = new List<string>();
            var temporary = new List<UnityEngine.Object>();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            UnityEngine.Random.State random = UnityEngine.Random.state;
            ElementalRaceBlueprints race = BlueprintBootstrap.ElementalRaces.Ifrit;
            BlueprintAbility[] donors = new[] {
                BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    ElementalEfreetiMagicFactory.EnlargeDonorGuid, "native Enlarge Person witness"),
                BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    ElementalEfreetiMagicFactory.ReduceDonorGuid, "native Reduce Person witness")
            };
            BlueprintComponent[][] donorComponents = donors.Select(value => value.ComponentsArray.ToArray()).ToArray();
            try
            {
                foreach (ElementalHeritageBlueprints heritage in race.Heritages.Choices())
                {
                    var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    try
                    {
                        UnitEntityData target = fixture.SpawnFixtureUnit(race.Race,
                            fixture.Caster.Blueprint.Faction, new Vector3(2, 0, 0), "EfreetiTarget");
                        Run(fixture.Caster, target, race, heritage, rows, assertions, temporary);
                    }
                    finally
                    {
                        fixture.Dispose();
                        Check(assertions, rows, heritage.Definition.Id + "-native-lifetime",
                            fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                            fixture.NativeObservationReleased && fixture.NativeTeardownObserved &&
                            fixture.AreaContextRestored && fixture.PlayerContextRestored,
                            "errors=" + fixture.NativeErrors + ";exceptions=" + fixture.NativeExceptions);
                    }
                }
                Check(assertions, rows, "donors-unchanged",
                    donors.Select((donor, index) => donor.Type == AbilityType.Spell &&
                        donor.Parent == null && donor.ComponentsArray.SequenceEqual(donorComponents[index]) &&
                        !donor.ComponentsArray.OfType<AbilityResourceLogic>().Any()).All(value => value),
                    "native ability types, parent links and exact component references remain unchanged");
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
                string path = Path.Combine(request.EvidenceDirectory, "elemental-efreeti-magic.json");
                File.WriteAllText(path, new JObject {
                    { "schemaVersion", 1 }, { "saveStateTouched", false }, { "cleanupExact", clean },
                    { "diagnostics", new JArray(diagnostics) }, { "observations", rows }
                }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void Run(UnitEntityData caster, UnitEntityData target, ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage, JArray rows, ICollection<RuntimeTestAssertion> assertions,
            ICollection<UnityEngine.Object> temporary)
        {
            UnitDescriptor owner = caster.Descriptor;
            string prefix = heritage.Definition.Id + "-";
            owner.AddFact(heritage.Marker);
            target.Descriptor.AddFact(heritage.Marker);
            ElementalAlternateTraitBlueprints trait = race.AlternateTraits.Require(ElementalAlternateTraitId.EfreetiMagic);
            BlueprintAbilityResource resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
            BlueprintAbility parent = trait.Mechanics().OfType<BlueprintAbility>().Single(value => value.Parent == null);
            BlueprintAbility[] variants = parent.ComponentsArray.OfType<AbilityVariants>().Single().Variants;
            owner.AddFact(trait.Marker);
            Check(assertions, rows, prefix + "replacement-and-shared-use",
                variants.Length == 2 && resource.GetMaxAmount(owner) == 1 &&
                owner.HasFact(trait.Provider) && owner.HasFact(race.Resistance) &&
                owner.HasFact(heritage.Affinity) && race.Heritages.Choices().All(value =>
                    !owner.HasFact(value.SlaFeature) && owner.Abilities.GetAbility(value.SlaAbility) == null) &&
                ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits),
                "one native resource and one root menu; no heritage SLA remains");

            BlueprintCharacterClass wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                BlueprintBootstrap.Library, "ba34257984f4c41408ce1dc2004e342e", "Efreeti native Wizard levels");
            BlueprintCharacterClass druid = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                BlueprintBootstrap.Library, "610d836f3a3a9ed42a4349b62f002e96", "Efreeti native Druid levels");
            ElementalSpellAffinityScenario.Advance(owner, wizard, 2);
            ElementalSpellAffinityScenario.Advance(owner, druid, 3);
            foreach (BlueprintAbility variant in variants)
            {
                AbilityData data = Data(owner, parent, variant);
                CheckParameters(assertions, rows, prefix + variant.name + "-base", data, owner);
                foreach (int change in new[] { 2, -2 })
                {
                    BlueprintFeature adjustment = Adjustment(change, temporary);
                    owner.AddFact(adjustment);
                    CheckParameters(assertions, rows, prefix + variant.name + "-charisma-" + change, Data(owner, parent, variant), owner);
                    owner.RemoveFact(adjustment);
                }
                Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
                data = Data(owner, parent, variant);
                var wrapped = new TargetWrapper(target);
                Buff[] before = target.Buffs.Enumerable.ToArray();
                int strength = target.Stats.Strength.ModifiedValue;
                int dexterity = target.Stats.Dexterity.ModifiedValue;
                UnitUseAbility canceled = ElementalUndineFeatScenario.CreateCommand(data, wrapped, caster);
                caster.Commands.Run(canceled);
                bool cancelQueued = caster.Commands.Contains(canceled);
                caster.Commands.InterruptAll(true);
                caster.Commands.RemoveFinishedAndUpdateQueue();
                Check(assertions, rows, prefix + variant.name + "-cancel",
                    cancelQueued && !canceled.IsStarted && owner.Resources.GetResourceAmount(resource) == 1 &&
                    target.Buffs.Enumerable.SequenceEqual(before),
                    "native queued cancellation spends no use and applies no effect");
                Check(assertions, rows, prefix + variant.name + "-target-and-availability",
                    data.CanTarget(wrapped) && data.IsAvailable && !data.IsAffectedByArcaneSpellFailure &&
                    data.Spellbook == null && variant.Type == AbilityType.SpellLike,
                    "another exact Ifrit heritage is a legal native person target");
                UnitUseAbility command = ElementalUndineFeatScenario.CreateCommand(data, wrapped, caster);
                object commandResult = ElementalUndineFeatScenario.InvokeCommandAction(command);
                bool detached = false;
                if (command.ExecutionProcess != null)
                    ElementalUndineFeatScenario.CompleteProcess(command.ExecutionProcess, out detached);
                ElementalUndineFeatScenario.InvokeCommandEnded(command, false);
                Buff[] applied = target.Buffs.Enumerable.Except(before).ToArray();
                bool enlarge = variant.AssetGuid.EndsWith("068", StringComparison.Ordinal);
                int direction = enlarge ? 1 : -1;
                Check(assertions, rows, prefix + variant.name + "-native-command-effect",
                    command.ExecutionProcess != null && !detached && applied.Length == 1 &&
                    target.Stats.Strength.ModifiedValue == strength + 2 * direction &&
                    target.Stats.Dexterity.ModifiedValue == dexterity - 2 * direction &&
                    Math.Abs(applied[0].TimeLeft.TotalSeconds - 60 * owner.Progression.CharacterLevel) < 0.01 &&
                    owner.Resources.GetResourceAmount(resource) == 0,
                    "result=" + commandResult + ";buffs=" + applied.Length + ";STR=" +
                        target.Stats.Strength.ModifiedValue + ";DEX=" + target.Stats.Dexterity.ModifiedValue +
                        ";resource=" + owner.Resources.GetResourceAmount(resource));
                Check(assertions, rows, prefix + variant.name + "-shared-zero",
                    variants.All(value => !Data(owner, parent, value).IsAvailable),
                    "either accepted cast blocks both variants at zero");
                foreach (Buff buff in applied) target.Buffs.RemoveFact(buff);
                Fact provider = owner.GetFact(trait.Provider);
                provider.Deactivate();
                provider.Activate();
                Check(assertions, rows, prefix + variant.name + "-reactivation-retains-spent",
                    owner.Resources.GetResourceAmount(resource) == 0 &&
                    ElementalHeritageRuntime.Reconcile(owner, null, null) &&
                    owner.Resources.GetResourceAmount(resource) == 0,
                    "actual native provider deactivation/activation and reconciliation do not refill");
            }

            ElementalSpellAffinityScenario.Advance(owner, wizard, 1);
            Check(assertions, rows, prefix + "level-up-retains-spent",
                owner.Progression.CharacterLevel == 6 && owner.Resources.GetResourceAmount(resource) == 0 &&
                Data(owner, parent, variants[0]).CalculateParams().CasterLevel == 6,
                "native multiclass level-up changes CL but not remaining uses");
            owner.RemoveFact(trait.Marker);
            Check(assertions, rows, prefix + "remove-cleans-owned",
                ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits) &&
                !owner.Resources.PersistantResources.Any(value => ReferenceEquals(value.Blueprint, resource)) &&
                owner.Abilities.GetAbility(parent) == null && owner.HasFact(heritage.SlaFeature),
                "only the exact owned daily graph is removed; heritage SLA returns");
            owner.AddFact(trait.Marker);
            Check(assertions, rows, prefix + "readd-retains-spent",
                owner.Resources.GetResourceAmount(resource) == 0,
                "same-day remove/re-add does not restore the spent trait use");
            owner.RemoveFact(trait.Marker);
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            owner.AddFact(trait.Marker);
            Check(assertions, rows, prefix + "rest-while-suppressed",
                owner.Resources.GetResourceAmount(resource) == 1 &&
                variants.All(value => Data(owner, parent, value).IsAvailable),
                "ordinary rest resets previous-day memory while the trait is absent");
        }

        private static AbilityData Data(UnitDescriptor owner, BlueprintAbility parent, BlueprintAbility variant)
        {
            return new AbilityData(new AbilityData(owner.Abilities.GetAbility(parent)), variant);
        }

        private static void CheckParameters(ICollection<RuntimeTestAssertion> assertions, JArray rows,
            string id, AbilityData data, UnitDescriptor owner)
        {
            var value = data.CalculateParams();
            Check(assertions, rows, id, value.CasterLevel == owner.Progression.CharacterLevel &&
                value.SpellLevel == 1 && value.DC == 11 + owner.Stats.Charisma.Bonus,
                "CL=" + value.CasterLevel + ";SL=" + value.SpellLevel + ";DC=" + value.DC +
                    ";CHA=" + owner.Stats.Charisma.Bonus + ";totalLevel=" + owner.Progression.CharacterLevel);
        }

        private static BlueprintFeature Adjustment(int amount, ICollection<UnityEngine.Object> temporary)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_Runtime_Efreeti_Charisma_" + amount;
            feature.Ranks = 1;
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = StatType.Charisma;
            bonus.Value = amount;
            bonus.Descriptor = amount > 0 ? ModifierDescriptor.Enhancement : ModifierDescriptor.UntypedStackable;
            feature.ComponentsArray = new BlueprintComponent[] { bonus };
            temporary.Add(feature);
            temporary.Add(bonus);
            return feature;
        }

        private static void Check(ICollection<RuntimeTestAssertion> assertions, JArray rows,
            string id, bool passed, string detail)
        {
            rows.Add(new JObject { { "id", id }, { "exact", passed }, { "detail", detail } });
            assertions.Add(new RuntimeTestAssertion {
                Name = "elemental-efreeti-" + id, Expected = "exact native shared-use trait behavior",
                Observed = detail, Status = passed ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "native UnitUseAbility, native effects, resource collection and rule parameters"
            });
        }
    }
}
