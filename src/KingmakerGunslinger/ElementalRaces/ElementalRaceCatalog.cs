using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalRaceCatalog
    {
        internal const int RaceCount = 4;

        internal static IReadOnlyList<ElementalRaceDefinition> Ordered()
        {
            var result = new[]
            {
                new ElementalRaceDefinition(ElementalRaceKind.Ifrit, "Ifrit",
                    "Ifrits are native outsiders touched by elemental fire. They are quick and compelling but often impulsive. Ifrits gain +2 Dexterity, +2 Charisma, -2 Wisdom, fire resistance 5, Keen Senses, +1 DC with Fire spells, and Burning Hands once per day as a Charisma-based spell-like ability using total character level.",
                    ElementalRaceIdentityCatalog.IfritRace,
                    ElementalRaceIdentityCatalog.IfritResistance,
                    ElementalRaceIdentityCatalog.IfritAffinity,
                    ElementalRaceIdentityCatalog.IfritSlaFeature,
                    ElementalRaceIdentityCatalog.IfritSlaResource,
                    ElementalRaceIdentityCatalog.IfritSlaAbility,
                    "Burning Hands", "Once per day, unleash the native Burning Hands cone. Caster level equals total character level.",
                    ElementalRaceIdentityCatalog.BurningHandsGuid, 1,
                    DamageEnergyType.Fire, SpellDescriptor.Fire, false,
                    new ElementalStatAdjustment(StatType.Dexterity, 2),
                    new ElementalStatAdjustment(StatType.Charisma, 2),
                    new ElementalStatAdjustment(StatType.Wisdom, -2)),
                new ElementalRaceDefinition(ElementalRaceKind.Oread, "Oread",
                    "Oreads are native outsiders touched by elemental earth. They gain +2 Strength, +2 Wisdom, -2 Charisma, acid resistance 5, Keen Senses, +1 DC with Acid spells, and Stone Fist once per day using total character level. Their 20-foot Slow and Steady movement is not reduced by armor or encumbrance.",
                    ElementalRaceIdentityCatalog.OreadRace,
                    ElementalRaceIdentityCatalog.OreadResistance,
                    ElementalRaceIdentityCatalog.OreadAffinity,
                    ElementalRaceIdentityCatalog.OreadSlaFeature,
                    ElementalRaceIdentityCatalog.OreadSlaResource,
                    ElementalRaceIdentityCatalog.OreadSlaAbility,
                    "Stone Fist", "Once per day, use Kingmaker's native Stone Fist spell as a spell-like ability. Caster level equals total character level.",
                    ElementalRaceIdentityCatalog.StoneFistGuid, 1,
                    DamageEnergyType.Acid, SpellDescriptor.Acid, true,
                    new ElementalStatAdjustment(StatType.Strength, 2),
                    new ElementalStatAdjustment(StatType.Wisdom, 2),
                    new ElementalStatAdjustment(StatType.Charisma, -2)),
                new ElementalRaceDefinition(ElementalRaceKind.Sylph, "Sylph",
                    "Sylphs are native outsiders touched by elemental air. They gain +2 Dexterity, +2 Intelligence, -2 Constitution, electricity resistance 5, Keen Senses, +1 DC with Electricity spells, and Feather Step once per day using total character level. Feather Step is Kingmaker's practical substitute for Feather Fall.",
                    ElementalRaceIdentityCatalog.SylphRace,
                    ElementalRaceIdentityCatalog.SylphResistance,
                    ElementalRaceIdentityCatalog.SylphAffinity,
                    ElementalRaceIdentityCatalog.SylphSlaFeature,
                    ElementalRaceIdentityCatalog.SylphSlaResource,
                    ElementalRaceIdentityCatalog.SylphSlaAbility,
                    "Feather Step", "Once per day, use Kingmaker's native Feather Step spell as a spell-like ability. Caster level equals total character level.",
                    ElementalRaceIdentityCatalog.FeatherStepGuid, 1,
                    DamageEnergyType.Electricity,
                    SpellDescriptor.Electricity, false,
                    new ElementalStatAdjustment(StatType.Dexterity, 2),
                    new ElementalStatAdjustment(StatType.Intelligence, 2),
                    new ElementalStatAdjustment(StatType.Constitution, -2)),
                new ElementalRaceDefinition(ElementalRaceKind.Undine, "Undine",
                    "Undines are native outsiders touched by elemental water. They gain +2 Dexterity, +2 Wisdom, -2 Strength, cold resistance 5, Keen Senses, +1 DC with Cold spells, and Hydraulic Push once per day using total character level. Kingmaker has no ordinary player swimming system, so swim clauses have no mechanical effect.",
                    ElementalRaceIdentityCatalog.UndineRace,
                    ElementalRaceIdentityCatalog.UndineResistance,
                    ElementalRaceIdentityCatalog.UndineAffinity,
                    ElementalRaceIdentityCatalog.UndineSlaFeature,
                    ElementalRaceIdentityCatalog.UndineSlaResource,
                    ElementalRaceIdentityCatalog.UndineSlaAbility,
                    "Hydraulic Push", "Once per day, make a Bull Rush against one creature using total character level plus the best Intelligence, Wisdom, or Charisma modifier. There is no saving throw.",
                    null, 1, DamageEnergyType.Cold,
                    SpellDescriptor.Cold, false,
                    new ElementalStatAdjustment(StatType.Dexterity, 2),
                    new ElementalStatAdjustment(StatType.Wisdom, 2),
                    new ElementalStatAdjustment(StatType.Strength, -2))
            };
            if (result.Length != RaceCount)
                throw new InvalidOperationException("Elemental race catalog count drifted.");
            return result;
        }
    }
}
