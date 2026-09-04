using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalHeritagePolicyTests
    {
        internal static void RunAll()
        {
            CatalogHasExactChoiceAndProviderInventory();
            AbilityModifiersAndOverlayDeltasAreExact();
            LegacyAbsenceResolvesToGeneralAndInvalidStatesFailClosed();
            NativeDonorsAndProjectOwnedImplementationsAreExact();
        }

        internal static void CatalogHasExactChoiceAndProviderInventory()
        {
            ElementalHeritagePersistenceMatrixPolicyTests
                .FixtureOrderingAndPresetCoverageAreExact();
            ElementalHeritagePersistenceMatrixPolicyTests
                .RespecTransitionsAndInvalidInputsAreExact();
            IReadOnlyList<ElementalHeritageDefinition> all =
                ElementalHeritagePolicy.Ordered();
            Assertions.Equal(12, all.Count,
                "Release A must define exactly twelve heritages.");
            Assertions.Equal(12, all.Select(entry => entry.MarkerSymbol)
                .Distinct(StringComparer.Ordinal).Count(),
                "Every heritage choice needs a distinct save-bearing marker.");

            foreach (ElementalHeritageRace race in Enum.GetValues(
                typeof(ElementalHeritageRace)))
            {
                IReadOnlyList<ElementalHeritageDefinition> choices =
                    ElementalHeritagePolicy.ForRace(race);
                Assertions.Equal(3, choices.Count,
                    race + " must expose exactly three choices.");
                Assertions.Equal(1, choices.Count(entry => entry.IsGeneral),
                    race + " must expose exactly one General choice.");
                Assertions.Equal(1, choices.Select(entry =>
                    entry.SelectionSymbol).Distinct(StringComparer.Ordinal)
                    .Count(), race + " choices must share one selection.");
                foreach (ElementalHeritageDefinition choice in choices)
                    Assertions.True(!string.IsNullOrWhiteSpace(choice.Name) &&
                        !string.IsNullOrWhiteSpace(choice.Description) &&
                        !string.IsNullOrWhiteSpace(choice.AffinityName) &&
                        !string.IsNullOrWhiteSpace(choice.AffinityDescription) &&
                        !string.IsNullOrWhiteSpace(choice.SlaName) &&
                        !string.IsNullOrWhiteSpace(choice.SlaDescription),
                        choice.Id + " must have complete player-facing text.");
            }
        }

        internal static void AbilityModifiersAndOverlayDeltasAreExact()
        {
            Stats(ElementalHeritageId.GeneralIfrit, 0, 2, 0, 0, -2, 2);
            Stats(ElementalHeritageId.Lavasoul, 0, -2, 2, 2, 0, 0);
            Stats(ElementalHeritageId.Sunsoul, 2, 0, 0, 0, -2, 2);
            Stats(ElementalHeritageId.GeneralOread, 2, 0, 0, 0, 2, -2);
            Stats(ElementalHeritageId.Gemsoul, 2, 0, 0, 0, -2, 2);
            Stats(ElementalHeritageId.Ironsoul, 0, -2, 2, 0, 2, 0);
            Stats(ElementalHeritageId.GeneralSylph, 0, 2, -2, 2, 0, 0);
            Stats(ElementalHeritageId.Smokesoul, 0, 2, -2, 0, 0, 2);
            Stats(ElementalHeritageId.Stormsoul, 0, 2, 0, 0, -2, 2);
            Stats(ElementalHeritageId.GeneralUndine, -2, 2, 0, 0, 2, 0);
            Stats(ElementalHeritageId.Mistsoul, 0, 0, 2, -2, 2, 0);
            Stats(ElementalHeritageId.Rimesoul, 0, 2, 0, 2, 0, -2);

            foreach (ElementalHeritageDefinition entry in
                ElementalHeritagePolicy.Ordered())
            {
                ElementalHeritageDefinition general =
                    ElementalHeritagePolicy.General(entry.ParentRace);
                IDictionary<ElementalHeritageStat, int> deltas =
                    ElementalHeritagePolicy.NetDeltas(entry).ToDictionary(
                        item => item.Stat, item => item.Value);
                foreach (ElementalHeritageStat stat in Enum.GetValues(
                    typeof(ElementalHeritageStat)))
                {
                    int delta;
                    deltas.TryGetValue(stat, out delta);
                    Assertions.Equal(entry.ModifierFor(stat),
                        general.ModifierFor(stat) + delta,
                        entry.Id + " overlay must produce exact final " + stat +
                        " without duplicating General stats.");
                }
                if (entry.IsGeneral)
                    Assertions.Equal(0, deltas.Count,
                        entry.Id + " must be a no-op legacy overlay.");
            }
        }

        internal static void LegacyAbsenceResolvesToGeneralAndInvalidStatesFailClosed()
        {
            foreach (ElementalHeritageRace race in Enum.GetValues(
                typeof(ElementalHeritageRace)))
            {
                ElementalHeritageId expected =
                    ElementalHeritagePolicy.General(race).Id;
                Assertions.Equal(expected,
                    ElementalHeritagePolicy.Resolve(race, null).Id,
                    race + " missing marker must resolve to General.");
                Assertions.Equal(expected,
                    ElementalHeritagePolicy.Resolve(race,
                        new ElementalHeritageId[0]).Id,
                    race + " empty marker set must resolve to General.");
                Assertions.Equal(expected,
                    ElementalHeritagePolicy.Resolve(race,
                        new[] { expected }).Id,
                    race + " explicit General must resolve to General.");
            }
            Assertions.Throws<InvalidOperationException>(() =>
                ElementalHeritagePolicy.Resolve(ElementalHeritageRace.Ifrit,
                    new[] { ElementalHeritageId.Lavasoul,
                        ElementalHeritageId.Sunsoul }),
                "Multiple heritage markers must fail closed.");
            Assertions.Throws<InvalidOperationException>(() =>
                ElementalHeritagePolicy.Resolve(ElementalHeritageRace.Ifrit,
                    new[] { ElementalHeritageId.Gemsoul }),
                "A foreign-race heritage marker must fail closed.");
        }

        internal static void NativeDonorsAndProjectOwnedImplementationsAreExact()
        {
            var donors = new Dictionary<ElementalHeritageId, string>
            {
                { ElementalHeritageId.GeneralIfrit, "4783c3709a74a794dbe7c8e7e0b1b038" },
                { ElementalHeritageId.Lavasoul, "b065231094a21d14dbf1c3832f776871" },
                { ElementalHeritageId.Sunsoul, "39a602aa80cc96f4597778b6d4d49c0a" },
                { ElementalHeritageId.GeneralOread, "85067a04a97416949b5d1dbf986d93f3" },
                { ElementalHeritageId.Gemsoul, "91da41b9793a4624797921f221db653c" },
                { ElementalHeritageId.GeneralSylph, "f3c0b267dd17a2a45a40805e31fe3cd1" },
                { ElementalHeritageId.Smokesoul, "4f8181e7a7f1d904fbaea64220e83379" },
                { ElementalHeritageId.Stormsoul, "ab395d2335d3f384e99dddee8562978f" },
                { ElementalHeritageId.Mistsoul, "14ec7a4e52e90fa47a4c8d63c69fd5c1" }
            };
            foreach (ElementalHeritageDefinition entry in
                ElementalHeritagePolicy.Ordered())
            {
                string donor;
                if (donors.TryGetValue(entry.Id, out donor))
                {
                    Assertions.Equal(
                        ElementalHeritageAbilityImplementation.NativeSpellClone,
                        entry.AbilityImplementation,
                        entry.Id + " must use an audited native donor.");
                    Assertions.Equal(donor, entry.DonorAbilityGuid,
                        entry.Id + " donor GUID drifted.");
                }
                else
                    Assertions.True(string.IsNullOrWhiteSpace(
                        entry.DonorAbilityGuid), entry.Id +
                        " project-owned SLA must not claim a donor.");
            }
            Implementation(ElementalHeritageId.GeneralUndine,
                ElementalHeritageAbilityImplementation.HydraulicPush);
            Implementation(ElementalHeritageId.Ironsoul,
                ElementalHeritageAbilityImplementation.UnerringWeapon);
            Implementation(ElementalHeritageId.Rimesoul,
                ElementalHeritageAbilityImplementation.ChillTouch);
            Assertions.Equal(2, Find(ElementalHeritageId.Mistsoul).SpellLevel,
                "Blur must retain its actual second-level SLA parameters.");
        }

        private static void Stats(ElementalHeritageId id, int strength,
            int dexterity, int constitution, int intelligence, int wisdom,
            int charisma)
        {
            ElementalHeritageDefinition entry = Find(id);
            int[] expected = { strength, dexterity, constitution,
                intelligence, wisdom, charisma };
            int index = 0;
            foreach (ElementalHeritageStat stat in Enum.GetValues(
                typeof(ElementalHeritageStat)))
                Assertions.Equal(expected[index++], entry.ModifierFor(stat),
                    id + " " + stat + " modifier drifted.");
        }

        private static void Implementation(ElementalHeritageId id,
            ElementalHeritageAbilityImplementation expected)
        {
            Assertions.Equal(expected, Find(id).AbilityImplementation,
                id + " implementation kind drifted.");
        }

        private static ElementalHeritageDefinition Find(ElementalHeritageId id)
        {
            return ElementalHeritagePolicy.Ordered().Single(entry =>
                entry.Id == id);
        }
    }
}
