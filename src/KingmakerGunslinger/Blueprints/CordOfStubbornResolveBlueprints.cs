using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Loot;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Cord;
using KingmakerGunslinger.Fatigue;

namespace KingmakerGunslinger.Blueprints
{
    internal static class CordOfStubbornResolveBlueprints
    {
        internal const string Symbol = "KMG.Items.CordOfStubbornResolve";
        internal const string AcquisitionGuid =
            "9572baf3952095f41abda1fb25055cce";
        internal const string AcquisitionName =
            "RichHuman_treasure_chest_04 (1)";
        internal const string AcquisitionArea = "CapitalTavern_Indoor";
        internal const string LegacyAcquisitionGuid =
            "e2add2e7254305b40aa1b9ae60ed2be0";
        internal const string LegacyAcquisitionName =
            "RichHuman_treasure_chest_2";
        internal const string LegacyAcquisitionArea = "CapitalSquareVillage";
        private const string FatiguedBuffGuid = "e6f2fc5d73d88064583cb828801212f4";
        private const string ExhaustedBuffGuid = "46d1b9cc3d0fd36469a471b047d773a2";

        internal static BlueprintItemEquipmentBelt Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintBuff fatigued = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, FatiguedBuffGuid, "native Fatigued buff for Cord interception");
            BlueprintBuff exhausted = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, ExhaustedBuffGuid, "native Exhausted buff for Cord interception");
            CanonicalFatigueApplicationRuntime.Configure(fatigued, exhausted);
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
                        "This belt grants a +2 enhancement bonus to Constitution. Whenever an effect would make the wearer fatigued, the wearer instead takes 1d6 damage that ignores damage reduction and cannot reduce the wearer below 1 hit point. Whenever an effect would make the wearer exhausted, the wearer takes this damage and becomes fatigued instead."),
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
            BlueprintLoot target = RequireTarget(library, AcquisitionGuid,
                AcquisitionName, AcquisitionArea, "Cord fixed campaign loot");
            BlueprintLoot legacy = RequireTarget(library,
                LegacyAcquisitionGuid, LegacyAcquisitionName,
                LegacyAcquisitionArea, "retired Cord campaign loot");
            var mutations = new List<CordCampaignLootMutation>();
            try
            {
                mutations.Add(CordCampaignLootMutation.Normalize(target, cord,
                    publish));
                mutations.Add(CordCampaignLootMutation.Normalize(legacy, cord,
                    false));
                var result = new CordCampaignLootPublication(mutations);
                result.Validate();
                logger.Info("acadamae-graduate",
                    "cord-campaign-loot.published",
                    "Normalized the module-aware Cord placement at " +
                    AcquisitionName + " (" + AcquisitionGuid +
                    ") and removed the retired square row; enabled=" +
                    publish + ".");
                return result;
            }
            catch
            {
                for (int index = mutations.Count - 1; index >= 0; index--)
                    mutations[index].Rollback();
                throw;
            }
        }

        private static BlueprintLoot RequireTarget(
            LibraryScriptableObject library, string guid, string name,
            string area, string role)
        {
            BlueprintLoot target = BlueprintLibraryLookup
                .RequireExact<BlueprintLoot>(library, guid, role);
            if (!string.Equals(target.name, name, StringComparison.Ordinal) ||
                target.Area == null || !string.Equals(target.Area.name, area,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Cord loot identity/area mismatch: " + guid + ";name=" +
                    target.name + ";area=" + (target.Area == null
                        ? "<none>" : target.Area.name));
            return target;
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
        private readonly List<CordCampaignLootMutation> _mutations;

        internal CordCampaignLootPublication(
            List<CordCampaignLootMutation> mutations)
        {
            _mutations = mutations;
        }

        internal void Validate()
        {
            if (_mutations == null || _mutations.Count != 2)
                throw new InvalidOperationException(
                    "Cord fixed-loot publication target count mismatch.");
            foreach (CordCampaignLootMutation mutation in _mutations)
                mutation.Validate();
        }

        internal void Rollback()
        {
            for (int index = _mutations.Count - 1; index >= 0; index--)
                _mutations[index].Rollback();
        }
    }

    internal sealed class CordCampaignLootMutation
    {
        private readonly BlueprintLoot _target;
        private readonly LootEntry[] _before;
        private readonly LootEntry[] _published;
        private readonly BlueprintItemEquipmentBelt _cord;
        private readonly bool _expected;
        private bool _changed;

        private CordCampaignLootMutation(BlueprintLoot target,
            LootEntry[] before, LootEntry[] published,
            BlueprintItemEquipmentBelt cord, bool expected, bool changed)
        {
            _target = target;
            _before = before;
            _published = published;
            _cord = cord;
            _expected = expected;
            _changed = changed;
        }

        internal static CordCampaignLootMutation Normalize(
            BlueprintLoot target, BlueprintItemEquipmentBelt cord,
            bool expected)
        {
            LootEntry[] before = target.Items ?? new LootEntry[0];
            bool exact = before.Count(value => value != null &&
                ReferenceEquals(value.Item, cord) && value.Count == 1) ==
                (expected ? 1 : 0) && !before.Any(value => value != null &&
                    ReferenceEquals(value.Item, cord) && value.Count != 1);
            if (exact) return new CordCampaignLootMutation(target, before,
                before, cord, expected, false);
            LootEntry[] retained = before.Where(value => value == null ||
                !ReferenceEquals(value.Item, cord)).ToArray();
            LootEntry[] published = expected ? retained.Concat(new[] {
                new LootEntry { Item = cord, Count = 1 } }).ToArray() : retained;
            target.Items = published;
            return new CordCampaignLootMutation(target, before, published,
                cord, expected, true);
        }

        internal void Validate()
        {
            LootEntry[] current = _target.Items ?? new LootEntry[0];
            int exact = current.Count(value => value != null &&
                ReferenceEquals(value.Item, _cord) && value.Count == 1);
            int any = current.Count(value => value != null &&
                ReferenceEquals(value.Item, _cord));
            if (exact != (_expected ? 1 : 0) || any != exact)
                throw new InvalidOperationException(
                    "Cord fixed-loot publication failed exact validation: " +
                    _target.name);
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
