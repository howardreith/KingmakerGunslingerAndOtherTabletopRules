using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Feats;
using Newtonsoft.Json.Linq;
using TMPro;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        private NativeIconScreenEvidence _nativeIconScreens;
        private IEnumerator<int> _nativeIconCapture;
        private readonly HashSet<string> _nativeIconStages = new HashSet<string>(StringComparer.Ordinal);

        private bool PollNativeIconCapture()
        {
            if (_nativeIconCapture == null) return false;
            if (_nativeIconCapture.MoveNext()) return true;
            DisposeNativeIconCapture();
            return false;
        }

        private void DisposeNativeIconCapture()
        {
            var capture = _nativeIconCapture;
            _nativeIconCapture = null;
            capture?.Dispose();
        }

        private bool PauseForNativeIconScreen(string key, string stage)
        {
            if (!NativeIconScreenEvidence.Supports(_request) || !_nativeIconStages.Add(_raceIndex + "|" + key)) return false;
            if (_nativeIconScreens == null) _nativeIconScreens = new NativeIconScreenEvidence(_request);
            _nativeIconCapture = _nativeIconScreens.Capture(stage,
                DescribeNativeIconRows(),
                () => _controller != null && _build != null && _build.gameObject.activeInHierarchy &&
                    ReferenceEquals(_controller, _build.LevelUpController) &&
                    ReferenceEquals(_controller, Game.Instance.UI.LevelUpController) &&
                    ReferenceEquals(_controller.Unit, _unit.Descriptor)).GetEnumerator();
            return true;
        }

        private JObject DescribeNativeIconRows()
        {
            var state = ElementalCharacterCreationRoutingObserver.DescribeActiveBuild();
            int rapidRows = 0;
            foreach (var row in _build.GetComponentsInChildren<CharBuildSelectorItem>(true)
                .Where(value => value.gameObject.activeInHierarchy && value.Feature != null))
            {
                string letter;
                if (!FirearmNativeMonogramPresentation.TryGetRapidLetter(row.Feature.Feature, out letter)) continue;
                if (row.Feature.Icon != null || row.Feature.NameForAcronim != letter ||
                    !row.GetComponentsInChildren<TextMeshProUGUI>(true).Any(text => text.isActiveAndEnabled && text.GetParsedText() == letter))
                    throw new InvalidOperationException("Actual native Rapid Reload row did not render its expected TMP glyph: " + row.Feature.Name);
                rapidRows++;
            }
            state["verifiedNativeRapidRows"] = rapidRows;
            state["visibleIconRows"] = new JArray(_build.GetComponentsInChildren<CharBuildSelectorItem>(true)
                .Where(row => row.gameObject.activeInHierarchy && row.Feature != null).Select(row => new JObject {
                    ["featureGuid"] = row.Feature.Feature?.AssetGuid,
                    ["entryType"] = row.Feature.GetType().FullName,
                    ["parameterGuid"] = (row.Feature as FeatureUIData)?.Param?.Blueprint?.AssetGuid,
                    ["displayName"] = row.Feature.Name,
                    ["icon"] = row.Feature.Icon == null ? null : row.Feature.Icon.name,
                    ["nativeAcronym"] = row.Feature.NameForAcronim,
                    ["selected"] = row.Toggle != null && row.Toggle.isOn,
                    ["texts"] = new JArray(row.GetComponentsInChildren<TextMeshProUGUI>(true)
                        .Where(text => text.isActiveAndEnabled).Select(text => new JObject {
                            ["object"] = text.name, ["parsedText"] = text.GetParsedText(),
                            ["font"] = text.font == null ? null : text.font.name, ["fontSize"] = text.fontSize,
                            ["truncated"] = text.isTextTruncated, ["overflowing"] = text.isTextOverflowing })) }));
            return state;
        }

        private bool CaptureNativeIconSelection(FeatureSelectionState selection, string guid) =>
            PauseForNativeIconScreen("selection-" + guid + "-" + RuntimeHelpers.GetHashCode(selection),
                "rendered-selection-ready:" + guid);

        // Choose already legal and visibly rendered native entries in the exact
        // disposable Gunslinger creator case. No prerequisites are changed.
        private IFeatureSelectionItem PreferredNativeIconChoice(IFeatureSelectionItem[] legal)
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationCase ||
                (string)_request.Parameters["class"] != "Gunslinger") return null;
            string race = (string)_request.Parameters["race"];
            if (race == "Undine")
            {
                var rapid = legal.SingleOrDefault(item => ReferenceEquals(item.Feature, BlueprintBootstrap.FirearmFeats.RapidReload));
                if (rapid != null) return rapid;
                return legal.SingleOrDefault(item => ReferenceEquals(item.Feature, BlueprintBootstrap.FirearmFeats.RapidReloadChoices[0]));
            }
            const string focus = "1e1f627d26ad36f43bbd26cc2bf8ac7e";
            var parameter = BlueprintBootstrap.FirearmFeats.WeaponFocusChoices[race == "Ifrit" ? 0 : race == "Oread" ? 1 : 2];
            var exact = legal.OfType<FeatureUIData>().SingleOrDefault(item => item.Feature.AssetGuid == focus &&
                item.Param != null && ReferenceEquals(item.Param.Blueprint, parameter));
            return exact ?? legal.SingleOrDefault(item => item.Feature.AssetGuid == focus);
        }
    }
}
