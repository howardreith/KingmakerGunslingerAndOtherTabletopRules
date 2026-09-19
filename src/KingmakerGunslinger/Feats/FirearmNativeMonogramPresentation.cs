using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Feats
{
    // Parametrized selector/selected-feature/character-sheet data use this constructor.
    // Keep blueprint sprites and saved FeatureParam identities intact. Native UI
    // supplies its own TMP font, background and overlays for the null-icon route.
    [HarmonyPatch]
    internal static class FirearmNativeMonogramPresentation
    {
        private static MethodBase TargetMethod() => typeof(FeatureUIData).GetConstructor(
            new[] { typeof(BlueprintFeature), typeof(FeatureParam) });

        private static void Postfix(FeatureUIData __instance)
        {
            string letter;
            if (!TryGetLetter(__instance, out letter)) return;
            __instance.Icon = null;
            __instance.NameForAcronim = letter;
        }

        internal static bool TryGetLetter(FeatureUIData data, out string letter)
        {
            letter = null;
            if (data == null || data.Feature == null) return false;
            var native = data.Feature as BlueprintParametrizedFeature;
            FirearmKind kind;
            if (native != null && NativeFirearmFeatIntegration.IsIntegrated(native) && data.Param != null &&
                NativeFirearmFeatIntegration.TryKind(data.Param, out kind))
                return FirearmNativeMonogramPolicy.TryLetter(kind, true, false, out letter);

            return TryGetRapidLetter(data.Feature, out letter);
        }

        internal static bool TryGetRapidLetter(BlueprintFeature feature, out string letter)
        {
            letter = null;
            if (feature == null) return false;
            var set = BlueprintBootstrap.FirearmFeats;
            if (set == null || set.RapidReloadChoices == null) return false;
            var kinds = OfficialFirearmSupport.Kinds;
            if (set.RapidReloadChoices.Length != kinds.Length) return false;
            for (int index = 0; index < kinds.Length; index++)
                if (ReferenceEquals(feature, set.RapidReloadChoices[index]))
                    return FirearmNativeMonogramPolicy.TryLetter(kinds[index], false, true, out letter);
            return false;
        }
    }

    // Native static selections enumerate raw BlueprintFeature objects. Adapt
    // only this owned selector's existing rows; its native DLC filtering and
    // selection prohibition run unchanged, and no choice is added or removed.
    [HarmonyPatch(typeof(BlueprintFeatureSelection), "get_Items")]
    internal static class RapidReloadNativeMonogramSelection
    {
        private static void Postfix(BlueprintFeatureSelection __instance,
            ref IEnumerable<IFeatureSelectionItem> __result)
        {
            var set = BlueprintBootstrap.FirearmFeats;
            if (set == null || !ReferenceEquals(__instance, set.RapidReload) || __result == null) return;
            __result = PresentExistingRows(__result);
        }

        private static IEnumerable<IFeatureSelectionItem> PresentExistingRows(IEnumerable<IFeatureSelectionItem> source)
        {
            foreach (var item in source)
            {
                string letter;
                if (item == null || !FirearmNativeMonogramPresentation.TryGetRapidLetter(item.Feature, out letter))
                { yield return item; continue; }
                yield return new FeatureUIData(item.Feature, (item as FeatureUIData)?.Param,
                    item.Name, item.Description, null, letter);
            }
        }
    }
}
