using System;
using System.IO;
using System.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>O06 paladin auras and O07/O08 pet natural armor (Phase 3 advanced).</summary>
    internal static class FavoredClassAdvancedTests
    {
        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "FavoredClass" }.Concat(parts).ToArray()));
        }

        private static void Contains(string text, string label, params string[] tokens)
        {
            foreach (string token in tokens)
                Assertions.True(text.Contains(token), label + " token: " + token);
        }

        // O06: the increment lives inside the native Morale modifier of the two
        // ally buffs, computed on the aura's caster; exact shape first, exact rollback.
        internal static void AuraIncrementFoldsIntoTheNativeMoraleModifier()
        {
            string aura = Source("FavoredClassAuraPublication.cs");
            Contains(aura, "aura publication",
                "saves[0].ModifierDescriptor != ModifierDescriptor.Morale",
                "saves[0].Value != NativeMoraleValue",
                "!(saves[0].Bonus.ValueType == ContextValueType.Simple && saves[0].Bonus.Value == 0)",
                "Set(rank, \"m_BaseValueType\", ContextRankBaseValueType.CustomProperty);",
                "Set(rank, \"m_Progression\", ContextRankProgression.AsIs);",
                "save.Bonus = new ContextValue { ValueType = ContextValueType.Rank, ValueRank = RankType };",
                "_bonuses[index].Key.Bonus = _bonuses[index].Value;",
                "_components[index].Key.ComponentsArray = _components[index].Value;",
                "SpellDescriptor.Fear", "SpellDescriptor.Charm");
            Assertions.False(aura.Contains("Size") || aura.Contains("AreaEffect") || aura.Contains("Immunity"),
                "The aura radius and the paladin's immunities are never touched.");
            string property = Source("Mechanics", "FavoredClassEarnedStepsProperty.cs");
            Contains(property, "steps property", "!FavoredClassRuntime.MechanicsEnabled",
                "unit.Descriptor.Progression.Features.GetRank(Feature)");
            string coordinator = Source("FavoredClassIntegrationCoordinator.cs");
            int check = coordinator.IndexOf("FavoredClassAuraPublication.Check(", StringComparison.Ordinal);
            int plan = coordinator.IndexOf("FavoredClassPublication.Plan(", StringComparison.Ordinal);
            int apply = coordinator.IndexOf("FavoredClassAuraPublication.Apply(", StringComparison.Ordinal);
            int commit = coordinator.IndexOf("publication.Commit();", StringComparison.Ordinal);
            Assertions.True(check > 0 && plan > check && commit > plan && apply > commit,
                "Validate, plan, commit, then apply the aura read point.");
            Assertions.True(coordinator.Contains("committedAura.Rollback();") &&
                coordinator.Contains("new[] { FavoredClassCatalog.EffectPaladinAuras }"),
                "A drifted aura contract withholds only O06; a failed publication rolls the aura back.");
            string publication = Source("FavoredClassPublication.cs");
            Assertions.True(publication.Contains("return \"native-contract-unavailable\";"),
                "Publication withholds an effect whose native read point failed validation.");
        }

        // O07/O08: master-owned investment projected onto the current qualified
        // pet only, as native natural armor (stacking, excluded from touch AC).
        internal static void PetArmorFollowsTheCurrentQualifiedPetOnly()
        {
            string pet = Source("Mechanics", "FavoredClassPetNaturalArmor.cs");
            Contains(pet, "pet armor",
                "Owner.Stats.AC.AddModifier(steps, this, ModifierDescriptor.NaturalArmor)",
                "if (master == null || !ReferenceEquals(master.Descriptor.Pet, Owner.Unit) ||",
                "!FavoredClassPets.IsQualified(Owner.Unit, PetClassGuid))",
                "if (previous != null && !ReferenceEquals(previous, current))",
                "Unproject(previous);",
                "if (existing == null)",
                "existing.CallComponents<FavoredClassPetNaturalArmor>(component => component.Refresh());",
                "public void HandleFactionChanged(UnitEntityData unit)",
                "if (IsReapplying)",
                "[JsonProperty]",
                "private UnitReference m_ProjectedPet;",
                "!FavoredClassRuntime.MechanicsEnabled");
            Assertions.False(pet.Contains("RuleCalculateAC") || pet.Contains("Owner.Stats.AC.AddModifier(steps, this, ModifierDescriptor.Dodge"),
                "Pet armor is native natural armor, never an untyped AC bonus that would reach touch AC.");
            string blueprints = Source("FavoredClassBlueprints.cs");
            Contains(blueprints, "pet blueprints",
                "BlueprintRoot.Instance.Progression.AnimalCompanion",
                "internal const string EidolonClassGuid = \"e3b3ad6decb14cdba2e7e14982d90035\";",
                "feature.HideInUI = true;",
                "spec.EffectId == FavoredClassCatalog.EffectCompanionArmor ||",
                "{ FavoredClassCatalog.Summoner, \"0f4c4ada51334b43a802350c5c0b85f5\" },");
        }

        // O06/O07 are not offered when an archetype removes every improved feature.
        internal static void ImprovedFeatureRuleExcludesOnlyFullReplacement()
        {
            string prerequisites = Source("FavoredClassPrerequisites.cs");
            Contains(prerequisites, "feature availability",
                "public sealed class PrerequisiteFavoredClassFeatureAvailable : Prerequisite",
                "!System.Linq.Enumerable.Any(archetypes, archetype => Removes(archetype, feature))",
                "entry.Features.Contains(feature)");
            string blueprints = Source("FavoredClassBlueprints.cs");
            Contains(blueprints, "improved features", "AuraOfCourageFeatureGuid, \"native Aura of Courage\"",
                "AuraOfResolveFeatureGuid, \"native Aura of Resolve\"",
                "HuntersBondSelectionGuid, \"native Hunter's Bond selection\"",
                "BlueprintCharacterClass hostClass = replacing.Count == 0 && improved.Length == 0 ? null :");
        }
    }
}
