using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Summoning;
using TurnBased.Controllers;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 14's 7D leg: all three insects attacking through the command the
    /// player's click produces, in both of the game's combat modes.
    ///
    /// <para>Everything before this drove the rules directly - a
    /// RuleAttackWithWeapon built by the fixture and triggered - which proves
    /// what a rule does and not what the game does with it. A player never
    /// triggers a rule. They order an attack, and a UnitAttack command plans
    /// the hands, waits on an animation, fires each attack as its contact
    /// lands, writes the combat log and spends an action. Every one of those
    /// steps is somewhere a correct rule can still produce a wrong creature:
    /// a sting that never gets planned into a full attack, a poison that
    /// arrives on the wrong limb's contact, an attack the log never names.</para>
    ///
    /// <para>So each cell here queues a real command on a real unit and lets
    /// the game run it. The two modes matter because they are different
    /// machines: RTWP ticks commands on world time, while turn-based gives the
    /// unit a turn and an action budget, and a creature that attacks correctly
    /// in one can be mute in the other. The soldier's cells force a full attack,
    /// because bite-and-sting separation only exists in a sequence of more than
    /// one attack, and a single attack would hide the thing being tested.</para>
    ///
    /// <para>A cell that cannot even enter its mode records that and fails its
    /// own row rather than throwing, because an exception here would cost the
    /// whole batch its remaining scenarios for a fact about one cell.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// (creature key, turn-based) for every cell, in run order.
        /// </summary>
        private static readonly string[][] Sprint14CombatCells =
        {
            new[] { "fire-beetle", "false" },
            new[] { "fire-beetle", "true" },
            new[] { "giant-ant-worker", "false" },
            new[] { "giant-ant-worker", "true" },
            new[] { "giant-ant-soldier", "false" },
            new[] { "giant-ant-soldier", "true" }
        };

        private int _sprint14CombatCell;
        private readonly List<string> _sprint14CombatRows = new List<string>();
        private readonly List<string> _sprint14CombatSteps = new List<string>();
        private UnitEntityData _sprint14CombatUnit;
        private UnitAttack _sprint14CombatCommand;
        private ExpandedSummoningAttackRollObserver _sprint14CombatObserver;
        private BlueprintBuff _sprint14CombatVenom;
        private bool _sprint14CombatTurnModeBefore;
        private bool _sprint14CombatModeCaptured;
        private bool _sprint14CombatSubscribed;
        private bool _sprint14CombatJoined;
        private bool _sprint14CombatActedOnce;
        private int _sprint14CombatActedFrame = -1;
        private int _sprint14CombatFirstRollFrame = -1;
        private string _sprint14CombatEntry = "not-entered";
        private readonly UnitCombatJoinController _sprint14CombatJoin =
            new UnitCombatJoinController();
        private readonly UnitCombatPrepareController _sprint14CombatPrepare =
            new UnitCombatPrepareController();
        private readonly HashSet<TurnController> _sprint14CombatPrepared =
            new HashSet<TurnController>();

        private const BindingFlags Sprint14CombatMembers =
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic;

        /// <summary>
        /// Set the mode this cell needs, enrol exactly the insect and the
        /// hostile, and queue the real command.
        /// </summary>
        private void BeginSprint14CombatCell()
        {
            UnitEntityData hostile = _rulesFixture.Hostile;
            string key = Sprint14CombatCells[_sprint14CombatCell][0];
            bool turnBased = Sprint14CombatCells[_sprint14CombatCell][1] ==
                "true";
            if (!_sprint14CombatModeCaptured)
            {
                _sprint14CombatTurnModeBefore = SettingsRoot.Instance
                    .EnableTurnBasedMode.CurrentValue;
                _sprint14CombatModeCaptured = true;
                _sprint14CombatVenom = _rulesFixture.Blueprints
                    .OfType<BlueprintBuff>().FirstOrDefault(value =>
                        value != null && value.name ==
                        "KMG_Summoning_Natural_GiantAnt_Venom");
            }
            _sprint14CombatActedOnce = false;
            _sprint14CombatActedFrame = -1;
            _sprint14CombatFirstRollFrame = -1;
            _sprint14CombatEntry = "not-entered";

            _sprint14CombatUnit = CastExpandedSummoningOwnTier(_rulesFixture,
                key);
            // The fixture's creatures are cast at level 1 initiative and never
            // fight anything; without a usable attack bonus a legitimate miss
            // would read exactly like a command that never fired.
            _sprint14CombatUnit.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            _sprint14CombatObserver = new ExpandedSummoningAttackRollObserver {
                Initiator = _sprint14CombatUnit };
            EventBus.Subscribe(_sprint14CombatObserver);

            var used = new List<int>();
            string chosen;
            PlaceExpandedSummoningUnit(_sprint14CombatUnit,
                ExpandedSummoningOpenPoint(hostile.Position, 2.5f, used,
                    out chosen));
            if (_rulesAwakeSnapshot == null)
                _rulesAwakeSnapshot = Game.Instance.State.AwakeUnits.ToArray();
            foreach (UnitEntityData unit in
                new[] { _sprint14CombatUnit, hostile })
                if (!Game.Instance.State.AwakeUnits.Contains(unit))
                    Game.Instance.State.AwakeUnits.Add(unit);

            if (_sprint14CombatVenom != null)
                hostile.Descriptor.Buffs.RemoveFact(_sprint14CombatVenom);

            _sprint14CombatEntry = EnterSprint14CombatMode(turnBased,
                _sprint14CombatUnit, hostile);

            _sprint14CombatCommand = new UnitAttack(hostile);
            _sprint14CombatCommand.Init(_sprint14CombatUnit);
            // A soldier's two limbs only separate inside a sequence; a single
            // attack would prove nothing about which one carries the venom.
            if (key == "giant-ant-soldier")
                _sprint14CombatCommand.ForceFullAttack = true;
            _sprint14CombatSteps.Add(key + "/" +
                (turnBased ? "turn-based" : "rtwp") + ":placed=" + chosen +
                ";entry=" + _sprint14CombatEntry + ";canStart=" +
                _sprint14CombatCommand.CanStart);
            _sprint14CombatUnit.Commands.Run(_sprint14CombatCommand);
        }

        /// <summary>
        /// Put the game into the mode this cell names and enrol exactly the two
        /// actors, saying plainly what happened either way.
        ///
        /// <para>Turn-based is the part that can refuse. The controller is
        /// enabled and subscribed before the actors join so the join rolls
        /// initiative under turn-based rules, and then the turn loop is walked
        /// until the insect is the acting unit - other units' turns are ended,
        /// which is what a player waiting for their creature's turn does.</para>
        /// </summary>
        private string EnterSprint14CombatMode(bool turnBased,
            UnitEntityData unit, UnitEntityData hostile)
        {
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    turnBased;
                Game.Instance.TurnBasedCombatController.Activate();
                if (turnBased && !_sprint14CombatSubscribed)
                {
                    EventBus.Subscribe(Game.Instance.TurnBasedCombatController);
                    _sprint14CombatSubscribed = true;
                }
                if (!_sprint14CombatJoined)
                {
                    unit.JoinCombat();
                    hostile.JoinCombat();
                    Game.Instance.Player.UpdateIsInCombat();
                    _sprint14CombatJoin.Tick();
                    _sprint14CombatPrepare.Tick();
                    _sprint14CombatJoined = true;
                }
                else
                {
                    unit.JoinCombat();
                    _sprint14CombatJoin.Tick();
                    _sprint14CombatPrepare.Tick();
                }
                Game.Instance.IsPaused = false;
                bool inTurnBased = CombatController.IsInTurnBasedCombat();
                if (!turnBased)
                    return "rtwp:inTurnBased=" + inTurnBased + ";inCombat=" +
                        Game.Instance.Player.IsInCombat;
                if (!inTurnBased)
                    return "turn-based-refused:inCombat=" +
                        Game.Instance.Player.IsInCombat;
                return "turn-based:" + AdvanceSprint14CombatToActor(unit);
            }
            catch (Exception exception)
            {
                return "entry-exception:" +
                    DescribeExpandedSummoningCorrectionException(exception);
            }
        }

        /// <summary>
        /// Walk the native turn loop until the given unit is the acting one.
        /// </summary>
        private string AdvanceSprint14CombatToActor(UnitEntityData unit)
        {
            var controller = Game.Instance.TurnBasedCombatController;
            var visited = new List<string>();
            for (int guard = 0; guard < 24; guard++)
            {
                TurnController turn = controller.CurrentTurn;
                if (turn == null)
                {
                    controller.Tick();
                    visited.Add("no-turn");
                    continue;
                }
                if (turn.Status == TurnController.TurnStatus.None &&
                    _sprint14CombatPrepared.Add(turn))
                    turn.Prepare();
                bool acting =
                    turn.Status == TurnController.TurnStatus.Preparing ||
                    turn.Status == TurnController.TurnStatus.Acting;
                if (ReferenceEquals(turn.Unit, unit) && acting)
                    return "round=" + controller.RoundNumber + ";actor=" +
                        (turn.Unit.Blueprint == null ? "<none>" :
                            turn.Unit.Blueprint.name) + ";status=" +
                        turn.Status + ";turnsSkipped=" + visited.Count;
                visited.Add(turn.Unit == null ? "<null>" :
                    (turn.Unit.Blueprint == null ? "<none>" :
                        turn.Unit.Blueprint.name) + ":" + turn.Status);
                turn.ForceToEnd(true);
                MethodInfo end = typeof(TurnController).GetMethod("End",
                    Sprint14CombatMembers, null, Type.EmptyTypes, null);
                if (end != null) end.Invoke(turn, null);
                controller.Tick();
            }
            return "never-reached-actor;turns=" +
                string.Join("/", visited.ToArray());
        }

        /// <summary>
        /// One frame of the running command; true once the cell is done.
        /// </summary>
        private bool FinishSprint14CombatCell(int frames)
        {
            if (_sprint14CombatCommand == null) return true;
            // The animation's contact is what the command waits on before it
            // fires an attack, so the frame it is acted on is recorded and the
            // first rule event must not precede it. That ordering is the
            // mandible or stinger actually landing on the target rather than a
            // rule that fired on its own schedule.
            if (_sprint14CombatCommand.IsRunning)
            {
                if (_sprint14CombatCommand.Animation != null && frames > 8 &&
                    !_sprint14CombatActedOnce)
                {
                    _sprint14CombatCommand.Animation.IsActed = true;
                    _sprint14CombatActedOnce = true;
                    _sprint14CombatActedFrame = frames;
                }
                _sprint14CombatCommand.Tick();
            }
            else if (!_sprint14CombatCommand.IsStarted &&
                !_sprint14CombatCommand.IsFinished &&
                _sprint14CombatCommand.IsUnitEnoughClose)
                _sprint14CombatCommand.Start();
            if (_sprint14CombatFirstRollFrame < 0 &&
                _sprint14CombatObserver.Rolls.Count > 0)
                _sprint14CombatFirstRollFrame = frames;
            bool done = _sprint14CombatCommand.IsFinished &&
                _sprint14CombatObserver.Rolls.Count > 0;
            return done || frames >= ExpandedSummoningCommandFrames;
        }

        /// <summary>
        /// Read the cell's outcome, release what it owned, and say whether it
        /// met the criteria its creature has.
        /// </summary>
        private void CompleteSprint14CombatCell(int frames)
        {
            string key = Sprint14CombatCells[_sprint14CombatCell][0];
            bool turnBased = Sprint14CombatCells[_sprint14CombatCell][1] ==
                "true";
            UnitEntityData hostile = _rulesFixture.Hostile;
            string[] rolls = _sprint14CombatObserver == null ?
                new string[0] : _sprint14CombatObserver.Rolls.ToArray();
            string[] weapons = rolls
                .Select(value => ExtractSprint14Field(value, "weapon="))
                .Where(value => value != null).ToArray();
            string[] distinct = weapons.Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            bool everyRollLogged = rolls.Length > 0 &&
                rolls.All(value => value.Contains(",logged=True"));
            bool contactBeforeRules = _sprint14CombatFirstRollFrame < 0 ||
                _sprint14CombatActedFrame < 0 ||
                _sprint14CombatFirstRollFrame >= _sprint14CombatActedFrame;

            // The venom is a sting effect. It may be present only when a sting
            // actually wounded, whatever the bite did.
            string stingName = Sprint14StingWeaponName(_sprint14CombatUnit);
            int stingHits = rolls.Count(value =>
                stingName != null &&
                ExtractSprint14Field(value, "weapon=") == stingName &&
                value.Contains(",hit=True"));
            int biteHits = rolls.Count(value =>
                stingName != null &&
                ExtractSprint14Field(value, "weapon=") != stingName &&
                value.Contains(",hit=True"));
            bool venomPresent = _sprint14CombatVenom != null &&
                hostile.Descriptor.Buffs.GetBuff(_sprint14CombatVenom) != null;
            bool noCrossDelivery = stingName == null ||
                venomPresent == stingHits > 0;

            // A grab bonus is specific to the grapple; the same creature's trip
            // gets none of it, so the difference between the two is the bonus
            // itself without disturbing a single fact on the unit.
            int grappleCmb = 0, tripCmb = 0;
            if (_sprint14CombatUnit != null)
            {
                grappleCmb = Rulebook.Trigger(new RuleCalculateCMB(
                    _sprint14CombatUnit, hostile,
                    CombatManeuver.Grapple)).Result;
                tripCmb = Rulebook.Trigger(new RuleCalculateCMB(
                    _sprint14CombatUnit, hostile,
                    CombatManeuver.Trip)).Result;
            }
            int grabBonus = grappleCmb - tripCmb;
            bool grabExact = key != "giant-ant-soldier" ? grabBonus == 0 :
                grabBonus == ExpandedSummoningSpecialProfiles
                    .SummonGrabManeuverBonus;

            bool commandRan = _sprint14CombatCommand != null &&
                _sprint14CombatCommand.IsStarted && rolls.Length > 0;
            bool modeEntered = turnBased ?
                _sprint14CombatEntry.StartsWith("turn-based:",
                    StringComparison.Ordinal) :
                _sprint14CombatEntry.StartsWith("rtwp:",
                    StringComparison.Ordinal);
            // A worker has one limb and must never produce two; a soldier has
            // two and must produce both, separately named.
            bool separation = key == "giant-ant-soldier" ?
                distinct.Length >= 2 && stingHits + biteHits >= 2 :
                distinct.Length == 1;

            bool ok = commandRan && modeEntered && everyRollLogged &&
                contactBeforeRules && noCrossDelivery && grabExact &&
                separation;
            _sprint14CombatRows.Add(key + "/" +
                (turnBased ? "turn-based" : "rtwp") + "[entry=" +
                _sprint14CombatEntry + ";started=" +
                (_sprint14CombatCommand != null &&
                    _sprint14CombatCommand.IsStarted) + ";finished=" +
                (_sprint14CombatCommand != null &&
                    _sprint14CombatCommand.IsFinished) + ";result=" +
                (_sprint14CombatCommand == null ? "<none>" :
                    _sprint14CombatCommand.Result.ToString()) + ";frames=" +
                frames + ";attacks=" + rolls.Length + ";weapons=" +
                string.Join("+", distinct) + ";logged=" + everyRollLogged +
                ";contactFrame=" + _sprint14CombatActedFrame +
                ";firstRuleFrame=" + _sprint14CombatFirstRollFrame +
                ";contactBeforeRules=" + contactBeforeRules +
                ";stingHits=" + stingHits + ";biteHits=" + biteHits +
                ";venom=" + venomPresent + ";noCrossDelivery=" +
                noCrossDelivery + ";grappleCmb=" + grappleCmb + ";tripCmb=" +
                tripCmb + ";grabBonus=" + grabBonus + ";grabExact=" +
                grabExact + ";separation=" + separation + ";rolls=" +
                string.Join(" | ", rolls) + "]" + (ok ? "=ok" : "=wrong"));

            if (_sprint14CombatObserver != null)
                EventBus.Unsubscribe(_sprint14CombatObserver);
            _sprint14CombatObserver = null;
            if (_sprint14CombatCommand != null &&
                !_sprint14CombatCommand.IsFinished)
                _sprint14CombatCommand.Interrupt();
            _sprint14CombatCommand = null;
            if (_sprint14CombatUnit != null)
                InterruptExpandedSummoningFixtureCommands(_sprint14CombatUnit);
            if (_sprint14CombatVenom != null)
                hostile.Descriptor.Buffs.RemoveFact(_sprint14CombatVenom);
            _sprint14CombatUnit = null;
        }

        /// <summary>The soldier's sting, or null for a creature without one.</summary>
        private static string Sprint14StingWeaponName(UnitEntityData unit)
        {
            if (unit == null || unit.Blueprint == null ||
                unit.Blueprint.Body == null) return null;
            Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon[] limbs =
                unit.Blueprint.Body.AdditionalLimbs;
            return limbs == null || limbs.Length != 1 || limbs[0] == null ?
                null : limbs[0].name;
        }

        private static string ExtractSprint14Field(string row, string field)
        {
            if (row == null) return null;
            int start = row.IndexOf(field, StringComparison.Ordinal);
            if (start < 0) return null;
            start += field.Length;
            int end = row.IndexOf(',', start);
            return end < 0 ? row.Substring(start) :
                row.Substring(start, end - start);
        }

        /// <summary>
        /// Leave both modes as they were found and say so.
        /// </summary>
        private void CompleteSprint14CombatModes()
        {
            string restored = "not-restored";
            try
            {
                if (_sprint14CombatModeCaptured)
                {
                    TurnController turn = Game.Instance
                        .TurnBasedCombatController.CurrentTurn;
                    if (turn != null)
                    {
                        turn.ForceToEnd(true);
                        MethodInfo end = typeof(TurnController).GetMethod(
                            "End", Sprint14CombatMembers, null,
                            Type.EmptyTypes, null);
                        if (end != null) end.Invoke(turn, null);
                    }
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                        _sprint14CombatTurnModeBefore;
                    Game.Instance.TurnBasedCombatController.Activate();
                    if (_sprint14CombatSubscribed)
                    {
                        EventBus.Unsubscribe(
                            Game.Instance.TurnBasedCombatController);
                        _sprint14CombatSubscribed = false;
                    }
                    if (_rulesFixture != null &&
                        _rulesFixture.Hostile != null)
                        _rulesFixture.Hostile.CombatState.LeaveCombat();
                    Game.Instance.Player.UpdateIsInCombat();
                    restored = "turnBasedRestoredTo=" +
                        _sprint14CombatTurnModeBefore + ";inTurnBased=" +
                        CombatController.IsInTurnBasedCombat() + ";inCombat=" +
                        Game.Instance.Player.IsInCombat;
                }
            }
            catch (Exception exception)
            {
                restored = "restore-exception:" +
                    DescribeExpandedSummoningCorrectionException(exception);
            }

            bool allCellsOk = _sprint14CombatRows.Count ==
                Sprint14CombatCells.Length &&
                _sprint14CombatRows.All(value => value.EndsWith("=ok",
                    StringComparison.Ordinal)) &&
                restored.StartsWith("turnBasedRestoredTo=",
                    StringComparison.Ordinal) &&
                !CombatController.IsInTurnBasedCombat();
            _rulesCases.Add(Assertion(
                "expanded-summoning-sprint14-combat-modes",
                "all three insects attack through the command a player's click produces, in RTWP and in turn-based combat: the command is queued on the unit, started by the game, waits on its own animation's contact and fires each attack after it, every attack reaches the combat log, the worker produces exactly one named weapon and the soldier's forced full attack produces its bite and its sting as separately named attacks, the soldier's grapple carries exactly the +4 grab that its trip does not, the venom is present only when a sting wounded, and both modes are left as they were found",
                "cells=" + _sprint14CombatRows.Count + "/" +
                    Sprint14CombatCells.Length + ";restored=" + restored +
                    ";" + string.Join(";", _sprint14CombatRows.ToArray()) +
                    ";steps=" + string.Join(" | ",
                        _sprint14CombatSteps.ToArray()),
                allCellsOk,
                "UnitAttack queued through UnitEntityData.Commands.Run and ticked by the game, UnitAttack.ForceFullAttack, the command's own UnitAnimationActionHandle contact, RuleAttackRoll records with SuspendCombatLog, RuleCalculateCMB grapple against trip, and the native turn loop's TurnController.Prepare/End"));
        }
    }
}
