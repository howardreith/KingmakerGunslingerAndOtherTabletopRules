using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Registers the shared firearm-proficiency fact used by firearm equipment
    /// restrictions and class progression. The fact grants Reload; Gunsmithing owns
    /// the separate maintenance actions.
    /// </summary>
    internal static class FirearmProficiencyBlueprints
    {
        internal const string Symbol = "KMG.Firearms.FirearmProficiency";
        internal const string InternalName = "KMG_FirearmProficiency_Feature";
        internal const string AbilityGrantComponentName = "$KMG_GrantFirearmReload";

        internal static BlueprintFeature Register(BlueprintRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException("registry");
            }

            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                Symbol,
                CreateFeature);
            ValidateBase(feature);
            return feature;
        }

        internal static void AttachReload(
            BlueprintFeature feature,
            BlueprintAbility reloadAbility,
            BlueprintAbility scatterShotAbility,
            BlueprintActivatableAbility paperCartridgeMode)
        {
            ValidateBase(feature);
            if (reloadAbility == null)
            {
                throw new ArgumentNullException("reloadAbility");
            }
            if (scatterShotAbility == null) throw new ArgumentNullException("scatterShotAbility");
            if (paperCartridgeMode == null) throw new ArgumentNullException("paperCartridgeMode");

            if (feature.ComponentsArray.Length != 0)
            {
                throw new InvalidOperationException(
                    "Firearm Proficiency already has components before its ability grant was attached.");
            }

            AddFacts addFacts = ScriptableObject.CreateInstance<AddFacts>();
            addFacts.name = AbilityGrantComponentName;
            addFacts.Facts = new BlueprintUnitFact[]
            {
                reloadAbility,
                scatterShotAbility,
                paperCartridgeMode
            };
            addFacts.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] { addFacts };
            Validate(feature, reloadAbility, scatterShotAbility, paperCartridgeMode);
        }

        internal static void ValidateBase(BlueprintFeature feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException("feature");
            }

            if (!string.Equals(feature.name, InternalName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The firearm-proficiency feature has an unexpected internal name.");
            }

            if (!feature.HideInUI)
            {
                throw new InvalidOperationException(
                    "Firearm proficiency must remain hidden until localized player-facing class content is added.");
            }

            if (feature.Ranks != 1)
            {
                throw new InvalidOperationException(
                    "Firearm proficiency must have exactly one rank.");
            }

            if (feature.ComponentsArray == null)
            {
                throw new InvalidOperationException(
                    "Firearm proficiency must expose a non-null component array.");
            }
        }

        internal static void Validate(
            BlueprintFeature feature,
            BlueprintAbility reloadAbility,
            BlueprintAbility scatterShotAbility,
            BlueprintActivatableAbility paperCartridgeMode)
        {
            ValidateBase(feature);
            if (reloadAbility == null)
            {
                throw new ArgumentNullException("reloadAbility");
            }

            AddFacts[] grants = feature.ComponentsArray.OfType<AddFacts>().ToArray();
            if (feature.ComponentsArray.Length != 1 || grants.Length != 1)
            {
                throw new InvalidOperationException(
                    "Firearm Proficiency must contain exactly one AddFacts ability grant.");
            }

            AddFacts grant = grants[0];
            if (!string.Equals(
                    grant.name,
                    AbilityGrantComponentName,
                    StringComparison.Ordinal) ||
                grant.DoNotRestoreMissingFacts ||
                grant.Facts == null ||
                grant.Facts.Length != 3 ||
                !ReferenceEquals(grant.Facts[0], reloadAbility) ||
                !ReferenceEquals(grant.Facts[1], scatterShotAbility) ||
                !ReferenceEquals(grant.Facts[2], paperCartridgeMode))
            {
                throw new InvalidOperationException(
                    "The Firearm Proficiency Reload grant has incorrect identity, restore policy, or target ability.");
            }
        }

        private static BlueprintFeature CreateFeature()
        {
            BlueprintFeature feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = InternalName;
            feature.Ranks = 1;
            feature.HideInUI = true;
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            ValidateBase(feature);
            return feature;
        }
    }
}
