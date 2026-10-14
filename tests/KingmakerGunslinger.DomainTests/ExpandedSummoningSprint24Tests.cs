using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint24Tests
    {
        private static readonly string[] PrivateIdentityTail = {
            TheropodRulesPolicy.DeinonychusTalonSymbol,
            TheropodRulesPolicy.DeinonychusUnitTypeSymbol,
            TheropodRulesPolicy.DeinonychusChassisSymbol,
            TheropodRulesPolicy.TyrannosaurusBiteSymbol,
            TheropodRulesPolicy.TyrannosaurusUnitTypeSymbol,
            TheropodRulesPolicy.TyrannosaurusChassisSymbol,
            TheropodRulesPolicy.TyrannosaurusPowerfulBiteSymbol,
            TheropodRulesPolicy.TyrannosaurusCombatTraitsSymbol,
            TheropodRulesPolicy.TyrannosaurusSwallowedSymbol,
            TheropodRulesPolicy.TyrannosaurusBleedingCriticalSymbol
        };

        internal static void RoutesAreTheExactEighteenHiddenRoots()
        {
            ExpandedSummoningCatalog.Validate();
            SummonVisibilityCatalog.Validate();

            SummonVariantSpec[] monster = NewRoutes(SummonFamily.Monster);
            SummonVariantSpec[] ally = NewRoutes(SummonFamily.NaturesAlly);
            Assertions.Equal(9, monster.Length,
                "The two theropods add nine Monster roots.");
            Assertions.Equal(9, ally.Length,
                "The two theropods add nine Nature's Ally roots.");

            AssertRouteShape(monster, TheropodRulesPolicy.DeinonychusKey,
                new[] { 4, 5, 6, 7, 8, 9 });
            AssertRouteShape(ally, TheropodRulesPolicy.DeinonychusKey,
                new[] { 4, 5, 6, 7, 8, 9 });
            AssertRouteShape(monster, TheropodRulesPolicy.TyrannosaurusKey,
                new[] { 7, 8, 9 });
            AssertRouteShape(ally, TheropodRulesPolicy.TyrannosaurusKey,
                new[] { 7, 8, 9 });

            Assertions.True(monster.All(value => value.Creature
                .TemplatePolicy(SummonFamily.Monster) ==
                    SummonTemplatePolicy.CelestialOrFiendish),
                "Every new Monster root uses only the released templates.");
            Assertions.True(ally.All(value => value.Creature
                .TemplatePolicy(SummonFamily.NaturesAlly) ==
                    SummonTemplatePolicy.CasterAlignment),
                "Every new Ally root stays untemplated under caster alignment.");
            Assertions.True(monster.Concat(ally).All(value =>
                    !SummonVisibilityCatalog.IsPublished(value)),
                "All eighteen new roots remain hidden in the candidate.");
            Assertions.Equal(107, ExpandedSummoningCatalog.All.Count,
                "Exactly two creatures append to the released catalog.");
            Assertions.Equal(TheropodRulesPolicy.RegisteredLogicalPlacements,
                SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "The current global route authority is centralized.");
            Assertions.Equal(TheropodRulesPolicy.HiddenLogicalPlacements,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Exactly the two Sprint 24 route sets are withheld.");
            Assertions.Equal(
                TheropodRulesPolicy.HiddenPublishedLogicalPlacements,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "No released route moves while the candidate is hidden.");
        }

        internal static void DeinonychusHasOneMixedRoleFourAttackRoutine()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For(TheropodRulesPolicy.DeinonychusKey);

            Assertions.True(new[] { profile.HitDice, profile.Strength,
                profile.Dexterity, profile.Constitution, profile.Intelligence,
                profile.Wisdom, profile.Charisma, profile.SpeedFeet,
                profile.NaturalArmor }.SequenceEqual(new[] {
                    4, 15, 15, 19, 2, 12, 14, 60, 3 }),
                "Deinonychus derives the adult profile.");
            Assertions.Equal("Medium", profile.Size,
                "Deinonychus stays Medium.");
            Assertions.Equal("DeinonychusTalon1d8", profile.PrimaryWeapon,
                "The first talon is primary.");
            Assertions.True(profile.AdditionalWeapons.SequenceEqual(new[] {
                    "DeinonychusTalon1d8", "Bite1d6" }),
                "The second talon and bite are primary attacks.");
            Assertions.True(profile.AdditionalSecondaryWeapons.SequenceEqual(
                    new[] { "Claw1d4" }),
                "Plural foreclaws are one secondary rolled attack.");
            Assertions.Equal(4, 1 + profile.AdditionalWeapons.Count +
                profile.AdditionalSecondaryWeapons.Count,
                "The full routine has exactly four attacks.");
            Assertions.True(profile.Facts.Contains("Pounce") &&
                profile.Facts.Contains("ImprovedInitiative") &&
                profile.Facts.Contains("DeinonychusChassis"),
                "Only the exact printed feat, native Pounce and chassis are carried.");
            string[] forbidden = { "Rake", "Rend", "Grab", "Swallow",
                "Fear", "Poison", "PowerfulCharge", "TripDefenseFourLegs" };
            Assertions.False(profile.Facts.Any(fact => forbidden.Any(value =>
                    fact.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)),
                "No cat, ape, grappler, poison or quadruped carrier leaks in.");
        }

        internal static void TyrannosaurusHasOnePowerfulBiteAndPrintedFeats()
        {
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For(TheropodRulesPolicy.TyrannosaurusKey);

            Assertions.True(new[] { profile.HitDice, profile.Strength,
                profile.Dexterity, profile.Constitution, profile.Intelligence,
                profile.Wisdom, profile.Charisma, profile.SpeedFeet,
                profile.NaturalArmor }.SequenceEqual(new[] {
                    18, 32, 13, 19, 2, 15, 10, 40, 14 }),
                "Tyrannosaurus derives the adult profile.");
            Assertions.Equal("Gargantuan", profile.Size,
                "Tyrannosaurus stays Gargantuan.");
            Assertions.Equal("TyrannosaurusBite4d6", profile.PrimaryWeapon,
                "Its only attack is the private 4d6 bite.");
            Assertions.Equal(0, profile.AdditionalWeapons.Count +
                profile.AdditionalSecondaryWeapons.Count,
                "No claws, tail or follow-up attack are invented.");
            foreach (string fact in new[] { "CriticalFocus", "Diehard",
                "Endurance", "ImprovedCriticalBite", "ImprovedInitiative",
                "IronWill", "SkillFocusPerception",
                "TyrannosaurusChassis", "TyrannosaurusPowerfulBite",
                "TyrannosaurusBleedingCritical" })
                Assertions.True(profile.Facts.Contains(fact),
                    "Printed or exact carrier is present: " + fact);
            Assertions.False(profile.Facts.Contains("PowerAttack") ||
                profile.Facts.Contains("Pounce"),
                "Power Attack and Pounce are not inferred from dinosaur flavor.");
        }

        internal static void HitPointsRanksAndRacialSkillsAreDerived()
        {
            TheropodRulesPolicy.Validate();
            Assertions.Equal(34,
                TheropodRulesPolicy.DeinonychusBaseRacialHitPoints + 4 * 4,
                "Four d8 animal hit dice derive 34 hp from the frozen base.");
            Assertions.Equal(153,
                TheropodRulesPolicy.TyrannosaurusBaseRacialHitPoints + 18 * 4,
                "Eighteen d8 animal hit dice derive 153 hp from the frozen base.");
            Assertions.Equal(14, TheropodRulesPolicy.DeinonychusPerceptionRanks +
                3 + 1 + TheropodRulesPolicy.DeinonychusRacialSkillBonus,
                "Deinonychus Perception is ranks, class, Wisdom and racial bonus.");
            Assertions.Equal(15, TheropodRulesPolicy.DeinonychusStealthRanks +
                3 + 2 + TheropodRulesPolicy.DeinonychusRacialSkillBonus,
                "Deinonychus Stealth is ranks, class, Dexterity and racial bonus.");
            Assertions.Equal(37, TheropodRulesPolicy.TyrannosaurusPerceptionRanks +
                3 + 2 + TheropodRulesPolicy.TyrannosaurusRacialPerceptionBonus +
                3 + TheropodRulesPolicy.TyrannosaurusSkillFocusCompletionBonus,
                "Tyrannosaurus Perception derives every printed contribution.");
        }

        internal static void PowerfulBiteCorrectsOnlyTheSignedStrengthTerm()
        {
            var expectedCorrection = new Dictionary<int, int> {
                { 9, 5 }, { 10, 5 }, { 11, 6 }, { 12, 6 },
                { 0, 0 }, { -1, -1 }, { -5, -5 }
            };
            foreach (KeyValuePair<int, int> sample in expectedCorrection)
            {
                int native = TheropodRulesPolicy
                    .NativeOrdinaryBiteStrengthContribution(sample.Key);
                int correction = TheropodRulesPolicy.PowerfulBiteCorrection(
                    sample.Key);
                Assertions.Equal(sample.Value, correction,
                    "Only the missing signed Strength difference is added for " +
                    sample.Key + ".");
                Assertions.Equal(sample.Key * 2, checked(native + correction),
                    "The final bite Strength contribution is exactly twice " +
                    sample.Key + ".");
            }
            Assertions.Equal(22,
                TheropodRulesPolicy.NativeOrdinaryBiteStrengthContribution(11) +
                TheropodRulesPolicy.PowerfulBiteCorrection(11),
                "Strength 32 yields the printed +22 bite term.");
        }

        internal static void GrabAndLaterSwallowHaveDifferentSizeGates()
        {
            Assertions.True(TheropodRulesPolicy.CanGrab(3, 3),
                "A Gargantuan Tyrannosaurus may Grab a same-size target.");
            Assertions.False(TheropodRulesPolicy.CanGrab(3, 4),
                "Grab does not accept a larger target.");
            Assertions.False(TheropodRulesPolicy.CanSwallow(3, 3),
                "A same-size mouth hold never makes Swallow Whole legal.");
            Assertions.True(TheropodRulesPolicy.CanSwallow(3, 2),
                "Swallow Whole accepts a target one size smaller.");
            Assertions.Equal(4, TheropodRulesPolicy.TyrannosaurusGrappleBonus,
                "The exact bite grapple owns the printed +4 contribution.");
            Assertions.Equal(2,
                TheropodRulesPolicy.TyrannosaurusSwallowDiceCount,
                "Periodic interior damage rolls two dice.");
            Assertions.Equal(11,
                TheropodRulesPolicy.TyrannosaurusSwallowFlatDamage,
                "Periodic interior damage uses +11, not Powerful Bite +22.");
        }

        internal static void BleedingCriticalRejectsEveryFalsePositive()
        {
            Func<bool, bool, bool, bool, bool, bool, bool, bool> decide =
                TheropodRulesPolicy.ShouldApplyBleedingCritical;
            Assertions.True(decide(true, true, true, true, false, true, true),
                "The first confirmed exact bite critical may bleed.");
            Assertions.False(decide(false, true, true, true, false, true, true),
                "Another owner cannot bleed.");
            Assertions.False(decide(true, false, true, true, false, true, true),
                "Another weapon cannot bleed.");
            Assertions.False(decide(true, true, false, true, false, true, true),
                "A miss cannot bleed.");
            Assertions.False(decide(true, true, true, false, false, true, true),
                "A threat without native confirmation cannot bleed.");
            Assertions.False(decide(true, true, true, true, true, true, true),
                "Critical immunity is respected.");
            Assertions.False(decide(true, true, true, true, false, false, true),
                "An unavailable target cannot receive the buff.");
            Assertions.False(decide(true, true, true, true, false, true, false),
                "A duplicate callback cannot apply bleed twice.");
        }

        internal static void IdentityAppendOwnsExactlyTenPrivateCarriers()
        {
            IReadOnlyList<SummoningIdentitySpec> identities =
                ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(2149, identities.Count,
                "The foundation appends exactly forty-eight Sprint 24 identities.");
            SummoningIdentitySpec[] tail = identities
                .Skip(identities.Count - PrivateIdentityTail.Length).ToArray();
            Assertions.True(tail.Select(value => value.Symbol)
                    .SequenceEqual(PrivateIdentityTail),
                "The ten private theropod carriers append once in frozen order.");
            var types = tail.ToDictionary(value => value.Symbol,
                value => value.PlannedType, StringComparer.Ordinal);
            Assertions.Equal("BlueprintItemWeapon",
                types[TheropodRulesPolicy.DeinonychusTalonSymbol],
                "The talon is a private weapon.");
            Assertions.Equal("BlueprintItemWeapon",
                types[TheropodRulesPolicy.TyrannosaurusBiteSymbol],
                "The powerful bite is a private weapon.");
            Assertions.Equal("BlueprintBuff",
                types[TheropodRulesPolicy.TyrannosaurusSwallowedSymbol],
                "The Tyrannosaurus owns its distinct periodic payload.");
        }

        private static SummonVariantSpec[] NewRoutes(SummonFamily family)
        {
            return ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => TheropodRulesPolicy.UsesExactRanks(
                    value.Creature.Key)).ToArray();
        }

        private static void AssertRouteShape(IEnumerable<SummonVariantSpec> source,
            string key, int[] tiers)
        {
            SummonVariantSpec[] routes = source.Where(value =>
                value.Creature.Key == key).OrderBy(value =>
                    value.ParentTier).ToArray();
            Assertions.True(routes.Select(value => value.ParentTier)
                    .SequenceEqual(tiers),
                key + " has the exact parent tiers.");
            SummonMultiplicity[] expected = tiers.Select((value, index) =>
                index == 0 ? SummonMultiplicity.One :
                index == 1 ? SummonMultiplicity.OneD3 :
                    SummonMultiplicity.OneD4PlusOne).ToArray();
            Assertions.True(routes.Select(value => value.Multiplicity)
                    .SequenceEqual(expected),
                key + " has one, 1d3 and then 1d4+1 quantities.");
        }
    }
}
