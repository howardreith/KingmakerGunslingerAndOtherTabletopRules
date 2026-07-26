using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Registers the shared firearm-proficiency fact used by firearm equipment
    /// restrictions and future class progression. The fact currently grants the three
    /// player-facing exact-firearm actions: Reload, Wrecked-to-Broken Overhaul, and
    /// Broken-to-Normal Repair.
    /// </summary>
    internal static class FirearmProficiencyBlueprints
    {
        internal const string Symbol = "KMG.Firearms.FirearmProficiency";
        internal const string InternalName = "KMG_FirearmProficiency_Feature";
        internal const string AbilityGrantComponentName = "$KMG_GrantFirearmAbilities";

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

        internal static void AttachAbilities(
            BlueprintFeature feature,
            BlueprintAbility reloadAbility,
            BlueprintAbility overhaulAbility,
            BlueprintAbility repairAbility)
        {
            ValidateBase(feature);
            if (reloadAbility == null)
            {
                throw new ArgumentNullException("reloadAbility");
            }

            if (overhaulAbility == null)
            {
                throw new ArgumentNullException("overhaulAbility");
            }

            if (repairAbility == null)
            {
                throw new ArgumentNullException("repairAbility");
            }

            if (ReferenceEquals(reloadAbility, overhaulAbility) ||
                ReferenceEquals(reloadAbility, repairAbility) ||
                ReferenceEquals(overhaulAbility, repairAbility))
            {
                throw new ArgumentException(
                    "Firearm Proficiency requires distinct Reload, Overhaul, and Repair abilities.");
            }

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
                overhaulAbility,
                repairAbility
            };
            addFacts.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] { addFacts };
            Validate(feature, reloadAbility, overhaulAbility, repairAbility);
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
            BlueprintAbility overhaulAbility,
            BlueprintAbility repairAbility)
        {
            ValidateBase(feature);
            if (reloadAbility == null)
            {
                throw new ArgumentNullException("reloadAbility");
            }

            if (overhaulAbility == null)
            {
                throw new ArgumentNullException("overhaulAbility");
            }

            if (repairAbility == null)
            {
                throw new ArgumentNullException("repairAbility");
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
                !ReferenceEquals(grant.Facts[1], overhaulAbility) ||
                !ReferenceEquals(grant.Facts[2], repairAbility))
            {
                throw new InvalidOperationException(
                    "The Firearm Proficiency ability grant has incorrect identity, restore policy, order, or target abilities.");
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
