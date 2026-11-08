using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Which summoned units lose the base game's summoned-creature glow.
    ///
    /// <para>The base game's summoned unit blueprints carry a permanent
    /// SummonedCreatureVisual buff whose effect adds a looping golden rim
    /// light and a warm colour tint to the unit's materials, and the KMG
    /// summon units cloned from them inherit it. The visual refinement
    /// measured it on the native-body Wolf (cast through Summon Nature's
    /// Ally, so not the alignment template) and on the Ghaele (guarded runs
    /// 20261017T1616452002903Z and 20261017T1639002324262Z, which also showed
    /// that the SummonedUnitBuff itself spawns no effect), and the owner asked
    /// for a normal wolf and for less shine on the others (VIS-008, VIS-036,
    /// VIS-037, VIS-038). The KMG creatures built from other donors, such as
    /// the Salamander, never carry the buff.</para>
    ///
    /// <para>Scope: units summoned through a KMG summoning ability (spell
    /// variants, native options and adapters all carry KMG blueprint names)
    /// or KMG summon units. The base game's own summons are untouched, and so
    /// is any registered visual variant that uses the rim glow as part of its
    /// look (the mephits and the lion).</para>
    /// </summary>
    internal static class SummonGlowPolicy
    {
        internal const string AbilityPrefix = "KMG_Summoning_";
        internal const string UnitPrefix = "KMG_Summoning_Unit_";
        /// <summary>The game's SummonedCreatureVisual buff.</summary>
        internal const string VisualBuffGuid = "706c182e86d9be848b59ddccca73d13e";

        /// <summary>
        /// Repair R6 PRES-07: the six native arrival buffs. Their effects keep
        /// their brief cue; a generic rim that would otherwise persist past
        /// this interval is ended on KMG summons.
        /// </summary>
        internal const float ArrivalRimSeconds = 3f;
        private static readonly string[] ArrivalBuffGuids = {
            "2c3b98e56c1b4e9479f692d7ef95b2d2", "25629d7e78016a340b0e50818b6d8bb5",
            "932d27490e1701548a48b4cbc2f2caac", "6fcdf014694b2b542a867763b4369cb3",
            "50d51854cf6a3434d96a87d050e1d09a", "0dff842f06edace43baf8a2f44207045" };
        internal const string ArrivalBuffNamePrefix = "SummonedCreatureSpawn";

        internal static bool IsArrivalBuff(string guid)
        { return Array.IndexOf(ArrivalBuffGuids, guid) >= 0; }

        internal static bool OutlastsArrival(bool loops, float lifetime)
        { return loops || lifetime > ArrivalRimSeconds; }

        /// <summary>
        /// Active generic rims in a "name:rims/tints/activeRims/activeTints"
        /// census: the persistent generic buff plus every arrival buff.
        /// </summary>
        internal static int ActiveGenericRims(string census)
        {
            int total = 0;
            foreach (string row in (census ?? string.Empty).Split('|'))
            {
                int colon = row.LastIndexOf(':');
                if (colon <= 0 || !row.StartsWith("SummonedCreature", StringComparison.Ordinal)) continue;
                string[] counts = row.Substring(colon + 1).Split('/');
                int active;
                if (counts.Length == 4 && int.TryParse(counts[2], out active)) total += active;
            }
            return total;
        }

        internal static bool Suppresses(string sourceAbilityName,
            string unitBlueprintName, bool hasRegisteredRim)
        {
            if (hasRegisteredRim) return false;
            return (sourceAbilityName != null &&
                    sourceAbilityName.StartsWith(AbilityPrefix, StringComparison.Ordinal)) ||
                (unitBlueprintName != null &&
                    unitBlueprintName.StartsWith(UnitPrefix, StringComparison.Ordinal));
        }
    }
}
