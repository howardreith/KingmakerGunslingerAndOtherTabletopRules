using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Presentation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class PlayerFacingPresentationAndDiscoverabilityTests
    {
        internal static void PlayerFacingTextPolicyRejectsLeaks()
        {
            string[] leaks =
            {
                "uses the family's stable weapon category",
                "while this exact weapon remains active",
                "genuine sneak attack damage",
                "native weapon-size step",
                "Apply the native Finesse Training damage-stat replacement",
                "Kingmaker has no usable native rule path",
                "Fey Bane uses the native bane property",
                "KMG_Internal_Value",
                "broken â€ text",
                "broken � text"
            };
            foreach (string leak in leaks)
                Assertions.False(PlayerFacingTextPolicy.IsClean(leak),
                    "Player-facing leak was accepted: " + leak);
            foreach (string text in new[]
            {
                "Use Dexterity instead of Strength for damage rolls.",
                "Fey Bane is especially effective against Fey creatures.",
                "A sneak attack that deals damage imposes a -2 penalty.",
                string.Empty
            })
                Assertions.True(PlayerFacingTextPolicy.IsClean(text),
                    "Meaningful player text was rejected: " + text);
        }

        internal static void DistributedCampaignPassesDiscoverabilityPolicy()
        {
            ProjectMagicItemDiscoverabilityAudit audit =
                ProjectMagicItemDiscoverabilityPolicy.Audit(
                    DistributedLocations());
            Assertions.True(audit.IsAcceptable,
                "Audited campaign distribution failed: " +
                string.Join("|", audit.Issues));
            Assertions.Equal(29, audit.ExactAreaDensity.Count,
                "The thirty items must span twenty-nine exact areas.");
            Assertions.Equal(3, audit.CampaignAreaDensity["FinalDungeon"],
                "One capstone item belongs on each Final Dungeon floor.");
            Assertions.Equal(2, audit.CampaignAreaDensity["IrovettiPalace"],
                "The only exact-area pairing must remain the thematic palace pair.");
        }

        internal static void DiscoverabilityPolicyRejectsUnsafeTargets()
        {
            ProjectMagicItemLocation[] locations = DistributedLocations();
            locations[0] = new ProjectMagicItemLocation("bad-temp",
                locations[1].TargetGuid, "Forest_HiddenCache",
                "RushlightFestivalCamp");
            ProjectMagicItemDiscoverabilityAudit audit =
                ProjectMagicItemDiscoverabilityPolicy.Audit(locations);
            string issues = string.Join("|", audit.Issues);
            Assertions.False(audit.IsAcceptable,
                "Unsafe clustered temporary hidden target was accepted.");
            foreach (string token in new[] {
                "temporary-area", "obscure-target", "distinct-targets" })
                Assertions.True(issues.Contains(token),
                    "Discoverability rejection omitted: " + token);
        }

        internal static void SourceTablesUseAuditedPersistentTargets()
        {
            string root = Environment.CurrentDirectory;
            string eastern = Read(root, "EasternWeaponCampaignBlueprints.cs");
            string spear = Read(root,
                "ElvenBranchedSpearCampaignBlueprints.cs");
            string firearm = Read(root,
                "RareFirearmCampaignLootBlueprints.cs");
            string cord = Read(root, "CordOfStubbornResolveBlueprints.cs");
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            string easternActive = Slice(eastern,
                "private static readonly EasternLootSpec[] Loot",
                "private static readonly EasternLootSpec[] CleanupLoot");
            string spearActive = Slice(spear,
                "private static readonly LootSpec[] Loot",
                "private static readonly CleanupSpec[] CleanupLoot");
            string firearmActive = Slice(firearm,
                "private static readonly TargetSpec[] Targets",
                "private static readonly CleanupSpec[] CleanupTargets");
            Assertions.Equal(18, Count(easternActive,
                "new EasternLootSpec("), "Eastern active loot count changed.");
            Assertions.Equal(6, Count(spearActive, "new LootSpec("),
                "Spear active loot count changed.");
            Assertions.Equal(5, Count(firearmActive, "new TargetSpec("),
                "Rare firearm active loot count changed.");
            foreach (string token in new[] {
                "SilverstepGrotto_FirstWorld", "RushlightFestivalCamp",
                "PitaxHorde", "Forest_Hidden", "Forest_cache" })
                Assertions.False((easternActive + spearActive + firearmActive)
                    .Contains(token), "Unsafe active loot target remains: " +
                    token);
            foreach (string token in new[] {
                "9572baf3952095f41abda1fb25055cce",
                "RichHuman_treasure_chest_04 (1)",
                "CapitalTavern_Indoor", "LegacyAcquisitionGuid",
                "LegacyAcquisitionArea" })
                Assertions.True(cord.Contains(token),
                    "Cord relocation contract lacks: " + token);
            foreach (string token in new[] {
                "across 29 exact areas", ";exactAreas=",
                "fixed Stag Lord Old Camp weapon chest" })
                Assertions.True(runtime.Contains(token),
                    "Runtime acquisition evidence lacks: " + token);
            Assertions.False(runtime.Contains("fixed Stag Lord Fort chest"),
                "Runtime evidence still names Border Sentinel's retired area.");
        }

        internal static void SourceTooltipsExcludeImplementationText()
        {
            string root = Environment.CurrentDirectory;
            string source = string.Join("\n", new[] {
                Read(root, "BasicAmmunitionBlueprints.cs"),
                Read(root, "GunsmithingSupplyBlueprints.cs"),
                Read(root, "FirearmRepairKitBlueprints.cs"),
                Read(root, "ProductionFirearmBlueprints.cs"),
                Read(root, "EasternWeaponBlueprints.cs"),
                Read(root, "EasternWeaponNamedBlueprints.cs"),
                Read(root, "ElvenBranchedSpearBlueprints.cs"),
                Read(root, "ElvenBranchedSpearNamedBlueprints.cs"),
                Read(root, "MagicFirearmBlueprints.cs"),
                Read(root, "CordOfStubbornResolveBlueprints.cs") });
            string audit = File.ReadAllText(Path.Combine(root, "docs",
                "ITEM-DESCRIPTION-AUDIT.md"));
            foreach (string phrase in new[] {
                "native-style proficiency", "stable weapon category",
                "single stable weapon", "this exact weapon",
                "genuine sneak attack", "damage-stat replacement",
                "Kingmaker has no usable", "native bane property",
                "native weapon-size step" })
                Assertions.False(source.IndexOf(phrase,
                        StringComparison.OrdinalIgnoreCase) >= 0,
                    "Player-facing source still contains: " + phrase);
            Assertions.True(source.Contains(
                "ConfigureEnchantmentText(value, string.Empty,") &&
                source.Contains("string.Empty, 0);"),
                "The Eastern policy-only enchantment must remain textless.");
            foreach (string redundant in new[] { "This is a ",
                " It remains a two-handed reach weapon", "bears a +1 enhancement bonus",
                " Elven Branched Spear. " })
                Assertions.False(source.Contains(redundant),
                    "Item source still repeats a normal-card trait: " + redundant);
            Assertions.True(source.Contains(
                    "Usable with Weapon Finesse. Grants a +2 bonus on attacks of opportunity triggered by enemy movement.") &&
                audit.Contains("| Moonlit Fork |") &&
                audit.Contains("KMG.ElvenBranchedSpear.MoonlitFork.Description") &&
                audit.Contains("## Magic firearms") &&
                audit.Contains("## Eastern named weapons") &&
                audit.Contains("## Elven Branched Spears"),
                "The individually reviewed item-description audit or Moonlit Fork snapshot is incomplete.");
        }

        private static ProjectMagicItemLocation[] DistributedLocations()
        {
            string[] areas =
            {
                "StagLordFort", "TrollLair_Exterior", "CandlemereTower",
                "Varnhold", "ArmagsTomb", "Brineheart", "CastleOfKnives",
                "FinalDungeon", "VarnholdStockade", "IrovettiPalace",
                "BlakemoorHideout", "FinalDungeon2", "StagLordOldCamp",
                "TrollLair_SecondLevel", "TrollhoundLair",
                "SilverstepGrotto_Cave", "SilverstepLake_Outdoor",
                "DunswardOutdoor", "BarbarianMainCamp", "PitaxTown",
                "GlenebonPlains", "IrovettiPalace", "FinalDungeon3",
                "HouseAtTheEdgeOfTime_2ndFloor",
                "HouseAtTheEdgeOfTime", "LoneCyclopCave",
                "CapitalRegionLair01", "NorthNarlmarchesRegionLair01",
                "MonsterLairHodag"
            };
            var result = new List<ProjectMagicItemLocation>();
            for (int index = 0; index < areas.Length; index++)
            {
                string targetName = index == 10
                    ? "RichHuman_NotHiddenLockedGood"
                    : index == 15 ? "Forest_UnhiddenLocked01" :
                        "TreasureChest_" + index;
                result.Add(new ProjectMagicItemLocation("item-" + index,
                    (index + 1).ToString("x32"), targetName, areas[index]));
            }
            result.Add(new ProjectMagicItemLocation(
                ProjectMagicItemDiscoverabilityPolicy.CordItemKey,
                ProjectMagicItemDiscoverabilityPolicy.CordTargetGuid,
                ProjectMagicItemDiscoverabilityPolicy.CordTargetName,
                ProjectMagicItemDiscoverabilityPolicy.CordAreaName));
            return result.ToArray();
        }

        private static string Read(string root, string file)
        {
            return File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", file));
        }

        private static string Slice(string source, string startToken,
            string endToken)
        {
            int start = source.IndexOf(startToken, StringComparison.Ordinal);
            int end = source.IndexOf(endToken, start,
                StringComparison.Ordinal);
            Assertions.True(start >= 0 && end > start,
                "Could not isolate active loot table: " + startToken);
            return source.Substring(start, end - start);
        }

        private static int Count(string source, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(token, index,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }
    }
}
