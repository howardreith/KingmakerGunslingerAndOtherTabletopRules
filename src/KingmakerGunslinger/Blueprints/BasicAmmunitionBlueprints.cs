using System;
using System.Globalization;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Registers three inert, stackable inventory ammunition items.
    /// Both are isolated clones of Kingmaker's mundane Diamond Dust BlueprintItem so they
    /// inherit known inventory/icon behavior without retaining native gameplay components.
    /// </summary>
    internal static class BasicAmmunitionBlueprints
    {
        internal const string BlackPowderSymbol = "KMG.Test.BlackPowderItem";
        internal const string LeadBallSymbol = "KMG.Test.LeadBulletItem";
        internal const string PaperCartridgeSymbol = "KMG.Ammunition.PaperCartridge";

        internal const string NativeDiamondDustGuid =
            "92752bbbf04dfa1439af186f48aee0e9";

        internal const string BlackPowderInternalName =
            "KMG_BlackPowderCharge_Item";
        internal const string LeadBallInternalName =
            "KMG_LeadBall_Item";
        internal const string PaperCartridgeInternalName =
            "KMG_PaperCartridge_Item";

        internal const string BlackPowderDisplayName = "Black Powder Charge";
        internal const string LeadBallDisplayName = "Lead Ball";
        internal const string PaperCartridgeDisplayName = "Paper Cartridge";

        private const string BlackPowderDescription =
            "A measured charge of black powder used with a projectile to load an early firearm.";
        private const string LeadBallDescription =
            "A cast lead projectile sized for an early firearm. Loading one also requires a black powder charge.";
        internal const string PaperCartridgeDescription =
            "A prepared paper or cloth bundle of black powder with a bullet or pellets. It replaces loose powder and shot, reduces reload time by one step, and increases misfire by 1 for that loaded shot. Compatible with early pistols, muskets, and blunderbusses.";
        private const string BlackPowderFlavor =
            "Keep dry, sealed, and well away from sparks.";
        private const string LeadBallFlavor =
            "Simple ammunition, cast to fit a firearm's bore.";
        private const string PaperCartridgeFlavor =
            "A measured charge and projectile wrapped together for a faster load.";

        private const int BlackPowderCost = 10;
        private const int LeadBallCost = 1;
        internal const int PaperCartridgeCost = 12;
        private const float ComponentWeight = 0.1f;
        internal const float PaperCartridgeWeight = 0f;

        internal static BasicAmmunitionBlueprintSet Register(
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
                NativeDiamondDustGuid,
                "native Diamond Dust stackable item");
            BlueprintItemAccess access = BlueprintItemAccess.Resolve();
            BlueprintItemSnapshot sourceBefore = access.Capture(source);

            BlueprintItem blackPowder = registry.Register<BlueprintItem>(
                BlackPowderSymbol,
                delegate
                {
                    BlueprintItem clone = BlueprintCloneService.Clone(
                        source,
                        BlackPowderInternalName);
                    ConfigureBlackPowder(clone, access);
                    return clone;
                });

            BlueprintItem leadBall = registry.Register<BlueprintItem>(
                LeadBallSymbol,
                delegate
                {
                    BlueprintItem clone = BlueprintCloneService.Clone(
                        source,
                        LeadBallInternalName);
                    ConfigureLeadBall(clone, access);
                    return clone;
                });

            BlueprintItem paperCartridge = registry.Register<BlueprintItem>(
                PaperCartridgeSymbol,
                delegate
                {
                    BlueprintItem clone = BlueprintCloneService.Clone(
                        source,
                        PaperCartridgeInternalName);
                    ConfigurePaperCartridge(clone, access);
                    return clone;
                });

            var result = new BasicAmmunitionBlueprintSet(
                source,
                blackPowder,
                leadBall,
                paperCartridge);
            Validate(result, sourceBefore, access);

            logger.Info(
                "ammunition",
                "basic-items.ready",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered stackable Black Powder Charge guid={0}, Lead Ball guid={1}, and Paper Cartridge guid={2} from isolated source guid={3}; costs={4}/{5}/{6}; weights={7}/{8}/{9}.",
                    registry.ResolveGuid(BlackPowderSymbol),
                    registry.ResolveGuid(LeadBallSymbol),
                    registry.ResolveGuid(PaperCartridgeSymbol),
                    NativeDiamondDustGuid,
                    blackPowder.Cost,
                    leadBall.Cost,
                    paperCartridge.Cost,
                    blackPowder.Weight,
                    leadBall.Weight,
                    paperCartridge.Weight));
            return result;
        }

        internal static void Validate(
            BasicAmmunitionBlueprintSet set,
            BlueprintItemAccess access)
        {
            if (set == null)
            {
                throw new ArgumentNullException("set");
            }

            if (access == null)
            {
                throw new ArgumentNullException("access");
            }

            Validate(set, null, access);
        }

        private static void ConfigureBlackPowder(
            BlueprintItem item,
            BlueprintItemAccess access)
        {
            item.ComponentsArray = Array.Empty<BlueprintComponent>();
            access.Configure(
                item,
                LocalizationService.Create(
                    "KMG.Item.BlackPowderCharge.Name",
                    BlackPowderDisplayName),
                LocalizationService.Create(
                    "KMG.Item.BlackPowderCharge.Description",
                    BlackPowderDescription),
                LocalizationService.Create(
                    "KMG.Item.BlackPowderCharge.Flavor",
                    BlackPowderFlavor),
                BlackPowderCost,
                ComponentWeight);
        }

        private static void ConfigureLeadBall(
            BlueprintItem item,
            BlueprintItemAccess access)
        {
            item.ComponentsArray = Array.Empty<BlueprintComponent>();
            access.Configure(
                item,
                LocalizationService.Create(
                    "KMG.Item.LeadBall.Name",
                    LeadBallDisplayName),
                LocalizationService.Create(
                    "KMG.Item.LeadBall.Description",
                    LeadBallDescription),
                LocalizationService.Create(
                    "KMG.Item.LeadBall.Flavor",
                    LeadBallFlavor),
                LeadBallCost,
                ComponentWeight);
        }

        private static void ConfigurePaperCartridge(
            BlueprintItem item,
            BlueprintItemAccess access)
        {
            item.ComponentsArray = Array.Empty<BlueprintComponent>();
            access.Configure(
                item,
                LocalizationService.Create("KMG.Item.PaperCartridge.Name",
                    PaperCartridgeDisplayName),
                LocalizationService.Create("KMG.Item.PaperCartridge.Description",
                    PaperCartridgeDescription),
                LocalizationService.Create("KMG.Item.PaperCartridge.Flavor",
                    PaperCartridgeFlavor),
                PaperCartridgeCost,
                PaperCartridgeWeight);
        }

        private static void Validate(
            BasicAmmunitionBlueprintSet set,
            BlueprintItemSnapshot sourceBefore,
            BlueprintItemAccess access)
        {
            if (set.Source == null || set.BlackPowder == null || set.LeadBall == null ||
                set.PaperCartridge == null)
            {
                throw new InvalidOperationException(
                    "The basic-ammunition blueprint set is incomplete.");
            }

            if (ReferenceEquals(set.Source, set.BlackPowder) ||
                ReferenceEquals(set.Source, set.LeadBall) ||
                ReferenceEquals(set.Source, set.PaperCartridge) ||
                ReferenceEquals(set.BlackPowder, set.LeadBall) ||
                ReferenceEquals(set.BlackPowder, set.PaperCartridge) ||
                ReferenceEquals(set.LeadBall, set.PaperCartridge))
            {
                throw new InvalidOperationException(
                    "Basic ammunition must use three distinct blueprint instances.");
            }

            ValidateOne(
                set.BlackPowder,
                BlackPowderInternalName,
                BlackPowderDisplayName,
                BlackPowderDescription,
                BlackPowderCost,
                ComponentWeight);
            ValidateOne(
                set.LeadBall,
                LeadBallInternalName,
                LeadBallDisplayName,
                LeadBallDescription,
                LeadBallCost,
                ComponentWeight);
            ValidateOne(
                set.PaperCartridge,
                PaperCartridgeInternalName,
                PaperCartridgeDisplayName,
                PaperCartridgeDescription,
                PaperCartridgeCost,
                PaperCartridgeWeight);

            if (sourceBefore != null && !sourceBefore.Matches(access.Capture(set.Source)))
            {
                throw new InvalidOperationException(
                    "Registering basic ammunition mutated the native Diamond Dust blueprint.");
            }
        }

        private static void ValidateOne(
            BlueprintItem item,
            string internalName,
            string displayName,
            string description,
            int cost,
            float weight)
        {
            if (!string.Equals(item.name, internalName, StringComparison.Ordinal) ||
                !string.Equals(item.Name, displayName, StringComparison.Ordinal) ||
                !string.Equals(item.Description, description, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A basic-ammunition item has incorrect identity or localization.");
            }

            if (!item.IsActuallyStackable ||
                item.Cost != cost ||
                !item.Weight.Equals(weight))
            {
                throw new InvalidOperationException(
                    "A basic-ammunition item has incorrect stack, cost, or weight settings.");
            }

            if (item.ComponentsArray == null || item.ComponentsArray.Length != 0)
            {
                throw new InvalidOperationException(
                    "Basic-ammunition items must contain no gameplay components.");
            }
        }
    }

    internal sealed class BasicAmmunitionBlueprintSet
    {
        internal BasicAmmunitionBlueprintSet(
            BlueprintItem source,
            BlueprintItem blackPowder,
            BlueprintItem leadBall,
            BlueprintItem paperCartridge)
        {
            Source = source ?? throw new ArgumentNullException("source");
            BlackPowder = blackPowder ?? throw new ArgumentNullException("blackPowder");
            LeadBall = leadBall ?? throw new ArgumentNullException("leadBall");
            PaperCartridge = paperCartridge ?? throw new ArgumentNullException("paperCartridge");
        }

        internal BlueprintItem Source { get; private set; }

        internal BlueprintItem BlackPowder { get; private set; }

        internal BlueprintItem LeadBall { get; private set; }

        internal BlueprintItem PaperCartridge { get; private set; }

        internal int Count
        {
            get { return 3; }
        }
    }
}
