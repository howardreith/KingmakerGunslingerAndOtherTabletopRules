using System;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummoningCurrentSurface
    {
        internal const int AppendedCatalogIdentityCount = Sprint26MagicalBeastRules.AppendedCatalogIdentityCount;
        internal const int RegisteredCreatureCount = 110;
        internal const int RegisteredMonsterCreatureCount = 96;
        internal const int RegisteredNatureAllyCreatureCount = 97;
        internal const int RegisteredMonsterPlacements = 545;
        internal const int RegisteredNatureAllyPlacements = 557;
        internal const int RegisteredLogicalPlacements = RegisteredMonsterPlacements + RegisteredNatureAllyPlacements;
        internal const int RegisteredTemplatedPlacements = 319;
        internal const int PublicSuppressedLogicalPlacements = 6;
        internal const int FinalPublishedLogicalPlacements = RegisteredLogicalPlacements - PublicSuppressedLogicalPlacements;
        internal const int PublicVisibleChoices = FinalPublishedLogicalPlacements + 29;
        internal static int PublishedMonsterPlacements { get { return RegisteredMonsterPlacements - (IsWithheld("gorgon") ? 2 : 0); } }
        internal static int PublishedNatureAllyPlacements { get { return RegisteredNatureAllyPlacements - (IsWithheld("bulette") ? 4 : 0); } }
        internal const int IconConceptCount = 122;
        internal static readonly string[] WithheldKeys = { "bulette", "gorgon" };
        internal static bool IsWithheld(string key) { return Array.IndexOf(WithheldKeys, key) >= 0; }
    }

    internal static class ExpandedSummoningSprint26Tests
    {
        internal static void ReachCorrectionPreservesOtherOwnersAndBonuses()
        {
            string gore = "6fa1a7d22d31434b85708d7ba7735649", bite = "f7801f4601554a478920e839ec40d9a4";
            Assertions.Equal(5, 2 + Sprint26MagicalBeastRules.ReachAdjustment(Sprint26MagicalBeastRules.GorgonUnitGuid, gore), "Measured adult Gorgon reach.");
            Assertions.Equal(10, 5 + Sprint26MagicalBeastRules.ReachAdjustment(Sprint26MagicalBeastRules.BuletteUnitGuid, bite), "Measured adult Bulette reach.");
            Assertions.Equal(10, 7 + Sprint26MagicalBeastRules.ReachAdjustment(Sprint26MagicalBeastRules.GorgonUnitGuid, gore), "An added live five-foot native bonus survives.");
            Assertions.Equal(0, Sprint26MagicalBeastRules.ReachAdjustment(Sprint26MagicalBeastRules.GorgonUnitGuid, bite), "Other weapons do not inherit the owner's correction.");
            Assertions.Equal(0, Sprint26MagicalBeastRules.ReachAdjustment("native-horse", gore), "A released donor does not inherit the weapon's correction.");
        }

        internal static void TemporaryStoneDoesNotAcquireNativeDeathDamage()
        {
            Assertions.True(Sprint26MagicalBeastRules.SuppressesNativePetrificationDeath(true, 1, true, false), "Only initialized temporary Gorgon stone suppresses the native petrification-only death predicate.");
            Assertions.False(Sprint26MagicalBeastRules.SuppressesNativePetrificationDeath(true, 2, true, false), "Another petrification source retains native lethal semantics.");
            Assertions.False(Sprint26MagicalBeastRules.SuppressesNativePetrificationDeath(true, 1, true, true), "Permanent stone retains native death/cure semantics.");
            Assertions.False(Sprint26MagicalBeastRules.SuppressesNativePetrificationDeath(true, 1, false, false), "Unowned or uninitialized stone is not a temporary Gorgon stage.");
            Assertions.False(Sprint26MagicalBeastRules.SuppressesNativePetrificationDeath(false, 1, true, false), "No condition is created by the life guard.");
            Assertions.False(Sprint26MagicalBeastRules.SuppressesNativePetrificationDeath(true, 0, true, false), "A missing condition counter cannot suppress native life processing.");
        }

        internal static void BreathStagesRequireAnotherActivation()
        {
            Assertions.False(Sprint26MagicalBeastRules.CanEscalate(false, false, null, null, "second"), "An unrelated stone effect is not a first Gorgon exposure.");
            Assertions.False(Sprint26MagicalBeastRules.CanEscalate(true, false, "first", "first", "first"), "Duplicate callbacks cannot escalate one breath.");
            Assertions.True(Sprint26MagicalBeastRules.CanEscalate(true, false, "first", "first", "second"), "A distinct exposure escalates the active Gorgon effect, including another caster.");
            Assertions.False(Sprint26MagicalBeastRules.CanEscalate(true, true, "first", "second", "third"), "Permanent state grants no new transition or natural recovery.");
            Assertions.False(Sprint26MagicalBeastRules.CanEscalate(true, false, "first", "second", "second"), "The later activation is itself deduplicated.");
        }

        internal static void BreathMathRemainsLive()
        {
            Assertions.Equal(21, Sprint26MagicalBeastRules.BreathDc(8, 7), "Adult baseline.");
            Assertions.Equal(24, Sprint26MagicalBeastRules.BreathDc(10, 9), "Live HD and Constitution both contribute.");
            Assertions.Equal(11, Sprint26MagicalBeastRules.BreathDc(8, -3), "A Constitution penalty is retained.");
            Assertions.Equal(2, Sprint26MagicalBeastRules.CooldownRounds(1), "Actual d4 low endpoint plus one.");
            Assertions.Equal(5, Sprint26MagicalBeastRules.CooldownRounds(4), "Actual d4 high endpoint plus one.");
            bool refused = false;
            try { Sprint26MagicalBeastRules.CooldownRounds(5); } catch (ArgumentOutOfRangeException) { refused = true; }
            Assertions.True(refused, "An invented out-of-die cooldown cannot be accepted.");
        }

        internal static void LeapBonusAndSavageBiteAreConditionalAndSigned()
        {
            foreach (int[] row in new[] { new[] { 40, 8 }, new[] { 30, 4 }, new[] { 20, 0 }, new[] { 10, -4 } })
                Assertions.Equal(row[1], Sprint26MagicalBeastRules.JumpOnlyBonus(row[0]), "Only the actual jump check receives the racial/speed terms.");
            foreach (int[] row in new[] { new[] { 6, 9 }, new[] { 7, 10 }, new[] { 0, 0 }, new[] { -1, -2 }, new[] { -3, -5 } })
                Assertions.Equal(row[1], Sprint26MagicalBeastRules.SavageBiteStrength(row[0]), "Savage Bite uses its explicit signed multiplier and rounds down.");
            Assertions.Equal(3, Sprint26MagicalBeastRules.LonePrimaryExcess(7), "Gorgon's ordinary gore removes only the lone-natural excess.");
            Assertions.Equal(0, Sprint26MagicalBeastRules.LonePrimaryExcess(-3), "A primary gore penalty was never multiplied by the native lone-attack rule.");
        }
    }
}
