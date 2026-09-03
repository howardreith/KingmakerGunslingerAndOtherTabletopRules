using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Visual.CharacterSystem;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    internal sealed class ElementalRaceVisualBlueprints
    {
        private readonly BlueprintRaceVisualPreset[] _presets;
        private readonly ElementalRaceVisualResourceRegistration[] _resources;

        internal ElementalRaceVisualBlueprints(
            ElementalRaceVisualDefinition definition,
            KingmakerEquipmentEntity body,
            IEnumerable<BlueprintRaceVisualPreset> presets,
            CustomizationOptions maleOptions,
            CustomizationOptions femaleOptions,
            IEnumerable<ElementalRaceVisualResourceRegistration> resources,
            bool usedFallback)
        {
            Definition = definition ?? throw new ArgumentNullException(
                "definition");
            Body = body ?? throw new ArgumentNullException("body");
            _presets = presets == null ? null : presets.ToArray();
            MaleOptions = maleOptions ?? throw new ArgumentNullException(
                "maleOptions");
            FemaleOptions = femaleOptions ?? throw new ArgumentNullException(
                "femaleOptions");
            _resources = resources == null ? null : resources.ToArray();
            UsedFallback = usedFallback;
            if (_presets == null || _presets.Length != 3 ||
                _presets.Any(value => value == null))
                throw new ArgumentException(
                    "Exactly three visual presets are required.", "presets");
            if (_resources == null || _resources.Length == 0 ||
                _resources.Any(value => value == null))
                throw new ArgumentException(
                    "Visual proxy resources are required.", "resources");
        }

        internal ElementalRaceVisualDefinition Definition { get; private set; }
        internal KingmakerEquipmentEntity Body { get; private set; }
        internal BlueprintRaceVisualPreset[] Presets
        { get { return (BlueprintRaceVisualPreset[])_presets.Clone(); } }
        internal CustomizationOptions MaleOptions { get; private set; }
        internal CustomizationOptions FemaleOptions { get; private set; }
        internal ElementalRaceVisualResourceRegistration[] Resources
        {
            get
            {
                return (ElementalRaceVisualResourceRegistration[])_resources
                    .Clone();
            }
        }
        internal bool UsedFallback { get; private set; }
        internal int BlueprintCount { get { return 4; } }
        internal int ResourceCount { get { return _resources.Length; } }
    }

    internal sealed class ElementalRaceVisualSet
    {
        private readonly ElementalRaceVisualBlueprints[] _ordered;
        private readonly ElementalRaceVisualResourceRegistry _registry;

        internal ElementalRaceVisualSet(
            IEnumerable<ElementalRaceVisualBlueprints> ordered,
            ElementalRaceVisualResourceRegistry registry)
        {
            _ordered = ordered == null ? null : ordered.ToArray();
            _registry = registry ?? throw new ArgumentNullException("registry");
            if (_ordered == null ||
                _ordered.Length != ElementalRaceCatalog.RaceCount ||
                _ordered.Any(value => value == null) ||
                _ordered[0].Definition.Kind != ElementalRaceKind.Ifrit ||
                _ordered[1].Definition.Kind != ElementalRaceKind.Oread ||
                _ordered[2].Definition.Kind != ElementalRaceKind.Sylph ||
                _ordered[3].Definition.Kind != ElementalRaceKind.Undine ||
                BlueprintCount != ElementalRaceVisualCatalog
                    .BlueprintIdentityCount ||
                ResourceCount != ElementalRaceVisualCatalog
                    .ResourceIdentityCount ||
                _registry.RegisteredCount != ResourceCount)
                throw new InvalidOperationException(
                    "Elemental visual set inventory or order drifted.");
        }

        internal int BlueprintCount
        { get { return _ordered.Sum(value => value.BlueprintCount); } }
        internal int ResourceCount
        { get { return _ordered.Sum(value => value.ResourceCount); } }

        internal ElementalRaceVisualBlueprints Require(ElementalRaceKind kind)
        {
            ElementalRaceVisualBlueprints value = _ordered.SingleOrDefault(
                candidate => candidate.Definition.Kind == kind);
            if (value == null)
                throw new InvalidOperationException(
                    "No visual definition was registered for " + kind + ".");
            return value;
        }

        internal IReadOnlyList<ElementalRaceVisualBlueprints> Ordered()
        {
            return (ElementalRaceVisualBlueprints[])_ordered.Clone();
        }

        internal void RollbackResources()
        {
            _registry.RollbackAll();
        }
    }
}
