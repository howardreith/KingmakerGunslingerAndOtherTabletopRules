using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string IceDevilSavedScope40 =
            "KMG_Sprint40_IceDevil_v1";
        private Buff _iceDevilLoadedSlow40;
        private UnitEntityData _iceDevilLoadedWall40;

        private bool Sprint40PersistenceScope
        {
            get
            {
                return _request != null && _request.Parameters != null &&
                    (string)_request.Parameters["persistenceScope"] ==
                        ExpandedSummoningRosterPersistencePolicy
                            .IceDevilScope;
            }
        }

        private static T IceDevilSavedBlueprint40<T>(string suffix)
            where T : BlueprintScriptableObject
        {
            return (T)ExpandedSummoningBlueprints.Registered.BySymbol[
                ExpandedSummoningSpecialBuilder.IceDevil40Prefix + suffix];
        }

        private static UnitEntityData[] IceDevilSavedSources40(
            UnitEntityData[] sources)
        {
            return sources.Where(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    IceDevilVisualPolicy40.Key)
                .OrderBy(unit => unit.UniqueId, StringComparer.Ordinal)
                .ToArray();
        }

        private static UnitEntityData[] IceDevilSavedState40(
            UnitEntityData caster)
        {
            return Sprint32SavedAuxiliaries(caster).Where(unit =>
                unit.Get<UnitPartSprint32ListenerReceipt>()?.Scope ==
                    IceDevilSavedScope40).ToArray();
        }

        private static bool OwnsIceDevilState40(UnitEntityData target,
            UnitEntityData caster, UnitEntityData[] sources)
        {
            var receipt = target?.Get<UnitPartSprint32ListenerReceipt>();
            if (receipt == null || receipt.Scope != IceDevilSavedScope40 ||
                receipt.UnitId != target.UniqueId ||
                receipt.CasterId != caster.UniqueId ||
                receipt.Role != "ice40-wall" ||
                string.IsNullOrEmpty(receipt.PrepareRunId) ||
                !ReferenceEquals(target.HoldingState, caster.HoldingState) ||
                target.Blueprint?.AssetGuid !=
                    IceDevilRules40Policy.WallSectionGuid)
                return false;
            UnitEntityData source = sources.FirstOrDefault(unit =>
                unit.UniqueId == receipt.SourceId &&
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    IceDevilVisualPolicy40.Key);
            return source != null && ReferenceEquals(target.Get<
                UnitPartSummonedMonster>()?.Summoner, source);
        }

        private static bool IceDevilCarries40(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            return unit?.Buffs?.GetBuff(blueprint) != null ||
                unit?.Blueprint?.AddFacts?.Any(fact =>
                    ReferenceEquals(fact, blueprint)) == true;
        }

        private static IceDevilSlowOnHit40 IceDevilSlowConsumer40(
            UnitEntityData source)
        {
            return source.Descriptor.Progression.Features.Enumerable
                .SelectMany(fact => fact.SelectComponents<
                    IceDevilSlowOnHit40>()).Single();
        }

        private static KytonRegeneration34 IceDevilRegeneration40(
            UnitEntityData source)
        {
            return source.Buffs.GetBuff(IceDevilSavedBlueprint40<
                BlueprintBuff>("Regeneration"))?.SelectComponents<
                    KytonRegeneration34>().SingleOrDefault();
        }

        private static JObject IceDevilWall40(UnitEntityData section)
        {
            var binding = section?.View?.GetComponent<
                InfernalWallNavmesh35>();
            Buff lifetime = section?.Buffs?.GetBuff(BlueprintRoot.Instance
                .SystemMechanics.SummonedUnitBuff);
            return new JObject {
                ["present"] = section != null,
                ["unit"] = section?.UniqueId,
                ["blueprint"] = section?.Blueprint?.AssetGuid,
                ["source"] = section?.Get<UnitPartSummonedMonster>()?
                    .Summoner?.UniqueId,
                ["lifetimeEndTicks"] = lifetime == null ?
                    (JToken)JValue.CreateNull() :
                    new JValue(lifetime.EndTime.Ticks),
                ["viewBinding"] = binding != null,
                ["gridNodesClaimed"] = binding?.GridNodesClaimed,
                ["activeGridClaims"] =
                    InfernalWallNavmesh35.ActiveGridClaimCount
            };
        }

        private static JObject IceDevilRegenerationState40(
            UnitEntityData source)
        {
            KytonRegeneration34 regeneration =
                IceDevilRegeneration40(source);
            return new JObject {
                ["owner"] = source?.UniqueId,
                ["suppressionRound"] = regeneration?.SuppressionRound,
                ["nativeActive"] = source == null ? false :
                    (bool)source.Descriptor.State.IsRegenerate,
                ["damage"] = source?.Damage
            };
        }

        private static void IceDevilRemoveFearPrecondition40(
            IEnumerable<UnitEntityData> units)
        {
            BlueprintBuff frightened = IceDevilSavedBlueprint40<
                BlueprintBuff>("FearFrightened");
            BlueprintBuff memory = IceDevilSavedBlueprint40<BlueprintBuff>(
                "FearImmunity");
            UnitEntityData[] owners = units.ToArray();
            foreach (UnitEntityData unit in owners)
                foreach (Buff buff in unit.Buffs.Enumerable.ToArray())
                    if ((ReferenceEquals(buff.Blueprint, frightened) ||
                         ReferenceEquals(buff.Blueprint, memory)) &&
                        owners.Contains(buff.Context?.MaybeCaster))
                        buff.Remove();
        }

        // Both saved owners must finish native materialization before the
        // fixture commands either of them. The shared wall helper already
        // settles the casting owner this way; the qualified Slow phase settles
        // its owner through Outsider30NativeAppearance. Nothing settled the
        // second owner, and an attack against a unit still appearing does not
        // resolve.
        private IEnumerable<int> IceDevilSettleOwners40(UnitEntityData[] owners)
        {
            Func<UnitEntityData, bool> pending = unit =>
                unit.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics
                    .SummonedUnitAppearBuff) != null ||
                !unit.Descriptor.State.CanAct ||
                unit.Commands.Raw.Any(value => value != null &&
                    !value.IsFinished);
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < deadline && owners.Any(pending))
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            Game.Instance.IsPaused = true;
            UnitEntityData unsettled = owners.FirstOrDefault(pending);
            if (unsettled != null) throw new InvalidOperationException(
                "Sprint40 persistence owner never settled: " +
                unsettled.UniqueId + ";appearing=" +
                (unsettled.Buffs.GetBuff(BlueprintRoot.Instance
                    .SystemMechanics.SummonedUnitAppearBuff) != null) +
                ";canAct=" + unsettled.Descriptor.State.CanAct +
                ";activeCommands=" + unsettled.Commands.Raw.Count(value =>
                    value != null && !value.IsFinished) + ".");
        }

        // Joining combat is not the same as being able to act in it. A unit
        // that has just joined waits for initiative, and the wall cast this
        // prepare performs first leaves the caster a standard-action cooldown.
        // The combat join and prepare controllers only settle that while the
        // game is actually advancing, so wait on the engine's own readiness
        // before dispatching the spear rather than queueing a command the
        // engine will not start.
        private IEnumerable<int> IceDevilCombatReady40(UnitEntityData[] owners)
        {
            Func<UnitEntityData, bool> pending = unit =>
                !unit.CombatState.IsInCombat ||
                unit.CombatState.IsWaitingInitiative ||
                !unit.CombatState.CanActInCombat ||
                unit.CombatState.Cooldown.StandardAction > 0f ||
                unit.Commands.Raw.Any(value => value != null &&
                    !value.IsFinished);
            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < deadline && owners.Any(pending))
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            Game.Instance.IsPaused = true;
            UnitEntityData unready = owners.FirstOrDefault(pending);
            if (unready != null) throw new InvalidOperationException(
                "Sprint40 persistence owner never became combat-ready: " +
                unready.UniqueId +
                ";inCombat=" + unready.CombatState.IsInCombat +
                ";waitingInitiative=" + unready.CombatState.IsWaitingInitiative +
                ";canActInCombat=" + unready.CombatState.CanActInCombat +
                ";standardCooldown=" + unready.CombatState.Cooldown.StandardAction +
                ";initiativeCooldown=" + unready.CombatState.Cooldown.Initiative +
                ";activeCommands=" + unready.Commands.Raw.Count(value =>
                    value != null && !value.IsFinished) + ".");
        }

        private static void IceDevilMeleeLane40(UnitEntityData caster,
            UnitEntityData[] sources, UnitEntityData owner,
            UnitEntityData target)
        {
            Vector3[] directions = {
                Vector3.forward, Vector3.right, Vector3.back, Vector3.left,
                (Vector3.forward + Vector3.right).normalized,
                (Vector3.forward + Vector3.left).normalized,
                (Vector3.back + Vector3.right).normalized,
                (Vector3.back + Vector3.left).normalized
            };
            Vector3 selectedOwner = Vector3.zero;
            Vector3 selectedTarget = Vector3.zero;
            bool selected = false;
            float separation = owner.Corpulence + target.Corpulence +
                new Feet(2).Meters;
            foreach (float distance in new[] { 6f, 8f, 10f, 12f })
            foreach (Vector3 direction in directions)
            {
                Vector3 origin = ObstacleAnalyzer.GetNearestNode(
                    caster.Position + direction * distance).clampedPosition;
                Vector3 across = Vector3.Cross(Vector3.up, direction)
                    .normalized;
                Vector3 endpoint = ObstacleAnalyzer.GetNearestNode(origin +
                    across * separation).clampedPosition;
                if (Vector3.Distance(origin, endpoint) < separation * .7f ||
                    Game.Instance.State.Units.All.Any(unit => unit != null &&
                        !unit.Destroyed && !sources.Contains(unit) &&
                        !ReferenceEquals(unit, caster) &&
                        (Vector3.Distance(unit.Position, origin) < 3f ||
                         Vector3.Distance(unit.Position, endpoint) < 3f)))
                    continue;
                selectedOwner = origin;
                selectedTarget = endpoint;
                selected = true;
                break;
            }
            if (!selected) throw new InvalidOperationException(
                "No bounded private Ice Devil melee lane is available.");
            PlaceExpandedSummoningUnit(owner, selectedOwner);
            PlaceExpandedSummoningUnit(target, selectedTarget);
            foreach (UnitEntityData unit in new[] { owner, target })
            {
                unit.Wake(0f);
                if (!Game.Instance.State.AwakeUnits.Contains(unit))
                    Game.Instance.State.AwakeUnits.Add(unit);
                unit.SightCache.ClearIfNeeded();
            }
            owner.Memory.Add(target).Visible = true;
            target.Memory.Add(owner).Visible = true;
            // Enter combat the same way the proven Sprint 34 lane does, which
            // is also the lane the qualified Sprint 40 Slow phase uses. A
            // queued UnitAttack for units that are not in combat never
            // resolves, and the enclosing prepare already captures and
            // restores both combat flags, so joining here is what it expects.
            owner.JoinCombat();
            target.JoinCombat();
            Game.Instance.Player.UpdateIsInCombat();
            new UnitCombatJoinController().Tick();
            new UnitCombatPrepareController().Tick();
            var probe = new UnitAttack(target);
            probe.Init(owner);
            if (!owner.Descriptor.State.CanAct || !owner.IsEnemy(target) ||
                !target.IsEnemy(owner) || !owner.HasLOS(target) ||
                !owner.IsInCombat || !target.IsInCombat ||
                !probe.IsUnitEnoughClose)
                throw new InvalidOperationException(
                    "Exact Ice Devil melee lane is not command-ready: " +
                    "canAct=" + owner.Descriptor.State.CanAct +
                    ";mutualHostility=" + (owner.IsEnemy(target) &&
                        target.IsEnemy(owner)) +
                    ";los=" + owner.HasLOS(target) +
                    ";ownerInCombat=" + owner.IsInCombat +
                    ";targetInCombat=" + target.IsInCombat +
                    ";enoughClose=" + probe.IsUnitEnoughClose + ".");
        }

        private IEnumerable<int> PrepareIceDevilSaved40(
            UnitEntityData caster, UnitEntityData[] sources)
        {
            UnitEntityData[] devils = IceDevilSavedSources40(sources);
            if (!Game.Instance.IsPaused || devils.Length != 2 ||
                IceDevilSavedState40(caster).Length != 0)
                throw new InvalidOperationException(
                    "Expected a paused, empty two-owner Sprint40 state.");
            UnitEntityData source = devils[0];
            UnitEntityData target = devils[1];

            foreach (int frame in IceDevilSettleOwners40(new[] { source, target }))
                yield return frame;

            // Select the listener faction before anything is spawned, which is
            // what the shared Sprint 32 contract says and what Sprint 35 does.
            // Its safety predicate refuses when any intake group's cached
            // faction set already contains the caster's faction, and the
            // source-owned wall section below is a summoned unit that joins
            // its summoner's group: forming the wall first put the caster's
            // own faction into the observed intake relations, and the shared
            // selection correctly refused rather than editing intake groups.
            BlueprintFaction hostileFaction = SelectSprint32ListenerFaction(
                caster, sources, Game.Instance.Player.Party.ToArray(),
                Game.Instance.State.Units.All.Where(unit => unit != null &&
                    !sources.Contains(unit) &&
                    !Game.Instance.Player.Party.Contains(unit)).ToArray());

            UnitEntityData wall = null;
            foreach (int frame in PrepareInfernalWallPersistence35(caster,
                source, sources,
                ExpandedSummoningSpecialBuilder.IceDevil40Prefix,
                "Sprint40", value => wall = value, activateBrain: false))
                yield return frame;
            if (wall == null) throw new InvalidOperationException(
                "Sprint40 persistence wall did not form.");
            var wallReceipt = wall.Ensure<UnitPartSprint32ListenerReceipt>();
            wallReceipt.Scope = IceDevilSavedScope40;
            wallReceipt.UnitId = wall.UniqueId;
            wallReceipt.CasterId = caster.UniqueId;
            wallReceipt.SourceId = source.UniqueId;
            wallReceipt.Role = "ice40-wall";
            wallReceipt.PrepareRunId = _request.RunId;
            wallReceipt.ListenerFaction = source.Faction.AssetGuid;

            BlueprintBuff slowBlueprint =
                IceDevilSlowConsumer40(source).Slowed;
            string sourceGroup = source.GroupId;
            string targetGroup = target.GroupId;
            BlueprintFaction sourceFaction = source.Faction;
            BlueprintFaction targetFaction = target.Faction;
            BlueprintFaction[] sourceAttacks = source.AttackFactions.ToArray();
            BlueprintFaction[] targetAttacks = target.AttackFactions.ToArray();
            int targetFortitude = target.Stats.SaveFortitude.BaseValue;
            int targetArmorClass = target.Stats.AC.BaseValue;
            int targetHitPoints = target.Stats.HitPoints.BaseValue;
            int targetDamage = target.Damage;
            bool sourceInCombat = source.IsInCombat;
            bool targetInCombat = target.IsInCombat;
            bool sourceInParty = Game.Instance.Player.Party.Contains(source);
            bool targetInParty = Game.Instance.Player.Party.Contains(target);
            bool relationsChanged = false;
            bool hostile = false;
            bool restored = false;
            try
            {
                source.Descriptor.State.AddConditionImmunity(
                    UnitCondition.Frightened);
                target.Descriptor.State.AddConditionImmunity(
                    UnitCondition.Frightened);
                source.GroupId = source.UniqueId;
                target.GroupId = target.UniqueId;
                relationsChanged = true;
                target.Descriptor.SwitchFactions(hostileFaction, true);
                source.AttackFactions.Match(new[] { hostileFaction });
                target.AttackFactions.Match(new[] { sourceFaction });
                foreach (UnitEntityData unit in new[] { source, target })
                {
                    unit.Group.ResetFactionSet();
                    unit.Group.UpdateAttackFactionsCache();
                }
                hostile = source.IsEnemy(target) && target.IsEnemy(source) &&
                    !ReferenceEquals(source.Group, target.Group) &&
                    source.Group.Count == 1 && target.Group.Count == 1;
                if (!hostile) throw new InvalidOperationException(
                    "Private Ice Devil owners are not mutually hostile.");
                target.Stats.SaveFortitude.BaseValue = -100;
                target.Stats.AC.BaseValue = -100;
                target.Stats.HitPoints.BaseValue = 100000;
                IceDevilMeleeLane40(caster, sources, source, target);
                foreach (int frame in IceDevilCombatReady40(
                    new[] { source, target })) yield return frame;
                foreach (int frame in IceDevilSpearUntil40(source, target,
                    () => target.Buffs.GetBuff(slowBlueprint) != null))
                    yield return frame;
            }
            finally
            {
                source.Commands.InterruptAll(true);
                target.Commands.InterruptAll(true);
                source.Commands.RemoveFinishedAndUpdateQueue();
                target.Commands.RemoveFinishedAndUpdateQueue();
                IceDevilRemoveFearPrecondition40(new[] { source, target });
                source.Descriptor.State.RemoveConditionImmunity(
                    UnitCondition.Frightened);
                target.Descriptor.State.RemoveConditionImmunity(
                    UnitCondition.Frightened);
                target.Stats.SaveFortitude.BaseValue = targetFortitude;
                target.Stats.AC.BaseValue = targetArmorClass;
                target.Damage = targetDamage;
                target.Stats.HitPoints.BaseValue = targetHitPoints;
                if (relationsChanged)
                {
                    source.AttackFactions.Match(sourceAttacks);
                    if (source.GroupId != sourceGroup)
                        source.GroupId = sourceGroup;
                    if (!ReferenceEquals(source.Faction, sourceFaction))
                        source.Descriptor.SwitchFactions(sourceFaction, true);
                    target.AttackFactions.Match(targetAttacks);
                    // Group first, faction second: leaving and rejoining are
                    // then accounted with the factions held at those edges.
                    if (target.GroupId != targetGroup)
                        target.GroupId = targetGroup;
                    if (!ReferenceEquals(target.Faction, targetFaction))
                        target.Descriptor.SwitchFactions(targetFaction, true);
                }
                if (!sourceInCombat && source.IsInCombat)
                    source.LeaveCombat();
                if (!targetInCombat && target.IsInCombat)
                    target.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
                restored = source.GroupId == sourceGroup &&
                    target.GroupId == targetGroup &&
                    ReferenceEquals(source.Faction, sourceFaction) &&
                    ReferenceEquals(target.Faction, targetFaction) &&
                    source.AttackFactions.SequenceEqual(sourceAttacks) &&
                    target.AttackFactions.SequenceEqual(targetAttacks) &&
                    source.IsInCombat == sourceInCombat &&
                    target.IsInCombat == targetInCombat &&
                    Game.Instance.Player.Party.Contains(source) ==
                        sourceInParty &&
                    Game.Instance.Player.Party.Contains(target) ==
                        targetInParty &&
                    !source.IsEnemy(target) && !target.IsEnemy(source);
                Game.Instance.IsPaused = true;
            }

            Buff slow = target.Buffs.GetBuff(slowBlueprint);
            bool slowExact = slow != null && slow.Active &&
                !slow.IsDisposed && slow.TimeLeft > TimeSpan.Zero &&
                ReferenceEquals(slow.Context?.MaybeCaster, source) &&
                target.Descriptor.State.HasCondition(UnitCondition.Slowed) &&
                target.Descriptor.State.CanAct &&
                target.Descriptor.State.CanMove;
            RosterPersistenceCheck("s40-slow-prepared", hostile && restored &&
                    slowExact,
                new JObject {
                    ["source"] = source.UniqueId,
                    ["target"] = target.UniqueId,
                    ["hostileDuringAttack"] = hostile,
                    ["relationsRestored"] = restored,
                    ["slowPresent"] = slow != null,
                    ["slowSource"] = slow?.Context?.MaybeCaster?.UniqueId,
                    ["slowEndTicks"] = slow?.EndTime.Ticks,
                    ["targetCanAct"] = target.Descriptor.State.CanAct,
                    ["targetCanMove"] = target.Descriptor.State.CanMove,
                    ["forcedRolls"] = 0,
                    ["actualNativeAttack"] = true
                }, "A real exact-owner spear hit and natural failed Fortitude save leave finite native Slow on the second owned Ice Devil; the temporary private hostility, groups, factions and combat state are restored before save.");
            if (!(hostile && restored && slowExact))
                throw new InvalidOperationException(
                    "Sprint40 Slow save precondition did not prepare cleanly.");

            KytonRegeneration34 regeneration =
                IceDevilRegeneration40(source);
            var injury = new PhysicalDamage(new DiceFormula(0,
                DiceType.Zero), PhysicalDamageForm.Slashing);
            injury.AddBonus(10);
            injury.AddAlignment(DamageAlignment.Good);
            RuleDealDamage damage = Rulebook.Trigger(new RuleDealDamage(
                target, source, new DamageBundle(injury)));
            if (regeneration == null || damage.Damage <= 0 ||
                regeneration.SuppressionRound != 1 ||
                (bool)source.Descriptor.State.IsRegenerate)
                throw new InvalidOperationException(
                    "Sprint40 good-damage regeneration state did not prepare.");

            Buff wallLifetime = wall.Buffs.GetBuff(BlueprintRoot.Instance
                .SystemMechanics.SummonedUnitBuff);
            if (wallLifetime == null ||
                wall.View?.GetComponent<InfernalWallNavmesh35>()?
                    .GridNodesClaimed <= 0)
                throw new InvalidOperationException(
                    "Sprint40 wall lifetime or navigation claim is absent.");
            JObject state = new JObject {
                ["role"] = "wall-regeneration-source",
                ["source"] = source.UniqueId,
                ["slowTarget"] = target.UniqueId,
                ["slowEndTicks"] = slow.EndTime.Ticks,
                ["wall"] = wall.UniqueId,
                ["wallEndTicks"] = wallLifetime.EndTime.Ticks,
                ["damage"] = source.Damage,
                ["regenerationSuppressionRound"] =
                    regeneration.SuppressionRound
            };
            source.Get<UnitPartRelease143RosterReceipt>().Sprint40State =
                state.ToString(Formatting.None);
            target.Get<UnitPartRelease143RosterReceipt>().Sprint40State =
                new JObject {
                    ["role"] = "slow-target",
                    ["source"] = source.UniqueId,
                    ["slowEndTicks"] = slow.EndTime.Ticks
                }.ToString(Formatting.None);
        }

        private void ObserveIceDevilSaved40(UnitEntityData caster,
            UnitEntityData[] sources, string phase)
        {
            UnitEntityData[] devils = IceDevilSavedSources40(sources);
            if (devils.Length != 2)
                throw new InvalidOperationException(
                    "Exact saved Sprint40 Ice Devil pair absent.");
            UnitEntityData source = devils.Single(unit =>
                JObject.Parse(unit.Get<UnitPartRelease143RosterReceipt>()
                    .Sprint40State)["role"]?.Value<string>() ==
                        "wall-regeneration-source");
            UnitEntityData target = devils.Single(unit =>
                !ReferenceEquals(unit, source));
            JObject saved = JObject.Parse(source.Get<
                UnitPartRelease143RosterReceipt>().Sprint40State);
            JObject targetSaved = JObject.Parse(target.Get<
                UnitPartRelease143RosterReceipt>().Sprint40State);
            BlueprintBuff slowBlueprint =
                IceDevilSlowConsumer40(source).Slowed;
            Buff slow = target.Buffs.GetBuff(slowBlueprint);
            BlueprintBuff fear = IceDevilSavedBlueprint40<BlueprintBuff>(
                "FearCarrier");
            BlueprintBuff regenerationBlueprint =
                IceDevilSavedBlueprint40<BlueprintBuff>("Regeneration");
            KytonRegeneration34 regeneration =
                IceDevilRegeneration40(source);
            UnitEntityData[] wallState = IceDevilSavedState40(caster);
            UnitEntityData wall = wallState.SingleOrDefault(unit =>
                unit.Get<UnitPartSprint32ListenerReceipt>()?.Role ==
                    "ice40-wall");
            bool enabled =
                _context.FeatureModules.Active.ExpandedSummoning;
            long slowEnd = (long)saved["slowEndTicks"];
            long wallEnd = (long)saved["wallEndTicks"];
            bool slowExact = slow != null && slow.Active &&
                !slow.IsDisposed && slow.EndTime.Ticks == slowEnd &&
                slow.TimeLeft > TimeSpan.Zero &&
                (ReferenceEquals(slow.Context?.MaybeCaster, source) ||
                 CircleOriginalCasterId(slow.Context) == source.UniqueId) &&
                target.Descriptor.State.HasCondition(UnitCondition.Slowed) &&
                (string)targetSaved["source"] == source.UniqueId &&
                (long)targetSaved["slowEndTicks"] == slowEnd;
            bool regenerationExact = regeneration != null &&
                regeneration.SuppressionRound ==
                    (int)saved["regenerationSuppressionRound"] &&
                source.Damage == (int)saved["damage"] &&
                !(bool)source.Descriptor.State.IsRegenerate;
            bool carriersExact = devils.All(unit =>
                IceDevilCarries40(unit, fear) &&
                IceDevilCarries40(unit, regenerationBlueprint));
            bool wallRetired = wall == null || wall.Destroyed ||
                !wall.IsInState ||
                (OwnsIceDevilState40(wall, caster, sources) &&
                 wall.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics
                     .SummonedUnitBuff) == null &&
                 wall.View?.GetComponent<InfernalWallNavmesh35>() == null);
            Buff lifetime = wall?.Buffs?.GetBuff(BlueprintRoot.Instance
                .SystemMechanics.SummonedUnitBuff);
            bool wallExact = enabled ?
                wall != null && OwnsIceDevilState40(wall, caster, sources) &&
                lifetime != null && lifetime.EndTime.Ticks == wallEnd &&
                wall.View?.GetComponent<InfernalWallNavmesh35>()?
                    .GridNodesClaimed > 0 &&
                InfernalWallNavmesh35.ActiveGridClaimCount > 0 :
                wallRetired &&
                    InfernalWallNavmesh35.ActiveGridClaimCount == 0;
            bool exact = slowExact && regenerationExact && carriersExact &&
                wallExact && source.UniqueId == (string)saved["source"] &&
                target.UniqueId == (string)saved["slowTarget"];
            _iceDevilLoadedSlow40 = slow;
            _iceDevilLoadedWall40 = wall;
            RosterPersistenceCheck("s40-state-" + phase, exact,
                new JObject {
                    ["moduleEnabled"] = enabled,
                    ["source"] = source.UniqueId,
                    ["target"] = target.UniqueId,
                    ["saved"] = saved,
                    ["slowPresent"] = slow != null,
                    ["slowEndTicks"] = slow?.EndTime.Ticks,
                    ["slowSource"] = slow?.Context?.MaybeCaster?.UniqueId,
                    ["slowCondition"] = target.Descriptor.State
                        .HasCondition(UnitCondition.Slowed),
                    ["regeneration"] =
                        IceDevilRegenerationState40(source),
                    ["fearAndRegenerationCarriers"] = carriersExact,
                    ["wall"] = IceDevilWall40(wall),
                    ["wallRetiredFromGameplay"] = wallRetired
                }, enabled ?
                    "The exact native Slow deadline/source, good-damage regeneration suppression, constant fear/regeneration carriers and CL13 owned wall/navigation state survive the ON boundary." :
                    "The exact native Slow deadline and saved damage remain, regeneration is disabled, and the module-OFF boundary retires the owned wall with zero navigation claims while allocating no private view resources.");
            if (!exact) throw new InvalidOperationException(
                "Sprint40 saved Slow, regeneration, carrier or wall state differs.");
        }

        private IEnumerable<int> CleanupIceDevilSaved40(
            UnitEntityData caster)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < deadline &&
                ((_iceDevilLoadedWall40 != null &&
                  !_iceDevilLoadedWall40.Destroyed) ||
                 InfernalWallNavmesh35.ActiveGridClaimCount != 0))
            {
                Game.Instance.IsPaused = false;
                Game.Instance.EntityDestroyer.Tick();
                yield return 0;
            }
            Game.Instance.IsPaused = true;
            bool slowRetired = _iceDevilLoadedSlow40 == null ||
                _iceDevilLoadedSlow40.IsDisposed ||
                _iceDevilLoadedSlow40.Owner?.Unit == null ||
                _iceDevilLoadedSlow40.Owner.Unit.Destroyed;
            bool wallRetired = (_iceDevilLoadedWall40 == null ||
                _iceDevilLoadedWall40.Destroyed ||
                !_iceDevilLoadedWall40.IsInState) &&
                InfernalWallNavmesh35.ActiveGridClaimCount == 0;
            RosterPersistenceCheck("s40-source-cleanup",
                slowRetired && wallRetired,
                new JObject {
                    ["slowDisposedOrOwnerDestroyed"] = slowRetired,
                    ["wallAbsent"] = wallRetired,
                    ["activeGridClaims"] =
                        InfernalWallNavmesh35.ActiveGridClaimCount
                }, "Destroying the two exact saved owners retires their finite Slow, wall unit and every navigation claim without touching another source.");
            if (!slowRetired || !wallRetired)
                throw new InvalidOperationException(
                    "Sprint40 source-owned cleanup did not finish.");
            _iceDevilLoadedSlow40 = null;
            _iceDevilLoadedWall40 = null;
            IceDevilSavedAbsence40(caster, "cleanup");
        }

        private void IceDevilSavedAbsence40(UnitEntityData caster,
            string phase)
        {
            BlueprintBuff slow = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(BlueprintBootstrap.Library,
                    "0bc608c3f2b548b44b7146b7530613ac",
                    "native Slow condition for Sprint40 absence");
            BlueprintBuff fear = IceDevilSavedBlueprint40<BlueprintBuff>(
                "FearCarrier");
            BlueprintBuff regeneration = IceDevilSavedBlueprint40<
                BlueprintBuff>("Regeneration");
            int auxiliaries = IceDevilSavedState40(caster).Length;
            int sources = Game.Instance.State.Units.All.Count(unit =>
                unit != null && !unit.Destroyed &&
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    IceDevilVisualPolicy40.Key);
            int walls = Game.Instance.State.Units.All.Count(unit =>
                unit != null && !unit.Destroyed &&
                unit.Blueprint?.AssetGuid ==
                    IceDevilRules40Policy.WallSectionGuid);
            int effects = Game.Instance.State.Units.All.Where(unit =>
                unit != null).Sum(unit => unit.Buffs.Enumerable.Count(buff =>
                    (ReferenceEquals(buff.Blueprint, fear) ||
                     ReferenceEquals(buff.Blueprint, regeneration)) &&
                        unit.Blueprint?.AssetGuid ==
                            IceDevilRules40Policy.UnitGuid ||
                    ReferenceEquals(buff.Blueprint, slow) &&
                        buff.Context?.MaybeCaster?.Blueprint?.AssetGuid ==
                            IceDevilRules40Policy.UnitGuid));
            bool exact = auxiliaries == 0 && sources == 0 && walls == 0 &&
                effects == 0 &&
                InfernalWallNavmesh35.ActiveGridClaimCount == 0;
            RosterPersistenceCheck("s40-state-" + phase + "-absence",
                exact, new JObject {
                    ["auxiliaries"] = auxiliaries,
                    ["sources"] = sources,
                    ["walls"] = walls,
                    ["ownedEffects"] = effects,
                    ["activeGridClaims"] =
                        InfernalWallNavmesh35.ActiveGridClaimCount
                }, "No Sprint40 owner, finite Slow, fear/regeneration carrier, wall or navigation claim remains after the cleanup save.");
            if (!exact) throw new InvalidOperationException(
                "Sprint40 saved state remains.");
        }

        private void ObserveIceDevilItems40(UnitEntityData[] sources,
            ItemEntity[] partyItems, int[] partyCounts)
        {
            UnitEntityData[] devils = IceDevilSavedSources40(sources);
            if (devils.Length != 2)
                throw new InvalidOperationException(
                    "Exact saved Sprint40 pair absent for item observation.");
            ItemEntity[] all = devils.SelectMany(unit =>
                unit.Descriptor.Inventory.Items).ToArray();
            BlueprintWeaponEnchantment frost =
                IceDevilSavedBlueprint40<BlueprintWeaponEnchantment>(
                    "SpearFrost");
            bool exact = all.Length == 6 &&
                all.Distinct().Count() == all.Length &&
                Sprint28PartyInventoryExact(partyItems, partyCounts);
            var rows = new JArray();
            foreach (UnitEntityData devil in devils)
            {
                var body = devil.Blueprint.Body;
                string[] expected = new BlueprintItem[] {
                    body.PrimaryHand, body.SecondaryHand,
                    body.PrimaryHandAlternative1,
                    body.SecondaryHandAlternative1,
                    body.PrimaryHandAlternative2,
                    body.SecondaryHandAlternative2,
                    body.PrimaryHandAlternative3,
                    body.SecondaryHandAlternative3
                }.Where(value => value != null)
                    .Concat(body.AdditionalLimbs ?? Array.Empty<
                        BlueprintItemWeapon>())
                    .Concat(body.AdditionalSecondaryLimbs ?? Array.Empty<
                        BlueprintItemWeapon>())
                    .Select(value => value.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                ItemEntity[] items = devil.Descriptor.Inventory.Items
                    .ToArray();
                ItemEntityWeapon spear = devil.Body.PrimaryHand.MaybeWeapon;
                int naturalSlots = devil.Body.AdditionalLimbs.Count(slot =>
                    slot.MaybeWeapon != null);
                bool row = items.Length == 3 &&
                    items.Select(item => item.Blueprint.AssetGuid)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .SequenceEqual(expected) &&
                    items.All(item => ReferenceEquals(item.Owner,
                            devil.Descriptor) && item.IsNonRemovable &&
                        !item.IsLootable && item.Count == 1) &&
                    spear != null && spear.Blueprint.Enchantments.Contains(
                        frost) && naturalSlots == 2;
                exact &= row;
                rows.Add(new JObject {
                    ["iceDevil"] = devil.UniqueId,
                    ["items"] = items.Length,
                    ["blueprints"] = new JArray(items.Select(item =>
                        item.Blueprint.name)),
                    ["primary"] = spear?.Blueprint?.name,
                    ["privateFrost"] = spear?.Blueprint?.Enchantments
                        .Contains(frost) == true,
                    ["naturalSlots"] = naturalSlots,
                    ["allOwnedNonremovableSingle"] = items.All(item =>
                        ReferenceEquals(item.Owner, devil.Descriptor) &&
                        item.IsNonRemovable && !item.IsLootable &&
                        item.Count == 1),
                    ["exact"] = row
                });
            }
            RosterPersistenceCheck("s40-items-owned", exact,
                new JObject {
                    ["owners"] = rows,
                    ["instances"] = new JArray(all.Select(
                        RuntimeHelpers.GetHashCode)),
                    ["distinctInstances"] = all.Distinct().Count(),
                    ["partyInventoryExact"] =
                        Sprint28PartyInventoryExact(partyItems, partyCounts)
                }, "Each saved Ice Devil owns its own nontransferable +1 frost spear, bite and tail; the private frost enchantment is present only through the exact spear blueprint and party inventory is untouched.");
            if (!exact) throw new InvalidOperationException(
                "Sprint40 saved item ownership differs.");
        }
    }
}
