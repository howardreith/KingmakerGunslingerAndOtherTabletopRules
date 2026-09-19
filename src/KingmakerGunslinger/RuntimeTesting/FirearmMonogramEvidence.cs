using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Feats;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class FirearmMonogramEvidence
    {
        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            if (request.Scenario != RuntimeTestScenarioCatalog.IconOverhaulVisualEvidence &&
                request.Scenario != RuntimeTestScenarioCatalog.ObserveFeatureModuleSettings)
                throw new InvalidOperationException("Exact guarded icon request required.");
            var roots = BlueprintBootstrap.Library.BlueprintsByAssetId.Values.OfType<BlueprintParametrizedFeature>()
                .Where(NativeFirearmFeatIntegration.IsIntegrated).OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            if (roots.Length != 5) throw new InvalidOperationException("Expected exactly five integrated native feat roots.");
            var set = BlueprintBootstrap.FirearmFeats;
            string[] letters = { "P", "M", "B" };
            var rows = new JArray();
            string path = Path.Combine(request.EvidenceDirectory, "firearm-native-monogram-data.json");
            foreach (var root in roots)
            {
                bool pass = true;
                Sprite rootIcon = root.Icon;
                for (int index = 0; index < letters.Length; index++)
                {
                    var parameterBlueprint = set.WeaponFocusChoices[index];
                    Sprite parameterIcon = parameterBlueprint.Icon;
                    var parameter = new FeatureParam(parameterBlueprint);
                    var data = new FeatureUIData(root, parameter);
                    var sheet = new UIFeature(root, parameter);
                    bool exact = Exact(data, root, parameter, letters[index]) && Exact(sheet, root, parameter, letters[index]) &&
                        data.Name == sheet.Name && data.Description == sheet.Description &&
                        rootIcon != null && ReferenceEquals(root.Icon, rootIcon) && parameterIcon != null &&
                        ReferenceEquals(parameterBlueprint.Icon, parameterIcon);
                    pass &= exact;
                    rows.Add(Describe("native-parameter", data, sheet, exact));
                }
                Add(assertions, "firearm-native-selected-" + root.AssetGuid, pass,
                    "P/M/B native selector and character-sheet data preserve exact blueprint parameters, names and fallback sprites", path);
            }
            var rapidMenu = set.RapidReload.Items.ToArray();
            bool rapidExact = rapidMenu.Select(item => item.Feature).SequenceEqual(set.RapidReloadChoices);
            for (int index = 0; index < letters.Length; index++)
            {
                var feature = set.RapidReloadChoices[index];
                Sprite icon = feature.Icon;
                var data = new FeatureUIData(feature);
                var sheet = new UIFeature(feature);
                var menu = rapidMenu.SingleOrDefault(item => ReferenceEquals(item.Feature, feature)) as FeatureUIData;
                bool exact = Exact(data, feature, null, letters[index]) && Exact(sheet, feature, null, letters[index]) &&
                    menu != null && Exact(menu, feature, null, letters[index]) &&
                    menu.Name == feature.Name && menu.Description == feature.Description &&
                    data.Name == feature.Name && sheet.Name == feature.Name && icon != null && ReferenceEquals(feature.Icon, icon);
                rapidExact &= exact;
                rows.Add(Describe("rapid-reload-child", data, sheet, exact));
                rows.Add(new JObject { ["kind"] = "actual-rapid-selection-entry", ["feature"] = feature.AssetGuid,
                    ["entryType"] = menu?.GetType().FullName, ["acronym"] = menu?.NameForAcronim,
                    ["iconIsNull"] = menu != null && menu.Icon == null, ["exact"] = exact });
            }
            Add(assertions, "firearm-native-rapid-children", rapidExact,
                "actual native Rapid Reload Items retain all three identities/order and use native text, with selected and character-sheet data", path);

            bool legacyExact = true;
            foreach (var root in roots)
                foreach (var feature in set.RegisteredWeaponFocusChoices.Skip(3))
                {
                    var parameter = new FeatureParam(feature);
                    var data = new FeatureUIData(root, parameter);
                    var expected = feature.Icon == null ? root.Icon : feature.Icon;
                    string ignored;
                    bool exact = !FirearmNativeMonogramPresentation.TryGetLetter(data, out ignored) &&
                        ReferenceEquals(data.Param, parameter) && ReferenceEquals(data.Icon, expected);
                    legacyExact &= exact;
                    rows.Add(new JObject { { "kind", "hidden-legacy-control" }, { "root", root.AssetGuid },
                        { "parameter", feature.AssetGuid }, { "name", data.Name }, { "exact", exact } });
                }
            Add(assertions, "firearm-native-legacy-preserved", legacyExact,
                "hidden Rifle/Revolver saved parameters retain their original presentation and identities", path);

            var iconGetter = typeof(FeatureUIData).GetMethod("GetParamIcon", BindingFlags.Static | BindingFlags.NonPublic);
            var acronymGetter = typeof(FeatureUIData).GetMethod("GetParamAcronim", BindingFlags.Static | BindingFlags.NonPublic);
            if (iconGetter == null || acronymGetter == null) throw new InvalidOperationException("Native parameter presentation contract changed.");
            var controls = roots.SelectMany(root => root.GetFullSelectionItems())
                .Where(value => value != null && value.Param != null && value.Param.Blueprint == null).ToArray();
            bool controlsExact = controls.Length > 30;
            foreach (var original in controls)
            {
                var data = new FeatureUIData(original.Feature, original.Param);
                var icon = iconGetter.Invoke(null, new object[] { original.Param.Value }) as Sprite;
                if (icon == null) icon = original.Feature.Icon;
                string acronym = (string)acronymGetter.Invoke(null, new object[] { original.Param.Value });
                string ignored;
                bool exact = !FirearmNativeMonogramPresentation.TryGetLetter(data, out ignored) &&
                    ReferenceEquals(data.Param, original.Param) && ReferenceEquals(data.Icon, icon) && data.NameForAcronim == acronym;
                controlsExact &= exact;
                rows.Add(new JObject { { "kind", "native-category-control" }, { "name", original.Name },
                    { "root", original.Feature.AssetGuid }, { "acronym", data.NameForAcronim }, { "exact", exact } });
            }
            Add(assertions, "firearm-native-other-categories-preserved", controlsExact,
                "native/eastern weapon-category data retains native getter results; no global font or icon override", path);
            RuntimeTestResultWriter.WriteAtomic(path, new JObject {
                { "schemaVersion", 1 }, { "runId", request.RunId },
                { "evidenceClass", "native UI data construction; no rendered screen or disk parameter round-trip claim" },
                { "records", rows }
            }.ToString(Formatting.Indented));
            files.Add(path);
        }

        private static bool Exact(FeatureUIData data, BlueprintFeature feature, FeatureParam parameter, string letter) =>
            data != null && ReferenceEquals(data.Feature, feature) && ReferenceEquals(data.Param, parameter) && data.Icon == null &&
            data.NameForAcronim == letter && Kingmaker.UI.Common.UIUtility.GetAbilityAcronym(data.NameForAcronim) == letter &&
            !string.IsNullOrWhiteSpace(data.Name) && !string.IsNullOrWhiteSpace(data.Description);

        private static JObject Describe(string kind, FeatureUIData data, UIFeature sheet, bool exact) => new JObject {
            { "kind", kind }, { "feature", data.Feature.AssetGuid },
            { "parameter", data.Param?.Blueprint?.AssetGuid }, { "name", data.Name },
            { "acronym", data.NameForAcronim }, { "iconIsNull", data.Icon == null },
            { "sheetIconIsNull", sheet.Icon == null }, { "exact", exact }
        };

        private static void Add(ICollection<RuntimeTestAssertion> assertions, string name, bool pass, string expected, string evidence) =>
            assertions.Add(new RuntimeTestAssertion { Name = name, Expected = expected, Observed = pass ? "exact" : "mismatch; inspect rows",
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, Evidence = evidence });
    }
}
