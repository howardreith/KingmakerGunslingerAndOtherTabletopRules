using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Loot;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Cord;

namespace KingmakerGunslinger.Blueprints
{
    internal static class CordOfStubbornResolveBlueprints
    {
        internal const string Symbol = "KMG.Items.CordOfStubbornResolve";
        internal const string AcquisitionGuid =
            "e2add2e7254305b40aa1b9ae60ed2be0";
        internal const string AcquisitionName = "RichHuman_treasure_chest_2";
        internal const string AcquisitionArea = "CapitalSquareVillage";
        private const string FatiguedBuffGuid = "e6f2fc5d73d88064583cb828801212f4";

        internal static BlueprintItemEquipmentBelt Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintBuff fatigued = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, FatiguedBuffGuid, "native Fatigued buff for Cord interception");
            CordConditionRuntime.Configure(fatigued);
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

        internal static CordCampaignLootPublication PublishCampaignLoot(
            LibraryScriptableObject library, BlueprintItemEquipmentBelt cord,
            bool publish, ModLogger logger)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (cord == null) throw new ArgumentNullException("cord");
            if (logger == null) throw new ArgumentNullException("logger");
            BlueprintLoot target = BlueprintLibraryLookup.RequireExact<BlueprintLoot>(
                library, AcquisitionGuid, "Cord fixed campaign loot");
            if (!string.Equals(target.name, AcquisitionName,
                    StringComparison.Ordinal) || target.Area == null ||
                !string.Equals(target.Area.name, AcquisitionArea,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Cord loot identity/area mismatch: " + AcquisitionGuid +
                    ";name=" + target.name + ";area=" +
                    (target.Area == null ? "<none>" : target.Area.name));
            LootEntry[] before = target.Items ?? new LootEntry[0];
            bool exact = before.Count(value => value != null &&
                ReferenceEquals(value.Item, cord) && value.Count == 1) ==
                (publish ? 1 : 0) && !before.Any(value => value != null &&
                    ReferenceEquals(value.Item, cord) && value.Count != 1);
            if (exact)
                return CordCampaignLootPublication.Unchanged(target, before,
                    cord, publish);
            LootEntry[] retained = before.Where(value => value == null ||
                !ReferenceEquals(value.Item, cord)).ToArray();
            LootEntry[] published = publish ? retained.Concat(new[] {
                new LootEntry { Item = cord, Count = 1 } }).ToArray() : retained;
            target.Items = published;
            var result = new CordCampaignLootPublication(target, before,
                published, cord, publish, true);
            result.Validate();
            logger.Info("acadamae-graduate", "cord-campaign-loot.published",
                "Normalized the module-aware Cord placement at " +
                AcquisitionName + " (" + AcquisitionGuid + "); enabled=" +
                publish + ".");
            return result;
        }

        private static bool IsNativeConstitutionTwoBelt(
            BlueprintItemEquipmentBelt belt)
        {
            return belt != null && belt.Cost == 4000 && string.Equals(
                belt.name, "BeltOfConstitution2", StringComparison.Ordinal);
        }
    }

    internal sealed class CordCampaignLootPublication
    {
        private readonly BlueprintLoot _target;
        private readonly LootEntry[] _before;
        private readonly LootEntry[] _published;
        private readonly BlueprintItemEquipmentBelt _cord;
        private readonly bool _expected;
        private bool _changed;

        internal CordCampaignLootPublication(BlueprintLoot target,
            LootEntry[] before, LootEntry[] published,
            BlueprintItemEquipmentBelt cord, bool expected, bool changed)
        { _target = target; _before = before; _published = published;
            _cord = cord; _expected = expected; _changed = changed; }

        internal static CordCampaignLootPublication Unchanged(
            BlueprintLoot target, LootEntry[] before,
            BlueprintItemEquipmentBelt cord, bool expected)
        { return new CordCampaignLootPublication(target, before, before, cord,
            expected, false); }

        internal void Validate()
        {
            LootEntry[] current = _target.Items ?? new LootEntry[0];
            int exact = current.Count(value => value != null &&
                ReferenceEquals(value.Item, _cord) && value.Count == 1);
            int any = current.Count(value => value != null &&
                ReferenceEquals(value.Item, _cord));
            if (exact != (_expected ? 1 : 0) || any != exact)
                throw new InvalidOperationException(
                    "Cord fixed-loot publication failed exact validation.");
        }

        internal void Rollback()
        {
            if (!_changed) return;
            LootEntry[] current = _target.Items ?? new LootEntry[0];
            if (current.Length != _published.Length || current.Where(
                (value, index) => !ReferenceEquals(value,
                    _published[index])).Any())
                throw new InvalidOperationException(
                    "Cord fixed-loot rollback refused after foreign mutation.");
            _target.Items = _before;
            _changed = false;
        }
    }
}
