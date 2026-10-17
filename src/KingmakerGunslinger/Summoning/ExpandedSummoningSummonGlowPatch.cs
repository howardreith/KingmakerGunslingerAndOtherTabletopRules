using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects.ColorTint;
using Kingmaker.Visual.MaterialEffects.RimLighting;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Removes the base game's summoned-creature glow from KMG summons
    /// (<see cref="SummonGlowPolicy"/>). When the SummonedUnitBuff spawns its
    /// effect on a unit the policy covers, the effect's looping rim-light and
    /// colour-tint animations are finished and their setup components
    /// disabled, so the material controller drops them and puts the unit's
    /// own rim and tint back. Nothing else in the effect, no other buff's
    /// effect and no transient flash is touched, and the effect is re-checked
    /// whenever the buff spawns it again (a reloaded view).
    /// </summary>
    [HarmonyPatch(typeof(Buff), "SpawnParticleEffect")]
    internal static class ExpandedSummoningSummonGlowPatch
    {
        private sealed class Record
        {
            internal int Rims;
            internal int Tints;
        }

        private static readonly FieldInfo EffectField = typeof(Buff).GetField(
            "m_ParticleEffect", BindingFlags.Instance | BindingFlags.NonPublic |
            BindingFlags.Public);
        private static readonly ConditionalWeakTable<UnitEntityView, Record> Records =
            new ConditionalWeakTable<UnitEntityView, Record>();

        /// <summary>"suppressed:rim=N,tint=M" for a view, or "&lt;none&gt;".</summary>
        internal static string Describe(UnitEntityView view)
        {
            Record record;
            if (view == null || !Records.TryGetValue(view, out record)) return "<none>";
            return "suppressed:rim=" + record.Rims + ",tint=" + record.Tints;
        }

        private static void Postfix(Buff __instance)
        {
            try
            {
                if (__instance == null || !ExpandedSummoningVisualVariantPatch.IsModuleActive)
                    return;
                BlueprintRoot root = BlueprintRoot.Instance;
                BlueprintBuff summoned = root == null || root.SystemMechanics == null ? null :
                    root.SystemMechanics.SummonedUnitBuff;
                if (summoned == null || __instance.Blueprint != summoned) return;
                UnitEntityData unit = __instance.Owner == null ? null : __instance.Owner.Unit;
                UnitEntityView view = unit == null ? null : unit.View;
                if (view == null) return;
                string ability = __instance.Context == null ||
                    __instance.Context.SourceAbility == null ? null :
                    __instance.Context.SourceAbility.name;
                bool registeredRim = ExpandedSummoningVisualVariantPatch.RimFor(view).HasValue;
                if (!SummonGlowPolicy.Suppresses(ability,
                        unit.Blueprint == null ? null : unit.Blueprint.name, registeredRim))
                    return;
                GameObject effect = EffectField == null ? null :
                    EffectField.GetValue(__instance) as GameObject;
                if (effect == null) return;
                var record = new Record();
                foreach (RimLightingAnimationSetup setup in
                         effect.GetComponentsInChildren<RimLightingAnimationSetup>(true))
                {
                    if (setup.Settings != null) setup.Settings.IsFinished = true;
                    setup.enabled = false;
                    record.Rims++;
                }
                foreach (ColorTintAnimationSetup setup in
                         effect.GetComponentsInChildren<ColorTintAnimationSetup>(true))
                {
                    if (setup.Settings != null) setup.Settings.IsFinished = true;
                    setup.enabled = false;
                    record.Tints++;
                }
                Records.Remove(view);
                Records.Add(view, record);
            }
            catch (Exception)
            {
                // A presentation change never interrupts the game's own buff.
            }
        }
    }
}
