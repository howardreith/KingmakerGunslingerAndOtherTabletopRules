using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Test-only receipt attached solely to newly created disposable summons.
    // No production mechanic reads this part; no links are reconstructed.
    public sealed class UnitPartSprint17PersistenceReceipt : UnitPart
    {
        [JsonProperty] public string Scope;
        [JsonProperty] public string Role;
        [JsonProperty] public string UnitId;
        [JsonProperty] public string CasterId;
        [JsonProperty] public int VenomDc, VenomTicks, VenomSaves, ConstitutionDamage;
    }

    internal sealed partial class RuntimeTestRunner
    {
        private bool Sprint17PersistenceScope
        { get { return (string)_request.Parameters?["persistenceScope"] == SerpentinePersistenceReviewPolicy.Scope; } }
        private readonly List<RuntimeTestAssertion> _snakePersistenceChecks = new List<RuntimeTestAssertion>();
        private readonly JArray _snakePersistenceRows = new JArray();
        private IEnumerator<int> _snakePersistenceSteps;
        private bool? _snakePersistencePause;

        private void StartSprint17Persistence()
        {
            try
            {
                if (_snakePersistenceSteps == null) _snakePersistenceSteps = RunSprint17Persistence().GetEnumerator();
                _snakePersistenceSteps.MoveNext();
            }
            catch (Exception exception)
            {
                SnakePersistenceCheck("exception", false,
                    new JObject { ["exception"] = DescribeExpandedSummoningCorrectionException(exception) },
                    "closed persistence fixture completes without exception");
                CompleteSprint17Persistence(RuntimeTestStatuses.Fail, "No additional working-save write was armed after failure.");
            }
        }

        private void SnakePersistenceCheck(string name, bool valid, JObject row, string expected)
        {
            row["check"] = name; row["passed"] = valid;
            _snakePersistenceRows.Add(row);
            _snakePersistenceChecks.Add(Assertion("sprint17-persistence-" + name, expected,
                row.ToString(Formatting.None), valid, "exact working-save, owned-fixture native persistence"));
        }

        private static BlueprintUnit SnakePersistenceBlueprint(BlueprintScriptableObject[] blueprints, string role)
        {
            string key = SerpentinePersistenceReviewPolicy.CreatureKey(role);
            var creature = ExpandedSummoningCatalog.All.Single(value => value.Key == key);
            string name = ExpandedSummoningInternalName(ExpandedSummoningIdentityCatalog.UnitSymbol(creature));
            return blueprints.OfType<BlueprintUnit>().Single(value => value.name == name);
        }

        private static bool SnakePersistenceOwns(UnitEntityData unit, UnitEntityData caster,
            BlueprintScriptableObject[] blueprints)
        {
            var receipt = unit.Get<UnitPartSprint17PersistenceReceipt>();
            if (receipt == null || SerpentinePersistenceReviewPolicy.CreatureKey(receipt.Role) == null) return false;
            var blueprint = SnakePersistenceBlueprint(blueprints, receipt.Role);
            return SerpentinePersistenceReviewPolicy.Owns(receipt.Scope, receipt.Role, receipt.UnitId,
                unit.UniqueId, receipt.CasterId, caster.UniqueId, blueprint.AssetGuid, unit.Blueprint.AssetGuid) &&
                ReferenceEquals(unit.Blueprint, blueprint) && unit.HoldingState == caster.HoldingState &&
                unit.Descriptor.Buffs.Enumerable.Count(buff =>
                    ReferenceEquals(buff.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) &&
                    buff.MaybeContext != null && ReferenceEquals(buff.MaybeContext.MaybeCaster, caster) &&
                    !buff.IsPermanent) == 1;
        }

        private static bool SnakePersistenceReady(UnitEntityData unit)
        {
            return SerpentinePersistenceReviewPolicy.NativeAppearanceReady(EntityFadedIn(unit),
                DissolveAmount(unit), unit.Descriptor.State.CanAct, unit.Descriptor.State.CanMove,
                unit.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) != null);
        }

        private IEnumerable<int> RunSprint17Persistence()
        {
            bool prepare = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare;
            bool verify = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup;
            var party = Game.Instance.Player.Party.Where(value => value != null && value.Descriptor != null).ToArray();
            if (party.Length != WorkingSaveSmokeScenario.ExpectedPartyCount)
                throw new InvalidOperationException("Working party fingerprint changed.");
            var caster = party.First(value => value.HoldingState != null);
            var blueprints = BlueprintBootstrap.Library.GetAllBlueprints().Where(value => value != null).ToArray();
            _snakePersistencePause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            var existing = ExpandedSummoningPersistentUnits(Game.Instance.State, party);
            // Never guess that an unmarked KMG summon belongs to this fixture.
            // Fail without touching it, including older/different fixtures.
            if (existing.Any(value => !SnakePersistenceOwns(value, caster, blueprints)))
                throw new InvalidOperationException("An unowned or malformed KMG summon is present; no broad cleanup is permitted.");
            var unrelated = caster.HoldingState.AllEntityData.OfType<UnitEntityData>()
                .Where(value => !existing.Any(owned => ReferenceEquals(owned, value)))
                .Select(value => new { Unit = value, value.Destroyed, value.HoldingState, value.Blueprint }).ToArray();
            int published;
            bool publication = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(
                BlueprintBootstrap.Library, _context.FeatureModules.Active.ExpandedSummoning, out published);
            SnakePersistenceCheck("publication", publication,
                new JObject { ["enabled"] = _context.FeatureModules.Active.ExpandedSummoning, ["published"] = published },
                "source-derived module publication, with private snake suppression unchanged");
            if (!prepare && !verify)
            {
                SnakePersistenceCheck("fresh-absence", existing.Length == 0,
                    new JObject { ["remainingSummons"] = existing.Length }, "fresh load contains zero fixture summons or receipts");
                CompleteSprint17Persistence(RuntimeTestStatuses.Pass, "");
                yield break;
            }
            UnitEntityData[] units = existing;
            if (prepare)
            {
                if (!_context.FeatureModules.Active.ExpandedSummoning)
                    throw new InvalidOperationException("Snake prepare requires Expanded Summoning enabled.");
                foreach (var step in DestroySprint17PersistenceUnits(existing, "stale")) yield return step;
                if (_snakePersistenceChecks.Any(value => value.Status != RuntimeTestStatuses.Pass))
                    throw new InvalidOperationException("Stale owned fixture did not cleanly retire.");
                var created = new List<UnitEntityData>();
                foreach (string role in SerpentinePersistenceReviewPolicy.Roles)
                {
                    string key = SerpentinePersistenceReviewPolicy.CreatureKey(role);
                    var variant = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                        .Where(value => value.Creature.Key == key && value.Multiplicity == SummonMultiplicity.One)
                        .OrderBy(value => value.ParentTier).First();
                    var unit = SpawnExpandedSummoningVariants(blueprints, caster, new[] { variant },
                        "Sprint17 marked snake persistence").Single();
                    var receipt = unit.Ensure<UnitPartSprint17PersistenceReceipt>();
                    receipt.Scope = SerpentinePersistenceReviewPolicy.ReceiptScope;
                    receipt.Role = role; receipt.UnitId = unit.UniqueId; receipt.CasterId = caster.UniqueId;
                    created.Add(unit);
                    SetExpandedSummoningBrainActive(unit, false);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                }
                units = created.ToArray();
                // Native appearance settles before arming short-lived state.
                // No buff removal, renderer override, or world-time jump.
                TimeSpan appearanceStart = Game.Instance.Player.GameTime;
                int appearanceFrames = 0;
                Game.Instance.IsPaused = false;
                for (; appearanceFrames < 600; appearanceFrames++)
                {
                    yield return 0;
                    if (appearanceFrames >= 30 && units.All(SnakePersistenceReady))
                        break;
                }
                Game.Instance.IsPaused = true;
                bool ready = units.All(SnakePersistenceReady);
                SnakePersistenceCheck("native-appearance", ready,
                    new JObject { ["frames"] = appearanceFrames,
                        ["nativeElapsedSeconds"] = (Game.Instance.Player.GameTime - appearanceStart).TotalSeconds,
                        ["units"] = new JArray(units.Select(value => new JObject {
                        ["id"] = value.UniqueId, ["fadedIn"] = EntityFadedIn(value), ["dissolve"] = DissolveAmount(value),
                        ["canAct"] = value.Descriptor.State.CanAct, ["canMove"] = value.Descriptor.State.CanMove,
                        ["appearanceLock"] = value.Descriptor.Buffs.GetBuff(
                            BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) != null })) },
                    "all four summons naturally finish appearance and native action/movement locks before arming");
                if (!ready)
                {
                    CompleteSprint17Persistence(RuntimeTestStatuses.Fail, "Native appearance/control did not settle; no attack or save armed.");
                    yield break;
                }
                ArmSprint17Persistence(units, blueprints);
            }
            InspectSprint17Persistence(units, caster, blueprints, prepare);
            if (verify)
            {
                ProbeReloadedSprint17Venom(units, blueprints);
                foreach (var step in DestroySprint17PersistenceUnits(units, "cleanup")) yield return step;
            }
            bool preserved = unrelated.All(value => value.Unit.Destroyed == value.Destroyed &&
                ReferenceEquals(value.Unit.HoldingState, value.HoldingState) &&
                ReferenceEquals(value.Unit.Blueprint, value.Blueprint)) &&
                Game.Instance.Player.Party.SequenceEqual(party);
            SnakePersistenceCheck("unrelated-preserved", preserved,
                new JObject { ["preexistingUnits"] = unrelated.Length, ["preserved"] = preserved },
                "all preexisting non-fixture units and exact party membership remain intact");
            if (_snakePersistenceChecks.Any(value => value.Status != RuntimeTestStatuses.Pass))
            {
                CompleteSprint17Persistence(RuntimeTestStatuses.Fail, "A mandatory fixture assertion failed; no save was armed.");
                yield break;
            }
            BeginExpandedSummoningPersistenceSave();
        }

        private IEnumerable<int> DestroySprint17PersistenceUnits(UnitEntityData[] units, string stage)
        {
            var owned = units.Where(value => value.View != null)
                .Select(value => value.View.GetComponent<SerpentineVisualAttachment>()).Where(value => value != null)
                .SelectMany(value => value.CaptureOwnedResources().Concat(new UnityEngine.Object[] { value })).Distinct().ToArray();
            foreach (var unit in units) CleanupExpandedSummoningUnit(unit);
            for (int frame = 0; frame < 5; frame++) { Game.Instance.EntityDestroyer.Tick(); yield return 0; }
            bool gone = units.All(value => value.Destroyed && value.View == null && value.HoldingState == null) &&
                owned.All(value => value == null);
            SnakePersistenceCheck(stage + "-native-destruction", gone,
                new JObject { ["units"] = units.Length, ["capturedResources"] = owned.Length,
                    ["remainingResources"] = owned.Count(value => value != null),
                    ["remainingUnits"] = units.Count(value => !value.Destroyed || value.View != null || value.HoldingState != null) },
                "native destruction removes only receipt-owned units, views and captured private resources");
        }

        private static UnitEntityData SnakePersistenceRole(UnitEntityData[] units, string role)
        { return units.Single(value => value.Get<UnitPartSprint17PersistenceReceipt>()?.Role == role); }

        private void ArmSprint17Persistence(UnitEntityData[] units, BlueprintScriptableObject[] blueprints)
        {
            var viper = SnakePersistenceRole(units, "viper");
            var constrictor = SnakePersistenceRole(units, "constrictor-snake");
            var venomTarget = SnakePersistenceRole(units, "venom-target");
            var heldTarget = SnakePersistenceRole(units, "hold-target");
            var venom = blueprints.OfType<BlueprintBuff>().Single(value => value.name == "KMG_Summoning_Natural_Viper_Venom");
            var random = UnityEngine.Random.state;
            int fort = venomTarget.Descriptor.Stats.SaveFortitude.BaseValue;
            int viperBab = viper.Descriptor.Stats.BaseAttackBonus.BaseValue;
            int constrictorBab = constrictor.Descriptor.Stats.BaseAttackBonus.BaseValue;
            var exposure = new Sprint17PoisonExposureObserver { Owner = viper, Target = venomTarget, Venom = venom };
            EventBus.Subscribe(exposure);
            try
            {
                venomTarget.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                viper.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                PlaceExpandedSummoningUnit(venomTarget, viper.Position + Vector3.forward);
                RuleAttackWithWeapon attack;
                int damageBefore = venomTarget.Damage;
                var applied = DeliverSprint17Venom(viper, venomTarget, venom, out attack, exposure);
                var state = DescribeSprint17Venom(applied);
                var roll = attack.AttackRoll;
                int? wound = attack.MeleeDamage == null ? (int?)null : attack.MeleeDamage.Damage;
                state["attack"] = new JObject { ["hit"] = roll != null && roll.IsHit,
                    ["roll"] = roll == null ? null : roll.Roll.ToString(),
                    ["bonus"] = roll == null ? 0 : roll.AttackBonus, ["targetAC"] = roll == null ? 0 : roll.TargetAC,
                    ["autoHit"] = roll != null && roll.AutoHit, ["autoMiss"] = roll != null && roll.AutoMiss,
                    ["critical"] = roll != null && roll.IsCriticalConfirmed,
                    ["finalDamage"] = wound.HasValue ? (JToken)wound.Value : JValue.CreateNull(),
                    ["targetDamageBefore"] = damageBefore, ["targetDamageAfter"] = venomTarget.Damage,
                    ["targetDead"] = venomTarget.Descriptor.State.IsDead,
                    ["weapon"] = attack.Weapon == null ? null : attack.Weapon.Blueprint.AssetGuid,
                    ["ownerEnemy"] = viper.IsEnemy(venomTarget), ["targetEnemy"] = venomTarget.IsEnemy(viper),
                    ["liveConstitution"] = viper.Descriptor.Stats.Constitution.ModifiedValue,
                    ["liveConstitutionModifier"] = viper.Descriptor.Stats.Constitution.Bonus,
                    ["targetFortitude"] = venomTarget.Descriptor.Stats.SaveFortitude.ModifiedValue };
                state["nativeInjurySaves"] = exposure.InjurySaves.DeepClone();
                state["nativeBuffSaves"] = exposure.BuffSaves.DeepClone();
                state["nativeConstitutionDamage"] = exposure.Damage.DeepClone();
                state["armingObservation"] = SerpentinePersistenceReviewPolicy.ArmingObservation(
                    roll != null && roll.IsHit, wound, exposure.InjurySaves.Count,
                    exposure.InjurySaves.Count == 1 && (bool)exposure.InjurySaves[0]["passed"], applied != null);
                var receipt = venomTarget.Get<UnitPartSprint17PersistenceReceipt>();
                receipt.VenomDc = (int)state["dc"]; receipt.VenomTicks = (int)state["ticks"];
                receipt.VenomSaves = (int)state["saves"];
                receipt.ConstitutionDamage = venomTarget.Descriptor.Stats.Constitution.Damage;
                SnakePersistenceCheck("arm-venom", attack.AttackRoll.IsHit && attack.MeleeDamage != null &&
                    attack.MeleeDamage.Damage > 0 && applied != null &&
                    ReferenceEquals(applied.Context.MaybeCaster, viper) && receipt.VenomTicks == 1 &&
                    receipt.ConstitutionDamage >= 1 && receipt.ConstitutionDamage <= 2, state,
                    "one actual seeded wounding bite delivers an owned Viper poison before the native save");
                constrictor.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                PlaceExpandedSummoningUnit(heldTarget, constrictor.Position + Vector3.forward);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                var grab = SummonGrabComponent.Find(constrictor);
                bool grabbed = grab != null && grab.TryGrab(heldTarget, SummonLimbs.PrimaryWeapon(constrictor), true);
                SnakePersistenceCheck("arm-hold", grabbed && ReferenceEquals(SummonHoldComponent.HeldTarget(constrictor), heldTarget),
                    new JObject { ["grabbed"] = grabbed, ["owner"] = constrictor.UniqueId, ["target"] = heldTarget.UniqueId },
                    "known-hit request-local grab establishes one native hold; no hostile or party damage");
            }
            finally
            {
                EventBus.Unsubscribe(exposure);
                venomTarget.Descriptor.Stats.SaveFortitude.BaseValue = fort;
                viper.Descriptor.Stats.BaseAttackBonus.BaseValue = viperBab;
                constrictor.Descriptor.Stats.BaseAttackBonus.BaseValue = constrictorBab;
                UnityEngine.Random.state = random;
            }
        }

        private void InspectSprint17Persistence(UnitEntityData[] units, UnitEntityData caster,
            BlueprintScriptableObject[] blueprints, bool prepare)
        {
            bool identities = units.Length == 4 && units.All(value => SnakePersistenceOwns(value, caster, blueprints)) &&
                units.Select(value => value.Get<UnitPartSprint17PersistenceReceipt>().Role).OrderBy(value => value)
                    .SequenceEqual(SerpentinePersistenceReviewPolicy.Roles.OrderBy(value => value));
            bool duration = units.All(value => value.Descriptor.Buffs.Enumerable.Count(buff =>
                ReferenceEquals(buff.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) &&
                !buff.IsPermanent && buff.TimeLeft > TimeSpan.Zero && buff.TimeLeft.TotalSeconds <= 127d) == 1);
            SnakePersistenceCheck("identities-context-duration", identities && duration && units.All(value =>
                value.Commands != null && value.View != null && ReferenceEquals(value.View.Data, value) && !value.Descriptor.State.IsDead),
                new JObject { ["identities"] = identities, ["duration"] = duration, ["count"] = units.Length,
                    ["ids"] = new JArray(units.Select(value => value.UniqueId)) },
                "four exact marked identities, native summoner context, live controls and finite native duration");
            if (!identities) throw new InvalidOperationException("Incomplete or ambiguous snake persistence fixture.");
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                var unit = SnakePersistenceRole(units, key);
                var attached = unit.View.GetComponent<SerpentineVisualAttachment>();
                bool enabled = _context.FeatureModules.Active.ExpandedSummoning;
                string outcome = ExpandedSummoningSerpentineViewPatch.DescribeView(unit.View);
                bool visual = enabled ? attached != null && attached.OriginalBodyLive :
                    // This hook returns before recording an attempt when the
                    // module is off; do not borrow another family's status.
                    attached == null && outcome == "not-attempted" &&
                    unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(value =>
                        value.sharedMesh != null && value.sharedMesh.vertexCount > 0) &&
                    unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(value =>
                        value.sharedMesh == null || !value.sharedMesh.name.StartsWith("KMG_", StringComparison.Ordinal));
                int dexDelta = unit.Descriptor.Stats.Dexterity.Bonus - (key == "viper" ? 1 : 3);
                int perception = unit.Descriptor.Stats.GetStat(StatType.SkillPerception).ModifiedValue;
                int stealth = unit.Descriptor.Stats.GetStat(StatType.SkillStealth).ModifiedValue;
                int mobility = unit.Descriptor.Stats.GetStat(StatType.SkillMobility).ModifiedValue;
                bool skills = perception == (key == "viper" ? 9 : 12) &&
                    stealth == (key == "viper" ? 9 : 11) + (prepare ? dexDelta : 0) &&
                    mobility == (key == "viper" ? 9 : 15) + (prepare ? dexDelta : 0);
                SnakePersistenceCheck(key + "-visual-skills", visual && skills,
                    new JObject { ["phase"] = prepare ? "prepared" : "reloaded", ["visual"] = outcome,
                        ["originalBody"] = attached != null && attached.OriginalBodyLive, ["perception"] = perception,
                        ["stealth"] = stealth, ["mobility"] = mobility, ["liveDexterityDelta"] = dexDelta },
                    "module-appropriate automatically attached view and exact land skill totals (live grapple penalty only before save)");
            }
            var viper = SnakePersistenceRole(units, "viper");
            var target = SnakePersistenceRole(units, "venom-target");
            var venom = blueprints.OfType<BlueprintBuff>().Single(value => value.name == "KMG_Summoning_Natural_Viper_Venom");
            var applied = target.Descriptor.Buffs.GetBuff(venom);
            var state = DescribeSprint17Venom(applied);
            var receipt = target.Get<UnitPartSprint17PersistenceReceipt>();
            var logic = applied == null ? null : applied.Components.OfType<BuffPoisonStatDamage>().SingleOrDefault();
            int count = target.Descriptor.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, venom));
            bool poison = SerpentinePersistenceReviewPolicy.PreservedVenom(count, (int)state["dc"],
                (int)state["ticks"], (int)state["saves"], receipt.VenomDc, receipt.VenomTicks, receipt.VenomSaves) &&
                applied != null && ReferenceEquals(applied.Context.MaybeCaster, viper) && logic != null &&
                logic.Stat == StatType.Constitution && logic.Value.Rolls == 1 && logic.Value.Dice == Kingmaker.RuleSystem.DiceType.D2 &&
                logic.Ticks == 6 && logic.SuccesfullSaves == 1 &&
                target.Descriptor.Stats.Constitution.Damage == receipt.ConstitutionDamage;
            state["applications"] = count; state["savedTicks"] = receipt.VenomTicks;
            state["constitutionDamage"] = target.Descriptor.Stats.Constitution.Damage;
            state["savedConstitutionDamage"] = receipt.ConstitutionDamage;
            SnakePersistenceCheck("venom-state", poison, state, "one exact DC13 1d2 Constitution venom retains source, counters and damage across native save/load");
            if (!prepare)
            {
                _snakeLoadResetTrace?.Capture("reset-assertion", units);
                var grab = SummonGrabComponent.Find(SnakePersistenceRole(units, "constrictor-snake"));
                bool free = grab != null;
                var resetRows = new JArray();
                foreach (var unit in units)
                {
                    var initiator = unit.Get<UnitPartGrappleInitiator>();
                    var held = unit.Get<UnitPartGrappleTarget>();
                    var links = unit.Get<UnitPartSummonGrappleLinks>();
                    bool holdBuff = grab != null && unit.Descriptor.HasFact(grab.HoldBuff);
                    bool grappledBuff = grab != null && unit.Descriptor.HasFact(grab.GrappledBuff);
                    bool cantAct = unit.Descriptor.State.HasCondition(UnitCondition.CantAct);
                    bool cantMove = unit.Descriptor.State.HasCondition(UnitCondition.CantMove);
                    int storedLinks = links == null ? 0 : links.Count;
                    string[] failures = SerpentinePersistenceReviewPolicy.SessionResetFailures(
                        grab != null, initiator != null, held != null, storedLinks,
                        holdBuff, grappledBuff, cantAct, cantMove);
                    free = free && failures.Length == 0;
                    var initiatorTarget = initiator == null ? null : initiator.Target.Value;
                    var targetOwner = held == null ? null : held.Initiator.Value;
                    resetRows.Add(new JObject {
                        ["id"] = unit.UniqueId, ["role"] = unit.Get<UnitPartSprint17PersistenceReceipt>().Role,
                        ["free"] = failures.Length == 0, ["failures"] = new JArray(failures),
                        ["initiatorPart"] = initiator != null, ["targetPart"] = held != null,
                        ["initiatorTarget"] = initiatorTarget == null ? null : initiatorTarget.UniqueId,
                        ["targetOwner"] = targetOwner == null ? null : targetOwner.UniqueId,
                        ["linksPart"] = links != null, ["storedLinks"] = storedLinks,
                        ["holdBuff"] = holdBuff, ["grappledBuff"] = grappledBuff,
                        ["cantAct"] = cantAct, ["cantMove"] = cantMove,
                        ["canAct"] = unit.Descriptor.State.CanAct, ["canMove"] = unit.Descriptor.State.CanMove,
                        ["appearanceLock"] = unit.Descriptor.Buffs.GetBuff(
                            BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) != null,
                        ["buffs"] = new JArray(unit.Descriptor.Buffs.Enumerable.Select(value => new JObject {
                            ["name"] = value.Blueprint.name, ["guid"] = value.Blueprint.AssetGuid,
                            ["source"] = value.Context == null || value.Context.MaybeCaster == null ? null :
                                value.Context.MaybeCaster.UniqueId, ["permanent"] = value.IsPermanent,
                            ["secondsLeft"] = value.TimeLeft.TotalSeconds })) });
                }
                SnakePersistenceCheck("session-hold-reset", free, new JObject { ["free"] = free,
                    ["grabPresent"] = grab != null,
                    ["holdBlueprint"] = grab == null || grab.HoldBuff == null ? null : grab.HoldBuff.AssetGuid,
                    ["grappledBlueprint"] = grab == null || grab.GrappledBuff == null ? null : grab.GrappledBuff.AssetGuid,
                    ["units"] = resetRows },
                    "ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD: no restored link, stale buff or movement/action lock");
            }
        }

        private void ProbeReloadedSprint17Venom(UnitEntityData[] units, BlueprintScriptableObject[] blueprints)
        {
            var owner = SnakePersistenceRole(units, "viper");
            var target = SnakePersistenceRole(units, "venom-target");
            var venom = blueprints.OfType<BlueprintBuff>().Single(value => value.name == "KMG_Summoning_Natural_Viper_Venom");
            var buff = target.Descriptor.Buffs.GetBuff(venom);
            var before = DescribeSprint17Venom(buff);
            var observer = new Sprint17PoisonExposureObserver { Owner = owner, Target = target, Venom = venom };
            int fort = target.Descriptor.Stats.SaveFortitude.BaseValue, damage = target.Descriptor.Stats.Constitution.Damage;
            var random = UnityEngine.Random.state;
            EventBus.Subscribe(observer);
            try
            {
                target.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                DueSprint17OwnedBuff(target, buff);
                var after = DescribeSprint17Venom(target.Descriptor.Buffs.GetBuff(venom));
                int delta = target.Descriptor.Stats.Constitution.Damage - damage;
                bool continued = (int)after["ticks"] == (int)before["ticks"] + 1 && delta >= 1 && delta <= 2 &&
                    observer.Damage.Count == 1 && observer.BuffSaves.Count == 1 && observer.InjurySaves.Count == 0;
                target.Descriptor.Stats.SaveFortitude.BaseValue = 100;
                DueSprint17OwnedBuff(target, target.Descriptor.Buffs.GetBuff(venom));
                bool cured = !target.Descriptor.HasFact(venom) && observer.Damage.Count == 1 &&
                    observer.BuffSaves.Count == 2 && (bool)observer.BuffSaves.Last["passed"];
                SnakePersistenceCheck("venom-native-continuation-cure", continued && cured,
                    new JObject { ["before"] = before, ["afterOneExposure"] = after, ["damageDelta"] = delta,
                        ["nativeSaves"] = observer.BuffSaves, ["nativeDamage"] = observer.Damage, ["cured"] = cured },
                    "one subsequent owned-buff exposure, then one native successful Fortitude cure; no new bite or world-clock advance");
            }
            finally { EventBus.Unsubscribe(observer); target.Descriptor.Stats.SaveFortitude.BaseValue = fort; UnityEngine.Random.state = random; }
        }

        private void CompleteSprint17Persistence(string status, string warning)
        {
            if (_snakePersistencePause.HasValue) { Game.Instance.IsPaused = _snakePersistencePause.Value; _snakePersistencePause = null; }
            var evidence = _workingSaveSmoke.Stop();
            bool writes = _request.Scenario != RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyAbsent;
            _snakePersistenceChecks.Add(Assertion("exact-working-load", "one exact working descriptor, distinct baseline",
                "working=" + evidence.WorkingMatchCount + ";baseline=" + evidence.BaselineMatchCount,
                evidence.WorkingMatchCount == 1 && evidence.BaselineMatchCount == 1 && evidence.DescriptorReferenceCorrelated,
                "unchanged guarded working-save sentinel"));
            _snakePersistenceChecks.Add(Assertion("exact-working-save-write", writes ? "one native working save" : "no write",
                "count=" + evidence.ExpectedWorkingSaveRoutineCount + ";stashed=" + evidence.ExpectedWorkingStashedAreaCount,
                !evidence.SaveWritingApiObserved && evidence.ExpectedWorkingSaveRoutineCount == (writes ? 1 : 0) &&
                    (!writes || evidence.ExpectedWorkingStashedAreaCount >= 1), "exact descriptor-bound native save sentinel"));
            _snakePersistenceChecks.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion, "Unity Mod Manager loaded version"));
            if (status == RuntimeTestStatuses.Pass && _snakePersistenceChecks.Any(value => value.Status != RuntimeTestStatuses.Pass))
                status = RuntimeTestStatuses.Fail;
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory, "sprint17-snake-persistence.json"),
                _snakePersistenceRows.ToString(Formatting.Indented));
            var result = CreateResult(status, _snakePersistenceChecks, null);
            result.WorkingSaveSmoke = evidence;
            if (!string.IsNullOrWhiteSpace(warning)) result.Warnings.Add(warning);
            Complete(result);
        }
    }
}
