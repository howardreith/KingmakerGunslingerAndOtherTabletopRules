using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Explosions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<object> _firearmInputSteps;
        private readonly List<RuntimeTestAssertion> _firearmInputAssertions = new List<RuntimeTestAssertion>();
        private readonly JArray _firearmInputRows = new JArray();
        private string _firearmInputStage = "not-started";
        private FirearmInputFixture _firearmInputFixture;
        private FirearmInputSaveGuard _firearmInputSaveGuard;

        private void PollFirearmNativeInput()
        {
            if (ResourcesLibrary.Preloading || !BlueprintBootstrap.IsInitialized) return;
            try
            {
                if (_firearmInputSteps == null)
                {
                    _manualElapsed = Stopwatch.StartNew();
                    _firearmInputSaveGuard = new FirearmInputSaveGuard(_request.RunId);
                    _firearmInputSteps = FirearmNativeInputCases().GetEnumerator();
                }
                if (_firearmInputSteps.MoveNext()) return;
                FinishFirearmInput(null);
            }
            catch (Exception exception) { FinishFirearmInput(exception); }
        }
        private void FinishFirearmInput(Exception failure)
        {
            try { StopFirearmInput(); }
            catch (Exception cleanup) { failure = new AggregateException(failure ?? cleanup, cleanup); }
            var result = CreateResult(failure == null && _firearmInputAssertions.Count != 0 &&
                _firearmInputAssertions.All(a => a.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _firearmInputAssertions,
                failure == null ? null : _firearmInputStage + ": " + failure);
            result.EvidenceFiles.Add(WriteFirearmInputEvidence(failure));
            Complete(result);
        }
        private void StopFirearmInput()
        {
            try
            {
                // Dispose request-local observation scopes if a timeout stops
                // the iterator between yields, before disposing its actors.
                if (_firearmInputSteps != null) { _firearmInputSteps.Dispose(); _firearmInputSteps = null; }
                if (_firearmInputFixture != null)
                {
                    _firearmInputFixture.Dispose();
                    FirearmInputCheck("fixture-restored", _firearmInputFixture.Restored,
                        "Exact original native world, inventory, clock and controller ownership restored.");
                    _firearmInputFixture = null;
                }
            }
            finally
            {
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                if (_firearmInputSaveGuard != null)
                {
                    var guard = _firearmInputSaveGuard;
                    _firearmInputSaveGuard = null;
                    guard.Dispose();
                    FirearmInputCheck("no-save-mutation", guard.Blocked == 0,
                        "Blocked native mutation attempts=" + guard.Blocked);
                }
            }
        }
        private string WriteFirearmInputEvidence(Exception failure)
        {
            string path = Path.Combine(_request.EvidenceDirectory, "firearm-native-input.json");
            RuntimeTestResultWriter.WriteAtomic(path, new JObject {
                ["runId"] = _request.RunId, ["stage"] = _firearmInputStage,
                ["failure"] = failure == null ? null : failure.ToString(),
                ["boundary"] = "Native ClickUnitHandler.OnClick / native commands and ability execution / native weapon rules. Request-local scene and animation cues only.",
                ["saveBacked"] = false, ["humanVisualAcceptance"] = "NOT RUN",
                ["rows"] = _firearmInputRows }.ToString(Formatting.Indented));
            return path;
        }
        private void FirearmInputCheck(string name, bool passed, string detail)
        {
            _firearmInputAssertions.Add(Assertion(name, "true", detail, passed,
                "Guarded native input/command fixture; no authorization bridge or suppression reset."));
            _firearmInputRows.Add(new JObject { ["check"] = name, ["passed"] = passed, ["detail"] = detail });
            WriteFirearmInputEvidence(null);
            if (!passed) throw new InvalidOperationException(name + ": " + detail);
        }
        private IEnumerable<object> FirearmNativeInputCases()
        {
            foreach (object step in FirearmReviewMergeCases()) yield return step;
            foreach (object step in FirearmReviewHoverCases()) yield return step;
            foreach (object step in FirearmReviewReloadTurnCases()) yield return step;
            foreach (object step in FirearmReviewRetargetCases()) yield return step;
            foreach (object step in FirearmReviewCoexistenceCases()) yield return step;
            foreach (object step in FirearmReviewOwnershipCases()) yield return step;
            _firearmInputStage = "native-repair-warning-matrix";
            _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows, false);
            CheckFirearmRepairWarnings(_firearmInputFixture);
            _firearmInputFixture.Dispose();
            FirearmInputCheck("repair-fixture-restored", _firearmInputFixture.Restored,
                "Readiness-only condition fixtures restored; no degradation proof uses assigned Wrecked state.");
            _firearmInputFixture = null;
            foreach (bool controllerCursor in new[] { false, true })
            foreach (bool turnBased in new[] { false, true })
            {
                string prefix = (controllerCursor ? "controller-cursor-" : "desktop-") + (turnBased ? "tb-" : "rtwp-");
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(turnBased, _firearmInputRows);
                var f = _firearmInputFixture;
                if (controllerCursor) f.EnableController(true);
                CheckRepairWarning(f, f.Actor, "Cannot repair firearms during combat.", prefix + "combat-repair");
                yield return null;
                _firearmInputStage = prefix + "native-initial-misfire";
                // An automatically constructed but not yet submitted command
                // must retain its pre-break generation even after a new order.
                UnitCommand staleAttack = UnitAttack.CreateAttackCommand(f.Actor, f.Enemy);
                FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
                f.Click(f.Enemy);
                for (int tick = 0; tick < 200 && f.Shots.Count == 0; tick++)
                { f.Pump(); f.Record(tick); yield return null; }
                FirearmInputCheck(prefix + "real-break", f.Shots.Count == 1 &&
                    f.State.Condition == FirearmCondition.Broken && f.State.IsEmpty &&
                    BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon), f.Describe());
                int powder = f.Powder;
                float aiBefore = f.Actor.CombatState.AIData.NextCommandTime;
                foreach (object step in DriveFirearmIdle(f)) yield return step;
                FirearmInputCheck(prefix + "native-ai-retry-window", turnBased ||
                    f.Actor.CombatState.AIData.NextCommandTime > aiBefore,
                    "nativeNextCommandTimeBefore=" + aiBefore + ";after=" + f.Actor.CombatState.AIData.NextCommandTime);
                FirearmInputCheck(prefix + "old-sequence-stopped", f.Shots.Count == 1 &&
                    f.State.IsEmpty && f.Powder == powder, f.Describe());
                CheckRepairWarning(f, f.Actor, "Cannot repair firearms during combat.", prefix + "combat-repair-after-action");
                FirearmInputCheck(prefix + "automatic-create-rejected",
                    UnitAttack.CreateAttackCommand(f.Actor, f.Enemy) == null, f.Describe());
                if (turnBased) { f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn(); }
                CheckBrokenInputNegatives(f, prefix);
                UnitCommand staleReload = f.CreateNativeAiCommand(f.Enemy);
                FirearmInputCheck(prefix + "stale-ai-reload-created", staleReload is UnitUseAbility &&
                    NativeFirearmAttackOrder.Get(staleReload)?.Order == null,
                    "Actual BlueprintAiAttack factory captured a suppressed reload before new input.");
                _firearmInputStage = prefix + "reload-first-new-click";
                FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                if (!turnBased) f.Turns.SetFirearmPaused(true);
                f.Click(f.Enemy);
                UnitUseAbility reload = f.Actor.Commands.Raw.OfType<UnitUseAbility>().FirstOrDefault();
                var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                FirearmInputCheck(prefix + "native-reload-submitted", reload != null && order != null &&
                    ReferenceEquals(order.Target, f.Enemy) && ReferenceEquals(order.Weapon, f.Weapon) &&
                    ReferenceEquals(reload.Spell.Blueprint, BlueprintBootstrap.ReloadTestMusketAbility), f.Describe());
                f.Turns.Execute(() => f.Actor.Commands.Run(staleAttack));
                FirearmInputCheck(prefix + "old-created-command-rejected", staleAttack != null &&
                    !f.Actor.Commands.Raw.Contains(staleAttack) && !f.Actor.Commands.Queue.Contains(staleAttack) &&
                    BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) && f.Actor.Commands.Raw.Contains(reload),
                    "Old factory command cannot borrow the newly accepted reload order.");
                f.Turns.Execute(() => f.Actor.Commands.Run(staleReload));
                FirearmInputCheck(prefix + "stale-ai-reload-rejected",
                    !f.Actor.Commands.Raw.Contains(staleReload) && !f.Actor.Commands.Queue.Contains(staleReload) &&
                    BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) && f.Actor.Commands.Raw.Contains(reload),
                    "An old native AI reload cannot borrow the newly accepted order.");
                f.WatchActionCost(reload);
                long resumedBefore = EmptyFirearmAttackCommandPatch.AutoReloadResumedAttacks;
                int acceptedFrame = Time.frameCount;
                for (int frame = 0; frame < 3; frame++) yield return null;
                FirearmInputCheck(prefix + "order-survives-later-frames", Time.frameCount > acceptedFrame &&
                    BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) && f.Shots.Count == 1 &&
                    (turnBased || (Game.Instance.IsPaused && f.LastClickPaused)),
                    "acceptedFrame=" + acceptedFrame + ";now=" + Time.frameCount + ";" + f.Describe());
                if (!turnBased) f.Turns.SetFirearmPaused(false);
                for (int tick = 0; tick < 240 && f.Shots.Count < 2; tick++)
                { f.Pump(); f.Record(tick); yield return null; }
                FirearmInputCheck(prefix + "reload-then-shot", f.Shots.Count == 2 &&
                    f.State.Condition == FirearmCondition.Broken && f.Powder == powder - 1 &&
                    !reload.Cutscene && !reload.IsIgnoreCooldown && reload.IsActed &&
                    f.PeakActionCost(reload) > 0 &&
                    ReferenceEquals(NativeFirearmAttackOrder.Get(f.LastShotCommand)?.Order, order) &&
                    (EmptyFirearmAttackCommandPatch.AutoReloadResumedAttacks > resumedBefore ||
                        (!turnBased && f.LastShotCommand.AiAction != null &&
                         f.LastShotCommand.AiAction.GetType().FullName == "Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAttack")),
                    f.Describe() + ";reload=" + reload.Result + ";cutscene=" + reload.Cutscene +
                    ";ignoreCooldown=" + reload.IsIgnoreCooldown + ";action=" + reload.Type +
                    ";nativeCooldownPeak=" + f.PeakActionCost(reload) +
                    ";nativeReloadCallbacks=" + (EmptyFirearmAttackCommandPatch.AutoReloadResumedAttacks - resumedBefore) +
                    ";nativeAiContinuation=" + (f.LastShotCommand != null && f.LastShotCommand.AiAction != null) +
                    ";sameAcceptedOrder=" + ReferenceEquals(NativeFirearmAttackOrder.Get(f.LastShotCommand)?.Order, order));
                _firearmInputStage = prefix + "new-sequence-to-wrecked";
                long burst = FirearmExplosionRuntimeDiagnostics.Applied;
                FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
                // Let the accepted new order continue through ordinary processing.
                float continuationDeadline = Time.realtimeSinceStartup + 30f;
                for (int tick = 0; Time.realtimeSinceStartup < continuationDeadline &&
                    f.State.Condition != FirearmCondition.Wrecked; tick++)
                {
                    if (turnBased && tick == 60)
                    {
                        f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn();
                        // A later turn still requires the player's ordinary order.
                        f.Click(f.Enemy);
                    }
                    f.Pump(); f.Record(tick); yield return null;
                }
                for (int tick = 0; tick < 8; tick++) { f.Pump(); yield return null; }
                FirearmInputCheck(prefix + "natural-wrecked-once", f.State.Condition == FirearmCondition.Wrecked &&
                    FirearmExplosionRuntimeDiagnostics.Applied == burst + 1,
                    f.Describe() + ";bursts=" + (FirearmExplosionRuntimeDiagnostics.Applied - burst));
                int shots = f.Shots.Count; powder = f.Powder;
                f.Click(f.Enemy);
                for (int tick = 0; tick < 20; tick++) { f.Pump(); yield return null; }
                FirearmInputCheck(prefix + "wrecked-rejects-fire-reload", f.Shots.Count == shots &&
                    f.Powder == powder && !f.Actor.AutoUseAbility.IsAvailable, f.Describe());
                if (!turnBased && !controllerCursor)
                    CheckFirearmCompletedRest(f);
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
            foreach (object step in FirearmManualRecoveryCases()) yield return step;
            foreach (object step in FirearmAdditionalInputCases()) yield return step;
        }

        private sealed class FirearmInputFixture : IDisposable,
            IGlobalRulebookHandler<RuleAttackWithWeapon>, IUnitRunCommandHandler
        {
            private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            internal readonly List<RuleAttackWithWeapon> Shots = new List<RuleAttackWithWeapon>();
            internal UnitAttack LastShotCommand;
            private readonly Dictionary<UnitUseAbility, float> actionCosts = new Dictionary<UnitUseAbility, float>();
            internal UnitEntityData Actor, Enemy;
            internal ItemEntityWeapon Weapon;
            internal ElementalNativeTurnScope Turns;
            private FirearmControllerInputScope controller;
            private FirearmStateRepositorySnapshot originalIdentity;
            private readonly JArray rows;
            private readonly ElementalUndineFeatScenario.PortalHarness scene;
            private readonly List<string> diagnostics = new List<string>();
            private readonly List<UnitEntityData> extraUnits = new List<UnitEntityData>();
            private readonly BlueprintFaction hostile;
            private BlueprintFaction peaceful;
            private readonly ItemEntity[] inventoryBefore;
            private readonly int[] countsBefore;
            private readonly UnitEntityData[] worldBefore;
            private readonly UnityEngine.Random.State randomBefore;
            private readonly UnitHandEquipmentController handsBefore;
            private readonly PropertyInfo handsProperty;
            private bool disposed;
            internal bool Restored;
            internal FirearmState State { get { return FirearmRuntimeState.Service.GetOrCreate(Weapon).Repository.State; } }
            internal int Powder { get { return Game.Instance.Player.Inventory.Count(BlueprintBootstrap.BasicAmmunition.BlackPowder); } }
            internal FirearmInputFixture(bool turnBased, JArray evidence, bool combat = true,
                FirearmKind kind = FirearmKind.Pistol, bool rapidReload = true, bool initiallyLoaded = true)
            {
                rows = evidence;
                if (Game.Instance.State.Units.All.Any() || Game.Instance.Player.Party.Any() ||
                    Game.Instance.CurrentlyLoadedArea != null || Game.Instance.ProjectileController.Projectiles.Any() ||
                    Game.Instance.State.AwakeUnits.Any() || Game.Instance.AwakeUnitGroups.Any())
                    throw new InvalidOperationException("The firearm scenario requires an empty save-free world.");
                worldBefore = Game.Instance.State.Units.All.ToArray();
                inventoryBefore = Game.Instance.Player.Inventory.Items.ToArray();
                countsBefore = inventoryBefore.Select(item => item.Count).ToArray();
                randomBefore = UnityEngine.Random.state;
                handsBefore = Game.Instance.HandsEquipmentController;
                handsProperty = typeof(Game).GetProperty("HandsEquipmentController", Members);
                if (handsBefore == null) handsProperty.SetValue(Game.Instance, new UnitHandEquipmentController(), null);
                scene = new ElementalUndineFeatScenario.PortalHarness(diagnostics);
                try
                {
                    var human = BlueprintRoot.Instance.Progression.CharacterRaces.Single(race =>
                        race.AssetGuid == "0a5d473ead98b0646b94495af250fdc4");
                    Actor = scene.Initialize(human);
                    if (combat)
                    {
                        // Restore the real default player brain omitted by the
                        // general save-free scene. AI must genuinely process
                        // the old manual target for stale-order qualification.
                        Actor.Blueprint.Brain = BlueprintRoot.Instance.DefaultPlayerCharacter.Brain;
                        if (Actor.Blueprint.Brain == null)
                            throw new InvalidOperationException("The native default player brain is unavailable.");
                        typeof(UnitDescriptor).GetProperty("Brain", Members).SetValue(Actor.Descriptor,
                            new UnitBrain(Actor.Descriptor), null);
                        Actor.Brain.RestoreAvailableActions();
                        if (!Actor.Brain.AvailableActions.Any(action => action.Blueprint != null &&
                            action.Blueprint.GetType().FullName == "Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAttack"))
                            throw new InvalidOperationException("The native player brain has no attack action.");
                    }
                    hostile = UnityEngine.Object.Instantiate(Actor.Blueprint.Faction);
                    hostile.name = "KMG_Runtime_FirearmInput_Hostile";
                    hostile.Peaceful = hostile.AlwaysEnemy = hostile.Neutral = hostile.IsDirectlyControllable = false;
                    hostile.Dummy = null; hostile.AttackFactions = new[] { Actor.Blueprint.Faction };
                    Enemy = scene.SpawnFixtureUnit(human, combat ? hostile : Actor.Blueprint.Faction, new Vector3(0, 0, 2), "FirearmInputTarget");
                    Actor.Memory.Add(Enemy); Enemy.Memory.Add(Actor);
                    Actor.Stats.HitPoints.BaseValue = Enemy.Stats.HitPoints.BaseValue = 10000;
                    Actor.Stats.BaseAttackBonus.BaseValue = 11;
                    Actor.Descriptor.AddFact(BlueprintBootstrap.FirearmProficiency);
                    if (rapidReload) Actor.Descriptor.AddFact(BlueprintBootstrap.FirearmFeats.RapidReloadChoices[
                        kind == FirearmKind.Pistol ? 0 : kind == FirearmKind.Musket ? 1 : 2]);
                    Actor.Descriptor.AddFact(BlueprintBootstrap.GunslingerClass.Gunsmithing);
                    Weapon = new ItemEntityWeapon(BlueprintBootstrap.ProductionFirearms.Entries.Single(entry =>
                        entry.Spec.Definition.Kind == kind).Item);
                    Actor.Body.PrimaryHand.InsertItem(Weapon);
                    FirearmRuntimeState.Service.Set(Weapon, new FirearmState(FirearmState.CurrentSchemaVersion,
                        initiallyLoaded ? 1 : 0, initiallyLoaded ? FirearmStateTokenCatalog.DiagnosticLeadBall : null, FirearmCondition.Normal));
                    originalIdentity = FirearmRuntimeState.Service.GetOrCreate(Weapon).Repository;
                    Game.Instance.Player.Inventory.Add(BlueprintBootstrap.BasicAmmunition.BlackPowder, 12);
                    Game.Instance.Player.Inventory.Add(BlueprintBootstrap.BasicAmmunition.LeadBall, 12);
                    Game.Instance.Player.Inventory.Add(BlueprintBootstrap.GunsmithingSupplies.GunsmithKit, 1);
                    Actor.AutoUseAbility = Actor.Descriptor.Abilities.GetAbility(BlueprintBootstrap.ReloadTestMusketAbility)?.Data;
                    if (Actor.AutoUseAbility == null) throw new InvalidOperationException("The native reload fact is missing.");
                    if (combat) Turns = new ElementalNativeTurnScope(Actor, Enemy, rows,
                        turnBased ? "firearm-tb-" : "firearm-rtwp-", turnBased);
                    EventBus.Subscribe(this);
                }
                catch { Dispose(); throw; }
            }
            internal bool SameItemIdentity
            {
                get {
                    var identity = FirearmRuntimeState.Service.GetOrCreate(Weapon).Repository;
                    return ReferenceEquals(Actor.Body.PrimaryHand.MaybeWeapon, Weapon) &&
                        identity.EntryId == originalIdentity.EntryId &&
                        identity.RuntimeReferenceHash == originalIdentity.RuntimeReferenceHash;
                }
            }
            internal bool LastClickPaused;
            internal void EnableController(bool cursorMode = false) { controller = new FirearmControllerInputScope(Actor, cursorMode); }
            internal void DisableController() { if (controller != null) { controller.Dispose(); controller = null; } }
            internal void MakeEnemyPeaceful(bool value)
            {
                if (peaceful == null)
                {
                    peaceful = UnityEngine.Object.Instantiate(Actor.Blueprint.Faction);
                    peaceful.name = "KMG_Runtime_Firearm_PeacefulInteraction";
                    peaceful.IsDirectlyControllable = peaceful.AlwaysEnemy = false;
                    // Native CanAttack explicitly permits every Neutral target.
                    peaceful.Neutral = false; peaceful.Peaceful = true;
                    peaceful.AttackFactions = new BlueprintFaction[0];
                }
                Turns.Execute(() => {
                    Enemy.Descriptor.SwitchFactions(value ? peaceful : hostile, true);
                    // Match(empty) clears faction membership without rebuilding
                    // the existing native group's cached attack factions.
                    Enemy.Group.UpdateAttackFactionsCache();
                    // Reclassify the two owned memory entries too: the native
                    // join controller consumes a separately cached enemy list.
                    Actor.Memory.Remove(Enemy); Actor.Memory.Add(Enemy);
                    Enemy.Memory.Remove(Actor); Enemy.Memory.Add(Actor);
                });
            }
            internal void ControllerInteract(UnitEntityData target)
            { Turns.Execute(() => controller.Interact(target)); }
            internal UnitCommand CreateNativeAiCommand(UnitEntityData target)
            {
                var action = Actor.Brain.AvailableActions.First(value => value.Blueprint != null &&
                    value.Blueprint.GetType().FullName == "Kingmaker.Controllers.Brain.Blueprints.BlueprintAiAttack");
                UnitCommand command = null;
                Turns.Execute(() => command = (UnitCommand)action.Blueprint.GetType().GetMethod("CreateCommand", Members)
                    .Invoke(action.Blueprint, new object[] { new Kingmaker.Controllers.Brain.DecisionContext { Unit = Actor }, target }));
                if (command != null) command.AiAction = action.Blueprint;
                return command;
            }
            internal UnitEntityData SpawnAdditional(bool friendly, string name)
            {
                var unit = scene.SpawnFixtureUnit(Actor.Descriptor.Progression.Race,
                    friendly ? Actor.Blueprint.Faction : hostile, new Vector3(1, 0, 2), name);
                unit.Stats.HitPoints.BaseValue = 10000;
                unit.IsInGame = true; unit.IsInFogOfWar = false;
                unit.View.SetVisible(true, true);
                Actor.Memory.Add(unit); unit.Memory.Add(Actor);
                extraUnits.Add(unit);
                return unit;
            }
            internal void Click(UnitEntityData target, bool simulate = false)
            {
                bool pause = Game.Instance.IsPaused;
                Turns.Execute(() => {
                    LastClickPaused = Game.Instance.IsPaused;
                    if (LastClickPaused != pause) throw new InvalidOperationException("Input lost native pause context.");
                    new Kingmaker.Controllers.Clicks.Handlers.ClickUnitHandler().OnClick(
                        target.View.gameObject, target.Position, 0, simulate, false);
                });
                rows.Add(new JObject { ["nativeClick"] = target.UniqueId, ["frame"] = Time.frameCount,
                    ["paused"] = pause, ["simulated"] = simulate, ["state"] = Describe() });
            }
            internal void WatchActionCost(UnitUseAbility command) { actionCosts[command] = 0; }
            internal float PeakActionCost(UnitUseAbility command) { return actionCosts[command]; }
            internal void Pump()
            {
                Turns.PumpCommands();
                foreach (var command in actionCosts.Keys.ToArray())
                {
                    // Swift/Free commands can end before their detached native
                    // ability process delivers. The save-free host has no
                    // default AbilityExecutionController; advance this exact
                    // watched process after it leaves the active command slots.
                    if (!Actor.Commands.Raw.Contains(command) && command.ExecutionProcess != null &&
                        !command.ExecutionProcess.IsEnded)
                        Turns.Execute(command.ExecutionProcess.Tick);
                    if (!command.IsActed) continue;
                    var cooldown = Actor.CombatState.Cooldown;
                    float charged = command.Type == UnitCommand.CommandType.Move ? cooldown.MoveAction :
                        command.Type == UnitCommand.CommandType.Swift ? cooldown.SwiftAction : cooldown.StandardAction;
                    actionCosts[command] = Math.Max(actionCosts[command], charged);
                }
            }
            internal UnitUseAbility ClickAbility(Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility blueprint, bool requireAccepted = true)
            {
                var fact = Actor.Descriptor.Abilities.GetAbility(blueprint);
                if (fact == null) throw new InvalidOperationException("The native action-bar ability is not granted.");
                using (var prediction = new FirearmSelfPredictionScope())
                    Turns.Execute(() => new Kingmaker.UI.UnitSettings.MechanicActionBarSlotAbility {
                        Unit = Actor, Ability = fact.Data }.OnClick());
                var command = Actor.Commands.Raw.Concat(Actor.Commands.Queue).OfType<UnitUseAbility>().SingleOrDefault(value =>
                    ReferenceEquals(value.Spell.Blueprint, blueprint));
                if (command == null)
                {
                    if (requireAccepted) throw new InvalidOperationException("The native action bar did not submit its exact ability.");
                    return null;
                }
                WatchActionCost(command);
                return command;
            }
            internal string Describe()
            {
                return "shots=" + Shots.Count + ";state=" + State + ";powder=" + Powder +
                    ";sameItemIdentity=" + SameItemIdentity +
                    ";suppressed=" + BrokenSequenceSuppressionRuntime.IsSuppressed(Actor, Weapon) +
                    ";commands=" + string.Join(",", Actor.Commands.Raw.Where(c => c != null).Select(c =>
                        c.GetType().Name + ":" + c.IsStarted + "/" + c.IsActed + "/" + c.Result)) +
                    ";queue=" + Actor.Commands.Queue.Count();
            }
            internal void Record(int tick)
            {
                if (tick >= 8 && tick % 20 != 0) return;
                var gates = new JArray();
                Turns.Execute(() => {
                    foreach (var c in Actor.Commands.Raw.Where(c => c != null))
                        gates.Add(new JObject { ["command"] = c.GetType().Name,
                            ["canStart"] = c.CanStart, ["enoughClose"] = c.IsUnitEnoughClose,
                            ["handsBusy"] = Actor.AreHandsBusyWithAnimation,
                            ["handsScheduled"] = Game.Instance.HandsEquipmentController.IsUpdateScheduledFor(Actor),
                            ["canAct"] = Actor.Descriptor.State.CanAct,
                            ["getUp"] = Actor.View.IsGetUp,
                            ["rigidbodyControlled"] = Actor.View.RigidbodyController != null && Actor.View.RigidbodyController.IsControllingRigidbody,
                            ["canActInCombat"] = Actor.CombatState.CanActInCombat,
                            ["prepared"] = Actor.CombatState.Prepared,
                            ["brainActive"] = Actor.IsBrainActive,
                            ["brainActions"] = Actor.Brain == null ? 0 : Actor.Brain.AvailableActions.Count,
                            ["awakeGroup"] = Game.Instance.AwakeUnitGroups.Contains(Actor.Group),
                            ["cooldown"] = Actor.CombatState.HasCooldownForCommand(c),
                            ["shouldStart"] = (bool)typeof(UnitActionController).GetMethod("ShouldStartCommand", Members)
                                .Invoke(new UnitActionController(), new object[] { c }),
                            ["fullAttack"] = c is UnitAttack && ((UnitAttack)c).IsFullAttack });
                });
                rows.Add(new JObject { ["pump"] = tick, ["state"] = Describe(), ["gates"] = gates,
                    ["unityTime"] = Time.time, ["realTime"] = Time.realtimeSinceStartup,
                    ["nextAiCommandTime"] = Actor.CombatState.AIData.NextCommandTime,
                    ["aiEnabled"] = Actor.IsAIEnabled,
                    ["gameLoading"] = Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingInProcess,
                    ["scheduledActions"] = ((System.Collections.ICollection)typeof(Game).GetField("m_BeforeTickActions", Members)
                        .GetValue(Game.Instance)).Count,
                    ["reloadResumed"] = EmptyFirearmAttackCommandPatch.AutoReloadResumedAttacks,
                    ["reloadCancelled"] = EmptyFirearmAttackCommandPatch.AutoReloadCanceledAttacks,
                    ["ledgerSubmission"] = BrokenSequenceSuppressionRuntime.Orders.Submission(Actor) });
            }
            public void HandleUnitRunCommand(UnitCommand command)
            {
                if (!ReferenceEquals(command.Executor, Actor)) return;
                var binding = NativeFirearmAttackOrder.Get(command);
                // Track detached native delivery for auto-use Free reloads too,
                // not only abilities clicked through the action bar. Preview
                // commands never execute and do not enter this delivery list.
                var cast = command as UnitUseAbility;
                if (cast != null && (binding == null || !binding.Preview)) WatchActionCost(cast);
                rows.Add(new JObject { ["nativeRun"] = command.GetType().Name,
                    ["ai"] = command.AiAction == null ? null : command.AiAction.GetType().Name,
                    ["bindingEpoch"] = binding == null ? -1 : binding.Epoch,
                    ["bindingSubmission"] = binding == null ? -1 : binding.Submission,
                    ["proposal"] = binding != null && binding.Proposal,
                    ["preview"] = binding != null && binding.Preview,
                    ["currentOrder"] = binding != null && BrokenSequenceSuppressionRuntime.Orders.IsCurrent(binding.Order),
                    ["ledgerSubmission"] = BrokenSequenceSuppressionRuntime.Orders.Submission(Actor),
                    ["state"] = Describe() });
            }
            public void OnEventAboutToTrigger(RuleAttackWithWeapon rule) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon rule)
            {
                if (!ReferenceEquals(rule.Initiator, Actor) || !ReferenceEquals(rule.Weapon, Weapon)) return;
                LastShotCommand = Actor.Commands.Raw.OfType<UnitAttack>().SingleOrDefault(command =>
                    ReferenceEquals(command.Target, rule.Target));
                if (LastShotCommand == null)
                    throw new InvalidOperationException("Native weapon shot has no owning attack command.");
                Shots.Add(rule);
                rows.Add(new JObject { ["shot"] = Shots.Count, ["target"] = rule.Target.UniqueId,
                    ["state"] = State.ToString(), ["frame"] = Time.frameCount,
                    ["nativeCommand"] = LastShotCommand.GetType().Name,
                    ["nativeAi"] = LastShotCommand.AiAction == null ? null : LastShotCommand.AiAction.GetType().Name,
                    ["acceptedOrder"] = BrokenSequenceSuppressionRuntime.Orders.IsCurrent(
                        NativeFirearmAttackOrder.Get(LastShotCommand)?.Order) });
            }
            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                EventBus.Unsubscribe(this);
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                try
                {
                    DisableController();
                    if (Turns != null) Turns.SetFirearmPaused(false);
                    foreach (var unit in extraUnits)
                    {
                        unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        if (unit.IsInCombat) unit.LeaveCombat();
                    }
                    Game.Instance.Player.UpdateIsInCombat();
                    if (Turns != null)
                    {
                        Turns.FlushFirearmCallbacks();
                        Turns.Dispose();
                    }
                    if (Weapon != null)
                    {
                        FirearmRuntimeState.Service.Forget(Weapon);
                        if (Actor != null && ReferenceEquals(Actor.Body.PrimaryHand.MaybeItem, Weapon))
                            Actor.Body.PrimaryHand.RemoveItem(false);
                    }
                    // Every projectile belongs to this empty-world fixture.
                    // Stop only those views after command teardown, never as
                    // evidence of a successful discharge or impact.
                    Game.Instance.ProjectileController.Clear();
                    scene.Dispose();
                    new Kingmaker.Controllers.SleepingUnitsController().Tick();
                    if (hostile != null) UnityEngine.Object.DestroyImmediate(hostile);
                    if (peaceful != null) UnityEngine.Object.DestroyImmediate(peaceful);
                    var inventory = Game.Instance.Player.Inventory;
                    foreach (var item in inventory.Items.Where(item => !inventoryBefore.Contains(item)).ToArray())
                        inventory.Remove(item, item.Count);
                    Restored = inventoryBefore.SequenceEqual(inventory.Items) &&
                        countsBefore.SequenceEqual(inventory.Items.Select(item => item.Count)) &&
                        worldBefore.SequenceEqual(Game.Instance.State.Units.All) &&
                        !Game.Instance.State.AwakeUnits.Any() && !Game.Instance.AwakeUnitGroups.Any() &&
                        !Game.Instance.ProjectileController.Projectiles.Any() &&
                        scene.PlayerContextRestored && scene.AreaContextRestored && (Turns == null || Turns.Restored);
                    if (!Restored) throw new InvalidOperationException("Firearm fixture native restoration is not exact.");
                }
                finally
                {
                    UnityEngine.Random.state = randomBefore;
                    if (handsBefore == null) handsProperty.SetValue(Game.Instance, null, null);
                }
            }
        }
        private sealed class FirearmInputSaveGuard : IDisposable
        {
            private static FirearmInputSaveGuard active;
            private readonly HarmonyInstance harmony;
            private readonly string id;
            private readonly List<MethodInfo> methods = new List<MethodInfo>();
            internal int Blocked;
            internal FirearmInputSaveGuard(string runId)
            {
                if (active != null) throw new InvalidOperationException("A firearm save guard is already active.");
                active = this; id = "KMG.FirearmNativeInput." + runId;
                harmony = HarmonyInstance.Create(id);
                try
                {
                    foreach (var method in typeof(Kingmaker.EntitySystem.Persistence.SaveManager).GetMethods(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m =>
                        m.Name == "SaveRoutine" || m.Name == "SaveStashedArea" || m.Name == "DeleteSave" || m.Name == "RemoveSaveFromList"))
                    {
                        harmony.Patch(method, new HarmonyMethod(typeof(FirearmInputSaveGuard).GetMethod("Block",
                            BindingFlags.Static | BindingFlags.NonPublic)));
                        methods.Add(method);
                    }
                    if (methods.Count != 5) throw new InvalidOperationException("Native save mutation contract changed.");
                }
                catch { Dispose(); throw; }
            }
            private static void Block()
            {
                if (active == null) return;
                active.Blocked++;
                throw new InvalidOperationException("Native save mutation blocked during firearm input qualification.");
            }
            public void Dispose()
            {
                foreach (var method in methods) harmony.Unpatch(method, HarmonyPatchType.All, id);
                methods.Clear();
                if (ReferenceEquals(active, this)) active = null;
            }
        }
    }
}
