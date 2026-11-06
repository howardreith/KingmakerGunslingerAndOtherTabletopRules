using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Visual closeout: one save-free indoor observation of a mixed summoned
        // group. Native summons, native selection (SelectionManager.MultiSelect)
        // and the native player group-move route (ClickGroundHandler
        // .MoveSelectedUnitsToPoint) in the existing static interior fixture.
        // Read-only otherwise: no material, scale, camera-effect or setting
        // override. Usability outcomes are recorded as observed, not forced.
        private static readonly string[] SummonVisualCloseoutKeys =
        {
            "nalfeshnee", "glabrezu", "vrock", "hezrou", "bone-devil", "kyton-chain-devil",
            "lillend-azata", ShadowOutsiderVisualPolicy.Stalker, ShadowOutsiderVisualPolicy.Demon, "satyr"
        };

        private IEnumerable<int> ReviewSummonVisualCloseout(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || !_context.FeatureModules.Active.ExpandedSummoning)
                throw new InvalidOperationException("Exact fresh closeout fixture with the summoning module enabled required.");
            var selection = SelectionManager.Instance;
            if (selection == null) throw new InvalidOperationException("Native SelectionManager missing.");
            var selectionBefore = selection.SelectedUnits.ToList();
            var owned = new List<UnitEntityData>();
            var keyOf = new Dictionary<UnitEntityData, string>();
            var rows = new JArray();
            var record = new JObject { ["phase"] = phase, ["bodies"] = rows };
            Vector3 centre = fixture.Caster.Position;
            try
            {
                for (int index = 0; index < SummonVisualCloseoutKeys.Length; index++)
                {
                    string key = SummonVisualCloseoutKeys[index];
                    var owner = CastExpandedSummoningOwnTier(fixture, key);
                    owned.Add(owner); keyOf[owner] = key;
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    float angle = index * (360f / SummonVisualCloseoutKeys.Length);
                    Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 4.5f;
                    PlaceExpandedSummoningUnit(owner, ObstacleAnalyzer.GetNearestNode(centre + offset).clampedPosition);
                }
                bool oldWaiting = _sprint25WaitingForUnityAppearance;
                DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                Func<UnitEntityData, bool> ready = owner => owner.View != null && EntityFadedIn(owner) &&
                    owner.Descriptor.State.CanAct && ShadowBodyVisible33(owner, keyOf[owner]);
                try
                {
                    while (DateTime.UtcNow < deadline && !owned.All(ready))
                    {
                        _sprint25WaitingForUnityAppearance = true;
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                }
                finally { _sprint25WaitingForUnityAppearance = oldWaiting; }
                bool appeared = owned.All(ready);
                Sprint25Check("closeout-group-appeared", appeared, new JObject {
                    ["notReady"] = new JArray(owned.Where(value => !ready(value)).Select(value => keyOf[value])) },
                    "All ten native summons appear in the static interior with their settled original presentation.");
                if (!appeared) throw new InvalidOperationException("Mixed group did not settle.");
                foreach (int frame in SummonOverhaulSettlePartyCamera(fixture.Caster)) yield return frame;
                var group = new List<UnitEntityData>(owned) { fixture.Caster };
                foreach (var owner in owned)
                {
                    rows.Add(new JObject { ["key"] = keyOf[owner], ["unit"] = owner.UniqueId,
                        ["position"] = SurveyVector(owner.Position), ["distanceToReference"] = owner.DistanceTo(fixture.Caster),
                        ["invisible"] = owner.Descriptor.State.HasCondition(UnitCondition.Invisible),
                        ["directlyControllable"] = owner.IsDirectlyControllable,
                        ["selection"] = Sprint22SelectionSurface(owner), ["pick"] = SummonVisualCloseoutPick(owner, group),
                        ["rootScale"] = SurveyVector(owner.View.transform.localScale) });
                }
                record["referenceBlueprint"] = fixture.Caster.Blueprint.name;
                record["groupImage"] = WriteExpandedSummoningPartyCameraCapture(fixture.Caster,
                    _request.EvidenceDirectory, "closeout-mixed-group.png");
                foreach (var owner in owned)
                {
                    foreach (int frame in SummonOverhaulSettlePartyCamera(owner)) yield return frame;
                    ((JObject)rows.First(row => (string)row["unit"] == owner.UniqueId))["interiorImage"] =
                        WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                            "closeout-" + keyOf[owner] + "-interior.png");
                }
                selection.MultiSelect(group.Select(value => value.View), false);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var selected = selection.SelectedUnits.ToList();
                foreach (int frame in SummonOverhaulSettlePartyCamera(fixture.Caster)) yield return frame;
                record["selectedGroupImage"] = WriteExpandedSummoningPartyCameraCapture(fixture.Caster,
                    _request.EvidenceDirectory, "closeout-mixed-group-selected.png");
                var stalker = owned.Single(value => keyOf[value] == ShadowOutsiderVisualPolicy.Stalker);
                foreach (int frame in SummonOverhaulSettlePartyCamera(stalker)) yield return frame;
                record["stalkerSelected"] = new JObject {
                    ["selected"] = selected.Contains(stalker),
                    ["invisible"] = stalker.Descriptor.State.HasCondition(UnitCondition.Invisible),
                    ["naturalInvisibilityActive"] = stalker.Buffs.Enumerable.Any(value =>
                        value.Blueprint.AssetGuid == "94b2838e8a492c44ebf89e7fe7a75a62" && value.IsTurnedOn),
                    ["image"] = WriteExpandedSummoningPartyCameraCapture(stalker, _request.EvidenceDirectory,
                        "closeout-invisible-stalker-selected.png") };
                bool allSelected = group.All(selected.Contains) && rows.All(row => (bool)row["selection"]["selectionSurface"]);
                Sprint25Check("closeout-mixed-group-selection", allSelected, new JObject {
                    ["selected"] = new JArray(group.Where(selected.Contains).Select(value => keyOf.ContainsKey(value) ? keyOf[value] : "reference")),
                    ["notSelected"] = new JArray(group.Where(value => !selected.Contains(value)).Select(value => keyOf.ContainsKey(value) ? keyOf[value] : "reference")) },
                    "Native MultiSelect selects the reference and all ten summons; each keeps a native selection surface.");
                if (!allSelected) throw new InvalidOperationException("Mixed group selection incomplete.");

                Vector3 target = Vector3.zero, direction = Vector3.zero; bool found = false;
                for (int step = 0; step < 8 && !found; step++)
                {
                    direction = Quaternion.Euler(0f, step * 45f, 0f) * Vector3.forward;
                    Vector3 candidate = centre + direction * 9f;
                    Vector3 node = ObstacleAnalyzer.GetNearestNode(candidate).clampedPosition;
                    Vector2 horizontal = new Vector2(node.x - candidate.x, node.z - candidate.z);
                    if (horizontal.magnitude < 1f && Math.Abs(node.y - centre.y) < 1.5f) { target = node; found = true; }
                }
                Sprint25Check("closeout-interior-target", found, new JObject { ["centre"] = SurveyVector(centre),
                    ["target"] = found ? SurveyVector(target) : null },
                    "A walkable interior point nine metres from the group exists on the native nav graph.");
                if (!found) throw new InvalidOperationException("No interior group-move target.");
                var start = group.ToDictionary(value => value, value => value.Position);
                var peak = group.ToDictionary(value => value, value => 0f);
                ClickGroundHandler.MoveSelectedUnitsToPoint(target, direction, false, false);
                var commands = group.ToDictionary(value => value, value => value.Commands.Move as UnitMoveTo);
                bool movingCaptured = false; int quiet = 0;
                DateTime moveStart = DateTime.UtcNow;
                deadline = moveStart.AddSeconds(25);
                while (DateTime.UtcNow < deadline && quiet < 15)
                {
                    Game.Instance.IsPaused = false; yield return 0;
                    foreach (var unit in group)
                        peak[unit] = Math.Max(peak[unit], unit.View.MovementAgent.Velocity.magnitude);
                    if (!movingCaptured && (DateTime.UtcNow - moveStart).TotalSeconds > 2.5)
                    {
                        record["groupMovingImage"] = WriteExpandedSummoningPartyCameraCapture(fixture.Caster,
                            _request.EvidenceDirectory, "closeout-group-moving.png");
                        movingCaptured = true;
                    }
                    bool still = group.All(unit => unit.Commands.Move == null &&
                        unit.View.MovementAgent.Velocity.sqrMagnitude < .0001f);
                    quiet = still ? quiet + 1 : 0;
                }
                foreach (int frame in SummonOverhaulSettlePartyCamera(fixture.Caster)) yield return frame;
                record["groupArrivedImage"] = WriteExpandedSummoningPartyCameraCapture(fixture.Caster,
                    _request.EvidenceDirectory, "closeout-group-arrived.png");
                var moves = new JArray();
                foreach (var unit in group)
                {
                    var command = commands[unit];
                    Vector3? goal = command == null ? (Vector3?)null : command.Target;
                    moves.Add(new JObject { ["key"] = keyOf.ContainsKey(unit) ? keyOf[unit] : "reference",
                        ["issued"] = command != null, ["result"] = command?.Result.ToString(),
                        ["start"] = SurveyVector(start[unit]), ["end"] = SurveyVector(unit.Position),
                        ["commandTarget"] = goal.HasValue ? SurveyVector(goal.Value) : null,
                        ["distanceToCommandTarget"] = goal.HasValue ? Vector3.Distance(unit.Position, goal.Value) : (float?)null,
                        ["travel"] = Vector3.Distance(start[unit], unit.Position), ["peakVelocity"] = peak[unit] });
                }
                record["groupMove"] = new JObject { ["route"] = "ClickGroundHandler.MoveSelectedUnitsToPoint (native player right-click route)",
                    ["target"] = SurveyVector(target), ["seconds"] = (DateTime.UtcNow - moveStart).TotalSeconds,
                    ["settled"] = quiet >= 15, ["units"] = moves };
                bool usable = quiet >= 15 && moves.All(row => (bool)row["issued"] && (float)row["travel"] > 1f &&
                    (float)row["distanceToCommandTarget"] < 2.5f);
                Sprint25Check("closeout-interior-group-move", usable, (JObject)record["groupMove"].DeepClone(),
                    "Every selected unit receives the native group move, travels, and settles within 2.5 m of its own native destination inside the interior.");
            }
            finally
            {
                foreach (var owner in owned.Where(value => !value.Destroyed))
                {
                    owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    if (owner.IsInCombat) owner.LeaveCombat();
                }
                fixture.Caster.Commands.InterruptAll(command => true); fixture.Caster.Commands.RemoveFinishedAndUpdateQueue();
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                selection.MultiSelect(selectionBefore.Where(value => value != null && !value.Destroyed && value.View != null)
                    .Select(value => value.View), false);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"),
                    record.ToString(Formatting.Indented));
            }
        }

        // Read-only native physics: the first unit collider hit by a camera ray
        // through this unit's soft collider centre. Records whether a neighbour
        // covers it from the party camera; not a synthetic click.
        private static JObject SummonVisualCloseoutPick(UnitEntityData owner, List<UnitEntityData> group)
        {
            var rig = TeleportationCastingCamera();
            var camera = rig == null ? Camera.main : rig.Camera;
            var collider = ReadExpandedSummoningOptionalMember(owner.View, "m_SoftCollider") as Collider;
            if (camera == null || collider == null) return new JObject { ["available"] = false };
            var ray = new Ray(camera.transform.position, collider.bounds.center - camera.transform.position);
            var first = Physics.RaycastAll(ray, 500f).OrderBy(hit => hit.distance)
                .Select(hit => hit.collider.GetComponentInParent<UnitEntityView>())
                .FirstOrDefault(view => view != null && group.Any(unit => ReferenceEquals(unit.View, view)));
            return new JObject { ["available"] = true, ["pickedSelf"] = first != null && ReferenceEquals(first, owner.View),
                ["firstHitUnit"] = first?.EntityData?.UniqueId };
        }
    }
}
