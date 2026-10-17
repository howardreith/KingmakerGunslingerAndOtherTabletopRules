using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Which summoned units lose the base game's summoned-creature glow.
    ///
    /// <para>Every summoned unit carries the game's SummonedUnitBuff for its
    /// whole duration, and that buff's effect adds a looping golden rim light
    /// and a warm colour tint to the unit's materials. The visual refinement
    /// measured it on the native-body Wolf (cast through Summon Nature's
    /// Ally, so not the alignment template) and on the Ghaele (guarded run
    /// 20261017T1616452002903Z), and the owner asked for a normal wolf and for
    /// less shine on the others (VIS-008, VIS-036, VIS-037, VIS-038). KMG's
    /// rebodied creatures already show none, because their attachments
    /// replace the materials.</para>
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
