using System;
using System.Linq;
using System.Collections.Generic;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummoningCurrentSurface
    {
        internal static IReadOnlyList<string> SourceRepresentedKeys { get {
            return ExpandedSummoningCatalog.All.Select(value => value.Key)
                .Concat(SummonNativeExpansionCatalog.All.Select(value => ExpandedSummoningCoveragePolicy.CanonicalKey(value.CreatureKey)))
                .Distinct(StringComparer.Ordinal).ToArray();
        } }
        internal static int SourceUnrepresentedCount { get {
            return ExpandedSummoningIdealRosterCatalog.All.Count(value => !SourceRepresentedKeys.Contains(value.Key));
        } }
        internal const int Sprint29PrivateIdentityCount = 52;
        internal const int Sprint29FoundationIdentityCount = 3 + 8 + Sprint29PrivateIdentityCount;
        internal const int Sprints30And31PrivateIdentityCount = 112;
        internal const int Sprints30And31FoundationIdentityCount = 5 + 23 + Sprints30And31PrivateIdentityCount;
        internal const int Sprint32PrivateIdentityCount = 77;
        internal const int AppendedCatalogIdentityCount = Sprint26MagicalBeastRules.AppendedCatalogIdentityCount + ExpandedSummoningSprint27Tests.AppendedCatalogIdentityCount + GiantHumanoidRulesPolicy.AppendedCatalogIdentityCount + Sprint29FoundationIdentityCount + Sprints30And31FoundationIdentityCount + 2 + 10 + Sprint32PrivateIdentityCount;
        internal const int RegisteredCreatureCount = 124;
        internal const int RegisteredMonsterCreatureCount = 102;
        internal const int RegisteredNatureAllyCreatureCount = 105;
        internal const int RegisteredMonsterPlacements = 572;
        internal const int RegisteredNatureAllyPlacements = 588;
        internal const int RegisteredLogicalPlacements = RegisteredMonsterPlacements + RegisteredNatureAllyPlacements;
        internal const int RegisteredTemplatedPlacements = 319;
        internal const int PublicSuppressedLogicalPlacements = 0;
        internal const int FinalPublishedLogicalPlacements = RegisteredLogicalPlacements - PublicSuppressedLogicalPlacements;
        internal const int PublicVisibleChoices = FinalPublishedLogicalPlacements + 29;
        internal static int PublishedMonsterPlacements { get { return RegisteredMonsterPlacements - (IsWithheld("gorgon") ? 2 : 0) - (IsWithheld("lillend-azata") ? 4 : 0); } }
        internal static int PublishedNatureAllyPlacements { get { return RegisteredNatureAllyPlacements - (IsWithheld("bulette") ? 4 : 0) - (IsWithheld("griffon") ? 6 : 0) - (IsWithheld("hill-giant") ? 4 : 0) - (IsWithheld("stone-giant") ? 4 : 0) - (IsWithheld("fire-giant") ? 3 : 0) - (IsWithheld("ettin") ? 5 : 0) - (IsWithheld("cloud-giant") ? 2 : 0) - (IsWithheld("storm-giant") ? 1 : 0) - (IsWithheld("satyr") ? 6 : 0); } }
        // Seven split-native, four preserved-native, plus Remove Stirge,
        // Trumpet Blast and the approved private Lullaby spell/effect concept.
        internal const int IconConceptCount = ExpandedSummoningIdentityCatalog.UnitCount + 7 + 4 + 3;
        internal static readonly string[] WithheldKeys = new string[0];
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

        internal static void OwnCarrierDoesNotMirrorIntoPermanentNativeStone()
        {
            string own = Sprint26MagicalBeastRules.PetrificationGuid;
            Assertions.True(Sprint26MagicalBeastRules.SuppliesOwnPetrifiedStatus(own, true, 1, false), "The exact sole condition carrier supplies its cloned native stone status directly.");
            Assertions.False(Sprint26MagicalBeastRules.SuppliesOwnPetrifiedStatus(Sprint26MagicalBeastRules.PetrifiedDonor, true, 1, false), "Native petrification keeps the original status factory.");
            Assertions.False(Sprint26MagicalBeastRules.SuppliesOwnPetrifiedStatus(own, false, 1, false), "Another victim's carrier cannot alter this state.");
            Assertions.False(Sprint26MagicalBeastRules.SuppliesOwnPetrifiedStatus(own, true, 2, false), "Overlapping conditions retain native status processing.");
            Assertions.False(Sprint26MagicalBeastRules.SuppliesOwnPetrifiedStatus(own, true, 0, false), "Condition removal still reaches native deactivation.");
            Assertions.False(Sprint26MagicalBeastRules.SuppliesOwnPetrifiedStatus(own, true, 1, true), "Native immunity still deactivates status presentation.");
        }

        internal static void AutomaticRecoveryPreservesOnlyOwnedInitializedStone()
        {
            string own=Sprint26MagicalBeastRules.PetrificationGuid;
            Assertions.True(Sprint26MagicalBeastRules.KeepsStoneFromAutomaticRecovery(own,true,true,true),"Both initialized stages survive native automatic out-of-combat recovery.");
            Assertions.False(Sprint26MagicalBeastRules.KeepsStoneFromAutomaticRecovery(Sprint26MagicalBeastRules.PetrifiedDonor,true,true,true),"An unrelated native carrier keeps native automatic behavior.");
            Assertions.False(Sprint26MagicalBeastRules.KeepsStoneFromAutomaticRecovery(own,false,true,true),"Another owner's carrier grants no exception.");
            Assertions.False(Sprint26MagicalBeastRules.KeepsStoneFromAutomaticRecovery(own,true,false,true),"Actual condition removal or immunity permits native recovery.");
            Assertions.False(Sprint26MagicalBeastRules.KeepsStoneFromAutomaticRecovery(own,true,true,false),"Uninitialized state is never proof of an owned effect.");
            Assertions.False(Sprint26MagicalBeastRules.KeepsStoneFromAutomaticRecovery(null,false,true,true),"A removed cure carrier cannot block recovery.");
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
            Assertions.True(Sprint26MagicalBeastRules.UsesMouthOrigin(Sprint26MagicalBeastRules.GorgonUnitGuid,Sprint26MagicalBeastRules.BreathProjectileGuid), "Only the exact own projectile receives its native jaw position.");
            Assertions.False(Sprint26MagicalBeastRules.UsesMouthOrigin(Sprint26MagicalBeastRules.BuletteUnitGuid,Sprint26MagicalBeastRules.BreathProjectileGuid), "Other owners retain native projectile origins.");
            Assertions.False(Sprint26MagicalBeastRules.UsesMouthOrigin(Sprint26MagicalBeastRules.GorgonUnitGuid,"f6544caac8fe528489327cd86a84b025"), "The shared native cone and all its other consumers retain their original origin.");
            Assertions.False(Sprint26MagicalBeastRules.UsesMouthOrigin(null,null), "Missing identity grants no visual override.");
            Assertions.Equal(21, Sprint26MagicalBeastRules.BreathDc(8, 7), "Adult baseline.");
            Assertions.Equal(24, Sprint26MagicalBeastRules.BreathDc(10, 9), "Live HD and Constitution both contribute.");
            Assertions.Equal(11, Sprint26MagicalBeastRules.BreathDc(8, -3), "A Constitution penalty is retained.");
            Assertions.Equal(2, Sprint26MagicalBeastRules.CooldownRounds(1), "Actual d4 low endpoint plus one.");
            Assertions.Equal(5, Sprint26MagicalBeastRules.CooldownRounds(4), "Actual d4 high endpoint plus one.");
            bool refused = false;
            try { Sprint26MagicalBeastRules.CooldownRounds(5); } catch (ArgumentOutOfRangeException) { refused = true; }
            Assertions.True(refused, "An invented out-of-die cooldown cannot be accepted.");
        }

        internal static void ChargeRequiresActualRunupBeyondCurrentReach()
        {
            float minimum = Sprint26MagicalBeastRules.ChargeMinimumMeters;
            Assertions.True(Sprint26MagicalBeastRules.ChargeTravelIsLegal(minimum), "Exactly ten feet is legal actual charge travel.");
            Assertions.False(Sprint26MagicalBeastRules.ChargeTravelIsLegal(minimum - .001f), "A successful jump roll cannot excuse insufficient charge movement.");
            Assertions.False(Sprint26MagicalBeastRules.ChargeTravelIsLegal(2.40459514f), "The retained failed native prototype must be refused.");
            Assertions.True(Sprint26MagicalBeastRules.ChargeHasRunup(12f, 5.548f), "A normal long native lane leaves enough movement outside attack approach.");
            Assertions.False(Sprint26MagicalBeastRules.ChargeHasRunup(7.53f, 5.548f), "Large reach and collision radii cannot consume the required run-up.");
            Assertions.False(Sprint26MagicalBeastRules.ChargeHasRunup(12f, 10f), "A live reach increase changes the available run-up.");
            Assertions.False(Sprint26MagicalBeastRules.ChargeTravelIsLegal(float.NaN), "Unknown travel is never legal.");
            Assertions.False(Sprint26MagicalBeastRules.ChargeTravelIsLegal(float.PositiveInfinity), "Nonfinite movement is never proof.");
            Assertions.False(Sprint26MagicalBeastRules.ChargeHasRunup(12f, -1f), "An invalid approach radius cannot grant distance.");
        }

        internal static void PersistenceOwnsOnlyTheTwoAssignedCreatures()
        {
            string scope = KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.MagicalBeastsScope;
            foreach (string key in new[] { "gorgon", "bulette" }) {
                var variant = KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.SelectCreatureVariant(scope, key);
                Assertions.Equal(key, variant.Creature.Key, "Exact named scope can prepare the registered withheld identity.");
                Assertions.Equal(SummonMultiplicity.One, variant.Multiplicity, "The fixture creates one of each source.");
            }
            Assertions.True(KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(scope, new[] {"bulette","gorgon"}), "Cold-load order does not change exact membership.");
            Assertions.False(KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(scope, new[] {"gorgon","gorgon"}), "Duplicate source cannot replace the other creature.");
            Assertions.False(KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(scope, new[] {"gorgon","bulette","wolf"}), "No unrelated actor may enter the selected roster.");
            bool refused=false;
            try { KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.SelectCreatureVariant(scope,"griffon"); } catch(InvalidOperationException) {refused=true;}
            Assertions.True(refused,"Sprint27 is outside this scope.");
            foreach(string scenario in new[] {"working-save-expanded-summoning-prepare","working-save-expanded-summoning-verify-cleanup","working-save-expanded-summoning-verify-absent"})
                Assertions.True(KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.ObserveLoad(scenario,scope),"All three existing guarded stages recognize the same named scope.");
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
