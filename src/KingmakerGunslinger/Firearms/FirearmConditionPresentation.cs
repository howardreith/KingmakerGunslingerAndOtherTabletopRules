using System;

namespace KingmakerGunslinger.Firearms
{
    internal static class FirearmConditionPresentation
    {
        internal static string Describe(FirearmCondition condition)
        {
            switch (condition)
            {
                case FirearmCondition.Normal:
                    return "Firearm condition: Normal — ready for ordinary use.";
                case FirearmCondition.Broken:
                    return "Firearm condition: Broken — misfire value increases by 4. " +
                        "It can still fire and reload; Quick Clear or Repair Firearm restores Normal.";
                case FirearmCondition.Wrecked:
                    return "Firearm condition: Wrecked — it cannot fire or reload. " +
                        "Overhaul Firearm for one uninterrupted minute out of combat to restore Broken.";
                default:
                    throw new ArgumentOutOfRangeException("condition");
            }
        }
    }
}
