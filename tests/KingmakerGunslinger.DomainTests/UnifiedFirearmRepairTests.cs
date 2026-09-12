using System;
using System.IO;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Regression coverage for the owner-approved unified firearm maintenance
    /// design: one full-round Repair Firearm action restores Broken or Wrecked
    /// firearms directly to Normal with a reusable Gunsmith's Kit, consumes
    /// nothing, preserves surviving loaded ammunition, and the two retired
    /// consumable maintenance kits leave every shop while their blueprint
    /// identities stay registered for save compatibility.
    /// </summary>
    internal static class UnifiedFirearmRepairTests
    {
        internal static void UnifiedRepairAbilityContract()
        {
            string ability = Read("src/KingmakerGunslinger/Blueprints",
                "RepairTestMusketAbilityBlueprints.cs");
            Assertions.True(ability.Contains(
                    "internal const string Symbol = \"KMG.Test.RepairAbility\";") &&
                ability.Contains(
                    "internal const string InternalName = \"KMG_RepairTestMusket_Ability\";") &&
                ability.Contains(
                    "internal const string DisplayName = \"Repair Firearm\";"),
                "The unified Repair Firearm blueprint identity changed.");
            Assertions.True(ability.Contains("SetIsFullRoundAction(true)") &&
                ability.Contains("UnitCommand.CommandType.Standard"),
                "Repair Firearm lost its full-round action economy.");
            Assertions.True(ability.Contains("gunsmithKit.Icon ?? testMusket.Icon") &&
                !ability.Contains("repairKit.Icon"),
                "The repair ability builder must not source its icon from a consumable kit.");
            Assertions.True(ability.Contains("Gunsmith's Kit") &&
                ability.Contains("is preserved"),
                "The repair description must name the reusable tool and ammunition preservation.");
            string wiring = Read("src/KingmakerGunslinger/Bootstrap",
                "BlueprintBootstrap.cs");
            Assertions.True(wiring.Contains(
                    "RepairTestMusketAbilityBlueprints.Register(\n                        registry,\n                        context.Logger,\n                        testMusket.Item,\n                        gunsmithingSupplies.GunsmithKit)") &&
                wiring.Contains(
                    "GunsmithingBlueprints.Register(\n                    registry, repairTestMusketAbility,"),
                "Bootstrap no longer wires the repair ability to the reusable tool and single-ability grant.");
        }

        internal static void UnifiedRepairEffectiveIconPreserved()
        {
            string icons = Read("src/KingmakerGunslinger/Blueprints",
                "ProjectAssetIcons.cs");
            // ProjectAssetIcons.Apply repaints the ability after registration, so
            // the repair-firearm project sprite remains the effective icon even
            // though the required tool changed.
            Assertions.True(icons.Contains(
                    "ApplyFact(reload, visited); ApplyFact(repair, visited);") &&
                icons.Contains(
                    "if (value.Contains(\"repair\")) return \"repair-firearm\";"),
                "The effective Repair Firearm icon override was lost.");
            Assertions.True(icons.Contains("\"repair-firearm\"") &&
                icons.Contains("items.SetIcon(repairKit, Require(\"repair-kit\"));") &&
                icons.Contains(
                    "items.SetIcon(supplies.OverhaulKit, Require(\"overhaul-kit\"));"),
                "Retired kit icons must remain loaded for save-compatibility blueprints.");
            Assertions.True(icons.Contains("supply-icon.retired") &&
                icons.Contains("CountPublishedRows") &&
                icons.Contains("RequireTableAbsent") &&
                !icons.Contains("retired.ContainsExact"),
                "Supply-icon validation no longer proves retired kits have zero actual rows in every vendor table.");
        }

        internal static void UnifiedGunsmithingGrantContract()
        {
            string feature = Read("src/KingmakerGunslinger/Blueprints",
                "GunsmithingBlueprints.cs");
            Assertions.True(feature.Contains("grants[0].Facts.Length != 3") &&
                feature.Contains(
                    "!ReferenceEquals(grants[0].Facts[0], repairAbility)") &&
                !feature.Contains("overhaulAbility"),
                "Gunsmithing must grant exactly one maintenance ability.");
            Assertions.True(
                feature.Contains("repair a Broken firearm to Normal") &&
                feature.Contains("outside combat") &&
                feature.Contains("completed full rest") &&
                feature.Contains("reusable Gunsmith's Kit"),
                "The Gunsmithing feature text no longer describes out-of-combat Broken-only repair plus full-rest restoration.");
            string crafting = Read("src/KingmakerGunslinger/Blueprints",
                "GunsmithingCraftingBlueprints.cs");
            Assertions.True(crafting.Contains("GoldCost != 22") &&
                crafting.Contains("GoldCost != 24") &&
                crafting.Contains("KMG.Gunsmithing.CraftedThisRest"),
                "Crafting ability costs or the shared once-per-rest entitlement changed.");
        }

        internal static void UnifiedLegacyOverhaulAliasContract()
        {
            string alias = Read("src/KingmakerGunslinger/Blueprints",
                "OverhaulTestMusketAbilityBlueprints.cs");
            Assertions.True(alias.Contains(
                    "internal const string Symbol = \"KMG.Test.OverhaulAbility\";") &&
                alias.Contains(
                    "internal const string InternalName = \"KMG_OverhaulTestMusket_Ability\";"),
                "The legacy Overhaul blueprint identity changed.");
            Assertions.True(alias.Contains("result.Hidden = true;") &&
                alias.Contains("result.ActionBarAutoFillIgnored = true;") &&
                alias.Contains("RepairTestMusketAbilityLogic.Create(") &&
                !alias.Contains("OverhaulTestMusketAbilityLogic"),
                "The legacy Overhaul ability is not a hidden delegate of unified repair.");
            Assertions.True(alias.Contains("gunsmithKit") &&
                !alias.Contains("repairKit"),
                "The legacy alias must require the reusable tool, never a consumable kit.");
        }

        internal static void UnifiedVendorRetirementContract()
        {
            string capital = Read("src/KingmakerGunslinger/Blueprints",
                "CapitalVendorBlueprints.cs");
            string capitalOffered = Slice(capital,
                "BlueprintItem[] gunslingerItems =",
                "int[] gunslingerCounts =");
            Assertions.False(capitalOffered.Contains("repairKit") ||
                capitalOffered.Contains("OverhaulKit"),
                "Capital vendor stock still offers a retired consumable kit.");
            Assertions.True(capitalOffered.Contains("gunsmithingSupplies.GunsmithKit"),
                "Capital vendor stock lost the reusable Gunsmith's Kit.");
            AssertOwnedRetainsKits(Slice(capital,
                "Concat(new BlueprintItem[] {", "}).Distinct()"),
                "capital");

            string bokken = Read("src/KingmakerGunslinger/Blueprints",
                "BokkenFirearmSupplyVendorBlueprints.cs");
            string bokkenOffered = Slice(bokken,
                "BlueprintItem[] stocked =",
                "BlueprintItem[] items =");
            Assertions.False(bokkenOffered.Contains("repairKit") ||
                bokkenOffered.Contains("OverhaulKit"),
                "Bokken vendor stock still offers a retired consumable kit.");
            Assertions.True(bokkenOffered.Contains("supplies.GunsmithKit"),
                "Bokken vendor stock lost the reusable Gunsmith's Kit.");
            AssertOwnedRetainsKits(Slice(bokken,
                "BlueprintItem[] owned =", "BlueprintItem[] stocked ="),
                "Bokken");

            string btsl = Read("src/KingmakerGunslinger/Blueprints",
                "BeneathStolenLandsVendorBlueprints.cs");
            string btslOffered = Slice(btsl,
                "BlueprintItem[] support =",
                "int[] supportCounts =");
            Assertions.False(btslOffered.Contains("repairKit") ||
                btslOffered.Contains("OverhaulKit"),
                "Beneath the Stolen Lands vendor stock still offers a retired kit.");
            Assertions.True(btslOffered.Contains("supplies.GunsmithKit"),
                "Beneath the Stolen Lands vendor stock lost the Gunsmith's Kit.");
            AssertOwnedRetainsKits(Slice(btsl,
                "Concat(new BlueprintItem[] {", "}).Distinct()"),
                "BTSL");

            string oleg = Read("src/KingmakerGunslinger/Blueprints",
                "OlegFirearmSupplyCleanupBlueprints.cs");
            AssertOwnedRetainsKits(Slice(oleg,
                "internal static BlueprintItem[] Owned(", "        }"),
                "Oleg cleanup");
        }

        internal static void UnifiedManifestCompatibilityContract()
        {
            string manifest = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json"));
            // All maintenance identities keep their exact symbols and stay active
            // for save compatibility.
            foreach (string symbol in new[] {
                "KMG.Test.RepairAbility", "KMG.Test.OverhaulAbility",
                "KMG.Test.FirearmRepairKitItem", "KMG.Gunsmithing.GunsmithKit",
                "KMG.Gunsmithing.OverhaulKit" })
            {
                int index = manifest.IndexOf("\"symbol\": \"" + symbol + "\"",
                    StringComparison.Ordinal);
                Assertions.True(index >= 0,
                    "Manifest lost the save-compatibility identity " + symbol + ".");
                if (index < 0) continue;
                string window = manifest.Substring(index, Math.Min(400,
                    manifest.Length - index));
                Assertions.True(window.Contains("\"status\": \"active\""),
                    "Manifest demoted the still-registered identity " + symbol + ".");
            }

            Assertions.True(manifest.Contains("outside combat") &&
                manifest.Contains("completed full rest"),
                "Manifest notes do not describe out-of-combat Broken-only repair with rest-only Wrecked recovery.");
            Assertions.True(manifest.Contains("hidden legacy") ||
                manifest.Contains("legacy alias"),
                "Manifest notes do not describe the hidden legacy Overhaul alias.");
        }

        internal static void UnifiedKitItemTextContract()
        {
            string supply = Read("src/KingmakerGunslinger/Blueprints",
                "GunsmithingSupplyBlueprints.cs");
            string kit = Read("src/KingmakerGunslinger/Blueprints",
                "FirearmRepairKitBlueprints.cs");
            Assertions.True(supply.Contains("Reusable tool") &&
                supply.Contains("repair a Broken firearm") &&
                supply.Contains("It is never consumed.") &&
                supply.Contains("completed full rest"),
                "The Gunsmith's Kit description no longer documents repair reuse and rest-only Wrecked recovery.");
            Assertions.True(supply.Contains("Obsolete: no longer used or sold") &&
                kit.Contains("Obsolete: no longer used or sold"),
                "A retired consumable kit is not clearly marked obsolete.");
            Assertions.True(kit.Contains("BasicAmmunitionBlueprints.NativeDiamondDustGuid"),
                "The obsolete kit lost its native template isolation.");
        }

        internal static void UnifiedVendorStockCleanupContract()
        {
            string cleanup = Read("src/KingmakerGunslinger/Acquisition",
                "RetiredKitVendorStockCleanup.cs");
            Assertions.True(cleanup.Contains(
                    "[HarmonyPatch(typeof(VendorLogic), \"BeginTrading\"") &&
                cleanup.Contains("ReferenceEquals(inventory, player.Inventory)") &&
                cleanup.Contains("retired.Contains(item.Blueprint)"),
                "The retired-kit sweep must hook trade-open, match exact retired blueprints, and guard the player inventory.");
            string policy = Read("src/KingmakerGunslinger/Acquisition",
                "RetiredVendorStockPolicy.cs");
            Assertions.True(policy.Contains("isSharedPlayerInventory") &&
                policy.Contains("Array.Empty<int>()"),
                "The sweep policy must select nothing from the shared player inventory.");
            string capital = Read("src/KingmakerGunslinger/Blueprints",
                "CapitalVendorBlueprints.cs");
            Assertions.True(capital.Contains(
                    "internal int CountPublishedRows(BlueprintItem item)") &&
                capital.Contains(
                    "independent of the intended offered-stock list"),
                "The capital publication must expose a direct table-row count for absence checks.");
        }

        private static void AssertOwnedRetainsKits(string ownedWindow, string vendor)
        {
            Assertions.True(ownedWindow.Contains("repairKit"),
                "The " + vendor + " cleanup-owned set lost the retired Firearm Repair Kit.");
            Assertions.True(ownedWindow.Contains("OverhaulKit"),
                "The " + vendor + " cleanup-owned set lost the retired Firearm Overhaul Kit.");
        }

        private static string Slice(string source, string startToken, string endToken)
        {
            int start = source.IndexOf(startToken, StringComparison.Ordinal);
            Assertions.True(start >= 0, "Vendor source lacks token: " + startToken);
            int end = source.IndexOf(endToken, start + startToken.Length,
                StringComparison.Ordinal);
            Assertions.True(end > start, "Vendor source lacks end token: " + endToken);
            return source.Substring(start, end - start);
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
