using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;

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
            BlueprintItemEquipmentBelt[] allBelts = library.GetAllBlueprints()
                .OfType<BlueprintItemEquipmentBelt>().ToArray();
            BlueprintItemEquipmentBelt[] donors = allBelts
                .Where(IsNativeConstitutionTwoBelt).ToArray();
            if (donors.Length != 1)
                throw new InvalidOperationException("Expected exactly one native +2 Constitution belt donor; observed " + donors.Length +
                    "; installed belts=" + string.Join("|", allBelts
                        .OrderBy(b => b.name, StringComparer.Ordinal)
                        .Select(b => b.name + ":" + b.Cost).ToArray()) + ".");
            BlueprintItemEquipmentBelt donor = donors[0];
            return registry.Register<BlueprintItemEquipmentBelt>(Symbol, () =>
            {
                BlueprintItemEquipmentBelt cord = BlueprintCloneService.Clone(donor,
                    "KMG_CordOfStubbornResolve_Item");
                BlueprintItemAccess.Resolve().ConfigureNonStackable(cord,
                    LocalizationService.Create("KMG.Item.CordOfStubbornResolve.Name",
                        "Cord of Stubborn Resolve"),
                    LocalizationService.Create("KMG.Item.CordOfStubbornResolve.Description",
                        "This belt grants a +2 enhancement bonus to Constitution. Kingmaker has no usable native nonlethal-damage rule path, so while equipped an effect that would cause fatigue instead deals 1d6 untyped, non-hostile self-damage that cannot reduce you below 1 hit point. An effect that would cause exhaustion deals that damage and leaves you fatigued instead."),
                    LocalizationService.Create("KMG.Item.CordOfStubbornResolve.Flavor",
                        "This tightly knotted cord steadies body and resolve against consuming weariness."),
                    15000, 1f);
                return cord;
            });
        }

        private static bool IsNativeConstitutionTwoBelt(
            BlueprintItemEquipmentBelt belt)
        {
            return belt != null && belt.Cost == 4000 && string.Equals(
                belt.name, "BeltOfConstitution2", StringComparison.Ordinal);
        }
    }
}
