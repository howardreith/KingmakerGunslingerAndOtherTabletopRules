using System;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>
    /// Pure ordering policy for the Release A save/respec qualification
    /// matrix. General fixtures retain the original 0.0.114 disposable IDs;
    /// alternates follow in catalog order.
    /// </summary>
    internal static class ElementalHeritagePersistenceMatrixPolicy
    {
        internal const int GenderCount = 2;

        internal static int FixtureCount(int raceCount)
        {
            if (raceCount < 1)
                throw new ArgumentOutOfRangeException("raceCount");
            return checked(raceCount * GenderCount *
                ElementalHeritagePolicy.ChoicesPerRace);
        }

        internal static int FixtureIndex(int raceIndex, int genderIndex,
            int heritageIndex, int raceCount)
        {
            Validate(raceIndex, genderIndex, heritageIndex, raceCount);
            return checked(heritageIndex * raceCount * GenderCount +
                raceIndex * GenderCount + genderIndex);
        }

        internal static int PresetIndex(int raceIndex, int genderIndex,
            int heritageIndex, int raceCount, int presetCount)
        {
            Validate(raceIndex, genderIndex, heritageIndex, raceCount);
            if (presetCount != ElementalHeritagePolicy.ChoicesPerRace)
                throw new ArgumentOutOfRangeException("presetCount");
            return (raceIndex * GenderCount + genderIndex +
                heritageIndex) % presetCount;
        }

        internal static int SourceHeritageIndex(int heritageIndex)
        {
            ValidateHeritage(heritageIndex);
            return heritageIndex == 0
                ? ElementalHeritagePolicy.ChoicesPerRace - 1
                : heritageIndex - 1;
        }

        internal static int RestoredHeritageIndex(int heritageIndex)
        {
            ValidateHeritage(heritageIndex);
            // General -> first alternate. First alternate -> General closes
            // the General/alternate/General chain. Second alternate -> first
            // alternate after its prepare-time first-to-second transition.
            return heritageIndex == 1 ? 0 : 1;
        }

        internal static bool RespecActorIdentityExact(
            bool persistedSource, string sourceActorId,
            string replacementActorId, bool distinctObjects)
        {
            if (!distinctObjects || string.IsNullOrEmpty(sourceActorId) ||
                string.IsNullOrEmpty(replacementActorId)) return false;

            bool sameStableIdentity = string.Equals(sourceActorId,
                replacementActorId, StringComparison.Ordinal);
            return persistedSource
                ? sameStableIdentity
                : !sameStableIdentity;
        }

        private static void Validate(int raceIndex, int genderIndex,
            int heritageIndex, int raceCount)
        {
            if (raceCount < 1 || raceIndex < 0 ||
                raceIndex >= raceCount)
                throw new ArgumentOutOfRangeException("raceIndex");
            if (genderIndex < 0 || genderIndex >= GenderCount)
                throw new ArgumentOutOfRangeException("genderIndex");
            ValidateHeritage(heritageIndex);
        }

        private static void ValidateHeritage(int heritageIndex)
        {
            if (heritageIndex < 0 || heritageIndex >=
                    ElementalHeritagePolicy.ChoicesPerRace)
                throw new ArgumentOutOfRangeException("heritageIndex");
        }
    }
}
