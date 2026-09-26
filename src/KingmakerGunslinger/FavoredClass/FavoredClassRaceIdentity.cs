using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    internal enum FavoredClassRaceProvider
    {
        /// <summary>Pathfinder: Kingmaker's own playable race blueprint.</summary>
        Native,
        /// <summary>
        /// An optional race identity the host resolves with TryGet; supplied
        /// in the qualified profile by an optional race mod.
        /// </summary>
        Optional,
        /// <summary>A Kingmaker Gunslinger elemental parent race.</summary>
        Kmg
    }

    internal sealed class FavoredClassRaceIdentity
    {
        internal FavoredClassRaceIdentity(string ancestry, string raceGuid,
            FavoredClassRaceProvider provider)
        {
            Ancestry = ancestry;
            RaceGuid = raceGuid;
            Provider = provider;
        }

        internal string Ancestry { get; private set; }
        internal string RaceGuid { get; private set; }
        internal FavoredClassRaceProvider Provider { get; private set; }
    }

    /// <summary>
    /// Exact race-blueprint identities for the 21 source-addressable races.
    /// Ancestry is resolved only from a unit's actual race blueprint GUID;
    /// names, portraits, visual presets, creature type facts and elemental
    /// affinity are never ancestry evidence. Native and optional identities
    /// are the ones the installed Favored Class host itself uses.
    /// </summary>
    internal static class FavoredClassRaceIdentities
    {
        private static readonly FavoredClassRaceIdentity[] Identities =
        {
            N(FavoredClassAncestry.Human, "0a5d473ead98b0646b94495af250fdc4"),
            N(FavoredClassAncestry.Elf, "25a5878d125338244896ebd3238226c8"),
            N(FavoredClassAncestry.HalfElf, "b3646842ffbd01643ab4dac7479b20b0"),
            N(FavoredClassAncestry.Gnome, "ef35a22c9a27da345a4528f0d5889157"),
            N(FavoredClassAncestry.Dwarf, "c4faf439f0e70bd40b5e36ee80d06be7"),
            N(FavoredClassAncestry.HalfOrc, "1dc20e195581a804890ddc74218bfd8e"),
            N(FavoredClassAncestry.Halfling, "b0c3ef2729c498f47970bb50fa1acd30"),
            N(FavoredClassAncestry.Tiefling, "5c4e42124dc2b4647af6e36cf2590500"),
            N(FavoredClassAncestry.Aasimar, "b7f02ba92b363064fb873963bec275ee"),
            O(FavoredClassAncestry.Goblin, "9d168ca7100e9314385ce66852385451"),
            O(FavoredClassAncestry.Duergar, "cd40ff5a556bcf3419bf7479616cd2ad"),
            O(FavoredClassAncestry.Dhampir, "d1335380a70e4bd7aa535f36770b93de"),
            O(FavoredClassAncestry.Drow, "c515d06d35d048e79801d07039338cda"),
            O(FavoredClassAncestry.Ganzi, "970bb406a3ac42d795a3ef1b5900fdf3"),
            O(FavoredClassAncestry.Suli, "f78db38a553f4f91a10a8e68c91019ad"),
            O(FavoredClassAncestry.Fetchling, "3cfdcda8edd74212a58d3b0d9d4041a4"),
            O(FavoredClassAncestry.Hobgoblin, "a68578b3a2a945a5b8561ec51a0dff5c"),
            K(FavoredClassAncestry.Ifrit, "556a2d9ae0c6401eaed87614a2caf539"),
            K(FavoredClassAncestry.Oread, "7ef60bcda0204429bf4859e2faa3cbf8"),
            K(FavoredClassAncestry.Sylph, "68b64570c6e943f1bcbe4571e88bf285"),
            K(FavoredClassAncestry.Undine, "557dea40c2cc440f8afe7d678d2d283a"),
        };

        internal static IList<FavoredClassRaceIdentity> All
        {
            get { return Array.AsReadOnly(Identities); }
        }

        /// <summary>The ancestry of an exact race GUID, or null when unrecognized.</summary>
        internal static string AncestryForRaceGuid(string raceGuid)
        {
            if (string.IsNullOrEmpty(raceGuid))
                return null;
            FavoredClassRaceIdentity identity = Identities.FirstOrDefault(value =>
                string.Equals(value.RaceGuid, raceGuid, StringComparison.OrdinalIgnoreCase));
            return identity == null ? null : identity.Ancestry;
        }

        internal static FavoredClassRaceIdentity ForAncestry(string ancestry)
        {
            return Identities.FirstOrDefault(value =>
                string.Equals(value.Ancestry, ancestry, StringComparison.Ordinal));
        }

        private static FavoredClassRaceIdentity N(string ancestry, string guid)
        {
            return new FavoredClassRaceIdentity(ancestry, guid, FavoredClassRaceProvider.Native);
        }

        private static FavoredClassRaceIdentity O(string ancestry, string guid)
        {
            return new FavoredClassRaceIdentity(ancestry, guid, FavoredClassRaceProvider.Optional);
        }

        private static FavoredClassRaceIdentity K(string ancestry, string guid)
        {
            return new FavoredClassRaceIdentity(ancestry, guid, FavoredClassRaceProvider.Kmg);
        }
    }
}
