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
    /// (<see cref="SummonGlowPolicy"/>). The glow is the effect of the game's
    /// SummonedCreatureVisual buff (<see cref="SummonGlowPolicy.VisualBuffGuid"/>):
    /// a looping rim light and a warm colour tint. Whenever that buff or the
    /// SummonedUnitBuff spawns its effect on a unit the policy covers, the
    /// visual buff's rim and tint animations are marked finished, so the
    /// material controller drops them and puts the unit's own rim and tint
    /// back. Either buff can spawn first (the visual buff comes with the unit
    /// blueprint, the summoned-unit buff with the summoning rule, and both
    /// again with a reloaded view), so each re-checks the other. The setup
    /// components stay enabled: the effect object is pooled, and re-enabling
    /// it for its next owner resets its animations. Nothing else in the
    /// effect, no other buff's effect and no transient flash is touched.
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
        private static int s_GlowBuffCalls;
        private static string s_LastSkip = "<none>";

        /// <summary>"suppressed:rim=N,tint=M" for a view, or "&lt;none&gt;".</summary>
        internal static string Describe(UnitEntityView view)
        {
            Record record;
            string own = view == null || !Records.TryGetValue(view, out record) ? "<none>" :
                "suppressed:rim=" + record.Rims + ",tint=" + record.Tints;
            lock (Sync)
                return own + ",calls=" + s_Calls + ",glowBuffCalls=" +
                    s_GlowBuffCalls + ",last=" + s_LastSkip;
        }

        /// <summary>
        /// Development evidence: every buff on the unit whose spawned effect
        /// carries rim-light or colour-tint setups, as
        /// "name:rims/tints/activeRims/activeTints" joined by '|'.
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
                int activeRims = 0, activeTints = 0;
                foreach (RimLightingAnimationSetup setup in rims)
                    if (setup.Settings != null && !setup.Settings.IsFinished) activeRims++;
                foreach (ColorTintAnimationSetup setup in tints)
                    if (setup.Settings != null && !setup.Settings.IsFinished) activeTints++;
                rows.Add((buff.Blueprint == null ? "<null>" : buff.Blueprint.name) +
                    ":" + rims.Length + "/" + tints.Length + "/" + activeRims + "/" +
                    activeTints);
            }
            return rows.Count == 0 ? "<none>" : string.Join("|", rows.ToArray());
        }

        private static void Note(string reason)
        {
            lock (Sync) s_LastSkip = reason.Replace(';', ',').Replace('|', '/');
        }

        private static void Postfix(Buff __instance)
        {
            try
            {
                lock (Sync) s_Calls++;
                if (__instance == null || __instance.Blueprint == null ||
                    !ExpandedSummoningVisualVariantPatch.IsModuleActive)
                    return;
                BlueprintRoot root = BlueprintRoot.Instance;
                BlueprintBuff summoned = root == null || root.SystemMechanics == null ? null :
                    root.SystemMechanics.SummonedUnitBuff;
                bool isVisual = __instance.Blueprint.AssetGuid == SummonGlowPolicy.VisualBuffGuid;
                bool isSummoned = summoned != null && __instance.Blueprint == summoned;
                if (!isVisual && !isSummoned) return;
                lock (Sync) s_GlowBuffCalls++;
                UnitEntityData unit = __instance.Owner == null ? null : __instance.Owner.Unit;
                UnitEntityView view = unit == null ? null : unit.View;
                if (view == null || unit.Descriptor == null) { Note("no-view"); return; }

                Buff visual = isVisual ? __instance : null;
                Buff summonedBuff = isSummoned ? __instance : null;
                foreach (Buff buff in unit.Descriptor.Buffs)
                {
                    if (buff == null || buff.Blueprint == null) continue;
                    if (visual == null && buff.Blueprint.AssetGuid == SummonGlowPolicy.VisualBuffGuid)
                        visual = buff;
                    else if (summonedBuff == null && summoned != null && buff.Blueprint == summoned)
                        summonedBuff = buff;
                }
                string unitName = unit.Blueprint == null ? null : unit.Blueprint.name;
                if (visual == null) { Note("no-visual-buff:" + unitName); return; }

                // The summoning ability is recorded on the summoned-unit buff's
                // context; the visual buff is a blueprint fact and has none.
                string ability = summonedBuff == null || summonedBuff.Context == null ||
                    summonedBuff.Context.SourceAbility == null ? null :
                    summonedBuff.Context.SourceAbility.name;
                bool registeredRim = ExpandedSummoningVisualVariantPatch.RimFor(view).HasValue;
                if (!SummonGlowPolicy.Suppresses(ability, unitName, registeredRim))
                {
                    Note("policy:" + (ability ?? "<null>") + "/" + (unitName ?? "<null>") +
                        "/rim=" + registeredRim);
                    return;
                }
                GameObject effect = EffectField == null ? null :
                    EffectField.GetValue(visual) as GameObject;
                if (effect == null) { Note("no-effect:" + unitName); return; }
                var record = new Record();
                foreach (RimLightingAnimationSetup setup in
                         effect.GetComponentsInChildren<RimLightingAnimationSetup>(true))
                {
                    if (setup.Settings == null) continue;
                    setup.Settings.IsFinished = true;
                    record.Rims++;
                }
                foreach (ColorTintAnimationSetup setup in
                         effect.GetComponentsInChildren<ColorTintAnimationSetup>(true))
                {
                    if (setup.Settings == null) continue;
                    setup.Settings.IsFinished = true;
                    record.Tints++;
                }
                Records.Remove(view);
                Records.Add(view, record);
                Note("applied:" + unitName + ":rim=" + record.Rims + ",tint=" + record.Tints);
            }
            catch (Exception exception)
            {
                // A presentation change never interrupts the game's own buff.
                Note("exception:" + exception.GetType().Name);
            }
        }
    }
}
