using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FavoredClassCatalogTests
    {
        // Charter sections 1 and 13: 53 distinct options in 54 appearances.
        internal static void InventoryReconcilesToTheCharterCounts()
        {
            IList<FavoredClassSourceRow> rows = FavoredClassCatalog.Rows;
            Assertions.Equal(54, rows.Count, "Table appearances.");
            Assertions.Equal(54, rows.Select(row => row.Id).Distinct(StringComparer.Ordinal).Count(),
                "Row identifiers must be unique.");
            IList<FavoredClassSourceRow> distinct = rows.Where(
                row => row.Disposition != FavoredClassDisposition.Alias).ToList();
            Assertions.Equal(53, distinct.Count, "Distinct published options.");
            Assertions.Equal(46, distinct.Count(row => row.Publisher == FavoredClassPublisher.Paizo),
                "Paizo options.");
            Assertions.Equal(7, distinct.Count(
                row => row.Publisher == FavoredClassPublisher.JonBrazerEnterprises),
                "Jon Brazer Enterprises options.");
            Assertions.Equal(15, rows.Count(row => row.SourceTable == "Gunslinger" &&
                row.Publisher == FavoredClassPublisher.Paizo),
                "Fourteen race-specific first-party Gunslinger rows plus universal Kitsune.");
            Assertions.Equal(22, rows.Count(row => row.SourceTable == "Gunslinger"),
                "Gunslinger-associated source rows.");
            foreach (string race in new[] { "Ifrit", "Oread", "Sylph", "Undine" })
                Assertions.Equal(8, rows.Count(row => row.SourceTable == race),
                    race + " rows.");
            Assertions.True(rows.Where(row => row.Publisher ==
                FavoredClassPublisher.JonBrazerEnterprises).Select(row => row.Id)
                .SequenceEqual(new[] { "G16", "G17", "G18", "G19", "G20", "G21", "G22" }),
                "Only G16-G22 are third-party rows.");
        }

        // Charter section 3.8 dispositions.
        internal static void DispositionsMatchTheAdoptedScope()
        {
            AssertIds(FavoredClassDisposition.Implement, "G01 G02 G04 G05 G06 G07 G08 G10 G11 G14 " +
                "I01 I06 I08 O01 O05 O06 O07 O08 S04 S06 U02 U04");
            AssertIds(FavoredClassDisposition.Adaptation, "I05 I07 O04");
            AssertIds(FavoredClassDisposition.OptionalThirdParty, "G16 G17 G18 G20 G21");
            AssertIds(FavoredClassDisposition.ProviderDeferred, "G09 G12 G13 G22");
            AssertIds(FavoredClassDisposition.EngineeringDeferred, "G03 G15 G19 S03 S08 U06 U07");
            AssertIds(FavoredClassDisposition.Excluded,
                "I02 I03 O02 O03 S01 S02 S05 S07 U01 U03 U05 U08");
            AssertIds(FavoredClassDisposition.Alias, "I04");
            Assertions.Equal("G11", FavoredClassCatalog.Row("I04").AliasOf, "I04 aliases G11.");
            Assertions.Equal(30, FavoredClassCatalog.Rows.Count(row => row.IsScheduled),
                "Scheduled rows: 22 faithful, 3 adapted, 5 third-party.");
        }

        // Unscheduled rows can never reach a menu; scheduled rows always do.
        internal static void EveryScheduledRowReachesAnEffectAndNoOtherRowDoes()
        {
            foreach (FavoredClassSourceRow row in FavoredClassCatalog.Rows)
            {
                IList<FavoredClassEffectSpec> effects = FavoredClassCatalog.EffectsForRow(row.Id);
                if (row.IsScheduled)
                    Assertions.True(effects.Count > 0, row.Id + " must reach an effect.");
                else if (row.Disposition == FavoredClassDisposition.Alias)
                    Assertions.True(effects.Select(effect => effect.Id).SequenceEqual(
                        FavoredClassCatalog.EffectsForRow(row.AliasOf).Select(effect => effect.Id)),
                        row.Id + " must reach exactly its aliased row's effects.");
                else
                    Assertions.Equal(0, effects.Count, row.Id + " must never be published.");
                Assertions.Equal(row.IsScheduled ? ProfileFor(row.Disposition)
                    : FavoredClassProfile.None, row.Profile, row.Id + " profile.");
            }
            Assertions.True(FavoredClassCatalog.EffectsForRow("G06").Select(effect => effect.Id)
                .SequenceEqual(new[] { FavoredClassCatalog.EffectHalflingNimble,
                    FavoredClassCatalog.EffectHalflingDodge }),
                "G06 has two branches with two counters.");
            HashSet<string> known = new HashSet<string>(
                FavoredClassCatalog.Rows.Select(row => row.Id), StringComparer.Ordinal);
            foreach (FavoredClassEffectSpec effect in FavoredClassCatalog.Effects)
                foreach (string id in effect.Rows)
                    Assertions.True(known.Contains(id), effect.Id + " references " + id);
        }

        // Rates, caps and source corrections (charter sections 3 and 7).
        internal static void RatesCapsAndSourceCorrectionsAreExact()
        {
            AssertRate(FavoredClassCatalog.EffectGrit, 4, null);
            AssertRate(FavoredClassCatalog.EffectMisfire, 4, null);
            AssertRate(FavoredClassCatalog.EffectFirearmConfirmation, 3, 5);
            AssertRate(FavoredClassCatalog.EffectPistolWhip, 3, null);
            AssertRate(FavoredClassCatalog.EffectHalflingNimble, 4, 2);
            AssertRate(FavoredClassCatalog.EffectHalflingDodge, 4, null);
            AssertRate(FavoredClassCatalog.EffectDrowNimble, 6, 2);
            AssertRate(FavoredClassCatalog.EffectInitiative, 2, null);
            AssertRate(FavoredClassCatalog.EffectDirtyTrickTrip, 2, null);
            AssertRate(FavoredClassCatalog.EffectBombDamage, 2, null);
            AssertRate(FavoredClassCatalog.EffectFireIntimidate, 2, null);
            AssertRate(FavoredClassCatalog.EffectSelectedRevelation, 6, null);
            AssertRate(FavoredClassCatalog.EffectDemoralize, 2, null);
            // I08/S06 adopt the +2 cap (AoN and legacy PRD), not d20PFSRD's +4.
            AssertRate(FavoredClassCatalog.EffectSelectedBloodlinePower, 6, 2);
            AssertRate(FavoredClassCatalog.EffectPerformanceRange, 1, 6);
            AssertRate(FavoredClassCatalog.EffectBullRushDragDefense, 1, null);
            AssertRate(FavoredClassCatalog.EffectUnarmedConfirmation, 3, 5);
            AssertRate(FavoredClassCatalog.EffectPaladinAuras, 4, null);
            AssertRate(FavoredClassCatalog.EffectCompanionArmor, 4, null);
            AssertRate(FavoredClassCatalog.EffectEidolonArmor, 4, null);
            AssertRate(FavoredClassCatalog.EffectAquaticPenetration, 1, null);
            AssertRate(FavoredClassCatalog.EffectGrappleStunning, 3, null);
            Assertions.Equal(22, FavoredClassCatalog.Effects.Count, "Canonical effect count.");
            Assertions.Equal(22, FavoredClassCatalog.Effects.Select(effect => effect.Id)
                .Distinct(StringComparer.Ordinal).Count(), "Effect identities must be unique.");
        }

        // M28: distinct source variants are never normalized by display name.
        internal static void DistinctVariantsKeepTheirOwnRatesAndProfiles()
        {
            FavoredClassEffectSpec halfling = FavoredClassCatalog.Effect(
                FavoredClassCatalog.EffectHalflingNimble);
            FavoredClassEffectSpec drow = FavoredClassCatalog.Effect(
                FavoredClassCatalog.EffectDrowNimble);
            Assertions.False(halfling.Rows.Intersect(drow.Rows).Any(),
                "Drow and Halfling Nimble must remain separate ledgers.");
            Assertions.Equal(FavoredClassProfile.FirstParty, FavoredClassCatalog.Row("G05").Profile,
                "Half-orc Pistol-Whip is first-party.");
            Assertions.Equal(FavoredClassProfile.ThirdParty, FavoredClassCatalog.Row("G20").Profile,
                "Orc Pistol-Whip is third-party.");
            Assertions.Equal(FavoredClassProfile.ThirdParty, FavoredClassCatalog.Row("G16").Profile,
                "Dhampir confirmation is third-party.");
            Assertions.Equal(FavoredClassProfile.FirstParty, FavoredClassCatalog.Row("G14").Profile,
                "Fetchling grit (Blood of Shadows) is first-party.");
            Assertions.Equal(FavoredClassDisposition.EngineeringDeferred,
                FavoredClassCatalog.Row("G19").Disposition,
                "The third-party Fetchling darkness option stays deferred.");
            foreach (string id in new[] { "G09", "G12", "G13", "G22", "G03", "G15", "G19",
                "S03", "S08", "U06", "U07" })
                Assertions.Equal(0, FavoredClassCatalog.EffectsForRow(id).Count,
                    id + " must not appear as a placeholder.");
        }

        // Adaptations disclose their omitted tabletop portion; faithful rows do not.
        internal static void AdaptationsDiscloseTheirOmissions()
        {
            foreach (FavoredClassEffectSpec effect in FavoredClassCatalog.Effects)
            {
                bool adapted = effect.Rows.Any(id => FavoredClassCatalog.Row(id).Disposition ==
                    FavoredClassDisposition.Adaptation);
                if (adapted)
                    Assertions.False(string.IsNullOrWhiteSpace(effect.OmittedPortion),
                        effect.Id + " must disclose its omitted portion.");
                else
                    Assertions.True(effect.OmittedPortion == null,
                        effect.Id + " is faithful and must not claim an omission.");
            }
        }

        private static FavoredClassProfile ProfileFor(FavoredClassDisposition disposition)
        {
            switch (disposition)
            {
                case FavoredClassDisposition.Implement:
                    return FavoredClassProfile.FirstParty;
                case FavoredClassDisposition.Adaptation:
                    return FavoredClassProfile.Adaptation;
                case FavoredClassDisposition.OptionalThirdParty:
                    return FavoredClassProfile.ThirdParty;
                default:
                    return FavoredClassProfile.None;
            }
        }

        private static void AssertIds(FavoredClassDisposition disposition, string expected)
        {
            string[] ids = FavoredClassCatalog.Rows.Where(row => row.Disposition == disposition)
                .Select(row => row.Id).ToArray();
            string[] want = expected.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Assertions.True(ids.OrderBy(id => id, StringComparer.Ordinal).SequenceEqual(
                want.OrderBy(id => id, StringComparer.Ordinal)),
                disposition + " rows were " + string.Join(" ", ids));
        }

        private static void AssertRate(string effectId, int divisor, int? cap)
        {
            FavoredClassRate rate = FavoredClassCatalog.Effect(effectId).Rate;
            Assertions.Equal(divisor, rate.Divisor, effectId + " divisor.");
            Assertions.Equal(cap, rate.CapSteps, effectId + " cap.");
        }
    }
}
