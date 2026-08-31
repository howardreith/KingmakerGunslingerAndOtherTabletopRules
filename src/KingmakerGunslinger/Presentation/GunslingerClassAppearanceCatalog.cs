using System;
using System.Collections.Generic;
using System.Globalization;

namespace KingmakerGunslinger.Presentation
{
    /// <summary>
    /// Evidence-selected native class-clothing identity for the Gunslinger.
    /// Every accessor returns a defensive copy so no class blueprint can share
    /// a mutable identifier array with this catalog or with a native donor.
    /// </summary>
    internal static class GunslingerClassAppearanceCatalog
    {
        internal const int DefaultPrimaryColor = 2;
        internal const int DefaultSecondaryColor = 22;

        // Native Magus direct class links. Installed-game rendering identifies
        // these as one fitted base plus its compatible belt/bracer accessory.
        private static readonly string[] MaleIds =
        {
            "6df8f61725a84294c8661bb9585eca97",
            "4c59d2b9740930145a27a4c693217d22"
        };

        private static readonly string[] FemaleIds =
        {
            "beba0e0c7dcd5c64d97d767be3e72995",
            "a93ead19aae8afc4794c54f5bcf73168"
        };

        internal static string[] MaleAssetIds()
        {
            return ValidateAndCopy("male", MaleIds, 2);
        }

        internal static string[] FemaleAssetIds()
        {
            return ValidateAndCopy("female", FemaleIds, 2);
        }

        internal static string[] ValidateAndCopy(string role,
            string[] assetIds, int expectedCount)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Appearance role is required.", "role");
            if (assetIds == null) throw new ArgumentNullException("assetIds");
            if (expectedCount < 0)
                throw new ArgumentOutOfRangeException("expectedCount");
            if (assetIds.Length != expectedCount)
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Gunslinger {0} appearance requires exactly {1} native links; observed {2}.",
                    role, expectedCount, assetIds.Length));

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = new string[assetIds.Length];
            for (int index = 0; index < assetIds.Length; index++)
            {
                string assetId = assetIds[index];
                if (!IsLowerHexAssetId(assetId))
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Gunslinger {0} appearance link {1} is not a 32-character lowercase hexadecimal asset identifier.",
                        role, index));
                if (!seen.Add(assetId))
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Gunslinger {0} appearance contains duplicate asset identifier {1}.",
                        role, assetId));
                result[index] = assetId;
            }
            return result;
        }

        private static bool IsLowerHexAssetId(string value)
        {
            if (value == null || value.Length != 32) return false;
            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];
                if (!((current >= '0' && current <= '9') ||
                    (current >= 'a' && current <= 'f')))
                    return false;
            }
            return true;
        }
    }
}
