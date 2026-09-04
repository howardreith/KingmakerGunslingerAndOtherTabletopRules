using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces.Visuals;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalRaceIdentityCatalog
    {
        internal const int LegacyMechanicIdentityCount = 24;
        internal const int HeritageIdentityCount = 53;
        internal const int MechanicIdentityCount = LegacyMechanicIdentityCount +
            HeritageIdentityCount;
        internal const int IdentityCount = MechanicIdentityCount +
            ElementalRaceVisualCatalog.BlueprintIdentityCount;
        internal const int ManifestIdentityCount = IdentityCount +
            ElementalRaceVisualCatalog.ResourceIdentityCount;

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

        internal const string UnerringWeaponPrimaryAbility =
            "KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponPrimaryAbility";
        internal const string UnerringWeaponSecondaryAbility =
            "KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponSecondaryAbility";
        internal const string UnerringWeaponEnchantment =
            "KMG.ElementalRaces.Oread.Ironsoul.UnerringWeaponEnchantment";
        internal const string ChillTouchDeliveryAbility =
            "KMG.ElementalRaces.Undine.Rimesoul.ChillTouchDeliveryAbility";
        internal const string ShockingGraspDeliveryAbility =
            "KMG.ElementalRaces.Sylph.Stormsoul.ShockingGraspDeliveryAbility";

        internal const string AasimarRaceGuid = "b7f02ba92b363064fb873963bec275ee";
        internal const string TieflingRaceGuid = "5c4e42124dc2b4647af6e36cf2590500";
        internal const string KeenSensesGuid = "9c747d24f6321f744aa1bb4bd343880d";
        internal const string SlowAndSteadyGuid = "786588ad1694e61498e77321d4b07157";
        internal const string OutsiderTypeGuid = "9054d3988d491d944ac144e27b6bc318";
        internal const string BurningHandsGuid = "4783c3709a74a794dbe7c8e7e0b1b038";
        internal const string StoneFistGuid = "85067a04a97416949b5d1dbf986d93f3";
        internal const string FeatherStepGuid = "f3c0b267dd17a2a45a40805e31fe3cd1";

        internal static IReadOnlyList<string> Symbols()
        {
            string[] legacyMechanics = new[]
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
            string[] mechanics = legacyMechanics.Concat(HeritageSymbols())
                .ToArray();
            return mechanics.Concat(ElementalRaceVisualCatalog
                .BlueprintSymbols()).ToArray();
        }

        internal static IReadOnlyList<string> HeritageSymbols()
        {
            ElementalHeritageDefinition[] alternate = ElementalHeritagePolicy
                .Ordered().Where(entry => !entry.IsGeneral).ToArray();
            string[] symbols = ElementalHeritagePolicy.Ordered()
                .Select(entry => entry.SelectionSymbol)
                .Distinct(StringComparer.Ordinal)
                .Concat(ElementalHeritagePolicy.Ordered().Select(entry =>
                    entry.MarkerSymbol))
                .Concat(alternate.Select(entry => entry.AffinityFeatureSymbol))
                .Concat(alternate.SelectMany(entry => new[]
                {
                    entry.SlaFeatureSymbol,
                    entry.SlaResourceSymbol,
                    entry.SlaAbilitySymbol
                }))
                .Concat(new[]
                {
                    UnerringWeaponPrimaryAbility,
                    UnerringWeaponSecondaryAbility,
                    UnerringWeaponEnchantment,
                    ChillTouchDeliveryAbility,
                    ShockingGraspDeliveryAbility
                }).ToArray();
            if (symbols.Length != HeritageIdentityCount ||
                symbols.Distinct(StringComparer.Ordinal).Count() !=
                    symbols.Length)
                throw new InvalidOperationException(
                    "Elemental heritage identity inventory drifted.");
            return symbols;
        }

        internal static void Validate()
        {
            IReadOnlyList<string> symbols = Symbols();
            if (symbols.Count != IdentityCount)
                throw new InvalidOperationException("Elemental race identity count drifted.");
            ElementalRaceVisualCatalog.Validate();
            string[] all = symbols.Concat(ElementalRaceVisualCatalog
                .ResourceSymbols()).ToArray();
            if (all.Length != ManifestIdentityCount ||
                all.Distinct(StringComparer.Ordinal).Count() != all.Length)
                throw new InvalidOperationException(
                    "Elemental blueprint and visual resource identities collided.");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < symbols.Count; index++)
                if (string.IsNullOrWhiteSpace(symbols[index]) ||
                    !seen.Add(symbols[index]))
                    throw new InvalidOperationException(
                        "Elemental race identity symbols must be nonempty and unique.");
        }
    }
}
