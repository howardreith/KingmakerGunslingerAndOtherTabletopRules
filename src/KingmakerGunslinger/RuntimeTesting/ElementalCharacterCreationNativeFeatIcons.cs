using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.UI.Common;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        private readonly HashSet<int> _nativeRacialFeatMenus = new HashSet<int>();
        private bool NativeRacialFeatCase => _request.Scenario == RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationCase &&
            _request.ExitAfterCompletion && _context.FeatureModules.Active.ElementalRaces &&
            (string)_request.Parameters["class"] == "Gunslinger";

        private IEnumerable<int> CaptureNativeRacialFeatMenu()
        {
            if (!NativeRacialFeatCase || _nativeRacialFeatMenus.Contains(_raceIndex)) yield break;
            var allFeats = BlueprintBootstrap.ElementalFeats.AllFeats();
            var seed = _build.GetComponentsInChildren<CharBuildSelectorItem>(true).FirstOrDefault(row =>
                row.gameObject.activeInHierarchy && row.Feature != null && allFeats.Contains(row.Feature.Feature));
            if (seed == null) yield break;
            var selection = seed.FeatureSelection;
            var extracted = selection.Selection.ExtractSelectionItems(_controller.Unit, _controller.Preview).Select(value => value.Feature).ToArray();
            // Wait for the ordinary general-feat selection, not a class subset.
            if (!allFeats.All(extracted.Contains)) yield break;
            var selector = seed.GetComponentInParent<CharBFeatureSelector>();
            if (selector == null || selector.Filter == null || selector.FilterState == null)
                throw new InvalidOperationException("Native racial feat selector/filter is unavailable.");
            var filter = selector.Filter; var state = selector.FilterState;
            var toggle = (Toggle)typeof(Kingmaker.UI.LevelUp.CharBSelectorFilter).GetField("m_ShowAllButton", Members).GetValue(filter);
            if (toggle == null || !toggle.isActiveAndEnabled || !toggle.interactable || toggle.isOn != state.ShowAll)
                throw new InvalidOperationException("Native ShowAll toggle is unavailable or differs from its filter state.");
            _nativeRacialFeatMenus.Add(_raceIndex);
            var race = (ElementalHeritageRace)Enum.Parse(typeof(ElementalHeritageRace), (string)_request.Parameters["race"]);
            var expected = ElementalFeatPolicy.Ordered().Where(value => value.AllowedRaces.Contains(race))
                .Select(value => BlueprintBootstrap.ElementalFeats.RequireFeature(value.Id)).ToArray();
            var preview = _controller.Preview;
            var facts = preview.Progression.Features.Enumerable.ToArray();
            var selected = selection.SelectedItem;
            bool originalShowAll = state.ShowAll; var originalKind = state.State;
            var scrolls = selector.GetComponentsInChildren<Component>(true)
                .Concat(seed.GetComponentsInParent<Component>(true)).Distinct()
                .Where(value => value is ScrollRect || value is ScrollRectExtended)
                .Select(value => new NativeIconScreenEvidence.NativeRowScroll(value))
                .Where(value => value.Active && value.Vertical && value.Content != null && value.Viewport != null)
                .Select(value => new { Scroll = value, Position = value.Position, Horizontal = value.Horizontal,
                    ContentPosition = value.Content.anchoredPosition, Velocity = value.Velocity }).ToArray();
            if (!scrolls.Any(value => seed.transform.IsChildOf(value.Scroll.Content)))
                throw new InvalidOperationException("Original native feat row has no owned scroll viewport.");
            var evidence = new JObject { ["race"] = race.ToString(), ["originalShowAll"] = originalShowAll,
                ["originalFilter"] = originalKind.ToString(), ["expectedFeatGuids"] = new JArray(expected.Select(value => value.AssetGuid)),
                ["capturedFeatGuids"] = new JArray(), ["restored"] = false };
            _character["nativeRacialFeatMenu"] = evidence;
            Func<bool> retained = () => OwnsNativeIconBuild() && ReferenceEquals(_controller.Preview, preview) &&
                ReferenceEquals(selector.FilterState, state) && ReferenceEquals(selection.SelectedItem, selected) &&
                preview.Progression.Features.Enumerable.SequenceEqual(facts);
            bool filterRestored = false;
            try
            {
                // The real Toggle invokes its native HandleShowAll listener,
                // including native text/arrow state and list refresh.
                toggle.isOn = true;
                for (int frame = 0; frame < 24; frame++) yield return 0;
                if (!retained() || !state.ShowAll || !toggle.isOn)
                    throw new InvalidOperationException("Native ShowAll did not preserve its original selection/preview.");
                foreach (var feat in expected)
                {
                    var rows = selector.GetComponentsInChildren<CharBuildSelectorItem>(true).Where(candidate =>
                        candidate.gameObject.activeInHierarchy && ReferenceEquals(candidate.FeatureSelection, selection) && candidate.Feature != null).ToArray();
                    var row = rows.Single(value => ReferenceEquals(value.Feature.Feature, feat));
                    var data = row.Feature;
                    var icon = (Image)typeof(CharBuildSelectorItem).GetField("m_ItemIcon", Members).GetValue(row);
                    var title = (TextMeshProUGUI)typeof(CharBuildSelectorItem).GetField("m_ItemName", Members).GetValue(row);
                    var originalSprite = feat.Icon;
                    bool originalInteractable = row.Toggle.interactable;
                    string originalEligibility = selection.GetViewState(data, _controller).CanSelectState.ToString();
                    var markers = new[] { "m_DisableMark", "m_ForbiddenMark", "m_AllreadyUsedMark" }
                        .Select(name => new { Name = name, Image = (Image)typeof(CharBuildSelectorItem).GetField(name, Members).GetValue(row) })
                        .Select(value => new { value.Name, value.Image, Active = value.Image.gameObject.activeSelf,
                            Enabled = value.Image.enabled, Sprite = value.Image.sprite }).ToArray();
                    Func<bool> markersRetained = () => markers.All(value => value.Image != null &&
                        value.Image.gameObject.activeSelf == value.Active && value.Image.enabled == value.Enabled &&
                        ReferenceEquals(value.Image.sprite, value.Sprite));
                    var controls = rows.Where(value => !allFeats.Contains(value.Feature.Feature) && value.Feature.Icon != null).ToArray();
                    Func<bool> exact = () => retained() && row != null && row.gameObject.activeInHierarchy &&
                        ReferenceEquals(row.Feature, data) && ReferenceEquals(feat.Icon, originalSprite) &&
                        row.Toggle.interactable == originalInteractable && markersRetained() &&
                        selection.GetViewState(data, _controller).CanSelectState.ToString() == originalEligibility;
                    Func<JObject> describe = () => new JObject { ["surface"] = "native-racial-feat-selector", ["race"] = race.ToString(),
                        ["showAll"] = state.ShowAll && toggle.isOn, ["expectedFeatGuids"] = new JArray(expected.Select(value => value.AssetGuid)),
                        ["targetRow"] = new JObject { ["featureGuid"] = feat.AssetGuid, ["name"] = data.Name,
                            ["iconName"] = feat.Icon?.name, ["renderedIconExact"] = icon.isActiveAndEnabled && ReferenceEquals(icon.sprite, feat.Icon) && ReferenceEquals(data.Icon, feat.Icon),
                            ["titleExact"] = title.isActiveAndEnabled && !title.isTextTruncated && string.Equals(title.GetParsedText(), data.Name, StringComparison.OrdinalIgnoreCase),
                            ["nativeEligibility"] = originalEligibility, ["nativeInteractable"] = originalInteractable,
                            ["nativeMarkers"] = new JObject(markers.Select(value => new JProperty(value.Name, value.Active))),
                            ["nativeMarkersRetained"] = markersRetained(),
                            ["selectionAndEligibilityRetained"] = exact(), ["otherIconRows"] = controls.Length,
                            ["otherIconsExact"] = controls.All(value => ReferenceEquals(((Image)typeof(CharBuildSelectorItem)
                                .GetField("m_ItemIcon", Members).GetValue(value)).sprite, value.Feature.Icon)) } };
                    foreach (int frame in _nativeIconScreens.CaptureRow("native-racial-feat-row:" + feat.AssetGuid,
                        (RectTransform)row.transform, describe, exact)) yield return frame;
                    var target = (JObject)describe()["targetRow"];
                    if (!(bool)target["renderedIconExact"] || !(bool)target["titleExact"] || !(bool)target["selectionAndEligibilityRetained"] ||
                        (int)target["otherIconRows"] == 0 || !(bool)target["otherIconsExact"])
                        throw new InvalidOperationException("Actual native racial feat icon/name/control differs for " + feat.name);
                    ((JArray)evidence["capturedFeatGuids"]).Add(feat.AssetGuid);
                }
                toggle.isOn = originalShowAll;
                for (int frame = 0; frame < 24; frame++) yield return 0;
                Canvas.ForceUpdateCanvases();
                foreach (var value in scrolls)
                {
                    if (!value.Scroll.Active) throw new InvalidOperationException("Original native feat scroll was replaced.");
                    value.Scroll.RestorePosition(value.Position);
                    value.Scroll.Velocity = value.Velocity;
                }
                for (int frame = 0; frame < 4; frame++) yield return 0;
                filterRestored = retained() && state.ShowAll == originalShowAll && toggle.isOn == originalShowAll && state.State == originalKind &&
                    scrolls.All(value => value.Scroll.Active && value.Scroll.Horizontal == value.Horizontal &&
                        (value.Scroll.Content.anchoredPosition - value.ContentPosition).sqrMagnitude < 0.0001f &&
                        (value.Scroll.Velocity - value.Velocity).sqrMagnitude < 0.0001f);
                evidence["restored"] = filterRestored;
                evidence["selectionAndFactsRetained"] = retained();
                evidence["nativeScrollCount"] = scrolls.Length;
                if (!filterRestored) throw new InvalidOperationException("Native ShowAll filter/scroll did not restore its exact original context.");
            }
            finally
            {
                // Normal success waits for native layout above. On an error,
                // restore the owned filter before outer creator cleanup runs;
                // preserve the first exception and leave restored=false.
                if (!filterRestored && retained()) toggle.isOn = originalShowAll;
            }
        }

        private void AppendNativeRacialFeatAssertion()
        {
            if (!NativeRacialFeatCase) return;
            var race = (ElementalHeritageRace)Enum.Parse(typeof(ElementalHeritageRace), (string)_request.Parameters["race"]);
            var expected = ElementalFeatPolicy.Ordered().Where(value => value.AllowedRaces.Contains(race))
                .Select(value => BlueprintBootstrap.ElementalFeats.RequireFeature(value.Id).AssetGuid).ToArray();
            var menus = _characters.OfType<JObject>().Select(value => value["nativeRacialFeatMenu"] as JObject).ToArray();
            bool passed = menus.Length == _races.Length && menus.All(value => value != null &&
                (bool?)value["restored"] == true && (bool?)value["selectionAndFactsRetained"] == true &&
                (int?)value["nativeScrollCount"] > 0 &&
                ((JArray)value["capturedFeatGuids"]).Values<string>().SequenceEqual(expected));
            Result.Assertions.Add(new RuntimeTestAssertion { Name = "actual-native-racial-feat-menu",
                Expected = race + ": " + expected.Length + " exact rows; native eligibility and original filter/selection/facts/scroll restored",
                Observed = new JArray(menus.Select(value => (JToken)value ?? JValue.CreateNull())).ToString(Newtonsoft.Json.Formatting.None),
                Status = passed ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, Evidence = EvidenceFileName });
        }
    }
}
