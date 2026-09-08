using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private readonly JArray _breathPersistenceRecords = new JArray();

            private sealed class BreathSaveObserver : IGlobalRulebookHandler<RuleSavingThrow>,
                IGlobalRulebookHandler<RuleDealDamage>
            {
                internal readonly List<RuleSavingThrow> Saves = new List<RuleSavingThrow>();
                internal readonly List<RuleDealDamage> Damage = new List<RuleDealDamage>();
                public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
                public void OnEventDidTrigger(RuleSavingThrow evt) { Saves.Add(evt); }
                public void OnEventAboutToTrigger(RuleDealDamage evt) { }
                public void OnEventDidTrigger(RuleDealDamage evt) { Damage.Add(evt); }
            }

            private static bool IsBreathTrait(ElementalAlternateTraitBlueprints trait)
            {
                return trait != null && (trait.Definition.Id == ElementalAlternateTraitId.AcidBreath ||
                    trait.Definition.Id == ElementalAlternateTraitId.OozeBreath);
            }

            private ElementalPersistenceFixture BreathConditionTargetFixture()
            {
                return _fixtures.Single(value => value.Blueprints.AlternateTraits.Race == ElementalHeritageRace.Ifrit &&
                    value.Gender == Gender.Male && value.Heritage.Definition.Id == ElementalHeritageId.GeneralIfrit);
            }

            private UnitEntityData RequireBreathConditionTarget()
            {
                ElementalPersistenceFixture fixture = BreathConditionTargetFixture();
                UnitEntityData target = Snapshot(_allUnits).OfType<UnitEntityData>()
                    .SingleOrDefault(value => IsFixtureUnit(value, fixture));
                if (target == null || target.View == null || target.IsInCombat)
                    throw new InvalidOperationException("The exact disposable Ifrit breath target is absent or unsafe.");
                return target;
            }

            private Vector3 BreathCastPosition(UnitEntityData caster, UnitEntityData target)
            {
                if (AstarPath.active == null)
                    throw new InvalidOperationException("Breath persistence requires real navigable scene ground.");
                UnitEntityData[] others = Game.Instance.State.Units.All.Where(value =>
                    !ReferenceEquals(value, caster) && !ReferenceEquals(value, target)).ToArray();
                foreach (float radius in new[] { 5f, 8f, 11f, 14f })
                    for (int direction = 0; direction < 16; direction++)
                    {
                        float angle = direction * Mathf.PI / 8;
                        Vector3 requested = _fixtureStagingPosition +
                            new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                        Pathfinding.NNInfo nearest = AstarPath.active.GetNearest(requested);
                        if (nearest.node == null || !nearest.node.Walkable ||
                            Vector3.Distance(nearest.clampedPosition, requested) > 1) continue;
                        Vector3 point = nearest.clampedPosition;
                        if (others.All(value => Vector3.Distance(value.Position, point) >
                            3f + Math.Max(0, value.Corpulence))) return point;
                    }
                throw new InvalidOperationException("No bounded navigable breath fixture location excludes every other unit.");
            }

            private void SpendBreathForPersistence(ElementalPersistenceFixture fixture, UnitEntityData unit,
                AbilityData data, string phase)
            {
                ElementalAlternateTraitBlueprints trait = PersistenceSlaTrait(fixture, fixture.Heritage);
                if (!IsBreathTrait(trait) || !IsFixtureUnit(unit, fixture) || !Game.Instance.IsPaused || unit.IsInCombat)
                    throw new InvalidOperationException("Only the exact paused disposable breath caster is authorized.");
                var resource = PersistenceSlaResource(fixture, fixture.Heritage);
                if (unit.Descriptor.Resources.GetResourceAmount(resource) != 1 || !data.IsAvailable)
                    throw new InvalidOperationException("A native breath persistence cast requires one unspent use.");
                UnitEntityData target = RequireBreathConditionTarget();
                Buff[] previousConditions = BreathConditionBuffs(target);
                if (previousConditions.Any(value => value.Context.MaybeCaster == null ||
                        !IsFixtureUnit(value.Context.MaybeCaster)) || target.Buffs.Enumerable.Any(value =>
                        value.Blueprint.AssetGuid == ElementalBreathFactory.SickenedGuid && !previousConditions.Contains(value)))
                    throw new InvalidOperationException("A foreign Sickened fact must not participate in the disposable breath cast.");
                if (!BreathSystemConditionExact(target, previousConditions.Length != 0))
                    throw new InvalidOperationException("The disposable target has an unrelated or incomplete native Sickened state.");
                Buff[] otherTargetBuffs = OtherBreathTargetBuffs(target);
                JArray otherTargetBuffsBefore = DescribeBreathTargetBuffs(otherTargetBuffs);
                var priorConditionEvidence = new JArray(previousConditions.Select(value => new JObject {
                    { "casterId", value.Context.MaybeCaster.UniqueId },
                    { "casterLevel", value.Context.Params.CasterLevel }, { "dc", value.Context.Params.DC },
                    { "stacking", value.Blueprint.Stacking.ToString() }, { "endTimeTicks", value.EndTime.Ticks }
                }));
                Vector3 position = BreathCastPosition(unit, target);
                Vector3 casterBefore = unit.Position, targetBefore = target.Position;
                int woundsBefore = target.Damage, reflexBefore = target.Stats.SaveReflex.BaseValue;
                TimeSpan clock = Game.Instance.TimeController.GameTime;
                UnityEngine.Random.State random = UnityEngine.Random.state;
                UnitEntityData[] foreign = Game.Instance.State.Units.All.Where(value => !IsFixtureUnit(value)).ToArray();
                int[] foreignWounds = foreign.Select(value => value.Damage).ToArray();
                Buff[][] foreignBuffs = foreign.Select(value => value.Buffs.Enumerable.ToArray()).ToArray();
                Vector3[] foreignPositions = foreign.Select(value => value.Position).ToArray();
                var priorProjectiles = Game.Instance.ProjectileController.Projectiles.ToArray();
                if (priorProjectiles.Length != 0)
                    throw new InvalidOperationException("Breath persistence requires an idle native projectile controller.");
                var created = new List<Projectile>();
                var observer = new BreathSaveObserver();
                JObject record = null;
                bool exact = false, released = false, subscribed = false;
                UnitUseAbility command = null;
                try
                {
                    // Fresh-load evidence is recorded before any fixture work.
                    // Each independent cast then starts with no old breath condition:
                    // native Sickened reapplication need not replace its caster context.
                    // Remove only prior command-created facts on this exact fixture;
                    // never synthesize a condition or change the daily use.
                    foreach (Buff buff in previousConditions) target.Buffs.RemoveFact(buff);
                    if (BreathConditionBuffs(target).Length != 0 || !BreathSystemConditionExact(target, false))
                        throw new InvalidOperationException("Prior disposable breath conditions did not clean up exactly.");
                    unit.Position = position;
                    target.Position = position + new Vector3(0, 0, 0.8f);
                    target.Stats.SaveReflex.BaseValue = -100;
                    var point = new TargetWrapper(position + new Vector3(0, 0, 1.4f));
                    if (!data.CanTarget(point) || Game.Instance.HandsEquipmentController == null)
                        throw new InvalidOperationException("Native breath target/start prerequisites are unavailable.");
                    var canceled = new UnitUseAbility(data, point);
                    unit.Commands.Run(canceled);
                    bool queued = unit.Commands.Contains(canceled);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                    bool canceledExact = queued && !canceled.IsStarted && !canceled.IsActed &&
                        unit.Descriptor.Resources.GetResourceAmount(resource) == 1;
                    EventBus.Subscribe(observer);
                    subscribed = true;
                    UnityEngine.Random.InitState(7419);
                    command = new UnitUseAbility(data, point);
                    unit.Commands.Run(command);
                    var controller = new UnitActionController();
                    for (int tick = 0; !command.IsActed && !command.IsFinished && tick < 10; tick++)
                    {
                        if (command.Animation != null) command.Animation.IsActed = true;
                        ElementalBreathScenario.TickCommand(controller, command);
                    }
                    for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 100; tick++)
                    {
                        command.ExecutionProcess.Tick();
                        foreach (Projectile projectile in Game.Instance.ProjectileController.Projectiles.Where(value =>
                            !priorProjectiles.Contains(value) && !created.Contains(value) && ReferenceEquals(value.Launcher, unit)).ToArray())
                        {
                            created.Add(projectile);
                            typeof(Projectile).GetProperty("IsHit").GetSetMethod(true).Invoke(projectile, new object[] { true });
                            projectile.OnHit();
                        }
                    }
                    int level = unit.Descriptor.Progression.CharacterLevel;
                    Buff[] conditions = BreathConditionBuffs(target);
                    bool ooze = trait.Definition.Id == ElementalAlternateTraitId.OozeBreath;
                    bool conditionExact = ooze ? conditions.Length == 1 &&
                        ReferenceEquals(conditions[0].Context.SourceAbility, data.Blueprint) &&
                        ReferenceEquals(conditions[0].Context.MaybeCaster, unit) &&
                        Math.Abs(conditions[0].TimeLeft.TotalSeconds - 18) < 0.01 : conditions.Length == 0;
                    exact = canceledExact && command.IsStarted && command.IsActed && !command.Cutscene &&
                        !command.IsIgnoreCooldown && command.ExecutionProcess != null && command.ExecutionProcess.IsEnded &&
                        created.Count == 1 && observer.Saves.Count == 1 && observer.Saves.All(value =>
                            ReferenceEquals(value.Initiator, target) && !value.IsPassed &&
                            value.DifficultyClass == ElementalBreathPolicy.DifficultyClass(level, unit.Stats.Constitution.Bonus)) &&
                        observer.Damage.Count == (ElementalBreathPolicy.DamageDice(level) == 0 ? 0 : 1) &&
                        observer.Damage.All(value => ReferenceEquals(value.Initiator, unit) &&
                            ReferenceEquals(value.Target, target) && ReferenceEquals(value.SourceAbility, data.Blueprint)) &&
                        unit.Descriptor.Resources.GetResourceAmount(resource) == 0 && !data.IsAvailable &&
                        data.GetAvailableForCastCount() == 0 && conditionExact;
                    record = new JObject { { "fixture", fixture.Label }, { "phase", phase }, { "kind", "native-breath-command" },
                        { "abilityGuid", data.Blueprint.AssetGuid }, { "resourceGuid", resource.AssetGuid },
                        { "targetId", target.UniqueId }, { "canceledExact", canceledExact },
                        { "priorConditionsIntentionallyRemovedBeforeIndependentCast", priorConditionEvidence },
                        { "started", command.IsStarted }, { "acted", command.IsActed }, { "result", command.Result.ToString() },
                        { "processEnded", command.ExecutionProcess != null && command.ExecutionProcess.IsEnded },
                        { "resourceAmount", unit.Descriptor.Resources.GetResourceAmount(resource) },
                        { "projectiles", created.Count }, { "saves", observer.Saves.Count },
                        { "damage", observer.Damage.Sum(value => value.Damage) }, { "conditionExact", conditionExact },
                        { "level", level }, { "currentConstitutionModifier", unit.Stats.Constitution.Bonus } };
                }
                finally
                {
                    if (subscribed) EventBus.Unsubscribe(observer);
                    released = true;
                    if (command != null && command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded)
                        command.ExecutionProcess.Detach();
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                    foreach (Projectile projectile in created) projectile.Cleared = true;
                    // Native Tick marks cleared projectiles for deferred removal; the
                    // following tick performs that removal. No unrelated projectile
                    // may be present at this request-local boundary.
                    Game.Instance.ProjectileController.Tick();
                    Game.Instance.ProjectileController.Tick();
                    target.Stats.SaveReflex.BaseValue = reflexBefore;
                    target.Damage = woundsBefore;
                    unit.Position = casterBefore;
                    target.Position = targetBefore;
                    UnityEngine.Random.state = random;
                    bool foreignExact = foreign.Select((value, index) =>
                        value.Damage == foreignWounds[index] && value.Position.Equals(foreignPositions[index]) &&
                        value.Buffs.Enumerable.SequenceEqual(foreignBuffs[index])).All(value => value);
                    bool otherTargetBuffsExact = OtherBreathTargetBuffs(target).SequenceEqual(otherTargetBuffs);
                    bool systemConditionExact = BreathSystemConditionExact(target,
                        trait.Definition.Id == ElementalAlternateTraitId.OozeBreath);
                    bool cleanup = released && foreignExact && unit.Position.Equals(casterBefore) && target.Position.Equals(targetBefore) &&
                        otherTargetBuffsExact && systemConditionExact &&
                        target.Damage == woundsBefore && target.Stats.SaveReflex.BaseValue == reflexBefore &&
                        Game.Instance.TimeController.GameTime == clock && Game.Instance.IsPaused &&
                        Game.Instance.ProjectileController.Projectiles.SequenceEqual(priorProjectiles);
                    if (record == null) record = new JObject { { "fixture", fixture.Label }, { "phase", phase },
                        { "kind", "native-breath-command-incomplete" } };
                    record["cleanupExact"] = cleanup;
                    record["cleanupCasterPositionExact"] = unit.Position.Equals(casterBefore);
                    record["cleanupTargetPositionExact"] = target.Position.Equals(targetBefore);
                    record["cleanupWoundsExact"] = target.Damage == woundsBefore;
                    record["cleanupReflexExact"] = target.Stats.SaveReflex.BaseValue == reflexBefore;
                    record["cleanupClockExact"] = Game.Instance.TimeController.GameTime == clock;
                    record["cleanupPaused"] = Game.Instance.IsPaused;
                    record["cleanupProjectilesBefore"] = priorProjectiles.Length;
                    record["cleanupProjectilesAfter"] = Game.Instance.ProjectileController.Projectiles.Count();
                    record["foreignStateExact"] = foreignExact;
                    record["otherTargetBuffsExact"] = otherTargetBuffsExact;
                    record["otherTargetBuffsBefore"] = otherTargetBuffsBefore;
                    record["otherTargetBuffsAfter"] = DescribeBreathTargetBuffs(OtherBreathTargetBuffs(target));
                    record["nativeSystemConditionExact"] = systemConditionExact;
                    record["nativeSystemConditionBuffs"] = DescribeBreathTargetBuffs(BreathSystemConditionBuffs(target));
                    record["breathConditionsAfterCleanup"] = DescribeBreathTargetBuffs(BreathConditionBuffs(target));
                    record["observerReleased"] = released;
                    record["exact"] = exact && cleanup;
                    _breathPersistenceRecords.Add(record);
                    Add(_assertions, "elemental-breath-persistence-" + phase + "-" + fixture.Label,
                        "native queued cancellation and accepted breath; exact daily use, save/condition and owned-only cleanup",
                        record.ToString(Newtonsoft.Json.Formatting.None), exact && cleanup,
                        "real UnitCommands/UnitActionController/AbilityExecutionProcess; only animation and projectile timing isolated");
                }
                if (!record.Value<bool>("exact")) throw new InvalidOperationException("Native breath persistence cast diverged: " +
                    record.ToString(Newtonsoft.Json.Formatting.None));
                RequireFixtureStagingOutOfCombat("breath-cast-restored-" + fixture.Label);
            }

            private Buff[] BreathConditionBuffs(UnitEntityData unit)
            {
                BlueprintAbility[] abilities = _blueprintSet.Undine.AlternateTraits.Traits().Where(IsBreathTrait)
                    .SelectMany(value => value.Mechanics()).OfType<BlueprintAbility>().ToArray();
                return unit.Buffs.Enumerable.Where(value => value.Context != null &&
                    abilities.Any(ability => ReferenceEquals(value.Context.SourceAbility, ability))).ToArray();
            }

            private static JArray DescribeBreathTargetBuffs(IEnumerable<Buff> buffs)
            {
                return new JArray(buffs.Select(value => new JObject {
                    { "instance", System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value) },
                    { "blueprintGuid", value.Blueprint.AssetGuid }, { "name", value.Blueprint.name },
                    { "stacking", value.Blueprint.Stacking.ToString() },
                    { "active", value.Active }, { "suppressed", value.IsSuppressed },
                    { "endTimeTicks", value.EndTime.Ticks },
                    { "sourceAbility", value.Context == null || value.Context.SourceAbility == null ?
                        null : value.Context.SourceAbility.AssetGuid },
                    { "casterId", value.Context == null || value.Context.MaybeCaster == null ?
                        null : value.Context.MaybeCaster.UniqueId }
                }));
            }

            private static Buff[] BreathSystemConditionBuffs(UnitEntityData unit)
            {
                var blueprint = Game.Instance.BlueprintRoot.SystemMechanics.SickenedBuff;
                if (blueprint == null) throw new InvalidOperationException("Native Sickened status mapping is absent.");
                return unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, blueprint)).ToArray();
            }

            private static bool BreathSystemConditionExact(UnitEntityData unit, bool sickened)
            {
                Buff[] buffs = BreathSystemConditionBuffs(unit);
                // UnitState.UpdateStatusEffect -> UnitStatusBuff owns this exact
                // native companion. The test never adds/removes it itself.
                return unit.Descriptor.State.HasCondition(UnitCondition.Sickened) == sickened &&
                    buffs.Length == (sickened ? 1 : 0) && buffs.All(value => value.Active &&
                        !value.IsSuppressed && value.IsPermanent && value.Context != null &&
                        value.Context.SourceAbility == null && ReferenceEquals(value.Context.MaybeCaster, unit));
            }

            private Buff[] OtherBreathTargetBuffs(UnitEntityData unit)
            {
                return unit.Buffs.Enumerable.Except(BreathConditionBuffs(unit))
                    .Except(BreathSystemConditionBuffs(unit)).ToArray();
            }

            private void RecordBreathSavedCondition(int expectedCasterLevel, string phase)
            {
                if (_legacyMigration) return;
                UnitEntityData[] units = Snapshot(_allUnits).OfType<UnitEntityData>().ToArray();
                var conditions = units.SelectMany(unit => BreathConditionBuffs(unit).Select(buff => new { unit, buff })).ToArray();
                bool exact = conditions.Length == (expectedCasterLevel > 0 ? 1 : 0);
                if (expectedCasterLevel > 0)
                {
                    UnitEntityData target = RequireBreathConditionTarget();
                    ElementalPersistenceFixture casterFixture = _fixtures.Last(value =>
                        ExpectedPersistenceTraits(value, value.Heritage).Any(trait => trait.Definition.Id == ElementalAlternateTraitId.OozeBreath));
                    UnitEntityData caster = units.SingleOrDefault(value => IsFixtureUnit(value, casterFixture));
                    exact &= caster != null && BreathSystemConditionExact(target, true) &&
                        conditions.All(value => ReferenceEquals(value.unit, target) &&
                        value.buff.Blueprint.AssetGuid == ElementalBreathFactory.SickenedGuid &&
                        value.buff.Active && !value.buff.IsSuppressed &&
                        ReferenceEquals(value.buff.Context.MaybeCaster, caster) &&
                        value.buff.Context.Params.CasterLevel == expectedCasterLevel &&
                        value.buff.Context.Params.DC == ElementalBreathPolicy.DifficultyClass(expectedCasterLevel, caster.Stats.Constitution.Bonus) &&
                        value.buff.TimeLeft.TotalSeconds > 0 && value.buff.TimeLeft.TotalSeconds <= 18.01);
                }
                var record = new JObject { { "phase", phase }, { "kind", "saved-native-sickened" },
                    { "expectedCasterLevel", expectedCasterLevel }, { "gameTimeTicks", Game.Instance.TimeController.GameTime.Ticks },
                    { "exact", exact }, { "conditions", new JArray(conditions.Select(value => new JObject {
                        { "targetId", value.unit.UniqueId }, { "buffGuid", value.buff.Blueprint.AssetGuid },
                        { "abilityGuid", value.buff.Context.SourceAbility.AssetGuid },
                        { "casterId", value.buff.Context.MaybeCaster == null ? null : value.buff.Context.MaybeCaster.UniqueId },
                        { "casterLevel", value.buff.Context.Params.CasterLevel }, { "dc", value.buff.Context.Params.DC },
                        { "endTimeTicks", value.buff.EndTime.Ticks }, { "secondsLeft", value.buff.TimeLeft.TotalSeconds }
                    })) } };
                _breathPersistenceRecords.Add(record);
                Add(_assertions, "elemental-breath-saved-condition-" + phase, "exact native saved Sickened context/duration or fresh absence",
                    record.ToString(Newtonsoft.Json.Formatting.None), exact, "native BuffCollection and actual command-created caster context");
                if (!exact) throw new InvalidOperationException("Breath saved condition diverged: " + record.ToString(Newtonsoft.Json.Formatting.None));
            }

            private bool BreathParametersExact(ElementalPersistenceFixture fixture, ElementalHeritageBlueprints heritage,
                AbilityData data, int expectedLevel)
            {
                if (!IsBreathTrait(PersistenceSlaTrait(fixture, heritage))) return true;
                var parameters = data.CalculateParams();
                return parameters.CasterLevel == expectedLevel && parameters.DC ==
                    ElementalBreathPolicy.DifficultyClass(expectedLevel, data.Caster.Stats.Constitution.Bonus) &&
                    !data.Caster.HasFact(_blueprintSet.Undine.SlaFeature);
            }
        }
    }
}
