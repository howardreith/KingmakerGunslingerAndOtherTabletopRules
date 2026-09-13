using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.UI.GlobalMap;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private NativeIconScreenEvidence _teleportationControlScreens;

        private IEnumerable<int> CaptureNativeStrategicControls(GlobalMapMessageBox panel,
            TeleportDestinationRows rows, Func<bool> resourcesRetained)
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationInteraction) yield break;
            if (!_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete ||
                _workingSaveSmoke.WriteObserved || _teleportationControlScreens != null || rows == null || resourcesRetained == null)
                throw new InvalidOperationException("Exact guarded strategic interaction and existing destination rows required.");
            var buttons = rows.Buttons.ToArray();
            var keys = rows.Actions.Select(value => value.Key).ToArray();
            var labels = rows.Actions.Select(value => TeleportContextPresentation.CompactRow(value, TeleportationText.Get)).ToArray();
            if (buttons.Length != 6 || keys.Distinct().Count() != 6 || labels.Length != 6)
                throw new InvalidOperationException("Expected the six existing prepared/spontaneous destination controls.");
            var nativeControls = panel.GetComponentsInChildren<Button>(true).Where(value =>
                !value.transform.IsChildOf(rows.transform) && value.IsActive() && value.IsInteractable()).ToArray();
            if (nativeControls.Length == 0) throw new InvalidOperationException("Native destination control evidence is empty.");
            string nativeBefore = TeleportationNativeButtons(panel);
            var originals = buttons.Concat(nativeControls).SelectMany(value => value.GetComponentsInChildren<Image>(true))
                .Distinct().Select(value => new { Image = value, Sprite = value.sprite, Enabled = value.enabled,
                    Active = value.gameObject.activeSelf }).ToArray();
            var party = Game.Instance.Player.Party.ToArray();
            var time = Game.Instance.Player.GameTime;
            var position = GlobalMapRules.State.PartyPosition;
            var canonical = new[] { BlueprintBootstrap.Teleportation.Teleport, BlueprintBootstrap.Teleportation.GreaterTeleport,
                BlueprintBootstrap.Teleportation.WordOfRecall };
            var sprites = canonical.Select(value => value.Icon).ToArray();
            if (sprites.Any(value => value == null)) throw new InvalidOperationException("Canonical strategic icon identity is unavailable.");
            Func<bool> imagesRetained = () => originals.All(value => value.Image != null &&
                ReferenceEquals(value.Image.sprite, value.Sprite) && value.Image.enabled == value.Enabled &&
                value.Image.gameObject.activeSelf == value.Active);
            // Production Update refreshes action snapshots every frame. Retain
            // the actual Buttons and stable keys/text, not obsolete snapshots.
            Func<bool> retained = () => panel != null && panel.gameObject.activeInHierarchy && rows != null &&
                rows.gameObject.activeInHierarchy && rows.Buttons.SequenceEqual(buttons) &&
                rows.Actions.Select(value => value.Key).SequenceEqual(keys) &&
                rows.Actions.Select(value => TeleportContextPresentation.CompactRow(value, TeleportationText.Get)).SequenceEqual(labels) &&
                nativeBefore == TeleportationNativeButtons(panel) && imagesRetained() && resourcesRetained() &&
                Game.Instance.CurrentMode == GameModeType.GlobalMap && Game.Instance.Player.Party.SequenceEqual(party) &&
                Game.Instance.Player.GameTime == time && Equals(GlobalMapRules.State.PartyPosition, position) &&
                GlobalMapRules.State.TravelData == null && !TeleportContextConfirmationPresenter.Pending && !_workingSaveSmoke.WriteObserved;
            if (!retained()) throw new InvalidOperationException("Native destination context is not stable before capture.");
            var exceptions = new List<string>();
            Application.LogCallback observe = (message, stack, kind) => {
                if (kind == LogType.Exception || kind == LogType.Error || kind == LogType.Assert)
                    exceptions.Add(kind + ": " + message + "\n" + stack);
            };
            Application.logMessageReceived += observe;
            _teleportationControlScreens = new NativeIconScreenEvidence(_request);
            try
            {
                using (_teleportationControlScreens)
                    for (int index = 0; index < buttons.Length; index++)
                    {
                        int targetIndex = index;
                        var button = buttons[index];
                        var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
                        var images = button.GetComponentsInChildren<Image>(true);
                        if (label == null || images.Length == 0) throw new InvalidOperationException("Native text button has no label/background.");
                        Func<JObject> describe = () => {
                            var action = rows.Actions[targetIndex];
                            var spell = action.Source.Spell == TeleportSpellKind.Teleport ? canonical[0] :
                                action.Source.Spell == TeleportSpellKind.GreaterTeleport ? canonical[1] : null;
                            return new JObject { ["surface"] = "native-strategic-text-control",
                                ["presentation"] = "native two-line text button; no spell image assignment",
                                ["targetRow"] = new JObject { ["name"] = labels[targetIndex], ["sourceKey"] = action.Key,
                                    ["spellKind"] = action.Source.Spell.ToString(), ["spellGuid"] = spell?.AssetGuid,
                                    ["sourceKind"] = action.Source.Kind.ToString(), ["bookGuid"] = action.Source.BookId,
                                    ["casterId"] = action.Source.CasterId, ["uses"] = action.Source.Uses,
                                    ["labelExact"] = label.isActiveAndEnabled && label.text == labels[targetIndex] &&
                                        string.Equals(label.GetParsedText(), labels[targetIndex], StringComparison.OrdinalIgnoreCase),
                                    ["labelTruncated"] = label.isTextTruncated, ["labelOverflowing"] = label.isTextOverflowing,
                                    ["nativeFont"] = label.font?.name, ["buttonActive"] = button.IsActive(),
                                    ["buttonInteractable"] = button.IsInteractable(), ["nativeImageCount"] = images.Length,
                                    ["canonicalSpellImages"] = images.Count(value => sprites.Contains(value.sprite)),
                                    ["nativeImageNames"] = new JArray(images.Select(value => value.sprite?.name)),
                                    ["nativeImagesRetained"] = imagesRetained(), ["nativeControlCount"] = nativeControls.Length,
                                    ["nativeControlsRetained"] = nativeBefore == TeleportationNativeButtons(panel),
                                    ["contextAndResourcesRetained"] = retained() } };
                        };
                        foreach (int frame in _teleportationControlScreens.CaptureRow("native-strategic-control:" + index,
                            (RectTransform)button.transform, describe, retained)) yield return frame;
                        var target = (JObject)describe()["targetRow"];
                        bool exact = (bool)target["labelExact"] && !(bool)target["labelTruncated"] && !(bool)target["labelOverflowing"] &&
                            (bool)target["buttonActive"] && (bool)target["buttonInteractable"] && (int)target["canonicalSpellImages"] == 0 &&
                            (bool)target["nativeImagesRetained"] && (bool)target["nativeControlsRetained"] && (bool)target["contextAndResourcesRetained"];
                        TeleportInteractionAssert("native-text-control-" + index, "exact visible native text control, original background and resources",
                            target.ToString(Newtonsoft.Json.Formatting.None), exact);
                        if (!exact) throw new InvalidOperationException("Native strategic text control differs: " + keys[index]);
                    }
                TeleportInteractionAssert("native-text-context", "all six real text controls captured with unchanged original context/resources",
                    "retained=" + retained(), retained());
            }
            finally
            {
                Application.logMessageReceived -= observe;
                CaptureTeleportInteraction("native-text-control-cleanup", new { retained = retained(), exceptions = exceptions.ToArray() });
                TeleportInteractionAssert("native-text-exceptions", "no UI exceptions during native control capture",
                    "count=" + exceptions.Count, exceptions.Count == 0);
            }
        }
    }
}
