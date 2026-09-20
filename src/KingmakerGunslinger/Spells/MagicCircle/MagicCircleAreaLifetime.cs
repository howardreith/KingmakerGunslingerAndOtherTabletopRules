using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Bootstrap;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.Spells.MagicCircle
{
    // Only the original timed carrier owns this emanation's lifetime. Native
    // AddAreaEffect already serializes its exact area instance; reuse that link
    // instead of adding another ownership registry or saved timer.
    internal sealed class MagicCircleAreaLifetime : BlueprintComponent
    {
        public BlueprintBuff Carrier;
        private static readonly FieldInfo AreaInstance = typeof(AddAreaEffect)
            .GetField("m_AreaEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void VerifyContract()
        {
            if (AreaInstance == null || AreaInstance.FieldType != typeof(AreaEffectEntityData))
                throw new MissingFieldException(typeof(AddAreaEffect).FullName, "m_AreaEffectInstance");
            MagicCircleCastContextPatch.VerifyContract();
        }

        internal static MagicCircleAreaLifetime ForOwnedArea(AreaEffectEntityData area)
        {
            // A foreign blueprint that copied our component is not our area.
            return area != null && BlueprintBootstrap.MagicCircles != null &&
                BlueprintBootstrap.MagicCircles.Any(value => ReferenceEquals(value.Area, area.Blueprint))
                ? area.Blueprint.GetComponent<MagicCircleAreaLifetime>() : null;
        }

        internal Buff FindActiveCarrier(AreaEffectEntityData area)
        {
            var parent = area.Context?.ParentContext;
            var bearer = parent?.MaybeOwner;
            if (bearer == null || bearer.Destroyed || !bearer.IsInState) return null;
            return bearer.Buffs.Enumerable.FirstOrDefault(buff => buff.Active &&
                ReferenceEquals(buff.Blueprint, Carrier) && ReferenceEquals(buff.Context, parent) &&
                buff.SelectComponents<AddAreaEffect>().Any(component =>
                    ReferenceEquals(AreaInstance.GetValue(component), area)));
        }
    }

    // Native CloneFor resolves MaybeCaster before constructing its child. If
    // that unit is temporarily unavailable or permanently removed, construction
    // substitutes the new owner. Circle contexts must retain the ORIGINAL native
    // UnitReference instead. Owner/position, parent, parameters, ranks and native
    // serialization remain unchanged; this adds no saved state or tracking.
    [HarmonyPatch(typeof(MechanicsContext), "CloneFor")]
    internal static class MagicCircleCastContextPatch
    {
        private static readonly FieldInfo CasterReference = typeof(MechanicsContext)
            .GetField("m_CasterReference", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void VerifyContract()
        {
            if (CasterReference == null || CasterReference.FieldType != typeof(UnitReference))
                throw new MissingFieldException(typeof(MechanicsContext).FullName, "m_CasterReference");
        }

        private static void Postfix(MechanicsContext __instance,
            BlueprintScriptableObject __0, MechanicsContext __result)
        {
            var circles = BlueprintBootstrap.MagicCircles;
            if (__result == null || circles == null || !circles.Any(circle =>
                ReferenceEquals(__0, circle.Carrier) || ReferenceEquals(__0, circle.Area) ||
                ReferenceEquals(__0, circle.Recipient))) return;
            CasterReference.SetValue(__result, CasterReference.GetValue(__instance));
        }
    }

    // Kingmaker's generic area cleanup ends spell areas when the original
    // caster dies/disappears. A timed circle cast on another creature must keep
    // its original deadline and caster attribution. Restrict this exception to
    // the exact marked area and its native carrier-instance link. Membership,
    // line of effect, entry/exit, scene callbacks and recipient ownership remain
    // native. This installs no polling loop and stores no custom runtime state.
    [HarmonyPatch(typeof(AreaEffectEntityData), "EndEffectIfNecessary")]
    internal static class MagicCircleAreaLifetimePatch
    {
        private static bool Prefix(AreaEffectEntityData __instance)
        {
            var lifetime = MagicCircleAreaLifetime.ForOwnedArea(__instance);
            if (lifetime == null) return true;
            if (!__instance.IsEnded && lifetime.FindActiveCarrier(__instance) == null) __instance.ForceEnd();
            return false;
        }
    }

    // Native point-targeted dispelling ForceEnds an area without removing the
    // AddAreaEffect carrier. Remove that exact carrier after a successful rule
    // so scene reconstruction cannot revive a dispelled circle. Generic ForceEnd
    // remains untouched: temporary scene unloading uses it too.
    [HarmonyPatch(typeof(RuleDispelMagic), "OnTrigger")]
    internal static class MagicCircleAreaDispelPatch
    {
        private static void Postfix(RuleDispelMagic __instance)
        {
            if (!__instance.Success || __instance.AreaEffect == null || !__instance.AreaEffect.IsEnded) return;
            var lifetime = MagicCircleAreaLifetime.ForOwnedArea(__instance.AreaEffect);
            lifetime?.FindActiveCarrier(__instance.AreaEffect)?.Remove();
        }
    }
}
