using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Acquisition.BetterVendors;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.CraftMagicItemsCompatibility;
using KingmakerGunslinger.CustomWeapons;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Registers the generic magic weapon variants used by optional merchant
    /// progression and resolves the seven canonical +1 items they extend.
    /// Registration is unconditional and module-independent, like every other
    /// saved item identity, so a variant bought in one session always resolves
    /// on a later load. Registration never publishes stock: no vendor table,
    /// loot table or catalog consumed by ordinary shops receives these items.
    ///
    /// The progression contract checks (native enhancement data, Reliable
    /// cost, and each variant's mechanics, price and presentation) never
    /// throw: a failure is recorded in the catalog status, which disables only
    /// the optional merchant progression. Another mod changing native data can
    /// therefore never take the rest of this mod down, even for players who
    /// do not use Better Vendors.
    /// </summary>
    internal static class ProgressionWeaponBlueprints
    {
        internal static ProgressionWeaponBlueprintCatalog Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            ProductionFirearmBlueprintCatalog firearms,
            MagicFirearmBlueprintCatalog magicFirearms,
            EasternWeaponBlueprintSet eastern,
            ElvenBranchedSpearBlueprintSet spears, ModLogger logger)
        {
            if (library == null || registry == null || firearms == null ||
                magicFirearms == null || magicFirearms.Reliable == null ||
                eastern == null || spears == null || logger == null)
                throw new ArgumentNullException(
                    "Progression weapon registration inputs are incomplete.");
            // The five native enhancement GUIDs are already required by the
            // authored magic firearm catalog; only their stricter progression
            // contract is checked here, and only into the status.
            BlueprintWeaponEnchantment[] enhancements = Enumerable.Range(1,
                    ProgressionWeaponCatalog.MaximumEnhancement)
                .Select(tier => NativeEnhancement(library, tier)).ToArray();
            var failures = new List<string>();
            for (int tier = 1; tier <= enhancements.Length; tier++)
                failures.Add(DescribeNativeEnhancement(enhancements[tier - 1], tier));
            BlueprintWeaponEnchantment reliable = magicFirearms.Reliable;
            Check(failures, "reliable-enchantment", delegate
            {
                Enchantments.ReliableBlueprints.Validate(reliable);
            });
            if (reliable.EnchantmentCost !=
                    ProgressionWeaponCatalog.ReliableEquivalentBonus ||
                reliable.EnchantmentCost !=
                    CraftMagicItemsCompatibilityPolicy.ReliableEquivalentBonus)
                failures.Add("reliable-cost");

            var easternAccess = new EasternWeaponItemAccess();
            var spearAccess = new SpearItemAccess();
            WeaponBlueprintAccess typeAccess = WeaponBlueprintAccess.Resolve();
            var entries = new List<ProgressionWeaponBlueprintEntry>();
            foreach (ProgressionWeaponSpec spec in ProgressionWeaponCatalog.All)
            {
                BlueprintItemWeapon sibling = CanonicalPlusOne(spec,
                    magicFirearms, eastern, spears);
                BlueprintItemWeapon item;
                if (spec.ReusesCanonicalItem)
                    item = sibling;
                else if (spec.IsFirearm)
                    item = RegisterFirearm(registry, spec, firearms,
                        enhancements, reliable, failures);
                else if (spec.Family == ProgressionWeaponFamily.ElvenBranchedSpear)
                    item = RegisterSpear(registry, spec, spears, sibling,
                        enhancements, typeAccess, spearAccess, failures);
                else
                    item = RegisterEastern(registry, spec, eastern, sibling,
                        enhancements, typeAccess, easternAccess, failures);
                entries.Add(new ProgressionWeaponBlueprintEntry(spec, item,
                    sibling, FamilyWeaponType(spec, firearms, eastern, spears)));
            }

            // Intentional family share: every variant keeps the exact item icon
            // its canonical +1 sibling already received from the established
            // ProjectAssetIcons stage (or its native fallback when a family's
            // presentation is disabled). No new art or cache is introduced.
            BlueprintItemAccess items = BlueprintItemAccess.Resolve();
            foreach (ProgressionWeaponBlueprintEntry entry in entries.Where(
                value => !value.Spec.ReusesCanonicalItem))
                items.SetIcon(entry.Item, entry.CanonicalPlusOne.Icon);

            failures.AddRange(DescribeCatalogFailures(entries, enhancements,
                reliable));
            var status = new ProgressionCatalogStatus(failures);
            var catalog = new ProgressionWeaponBlueprintCatalog(
                entries.ToArray(), enhancements, reliable, status);
            string counts = string.Format(CultureInfo.InvariantCulture,
                "entries={0};reused={1};registered={2};firearms={3};melee={4};publishedToVendors=false",
                catalog.Entries.Length,
                catalog.Entries.Count(value => value.Spec.ReusesCanonicalItem),
                catalog.Entries.Count(value => !value.Spec.ReusesCanonicalItem),
                catalog.Entries.Count(value => value.Spec.IsFirearm),
                catalog.Entries.Count(value => !value.Spec.IsFirearm));
            if (status.IsUsable)
                logger.Info("better-vendors", "progression-catalog.ready", counts);
            else
                logger.Warning("better-vendors", "progression-catalog.degraded",
                    counts + ";merchantProgression=disabled;itemsRemainRegistered=true;" +
                    status);
            return catalog;
        }

        /// <summary>
        /// Every progression contract failure of the registered catalog, or
        /// none. Never throws for mismatched game or presentation data.
        /// </summary>
        internal static IEnumerable<string> DescribeCatalogFailures(
            IList<ProgressionWeaponBlueprintEntry> entries,
            BlueprintWeaponEnchantment[] enhancements,
            BlueprintWeaponEnchantment reliable)
        {
            if (entries == null || enhancements == null || reliable == null)
                throw new ArgumentNullException("entries");
            var failures = new List<string>();
            if (entries.Count != ProgressionWeaponCatalog.EntryCount ||
                entries.Select(value => value.Item).Distinct().Count() !=
                    ProgressionWeaponCatalog.EntryCount ||
                entries.Select(value => value.Item.AssetGuid).Distinct(
                    StringComparer.Ordinal).Count() !=
                    ProgressionWeaponCatalog.EntryCount ||
                entries.Count(value => value.Spec.ReusesCanonicalItem) !=
                    ProgressionWeaponCatalog.ReusedEntryCount)
                failures.Add("catalog-identity");
            foreach (ProgressionWeaponBlueprintEntry entry in entries)
            {
                ProgressionWeaponBlueprintEntry current = entry;
                string failure = null;
                Check(failures, current.Spec.Symbol, delegate
                {
                    failure = DescribeEntryFailure(current, enhancements, reliable);
                });
                if (failure != null)
                    failures.Add(current.Spec.Symbol + ":" + failure);
            }
            return failures;
        }

        private static string DescribeEntryFailure(
            ProgressionWeaponBlueprintEntry entry,
            BlueprintWeaponEnchantment[] enhancements,
            BlueprintWeaponEnchantment reliable)
        {
            ProgressionWeaponSpec spec = entry.Spec;
            BlueprintItemWeapon item = entry.Item;
            BlueprintWeaponEnchantment[] actual =
                MagicFirearmBlueprints.ReadEnchantments(item);
            BlueprintWeaponEnchantment[] expected = ExpectedEnchantments(spec,
                enhancements, reliable);
            WeaponEnhancementBonus[] bonuses = actual.SelectMany(value =>
                (value.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<WeaponEnhancementBonus>()).ToArray();
            return
                !string.Equals(item.AssetGuid, spec.Guid, StringComparison.Ordinal)
                    ? "identity" :
                !actual.SequenceEqual(expected) ? "enchantment-package" :
                bonuses.Length != 1 ||
                    bonuses[0].EnhancementBonus != spec.ActualEnhancement ||
                    bonuses[0].Stack ? "actual-enhancement" :
                actual.Sum(value => value.EnchantmentCost) != spec.EquivalentBonus
                    ? "equivalent-bonus" :
                !item.IsMagic || item.IsMasterwork ? "magic-quality" :
                item.Cost != spec.Cost ? "price" :
                !ReferenceEquals(MagicFirearmBlueprints.ReadWeaponType(item),
                    entry.FamilyWeaponType) ? "family-weapon-type" :
                !item.Weight.Equals(entry.CanonicalPlusOne.Weight) ? "weight" :
                item.IsActuallyStackable ? "stackable" :
                item.DamageType == null || item.DamageType.Physical.Material != 0
                    ? "special-material" :
                CraftMagicItemsRegistrationCatalog.IsFirearm(item) !=
                    spec.IsFirearm ? "firearm-definition" :
                string.IsNullOrWhiteSpace(item.Description)
                    ? "description" :
                !string.Equals(item.Name, spec.DisplayName, StringComparison.Ordinal)
                    ? "display-name" :
                item.Icon == null ||
                    !ReferenceEquals(item.Icon, entry.CanonicalPlusOne.Icon)
                    ? "family-icon" :
                item.VisualParameters == null ||
                    entry.CanonicalPlusOne.VisualParameters == null ||
                    !ReferenceEquals(item.VisualParameters.Model,
                        entry.CanonicalPlusOne.VisualParameters.Model)
                    ? "family-visual" : null;
        }

        /// <summary>
        /// Runs one progression contract check and records, rather than
        /// throws, any exception it raises.
        /// </summary>
        private static void Check(List<string> failures, string subject,
            Action check)
        {
            try
            {
                check();
            }
            catch (Exception exception)
            {
                failures.Add(subject + ":" + exception.GetType().Name + ":" +
                    exception.Message);
            }
        }

        internal static BlueprintWeaponEnchantment[] ExpectedEnchantments(
            ProgressionWeaponSpec spec, BlueprintWeaponEnchantment[] enhancements,
            BlueprintWeaponEnchantment reliable)
        {
            BlueprintWeaponEnchantment enhancement =
                enhancements[spec.ActualEnhancement - 1];
            return spec.Reliable ? new[] { enhancement, reliable } :
                new[] { enhancement };
        }

        private static BlueprintItemWeapon RegisterFirearm(
            BlueprintRegistry registry, ProgressionWeaponSpec spec,
            ProductionFirearmBlueprintCatalog firearms,
            BlueprintWeaponEnchantment[] enhancements,
            BlueprintWeaponEnchantment reliable, List<string> failures)
        {
            FirearmKind kind = ProgressionWeaponCatalog.FirearmKindOf(spec.Family);
            ProductionFirearmBlueprintEntry family =
                MagicFirearmBlueprints.RequireFamily(firearms, kind);
            var itemSpec = new MagicFirearmItemSpec(spec.Symbol,
                spec.InternalName, spec.DisplayName, kind, spec.Cost,
                spec.EquivalentBonus, false,
                spec.Reliable ? ProgressionWeaponCatalog.ReliableItemDescription :
                    string.Empty, string.Empty,
                ExpectedEnchantments(spec, enhancements, reliable));
            BlueprintItemWeapon item = MagicFirearmBlueprints.RegisterItem(
                registry, itemSpec, family);
            Check(failures, spec.Symbol, delegate
            {
                MagicFirearmBlueprints.ValidateItem(item, itemSpec, family);
            });
            return item;
        }

        private static BlueprintItemWeapon RegisterEastern(
            BlueprintRegistry registry, ProgressionWeaponSpec spec,
            EasternWeaponBlueprintSet eastern, BlueprintItemWeapon canonical,
            BlueprintWeaponEnchantment[] enhancements,
            WeaponBlueprintAccess typeAccess, EasternWeaponItemAccess access,
            List<string> failures)
        {
            EasternWeaponFamily family =
                ProgressionWeaponCatalog.EasternFamilyOf(spec.Family);
            CustomWeaponCategoryDefinition definition =
                EasternWeaponCatalog.RequireCategory(family);
            BlueprintWeaponType weaponType = eastern.Require(family).WeaponType;
            BlueprintWeaponEnchantment[] enchantments =
                ExpectedEnchantments(spec, enhancements, null);
            // The canonical +1 item is itself the exact native-donor clone of
            // this family; cloning it keeps every inherited field and the
            // adapter below re-applies every configured one.
            BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(
                spec.Symbol, delegate
                {
                    BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                        canonical, spec.InternalName);
                    typeAccess.Set(clone, weaponType);
                    access.Configure(clone, definition, spec.Symbol,
                        spec.DisplayName, spec.Cost, false, enchantments);
                    Assets.EasternWeaponAssetRuntime.ApplyTo(clone, spec.Symbol,
                        family);
                    return clone;
                });
            Check(failures, spec.Symbol, delegate
            {
                access.Validate(item, definition, spec.Symbol, spec.DisplayName,
                    spec.Cost, true, false, spec.ActualEnhancement);
            });
            return item;
        }

        private static BlueprintItemWeapon RegisterSpear(
            BlueprintRegistry registry, ProgressionWeaponSpec spec,
            ElvenBranchedSpearBlueprintSet spears, BlueprintItemWeapon canonical,
            BlueprintWeaponEnchantment[] enhancements,
            WeaponBlueprintAccess typeAccess, SpearItemAccess access,
            List<string> failures)
        {
            BlueprintWeaponEnchantment[] enchantments =
                ExpectedEnchantments(spec, enhancements, null);
            BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(
                spec.Symbol, delegate
                {
                    BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                        canonical, spec.InternalName);
                    typeAccess.Set(clone, spears.WeaponType);
                    access.Configure(clone, spec.Symbol, spec.DisplayName,
                        spec.Cost, false, enchantments);
                    Assets.ElvenBranchedSpearAssetRuntime.ApplyTo(clone,
                        spec.Symbol);
                    return clone;
                });
            Check(failures, spec.Symbol, delegate
            {
                access.Validate(item, spec.DisplayName, spec.Cost, true, false,
                    spec.ActualEnhancement);
            });
            return item;
        }

        private static BlueprintItemWeapon CanonicalPlusOne(
            ProgressionWeaponSpec spec, MagicFirearmBlueprintCatalog magicFirearms,
            EasternWeaponBlueprintSet eastern, ElvenBranchedSpearBlueprintSet spears)
        {
            BlueprintItemWeapon item;
            string expectedSymbol;
            switch (spec.Family)
            {
                case ProgressionWeaponFamily.Pistol:
                    expectedSymbol = MagicFirearmBlueprints.PistolPlus1Symbol;
                    item = magicFirearms.Require(expectedSymbol).Item;
                    break;
                case ProgressionWeaponFamily.Musket:
                    expectedSymbol = MagicFirearmBlueprints.MusketPlus1Symbol;
                    item = magicFirearms.Require(expectedSymbol).Item;
                    break;
                case ProgressionWeaponFamily.Blunderbuss:
                    expectedSymbol = MagicFirearmBlueprints.BlunderbussPlus1Symbol;
                    item = magicFirearms.Require(expectedSymbol).Item;
                    break;
                case ProgressionWeaponFamily.ElvenBranchedSpear:
                    expectedSymbol = ElvenBranchedSpearCatalog.Require(
                        ElvenBranchedSpearItemKind.PlusOne).Symbol;
                    item = spears.Require(ElvenBranchedSpearItemKind.PlusOne).Item;
                    break;
                default:
                    EasternWeaponFamily family =
                        ProgressionWeaponCatalog.EasternFamilyOf(spec.Family);
                    expectedSymbol = EasternWeaponCatalog.RequireGeneric(family,
                        EasternWeaponGenericKind.PlusOne).Symbol;
                    item = eastern.Require(family,
                        EasternWeaponGenericKind.PlusOne).Item;
                    break;
            }
            ProgressionWeaponSpec plusOne = ProgressionWeaponCatalog.All.Single(
                value => value.Family == spec.Family && value.ReusesCanonicalItem);
            if (item == null ||
                !string.Equals(plusOne.Symbol, expectedSymbol,
                    StringComparison.Ordinal) ||
                !string.Equals(item.AssetGuid, plusOne.Guid,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Canonical +1 identity changed for progression family " +
                    spec.Family + ".");
            return item;
        }

        private static BlueprintWeaponType FamilyWeaponType(
            ProgressionWeaponSpec spec,
            ProductionFirearmBlueprintCatalog firearms,
            EasternWeaponBlueprintSet eastern,
            ElvenBranchedSpearBlueprintSet spears)
        {
            if (spec.IsFirearm)
                return MagicFirearmBlueprints.RequireFamily(firearms,
                    ProgressionWeaponCatalog.FirearmKindOf(spec.Family)).WeaponType;
            if (spec.Family == ProgressionWeaponFamily.ElvenBranchedSpear)
                return spears.WeaponType;
            return eastern.Require(ProgressionWeaponCatalog.EasternFamilyOf(
                spec.Family)).WeaponType;
        }

        private static BlueprintWeaponEnchantment NativeEnhancement(
            LibraryScriptableObject library, int tier)
        {
            return BlueprintLibraryLookup
                .RequireExact<BlueprintWeaponEnchantment>(library,
                    BetterVendorsContract.EnhancementGuid(tier),
                    "native +" + tier + " weapon enchantment");
        }

        private static string DescribeNativeEnhancement(
            BlueprintWeaponEnchantment value, int tier)
        {
            return ProgressionWeaponCatalog.DescribeNativeEnhancementMismatch(
                tier, value.name, value.EnchantmentCost,
                (value.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<WeaponEnhancementBonus>()
                    .Select(component => new KeyValuePair<int, bool>(
                        component.EnhancementBonus, component.Stack)));
        }
    }

    internal sealed class ProgressionWeaponBlueprintEntry
    {
        internal ProgressionWeaponBlueprintEntry(ProgressionWeaponSpec spec,
            BlueprintItemWeapon item, BlueprintItemWeapon canonicalPlusOne,
            BlueprintWeaponType familyWeaponType)
        {
            Spec = spec ?? throw new ArgumentNullException("spec");
            Item = item ?? throw new ArgumentNullException("item");
            CanonicalPlusOne = canonicalPlusOne ??
                throw new ArgumentNullException("canonicalPlusOne");
            FamilyWeaponType = familyWeaponType ??
                throw new ArgumentNullException("familyWeaponType");
        }

        internal ProgressionWeaponSpec Spec { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
        internal BlueprintItemWeapon CanonicalPlusOne { get; private set; }
        internal BlueprintWeaponType FamilyWeaponType { get; private set; }
    }

    internal sealed class ProgressionWeaponBlueprintCatalog
    {
        private readonly Dictionary<string, ProgressionWeaponBlueprintEntry>
            _byGuid;

        internal ProgressionWeaponBlueprintCatalog(
            ProgressionWeaponBlueprintEntry[] entries,
            BlueprintWeaponEnchantment[] enhancements,
            BlueprintWeaponEnchantment reliable, ProgressionCatalogStatus status)
        {
            Entries = entries ?? throw new ArgumentNullException("entries");
            Enhancements = enhancements ??
                throw new ArgumentNullException("enhancements");
            Reliable = reliable ?? throw new ArgumentNullException("reliable");
            Status = status ?? throw new ArgumentNullException("status");
            _byGuid = entries.GroupBy(value => value.Spec.Guid,
                    StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(),
                    StringComparer.Ordinal);
        }

        internal ProgressionWeaponBlueprintEntry[] Entries { get; private set; }
        internal BlueprintWeaponEnchantment[] Enhancements { get; private set; }
        internal BlueprintWeaponEnchantment Reliable { get; private set; }

        /// <summary>
        /// Whether every progression contract check passed. Merchant
        /// progression may stock these entries only while it is usable.
        /// </summary>
        internal ProgressionCatalogStatus Status { get; private set; }

        internal ProgressionWeaponSpec[] Specs
        {
            get { return Entries.Select(value => value.Spec).ToArray(); }
        }

        internal bool TryGet(string guid, out ProgressionWeaponBlueprintEntry entry)
        {
            entry = null;
            return guid != null && _byGuid.TryGetValue(guid, out entry);
        }

        internal ProgressionWeaponBlueprintEntry Require(string guid)
        {
            ProgressionWeaponBlueprintEntry entry;
            if (!TryGet(guid, out entry))
                throw new KeyNotFoundException(
                    "No registered progression weapon has GUID " + guid + ".");
            return entry;
        }
    }
}
