using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Primitive-only transport shape for a future persistence adapter. It is
    /// intentionally mutable for serializers; FirearmState remains immutable.
    /// </summary>
    public sealed class FirearmStateData
    {
        public int SchemaVersion { get; set; }

        public int LoadedRounds { get; set; }

        public string LoadedAmmunitionId { get; set; }

        public string Condition { get; set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{\"schemaVersion\":{0},\"loadedRounds\":{1},\"loadedAmmunitionId\":{2},\"condition\":\"{3}\"}}",
                SchemaVersion,
                LoadedRounds,
                LoadedAmmunitionId == null ? "null" : "\"" + LoadedAmmunitionId + "\"",
                Condition ?? string.Empty);
        }
    }
}
