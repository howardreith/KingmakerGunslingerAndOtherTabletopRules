using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Removes the firearm proficiency scopes its presentation does not grant
    /// (FirearmProficiencyScopeRules), by blueprint identity, sparing the full
    /// proficiency of a legacy-wrapper owner. See FirearmProficiencyScopeRules
    /// for the extension boundary a future independent provider must respect.
    /// </summary>
    public sealed class FirearmProficiencyScopeReconciler :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintFeature ExpectedProficiency;
        public BlueprintFeature FullProficiency;
        public BlueprintFeature LegacyCompatibilityWrapper;
        public BlueprintFeature[] IncompatibleProficiencies;

        public override void OnTurnOn()
        {
            if (Owner == null || ExpectedProficiency == null ||
                FullProficiency == null || LegacyCompatibilityWrapper == null ||
                IncompatibleProficiencies == null ||
                IncompatibleProficiencies.Any(value => value == null))
                throw new InvalidOperationException(
                    "Firearm proficiency scope reconciliation is incomplete.");
            foreach (BlueprintFeature incompatible in IncompatibleProficiencies)
            {
                if (ReferenceEquals(incompatible, FullProficiency) &&
                    Owner.HasFact(LegacyCompatibilityWrapper))
                    continue;
                if (Owner.HasFact(incompatible)) Owner.RemoveFact(incompatible);
            }
        }

        public override void OnTurnOff() { }
    }

    internal sealed class FirearmScopedProficiencyBlueprintSet
    {
        internal FirearmScopedProficiencyBlueprintSet(BlueprintFeature oneHanded,
            BlueprintFeature twoHanded)
        {
            OneHanded = oneHanded ?? throw new ArgumentNullException("oneHanded");
            TwoHanded = twoHanded ?? throw new ArgumentNullException("twoHanded");
        }

        internal BlueprintFeature OneHanded { get; private set; }
        internal BlueprintFeature TwoHanded { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class FirearmScopedProficiencyBlueprints
    {
        internal const string OneHandedSymbol =
            "KMG.Firearms.OneHandedFirearmProficiency";
        internal const string TwoHandedSymbol =
            "KMG.Firearms.TwoHandedFirearmProficiency";

        internal static FirearmScopedProficiencyBlueprintSet Register(
            BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintFeature oneHanded = registry.Register<BlueprintFeature>(
                OneHandedSymbol, () => Create("OneHanded"));
            BlueprintFeature twoHanded = registry.Register<BlueprintFeature>(
                TwoHandedSymbol, () => Create("TwoHanded"));
            return new FirearmScopedProficiencyBlueprintSet(oneHanded, twoHanded);
        }

        internal static void AttachActions(
            FirearmScopedProficiencyBlueprintSet set,
            BlueprintAbility reload, BlueprintAbility scatter,
            BlueprintActivatableAbility paperCartridgeMode)
        {
            if (set == null) throw new ArgumentNullException("set");
            if (reload == null) throw new ArgumentNullException("reload");
            if (scatter == null) throw new ArgumentNullException("scatter");
            if (paperCartridgeMode == null) throw new ArgumentNullException("paperCartridgeMode");
            Attach(set.OneHanded, reload, paperCartridgeMode);
            Attach(set.TwoHanded, reload, scatter, paperCartridgeMode);
            Validate(set, reload, scatter, paperCartridgeMode);
        }

        internal static void AttachScopeReconciliation(
            BlueprintFeature basePresentation,
            ArchetypeProficiencyBlueprintSet archetypePresentations,
            BlueprintFeature full,
            FirearmScopedProficiencyBlueprintSet scoped,
            BlueprintFeature legacyCompatibilityWrapper)
        {
            if (basePresentation == null || archetypePresentations == null ||
                full == null || scoped == null ||
                legacyCompatibilityWrapper == null)
                throw new ArgumentNullException("scope reconciliation blueprint");
            Func<FirearmProficiencyScope, BlueprintFeature> feature = scope =>
                scope == FirearmProficiencyScope.Full ? full :
                scope == FirearmProficiencyScope.OneHanded ? scoped.OneHanded : scoped.TwoHanded;
            Func<FirearmProficiencyPresentation, BlueprintFeature[]> incompatible = presentation =>
                FirearmProficiencyScopeRules.Incompatible(presentation).Select(feature).ToArray();
            AppendReconciler(basePresentation,
                feature(FirearmProficiencyScopeRules.Expected(FirearmProficiencyPresentation.Gunslinger)),
                full, legacyCompatibilityWrapper, incompatible(FirearmProficiencyPresentation.Gunslinger));
            AppendReconciler(archetypePresentations.Pistolero,
                feature(FirearmProficiencyScopeRules.Expected(FirearmProficiencyPresentation.Pistolero)),
                full, legacyCompatibilityWrapper, incompatible(FirearmProficiencyPresentation.Pistolero));
            AppendReconciler(archetypePresentations.MusketMaster,
                feature(FirearmProficiencyScopeRules.Expected(FirearmProficiencyPresentation.MusketMaster)),
                full, legacyCompatibilityWrapper, incompatible(FirearmProficiencyPresentation.MusketMaster));
        }

        private static void AppendReconciler(BlueprintFeature presentation,
            BlueprintFeature expected, BlueprintFeature full,
            BlueprintFeature legacyCompatibilityWrapper,
            params BlueprintFeature[] incompatible)
        {
            var component = ScriptableObject.CreateInstance<
                FirearmProficiencyScopeReconciler>();
            component.name = "$KMG_ReconcileFirearmProficiencyScope";
            component.ExpectedProficiency = expected;
            component.FullProficiency = full;
            component.LegacyCompatibilityWrapper = legacyCompatibilityWrapper;
            component.IncompatibleProficiencies = incompatible.ToArray();
            presentation.ComponentsArray = (presentation.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Concat(new BlueprintComponent[]
                { component }).ToArray();
        }

        internal static void Validate(FirearmScopedProficiencyBlueprintSet set,
            BlueprintAbility reload, BlueprintAbility scatter,
            BlueprintActivatableAbility paperCartridgeMode)
        {
            ValidateOne(set.OneHanded, reload, paperCartridgeMode);
            ValidateOne(set.TwoHanded, reload, scatter, paperCartridgeMode);
        }

        private static BlueprintFeature Create(string suffix)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_" + suffix + "FirearmProficiency_Feature";
            feature.Ranks = 1;
            feature.HideInUI = true;
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            return feature;
        }

        private static void Attach(BlueprintFeature feature,
            params BlueprintUnitFact[] abilities)
        {
            if (feature.ComponentsArray == null || feature.ComponentsArray.Length != 0)
                throw new InvalidOperationException(
                    "Scoped firearm proficiency actions were already attached.");
            var facts = ScriptableObject.CreateInstance<AddFacts>();
            facts.name = "$KMG_GrantScopedFirearmActions";
            facts.Facts = abilities.ToArray();
            facts.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] { facts };
        }

        private static void ValidateOne(BlueprintFeature feature,
            params BlueprintUnitFact[] expected)
        {
            AddFacts[] grants = (feature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AddFacts>().ToArray();
            if (feature.Ranks != 1 || !feature.HideInUI || grants.Length != 1 ||
                grants[0].DoNotRestoreMissingFacts ||
                grants[0].Facts == null || grants[0].Facts.Length != expected.Length)
                throw new InvalidOperationException(
                    "Scoped firearm proficiency action grant is incomplete.");
            for (int index = 0; index < expected.Length; index++)
                if (!ReferenceEquals(grants[0].Facts[index], expected[index]))
                    throw new InvalidOperationException(
                        "Scoped firearm proficiency action identity changed.");
        }
    }
}
