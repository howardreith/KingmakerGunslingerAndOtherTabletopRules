using System;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
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
            private readonly JArray _efreetiPersistenceRecords = new JArray();

            private static BlueprintAbility[] EfreetiPersistenceVariants(ElementalPersistenceFixture fixture)
            {
                if (fixture.Blueprints.AlternateTraits.Race != ElementalHeritageRace.Ifrit)
                    return new BlueprintAbility[0];
                return fixture.Blueprints.AlternateTraits.Require(ElementalAlternateTraitId.EfreetiMagic)
                    .Mechanics().OfType<BlueprintAbility>().Single(value => value.Parent == null)
                    .ComponentsArray.OfType<AbilityVariants>().Single().Variants;
            }

            private static int EfreetiPersistenceVariantIndex(ElementalPersistenceFixture fixture)
            {
                int heritage = Array.IndexOf(fixture.Blueprints.Heritages.Choices().ToArray(), fixture.Heritage);
                return ElementalBloodInsightPersistencePolicy.EfreetiVariantIndex(
                    fixture.Gender == Kingmaker.Blueprints.Gender.Male ? 0 : 1, heritage);
            }

            private static int EfreetiPersistenceStatDelta(ElementalPersistenceFixture fixture, ElementalHeritageStat stat)
            {
                int direction = EfreetiPersistenceVariantIndex(fixture) == 0 ? 1 : -1;
                if (stat == ElementalHeritageStat.Strength) return 2 * direction;
                if (stat == ElementalHeritageStat.Dexterity) return -2 * direction;
                return 0;
            }

            private static Buff[] EfreetiPersistenceBuffs(ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                BlueprintAbility[] variants = EfreetiPersistenceVariants(fixture);
                return unit.Buffs.Enumerable.Where(value => value.Context != null &&
                    variants.Any(variant => ReferenceEquals(value.Context.SourceAbility, variant))).ToArray();
            }

            private bool EfreetiPersistenceBuffExact(ElementalPersistenceFixture fixture, UnitEntityData unit,
                int expectedCasterLevel)
            {
                BlueprintAbility[] variants = EfreetiPersistenceVariants(fixture);
                Buff[] buffs = EfreetiPersistenceBuffs(fixture, unit);
                if (variants.Length == 0) return expectedCasterLevel == 0 && buffs.Length == 0;
                BlueprintAbility expected = variants[EfreetiPersistenceVariantIndex(fixture)];
                // Read-only observations include the validated pre-respec source
                // actor, whose ID intentionally differs from the persisted
                // replacement. Mutation helpers still require IsFixtureUnit.
                bool exact = buffs.Length == (expectedCasterLevel > 0 ? 1 : 0) &&
                    buffs.All(value => value.Active && !value.IsSuppressed &&
                        ReferenceEquals(value.Context.SourceAbility, expected) &&
                        ReferenceEquals(value.Context.MaybeCaster, unit) &&
                        value.Context.Params.CasterLevel == expectedCasterLevel &&
                        value.Context.Params.SpellLevel == 1 &&
                        value.TimeLeft.TotalSeconds > 0 &&
                        value.TimeLeft.TotalSeconds <= 60 * expectedCasterLevel + 0.01);
                _efreetiPersistenceRecords.Add(new JObject {
                    { "fixture", fixture.Label }, { "phase", _stage }, { "kind", "native-size-buff" },
                    { "expectedCasterLevel", expectedCasterLevel }, { "expectedAbilityGuid", expected.AssetGuid },
                    { "gameTimeTicks", Game.Instance.TimeController.GameTime.Ticks },
                    { "paused", Game.Instance.IsPaused }, { "exact", exact },
                    { "buffs", new JArray(buffs.Select(value => new JObject {
                        { "guid", value.Blueprint.AssetGuid }, { "sourceAbility", value.Context.SourceAbility.AssetGuid },
                        { "casterId", value.Context.MaybeCaster == null ? null : value.Context.MaybeCaster.UniqueId },
                        { "casterLevel", value.Context.Params.CasterLevel },
                        { "spellLevel", value.Context.Params.SpellLevel }, { "dc", value.Context.Params.DC },
                        { "endTimeTicks", value.EndTime.Ticks }, { "secondsLeft", value.TimeLeft.TotalSeconds }
                    })) }
                });
                return exact;
            }

            private void SpendPersistenceSla(ElementalPersistenceFixture fixture, UnitEntityData unit,
                AbilityData root, string phase)
            {
                var resource = PersistenceSlaResource(fixture, fixture.Heritage);
                ElementalAlternateTraitBlueprints trait = PersistenceSlaTrait(fixture, fixture.Heritage);
                if (trait == null)
                {
                    InvokeAbilitySpend(root, resource);
                    return;
                }
                if (trait.Definition.Id != ElementalAlternateTraitId.EfreetiMagic ||
                    !IsFixtureUnit(unit, fixture) || !Game.Instance.IsPaused)
                    throw new InvalidOperationException("Daily-trait persistence requires the exact paused Efreeti fixture.");
                BlueprintAbility[] variants = EfreetiPersistenceVariants(fixture);
                BlueprintAbility selected = variants[EfreetiPersistenceVariantIndex(fixture)];
                var data = new AbilityData(root, selected);
                var target = new TargetWrapper(unit);
                Buff[] before = unit.Buffs.Enumerable.ToArray();
                UnitUseAbility canceled = ElementalUndineFeatScenario.CreateCommand(data, target, unit);
                unit.Commands.Run(canceled);
                bool queued = unit.Commands.Contains(canceled);
                unit.Commands.InterruptAll(true);
                unit.Commands.RemoveFinishedAndUpdateQueue();
                bool canceledExact = queued && !canceled.IsStarted &&
                    unit.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                    unit.Buffs.Enumerable.SequenceEqual(before);
                if (!canceledExact || !data.IsAvailable || !data.CanTarget(target) ||
                    EfreetiPersistenceBuffs(fixture, unit).Length != 0)
                    throw new InvalidOperationException(fixture.Label + " did not begin the native Efreeti cast in an exact state.");
                UnitUseAbility command = ElementalUndineFeatScenario.CreateCommand(data, target, unit);
                object result = ElementalUndineFeatScenario.InvokeCommandAction(command);
                bool detached = false;
                if (command.ExecutionProcess != null)
                    ElementalUndineFeatScenario.CompleteProcess(command.ExecutionProcess, out detached);
                ElementalUndineFeatScenario.InvokeCommandEnded(command, false);
                int level = unit.Descriptor.Progression.CharacterLevel;
                Buff[] added = unit.Buffs.Enumerable.Except(before).ToArray();
                bool exact = command.ExecutionProcess != null && !detached && added.Length == 1 &&
                    before.All(value => unit.Buffs.Enumerable.Contains(value)) &&
                    Math.Abs(added[0].TimeLeft.TotalSeconds - 60 * level) < 0.01 &&
                    unit.Descriptor.Resources.GetResourceAmount(resource) == 0 &&
                    variants.All(value => !new AbilityData(root, value).IsAvailable &&
                        new AbilityData(root, value).GetAvailableForCastCount() == 0) &&
                    EfreetiPersistenceBuffExact(fixture, unit, level);
                var record = new JObject {
                    { "fixture", fixture.Label }, { "phase", phase }, { "kind", "native-command" },
                    { "abilityGuid", selected.AssetGuid }, { "canceledExact", canceledExact },
                    { "result", result == null ? null : result.ToString() },
                    { "resourceGuid", resource.AssetGuid }, { "resourceAmount", unit.Descriptor.Resources.GetResourceAmount(resource) },
                    { "nativeProcess", command.ExecutionProcess != null }, { "detached", detached }, { "exact", exact }
                };
                _efreetiPersistenceRecords.Add(record);
                Add(_assertions, "elemental-efreeti-persistence-" + phase + "-" + fixture.Label,
                    "native cancellation, exact accepted self cast, one size buff, one shared use spent and both choices blocked",
                    record.ToString(Newtonsoft.Json.Formatting.None), exact,
                    "native UnitUseAbility and AbilityExecutionProcess, no effect or resource-spend fallback for Efreeti Magic");
                if (!exact) throw new InvalidOperationException(fixture.Label + " native Efreeti persistence cast diverged.");
            }

            private void RemoveEfreetiPersistenceBuff(ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                if (!IsFixtureUnit(unit, fixture))
                    throw new InvalidOperationException("Size-effect cleanup target is not the exact disposable fixture.");
                foreach (Buff buff in EfreetiPersistenceBuffs(fixture, unit)) unit.Buffs.RemoveFact(buff);
                if (!EfreetiPersistenceBuffExact(fixture, unit, 0))
                    throw new InvalidOperationException("The exact request-created size effect was not cleaned before rest.");
            }
        }
    }
}
