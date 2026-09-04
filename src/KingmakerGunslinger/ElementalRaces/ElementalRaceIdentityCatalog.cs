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
        internal const int FeatIdentityCount = 25;
        internal const int MechanicIdentityCount = LegacyMechanicIdentityCount +
            HeritageIdentityCount + FeatIdentityCount;
        internal const int RaceBlueprintIdentityCount =
            LegacyMechanicIdentityCount + HeritageIdentityCount +
            ElementalRaceVisualCatalog.BlueprintIdentityCount;
        internal const int IdentityCount = RaceBlueprintIdentityCount +
            FeatIdentityCount;
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

        internal const string ElementalStrikeFeat =
            "KMG.ElementalRaces.Feats.ElementalStrike";
        internal const string ScorchingWeaponsFeat =
            "KMG.ElementalRaces.Feats.ScorchingWeapons";
        internal const string InnerFlameFeat =
            "KMG.ElementalRaces.Feats.InnerFlame";
        internal const string BlazingAuraFeat =
            "KMG.ElementalRaces.Feats.BlazingAura";
        internal const string FiresightFeat =
            "KMG.ElementalRaces.Feats.Firesight";
        internal const string AiryStepFeat =
            "KMG.ElementalRaces.Feats.AiryStep";
        internal const string WingsOfAirFeat =
            "KMG.ElementalRaces.Feats.WingsOfAir";
        internal const string CloudGazerFeat =
            "KMG.ElementalRaces.Feats.CloudGazer";
        internal const string InnerBreathFeat =
            "KMG.ElementalRaces.Feats.InnerBreath";
        internal const string HydraulicManeuverFeat =
            "KMG.ElementalRaces.Feats.HydraulicManeuver";
        internal const string TritonPortalFeat =
            "KMG.ElementalRaces.Feats.TritonPortal";
        internal const string ElementalStrikeAbility =
            "KMG.ElementalRaces.Feats.ElementalStrike.Ability";
        internal const string ElementalStrikeBuff =
            "KMG.ElementalRaces.Feats.ElementalStrike.Buff";
        internal const string ScorchingWeaponsAbility =
            "KMG.ElementalRaces.Feats.ScorchingWeapons.Ability";
        internal const string ScorchingWeaponsBuff =
            "KMG.ElementalRaces.Feats.ScorchingWeapons.Buff";
        internal const string ScorchingWeaponsEnchantment =
            "KMG.ElementalRaces.Feats.ScorchingWeapons.Enchantment";
        internal const string BlazingAuraAbility =
            "KMG.ElementalRaces.Feats.BlazingAura.Ability";
        internal const string BlazingAuraBuff =
            "KMG.ElementalRaces.Feats.BlazingAura.Buff";
        internal const string WingsOfAirBuff =
            "KMG.ElementalRaces.Feats.WingsOfAir.Buff";
        internal const string HydraulicManeuverAbility =
            "KMG.ElementalRaces.Feats.HydraulicManeuver.Ability";
        internal const string HydraulicBullRushAbility =
            "KMG.ElementalRaces.Feats.HydraulicManeuver.BullRushAbility";
        internal const string HydraulicDisarmAbility =
            "KMG.ElementalRaces.Feats.HydraulicManeuver.DisarmAbility";
        internal const string HydraulicTripAbility =
            "KMG.ElementalRaces.Feats.HydraulicManeuver.TripAbility";
        internal const string HydraulicDirtyTrickBlindAbility =
            "KMG.ElementalRaces.Feats.HydraulicManeuver.DirtyTrickBlindAbility";
        internal const string TritonPortalAbility =
            "KMG.ElementalRaces.Feats.TritonPortal.Ability";

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
            string[] raceBlueprints = legacyMechanics.Concat(
                    HeritageSymbols()).Concat(ElementalRaceVisualCatalog
                    .BlueprintSymbols()).ToArray();
            return raceBlueprints.Concat(FeatSymbols()).ToArray();
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

        internal static IReadOnlyList<string> FeatSymbols()
        {
            string[] symbols =
            {
                ElementalStrikeFeat, ScorchingWeaponsFeat, InnerFlameFeat,
                BlazingAuraFeat, FiresightFeat, AiryStepFeat, WingsOfAirFeat,
                CloudGazerFeat, InnerBreathFeat, HydraulicManeuverFeat,
                TritonPortalFeat, ElementalStrikeAbility,
                ElementalStrikeBuff, ScorchingWeaponsAbility,
                ScorchingWeaponsBuff, ScorchingWeaponsEnchantment,
                BlazingAuraAbility, BlazingAuraBuff, WingsOfAirBuff,
                HydraulicManeuverAbility, HydraulicBullRushAbility,
                HydraulicDisarmAbility, HydraulicTripAbility,
                HydraulicDirtyTrickBlindAbility, TritonPortalAbility
            };
            if (symbols.Length != FeatIdentityCount ||
                symbols.Distinct(StringComparer.Ordinal).Count() !=
                    symbols.Length)
                throw new InvalidOperationException(
                    "Elemental feat identity inventory drifted.");
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
