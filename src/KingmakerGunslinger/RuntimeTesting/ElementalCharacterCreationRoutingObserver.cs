using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded observation only. Never creates a unit, chooses an option,
    /// rebuilds a view, opens a phase, loads a save or writes a blueprint.
    /// An absent active creator is NOT-RUN, never evidence of a usable route.
    /// </summary>
    internal static class ElementalCharacterCreationRoutingObserver
    {
        internal const string EvidenceFileName = "elemental-character-creation-routing.json";
        private static readonly string[] TraitFields =
        {
            "traits_selection", "traits_selection2", "combat_traits",
            "faith_traits", "magic_traits", "religion_traits", "social_traits",
            "racial_traits", "regional_traits", "equipment_traits", "adopted",
            "additional_traits"
        };
        private static readonly BindingFlags Fields = BindingFlags.Public |
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        private static readonly CharacterCreationObservationIdentity Identities =
            new CharacterCreationObservationIdentity();
        private static readonly JArray Snapshots = new JArray();
        private static readonly JArray Failures = new JArray();
        private static RuntimeTestRequest _request;
        private static bool _capturing;
        private static int _readOnlyChecks;
        private static bool _beforeCaptured;
        private static bool _afterCaptured;

        internal static void Arm(RuntimeTestRequest request)
        {
            if (request == null || request.Scenario !=
                RuntimeTestScenarioCatalog.ObserveElementalCharacterCreationRouting)
                return;
            if (_request != null) throw new InvalidOperationException(
                "Only one guarded character-creation observation may be armed.");
            _request = request;
        }

        internal static void BeforeOptionalReconciliation(string checkpoint)
        {
            if (_request == null || _capturing || _beforeCaptured ||
                BlueprintBootstrap.Library == null) return;
            if (Capture("before-kmg-optional-reconciliation:" + checkpoint))
                _beforeCaptured = true;
        }

        internal static void AfterOptionalReconciliation(string checkpoint)
        {
            if (_request == null || _capturing || !_beforeCaptured ||
                _afterCaptured) return;
            if (Capture("after-kmg-optional-reconciliation:" + checkpoint))
                _afterCaptured = true;
        }

        private static bool Capture(string checkpoint)
        {
            if (_request == null || _capturing) return false;
            _capturing = true;
            try
            {
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                if (library == null) return false;
                BlueprintScriptableObject[] all = library.GetAllBlueprints().ToArray();
                var selections = new Dictionary<BlueprintFeatureSelection, HashSet<string>>(
                    ReferenceComparer<BlueprintFeatureSelection>.Instance);
                BlueprintRace[] raceCatalog = BlueprintRoot.Instance.Progression.CharacterRaces;
                BlueprintRace[] catalogBefore = (BlueprintRace[])raceCatalog.Clone();
                foreach (BlueprintRace race in all.OfType<BlueprintRace>()
                    .Concat(raceCatalog).Distinct(ReferenceComparer<BlueprintRace>.Instance))
                    CollectRaceSelections((race.Features ?? new BlueprintFeatureBase[0]).OfType<BlueprintFeature>(), "race:" + race.AssetGuid +
                        ":" + race.name, selections,
                        new HashSet<BlueprintFeature>(ReferenceComparer<BlueprintFeature>.Instance));
                foreach (BlueprintFeatureSelection selection in all.OfType<BlueprintFeatureSelection>()
                    .Where(value => value.name.IndexOf("heritage",
                        StringComparison.OrdinalIgnoreCase) >= 0))
                    AddSelection(selection, "heritage-name-discovery-only", selections);

                JArray ruCallbacks = CollectRacesUnleashedCallbacks(selections);
                var foreign = new JObject();
                Assembly favored = AppDomain.CurrentDomain.GetAssemblies()
                    .SingleOrDefault(value => value.GetName().Name == "ZFavoredClass");
                Type traitsType = favored == null ? null : favored.GetType("ZFavoredClass.Traits", false);
                foreach (string fieldName in TraitFields)
                {
                    FieldInfo field = traitsType == null ? null : traitsType.GetField(fieldName, Fields);
                    BlueprintFeature feature = field == null ? null : field.GetValue(null) as BlueprintFeature;
                    foreign[fieldName] = feature == null ? (JToken)JValue.CreateNull() : DescribeFeature(feature);
                    var selection = feature as BlueprintFeatureSelection;
                    if (selection != null) AddSelection(selection,
                        "ZFavoredClass.Traits." + fieldName, selections);
                }
                // Record every owning progression for the exact inspected selections.
                foreach (BlueprintProgression progression in all.OfType<BlueprintProgression>())
                    foreach (var level in progression.LevelEntries ?? new LevelEntry[0])
                        foreach (var feature in level.Features ?? new List<BlueprintFeatureBase>())
                        {
                            var selection = feature as BlueprintFeatureSelection;
                            if (selection != null && selections.ContainsKey(selection))
                                selections[selection].Add("progression:" + progression.AssetGuid +
                                    ":" + progression.name + ":level=" + level.Level);
                        }

                var rows = new JArray();
                foreach (var entry in selections.OrderBy(value => value.Key.AssetGuid,
                    StringComparer.Ordinal))
                    rows.Add(DescribeSelection(entry.Key, entry.Value));
                Snapshots.Add(new JObject
                {
                    ["checkpoint"] = checkpoint,
                    ["utc"] = DateTime.UtcNow.ToString("o"),
                    ["selections"] = rows,
                    ["raceCatalogReference"] = Id(raceCatalog),
                    ["raceCatalog"] = new JArray(raceCatalog.Select((race, index) =>
                        new JObject { ["index"] = index, ["reference"] = Id(race),
                            ["guid"] = race.AssetGuid, ["name"] = race.name,
                            ["directSelections"] = new JArray((race.Features ?? new BlueprintFeatureBase[0])
                                .OfType<BlueprintFeatureSelection>().Select(value => value.AssetGuid)) })),
                    ["raceCatalogUnchanged"] = ReferenceEquals(raceCatalog,
                        BlueprintRoot.Instance.Progression.CharacterRaces) &&
                        CharacterCreationObservationIdentity.SameOrderedReferences(catalogBefore, raceCatalog),
                    ["favoredClassFields"] = foreign,
                    ["racesUnleashedChargenCallbacks"] = ruCallbacks,
                    ["activeBuild"] = DescribeActiveBuild(),
                    ["blueprintEnumerationUnchanged"] = all.SequenceEqual(library.GetAllBlueprints(),
                        ReferenceComparer<BlueprintScriptableObject>.Instance)
                });
                WriteEvidence();
                return true;
            }
            catch (Exception exception)
            {
                Failures.Add(checkpoint + ": " + exception);
                // Observation cannot interrupt or conceal the production callback.
                return false;
            }
            finally { _capturing = false; }
        }

        private static void CollectRaceSelections(IEnumerable<BlueprintFeature> features,
            string owner, IDictionary<BlueprintFeatureSelection, HashSet<string>> result,
            ISet<BlueprintFeature> visited)
        {
            foreach (BlueprintFeature feature in features ?? new BlueprintFeature[0])
            {
                if (feature == null || !visited.Add(feature)) continue;
                var selection = feature as BlueprintFeatureSelection;
                if (selection == null) continue;
                AddSelection(selection, owner, result);
                CollectRaceSelections(selection.AllFeatures, owner, result, visited);
                CollectRaceSelections(selection.Features, owner, result, visited);
            }
        }

        private static JArray CollectRacesUnleashedCallbacks(
            IDictionary<BlueprintFeatureSelection, HashSet<string>> selections)
        {
            var result = new JArray();
            Assembly ru = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(value => value.GetName().Name == "RacesUnleashed");
            Type hook = ru == null ? null : ru.GetType("RacesUnleashed.ApplyClassMechanics_Apply_Patch", false);
            FieldInfo field = hook == null ? null : hook.GetField("onChargenApply", Fields);
            var callbacks = field == null ? null : field.GetValue(null) as IEnumerable;
            if (callbacks == null) return result;
            foreach (Delegate callback in callbacks)
            {
                object target = callback.Target;
                if (target == null || !target.GetType().FullName.StartsWith(
                    "RacesUnleashed.RacialTraits+", StringComparison.Ordinal)) continue;
                FieldInfo choicesField = target.GetType().GetField("alternativeRacialTraits", Fields);
                FieldInfo raceField = target.GetType().GetField("race", Fields);
                var choices = choicesField == null ? null : choicesField.GetValue(target)
                    as IEnumerable<BlueprintFeatureSelection>;
                var race = raceField == null ? null : raceField.GetValue(target) as BlueprintRace;
                if (choices == null || race == null) throw new InvalidOperationException(
                    "Races Unleashed racial-trait callback contract changed.");
                BlueprintFeatureSelection[] entries = choices.ToArray();
                foreach (BlueprintFeatureSelection selection in entries)
                    AddSelection(selection, "RacesUnleashed.onChargenApply:race:" + race.AssetGuid + ":" + race.name,
                        selections);
                result.Add(new JObject { ["reference"] = Id(callback), ["targetReference"] = Id(target),
                    ["targetType"] = target.GetType().FullName, ["method"] = callback.Method.Name,
                    ["raceGuid"] = race.AssetGuid, ["selectionsReference"] = Id(choices),
                    ["selections"] = new JArray(entries.Select(value => value.AssetGuid)),
                    ["callbackInvoked"] = false });
            }
            return result;
        }

        private static void AddSelection(BlueprintFeatureSelection selection,
            string owner, IDictionary<BlueprintFeatureSelection, HashSet<string>> result)
        {
            HashSet<string> owners;
            if (!result.TryGetValue(selection, out owners))
            {
                owners = new HashSet<string>(StringComparer.Ordinal);
                result.Add(selection, owners);
            }
            owners.Add(owner);
        }

        private static JObject DescribeSelection(BlueprintFeatureSelection selection,
            IEnumerable<string> owners)
        {
            BlueprintFeature[] features = selection.Features;
            BlueprintFeature[] allFeatures = selection.AllFeatures;
            BlueprintComponent[] components = selection.ComponentsArray;
            BlueprintFeature[] featuresBefore = features == null ? null : (BlueprintFeature[])features.Clone();
            BlueprintFeature[] allBefore = allFeatures == null ? null : (BlueprintFeature[])allFeatures.Clone();
            JObject row = DescribeFeature(selection);
            row["owners"] = new JArray(owners.OrderBy(value => value, StringComparer.Ordinal));
            row["group"] = selection.Group.ToString();
            row["group2"] = selection.Group2.ToString();
            row["mode"] = selection.Mode.ToString();
            row["obligatory"] = selection.Obligatory;
            row["ignorePrerequisites"] = selection.IgnorePrerequisites;
            row["featuresReference"] = Id(features);
            row["allFeaturesReference"] = Id(allFeatures);
            row["featuresCount"] = features == null ? -1 : features.Length;
            row["allFeaturesCount"] = allFeatures == null ? -1 : allFeatures.Length;
            row["features"] = FeatureArray(features);
            row["allFeatures"] = FeatureArray(allFeatures);
            row["arraysSameReference"] = ReferenceEquals(features, allFeatures);
            row["arraysSameOrderedReferences"] = CharacterCreationObservationIdentity.SameOrderedReferences(
                features, allFeatures);
            row["arraysSameOrderedGuids"] = features == null || allFeatures == null
                ? features == allFeatures
                : features.Select(value => value == null ? null : value.AssetGuid).SequenceEqual(
                    allFeatures.Select(value => value == null ? null : value.AssetGuid));
            bool unchanged = ReferenceEquals(features, selection.Features) &&
                ReferenceEquals(allFeatures, selection.AllFeatures) &&
                ReferenceEquals(components, selection.ComponentsArray) &&
                CharacterCreationObservationIdentity.SameOrderedReferences(featuresBefore, selection.Features) &&
                CharacterCreationObservationIdentity.SameOrderedReferences(allBefore, selection.AllFeatures);
            row["observerPreservedArraysAndComponents"] = unchanged;
            _readOnlyChecks++;
            if (!unchanged) Failures.Add("Observer changed selection " + selection.AssetGuid);
            return row;
        }

        private static JArray FeatureArray(IEnumerable<BlueprintFeature> features)
        {
            return features == null ? new JArray() :
                new JArray(features.Select(value => value == null ?
                    (JToken)JValue.CreateNull() : DescribeFeature(value)));
        }

        private static JObject DescribeFeature(BlueprintFeature feature)
        {
            return new JObject
            {
                ["reference"] = Id(feature),
                ["guid"] = feature.AssetGuid,
                ["internalName"] = feature.name,
                ["displayName"] = feature.Name,
                ["groupsReference"] = Id(feature.Groups),
                ["groups"] = new JArray((feature.Groups ?? new FeatureGroup[0]).Select(value => value.ToString())),
                ["isClassFeature"] = feature.IsClassFeature,
                ["hideInUI"] = feature.HideInUI,
                ["hideInCharacterSheetAndLevelUp"] = feature.HideInCharacterSheetAndLevelUp,
                ["ranks"] = feature.Ranks,
                ["isDlcAvailable"] = feature.IsDlcAvailable(),
                ["componentsReference"] = Id(feature.ComponentsArray),
                ["prerequisites"] = new JArray((feature.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<Prerequisite>().Select(DescribePrerequisite))
            };
        }

        private static JObject DescribePrerequisite(Prerequisite prerequisite)
        {
            var fields = new JObject();
            foreach (FieldInfo field in prerequisite.GetType().GetFields(Fields)
                .Where(value => !value.IsStatic).OrderBy(value => value.Name, StringComparer.Ordinal))
            {
                object value = field.GetValue(prerequisite);
                var blueprint = value as BlueprintScriptableObject;
                if (value == null) fields[field.Name] = JValue.CreateNull();
                else if (blueprint != null) fields[field.Name] = new JObject
                { ["reference"] = Id(blueprint), ["guid"] = blueprint.AssetGuid, ["name"] = blueprint.name };
                else if (value is string || value.GetType().IsPrimitive || value.GetType().IsEnum)
                    fields[field.Name] = value.ToString();
                else if (value is BlueprintScriptableObject[])
                    fields[field.Name] = new JArray(((BlueprintScriptableObject[])value).Select(item =>
                        item == null ? (JToken)JValue.CreateNull() : new JObject
                        { ["reference"] = Id(item), ["guid"] = item.AssetGuid, ["name"] = item.name }));
                else fields[field.Name] = new JObject
                { ["reference"] = Id(value), ["type"] = value.GetType().FullName };
            }
            return new JObject
            { ["reference"] = Id(prerequisite), ["type"] = prerequisite.GetType().FullName, ["fields"] = fields };
        }

        internal static JObject DescribeActiveBuild()
        {
            CharacterBuildController build = Game.Instance == null || Game.Instance.UI == null
                ? null : Game.Instance.UI.CharacterBuildController;
            LevelUpController controller = build == null ? null : build.LevelUpController;
            if (controller == null || controller.State == null)
                return new JObject { ["status"] = "NOT-RUN", ["reason"] = "No active native character creator",
                    ["gameAvailable"] = Game.Instance != null,
                    ["uiAvailable"] = Game.Instance != null && Game.Instance.UI != null,
                    ["buildAvailable"] = build != null };
            LevelUpState state = controller.State;
            var selections = new JArray();
            foreach (FeatureSelectionState selection in state.Selections)
            {
                string[] phases = (build.CharacterBuildPhaseStates ?? new List<CharBPhase>())
                    .OfType<CharBPhaseFeatures>()
                    .Where(value => value.FeatureCollections != null &&
                        value.FeatureCollections.Any(item => ReferenceEquals(item, selection)))
                    .Select(value => value.Phase + ":" + value.LabelText).ToArray();
                IFeatureSelectionItem[] items = selection.Selection
                    .ExtractSelectionItems(controller.Unit, controller.Preview).ToArray();
                object rawViews = typeof(FeatureSelectionState).GetField("m_ViewState", Fields)
                    .GetValue(selection);
                var views = rawViews as IEnumerable<FeatureSelectionViewState>;
                selections.Add(new JObject
                {
                    ["stateReference"] = Id(selection),
                    ["parentReference"] = Id(selection.Parent),
                    ["sourceReference"] = Id(selection.Source),
                    ["sourceGuid"] = selection.Source == null ? null : selection.Source.AssetGuid,
                    ["selectionReference"] = Id(selection.Selection),
                    ["selectionGuid"] = (selection.Selection as BlueprintScriptableObject)?.AssetGuid,
                    ["index"] = selection.Index, ["level"] = selection.Level,
                    ["consumedByActualPhases"] = new JArray(phases),
                    ["selectedGuid"] = selection.SelectedItem?.Feature?.AssetGuid,
                    ["complete"] = selection.Selected,
                    ["isSelectionProhibitedForUnit"] = selection.Selection.IsSelectionProhibited(controller.Unit),
                    ["isSelectionProhibitedForPreview"] = selection.Selection.IsSelectionProhibited(controller.Preview),
                    ["canSelectAnything"] = selection.CanSelectAnything(state, controller.Preview),
                    ["extractedChoices"] = new JArray(items.Select(item => new JObject
                    {
                        ["reference"] = Id(item), ["guid"] = item.Feature?.AssetGuid,
                        ["canSelect"] = selection.Selection.CanSelect(controller.Preview, state, selection, item)
                    })),
                    ["viewStateReference"] = Id(rawViews),
                    ["existingViewCount"] = views == null ? (JToken)JValue.CreateNull() : views.Count(),
                    ["existingViewEmpty"] = views == null ? (JToken)JValue.CreateNull() : !views.Any(),
                    ["existingViews"] = views == null ? new JArray() : new JArray(views.Select(value =>
                        new JObject { ["featureGuid"] = value.Feature?.AssetGuid,
                            ["canSelect"] = value.CanSelect, ["canSelectState"] = value.CanSelectState.ToString() }))
                });
            }
            var button = typeof(CharacterBuildController).GetField("m_CompleteButton", Fields)
                .GetValue(build) as UnityEngine.UI.Button;
            return new JObject
            {
                ["status"] = "OBSERVED-ACTIVE-STATE",
                ["controllerReference"] = Id(controller),
                ["buildReference"] = Id(build),
                ["stateReference"] = Id(state),
                ["previewReference"] = Id(controller.Preview),
                ["unitReference"] = Id(controller.Unit),
                ["mode"] = state.Mode.ToString(),
                ["nextLevel"] = state.NextLevel,
                ["currentPhase"] = build.CurrentPhase?.ToString(),
                ["nextPhase"] = build.NextPhase.ToString(),
                ["nextOrCompleteButtonInteractable"] = button == null ? (JToken)JValue.CreateNull() : button.interactable,
                ["complete"] = state.IsComplete(),
                ["remainingSelections"] = state.RemainingSelections(),
                ["phaseOrder"] = new JArray((build.CharacterBuildPhaseStates ?? new List<CharBPhase>())
                    .Select(value => new JObject { ["reference"] = Id(value), ["phase"] = value.Phase.ToString(),
                        ["label"] = value.LabelText, ["unlocked"] = value.IsUnlocked })),
                ["selections"] = selections,
                ["selectorLayers"] = new JArray(build.GetComponentsInChildren<CharBSelectorLayer>(true)
                    .Select(layer => DescribeLayer(layer)))
            };
        }

        private static JObject DescribeLayer(CharBSelectorLayer layer)
        {
            object rawItems = typeof(CharBSelectorLayer).GetField("m_SelectionItems", Fields).GetValue(layer);
            var items = rawItems as IEnumerable<IFeatureSelectionItem>;
            object selection = typeof(CharBSelectorLayer).GetProperty("CurrentSelectionState", Fields).GetValue(layer, null);
            return new JObject
            {
                ["reference"] = Id(layer), ["active"] = layer.gameObject.activeInHierarchy,
                ["selectionStateReference"] = Id(selection),
                ["itemsReference"] = Id(rawItems),
                ["visibleChoiceCount"] = items == null ? (JToken)JValue.CreateNull() : items.Count(),
                ["visibleChoiceGuids"] = items == null ? new JArray() :
                    new JArray(items.Select(item => item.Feature?.AssetGuid))
            };
        }

        private static void WriteEvidence()
        {
            if (_request == null) return;
            var evidence = new JObject
            {
                ["schemaVersion"] = 1,
                ["runId"] = _request.RunId,
                ["observationOnly"] = true,
                ["saveStateTouched"] = false,
                ["playerFacingAcceptance"] = "NOT-RUN",
                ["snapshots"] = Snapshots.DeepClone(),
                ["failures"] = Failures.DeepClone()
            };
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                EvidenceFileName), evidence.ToString(Formatting.Indented));
        }

        internal static RuntimeTestResult Run(ModContext context, RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            Capture("runtime-ready");
            var assertions = new List<RuntimeTestAssertion>();
            Add(assertions, "observation-captured", Snapshots.Count > 0, "snapshots=" + Snapshots.Count);
            Add(assertions, "observation-complete-without-errors", Failures.Count == 0, Failures.ToString());
            Add(assertions, "pre-reconciliation-observed", _beforeCaptured, _beforeCaptured.ToString());
            Add(assertions, "post-reconciliation-observed", _afterCaptured, _afterCaptured.ToString());
            Add(assertions, "observer-preserved-blueprint-arrays", _readOnlyChecks > 0 && Failures.Count == 0,
                "checks=" + _readOnlyChecks);
            JObject final = Snapshots.Last as JObject;
            bool catalogsExact = final != null && Snapshots.OfType<JObject>()
                .All(snapshot => (bool)snapshot["raceCatalogUnchanged"] &&
                    (bool)snapshot["blueprintEnumerationUnchanged"] &&
                    ((JArray)snapshot["raceCatalog"]).OfType<JObject>().All(race =>
                        ((JArray)race["directSelections"]).All(guid =>
                            ((JArray)snapshot["selections"]).OfType<JObject>()
                                .Any(row => (string)row["guid"] == (string)guid))));
            Add(assertions, "published-race-selection-coverage", catalogsExact,
                "Every directly published race selection is observed with unchanged catalog references.");
            bool ruSelectionsExact = final != null && ((JArray)final["racesUnleashedChargenCallbacks"])
                .OfType<JObject>().All(callback => ((JArray)callback["selections"]).All(guid =>
                    ((JArray)final["selections"]).OfType<JObject>().Any(row => (string)row["guid"] == (string)guid)));
            Add(assertions, "races-unleashed-callback-selection-coverage", ruSelectionsExact,
                "Installed registered callbacks inspected without invoking them.");
            if (final != null && context.FeatureModules.Active.ElementalRaces)
            {
                JObject[] rows = ((JArray)final["selections"]).OfType<JObject>().ToArray();
                Add(assertions, "all-four-elemental-heritage-selections", rows.Count(row =>
                    ((string)row["internalName"]).StartsWith("KMG_ElementalRaces_", StringComparison.Ordinal) &&
                    ((string)row["internalName"]).EndsWith("_HeritageSelection", StringComparison.Ordinal)) == 4,
                    "Four exact current heritage selectors required.");
                Add(assertions, "all-ten-elemental-replacement-selections", rows.Count(row =>
                    ((string)row["guid"]).StartsWith("e117e1e0a17a4acec001", StringComparison.Ordinal)) == 10,
                    "Ten fixed replacement-slot identities required.");
            }
            WriteEvidence();
            _request = null;
            return new RuntimeTestResult
            {
                SchemaVersion = 1, RunId = request.RunId, Scenario = request.Scenario,
                Status = assertions.All(value => value.Status == RuntimeTestStatuses.Pass)
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName + ";mvid=" +
                    context.Assembly.ManifestModule.ModuleVersionId + ";pid=" + Process.GetCurrentProcess().Id,
                GitCommit = context.Assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                    .OfType<AssemblyMetadataAttribute>().Where(value => value.Key == "GitCommit")
                    .Select(value => value.Value).SingleOrDefault() ?? string.Empty,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = new List<string>
                { "Observation-only PASS does not qualify character creation. Missing active phase is NOT-RUN." },
                Warnings = new List<string>(), ExceptionSummary = Failures.Count == 0 ? string.Empty : Failures.ToString(),
                EvidenceFiles = new List<string> { Path.Combine(request.EvidenceDirectory, EvidenceFileName) },
                AutomaticExitRequested = request.ExitAfterCompletion, EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions, string name, bool passed, string observed)
        {
            assertions.Add(new RuntimeTestAssertion { Name = name, Expected = "true", Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = EvidenceFileName + "; observation coverage only" });
        }
        private static string Id(object value) { return Identities.Get(value); }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            internal static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();
            public bool Equals(T left, T right) { return ReferenceEquals(left, right); }
            public int GetHashCode(T value) { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
