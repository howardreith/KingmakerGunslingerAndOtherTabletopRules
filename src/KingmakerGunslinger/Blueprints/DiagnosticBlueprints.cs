using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// The bootstrap's only custom blueprint. The feature is deliberately unreferenced,
    /// hidden, component-free, and therefore neither selectable nor behavior-bearing.
    /// </summary>
    internal static class DiagnosticBlueprints
    {
        internal const string InitializedFeatureSymbol = "KMG.Diagnostic.InitializedFeature";
        private const string InitializedFeatureName = "KMG_Diagnostic_InitializedFeature";

        internal static BlueprintFeature Register(BlueprintRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException("registry");
            }

            return registry.Register<BlueprintFeature>(
                InitializedFeatureSymbol,
                CreateInitializedFeature);
        }

        internal static void Validate(BlueprintFeature feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException("feature");
            }

            if (!feature.HideInUI)
            {
                throw new InvalidOperationException("The diagnostic feature must remain hidden in the UI.");
            }

            if (feature.Ranks != 1)
            {
                throw new InvalidOperationException("The diagnostic feature must have exactly one rank.");
            }

            if (feature.ComponentsArray == null || feature.ComponentsArray.Length != 0)
            {
                throw new InvalidOperationException("The diagnostic feature must remain component-free.");
            }
        }

        private static BlueprintFeature CreateInitializedFeature()
        {
            BlueprintFeature feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = InitializedFeatureName;
            feature.Ranks = 1;
            feature.HideInUI = true;
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            Validate(feature);
            return feature;
        }
    }
}
