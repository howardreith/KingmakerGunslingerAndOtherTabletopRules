using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// GS-AUD-0034 native boundary observation.
        ///
        /// The repair is that neither postfix on
        /// <c>BlueprintParametrizedFeature.ExtractSelectionItems</c> may discard
        /// an <c>IFeatureSelectionItem</c> it does not recognise. The engine-free
        /// rule is covered by domain tests; what was missing is the REAL
        /// handlers being exercised on the installed engine's own types.
        ///
        /// Both production postfixes are invoked directly here, by reflection
        /// because each is private, with a host sequence that mixes genuine
        /// native <c>FeatureUIData</c> rows with a SYNTHETIC foreign
        /// implementation. The synthetic row is labelled as such in the
        /// evidence: no installed mod has been shown to supply a non-FeatureUIData
        /// item, so this demonstrates boundary preservation and NOT the repair of
        /// an observed third-party breakage.
        ///
        /// Every assertion inspects the complete <c>IFeatureSelectionItem</c>
        /// sequence. A projection through <c>OfType&lt;FeatureUIData&gt;()</c>
        /// would silently drop the control and could not detect the defect at
        /// all - that projection is exactly what the repair removed.
        ///
        /// This is a direct handler observation. It is deliberately separate
        /// from the scenario's existing native selection-flow evidence, which
        /// enumerates through the engine and selects a returned row through
        /// <c>FeatureSelectionState</c>; the two are different kinds of evidence
        /// and are recorded separately.
        /// </summary>
        private sealed class SyntheticForeignSelectionItem : IFeatureSelectionItem
        {
            internal SyntheticForeignSelectionItem(string label) { Label = label; }
            internal string Label { get; private set; }
            // IFeatureSelectionItem declares Feature, Param and NameForAcronim
            // and inherits Name, Description and Icon from IUIDataProvider on
            // the installed engine. Nulls are legitimate here: this stands in
            // for another publisher's row, and nothing in the boundary rule may
            // dereference it.
            public BlueprintFeature Feature { get { return null; } }
            public FeatureParam Param { get { return null; } }
            public string NameForAcronim { get { return Label; } }
            public string Name { get { return Label; } }
            public string Description { get { return "Synthetic foreign selection row."; } }
            public UnityEngine.Sprite Icon { get { return null; } }
        }

        private static IFeatureSelectionItem[] InvokeSelectionPostfix(Type patchType,
            BlueprintParametrizedFeature instance, UnitDescriptor before,
            UnitDescriptor preview, IEnumerable<IFeatureSelectionItem> host)
        {
            MethodInfo postfix = patchType.GetMethod("Postfix",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (postfix == null) throw new InvalidOperationException(
                "No Postfix on " + patchType.FullName + "; the patch shape changed.");
            object[] arguments = { instance, before, preview, host };
            postfix.Invoke(null, arguments);
            // The ref parameter is read back from the argument array.
            return ((IEnumerable<IFeatureSelectionItem>)arguments[3] ??
                Enumerable.Empty<IFeatureSelectionItem>()).ToArray();
        }

        private void ObserveSelectionBoundaryHandlers(
            List<RuntimeTestAssertion> assertions,
            BlueprintParametrizedFeature contributing)
        {
            // Its own disposable chargen owner, so this observation does not
            // depend on another stage's unit lifetime.
            var owner = new Kingmaker.UI.LevelUp.ChargenUnit(
                Kingmaker.Blueprints.Root.BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            try
            {
                if (owner.Descriptor.AddFact(BlueprintBootstrap.FirearmProficiency) == null)
                    throw new InvalidOperationException(
                        "The boundary observation's owner could not receive full proficiency.");
                ObserveSelectionBoundaryHandlersCore(assertions, owner.Descriptor, contributing);
            }
            finally { owner.Dispose(); }
        }

        private void ObserveSelectionBoundaryHandlersCore(
            List<RuntimeTestAssertion> assertions, UnitDescriptor descriptor,
            BlueprintParametrizedFeature contributing)
        {
            // A parametrized feature neither handler extends, found by asking
            // the handlers themselves rather than by assuming a name. Its rows
            // are therefore pure native rows, which is what makes them a sound
            // host sequence for the direct test.
            BlueprintParametrizedFeature inert = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintParametrizedFeature>()
                .FirstOrDefault(feature => feature != null &&
                    !ReferenceEquals(feature, contributing) &&
                    CustomWeapons.CustomWeaponSelectorRuntime.Append(feature,
                        Enumerable.Empty<FeatureUIData>()).Count() == 0 &&
                    Feats.NativeFirearmFeatIntegration.Append(feature,
                        Enumerable.Empty<FeatureUIData>(), descriptor).Count() == 0);
            if (inert == null) throw new InvalidOperationException(
                "No parametrized feature is left unextended by both handlers; " +
                "the no-contribution control cannot be constructed honestly.");

            FeatureUIData[] nativeRows = inert
                .ExtractSelectionItems(descriptor, descriptor)
                .OfType<FeatureUIData>().Where(value => value != null)
                .Take(3).ToArray();
            if (nativeRows.Length == 0) throw new InvalidOperationException(
                "The inert parametrized feature offered no native rows to compose with.");

            var foreignFirst = new SyntheticForeignSelectionItem("KMG_SYNTHETIC_FOREIGN_1");
            var foreignSecond = new SyntheticForeignSelectionItem("KMG_SYNTHETIC_FOREIGN_2");
            // Interleaved deliberately: a handler that appended the unfamiliar
            // rows by rebuilding the list could still pass if they started at
            // the end.
            var host = new List<IFeatureSelectionItem> { foreignFirst };
            host.AddRange(nativeRows.Cast<IFeatureSelectionItem>());
            host.Add(foreignSecond);
            IFeatureSelectionItem[] hostArray = host.ToArray();

            foreach (var handler in new[] {
                Tuple.Create("custom-weapon",
                    typeof(CustomWeapons.CustomWeaponLevelUpSelectorPatch)),
                Tuple.Create("native-firearm-feat",
                    typeof(Feats.NativeFirearmFeatLevelUpMenuPatch)) })
            {
                string id = handler.Item1;
                Type patch = handler.Item2;

                // ---- contribution ----
                IFeatureSelectionItem[] composed = InvokeSelectionPostfix(
                    patch, contributing, descriptor, descriptor, hostArray);
                IFeatureSelectionItem[] foreignSurvivors = composed
                    .Where(value => value is SyntheticForeignSelectionItem).ToArray();
                FeatureUIData[] composedUnderstood = composed
                    .OfType<FeatureUIData>().ToArray();
                string[] addedNames = composedUnderstood
                    .Where(value => !nativeRows.Any(native => ReferenceEquals(native, value)))
                    .Select(value => value.Name ?? string.Empty).ToArray();
                bool addedSomething = addedNames.Length > 0;
                bool noDuplicateReferences = composed.Length ==
                    composed.Distinct().Count();
                bool nativeRowsKept = nativeRows.All(native =>
                    composed.Any(value => ReferenceEquals(value, native)));
                // Preserved BY REFERENCE and in their original relative order.
                bool foreignByReference = foreignSurvivors.Length == 2 &&
                    ReferenceEquals(foreignSurvivors[0], foreignFirst) &&
                    ReferenceEquals(foreignSurvivors[1], foreignSecond);
                // The composing caller's own contract for understood rows.
                string[] understoodNames = composedUnderstood
                    .Select(value => value.Name ?? string.Empty).ToArray();
                bool understoodOrdered = understoodNames
                    .SequenceEqual(understoodNames.OrderBy(value => value,
                        StringComparer.Ordinal));

                assertions.Add(Assertion(
                    "selection-boundary-native-" + id + "-contribution",
                    "the real postfix contributes its own rows, keeps every native row, preserves " +
                        "both synthetic foreign rows BY REFERENCE and in their original relative " +
                        "order, introduces no duplicate reference, and leaves the understood rows " +
                        "in the composing caller's alphabetical contract. Asserted on the complete " +
                        "IFeatureSelectionItem sequence, never a FeatureUIData projection",
                    "hostRows=" + hostArray.Length + ";composedRows=" + composed.Length +
                        ";added=" + addedNames.Length + ";nativeKept=" + nativeRowsKept +
                        ";foreignSurvivors=" + foreignSurvivors.Length +
                        ";foreignByReference=" + foreignByReference +
                        ";noDuplicates=" + noDuplicateReferences +
                        ";understoodAlphabetical=" + understoodOrdered,
                    addedSomething && nativeRowsKept && foreignByReference &&
                        noDuplicateReferences && understoodOrdered,
                    "the production " + patch.FullName + ".Postfix invoked directly on native " +
                        "FeatureUIData rows mixed with a SYNTHETIC foreign IFeatureSelectionItem; " +
                        "no installed mod has been shown to supply such a row"));

                // ---- no contribution ----
                IFeatureSelectionItem[] untouched = InvokeSelectionPostfix(
                    patch, inert, descriptor, descriptor, hostArray);
                bool identicalSequence = untouched.Length == hostArray.Length &&
                    !untouched.Where((value, index) =>
                        !ReferenceEquals(value, hostArray[index])).Any();
                assertions.Add(Assertion(
                    "selection-boundary-native-" + id + "-no-contribution",
                    "when the real postfix contributes nothing, the host sequence is returned with " +
                        "every entry and its exact order intact - including the interleaved " +
                        "synthetic foreign rows, which the superseded " +
                        "OfType<FeatureUIData>() narrowing dropped silently",
                    "hostRows=" + hostArray.Length + ";returnedRows=" + untouched.Length +
                        ";referenceIdenticalInOrder=" + identicalSequence,
                    identicalSequence,
                    "the production " + patch.FullName + ".Postfix invoked directly with a " +
                        "parametrized feature neither handler extends"));
            }

            // ---- both handlers composed, in the order Harmony would apply them ----
            IFeatureSelectionItem[] first = InvokeSelectionPostfix(
                typeof(CustomWeapons.CustomWeaponLevelUpSelectorPatch),
                contributing, descriptor, descriptor, hostArray);
            IFeatureSelectionItem[] both = InvokeSelectionPostfix(
                typeof(Feats.NativeFirearmFeatLevelUpMenuPatch),
                contributing, descriptor, descriptor, first);
            IFeatureSelectionItem[] bothForeign = both
                .Where(value => value is SyntheticForeignSelectionItem).ToArray();
            bool bothKeepForeign = bothForeign.Length == 2 &&
                ReferenceEquals(bothForeign[0], foreignFirst) &&
                ReferenceEquals(bothForeign[1], foreignSecond);
            bool bothNoDuplicates = both.Length == both.Distinct().Count();
            assertions.Add(Assertion("selection-boundary-native-composed",
                "running both real postfixes in sequence still preserves both synthetic foreign " +
                    "rows by reference and in order, and introduces no duplicate reference - the " +
                    "second handler must not discard what the first composed",
                "afterFirst=" + first.Length + ";afterBoth=" + both.Length +
                    ";foreignSurvivors=" + bothForeign.Length +
                    ";foreignByReference=" + bothKeepForeign +
                    ";noDuplicates=" + bothNoDuplicates,
                bothKeepForeign && bothNoDuplicates && both.Length >= first.Length,
                "both production postfixes invoked directly, chained as Harmony would apply them"));
        }
    }
}
