using System;
using System.Globalization;

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

        internal static string DescribeQualities(FirearmDefinition definition,
            FirearmState state)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (state == null) throw new ArgumentNullException("state");
            string handedness = definition.Kind == FirearmKind.Pistol ||
                definition.Kind == FirearmKind.Revolver
                    ? "One-Handed" : "Two-Handed";
            string range = definition.HasFixedRangeIncrement
                ? definition.RangeIncrementFeet.ToString(
                    CultureInfo.InvariantCulture) + " ft. Range"
                : "Scatter Cone";
            int misfire = Math.Min(FirearmDefinition.MaximumMisfireValue,
                definition.MisfireValue +
                    (state.Condition == FirearmCondition.Broken ? 4 : 0));
            return string.Format(CultureInfo.InvariantCulture,
                "Firearm, {0}, {1}, Capacity {2}, {3}, Misfire {4}, Condition: {5}. {6}",
                definition.Era, handedness, definition.Capacity, range,
                misfire, state.Condition,
                FirearmPenetrationPresentation.Describe(definition));
        }
    }
}
