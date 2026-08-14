using System;

namespace KingmakerGunslinger.EasternWeapons
{
    [Flags]
    internal enum EasternWeaponNativeProperty
    {
        None = 0,
        Flaming = 1 << 0,
        Frost = 1 << 1,
        Agile = 1 << 2,
        Keen = 1 << 3,
        GhostTouch = 1 << 4,
        Shock = 1 << 5,
        Thundering = 1 << 6,
        Holy = 1 << 7,
        BrilliantEnergy = 1 << 8,
        Speed = 1 << 9
    }

    internal enum EasternWeaponNamedKind
    {
        PaperLantern,
        QuietCurrent,
        FallingPetal,
        FoxfireWhisper,
        EmptySleeve,
        NightWithoutMoon,
        WayfarersOath,
        WinterReed,
        DrawnHorizon,
        ThunderAtTheGate,
        MoonlitCrossing,
        HeavensMeasure,
        BorderSentinel,
        CloudCleaver,
        StormOverStone,
        MountainSunder,
        UnfixedForm,
        WorldTreeSeverer
    }

    internal sealed class EasternWeaponNamedSpec
    {
        internal EasternWeaponNamedSpec(EasternWeaponNamedKind kind,
            EasternWeaponFamily family, string symbol, string displayName,
            int enhancement, bool coldIron,
            EasternWeaponNativeProperty properties, int bespokePremium,
            int finalCost)
        {
            if (string.IsNullOrWhiteSpace(symbol) ||
                string.IsNullOrWhiteSpace(displayName) || enhancement < 1 ||
                enhancement > 5 || bespokePremium < 0 || finalCost <= 0)
                throw new ArgumentException(
                    "Eastern named-weapon specification is invalid.");
            Kind = kind; Family = family; Symbol = symbol;
            DisplayName = displayName; Enhancement = enhancement;
            ColdIron = coldIron; Properties = properties;
            BespokePremium = bespokePremium; FinalCost = finalCost;
        }

        internal EasternWeaponNamedKind Kind { get; private set; }
        internal EasternWeaponFamily Family { get; private set; }
        internal string Symbol { get; private set; }
        internal string DisplayName { get; private set; }
        internal int Enhancement { get; private set; }
        internal bool ColdIron { get; private set; }
        internal EasternWeaponNativeProperty Properties { get; private set; }
        internal int BespokePremium { get; private set; }
        internal int FinalCost { get; private set; }

        internal bool Has(EasternWeaponNativeProperty property)
        { return (Properties & property) != 0; }

        internal int NativeEffectiveBonus
        {
            get
            {
                int value = Enhancement;
                foreach (EasternWeaponNativeProperty property in Enum.GetValues(
                    typeof(EasternWeaponNativeProperty)))
                    if (property != EasternWeaponNativeProperty.None &&
                        Has(property)) value += PropertyBonus(property);
                return value;
            }
        }

        private static int PropertyBonus(EasternWeaponNativeProperty property)
        {
            return property == EasternWeaponNativeProperty.Holy ? 2 :
                property == EasternWeaponNativeProperty.Speed ? 3 :
                property == EasternWeaponNativeProperty.BrilliantEnergy ? 4 : 1;
        }
    }

    internal static class EasternWeaponNamedCatalog
    {
        private const string Root = "KMG.EasternWeapons.";

        private static readonly EasternWeaponNamedSpec[] Items =
        {
            Spec(EasternWeaponNamedKind.PaperLantern,
                EasternWeaponFamily.Wakizashi, "PaperLantern", "Paper Lantern",
                1, true, EasternWeaponNativeProperty.Flaming, 0, 10370),
            Spec(EasternWeaponNamedKind.QuietCurrent,
                EasternWeaponFamily.Wakizashi, "QuietCurrent", "Quiet Current",
                2, false, EasternWeaponNativeProperty.Agile, 0, 18335),
            Spec(EasternWeaponNamedKind.FallingPetal,
                EasternWeaponFamily.Wakizashi, "FallingPetal", "Falling Petal",
                2, false, EasternWeaponNativeProperty.Agile |
                    EasternWeaponNativeProperty.Keen, 4000, 36335),
            Spec(EasternWeaponNamedKind.FoxfireWhisper,
                EasternWeaponFamily.Wakizashi, "FoxfireWhisper", "Foxfire Whisper",
                3, false, EasternWeaponNativeProperty.Agile |
                    EasternWeaponNativeProperty.GhostTouch, 0, 50335),
            Spec(EasternWeaponNamedKind.EmptySleeve,
                EasternWeaponFamily.Wakizashi, "EmptySleeve", "Empty Sleeve",
                4, false, EasternWeaponNativeProperty.Agile |
                    EasternWeaponNativeProperty.Keen, 0, 72335),
            Spec(EasternWeaponNamedKind.NightWithoutMoon,
                EasternWeaponFamily.Wakizashi, "NightWithoutMoon",
                "Night Without Moon", 5, false,
                EasternWeaponNativeProperty.Agile |
                    EasternWeaponNativeProperty.Keen |
                    EasternWeaponNativeProperty.Speed, 0, 200335),

            Spec(EasternWeaponNamedKind.WayfarersOath,
                EasternWeaponFamily.Katana, "WayfarersOath", "Wayfarer's Oath",
                1, true, EasternWeaponNativeProperty.None, 2000, 6400),
            Spec(EasternWeaponNamedKind.WinterReed,
                EasternWeaponFamily.Katana, "WinterReed", "Winter Reed",
                2, false, EasternWeaponNativeProperty.Frost, 0, 18350),
            Spec(EasternWeaponNamedKind.DrawnHorizon,
                EasternWeaponFamily.Katana, "DrawnHorizon", "Drawn Horizon",
                3, false, EasternWeaponNativeProperty.Keen, 0, 32350),
            Spec(EasternWeaponNamedKind.ThunderAtTheGate,
                EasternWeaponFamily.Katana, "ThunderAtTheGate",
                "Thunder at the Gate", 3, false,
                EasternWeaponNativeProperty.Shock |
                    EasternWeaponNativeProperty.Thundering, 0, 50350),
            Spec(EasternWeaponNamedKind.MoonlitCrossing,
                EasternWeaponFamily.Katana, "MoonlitCrossing", "Moonlit Crossing",
                4, false, EasternWeaponNativeProperty.Keen |
                    EasternWeaponNativeProperty.Holy, 8000, 106350),
            Spec(EasternWeaponNamedKind.HeavensMeasure,
                EasternWeaponFamily.Katana, "HeavensMeasure", "Heaven's Measure",
                5, false, EasternWeaponNativeProperty.Keen |
                    EasternWeaponNativeProperty.BrilliantEnergy, 0, 200350),

            Spec(EasternWeaponNamedKind.BorderSentinel,
                EasternWeaponFamily.Nodachi, "BorderSentinel", "Border Sentinel",
                1, true, EasternWeaponNativeProperty.None, 0, 4420),
            Spec(EasternWeaponNamedKind.CloudCleaver,
                EasternWeaponFamily.Nodachi, "CloudCleaver", "Cloud-Cleaver",
                2, false, EasternWeaponNativeProperty.Keen, 0, 18360),
            Spec(EasternWeaponNamedKind.StormOverStone,
                EasternWeaponFamily.Nodachi, "StormOverStone", "Storm Over Stone",
                3, false, EasternWeaponNativeProperty.Shock |
                    EasternWeaponNativeProperty.Thundering, 0, 50360),
            Spec(EasternWeaponNamedKind.MountainSunder,
                EasternWeaponFamily.Nodachi, "MountainSunder", "Mountain-Sunder",
                3, false, EasternWeaponNativeProperty.Keen, 20000, 52360),
            Spec(EasternWeaponNamedKind.UnfixedForm,
                EasternWeaponFamily.Nodachi, "UnfixedForm", "Unfixed Form",
                4, false, EasternWeaponNativeProperty.Keen, 12000, 62360),
            Spec(EasternWeaponNamedKind.WorldTreeSeverer,
                EasternWeaponFamily.Nodachi, "WorldTreeSeverer",
                "World-Tree Severer", 5, false,
                EasternWeaponNativeProperty.Holy |
                    EasternWeaponNativeProperty.Speed, 0, 200360)
        };

        static EasternWeaponNamedCatalog()
        {
            if (Items.Length != 18 ||
                Array.Exists(Items, value => value.NativeEffectiveBonus > 10))
                throw new InvalidOperationException(
                    "Eastern named catalog cardinality or +10 ceiling is invalid.");
        }

        internal static EasternWeaponNamedSpec[] All
        { get { return (EasternWeaponNamedSpec[])Items.Clone(); } }

        internal static EasternWeaponNamedSpec Require(
            EasternWeaponNamedKind kind)
        {
            foreach (EasternWeaponNamedSpec item in Items)
                if (item.Kind == kind) return item;
            throw new ArgumentOutOfRangeException("kind");
        }

        private static EasternWeaponNamedSpec Spec(EasternWeaponNamedKind kind,
            EasternWeaponFamily family, string key, string displayName,
            int enhancement, bool coldIron,
            EasternWeaponNativeProperty properties, int bespokePremium,
            int finalCost)
        {
            return new EasternWeaponNamedSpec(kind, family,
                Root + family + "." + key, displayName, enhancement, coldIron,
                properties, bespokePremium, finalCost);
        }
    }
}
