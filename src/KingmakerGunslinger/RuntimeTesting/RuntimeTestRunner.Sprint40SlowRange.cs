using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Visual.FogOfWar;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The sole S39 gameplay carry-forward: no nimbus/spell campaign.
        private IEnumerable<int> ReviewNalfeshneeSlowRange40(ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>(); var trace = new InfernalEvents34();
            var cells = new JArray(); var receipt = new JObject {
                ["cells"] = cells, ["events"] = trace.Rows,
                ["scope"] = "Nalfeshnee-owned Slow close-range correction only" };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Nalfeshnee Slow boundary: " + id);
            };
            var recipient = fixture.Hostile;
            try
            {
                var owner = CastExpandedSummoningOwnTier(fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
                owned.Add(owner); VrockQuiesce37(owner);
                foreach (int frame in Outsider30NativeAppearance(owner, owner.Blueprint.name)) yield return frame;
                var spell = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix + "Slow"];
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(spell));
                JObject hostility = Nalfeshnee39EstablishHostility(fixture, owner, recipient);
                check("nalfeshnee-slow-hostility", owner.IsEnemy(recipient) && recipient.IsEnemy(owner) &&
                    !ReferenceEquals(owner.Group, recipient.Group), hostility,
                    "Exact disposable recipient, supported native group setter and mutual hostility.");
                var probe = new UnitUseAbility(data, new TargetWrapper(recipient)); probe.Init(owner);
                float nativeRadius = probe.ApproachRadius;
                float printed = new Feet(NalfeshneeRules39Policy.SlowRangeFeet(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel)).Meters;
                float footprint = owner.Corpulence + recipient.Corpulence;
                check("nalfeshnee-slow-native-range", spell.CustomRange.Value == 55 &&
                    Math.Abs(nativeRadius - printed) <= footprint + .1f,
                    new JObject { ["printedFeet"] = 55, ["nativeApproachRadius"] = nativeRadius,
                        ["ownerCorpulence"] = owner.Corpulence, ["targetCorpulence"] = recipient.Corpulence,
                        ["nativeRadiusMinusPrinted"] = nativeRadius - printed },
                    "Actual command approach radius is the printed CL12 close range with the engine's real footprint allowance, far below220 feet.");
                // Unit HasLOS is an AI sight-range consumer (11.7m here),
                // shorter than this printed cast boundary. Use real geometry
                // and stationary native party vision, then actual CanTarget/
                // UnitUseAbility. Do not enlarge anyone's vision or waive the
                // spell's native eligibility/range checks.
                var survey = new JArray(); receipt["rangeSurvey"] = survey;
                Vector3 origin, outside;
                var constraint = Sprint40WalkingConstraint(fixture.Caster);
                var anchor = AstarPath.active.GetNearest(fixture.Caster.Position, constraint);
                if (anchor.node == null) throw new InvalidOperationException("No native range observer walking origin.");
                constraint.SetStart(anchor.node); constraint.constrainArea = true;
                if (!TryNalfeshneeRangeSpan40(fixture.Caster, nativeRadius + .75f, constraint, survey, out origin, out outside))
                    throw new InvalidOperationException("No bounded native Slow range geometry around the stationary observer.");
                string lane = "Native connected graph/clear geometry; stationary observer at midpoint; actual player-cast consumers follow.";
                Vector3 axis = (outside - origin).normalized;
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(recipient, origin + axis * (nativeRadius - .75f));
                DateTime visibleDeadline = DateTime.UtcNow.AddSeconds(10);
                while ((!owner.IsVisibleForPlayer || !recipient.IsVisibleForPlayer) && DateTime.UtcNow < visibleDeadline)
                { Game.Instance.IsPaused = false; yield return 0; }
                var insideProbe = new UnitUseAbility(data, new TargetWrapper(recipient)); insideProbe.Init(owner);
                bool insideLegal = data.IsAvailable && data.CanTarget(new TargetWrapper(recipient)) &&
                    insideProbe.IsUnitEnoughClose && owner.IsVisibleForPlayer && recipient.IsVisibleForPlayer;
                check("nalfeshnee-slow-inside-targeting", insideLegal,
                    new JObject { ["lane"] = lane, ["centerDistance"] = Vector3.Distance(owner.Position, recipient.Position),
                        ["nativeEnoughClose"] = insideProbe.IsUnitEnoughClose,
                        ["ownerNativeVisionMeters"] = owner.VisionRangeMeters,
                        ["ownerNativeUnitSight"] = owner.HasLOS(recipient),
                        ["recipientVisibleForPlayer"] = recipient.IsVisibleForPlayer,
                        ["nativeApproachRadius"] = insideProbe.ApproachRadius },
                    "Real native targeting accepts the recipient just inside its measured command boundary.");
                int start = trace.Rows.Count;
                Vector3 insideStart = owner.Position;
                var inside = new UnitUseAbility(data, new TargetWrapper(recipient)); owner.Commands.Run(inside);
                foreach (int frame in WaitInfernalCommand34(inside, () => true, 25)) yield return frame;
                var insideActs = trace.Rows.Skip(start).Where(x => (string)x["event"] == "command-act" &&
                    (string)x["owner"] == owner.UniqueId && (string)x["ability"] == spell.AssetGuid).ToArray();
                check("nalfeshnee-slow-inside-cast", inside.Result == UnitCommand.ResultType.Success &&
                    insideActs.Length == 1 && (bool)insideActs[0]["nativeEnoughClose"],
                    new JObject { ["command"] = Sprint27AiCommand(inside), ["acts"] = new JArray(insideActs),
                        ["actualTravel"] = Vector3.Distance(insideStart, owner.Position),
                        ["actualCastCenterDistance"] = Vector3.Distance(owner.Position, recipient.Position) },
                    "A real owned Slow command completes inside the correct close range.");
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(recipient, outside);
                var outsideProbe = new UnitUseAbility(data, new TargetWrapper(recipient)); outsideProbe.Init(owner);
                check("nalfeshnee-slow-outside-needs-approach", !outsideProbe.IsUnitEnoughClose &&
                    data.CanTarget(new TargetWrapper(recipient)),
                    new JObject { ["centerDistance"] = Vector3.Distance(owner.Position, recipient.Position),
                        ["nativeEnoughClose"] = outsideProbe.IsUnitEnoughClose,
                        ["nativeApproachRadius"] = outsideProbe.ApproachRadius },
                    "The native command refuses immediate execution outside close range while retaining legal target identity.");
                Vector3 before = owner.Position; start = trace.Rows.Count;
                var approach = new UnitUseAbility(data, new TargetWrapper(recipient)); owner.Commands.Run(approach);
                foreach (int frame in WaitInfernalCommand34(approach, () => true, 30)) yield return frame;
                var acts = trace.Rows.Skip(start).Where(x => (string)x["event"] == "command-act" &&
                    (string)x["owner"] == owner.UniqueId && (string)x["ability"] == spell.AssetGuid).ToArray();
                check("nalfeshnee-slow-outside-native-approach", approach.Result == UnitCommand.ResultType.Success &&
                    Vector3.Distance(before, owner.Position) > .2f && acts.Length == 1 &&
                    (bool)acts[0]["nativeEnoughClose"],
                    new JObject { ["command"] = Sprint27AiCommand(approach),
                        ["travel"] = Vector3.Distance(before, owner.Position), ["acts"] = new JArray(acts) },
                    "Outside the native boundary, the actual Nalfeshnee walks into range before casting; no party actor is repositioned or range guard waived.");
            }
            finally
            {
                trace.Dispose(); DisposeExpandedSummoningUnits(fixture.Created, owned);
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-nalfeshnee-slow-range.json"), receipt.ToString(Formatting.Indented));
            }
        }

        // The same small span is required before choosing this fixture's
        // position and after observing the real command boundary. No actor's
        // sight or the spell's targeting/range consumer is altered.
        private static bool TryNalfeshneeRangeSpan40(UnitEntityData observer, float span,
            Pathfinding.PathNNConstraint constraint, JArray survey, out Vector3 origin, out Vector3 target)
        {
            origin = Vector3.zero; target = Vector3.zero;
            var anchor = AstarPath.active.GetNearest(observer.Position, constraint);
            if (anchor.node == null) return false;
            var graph = AstarPath.active.graphs[anchor.node.GraphIndex] as Pathfinding.IRaycastableGraph;
            if (graph == null) return false;
            for (int angle = 0; angle < 24; angle++)
            {
                Vector3 axis = Quaternion.Euler(0, angle * 15f, 0) * Vector3.forward;
                Vector3 requestedSource = observer.Position - axis * (span * .5f);
                Vector3 requestedTarget = observer.Position + axis * (span * .5f);
                var from = AstarPath.active.GetNearest(requestedSource, constraint);
                var to = AstarPath.active.GetNearest(requestedTarget, constraint);
                Vector3 sourceProjection = requestedSource - from.clampedPosition; sourceProjection.y = 0;
                Vector3 targetProjection = requestedTarget - to.clampedPosition; targetProjection.y = 0;
                bool connected = from.node != null && to.node != null && from.node.Walkable && to.node.Walkable &&
                    from.node.Area == anchor.node.Area && to.node.Area == anchor.node.Area &&
                    from.node.GraphIndex == anchor.node.GraphIndex && to.node.GraphIndex == anchor.node.GraphIndex &&
                    sourceProjection.magnitude <= .6f && targetProjection.magnitude <= .6f;
                Pathfinding.GraphHitInfo hit;
                bool clear = connected && !graph.Linecast(from.clampedPosition, to.clampedPosition, from.node,
                    out hit, new List<Pathfinding.GraphNode>()) && !LineOfSightGeometry.Instance.HasObstacle(
                        from.clampedPosition + Vector3.up * 1.2f, to.clampedPosition + Vector3.up * 1.2f, 0);
                bool sight = clear && observer.HasLOS(from.clampedPosition) && observer.HasLOS(to.clampedPosition);
                float distance = Vector3.Distance(from.clampedPosition, to.clampedPosition);
                survey.Add(new JObject { ["source"] = Vec(from.clampedPosition), ["target"] = Vec(to.clampedPosition),
                    ["connectedNativeGraph"] = connected, ["clearNativeGeometry"] = clear,
                    ["stationaryObserverNativeSightBoth"] = sight, ["projectedDistance"] = distance,
                    ["sourceWalkable"] = from.node != null && from.node.Walkable,
                    ["targetWalkable"] = to.node != null && to.node.Walkable,
                    ["sourceHorizontalClamp"] = sourceProjection.magnitude, ["targetHorizontalClamp"] = targetProjection.magnitude });
                if (!sight || distance < span - .25f) continue;
                origin = from.clampedPosition; target = to.clampedPosition; return true;
            }
            return false;
        }
    }
}
