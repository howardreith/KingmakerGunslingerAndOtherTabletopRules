using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;

namespace KingmakerGunslinger.Blueprints
{
    internal static class EasternWeaponCampaignBlueprints
    {
        private static readonly EasternVendorSpec[] Vendors =
        {
            new EasternVendorSpec("f720440559fc00949900bfa1575196ac",
                "C11_OlegVendorTable", EarlyGenericKinds(),
                new EasternWeaponNamedKind[0], false,
                "Act-I mundane and masterwork stock"),
            new EasternVendorSpec(CapitalVendorBlueprints.TableGuid,
                CapitalVendorBlueprints.ExpectedTableName, AllGenericKinds(),
                new EasternWeaponNamedKind[0], false,
                "capital recurring generic stock"),
            new EasternVendorSpec("f072a8f6889b5f345b7f4e7c74cb3e4c",
                "DireNarlmarchesVillageVendorTable", AllGenericKinds(),
                new EasternWeaponNamedKind[0], false,
                "later regional replacement stock"),
            new EasternVendorSpec("e5ab1fccf37c55f41a20a80c6ba6a460",
                "PitaxTownVendorTable", AllGenericKinds(),
                new EasternWeaponNamedKind[0], false,
                "Pitax specialist generic stock"),
            new EasternVendorSpec(
                BeneathStolenLandsVendorBlueprints.StandaloneHonestGuyTableGuid,
                BeneathStolenLandsVendorBlueprints.ExpectedNames[0],
                AllGenericKinds(), new EasternWeaponNamedKind[0], true,
                "standalone BTSL"),
            new EasternVendorSpec(
                BeneathStolenLandsVendorBlueprints.StandaloneXellirenTableGuid,
                BeneathStolenLandsVendorBlueprints.ExpectedNames[1],
                new EasternWeaponGenericKind[0], new EasternWeaponNamedKind[0], true,
                "standalone BTSL support merchant cleanup"),
            new EasternVendorSpec(
                BeneathStolenLandsVendorBlueprints.CampaignHonestGuyTableGuid,
                BeneathStolenLandsVendorBlueprints.ExpectedNames[2],
                AllGenericKinds(), new EasternWeaponNamedKind[0], true,
                "campaign Tenebrous Depths"),
            new EasternVendorSpec(
                BeneathStolenLandsVendorBlueprints.CampaignXellirenTableGuid,
                BeneathStolenLandsVendorBlueprints.ExpectedNames[3],
                new EasternWeaponGenericKind[0], new EasternWeaponNamedKind[0], true,
                "campaign Tenebrous Depths support merchant cleanup")
        };

        private static readonly EasternLootSpec[] Loot =
        {
            new EasternLootSpec("59cb0ac65b4093440ad341b9a2f372cf",
                "Forest_BarrikadedChest1", "StagLordFort", "late Act I",
                new[] { EasternWeaponNamedKind.PaperLantern }),
            new EasternLootSpec("020246502ff864f4aab19e2fc00e63ee",
                "Forest_chest_close", "TrollLair_Exterior", "Act II",
                new[] { EasternWeaponNamedKind.WayfarersOath }),
            new EasternLootSpec("e72cdc1e01c1eb144b6c29084dd111fb",
                "Forest_ChestWithMasterworkWeapons", "StagLordOldCamp",
                "late Act I",
                new[] { EasternWeaponNamedKind.BorderSentinel }),
            new EasternLootSpec("a2d14c56093720947a6ca4978c6a5985",
                "Forest_OldDwarfChest", "TrollLair_SecondLevel",
                "Act II", new[] { EasternWeaponNamedKind.QuietCurrent }),
            new EasternLootSpec("7208dc79fd87ca849babf696e62d4e93",
                "Forest_TrollhoundLairLoot02", "TrollhoundLair",
                "Act II", new[] { EasternWeaponNamedKind.WinterReed }),
            new EasternLootSpec("2bffac36ed3499f4f9a1e6456e96a0f6",
                "Forest_LockedLoot01", "CandlemereTower", "Act II",
                new[] { EasternWeaponNamedKind.CloudCleaver }),
            new EasternLootSpec("df9ac89a7d8533a4e999bd267ae52b65",
                "Forest_UnhiddenLocked01", "SilverstepGrotto_Cave", "Act III",
                new[] { EasternWeaponNamedKind.FallingPetal }),
            new EasternLootSpec("5e302038ce8b06f418a327d4eeadb51d",
                "Forest_loot_box_02", "SilverstepLake_Outdoor",
                "Act III", new[] { EasternWeaponNamedKind.DrawnHorizon }),
            new EasternLootSpec("2d95232e6fc0b594bb6e13e3d3ea0dc3",
                "Forest_Loot01", "Varnhold", "Act IV",
                new[] { EasternWeaponNamedKind.StormOverStone }),
            new EasternLootSpec("a9bb1f714425c564aadee3cc712fb96a",
                "Forest_CyclopLootRoot", "DunswardOutdoor", "Act IV",
                new[] { EasternWeaponNamedKind.FoxfireWhisper }),
            new EasternLootSpec("399410bf927fb3349bad940394fd9abe",
                "Barbarians_LootRoot", "ArmagsTomb", "Act IV",
                new[] { EasternWeaponNamedKind.ThunderAtTheGate }),
            new EasternLootSpec("462bf0e4476e8c7498b2462219d46d25",
                "Hills_chest_closed", "BarbarianMainCamp", "Act IV",
                new[] { EasternWeaponNamedKind.MountainSunder }),
            new EasternLootSpec("c0f1626bb1a0b3b47ad452ce75c7f0e2",
                "RichHuman_GoodLoot_Locked#1", "PitaxTown", "Act V",
                new[] { EasternWeaponNamedKind.EmptySleeve }),
            new EasternLootSpec("b4183a776ad4c0b44acbc04837630a2e",
                "RichHuman_treasure_chest_02", "Brineheart", "Act V",
                new[] { EasternWeaponNamedKind.MoonlitCrossing }),
            new EasternLootSpec("2e5e8c271f5b1ff4ca42dea4f8d8fb37",
                "Plains_good_loot_1", "GlenebonPlains", "Act V",
                new[] { EasternWeaponNamedKind.UnfixedForm }),
            new EasternLootSpec("b3344268950f27f4b840f216959f150e",
                "FirstWorld_GoodLoot_Trapped_1", "CastleOfKnives", "late game",
                new[] { EasternWeaponNamedKind.NightWithoutMoon }),
            new EasternLootSpec("e3703cd9a6de2f24c80c1505e3c9784f",
                "FirstWorld_2ndFloorGoodLoot05",
                "HouseAtTheEdgeOfTime_2ndFloor", "late game",
                new[] { EasternWeaponNamedKind.HeavensMeasure }),
            new EasternLootSpec("7e6448d1d8a7e4f4d9cc340b8f15e732",
                "RichHuman_Loot_1", "FinalDungeon", "late game",
                new[] { EasternWeaponNamedKind.WorldTreeSeverer })
        };

        private static readonly EasternLootSpec[] CleanupLoot =
        {
            new EasternLootSpec("193b1222846a0114197e716cb35d3ce8",
                "Forest_cache", "VordakaiTombLevel2", "Issue 12 cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("892f160d48fdaf64293c504c10c21930",
                "PoorHuman_treasure_chest_04", "StagLordFort", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("31d7c9d0521aa29419337eb668e97911",
                "RichHuman_ST_BackpackBard_U_Any", "CapitalTavern_Indoor", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("0a3251c637b071945a445e2a098c21e7",
                "PoorHuman_treasure_chest_locked", "CapitalSquareVillage", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("364711342543d814eb95aa98a4c65e58",
                "Forest_cache_1", "LonelyBarrow", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("70c4615a8d667dc4cb740c22ee7b5eed",
                "Forest_LootBoxGood2", "GoblinKingFort", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("8a75690f59458e542803eda25f7599c9",
                "Forest_cache_1491", "VordakaiTombLevel1", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("098ebbe376dce16468809a54323178af",
                "Forest_cache_1348", "VordakaiTombLevel1", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("4f3cc1d502366254d9d4aeef485b942a",
                "Forest_cache_1666", "VordakaiTombLevel1", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("32f2f5cdbc812664884b52a59300d569",
                "RichHuman_GoodLoot_Hidden#1", "PitaxTown", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("63820366a0d66b543ba24435bb943bef",
                "RichHuman_GoodLoot_BarrelJewelry", "PitaxTown", "human-review cleanup",
                new EasternWeaponNamedKind[0]),
            new EasternLootSpec("c8b8159fb695be64883b609a7e77e75d",
                "PoorHuman_treasure_chest_03", "StagLordFort",
                "discoverability cleanup", new EasternWeaponNamedKind[0]),
            new EasternLootSpec("6abcbbc0a161aa54380808655de92197",
                "Forest_HiddenRoomChest3", "TrollLair_SecondLevel",
                "discoverability cleanup", new EasternWeaponNamedKind[0]),
            new EasternLootSpec("27b9b282c32996842bde77e360b72107",
                "Forest_HiddenPoor_Box", "ShrineOfLamashtu",
                "discoverability cleanup", new EasternWeaponNamedKind[0]),
            new EasternLootSpec("5b8346d4fc947624e9f8728fe7a12535",
                "Forest_HiddenLocked02", "SilverstepGrotto_Cave",
                "discoverability cleanup", new EasternWeaponNamedKind[0]),
            new EasternLootSpec("040bad335c144784798a580e41b5410f",
                "Forest_Good_GuardedChest", "SilverstepGrotto_FirstWorld",
                "discoverability cleanup", new EasternWeaponNamedKind[0]),
            new EasternLootSpec("8caed33ddd19e9447b852672e4b795f5",
                "Forest_cache", "VordakaiTombLevel1",
                "discoverability cleanup", new EasternWeaponNamedKind[0]),
            new EasternLootSpec("1946bfd560469984788d4523e0d2786a",
                "Barbarians_GoodLootRoot", "ArmagsTomb_Level2",
                "discoverability cleanup", new EasternWeaponNamedKind[0]),
            new EasternLootSpec("3160ffda16f855747ac22738f55a5c67",
                "RichHuman_Box10", "RushlightFestivalCamp",
                "discoverability cleanup", new EasternWeaponNamedKind[0]),
            new EasternLootSpec("db0e9ac023132cf46b49cd034dabf283",
                "RichHuman_GoodLoot_Locked", "PitaxHorde",
                "discoverability cleanup", new EasternWeaponNamedKind[0]),
            new EasternLootSpec("2252283386d5fb84b9e41d0187ed6dbc",
                "FirstWorld_2ndFloorGoodHiddenLockedLoot08",
                "HouseAtTheEdgeOfTime_2ndFloor",
                "discoverability cleanup", new EasternWeaponNamedKind[0])
        };

        internal static EasternVendorSpec[] VendorSpecs
        { get { return Vendors.ToArray(); } }
        internal static EasternLootSpec[] LootSpecs
        { get { return Loot.ToArray(); } }
        internal static int PublicationLootTargetCount
        { get { return Loot.Length + CleanupLoot.Length; } }

        internal static EasternWeaponCampaignPublication Publish(
            LibraryScriptableObject library, EasternWeaponBlueprintSet weapons,
            ModLogger logger)
        {
            if (library == null || weapons == null || weapons.Named == null ||
                logger == null) throw new ArgumentNullException(
                    "Eastern campaign publication inputs are incomplete.");
            BlueprintItem[] owned = weapons.Entries.Select(value =>
                (BlueprintItem)value.Item).Concat(weapons.Named.Entries.Select(
                    value => (BlueprintItem)value.Item)).ToArray();
            var vendorMutations = new List<EasternVendorMutation>();
            var lootMutations = new List<EasternLootMutation>();
            try
            {
                foreach (EasternVendorSpec spec in Vendors)
                {
                    BlueprintSharedVendorTable table = library.GetAllBlueprints()
                        .OfType<BlueprintSharedVendorTable>().SingleOrDefault(value =>
                            string.Equals(value.AssetGuid, spec.Guid,
                                StringComparison.Ordinal));
                    if (table == null && spec.Optional)
                    {
                        logger.Info("eastern-weapons",
                            "campaign.vendor-skipped-optional",
                            "SKIPPED_OPTIONAL_TABLE_ABSENT;guid=" + spec.Guid +
                            ";mode=" + spec.Mode);
                        continue;
                    }
                    if (table == null || !string.Equals(table.name, spec.Name,
                        StringComparison.Ordinal))
                        throw new InvalidOperationException(
                            "Eastern vendor identity mismatch: " + spec.Guid +
                            ";expected=" + spec.Name + ";observed=" +
                            (table == null ? "<absent>" : table.name));
                    BlueprintItem[] desired = ResolveVendorItems(spec, weapons);
                    BlueprintComponent[] before = table.ComponentsArray ??
                        new BlueprintComponent[0];
                    bool exact = IsExactVendor(before, desired, owned);
                    if (exact)
                    {
                        vendorMutations.Add(EasternVendorMutation.Unchanged(
                            table, before, desired, spec));
                        continue;
                    }
                    BlueprintComponent[] retained = before.Where(value =>
                    {
                        LootItemsPackFixed fixedEntry = value as LootItemsPackFixed;
                        return fixedEntry == null || !owned.Contains(
                            CapitalVendorBlueprints.ReadItem(fixedEntry));
                    }).ToArray();
                    BlueprintComponent[] additions = desired.Select(item =>
                        (BlueprintComponent)CapitalVendorBlueprints
                            .CreateFixedEntry(item, 1)).ToArray();
                    VendorCatalogPublication<BlueprintComponent> transaction =
                        spec.IsBtsl ? VendorCatalogPublication<BlueprintComponent>
                            .CreateIntegrated(retained, additions,
                                CapitalVendorBlueprints.ReadVendorSortKey) :
                            VendorCatalogPublication<BlueprintComponent>.Create(
                                retained, additions);
                    table.ComponentsArray = transaction.Published;
                    var mutation = new EasternVendorMutation(table, before,
                        transaction.Published, desired, spec, true);
                    mutation.Validate();
                    vendorMutations.Add(mutation);
                }

                foreach (EasternLootSpec spec in Loot.Concat(CleanupLoot))
                {
                    BlueprintLoot target = BlueprintLibraryLookup
                        .RequireExact<BlueprintLoot>(library, spec.Guid,
                            "Eastern fixed loot " + spec.Name);
                    if (!string.Equals(target.name, spec.Name,
                            StringComparison.Ordinal) || target.Area == null ||
                        !string.Equals(target.Area.name, spec.AreaName,
                            StringComparison.Ordinal))
                        throw new InvalidOperationException(
                            "Eastern loot identity/area mismatch: " + spec.Guid +
                            ";name=" + target.name + ";area=" +
                            (target.Area == null ? "<none>" : target.Area.name));
                    BlueprintItem[] desired = spec.NamedKinds.Select(kind =>
                        (BlueprintItem)weapons.Named.Require(kind).Item).ToArray();
                    LootEntry[] before = target.Items ?? new LootEntry[0];
                    bool exact = IsExactLoot(before, desired, owned);
                    if (exact)
                    {
                        lootMutations.Add(EasternLootMutation.Unchanged(target,
                            before, desired, spec));
                        continue;
                    }
                    LootEntry[] retained = before.Where(value => value == null ||
                        !owned.Contains(value.Item)).ToArray();
                    LootEntry[] published = retained.Concat(desired.Select(item =>
                        new LootEntry { Item = item, Count = 1 })).ToArray();
                    target.Items = published;
                    var mutation = new EasternLootMutation(target, before,
                        published, desired, spec, true);
                    mutation.Validate();
                    lootMutations.Add(mutation);
                }
                var result = new EasternWeaponCampaignPublication(
                    vendorMutations, lootMutations);
                result.Validate();
                weapons.AttachCampaign(result);
                logger.Info("eastern-weapons", "campaign.published",
                    "Published generic campaign/BTSL stock and all eighteen named weapons at distinct fixed campaign targets.");
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

        private static BlueprintItem[] ResolveVendorItems(EasternVendorSpec spec,
            EasternWeaponBlueprintSet weapons)
        {
            return EasternWeaponCatalog.AllGenericItems.Where(value =>
                    spec.GenericKinds.Contains(value.Kind)).Select(value =>
                        (BlueprintItem)weapons.Require(value.Family,
                            value.Kind).Item)
                .Concat(spec.NamedKinds.Select(value =>
                    (BlueprintItem)weapons.Named.Require(value).Item)).ToArray();
        }

        private static bool IsExactVendor(BlueprintComponent[] current,
            BlueprintItem[] desired, BlueprintItem[] owned)
        {
            return desired.All(item => current.OfType<LootItemsPackFixed>()
                    .Count(value => ReferenceEquals(
                        CapitalVendorBlueprints.ReadItem(value), item) &&
                        CapitalVendorBlueprints.ReadCount(value) == 1) == 1) &&
                !current.OfType<LootItemsPackFixed>().Any(value =>
                    owned.Contains(CapitalVendorBlueprints.ReadItem(value)) &&
                    !desired.Contains(CapitalVendorBlueprints.ReadItem(value)));
        }

        private static bool IsExactLoot(LootEntry[] current,
            BlueprintItem[] desired, BlueprintItem[] owned)
        {
            return desired.All(item => current.Count(value => value != null &&
                    ReferenceEquals(value.Item, item) && value.Count == 1) == 1) &&
                !current.Any(value => value != null && owned.Contains(value.Item) &&
                    !desired.Contains(value.Item));
        }

        private static EasternWeaponGenericKind[] EarlyGenericKinds()
        { return new[] { EasternWeaponGenericKind.Mundane,
            EasternWeaponGenericKind.Masterwork }; }

        private static EasternWeaponGenericKind[] AllGenericKinds()
        { return (EasternWeaponGenericKind[])Enum.GetValues(
            typeof(EasternWeaponGenericKind)); }
    }

    internal sealed class EasternVendorSpec
    {
        internal EasternVendorSpec(string guid, string name,
            EasternWeaponGenericKind[] genericKinds,
            EasternWeaponNamedKind[] namedKinds, bool optional, string mode)
        { Guid = guid; Name = name; GenericKinds = genericKinds;
            NamedKinds = namedKinds; Optional = optional; Mode = mode; }
        internal string Guid { get; private set; }
        internal string Name { get; private set; }
        internal EasternWeaponGenericKind[] GenericKinds { get; private set; }
        internal EasternWeaponNamedKind[] NamedKinds { get; private set; }
        internal bool Optional { get; private set; }
        internal string Mode { get; private set; }
        internal bool IsBtsl { get { return Optional; } }
    }

    internal sealed class EasternLootSpec
    {
        internal EasternLootSpec(string guid, string name, string areaName,
            string band, EasternWeaponNamedKind[] namedKinds)
        { Guid = guid; Name = name; AreaName = areaName; Band = band;
            NamedKinds = namedKinds; }
        internal string Guid { get; private set; }
        internal string Name { get; private set; }
        internal string AreaName { get; private set; }
        internal string Band { get; private set; }
        internal EasternWeaponNamedKind[] NamedKinds { get; private set; }
    }

    internal sealed class EasternWeaponCampaignPublication
    {
        private readonly List<EasternVendorMutation> _vendors;
        private readonly List<EasternLootMutation> _loot;
        internal EasternWeaponCampaignPublication(
            List<EasternVendorMutation> vendors,
            List<EasternLootMutation> loot)
        { _vendors = vendors; _loot = loot; }
        internal int VendorCount { get { return _vendors.Count; } }
        internal int LootTargetCount { get { return _loot.Count; } }
        internal int VendorRowCount { get { return _vendors.Sum(value =>
            value.ItemCount); } }
        internal int LootRowCount { get { return _loot.Sum(value =>
            value.ItemCount); } }
        internal int BtslTableCount { get { return _vendors.Count(value =>
            value.Spec.IsBtsl); } }
        internal int BtslRowCount { get { return _vendors.Where(value =>
            value.Spec.IsBtsl).Sum(value => value.ItemCount); } }

        internal void Validate()
        {
            int required = EasternWeaponCampaignBlueprints.VendorSpecs.Count(
                value => !value.Optional);
            int maximum = EasternWeaponCampaignBlueprints.VendorSpecs.Length;
            int expectedLoot = EasternWeaponCampaignBlueprints
                .PublicationLootTargetCount;
            if (_vendors.Count < required || _vendors.Count > maximum ||
                _loot.Count != expectedLoot || LootRowCount != 18 ||
                _vendors.Select(value => value.Table).Distinct().Count() !=
                    _vendors.Count ||
                _loot.Select(value => value.Target).Distinct().Count() !=
                    expectedLoot ||
                _vendors.Any(value => value.Spec.IsBtsl &&
                    value.Spec.NamedKinds.Length != 0) ||
                BtslRowCount != _vendors.Count(value => value.Spec.IsBtsl &&
                    BeneathStolenLandsVendorBlueprints.IsHonestGuyTable(
                        value.Spec.Guid)) * 12)
                throw new InvalidOperationException(
                    "Eastern campaign publication cardinality mismatch.");
            foreach (EasternVendorMutation value in _vendors) value.Validate();
            foreach (EasternLootMutation value in _loot) value.Validate();
            EasternWeaponNamedKind[] placed = _vendors.SelectMany(value =>
                    value.Spec.NamedKinds).Concat(_loot.SelectMany(value =>
                    value.Spec.NamedKinds)).ToArray();
            if (placed.Length != 18 || placed.Distinct().Count() != 18 ||
                Enum.GetValues(typeof(EasternWeaponNamedKind))
                    .Cast<EasternWeaponNamedKind>().Any(value =>
                        placed.Count(kind => kind == value) != 1))
                throw new InvalidOperationException(
                    "Eastern named progression placement is not singular.");
        }

        internal void Rollback()
        {
            for (int index = _loot.Count - 1; index >= 0; index--)
                _loot[index].Rollback();
            for (int index = _vendors.Count - 1; index >= 0; index--)
                _vendors[index].Rollback();
        }
    }

    internal sealed class EasternVendorMutation
    {
        private readonly BlueprintComponent[] _before;
        private readonly BlueprintComponent[] _published;
        private readonly BlueprintItem[] _items;
        private bool _changed;
        internal EasternVendorMutation(BlueprintSharedVendorTable table,
            BlueprintComponent[] before, BlueprintComponent[] published,
            BlueprintItem[] items, EasternVendorSpec spec, bool changed)
        { Table = table; _before = before; _published = published;
            _items = items; Spec = spec; _changed = changed; }
        internal BlueprintSharedVendorTable Table { get; private set; }
        internal EasternVendorSpec Spec { get; private set; }
        internal int ItemCount { get { return _items.Length; } }
        internal static EasternVendorMutation Unchanged(
            BlueprintSharedVendorTable table, BlueprintComponent[] before,
            BlueprintItem[] items, EasternVendorSpec spec)
        { return new EasternVendorMutation(table, before, before, items, spec,
            false); }
        internal void Validate()
        {
            BlueprintComponent[] current = Table.ComponentsArray ??
                new BlueprintComponent[0];
            if (_items.Any(item => current.OfType<LootItemsPackFixed>().Count(
                value => ReferenceEquals(CapitalVendorBlueprints.ReadItem(value),
                    item) && CapitalVendorBlueprints.ReadCount(value) == 1) != 1))
                throw new InvalidOperationException(
                    "Eastern vendor validation failed: " + Spec.Name);
        }
        internal void Rollback()
        {
            if (!_changed) return;
            BlueprintComponent[] current = Table.ComponentsArray ??
                new BlueprintComponent[0];
            if (current.Length != _published.Length || current.Where(
                (value, index) => !ReferenceEquals(value, _published[index])).Any())
                throw new InvalidOperationException(
                    "Eastern vendor rollback refused after foreign mutation.");
            Table.ComponentsArray = _before;
            _changed = false;
        }
    }

    internal sealed class EasternLootMutation
    {
        private readonly LootEntry[] _before;
        private readonly LootEntry[] _published;
        private readonly BlueprintItem[] _items;
        private bool _changed;
        internal EasternLootMutation(BlueprintLoot target, LootEntry[] before,
            LootEntry[] published, BlueprintItem[] items, EasternLootSpec spec,
            bool changed)
        { Target = target; _before = before; _published = published;
            _items = items; Spec = spec; _changed = changed; }
        internal BlueprintLoot Target { get; private set; }
        internal EasternLootSpec Spec { get; private set; }
        internal int ItemCount { get { return _items.Length; } }
        internal static EasternLootMutation Unchanged(BlueprintLoot target,
            LootEntry[] before, BlueprintItem[] items, EasternLootSpec spec)
        { return new EasternLootMutation(target, before, before, items, spec,
            false); }
        internal void Validate()
        {
            LootEntry[] current = Target.Items ?? new LootEntry[0];
            if (_items.Any(item => current.Count(value => value != null &&
                ReferenceEquals(value.Item, item) && value.Count == 1) != 1))
                throw new InvalidOperationException(
                    "Eastern fixed-loot validation failed: " + Spec.Name);
        }
        internal void Rollback()
        {
            if (!_changed) return;
            LootEntry[] current = Target.Items ?? new LootEntry[0];
            if (current.Length != _published.Length || current.Where(
                (value, index) => !ReferenceEquals(value, _published[index])).Any())
                throw new InvalidOperationException(
                    "Eastern fixed-loot rollback refused after foreign mutation.");
            Target.Items = _before;
            _changed = false;
        }
    }
}
