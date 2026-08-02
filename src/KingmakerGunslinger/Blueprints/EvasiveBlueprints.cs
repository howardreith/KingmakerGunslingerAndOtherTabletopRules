using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class EvasiveBlueprintSet
    {
        internal EvasiveBlueprintSet(BlueprintFeature feature,
            BlueprintFeature evasion, BlueprintFeature uncanny,
            BlueprintFeature improved)
        { Feature = feature; Evasion = evasion; UncannyDodge = uncanny;
          ImprovedUncannyDodge = improved; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintFeature Evasion { get; private set; }
        internal BlueprintFeature UncannyDodge { get; private set; }
        internal BlueprintFeature ImprovedUncannyDodge { get; private set; }
        internal int Count { get { return 4; } }
    }

    internal static class EvasiveBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.EvasiveFeature";
        internal const string EvasionSymbol = "KMG.Deeds.EvasiveEvasionBenefit";
        internal const string UncannySymbol = "KMG.Deeds.EvasiveUncannyDodgeBenefit";
        internal const string ImprovedSymbol =
            "KMG.Deeds.EvasiveImprovedUncannyDodgeBenefit";
        private const string EvasionGuid = "576933720c440aa4d8d42b0c54b77e80";
        private const string UncannyGuid = "3c08d842e802c3e4eb19d15496145709";
        private const string ImprovedGuid = "485a18c05792521459c7d06c63128c79";

        internal static EvasiveBlueprintSet Register(LibraryScriptableObject library,
            BlueprintRegistry registry, BlueprintAbilityResource grit,
            BlueprintCharacterClass gunslingerClass)
        {
            BlueprintFeature nativeEvasion = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library, EvasionGuid,
                    "native Evasion feature");
            BlueprintFeature nativeUncanny = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library, UncannyGuid,
                    "native Uncanny Dodge feature");
            BlueprintFeature nativeImproved = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library, ImprovedGuid,
                    "native Improved Uncanny Dodge feature");
            BlueprintFeature evasion = registry.Register<BlueprintFeature>(
                EvasionSymbol, () => CloneBenefit(nativeEvasion,
                    "KMG_Evasive_Evasion"));
            BlueprintFeature uncanny = registry.Register<BlueprintFeature>(
                UncannySymbol, () => CloneBenefit(nativeUncanny,
                    "KMG_Evasive_UncannyDodge"));
            BlueprintFeature improved = registry.Register<BlueprintFeature>(
                ImprovedSymbol, () => CloneBenefit(nativeImproved,
                    "KMG_Evasive_ImprovedUncannyDodge"));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(grit, gunslingerClass,
                    evasion, uncanny, improved));
            Validate(feature, evasion, uncanny, improved);
            return new EvasiveBlueprintSet(feature, evasion, uncanny, improved);
        }

        private static BlueprintFeature CloneBenefit(BlueprintFeature source,
            string name)
        {
            BlueprintFeature result = BlueprintCloneService.Clone(source, name);
            result.HideInUI = true;
            result.IsClassFeature = true;
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbilityResource grit,
            BlueprintCharacterClass gunslingerClass, BlueprintFeature evasion,
            BlueprintFeature uncanny, BlueprintFeature improved)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_Evasive_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var controller = ScriptableObject.CreateInstance<EvasiveGrantController>();
            controller.name = "$KMG_Evasive_ConditionalGrants";
            controller.Grit = grit; controller.GunslingerClass = gunslingerClass;
            controller.EvasionBenefit = evasion;
            controller.UncannyDodgeBenefit = uncanny;
            controller.ImprovedUncannyDodgeBenefit = improved;
            result.ComponentsArray = new BlueprintComponent[] { controller };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Evasive.Feature.Name", "Evasive"),
                LocalizationService.Create("KMG.Evasive.Feature.Description",
                    "While you have at least 1 grit, gain Evasion, Uncanny Dodge, and Improved Uncanny Dodge."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintFeature evasion, BlueprintFeature uncanny,
            BlueprintFeature improved)
        {
            EvasiveGrantController controller = feature.GetComponent<
                EvasiveGrantController>();
            if (controller == null || !ReferenceEquals(controller.EvasionBenefit,
                evasion) || !ReferenceEquals(controller.UncannyDodgeBenefit,
                uncanny) || !ReferenceEquals(
                controller.ImprovedUncannyDodgeBenefit, improved) ||
                evasion.ComponentsArray.Length != 1 ||
                uncanny.ComponentsArray.Length != 2 ||
                improved.ComponentsArray.Length != 1)
                throw new InvalidOperationException(
                    "Evasive exact native-component contract is incomplete.");
            controller.Validate();
        }
    }
}
