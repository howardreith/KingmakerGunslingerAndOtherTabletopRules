using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                try
                {
                    receipt["placement"] = PrepareIceDevilQuietCaster40(fixture);
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
                    receipt["events"] = isolation.Describe();
                    RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                        "sprint40-fixture-isolation.json"), receipt.ToString(Formatting.Indented));
                }
            }
        }

        private JObject PrepareIceDevilQuietCaster40(ExpandedSummoningCorrectionFixture fixture)
        {
            var protectedUnits = fixture.UnitsBefore.OfType<UnitEntityData>().Where(x =>
                x != null && !x.Destroyed && x.IsInGame && ReferenceEquals(x.HoldingState, fixture.Scene)).ToArray();
            if (protectedUnits.Length == 0) throw new InvalidOperationException("No nonempty protected scene intake.");
            Vector3 origin = fixture.Caster.Position;
            int surveyed = 0;
            for (int radius = 64; radius <= 96; radius += 8)
            foreach (Vector3 direction in CompassOffsets)
            {
                surveyed++;
                Vector3 requested = origin + direction.normalized * radius;
                var node = ObstacleAnalyzer.GetNearestNode(requested);
                if (node.node == null || !node.node.Walkable || Vector3.Distance(requested, node.clampedPosition) > 1.5f)
                    continue;
                Vector3 point = node.clampedPosition;
                float separation = protectedUnits.Min(x => {
                    Vector3 delta = x.Position - point; delta.y = 0; return delta.magnitude - x.Corpulence;
                });
                if (separation < 60f) continue;
                var end = ObstacleAnalyzer.GetNearestNode(point + Vector3.forward * 5f);
                if (end.node == null || !end.node.Walkable ||
                    Vector3.Distance(end.clampedPosition, point + Vector3.forward * 5f) > 1.5f ||
                    Vector3.Distance(ObstacleAnalyzer.TraceAlongNavmesh(point, end.clampedPosition), end.clampedPosition) > .15f)
                    continue;
                // Only the disposable caster moves. The original party anchor
                // and every other intake actor remain at their exact positions.
                PlaceExpandedSummoningUnit(fixture.Caster, point);
                return new JObject { ["caster"] = fixture.Caster.UniqueId, ["point"] = Vec(point),
                    ["protectedActors"] = protectedUnits.Length, ["nearestProtectedGapMeters"] = separation,
                    ["surveyed"] = surveyed, ["scene"] = "existing Oleg outdoor fixture",
                    ["intakeActorsMoved"] = 0 };
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
