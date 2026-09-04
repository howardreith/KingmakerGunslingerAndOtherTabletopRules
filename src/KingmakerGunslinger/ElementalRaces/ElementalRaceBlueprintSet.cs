using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.ElementalRaces.Visuals;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalRaceBlueprints
    {
        internal ElementalRaceBlueprints(ElementalRaceDefinition definition,
            BlueprintRace race, BlueprintFeature resistance,
            BlueprintFeature affinity, BlueprintFeature slaFeature,
            BlueprintAbilityResource slaResource, BlueprintAbility slaAbility,
            ElementalRaceVisualBlueprints visuals,
            ElementalHeritageRaceBlueprints heritages)
        {
            Definition = definition ?? throw new ArgumentNullException("definition");
            Race = race ?? throw new ArgumentNullException("race");
            Resistance = resistance ?? throw new ArgumentNullException("resistance");
            Affinity = affinity ?? throw new ArgumentNullException("affinity");
            SlaFeature = slaFeature ?? throw new ArgumentNullException("slaFeature");
            SlaResource = slaResource ?? throw new ArgumentNullException("slaResource");
            SlaAbility = slaAbility ?? throw new ArgumentNullException("slaAbility");
            Visuals = visuals ?? throw new ArgumentNullException("visuals");
            Heritages = heritages ?? throw new ArgumentNullException(
                "heritages");
        }

        internal ElementalRaceDefinition Definition { get; private set; }
        internal BlueprintRace Race { get; private set; }
        internal BlueprintFeature Resistance { get; private set; }
        internal BlueprintFeature Affinity { get; private set; }
        internal BlueprintFeature SlaFeature { get; private set; }
        internal BlueprintAbilityResource SlaResource { get; private set; }
        internal BlueprintAbility SlaAbility { get; private set; }
        internal ElementalRaceVisualBlueprints Visuals { get; private set; }
        internal ElementalHeritageRaceBlueprints Heritages
        { get; private set; }
        internal int Count
        {
            get { return 6 + Visuals.BlueprintCount + Heritages.RegisteredCount; }
        }
    }

    internal sealed class ElementalRaceBlueprintSet
    {
        private readonly ElementalRaceBlueprints[] _ordered;

        internal ElementalRaceBlueprintSet(
            IEnumerable<ElementalRaceBlueprints> ordered,
            ElementalRaceVisualSet visuals)
        {
            _ordered = ordered == null ? null : ordered.ToArray();
            Visuals = visuals ?? throw new ArgumentNullException("visuals");
            if (_ordered == null ||
                _ordered.Length != ElementalRaceCatalog.RaceCount ||
                _ordered.Any(value => value == null) ||
                _ordered.Select(value => value.Definition.Kind).Distinct()
                    .Count() != ElementalRaceCatalog.RaceCount ||
                _ordered[0].Definition.Kind != ElementalRaceKind.Ifrit ||
                _ordered[1].Definition.Kind != ElementalRaceKind.Oread ||
                _ordered[2].Definition.Kind != ElementalRaceKind.Sylph ||
                _ordered[3].Definition.Kind != ElementalRaceKind.Undine)
                throw new InvalidOperationException(
                    "Elemental race blueprint order must be Ifrit, Oread, Sylph, Undine.");
            if (Count != ElementalRaceIdentityCatalog
                    .RaceBlueprintIdentityCount)
                throw new InvalidOperationException(
                    "Elemental race blueprint count does not match the identity catalog.");
        }

        internal ElementalRaceBlueprints Ifrit { get { return _ordered[0]; } }
        internal ElementalRaceBlueprints Oread { get { return _ordered[1]; } }
        internal ElementalRaceBlueprints Sylph { get { return _ordered[2]; } }
        internal ElementalRaceBlueprints Undine { get { return _ordered[3]; } }
        internal ElementalRaceVisualSet Visuals { get; private set; }
        internal int Count { get { return _ordered.Sum(value => value.Count); } }

        internal BlueprintRace[] OrderedRaces()
        {
            return _ordered.Select(value => value.Race).ToArray();
        }

        internal IReadOnlyList<ElementalRaceBlueprints> OrderedBlueprints()
        {
            return (ElementalRaceBlueprints[])_ordered.Clone();
        }

        internal void RollbackVisualResources()
        {
            Visuals.RollbackResources();
        }
    }
}
