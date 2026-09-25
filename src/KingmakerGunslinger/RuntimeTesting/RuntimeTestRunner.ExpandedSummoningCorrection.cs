using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The correction order's focused live cases (2026-09-25): the cats'
    /// attack and target identity, the universal grab size rule and the
    /// swallow size limits, the Giant Flytrap's multi-link hold, engulf,
    /// release paths and crowded pathing, the chartered mephit roles with the
    /// ally-safe cloud cases, the Cyclops armor class and Flash of Insight,
    /// the Web's ranged touch path, and the docile hooves - and, in a second
    /// scenario, the visual variants' resource ownership across repeated
    /// lifecycles. Both run on disposable units in the guarded working save
    /// and restore it exactly. Both span frames: an area effect finds its
    /// units through the game's own grid on the frames after its placement,
    /// a projectile flies on world time and makes its own attack roll when
    /// it lands, and the game destroys a view at the end of the frame that
    /// disposed its unit.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class ExpandedSummoningCorrectionFixture
        {
            internal BlueprintScriptableObject[] Blueprints;
            internal UnitEntityData Caster;
            internal BlueprintUnit CasterBlueprint;
            internal Kingmaker.EntitySystem.SceneEntitiesState Scene;
            internal object SceneEntities;
            internal object AllUnits;
            internal object Party;
            internal object[] UnitsBefore;
            internal object[] PartyBefore;
            internal object[] ExactStart;
            internal MethodInfo SummonRuleMethod;
            internal readonly List<UnitEntityData> Created = new List<UnitEntityData>();
            internal readonly ExpandedSummoningMechanicalEvidence Evidence =
                new ExpandedSummoningMechanicalEvidence();
            internal UnitEntityData Hostile;
            internal BlueprintUnit HostileBlueprint;
            internal Size HostileSize;
        }

        /// <summary>
        /// The disposable caster in the guarded working save, the summon-rule
        /// capture and the exact snapshots every correction scenario shares.
        /// </summary>
        private ExpandedSummoningCorrectionFixture BeginExpandedSummoningCorrectionFixture(
            string casterName)
        {
            var fixture = new ExpandedSummoningCorrectionFixture();
            fixture.Blueprints = BlueprintBootstrap.Library.GetAllBlueprints()
                .Where(value => value != null).ToArray();
            object state = ReadExactMember(Game.Instance, "State");
            fixture.AllUnits = ReadExactMember(state, "AllUnits");
            object player = ReadExactMember(Game.Instance, "Player");
            fixture.Party = ReadExactMember(player, "Party");
            fixture.UnitsBefore = SnapshotReferences(fixture.AllUnits);
            fixture.PartyBefore = SnapshotReferences(fixture.Party);
            fixture.SummonRuleMethod = typeof(RuleSummonUnit).GetMethod("OnTrigger",
                BindingFlags.Public | BindingFlags.Instance);
            if (fixture.SummonRuleMethod == null)
                throw new MissingMethodException(typeof(RuleSummonUnit).FullName, "OnTrigger");
            MethodInfo capturePostfix = typeof(RuntimeTestRunner).GetMethod(
                "ExpandedSummoningRuleCapturePostfix",
                BindingFlags.NonPublic | BindingFlags.Static);
            _context.Harmony.Patch(fixture.SummonRuleMethod, null,
                new HarmonyMethod(capturePostfix), null);
            _expandedSummoningRuleCaptureActive = true;
            UnitEntityData areaAnchor = fixture.PartyBefore.OfType<UnitEntityData>()
                .FirstOrDefault(value => value.HoldingState != null);
            if (areaAnchor == null)
                throw new InvalidOperationException(
                    "The guarded working save has no party unit in an exact area state.");
            fixture.Scene = areaAnchor.HoldingState;
            fixture.CasterBlueprint = UnityEngine.Object.Instantiate(
                BlueprintRoot.Instance.DefaultPlayerCharacter);
            fixture.CasterBlueprint.name = casterName;
            fixture.CasterBlueprint.IsCheater = true;
            fixture.Caster = Game.Instance.EntityCreator.SpawnUnit(fixture.CasterBlueprint,
                areaAnchor.Position, Quaternion.identity, fixture.Scene);
            if (fixture.Caster == null || fixture.Caster.View == null ||
                fixture.Caster.View.Data == null)
                throw new InvalidOperationException(
                    "Native entity creation did not produce a live caster view.");
            Game.Instance.EntityCreator.Tick();
            if (!fixture.Caster.IsInState)
                throw new InvalidOperationException(
                    "The disposable caster did not enter the exact loaded-area state.");
            fixture.SceneEntities = fixture.Scene.AllEntityData;
            fixture.Caster.Descriptor.Stats.HitPoints.BaseValue = 100000;
            // A good caster: the Summon Monster executions of templated
            // creatures choose the celestial template from the caster's
            // alignment, and the disposable caster stays out of the AI.
            fixture.Caster.Descriptor.Alignment.Set(Alignment.NeutralGood);
            SetExpandedSummoningBrainActive(fixture.Caster, false);
            fixture.ExactStart = SnapshotReferences(fixture.SceneEntities);
            return fixture;
        }

        /// <summary>
        /// A hostile target the way the mechanical run makes one: a Pixie's
        /// dance names the faction that is an enemy of the party.
        /// </summary>
        private static void CreateExpandedSummoningCorrectionHostile(
            ExpandedSummoningCorrectionFixture fixture)
        {
            UnitEntityData pixie = CastExpandedSummoningVariant(fixture.Blueprints,
                fixture.Caster, ExpandedSummoningVariant(SummonFamily.NaturesAlly, "pixie", 9,
                    SummonMultiplicity.One), null, fixture.Evidence).Single();
            fixture.Created.Add(pixie);
            BlueprintAbility dance = fixture.Blueprints.OfType<BlueprintAbility>().Single(
                value => value.name == "KMG_Summoning_Special_Pixie_IrresistibleDance");
            BlueprintUnit hostileBlueprint;
            fixture.Hostile = CreateExpandedSummoningHostileTarget(fixture.Blueprints, pixie,
                dance, fixture.Caster.Position + Vector3.forward, fixture.Scene,
                out hostileBlueprint);
            fixture.HostileBlueprint = hostileBlueprint;
            fixture.Hostile.Descriptor.Stats.HitPoints.BaseValue = 100000;
            fixture.HostileSize = fixture.Hostile.Descriptor.State.Size;
            SetExpandedSummoningBrainActive(fixture.Hostile, false);
            DisposeExpandedSummoningUnits(fixture.Created, new[] { pixie });
        }

        private void EndExpandedSummoningCorrectionFixture(
            ExpandedSummoningCorrectionFixture fixture, out bool cleaned)
        {
            cleaned = false;
            if (fixture == null) return;
            try
            {
                foreach (UnitEntityData unit in fixture.Created.Distinct().ToArray())
                    CleanupExpandedSummoningUnit(unit);
                if (fixture.Hostile != null && !fixture.Hostile.Destroyed)
                    fixture.Hostile.Destroy();
                if (fixture.HostileBlueprint != null)
                    UnityEngine.Object.Destroy(fixture.HostileBlueprint);
                if (fixture.SceneEntities != null)
                    DrainExpandedSummoningDestroyQueue(fixture.SceneEntities, fixture.ExactStart);
            }
            finally
            {
                _expandedSummoningRuleCaptureActive = false;
                ExpandedSummoningRuleCapture.Clear();
                if (fixture.SummonRuleMethod != null)
                    _context.Harmony.Unpatch(fixture.SummonRuleMethod, HarmonyPatchType.All,
                        _context.ModId);
                IEnumerable<UnitEntityData> localUnits = fixture.SceneEntities == null
                    ? Enumerable.Empty<UnitEntityData>()
                    : SnapshotReferences(fixture.SceneEntities).OfType<UnitEntityData>();
                foreach (UnitEntityData unit in localUnits.Concat(
                    SnapshotReferences(fixture.AllUnits).OfType<UnitEntityData>())
                    .Where(value => !fixture.UnitsBefore.Any(prior =>
                        ReferenceEquals(prior, value))).Distinct().ToArray())
                {
                    if (unit.IsInState) unit.Destroy();
                    else unit.Dispose();
                }
                Game.Instance.EntityDestroyer.Tick();
                if (fixture.CasterBlueprint != null)
                    UnityEngine.Object.Destroy(fixture.CasterBlueprint);
                cleaned = SameReferences(fixture.UnitsBefore, SnapshotReferences(fixture.AllUnits)) &&
                    SameReferences(fixture.PartyBefore, SnapshotReferences(fixture.Party)) &&
                    (fixture.Caster == null || !ContainsReference(fixture.AllUnits, fixture.Caster));
            }
        }

        /// <summary>
        /// The own-tier variant of a creature, from the Nature's Ally family
        /// where it has one (no template) and otherwise from Summon Monster.
        /// </summary>
        private static SummonVariantSpec ExpandedSummoningOwnTierVariant(string creatureKey,
            SummonMultiplicity multiplicity)
        {
            SummonVariantSpec[] candidates = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster))
                .Where(value => value.Creature.Key == creatureKey &&
                    value.Multiplicity == multiplicity)
                .OrderBy(value => value.Family == SummonFamily.NaturesAlly ? 0 : 1)
                .ThenBy(value => value.ParentTier).ToArray();
            if (candidates.Length == 0)
                throw new InvalidOperationException("No " + multiplicity + " variant of " +
                    creatureKey + " exists in the roster.");
            return candidates[0];
        }

        private static UnitEntityData CastExpandedSummoningOwnTier(
            ExpandedSummoningCorrectionFixture fixture, string creatureKey)
        {
            UnitEntityData unit = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                ExpandedSummoningOwnTierVariant(creatureKey, SummonMultiplicity.One), null,
                fixture.Evidence).Single();
            fixture.Created.Add(unit);
            RemoveExpandedSummoningAppearanceBuffs(unit);
            return unit;
        }

        /// <summary>
        /// Keeps a fixture unit out of the AI across the frames a phase waits:
        /// the game's own brain-active switch, and an emptied action list so
        /// no summon spends its abilities or walks off while a projectile
        /// flies or an area effect settles.
        /// </summary>
        private static void SetExpandedSummoningBrainActive(UnitEntityData unit, bool active)
        {
            if (unit == null) return;
            PropertyInfo property = typeof(UnitEntityData).GetProperty("IsBrainActive",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo setter = property == null ? null : property.GetSetMethod(true);
            if (setter != null) setter.Invoke(unit, new object[] { active });
            else
            {
                FieldInfo backing = typeof(UnitEntityData).GetField("<IsBrainActive>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (backing != null) backing.SetValue(unit, active);
            }
            if (active || unit.Brain == null) return;
            if (unit.Brain.Actions != null) unit.Brain.Actions.Clear();
            if (unit.Brain.AvailableActions != null) unit.Brain.AvailableActions.Clear();
        }

        private static UnitEntityData CastExpandedSummoningQuietUnit(
            ExpandedSummoningCorrectionFixture fixture, string creatureKey)
        {
            UnitEntityData unit = CastExpandedSummoningOwnTier(fixture, creatureKey);
            unit.Descriptor.Stats.HitPoints.BaseValue = 100000;
            SetExpandedSummoningBrainActive(unit, false);
            return unit;
        }

        private static string ExpandedSummoningPlanFullAttack(UnitEntityData attacker,
            UnitEntityData target, bool charge, int rakeLimbCount, out int rakeSlots,
            out int attacks)
        {
            var command = new UnitAttack(target);
            command.IsCharge = charge;
            command.Init(attacker);
            List<AttackHandInfo> planned = command.CreateFullAttack();
            attacks = planned.Count;
            rakeSlots = planned.Count(info => info != null && info.Hand != null &&
                SummonLimbs.IsRakeSlot(attacker, info.Hand, rakeLimbCount));
            return (rakeSlots == 0 ? "dropped" : "kept") + "(" + attacks + "/" + rakeSlots + ")";
        }

        private static string DescribeExpandedSummoningCondition(UnitEntityData unit)
        {
            return "cantMove=" + unit.Descriptor.State.HasCondition(UnitCondition.CantMove) +
                ";entangled=" + unit.Descriptor.State.HasCondition(UnitCondition.Entangled) +
                ";cantAct=" + unit.Descriptor.State.HasCondition(UnitCondition.CantAct);
        }

        private static string Sanitize(string value)
        {
            return value == null ? "<null>" : value.Replace(';', ',').Replace('|', '/');
        }

        /// <summary>
        /// Starts an ability the way the synchronous helper does (the native
        /// command under the cutscene context, the cast animation acted) but
        /// leaves its execution process to the game's own executor on the
        /// frames that follow, so a projectile flies on world time and makes
        /// its own attack roll when it lands.
        /// </summary>
        private static UnitUseAbility BeginExpandedSummoningDetachedAbility(UnitEntityData caster,
            BlueprintAbility ability, TargetWrapper target)
        {
            Ability granted = caster.Descriptor.Abilities.GetAbility(ability);
            if (granted == null) throw new InvalidOperationException(
                "The exact KMG ability was not granted: " + ability.name + ".");
            InterruptExpandedSummoningFixtureCommands(caster);
            var data = new AbilityData(granted);
            UnitUseAbility command;
            var cutsceneParameters = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
            using (cutsceneParameters.Data)
                command = new UnitUseAbility(data, target);
            if (!command.Cutscene || !data.IsAvailable || !command.CanStart)
                throw new InvalidOperationException("The KMG ability command was unavailable: " +
                    ability.name + ";available=" + data.IsAvailable + ";canStart=" + command.CanStart +
                    ";canTarget=" + data.CanTarget(target) + ".");
            command.IgnoreCooldown(TimeSpan.Zero);
            command.Init(caster);
            command.Start();
            if (!command.IsRunning)
                throw new InvalidOperationException("The KMG ability command did not start: " +
                    ability.name + ";result=" + command.Result + ";canTarget=" + data.CanTarget(target) +
                    ";enoughClose=" + command.IsUnitEnoughClose + ".");
            if (command.Animation != null) command.Animation.IsActed = true;
            command.Tick();
            if (!string.Equals(command.Result.ToString(), "Success", StringComparison.Ordinal) ||
                command.ExecutionProcess == null)
                throw new InvalidOperationException("The KMG ability did not begin executing: " +
                    ability.name + ";result=" + command.Result + ".");
            return command;
        }

        /// <summary>
        /// One frame of a detached ability: the game's executor advances the
        /// process on its own; after a long wait the process is ticked here
        /// as well so an executor that dropped it still ends.
        /// </summary>
        private static bool TickExpandedSummoningDetachedAbility(UnitUseAbility command, int frame)
        {
            if (command == null || command.ExecutionProcess == null) return true;
            if (!command.ExecutionProcess.IsEnded && frame >= 120) command.ExecutionProcess.Tick();
            return command.ExecutionProcess.IsEnded;
        }

        private static void EndExpandedSummoningDetachedAbility(UnitUseAbility command)
        {
            if (command == null) return;
            if (command.Animation != null) FinishExpandedSummoningAnimation(command.Animation);
            if (command.Executor != null) InterruptExpandedSummoningFixtureCommands(command.Executor);
        }

        /// <summary>Records every attack roll a chosen initiator makes (the web's ranged touch attack).</summary>
        private sealed class ExpandedSummoningAttackRollObserver : IGlobalRulebookHandler<RuleAttackRoll>
        {
            internal UnitEntityData Initiator;
            internal readonly List<string> Rolls = new List<string>();

            public void OnEventAboutToTrigger(RuleAttackRoll evt) { }

            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                if (evt == null || Initiator == null || !ReferenceEquals(evt.Initiator, Initiator)) return;
                Rolls.Add("type=" + evt.AttackType + ",natural=" + (int)evt.Roll + ",bonus=" +
                    evt.AttackBonus + ",targetAc=" + evt.TargetAC + ",hit=" + evt.IsHit +
                    ",weapon=" + (evt.Weapon == null || evt.Weapon.Blueprint == null ? "<none>" :
                        evt.Weapon.Blueprint.name));
            }
        }

        // ---------------------------------------------------------------------------------------------------------
        // The rules scenario: the synchronous cases on the first frame, then
        // the wind wall, the cloud and the web across frames.
        // ---------------------------------------------------------------------------------------------------------

        private ExpandedSummoningCorrectionFixture _rulesFixture;
        private int _rulesPhase;
        private int _rulesWait;
        private readonly List<RuntimeTestAssertion> _rulesCases = new List<RuntimeTestAssertion>();
        private readonly List<string> _rulesSteps = new List<string>();
        private UnitEntityData _rulesWolf;
        private UnitEntityData _rulesMephit;
        private UnitEntityData _rulesSpider;
        private BlueprintAbility _rulesWeb;
        private BlueprintAbilityResource _rulesWebResource;
        private BlueprintBuff _rulesWebbed;
        private UnitUseAbility _rulesWebCommand;
        private ExpandedSummoningAttackRollObserver _rulesObserver;
        private int _rulesHostileAc;
        private int _rulesHostileDamage;
        private int _rulesCasterDamage;
        private int _rulesWebUses0, _rulesWebUses1, _rulesWebUses2;
        private int _rulesHighTouch, _rulesLowTouch;
        private bool _rulesMissedHighTouch;
        private string _rulesWebRollA = "<none>";

        private void PollExpandedSummoningRules()
        {
            string stage = "phase" + _rulesPhase;
            try
            {
                if (_rulesPhase == 0)
                {
                    stage = "construct-fixture";
                    _rulesFixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_ExpandedSummoning_RulesCaster");
                    stage = "hostile";
                    CreateExpandedSummoningCorrectionHostile(_rulesFixture);
                    _rulesHostileDamage = _rulesFixture.Hostile.Descriptor.Damage;
                    _rulesCasterDamage = _rulesFixture.Caster.Descriptor.Damage;
                    string detail;
                    bool ok;
                    stage = "cats";
                    ok = ExerciseExpandedSummoningCorrectionCats(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-cats",
                        "leopard, lion and dire lion grab with the bite only; the tiger and the smilodon with the bite and both foreclaws; no rake claw ever grabs; a single attack never carries a rake slot; the full attack drops the rake slots unless the cat charges or held the exact target since its round began",
                        detail, ok, "SummonGrabComponent.IsGrabLimb and TryGrab on live limbs; UnitAttack.CreateSingleAttack and CreateFullAttack through the sequencing seam"));
                    stage = "grapple-sizes";
                    ok = ExerciseExpandedSummoningCorrectionGrappleSizes(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-grapple-sizes",
                        "a grab works against a target of the holder's size or smaller and is refused against a larger one; grapple checks carry +4 from the grab and +5 more to maintain; the worm grabs a Gargantuan foe but never swallows it and refuses a Colossal one",
                        detail, ok, "TryGrab with the hostile's size set exactly; RuleCalculateCMB grapple against trip; the hold buff's later-turn tick"));
                    stage = "flytrap";
                    ok = ExerciseExpandedSummoningCorrectionFlytrap(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-flytrap",
                        "one link per bite, four at most, each held state naming the flytrap and its own bite; a bite already holding cannot grab again; a Large held foe is maintained but never engulfed, a Medium one is engulfed on the later turn and takes the engulf damage; escape, the last link ending, the holder's disposal, the swallow lifecycle and the area-leave sweep each release; a free unit still finds a path past four held units",
                        detail, ok, "held-state buffs on the targets, the multi-hold tick, UnitHelper.TryBreakFree, SummonGrappleAreaSafeguard.Sweep, the movement agent's path request with the four held units standing"));
                    stage = "mephit-roles";
                    ok = ExerciseExpandedSummoningCorrectionMephitRoles(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-mephit-roles",
                        "chill metal targets only metal-bearers and runs the seven-round table (full for armor, minimal for a weapon); pyrotechnics blinds enemies only for 1d4+1 rounds; magma form gives DR 20/magic, speed 10 and forbids attacks while abilities work; glitterdust blinds the hostile and never the party caster or the allied summon",
                        detail, ok, "live casts on the disposable units; SummonChillMetalComponent outcomes; RuleDealDamage through the form's damage reduction; UnitAttack.ShouldBeInterrupted"));
                    stage = "cyclops";
                    ok = ExerciseExpandedSummoningCorrectionCyclops(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-cyclops",
                        "armor class 19 with +4 armor (a fact, no item) and +7 natural armor beside the game's own difficulty modifier; Flash of Insight makes the next attack's own d20 a natural 20 with an ordinary confirmation roll, is spent by that one attack, and touches no other roll",
                        detail, ok, "RuleCalculateAC, the armor class modifier list, RuleAttackWithWeapon at a forced natural 1 while armed and after, a Will save rolled while armed"));
                    stage = "hooves";
                    ok = ExerciseExpandedSummoningCorrectionHooves(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-hooves",
                        "the pony's and the horse's hooves are both secondary: -5 to hit and half the Strength modifier to damage against the same hooves treated as primary, both listed in the full attack",
                        detail, ok, "RuleCalculateAttackBonus and RuleCalculateWeaponStats with the docile flag on and off; UnitAttack.CreateFullAttack"));
                    stage = "wind-wall-cast";
                    BeginExpandedSummoningWindWall();
                    _rulesWait = 0;
                    _rulesPhase = 1;
                    return;
                }
                if (_rulesPhase == 1)
                {
                    if (_rulesWait++ < 6) return;
                    stage = "wind-wall";
                    FinishExpandedSummoningWindWall();
                    stage = "cloud-cast";
                    BeginExpandedSummoningCloud();
                    _rulesWait = 0;
                    _rulesPhase = 2;
                    return;
                }
                if (_rulesPhase == 2)
                {
                    if (_rulesWait++ < 6) return;
                    stage = "cloud";
                    FinishExpandedSummoningCloud();
                    stage = "web-cast-a";
                    BeginExpandedSummoningWeb();
                    _rulesWait = 0;
                    _rulesPhase = 3;
                    return;
                }
                if (_rulesPhase == 3)
                {
                    stage = "web-a";
                    bool ended = TickExpandedSummoningDetachedAbility(_rulesWebCommand, _rulesWait);
                    if (!ended && _rulesWait++ < 420) return;
                    _rulesSteps.Add("webA:ended=" + ended + ";frames=" + _rulesWait);
                    EndExpandedSummoningDetachedAbility(_rulesWebCommand);
                    stage = "web-cast-b";
                    ContinueExpandedSummoningWeb();
                    _rulesWait = 0;
                    _rulesPhase = 4;
                    return;
                }
                if (_rulesPhase == 4)
                {
                    stage = "web-b";
                    bool ended = TickExpandedSummoningDetachedAbility(_rulesWebCommand, _rulesWait);
                    if (!ended && _rulesWait++ < 420) return;
                    _rulesSteps.Add("webB:ended=" + ended + ";frames=" + _rulesWait);
                    EndExpandedSummoningDetachedAbility(_rulesWebCommand);
                    stage = "web";
                    FinishExpandedSummoningWeb();
                    CompleteExpandedSummoningRules();
                }
            }
            catch (Exception exception)
            {
                _rulesCases.Add(Assertion("expanded-summoning-correction-" + stage,
                    "the stage completes", "exception=" + exception.GetType().Name + ":" +
                    Sanitize(exception.Message) + ";steps=" + string.Join("||", _rulesSteps.ToArray()),
                    false, "the correction rules fixture"));
                CompleteExpandedSummoningRules();
            }
        }

        private void CompleteExpandedSummoningRules()
        {
            bool cleaned = false;
            SummonDocileHoovesComponent.SuspendedForFixture = false;
            if (_rulesObserver != null)
            {
                try { EventBus.Unsubscribe(_rulesObserver); } catch (Exception) { }
                _rulesObserver = null;
            }
            try
            {
                if (_rulesFixture != null && _rulesFixture.Hostile != null &&
                    !_rulesFixture.Hostile.Destroyed)
                {
                    _rulesFixture.Hostile.Descriptor.State.Size = _rulesFixture.HostileSize;
                    _rulesFixture.Hostile.Descriptor.Damage = _rulesHostileDamage;
                }
                if (_rulesFixture != null && _rulesFixture.Caster != null &&
                    !_rulesFixture.Caster.Destroyed)
                    _rulesFixture.Caster.Descriptor.Damage = _rulesCasterDamage;
                EndExpandedSummoningCorrectionFixture(_rulesFixture, out cleaned);
            }
            catch (Exception exception)
            {
                _rulesCases.Add(Assertion("expanded-summoning-correction-cleanup-exception",
                    "cleanup completes", "exception=" + exception.GetType().Name + ":" +
                    Sanitize(exception.Message), false, "the correction rules fixture"));
            }
            _rulesCases.Add(Assertion("expanded-summoning-correction-cleanup",
                "exact party and global-unit snapshots restored", "cleaned=" + cleaned, cleaned,
                "per-unit disposal and final exact snapshots"));
            _rulesCases.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(_rulesCases.All(value =>
                value.Status == "PASS") ? "PASS" : "FAIL", _rulesCases, null);
            foreach (string step in _rulesSteps) result.Diagnostics.Add("correction=" + step);
            if (_rulesFixture != null)
                foreach (string diagnostic in _rulesFixture.Evidence.Diagnostics)
                    result.Diagnostics.Add("correction=" + diagnostic);
            _rulesFixture = null;
            Complete(result);
        }

        private static bool ExerciseExpandedSummoningCorrectionCats(
            ExpandedSummoningCorrectionFixture fixture, out string detail)
        {
            var steps = new List<string>();
            bool ok = true;
            UnitEntityData hostile = fixture.Hostile;
            BlueprintBuff hold = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_Grapple_Hold");
            BlueprintBuff grappled = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_Grapple_Grappled");
            int damageBefore = hostile.Descriptor.Damage;
            try
            {
                foreach (string key in new[] { "leopard", "lion", "dire-lion", "tiger", "dire-tiger" })
                {
                    bool clawsGrab = key == "tiger" || key == "dire-tiger";
                    UnitEntityData cat = CastExpandedSummoningOwnTier(fixture, key);
                    cat.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                    SummonGrabComponent grab = SummonGrabComponent.Find(cat);
                    List<Kingmaker.Items.Slots.WeaponSlot> limbs = cat.Body.AdditionalLimbs;
                    int limbCount = limbs == null ? 0 : limbs.Count;
                    ItemEntityWeapon bite = cat.Body.PrimaryHand.MaybeWeapon;
                    if (grab == null || limbCount != 4 || bite == null)
                    {
                        steps.Add(key + ":grab=" + (grab != null) + ";limbs=" + limbCount);
                        ok = false;
                        continue;
                    }
                    ItemEntityWeapon[] claws = limbs.Select(value => value.MaybeWeapon).ToArray();
                    bool classification = grab.IsGrabLimb(cat, bite) &&
                        grab.IsGrabLimb(cat, claws[0]) == clawsGrab &&
                        grab.IsGrabLimb(cat, claws[1]) == clawsGrab &&
                        !grab.IsGrabLimb(cat, claws[2]) && !grab.IsGrabLimb(cat, claws[3]) &&
                        !SummonRakeComponent.IsRakeWeapon(cat, bite) &&
                        !SummonRakeComponent.IsRakeWeapon(cat, claws[0]) &&
                        !SummonRakeComponent.IsRakeWeapon(cat, claws[1]) &&
                        SummonRakeComponent.IsRakeWeapon(cat, claws[2]) &&
                        SummonRakeComponent.IsRakeWeapon(cat, claws[3]);
                    // Live grabs at a known hit: the rake claws never, the
                    // foreclaws only for the tiger and the smilodon, the bite always.
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool rakeRefused = !grab.TryGrab(hostile, claws[3], true) &&
                        !grab.TryGrab(hostile, claws[2], true);
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool clawGrabbed = grab.TryGrab(hostile, claws[0], true);
                    if (clawGrabbed) ReleaseExpandedSummoningHold(cat, hostile, hold);
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool biteGrabbed = grab.TryGrab(hostile, bite, true);
                    // A single attack (an attack of opportunity is one) never
                    // carries a rake slot; the full attack drops the rakes
                    // until the held foe has been held since the round began.
                    var single = new UnitAttack(hostile);
                    single.Init(cat);
                    List<AttackHandInfo> singleAttacks = single.CreateSingleAttack();
                    bool singleClean = singleAttacks.Count == 1 && singleAttacks[0] != null &&
                        singleAttacks[0].Hand != null && !SummonLimbs.IsRakeSlot(cat,
                            singleAttacks[0].Hand, grab.RakeLimbCount);
                    int rakeSlots, attacks;
                    string sameTurn = ExpandedSummoningPlanFullAttack(cat, hostile, false,
                        grab.RakeLimbCount, out rakeSlots, out attacks);
                    bool sameTurnDropped = rakeSlots == 0 && attacks == 3;
                    string sequence = "not-held";
                    if (biteGrabbed)
                    {
                        hostile.Descriptor.Buffs.GetBuff(grappled).TickMechanics();
                        sequence = ExerciseExpandedSummoningRakeSequence(cat, hostile,
                            fixture.Caster);
                    }
                    bool sequenceExact = sequence.Contains("held=kept(5/2)") &&
                        sequence.Contains("other=dropped(3/0)") &&
                        sequence.Contains("charge=kept(5/2)") &&
                        sequence.Contains("ordinary=dropped(3/0)");
                    ReleaseExpandedSummoningHold(cat, hostile, hold);
                    bool released = hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleTarget>() ==
                        null && !hostile.Descriptor.HasFact(grappled);
                    steps.Add(key + ":classification=" + classification + ";rakeRefused=" +
                        rakeRefused + ";clawGrabbed=" + clawGrabbed + "(expected " + clawsGrab +
                        ");biteGrabbed=" + biteGrabbed + ";singleAttackClean=" + singleClean +
                        ";sameTurnPlan=" + sameTurn + ";sequence[" + sequence + "];released=" +
                        released);
                    ok = ok && classification && rakeRefused && clawGrabbed == clawsGrab &&
                        biteGrabbed && singleClean && sameTurnDropped && sequenceExact && released;
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { cat });
                }
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + exception.GetType().Name + ":" + Sanitize(exception.Message));
                ok = false;
            }
            finally
            {
                hostile.Descriptor.Damage = damageBefore;
            }
            detail = string.Join(";", steps.ToArray());
            return ok;
        }

        private static bool ExerciseExpandedSummoningCorrectionGrappleSizes(
            ExpandedSummoningCorrectionFixture fixture, out string detail)
        {
            var steps = new List<string>();
            bool ok = true;
            UnitEntityData hostile = fixture.Hostile;
            BlueprintBuff hold = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_Grapple_Hold");
            BlueprintBuff grappled = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_Grapple_Grappled");
            BlueprintBuff swallowed = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_PurpleWorm_Swallowed");
            int damageBefore = hostile.Descriptor.Damage;
            try
            {
                foreach (string[] row in new[] {
                    new[] { "grizzly-bear", "Large" }, new[] { "leopard", "Medium" } })
                {
                    UnitEntityData holder = CastExpandedSummoningOwnTier(fixture, row[0]);
                    holder.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                    SummonGrabComponent grab = SummonGrabComponent.Find(holder);
                    ItemEntityWeapon limb = grab == null ? null : grab.FirstGrabWeapon(holder);
                    Size holderSize = holder.Descriptor.State.Size;
                    var outcomes = new List<string>();
                    bool rowOk = grab != null && limb != null && holderSize.ToString() == row[1];
                    foreach (int delta in new[] { -1, 0, 1 })
                    {
                        Size targetSize = (Size)((int)holderSize + delta);
                        hostile.Descriptor.State.Size = targetSize;
                        UnityEngine.Random.InitState(FindNativeD20Seed(20));
                        bool grabbed = grab != null && limb != null &&
                            grab.TryGrab(hostile, limb, true);
                        int grappleCmb = 0, tripCmb = 0;
                        if (grabbed)
                        {
                            var grapple = new RuleCalculateCMB(holder, hostile, CombatManeuver.Grapple);
                            Rulebook.Trigger(grapple);
                            var trip = new RuleCalculateCMB(holder, hostile, CombatManeuver.Trip);
                            Rulebook.Trigger(trip);
                            grappleCmb = grapple.Result;
                            tripCmb = trip.Result;
                            ReleaseExpandedSummoningHold(holder, hostile, hold);
                        }
                        bool expected = delta <= 0;
                        outcomes.Add(targetSize + ":grabbed=" + grabbed + (grabbed ?
                            ";maintainCmb=" + grappleCmb + ";tripCmb=" + tripCmb : ""));
                        rowOk = rowOk && grabbed == expected &&
                            (!grabbed || grappleCmb - tripCmb ==
                                ExpandedSummoningSpecialProfiles.SummonGrabManeuverBonus +
                                ExpandedSummoningSpecialProfiles.SummonHoldMaintainBonus);
                    }
                    hostile.Descriptor.State.Size = fixture.HostileSize;
                    // The +4 grab bonus alone, with nothing held.
                    var freeGrapple = new RuleCalculateCMB(holder, hostile, CombatManeuver.Grapple);
                    Rulebook.Trigger(freeGrapple);
                    var freeTrip = new RuleCalculateCMB(holder, hostile, CombatManeuver.Trip);
                    Rulebook.Trigger(freeTrip);
                    bool grabBonus = freeGrapple.Result - freeTrip.Result ==
                        ExpandedSummoningSpecialProfiles.SummonGrabManeuverBonus;
                    steps.Add(row[0] + ":size=" + holderSize + ";" + string.Join(",",
                        outcomes.ToArray()) + ";freeGrappleCmb=" + freeGrapple.Result +
                        ";freeTripCmb=" + freeTrip.Result + ";grabBonus=" + grabBonus);
                    ok = ok && rowOk && grabBonus;
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { holder });
                }

                // The worm: a Gargantuan foe is grabbed (same size) and held
                // through a successful later-turn check without a swallow; a
                // Colossal foe is refused outright.
                UnitEntityData worm = CastExpandedSummoningOwnTier(fixture, "purple-worm");
                worm.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                SummonGrabComponent wormGrab = SummonGrabComponent.Find(worm);
                ItemEntityWeapon wormBite = worm.Body.PrimaryHand.MaybeWeapon;
                hostile.Descriptor.State.Size = Size.Gargantuan;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool gargantuanGrabbed = wormGrab.TryGrab(hostile, wormBite, true);
                bool sizeRefused = !wormGrab.IsSwallowSizeAllowed(worm, hostile);
                string maintainOutcome = "not-held";
                if (gargantuanGrabbed)
                {
                    hostile.Descriptor.Buffs.GetBuff(grappled).TickMechanics();
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    worm.Descriptor.Buffs.GetBuff(hold).TickMechanics();
                    maintainOutcome = "swallowed=" + (hostile.Get<Kingmaker.UnitLogic.Parts
                            .UnitPartSwallowed>() != null) + ";stillHeld=" +
                        (hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleTarget>() != null) +
                        ";damage=" + damageBefore + "->" + hostile.Descriptor.Damage;
                }
                bool notSwallowed = gargantuanGrabbed &&
                    hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowed>() == null &&
                    hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleTarget>() != null &&
                    !hostile.Descriptor.HasFact(swallowed) && hostile.Descriptor.Damage > damageBefore;
                ReleaseExpandedSummoningHold(worm, hostile, hold);
                hostile.Descriptor.State.Size = Size.Colossal;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool colossalRefused = !wormGrab.TryGrab(hostile, wormBite, true);
                hostile.Descriptor.State.Size = Size.Huge;
                bool hugeAllowed = wormGrab.IsSwallowSizeAllowed(worm, hostile);
                hostile.Descriptor.State.Size = fixture.HostileSize;
                steps.Add("worm:gargantuanGrabbed=" + gargantuanGrabbed + ";swallowSizeRefused=" +
                    sizeRefused + ";" + maintainOutcome + ";notSwallowed=" + notSwallowed +
                    ";colossalRefused=" + colossalRefused + ";hugeSwallowAllowed=" + hugeAllowed);
                ok = ok && gargantuanGrabbed && sizeRefused && notSwallowed && colossalRefused &&
                    hugeAllowed;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { worm });
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + exception.GetType().Name + ":" + Sanitize(exception.Message));
                ok = false;
            }
            finally
            {
                hostile.Descriptor.State.Size = fixture.HostileSize;
                hostile.Descriptor.Damage = damageBefore;
            }
            detail = string.Join(";", steps.ToArray());
            return ok;
        }

        private static bool ExerciseExpandedSummoningCorrectionFlytrap(
            ExpandedSummoningCorrectionFixture fixture, out string detail)
        {
            var steps = new List<string>();
            bool ok = true;
            UnitEntityData hostile = fixture.Hostile;
            BlueprintBuff multiHold = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_Grapple_MultiHold");
            BlueprintBuff multiHeld = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_Grapple_MultiHeld");
            BlueprintBuff engulfed = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_GiantFlytrap_Engulfed");
            BlueprintBuff flytrapTraits = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_GiantFlytrap_CombatTraits");
            int damageBefore = hostile.Descriptor.Damage;
            var wolves = new List<UnitEntityData>();
            try
            {
                UnitEntityData flytrap = CastExpandedSummoningOwnTier(fixture, "giant-flytrap");
                flytrap.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                SummonGrabComponent grab = SummonGrabComponent.Find(flytrap);
                List<Kingmaker.Items.Slots.WeaponSlot> limbs = flytrap.Body.AdditionalLimbs;
                var bites = new List<ItemEntityWeapon> { flytrap.Body.PrimaryHand.MaybeWeapon };
                if (limbs != null) bites.AddRange(limbs.Select(value => value.MaybeWeapon));
                bool fourBites = bites.Count == 4 && bites.All(value => value != null) &&
                    grab != null && grab.MaxHeldTargets == 4;
                for (int index = 0; index < 4; index++)
                {
                    UnitEntityData wolf = CastExpandedSummoningVariant(fixture.Blueprints,
                        fixture.Caster, ExpandedSummoningVariant(SummonFamily.NaturesAlly, "wolf", 2,
                            SummonMultiplicity.One), null, fixture.Evidence).Single();
                    fixture.Created.Add(wolf);
                    RemoveExpandedSummoningAppearanceBuffs(wolf);
                    wolf.Descriptor.Stats.HitPoints.BaseValue = 100000;
                    wolf.Descriptor.Stats.BaseAttackBonus.BaseValue = -100;
                    SetExpandedSummoningBrainActive(wolf, false);
                    wolves.Add(wolf);
                }
                // Four targets around the flytrap, within its bites' reach.
                Vector3 centre = flytrap.Position;
                hostile.Descriptor.Stats.BaseAttackBonus.BaseValue = -100;
                hostile.Translocate(centre + new Vector3(2.5f, 0f, 0f), null);
                wolves[0].Translocate(centre + new Vector3(-2.5f, 0f, 0f), null);
                wolves[1].Translocate(centre + new Vector3(0f, 0f, 2.5f), null);
                wolves[2].Translocate(centre + new Vector3(0f, 0f, -2.5f), null);
                wolves[3].Translocate(centre + new Vector3(2.5f, 0f, 2.5f), null);
                Func<UnitEntityData, string> linkOf = target =>
                {
                    Buff state = SummonHoldComponent.HeldState(flytrap, target, grab);
                    ItemEntityWeapon weapon = SummonGrappleLinks.EstablishingWeapon(state);
                    return state == null ? "none" : "holder=" + ReferenceEquals(
                        state.Context == null ? null : state.Context.MaybeCaster, flytrap) +
                        ",bite=" + bites.IndexOf(weapon);
                };
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool link0 = fourBites && grab.TryGrab(hostile, bites[0], true);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool link1 = fourBites && grab.TryGrab(wolves[0], bites[1], true);
                // The bite that holds the hostile cannot take another foe.
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool busyBiteRefused = fourBites && !grab.TryGrab(wolves[1], bites[0], true);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool link2 = fourBites && grab.TryGrab(wolves[1], bites[2], true);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool link3 = fourBites && grab.TryGrab(wolves[2], bites[3], true);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool fifthRefused = fourBites && !grab.TryGrab(wolves[3], bites[0], true) &&
                    !grab.TryGrab(wolves[3], bites[1], true);
                List<UnitEntityData> held = SummonMultiHoldComponent.HeldTargets(flytrap, multiHeld);
                string links = "hostile[" + linkOf(hostile) + "];wolf0[" + linkOf(wolves[0]) +
                    "];wolf1[" + linkOf(wolves[1]) + "];wolf2[" + linkOf(wolves[2]) + "]";
                bool distinct = held.Count == 4 && links == "hostile[holder=True,bite=0];wolf0[holder=True,bite=1];wolf1[holder=True,bite=2];wolf2[holder=True,bite=3]" &&
                    flytrap.Descriptor.HasFact(multiHold) &&
                    hostile.Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                    hostile.Descriptor.State.HasCondition(UnitCondition.Entangled) &&
                    !flytrap.Descriptor.State.HasCondition(UnitCondition.CantAct);
                steps.Add("links:fourBites=" + fourBites + ";link0=" + link0 + ";link1=" + link1 +
                    ";busyBiteRefused=" + busyBiteRefused + ";link2=" + link2 + ";link3=" + link3 +
                    ";fifthRefused=" + fifthRefused + ";held=" + held.Count + ";" + links +
                    ";distinct=" + distinct);
                ok = ok && fourBites && link0 && link1 && busyBiteRefused && link2 && link3 &&
                    fifthRefused && distinct;

                // Crowded pathing: the party caster, standing beside the crowd,
                // asks the game's own pathfinder for a route to the far side.
                string pathing = "not-run";
                bool pathFound = false;
                try
                {
                    fixture.Caster.Translocate(centre + new Vector3(-6f, 0f, -6f), null);
                    UnitMovementAgent agent = fixture.Caster.View == null ? null :
                        fixture.Caster.View.MovementAgent as UnitMovementAgent;
                    if (agent == null) pathing = "no-agent";
                    else
                    {
                        Vector3 destination = centre + new Vector3(6f, 0f, 6f);
                        Pathfinding.Path path = agent.FindPath(destination,
                            new OnPathDelegate(value => { }), null, false, 0);
                        if (path == null) pathing = "no-path-request";
                        else
                        {
                            AstarPath.WaitForPath(path);
                            int points = path.vectorPath == null ? 0 : path.vectorPath.Count;
                            ObstaclePathingResult obstacles = ObstaclePathfinder
                                .PathAroundStandingObstacles(path, agent, null);
                            pathFound = !path.error && points > 1 &&
                                obstacles != ObstaclePathingResult.NoPath;
                            pathing = "error=" + path.error + ";points=" + points +
                                ";obstacles=" + obstacles + ";heldStanding=" +
                                SummonMultiHoldComponent.HeldTargets(flytrap, multiHeld).Count;
                        }
                    }
                }
                catch (Exception exception)
                {
                    pathing = "exception=" + exception.GetType().Name + ":" +
                        Sanitize(exception.Message);
                }
                steps.Add("pathing[" + pathing + "]");
                ok = ok && pathFound;

                // Engulf: a Large held foe is maintained (damage, still held);
                // a Medium one is engulfed on the later turn.
                Buff hostileState = SummonHoldComponent.HeldState(flytrap, hostile, grab);
                if (hostileState != null) hostileState.TickMechanics();
                bool eligible = SummonHoldComponent.IsHeldSinceRoundStart(flytrap, hostile);
                hostile.Descriptor.State.Size = Size.Large;
                int beforeLarge = hostile.Descriptor.Damage;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                flytrap.Descriptor.Buffs.GetBuff(multiHold).TickMechanics();
                bool largeMaintained = hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowed>() ==
                    null && ReferenceEquals(SummonHeldComponent.HolderOf(hostile, multiHeld), flytrap) &&
                    hostile.Descriptor.Damage > beforeLarge;
                int stillHeldAfterLarge = SummonMultiHoldComponent.HeldTargets(flytrap, multiHeld).Count;
                hostile.Descriptor.State.Size = Size.Medium;
                int beforeEngulf = hostile.Descriptor.Damage;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                flytrap.Descriptor.Buffs.GetBuff(multiHold).TickMechanics();
                Kingmaker.UnitLogic.Parts.UnitPartSwallowed swallowedPart =
                    hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowed>();
                Kingmaker.UnitLogic.Parts.UnitPartSwallowWhole swallower =
                    flytrap.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowWhole>();
                bool engulfedNow = swallowedPart != null &&
                    ReferenceEquals(swallowedPart.Swallower.Value, flytrap) &&
                    hostile.Descriptor.HasFact(engulfed) &&
                    SummonHeldComponent.HolderOf(hostile, multiHeld) == null &&
                    swallower != null && swallower.SwallowedUnits.Count == 1 &&
                    hostile.Descriptor.Damage > beforeEngulf;
                int beforeEngulfTick = hostile.Descriptor.Damage;
                Buff engulfedState = hostile.Descriptor.Buffs.GetBuff(engulfed);
                if (engulfedState != null) engulfedState.TickMechanics();
                bool engulfDamage = hostile.Descriptor.Damage > beforeEngulfTick;
                int heldAfterEngulf = SummonMultiHoldComponent.HeldTargets(flytrap, multiHeld).Count;
                steps.Add("engulf:eligible=" + eligible + ";largeMaintained=" + largeMaintained +
                    ";heldAfterLarge=" + stillHeldAfterLarge + ";engulfed=" + engulfedNow +
                    ";engulfTickDamage=" + engulfDamage + ";heldAfterEngulf=" + heldAfterEngulf +
                    ";hostile=" + DescribeExpandedSummoningCondition(hostile));
                ok = ok && eligible && largeMaintained && stillHeldAfterLarge == 4 && engulfedNow &&
                    engulfDamage && heldAfterEngulf == 3;

                // Release paths. Escape: a held wolf with an overwhelming check
                // breaks free at its own tick and only its link ends.
                wolves[0].Descriptor.Stats.BaseAttackBonus.BaseValue = 200;
                Buff wolfState = SummonHoldComponent.HeldState(flytrap, wolves[0], grab);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                if (wolfState != null) wolfState.TickMechanics();
                bool escaped = SummonHeldComponent.HolderOf(wolves[0], multiHeld) == null &&
                    !wolves[0].Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                    SummonMultiHoldComponent.HeldTargets(flytrap, multiHeld).Count == 2 &&
                    flytrap.Descriptor.HasFact(multiHold);
                // The area-leave sweep releases a held unit it is handed.
                int swept = SummonGrappleAreaSafeguard.Sweep(true, new[] { wolves[1] });
                bool sweptFree = swept == 1 &&
                    SummonHeldComponent.HolderOf(wolves[1], multiHeld) == null &&
                    SummonMultiHoldComponent.HeldTargets(flytrap, multiHeld).Count == 1;
                // The holder's own hold ending (dismissal, death, dispel)
                // releases the last link; the buff is gone with it.
                flytrap.Descriptor.Buffs.RemoveFact(flytrap.Descriptor.Buffs.GetBuff(multiHold));
                bool holdEnded = SummonHeldComponent.HolderOf(wolves[2], multiHeld) == null &&
                    !wolves[2].Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                    SummonMultiHoldComponent.HeldTargets(flytrap, multiHeld).Count == 0 &&
                    !flytrap.Descriptor.HasFact(multiHold);
                // The swallow lifecycle: the traits ending spits the engulfed foe out.
                flytrap.Descriptor.Buffs.RemoveFact(flytrap.Descriptor.Buffs.GetBuff(flytrapTraits));
                bool spatOut = hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowed>() == null &&
                    !hostile.Descriptor.HasFact(engulfed) &&
                    !hostile.Descriptor.State.HasCondition(UnitCondition.CantAct) &&
                    swallower.SwallowedUnits.Count == 0;
                // Re-established links end with the holder's disposal.
                flytrap.Descriptor.AddBuff(flytrapTraits, flytrap, null);
                SummonGrabComponent grabAgain = SummonGrabComponent.Find(flytrap);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool regrabbed = grabAgain != null && grabAgain.TryGrab(wolves[3], bites[0], true);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { flytrap });
                bool disposalReleased = SummonHeldComponent.HolderOf(wolves[3], multiHeld) == null &&
                    !wolves[3].Descriptor.State.HasCondition(UnitCondition.CantMove);
                steps.Add("release:escaped=" + escaped + ";swept=" + swept + ";sweptFree=" +
                    sweptFree + ";holdEnded=" + holdEnded + ";spatOut=" + spatOut +
                    ";regrabbed=" + regrabbed + ";disposalReleased=" + disposalReleased);
                ok = ok && escaped && sweptFree && holdEnded && spatOut && regrabbed &&
                    disposalReleased;
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + exception.GetType().Name + ":" + Sanitize(exception.Message));
                ok = false;
            }
            finally
            {
                hostile.Descriptor.State.Size = fixture.HostileSize;
                hostile.Descriptor.Damage = damageBefore;
                hostile.Descriptor.Stats.BaseAttackBonus.BaseValue = 0;
                foreach (UnitEntityData wolf in wolves)
                    if (fixture.Created.Contains(wolf))
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { wolf });
                fixture.Caster.Translocate(fixture.Hostile.Position + Vector3.back, null);
            }
            detail = string.Join(";", steps.ToArray());
            return ok;
        }

        private static BlueprintItemWeapon ExpandedSummoningWeaponOfCategory(
            BlueprintScriptableObject[] blueprints, WeaponCategory category)
        {
            return blueprints.OfType<BlueprintItemWeapon>().Where(value =>
                value.Category == category && !value.IsNatural && value.Type != null)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
        }

        /// <summary>Equips a weapon in the hostile's primary hand for one attack and puts the original back.</summary>
        private static string ExpandedSummoningHostileAttackWith(UnitEntityData hostile,
            UnitEntityData target, BlueprintItemWeapon blueprint, out bool autoMiss,
            out int missChance, out bool hit)
        {
            autoMiss = false;
            missChance = 0;
            hit = false;
            if (blueprint == null) return "no-blueprint";
            ItemEntity original = hostile.Body.PrimaryHand.MaybeItem;
            ItemEntity originalOff = hostile.Body.SecondaryHand.MaybeItem;
            var weapon = new ItemEntityWeapon(blueprint);
            string outcome;
            try
            {
                if (original != null) hostile.Body.PrimaryHand.RemoveItem(false);
                if (originalOff != null) hostile.Body.SecondaryHand.RemoveItem(false);
                hostile.Body.PrimaryHand.InsertItem(weapon);
                hostile.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                int damageBefore = target.Descriptor.Damage;
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                var attack = new RuleAttackWithWeapon(hostile, target, weapon, 0);
                Rulebook.Trigger(attack);
                autoMiss = attack.AttackRoll != null && attack.AttackRoll.AutoMiss;
                missChance = attack.AttackRoll == null ? -1 : attack.AttackRoll.MissChance;
                hit = attack.AttackRoll != null && attack.AttackRoll.IsHit;
                outcome = blueprint.name + ":type=" + (attack.AttackRoll == null ? "<none>" :
                    attack.AttackRoll.AttackType.ToString()) + ",autoMiss=" + autoMiss +
                    ",missChance=" + missChance + ",hit=" + hit;
                target.Descriptor.Damage = damageBefore;
            }
            finally
            {
                if (ReferenceEquals(hostile.Body.PrimaryHand.MaybeItem, weapon))
                    hostile.Body.PrimaryHand.RemoveItem(false);
                weapon.Dispose();
                if (original != null && hostile.Body.PrimaryHand.MaybeItem == null)
                    hostile.Body.PrimaryHand.InsertItem(original);
                if (originalOff != null && hostile.Body.SecondaryHand.MaybeItem == null)
                    hostile.Body.SecondaryHand.InsertItem(originalOff);
            }
            return outcome;
        }

        private static AreaEffectEntityData FindExpandedSummoningArea(string blueprintName)
        {
            foreach (AreaEffectEntityData area in Game.Instance.State.AreaEffects)
                if (area != null && !area.IsEnded && area.Blueprint != null &&
                    area.Blueprint.name == blueprintName)
                    return area;
            return null;
        }

        private static void EndExpandedSummoningArea(AreaEffectEntityData area)
        {
            if (area == null) return;
            area.ForceEnd();
            for (int tick = 0; tick < 3; tick++) area.Tick();
            if (!area.Destroyed) area.Destroy();
            Game.Instance.EntityDestroyer.Tick();
        }

        // --- the wind wall (frames) ---------------------------------------------------------------------------------

        private void BeginExpandedSummoningWindWall()
        {
            ExpandedSummoningCorrectionFixture fixture = _rulesFixture;
            _rulesWolf = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                ExpandedSummoningVariant(SummonFamily.NaturesAlly, "wolf", 2, SummonMultiplicity.One),
                null, fixture.Evidence).Single();
            fixture.Created.Add(_rulesWolf);
            RemoveExpandedSummoningAppearanceBuffs(_rulesWolf);
            _rulesWolf.Descriptor.Stats.HitPoints.BaseValue = 100000;
            SetExpandedSummoningBrainActive(_rulesWolf, false);
            SetExactProperty(fixture.Hostile.Descriptor.Stats.GetStat(StatType.SaveWill), "BaseValue", -100);
            SetExactProperty(fixture.Hostile.Descriptor.Stats.GetStat(StatType.SaveFortitude), "BaseValue", -100);
            SetExactProperty(fixture.Hostile.Descriptor.Stats.GetStat(StatType.SaveReflex), "BaseValue", -100);
            _rulesMephit = CastExpandedSummoningQuietUnit(fixture, "dust-mephit");
            BlueprintAbility windWall = fixture.Blueprints.OfType<BlueprintAbility>().Single(value =>
                value.name == "KMG_Summoning_Special_DustMephit_SpellLikeTwo");
            Vector3 wallCentre = _rulesMephit.Position;
            fixture.Caster.Translocate(wallCentre + new Vector3(1f, 0f, 0f), null);
            _rulesWolf.Translocate(wallCentre + new Vector3(-1f, 0f, 0f), null);
            fixture.Hostile.Translocate(wallCentre + new Vector3(12f, 0f, 0f), null);
            SummonWindWallComponent.ClearOutcomes();
            ExecuteExpandedSummoningRuntimeAbility(_rulesMephit, windWall, 3,
                new TargetWrapper(_rulesMephit), false);
            Game.Instance.EntityCreator.Tick();
            _rulesSteps.Add("windWall:cast;execution=" + _expandedSummoningLastAbilityExecution);
        }

        private void FinishExpandedSummoningWindWall()
        {
            ExpandedSummoningCorrectionFixture fixture = _rulesFixture;
            UnitEntityData hostile = fixture.Hostile, caster = fixture.Caster, wolf = _rulesWolf,
                dust = _rulesMephit;
            BlueprintScriptableObject[] blueprints = fixture.Blueprints;
            BlueprintBuff windWallState = blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_DustMephit_WindWallState");
            AreaEffectEntityData wall = FindExpandedSummoningArea(
                "KMG_Summoning_Special_DustMephit_WindWallArea");
            int inside = wall == null ? -1 : wall.UnitsInside.Count();
            bool sheltered = wall != null && caster.Descriptor.HasFact(windWallState) &&
                wolf.Descriptor.HasFact(windWallState) && dust.Descriptor.HasFact(windWallState) &&
                !hostile.Descriptor.HasFact(windWallState);
            bool bowMiss, thrownMiss, meleeMiss, rayMiss, bowHit, thrownHit, meleeHit, rayHit;
            int bowChance, thrownChance, meleeChance, rayChance;
            string bow = ExpandedSummoningHostileAttackWith(hostile, caster,
                ExpandedSummoningWeaponOfCategory(blueprints, WeaponCategory.Longbow),
                out bowMiss, out bowChance, out bowHit);
            BlueprintItemWeapon thrownBlueprint =
                ExpandedSummoningWeaponOfCategory(blueprints, WeaponCategory.ThrowingAxe) ??
                ExpandedSummoningWeaponOfCategory(blueprints, WeaponCategory.Dart) ??
                ExpandedSummoningWeaponOfCategory(blueprints, WeaponCategory.Sling);
            string thrown = ExpandedSummoningHostileAttackWith(hostile, caster, thrownBlueprint,
                out thrownMiss, out thrownChance, out thrownHit);
            string melee = ExpandedSummoningHostileAttackWith(hostile, caster,
                ExpandedSummoningWeaponOfCategory(blueprints, WeaponCategory.Longsword),
                out meleeMiss, out meleeChance, out meleeHit);
            string ray;
            try
            {
                BlueprintItemWeapon rayBlueprint = blueprints.OfType<BlueprintItemWeapon>()
                    .Single(value => value.AssetGuid ==
                        ExpandedSummoningSpecialBuilder.NativeRayWeaponGuid);
                var rayWeapon = new ItemEntityWeapon(rayBlueprint);
                int before = caster.Descriptor.Damage;
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                var rayAttack = new RuleAttackWithWeapon(hostile, caster, rayWeapon, 0);
                Rulebook.Trigger(rayAttack);
                rayMiss = rayAttack.AttackRoll != null && rayAttack.AttackRoll.AutoMiss;
                rayChance = rayAttack.AttackRoll == null ? -1 : rayAttack.AttackRoll.MissChance;
                rayHit = rayAttack.AttackRoll != null && rayAttack.AttackRoll.IsHit;
                ray = "ray:type=" + (rayAttack.AttackRoll == null ? "<none>" :
                    rayAttack.AttackRoll.AttackType.ToString()) + ",autoMiss=" + rayMiss +
                    ",missChance=" + rayChance + ",hit=" + rayHit;
                caster.Descriptor.Damage = before;
                rayWeapon.Dispose();
            }
            catch (Exception exception)
            {
                rayMiss = false; rayChance = 0; rayHit = false;
                ray = "ray:exception=" + exception.GetType().Name;
            }
            string wallOutcomes = string.Join("|", SummonWindWallComponent.ObservedOutcomes.ToArray());
            EndExpandedSummoningArea(wall);
            bool wallEnded = !caster.Descriptor.HasFact(windWallState) &&
                !wolf.Descriptor.HasFact(windWallState);
            string detail = "windWall:area=" + (wall != null) + ";inside=" + inside + ";sheltered=" +
                sheltered + ";bow[" + bow + "];thrown[" + thrown + "];melee[" + melee + "];" + ray +
                ";outcomes=" + Sanitize(wallOutcomes) + ";ended=" + wallEnded;
            bool ok = sheltered && bowMiss && !bowHit && !thrownMiss &&
                thrownChance == ExpandedSummoningSpecialProfiles.WindWallOtherRangedMissChance &&
                !meleeMiss && meleeChance == 0 && !rayMiss && rayChance == 0 && wallEnded;
            _rulesCases.Add(Assertion("expanded-summoning-correction-wind-wall",
                "the dust mephit's wall of wind shelters the allies inside it - the party caster, the allied summon and the mephit - and not the hostile outside: arrows and bolts aimed at a sheltered ally are deflected outright, other ranged weapons roll the tabletop 30% miss chance, melee and rays pass; the shelter ends with the wall",
                detail, ok, "the area effect found through the game's own grid on the frames after placement; RuleAttackWithWeapon by the hostile with a bow, a thrown weapon, a sword and the ray weapon; SummonWindWallComponent outcomes"));
            caster.Descriptor.Damage = _rulesCasterDamage;
            DisposeExpandedSummoningUnits(fixture.Created, new[] { dust });
            _rulesMephit = null;
        }

        // --- the ally-safe cloud (frames) --------------------------------------------------------------------------------

        private void BeginExpandedSummoningCloud()
        {
            ExpandedSummoningCorrectionFixture fixture = _rulesFixture;
            _rulesMephit = CastExpandedSummoningQuietUnit(fixture, "ooze-mephit");
            BlueprintAbility cloud = fixture.Blueprints.OfType<BlueprintAbility>().Single(value =>
                value.name == "KMG_Summoning_Special_OozeMephit_SpellLikeTwo");
            Vector3 cloudCentre = _rulesMephit.Position + new Vector3(3f, 0f, 0f);
            fixture.Hostile.Translocate(cloudCentre, null);
            fixture.Caster.Translocate(cloudCentre + new Vector3(1f, 0f, 0f), null);
            _rulesWolf.Translocate(cloudCentre + new Vector3(-1f, 0f, 0f), null);
            _rulesMephit.Translocate(cloudCentre + new Vector3(0f, 0f, 1f), null);
            bool nauseatedBefore = fixture.Hostile.Descriptor.State.HasCondition(UnitCondition.Nauseated);
            ExecuteExpandedSummoningRuntimeAbility(_rulesMephit, cloud, 3, new TargetWrapper(cloudCentre), false);
            Game.Instance.EntityCreator.Tick();
            _rulesSteps.Add("cloud:cast;nauseatedBefore=" + nauseatedBefore + ";execution=" +
                _expandedSummoningLastAbilityExecution);
        }

        private void FinishExpandedSummoningCloud()
        {
            ExpandedSummoningCorrectionFixture fixture = _rulesFixture;
            UnitEntityData hostile = fixture.Hostile, caster = fixture.Caster, wolf = _rulesWolf,
                ooze = _rulesMephit;
            AreaEffectEntityData cloudArea = FindExpandedSummoningArea(
                "KMG_Summoning_Special_OozeMephit_StinkingCloudArea");
            int inside = cloudArea == null ? -1 : cloudArea.UnitsInside.Count();
            string insideNames = cloudArea == null ? "<no-area>" : string.Join(",",
                cloudArea.UnitsInside.Select(value => value.Blueprint == null ? "?" :
                    value.Blueprint.name).ToArray());
            bool hostileNauseated = hostile.Descriptor.State.HasCondition(UnitCondition.Nauseated);
            bool casterClean = !caster.Descriptor.State.HasCondition(UnitCondition.Nauseated);
            bool wolfClean = !wolf.Descriptor.State.HasCondition(UnitCondition.Nauseated);
            bool oozeClean = !ooze.Descriptor.State.HasCondition(UnitCondition.Nauseated);
            bool hostileInside = cloudArea != null && cloudArea.UnitsInside.Contains(hostile);
            bool alliesInside = cloudArea != null && cloudArea.UnitsInside.Contains(caster) &&
                cloudArea.UnitsInside.Contains(wolf);
            EndExpandedSummoningArea(cloudArea);
            foreach (UnitEntityData unit in new[] { hostile, caster, wolf, ooze })
                foreach (Buff buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                        value.Blueprint != null && value.Blueprint.name.IndexOf("StinkingCloud",
                            StringComparison.Ordinal) >= 0).ToArray())
                    unit.Descriptor.Buffs.RemoveFact(buff);
            string detail = "cloud:area=" + (cloudArea != null) + ";inside=" + inside + "(" +
                Sanitize(insideNames) + ");hostileInside=" + hostileInside + ";alliesInside=" + alliesInside +
                ";hostileNauseated=" + hostileNauseated + ";partyCasterClean=" + casterClean +
                ";alliedSummonClean=" + wolfClean + ";mephitClean=" + oozeClean;
            bool ok = cloudArea != null && hostileInside && alliesInside && hostileNauseated &&
                casterClean && wolfClean && oozeClean;
            _rulesCases.Add(Assertion("expanded-summoning-correction-cloud",
                "the ooze mephit's stinking cloud, placed where the hostile, the party caster, an allied summon and the mephit all stand inside it (the only placement, no safe target), nauseates the hostile and touches no ally",
                detail, ok, "the project ally-safe area clone found through the game's own grid on the frames after placement; UnitsInside and the nauseated condition per unit"));
            DisposeExpandedSummoningUnits(fixture.Created, new[] { ooze });
            _rulesMephit = null;
        }

        // --- the web (frames) --------------------------------------------------------------------------------------------

        private void SetExpandedSummoningHostileEscape(int value)
        {
            UnitEntityData hostile = _rulesFixture.Hostile;
            hostile.Descriptor.Stats.BaseAttackBonus.BaseValue = value;
            SetExactProperty(hostile.Descriptor.Stats.GetStat(StatType.SkillMobility), "BaseValue", value);
            SetExactProperty(hostile.Descriptor.Stats.GetStat(StatType.SkillAthletics), "BaseValue", value);
        }

        private void BeginExpandedSummoningWeb()
        {
            ExpandedSummoningCorrectionFixture fixture = _rulesFixture;
            UnitEntityData hostile = fixture.Hostile;
            _rulesWebbed = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.AssetGuid == ExpandedSummoningSpecialBuilder.NativeWebGrappledGuid);
            _rulesSpider = CastExpandedSummoningQuietUnit(fixture, "giant-spider");
            _rulesWeb = fixture.Blueprints.OfType<BlueprintAbility>().Single(value =>
                value.name == "KMG_Summoning_Special_GiantSpider_Web");
            _rulesWebResource = fixture.Blueprints.OfType<BlueprintAbilityResource>()
                .Single(value => value.name == "KMG_Summoning_Special_GiantSpider_WebResource");
            hostile.Translocate(_rulesSpider.Position + new Vector3(3f, 0f, 0f), null);
            fixture.Caster.Translocate(_rulesSpider.Position + new Vector3(-3f, 0f, 0f), null);
            Func<AbilityData> data = () => new AbilityData(_rulesSpider.Descriptor.Abilities.GetAbility(_rulesWeb));
            Size spiderSize = _rulesSpider.Descriptor.State.Size;
            hostile.Descriptor.State.Size = (Size)((int)spiderSize + 1);
            bool oneLargerAllowed = data().CanTarget(new TargetWrapper(hostile));
            hostile.Descriptor.State.Size = (Size)((int)spiderSize + 2);
            bool twoLargerRefused = !data().CanTarget(new TargetWrapper(hostile));
            hostile.Descriptor.State.Size = fixture.HostileSize;
            _rulesHostileAc = hostile.Descriptor.Stats.AC.BaseValue;
            // High touch AC, hopeless Reflex: the touch attack misses.
            hostile.Descriptor.Stats.AC.BaseValue = 110;
            SetExactProperty(hostile.Descriptor.Stats.GetStat(StatType.SaveReflex), "BaseValue", -100);
            var touchAc = new RuleCalculateAC(_rulesSpider, hostile, AttackType.RangedTouch);
            Rulebook.Trigger(touchAc);
            _rulesHighTouch = touchAc.TargetAC;
            _rulesWebUses0 = _rulesSpider.Descriptor.Resources.GetResourceAmount(_rulesWebResource);
            _rulesObserver = new ExpandedSummoningAttackRollObserver { Initiator = _rulesSpider };
            EventBus.Subscribe(_rulesObserver);
            UnityEngine.Random.InitState(FindNativeD20Seed(10));
            _rulesWebCommand = BeginExpandedSummoningDetachedAbility(_rulesSpider, _rulesWeb,
                new TargetWrapper(hostile));
            _rulesSteps.Add("web:spiderSize=" + spiderSize + ";oneLargerAllowed=" + oneLargerAllowed +
                ";twoLargerRefused=" + twoLargerRefused + ";highTouchAc=" + _rulesHighTouch +
                ";usesBefore=" + _rulesWebUses0);
            if (!oneLargerAllowed || !twoLargerRefused)
                _rulesCases.Add(Assertion("expanded-summoning-correction-web-size",
                    "a foe one size larger than the spider can be webbed; two sizes larger cannot",
                    "oneLargerAllowed=" + oneLargerAllowed + ";twoLargerRefused=" + twoLargerRefused, false,
                    "AbilityData.CanTarget with the hostile's size set exactly"));
        }

        private void ContinueExpandedSummoningWeb()
        {
            ExpandedSummoningCorrectionFixture fixture = _rulesFixture;
            UnitEntityData hostile = fixture.Hostile;
            _rulesMissedHighTouch = !hostile.Descriptor.HasFact(_rulesWebbed);
            _rulesWebUses1 = _rulesSpider.Descriptor.Resources.GetResourceAmount(_rulesWebResource);
            _rulesWebRollA = string.Join("|", _rulesObserver.Rolls.ToArray());
            _rulesObserver.Rolls.Clear();
            // Low touch AC, superb Reflex: the touch attack hits and there is no save.
            hostile.Descriptor.Stats.AC.BaseValue = -100;
            SetExactProperty(hostile.Descriptor.Stats.GetStat(StatType.SaveReflex), "BaseValue", 100);
            var lowTouchAc = new RuleCalculateAC(_rulesSpider, hostile, AttackType.RangedTouch);
            Rulebook.Trigger(lowTouchAc);
            _rulesLowTouch = lowTouchAc.TargetAC;
            UnityEngine.Random.InitState(FindNativeD20Seed(10));
            _rulesWebCommand = BeginExpandedSummoningDetachedAbility(_rulesSpider, _rulesWeb,
                new TargetWrapper(hostile));
        }

        private void FinishExpandedSummoningWeb()
        {
            ExpandedSummoningCorrectionFixture fixture = _rulesFixture;
            UnitEntityData hostile = fixture.Hostile;
            bool webbedLowTouch = hostile.Descriptor.HasFact(_rulesWebbed) &&
                hostile.Descriptor.State.HasCondition(UnitCondition.Entangled) &&
                hostile.Descriptor.State.HasCondition(UnitCondition.CantMove);
            _rulesWebUses2 = _rulesSpider.Descriptor.Resources.GetResourceAmount(_rulesWebResource);
            string rollB = string.Join("|", _rulesObserver.Rolls.ToArray());
            EventBus.Unsubscribe(_rulesObserver);
            _rulesObserver = null;
            bool thirdUse = new AbilityData(_rulesSpider.Descriptor.Abilities.GetAbility(_rulesWeb)).IsAvailable;
            // Escape: the state's own per-round check against the
            // Constitution-based DC - hopeless, then overwhelming.
            SetExpandedSummoningHostileEscape(-100);
            Buff webState = hostile.Descriptor.Buffs.GetBuff(_rulesWebbed);
            UnityEngine.Random.InitState(FindNativeD20Seed(10));
            if (webState != null) webState.TickMechanics();
            bool stillWebbed = hostile.Descriptor.HasFact(_rulesWebbed);
            SetExpandedSummoningHostileEscape(200);
            webState = hostile.Descriptor.Buffs.GetBuff(_rulesWebbed);
            UnityEngine.Random.InitState(FindNativeD20Seed(10));
            if (webState != null) webState.TickMechanics();
            bool brokeFree = !hostile.Descriptor.HasFact(_rulesWebbed) &&
                !hostile.Descriptor.State.HasCondition(UnitCondition.CantMove);
            SetExpandedSummoningHostileEscape(0);
            bool immune = _rulesSpider.Descriptor.HasFact(fixture.Blueprints.OfType<BlueprintUnitFact>()
                .Single(value => value.AssetGuid == "3051e7002c803fc47a11bcfa381b9fbd"));
            Buff selfWeb = _rulesSpider.Descriptor.AddBuff(_rulesWebbed, _rulesSpider, TimeSpan.FromSeconds(60f));
            string immunityDirect = selfWeb == null ? "refused" : "applied";
            if (selfWeb != null) _rulesSpider.Descriptor.Buffs.RemoveFact(selfWeb);
            hostile.Descriptor.Stats.AC.BaseValue = _rulesHostileAc;
            SetExactProperty(hostile.Descriptor.Stats.GetStat(StatType.SaveReflex), "BaseValue", 0);
            if (hostile.Descriptor.HasFact(_rulesWebbed))
                hostile.Descriptor.Buffs.RemoveFact(hostile.Descriptor.Buffs.GetBuff(_rulesWebbed));
            string detail = string.Join(";", _rulesSteps.Where(value => value.StartsWith("web", StringComparison.Ordinal)).ToArray()) +
                ";rollA=" + Sanitize(_rulesWebRollA) + ";missedHighTouch=" + _rulesMissedHighTouch +
                ";lowTouchAc=" + _rulesLowTouch + ";rollB=" + Sanitize(rollB) + ";webbedLowTouch=" + webbedLowTouch +
                ";uses=" + _rulesWebUses0 + "->" + _rulesWebUses1 + "->" + _rulesWebUses2 +
                ";thirdUseAvailable=" + thirdUse + ";stillWebbedHopeless=" + stillWebbed +
                ";brokeFreeOverwhelming=" + brokeFree + ";immunityFact=" + immune +
                ";directWebOnSpider=" + immunityDirect;
            bool ok = _rulesHighTouch >= 100 && _rulesMissedHighTouch && _rulesWebRollA.Contains("type=RangedTouch") &&
                _rulesWebRollA.Contains("hit=False") && _rulesLowTouch <= 0 && rollB.Contains("hit=True") &&
                webbedLowTouch && _rulesWebUses0 == 2 && _rulesWebUses1 == 1 && _rulesWebUses2 == 0 && !thirdUse &&
                stillWebbed && brokeFree && immune && immunityDirect == "refused";
            _rulesCases.Add(Assertion("expanded-summoning-correction-web",
                "the web is a ranged touch attack: it misses a high-touch-AC foe whatever its Reflex save and webs a low-touch-AC foe whatever its Reflex save; a target more than one size larger is refused; the webbed foe escapes only on the Constitution-based check; two uses per summoning; the spider carries the native web immunity and the web-grappled state is refused on it",
                detail, ok, "live web casts whose projectile flies on world time and makes its own attack roll (recorded by a global rulebook observer) against the hostile's exact touch AC and Reflex save; the web-grappled state's own per-round break-free"));
            DisposeExpandedSummoningUnits(fixture.Created, new[] { _rulesSpider });
            _rulesSpider = null;
            if (_rulesWolf != null && fixture.Created.Contains(_rulesWolf))
                DisposeExpandedSummoningUnits(fixture.Created, new[] { _rulesWolf });
            _rulesWolf = null;
        }

        // --- the synchronous mephit roles ------------------------------------------------------------------------------

        private static bool ExerciseExpandedSummoningCorrectionMephitRoles(
            ExpandedSummoningCorrectionFixture fixture, out string detail)
        {
            var steps = new List<string>();
            bool ok = true;
            UnitEntityData hostile = fixture.Hostile;
            UnitEntityData caster = fixture.Caster;
            BlueprintScriptableObject[] blueprints = fixture.Blueprints;
            int damageBefore = hostile.Descriptor.Damage;
            int casterDamageBefore = caster.Descriptor.Damage;
            ItemEntityArmor metalArmor = null;
            UnitEntityData wolf = null;
            try
            {
                wolf = CastExpandedSummoningVariant(blueprints, caster, ExpandedSummoningVariant(
                    SummonFamily.NaturesAlly, "wolf", 2, SummonMultiplicity.One), null,
                    fixture.Evidence).Single();
                fixture.Created.Add(wolf);
                RemoveExpandedSummoningAppearanceBuffs(wolf);
                wolf.Descriptor.Stats.HitPoints.BaseValue = 100000;
                SetExpandedSummoningBrainActive(wolf, false);
                int wolfDamageBefore = wolf.Descriptor.Damage;
                SetExactProperty(hostile.Descriptor.Stats.GetStat(StatType.SaveWill), "BaseValue", -100);
                SetExactProperty(hostile.Descriptor.Stats.GetStat(StatType.SaveFortitude), "BaseValue", -100);
                SetExactProperty(hostile.Descriptor.Stats.GetStat(StatType.SaveReflex), "BaseValue", -100);

                // --- Chill Metal (ice mephit) ------------------------------------------------------------------
                UnitEntityData ice = CastExpandedSummoningQuietUnit(fixture, "ice-mephit");
                BlueprintAbility chillMetal = blueprints.OfType<BlueprintAbility>().Single(value =>
                    value.name == "KMG_Summoning_Special_IceMephit_SpellLikeTwo");
                BlueprintBuff chillState = blueprints.OfType<BlueprintBuff>().Single(value =>
                    value.name == "KMG_Summoning_Special_IceMephit_ChillMetalState");
                hostile.Translocate(ice.Position + new Vector3(2f, 0f, 0f), null);
                BlueprintItemArmor armorBlueprint = blueprints.OfType<BlueprintItemArmor>().Where(
                    value => value.Type != null && value.Type.IsArmor &&
                        value.Type.ProficiencyGroup == ArmorProficiencyGroup.Medium &&
                        SummonChillMetal.IsMetalArmorType(value.Type.name))
                    .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
                bool noMetalBefore = SummonChillMetal.Tier(wolf) == 0 &&
                    !new AbilityData(ice.Descriptor.Abilities.GetAbility(chillMetal))
                        .CanTarget(new TargetWrapper(wolf));
                ItemEntity hostileWeapon = hostile.Body.PrimaryHand.MaybeItem;
                if (hostileWeapon != null) hostile.Body.PrimaryHand.RemoveItem(false);
                if (armorBlueprint != null && !hostile.Body.Armor.HasArmor)
                {
                    metalArmor = new ItemEntityArmor(armorBlueprint);
                    hostile.Body.Armor.InsertItem(metalArmor);
                }
                string armored = SummonChillMetal.Describe(hostile);
                bool targetable = new AbilityData(ice.Descriptor.Abilities.GetAbility(chillMetal))
                    .CanTarget(new TargetWrapper(hostile));
                SummonChillMetalComponent.ClearOutcomes();
                ExecuteExpandedSummoningRuntimeAbility(ice, chillMetal, 2, new TargetWrapper(hostile),
                    false, hostile);
                bool chilled = hostile.Descriptor.HasFact(chillState);
                var ticks = new List<string>();
                Buff chill = hostile.Descriptor.Buffs.GetBuff(chillState);
                for (int round = 0; round < 6 && chill != null; round++)
                {
                    int before = hostile.Descriptor.Damage;
                    chill.TickMechanics();
                    ticks.Add((hostile.Descriptor.Damage - before).ToString());
                }
                string chillOutcomes = string.Join("|", SummonChillMetalComponent.ObservedOutcomes
                    .Select(value => value.Substring(value.IndexOf(':') + 1)).ToArray());
                bool fullTable = chillOutcomes.Contains("round=2;tier=2;dice=1d4") &&
                    chillOutcomes.Contains("round=3;tier=2;dice=2d4") &&
                    chillOutcomes.Contains("round=4;tier=2;dice=2d4") &&
                    chillOutcomes.Contains("round=5;tier=2;dice=2d4") &&
                    chillOutcomes.Contains("round=6;tier=2;dice=1d4") &&
                    chillOutcomes.Contains("round=7;tier=2;dice=0d4") &&
                    ticks.Count == 6 && ticks.Take(5).All(value => int.Parse(value) > 0) &&
                    ticks[5] == "0";
                if (chill != null && chill.Active) hostile.Descriptor.Buffs.RemoveFact(chill);
                // Only a metal weapon: the table's minimal 1 or 2 points.
                if (metalArmor != null) hostile.Body.Armor.RemoveItem(false);
                BlueprintItemWeapon swordBlueprint = ExpandedSummoningWeaponOfCategory(blueprints,
                    WeaponCategory.Longsword);
                var sword = new ItemEntityWeapon(swordBlueprint);
                hostile.Body.PrimaryHand.InsertItem(sword);
                string armed = SummonChillMetal.Describe(hostile);
                SummonChillMetalComponent.ClearOutcomes();
                Buff minimalState = hostile.Descriptor.AddBuff(chillState, ice, TimeSpan.FromSeconds(60f));
                var minimalTicks = new List<string>();
                for (int round = 0; round < 6 && minimalState != null; round++)
                {
                    int before = hostile.Descriptor.Damage;
                    minimalState.TickMechanics();
                    minimalTicks.Add((hostile.Descriptor.Damage - before).ToString());
                }
                bool minimalTable = string.Join(",", minimalTicks.ToArray()) == "1,2,2,2,1,0" &&
                    SummonChillMetalComponent.ObservedOutcomes.All(value => value.Contains("tier=1"));
                if (minimalState != null && minimalState.Active) hostile.Descriptor.Buffs.RemoveFact(minimalState);
                hostile.Body.PrimaryHand.RemoveItem(false);
                sword.Dispose();
                if (hostileWeapon != null) hostile.Body.PrimaryHand.InsertItem(hostileWeapon);
                bool noMetalAfter = SummonChillMetal.Tier(hostile) == 0;
                steps.Add("chillMetal:noMetalUntargetable=" + noMetalBefore + ";armored[" + armored +
                    "];targetable=" + targetable + ";chilled=" + chilled + ";ticks=" +
                    string.Join(",", ticks.ToArray()) + ";outcomes=" + Sanitize(chillOutcomes) +
                    ";fullTable=" + fullTable + ";armed[" + armed + "];minimalTicks=" +
                    string.Join(",", minimalTicks.ToArray()) + ";minimalTable=" + minimalTable +
                    ";noMetalAfter=" + noMetalAfter + ";execution=" +
                    _expandedSummoningLastAbilityExecution);
                ok = ok && noMetalBefore && armorBlueprint != null && targetable && chilled &&
                    fullTable && minimalTable && noMetalAfter;
                hostile.Descriptor.Damage = damageBefore;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { ice });

                // --- Pyrotechnics and Magma Form (magma mephit) ----------------------------------------------------
                UnitEntityData magma = CastExpandedSummoningQuietUnit(fixture, "magma-mephit");
                BlueprintAbility pyrotechnics = blueprints.OfType<BlueprintAbility>().Single(value =>
                    value.name == "KMG_Summoning_Special_MagmaMephit_SpellLikeOne");
                BlueprintAbility magmaForm = blueprints.OfType<BlueprintAbility>().Single(value =>
                    value.name == "KMG_Summoning_Special_MagmaMephit_SpellLikeTwo");
                BlueprintBuff blinded = blueprints.OfType<BlueprintBuff>().Single(value =>
                    value.name == "KMG_Summoning_Special_MagmaMephit_PyrotechnicsBlindedState");
                BlueprintBuff lava = blueprints.OfType<BlueprintBuff>().Single(value =>
                    value.name == "KMG_Summoning_Special_MagmaMephit_MagmaFormState");
                BlueprintAbility magmaBreath = blueprints.OfType<BlueprintAbility>().Single(value =>
                    value.name == "KMG_Summoning_Special_MagmaMephit_Breath");
                hostile.Translocate(magma.Position + new Vector3(2f, 0f, 0f), null);
                caster.Translocate(magma.Position + new Vector3(-2f, 0f, 0f), null);
                wolf.Translocate(magma.Position + new Vector3(0f, 0f, 2f), null);
                ExecuteExpandedSummoningRuntimeAbility(magma, pyrotechnics, 2, new TargetWrapper(magma), false);
                bool hostileBlinded = hostile.Descriptor.HasFact(blinded) &&
                    hostile.Descriptor.State.HasCondition(UnitCondition.Blindness);
                bool alliesSighted = !caster.Descriptor.HasFact(blinded) && !wolf.Descriptor.HasFact(blinded) &&
                    !magma.Descriptor.HasFact(blinded);
                Buff blindState = hostile.Descriptor.Buffs.GetBuff(blinded);
                double blindSeconds = blindState == null ? -1 : blindState.TimeLeft.TotalSeconds;
                bool blindDuration = blindSeconds >= 2 * 6 - 0.5 && blindSeconds <= 5 * 6 + 0.5;
                if (blindState != null) hostile.Descriptor.Buffs.RemoveFact(blindState);
                int speedBefore = magma.Descriptor.Stats.Speed.ModifiedValue;
                ExecuteExpandedSummoningRuntimeAbility(magma, magmaForm, 1, new TargetWrapper(magma), false);
                bool pooled = magma.Descriptor.HasFact(lava);
                int speedInForm = magma.Descriptor.Stats.Speed.ModifiedValue;
                bool cannotAttack = magma.Descriptor.State.HasCondition(UnitCondition.CanNotAttack);
                var lavaAttack = new UnitAttack(hostile);
                lavaAttack.Init(magma);
                bool attackInterrupted = lavaAttack.ShouldBeInterrupted;
                bool abilitiesWork = new AbilityData(magma.Descriptor.Abilities.GetAbility(magmaBreath)).IsAvailable;
                Func<int> slash = () =>
                {
                    int before = magma.Descriptor.Damage;
                    var damage = new PhysicalDamage(new DiceFormula(0, DiceType.Zero),
                        PhysicalDamageForm.Slashing);
                    damage.AddBonus(18);
                    var rule = new RuleDealDamage(hostile, magma, damage);
                    Rulebook.Trigger(rule);
                    int dealt = magma.Descriptor.Damage - before;
                    magma.Descriptor.Damage = before;
                    return dealt;
                };
                int inForm = slash();
                Buff lavaState = magma.Descriptor.Buffs.GetBuff(lava);
                if (lavaState != null) magma.Descriptor.Buffs.RemoveFact(lavaState);
                int outOfForm = slash();
                int speedAfter = magma.Descriptor.Stats.Speed.ModifiedValue;
                bool attacksAgain = !magma.Descriptor.State.HasCondition(UnitCondition.CanNotAttack);
                steps.Add("magma:blinded=" + hostileBlinded + ";alliesSighted=" + alliesSighted +
                    ";blindSeconds=" + blindSeconds.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) +
                    ";pooled=" + pooled + ";speed=" + speedBefore + "->" + speedInForm + "->" + speedAfter +
                    ";cannotAttack=" + cannotAttack + ";attackInterrupted=" + attackInterrupted +
                    ";abilitiesWork=" + abilitiesWork + ";slash18InForm=" + inForm + ";slash18OutOfForm=" +
                    outOfForm + "(DR 5/magic and the game's difficulty scaling);attacksAgain=" + attacksAgain +
                    ";execution=" + _expandedSummoningLastAbilityExecution);
                ok = ok && hostileBlinded && alliesSighted && blindDuration && pooled &&
                    speedInForm == ExpandedSummoningSpecialProfiles.MagmaFormSpeedFeet &&
                    speedAfter == speedBefore && cannotAttack && attackInterrupted && abilitiesWork &&
                    inForm == 0 && outOfForm > 0 && outOfForm <= 18 - 5 && attacksAgain;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { magma });

                // --- Glitterdust (salt mephit): enemies only ---------------------------------------------------
                UnitEntityData salt = CastExpandedSummoningQuietUnit(fixture, "salt-mephit");
                BlueprintAbility glitterdust = blueprints.OfType<BlueprintAbility>().Single(value =>
                    value.name == "KMG_Summoning_Special_SaltMephit_SpellLikeOne");
                Vector3 dustCentre = salt.Position + new Vector3(3f, 0f, 0f);
                hostile.Translocate(dustCentre, null);
                caster.Translocate(dustCentre + new Vector3(1f, 0f, 0f), null);
                wolf.Translocate(dustCentre + new Vector3(-1f, 0f, 0f), null);
                bool blindBefore = hostile.Descriptor.State.HasCondition(UnitCondition.Blindness);
                ExecuteExpandedSummoningRuntimeAbility(salt, glitterdust, 2, new TargetWrapper(dustCentre), false);
                bool hostileGlittered = hostile.Descriptor.State.HasCondition(UnitCondition.Blindness);
                bool casterSighted = !caster.Descriptor.State.HasCondition(UnitCondition.Blindness);
                bool wolfSighted = !wolf.Descriptor.State.HasCondition(UnitCondition.Blindness);
                foreach (UnitEntityData unit in new[] { hostile, caster, wolf })
                    foreach (Buff buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                            value.Blueprint != null && value.Blueprint.name.IndexOf("Glitterdust",
                                StringComparison.Ordinal) >= 0).ToArray())
                        unit.Descriptor.Buffs.RemoveFact(buff);
                steps.Add("glitterdust:blindBefore=" + blindBefore + ";hostileBlinded=" + hostileGlittered +
                    ";partyCasterSighted=" + casterSighted + ";alliedSummonSighted=" + wolfSighted +
                    ";execution=" + _expandedSummoningLastAbilityExecution);
                ok = ok && !blindBefore && hostileGlittered && casterSighted && wolfSighted;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { salt });
                bool wolfUntouched = wolf.Descriptor.Damage == wolfDamageBefore;
                steps.Add("alliedSummonDamage=" + wolfDamageBefore + "->" + wolf.Descriptor.Damage);
                ok = ok && wolfUntouched;
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + exception.GetType().Name + ":" + Sanitize(exception.Message));
                ok = false;
            }
            finally
            {
                if (metalArmor != null)
                {
                    if (hostile.Body.Armor.HasArmor && ReferenceEquals(hostile.Body.Armor.Armor, metalArmor))
                        hostile.Body.Armor.RemoveItem(false);
                    metalArmor.Dispose();
                }
                hostile.Descriptor.Damage = damageBefore;
                caster.Descriptor.Damage = casterDamageBefore;
                if (wolf != null && fixture.Created.Contains(wolf))
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { wolf });
            }
            detail = string.Join(";", steps.ToArray());
            return ok;
        }

        private static bool ExerciseExpandedSummoningCorrectionCyclops(
            ExpandedSummoningCorrectionFixture fixture, out string detail)
        {
            var steps = new List<string>();
            bool ok = true;
            UnitEntityData hostile = fixture.Hostile;
            BlueprintScriptableObject[] blueprints = fixture.Blueprints;
            int damageBefore = hostile.Descriptor.Damage;
            try
            {
                UnitEntityData cyclops = CastExpandedSummoningOwnTier(fixture, "cyclops");
                var ac = new RuleCalculateAC(hostile, cyclops, AttackType.Melee);
                Rulebook.Trigger(ac);
                string modifiers = string.Join(",", cyclops.Stats.AC.Modifiers
                    .Select(value => value.ModDescriptor + ":" + value.ModValue).ToArray());
                // The game's own difficulty modifier sits beside the stat
                // block's; the tabletop breakdown is everything but it.
                int difficulty = cyclops.Stats.AC.Modifiers.Where(value =>
                    value.ModDescriptor == ModifierDescriptor.Difficulty).Sum(value => value.ModValue);
                int tabletop = 10 + cyclops.Stats.AC.Modifiers.Where(value =>
                    value.ModDescriptor != ModifierDescriptor.Difficulty).Sum(value => value.ModValue);
                bool armorFact = cyclops.Stats.AC.Modifiers.Any(value =>
                    value.ModDescriptor == ModifierDescriptor.Armor && value.ModValue ==
                        ExpandedSummoningSpecialProfiles.CyclopsHideArmorBonus);
                bool naturalArmor = cyclops.Stats.AC.Modifiers.Any(value =>
                    value.ModDescriptor == ModifierDescriptor.NaturalArmor && value.ModValue == 7);
                bool noArmorItem = !cyclops.Body.Armor.HasArmor;
                bool acExact = tabletop == 19 && ac.TargetAC == 19 + difficulty;
                steps.Add("ac:target=" + ac.TargetAC + ";tabletop=" + tabletop + ";difficulty=" + difficulty +
                    ";modifiers=" + Sanitize(modifiers) + ";armorFact=" + armorFact + ";natural7=" + naturalArmor +
                    ";noArmorItem=" + noArmorItem + ";dex=" + cyclops.Descriptor.Stats.Dexterity.ModifiedValue +
                    ";size=" + cyclops.Descriptor.State.Size);
                ok = ok && acExact && armorFact && naturalArmor && noArmorItem;

                BlueprintAbility flash = blueprints.OfType<BlueprintAbility>().Single(value =>
                    value.name == "KMG_Summoning_Special_Cyclops_FlashOfInsight");
                BlueprintAbilityResource flashResource = blueprints.OfType<BlueprintAbilityResource>()
                    .Single(value => value.name == "KMG_Summoning_Special_Cyclops_FlashOfInsightResource");
                BlueprintBuff flashState = blueprints.OfType<BlueprintBuff>().Single(value =>
                    value.name == "KMG_Summoning_Special_Cyclops_FlashOfInsightState");
                int usesBefore = cyclops.Descriptor.Resources.GetResourceAmount(flashResource);
                ExecuteExpandedSummoningRuntimeAbility(cyclops, flash, 1, new TargetWrapper(cyclops), false);
                int usesAfter = cyclops.Descriptor.Resources.GetResourceAmount(flashResource);
                bool armed = cyclops.Descriptor.HasFact(flashState);
                // A save rolled while armed is untouched and leaves the arming.
                UnityEngine.Random.InitState(FindNativeD20Seed(1));
                var save = new RuleSavingThrow(cyclops, SavingThrowType.Will, 100);
                Rulebook.Trigger(save);
                int saveRoll = save.RollResult;
                bool stillArmed = cyclops.Descriptor.HasFact(flashState) &&
                    !CyclopsFlashOfInsightComponent.IsArmed(cyclops);
                cyclops.Descriptor.Stats.BaseAttackBonus.BaseValue = 0;
                ItemEntityWeapon greataxe = cyclops.Body.PrimaryHand.MaybeWeapon;
                UnityEngine.Random.InitState(FindNativeD20Seed(1));
                var armedAttack = new RuleAttackWithWeapon(cyclops, hostile, greataxe, 0);
                Rulebook.Trigger(armedAttack);
                RuleAttackRoll roll = armedAttack.AttackRoll;
                int natural = roll == null ? -1 : (int)roll.Roll;
                int confirmation = roll == null ? -1 : (int)roll.CriticalConfirmationRoll;
                bool chosenTwenty = roll != null && natural == 20 && roll.IsHit && roll.IsCriticalRoll &&
                    !roll.AutoHit && !roll.AutoCriticalThreat && !roll.AutoCriticalConfirmation;
                bool spent = !cyclops.Descriptor.HasFact(flashState) &&
                    !CyclopsFlashOfInsightComponent.IsArmed(cyclops);
                hostile.Descriptor.Damage = damageBefore;
                UnityEngine.Random.InitState(FindNativeD20Seed(1));
                var plainAttack = new RuleAttackWithWeapon(cyclops, hostile, greataxe, 0);
                Rulebook.Trigger(plainAttack);
                int plainNatural = plainAttack.AttackRoll == null ? -1 : (int)plainAttack.AttackRoll.Roll;
                bool plainMiss = plainAttack.AttackRoll != null && !plainAttack.AttackRoll.IsHit;
                bool secondUse = new AbilityData(cyclops.Descriptor.Abilities.GetAbility(flash)).IsAvailable;
                hostile.Descriptor.Damage = damageBefore;
                steps.Add("flash:uses=" + usesBefore + "->" + usesAfter + ";armed=" + armed +
                    ";saveWhileArmed=" + saveRoll + ";stillArmedAfterSave=" + stillArmed +
                    ";armedNatural=" + natural + ";hit=" + (roll != null && roll.IsHit) + ";threat=" +
                    (roll != null && roll.IsCriticalRoll) + ";confirmationRoll=" + confirmation +
                    ";confirmed=" + (roll != null && roll.IsCriticalConfirmed) + ";autoFlags=" +
                    (roll != null && (roll.AutoHit || roll.AutoCriticalThreat || roll.AutoCriticalConfirmation)) +
                    ";spent=" + spent + ";nextNatural=" + plainNatural + ";nextMiss=" + plainMiss +
                    ";secondUseAvailable=" + secondUse);
                ok = ok && usesBefore == 1 && usesAfter == 0 && armed && saveRoll == 1 && stillArmed &&
                    chosenTwenty && confirmation >= 1 && confirmation <= 20 && spent &&
                    plainNatural == 1 && plainMiss && !secondUse;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { cyclops });
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + exception.GetType().Name + ":" + Sanitize(exception.Message));
                ok = false;
            }
            finally
            {
                hostile.Descriptor.Damage = damageBefore;
            }
            detail = string.Join(";", steps.ToArray());
            return ok;
        }

        private static bool ExerciseExpandedSummoningCorrectionHooves(
            ExpandedSummoningCorrectionFixture fixture, out string detail)
        {
            var steps = new List<string>();
            bool ok = true;
            UnitEntityData hostile = fixture.Hostile;
            try
            {
                foreach (string key in new[] { "pony", "horse" })
                {
                    UnitEntityData animal = CastExpandedSummoningOwnTier(fixture, key);
                    List<ItemEntityWeapon> hooves = SummonDocileHoovesComponent.Hooves(animal);
                    int strength = animal.Descriptor.Stats.Strength.Bonus;
                    var rows = new List<string>();
                    bool animalOk = hooves.Count == 2;
                    bool flaggedAtSpawn = hooves.Count == 2 && hooves.All(value => value.ForceSecondary);
                    foreach (ItemEntityWeapon hoof in hooves)
                    {
                        int additionalIndex;
                        SummonLimbKind kind = SummonLimbs.Classify(animal, hoof, out additionalIndex);
                        SummonDocileHoovesComponent.SuspendedForFixture = false;
                        var secondaryBonus = new RuleCalculateAttackBonus(animal, hostile, hoof, 0);
                        Rulebook.Trigger(secondaryBonus);
                        var secondaryStats = new RuleCalculateWeaponStats(animal, hoof, null);
                        Rulebook.Trigger(secondaryStats);
                        int secondaryDamage = secondaryStats.DamageDescription.Count == 0 ? -999 :
                            secondaryStats.DamageDescription[0].Bonus;
                        bool secondaryFlag = hoof.ForceSecondary && hoof.IsSecondary;
                        // The same hoof as a primary attack, with the docile
                        // carrier's rule hooks suspended for the comparison.
                        SummonDocileHoovesComponent.SuspendedForFixture = true;
                        hoof.ForceSecondary = false;
                        var primaryBonus = new RuleCalculateAttackBonus(animal, hostile, hoof, 0);
                        Rulebook.Trigger(primaryBonus);
                        var primaryStats = new RuleCalculateWeaponStats(animal, hoof, null);
                        Rulebook.Trigger(primaryStats);
                        int primaryDamage = primaryStats.DamageDescription.Count == 0 ? -999 :
                            primaryStats.DamageDescription[0].Bonus;
                        hoof.ForceSecondary = true;
                        SummonDocileHoovesComponent.SuspendedForFixture = false;
                        bool hoofOk = secondaryFlag && kind != SummonLimbKind.None &&
                            secondaryBonus.Result == primaryBonus.Result - 5 &&
                            primaryDamage == strength && secondaryDamage == strength / 2;
                        rows.Add(kind + (additionalIndex >= 0 ? "[" + additionalIndex + "]" : "") +
                            ":secondary=" + secondaryFlag + ",attack=" + secondaryBonus.Result + "(primary " +
                            primaryBonus.Result + "),damageBonus=" + secondaryDamage + "(primary " +
                            primaryDamage + ")");
                        animalOk = animalOk && hoofOk;
                    }
                    int rakeSlots, attacks;
                    ExpandedSummoningPlanFullAttack(animal, hostile, false, 0, out rakeSlots, out attacks);
                    steps.Add(key + ":strengthBonus=" + strength + ";hooves=" + hooves.Count +
                        ";flaggedAtSpawn=" + flaggedAtSpawn + ";" + string.Join(";", rows.ToArray()) +
                        ";fullAttack=" + attacks);
                    ok = ok && animalOk && flaggedAtSpawn && attacks == 2;
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { animal });
                }
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + exception.GetType().Name + ":" + Sanitize(exception.Message));
                ok = false;
            }
            finally
            {
                SummonDocileHoovesComponent.SuspendedForFixture = false;
            }
            detail = string.Join(";", steps.ToArray());
            return ok;
        }

        // ---------------------------------------------------------------------------------------------------------
        // The Cyclops Flash of Insight across the persistence trio's save and reload.
        // ---------------------------------------------------------------------------------------------------------

        private string _expandedSummoningPersistenceFlashDetail = "<not evaluated>";
        private bool _expandedSummoningPersistenceFlashValid;

        private static UnitEntityData ExpandedSummoningPersistentCyclops(UnitEntityData[] units)
        {
            return units == null ? null : units.FirstOrDefault(value => value != null &&
                value.Blueprint != null && value.Blueprint.name == "KMG_Summoning_Unit_Cyclops");
        }

        /// <summary>Prepare: the Cyclops spends its one Flash of Insight (the arming state stays) before the save.</summary>
        private static void ArmExpandedSummoningPersistenceFlash(UnitEntityData[] units)
        {
            UnitEntityData cyclops = ExpandedSummoningPersistentCyclops(units);
            if (cyclops == null) return;
            BlueprintAbility flash = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintAbility>().Single(value =>
                    value.name == "KMG_Summoning_Special_Cyclops_FlashOfInsight");
            ExecuteExpandedSummoningRuntimeAbility(cyclops, flash, 1, new TargetWrapper(cyclops), false);
        }

        /// <summary>
        /// Prepare: spent and armed. Verify-cleanup (after the reload): still
        /// spent and unavailable, armed exactly once; the next attack's own d20
        /// is the chosen 20 and the arming is gone; the attack after it rolls
        /// its own 1 and misses. Damage dealt to the target is put back.
        /// </summary>
        private static string DescribeExpandedSummoningPersistenceFlash(UnitEntityData[] units,
            bool prepare, bool verifyCleanup, UnitEntityData target, out bool valid)
        {
            valid = !prepare && !verifyCleanup;
            UnitEntityData cyclops = ExpandedSummoningPersistentCyclops(units);
            if (cyclops == null) return "cyclops=absent";
            BlueprintScriptableObject[] blueprints = BlueprintBootstrap.Library.GetAllBlueprints()
                .Where(value => value != null).ToArray();
            BlueprintAbility flash = blueprints.OfType<BlueprintAbility>().Single(value =>
                value.name == "KMG_Summoning_Special_Cyclops_FlashOfInsight");
            BlueprintAbilityResource resource = blueprints.OfType<BlueprintAbilityResource>().Single(
                value => value.name == "KMG_Summoning_Special_Cyclops_FlashOfInsightResource");
            BlueprintBuff state = blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_Cyclops_FlashOfInsightState");
            int uses = cyclops.Descriptor.Resources.GetResourceAmount(resource);
            Ability granted = cyclops.Descriptor.Abilities.GetAbility(flash);
            bool available = granted != null && new AbilityData(granted).IsAvailable;
            int armedStates = cyclops.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value =>
                ReferenceEquals(value.Blueprint, state));
            string detail = "uses=" + uses + ";available=" + available + ";armedStates=" + armedStates;
            if (prepare)
            {
                valid = uses == 0 && !available && armedStates == 1;
                return detail;
            }
            if (!verifyCleanup || target == null) return detail;
            ItemEntityWeapon weapon = cyclops.Body == null || cyclops.Body.PrimaryHand == null ? null :
                cyclops.Body.PrimaryHand.MaybeWeapon;
            if (weapon == null) return detail + ";weapon=absent";
            int damageBefore = target.Descriptor.Damage;
            cyclops.Descriptor.Stats.BaseAttackBonus.BaseValue = 0;
            UnityEngine.Random.InitState(FindNativeD20Seed(1));
            var armed = new RuleAttackWithWeapon(cyclops, target, weapon, 0);
            Rulebook.Trigger(armed);
            int natural = armed.AttackRoll == null ? -1 : (int)armed.AttackRoll.Roll;
            bool hit = armed.AttackRoll != null && armed.AttackRoll.IsHit;
            bool spent = !cyclops.Descriptor.HasFact(state) &&
                !CyclopsFlashOfInsightComponent.IsArmed(cyclops);
            target.Descriptor.Damage = damageBefore;
            UnityEngine.Random.InitState(FindNativeD20Seed(1));
            var plain = new RuleAttackWithWeapon(cyclops, target, weapon, 0);
            Rulebook.Trigger(plain);
            int nextNatural = plain.AttackRoll == null ? -1 : (int)plain.AttackRoll.Roll;
            bool nextMiss = plain.AttackRoll != null && !plain.AttackRoll.IsHit;
            target.Descriptor.Damage = damageBefore;
            detail += ";reloadedNatural=" + natural + ";hit=" + hit + ";spent=" + spent +
                ";nextNatural=" + nextNatural + ";nextMiss=" + nextMiss;
            valid = uses == 0 && !available && armedStates == 1 && natural == 20 && hit && spent &&
                nextNatural == 1 && nextMiss;
            return detail;
        }

        // ---------------------------------------------------------------------------------------------------------
        // The visual resource lifecycle scenario (several frames: the game
        // destroys a view at the end of the frame that disposed its unit).
        // ---------------------------------------------------------------------------------------------------------

        private const string NativeAirMephitDonorGuid = "50782bc4eb36aac4287023e20ee00808";
        private const int VisualLifecycleCycles = 3;
        private ExpandedSummoningCorrectionFixture _visualLifecycleFixture;
        private int _visualLifecyclePhase;
        private int _visualLifecycleCycle;
        private int _visualLifecycleWait;
        private int _visualLifecycleBaselineMaterials = -1;
        private int _visualLifecycleBaselineTextures = -1;
        private string _visualLifecycleDonorBefore;
        private readonly List<string> _visualLifecycleSteps = new List<string>();
        private readonly List<RuntimeTestAssertion> _visualLifecycleCases =
            new List<RuntimeTestAssertion>();
        private bool _visualLifecycleFailed;

        private static string DescribeExpandedSummoningViewMaterials(UnitEntityView view)
        {
            if (view == null) return "<no-view>";
            var parts = new List<string>();
            foreach (Renderer renderer in view.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null || renderer.sharedMaterials == null) continue;
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null) { parts.Add("<null>"); continue; }
                    string rim = material.HasProperty("_RimColor") ?
                        material.GetColor("_RimColor").ToString("0.##") : "-";
                    parts.Add(material.name.Replace(';', ',').Replace('|', '/') + "@" + rim);
                }
            }
            return string.Join("|", parts.ToArray());
        }

        private static bool ExpandedSummoningViewCarriesVariantMaterial(UnitEntityView view)
        {
            if (view == null) return false;
            foreach (Renderer renderer in view.GetComponentsInChildren<Renderer>(true))
                if (renderer != null && renderer.sharedMaterials != null &&
                    renderer.sharedMaterials.Any(material => material != null &&
                        material.name.StartsWith(ExpandedSummoningVisualVariantPatch.VariantMaterialName,
                            StringComparison.Ordinal)))
                    return true;
            return false;
        }

        private string SpawnExpandedSummoningDonorMephit(ExpandedSummoningCorrectionFixture fixture)
        {
            BlueprintUnit donor = fixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                value.AssetGuid == NativeAirMephitDonorGuid);
            UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(donor,
                fixture.Caster.Position + new Vector3(2f, 0f, 0f), Quaternion.identity, fixture.Scene);
            Game.Instance.EntityCreator.Tick();
            string materials = unit == null ? "<no-unit>" : DescribeExpandedSummoningViewMaterials(unit.View) +
                ";variantPatch=" + ExpandedSummoningVisualVariantPatch.DescribeView(unit == null ? null : unit.View);
            if (unit != null)
            {
                CleanupExpandedSummoningUnit(unit);
                Game.Instance.EntityDestroyer.Tick();
            }
            return materials;
        }

        private void PollExpandedSummoningVisualLifecycle()
        {
            try
            {
                if (_visualLifecycleFixture == null && _visualLifecyclePhase == 0)
                {
                    _visualLifecycleFixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_ExpandedSummoning_LifecycleCaster");
                    ExpandedSummoningVisualVariantPatch.ClearObservations();
                    ExpandedSummoningVisualVariantPatch.FaultAfterRenderers = 0;
                    int textures, live;
                    string counts = ExpandedSummoningVisualVariantPatch.CountOwnedObjects(
                        out _visualLifecycleBaselineMaterials, out textures, out live);
                    _visualLifecycleBaselineTextures = textures;
                    _visualLifecycleDonorBefore = SpawnExpandedSummoningDonorMephit(_visualLifecycleFixture);
                    _visualLifecycleSteps.Add("baseline:" + counts + ";donor[" +
                        Sanitize(_visualLifecycleDonorBefore) + "]");
                    _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-baseline",
                        "no variant-owned material or texture exists before the cycles",
                        counts + ";live=" + live, _visualLifecycleBaselineMaterials == 0 && textures == 0 && live == 0,
                        "Resources.FindObjectsOfTypeAll filtered on the variant prefix"));
                    _visualLifecyclePhase = 1;
                    _visualLifecycleCycle = 0;
                    return;
                }
                ExpandedSummoningCorrectionFixture fixture = _visualLifecycleFixture;
                if (_visualLifecyclePhase == 1)
                {
                    // Cast the set, check every attach, dispose, then wait for the views to go.
                    _visualLifecycleCycle++;
                    var outcomes = new List<string>();
                    var units = new List<UnitEntityData>();
                    foreach (SummonVariantSpec variant in new[] {
                        ExpandedSummoningOwnTierVariant("dust-mephit", SummonMultiplicity.One),
                        ExpandedSummoningOwnTierVariant("steam-mephit", SummonMultiplicity.OneD3),
                        ExpandedSummoningOwnTierVariant("tiger", SummonMultiplicity.One),
                        ExpandedSummoningOwnTierVariant("cheetah", SummonMultiplicity.One),
                        ExpandedSummoningOwnTierVariant("lion", SummonMultiplicity.One) })
                    {
                        UnitEntityData[] spawned;
                        try
                        {
                            spawned = CastExpandedSummoningVariant(fixture.Blueprints,
                                fixture.Caster, variant, null, fixture.Evidence);
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidOperationException("Cast of " + variant.StableKey + " failed: " +
                                exception.Message + " (captured=" + string.Join(",", ExpandedSummoningRuleCapture.ToArray()
                                    .Select(value => value.Blueprint == null ? "?" : value.Blueprint.name).ToArray()) +
                                "; execution=" + _expandedSummoningLastAbilityExecution + ")", exception);
                        }
                        fixture.Created.AddRange(spawned);
                        units.AddRange(spawned);
                        foreach (UnitEntityData unit in spawned)
                        {
                            SetExpandedSummoningBrainActive(unit, false);
                            outcomes.Add(variant.Creature.Key + "=" + Sanitize(
                                ExpandedSummoningVisualVariantPatch.DescribeView(unit.View)) + "{" +
                                ExpandedSummoningVisualVariantPatch.DescribeOwnership(unit.View) + "}");
                        }
                    }
                    int materials, textures, live;
                    string counts = ExpandedSummoningVisualVariantPatch.CountOwnedObjects(
                        out materials, out textures, out live);
                    bool allApplied = outcomes.Count >= 5 && outcomes.All(value =>
                        value.Contains("=variant:applied"));
                    bool owned = live == units.Count && materials >= units.Count;
                    _visualLifecycleSteps.Add("cycle" + _visualLifecycleCycle + ":cast=" + units.Count +
                        ";" + string.Join(",", outcomes.ToArray()) + ";" + counts);
                    if (!allApplied || !owned) _visualLifecycleFailed = true;
                    foreach (UnitEntityData unit in units.ToArray())
                    {
                        CleanupExpandedSummoningUnit(unit);
                        fixture.Created.Remove(unit);
                    }
                    Game.Instance.EntityDestroyer.Tick();
                    Game.Instance.EntityDestroyer.Tick();
                    _visualLifecycleWait = 0;
                    _visualLifecyclePhase = 2;
                    return;
                }
                if (_visualLifecyclePhase == 2)
                {
                    if (_visualLifecycleWait++ < 3) return;
                    int materials, textures, live;
                    string counts = ExpandedSummoningVisualVariantPatch.CountOwnedObjects(
                        out materials, out textures, out live);
                    int destroyed = ExpandedSummoningVisualVariantPatch.ObservedReleases.Count(value =>
                        value.Contains("=view-destroyed;"));
                    bool back = materials == _visualLifecycleBaselineMaterials &&
                        textures == _visualLifecycleBaselineTextures && live == 0;
                    _visualLifecycleSteps.Add("cycle" + _visualLifecycleCycle + ":afterDispose=" + counts +
                        ";viewDestroyedReleases=" + destroyed);
                    if (!back) _visualLifecycleFailed = true;
                    ExpandedSummoningVisualVariantPatch.ClearObservations();
                    _visualLifecyclePhase = _visualLifecycleCycle < VisualLifecycleCycles ? 1 : 3;
                    return;
                }
                if (_visualLifecyclePhase == 3)
                {
                    // A failed attach part way: rolled back in the same frame.
                    ExpandedSummoningVisualVariantPatch.FaultAfterRenderers = 1;
                    UnitEntityData faulted;
                    try
                    {
                        faulted = CastExpandedSummoningOwnTier(fixture, "dust-mephit");
                    }
                    finally
                    {
                        ExpandedSummoningVisualVariantPatch.FaultAfterRenderers = 0;
                    }
                    SetExpandedSummoningBrainActive(faulted, false);
                    string outcome = ExpandedSummoningVisualVariantPatch.DescribeView(faulted.View);
                    string releases = string.Join("|", ExpandedSummoningVisualVariantPatch.ObservedReleases.ToArray());
                    bool native = !ExpandedSummoningViewCarriesVariantMaterial(faulted.View);
                    int materials, textures, live;
                    string counts = ExpandedSummoningVisualVariantPatch.CountOwnedObjects(
                        out materials, out textures, out live);
                    bool rolledBack = outcome.StartsWith("variant:exception:", StringComparison.Ordinal) &&
                        releases.Contains("attach-failed:InvalidOperationException") && native &&
                        materials == _visualLifecycleBaselineMaterials &&
                        textures == _visualLifecycleBaselineTextures && live == 0;
                    _visualLifecycleSteps.Add("fault:outcome=" + Sanitize(outcome) + ";releases=" +
                        Sanitize(releases) + ";nativeMaterials=" + native + ";" + counts);
                    _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-failed-attach",
                        "an attach that throws after a renderer swap leaves the donor's materials on the view and no owned object behind",
                        "outcome=" + Sanitize(outcome) + ";" + counts + ";native=" + native,
                        rolledBack, "ExpandedSummoningVisualVariantPatch.FaultAfterRenderers on a live dust mephit cast"));
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { faulted });
                    ExpandedSummoningVisualVariantPatch.ClearObservations();
                    _visualLifecycleWait = 0;
                    _visualLifecyclePhase = 4;
                    return;
                }
                if (_visualLifecyclePhase == 4)
                {
                    if (_visualLifecycleWait++ < 3) return;
                    // The module-wide sweep on a live variant.
                    UnitEntityData lion = CastExpandedSummoningOwnTier(fixture, "lion");
                    SetExpandedSummoningBrainActive(lion, false);
                    string before = ExpandedSummoningVisualVariantPatch.DescribeView(lion.View);
                    bool variantOn = ExpandedSummoningViewCarriesVariantMaterial(lion.View);
                    string sweep = ExpandedSummoningVisualVariantPatch.ReleaseAll("fixture-module-shutdown");
                    bool variantOff = !ExpandedSummoningViewCarriesVariantMaterial(lion.View);
                    string ownership = ExpandedSummoningVisualVariantPatch.DescribeOwnership(lion.View);
                    int materials, textures, live;
                    string counts = ExpandedSummoningVisualVariantPatch.CountOwnedObjects(
                        out materials, out textures, out live);
                    bool swept = before.StartsWith("variant:applied", StringComparison.Ordinal) && variantOn &&
                        variantOff && ownership.Contains("released=True") &&
                        materials == _visualLifecycleBaselineMaterials &&
                        textures == _visualLifecycleBaselineTextures && live == 0;
                    _visualLifecycleSteps.Add("sweep:before=" + Sanitize(before) + ";variantOn=" + variantOn +
                        ";" + sweep + ";variantOff=" + variantOff + ";ownership=" + ownership + ";" + counts);
                    _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-sweep",
                        "the module-wide release puts a live variant's renderers back and destroys every owned object",
                        sweep + ";" + counts + ";variantOff=" + variantOff, swept,
                        "ExpandedSummoningVisualVariantPatch.ReleaseAll on a live lion"));
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { lion });
                    ExpandedSummoningVisualVariantPatch.ClearObservations();
                    _visualLifecycleWait = 0;
                    _visualLifecyclePhase = 5;
                    return;
                }
                if (_visualLifecyclePhase == 5)
                {
                    if (_visualLifecycleWait++ < 3) return;
                    // The donor and the Pteranodon after everything.
                    string donorAfter = SpawnExpandedSummoningDonorMephit(fixture);
                    UnitEntityData pteranodon = CastExpandedSummoningOwnTier(fixture, "pteranodon");
                    SetExpandedSummoningBrainActive(pteranodon, false);
                    string pteranodonView = ExpandedSummoningPteranodonViewPatch.DescribeView(pteranodon.View);
                    bool pteranodonClean = pteranodonView.StartsWith("visual:attached;", StringComparison.Ordinal) &&
                        !ExpandedSummoningViewCarriesVariantMaterial(pteranodon.View) &&
                        ExpandedSummoningVisualVariantPatch.DescribeView(pteranodon.View) == "<none>";
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { pteranodon });
                    bool donorSame = donorAfter == _visualLifecycleDonorBefore &&
                        !donorAfter.Contains(ExpandedSummoningVisualVariantPatch.VariantMaterialName) &&
                        donorAfter.Contains("variantPatch=<none>");
                    _visualLifecycleSteps.Add("donorAfter[" + Sanitize(donorAfter) + "];pteranodon=" +
                        Sanitize(pteranodonView));
                    _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-donor-and-pteranodon",
                        "the native air mephit keeps its own materials and rim colour after the cycles; the Pteranodon keeps its own visual and carries no variant material",
                        "donorSame=" + donorSame + ";pteranodon=" + Sanitize(pteranodonView),
                        donorSame && pteranodonClean,
                        "a native donor spawned before and after; ExpandedSummoningPteranodonViewPatch.DescribeView"));
                    _visualLifecycleWait = 0;
                    _visualLifecyclePhase = 6;
                    return;
                }
                if (_visualLifecyclePhase == 6)
                {
                    if (_visualLifecycleWait++ < 3) return;
                    int materials, textures, live;
                    string counts = ExpandedSummoningVisualVariantPatch.CountOwnedObjects(
                        out materials, out textures, out live);
                    bool final = materials == _visualLifecycleBaselineMaterials &&
                        textures == _visualLifecycleBaselineTextures && live == 0;
                    _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-cycles",
                        VisualLifecycleCycles + " cast-and-dispose cycles of the dust mephit, a 1d3 steam mephit cast, the tiger, the cheetah and the lion: every view attached with its variant, every owned material, texture and controller instance destroyed with the view, counts back at baseline after each cycle and at the end",
                        string.Join("||", _visualLifecycleSteps.ToArray()) + "||final=" + counts,
                        !_visualLifecycleFailed && final,
                        "ExpandedSummoningVisualVariantPatch.CountOwnedObjects, DescribeView, DescribeOwnership and ObservedReleases across frames"));
                    CompleteExpandedSummoningVisualLifecycle();
                }
            }
            catch (Exception exception)
            {
                _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-phase" + _visualLifecyclePhase,
                    "the phase completes", "exception=" + exception.GetType().Name + ":" +
                    Sanitize(exception.Message) + ";steps=" + string.Join("||", _visualLifecycleSteps.ToArray()),
                    false, "the visual lifecycle fixture"));
                CompleteExpandedSummoningVisualLifecycle();
            }
        }

        private void CompleteExpandedSummoningVisualLifecycle()
        {
            bool cleaned = false;
            ExpandedSummoningVisualVariantPatch.FaultAfterRenderers = 0;
            try { EndExpandedSummoningCorrectionFixture(_visualLifecycleFixture, out cleaned); }
            catch (Exception exception)
            {
                _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-cleanup-exception",
                    "cleanup completes", "exception=" + exception.GetType().Name + ":" +
                    Sanitize(exception.Message), false, "the visual lifecycle fixture"));
            }
            _visualLifecycleFixture = null;
            _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-cleanup",
                "exact party and global-unit snapshots restored", "cleaned=" + cleaned, cleaned,
                "per-unit disposal and final exact snapshots"));
            _visualLifecycleCases.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(_visualLifecycleCases.All(value =>
                value.Status == "PASS") ? "PASS" : "FAIL", _visualLifecycleCases, null);
            foreach (string step in _visualLifecycleSteps) result.Diagnostics.Add("lifecycle=" + step);
            Complete(result);
        }
    }
}
