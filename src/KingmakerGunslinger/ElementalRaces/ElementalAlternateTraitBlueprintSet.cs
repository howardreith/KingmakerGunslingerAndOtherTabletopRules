using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalAlternateTraitBlueprints
    {
        internal ElementalAlternateTraitBlueprints(
            ElementalAlternateTraitDefinition definition,
            BlueprintFeature marker, BlueprintFeature provider)
        {
            Definition = definition ?? throw new ArgumentNullException(
                "definition");
            Marker = marker ?? throw new ArgumentNullException("marker");
            Provider = provider ?? throw new ArgumentNullException(
                "provider");
            if (ReferenceEquals(Marker, Provider))
                throw new InvalidOperationException(
                    "A selectable trait marker cannot also be its hidden provider.");
        }

        internal ElementalAlternateTraitDefinition Definition
        { get; private set; }
        internal BlueprintFeature Marker { get; private set; }
        internal BlueprintFeature Provider { get; private set; }
    }

    internal sealed class ElementalAlternateTraitSelectionBlueprints
    {
        internal ElementalAlternateTraitSelectionBlueprints(
            ElementalAlternateTraitSelectionDefinition definition,
            BlueprintFeatureSelection selection,
            BlueprintFeature retainMarker,
            IEnumerable<ElementalAlternateTraitBlueprints> choices)
        {
            Definition = definition ?? throw new ArgumentNullException(
                "definition");
            Selection = selection ?? throw new ArgumentNullException(
                "selection");
            RetainMarker = retainMarker ?? throw new ArgumentNullException(
                "retainMarker");
            Choices = choices == null ? null : choices.ToArray();
            BlueprintFeature[] expected = new[] { RetainMarker }.Concat(
                Choices == null ? Enumerable.Empty<BlueprintFeature>() :
                Choices.Select(value => value.Marker)).ToArray();
            if (Choices == null || Choices.Length == 0 ||
                Choices.Any(value => value == null ||
                    value.Definition.ParentRace != definition.Race ||
                    value.Definition.PrimarySlot != definition.Slot) ||
                Selection.Features == null ||
                Selection.AllFeatures == null ||
                !Selection.Features.SequenceEqual(expected) ||
                !Selection.AllFeatures.SequenceEqual(expected))
                throw new InvalidOperationException(
                    "An alternate-trait slot selection graph is incomplete.");
        }

        internal ElementalAlternateTraitSelectionDefinition Definition
        { get; private set; }
        internal BlueprintFeatureSelection Selection { get; private set; }
        internal BlueprintFeature RetainMarker { get; private set; }
        internal ElementalAlternateTraitBlueprints[] Choices
        { get; private set; }
    }

    internal sealed class ElementalAlternateTraitRaceBlueprints
    {
        private readonly ElementalAlternateTraitBlueprints[] m_Traits;
        private readonly ElementalAlternateTraitSelectionBlueprints[]
            m_Selections;

        internal ElementalAlternateTraitRaceBlueprints(
            ElementalHeritageRace race,
            IEnumerable<ElementalAlternateTraitBlueprints> traits,
            IEnumerable<ElementalAlternateTraitSelectionBlueprints>
                selections)
        {
            Race = race;
            m_Traits = traits == null ? null : traits.ToArray();
            m_Selections = selections == null ? null : selections.ToArray();
            if (m_Traits == null || m_Selections == null ||
                m_Traits.Length != ElementalAlternateTraitPolicy.ForRace(
                    race).Count ||
                m_Selections.Length != ElementalAlternateTraitPolicy
                    .SelectionsForRace(race).Count ||
                m_Traits.Any(value => value == null ||
                    value.Definition.ParentRace != race) ||
                m_Selections.Any(value => value == null ||
                    value.Definition.Race != race) ||
                m_Selections.SelectMany(value => value.Choices).Select(value =>
                    value.Definition.Id).Distinct().Count() != m_Traits.Length)
                throw new InvalidOperationException(
                    "The parent-race alternate-trait graph is incomplete.");
        }

        internal ElementalHeritageRace Race { get; private set; }
        internal int RegisteredCount
        {
            get { return m_Traits.Length * 2 + m_Selections.Length * 2; }
        }

        internal IReadOnlyList<ElementalAlternateTraitBlueprints> Traits()
        {
            return (ElementalAlternateTraitBlueprints[])m_Traits.Clone();
        }

        internal IReadOnlyList<ElementalAlternateTraitSelectionBlueprints>
            Selections()
        {
            return (ElementalAlternateTraitSelectionBlueprints[])
                m_Selections.Clone();
        }

        internal ElementalAlternateTraitBlueprints Require(
            ElementalAlternateTraitId id)
        {
            return m_Traits.Single(value => value.Definition.Id == id);
        }

        internal BlueprintFeature[] OwnedProviders()
        {
            return m_Traits.Select(value => value.Provider).ToArray();
        }
    }
}
