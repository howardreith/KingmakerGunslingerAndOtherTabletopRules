using System;
using System.IO;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The visual refinement's summoned-glow change (VIS-008, VIS-036, VIS-037,
    /// VIS-038): which summons lose the base game's SummonedUnitBuff rim and
    /// tint, and that the patch touches only that buff's effect.
    /// </summary>
    internal static class SummonGlowPolicyTests
    {
        internal static void OnlyKmgSummonsWithoutARegisteredRimLoseTheGlow()
        {
            Assertions.True(SummonGlowPolicy.Suppresses(
                    "KMG_Summoning_Ability_SNA_Tier2_Wolf_One", "KMG_Summoning_Unit_Wolf", false),
                "A KMG summon spell's wolf loses the summoned glow.");
            Assertions.True(SummonGlowPolicy.Suppresses(
                    "KMG_Summoning_NativeOption_SM_Tier9_Thanadaemon_One", "Thanadaemon", false),
                "A native unit summoned through a KMG native option loses it too.");
            Assertions.True(SummonGlowPolicy.Suppresses(
                    "KMG_Summoning_Adapter_MasterSummoner_Tier9_FrostGiant_OneD3", "FrostGiant", false),
                "A native unit summoned through a KMG adapter loses it too.");
            Assertions.True(SummonGlowPolicy.Suppresses(null, "KMG_Summoning_Unit_GhaeleAzata", false),
                "A KMG summon unit loses it when no source ability is recorded.");
            Assertions.False(SummonGlowPolicy.Suppresses(
                    "SummonMonsterIISingle", "CR1_WolfStandardSummon", false),
                "The base game's own summons keep the game's glow.");
            Assertions.False(SummonGlowPolicy.Suppresses(
                    "KMG_Summoning_Ability_SM_Tier3_IceMephit_One", "KMG_Summoning_Unit_IceMephit", true),
                "A registered rim variant (the mephits, the lion) keeps the glow its look is built on.");
            Assertions.False(SummonGlowPolicy.Suppresses(null, null, false),
                "An unidentified unit is left alone.");
        }

        internal static void ThePatchTouchesOnlyTheSummonedUnitBuffEffect()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning", "ExpandedSummoningSummonGlowPatch.cs"));
            Assertions.True(source.Contains("[HarmonyPatch(typeof(Buff), \"SpawnParticleEffect\")]") &&
                source.Contains("SystemMechanics.SummonedUnitBuff") &&
                source.Contains("__instance.Blueprint != summoned") &&
                source.Contains("ExpandedSummoningVisualVariantPatch.IsModuleActive") &&
                source.Contains("ExpandedSummoningVisualVariantPatch.RimFor(view).HasValue") &&
                source.Contains("SummonGlowPolicy.Suppresses("),
                "The patch must gate on the module, the exact summoned-unit buff, the " +
                "policy and the registered rim variants.");
            Assertions.True(source.Contains("GetComponentsInChildren<RimLightingAnimationSetup>(true)") &&
                source.Contains("GetComponentsInChildren<ColorTintAnimationSetup>(true)") &&
                source.Contains("setup.Settings.IsFinished = true") &&
                !source.Contains("Destroy("),
                "It finishes only that effect's rim and tint animations and destroys nothing.");
        }
    }
}
