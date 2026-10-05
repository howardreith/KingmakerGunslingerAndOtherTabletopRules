using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UI.Group;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Independent cells per creature/mode. No historical
        // insect cell is replayed, and no AI cell is given a manual attack.
        private static readonly string[][] Sprint16CombatCells =
            (from key in new[] { "crocodile", "dire-crocodile" }
             from mode in new[] { "rtwp", "turn-based" }
             from driver in (key == "crocodile"
                 ? new[] { "manual", "ai-fresh", "ai-player-cooldown", "manual-hold" }
                 : new[] { "manual", "ai-fresh", "ai-player-cooldown", "manual-hold", "manual-swallow" })
             select new[] { key, mode, driver }).ToArray();

        // Scoped only to one synchronous manual-cell summon. The native rule
        // creates its normal controllable summon part; no global control patch.
        private sealed class Sprint16ManualSummonControl : IGlobalRulebookHandler<RuleSummonUnit>
        {
            internal UnitEntityData Caster;
            internal BlueprintUnit Blueprint;
            internal int Matched;
            public void OnEventAboutToTrigger(RuleSummonUnit evt)
            {
                if (!ReferenceEquals(evt.Initiator, Caster) ||
                    !ReferenceEquals(evt.Blueprint, Blueprint)) return;
                evt.IsDirectlyControllable = true;
                Matched++;
            }
            public void OnEventDidTrigger(RuleSummonUnit evt) { }
        }

        private ExpandedSummoningCorrectionFixture _crocCombatFixture;
        private UnitEntityData _crocCombatOwner;
        private UnitReference _crocCombatMasterBefore;
        private bool _crocCombatCapitalMaster;
        private BlueprintFaction _crocCombatFactionBefore;
        private BlueprintFaction[] _crocCombatAttackFactionsBefore;
        private bool _crocCombatFactionChanged;
        private JObject _crocCombatControlBefore;
        private JObject _crocCombatControlAfter;
        private int _crocCombatControlRules;
        private UnitEntityData[] _crocCombatAwakeBefore;
        private Sprint16RuleObserver _crocCombatObserver;
        private readonly JArray _crocCombatRows = new JArray();
        private readonly Dictionary<UnitCommand, JObject> _crocCombatCommands =
            new Dictionary<UnitCommand, JObject>();
        private readonly HashSet<TurnController> _crocCombatPrepared = new HashSet<TurnController>();
        private readonly UnitCombatJoinController _crocCombatJoin = new UnitCombatJoinController();
        private readonly UnitCombatPrepareController _crocCombatPrepare = new UnitCombatPrepareController();
        private int _crocCombatCell;
        private int _crocCombatFrame;
        private int _crocCombatReadyFrame = -1;
        private bool _crocCombatManualAttackQueued;
        private int _crocCombatManualAttackAttempts;
        private bool _crocCombatModeBefore;
        private bool _crocCombatPauseBefore;
        private TimeSpan _crocCombatTimeBefore;
        private Vector3 _crocCombatOrigin;
        private float _crocCombatTravel;
        private DateTime _crocCombatStartUtc;
        private int _crocCombatSprintFirstSeen = -1;
        private bool _crocCombatSharedCooldown;
        private int _crocCombatFirstAttack = -1;
        private bool _crocCombatAwaitPlayerCast;
        private UnitUseAbility _crocCombatRejectedCast;
        private Buff _crocCombatCooldown;
        private TimeSpan _crocCombatCooldownDeadline;
        private readonly JArray _crocCombatContacts = new JArray();
        private readonly JArray _crocCombatAttackOrigins = new JArray();
        private readonly List<UnitAttack> _crocCombatIssuedAttacks = new List<UnitAttack>();
        private UnitEntityData[] _crocCombatSelectionBefore;
        private UnitEntityData _crocCombatGroupBefore;

        private void PollSprint16Combat()
        {
            try
            {
                if (_crocCombatFixture == null)
                {
                    _crocCombatModeBefore = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
                    _crocCombatPauseBefore = Game.Instance.IsPaused;
                    _crocCombatTimeBefore = Game.Instance.Player.GameTime;
                    _crocCombatAwakeBefore = Game.Instance.State.AwakeUnits.ToArray();
                    _crocCombatSelectionBefore = Game.Instance.UI.SelectionManagerPC.SelectedUnits.ToArray();
                    _crocCombatGroupBefore = GroupController.Instance.GetCurrentCharacter();
                    _crocCombatFixture = BeginExpandedSummoningCorrectionFixture("KMG_Runtime_Sprint16_CombatCaster");
                    CreateExpandedSummoningCorrectionHostile(_crocCombatFixture);
                    // This full-attack matrix isolates bite/tail commands from
                    // the hold that legitimately stops an attack sequence.
                    // Grab and maintain are exercised with immunity absent in
                    // the preceding rule matrix.
                    _crocCombatFixture.Hostile.Descriptor.State.AddCondition(
                        UnitCondition.ImmuneToCombatManeuvers, null);
                }
                if (_crocCombatOwner == null) BeginSprint16CombatCell();
                _crocCombatFrame++;
                // Join/mode activation may interrupt a command during setup.
                // Keep AI's native action list intact, but open its decision
                // clock only after this request-local fixture is in game and
                // the normal appearance/setup controllers have settled.
                bool nativeReady = _crocCombatFrame >= 30 && _crocCombatOwner.IsInGame &&
                    _crocCombatOwner.View != null && _crocCombatOwner.View.IsInGame &&
                    _crocCombatOwner.Descriptor.State.CanAct;
                bool ready = StepSprint16CombatTurn() && nativeReady;
                if (ready && _crocCombatReadyFrame < 0)
                {
                    _crocCombatReadyFrame = _crocCombatFrame;
                    string driver = Sprint16CombatCells[_crocCombatCell][2];
                    if (driver == "ai-fresh")
                        _crocCombatOwner.CombatState.AIData.NextCommandTime = Time.time;
                    if (driver.StartsWith("manual", StringComparison.Ordinal) || driver == "ai-player-cooldown")
                    {
                        var sprint = Sprint16Sprint(_crocCombatFixture.Blueprints,
                            Sprint16CombatCells[_crocCombatCell][0]);
                        // Real queued swift command, not the synchronous
                        // mechanics helper and not a forced animation contact.
                        var cast = new UnitUseAbility(new AbilityData(
                            _crocCombatOwner.Descriptor.Abilities.GetAbility(sprint)),
                            new TargetWrapper(_crocCombatOwner));
                        _crocCombatOwner.Commands.Run(cast);
                    }
                }
                ObserveSprint16CombatFrame();
                bool manual = Sprint16CombatCells[_crocCombatCell][2].StartsWith("manual", StringComparison.Ordinal);
                if (manual && ready && _crocCombatRejectedCast == null &&
                    _crocCombatSprintFirstSeen >= 0 &&
                    !_crocCombatOwner.Commands.Raw.OfType<UnitUseAbility>().Any(value => !value.IsFinished))
                {
                    // Deliberate invalid native command; never call the
                    // synchronous helper, waive availability or change a timer.
                    _crocCombatCooldown = _crocCombatOwner.Descriptor.Buffs.GetBuff(
                        Sprint16SprintBuff(_crocCombatFixture.Blueprints, Sprint16CombatCells[_crocCombatCell][0], true));
                    _crocCombatCooldownDeadline = _crocCombatCooldown.EndTime;
                    _crocCombatRejectedCast = new UnitUseAbility(new AbilityData(
                        _crocCombatOwner.Descriptor.Abilities.GetAbility(Sprint16Sprint(
                            _crocCombatFixture.Blueprints, Sprint16CombatCells[_crocCombatCell][0]))),
                        new TargetWrapper(_crocCombatOwner));
                    _crocCombatOwner.Commands.Run(_crocCombatRejectedCast);
                }
                if (manual && ready && !_crocCombatManualAttackQueued &&
                    _crocCombatRejectedCast != null && _crocCombatRejectedCast.IsFinished &&
                    !_crocCombatOwner.Commands.Raw.OfType<UnitUseAbility>().Any(value => !value.IsFinished))
                {
                    // Keep incidental native attacks from establishing a hold
                    // while the deliberate cooldown rejection is still queued.
                    // Remove only this target's fixture immunity immediately
                    // before issuing the real held-target attack.
                    string driver = Sprint16CombatCells[_crocCombatCell][2];
                    if (driver == "manual-hold" || driver == "manual-swallow")
                        _crocCombatFixture.Hostile.Descriptor.State.RemoveConditionAll(
                            UnitCondition.ImmuneToCombatManeuvers);
                    var attack = new UnitAttack(_crocCombatFixture.Hostile) { ForceFullAttack = true };
                    _crocCombatIssuedAttacks.Add(attack);
                    _crocCombatOwner.Commands.Run(attack);
                    _crocCombatManualAttackQueued = true;
                    _crocCombatManualAttackAttempts++;
                }
                string heldDriver = Sprint16CombatCells[_crocCombatCell][2];
                if (manual && ready && (heldDriver == "manual-hold" || heldDriver == "manual-swallow") &&
                    _crocCombatManualAttackQueued && _crocCombatManualAttackAttempts < 4 &&
                    _crocCombatObserver.Checks.Count > 0 &&
                    SummonHoldComponent.HeldTarget(_crocCombatOwner) == null &&
                    _crocCombatFixture.Hostile.Get<UnitPartSwallowed>() == null &&
                    !_crocCombatOwner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                {
                    // A native natural-1/escape can legitimately refuse a hold.
                    // Retry a bounded real command; never force a rule roll,
                    // a hit, an animation contact, a grapple or a rider.
                    var retry = new UnitAttack(_crocCombatFixture.Hostile) { ForceFullAttack = true };
                    _crocCombatIssuedAttacks.Add(retry);
                    _crocCombatOwner.Commands.Run(retry);
                    _crocCombatManualAttackAttempts++;
                }
                bool twoWeapons = _crocCombatObserver.Attacks.Where(value => value.Weapon != null)
                    .Select(value => value.Weapon.Blueprint.AssetGuid).Distinct().Count() >= 2;
                // Leave enough ordinary decisions after the first attack to
                // detect a repeated unavailable Sprint queue or an adjacent
                // stall. Both modes are bounded by wall time, not frame rate.
                string currentDriver = Sprint16CombatCells[_crocCombatCell][2];
                bool heldCell = currentDriver == "manual-hold" || currentDriver == "manual-swallow";
                bool rider = _crocCombatObserver.Damage.Any(value => ReferenceEquals(value.Initiator,
                    _crocCombatOwner) && value.AttackRoll == null);
                bool done = CrocodilianCommandReviewPolicy.CanFinish(manual,
                    _crocCombatRejectedCast != null && _crocCombatRejectedCast.IsFinished,
                    Sprint16IssuedAttackObserved(), heldCell ? rider : twoWeapons,
                    _crocCombatFirstAttack < 0 ? -1 : _crocCombatFrame - _crocCombatFirstAttack);
                // A native move/Sprint, rejected attempt, and later stationary
                // full attack span more than the old 18-second wall window.
                // Observe bounded subsequent native turns without driving AI.
                if (done || (DateTime.UtcNow - _crocCombatStartUtc).TotalSeconds >= 60d)
                {
                    CompleteSprint16CombatCell();
                    _crocCombatCell++;
                    if (_crocCombatCell == Sprint16CombatCells.Length)
                        CompleteSprint16Combat();
                }
            }
            catch (Exception exception)
            {
                Sprint16Check(_crocodilianAssertions, _crocCombatRows,
                    "combat-exception", false, new JObject {
                        ["cell"] = _crocCombatCell,
                        ["exception"] = DescribeExpandedSummoningCorrectionException(exception) },
                    "all crocodilian native command and AI cells complete");
                CompleteSprint16Combat();
            }
        }

        private void BeginSprint16CombatCell()
        {
            string[] cell = Sprint16CombatCells[_crocCombatCell];
            UnitEntityData hostile = _crocCombatFixture.Hostile;
            ResetExpandedSummoningHostile(_crocCombatFixture);
            bool manual = cell[2].StartsWith("manual", StringComparison.Ordinal);
            if (manual)
            {
                var control = new Sprint16ManualSummonControl {
                    Caster = _crocCombatFixture.Caster,
                    Blueprint = _crocCombatFixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                        value.name == "KMG_Summoning_Unit_" +
                            (cell[0] == "crocodile" ? "Crocodile" : "DireCrocodile")) };
                EventBus.Subscribe(control);
                try { _crocCombatOwner = CastExpandedSummoningOwnTier(_crocCombatFixture, cell[0]); }
                finally { EventBus.Unsubscribe(control); }
                _crocCombatControlRules = control.Matched;
                var part = _crocCombatOwner.Get<UnitPartSummonedMonster>();
                if (control.Matched != 1 || part == null || !part.IsDirectlyControllable)
                    throw new InvalidOperationException("Manual native summon rule/control mismatch: rules=" +
                        control.Matched + ";part=" + (part != null) + ";flag=" +
                        (part != null && part.IsDirectlyControllable));
                _crocCombatControlBefore = Sprint16ControlObservation(_crocCombatOwner);
                _crocCombatMasterBefore = _crocCombatOwner.Descriptor.Master;
                _crocCombatCapitalMaster = false;
                _crocCombatFactionBefore = _crocCombatOwner.Faction;
                _crocCombatAttackFactionsBefore = _crocCombatOwner.AttackFactions.ToArray();
                _crocCombatFactionChanged = false;
                // RuleSummonUnit matches attack factions/group, not the native
                // faction identity. Outside the capital the control predicate
                // also requires a controllable faction. Switch only this owned
                // manual fixture through the existing native API; keep its
                // summon identity and attack-faction differences intact.
                if (!_crocCombatOwner.Faction.IsDirectlyControllable &&
                    !(Game.Instance.CurrentlyLoadedArea != null &&
                        Game.Instance.CurrentlyLoadedArea.IsCapital))
                {
                    UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
                    BlueprintFaction faction = main == null ? null : main.Faction;
                    if (faction == null || !faction.IsDirectlyControllable)
                        throw new InvalidOperationException("No exact controllable player faction: " +
                            _crocCombatControlBefore.ToString(Formatting.None));
                    _crocCombatFactionChanged = true;
                    _crocCombatOwner.Descriptor.SwitchFactions(faction, false);
                }
                // The capital's native control predicate additionally requires
                // MainCharacter or its pet. Give only this disposable manual
                // actor that identity; retain its native summoner link. Never
                // edit the player, party, area flag or a turn-controller flag.
                if (Game.Instance.CurrentlyLoadedArea != null &&
                    Game.Instance.CurrentlyLoadedArea.IsCapital && !_crocCombatOwner.IsDirectlyControllable)
                {
                    _crocCombatOwner.Descriptor.Master = Game.Instance.Player.MainCharacter;
                    _crocCombatCapitalMaster = true;
                }
                _crocCombatControlAfter = Sprint16ControlObservation(_crocCombatOwner);
                if (!_crocCombatOwner.IsDirectlyControllable)
                    throw new InvalidOperationException("Manual fixture fails the native control predicate; before=" +
                        _crocCombatControlBefore.ToString(Formatting.None) + ";after=" +
                        _crocCombatControlAfter.ToString(Formatting.None));
            }
            else _crocCombatOwner = CastExpandedSummoningOwnTier(_crocCombatFixture, cell[0]);
            // Never empty an AI cell's action list. The manual cell explicitly
            // disables its brain, which is disclosed in its evidence.
            bool heldCell = cell[2] == "manual-hold" || cell[2] == "manual-swallow";
            RemoveExpandedSummoningAppearanceBuffs(_crocCombatOwner);
            SetExpandedSummoningBrainActive(_crocCombatOwner, !manual);
            if (!manual) _crocCombatOwner.Brain.RestoreAvailableActions();
            hostile.Descriptor.State.RemoveConditionAll(UnitCondition.ImmuneToCombatManeuvers);
            hostile.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null);
            if (heldCell) hostile.Descriptor.State.Size = cell[2] == "manual-swallow"
                ? (Size)((int)_crocCombatOwner.Descriptor.State.Size - 1) : _crocCombatOwner.Descriptor.State.Size;
            _crocCombatAwaitPlayerCast = cell[2] == "ai-player-cooldown";
            _crocCombatOwner.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            _crocCombatOwner.Descriptor.Stats.HitPoints.BaseValue = 100000;
            _crocCombatOwner.Memory.Add(hostile);
            hostile.Memory.Add(_crocCombatOwner);
            var directions = new List<int>();
            string chosen;
            Vector3 point = ExpandedSummoningOpenPoint(hostile.Position,
                manual ? 1.5f : 8f, directions, out chosen);
            PlaceExpandedSummoningUnit(_crocCombatOwner, point);
            _crocCombatOrigin = _crocCombatOwner.Position;
            _crocCombatTravel = 0;
            _crocCombatFrame = 0;
            _crocCombatReadyFrame = -1;
            _crocCombatFirstAttack = -1;
            _crocCombatSprintFirstSeen = -1;
            _crocCombatSharedCooldown = false;
            _crocCombatManualAttackQueued = false;
            _crocCombatManualAttackAttempts = 0;
            _crocCombatRejectedCast = null;
            _crocCombatCooldown = null;
            _crocCombatContacts.Clear();
            _crocCombatAttackOrigins.Clear();
            _crocCombatIssuedAttacks.Clear();
            _crocCombatCommands.Clear();
            _crocCombatPrepared.Clear();
            _crocCombatStartUtc = DateTime.UtcNow;
            _crocCombatObserver = new Sprint16RuleObserver { Owner = _crocCombatOwner, Target = hostile };
            _crocCombatObserver.ObserveWeaponContact = rule =>
            {
                _crocCombatAttackOrigins.Add(new JObject {
                    ["frame"] = _crocCombatFrame, ["weapon"] = rule.Weapon.Blueprint.name,
                    ["opportunity"] = rule.IsAttackOfOpportunity,
                    ["issuedCommandExecuting"] = _crocCombatIssuedAttacks.Any(value => value.IsStarted && !value.IsFinished),
                    ["rejectionFinished"] = _crocCombatRejectedCast != null && _crocCombatRejectedCast.IsFinished,
                    ["targetImmuneDuringSetup"] = hostile.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers) });
                RecordSprint16Contact(rule.Initiator, rule.Target,
                    rule.Weapon.Blueprint.Category == WeaponCategory.Bite ? "bite" : "tail");
            };
            _crocCombatObserver.ObserveRiderContact = rule => RecordSprint16Contact(
                rule.Initiator, rule.Target, cell[2] == "manual-swallow" ? "swallow-initial-bite" : "death-roll");
            EventBus.Subscribe(_crocCombatObserver);
            foreach (UnitEntityData unit in new[] { _crocCombatOwner, hostile })
                if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
            SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = cell[1] == "turn-based";
            Game.Instance.TurnBasedCombatController.Activate();
            _crocCombatOwner.JoinCombat();
            hostile.JoinCombat();
            Game.Instance.Player.UpdateIsInCombat();
            _crocCombatJoin.Tick();
            _crocCombatPrepare.Tick();
            if (!manual)
                _crocCombatOwner.CombatState.AIData.NextCommandTime = float.MaxValue;
            Game.Instance.IsPaused = false;
        }

        private static JObject Sprint16ControlObservation(UnitEntityData unit)
        {
            var area = Game.Instance.CurrentlyLoadedArea;
            var part = unit.Get<UnitPartSummonedMonster>();
            return new JObject {
                ["area"] = area == null ? null : area.AssetGuid,
                ["capital"] = area != null && area.IsCapital,
                ["faction"] = unit.Faction == null ? null : unit.Faction.AssetGuid,
                ["factionControllable"] = unit.Faction != null && unit.Faction.IsDirectlyControllable,
                ["summonPartControllable"] = part != null && part.IsDirectlyControllable,
                ["finallyDead"] = unit.Descriptor.State.IsFinallyDead,
                ["panicked"] = unit.Descriptor.State.IsPanicked,
                ["detached"] = unit.IsDetached,
                ["preventDirectControl"] = (bool)unit.PreventDirectControl,
                ["exCompanion"] = Game.Instance.Player.ExCompanions.Any(value =>
                    ReferenceEquals(value.Value, unit.Descriptor.Master.Value ?? unit)),
                ["master"] = unit.Descriptor.Master.Value == null ? null : unit.Descriptor.Master.Value.UniqueId,
                ["summoner"] = part == null || part.Summoner == null ? null : part.Summoner.UniqueId,
                ["controllable"] = unit.IsDirectlyControllable
            };
        }

        private bool StepSprint16CombatTurn()
        {
            bool wanted = Sprint16CombatCells[_crocCombatCell][1] == "turn-based";
            if (!wanted) return !CombatController.IsInTurnBasedCombat();
            if (!CombatController.IsInTurnBasedCombat()) return false;
            _crocCombatJoin.Tick();
            _crocCombatPrepare.Tick();
            TurnController turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
            if (turn == null) return false;
            if (turn.Status == TurnController.TurnStatus.None && _crocCombatPrepared.Add(turn)) turn.Prepare();
            bool acting = turn.Status == TurnController.TurnStatus.Preparing ||
                turn.Status == TurnController.TurnStatus.Acting;
            if (ReferenceEquals(turn.Unit, _crocCombatOwner))
            {
                string driver = Sprint16CombatCells[_crocCombatCell][2];
                if (driver.StartsWith("manual", StringComparison.Ordinal))
                    SetExpandedSummoningBrainActive(_crocCombatOwner, false);
                bool needsMaintain = driver == "manual-hold" || driver == "manual-swallow";
                // A completed native bite may leave a controllable summon at
                // an idle turn. End only this fixture owner's finished turn so
                // later native rounds can maintain; never invoke maintain here.
                if (needsMaintain && _crocCombatManualAttackQueued && _crocCombatFirstAttack >= 0 &&
                    !_crocCombatObserver.Damage.Any(value => ReferenceEquals(value.Initiator,
                        _crocCombatOwner) && value.AttackRoll == null) && turn.CanEndTurnAndNoActing())
                {
                    turn.ForceToEnd(true);
                    typeof(TurnController).GetMethod("End", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance, null,
                        Type.EmptyTypes, null).Invoke(turn, null);
                    return false;
                }
                return acting;
            }
            if (acting)
            {
                // Native turn progression; no actions, movement, saves or
                // resource use are requested of the working-save party.
                turn.ForceToEnd(true);
                typeof(TurnController).GetMethod("End", BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance, null,
                    Type.EmptyTypes, null).Invoke(turn, null);
            }
            return false;
        }

        private void ObserveSprint16CombatFrame()
        {
            foreach (UnitCommand command in _crocCombatOwner.Commands.Raw.Where(value => value != null))
            {
                JObject row;
                if (!_crocCombatCommands.TryGetValue(command, out row))
                {
                    row = new JObject { ["type"] = command.GetType().Name,
                        ["firstFrame"] = _crocCombatFrame,
                        ["aiAction"] = command.AiAction == null ? null : command.AiAction.name,
                        ["aiGuid"] = command.AiAction == null ? null : command.AiAction.AssetGuid };
                    _crocCombatCommands.Add(command, row);
                }
                row["started"] = command.IsStarted;
                row["finished"] = command.IsFinished;
                row["result"] = command.Result.ToString();
                row["lastFrame"] = _crocCombatFrame;
                UnitAttack attack = command as UnitAttack;
                if (attack != null && attack.Animation != null)
                    row["nativeContactObserved"] = (bool?)row["nativeContactObserved"] == true ||
                        attack.Animation.IsActed;
            }
            // Read only: native animation contacts and command controllers
            // drive the events. No Animation.IsActed or command.Tick writes.
            if (_crocCombatFirstAttack < 0 && _crocCombatObserver.Attacks.Count > 0)
                _crocCombatFirstAttack = _crocCombatFrame;
            _crocCombatTravel = Math.Max(_crocCombatTravel,
                Vector3.Distance(_crocCombatOrigin, _crocCombatOwner.Position));
            string key = Sprint16CombatCells[_crocCombatCell][0];
            if (_crocCombatOwner.Descriptor.HasFact(Sprint16SprintBuff(_crocCombatFixture.Blueprints, key, true)))
            {
                if (_crocCombatSprintFirstSeen < 0) _crocCombatSprintFirstSeen = _crocCombatFrame;
                _crocCombatSharedCooldown = !new AbilityData(_crocCombatOwner.Descriptor.Abilities
                    .GetAbility(Sprint16Sprint(_crocCombatFixture.Blueprints, key))).IsAvailable;
                if (_crocCombatAwaitPlayerCast)
                {
                    // Start ordinary AI only after the player's real cast has
                    // taken the shared cooldown. No action is removed/added.
                    _crocCombatOwner.CombatState.AIData.NextCommandTime = Time.time;
                    _crocCombatAwaitPlayerCast = false;
                }
            }
        }

        private void CompleteSprint16CombatCell()
        {
            string[] cell = Sprint16CombatCells[_crocCombatCell];
            var attacks = new JArray(_crocCombatObserver.Attacks.Select(value => new JObject {
                ["weapon"] = value.Weapon == null ? null : value.Weapon.Blueprint.name,
                ["weaponGuid"] = value.Weapon == null ? null : value.Weapon.Blueprint.AssetGuid,
                ["hit"] = value.IsHit, ["bonus"] = value.AttackBonus,
                ["logged"] = !value.SuspendCombatLog, ["critical"] = value.IsCriticalConfirmed
            }));
            var sprint = Sprint16Sprint(_crocCombatFixture.Blueprints, cell[0]);
            RuleCastSpell[] casts = _crocCombatObserver.Casts.Where(value =>
                ReferenceEquals(value.Spell.Blueprint, sprint)).ToArray();
            foreach (var pair in _crocCombatCommands)
            {
                pair.Value["finished"] = pair.Key.IsFinished;
                pair.Value["result"] = pair.Key.Result.ToString();
            }
            bool ai = !cell[2].StartsWith("manual", StringComparison.Ordinal);
            bool heldCell = cell[2] == "manual-hold" || cell[2] == "manual-swallow";
            bool twoWeapons = _crocCombatObserver.Attacks.Where(value => value.Weapon != null)
                .Select(value => value.Weapon.Blueprint.AssetGuid).Distinct().Count() == 2;
            bool nativeAiAttack = _crocCombatCommands.Any(value => value.Key is UnitAttack &&
                value.Key.AiAction != null);
            bool noFailedSpam = _crocCombatCommands.Count(value => value.Key is UnitUseAbility &&
                !ReferenceEquals(value.Key, _crocCombatRejectedCast)) <= 1;
            bool rejected = ai || _crocCombatRejectedCast != null && _crocCombatRejectedCast.IsFinished &&
                (_crocCombatRejectedCast.Result == UnitCommand.ResultType.Fail ||
                    _crocCombatRejectedCast.Result == UnitCommand.ResultType.Interrupt &&
                    !_crocCombatRejectedCast.IsActed) && _crocCombatCooldown != null &&
                _crocCombatCooldown.EndTime == _crocCombatCooldownDeadline &&
                ReferenceEquals(_crocCombatOwner.Descriptor.Buffs.GetBuff(
                    Sprint16SprintBuff(_crocCombatFixture.Blueprints, cell[0], true)), _crocCombatCooldown);
            bool rider = _crocCombatObserver.Damage.Any(value => ReferenceEquals(value.Initiator,
                _crocCombatOwner) && value.AttackRoll == null);
            bool relationship = cell[2] == "manual-swallow" ?
                _crocCombatFixture.Hostile.Get<UnitPartSwallowed>() != null :
                ReferenceEquals(SummonHoldComponent.HeldTarget(_crocCombatOwner), _crocCombatFixture.Hostile) &&
                    _crocCombatFixture.Hostile.Descriptor.State.HasCondition(UnitCondition.Prone);
            bool exact = _crocCombatReadyFrame >= 0 && (heldCell ? rider && relationship : twoWeapons) &&
                (ai || Sprint16IssuedAttackObserved()) &&
                rejected && casts.Length == 1 && casts[0].Success &&
                _crocCombatSharedCooldown && noFailedSpam &&
                _crocCombatObserver.Attacks.All(value => !value.SuspendCombatLog) &&
                (!ai || nativeAiAttack && _crocCombatTravel > 1f) &&
                CombatController.IsInTurnBasedCombat() == (cell[1] == "turn-based");
            Sprint16Check(_crocodilianAssertions, _crocCombatRows,
                cell[0] + "-" + cell[1] + "-" + cell[2], exact,
                new JObject {
                    ["brain"] = _crocCombatOwner.Blueprint.Brain.name,
                    ["liveBrainActions"] = new JArray(_crocCombatOwner.Brain.Actions.Select(value => value.Blueprint.name)),
                    ["frames"] = _crocCombatFrame, ["readyFrame"] = _crocCombatReadyFrame,
                    ["firstAttackFrame"] = _crocCombatFirstAttack,
                    ["sprintFirstSeenFrame"] = _crocCombatSprintFirstSeen,
                    ["sprintCasts"] = casts.Length, ["sharedCooldown"] = _crocCombatSharedCooldown,
                    ["travelMeters"] = _crocCombatTravel, ["nativeAiAttack"] = nativeAiAttack,
                    ["deliberateCooldownCommandRejected"] = rejected,
                    ["rejectedCommandResult"] = _crocCombatRejectedCast == null ? null : _crocCombatRejectedCast.Result.ToString(),
                    ["rejectedCommandActed"] = _crocCombatRejectedCast != null && _crocCombatRejectedCast.IsActed,
                    ["directlyControllable"] = _crocCombatOwner.IsDirectlyControllable,
                    ["manualNativeAttackAttempts"] = _crocCombatManualAttackAttempts,
                    ["issuedAttackObserved"] = Sprint16IssuedAttackObserved(),
                    ["attackOrigins"] = new JArray(_crocCombatAttackOrigins),
                    ["nativeManualControlRules"] = _crocCombatControlRules,
                    ["disposableCapitalPetMaster"] = _crocCombatCapitalMaster,
                    ["disposableManualFactionChanged"] = _crocCombatFactionChanged,
                    ["controlBefore"] = _crocCombatControlBefore,
                    ["controlAfter"] = _crocCombatControlAfter,
                    ["nativeSummonerRetained"] = _crocCombatOwner.Get<UnitPartSummonedMonster>() != null &&
                        ReferenceEquals(_crocCombatOwner.Get<UnitPartSummonedMonster>().Summoner,
                            _crocCombatFixture.Caster),
                    ["maneuvers"] = new JArray(_crocCombatObserver.Checks.Select(value => new JObject {
                        ["initiator"] = value.Initiator.UniqueId,
                        ["roll"] = value.InitiatorRoll.Value, ["success"] = value.Success,
                        ["cmb"] = value.InitiatorCMB, ["cmd"] = value.TargetCMD })),
                    ["brainActive"] = _crocCombatOwner.IsBrainActive,
                    ["ownerBuffs"] = new JArray(_crocCombatOwner.Buffs.Enumerable.Select(value => new JObject {
                        ["name"] = value.Blueprint.name, ["nextTick"] = ReadExactMember(value, "NextTickTime").ToString(),
                        ["round"] = value.RoundNumber })),
                    ["turnStatus"] = Game.Instance.TurnBasedCombatController.CurrentTurn == null ? null :
                        Game.Instance.TurnBasedCombatController.CurrentTurn.Status.ToString(),
                    ["turnOwner"] = Game.Instance.TurnBasedCombatController.CurrentTurn == null ? null :
                        Game.Instance.TurnBasedCombatController.CurrentTurn.Unit.UniqueId,
                    ["nativeMaintainObserved"] = rider, ["expectedRelationship"] = relationship,
                    ["contacts"] = new JArray(_crocCombatContacts),
                    ["damage"] = new JArray(_crocCombatObserver.Damage.Select(Sprint16DamageEvent)),
                    ["commands"] = new JArray(_crocCombatCommands.Values), ["attacks"] = attacks,
                    ["grabSuppression"] = heldCell ? "setup-only target immunity removed before queued native bite; later rounds resolve rider" :
                        "disposable target immune to maneuvers for full-attack separation only",
                    ["animationContactForced"] = false
                }, heldCell ? "native bite establishes hold; native later-round maintain resolves exactly the selected rider" :
                    "one real Sprint; bite and secondary tail commands; AI moves/attacks through the shared cooldown without spam");
            foreach (string contact in (heldCell ? new[] { "bite", cell[2] == "manual-swallow" ?
                    "swallow-initial-bite" : "death-roll" } : new[] { "bite", "tail" }))
            {
                JObject[] samples = _crocCombatContacts.OfType<JObject>().Where(value => (string)value["kind"] == contact).ToArray();
                Sprint16Check(_crocodilianAssertions, _crocCombatRows,
                    string.Join("-", cell) + "-contact-" + contact,
                    samples.Length > 0 && samples.All(value => (bool?)value["contact"] == true),
                    new JObject { ["samples"] = new JArray(samples) },
                    "original weighted jaw/tail surface within 0.25m of live target bounds at each actual rule event");
            }
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory, "sprint16-combat.json"),
                _crocCombatRows.ToString(Formatting.Indented));
            CleanupSprint16CombatCell();
        }

        private bool Sprint16IssuedAttackObserved()
        {
            return _crocCombatAttackOrigins.OfType<JObject>().Any(value =>
                (bool?)value["issuedCommandExecuting"] == true && (bool?)value["opportunity"] == false &&
                (bool?)value["rejectionFinished"] == true);
        }

        private void RecordSprint16Contact(UnitEntityData owner, UnitEntityData target, string kind)
        {
            if (_crocCombatContacts.OfType<JObject>().Count(value => (string)value["kind"] == kind) >= 8) return;
            var row = new JObject { ["kind"] = kind, ["frame"] = _crocCombatFrame,
                ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["contact"] = false };
            _crocCombatContacts.Add(row);
            Mesh baked = null;
            try
            {
                var mesh = owner.View == null ? null : owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .SingleOrDefault(value => value.enabled && value.sharedMesh != null &&
                        value.sharedMesh.name == "KMG_" + Sprint16CombatCells[_crocCombatCell][0] + "_Original");
                var targetMesh = target.View == null ? null : target.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Where(value => value.enabled && value.sharedMesh != null && value.sharedMesh.vertexCount >= 100)
                    .OrderByDescending(value => value.bones.Length).FirstOrDefault();
                if (mesh == null || targetMesh == null)
                    throw new InvalidOperationException("Exact original or native target renderer unavailable.");
                Bounds bounds = targetMesh.bounds;
                baked = new Mesh();
                mesh.BakeMesh(baked);
                Vector3[] vertices = mesh.sharedMesh.vertices;
                Vector3[] bakedVertices = baked.vertices;
                BoneWeight[] weights = mesh.sharedMesh.boneWeights;
                Matrix4x4[] bindposes = mesh.sharedMesh.bindposes;
                if (vertices.Length != weights.Length || bindposes.Length != mesh.bones.Length)
                    throw new InvalidOperationException("Original skin contract changed.");
                // Compute the same weighted bone/bind-pose transformation the
                // skin uses. This avoids assuming BakeMesh's version-specific
                // renderer-scale convention at a mechanical contact boundary.
                Matrix4x4[] skinToWorld = mesh.bones.Select((bone, index) =>
                    bone.localToWorldMatrix * bindposes[index]).ToArray();
                var anchors = new HashSet<int>(Enumerable.Range(0, mesh.bones.Length).Where(index =>
                    mesh.bones[index] != null && (kind == "tail"
                        ? mesh.bones[index].name.StartsWith("cent_tail", StringComparison.Ordinal)
                        : mesh.bones[index].name == "cent_jaw1_jnt" || mesh.bones[index].name == "cent_head1_jnt")));
                float gap = float.MaxValue;
                Vector3 closest = Vector3.zero;
                int count = 0;
                float legacyGap = float.MaxValue;
                float transformedBakeGap = float.MaxValue;
                for (int index = 0; index < vertices.Length; index++)
                {
                    BoneWeight w = weights[index];
                    if (!(anchors.Contains(w.boneIndex0) && w.weight0 >= 0.25f ||
                        anchors.Contains(w.boneIndex1) && w.weight1 >= 0.25f ||
                        anchors.Contains(w.boneIndex2) && w.weight2 >= 0.25f ||
                        anchors.Contains(w.boneIndex3) && w.weight3 >= 0.25f)) continue;
                    Vector3 vertex = vertices[index];
                    Vector3 world = skinToWorld[w.boneIndex0].MultiplyPoint3x4(vertex) * w.weight0 +
                        skinToWorld[w.boneIndex1].MultiplyPoint3x4(vertex) * w.weight1 +
                        skinToWorld[w.boneIndex2].MultiplyPoint3x4(vertex) * w.weight2 +
                        skinToWorld[w.boneIndex3].MultiplyPoint3x4(vertex) * w.weight3;
                    Vector3 legacy = mesh.transform.position + mesh.transform.rotation * bakedVertices[index];
                    legacyGap = Math.Min(legacyGap, Vector3.Distance(legacy, bounds.ClosestPoint(legacy)));
                    Vector3 transformed = mesh.transform.TransformPoint(bakedVertices[index]);
                    transformedBakeGap = Math.Min(transformedBakeGap,
                        Vector3.Distance(transformed, bounds.ClosestPoint(transformed)));
                    float distance = Vector3.Distance(world, bounds.ClosestPoint(world));
                    if (distance < gap) { gap = distance; closest = world; }
                    count++;
                }
                var pose = CrocodilianAttackVisualPose.For(owner);
                row["visualContactPose"] = pose == null ? null : pose.Describe();
                row["mesh"] = mesh.sharedMesh.name;
                row["weightedVertices"] = count;
                row["nearestGapMeters"] = gap;
                row["legacyBakeGapMeters"] = legacyGap;
                row["transformedBakeGapMeters"] = transformedBakeGap;
                row["worldPoseMethod"] = "sum(weight * bone.localToWorld * bindpose * originalVertex)";
                row["rendererLossyScale"] = mesh.transform.lossyScale.ToString("F3");
                row["ownerPosition"] = owner.Position.ToString("F3");
                row["ownerRotation"] = owner.View.transform.eulerAngles.ToString("F3");
                row["rendererBoundsCenter"] = mesh.bounds.center.ToString("F3");
                row["rendererBoundsSize"] = mesh.bounds.size.ToString("F3");
                row["bakedBoundsSize"] = baked.bounds.size.ToString("F3");
                row["surfacePoint"] = closest.ToString("F3");
                row["targetBoundsCenter"] = bounds.center.ToString("F3");
                row["targetBoundsSize"] = bounds.size.ToString("F3");
                row["nativeAnimationContact"] = owner.Commands.Raw.OfType<UnitAttack>().Any(value =>
                    value.Animation != null && value.Animation.IsActed);
                row["contact"] = count > 0 && gap <= 0.25f;
            }
            catch (Exception exception) { row["exception"] = exception.ToString(); }
            finally { if (baked != null) UnityEngine.Object.Destroy(baked); }
        }

        private void CleanupSprint16CombatCell()
        {
            if (_crocCombatObserver != null) EventBus.Unsubscribe(_crocCombatObserver);
            _crocCombatObserver = null;
            if (_crocCombatOwner != null)
            {
                SummonGrabComponent grab = SummonGrabComponent.Find(_crocCombatOwner);
                if (grab != null) Sprint16Release(_crocCombatFixture, _crocCombatOwner, grab);
                TurnController turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                if (turn != null && ReferenceEquals(turn.Unit, _crocCombatOwner))
                {
                    turn.ForceToEnd(true);
                    typeof(TurnController).GetMethod("End", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance, null,
                        Type.EmptyTypes, null).Invoke(turn, null);
                }
                InterruptExpandedSummoningFixtureCommands(_crocCombatOwner);
                _crocCombatOwner.CombatState.LeaveCombat();
                if (_crocCombatCapitalMaster)
                    _crocCombatOwner.Descriptor.Master = _crocCombatMasterBefore;
                _crocCombatCapitalMaster = false;
                if (_crocCombatFactionChanged)
                {
                    _crocCombatOwner.Descriptor.SwitchFactions(_crocCombatFactionBefore, false);
                    _crocCombatOwner.AttackFactions.Match(_crocCombatAttackFactionsBefore);
                    if (!ReferenceEquals(_crocCombatOwner.Faction, _crocCombatFactionBefore) ||
                        !_crocCombatOwner.AttackFactions.SequenceEqual(_crocCombatAttackFactionsBefore))
                        throw new InvalidOperationException("Disposable manual faction restoration failed.");
                }
                _crocCombatFactionChanged = false;
                _crocCombatControlBefore = null;
                _crocCombatControlAfter = null;
                _crocCombatControlRules = 0;
                DisposeExpandedSummoningUnits(_crocCombatFixture.Created, new[] { _crocCombatOwner });
            }
            _crocCombatOwner = null;
        }

        private void CompleteSprint16Combat()
        {
            bool cleaned = false;
            try
            {
                CleanupSprint16CombatCell();
                if (_crocCombatFixture != null && _crocCombatFixture.Hostile != null)
                    _crocCombatFixture.Hostile.CombatState.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
                EndExpandedSummoningCorrectionFixture(_crocCombatFixture, out cleaned);
                if (_crocCombatAwakeBefore != null)
                {
                    Game.Instance.State.AwakeUnits.Clear();
                    Game.Instance.State.AwakeUnits.AddRange(_crocCombatAwakeBefore);
                }
                Game.Instance.Player.UpdateIsInCombat();
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = _crocCombatModeBefore;
                Game.Instance.TurnBasedCombatController.Activate();
                GroupController.Instance.SelectUnit(_crocCombatGroupBefore);
                Game.Instance.UI.SelectionManagerPC.MultiSelect(
                    _crocCombatSelectionBefore.Select(value => value.View).ToArray(), false);
                Game.Instance.Player.GameTime = _crocCombatTimeBefore;
                Game.Instance.IsPaused = _crocCombatPauseBefore;
                cleaned = cleaned && Game.Instance.State.AwakeUnits.SequenceEqual(_crocCombatAwakeBefore) &&
                    Game.Instance.UI.SelectionManagerPC.SelectedUnits.SequenceEqual(_crocCombatSelectionBefore) &&
                    ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), _crocCombatGroupBefore) &&
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue == _crocCombatModeBefore &&
                    Game.Instance.IsPaused == _crocCombatPauseBefore &&
                    Game.Instance.Player.GameTime == _crocCombatTimeBefore;
            }
            catch (Exception exception)
            {
                Sprint16Check(_crocodilianAssertions, _crocCombatRows, "combat-cleanup-exception", false,
                    new JObject { ["exception"] = DescribeExpandedSummoningCorrectionException(exception) },
                    "exact fixture, mode, clock, pause and awake-unit restoration");
            }
            Sprint16Check(_crocodilianAssertions, _crocCombatRows, "combat-cleanup", cleaned,
                new JObject { ["cleaned"] = cleaned, ["cells"] = _crocCombatCell,
                    ["awakeRestored"] = Game.Instance.State.AwakeUnits.SequenceEqual(_crocCombatAwakeBefore),
                    ["modeRestored"] = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue == _crocCombatModeBefore,
                    ["pauseRestored"] = Game.Instance.IsPaused == _crocCombatPauseBefore,
                    ["clockRestored"] = Game.Instance.Player.GameTime == _crocCombatTimeBefore,
                    ["selectionRestored"] = Game.Instance.UI.SelectionManagerPC.SelectedUnits.SequenceEqual(_crocCombatSelectionBefore),
                    ["groupRestored"] = ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), _crocCombatGroupBefore),
                    ["unitCensusRestored"] = _crocCombatFixture != null &&
                        SameReferences(_crocCombatFixture.UnitsBefore, SnapshotReferences(_crocCombatFixture.AllUnits)),
                    ["partyCensusRestored"] = _crocCombatFixture != null &&
                        SameReferences(_crocCombatFixture.PartyBefore, SnapshotReferences(_crocCombatFixture.Party)),
                    ["originalUnitsLost"] = _crocCombatFixture == null ? null : new JArray(
                        _crocCombatFixture.UnitsBefore.OfType<UnitEntityData>().Where(value => value.Destroyed ||
                            !ContainsReference(_crocCombatFixture.AllUnits, value)).Select(value => value.UniqueId)),
                    ["unexpectedUnits"] = _crocCombatFixture == null ? null : new JArray(
                        SnapshotReferences(_crocCombatFixture.AllUnits).OfType<UnitEntityData>()
                            .Where(value => !_crocCombatFixture.UnitsBefore.Any(prior => ReferenceEquals(prior, value)))
                            .Select(value => value.Blueprint.name + ":" + value.UniqueId)) },
                "exact fixture, mode, clock, pause and awake-unit restoration");
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory, "sprint16-combat.json"),
                _crocCombatRows.ToString(Formatting.Indented));
            _sprint16CombatComplete = true;
        }
    }
}
