using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private void PrepareCrystallinePersistence(ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                ElementalAlternateTraitBlueprints trait = ExpectedPersistenceTraits(fixture, fixture.Heritage)
                    .SingleOrDefault(value => value.Definition.Id == ElementalAlternateTraitId.CrystallineForm);
                if (trait == null) return;
                if (!IsFixtureUnit(unit, fixture) || !Game.Instance.IsPaused)
                    throw new InvalidOperationException("Crystalline setup requires an exact paused disposable fixture.");
                BlueprintAbilityResource resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
                BlueprintActivatableAbility blueprint = trait.Mechanics().OfType<BlueprintActivatableAbility>().Single();
                ActivatableAbility mode = unit.Descriptor.ActivatableAbilities.Enumerable.Single(value =>
                    ReferenceEquals(value.Blueprint, blueprint));
                if (unit.Descriptor.Resources.GetResourceAmount(resource) != 1)
                    throw new InvalidOperationException("Crystalline fixture must have one native use before setup.");
                mode.IsOn = false;
                // Seed only the ordinary native resource boundary, not the
                // saved ledger. Actual ray-hit expenditure is independently
                // qualified by ElementalCrystallineFormScenario's commands.
                if (fixture.Gender == Gender.Male) unit.Descriptor.Resources.Spend(resource, 1);
                else mode.IsOn = true;
            }

            private JObject RecordCrystallinePersistence(ElementalPersistenceFixture fixture, UnitEntityData unit,
                ICollection<ElementalAlternateTraitBlueprints> expectedTraits, string phase)
            {
                if (fixture.Blueprints.AlternateTraits.Race != ElementalHeritageRace.Oread) return null;
                ElementalAlternateTraitBlueprints trait = fixture.Blueprints.AlternateTraits
                    .Require(ElementalAlternateTraitId.CrystallineForm);
                bool expected = expectedTraits.Contains(trait);
                BlueprintAbilityResource resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
                BlueprintActivatableAbility blueprint = trait.Mechanics().OfType<BlueprintActivatableAbility>().Single();
                ActivatableAbility[] modes = unit.Descriptor.ActivatableAbilities.Enumerable.Where(value =>
                    ReferenceEquals(value.Blueprint, blueprint)).ToArray();
                Buff[] buffs = unit.Buffs.Enumerable.Where(value =>
                    ReferenceEquals(value.Blueprint, blueprint.Buff)).ToArray();
                int resourceCount = unit.Descriptor.Resources.PersistantResources.Count(value =>
                    value != null && ReferenceEquals(value.Blueprint, resource));
                int amount = unit.Descriptor.Resources.GetResourceAmount(resource);
                bool afterRest = phase == "module-off-after-rest";
                int expectedAmount = expected && (afterRest || fixture.Gender == Gender.Female) ? 1 : 0;
                bool armed = modes.Length == 1 && modes[0].IsOn;
                bool expectedArmed = expected && fixture.Gender == Gender.Female;
                bool exact = IsFixtureUnit(unit, fixture) && resourceCount == (expected ? 1 : 0) &&
                    amount == expectedAmount && modes.Length == (expected ? 1 : 0) &&
                    (afterRest || armed == expectedArmed) && buffs.Length == (armed ? 1 : 0) &&
                    buffs.All(value => value.Active && !value.IsSuppressed) &&
                    (!expected || resource.GetMaxAmount(unit.Descriptor) == 1);
                // Native rest owns whether consent stays enabled. Outside that
                // one observation the saved consent is required exactly.
                bool reconciled = ElementalHeritageRuntime.Reconcile(unit.Descriptor, null, null);
                exact &= reconciled && unit.Descriptor.Resources.GetResourceAmount(resource) == amount &&
                    modes.SequenceEqual(unit.Descriptor.ActivatableAbilities.Enumerable.Where(value =>
                        ReferenceEquals(value.Blueprint, blueprint))) &&
                    buffs.SequenceEqual(unit.Buffs.Enumerable.Where(value =>
                        ReferenceEquals(value.Blueprint, blueprint.Buff))) &&
                    (modes.Length == 0 || modes[0].IsOn == armed);
                var record = new JObject {
                    { "phase", phase }, { "fixture", fixture.Label }, { "expected", expected },
                    { "resourceGuid", resource.AssetGuid }, { "resourceCount", resourceCount },
                    { "amount", amount }, { "expectedAmount", expectedAmount },
                    { "modeGuid", blueprint.AssetGuid }, { "modeCount", modes.Length },
                    { "armed", armed }, { "buffCount", buffs.Length },
                    { "nativeRestConsentObservationOnly", afterRest },
                    { "reconcileAccepted", reconciled }, { "exact", exact }
                };
                Add(_assertions, "elemental-crystalline-persistence-" + phase + "-" + fixture.Label,
                    "exact native saved resource/consent or exact absence; unchanged on reconciliation",
                    record.ToString(Newtonsoft.Json.Formatting.None), exact,
                    "native selected trait, native resource expenditure seed and activatable state; real save/load, rest, level and respec");
                if (!exact) throw new InvalidOperationException("Crystalline persistence diverged: " +
                    record.ToString(Newtonsoft.Json.Formatting.None));
                return record;
            }
        }
    }
}

