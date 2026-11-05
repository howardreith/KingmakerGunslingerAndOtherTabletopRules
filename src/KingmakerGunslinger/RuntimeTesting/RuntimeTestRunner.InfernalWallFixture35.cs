using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class InfernalWallFixturePlan35
        {
            internal readonly Vector3 Start, Destination, Midpoint, CastingPoint;
            internal readonly JObject Survey;
            internal InfernalWallFixturePlan35(Vector3 start, Vector3 destination,
                Vector3 midpoint, Vector3 castingPoint, JObject survey)
            { Start = start; Destination = destination; Midpoint = midpoint; CastingPoint = castingPoint; Survey = survey; }
            internal JObject Document()
            {
                return new JObject { ["start"] = SurveyVector(Start), ["destination"] = SurveyVector(Destination),
                    ["wallMidpoint"] = SurveyVector(Midpoint), ["castingPoint"] = SurveyVector(CastingPoint),
                    ["nativeSurvey"] = Survey.DeepClone() };
            }
        }

        private static void InfernalWallPrepareActor35(UnitEntityData actor)
        {
            SetExpandedSummoningBrainActive(actor, false);
            actor.Commands.InterruptAll(true);
            actor.Commands.RemoveFinishedAndUpdateQueue();
            actor.View.MovementAgent.Stop();
        }

        // Used by the full Wall phase and its explicit placement preflight.
        // Neither path can bypass the same initial/movement/return checkpoint.
        private IEnumerable<int> InfernalWallFixturePrerequisite35(
            ExpandedSummoningCorrectionFixture fixture, IList<UnitEntityData> owned,
            InfernalWallFixturePlan35 plan, Action<string, bool, JObject, string> check)
        {
            foreach (var actor in owned)
                PlaceExpandedSummoningUnit(actor, plan.CastingPoint);
            foreach (int frame in InfernalWallPlaceAndSettle35(fixture.Caster,
                plan.Start, plan, "initial-path")) yield return frame;
            foreach (int frame in InfernalEarlyWallMove35(fixture, plan.Start,
                plan.Destination, plan.Midpoint, false, "baseline", check, plan, true)) yield return frame;
            check("wall-fixture-prerequisite", true, new JObject {
                ["plan"] = plan.Document(), ["final"] = InfernalWallActorState35(fixture.Caster),
                ["initialAndReturnPlacementVerified"] = true,
                ["baselineNativeMovementVerified"] = true,
                ["wallAbilityLookupReached"] = false, ["wallCastingReached"] = false,
                ["tailReached"] = false,
                ["preflightOnly"] = ExpandedSummoningNativeProfilePolicy.WallPlacementPreflightPhase(
                    RuntimeTestRequestParser.Sprint32ReviewPhase(_request))
            }, "Shared fixture establishes its immutable plan, real baseline movement and return before any Wall ability lookup.");
        }

        private static JObject InfernalWallActorState35(UnitEntityData actor)
        {
            var view = actor.View;
            var agent = view.AgentASP;
            Func<Kingmaker.UnitLogic.Commands.Base.UnitCommand, JObject> command = value => {
                var row = Sprint27AiCommand(value);
                row["reference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
                return row;
            };
            return new JObject {
                ["frame"] = Time.frameCount, ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds,
                ["actor"] = actor.UniqueId, ["blueprint"] = actor.Blueprint.AssetGuid,
                ["area"] = Game.Instance.CurrentlyLoadedArea?.AssetGuid,
                ["scene"] = view.gameObject.scene.name, ["sceneHandle"] = view.gameObject.scene.handle,
                ["entityWorld"] = SurveyVector(actor.Position), ["previousEntityWorld"] = SurveyVector(actor.PreviousPosition),
                ["viewWorld"] = SurveyVector(view.transform.position), ["viewInstance"] = view.GetInstanceID(),
                ["viewTransformInstance"] = view.transform.GetInstanceID(),
                ["agentWorld"] = agent == null ? null : SurveyVector(agent.transform.position),
                ["agentInstance"] = agent == null ? (int?)null : agent.GetInstanceID(),
                ["agentTransformInstance"] = agent == null ? (int?)null : agent.transform.GetInstanceID(),
                ["agentIsExactNativeViewAgent"] = agent != null && agent.Unit == view && ReferenceEquals(view.MovementAgent, agent),
                ["nativeCoordinateContract"] = "Entity/view/agent transform positions are world coordinates. AgentASP is on the view GameObject; no independent agent Position exists. Native Translocate synchronously grounds Y/FlyHeight and preserves XZ.",
                ["commandsEmpty"] = actor.Commands.Empty, ["brainActive"] = actor.IsBrainActive,
                ["activeCommands"] = new JArray(actor.Commands.Raw.Where(value => value != null).Select(command)),
                ["queuedCommands"] = new JArray(actor.Commands.Queue.Where(value => value != null).Select(command)),
                ["velocity"] = SurveyVector(view.MovementAgent.Velocity),
                ["isReallyMoving"] = agent?.IsReallyMoving, ["wantsToMove"] = agent?.WantsToMove,
                ["isStopping"] = agent?.IsStopping, ["pathFailed"] = agent?.PathFailed,
                ["repathNeeded"] = agent?.RepathNeeded,
                ["path"] = agent == null ? null : Cleanup2GroundPath(agent.Path, actor.Position),
                ["requestedPath"] = agent == null ? null : Cleanup2GroundPath(
                    agent.GetType().GetField("m_RequestedPath", Cleanup2Fields)?.GetValue(agent) as Pathfinding.Path, actor.Position)
            };
        }

        private IEnumerable<int> InfernalWallPlaceAndSettle35(UnitEntityData actor,
            Vector3 requested, InfernalWallFixturePlan35 plan, string label)
        {
            var mode = _cleanup2NativeDefaultOwner;
            if (mode == null || !mode.Current) throw new InvalidOperationException("Wall placement requires its exact native Default lease.");
            string path = Path.Combine(_request.EvidenceDirectory,
                "s35-wall-placement-" + label + "-" + actor.UniqueId + ".json");
            var frames = new JArray();
            var row = new JObject { ["label"] = label, ["plan"] = plan.Document(),
                ["requestedPosition"] = SurveyVector(requested),
                ["beforePreparation"] = InfernalWallActorState35(actor), ["nativeFrames"] = frames };
            try
            {
                row["stage"] = "preparing-placement";
                InfernalWallPrepareActor35(actor);
                row["beforePlacement"] = InfernalWallActorState35(actor);
                Vector3 expected; JObject expectedGround;
                if (!Cleanup2CreatureNativeGroundRoot(actor, requested, out expected, out expectedGround))
                    throw new InvalidOperationException("Planned Wall witness has no native floor support.");
                row["expectedNativeGroundPosition"] = SurveyVector(expected);
                row["expectedGround"] = expectedGround;
                // One native placement. No view/agent position write, chasing
                // the settled position, or retrying teleports supplies success.
                PlaceExpandedSummoningUnit(actor, requested);
                row["stage"] = "immediate-native-placement";
                row["immediate"] = InfernalWallActorState35(actor);
                bool immediate = Vector3.Distance(actor.Position, expected) <= .01f &&
                    Vector3.Distance(actor.View.transform.position, expected) <= .01f &&
                    actor.View.AgentASP != null && actor.View.AgentASP.Unit == actor.View &&
                    ReferenceEquals(actor.View.MovementAgent, actor.View.AgentASP) &&
                    Vector3.Distance(actor.View.AgentASP.transform.position, expected) <= .01f;
                row["immediateAgreement"] = immediate;
                RuntimeTestResultWriter.WriteAtomic(path, row.ToString(Formatting.Indented));
                Sprint25Check("wall-placement-" + label + "-immediate", immediate, row,
                    "Native Translocate immediately agrees with the independently queried native ground root.");
                if (!immediate) throw new InvalidOperationException("Immediate native Wall placement disagreement: " + label);

                int quiet = 0, observed = 0, lastFrame = Time.frameCount;
                Vector3 previous = actor.Position;
                JObject divergence = null;
                row["stage"] = "ordinary-native-frames";
                foreach (int frame in WaitPresentation32(mode, () => quiet >= 3 || divergence != null, 8, () => {
                    if (Time.frameCount == lastFrame) return;
                    lastFrame = Time.frameCount; observed++;
                    bool intended = InfernalEarlyDistanceToSegment35(actor.Position, requested, requested) < .05f &&
                        Vector3.Distance(actor.Position, expected) < .02f &&
                        Vector3.Distance(actor.View.transform.position, actor.Position) <= .01f;
                    bool stable = intended && actor.Commands.Empty && actor.View.MovementAgent.Velocity.sqrMagnitude < .0001f &&
                        Vector3.Distance(previous, actor.Position) < .01f && actor.Descriptor.State.CanAct;
                    quiet = stable ? quiet + 1 : 0; previous = actor.Position;
                    row["observedNativeFrames"] = observed; row["quietFrames"] = quiet;
                    JObject state = InfernalWallActorState35(actor);
                    state["atIntendedPosition"] = intended; state["quietFrames"] = quiet;
                    if (observed <= 12) frames.Add(state);
                    if (!intended && divergence == null) { divergence = state; row["firstDivergence"] = state.DeepClone(); }
                })) yield return frame;
                Vector3 supported; JObject ground;
                bool pass = quiet >= 3 && divergence == null &&
                    Cleanup2CreatureNativeGroundRoot(actor, actor.Position, out supported, out ground) &&
                    Vector3.Distance(supported, actor.Position) < .02f &&
                    InfernalEarlyDistanceToSegment35(actor.Position, requested, requested) < .05f;
                // Query again solely to retain the final support receipt even
                // when an earlier conjunct refused; this never places a unit.
                Cleanup2CreatureNativeGroundRoot(actor, actor.Position, out supported, out ground);
                row["final"] = InfernalWallActorState35(actor); row["ground"] = ground;
                row["observedNativeFrames"] = observed; row["quietFrames"] = quiet; row["settledAgreement"] = pass;
                row["stage"] = "settled-observation";
                Sprint25Check("wall-placement-" + label + "-settled", pass, row,
                    "Three consecutive ordinary native frames preserve the original intended position and native support.");
                if (!pass) throw new InvalidOperationException("Wall native placement did not persist: " + label);
            }
            finally
            {
                if (row["settledAgreement"] == null) row["settledAgreement"] = false;
                if (row["final"] == null)
                {
                    try
                    {
                        row["final"] = InfernalWallActorState35(actor);
                        Vector3 supported; JObject ground;
                        row["finalSupportQuerySucceeded"] = Cleanup2CreatureNativeGroundRoot(actor, actor.Position, out supported, out ground);
                        row["ground"] = ground;
                    }
                    catch (Exception error) { row["finalObservationError"] = error.ToString(); }
                }
                RuntimeTestResultWriter.WriteAtomic(path, row.ToString(Formatting.Indented));
            }
        }
    }
}
