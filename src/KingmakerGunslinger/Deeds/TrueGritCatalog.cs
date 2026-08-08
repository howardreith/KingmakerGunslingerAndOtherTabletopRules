using System;
using System.Linq;

namespace KingmakerGunslinger.Deeds
{
    internal static class TrueGritCatalog
    {
        private static readonly TrueGritChoice[] Values =
        {
            Choice(TrueGritDeed.Deadeye, "Deadeye"),
            Choice(TrueGritDeed.GunslingersDodge, "Gunslinger's Dodge"),
            Choice(TrueGritDeed.QuickClear, "Quick Clear"),
            Choice(TrueGritDeed.GunslingerInitiative, "Gunslinger Initiative"),
            Choice(TrueGritDeed.PistolWhip, "Pistol-Whip"),
            Choice(TrueGritDeed.StopBleeding, "Utility Shot: Stop Bleeding"),
            Choice(TrueGritDeed.DeadShot, "Dead Shot"),
            Choice(TrueGritDeed.StartlingShot, "Startling Shot"),
            Choice(TrueGritDeed.TargetingArms, "Targeting: Arms"),
            Choice(TrueGritDeed.TargetingHead, "Targeting: Head"),
            Choice(TrueGritDeed.TargetingTorso, "Targeting: Torso"),
            Choice(TrueGritDeed.TargetingLegs, "Targeting: Legs"),
            Choice(TrueGritDeed.BleedingWound, "Bleeding Wound"),
            Choice(TrueGritDeed.ExpertLoading, "Expert Loading"),
            Choice(TrueGritDeed.LightningReload, "Lightning Reload"),
            Choice(TrueGritDeed.Evasive, "Evasive"),
            Choice(TrueGritDeed.MenacingShot, "Menacing Shot"),
            Choice(TrueGritDeed.CheatDeath, "Cheat Death"),
            Choice(TrueGritDeed.DeathsShot, "Death's Shot"),
            Choice(TrueGritDeed.StunningShot, "Stunning Shot"),
            Choice(TrueGritDeed.FocusedAim, "Focused Aim"),
            Choice(TrueGritDeed.TwinShotKnockdown, "Twin Shot Knockdown"),
            Choice(TrueGritDeed.SteadyAim, "Steady Aim"),
            Choice(TrueGritDeed.FastMusket, "Fast Musket")
        };

        internal static TrueGritChoice[] Choices
        { get { return Values.ToArray(); } }

        internal static bool Contains(TrueGritDeed deed)
        { return Values.Any(value => value.Deed == deed); }

        internal static bool IsValidPair(TrueGritDeed first, TrueGritDeed second)
        { return first != second && Contains(first) && Contains(second); }

        private static TrueGritChoice Choice(TrueGritDeed deed, string name)
        { return new TrueGritChoice(deed, name); }
    }
}
