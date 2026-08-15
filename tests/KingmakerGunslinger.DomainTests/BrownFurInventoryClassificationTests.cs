using System.Collections.Generic;
using KingmakerGunslinger.BrownFur;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BrownFurInventoryClassificationTests
    {
        private const string OrdinaryGuid =
            "3481906baed9487e8403e91a2e9d010a";

        internal static void GenericContractsAreDeterministic()
        {
            BrownFurInventoryClassificationDecision decision = Decide(
                OrdinaryGuid, "Personal", true, true, "1 minute/level", 1,
                new[] { "Kingmaker.UnitLogic.Buffs.Polymorph" }, 0);
            Assertions.Equal(BrownFurInventoryQualifications.Generic,
                decision.QualificationStatus,
                "An extendable Personal polymorph selector must use generic contracts.");
            Assertions.True(decision.ShareTransmutation.Contains(
                    "selected-variant canonicalization") &&
                decision.PowerfulChange.Contains("descriptor-preserving") &&
                decision.TransmutationSupremacy.Contains("native Extend") &&
                decision.IsExplained,
                "Generic inventory decisions must name all three qualified paths.");
        }

        internal static void EveryBonusCarrierFamilyIsSupported()
        {
            var carriers = new[] {
                "Kingmaker.Designers.Mechanics.Buffs.AddStatBonusAbilityValue",
                "Kingmaker.Designers.Mechanics.Buffs.ChangeUnitSize",
                "Kingmaker.UnitLogic.Buffs.Components.AddGenericStatBonus",
                "Kingmaker.UnitLogic.Buffs.Polymorph",
                "Kingmaker.UnitLogic.FactLogic.AddContextStatBonus",
                "Kingmaker.UnitLogic.FactLogic.AddStatBonus"
            };
            BrownFurInventoryClassificationDecision decision = Decide(
                OrdinaryGuid, "Touch", false, true, "1 minute/level",
                carriers.Length, carriers, 0);
            Assertions.Equal(BrownFurInventoryQualifications.Generic,
                decision.QualificationStatus,
                "Every installed positive bonus-carrier family must classify generically.");
            Assertions.True(decision.ShareTransmutation.StartsWith(
                    BrownFurInventoryQualifications.Ineligible) &&
                decision.RequiredAdapter.Contains("AddContextStatBonus") &&
                decision.RequiredAdapter.Contains("Polymorph"),
                "Dimension-level ineligibility must not hide qualified Powerful Change.");
        }

        internal static void NamedAndNoOpDurationsAreExact()
        {
            foreach (string guid in new[] {
                "3e4a0790fc2749bbacb1b3b1d2401148",
                "91266b6d2a4c4fd6b8e1549bc2381d12",
                "c7b52e9a09ef442f9308d9119f5877d2",
                "df7d13c967bce6a40bec3ba7c9f0e64c",
                "e48638596c955a74c8a32dbc90b518c1" })
                Assertions.Equal(BrownFurInventoryQualifications.Named,
                    Decide(guid, "Close", false, false, "1 hour", 0,
                        new string[0], 0).QualificationStatus,
                    "Each exact exceptional duration path must use a named adapter.");

            foreach (string guid in new[] {
                "16e23c7a8ae53cc42a93066d19766404",
                "3105d6e9febdc3f41a08d2b7dda1fe74",
                "4aa7942c3e62a164387a73184bca3fc1",
                "d752e84d9708495a93ab1237bd9c1dff",
                "e243740dfdb17a246b116b334ed0b165" })
            {
                BrownFurInventoryClassificationDecision decision = Decide(
                    guid, "Close", false, false, string.Empty, 0,
                    new string[0], 0);
                Assertions.Equal(BrownFurInventoryQualifications.Generic,
                    decision.QualificationStatus,
                    "Each exact instantaneous/permanent/selector path must be a proven no-op.");
                Assertions.True(decision.TransmutationSupremacy.Contains(
                    "proven instantaneous, permanent, or selector no-op"),
                    "No-op classification must be explicit.");
            }
        }

        internal static void UnknownStructuresFailClosed()
        {
            Assertions.Equal(BrownFurInventoryQualifications.Unexplained,
                Decide(OrdinaryGuid, "Close", false, false, "mystery", 0,
                    new string[0], 0).QualificationStatus,
                "An unknown non-Extend duration cannot be guessed.");
            Assertions.Equal(BrownFurInventoryQualifications.Unexplained,
                Decide(OrdinaryGuid, "Personal", false, true,
                    "1 minute/level", 0, new string[0], 1)
                    .QualificationStatus,
                "Hard-coded caster routing requires an explicit adapter.");
            Assertions.Equal(BrownFurInventoryQualifications.Unexplained,
                Decide(OrdinaryGuid, "Touch", false, true,
                    "1 minute/level", 1,
                    new[] { "Unknown.BonusCarrier" }, 0)
                    .QualificationStatus,
                "An unknown positive bonus carrier cannot be guessed.");
            Assertions.Equal(BrownFurInventoryQualifications.Unexplained,
                Decide("BAD", "Touch", false, true, "1 minute/level", 0,
                    new string[0], 0).QualificationStatus,
                "Malformed inventory identity must fail closed.");
        }

        private static BrownFurInventoryClassificationDecision Decide(
            string guid, string range, bool variants, bool extend,
            string duration, int bonuses, IEnumerable<string> carriers,
            int toCaster)
        {
            return BrownFurInventoryClassificationPolicy.Decide(
                new BrownFurInventoryClassificationInput(guid, range,
                    variants, extend, duration, bonuses, carriers, toCaster));
        }
    }
}
