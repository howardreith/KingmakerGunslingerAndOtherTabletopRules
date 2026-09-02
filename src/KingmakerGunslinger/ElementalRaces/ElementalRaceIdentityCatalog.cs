using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalRaceIdentityCatalog
    {
        internal const int IdentityCount = 24;

        internal const string IfritRace = "KMG.ElementalRaces.Ifrit.Race";
        internal const string IfritResistance = "KMG.ElementalRaces.Ifrit.FireResistance";
        internal const string IfritAffinity = "KMG.ElementalRaces.Ifrit.FireAffinity";
        internal const string IfritSlaFeature = "KMG.ElementalRaces.Ifrit.BurningHandsFeature";
        internal const string IfritSlaResource = "KMG.ElementalRaces.Ifrit.BurningHandsResource";
        internal const string IfritSlaAbility = "KMG.ElementalRaces.Ifrit.BurningHandsAbility";

        internal const string OreadRace = "KMG.ElementalRaces.Oread.Race";
        internal const string OreadResistance = "KMG.ElementalRaces.Oread.AcidResistance";
        internal const string OreadAffinity = "KMG.ElementalRaces.Oread.AcidAffinity";
        internal const string OreadSlaFeature = "KMG.ElementalRaces.Oread.StoneFistFeature";
        internal const string OreadSlaResource = "KMG.ElementalRaces.Oread.StoneFistResource";
        internal const string OreadSlaAbility = "KMG.ElementalRaces.Oread.StoneFistAbility";

        internal const string SylphRace = "KMG.ElementalRaces.Sylph.Race";
        internal const string SylphResistance = "KMG.ElementalRaces.Sylph.ElectricityResistance";
        internal const string SylphAffinity = "KMG.ElementalRaces.Sylph.AirAffinity";
        internal const string SylphSlaFeature = "KMG.ElementalRaces.Sylph.FeatherStepFeature";
        internal const string SylphSlaResource = "KMG.ElementalRaces.Sylph.FeatherStepResource";
        internal const string SylphSlaAbility = "KMG.ElementalRaces.Sylph.FeatherStepAbility";

        internal const string UndineRace = "KMG.ElementalRaces.Undine.Race";
        internal const string UndineResistance = "KMG.ElementalRaces.Undine.ColdResistance";
        internal const string UndineAffinity = "KMG.ElementalRaces.Undine.WaterAffinity";
        internal const string UndineSlaFeature = "KMG.ElementalRaces.Undine.HydraulicPushFeature";
        internal const string UndineSlaResource = "KMG.ElementalRaces.Undine.HydraulicPushResource";
        internal const string UndineSlaAbility = "KMG.ElementalRaces.Undine.HydraulicPushAbility";

        internal const string AasimarRaceGuid = "b7f02ba92b363064fb873963bec275ee";
        internal const string KeenSensesGuid = "9c747d24f6321f744aa1bb4bd343880d";
        internal const string SlowAndSteadyGuid = "786588ad1694e61498e77321d4b07157";
        internal const string OutsiderTypeGuid = "9054d3988d491d944ac144e27b6bc318";
        internal const string BurningHandsGuid = "4783c3709a74a794dbe7c8e7e0b1b038";
        internal const string StoneFistGuid = "85067a04a97416949b5d1dbf986d93f3";
        internal const string FeatherStepGuid = "f3c0b267dd17a2a45a40805e31fe3cd1";

        internal static IReadOnlyList<string> Symbols()
        {
            return new[]
            {
                IfritRace, IfritResistance, IfritAffinity, IfritSlaFeature,
                IfritSlaResource, IfritSlaAbility,
                OreadRace, OreadResistance, OreadAffinity, OreadSlaFeature,
                OreadSlaResource, OreadSlaAbility,
                SylphRace, SylphResistance, SylphAffinity, SylphSlaFeature,
                SylphSlaResource, SylphSlaAbility,
                UndineRace, UndineResistance, UndineAffinity, UndineSlaFeature,
                UndineSlaResource, UndineSlaAbility
            };
        }

        internal static void Validate()
        {
            IReadOnlyList<string> symbols = Symbols();
            if (symbols.Count != IdentityCount)
                throw new InvalidOperationException("Elemental race identity count drifted.");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < symbols.Count; index++)
                if (string.IsNullOrWhiteSpace(symbols[index]) ||
                    !seen.Add(symbols[index]))
                    throw new InvalidOperationException(
                        "Elemental race identity symbols must be nonempty and unique.");
        }
    }
}
