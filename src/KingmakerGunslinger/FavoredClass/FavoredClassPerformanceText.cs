using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// O01 displayed range: the owner's description of an invested
    /// performance states the range its own area actually has. Every native
    /// statement of the base range ("within 30 feet", "30-foot") becomes the
    /// owner's range; a description that states no range gains one closing
    /// sentence with it. Uninvested owners keep the exact native text.
    /// </summary>
    internal static class FavoredClassPerformanceText
    {
        internal static string OwnerDescription(string native, int baseFeet, int ownerFeet)
        {
            if (native == null) return null;
            if (baseFeet <= 0) throw new ArgumentOutOfRangeException("baseFeet");
            if (ownerFeet == baseFeet) return native;
            var statement = new Regex(@"\b" + baseFeet.ToString(CultureInfo.InvariantCulture) +
                @"(?=(?: |-)(?:feet|foot)\b)");
            if (statement.IsMatch(native))
                return statement.Replace(native, ownerFeet.ToString(CultureInfo.InvariantCulture));
            return native.TrimEnd() + " Range: " + ownerFeet.ToString(CultureInfo.InvariantCulture) + " feet.";
        }
    }
}
