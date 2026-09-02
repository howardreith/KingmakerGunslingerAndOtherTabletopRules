using System;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;

namespace KingmakerGunslinger.ElementalRaces
{
    internal enum ElementalRaceKind
    {
        Ifrit = 0,
        Oread = 1,
        Sylph = 2,
        Undine = 3
    }

    internal sealed class ElementalStatAdjustment
    {
        internal ElementalStatAdjustment(StatType stat, int value)
        {
            if (value != 2 && value != -2)
                throw new ArgumentOutOfRangeException("value");
            Stat = stat;
            Value = value;
        }

        internal StatType Stat { get; private set; }
        internal int Value { get; private set; }
    }

    internal sealed class ElementalRaceDefinition
    {
        internal ElementalRaceDefinition(ElementalRaceKind kind,
            string displayName, string description, string raceSymbol,
            string resistanceSymbol, string affinitySymbol,
            string slaFeatureSymbol, string slaResourceSymbol,
            string slaAbilitySymbol, string slaName, string slaDescription,
            string donorAbilityGuid, int spellLevel,
            DamageEnergyType resistance, SpellDescriptor affinity,
            bool slowAndSteady, params ElementalStatAdjustment[] stats)
        {
            if (string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(raceSymbol) ||
                string.IsNullOrWhiteSpace(resistanceSymbol) ||
                string.IsNullOrWhiteSpace(affinitySymbol) ||
                string.IsNullOrWhiteSpace(slaFeatureSymbol) ||
                string.IsNullOrWhiteSpace(slaResourceSymbol) ||
                string.IsNullOrWhiteSpace(slaAbilitySymbol) ||
                string.IsNullOrWhiteSpace(slaName) ||
                string.IsNullOrWhiteSpace(slaDescription) ||
                spellLevel < 1 || stats == null || stats.Length != 3)
                throw new ArgumentException(
                    "An elemental race definition is incomplete.");
            Kind = kind;
            DisplayName = displayName;
            Description = description;
            RaceSymbol = raceSymbol;
            ResistanceSymbol = resistanceSymbol;
            AffinitySymbol = affinitySymbol;
            SlaFeatureSymbol = slaFeatureSymbol;
            SlaResourceSymbol = slaResourceSymbol;
            SlaAbilitySymbol = slaAbilitySymbol;
            SlaName = slaName;
            SlaDescription = slaDescription;
            DonorAbilityGuid = donorAbilityGuid;
            SpellLevel = spellLevel;
            Resistance = resistance;
            Affinity = affinity;
            SlowAndSteady = slowAndSteady;
            Stats = (ElementalStatAdjustment[])stats.Clone();
        }

        internal ElementalRaceKind Kind { get; private set; }
        internal string DisplayName { get; private set; }
        internal string Description { get; private set; }
        internal string RaceSymbol { get; private set; }
        internal string ResistanceSymbol { get; private set; }
        internal string AffinitySymbol { get; private set; }
        internal string SlaFeatureSymbol { get; private set; }
        internal string SlaResourceSymbol { get; private set; }
        internal string SlaAbilitySymbol { get; private set; }
        internal string SlaName { get; private set; }
        internal string SlaDescription { get; private set; }
        internal string DonorAbilityGuid { get; private set; }
        internal int SpellLevel { get; private set; }
        internal DamageEnergyType Resistance { get; private set; }
        internal SpellDescriptor Affinity { get; private set; }
        internal bool SlowAndSteady { get; private set; }
        internal ElementalStatAdjustment[] Stats { get; private set; }
    }
}
