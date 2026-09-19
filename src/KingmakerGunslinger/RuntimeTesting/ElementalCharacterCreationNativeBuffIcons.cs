using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        private bool NativeRacialBuffCase => NativeSheetCase && _request.ExitAfterCompletion &&
            _context.FeatureModules.Active.ElementalRaces;

        private string[] NativeRacialBuffSymbols()
        {
            switch ((string)_request.Parameters["race"])
            {
                case "Ifrit": return new[] { "Feats.ElementalStrike.Buff", "Feats.ScorchingWeapons.Buff", "Feats.BlazingAura.Buff", "Traits.Ifrit.FireInTheBlood.FastHealingBuff" };
                case "Oread": return new[] { "Traits.Oread.StoneInTheBlood.FastHealingBuff", "Traits.Oread.CrystallineForm.ArmedBuff", "Traits.Oread.TreacherousEarth.TerrainBuff" };
                case "Sylph": return new[] { "Feats.WingsOfAir.Buff", "Traits.Sylph.StormInTheBlood.FastHealingBuff", "Traits.Sylph.BreezeKissed.CalmedBuff" };
                case "Undine": return new[] { "Traits.Undine.NereidFascination.FascinatedBuff", "Traits.Undine.NereidFascination.AssistanceBuff", "Traits.Undine.NereidFascination.AuraBuff" };
                default: throw new InvalidOperationException("Unsupported native racial buff review race.");
            }
        }

        private IEnumerable<int> CaptureNativeRacialBuffSheet(CharacterScreenController sheet,
            UnitEntityData owner, Func<bool> ownsSheet)
        {
            if (!NativeRacialBuffCase || _raceIndex != 0) yield break;
            var game = Game.Instance;
            var activeByDefault = typeof(FactCollection).GetProperty("ActiveByDefault", Members);
            bool originalOwnerTurnedOn = owner.Descriptor.IsTurnedOn;
            // A registered remote mercenary keeps its descriptor turned on while
            // its native buff collection is inactive. Preserve that exact state;
            // AddFact's activation boundary is the collection, not the descriptor.
            Func<bool> dormant = () => !owner.IsInGame && owner.Descriptor.IsTurnedOn == originalOwnerTurnedOn &&
                activeByDefault != null && !(bool)activeByDefault.GetValue(owner.Buffs, null);
            var setup = new JObject { ["stage"] = "native-prerequisites", ["ownerId"] = owner.UniqueId,
                ["ownsSheet"] = ownsSheet(), ["ownerInGame"] = owner.IsInGame,
                ["ownerTurnedOn"] = owner.Descriptor.IsTurnedOn, ["activeByDefaultProperty"] = activeByDefault != null,
                ["buffsActiveByDefault"] = activeByDefault == null ? JValue.CreateNull() : new JValue((bool)activeByDefault.GetValue(owner.Buffs, null)),
                ["loadedEvidencePresent"] = _loaded != null, ["guardOwned"] = ReferenceEquals(_saveGuardOwner, this),
                ["sameOwner"] = ReferenceEquals(owner, _unit), ["committed"] = _committed,
                ["inParty"] = game.Player.Party.Contains(owner), ["customCompanion"] = owner.Descriptor.IsCustomCompanion(),
                ["originalBuffs"] = new JArray(owner.Buffs.Enumerable.Select(value => new JObject {
                    ["guid"] = value.Blueprint.AssetGuid, ["active"] = value.Active, ["turnedOn"] = value.IsTurnedOn })) };
            _character["nativeRacialBuffSetup"] = setup;
            Write();
            if (!ownsSheet() || !dormant() || _loaded == null || _loaded.SaveWritingApiObserved ||
                !ReferenceEquals(_saveGuardOwner, this) || !ReferenceEquals(owner, _unit) || !_committed ||
                game.Player.Party.Contains(owner) || !owner.Descriptor.IsCustomCompanion())
                throw new InvalidOperationException("Native buff review requires the exact dormant remote mercenary and guarded sheet: " + setup);
            var symbols = NativeRacialBuffSymbols().Select(value => "KMG.ElementalRaces." + value).ToArray();
            var bindings = OwnedIconAssignments.Bindings.Where(value => value.BlueprintType == typeof(BlueprintBuff) && symbols.Contains(value.Symbol)).ToArray();
            setup["stage"] = "resolve-owned-buffs";
            setup["bindingSymbols"] = new JArray(bindings.Select(value => value.Symbol));
            Write();
            if (bindings.Length != symbols.Length || bindings.Select(value => value.Symbol).Distinct().Count() != symbols.Length)
                throw new InvalidOperationException("Racial buff review bindings are incomplete or ambiguous.");
            var manifest = BlueprintManifest.Load(_context.ModEntry.Path);
            var blueprints = bindings.Select(value => (BlueprintBuff)value.Resolve(BlueprintBootstrap.Library, manifest)).ToArray();
            var controls = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintBuff>().Where(value => value.name == "BlessBuff").ToArray();
            setup["stage"] = "resolve-native-control";
            setup["ownedBuffs"] = new JArray(blueprints.Select(value => new JObject {
                ["guid"] = value.AssetGuid, ["hidden"] = value.IsHiddenInUI, ["icon"] = value.Icon?.name }));
            setup["nativeBlessMatches"] = new JArray(controls.Select(value => value.AssetGuid));
            Write();
            var control = controls.Single();
            if (blueprints.Concat(new[] { control }).Any(value => value.IsHiddenInUI || value.Icon == null) || blueprints.Contains(control))
                throw new InvalidOperationException("Native buff review requires visible owned paintings and an ordinary native control.");
            var originalBuffs = owner.Buffs.Enumerable.ToArray();
            if (originalBuffs.Any(value => blueprints.Contains(value.Blueprint) || ReferenceEquals(value.Blueprint, control)))
                throw new InvalidOperationException("A review buff already belongs to the mercenary; it must not be replaced.");
            var originalFeatures = owner.Descriptor.Progression.Features.Enumerable.ToArray();
            var originalAbilities = owner.Abilities.Enumerable.ToArray();
            var originalAreas = game.State.AreaEffects.All.ToArray();
            var originalUnits = game.State.Units.All.ToArray();
            var originalOthers = originalUnits.Where(value => !ReferenceEquals(value, owner)).Select(value => new {
                Unit = value, Buffs = value.Buffs.Enumerable.ToArray(), value.Damage, value.Position }).ToArray();
            var originalParty = game.Player.Party.ToArray();
            var originalTime = game.Player.GameTime;
            var added = new List<Buff>();
            var exceptions = new List<string>();
            var evidence = new JObject { ["race"] = (string)_request.Parameters["race"], ["ownerId"] = owner.UniqueId,
                ["presentation"] = "native inactive buff rows on the dormant remote mercenary; no effect activation",
                ["expectedBuffGuids"] = new JArray(blueprints.Select(value => value.AssetGuid)),
                ["capturedBuffGuids"] = new JArray(), ["restored"] = false };
            _character["nativeRacialBuffSheet"] = evidence;
            setup["stage"] = "native-fixture-ready";
            Func<bool> worldRetained = () => dormant() && game.IsPaused && game.Player.GameTime == originalTime &&
                game.State.AreaEffects.All.SequenceEqual(originalAreas) && game.State.Units.All.SequenceEqual(originalUnits) &&
                game.Player.Party.SequenceEqual(originalParty) && owner.Descriptor.Progression.Features.Enumerable.SequenceEqual(originalFeatures) &&
                owner.Abilities.Enumerable.SequenceEqual(originalAbilities) && originalOthers.All(value =>
                    value.Unit.Buffs.Enumerable.SequenceEqual(value.Buffs) && value.Unit.Damage == value.Damage && value.Unit.Position == value.Position);
            Func<bool> retained = () => ownsSheet() && worldRetained() && added.All(value =>
                !value.Active && !value.IsTurnedOn && ReferenceEquals(value.Owner.Unit, owner) && owner.Buffs.Enumerable.Contains(value)) &&
                owner.Buffs.Enumerable.SequenceEqual(originalBuffs.Concat(added));
            Application.LogCallback observe = (message, stack, kind) => {
                if (kind == LogType.Exception || kind == LogType.Error || kind == LogType.Assert) exceptions.Add(kind + ": " + message + "\n" + stack);
            };
            Application.logMessageReceived += observe;
            try
            {
                // Native FactCollection.AddFact skips activation when its existing
                // ActiveByDefault is false. Never change that lifecycle flag or
                // activate these effects; native SetBuff supplies its own dimming.
                foreach (var blueprint in blueprints.Concat(new[] { control }))
                {
                    if (!retained()) throw new InvalidOperationException("Dormant buff fixture changed before native addition.");
                    var buff = owner.Buffs.AddBuff(blueprint, owner, TimeSpan.FromMinutes(5), null);
                    if (buff == null) throw new InvalidOperationException("Native buff addition returned no owned fact: " + blueprint.AssetGuid);
                    added.Add(buff);
                    if (!retained()) throw new InvalidOperationException("Native buff addition activated an effect or changed the world.");
                }
                // Refresh only invalidates scores/attack/defense. Invalidate this
                // real section too so UpdateData cannot reuse its empty list for
                // the same remote owner after our request-local additions.
                sheet.BuffsAndConditions.SetDirty();
                sheet.Refresh();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (!retained() || !sheet.BuffsAndConditions.IsShowed || !sheet.BuffsAndConditions.gameObject.activeInHierarchy)
                    throw new InvalidOperationException("Native buffs-and-conditions section did not open for the exact owner.");
                Func<CharSComponentBuffSlot[]> visible = () => sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true)
                    .Where(value => value.gameObject.activeInHierarchy && value.Buff != null).ToArray();
                evidence["nativeRowsBeforeCapture"] = new JArray(visible().Select(value => new JObject {
                    ["guid"] = value.Buff.Blueprint.AssetGuid, ["ownerId"] = value.Buff.Owner.Unit.UniqueId,
                    ["name"] = value.Name.GetParsedText(), ["expectedName"] = value.Buff.Name,
                    ["sprite"] = value.Icon.sprite?.name, ["alpha"] = value.CanvasGroup.alpha }));
                Write();
                if (added.Any(buff => visible().Count(value => ReferenceEquals(value.Buff, buff)) != 1))
                    throw new InvalidOperationException("Native buff section did not bind every exact owned fact: " + evidence["nativeRowsBeforeCapture"]);
                var controlRow = visible().Single(value => ReferenceEquals(value.Buff, added.Last()));
                Func<bool> controlRetained = () => controlRow.gameObject.activeInHierarchy && ReferenceEquals(controlRow.Buff, added.Last()) &&
                    controlRow.Icon.isActiveAndEnabled && ReferenceEquals(controlRow.Icon.sprite, control.Icon) &&
                    controlRow.Name.text == controlRow.Buff.Name &&
                    string.Equals(controlRow.Name.GetParsedText(), controlRow.Buff.Name, StringComparison.OrdinalIgnoreCase) && !controlRow.Buff.Active;
                foreach (var buff in added.Take(blueprints.Length))
                {
                    var row = visible().Single(value => ReferenceEquals(value.Buff, buff));
                    float nativeAlpha = row.CanvasGroup.alpha;
                    string timer = row.EventSting.text;
                    var eventIcon = row.EventIcon.sprite;
                    Func<bool> context = () => retained() && controlRetained() && ReferenceEquals(row.Buff, buff) &&
                        row.CanvasGroup.alpha == nativeAlpha && row.EventSting.text == timer && ReferenceEquals(row.EventIcon.sprite, eventIcon);
                    Func<JObject> describe = () => new JObject { ["surface"] = "native-racial-buff-sheet",
                        ["race"] = (string)_request.Parameters["race"], ["targetRow"] = new JObject {
                            ["name"] = buff.Name, ["buffGuid"] = buff.Blueprint.AssetGuid, ["ownerId"] = owner.UniqueId,
                            ["expectedSprite"] = buff.Blueprint.Icon.name, ["renderedSprite"] = row.Icon.sprite?.name,
                            ["spriteExact"] = row.Icon.isActiveAndEnabled && ReferenceEquals(row.Icon.sprite, buff.Blueprint.Icon),
                            ["labelExact"] = row.Name.isActiveAndEnabled && row.Name.text == buff.Name &&
                                string.Equals(row.Name.GetParsedText(), buff.Name, StringComparison.OrdinalIgnoreCase),
                            ["nativeSectionShown"] = sheet.BuffsAndConditions.IsShowed,
                            ["labelTruncated"] = row.Name.isTextTruncated, ["labelOverflowing"] = row.Name.isTextOverflowing,
                            ["labelGeometry"] = NativeBuffLabelGeometry(row, visible()),
                            ["buffActive"] = buff.Active, ["buffTurnedOn"] = buff.IsTurnedOn,
                            ["ownerOutsideWorld"] = !owner.IsInGame,
                            ["ownerTurnedOnRetained"] = owner.Descriptor.IsTurnedOn == originalOwnerTurnedOn,
                            ["buffCollectionInactive"] = !(bool)activeByDefault.GetValue(owner.Buffs, null),
                            ["nativeAlpha"] = row.CanvasGroup.alpha, ["nativeStateRetained"] = context(),
                            ["nativeControlGuid"] = control.AssetGuid, ["nativeControlExact"] = controlRetained(),
                            ["fixtureBuffCount"] = added.Count, ["worldAndOriginalFactsRetained"] = worldRetained() } };
                    foreach (int frame in _nativeIconScreens.CaptureRow("native-racial-buff-row:" + buff.Blueprint.AssetGuid,
                        (RectTransform)row.transform, describe, context, observeOnly: true)) yield return frame;
                    var target = describe()["targetRow"];
                    if (!(bool)target["spriteExact"] || !(bool)target["labelExact"] || (bool)target["labelTruncated"] ||
                        !(bool)target["labelGeometry"]["complete"] || nativeAlpha <= 0 || !context())
                        throw new InvalidOperationException("Actual native buff row differs: " + buff.Blueprint.AssetGuid + "; geometry=" + target["labelGeometry"]);
                    ((JArray)evidence["capturedBuffGuids"]).Add(buff.Blueprint.AssetGuid);
                }
            }
            finally
            {
                foreach (var buff in added.AsEnumerable().Reverse())
                    try { if (owner.Buffs.Enumerable.Contains(buff)) buff.Remove(); }
                    catch (Exception error) { exceptions.Add("Owned buff cleanup: " + error); }
                try { sheet.BuffsAndConditions.SetDirty(); sheet.Refresh(); }
                catch (Exception error) { exceptions.Add("Native sheet refresh: " + error); }
                // Native FillData hides surplus pooled rows but retains their
                // Buff references. Clear only rows holding our exact removed facts.
                foreach (var row in sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true))
                    try { if (added.Contains(row.Buff)) { row.Clear(); row.Buff = null; row.Hide(true); } }
                    catch (Exception error) { exceptions.Add("Owned pooled row cleanup: " + error); }
                Application.logMessageReceived -= observe;
                evidence["exceptions"] = new JArray(exceptions);
                evidence["originalBuffsRestored"] = owner.Buffs.Enumerable.SequenceEqual(originalBuffs);
                evidence["worldAndOriginalFactsRetained"] = worldRetained();
                evidence["pooledRowsReleased"] = !sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true).Any(value => added.Contains(value.Buff));
                evidence["restored"] = (bool)evidence["originalBuffsRestored"] && (bool)evidence["worldAndOriginalFactsRetained"] &&
                    (bool)evidence["pooledRowsReleased"] && exceptions.Count == 0;
                Write();
                if (!(bool)evidence["restored"]) throw new InvalidOperationException("Native buff sheet fixture failed exact restoration: " + evidence);
            }
            for (int frame = 0; frame < 12; frame++) yield return 0;
            if (!worldRetained() || !owner.Buffs.Enumerable.SequenceEqual(originalBuffs))
                throw new InvalidOperationException("Native buff cleanup changed during deferred UI refresh.");
        }

        private static JObject NativeBuffLabelGeometry(CharSComponentBuffSlot row, CharSComponentBuffSlot[] visibleRows)
        {
            var label = row.Name;
            var parent = (RectTransform)row.transform;
            var info = label.textInfo;
            var characters = info == null || info.characterInfo == null ? new TMP_CharacterInfo[0] :
                info.characterInfo.Take(info.characterCount).ToArray();
            string generated = new string(characters.Select(value => value.character).ToArray());
            var required = characters.Where(value => !char.IsWhiteSpace(value.character)).ToArray();
            bool complete = required.Length > 0 && info.meshInfo != null && generated == label.GetParsedText() &&
                string.Equals(generated, row.Buff.Name, StringComparison.OrdinalIgnoreCase) &&
                required.All(value => value.isVisible && value.materialReferenceIndex >= 0 &&
                    value.materialReferenceIndex < info.meshInfo.Length && value.vertexIndex >= 0 &&
                    value.vertexIndex + 4 <= info.meshInfo[value.materialReferenceIndex].vertexCount &&
                    info.meshInfo[value.materialReferenceIndex].vertices != null &&
                    value.vertexIndex + 4 <= info.meshInfo[value.materialReferenceIndex].vertices.Length);
            // Read the final native mesh. No text assignment, forced mesh update,
            // font sizing, mask change or layout mutation is permitted here.
            var points = new List<Vector3>();
            if (complete)
                foreach (var character in required)
                {
                    var mesh = info.meshInfo[character.materialReferenceIndex];
                    for (int vertex = 0; vertex < 4; vertex++)
                        points.Add(parent.InverseTransformPoint(label.transform.TransformPoint(
                            mesh.vertices[character.vertexIndex + vertex])));
                }
            bool finite = points.Count > 0 && points.All(value =>
                !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y));
            var bounds = new Bounds(points.Count == 0 ? Vector3.zero : points[0], Vector3.zero);
            foreach (var point in points.Skip(1)) bounds.Encapsulate(point);
            var rowBounds = new Bounds(parent.rect.center, parent.rect.size);
            var labelBounds = NativeBuffRectBounds(parent, (RectTransform)label.transform);
            var iconBounds = NativeBuffRectBounds(parent, (RectTransform)row.Icon.transform);
            var timerBounds = NativeBuffRectBounds(parent, (RectTransform)row.EventSting.transform);
            var timerIconBounds = NativeBuffRectBounds(parent, (RectTransform)row.EventIcon.transform);
            var masks = label.GetComponentsInParent<Mask>(true).Where(value => value.IsActive())
                .Select(value => (RectTransform)value.transform)
                .Concat(label.GetComponentsInParent<RectMask2D>(true).Where(value => value.IsActive())
                    .Select(value => (RectTransform)value.transform)).Distinct().ToArray();
            bool masksContain = masks.All(value => NativeBuffBoundsContain(NativeBuffRectBounds(parent, value), bounds));
            bool inRow = finite && NativeBuffBoundsContain(rowBounds, bounds);
            var neighbors = visibleRows.Where(value => !ReferenceEquals(value, row))
                .Select(value => NativeBuffRectBounds(parent, (RectTransform)value.transform)).ToArray();
            bool clearOfNeighbors = finite && neighbors.All(value => !NativeBuffBoundsOverlap(bounds, value));
            bool separate = finite && !NativeBuffBoundsOverlap(bounds, iconBounds) &&
                !NativeBuffBoundsOverlap(bounds, timerBounds) && !NativeBuffBoundsOverlap(bounds, timerIconBounds);
            // Overflow mode deliberately draws outside the label's nominal rect.
            // Neither the label nor its unmasked layout row clips that mesh.
            // Require every final glyph quad inside actual clipping masks and
            // clear of all neighboring rows and this row's icon/timer. Retain
            // nominal containment as diagnostics, without inventing a tolerance
            // or modifying the native font, mesh, layout or visibility.
            bool usable = complete && finite && bounds.size.x > 0 && bounds.size.y > 0 &&
                masks.Length > 0 && masksContain && clearOfNeighbors && separate && label.overflowMode == TextOverflowModes.Overflow &&
                !label.isTextTruncated && !label.canvasRenderer.cull;
            return new JObject { ["api"] = "native TMP_TextInfo final mesh vertices; read-only",
                ["complete"] = usable, ["generatedText"] = generated, ["parsedText"] = label.GetParsedText(),
                ["characterCount"] = characters.Length,
                ["requiredGlyphCount"] = required.Length, ["allGlyphsGenerated"] = complete,
                ["finiteGeometry"] = finite, ["glyphsWithinRow"] = inRow,
                ["glyphsWithinNameRect"] = finite && NativeBuffBoundsContain(labelBounds, bounds),
                ["glyphsWithinClippingMasks"] = masksContain, ["clipMaskCount"] = masks.Length,
                ["clippingMaskBounds"] = new JArray(masks.Select(value => NativeBuffBoundsEvidence(NativeBuffRectBounds(parent, value)))),
                ["glyphsClearOfOtherRows"] = clearOfNeighbors, ["otherRowCount"] = neighbors.Length,
                ["otherRowBounds"] = new JArray(neighbors.Select(NativeBuffBoundsEvidence)),
                ["glyphsClearOfIconAndTimer"] = separate, ["canvasRendererCulled"] = label.canvasRenderer.cull,
                ["overflowMode"] = label.overflowMode.ToString(), ["overflowReported"] = label.isTextOverflowing,
                ["fontSize"] = label.fontSize, ["fontSizeMin"] = label.fontSizeMin,
                ["fontSizeMax"] = label.fontSizeMax, ["autoSizing"] = label.enableAutoSizing,
                ["glyphBounds"] = NativeBuffBoundsEvidence(bounds), ["rowBounds"] = NativeBuffBoundsEvidence(rowBounds),
                ["nameBounds"] = NativeBuffBoundsEvidence(labelBounds), ["iconBounds"] = NativeBuffBoundsEvidence(iconBounds),
                ["timerBounds"] = NativeBuffBoundsEvidence(timerBounds), ["timerIconBounds"] = NativeBuffBoundsEvidence(timerIconBounds) };
        }

        private static Bounds NativeBuffRectBounds(RectTransform parent, RectTransform rectangle)
        {
            var corners = new Vector3[4]; rectangle.GetWorldCorners(corners);
            var bounds = new Bounds(parent.InverseTransformPoint(corners[0]), Vector3.zero);
            foreach (var corner in corners.Skip(1)) bounds.Encapsulate(parent.InverseTransformPoint(corner));
            return bounds;
        }

        private static bool NativeBuffBoundsContain(Bounds outer, Bounds inner)
        {
            const float tolerance = 0.01f;
            return inner.min.x >= outer.min.x - tolerance && inner.max.x <= outer.max.x + tolerance &&
                inner.min.y >= outer.min.y - tolerance && inner.max.y <= outer.max.y + tolerance;
        }

        private static bool NativeBuffBoundsOverlap(Bounds left, Bounds right)
        {
            const float tolerance = 0.01f;
            return left.min.x < right.max.x - tolerance && left.max.x > right.min.x + tolerance &&
                left.min.y < right.max.y - tolerance && left.max.y > right.min.y + tolerance;
        }

        private static JObject NativeBuffBoundsEvidence(Bounds value)
        {
            return new JObject { ["minX"] = value.min.x, ["maxX"] = value.max.x,
                ["minY"] = value.min.y, ["maxY"] = value.max.y };
        }

        private void AppendNativeRacialBuffAssertion()
        {
            if (!NativeRacialBuffCase) return;
            var entries = _characters.OfType<JObject>().Select(value => value["nativeRacialBuffSheet"] as JObject).Where(value => value != null).ToArray();
            bool passed = entries.Length == 1 && (bool?)entries[0]["restored"] == true &&
                ((JArray)entries[0]["expectedBuffGuids"]).Count == NativeRacialBuffSymbols().Length &&
                JToken.DeepEquals(entries[0]["expectedBuffGuids"], entries[0]["capturedBuffGuids"]);
            Result.Assertions.Add(new RuntimeTestAssertion { Name = "actual-native-racial-buff-sheet",
                Expected = "exact native dormant buff rows, ordinary control and full original fact/world restoration",
                Observed = new JArray(entries).ToString(Newtonsoft.Json.Formatting.None),
                Status = passed ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, Evidence = EvidenceFileName });
        }
    }
}
