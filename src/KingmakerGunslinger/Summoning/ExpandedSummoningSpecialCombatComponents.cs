using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Units;
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
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class BebelithCombatComponent :
        RuleInitiatorLogicComponent<RuleAttackRoll>,
        IInitiatorRulebookHandler<RuleCalculateWeaponStats>,
        IInitiatorRulebookHandler<RuleAttackWithWeapon>, ITickEachRound
    {
        private static readonly ConditionalWeakTable<UnitEntityData, OwnerState>
            States = new ConditionalWeakTable<UnitEntityData, OwnerState>();

        public BlueprintItemWeapon Claw;
        public BlueprintItemWeapon Bite;
        public BlueprintUnitFact OutsiderType;
        public BlueprintBuff DismantledArmor;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || !IsNaturalWeapon(evt.Weapon == null ? null :
                    evt.Weapon.Blueprint) || !IsDemon(evt.Target)) return;
            evt.SetAttackBonusPenalty(evt.AttackBonusPenalty -
                ExpandedSummoningSpecialProfiles.BebelithDemonHunterBonus);
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt == null || evt.AttackWithWeapon == null ||
                !IsNaturalWeapon(evt.Weapon == null ? null : evt.Weapon.Blueprint) ||
                !IsDemon(evt.AttackWithWeapon.Target)) return;
            evt.AddBonusDamage(
                ExpandedSummoningSpecialProfiles.BebelithDemonHunterBonus);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || !evt.AttackRoll.IsHit ||
                evt.Target == null || evt.Weapon == null ||
                !ReferenceEquals(evt.Weapon.Blueprint, Claw) || Owner == null ||
                Owner.Unit == null || DismantledArmor == null) return;
            bool hasArmor = evt.Target.Body != null &&
                evt.Target.Body.Armor != null && evt.Target.Body.Armor.HasArmor;
            bool attempt;
            lock (States)
            {
                OwnerState state = States.GetOrCreateValue(Owner.Unit);
                int priorHits = state.HitCount(evt.Target);
                attempt = ExpandedSummoningSpecialProfiles
                    .ShouldAttemptBebelithDismantle(true, true, hasArmor,
                        priorHits, state.WasAttempted(evt.Target));
                state.RecordHit(evt.Target);
                if (attempt) state.MarkAttempted(evt.Target);
            }
            if (!attempt) return;
            var saving = new RuleSavingThrow(evt.Target,
                SavingThrowType.Reflex,
                ExpandedSummoningSpecialProfiles.BebelithDismantleReflexDc);
            Rulebook.Trigger(saving);
            if (saving.IsPassed || evt.Target.Descriptor.HasFact(
                    DismantledArmor)) return;
            evt.Target.Descriptor.Buffs.AddBuff(DismantledArmor,
                Fact == null ? null : Fact.MaybeContext,
                TimeSpan.FromSeconds(6d * ExpandedSummoningSpecialProfiles
                    .BebelithDismantleRounds));
        }

        public void OnNewRound()
        {
            if (Owner == null || Owner.Unit == null) return;
            lock (States) { States.Remove(Owner.Unit); }
        }

        private bool IsNaturalWeapon(BlueprintItemWeapon weapon)
        { return weapon != null && (ReferenceEquals(weapon, Claw) ||
            ReferenceEquals(weapon, Bite)); }

        private bool IsDemon(UnitEntityData target)
        {
            return target != null && target.Descriptor != null &&
                OutsiderType != null && target.Descriptor.HasFact(OutsiderType) &&
                target.Descriptor.Alignment != null &&
                ExpandedSummoningSpecialProfiles.IsBebelithDemonHuntingTarget(
                    true, (int)target.Descriptor.Alignment.Value);
        }

        private sealed class OwnerState
        {
            private readonly Dictionary<UnitEntityData, TargetState> _targets =
                new Dictionary<UnitEntityData, TargetState>(
                    ReferenceComparer.Instance);

            internal int HitCount(UnitEntityData target)
            { TargetState value; return _targets.TryGetValue(target, out value) ?
                value.Hits : 0; }

            internal bool WasAttempted(UnitEntityData target)
            { TargetState value; return _targets.TryGetValue(target, out value) &&
                value.Attempted; }

            internal void RecordHit(UnitEntityData target)
            { Get(target).Hits++; }

            internal void MarkAttempted(UnitEntityData target)
            { Get(target).Attempted = true; }

            private TargetState Get(UnitEntityData target)
            {
                TargetState value;
                if (!_targets.TryGetValue(target, out value))
                    _targets.Add(target, value = new TargetState());
                return value;
            }
        }

        private sealed class TargetState
        { internal int Hits; internal bool Attempted; }

        private sealed class ReferenceComparer : IEqualityComparer<UnitEntityData>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public bool Equals(UnitEntityData left, UnitEntityData right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(UnitEntityData value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }

    /// <summary>
    /// Cyclops Flash of Insight (Sprint 3; rebuilt under the correction
    /// order). The tabletop ability chooses the result of one die roll
    /// before it is rolled; here the chosen roll is the next attack's own
    /// d20, and the chosen result is 20. Lives on the one-round armed state
    /// the ability applies. When the cyclops's own attack roll is about to
    /// trigger, the state arms exactly that attack's first d20; when that
    /// RuleRollD20 is about to trigger, its pre-rolled result becomes 20
    /// (the game's own pre-roll seam, read by Roll()) and the arming is
    /// consumed - so the attack rule sees a natural 20 (a hit, a threat
    /// inside any range) and rolls the critical confirmation normally on a
    /// second, untouched d20. A roll the cyclops merely witnesses, a roll
    /// of another kind, or a later attack never sees the arming: the
    /// initiator-scoped handler, the per-attack arming and the native
    /// RemoveBuffOnAttack on the same state bound it to one attack per use,
    /// and the arming is transient, never saved.
    /// </summary>
    [Serializable]
    public sealed class CyclopsFlashOfInsightComponent :
        RuleInitiatorLogicComponent<RuleAttackRoll>,
        IInitiatorRulebookHandler<RuleRollD20>
    {
        private sealed class Arming { internal RuleAttackRoll Attack; }

        private static readonly ConditionalWeakTable<UnitEntityData, Arming> Armings =
            new ConditionalWeakTable<UnitEntityData, Arming>();
        private static readonly System.Reflection.FieldInfo PreRolledResult =
            typeof(RuleRollD20).GetField("m_PreRolledResult",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

        internal const int ChosenResult = 20;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null) return;
            if (!ExpandedSummoningSpecialProfiles.ShouldApplyFlashOfInsight(
                    ReferenceEquals(evt.Initiator, Owner.Unit), Fact != null))
                return;
            Arming arming;
            if (Armings.TryGetValue(Owner.Unit, out arming)) arming.Attack = evt;
            else Armings.Add(Owner.Unit, new Arming { Attack = evt });
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt)
        {
            // Whatever the roll did, the arming never outlives its attack.
            if (Owner != null && Owner.Unit != null) Armings.Remove(Owner.Unit);
        }

        public void OnEventAboutToTrigger(RuleRollD20 evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null ||
                !ReferenceEquals(evt.Initiator, Owner.Unit) || PreRolledResult == null)
                return;
            Arming arming;
            if (!Armings.TryGetValue(Owner.Unit, out arming) || arming.Attack == null) return;
            // The first d20 after arming is the attack roll itself; the
            // confirmation that may follow is a later RuleRollD20 and finds
            // the arming consumed.
            PreRolledResult.SetValue(evt, (int?)ChosenResult);
            Armings.Remove(Owner.Unit);
        }

        public void OnEventDidTrigger(RuleRollD20 evt) { }

        /// <summary>For the runtime fixture: whether an arming is pending for this unit.</summary>
        internal static bool IsArmed(UnitEntityData unit)
        {
            Arming arming;
            return unit != null && Armings.TryGetValue(unit, out arming) && arming.Attack != null;
        }
    }

    /// <summary>Which limb an attack came from, by slot identity.</summary>
    internal enum SummonLimbKind { None, PrimaryHand, Additional }

    /// <summary>
    /// Attack identity for the grapple and rake contracts. A limb is the slot
    /// an attack's weapon entity sits in - the primary hand (a bite, a slam)
    /// or one of the body's additional limbs, where the game lists secondary
    /// limbs after the additional ones - never the weapon blueprint, which
    /// primary claws and rake claws share.
    /// </summary>
    internal static class SummonLimbs
    {
        internal static SummonLimbKind Classify(UnitEntityData owner, ItemEntityWeapon weapon,
            out int additionalIndex)
        {
            additionalIndex = -1;
            if (owner == null || owner.Body == null || weapon == null) return SummonLimbKind.None;
            if (owner.Body.PrimaryHand != null &&
                ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon, weapon))
                return SummonLimbKind.PrimaryHand;
            List<Kingmaker.Items.Slots.WeaponSlot> limbs = owner.Body.AdditionalLimbs;
            if (limbs == null) return SummonLimbKind.None;
            for (int index = 0; index < limbs.Count; index++)
                if (limbs[index] != null && ReferenceEquals(limbs[index].MaybeWeapon, weapon))
                {
                    additionalIndex = index;
                    return SummonLimbKind.Additional;
                }
            return SummonLimbKind.None;
        }

        internal static int AdditionalLimbCount(UnitEntityData owner)
        {
            return owner == null || owner.Body == null || owner.Body.AdditionalLimbs == null
                ? 0 : owner.Body.AdditionalLimbs.Count;
        }

        internal static bool IsRakeSlot(UnitEntityData owner, Kingmaker.Items.Slots.WeaponSlot slot,
            int rakeLimbCount)
        {
            if (owner == null || owner.Body == null || slot == null ||
                owner.Body.AdditionalLimbs == null) return false;
            int index = owner.Body.AdditionalLimbs.IndexOf(slot);
            return index >= 0 && ExpandedSummoningSpecialProfiles.IsRakeSlot(index,
                owner.Body.AdditionalLimbs.Count, rakeLimbCount);
        }

        internal static bool IsRakeLimb(UnitEntityData owner, ItemEntityWeapon weapon,
            int rakeLimbCount)
        {
            int index;
            return Classify(owner, weapon, out index) == SummonLimbKind.Additional &&
                ExpandedSummoningSpecialProfiles.IsRakeSlot(index, AdditionalLimbCount(owner),
                    rakeLimbCount);
        }

        /// <summary>The bite: the primary hand's weapon entity, or null.</summary>
        internal static ItemEntityWeapon PrimaryWeapon(UnitEntityData owner)
        {
            return owner == null || owner.Body == null || owner.Body.PrimaryHand == null
                ? null : owner.Body.PrimaryHand.MaybeWeapon;
        }
    }

    /// <summary>
    /// The in-memory side of a hold link: the limb that established it, keyed
    /// by the held unit's grappled-state buff instance. Buff instances are the
    /// durable state (they save and load); after a load this table is empty
    /// and the holder's first grab limb stands in for the maintain damage.
    /// </summary>
    internal static class SummonGrappleLinks
    {
        private sealed class LinkInfo { internal ItemEntityWeapon Weapon; }

        private static readonly ConditionalWeakTable<Buff, LinkInfo> Links =
            new ConditionalWeakTable<Buff, LinkInfo>();

        internal static void Record(Buff heldState, ItemEntityWeapon weapon)
        {
            if (heldState == null) return;
            LinkInfo info;
            if (Links.TryGetValue(heldState, out info)) info.Weapon = weapon;
            else Links.Add(heldState, new LinkInfo { Weapon = weapon });
        }

        internal static ItemEntityWeapon EstablishingWeapon(Buff heldState)
        {
            LinkInfo info;
            return heldState != null && Links.TryGetValue(heldState, out info) ? info.Weapon : null;
        }
    }

    /// <summary>
    /// The damage of an attack, as the game computes it: the weapon entity's
    /// dice and type, the Strength contribution for that limb, enhancement
    /// and every weapon-stats modifier, dealt as one damage rule.
    /// </summary>
    internal static class SummonGrappleDamage
    {
        internal static int DealWeaponDamage(UnitEntityData owner, UnitEntityData target,
            ItemEntityWeapon weapon, MechanicsContext context)
        {
            if (owner == null || target == null || weapon == null) return 0;
            var stats = new RuleCalculateWeaponStats(owner, weapon, null);
            if (context != null) context.TriggerRule(stats);
            else Rulebook.Trigger(stats);
            var damages = new List<BaseDamage>();
            if (stats.DamageDescription != null)
                foreach (DamageDescription description in stats.DamageDescription)
                    if (description != null) damages.Add(description.CreateDamage());
            if (damages.Count == 0) return 0;
            var bundle = new DamageBundle(damages.ToArray());
            bundle.Weapon = weapon;
            var rule = new RuleDealDamage(owner, target, bundle);
            if (context != null) context.TriggerRule(rule);
            else Rulebook.Trigger(rule);
            return rule.Damage;
        }
    }

    /// <summary>
    /// Shared summon grab (Sprint 4; rebuilt under the 2026-09-25 correction
    /// order around attack identity, target identity and the universal grab
    /// rule). A grab starts only from a hit with one of the owner's grab
    /// limbs - the primary hand (a bite) and/or the first additional limbs
    /// (foreclaws, slams, bites), never a rake claw - against a target no
    /// larger than the owner (explicit exceptions per creature), through the
    /// game's own grapple check with the tabletop +4 (ManeuverBonus on the
    /// same traits, so it reaches start and maintain alike). A single-link
    /// holder carries the native initiator and target parts, and the game's
    /// grapple controller then drives break-free and reach; the Giant
    /// Flytrap holds one target per bite through the project multi-link
    /// state (its MultiHold buff and one held-state buff per target, whose
    /// context names the holder). A swallower never swallows on the grab:
    /// that is the later turn's maintain check (SummonHoldComponent).
    /// </summary>
    [Serializable]
    public sealed class SummonGrabComponent :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public bool GrabWithPrimaryHand;
        public int GrabAdditionalLimbCount;
        /// <summary>The last N additional limbs are rake claws: they never grab and strike only on a charge or against the held foe.</summary>
        public int RakeLimbCount;
        /// <summary>0: same size or smaller (the universal rule); a creature whose stat block says otherwise sets its exception here.</summary>
        public int MaxTargetSizeDelta;
        /// <summary>1 for every holder but the Flytrap (one per bite, four).</summary>
        public int MaxHeldTargets = 1;
        public BlueprintBuff HoldBuff;
        public BlueprintBuff GrappledBuff;
        /// <summary>Set only for a swallower (or the Flytrap's engulf): applied on a later turn's successful check.</summary>
        public BlueprintBuff SwallowedBuff;
        public bool SwallowMaxSizeIsAbsolute;
        public Size SwallowMaxSize;
        /// <summary>-1: up to one size smaller than the swallower (the universal rule).</summary>
        public int SwallowMaxSizeDelta = -1;
        public int ConstrictDiceCount;
        public DiceType ConstrictDiceType;
        public int ConstrictBonus;

        internal bool MultiLink { get { return MaxHeldTargets > 1; } }

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || evt.Weapon == null ||
                evt.Target == null) return;
            TryGrab(evt.Target, evt.Weapon, evt.AttackRoll.IsHit);
        }

        /// <summary>
        /// The owner's live grab component - the instance on its buff, which
        /// knows its owner and its fact; the blueprint's own instance only as
        /// a last resort for configuration reads.
        /// </summary>
        internal static SummonGrabComponent Find(UnitEntityData owner)
        {
            if (owner == null || owner.Descriptor == null) return null;
            foreach (Buff buff in owner.Descriptor.Buffs.RawFacts.OfType<Buff>())
            {
                if (buff == null || buff.Blueprint == null) continue;
                SummonGrabComponent live = buff.Components == null ? null :
                    buff.Components.OfType<SummonGrabComponent>().FirstOrDefault();
                if (live != null) return live;
                if (buff.Blueprint.ComponentsArray == null) continue;
                SummonGrabComponent grab = buff.Blueprint.ComponentsArray
                    .OfType<SummonGrabComponent>().FirstOrDefault();
                if (grab != null) return grab;
            }
            return null;
        }

        internal bool IsGrabLimb(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            int index;
            SummonLimbKind kind = SummonLimbs.Classify(owner, weapon, out index);
            return ExpandedSummoningSpecialProfiles.IsGrabLimb(kind == SummonLimbKind.PrimaryHand,
                kind == SummonLimbKind.Additional ? index : -1, GrabWithPrimaryHand,
                GrabAdditionalLimbCount, SummonLimbs.AdditionalLimbCount(owner), RakeLimbCount);
        }

        /// <summary>The first grab limb's weapon entity (the stand-in for damage after a load).</summary>
        internal ItemEntityWeapon FirstGrabWeapon(UnitEntityData owner)
        {
            if (owner == null || owner.Body == null) return null;
            if (GrabWithPrimaryHand) return SummonLimbs.PrimaryWeapon(owner);
            List<Kingmaker.Items.Slots.WeaponSlot> limbs = owner.Body.AdditionalLimbs;
            if (limbs == null) return null;
            for (int index = 0; index < limbs.Count && index < GrabAdditionalLimbCount; index++)
                if (limbs[index] != null && limbs[index].MaybeWeapon != null)
                    return limbs[index].MaybeWeapon;
            return null;
        }

        internal bool IsSwallowSizeAllowed(UnitEntityData owner, UnitEntityData target)
        {
            return owner != null && target != null &&
                ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(
                    (int)target.Descriptor.State.Size, (int)owner.Descriptor.State.Size,
                    SwallowMaxSizeIsAbsolute, (int)SwallowMaxSize, SwallowMaxSizeDelta);
        }

        /// <summary>The links this owner holds now: single (native parts) or multi (held-state buffs naming it).</summary>
        internal int HeldCount(UnitEntityData owner)
        {
            if (owner == null) return 0;
            if (!MultiLink)
            {
                UnitPartSwallowWhole swallower = owner.Get<UnitPartSwallowWhole>();
                bool swallowed = swallower != null && swallower.SwallowedUnits != null &&
                    swallower.SwallowedUnits.Any(value => value.Value != null);
                return owner.Get<UnitPartGrappleInitiator>() != null || swallowed ? 1 : 0;
            }
            UnitPartSwallowWhole part = owner.Get<UnitPartSwallowWhole>();
            int engulfed = part == null || part.SwallowedUnits == null ? 0 :
                part.SwallowedUnits.Count(value => value.Value != null);
            return SummonMultiHoldComponent.HeldTargets(owner, GrappledBuff).Count + engulfed;
        }

        /// <summary>
        /// The whole decision and its consequences, callable by the guarded
        /// runtime fixture with a known hit and the exact limb weapon entity
        /// so the hold can be proven without a second attack roll.
        /// </summary>
        internal bool TryGrab(UnitEntityData target, ItemEntityWeapon weapon, bool isHit)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (owner == null || target == null || target.Descriptor == null)
                return false;
            bool sizeAllowed = ExpandedSummoningSpecialProfiles.IsGrabSizeAllowed(
                (int)target.Descriptor.State.Size, (int)owner.Descriptor.State.Size,
                MaxTargetSizeDelta);
            bool targetHeld = target.Get<UnitPartGrappleTarget>() != null ||
                SummonHeldComponent.HolderOf(target, GrappledBuff) != null;
            // One link per limb: a bite that already holds someone cannot
            // take a second foe (the flytrap's four bites, one each).
            bool limbBusy = MultiLink && SummonMultiHoldComponent.HeldTargets(owner, GrappledBuff)
                .Any(held => ReferenceEquals(SummonGrappleLinks.EstablishingWeapon(
                    SummonHoldComponent.HeldState(owner, held, this)), weapon));
            if (!ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(isHit,
                    IsGrabLimb(owner, weapon), HeldCount(owner) >= MaxHeldTargets || limbBusy,
                    targetHeld, target.Get<UnitPartSwallowed>() != null,
                    ReferenceEquals(owner, target), sizeAllowed))
                return false;
            if (HoldBuff == null || GrappledBuff == null) return false;
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            var maneuver = new RuleCombatManeuver(owner, target,
                CombatManeuver.Grapple);
            if (context != null) context.TriggerRule(maneuver);
            else Rulebook.Trigger(maneuver);
            if (!maneuver.Success) return false;
            Buff heldState;
            if (!MultiLink)
            {
                owner.Ensure<UnitPartGrappleInitiator>().Init(target, HoldBuff, context);
                target.Ensure<UnitPartGrappleTarget>().Init(owner, GrappledBuff, context);
                heldState = target.Descriptor.Buffs.GetBuff(GrappledBuff);
            }
            else
            {
                if (owner.Descriptor.Buffs.GetBuff(HoldBuff) == null)
                    owner.Descriptor.Buffs.AddBuff(HoldBuff, context, null);
                heldState = target.Descriptor.Buffs.AddBuff(GrappledBuff,
                    new MechanicsContext(owner, target.Descriptor, GrappledBuff, context,
                        target), null);
            }
            SummonGrappleLinks.Record(heldState, weapon);
            DealConstrict(owner, target, context);
            return true;
        }

        internal void DealConstrict(UnitEntityData owner, UnitEntityData target,
            MechanicsContext context)
        {
            if (ConstrictDiceCount <= 0 || owner == null || target == null) return;
            var damage = new PhysicalDamage(new DiceFormula(ConstrictDiceCount,
                ConstrictDiceType), PhysicalDamageForm.Bludgeoning);
            damage.AddBonus(ConstrictBonus);
            var rule = new RuleDealDamage(owner, target, damage);
            if (context != null) context.TriggerRule(rule);
            else Rulebook.Trigger(rule);
        }

        /// <summary>
        /// The later-turn check for a swallower that began the round holding:
        /// the maintain check succeeded and is used as though attempting to
        /// pin; the target is swallowed (engulfed) and takes the bite's
        /// damage; the hold link ends because the swallowed state supersedes
        /// it. Returns the bite damage dealt.
        /// </summary>
        internal int SwallowHeld(UnitEntityData owner, UnitEntityData target,
            MechanicsContext context)
        {
            if (owner == null || target == null || SwallowedBuff == null) return 0;
            int damage = SummonGrappleDamage.DealWeaponDamage(owner, target,
                SummonLimbs.PrimaryWeapon(owner), context);
            if (target.Descriptor.State.IsDead || target.Destroyed) return damage;
            SummonHoldComponent.ReleaseLink(owner, target, this, false);
            owner.Ensure<UnitPartSwallowWhole>().Swallow(target, SwallowedBuff);
            return damage;
        }
    }

    /// <summary>
    /// The holder's side of a single-link hold (Sprint 4), carried by the hold
    /// buff the native initiator part applies. Each new round the holder
    /// makes a grapple check to maintain at the tabletop +5 (on top of the
    /// +4 grab bonus): success deals the damage of the attack that
    /// established the hold - the establishing limb's weapon entity through
    /// the game's own weapon-stats rule, the first grab limb after a load -
    /// plus any constrict; a swallower that began the round holding uses the
    /// successful check as though attempting to pin and swallows instead
    /// (bite damage, swallowed state); failure releases. When the hold state
    /// ends for any reason - the target broke free, the holder fell, the
    /// summon expired or was dismissed, the buff was dispelled - the target
    /// this summon holds is released too, and only that target: the link is
    /// owned here and never inferred from the target's side.
    /// </summary>
    [Serializable]
    public sealed class SummonHoldComponent : BuffLogic, ITickEachRound,
        IInitiatorRulebookHandler<RuleCalculateCMB>
    {
        public void OnNewRound()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData target = HeldTarget(owner);
            if (target == null) return;
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            MaintainLink(owner, target, grab, context, Buff, HeldState(owner, target, grab));
        }

        /// <summary>
        /// One maintain check for one link, shared by the single and the
        /// multi-link holders. Returns what happened, for the runtime fixture:
        /// "released", "maintained:N" (damage) or "swallowed:N".
        /// </summary>
        internal static string MaintainLink(UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab, MechanicsContext context, Buff holdBuff, Buff heldState)
        {
            var maneuver = new RuleCombatManeuver(owner, target, CombatManeuver.Grapple);
            if (context != null) context.TriggerRule(maneuver);
            else Rulebook.Trigger(maneuver);
            if (!ExpandedSummoningSpecialProfiles.ShouldMaintainSummonHold(true,
                    maneuver.Success))
            {
                ReleaseLink(owner, target, grab, true);
                return "released";
            }
            int roundsHeld = heldState == null ? 0 : heldState.RoundNumber;
            if (grab != null && ExpandedSummoningSpecialProfiles.ShouldSwallowOnMaintain(
                    grab.SwallowedBuff != null, maneuver.Success, roundsHeld,
                    grab.IsSwallowSizeAllowed(owner, target)))
                return "swallowed:" + grab.SwallowHeld(owner, target, context);
            ItemEntityWeapon weapon = SummonGrappleLinks.EstablishingWeapon(heldState) ??
                (grab == null ? SummonLimbs.PrimaryWeapon(owner) : grab.FirstGrabWeapon(owner));
            int damage = SummonGrappleDamage.DealWeaponDamage(owner, target, weapon, context);
            if (grab != null) grab.DealConstrict(owner, target, context);
            return "maintained:" + damage;
        }

        public void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            if (evt == null || evt.Type != CombatManeuver.Grapple || Owner == null ||
                Owner.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit))
                return;
            UnitEntityData target = HeldTarget(Owner.Unit);
            if (target == null || !ReferenceEquals(evt.Target, target)) return;
            evt.AddBonus(ExpandedSummoningSpecialProfiles.SummonHoldMaintainBonus,
                Fact);
        }

        public void OnEventDidTrigger(RuleCalculateCMB evt) { }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData target = HeldTarget(owner);
            if (target != null) Release(owner, target);
        }

        /// <summary>
        /// The unit this summon's native initiator part names, provided that
        /// unit's own target part points back at the summon.
        /// </summary>
        internal static UnitEntityData HeldTarget(UnitEntityData owner)
        {
            if (owner == null) return null;
            UnitPartGrappleInitiator part = owner.Get<UnitPartGrappleInitiator>();
            UnitEntityData target = part == null ? null : part.Target.Value;
            if (target == null) return null;
            UnitPartGrappleTarget held = target.Get<UnitPartGrappleTarget>();
            return held != null && ReferenceEquals(held.Initiator.Value, owner) ?
                target : null;
        }

        /// <summary>The target's grappled-state buff instance for this holder's link, single or multi.</summary>
        internal static Buff HeldState(UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab)
        {
            if (owner == null || target == null || grab == null || grab.GrappledBuff == null)
                return null;
            if (!grab.MultiLink) return target.Descriptor.Buffs.GetBuff(grab.GrappledBuff);
            return target.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(value =>
                ReferenceEquals(value.Blueprint, grab.GrappledBuff) && value.Context != null &&
                ReferenceEquals(value.Context.MaybeCaster, owner));
        }

        /// <summary>
        /// True when this owner holds exactly this target and held it when the
        /// owner's current round began: the target's grappled state has
        /// ticked at least once. The rake and the swallow both gate on it.
        /// </summary>
        internal static bool IsHeldSinceRoundStart(UnitEntityData owner, UnitEntityData target)
        {
            if (owner == null || target == null) return false;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            if (grab == null) return false;
            bool held = grab.MultiLink
                ? ReferenceEquals(SummonHeldComponent.HolderOf(target, grab.GrappledBuff), owner)
                : ReferenceEquals(HeldTarget(owner), target);
            Buff state = HeldState(owner, target, grab);
            return ExpandedSummoningSpecialProfiles.IsHeldSinceRoundStart(held,
                state == null ? 0 : state.RoundNumber);
        }

        /// <summary>
        /// Ends one link. Single: removes the target's native part (which
        /// removes its grappled buff and condition); the summon's own
        /// initiator part is left to the game's grapple controller, which
        /// drops it once the target is no longer grappled, and to the
        /// summon's own disposal - removing it here would re-enter this
        /// buff's removal. Multi: removes the target's held-state buff for
        /// this holder and, when it was the last link, the holder's own
        /// MultiHold buff.
        /// </summary>
        internal static void ReleaseLink(UnitEntityData owner, UnitEntityData target,
            SummonGrabComponent grab, bool dropHoldWhenLast)
        {
            if (owner == null || target == null) return;
            if (grab == null || !grab.MultiLink)
            {
                Release(owner, target);
                return;
            }
            Buff state = HeldState(owner, target, grab);
            if (state != null) target.Descriptor.Buffs.RemoveFact(state);
            if (dropHoldWhenLast && grab.HoldBuff != null &&
                SummonMultiHoldComponent.HeldTargets(owner, grab.GrappledBuff).Count == 0)
            {
                Buff hold = owner.Descriptor.Buffs.GetBuff(grab.HoldBuff);
                if (hold != null) owner.Descriptor.Buffs.RemoveFact(hold);
            }
        }

        internal static void Release(UnitEntityData owner, UnitEntityData target)
        {
            if (owner == null || target == null) return;
            UnitPartGrappleTarget held = target.Get<UnitPartGrappleTarget>();
            if (held != null && ReferenceEquals(held.Initiator.Value, owner))
                target.Remove<UnitPartGrappleTarget>();
        }
    }

    /// <summary>
    /// The holder's side of the multi-link hold (the Giant Flytrap: one held
    /// target per bite, four at most). One instance of this buff stands on the
    /// holder while it holds anyone; the links themselves are the held-state
    /// buffs on the targets, each naming this holder as its caster, so they
    /// save, load and count on their own. Each new round every link gets its
    /// own maintain check (+5 on the +4): success deals the establishing
    /// bite's damage, or - when the link began the round held and the target
    /// is Medium or smaller - engulfs it instead; failure releases that link
    /// alone; a target out of the bite's reach is released. When the last
    /// link ends the buff removes itself; when the buff turns off for any
    /// reason every link is released.
    /// </summary>
    [Serializable]
    public sealed class SummonMultiHoldComponent : BuffLogic, ITickEachRound,
        IInitiatorRulebookHandler<RuleCalculateCMB>
    {
        public void OnNewRound()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            if (owner == null || grab == null) return;
            MechanicsContext context = Fact == null ? null : Fact.MaybeContext;
            List<UnitEntityData> targets = HeldTargets(owner, grab.GrappledBuff);
            if (targets.Count == 0)
            {
                Buff.Remove();
                return;
            }
            foreach (UnitEntityData target in targets)
            {
                if (!target.Descriptor.State.IsConscious ||
                    !Kingmaker.Controllers.Combat.UnitEngagementExtension.IsReach(owner, target,
                        owner.Body.PrimaryHand))
                {
                    SummonHoldComponent.ReleaseLink(owner, target, grab, false);
                    continue;
                }
                SummonHoldComponent.MaintainLink(owner, target, grab, context, Buff,
                    SummonHoldComponent.HeldState(owner, target, grab));
            }
            if (HeldTargets(owner, grab.GrappledBuff).Count == 0 && Buff != null &&
                Buff.Active)
                Buff.Remove();
        }

        public void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            if (evt == null || evt.Type != CombatManeuver.Grapple || Owner == null ||
                Owner.Unit == null || !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                evt.Target == null) return;
            SummonGrabComponent grab = SummonGrabComponent.Find(Owner.Unit);
            if (grab == null ||
                !ReferenceEquals(SummonHeldComponent.HolderOf(evt.Target, grab.GrappledBuff),
                    Owner.Unit)) return;
            evt.AddBonus(ExpandedSummoningSpecialProfiles.SummonHoldMaintainBonus, Fact);
        }

        public void OnEventDidTrigger(RuleCalculateCMB evt) { }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            if (owner == null || grab == null) return;
            foreach (UnitEntityData target in HeldTargets(owner, grab.GrappledBuff))
                SummonHoldComponent.ReleaseLink(owner, target, grab, false);
        }

        /// <summary>Every loaded unit whose held-state buff names this holder.</summary>
        internal static List<UnitEntityData> HeldTargets(UnitEntityData owner,
            BlueprintBuff heldState)
        {
            var result = new List<UnitEntityData>();
            if (owner == null || heldState == null || Game.Instance == null ||
                Game.Instance.State == null) return result;
            foreach (UnitEntityData unit in Game.Instance.State.Units.All)
            {
                if (unit == null || unit.Descriptor == null || unit.Destroyed) continue;
                if (ReferenceEquals(SummonHeldComponent.HolderOf(unit, heldState), owner))
                    result.Add(unit);
            }
            return result;
        }
    }

    /// <summary>
    /// The held unit's side of a multi-link hold, carried by the held-state
    /// buff whose context names the holder. Each new round the unit attempts
    /// to break free through the game's own check (UnitHelper.TryBreakFree:
    /// the better of Mobility and Athletics, or its CMB, against the
    /// holder's CMD), and the link ends on success, when the holder is gone
    /// or unconscious, or when the holder no longer carries its hold. The
    /// buff's own conditions (Entangled, CantMove) are the grappled state.
    /// </summary>
    [Serializable]
    public sealed class SummonHeldComponent : BuffLogic, ITickEachRound
    {
        public void OnNewRound()
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData holder = Buff == null || Buff.Context == null ? null :
                Buff.Context.MaybeCaster;
            if (owner == null) return;
            SummonGrabComponent grab = SummonGrabComponent.Find(holder);
            if (holder == null || holder.Destroyed || !holder.Descriptor.State.IsConscious ||
                grab == null || grab.HoldBuff == null ||
                holder.Descriptor.Buffs.GetBuff(grab.HoldBuff) == null)
            {
                Buff.Remove();
                return;
            }
            if (UnitHelper.TryBreakFree(owner, holder, UnitHelper.BreakFreeFlags.Default,
                    Buff.Context, null))
                SummonHoldComponent.ReleaseLink(holder, owner, grab, true);
        }

        /// <summary>The holder a unit's held-state buff names, or null.</summary>
        internal static UnitEntityData HolderOf(UnitEntityData unit, BlueprintBuff heldState)
        {
            if (unit == null || unit.Descriptor == null || heldState == null) return null;
            foreach (Buff buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>())
                if (buff != null && ReferenceEquals(buff.Blueprint, heldState) &&
                    buff.Context != null && buff.Context.MaybeCaster != null)
                    return buff.Context.MaybeCaster;
            return null;
        }
    }

    /// <summary>
    /// The swallower's side (Sprint 4), carried by the worm's and the
    /// flytrap's combat-traits buff. The game's own swallow-whole part spits
    /// everyone out when the swallower dies or is destroyed; this component
    /// spits out when the buff turns off for any other reason - the summon
    /// disposed, the traits dispelled - so no unit is ever left inside a
    /// creature that is gone.
    /// </summary>
    [Serializable]
    public sealed class SummonSwallowLifecycleComponent : BuffLogic
    {
        public override void OnTurnOff()
        {
            base.OnTurnOff();
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitPartSwallowWhole part = owner == null ? null :
                owner.Get<UnitPartSwallowWhole>();
            if (part != null) part.SpitOut(true);
        }
    }

    /// <summary>
    /// Area-transition safeguard for the shared summon grapple lifecycle
    /// (Sprint 4). When the party leaves an area, every party member held
    /// (natively or through a multi-link held state), engulfed or swallowed
    /// by a KMG summon is released before the summon is left behind; when an
    /// area finishes loading, any party member whose hold or swallow points
    /// at a unit that no longer exists is released. Subscribed once at load;
    /// inert while the module is off.
    /// </summary>
    internal sealed class SummonGrappleAreaSafeguard : IPartyLeaveAreaHandler,
        IAreaLoadingStagesHandler, IGlobalSubscriber
    {
        private static SummonGrappleAreaSafeguard _instance;

        internal static void Attach()
        {
            if (_instance != null) return;
            _instance = new SummonGrappleAreaSafeguard();
            EventBus.Subscribe(_instance);
        }

        public void HandlePartyLeaveArea(BlueprintArea currentArea,
            BlueprintAreaEnterPoint targetArea)
        { Sweep(true); }

        public void OnAreaScenesLoaded() { }

        public void OnAreaLoadingComplete() { Sweep(false); }

        /// <summary>
        /// Releases party members held or swallowed by a KMG summon (always
        /// when leaving; when loading only if the holder is gone). Returns the
        /// number released, for the runtime fixture.
        /// </summary>
        internal static int Sweep(bool leaving)
        {
            if (Game.Instance == null || Game.Instance.Player == null) return 0;
            return Sweep(leaving, Game.Instance.Player.Party);
        }

        /// <summary>The same sweep over an explicit set of units (the runtime
        /// fixture's held target stands in for a party member).</summary>
        internal static int Sweep(bool leaving, IEnumerable<UnitEntityData> units)
        {
            int released = 0;
            if (units == null) return 0;
            foreach (UnitEntityData unit in units.Where(
                value => value != null && value.Descriptor != null).ToArray())
            {
                UnitPartGrappleTarget held = unit.Get<UnitPartGrappleTarget>();
                if (held != null)
                {
                    UnitEntityData holder = held.Initiator.Value;
                    if (ShouldRelease(leaving, holder))
                    {
                        unit.Remove<UnitPartGrappleTarget>();
                        if (holder != null &&
                            holder.Get<UnitPartGrappleInitiator>() != null &&
                            ReferenceEquals(holder.Get<UnitPartGrappleInitiator>()
                                .Target.Value, unit))
                            holder.Remove<UnitPartGrappleInitiator>();
                        released++;
                    }
                }
                foreach (Buff state in unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Where(value => value != null && value.Blueprint != null &&
                        value.Blueprint.ComponentsArray != null &&
                        value.Blueprint.ComponentsArray.OfType<SummonHeldComponent>().Any())
                    .ToArray())
                {
                    UnitEntityData holder = state.Context == null ? null :
                        state.Context.MaybeCaster;
                    if (!ShouldRelease(leaving, holder)) continue;
                    SummonGrabComponent grab = SummonGrabComponent.Find(holder);
                    if (holder != null && grab != null)
                        SummonHoldComponent.ReleaseLink(holder, unit, grab, true);
                    else unit.Descriptor.Buffs.RemoveFact(state);
                    released++;
                }
                UnitPartSwallowed swallowed = unit.Get<UnitPartSwallowed>();
                if (swallowed != null)
                {
                    UnitEntityData swallower = swallowed.Swallower.Value;
                    if (ShouldRelease(leaving, swallower))
                    {
                        UnitPartSwallowWhole part = swallower == null ? null :
                            swallower.Get<UnitPartSwallowWhole>();
                        if (part != null) part.Free(unit);
                        else unit.Remove<UnitPartSwallowed>();
                        released++;
                    }
                }
            }
            return released;
        }

        private static bool ShouldRelease(bool leaving, UnitEntityData holder)
        {
            if (holder == null || holder.Destroyed) return true;
            return leaving && IsKmgSummon(holder);
        }

        internal static bool IsKmgSummon(UnitEntityData unit)
        {
            return unit != null && unit.Blueprint != null &&
                unit.Blueprint.name.StartsWith("KMG_Summoning_Unit_",
                    StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Sprint 7, rebuilt under the correction order: the cats' rake, gated on
    /// attack identity and target identity. A rake claw (the last two limbs
    /// of the body) strikes on a charge - Pounce makes the charge a full
    /// attack - or against the exact foe this cat holds and held when its
    /// round began (the held state has ticked at least once). Any other
    /// attack roll with a rake claw is an automatic, silent miss: no roll, no
    /// damage, no combat-log line. This is the safety net; the attack
    /// sequence itself drops the rake attacks first (see
    /// ExpandedSummoningRakeSequencePatch), so an ordinary full attack, an
    /// attack of opportunity or a command replay never even spends the swing.
    /// </summary>
    [Serializable]
    public sealed class SummonRakeComponent :
        RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || Owner == null) return;
            UnitEntityData owner = Owner.Unit;
            if (owner == null || owner.Body == null) return;
            bool isRakeWeapon = IsRakeWeapon(owner, evt.Weapon);
            bool isCharge = evt.RuleAttackWithWeapon != null &&
                evt.RuleAttackWithWeapon.IsCharge;
            bool heldTargetSinceRoundStart = SummonHoldComponent.IsHeldSinceRoundStart(
                owner, evt.Target);
            if (ExpandedSummoningSpecialProfiles.ShouldRakeApply(isRakeWeapon,
                    isCharge, heldTargetSinceRoundStart)) return;
            evt.AutoMiss = true;
            evt.SuspendCombatLog = true;
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }

        /// <summary>True when the weapon entity sits in one of the owner's rake slots.</summary>
        public static bool IsRakeWeapon(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            return SummonLimbs.IsRakeLimb(owner, weapon,
                ExpandedSummoningSpecialProfiles.CatRakeSlotCount);
        }
    }

    /// <summary>
    /// The attack-sequencing seam for the rake: when the game builds a full
    /// attack for a cat that is neither charging nor holding its target since
    /// its round began, the rake claws are removed from the planned attacks,
    /// so no hidden extra swing consumes animation time or triggers anything.
    /// A single attack and an attack of opportunity use the primary hand
    /// only and never reach here. Outcomes are kept for the runtime fixture.
    /// </summary>
    [HarmonyPatch(typeof(Kingmaker.UnitLogic.Commands.UnitAttack), "CreateFullAttack")]
    internal static class ExpandedSummoningRakeSequencePatch
    {
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) { return Outcomes.ToArray(); } } }

        internal static void ClearOutcomes() { lock (Sync) { Outcomes.Clear(); } }

        private static void Postfix(Kingmaker.UnitLogic.Commands.UnitAttack __instance,
            List<Kingmaker.UnitLogic.Commands.AttackHandInfo> __result)
        {
            try
            {
                if (__instance == null || __result == null) return;
                UnitEntityData owner = __instance.Executor;
                SummonGrabComponent grab = SummonGrabComponent.Find(owner);
                if (grab == null || grab.RakeLimbCount <= 0) return;
                UnitEntityData target = __instance.TargetUnit;
                bool heldSinceRoundStart = SummonHoldComponent.IsHeldSinceRoundStart(owner, target);
                if (ExpandedSummoningSpecialProfiles.ShouldRakeApply(true, __instance.IsCharge,
                        heldSinceRoundStart))
                {
                    Record(owner, "kept;charge=" + __instance.IsCharge + ";held=" +
                        heldSinceRoundStart + ";attacks=" + __result.Count);
                    return;
                }
                int removed = __result.RemoveAll(info => info != null && info.Hand != null &&
                    SummonLimbs.IsRakeSlot(owner, info.Hand, grab.RakeLimbCount));
                Record(owner, "removed=" + removed + ";attacks=" + __result.Count);
            }
            catch (Exception)
            {
                // The rake gate never interrupts the game's own attack command.
            }
        }

        private static void Record(UnitEntityData owner, string outcome)
        {
            lock (Sync)
            {
                if (Outcomes.Count >= 64) Outcomes.RemoveAt(0);
                Outcomes.Add((owner == null || owner.Blueprint == null ? "?" :
                    owner.Blueprint.name) + "=" + outcome);
            }
        }
    }

    /// <summary>
    /// The Web's target limit (correction order): a web catches a creature
    /// up to one size category larger than the spinner. A blueprint
    /// component the game consults before the ability can target.
    /// </summary>
    [Serializable]
    public sealed class SummonWebTargetSizeChecker : BlueprintComponent,
        Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityTargetChecker
    {
        public int MaxSizeDelta = 1;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target == null ? null : target.Unit;
            if (caster == null || unit == null) return false;
            return ExpandedSummoningSpecialProfiles.IsWebTargetSizeAllowed(
                (int)unit.Descriptor.State.Size, (int)caster.Descriptor.State.Size, MaxSizeDelta);
        }
    }

    /// <summary>
    /// Wind Wall (correction order): while an ally stands inside the Dust
    /// Mephit's wall of wind, arrows and bolts shot at it are deflected and
    /// miss outright; any other normal ranged weapon has a 30% miss chance
    /// through the game's own miss-chance roll. Rays, touches and kinetic
    /// blasts pass, as spells pass the tabletop wall. Melee is untouched.
    /// The game hands every attack roll against the owner to this handler
    /// before the roll. Outcomes are kept for the runtime fixture.
    /// </summary>
    [Serializable]
    public sealed class SummonWindWallComponent : BuffLogic,
        ITargetRulebookHandler<RuleAttackRoll>
    {
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) { return Outcomes.ToArray(); } } }

        internal static void ClearOutcomes() { lock (Sync) { Outcomes.Clear(); } }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null ||
                !ReferenceEquals(evt.Target, Owner.Unit)) return;
            WeaponCategory? category = evt.Weapon == null || evt.Weapon.Blueprint == null ?
                (WeaponCategory?)null : evt.Weapon.Blueprint.Category;
            SummonWindWallOutcome outcome = ExpandedSummoningSpecialProfiles.WindWallOutcome(
                evt.AttackType.IsRanged(),
                category.HasValue && IsArrowOrBolt(category.Value),
                category.HasValue && IsSpellDelivery(category.Value));
            switch (outcome)
            {
                case SummonWindWallOutcome.Deflected:
                    evt.AutoMiss = true;
                    break;
                case SummonWindWallOutcome.MissChance:
                    evt.IncreaseMissChance(
                        ExpandedSummoningSpecialProfiles.WindWallOtherRangedMissChance);
                    break;
                default:
                    return;
            }
            lock (Sync)
            {
                Outcomes.Add(Owner.Unit.UniqueId + ":" + outcome + ":" +
                    (category.HasValue ? category.Value.ToString() : "none") +
                    ":missChance=" + evt.MissChance);
            }
        }

        public void OnEventDidTrigger(RuleAttackRoll evt) { }

        /// <summary>Bows and crossbows shoot the arrows and bolts the wall deflects outright.</summary>
        internal static bool IsArrowOrBolt(WeaponCategory category)
        {
            switch (category)
            {
                case WeaponCategory.Longbow:
                case WeaponCategory.Shortbow:
                case WeaponCategory.LightCrossbow:
                case WeaponCategory.HeavyCrossbow:
                case WeaponCategory.HandCrossbow:
                case WeaponCategory.LightRepeatingCrossbow:
                case WeaponCategory.HeavyRepeatingCrossbow:
                    return true;
            }
            return false;
        }

        /// <summary>Rays, touches and kinetic blasts are spell deliveries, not weapons the wall stops.</summary>
        internal static bool IsSpellDelivery(WeaponCategory category)
        {
            return category == WeaponCategory.Touch || category == WeaponCategory.Ray ||
                category == WeaponCategory.KineticBlast;
        }
    }

    /// <summary>
    /// Chill Metal (correction order): what the target carries decides the
    /// tier of cold - the table's full dice for a creature in metal armor,
    /// its minimal 1 or 2 points for one carrying only a metal weapon, and
    /// nothing (no valid target) for one carrying no metal at all. Armor is
    /// metal unless its type is padded, leather or hide; a manufactured
    /// weapon is metal unless it is one of the wooden kinds (clubs, staves,
    /// slings, bows, nunchaku). Shields alone are not counted: they never
    /// reach the tabletop's one-fifth-of-body-weight threshold.
    /// </summary>
    internal static class SummonChillMetal
    {
        private static readonly string[] NonMetalArmorTokens = { "Padded", "Leather", "Hide" };
        private static readonly WeaponCategory[] WoodenCategories = {
            WeaponCategory.Club, WeaponCategory.Greatclub, WeaponCategory.Quarterstaff,
            WeaponCategory.Sling, WeaponCategory.SlingStaff, WeaponCategory.Shortbow,
            WeaponCategory.Longbow, WeaponCategory.Nunchaku
        };
        private static readonly WeaponCategory[] NotManufactured = {
            WeaponCategory.UnarmedStrike, WeaponCategory.Touch, WeaponCategory.Ray,
            WeaponCategory.Bomb, WeaponCategory.KineticBlast, WeaponCategory.Bite,
            WeaponCategory.Claw, WeaponCategory.Gore, WeaponCategory.OtherNaturalWeapons
        };

        internal static string ArmorTypeName(UnitEntityData unit)
        {
            if (unit == null || unit.Body == null || unit.Body.Armor == null) return null;
            ItemEntityArmor armor = unit.Body.Armor.MaybeArmor;
            if (armor == null || armor.Blueprint == null || armor.Blueprint.Type == null)
                return null;
            return armor.Blueprint.Type.name;
        }

        internal static bool IsMetalArmorType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return false;
            foreach (string token in NonMetalArmorTokens)
                if (typeName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;
            return true;
        }

        internal static bool WearsMetalArmor(UnitEntityData unit)
        { return IsMetalArmorType(ArmorTypeName(unit)); }

        internal static bool IsMetalWeapon(BlueprintItemWeapon weapon)
        {
            if (weapon == null || weapon.IsNatural) return false;
            WeaponCategory category = weapon.Category;
            return Array.IndexOf(NotManufactured, category) < 0 &&
                Array.IndexOf(WoodenCategories, category) < 0;
        }

        internal static bool WieldsMetalWeapon(UnitEntityData unit)
        {
            if (unit == null || unit.Body == null) return false;
            foreach (Kingmaker.Items.Slots.HandSlot hand in new[] {
                unit.Body.PrimaryHand, unit.Body.SecondaryHand })
                if (hand != null && hand.MaybeWeapon != null &&
                    IsMetalWeapon(hand.MaybeWeapon.Blueprint))
                    return true;
            return false;
        }

        internal static int Tier(UnitEntityData unit)
        {
            return ExpandedSummoningSpecialProfiles.ChillMetalTier(WearsMetalArmor(unit),
                WieldsMetalWeapon(unit));
        }

        internal static string Describe(UnitEntityData unit)
        {
            return "armor=" + (ArmorTypeName(unit) ?? "none") + ";metalArmor=" +
                WearsMetalArmor(unit) + ";metalWeapon=" + WieldsMetalWeapon(unit) +
                ";tier=" + Tier(unit);
        }
    }

    /// <summary>Chill Metal targets only a creature wearing or carrying metal.</summary>
    [Serializable]
    public sealed class SummonChillMetalTargetChecker : BlueprintComponent,
        Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target == null ? null : target.Unit;
            return unit != null && SummonChillMetal.Tier(unit) > 0;
        }
    }

    /// <summary>
    /// The chilled state: at the start of each of its rounds the target takes
    /// the table's cold for that round of the spell (the state's first tick
    /// is the spell's second round), in full for metal armor, minimal for a
    /// metal weapon only, and none once it carries no metal. Cold is dealt
    /// through the game's damage rule by the mephit that cast it, so cold
    /// immunity and resistance apply. Outcomes are kept for the fixture.
    /// </summary>
    [Serializable]
    public sealed class SummonChillMetalComponent : BuffLogic, ITickEachRound
    {
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) { return Outcomes.ToArray(); } } }

        internal static void ClearOutcomes() { lock (Sync) { Outcomes.Clear(); } }

        public void OnNewRound()
        {
            if (Owner == null || Owner.Unit == null || Buff == null) return;
            UnitEntityData owner = Owner.Unit;
            int round = Buff.RoundNumber + 1;
            int tier = SummonChillMetal.Tier(owner);
            int dice = ExpandedSummoningSpecialProfiles.ChillMetalDice(round);
            int minimal = ExpandedSummoningSpecialProfiles.ChillMetalMinimalDamage(round);
            int dealt = 0;
            if (tier > 0 && dice > 0)
            {
                var description = new DamageDescription {
                    TypeDescription = new DamageTypeDescription {
                        Type = DamageType.Energy, Energy = DamageEnergyType.Cold },
                    Dice = tier == 2 ? new DiceFormula(dice, DiceType.D4) :
                        new DiceFormula(0, DiceType.Zero),
                    Bonus = tier == 2 ? 0 : minimal
                };
                UnitEntityData caster = Buff.Context != null && Buff.Context.MaybeCaster != null ?
                    Buff.Context.MaybeCaster : owner;
                var rule = new RuleDealDamage(caster, owner,
                    new DamageBundle(new BaseDamage[] { description.CreateDamage() }));
                Rulebook.Trigger(rule);
                dealt = rule.Damage;
            }
            lock (Sync)
            {
                Outcomes.Add(owner.UniqueId + ":round=" + round + ";tier=" + tier + ";dice=" +
                    dice + "d4;minimal=" + minimal + ";damage=" + dealt);
            }
        }
    }

    /// <summary>
    /// Docile hooves (correction order): the Pony's and the Horse's hooves
    /// are secondary natural attacks - the tabletop Docile quality, which a
    /// summon never trains away. The game computes a secondary natural
    /// attack (-5 to hit, half the Strength modifier to damage) for any
    /// weapon entity whose ForceSecondary flag is set, so this carrier sets
    /// it on the primary hand's hoof and on every additional limb's hoof
    /// when it turns on - at spawn and again after a load - and clears it
    /// when it turns off. No other stat, limb or attack is touched.
    /// </summary>
    [Serializable]
    public sealed class SummonDocileHoovesComponent : BuffLogic,
        IInitiatorRulebookHandler<RuleCalculateAttackBonus>,
        IInitiatorRulebookHandler<RuleCalculateWeaponStats>
    {
        /// <summary>Fixture-only: lets the fixture compute the same hooves as primary for the comparison.</summary>
        internal static bool SuspendedForFixture { get; set; }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            // The unit's facts activate before its body's limbs exist; the
            // body patch below and the rule-time hooks catch that case.
            Apply(Owner == null ? null : Owner.Unit, true);
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            Apply(Owner == null ? null : Owner.Unit, false);
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonus evt)
        { Ensure(evt == null ? null : evt.Initiator, evt == null ? null : evt.Weapon); }

        public void OnEventDidTrigger(RuleCalculateAttackBonus evt) { }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        { Ensure(evt == null ? null : evt.Initiator, evt == null ? null : evt.Weapon); }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        private void Ensure(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            if (SuspendedForFixture || owner == null || weapon == null || Owner == null ||
                !ReferenceEquals(owner, Owner.Unit)) return;
            if (Hooves(owner).Contains(weapon)) weapon.ForceSecondary = true;
        }

        /// <summary>Sets or clears the docile flag on every hoof; returns how many hooves were touched.</summary>
        internal static int Apply(UnitEntityData owner, bool secondary)
        {
            int count = 0;
            foreach (ItemEntityWeapon hoof in Hooves(owner))
            {
                hoof.ForceSecondary = secondary;
                count++;
            }
            return count;
        }

        /// <summary>True when the unit carries a docile-hoof carrier buff.</summary>
        internal static bool Carries(UnitDescriptor descriptor)
        {
            if (descriptor == null || descriptor.Buffs == null) return false;
            foreach (Buff buff in descriptor.Buffs.RawFacts.OfType<Buff>())
                if (buff != null && buff.Blueprint != null && buff.Blueprint.ComponentsArray != null &&
                    buff.Blueprint.ComponentsArray.OfType<SummonDocileHoovesComponent>().Any())
                    return true;
            return false;
        }

        /// <summary>Every natural weapon entity on the body: the primary hand's and the additional limbs'.</summary>
        internal static List<ItemEntityWeapon> Hooves(UnitEntityData owner)
        {
            var result = new List<ItemEntityWeapon>();
            if (owner == null || owner.Body == null) return result;
            ItemEntityWeapon primary = SummonLimbs.PrimaryWeapon(owner);
            if (primary != null && primary.Blueprint != null && primary.Blueprint.IsNatural)
                result.Add(primary);
            if (owner.Body.AdditionalLimbs != null)
                foreach (Kingmaker.Items.Slots.WeaponSlot limb in owner.Body.AdditionalLimbs)
                    if (limb != null && limb.MaybeWeapon != null &&
                        limb.MaybeWeapon.Blueprint != null && limb.MaybeWeapon.Blueprint.IsNatural)
                        result.Add(limb.MaybeWeapon);
            return result;
        }
    }

    /// <summary>
    /// The unit's facts activate before its body's limbs are created, so
    /// the docile carrier's own activation finds no hooves; once the body
    /// initializes, the flag is set on every hoof of a unit that carries
    /// the docile-hoof carrier (spawn and load alike).
    /// </summary>
    [HarmonyPatch(typeof(UnitBody), "Initialize")]
    internal static class ExpandedSummoningDocileHoovesBodyPatch
    {
        private static void Postfix(UnitBody __instance)
        {
            try
            {
                UnitDescriptor owner = __instance == null ? null : __instance.Owner;
                if (owner == null || owner.Unit == null ||
                    !SummonDocileHoovesComponent.Carries(owner)) return;
                SummonDocileHoovesComponent.Apply(owner.Unit, true);
            }
            catch (Exception)
            {
                // The docile flag never interrupts the game's own body initialization.
            }
        }
    }

    public sealed class PixieSleepArrowComponent :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintItemWeapon SleepBow;
        public BlueprintAbilityResource SleepArrowResource;
        public BlueprintBuff SleepingBuff;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || Owner == null ||
                Owner.Unit == null || evt.Target == null || evt.Weapon == null ||
                SleepBow == null || SleepArrowResource == null ||
                SleepingBuff == null) return;
            int remaining = Owner.Resources.GetResourceAmount(SleepArrowResource);
            if (!ExpandedSummoningSpecialProfiles.ShouldSpendPixieSleepArrow(
                    ReferenceEquals(evt.Weapon.Blueprint, SleepBow),
                    evt.AttackRoll.IsHit, remaining)) return;
            bool spent = false;
            Buff applied = null;
            try
            {
                Owner.Resources.Spend(SleepArrowResource, 1);
                spent = true;
                var saving = new RuleSavingThrow(evt.Target,
                    SavingThrowType.Will,
                    ExpandedSummoningSpecialProfiles.PixieSleepArrowWillDc);
                Rulebook.Trigger(saving);
                if (saving.IsPassed) return;
                applied = evt.Target.Descriptor.Buffs.AddBuff(SleepingBuff,
                    Fact == null ? null : Fact.MaybeContext,
                    TimeSpan.FromSeconds(6d * ExpandedSummoningSpecialProfiles
                        .PixieSleepArrowRounds));
                if (applied == null)
                    throw new InvalidOperationException(
                        "The native Sleeping buff rejected a Pixie sleep arrow.");
            }
            catch
            {
                if (applied != null)
                    evt.Target.Descriptor.Buffs.RemoveFact(applied);
                if (spent) Owner.Resources.Restore(SleepArrowResource, 1);
            }
        }
    }
}
