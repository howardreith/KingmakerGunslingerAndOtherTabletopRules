using System;
using System.Collections.Generic;
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
        private static readonly object Sync = new object();
        private static int s_Calls;
        private static int s_SummonedBuffCalls;
        private static string s_LastSkip = "<none>";

        /// <summary>"suppressed:rim=N,tint=M" for a view, or "&lt;none&gt;".</summary>
        internal static string Describe(UnitEntityView view)
        {
            Record record;
            string own = view == null || !Records.TryGetValue(view, out record) ? "<none>" :
                "suppressed:rim=" + record.Rims + ",tint=" + record.Tints;
            lock (Sync)
                return own + ",calls=" + s_Calls + ",summonedBuffCalls=" +
                    s_SummonedBuffCalls + ",lastSkip=" + s_LastSkip;
        }

        /// <summary>
        /// Development evidence: every buff on the unit whose spawned effect
        /// carries rim-light or colour-tint setups, as
        /// "name:rims/tints/activeRims" joined by '/'.
        /// </summary>
        internal static string DescribeBuffEffects(UnitEntityData unit)
        {
            if (unit == null || unit.Descriptor == null || EffectField == null)
                return "<none>";
            var rows = new List<string>();
            foreach (Buff buff in unit.Descriptor.Buffs)
            {
                GameObject effect = buff == null ? null :
                    EffectField.GetValue(buff) as GameObject;
                if (effect == null) continue;
                RimLightingAnimationSetup[] rims =
                    effect.GetComponentsInChildren<RimLightingAnimationSetup>(true);
                ColorTintAnimationSetup[] tints =
                    effect.GetComponentsInChildren<ColorTintAnimationSetup>(true);
                if (rims.Length == 0 && tints.Length == 0) continue;
                int active = 0;
                foreach (RimLightingAnimationSetup setup in rims)
                    if (setup.Settings != null && !setup.Settings.IsFinished) active++;
                rows.Add((buff.Blueprint == null ? "<null>" : buff.Blueprint.name) +
                    ":" + rims.Length + "/" + tints.Length + "/" + active);
            }
            return rows.Count == 0 ? "<none>" : string.Join("|", rows.ToArray());
        }

        private static void Skip(string reason)
        {
            lock (Sync) s_LastSkip = reason.Replace(';', ',').Replace('|', '/');
        }

        private static void Postfix(Buff __instance)
        {
            try
            {
                lock (Sync) s_Calls++;
                if (__instance == null || !ExpandedSummoningVisualVariantPatch.IsModuleActive)
                    return;
                BlueprintRoot root = BlueprintRoot.Instance;
                BlueprintBuff summoned = root == null || root.SystemMechanics == null ? null :
                    root.SystemMechanics.SummonedUnitBuff;
                if (summoned == null || __instance.Blueprint != summoned) return;
                lock (Sync) s_SummonedBuffCalls++;
                UnitEntityData unit = __instance.Owner == null ? null : __instance.Owner.Unit;
                UnitEntityView view = unit == null ? null : unit.View;
                if (view == null) { Skip("no-view"); return; }
                string ability = __instance.Context == null ||
                    __instance.Context.SourceAbility == null ? null :
                    __instance.Context.SourceAbility.name;
                string unitName = unit.Blueprint == null ? null : unit.Blueprint.name;
                bool registeredRim = ExpandedSummoningVisualVariantPatch.RimFor(view).HasValue;
                if (!SummonGlowPolicy.Suppresses(ability, unitName, registeredRim))
                {
                    Skip("policy:" + (ability ?? "<null>") + "/" + (unitName ?? "<null>") +
                        "/rim=" + registeredRim);
                    return;
                }
                GameObject effect = EffectField == null ? null :
                    EffectField.GetValue(__instance) as GameObject;
                if (effect == null) { Skip("no-effect:" + unitName); return; }
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
                Skip("applied:" + unitName + ":rim=" + record.Rims + ",tint=" + record.Tints);
            }
            catch (Exception exception)
            {
                // A presentation change never interrupts the game's own buff.
                Skip("exception:" + exception.GetType().Name);
            }
        }
    }
}
