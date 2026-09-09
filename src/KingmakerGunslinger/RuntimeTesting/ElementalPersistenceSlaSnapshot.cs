using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private sealed class PersistenceSlaSnapshot
            {
                internal bool CastApplicable, Exact;
                internal BlueprintAbility Ability;
                internal BlueprintAbilityResource Resource;
                internal AbilityData Data, Executable;
                internal int Amount, Maximum, CasterLevel;
                internal bool Available => Executable != null && Executable.IsAvailable;
                internal int AvailableCount => Executable == null ? 0 : Executable.GetAvailableForCastCount();
                internal JObject Evidence => new JObject {
                    ["castApplicable"] = CastApplicable, ["exact"] = Exact,
                    ["abilityGuid"] = Ability == null ? null : Ability.AssetGuid,
                    ["executableAbilityGuid"] = Executable == null ? null : Executable.Blueprint.AssetGuid,
                    ["resourceGuid"] = Resource == null ? null : Resource.AssetGuid,
                    ["amount"] = Amount, ["maximum"] = Maximum, ["casterLevel"] = CasterLevel,
                    ["available"] = Available, ["availableCount"] = AvailableCount };
            }

            private static bool IsPassiveSlaReplacement(ElementalAlternateTraitBlueprints trait)
            {
                return trait != null && trait.Definition.IsPublished &&
                    trait.Definition.Replaces(ElementalRacialTraitSlot.RacialSpellLikeAbility) &&
                    new[] { ElementalAlternateTraitId.BrazenFlame, ElementalAlternateTraitId.ForgeHardened,
                        ElementalAlternateTraitId.Secretive, ElementalAlternateTraitId.WhisperingWind }.Contains(trait.Definition.Id);
            }

            private bool PersistenceSlaAbsentExact(ElementalPersistenceFixture fixture, UnitDescriptor owner,
                ElementalHeritageBlueprints heritage)
            {
                var trait = PersistenceSlaTrait(fixture, heritage);
                return IsPassiveSlaReplacement(trait) &&
                    !trait.Mechanics().OfType<BlueprintAbility>().Any() &&
                    !trait.Mechanics().OfType<BlueprintAbilityResource>().Any() &&
                    owner.Progression.Features.GetRank(trait.Marker) == 1 &&
                    owner.Progression.Features.GetRank(trait.Provider) == 1 &&
                    fixture.Blueprints.Heritages.Choices().All(value =>
                        owner.Progression.Features.GetRank(value.SlaFeature) == 0 &&
                        owner.Abilities.GetAbility(value.SlaAbility) == null &&
                        !owner.Resources.PersistantResources.Any(resource => resource != null && ReferenceEquals(resource.Blueprint, value.SlaResource)));
            }

            private PersistenceSlaSnapshot ObservePersistenceSla(ElementalPersistenceFixture fixture, UnitDescriptor owner,
                ElementalHeritageBlueprints heritage, int expectedAmount, int expectedLevel)
            {
                var result = new PersistenceSlaSnapshot { Ability = PersistenceSlaAbility(fixture, heritage),
                    Resource = PersistenceSlaResource(fixture, heritage) };
                result.CastApplicable = result.Ability != null;
                if (!result.CastApplicable)
                {
                    result.Exact = result.Resource == null && PersistenceSlaAbsentExact(fixture, owner, heritage);
                    return result;
                }
                if (result.Resource == null) throw new InvalidOperationException("A persistence SLA lost its exact daily resource.");
                result.Amount = owner.Resources.GetResourceAmount(result.Resource);
                result.Maximum = result.Resource.GetMaxAmount(owner);
                result.Data = RequireAbility(owner.Unit, result.Ability);
                result.Executable = ResolveExecutableAbility(result.Data);
                result.CasterLevel = result.Executable.CreateExecutionContext(new TargetWrapper(owner.Unit)).Params.CasterLevel;
                var trait = PersistenceSlaTrait(fixture, heritage);
                bool nereid = trait?.Definition.Id == ElementalAlternateTraitId.NereidFascination;
                AbilityType type = nereid || IsBreathTrait(trait) ? AbilityType.Supernatural : AbilityType.SpellLike;
                result.Exact = owner.Unit.Blueprint != null && !owner.Unit.Blueprint.IsCheater &&
                    owner.Resources.PersistantResources.Count(value => value != null && ReferenceEquals(value.Blueprint, result.Resource)) == 1 &&
                    owner.Abilities.Enumerable.Count(value => ReferenceEquals(value.Blueprint, result.Ability)) == 1 &&
                    result.Maximum == 1 && result.Amount == expectedAmount &&
                    (ReferenceEquals(result.Executable.Blueprint, result.Ability) || ReferenceEquals(result.Executable.Blueprint.Parent, result.Ability)) &&
                    result.Data.Blueprint.Type == type && result.Executable.Blueprint.Type == type &&
                    result.Data.Spellbook == null && result.Executable.Spellbook == null &&
                    !result.Data.RequireMaterialComponent && !result.Executable.RequireMaterialComponent &&
                    !result.Data.IsAffectedByArcaneSpellFailure && !result.Executable.IsAffectedByArcaneSpellFailure &&
                    result.AvailableCount == expectedAmount && result.Available == (expectedAmount > 0) &&
                    (expectedLevel == 0 || result.CasterLevel == expectedLevel &&
                        BreathParametersExact(fixture, heritage, result.Executable, expectedLevel)) &&
                    (!nereid || result.Executable.CalculateParams().DC ==
                        ElementalNereidPolicy.DifficultyClass(owner.Progression.CharacterLevel, owner.Stats.Charisma.Bonus));
                return result;
            }
        }
    }
}