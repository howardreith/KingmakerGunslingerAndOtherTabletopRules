using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Six fixed cases, each in its own fresh process. Reuses the successful
        // Hound owned-memory/clock admission and the native cost observer.
        private IEnumerable<int> ReviewSprints3031ActionBoundary(ExpandedSummoningCorrectionFixture fixture, string key)
        {
            if (!new[] { "lemure", "dretch", "hound-archon", "lantern-archon", "astral-deva", "trumpet-archon" }.Contains(key))
                throw new InvalidOperationException("Unsupported bounded action owner.");
            bool ai = key == "lantern-archon" || key == "astral-deva";
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            CreateExpandedSummoningCorrectionHostile(fixture);
            var target = fixture.Hostile;
            int oldAc = target.Stats.AC.BaseValue, oldHp = target.Stats.HitPoints.BaseValue;
            var partyItems = Game.Instance.Player.Inventory.Items.ToArray();
            var partyCounts = partyItems.Select(item => item.Count).ToArray();
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            UnitEntityData owner = null;
            var fixtureAid = new List<Buff>();
            var observer = new Sprint24AttackObserver();
            bool subscribed = false;
            string ownerId = null, targetId = target.UniqueId;
            Sprints3031LifetimeTrace lifetime = null;
            var cleanupErrors = new JArray();
            var cleanupActors = new JArray();
            Action<string, Action> cleanup = (stage, action) => {
                try { action(); }
                catch (Exception error) { cleanupErrors.Add(new JObject { ["stage"] = stage, ["exception"] = error.ToString() }); }
            };
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 40f);
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, key);
                ownerId = owner.UniqueId;
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                // Native presentation must become usable; no renderer/rig repair.
                DateTime appearanceDeadline = DateTime.UtcNow.AddSeconds(12);
                bool waitingBefore = _sprint25WaitingForUnityAppearance;
                try {
                    while (DateTime.UtcNow < appearanceDeadline && (owner.View == null || !EntityFadedIn(owner) ||
                        DissolveAmount(owner) > .02f || !owner.Descriptor.State.CanAct))
                    { _sprint25WaitingForUnityAppearance = EntityFadedIn(owner); Game.Instance.IsPaused = false; yield return 0; }
                } finally { _sprint25WaitingForUnityAppearance = waitingBefore; }
                if (key == "lantern-archon") {
                    // This case asks the unchanged brain for its ray fallback.
                    // Native Aid eligibility rejects already-aided allies. Seed
                    // that explicit fixture state only on these disposable actors;
                    // this is not evidence of casting Aid or spending its action.
                    var aidBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                        KingmakerGunslinger.Bootstrap.BlueprintBootstrap.Library,
                        "319b4679f25779e4e9d04360381254e1", "native Aid fixture precondition");
                    foreach (var actor in new[] { owner, fixture.Caster }) {
                        if (actor.Buffs.GetBuff(aidBuff) != null) continue;
                        var applied = ApplySprint12Buff(actor, aidBuff, owner, TimeSpan.FromSeconds(180), 3);
                        if (applied.AppliedBuff == null) throw new InvalidOperationException("Owned already-aided fixture failed.");
                        fixtureAid.Add(applied.AppliedBuff);
                    }
                    Sprint25Check("lantern-ray-ai-aided-precondition",
                        owner.Buffs.GetBuff(aidBuff) != null && fixture.Caster.Buffs.GetBuff(aidBuff) != null,
                        new JObject { ["owner"] = owner.UniqueId, ["caster"] = fixture.Caster.UniqueId,
                            ["fixtureAddedBuffs"] = fixtureAid.Count, ["nativeAidCastClaim"] = false },
                        "Already-aided disposable allies make the unchanged native ray AI available without changing its brain, scores or spell access.");
                }
                var power = owner.Descriptor.ActivatableAbilities.Enumerable.FirstOrDefault(value =>
                    value.Blueprint.AssetGuid == "a7b339e4f6ff93a4697df5d7a87ff619");
                if (power != null) foreach (int frame in Sprint22SetPowerAttack(owner, true)) yield return frame;
                owner.Stats.HitPoints.BaseValue = 1000; // Disposable action fixture; no damage is fabricated as a hit.
                if (key == "dretch" || key == "trumpet-archon")
                    foreach (int frame in Sprints3031HeldEscape(fixture, owner, key)) yield return frame;
                var items = owner.Descriptor.Inventory.Items.ToArray();
                var privateResources = Sprints3031PrivateResources(owner, key);
                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction, new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(target, far);
                var probe = new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
                probe.Init(owner);
                float distance = key == "lantern-archon" ? 5f : Mathf.Max(.25f, probe.ApproachRadius - .1f);
                PlaceExpandedSummoningUnit(owner, far - (far - origin).normalized * distance);
                target.Stats.AC.BaseValue = 0; target.Stats.HitPoints.BaseValue = 1000;
                if (key == "trumpet-archon") ObserveSprints3031BlindFight(fixture, owner, target);
                if (key == "astral-deva") owner.Damage = 100; // Explicit AI healing precondition, not a claimed hit.
                foreach (var actor in new[] { owner, target }) {
                    actor.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(actor)) Game.Instance.State.AwakeUnits.Add(actor);
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                if (owner.HasLOS(target)) owner.Memory.Add(target).Visible = true;
                if (target.HasLOS(owner)) target.Memory.Add(owner).Visible = true;
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                var coverage = _sprint25Clock.DescribeTurnCoverage();
                var tickIds = ((JArray)coverage["tickUnits"]).Values<string>().ToArray();
                var foreign = Game.Instance.State.Units.All.Where(actor => actor != null && !actor.Destroyed && actor.IsInState &&
                    actor.IsInCombat && !tickIds.Contains(actor.UniqueId, StringComparer.Ordinal)).ToArray();
                bool ready = foreign.Length == 0 && owner.View != null && owner.Descriptor.State.CanAct &&
                    owner.HasLOS(target) && owner.Memory.ContainsVisible(target) && owner.IsEnemy(target) &&
                    !target.Descriptor.State.IsDead && (ai || probe.IsUnitEnoughClose);
                Sprint25Check(key + "-boundary-preconditions", ready, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["lane"] = lane,
                    ["nativeClock"] = coverage, ["foreignCombatants"] = new JArray(foreign.Select(actor => actor.UniqueId)),
                    ["los"] = owner.HasLOS(target), ["visible"] = owner.Memory.ContainsVisible(target),
                    ["range"] = probe.IsUnitEnoughClose, ["healingPreconditionDamage"] = owner.Damage },
                    "Only the exact owned pair and the established profile anchor enter the native turn clock.");
                if (!ready) throw new InvalidOperationException("Bounded turn fixture preconditions failed: " + key);
                var admission = new JObject();
                foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                DateTime turnDeadline = DateTime.UtcNow.AddSeconds(30);
                while (DateTime.UtcNow < turnDeadline && !Sprint18OwnerIsActing(owner)) yield return 0;
                if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner))
                    throw new InvalidOperationException("Exact owner did not receive its native turn: " + key);
                observer.Owner = owner; observer.Target = target;
                var powerRows = new JArray(); var beforePowerRows = new JArray();
                observer.BeforeRoll = value => beforePowerRows.Add(new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = value.Target.UniqueId,
                    ["weapon"] = value.Weapon?.Blueprint.AssetGuid,
                    ["powerAttack"] = power == null ? null : Sprint22PowerAttackState(owner, power) });
                observer.AfterWeapon = value => powerRows.Add(new JObject {
                    ["owner"] = value.Initiator.UniqueId, ["target"] = value.Target.UniqueId,
                    ["weapon"] = value.Weapon.Blueprint.AssetGuid,
                    ["powerAttack"] = power == null ? null : Sprint22PowerAttackState(owner, power),
                    ["damageBonus"] = value.AttackRoll?.WeaponStats?.DamageDescription.FirstOrDefault()?.Bonus,
                    ["attackSources"] = value.AttackRoll?.AttackBonusRule == null ? null :
                        new JArray(value.AttackRoll.AttackBonusRule.BonusSources.Select(modifier => new JObject {
                            ["bonus"] = modifier.Bonus, ["source"] = modifier.Source?.Blueprint?.AssetGuid })) });
                EventBus.Subscribe(observer); subscribed = true;
                var costs = new JArray(); var commands = new HashSet<UnitCommand>();
                using (var economy = new FirearmReviewCosts(owner, costs))
                using (var trace = new Sprint26CommandTrace(owner, new JArray()))
                {
                    UnitCommand completed = null;
                    int damageBefore = owner.Damage;
                    if (ai) { Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions(); }
                    else { completed = new UnitAttack(target) { IsSingleAttack = key != "trumpet-archon", ForceFullAttack = key == "trumpet-archon" }; commands.Add(completed); owner.Commands.Run(completed); }
                    DateTime deadline = DateTime.UtcNow.AddSeconds(45); int activeTurnFrames = 0;
                    while (DateTime.UtcNow < deadline) {
                        if (Sprint18OwnerIsActing(owner)) activeTurnFrames++;
                        foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        if (ai) completed = commands.FirstOrDefault(value => value.AiAction != null && value.IsFinished &&
                            value is UnitUseAbility && (key == "lantern-archon" ?
                                ((UnitUseAbility)value).Spell.Blueprint.AssetGuid == Lantern31StandardRay ||
                                ((UnitUseAbility)value).Spell.Blueprint.AssetGuid == Lantern31FullRay :
                                value.AiAction.name == "KMG_Summoning_OutsiderCelestial_AstralDeva_HealAi") &&
                            ((UnitUseAbility)value).ExecutionProcess != null && ((UnitUseAbility)value).ExecutionProcess.IsEnded);
                        if (completed != null && completed.IsFinished &&
                            (!(completed is UnitUseAbility) || ((UnitUseAbility)completed).ExecutionProcess?.IsEnded == true) &&
                            (key == "astral-deva" ? owner.Damage < damageBefore : observer.Rolls.Count > 0 &&
                                observer.Damage.Count == observer.Rolls.Count(value => value.IsHit))) break;
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    var cast = completed as UnitUseAbility;
                    bool ray = key != "lantern-archon" || cast != null &&
                        (cast.Spell.Blueprint.AssetGuid == Lantern31StandardRay && observer.Rolls.Count == 1 ||
                         cast.Spell.Blueprint.AssetGuid == Lantern31FullRay && observer.Rolls.Count == 2);
                    bool healing = key != "astral-deva" || cast != null && owner.Damage < damageBefore &&
                        owner.Descriptor.Resources.GetResourceAmount(Sprints3031PersistenceResource(key, "HealResource")) == 0;
                    bool trumpetMelee = key != "trumpet-archon" || powerRows.Count == 3 && beforePowerRows.Count == 3 &&
                        powerRows.All(row => (int)row["damageBonus"] == 23 &&
                            ((JArray)row["attackSources"]).Count(modifier => (int)modifier["bonus"] == -4 &&
                                (string)modifier["source"] == power.Blueprint.Buff.AssetGuid) == 1) &&
                        beforePowerRows.All(row => (bool)row["powerAttack"]["isOn"] && (bool)row["powerAttack"]["isRunning"]);
                    bool exact = trumpetMelee && completed != null && completed.IsFinished && (completed.Result == UnitCommand.ResultType.Success || completed is UnitAttack && trace.Exhausted(completed)) &&
                        !completed.Cutscene && !completed.IsIgnoreCooldown && activeTurnFrames > 0 && economy.Count(completed) == 1 &&
                        economy.Exact(completed, true) && ray && healing && (!ai || completed.AiAction != null) &&
                        (ai || observer.WeaponAttacks.Count == (key == "trumpet-archon" ? 3 : 1)) &&
                        (key == "astral-deva" || observer.Damage.Count == observer.Rolls.Count(value => value.IsHit));
                    Sprint25Check(key + "-native-turn-action", exact, new JObject {
                        ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["admission"] = admission,
                        ["activeTurnFrames"] = activeTurnFrames, ["command"] = completed == null ? null : Sprint27AiCommand(completed),
                        ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                        ["damageBeforeHealing"] = damageBefore, ["damageAfterHealing"] = owner.Damage,
                        ["powerBeforeRoll"] = beforePowerRows, ["powerAndWeapons"] = powerRows, ["trumpetMeleeExact"] = trumpetMelee,
                        ["rolls"] = new JArray(observer.Rolls.Select(value => new JObject { ["weapon"] = value.Weapon?.Blueprint.AssetGuid,
                            ["roll"] = value.Roll.Value, ["hit"] = value.IsHit, ["targetAc"] = value.TargetAC, ["bonus"] = value.AttackBonus })),
                        ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)), ["executionEnded"] = cast?.ExecutionProcess?.IsEnded,
                        ["fixtureCommands"] = ai ? 0 : 1, ["fixtureAiTicks"] = 0, ["forcedRolls"] = 0 },
                        "A real native active turn spends one actual command cost (Trumpet full sword routine retains all three actual Power Attack contributions); Lantern/Astral use autonomous ray/healing decisions, and every observed ray impact is terminal.");
                    if (!exact) throw new InvalidOperationException("Bounded native action outcome failed: " + key);
                    if (key == "astral-deva" || key == "trumpet-archon")
                        ObserveSprints3031Disarm(fixture, owner, target, key);
                    // A queued next action is canceled before its turn can spend it.
                    Game.Instance.IsPaused = true;
                    UnitCommand pending = Sprints3031FutureCommand(owner, target, key);
                    int rollsBefore = observer.Rolls.Count, damageEventsBefore = observer.Damage.Count;
                    var stateBefore = Sprints3031PersistenceState(owner, key);
                    owner.Commands.Run(pending);
                    bool queued = owner.Commands.Raw.Contains(pending) && !pending.IsStarted && !pending.IsFinished;
                    pending.Interrupt(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    bool canceled = queued && pending.IsFinished && pending.Result != UnitCommand.ResultType.Success &&
                        economy.Count(pending) == 0 && observer.Rolls.Count == rollsBefore && observer.Damage.Count == damageEventsBefore &&
                        JToken.DeepEquals(stateBefore, Sprints3031PersistenceState(owner, key));
                    Sprint25Check(key + "-turn-cancel", canceled, new JObject {
                        ["owner"] = owner.UniqueId, ["queuedBeforeCancel"] = queued, ["command"] = Sprint27AiCommand(pending),
                        ["costApplications"] = economy.Count(pending), ["stateBefore"] = stateBefore,
                        ["stateAfter"] = Sprints3031PersistenceState(owner, key), ["nativeTurn"] = Sprint22TurnState(owner, target) },
                        "Actual native cancellation closes the queued future action without a roll, effect, item or resource debit.");
                    if (!canceled) throw new InvalidOperationException("Owned future action cancellation failed: " + key);
                    // Existing RTWP expiry boundary: no time jump and no protected
                    // actor reset. Arm the next command while paused, then expire
                    // only this owner's native summon marker before unpausing.
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                    Game.Instance.TurnBasedCombatController.Activate();
                    UnitCommand afterExpiry = Sprints3031FutureCommand(owner, target, key);
                    owner.Commands.Run(afterExpiry);
                    bool expiryQueued = owner.Commands.Raw.Contains(afterExpiry) && !afterExpiry.IsStarted;
                    lifetime = new Sprints3031LifetimeTrace(owner, key);
                    var queuedBeforeExpiry = Sprints3031QueueState(owner);
                    var expiry = ExpireSprint16OwnedBuff(owner, owner.Buffs.Enumerable.Single(buff =>
                        ReferenceEquals(buff.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)));
                    DateTime expiryDeadline = DateTime.UtcNow.AddSeconds(20);
                    while (DateTime.UtcNow < expiryDeadline && (!Sprints3031SourceAbsent(owner) || !afterExpiry.IsFinished || privateResources.Any(value => value != null)))
                    { Game.Instance.IsPaused = false; yield return 0; }
                    var live = Game.Instance.State.Units.All.Where(value => value != null && !value.Destroyed && value.IsInState).ToArray();
                    var itemRows = new JArray(items.Select(item => OutsiderEscapeItemState(item, owner, live)));
                    var queueAfterExpiry = Sprints3031QueueState(owner);
                    bool retired = lifetime.Calls.Count == 1 && lifetime.Calls.All(row =>
                        (bool)row["validOwnerBefore"] && (int)row["after"]["unfinishedRaw"] == 0 &&
                        (int)row["after"]["unfinishedQueued"] == 0 && (bool)row["stateUnspent"]);
                    bool noFutureWork = Sprints3031Commands(owner).All(command => command.IsFinished ||
                        command is UnitMoveTo && lifetime.NativeMoves.Contains(command)) &&
                        (!TurnBased.Controllers.CombatController.IsInTurnBasedCombat() ||
                            !ReferenceEquals(Game.Instance.TurnBasedCombatController.CurrentTurn?.Unit, owner));
                    bool expired = expiryQueued && retired && noFutureWork && Sprints3031SourceAbsent(owner) && afterExpiry.IsFinished &&
                        afterExpiry.Result != UnitCommand.ResultType.Success && economy.Count(afterExpiry) == 0 &&
                        observer.Rolls.Count == rollsBefore && observer.Damage.Count == damageEventsBefore &&
                        privateResources.All(value => value == null) && itemRows.All(row => (bool)row["safe"]) &&
                        Sprint28PartyInventoryExact(partyItems, partyCounts);
                    Sprint25Check(key + "-queued-expiry", expired, new JObject {
                        ["owner"] = owner.UniqueId, ["queued"] = expiryQueued, ["expiry"] = expiry,
                        ["command"] = Sprint27AiCommand(afterExpiry), ["costApplications"] = economy.Count(afterExpiry),
                        ["queueBeforeExpiry"] = queuedBeforeExpiry, ["queueAfterExpiry"] = queueAfterExpiry,
                        ["productLifetimeCallbacks"] = lifetime.Calls, ["nativeMovementOrigins"] = lifetime.MovementOrigins,
                        ["nativeTurnAfterExpiry"] = Sprint22TurnState(owner, target), ["ownerFutureWorkRetired"] = noFutureWork,
                        ["fixtureInterruptsAfterExpiry"] = 0,
                        ["ownerState"] = Sprints3031LifecycleState(owner), ["items"] = itemRows,
                        ["privateResourcesAbsent"] = privateResources.All(value => value == null), ["fixtureDisposalsBeforeAssertion"] = 0, ["capturedPrivateResources"] = privateResources.Length },
                        "Native summon expiry closes the owned future command and source grants without a later attack/blast, cost, transferable item or private-resource leak.");
                    if (!expired) throw new InvalidOperationException("Owned expiry/cancellation boundary failed: " + key);
                }
            }
            finally {
                // Record secondary failures without replacing a primary iterator
                // exception. Every independent safe restoration step is attempted.
                cleanup("lifetime-observer-hooks", () => lifetime?.Dispose());
                cleanup("unsubscribe", () => { if (subscribed) EventBus.Unsubscribe(observer); });
                foreach (var buff in fixtureAid.Where(value => value != null && !value.IsDisposed))
                    cleanup("fixture-aid", () => buff.Remove());
                cleanup("rtwp-setting", () => SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false);
                cleanup("rtwp-controller", () => Game.Instance.TurnBasedCombatController.Activate());
                foreach (var actor in new[] { owner, target }.Where(value => value != null)) {
                    string id = ReferenceEquals(actor, owner) ? ownerId : targetId;
                    cleanupActors.Add(new JObject { ["owner"] = id, ["destroyed"] = actor.Destroyed,
                        ["inState"] = actor.IsInState, ["viewPresent"] = actor.View != null });
                    // Destroyed data must not enter native view-dependent OnEnded.
                    if (actor.Destroyed) continue;
                    cleanup("live-queue-" + id, () => {
                        if (!actor.IsInState || actor.View == null)
                            throw new InvalidOperationException("Live disposable actor lacks safe command teardown state: " + id);
                        actor.Commands.InterruptAll(command => true);
                    });
                    cleanup("live-combat-" + id, () => { if (!actor.Destroyed && actor.IsInCombat) actor.LeaveCombat(); });
                }
                cleanup("live-owner-disposal", () => {
                    if (owner == null || owner.Destroyed) return;
                    if (Sprints3031Commands(owner).Any(command => !command.IsFinished))
                        throw new InvalidOperationException("Live owner still has executable work before fixture disposal.");
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                });
                cleanup("target-ac", () => target.Stats.AC.BaseValue = oldAc);
                cleanup("target-hp", () => target.Stats.HitPoints.BaseValue = oldHp);
                cleanup("target-reset", () => {
                    if (target.Destroyed || !target.IsInState || target.View == null)
                        throw new InvalidOperationException("Disposable target cannot safely enter native reset.");
                    ResetExpandedSummoningHostile(fixture);
                });
                cleanup("player-combat", () => Game.Instance.Player.UpdateIsInCombat());
                cleanup("restore-mode", () => SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode);
                cleanup("restore-controller", () => Game.Instance.TurnBasedCombatController.Activate());
                cleanup("restore-pause", () => Game.Instance.IsPaused = oldPause);
                // A primary failure continues unwinding unchanged. If there is
                // none, the following cleanup assertion fails on these errors.
                if (cleanupErrors.Count != 0)
                    Sprint25Check(key + "-secondary-cleanup-errors", false,
                        new JObject { ["owner"] = ownerId, ["target"] = targetId, ["errors"] = cleanupErrors },
                        "All safe cleanup steps are attempted; secondary failures remain separate from the original assertion/exception.");
            }
            bool clean = cleanupErrors.Count == 0 && !Game.Instance.State.AreaEffects.All.Except(areasBefore).Any() &&
                Sprint28PartyInventoryExact(partyItems, partyCounts) && !Sprints3031Commands(target).Any(command => !command.IsFinished);
            Sprint25Check(key + "-boundary-cleanup", clean, new JObject { ["cleanupErrors"] = cleanupErrors,
                ["actorsBeforeCleanup"] = cleanupActors, ["areasExact"] = !Game.Instance.State.AreaEffects.All.Except(areasBefore).Any(),
                ["partyInventoryExact"] = Sprint28PartyInventoryExact(partyItems, partyCounts), ["targetQueueEmpty"] = !target.Commands.Raw.Any(command => command != null && !command.IsFinished) },
                "The isolated case restores areas, owned queues and protected inventory; a failed cleanup stops the sequence.");
            if (!clean) throw new InvalidOperationException("Bounded action fixture cleanup failed.");
        }

        private static UnitCommand[] Sprints3031QueuedCommands(UnitEntityData actor)
        {
            var field = typeof(UnitCommands).GetField("m_Queue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var queue = field?.GetValue(actor.Commands) as LinkedList<UnitCommand>;
            if (queue == null) throw new InvalidOperationException("Installed native command queue contract is absent.");
            return queue.Where(command => command != null).ToArray();
        }
        private static UnitCommand[] Sprints3031Commands(UnitEntityData actor)
        {
            return actor.Commands.Raw.Where(command => command != null).Concat(Sprints3031QueuedCommands(actor)).Distinct().ToArray();
        }
        private static JObject Sprints3031CommandSummary(UnitCommand command)
        {
            // Terminal snapshots read data only, never live range/view predicates.
            return new JObject { ["type"] = command.GetType().Name, ["owner"] = command.Executor?.UniqueId,
                ["target"] = command.TargetUnit?.UniqueId, ["started"] = command.IsStarted,
                ["acted"] = command.IsActed, ["finished"] = command.IsFinished, ["result"] = command.Result.ToString() };
        }
        private static JObject Sprints3031QueueState(UnitEntityData actor)
        {
            var raw = actor.Commands.Raw.Where(command => command != null).ToArray();
            var queued = Sprints3031QueuedCommands(actor);
            return new JObject { ["owner"] = actor.UniqueId, ["destroyed"] = actor.Destroyed, ["inState"] = actor.IsInState,
                ["awake"] = Game.Instance.State.AwakeUnits.Contains(actor), ["viewPresent"] = actor.View != null,
                ["raw"] = new JArray(raw.Select(Sprints3031CommandSummary)), ["queued"] = new JArray(queued.Select(Sprints3031CommandSummary)),
                ["unfinishedRaw"] = raw.Count(command => !command.IsFinished), ["unfinishedQueued"] = queued.Count(command => !command.IsFinished) };
        }
        // Read-only hooks around the actual product callback; no command writes.
        private sealed class Sprints3031LifetimeTrace : IDisposable
        {
            private static Sprints3031LifetimeTrace active;
            private readonly UnitEntityData owner;
            private readonly string key;
            private readonly string id = "KMG.S3031.Lifetime." + Guid.NewGuid().ToString("N");
            private readonly Harmony12.HarmonyInstance harmony;
            private readonly System.Reflection.MethodInfo callback = typeof(OutsiderLifetimeBoundary).GetMethod("HandleBuffDidRemoved");
            private readonly System.Reflection.MethodInfo run = typeof(UnitCommands).GetMethod("Run", new[] { typeof(UnitCommand) });
            internal readonly JArray Calls = new JArray();
            internal readonly JArray MovementOrigins = new JArray();
            internal readonly HashSet<UnitCommand> NativeMoves = new HashSet<UnitCommand>();
            internal Sprints3031LifetimeTrace(UnitEntityData owner, string key)
            {
                if (active != null) throw new InvalidOperationException("Nested S30/31 lifetime observation.");
                this.owner = owner; this.key = key; harmony = Harmony12.HarmonyInstance.Create(id);
                var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                try {
                    harmony.Patch(callback, new Harmony12.HarmonyMethod(typeof(Sprints3031LifetimeTrace).GetMethod("Before", flags)),
                        new Harmony12.HarmonyMethod(typeof(Sprints3031LifetimeTrace).GetMethod("After", flags)));
                    harmony.Patch(run, new Harmony12.HarmonyMethod(typeof(Sprints3031LifetimeTrace).GetMethod("Running", flags)));
                    active = this;
                } catch (Exception original) {
                    try { Dispose(); } catch (Exception secondary) { throw new AggregateException(original, secondary); }
                    throw;
                }
            }
            private static void Before(OutsiderLifetimeBoundary __instance, Buff buff, out JObject __state)
            {
                __state = null; var trace = active;
                if (trace == null || !ReferenceEquals(__instance.Owner?.Unit, trace.owner) ||
                    !ReferenceEquals(buff?.Owner?.Unit, trace.owner) ||
                    !ReferenceEquals(buff?.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)) return;
                __state = new JObject { ["owner"] = trace.owner.UniqueId, ["marker"] = buff.Blueprint.AssetGuid,
                    ["validOwnerBefore"] = !trace.owner.Destroyed && trace.owner.IsInState && trace.owner.View != null,
                    ["before"] = Sprints3031QueueState(trace.owner), ["stateBefore"] = Sprints3031PersistenceState(trace.owner, trace.key) };
            }
            private static void After(JObject __state)
            {
                var trace = active; if (trace == null || __state == null) return;
                __state["after"] = Sprints3031QueueState(trace.owner);
                __state["stateAfter"] = Sprints3031PersistenceState(trace.owner, trace.key);
                __state["stateUnspent"] = JToken.DeepEquals(__state["stateBefore"], __state["stateAfter"]);
                trace.Calls.Add(__state);
            }
            private static void Running(UnitCommands __instance, UnitCommand __0)
            {
                var trace = active; if (trace == null || !ReferenceEquals(__instance, trace.owner.Commands) || !(__0 is UnitMoveTo)) return;
                var frames = new System.Diagnostics.StackTrace(1, false).GetFrames() ?? Array.Empty<System.Diagnostics.StackFrame>();
                var methods = frames.Select(frame => frame.GetMethod()).Where(method => method != null).ToArray();
                bool native = methods.Any(method => method.DeclaringType != null && method.DeclaringType != typeof(UnitCommands) &&
                    method.DeclaringType.Assembly == typeof(UnitCommands).Assembly);
                if (native) trace.NativeMoves.Add(__0);
                trace.MovementOrigins.Add(new JObject { ["type"] = __0.GetType().Name, ["nativeCaller"] = native,
                    ["callers"] = new JArray(methods.Take(12).Select(method => (method.DeclaringType?.FullName ?? "<dynamic>") + "." + method.Name)) });
            }
            public void Dispose()
            {
                if (ReferenceEquals(active, this)) active = null;
                var failures = new List<Exception>();
                foreach (var hook in new[] { callback, run }.Where(value => value != null)) {
                    try {
                        harmony.Unpatch(hook, Harmony12.HarmonyPatchType.All, id);
                        var info = harmony.GetPatchInfo(hook);
                        if (info != null && (info.Prefixes.Any(value => value.owner == id) || info.Postfixes.Any(value => value.owner == id)))
                            throw new InvalidOperationException("S30/31 lifetime observation hook remains.");
                    } catch (Exception error) { failures.Add(error); }
                }
                if (failures.Count != 0) throw new AggregateException("Lifetime observation teardown failed.", failures);
            }
        }

        private IEnumerable<int> Sprints3031HeldEscape(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, string key)
        {
            UnitEntityData holder = null;
            SummonGrabComponent grab = null;
            int strength = owner.Stats.Strength.BaseValue;
            var before = Sprints3031PersistenceState(owner, key);
            var events = new OutsiderEscapeEvents(owner);
            bool subscribed = false;
            try {
                holder = CastExpandedSummoningQuietUnit(fixture, "giant-crab", fixture.Hostile);
                Sprint28SetBrainActivePreservingActions(holder, false);
                holder.Commands.InterruptAll(true); holder.Commands.RemoveFinishedAndUpdateQueue();
                holder.Stats.BaseAttackBonus.BaseValue = 50; // Explicit disposable grab precondition, not a forced roll.
                PlaceExpandedSummoningUnit(holder, owner.Position + Vector3.forward);
                if (key == "trumpet-archon") owner.Stats.Strength.BaseValue = 6; // Select Escape Artist rather than its better adult CMB.
                grab = SummonGrabComponent.Find(holder);
                var limb = grab?.FirstGrabWeapon(holder);
                if (grab == null || limb == null || !grab.MultiLink)
                    throw new InvalidOperationException("Existing multi-hold fixture is unavailable.");
                var attacks = new JArray();
                Buff held = null;
                for (int attempt = 0; attempt < 5 && held == null; attempt++) {
                    var attack = Rulebook.Trigger(new RuleAttackWithWeapon(holder, owner, limb, 0));
                    held = owner.Buffs.Enumerable.FirstOrDefault(buff => ReferenceEquals(buff.Blueprint, grab.GrappledBuff) &&
                        ReferenceEquals(buff.Context?.MaybeCaster, holder));
                    attacks.Add(new JObject { ["roll"] = attack.AttackRoll?.Roll.Value, ["hit"] = attack.AttackRoll?.IsHit,
                        ["damage"] = owner.Damage, ["held"] = held != null, ["weapon"] = limb.Blueprint.AssetGuid });
                }
                bool ready = held != null && owner.Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                    holder.Buffs.GetBuff(grab.HoldBuff) != null && !owner.Descriptor.State.IsDead;
                if (!ready) throw new InvalidOperationException("Real owned attack did not establish held state: " + key + ";" + attacks);
                int mobilityBefore = owner.Stats.SkillMobility.ModifiedValue;
                EventBus.Subscribe(events); subscribed = true;
                // Existing native buff consumer, on the exact disposable held
                // owner. This is a held-round consumer, not a command/action claim.
                held.TickMechanics();
                EventBus.Unsubscribe(events); subscribed = false;
                bool released = !owner.Buffs.Enumerable.Contains(held) || held.IsDisposed;
                int expected = key == "dretch" ? 3 : 15; // Printed ranks plus live -4 Dex from native Entangled.
                var check = events.Skills.SingleOrDefault();
                bool exact = check != null && events.Maneuvers.Count == 0 && check.StatType == Kingmaker.EntitySystem.Stats.StatType.SkillMobility &&
                    check.BaseRollResult - check.D20.Value == expected && check.EnsureSuccess != true &&
                    released == check.IsPassed && JToken.DeepEquals(before, Sprints3031PersistenceState(owner, key));
                Sprint25Check(key + "-held-escape-consumer", exact, new JObject {
                    ["owner"] = owner.UniqueId, ["holder"] = holder.UniqueId, ["attacks"] = attacks,
                    ["nativeHeldBefore"] = ready, ["released"] = released, ["expectedHeldEscapeArtist"] = expected,
                    ["liveDexterity"] = owner.Stats.Dexterity.ModifiedValue, ["ordinaryMobilityBefore"] = mobilityBefore,
                    ["check"] = check == null ? null : OutsiderEscapeSkill(check), ["nativeHeldRoundTicks"] = 1,
                    ["forcedRolls"] = 0, ["actionClaim"] = false, ["resourcesBefore"] = before,
                    ["resourcesAfter"] = Sprints3031PersistenceState(owner, key) },
                    "A real attack establishes the actual held state; its native round consumer rolls attributed Escape Artist with live Entangled Dexterity, releases exactly on success, and leaves slots/resources/items unchanged.");
                if (!exact) throw new InvalidOperationException("Held Escape Artist consumer failed: " + key);
            }
            finally {
                if (subscribed) EventBus.Unsubscribe(events);
                if (holder != null && grab != null) SummonHoldComponent.ReleaseLink(holder, owner, grab, true);
                owner.Stats.Strength.BaseValue = strength;
                if (holder != null) {
                    holder.Commands.InterruptAll(true); holder.Commands.RemoveFinishedAndUpdateQueue();
                    if (holder.IsInCombat) holder.LeaveCombat();
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { holder });
                }
            }
            for (int frame = 0; frame < 16; frame++) yield return 0;
            bool clean = holder != null && holder.Destroyed && !holder.IsInState &&
                SummonHeldComponent.HolderOf(owner, grab.GrappledBuff) == null &&
                owner.Get<UnitPartGrappleTarget>() == null && !owner.Descriptor.State.HasCondition(UnitCondition.CantMove);
            Sprint25Check(key + "-held-reset", clean, new JObject { ["holderGone"] = holder?.Destroyed,
                ["ownerFree"] = !owner.Descriptor.State.HasCondition(UnitCondition.CantMove), ["strengthRestored"] = owner.Stats.Strength.BaseValue == strength },
                "Exact disposable hold ownership is gone before the subsequent native turn case.");
            if (!clean) throw new InvalidOperationException("Held fixture reset failed; turn case is unrun.");
        }

        private void ObserveSprints3031Disarm(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, UnitEntityData target, string key)
        {
            var inventory = owner.Descriptor.Inventory.Items.ToArray(); var counts = inventory.Select(value => value.Count).ToArray();
            var sourceItems = target.Descriptor.Inventory.Items.ToArray(); var sourceCounts = sourceItems.Select(value => value.Count).ToArray();
            var party = Game.Instance.Player.Inventory.Items.ToArray(); var partyCounts = party.Select(value => value.Count).ToArray();
            var weapon = owner.Body.PrimaryHand.MaybeWeapon;
            if (weapon == null || !ReferenceEquals(weapon.Owner, owner.Descriptor) || weapon.IsLootable)
                throw new InvalidOperationException("Celestial disarm ownership precondition failed.");
            int bab = target.Stats.BaseAttackBonus.BaseValue; var attempts = new JArray(); bool success = false;
            try {
                target.Stats.BaseAttackBonus.BaseValue = 50;
                for (int attempt = 0; attempt < 5 && !success; attempt++) {
                    var rule = Rulebook.Trigger(new RuleCombatManeuver(target, owner, CombatManeuver.Disarm));
                    success = rule.Success; attempts.Add(new JObject { ["success"] = success, ["partial"] = rule.IsPartialSuccess });
                }
            } finally { target.Stats.BaseAttackBonus.BaseValue = bab; }
            bool exact = success && owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff) != null &&
                Sprint28InventoryExact(owner.Descriptor.Inventory, inventory, counts) && inventory.All(value => ReferenceEquals(value.Owner, owner.Descriptor) && !value.IsLootable) &&
                Sprint28InventoryExact(target.Descriptor.Inventory, sourceItems, sourceCounts) && Sprint28PartyInventoryExact(party, partyCounts);
            Sprint25Check(key + "-disarm-owned", exact, new JObject { ["owner"] = owner.UniqueId, ["source"] = target.UniqueId,
                ["weapon"] = weapon.Blueprint.AssetGuid, ["attempts"] = attempts, ["forcedRolls"] = 0,
                ["ownedInventoryExact"] = Sprint28InventoryExact(owner.Descriptor.Inventory, inventory, counts),
                ["nativeMainHandDisabled"] = owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff) != null,
                ["items"] = new JArray(inventory.Select(value => new JObject { ["guid"] = value.Blueprint.AssetGuid, ["lootable"] = value.IsLootable })) },
                "Real native Disarm disables the exact summon weapon without transfer, duplication or loot exposure; source and party inventories remain exact.");
            if (!exact) throw new InvalidOperationException("Celestial disarm ownership failed: " + key);
            // Reset only the just-observed condition on this disposable owner;
            // no item is replaced. The following cancellation is a separately
            // available action, not a command already invalidated by Disarm.
            owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff).Remove();
        }

        private void ObserveSprints3031BlindFight(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, UnitEntityData target)
        {
            var blur = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == "BlurBuff");
            var transient = new List<UnityEngine.Object>();
            try {
                var applied = ApplySprint12Buff(target, blur, target, TimeSpan.FromSeconds(30), 10);
                if (applied.AppliedBuff == null) throw new InvalidOperationException("Native Blur fixture did not apply.");
                var weapon = owner.Body.PrimaryHand.MaybeWeapon;
                var attack = Rulebook.Trigger(new RuleAttackWithWeapon(owner, target, weapon, 0));
                var conceal = attack.AttackRoll?.ConcealmentCheck;
                bool attackExact = conceal != null && conceal.Concealment != Concealment.None && conceal.RerollIfFailed &&
                    !string.IsNullOrEmpty(conceal.RerollSource) && attack.AttackRoll.Weapon == weapon;
                owner.Descriptor.State.AddCondition(UnitCondition.Blindness, null);
                var melee = Rulebook.Trigger(new RuleCalculateAC(target, owner, AttackType.Melee));
                var ranged = Rulebook.Trigger(new RuleCalculateAC(target, owner, AttackType.Ranged));
                owner.Descriptor.State.RemoveCondition(UnitCondition.Blindness);
                var gaze = TraitCondition("S3031-Trumpet-Gaze-Control", SpellDescriptor.GazeAttack, transient);
                var effect = TraitEffect("S3031-Trumpet-Gaze-Control", SpellDescriptor.GazeAttack, transient);
                effect.Type = AbilityType.Supernatural;
                var control = ApplySprint12Buff(target, gaze, target, TimeSpan.FromSeconds(6), 10);
                var victim = ApplySprint12Buff(owner, gaze, target, TimeSpan.FromSeconds(6), 10);
                var resistance = Astral31Resistance(target, owner, effect, 1, 1);
                bool gazeExact = control.AppliedBuff != null && victim.AppliedBuff != null && !victim.Immunity && !resistance.TargetIsImmune;
                bool exact = attackExact && melee.IgnoreBlindnessPenalty && !ranged.IgnoreBlindnessPenalty && gazeExact;
                Sprint25Check("trumpet-blind-fight-consumers", exact, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["weapon"] = weapon.Blueprint.AssetGuid,
                    ["concealment"] = conceal?.Concealment.ToString(), ["concealmentRoll"] = conceal?.Roll.Value,
                    ["rerollIfFailed"] = conceal?.RerollIfFailed, ["rerollSource"] = conceal?.RerollSource,
                    ["concealmentSuccess"] = conceal?.Success, ["hit"] = attack.AttackRoll?.IsHit,
                    ["meleeIgnoresBlindnessPenalty"] = melee.IgnoreBlindnessPenalty,
                    ["rangedIgnoresBlindnessPenalty"] = ranged.IgnoreBlindnessPenalty,
                    ["gazeApplied"] = victim.AppliedBuff != null, ["gazeControlApplied"] = control.AppliedBuff != null,
                    ["gazeEffect"] = Astral31ResistanceRow(resistance), ["forcedRolls"] = 0 },
                    "The real melee attack consumes native concealment with the private feat reroll; blindness AC compensation is melee-only, and actual gaze descriptor buff/ability consumers remain eligible.");
                if (!exact) throw new InvalidOperationException("Trumpet Blind-Fight native consumers failed.");
            } finally {
                RemoveSprint12Buff(target, blur);
                owner.Descriptor.State.RemoveCondition(UnitCondition.Blindness);
                foreach (var buff in transient.OfType<BlueprintBuff>()) { RemoveSprint12Buff(owner, buff); RemoveSprint12Buff(target, buff); }
                foreach (var value in transient) UnityEngine.Object.Destroy(value);
            }
        }

        private static UnitCommand Sprints3031FutureCommand(UnitEntityData owner, UnitEntityData target, string key)
        {
            string symbol = key == "trumpet-archon" ? ExpandedSummoningSpecialBuilder.Trumpet31Prefix + "TrumpetBlast" :
                key == "astral-deva" ? ExpandedSummoningSpecialBuilder.Astral31Prefix + "BladeBarrier" : null;
            if (key == "lantern-archon") return new UnitUseAbility(new AbilityData(owner.Descriptor.Abilities.Enumerable.Single(value =>
                value.Blueprint.AssetGuid == Lantern31StandardRay)), new TargetWrapper(target));
            if (symbol != null) {
                var blueprint = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[symbol];
                return new UnitUseAbility(new AbilityData(owner.Descriptor.Abilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, blueprint))),
                    new TargetWrapper(key == "trumpet-archon" ? owner : target));
            }
            return new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
        }
    }
}
