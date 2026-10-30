using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class InfernalRules35Tests
    {
        internal static void HiddenRoutesHaveExactScopeAndQuantity()
        {
            var barbed = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "barbed-devil");
            var bone = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "bone-devil");
            Assertions.False(barbed.NaturesAllyTier.HasValue ||
                barbed.MonsterTemplated, "Barbed is Monster-only and untemplated.");
            Assertions.False(bone.NaturesAllyTier.HasValue ||
                bone.MonsterTemplated, "Bone is Monster-only and untemplated.");
            var barbedRoutes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == barbed.Key)
                .OrderBy(value => value.ParentTier).ToArray();
            var boneRoutes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == bone.Key)
                .OrderBy(value => value.ParentTier).ToArray();
            Assertions.True(barbedRoutes.Select(value => value.ParentTier)
                .SequenceEqual(new[] { 8, 9 }), "Barbed has VIII and IX only.");
            Assertions.True(barbedRoutes.Select(value => value.Multiplicity)
                .SequenceEqual(new[] { SummonMultiplicity.One,
                    SummonMultiplicity.OneD3 }),
                "Barbed quantities are one then 1d3.");
            Assertions.True(boneRoutes.Select(value => value.ParentTier)
                .SequenceEqual(new[] { 7, 8, 9 }),
                "Bone has VII through IX only.");
            Assertions.True(boneRoutes.Select(value => value.Multiplicity)
                .SequenceEqual(new[] { SummonMultiplicity.One,
                    SummonMultiplicity.OneD3,
                    SummonMultiplicity.OneD4PlusOne }),
                "Bone quantities are one, 1d3, then 1d4+1.");
            Assertions.True(barbedRoutes.Concat(boneRoutes).All(value =>
                !SummonVisibilityCatalog.IsPublished(value)),
                "All five executable routes remain suppressed for early proof.");
            SummonVisibilityCatalog.Validate();
            SummonChoicePresentationPolicy.Validate();
        }

        internal static void ContactPredicatesStayDistinct()
        {
            Assertions.True(InfernalRules35.QualifiesForBarbedDefense(true,
                true, false, 20, 5),
                "A natural long-reach body strike is not a reach weapon.");
            Assertions.False(InfernalRules35.QualifiesForBarbedDefense(true,
                true, true, 10, 5),
                "A manufactured reach weapon is exempt.");
            Assertions.False(InfernalRules35.QualifiesForBarbedDefense(false,
                true, false, 5, 5), "A miss does not retaliate.");
            Assertions.False(InfernalRules35.QualifiesForBarbedDefense(true,
                false, false, 5, 5), "A ranged strike does not retaliate.");
            Assertions.True(InfernalRules35.QualifiesForClawFear(true, true, 1),
                "A wounding claw qualifies for fear.");
            Assertions.False(InfernalRules35.QualifiesForClawFear(true, true, 0),
                "An absorbed claw does not qualify for fear.");
            Assertions.False(InfernalRules35.QualifiesForClawFear(false, true, 10),
                "Other damage does not qualify for claw fear.");
        }

        internal static void LiveModifiersDerivePrintedContracts()
        {
            Assertions.Equal(20, InfernalRules35.FearDc(12, 4, 0),
                "Barbed baseline fear DC.");
            Assertions.Equal(19, InfernalRules35.FearDc(10, 4, 0),
                "Bone baseline aura DC.");
            Assertions.Equal(18, InfernalRules35.FearDc(12, 2, 0),
                "Drained Charisma remains live.");
            Assertions.Equal(9, InfernalRules35.ImpaleStrengthBonus(6),
                "Printed impale Strength term.");
            Assertions.Equal(4, InfernalRules35.ImpaleStrengthBonus(3),
                "Positive half rounds down.");
            Assertions.Equal(-2, InfernalRules35.ImpaleStrengthBonus(-2),
                "A Strength penalty is not multiplied.");
            InfernalRules35.Validate();
        }

        internal static void PoisonRequiresTheWoundingSting()
        {
            Assertions.True(InfernalRules35.PoisonQualifies(true, true, 1),
                "An exact wounding sting delivers poison.");
            Assertions.False(InfernalRules35.PoisonQualifies(true, true, 0),
                "Absorption prevents injury poison.");
            Assertions.False(InfernalRules35.PoisonQualifies(true, false, 0),
                "A miss never poisons.");
            Assertions.False(InfernalRules35.PoisonQualifies(false, true, 20),
                "Other limbs never poison.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                InfernalRules35.PoisonQualifies(true, true, -1),
                "Negative final damage is invalid input.");
        }
    }
}
