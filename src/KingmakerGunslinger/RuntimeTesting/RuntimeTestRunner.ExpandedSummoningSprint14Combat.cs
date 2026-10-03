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
        private bool _sprint14CombatJoined;
        private int _sprint14CombatActings;
        private int _sprint14CombatActedFrame = -1;
        private int _sprint14CombatGrabBonusBefore;
        private bool _sprint14CombatSoldierSeparationShown;
        private bool _sprint14CombatQueued;
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
            _sprint14CombatActings = 0;
            _sprint14CombatActedFrame = -1;
            _sprint14CombatFirstRollFrame = -1;
            _sprint14CombatEntry = "not-entered";
            _sprint14CombatQueued = false;
            _sprint14CombatTurnsSeen.Clear();

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

            // Measured now, before anything is held. A grab bonus applies
            // to the grapple and not to the trip, so the difference between
            // the two is the bonus itself - but only while nothing is held,
            // because a creature already holding its target also carries the
            // separate +5 to maintain, and the first version of this read the
            // sum of the two as a wrong answer.
            _sprint14CombatGrabBonusBefore =
                Rulebook.Trigger(new RuleCalculateCMB(_sprint14CombatUnit,
                    hostile, CombatManeuver.Grapple)).Result -
                Rulebook.Trigger(new RuleCalculateCMB(_sprint14CombatUnit,
                    hostile, CombatManeuver.Trip)).Result;

            _sprint14CombatEntry = EnterSprint14CombatMode(turnBased,
                _sprint14CombatUnit, hostile);
            _sprint14CombatSteps.Add(key + "/" +
                (turnBased ? "turn-based" : "rtwp") + ":placed=" + chosen +
                ";entry=" + _sprint14CombatEntry + ";grabBonusBefore=" +
                _sprint14CombatGrabBonusBefore);
        }

        /// <summary>
        /// Wait for this cell's mode to be ready to take a command, then queue
        /// it. True once the command is queued or the wait is given up on.
        ///
        /// <para>RTWP is ready at once. Turn-based is not, and the first
        /// version of this failed for a reason worth keeping: it walked the
        /// native turn loop by calling the controller's Tick two dozen times
        /// inside a single frame, and CurrentTurn was null for every one of
        /// them. Initiative and the first turn need real frames. So this is
        /// asked once per frame and the game's own update does the advancing
        /// in between, which is also what a player waiting for their
        /// creature's turn actually does. Other units' turns are ended as they
        /// come up, because the insect's turn is the one being tested.</para>
        /// </summary>
        private bool AdvanceSprint14CombatEntry(int frames)
        {
            if (_sprint14CombatQueued) return true;
            bool turnBased = Sprint14CombatCells[_sprint14CombatCell][1] ==
                "true";
            if (turnBased && _sprint14CombatEntry.StartsWith("turn-based",
                    StringComparison.Ordinal) &&
                !_sprint14CombatEntry.StartsWith("turn-based-refused",
                    StringComparison.Ordinal))
            {
                string ready = StepSprint14TurnLoop();
                if (ready == null)
                {
                    if (frames < Sprint14TurnEntryFrames) return false;
                    _sprint14CombatEntry = "turn-based:never-reached-actor;" +
                        "waited=" + frames + ";turns=" + string.Join("/",
                            _sprint14CombatTurnsSeen.Distinct().ToArray());
                }
                else _sprint14CombatEntry = "turn-based:" + ready;
            }
            QueueSprint14CombatCommand();
            return true;
        }

        /// <summary>
        /// How long a cell waits for its creature's turn. Six seconds of frames
        /// is far more than initiative needs and still bounded.
        /// </summary>
        private const int Sprint14TurnEntryFrames = 360;

        private readonly List<string> _sprint14CombatTurnsSeen =
            new List<string>();

        /// <summary>
        /// One frame's look at the native turn loop. Returns a description once
        /// the insect is the acting unit, or null while still waiting.
        /// </summary>
        private string StepSprint14TurnLoop()
        {
            var controller = Game.Instance.TurnBasedCombatController;
            _sprint14CombatJoin.Tick();
            _sprint14CombatPrepare.Tick();
            TurnController turn = controller.CurrentTurn;
            if (turn == null)
            {
                _sprint14CombatTurnsSeen.Add("no-turn");
                return null;
            }
            if (turn.Status == TurnController.TurnStatus.None &&
                _sprint14CombatPrepared.Add(turn))
                turn.Prepare();
            bool acting = turn.Status == TurnController.TurnStatus.Preparing ||
                turn.Status == TurnController.TurnStatus.Acting;
            if (ReferenceEquals(turn.Unit, _sprint14CombatUnit) && acting)
                return "round=" + controller.RoundNumber + ";status=" +
                    turn.Status + ";turnsWaited=" +
                    _sprint14CombatTurnsSeen.Count;
            _sprint14CombatTurnsSeen.Add((turn.Unit == null ? "<null>" :
                turn.Unit.Blueprint == null ? "<none>" :
                turn.Unit.Blueprint.name) + ":" + turn.Status);
            if (!ReferenceEquals(turn.Unit, _sprint14CombatUnit) && acting)
            {
                turn.ForceToEnd(true);
                MethodInfo end = typeof(TurnController).GetMethod("End",
                    Sprint14CombatMembers, null, Type.EmptyTypes, null);
                if (end != null) end.Invoke(turn, null);
            }
            return null;
        }

        /// <summary>Queue the real command on the real unit.</summary>
        private void QueueSprint14CombatCommand()
        {
            string key = Sprint14CombatCells[_sprint14CombatCell][0];
            _sprint14CombatCommand = new UnitAttack(_rulesFixture.Hostile);
            _sprint14CombatCommand.Init(_sprint14CombatUnit);
            // A soldier's two limbs only separate inside a sequence; a single
            // attack would prove nothing about which one carries the venom.
            if (key == "giant-ant-soldier")
                _sprint14CombatCommand.ForceFullAttack = true;
            _sprint14CombatSteps.Add(key + ":canStart=" +
                _sprint14CombatCommand.CanStart);
            _sprint14CombatUnit.Commands.Run(_sprint14CombatCommand);
            _sprint14CombatQueued = true;
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
                // Deliberately not subscribing the turn-based controller to
                // the EventBus. Where that mod is enabled it holds its own
                // subscription, and a second one delivers every event twice,
                // which would advance turns at double rate and make this
                // fixture the cause of what it is measuring.
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
                // The walk itself happens a frame at a time from the
                // machine; entry only has to get the mode and the enrolment
                // right.
                return "turn-based:entered";
            }
            catch (Exception exception)
            {
                return "entry-exception:" +
                    DescribeExpandedSummoningCorrectionException(exception);
            }
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
                // Every attack in a sequence has its own contact, and the
                // first version acted one animation and then latched a flag,
                // so a forced full attack landed its first attack and then
                // waited forever on a contact nothing would act - which is
                // the Interrupt the soldier's RTWP cell recorded. The frame of
                // the first acting is still what the ordering check uses.
                if (_sprint14CombatCommand.Animation != null && frames > 8 &&
                    !_sprint14CombatCommand.Animation.IsActed)
                {
                    _sprint14CombatCommand.Animation.IsActed = true;
                    _sprint14CombatActings++;
                    if (_sprint14CombatActedFrame < 0)
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

            // The bonus that matters was measured before the attack, with
            // nothing held. The value after it is recorded too, because a
            // soldier that established a hold legitimately carries the +5
            // maintain on top of the +4 grab and reads 9 - which is how the
            // first version of this produced a wrong answer out of two right
            // numbers.
            int grabBonus = _sprint14CombatGrabBonusBefore;
            int grabBonusAfter = 0;
            if (_sprint14CombatUnit != null)
                grabBonusAfter =
                    Rulebook.Trigger(new RuleCalculateCMB(
                        _sprint14CombatUnit, hostile,
                        CombatManeuver.Grapple)).Result -
                    Rulebook.Trigger(new RuleCalculateCMB(
                        _sprint14CombatUnit, hostile,
                        CombatManeuver.Trip)).Result;
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
            //
            // The first version required both of the soldier's attacks to
            // *hit*, which is the dice and not the rule: its RTWP cell planned
            // and fired both, named them separately, delivered the venom from
            // the sting alone - and failed, because the bite happened to miss.
            // What the contract asks for is records that identify both
            // attacks, so the count that matters is attacks made.
            //
            // The other case is engine behaviour rather than a defect. In
            // turn-based the soldier's bite hit, its grab took hold, and the
            // sequence ended after that one attack: a creature that has just
            // seized its target does not go on swinging. A cell cut short that
            // way is accepted and says so, and the separation still has to be
            // demonstrated somewhere - the closing assertion requires at least
            // one soldier cell to have produced both attacks in one sequence,
            // so this cannot pass with the grab as an excuse every time.
            bool grabCutSequenceShort = key == "giant-ant-soldier" &&
                rolls.Length == 1 && biteHits >= 1 &&
                grabBonusAfter > grabBonus;
            bool separation;
            if (key != "giant-ant-soldier") separation = distinct.Length == 1;
            else
            {
                bool bothAttacksMade = distinct.Length >= 2 &&
                    rolls.Length >= 2;
                if (bothAttacksMade)
                    _sprint14CombatSoldierSeparationShown = true;
                separation = bothAttacksMade || grabCutSequenceShort;
            }

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
                ";actings=" + _sprint14CombatActings +
                ";stingHits=" + stingHits + ";biteHits=" + biteHits +
                ";venom=" + venomPresent + ";noCrossDelivery=" +
                noCrossDelivery + ";grabBonusBeforeAttack=" + grabBonus +
                ";grabBonusAfterAttack=" + grabBonusAfter + ";grabExact=" +
                grabExact + ";separation=" + separation +
                ";grabCutSequenceShort=" + grabCutSequenceShort + ";rolls=" +
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
            // The body has to go before the next cell casts. Every cell is
            // placed at the same open point beside the hostile, and the first
            // version left each creature standing there: cells one and two
            // worked, and from the third onwards the spot was occupied, so the
            // new insect never got close enough for the game to start its
            // command. All four of those cells recorded canStart=True and
            // started=False, which is what being unable to reach the target
            // looks like from outside.
            if (_sprint14CombatUnit != null)
                DisposeExpandedSummoningUnits(_rulesFixture.Created,
                    new[] { _sprint14CombatUnit });
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
                    if (_rulesFixture != null &&
                        _rulesFixture.Hostile != null)
                        _rulesFixture.Hostile.CombatState.LeaveCombat();
                    Game.Instance.Player.UpdateIsInCombat();
                    // What this fixture owns is the mode setting it
                    // changed and the enrolment it created. Whether the game
                    // still reports turn-based combat depends on the setting
                    // it found - which was already true here - so asserting
                    // that flag is off would demand a state the fixture never
                    // had and must not impose.
                    restored = "turnBasedRestoredTo=" +
                        _sprint14CombatTurnModeBefore + ";settingNow=" +
                        SettingsRoot.Instance.EnableTurnBasedMode
                            .CurrentValue + ";hostileInCombat=" +
                        (_rulesFixture.Hostile.CombatState != null &&
                            _rulesFixture.Hostile.CombatState.IsInCombat) +
                        ";inTurnBased=" +
                        CombatController.IsInTurnBasedCombat();
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
                restored.Contains(";settingNow=" +
                    _sprint14CombatTurnModeBefore) &&
                restored.Contains(";hostileInCombat=False") &&
                // Somewhere, in one mode or the other, the soldier's forced
                // full attack has to have actually produced both of its
                // attacks in one sequence. Accepting a grab-shortened cell is
                // only reasonable while that stays true elsewhere.
                _sprint14CombatSoldierSeparationShown;
            _rulesCases.Add(Assertion(
                "expanded-summoning-sprint14-combat-modes",
                "all three insects attack through the command a player's click produces, in RTWP and in turn-based combat: the command is queued on the unit, started by the game, waits on its own animation's contact and fires each attack after it, every attack reaches the combat log, the worker produces exactly one named weapon and the soldier's forced full attack produces its bite and its sting as separately named attacks - counting attacks made rather than attacks that hit, since which of them lands is the dice - with a sequence cut short by the soldier's own grab accepted as the engine behaviour it is, provided at least one of its two cells still produced both attacks in one sequence; the soldier's grapple carries exactly the +4 grab that its trip does not, measured before anything is held so the separate +5 to maintain cannot be mistaken for it; the venom is present only when a sting wounded; and both modes are left as they were found",
                "cells=" + _sprint14CombatRows.Count + "/" +
                    Sprint14CombatCells.Length +
                    ";soldierSeparationShown=" +
                    _sprint14CombatSoldierSeparationShown +
                    ";restored=" + restored +
                    ";" + string.Join(";", _sprint14CombatRows.ToArray()) +
                    ";steps=" + string.Join(" | ",
                        _sprint14CombatSteps.ToArray()),
                allCellsOk,
                "UnitAttack queued through UnitEntityData.Commands.Run and ticked by the game, UnitAttack.ForceFullAttack, the command's own UnitAnimationActionHandle contact, RuleAttackRoll records with SuspendCombatLog, RuleCalculateCMB grapple against trip, and the native turn loop's TurnController.Prepare/End"));
        }
    }
}
