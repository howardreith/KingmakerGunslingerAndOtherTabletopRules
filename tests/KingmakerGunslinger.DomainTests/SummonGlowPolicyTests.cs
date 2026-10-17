using System;
using System.IO;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The visual refinement's summoned-glow change (VIS-008, VIS-036, VIS-037,
    /// VIS-038): which summons lose the base game's SummonedCreatureVisual rim
    /// and tint, and that the patch touches only that buff's effect.
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

        internal static void ThePatchTouchesOnlyTheSummonedCreatureVisualEffect()
        {
            Assertions.Equal("706c182e86d9be848b59ddccca73d13e", SummonGlowPolicy.VisualBuffGuid,
                "The glow is the game's SummonedCreatureVisual buff (guarded run 20261017T1639002324262Z).");
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning", "ExpandedSummoningSummonGlowPatch.cs"));
            Assertions.True(source.Contains("[HarmonyPatch(typeof(Buff), \"SpawnParticleEffect\")]") &&
                source.Contains("SystemMechanics.SummonedUnitBuff") &&
                source.Contains("AssetGuid == SummonGlowPolicy.VisualBuffGuid") &&
                source.Contains("if (!isVisual && !isSummoned) return;") &&
                source.Contains("EffectField.GetValue(visual)") &&
                source.Contains("ExpandedSummoningVisualVariantPatch.IsModuleActive") &&
                source.Contains("ExpandedSummoningVisualVariantPatch.RimFor(view).HasValue") &&
                source.Contains("SummonGlowPolicy.Suppresses("),
                "The patch must gate on the module, the two summoning buffs, the policy " +
                "and the registered rim variants, and change only the visual buff's effect.");
            Assertions.True(source.Contains("GetComponentsInChildren<RimLightingAnimationSetup>(true)") &&
                source.Contains("GetComponentsInChildren<ColorTintAnimationSetup>(true)") &&
                source.Contains("setup.Settings.IsFinished = true") &&
                !source.Contains("Destroy(") && !source.Contains(".enabled = false"),
                "It finishes only that effect's rim and tint animations, destroys nothing and " +
                "leaves the pooled setups enabled so the effect's next owner resets them.");
        }
    }
}
