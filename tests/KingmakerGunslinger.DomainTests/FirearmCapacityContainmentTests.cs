using System;
using System.Linq;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// RISK-01 / S4 T5: the legacy Advanced Revolver stays registered and
    /// loadable but unsupported, and no other production firearm may declare
    /// a capacity the saved state tokens cannot represent.
    /// </summary>
    internal static class FirearmCapacityContainmentTests
    {
        private static ProductionFirearmWeaponSpec[] Production()
        {
            return new[] { ProductionFirearmCatalog.CreatePistol(), ProductionFirearmCatalog.CreateMusket(),
                ProductionFirearmCatalog.CreateBlunderbuss(), ProductionFirearmCatalog.CreateAdvancedRifle(),
                ProductionFirearmCatalog.CreateAdvancedRevolver() };
        }

        private static ProductionFirearmWeaponSpec Revolver(string key, bool fireable,
            ProductionFirearmAcquisitionRole role, FirearmDefinition definition)
        {
            return new ProductionFirearmWeaponSpec(key, "Advanced Revolver", definition, 1, 8, 4,
                false, 4000, 4f, fireable, role);
        }

        internal static void DurableCapacityMatchesTheRegisteredTokens()
        {
            int largest = FirearmStateTokenCatalog.CreateCapacityOneDiagnostic().Definitions
                .Max(value => value.State.LoadedRounds);
            Assertions.Equal(FirearmStateTokenCatalog.DurableLeadBallCapacity, largest,
                "The durable capacity must be the largest round count the registered tokens hold.");
            FirearmState two = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                new FirearmStateRules(6, new[] { FirearmStateTokenCatalog.DiagnosticLeadBall }),
                FirearmStateTokenCatalog.DiagnosticLeadBall, 2);
            Assertions.Throws<NotSupportedException>(
                () => FirearmStateTokenCatalog.CreateCapacityOneDiagnostic().Encode(two),
                "A two-round state must be unrepresentable by the registered tokens.");
        }

        internal static void ProductionCatalogIsContained()
        {
            FirearmCapacityContainment.Validate(Production());
            ProductionFirearmWeaponSpec[] unrepresentable = Production()
                .Where(value => !FirearmCapacityContainment.IsRepresentable(value.Definition)).ToArray();
            Assertions.True(unrepresentable.Length == 1 && unrepresentable[0].Key == "advanced-revolver",
                "Only the legacy Advanced Revolver may exceed the saved capacity.");
            ProductionFirearmWeaponSpec revolver = unrepresentable[0];
            Assertions.True(FirearmCapacityContainment.IsContainedLegacy(revolver) && !revolver.IsPlayerFireable &&
                revolver.AcquisitionRole == ProductionFirearmAcquisitionRole.LegacyRecognitionOnly,
                "The revolver must be unequippable and recognition-only.");
            Assertions.Equal(6, revolver.Definition.Capacity, "The revolver's declared capacity is preserved, not rewritten.");
        }

        internal static void UnrepresentableCapacityIsRefused()
        {
            FirearmDefinition six = FirearmDefinitions.CreateAdvancedRevolver();
            Assertions.Throws<InvalidOperationException>(() => FirearmCapacityContainment.Validate(new[] {
                Revolver("advanced-revolver", true, ProductionFirearmAcquisitionRole.LegacyRecognitionOnly, six) }),
                "Making the revolver fireable must fail registration.");
            Assertions.Throws<InvalidOperationException>(() => FirearmCapacityContainment.Validate(new[] {
                Revolver("advanced-revolver", false, ProductionFirearmAcquisitionRole.OrdinaryCampaignCraftingBase, six) }),
                "Publishing the revolver as a crafting base must fail registration.");
            Assertions.Throws<InvalidOperationException>(() => FirearmCapacityContainment.Validate(new[] {
                Revolver("pepperbox", false, ProductionFirearmAcquisitionRole.LegacyRecognitionOnly, six) }),
                "The exception must be exact to the legacy revolver's identity.");
            FirearmDefinition two = new FirearmDefinition(FirearmEra.Advanced, FirearmKind.Revolver,
                2, 20, 1, 5, new ReloadProfile(ReloadActionType.Move, true, 2), false);
            Assertions.Throws<InvalidOperationException>(() => FirearmCapacityContainment.Validate(new[] {
                Revolver("advanced-revolver", false, ProductionFirearmAcquisitionRole.LegacyRecognitionOnly, two) }),
                "A different capacity under the legacy identity must fail registration.");
            Assertions.True(FirearmCapacityContainment.IsRepresentable(FirearmDefinitions.CreateAdvancedRifle()),
                "A capacity-one firearm is representable.");
        }
    }
}
