using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Acquisition
{
    internal sealed class CampaignWeaponPlacement
    {
        internal CampaignWeaponPlacement(string key, BlueprintItemWeapon item,
            string guid, string name, string area, bool enabled, bool relocated)
        {
            Key = key; Item = item; Location = new ProjectMagicItemLocation(
                key, guid, name, area); ModuleEnabled = enabled;
            Relocated = relocated;
        }
        internal string Key { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
        internal ProjectMagicItemLocation Location { get; private set; }
        internal bool ModuleEnabled { get; private set; }
        internal bool Relocated { get; private set; }
    }

    // The publication tables remain the sole authority for target identities.
    // Cord and merchant-only weapons deliberately do not enter this registry.
    internal static class CampaignWeaponRegistry
    {
        internal static bool IsNamedKey(string key)
        {
            return RareFirearmCampaignLootBlueprints.TargetSpecs.Any(value => value.ItemSymbol == key) ||
                EasternWeaponCampaignBlueprints.LootSpecs.Any(value => "eastern:" + value.NamedKinds.Single() == key) ||
                ElvenBranchedSpearCampaignBlueprints.LootSpecs.Any(value => "spear:" + value.NamedKind == key);
        }
        internal static CampaignWeaponPlacement[] Read()
        {
            ModContext context;
            if (!BlueprintBootstrap.IsInitialized || !ModContext.TryGet(out context))
                throw new InvalidOperationException("Campaign weapon graph is not initialized.");
            var result = new List<CampaignWeaponPlacement>();
            foreach (var spec in RareFirearmCampaignLootBlueprints.TargetSpecs)
                result.Add(new CampaignWeaponPlacement(spec.ItemSymbol,
                    BlueprintBootstrap.MagicFirearms.Require(spec.ItemSymbol).Item,
                    spec.Guid, spec.Name, spec.AreaName,
                    context.FeatureModules.Active.Gunslinger, true));
            foreach (var spec in EasternWeaponCampaignBlueprints.LootSpecs)
            {
                var kind = spec.NamedKinds.Single();
                // World-Tree Severer retains its original target. Night Without Moon
                // moved from the former Final Dungeon cluster before 0.0.87;
                // that historical relocation is also eligible for recovery.
                bool relocated = kind != EasternWeapons.EasternWeaponNamedKind.WorldTreeSeverer;
                result.Add(new CampaignWeaponPlacement("eastern:" + kind,
                    BlueprintBootstrap.EasternWeapons.Named.Require(kind).Item,
                    spec.Guid, spec.Name, spec.AreaName,
                    context.FeatureModules.Active.EasternWeapons, relocated));
            }
            foreach (var spec in ElvenBranchedSpearCampaignBlueprints.LootSpecs)
                result.Add(new CampaignWeaponPlacement("spear:" + spec.NamedKind,
                    BlueprintBootstrap.ElvenBranchedSpears.Named.Require(spec.NamedKind).Item,
                    spec.Guid, spec.Name, spec.AreaName,
                    context.FeatureModules.Active.ElvenBranchedSpears, true));
            if (result.Count != 29 || result.Select(value => value.Key).Distinct().Count() != 29 ||
                result.Select(value => value.Item.AssetGuid).Distinct().Count() != 29 ||
                result.Select(value => value.Location.TargetGuid).Distinct().Count() != 29)
                throw new InvalidOperationException("Complete named campaign weapon registry differs.");
            return result.ToArray();
        }

        internal static CampaignWeaponPlacement Require(string key)
        {
            var match = Read().SingleOrDefault(value => string.Equals(value.Key, key, StringComparison.Ordinal));
            if (match == null) throw new InvalidOperationException("Not a registered named campaign weapon.");
            return match;
        }
    }
}
