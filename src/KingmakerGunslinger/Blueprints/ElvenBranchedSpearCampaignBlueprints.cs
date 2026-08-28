using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElvenBranchedSpear;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ElvenBranchedSpearCampaignBlueprints
    {
        private static readonly VendorSpec[] Vendors =
        {
            new VendorSpec("f720440559fc00949900bfa1575196ac",
                "C11_OlegVendorTable", new[] {
                    ElvenBranchedSpearItemKind.Mundane,
                    ElvenBranchedSpearItemKind.Masterwork,
                    ElvenBranchedSpearItemKind.ColdIron,
                    ElvenBranchedSpearItemKind.MasterworkColdIron }, null),
            new VendorSpec(CapitalVendorBlueprints.TableGuid,
                CapitalVendorBlueprints.ExpectedTableName,
                AllFoundationKinds(), null),
            new VendorSpec("f072a8f6889b5f345b7f4e7c74cb3e4c",
                "DireNarlmarchesVillageVendorTable", AllFoundationKinds(), null),
            new VendorSpec("e5ab1fccf37c55f41a20a80c6ba6a460",
                "PitaxTownVendorTable", AllFoundationKinds(),
                null),
            new VendorSpec(BeneathStolenLandsVendorBlueprints
                    .StandaloneHonestGuyTableGuid,
                BeneathStolenLandsVendorBlueprints.ExpectedNames[0],
                AllFoundationKinds(), null, true, "standalone BTSL"),
            new VendorSpec(BeneathStolenLandsVendorBlueprints
                    .StandaloneXellirenTableGuid,
                BeneathStolenLandsVendorBlueprints.ExpectedNames[1],
                new ElvenBranchedSpearItemKind[0], null, true,
                "standalone BTSL support merchant cleanup"),
            new VendorSpec(BeneathStolenLandsVendorBlueprints
                    .CampaignHonestGuyTableGuid,
                BeneathStolenLandsVendorBlueprints.ExpectedNames[2],
                AllFoundationKinds(), null, true, "campaign Tenebrous Depths"),
            new VendorSpec(BeneathStolenLandsVendorBlueprints
                    .CampaignXellirenTableGuid,
                BeneathStolenLandsVendorBlueprints.ExpectedNames[3],
                new ElvenBranchedSpearItemKind[0], null, true,
                "campaign Tenebrous Depths support merchant cleanup")
        };

        private static readonly LootSpec[] Loot =
        {
            new LootSpec(NamedSpearKind.Boughkeeper,
                "40db074f21260344b95d0e9919c8e682",
                "Forest_PoorLoot01", "CapitalRegionLair01"),
            new LootSpec(NamedSpearKind.Thornstep,
                "3322c56f38031eb4983b6f87c95081b7",
                "Forest_GoodLoot01", "NorthNarlmarchesRegionLair01"),
            new LootSpec(NamedSpearKind.MoonlitFork,
                "2aa7aa5c2df96b143bd2fc62a8547c9c",
                "Forest_TH_GreatclubBarbarianMagic", "MonsterLairHodag"),
            new LootSpec(NamedSpearKind.VipersReach,
                "8a850f7758cb77b498621a307445bb1e",
                "Forest_GoodLoot_withWeaponOrArmor", "LoneCyclopCave"),
            new LootSpec(NamedSpearKind.BriarCrownedSpear,
                "decb6060ab534294eb6d35510e45d317",
                "RichHuman_NotHiddenLockedGood", "BlakemoorHideout"),
            new LootSpec(NamedSpearKind.SpearOfTheFirstBranch,
                "13e98ebc52714d34eb8e53f1099110fd",
                "RichHuman_Loot_5_2lvl", "FinalDungeon2")
        };

        private static readonly CleanupSpec[] CleanupLoot =
        {
            new CleanupSpec("59cb0ac65b4093440ad341b9a2f372cf",
                "Forest_BarrikadedChest1", "StagLordFort"),
            new CleanupSpec("70c4615a8d667dc4cb740c22ee7b5eed",
                "Forest_LootBoxGood2", "GoblinKingFort"),
            new CleanupSpec("193b1222846a0114197e716cb35d3ce8",
                "Forest_cache", "VordakaiTombLevel2"),
            new CleanupSpec("7e6448d1d8a7e4f4d9cc340b8f15e732",
                "RichHuman_Loot_1", "FinalDungeon"),
            new CleanupSpec("99fe8ae070cabca40b25110fc0714b03",
                "Forest_StoneWithTreasure", "BigNarlmarches"),
            new CleanupSpec("1cf548dcd2a49a94d82be1df8efd26ef",
                "Forest_cache_1_515", "LonelyBarrow"),
            new CleanupSpec("2179d0c774e6c034c83529fad2ba785c",
                "RichHuman_Armory_ChestHuge_Outline (3)", "IrovettiPalace"),
            new CleanupSpec("19c1920cf93076249b5c4f29488851f9",
                "Forest_PriestGhost_TreasureStoneLoot", "BigNarlmarches"),
            new CleanupSpec("364711342543d814eb95aa98a4c65e58",
                "Forest_cache_1", "LonelyBarrow"),
            new CleanupSpec("8a07f25d4083eb84c943bf95684f8e16",
                "Forest_Loot01", "CandlemereTower"),
            new CleanupSpec("53d54ca50fccb8c4d9242904eba04d14",
                "Forest_cache_1561", "VordakaiTombLevel2")
        };

        internal static VendorSpec[] VendorSpecs { get { return Vendors.ToArray(); } }
        internal static LootSpec[] LootSpecs { get { return Loot.ToArray(); } }

        internal static ElvenBranchedSpearCampaignPublication Publish(
            LibraryScriptableObject library, ElvenBranchedSpearBlueprintSet foundation,
            ModLogger logger)
        {
            if (library == null || foundation == null || foundation.Named == null ||
                logger == null) throw new ArgumentNullException(
                    "Elven Branched Spear campaign publication inputs are incomplete.");
            BlueprintItem[] owned = foundation.Entries.Select(value =>
                (BlueprintItem)value.Item).Concat(foundation.Named.Entries.Select(value =>
                    (BlueprintItem)value.Item)).ToArray();
            var vendorMutations = new List<SpearVendorMutation>();
            var lootMutations = new List<SpearLootMutation>();
            try
            {
                foreach (VendorSpec spec in Vendors)
                {
                    BlueprintSharedVendorTable table = library.GetAllBlueprints()
                        .OfType<BlueprintSharedVendorTable>().SingleOrDefault(value =>
                            string.Equals(value.AssetGuid, spec.Guid,
                                StringComparison.Ordinal));
                    if (table == null && spec.Optional)
                    {
                        logger.Info("elven-branched-spear",
                            "campaign.vendor-skipped-optional",
                            "SKIPPED_OPTIONAL_TABLE_ABSENT;guid=" + spec.Guid +
                            ";mode=" + spec.Mode);
                        continue;
                    }
                    if (table == null)
                        throw new InvalidOperationException(
                            "Required Elven Branched Spear vendor is absent: " +
                            spec.Guid + ";name=" + spec.Name);
                    if (!string.Equals(table.name, spec.Name, StringComparison.Ordinal))
                        throw new InvalidOperationException("Spear vendor identity mismatch: " +
                            spec.Guid + ";name=" + table.name);
                    BlueprintItem[] desired = spec.FoundationKinds.Select(value =>
                        (BlueprintItem)foundation.Require(value).Item).Concat(
                            spec.NamedKind.HasValue ? new[] { (BlueprintItem)foundation.Named
                                .Require(spec.NamedKind.Value).Item } :
                                new BlueprintItem[0]).ToArray();
                    BlueprintComponent[] before = table.ComponentsArray ??
                        new BlueprintComponent[0];
                    bool exact = desired.All(item => before.OfType<LootItemsPackFixed>()
                        .Count(value => ReferenceEquals(
                            CapitalVendorBlueprints.ReadItem(value), item) &&
                            CapitalVendorBlueprints.ReadCount(value) == 1) == 1) &&
                        !before.OfType<LootItemsPackFixed>().Any(value =>
                            owned.Contains(CapitalVendorBlueprints.ReadItem(value)) &&
                            !desired.Contains(CapitalVendorBlueprints.ReadItem(value)));
                    if (exact)
                    {
                        vendorMutations.Add(SpearVendorMutation.Unchanged(table, before,
                            desired, spec));
                        continue;
                    }
                    BlueprintComponent[] retained = before.Where(value =>
                    {
                        LootItemsPackFixed fixedEntry = value as LootItemsPackFixed;
                        return fixedEntry == null ||
                            !owned.Contains(CapitalVendorBlueprints.ReadItem(fixedEntry));
                    }).ToArray();
                    BlueprintComponent[] additions = desired.Select(item =>
                        (BlueprintComponent)CapitalVendorBlueprints.CreateFixedEntry(item, 1))
                        .ToArray();
                    VendorCatalogPublication<BlueprintComponent> transaction =
                        spec.Optional ? VendorCatalogPublication<BlueprintComponent>
                            .CreateIntegrated(retained, additions,
                                CapitalVendorBlueprints.ReadVendorSortKey) :
                            VendorCatalogPublication<BlueprintComponent>.Create(
                                retained, additions);
                    table.ComponentsArray = transaction.Published;
                    var mutation = new SpearVendorMutation(table, before,
                        transaction.Published, desired, spec, true);
                    mutation.Validate();
                    vendorMutations.Add(mutation);
                }

                foreach (LootSpec spec in Loot)
                {
                    BlueprintLoot target = BlueprintLibraryLookup.RequireExact<BlueprintLoot>(
                        library, spec.Guid, "Elven Branched Spear fixed loot " + spec.Name);
                    if (!string.Equals(target.name, spec.Name, StringComparison.Ordinal) ||
                        target.Area == null || !string.Equals(target.Area.name,
                            spec.AreaName, StringComparison.Ordinal))
                        throw new InvalidOperationException("Spear loot identity/area mismatch: " +
                            spec.Guid + ";name=" + target.name + ";area=" +
                            (target.Area == null ? "<none>" : target.Area.name));
                    BlueprintItem desired = foundation.Named.Require(spec.NamedKind).Item;
                    LootEntry[] before = target.Items ?? new LootEntry[0];
                    bool exact = before.Count(value => value != null &&
                        ReferenceEquals(value.Item, desired) && value.Count == 1) == 1 &&
                        !before.Any(value => value != null && owned.Contains(value.Item) &&
                            !ReferenceEquals(value.Item, desired));
                    if (exact)
                    {
                        lootMutations.Add(SpearLootMutation.Unchanged(target, before,
                            desired, spec));
                        continue;
                    }
                    LootEntry[] retained = before.Where(value => value == null ||
                        !owned.Contains(value.Item)).ToArray();
                    LootEntry[] published = retained.Concat(new[] {
                        new LootEntry { Item = desired, Count = 1 } }).ToArray();
                    target.Items = published;
                    var mutation = new SpearLootMutation(target, before, published,
                        desired, spec, true);
                    mutation.Validate();
                    lootMutations.Add(mutation);
                }
                foreach (CleanupSpec spec in CleanupLoot)
                {
                    BlueprintLoot target = BlueprintLibraryLookup.RequireExact<BlueprintLoot>(
                        library, spec.Guid, "Elven Branched Spear stale loot " + spec.Name);
                    if (!string.Equals(target.name, spec.Name, StringComparison.Ordinal) ||
                        target.Area == null || !string.Equals(target.Area.name,
                            spec.AreaName, StringComparison.Ordinal))
                        throw new InvalidOperationException(
                            "Spear cleanup identity/area mismatch: " + spec.Guid);
                    LootEntry[] before = target.Items ?? new LootEntry[0];
                    LootEntry[] published = before.Where(value => value == null ||
                        !owned.Contains(value.Item)).ToArray();
                    if (published.Length == before.Length) continue;
                    target.Items = published;
                    lootMutations.Add(new SpearLootMutation(target, before,
                        published, null, null, true));
                }
                var result = new ElvenBranchedSpearCampaignPublication(
                    vendorMutations, lootMutations);
                result.Validate();
                logger.Info("elven-branched-spear", "campaign.published",
                    "Normalized generic campaign/BTSL stock and six distinct fixed-loot placements for the named spear progression.");
                return result;
            }
            catch
            {
                for (int index = lootMutations.Count - 1; index >= 0; index--)
                    lootMutations[index].Rollback();
                for (int index = vendorMutations.Count - 1; index >= 0; index--)
                    vendorMutations[index].Rollback();
                throw;
            }
        }

        private static ElvenBranchedSpearItemKind[] AllFoundationKinds()
        { return ElvenBranchedSpearCatalog.All.Select(value => value.Kind).ToArray(); }

        internal sealed class VendorSpec
        {
            internal VendorSpec(string guid, string name,
                ElvenBranchedSpearItemKind[] kinds, NamedSpearKind? namedKind,
                bool optional = false, string mode = "campaign")
            { Guid = guid; Name = name; FoundationKinds = kinds; NamedKind = namedKind;
                Optional = optional; Mode = mode; }
            internal string Guid { get; private set; }
            internal string Name { get; private set; }
            internal ElvenBranchedSpearItemKind[] FoundationKinds { get; private set; }
            internal NamedSpearKind? NamedKind { get; private set; }
            internal bool Optional { get; private set; }
            internal string Mode { get; private set; }
        }

        internal sealed class LootSpec
        {
            internal LootSpec(NamedSpearKind kind, string guid, string name,
                string areaName)
            { NamedKind = kind; Guid = guid; Name = name; AreaName = areaName; }
            internal NamedSpearKind NamedKind { get; private set; }
            internal string Guid { get; private set; }
            internal string Name { get; private set; }
            internal string AreaName { get; private set; }
        }

        private sealed class CleanupSpec
        {
            internal CleanupSpec(string guid, string name, string areaName)
            { Guid = guid; Name = name; AreaName = areaName; }
            internal string Guid { get; private set; }
            internal string Name { get; private set; }
            internal string AreaName { get; private set; }
        }
    }

    internal sealed class ElvenBranchedSpearCampaignPublication
    {
        private readonly List<SpearVendorMutation> _vendors;
        private readonly List<SpearLootMutation> _loot;
        internal ElvenBranchedSpearCampaignPublication(List<SpearVendorMutation> vendors,
            List<SpearLootMutation> loot) { _vendors = vendors; _loot = loot; }
        internal int VendorCount { get { return _vendors.Count; } }
        internal int LootCount { get { return _loot.Count; } }
        internal void Validate()
        {
            int requiredVendors = ElvenBranchedSpearCampaignBlueprints.VendorSpecs
                .Count(value => !value.Optional);
            int maximumVendors = ElvenBranchedSpearCampaignBlueprints.VendorSpecs.Length;
            if (_vendors.Count < requiredVendors || _vendors.Count > maximumVendors ||
                _loot.Count < 6 ||
                _vendors.Select(value => value.Table).Distinct().Count() != _vendors.Count ||
                _loot.Select(value => value.Target).Distinct().Count() !=
                    _loot.Count)
                throw new InvalidOperationException(
                    "Elven Branched Spear campaign publication cardinality mismatch.");
            foreach (SpearVendorMutation value in _vendors) value.Validate();
            foreach (SpearLootMutation value in _loot) value.Validate();
            if (ElvenBranchedSpearCampaignBlueprints.VendorSpecs
                .Where(value => !value.Optional).Any(spec => !_vendors.Any(value =>
                    ReferenceEquals(value.Spec, spec))))
                throw new InvalidOperationException(
                    "A required Elven Branched Spear campaign vendor was not published.");
        }
        internal void Rollback()
        {
            for (int index = _loot.Count - 1; index >= 0; index--)
                _loot[index].Rollback();
            for (int index = _vendors.Count - 1; index >= 0; index--)
                _vendors[index].Rollback();
        }
    }

    internal sealed class SpearVendorMutation
    {
        private readonly BlueprintComponent[] _before;
        private readonly BlueprintComponent[] _published;
        private readonly BlueprintItem[] _items;
        private bool _changed;
        internal SpearVendorMutation(BlueprintSharedVendorTable table,
            BlueprintComponent[] before, BlueprintComponent[] published,
            BlueprintItem[] items, ElvenBranchedSpearCampaignBlueprints.VendorSpec spec,
            bool changed) { Table = table; _before = before; _published = published;
            _items = items; Spec = spec; _changed = changed; }
        internal BlueprintSharedVendorTable Table { get; private set; }
        internal ElvenBranchedSpearCampaignBlueprints.VendorSpec Spec { get; private set; }
        internal static SpearVendorMutation Unchanged(BlueprintSharedVendorTable table,
            BlueprintComponent[] before, BlueprintItem[] items,
            ElvenBranchedSpearCampaignBlueprints.VendorSpec spec)
        { return new SpearVendorMutation(table, before, before, items, spec, false); }
        internal void Validate()
        {
            BlueprintComponent[] current = Table.ComponentsArray ??
                new BlueprintComponent[0];
            if (_items.Any(item => current.OfType<LootItemsPackFixed>().Count(value =>
                ReferenceEquals(CapitalVendorBlueprints.ReadItem(value), item) &&
                CapitalVendorBlueprints.ReadCount(value) == 1) != 1))
                throw new InvalidOperationException("Spear vendor validation failed: " + Spec.Name);
        }
        internal void Rollback()
        {
            if (!_changed) return;
            BlueprintComponent[] current = Table.ComponentsArray ??
                new BlueprintComponent[0];
            if (current.Length != _published.Length || current.Where((value, index) =>
                !ReferenceEquals(value, _published[index])).Any())
                throw new InvalidOperationException(
                    "Spear vendor rollback refused after foreign mutation.");
            Table.ComponentsArray = _before;
            _changed = false;
        }
    }

    internal sealed class SpearLootMutation
    {
        private readonly LootEntry[] _before;
        private readonly LootEntry[] _published;
        private bool _changed;
        internal SpearLootMutation(BlueprintLoot target, LootEntry[] before,
            LootEntry[] published, BlueprintItem item,
            ElvenBranchedSpearCampaignBlueprints.LootSpec spec, bool changed)
        { Target = target; _before = before; _published = published; Item = item;
            Spec = spec; _changed = changed; }
        internal BlueprintLoot Target { get; private set; }
        internal BlueprintItem Item { get; private set; }
        internal ElvenBranchedSpearCampaignBlueprints.LootSpec Spec { get; private set; }
        internal static SpearLootMutation Unchanged(BlueprintLoot target,
            LootEntry[] before, BlueprintItem item,
            ElvenBranchedSpearCampaignBlueprints.LootSpec spec)
        { return new SpearLootMutation(target, before, before, item, spec, false); }
        internal void Validate()
        {
            if (Item == null) return;
            LootEntry[] current = Target.Items ?? new LootEntry[0];
            if (current.Count(value => value != null && ReferenceEquals(value.Item, Item) &&
                value.Count == 1) != 1)
                throw new InvalidOperationException("Spear fixed-loot validation failed: " +
                    Spec.Name);
        }
        internal void Rollback()
        {
            if (!_changed) return;
            LootEntry[] current = Target.Items ?? new LootEntry[0];
            if (current.Length != _published.Length || current.Where((value, index) =>
                !ReferenceEquals(value, _published[index])).Any())
                throw new InvalidOperationException(
                    "Spear fixed-loot rollback refused after foreign mutation.");
            Target.Items = _before;
            _changed = false;
        }
    }
}
