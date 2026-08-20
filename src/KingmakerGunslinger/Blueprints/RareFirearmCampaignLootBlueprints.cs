using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Blueprints
{
    internal static class RareFirearmCampaignLootBlueprints
    {
        private static readonly TargetSpec[] Targets =
        {
            new TargetSpec(MagicFirearmBlueprints.DuelistsRebuttalSymbol,
                "1f0bef6b8e540d644962171dc8810459", "Forest_Container_7_good",
                "VarnholdStockade"),
            new TargetSpec(MagicFirearmBlueprints.RiverKingsMeasureSymbol,
                "b34367a637010f743815aed5875152bd",
                "PoorHuman_IrovettiChambers_ChestHuge_Outline (3)",
                "IrovettiPalace"),
            new TargetSpec(MagicFirearmBlueprints.IrovettisOvationSymbol,
                "aeba7802ade083841935daf88d4652d3",
                "RichHuman_GoodLoot", "IrovettiPalaceFW"),
            new TargetSpec(MagicFirearmBlueprints.TheLastWordSymbol,
                "3bc451b100283774a9e23699dd869f1a",
                "FirstWorld_GoodLoot_Locked_2", "CastleOfKnives"),
            new TargetSpec(MagicFirearmBlueprints.WatchAtWorldsEndSymbol,
                "5a9b9e4b884ae064fa7caa5a13eab065",
                "FirstWorld_VeryGoodHiddenLoot02", "HouseAtTheEdgeOfTime")
        };

        private static readonly CleanupSpec[] CleanupTargets =
        {
            new CleanupSpec("193b1222846a0114197e716cb35d3ce8",
                "Forest_cache", "VordakaiTombLevel2"),
            new CleanupSpec("485300a2036a763499aa77ebac1f83c6",
                "Forest_PoorLoot_PuzzleItem3_Instrument", "IrovettiPalace"),
            new CleanupSpec("36d315a81b36980438e2ef1a866791d1",
                "FirstWorld_BasementGoodLoot01", "HouseAtTheEdgeOfTime_Basement")
        };

        internal static TargetSpec[] TargetSpecs
        { get { return Targets.ToArray(); } }

        internal static RareFirearmCampaignLootPublication Publish(
            LibraryScriptableObject library, MagicFirearmBlueprintCatalog catalog,
            ModLogger logger)
        {
            if (library == null || catalog == null || logger == null)
                throw new ArgumentNullException("Campaign loot publication inputs are incomplete.");
            BlueprintItem[] owned = catalog.Entries.Where(value =>
                value.Spec.Symbol != MagicFirearmBlueprints.PistolPlus1Symbol &&
                value.Spec.Symbol != MagicFirearmBlueprints.MusketPlus1Symbol &&
                value.Spec.Symbol != MagicFirearmBlueprints.BlunderbussPlus1Symbol)
                .Select(value => (BlueprintItem)value.Item).ToArray();
            var mutations = new List<RareFirearmLootMutation>();
            var cleanupMutations = new List<RareFirearmLootCleanupMutation>();
            try
            {
                foreach (TargetSpec spec in Targets)
                {
                    BlueprintLoot target = BlueprintLibraryLookup.RequireExact<BlueprintLoot>(
                        library, spec.Guid, "fixed rare-firearm loot target " + spec.Name);
                    if (!string.Equals(target.name, spec.Name, StringComparison.Ordinal) ||
                        target.Area == null || !string.Equals(target.Area.name,
                            spec.AreaName, StringComparison.Ordinal))
                        throw new InvalidOperationException("Rare firearm target identity/area mismatch: " +
                            spec.Guid + ";name=" + target.name + ";area=" +
                            (target.Area == null ? "<none>" : target.Area.name));
                    BlueprintItem desired = catalog.Require(spec.ItemSymbol).Item;
                    LootEntry[] before = target.Items ?? new LootEntry[0];
                    LootEntry[] desiredMatches = before.Where(value => value != null &&
                        ReferenceEquals(value.Item, desired)).ToArray();
                    bool foreignOwned = before.Any(value => value != null &&
                        owned.Contains(value.Item) && !ReferenceEquals(value.Item, desired));
                    if (desiredMatches.Length == 1 && desiredMatches[0].Count == 1 &&
                        !foreignOwned)
                    {
                        mutations.Add(RareFirearmLootMutation.Unchanged(target, before,
                            desired, spec));
                        continue;
                    }
                    LootEntry[] retained = before.Where(value => value == null ||
                        !owned.Contains(value.Item)).ToArray();
                    var addition = new LootEntry { Item = desired, Count = 1 };
                    LootEntry[] published = retained.Concat(new[] { addition }).ToArray();
                    target.Items = published;
                    var mutation = new RareFirearmLootMutation(target, before, published,
                        desired, spec, true);
                    mutation.Validate();
                    mutations.Add(mutation);
                }
                foreach (CleanupSpec spec in CleanupTargets)
                {
                    BlueprintLoot target = BlueprintLibraryLookup.RequireExact<BlueprintLoot>(
                        library, spec.Guid, "retired rare-firearm loot target " + spec.Name);
                    if (!string.Equals(target.name, spec.Name, StringComparison.Ordinal) ||
                        target.Area == null || !string.Equals(target.Area.name,
                            spec.AreaName, StringComparison.Ordinal))
                        throw new InvalidOperationException("Retired rare firearm target identity/area mismatch: " +
                            spec.Guid + ";name=" + target.name + ";area=" +
                            (target.Area == null ? "<none>" : target.Area.name));
                    LootEntry[] before = target.Items ?? new LootEntry[0];
                    LootEntry[] published = before.Where(value => value == null ||
                        !owned.Contains(value.Item)).ToArray();
                    var cleanup = new RareFirearmLootCleanupMutation(target, before,
                        published, owned, !before.SequenceEqual(published));
                    if (!before.SequenceEqual(published)) target.Items = published;
                    cleanup.Validate();
                    cleanupMutations.Add(cleanup);
                }
                var result = new RareFirearmCampaignLootPublication(mutations,
                    cleanupMutations);
                result.Validate();
                logger.Info("acquisition", "rare-firearm-loot.published",
                    "Published five exact count-one named firearms to five distinct fixed base-campaign BlueprintLoot targets.");
                return result;
            }
            catch
            {
                for (int index = cleanupMutations.Count - 1; index >= 0; index--)
                    cleanupMutations[index].Rollback();
                for (int index = mutations.Count - 1; index >= 0; index--)
                    mutations[index].Rollback();
                throw;
            }
        }

        internal sealed class TargetSpec
        {
            internal TargetSpec(string itemSymbol, string guid, string name,
                string areaName)
            { ItemSymbol = itemSymbol; Guid = guid; Name = name; AreaName = areaName; }
            internal string ItemSymbol { get; private set; }
            internal string Guid { get; private set; }
            internal string Name { get; private set; }
            internal string AreaName { get; private set; }
        }

        internal sealed class CleanupSpec
        {
            internal CleanupSpec(string guid, string name, string areaName)
            { Guid = guid; Name = name; AreaName = areaName; }
            internal string Guid { get; private set; }
            internal string Name { get; private set; }
            internal string AreaName { get; private set; }
        }
    }

    internal sealed class RareFirearmCampaignLootPublication
    {
        private readonly List<RareFirearmLootMutation> _mutations;
        private readonly List<RareFirearmLootCleanupMutation> _cleanupMutations;
        internal RareFirearmCampaignLootPublication(List<RareFirearmLootMutation> mutations,
            List<RareFirearmLootCleanupMutation> cleanupMutations)
        { _mutations = mutations; _cleanupMutations = cleanupMutations; }
        internal int Count { get { return _mutations.Count; } }
        internal void Validate()
        {
            if (_mutations.Count != 5 || _mutations.Select(value => value.Target)
                .Distinct().Count() != 5 || _mutations.Select(value => value.Item)
                .Distinct().Count() != 5)
                throw new InvalidOperationException("Rare firearm loot publication count/identity mismatch.");
            foreach (RareFirearmLootMutation mutation in _mutations) mutation.Validate();
            if (_cleanupMutations.Count != 3)
                throw new InvalidOperationException("Rare firearm retired-loot cleanup count mismatch.");
            foreach (RareFirearmLootCleanupMutation mutation in _cleanupMutations)
                mutation.Validate();
        }
        internal void Rollback()
        {
            for (int index = _cleanupMutations.Count - 1; index >= 0; index--)
                _cleanupMutations[index].Rollback();
            for (int index = _mutations.Count - 1; index >= 0; index--)
                _mutations[index].Rollback();
        }
    }

    internal sealed class RareFirearmLootCleanupMutation
    {
        private readonly BlueprintLoot _target;
        private readonly LootEntry[] _before;
        private readonly LootEntry[] _published;
        private readonly BlueprintItem[] _owned;
        private bool _changed;
        internal RareFirearmLootCleanupMutation(BlueprintLoot target, LootEntry[] before,
            LootEntry[] published, BlueprintItem[] owned, bool changed)
        { _target = target; _before = before; _published = published;
            _owned = owned; _changed = changed; }
        internal void Validate()
        {
            LootEntry[] current = _target.Items ?? new LootEntry[0];
            if (current.Any(value => value != null && _owned.Contains(value.Item)))
                throw new InvalidOperationException("Retired rare firearm loot target still contains project firearm rows: " + _target.name);
        }
        internal void Rollback()
        {
            if (!_changed) return;
            LootEntry[] current = _target.Items ?? new LootEntry[0];
            if (current.Length != _published.Length || current.Where((value, index) =>
                !ReferenceEquals(value, _published[index])).Any())
                throw new InvalidOperationException("Rare firearm cleanup rollback refused after foreign mutation.");
            _target.Items = _before;
            _changed = false;
        }
    }

    internal sealed class RareFirearmLootMutation
    {
        private readonly LootEntry[] _before;
        private readonly LootEntry[] _published;
        private bool _changed;
        internal RareFirearmLootMutation(BlueprintLoot target, LootEntry[] before,
            LootEntry[] published, BlueprintItem item,
            RareFirearmCampaignLootBlueprints.TargetSpec spec, bool changed)
        { Target = target; _before = before; _published = published; Item = item;
            Spec = spec; _changed = changed; }
        internal BlueprintLoot Target { get; private set; }
        internal BlueprintItem Item { get; private set; }
        internal RareFirearmCampaignLootBlueprints.TargetSpec Spec { get; private set; }
        internal static RareFirearmLootMutation Unchanged(BlueprintLoot target,
            LootEntry[] before, BlueprintItem item,
            RareFirearmCampaignLootBlueprints.TargetSpec spec)
        { return new RareFirearmLootMutation(target, before, before, item, spec, false); }
        internal void Validate()
        {
            LootEntry[] current = Target.Items ?? new LootEntry[0];
            LootEntry[] matches = current.Where(value => value != null &&
                ReferenceEquals(value.Item, Item)).ToArray();
            if (matches.Length != 1 || matches[0].Count != 1)
                throw new InvalidOperationException("Named firearm fixed-loot validation failed: " + Spec.Name);
            int retainedCount = _before.Count(value => value == null ||
                !ReferenceEquals(value.Item, Item));
            if (_changed && current.Length < retainedCount + 1)
                throw new InvalidOperationException("Fixed loot publication removed pre-existing content.");
        }
        internal void Rollback()
        {
            if (!_changed) return;
            LootEntry[] current = Target.Items ?? new LootEntry[0];
            if (current.Length != _published.Length || current.Where((value, index) =>
                !ReferenceEquals(value, _published[index])).Any())
                throw new InvalidOperationException("Rare firearm loot rollback refused after foreign mutation.");
            Target.Items = _before;
            _changed = false;
        }
    }
}
