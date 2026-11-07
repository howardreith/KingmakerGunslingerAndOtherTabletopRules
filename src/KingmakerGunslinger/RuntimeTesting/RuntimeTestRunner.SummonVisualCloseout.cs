using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
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
        // Visual closeout: one save-free observation of a mixed summoned group
        // beside the ordinary humanoid reference in the static interior
        // fixture: per-creature dim-interior close-ups, read-only camera-ray
        // picks (can each body be hovered in the crowd) and a confined group
        // move by ordinary native UnitMoveTo commands to one shared point.
        // Summons are natively AI-driven: the summoning spells' spawn action
        // sets IsDirectlyControllable=false, so the player never selects them.
        // That is recorded per unit, not patched. No material, scale,
        // camera-effect or setting override; usability is observed, not forced.
        private static readonly string[] SummonVisualCloseoutKeys =
        {
            "nalfeshnee", "glabrezu", "vrock", "hezrou", "bone-devil", "kyton-chain-devil",
            "lillend-azata", ShadowOutsiderVisualPolicy.Stalker, ShadowOutsiderVisualPolicy.Demon, "satyr"
        };

        private IEnumerable<int> ReviewSummonVisualCloseout(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39VisualCloseoutPhase)
                throw new InvalidOperationException("Exact fresh closeout fixture with the summoning module enabled required.");
            const string prefix = "closeout-";
            var owned = new List<UnitEntityData>();
            var keyOf = new Dictionary<UnitEntityData, string>();
            var rows = new JArray();
            var record = new JObject { ["phase"] = phase, ["capital"] = Game.Instance.CurrentlyLoadedArea?.IsCapital, ["bodies"] = rows };
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
                Sprint25Check(prefix + "group-appeared", appeared, new JObject {
                    ["notReady"] = new JArray(owned.Where(value => !ready(value)).Select(value => keyOf[value])) },
                    "All ten native summons appear with their settled original presentation.");
                if (!appeared) throw new InvalidOperationException("Mixed group did not settle.");
                foreach (int frame in SummonOverhaulSettlePartyCamera(fixture.Caster)) yield return frame;
                var group = new List<UnitEntityData>(owned) { fixture.Caster };
                Func<UnitEntityData, string> name = unit => keyOf.ContainsKey(unit) ? keyOf[unit] : "reference";
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
                record["referenceDirectlyControllable"] = fixture.Caster.IsDirectlyControllable;
                record["groupImage"] = WriteExpandedSummoningPartyCameraCapture(fixture.Caster,
                    _request.EvidenceDirectory, prefix + "mixed-group.png");
                foreach (var owner in owned)
                {
                    foreach (int frame in SummonOverhaulSettlePartyCamera(owner)) yield return frame;
                    ((JObject)rows.First(row => (string)row["unit"] == owner.UniqueId))["interiorImage"] =
                        WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                            prefix + keyOf[owner] + "-interior.png");
                }

                Vector3 target = Vector3.zero, direction = Vector3.zero; bool found = false;
                for (int step = 0; step < 8 && !found; step++)
                {
                    direction = Quaternion.Euler(0f, step * 45f, 0f) * Vector3.forward;
                    Vector3 candidate = centre + direction * 9f;
                    Vector3 node = ObstacleAnalyzer.GetNearestNode(candidate).clampedPosition;
                    Vector2 horizontal = new Vector2(node.x - candidate.x, node.z - candidate.z);
                    if (horizontal.magnitude < 1f && Math.Abs(node.y - centre.y) < 1.5f) { target = node; found = true; }
                }
                Sprint25Check(prefix + "move-target", found, new JObject { ["centre"] = SurveyVector(centre),
                    ["target"] = found ? SurveyVector(target) : null },
                    "A walkable point nine metres from the group exists on the native nav graph.");
                if (!found) throw new InvalidOperationException("No group-move target.");
                var start = group.ToDictionary(value => value, value => value.Position);
                var peak = group.ToDictionary(value => value, value => 0f);
                // One shared point for every body: the crowding worst case.
                const string route = "UnitMoveTo(shared interior point, 0.3) per unit (summons are natively AI-driven, not player-selectable)";
                foreach (var unit in group)
                {
                    var move = new UnitMoveTo(target, .3f); move.Init(unit);
                    if (move.CanStart) unit.Commands.Run(move);
                }
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
                            _request.EvidenceDirectory, prefix + "group-moving.png");
                        movingCaptured = true;
                    }
                    bool still = group.All(unit => unit.Commands.Move == null &&
                        unit.View.MovementAgent.Velocity.sqrMagnitude < .0001f);
                    quiet = still ? quiet + 1 : 0;
                }
                foreach (int frame in SummonOverhaulSettlePartyCamera(fixture.Caster)) yield return frame;
                record["groupArrivedImage"] = WriteExpandedSummoningPartyCameraCapture(fixture.Caster,
                    _request.EvidenceDirectory, prefix + "group-arrived.png");
                var moves = new JArray();
                foreach (var unit in group)
                {
                    var command = commands[unit];
                    Vector3? goal = command == null ? (Vector3?)null : command.Target;
                    moves.Add(new JObject { ["key"] = name(unit),
                        ["issued"] = command != null, ["result"] = command?.Result.ToString(),
                        ["start"] = SurveyVector(start[unit]), ["end"] = SurveyVector(unit.Position),
                        ["commandTarget"] = goal.HasValue ? SurveyVector(goal.Value) : null,
                        ["distanceToCommandTarget"] = goal.HasValue ? Vector3.Distance(unit.Position, goal.Value) : (float?)null,
                        ["distanceToClickTarget"] = Vector3.Distance(unit.Position, target),
                        ["travel"] = Vector3.Distance(start[unit], unit.Position), ["peakVelocity"] = peak[unit] });
                }
                record["groupMove"] = new JObject { ["route"] = route,
                    ["target"] = SurveyVector(target), ["seconds"] = (DateTime.UtcNow - moveStart).TotalSeconds,
                    ["settled"] = quiet >= 15, ["units"] = moves };
                // Usable: every unit received the move, travelled and the
                // whole group came to rest (no endless pushing) in 25 s.
                bool usable = quiet >= 15 && moves.All(row => (bool)row["issued"] && (float)row["travel"] > 1f);
                Sprint25Check(prefix + "group-move", usable, (JObject)record["groupMove"].DeepClone(),
                    "Every unit receives the group move, travels and the whole group comes to rest within 25 seconds.");
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
