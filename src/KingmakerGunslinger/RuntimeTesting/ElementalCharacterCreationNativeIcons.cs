using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UI.Common;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Feats;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

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
            _nativeIconCapture = CaptureNativeIconSequence(stage).GetEnumerator();
            return true;
        }

        private bool OwnsNativeIconBuild() => _controller != null && _build != null && _build.gameObject.activeInHierarchy &&
            ReferenceEquals(_controller, _build.LevelUpController) &&
            ReferenceEquals(_controller, Game.Instance.UI.LevelUpController) &&
            ReferenceEquals(_controller.Unit, _unit.Descriptor);

        private IEnumerable<int> CaptureNativeIconSequence(string stage)
        {
            foreach (int frame in _nativeIconScreens.Capture(stage, DescribeNativeIconRows(), OwnsNativeIconBuild)) yield return frame;
            if ((_request.Scenario == RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationCase || NativeSheetCase) &&
                (string)_request.Parameters["class"] == "Gunslinger" &&
                (stage.EndsWith(":Total", StringComparison.Ordinal) || stage.EndsWith(":TotalInChargen", StringComparison.Ordinal)))
            {
                var totalRows = (Kingmaker.UI.LevelUp.CharBNewAbilities)typeof(Kingmaker.UI.LevelUp.Phase.CharBPhaseTotal)
                    .GetField("m_CharNewAbilities", Members).GetValue(_build.Total);
                foreach (int frame in CaptureNativeFactSlots(totalRows.transform, _controller.Preview,
                    "creator-total", OwnsNativeIconBuild)) yield return frame;
            }
            const string focus = "1e1f627d26ad36f43bbd26cc2bf8ac7e";
            if (stage != "rendered-selection-ready:" + focus ||
                _request.Scenario != RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationCase) yield break;
            var rows = _build.GetComponentsInChildren<CharBuildSelectorItem>(true).Where(row =>
                row.gameObject.activeInHierarchy && row.Feature?.Feature?.AssetGuid == focus).ToArray();
            var targets = new List<CharBuildSelectorItem>();
            var proficiency = NativeIconExoticProficiency();
            if ((string)_request.Parameters["class"] == "Gunslinger")
                foreach (var parameter in BlueprintBootstrap.FirearmFeats.WeaponFocusChoices)
                    targets.Add(rows.Single(row => (row.Feature as FeatureUIData)?.Param?.Blueprint == parameter));
            else if (proficiency != null)
            {
                if (!_controller.Preview.Progression.Features.HasFact(proficiency))
                    throw new InvalidOperationException("The native Fighter creator has not learned its required exotic proficiency.");
                targets.Add(rows.Single(row => (int?)(row.Feature as FeatureUIData)?.Param?.WeaponCategory == NativeIconExoticCategory));
            }
            if (_context.FeatureModules.Active.EasternWeapons)
                targets.Add(rows.Single(row => (int?)(row.Feature as FeatureUIData)?.Param?.WeaponCategory == 0x004b4d4a));
            foreach (var row in targets)
            {
                var data = (FeatureUIData)row.Feature;
                string name = data.Name;
                foreach (int frame in _nativeIconScreens.CaptureRow("native-weapon-row:" + name,
                    (RectTransform)row.transform, () => {
                        var state = DescribeNativeIconRows();
                        state["targetRow"] = new JObject { ["name"] = name, ["featureGuid"] = data.Feature.AssetGuid,
                            ["parameterGuid"] = data.Param?.Blueprint?.AssetGuid,
                            ["category"] = data.Param?.WeaponCategory.HasValue == true ? (JToken)(int)data.Param.WeaponCategory.Value : null,
                            ["acronym"] = data.NameForAcronim, ["iconIsNull"] = data.Icon == null,
                            ["learnedProficiencyGuid"] = proficiency?.AssetGuid,
                            ["proficiencyPresent"] = proficiency != null && _controller.Preview.Progression.Features.HasFact(proficiency) };
                        return state;
                    }, () => OwnsNativeIconBuild() && row != null && ReferenceEquals(row.Feature, data))) yield return frame;
            }
        }

        private int NativeIconExoticCategory => (string)_request.Parameters["race"] == "Ifrit" ? 0x004b4d48 :
            (string)_request.Parameters["race"] == "Oread" ? 0x004b4d49 : 0x004b4d47;

        private BlueprintFeature NativeIconExoticProficiency()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationCase ||
                (string)_request.Parameters["class"] != "Fighter") return null;
            int category = NativeIconExoticCategory;
            if (!(category == 0x004b4d47 ? _context.FeatureModules.Active.ElvenBranchedSpears :
                _context.FeatureModules.Active.EasternWeapons)) return null;
            string guid = category == 0x004b4d48 ? "b14f7d9b2b665801a9d5b916c6be4ea9" :
                category == 0x004b4d49 ? "93ef81404f085e2a8b261bdab15d5a08" : "017d586ec4546feabf6eaaa67ce74a3f";
            return BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library, guid,
                "native icon fixture exotic proficiency");
        }

        private IEnumerable<int> CaptureNativeFactSlots(Transform root, UnitDescriptor owner,
            string surface, Func<bool> ownsUi)
        {
            var slots = root.GetComponentsInChildren<CharSComponentAbilitySlot>(true)
                .Where(slot => slot.gameObject.activeInHierarchy && NativeSlotFact(slot) != null &&
                    ReferenceEquals(NativeSlotFact(slot).Owner, owner)).ToArray();
            var targets = slots.Where(slot => {
                string ignored;
                return FirearmNativeMonogramPresentation.TryGetLetter(new FeatureUIData(NativeSlotFact(slot)), out ignored);
            }).ToArray();
            if (targets.Length != 1)
                throw new InvalidOperationException("Expected one actual selected firearm fact in " + surface + "; found " + targets.Length);
            foreach (var slot in targets)
            {
                var fact = NativeSlotFact(slot);
                var parameter = fact.Param;
                var blueprintIcon = fact.Blueprint.Icon;
                var factIcon = fact.Icon;
                var data = new FeatureUIData(fact);
                string letter;
                if (!FirearmNativeMonogramPresentation.TryGetLetter(data, out letter))
                    throw new InvalidOperationException("Selected firearm fact identity changed.");
                var glyph = (TextMeshProUGUI)typeof(CharSComponentAbilitySlot).GetField("m_AcronimText", Members).GetValue(slot);
                var background = NativeSlotBackground(slot);
                bool controlsExact = slots.Length > targets.Length && slots.Except(targets).All(control =>
                    ReferenceEquals(control.Icon.sprite, NativeSlotFact(control).Icon ?? NativeSlotBackground(control)));
                Func<bool> retained = () => ownsUi() && slot != null && ReferenceEquals(NativeSlotFact(slot), fact) &&
                    ReferenceEquals(fact.Param, parameter) && ReferenceEquals(fact.Blueprint.Icon, blueprintIcon) &&
                    ReferenceEquals(fact.Icon, factIcon);
                foreach (int frame in _nativeIconScreens.CaptureRow("native-selected-fact:" + fact.Name,
                    (RectTransform)slot.transform, () => new JObject {
                        ["surface"] = surface, ["ownerId"] = owner.Unit.UniqueId,
                        ["targetRow"] = new JObject { ["name"] = fact.Name, ["featureGuid"] = fact.Blueprint.AssetGuid,
                            ["parameterGuid"] = parameter?.Blueprint?.AssetGuid,
                            ["expectedLetter"] = letter, ["nativeGlyph"] = glyph?.GetParsedText(),
                            ["glyphActive"] = glyph != null && glyph.isActiveAndEnabled,
                            ["glyphTruncated"] = glyph == null || glyph.isTextTruncated,
                            ["glyphOverflowing"] = glyph == null || glyph.isTextOverflowing,
                            ["font"] = glyph?.font?.name,
                            ["nativeBackground"] = ReferenceEquals(slot.Icon.sprite, background),
                            ["factAndParameterRetained"] = retained(), ["fallbackIconPresent"] = blueprintIcon != null,
                            ["otherFactRows"] = slots.Length - targets.Length, ["otherFactIconsExact"] = controlsExact }
                    }, retained)) yield return frame;
                // Keep a diagnostic native image before rejecting an incorrect
                // presentation. This checks actual rendered slots, not data alone.
                if (glyph == null || !glyph.isActiveAndEnabled || glyph.GetParsedText() != letter ||
                    glyph.isTextTruncated || glyph.isTextOverflowing || !ReferenceEquals(slot.Icon.sprite, background) ||
                    blueprintIcon == null || !controlsExact || !retained())
                    throw new InvalidOperationException("Actual native selected-fact presentation differs for " + data.Name);
            }
        }

        private static Feature NativeSlotFact(CharSComponentAbilitySlot slot) => slot.Feature ?? slot.IUIDataProvider as Feature;
        private static Sprite NativeSlotBackground(CharSComponentAbilitySlot slot) =>
            UIUtility.GetIconByText(slot.Feature != null ? slot.Feature.Blueprint.name : slot.IUIDataProvider.Name);

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
        // disposable creator case. No prerequisites are changed.
        private IFeatureSelectionItem PreferredNativeIconChoice(IFeatureSelectionItem[] legal)
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationCase && !NativeSheetCase) return null;
            const string focus = "1e1f627d26ad36f43bbd26cc2bf8ac7e";
            var proficiency = NativeIconExoticProficiency();
            if (proficiency != null)
            {
                // A level-one Fighter has two native feat choices. Learn the
                // actual proficiency first, then choose Weapon Focus normally.
                // This never grants facts directly or changes eligibility.
                if (!_controller.Preview.Progression.Features.HasFact(proficiency))
                    return legal.SingleOrDefault(item => ReferenceEquals(item.Feature, proficiency)) ??
                        legal.SingleOrDefault(item => item.Feature.AssetGuid == "9a01b6815d6c3684cb25f30b8bf20932");
                return legal.OfType<FeatureUIData>().SingleOrDefault(item => item.Feature.AssetGuid == focus &&
                    (int?)item.Param?.WeaponCategory == NativeIconExoticCategory) ??
                    legal.SingleOrDefault(item => item.Feature.AssetGuid == focus);
            }
            if ((string)_request.Parameters["class"] != "Gunslinger") return null;
            string race = (string)_request.Parameters["race"];
            if (race == "Undine")
            {
                var rapid = legal.SingleOrDefault(item => ReferenceEquals(item.Feature, BlueprintBootstrap.FirearmFeats.RapidReload));
                if (rapid != null) return rapid;
                return legal.SingleOrDefault(item => ReferenceEquals(item.Feature,
                    BlueprintBootstrap.FirearmFeats.RapidReloadChoices[NativeSheetCase ? _raceIndex : 0]));
            }
            var parameter = BlueprintBootstrap.FirearmFeats.WeaponFocusChoices[NativeSheetCase ? _raceIndex :
                race == "Ifrit" ? 0 : race == "Oread" ? 1 : 2];
            var exact = legal.OfType<FeatureUIData>().SingleOrDefault(item => item.Feature.AssetGuid == focus &&
                item.Param != null && ReferenceEquals(item.Param.Blueprint, parameter));
            return exact ?? legal.SingleOrDefault(item => item.Feature.AssetGuid == focus);
        }
    }
}
