using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // One combat case per fresh process: the prior native command fixture
        // demonstrated a combat-state boundary, so these are not shared cases.
        private IEnumerable<int> ReviewHound30Combat(ExpandedSummoningCorrectionFixture fixture, bool natural, bool single = false, bool ai = false, bool turn = false)
        {
            string id = turn ? "hound-turn" : ai ? "hound-ai" : single ? "hound-single" : natural ? "hound-natural" : "hound-armed";
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            CreateExpandedSummoningCorrectionHostile(fixture);
            var target = fixture.Hostile;
            int targetAc = target.Stats.AC.BaseValue;
            var areas = Game.Instance.State.AreaEffects.All.ToArray();
            UnitEntityData owner = null;
            UnityEngine.Object[] privateResources = new UnityEngine.Object[0];
            ItemEntityWeapon[] items = new ItemEntityWeapon[0];
            JObject queuesBeforePostCombatCancellation = null;
            try
            {
                try
                {
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                    Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                    for (int frame = 0; frame < 16; frame++) yield return 0;
                    owner = CastExpandedSummoningOwnTier(fixture, "hound-archon");
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    foreach (int frame in Outsider30NativeAppearance(owner, "hound-archon")) yield return frame;
                    foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                    if (natural)
                    {
                        var change = new UnitSwitchHandEquipmentSet(1); owner.Commands.Run(change);
                        DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                        while (!change.IsFinished && DateTime.UtcNow < deadline) yield return 0;
                        if (!change.IsFinished || owner.Body.CurrentHandEquipmentSetIndex != 1)
                            throw new InvalidOperationException("Hound natural mode did not finish its native switch.");
                    }
                    var main = owner.Body.PrimaryHand.MaybeWeapon;
                    var bite = owner.Body.AdditionalLimbs.Select(slot => slot.MaybeWeapon).Single(value => value != null);
                    items = new[] { main, bite };
                    var toggle = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value =>
                        value.Blueprint.AssetGuid == "a7b339e4f6ff93a4697df5d7a87ff619");
                    var offMain = Sprint29WeaponRule(owner, main); var offBite = Sprint29WeaponRule(owner, bite);
                    if (!natural) foreach (int frame in Sprint22SetPowerAttack(owner, true)) yield return frame;
                    var mainRule = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner, main, 0));
                    var biteRule = Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner, bite, 0));
                    var onMain = Sprint29WeaponRule(owner, main); var onBite = Sprint29WeaponRule(owner, bite);
                    bool source = natural ? !toggle.IsOn && owner.Buffs.GetBuff(toggle.Blueprint.Buff) == null :
                        toggle.IsOn && toggle.IsRunning && owner.Buffs.GetBuff(toggle.Blueprint.Buff) != null &&
                        mainRule.BonusSources.Any(value => value.Bonus == -2 && ReferenceEquals(value.Source?.Blueprint, toggle.Blueprint.Buff)) &&
                        biteRule.BonusSources.Any(value => value.Bonus == -2 && ReferenceEquals(value.Source?.Blueprint, toggle.Blueprint.Buff));
                    bool rules = natural ? (int)onMain["attack"] == 8 && (int)onBite["attack"] == 8 &&
                        (int)onMain["damageBonus"] == 2 && (int)onBite["damageBonus"] == 2 :
                        (int)offMain["attack"] == 9 && (int)offBite["attack"] == 3 &&
                        (int)onMain["attack"] == 7 && (int)onBite["attack"] == 1 &&
                        (int)onMain["damageBonus"] == 9 && (int)onBite["damageBonus"] == 3;
                    Sprint25Check(id + "-power-state", source && rules, new JObject {
                        ["owner"] = owner.UniqueId, ["mode"] = owner.Body.CurrentHandEquipmentSetIndex,
                        ["powerAttack"] = Sprint22PowerAttackState(owner, toggle), ["offMain"] = offMain, ["offBite"] = offBite,
                        ["liveMain"] = onMain, ["liveBite"] = onBite,
                        ["mainSources"] = new JArray(mainRule.BonusSources.Select(value => new JObject {
                            ["bonus"] = value.Bonus, ["source"] = value.Source?.Blueprint?.AssetGuid })),
                        ["biteSources"] = new JArray(biteRule.BonusSources.Select(value => new JObject {
                            ["bonus"] = value.Bonus, ["source"] = value.Source?.Blueprint?.AssetGuid })) },
                        "Native toggle/buff, source-attributed attack modifiers and actual equipped weapon rules precede the resolved command.");
                    if (!source || !rules) throw new InvalidOperationException("Hound native Power Attack/rules precondition failed.");
                    owner.Commands.InterruptAll(true); target.Commands.InterruptAll(true);
                    owner.Commands.RemoveFinishedAndUpdateQueue(); target.Commands.RemoveFinishedAndUpdateQueue();
                    Vector3 far; string lane, direction;
                    Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 5f, out far, out lane, out direction);
                    PlaceExpandedSummoningUnit(owner, origin);
                    PlaceExpandedSummoningUnit(target, origin + (far - origin).normalized * (owner.Corpulence + target.Corpulence + .8f));
                    var probe = new UnitAttack(target); probe.Init(owner);
                    PlaceExpandedSummoningUnit(target, origin + (far - origin).normalized * Math.Max(
                        owner.Corpulence + target.Corpulence + .01f, probe.ApproachRadius - .05f));
                    target.Stats.AC.BaseValue = 0; // Disposable target only; no supplied roll or hit.
                    foreach (var actor in new[] { owner, target })
                    {
                        actor.Wake(0f);
                        if (!Game.Instance.State.AwakeUnits.Contains(actor)) Game.Instance.State.AwakeUnits.Add(actor);
                    }
                    owner.Memory.Add(target); target.Memory.Add(owner);
                    owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                    new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                    owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                    new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                    bool ready = owner.IsInGame && owner.View.IsInGame && target.IsInGame && target.View.IsInGame &&
                        owner.Descriptor.State.CanAct && owner.IsEnemy(target) && target.IsEnemy(owner) &&
                        owner.HasLOS(target) && owner.Memory.ContainsVisible(target) && probe.IsUnitEnoughClose &&
                        !target.Descriptor.State.IsDead;
                    Sprint25Check(id + "-ready", ready, new JObject {
                        ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["lane"] = lane, ["direction"] = direction,
                        ["inRange"] = probe.IsUnitEnoughClose, ["approachRadius"] = probe.ApproachRadius,
                        ["distance"] = owner.DistanceTo(target), ["los"] = owner.HasLOS(target),
                        ["visible"] = owner.Memory.ContainsVisible(target), ["targetAc"] = target.Stats.AC.ModifiedValue,
                        ["powerAttack"] = Sprint22PowerAttackState(owner, toggle) },
                        "A fresh awake visible disposable hostile and native action/range preconditions precede the command.");
                    if (!ready) throw new InvalidOperationException("Hound native attack fixture is not ready.");
                    var swordView = owner.View.GetComponent<HoundArchonSwordView>();
                    bool nativeSword = false;
                    int visibilityWaits = 0;
                    bool waitingBefore = _sprint25WaitingForUnityAppearance;
                    DateTime visibilityDeadline = DateTime.UtcNow.AddSeconds(5);
                    try
                    {
                        while (visibilityWaits < 600 && DateTime.UtcNow < visibilityDeadline)
                        {
                            nativeSword = swordView != null && swordView.isActiveAndEnabled && swordView.LateUpdates > 0 &&
                                swordView.ObservedEquipmentSet == owner.Body.CurrentHandEquipmentSetIndex &&
                                swordView.ObservedWeaponGuid == main.Blueprint.AssetGuid && swordView.Model != null &&
                                swordView.Model.activeSelf == !natural && swordView.Model.GetComponentsInChildren<Renderer>(true).Any();
                            if (nativeSword) break;
                            // This observer's native clock can process the switch within
                            // one Unity Update. Observe the actual per-view LateUpdate;
                            // never call Refresh or force model visibility from the fixture.
                            _sprint25WaitingForUnityAppearance = true;
                            Game.Instance.IsPaused = false; visibilityWaits++; yield return 0;
                        }
                    }
                    finally { _sprint25WaitingForUnityAppearance = waitingBefore; }
                    Sprint25Check(id + "-weapon-model", nativeSword, new JObject {
                        ["owner"] = owner.UniqueId, ["status"] = swordView?.Status,
                        ["unityFrame"] = Time.frameCount, ["visibilityWaits"] = visibilityWaits,
                        ["lateUpdates"] = swordView?.LateUpdates, ["observedMode"] = swordView?.ObservedEquipmentSet,
                        ["observedWeapon"] = swordView?.ObservedWeaponGuid, ["expectedWeapon"] = main.Blueprint.AssetGuid,
                        ["refreshUnityFrame"] = swordView?.LastRefreshUnityFrame,

                        ["active"] = swordView?.Model?.activeSelf, ["expectedArmed"] = !natural,
                        ["parent"] = swordView?.Model?.transform.parent?.name,
                        ["nativeCharacterAvatarPresent"] = owner.View.GetComponent<Kingmaker.Visual.CharacterSystem.Character>() != null },
                        "The exact owned native sword model follows the measured palm only while that weapon is equipped; natural mode hides it.");
                    if (!nativeSword) throw new InvalidOperationException("Hound owned weapon model/mode mismatch.");
                    privateResources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View);
                    var observer = new Sprint24AttackObserver { Owner = owner, Target = target };
                    var costs = new JArray(); var endings = new JArray();
                    var issued = ai ? null : new UnitAttack(target) { ForceFullAttack = !single, IsSingleAttack = single };
                    UnitAttack command = issued;
                    var commands = new HashSet<UnitCommand>();
                    var ambient = new HashSet<string>(); int aiTicks = 0;
                    EventBus.Subscribe(observer);
                    try
                    {
                        using (var economy = new FirearmReviewCosts(owner, costs))
                        using (var trace = new Sprint26CommandTrace(owner, endings))
                        {
                            if (turn)
                            {
                                var admission = new JObject();
                                foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                                int waited = 0; DateTime turnDeadline = DateTime.UtcNow.AddSeconds(30);
                                while (waited++ < 500 && DateTime.UtcNow < turnDeadline && !Sprint18OwnerIsActing(owner)) yield return 0;
                                bool acting = (bool)admission["admitted"] && owner.IsVisibleForPlayer && Sprint18OwnerIsActing(owner);
                                Sprint25Check(id + "-turn-admission", acting, new JObject {
                                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["admission"] = admission,
                                    ["turn"] = Sprint22TurnState(owner, target), ["waitedFrames"] = waited },
                                    "The exact owned Hound receives its real native active turn before the standard attack.");
                                if (!acting) throw new InvalidOperationException("Hound native turn admission failed.");
                            }
                            if (ai)
                            {
                                Sprint28SetBrainActivePreservingActions(owner, true);
                                owner.Brain.RestoreAvailableActions();
                            }
                            else { commands.Add(issued); owner.Commands.Run(issued); }
                            int frames = 0; DateTime deadline = DateTime.UtcNow.AddSeconds(45);
                            int expected = single ? 1 : natural ? 2 : 3;
                            while (frames++ < 4000 && DateTime.UtcNow < deadline)
                            {
                                if (ai && (frames == 1 || frames % 60 == 0 &&
                                    !owner.Commands.Raw.Any(value => value != null && !value.IsFinished)))
                                {
                                    foreach (string candidate in Sprint24TickExactAi(owner))
                                        if (candidate != owner.UniqueId) ambient.Add(candidate);
                                    aiTicks++;
                                }
                                foreach (var observed in owner.Commands.Raw.Where(value => value != null)) commands.Add(observed);
                                if (ai) command = commands.OfType<UnitAttack>().FirstOrDefault(value =>
                                    value.AiAction != null && value.IsFinished &&
                                    (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value)));
                                if (command != null && command.IsFinished &&
                                    observer.WeaponAttacks.Count >= command.AllAttacks.Count &&
                                    (!ai || observer.Damage.Any(value => value.Damage > 0))) break;
                                Game.Instance.IsPaused = false; yield return 0;
                            }
                            if (ai)
                            {
                                Sprint28SetBrainActivePreservingActions(owner, false);
                                expected = command?.AllAttacks.Count ?? 0;
                            }
                            int hits = observer.WeaponAttacks.Count(value => value.AttackRoll != null && value.AttackRoll.IsHit);
                            int impactFrames = 0; DateTime impactDeadline = DateTime.UtcNow.AddSeconds(10);
                            while (observer.Damage.Count < hits && impactFrames++ < 600 && DateTime.UtcNow < impactDeadline) yield return 0;
                            var mesh = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                                .Single(value => value.name == TheropodVisualPolicy.BodyRendererName);
                            string presentation = CaptureSprint17OriginalBodyFrame(owner, mesh, id + "-combat");
                            bool aiOwned = !ai || command != null && command.AiAction != null && aiTicks > 0 &&
                                commands.OfType<UnitAttack>().Count(value => value.AiAction != null && value.IsFinished) == 1;
                            bool exact = command != null && command.IsFinished && aiOwned &&
                                (ai ? expected == 1 || expected == 3 : expected == (single ? 1 : natural ? 2 : 3)) &&
                                (command.Result == UnitCommand.ResultType.Success || trace.Exhausted(command)) &&
                                command.AllAttacks.Count == expected && observer.OwnerWeaponAttacks.Count == expected &&
                                observer.WeaponAttacks.Count == expected && observer.Rolls.Count == expected &&
                                observer.WeaponAttacks.Count(value => ReferenceEquals(value.Weapon, main)) == (expected == 1 || natural ? 1 : 2) &&
                                observer.WeaponAttacks.Count(value => ReferenceEquals(value.Weapon, bite)) == (expected == 1 ? 0 : 1) &&
                                observer.Damage.Count == hits && observer.Damage.Any(value => value.Damage > 0) &&
                                !target.Descriptor.State.IsDead && economy.Count(command) == 1 && economy.Exact(command, turn) &&
                                !command.Cutscene && !command.IsIgnoreCooldown;
                            Sprint25Check(id + "-resolved", exact, new JObject {
                                ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["frames"] = frames,
                                ["commandFinished"] = command?.IsFinished, ["commandResult"] = command?.Result.ToString(),
                                ["planned"] = command?.AllAttacks.Count, ["aiOwned"] = aiOwned, ["aiTicks"] = aiTicks,
                                ["commands"] = new JArray(commands.Select(Sprint27AiCommand)),
                                ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["attacks"] = observer.WeaponAttacks.Count, ["hits"] = hits,
                                ["rolls"] = new JArray(observer.WeaponAttacks.Select(value => new JObject {
                                    ["weapon"] = value.Weapon?.Blueprint.AssetGuid, ["hit"] = value.AttackRoll?.IsHit,
                                    ["naturalRoll"] = value.AttackRoll?.Roll.Value, ["attackBonus"] = value.AttackRoll?.AttackBonus,
                                    ["attackPenalty"] = value.AttackRoll?.AttackBonusPenalty,
                                    ["weaponDamageBonus"] = value.AttackRoll?.WeaponStats?.DamageDescription.FirstOrDefault()?.Bonus })),
                                ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)), ["impactWaitFrames"] = impactFrames,
                                ["costs"] = costs, ["nativeEnds"] = endings, ["powerAttack"] = Sprint22PowerAttackState(owner, toggle),
                                ["turnBased"] = turn, ["turnAfterCommand"] = turn ? Sprint22TurnState(owner, target) : null,
                                ["presentation"] = presentation, ["fixtureCommands"] = ai ? 0 : 1, ["forcedRolls"] = 0,
                                ["rendererState"] = new JArray(owner.View.GetComponentsInChildren<Renderer>(true).Select(value =>
                                    new JObject { ["name"] = value.name, ["enabled"] = value.enabled,
                                        ["center"] = new JArray(value.bounds.center.x, value.bounds.center.y, value.bounds.center.z) })) },
                                ai ? "The unchanged native brain may make useful Aid decisions, then selects and resolves its own armed attack fallback with real damage and one native attack cost; no fixture attack command." :
                                single ? "One native standard single attack resolves only the greatsword, with no secondary bite or extra iterative, real damage and one action cost." :
                                "One native full-attack command spends its action and resolves the exact armed sword/bite or alternative primary slam/bite routine with native random rolls and damage.");
                            if (!exact) throw new InvalidOperationException("Hound native attack failed: " + id);
                        }
                    }
                    finally { EventBus.Unsubscribe(observer); }
                }
                finally
                {
                    if (owner != null && !owner.Destroyed)
                    {
                        Sprint28SetBrainActivePreservingActions(owner, false);
                        owner.Commands.InterruptAll(true); target.Commands.InterruptAll(true);
                        if (owner.IsInCombat) owner.LeaveCombat(); if (target.IsInCombat) target.LeaveCombat();
                        queuesBeforePostCombatCancellation = OutsiderMeleeCleanupState(fixture, owner, targetAc);
                        foreach (var actor in new[] { owner, target })
                        {
                            foreach (var pending in actor.Commands.Raw.Where(value =>
                                value != null && !value.IsFinished).ToArray()) pending.Interrupt(true);
                            actor.Commands.RemoveFinishedAndUpdateQueue();
                        }
                        if (target.Memory.Contains(owner)) target.Memory.Remove(owner);
                        if (owner.Memory.Contains(target)) owner.Memory.Remove(target);
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    }
                    target.Stats.AC.BaseValue = targetAc;
                    ResetExpandedSummoningHostile(fixture); Game.Instance.Player.UpdateIsInCombat();
                    foreach (var pending in target.Commands.Raw.Where(value =>
                        value != null && !value.IsFinished).ToArray()) pending.Interrupt(true);
                    target.Commands.RemoveFinishedAndUpdateQueue();
                }
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var actorCleanup = OutsiderMeleeCleanupState(fixture, owner, targetAc);
                bool clean = (bool)actorCleanup["clean"] && privateResources.Length >= 3 && privateResources.All(value => value == null) &&
                    !Game.Instance.State.AreaEffects.All.Except(areas).Any() && items.All(value => !value.IsLootable && value.Collection == null);
                Sprint25Check(id + "-cleanup", clean, new JObject {
                    ["actorCleanup"] = actorCleanup, ["queuesBeforePostCombatCancellation"] = queuesBeforePostCombatCancellation,
                    ["ownerAbsent"] = !owner.IsInState, ["privateCaptured"] = privateResources.Length,
                    ["privateRemaining"] = privateResources.Count(value => value != null),
                    ["areasExact"] = !Game.Instance.State.AreaEffects.All.Except(areas).Any(),
                    ["itemCollectionsAbsent"] = items.All(value => value.Collection == null),
                    ["itemsNonlootable"] = items.All(value => !value.IsLootable), ["targetStillInCombat"] = target.IsInCombat,
                    ["freshProcessBoundary"] = true },
                    "Owned body resources, items and emitters are absent; no following combat subcase shares this process.");
                if (!clean) throw new InvalidOperationException("Hound combat owned cleanup failed.");
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
