using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Defensive-copy and ordinal-comparison helpers for mutable serializer DTOs.
    /// Persistence stores must never expose their owned FirearmStateData instance.
    /// </summary>
    internal static class FirearmStateDataUtility
    {
        internal static FirearmStateData Clone(FirearmStateData data)
        {
            if (data == null)
            {
                return null;
            }

            return new FirearmStateData
            {
                SchemaVersion = data.SchemaVersion,
                LoadedRounds = data.LoadedRounds,
                LoadedAmmunitionId = data.LoadedAmmunitionId,
                Condition = data.Condition
            };
        }

        internal static bool AreEqual(FirearmStateData left, FirearmStateData right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.SchemaVersion == right.SchemaVersion &&
                left.LoadedRounds == right.LoadedRounds &&
                string.Equals(
                    left.LoadedAmmunitionId,
                    right.LoadedAmmunitionId,
                    StringComparison.Ordinal) &&
                string.Equals(left.Condition, right.Condition, StringComparison.Ordinal);
        }

        internal static string Describe(FirearmStateData data)
        {
            if (data == null)
            {
                return "<absent>";
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "schema={0}; rounds={1}; ammunition={2}; condition={3}",
                data.SchemaVersion,
                data.LoadedRounds,
                data.LoadedAmmunitionId ?? "<none>",
                data.Condition ?? "<null>");
        }
    }
}
