using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Feats;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Rapid Reload must require firearm proficiency, never Gunslinger class
        // identity. This scenario reads the registered production blueprints and
        // drives the real LevelUpController: every eligibility answer below comes
        // from the native prerequisite machinery (BlueprintFeature.MeetsPrerequisites
        // and BlueprintFeatureSelection.CanSelect through SelectFeature), never from
        // a mocked selection system or a duplicated policy formula. No production
        // prerequisite is bypassed: IgnorePrerequisites stays off and no target fact
        // is inserted directly except on the explicitly owned fixtures below.
        private const string RapidReloadBasicFeatSelectionGuid =
            "247a4068296e8be42890143f451b4b45";
        private const string RapidReloadFighterFeatSelectionGuid =
            "41c8486641f7d6d4283ca9dae4147a9f";

        private RuntimeTestResult RunRapidReloadProficiencyGate()
        {
            FirearmFeatBlueprintSet feats = BlueprintBootstrap.FirearmFeats;
            if (feats == null)
                throw new InvalidOperationException(
                    "The registered firearm feat blueprints are unavailable.");
            BlueprintFeatureSelection parent = feats.RapidReload;
            BlueprintFeature[] children = feats.RapidReloadChoices;
            BlueprintFeature[] registeredChildren = feats.RegisteredRapidReloadChoices;
            BlueprintFeature fullProficiency = BlueprintBootstrap.FirearmProficiency;
            FirearmScopedProficiencyBlueprintSet scoped =
                BlueprintBootstrap.ScopedFirearmProficiencies;
            BlueprintFeature legacyWrapper = feats.ExoticWeaponProficiency;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            FirearmKind[] kinds = RapidReloadPrerequisiteRules.ParentGateKinds;
            if (parent == null || children == null || children.Length != kinds.Length ||
                registeredChildren == null || fullProficiency == null || scoped == null ||
                legacyWrapper == null || gunslinger == null)
                throw new InvalidOperationException(
                    "The Rapid Reload gate fixture inputs are incomplete.");
            BlueprintFeatureSelection basic = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(BlueprintBootstrap.Library,
                    RapidReloadBasicFeatSelectionGuid, "native basic feat selection");
            BlueprintFeatureSelection fighterFeats = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(BlueprintBootstrap.Library,
                    RapidReloadFighterFeatSelectionGuid,
                    "native Fighter combat feat selection");
            BlueprintCharacterClass fighter = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintCharacterClass>()
                .SingleOrDefault(value => value.name == "FighterClass");
            if (fighter == null)
                throw new InvalidOperationException(
                    "The native Fighter class is unavailable for the Rapid Reload gate.");

            // --- Registered blueprint wiring -------------------------------
            var wiringFailures = new List<string>();
            JObject wiring = DescribeRapidReloadWiring(parent, children,
                registeredChildren, fullProficiency, scoped, legacyWrapper, kinds,
                wiringFailures);
            var catalogFailures = new List<string>();
            JObject catalog = DescribeRapidReloadCatalogs(parent, registeredChildren,
                legacyWrapper, basic, fighterFeats, kinds.Length, catalogFailures);

            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            var matrix = new JArray();
            var matrixFailures = new List<string>();
            var flow = new JArray();
            var flowFailures = new List<string>();
            var pendingFailures = new List<string>();
            JObject pending = null;
            var musketMasterFailures = new List<string>();
            JObject musketMaster = null;
            BlueprintFeature independentFull = null;
            BlueprintFeature independentOneHanded = null;
            BlueprintFeature independentTwoHanded = null;
            bool cleaned = false;
            bool ignoreOff = !IgnorePrerequisites.Ignore;
            try
            {
                independentFull = CreateIndependentProficiencySource(
                    "KMG_RuntimeFixture_IndependentFullFirearmProficiency",
                    fullProficiency);
                independentOneHanded = CreateIndependentProficiencySource(
                    "KMG_RuntimeFixture_IndependentOneHandedFirearmProficiency",
                    scoped.OneHanded);
                independentTwoHanded = CreateIndependentProficiencySource(
                    "KMG_RuntimeFixture_IndependentTwoHandedFirearmProficiency",
                    scoped.TwoHanded);

                // --- Native prerequisite matrix on disposable units ---------
                AddRapidReloadMatrixRow(matrix, matrixFailures, "A.no-proficiency",
                    parent, children, kinds, false, false, false, null);
                AddRapidReloadMatrixRow(matrix, matrixFailures, "C.full-proficiency",
                    parent, children, kinds, true, false, false,
                    descriptor => GrantFixtureFact(descriptor, fullProficiency));
                AddRapidReloadMatrixRow(matrix, matrixFailures, "D.one-handed",
                    parent, children, kinds, false, true, false,
                    descriptor => GrantFixtureFact(descriptor, scoped.OneHanded));
                AddRapidReloadMatrixRow(matrix, matrixFailures, "E.two-handed",
                    parent, children, kinds, false, false, true,
                    descriptor => GrantFixtureFact(descriptor, scoped.TwoHanded));
                AddRapidReloadMatrixRow(matrix, matrixFailures, "E2.both-scoped",
                    parent, children, kinds, false, true, true, descriptor =>
                    {
                        GrantFixtureFact(descriptor, scoped.OneHanded);
                        GrantFixtureFact(descriptor, scoped.TwoHanded);
                    });
                // F: an independent, test-only source granting an existing
                // proficiency fact through the real AddFacts mechanism. No
                // Gunslinger level and no legacy wrapper are involved.
                AddRapidReloadMatrixRow(matrix, matrixFailures, "F.independent-full",
                    parent, children, kinds, true, false, false,
                    descriptor => GrantFixtureFact(descriptor, independentFull));
                AddRapidReloadMatrixRow(matrix, matrixFailures,
                    "F.independent-one-handed", parent, children, kinds, false, true,
                    false,
                    descriptor => GrantFixtureFact(descriptor, independentOneHanded));
                AddRapidReloadMatrixRow(matrix, matrixFailures,
                    "F.independent-two-handed", parent, children, kinds, false, false,
                    true,
                    descriptor => GrantFixtureFact(descriptor, independentTwoHanded));
                // G: Gunslinger class identity with every firearm proficiency
                // fact removed must not qualify.
                AddRapidReloadMatrixRow(matrix, matrixFailures,
                    "G.gunslinger-identity-without-proficiency", parent, children,
                    kinds, false, false, false, descriptor =>
                    {
                        GrantFixtureFact(descriptor, gunslinger.Proficiencies);
                        RemoveFixtureFact(descriptor, fullProficiency);
                        RemoveFixtureFact(descriptor, scoped.OneHanded);
                        RemoveFixtureFact(descriptor, scoped.TwoHanded);
                    });
                // H: the preserved legacy wrapper still satisfies the gate
                // through its existing full-proficiency grant.
                AddRapidReloadMatrixRow(matrix, matrixFailures,
                    "H.legacy-wrapper-owner", parent, children, kinds, true, false,
                    false, descriptor => GrantFixtureFact(descriptor, legacyWrapper));

                // --- Real selection flow ------------------------------------
                MethodInfo start = typeof(LevelUpController).GetMethods(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Single(value => value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo apply = typeof(LevelUpController).GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object mode = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "LevelUp", false);

                // A + B: no firearm proficiency, both catalogs. The Fighter
                // level-one preview already carries native martial (crossbow)
                // proficiency, which must not unlock Rapid Reload.
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "A.ordinary-feat-no-proficiency", fighter, basic, parent,
                    children[0], null, false, flowFailures));
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "A.combat-feat-no-proficiency", fighter, fighterFeats, parent,
                    children[0], null, false, flowFailures));
                // C: full proficiency, both catalogs.
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "C.ordinary-feat-full-proficiency", fighter, basic, parent,
                    children[0],
                    descriptor => GrantFixtureFact(descriptor, fullProficiency),
                    true, flowFailures));
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "C.combat-feat-full-proficiency", fighter, fighterFeats, parent,
                    children[0],
                    descriptor => GrantFixtureFact(descriptor, fullProficiency),
                    true, flowFailures));
                // D and E keep the firearm-specific restriction inside the real
                // child menu.
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "D.one-handed-pistol", fighter, basic, parent, children[0],
                    descriptor => GrantFixtureFact(descriptor, scoped.OneHanded), true,
                    flowFailures));
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "D.one-handed-musket-refused", fighter, basic, parent, children[1],
                    descriptor => GrantFixtureFact(descriptor, scoped.OneHanded), false,
                    flowFailures));
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "D.one-handed-blunderbuss-refused", fighter, basic, parent,
                    children[2],
                    descriptor => GrantFixtureFact(descriptor, scoped.OneHanded),
                    false, flowFailures));
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "E.two-handed-musket", fighter, basic, parent, children[1],
                    descriptor => GrantFixtureFact(descriptor, scoped.TwoHanded), true,
                    flowFailures));
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "E.two-handed-blunderbuss", fighter, basic, parent, children[2],
                    descriptor => GrantFixtureFact(descriptor, scoped.TwoHanded), true,
                    flowFailures));
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "E.two-handed-pistol-refused", fighter, basic, parent, children[0],
                    descriptor => GrantFixtureFact(descriptor, scoped.TwoHanded), false,
                    flowFailures));
                // F: the decisive future-proofing regression through the normal
                // feat-selection flow on a character with no Gunslinger level.
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "F.independent-source-full", fighter, basic, parent, children[0],
                    descriptor => GrantFixtureFact(descriptor, independentFull), true,
                    flowFailures));
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "F.independent-source-two-handed", fighter, basic, parent,
                    children[1],
                    descriptor => GrantFixtureFact(descriptor, independentTwoHanded),
                    true, flowFailures));
                // H: the legacy wrapper owner keeps its acquisition route.
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "H.legacy-wrapper-owner", fighter, basic, parent, children[0],
                    descriptor => GrantFixtureFact(descriptor, legacyWrapper), true,
                    flowFailures));
                // I: a fresh level-one Gunslinger qualifies from its class
                // proficiency package alone, inside the level-up visit.
                flow.Add(RunRapidReloadSelectionAttempt(start, apply, mode,
                    "I.fresh-level-one-gunslinger", gunslinger.CharacterClass, basic,
                    parent, children[0], null, true, flowFailures));

                pending = RunRapidReloadPendingClassChange(start, apply, mode,
                    gunslinger.CharacterClass, fighter, basic, parent, children[0],
                    pendingFailures);
                musketMaster = DescribeMusketMasterRapidReload(gunslinger, children,
                    start, mode, musketMasterFailures);
            }
            finally
            {
                DestroyFixtureBlueprint(independentFull);
                DestroyFixtureBlueprint(independentOneHanded);
                DestroyFixtureBlueprint(independentTwoHanded);
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }

            string wiringObserved = wiring.ToString(Newtonsoft.Json.Formatting.None) +
                (wiringFailures.Count == 0 ? "" :
                    ";failures=" + string.Join("|", wiringFailures.ToArray()));
            string catalogObserved = catalog.ToString(Newtonsoft.Json.Formatting.None) +
                (catalogFailures.Count == 0 ? "" :
                    ";failures=" + string.Join("|", catalogFailures.ToArray()));
            string matrixObserved = "ignorePrerequisitesOff=" + ignoreOff + ";" +
                matrix.ToString(Newtonsoft.Json.Formatting.None) +
                (matrixFailures.Count == 0 ? "" :
                    ";failures=" + string.Join("|", matrixFailures.ToArray()));
            string flowObserved = flow.ToString(Newtonsoft.Json.Formatting.None) +
                (flowFailures.Count == 0 ? "" :
                    ";failures=" + string.Join("|", flowFailures.ToArray()));
            string pendingObserved = (pending == null ? "<not-run>" :
                pending.ToString(Newtonsoft.Json.Formatting.None)) +
                (pendingFailures.Count == 0 ? "" :
                    ";failures=" + string.Join("|", pendingFailures.ToArray()));
            string musketObserved = (musketMaster == null ? "<not-run>" :
                musketMaster.ToString(Newtonsoft.Json.Formatting.None)) +
                (musketMasterFailures.Count == 0 ? "" :
                    ";failures=" + string.Join("|", musketMasterFailures.ToArray()));

            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("rapid-reload-parent-proficiency-prerequisites",
                    "the registered Rapid Reload selection carries exactly one OR-grouped (Prerequisite.GroupType.Any) firearm-proficiency prerequisite per official kind, each referencing the registered full and scoped proficiency features, with no class, archetype or legacy-wrapper prerequisite and IgnorePrerequisites false",
                    wiringObserved, wiringFailures.Count == 0,
                    "registered BlueprintFeatureSelection.ComponentsArray"),
                Assertion("rapid-reload-catalog-publication",
                    "the same gated parent stays published exactly once in the ordinary and Fighter combat feat catalogs; no compatibility-only child is published and the legacy wrapper stays out of every selection",
                    catalogObserved, catalogFailures.Count == 0,
                    "native feat selection Features/AllFeatures enumeration"),
                Assertion("rapid-reload-native-prerequisite-matrix",
                    "BlueprintFeature.MeetsPrerequisites on disposable units matches the shared gate rules for absent, full, one-handed, two-handed, independent-source, Gunslinger-identity-without-facts and legacy-wrapper characters",
                    matrixObserved, ignoreOff && matrixFailures.Count == 0,
                    "native BlueprintFeature.MeetsPrerequisites scored by RapidReloadPrerequisiteRules"),
                Assertion("rapid-reload-selection-flow",
                    "LevelUpController selection through the ordinary and Fighter combat feat slots grants Rapid Reload exactly when firearm proficiency covers the chosen firearm, and never otherwise; a refused attempt leaves no Rapid Reload fact after ApplyLevelup",
                    flowObserved, flowFailures.Count == 0,
                    "LevelUpController SelectFeature/ApplyLevelup on disposable units"),
                Assertion("rapid-reload-pending-build-refresh",
                    "eligibility follows the pending class: a Gunslinger visit qualifies, switching the pending class to Fighter withdraws the parent and every child, switching back restores them, and a held-then-abandoned Gunslinger choice does not survive confirmation as a Fighter",
                    pendingObserved, pendingFailures.Count == 0,
                    "repeated LevelUpController visits with SelectClass and ApplyClassMechanics"),
                Assertion("rapid-reload-musket-master-and-duplicates",
                    "Musket Master still grants Rapid Reload (Musket) at level one, the owned child can no longer be selected again, and the remaining legal choice stays available",
                    musketObserved, musketMasterFailures.Count == 0,
                    "archetype LevelEntry inspection plus native CanSelect on an owning unit"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "detached entity disposal and exact reference snapshots"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(
                assertions.TrueForAll(value => value.Status == "PASS")
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private static JObject DescribeRapidReloadWiring(
            BlueprintFeatureSelection parent, BlueprintFeature[] children,
            BlueprintFeature[] registeredChildren, BlueprintFeature fullProficiency,
            FirearmScopedProficiencyBlueprintSet scoped, BlueprintFeature legacyWrapper,
            FirearmKind[] kinds, IList<string> failures)
        {
            BlueprintComponent[] parentComponents = parent.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            PrerequisiteFirearmProficiency[] parentGate = parentComponents
                .OfType<PrerequisiteFirearmProficiency>().ToArray();
            if (parentGate.Length != kinds.Length)
                failures.Add("parent-gate-count=" + parentGate.Length);
            if (!parentGate.Select(value => value.Kind).SequenceEqual(kinds))
                failures.Add("parent-gate-kinds");
            if (parentGate.Any(value => value.Group != Prerequisite.GroupType.Any))
                failures.Add("parent-gate-not-or-grouped");
            if (parentGate.Any(value =>
                    !ReferenceEquals(value.FullProficiency, fullProficiency) ||
                    !ReferenceEquals(value.OneHandedProficiency, scoped.OneHanded) ||
                    !ReferenceEquals(value.TwoHandedProficiency, scoped.TwoHanded)))
                failures.Add("parent-gate-proficiency-identity");
            if (parentComponents.OfType<Prerequisite>().Count() != parentGate.Length)
                failures.Add("parent-carries-foreign-prerequisites");
            if (parentComponents.OfType<PrerequisiteClassLevel>().Any() ||
                parentComponents.OfType<PrerequisiteArchetypeLevel>().Any() ||
                parentComponents.OfType<PrerequisiteNoClassLevel>().Any())
                failures.Add("parent-has-class-prerequisite");
            if (parentComponents.OfType<PrerequisiteFeature>().Any(value =>
                    ReferenceEquals(value.Feature, legacyWrapper)))
                failures.Add("parent-requires-legacy-wrapper");
            if (parent.IgnorePrerequisites) failures.Add("parent-ignores-prerequisites");
            var childRows = new JArray();
            for (int index = 0; index < children.Length; index++)
            {
                BlueprintComponent[] components = children[index].ComponentsArray ??
                    Array.Empty<BlueprintComponent>();
                PrerequisiteFirearmProficiency[] gate = components
                    .OfType<PrerequisiteFirearmProficiency>().ToArray();
                if (gate.Length != 1) failures.Add("child-gate-count:" + kinds[index]);
                else
                {
                    if (gate[0].Kind != kinds[index])
                        failures.Add("child-gate-kind:" + kinds[index]);
                    if (gate[0].Group != Prerequisite.GroupType.All)
                        failures.Add("child-gate-not-and-grouped:" + kinds[index]);
                    if (!ReferenceEquals(gate[0].FullProficiency, fullProficiency) ||
                        !ReferenceEquals(gate[0].OneHandedProficiency, scoped.OneHanded) ||
                        !ReferenceEquals(gate[0].TwoHandedProficiency, scoped.TwoHanded))
                        failures.Add("child-gate-proficiency-identity:" + kinds[index]);
                }
                if (components.OfType<PrerequisiteClassLevel>().Any() ||
                    components.OfType<PrerequisiteArchetypeLevel>().Any())
                    failures.Add("child-has-class-prerequisite:" + kinds[index]);
                childRows.Add(new JObject {
                    ["kind"] = kinds[index].ToString(),
                    ["guid"] = children[index].AssetGuid,
                    ["prerequisiteCount"] = components.OfType<Prerequisite>().Count(),
                    ["firearmPrerequisiteCount"] = gate.Length,
                    ["group"] = gate.Length == 1 ? gate[0].Group.ToString() : "<none>",
                    ["uiText"] = gate.Length == 1 ? gate[0].GetUIText() : "<none>" });
            }
            return new JObject {
                ["parentGuid"] = parent.AssetGuid,
                ["parentName"] = parent.name,
                ["parentIgnorePrerequisites"] = parent.IgnorePrerequisites,
                ["parentGate"] = new JArray(parentGate.Select(value => new JObject {
                    ["componentName"] = value.name,
                    ["kind"] = value.Kind.ToString(),
                    ["group"] = value.Group.ToString(),
                    ["uiText"] = value.GetUIText() })),
                ["registeredChildCount"] = registeredChildren.Length,
                ["children"] = childRows };
        }

        private static JObject DescribeRapidReloadCatalogs(
            BlueprintFeatureSelection parent, BlueprintFeature[] registeredChildren,
            BlueprintFeature legacyWrapper, BlueprintFeatureSelection basic,
            BlueprintFeatureSelection fighterFeats, int officialCount,
            IList<string> failures)
        {
            foreach (var entry in new[] {
                new KeyValuePair<string, BlueprintFeatureSelection>("basic", basic),
                new KeyValuePair<string, BlueprintFeatureSelection>("fighter", fighterFeats) })
            {
                int features = CountRapidReloadReference(entry.Value.Features, parent);
                int allFeatures = CountRapidReloadReference(entry.Value.AllFeatures, parent);
                if (features != 1 || allFeatures != 1)
                    failures.Add("parent-publication:" + entry.Key + "=" + features +
                        "/" + allFeatures);
            }
            BlueprintFeature[] compatibilityOnly = registeredChildren
                .Skip(officialCount).ToArray();
            BlueprintFeatureSelection[] allSelections = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintFeatureSelection>().ToArray();
            foreach (BlueprintFeature retired in compatibilityOnly)
            {
                if (CountRapidReloadReference(parent.Features, retired) != 0 ||
                    CountRapidReloadReference(parent.AllFeatures, retired) != 0)
                    failures.Add("retired-child-in-parent:" + retired.name);
                if (allSelections.Any(selection =>
                        CountRapidReloadReference(selection.Features, retired) != 0 ||
                        CountRapidReloadReference(selection.AllFeatures, retired) != 0))
                    failures.Add("retired-child-published:" + retired.name);
            }
            if (allSelections.Any(selection =>
                    CountRapidReloadReference(selection.Features, legacyWrapper) != 0 ||
                    CountRapidReloadReference(selection.AllFeatures, legacyWrapper) != 0))
                failures.Add("legacy-wrapper-published");
            if (parent.Features.Length != officialCount ||
                parent.AllFeatures.Length != officialCount)
                failures.Add("parent-choice-count=" + parent.Features.Length + "/" +
                    parent.AllFeatures.Length);
            return new JObject {
                ["basicFeatures"] = CountRapidReloadReference(basic.Features, parent),
                ["basicAllFeatures"] = CountRapidReloadReference(basic.AllFeatures, parent),
                ["fighterFeatures"] = CountRapidReloadReference(fighterFeats.Features, parent),
                ["fighterAllFeatures"] = CountRapidReloadReference(
                    fighterFeats.AllFeatures, parent),
                ["parentChoices"] = new JArray(parent.AllFeatures.Select(value =>
                    value == null ? "<null>" : value.name)),
                ["compatibilityOnlyChoices"] = new JArray(compatibilityOnly.Select(
                    value => value.name)),
                ["selectionsScanned"] = allSelections.Length };
        }

        private static int CountRapidReloadReference(BlueprintFeature[] source,
            BlueprintFeature target)
        {
            return (source ?? Array.Empty<BlueprintFeature>()).Count(value =>
                ReferenceEquals(value, target) || value != null && target != null &&
                string.Equals(value.AssetGuid, target.AssetGuid,
                    StringComparison.Ordinal));
        }

        // Test-only proficiency source. It is created for this request only,
        // never registered, never published and destroyed in the scenario's
        // finally block, so players can never acquire it.
        private static BlueprintFeature CreateIndependentProficiencySource(
            string name, BlueprintFeature granted)
        {
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.name = "$KMG_RuntimeFixture_GrantFirearmProficiency";
            grant.Facts = new BlueprintUnitFact[] { granted };
            grant.DoNotRestoreMissingFacts = false;
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = name;
            feature.Ranks = 1;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            feature.ComponentsArray = new BlueprintComponent[] { grant };
            return feature;
        }

        private static void DestroyFixtureBlueprint(BlueprintScriptableObject value)
        {
            if (value == null) return;
            foreach (BlueprintComponent component in value.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
                if (component != null) UnityEngine.Object.DestroyImmediate(component);
            UnityEngine.Object.DestroyImmediate(value);
        }

        private static void GrantFixtureFact(UnitDescriptor descriptor,
            BlueprintFeature feature)
        {
            if (descriptor.AddFact(feature) == null)
                throw new InvalidOperationException(
                    "The disposable unit rejected the fixture fact " + feature.name + ".");
        }

        private static void RemoveFixtureFact(UnitDescriptor descriptor,
            BlueprintFeature feature)
        {
            if (descriptor.HasFact(feature)) descriptor.RemoveFact(feature);
        }

        private static void AddRapidReloadMatrixRow(JArray matrix,
            IList<string> failures, string label, BlueprintFeatureSelection parent,
            BlueprintFeature[] children, FirearmKind[] kinds, bool expectedFull,
            bool expectedOneHanded, bool expectedTwoHanded,
            Action<UnitDescriptor> prepare)
        {
            UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                if (prepare != null) prepare(descriptor);
                bool parentObserved = parent.MeetsPrerequisites(null, descriptor, null);
                bool parentExpected = RapidReloadPrerequisiteRules.ParentQualifies(
                    expectedFull, expectedOneHanded, expectedTwoHanded);
                if (parentObserved != parentExpected)
                    failures.Add(label + ":parent=" + parentObserved);
                var childRows = new JArray();
                for (int index = 0; index < children.Length; index++)
                {
                    bool observed = children[index].MeetsPrerequisites(null, descriptor,
                        null);
                    bool expected = RapidReloadPrerequisiteRules.ChildQualifies(
                        kinds[index], expectedFull, expectedOneHanded, expectedTwoHanded);
                    if (observed != expected)
                        failures.Add(label + ":" + kinds[index] + "=" + observed);
                    childRows.Add(new JObject {
                        ["kind"] = kinds[index].ToString(),
                        ["observed"] = observed, ["expected"] = expected });
                }
                matrix.Add(new JObject {
                    ["case"] = label,
                    ["grantedFull"] = expectedFull,
                    ["grantedOneHanded"] = expectedOneHanded,
                    ["grantedTwoHanded"] = expectedTwoHanded,
                    ["parentObserved"] = parentObserved,
                    ["parentExpected"] = parentExpected,
                    ["children"] = childRows });
            }
            finally { unit.Dispose(); }
        }

        // One real level-up visit: build a disposable unit of the given class,
        // find the requested native feat slot, try to commit Rapid Reload and the
        // requested firearm choice through the controller, then apply the level.
        private JObject RunRapidReloadSelectionAttempt(MethodInfo start,
            MethodInfo apply, object mode, string label,
            BlueprintCharacterClass characterClass,
            BlueprintFeatureSelection slotSelection, BlueprintFeatureSelection parent,
            BlueprintFeature child, Action<UnitDescriptor> prepare,
            bool expectAcquired, IList<string> failures)
        {
            UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            LevelUpController controller = null;
            var row = new JObject { ["case"] = label,
                ["class"] = characterClass.name, ["slot"] = slotSelection.name,
                ["child"] = child.name, ["expectAcquired"] = expectAcquired };
            bool parentCanSelect = false, parentSelected = false, childSelected = false;
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                if (prepare != null) prepare(descriptor);
                controller = (LevelUpController)start.Invoke(null,
                    new object[] { descriptor, false, null, null, mode });
                if (!controller.SelectClass(characterClass, false))
                    throw new InvalidOperationException(
                        "Native class selection rejected the Rapid Reload gate visit.");
                controller.ApplyClassMechanics();
                row["previewCrossbowProficiency"] =
                    controller.Preview.Proficiencies.Contains(WeaponCategory.LightCrossbow) ||
                    controller.Preview.Proficiencies.Contains(WeaponCategory.HeavyCrossbow);
                row["previewParentEligible"] = parent.MeetsPrerequisites(null,
                    controller.Preview, controller.State);
                FeatureSelectionState slot = controller.State.Selections
                    .FirstOrDefault(value => !value.Selected &&
                        ReferenceEquals(value.Selection, slotSelection));
                if (slot == null)
                {
                    failures.Add(label + ":slot-absent");
                    row["slotPresent"] = false;
                    return row;
                }
                row["slotPresent"] = true;
                IFeatureSelectionItem[] items = slotSelection
                    .ExtractSelectionItems(controller.Preview, controller.Preview).ToArray();
                IFeatureSelectionItem parentItem = items.FirstOrDefault(value =>
                    value != null && ReferenceEquals(value.Feature, parent));
                row["parentOffered"] = parentItem != null;
                if (parentItem == null)
                {
                    failures.Add(label + ":parent-not-offered");
                    return row;
                }
                parentCanSelect = slotSelection.CanSelect(controller.Preview,
                    controller.State, slot, parentItem);
                row["parentCanSelect"] = parentCanSelect;
                parentSelected = controller.SelectFeature(slot, parentItem);
                row["parentSelected"] = parentSelected;
                if (parentSelected)
                {
                    FeatureSelectionState childSlot = slot.Next;
                    IFeatureSelectionItem[] childItems = childSlot == null ||
                        childSlot.Selection == null ? Array.Empty<IFeatureSelectionItem>() :
                        childSlot.Selection.ExtractSelectionItems(controller.Preview,
                            controller.Preview).ToArray();
                    row["childMenu"] = new JArray(childItems.Select(value =>
                        value == null || value.Feature == null ? "<null>" :
                            value.Feature.name));
                    IFeatureSelectionItem childItem = childItems.FirstOrDefault(value =>
                        value != null && ReferenceEquals(value.Feature, child));
                    row["childOffered"] = childItem != null;
                    if (childItem != null)
                    {
                        row["childCanSelect"] = childSlot.Selection.CanSelect(
                            controller.Preview, controller.State, childSlot, childItem);
                        childSelected = controller.SelectFeature(childSlot, childItem);
                    }
                    row["childSelected"] = childSelected;
                    if (!childSelected)
                    {
                        // Never leave a dangling parent commit behind: the
                        // confirmation below must not be able to bank an empty
                        // Rapid Reload selection.
                        try { controller.UnselectFeature(slot); }
                        catch (Exception error) { row["unselectError"] = error.Message; }
                    }
                }
                apply.Invoke(controller, new object[] { descriptor });
                controller.Cancel();
                controller = null;
                bool acquiredParent = descriptor.Progression.Features.GetRank(parent) > 0;
                bool acquiredChild = descriptor.Progression.Features.GetRank(child) > 0;
                row["acquiredParent"] = acquiredParent;
                row["acquiredChild"] = acquiredChild;
                row["classLevel"] = descriptor.Progression.GetClassLevel(characterClass);
                if (acquiredChild != expectAcquired)
                    failures.Add(label + ":acquiredChild=" + acquiredChild);
                if (!expectAcquired && acquiredParent)
                    failures.Add(label + ":empty-parent-banked");
                if (expectAcquired && !parentCanSelect)
                    failures.Add(label + ":parent-refused");
                if (!expectAcquired && childSelected)
                    failures.Add(label + ":ineligible-choice-committed");
            }
            finally
            {
                if (controller != null)
                {
                    try { controller.Cancel(); }
                    catch (Exception error) { row["cleanupError"] = error.Message; }
                }
                unit.Dispose();
            }
            return row;
        }

        // Pending-build proof: eligibility must follow the class being built,
        // and a choice held under a qualifying pending class must not survive a
        // confirmation made under a non-qualifying one.
        private JObject RunRapidReloadPendingClassChange(MethodInfo start,
            MethodInfo apply, object mode, BlueprintCharacterClass gunslinger,
            BlueprintCharacterClass fighter, BlueprintFeatureSelection slotSelection,
            BlueprintFeatureSelection parent, BlueprintFeature child,
            IList<string> failures)
        {
            UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            var row = new JObject();
            LevelUpController controller = null;
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                var parentEligibility = new JArray();
                var childEligibility = new JArray();
                BlueprintCharacterClass[] sequence = { gunslinger, fighter, gunslinger };
                for (int visit = 0; visit < sequence.Length; visit++)
                {
                    controller = (LevelUpController)start.Invoke(null,
                        new object[] { descriptor, false, null, null, mode });
                    if (!controller.SelectClass(sequence[visit], false))
                        throw new InvalidOperationException(
                            "Native class selection rejected a pending-change visit.");
                    controller.ApplyClassMechanics();
                    parentEligibility.Add(parent.MeetsPrerequisites(null,
                        controller.Preview, controller.State));
                    childEligibility.Add(child.MeetsPrerequisites(null,
                        controller.Preview, controller.State));
                    controller.Cancel();
                    controller = null;
                }
                row["pendingClassSequence"] = new JArray(sequence.Select(
                    value => value.name));
                row["parentEligibility"] = parentEligibility;
                row["childEligibility"] = childEligibility;
                if (!(bool)parentEligibility[0])
                    failures.Add("gunslinger-visit-not-eligible");
                if ((bool)parentEligibility[1])
                    failures.Add("fighter-visit-eligible-without-proficiency");
                if (!(bool)parentEligibility[2])
                    failures.Add("gunslinger-visit-did-not-refresh");
                if (!(bool)childEligibility[0] || (bool)childEligibility[1] ||
                    !(bool)childEligibility[2])
                    failures.Add("child-eligibility-did-not-refresh");

                // Hold the choice under the qualifying pending class, abandon
                // the visit, then confirm the level as a Fighter.
                controller = (LevelUpController)start.Invoke(null,
                    new object[] { descriptor, false, null, null, mode });
                if (!controller.SelectClass(gunslinger, false))
                    throw new InvalidOperationException(
                        "Native class selection rejected the stale-choice visit.");
                controller.ApplyClassMechanics();
                FeatureSelectionState slot = controller.State.Selections
                    .FirstOrDefault(value => !value.Selected &&
                        ReferenceEquals(value.Selection, slotSelection));
                bool held = false;
                if (slot != null)
                {
                    IFeatureSelectionItem parentItem = slotSelection
                        .ExtractSelectionItems(controller.Preview, controller.Preview)
                        .FirstOrDefault(value => value != null &&
                            ReferenceEquals(value.Feature, parent));
                    if (parentItem != null && controller.SelectFeature(slot, parentItem))
                    {
                        FeatureSelectionState childSlot = slot.Next;
                        IFeatureSelectionItem childItem = childSlot == null ||
                            childSlot.Selection == null ? null : childSlot.Selection
                                .ExtractSelectionItems(controller.Preview,
                                    controller.Preview)
                                .FirstOrDefault(value => value != null &&
                                    ReferenceEquals(value.Feature, child));
                        held = childItem != null &&
                            controller.SelectFeature(childSlot, childItem);
                    }
                }
                row["staleChoiceHeld"] = held;
                if (!held) failures.Add("stale-choice-was-never-held");
                controller.Cancel();
                controller = null;

                controller = (LevelUpController)start.Invoke(null,
                    new object[] { descriptor, false, null, null, mode });
                if (!controller.SelectClass(fighter, false))
                    throw new InvalidOperationException(
                        "Native class selection rejected the confirmation visit.");
                controller.ApplyClassMechanics();
                apply.Invoke(controller, new object[] { descriptor });
                controller.Cancel();
                controller = null;
                bool survived = descriptor.Progression.Features.GetRank(child) > 0 ||
                    descriptor.Progression.Features.GetRank(parent) > 0;
                row["staleChoiceSurvivedConfirmation"] = survived;
                row["fighterLevel"] = descriptor.Progression.GetClassLevel(fighter);
                row["gunslingerLevel"] = descriptor.Progression.GetClassLevel(gunslinger);
                if (survived) failures.Add("stale-choice-survived-confirmation");
            }
            finally
            {
                if (controller != null)
                {
                    try { controller.Cancel(); }
                    catch (Exception error) { row["cleanupError"] = error.Message; }
                }
                unit.Dispose();
            }
            return row;
        }

        // Musket Master keeps its automatic level-one Rapid Reload (Musket)
        // grant, and an already-owned child cannot consume a second feat.
        private JObject DescribeMusketMasterRapidReload(
            GunslingerClassBlueprintSet gunslinger, BlueprintFeature[] children,
            MethodInfo start, object mode, IList<string> failures)
        {
            BlueprintArchetype archetype = gunslinger.MusketMaster == null ? null :
                gunslinger.MusketMaster.Archetype;
            if (archetype == null)
            {
                failures.Add("musket-master-archetype-missing");
                return new JObject { ["archetype"] = "<missing>" };
            }
            BlueprintFeature musketChild = children[1];
            LevelEntry levelOne = (archetype.AddFeatures ?? Array.Empty<LevelEntry>())
                .FirstOrDefault(value => value.Level == 1);
            bool granted = levelOne != null && levelOne.Features != null &&
                levelOne.Features.Any(value => ReferenceEquals(value, musketChild));
            if (!granted) failures.Add("musket-master-level-one-grant-missing");
            var row = new JObject {
                ["archetype"] = archetype.name,
                ["levelOneGrantsRapidReloadMusket"] = granted,
                ["levelOneFeatures"] = new JArray((levelOne == null ||
                        levelOne.Features == null ?
                        new List<BlueprintFeatureBase>() : levelOne.Features)
                    .Select(value => value == null ? "<null>" : value.name)) };
            UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            LevelUpController controller = null;
            try
            {
                UnitDescriptor descriptor = unit.Descriptor;
                GrantFixtureFact(descriptor,
                    BlueprintBootstrap.ScopedFirearmProficiencies.TwoHanded);
                GrantFixtureFact(descriptor, musketChild);
                controller = (LevelUpController)start.Invoke(null,
                    new object[] { descriptor, false, null, null, mode });
                if (!controller.SelectClass(gunslinger.CharacterClass, false))
                    throw new InvalidOperationException(
                        "Native class selection rejected the Musket Master duplicate visit.");
                controller.ApplyClassMechanics();
                BlueprintFeatureSelection parent =
                    BlueprintBootstrap.FirearmFeats.RapidReload;
                FeatureSelectionState slot = controller.State.Selections
                    .FirstOrDefault(value => !value.Selected &&
                        value.Selection is BlueprintFeatureSelection);
                IFeatureSelectionItem[] items = parent.ExtractSelectionItems(
                    controller.Preview, controller.Preview).ToArray();
                IFeatureSelectionItem ownedItem = items.FirstOrDefault(value =>
                    value != null && ReferenceEquals(value.Feature, musketChild));
                IFeatureSelectionItem otherItem = items.FirstOrDefault(value =>
                    value != null && ReferenceEquals(value.Feature, children[2]));
                bool ownedSelectable = slot != null && ownedItem != null &&
                    parent.CanSelect(controller.Preview, controller.State, slot, ownedItem);
                bool otherSelectable = slot != null && otherItem != null &&
                    parent.CanSelect(controller.Preview, controller.State, slot, otherItem);
                row["slotPresent"] = slot != null;
                row["ownedChildStillSelectable"] = ownedSelectable;
                row["unownedLegalChildSelectable"] = otherSelectable;
                if (slot == null) failures.Add("musket-master-duplicate-slot-absent");
                if (ownedSelectable) failures.Add("owned-child-can-be-reselected");
                if (slot != null && !otherSelectable)
                    failures.Add("legal-unowned-child-unavailable");
                controller.Cancel();
                controller = null;
            }
            finally
            {
                if (controller != null)
                {
                    try { controller.Cancel(); }
                    catch (Exception error) { row["cleanupError"] = error.Message; }
                }
                unit.Dispose();
            }
            return row;
        }
    }
}
