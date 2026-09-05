using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalAlternateTraitPolicyTests
    {
        internal static void CatalogAndReplacementSlotsAreExact()
        {
            ElementalAlternateTraitDefinition[] all =
                ElementalAlternateTraitPolicy.Ordered().ToArray();
            Assertions.Equal(21, all.Length,
                "Release C must expose exactly the required 21 alternate racial traits.");
            Assertions.Equal(21, all.Select(value => value.Id).Distinct()
                .Count(), "Every alternate racial trait needs one semantic identity.");
            Assertions.Equal(21, all.Select(value => value.MarkerSymbol)
                .Distinct(StringComparer.Ordinal).Count(),
                "Every alternate racial trait needs one stable marker symbol.");
            Assertions.Equal(21, all.Select(value => value.ProviderSymbol)
                .Distinct(StringComparer.Ordinal).Count(),
                "Every alternate racial trait needs one distinct owned provider symbol.");
            Assertions.True(all.All(value => value.MarkerSymbol !=
                    value.ProviderSymbol),
                "Visible trait markers and hidden mechanic providers must have distinct identities.");

            Count(ElementalHeritageRace.Ifrit, 6);
            Count(ElementalHeritageRace.Oread, 5);
            Count(ElementalHeritageRace.Sylph, 7);
            Count(ElementalHeritageRace.Undine, 3);

            Slots(ElementalAlternateTraitId.WildfireHeart,
                ElementalRacialTraitSlot.EnergyResistance);
            Slots(ElementalAlternateTraitId.BrazenFlame,
                ElementalRacialTraitSlot.EnergyResistance |
                ElementalRacialTraitSlot.RacialSpellLikeAbility);
            Slots(ElementalAlternateTraitId.FireInTheBlood,
                ElementalRacialTraitSlot.ElementalAffinity);
            Slots(ElementalAlternateTraitId.EfreetiMagic,
                ElementalRacialTraitSlot.RacialSpellLikeAbility);
            Slots(ElementalAlternateTraitId.ForgeHardened,
                ElementalRacialTraitSlot.RacialSpellLikeAbility);
            Slots(ElementalAlternateTraitId.FireInsight,
                ElementalRacialTraitSlot.ElementalAffinity);

            Slots(ElementalAlternateTraitId.CrystallineForm,
                ElementalRacialTraitSlot.ElementalAffinity);
            Slots(ElementalAlternateTraitId.EarthInsight,
                ElementalRacialTraitSlot.ElementalAffinity);
            Slots(ElementalAlternateTraitId.GraniteSkin,
                ElementalRacialTraitSlot.EnergyResistance);
            Slots(ElementalAlternateTraitId.StoneInTheBlood,
                ElementalRacialTraitSlot.ElementalAffinity);
            Slots(ElementalAlternateTraitId.TreacherousEarth,
                ElementalRacialTraitSlot.RacialSpellLikeAbility);

            Slots(ElementalAlternateTraitId.AirInsight,
                ElementalRacialTraitSlot.ElementalAffinity);
            Slots(ElementalAlternateTraitId.BreezeKissed,
                ElementalRacialTraitSlot.ElementalAffinity);
            Slots(ElementalAlternateTraitId.LikeTheWind,
                ElementalRacialTraitSlot.EnergyResistance);
            Slots(ElementalAlternateTraitId.Secretive,
                ElementalRacialTraitSlot.EnergyResistance |
                ElementalRacialTraitSlot.RacialSpellLikeAbility);
            Slots(ElementalAlternateTraitId.StormInTheBlood,
                ElementalRacialTraitSlot.ElementalAffinity);
            Slots(ElementalAlternateTraitId.ThunderousResilience,
                ElementalRacialTraitSlot.EnergyResistance);
            Slots(ElementalAlternateTraitId.WhisperingWind,
                ElementalRacialTraitSlot.RacialSpellLikeAbility);

            Slots(ElementalAlternateTraitId.AcidBreath,
                ElementalRacialTraitSlot.RacialSpellLikeAbility);
            Slots(ElementalAlternateTraitId.NereidFascination,
                ElementalRacialTraitSlot.RacialSpellLikeAbility);
            Slots(ElementalAlternateTraitId.OozeBreath,
                ElementalRacialTraitSlot.RacialSpellLikeAbility);

            Assertions.True(all.All(value =>
                    !string.IsNullOrWhiteSpace(value.Name) &&
                    !string.IsNullOrWhiteSpace(value.Description) &&
                    value.MarkerSymbol.StartsWith("KMG.ElementalRaces.Traits.",
                        StringComparison.Ordinal)),
                "Every Release C trait needs complete text and a project-owned marker.");

            ElementalAlternateTraitSelectionDefinition[] selections =
                ElementalAlternateTraitPolicy.OrderedSelections().ToArray();
            Assertions.Equal(10, selections.Length,
                "Only slots with at least one required alternate need a visible selection.");
            Assertions.Equal(10, selections.Select(value =>
                value.SelectionSymbol).Distinct(StringComparer.Ordinal).Count(),
                "Every visible slot selection needs one stable identity.");
            Assertions.Equal(10, selections.Select(value =>
                value.RetainMarkerSymbol).Distinct(StringComparer.Ordinal)
                .Count(),
                "Every visible slot selection needs an explicit retain-base choice.");
            Assertions.Equal(21, selections.SelectMany(value => value.Choices)
                .Select(value => value.Id).Distinct().Count(),
                "Every alternate trait must appear in exactly one primary-slot selection.");
            Assertions.Equal(3, ElementalAlternateTraitPolicy
                .SelectionsForRace(ElementalHeritageRace.Ifrit).Count,
                "Ifrit needs energy, affinity, and SLA selections.");
            Assertions.Equal(3, ElementalAlternateTraitPolicy
                .SelectionsForRace(ElementalHeritageRace.Oread).Count,
                "Oread needs energy, affinity, and SLA selections.");
            Assertions.Equal(3, ElementalAlternateTraitPolicy
                .SelectionsForRace(ElementalHeritageRace.Sylph).Count,
                "Sylph needs energy, affinity, and SLA selections.");
            Assertions.Equal(1, ElementalAlternateTraitPolicy
                .SelectionsForRace(ElementalHeritageRace.Undine).Count,
                "Undine needs only the SLA selection represented by required content.");
            Assertions.Equal(62,
                ElementalAlternateTraitPolicy.FrameworkIdentityCount,
                "Release C framework identity count must include selections, retain markers, trait markers, and owned providers.");
        }

        internal static void ExhaustiveProviderAndLegalityMatrixIsExact()
        {
            var expectedLegalCounts = new Dictionary<
                ElementalHeritageRace, int>
            {
                { ElementalHeritageRace.Ifrit, 21 },
                { ElementalHeritageRace.Oread, 16 },
                { ElementalHeritageRace.Sylph, 28 },
                { ElementalHeritageRace.Undine, 4 }
            };

            foreach (ElementalHeritageRace race in Enum.GetValues(
                typeof(ElementalHeritageRace)))
            {
                ElementalAlternateTraitDefinition[] traits =
                    ElementalAlternateTraitPolicy.ForRace(race).ToArray();
                ElementalAlternateTraitId[][] sets = PowerSet(traits);
                ElementalAlternateTraitId[][] legal = sets.Where(value =>
                    ExpectedLegal(value)).ToArray();
                Assertions.Equal(expectedLegalCounts[race], legal.Length,
                    race + " legal replacement combinations drifted.");

                foreach (ElementalHeritageDefinition heritage in
                    ElementalHeritagePolicy.ForRace(race))
                {
                    foreach (ElementalAlternateTraitId[] selected in sets)
                    {
                        bool expected = ExpectedLegal(selected);
                        Assertions.Equal(expected,
                            ElementalAlternateTraitPolicy.IsLegal(race,
                                selected),
                            race + "/" + heritage.Id +
                            " legality must be derived only from exact slot overlap.");
                        if (!expected)
                        {
                            Assertions.Throws<InvalidOperationException>(() =>
                                ElementalAlternateTraitPolicy.Resolve(race,
                                    heritage.Id, selected),
                                "An overlapping slot combination must fail closed.");
                            continue;
                        }

                        ElementalAlternateTraitState state =
                            ElementalAlternateTraitPolicy.Resolve(race,
                                heritage.Id, selected);
                        AssertState(state, heritage, selected);
                    }
                }

                Assertions.False(ElementalAlternateTraitPolicy.IsLegal(race,
                    new[] { traits[0].Id, traits[0].Id }),
                    "Selecting one trait twice must fail closed.");
                Assertions.Throws<InvalidOperationException>(() =>
                    ElementalAlternateTraitPolicy.Resolve(race,
                        ElementalHeritagePolicy.General(race).Id,
                        new[] { traits[0].Id, traits[0].Id }),
                    "Duplicate trait facts must not silently collapse.");
            }

            Assertions.False(ElementalAlternateTraitPolicy.IsLegal(
                ElementalHeritageRace.Ifrit,
                new[] { ElementalAlternateTraitId.GraniteSkin }),
                "A trait from another exact parent race must fail closed.");
            Assertions.Throws<InvalidOperationException>(() =>
                ElementalAlternateTraitPolicy.Resolve(
                    ElementalHeritageRace.Ifrit,
                    ElementalHeritageId.Gemsoul,
                    new ElementalAlternateTraitId[0]),
                "A heritage from another parent race must fail closed.");
        }

        internal static void OrderingRemovalAndReconstructionAreDeterministic()
        {
            foreach (ElementalHeritageRace race in Enum.GetValues(
                typeof(ElementalHeritageRace)))
            {
                ElementalAlternateTraitDefinition[] definitions =
                    ElementalAlternateTraitPolicy.ForRace(race).ToArray();
                ElementalAlternateTraitId[][] legal = PowerSet(definitions)
                    .Where(ExpectedLegal).ToArray();
                foreach (ElementalHeritageDefinition heritage in
                    ElementalHeritagePolicy.ForRace(race))
                {
                    foreach (ElementalAlternateTraitId[] selected in legal)
                    {
                        ElementalAlternateTraitState expected =
                            ElementalAlternateTraitPolicy.Resolve(race,
                                heritage.Id, selected);
                        foreach (ElementalAlternateTraitId[] order in
                            Permutations(selected))
                            Assertions.Equal(expected.Fingerprint,
                                ElementalAlternateTraitPolicy.Resolve(race,
                                    heritage.Id, order).Fingerprint,
                                "Fact activation order must not change desired providers.");

                        string[] serializedMarkers = expected.MarkerSymbols()
                            .Reverse().ToArray();
                        Assertions.Equal(expected.Fingerprint,
                            ElementalAlternateTraitPolicy.ResolveMarkers(race,
                                heritage.Id, serializedMarkers).Fingerprint,
                            "Exact save-bearing marker reconstruction must be order independent.");

                        foreach (ElementalAlternateTraitId removed in selected)
                        {
                            ElementalAlternateTraitId[] intermediate = selected
                                .Where(value => value != removed).ToArray();
                            ElementalAlternateTraitPolicy.Resolve(race,
                                heritage.Id, intermediate);
                            ElementalAlternateTraitId[] restored = intermediate
                                .Concat(new[] { removed }).Reverse().ToArray();
                            Assertions.Equal(expected.Fingerprint,
                                ElementalAlternateTraitPolicy.Resolve(race,
                                    heritage.Id, restored).Fingerprint,
                                "Removing and re-adding a trait must restore the same desired state.");
                        }
                    }
                }

                foreach (ElementalAlternateTraitDefinition trait in definitions)
                {
                    ElementalAlternateTraitState first =
                        ElementalAlternateTraitPolicy.Resolve(race,
                            ElementalHeritagePolicy.General(race).Id,
                            new[] { trait.Id });
                    foreach (ElementalHeritageDefinition changedHeritage in
                        ElementalHeritagePolicy.ForRace(race))
                    {
                        ElementalAlternateTraitState changed =
                            ElementalAlternateTraitPolicy.Resolve(race,
                                changedHeritage.Id, new[] { trait.Id });
                        Assertions.True(changed.Traits.Single().Id == trait.Id,
                            "Changing heritage must retain the exact active trait marker.");
                        Assertions.Equal(first.ConsumedSlots,
                            changed.ConsumedSlots,
                            "Changing heritage must not change consumed replacement slots.");
                        AssertState(changed, changedHeritage,
                            new[] { trait.Id });
                    }
                }

                foreach (ElementalAlternateTraitSelectionDefinition selection
                    in ElementalAlternateTraitPolicy.SelectionsForRace(race))
                    for (int oldIndex = 0; oldIndex < selection.Choices.Count;
                        oldIndex++)
                        for (int newIndex = 0;
                            newIndex < selection.Choices.Count; newIndex++)
                        {
                            if (oldIndex == newIndex) continue;
                            ElementalAlternateTraitId oldId =
                                selection.Choices[oldIndex].Id;
                            ElementalAlternateTraitId newId =
                                selection.Choices[newIndex].Id;
                            Assertions.True(ElementalAlternateTraitPolicy
                                    .TransitionMarkers(race,
                                        new[] { oldId }, newId, null)
                                    .SequenceEqual(new[] { newId }),
                                "Activating a replacement before the old same-selection marker turns off must prefer the new marker.");
                            Assertions.True(ElementalAlternateTraitPolicy
                                    .TransitionMarkers(race,
                                        new[] { oldId, newId }, null, oldId)
                                    .SequenceEqual(new[] { newId }),
                                "Turning off an old marker after its replacement activates must retain the new marker.");
                        }
            }

            Assertions.Throws<InvalidOperationException>(() =>
                ElementalAlternateTraitPolicy.ResolveMarkers(
                    ElementalHeritageRace.Ifrit,
                    ElementalHeritageId.GeneralIfrit,
                    new[] { "KMG.ElementalRaces.Traits.NotOwned" }),
                "Unknown marker identities must fail closed during reconstruction.");
            string oreadMarker = ElementalAlternateTraitPolicy.Find(
                ElementalAlternateTraitId.GraniteSkin).MarkerSymbol;
            Assertions.Throws<InvalidOperationException>(() =>
                ElementalAlternateTraitPolicy.ResolveMarkers(
                    ElementalHeritageRace.Ifrit,
                    ElementalHeritageId.GeneralIfrit,
                    new[] { oreadMarker }),
                "A foreign-race project marker must fail closed during reconstruction.");
            Assertions.Throws<InvalidOperationException>(() =>
                ElementalAlternateTraitPolicy.TransitionMarkers(
                    ElementalHeritageRace.Ifrit,
                    new[] { ElementalAlternateTraitId.EfreetiMagic },
                    ElementalAlternateTraitId.BrazenFlame, null),
                "A cross-selection multi-slot conflict must remain fail-closed during activation.");
        }

        private static void AssertState(ElementalAlternateTraitState state,
            ElementalHeritageDefinition heritage,
            IEnumerable<ElementalAlternateTraitId> selected)
        {
            ElementalAlternateTraitDefinition[] definitions = selected.Select(
                ElementalAlternateTraitPolicy.Find).ToArray();
            ElementalRacialTraitSlot consumed = definitions.Aggregate(
                ElementalRacialTraitSlot.None,
                (current, value) => current | value.ReplacedSlots);
            Assertions.Equal(heritage.ParentRace, state.Race,
                "Resolved state must retain the exact parent race.");
            Assertions.Equal(heritage.Id, state.Heritage.Id,
                "Resolved state must retain the exact heritage.");
            Assertions.Equal(consumed, state.ConsumedSlots,
                "Resolved state must expose the exact consumed slot mask.");
            Assertions.Equal(definitions.Length, state.Traits.Count,
                "Resolved state must retain every active trait exactly once.");
            Assertions.True(state.Traits.Select(value => value.Id)
                .SequenceEqual(definitions.Select(value => value.Id)
                    .OrderBy(value => (int)value)),
                "Resolved traits must have deterministic catalog order.");
            string[] traitProviders = state.TraitProviderSymbols();
            Assertions.True(traitProviders.SequenceEqual(state.Traits.Select(
                    value => value.ProviderSymbol)),
                "Each active marker must resolve to its exact owned provider in catalog order.");
            Assertions.Equal(traitProviders.Length, traitProviders.Distinct(
                StringComparer.Ordinal).Count(),
                "An alternate-trait provider must never be desired twice.");

            bool resistanceActive = (consumed &
                ElementalRacialTraitSlot.EnergyResistance) == 0;
            AssertProvider(state.EnergyResistanceProviderSymbol,
                resistanceActive,
                "energy resistance");
            if (resistanceActive)
                Assertions.Equal(ExpectedResistance(heritage.ParentRace),
                    state.EnergyResistanceProviderSymbol,
                    "An unconsumed resistance slot must use the exact parent-race provider.");
            bool affinityActive = (consumed &
                ElementalRacialTraitSlot.ElementalAffinity) == 0;
            AssertProvider(state.ElementalAffinityProviderSymbol,
                affinityActive,
                "elemental affinity");
            if (affinityActive)
                Assertions.Equal(heritage.AffinityFeatureSymbol,
                    state.ElementalAffinityProviderSymbol,
                    "An unconsumed affinity slot must use the active heritage provider.");
            bool slaActive = (consumed &
                ElementalRacialTraitSlot.RacialSpellLikeAbility) == 0;
            AssertProvider(state.RacialSlaFeatureSymbol, slaActive,
                "racial SLA feature");
            AssertProvider(state.RacialSlaResourceSymbol, slaActive,
                "racial SLA resource");
            AssertProvider(state.RacialSlaAbilitySymbol, slaActive,
                "racial SLA ability");

            if (slaActive)
            {
                Assertions.Equal(heritage.SlaFeatureSymbol,
                    state.RacialSlaFeatureSymbol,
                    "An unconsumed SLA slot must use the active heritage feature.");
                Assertions.Equal(heritage.SlaResourceSymbol,
                    state.RacialSlaResourceSymbol,
                    "An unconsumed SLA slot must use the active heritage resource.");
                Assertions.Equal(heritage.SlaAbilitySymbol,
                    state.RacialSlaAbilitySymbol,
                    "An unconsumed SLA slot must use the active heritage ability.");
            }

            foreach (ElementalHeritageStat stat in Enum.GetValues(
                typeof(ElementalHeritageStat)))
                Assertions.Equal(heritage.ModifierFor(stat),
                    state.ModifierFor(stat),
                    "Alternate traits must not drift heritage ability modifiers.");

            bool expectedHydraulic = slaActive &&
                heritage.Id == ElementalHeritageId.GeneralUndine;
            Assertions.Equal(expectedHydraulic, state.HasActiveHydraulicPush,
                "SLA-dependent feats must follow the exact active Hydraulic Push provider.");
            string[] providers = new[]
            {
                state.EnergyResistanceProviderSymbol,
                state.ElementalAffinityProviderSymbol,
                state.RacialSlaFeatureSymbol
            }.Where(value => value != null).ToArray();
            Assertions.Equal(providers.Length, providers.Distinct(
                StringComparer.Ordinal).Count(),
                "No project-owned slot provider may be active twice.");
        }

        private static void AssertProvider(string symbol, bool expected,
            string label)
        {
            Assertions.Equal(expected, !string.IsNullOrWhiteSpace(symbol),
                "The " + label + " provider must be present exactly when its slot is not consumed.");
        }

        private static bool ExpectedLegal(
            IEnumerable<ElementalAlternateTraitId> ids)
        {
            ElementalRacialTraitSlot consumed = ElementalRacialTraitSlot.None;
            var seen = new HashSet<ElementalAlternateTraitId>();
            foreach (ElementalAlternateTraitId id in ids)
            {
                if (!seen.Add(id)) return false;
                ElementalRacialTraitSlot slots =
                    ElementalAlternateTraitPolicy.Find(id).ReplacedSlots;
                if ((consumed & slots) != 0) return false;
                consumed |= slots;
            }
            return true;
        }

        private static ElementalAlternateTraitId[][] PowerSet(
            ElementalAlternateTraitDefinition[] definitions)
        {
            var result = new List<ElementalAlternateTraitId[]>();
            int count = 1 << definitions.Length;
            for (int mask = 0; mask < count; mask++)
            {
                var ids = new List<ElementalAlternateTraitId>();
                for (int index = 0; index < definitions.Length; index++)
                    if ((mask & (1 << index)) != 0)
                        ids.Add(definitions[index].Id);
                result.Add(ids.ToArray());
            }
            return result.ToArray();
        }

        private static IEnumerable<ElementalAlternateTraitId[]> Permutations(
            ElementalAlternateTraitId[] values)
        {
            if (values.Length < 2)
            {
                yield return (ElementalAlternateTraitId[])values.Clone();
                yield break;
            }
            for (int index = 0; index < values.Length; index++)
            {
                ElementalAlternateTraitId head = values[index];
                ElementalAlternateTraitId[] tail = values.Where((value,
                    tailIndex) => tailIndex != index).ToArray();
                foreach (ElementalAlternateTraitId[] permutation in
                    Permutations(tail))
                    yield return new[] { head }.Concat(permutation).ToArray();
            }
        }

        private static void Count(ElementalHeritageRace race, int expected)
        {
            Assertions.Equal(expected,
                ElementalAlternateTraitPolicy.ForRace(race).Count,
                race + " required alternate-trait count drifted.");
        }

        private static string ExpectedResistance(ElementalHeritageRace race)
        {
            switch (race)
            {
                case ElementalHeritageRace.Ifrit:
                    return "KMG.ElementalRaces.Ifrit.FireResistance";
                case ElementalHeritageRace.Oread:
                    return "KMG.ElementalRaces.Oread.AcidResistance";
                case ElementalHeritageRace.Sylph:
                    return "KMG.ElementalRaces.Sylph.ElectricityResistance";
                case ElementalHeritageRace.Undine:
                    return "KMG.ElementalRaces.Undine.ColdResistance";
                default:
                    throw new ArgumentOutOfRangeException("race");
            }
        }

        private static void Slots(ElementalAlternateTraitId id,
            ElementalRacialTraitSlot expected)
        {
            Assertions.Equal(expected,
                ElementalAlternateTraitPolicy.Find(id).ReplacedSlots,
                id + " replacement slots drifted.");
        }
    }
}
