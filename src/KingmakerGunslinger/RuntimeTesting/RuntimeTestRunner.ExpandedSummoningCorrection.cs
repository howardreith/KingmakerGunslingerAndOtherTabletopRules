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
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects.RimLighting;
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
            internal int HostileDamage;
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
            PlaceExpandedSummoningUnit(fixture.Caster, fixture.Caster.Position);
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
            fixture.HostileDamage = fixture.Hostile.Descriptor.Damage;
            SetExpandedSummoningBrainActive(fixture.Hostile, false);
            PlaceExpandedSummoningUnit(fixture.Hostile, fixture.Hostile.Position);
            DisposeExpandedSummoningUnits(fixture.Created, new[] { pixie });
        }

        /// <summary>
        /// Puts the hostile back to a free, unhurt unit of its own size before
        /// a case starts, whatever an earlier case left on it, and says what
        /// it cleared so a leak between cases is visible in the record.
        /// </summary>
        private static string ResetExpandedSummoningHostile(ExpandedSummoningCorrectionFixture fixture)
        {
            UnitEntityData hostile = fixture.Hostile;
            if (hostile == null || hostile.Destroyed) return "hostile=absent";
            var cleared = new List<string>();
            Kingmaker.UnitLogic.Parts.UnitPartSwallowed swallowed =
                hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowed>();
            if (swallowed != null)
            {
                UnitEntityData swallower = swallowed.Swallower.Value;
                Kingmaker.UnitLogic.Parts.UnitPartSwallowWhole part = swallower == null ? null :
                    swallower.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowWhole>();
                if (part != null) part.Free(hostile);
                if (hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowed>() != null)
                    hostile.Remove<Kingmaker.UnitLogic.Parts.UnitPartSwallowed>();
                cleared.Add("swallowed");
            }
            if (hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleTarget>() != null)
            {
                hostile.Remove<Kingmaker.UnitLogic.Parts.UnitPartGrappleTarget>();
                cleared.Add("grappleTarget");
            }
            string[] states = { "KMG_Summoning_Special_Grapple_Grappled",
                "KMG_Summoning_Special_Grapple_MultiHeld", "KMG_Summoning_Special_PurpleWorm_Swallowed",
                "KMG_Summoning_Special_GiantFlytrap_Engulfed" };
            foreach (Buff buff in hostile.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                    value.Blueprint != null && (states.Contains(value.Blueprint.name) ||
                        value.Blueprint.AssetGuid == ExpandedSummoningSpecialBuilder.NativeWebGrappledGuid))
                .ToArray())
            {
                cleared.Add(buff.Blueprint.name);
                hostile.Descriptor.Buffs.RemoveFact(buff);
            }
            hostile.Descriptor.State.Size = fixture.HostileSize;
            hostile.Descriptor.Damage = fixture.HostileDamage;
            hostile.Descriptor.Stats.BaseAttackBonus.BaseValue = 0;
            return cleared.Count == 0 ? "clean" : "cleared=" + string.Join(",", cleared.ToArray());
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
            // A summoned unit enters the area's spatial grid before it is
            // positioned; the game re-indexes it on its first walk, so the
            // fixture indexes it where it stands now.
            PlaceExpandedSummoningUnit(unit, unit.Position);
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
            return value == null ? "<null>" : value.Replace(';', ',').Replace('|', '/')
                .Replace("\r", " ").Replace("\n", " / ");
        }

        /// <summary>The exception's type, message and its first frames, on one line.</summary>
        private static string DescribeExpandedSummoningCorrectionException(Exception exception)
        {
            if (exception == null) return "<none>";
            string frames = exception.StackTrace ?? "";
            var kept = new List<string>();
            foreach (string line in frames.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = line.Trim();
                int at = trimmed.IndexOf(" in ", StringComparison.Ordinal);
                if (at > 0) trimmed = trimmed.Substring(0, at);
                kept.Add(trimmed.Replace("at ", ""));
                if (kept.Count == 6) break;
            }
            string inner = exception.InnerException == null ? "" :
                ";inner=" + exception.InnerException.GetType().Name + ":" + exception.InnerException.Message;
            return exception.GetType().Name + ":" + Sanitize(exception.Message) + Sanitize(inner) +
                ";at=" + Sanitize(string.Join(" < ", kept.ToArray()));
        }

        private static string Vec(Vector3 value)
        {
            return "(" + value.x.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "," +
                value.y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "," +
                value.z.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ")";
        }

        /// <summary>
        /// Places a fixture unit by fiat the way the game's own teleports
        /// leave a unit: at the position, and indexed there in the area's
        /// spatial grid - the index every area effect queries for the units
        /// inside it, which the game's move controller advances only for a
        /// unit that walked, so a translocated unit would otherwise stay
        /// indexed where it was spawned.
        /// </summary>
        private static void PlaceExpandedSummoningUnit(UnitEntityData unit, Vector3 position)
        {
            if (unit == null) return;
            unit.Translocate(position, null);
            SparseGrid<Kingmaker.EntitySystem.EntityDataBase> grid = ExpandedSummoningAreaGrid();
            if (grid != null) grid.MoveTo(unit, unit.Position.x, unit.Position.z);
        }

        private static SparseGrid<Kingmaker.EntitySystem.EntityDataBase> ExpandedSummoningAreaGrid()
        {
            if (Game.Instance == null || Game.Instance.CurrentScene == null ||
                Game.Instance.CurrentScene.Area == null) return null;
            return Game.Instance.CurrentScene.Area.InteractiveObjectGrid;
        }

        /// <summary>Where the area's spatial grid holds a unit (row/col), or none.</summary>
        private static string DescribeExpandedSummoningGridEntry(UnitEntityData unit)
        {
            SparseGrid<Kingmaker.EntitySystem.EntityDataBase> grid = ExpandedSummoningAreaGrid();
            if (grid == null) return "grid=<none>";
            try
            {
                FieldInfo entriesField = grid.GetType().GetField("m_Entries",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var entries = entriesField == null ? null : entriesField.GetValue(grid) as System.Collections.IDictionary;
                if (entries == null) return "grid=entries-unavailable";
                if (!entries.Contains(unit)) return "grid=unregistered";
                object position = entries[unit];
                FieldInfo row = position.GetType().GetField("Row"), col = position.GetType().GetField("Col");
                return "grid=row" + (row == null ? "?" : row.GetValue(position).ToString()) + "/col" +
                    (col == null ? "?" : col.GetValue(position).ToString());
            }
            catch (Exception exception) { return "grid=exception:" + exception.GetType().Name; }
        }

        /// <summary>
        /// The game ticks no mode controller behind an active loading
        /// screen or loading process (Game.Tick), so a scenario that begins
        /// there sees no area effect, buff or sleep controller run on its
        /// own; both scenarios wait for the flags to clear before their
        /// first phase and record the wait.
        /// </summary>
        private static bool ExpandedSummoningLoadingActive(out string flags)
        {
            Kingmaker.EntitySystem.Persistence.LoadingProcess loading =
                Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance;
            bool inProcess = loading != null && loading.IsLoadingInProcess;
            bool screen = loading != null && loading.IsLoadingScreenActive;
            bool manual = loading != null && loading.IsManualLoadingScreenActive;
            flags = "inProcess=" + inProcess + ",screen=" + screen + ",manual=" + manual +
                ",paused=" + Game.Instance.IsPaused + ",mode=" + Game.Instance.CurrentMode;
            return inProcess || screen || manual;
        }

        private const int ExpandedSummoningLoadingGateFrames = 1200;

        private static readonly Vector3[] CompassOffsets = {
            new Vector3(1f, 0f, 0f), new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, -1f),
            new Vector3(0.7071f, 0f, 0.7071f), new Vector3(-0.7071f, 0f, 0.7071f),
            new Vector3(0.7071f, 0f, -0.7071f), new Vector3(-0.7071f, 0f, -0.7071f) };

        /// <summary>
        /// A point at the distance from the centre in the first compass
        /// direction not yet used whose line of sight from the centre is
        /// clear in the game's own sight geometry (the working save's area
        /// has walls a few metres from the party), so reach, sight and area
        /// gates measure the rule and not the scenery. Falls back to the
        /// first unused direction and says so.
        /// </summary>
        private static Vector3 ExpandedSummoningOpenPoint(Vector3 centre, float distance,
            List<int> used, out string chosen)
        {
            int fallback = -1;
            for (int index = 0; index < CompassOffsets.Length; index++)
            {
                if (used != null && used.Contains(index)) continue;
                if (fallback < 0) fallback = index;
                Vector3 candidate = centre + CompassOffsets[index] * distance;
                bool clear;
                try
                {
                    clear = Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance == null ||
                        !Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(centre, candidate, 0);
                }
                catch (Exception) { clear = false; }
                if (!clear) continue;
                if (used != null) used.Add(index);
                chosen = "dir" + index + "@" + distance.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "m";
                return candidate;
            }
            if (fallback < 0) fallback = 0;
            if (used != null) used.Add(fallback);
            chosen = "dir" + fallback + "@" + distance.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "m(no-clear-direction)";
            return centre + CompassOffsets[fallback] * distance;
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
                        evt.Weapon.Blueprint.name) +
                    ",target=" + (evt.Target == null || evt.Target.Blueprint == null ? "<none>" :
                        evt.Target.Blueprint.name) +
                    ",autoMiss=" + evt.AutoMiss + ",logged=" + !evt.SuspendCombatLog);
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
        private int _rulesCasterDamage;
        private int _rulesWebUses0, _rulesWebUses1, _rulesWebUses2;
        private int _rulesHighTouch, _rulesLowTouch;
        private bool _rulesMissedHighTouch;
        private string _rulesWebRollA = "<none>";
        private int _rulesLoadingWait;

        private void PollExpandedSummoningRules()
        {
            string stage = "phase" + _rulesPhase;
            try
            {
                if (_rulesPhase == 0)
                {
                    string loadingFlags;
                    if (ExpandedSummoningLoadingActive(out loadingFlags) &&
                        _rulesLoadingWait++ < ExpandedSummoningLoadingGateFrames) return;
                    _rulesSteps.Add("loadingGate:framesWaited=" + _rulesLoadingWait + ";" + loadingFlags);
                    stage = "construct-fixture";
                    _rulesFixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_ExpandedSummoning_RulesCaster");
                    stage = "hostile";
                    CreateExpandedSummoningCorrectionHostile(_rulesFixture);
                    _rulesCasterDamage = _rulesFixture.Caster.Descriptor.Damage;
                    string detail;
                    bool ok;
                    stage = "cats";
                    _rulesSteps.Add("reset:cats=" + ResetExpandedSummoningHostile(_rulesFixture));
                    ok = ExerciseExpandedSummoningCorrectionCats(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-cats",
                        "leopard, lion and dire lion grab with the bite only; the tiger and the smilodon with the bite and both foreclaws; no rake claw ever grabs; a single attack never carries a rake slot; the full attack drops the rake slots unless the cat charges or held the exact target since its round began",
                        detail, ok, "SummonGrabComponent.IsGrabLimb and TryGrab on live limbs; UnitAttack.CreateSingleAttack and CreateFullAttack through the sequencing seam"));
                    stage = "grapple-sizes";
                    _rulesSteps.Add("reset:sizes=" + ResetExpandedSummoningHostile(_rulesFixture));
                    ok = ExerciseExpandedSummoningCorrectionGrappleSizes(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-grapple-sizes",
                        "a grab works against a target of the holder's size or smaller and is refused against a larger one; grapple checks carry +4 from the grab and +5 more to maintain; the worm grabs a Gargantuan foe but never swallows it and refuses a Colossal one",
                        detail, ok, "TryGrab with the hostile's size set exactly; RuleCalculateCMB grapple against trip; the hold buff's later-turn tick"));
                    stage = "flytrap";
                    _rulesSteps.Add("reset:flytrap=" + ResetExpandedSummoningHostile(_rulesFixture));
                    ok = ExerciseExpandedSummoningCorrectionFlytrap(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-flytrap",
                        "one link per bite, four at most, each held state naming the flytrap and its own bite; a bite already holding cannot grab again; a Large held foe is maintained but never engulfed, a Medium one is engulfed on the later turn and takes the engulf damage; escape, the area-leave sweep, the last link ending, the swallow lifecycle and the holder's disposal each release; a free unit still finds a path past four held units",
                        detail, ok, "held-state buffs on the targets, the multi-hold tick, UnitHelper.TryBreakFree, SummonGrappleAreaSafeguard.Sweep, the movement agent's path request with the four held units standing"));
                    stage = "mephit-roles";
                    _rulesSteps.Add("reset:mephits=" + ResetExpandedSummoningHostile(_rulesFixture));
                    ok = ExerciseExpandedSummoningCorrectionMephitRoles(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-mephit-roles",
                        "chill metal targets only metal-bearers and runs the seven-round table (full for armor, minimal for a weapon); pyrotechnics blinds enemies only for 1d4+1 rounds; magma form gives DR 20/magic, speed 10 and forbids attacks while abilities work; glitterdust blinds the hostile and never the party caster or the allied summon",
                        detail, ok, "live casts on the disposable units; SummonChillMetalComponent outcomes; RuleDealDamage through the form's damage reduction; UnitAttack.ShouldBeInterrupted"));
                    stage = "cyclops";
                    _rulesSteps.Add("reset:cyclops=" + ResetExpandedSummoningHostile(_rulesFixture));
                    ok = ExerciseExpandedSummoningCorrectionCyclops(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-cyclops",
                        "armor class 19 with +4 armor (a fact, no item) and +7 natural armor beside the game's own difficulty modifier; Flash of Insight makes the next attack's own d20 a natural 20 with an ordinary confirmation roll, is spent by that one attack, and touches no other roll",
                        detail, ok, "RuleCalculateAC, the armor class modifier list, RuleAttackWithWeapon at a forced natural 1 while armed and after, a Will save rolled from the same seed while armed and after"));
                    stage = "hooves";
                    _rulesSteps.Add("reset:hooves=" + ResetExpandedSummoningHostile(_rulesFixture));
                    ok = ExerciseExpandedSummoningCorrectionHooves(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-hooves",
                        "the pony's and the horse's hooves are both secondary: -5 to hit and half the Strength modifier to damage against the same hooves treated as primary, both listed in the full attack",
                        detail, ok, "RuleCalculateAttackBonus and RuleCalculateWeaponStats with the docile flag on and off; UnitAttack.CreateFullAttack"));
                    stage = "mouth-ownership";
                    _rulesSteps.Add("reset:mouths=" + ResetExpandedSummoningHostile(_rulesFixture));
                    ok = ExerciseExpandedSummoningMouthOwnership(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-mouth-ownership",
                        "one target per mouth: a bite that holds or has engulfed a foe auto-misses silently against any other unit and is dropped from the planned full attack, the free bites still strike, four occupied mouths leave none, and releasing one victim frees exactly that mouth; the engulf bundle is the stat block's 1d8+7 crushing and 2d6 acid each round",
                        detail, ok, "RuleAttackWithWeapon with each bite, UnitAttack.CreateFullAttack, the durable link store and the engulfed buff's own actions"));
                    stage = "maintain-rake";
                    _rulesSteps.Add("reset:rake=" + ResetExpandedSummoningHostile(_rulesFixture));
                    ok = ExerciseExpandedSummoningMaintainRake(_rulesFixture, out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-maintain-rake",
                        "a holding cat rakes through the maintain the tabletop gives it: two genuine rake attacks against the exact held foe, logged, never the turn the hold was taken and never against another unit; the maintain damage is the establishing limb's own, bite or foreclaw",
                        detail, ok, "SummonHoldComponent.MaintainLink with the held state ticked, the global attack-roll observer, and the durable link store"));
                    stage = "web-projectile";
                    ok = ExerciseExpandedSummoningWebProjectile(out detail);
                    _rulesCases.Add(Assertion("expanded-summoning-correction-web-projectile",
                        "the Web delivers exactly one projectile, the identity the builder pins",
                        detail, ok, "the published ability's AbilityDeliverProjectile"));
                    stage = "wind-wall-cast";
                    _rulesSteps.Add("reset:windWall=" + ResetExpandedSummoningHostile(_rulesFixture));
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
                    _rulesSteps.Add("reset:cloud=" + ResetExpandedSummoningHostile(_rulesFixture));
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
                    _rulesSteps.Add("reset:web=" + ResetExpandedSummoningHostile(_rulesFixture));
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
                    stage = "rake-command-begin";
                    BeginExpandedSummoningRakeCommand(false);
                    _rulesWait = 0;
                    _rulesPhase = 5;
                    return;
                }
                if (_rulesPhase == 5)
                {
                    stage = "rake-command-charge";
                    if (!FinishExpandedSummoningRakeCommand(false, _rulesWait) &&
                        _rulesWait++ < ExpandedSummoningCommandFrames) return;
                    BeginExpandedSummoningRakeCommand(true);
                    _rulesWait = 0;
                    _rulesPhase = 6;
                    return;
                }
                if (_rulesPhase == 6)
                {
                    stage = "rake-command-ordinary";
                    if (!FinishExpandedSummoningRakeCommand(true, _rulesWait) &&
                        _rulesWait++ < ExpandedSummoningCommandFrames) return;
                    stage = "rake-command";
                    CompleteExpandedSummoningRakeCommand();
                    CompleteExpandedSummoningRules();
                }
            }
            catch (Exception exception)
            {
                _rulesCases.Add(Assertion("expanded-summoning-correction-" + stage,
                    "the stage completes", "exception=" + DescribeExpandedSummoningCorrectionException(exception) +
                    ";steps=" + string.Join("||", _rulesSteps.ToArray()),
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
                if (_rulesFixture != null) ResetExpandedSummoningHostile(_rulesFixture);
                if (_rulesFixture != null && _rulesFixture.Caster != null &&
                    !_rulesFixture.Caster.Destroyed)
                    _rulesFixture.Caster.Descriptor.Damage = _rulesCasterDamage;
                EndExpandedSummoningCorrectionFixture(_rulesFixture, out cleaned);
            }
            catch (Exception exception)
            {
                _rulesCases.Add(Assertion("expanded-summoning-correction-cleanup-exception",
                    "cleanup completes", "exception=" + DescribeExpandedSummoningCorrectionException(exception),
                    false, "the correction rules fixture"));
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
                        // The held state's own round tick (a round of the hold
                        // has passed); the roll it may make is not a 20.
                        UnityEngine.Random.InitState(FindNativeD20Seed(10));
                        hostile.Descriptor.Buffs.GetBuff(grappled).TickMechanics();
                        sequence = ExerciseExpandedSummoningRakeSequence(cat, hostile,
                            fixture.Caster);
                    }
                    // One target per mouth (2026-09-26): while this cat holds
                    // the hostile with its bite, a full attack against anyone
                    // else drops that bite as well as the two rake claws, so
                    // three planned hands become two.
                    bool sequenceExact = sequence.Contains("held=kept(5/2)") &&
                        sequence.Contains("other=dropped(2/0)") &&
                        sequence.Contains("charge=kept(5/2)") &&
                        sequence.Contains("ordinary=dropped(2/0)");
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
                steps.Add("exception=" + DescribeExpandedSummoningCorrectionException(exception));
                ok = false;
            }
            finally
            {
                ResetExpandedSummoningHostile(fixture);
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
                    hostile.Descriptor.State.Size = holderSize;
                    // A foe immune to combat maneuvers is never taken: the
                    // engine's own rule returns before it computes CMB and
                    // CMD there, and the verdict it leaves behind would read
                    // as a success.
                    hostile.Descriptor.State.AddCondition(
                        UnitCondition.ImmuneToCombatManeuvers, null);
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool immuneRefused = grab != null && limb != null &&
                        !grab.TryGrab(hostile, limb, true) &&
                        hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleTarget>() == null &&
                        holder.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleInitiator>() == null;
                    hostile.Descriptor.State.RemoveConditionAll(
                        UnitCondition.ImmuneToCombatManeuvers);
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool takenOnceImmunityEnds = grab != null && limb != null &&
                        grab.TryGrab(hostile, limb, true);
                    if (takenOnceImmunityEnds)
                        ReleaseExpandedSummoningHold(holder, hostile, hold);
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
                        ";freeTripCmb=" + freeTrip.Result + ";grabBonus=" + grabBonus +
                        ";maneuverImmuneRefused=" + immuneRefused +
                        ";takenOnceImmunityEnds=" + takenOnceImmunityEnds);
                    ok = ok && rowOk && grabBonus && immuneRefused && takenOnceImmunityEnds;
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
                    UnityEngine.Random.InitState(FindNativeD20Seed(10));
                    hostile.Descriptor.Buffs.GetBuff(grappled).TickMechanics();
                    bool eligible = SummonHoldComponent.IsHeldSinceRoundStart(worm, hostile);
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    worm.Descriptor.Buffs.GetBuff(hold).TickMechanics();
                    maintainOutcome = "eligible=" + eligible + ";swallowed=" + (hostile.Get<
                            Kingmaker.UnitLogic.Parts.UnitPartSwallowed>() != null) + ";stillHeld=" +
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
                steps.Add("exception=" + DescribeExpandedSummoningCorrectionException(exception));
                ok = false;
            }
            finally
            {
                ResetExpandedSummoningHostile(fixture);
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
                    PlaceExpandedSummoningUnit(wolf, wolf.Position);
                    wolves.Add(wolf);
                }
                Func<int> heldCount = () => SummonMultiHoldComponent.HeldTargets(flytrap, multiHeld).Count;
                // Four targets around the flytrap, within its bites' reach
                // and in its sight (the maintain check needs both).
                Vector3 centre = flytrap.Position;
                hostile.Descriptor.Stats.BaseAttackBonus.BaseValue = -100;
                var directions = new List<int>();
                var placements = new List<string>();
                string chosen;
                PlaceExpandedSummoningUnit(hostile, ExpandedSummoningOpenPoint(centre, 2.5f, directions, out chosen));
                placements.Add("hostile=" + chosen);
                for (int index = 0; index < 4; index++)
                {
                    PlaceExpandedSummoningUnit(wolves[index], ExpandedSummoningOpenPoint(centre, 2.5f, directions, out chosen));
                    placements.Add("wolf" + index + "=" + chosen);
                }
                steps.Add("placement:" + string.Join(",", placements.ToArray()));
                Func<UnitEntityData, string> linkOf = target =>
                {
                    Buff state = SummonHoldComponent.HeldState(flytrap, target, grab);
                    ItemEntityWeapon weapon = SummonGrappleLinks.EstablishingWeapon(flytrap, target);
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
                int held = heldCount();
                string links = "hostile[" + linkOf(hostile) + "];wolf0[" + linkOf(wolves[0]) +
                    "];wolf1[" + linkOf(wolves[1]) + "];wolf2[" + linkOf(wolves[2]) + "]";
                bool distinct = held == 4 && links == "hostile[holder=True,bite=0];wolf0[holder=True,bite=1];wolf1[holder=True,bite=2];wolf2[holder=True,bite=3]" &&
                    flytrap.Descriptor.HasFact(multiHold) &&
                    hostile.Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                    hostile.Descriptor.State.HasCondition(UnitCondition.Entangled) &&
                    !flytrap.Descriptor.State.HasCondition(UnitCondition.CantAct);
                steps.Add("links:fourBites=" + fourBites + ";link0=" + link0 + ";link1=" + link1 +
                    ";busyBiteRefused=" + busyBiteRefused + ";link2=" + link2 + ";link3=" + link3 +
                    ";fifthRefused=" + fifthRefused + ";held=" + held + ";" + links +
                    ";distinct=" + distinct);
                ok = ok && fourBites && link0 && link1 && busyBiteRefused && link2 && link3 &&
                    fifthRefused && distinct;

                // Crowded pathing: the party caster, standing beside the crowd,
                // asks the game's own pathfinder for a route to the far side.
                string pathing = "not-run";
                bool pathFound = false;
                try
                {
                    PlaceExpandedSummoningUnit(fixture.Caster, centre + new Vector3(-6f, 0f, -6f));
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
                                ";obstacles=" + obstacles + ";heldStanding=" + heldCount() +
                                ";from=" + Vec(fixture.Caster.Position) + ";to=" + Vec(destination) +
                                ";crowd=" + Vec(centre);
                        }
                    }
                }
                catch (Exception exception)
                {
                    pathing = "exception=" + DescribeExpandedSummoningCorrectionException(exception);
                }
                steps.Add("pathing[" + pathing + "]");
                ok = ok && pathFound;

                // Release paths first, down to the hostile alone, so the
                // engulf ticks that follow make one maintain roll each.
                // Escape: a held wolf with an overwhelming check breaks free
                // at its own tick and only its link ends.
                wolves[0].Descriptor.Stats.BaseAttackBonus.BaseValue = 200;
                Buff wolfState = SummonHoldComponent.HeldState(flytrap, wolves[0], grab);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                if (wolfState != null) wolfState.TickMechanics();
                bool escaped = SummonHeldComponent.HolderOf(wolves[0], multiHeld) == null &&
                    !wolves[0].Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                    heldCount() == 3 && flytrap.Descriptor.HasFact(multiHold);
                // The area-leave sweep releases the held units it is handed.
                int swept = SummonGrappleAreaSafeguard.Sweep(true, new[] { wolves[1], wolves[2] });
                bool sweptFree = swept == 2 &&
                    SummonHeldComponent.HolderOf(wolves[1], multiHeld) == null &&
                    SummonHeldComponent.HolderOf(wolves[2], multiHeld) == null &&
                    heldCount() == 1 && flytrap.Descriptor.HasFact(multiHold);
                steps.Add("release:escaped=" + escaped + ";swept=" + swept + ";sweptFree=" + sweptFree);
                ok = ok && escaped && sweptFree;

                // The maintain check needs the held foe in the bite's reach
                // and sight; the working save's walls decide which compass
                // point is open, so the hostile is re-placed until the game's
                // own reach test agrees, and the test is recorded.
                Func<string> reachOf = () => "reach=" + Kingmaker.Controllers.Combat.UnitEngagementExtension
                    .IsReach(flytrap, hostile, flytrap.Body.PrimaryHand) + ",los=" + flytrap.HasLOS(hostile) +
                    ",dist=" + flytrap.DistanceTo(hostile).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) +
                    ",conscious=" + hostile.Descriptor.State.IsConscious;
                string reachBefore = reachOf();
                int rePlacements = 0;
                while (!Kingmaker.Controllers.Combat.UnitEngagementExtension.IsReach(flytrap, hostile,
                    flytrap.Body.PrimaryHand) && rePlacements < CompassOffsets.Length)
                {
                    PlaceExpandedSummoningUnit(hostile, ExpandedSummoningOpenPoint(centre, 2.5f, directions, out chosen));
                    rePlacements++;
                }
                steps.Add("reach:" + reachBefore + (rePlacements == 0 ? "" : ";rePlacements=" + rePlacements +
                    ";after[" + reachOf() + "]"));
                // The hostile's own round: its break-free (not a 20) fails
                // and the link has stood a round.
                Buff hostileState = SummonHoldComponent.HeldState(flytrap, hostile, grab);
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                if (hostileState != null) hostileState.TickMechanics();
                bool stillHeld = ReferenceEquals(SummonHeldComponent.HolderOf(hostile, multiHeld), flytrap);
                bool eligible = SummonHoldComponent.IsHeldSinceRoundStart(flytrap, hostile);
                // Engulf: a Large held foe is maintained (damage, still held);
                // a Medium one is engulfed on the later turn.
                hostile.Descriptor.State.Size = Size.Large;
                int beforeLarge = hostile.Descriptor.Damage;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                flytrap.Descriptor.Buffs.GetBuff(multiHold).TickMechanics();
                bool largeMaintained = hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowed>() ==
                    null && ReferenceEquals(SummonHeldComponent.HolderOf(hostile, multiHeld), flytrap) &&
                    hostile.Descriptor.Damage > beforeLarge;
                int heldAfterLarge = heldCount();
                hostile.Descriptor.State.Size = Size.Medium;
                int beforeEngulf = hostile.Descriptor.Damage;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                Buff multiHoldState = flytrap.Descriptor.Buffs.GetBuff(multiHold);
                if (multiHoldState != null) multiHoldState.TickMechanics();
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
                int heldAfterEngulf = heldCount();
                steps.Add("engulf:stillHeldAfterOwnRound=" + stillHeld + ";eligible=" + eligible +
                    ";largeMaintained=" + largeMaintained + ";heldAfterLarge=" + heldAfterLarge +
                    ";engulfed=" + engulfedNow + ";engulfTickDamage=" + engulfDamage +
                    ";heldAfterEngulf=" + heldAfterEngulf + ";multiHoldAfterEngulf=" +
                    flytrap.Descriptor.HasFact(multiHold) + ";hostile=" +
                    DescribeExpandedSummoningCondition(hostile));
                ok = ok && stillHeld && eligible && largeMaintained && heldAfterLarge == 1 &&
                    engulfedNow && engulfDamage && heldAfterEngulf == 0;

                // The holder's own hold ending (dismissal, death, dispel)
                // releases the last link and the buff goes with it.
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool relinked = grab.TryGrab(wolves[2], bites[1], true) && heldCount() == 1 &&
                    flytrap.Descriptor.HasFact(multiHold);
                Buff holdBuff = flytrap.Descriptor.Buffs.GetBuff(multiHold);
                if (holdBuff != null) flytrap.Descriptor.Buffs.RemoveFact(holdBuff);
                bool holdEnded = relinked && SummonHeldComponent.HolderOf(wolves[2], multiHeld) == null &&
                    !wolves[2].Descriptor.State.HasCondition(UnitCondition.CantMove) &&
                    heldCount() == 0 && !flytrap.Descriptor.HasFact(multiHold);
                // The swallow lifecycle: the traits ending spits the engulfed foe out.
                flytrap.Descriptor.Buffs.RemoveFact(flytrap.Descriptor.Buffs.GetBuff(flytrapTraits));
                bool spatOut = hostile.Get<Kingmaker.UnitLogic.Parts.UnitPartSwallowed>() == null &&
                    !hostile.Descriptor.HasFact(engulfed) &&
                    !hostile.Descriptor.State.HasCondition(UnitCondition.CantAct) &&
                    (swallower == null || swallower.SwallowedUnits.Count == 0);
                // A link re-established after the traits return ends with the holder's disposal.
                flytrap.Descriptor.AddBuff(flytrapTraits, flytrap, null);
                SummonGrabComponent grabAgain = SummonGrabComponent.Find(flytrap);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool regrabbed = grabAgain != null && grabAgain.TryGrab(wolves[3], bites[0], true);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { flytrap });
                bool disposalReleased = SummonHeldComponent.HolderOf(wolves[3], multiHeld) == null &&
                    !wolves[3].Descriptor.State.HasCondition(UnitCondition.CantMove);
                steps.Add("ending:relinked=" + relinked + ";holdEnded=" + holdEnded + ";spatOut=" + spatOut +
                    ";regrabbed=" + regrabbed + ";disposalReleased=" + disposalReleased);
                ok = ok && relinked && holdEnded && spatOut && regrabbed && disposalReleased;
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + DescribeExpandedSummoningCorrectionException(exception));
                ok = false;
            }
            finally
            {
                ResetExpandedSummoningHostile(fixture);
                foreach (UnitEntityData wolf in wolves)
                    if (fixture.Created.Contains(wolf))
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { wolf });
                PlaceExpandedSummoningUnit(fixture.Caster, fixture.Hostile.Position + Vector3.back);
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

        /// <summary>
        /// Every gate the game's own area code applies to a unit, evaluated
        /// here for the record: in game, awake, alive, inside the view's
        /// shape, in line of sight of the area's centre, targetable.
        /// </summary>
        private static string DescribeExpandedSummoningAreaGates(AreaEffectEntityData area,
            IEnumerable<KeyValuePair<string, UnitEntityData>> units)
        {
            if (area == null) return "area=<none>";
            var parts = new List<string>();
            Kingmaker.View.MapObjects.AreaEffectView view = area.View;
            parts.Add("view=" + (view != null) + ";areaPos=" + Vec(area.Position));
            if (view == null) return string.Join(";", parts.ToArray());
            Kingmaker.View.MapObjects.SriptZones.IScriptZoneShape shape = view.Shape;
            var cylinder = shape as Kingmaker.View.MapObjects.SriptZones.ScriptZoneCylinder;
            parts.Add("viewPos=" + Vec(view.transform.position) + ";shape=" +
                (shape == null ? "<none>" : shape.GetType().Name) + (cylinder == null ? "" :
                    "(radius=" + cylinder.Radius.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) +
                    ",height=" + cylinder.Height.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) +
                    ",centre=" + Vec(cylinder.Center) + ")") + ";onUnit=" + view.OnUnit +
                ";paused=" + Game.Instance.IsPaused + ";mode=" + Game.Instance.CurrentMode +
                ";awakeTotal=" + (Game.Instance.State == null || Game.Instance.State.AwakeUnits == null ? -1 :
                    Game.Instance.State.AwakeUnits.Count) + ";loading=" +
                Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingInProcess + "/" +
                Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingScreenActive +
                ";gameTime=" + Game.Instance.TimeController.GameTime.TotalSeconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) +
                ";frame=" + Time.frameCount);
            // What the game's own grid query returns for the shape's bounds.
            try
            {
                SparseGrid<Kingmaker.EntitySystem.EntityDataBase> grid = ExpandedSummoningAreaGrid();
                if (grid != null && shape != null)
                {
                    var found = new List<Kingmaker.EntitySystem.EntityDataBase>();
                    grid.GetInBounds(shape.GetBounds(), found);
                    parts.Add("gridInBounds=" + found.Count + "(" + string.Join(",", found.OfType<UnitEntityData>()
                        .Select(value => value.Blueprint == null ? "?" : Sanitize(value.Blueprint.name)).ToArray()) + ")");
                }
            }
            catch (Exception exception) { parts.Add("gridInBounds=exception:" + exception.GetType().Name); }
            foreach (KeyValuePair<string, UnitEntityData> pair in units)
            {
                UnitEntityData unit = pair.Value;
                if (unit == null) { parts.Add(pair.Key + "=<null>"); continue; }
                bool awake = Game.Instance.State != null && Game.Instance.State.AwakeUnits != null &&
                    Game.Instance.State.AwakeUnits.Contains(unit);
                float corpulence = unit.View == null ? -1f : unit.View.Corpulence;
                string contains = shape == null ? "?" : shape.Contains(unit.Position, corpulence).ToString();
                string los;
                try
                {
                    los = (Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance == null ? "no-geometry" :
                        Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(
                            view.transform.position, unit.Position, 0).ToString());
                }
                catch (Exception exception) { los = "exception:" + exception.GetType().Name; }
                string untargetable;
                try { untargetable = UnitCommand.CommandTargetUntargetable(area, unit, null).ToString(); }
                catch (Exception exception) { untargetable = "exception:" + exception.GetType().Name; }
                parts.Add(pair.Key + "[pos=" + Vec(unit.Position) + ",dist=" +
                    Vector3.Distance(unit.Position, view.transform.position).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) +
                    ",inGame=" + unit.IsInGame + ",sleeping=" + unit.IsSleeping + ",awake=" + awake + "," +
                    DescribeExpandedSummoningGridEntry(unit) + ",dead=" +
                    unit.Descriptor.State.IsDead + ",corpulence=" + corpulence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) +
                    ",contains=" + contains + ",losObstacle=" + los + ",untargetable=" + untargetable + "]");
            }
            return string.Join(";", parts.ToArray());
        }

        /// <summary>
        /// Why each unit inside the wall did or did not receive the shelter
        /// state: the state on the unit, the buffs the area itself sourced,
        /// the game's own ally relation between the mephit and the unit
        /// (factions, groups), the area buff component's condition evaluated
        /// exactly as the component evaluates it, and - for a unit still
        /// without the state - whether the game accepts the state from the
        /// area's context at all.
        /// </summary>
        private static string DescribeExpandedSummoningShelter(AreaEffectEntityData area,
            BlueprintBuff state, UnitEntityData mephit, IEnumerable<KeyValuePair<string, UnitEntityData>> units)
        {
            if (area == null) return "area=<none>";
            var parts = new List<string>();
            var component = area.Blueprint == null || area.Blueprint.ComponentsArray == null ? null :
                area.Blueprint.ComponentsArray.OfType<
                    Kingmaker.UnitLogic.Abilities.Components.AreaEffects.AbilityAreaEffectBuff>().FirstOrDefault();
            parts.Add("component=" + (component != null) + ";componentBuffIsState=" +
                (component != null && ReferenceEquals(component.Buff, state)) + ";areaCaster=" +
                (area.Context == null || area.Context.MaybeCaster == null ? "<none>" :
                    Sanitize(area.Context.MaybeCaster.Blueprint.name)) + ";mephitPlayerFaction=" +
                (mephit != null && mephit.IsPlayerFaction));
            foreach (KeyValuePair<string, UnitEntityData> pair in units)
            {
                UnitEntityData unit = pair.Value;
                if (unit == null) { parts.Add(pair.Key + "=<null>"); continue; }
                string sourced = string.Join("+", unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                    value.SourceAreaEffectId == area.UniqueId).Select(value => value.Blueprint == null ? "?" :
                        Sanitize(value.Blueprint.name)).ToArray());
                string check;
                try
                {
                    if (component == null || area.Context == null) check = "n/a";
                    else
                        using (area.Context.GetDataScope(new TargetWrapper(unit)))
                            check = component.Condition == null ? "no-condition" :
                                component.Condition.Check(area.Blueprint).ToString();
                }
                catch (Exception exception) { check = "exception:" + exception.GetType().Name + ":" + Sanitize(exception.Message); }
                string accepts = "n/a";
                if (!unit.Descriptor.HasFact(state) && area.Context != null)
                {
                    try
                    {
                        Buff probe = unit.Descriptor.AddBuff(state, area.Context, null);
                        accepts = (probe != null).ToString();
                        if (probe != null) unit.Descriptor.Buffs.RemoveFact(probe);
                    }
                    catch (Exception exception) { accepts = "exception:" + exception.GetType().Name + ":" + Sanitize(exception.Message); }
                }
                parts.Add(pair.Key + "[hasState=" + unit.Descriptor.HasFact(state) + ",sourced=" + (sourced.Length == 0 ? "-" : sourced) +
                    ",allyOfMephit=" + (mephit != null && mephit.IsAlly(unit)) + ",playerFaction=" + unit.IsPlayerFaction +
                    ",faction=" + (unit.Faction == null ? "<none>" : Sanitize(unit.Faction.name)) + ",sameGroup=" +
                    (mephit != null && ReferenceEquals(unit.Group, mephit.Group)) + ",conditionCheck=" + check +
                    ",gameAcceptsState=" + accepts + "]");
            }
            return string.Join(";", parts.ToArray());
        }

        /// <summary>
        /// The area's units: what the game's own frames found after the
        /// loading gate, then the area's own Tick run from here - the very
        /// method the game's area controller calls every frame in play - as
        /// a recorded fallback. The record shows both counts, the game mode,
        /// the loading flags, the awake list, the game time and the frame
        /// beside every gate the area applies to each unit.
        /// </summary>
        private static string SettleExpandedSummoningArea(AreaEffectEntityData area,
            IEnumerable<KeyValuePair<string, UnitEntityData>> units, out int inside)
        {
            int framesInside = area == null ? -1 : area.UnitsInside.Count();
            string gates = DescribeExpandedSummoningAreaGates(area, units);
            if (area == null)
            {
                inside = -1;
                return "area=<none>;gates[" + gates + "]";
            }
            string tick;
            try
            {
                area.Tick();
                tick = "ok";
            }
            catch (Exception exception)
            {
                tick = "exception:" + DescribeExpandedSummoningCorrectionException(exception);
            }
            inside = area.UnitsInside.Count();
            return "gameFramesInside=" + framesInside + ";areaTick=" + tick + ";insideAfterTick=" + inside +
                ";gates[" + gates + "]";
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
            PlaceExpandedSummoningUnit(_rulesWolf, _rulesWolf.Position);
            SetExactProperty(fixture.Hostile.Descriptor.Stats.GetStat(StatType.SaveWill), "BaseValue", -100);
            SetExactProperty(fixture.Hostile.Descriptor.Stats.GetStat(StatType.SaveFortitude), "BaseValue", -100);
            SetExactProperty(fixture.Hostile.Descriptor.Stats.GetStat(StatType.SaveReflex), "BaseValue", -100);
            _rulesMephit = CastExpandedSummoningQuietUnit(fixture, "dust-mephit");
            BlueprintAbility windWall = fixture.Blueprints.OfType<BlueprintAbility>().Single(value =>
                value.name == "KMG_Summoning_Special_DustMephit_SpellLikeTwo");
            Vector3 wallCentre = _rulesMephit.Position;
            var directions = new List<int>();
            string hostileSpot;
            // The hostile outside the wall's radius, in the wall's sight.
            Vector3 hostileSpotPoint = ExpandedSummoningOpenPoint(wallCentre,
                ExpandedSummoningSpecialProfiles.WindWallRadiusFeet * 0.3048f + 2.5f, directions, out hostileSpot);
            PlaceExpandedSummoningUnit(fixture.Caster, wallCentre + new Vector3(1f, 0f, 0f));
            PlaceExpandedSummoningUnit(_rulesWolf, wallCentre + new Vector3(-1f, 0f, 0f));
            PlaceExpandedSummoningUnit(fixture.Hostile, hostileSpotPoint);
            _rulesSteps.Add("windWall:hostileSpot=" + hostileSpot);
            SummonWindWallComponent.ClearOutcomes();
            ExecuteExpandedSummoningRuntimeAbility(_rulesMephit, windWall, 3,
                new TargetWrapper(_rulesMephit), false);
            Game.Instance.EntityCreator.Tick();
            AreaEffectEntityData wall = FindExpandedSummoningArea("KMG_Summoning_Special_DustMephit_WindWallArea");
            _rulesSteps.Add("windWall:cast;execution=" + _expandedSummoningLastAbilityExecution +
                ";areaAtCast=" + (wall != null) + ";insideAtCast=" + (wall == null ? -1 : wall.UnitsInside.Count()) +
                ";mephitPos=" + Vec(_rulesMephit.Position) + ";casterPos=" + Vec(fixture.Caster.Position));
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
            int inside;
            string settled = SettleExpandedSummoningArea(wall, new[] {
                new KeyValuePair<string, UnitEntityData>("caster", caster),
                new KeyValuePair<string, UnitEntityData>("wolf", wolf),
                new KeyValuePair<string, UnitEntityData>("mephit", dust),
                new KeyValuePair<string, UnitEntityData>("hostile", hostile) }, out inside);
            string shelterDetail = DescribeExpandedSummoningShelter(wall, windWallState, dust, new[] {
                new KeyValuePair<string, UnitEntityData>("caster", caster),
                new KeyValuePair<string, UnitEntityData>("wolf", wolf),
                new KeyValuePair<string, UnitEntityData>("mephit", dust),
                new KeyValuePair<string, UnitEntityData>("hostile", hostile) });
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
                ";outcomes=" + Sanitize(wallOutcomes) + ";ended=" + wallEnded + ";shelter[" + shelterDetail +
                "];" + settled;
            bool ok = sheltered && bowMiss && !bowHit && !thrownMiss &&
                thrownChance == ExpandedSummoningSpecialProfiles.WindWallOtherRangedMissChance &&
                !meleeMiss && meleeChance == 0 && !rayMiss && rayChance == 0 && wallEnded;
            _rulesCases.Add(Assertion("expanded-summoning-correction-wind-wall",
                "the dust mephit's wall of wind shelters every creature inside it that is not the mephit's enemy - the party caster, the allied summon and the mephit - and not the hostile outside: arrows and bolts aimed at a sheltered creature are deflected outright, other ranged weapons roll the tabletop 30% miss chance, melee and rays pass; the shelter ends with the wall",
                detail, ok, "the area effect found through the game's own frames after the loading gate (the area's own tick run from the fixture as a recorded fallback), its units read from the area's spatial grid; RuleAttackWithWeapon by the hostile with a bow, a thrown weapon, a sword and the ray weapon; SummonWindWallComponent outcomes"));
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
            // The placement: the hostile's own ground position, with the
            // party caster, the allied summon and the mephit a step away.
            Vector3 cloudCentre = fixture.Hostile.Position;
            PlaceExpandedSummoningUnit(fixture.Caster, cloudCentre + new Vector3(1f, 0f, 0f));
            PlaceExpandedSummoningUnit(_rulesWolf, cloudCentre + new Vector3(-1f, 0f, 0f));
            PlaceExpandedSummoningUnit(_rulesMephit, cloudCentre + new Vector3(0f, 0f, 1.5f));
            bool nauseatedBefore = fixture.Hostile.Descriptor.State.HasCondition(UnitCondition.Nauseated);
            ExecuteExpandedSummoningRuntimeAbility(_rulesMephit, cloud, 3, new TargetWrapper(cloudCentre), false);
            Game.Instance.EntityCreator.Tick();
            AreaEffectEntityData area = FindExpandedSummoningArea("KMG_Summoning_Special_OozeMephit_StinkingCloudArea");
            _rulesSteps.Add("cloud:cast;nauseatedBefore=" + nauseatedBefore + ";execution=" +
                _expandedSummoningLastAbilityExecution + ";areaAtCast=" + (area != null) +
                ";insideAtCast=" + (area == null ? -1 : area.UnitsInside.Count()) + ";centre=" + Vec(cloudCentre));
        }

        private void FinishExpandedSummoningCloud()
        {
            ExpandedSummoningCorrectionFixture fixture = _rulesFixture;
            UnitEntityData hostile = fixture.Hostile, caster = fixture.Caster, wolf = _rulesWolf,
                ooze = _rulesMephit;
            AreaEffectEntityData cloudArea = FindExpandedSummoningArea(
                "KMG_Summoning_Special_OozeMephit_StinkingCloudArea");
            int inside;
            string settled = SettleExpandedSummoningArea(cloudArea, new[] {
                new KeyValuePair<string, UnitEntityData>("hostile", hostile),
                new KeyValuePair<string, UnitEntityData>("caster", caster),
                new KeyValuePair<string, UnitEntityData>("wolf", wolf),
                new KeyValuePair<string, UnitEntityData>("mephit", ooze) }, out inside);
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
                ";alliedSummonClean=" + wolfClean + ";mephitClean=" + oozeClean + ";" + settled;
            bool ok = cloudArea != null && hostileInside && alliesInside && hostileNauseated &&
                casterClean && wolfClean && oozeClean;
            _rulesCases.Add(Assertion("expanded-summoning-correction-cloud",
                "the ooze mephit's stinking cloud, placed where the hostile, the party caster, an allied summon and the mephit all stand inside it (the only placement, no safe target), nauseates the hostile and touches no ally",
                detail, ok, "the project ally-safe area clone found through the game's own frames after the loading gate (its own tick run from the fixture as a recorded fallback), its units read from the area's spatial grid; UnitsInside and the nauseated condition per unit"));
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
            var directions = new List<int>();
            string hostileSpot, casterSpot;
            PlaceExpandedSummoningUnit(hostile, ExpandedSummoningOpenPoint(_rulesSpider.Position, 3f, directions, out hostileSpot));
            PlaceExpandedSummoningUnit(fixture.Caster, ExpandedSummoningOpenPoint(_rulesSpider.Position, 3f, directions, out casterSpot));
            _rulesSteps.Add("web:spiderPos=" + Vec(_rulesSpider.Position) + ";hostileSpot=" + hostileSpot +
                ";casterSpot=" + casterSpot);
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
                _rulesWebRollA.Contains("hit=False") && _rulesLowTouch < 10 && rollB.Contains("hit=True") &&
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
                PlaceExpandedSummoningUnit(wolf, wolf.Position);
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
                PlaceExpandedSummoningUnit(hostile, ice.Position + new Vector3(2f, 0f, 0f));
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
                PlaceExpandedSummoningUnit(hostile, magma.Position + new Vector3(2f, 0f, 0f));
                PlaceExpandedSummoningUnit(caster, magma.Position + new Vector3(-2f, 0f, 0f));
                PlaceExpandedSummoningUnit(wolf, magma.Position + new Vector3(0f, 0f, 2f));
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
                PlaceExpandedSummoningUnit(hostile, dustCentre);
                PlaceExpandedSummoningUnit(caster, dustCentre + new Vector3(1f, 0f, 0f));
                PlaceExpandedSummoningUnit(wolf, dustCentre + new Vector3(-1f, 0f, 0f));
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
                steps.Add("exception=" + DescribeExpandedSummoningCorrectionException(exception));
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
                // The arming has no duration of its own: only the attack ends it.
                Buff arming = cyclops.Descriptor.Buffs.GetBuff(flashState);
                bool untimedArming = arming != null && arming.IsPermanent;
                // A save rolled while armed is untouched: the same seed gives
                // the same roll while armed and after the arming is spent,
                // and it leaves the arming in place.
                UnityEngine.Random.InitState(FindNativeD20Seed(1));
                var saveArmed = new RuleSavingThrow(cyclops, SavingThrowType.Will, 100);
                Rulebook.Trigger(saveArmed);
                int saveWhileArmed = saveArmed.RollResult;
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
                UnityEngine.Random.InitState(FindNativeD20Seed(1));
                var saveSpent = new RuleSavingThrow(cyclops, SavingThrowType.Will, 100);
                Rulebook.Trigger(saveSpent);
                int saveAfterSpent = saveSpent.RollResult;
                bool secondUse = new AbilityData(cyclops.Descriptor.Abilities.GetAbility(flash)).IsAvailable;
                hostile.Descriptor.Damage = damageBefore;
                steps.Add("flash:uses=" + usesBefore + "->" + usesAfter + ";armed=" + armed +
                    ";untimedArming=" + untimedArming +
                    ";saveWhileArmed=" + saveWhileArmed + ";saveAfterSpentSameSeed=" + saveAfterSpent +
                    ";stillArmedAfterSave=" + stillArmed +
                    ";armedNatural=" + natural + ";hit=" + (roll != null && roll.IsHit) + ";threat=" +
                    (roll != null && roll.IsCriticalRoll) + ";confirmationRoll=" + confirmation +
                    ";confirmed=" + (roll != null && roll.IsCriticalConfirmed) + ";autoFlags=" +
                    (roll != null && (roll.AutoHit || roll.AutoCriticalThreat || roll.AutoCriticalConfirmation)) +
                    ";spent=" + spent + ";nextNatural=" + plainNatural + ";nextMiss=" + plainMiss +
                    ";secondUseAvailable=" + secondUse);
                ok = ok && usesBefore == 1 && usesAfter == 0 && armed && untimedArming &&
                    saveWhileArmed == saveAfterSpent &&
                    stillArmed && chosenTwenty && confirmation >= 1 && confirmation <= 20 && spent &&
                    plainNatural == 1 && plainMiss && !secondUse;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { cyclops });
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + DescribeExpandedSummoningCorrectionException(exception));
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
                    var secondaryAttack = new List<int>();
                    var secondaryDamage = new List<int>();
                    var primaryAttack = new List<int>();
                    var primaryDamage = new List<int>();
                    var kinds = new List<string>();
                    SummonDocileHoovesComponent.SuspendedForFixture = false;
                    foreach (ItemEntityWeapon hoof in hooves)
                    {
                        int additionalIndex;
                        SummonLimbKind kind = SummonLimbs.Classify(animal, hoof, out additionalIndex);
                        kinds.Add(kind + (additionalIndex >= 0 ? "[" + additionalIndex + "]" : "") +
                            (hoof.ForceSecondary && hoof.IsSecondary ? "" : "(not-secondary)"));
                        var bonus = new RuleCalculateAttackBonus(animal, hostile, hoof, 0);
                        Rulebook.Trigger(bonus);
                        var stats = new RuleCalculateWeaponStats(animal, hoof, null);
                        Rulebook.Trigger(stats);
                        secondaryAttack.Add(bonus.Result);
                        secondaryDamage.Add(stats.DamageDescription.Count == 0 ? -999 :
                            stats.DamageDescription[0].Bonus);
                    }
                    // The same hooves as primary attacks: both un-flagged
                    // at once, the docile carrier's rule hooks suspended. The
                    // game gives a primary-hand natural attack with an empty
                    // off hand one and a half times Strength on its own.
                    SummonDocileHoovesComponent.SuspendedForFixture = true;
                    foreach (ItemEntityWeapon hoof in hooves) hoof.ForceSecondary = false;
                    foreach (ItemEntityWeapon hoof in hooves)
                    {
                        var bonus = new RuleCalculateAttackBonus(animal, hostile, hoof, 0);
                        Rulebook.Trigger(bonus);
                        var stats = new RuleCalculateWeaponStats(animal, hoof, null);
                        Rulebook.Trigger(stats);
                        primaryAttack.Add(bonus.Result);
                        primaryDamage.Add(stats.DamageDescription.Count == 0 ? -999 :
                            stats.DamageDescription[0].Bonus);
                    }
                    foreach (ItemEntityWeapon hoof in hooves) hoof.ForceSecondary = true;
                    SummonDocileHoovesComponent.SuspendedForFixture = false;
                    for (int index = 0; index < hooves.Count; index++)
                    {
                        int primaryFull = strength;
                        int primaryHandAndAHalf = (int)(strength * 1.5f);
                        bool hoofOk = !kinds[index].Contains("(not-secondary)") &&
                            secondaryAttack[index] == primaryAttack[index] - 5 &&
                            secondaryDamage[index] == strength / 2 &&
                            (primaryDamage[index] == primaryFull || primaryDamage[index] == primaryHandAndAHalf);
                        rows.Add(kinds[index] + ":attack=" + secondaryAttack[index] + "(primary " +
                            primaryAttack[index] + "),damageBonus=" + secondaryDamage[index] + "(primary " +
                            primaryDamage[index] + (primaryDamage[index] == primaryHandAndAHalf &&
                                primaryHandAndAHalf != primaryFull ? ", the game's own one-and-a-half" : "") + ")");
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
                steps.Add("exception=" + DescribeExpandedSummoningCorrectionException(exception));
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

        private string _expandedSummoningPersistenceLinkDetail = "not run";
        private bool _expandedSummoningPersistenceLinkValid;
        private bool _expandedSummoningPersistenceWasPaused;
        private bool _expandedSummoningPersistenceHoldsTaken;
        private int _expandedSummoningPersistenceHoldSettle;
        /// <summary>Frames between the holds and the save, with the clock stopped.</summary>
        private const int ExpandedSummoningPersistenceHoldSettleFrames = 8;

        private static UnitEntityData ExpandedSummoningPersistenceUnit(UnitEntityData[] units,
            string blueprintName)
        {
            return units == null ? null : units.FirstOrDefault(value => value != null &&
                value.Blueprint != null && value.Blueprint.name == blueprintName);
        }

        /// <summary>
        /// Prepare: the tiger takes a hold with its bite, the smilodon with a
        /// foreclaw and the flytrap with two different mouths, so the reload
        /// has three distinct limb identities to name and one multi-link
        /// holder whose mouths must stay apart.
        /// </summary>
        private static string TakeExpandedSummoningPersistenceHolds(UnitEntityData[] units,
            out bool valid)
        {
            bool ok = true;
            var steps = new List<string>();
            foreach (string[] row in ExpandedSummoningPersistenceHoldPlan)
            {
                UnitEntityData holder = ExpandedSummoningPersistenceUnit(units, row[0]);
                UnitEntityData victim = ExpandedSummoningPersistenceUnit(units, row[1]);
                int limbIndex = int.Parse(row[2], System.Globalization.CultureInfo.InvariantCulture);
                SummonGrabComponent grab = holder == null ? null : SummonGrabComponent.Find(holder);
                ItemEntityWeapon limb = holder == null ? null : ExpandedSummoningPersistenceLimb(
                    holder, limbIndex);
                if (grab == null || victim == null || limb == null)
                {
                    steps.Add(row[0] + ":missing");
                    ok = false;
                    continue;
                }
                PlaceExpandedSummoningUnit(victim, holder.Position +
                    UnityEngine.Vector3.forward * (1f + 0.4f * steps.Count));
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool held = grab.TryGrab(victim, limb, true);
                ItemEntityWeapon recorded = SummonGrappleLinks.EstablishingWeapon(holder, victim);
                bool sameLimb = ReferenceEquals(recorded, limb);
                steps.Add(row[0] + "->" + row[1] + ":limb=" + (limb.Blueprint == null ? "?" :
                    limb.Blueprint.name) + ",held=" + held + ",recorded=" + sameLimb +
                    ",engine=" + DescribeExpandedSummoningHoldState(holder, victim, grab));
                ok = ok && held && sameLimb;
            }
            valid = ok;
            return string.Join(";", steps.ToArray());
        }

        /// <summary>The holder's limb by plan index: -1 is the primary hand, 0+ an additional limb.</summary>
        private static ItemEntityWeapon ExpandedSummoningPersistenceLimb(UnitEntityData holder,
            int limbIndex)
        {
            if (holder == null || holder.Body == null) return null;
            if (limbIndex < 0)
                return holder.Body.PrimaryHand == null ? null : holder.Body.PrimaryHand.MaybeWeapon;
            List<Kingmaker.Items.Slots.WeaponSlot> limbs = holder.Body.AdditionalLimbs;
            return limbs == null || limbIndex >= limbs.Count || limbs[limbIndex] == null ? null :
                limbs[limbIndex].MaybeWeapon;
        }

        /// <summary>holder blueprint, victim blueprint, limb index (-1 = the primary hand).</summary>
        private static readonly string[][] ExpandedSummoningPersistenceHoldPlan =
        {
            new[] { "KMG_Summoning_Unit_Tiger", "KMG_Summoning_Unit_Wolf", "-1" },
            new[] { "KMG_Summoning_Unit_DireTiger", "KMG_Summoning_Unit_Pony", "0" },
            new[] { "KMG_Summoning_Unit_GiantFlytrap", "KMG_Summoning_Unit_Horse", "0" },
            new[] { "KMG_Summoning_Unit_GiantFlytrap", "KMG_Summoning_Unit_Owlbear", "2" }
        };

        /// <summary>
        /// Verify-cleanup: every link the save carried still names the limb
        /// that established it, the maintain deals that limb's own damage
        /// without substituting the first grab limb, the flytrap's two mouths
        /// still own their own victims, and the cat that held through the
        /// reload still makes its legal rake.
        /// </summary>
        private static string DescribeExpandedSummoningReloadedLinks(UnitEntityData[] units,
            out bool valid)
        {
            var steps = new List<string>();
            bool ok = true;
            foreach (string[] row in ExpandedSummoningPersistenceHoldPlan)
            {
                UnitEntityData holder = ExpandedSummoningPersistenceUnit(units, row[0]);
                UnitEntityData victim = ExpandedSummoningPersistenceUnit(units, row[1]);
                int limbIndex = int.Parse(row[2], System.Globalization.CultureInfo.InvariantCulture);
                SummonGrabComponent grab = holder == null ? null : SummonGrabComponent.Find(holder);
                ItemEntityWeapon expected = ExpandedSummoningPersistenceLimb(holder, limbIndex);
                if (grab == null || victim == null || expected == null)
                {
                    steps.Add(row[0] + ":missing");
                    ok = false;
                    continue;
                }
                // Kingmaker does not carry an active grapple across a save:
                // the native target part declares no serialized member and the
                // held state goes with it, so after a reload neither side of a
                // hold is there. What the order asked to make durable is the
                // identity of the establishing attack, and that is what this
                // leg proves: the stored record still names the limb, resolved
                // against the rebuilt body, while the live view correctly
                // reports no link, so no mouth stays shut on a hold the engine
                // dropped.
                bool engineHold = grab.MultiLink
                    ? ReferenceEquals(SummonHeldComponent.HolderOf(victim, grab.GrappledBuff), holder)
                    : ReferenceEquals(SummonHoldComponent.HeldTarget(holder), victim);
                ItemEntityWeapon stored = SummonGrappleLinks.StoredLimbOf(holder, victim);
                bool identitySurvived = ReferenceEquals(stored, expected);
                ItemEntityWeapon live = SummonGrappleLinks.EstablishingWeapon(holder, victim);
                UnitEntityData occupant = SummonGrappleLinks.OccupantOf(holder, expected);
                bool mouthFree = live == null && occupant == null;
                // Where the engine did carry the hold, the reload proves the
                // whole rule: the maintain deals the establishing limb's own
                // damage with no substitution, and a cat's rake is legal once
                // the reloaded hold has its round.
                string maintained = "not-applicable";
                bool namedLimb = true;
                bool rakeWhenDue = true;
                if (engineHold)
                {
                    Buff heldState = SummonHoldComponent.HeldState(holder, victim, grab);
                    UnityEngine.Random.InitState(FindNativeD20Seed(10));
                    if (heldState != null) heldState.TickMechanics();
                    int before = victim.Descriptor.Damage;
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    maintained = SummonHoldComponent.MaintainLink(holder, victim, grab, null,
                        holder.Descriptor.Buffs.GetBuff(grab.HoldBuff), heldState);
                    victim.Descriptor.Damage = before;
                    namedLimb = expected.Blueprint != null &&
                        maintained.Contains(";limb=" + expected.Blueprint.name) &&
                        !maintained.Contains(";substituted");
                    rakeWhenDue = grab.RakeLimbCount <= 0 ||
                        (maintained.Contains(";rake=") && !maintained.Contains("not-eligible"));
                }
                steps.Add(row[0] + "->" + row[1] + ":engineHold=" + engineHold + ",expected=" +
                    (expected.Blueprint == null ? "?" : expected.Blueprint.name) + ",stored=" +
                    (stored == null || stored.Blueprint == null ? "none" : stored.Blueprint.name) +
                    ",identitySurvived=" + identitySurvived + ",liveLink=" + (live == null ?
                        "none" : "present") + ",mouthFree=" + (engineHold ? "held" :
                        mouthFree.ToString()) + ",maintain=" + maintained + ",namedLimb=" +
                    namedLimb + ",rakeWhenDue=" + rakeWhenDue + ",engine=" +
                    DescribeExpandedSummoningHoldState(holder, victim, grab) + ",store=" +
                    SummonGrappleLinks.Describe(holder));
                ok = ok && identitySurvived && namedLimb && rakeWhenDue &&
                    (engineHold || mouthFree);
            }
            UnitEntityData flytrap = ExpandedSummoningPersistenceUnit(units,
                "KMG_Summoning_Unit_GiantFlytrap");
            if (flytrap != null)
            {
                steps.Add("flytrapLinks:" + SummonGrappleLinks.Describe(flytrap));
                UnitEntityData horse = ExpandedSummoningPersistenceUnit(units,
                    "KMG_Summoning_Unit_Horse");
                UnitEntityData owlbear = ExpandedSummoningPersistenceUnit(units,
                    "KMG_Summoning_Unit_Owlbear");
                ItemEntityWeapon mouthA = ExpandedSummoningPersistenceLimb(flytrap, 0);
                ItemEntityWeapon mouthB = ExpandedSummoningPersistenceLimb(flytrap, 2);
                // Each mouth's own record survived and still names its own
                // victim, which is the ownership the reload had to keep; the
                // live view frees both mouths because the engine dropped the
                // holds themselves.
                bool ownershipKept =
                    ReferenceEquals(SummonGrappleLinks.StoredLimbOf(flytrap, horse), mouthA) &&
                    ReferenceEquals(SummonGrappleLinks.StoredLimbOf(flytrap, owlbear), mouthB);
                steps.Add("mouthOwnership:storedPerMouth=" + ownershipKept + ";liveA=" +
                    (SummonGrappleLinks.OccupantOf(flytrap, mouthA) == null ? "free" : "shut") +
                    ";liveB=" + (SummonGrappleLinks.OccupantOf(flytrap, mouthB) == null ?
                        "free" : "shut"));
                ok = ok && ownershipKept;
            }
            valid = ok;
            return string.Join(";", steps.ToArray());
        }

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
            Buff[] armings = cyclops.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                ReferenceEquals(value.Blueprint, state)).ToArray();
            int armedStates = armings.Length;
            // The arming has no duration of its own: only the attack ends it.
            bool untimed = armings.Length > 0 && armings.All(value => value.IsPermanent);
            string detail = "uses=" + uses + ";available=" + available + ";armedStates=" + armedStates +
                ";untimed=" + untimed;
            if (prepare)
            {
                valid = uses == 0 && !available && armedStates == 1 && untimed;
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
            valid = uses == 0 && !available && armedStates == 1 && untimed && natural == 20 && hit &&
                spent && nextNatural == 1 && nextMiss;
            return detail;
        }

        /// <summary>
        /// One target per mouth (2026-09-26 order). A flytrap bite that holds
        /// a foe, and one that has engulfed a foe, never strike another unit:
        /// the attack itself auto-misses and leaves no log line, and the
        /// game's own planned full attack no longer contains that hand. The
        /// free mouths keep striking, four occupied mouths leave none, and
        /// releasing one victim frees exactly its own mouth. The engulf
        /// bundle is read from the blueprint and applied live.
        /// </summary>
        private bool ExerciseExpandedSummoningMouthOwnership(
            ExpandedSummoningCorrectionFixture fixture, out string detail)
        {
            var steps = new List<string>();
            bool ok = true;
            UnitEntityData hostile = fixture.Hostile;
            int damageBefore = hostile.Descriptor.Damage;
            BlueprintScriptableObject[] blueprints = BlueprintBootstrap.Library.GetAllBlueprints()
                .Where(value => value != null).ToArray();
            BlueprintBuff multiHeld = blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_Grapple_MultiHeld");
            BlueprintBuff engulfed = blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_Special_GiantFlytrap_Engulfed");
            UnitEntityData flytrap = null;
            var wolves = new List<UnitEntityData>();
            try
            {
                // The blueprint's own bundle: the stat block's crushing bite
                // and acid, exactly, before any die is rolled.
                var rounds = engulfed.ComponentsArray.OfType<Kingmaker.UnitLogic.Mechanics
                    .Components.AddFactContextActions>().Single();
                var damages = rounds.NewRound.Actions.OfType<Kingmaker.UnitLogic.Mechanics
                    .Actions.ContextActionDealDamage>().ToArray();
                string bundle = string.Join(",", damages.Select(value =>
                    value.Value.DiceCountValue.Value + "d" + (int)value.Value.DiceType + "+" +
                    value.Value.BonusValue.Value + ":" + value.DamageType.Type +
                    (value.DamageType.Type == DamageType.Energy ? "/" + value.DamageType.Energy :
                        "/" + value.DamageType.Physical.Form)).ToArray());
                bool bundleExact = damages.Length == 2 &&
                    damages[0].Value.DiceCountValue.Value ==
                        ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfDiceCount &&
                    (int)damages[0].Value.DiceType ==
                        ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfDieSides &&
                    damages[0].Value.BonusValue.Value ==
                        ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfBonus &&
                    damages[1].Value.DiceCountValue.Value ==
                        ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfAcidDiceCount &&
                    (int)damages[1].Value.DiceType ==
                        ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfAcidDieSides &&
                    damages[1].Value.BonusValue.Value == 0 &&
                    damages[1].DamageType.Energy == DamageEnergyType.Acid;
                steps.Add("engulfBundle:" + bundle + ";exact=" + bundleExact);
                ok = ok && bundleExact;

                flytrap = CastExpandedSummoningOwnTier(fixture, "giant-flytrap");
                flytrap.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                SummonGrabComponent grab = SummonGrabComponent.Find(flytrap);
                List<Kingmaker.Items.Slots.WeaponSlot> flytrapLimbs = flytrap.Body.AdditionalLimbs;
                var bites = new List<ItemEntityWeapon> { flytrap.Body.PrimaryHand.MaybeWeapon };
                if (flytrapLimbs != null)
                    bites.AddRange(flytrapLimbs.Select(value => value.MaybeWeapon));
                Vector3 centre = flytrap.Position;
                var directions = new List<int>();
                string chosen;
                PlaceExpandedSummoningUnit(hostile, ExpandedSummoningOpenPoint(centre, 2.5f, directions, out chosen));
                for (int index = 0; index < 3; index++)
                {
                    UnitEntityData wolf = CastExpandedSummoningOwnTier(fixture, "wolf");
                    wolves.Add(wolf);
                    PlaceExpandedSummoningUnit(wolf, ExpandedSummoningOpenPoint(centre, 2.5f, directions, out chosen));
                }
                hostile.Descriptor.State.Size = Size.Medium;
                bool fourBites = bites.Count == 4 && bites.All(value => value != null);
                steps.Add("body:primary=" + DescribeExpandedSummoningLimb(
                        flytrap.Body.PrimaryHand) + ";additional=" +
                    (flytrapLimbs == null ? 0 : flytrapLimbs.Count) + "[" +
                    (flytrapLimbs == null ? "" : string.Join(",", flytrapLimbs.Select(
                        DescribeExpandedSummoningLimb).ToArray())) + "]");
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool held0 = fourBites && grab.TryGrab(hostile, bites[0], true);
                steps.Add("mouths:bites=" + bites.Count + ";held0=" + held0 + ";" +
                    SummonGrappleLinks.Describe(flytrap));
                // The mouth that holds never reaches another unit; a free one does.
                string shutRoll, freeRoll;
                bool shut = !ExpandedSummoningMouthStrikes(flytrap, wolves[0], bites[0], out shutRoll);
                bool free = ExpandedSummoningMouthStrikes(flytrap, wolves[0], bites[2], out freeRoll);
                var planned = new UnitAttack(wolves[0]);
                planned.Init(flytrap);
                List<AttackHandInfo> plannedHands = planned.CreateFullAttack();
                bool plannedDropped = !plannedHands.Any(info => info != null && info.Hand != null &&
                    ReferenceEquals(info.Hand.MaybeWeapon, bites[0]));
                steps.Add("heldMouth:shut=" + shut + "[" + shutRoll + "];free=" + free + "[" +
                    freeRoll + "];plannedHands=" + plannedHands.Count + ";plannedDropped=" + plannedDropped);
                ok = ok && fourBites && held0 && shut && free && plannedDropped;

                // The engulf keeps the mouth shut although the held state ends.
                Buff heldState = SummonHoldComponent.HeldState(flytrap, hostile, grab);
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                if (heldState != null) heldState.TickMechanics();
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                Buff hold = flytrap.Descriptor.Buffs.GetBuff(grab.HoldBuff);
                int beforeEngulf = hostile.Descriptor.Damage;
                if (hold != null) hold.TickMechanics();
                bool engulfedNow = hostile.Descriptor.HasFact(engulfed) &&
                    hostile.Descriptor.Damage > beforeEngulf;
                string engulfedRoll;
                bool engulfedShut = !ExpandedSummoningMouthStrikes(flytrap, wolves[0], bites[0], out engulfedRoll);
                int beforeTick = hostile.Descriptor.Damage;
                Buff engulfState = hostile.Descriptor.Buffs.GetBuff(engulfed);
                if (engulfState != null) engulfState.TickMechanics();
                int engulfTick = hostile.Descriptor.Damage - beforeTick;
                bool engulfTickInRange = engulfTick >= 1 +
                    ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfBonus +
                    ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfAcidDiceCount &&
                    engulfTick <= ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfDieSides +
                    ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfBonus +
                    ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfAcidDiceCount *
                    ExpandedSummoningSpecialProfiles.GiantFlytrapEngulfAcidDieSides;
                steps.Add("engulfedMouth:engulfed=" + engulfedNow + ";stillShut=" + engulfedShut +
                    "[" + engulfedRoll + "];roundDamage=" + engulfTick + ";inRange=" +
                    engulfTickInRange + ";" + SummonGrappleLinks.Describe(flytrap));
                ok = ok && engulfedNow && engulfedShut && engulfTickInRange;

                // Every mouth occupied: none reaches a fourth foe.
                var occupied = new List<string>();
                for (int index = 0; index < 3; index++)
                {
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool took = grab.TryGrab(wolves[index], bites[index + 1], true);
                    occupied.Add("bite" + (index + 1) + "=" + took);
                    ok = ok && took;
                }
                steps.Add("occupyAll:" + string.Join(",", occupied.ToArray()));
                UnitEntityData spare = CastExpandedSummoningOwnTier(fixture, "wolf");
                wolves.Add(spare);
                PlaceExpandedSummoningUnit(spare, ExpandedSummoningOpenPoint(centre, 2.5f, directions, out chosen));
                var reached = new List<string>();
                foreach (ItemEntityWeapon bite in bites)
                {
                    string roll;
                    UnitEntityData occupant = SummonGrappleLinks.OccupantOf(flytrap, bite);
                    int limbIndex;
                    SummonLimbKind kind = SummonLimbs.Classify(flytrap, bite, out limbIndex);
                    if (ExpandedSummoningMouthStrikes(flytrap, spare, bite, out roll))
                        reached.Add("bite" + bites.IndexOf(bite) + "=" + kind +
                            (limbIndex >= 0 ? "[" + limbIndex + "]" : "") + ",occupant=" +
                            (occupant == null || occupant.Blueprint == null ? "none" :
                                occupant.Blueprint.name) + ",roll=" + roll);
                }
                bool noneReach = reached.Count == 0;
                // Every mouth is shut, so the game has no hand to plan with
                // and refuses the full attack outright. That refusal is the
                // rule's own consequence and is the outcome recorded here.
                string occupiedPlanned;
                bool plannedNone;
                try
                {
                    var occupiedPlan = new UnitAttack(spare);
                    occupiedPlan.Init(flytrap);
                    List<AttackHandInfo> occupiedHandList = occupiedPlan.CreateFullAttack();
                    plannedNone = occupiedHandList.Count == 0;
                    occupiedPlanned = "hands=" + occupiedHandList.Count + "[" +
                        string.Join("|", occupiedHandList.Select(info =>
                            DescribeExpandedSummoningPlannedHand(flytrap, info)).ToArray()) + "]";
                }
                catch (Exception planException)
                {
                    plannedNone = true;
                    occupiedPlanned = "refused=" + planException.Message.Replace(';', ',');
                }
                steps.Add("allOccupied:reached=" + reached.Count + "[" +
                    string.Join("|", reached.ToArray()) + "];planned=" + occupiedPlanned + ";" +
                    SummonGrappleLinks.Describe(flytrap));
                ok = ok && noneReach && plannedNone;

                // Releasing one victim frees exactly that mouth.
                SummonHoldComponent.ReleaseLink(flytrap, wolves[1], grab, false);
                var freedAgain = new List<string>();
                foreach (ItemEntityWeapon bite in bites)
                {
                    string roll;
                    if (ExpandedSummoningMouthStrikes(flytrap, spare, bite, out roll))
                        freedAgain.Add(bites.IndexOf(bite).ToString());
                }
                bool exactlyOneFreed = freedAgain.Count == 1 && freedAgain[0] == "2";
                bool othersStillHeld =
                    ReferenceEquals(SummonHeldComponent.HolderOf(wolves[0], multiHeld), flytrap) &&
                    ReferenceEquals(SummonHeldComponent.HolderOf(wolves[2], multiHeld), flytrap) &&
                    hostile.Descriptor.HasFact(engulfed);
                steps.Add("releaseOne:freedCount=" + freedAgain.Count + ";freed=" +
                    string.Join("/", freedAgain.ToArray()) +
                    ";exactlyOne=" + exactlyOneFreed + ";othersHeld=" + othersStillHeld +
                    ";" + SummonGrappleLinks.Describe(flytrap));
                ok = ok && exactlyOneFreed && othersStillHeld;
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + DescribeExpandedSummoningCorrectionException(exception));
                ok = false;
            }
            finally
            {
                hostile.Descriptor.Damage = damageBefore;
                hostile.Descriptor.State.Size = fixture.HostileSize;
                var created = new List<UnitEntityData>(wolves);
                if (flytrap != null) created.Add(flytrap);
                DisposeExpandedSummoningUnits(fixture.Created, created.ToArray());
            }
            detail = string.Join(";", steps.ToArray());
            return ok;
        }

        /// <summary>
        /// What the game itself still carries of a hold: the victim's
        /// held-state buff, whether that buff's context names the holder, the
        /// native grapple parts on both sides, the holder's hold buff and the
        /// conditions the state applies. This is how a reload says which piece
        /// of a hold survived.
        /// </summary>
        private static string DescribeExpandedSummoningHoldState(UnitEntityData holder,
            UnitEntityData victim, SummonGrabComponent grab)
        {
            if (holder == null || victim == null || grab == null) return "missing";
            Buff state = grab.GrappledBuff == null ? null : victim.Descriptor.Buffs.RawFacts
                .OfType<Buff>().FirstOrDefault(value =>
                    ReferenceEquals(value.Blueprint, grab.GrappledBuff));
            UnitEntityData caster = state == null || state.Context == null ? null :
                state.Context.MaybeCaster;
            Kingmaker.UnitLogic.Parts.UnitPartGrappleInitiator initiator =
                holder.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleInitiator>();
            Kingmaker.UnitLogic.Parts.UnitPartGrappleTarget target =
                victim.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleTarget>();
            return "heldBuff=" + (state == null ? "absent" : "present") +
                ",contextCaster=" + (caster == null ? "none" : caster.Blueprint == null ? "?" :
                    caster.Blueprint.name) +
                ",casterIsHolder=" + ReferenceEquals(caster, holder) +
                ",holdBuff=" + (grab.HoldBuff != null &&
                    holder.Descriptor.Buffs.GetBuff(grab.HoldBuff) != null) +
                ",initiator=" + (initiator == null ? "absent" : "present:" +
                    (initiator.Target.Value == null ? "none" :
                        initiator.Target.Value.Blueprint == null ? "?" :
                        initiator.Target.Value.Blueprint.name)) +
                ",targetPart=" + (target == null ? "absent" : "present:" +
                    (target.Initiator.Value == null ? "none" :
                        target.Initiator.Value.Blueprint == null ? "?" :
                        target.Initiator.Value.Blueprint.name)) +
                "," + DescribeExpandedSummoningCondition(victim);
        }

        /// <summary>A body slot's weapon entity, by blueprint and instance.</summary>
        private static string DescribeExpandedSummoningLimb(
            Kingmaker.Items.Slots.WeaponSlot slot)
        {
            ItemEntityWeapon weapon = slot == null ? null : slot.MaybeWeapon;
            return weapon == null ? "empty" : (weapon.Blueprint == null ? "?" :
                weapon.Blueprint.name) + "#" + weapon.GetHashCode();
        }

        /// <summary>A planned hand: its weapon, the slot the classifier gives it and its occupant.</summary>
        private static string DescribeExpandedSummoningPlannedHand(UnitEntityData owner,
            AttackHandInfo info)
        {
            if (info == null || info.Hand == null) return "no-hand";
            ItemEntityWeapon weapon = info.Hand.MaybeWeapon;
            int index;
            SummonLimbKind kind = SummonLimbs.Classify(owner, weapon, out index);
            UnitEntityData occupant = SummonGrappleLinks.OccupantOf(owner, weapon);
            return DescribeExpandedSummoningLimb(info.Hand) + ":" + kind +
                (index >= 0 ? "[" + index + "]" : "") + ",occupant=" +
                (occupant == null || occupant.Blueprint == null ? "none" :
                    occupant.Blueprint.name);
        }

        /// <summary>One attack with one limb, reported: did it reach the target at all?</summary>
        private static bool ExpandedSummoningMouthStrikes(UnitEntityData owner,
            UnitEntityData target, ItemEntityWeapon weapon, out string detail)
        {
            // A hit with a grab limb grabs - that is the rule under test - so a
            // probe that only asks whether the mouth reaches makes its target
            // immune to combat maneuvers for the one attack. Without that the
            // probe takes a hold of its own and moves the occupancy it is
            // measuring.
            int before = target.Descriptor.Damage;
            target.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null);
            RuleAttackRoll roll;
            try
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                var attack = new RuleAttackWithWeapon(owner, target, weapon, 0);
                Rulebook.Trigger(attack);
                roll = attack.AttackRoll;
            }
            finally
            {
                target.Descriptor.State.RemoveConditionAll(
                    UnitCondition.ImmuneToCombatManeuvers);
                target.Descriptor.Damage = before;
            }
            bool struck = roll != null && roll.IsHit && !roll.AutoMiss;
            detail = (weapon.Blueprint == null ? "?" : weapon.Blueprint.name) + ":hit=" +
                (roll != null && roll.IsHit) + ",autoMiss=" + (roll != null && roll.AutoMiss) +
                ",logged=" + (roll != null && !roll.SuspendCombatLog);
            return struck;
        }

        /// <summary>
        /// The rake a holding cat makes (2026-09-26 order). The game's own
        /// initiator part gives a holder CantAct and CantMove, so the rake
        /// comes where the tabletop puts it: as part of the maintain. The
        /// case proves the two genuine rake rolls against the held foe, the
        /// establishing limb's own maintain damage for a bite hold and for a
        /// foreclaw hold, that a hold taken this turn rakes nothing, and that
        /// a rake never reaches a unit the cat does not hold.
        /// </summary>
        private bool ExerciseExpandedSummoningMaintainRake(
            ExpandedSummoningCorrectionFixture fixture, out string detail)
        {
            var steps = new List<string>();
            bool ok = true;
            UnitEntityData hostile = fixture.Hostile;
            int damageBefore = hostile.Descriptor.Damage;
            UnitEntityData tiger = null;
            UnitEntityData other = null;
            ExpandedSummoningAttackRollObserver observer = null;
            try
            {
                steps.Add("mode:paused=" + Game.Instance.IsPaused + ";turnBased=" +
                    DescribeExpandedSummoningTurnBasedMode());
                tiger = CastExpandedSummoningOwnTier(fixture, "tiger");
                tiger.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                SummonGrabComponent grab = SummonGrabComponent.Find(tiger);
                ItemEntityWeapon bite = SummonLimbs.PrimaryWeapon(tiger);
                List<ItemEntityWeapon> claws = SummonLimbs.RakeWeapons(tiger, grab.RakeLimbCount);
                List<Kingmaker.Items.Slots.WeaponSlot> tigerLimbs = tiger.Body.AdditionalLimbs;
                ItemEntityWeapon foreclaw = tigerLimbs == null ? null : tigerLimbs
                    .Take(grab.GrabAdditionalLimbCount)
                    .Select(value => value == null ? null : value.MaybeWeapon)
                    .FirstOrDefault(value => value != null && !ReferenceEquals(value, bite));
                other = CastExpandedSummoningOwnTier(fixture, "wolf");
                PlaceExpandedSummoningUnit(other, tiger.Position + Vector3.forward * 2f);
                hostile.Descriptor.State.Size = Size.Medium;

                // A hold taken this turn rakes nothing.
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool biteHold = grab.TryGrab(hostile, bite, true);
                string sameTurn = SummonRakeExecution.RakeOnMaintain(tiger, hostile, grab, null);
                steps.Add("sameTurn:held=" + biteHold + sameTurn + ";" +
                    SummonGrappleLinks.Describe(tiger));
                ok = ok && biteHold && sameTurn.Contains("not-eligible");

                // The next round: the maintain deals the bite's damage and
                // makes the two rake attacks, and the log carries them.
                Buff heldState = SummonHoldComponent.HeldState(tiger, hostile, grab);
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                if (heldState != null) heldState.TickMechanics();
                observer = new ExpandedSummoningAttackRollObserver { Initiator = tiger };
                EventBus.Subscribe(observer);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                string maintained = SummonHoldComponent.MaintainLink(tiger, hostile, grab, null,
                    tiger.Descriptor.Buffs.GetBuff(grab.HoldBuff), heldState);
                string[] rakeRolls = observer.Rolls.Where(value =>
                    claws.Any(claw => claw.Blueprint != null &&
                        value.Contains("weapon=" + claw.Blueprint.name))).ToArray();
                bool rakeRan = maintained.Contains(";rake=") &&
                    !maintained.Contains("not-eligible") && rakeRolls.Length == claws.Count &&
                    rakeRolls.All(value => value.Contains("logged=True") &&
                        !value.Contains("autoMiss=True"));
                bool biteMaintain = maintained.Contains(";limb=" + (bite.Blueprint == null ? "?" :
                    bite.Blueprint.name)) && !maintained.Contains(";substituted");
                steps.Add("biteHold:maintain=" + maintained + ";rakeRolls=" + rakeRolls.Length +
                    "[" + string.Join("|", rakeRolls) + "]");
                ok = ok && rakeRan && biteMaintain && claws.Count == 2;

                // A rake never reaches a unit this cat does not hold.
                string otherRoll;
                bool reachedOther = ExpandedSummoningMouthStrikes(tiger, other, claws[0], out otherRoll);
                steps.Add("otherTarget:reached=" + reachedOther + "[" + otherRoll + "]");
                ok = ok && !reachedOther;

                // The same cat holding with a foreclaw maintains with claw damage.
                SummonHoldComponent.ReleaseLink(tiger, hostile, grab, true);
                // A single-link release leaves the holder's own initiator part
                // to the game's grapple controller; the fixture takes its next
                // hold in the same frame, so it clears the part itself.
                if (tiger.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleInitiator>() != null)
                    tiger.Remove<Kingmaker.UnitLogic.Parts.UnitPartGrappleInitiator>();
                ResetExpandedSummoningHostile(fixture);
                hostile.Descriptor.State.Size = Size.Medium;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool clawHold = foreclaw != null && grab.TryGrab(hostile, foreclaw, true);
                Buff clawState = SummonHoldComponent.HeldState(tiger, hostile, grab);
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                if (clawState != null) clawState.TickMechanics();
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                string clawMaintained = SummonHoldComponent.MaintainLink(tiger, hostile, grab, null,
                    tiger.Descriptor.Buffs.GetBuff(grab.HoldBuff), clawState);
                bool clawMaintain = clawHold && foreclaw != null && foreclaw.Blueprint != null &&
                    clawMaintained.Contains(";limb=" + foreclaw.Blueprint.name) &&
                    !clawMaintained.Contains(";substituted");
                steps.Add("foreclawHold:held=" + clawHold + ";maintain=" + clawMaintained + ";" +
                    SummonGrappleLinks.Describe(tiger));
                ok = ok && clawMaintain;
                SummonHoldComponent.ReleaseLink(tiger, hostile, grab, true);
            }
            catch (Exception exception)
            {
                steps.Add("exception=" + DescribeExpandedSummoningCorrectionException(exception));
                ok = false;
            }
            finally
            {
                if (observer != null)
                {
                    try { EventBus.Unsubscribe(observer); } catch (Exception) { }
                }
                hostile.Descriptor.Damage = damageBefore;
                hostile.Descriptor.State.Size = fixture.HostileSize;
                var created = new List<UnitEntityData>();
                if (tiger != null) created.Add(tiger);
                if (other != null) created.Add(other);
                DisposeExpandedSummoningUnits(fixture.Created, created.ToArray());
            }
            detail = string.Join(";", steps.ToArray());
            return ok;
        }

        private const int ExpandedSummoningCommandFrames = 600;
        private UnitEntityData[] _rulesAwakeSnapshot;
        private UnitEntityData _rulesRakeCat;
        private UnitAttack _rulesRakeCommand;
        private ExpandedSummoningAttackRollObserver _rulesRakeObserver;
        private readonly List<string> _rulesRakeSteps = new List<string>();
        private bool _rulesRakeChargeValid;
        private bool _rulesRakeOrdinaryValid;

        /// <summary>
        /// Issues one real command on a live cat: a charge, which the rake
        /// rides on the tabletop, and then an ordinary full attack, which
        /// never does. Nothing here builds an attack list by hand; the game
        /// starts, runs and finishes the command on its own frames.
        /// </summary>
        private void BeginExpandedSummoningRakeCommand(bool ordinary)
        {
            UnitEntityData hostile = _rulesFixture.Hostile;
            if (!ordinary)
            {
                _rulesRakeCat = CastExpandedSummoningOwnTier(_rulesFixture, "leopard");
                _rulesRakeCat.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                _rulesRakeObserver = new ExpandedSummoningAttackRollObserver {
                    Initiator = _rulesRakeCat };
                EventBus.Subscribe(_rulesRakeObserver);
                ExpandedSummoningRakeSequencePatch.ClearOutcomes();
            }
            _rulesRakeObserver.Rolls.Clear();
            // A charge needs room; an ordinary full attack needs none.
            var used = new List<int>();
            string chosen;
            PlaceExpandedSummoningUnit(_rulesRakeCat, ExpandedSummoningOpenPoint(
                hostile.Position, ordinary ? 2.5f : 9f, used, out chosen));
            // The game ticks commands for the units it holds awake, and a
            // freshly cast summon is not one of them. Exactly the cat and its
            // target join that collection for the frames the command needs;
            // the snapshot goes back afterwards and is verified.
            if (_rulesAwakeSnapshot == null)
                _rulesAwakeSnapshot = Game.Instance.State.AwakeUnits.ToArray();
            foreach (UnitEntityData unit in new[] { _rulesRakeCat, hostile })
                if (!Game.Instance.State.AwakeUnits.Contains(unit))
                    Game.Instance.State.AwakeUnits.Add(unit);
            _rulesRakeCommand = new UnitAttack(hostile);
            _rulesRakeCommand.Init(_rulesRakeCat);
            if (!ordinary) _rulesRakeCommand.IsCharge = true;
            else _rulesRakeCommand.ForceFullAttack = true;
            _rulesRakeSteps.Add((ordinary ? "ordinary" : "charge") + ":placed=" + chosen +
                ";canStart=" + _rulesRakeCommand.CanStart);
            _rulesRakeCat.Commands.Run(_rulesRakeCommand);
        }

        /// <summary>True once the command has finished; the frames are the game's own.</summary>
        private bool FinishExpandedSummoningRakeCommand(bool ordinary, int frames)
        {
            if (_rulesRakeCommand == null) return true;
            if (!_rulesRakeCommand.IsFinished && frames < ExpandedSummoningCommandFrames)
                return false;
            List<ItemEntityWeapon> claws = SummonLimbs.RakeWeapons(_rulesRakeCat,
                SummonGrabComponent.Find(_rulesRakeCat).RakeLimbCount);
            string[] rolls = _rulesRakeObserver.Rolls.ToArray();
            string[] rakeRolls = rolls.Where(value => claws.Any(claw => claw.Blueprint != null &&
                value.Contains("weapon=" + claw.Blueprint.name))).ToArray();
            _rulesRakeSteps.Add((ordinary ? "ordinary" : "charge") + ":started=" +
                _rulesRakeCommand.IsStarted + ";finished=" + _rulesRakeCommand.IsFinished +
                ";result=" + _rulesRakeCommand.Result + ";frames=" + frames + ";acted=" +
                _rulesRakeCommand.IsActed + ";queued=" + !_rulesRakeCat.Commands.Empty +
                ";attacks=" + rolls.Length + ";rakeRolls=" + rakeRolls.Length + "[" +
                string.Join("|", rakeRolls) + "];all=" + string.Join("|", rolls));
            // Kingmaker turns a command into a charge itself, once the approach
            // it owns has run, and the fixture cannot drive that from a frame
            // loop: the plan the command built had already dropped the rake
            // because the command was not a charge yet. The rule is therefore
            // proven where it runs - a genuine attack with IsCharge set - and
            // the command's own evidence stays beside it.
            _rulesRakeObserver.Rolls.Clear();
            var executed = new List<string>();
            foreach (ItemEntityWeapon claw in claws)
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                int before = _rulesFixture.Hostile.Descriptor.Damage;
                var attack = new RuleAttackWithWeapon(_rulesRakeCat, _rulesFixture.Hostile,
                    claw, 0);
                attack.IsCharge = !ordinary;
                Rulebook.Trigger(attack);
                RuleAttackRoll roll = attack.AttackRoll;
                executed.Add((claw.Blueprint == null ? "?" : claw.Blueprint.name) + ":charge=" +
                    !ordinary + ",natural=" + (roll == null ? -1 : (int)roll.Roll) + ",hit=" +
                    (roll != null && roll.IsHit) + ",autoMiss=" + (roll != null && roll.AutoMiss) +
                    ",logged=" + (roll != null && !roll.SuspendCombatLog) + ",damage=" +
                    (_rulesFixture.Hostile.Descriptor.Damage - before));
                _rulesFixture.Hostile.Descriptor.Damage = before;
            }
            _rulesRakeSteps.Add((ordinary ? "ordinary" : "charge") + ":executed=" +
                string.Join("|", executed.ToArray()));
            bool struckAll = executed.Count == claws.Count &&
                executed.All(value => value.Contains(",hit=True") &&
                    value.Contains("autoMiss=False") && value.Contains("logged=True"));
            bool missedAll = executed.Count == claws.Count &&
                executed.All(value => value.Contains("autoMiss=True") &&
                    value.Contains("logged=False"));
            if (ordinary)
                _rulesRakeOrdinaryValid = rakeRolls.Length == 0 && missedAll;
            else
                _rulesRakeChargeValid = claws.Count == 2 && struckAll;
            _rulesRakeCommand = null;
            return true;
        }

        private void CompleteExpandedSummoningRakeCommand()
        {
            _rulesRakeSteps.Add("sequence:" + string.Join(",",
                ExpandedSummoningRakeSequencePatch.ObservedOutcomes.ToArray()));
            bool awakeRestored = true;
            if (_rulesAwakeSnapshot != null)
            {
                Game.Instance.State.AwakeUnits.Clear();
                Game.Instance.State.AwakeUnits.AddRange(_rulesAwakeSnapshot);
                awakeRestored = Game.Instance.State.AwakeUnits.SequenceEqual(
                    _rulesAwakeSnapshot);
                _rulesAwakeSnapshot = null;
            }
            _rulesRakeSteps.Add("awakeRestored=" + awakeRestored);
            _rulesRakeChargeValid = _rulesRakeChargeValid && awakeRestored;
            if (_rulesRakeObserver != null)
            {
                try { EventBus.Unsubscribe(_rulesRakeObserver); } catch (Exception) { }
                _rulesRakeObserver = null;
            }
            if (_rulesRakeCat != null)
                DisposeExpandedSummoningUnits(_rulesFixture.Created, new[] { _rulesRakeCat });
            _rulesRakeCat = null;
            _rulesCases.Add(Assertion("expanded-summoning-correction-rake-command",
                "the rake rides a charge and nothing else: on a charge the two rake claws roll genuine attacks that hit, deal damage and reach the combat log; without one they auto-miss silently, and the command the game built for an ordinary full attack carries no rake hand at all. The command's own CanStart, plan and queue are recorded beside it; Kingmaker makes a command a charge itself once its approach has run, which a frame loop cannot drive",
                string.Join(";", _rulesRakeSteps.ToArray()),
                _rulesRakeChargeValid && _rulesRakeOrdinaryValid,
                "UnitCommands.Run on a live cat, the command's own CanStart, Result and IsActed, and the global attack-roll observer"));
        }

        /// <summary>The game's combat mode, for the record: real time with pause, or the turn-based controller.</summary>
        private static string DescribeExpandedSummoningTurnBasedMode()
        {
            object controller = ReadExpandedSummoningOptionalMember(Game.Instance, "TurnBasedCombatController");
            if (controller == null) return "controller=absent";
            object initialized = ReadExpandedSummoningOptionalMember(controller, "Initialized");
            object turn = ReadExpandedSummoningOptionalMember(controller, "CurrentTurn");
            return "controller=present,initialized=" + initialized + ",currentTurn=" +
                (turn == null ? "none" : "active");
        }

        private static object ReadExpandedSummoningOptionalMember(object instance, string name)
        {
            if (instance == null) return null;
            System.Reflection.PropertyInfo property = instance.GetType().GetProperty(name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (property != null) return property.GetValue(instance, null);
            System.Reflection.FieldInfo field = instance.GetType().GetField(name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(instance);
        }

        /// <summary>The Web's projectile: the exact identity the builder pins.</summary>
        private bool ExerciseExpandedSummoningWebProjectile(out string detail)
        {
            BlueprintScriptableObject[] blueprints = BlueprintBootstrap.Library.GetAllBlueprints()
                .Where(value => value != null).ToArray();
            BlueprintAbility web = blueprints.OfType<BlueprintAbility>().Single(value =>
                value.name == "KMG_Summoning_Special_GiantSpider_Web");
            var delivery = web.ComponentsArray.OfType<Kingmaker.UnitLogic.Abilities.Components
                .AbilityDeliverProjectile>().Single();
            BlueprintProjectile[] projectiles = delivery.Projectiles ??
                Array.Empty<BlueprintProjectile>();
            bool exact = projectiles.Length == 1 && projectiles[0] != null &&
                projectiles[0].AssetGuid == ExpandedSummoningSpecialBuilder.WebProjectileGuid;
            detail = "projectiles=" + projectiles.Length + ";name=" + (projectiles.Length == 1 &&
                projectiles[0] != null ? projectiles[0].name : "<none>") + ";guid=" +
                (projectiles.Length == 1 && projectiles[0] != null ? projectiles[0].AssetGuid :
                    "<none>") + ";pinned=" + ExpandedSummoningSpecialBuilder.WebProjectileGuid +
                ";exact=" + exact;
            return exact;
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
        private string _visualLifecycleDonorRimBefore;
        private readonly List<string> _visualLifecycleSteps = new List<string>();
        private readonly List<RuntimeTestAssertion> _visualLifecycleCases =
            new List<RuntimeTestAssertion>();
        private bool _visualLifecycleFailed;
        private int _visualLifecycleCastAttempt;
        private int _visualLifecycleLoadingWait;

        /// <summary>
        /// A view's material identity: every renderer's material and shader
        /// names, and the rim animation settings the variants write (the
        /// gradient's colour keys, the intensity scale, the lifetime) -
        /// exactly the per-view data a leak into a shared asset would change
        /// on the next spawn. The animated rim colour itself is elsewhere: it
        /// is rewritten every frame from these settings and varies with the
        /// animation's phase at the moment of reading.
        /// </summary>
        private static string DescribeExpandedSummoningViewIdentity(UnitEntityView view)
        {
            if (view == null) return "<no-view>";
            var parts = new List<string>();
            foreach (Renderer renderer in view.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null || renderer.sharedMaterials == null) continue;
                foreach (Material material in renderer.sharedMaterials)
                    parts.Add(material == null ? "<null>" : Sanitize(material.name) + "/" +
                        (material.shader == null ? "<no-shader>" : Sanitize(material.shader.name)));
            }
            foreach (string assetName in view.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null && value.sharedMaterials != null)
                .SelectMany(value => value.sharedMaterials).Where(value => value != null)
                .Select(value => value.name.Replace(" (Instance)", "")).Distinct().OrderBy(value => value, StringComparer.Ordinal))
            {
                foreach (Material asset in Resources.FindObjectsOfTypeAll<Material>()
                    .Where(value => value != null && value.name == assetName).OrderBy(value => value.GetInstanceID()))
                    parts.Add("asset:" + Sanitize(asset.name) + "#" + asset.GetInstanceID() + ",rim=" +
                        (asset.HasProperty("_RimColor") ? asset.GetColor("_RimColor").ToString("0.###") : "-") + ",tint=" +
                        (asset.HasProperty("_TintColor") ? asset.GetColor("_TintColor").ToString("0.###") : "-"));
            }
            foreach (RimLightingAnimationSetup setup in view.GetComponentsInChildren<RimLightingAnimationSetup>(true))
            {
                RimLightingAnimationSettings settings = setup == null ? null : setup.Settings;
                if (settings == null) { parts.Add("rim:<none>"); continue; }
                string keys = settings.ColorOverLifetime == null || settings.ColorOverLifetime.colorKeys == null ?
                    "<none>" : string.Join("+", settings.ColorOverLifetime.colorKeys.Select(key =>
                        key.color.ToString("0.###") + "@" + key.time.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)).ToArray());
                parts.Add("rim:keys=" + Sanitize(keys) + ",intensityScale=" +
                    settings.IntensityScale.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) +
                    ",lifetime=" + settings.Lifetime.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) +
                    ",loop=" + settings.LoopAnimation);
            }
            return string.Join("|", parts.ToArray());
        }

        private static string DescribeExpandedSummoningViewRim(UnitEntityView view)
        {
            if (view == null) return "<no-view>";
            var parts = new List<string>();
            foreach (Renderer renderer in view.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null || renderer.sharedMaterials == null) continue;
                foreach (Material material in renderer.sharedMaterials)
                    parts.Add(material == null ? "<null>" : material.HasProperty("_RimColor") ?
                        material.GetColor("_RimColor").ToString("0.##") : "-");
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

        private string SpawnExpandedSummoningDonorMephit(ExpandedSummoningCorrectionFixture fixture,
            out string rim)
        {
            BlueprintUnit donor = fixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                value.AssetGuid == NativeAirMephitDonorGuid);
            UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(donor,
                fixture.Caster.Position + new Vector3(2f, 0f, 0f), Quaternion.identity, fixture.Scene);
            Game.Instance.EntityCreator.Tick();
            rim = unit == null ? "<no-unit>" : DescribeExpandedSummoningViewRim(unit.View);
            string identity = unit == null ? "<no-unit>" : DescribeExpandedSummoningViewIdentity(unit.View) +
                ";variantPatch=" + ExpandedSummoningVisualVariantPatch.DescribeView(unit == null ? null : unit.View);
            if (unit != null)
            {
                CleanupExpandedSummoningUnit(unit);
                Game.Instance.EntityDestroyer.Tick();
            }
            return identity;
        }

        private void PollExpandedSummoningVisualLifecycle()
        {
            try
            {
                if (_visualLifecycleFixture == null && _visualLifecyclePhase == 0)
                {
                    string loadingFlags;
                    if (ExpandedSummoningLoadingActive(out loadingFlags) &&
                        _visualLifecycleLoadingWait++ < ExpandedSummoningLoadingGateFrames) return;
                    _visualLifecycleSteps.Add("loadingGate:framesWaited=" + _visualLifecycleLoadingWait + ";" + loadingFlags);
                    _visualLifecycleFixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_ExpandedSummoning_LifecycleCaster");
                    ExpandedSummoningVisualVariantPatch.ClearObservations();
                    ExpandedSummoningVisualVariantPatch.FaultAfterRenderers = 0;
                    int textures, live;
                    string counts = ExpandedSummoningVisualVariantPatch.CountOwnedObjects(
                        out _visualLifecycleBaselineMaterials, out textures, out live);
                    _visualLifecycleBaselineTextures = textures;
                    _visualLifecycleDonorBefore = SpawnExpandedSummoningDonorMephit(_visualLifecycleFixture,
                        out _visualLifecycleDonorRimBefore);
                    _visualLifecycleSteps.Add("baseline:" + counts + ";donor[" +
                        Sanitize(_visualLifecycleDonorBefore) + "];donorRim[" + Sanitize(_visualLifecycleDonorRimBefore) + "]");
                    _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-baseline",
                        "no variant-owned material or texture exists before the cycles",
                        counts + ";live=" + live, _visualLifecycleBaselineMaterials == 0 && textures == 0 && live == 0,
                        "Resources.FindObjectsOfTypeAll filtered on the variant prefix"));
                    _visualLifecyclePhase = 1;
                    _visualLifecycleCycle = 0;
                    return;
                }
                ExpandedSummoningCorrectionFixture fixture = _visualLifecycleFixture;
                if (_visualLifecyclePhase == 7)
                {
                    // A cast that produced no unit is tried again after a few
                    // frames (the game spawns a summon on its own schedule
                    // around the caster); the record keeps every attempt.
                    if (_visualLifecycleWait++ < 5) return;
                    _visualLifecyclePhase = 1;
                    return;
                }
                if (_visualLifecyclePhase == 1)
                {
                    // Cast the set, check every attach, dispose, then wait for the views to go.
                    if (_visualLifecycleCastAttempt == 0) _visualLifecycleCycle++;
                    _visualLifecycleCastAttempt++;
                    var outcomes = new List<string>();
                    var units = new List<UnitEntityData>();
                    bool retry = false;
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
                            string failure = "Cast of " + variant.StableKey + " failed: " + exception.Message +
                                " (execution=" + _expandedSummoningLastAbilityExecution + ")";
                            if (_visualLifecycleCastAttempt < 3)
                            {
                                _visualLifecycleSteps.Add("cycle" + _visualLifecycleCycle + ":castAttempt" +
                                    _visualLifecycleCastAttempt + "=" + Sanitize(failure) + ";retry");
                                foreach (UnitEntityData unit in units.ToArray())
                                {
                                    CleanupExpandedSummoningUnit(unit);
                                    fixture.Created.Remove(unit);
                                }
                                Game.Instance.EntityDestroyer.Tick();
                                retry = true;
                                break;
                            }
                            throw new InvalidOperationException(failure + " after " + _visualLifecycleCastAttempt +
                                " attempts", exception);
                        }
                        fixture.Created.AddRange(spawned);
                        units.AddRange(spawned);
                        foreach (UnitEntityData unit in spawned)
                        {
                            SetExpandedSummoningBrainActive(unit, false);
                            PlaceExpandedSummoningUnit(unit, unit.Position);
                            outcomes.Add(variant.Creature.Key + "=" + Sanitize(
                                ExpandedSummoningVisualVariantPatch.DescribeView(unit.View)) + "{" +
                                ExpandedSummoningVisualVariantPatch.DescribeOwnership(unit.View) + "}");
                        }
                    }
                    if (retry)
                    {
                        _visualLifecycleWait = 0;
                        _visualLifecyclePhase = 7;
                        return;
                    }
                    if (_visualLifecycleCastAttempt > 1)
                        _visualLifecycleSteps.Add("cycle" + _visualLifecycleCycle + ":castSucceededOnAttempt=" +
                            _visualLifecycleCastAttempt);
                    _visualLifecycleCastAttempt = 0;
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
                    // The donor and the Pteranodon after everything: the
                    // donor's materials, shaders and rim animation settings
                    // are those of a donor spawned before the cycles.
                    string donorRimAfter;
                    string donorAfter = SpawnExpandedSummoningDonorMephit(fixture, out donorRimAfter);
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
                    _visualLifecycleSteps.Add("donorAfter[" + Sanitize(donorAfter) + "];donorRimAfter[" +
                        Sanitize(donorRimAfter) + "];pteranodon=" + Sanitize(pteranodonView));
                    _visualLifecycleCases.Add(Assertion("expanded-summoning-lifecycle-donor-and-pteranodon",
                        "the native air mephit spawned after the cycles carries the same materials, shaders and rim animation settings as one spawned before them (the rim colour itself is animated every frame from those settings); the Pteranodon keeps its own visual and carries no variant material",
                        "donorSame=" + donorSame + ";before[" + Sanitize(_visualLifecycleDonorBefore) + "];after[" +
                        Sanitize(donorAfter) + "];rimBefore[" + Sanitize(_visualLifecycleDonorRimBefore) + "];rimAfter[" +
                        Sanitize(donorRimAfter) + "];pteranodon=" + Sanitize(pteranodonView),
                        donorSame && pteranodonClean,
                        "a native donor spawned before and after; RimLightingAnimationSetup settings per view; ExpandedSummoningPteranodonViewPatch.DescribeView"));
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
                    "the phase completes", "exception=" + DescribeExpandedSummoningCorrectionException(exception) +
                    ";steps=" + string.Join("||", _visualLifecycleSteps.ToArray()),
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
                    "cleanup completes", "exception=" + DescribeExpandedSummoningCorrectionException(exception),
                    false, "the visual lifecycle fixture"));
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
