using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalHeritageBlueprints
    {
        internal ElementalHeritageBlueprints(
            ElementalHeritageDefinition definition, BlueprintFeature marker,
            BlueprintFeature affinity, BlueprintFeature slaFeature,
            BlueprintAbilityResource slaResource, BlueprintAbility slaAbility,
            IEnumerable<BlueprintScriptableObject> auxiliaryBlueprints)
        {
            Definition = definition ?? throw new ArgumentNullException(
                "definition");
            Marker = marker ?? throw new ArgumentNullException("marker");
            Affinity = affinity ?? throw new ArgumentNullException("affinity");
            SlaFeature = slaFeature ?? throw new ArgumentNullException(
                "slaFeature");
            SlaResource = slaResource ?? throw new ArgumentNullException(
                "slaResource");
            SlaAbility = slaAbility ?? throw new ArgumentNullException(
                "slaAbility");
            AuxiliaryBlueprints = auxiliaryBlueprints == null
                ? new BlueprintScriptableObject[0]
                : auxiliaryBlueprints.ToArray();
            if (AuxiliaryBlueprints.Any(value => value == null))
                throw new ArgumentException(
                    "Heritage auxiliary blueprints must be non-null.");
        }

        internal ElementalHeritageDefinition Definition { get; private set; }
        internal BlueprintFeature Marker { get; private set; }
        internal BlueprintFeature Affinity { get; private set; }
        internal BlueprintFeature SlaFeature { get; private set; }
        internal BlueprintAbilityResource SlaResource { get; private set; }
        internal BlueprintAbility SlaAbility { get; private set; }
        internal BlueprintScriptableObject[] AuxiliaryBlueprints
        { get; private set; }
        internal int RegisteredCount
        {
            get
            {
                return 1 + (Definition.IsGeneral ? 0 : 4) +
                    AuxiliaryBlueprints.Length;
            }
        }
    }

    internal sealed class ElementalHeritageRaceBlueprints
    {
        private readonly ElementalHeritageBlueprints[] _choices;

        internal ElementalHeritageRaceBlueprints(ElementalHeritageRace race,
            BlueprintFeatureSelection selection,
            IEnumerable<ElementalHeritageBlueprints> choices)
        {
            Race = race;
            Selection = selection ?? throw new ArgumentNullException(
                "selection");
            _choices = choices == null ? null : choices.ToArray();
            if (_choices == null ||
                _choices.Length != ElementalHeritagePolicy.ChoicesPerRace ||
                _choices.Any(value => value == null ||
                    value.Definition.ParentRace != race) ||
                _choices.Count(value => value.Definition.IsGeneral) != 1 ||
                Selection.AllFeatures == null ||
                !Selection.AllFeatures.SequenceEqual(_choices.Select(value =>
                    value.Marker)))
                throw new InvalidOperationException(
                    "The elemental heritage selection graph is incomplete.");
        }

        internal ElementalHeritageRace Race { get; private set; }
        internal BlueprintFeatureSelection Selection { get; private set; }
        internal int RegisteredCount
        {
            get { return 1 + _choices.Sum(value => value.RegisteredCount); }
        }

        internal IReadOnlyList<ElementalHeritageBlueprints> Choices()
        {
            return (ElementalHeritageBlueprints[])_choices.Clone();
        }

        internal ElementalHeritageBlueprints General
        {
            get { return _choices.Single(value => value.Definition.IsGeneral); }
        }

        internal ElementalHeritageBlueprints Require(ElementalHeritageId id)
        {
            return _choices.Single(value => value.Definition.Id == id);
        }
    }
}
