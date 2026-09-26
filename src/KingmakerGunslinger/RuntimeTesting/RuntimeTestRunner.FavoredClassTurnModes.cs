using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>Records the attacks and maneuvers the native commands make.</summary>
        private sealed class FcbTurnModeRuleObserver : IGlobalRulebookHandler<RuleAttackWithWeapon>,
            IGlobalRulebookHandler<RuleCombatManeuver>
        {
            internal readonly List<RuleAttackWithWeapon> Attacks = new List<RuleAttackWithWeapon>();
            internal readonly List<RuleCombatManeuver> Maneuvers = new List<RuleCombatManeuver>();

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (evt != null) Attacks.Add(evt);
            }

            public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }

            public void OnEventDidTrigger(RuleCombatManeuver evt)
            {
                if (evt != null) Maneuvers.Add(evt);
            }
        }

        private static readonly string[] FcbTurnModeSections =
            { "initiative", "dodge", "pistolWhip", "demoralize", "grit" };

        // L07 with M10, M12 and M13: the Gunslinger's Initiative, Dodge and
        // Pistol-Whip deeds with their favored-class improvements, grit across
        // rounds and the Rogue's demoralize, through the native combat entry
        // and real native commands, in real time and in a fresh turn-based
        // combat whose initiative is rolled under turn-based rules.
        private RuntimeTestResult RunFavoredClassTurnModes()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                leaves != null && gunslinger != null && FavoredClassRuntime.MechanicsEnabled &&
                !Game.Instance.State.Units.All.Any() && !Game.Instance.Player.Party.Any() &&
                Game.Instance.CurrentlyLoadedArea == null;
            assertions.Add(Assertion("fcb-integration-published",
                "the exact qualified host is ready, mechanics are enabled and the save-free world is empty",
                status.ToString(), ready, "FavoredClassIntegrationStatusRegistry; empty main-menu world"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);

            var evidence = new JObject();
            var failures = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string mode in new[] { "rt", "tb" })
                foreach (string section in FcbTurnModeSections)
                    failures[section + "-" + mode] = new List<string>();
            failures["cleanup"] = new List<string>();
            FirearmInputSaveGuard guard = null;
            var tutorial = BlueprintRoot.Instance.UITutorials.TBMStarted.Lock;
            bool tutorialLocked = tutorial != null && tutorial.IsLocked;
            var flagsBefore = Game.Instance.Player.UnlockableFlags.UnlockedFlags.ToArray();
            try
            {
                if (tutorial == null) throw new InvalidOperationException("Native turn tutorial flag absent.");
                guard = new FirearmInputSaveGuard(_request.RunId);
                // The fixture borrows the save-free main-menu Player; the native
                // turn-based combat start shows its tutorial once. Mark it seen
                // for the run; the exact flags are verified after restoration.
                tutorial.Unlock();
                foreach (bool turnBased in new[] { false, true })
                {
                    string mode = turnBased ? "tb" : "rt";
                    var initiative = new JObject();
                    foreach (string grit in new[] { "positive", "zero", "zero-true-grit" })
                        initiative[grit] = ObserveTurnModeInitiative(turnBased, grit, leaves, gunslinger,
                            failures["initiative-" + mode]);
                    evidence["initiative-" + mode] = initiative;
                    evidence["deeds-" + mode] = ObserveTurnModeDeeds(turnBased, leaves, gunslinger, failures, mode);
                }
            }
            catch (Exception exception)
            {
                failures["cleanup"].Add("exception=" + exception);
            }
            finally
            {
                if (guard != null) guard.Dispose();
                if (tutorial != null && tutorialLocked) tutorial.Lock();
                bool flagsRestored = flagsBefore.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal)
                    .SequenceEqual(Game.Instance.Player.UnlockableFlags.UnlockedFlags.OrderBy(value =>
                        value.Key.AssetGuid, StringComparer.Ordinal));
                if (!flagsRestored)
                    failures["cleanup"].Add("the main-menu Player's unlockable flags were not restored exactly");
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-turn-modes.json", evidence);
            foreach (string mode in new[] { "rt", "tb" })
            {
                string name = mode == "rt" ? "real time" : "turn-based";
                JToken deeds = evidence["deeds-" + mode];
                assertions.Add(Assertion("fcb-turn-modes-initiative-" + mode,
                    name + ": the native combat entry stores d20 + Initiative + the deed's +2 + four earned Ifrit steps with grit, nothing extra at 0 grit, and the full bonus again at 0 grit with True Grit (Initiative)" +
                        (mode == "tb" ? "; the roll happens inside turn-based combat and the turn controller's known initiative equals the stored value" : ""),
                    Describe(evidence["initiative-" + mode], failures["initiative-" + mode]),
                    failures["initiative-" + mode].Count == 0,
                    "UnitCombatJoinController + UnitCombatPrepareController; UnitCombatState.Initiative; CombatController known initiative"));
                assertions.Add(Assertion("fcb-turn-modes-dodge-" + mode,
                    name + " (M12): the real Gunslinger's Dodge command spends one grit" +
                        (mode == "tb" ? " and the swift action" : "") +
                        ", gives AC +2 plus the three earned Halfling steps, moves nothing, cannot be repeated while active, still protects " +
                        (mode == "tb" ? "during the enemy's turn" : "after five seconds") +
                        ", and has expired, with AC back to its base, " + (mode == "tb" ? "by the caster's next turn" : "within six and a half seconds"),
                    Describe(deeds == null ? null : deeds["dodge"], failures["dodge-" + mode]),
                    failures["dodge-" + mode].Count == 0,
                    "UnitUseAbility of the production Dodge ability; Stats.AC; BuffCollection"));
                assertions.Add(Assertion("fcb-turn-modes-pistol-whip-" + mode,
                    name + " (M10): a queued Pistol-Whip interrupted before it acts spends nothing and attacks nothing; each real Pistol-Whip command makes exactly one stand-in attack" +
                        (mode == "tb" ? " for the standard action" : "") +
                        " and spends one grit; its attack bonus with the three earned steps exceeds the same deed's without them by exactly 3, while the trip's CMB is unchanged",
                    Describe(deeds == null ? null : deeds["pistolWhip"], failures["pistolWhip-" + mode]),
                    failures["pistolWhip-" + mode].Count == 0,
                    "UnitUseAbility of the production Pistol-Whip ability; RuleAttackWithWeapon and RuleCombatManeuver observers"));
                assertions.Add(Assertion("fcb-turn-modes-demoralize-" + mode,
                    name + " (I07): the native demoralize command's Intimidate check with the two earned Rogue steps exceeds the same command's without them by exactly 2",
                    Describe(deeds == null ? null : deeds["demoralize"], failures["demoralize-" + mode]),
                    failures["demoralize-" + mode].Count == 0,
                    "UnitUseAbility of the native Persuasion demoralize; RuleSkillCheck observer"));
                assertions.Add(Assertion("fcb-turn-modes-grit-" + mode,
                    name + ": grit falls by exactly one per spending deed and never refills between " +
                        (mode == "tb" ? "turns and rounds" : "rounds"),
                    Describe(deeds == null ? null : deeds["grit"], failures["grit-" + mode]),
                    failures["grit-" + mode].Count == 0, "UnitAbilityResourceCollection"));
            }
            assertions.Add(Assertion("fcb-turn-modes-cleanup",
                "every fixture, turn scope and tutorial flag is restored exactly",
                Describe(null, failures["cleanup"]), failures["cleanup"].Count == 0,
                "PortalHarness and ElementalNativeTurnScope restoration; UnlockableFlags"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private JObject ObserveTurnModeInitiative(bool turnBased, string gritCase, FavoredClassBlueprintSet leaves,
            GunslingerClassBlueprintSet gunslinger, IList<string> failures)
        {
            var row = new JObject { ["case"] = gritCase };
            var rows = new JArray();
            var diagnostics = new List<string>();
            var scene = new ElementalUndineFeatScenario.PortalHarness(diagnostics);
            BlueprintFaction hostile = null;
            ElementalNativeTurnScope turns = null;
            var observer = new FavoredClassInitiativeObserver();
            bool subscribed = false;
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            PropertyInfo handsProperty = typeof(Game).GetProperty("HandsEquipmentController",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            UnitHandEquipmentController handsBefore = Game.Instance.HandsEquipmentController;
            if (handsBefore == null)
                handsProperty.SetValue(Game.Instance, new UnitHandEquipmentController(), null);
            string label = gritCase + (turnBased ? " turn-based" : " real time") + ": ";
            try
            {
                BlueprintRace human = BlueprintRoot.Instance.Progression.CharacterRaces.Single(race =>
                    race.AssetGuid == FcbHumanRace);
                UnitEntityData actor = scene.Initialize(human);
                actor.Descriptor.Stats.Wisdom.BaseValue = 14;
                actor.Descriptor.AddFact(gunslinger.Grit.Feature);
                actor.Descriptor.AddFact(gunslinger.Initiative);
                GrantFavoredClassRanks(actor, leaves.Pair(FavoredClassCatalog.EffectInitiative, null).Full, 4);
                BlueprintAbilityResource grit = gunslinger.Grit.Resource;
                if (gritCase != "positive")
                    actor.Descriptor.Resources.Spend(grit, actor.Descriptor.Resources.GetResourceAmount(grit));
                if (gritCase == "zero-true-grit")
                    actor.Descriptor.AddFact(gunslinger.TrueGrit.ChoiceFor(TrueGritDeed.GunslingerInitiative));
                row["grit"] = actor.Descriptor.Resources.GetResourceAmount(grit);
                int expectedBonus = gritCase == "zero" ? 0 : 2 + 4;
                if (gritCase == "positive" && (int)row["grit"] < 1)
                    failures.Add(label + "the fixture Gunslinger has no grit");
                hostile = UnityEngine.Object.Instantiate(actor.Blueprint.Faction);
                hostile.name = "KMG_Runtime_FavoredClassTurnModes_Hostile";
                hostile.Peaceful = hostile.AlwaysEnemy = hostile.Neutral = hostile.IsDirectlyControllable = false;
                hostile.Dummy = null;
                hostile.AttackFactions = new[] { actor.Blueprint.Faction };
                UnitEntityData enemy = scene.SpawnFixtureUnit(human, hostile, new Vector3(0, 0, 2),
                    "FavoredClassTurnModesInitiativeEnemy");
                actor.Memory.Add(enemy);
                enemy.Memory.Add(actor);
                int statInitiative = actor.Stats.Initiative.ModifiedValue;
                row["initiativeStat"] = statInitiative;
                EventBus.Subscribe(observer);
                subscribed = true;
                turns = new ElementalNativeTurnScope(actor, enemy, rows,
                    "fcb-turn-modes-initiative-" + gritCase + "-" + (turnBased ? "tb" : "rt") + "-", turnBased, true);
                EventBus.Unsubscribe(observer);
                subscribed = false;
                int index = observer.Rules.FindLastIndex(rule =>
                    ReferenceEquals(((RulebookEvent)rule).Initiator, actor));
                if (index < 0)
                {
                    failures.Add(label + "the native combat entry raised no initiative rule for the Gunslinger");
                    return row;
                }
                RuleInitiativeRoll roll = observer.Rules[index];
                int stored = actor.CombatState.Initiative;
                int d20 = roll.D20;
                int expected = d20 + statInitiative + expectedBonus;
                row["d20"] = d20;
                row["storedInitiative"] = stored;
                row["expectedStored"] = expected;
                row["rolledInTurnBasedCombat"] = observer.TurnBasedAtHandler[index];
                if (stored != expected)
                    failures.Add(label + "the stored initiative is " + stored + ", expected d20 " + d20 + " + stat " +
                        statInitiative + " + " + expectedBonus + " = " + expected);
                if (observer.TurnBasedAtHandler[index] != turnBased)
                    failures.Add(label + "the initiative was rolled " + (observer.TurnBasedAtHandler[index] ? "inside" : "outside") +
                        " turn-based combat");
                if (turnBased)
                {
                    var known = typeof(CombatController).GetField("m_KnownInitiative",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(Game.Instance.TurnBasedCombatController) as Dictionary<string, int>;
                    int value;
                    bool cached = known != null && known.TryGetValue(actor.UniqueId, out value);
                    row["turnBasedKnownInitiative"] = cached ? (JToken)known[actor.UniqueId] : "not-cached";
                    row["round"] = Game.Instance.TurnBasedCombatController.RoundNumber;
                    if (!cached || known[actor.UniqueId] != stored)
                        failures.Add(label + "the turn controller's known initiative is not the stored value");
                }
                row["turns"] = rows;
            }
            catch (Exception exception)
            {
                failures.Add(label + "exception=" + exception);
            }
            finally
            {
                try
                {
                    if (subscribed) EventBus.Unsubscribe(observer);
                    if (turns != null) turns.Dispose();
                    scene.Dispose();
                    new SleepingUnitsController().Tick();
                    if (hostile != null) UnityEngine.Object.DestroyImmediate(hostile);
                    bool restored = !Game.Instance.State.Units.All.Any() &&
                        !Game.Instance.State.AwakeUnits.Any() && !Game.Instance.AwakeUnitGroups.Any() &&
                        scene.PlayerContextRestored && scene.AreaContextRestored &&
                        (turns == null || turns.Restored);
                    row["restored"] = restored;
                    if (!restored)
                        failures.Add(label + "the save-free fixture was not restored exactly");
                }
                finally
                {
                    UnityEngine.Random.state = randomBefore;
                    if (handsBefore == null) handsProperty.SetValue(Game.Instance, null, null);
                }
            }
            row["diagnostics"] = new JArray(diagnostics);
            return row;
        }

        private JObject ObserveTurnModeDeeds(bool turnBased, FavoredClassBlueprintSet leaves,
            GunslingerClassBlueprintSet gunslinger, IDictionary<string, List<string>> failures, string mode)
        {
            var row = new JObject();
            var rows = new JArray();
            var diagnostics = new List<string>();
            List<string> dodgeFailures = failures["dodge-" + mode], whipFailures = failures["pistolWhip-" + mode],
                demoralizeFailures = failures["demoralize-" + mode], gritFailures = failures["grit-" + mode];
            var scene = new ElementalUndineFeatScenario.PortalHarness(diagnostics);
            BlueprintFaction hostile = null;
            ElementalNativeTurnScope turns = null;
            var rules = new FcbTurnModeRuleObserver();
            var checks = new FavoredClassSkillCheckObserver();
            bool subscribed = false;
            ItemEntityWeapon pistol = null;
            UnitEntityData actor = null;
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            PropertyInfo handsProperty = typeof(Game).GetProperty("HandsEquipmentController",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            UnitHandEquipmentController handsBefore = Game.Instance.HandsEquipmentController;
            if (handsBefore == null)
                handsProperty.SetValue(Game.Instance, new UnitHandEquipmentController(), null);
            var gritTrail = new JArray();
            try
            {
                BlueprintRace human = BlueprintRoot.Instance.Progression.CharacterRaces.Single(race =>
                    race.AssetGuid == FcbHumanRace);
                actor = scene.Initialize(human);
                actor.Descriptor.Stats.Wisdom.BaseValue = 18;
                actor.Descriptor.AddFact(gunslinger.Grit.Feature);
                actor.Descriptor.AddFact(gunslinger.Dodge.Feature);
                actor.Descriptor.AddFact(gunslinger.PistolWhip.Feature);
                BlueprintAbility persuasion = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    BlueprintBootstrap.Library, FcbPersuasionUseAbilityGuid, "PersuasionUseAbility");
                actor.Descriptor.AddFact(persuasion);
                BlueprintAbilityResource grit = gunslinger.Grit.Resource;
                actor.Descriptor.Resources.Restore(grit);
                FavoredClassLeafPair dodge = leaves.Pair(FavoredClassCatalog.EffectHalflingDodge, null);
                FavoredClassLeafPair whip = leaves.Pair(FavoredClassCatalog.EffectPistolWhip, null);
                FavoredClassLeafPair demoralize = leaves.Pair(FavoredClassCatalog.EffectDemoralize, null);
                GrantFavoredClassRanks(actor, dodge.Full, 3);
                GrantFavoredClassRanks(actor, whip.Full, 3);
                GrantFavoredClassRanks(actor, demoralize.Full, 2);
                pistol = new ItemEntityWeapon(BlueprintBootstrap.ProductionFirearms.Pistol.Item);
                actor.Body.PrimaryHand.InsertItem(pistol);
                FirearmRuntimeState.Service.Set(pistol, new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
                hostile = UnityEngine.Object.Instantiate(actor.Blueprint.Faction);
                hostile.name = "KMG_Runtime_FavoredClassTurnModes_DeedsHostile";
                hostile.Peaceful = hostile.AlwaysEnemy = hostile.Neutral = hostile.IsDirectlyControllable = false;
                hostile.Dummy = null;
                hostile.AttackFactions = new[] { actor.Blueprint.Faction };
                UnitEntityData enemy = scene.SpawnFixtureUnit(human, hostile, new Vector3(0, 0, 1f),
                    "FavoredClassTurnModesDeedsEnemy");
                enemy.Descriptor.Stats.HitPoints.BaseValue = 1000;
                actor.Memory.Add(enemy);
                enemy.Memory.Add(actor);
                EventBus.Subscribe(rules);
                EventBus.Subscribe(checks);
                subscribed = true;
                turns = new ElementalNativeTurnScope(actor, enemy, rows, "fcb-turn-modes-deeds-" + mode + "-",
                    turnBased, true);
                UnitEntityData caster = actor;
                Action<string> gritAt = stage => gritTrail.Add(stage + "=" +
                    caster.Descriptor.Resources.GetResourceAmount(grit));
                Action waitToAct = () =>
                {
                    if (turnBased) return;
                    for (int tick = 0; tick < 60 && (!caster.CombatState.CanActInCombat ||
                        caster.CombatState.Cooldown.StandardAction > 0f); tick++)
                        turns.PumpCommands(false);
                };
                waitToAct();
                gritAt("start");

                // M12: Gunslinger's Dodge through the real command.
                var dodgeRow = new JObject();
                row["dodge"] = dodgeRow;
                Ability dodgeFact = actor.Descriptor.Abilities.GetAbility(gunslinger.Dodge.ProneAbility);
                if (dodgeFact == null) throw new InvalidOperationException("The Dodge ability was not granted.");
                AbilityData dodgeData = dodgeFact.Data;
                int acBase = actor.Stats.AC.ModifiedValue;
                int gritBefore = actor.Descriptor.Resources.GetResourceAmount(grit);
                Vector3 positionBefore = actor.Position;
                float[] costsBefore = FcbCosts(actor);
                bool availableBefore = dodgeData.IsAvailable;
                TimeSpan castTime = Game.Instance.Player.GameTime;
                UnitUseAbility dodgeCommand = FcbRunCommand(turns, turnBased, actor, dodgeData,
                    new TargetWrapper(actor), 0);
                Buff dodgeBuff = actor.Buffs.GetBuff(gunslinger.Dodge.ArmorClassBuff);
                int acActive = actor.Stats.AC.ModifiedValue;
                float[] costsAfter = FcbCosts(actor);
                dodgeRow["availableBefore"] = availableBefore;
                dodgeRow["commandResult"] = dodgeCommand.Result.ToString();
                dodgeRow["acBase"] = acBase;
                dodgeRow["acActive"] = acActive;
                dodgeRow["moved"] = Math.Round((actor.Position - positionBefore).magnitude, 3);
                dodgeRow["availableWhileActive"] = dodgeData.IsAvailable;
                dodgeRow["costs"] = string.Join(",", costsBefore) + "->" + string.Join(",", costsAfter);
                dodgeRow["buffEnds"] = dodgeBuff == null ? "none" : (dodgeBuff.EndTime - castTime).TotalSeconds.ToString("0.##");
                gritAt("after dodge");
                if (!availableBefore) dodgeFailures.Add("Dodge was unavailable before the command");
                if (dodgeBuff == null) dodgeFailures.Add("the Dodge command applied no buff");
                if (acActive - acBase != GunslingerDodgeArmorClassBonus.Bonus + 3)
                    dodgeFailures.Add("the active Dodge AC is +" + (acActive - acBase) + ", expected +" +
                        (GunslingerDodgeArmorClassBonus.Bonus + 3));
                if ((double)dodgeRow["moved"] > 0.01) dodgeFailures.Add("Dodge moved the Gunslinger");
                if (dodgeData.IsAvailable) dodgeFailures.Add("Dodge was available again while active");
                if (actor.Descriptor.Resources.GetResourceAmount(grit) != gritBefore - 1)
                    dodgeFailures.Add("Dodge did not spend exactly one grit");
                if (turnBased && (costsAfter[2] != costsBefore[2] + 6f || costsAfter[0] != costsBefore[0]))
                    dodgeFailures.Add("the turn-based Dodge did not charge exactly the swift action");

                // M10: an interrupted Pistol-Whip, then two real ones: with the
                // earned steps and, on a later turn or cooldown, without them.
                var whipRow = new JObject();
                row["pistolWhip"] = whipRow;
                Ability whipFact = actor.Descriptor.Abilities.GetAbility(gunslinger.PistolWhip.Ability);
                if (whipFact == null) throw new InvalidOperationException("The Pistol-Whip ability was not granted.");
                AbilityData whipData = whipFact.Data;
                var enemyTarget = new TargetWrapper(enemy);
                int attacksBefore = rules.Attacks.Count;
                int gritBeforeCancel = actor.Descriptor.Resources.GetResourceAmount(grit);
                float[] costsBeforeCancel = FcbCosts(actor);
                var canceled = new UnitUseAbility(whipData, enemyTarget);
                actor.Commands.Run(canceled);
                bool queued = actor.Commands.Contains(canceled);
                actor.Commands.InterruptAll(true);
                actor.Commands.RemoveFinishedAndUpdateQueue();
                whipRow["cancel"] = "queued=" + queued + ";started=" + canceled.IsStarted + ";acted=" + canceled.IsActed +
                    ";attacks=" + (rules.Attacks.Count - attacksBefore);
                if (!queued || canceled.IsActed || rules.Attacks.Count != attacksBefore ||
                    actor.Descriptor.Resources.GetResourceAmount(grit) != gritBeforeCancel ||
                    !FcbCosts(actor).SequenceEqual(costsBeforeCancel))
                    whipFailures.Add("an interrupted Pistol-Whip spent, charged or attacked");
                JObject withSteps = FcbWhip(turns, turnBased, actor, whipData, enemyTarget, rules, gunslinger, grit,
                    whipFailures, "with steps");
                whipRow["withSteps"] = withSteps;
                gritAt("after whip with steps");

                // The Dodge window: still active on the enemy's turn (turn-based)
                // or after five seconds (real time), gone by one round.
                if (turnBased)
                {
                    turns.ReachEnemyTurn();
                    dodgeRow["enemyTurnAc"] = actor.Stats.AC.ModifiedValue;
                    dodgeRow["enemyTurnBuff"] = actor.Buffs.GetBuff(gunslinger.Dodge.ArmorClassBuff) != null;
                    turns.ReachCasterTurn();
                    gritAt("caster turn 2");
                }
                else
                {
                    Func<double> elapsed = () => (Game.Instance.Player.GameTime - castTime).TotalSeconds;
                    for (int tick = 0; tick < 40 && elapsed() < 5.0; tick++) turns.PumpCommands(false);
                    dodgeRow["windowCheckSeconds"] = elapsed().ToString("0.##");
                    dodgeRow["fiveSecondsAc"] = actor.Stats.AC.ModifiedValue;
                    dodgeRow["fiveSecondsBuff"] = actor.Buffs.GetBuff(gunslinger.Dodge.ArmorClassBuff) != null;
                    if (elapsed() >= 6.0)
                        dodgeFailures.Add("the real-time window check came after six seconds");
                    for (int tick = 0; tick < 40 && elapsed() < 6.5; tick++) turns.PumpCommands(false);
                    gritAt("after one round");
                }
                bool protectedWindow = turnBased ? (bool)dodgeRow["enemyTurnBuff"] &&
                    (int)dodgeRow["enemyTurnAc"] == acActive : (bool)dodgeRow["fiveSecondsBuff"] &&
                    (int)dodgeRow["fiveSecondsAc"] == acActive;
                dodgeRow["expiredBuff"] = actor.Buffs.GetBuff(gunslinger.Dodge.ArmorClassBuff) == null;
                dodgeRow["expiredAc"] = actor.Stats.AC.ModifiedValue;
                dodgeRow["elapsedSeconds"] = (Game.Instance.Player.GameTime - castTime).TotalSeconds.ToString("0.##");
                if (!protectedWindow)
                    dodgeFailures.Add("the Dodge bonus was not active through its window");
                if (!(bool)dodgeRow["expiredBuff"] || (int)dodgeRow["expiredAc"] != acBase)
                    dodgeFailures.Add("the Dodge buff or its AC outlived one round");

                waitToAct();
                RemoveFavoredClassRanks(actor, whip.Full);
                JObject withoutSteps = FcbWhip(turns, turnBased, actor, whipData, enemyTarget, rules, gunslinger, grit,
                    whipFailures, "without steps");
                whipRow["withoutSteps"] = withoutSteps;
                gritAt("after whip without steps");
                if (withSteps["attackBonus"] != null && withoutSteps["attackBonus"] != null)
                {
                    int delta = (int)withSteps["attackBonus"] - (int)withoutSteps["attackBonus"];
                    whipRow["attackDelta"] = delta;
                    if (delta != 3)
                        whipFailures.Add("the earned steps changed the deed attack bonus by " + delta + ", expected 3");
                }
                if (withSteps["tripCmb"] == null || withoutSteps["tripCmb"] == null)
                    whipFailures.Add("a Pistol-Whip did not trip, so the CMB comparison is vacuous");
                else if ((int)withSteps["tripCmb"] != (int)withoutSteps["tripCmb"])
                    whipFailures.Add("the trip CMB changed with the earned steps");

                // I07: the native demoralize command with and without the steps.
                var demoralizeRow = new JObject();
                row["demoralize"] = demoralizeRow;
                Ability demoralizeFact = actor.Descriptor.Abilities.GetAbility(persuasion);
                if (demoralizeFact == null) throw new InvalidOperationException("The demoralize ability was not granted.");
                Func<string, int> demoralizeOnce = stage =>
                {
                    if (turnBased)
                    {
                        turns.EndCurrentTurn();
                        turns.ReachCasterTurn();
                    }
                    else waitToAct();
                    checks.Checks.Clear();
                    float[] before = FcbCosts(caster);
                    UnitUseAbility command = FcbRunCommand(turns, turnBased, caster, demoralizeFact.Data,
                        enemyTarget, 0);
                    Tuple<UnitEntityData, int> check = checks.Checks.LastOrDefault(value =>
                        ReferenceEquals(value.Item1, caster));
                    demoralizeRow[stage] = "result=" + command.Result + ";checks=" + checks.Checks.Count +
                        ";bonus=" + (check == null ? "none" : check.Item2.ToString()) + ";costs=" +
                        string.Join(",", before) + "->" + string.Join(",", FcbCosts(caster));
                    if (turnBased && FcbCosts(caster)[0] != before[0] + 6f)
                        demoralizeFailures.Add(stage + ": the turn-based demoralize did not charge the standard action");
                    return check == null ? int.MinValue : check.Item2;
                };
                int demoralizeWith = demoralizeOnce("withSteps");
                RemoveFavoredClassRanks(actor, demoralize.Full);
                int demoralizeWithout = demoralizeOnce("withoutSteps");
                if (demoralizeWith == int.MinValue || demoralizeWithout == int.MinValue)
                    demoralizeFailures.Add("a demoralize command raised no Intimidate check");
                else if (demoralizeWith - demoralizeWithout != 2)
                    demoralizeFailures.Add("the earned steps changed the demoralize check by " +
                        (demoralizeWith - demoralizeWithout) + ", expected 2");
                gritAt("end");

                // Grit: one per spending deed, no refill across rounds.
                row["grit"] = new JObject { ["trail"] = gritTrail };
                int start = int.Parse(((string)gritTrail[0]).Split('=')[1]);
                int end = actor.Descriptor.Resources.GetResourceAmount(grit);
                ((JObject)row["grit"])["spent"] = start - end;
                if (start < 3)
                    gritFailures.Add("the fixture started with " + start + " grit, fewer than the three spending deeds");
                if (start - end != 3)
                    gritFailures.Add("three spending deeds changed grit by " + (start - end));
                string[] values = gritTrail.Select(value => ((string)value).Split('=')[1]).ToArray();
                for (int index = 1; index < values.Length; index++)
                    if (int.Parse(values[index]) > int.Parse(values[index - 1]))
                        gritFailures.Add("grit refilled at " + (string)gritTrail[index]);
                row["turns"] = rows;
            }
            catch (Exception exception)
            {
                foreach (List<string> list in new[] { dodgeFailures, whipFailures, demoralizeFailures, gritFailures })
                    if (list.Count == 0) list.Add("exception=" + exception.GetType().Name + ": " + exception.Message);
                row["exception"] = exception.ToString();
            }
            finally
            {
                try
                {
                    if (subscribed)
                    {
                        EventBus.Unsubscribe(rules);
                        EventBus.Unsubscribe(checks);
                    }
                    if (turns != null) turns.Dispose();
                    if (pistol != null)
                    {
                        FirearmRuntimeState.Service.Forget(pistol);
                        if (actor != null && actor.Body.PrimaryHand.MaybeItem != null)
                            actor.Body.PrimaryHand.RemoveItem(false);
                    }
                    scene.Dispose();
                    new SleepingUnitsController().Tick();
                    if (hostile != null) UnityEngine.Object.DestroyImmediate(hostile);
                    bool restored = !Game.Instance.State.Units.All.Any() &&
                        !Game.Instance.State.AwakeUnits.Any() && !Game.Instance.AwakeUnitGroups.Any() &&
                        scene.PlayerContextRestored && scene.AreaContextRestored &&
                        (turns == null || turns.Restored);
                    row["restored"] = restored;
                    if (!restored)
                        failures["cleanup"].Add(mode + ": the save-free deeds fixture was not restored exactly");
                }
                finally
                {
                    UnityEngine.Random.state = randomBefore;
                    if (handsBefore == null) handsProperty.SetValue(Game.Instance, null, null);
                }
            }
            row["diagnostics"] = new JArray(diagnostics);
            return row;
        }

        /// <summary>One real Pistol-Whip command, its natural roll forced to 20 so it hits and trips.</summary>
        private static JObject FcbWhip(ElementalNativeTurnScope turns, bool turnBased, UnitEntityData actor,
            AbilityData data, TargetWrapper target, FcbTurnModeRuleObserver rules,
            GunslingerClassBlueprintSet gunslinger, BlueprintAbilityResource grit, IList<string> failures, string stage)
        {
            var row = new JObject();
            int attacks = rules.Attacks.Count, maneuvers = rules.Maneuvers.Count;
            int gritBefore = actor.Descriptor.Resources.GetResourceAmount(grit);
            float[] before = FcbCosts(actor);
            UnitUseAbility command = FcbRunCommand(turns, turnBased, actor, data, target, FindNativeD20Seed(20));
            RuleAttackWithWeapon[] made = rules.Attacks.Skip(attacks).Where(value => value.Weapon != null &&
                (ReferenceEquals(value.Weapon.Blueprint, gunslinger.PistolWhip.OneHandedItem) ||
                 ReferenceEquals(value.Weapon.Blueprint, gunslinger.PistolWhip.TwoHandedItem))).ToArray();
            RuleCombatManeuver[] trips = rules.Maneuvers.Skip(maneuvers).Where(value =>
                value.Type == CombatManeuver.Trip).ToArray();
            row["result"] = command.Result.ToString();
            row["standInAttacks"] = made.Length;
            row["allAttacks"] = rules.Attacks.Count - attacks;
            row["gritSpent"] = gritBefore - actor.Descriptor.Resources.GetResourceAmount(grit);
            row["costs"] = string.Join(",", before) + "->" + string.Join(",", FcbCosts(actor));
            if (made.Length == 1 && made[0].AttackRoll != null)
            {
                row["attackBonus"] = made[0].AttackRoll.AttackBonus;
                row["natural"] = (int)made[0].AttackRoll.Roll;
                row["hit"] = made[0].AttackRoll.IsHit;
            }
            if (trips.Length == 1) row["tripCmb"] = trips[0].InitiatorCMB;
            if (made.Length != 1 || rules.Attacks.Count - attacks != 1)
                failures.Add(stage + ": the command made " + (rules.Attacks.Count - attacks) +
                    " attacks, expected exactly one stand-in attack");
            if ((int)row["gritSpent"] != 1)
                failures.Add(stage + ": the command spent " + row["gritSpent"] + " grit");
            if (turnBased && FcbCosts(actor)[0] != before[0] + 6f)
                failures.Add(stage + ": the turn-based Pistol-Whip did not charge the standard action");
            return row;
        }

        /// <summary>Runs one native command to completion in either mode, seeding its effect's first roll.</summary>
        private static UnitUseAbility FcbRunCommand(ElementalNativeTurnScope turns, bool turnBased,
            UnitEntityData caster, AbilityData data, TargetWrapper target, int seed)
        {
            var command = new UnitUseAbility(data, target);
            caster.Commands.Run(command);
            try
            {
                if (turnBased)
                {
                    for (int tick = 0; !command.IsActed && !command.IsFinished && tick < 16; tick++)
                        turns.Drive(command);
                    if (seed != 0) UnityEngine.Random.InitState(seed);
                    for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded &&
                        tick < 120; tick++)
                        turns.Execute(command.ExecutionProcess.Tick);
                    if (!command.IsFinished) turns.Drive(command);
                }
                else
                {
                    // Real time: a pump acts the command; the execution
                    // process it creates is ticked from the next pump on.
                    for (int tick = 0; !command.IsActed && !command.IsFinished && tick < 60; tick++)
                        turns.PumpCommands(false);
                    if (seed != 0) UnityEngine.Random.InitState(seed);
                    for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded &&
                        tick < 60; tick++)
                        turns.PumpCommands(false);
                }
                if (!command.IsActed)
                    throw new InvalidOperationException("The native command never acted: " +
                        data.Blueprint.name + " (" + command.Result + ").");
                return command;
            }
            finally
            {
                if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded)
                    command.ExecutionProcess.Detach();
            }
        }

        private static float[] FcbCosts(UnitEntityData unit)
        {
            return new[] { unit.CombatState.Cooldown.StandardAction, unit.CombatState.Cooldown.MoveAction,
                unit.CombatState.Cooldown.SwiftAction };
        }
    }
}
