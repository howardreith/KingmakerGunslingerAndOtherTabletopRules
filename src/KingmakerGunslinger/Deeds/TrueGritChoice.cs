using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class TrueGritChoice
    {
        internal TrueGritChoice(TrueGritDeed deed, string displayName)
        {
            if (!Enum.IsDefined(typeof(TrueGritDeed), deed))
                throw new ArgumentOutOfRangeException("deed");
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name is required.", "displayName");
            Deed = deed;
            DisplayName = displayName;
        }

        internal TrueGritDeed Deed { get; private set; }
        internal string DisplayName { get; private set; }
    }
}
