using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Assets
{
    internal static class WeaponVisualVariantCatalog
    {
        internal const string SpearClassic = "ElvenBranchedSpear.ClassicBranch";
        internal const string SpearThorn = "ElvenBranchedSpear.ThornBranch";
        internal const string SpearCrown = "ElvenBranchedSpear.CrownBranch";
        internal const string WakizashiClassic = "Wakizashi.Classic";
        internal const string WakizashiPetal = "Wakizashi.Petal";
        internal const string WakizashiMoon = "Wakizashi.Moon";
        internal const string WakizashiCapstone = "Wakizashi.Capstone";
        internal const string KatanaClassic = "Katana.Classic";
        internal const string KatanaReed = "Katana.Reed";
        internal const string KatanaRegal = "Katana.Regal";
        internal const string KatanaCapstone = "Katana.Capstone";
        internal const string NodachiClassic = "Nodachi.Classic";
        internal const string NodachiCleaver = "Nodachi.Cleaver";
        internal const string NodachiTitan = "Nodachi.Titan";
        internal const string NodachiCapstone = "Nodachi.Capstone";
        internal const string PistolService = "Pistol.Service";
        internal const string PistolDuelist = "Pistol.Duelist";
        internal const string PistolLastWord = "Pistol.LastWord";
        internal const string MusketService = "Musket.Service";
        internal const string BlunderbussService = "Blunderbuss.Service";
        internal const string RifleService = "Rifle.Service";
        internal const string RevolverService = "Revolver.Service";

        private static readonly Dictionary<string, string> Variants =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "KMG.ElvenBranchedSpear.BaseItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.MasterworkItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.ColdIronItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.MasterworkColdIronItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.Plus1Item", SpearClassic },
                { "KMG.ElvenBranchedSpear.Plus1ColdIronItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.Boughkeeper", SpearThorn },
                { "KMG.ElvenBranchedSpear.Thornstep", SpearThorn },
                { "KMG.ElvenBranchedSpear.MoonlitFork", SpearThorn },
                { "KMG.ElvenBranchedSpear.VipersReach", SpearCrown },
                { "KMG.ElvenBranchedSpear.BriarCrownedSpear", SpearCrown },
                { "KMG.ElvenBranchedSpear.SpearOfTheFirstBranch", SpearCrown },
                { "KMG.EasternWeapons.Wakizashi.BaseItem", WakizashiClassic },
                { "KMG.EasternWeapons.Wakizashi.MasterworkItem", WakizashiClassic },
                { "KMG.EasternWeapons.Wakizashi.ColdIronItem", WakizashiPetal },
                { "KMG.EasternWeapons.Wakizashi.Plus1Item", WakizashiClassic },
                { "KMG.EasternWeapons.Wakizashi.PaperLantern", WakizashiPetal },
                { "KMG.EasternWeapons.Wakizashi.QuietCurrent", WakizashiPetal },
                { "KMG.EasternWeapons.Wakizashi.FallingPetal", WakizashiPetal },
                { "KMG.EasternWeapons.Wakizashi.FoxfireWhisper", WakizashiMoon },
                { "KMG.EasternWeapons.Wakizashi.EmptySleeve", WakizashiMoon },
                { "KMG.EasternWeapons.Wakizashi.NightWithoutMoon", WakizashiCapstone },
                { "KMG.EasternWeapons.Katana.BaseItem", KatanaClassic },
                { "KMG.EasternWeapons.Katana.MasterworkItem", KatanaClassic },
                { "KMG.EasternWeapons.Katana.ColdIronItem", KatanaReed },
                { "KMG.EasternWeapons.Katana.Plus1Item", KatanaClassic },
                { "KMG.EasternWeapons.Katana.WayfarersOath", KatanaReed },
                { "KMG.EasternWeapons.Katana.WinterReed", KatanaReed },
                { "KMG.EasternWeapons.Katana.DrawnHorizon", KatanaReed },
                { "KMG.EasternWeapons.Katana.ThunderAtTheGate", KatanaRegal },
                { "KMG.EasternWeapons.Katana.MoonlitCrossing", KatanaRegal },
                { "KMG.EasternWeapons.Katana.HeavensMeasure", KatanaCapstone },
                { "KMG.EasternWeapons.Nodachi.BaseItem", NodachiClassic },
                { "KMG.EasternWeapons.Nodachi.MasterworkItem", NodachiClassic },
                { "KMG.EasternWeapons.Nodachi.ColdIronItem", NodachiCleaver },
                { "KMG.EasternWeapons.Nodachi.Plus1Item", NodachiClassic },
                { "KMG.EasternWeapons.Nodachi.BorderSentinel", NodachiCleaver },
                { "KMG.EasternWeapons.Nodachi.CloudCleaver", NodachiCleaver },
                { "KMG.EasternWeapons.Nodachi.StormOverStone", NodachiCleaver },
                { "KMG.EasternWeapons.Nodachi.MountainSunder", NodachiTitan },
                { "KMG.EasternWeapons.Nodachi.UnfixedForm", NodachiTitan },
                { "KMG.EasternWeapons.Nodachi.WorldTreeSeverer", NodachiCapstone },
                { "KMG.Test.TestMusketItem", MusketService },
                { "KMG.Firearms.EarlyPistolItem", PistolService },
                { "KMG.Firearms.EarlyMusketItem", MusketService },
                { "KMG.Firearms.EarlyBlunderbussItem", BlunderbussService },
                { "KMG.Firearms.AdvancedRifleItem", RifleService },
                { "KMG.Firearms.AdvancedRevolverItem", RevolverService },
                { "KMG.Firearms.PistolPlus1Item", PistolService },
                { "KMG.Firearms.MusketPlus1Item", MusketService },
                { "KMG.Firearms.BlunderbussPlus1Item", BlunderbussService },
                { "KMG.Firearms.DuelistsRebuttalItem", PistolDuelist },
                { "KMG.Firearms.RiverKingsMeasureItem", MusketService },
                { "KMG.Firearms.IrovettisOvationItem", BlunderbussService },
                { "KMG.Firearms.TheLastWordItem", PistolLastWord },
                { "KMG.Firearms.WatchAtTheWorldsEndItem", MusketService }
            };

        internal static string Require(string blueprintSymbol)
        {
            if (string.IsNullOrEmpty(blueprintSymbol))
                throw new ArgumentException(
                    "A blueprint symbol is required.", "blueprintSymbol");
            string variant;
            if (!Variants.TryGetValue(blueprintSymbol, out variant))
                throw new KeyNotFoundException(
                    "No approved weapon visual variant is mapped for " +
                    blueprintSymbol + ".");
            return variant;
        }

        internal static bool TryGet(string blueprintSymbol, out string variant)
        { return Variants.TryGetValue(blueprintSymbol, out variant); }

        internal static KeyValuePair<string, string>[] Snapshot()
        {
            var value = new KeyValuePair<string, string>[Variants.Count];
            int index = 0;
            foreach (KeyValuePair<string, string> pair in Variants)
                value[index++] = pair;
            return value;
        }
    }
}
