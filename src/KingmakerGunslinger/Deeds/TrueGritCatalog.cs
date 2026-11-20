using System;
using System.Linq;

namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// The deeds True Grit may reduce. The rule that the two level-20 choices
    /// are different deeds is not enforced here: the native level-up selection
    /// enforces it (each choice is a one-rank feature, and
    /// BlueprintFeatureSelection.CanSelect refuses a feature the preview already
    /// has), observed by the disposable-true-grit-level-up-transaction scenario
    /// (RISK-04). A former test-only IsValidPair helper was removed so it is not
    /// mistaken for production protection.
    /// </summary>
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

        private static TrueGritChoice Choice(TrueGritDeed deed, string name)
        { return new TrueGritChoice(deed, name); }
    }
}
