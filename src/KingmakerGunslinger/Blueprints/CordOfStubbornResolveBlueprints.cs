using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;

namespace KingmakerGunslinger.Blueprints
{
    internal static class CordOfStubbornResolveBlueprints
    {
        internal const string Symbol = "KMG.Items.CordOfStubbornResolve";

        internal static BlueprintItemEquipmentBelt Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintItemEquipmentBelt[] donors = library.GetAllBlueprints()
                .OfType<BlueprintItemEquipmentBelt>()
                .Where(IsNativeConstitutionTwoBelt).ToArray();
            if (donors.Length != 1)
                throw new InvalidOperationException("Expected exactly one native +2 Constitution belt donor; observed " + donors.Length + ".");
            BlueprintItemEquipmentBelt donor = donors[0];
            return registry.Register<BlueprintItemEquipmentBelt>(Symbol, () =>
            {
                BlueprintItemEquipmentBelt cord = BlueprintCloneService.Clone(donor,
                    "KMG_CordOfStubbornResolve_Item");
                BlueprintItemAccess.Resolve().ConfigureNonStackable(cord,
                    LocalizationService.Create("KMG.Item.CordOfStubbornResolve.Name",
                        "Cord of Stubborn Resolve"),
                    LocalizationService.Create("KMG.Item.CordOfStubbornResolve.Description",
                        "This belt grants a +2 enhancement bonus to Constitution. While equipped, an effect that would cause fatigue instead deals 1d6 nonlethal damage. An effect that would cause exhaustion instead deals 1d6 nonlethal damage and leaves the wearer fatigued."),
                    LocalizationService.Create("KMG.Item.CordOfStubbornResolve.Flavor",
                        "This tightly knotted cord steadies body and resolve against consuming weariness."),
                    15000, 1f);
                return cord;
            });
        }

        private static bool IsNativeConstitutionTwoBelt(
            BlueprintItemEquipmentBelt belt)
        {
            if (belt == null || belt.Cost != 4000) return false;
            return belt.Enchantments.SelectMany(e => e.ComponentsArray ??
                    Array.Empty<BlueprintComponent>())
                .OfType<AddStatBonus>()
                .Any(b => b.Stat == StatType.Constitution && b.Value == 2 &&
                    b.Descriptor == ModifierDescriptor.Enhancement);
        }
    }
}
