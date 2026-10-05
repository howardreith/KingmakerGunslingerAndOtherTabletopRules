using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
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
        // The existing exact-working-save sentinel owns the write and the
        // ordinary three stages. This closed scope changes only the fixture,
        // never the permitted save or the native load/save entry points.
        private bool Sprint16PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == "crocodilians"; } }

        private readonly List<RuntimeTestAssertion> _sprint16PersistenceChecks =
            new List<RuntimeTestAssertion>();
        private readonly JArray _sprint16PersistenceRows = new JArray();
        private UnitEntityData[] _sprint16PersistenceUnits;
        private bool? _sprint16PersistencePause;

        private static string Sprint16UnitName(string key)
        {
            return "KMG_Summoning_Unit_" + (key == "crocodile" ? "Crocodile" :
                key == "dire-crocodile" ? "DireCrocodile" : "Wolf");
        }

        private void StartSprint16Persistence()
        {
            try
            {
                bool prepare = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare;
                bool verify = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup;
                UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                    value != null && value.Descriptor != null).ToArray();
                if (party.Length != WorkingSaveSmokeScenario.ExpectedPartyCount)
                    throw new InvalidOperationException("Working party fingerprint changed before targeted persistence.");
                UnitEntityData caster = party.First(value => value.HoldingState != null);
                object gameState = ReadExactMember(Game.Instance, "State");
                BlueprintScriptableObject[] blueprints = BlueprintBootstrap.Library.GetAllBlueprints()
                    .Where(value => value != null).ToArray();
                if (!_sprint16PersistencePause.HasValue)
                    _sprint16PersistencePause = Game.Instance.IsPaused;
                // Freeze game time while the short, already-running native
                // Sprint state is inspected/saved. No duration is extended.
                Game.Instance.IsPaused = true;
                if (verify && _expandedSummoningPersistenceCleanupStarted)
                {
                    if (_expandedSummoningPersistenceCleanupSettleUpdates++ < 5) return;
                    UnitEntityData[] references = ExpandedSummoningPersistentUnits(gameState, party);
                    int live = references.Count(value => !value.Destroyed ||
                        value.View != null || value.HoldingState != null);
                    bool gone = _sprint16PersistenceUnits.All(value => value.Destroyed &&
                        value.View == null && value.HoldingState == null) && live == 0;
                    Sprint16Check(_sprint16PersistenceChecks, _sprint16PersistenceRows,
                        "persistence-native-expiry-cleanup", gone,
                        new JObject { ["remainingLive"] = live, ["cachedReferences"] = references.Length,
                            ["units"] = new JArray(_sprint16PersistenceUnits.Select(value => new JObject {
                                ["id"] = value.UniqueId, ["destroyed"] = value.Destroyed,
                                ["viewAbsent"] = value.View == null, ["areaAbsent"] = value.HoldingState == null })) },
                        "all five lifecycle-expired units and their views are detached before the cleanup save");
                    SaveSprint16PersistenceIfValid();
                    return;
                }
                if (prepare)
                {
                    if (!_context.FeatureModules.Active.ExpandedSummoning)
                        throw new InvalidOperationException("Targeted prepare requires Expanded Summoning enabled.");
                    if (RemoveExpandedSummoningStaleSummons(gameState, party) != 0)
                        throw new InvalidOperationException("Stale working-save summons could not be removed.");
                    string[] keys = { "crocodile", "crocodile", "dire-crocodile", "dire-crocodile", "wolf" };
                    SummonVariantSpec[] variants = keys.Select(key => ExpandedSummoningCatalog
                        .GenerateVariants(SummonFamily.NaturesAlly).Where(value =>
                            value.Creature.Key == key && value.Multiplicity == SummonMultiplicity.One)
                        .OrderBy(value => value.ParentTier).First()).ToArray();
                    _sprint16PersistenceUnits = SpawnExpandedSummoningVariants(blueprints, caster,
                        variants, "Sprint 16 targeted persistence");
                    foreach (UnitEntityData unit in _sprint16PersistenceUnits)
                    {
                        SetExpandedSummoningBrainActive(unit, false);
                        // These are already-materialized disposable fixtures.
                        // Do not persist the unrelated native appearance lock.
                        RemoveExpandedSummoningAppearanceBuffs(unit);
                    }
                    ArmSprint16Persistence(blueprints);
                }
                else _sprint16PersistenceUnits = ExpandedSummoningPersistentUnits(gameState, party);

                int published;
                bool publication = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(
                    BlueprintBootstrap.Library, _context.FeatureModules.Active.ExpandedSummoning, out published);
                Sprint16Check(_sprint16PersistenceChecks, _sprint16PersistenceRows, "persistence-publication", publication,
                    new JObject { ["enabled"] = _context.FeatureModules.Active.ExpandedSummoning,
                        ["published"] = published }, "source-derived required-parent publication, including suppression");
                if (!prepare && !verify)
                {
                    Sprint16Check(_sprint16PersistenceChecks, _sprint16PersistenceRows, "persistence-final-absence",
                        _sprint16PersistenceUnits.Length == 0,
                        new JObject { ["units"] = _sprint16PersistenceUnits.Length },
                        "fresh process finds zero KMG summons after the cleanup save");
                    CompleteSprint16Persistence(RuntimeTestStatuses.Pass, "");
                    return;
                }
                InspectSprint16PersistentUnits(blueprints, caster, prepare);
                if (verify)
                {
                    foreach (UnitEntityData unit in _sprint16PersistenceUnits)
                    {
                        Buff[] lifecycle = unit.Descriptor.Buffs.Enumerable.Where(value =>
                            ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)).ToArray();
                        if (lifecycle.Length != 1) throw new InvalidOperationException("Expected one owned native summon lifecycle.");
                        // Keep the native view until the destruction controller
                        // handles it and removes the unit from its scene. Direct
                        // Dispose beforehand can orphan private view resources.
                        CleanupExpandedSummoningUnit(unit);
                    }
                    Game.Instance.EntityDestroyer.Tick();
                    _expandedSummoningPersistenceCleanupStarted = true;
                    return;
                }
                SaveSprint16PersistenceIfValid();
            }
            catch (Exception exception)
            {
                Sprint16Check(_sprint16PersistenceChecks, _sprint16PersistenceRows, "persistence-exception", false,
                    new JObject { ["exception"] = DescribeExpandedSummoningCorrectionException(exception) },
                    "targeted persistence completes without exception");
                CompleteSprint16Persistence(RuntimeTestStatuses.Fail, "No additional save was armed after the fixture failure.");
            }
        }

        private void ArmSprint16Persistence(BlueprintScriptableObject[] blueprints)
        {
            UnityEngine.Random.State random = UnityEngine.Random.state;
            try
            {
                foreach (string key in CrocodilianVisualPolicy.Keys)
                {
                    UnitEntityData[] pair = _sprint16PersistenceUnits.Where(value => value.Blueprint.name == Sprint16UnitName(key)).ToArray();
                    if (pair.Length != 2) throw new InvalidOperationException("Missing targeted crocodilian pair.");
                    for (int index = 0; index < pair.Length; index++)
                    {
                        ExecuteExpandedSummoningRuntimeAbility(pair[index], Sprint16Sprint(blueprints, key),
                            0, new TargetWrapper(pair[index]), false);
                        // Cooldown-only copy crosses the real native state
                        // expiry boundary; its 60-second cooldown is untouched.
                        if (index == 1) ExpireSprint16OwnedBuff(pair[index],
                            pair[index].Descriptor.Buffs.GetBuff(Sprint16SprintBuff(blueprints, key, false)));
                    }
                }
                UnitEntityData[] crocs = _sprint16PersistenceUnits.Where(value => value.Blueprint.name == Sprint16UnitName("crocodile")).ToArray();
                UnitEntityData dire = _sprint16PersistenceUnits.First(value => value.Blueprint.name == Sprint16UnitName("dire-crocodile"));
                UnitEntityData wolf = _sprint16PersistenceUnits.Single(value => value.Blueprint.name == Sprint16UnitName("wolf"));
                ArmSprint16PersistentLink(crocs[0], crocs[1], false);
                ArmSprint16PersistentLink(dire, wolf, true);
            }
            finally { UnityEngine.Random.state = random; }
        }

        private void ArmSprint16PersistentLink(UnitEntityData owner, UnitEntityData target, bool swallow)
        {
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            int bab = owner.Descriptor.Stats.BaseAttackBonus.BaseValue;
            try
            {
                owner.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                PlaceExpandedSummoningUnit(target, owner.Position + Vector3.forward);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                bool grabbed = grab.TryGrab(target, SummonLimbs.PrimaryWeapon(owner), true);
                Buff held = SummonHoldComponent.HeldState(owner, target, grab);
                string outcome = "held";
                if (swallow && held != null)
                {
                    UnityEngine.Random.InitState(FindNativeD20Seed(10));
                    held.TickMechanics();
                    int claimed = -1;
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    outcome = SummonHoldComponent.MaintainLink(owner, target, grab, null,
                        owner.Descriptor.Buffs.GetBuff(grab.HoldBuff), held, ref claimed);
                }
                bool exact = grabbed && (swallow ? target.Get<UnitPartSwallowed>() != null &&
                    ReferenceEquals(target.Get<UnitPartSwallowed>().Swallower.Value, owner) &&
                    target.Descriptor.HasFact(grab.SwallowedBuff) :
                    ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target));
                Sprint16Check(_sprint16PersistenceChecks, _sprint16PersistenceRows,
                    swallow ? "persistence-arm-swallow" : "persistence-arm-hold", exact,
                    new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                        ["outcome"] = outcome, ["grabbed"] = grabbed },
                    "request-local known-hit native grapple; swallow through successful later-round maintain; no hostile added to save");
            }
            finally { owner.Descriptor.Stats.BaseAttackBonus.BaseValue = bab; }
        }

        private static void ExpireSprint16OwnedBuff(UnitEntityData owner, Buff buff)
        {
            if (buff == null) throw new InvalidOperationException("Expected owned disposable buff is missing.");
            PropertyInfo endTime = typeof(Buff).GetProperty("EndTime",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (endTime == null || !endTime.CanWrite) throw new MissingMemberException("Buff.EndTime setter");
            endTime.SetValue(buff, Game.Instance.TimeController.GameTime - TimeSpan.FromMilliseconds(1), null);
            owner.Descriptor.Buffs.UpdateNextEvent();
            owner.Descriptor.Buffs.Tick();
        }

        private void InspectSprint16PersistentUnits(BlueprintScriptableObject[] blueprints,
            UnitEntityData caster, bool prepare)
        {
            UnitEntityData[] units = _sprint16PersistenceUnits;
            string[] expected = { Sprint16UnitName("crocodile"), Sprint16UnitName("crocodile"),
                Sprint16UnitName("dire-crocodile"), Sprint16UnitName("dire-crocodile"), Sprint16UnitName("wolf") };
            bool identities = units.Select(value => value.Blueprint.name).OrderBy(value => value)
                .SequenceEqual(expected.OrderBy(value => value)) && units.All(value =>
                    ReferenceEquals(value.Blueprint, blueprints.Single(blueprint => blueprint.name == value.Blueprint.name)) &&
                    value.HoldingState == caster.HoldingState && value.Commands != null &&
                    value.View != null && ReferenceEquals(value.View.Data, value));
            bool lifetimes = units.All(value => value.Descriptor.Buffs.Enumerable.Count(buff =>
                ReferenceEquals(buff.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) &&
                buff.MaybeContext != null && ReferenceEquals(buff.MaybeContext.MaybeCaster, caster) &&
                !buff.IsPermanent && buff.TimeLeft > TimeSpan.Zero && buff.TimeLeft.TotalSeconds <= 127d) == 1);
            Sprint16Check(_sprint16PersistenceChecks, _sprint16PersistenceRows, "persistence-identities-context-duration",
                identities && lifetimes, new JObject { ["identities"] = identities, ["lifetimes"] = lifetimes,
                    ["units"] = new JArray(units.Select(value => value.Blueprint.name + ":" + value.UniqueId)) },
                "five exact registered identities, original summoner context and finite native summon lifetimes");
            foreach (string key in CrocodilianVisualPolicy.Keys)
            {
                UnitEntityData[] pair = units.Where(value => value.Blueprint.name == Sprint16UnitName(key)).ToArray();
                var records = new JArray();
                var state = Sprint16SprintBuff(blueprints, key, false);
                var cooldown = Sprint16SprintBuff(blueprints, key, true);
                bool exact = pair.Length == 2 && pair.Count(value => value.Descriptor.HasFact(state)) == 1;
                foreach (UnitEntityData unit in pair)
                {
                    Buff running = unit.Descriptor.Buffs.GetBuff(state);
                    Buff waiting = unit.Descriptor.Buffs.GetBuff(cooldown);
                    ModifiableValue perception = unit.Descriptor.Stats.GetStat(StatType.SkillPerception);
                    ModifiableValue stealth = unit.Descriptor.Stats.GetStat(StatType.SkillStealth);
                    ModifiableValue mobility = unit.Descriptor.Stats.GetStat(StatType.SkillMobility);
                    CrocodilianRulesProfile profile = CrocodilianRulesPolicy.For(key);
                    // The prepared hold legitimately changes Dexterity. The
                    // clean reload must recover the printed land totals; the
                    // save-boundary row records the exact live penalty instead.
                    int expectedStealth = (key == "crocodile" ? 5 : 0) + (prepare ?
                        unit.Descriptor.Stats.Dexterity.Bonus - (key == "crocodile" ? 1 : 0) : 0);
                    string visual = unit.View == null ? "no-view" : ExpandedSummoningPteranodonViewPatch.DescribeView(unit.View);
                    string renderer = unit.View == null ? "no-view" : DescribePteranodonRenderers(unit.View);
                    bool enabled = _context.FeatureModules.Active.ExpandedSummoning;
                    bool appearance = enabled ? visual.StartsWith("visual:attached;", StringComparison.Ordinal) &&
                        renderer.StartsWith("mesh=KMG_" + key + "_Original;", StringComparison.Ordinal) &&
                        renderer.Contains(";bones=27;") : visual == "donor-visual:module-disabled" &&
                        !renderer.Contains("_Original") && renderer.Contains(";enabled=true;");
                    bool live = waiting != null && !waiting.IsPermanent && waiting.TimeLeft.TotalSeconds > 0 &&
                        waiting.TimeLeft.TotalSeconds <= 60.01d && ReferenceEquals(waiting.MaybeContext.MaybeCaster, unit) &&
                        unit.Descriptor.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, cooldown)) == 1 &&
                        (running == null || running.TimeLeft.TotalSeconds > 0 && running.TimeLeft.TotalSeconds <= 6.01d &&
                            ReferenceEquals(running.MaybeContext.MaybeCaster, unit)) &&
                        unit.Descriptor.Stats.Speed.BaseValue == 20 &&
                        unit.Descriptor.Stats.Speed.ModifiedValue == (running == null ? 20 : 40) &&
                        !new AbilityData(unit.Descriptor.Abilities.GetAbility(Sprint16Sprint(blueprints, key))).IsAvailable &&
                        perception.BaseValue == profile.PerceptionRanks && stealth.BaseValue == profile.StealthRanks &&
                        perception.ModifiedValue == (key == "crocodile" ? 8 : 14) &&
                        stealth.ModifiedValue == expectedStealth && mobility.BaseValue == 0 && appearance;
                    records.Add(new JObject { ["id"] = unit.UniqueId, ["active"] = running != null,
                        ["activeSeconds"] = running == null ? 0 : running.TimeLeft.TotalSeconds,
                        ["cooldownSeconds"] = waiting == null ? 0 : waiting.TimeLeft.TotalSeconds,
                        ["speed"] = Sprint16Speed(unit), ["perception"] = DescribeSprint16Skill(perception),
                        ["stealth"] = DescribeSprint16Skill(stealth), ["mobility"] = DescribeSprint16Skill(mobility),
                        ["expectedLiveStealth"] = expectedStealth,
                        ["visual"] = visual, ["renderer"] = renderer, ["exact"] = live });
                    exact = exact && live;
                    if (!prepare && waiting != null)
                    {
                        if (running != null) ExpireSprint16OwnedBuff(unit, running);
                        bool stateExpired = unit.Descriptor.Stats.Speed.ModifiedValue == 20 &&
                            !unit.Descriptor.HasFact(state) && unit.Descriptor.HasFact(cooldown);
                        ExpireSprint16OwnedBuff(unit, waiting);
                        bool cooldownExpired = !unit.Descriptor.HasFact(cooldown) &&
                            new AbilityData(unit.Descriptor.Abilities.GetAbility(Sprint16Sprint(blueprints, key))).IsAvailable;
                        exact = exact && stateExpired && cooldownExpired;
                        records.Add(new JObject { ["id"] = unit.UniqueId,
                            ["nativeStateExpiry"] = stateExpired, ["nativeCooldownExpiry"] = cooldownExpired });
                    }
                }
                Sprint16Check(_sprint16PersistenceChecks, _sprint16PersistenceRows, "persistence-" + key + "-sprint-skills-visual",
                    exact, new JObject { ["phase"] = prepare ? "prepare" : "reloaded", ["copies"] = records },
                    "one active and one cooldown-only copy; exact buff ownership, speed and skill totals; module-appropriate views; native expiry after reload");
            }
            if (!prepare)
            {
                var records = new JArray();
                bool reset = true;
                var relationshipBuffs = units.Select(SummonGrabComponent.Find).Where(value => value != null)
                    .SelectMany(value => new[] { value.HoldBuff, value.GrappledBuff, value.SwallowedBuff })
                    .Where(value => value != null).Distinct().ToArray();
                foreach (UnitEntityData unit in units)
                {
                    var links = unit.Get<UnitPartSummonGrappleLinks>();
                    var swallower = unit.Get<UnitPartSwallowWhole>();
                    string[] stuckBuffs = unit.Descriptor.Buffs.Enumerable.Where(value =>
                        relationshipBuffs.Any(blueprint => ReferenceEquals(value.Blueprint, blueprint)))
                        .Select(value => value.Blueprint.name).ToArray();
                    bool free = unit.Get<UnitPartGrappleInitiator>() == null && unit.Get<UnitPartGrappleTarget>() == null &&
                        unit.Get<UnitPartSwallowed>() == null && (links == null || links.Count == 0) &&
                        (swallower == null || swallower.SwallowedUnits.Count == 0) &&
                        stuckBuffs.Length == 0 && !unit.Descriptor.State.HasCondition(UnitCondition.CantAct) &&
                        !unit.Descriptor.State.HasCondition(UnitCondition.CantMove);
                    reset = reset && free;
                    records.Add(new JObject { ["id"] = unit.UniqueId, ["free"] = free,
                        ["storedLinks"] = links == null ? 0 : links.Count, ["stuckBuffs"] = new JArray(stuckBuffs),
                        ["heldInitiator"] = unit.Get<UnitPartGrappleInitiator>() != null,
                        ["heldTarget"] = unit.Get<UnitPartGrappleTarget>() != null,
                        ["swallowed"] = unit.Get<UnitPartSwallowed>() != null,
                        ["swallowerCount"] = swallower == null ? 0 : swallower.SwallowedUnits.Count,
                        ["cantAct"] = unit.Descriptor.State.HasCondition(UnitCondition.CantAct),
                        ["cantMove"] = unit.Descriptor.State.HasCondition(UnitCondition.CantMove),
                        ["buffs"] = new JArray(unit.Descriptor.Buffs.Enumerable.Select(value => value.Blueprint.name)) });
                }
                Sprint16Check(_sprint16PersistenceChecks, _sprint16PersistenceRows, "persistence-clean-session-links", reset,
                    new JObject { ["units"] = records },
                    "OwnerAcceptedEngineLimitation: ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD; no reconstruction or orphaned hold/swallow state");
            }
        }

        private void SaveSprint16PersistenceIfValid()
        {
            if (_sprint16PersistenceChecks.Any(value => value.Status != RuntimeTestStatuses.Pass))
            {
                CompleteSprint16Persistence(RuntimeTestStatuses.Fail, "Fixture checks failed; no additional save was armed.");
                return;
            }
            BeginExpandedSummoningPersistenceSave();
        }

        private void CompleteSprint16Persistence(string status, string warning)
        {
            if (_sprint16PersistencePause.HasValue)
            {
                Game.Instance.IsPaused = _sprint16PersistencePause.Value;
                _sprint16PersistencePause = null;
            }
            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            bool writes = _request.Scenario != RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyAbsent;
            var assertions = _sprint16PersistenceChecks;
            assertions.Add(Assertion("exact-working-load", "one correlated working descriptor; baseline distinct",
                "working=" + evidence.WorkingMatchCount + ";baseline=" + evidence.BaselineMatchCount,
                evidence.WorkingMatchCount == 1 && evidence.BaselineMatchCount == 1 && evidence.DescriptorReferenceCorrelated,
                "unchanged guarded working-save load sentinel"));
            assertions.Add(Assertion("exact-working-save-write", writes ? "one exact native working save" : "no save write",
                "count=" + evidence.ExpectedWorkingSaveRoutineCount + ";stashed=" + evidence.ExpectedWorkingStashedAreaCount,
                !evidence.SaveWritingApiObserved && evidence.ExpectedWorkingSaveRoutineCount == (writes ? 1 : 0) &&
                    (!writes || evidence.ExpectedWorkingStashedAreaCount >= 1), "exact descriptor-bound native save sentinel"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion, "Unity Mod Manager ModEntry.Info.Version"));
            if (status == RuntimeTestStatuses.Pass && assertions.Any(value => value.Status != RuntimeTestStatuses.Pass))
                status = RuntimeTestStatuses.Fail;
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory, "sprint16-persistence.json"),
                _sprint16PersistenceRows.ToString(Formatting.Indented));
            RuntimeTestResult result = CreateResult(status, assertions, null);
            result.WorkingSaveSmoke = evidence;
            if (!string.IsNullOrWhiteSpace(warning)) result.Warnings.Add(warning);
            Complete(result);
        }
    }
}
