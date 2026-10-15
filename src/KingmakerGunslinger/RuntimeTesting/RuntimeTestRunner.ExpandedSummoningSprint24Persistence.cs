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
        // There is no separate active-mouth-hold actor: one Tyrannosaurus bite
        // cannot hold one victim while another is swallowed, because a swallowed
        // victim keeps that mouth shut. The active hold this fixture resets is
        // the owner's own residue from the grab that preceded the swallow.
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
                try
                {
                    // Order matters, and one mouth is the reason. SwallowHeld
                    // re-records the link and marks it engulfed, so a swallowed
                    // victim keeps this single bite shut until it is spat out:
                    // afterwards TryGrab sees limbBusy and any further bite is
                    // AutoMissed with its log suspended. So the confirmed
                    // critical is taken first, while the mouth is still free,
                    // and the grab/swallow is established last. A live mouth
                    // hold and an already-swallowed victim cannot coexist on one
                    // Tyrannosaurus, and this fixture no longer pretends they
                    // can: the grab that precedes the swallow leaves the owner's
                    // own hold residue, which is the active hold the load
                    // boundary has to clear.
                    bleeding = Sprint24CreateStateActor(caster, source,
                        Sprint24BleedingRole, bleed.AssetGuid,
                        Vector3.right * 3f);
                    swallowed = Sprint24CreateStateActor(caster, source,
                        Sprint24SwallowedRole, grab.SwallowedBuff.AssetGuid,
                        Vector3.forward * 3f);
                    swallowed.Descriptor.State.Size = Size.Huge;
                    source.Stats.AdditionalAttackBonus.BaseValue = 100;
                    source.Stats.AdditionalCMB.BaseValue = 100;

                    // Immune to maneuvers so the critical bite cannot also
                    // start a grab and shut the mouth before the swallow.
                    bleeding.Descriptor.State.AddCondition(
                        UnitCondition.ImmuneToCombatManeuvers, null);
                    UnityEngine.Random.InitState(
                        Sprint24FindD20PairSeed(19, 20));
                    RuleAttackWithWeapon critical = Rulebook.Trigger(
                        new RuleAttackWithWeapon(source, bleeding, bite, 0));
                    bleeding.Descriptor.State.RemoveConditionAll(
                        UnitCondition.ImmuneToCombatManeuvers);

                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    bool grabbed = grab.TryGrab(swallowed, bite, true);
                    if (!grabbed)
                        throw new InvalidOperationException(
                            "Sprint 24 persistence swallow hold did not establish.");
                    bool grappledTarget =
                        swallowed.Get<UnitPartGrappleTarget>() != null &&
                        swallowed.Descriptor.HasFact(grab.GrappledBuff);
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

                    Buff swallowedBuff = swallowed.Descriptor.Buffs
                        .GetBuff(grab.SwallowedBuff);
                    Buff bleedingBuff = bleeding.Descriptor.Buffs
                        .GetBuff(bleed);
                    // The owner's own session hold state after the swallow: the
                    // non-MultiLink release clears the victim's grappled part,
                    // never the initiator or the owner's hold buff.
                    bool ownerHoldResidue =
                        source.Get<UnitPartGrappleInitiator>() != null ||
                        source.Descriptor.HasFact(grab.HoldBuff);
                    bool mouthShut = SummonGrappleLinks.IsLimbOccupied(
                        source, bite);
                    bool exact = grabbed && grappledTarget &&
                        ownerHoldResidue && mouthShut &&
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
                            ["swallowedTarget"] = swallowed.UniqueId,
                            ["bleedingTarget"] = bleeding.UniqueId,
                            ["event"] =
                                "native-hold-swallowed-and-confirmed-bleed-before-save",
                            ["grabbed"] = grabbed,
                            ["grappledTarget"] = grappledTarget,
                            ["activeMouthHoldResidue"] = ownerHoldResidue,
                            ["engulfedMouthShut"] = mouthShut,
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
                        }, "Before the exact working-save write, the saved Tyrannosaurus owns an active mouth-hold residue, an engulfed-shut mouth, one already-swallowed native state and one confirmed-bite bleed.");
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

            if (actors.Length != 2 || sources.Length != 2)
                throw new InvalidOperationException(
                    "Sprint 24 cold load requires exactly two sources and two state actors.");
            UnitEntityData swallowedActor = actors.Single(value =>
                value.Get<UnitPartSprint24StateReceipt>().Role ==
                    Sprint24SwallowedRole);
            UnitEntityData bleedingActor = actors.Single(value =>
                value.Get<UnitPartSprint24StateReceipt>().Role ==
                    Sprint24BleedingRole);
            bool receipts = Sprint24OwnsStateActor(swallowedActor, source,
                    Sprint24SwallowedRole, grab.SwallowedBuff.AssetGuid) &&
                Sprint24OwnsStateActor(bleedingActor, source,
                    Sprint24BleedingRole, bleed.AssetGuid);
            // The whole accepted limitation, not a part of it: every KMG-owned
            // grab, hold, swallow, engulf and mouth occupancy is session-scoped
            // and the load boundary owes a clean reset with no lingering
            // condition, occupied slot or dangling link.
            bool holdReset =
                swallowedActor.Get<UnitPartGrappleTarget>() == null &&
                !swallowedActor.Descriptor.HasFact(grab.GrappledBuff) &&
                source.Get<UnitPartGrappleInitiator>() == null &&
                !source.Descriptor.HasFact(grab.HoldBuff) &&
                SummonHoldComponent.HeldTarget(source) == null;
            UnitPartSwallowed swallowedPart = swallowedActor
                .Get<UnitPartSwallowed>();
            UnitPartSwallowWhole swallower = source
                .Get<UnitPartSwallowWhole>();
            Buff swallowedState = swallowedActor.Descriptor.Buffs
                .GetBuff(grab.SwallowedBuff);
            bool swallowReset = swallowedPart == null &&
                (swallower == null ||
                    (swallower.SwallowedUnits.Count == 0 &&
                        !swallower.SwallowedUnits.Contains(swallowedActor))) &&
                swallowedState == null;
            // No dangling link: the engulfed mouth is free again, so the
            // reloaded swallower is a released, usable unit.
            bool mouthReleased = !SummonGrappleLinks.IsLimbOccupied(
                source, bite);
            // Bleeding Critical is the native Bleed2d6Buff, not a grapple
            // relationship, so it is the one state that legitimately survives.
            Buff bleedingState = bleedingActor.Descriptor.Buffs
                .GetBuff(bleed);
            bool bleedPreserved = bleedingState != null &&
                ReferenceEquals(bleedingState.MaybeContext.MaybeCaster,
                    source);
            RosterPersistenceCheck("sprint24-cold-state-boundary",
                receipts && holdReset && swallowReset && mouthReleased &&
                    bleedPreserved,
                new JObject {
                    ["unit"] = source.UniqueId,
                    ["event"] =
                        "fresh-process-load-resets-every-grapple-relationship-and-keeps-native-bleed",
                    ["moduleEnabled"] =
                        _context.FeatureModules.Active.ExpandedSummoning,
                    ["receiptsExact"] = receipts,
                    ["activeMouthHoldReset"] = holdReset,
                    ["alreadySwallowedReset"] = swallowReset,
                    ["engulfedMouthReleased"] = mouthReleased,
                    ["bleedingCriticalPreserved"] = bleedPreserved,
                    ["acceptedReloadLimit"] =
                        "ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD",
                    ["ownerDecisionUtc"] = "2026-10-15"
                }, "Cold load resets every KMG-owned grab, hold, swallow, engulf and mouth occupancy under the accepted limitation, leaving no dangling link; only the native Bleeding Critical payload survives, still owned by its exact source.");

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
            bool noOrphanInteriorPacket = false;
            bool bleedTick = false;
            bool bleedRemoved = false;
            try
            {
                TimeSpan now = Game.Instance.TimeController.GameTime;
                // The swallowed relationship was reset by the load boundary, so
                // the Tyrannosaurus-owned 2d8+11 interior packet must not fire
                // at all. That is the "no delayed damage, no orphan timer" half
                // of the accepted limitation, and it is asserted rather than
                // assumed: tick the former victim and require silence.
                swallowedActor.Descriptor.Buffs.UpdateNextEvent();
                swallowedActor.Descriptor.Buffs.Tick();
                swallowedActor.Descriptor.Buffs.Tick();
                noOrphanInteriorPacket = swallowedState == null &&
                    swallowedObserver.Damage.Count == 0;
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
                noOrphanInteriorPacket && bleedTick && bleedRemoved,
                new JObject {
                    ["unit"] = source.UniqueId,
                    ["event"] =
                        "loaded-native-periodic-payloads-and-healing",
                    ["moduleEnabled"] =
                        _context.FeatureModules.Active.ExpandedSummoning,
                    ["noOrphanInteriorPacket"] = noOrphanInteriorPacket,
                    ["bleed2d6Once"] = bleedTick,
                    ["bleedRemovedByHealing"] = bleedRemoved,
                    ["interiorLimit"] =
                        "SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED",
                    ["acceptedReloadLimit"] =
                        "ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD"
                }, "After cold load the reset swallow leaves no orphan timer and delivers no 2d8+11 interior packet, while the confirmed bleed still receives exactly one native 2d6 tick that healing removes.");

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
