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
