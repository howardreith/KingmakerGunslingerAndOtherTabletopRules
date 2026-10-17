using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.UI.SettingsUI;
using TurnBased.Controllers;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static UnitCombatEngagementController _sprint25EngagementController;
        private static ExpandedSummoningCorrectionFixture _sprint25EngagementFixture;
        private static readonly HashSet<string> Sprint25EngagementTicked = new HashSet<string>();

        private static bool Sprint25EngagementOwnedUnitPrefix(
            UnitCombatEngagementController __instance, UnitEntityData unit)
        {
            if (!ReferenceEquals(__instance, _sprint25EngagementController)) return true;
            var fixture = _sprint25EngagementFixture;
            bool owned = fixture != null && unit != null &&
                (ReferenceEquals(unit, fixture.Caster) || ReferenceEquals(unit, fixture.Hostile) ||
                 fixture.Created.Any(value => ReferenceEquals(value, unit)));
            if (owned) Sprint25EngagementTicked.Add(unit.UniqueId);
            return owned;
        }

        private static void TickSprint25Engagement(ExpandedSummoningCorrectionFixture fixture,
            Sprint25OverrunObserver observer = null)
        {
            foreach (UnitEntityData unit in fixture.Created.Where(value => value != null &&
                !value.Destroyed && value.IsInState && value.View != null))
                UnitProneController.Tick(unit);
            observer?.ObserveNativeProne();
            // Keep the real queue drain and native responder accounting.
            // TickUnit is filtered to exact fixture owners. The controller's
            // final unconditional ambient prevention-flag clear is restored.
            var flags = fixture.UnitsBefore.OfType<UnitEntityData>().ToDictionary(
                value => value, value => value.CombatState.PreventAttacksOfOpporunityNextFrame);
            try { _sprint25EngagementController.Tick(); }
            finally
            {
                foreach (var pair in flags)
                    pair.Key.CombatState.PreventAttacksOfOpporunityNextFrame = pair.Value;
            }
        }

        private sealed class Sprint25OverrunObserver :
            IGlobalRulebookHandler<RuleCombatManeuver>,
            IGlobalRulebookHandler<RuleAttackWithWeapon>,
            IGlobalRulebookHandler<RuleDealDamage>
        {
            internal UnitEntityData Owner, Target;
            internal Action PrepareContactResources;
            internal bool ContactResourcesPrepared;
            internal readonly List<RuleCombatManeuver> Maneuvers = new List<RuleCombatManeuver>();
            internal readonly List<RuleAttackWithWeapon> Opportunities = new List<RuleAttackWithWeapon>();
            internal readonly List<RuleDealDamage> Damage = new List<RuleDealDamage>();
            internal readonly JArray ManeuverTimeline = new JArray();
            internal readonly JArray OpportunityTimeline = new JArray();
            internal bool LastActiveProne;
            internal void ObserveNativeProne()
            {
                bool active = Target.Descriptor.State.Prone.Active;
                if (active == LastActiveProne) return;
                ManeuverTimeline.Add(new JObject { ["stage"] = "native-prone-state-transition",
                    ["precedingManeuverRuleId"] = Maneuvers.Count == 0 ? (JToken)null :
                        System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Maneuvers.Last()),
                    ["target"] = Target.UniqueId, ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                    ["proneRequested"] = Target.Descriptor.State.Prone.ShouldBeActive, ["proneActive"] = active });
                LastActiveProne = active;
            }
            public void OnEventAboutToTrigger(RuleCombatManeuver evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) && evt.Type == CombatManeuver.Overrun)
                {
                    ManeuverTimeline.Add(new JObject { ["stage"] = "before-native-rule",
                        ["ruleId"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(evt),
                        ["target"] = evt.Target?.UniqueId, ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                        ["proneRequested"] = evt.Target?.Descriptor.State.Prone.ShouldBeActive,
                        ["proneActive"] = evt.Target?.Descriptor.State.Prone.Active });
                    if (ReferenceEquals(evt.Target, Target) && !ContactResourcesPrepared)
                    {
                        PrepareContactResources?.Invoke();
                        ContactResourcesPrepared = true;
                    }
                    UnityEngine.Random.InitState(FindNativeD20Seed(10));
                }
            }
            public void OnEventDidTrigger(RuleCombatManeuver evt)
            {
                if (!ReferenceEquals(evt.Initiator, Owner)) return;
                Maneuvers.Add(evt);
                ManeuverTimeline.Add(new JObject { ["stage"] = "after-native-rule",
                    ["ruleId"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(evt),
                    ["target"] = evt.Target?.UniqueId, ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                    ["success"] = evt.Success, ["partial"] = evt.IsPartialSuccess,
                    ["proneRequested"] = evt.Target?.Descriptor.State.Prone.ShouldBeActive,
                    ["proneActive"] = evt.Target?.Descriptor.State.Prone.Active });
            }
            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            { if (ReferenceEquals(evt.Target, Target)) Damage.Add(evt); }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (ReferenceEquals(evt.Target, Target) && evt.IsAttackOfOpportunity)
                {
                    Opportunities.Add(evt);
                    OpportunityTimeline.Add(new JObject {
                        ["attackRuleId"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(evt),
                        ["precedingManeuverRuleId"] = Maneuvers.Count == 0 ? (JToken)null :
                            System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Maneuvers.Last()),
                        ["initiator"] = evt.Initiator.UniqueId, ["target"] = evt.Target.UniqueId,
                        ["weapon"] = evt.Weapon.Blueprint.AssetGuid,
                        ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                        ["proneActive"] = evt.Target.Descriptor.State.Prone.Active });
                }
            }
        }

        private Vector3 Sprint25IsolatedAnchor(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach (float distance in new[] { 20f, 30f, 40f })
                foreach (Vector3 direction in CompassOffsets)
                {
                    var candidate = AstarPath.active.GetNearest(fixture.Caster.Position + direction * distance);
                    if (candidate.node != null && candidate.node.Walkable &&
                        fixture.UnitsBefore.OfType<UnitEntityData>().All(value =>
                            Vector3.Distance(candidate.clampedPosition, value.Position) > 16f))
                        return candidate.clampedPosition;
                }
            throw new InvalidOperationException("No native combat lane isolated from protected fixture-anchor actors.");
        }

        private IEnumerable<int> Sprint25Overrun(ExpandedSummoningCorrectionFixture fixture)
        {
            Vector3 casterBefore = fixture.Caster.Position;
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            UnitCombatEngagementController previous = Game.Instance.CombatEngagementController;
            var controller = new UnitCombatEngagementController();
            MethodInfo setter = typeof(Game).GetProperty("CombatEngagementController")?.GetSetMethod(true);
            MethodInfo tick = typeof(UnitCombatEngagementController).GetMethod("TickUnit",
                BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(UnitEntityData) }, null);
            MethodInfo prefix = typeof(RuntimeTestRunner).GetMethod(nameof(Sprint25EngagementOwnedUnitPrefix),
                BindingFlags.Static | BindingFlags.NonPublic);
            if (setter == null || tick == null || prefix == null || _sprint25EngagementController != null)
                throw new InvalidOperationException("Native scoped engagement prerequisites unavailable.");
            bool patched = false, assigned = false, subscribed = false;
            try
            {
                _sprint25EngagementController = controller;
                _sprint25EngagementFixture = fixture;
                Sprint25EngagementTicked.Clear();
                _context.Harmony.Patch(tick, new HarmonyMethod(prefix), null, null);
                patched = true;
                setter.Invoke(Game.Instance, new object[] { controller }); assigned = true;
                EventBus.Subscribe(controller); subscribed = true;
                PlaceExpandedSummoningUnit(fixture.Caster, Sprint25IsolatedAnchor(fixture));
                string[] cases = { "failure", "partial", "threshold", "maneuver-immune", "prone-immune", "already-prone", "repeated-notification", "repeated-command", "exhausted" };
                foreach (string sample in cases)
                    foreach (int frame in Sprint25OverrunCase(fixture, sample)) yield return frame;
                foreach (int frame in Sprint25OverrunCase(fixture, "threshold", true)) yield return frame;
            }
            finally
            {
                bool unsubscribed = !subscribed, unpatched = !patched;
                try
                {
                    try
                    {
                        if (subscribed) { EventBus.Unsubscribe(controller); unsubscribed = true; }
                    }
                    finally { if (assigned) setter.Invoke(Game.Instance, new object[] { previous }); }
                }
                finally
                {
                    try
                    {
                        if (patched) { _context.Harmony.Unpatch(tick, HarmonyPatchType.Prefix, _context.ModId); unpatched = true; }
                    }
                    finally
                    {
                        _sprint25EngagementController = null;
                        _sprint25EngagementFixture = null;
                        PlaceExpandedSummoningUnit(fixture.Caster, casterBefore);
                        UnityEngine.Random.state = randomBefore;
                        Sprint25Check("engagement-controller-restored", unsubscribed && unpatched && ReferenceEquals(previous, Game.Instance.CombatEngagementController),
                            new JObject { ["exactOwnedTickIds"] = new JArray(Sprint25EngagementTicked),
                                ["previousWasNull"] = previous == null, ["referenceRestored"] = ReferenceEquals(previous, Game.Instance.CombatEngagementController),
                                ["unsubscribed"] = unsubscribed, ["unpatched"] = unpatched },
                            "The request-scoped native engagement controller, event subscription and exact-owner tick filter are removed, restoring the original reference.");
                    }
                }
            }
        }

        private IEnumerable<int> Sprint25OverrunCase(
            ExpandedSummoningCorrectionFixture fixture, string sample, bool turnBased = false)
        {
            var created = new List<UnitEntityData>();
            UnitUseAbility command = null;
            Sprint25OverrunObserver observer = null;
            try
            {
                UnitEntityData owner = CastExpandedSummoningQuietUnit(fixture, "brachiosaurus");
                UnitEntityData target = CastExpandedSummoningQuietUnit(fixture, "wolf", fixture.Hostile);
                UnitEntityData later = CastExpandedSummoningQuietUnit(fixture, "wolf", fixture.Hostile);
                UnitEntityData[] responders = { CastExpandedSummoningQuietUnit(fixture, "wolf"),
                    CastExpandedSummoningQuietUnit(fixture, "wolf") };
                created.AddRange(new[] { owner, target, later }.Concat(responders));
                foreach (int frame in Sprint25NativeAppearance(owner, "brachiosaurus")) yield return frame;
                foreach (UnitEntityData unit in created)
                {
                    unit.Stats.HitPoints.BaseValue = 100000;
                    unit.Descriptor.Damage = 0;
                    unit.CombatState.OnNewRound();
                    unit.CombatState.AttackOfOpportunityCount = 0;
                }
                target.Stats.Dexterity.BaseValue = 10;
                BlueprintAbility blueprint = fixture.Blueprints.OfType<BlueprintAbility>().Single(
                    value => value.name == "KMG_Summoning_Special_Brachiosaurus_Overrun");
                Ability granted = owner.Descriptor.Abilities.GetAbility(blueprint);
                if (granted == null) throw new InvalidOperationException("Exact owner lacks ordinary Overrun.");
                var ability = new AbilityData(granted);
                Vector3 start; string route;
                TargetWrapper destination = Sprint23FindTrampleRoute(fixture, owner, target,
                    ability, out start, out route, true);
                if (destination == null) throw new InvalidOperationException("No legal ordinary Overrun route: " + route);
                Vector3 end = destination.Point;
                Vector3 direction = (end - start).normalized;
                Vector3 side = Vector3.Cross(direction, Vector3.up).normalized;
                PlaceExpandedSummoningUnit(later, Vector3.Lerp(start, end, .8f));
                for (int index = 0; index < responders.Length; index++)
                {
                    UnitEntityData responder = responders[index];
                    PlaceExpandedSummoningUnit(responder, target.Position + side * (index == 0 ? 1f : -1f));
                    responder.CombatState.AttackOfOpportunityCount = sample == "exhausted" ? 0 : 1;
                    responder.CombatState.PreventAttacksOfOpporunityNextFrame = false;
                    responder.Memory.Add(target); target.Memory.Add(responder);
                    responder.JoinCombat();
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.JoinCombat(); target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick();
                new UnitCombatPrepareController().Tick();
                int initiativeFrames = 0;
                while (responders.Any(value => !value.CombatState.CanActInCombat) && initiativeFrames++ < 80)
                    yield return 0;
                if (responders.Any(value => !value.CombatState.CanActInCombat || !value.Descriptor.State.CanAct))
                    throw new InvalidOperationException("Native opportunity responders have not completed initiative prerequisites.");
                TickSprint25Engagement(fixture);

                if (sample == "maneuver-immune") target.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null);
                if (sample == "prone-immune") target.Descriptor.State.AddConditionImmunity(UnitCondition.Prone);
                if (sample == "already-prone") target.Descriptor.State.Prone.ShouldBeActive = true;
                UnitProneController.Tick(target);
                if (sample == "already-prone" && !target.Descriptor.State.Prone.Active)
                    throw new InvalidOperationException("Native already-prone prerequisite did not activate.");
                int cmb = Rulebook.Trigger(new RuleCalculateCMB(owner, target, CombatManeuver.Overrun)).Result;
                int beforeCmd = ProbeCombatManeuverDefenceParts(owner, target, CombatManeuver.Overrun).Result;
                int margin = sample == "failure" ? -1 : sample == "partial" ? 3 : 5;
                target.Stats.AdditionalCMD.BaseValue += cmb + 10 - margin - beforeCmd;
                if (turnBased)
                {
                    var admission = new JObject();
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                    int waited = 0;
                    while (!Sprint18OwnerIsActing(owner) && waited++ < 500) yield return 0;
                    if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner))
                        throw new InvalidOperationException("Ordinary Overrun owner never reached its native turn: " + admission);
                }
                bool beforeProne = target.Descriptor.State.Prone.ShouldBeActive || target.Descriptor.State.Prone.Active;
                bool beforeProneActive = target.Descriptor.State.Prone.Active;
                bool beforeProneRequested = target.Descriptor.State.Prone.ShouldBeActive;
                bool knockEligible = UnitHelper.CanBeKnockedOff(target);
                int[] beforeOpportunities = responders.Select(value => value.CombatState.AttackOfOpportunityCount).ToArray();
                int[] minimumOpportunities = (int[])beforeOpportunities.Clone();
                int[] lastOpportunities = (int[])beforeOpportunities.Clone();
                var opportunityTrace = new JArray();
                observer = new Sprint25OverrunObserver { Owner = owner, Target = target, LastActiveProne = beforeProneActive };
                observer.PrepareContactResources = () => {
                    // Setup only the disposable responders' prerequisite at
                    // contact. Native round refill during the movement/animation
                    // lead-in must not turn the exhausted case into a fresh one.
                    // Keep the maneuvering owner's deliberately exhausted
                    // opportunity out of the two-responder prerequisite too.
                    owner.CombatState.AttackOfOpportunityCount = 0;
                    for (int index = 0; index < responders.Length; index++)
                    {
                        responders[index].CombatState.AttackOfOpportunityCount = sample == "exhausted" ? 0 : 1;
                        beforeOpportunities[index] = responders[index].CombatState.AttackOfOpportunityCount;
                        minimumOpportunities[index] = beforeOpportunities[index];
                        lastOpportunities[index] = beforeOpportunities[index];
                    }
                    opportunityTrace.Add(new JObject { ["phase"] = "fixture-resource-prerequisite-before-native-maneuver",
                        ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                        ["ownerOpportunityCount"] = owner.CombatState.AttackOfOpportunityCount,
                        ["responderCanActInCombat"] = new JArray(responders.Select(value => value.CombatState.CanActInCombat)),
                        ["responderCanAct"] = new JArray(responders.Select(value => value.Descriptor.State.CanAct)),
                        ["responderEngagedWithTarget"] = new JArray(responders.Select(value => value.CombatState.IsEngage(target))),
                        ["counts"] = new JArray(beforeOpportunities) });
                };
                EventBus.Subscribe(observer);
                float standardBefore = owner.CombatState.Cooldown.StandardAction;
                float moveBefore = owner.CombatState.Cooldown.MoveAction;
                float maxStandard = standardBefore, maxMove = moveBefore;
                int frames = 0;
                int commandCount = sample == "repeated-command" ? 2 : 1;
                for (int attempt = 0; attempt < commandCount; attempt++)
                {
                if (attempt > 0)
                {
                    EndExpandedSummoningDetachedAbility(command);
                    owner.Commands.InterruptAll(true);
                    owner.Commands.RemoveFinishedAndUpdateQueue();
                    destination = Sprint23FindTrampleRoute(fixture, owner, target, ability, out start, out route, true);
                    if (destination == null) throw new InvalidOperationException("No repeated ordinary Overrun route: " + route);
                }
                command = new UnitUseAbility(ability, destination);
                if (!command.CanStart) throw new InvalidOperationException("Native Overrun command cannot start.");
                owner.Commands.Run(command);
                var controller = new UnitActionController();
                frames = 0;
                bool repeated = false;
                for (; frames < 700; frames++)
                {
                    if (!command.IsFinished && (!command.IsStarted || command.ExecutionProcess == null))
                    {
                        if (command.Animation != null && frames > 4) command.Animation.IsActed = true;
                        ElementalBreathScenario.TickCommand(controller, command);
                    }
                    if (command.ExecutionProcess != null)
                        TickExpandedSummoningDetachedAbility(command, frames);
                    if (sample == "repeated-notification" && !repeated && observer.Maneuvers.Count > 0)
                    {
                        var rider = owner.Descriptor.Progression.Features.Enumerable
                            .SelectMany(value => value.SelectComponents<BrachiosaurusOverrunKnockdown>()).Single();
                        rider.OnEventDidTrigger(observer.Maneuvers[0]);
                        rider.OnEventDidTrigger(observer.Maneuvers[0]);
                        repeated = true;
                    }
                    TickSprint25Engagement(fixture, observer);
                    maxStandard = Math.Max(maxStandard, owner.CombatState.Cooldown.StandardAction);
                    maxMove = Math.Max(maxMove, owner.CombatState.Cooldown.MoveAction);
                    yield return 0;
                    if (observer.ContactResourcesPrepared)
                    {
                        int[] counts = responders.Select(value => value.CombatState.AttackOfOpportunityCount).ToArray();
                        for (int index = 0; index < counts.Length; index++)
                            minimumOpportunities[index] = Math.Min(minimumOpportunities[index], counts[index]);
                        if (!counts.SequenceEqual(lastOpportunities))
                            opportunityTrace.Add(new JObject { ["phase"] = "native-spending-or-round-refill",
                                ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                                ["counts"] = new JArray(counts), ["observedAttacks"] = observer.Opportunities.Count });
                        lastOpportunities = counts;
                    }
                    if (command.IsFinished && frames > 100) break;
                }
                }
                RuleCombatManeuver result = observer.Maneuvers.FirstOrDefault(value => ReferenceEquals(value.Target, target));
                bool afterProne = target.Descriptor.State.Prone.ShouldBeActive || target.Descriptor.State.Prone.Active;
                bool afterProneActive = target.Descriptor.State.Prone.Active;
                bool knockdown = sample != "failure" && sample != "partial" &&
                    sample != "maneuver-immune" && sample != "prone-immune" && sample != "already-prone";
                bool expectedOpportunities = knockdown && sample != "exhausted";
                int[] afterOpportunities = responders.Select(value => value.CombatState.AttackOfOpportunityCount).ToArray();
                bool legalResponders = observer.Opportunities.All(value => responders.Contains(value.Initiator)) &&
                    responders.All(value => observer.Opportunities.Count(attack => ReferenceEquals(attack.Initiator, value)) <= 1);
                bool attributedDamage = observer.Damage.All(value => responders.Contains(value.Initiator));
                bool responses = expectedOpportunities ? responders.All(value => observer.Opportunities.Count(
                    attack => ReferenceEquals(attack.Initiator, value)) == 1) && minimumOpportunities.All(value => value == 0) :
                    observer.Opportunities.Count == 0 && beforeOpportunities.SequenceEqual(minimumOpportunities);
                float travelled = Vector3.Distance(start, owner.Position);
                bool nativeBudget = !command.Cutscene && !command.IsIgnoreCooldown &&
                    blueprint.IsFullRoundAction && ability.RequireFullRoundAction;
                bool outcome = result != null && (sample == "maneuver-immune" ?
                    !result.Success && !result.IsPartialSuccess && !afterProne :
                    result.InitiatorCMB == 32 && result.SuccesValue == margin &&
                    result.Success == (margin >= 0) && result.IsPartialSuccess == (margin >= 0 && margin < 5));
                Sprint25Check("overrun-" + sample + (turnBased ? "-turn-based" : "-rtwp"), outcome && observer.ContactResourcesPrepared && command.IsStarted && command.IsFinished && nativeBudget &&
                    maxStandard > standardBefore && (!turnBased || maxMove > moveBefore) &&
                    (sample == "failure" || travelled > 2f) && travelled <= owner.Stats.Speed.ModifiedValue * .3048f * 2f + 1f &&
                    afterProne == (beforeProne || knockdown) && afterProneActive == (beforeProneActive || knockdown) && legalResponders && attributedDamage && responses &&
                    observer.Maneuvers.Count == commandCount,
                    new JObject { ["unit"] = owner.UniqueId, ["target"] = target.UniqueId,
                        ["laterTarget"] = later.UniqueId, ["ability"] = blueprint.AssetGuid,
                        ["sample"] = sample, ["route"] = route, ["frames"] = frames,
                        ["nativeInitiativeWaitFrames"] = initiativeFrames,
                        ["commandCount"] = commandCount, ["mode"] = turnBased ? "turn-based" : "rtwp",
                        ["commandFinished"] = command.IsFinished, ["commandResult"] = command.Result.ToString(),
                        ["effectiveCmd"] = result?.TargetCMD, ["cmb"] = result?.InitiatorCMB,
                        ["naturalRoll"] = result == null ? (JToken)null : result.InitiatorRoll.Value,
                        ["nativePartialSuccess"] = result?.IsPartialSuccess, ["nativeSuccess"] = result?.Success,
                        ["nativeMargin"] = result?.SuccesValue, ["nativeAutoFailure"] = result?.AutoFailure,
                        ["knockEligible"] = knockEligible, ["proneBefore"] = beforeProne, ["proneAfter"] = afterProne,
                        ["proneRequestedBefore"] = beforeProneRequested,
                        ["proneActiveBefore"] = beforeProneActive, ["proneActiveAfter"] = afterProneActive,
                        ["maneuverTimeline"] = observer.ManeuverTimeline,
                        ["opportunityTimeline"] = observer.OpportunityTimeline,
                        ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)),
                        ["damageAttributedToNativeResponders"] = attributedDamage,
                        ["nativeBudget"] = nativeBudget, ["travelled"] = travelled,
                        ["standardCooldown"] = new JArray(standardBefore, maxStandard),
                        ["moveCooldown"] = new JArray(moveBefore, maxMove),
                        ["opportunityTiming"] = opportunityTrace,
                        ["responders"] = new JArray(responders.Select((value, index) => new JObject {
                            ["unit"] = value.UniqueId, ["before"] = beforeOpportunities[index],
                            ["after"] = afterOpportunities[index],
                            ["minimumObserved"] = minimumOpportunities[index],
                            ["attacks"] = observer.Opportunities.Count(attack => ReferenceEquals(attack.Initiator, value)) })),
                        ["maneuverTargets"] = new JArray(observer.Maneuvers.Select(value => value.Target.UniqueId)),
                        ["opportunities"] = new JArray(observer.Opportunities.Select(value => new JObject {
                            ["initiator"] = value.Initiator.UniqueId, ["target"] = value.Target.UniqueId,
                            ["weapon"] = value.Weapon.Blueprint.AssetGuid, ["hit"] = value.AttackRoll?.IsHit })) },
                    "One production command resolves the native margin/partial-success and knockdown eligibility, affects only one target, stays inside its full-round double-move budget, and spends each legal responder's opportunity at most once.");
            }
            finally
            {
                if (observer != null) EventBus.Unsubscribe(observer);
                EndExpandedSummoningDetachedAbility(command);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                foreach (UnitEntityData unit in created)
                {
                    unit.Commands.InterruptAll(true);
                    if (unit.IsInCombat) unit.LeaveCombat();
                }
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, created);
            }
        }
    }
}
