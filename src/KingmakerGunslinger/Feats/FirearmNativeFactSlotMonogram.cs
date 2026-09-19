using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.Feats
{
    // The native sheet/final-review fact slot reads Fact.Icon directly. Adapt
    // that one typed consumer, retaining the fact, parameter and fallback art.
    [HarmonyPatch]
    internal static class FirearmNativeFactSlotMonogram
    {
        private static MethodBase TargetMethod() => typeof(CharSComponentAbilitySlot)
            .GetMethod("SetFeature", new[] { typeof(Feature) });

        private static void Postfix(CharSComponentAbilitySlot __instance, Feature feature,
            TextMeshProUGUI ___m_AcronimText)
        {
            if (__instance == null || !ReferenceEquals(__instance.Feature, feature)) return;
            Present(__instance, feature, ___m_AcronimText, false);
        }

        internal static void Present(CharSComponentAbilitySlot slot, Feature feature,
            TextMeshProUGUI glyph, bool totalData)
        {
            if (feature == null || slot == null || glyph == null) return;
            string letter;
            var root = feature.Blueprint as BlueprintParametrizedFeature;
            if (!(root != null && NativeFirearmFeatIntegration.IsIntegrated(root)) &&
                !FirearmNativeMonogramPresentation.TryGetRapidLetter(feature.Blueprint, out letter)) return;
            if (!FirearmNativeMonogramPresentation.TryGetLetter(new FeatureUIData(feature), out letter)) return;
            slot.SetIcon(null);
            // SetData always restores its round border; the native Total feat
            // block may subsequently call DeleteMask. Preserve that sequence.
            if (totalData) slot.SetBorder(false);
            glyph.text = letter;
        }
    }

    [HarmonyPatch]
    internal static class FirearmNativeTotalSlotMonogram
    {
        private static MethodBase TargetMethod() => typeof(CharSComponentAbilitySlot)
            .GetMethod("SetData", new[] { typeof(IUIDataProvider) });

        private static void Postfix(CharSComponentAbilitySlot __instance, IUIDataProvider dataProvider,
            TextMeshProUGUI ___m_AcronimText)
        {
            if (__instance == null || !ReferenceEquals(__instance.IUIDataProvider, dataProvider)) return;
            FirearmNativeFactSlotMonogram.Present(__instance, dataProvider as Feature, ___m_AcronimText, true);
        }
    }

    // The native Total list has a disabled preferred-size fitter, leaving its
    // nested feat grid below the scroll extent. Activate only vertical fitting
    // for this exact firearm-containing list, without moving individual rows.
    [HarmonyPatch(typeof(CharBNewAbilities), "FillData")]
    internal static class FirearmNativeTotalLayout
    {
        private static void Prefix(CharBNewAbilities __instance)
        {
            foreach (var fit in __instance.GetComponentsInChildren<FirearmNativeTotalFit>(true)) fit.Restore();
        }

        private static void Postfix(CharBNewAbilities __instance, LevelUpController levelUpController)
        {
            if (levelUpController?.Preview == null) return;
            foreach (var slot in __instance.GetComponentsInChildren<CharSComponentAbilitySlot>(true))
            {
                var fact = slot.IUIDataProvider as Feature;
                string letter;
                if (fact == null || !ReferenceEquals(fact.Owner, levelUpController.Preview) ||
                    !FirearmNativeMonogramPresentation.TryGetLetter(new FeatureUIData(fact), out letter)) continue;
                var scroll = slot.GetComponentsInParent<ScrollRect>(true).FirstOrDefault(value =>
                    value.vertical && value.content != null && slot.transform.IsChildOf(value.content) &&
                    value.transform.IsChildOf(__instance.transform));
                if (scroll == null) continue;
                var fitter = scroll.content.GetComponent<ContentSizeFitterExtended>();
                if (fitter == null || fitter.enabled ||
                    FirearmNativeTotalFit.Mode(fitter) != ContentSizeFitterExtended.FitMode.PreferredSize) continue;
                var fit = scroll.content.GetComponent<FirearmNativeTotalFit>() ??
                    scroll.content.gameObject.AddComponent<FirearmNativeTotalFit>();
                fit.Apply(fitter);
            }
        }
    }

    internal sealed class FirearmNativeTotalFit : MonoBehaviour
    {
        private static readonly FieldInfo VerticalFit = typeof(ContentSizeFitterExtended)
            .GetField("m_VerticalFit", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo HorizontalFit = typeof(ContentSizeFitterExtended)
            .GetField("m_HorizontalFit", BindingFlags.Instance | BindingFlags.NonPublic);
        private ContentSizeFitterExtended _fitter;
        internal bool Applied { get; private set; }
        internal ContentSizeFitterExtended.FitMode Original { get; private set; }
        internal ContentSizeFitterExtended.FitMode OriginalHorizontal { get; private set; }
        internal bool OriginalEnabled { get; private set; }
        internal bool Restored => _fitter != null && !Applied && _fitter.enabled == OriginalEnabled &&
            Mode(_fitter) == Original && Mode(_fitter, false) == OriginalHorizontal;
        internal static ContentSizeFitterExtended.FitMode Mode(ContentSizeFitterExtended fitter, bool vertical = true) =>
            (ContentSizeFitterExtended.FitMode)(vertical ? VerticalFit : HorizontalFit).GetValue(fitter);

        internal void Apply(ContentSizeFitterExtended fitter)
        {
            if (Applied) return;
            _fitter = fitter;
            Original = Mode(fitter);
            OriginalHorizontal = Mode(fitter, false);
            OriginalEnabled = fitter.enabled;
            HorizontalFit.SetValue(fitter, ContentSizeFitterExtended.FitMode.Unconstrained);
            fitter.enabled = true;
            Applied = true;
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }

        internal void Restore()
        {
            if (!Applied) return;
            if (_fitter != null && _fitter.enabled && Mode(_fitter) == Original &&
                Mode(_fitter, false) == ContentSizeFitterExtended.FitMode.Unconstrained)
            {
                _fitter.enabled = OriginalEnabled;
                HorizontalFit.SetValue(_fitter, OriginalHorizontal);
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
            }
            Applied = false;
        }

        private void OnDisable() => Restore();
        private void OnDestroy() => Restore();
    }

    [HarmonyPatch(typeof(CharacterBuildController), "OnHide")]
    internal static class FirearmNativeTotalLayoutCleanup
    {
        private static void Postfix(CharacterBuildController __instance)
        {
            foreach (var fit in __instance.GetComponentsInChildren<FirearmNativeTotalFit>(true)) fit.Restore();
        }
    }
}
