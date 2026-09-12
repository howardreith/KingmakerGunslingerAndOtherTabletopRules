using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UI.Selection;
using UnityEngine;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using TurnBased.Controllers;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Uses the existing PortalHarness for request-owned scene/area/party actors.
    // This scope owns only native turn activation and synchronous controller ticks.
    internal sealed class ElementalNativeTurnScope : IDisposable
    {
        [ThreadStatic] private static ElementalNativeTurnScope Active;
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly UnitEntityData _caster, _enemy;
        private readonly bool _turnBasedBefore, _pausedBefore, _scrollBefore;
        private readonly TimeSpan _timeBefore;
        private readonly bool[] _avoidanceBefore;
        private readonly PropertyInfo _cameraProperty;
        private readonly CameraController _cameraBefore;
        private readonly PropertyInfo _handsProperty = typeof(Game).GetProperty("HandsEquipmentController", Members);
        private readonly UnitHandEquipmentController _ownedHands;
        private readonly SelectionManager _selectionBefore;
        private readonly PropertyInfo _selectionProperty;
        private GameObject _selectionObject;
        private SelectionManager _selection;
        private readonly GameMode _tickMode = new GameMode(GameModeType.Default, new IController[0]);
        private readonly GameMode _firearmPauseMode = new GameMode(GameModeType.Pause, new IController[0]);
        private GameMode[] _beforeFirearmPause;
        private bool _firearmPaused;
        private readonly UnitActionController _actions = new UnitActionController();
        private readonly UnitCombatJoinController _join = new UnitCombatJoinController();
        private readonly UnitCombatPrepareController _prepare = new UnitCombatPrepareController();
        private readonly OwnedCooldowns _cooldowns = new OwnedCooldowns();
        private readonly OwnedBuffs _buffs = new OwnedBuffs();
        private readonly HashSet<TurnController> _prepared = new HashSet<TurnController>();
        private bool _subscribed, _entered, _disposed, _navigationObserved;
        private int _commandTicks;
        internal bool Restored { get; private set; }
        internal readonly JArray Turns = new JArray();

        internal ElementalNativeTurnScope(UnitEntityData caster, UnitEntityData enemy, JArray observations, string label, bool turnBased = true)
        {
            _caster = caster; _enemy = enemy;
            _ownedHands = Game.Instance.HandsEquipmentController ?? new UnitHandEquipmentController();
            observations.Add(new JObject { ["name"] = label + "native-turn-state", ["turns"] = Turns });
            if (Active != null || caster == null || enemy == null || ReferenceEquals(caster, enemy) ||
                Game.Instance.Player.IsInCombat || CombatController.IsInTurnBasedCombat() ||
                !caster.IsInState || !enemy.IsInState || !enemy.IsEnemy(caster) ||
                !Game.Instance.Player.Party.Contains(caster) ||
                Game.Instance.State.Units.All.Any(unit => !ReferenceEquals(unit, caster) && !ReferenceEquals(unit, enemy)))
                throw new InvalidOperationException("Native elemental turn scope requires exactly two disposable live actors and idle combat.");
            _turnBasedBefore = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            _pausedBefore = Game.Instance.IsPaused;
            _timeBefore = Game.Instance.Player.GameTime;
            _scrollBefore = SettingsRoot.Instance.CameraScrollToCurrentUnit.CurrentValue;
            _cameraProperty = typeof(Game).GetProperty("CameraController", Members);
            if (_cameraProperty == null || _cameraProperty.GetSetMethod(true) == null)
                throw new MissingMethodException("Native CameraController setter");
            _cameraBefore = Game.Instance.CameraController;
            _selectionBefore = SelectionManager.Instance;
            _selectionProperty = typeof(SelectionManager).GetProperty("Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (!ReferenceEquals(_selectionBefore, null) || Kingmaker.UI._ConsoleUI.Models.UnitSelectionManager.Instance != null ||
                _selectionProperty == null || _selectionProperty.GetSetMethod(true) == null)
                throw new InvalidOperationException("The save-free native selection-service boundary is not empty.");
            _avoidanceBefore = new[] { caster, enemy }.Select(unit => unit.View.AgentASP.AvoidanceDisabled).ToArray();
            Active = this;
            try
            {
                _selectionObject = new GameObject("KMG_Runtime_ElementalTurn_Selection");
                _selectionObject.hideFlags = HideFlags.HideAndDontSave;
                _selection = _selectionObject.AddComponent<SelectionManager>();
                if (!ReferenceEquals(SelectionManager.Instance, _selection))
                    throw new InvalidOperationException("The native disposable SelectionManager did not establish its instance.");
                foreach (var unit in new[] { caster, enemy })
                {
                    unit.IsInGame = true; unit.IsInFogOfWar = false;
                    unit.View.SetVisible(true, true);
                    unit.View.AgentASP.AvoidanceDisabled = true;
                    if (!unit.IsVisibleForPlayer) throw new InvalidOperationException("Owned combatant is absent from the native turn filter.");
                }
                // Enroll the owned actors before toggling native turn-based mode.
                // Main-menu GameTime and LastSurpriseActionTime both begin at zero;
                // a fabricated fresh-combat event would infer an opening ambush
                // despite there being no offensive command. The normal toggle
                // callback uses Reset(true, false) and preserves existing combat.
                _entered = true;
                Tick(() => {
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                    Game.Instance.TurnBasedCombatController.Activate();
                    caster.JoinCombat(); enemy.JoinCombat();
                    Game.Instance.Player.UpdateIsInCombat();
                    _join.Tick(); _prepare.Tick();
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = turnBased;
                    Game.Instance.TurnBasedCombatController.Activate();
                });
                if (!turnBased)
                {
                    _selection.SelectUnit(caster.View, true, true, false);
                    if (CombatController.IsInTurnBasedCombat() || !Game.Instance.Player.IsInCombat)
                        throw new InvalidOperationException("Native RTWP fixture enrollment failed.");
                    return;
                }
                EventBus.Subscribe(Game.Instance.TurnBasedCombatController); _subscribed = true;
                if (Game.Instance.TurnBasedCombatController.RoundNumber != 1 ||
                    Game.Instance.TurnBasedCombatController.IsSurprised(caster) ||
                    Game.Instance.TurnBasedCombatController.IsSurprised(enemy))
                    throw new InvalidOperationException("Native mode toggle did not establish ordinary combat.");
                Turns.Add(new JObject { ["combatEntry"] = "native turn-based toggle after owned combat enrollment",
                    ["round"] = Game.Instance.TurnBasedCombatController.RoundNumber, ["surpriseRound"] = false });
                if (!CombatController.IsInTurnBasedCombat() || !Game.Instance.Player.IsInCombat ||
                    !Game.Instance.TurnBasedCombatController.SortedUnits.Contains(caster) ||
                    !Game.Instance.TurnBasedCombatController.SortedUnits.Contains(enemy))
                    throw new InvalidOperationException("Native turn controller did not enroll both owned actors.");
                ReachCasterTurn();
            }
            catch { Dispose(); throw; }
        }

        private void Tick(Action action)
        {
            var field = typeof(Game).GetField("m_GameModes", Members);
            var modes = field == null ? null : field.GetValue(Game.Instance) as Stack<GameMode>;
            if (modes == null || modes.Contains(_tickMode)) throw new InvalidOperationException("Native mode scope is ambiguous.");
            GameMode[] before = modes.ToArray();
            bool pause = Game.Instance.IsPaused;
            CameraController camera = Game.Instance.CameraController;
            UnitHandEquipmentController hands = Game.Instance.HandsEquipmentController;
            bool scroll = SettingsRoot.Instance.CameraScrollToCurrentUnit.CurrentValue;
            float delta = Game.Instance.TimeController.DeltaTime, gameDelta = Game.Instance.TimeController.GameDeltaTime;
            modes.Push(_tickMode);
            try
            {
                if (!_firearmPaused) Game.Instance.IsPaused = false;
                // Main-menu mode transitions rebuild public gameplay services.
                // Bind the same owned hands controller for each native scope,
                // just as the camera dependency is scoped below.
                _handsProperty.SetValue(Game.Instance, _ownedHands, null);
                _cameraProperty.SetValue(Game.Instance, new CameraController(false, false, false), null);
                SettingsRoot.Instance.CameraScrollToCurrentUnit.CurrentValue = false;
                Game.Instance.TimeController.SetDeltaTime(0.25f);
                Game.Instance.TimeController.SetGameDeltaTime(0.25f);
                action();
            }
            finally
            {
                Game.Instance.TimeController.SetDeltaTime(delta);
                Game.Instance.TimeController.SetGameDeltaTime(gameDelta);
                _cameraProperty.SetValue(Game.Instance, camera, null);
                _handsProperty.SetValue(Game.Instance, hands, null);
                SettingsRoot.Instance.CameraScrollToCurrentUnit.CurrentValue = scroll;
                if (!_firearmPaused) Game.Instance.IsPaused = pause;
                if (modes.Count != before.Length + 1 || !ReferenceEquals(modes.Peek(), _tickMode))
                    throw new InvalidOperationException("Native tick changed the scoped game-mode stack.");
                modes.Pop();
                if (!CharacterCreationObservationIdentity.SameOrderedReferences(before, modes.ToArray()))
                    throw new InvalidOperationException("Native mode stack restoration was not exact.");
            }
        }

        internal void ReachCasterTurn()
        {
            var controller = Game.Instance.TurnBasedCombatController;
            for (int tick = 0; tick < 240; ++tick)
            {
                Tick(() => {
                    _join.Tick(); _prepare.Tick(); controller.Tick(); controller.TickTime();
                    foreach (var unit in new[] { _caster, _enemy })
                        if (unit.IsInCombat) { _cooldowns.TickExact(unit); _buffs.TickExact(unit); }
                });
                var turn = controller.CurrentTurn;
                if (tick < 12 || tick == 239)
                {
                    var next = typeof(CombatController).GetField("m_NextUnit", Members).GetValue(controller) as UnitEntityData;
                    Turns.Add(new JObject { ["tick"] = tick, ["initialized"] = controller.Initialized,
                        ["waitingUI"] = controller.WaitingForUI.Value, ["uiGuardCount"] = controller.WaitingForUI.GuardCount,
                        ["round"] = controller.RoundNumber, ["timeSinceStart"] = controller.TimeSinceStart,
                        ["current"] = turn == null ? "none" : ReferenceEquals(turn.Unit, _caster) ? "caster" : "enemy",
                        ["status"] = turn == null ? "none" : turn.Status.ToString(), ["next"] = next == null ? "none" : ReferenceEquals(next, _caster) ? "caster" : "enemy",
                        ["casterSurprised"] = controller.IsSurprised(_caster), ["enemySurprised"] = controller.IsSurprised(_enemy),
                        ["casterPrepared"] = _caster.CombatState.Prepared, ["casterGetUp"] = _caster.View.IsGetUp,
                        ["casterTimeToNextTurn"] = _caster.GetTimeToNextTurn(), ["enemyTimeToNextTurn"] = _enemy.GetTimeToNextTurn(),
                        ["selectedCaster"] = ReferenceEquals(_selection.SingleSelectedUnit, _caster) });
                }
                if (turn == null) continue;
                if (!ReferenceEquals(turn.Unit, _caster) && !ReferenceEquals(turn.Unit, _enemy))
                    throw new InvalidOperationException("A foreign actor entered the owned turn scope.");
                if (turn.Status == TurnController.TurnStatus.None && _prepared.Add(turn))
                    Tick(turn.Prepare);
                if (ReferenceEquals(turn.Unit, _caster) &&
                    (turn.Status == TurnController.TurnStatus.Preparing || turn.Status == TurnController.TurnStatus.Acting))
                {
                    Turns.Add(new JObject { ["round"] = controller.RoundNumber, ["actorId"] = turn.Unit.UniqueId,
                        ["status"] = turn.Status.ToString(), ["nativeCurrentActorExact"] = true });
                    return;
                }
                if (ReferenceEquals(turn.Unit, _enemy)) EndCurrentTurn();
            }
            throw new InvalidOperationException("The native turn loop did not reach the exact disposable caster.");
        }

        internal void EndCurrentTurn()
        {
            var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
            if (turn == null || (!ReferenceEquals(turn.Unit, _caster) && !ReferenceEquals(turn.Unit, _enemy)))
                throw new InvalidOperationException("Cannot advance an unrelated native turn.");
            Tick(() => {
                turn.ForceToEnd(true);
                typeof(TurnController).GetMethod("End", Members, null, Type.EmptyTypes, null).Invoke(turn, null);
            });
            if (turn.Status != TurnController.TurnStatus.Ended) throw new InvalidOperationException("Native turn end did not complete.");
        }

        private sealed class OwnedCooldowns : UnitCombatCooldownsController
        {
            internal void TickExact(UnitEntityData unit) { base.TickOnUnit(unit); }
        }
        private sealed class OwnedBuffs : UnitBuffsController
        {
            internal void TickExact(UnitEntityData unit) { base.TickOnUnit(unit); }
        }

        internal void Execute(Action action) { Tick(action); }

        // Native CurrentMode reads the stack; IsPaused separately reads the
        // registered mode count. Own both parts of the empty-controller Pause
        // context across frames. Calling the public setter from the main-menu
        // host can queue a future mode change instead of pausing the input.
        // This sets only fixture context; command processing and costs stay native.
        internal void SetFirearmPaused(bool value)
        {
            if (_firearmPaused == value) return;
            var modes = (Stack<GameMode>)typeof(Game).GetField("m_GameModes", Members).GetValue(Game.Instance);
            if (modes.Contains(_tickMode)) throw new InvalidOperationException("Pause transition inside a native tick.");
            var counts = (int[])typeof(Game).GetField("m_ModesCount", Members).GetValue(Game.Instance);
            int pauseIndex = (int)GameModeType.Pause;
            if (counts[pauseIndex] != (_firearmPaused ? 1 : 0))
                throw new InvalidOperationException("Native pause registration ownership changed.");
            if (value)
            {
                if (Game.Instance.IsPaused || modes.Contains(_firearmPauseMode))
                    throw new InvalidOperationException("A foreign pause exists in the disposable fixture.");
                _beforeFirearmPause = modes.ToArray();
                modes.Push(_firearmPauseMode);
                counts[pauseIndex]++;
                _firearmPaused = true;
            }
            else
            {
                if (!ReferenceEquals(modes.Peek(), _firearmPauseMode))
                    throw new InvalidOperationException("Owned native pause mode was superseded.");
                modes.Pop();
                counts[pauseIndex]--;
                _firearmPaused = false;
                if (!CharacterCreationObservationIdentity.SameOrderedReferences(_beforeFirearmPause, modes.ToArray()))
                    throw new InvalidOperationException("Native pause mode restoration was not exact.");
                _beforeFirearmPause = null;
            }
            if (Game.Instance.IsPaused != value) throw new InvalidOperationException("Native pause state disagrees with the fixture.");
            Turns.Add(new JObject { ["nativePaused"] = Game.Instance.IsPaused, ["frame"] = Time.frameCount });
        }


        internal void LeaveCombatForRecovery()
        {
            Tick(() => {
                Game.Instance.TurnBasedCombatController.HandlePartyCombatStateChanged(false);
                if (_caster.IsInCombat) _caster.LeaveCombat();
                if (_enemy.IsInCombat) _enemy.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
            });
            if (_caster.IsInCombat || _enemy.IsInCombat || Game.Instance.Player.IsInCombat)
                throw new InvalidOperationException("Owned native combat did not end before recovery input.");
        }


        // Full native command/controller ticks for exactly the two owned actors.
        // Animation act cues are supplied by the save-free fixture, never attack
        // rules, cooldown resets, command authorization, or reload completion.
        internal void PumpCommands()
        {
            if (_firearmPaused || Game.Instance.IsPaused)
                throw new InvalidOperationException("Native command processing cannot be driven while paused.");
            Tick(() => {
                DrainFirearmCallbacks();
                // Main-menu scenes do not run the default game controllers.
                // Rebuild the real awake lists and advance owned visual handles;
                // otherwise a native draw-weapon coroutine never releases hands.
                new SleepingUnitsController().Tick();
                _join.Tick(); _prepare.Tick();
                Game.Instance.HandsEquipmentController.Tick();
                foreach (var unit in new[] { _caster, _enemy })
                {
                    unit.View.AnimationManager.Tick();
                    unit.View.AnimationManager.Update(0.25f);
                }
                var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                if (CombatController.IsInTurnBasedCombat() && turn != null)
                    typeof(TurnController).GetMethod("Tick", Members).Invoke(turn, null);
                else Game.Instance.Player.GameTime += TimeSpan.FromSeconds(0.25);
                foreach (var unit in new[] { _caster, _enemy })
                {
                    _cooldowns.TickExact(unit);
                    _buffs.TickExact(unit);
                    foreach (var command in unit.Commands.Raw.Where(value => value != null).ToArray())
                    {
                        if (command.Animation != null) command.Animation.IsActed = true;
                        var cast = command as UnitUseAbility;
                        if (cast != null && cast.ExecutionProcess != null && !cast.ExecutionProcess.IsEnded)
                            cast.ExecutionProcess.Tick();
                    }
                    typeof(UnitActionController).GetMethod("TickOnUnit", Members)
                        .Invoke(_actions, new object[] { unit });
                }
                new Kingmaker.Controllers.Brain.AiBrainController().Tick();
                Game.Instance.ProjectileController.Tick();
            });
        }

        // The main-menu host does not run Game.Tick. Drain only callbacks
        // produced by this firearm fixture through that native entry point,
        // with the already-owned empty controller mode. Never invoke a private
        // reload helper or manufacture its completion.
        private void DrainFirearmCallbacks()
        {
            var pending = (List<Action>)typeof(Game).GetField("m_BeforeTickActions", Members).GetValue(Game.Instance);
            if (pending.Count == 0) return;
            string prefix = typeof(Firing.EmptyFirearmAttackCommandPatch).FullName + "+";
            if (pending.Any(action => action == null || action.Method.DeclaringType == null ||
                !action.Method.DeclaringType.FullName.StartsWith(prefix, StringComparison.Ordinal)))
                throw new InvalidOperationException("Foreign scheduled callbacks in owned firearm fixture: " +
                    string.Join(",", pending.Select(action => action == null ? "null" : action.Method.DeclaringType.FullName)));
            Game.Instance.Tick();
            Game.Instance.TimeController.SetDeltaTime(0.25f);
            Game.Instance.TimeController.SetGameDeltaTime(0.25f);
            if (pending.Count != 0) throw new InvalidOperationException("Native Game.Tick did not drain owned firearm callbacks.");
        }
        internal void FlushFirearmCallbacks() { Tick(DrainFirearmCallbacks); }

        internal void Drive(UnitUseAbility command)
        {
            var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
            if (!CombatController.IsInTurnBasedCombat() || turn == null || !ReferenceEquals(turn.Unit, _caster) ||
                !ReferenceEquals(command.Executor, _caster) || command.Cutscene || command.IsIgnoreCooldown)
                throw new InvalidOperationException("Cannot drive a command outside its exact native caster turn.");
            Tick(() => {
                bool scheduled = Game.Instance.HandsEquipmentController.IsUpdateScheduledFor(_caster);
                Game.Instance.HandsEquipmentController.Tick();
                typeof(TurnController).GetMethod("Tick", Members, null, Type.EmptyTypes, null).Invoke(turn, null);
                if (command.Animation != null) command.Animation.IsActed = true;
                ElementalBreathScenario.TickCommand(_actions, command);
                if (_commandTicks++ < 6 || (!command.IsStarted && _commandTicks == 16))
                    Turns.Add(new JObject { ["commandTick"] = _commandTicks, ["ability"] = command.Spell.Blueprint.AssetGuid,
                        ["turnStatus"] = turn.Status.ToString(), ["started"] = command.IsStarted, ["acted"] = command.IsActed,
                        ["handsScheduledBefore"] = scheduled, ["handsScheduledAfter"] = Game.Instance.HandsEquipmentController.IsUpdateScheduledFor(_caster),
                        ["handsBusy"] = _caster.AreHandsBusyWithAnimation, ["dontWaitForHands"] = command.DontWaitForHands,
                        ["awaitMovement"] = command.AwaitMovementFinish, ["reallyMoving"] = _caster.View.MovementAgent.IsReallyMoving,
                        ["canStart"] = command.CanStart, ["canAct"] = _caster.Descriptor.State.CanAct,
                        ["hasCooldown"] = _caster.CombatState.HasCooldownForCommand(command),
                        ["startGate"] = (bool)typeof(UnitActionController).GetMethod("ShouldStartCommand", Members).Invoke(_actions, new object[] { command }) });
            });
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                SetFirearmPaused(false);
                _caster.Commands.InterruptAll(true); _caster.Commands.RemoveFinishedAndUpdateQueue();
                _enemy.Commands.InterruptAll(true); _enemy.Commands.RemoveFinishedAndUpdateQueue();
                if (_subscribed) { EventBus.Unsubscribe(Game.Instance.TurnBasedCombatController); _subscribed = false; }
                if (_entered)
                {
                    Game.Instance.TurnBasedCombatController.HandlePartyCombatStateChanged(false);
                    if (_caster.IsInCombat) _caster.LeaveCombat();
                    if (_enemy.IsInCombat) _enemy.LeaveCombat();
                    Game.Instance.Player.UpdateIsInCombat();
                }
                if (_selection != null)
                {
                    if (!ReferenceEquals(SelectionManager.Instance, _selection))
                        throw new InvalidOperationException("Native disposable SelectionManager ownership changed.");
                    typeof(SelectionManager).GetMethod("Clear", Members, null, Type.EmptyTypes, null).Invoke(_selection, null);
                    UnityEngine.Object.DestroyImmediate(_selectionObject);
                    _selectionProperty.SetValue(null, _selectionBefore, null);
                    _selection = null; _selectionObject = null;
                }
                _caster.View.AgentASP.AvoidanceDisabled = _avoidanceBefore[0];
                _enemy.View.AgentASP.AvoidanceDisabled = _avoidanceBefore[1];
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = _turnBasedBefore;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.Player.GameTime = _timeBefore;
                Game.Instance.IsPaused = _pausedBefore;
                Restored = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue == _turnBasedBefore &&
                    Game.Instance.IsPaused == _pausedBefore && Game.Instance.Player.GameTime == _timeBefore &&
                    ReferenceEquals(Game.Instance.CameraController, _cameraBefore) &&
                    ReferenceEquals(SelectionManager.Instance, _selectionBefore) &&
                    SettingsRoot.Instance.CameraScrollToCurrentUnit.CurrentValue == _scrollBefore &&
                    !Game.Instance.Player.IsInCombat && !_caster.IsInCombat && !_enemy.IsInCombat &&
                    _caster.View.AgentASP.AvoidanceDisabled == _avoidanceBefore[0] &&
                    _enemy.View.AgentASP.AvoidanceDisabled == _avoidanceBefore[1];
                if (!Restored) throw new InvalidOperationException("Native turn scope did not restore its exact prior controller/settings state.");
            }
            finally { if (ReferenceEquals(Active, this)) Active = null; }
        }

        // The save-free scene has no Astar graph. This native method only
        // flushes and erodes that absent grid; it does not compute turn costs.
        [HarmonyPatch]
        private static class OwnedNavigationGridPatch
        {
            private static MethodBase TargetMethod() => typeof(CombatController).GetMethods(Members)
                .Single(method => method.Name == "UpdateNavigationGridTags" &&
                    method.ReturnType == typeof(void) && method.GetParameters().Length == 0);
            private static bool Prefix(CombatController __instance)
            {
                var scope = Active;
                if (scope == null) return true;
                if (!ReferenceEquals(__instance, Game.Instance.TurnBasedCombatController) ||
                    Game.Instance.State.Units.All.Any(unit => !ReferenceEquals(unit, scope._caster) && !ReferenceEquals(unit, scope._enemy)))
                    throw new InvalidOperationException("Navigation accommodation escaped the exact disposable actor scope.");
                if (!scope._navigationObserved) scope.Turns.Add(new JObject { ["navigationGridAccommodation"] = "absent save-free Astar grid only" });
                scope._navigationObserved = true;
                return false;
            }
        }
    }
}
