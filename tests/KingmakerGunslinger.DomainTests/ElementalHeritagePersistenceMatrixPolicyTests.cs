using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalHeritagePersistenceMatrixPolicyTests
    {
        internal static void FixtureOrderingAndPresetCoverageAreExact()
        {
            const int raceCount = 4;
            Assertions.Equal(24,
                ElementalHeritagePersistenceMatrixPolicy.FixtureCount(
                    raceCount),
                "Persistence must cover four races, two sexes, and three heritages.");
            Assertions.Equal(8,
                ElementalHeritagePersistenceMatrixPolicy
                    .LegacyGeneralFixtureCount(raceCount),
                "Legacy migration must cover the original four-race/two-sex General fixture prefix.");
            var indices = new List<int>();
            for (int heritage = 0; heritage < 3; heritage++)
                for (int race = 0; race < raceCount; race++)
                    for (int gender = 0; gender < 2; gender++)
                    {
                        int index =
                            ElementalHeritagePersistenceMatrixPolicy
                                .FixtureIndex(race, gender, heritage,
                                    raceCount);
                        indices.Add(index);
                        if (heritage == 0)
                            Assertions.Equal(race * 2 + gender, index,
                                "General fixtures must retain the original eight identity positions.");
                    }
            Assertions.True(indices.SequenceEqual(Enumerable.Range(0, 24)),
                "The persistence fixture order must be contiguous and deterministic.");

            for (int race = 0; race < raceCount; race++)
                for (int gender = 0; gender < 2; gender++)
                {
                    int[] presets = Enumerable.Range(0, 3).Select(
                        heritage =>
                            ElementalHeritagePersistenceMatrixPolicy
                                .PresetIndex(race, gender, heritage,
                                    raceCount, 3)).OrderBy(value => value)
                        .ToArray();
                    Assertions.True(presets.SequenceEqual(
                            new[] { 0, 1, 2 }),
                        "Every race/sex heritage trio must cover all three production presets.");
                }
        }

        internal static void RespecTransitionsAndInvalidInputsAreExact()
        {
            foreach (bool currentCreation in new[] { false, true })
                foreach (int rank in new[] { -1, 0, 1, 2, 20 })
                    Assertions.Equal(rank == (currentCreation ? 1 : 0),
                        ElementalHeritagePersistenceMatrixPolicy
                            .CreationSelectionRankExact(currentCreation, rank),
                        "Every new heritage/trait selection is absent on a markerless legacy load and singular on current creation/respec; duplicates fail.");
            int[] sources = Enumerable.Range(0, 3).Select(
                ElementalHeritagePersistenceMatrixPolicy
                    .SourceHeritageIndex).ToArray();
            int[] restored = Enumerable.Range(0, 3).Select(
                ElementalHeritagePersistenceMatrixPolicy
                    .RestoredHeritageIndex).ToArray();
            Assertions.True(sources.SequenceEqual(new[] { 2, 0, 1 }),
                "Prepare must cover alternate-B-to-General, General-to-alternate-A, and alternate-A-to-alternate-B.");
            Assertions.True(restored.SequenceEqual(new[] { 1, 0, 1 }),
                "Restoration must close General-to-alternate-A-to-General and retain a distinct transition for every fixture.");
            Assertions.Equal(0, sources[1],
                "The first alternate must begin from General.");
            Assertions.Equal(0, restored[1],
                "The persisted first alternate must respec back to General.");
            Assertions.Equal(1, sources[2],
                "The second alternate must begin from the first alternate.");

            Assertions.True(ElementalHeritagePersistenceMatrixPolicy
                    .RespecActorIdentityExact(false, "source", "fixture",
                        true),
                "Prepare-time Respec must use a distinct source identity and a distinct replacement object.");
            Assertions.False(ElementalHeritagePersistenceMatrixPolicy
                    .RespecActorIdentityExact(false, "fixture", "fixture",
                        true),
                "Prepare-time Respec must not reuse its disposable source identity.");
            Assertions.True(ElementalHeritagePersistenceMatrixPolicy
                    .RespecActorIdentityExact(true, "fixture", "fixture",
                        true),
                "Save-backed Respec must preserve the persisted actor identity while replacing the object.");
            Assertions.False(ElementalHeritagePersistenceMatrixPolicy
                    .RespecActorIdentityExact(true, "fixture", "other",
                        true),
                "Save-backed Respec must fail if the persisted identity changes.");
            Assertions.False(ElementalHeritagePersistenceMatrixPolicy
                    .RespecActorIdentityExact(true, "fixture", "fixture",
                        false),
                "Respec must always replace the native object and descriptor.");
            Assertions.False(ElementalHeritagePersistenceMatrixPolicy
                    .RespecActorIdentityExact(true, null, "fixture", true),
                "Missing actor identities must fail closed.");

            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ElementalHeritagePersistenceMatrixPolicy.FixtureCount(0),
                "Zero races must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ElementalHeritagePersistenceMatrixPolicy
                    .LegacyGeneralFixtureCount(0),
                "Legacy fixture counting must fail closed for zero races.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ElementalHeritagePersistenceMatrixPolicy.FixtureIndex(
                    4, 0, 0, 4),
                "Out-of-range races must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ElementalHeritagePersistenceMatrixPolicy.PresetIndex(
                    0, 2, 0, 4, 3),
                "Out-of-range genders must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ElementalHeritagePersistenceMatrixPolicy.PresetIndex(
                    0, 0, 0, 4, 2),
                "A non-three-preset visual matrix must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ElementalHeritagePersistenceMatrixPolicy
                    .SourceHeritageIndex(3),
                "Out-of-range heritages must fail closed.");
        }
    }
}
