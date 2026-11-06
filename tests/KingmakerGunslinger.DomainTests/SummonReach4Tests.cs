using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    // Sprint 4 summon reach. Behaviour is evaluated with the installed
    // contract (decompiled Kingmaker 2.1.7b and ProperFlanking2 1.2.3-2.1):
    // - native IsReach: edge gap < live item range, in meters;
    // - ProperFlanking2: a melee weapon whose type range exceeds 2 ft also
    //   needs edge gap >= (size reach + 1) ft x 0.5;
    // - live item range: type range + ReachRange + exact-item corrections.
    internal static class SummonReach4Tests
    {
        private const double FootMeters = .3048d;
        private const string GlaiveOwner = "182032f380314a7581d0016c443945a2";
        private const string Glaive = "804dc1aecc9f43b9bcad0c48547c8b28";
        private const string LongspearOwner = "1512476cf6674704b6b0bcc3b67b48d9";
        private const string Longspear = "979ea8542538458184a4ae084c8d7f73";

        private static bool Threatens(double edgeGapMeters, int liveRangeFeet, int rawTypeFeet, int sizeReachFeet)
        {
            bool native = edgeGapMeters < liveRangeFeet * FootMeters;
            bool deadZone = SummonReachRepresentation4.ProperFlanking2ReachWeapon(true, rawTypeFeet) &&
                edgeGapMeters < (sizeReachFeet + 1) * FootMeters * .5d;
            return native && !deadZone;
        }

        private static int SizeReachFeet(SummonReachRepresentation4.Row row)
        {
            // Census: size reach is Small/Medium 5, Large 9, Huge 12,
            // Gargantuan 15; the armoured dinosaurs add chassis Reach bonuses.
            if (row.OwnerKey == "ankylosaurus" || row.OwnerKey == "stegosaurus") return 12;
            if (row.OwnerKey == "brachiosaurus") return 15;
            return row.OwnerReachRangeFeet + 5;
        }

        internal static void RowsPreserveLiveRangeAndLeaveTheDeadZone()
        {
            Assertions.Equal(45, SummonReachRepresentation4.Rows.Length, "Every falsely classified census row is represented.");
            Assertions.Equal(2, SummonReachRepresentation4.NativeTypeRangeFeet(SummonReachRepresentation4.StoredRawFeet),
                "The stored raw five feet is the native minimum weapon range.");
            foreach (var row in SummonReachRepresentation4.Rows)
            {
                string name = row.OwnerKey + " " + row.ItemName;
                Assertions.True(SummonReachRepresentation4.ProperFlanking2ReachWeapon(true, row.PreviousRawFeet),
                    name + ": the previous type was a ProperFlanking2 reach weapon.");
                Assertions.False(SummonReachRepresentation4.ProperFlanking2ReachWeapon(true, SummonReachRepresentation4.StoredRawFeet),
                    name + ": the stored type is not a ProperFlanking2 reach weapon.");
                Assertions.Equal(row.LiveRangeBeforeFeet, row.LiveRangeAfterFeet, name + ": the live wielded range is unchanged.");
                Assertions.True(row.Kind == SummonReachRepresentation4.Natural ||
                    row.Kind == SummonReachRepresentation4.NaturalExtended ||
                    row.Kind == SummonReachRepresentation4.OrdinaryManufactured, name + ": classified.");
            }
        }

        internal static void AdjacentLegalOuterRangeKeptBeyondRejected()
        {
            foreach (var row in SummonReachRepresentation4.Rows)
            {
                string name = row.OwnerKey + " " + row.ItemName;
                int size = SizeReachFeet(row);
                double adjacent = .15d;
                double inside = row.LiveRangeAfterFeet * FootMeters - .10d;
                double beyond = row.LiveRangeAfterFeet * FootMeters + .10d;
                Assertions.False(Threatens(adjacent, row.LiveRangeBeforeFeet, row.PreviousRawFeet, size),
                    name + ": before, ProperFlanking2 refused the adjacent target.");
                Assertions.True(Threatens(adjacent, row.LiveRangeAfterFeet, SummonReachRepresentation4.StoredRawFeet, size),
                    name + ": after, the adjacent target is legal.");
                Assertions.True(Threatens(inside, row.LiveRangeBeforeFeet, row.PreviousRawFeet, size) &&
                    Threatens(inside, row.LiveRangeAfterFeet, SummonReachRepresentation4.StoredRawFeet, size),
                    name + ": just inside the outer range stays legal.");
                Assertions.False(Threatens(beyond, row.LiveRangeAfterFeet, SummonReachRepresentation4.StoredRawFeet, size),
                    name + ": just beyond the outer range stays rejected.");
            }
        }

        internal static void GenuineReachWeaponsKeepTheirDeadZone()
        {
            foreach (var weapon in new[] { new[] { GlaiveOwner, Glaive }, new[] { LongspearOwner, Longspear } })
            {
                Assertions.True(SummonReachRepresentation4.Find(weapon[0], weapon[1]) == null, "A genuine reach weapon is not converted.");
                Assertions.Equal(0, SummonReachRepresentation4.CorrectionFeet(weapon[0], weapon[1]), "No correction applies to it.");
                // Medium wielder; raw 10 (6 ft type) plus its existing exact +4.
                int live = SummonReachRepresentation4.NativeTypeRangeFeet(10) + 0 + 4;
                Assertions.Equal(10, live, "The reach weapon keeps its ten feet.");
                Assertions.False(Threatens(.15d, live, 10, 5), "It still cannot strike an adjacent foe.");
                Assertions.True(Threatens(live * FootMeters - .10d, live, 10, 5), "It still strikes at its farther legal distance.");
            }
        }

        internal static void MixedRoutinesKeepPerAttackReach()
        {
            // Salamander: printed "Reach 5 ft. (10 ft. with tail)". The
            // spear is an ordinary raw-5 weapon and is not converted.
            var tail = SummonReachRepresentation4.Rows.Single(row => row.ItemName == "KMG_Summoning_Special_Salamander_Tail");
            Assertions.Equal(SummonReachRepresentation4.NaturalExtended, tail.Kind, "The tail is a per-attack extension.");
            Assertions.Equal(0, SummonReachRepresentation4.CorrectionFeet(tail.Owner, "99394d453c6f425f84d4b92f7a8deea0"),
                "The shorter spear gets no correction.");
            int spear = SummonReachRepresentation4.NativeTypeRangeFeet(5);
            Assertions.True(tail.LiveRangeAfterFeet > spear, "The tail still reaches farther than the spear.");
            double between = (spear + tail.LiveRangeAfterFeet) * .5d * FootMeters;
            Assertions.True(Threatens(between, tail.LiveRangeAfterFeet, SummonReachRepresentation4.StoredRawFeet, 5) &&
                !Threatens(between, spear, 5, 5), "Between the two ranges only the tail is eligible.");
            Assertions.True(Threatens(.15d, tail.LiveRangeAfterFeet, SummonReachRepresentation4.StoredRawFeet, 5) &&
                Threatens(.15d, spear, 5, 5), "Adjacent, both the spear and the tail are eligible.");
            // Kyton chain: printed "(10 ft. with chains)", over its existing
            // +4 correction (and +15 more while it controls its chains).
            var chain = SummonReachRepresentation4.Rows.Single(row => row.OwnerKey == "kyton-chain-devil");
            Assertions.Equal(10, chain.LiveRangeAfterFeet, "The chain keeps ten feet.");
            Assertions.Equal(4, chain.ExistingAddedFeet, "The existing chain correction is kept, not replaced.");
            // Every converted owner keeps each attack's own live range.
            foreach (var owner in SummonReachRepresentation4.Rows.GroupBy(row => row.OwnerKey))
                foreach (var row in owner)
                    Assertions.Equal(row.LiveRangeBeforeFeet, row.LiveRangeAfterFeet, row.OwnerKey + " " + row.ItemName + " keeps its own range.");
        }

        internal static void SharedConsumersAreCorrectedPerOwner()
        {
            const string slam = "fe074f0010824f9cb2ab6864469e1852", club = "2b821e28b95d40f197f5fb847008b93f";
            var slamOwners = SummonReachRepresentation4.Rows.Where(row => row.Item == slam).Select(row => row.OwnerKey).OrderBy(value => value).ToArray();
            Assertions.True(slamOwners.SequenceEqual(new[] { "fire-giant", "hill-giant", "stone-giant" }),
                "Every census consumer of the shared giant slam is corrected.");
            var clubOwners = SummonReachRepresentation4.Rows.Where(row => row.Item == club).Select(row => row.OwnerKey).OrderBy(value => value).ToArray();
            Assertions.True(clubOwners.SequenceEqual(new[] { "hill-giant", "stone-giant" }),
                "Every census consumer of the shared greatclub is corrected.");
            Assertions.Equal(0, SummonReachRepresentation4.CorrectionFeet("00000000000000000000000000000000", slam),
                "Another owner of the same item gets no correction.");
            Assertions.Equal(0, SummonReachRepresentation4.CorrectionFeet(SummonReachRepresentation4.Rows[0].Owner, slam),
                "The owner's other items get no correction.");
            Assertions.Equal(2, SummonReachRepresentation4.Rows.Count(row => row.OwnerKey == "ettin"),
                "Both Ettin flails, which share one private type, are corrected.");
        }

        internal static void CorrectionIsExactAndStateless()
        {
            Assertions.Equal(SummonReachRepresentation4.Rows.Length,
                SummonReachRepresentation4.Rows.Select(row => row.Owner + "|" + row.Item).Distinct().Count(),
                "One correction per exact owner and item.");
            foreach (var row in SummonReachRepresentation4.Rows)
            {
                int first = SummonReachRepresentation4.CorrectionFeet(row.Owner, row.Item);
                Assertions.Equal(first, SummonReachRepresentation4.CorrectionFeet(row.Owner, row.Item),
                    row.ItemName + ": repeated reads never accumulate.");
                Assertions.Equal(row.CorrectionFeet, first, row.ItemName + ": the lookup returns the row's correction.");
            }
            Assertions.Equal(0, SummonReachRepresentation4.CorrectionFeet(null, null), "Unknown identity gets nothing.");
            foreach (var row in SummonReachRepresentation4.Rows.Where(row => row.ExistingAbsoluteFeet.HasValue))
            {
                Assertions.Equal(0, row.CorrectionFeet, row.ItemName + ": the existing absolute correction is not stacked on.");
                Assertions.Equal(15, row.LiveRangeAfterFeet, row.ItemName + ": the Huge giant keeps fifteen feet.");
            }
        }

        internal static void BarbedDefenseFollowsTheSameReachWeaponBoundary()
        {
            float ordinary = SummonReachRepresentation4.NativeMinWeaponRangeFeet;
            Assertions.True(InfernalRules35.QualifiesForBarbedDefense(true, true, true,
                    SummonReachRepresentation4.NativeTypeRangeFeet(SummonReachRepresentation4.StoredRawFeet), ordinary),
                "An ordinary manufactured weapon wielded with creature reach is not a weapon of exceptional reach.");
            Assertions.False(InfernalRules35.QualifiesForBarbedDefense(true, true, true,
                    SummonReachRepresentation4.NativeTypeRangeFeet(10), ordinary),
                "A genuine reach weapon (glaive, longspear) still does not endanger its user.");
        }
    }
}
