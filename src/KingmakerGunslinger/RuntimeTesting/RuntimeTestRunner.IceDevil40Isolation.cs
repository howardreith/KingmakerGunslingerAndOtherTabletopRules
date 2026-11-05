using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewIceDevilScope40(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            using (var isolation = new IceDevilFixtureIsolation40(fixture))
            {
                var receipt = new JObject { ["phase"] = phase };
                var camera = TeleportationCastingCamera();
                Vector3 cameraPosition = camera.transform.position, cameraTarget = camera.GetPosition();
                Quaternion cameraRotation = camera.transform.rotation;
                bool casterLent = false;
                try
                {
                    var placement = new JObject(); receipt["placement"] = placement;
                    PrepareIceDevilQuietCaster40(fixture, placement);
                    // Native party vision and the real camera follow only this
                    // disposable observer caster. Do not force the unit's FOW,
                    // fader, renderer or dissolve state, or move an intake actor.
                    if (Game.Instance.Player.Party.Contains(fixture.Caster))
                        throw new InvalidOperationException("Disposable observer caster unexpectedly already in party.");
                    Game.Instance.Player.Party.Add(fixture.Caster); casterLent = true;
                    camera.ScrollToImmediately(fixture.Caster.Position);
                    placement["nativePartyVisionCaster"] = fixture.Caster.UniqueId;
                    placement["nativeCameraTarget"] = Vec(camera.GetPosition());
                    IEnumerable<int> run;
                    switch (phase)
                    {
                        case ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilEarlyPhase:
                            CreateSprint32TurnHostile(fixture); run = ReviewIceDevilEarly40(fixture); break;
                        case ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilCombatPhase:
                            CreateSprint32TurnHostile(fixture); run = ReviewIceDevilCombat40(fixture); break;
                        case ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilEffectsPhase:
                            CreateSprint32TurnHostile(fixture); run = ReviewIceDevilEffects40(fixture); break;
                        case ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilWallsPhase:
                            CreateSprint32TurnHostile(fixture); run = ReviewIceDevilWalls40(fixture); break;
                        case ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilSlowPhase:
                            CreateSprint32TurnHostile(fixture); run = ReviewIceDevilSlow40(fixture); break;
                        case ExpandedSummoningNativeProfilePolicy.Sprint40NalfeshneeRangePhase:
                            CreateSprint32TurnHostile(fixture); run = ReviewNalfeshneeSlowRange40(fixture); break;
                        case ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilPresentationPhase:
                            CreateSprint32TurnHostile(fixture); run = ReviewInfernalBodies34(fixture, iceDevil: true); break;
                        case ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilAiPhase:
                            run = ReviewIceDevilAi40(fixture); break;
                        case ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilSlowTurnPhase:
                            run = ReviewIceDevilSlowTurn40(fixture); break;
                        case ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilPublicPhase:
                            ReviewIceDevilPublic40(fixture); run = Enumerable.Empty<int>(); break;
                        default: throw new InvalidOperationException("Unknown exact Sprint40 native scope.");
                    }
                    foreach (int frame in run)
                    {
                        isolation.Validate(); yield return frame;
                        isolation.Validate();
                    }
                    isolation.Validate();
                    bool observed = isolation.OwnedEvents > 0;
                    Sprint25Check("sprint40-protected-targets", observed, isolation.Describe(),
                        "A nonempty protected intake and observed owned effects exercise the actual global listener; no owned save/damage/heal/application reaches a protected actor.");
                    if (!observed) throw new InvalidOperationException("Sprint40 isolation listener had no owned events.");
                }
                finally
                {
                    if (casterLent) Game.Instance.Player.Party.Remove(fixture.Caster);
                    camera.ScrollToImmediately(cameraTarget); camera.transform.position = cameraPosition;
                    camera.transform.rotation = cameraRotation;
                    typeof(CameraRig).GetField("m_TargetPosition", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(camera, cameraTarget);
                    receipt["events"] = isolation.Describe();
                    receipt["observerPartyRestored"] = !Game.Instance.Player.Party.Contains(fixture.Caster);
                    RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                        "sprint40-fixture-isolation.json"), receipt.ToString(Formatting.Indented));
                }
            }
        }

        private void PrepareIceDevilQuietCaster40(ExpandedSummoningCorrectionFixture fixture, JObject receipt)
        {
            var protectedUnits = fixture.UnitsBefore.OfType<UnitEntityData>().Where(x =>
                x != null && !x.Destroyed && x.IsInGame && ReferenceEquals(x.HoldingState, fixture.Scene)).ToArray();
            if (protectedUnits.Length == 0) throw new InvalidOperationException("No nonempty protected scene intake.");
            Vector3 origin = fixture.Caster.Position;
            var candidates = new JArray(); receipt["candidates"] = candidates;
            receipt["origin"] = Vec(origin);
            receipt["protectedActors"] = new JArray(protectedUnits.Select(x => new JObject {
                ["unit"] = x.UniqueId, ["position"] = Vec(x.Position), ["corpulence"] = x.Corpulence }));
            int surveyed = 0;
            for (int radius = 64; radius <= 128; radius += 8)
            foreach (Vector3 direction in CompassOffsets)
            {
                surveyed++;
                Vector3 requested = origin + direction.normalized * radius;
                var node = ObstacleAnalyzer.GetNearestNode(requested);
                Vector3 projection = requested - node.clampedPosition; projection.y = 0;
                var candidate = new JObject { ["requested"] = Vec(requested),
                    ["clamped"] = Vec(node.clampedPosition), ["node"] = node.node != null,
                    ["walkable"] = node.node != null && node.node.Walkable,
                    ["horizontalProjectionGap"] = projection.magnitude };
                candidates.Add(candidate);
                if (node.node == null || !node.node.Walkable || projection.magnitude > 1.5f)
                    continue;
                Vector3 point = node.clampedPosition;
                float separation = protectedUnits.Min(x => {
                    Vector3 delta = x.Position - point; delta.y = 0; return delta.magnitude - x.Corpulence;
                });
                candidate["protectedGapMeters"] = separation;
                if (separation < 60f) continue;
                bool localRoute = false;
                foreach (var localDirection in CompassOffsets.Take(4))
                {
                    Vector3 requestedEnd = point + localDirection * 5f;
                    var end = ObstacleAnalyzer.GetNearestNode(requestedEnd);
                    Vector3 endProjection = requestedEnd - end.clampedPosition; endProjection.y = 0;
                    if (end.node != null && end.node.Walkable && endProjection.magnitude <= 1.5f &&
                        Vector3.Distance(ObstacleAnalyzer.TraceAlongNavmesh(point, end.clampedPosition), end.clampedPosition) <= .15f)
                    { localRoute = true; break; }
                }
                candidate["localNativeRoute"] = localRoute;
                if (!localRoute) continue;
                // Only the disposable caster moves. The original party anchor
                // and every other intake actor remain at their exact positions.
                PlaceExpandedSummoningUnit(fixture.Caster, point);
                receipt["caster"] = fixture.Caster.UniqueId; receipt["point"] = Vec(point);
                receipt["nearestProtectedGapMeters"] = separation; receipt["surveyed"] = surveyed;
                receipt["scene"] = "existing Oleg outdoor fixture"; receipt["intakeActorsMoved"] = 0;
                return;
            }
            throw new InvalidOperationException("No bounded native quiet caster position60m from protected intake; no summon created.");
        }

        private sealed class IceDevilFixtureIsolation40 : IDisposable,
            IGlobalRulebookHandler<RuleSavingThrow>, IGlobalRulebookHandler<RuleDealDamage>,
            IGlobalRulebookHandler<RuleHealDamage>, IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly ExpandedSummoningCorrectionFixture fixture;
            private readonly HashSet<UnitEntityData> protectedUnits;
            private readonly HashSet<UnitEntityData> ownedSources = new HashSet<UnitEntityData>();
            private readonly JArray violations = new JArray();
            internal int OwnedEvents { get; private set; }
            internal IceDevilFixtureIsolation40(ExpandedSummoningCorrectionFixture value)
            {
                fixture = value; protectedUnits = new HashSet<UnitEntityData>(value.UnitsBefore.OfType<UnitEntityData>());
                if (protectedUnits.Count == 0) throw new InvalidOperationException("Empty protected intake.");
                EventBus.Subscribe(this);
            }
            private void Observe(string kind, UnitEntityData source, UnitEntityData target)
            {
                bool owned = source != null && (ReferenceEquals(source, fixture.Caster) || fixture.Created.Contains(source) ||
                    ownedSources.Contains(source) || ReferenceEquals(source.Get<UnitPartSummonedMonster>()?.Summoner, fixture.Caster));
                if (!owned) return;
                ownedSources.Add(source);
                OwnedEvents++;
                if (target != null && protectedUnits.Contains(target)) violations.Add(new JObject {
                    ["kind"] = kind, ["source"] = source.UniqueId, ["target"] = target.UniqueId,
                    ["targetBlueprint"] = target.Blueprint?.name });
            }
            internal void Validate()
            {
                if (violations.Count != 0) throw new InvalidOperationException("Owned effect reached protected actor: " + violations);
            }
            internal JObject Describe() { return new JObject { ["protectedActors"] = protectedUnits.Count,
                ["ownedEvents"] = OwnedEvents, ["violations"] = violations }; }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt) { Observe("save", evt.Reason?.Context?.MaybeCaster, evt.Initiator); }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt) { Observe("damage", evt.Initiator, evt.Target); }
            public void OnEventAboutToTrigger(RuleHealDamage evt) { }
            public void OnEventDidTrigger(RuleHealDamage evt) { Observe("heal", evt.Initiator, evt.Target); }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt) { Observe("buff", evt.Context?.MaybeCaster, evt.Initiator); }
            public void Dispose() { EventBus.Unsubscribe(this); }
        }
    }
}
