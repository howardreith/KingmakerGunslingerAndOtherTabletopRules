using System;
using System.Globalization;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Registers one inert stackable component item consumed by the player-facing
    /// Wrecked-to-Broken overhaul delivery. It is an isolated Diamond Dust clone so
    /// inventory stacking and icon behavior are known on Kingmaker 2.1.7b.
    /// </summary>
    internal static class FirearmRepairKitBlueprints
    {
        internal const string Symbol = "KMG.Test.FirearmRepairKitItem";
        internal const string InternalName = "KMG_FirearmRepairKit_Item";
        internal const string DisplayName = "Firearm Repair Kit";

        private const string Description =
            "A compact set of replacement springs, pins, tools, and fitted parts. Consuming one lets a trained wielder overhaul a Wrecked Test Musket into an empty Broken firearm; ordinary repair is still required afterward.";
        private const string Flavor =
            "Enough fitted parts for one emergency overhaul, not a complete repair.";
        private const int Cost = 50;
        private const float Weight = 1.0f;

        internal static BlueprintItem Register(
            LibraryScriptableObject library,
            BlueprintRegistry registry,
            ModLogger logger)
        {
            if (library == null)
            {
                throw new ArgumentNullException("library");
            }

            if (registry == null)
            {
                throw new ArgumentNullException("registry");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            BlueprintItem source = BlueprintLibraryLookup.RequireExact<BlueprintItem>(
                library,
                BasicAmmunitionBlueprints.NativeDiamondDustGuid,
                "native Diamond Dust stackable item");
            BlueprintItemAccess access = BlueprintItemAccess.Resolve();
            BlueprintItemSnapshot sourceBefore = access.Capture(source);

            BlueprintItem repairKit = registry.Register<BlueprintItem>(
                Symbol,
                delegate
                {
                    BlueprintItem clone = BlueprintCloneService.Clone(
                        source,
                        InternalName);
                    clone.ComponentsArray = Array.Empty<BlueprintComponent>();
                    access.Configure(
                        clone,
                        LocalizationService.Create(
                            "KMG.Item.FirearmRepairKit.Name",
                            DisplayName),
                        LocalizationService.Create(
                            "KMG.Item.FirearmRepairKit.Description",
                            Description),
                        LocalizationService.Create(
                            "KMG.Item.FirearmRepairKit.Flavor",
                            Flavor),
                        Cost,
                        Weight);
                    return clone;
                });

            Validate(repairKit);
            if (!sourceBefore.Matches(access.Capture(source)))
            {
                throw new InvalidOperationException(
                    "Registering the Firearm Repair Kit mutated the native Diamond Dust blueprint.");
            }

            logger.Info(
                "recovery",
                "repair-kit.ready",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered stackable Firearm Repair Kit guid={0}; cost={1}; weight={2}; sourceGuid={3}.",
                    registry.ResolveGuid(Symbol),
                    repairKit.Cost,
                    repairKit.Weight,
                    BasicAmmunitionBlueprints.NativeDiamondDustGuid));
            return repairKit;
        }

        internal static void Validate(BlueprintItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (!string.Equals(item.name, InternalName, StringComparison.Ordinal) ||
                !string.Equals(item.Name, DisplayName, StringComparison.Ordinal) ||
                !string.Equals(item.Description, Description, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The Firearm Repair Kit has incorrect identity or localization.");
            }

            if (!item.IsActuallyStackable ||
                item.Cost != Cost ||
                !item.Weight.Equals(Weight))
            {
                throw new InvalidOperationException(
                    "The Firearm Repair Kit has incorrect stack, cost, or weight settings.");
            }

            if (item.ComponentsArray == null || item.ComponentsArray.Length != 0)
            {
                throw new InvalidOperationException(
                    "The Firearm Repair Kit must contain no gameplay components.");
            }
        }
    }
}
