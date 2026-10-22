using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string Lantern31StandardRay = "d4c2ce6c90094fdfb0fd908312372d72";
        private const string Lantern31FullRay = "fe85344c9eaa4d6596293ff1e3b6b552";

        private IEnumerable<int> ReviewLantern31Rays(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (int frame in ReviewLantern31RaySubcase(fixture, false)) yield return frame;
                foreach (int frame in ReviewLantern31RaySubcase(fixture, true)) yield return frame;
            }
            finally
            {
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> ReviewLantern31RaySubcase(
            ExpandedSummoningCorrectionFixture fixture, bool fullRound)
        {
            string id = fullRound ? "lantern-ray-full" : "lantern-ray-standard";
            string abilityGuid = fullRound ? Lantern31FullRay : Lantern31StandardRay;
            int expectedProjectiles = fullRound ?
                ExpandedSummoningSpecialProfiles.LanternFullRayProjectiles :
                ExpandedSummoningSpecialProfiles.LanternStandardRayProjectiles;
            object[] unitsBefore = Game.Instance.State.Units.All.Cast<object>().ToArray();
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            UnitEntityData owner = null;
            UnitEntityData target = null;
            UnitUseAbility command = null;
            var observer = new Sprint24AttackObserver();
            Lantern31ResistanceObserver resistance = null;
            try
            {
                owner = CastExpandedSummoningOwnTier(fixture, "lantern-archon");
                target = CastExpandedSummoningOwnTier(fixture, "lantern-archon");
                observer.Owner = owner;
                observer.Target = target;
                resistance = new Lantern31ResistanceObserver(owner, target);
                Sprint28SetBrainActivePreservingActions(owner, false);
                Sprint28SetBrainActivePreservingActions(target, false);
                // This exact Lantern is the disposable defense target. A
                // summoned unit retains its master's group relationship even
                // after a faction switch, so copy the already-proven hostile
                // fixture envelope after detaching only this owned target.
                target.Descriptor.SetMaster(null);
                target.Descriptor.SwitchFactions(fixture.Hostile.Faction, true);
                target.GroupId = fixture.Hostile.GroupId;
                target.AttackFactions.Match(fixture.Hostile.AttackFactions.ToArray());
                target.Descriptor.Stats.HitPoints.BaseValue = 100;
                target.Descriptor.Stats.AC.BaseValue = 0;
                for (int frame = 0; frame < 12; frame++) yield return 0;

                var ability = owner.Descriptor.Abilities.Enumerable.Single(value =>
                    value.Blueprint.AssetGuid == abilityGuid);
                var data = new AbilityData(ability);
                var selected = new TargetWrapper(target);

                Vector3 nearTarget;
                string nearLane, nearDirection;
                Vector3 nearOrigin = Sprint24AiCombatLane(fixture.Caster, owner,
                    fixture.Hostile, 8f, out nearTarget, out nearLane,
                    out nearDirection);
                PlaceExpandedSummoningUnit(fixture.Hostile,
                    fixture.Caster.Position + Vector3.forward * 30f);
                Vector3 laneVector = (nearTarget - nearOrigin).normalized;
                Vector3 farOrigin = nearOrigin;
                Vector3 farTarget = nearOrigin + laneVector * 12.5f;
                string farLane = nearLane + ";extended-to-12.5m-for-native-approach";
                PlaceExpandedSummoningUnit(owner, nearOrigin);
                PlaceExpandedSummoningUnit(target, nearTarget);
                owner.Commands.InterruptAll(true);
                target.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                target.Commands.RemoveFinishedAndUpdateQueue();
                foreach (UnitEntityData actor in new[] { owner, target })
                {
                    actor.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(actor))
                        Game.Instance.State.AwakeUnits.Add(actor);
                }
                owner.Memory.Add(target);
                target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded();
                target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                if (owner.HasLOS(target)) owner.Memory.Add(target).Visible = true;
                owner.JoinCombat();
                target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick();
                new UnitCombatPrepareController().Tick();

                bool nearAccepted = data.CanTarget(selected);
                PlaceExpandedSummoningUnit(owner, farOrigin);
                PlaceExpandedSummoningUnit(target, farTarget);
                owner.SightCache.ClearIfNeeded();
                target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                float refusedDistance = owner.DistanceTo(target);
                bool farLos = owner.HasLOS(target);
                float nativeApproachDistance = data.GetApproachDistance(target);
                bool farAccepted = data.CanTarget(selected);
                PlaceExpandedSummoningUnit(owner, nearOrigin);
                PlaceExpandedSummoningUnit(target, nearTarget);
                owner.SightCache.ClearIfNeeded();
                target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                if (owner.HasLOS(target)) owner.Memory.Add(target).Visible = true;
                bool nearRestored = data.CanTarget(selected);
                bool friendRefused = !data.CanTarget(new TargetWrapper(fixture.Caster));
                bool selfRefused = !data.CanTarget(new TargetWrapper(owner));
                var targetCheckers = (data.Blueprint.ComponentsArray ??
                    Array.Empty<Kingmaker.Blueprints.BlueprintComponent>())
                    .OfType<Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityTargetChecker>()
                    .ToArray();
                var targetCheckerRows = new JArray(targetCheckers.Select(value =>
                    new JObject { ["component"] = value.GetType().FullName,
                        ["enemyAccepted"] = value.CanTarget(owner, selected) }));
                var delivery = data.Blueprint.ComponentsArray.OfType<
                    Kingmaker.UnitLogic.Abilities.Components.AbilityDeliverProjectile>()
                    .Single();

                int touch = Rulebook.Trigger(new RuleCalculateAC(owner, target,
                    AttackType.RangedTouch)).TargetAC;
                int ordinary = Rulebook.Trigger(new RuleCalculateAC(owner, target,
                    AttackType.Ranged)).TargetAC;
                var parameters = data.CalculateParams();
                var context = new AbilityExecutionContext(
                    data, parameters, selected, null);
                int nativeSr = target.Get<UnitPartSpellResistance>()?.GetValue(context) ?? 0;
                var dr = target.Get<UnitPartDamageReduction>();
                var drSources = dr == null ? new JArray() : new JArray(dr.AllSources.Select(value =>
                    new JObject { ["component"] = value.GetType().FullName,
                        ["value"] = value.GetCurrentValue() }));
                float nearDistance = owner.DistanceTo(target);
                bool ready = nearAccepted && farAccepted && nearRestored &&
                    refusedDistance > nativeApproachDistance + .01f &&
                    nearDistance < nativeApproachDistance &&
                    friendRefused && selfRefused &&
                    data.Blueprint.CanTargetEnemies && data.Blueprint.CanTargetFriends &&
                    !data.Blueprint.CanTargetSelf && !data.Blueprint.CanTargetPoint &&
                    targetCheckers.OfType<LanternRayEnemyTargetChecker>().Count() == 1 &&
                    targetCheckers.All(value => value.CanTarget(owner, selected)) &&
                    Math.Abs(delivery.Length.Meters -
                        new Feet(ExpandedSummoningSpecialProfiles.LanternRayRangeFeet).Meters) < .001f &&
                    owner.IsInGame && target.IsInGame &&
                    owner.Descriptor.State.CanAct && owner.IsEnemy(target) &&
                    target.IsEnemy(owner) && target.Descriptor.Master.Value == null &&
                    owner.HasLOS(target) && owner.Memory.ContainsVisible(target) &&
                    data.IsAvailable && data.CanTarget(selected) &&
                    data.Blueprint.ActionType == UnitCommand.CommandType.Standard &&
                    data.Blueprint.IsFullRoundAction == fullRound &&
                    touch != ordinary && nativeSr > 0 &&
                    drSources.OfType<JObject>().Any(row => (int)row["value"] >= 10);
                Sprint25Check(id + "-ready", ready, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                    ["ability"] = data.Blueprint.AssetGuid,
                    ["abilityName"] = data.Blueprint.name,
                    ["fullRound"] = data.Blueprint.IsFullRoundAction,
                    ["actionType"] = data.Blueprint.ActionType.ToString(),
                    ["projectilesExpected"] = expectedProjectiles,
                    ["rangeFeet"] = ExpandedSummoningSpecialProfiles.LanternRayRangeFeet,
                    ["farDistanceMeters"] = refusedDistance,
                    ["nativeApproachDistanceMeters"] = nativeApproachDistance,
                    ["farAcceptedForApproach"] = farAccepted, ["farLos"] = farLos,
                    ["farLane"] = farLane,
                    ["nearDistanceMeters"] = nearDistance,
                    ["nearLane"] = nearLane, ["nearAccepted"] = nearAccepted,
                    ["nearRestored"] = nearRestored, ["available"] = data.IsAvailable,
                    ["canTarget"] = data.CanTarget(selected), ["los"] = owner.HasLOS(target),
                    ["visible"] = owner.Memory.ContainsVisible(target),
                    ["ownerCanOrdinarilyAttack"] = owner.CanAttack(target),
                    ["ownerEnemy"] = owner.IsEnemy(target),
                    ["targetEnemy"] = target.IsEnemy(owner),
                    ["targetMasterAbsent"] = target.Descriptor.Master.Value == null,
                    ["ownerGroup"] = owner.GroupId, ["targetGroup"] = target.GroupId,
                    ["friendRefused"] = friendRefused, ["selfRefused"] = selfRefused,
                    ["canTargetEnemies"] = data.Blueprint.CanTargetEnemies,
                    ["nativeRelationBypass"] = data.Blueprint.CanTargetFriends,
                    ["deliveryRangeMeters"] = delivery.Length.Meters,
                    ["targetCheckers"] = targetCheckerRows,
                    ["touchAc"] = touch, ["ordinaryAc"] = ordinary,
                    ["nativeSpellResistance"] = nativeSr, ["damageReduction"] = drSources },
                    "The exact owned ray has its intended action mode and 30-foot delivery/native approach distance; native targeting accepts a distant enemy for approach but refuses friends and self, and the proven 8-m lane acquires the enemy facing distinct live touch AC, SR and DR consumers.");
                if (!ready) throw new InvalidOperationException("Lantern ray native preconditions failed: " + id);

                var costs = new JArray();
                EventBus.Subscribe(observer);
                EventBus.Subscribe(resistance);
                try
                {
                    command = new UnitUseAbility(data, selected);
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        owner.Commands.Run(command);
                        int frames = 0;
                        DateTime deadline = DateTime.UtcNow.AddSeconds(40);
                        while (frames++ < 3600 && DateTime.UtcNow < deadline &&
                            (!command.IsFinished || command.ExecutionProcess != null &&
                                !command.ExecutionProcess.IsEnded ||
                             observer.Rolls.Count < expectedProjectiles))
                        {
                            Game.Instance.IsPaused = false;
                            yield return 0;
                        }
                        int hits = observer.Rolls.Count(value => value.IsHit);
                        int impactFrames = 0;
                        DateTime impactDeadline = DateTime.UtcNow.AddSeconds(15);
                        while (impactFrames++ < 900 && DateTime.UtcNow < impactDeadline &&
                            observer.Damage.Count < hits)
                        {
                            Game.Instance.IsPaused = false;
                            yield return 0;
                        }
                        JObject cost = costs.OfType<JObject>().SingleOrDefault();
                        bool direct = observer.Damage.Count == expectedProjectiles &&
                            observer.Damage.All(value => value.DamageBundle.Count() == 1 &&
                                value.DamageBundle.Single() is DirectDamage && value.Damage > 0 &&
                                value.Damage == value.DamageWithoutReduction);
                        bool touchExact = observer.Rolls.Count == expectedProjectiles &&
                            observer.Rolls.All(value => value.IsHit &&
                                value.AttackType == AttackType.RangedTouch &&
                                value.TargetAC == touch);
                        bool exact = command.IsFinished &&
                            command.Result == UnitCommand.ResultType.Success &&
                            command.ExecutionProcess != null && command.ExecutionProcess.IsEnded &&
                            touchExact && direct && resistance.Rows.Count == 0 &&
                            economy.Count(command) == 1 && economy.Exact(command, false) &&
                            cost != null && (bool)cost["fullRound"] == fullRound &&
                            !command.Cutscene && !command.IsIgnoreCooldown &&
                            command.AiAction == null && !target.Descriptor.State.IsDead;
                        Sprint25Check(id + "-resolved", exact, new JObject {
                            ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                            ["ability"] = data.Blueprint.AssetGuid,
                            ["frames"] = frames, ["impactWaitFrames"] = impactFrames,
                            ["command"] = Sprint27AiCommand(command),
                            ["executionEnded"] = command.ExecutionProcess?.IsEnded,
                            ["rolls"] = new JArray(observer.Rolls.Select(value => new JObject {
                                ["attackType"] = value.AttackType.ToString(),
                                ["naturalRoll"] = value.Roll.Value,
                                ["attackBonus"] = value.AttackBonus,
                                ["targetAc"] = value.TargetAC,
                                ["hit"] = value.IsHit })),
                            ["hits"] = hits,
                            ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)),
                            ["spellResistanceChecks"] = resistance.Rows.DeepClone(),
                            ["nativeSpellResistance"] = nativeSr,
                            ["damageReduction"] = drSources.DeepClone(),
                            ["costs"] = costs, ["forcedRolls"] = 0,
                            ["fixtureEffectExecutions"] = 0 },
                            "One real native command launches the exact ray count, resolves every random ranged-touch hit to terminal direct light damage through live DR without an SR check, and pays its standard or full-round RTWP action mode.");
                        if (!exact) throw new InvalidOperationException("Lantern ray native resolution failed: " + id);
                    }
                }
                finally
                {
                    EventBus.Unsubscribe(resistance);
                    EventBus.Unsubscribe(observer);
                }
            }
            finally
            {
                EventBus.Unsubscribe(resistance);
                EventBus.Unsubscribe(observer);
                foreach (UnitEntityData unit in new[] { owner, target }.Where(value =>
                    value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    if (unit.IsInCombat) unit.LeaveCombat();
                }
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, target }.Where(value =>
                    value != null && !value.Destroyed).ToArray());
            }
            for (int frame = 0; frame < 12; frame++) yield return 0;
            bool actorsAbsent = owner != null && target != null && owner.Destroyed && target.Destroyed &&
                !owner.IsInState && !target.IsInState &&
                !Game.Instance.State.Units.All.Any(value => ReferenceEquals(value, owner) ||
                    ReferenceEquals(value, target));
            bool areasExact = !Game.Instance.State.AreaEffects.All.Except(areasBefore).Any();
            bool unitsExact = !Game.Instance.State.Units.All.Cast<object>().Except(unitsBefore).Any();
            bool commandsClosed = command == null || command.IsFinished;
            bool clean = actorsAbsent && areasExact && unitsExact && commandsClosed;
            Sprint25Check(id + "-cleanup", clean, new JObject {
                ["ownerAbsent"] = owner != null && !owner.IsInState,
                ["targetAbsent"] = target != null && !target.IsInState,
                ["areasExact"] = areasExact, ["unitsExact"] = unitsExact,
                ["commandClosed"] = commandsClosed,
                ["nextSubcasePermitted"] = clean },
                "The disposable ray pair, command and owned aura areas are absent before the next fixed subcase; failed cleanup stops the sequence.");
            if (!clean) throw new InvalidOperationException("Lantern ray shared-process cleanup failed: " + id);
        }

        private sealed class Lantern31ResistanceObserver :
            IGlobalRulebookHandler<RuleSpellResistanceCheck>
        {
            private readonly UnitEntityData owner;
            private readonly UnitEntityData target;
            internal readonly JArray Rows = new JArray();
            internal Lantern31ResistanceObserver(UnitEntityData owner, UnitEntityData target)
            { this.owner = owner; this.target = target; }
            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt) { }
            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
                if (!ReferenceEquals(evt.Context?.MaybeCaster, owner) ||
                    !ReferenceEquals(evt.Target, target)) return;
                Rows.Add(new JObject { ["ability"] = evt.Ability?.AssetGuid,
                    ["caster"] = owner.UniqueId, ["target"] = target.UniqueId,
                    ["resistance"] = evt.SpellResistance,
                    ["resisted"] = evt.IsSpellResisted,
                    ["hasRoll"] = evt.HasResistanceRoll });
            }
        }
    }
}
