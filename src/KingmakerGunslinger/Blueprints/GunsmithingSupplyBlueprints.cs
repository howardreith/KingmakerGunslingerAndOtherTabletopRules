using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class GunsmithingSupplyBlueprintSet
    {
        internal GunsmithingSupplyBlueprintSet(BlueprintItem gunsmithKit,
            BlueprintItem overhaulKit)
        { GunsmithKit = gunsmithKit; OverhaulKit = overhaulKit; }
        internal BlueprintItem GunsmithKit { get; private set; }
        internal BlueprintItem OverhaulKit { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class GunsmithingSupplyBlueprints
    {
        internal const string GunsmithKitSymbol = "KMG.Gunsmithing.GunsmithKit";
        internal const string OverhaulKitSymbol = "KMG.Gunsmithing.OverhaulKit";

        internal static GunsmithingSupplyBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            BlueprintItem source = BlueprintLibraryLookup.RequireExact<BlueprintItem>(
                library, BasicAmmunitionBlueprints.NativeDiamondDustGuid,
                "native inventory supply template");
            BlueprintItemAccess access = BlueprintItemAccess.Resolve();
            BlueprintItem tool = registry.Register<BlueprintItem>(GunsmithKitSymbol, () =>
            {
                BlueprintItem clone = BlueprintCloneService.Clone(source,
                    "KMG_GunsmithKit_Item");
                clone.ComponentsArray = Array.Empty<BlueprintComponent>();
                access.ConfigureNonStackable(clone,
                    LocalizationService.Create("KMG.Item.GunsmithKit.Name", "Gunsmith's Kit"),
                    LocalizationService.Create("KMG.Item.GunsmithKit.Description",
                        "A durable, non-consumable set of firearm-cleaning and ammunition-casting tools required to craft basic firearm ammunition."),
                    LocalizationService.Create("KMG.Item.GunsmithKit.Flavor",
                        "Molds, measures, files, and compact hand tools in a fitted case."),
                    100, 2f);
                return clone;
            });
            BlueprintItem overhaul = registry.Register<BlueprintItem>(OverhaulKitSymbol, () =>
            {
                BlueprintItem clone = BlueprintCloneService.Clone(source,
                    "KMG_OverhaulKit_Item");
                clone.ComponentsArray = Array.Empty<BlueprintComponent>();
                access.Configure(clone,
                    LocalizationService.Create("KMG.Item.OverhaulKit.Name", "Firearm Overhaul Kit"),
                    LocalizationService.Create("KMG.Item.OverhaulKit.Description",
                        "A consumable set of fitted replacement parts used by Overhaul Firearm to restore one Wrecked firearm to Broken condition."),
                    LocalizationService.Create("KMG.Item.OverhaulKit.Flavor",
                        "A complete field replacement set for a badly damaged lock and barrel assembly."),
                    100, 1f);
                return clone;
            });
            if (tool.IsActuallyStackable || !overhaul.IsActuallyStackable ||
                tool.Cost != 100 || overhaul.Cost != 100)
                throw new InvalidOperationException("Gunsmithing supply item contract failed.");
            return new GunsmithingSupplyBlueprintSet(tool, overhaul);
        }
    }
}
