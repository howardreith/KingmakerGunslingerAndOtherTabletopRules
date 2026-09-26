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
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>Records every initiative rule the native combat entry raises.</summary>
        private sealed class FavoredClassInitiativeObserver : IUnitInitiativeHandler, IGlobalSubscriber
        {
            internal readonly List<RuleInitiativeRoll> Rules = new List<RuleInitiativeRoll>();
            internal readonly List<int> StoredAtHandler = new List<int>();
            internal readonly List<bool> TurnBasedAtHandler = new List<bool>();

            public void HandleUnitRollsInitiative(RuleInitiativeRoll rule)
            {
                if (rule == null) return;
                Rules.Add(rule);
                TurnBasedAtHandler.Add(CombatController.IsInTurnBasedCombat());
                UnitEntityData unit = ((RulebookEvent)rule).Initiator;
                StoredAtHandler.Add(unit == null || unit.CombatState == null ? int.MinValue :
                    unit.CombatState.Initiative);
            }
        }

        // M13/L07 and blocker D3: the Gunslinger Initiative bonus and its
        // Ifrit improvement must reach the initiative the native combat-entry
        // controller stores and orders by, in real-time and turn-based entry.
        private RuntimeTestResult RunFavoredClassInitiativeTiming()
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
            var failures = new Dictionary<bool, List<string>>
            {
                { false, new List<string>() }, { true, new List<string>() }
            };
            FirearmInputSaveGuard guard = null;
            try
            {
                guard = new FirearmInputSaveGuard(_request.RunId);
                foreach (bool turnBased in new[] { false, true })
                    evidence[turnBased ? "turnBased" : "realTime"] = ObserveInitiativeEntry(turnBased,
                        leaves, gunslinger, failures[turnBased]);
            }
            catch (Exception exception)
            {
                failures[false].Add("exception=" + exception);
            }
            finally
            {
                if (guard != null) guard.Dispose();
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-initiative-timing.json", evidence);
            assertions.Add(Assertion("fcb-initiative-real-time-entry",
                "entering real-time combat, the initiative the native controller stores equals d20 + Initiative + the deed's +2 + the earned Ifrit steps",
                Describe(evidence["realTime"], failures[false]), failures[false].Count == 0,
                "UnitCombatJoinController + UnitCombatPrepareController; UnitCombatState.Initiative"));
            assertions.Add(Assertion("fcb-initiative-turn-based-entry",
                "entering turn-based combat, the stored initiative the turn order uses equals d20 + Initiative + the deed's +2 + the earned Ifrit steps",
                Describe(evidence["turnBased"], failures[true]), failures[true].Count == 0,
                "native turn-based toggle; CombatController known initiative"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private JObject ObserveInitiativeEntry(bool turnBased, FavoredClassBlueprintSet leaves,
            GunslingerClassBlueprintSet gunslinger, IList<string> failures)
        {
            var row = new JObject();
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
            try
            {
                BlueprintRace human = BlueprintRoot.Instance.Progression.CharacterRaces.Single(race =>
                    race.AssetGuid == FcbHumanRace);
                UnitEntityData actor = scene.Initialize(human);
                actor.Descriptor.Stats.Wisdom.BaseValue = 14;
                actor.Descriptor.AddFact(gunslinger.Grit.Feature);
                actor.Descriptor.AddFact(gunslinger.Initiative);
                GrantFavoredClassRanks(actor, leaves.Pair(FavoredClassCatalog.EffectInitiative, null).Full, 4);
                int grit = actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
                row["grit"] = grit;
                if (grit < 1)
                    failures.Add("the fixture Gunslinger has no grit, so the deed cannot apply");
                hostile = UnityEngine.Object.Instantiate(actor.Blueprint.Faction);
                hostile.name = "KMG_Runtime_FavoredClassInitiative_Hostile";
                hostile.Peaceful = hostile.AlwaysEnemy = hostile.Neutral = hostile.IsDirectlyControllable = false;
                hostile.Dummy = null;
                hostile.AttackFactions = new[] { actor.Blueprint.Faction };
                UnitEntityData enemy = scene.SpawnFixtureUnit(human, hostile, new Vector3(0, 0, 2),
                    "FavoredClassInitiativeEnemy");
                actor.Memory.Add(enemy);
                enemy.Memory.Add(actor);
                int statInitiative = actor.Stats.Initiative.ModifiedValue;
                row["initiativeStat"] = statInitiative;
                EventBus.Subscribe(observer);
                subscribed = true;
                turns = new ElementalNativeTurnScope(actor, enemy, rows,
                    "fcb-initiative-" + (turnBased ? "tb" : "rtwp") + "-", turnBased);
                EventBus.Unsubscribe(observer);
                subscribed = false;
                int index = observer.Rules.FindLastIndex(rule =>
                    ReferenceEquals(((RulebookEvent)rule).Initiator, actor));
                if (index < 0)
                {
                    failures.Add("the native combat entry raised no initiative rule for the Gunslinger");
                    return row;
                }
                RuleInitiativeRoll roll = observer.Rules[index];
                int d20 = roll.D20;
                int stored = actor.CombatState.Initiative;
                int expected = d20 + statInitiative + 2 + 4;
                row["d20"] = d20;
                row["storedInitiative"] = stored;
                row["storedWhenHandlersRan"] = observer.StoredAtHandler[index];
                row["ruleModifierAfterHandlers"] = roll.Modifier;
                row["ruleResultAfterHandlers"] = roll.Result;
                row["expectedStored"] = expected;
                if (turnBased)
                {
                    // The scope enrolls both actors before toggling turn-based
                    // mode, so the controller orders them from the stored
                    // combat-state initiative; its known-initiative cache is
                    // recorded as evidence only.
                    var known = typeof(CombatController).GetField("m_KnownInitiative",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(Game.Instance.TurnBasedCombatController) as Dictionary<string, int>;
                    int value;
                    row["turnBasedKnownInitiative"] = known != null && known.TryGetValue(actor.UniqueId, out value)
                        ? (JToken)value : "not-cached";
                    row["turnBasedSortedFirst"] = Game.Instance.TurnBasedCombatController.SortedUnits
                        .Select(unit => ReferenceEquals(unit, actor) ? "gunslinger" : "enemy")
                        .FirstOrDefault();
                    row["enemyStoredInitiative"] = enemy.CombatState.Initiative;
                }
                if (stored != expected)
                    failures.Add("the stored initiative is " + stored + ", expected d20 " + d20 + " + stat " +
                        statInitiative + " + deed 2 + favored-class 4 = " + expected);
                row["turns"] = rows;
            }
            catch (Exception exception)
            {
                failures.Add("exception=" + exception);
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
                        failures.Add("the save-free fixture was not restored exactly");
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
    }
}
