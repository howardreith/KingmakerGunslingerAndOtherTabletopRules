using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Same complete request-parser boundary as the existing early attribution
    // control. No settings, save flag, general feature toggle or UI input.
    internal static class ElementalNereidQualificationControl
    {
        internal static void TryActivateEarly(ModContext context)
        {
            var decision = RuntimeTestRequestParser.TryActivate(
                Environment.GetCommandLineArgs(), context.ModEntry.Info.Version);
            if (!decision.Accepted || decision.Request == null) return;
            bool deferredMarkers = decision.Request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalDeferredMarkers;
            if (!deferredMarkers && !RuntimeTestScenarioCatalog.IsNereidQualificationScenario(decision.Request.Scenario) &&
                !RuntimeTestScenarioCatalog.IsNereidPersistenceScope(decision.Request.Scenario,
                    (string)decision.Request.Parameters?["qualificationTrait"])) return;
            if (!decision.Request.ExitAfterCompletion)
                throw new InvalidOperationException("Nereid qualification requires guarded process exit.");
            ElementalAlternateTraitPolicy.NereidQualificationActive = true;
            if (deferredMarkers) ElementalDeferredMarkerObservation.Activate();
            context.Logger.Info("elemental-races", "nereid.guarded-observation",
                "runId=" + decision.Request.RunId + ";ordinaryPublication=true;scope=guarded-observation");
        }
    }

    // Observation only, filtered to this exact guarded trait. No native state
    // or lifecycle return value is changed by either callback.
    [HarmonyPatch(typeof(Fact), "PostLoad", new Type[0])]
    internal static class ElementalNereidFactLoadObservation
    {
        private static void Prefix(Fact __instance)
        {
            ElementalDeferredMarkerObservation.Observe(__instance);
            Observe(__instance, "before-native-post-load");
        }
        private static void Postfix(Fact __instance) { Observe(__instance, "after-native-post-load"); }
        internal static void ObserveException(Exception exception)
        {
            if (!ElementalAlternateTraitPolicy.NereidQualificationActive || exception == null) return;
            string detail = exception.ToString();
            if (!detail.Contains("PostLoad") && !detail.Contains("ElementalNereid")) return;
            ModContext context;
            if (ModContext.TryGet(out context)) context.Logger.Info("elemental-races", "nereid.native-load-exception", detail);
        }
        private static string SavedComponent(object value)
        {
            var data = value.GetType().GetField("Data")?.GetValue(value) as IDictionary;
            return value.GetType().GetField("ComponentName")?.GetValue(value) + ":" +
                (data == null ? "null" : string.Join(",", data.Keys.Cast<object>()
                    .Select(key => key + "=" + data[key]?.GetType().FullName)));
        }
        internal static bool Owns(Fact fact)
        {
            if (!ElementalAlternateTraitPolicy.NereidQualificationActive || !(fact is Buff) || fact.Blueprint == null) return false;
            string guid = fact.Blueprint.AssetGuid;
            return guid == "e118e1e0a17a4acec001000000000006" ||
                guid == "e118e1e0a17a4acec001000000000003" || guid == "e118e1e0a17a4acec001000000000005";
        }
        internal static void ObserveDeactivation(Fact fact)
        {
            if (!Owns(fact)) return;
            Observe(fact, "native-deactivate");
            ModContext context;
            if (ModContext.TryGet(out context)) context.Logger.Info("elemental-races", "nereid.deactivation-caller",
                "guid=" + fact.Blueprint.AssetGuid + ";stack=" + new System.Diagnostics.StackTrace(1, false));
        }
        internal static void Observe(Fact fact, string phase)
        {
            if (!ElementalAlternateTraitPolicy.NereidQualificationActive || !(fact is Buff) || fact.Blueprint == null) return;
            string guid = fact.Blueprint.AssetGuid;
            if (guid != "e118e1e0a17a4acec001000000000006" &&
                guid != "e118e1e0a17a4acec001000000000003" &&
                guid != "e118e1e0a17a4acec001000000000005") return;
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var data = typeof(Fact).GetField("m_ComponentsData", flags)?.GetValue(fact) as IEnumerable;
            var buff = (Buff)fact;
            context.Logger.Info("elemental-races", "nereid.fact-load",
                "phase=" + phase + ";guid=" + guid + ";owner=" + buff.Owner?.Unit?.UniqueId +
                ";fact=" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(fact) +
                ";ownerOn=" + buff.Owner?.IsTurnedOn + ";inGame=" + buff.Owner?.Unit?.IsInGame +
                ";dead=" + buff.Owner?.State?.IsDead + ";suppressed=" + buff.IsSuppressed +
                ";initialized=" + fact.Initialized + ";active=" + fact.Active +
                ";didPostLoad=" + typeof(Fact).GetField("m_DidPostLoad", flags)?.GetValue(fact) +
                ";savedComponents=" + (data == null ? "<null>" : string.Join("|", data.Cast<object>().Select(SavedComponent))) +
                ";components=" + string.Join("|", fact.SelectComponents<Kingmaker.Blueprints.GameLogicComponent>()
                    .Select(value => value.name + ":" + value.GetInstanceID())));
        }
    }

    [HarmonyPatch(typeof(Fact), "Deactivate", new Type[0])]
    internal static class ElementalNereidDeactivationObservation
    {
        private static void Prefix(Fact __instance) { ElementalNereidFactLoadObservation.ObserveDeactivation(__instance); }
    }

    [HarmonyPatch(typeof(UberDebug), "LogException", new[] { typeof(Exception), typeof(UnityEngine.Object) })]
    internal static class ElementalNereidLoadExceptionWithContextObservation
    {
        private static void Prefix(Exception __0) { ElementalNereidFactLoadObservation.ObserveException(__0); }
    }
    [HarmonyPatch(typeof(UberDebug), "LogException", new[] { typeof(Exception) })]
    internal static class ElementalNereidLoadExceptionObservation
    {
        private static void Prefix(Exception __0) { ElementalNereidFactLoadObservation.ObserveException(__0); }
    }
}
