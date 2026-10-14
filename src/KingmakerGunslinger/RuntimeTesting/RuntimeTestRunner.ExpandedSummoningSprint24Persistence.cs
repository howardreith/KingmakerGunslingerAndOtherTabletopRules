using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
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
    /// <summary>
    /// Request-owned victim receipt for Sprint 24's targeted working-save
    /// review. It identifies only three disposable actors and does not
    /// serialize or reconstruct a mouth-hold link.
    /// </summary>
    public sealed class UnitPartSprint24StateReceipt : UnitPart
    {
        [JsonProperty] public string Scope;
        [JsonProperty] public string Role;
        [JsonProperty] public string UnitId;
        [JsonProperty] public string SourceId;
        [JsonProperty] public string StateBuffGuid;
        [JsonProperty] public string PrepareRunId;
    }

    internal sealed partial class RuntimeTestRunner
    {
        private const string Sprint24StateScope =
            "KMG_Sprint24_Theropod_State_v1";
        private const string Sprint24HeldRole = "active-mouth-hold";
        private const string Sprint24SwallowedRole = "already-swallowed";
        private const string Sprint24BleedingRole = "bleeding-critical";
        private const string Sprint24NativeBleedGuid =
            "16249b8075ab8684ca105a78a047a5ef";

        private static UnitEntityData[] Sprint24StateActors(
            UnitEntityData caster)
        {
            IEnumerable<UnitEntityData> state = Game.Instance?.State?.Units?
                .All ?? Enumerable.Empty<UnitEntityData>();
            IEnumerable<UnitEntityData> area = caster?.HoldingState == null
                ? Enumerable.Empty<UnitEntityData>()
                : caster.HoldingState.AllEntityData.OfType<UnitEntityData>();
            return state.Concat(area).Where(value => value != null &&
                    value.Descriptor != null &&
                    value.Get<UnitPartSprint24StateReceipt>() != null)
                .Distinct().ToArray();
        }

        private UnitEntityData Sprint24CreateStateActor(
            UnitEntityData caster, UnitEntityData source, string role,
            string stateBuffGuid, Vector3 offset)
        {
            UnitEntityData actor = Game.Instance.EntityCreator.SpawnUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter,
                source.Position + offset, Quaternion.identity,
                caster.HoldingState);
            Game.Instance.EntityCreator.Tick();
            if (actor == null || actor.View == null ||
                !ReferenceEquals(actor.HoldingState, caster.HoldingState))
                throw new InvalidOperationException(
                    "Sprint 24 persistence victim did not enter the exact working area.");
            actor.Descriptor.Stats.HitPoints.BaseValue = 100000;
            SetExpandedSummoningBrainActive(actor, false);
            PlaceExpandedSummoningUnit(actor, actor.Position);
            if (!Game.Instance.State.AwakeUnits.Contains(actor))
                Game.Instance.State.AwakeUnits.Add(actor);
            UnitPartSprint24StateReceipt receipt =
                actor.Ensure<UnitPartSprint24StateReceipt>();
            receipt.Scope = Sprint24StateScope;
            receipt.Role = role;
            receipt.UnitId = actor.UniqueId;
            receipt.SourceId = source.UniqueId;
            receipt.StateBuffGuid = stateBuffGuid;
            receipt.PrepareRunId = _request.RunId;
            return actor;
        }

        private static bool Sprint24OwnsStateActor(UnitEntityData actor,
            UnitEntityData source, string role, string stateBuffGuid)
        {
            UnitPartSprint24StateReceipt receipt = actor == null ? null :
                actor.Get<UnitPartSprint24StateReceipt>();
            return receipt != null &&
                receipt.Scope == Sprint24StateScope &&
                receipt.Role == role &&
                receipt.UnitId == actor.UniqueId &&
                receipt.SourceId == source.UniqueId &&
                receipt.StateBuffGuid == stateBuffGuid &&
                !string.IsNullOrWhiteSpace(receipt.PrepareRunId) &&
                ReferenceEquals(actor.Blueprint,
                    BlueprintRoot.Instance.DefaultPlayerCharacter) &&
                ReferenceEquals(actor.HoldingState, source.HoldingState) &&
                !Game.Instance.Player.Party.Contains(actor);
        }

        private IEnumerable<int> Sprint24PersistentStates(
            UnitEntityData caster, UnitEntityData[] sources,
            bool prepare, bool verify)
        {
            UnitEntityData[] actors = Sprint24StateActors(caster);
            if (!prepare && !verify)
            {
                RosterPersistenceCheck("sprint24-fresh-state-absence",
                    actors.Length == 0,
                    new JObject {
                        ["event"] = "fresh-process-after-theropod-cleanup",
                        ["owner"] = Sprint24StateScope,
                        ["ownedVictims"] = actors.Length
                    }, "Fresh load finds no receipt-owned mouth-hold, swallowed or bleed victim fixture.");
                yield break;
            }

            UnitEntityData source = sources.SingleOrDefault(value =>
                value.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    TheropodRulesPolicy.TyrannosaurusKey);
            if (source == null)
                throw new InvalidOperationException(
                    "Sprint 24 persistence requires its exact saved Tyrannosaurus source.");
            SummonGrabComponent grab = SummonGrabComponent.Find(source);
            ItemEntityWeapon bite = source.Body?.PrimaryHand?.MaybeWeapon;
            BlueprintBuff bleed = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintBuff>().Single(value =>
                    value.AssetGuid == Sprint24NativeBleedGuid);
            if (grab == null || bite == null)
                throw new InvalidOperationException(
                    "Sprint 24 persistence source lacks its bite/grab carriers.");

            if (prepare)
            {
                if (actors.Length != 0 || sources.Length != 2)
                    throw new InvalidOperationException(
                        "Sprint 24 prepare requires two theropods and no prior state actors.");
                int attackBefore = source.Stats.AdditionalAttackBonus.BaseValue;
                int cmbBefore = source.Stats.AdditionalCMB.BaseValue;
                UnityEngine.Random.State randomBefore =
                    UnityEngine.Random.state;
                UnitEntityData swallowed = null;
                UnitEntityData bleeding = null;
                UnitEntityData held = null;
                try
                {
                    swallowed = Sprint24CreateStateActor(caster, source,
                        Sprint24SwallowedRole, grab.SwallowedBuff.AssetGuid,
                        Vector3.forward * 3f);
                    bleeding = Sprint24CreateStateActor(caster, source,
                        Sprint24BleedingRole, bleed.AssetGuid,
                        Vector3.right * 3f);
                    held = Sprint24CreateStateActor(caster, source,
                        Sprint24HeldRole, grab.GrappledBuff.AssetGuid,
                        Vector3.left * 3f);
                    swallowed.Descriptor.State.Size = Size.Huge;
                    held.Descriptor.State.Size = Size.Gargantuan;
                    source.Stats.AdditionalAttackBonus.BaseValue = 100;
                    source.Stats.AdditionalCMB.BaseValue = 100;

                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    if (!grab.TryGrab(swallowed, bite, true))
                        throw new InvalidOperationException(
                            "Sprint 24 persistence swallow hold did not establish.");
                    Buff heldForSwallow = SummonHoldComponent.HeldState(
                        source, swallowed, grab);
                    if (heldForSwallow == null)
                        throw new InvalidOperationException(
                            "Sprint 24 persistence swallow held state is absent.");
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    heldForSwallow.TickMechanics();
                    Buff ownerHold = source.Descriptor.Buffs.GetBuff(
                        grab.HoldBuff);
                    if (ownerHold == null)
                        throw new InvalidOperationException(
                            "Sprint 24 persistence swallow owner hold is absent.");
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    ownerHold.TickMechanics();

                    bleeding.Descriptor.State.AddCondition(
                        UnitCondition.ImmuneToCombatManeuvers, null);
                    UnityEngine.Random.InitState(
                        Sprint24FindD20PairSeed(19, 20));
                    RuleAttackWithWeapon critical = Rulebook.Trigger(
                        new RuleAttackWithWeapon(source, bleeding, bite, 0));
                    bleeding.Descriptor.State.RemoveConditionAll(
                        UnitCondition.ImmuneToCombatManeuvers);

                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool heldNow = grab.TryGrab(held, bite, true);
                    Buff swallowedBuff = swallowed.Descriptor.Buffs
                        .GetBuff(grab.SwallowedBuff);
                    Buff bleedingBuff = bleeding.Descriptor.Buffs
                        .GetBuff(bleed);
                    bool exact = heldNow &&
                        ReferenceEquals(SummonHoldComponent.HeldTarget(
                            source), held) &&
                        swallowed.Get<UnitPartSwallowed>() != null &&
                        ReferenceEquals(swallowed.Get<UnitPartSwallowed>()
                            .Swallower.Value, source) &&
                        source.Get<UnitPartSwallowWhole>() != null &&
                        source.Get<UnitPartSwallowWhole>().SwallowedUnits
                            .Contains(swallowed) &&
                        swallowedBuff != null &&
                        ReferenceEquals(swallowedBuff.MaybeContext
                            .MaybeCaster, source) &&
                        critical.AttackRoll != null &&
                        critical.AttackRoll.IsCriticalConfirmed &&
                        bleedingBuff != null &&
                        ReferenceEquals(bleedingBuff.MaybeContext
                            .MaybeCaster, source);
                    RosterPersistenceCheck(
                        "sprint24-state-fixture-prepared", exact,
                        new JObject {
                            ["unit"] = source.UniqueId,
                            ["heldTarget"] = held.UniqueId,
                            ["swallowedTarget"] = swallowed.UniqueId,
                            ["bleedingTarget"] = bleeding.UniqueId,
                            ["event"] =
                                "native-hold-swallowed-and-confirmed-bleed-before-save",
                            ["held"] = heldNow,
                            ["swallowed"] = swallowedBuff != null,
                            ["criticalNatural"] =
                                critical.AttackRoll?.Roll.Value,
                            ["criticalConfirmed"] =
                                critical.AttackRoll?.IsCriticalConfirmed,
                            ["bleeding"] = bleedingBuff != null,
                            ["acceptedReloadLimit"] =
                                "ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD",
                            ["interiorLimit"] =
                                "SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED"
                        }, "Before the exact working-save write, one active mouth hold, one already-swallowed native state and one confirmed-bite bleed are owned by the saved Tyrannosaurus.");
                }
                finally
                {
                    source.Stats.AdditionalAttackBonus.BaseValue =
                        attackBefore;
                    source.Stats.AdditionalCMB.BaseValue = cmbBefore;
                    UnityEngine.Random.state = randomBefore;
                }
                for (int frame = 0; frame < 3; frame++) yield return 0;
                yield break;
            }

            if (actors.Length != 3 || sources.Length != 2)
                throw new InvalidOperationException(
                    "Sprint 24 cold load requires exactly two sources and three state actors.");
            UnitEntityData heldActor = actors.Single(value =>
                value.Get<UnitPartSprint24StateReceipt>().Role ==
                    Sprint24HeldRole);
            UnitEntityData swallowedActor = actors.Single(value =>
                value.Get<UnitPartSprint24StateReceipt>().Role ==
                    Sprint24SwallowedRole);
            UnitEntityData bleedingActor = actors.Single(value =>
                value.Get<UnitPartSprint24StateReceipt>().Role ==
                    Sprint24BleedingRole);
            bool receipts = Sprint24OwnsStateActor(heldActor, source,
                    Sprint24HeldRole, grab.GrappledBuff.AssetGuid) &&
                Sprint24OwnsStateActor(swallowedActor, source,
                    Sprint24SwallowedRole, grab.SwallowedBuff.AssetGuid) &&
                Sprint24OwnsStateActor(bleedingActor, source,
                    Sprint24BleedingRole, bleed.AssetGuid);
            bool holdReset = heldActor.Get<UnitPartGrappleTarget>() == null &&
                !heldActor.Descriptor.HasFact(grab.GrappledBuff) &&
                source.Get<UnitPartGrappleInitiator>() == null &&
                !source.Descriptor.HasFact(grab.HoldBuff) &&
                SummonHoldComponent.HeldTarget(source) == null;
            UnitPartSwallowed swallowedPart = swallowedActor
                .Get<UnitPartSwallowed>();
            UnitPartSwallowWhole swallower = source
                .Get<UnitPartSwallowWhole>();
            Buff swallowedState = swallowedActor.Descriptor.Buffs
                .GetBuff(grab.SwallowedBuff);
            bool swallowedPreserved = swallowedPart != null &&
                ReferenceEquals(swallowedPart.Swallower.Value, source) &&
                swallower != null &&
                swallower.SwallowedUnits.Count == 1 &&
                swallower.SwallowedUnits.Contains(swallowedActor) &&
                swallowedState != null &&
                ReferenceEquals(swallowedState.MaybeContext.MaybeCaster,
                    source);
            Buff bleedingState = bleedingActor.Descriptor.Buffs
                .GetBuff(bleed);
            bool bleedPreserved = bleedingState != null &&
                ReferenceEquals(bleedingState.MaybeContext.MaybeCaster,
                    source);
            RosterPersistenceCheck("sprint24-cold-state-boundary",
                receipts && holdReset && swallowedPreserved &&
                    bleedPreserved,
                new JObject {
                    ["unit"] = source.UniqueId,
                    ["event"] =
                        "fresh-process-load-distinguishes-session-hold-from-native-states",
                    ["moduleEnabled"] =
                        _context.FeatureModules.Active.ExpandedSummoning,
                    ["receiptsExact"] = receipts,
                    ["activeMouthHoldReset"] = holdReset,
                    ["alreadySwallowedPreserved"] = swallowedPreserved,
                    ["bleedingCriticalPreserved"] = bleedPreserved,
                    ["acceptedReloadLimit"] =
                        "ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD"
                }, "Cold load resets only the un-restorable mouth hold; the same-source native swallowed and Bleeding Critical states remain intact with the module either ON or OFF.");

            PropertyInfo nextTick = typeof(Buff).GetProperty(
                "NextTickTime", BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (nextTick == null || !nextTick.CanWrite)
                throw new MissingMemberException(
                    "Native persistence Buff.NextTickTime setter is unavailable.");
            var swallowedObserver = new Sprint16RuleObserver {
                Owner = source, Target = swallowedActor };
            var bleedObserver = new Sprint16RuleObserver {
                Owner = source, Target = bleedingActor };
            EventBus.Subscribe(swallowedObserver);
            EventBus.Subscribe(bleedObserver);
            bool swallowedTick = false;
            bool bleedTick = false;
            bool bleedRemoved = false;
            try
            {
                TimeSpan now = Game.Instance.TimeController.GameTime;
                int swallowedRound = swallowedState == null ? -1 :
                    swallowedState.RoundNumber;
                if (swallowedState != null)
                {
                    nextTick.SetValue(swallowedState, now, null);
                    swallowedActor.Descriptor.Buffs.UpdateNextEvent();
                    swallowedActor.Descriptor.Buffs.Tick();
                    RuleDealDamage damage = swallowedObserver.Damage
                        .SingleOrDefault();
                    BaseDamage chunk = damage == null ? null :
                        damage.DamageBundle.SingleOrDefault();
                    swallowedActor.Descriptor.Buffs.Tick();
                    swallowedTick = damage != null && chunk != null &&
                        damage.AttackRoll == null &&
                        ReferenceEquals(damage.Initiator, source) &&
                        chunk.Dice.Rolls == 2 &&
                        chunk.Dice.Dice == DiceType.D8 &&
                        chunk.Bonus == 11 &&
                        swallowedObserver.Damage.Count == 1 &&
                        swallowedState.RoundNumber == swallowedRound + 1;
                }
                int bleedRound = bleedingState == null ? -1 :
                    bleedingState.RoundNumber;
                if (bleedingState != null)
                {
                    nextTick.SetValue(bleedingState, now, null);
                    bleedingActor.Descriptor.Buffs.UpdateNextEvent();
                    bleedingActor.Descriptor.Buffs.Tick();
                    RuleDealDamage damage = bleedObserver.Damage
                        .SingleOrDefault();
                    BaseDamage chunk = damage == null ? null :
                        damage.DamageBundle.SingleOrDefault();
                    bleedingActor.Descriptor.Buffs.Tick();
                    bleedTick = damage != null && chunk != null &&
                        damage.AttackRoll == null &&
                        chunk.Dice.Rolls == 2 &&
                        chunk.Dice.Dice == DiceType.D6 &&
                        chunk.Bonus == 0 &&
                        bleedObserver.Damage.Count == 1 &&
                        bleedingState.RoundNumber == bleedRound + 1;
                    bleedingActor.Descriptor.Damage = Math.Max(20,
                        bleedingActor.Descriptor.Damage);
                    RuleHealDamage healing = Rulebook.Trigger(
                        new RuleHealDamage(source, bleedingActor,
                            new DiceFormula(0, DiceType.D6), 5));
                    bleedRemoved = healing.Value > 0 &&
                        !bleedingActor.Descriptor.HasFact(bleed);
                }
            }
            finally
            {
                EventBus.Unsubscribe(swallowedObserver);
                EventBus.Unsubscribe(bleedObserver);
            }
            RosterPersistenceCheck("sprint24-cold-state-behavior",
                swallowedTick && bleedTick && bleedRemoved,
                new JObject {
                    ["unit"] = source.UniqueId,
                    ["event"] =
                        "loaded-native-periodic-payloads-and-healing",
                    ["moduleEnabled"] =
                        _context.FeatureModules.Active.ExpandedSummoning,
                    ["swallowed2d8Plus11Once"] = swallowedTick,
                    ["bleed2d6Once"] = bleedTick,
                    ["bleedRemovedByHealing"] = bleedRemoved,
                    ["interiorLimit"] =
                        "SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED"
                }, "After cold load, the preserved swallowed victim receives one Tyrannosaurus-owned 2d8+11 packet, the confirmed bleed receives one native 2d6 tick, and healing removes only the bleed.");

            if (swallower != null && swallowedActor
                .Get<UnitPartSwallowed>() != null)
                swallower.Free(swallowedActor);
            foreach (UnitEntityData actor in actors)
                if (!actor.Destroyed) actor.Destroy();
            Game.Instance.EntityDestroyer.Tick();
            for (int frame = 0; frame < 30; frame++) yield return 0;
            bool cleaned = actors.All(value => value.Destroyed) &&
                Sprint24StateActors(caster).Length == 0 &&
                source.Get<UnitPartGrappleInitiator>() == null &&
                (source.Get<UnitPartSwallowWhole>() == null ||
                    source.Get<UnitPartSwallowWhole>().SwallowedUnits.Count ==
                        0);
            RosterPersistenceCheck("sprint24-state-cleanup", cleaned,
                new JObject {
                    ["unit"] = source.UniqueId,
                    ["event"] = "exact-receipt-owned-victim-cleanup",
                    ["destroyedVictims"] = actors.Count(value =>
                        value.Destroyed),
                    ["remainingReceipts"] =
                        Sprint24StateActors(caster).Length,
                    ["sourceHoldPart"] =
                        source.Get<UnitPartGrappleInitiator>() != null,
                    ["sourceSwallowedCount"] =
                        source.Get<UnitPartSwallowWhole>() == null ? 0 :
                        source.Get<UnitPartSwallowWhole>()
                            .SwallowedUnits.Count
                }, "Only the three receipt-owned victims are retired, leaving no mouth hold, swallowed victim, bleed fixture or timer before the cleanup save.");
        }
    }
}
